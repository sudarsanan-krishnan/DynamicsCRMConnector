//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Property bag that wraps an IDictionary{string, string}
    /// Settings on subobjects are "flattened" into an "a.b.c"
    /// style naming, separated by '.' characters. For arrays,
    /// elements are indicated by "a.0.b.3" style.
    /// </summary>
    /// <remarks>
    /// Arrays aren't actually implemented yet, so support
    /// doesn't work right now. Will need to revisit later.
    /// </remarks>
    public class FlatDictionaryPropertyBag : PropertyBag
    {
        private readonly IDictionary<string, string> dictionary;
        private readonly string propertyBagPrefix;

        public FlatDictionaryPropertyBag(IDictionary<string, string> map) : this(map, null)
        {            
        }

        private FlatDictionaryPropertyBag(IDictionary<string, string> map, string prefix)
        {
            this.dictionary = map;
            this.propertyBagPrefix = prefix;
        }
       
        public override object Object
        {
            get
            {
                throw new InvalidOperationException("Can't get underlying object for flat dictionary"); 
            }
        }

        public override object this[string propertyName]
        {
            get
            {
                return this.dictionary[this.RealPropertyName(propertyName)];
            }
        }

        public override bool ContainsKey(string propertyName)
        {
            string rootName = this.RealPropertyName(propertyName);
            return this.dictionary.Keys.Any(k => k.StartsWith(rootName, StringComparison.OrdinalIgnoreCase));
        }

        public override PropertyBag GetBag(string propertyName)
        {
            string subName = this.RealPropertyName(propertyName);
            var subKeys = this.dictionary.Keys.Where(k => k.StartsWith(subName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (subKeys.Count == 1 && subKeys[0] == subName)
            {
                // single scalar value, wrap it in another property bag
                return PropertyBag.Create(this.dictionary[subName]);
            }

            if (subKeys.Count == 0)
            {
                return PropertyBag.Create(new { });
            }

            return new FlatDictionaryPropertyBag(this.dictionary, subName);
        }

        private string RealPropertyName(string propertyName)
        {
            return string.IsNullOrEmpty(this.propertyBagPrefix)
                ? propertyName
                : this.propertyBagPrefix + "." + propertyName;
        }
    }
}
