//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Class modelling a data type as defined by the API.
    /// </summary>
    public abstract class DataType
    {
        /// <summary>
        /// Name identifying the data type.
        /// </summary>
        public abstract string Name 
        {
            get; 
        }

        /// <summary>
        /// Get the contained properties of this data tye,
        /// if any.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Inherited code - by design")]
        public abstract IEnumerable<KeyValuePair<string, DataType>> Properties 
        {
            get;
        }

        /// <summary>
        /// Returns the list of property names contained by this data type.
        /// If this is a scalar property (the default), a single
        /// empty string is returned.
        /// </summary>
        public virtual IEnumerable<string> PropertyNames
        {
            get 
            {
                yield return string.Empty;
            }
        }

        /// <summary>
        /// Retrieve a value of this type as an object
        /// given a property bag (possibly nested).
        /// </summary>
        /// <param name="propertyValues">set of properties.</param>
        /// <returns>The value.</returns>
        public object GetValue(object propertyValues)
        {
            return GetValue(PropertyBag.Create(propertyValues));
        }

        protected abstract object GetValue(PropertyBag propertyValues);
    }
}