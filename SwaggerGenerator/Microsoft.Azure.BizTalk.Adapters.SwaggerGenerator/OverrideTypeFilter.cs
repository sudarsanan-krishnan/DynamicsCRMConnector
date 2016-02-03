//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;
    using System.Linq;
    using Swashbuckle.Swagger;
    using System.Reflection;

    /// <summary>
    /// Class used for overridign types
    /// </summary>
    public class OverrideTypeFilter : ISchemaFilter
    {
        private readonly string[] _supportedTypes = {"string", "int", "boolean"};

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="schemaRegistry"></param>
        /// <param name="type"></param>
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema != null && schemaRegistry != null && type != null && schema.properties != null)
            {
                foreach (string property in schema.properties.Keys)
                {
                    Schema propSchema = schema.properties[property];
                    var newType = this.GetOverrideType(type, property);
                    if (!string.IsNullOrEmpty(newType))
                    {
                        // Only string, boolean and int data types are supported in OverideTypeAttribute. 
                        if (this.IsSupportedDataType(newType))
                        {
                            propSchema.@ref = null;
                            propSchema.type = newType;
                        }
                    }
                }
            }
        }

        private string GetOverrideType(Type type, string propertyName)
        {
            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var overrideTypeAttribute = propertyInfo.GetCustomAttribute<OverrideTypeAttribute>();
                if (overrideTypeAttribute != null)
                {
                    return overrideTypeAttribute.FieldType;
                }
            }

            return null;
        }

        private bool IsSupportedDataType(string type)
        {
            return this._supportedTypes.Contains(type);
        }
    }
}
