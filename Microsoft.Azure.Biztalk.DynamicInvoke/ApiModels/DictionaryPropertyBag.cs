//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Property accessor that wraps an IDictionary[string, object]
    /// </summary>
    class DictionaryPropertyBag : PropertyBag
    {
        private readonly IDictionary<string, object> dictionary;

        public DictionaryPropertyBag(IDictionary<string, object> dict)
        {
            this.dictionary = dict;
        }

        public override object Object 
        {
            get
            {
                return this.dictionary; 
            }
        }

        public override object this[string propertyName]
        {
            get
            {
                return this.dictionary[propertyName];
            }
        }

        public override bool ContainsKey(string propertyName)
        {
            return this.dictionary.ContainsKey(propertyName);
        }

        public override PropertyBag GetBag(string propertyName)
        {
            if (this.dictionary.ContainsKey(propertyName))
            {
                return PropertyBag.Create(this.dictionary[propertyName]);
            }

            throw new KeyNotFoundException(
                string.Format(CultureInfo.CurrentCulture, "Cannot create property bag for property \"{0}\" which is not in the bag", propertyName));
        }
    }
}
