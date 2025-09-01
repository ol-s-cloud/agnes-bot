# AGNES-BOT — Trade Template MVP (User Guide)

A minimal, safe NinjaTrader 8 strategy that reads a **public GitHub config** plus a **private local override** on your computer. Local values override GitHub values.

---

## 0) Quick Links

**GitHub config (Raw URL for NinjaTrader → ConfigUrl):**
```
https://raw.githubusercontent.com/ol-s-cloud/agnes-bot/main/config/config.txt
```

**Local override file paths (choose based on your system):**

*Windows (typical NinjaTrader install):*
```
C:\Users\YourUsername\Documents\NinjaTrader 8\templates\Strategy\trade_template_local.config
```

*Mac (if running NinjaTrader via Parallels/VM):*
```
/Users/gs/Documents/NinjaTrader8/templates/strategy/trade_template_local.config
```

**Optional LKG ("last-known-good") file paths:**

*Windows:*
```
C:\Users\YourUsername\Documents\NinjaTrader 8\templates\Strategy\trade_template_last_good.config
```

*Mac:*
```
/Users/gs/Documents/NinjaTrader8/templates/strategy/trade_template_last_good.config
```

> *LKG lets the strategy recover if a bad config is pushed.*

---

## 1) What's in This Repo

- `config/config.txt` — **public** parameters (EMA lengths, TP/SL, filters, etc.). **No secrets**.
- `nt/TradeTemplateEMA.cs` — NinjaTrader strategy source code.
- `web/index.html` — web UI you can deploy on Vercel to generate config text.
- `README.md`, `DOCS.md`, `USER-GUIDE.md` — documentation.

---

## 2) Create Your Private Local Override (One-Time Setup)

**Step 1: Create the directory structure**

*Windows:*
```
C:\Users\YourUsername\Documents\NinjaTrader 8\templates\Strategy\
```

*Mac (adjust for your username):*
```
/Users/gs/Documents/NinjaTrader8/templates/strategy/
```

**Step 2: Create the local config file**

Create a file called `trade_template_local.config` in the above directory.

**Suggested starter content:**
```
# LOCAL OVERRIDES (private) — these override GitHub values with the same keys
account_mode=demo        # demo requires Sim; live blocks Sim
quantity=2
sl_ticks=18

# Optional: define custom LKG path here instead of in GitHub config
# lkg_path=C:\Users\YourUsername\Documents\NinjaTrader 8\templates\Strategy\trade_template_last_good.config

# Optional: keep any secrets ONLY here (never in GitHub)
# api_key=YOUR_REAL_KEY
```

> **Important**: Local overrides always win on conflicts. Keep secrets local only.

---

## 3) Load the Strategy in NinjaTrader 8

**Step 1: Import Strategy Code**
1. Open **NinjaTrader 8** → **New** → **NinjaScript Editor**
2. Create a new Strategy or open existing file
3. Copy and paste the contents of `nt/TradeTemplateEMA.cs`
4. Save (it compiles automatically)

**Step 2: Configure Strategy on Chart**
1. Open an **ES chart** (e.g., 5-minute timeframe)
2. Right-click chart → **Strategies...** → select **TradeTemplateEMA**
3. Set these properties:

   **ConfigUrl:**
   ```
   https://raw.githubusercontent.com/ol-s-cloud/agnes-bot/main/config/config.txt
   ```

   **LocalConfigPath (Windows):**
   ```
   C:\Users\YourUsername\Documents\NinjaTrader 8\templates\Strategy\trade_template_local.config
   ```

   **LocalConfigPath (Mac/VM):**
   ```
   /Users/gs/Documents/NinjaTrader8/templates/strategy/trade_template_local.config
   ```

   **LkgPath (optional):**
   Use corresponding path from section 0 above.

   **EnforceAccountMode:** `True`

4. Set **Enabled = True**
5. Ensure your **chart account is Sim** (e.g., **Sim101**) while `account_mode=demo`

> **Cross-platform note**: If running NinjaTrader in Windows VM on Mac, use Windows-style paths that the VM can access, such as network mapped drives or shared folders.

---

## 4) Verify It's Working (Test Run)

**Step 1: Check NinjaTrader Output**
1. Open **Control Center** → **New** → **NinjaScript Output**
2. You should see:
   ```
   [Config] Applied OK. version=v1-github, updated_at=2025-09-01T00:00:00Z, account_mode=demo
   ```

**Step 2: Test GitHub Configuration Read**
1. Edit `config/config.txt` in GitHub
2. Change `config_version=github-test` and commit
3. Within 5-10 seconds, Output should show `version=github-test`

**Step 3: Test Local Override Wins**
1. Add `config_version=local-test` to your local config file and save
2. Output should switch to `version=local-test` (proving local override works)

---

