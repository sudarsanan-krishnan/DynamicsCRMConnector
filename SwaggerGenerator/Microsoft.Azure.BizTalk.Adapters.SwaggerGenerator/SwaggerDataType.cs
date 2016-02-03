//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System.Collections.Generic;

    public class SwaggerDataType
    {
        private string type, id, reference;
        private IDictionary<string, SwaggerDataType> properties = new Dictionary<string, SwaggerDataType>();

        public string Type
        {
            get { return this.type; }
            set { this.type = Helper.SanitizeName(value); }
        }

        public string Id
        {
            get { return this.id; }
            set { this.id = Helper.SanitizeName(value); }
        }

        public string Ref
        {
            get { return this.reference; }
            set { this.reference = Helper.SanitizeName(value); }
        }

        public void AddProperty(string name, SwaggerDataType dataType)
        {
            this.properties.Add(Helper.SanitizeName(name), dataType);
        }

        public IDictionary<string, SwaggerDataType> GetProperties()
        {
            return this.properties;
        }
    }
}
