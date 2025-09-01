# Trade Template MVP (EMA 10/50 arm → 10/200 trigger)

[![Version](https://img.shields.io/badge/version-v1.0-blue.svg)](https://github.com/yourusername/trade-template-mvp/releases)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![NinjaTrader](https://img.shields.io/badge/NinjaTrader-8-orange.svg)](https://ninjatrader.com/)
[![Deploy to Vercel](https://img.shields.io/badge/deploy-vercel-black.svg)](https://vercel.com/import/project?template=https://github.com/yourusername/trade-template-mvp)

Hello guys, This MVP runs a NinjaTrader 8 strategy that reads configuration from **GitHub + local files** and executes ES trades with dynamic TP/SL management. A browser-based config editor (deployable to Vercel) lets you adjust parameters without touching code.

**⚠️ DISCLAIMER: THIS IS A TEST SCRIPT FOR NOW - TRY AT YOUR OWN RISK. I'M STILL WORKING ON IMPROVEMENTS. CONTRIBUTIONS WELCOME!**

## 🎯 How It Works

**Two-Stage Entry System:**
- **Arms** when EMA(10) crosses EMA(50)
- **Triggers** when EMA(10) crosses EMA(200) in the same direction
- Optional filters: **FVG**, **Imbalance**, **Range**
- **TP 20 ticks**, **SL 16 ticks**, **Quantity 1** (all adjustable)

**Smart Configuration System:**
- Polls GitHub config every 5 seconds for real-time updates
- Local file overrides for sensitive parameters
- Last-known-good recovery if invalid config detected
- SHA-256 change detection prevents unnecessary reloads

## 🚀 Quick Start

### 1. Deploy Web Editor (Optional)
[![Deploy with Vercel](https://vercel.com/button)](https://vercel.com/new/clone?repository-url=https://github.com/ol-s-cloud/agnes-bot)

Set **Output Directory** to `web` in Vercel settings.

### 2. Get Your Config URL
1. Fork this repo
2. Go to `config/config.txt` → Click **Raw**
3. Copy the URL (looks like `https://raw.githubusercontent.com/YOUR-USER/trade-template-mvp/main/config/config.txt`)

### 3. Setup NinjaTrader
1. Open NinjaScript Editor → New Strategy
2. Replace content with `nt/TradeTemplateEMA.cs`
3. Set `ConfigUrl` to your Raw GitHub URL
4. **Start with Sim101 account**

### 4. Configure Parameters
- Use the web editor OR edit `config/config.txt` directly
- Commit changes → NinjaTrader picks them up automatically

##  Safety Features

**Account Mode Enforcement:**
- `account_mode=demo` → Blocks non-Sim accounts
- `account_mode=live` → Blocks Sim accounts
- Prevents accidental live trading

**Configuration Validation:**
- Parameter bounds checking
- Zero TP/SL detection  
- Last-known-good fallback system
- Local file overrides for sensitive data

**Risk Management:**
- Fixed position sizing
- Mandatory stop-loss and take-profit
- Bar-close execution (reduces noise)

##  Repository Structure

```
trade-template-mvp/
├── README.md                  # You are here
├── DOCS.md                    # Detailed technical docs
├── CHANGELOG.md               # Version history
├── config/
│   └── config.txt            # Live configuration (public)
├── nt/
│   └── TradeTemplateEMA.cs   # NinjaTrader strategy
└── web/
    └── index.html            # Config editor interface
```

## ⚙️ Key Configuration Parameters

| Parameter | Default | Description |
|-----------|---------|-------------|
| `ema_fast` | 10 | Fast EMA period |
| `ema_mid` | 50 | Medium EMA period (arm signal) |
| `ema_slow` | 200 | Slow EMA period (trigger signal) |
| `tp_ticks` | 20 | Take profit in ticks |
| `sl_ticks` | 16 | Stop loss in ticks |
| `account_mode` | demo | demo/live enforcement |
| `poll_seconds` | 5 | Config reload frequency |

See [DOCS.md](DOCS.md) for complete parameter reference.

##  Local File Approach

**Why Local Files?**
- Keep secrets out of public repos
- Override specific parameters without forking
- Faster response (no network calls)
- Compliance with broker requirements

Create `trade_template_local.config` in your Documents folder:
```
account_mode=live
quantity=2
# api_key=your_secret_key_here
```

## Filters (Imbalance, FVG, Range)

The strategy supports three optional filters to refine trade entries.  
They are **disabled by default** in `config/config.txt`, but you can turn them on in either the GitHub config or your **local override file**.

### 🔹 1. Imbalance Filter
- Key: `use_imbalance`
- Checks if candle body ≥ 60% of bar range.
- Long trades require close > open, shorts require close < open.
- Blocks weak candles.

### 🔹 2. FVG (Fair Value Gap) Filter
- Key: `use_fvg`
- Uses a 3-bar pattern:
  - Bullish FVG → `Low[1] > High[2]`
  - Bearish FVG → `High[1] < Low[2]`
- Blocks trades if no FVG in the right direction.

### 🔹 3. Range Filter
- Keys: `use_range_filter`, `range_lookback`, `range_max_ticks`
- Looks back `range_lookback` bars, measures `High - Low`.
- If range > `range_max_ticks * TickSize`, trade is blocked (market too wide).

### ✅ Example local override
Put this in your local config file to enable all three filters:

```txt
use_fvg=true
use_imbalance=true
use_range_filter=true
range_lookback=20
range_max_ticks=24


##  Contributing

Contributions welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) first.

**Priority Areas:**
- Additional technical indicators
- Risk management enhancements  
- Backtesting integration
- Multi-timeframe analysis

##  Roadmap

- [ ] Daily loss limits
- [ ] Position sizing algorithms
- [ ] Multi-asset support
- [ ] Backtesting framework
- [ ] Real-time performance metrics
- [ ] Webhook integrations

---

## Author & Contact

**Authored by:** Olar Sa'id  
**GitHub:** [@ol-s-cloud](https://github.com/ol-s-cloud)  
**Repository:** [AGNES-BOT](https://github.com/ol-s-cloud/agnes-bot)  

*If this project helped you, please star the repository and consider contributing!*

**Disclaimer:** Educational software only. Trading involves substantial risk. Always test thoroughly on simulation accounts before live trading.

##  Support

- **Documentation:** [DOCS.md](DOCS.md)

##  Legal

This software is for educational purposes. Trading involves substantial risk. Past performance does not guarantee future results. Always test thoroughly on simulation before risking real capital.

**ALWAYS CHECK YOUR TRADING ENVIRONMENT BEFORE ENABLING THE STRATEGY.**

---

**Version:** v1.0 | **Last Updated:** 2025-01-09 | **License:** MIT
