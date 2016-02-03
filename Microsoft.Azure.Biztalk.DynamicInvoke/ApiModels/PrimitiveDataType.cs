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

    /// <summary>
    /// Data type for a primitive value, such as a string or integer.
    /// </summary>
    public class PrimitiveDataType : DataType
    {
        private readonly string dataTypeName;
        private string dataTypeDefaultValue;

        public PrimitiveDataType(string name, string defaultValue = "")
        {
            this.dataTypeName = name;
            this.dataTypeDefaultValue = defaultValue;
        }

        public override string Name
        {
            get { return this.dataTypeName; }
        }

        public string DefaultValue
        {
            get { return this.dataTypeDefaultValue; }
            set { this.dataTypeDefaultValue = value; }
        }

        public override IEnumerable<KeyValuePair<string, DataType>> Properties
        {
            get { return Enumerable.Empty<KeyValuePair<string, DataType>>(); }
        }

        protected override object GetValue(PropertyBag propertyValues)
        {
            // in this case, the value is just the entire property object.
            if (propertyValues != null)
            {
                return propertyValues.Object;
            }
            else
            {
                return null;
            }
        }
    }
}
