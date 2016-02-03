//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    /// <summary>
    /// Implementation of <see cref="PropertyBag"/> that
    /// wraps an individual object and retrieves values via
    /// reflection.
    /// </summary>
    class ObjectPropertyBag : PropertyBag
    {
        private const BindingFlags PublicProperties = BindingFlags.Instance | BindingFlags.Public;        
        private readonly object objectProperties;

        public ObjectPropertyBag(object props)
        {
            this.objectProperties = props;
        }

        public override object Object
        {
            get
            { 
                return this.objectProperties;
            } 
        }

        public override object this[string propertyName]
        {
            get
            {
                PropertyInfo property = this.objectProperties.GetType().GetProperty(propertyName, PublicProperties);
                return this.GetPropertyValue(property, propertyName);
            }
        }

        public override bool ContainsKey(string propertyName)
        {
            if (this.objectProperties.GetType().GetProperty(propertyName, PublicProperties) != null)
            {
                return true;
            }

            return false;
        }

        public override PropertyBag GetBag(string propertyName)
        {
            return PropertyBag.Create(this[propertyName]);
        }

        private object GetPropertyValue(PropertyInfo property, string propertyName)
        {
            if (property == null)
            {
                throw new KeyNotFoundException(string.Format(CultureInfo.CurrentCulture, "Property {0} is not present", propertyName));
            }

            return property.GetValue(this.objectProperties);
        }
    }
}
