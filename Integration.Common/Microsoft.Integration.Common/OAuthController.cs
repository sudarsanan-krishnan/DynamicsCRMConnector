//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Azure.AppService.ApiApps.Service;
    using System.Collections.Generic;
    
    /// <summary>
    /// Base ApiController for all OAuth Connectors.
    /// </summary>
    public class OAuthController : ApiController
    {
        /// <summary>
        /// Authorization header constant
        /// </summary>
        protected const string AuthorizationHeader = "Authorization";

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="providerName">ProviderName (e.g. facebook, box, dropbox, etc.) for which the token needs to be retrieved.</param>
        public OAuthController(string providerName)
        {
            this.TokenProvider = new OAuthTokenProvider(providerName);
        }

        /// <summary>
        /// Static OAuthTokenProvider instance
        /// </summary>
        public OAuthTokenProvider TokenProvider { get; private set; }

        /// <summary>
        /// Gets the OAuth AccessToken for the Provider specified.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "By design")]
        public virtual async Task<string> GetTokenAsync()
        {
            string accessToken = await this.TokenProvider.GetTokenAsync(this.Request);
            return accessToken;
        }

        /// <summary>
        /// Gets the OAuth TokenResult object for the Provider specified.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "By design")]
        public async Task<TokenResult> GetTokenResultAsync()
        {
            TokenResult tokenResult = await this.TokenProvider.GetTokenResultAsync(this.Request);

            if (tokenResult == null || tokenResult.Properties == null)
            {
                Logger.LogError(this.Request, false, "Token result or properties returned are null");
                throw new UnauthorizedAccessException(CommonResource.AccessTokenNotFound);
            }

            if(!tokenResult.Properties.ContainsKey(OAuthTokenProvider.AccessToken))
            {
                throw new UnauthorizedAccessException(CommonResource.AccessTokenNotFound);
            }

            return tokenResult;
        }

        /// <summary>
        /// Add OAuth 2.0 Authorization Header to the request 
        /// </summary>
        public virtual async Task AddOAuthHeader(HttpRequestMessage request)
        {
            string accessToken = await this.TokenProvider.GetTokenAsync(this.Request);
            string authHeader = "Bearer " + accessToken;
            request.Headers.Add(AuthorizationHeader, authHeader);
        }

        /// <summary>
        /// Encodes special characters in a string.
        /// </summary>
        /// <param name="originalString">The string to be encoded</param>
        /// <returns>The enocded string.</returns>
        protected static string UrlEncodeSpecialCharacters(string originalString)
        {
            return originalString.Replace("!", "%21").Replace("(", "%28").Replace(")", "%29");
        }
    }
}