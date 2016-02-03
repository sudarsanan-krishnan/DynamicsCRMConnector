using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Description;
using System.Xml.Schema;
using Microsoft.Azure.AppService.ApiApps.Service;
using Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator;
using Microsoft.Integration.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using WebActivatorEx;

using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System.Configuration;
using CrmTypes = Microsoft.Crm.Sdk.Types;

using DynamicsCRMConnector.Models;

namespace DynamicsCRMConnector.Controllers
{
    public class CRMEntityController : ApiController
    {
        private async Task<HttpResponseMessage> RetryAsync(Func<Task<HttpResponseMessage>> action)
        {
            HttpResponseMessage response = null;
           
            response = await action();
                
            return response;
        }

        /// <summary>
        /// Overriding Swashbuckle API to get the dynamic Swagger
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "Reviewed"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Reviewed"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Reviewed"), HttpGet]
        [Route("swagger/docs/{version}")]
        public async Task<HttpResponseMessage> GetSwagger(string version = "v1")
        {
                DynamicSwaggerHelper helper = new DynamicSwaggerHelper();
                //Logger.LogMessage(this.Request, true, "Inside GetSwagger. Generating Dynamic Swagger now.");

                return await this.RetryAsync(async () =>
                {
                    //await this.Initialize();
                    return helper.GetJsonSwagger(this.Request, "1.0");
                });
        }

        [HttpPost]
        [Route("api/{entityName}")]
        public void Create(string entityName, [FromBody] JObject objStructure)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString;
            
            CrmConnection crmConnection = CrmConnection.Parse(connectionString);
            using (OrganizationService service = new OrganizationService(crmConnection))
            {
                if (objStructure != null)
                {
                    Entity entity = new Entity(entityName);
                    Guid guidValue;

                    foreach (var attribute in objStructure)
                    {
                        if (Guid.TryParse(attribute.Value.ToString(), out guidValue))
                        {
                            RetrieveAttributeRequest attributeRequest = new RetrieveAttributeRequest
                            {
                                EntityLogicalName = entityName,
                                LogicalName = attribute.Key.ToString(),
                                RetrieveAsIfPublished = true
                            };

                            // Execute the request
                            RetrieveAttributeResponse attributeResponse =
                                (RetrieveAttributeResponse)service.Execute(attributeRequest);

                            if (attributeResponse.AttributeMetadata.AttributeType == AttributeTypeCode.Lookup)
                            {
                                string relatedEntityName =
                                    ((LookupAttributeMetadata)(attributeResponse.AttributeMetadata)).Targets[0];

                                EntityReference eref = new EntityReference(relatedEntityName, guidValue);
                                entity[attribute.Key.ToString()] = eref;
                                continue;
                            }
                        }

                        entity[attribute.Key.ToString()] = attribute.Value.ToString();
                    }

                    service.Create(entity);
                }
            }
        }
    }
}
