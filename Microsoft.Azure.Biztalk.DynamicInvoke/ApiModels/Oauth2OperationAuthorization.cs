//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System;
    using System.Net.Http;

    /// <summary>
    /// Class encapsulating OAuth authentication.
    /// Expects the oauth bearer token to be available
    /// in the inputs at a well-known key.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Oauth", Justification = "parameter name is correct")]
    public class Oauth2OperationAuthorization : OperationAuthorization
    {
        /// <summary>
        /// Key in inputs to store the bearer token under.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth", Justification = "parameter name is correct")]
        public const string OAuthToken = "$OAuthBearerToken";

        public override string AuthType
        {
            get
            { 
                return "oauth2";
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OAuth", Justification = "parameter name is correct")]
        public override void Authenticate(HttpRequestMessage request, object propertyValues)
        {
            if (request != null)
            {
                var propertyBag = PropertyBag.Create(propertyValues);
                if (!propertyBag.ContainsKey(OAuthToken))
                {
                    throw new InvalidOperationException("Operation requires OAuth token, but not provided");
                }

                string token = propertyBag[OAuthToken].ToString();
                request.Headers.Add("Authorization", "Bearer " + token);
            }
        }
    }
}
