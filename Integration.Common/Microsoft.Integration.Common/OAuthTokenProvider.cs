//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Azure.AppService.ApiApps.Service;

    /// <summary>
    /// Class which retrieves the AccessToken for the given provider
    /// </summary>
    public class OAuthTokenProvider
    {
        /// <summary>
        /// AccessToken field in TokenResult.
        /// </summary>
        public static readonly string AccessToken = "AccessToken";

        /// <summary>
        /// AppSetting name to be set to true if the Microservice is running under self-hosted context
        /// </summary>
        public const string SelfHosted = "SelfHosted";

        /// <summary>
        /// provider for which the AccessToken will be retrieved
        /// </summary>
        private readonly string providerName;

        /// <summary>
        /// bool to indicate whether the token should be retrieved from ZumoAuthHeader
        /// or from TokenStore
        /// </summary>
        private readonly bool isSelfHosted;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="providerName"></param>
        public OAuthTokenProvider(string providerName)
        {
            this.providerName = providerName;
            this.isSelfHosted = GetBoolValue(SelfHosted, false);
        }

        /// <summary>
        /// bool to indicate whether the token should be retrieved from ZumoAuthHeader
        /// or from TokenStore
        /// </summary>
        public bool IsSelfHosted
        {
            get { return this.isSelfHosted; }
        }

        /// <summary>
        /// Gets the OAuth Access Token from the token store or from
        /// ZumoAuthHeader based on the environment context of the Microservice (self-hosted or not)
        /// </summary>
        /// <param name="request"></param>
        public async Task<TokenResult> GetTokenResultAsync(HttpRequestMessage request)
        {
            Trace.WriteLine("Start: GetTokenResultAsync");

            if (this.isSelfHosted)
            {
                Logger.LogMessage(
                    request,
                    false,
                    "Self-Hosted Environment. Returning AccessToken which is set in ZumoAuthHeader. HeaderName : {0}",
                    Runtime.XZumoAuthHeader);
                return GetTokenResult(request.Headers);
            }


            var runtime = Runtime.FromAppSettings(request);
            var emaUser = runtime.CurrentUser;

            if (emaUser == null)
            {
                Logger.LogError(request, false, "CurrentUser not set in RuntimeSDK. Hence, returning token as null");
                return null;
            }

            Logger.LogMessage(request, false, "Fetching Token using EmaUserInfo.GetRawTokenAsync({0})", this.providerName);
            TokenResult tokenResult = await emaUser.GetRawTokenAsync(this.providerName);

            Logger.LogMessage(request, false, "GetTokenResultAsync: Token Diagnostics String: '{0}'", tokenResult.Diagnostics);

            if (tokenResult == null || tokenResult.Properties == null)
            {
                Logger.LogError(request, false, "Token result or properties returned are null");
                throw new UnauthorizedAccessException(CommonResource.AccessTokenNotFound);
            }

            return tokenResult;
        }

        /// <summary>
        /// Gets the OAuth Access Token from the token store or from
        /// ZumoAuthHeader based on the environment context of the Microservice (self-hosted or not) 
        /// </summary>
        /// <param name="request"></param>
        public async Task<string> GetTokenAsync(HttpRequestMessage request)
        {
            Logger.LogMessage(request, "Start: GetTokenAsync");
            TokenResult tokenResult = await this.GetTokenResultAsync(request);

            if (tokenResult == null || tokenResult.Properties == null)
            {
                Logger.LogError(request, false, "Token result or properties returned are null. Could not retrive OAuth token from token store");
                throw new UnauthorizedAccessException(CommonResource.AccessTokenNotFound);
            }

            if (tokenResult.Properties.ContainsKey(AccessToken))
            {
                return tokenResult.Properties[AccessToken];
            }
            else
            {
                throw new UnauthorizedAccessException(CommonResource.AccessTokenNotFound);
            }
        }

        /// <summary>
        /// Sets given TokenResult into TokenStore
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tokenResult"></param>
        /// <returns></returns>
        public async Task SetTokenAsync(HttpRequestMessage request, TokenResult tokenResult)
        {
            if (this.isSelfHosted)
            {
                return;
            }

            var runtime = Runtime.FromAppSettings(request);
            var emaUser = runtime.CurrentUser;
            await emaUser.SetTokenAsync(this.providerName, tokenResult);
        }

        private static bool GetBoolValue(string key, bool? defaultValue = null)
        {
            string value = ConfigurationManager.AppSettings.Get(key);

            bool result; 
            if (String.IsNullOrEmpty(value) || !bool.TryParse(value, out result))
            {
                if (defaultValue != null)
                {
                    return defaultValue.Value;
                }

                return false;
            }

            return result;
        }

        private static TokenResult GetTokenResult(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        {
            foreach (var header in headers)
            {
                if (header.Key == Runtime.XZumoAuthHeader
                    && header.Value != null
                    && !string.IsNullOrWhiteSpace(header.Value.FirstOrDefault()))
                {
                    string[] splitArray = header.Value.FirstOrDefault().Split(',');
                    TokenResult tokenResult = new TokenResult();

                    for (int i = 0; i < splitArray.Length; i++)
                    {
                        string[] keyValuePairSegments = splitArray[i].Trim().Split('=');
                        if (keyValuePairSegments.Count() == 2 && !string.IsNullOrWhiteSpace(keyValuePairSegments[0]) && !string.IsNullOrWhiteSpace(keyValuePairSegments[1]))
                        {
                            tokenResult.Properties[keyValuePairSegments[0]] = keyValuePairSegments[1];
                        }
                    }

                    return tokenResult;
                }
            }

            return null;
        }
    }
}
