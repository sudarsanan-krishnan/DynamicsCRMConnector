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
    public static class Swagger20Parser
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

        internal static string StripDefinitionPrefix(string returnType)
        {
            return returnType.Replace("#/definitions/", string.Empty);
        }


        private static string GetBaseUri(JObject swaggerJson)
        {
            string scheme = (string)((JArray)swaggerJson["schemes"])[0];
            string host = (string)swaggerJson["host"];
            return string.Format(CultureInfo.CurrentCulture, "{0}://{1}", scheme, host);
        }

        private static IEnumerable<DataType> GetDataTypes(JObject swaggerJson)
        {
            var models = swaggerJson["definitions"] ?? new JObject();
            return models.Children().Select(DataTypeFromModel);
        }

        private static DataType DataTypeFromModel(JToken model)
        {
            var modelProperty = (JProperty)model;
            JToken modelValue = modelProperty.Value;
            JArray requiredArray = (JArray)modelValue["required"];
            var typeId = modelProperty.Name;
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
            List<Operation> list = new List<Operation>();
            var apis = swaggerJson["paths"] ?? new JObject();
            foreach (var api in apis)
            {
                JProperty property = api as JProperty;
                var operationPath = property.Name;
                JObject operations = property.Value as JObject;
                foreach (var operationDetail in operations)
                {
                    Operation op = OperationFromSwaggerOperation(operationPath, operationDetail.Key, operationDetail.Value);
                    list.Add(op);
                }
            }

            return list;
        }

        private static int GetReturnCodeForSuccess(JToken response)
        {
            if (response["200"] != null)
            {
                return 200;
            }

            if (response["202"] != null)
            {
                return 202;
            }

            if (response["201"] != null)
            {
                return 201;
            }

            if (response["206"] != null)
            {
                return 206;
            }

            if (response["204"] != null)
            {
                return 204;
            }

            throw new InvalidOperationException("Succes code not found");
        }

        private static Operation OperationFromSwaggerOperation(string operationpath, string operationMethod, JToken operationToken)
        {
            var method = new HttpMethod(operationMethod);
            var methodName = (string)operationToken["operationId"];
            var responses = operationToken["responses"];
            int successCode = GetReturnCodeForSuccess(responses);
            var returnSchema = responses[successCode.ToString()]["schema"];

            string returnType = string.Empty;
            if (returnSchema != null)
            {
                returnType = (string)returnSchema["type"];
            }

            IEnumerable<IOperationParameter> operationParamList = null;
            if (string.IsNullOrEmpty(returnType) && returnSchema != null)
            {
                returnType = (string)returnSchema["$ref"];
            }

            returnType = StripDefinitionPrefix(returnType);
            if (operationToken["parameters"] != null)
            {
                operationParamList = operationToken["parameters"].Children().Select(CreateOperationParameter).Where(p => p != null);
            }

            var itemType = string.Empty;

            if (returnType.Equals("array", StringComparison.OrdinalIgnoreCase))
            {
                var type = returnSchema["items"]["$ref"] ?? returnSchema["items"]["type"];
                if (type != null)
                {
                    itemType = StripDefinitionPrefix(type.ToString());
                }
            }

            return new Operation(methodName, returnType, operationpath, method, ParserUtil.CreateAuthorization(operationToken),
                operationParamList,
                responses.Children().Select(CreateStatusCode).Distinct(),
                itemType);
        }

        // This has to be refactored later.
       private static string GetReturnCodeForSuccess(IEnumerable<string> responseCodes)
        {
            var successCodes = responseCodes.Where(code => code.StartsWith("2"));
            if (successCodes == null || !successCodes.Any())
            {
                throw new InvalidOperationException("Success code not found");
            }

            var orderedSuccessCodes = successCodes.OrderBy(c => c);
            return orderedSuccessCodes.First();
        }

        private static HttpStatusCode CreateStatusCode(JToken responseMessageJson)
        {
            JProperty responseCode = responseMessageJson as JProperty;            
            if(responseCode.Name.Equals("default"))
            {                
                IList<string> opCodes = new List<string>();
                 foreach(JProperty token in  responseCode.Parent.Children())
                 {
                     if (token.Name.Equals("default"))
                     {
                         continue;
                     }

                     opCodes.Add(token.Name);
                 }

                 return (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), GetReturnCodeForSuccess(opCodes));                
            }

            HttpStatusCode statusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), responseCode.Name);
            return statusCode;
        }        
        private static IOperationParameter CreateOperationParameter(JToken parameterJson)
        {
            var paramName = (string)parameterJson["name"];
            var paramType = (string)parameterJson["in"];
            var paramDataType = (string)parameterJson["type"] ?? (string)parameterJson["$ref"] ?? (string)parameterJson["schema"]["type"] ?? (string)parameterJson["schema"]["$ref"];
            paramDataType = StripDefinitionPrefix(paramDataType);
            var defaultValue = parameterJson["defaultValue"];
            var paramRequired = parameterJson["required"] != null ? Convert.ToBoolean(parameterJson["required"].ToString(), CultureInfo.CurrentCulture) : false;
            switch (paramType)
            {
                case "path":
                    return new PathOperationParameter(paramName, paramDataType, paramRequired, defaultValue != null ? (string)defaultValue : string.Empty);
                case "query":
                    return new QueryStringOperationParameter(paramName, paramDataType, paramRequired, defaultValue != null ? (string)defaultValue : string.Empty);
                case "body":
                    return CreateBodyOperationParameter(paramDataType, defaultValue, paramRequired);
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

        private static IOperationParameter CreateBodyOperationParameter(string paramDataType, JToken defaultValue, bool paramRequired)
        {
            if (ParserUtil.IsPrimitiveType(paramDataType))
            {
                return new BodyOperationParameter(new PrimitiveDataType(paramDataType, defaultValue != null ? (string)defaultValue : string.Empty), paramRequired);
            }
            
            return new BodyOperationParameter(new TypeReferenceDataType(paramDataType), paramRequired);
        }
    }
}
