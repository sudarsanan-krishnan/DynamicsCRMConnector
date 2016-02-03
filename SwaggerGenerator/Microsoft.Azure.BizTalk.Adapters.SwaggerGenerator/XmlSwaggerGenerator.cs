//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Schema;

    public abstract class XmlSwaggerGenerator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "It Will always return XmlDocument")]
        public XmlDocument GenerateSwagger(XmlDocument swaggerDocument)
        {
            if (swaggerDocument == null)
            {
                throw new ArgumentNullException("swaggerDocument");
            }

            var root = swaggerDocument.SelectSingleNode("XmlSwagger");
            var apis = root.SelectNodes("apis");

            var apiNodes = new List<XmlNode>();

            foreach (XmlNode api in apis)
            {
                var node = api.SelectSingleNode("operations/nickname");
                string operationName = GetOperationName(node.InnerText);
                var operationSchemas = this.GenerateOperationSchema(operationName);
                
                foreach (var operationSchema in operationSchemas)
                {
                    var apiModified = api.CloneNode(true);
                    var operation = apiModified.SelectSingleNode("operations");

                    UpdateOperationNode(operationSchema, swaggerDocument, root, apiModified, operation);
                    apiNodes.Add(apiModified);
                }

                root.RemoveChild(api);
            }

            foreach (var apiNode in apiNodes)
            {
                root.AppendChild(apiNode);
            }

            return swaggerDocument;
        }

        // for multiple api, multiple operation scenario
        // used in sharepoint
        public XmlDocument GenerateSwagger(XmlDocument swaggerDocument, List<string> entityNames)
        {
            if (swaggerDocument == null)
            {
                throw new ArgumentNullException("swaggerDocument");
            }

            var root = swaggerDocument.SelectSingleNode("XmlSwagger");
            var apis = root.SelectNodes("apis");

            var apiNodes = new List<XmlNode>();

            foreach (XmlNode api in apis)
            {
                bool isModified = false;
                foreach (string entityName in entityNames)
                {
                    var apiModified = api.CloneNode(true);

                    var nodeList = api.SelectNodes("operations/nickname");

                    // each api can have multiple operations like get/put/post on it
                    foreach (XmlNode node in nodeList)
                    {
                        string operationName = GetOperationName(node.InnerText);

                        IList<OperationSchema> operationSchemas = null;

                        XmlNode operationNodeToBeModified = null;

                        // find the operation in the new api on which we will be working on
                        XmlNodeList apiOperations = apiModified.SelectNodes("operations");
                        if (apiOperations != null)
                        {
                            foreach (XmlNode apiOperation in apiOperations)
                            {
                                var nickname = apiOperation.SelectSingleNode("nickname");
                                if (nickname != null && nickname.InnerText == node.InnerText)
                                {
                                    operationNodeToBeModified = apiOperation;
                                    break;
                                }
                            }
                        }

                        try
                        {
                            operationSchemas = this.GenerateOperationSchemaForEntity(entityName, operationName);
                        }
                        catch (InvalidOperationException)
                        {
                            isModified = true;

                            // some operations are not valid for a particular entity remove that operation from the list
                            if (operationNodeToBeModified != null)
                            {
                                apiModified.RemoveChild(operationNodeToBeModified);
                            }
                        }

                        if (operationSchemas != null)
                        {
                            isModified = true;
                            foreach (var operationSchema in operationSchemas)
                            {
                                UpdateOperationNode(operationSchema, swaggerDocument, root, apiModified,
                                    operationNodeToBeModified);
                            }
                        }
                    }

                    // we will add cloned api only if isModified is true and operations are present in apiModified
                    XmlNodeList operationList = apiModified.SelectNodes("operations");
                    if (operationList != null && operationList.Count > 0)
                    {
                        apiNodes.Add(apiModified);
                    }

                    if (!isModified)
                    {
                        // since api should not be modified, we will add it as is
                        apiNodes.Add(api);
                    }
                }

                root.RemoveChild(api);
            }

            foreach (var apiNode in apiNodes)
            {
                root.AppendChild(apiNode);
            }

            return swaggerDocument;
        }

        protected void UpdateOperationNode(OperationSchema operationSchema, XmlDocument swaggerDocument, XmlNode root, XmlNode apiModified, XmlNode operationNodeToBeModified)
        {
                // Adding the XSDs to 'models' section in swagger
                var models = root.SelectSingleNode("models");

                string typeName = operationSchema.OperationName;
                var type = swaggerDocument.CreateElement(typeName);
                models.AppendChild(type);

                foreach (var xmlSchema in operationSchema.Schemas)
                {
                    XmlNode schemaImported = swaggerDocument.ImportNode(GetSchemaNode(xmlSchema), true);
                    type.AppendChild(schemaImported);
                }

                // Modifying the path to associate it with a specific entity.
                XmlNode path = apiModified.SelectSingleNode("path");
                string pathValue = path.InnerText;

                if (pathValue.Contains("{"))
                {
                    var index = pathValue.IndexOf('{');
                    pathValue = pathValue.Remove(index);
                    pathValue += operationSchema.OperationSubpath;
                    path.InnerText = pathValue;
                }

                var operation = operationNodeToBeModified;

                var nickName = operation.SelectSingleNode("nickname");
                nickName.InnerText = operationSchema.OperationName;

                // Setting the response type
                AddTypes(swaggerDocument, operation, operationSchema.ResponseNamespace,
                    operationSchema.ResponseElement, typeName);

                // Setting the request type
                var parameters = operation.SelectNodes("parameters");
                if (parameters != null)
                {
                    foreach (XmlNode parameter in parameters)
                    {
                        var paramtype = parameter.SelectSingleNode("paramType");
                        if (paramtype.InnerText == "body")
                        {
                            AddTypes(swaggerDocument, parameter, operationSchema.RequestNamespace,
                                operationSchema.RequestElement, typeName);
                            break;
                        }
                    }
                }
        }

        protected virtual IList<OperationSchema> GenerateOperationSchema(string operationGroup)
        {
            throw new NotImplementedException();
        }

        protected virtual IList<OperationSchema> GenerateOperationSchemaForEntity(string entityName, string operationGroup)
        {
            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "By Design")]
        protected virtual XmlNode GetSchemaNode(XmlSchema xmlSchema)
        {
            throw new NotImplementedException();
        }

        private static string GetOperationName(string nickName)
        {
            var index = nickName.IndexOf('_');
            return nickName.Substring(index + 1);
        }

        private static void AddTypes(XmlDocument swaggerDocument, XmlNode node, string ns, string element, string typeName)
        {
            if (ns == null && element == null)
            {
                return;
            }

            var type = node.SelectSingleNode("type");

            XmlElement xmltype = swaggerDocument.CreateElement(type.Name);
            xmltype.SetAttribute("namespace", ns);
            xmltype.SetAttribute("element", element);
            xmltype.InnerXml = typeName;

            node.ReplaceChild(xmltype, type);
        }
    }
}