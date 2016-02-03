//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections;
    using System.Collections.Generic;
    
    /// <summary>
    /// Factory for property bag objects that let us retrieve
    /// values from either a dictionary or an arbitrary object
    /// </summary>
    public abstract class PropertyBag
    {
        /// <summary>
        /// Get the raw object that this property bag is wrapping.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Object", Justification = "Inherited code - no side effect because of this")]
        public abstract object Object 
        {
            get; 
        }

        /// <summary>
        /// Access the value of the given property.
        /// </summary>
        /// <param name="propertyName">Name of property to retrieve</param>
        /// <returns>The property value, throws KeyNotFoundException if invalid name.</returns>
        public abstract object this[string propertyName]
        {
            get;
        }

        /// <summary>
        /// Factory for IPropertyAccessor instances. Creates the proper
        /// implementation based on if it's a dictionary or not.
        /// </summary>
        /// <param name="props">property object to access</param>
        /// <returns>wrapping property accessor.</returns>
        public static PropertyBag Create(object props)
        {
            return AlreadyABag(props) ??
                AsStringObjectDictionaryBag(props) ??
                AsFlatStringDictionaryBag(props) ??
                AsObjectPropertyBag(props);
        }

        /// <summary>
        /// Does this object have the named property?
        /// </summary>
        /// <param name="propertyName">Name to look for</param>
        /// <returns>true if property is available, false if not.</returns>
        public abstract bool ContainsKey(string propertyName);

        /// <summary>
        /// Get a new bag wrapping the property named in
        /// <paramref name="propertyName"/>
        /// </summary>
        /// <param name="propertyName">The property name to get a bag for.</param>
        /// <returns>New property bag instance.</returns>
        public abstract PropertyBag GetBag(string propertyName);

        private static PropertyBag AsFlatStringDictionaryBag(object props)
        {
            var dict = props as IDictionary<string, string>;
            return dict == null ? null : new FlatDictionaryPropertyBag(dict);
        }

        private static PropertyBag AlreadyABag(object props)
        {
            return props as PropertyBag;
        }

        private static PropertyBag AsStringObjectDictionaryBag(object props)
        {
            var dict = props as IDictionary<string, object>;
            return dict == null ? null : new DictionaryPropertyBag(dict);
        }

        private static PropertyBag AsObjectPropertyBag(object props)
        {
            return new ObjectPropertyBag(props);
        }
    }
}
