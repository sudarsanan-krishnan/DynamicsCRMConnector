//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections.Generic;

    public class HeaderOperationParameter : IOperationParameter
    {
        public HeaderOperationParameter(string headerName, string type, bool isRequired, string defaultValue="")
        {
            Name = headerName;
            Type = type;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
        }

        public string Name 
        { 
            get;
            private set; 
        }

        public string Type 
        {
            get; 
            private set; 
        }

        public string DefaultValue
        {
            get;
            private set;
        }

        public bool IsRequired 
        { 
            get;
            private set;
        }

        public OperationParameterType ParameterType
        {
            get 
            { 
                return OperationParameterType.Header;
            }
        }

        public IEnumerable<string> FlattenedNames
        { 
            get 
            {
                yield return Name;
            } 
        }

        public object GetValue(object propertyValues)
        {
            var accessor = PropertyBag.Create(propertyValues);
            if (accessor.ContainsKey(Name))
            {
                return accessor[Name].ToString();
            }

            return null;
        }
    }
}
