using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DynamicsCRMConnector.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Configuration;
    using System.Web.Http;
    using System.Xml;
    using Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator;
    using Microsoft.Integration.Common;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.Application;
    using Swashbuckle.Swagger;
    using Swashbuckle.Swagger.Filters;

    using Microsoft.Xrm.Client;
    using Microsoft.Xrm.Client.Services;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Sdk.Query;
    using System.Configuration;
    using CrmTypes = Microsoft.Crm.Sdk.Types;

    /// <summary>
    /// 
    /// </summary>
    public class DynamicSwaggerHelper
    {
        IList<string> entities;

        /// <summary>
        /// This method gets the modified JSON swagger 
        /// </summary>
        /// <param name="request">Required</param>
        /// <param name="accessToken">Required</param>
        /// <param name="instance">Required</param>
        /// <param name="version">Required</param>
        /// <param name="apiversion">Required</param>
        /// <returns></returns>
        public HttpResponseMessage GetJsonSwagger(HttpRequestMessage request, string version)
        {
            SwaggerDocument apideclaration = this.GetApiDeclaration(request, version);

            Dictionary<string, PathItem> apilist = new Dictionary<string, PathItem>();
            HttpResponseMessage response = new HttpResponseMessage();

            entities = GetEntityList();

            if (entities == null)
            {
                Logger.LogError(request, false, "Entities not set in Application Settings");
                throw new ArgumentException("Entity list is empty");
            }

            int entityCount = entities.Count;

            for (int i = 0; i < entityCount; i++)
            {
                // This section does all the modifying swagger work
                apilist = apilist.Concat(AddApi(apideclaration.paths, i)).ToDictionary(x => x.Key, x => x.Value);
                AddModels(apideclaration.definitions, i); //, requiredItems, properties, 0);
            }

            //Clearing out the older APIs and replacing them with entity specific APIs
            apideclaration.paths.Clear();
            apideclaration.paths = apilist;

            // return json response        
            HttpResponseMessage swaggerResponse = new HttpResponseMessage();
            string jsonSwagger = JsonConvert.SerializeObject(apideclaration, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new[] { new VendorExtensionsConverter() }
            });

            swaggerResponse.Content = new StringContent(jsonSwagger, Encoding.Default, "application/json");

            //Logger.LogMessage(request, false, "Dynamic Swagger generated.");
            return swaggerResponse;
        }

        internal static SwaggerGenerator GetSwaggerProvider(HttpRequestMessage swaggerRequest, string version)
        {
            var httpConfig = swaggerRequest.GetConfiguration();
            List<ISchemaFilter> schemaFilters = new List<ISchemaFilter>();
            schemaFilters.Add(new SummaryAndVisibilityModelFilter());

            List<IOperationFilter> operationFilters = new List<IOperationFilter>();
            operationFilters.Add(new HandleFromUriParams());
            operationFilters.Add(new OperationNameAndDefaultResponseFilter());
            operationFilters.Add(new SummaryAndVisibilityOperationFilter());
            //operationFilters.Add(new AddDefaultsOpFilter());

            VersionInfoBuilder versionInfoBuilder = new VersionInfoBuilder();
            versionInfoBuilder.Version(version, "CRM Connector V1");
            IDictionary<string, Info> apiVersions = versionInfoBuilder.Build();

            List<string> schemes = new List<string>();
            schemes.Add("http");
            var securitySchemeBuilders = new Dictionary<string, SecurityScheme>();
            var customSchemaMappings = new Dictionary<Type, Func<Schema>>();
            var documentFilters = new List<IDocumentFilter>();

            var options = new SwaggerGeneratorOptions(
                versionSupportResolver: (apidesc, targetapi) => JsonSwaggerGenerator.ResolveVersionSupportByRouteConstraint(apidesc, targetapi),
                schemes: schemes,
                schemaFilters: schemaFilters,
                securityDefinitions: securitySchemeBuilders,
                operationFilters: operationFilters,
                ignoreObsoleteActions: true,
                customSchemaMappings: customSchemaMappings,
                documentFilters: documentFilters,
                groupingKeyComparer:
                null,
                groupingKeySelector:
                null);

            return new SwaggerGenerator(
                httpConfig.Services.GetApiExplorer(),
                httpConfig.GetJsonContractResolver(),
                apiVersions,
                options);
        }

        private SwaggerDocument GetApiDeclaration(HttpRequestMessage request, string version)
        {
            SwaggerGenerator gen = GetSwaggerProvider(request, version);
            return gen.GetSwagger(JsonSwaggerGenerator.DefaultRootUrlResolver(request), version);
        }
        
        private static IList<string> GetEntityList()
        {
            var entities = WebConfigurationManager.AppSettings["entityNames"];

            IList<string> stringList = GetListFromString(entities);

            if (stringList != null)
            {
                for (int i = 0; i < stringList.Count; i++)
                {
                    stringList[i] = stringList[i].Trim();
                }

                return stringList;
            }

            return null;
        }

        private static IList<string> GetListFromString(string entities)
        {
            if (!string.IsNullOrEmpty(entities))
            {
                string[] entityList = entities.Split(',');
                return entityList.ToList();
            }

            return null;
        }
        
        private Dictionary<string, PathItem> AddApi(IDictionary<string, PathItem> list, int index)
        {
            Dictionary<string, PathItem> listApi = new Dictionary<string, PathItem>();
            var entityName = entities[index];

            foreach (var apiPath in list.Keys)
            {
                var api = list[apiPath];

                if (!apiPath.Contains("/swagger/docs/{version}") && !apiPath.Contains("/api/query"))
                {
                    var originalApiTuple = new Tuple<string, PathItem>(apiPath, api);
                    var newApi = GetApi(originalApiTuple, entityName);
                    var newApiOperations = newApi.Item2;

                    if (newApiOperations.get != null || newApiOperations.put != null ||
                        newApiOperations.post != null || newApiOperations.delete != null ||
                        newApiOperations.head != null || newApiOperations.options != null
                        || newApiOperations.patch != null)
                    {
                        if (!listApi.ContainsKey(newApi.Item1))
                        {
                            listApi.Add(newApi.Item1, newApi.Item2);
                        }
                    }
                }
            }

            return listApi;
        }

        private static Tuple<string, PathItem> GetApi(Tuple<string, PathItem> api, string entityName)
        {
            string originalApiPath = api.Item1;
            PathItem originalApi = api.Item2;
            PathItem perEntityApiPath = Clone<PathItem>(originalApi);

            string newApiPath = string.Empty;

            // Adding the sfobject type to the API. This makes the API entity specific
            newApiPath = originalApiPath.Replace("{entityName}", entityName) + "/";

            perEntityApiPath.post = UpdatePostAndDeleteOperations(perEntityApiPath.post, originalApi.post, entityName);

            return new Tuple<string, PathItem>(newApiPath, perEntityApiPath);
        }

        private static Operation UpdatePostAndDeleteOperations(Operation operation, Operation originalOperation, string entityName)
        {
            if (operation == null)
            {
                return operation;
            }

            operation.summary = originalOperation.operationId + ' ' + entityName;
            operation.operationId = originalOperation.operationId + entityName;
            
            Parameter paramToRemove = new Parameter();

            foreach (Parameter p in operation.parameters)
            {
                Parameter originalParameter = originalOperation.parameters.FirstOrDefault(q => q.name == p.name);
                if (p.@in.Equals("path", StringComparison.CurrentCultureIgnoreCase) && p.name.Equals("entityName", StringComparison.CurrentCultureIgnoreCase))
                {
                    paramToRemove = p;
                }

                if (p.@in.Equals("body", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (operation.operationId.Equals("Create" + entityName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        //Changed the body paramter to point to a model which lists all the columns 
                        //of the entity along with the required ones (required and editableColumns)
                        UpdatePostParameters(p, originalParameter, entityName);
                    }
                    else if (operation.operationId.Equals("Update" + entityName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        UpdatePutParamters(p, originalParameter, entityName);
                    }
                }

                //UpdateParamSummaryAndDescription(p, entityName);
            }

            operation.parameters.Remove(paramToRemove);
            UpdateAPIResponse(ref operation, ref originalOperation);
            return operation;
        }


        private static void UpdatePostParameters(Parameter p, Parameter originalParameter, string entityName)
        {
            if (originalParameter != null && originalParameter.schema != null)
            {
                if (p.schema == null)
                {
                    p.schema = new Schema();
                }

                p.schema.@ref = originalParameter.schema.@ref;
                // make body parameters of operations, entity specific with required columns tagged (required and editableColumns)
                p.schema.@ref = p.schema.@ref.Remove(originalParameter.schema.@ref.LastIndexOf('/')) + "/" + entityName + "Create";
            }
        }

        private static void UpdatePutParamters(Parameter p, Parameter originalParameter, string entityName)
        {
            if (originalParameter != null && originalParameter.schema != null)
            {
                if (p.schema == null)
                {
                    p.schema = new Schema();
                }

                p.schema.@ref = originalParameter.schema.@ref;
                // make body parameters of operations, entity specific (allColumns)
                p.schema.@ref = p.schema.@ref.Remove(originalParameter.schema.@ref.LastIndexOf('/')) + "/" + entityName;
            }
        }

        //This is needed because Cloning did not copy response schemas
        private static void UpdateAPIResponse(ref Operation operation, ref Operation originalOperation)
        {
            if (operation.responses != null)
            {
                // update output type to be entity specific
                foreach (string responseCode in originalOperation.responses.Keys)
                {
                    if ((responseCode.StartsWith("2", StringComparison.OrdinalIgnoreCase) || responseCode == "default") && originalOperation.responses[responseCode].schema != null)
                    {
                        string reference = originalOperation.responses[responseCode].schema.@ref;

                        if (operation.responses[responseCode].schema == null)
                        {
                            operation.responses[responseCode].schema = new Schema();
                        }

                        if (originalOperation.responses[responseCode].schema.items != null)
                        {
                            if (operation.responses[responseCode].schema.items == null)
                            {
                                operation.responses[responseCode].schema.items = new Schema();
                            }

                            operation.responses[responseCode].schema.items.@ref =
                                originalOperation.responses[responseCode].schema.items.@ref;

                            operation.responses[responseCode].schema.type =
                                originalOperation.responses[responseCode].schema.type;
                        }

                        operation.responses[responseCode].schema.@ref = reference;
                    }
                }
            }
        }

        /// <summary>
        /// This method adds all the models required by different APIs
        /// Enitity, EnitityCreate, QueryResponse_Enitity, EnitityRecord.
        /// </summary>
        /// <param name="models"></param>
        /// <param name="index"></param>
        private void AddModels(IDictionary<string, Schema> models, int index)
        {
            //Dictionary<Type, List<string>> requiredItems;
            Dictionary<string, Schema> properties = new Dictionary<string,Schema>();
            GetEntityAttributes(properties, index);

            // Adding the data type with the requried fields set. This is for Create API
            Schema createType = new Schema();
            createType.properties = new Dictionary<string, Schema>();
            //createType.required = new List<string>();

            foreach (string key in properties.Keys)
            {
                if (!createType.properties.ContainsKey(key))
                {
                    createType.properties.Add(key, properties[key]);
                }
            }

            //foreach (string key in requiredItems[editableColumns])
            //{
            //    if (!createType.required.Contains(key))
            //    {
            //        createType.required.Add(key);
            //    }
            //}


            createType.type = "object";
            createType.title = entities[index] + "Create";

            // Adding all the generated models
            models.Add(createType.title, createType);
        }

        private void GetEntityAttributes(Dictionary<string, Schema> properties, int index)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString;

            CrmConnection crmConnection = CrmConnection.Parse(connectionString);
            using (OrganizationService service = new OrganizationService(crmConnection))
            {
                RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
                    {
                        EntityFilters = EntityFilters.Attributes,
                        LogicalName = entities[index],
                        RetrieveAsIfPublished = false
                    };

                RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)service.Execute(retrieveEntityRequest);

                for (int i=0; i < retrieveEntityResponse.EntityMetadata.Attributes.Length; i++ )
                {
                    if ((bool)retrieveEntityResponse.EntityMetadata.Attributes[i].CanBeSecuredForCreate
                        || (bool)retrieveEntityResponse.EntityMetadata.Attributes[i].IsCustomAttribute)
                    {
                        Schema columnSchema = new Schema();
                        columnSchema.type = "string";
                        columnSchema.vendorExtensions = new Dictionary<string, object>();
                                                
                        LocalizedLabelCollection labelCollection = retrieveEntityResponse.EntityMetadata.Attributes[i].DisplayName.LocalizedLabels;
                        string label = labelCollection.Count > 0 ?
                                        labelCollection[0].Label : retrieveEntityResponse.EntityMetadata.Attributes[i].LogicalName;
                        columnSchema.vendorExtensions.Add(SummaryAndVisibilityOperationFilter.summaryVendorExtension, label);

                        if (retrieveEntityResponse.EntityMetadata.Attributes[i].AttributeType == AttributeTypeCode.Lookup)
                        {
                            string relatedEntityName =
                                    ((LookupAttributeMetadata)(retrieveEntityResponse.EntityMetadata.Attributes[i])).Targets[0];
                            
                            Dictionary<string, string> lookupRef;

                            if (!properties.ContainsKey("LookupReferences"))
                            { 
                                lookupRef= new Dictionary<string, string>();
                                lookupRef.Add(retrieveEntityResponse.EntityMetadata.Attributes[i].LogicalName,
                                                relatedEntityName);

                                Schema hiddenColSchema = new Schema();
                                hiddenColSchema.type = "object";

                                Schema addProp = new Schema();
                                addProp.type = "string";
                                hiddenColSchema.additionalProperties = addProp;

                                hiddenColSchema.vendorExtensions = new Dictionary<string, object>();
                                hiddenColSchema.vendorExtensions.Add(SummaryAndVisibilityOperationFilter.visibilityVendorExtension, Visibility.Internal.ToString().ToLower());
                                hiddenColSchema.readOnly = true;
                                hiddenColSchema.@default = lookupRef;

                                properties.Add("LookupReferences", hiddenColSchema);
                            }
                            else
                            {
                                lookupRef = (Dictionary<string,string>)properties["LookupReferences"].@default;
                                lookupRef.Add(retrieveEntityResponse.EntityMetadata.Attributes[i].LogicalName,
                                                relatedEntityName);
                            }
                        }

                        if (labelCollection.Count > 0)
                        {
                            properties.Add(retrieveEntityResponse.EntityMetadata.Attributes[i].LogicalName, columnSchema);
                        }
                    }
                }
            }
        }
        
        private static T Clone<T>(T source)
        {
            var settings = new JsonSerializerSettings();
            settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;

            var serialized = JsonConvert.SerializeObject(source, settings);
            
            return JsonConvert.DeserializeObject<T>(serialized, settings);
        }

        private static void UpdateRefInSchema(Schema source, Schema destination)
        {
            if (source == null || destination == null)
            {
                return;
            }

            destination.@ref = source.@ref;
            destination.items = source.items;

            if (source.properties != null)
            {
                foreach (string s in source.properties.Keys)
                {
                    UpdateRefInSchema(source.properties[s], destination.properties[s]);
                }
            }
        }
    }

}