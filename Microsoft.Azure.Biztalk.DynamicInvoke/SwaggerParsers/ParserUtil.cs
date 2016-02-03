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

    internal static class ParserUtil
    {
        internal static KeyValuePair<string, DataType> DataTypeFromModelProperty(JToken model)
        {
            var modelProperty = (JProperty)model;
            var propertyName = modelProperty.Name;
            var dataTypeFields = modelProperty.Value;
            if (dataTypeFields["type"] != null)
            {
                var propertyType = (string)dataTypeFields["type"];
                if (IsPrimitiveType(propertyType))
                {
                    var defaultValue = dataTypeFields["defaultValue"];
                    return new KeyValuePair<string, DataType>(propertyName, new PrimitiveDataType(propertyType, defaultValue != null ? (string)defaultValue : string.Empty));
                }

                if (propertyType == "array")
                {
                    return new KeyValuePair<string, DataType>(propertyName, CreateArrayDataType(dataTypeFields));
                }

                if (propertyType == "object" && dataTypeFields["additionalProperties"] != null)
                {
                    return new KeyValuePair<string, DataType>(propertyName, CreateMapDataType(dataTypeFields));
                }

                return new KeyValuePair<string, DataType>(propertyName, new TypeReferenceDataType((string)dataTypeFields["type"]));
            }

            if (dataTypeFields["$ref"] != null)
            {
                return new KeyValuePair<string, DataType>(propertyName, new TypeReferenceDataType((string)dataTypeFields["$ref"]));
            }

            throw new ArgumentException("No type or $ref in model, this is invalid swagger", "model");
        }

        internal static bool IsPrimitiveType(string typeName)
        {
            return typeName == "integer" || typeName == "number" || typeName == "string" || typeName == "boolean";
        }

        internal static OperationAuthorization CreateAuthorization(JToken operationToken)
        {
            JToken authToken = operationToken["authorizations"];
            if (authToken != null)
            {
                if (authToken["oauth2"] != null)
                {
                    return new Oauth2OperationAuthorization();
                }
            }

            return new UnauthenticatedOperationAuthorization();
        }

        private static DataType CreateArrayDataType(JToken dataTypeFields)
        {
            var itemType = (string)dataTypeFields["items"]["type"];
            if (string.IsNullOrEmpty(itemType))
            {
                itemType = (string)dataTypeFields["items"]["$ref"];
            }

            if (IsPrimitiveType(itemType))
            {
                return new ArrayDataType(new PrimitiveDataType(itemType));
            }

            return new ArrayDataType(new TypeReferenceDataType(itemType));
        }

        private static DataType CreateMapDataType(JToken dataTypeFields)
        {
            var itemType = (string)dataTypeFields["additionalProperties"]["type"];
            if (string.IsNullOrEmpty(itemType))
            {
                itemType = (string)dataTypeFields["additionalProperties"]["$ref"];
            }

            if (IsPrimitiveType(itemType))
            {
                return new MapDataType(new PrimitiveDataType(itemType));
            }

            return new MapDataType(new TypeReferenceDataType(itemType));
        }
    }
}
