//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Net.Http;
    
    /// <summary>
    /// Null object implementation of <see cref="OperationAuthorization"/>
    /// which does not do any authorization.
    /// </summary>
    public class UnauthenticatedOperationAuthorization : OperationAuthorization
    {
        public override string AuthType
        {
            get { return "none"; }
        }

        public override void Authenticate(HttpRequestMessage request, object propertyValues)
        {
            // Deliberate NOOP
        }
    }
}
