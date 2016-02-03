//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class representing parameters in the body of an operation.
    /// This represents the entire body, the data type provides
    /// the actual parameter names.
    /// </summary>
    public class BodyOperationParameter : IOperationParameter
    {
        private readonly DataType dataType;

        public BodyOperationParameter(DataType dataTypeArgument, bool isRequired)
        {
            dataType = dataTypeArgument;
            IsRequired = isRequired;
        }

        public string Name
        {
            get 
            { 
                return null; 
            }
        }

        public string Type 
        {
            get 
            {
                return dataType.Name; 
            }
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
                return OperationParameterType.Body;
            }
        }

        public IEnumerable<string> FlattenedNames
        {
            get
            {
                return dataType.PropertyNames;
            }
        }

        public object GetValue(object propertyValues)
        {
            return dataType.GetValue(propertyValues);
        }
    }
}
