//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class FormOperationParameter : IOperationParameter
    {
        private readonly DataType operationDataType;

        public FormOperationParameter(DataType dataType, bool isRequired)
        {
            operationDataType = dataType;
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
                return operationDataType.Name; 
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
                return operationDataType.PropertyNames;
            }
        }

        public object GetValue(object propertyValues)
        {
            PropertyBag bag = PropertyBag.Create(propertyValues);
            if (bag.ContainsKey("Body"))
            {
                return bag["Body"];
            }

            return string.Empty;            
        }
    }
}
