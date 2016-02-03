//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections;
    using System.Collections.Generic;
    
    /// <summary>
    /// Enum marking where in the request each
    /// operation parameter should end up.
    /// </summary>
    public enum OperationParameterType
    {
        /// <summary>
        /// Default value, unknown type
        /// </summary>
        None = 0,

        /// <summary>
        /// This parameter maps to the URI path
        /// </summary>
        Path,

        /// <summary>
        /// This parameter maps to a query string parameter
        /// </summary>
        Query,

        /// <summary>
        /// This parameter is mapped to a request header
        /// </summary>
        Header,

        /// <summary>
        /// This parameter defines the body of a request
        /// </summary>
        Body,

        /// <summary>
        /// This parameter is part of form data
        /// </summary>
        Form
    }
    
    /// <summary>
    /// A direct parameter on a REST API operation
    /// </summary>
    public interface IOperationParameter
    {
        /// <summary>
        /// The name of the parameter
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The type indicator from the API metadata.
        /// Could be either a well known type like
        /// "string" or a reference to a model in the
        /// API.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Default Value of a Parameter
        /// </summary>
        string DefaultValue { get; }

        /// <summary>
        /// What kind of parameter this is
        /// </summary>
        OperationParameterType ParameterType { get; }

        /// <summary>
        /// Get a collection of the names used by this parameter.
        /// This would be the key (or keys) used to retrieve
        /// this parameter's values from an arguments dictionary.
        /// </summary>
        IEnumerable<string> FlattenedNames { get; }

        /// <summary>
        /// Flag to indicate if the Parameter is required
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        /// Retrieve the value for this parameter from
        /// the property bag.
        /// </summary>
        /// <param name="propertyValues">The property bag to get the
        /// value from.</param>
        /// <returns>The value from the property back. Returns
        /// null if the value is not in the property bag.</returns>
        object GetValue(object propertyValues);
    }
}
