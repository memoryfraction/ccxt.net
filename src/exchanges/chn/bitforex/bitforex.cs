﻿using CCXT.NET.Shared.Coin;
using CCXT.NET.Shared.Extension;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CCXT.NET.Bitforex
{
    /// <summary>
    ///
    /// </summary>
    public sealed class BitforexClient : CCXT.NET.Shared.Coin.XApiClient, IXApiClient
    {
        /// <summary>
        ///
        /// </summary>
        public override string DealerName { get; set; } = "Bitforex";

        /// <summary>
        ///
        /// </summary>
        /// <param name="division">exchange's division for communication</param>
        public BitforexClient(string division)
            : base(division)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="division">exchange's division for communication</param>
        /// <param name="connect_key">exchange's api key for connect</param>
        /// <param name="secret_key">exchange's secret key for signature</param>
        public BitforexClient(string division, string connect_key, string secret_key)
            : base(division, connect_key, secret_key, authentication: true)
        {
        }

        /// <summary>
        /// information of exchange for trading
        /// </summary>
        public override ExchangeInfo ExchangeInfo
        {
            get
            {
                if (base.ExchangeInfo == null)
                {
                    base.ExchangeInfo = new ExchangeInfo(this.DealerName)
                    {
                        Countries = new List<string>
                        {
                            "CN"
                        },
                        Urls = new ExchangeUrls
                        {
                            logo = "",
                            api = new Dictionary<string, string>
                            {
                                { "public", "https://api.bitforex.com" },
                                { "private", "https://api.bitforex.com" },
                                { "trade", "https://api.bitforex.com" }
                            },
                            www = "https://bitforex.com/",
                            doc = new List<string>
                            {
                                "https://github.com/bitforexapi/API_Doc_en/wiki"
                            }
                        },
                        RequiredCredentials = new RequiredCredentials
                        {
                            apikey = true,
                            secret = true,
                            uid = false,
                            login = false,
                            password = false,
                            twofa = false
                        },
                        LimitRate = new ExchangeLimitRate
                        {
                            useTotal = false,
                            token = new ExchangeLimitCalled { rate = 60000 },
                            @public = new ExchangeLimitCalled { rate = 1000 },
                            @private = new ExchangeLimitCalled { rate = 1000 },
                            trade = new ExchangeLimitCalled { rate = 1000 },
                            total = new ExchangeLimitCalled { rate = 1000 }     // up to 3000 requests per 5 minutes ≈ 600 requests per minute ≈ 10 requests per second ≈ 100 ms
                        },
                        Fees = new MarketFees
                        {
                            trading = new MarketFee
                            {
                                tierBased = false,      // true for tier-based/progressive
                                percentage = false,     // fixed commission

                                maker = 0.2m / 100m,
                                taker = 0.2m / 100m
                            }
                        },
                        Timeframes = new Dictionary<string, string>
                        {
                            { "1m","60" },
                            { "3m","180" },
                            { "5m","300" },
                            { "15m","900" },
                            { "30m","1800" },
                            { "1h","3600" },
                            { "2h","7200" },
                            { "4h","14400" },
                            { "6h","21600" },
                            { "12h","43200" },
                            { "1d","86400" },
                            { "3d","259200" },
                            { "1w","604800" }
                        }
                    };
                }

                return base.ExchangeInfo;
            }
        }

        private HMACSHA256 __encryptor = null;

        /// <summary>
        ///
        /// </summary>
        public HMACSHA256 Encryptor
        {
            get
            {
                if (__encryptor == null)
                    __encryptor = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));

                return __encryptor;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="endpoint">api link address of a function</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<RestRequest> CreatePostRequestAsync(string endpoint, Dictionary<string, object> args = null)
        {
            var _request = await base.CreatePostRequestAsync(endpoint, args);

            if (IsAuthentication == true)
            {
                var _params = new Dictionary<string, object>();
                {
                    foreach (var _param in _request.Parameters)
                        _params.Add(_param.Name, _param.Value);

                    _request.ParametersClear();
                }

                var _nonce = GenerateOnlyNonce(13).ToString();

                _params.Add("nonce", _nonce);
                _params.Add("accessKey", ConnectKey);

                var _post_data = ToQueryString(_params);
                {
                    // Sort params (sort by first letter, first letter is same as the second letter
                    _post_data = String.Join("&", new SortedSet<string>(_post_data.Split('&'), StringComparer.Ordinal));

                    // Signed Content(content)
                    var _message = endpoint + $"?{_post_data}";

                    // For signing keys(secretKey)
                    var _sign_data = this.ConvertHexString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_message)));

                    _post_data += $"&signData={_sign_data.ToLower()}";
                }

                _request.Resource = endpoint + $"?{_post_data}";
            }

            return await Task.FromResult(_request);
        }

        /// <summary>
        ///
        /// </summary>
        public new Dictionary<int, string> ErrorMessages = new Dictionary<int, string>
        {
            { 1000, "System Error" },
            { 1001, "No right to Access" },
            { 1002, "Record does not exist" },
            { 1003, "Parameter error" },
            { 1010, "Api interface prohibits access" },
            { 1011, "Missing authentication parameters" },
            { 1012, "Nonce does not exist or is not correct" },
            { 1013, "accessKey does not exist" },
            { 1014, "User is locked" },
            { 1015, "Request too often" },
            { 1016, "Signing information is wrong" },
            { 1017, "IP is not allowed" },
            { 1018, "AccessKey needs to bind IP" },
            { 1019, "The type of transaction is wrong" },
            { 1020, "K line type does not exist" },
            { 1021, "The currency type is wrong" },
            { 3001, "Account freeze" },
            { 3002, "Insufficient funds available" },
            { 4001, "Prohibit orders" },
            { 4002, "Unreasonable unit price" },
            { 4003, "Order quantity is less than the minimum order limit" },
            { 4004, "Order does not exist or does not belong to the user" },
        };

        /// <summary>
        ///
        /// </summary>
        /// <param name="response">response value arrive from exchange's server</param>
        /// <returns></returns>
        public override BoolResult GetResponseMessage(RestResponse response = null)
        {
            var _result = new BoolResult();

            if (response != null)
            {
                if (response.IsSuccessful == true)
                {
                    var _json_result = this.DeserializeObject<JToken>(response.Content);

                    var _json_error = _json_result.SelectToken("code");
                    if (_json_error != null)
                    {
                        var _error_code = _json_error.Value<int>();
                        if (_error_code != 0)
                        {
                            _result.SetFailure(
                                      GetErrorMessage(_error_code),
                                      ErrorCode.ResponseDataError,
                                      _error_code
                                  );
                        }
                    }
                }

                if (_result.success == true && response.IsSuccessful == false)
                {
                    _result.SetFailure(
                            response.ErrorMessage ?? response.StatusDescription,
                            ErrorCode.ResponseRestError,
                            (int)response.StatusCode,
                            false
                        );
                }
            }

            return _result;
        }
    }
}