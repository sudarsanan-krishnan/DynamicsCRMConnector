using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DynamicsCRMConnector
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Sdk.Messages;

    class RetrieveMetadata
    {
        protected Dictionary<string, SortedDictionary<string, AttributeMetadata>> EntityAttributes;
        protected IOrganizationService OrganizationService;

        public RetrieveMetadata(IOrganizationService OrganizationService)
        {
            this.OrganizationService = OrganizationService;
            this.EntityAttributes = new Dictionary<string, SortedDictionary<string, AttributeMetadata>>();
        }

        public SortedDictionary<string, AttributeMetadata> GetAttributes(string entityLogicalName)
        {
            SortedDictionary<string, AttributeMetadata> attributes;

            if (this.EntityAttributes.ContainsKey(entityLogicalName))
            {
                this.EntityAttributes.TryGetValue(entityLogicalName, out attributes);
            }
            else
            {
                attributes = new SortedDictionary<string, AttributeMetadata>();

                RetrieveEntityRequest request = new RetrieveEntityRequest()
                {
                    EntityFilters = EntityFilters.All,
                    LogicalName = entityLogicalName
                };
                RetrieveEntityResponse response = (RetrieveEntityResponse)this.OrganizationService.Execute(request);

                EntityMetadata entityMetadata = response.EntityMetadata;
                foreach (AttributeMetadata currentAttribute in entityMetadata.Attributes)
                {
                    //if (currentAttribute.AttributeOf == null)
                    //{
                    attributes.Add(currentAttribute.SchemaName, currentAttribute);
                    //}
                }

                this.EntityAttributes.Add(entityLogicalName, attributes);
            }

            return attributes;
        }
    }
}