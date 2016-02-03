//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using Newtonsoft.Json.Serialization;

    public static class Extensions
    {
        public static IContractResolver GetJsonContractResolver(this HttpConfiguration httpConfig)
        {           
            var formatter = httpConfig.Formatters.JsonFormatter;
            return (formatter != null)
                ? formatter.SerializerSettings.ContractResolver
                : new DefaultContractResolver();
        }
       
        public static string DefaultRootUrlResolver(this HttpRequestMessage request)
        {
            string text = request.GetConfiguration().VirtualPathRoot.TrimEnd(new char[] { '/' });
            Uri requestUri = request.RequestUri;
            return string.Format("{0}://{1}:{2}{3}", new object[]	
            {
                requestUri.Scheme,	
                requestUri.Host,		
                requestUri.Port,		        
                text	
            });
        }
    }
}