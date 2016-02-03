//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System.Collections.Generic;
    using System.Xml.Schema;

    public class OperationSchema
    {
        private IList<XmlSchema> schemas = new List<XmlSchema>();

        public string OperationName { get; set; }

        public string OperationSubpath { get; set; }

        public IList<XmlSchema> Schemas
        {
            get { return schemas; }
        }

        public string RequestNamespace { get; set; }

        public string RequestElement { get; set; }

        public string ResponseNamespace { get; set; }

        public string ResponseElement { get; set; }
    }
}
