//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System.Collections.Generic;
    using System.Web.Http.Description;
    using Swashbuckle.Swagger;

    /// <summary>
    /// 
    /// </summary>
    public class OauthApiFilter : IDocumentFilter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="schemaRegistry"></param>
        /// <param name="apiExplorer"></param>
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            IList<string> apilist = new List<string>();

            if (swaggerDoc != null && swaggerDoc.paths != null)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, PathItem> pathitem in swaggerDoc.paths)
                {
                    if (pathitem.Key.Contains("OAuth"))
                    {
                        apilist.Add(pathitem.Key);
                    }
                }
            }

            foreach (string pathitem in apilist)
            {
                swaggerDoc.paths.Remove(pathitem);
            }

            if (swaggerDoc != null && swaggerDoc.definitions != null)
            {
                swaggerDoc.definitions.Remove("TokenResult");
                swaggerDoc.definitions.Remove("Object");
            }
        }
    }
}
