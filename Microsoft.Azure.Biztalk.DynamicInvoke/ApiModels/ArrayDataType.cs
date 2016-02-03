//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    public class ArrayDataType : DataType
    {
        public ArrayDataType(DataType itemType)
        {
            this.ItemType = itemType;
        }

        public DataType ItemType { get; private set; }

        public override string Name
        {
            get { return "array of [" + this.ItemType.Name + "]"; }
        }

        public override IEnumerable<KeyValuePair<string, DataType>> Properties
        {
            get { return Enumerable.Empty<KeyValuePair<string, DataType>>(); }
        }

        protected override object GetValue(PropertyBag propertyValues)
        {
            if (propertyValues != null)
            {
                JArray array = JArray.Parse(propertyValues.Object.ToString());
                if (array != null)
                {
                    return array;
                }
                else
                {
                    if (propertyValues.Object is IEnumerable && !(propertyValues.Object is string))
                    {
                        var items = ((IEnumerable)propertyValues.Object).Cast<object>();
                        return new JArray(items.Select(item => this.ItemType.GetValue(PropertyBag.Create(item))));
                    }
                }

                return new JArray(this.ItemType.GetValue(propertyValues));
            }
            else
            {
                return null;
            }
        }
    }
}
