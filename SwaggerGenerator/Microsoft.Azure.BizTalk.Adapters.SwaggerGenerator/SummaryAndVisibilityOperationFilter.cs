//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Web.Http.Description;
    using System.Web.Http.Controllers;
    using Swashbuckle.Swagger;

    /// <summary>
    /// 
    /// </summary>
    public class SummaryAndVisibilityOperationFilter : IOperationFilter
    {
        /// <summary>
        /// 
        /// </summary>
        public const string summaryVendorExtension = "x-ms-summary";

        /// <summary>
        /// 
        /// </summary>
        public const string visibilityVendorExtension = "x-ms-visibility";

        /// <summary>
        /// 
        /// </summary>
        public const string triggerRecommendation = "x-ms-scheduler-recommendation";

        /// <summary>
        /// 
        /// </summary>
        public const string triggerRecommendationValue = "@coalesce(triggers()?.outputs?.body?['triggerState'], '')";

        /// <summary>
        /// 
        /// </summary>
        public const string callbackRecommendationValue = "@accessKeys('default').primary.secretRunUri";

        /// <summary>
        /// 
        /// </summary>
        public const string triggerIdRecommendationValue = "@workflow().name";
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="schemaRegistry"></param>
        /// <param name="apiDescription"></param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            string operationName = operation.operationId;

            if (!string.IsNullOrEmpty(operationName))
            {
                // get visibility for operation 
                Visibility operationVisibility = GetVisibilityForOperation(apiDescription);
                string operationSummary = this.GetSummaryForOperation(apiDescription);
                operation.vendorExtensions = SetVendorExtension(operationSummary, operationVisibility, operation.vendorExtensions);

                if (operation.parameters == null)
                {
                    return;
                }

                foreach (Parameter param in operation.parameters)
                {
                    string summary = null;

                    // we need to get summary for query parameters only
                    summary = GetSummaryForParameterAndAddDefaultValue(apiDescription, param);
                    summary = summary == null ? param.description : summary;
                    
                    // we need to get visibility for all parameters
                    Visibility visibility = GetVisibilityForParameter(apiDescription, param.name);
                    param.vendorExtensions = SetVendorExtension(summary, visibility, param.vendorExtensions);
                                        
                    // for parameter as trigger state
                    if(param.name.Equals("triggerState", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if(param.vendorExtensions == null)
                        {
                            param.vendorExtensions = new Dictionary<string, object>();
                        }

                        param.vendorExtensions.Add(triggerRecommendation, triggerRecommendationValue);
                    }

                    if (param.name.Equals("triggerId", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (param.vendorExtensions == null)
                        {
                            param.vendorExtensions = new Dictionary<string, object>();
                        }

                        param.vendorExtensions.Add(triggerRecommendation, triggerIdRecommendationValue);
                    }
                }
            }
        }


        public static Dictionary<string, object> SetVendorExtension(string summary, Visibility visibility, Dictionary<string, object> vendorExtensions)
        {
            if(summary == null && visibility == Visibility.None)
            {
                return vendorExtensions;
            }
                        
            if (vendorExtensions == null)
            {
                vendorExtensions = new Dictionary<string, object>();
            }

            if (!vendorExtensions.ContainsKey(summaryVendorExtension) && summary != null)
            {
                vendorExtensions.Add(summaryVendorExtension, summary);
            }

            if (!vendorExtensions.ContainsKey(visibilityVendorExtension) && visibility != Visibility.None)
            {
                vendorExtensions.Add(visibilityVendorExtension, visibility.ToString().ToLower());
            }

            // nothing got added keep it null
            if (vendorExtensions.Keys.Count == 0)
            {
                return null;
            }

            return vendorExtensions;
        }


        private Visibility GetVisibilityForOperation(ApiDescription apiDescription)
        {
            // visibility 
            var customVisibilityAttribute = apiDescription.ActionDescriptor.GetCustomAttributes<CustomVisibilityAttribute>();
            if (customVisibilityAttribute != null && customVisibilityAttribute.Count > 0)
            {
                return customVisibilityAttribute[0].Visibility;
            }

            return Visibility.None;
        }

        private string GetSummaryForOperation(ApiDescription apiDescription)
        {
            var customSummaryAttribute = apiDescription.ActionDescriptor.GetCustomAttributes<CustomSummaryAttribute>();
            if (customSummaryAttribute != null && customSummaryAttribute.Count > 0)
            {
                return customSummaryAttribute[0].Summary;
            }

            return null;
        }

        private Visibility GetVisibilityForParameter(ApiDescription apiDescription, string parameterName)
        {
            var parameters = apiDescription.ActionDescriptor.GetParameters();
            if (parameters != null)
            {
                foreach (HttpParameterDescriptor paramDesc in parameters)
                {
                    if (paramDesc.ParameterName.Equals(parameterName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // visibility 
                        var customVisibilityAttribute = paramDesc.GetCustomAttributes<CustomVisibilityAttribute>();
                        if (customVisibilityAttribute != null && customVisibilityAttribute.Count > 0)
                        {
                            return customVisibilityAttribute[0].Visibility;
                        }
                    }
                }
            }

            return Visibility.None;
        }

        private string GetSummaryForParameterAndAddDefaultValue(ApiDescription apiDescription, Parameter param)
        {
            var parameters = apiDescription.ActionDescriptor.GetParameters();
            if (parameters != null)
            {
                foreach (HttpParameterDescriptor paramDesc in parameters)
                {
                    if (paramDesc.ParameterName.Equals(param.name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (param.@enum != null && paramDesc.DefaultValue != null)
                        {
                            param.@default = param.@enum[(int)paramDesc.DefaultValue];
                        }
                        else
                        {
                            param.@default = paramDesc.DefaultValue;
                        }

                        var customSummaryAttribute = paramDesc.GetCustomAttributes<CustomSummaryAttribute>();
                        if (customSummaryAttribute != null && customSummaryAttribute.Count > 0)
                        {
                            return customSummaryAttribute[0].Summary;
                        }
                    }
                }
            }

            return null;
        }
    }
}
