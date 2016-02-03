//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class represents a single operation on a REST API
    /// </summary>
    public class Operation
    {
        private readonly List<IOperationParameter> operationParameters;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "inherited code - Its okay to be not strongly typed here. The CITs will catch the code issues")]
        public Operation(string name, string returnType, string uriTemplate, HttpMethod method, OperationAuthorization authorization,
            IEnumerable<IOperationParameter> parameters, IEnumerable<HttpStatusCode> expectedStatusCodes)
            : this(name, returnType, uriTemplate, method, authorization, parameters, expectedStatusCodes, string.Empty)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "inherited code - Its okay to be not strongly typed here. The CITs will catch the code issues")]
        public Operation(string name, string returnType, string uriTemplate, HttpMethod method, OperationAuthorization authorization,
            IEnumerable<IOperationParameter> parameters, IEnumerable<HttpStatusCode> expectedStatusCodes, string refType)
        {
            GuardNotNull(name, "name");
            GuardNotNull(uriTemplate, "uriTemplate");
            GuardNotNull(method, "method");

            Name = name;
            ReturnType = returnType;
            UriTemplate = uriTemplate;
            Method = method;
            Authorization = authorization ?? new UnauthenticatedOperationAuthorization();
            ExpectedStatusCodes = expectedStatusCodes.ToList();
            Ref = refType;
            if (parameters == null)
            {
                operationParameters = new List<IOperationParameter>();
            }
            else
            {
                operationParameters = parameters.ToList();
            }
        }

        /// <summary>
        /// Name of the operation
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Return Type of the operation
        /// </summary>
        public string ReturnType
        {
            get;
            private set;
        }

        /// <summary>
        /// The Uri template used to create requests to this
        /// operation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "inherited code - Its okay to be not strongly typed here. The CITs will catch the code issues")]
        public string UriTemplate
        {
            get;
            private set;
        }

        /// <summary>
        /// The HTTP method used for this operation.
        /// </summary>
        public HttpMethod Method
        {
            get;
            private set;
        }

        /// <summary>
        /// The positive/negative Status Codes that are expected for this operation
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Used by test code")]
        public List<HttpStatusCode> ExpectedStatusCodes
        {
            get;
            private set;
        }

        /// <summary>
        /// In case the Operation's ReturnType is 'array', Ref provides us with the JOBject that the array comprises of 
        /// </summary>
        public string Ref
        {
            get;
            private set;
        }

        /// <summary>
        /// Authorization object for this operation
        /// </summary>
        public OperationAuthorization Authorization
        {
            get;
            private set;
        }

        /// <summary>
        /// The parameters for this operation
        /// </summary>
        public IEnumerable<IOperationParameter> Parameters
        {
            get { return operationParameters.AsReadOnly(); }
        }

        /// <summary>
        /// Get a flattened list of the parameter names, these are
        /// the keys that would be used in a parameter dictionary
        /// when constructing a 
        /// </summary>
        public IEnumerable<string> ParameterNames
        {
            get
            {
                return operationParameters.SelectMany(p => p.FlattenedNames);
            }
        }

        /// <summary>
        /// Get the path of the uri for this operation.
        /// </summary>
        /// <param name="parameterValues">The values for various parameters.</param>
        /// <returns>The path string.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings", Justification = "inherited code - Its okay to be not strongly typed here. The CITs will catch the code issues")]
        public string GetUriPath(object parameterValues)
        {
            string path = UriTemplate;
            foreach (var parameter in operationParameters.Where(p => p.ParameterType == OperationParameterType.Path))
            {
                path = path.Replace("{" + parameter.Name + "}", parameter.GetValue(parameterValues).ToString());
            }

            return path;
        }

        /// <summary>
        /// Get the query parameter string part of the uri for this operation.
        /// </summary>
        /// <param name="parameterValues">The values for various parameters.</param>
        /// <returns>The query string (not including the leading ?)</returns>
        public string GetQueryParameters(object parameterValues, bool urlEncodeSpecialCharacters = false)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var parameter in operationParameters.Where(p => p.ParameterType == OperationParameterType.Query))
            {
                object obj = parameter.GetValue(parameterValues);
                if (obj != null)
                {
                    queryString[parameter.Name] = obj as string;
                }
            }

            if (urlEncodeSpecialCharacters)
            {
                // ParseQueryString helps in url encoding of the query string but it has gaps with respect to 
                // certain characters specially for twitter connector. Hence the need for UrlEncodeSpecialCharacters().
                // urlEncodeSpecialCharacters flag is set to true only for twitter connector.
                return UrlEncodeSpecialCharacters(queryString.ToString());
            }

            return queryString.ToString();
        }

        public string GetRequestBody(object parameterValues)
        {
            // TODO: Verify there's only one body parameter?
            if (parameterValues != null)
            {
                var bodyParameter = operationParameters.FirstOrDefault(p => p.ParameterType == OperationParameterType.Body);
                if (bodyParameter != null)
                {
                    if (bodyParameter is FormOperationParameter)
                    {
                        JObject obj = JObject.Parse(parameterValues.ToString());
                        IDictionary<string, string> bodyArguments = new Dictionary<string, string>();
                        foreach (var x in obj)
                        {
                            string name = x.Key;
                            JToken value = x.Value;
                            bodyArguments.Add(name, value.ToString());
                        }

                        parameterValues = bodyArguments;
                        return bodyParameter.GetValue(parameterValues).ToString();
                    }

                    return parameterValues.ToString();
                }
            }

            return null;
        }

        private static void GuardNotNull<T>(T argValue, string argName) where T : class
        {
            if (argValue == null)
            {
                throw new ArgumentException("Null value passed for argument", argName);
            }
        }

        private static string UrlEncodeSpecialCharacters(string originalString)
        {
            return originalString.Replace("!", "%21").Replace("(", "%28").Replace(")", "%29");
        }
    }
}
