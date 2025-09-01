#region Using declarations
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using NinjaTrader.Cbi;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.Strategies;
#endregion

// TradeTemplateEMA v1 (Local > GitHub + Validation + LKG + Version feedback + Configurable bounds/poll + SHA-256 hash)
// - Arms when EMA(10) crosses EMA(50)
// - Triggers when EMA(10) crosses EMA(200) in the same direction
// - TP/SL in ticks, fixed quantity
// - Optional filters: FVG, Imbalance, Range
// - Polls public config (ConfigUrl) + local config (LocalConfigPath); merge = GitHub base, then Local overrides (LOCAL WINS)
// - account_mode guard: demo requires Sim; live blocks Sim
// - Validation of ranges; invalid -> try Last-Known-Good; otherwise keep previous
// - Prints: version + updated_at + account_mode on successful apply
// - Configurable validation bounds and poll interval via config
// - Skips re-apply when config unchanged via SHA-256 hash

namespace NinjaTrader.NinjaScript.Strategies
{
    public class TradeTemplateEMA : Strategy
    {
        // ---- Exposed params ----
        [NinjaScriptProperty] public int EmaFast     { get; set; } = 10;
        [NinjaScriptProperty] public int EmaMid      { get; set; } = 50;
        [NinjaScriptProperty] public int EmaSlow     { get; set; } = 200;

        [NinjaScriptProperty] public int TpTicks     { get; set; } = 20;
        [NinjaScriptProperty] public int SlTicks     { get; set; } = 16;
        [NinjaScriptProperty] public int Quantity    { get; set; } = 1;

        [NinjaScriptProperty] public bool UseFVG         { get; set; } = false;
        [NinjaScriptProperty] public bool UseImbalance   { get; set; } = false;
        [NinjaScriptProperty] public bool UseRangeFilter { get; set; } = false;
        [NinjaScriptProperty] public int  RangeLookback  { get; set; } = 20;
        [NinjaScriptProperty] public int  RangeMaxTicks  { get; set; } = 24;

        [NinjaScriptProperty] public string ConfigUrl       { get; set; } = "https://raw.githubusercontent.com/USER/trade-template-mvp/main/config/config.txt";
        [NinjaScriptProperty] public string LocalConfigPath { get; set; } = ""; // e.g. C:\\Users\\You\\Documents\\NinjaTrader 8\\templates\\Strategy\\trade_template_local.config

        [NinjaScriptProperty] public string LkgPath { get; set; } = ""; // optional override for LKG location
        [NinjaScriptProperty] public bool EnforceAccountMode { get; set; } = true;

        // ---- Internals ----
        private EMA emaFast, emaMid, emaSlow;
        private int pendingDir = 0; // +1 long-armed, -1 short-armed
        private DateTime lastConfigLoad = DateTime.MinValue;
        private int pollSeconds = 5;

        private string accountMode = "demo"; // "demo" or "live"
        private string cfgVersion = "";
        private string cfgUpdatedAt = "";

        private Dictionary<string,string> lastGoodKv = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        private string lastGoodPath;
        private string lastAppliedHash = "";

        // Validation bounds (min fixed, max configurable via config)
        private const int EMA_MIN = 1;
        private const int TICKS_MIN = 1;
        private const int QTY_MIN = 1;
        private const int RANGE_LK_MIN = 2;
        private const int RANGE_TK_MIN = 1;

        private int EMA_MAX_CFG = 400;
        private int TICKS_MAX_CFG = 200;
        private int QTY_MAX_CFG = 50;
        private int RANGE_LK_MAX_CFG = 500;
        private int RANGE_TK_MAX_CFG = 200;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name                 = "TradeTemplateEMA";
                Calculate            = Calculate.OnBarClose; // cleaner for MVP
                IsUnmanaged          = false;
                EntriesPerDirection  = 1;
                EntryHandling        = EntryHandling.AllEntries;
            }
            else if (State == State.DataLoaded)
            {
                emaFast = EMA(EmaFast);
                emaMid  = EMA(EmaMid);
                emaSlow = EMA(EmaSlow);

                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string dir  = Path.Combine(docs, "NinjaTrader 8", "templates", "Strategy");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                lastGoodPath = !string.IsNullOrWhiteSpace(LkgPath)
                               ? LkgPath
                               : Path.Combine(dir, "trade_template_last_good.config");
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < Math.Max(Math.Max(EmaFast, EmaMid), EmaSlow) + 5)
                return;

            if ((DateTime.UtcNow - lastConfigLoad).TotalSeconds >= pollSeconds)
            {
                LoadAndMergeConfig();
                lastConfigLoad = DateTime.UtcNow;
            }

            if (EnforceAccountMode && !AccountModeMatches())
                return;

            bool crossUp50   = CrossAbove(emaFast, emaMid, 1);
            bool crossDown50 = CrossBelow(emaFast, emaMid, 1);
            if (crossUp50)   pendingDir = +1;
            if (crossDown50) pendingDir = -1;

            bool crossUp200   = CrossAbove(emaFast, emaSlow, 1);
            bool crossDown200 = CrossBelow(emaFast, emaSlow, 1);

