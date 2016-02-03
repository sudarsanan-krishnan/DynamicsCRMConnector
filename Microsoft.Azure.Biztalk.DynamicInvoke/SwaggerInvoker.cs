//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Biztalk.DynamicInvoke
{
    using System.Collections.Generic;
    using System.Net.Http;
    using Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels;
    using Newtonsoft.Json.Linq;

    public class SwaggerInvoker
    {
        public SwaggerInvoker(string swagger)
        {
            Model = SwaggerFile.Parse(swagger);
        }

        public ApiModel Model
        { 
            get;
            private set; 
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "auth", Justification = "Inherited code - No side effect because of this")]
        public HttpRequestMessage Create(string nickname,
                                         IDictionary<string, string> urlArguments,
                                         IDictionary<string, string> queryArguments,
                                         IDictionary<string, string> headerArguments,
                                         JObject bodyObject,
                                         IDictionary<string, string> authArguments,
                                         bool urlEncodeSpecialCharacters = false)
        {
            return Model.CreateRequest(nickname, urlArguments, queryArguments, bodyObject, headerArguments, authArguments, urlEncodeSpecialCharacters);
        }
    }
}
