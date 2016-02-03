//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{    
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Swashbuckle.Swagger;

    public class OperationNameAndDefaultResponseFilter : IOperationFilter
    {
        private const string defaultResponseCode = "default";

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, System.Web.Http.Description.ApiDescription apiDescription)
        {
            // stripping the name
            string operationName = operation.operationId;

            if (!string.IsNullOrEmpty(operationName))
            {
                // swashbuckle adds controller name, stripping that
                int index = operationName.IndexOf("_", 0);
                if (index >= 0 && (index + 1) < operationName.Length)
                {
                    operation.operationId = operationName.Substring(index + 1);
                }
            }

            // operation response change
            IDictionary<string, Response> responses = operation.responses;
            if (responses != null && !responses.ContainsKey(defaultResponseCode))
            {
                try
                {
                    string successResponseCode = JsonSwaggerGenerator.GetReturnCodeForSuccess(responses.Keys);

                    Response successResponse = responses[successResponseCode];
                    Response defaultResponse = new Response();
                    defaultResponse.description = Resources.DefaultResponseDescription;
                    defaultResponse.schema = null;
                    responses.Add(defaultResponseCode, defaultResponse);
                }
                catch(InvalidOperationException)
                {
                    throw new Exception("No success code found, not adding default response code");
                }
            }
        }
    }
}
