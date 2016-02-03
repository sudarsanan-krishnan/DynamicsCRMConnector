//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Biztalk.DynamicInvoke
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels;
    using Microsoft.Integration.Common;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param", Justification = "Parameter is fine")]
    public enum ParamType
    {
        Url,
        Query,
        Body,
        Header,
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth", Justification = "Parameter is fine")]
        Auth
    }

    public class HttpController
    {
        public const string Body = "Body";
        private IList<ApiModel> connectorModel = new List<ApiModel>();

        public void RegisterSwagger(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            foreach (System.IO.FileInfo fileInfo in dirInfo.GetFiles(@"*.json"))
            {
                using (StreamReader sr = new StreamReader(fileInfo.FullName))
                {
                    string metadata = sr.ReadToEnd();
                    ApiModel connector = SwaggerFile.Parse(metadata);
                    connectorModel.Add(connector);
                }
            }
        }

        // Method used for Testing
        public void InitializeConnectorModel()
        {
            this.connectorModel = new List<ApiModel>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Inherited code - CITs will ensure that this is fine"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "test hook")]
        public void RegisterSwaggerWithBaseUri(string newBaseUri, string path, string fileName)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(Path.Combine(dirInfo.FullName, fileName));
                if (fileInfo == null)
                {
                    throw new Exception("Could not find Swagger File: " + fileName);
                }

                AddSwaggerDetailsToConnectorModel(fileInfo, newBaseUri);
            }
            else
            {
                foreach (System.IO.FileInfo fileInfo in dirInfo.GetFiles(@"*.json"))
                {
                    AddSwaggerDetailsToConnectorModel(fileInfo, newBaseUri);
                }
            }
        }

        public async Task<HttpResponseMessage> InvokeHttpMethod(HttpRequestMessage request)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.Content.Headers.ContentType != null)
            {
                // Force reset the content type if the response is returning any content.
                // We are doing this because the workflow expects only application/json content type
                // dropbox returns application/jsvascript as the content type 
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return response;
        }

        public HttpRequestMessage CreateRequest(string operationName, HttpRequestMessage incomingRequestMessage, bool urlEncodeSpecialCharacters = false)
        {
            IDictionary<ParamType, IDictionary<string, string>> arguments = GetOperationParameters(incomingRequestMessage);
            return CreateRequest(operationName, arguments, urlEncodeSpecialCharacters);
        }

        public static HttpResponseMessage HandleException(HttpRequestMessage request, Exception ex)
        {
            HttpStatusCode returnCode = HttpStatusCode.InternalServerError;
            if (request != null && ex != null)
            {

                if (ex is InvalidOperationException || ex is ContentInvalidBase64Exception || ex is ContentNullException || ex is ArgumentException || ex is HttpException)
                {
                    returnCode = HttpStatusCode.BadRequest;
                    Logger.LogUserError(request, ex.ToString());
                }
                else if (ex is NotImplementedException)
                {
                    returnCode = HttpStatusCode.NotFound;
                    Logger.LogException(request, ex);
                }
                else if (ex is UnauthorizedAccessException)
                {
                    returnCode = HttpStatusCode.Unauthorized;
                    Logger.LogExceptionAsWarning(request, ex);
                }
                else
                {
                    Logger.LogException(request, ex);
                }
            }

            return HttpHelpers.CreateErrorResponse(request, returnCode, ex.Message);
        }

        public HttpRequestMessage CreateRequest(string operationName, HttpRequestMessage incomingRequestMessage, string body, bool urlEncodeSpecialCharacters = false)
        {
            IDictionary<ParamType, IDictionary<string, string>> arguments = GetOperationParameters(incomingRequestMessage);
            return CreateRequest(operationName, arguments, body, urlEncodeSpecialCharacters);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "by design")]
        public HttpRequestMessage CreateRequest(string operationName, IDictionary<ParamType, IDictionary<string, string>> arguments, bool urlEncodeSpecialCharacters = false)
        {
            return CreateRequest(operationName, arguments, string.Empty, urlEncodeSpecialCharacters);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "No side effect because of this")]
        public HttpRequestMessage CreateRequest(string operationName, IDictionary<ParamType, IDictionary<string, string>> arguments, string body, bool urlEncodeSpecialCharacters = false)
        {
            if (connectorModel.Count == 0)
            {
                throw new NotImplementedException("No swagger files registered");
            }

            if (arguments != null)
            {
                JToken jsonBodyObject = null;
                if (!string.IsNullOrEmpty(body))
                {
                    jsonBodyObject = RemoveNullValuesIfPresentFromBody(body);
                }

                foreach (ApiModel apiModel in connectorModel)
                {
                    if (apiModel.Keys.Contains(operationName))
                    {
                        if (jsonBodyObject != null)
                        {
                            return SubmitRequest(apiModel, operationName, arguments, jsonBodyObject, urlEncodeSpecialCharacters);
                        }

                        return SubmitRequest(apiModel, operationName, arguments, urlEncodeSpecialCharacters);
                    }
                }
            }

            throw new InvalidOperationException("swagger definition not found for operation: " + operationName);
        }

        public static JToken RemoveNullValuesIfPresentFromBody(string body)
        {
            JToken jtoken = JToken.Parse(body);
            var jsonBodyObjectCopy = JsonConvert.DeserializeObject<JToken>(body, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return jsonBodyObjectCopy;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "by design")]
        public static IDictionary<ParamType, IDictionary<string, string>> GetOperationParameters(HttpRequestMessage incomingRequestMessage)
        {
            IDictionary<string, string> urlArguments = new Dictionary<string, string>();
            IDictionary<ParamType, IDictionary<string, string>> argumentsMap = new Dictionary<ParamType, IDictionary<string, string>>();
            // Getting the url parameters from the routes
            System.Web.Http.Routing.IHttpRouteData[] routeData = incomingRequestMessage.GetConfiguration().Routes.GetRouteData(incomingRequestMessage).Values["MS_SubRoutes"] as System.Web.Http.Routing.IHttpRouteData[];
            if (routeData != null)
            {
                foreach (System.Web.Http.Routing.IHttpRouteData route in routeData)
                {
                    foreach (string str in route.Values.Keys)
                    {
                        if (!urlArguments.ContainsKey(str))
                        {
                            urlArguments.Add(str, route.Values[str] as string);
                        }
                    }
                }
            }

            argumentsMap.Add(ParamType.Url, urlArguments);
            IDictionary<string, string> queryArguments = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in incomingRequestMessage.GetQueryNameValuePairs())
            {
                if (!string.IsNullOrEmpty(pair.Value))
                {
                    queryArguments.Add(pair.Key, pair.Value);
                }
            }

            argumentsMap.Add(ParamType.Query, queryArguments);
            return argumentsMap;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Used by the test code"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Used by the test code")]
        public List<ApiModel> GetConnectorModel()
        {
            return connectorModel.ToList();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Used by the test code")]
        public IList<Operation> GetListOfOperations()
        {
            List<Operation> allOperations = new List<Operation>();
            if (connectorModel.Count == 0)
            {
                throw new NotImplementedException("No swagger files registered");
            }

            foreach (ApiModel apiModel in connectorModel)
            {
                allOperations.AddRange(apiModel.Operations.ToList());
            }

            return allOperations;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Used by test code")]
        public DataType GetModel(string dataType)
        {
            DataType returnedType = null;
            if (connectorModel.Count == 0)
            {
                throw new NotImplementedException("No swagger files registered");
            }

            foreach (ApiModel apiModel in connectorModel)
            {
                if (apiModel.DataTypes.Count() > 0)
                {
                    try
                    {
                        returnedType = apiModel.DataTypes.First(type => type.Name == dataType);
                    }
                    catch
                    {
                        // Since Multiple ApiModels may be present, search for datatype in next ApiModel
                        continue;
                    }

                    if (returnedType != null)
                    {
                        break;
                    }
                }
            }

            return returnedType;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Param", Justification = "Name is fine")]
        public static ParamType GetMappedParamType(OperationParameterType operationParameterType)
        {
            switch (operationParameterType)
            {
                case OperationParameterType.Body:
                case OperationParameterType.Form:
                    return ParamType.Body;
                case OperationParameterType.Header:
                    return ParamType.Header;
                case OperationParameterType.Path:
                    return ParamType.Url;
                case OperationParameterType.Query:
                    return ParamType.Query;
                default:
                    return ParamType.Query;
            }
        }

        private static HttpRequestMessage SubmitRequest(ApiModel apiModel, string operationName, IDictionary<ParamType, IDictionary<string, string>> arguments, bool urlEncodeSpecialCharacters = false)
        {
            IDictionary<string, string> bodyArguments = GetMap(ParamType.Body, arguments);
            JObject bodyObject = JObject.Parse(JsonConvert.SerializeObject(bodyArguments));
            return SubmitRequest(apiModel, operationName, arguments, bodyObject, urlEncodeSpecialCharacters);
        }

        private static HttpRequestMessage SubmitRequest(ApiModel apiModel, string operationName, IDictionary<ParamType, IDictionary<string, string>> arguments, JToken bodyObject, bool urlEncodeSpecialCharacters = false)
        {
            IDictionary<string, string> urlArguments = GetMap(ParamType.Url, arguments);
            IDictionary<string, string> queryArguments = GetMap(ParamType.Query, arguments);
            IDictionary<string, string> headerArguments = GetMap(ParamType.Header, arguments);
            IDictionary<string, string> authArguments = GetMap(ParamType.Auth, arguments);
            return apiModel.CreateRequest(operationName, urlArguments, queryArguments, bodyObject, headerArguments, authArguments, urlEncodeSpecialCharacters);
        }

        private static IDictionary<string, string> GetMap(ParamType type, IDictionary<ParamType, IDictionary<string, string>> map)
        {
            if (map.ContainsKey(type))
            {
                return map[type];
            }

            return new Dictionary<string, string>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Inherited code")]
        private static bool IsPrimitiveType(string typeName)
        {
            return typeName == "float" || typeName == "integer" || typeName == "number" || typeName == "string" || typeName == "boolean" || typeName == "date";
        }

        private void AddSwaggerDetailsToConnectorModel(System.IO.FileInfo fileInfo, string newBaseUri)
        {
            using (StreamReader sr = new StreamReader(fileInfo.FullName))
            {
                string metadata = sr.ReadToEnd();
                ApiModel connector = SwaggerFile.Parse(metadata);
                connector = connector.WithBaseUri(newBaseUri);
                connectorModel.Add(connector);
            }
        }
    }
}
