//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections.Generic;
    using System.Linq;

    public class MapDataType : DataType
    {
        public MapDataType(DataType additionalPropertiesType)
        {
            this.AdditionalPropertiesType = additionalPropertiesType;
        }

        public DataType AdditionalPropertiesType { get; private set; }

        public override string Name
        {
            get { return "map of <string," + this.AdditionalPropertiesType.Name + ">"; }
        }

        public override IEnumerable<KeyValuePair<string, DataType>> Properties
        {
            get { return Enumerable.Empty<KeyValuePair<string, DataType>>(); }
        }

        protected override object GetValue(PropertyBag propertyValues)
        {
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