            if (Position.MarketPosition == MarketPosition.Flat && pendingDir != 0)
            {
                if (pendingDir == +1 && crossUp200 && FiltersOK(+1))
                    EnterWithStops(true);
                else if (pendingDir == -1 && crossDown200 && FiltersOK(-1))
                    EnterWithStops(false);

                if (crossUp200 || crossDown200)
                    pendingDir = 0;
            }
        }

        private void EnterWithStops(bool isLong)
        {
            SetStopLoss(CalculationMode.Ticks, SlTicks);
            SetProfitTarget(CalculationMode.Ticks, TpTicks);
            int q = Math.Max(1, Quantity);
            if (isLong)  EnterLong(q, "EMA_Long");
            else         EnterShort(q, "EMA_Short");
        }

        private bool FiltersOK(int dir)
        {
            if (UseRangeFilter && RangeLookback > 1)
            {
                double hh = MAX(High, RangeLookback)[0];
                double ll = MIN(Low,  RangeLookback)[0];
                if ((hh - ll) > (RangeMaxTicks * TickSize)) return false;
            }

            if (UseImbalance)
            {
                double body  = Math.Abs(Close[0] - Open[0]);
                double range = High[0] - Low[0];
                if (range <= 0) return false;
                double bodyRatio = body / range;
                if (bodyRatio < 0.6) return false;
                if (dir == +1 && Close[0] < Open[0]) return false;
                if (dir == -1 && Close[0] > Open[0]) return false;
            }

            if (UseFVG && CurrentBar >= 2)
            {
                bool bullFVG = Low[1] > High[2];
                bool bearFVG = High[1] < Low[2];
                if (dir == +1 && !bullFVG) return false;
                if (dir == -1 && !bearFVG) return false;
            }

            return true;
        }

        private bool AccountModeMatches()
        {
            string acctName = "";
            try { acctName = Account?.Name ?? ""; } catch { acctName = ""; }

            bool isSim = !string.IsNullOrEmpty(acctName) && acctName.IndexOf("Sim", StringComparison.OrdinalIgnoreCase) >= 0;

            if (accountMode.Equals("demo", StringComparison.OrdinalIgnoreCase))
            {
                if (!isSim)
                {
                    Print("[Guard] account_mode=demo but account is not Sim. Blocking entries.");
                    return false;
                }
            }
            else if (accountMode.Equals("live", StringComparison.OrdinalIgnoreCase))
            {
                if (isSim)
                {
                    Print("[Guard] account_mode=live but account is Sim. Blocking entries.");
                    return false;
                }
            }
            return true;
        }

        // ----------------------------
        // Config loading / merging / validation / LKG
        // ----------------------------
        private void LoadAndMergeConfig()
        {
            var merged = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            var gh = ReadConfigFromUrl(ConfigUrl);
            foreach (var kv in gh) merged[kv.Key] = kv.Value;

            var loc = ReadConfigFromFile(LocalConfigPath);
            foreach (var kv in loc) merged[kv.Key] = kv.Value;

            var candidateHash = HashKv(merged);
            if (candidateHash == lastAppliedHash)
                return;

            if (!ValidateKv(merged))
            {
                Print("[Config] Validation failed. Attempting last-known-good fallback...");
                var lkg = ReadConfigFromFile(lastGoodPath);
                if (lkg.Count > 0 && ValidateKv(lkg))
                {
                    ApplyKv(lkg);
                    lastAppliedHash = HashKv(lkg);
                    lastGoodKv = lkg;
                    Print("[Config] Recovered using last-known-good.");
                    return;
                }
                Print("[Config] No valid fallback found. Keeping previous in-memory settings.");
                return;
            }

            ApplyKv(merged);
            lastAppliedHash = candidateHash;

            try
            {
                File.WriteAllText(lastGoodPath, KvToText(merged));
                lastGoodKv = merged;
            }
            catch { /* ignore */ }

            cfgVersion   = merged.ContainsKey("config_version") ? merged["config_version"] : "";
            cfgUpdatedAt = merged.ContainsKey("updated_at")     ? merged["updated_at"]     : "";
            var amode    = GetSafe(merged, "account_mode", accountMode);
            Print($"[Config] Applied OK. version={cfgVersion}, updated_at={cfgUpdatedAt}, account_mode={amode}");
        }

        private Dictionary<string,string> ReadConfigFromUrl(string url)
        {
            var d = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(url)) return d;

            try
            {
                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    string text = wc.DownloadString(url);
                    return ParseKv(text);
                }
            }
            catch { return d; }
        }

        private Dictionary<string,string> ReadConfigFromFile(string path)
        {
            var d = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    string text = File.ReadAllText(path);
                    return ParseKv(text);
                }
            }
            catch { }
            return d;
        }

        private Dictionary<string,string> ParseKv(string content)
        {
            var d = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(content)) return d;

            using (var sr = new StringReader(content))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("#")) continue;
                    int eq = trimmed.IndexOf('=');
                    if (eq <= 0) continue;
                    string k = trimmed.Substring(0, eq).Trim();
                    string v = trimmed.Substring(eq + 1).Trim();
                    d[k] = v;
                }
            }
            return d;
        }

        private void ApplyKv(Dictionary<string,string> kv)
        {
            if (kv == null) return;

            EmaFast       = GetInt(kv, "ema_fast", EmaFast);
            EmaMid        = GetInt(kv, "ema_mid", EmaMid);
            EmaSlow       = GetInt(kv, "ema_slow", EmaSlow);

            TpTicks       = GetInt(kv, "tp_ticks", TpTicks);
            SlTicks       = GetInt(kv, "sl_ticks", SlTicks);
            Quantity      = GetInt(kv, "quantity", Quantity);

            UseFVG        = GetBool(kv, "use_fvg", UseFVG);
            UseImbalance  = GetBool(kv, "use_imbalance", UseImbalance);
            UseRangeFilter= GetBool(kv, "use_range_filter", UseRangeFilter);
            RangeLookback = GetInt(kv, "range_lookback", RangeLookback);
            RangeMaxTicks = GetInt(kv, "range_max_ticks", RangeMaxTicks);

            string am;
            if (kv.TryGetValue("account_mode", out am))
                accountMode = am;

            EMA_MAX_CFG        = ParseInt(kv, "max_ema_length",     EMA_MAX_CFG);
            TICKS_MAX_CFG      = ParseInt(kv, "max_ticks",          TICKS_MAX_CFG);
            QTY_MAX_CFG        = ParseInt(kv, "max_quantity",       QTY_MAX_CFG);
            RANGE_LK_MAX_CFG   = ParseInt(kv, "max_range_lookback", RANGE_LK_MAX_CFG);
            RANGE_TK_MAX_CFG   = ParseInt(kv, "max_range_ticks",    RANGE_TK_MAX_CFG);

            pollSeconds = Math.Max(1, ParseInt(kv, "poll_seconds", pollSeconds));

            var lkgOverride = GetSafe(kv, "lkg_path", "");
            if (!string.IsNullOrWhiteSpace(lkgOverride))
                lastGoodPath = lkgOverride;
        }

        private bool ValidateKv(Dictionary<string,string> kv)
        {
            int ef = ParseInt(kv, "ema_fast", EmaFast);
            int em = ParseInt(kv, "ema_mid",  EmaMid);
            int es = ParseInt(kv, "ema_slow", EmaSlow);

            int tp = ParseInt(kv, "tp_ticks", TpTicks);
            int sl = ParseInt(kv, "sl_ticks", SlTicks);
            int q  = ParseInt(kv, "quantity", Quantity);

            int rl = ParseInt(kv, "range_lookback", RangeLookback);
            int rt = ParseInt(kv, "range_max_ticks", RangeMaxTicks);

            bool ok = true;
            if (ef < EMA_MIN || ef > EMA_MAX_CFG) ok = false;
            if (em < EMA_MIN || em > EMA_MAX_CFG) ok = false;
            if (es < EMA_MIN || es > EMA_MAX_CFG) ok = false;

            if (tp < TICKS_MIN || tp > TICKS_MAX_CFG) ok = false;
            if (sl < TICKS_MIN || sl > TICKS_MAX_CFG) ok = false;
            if (q  < QTY_MIN   || q  > QTY_MAX_CFG)   ok = false;

            if (rl < RANGE_LK_MIN || rl > RANGE_LK_MAX_CFG) ok = false;
            if (rt < RANGE_TK_MIN || rt > RANGE_TK_MAX_CFG) ok = false;

            if (!ok)
            {
                Print("[Config] Out-of-range parameter detected.");
                return false;
            }
            if (tp == 0 || sl == 0)
            {
                Print("[Config] tp/sl cannot be zero.");
                return false;
            }
            return true;
        }

        private string KvToText(Dictionary<string,string> kv)
        {
            var sb = new StringBuilder();
            foreach (var p in kv)
                sb.AppendLine($"{p.Key}={p.Value}");
            return sb.ToString();
        }

        private string HashKv(Dictionary<string,string> kv)
        {
            if (kv == null || kv.Count == 0) return "";
            var sb = new StringBuilder();
            foreach (var kvp in kv.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                sb.Append(kvp.Key).Append('=').Append(kvp.Value).Append('\n');
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var hash  = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", ""); // hex
            }
        }

        private string GetSafe(Dictionary<string,string> d, string k, string defv)
        {
            return (d != null && d.ContainsKey(k)) ? d[k] : defv;
        }

        private int ParseInt(Dictionary<string,string> d, string k, int defv)
        {
            if (d != null && d.ContainsKey(k) && int.TryParse(d[k], out var v)) return v;
            return defv;
        }

        private int  GetInt (Dictionary<string,string> d, string k, int defv)  => d.ContainsKey(k) && int.TryParse(d[k], out var v) ? v : defv;
        private bool GetBool(Dictionary<string,string> d, string k, bool defv) => d.ContainsKey(k) && bool.TryParse(d[k], out var v) ? v : defv;
    }
}
