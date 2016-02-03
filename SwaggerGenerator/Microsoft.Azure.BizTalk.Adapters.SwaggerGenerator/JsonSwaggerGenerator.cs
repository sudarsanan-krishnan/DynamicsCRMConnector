//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Newtonsoft.Json;
    using Swashbuckle.Application;
    using Swashbuckle.Swagger;

    public abstract class JsonSwaggerGenerator : IDocumentFilter
    {
        private const string JsonContentType = "application/json";
        private const string SwaggerApiPath = "swagger";
        private const string TypePrefix = "#/definitions/";

        /// <summary>
        /// This function is used to convert .Net understandable types to Json types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string JsonDataTypeFromType(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }

            if (type == typeof(decimal))
            {
                return "number";
            }

            if (type == typeof(bool))
            {
                return "boolean";
            }

            if (type == typeof(DateTime))
            {
                return "string";
            }

            if (type == typeof(int))
            {
                return "integer";
            }

            return "string";
        }

        public void Apply(SwaggerDocument apideclaration, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            this.GetJsonSwagger(apideclaration);
        }

        public void GetJsonSwagger(SwaggerDocument apiDeclaration)
        {
            IDictionary<string, PathItem> tempApis = new Dictionary<string, PathItem>();
            IEnumerable<string> modelsToDelete = this.GetModelsToDelete();
            if (modelsToDelete != null)
            {
                foreach (string model in this.GetModelsToDelete())
                {
                    apiDeclaration.definitions.Remove(model);
                }
            }

            IDictionary<string, PathItem> apiList = new Dictionary<string, PathItem>(apiDeclaration.paths);
            IEnumerable<string> apisToIgnore = this.GetApisToIgnore();
            List<string> apisToIgnoreTrimmed = apisToIgnore.Select(api => api.Trim('/')).ToList();

            foreach (var apiPath in apiList.Keys)
            {
                var api = apiList[apiPath];
                if (apisToIgnoreTrimmed.Contains(apiPath.Trim('/')))
                {
                    apiDeclaration.paths.Remove(apiPath);
                    continue;
                }

                IEnumerable<HttpMethod> methods = GetApplicableHttpMethods(api);
                foreach (HttpMethod method in methods)
                {
                    List<JsonSwagger> jsonSwaggerList = EnhanceSwagger(apiPath, method);
                    if (jsonSwaggerList != null)
                    {
                        apiDeclaration.paths.Remove(apiPath);
                        Operation operation = GetOperation(api, method);
                        RemoveParameter(JsonSwagger.ParamToRemove, operation);
                        EnhanceApis(tempApis, jsonSwaggerList, api, operation);
                        EnhanceModels(apiDeclaration.definitions, jsonSwaggerList);
                    }
                }
            }

            foreach (KeyValuePair<string, PathItem> keyValuePair in tempApis)
            {
                apiDeclaration.paths.Add(keyValuePair);
            }
        }

        protected virtual List<JsonSwagger> EnhanceSwagger(string path, HttpMethod method)
        {
            throw new NotImplementedException();
        }

        protected abstract IEnumerable<string> GetModelsToDelete();

        protected abstract IEnumerable<string> GetApisToIgnore();

        protected static T Clone<T>(T source)
        {
            try
            {
                var serialized = JsonConvert.SerializeObject(source, new JsonSerializerSettings()
                {
                    // ignore null
                    NullValueHandling = NullValueHandling.Ignore,

                    // needed for cloning of schema objects, otherwise serialization will fail
                    Converters = new[] { new VendorExtensionsConverter() },

                    // serialization fails if $ref is the first property of any JObject hence doing a reverse sort of properties
                    // and then cloning, making description field to be first
                    ContractResolver = new OrderedContractResolver()
                });

                return JsonConvert.DeserializeObject<T>(serialized);
            }
            catch (JsonSerializationException ex)
            {
                throw new JsonSerializationException("Cannot clone the item", ex);
            }
        }

        public static string DefaultRootUrlResolver(HttpRequestMessage request)
        {
            string text = request.GetConfiguration().VirtualPathRoot.TrimEnd(new char[] { '/' });
            Uri requestUri = request.RequestUri;
            return string.Format("{0}://{1}:{2}{3}", new object[] { requestUri.Scheme, requestUri.Host, requestUri.Port, text });
        }

        private static IEnumerable<HttpMethod> GetApplicableHttpMethods(PathItem pathItem)
        {
            List<HttpMethod> methods = new List<HttpMethod>();
            if (pathItem.get != null)
            {
                methods.Add(HttpMethod.Get);
            }
            if (pathItem.put != null)
            {
                methods.Add(HttpMethod.Put);
            }
            if (pathItem.post != null)
            {
                methods.Add(HttpMethod.Post);
            }
            if (pathItem.delete != null)
            {
                methods.Add(HttpMethod.Delete);
            }
            if (pathItem.head != null)
            {
                methods.Add(HttpMethod.Head);
            }
            if (pathItem.options != null)
            {
                methods.Add(HttpMethod.Options);
            }

            return methods;
        } 

        private static Operation GetOperation(PathItem pathItem, HttpMethod method)
        {
            if (method == HttpMethod.Get)
            {
                return pathItem.get;
            }
            else if (method == HttpMethod.Put)
            {
                return pathItem.put;
            }
            else if (method == HttpMethod.Post)
            {
                return pathItem.post;
            }
            else if (method == HttpMethod.Delete)
            {
                return pathItem.delete;
            }
            else if (method == HttpMethod.Head)
            {
                return pathItem.head;
            }
            else if (method == HttpMethod.Options)
            {
                return pathItem.options;
            }

            return null;
        }

        public static bool ResolveVersionSupportByRouteConstraint(ApiDescription apiDesc, string targetApiVersion)
        {
            return true;
        }

        private static void EnhanceModels(IDictionary<string, Schema> models, List<JsonSwagger> jsonSwaggerList)
        {
            foreach (JsonSwagger jsonswagger in jsonSwaggerList)
            {
                foreach (KeyValuePair<string, SwaggerDataType> requestTypeModel in jsonswagger.GetRequestTypeModels())
                {
                    string key = requestTypeModel.Key.StartsWith(TypePrefix) ? requestTypeModel.Key.Replace(TypePrefix, string.Empty) : requestTypeModel.Key;
                    if (!models.ContainsKey(key))
                    {
                        models.Add(key, GetDataType(requestTypeModel.Value, jsonswagger.GetRequestTypeModels()));
                    }
                }

                foreach (KeyValuePair<string, SwaggerDataType> responseTypeModel in jsonswagger.GetResponseTypeModels())
                {
                    string key = responseTypeModel.Key.StartsWith(TypePrefix) ? responseTypeModel.Key.Replace(TypePrefix, string.Empty) : responseTypeModel.Key;
                    if (!models.ContainsKey(key))
                    {
                        models.Add(key, GetDataType(responseTypeModel.Value, jsonswagger.GetResponseTypeModels(), jsonswagger.ResponseTypeModelName));
                    }
                }
            }
        }

        private static Schema GetDataType(SwaggerDataType swaggerDataType, IDictionary<string, SwaggerDataType> models, string responseTypeModelName = null)
        {
            Schema datatype = new Schema();
            datatype.type = GetQualifiedSwaggerModelDefinition(swaggerDataType.Type);
            datatype.title = swaggerDataType.Id;
            datatype.properties = new Dictionary<string, Schema>();

            if (IsModel(swaggerDataType.Type) && !models.ContainsKey(swaggerDataType.Type))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type {0} does not exist in model collection", swaggerDataType.Type));
            }

            if (swaggerDataType.Type == "array")
            {
                Schema items = new Schema();
                items.@ref = GetQualifiedSwaggerModelDefinition(swaggerDataType.Ref);
                datatype.items = items;
            }

            if (swaggerDataType.GetProperties() != null)
            {
                foreach (KeyValuePair<string, SwaggerDataType> swaggerProperty in swaggerDataType.GetProperties())
                {
                    if (IsModel(swaggerProperty.Value.Type) && !models.ContainsKey(swaggerProperty.Value.Type))
                    {
                        if (swaggerProperty.Value.Type.Equals("array") && !models.ContainsKey(responseTypeModelName))
                        {
                            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type {0} does not exist in model collection", responseTypeModelName));
                        }
                        else
                        {
                            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Type {0} does not exist in model collection", swaggerProperty.Value.Type));
                        }
                    }

                    string typeRef = GetQualifiedSwaggerModelDefinition(swaggerProperty.Value.Type);
                    Schema propertySchema = new Schema();
                    if (IsModel(swaggerProperty.Value.Type))
                    {
                        propertySchema.@ref = typeRef;
                    }
                    else
                    {
                        if (swaggerProperty.Value.Type == "array")
                        {
                            Schema items = new Schema();
                            items.@ref = GetQualifiedSwaggerModelDefinition(swaggerProperty.Value.Ref);
                            propertySchema.items = items;
                        }

                        propertySchema.type = typeRef;
                    }

                    propertySchema.title = swaggerProperty.Value.Id;
                    datatype.properties.Add(swaggerProperty.Key, propertySchema);
                }
            }

            return datatype;
        }

        private static bool IsModel(string type)
        {
            switch (type)
            {
                case "integer":
                case "number":
                case "string":
                case "boolean":
                case "object":
                case "array":
                    return false;
                default:
                    return true;
            }
        }

        private static void EnhanceApis(
            IDictionary<string, PathItem> tempApis, IEnumerable<JsonSwagger> jsonSwaggerList, PathItem api, Operation operation)
        {
            PathItem newApi;

            foreach (JsonSwagger jsonSwagger in jsonSwaggerList)
            {
                tempApis.TryGetValue(jsonSwagger.NewPath, out newApi);

                if (newApi == null)
                {
                    newApi = Clone<PathItem>(api);
                    tempApis.Add(jsonSwagger.NewPath, newApi);
                }

                EnhanceApi(newApi, api, jsonSwagger, operation);
            }
        }

        private static void EnhanceApi(PathItem newApi, PathItem oldApi, JsonSwagger jsonSwagger, Operation operation)
        {
            if (operation == oldApi.delete)
            {
                EnhanceOperation(newApi.delete, oldApi.delete, jsonSwagger);
            }
            if (operation == oldApi.get)
            {
                EnhanceOperation(newApi.get, oldApi.get, jsonSwagger);
            }
            if (operation == oldApi.head)
            {
                EnhanceOperation(newApi.head, oldApi.head, jsonSwagger);
            }
            if (operation == oldApi.options)
            {
                EnhanceOperation(newApi.options, oldApi.options, jsonSwagger);
            }
            if (operation == oldApi.patch)
            {
                EnhanceOperation(newApi.patch, oldApi.patch, jsonSwagger);
            }
            if (operation == oldApi.post)
            {
                EnhanceOperation(newApi.post, oldApi.post, jsonSwagger);
            }
            if (operation == oldApi.put)
            {
                EnhanceOperation(newApi.put, oldApi.put, jsonSwagger);
            }
        }

        private static void EnhanceOperation(Operation newOperation, Operation oldOperation, JsonSwagger jsonSwagger)
        {
            if (newOperation == null)
            {
                return;
            }

            newOperation.operationId = Helper.SanitizeName(jsonSwagger.NickName);
            newOperation.summary = jsonSwagger.Summary;
            newOperation.description = jsonSwagger.Description;
            newOperation.vendorExtensions = oldOperation.vendorExtensions;
            SetParamAndResponse(newOperation, oldOperation, jsonSwagger);
        }

        private static void RemoveParameter(string paramName, Operation op)
        {
            if (string.IsNullOrWhiteSpace(paramName))
            {
                return;
            }

            bool isRemoved = false;

            for (int i = 0; i < op.parameters.Count; i++)
            {
                if (op.parameters[i].name.Equals(paramName))
                {
                    op.parameters.Remove(op.parameters[i]);
                    isRemoved = true;
                    break;
                }
            }

            if (!isRemoved)
            {
                throw new Exception("Parameter to remove is not present in operation parameters");
            }
        }

        ////private static void RemoveXmlResponses(IDictionary<string, PathItem> list)
        ////{
        ////    foreach (var api in list)
        ////    {
        ////        foreach (Operation op in api.Value.o)
        ////        {
        ////            RemoveXml(op.Produces);
        ////            RemoveXml(op.Consumes);
        ////        }
        ////    }
        ////}

        private static void RemoveXml(IList<string> list)
        {
            IList<string> newList = new List<string>();

            foreach (string response in list)
            {
                if (response.Equals("application/xml") || response.Equals("text/xml"))
                {
                    newList.Add(response);
                }
            }

            foreach (string response in newList)
            {
                list.Remove(response);
            }
        }

        private static string GetXmlCommentsPath(string fileName)
        {
            return string.Format(@"{0}\bin\{1}", System.AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        private static void SetParamAndResponse(Operation newOperation, Operation oldOperation, JsonSwagger jsonSwagger)
        {
            foreach (Parameter oldParameter in oldOperation.parameters)
            {
                Parameter newParameter = newOperation.parameters.First(p => p.name == oldParameter.name);
                newParameter.vendorExtensions = oldParameter.vendorExtensions;
            }

            if (!string.IsNullOrEmpty(jsonSwagger.ResponseType))
            {
                if (newOperation.responses != null)
                {
                    string responseCode = GetReturnCodeForSuccess(newOperation.responses.Keys);

                    Response newResponse = newOperation.responses[responseCode];
                    Response oldResponse = oldOperation.responses[responseCode];
                    if (oldResponse.schema != null)
                    {
                        if (newResponse.schema == null)
                        {
                            newResponse.schema = new Schema();
                        }

                        if (IsModel(jsonSwagger.ResponseType))
                        {
                            newResponse.schema.@ref = GetQualifiedSwaggerModelDefinition(jsonSwagger.ResponseType);
                            newResponse.schema.type = null;
                        }
                        else
                        {
                            newResponse.schema.type = jsonSwagger.ResponseType;
                        }

                        if (newResponse.schema.type == "array")
                        {
                            newResponse.schema.items = new Schema();
                            if (jsonSwagger.ResponseTypeModelName == null)
                            {
                                throw new ArgumentNullException("ResponseTypeModelName cannot be null if ResponseType is array");
                            }

                            if (IsModel(jsonSwagger.ResponseTypeModelName))
                            {
                                newResponse.schema.items.@ref = GetQualifiedSwaggerModelDefinition(jsonSwagger.ResponseTypeModelName);
                            }
                            else
                            {
                                newResponse.schema.items.type = jsonSwagger.ResponseTypeModelName;
                            }
                        }
                    }

                    if (newOperation.responses.ContainsKey("default"))
                    {
                        newOperation.responses["default"] = newResponse;
                    }
                    else
                    {
                        newOperation.responses.Add("default", newResponse);
                    }
                }
            }

            foreach (Parameter p in newOperation.parameters)
            {
                if (p.@in.Equals("body"))
                {
                    if (IsModel(jsonSwagger.RequestType))
                    {
                        if (p.schema == null)
                        {
                            p.schema = new Schema();
                        }

                        p.schema.@ref = GetQualifiedSwaggerModelDefinition(jsonSwagger.RequestType);
                    }
                    else
                    {
                        p.type = jsonSwagger.RequestType;
                    }
                }
            }
        }

        private static string GetQualifiedSwaggerModelDefinition(string type)
        {
            string qualifiedSwaggerType;

            if (IsModel(type))
            {
                if (type.StartsWith(TypePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    qualifiedSwaggerType = type;
                }
                else
                {
                    qualifiedSwaggerType = TypePrefix + type;
                }
            }
            else
            {
                qualifiedSwaggerType = type;
            }

            return qualifiedSwaggerType;
        }

        internal static string GetReturnCodeForSuccess(IEnumerable<string> responseCodes)
        {
            var successCodes = responseCodes.Where(code => code.StartsWith("2"));
            if (successCodes == null || !successCodes.Any())
            {
                throw new InvalidOperationException("Success code not found");
            }

            var orderedSuccessCodes = successCodes.OrderBy(c => c);
            return orderedSuccessCodes.First();
        }
    }
}