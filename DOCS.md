# Technical Documentation

## Architecture Overview

The Trade Template MVP implements a distributed configuration system that separates strategy logic from parameter management, enabling real-time updates without recompilation.

### Component Breakdown

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────┐
│   Web Editor    │    │  GitHub Config   │    │   NinjaTrader 8     │
│   (Vercel)      │ -> │  (Public Repo)   │ <- │   Strategy          │
└─────────────────┘    └──────────────────┘    └─────────────────────┘
                                ^                          ^
                                │                          │
                        ┌──────────────────┐              │
                        │  Local Override  │ -------------┘
                        │     (Private)    │
                        └──────────────────┘
```

### Configuration Hierarchy

1. **Base Configuration**: GitHub public config file
2. **Local Overrides**: Private local file (LOCAL WINS)
3. **UI Defaults**: NinjaTrader strategy properties
4. **Last Known Good**: Automatic fallback system

## Strategy Logic

### Two-Stage Entry System

**Arming Phase:**
- Monitors EMA(10) vs EMA(50) crossovers
- Sets `pendingDir`: +1 (long) or -1 (short)
- Remains armed until trigger or opposite cross

**Trigger Phase:**
- Requires EMA(10) vs EMA(200) cross in same direction as arm
- Executes entry with pre-set TP/SL
- Resets arm state after trigger attempt

### Optional Filters

**Fair Value Gap (FVG):**
```csharp
bool bullFVG = Low[1] > High[2];  // 3-bar gap up
bool bearFVG = High[1] < Low[2];  // 3-bar gap down
```

**Imbalance Filter:**
```csharp
double bodyRatio = Math.Abs(Close[0] - Open[0]) / (High[0] - Low[0]);
// Requires bodyRatio >= 0.6 (strong directional bar)
```

**Range Filter:**
```csharp
double range = MAX(High, RangeLookback)[0] - MIN(Low, RangeLookback)[0];
// Blocks trades when range > RangeMaxTicks * TickSize
```

## Configuration System

### File Format

Key-value pairs with hash-style comments:
```
# Strategy Parameters
ema_fast=10
ema_mid=50
ema_slow=200
tp_ticks=20
sl_ticks=16
quantity=1

# Account Safety
account_mode=demo

