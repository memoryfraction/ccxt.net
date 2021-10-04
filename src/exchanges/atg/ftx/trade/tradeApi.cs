using CCXT.NET.Shared.Coin;
using CCXT.NET.Shared.Coin.Trade;
using CCXT.NET.Shared.Coin.Types;
using CCXT.NET.Shared.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCXT.NET.Ftx.Trade
{
    /// <summary>
    ///
    /// </summary>
    public class TradeApi : CCXT.NET.Shared.Coin.Trade.TradeApi, ITradeApi
    {
        private readonly string __connect_key;
        private readonly string __secret_key;

        /// <summary>
        ///
        /// </summary>
        public TradeApi(string connect_key, string secret_key, bool is_live = true)
        {
            __connect_key = connect_key;
            __secret_key = secret_key;

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
        public override XApiClient tradeClient
        {
            get
            {
                if (base.tradeClient == null)
                {
                    var _division = (IsLive == false ? "test." : "") + "trade";
                    base.tradeClient = new FtxClient(_division, __connect_key, __secret_key);
                }

                return base.tradeClient;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public override CCXT.NET.Shared.Coin.Public.PublicApi publicApi
        {
            get
            {
                if (base.publicApi == null)
                    base.publicApi = new CCXT.NET.Ftx.Public.PublicApi();

                return base.publicApi;
            }
        }

        /// <summary>
        /// Get your orders.
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="timeframe">time frame interval (optional): default "1d"</param>
        /// <param name="since">return committed data since given time (milli-seconds) (optional): default 0</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<MyOrders> FetchMyOrdersAsync(string base_name, string quote_name, string timeframe = "1d", long since = 0, int limits = 20, Dictionary<string, object> args = null)
        {
            var _result = new MyOrders(base_name, quote_name);

            var _market = await publicApi.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _params = new Dictionary<string, object>();
                {
                    _params.Add("market", _market.result.symbol);

                    tradeClient.MergeParamsAndArgs(_params, args);
                }

                var _json_value = await tradeClient.CallApiGet1Async("/api/orders", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _json_data = tradeClient.DeserializeObject<List<BMyOrderItem>>(_json_value.Content);
                    {
                        var _orders = _json_data
                                            .Where(o => o.symbol == _market.result.symbol && o.timestamp >= since)
                                            .OrderByDescending(o => o.timestamp)
                                            .Take(limits);

                        foreach (var _o in _orders)
                        {
                            _o.amount = _o.price * _o.quantity;
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

        /// <summary>
        /// Get all balance-affecting executions. This includes each trade, insurance charge, and settlement.
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="timeframe">time frame interval (optional): default "1d"</param>
        /// <param name="since">return committed data since given time (milli-seconds) (optional): default 0</param>
        /// <param name="limits">maximum number of items (optional): default 20</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<MyTrades> FetchMyTradesAsync(string base_name, string quote_name, string timeframe = "1d", long since = 0, int limits = 20, Dictionary<string, object> args = null)
        {
            var _result = new MyTrades(base_name, quote_name);

            var _market = await publicApi.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _timeframe = tradeClient.ExchangeInfo.GetTimeframe(timeframe);
                var _timestamp = tradeClient.ExchangeInfo.GetTimestamp(timeframe);

                var _params = new Dictionary<string, object>();
                {
                    _params.Add("symbol", _market.result.symbol);
                    _params.Add("count", limits);
                    _params.Add("reverse", true);

                    if (since > 0)
                        _params.Add("startTime", CUnixTime.ConvertToUtcTimeMilli(since).ToString("yyyy-MM-dd HH:mm"));

                    tradeClient.MergeParamsAndArgs(_params, args);
                }

                var _json_value = await tradeClient.CallApiGet1Async($"/api/markets/USD/trades", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _json_data = tradeClient.DeserializeObject<List<BMyTradeItem>>(_json_value.Content);
                    {
                        var _trades = _json_data
                                            .Where(t => t.timestamp >= since)
                                            .OrderByDescending(t => t.timestamp)
                                            .Take(limits);

                        foreach (var _t in _trades)
                        {
                            _t.amount = _t.price * _t.quantity;
                            _result.result.Add(_t);
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

        /// <summary>
        /// Create a new limit order.
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="quantity">amount of coin</param>
        /// <param name="price">price of coin</param>
        /// <param name="sideType">type of buy(bid) or sell(ask)</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<MyOrder> CreateLimitOrderAsync(string base_name, string quote_name, decimal quantity, decimal price, SideType sideType, Dictionary<string, object> args = null)
        {
            var _result = new MyOrder(base_name, quote_name);

            var _market = await publicApi.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _buy_sell = sideType == SideType.Bid ? "Buy" : "Sell";

                var _params = tradeClient.MergeParamsAndArgs(
                    new Dictionary<string, object>
                    {
                        { "symbol", _market.result.symbol },
                        { "side", _buy_sell },
                        { "ordType", "Limit" },
                        { "orderQty", quantity },
                        { "price", price }
                    },
                    args
                );

                var _json_value = await tradeClient.CallApiPost1Async("/api/v1/order", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _order = tradeClient.DeserializeObject<BPlaceOrderItem>(_json_value.Content);
                    {
                        _order.orderType = OrderType.Limit;

                        _order.remaining = Math.Max(_order.quantity - _order.filled, 0);
                        _order.cost = _order.price * _order.filled;

                        _result.result = _order;
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
        /// Create a new market order.
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="quantity">amount of coin</param>
        /// <param name="price">price of coin</param>
        /// <param name="sideType">type of buy(bid) or sell(ask)</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<MyOrder> CreateMarketOrderAsync(string base_name, string quote_name, decimal quantity, decimal price, SideType sideType, Dictionary<string, object> args = null)
        {
            var _result = new MyOrder(base_name, quote_name);

            var _market = await publicApi.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _buy_sell = sideType == SideType.Bid ? "Buy" : "Sell";

                var _params = tradeClient.MergeParamsAndArgs(
                    new Dictionary<string, object>
                    {
                        { "symbol", _market.result.symbol },
                        { "side", _buy_sell },
                        { "ordType", "Market" },
                        { "orderQty", quantity }
                    },
                    args
                );

                var _json_value = await tradeClient.CallApiPost1Async("/api/v1/order", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _order = tradeClient.DeserializeObject<BPlaceOrderItem>(_json_value.Content);
                    {
                        _order.orderType = OrderType.Market;

                        //_order.amount = (_order.quantity * _order.price).Normalize();
                        //_order.fee = _order.amount * tradeClient.ExchangeInfo.Fees.trading.maker;

                        _result.result = _order;
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
        /// Create a new limit bulk order.
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public async ValueTask<MyOrders> CreateBulkOrder(List<BBulkOrderItem> orders, Dictionary<string, object> args = null)
        {
            var _result = new MyOrders();

            tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);
            {
                var _params = tradeClient.MergeParamsAndArgs(
                    new Dictionary<string, object>
                    {
                        { "orders", orders }
                    },
                    args
                );

                var _json_value = await tradeClient.CallApiPost1Async("/api/v1/order/bulk", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _orders = tradeClient.DeserializeObject<List<BMyOrderItem>>(_json_value.Content);
                    {
                        _result.result = _orders.ToList<IMyOrderItem>();
                    }
                }

                _result.SetResult(_json_result);
            }

            return _result;
        }

        /// <summary>
        /// Close a position
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="orderType">The type of order is limit, market or position</param>
        /// <param name="price">price of coin</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public async ValueTask<MyOrder> ClosePosition(string base_name, string quote_name, OrderType orderType, decimal price = 0.0m, Dictionary<string, object> args = null)
        {
            var _result = new MyOrder(base_name, quote_name);

            var _market = await publicApi.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _params = new Dictionary<string, object>();
                {
                    _params.Add("symbol", _market.result.symbol);
                    _params.Add("execInst", "Close");

                    if (orderType == OrderType.Limit)
                        _params.Add("price", price);

                    tradeClient.MergeParamsAndArgs(_params, args);
                }

                var _json_value = await tradeClient.CallApiPost1Async("/api/v1/order", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _order = tradeClient.DeserializeObject<BMyOrderItem>(_json_value.Content);
                    {
                        _result.result = _order;
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
        ///
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="leverage"></param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public async ValueTask<MyPosition> ChooseLeverage(string base_name, string quote_name, decimal leverage, Dictionary<string, object> args = null)
        {
            var _result = new MyPosition(base_name, quote_name);

            var _market = await publicApi.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _params = tradeClient.MergeParamsAndArgs(
                    new Dictionary<string, object>
                    {
                        { "symbol", _market.result.symbol },
                        { "leverage", leverage }
                    },
                    args
                );

                var _json_value = await tradeClient.CallApiPost1Async("/api/v1/position/leverage", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _position = tradeClient.DeserializeObject<BMyPositionItem>(_json_value.Content);
                    {
                        _result.result = _position;
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
        /// Update an order.
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="order_id">Order number registered for sale or purchase</param>
        /// <param name="quantity">amount of coin</param>
        /// <param name="price">price of coin</param>
        /// <param name="sideType">type of buy(bid) or sell(ask)</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public async ValueTask<MyOrder> UpdateOrder(string base_name, string quote_name, string order_id, decimal quantity, decimal price, SideType sideType, Dictionary<string, object> args = null)
        {
            var _result = new MyOrder(base_name, quote_name);

            var _market = await publicApi.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _params = tradeClient.MergeParamsAndArgs(
                    new Dictionary<string, object>
                    {
                        { "orderID", order_id },
                        { "orderQty", quantity },
                        { "price", price }
                    },
                    args
                );

                var _json_value = await tradeClient.CallApiPut1Async("/api/v1/order", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _json_data = tradeClient.DeserializeObject<BPlaceOrderItem>(_json_value.Content);
                    _result.result = _json_data;
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
        /// Update orders in bulk.
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public async ValueTask<MyOrders> UpdateOrders(List<BBulkUpdateOrderItem> orders, Dictionary<string, object> args = null)
        {
            var _result = new MyOrders();

            tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);
            {
                var _params = tradeClient.MergeParamsAndArgs(
                    new Dictionary<string, object>
                    {
                        { "orders", orders }
                    },
                    args
                );

                var _json_value = await tradeClient.CallApiPut1Async("/api/v1/order/bulk", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _orders = tradeClient.DeserializeObject<List<BMyOrderItem>>(_json_value.Content);
                    {
                        _result.result = _orders.ToList<IMyOrderItem>();
                    }
                }

                _result.SetResult(_json_result);
            }

            return _result;
        }

        /// <summary>
        /// Cancel an order.
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="order_id">Order number registered for sale or purchase</param>
        /// <param name="quantity">amount of coin</param>
        /// <param name="price">price of coin</param>
        /// <param name="sideType">type of buy(bid) or sell(ask)</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<MyOrder> CancelOrderAsync(string base_name, string quote_name, string order_id, decimal quantity, decimal price, SideType sideType, Dictionary<string, object> args = null)
        {
            var _result = new MyOrder(base_name, quote_name);

            var _market = await publicApi.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _params = tradeClient.MergeParamsAndArgs(
                    new Dictionary<string, object>
                    {
                        { "orderID", order_id }
                    },
                    args
                );

                var _json_value = await tradeClient.CallApiDelete1Async("/api/v1/order", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _json_data = tradeClient.DeserializeObject<List<BPlaceOrderItem>>(_json_value.Content);

                    var _order = _json_data.FirstOrDefault();
                    if (_order != null)
                    {
                        _result.result = _order;
                    }
                    else
                    {
                        _json_result.SetFailure();
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
        /// Cancel orders. Send multiple order IDs to cancel in bulk.
        /// </summary>
        /// <param name="base_name">The type of trading base-currency of which information you want to query for.</param>
        /// <param name="quote_name">The type of trading quote-currency of which information you want to query for.</param>
        /// <param name="order_ids"></param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<MyOrders> CancelOrdersAsync(string base_name, string quote_name, string[] order_ids, Dictionary<string, object> args = null)
        {
            var _result = new MyOrders(base_name, quote_name);

            var _market = await publicApi.LoadMarketAsync(_result.marketId);
            if (_market.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _params = tradeClient.MergeParamsAndArgs(
                    new Dictionary<string, object>
                    {
                        { "orderID", order_ids }
                    },
                    args
                );

                var _json_value = await tradeClient.CallApiDelete1Async("/api/v1/order", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _json_data = tradeClient.DeserializeObject<List<BPlaceOrderItem>>(_json_value.Content);
                    {
                        _result.result.AddRange(_json_data);
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
        /// Cancels all of your orders.
        /// </summary>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<MyOrders> CancelAllOrdersAsync(Dictionary<string, object> args = null)
        {
            var _result = new MyOrders();

            var _markets = await publicApi.LoadMarketsAsync();
            if (_markets.success == true)
            {
                tradeClient.ExchangeInfo.ApiCallWait(TradeType.Trade);

                var _params = tradeClient.MergeParamsAndArgs(args);

                var _json_value = await tradeClient.CallApiDelete1Async("/api/v1/order/all", _params);
#if DEBUG
                _result.rawJson = _json_value.Content;
#endif
                var _json_result = tradeClient.GetResponseMessage(_json_value.Response);
                if (_json_result.success == true)
                {
                    var _json_data = tradeClient.DeserializeObject<List<BPlaceOrderItem>>(_json_value.Content);
                    {
                        _result.result.AddRange(_json_data);
                    }
                }

                _result.SetResult(_json_result);
            }
            else
            {
                _result.SetResult(_markets);
            }

            return _result;
        }
    }
}