## 5) Day-to-Day Usage

**Configuration Updates:**
- Adjust parameters in **GitHub** (`config/config.txt`) for public, non-secret settings
- Keep personal or sensitive overrides in your **local** file (they always win)
- Strategy polls every **5 seconds** by default (`poll_seconds`)

**Key Configuration Parameters:**

| Parameter | Default | Description |
|-----------|---------|-------------|
| `ema_fast` | 10 | Fast EMA period |
| `ema_mid` | 50 | Medium EMA (arm signal) |
| `ema_slow` | 200 | Slow EMA (trigger signal) |
| `tp_ticks` | 20 | Take profit in ticks |
| `sl_ticks` | 16 | Stop loss in ticks |
| `quantity` | 1 | Position size |
| `account_mode` | demo | demo/live enforcement |
| `use_fvg` | false | Fair Value Gap filter |
| `use_imbalance` | false | Strong body bar filter |
| `use_range_filter` | false | Range-bound market filter |
| `poll_seconds` | 5 | Config reload frequency |

**Time Format:**
- Use ISO-8601 UTC format: `2025-01-09T15:30:00Z`
- The 'Z' suffix indicates UTC timezone

---

## 6) Safety & Troubleshooting

**Account Mode Guardrails:**
- `[Guard] account_mode=demo but account is not Sim` → switch chart to **Sim101** or set `account_mode=live`
- `[Guard] account_mode=live but account is Sim` → switch to **non-Sim** account or set `account_mode=demo`

**Configuration Validation:**
- Out-of-range values are rejected; strategy tries **LKG** file as fallback
- If no valid fallback found, keeps previous in-memory settings
- Check NinjaScript Output for validation error messages

**Common Issues:**

*Config not loading:*
- Verify Raw URL opens in browser and shows plain text
- Check network connectivity from NinjaTrader PC
- Ensure config file syntax is correct (key=value format)

*Local file not found:*
- Verify exact file path and spelling
- Check file permissions
- Ensure directory structure exists

*Path issues on Mac:*
- If using VM, ensure paths are accessible from Windows environment
- Use mapped network drives or shared folders as needed
- Test paths by opening them in NinjaTrader file dialogs

---

## 7) Going Live (When Ready)

**Safety Checklist:**
1. Thoroughly test on simulation with realistic position sizes
2. Understand the strategy logic and market conditions
3. Set appropriate risk management parameters

**Go-Live Steps:**
1. In your **local** config file, set `account_mode=live`
2. In NinjaTrader, select a **non-Sim** account for the chart
3. Start with minimal position size
4. Monitor closely during initial live trading

**Risk Management Reminders:**
- This is basic strategy with fixed TP/SL
- Consider adding daily loss limits in future versions
- Never risk more than you can afford to lose
- Keep detailed trading logs for analysis

---

## 8) Optional: Vercel Web Editor

**Deploy Web Interface:**
1. Import this repo to Vercel
2. Set Framework: **Other**
3. Set Output directory: `web`
4. Deploy

**Usage:**
1. Open your Vercel URL
2. Edit parameter values
3. Click **Copy Config**
4. Paste into `config/config.txt` in GitHub
5. Commit changes

The web interface auto-generates properly formatted config files and handles timestamp formatting.

---

## 9) Advanced Features

**SHA-256 Change Detection:**
- Strategy only reloads when config actually changes
- Reduces unnecessary processing and log noise
- Ensures reliable parameter updates

**Last-Known-Good Recovery:**
- Automatically saves valid configurations
- Falls back to previous working settings if new config fails validation
- Prevents strategy from breaking due to invalid parameters

**Dynamic Validation Bounds:**
- Configure maximum allowed values via config file
- Prevents accidentally dangerous parameter values
- Customizable per trading environment

---

## 10) Support and Community

**Getting Help:**
- Check NinjaScript Output window for error messages
- Review [DOCS.md](DOCS.md) for detailed technical information
- Open GitHub Issues for bugs or feature requests
- Use GitHub Discussions for general questions

**Contributing:**
- Read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines
- Always test on simulation first
- Focus on risk management and safety
- Share improvements with the community

---

## Author & Contact

**Authored by:** Olar Sa'id  
**GitHub:** [@ol-s-cloud](https://github.com/ol-s-cloud)  
**Repository:** [AGNES-BOT](https://github.com/ol-s-cloud/agnes-bot)  

*If this project helped you, please star the repository and consider contributing!*

**Disclaimer:** Educational software only. Trading involves substantial risk. Always test thoroughly on simulation accounts before live trading.



**Remember**: This software handles real money. Always prioritize safety, test thoroughly, and never trade with funds you cannot afford to lose.

**ALWAYS CHECK YOUR TRADING ENVIRONMENT BEFORE ENABLING THE STRATEGY.**

---

