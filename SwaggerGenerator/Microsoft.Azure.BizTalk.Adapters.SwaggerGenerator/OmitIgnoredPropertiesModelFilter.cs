//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Swashbuckle.Swagger;

    /// <summary>
    /// This is to omit class properties which have jsonignoreattribute tag
    /// </summary>
    public class OmitIgnoredPropertiesModelFilter : ISchemaFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dataTypeRegistry"></param>
        /// <param name="type"></param>
        public void Apply(Schema model, SchemaRegistry dataTypeRegistry, Type type)
        {
            if (model != null && dataTypeRegistry != null && type != null)
            {
                var ignoredProperties = type.GetProperties().Where(p => p.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).FirstOrDefault() != null);

                foreach (var property in ignoredProperties)
                {
                    model.properties.Remove(property.Name);
                }
            }
        }
    }
}