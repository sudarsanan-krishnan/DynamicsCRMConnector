//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections.Generic;
    using System.Web;

    public class QueryStringOperationParameter : IOperationParameter
    {
        public QueryStringOperationParameter(string name, string type, bool isRequired, string defaultValue="")
        {
            Name = name;
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

        public bool IsRequired
        { 
            get;
            private set; 
        }

        public string DefaultValue
        {
            get;
            private set;
        }

        public OperationParameterType ParameterType 
        {
            get
            {
                return OperationParameterType.Query; 
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
            if (accessor.ContainsKey(Name) && accessor[Name] != null)
            {
                return accessor[Name].ToString();
                //return HttpUtility.UrlEncode(accessor[Name].ToString());
            }

            return null;
        }
    }
}
