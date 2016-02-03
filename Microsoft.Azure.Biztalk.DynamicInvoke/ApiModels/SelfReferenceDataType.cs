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
    /// A property of a recursive data type
    /// that references the data type in a
    /// property - for example, a tree structure.
    /// </summary>
    class SelfReferenceDataType : DataType
    {
        public override string Name
        {
            get
            {
                return string.Empty; 
            }
        }

        public override IEnumerable<KeyValuePair<string, DataType>> Properties
        {
            get 
            {
                throw new InvalidOperationException("Cannot get properties from an unresolved self reference"); 
            }
        }

        protected override object GetValue(PropertyBag propertyValues)
        {
            throw new InvalidOperationException("Cannot get value from an unresolved self reference");
        }
    }
}
