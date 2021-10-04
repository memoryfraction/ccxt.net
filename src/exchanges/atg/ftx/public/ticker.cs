using CCXT.NET.Shared.Coin.Public;
using Newtonsoft.Json;

namespace CCXT.NET.Ftx.Public
{
    /// <summary>
    /// 
    /// </summary>
    public class FTicker : CCXT.NET.Shared.Coin.Public.Ticker, ITicker
    {
        /// <summary>
        ///
        /// </summary>
        public new FTickerItem result
        {
            get;
            set;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class FTickerItem : CCXT.NET.Shared.Coin.Public.TickerItem, ITickerItem
    {
        /// <summary>
        /// string symbol of the market ('BTCUSD', 'ETHBTC', ...)
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public override string symbol
        {
            get;
            set;
        }
        
        /// <summary>
        /// highest price for last 24H
        /// </summary>
        [JsonProperty(PropertyName = "high")]
        public override decimal highPrice
        {
            get;
            set;
        }

        /// <summary>
        /// lowest price for last 24H
        /// </summary>
        [JsonProperty(PropertyName = "low")]
        public override decimal lowPrice
        {
            get;
            set;
        }
        
        /// <summary>
        /// current best bid (buy) price
        /// </summary>
        [JsonProperty(PropertyName = "bid")]
        public override decimal bidPrice
        {
            get;
            set;
        }

        /// <summary>
        /// current best ask (sell) price
        /// </summary>
        [JsonProperty(PropertyName = "ask")]
        public override decimal askPrice
        {
            get;
            set;
        }

        /// <summary>
        /// current best bid (buy) amount (may be missing or undefined)
        /// </summary>
        [JsonProperty(PropertyName = "bidSize")]
        public override decimal bidQuantity
        {
            get;
            set;
        }

        /// <summary>
        /// current best ask (sell) amount (may be missing or undefined)
        /// </summary>
        [JsonProperty(PropertyName = "askSize")]
        public override decimal askQuantity
        {
            get;
            set;
        }

        /// <summary>
        /// price of last trade (closing price for current period)
        /// </summary>
        [JsonProperty(PropertyName = "last")]
        public override decimal closePrice
        {
            get;
            set;
        }

        /// <summary>
        /// volume of quote currency traded for last 24 hours
        /// </summary>
        [JsonProperty(PropertyName = "quoteVolume24h")]
        public override decimal quoteVolume
        {
            get;
            set;
        }

        /// <summary>
        /// relative change, `(changePrice / openPrice) * 100`
        /// </summary>
        [JsonProperty(PropertyName = "change24h")]
        public override decimal percentage
        {
            get;
            set;
        }
   }
}