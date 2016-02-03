//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web;

    /// <summary>
    /// Operation Parameter that maps into the
    /// URI path.
    /// </summary>
    public class PathOperationParameter : IOperationParameter
    {
        public PathOperationParameter(string parameterName, string parameterType, bool isRequired, string defaultValue="")
        {
            Name = parameterName;
            Type = parameterType;
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
                return OperationParameterType.Path; 
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
            try
            {
                var accessor = PropertyBag.Create(propertyValues);
                return HttpUtility.UrlPathEncode(accessor[Name].ToString());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format(CultureInfo.CurrentCulture, "Required Path Parameter \"{0}\" has no value", Name), ex);
            }
        }
    }
}
