//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.AppService.ApiApps.Service;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// HttpHelpers
    /// </summary>
    public static class HttpHelpers
    {
        /// <summary>
        /// Helper method to create ErrorRespone
        /// </summary>
        /// <param name="request">This should be the source request. For Eg: </param>
        /// <param name="body"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, ErrorResponseBody body)
        {
            var jsonFormatter = new JsonMediaTypeFormatter();
            jsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            return request.CreateResponse<ErrorResponseBody>(body.Status, body, jsonFormatter);
        }

        /// <summary>
        /// Helper method to create ErrorRespone
        /// </summary>
        /// <param name="request"></param>
        /// <param name="statusCode"></param>
        /// <param name="message"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateErrorResponse(HttpRequestMessage request, HttpStatusCode statusCode, string message, List<string> errors = null)
        {
            var body = new ErrorResponseBody()
            {
                Status = statusCode,
                Message = message,
                Source = request.RequestUri.Host,
                Errors = errors
            };

            return HttpHelpers.CreateErrorResponse(request, body);
        }

        /// <summary>
        /// Creates a response message from Exception
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateResponseMessageFromException(Exception exception, HttpRequestMessage request)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            string json = JsonConvert.SerializeObject(JObject.FromObject(GetErrorBodyFromException(exception, request)));
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            response.RequestMessage = request;

            return response;
        }

        /// <summary>
        /// Helper method to create an error response for a particular request from the body of another response
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="responseReader"></param>
        /// <returns></returns>
        public async static Task<HttpResponseMessage> CreateErrorResponse(HttpRequestMessage request, HttpResponseMessage response, Func<JObject, string> responseReader)
        {
            var body = await GetErrorBodyFromErrorReponse(response, responseReader);
            Logger.LogWarning(request, false, JsonConvert.SerializeObject(body));
            return HttpHelpers.CreateErrorResponse(request, body);
        } 

        /// <summary>
        /// Helper Method to Create ErrorResponseBody from the HttpResponseMessage
        /// </summary>
        /// <param name="response"></param>
        /// <param name="responseReader">Should read the body of HttpResponseMessage and return the error message in string format</param>
        /// <returns></returns>
        public async static Task<ErrorResponseBody> GetErrorBodyFromErrorReponse(HttpResponseMessage response, Func<JObject, string> responseReader)
        {
            if (response == null)
            {
                throw  new ArgumentNullException("response");
            }

            if (responseReader == null)
            {
                throw new ArgumentNullException("responseReader");
            }

            string bodyString = await response.Content.ReadAsStringAsync();
            var body = JObject.Parse(bodyString);

            ErrorResponseBody errorResponse =  body.ToObject<ErrorResponseBody>();
            
            // checking if this is already properly formatted message . If no format it now. else return the message
            if (errorResponse == null || errorResponse.Message == null || errorResponse.Status == 0)
            {
                return new ErrorResponseBody()
                {
                    Status = response.StatusCode,
                    Message = responseReader(body),
                    Source = response.RequestMessage.RequestUri.Host
                };
            }

            return errorResponse;
        }

        private static string FormatException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            while (ex is AggregateException)
            {
                ex = ex.InnerException;
            }

            while (ex != null)
            {
                sb.Append(ex.Message);
                ex = ex.InnerException;
                if (ex != null)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }

        private static ErrorResponseBody GetErrorBodyFromException(Exception ex, HttpRequestMessage request)
        {
            return new ErrorResponseBody
            {
                Status = HttpStatusCode.InternalServerError,
                Source = request.RequestUri.ToString(),
                Message = FormatException(ex)
            };
        }
    }
}
