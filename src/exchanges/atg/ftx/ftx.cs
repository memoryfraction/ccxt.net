using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CCXT.NET.Shared.Coin;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace CCXT.NET.Ftx
{
    /// <summary>
    ///
    /// </summary>
    public sealed class FtxClient : CCXT.NET.Shared.Coin.XApiClient, IXApiClient
    {
        /// <summary>
        ///
        /// </summary>
        public override string DealerName { get; set; } = "Ftx";

        /// <summary>
        ///
        /// </summary>
        /// <param name="division">exchange's division for communication</param>
        public FtxClient(string division)
            : base(division)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="division">exchange's division for communication</param>
        /// <param name="connect_key">exchange's api key for connect</param>
        /// <param name="secret_key">exchange's secret key for signature</param>
        public FtxClient(string division, string connect_key, string secret_key)
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
                            "ATG" // Antigua and Barbuda
                        },
                        Urls = new ExchangeUrls
                        {
                            logo = "https://user-images.githubusercontent.com/1294454/67149189-df896480-f2b0-11e9-8816-41593e17f9ec.jpg",
                            api = new Dictionary<string, string>
                            {
                                { "public", "https://ftx.com" },
                                { "private", "https://ftx.com" }
                            },
                            www = "https://ftx.com",
                            doc = new List<string>
                            {
                                "https://github.com/ftexchange/ftx"
                            },
                            fees = new List<string>
                            {
                                "https://ftexchange.zendesk.com/hc/en-us/articles/360024479432-Fees"
                            }
                        },
                        AmountMultiplier = new Dictionary<string, decimal>
                        {
                        },
                        RequiredCredentials = new RequiredCredentials
                        {
                            apikey = true,
                            secret = true,
                            uid = false,
                            login = false,
                            password = false,
                            twofa = true
                        },
                        LimitRate = new ExchangeLimitRate
                        {
                            useTotal = true,
                            token = new ExchangeLimitCalled { rate = 60000 },           // 100 request per minute
                            @public = new ExchangeLimitCalled { rate = 2000 },
                            @private = new ExchangeLimitCalled { rate = 2000 },
                            trade = new ExchangeLimitCalled { rate = 2000 },
                            total = new ExchangeLimitCalled { rate = 2000 }
                        },
                        Fees = new MarketFees
                        {
                            trading = new MarketFee
                            {
                                tierBased = false,          // true for tier-based/progressive
                                percentage = false,         // fixed commission

                                maker = 0.001m,    
                                taker = 0.001m
                            }
                        },
                        Timeframes = new Dictionary<string, string>
                        {
                            { "15s","15"},
                            { "1m","60"},
                            { "5m","300"},
                            { "15m","900"},
                            { "1h","3600"},
                            { "4h","14400"},
                            { "1d","86400"},
                            { "3d","259200"},
                            { "1w","604800"},
                            { "2w","1209600"},
                            { "1M","2592000"}
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
        public override async ValueTask<IRestRequest> CreatePostRequestAsync(string endpoint, Dictionary<string, object> args = null)
        {
            var _request = await base.CreatePostRequestAsync(endpoint);

            if (IsAuthentication == true)
            {
                var _nonce = (GenerateOnlyNonce(10) + 3600).ToString();

                var _json_body = "";
                if (args != null && args.Count > 0)
                {
                    _json_body = Regex.Unescape(this.SerializeObject(args, Formatting.None));
                    _request.AddParameter("application/json", _json_body, ParameterType.RequestBody);
                }

                var _signature = await CreateSignature(_request.Method, endpoint, _nonce, _json_body);
                {
                    _request.AddHeader("FTX-TS", _nonce);
                    _request.AddHeader("FTX-KEY", ConnectKey);
                    _request.AddHeader("FTX-SIGN", _signature);
                }
            }

            return await Task.FromResult(_request);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<IRestRequest> CreatePutRequestAsync(string endpoint, Dictionary<string, object> args = null)
        {
            var _request = await base.CreatePutRequestAsync(endpoint);

            if (IsAuthentication == true)
            {
                var _nonce = (GenerateOnlyNonce(10) + 3600).ToString();

                var _json_body = "";
                if (args != null && args.Count > 0)
                {
                    _json_body = Regex.Unescape(this.SerializeObject(args, Formatting.None));
                    _request.AddParameter("application/json", _json_body, ParameterType.RequestBody);
                }

                var _signature = await CreateSignature(_request.Method, endpoint, _nonce, _json_body);
                {
                    _request.AddHeader("FTX-TS", _nonce);
                    _request.AddHeader("FTX-KEY", ConnectKey);
                    _request.AddHeader("FTX-SIGN", _signature);
                }
            }

            return await Task.FromResult(_request);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="endpoint">api link address of a function</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<IRestRequest> CreateGetRequestAsync(string endpoint, Dictionary<string, object> args = null)
        {
            var _request = await base.CreateGetRequestAsync(endpoint, args);

            if (IsAuthentication == true)
            {
                var _post_params = _request.Parameters.ToDictionary(p => p.Name, p => p.Value);

                var _post_data = ToQueryString(_post_params);

                var _nonce = (GenerateOnlyNonce(10) + 3600).ToString();
                if (args != null && args.Count > 0)
                    endpoint += $"?{_post_data}";

                var _signature = await CreateSignature(_request.Method, endpoint, _nonce);
                {
                    _request.AddHeader("FTX-TS", _nonce);
                    _request.AddHeader("FTX-KEY", ConnectKey);
                    _request.AddHeader("FTX-SIGN", _signature);
                }
            }

            return await Task.FromResult(_request);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="endpoint">api link address of a function</param>
        /// <param name="args">Add additional attributes for each exchange</param>
        /// <returns></returns>
        public override async ValueTask<IRestRequest> CreateDeleteRequestAsync(string endpoint, Dictionary<string, object> args = null)
        {
            var _request = await base.CreateDeleteRequestAsync(endpoint);

            if (IsAuthentication == true)
            {
                var _nonce = (GenerateOnlyNonce(10) + 3600).ToString();

                var _json_body = "";
                if (args != null && args.Count > 0)
                {
                    _json_body = Regex.Unescape(this.SerializeObject(args, Formatting.None));
                    _request.AddParameter("application/json", _json_body, ParameterType.RequestBody);
                }

                var _signature = await CreateSignature(_request.Method, endpoint, _nonce, _json_body);
                {
                    _request.AddHeader("FTX-TS", _nonce);
                    _request.AddHeader("FTX-KEY", ConnectKey);
                    _request.AddHeader("FTX-SIGN", _signature);
                }
            }

            return await Task.FromResult(_request);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public async ValueTask<string> CreateSignature(Method verb, string endpoint, string nonce, string json_body = "")
        {
            return await Task.FromResult
                    (
                        this.ConvertHexString
                        (
                            Encryptor.ComputeHash
                            (
                                Encoding.UTF8.GetBytes($"{nonce}{verb}{endpoint}{json_body}")
                            )
                        )
                        .ToLower()
                    );
        }

        /// <summary>
        ///
        /// </summary>
        public new Dictionary<string, ErrorCode> ErrorMessages = new Dictionary<string, ErrorCode>
        {
            { "Please slow down", ErrorCode.RateLimitExceeded },  // {"error":"Please slow down" },"success":false}
            { "Size too small for provide", ErrorCode.InvalidOrder },  // {"error":"Size too small for provide" },"success":false}
            { "Not logged in", ErrorCode.AuthenticationError },  // {"error":"Not logged in" },"success":false}
            { "Not enough balances", ErrorCode.InsufficientFunds },  // {"error":"Not enough balances" },"success":false}
            { "InvalidPrice", ErrorCode.InvalidOrder },  // {"error":"Invalid price" },"success":false}
            { "Size too small", ErrorCode.InvalidOrder },  // {"error":"Size too small" },"success":false}
            { "Size too large", ErrorCode.InvalidOrder },  // {"error":"Size too large" },"success":false}
            { "Missing parameter price", ErrorCode.InvalidOrder },  // {"error":"Missing parameter price" },"success":false}
            { "Order not found", ErrorCode.OrderNotFound },  // {"error":"Order not found" },"success":false}
            { "Order already closed", ErrorCode.InvalidOrder },  // {"error":"Order already closed" },"success":false}
            { "Trigger price too high", ErrorCode.InvalidOrder },  // {"error":"Trigger price too high" },"success":false}
            { "Trigger price too low", ErrorCode.InvalidOrder },  // {"error":"Trigger price too low" },"success":false}
            { "Order already queued for cancellation", ErrorCode.CancelPending },  // {"error":"Order already queued for cancellation" },"success":false}
            { "Duplicate client order ID", ErrorCode.DuplicateOrderId },  // {"error":"Duplicate client order ID" },"success":false}
            { "Spot orders cannot be reduce-only", ErrorCode.InvalidOrder },  // {"error":"Spot orders cannot be reduce-only" },"success":false}
            { "Invalid reduce-only order", ErrorCode.InvalidOrder },  // {"error":"Invalid reduce-only order" },"success":false}
            { "Account does not have enough balances", ErrorCode.InsufficientFunds },  // {"success":false },"error":"Account does not have enough balances"}
            { "Not authorized for subaccount-specific access", ErrorCode.PermissionDenied }  // {"success":false },"error":"Not authorized for subaccount-specific access"}
        };

        /// <summary>
        ///
        /// </summary>
        /// <param name="response">response value arrive from exchange's server</param>
        /// <returns></returns>
        public override BoolResult GetResponseMessage(IRestResponse response = null)
        {
            var _result = new BoolResult();

            if (response != null)
            {
                if (response.IsSuccessful == false) 
                {
                    if ((int)response.StatusCode != 429)
                    {
                        if (String.IsNullOrEmpty(response.Content) == false && response.Content[0] == '{')
                        {
                            var _json_result = this.DeserializeObject<JToken>(response.Content);

                            var _json_error = _json_result.SelectToken("error");
                            if (_json_error != null)
                            {
                                var _error_code = ErrorCode.ExchangeError;

                                var _error_msg = _json_error.Value<string>();
                                if (String.IsNullOrEmpty(_error_msg) == false)
                                {
                                    if (ErrorMessages.ContainsKey(_error_msg) == true)
                                        _error_code = ErrorMessages[_error_msg];
                                }
                                else
                                {
                                    _error_msg = response.Content;
                                }

                                _result.SetFailure(_error_msg, _error_code);
                            }
                        }
                    }
                    else
                    {
                        _result.SetFailure(
                                response.ErrorMessage ?? response.StatusDescription,
                                ErrorCode.DDoSProtection,
                                (int)response.StatusCode,
                                false
                            );
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