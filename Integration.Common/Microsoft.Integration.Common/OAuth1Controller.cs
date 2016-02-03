//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.AppService.ApiApps.Service;
    using Newtonsoft.Json;

    /// <summary>
    /// Base ApiController for all OAuth 1.0 Connectors.
    /// </summary>
    public class OAuth1Controller : OAuthController
    {
        private const string ConsumerKeyField = "oauth_consumer_key";
        private const string NonceField = "oauth_nonce";
        private const string SignatureMethodField = "oauth_signature_method";
        private const string TimestampField = "oauth_timestamp";
        private const string VersionField = "oauth_version";
        private const string TokenField = "oauth_token";
        private const string Version = "1.0";
        private const string HashAlgorithm = "HMAC-SHA1";

        private const string AuthHeaderFormatOAuth1 = "OAuth oauth_token=\"{0}\", oauth_nonce=\"{1}\", oauth_consumer_key=\"{2}\", oauth_signature_method=\"{3}\", " +
               "oauth_timestamp=\"{4}\", oauth_version=\"{5}\", oauth_signature=\"{6}\"";

        private static readonly DateTime epochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private TokenResult tokenResult;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="providerName">ProviderName (e.g. quickbooks, twitter, etc.) for which the token needs to be retrieved.</param>
        public OAuth1Controller(string providerName)
            : base(providerName)
        {
        }

        /// <summary>
        /// Add OAuth 1.0 Authorization Header to the request 
        /// </summary>
        public override async Task AddOAuthHeader(HttpRequestMessage request)
        {
            // Try to get token result from request. If not found make call to Gateway to get the token.
            if (!this.TryGetTokenResultFromHeader(out tokenResult))
            {
                this.tokenResult = await this.GetTokenResultAsync();
            }

            if(this.tokenResult == null || this.tokenResult.Properties == null)
            {
                Logger.LogError(request, false, "Token result or properties returned are null");
                throw new UnauthorizedAccessException(CommonResource.AccessTokenNotFound);
            }

            if (!this.tokenResult.Properties.ContainsKey("AccessToken"))
            {
                Logger.LogError(request, false, "Couldn't find AccessToken in OAuth TokenResult.");
                throw new UnauthorizedAccessException(CommonResource.AccessTokenNotFound);
            }

            if (!this.tokenResult.Properties.ContainsKey("ConsumerKey"))
            {
                Logger.LogError(request, false, "Couldn't find ConsumerKey in OAuth TokenResult.");
                throw new UnauthorizedAccessException(CommonResource.AccessTokenInvalid);
            }

            if (!this.tokenResult.Properties.ContainsKey("AccessTokenSecret"))
            {
                Logger.LogError(request, false, "Couldn't find AccessTokenSecret in OAuth TokenResult.");
                throw new UnauthorizedAccessException(CommonResource.AccessTokenInvalid);
            }

            Dictionary<string, string> arguments = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in request.GetQueryNameValuePairs())
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    arguments.Add(pair.Key, pair.Value);
                }
            }

            Uri baseUrl = new Uri(request.RequestUri.GetLeftPart(UriPartial.Path));

            string nonce = GenerateNonce();
            string timestamp = GenerateTimestamp();
            string signature = await this.GenerateSignature(request.Method, baseUrl, nonce, timestamp, arguments);

            string authHeader = string.Format(CultureInfo.InvariantCulture,
                                            AuthHeaderFormatOAuth1,
                                            Uri.EscapeDataString(this.tokenResult.Properties["AccessToken"]),
                                            Uri.EscapeDataString(nonce),
                                            Uri.EscapeDataString(this.tokenResult.Properties["ConsumerKey"]),
                                            Uri.EscapeDataString(HashAlgorithm),
                                            Uri.EscapeDataString(timestamp),
                                            Uri.EscapeDataString(Version),
                                            Uri.EscapeDataString(signature));

            request.Headers.Add(AuthorizationHeader, authHeader);
        }

        private static string GenerateNonce()
        {
            return Guid.NewGuid().ToString();
        }

        private static string GenerateTimestamp()
        {
            var timeSpan = DateTime.UtcNow - epochTime;
            return Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        private async Task<string> GenerateSignature(HttpMethod requestMethod, Uri baseUrl, string nonce, string timestamp, Dictionary<string, string> requestParameters)
        {
            string signatureBase = this.GenerateSignatureBase(requestMethod, baseUrl, nonce, timestamp, requestParameters);
            return await this.GenerateSignature(signatureBase);
        }

        private async Task<string> GenerateSignature(string signatureBase)
        {
            string signature = string.Empty;
            if (this.TokenProvider.IsSelfHosted)
            {
                string signingKey = this.GenerateSigningKey();
                signature = ComputeHash(signingKey, signatureBase);
            }
            else
            {
                var signatureContents = await Runtime.SignOAuth1Async(
                    this.tokenResult,
                    ASCIIEncoding.ASCII.GetBytes(signatureBase));

                signature = Convert.ToBase64String(signatureContents);
            }

            return signature;
        }

        private string GenerateSignatureBase(HttpMethod requestMethod, Uri baseUrl, string nonce, string timestamp, Dictionary<string, string> requestParameters)
        {
            SortedDictionary<string, string> signatureBaseParameters = new SortedDictionary<string, string>();

            // populate the parameter collection dictionary with additional oauth_* parameters
            signatureBaseParameters.Add(ConsumerKeyField, Uri.EscapeDataString(this.tokenResult.Properties["ConsumerKey"]));
            signatureBaseParameters.Add(NonceField, Uri.EscapeDataString(nonce));
            signatureBaseParameters.Add(SignatureMethodField, Uri.EscapeDataString(HashAlgorithm));
            signatureBaseParameters.Add(TimestampField, Uri.EscapeDataString(timestamp));
            signatureBaseParameters.Add(VersionField, Uri.EscapeDataString(Version));
            signatureBaseParameters.Add(TokenField, Uri.EscapeDataString(this.tokenResult.Properties["AccessToken"]));

            // url-encode the keys and the values of the request parameters
            foreach (var parameter in requestParameters)
            {
                signatureBaseParameters.Add(Uri.EscapeDataString(parameter.Key), Uri.EscapeDataString(parameter.Value));
            }

            // generate the parameterString by iterating over the sorted dictionary and adding the string
            // 'key=value' concatenated with '&'
            var parameterArray = signatureBaseParameters.Select(param => string.Format(CultureInfo.InvariantCulture, "{0}={1}", param.Key, param.Value));
            string parameterString = string.Join("&", parameterArray);

            return string.Format(CultureInfo.InvariantCulture,
                                "{0}&{1}&{2}",
                                Uri.EscapeDataString(requestMethod.ToString().ToUpperInvariant()),
                                Uri.EscapeDataString(baseUrl.ToString()),
                                Uri.EscapeDataString(parameterString));
        }

        private string GenerateSigningKey()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}&{1}",
                Uri.EscapeDataString(this.tokenResult.Properties["ConsumerSecret"]),
                Uri.EscapeDataString(this.tokenResult.Properties["AccessTokenSecret"]));
        }

        private static string ComputeHash(string signingKey, string signatureBase)
        {
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(signingKey)))
            {
                return Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes(signatureBase)));
            }
        }

        private bool TryGetTokenResultFromHeader(out TokenResult result)
        {
            IEnumerable<string> values;
            if (this.Request.Headers.TryGetValues(CommonConstants.tokenResultHeaderName, out values))
            {
                try
                {
                    result = JsonConvert.DeserializeObject<TokenResult>(values.First());
                    return true;
                }
                catch (JsonException)
                {
                    // Igonre the serialization exception. Caller will try to get the token by making a call to gateway directly 
                }
            }

            result = null;
            return false;
        }
    }
}