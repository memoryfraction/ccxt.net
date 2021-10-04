using Newtonsoft.Json;
using CCXT.NET.Shared.Coin.Public;
using System.Collections.Generic;

namespace CCXT.NET.Ftx.Public
{
    /// <summary>
    ///
    /// </summary>
    public class FMarkets : CCXT.NET.Shared.Coin.Public.Markets, IMarkets
    {
        /// <summary>
        ///
        /// </summary>
        public new List<FMarketItem> result
        {
            get;
            set;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class FMarketItem : CCXT.NET.Shared.Coin.Public.MarketItem, IMarketItem
    {
        /// <summary>
        ///
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public override string symbol
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "enabled")]
        public override bool active
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string baseCurrency
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string quoteCurrency
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public string underlying
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal sizeIncrement
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal priceIncrement
        {
            get;
            set;
        }
    }
}