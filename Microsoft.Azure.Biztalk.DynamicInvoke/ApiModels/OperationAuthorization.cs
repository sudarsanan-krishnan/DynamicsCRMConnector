//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke.ApiModels
{
    using System.Net.Http;
    
    /// <summary>
    /// Base class representing required authorization
    /// for an operation.
    /// </summary>
    public abstract class OperationAuthorization
    {
        /// <summary>
        /// String indicating which kind of authorization
        /// mechanism this is.
        /// </summary>
        /// <remarks>
        /// This is a string instead of an enum becuase
        /// the source documents may specify auth schemes
        /// we don't understand up front.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Auth", Justification = "param name is correct")]
        public abstract string AuthType { get; }

        /// <summary>
        /// Stamp a request message with authentication.
        /// </summary>
        /// <param name="request">The request to authenticate</param>
        /// <param name="propertyValues">Property values to pull inputs from.</param>
        public abstract void Authenticate(HttpRequestMessage request, object propertyValues);
    }
}