# Versioning
config_version=v1.1
updated_at=2025-01-09T15:30:00Z
```

### Polling Mechanism

**Frequency:** Every `poll_seconds` (default: 5)
**Process:**
1. Download GitHub config via HTTP
2. Read local override file (if exists)
3. Merge dictionaries (local wins conflicts)
4. Generate SHA-256 hash of merged config
5. Skip application if hash unchanged
6. Validate parameters against bounds
7. Apply valid config or fallback to Last Known Good

### Change Detection

SHA-256 hashing prevents unnecessary parameter updates:
```csharp
private string HashKv(Dictionary<string,string> kv)
{
    var ordered = kv.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase);
    var content = string.Join("\n", ordered.Select(kvp => $"{kvp.Key}={kvp.Value}"));
    using (var sha = SHA256.Create())
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "");
    }
}
```

### Validation System

**Parameter Bounds:**
- EMA periods: 1 to `max_ema_length` (default 1000)
- TP/SL ticks: 1 to `max_ticks` (default 500)  
- Quantity: 1 to `max_quantity` (default 50)
- Range lookback: 2 to `max_range_lookback` (default 1000)
- Range ticks: 1 to `max_range_ticks` (default 500)

**Critical Validations:**
- Zero TP/SL detection (prevents invalid orders)
- Account mode string validation
- Numeric parsing with fallbacks

**Fallback Chain:**
1. New merged config (if valid)
2. Last Known Good config (if valid)
3. Current in-memory values (no changes)

### Account Mode Enforcement

Prevents production/simulation mix-ups:

| account_mode | Sim Account | Live Account |
|-------------|-------------|--------------|
| `demo`      | ✅ Allowed   | ❌ Blocked    |
| `live`      | ❌ Blocked   | ✅ Allowed    |

Detection logic:
```csharp
bool isSim = Account?.Name?.IndexOf("Sim", StringComparison.OrdinalIgnoreCase) >= 0;
```

## Complete Configuration Reference

### Core Strategy Parameters

| Parameter | Type | Default | Range | Description |
|-----------|------|---------|-------|-------------|
| `ema_fast` | int | 10 | 1-1000 | Fast EMA period |
| `ema_mid` | int | 50 | 1-1000 | Medium EMA (arm signal) |
| `ema_slow` | int | 200 | 1-1000 | Slow EMA (trigger signal) |
| `tp_ticks` | int | 20 | 1-500 | Take profit in ticks |
| `sl_ticks` | int | 16 | 1-500 | Stop loss in ticks |
| `quantity` | int | 1 | 1-50 | Position size |

### Filter Parameters

| Parameter | Type | Default | Range | Description |
|-----------|------|---------|-------|-------------|
| `use_fvg` | bool | false | - | Enable Fair Value Gap filter |
| `use_imbalance` | bool | false | - | Enable strong-body bar filter |
| `use_range_filter` | bool | false | - | Enable range-bound market filter |
| `range_lookback` | int | 20 | 2-1000 | Bars to analyze for range |
| `range_max_ticks` | int | 24 | 1-500 | Maximum range to allow trading |

### Operational Parameters

| Parameter | Type | Default | Range | Description |
|-----------|------|---------|-------|-------------|
| `account_mode` | string | demo | demo/live | Account type enforcement |
| `config_version` | string | v1 | - | Configuration identifier |
| `updated_at` | string | auto | ISO-8601 | Timestamp for tracking changes |
| `poll_seconds` | int | 5 | 1-300 | Config refresh frequency |
| `lkg_path` | string | - | file path | Custom Last Known Good location |

### Validation Bounds (Advanced)

| Parameter | Type | Default | Range | Description |
|-----------|------|---------|-------|-------------|
| `max_ema_length` | int | 1000 | 100-10000 | Upper limit for EMA periods |
| `max_ticks` | int | 500 | 10-10000 | Upper limit for TP/SL ticks |
| `max_quantity` | int | 50 | 1-10000 | Upper limit for position size |
| `max_range_lookback` | int | 1000 | 10-20000 | Upper limit for range analysis |
| `max_range_ticks` | int | 500 | 10-10000 | Upper limit for range filter |

## Local Configuration Setup

### Why Use Local Files?

1. **Security**: Keep API keys and sensitive data private
2. **Flexibility**: Override specific parameters without forking repos
3. **Performance**: Faster access than network calls
4. **Compliance**: Meet broker/regulatory requirements

### Setup Instructions

1. **Determine Local Path:**
   - Default: `%USERPROFILE%\Documents\NinjaTrader 8\templates\Strategy\trade_template_local.config`
   - Custom: Set `LocalConfigPath` in NinjaTrader strategy properties

2. **Create Local Override File:**
   ```
   # Local overrides - this file is private
   account_mode=live
   quantity=3
   tp_ticks=25
   
   # Future API integration
   api_key=your_secret_key_here
   rithmic_user=your_username
   ```

3. **Test Configuration:**
   - Enable strategy on chart
   - Check NinjaTrader Output window for config logs
   - Verify parameters applied correctly

### Security Best Practices

- Never commit local config files to version control
- Add `*local*.config` to `.gitignore`
- Use environment-specific local files for different trading accounts
- Regularly backup local configurations

## Deployment Guide

### NinjaTrader Setup

1. **Import Strategy:**
   ```
   NinjaScript Editor → Strategies → New Strategy
   Name: TradeTemplateEMA
   Replace content with nt/TradeTemplateEMA.cs
   Compile (F5)
   ```

2. **Configure URLs:**
   - Set `ConfigUrl` to your GitHub raw URL
   - Set `LocalConfigPath` if using local overrides
   - Leave other properties as defaults initially

3. **Chart Setup:**
   ```
   Right-click chart → Strategies → Add TradeTemplateEMA
   Enable: True
   Account: Start with Sim101
   Confirm strategy loads without errors
   ```

### Vercel Web Interface

1. **Deploy:**
   - Import GitHub repo to Vercel
   - Set Output Directory: `web`
   - Deploy

2. **Usage:**
   - Visit deployed URL
   - Adjust parameters
   - Click "Copy Config"
   - Paste into GitHub `config/config.txt`
   - Commit changes

### Monitoring

**NinjaTrader Output Window:**
```
[Config] Applied OK. version=v1.1, updated_at=2025-01-09T15:30:00Z, account_mode=demo
[Guard] account_mode=demo but account is not Sim. Blocking entries.
```

**Configuration Change Logs:**
- Successful applies show version and timestamp
- Validation failures indicate specific problems
- Guard messages show account enforcement

## Troubleshooting

### Common Issues

**Strategy Not Loading Config:**
- Verify GitHub raw URL is correct
- Check network connectivity from NinjaTrader PC
- Confirm config file syntax (key=value format)

**Validation Failures:**
- Check parameter bounds against max_* settings
- Ensure TP/SL are not zero
- Verify numeric values parse correctly

**Account Mode Blocks:**
- Confirm account name matches mode (Sim vs Live)
- Check account connection in NinjaTrader
- Verify account_mode parameter spelling

**Local File Not Found:**
- Check file path and permissions
- Verify directory exists
- Use absolute paths for custom locations

### Debug Steps

1. **Enable Strategy → Check Output Window**
2. **Test Config Manually → Copy/paste to validate syntax**
3. **Verify Account Mode → Print Account.Name in strategy**
4. **Check File Paths → Use File.Exists() debugging**
5. **Monitor Hash Changes → Log applied vs candidate hashes**

---

---

## Author & Contact

**Authored by:** Olar Sa'id  
**GitHub:** [@ol-s-cloud](https://github.com/ol-s-cloud)  
**Repository:** [AGNES-BOT](https://github.com/ol-s-cloud/agnes-bot)  

*If this project helped you, please star the repository and consider contributing!*

**Disclaimer:** Educational software only. Trading involves substantial risk. Always test thoroughly on simulation accounts before live trading.


*For additional support, see GitHub Issues or Discussions.*
