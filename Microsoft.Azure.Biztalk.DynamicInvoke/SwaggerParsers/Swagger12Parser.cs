//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.SwaggerParsers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Parsing logic for creating an ApiModel
    /// out of a swagger v1.2 document.
    /// </summary>
    public static class Swagger12Parser
    {
        public static ApiModel Parse(string swaggerDoc)
        {
            var swaggerJson = JObject.Parse(swaggerDoc);
            return Parse(swaggerJson);
        }

        public static ApiModel Parse(JObject swaggerJson)
        {
            return new ApiModel(GetBaseUri(swaggerJson), GetDataTypes(swaggerJson), GetOperations(swaggerJson));
        }

        private static string GetBaseUri(JObject swaggerJson)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}{1}", swaggerJson["basePath"], swaggerJson["resourcePath"]);
        }

        private static IEnumerable<DataType> GetDataTypes(JObject swaggerJson)
        {
            var models = swaggerJson["models"] ?? new JObject();
            return models.Children().Select(DataTypeFromModel);
        }

        private static DataType DataTypeFromModel(JToken model)
        {
            var modelProperty = (JProperty)model;
            JToken modelValue = modelProperty.Value;
            JArray requiredArray = (JArray)modelValue["required"];
            var typeId = (string)modelValue["id"];
            return new CompositeDataType(typeId,
                modelValue["properties"].Children().Select(ParserUtil.DataTypeFromModelProperty), 
                requiredArray != null ? requiredArray.Children().Select(RequiredFromModelProperty) : new List<string>());
        }

        private static string RequiredFromModelProperty(JToken model)
        {
            return model.ToString();
        }

        private static IEnumerable<Operation> GetOperations(JObject swaggerJson)
        {
            var apis = swaggerJson["apis"] ?? new JObject();
            return apis.Children().SelectMany(OperationsFromApi);
        }

        private static IEnumerable<Operation> OperationsFromApi(JToken apiToken)
        {
            var operationPath = (string)apiToken["path"];
            return apiToken["operations"].Children().Select(op => OperationFromSwaggerOperation(operationPath, op));
        }

        private static Operation OperationFromSwaggerOperation(string path, JToken operationToken)
        {
            var method = new HttpMethod((string)operationToken["method"]);
            var methodName = (string)operationToken["nickname"];
            var returnType = (string)operationToken["type"];
            return new Operation(methodName, returnType, path, method, ParserUtil.CreateAuthorization(operationToken),
                operationToken["parameters"].Children().Select(CreateOperationParameter).Where(p => p != null),
                operationToken["responseMessages"].Children().Select(CreateStatusCode),
                returnType == "array" ? operationToken["items"]["$ref"].ToString() : string.Empty);
        }

        private static HttpStatusCode CreateStatusCode(JToken responseMessageJson)
        {
            var responseMessageCode = (string)responseMessageJson["code"];
            HttpStatusCode statusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), responseMessageCode);
            return statusCode;
        }

        private static IOperationParameter CreateOperationParameter(JToken parameterJson)
        {
            var paramName = (string)parameterJson["name"];
            var paramType = (string)parameterJson["paramType"];
            var paramDataType = (string)parameterJson["type"] ?? (string)parameterJson["$ref"];
            var defaultValue = parameterJson["defaultValue"];
            var paramRequired = parameterJson["required"] != null ? Convert.ToBoolean(parameterJson["required"].ToString(), CultureInfo.CurrentCulture) : false;
            switch (paramType)
            {
                case "path":
                    return new PathOperationParameter(paramName, paramDataType, paramRequired, defaultValue != null ? (string)defaultValue : string.Empty);
                case "query":               
                    return new QueryStringOperationParameter(paramName, paramDataType, paramRequired, defaultValue!=null?(string)defaultValue:string.Empty);
                case "body":
                    return new BodyOperationParameter(new TypeReferenceDataType(paramDataType), paramRequired);
                case "header":
                    return new HeaderOperationParameter(paramName, paramDataType, paramRequired, defaultValue != null ? (string)defaultValue : string.Empty);
                case "form":
                    // Not implemented, skip for now
                    return new FormOperationParameter(new TypeReferenceDataType(paramDataType), paramRequired);
                    //return null;
                default:
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture, "Unknown operation parameter type {0}, invalid swagger doc",
                            paramType),
                        "parameterJson");
            }
        }
    }
}
