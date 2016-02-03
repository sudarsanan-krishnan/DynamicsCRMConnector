//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Azure.AppService.ApiApps.Service;

    /// <summary>
    /// Helper class for validating OAuth access token.
    /// </summary>
    public class OAuthTokenValidator
    {
        /// <summary>
        /// Validates the access token for any OAuth connector.
        /// </summary>
        /// <param name="authController">The connector controller which uses access token based authentication</param>
        /// <param name="connectorName">The name of the connector for which the validation will be done.</param>
        /// <param name="getAccessTokenVerificationRequest">Callback that computes the request object that will be used to do access token verification.</param>
        /// <param name="webResponseProcessor">Callback that processes the web response from 3rd party service.</param>
        /// <returns>A status check record indiciating the outcome of access token check.</returns>
        public static async Task<StatusCheckEntry> ValidateAccessToken(ApiController authController, string connectorName, Func<Task<HttpWebRequest>> getAccessTokenVerificationRequest = null, Func<WebResponse, StatusCheckEntry> webResponseProcessor = null)
        {
            Logger.LogMessage(authController.Request, false, "Retrieving {0} connector status", connectorName);
            if (getAccessTokenVerificationRequest == null)
            {
                throw new ArgumentNullException("getAccessTokenVerificationRequest", "getAccessTokenVerificationRequest cannot be null");
            }

            return await Task.Factory.StartNew(() =>
                {
                    var accessTokenStatusCheck = new StatusCheckEntry();
                    try
                    {
                        // Constructing the outgoing request object
                        HttpWebRequest outgoingRequest = null;
                        Logger.LogMessage(authController.Request, false, "Constructing outgoing request object");
                        outgoingRequest = getAccessTokenVerificationRequest().Result;
                        outgoingRequest.ReadWriteTimeout = -1;
                        outgoingRequest.Timeout = -1;
                        outgoingRequest.KeepAlive = false;
                        Logger.LogMessage(authController.Request, false, "Calling {0} API with Request URI: {1}", connectorName, outgoingRequest.RequestUri.ToString());

                        // Make the call and check the response code
                        WebResponse webResponse = outgoingRequest.GetResponse();
                        HttpStatusCode statusCode = ((HttpWebResponse)webResponse).StatusCode;
                        Logger.LogMessage(authController.Request, false, "Status returning , {0}", statusCode);

                        if (webResponseProcessor != null)
                        {
                            accessTokenStatusCheck = webResponseProcessor(webResponse);
                        }
                        else
                        {
                            accessTokenStatusCheck = ProcessStatusCheckResponse(authController, webResponse);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Equals("The remote server returned an error: (401) Unauthorized."))
                        {
                            Logger.LogMessage(authController.Request, ex.Message);
                            accessTokenStatusCheck = GetInvalidAccessTokenStatusCheckEntry();
                        }
                        else
                        {
                            // Log the exception stack
                            Logger.LogException(authController.Request, ex);

                            // Construct a status check entry mentioning that status check failed
                            accessTokenStatusCheck.Level = StatusLevel.Error;
                            accessTokenStatusCheck.Message = string.Format(CommonResource.StatusCheckFailed, connectorName);
                            accessTokenStatusCheck.Name = StatusMessages.StatusCheckFailed;
                        }
                    }

                    return accessTokenStatusCheck;
                }
            );
        }

        /// <summary>
        /// Checks whether request headers have zumo token or not.
        /// </summary>
        /// <param name="request">The request object</param>
        /// <returns>Whether or not zumo token is present in header or not.</returns>
        public static bool DoesRequestTokenHaveZumoToken(HttpRequestMessage request)
        {
            bool hasZumoTokenHeader = false;
            if (request != null)
            {
                // Check if the request headers contains zumo auth token or not. If not, throw an exception indicating the same.
                // This particular check is required because when we try to get the emaUser using the following lines of code:
                //            var runtime = Runtime.FromAppSettings(request);
                //            var emaUser = runtime.CurrentUser;
                // we will get emaUser as null, when request does not contain a zumo token header and / or the zumo token is invalid.
                foreach (var header in request.Headers)
                {
                    if (header.Key == Runtime.XZumoAuthHeader
                        && header.Value != null
                        && !string.IsNullOrWhiteSpace(header.Value.FirstOrDefault()))
                    {
                        hasZumoTokenHeader = true;
                    }
                }
            }

            return hasZumoTokenHeader;
        }

        /// <summary>
        /// Constructs a status check entry for invalid access token.
        /// </summary>
        /// <returns>Status check entry</returns>
        public static StatusCheckEntry GetInvalidAccessTokenStatusCheckEntry()
        {
            StatusCheckEntry statusCheckEntry = new StatusCheckEntry()
            {
                Level = StatusLevel.Error,
                Message = CommonResource.AccessTokenInvalid,
                Name = StatusMessages.AccessTokenInvalid
            };

            return statusCheckEntry;
        }

        private static StatusCheckEntry ProcessStatusCheckResponse(ApiController apiController, WebResponse webResponse)
        {
            StatusCheckEntry statusCheckEntry = new StatusCheckEntry();
            HttpStatusCode statusCode = ((HttpWebResponse)webResponse).StatusCode;
            if (statusCode == HttpStatusCode.OK)
            {
                // Everything good. Return valid status.
                statusCheckEntry.Level = StatusLevel.Info;
                statusCheckEntry.Message = CommonResource.AccessTokenValid;
                statusCheckEntry.Name = StatusMessages.AccessTokenValid;
            }
            else
            {
                // Log the cause of failure, if available
                using (var reader = new StreamReader(webResponse.GetResponseStream(), ASCIIEncoding.ASCII))
                {
                    Logger.LogMessage(apiController.Request, false, "Verify call failed: " + reader.ReadToEnd());
                }
                statusCheckEntry = GetInvalidAccessTokenStatusCheckEntry();
            }

            return statusCheckEntry;
        }
    }
}