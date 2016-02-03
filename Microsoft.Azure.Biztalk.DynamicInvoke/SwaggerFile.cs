//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke
{
    using System;
    using Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels;
    using Microsoft.Azure.Biztalk.DynamicInvoke.SwaggerParsers;
    using Newtonsoft.Json.Linq;

    public static class SwaggerFile
    {
        public static ApiModel Parse(string swaggerDoc)
        {
            JObject swaggerJson = JObject.Parse(swaggerDoc);

            if ((string)swaggerJson["swagger"] == "2.0")
            {
                return Swagger20Parser.Parse(swaggerJson);
            }

            if ((string)swaggerJson["swaggerVersion"] == "1.2")
            {
                return Swagger12Parser.Parse(swaggerJson);
            }

            throw new ArgumentException("Cannot parse, unknown swagger version", "swaggerDoc");
        }
    }
}
