//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class representing an entire REST API endpoint.
    /// </summary>    
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Api", Justification = "name is apt")]
    public class ApiModel
    {
        /// <summary>
        /// base URL
        /// </summary>
        private readonly string baseUri;
        
        /// <summary>
        /// parameter details
        /// </summary>
        private readonly Dictionary<string, DataType> dataTypes;
       
        /// <summary>
        /// operation details
        /// </summary>
        private readonly Dictionary<string, Operation> operations;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="dataTypes"></param>
        /// <param name="operations"></param>        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "inherited code - string is fine. CITS will catch issues, if any")]
        public ApiModel(string baseUriArgument, IEnumerable<DataType> dataTypesArgument, IEnumerable<Operation> operationsArgument)
        {
            baseUri = baseUriArgument;
            dataTypes = TypeReferenceResolver.ResolveTypeReferences(dataTypesArgument)
                .ToDictionary(dt => dt.Name);
            operations = TypeReferenceResolver.ResolveOperationReferences(dataTypes.Values, operationsArgument)
                .ToDictionary(op => op.Name);
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="baseUriArgument"></param>
        /// <param name="dataTypes"></param>
        /// <param name="operations"></param>
        private ApiModel(string baseUriArgument, Dictionary<string, DataType> dataTypesArgument, Dictionary<string, Operation> operationsArgument)
        {
            baseUri = baseUriArgument;
            dataTypes = dataTypesArgument;
            operations = operationsArgument;
        }

        /// <summary>
        /// 
        /// </summary>
        public Uri BaseUri
        {
            get { return new Uri(baseUri, UriKind.RelativeOrAbsolute); }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DataType> DataTypes
        {
            get { return dataTypes.Values; }
        }

        public IEnumerable<Operation> Operations
        {
            get { return operations.Values; }
        }

        public IEnumerable<string> Keys
        {
            get { return operations.Keys; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newBaseUri"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "", Justification = "inherited code - Its okay to be not strongly typed here. The CITs will catch the code issues")]
        public ApiModel WithBaseUri(string newBaseUri)
        {
            return new ApiModel(newBaseUri, dataTypes, operations);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "auth", Justification = "name is apt"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification = "inherited code - Its okay to be not strongly typed here. The CITs will catch the code issues"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "urlparameter", Justification = "name is fine"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "caller of this method will do the cleanup")]
        public HttpRequestMessage CreateRequest(string operationName,
                                                object urlparameterValues, 
                                                object queryParameterValues, 
                                                JToken bodyObject, 
                                                object headerParameterValues,
                                                object authParametersValues,
                                                bool urlEncodeSpecialCharacters = false)
        {
            if (!operations.ContainsKey(operationName))
            {
                throw new KeyNotFoundException(string.Format(CultureInfo.CurrentCulture, "The operation \"{0}\" is not defined for this API", operationName));
            }

            var operation = operations[operationName];
            var uri = new UriBuilder(baseUri + operation.GetUriPath(urlparameterValues))
            {
                Query = operation.GetQueryParameters(queryParameterValues, urlEncodeSpecialCharacters)
            };

            string body = operation.GetRequestBody(bodyObject);

            var requestMessage = new HttpRequestMessage(operation.Method, uri.Uri);
            if (!string.IsNullOrEmpty(body))
            {
                // TODO - other content types
                HttpContent content = new StringContent(body);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                requestMessage.Content = content;
            }

            foreach (var headerParam in operation.Parameters.OfType<HeaderOperationParameter>())
            {
                var value = headerParam.GetValue(headerParameterValues);
                if (value != null)
                {
                    requestMessage.Headers.Add(headerParam.Name, value.ToString());
                }
            }

            operation.Authorization.Authenticate(requestMessage, authParametersValues);
            return requestMessage;
        }
    }
}
