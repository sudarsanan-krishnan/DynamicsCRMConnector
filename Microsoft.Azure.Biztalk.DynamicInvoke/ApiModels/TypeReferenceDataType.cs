//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System;
    using System.Collections.Generic;
    
    /// <summary>
    /// This class represents a reference to another
    /// named data type by reference. It can't be
    /// used to serialize directly.
    /// </summary>
    public class TypeReferenceDataType : DataType
    {
        private readonly string dataType;

        public TypeReferenceDataType(string typeRef)
        {
            this.dataType = typeRef;
        }

        public override string Name
        {
            get { return this.dataType; }
        }

        public override IEnumerable<KeyValuePair<string, DataType>> Properties
        {
            get { throw new InvalidOperationException("Cannot get properties from an unresolved type reference"); }
        }

        protected override object GetValue(PropertyBag propertyValues)
        {
            throw new InvalidOperationException("Cannot serialize value from an unresolved type reference");
        }
    }
}
