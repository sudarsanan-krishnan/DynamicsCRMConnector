//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Swashbuckle.Swagger;
    using System.Reflection;

    /// <summary>
    /// 
    /// </summary>
    public class SummaryAndVisibilityModelFilter : ISchemaFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dataTypeRegistry"></param>
        /// <param name="type"></param>
        public void Apply(Schema model, SchemaRegistry dataTypeRegistry, Type type)
        {
            if (model != null && dataTypeRegistry != null && type != null && model.properties != null)
            {
                bool isPushTrigger = false;

                // not taking dependency on runtime dll to avoid circular dependencies betwwen saas and enterprise repo
                if (!string.IsNullOrEmpty(type.AssemblyQualifiedName) && type.AssemblyQualifiedName.StartsWith("Microsoft.Azure.AppService.ApiApps.Service.TriggerInput", StringComparison.InvariantCultureIgnoreCase))
                {
                    isPushTrigger = true;
                }

                foreach (string property in model.properties.Keys)
                {
                    Schema propSchema = model.properties[property];
                    propSchema.vendorExtensions = SetSummaryAndVisibility(type, property, propSchema.description, propSchema.vendorExtensions);

                    if (isPushTrigger && property.Equals("callbackurl", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (model.required == null)
                        {
                            model.required = new List<string>();
                        }

                        if (!model.required.Contains(property))
                        {
                            model.required.Add(property);
                        }

                        propSchema.vendorExtensions = SummaryAndVisibilityOperationFilter.SetVendorExtension(summary: null, visibility: Visibility.Internal, vendorExtensions: propSchema.vendorExtensions);
                        if (!propSchema.vendorExtensions.ContainsKey(SummaryAndVisibilityOperationFilter.triggerRecommendation))
                        {
                            propSchema.vendorExtensions.Add(SummaryAndVisibilityOperationFilter.triggerRecommendation, SummaryAndVisibilityOperationFilter.callbackRecommendationValue);
                        }
                    }
                }

                // in case of no properties inside the class or object, set the visibility/summary attributes on the class object.
                if (model.properties.Keys.Count == 0)
                {
                    model.vendorExtensions = SetSummaryAndVisibility(type, model.description, model.vendorExtensions);
                }
            }
        }

        private Dictionary<string, object> SetSummaryAndVisibility(Type type, string propertyName, string defaultDescription, Dictionary<string, object> vendorExtensions)
        {
            string summary = defaultDescription;
            Visibility visibility = Visibility.None;

            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo != null)
            {
                var summaryAttribute = propertyInfo.GetCustomAttribute<CustomSummaryAttribute>();
                if (summaryAttribute != null)
                {
                    summary = summaryAttribute.Summary;
                }

                var visibilityAttribute = propertyInfo.GetCustomAttribute<CustomVisibilityAttribute>();
                if (visibilityAttribute != null)
                {
                    visibility = visibilityAttribute.Visibility;
                }
            }

            return SummaryAndVisibilityOperationFilter.SetVendorExtension(summary, visibility, vendorExtensions);
        }

        private Dictionary<string, object> SetSummaryAndVisibility(Type type, string defaultDescription, Dictionary<string, object> vendorExtensions)
        {
            string summary = defaultDescription;
            Visibility visibility = Visibility.None;

            if (type != null)
            {
                var summaryAttribute = type.GetCustomAttribute<CustomSummaryAttribute>();
                if (summaryAttribute != null)
                {
                    summary = summaryAttribute.Summary;
                }

                var visibilityAttribute = type.GetCustomAttribute<CustomVisibilityAttribute>();
                if (visibilityAttribute != null)
                {
                    visibility = visibilityAttribute.Visibility;
                }
            }

            return SummaryAndVisibilityOperationFilter.SetVendorExtension(summary, visibility, vendorExtensions);
        }
    }
}
