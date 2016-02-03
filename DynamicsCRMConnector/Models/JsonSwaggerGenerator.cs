//-----------------------------------------------------------------------
// <copyright file="JsonSwaggerGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DynamicsCRMConnector.Models
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Runtime.CompilerServices;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Swashbuckle.Application;
    using Swashbuckle.Swagger;

    internal static class JsonSwaggerGenerator
    {
        internal static IContractResolver GetJsonContractResolver(this HttpConfiguration httpConfig)
        {
            var formatter = httpConfig.Formatters.JsonFormatter;
            return (formatter != null)
                ? formatter.SerializerSettings.ContractResolver
                : new DefaultContractResolver();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static string DefaultRootUrlResolver(HttpRequestMessage request)
        {
            string text = request.GetConfiguration().VirtualPathRoot.TrimEnd(new char[] { '/' });
            Uri requestUri = request.RequestUri;
            return string.Format(CultureInfo.InvariantCulture, "{0}://{1}:{2}{3}", new object[]
            { 
                requestUri.Scheme, requestUri.Host, requestUri.Port, text 
            });
        }

        internal static bool ResolveVersionSupportByRouteConstraint(ApiDescription apiDesc, string targetApiVersion)
        {
            return true;
        }
    }    
}