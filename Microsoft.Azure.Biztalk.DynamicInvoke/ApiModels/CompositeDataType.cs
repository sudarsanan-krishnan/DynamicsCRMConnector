//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Model for a data type made up of a set of properties, each of
    /// which has a name and a DataType.
    /// </summary>
    public class CompositeDataType : DataType
    {
        private readonly string name;
        private readonly List<KeyValuePair<string, DataType>> properties;
        private readonly List<string> requiredProperties;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Inherited code - by design")]
        public CompositeDataType(string nameArgument, IEnumerable<KeyValuePair<string, DataType>> propertiesArgument, IEnumerable<string> required)
        {
            this.properties = propertiesArgument
                .Select(prop => prop.Value is SelfReferenceDataType ? 
                    new KeyValuePair<string, DataType>(prop.Key, this) :
                    prop)
                .ToList();
            this.name = nameArgument;
            this.requiredProperties = required.ToList();
        }

        public override string Name
        {
            get 
            {
                return this.name; 
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies", Justification = "not a precompiled assembly")]
        public override IEnumerable<KeyValuePair<string, DataType>> Properties
        {
            get 
            {
                return this.properties.AsReadOnly();
            }
        }

        /// <summary>
        /// Get the list of required properties of this data type,
        /// if any.
        /// </summary>
        public IList<string> Required 
        {
            get 
            {
                return this.requiredProperties; 
            } 
        }

        public override IEnumerable<string> PropertyNames
        {
            get
            {
                foreach (var kvp in this.properties)
                {
                    string propName = kvp.Key;
                    DataType propertyType = kvp.Value;
                    foreach (string n in propertyType.PropertyNames)
                    {
                        yield return propName + (string.IsNullOrEmpty(n) ? string.Empty : ".") + n;
                    }
                }
            }
        }

        protected override object GetValue(PropertyBag propertyValues)
        {
            return new JObject(this.properties.Select(p => GetJProperty(p, propertyValues)).Where(p => p != null));
        }

        private static JProperty GetJProperty(KeyValuePair<string, DataType> sourceProperty, PropertyBag propertyValues)
        {
            string propertyName = sourceProperty.Key;
            if (propertyValues.ContainsKey(propertyName))
            {
                var propertyValue = sourceProperty.Value.GetValue(propertyValues.GetBag(propertyName));

                if (propertyValue != null)
                {
                    if (!(propertyValue is JToken))
                    {
                        propertyValue = new JValue(propertyValue);
                    }

                    return new JProperty(propertyName, propertyValue);
                }
            }

            return null;
        }
    }
}
