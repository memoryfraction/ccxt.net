using CCXT.NET.Shared.Coin;
using CCXT.NET.Shared.Coin.Public;
using CCXT.NET.Shared.Coin.Types;
using CCXT.NET.Shared.Configuration;
using CCXT.NET.Shared.Extension;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCXT.NET.Ftx.Public
{
    /// <summary>
    /// exchange's public API implement class
    /// </summary>
    public class PublicApi : CCXT.NET.Shared.Coin.Public.PublicApi, IPublicApi
    {
        /// <summary>
        ///
        /// </summary>
        public PublicApi(bool is_live = true)
        {
            IsLive = is_live;
        }

        private bool IsLive
        {
            get;
            set;
        }

        /// <summary>
        ///
        /// </summary>
        public override XApiClient publicClient
        {
            get
            {
                if (base.publicClient == null)
                {
                    var _division = (IsLive == false ? "test." : "") + "public";
                    base.publicClient = new FtxClient(_division);
                }

                return base.publicClient;
            }
        }

        /// <summary>
        /// Fetch symbols, market ids and exchanger's information
        /// </summary>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<Markets> FetchMarketsAsync(Dictionary<string, object> args = null)
        {
            var _result = new Markets();

            publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);
            {
                var _params = publicClient.MergeParamsAndArgs(args);

                var _json_value = await publicClient.CallApiGet1Async("/api/markets", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = publicClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _markets = publicClient.DeserializeObject<FMarkets>(_json_value.Content);
                    foreach (var _m in _markets.result)
                    {
                        if (_m.active == false)
                            continue;

                        _m.marketId = _m.symbol;

                        _m.baseId = _m.baseCurrency.IsNotEmpty() ? _m.baseCurrency : _m.underlying;
                        _m.quoteId = _m.quoteCurrency.IsNotEmpty() ? _m.quoteCurrency : "USD";

                        _m.baseName = publicClient.ExchangeInfo.CurrencyCode(_m.baseId);
                        _m.quoteName = publicClient.ExchangeInfo.CurrencyCode(_m.quoteId);

                        if (_m.type != "future")
                        {
                            _m.symbol = _m.baseName + "/" + _m.quoteName;
                            if (_m.type == "spot")
                                _m.spot = true;
                        }
                        else
                            _m.future = true;

                        _m.precision = new MarketPrecision()
                        {
                            amount = _m.sizeIncrement,
                            price = _m.priceIncrement
                        };

                        _m.limits = new MarketLimits
                        {
                            amount = new MarketMinMax
                            {
                                min = _m.sizeIncrement,
                                max = 0
                            },
                            price = new MarketMinMax
                            {
                                min = _m.priceIncrement,
                                max = 0
                            },
                            cost = new MarketMinMax
                            {
                                min = 0,
                                max = 0
                            }
                        };

                        _result.result.Add(_m.marketId, _m);
                    }
                }

                _result.SetResult(_json_result);
            }

            return _result;
        }

        /// <summary>
        /// Fetch current best bid and ask, as well as the last trade price.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="args">Add additional attributes for each exchange: timeframe</param>
        /// <returns></returns>
        public async ValueTask<Ticker> FetchTickerAsync(string symbol, Dictionary<string, object> args = null)
        {
            var _result = new Ticker(symbol);

            var _market = await this.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

                var _params = publicClient.MergeParamsAndArgs(args);

                var _json_value = await publicClient.CallApiGet1Async($"/api/markets/{_market.result.marketId}");
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = publicClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _json_data = publicClient.DeserializeObject<FTicker>(_json_value.Content);
                    if (_json_data.success == true)
                    {
                        _result.SetResult(_json_data);
                        {
                            _json_data.result.symbol = _market.result.symbol;
                            _json_data.result.timestamp = CUnixTime.NowMilli;
                            _json_data.result.percentage *= 100.0m;

                            _result.result = _json_data.result;
                        }
                    }
                    else
                    {
                        var _message = publicClient.GetErrorMessage(_json_data.statusCode);
                        _json_result.SetFailure(
                                _message,
                                ErrorCode.ResponseDataError
                            );
                    }
                }

                _result.SetResult(_json_result);
            }
            else
            {
                _result.SetResult(_market);
            }

            return _result;
        }

        /// <summary>
        /// Fetch pending or registered order details
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public async ValueTask<OrderBooks> FetchOrderBooksAsync(string symbol, int limits = 20, Dictionary<string, object> args = null)
        {
            var _result = new OrderBooks(symbol);

            var _market = await this.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

                var _params = new Dictionary<string, object>();
                {
                    _params.Add("symbol", _market.result.symbol);
                    _params.Add("depth", limits);

                    publicClient.MergeParamsAndArgs(_params, args);
                }

                var _json_value = await publicClient.CallApiGet1Async($"/api/markets/{_market.result.marketId}/orderbook");
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = publicClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _orderbooks = publicClient.DeserializeObject<List<BOrderBookItem>>(_json_value.Content);
                    {
                        var _asks = new List<OrderBookItem>();
                        var _bids = new List<OrderBookItem>();

                        foreach (var _o in _orderbooks)
                        {
                            _o.amount = _o.quantity * _o.price;
                            _o.count = 1;

                            if (_o.side.ToLower() == "sell")
                                _asks.Add(_o);
                            else
                                _bids.Add(_o);
                        }

                        _result.result.asks = _asks.OrderBy(o => o.price).Take(limits).ToList();
                        _result.result.bids = _bids.OrderByDescending(o => o.price).Take(limits).ToList();

                        _result.result.symbol = _market.result.symbol;
                        _result.result.timestamp = CUnixTime.NowMilli;
                        _result.result.nonce = CUnixTime.Now;
                    }
                }

                _result.SetResult(_json_result);
            }
            else
            {
                _result.SetResult(_market);
            }

            return _result;
        }

        /// <summary>
        /// Fetch array of symbol name and OHLCVs data
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="timeframe">time frame interval (optional): default "1d"</param>
        /// <param name="since">return committed data since given time (milli-seconds) (optional): default 0</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public async ValueTask<OHLCVs> FetchOHLCVsAsync(string symbol, string timeframe = "1d", long since = 0, int limits = 20, Dictionary<string, object> args = null)
        {
            var _result = new OHLCVs(symbol);

            var _market = await this.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

                var _timeframe = publicClient.ExchangeInfo.GetTimeframe(timeframe);
                var _timestamp = publicClient.ExchangeInfo.GetTimestamp(timeframe);

                var _params = new Dictionary<string, object>();
                {
                    var _limits = limits <= 1 ? 1
                                : limits <= 500 ? limits
                                : 500;

                    _params.Add("symbol", _market.result.symbol);
                    _params.Add("binSize", _timeframe);
                    _params.Add("count", _limits);
                    _params.Add("partial", false);
                    _params.Add("reverse", true);

                    publicClient.MergeParamsAndArgs(_params, args);
                }

                var _json_value = await publicClient.CallApiGet1Async($"/api/markets/{_market.result.marketId}/candles");
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = publicClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _json_data = publicClient.DeserializeObject<List<FTickerItem>>(_json_value.Content);

                    _result.result.AddRange(
                         _json_data
                             .Select(x => new OHLCVItem
                             {
                                 timestamp = x.timestamp,
                                 openPrice = x.openPrice,
                                 highPrice = x.highPrice,
                                 lowPrice = x.lowPrice,
                                 closePrice = x.closePrice,
                                 amount = x.quoteVolume,
                                 volume = x.baseVolume
                             })
                             .Where(o => o.timestamp >= since)
                             .OrderByDescending(o => o.timestamp)
                             .Take(limits)
                         );
                }

                _result.SetResult(_json_result);
            }
            else
            {
                _result.SetResult(_market);
            }

            return _result;
        }

        /// <summary>
        /// Fetch array of recent trades data
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="timeframe">time frame interval (optional): default "1d"</param>
        /// <param name="since">return committed data since given time (milli-seconds) (optional): default 0</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public async ValueTask<CompleteOrders> FetchCompleteOrdersAsync(string symbol, string timeframe = "1d", long since = 0, int limits = 20, Dictionary<string, object> args = null)
        {
            var _result = new CompleteOrders(symbol);

            var _market = await this.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                publicClient.ExchangeInfo.ApiCallWait(TradeType.Public);

                var _timeframe = publicClient.ExchangeInfo.GetTimeframe(timeframe);
                var _timestamp = publicClient.ExchangeInfo.GetTimestamp(timeframe);

                var _params = new Dictionary<string, object>();
                {
                    var _limits = limits <= 1 ? 1
                                : limits <= 500 ? limits
                                : 500;

                    _params.Add("symbol", _market.result.symbol);
                    _params.Add("count", _limits);
                    _params.Add("reverse", true);

                    publicClient.MergeParamsAndArgs(_params, args);
                }

                var _json_value = await publicClient.CallApiGet1Async($"/api/markets/{_market.result.marketId}/trades");
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = publicClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _json_data = publicClient.DeserializeObject<List<BCompleteOrderItem>>(_json_value.Content);
                    {
                        var _orders = _json_data
                                                .Where(t => t.timestamp >= since)
                                                .OrderByDescending(t => t.timestamp)
                                                .Take(limits);

                        foreach (var _o in _orders)
                        {
                            _o.orderType = OrderType.Limit;
                            _o.fillType = FillType.Fill;

                            _o.amount = _o.quantity * _o.price;
                            _result.result.Add(_o);
                        }
                    }
                }

                _result.SetResult(_json_result);
            }
            else
            {
                _result.SetResult(_market);
            }

            return _result;
        }
    }
}