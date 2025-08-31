# Trade Template MVP (EMA 10/50 arm → 10/200 trigger)

Hello guys, This MVP runs a NinjaTrader 8 strategy that reads a public **config file** and places ES trades with fixed TP/SL.
A tiny web page (deployable to Vercel) lets you build the config text easily.

PS: THIS IS A TESTSCRIPT FOR NOW AND TRY AT YOUR OWN RISK. AS I AM STILL WORKING ON THIS AND IMPROVING THE CODES FURTHER, I CANNOT GUARANTEE ANYTHING YET SO FEEL FREE TO CONTRIBIUTE AS WELL. SEE BELOW SO FAR WHAT I HAVE DONE AND HOW IT WORKS:

## How it works
- Arms when **EMA(10) crosses EMA(50)**.
- Triggers when **EMA(10) crosses EMA(200)** in the same direction.
- Optional filters: **FVG**, **Imbalance**, **Range**.
- **TP 20 ticks**, **SL 16 ticks**, **Quantity 1** (all adjustable in config).
- **account_mode**: `demo` or `live`. The strategy enforces this to prevent mistakes. (ALWAYS CHECK TO MAKE SURE YOUR ENVIRONMENT)



## Point NinjaTrader to your config
1. In NinjaTrader 8, add the strategy (see `nt/TradeTemplateEMA.cs`) and set its `ConfigUrl` to this Raw URL.
2. Enable the strategy on a chart. Start on **Sim101**.

## Safety Plugs Implemented
- The strategy will **block entries** if `account_mode=demo` and you’re not on a Sim account, or if `account_mode=live` and you’re on Sim.
- Keep secrets out of public repos. The config file has a comment placeholder for future API keys—don’t commit real keys.

STAY TUNED!
