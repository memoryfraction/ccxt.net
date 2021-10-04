using System;
using System.Linq;
using System.Threading.Tasks;

namespace ftx
{
    class Program
    {
        private static int __step_no = 1;

        static async Task Main(string[] args)
        {
            var _public_api = new CCXT.NET.Ftx.Public.PublicApi();

            if (__step_no == 0)
            {
                var _markets = await _public_api.FetchMarketsAsync();
                if (_markets.success == true)
                {
                    Console.Out.WriteLine($"{_markets.success}");
                }
                else
                {
                    Console.Out.WriteLine($"error: {_markets.message}");
                }
            }

            if (__step_no == 1)
            {
                var _ticker = await _public_api.FetchTickerAsync("ETH-PERP");
                if (_ticker.success == true)
                {
                    Console.Out.WriteLine($"symbol: {_ticker.marketId}, closePrice: {_ticker.result.lastPrice}");
                }
                else
                {
                    Console.Out.WriteLine($"error: {_ticker.message}");
                }
            }

            if (__step_no == 2)
            {
                var _tickers = await _public_api.FetchTickersAsync();
                if (_tickers.success == true)
                {
                    var _btcusd_tickers = _tickers.result.Where(t => t.symbol.ToUpper().Contains("BTCUSD"));

                    foreach (var _t in _btcusd_tickers)
                        Console.Out.WriteLine($"symbol: {_t.symbol}, closePrice: {_t.closePrice}");
                }
                else
                {
                    Console.Out.WriteLine($"error: {_tickers.message}");
                }
            }

            Console.Out.WriteLine("hit return to exit...");
            Console.ReadLine();
        }
    }
}
