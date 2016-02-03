//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System.Collections.Generic;
    using System.Threading;

    public class JsonSwagger
    {
        private string requestType, responseType, responseTypeModel;
        private IDictionary<string, SwaggerDataType> requestTypeModels = new Dictionary<string, SwaggerDataType>();
        private IDictionary<string, SwaggerDataType> responseTypeModels = new Dictionary<string, SwaggerDataType>();

        public JsonSwagger(string path, string nickName, string requestType, string responseType = "", string summary = "", string description = "")
        {
            this.NewPath = "/" + path;
            this.NickName = nickName;
            this.requestType = Helper.SanitizeName(requestType);
            this.responseType = Helper.SanitizeName(responseType);
            this.Summary = summary;
            this.Description = description;
        }

        public string NewPath { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }

        public string NickName { get; set; }

        public string RequestType
        {
            get { return this.requestType;  }
            set { this.requestType = Helper.SanitizeName(value); }
        }

        public string ResponseType
        {
            get { return this.responseType; }
            set { this.responseType = Helper.SanitizeName(value); }
        }

        public string ResponseTypeModelName
        {
            get { return this.responseTypeModel; }
            set { this.responseTypeModel = Helper.SanitizeName(value); }
        }

        public static string ParamToRemove { get; set; }
        
        public void AddRequestTypeModel(string name, SwaggerDataType swaggerDataType)
        {
            this.requestTypeModels.Add(Helper.SanitizeName(name), swaggerDataType);
        }

        public void AddResponseTypeModel(string name, SwaggerDataType swaggerDataType)
        {
            this.responseTypeModels.Add(Helper.SanitizeName(name), swaggerDataType);
        }

        public IDictionary<string, SwaggerDataType> GetRequestTypeModels()
        {
            return new Dictionary<string, SwaggerDataType>(this.requestTypeModels);
        }

        public IDictionary<string, SwaggerDataType> GetResponseTypeModels()
        {
            return new Dictionary<string, SwaggerDataType>(this.responseTypeModels);
        }
    }
}
