//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using Swashbuckle.Swagger;

    /// <summary>
    /// Operation filter to be used by swashbuckle to remove schema from 202 
    /// We do not send any object in 202 response of our poll operations
    /// </summary>
    public class Response202Filter : IOperationFilter
    {
        /// <summary>
        ///  Implement the interace of operation filter
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="schemaRegistry"></param>
        /// <param name="apiDescription"></param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, System.Web.Http.Description.ApiDescription apiDescription)
        {
            if (operation != null && operation.responses != null && operation.responses.ContainsKey("202"))
            {
                operation.responses["202"].schema = null;
            }
        }
    }
}
