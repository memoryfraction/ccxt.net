using OdinSdk.BaseLib.Coin;

namespace CCXT.NET.Bithumb.Public
{
    /// <summary>
    /// https://api.bithumb.com/public/ticker/{currency}
    /// bithumb 거래소 마지막 거래 정보
    /// * {currency} = BTC, ETH, DASH, LTC, ETC, XRP (기본값: BTC), ALL(전체)
    /// </summary>
    public class BTickerItem
    {
        /// <summary>
        /// 최근 24시간 내 시작 거래금액
        /// </summary>
        public decimal opening_price
        {
            get;
            set;
        }

        /// <summary>
        /// 최근 24시간 내 마지막 거래금액
        /// </summary>
        public decimal closing_price
        {
            get;
            set;
        }

        /// <summary>
        /// 최근 24시간 내 최저 거래금액
        /// </summary>
        public decimal min_price
        {
            get;
            set;
        }

        /// <summary>
        /// 최근 24시간 내 최고 거래금액
        /// </summary>
        public decimal max_price
        {
            get;
            set;
        }

        /// <summary>
        /// 최근 24시간 내 평균 거래금액
        /// </summary>
        public decimal average_price
        {
            get;
            set;
        }

        /// <summary>
        /// 최근 24시간 내 BTC 거래량
        /// </summary>
        public decimal units_traded
        {
            get;
            set;
        }

        /// <summary>
        /// 최근 1일간 BTC 거래량
        /// </summary>
        public decimal volume_1day
        {
            get;
            set;
        }

        /// <summary>
        /// 최근 7일간 BTC 거래량
        /// </summary>
        public decimal volume_7day
        {
            get;
            set;
        }


        /// <summary>
        /// 거래 대기건 최고 구매가
        /// </summary>
        public decimal buy_price
        {
            get;
            set;
        }

        /// <summary>
        /// 거래 대기건 최소 판매가
        /// </summary>
        public decimal sell_price
        {
            get;
            set;
        }

        /// <summary>
        /// 현재 시간 Timestamp
        /// </summary>
        public long date
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Ticker : ApiResult<BTickerItem>
    {
        /// <summary>
        /// 
        /// </summary>
        public Ticker()
        {
            this.result = new BTickerItem();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BTickerAll
    {
        /// <summary>
        /// 
        /// </summary>
        public BTickerAll()
        {
            BTC = new BTickerItem();
            ETH = new BTickerItem();
            XRP = new BTickerItem();
            BCH = new BTickerItem();
            EOS = new BTickerItem();
            LTC = new BTickerItem();
            TRX = new BTickerItem();
            DASH = new BTickerItem();
            XMR = new BTickerItem();
            VNE = new BTickerItem();
            ETC = new BTickerItem();
            ICX = new BTickerItem();
            QTUM = new BTickerItem();
            OMG = new BTickerItem();
            ZEC = new BTickerItem();
            BTG = new BTickerItem();
        }

        /// <summary>
        /// 오미세고
        /// </summary>
        public BTickerItem OMG
        {
            get;
            set;
        }

        /// <summary>
        /// 비체인
        /// </summary>
        public BTickerItem VNE
        {
            get;
            set;
        }

        /// <summary>
        /// Tronix
        /// </summary>
        public BTickerItem TRX
        {
            get;
            set;
        }

        /// <summary>
        /// BitCoin
        /// </summary>
        public BTickerItem BTC
        {
            get;
            set;
        }

        /// <summary>
        /// Ethereum
        /// </summary>
        public BTickerItem ETH
        {
            get;
            set;
        }

        /// <summary>
        /// DashCoin
        /// </summary>
        public BTickerItem DASH
        {
            get;
            set;
        }

        /// <summary>
        /// LiteCoin
        /// </summary>
        public BTickerItem LTC
        {
            get;
            set;
        }

        /// <summary>
        /// Ethereum Classic
        /// </summary>
        public BTickerItem ETC
        {
            get;
            set;
        }

        /// <summary>
        /// Ripple
        /// </summary>
        public BTickerItem XRP
        {
            get;
            set;
        }

        /// <summary>
        /// Bitcoin Cash
        /// </summary>
        public BTickerItem BCH
        {
            get;
            set;
        }

        /// <summary>
        /// Monero
        /// </summary>
        public BTickerItem XMR
        {
            get;
            set;
        }

        /// <summary>
        /// Z-CASH
        /// </summary>
        public BTickerItem ZEC
        {
            get;
            set;
        }

        /// <summary>
        /// Quntum
        /// </summary>
        public BTickerItem QTUM
        {
            get;
            set;
        }

        /// <summary>
        /// Bitcoin Gold
        /// </summary>
        public BTickerItem BTG
        {
            get;
            set;
        }

        /// <summary>
        /// EOS
        /// </summary>
        public BTickerItem EOS
        {
            get;
            set;
        }

        /// <summary>
        /// ICX
        /// </summary>
        public BTickerItem ICX
        {
            get;
            set;
        }

        /// <summary>
        /// 현재 시간 Timestamp
        /// </summary>
        public long date
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class BTickers : ApiResult<BTickerAll>
    {
        /// <summary>
        /// 
        /// </summary>
        public BTickers()
        {
            this.result = new BTickerAll();
        }
    }
}