//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System.Collections.Generic;
    using System.Net;
    using Newtonsoft.Json;

    /// <summary>
    /// Class to represent the body of error resposne
    /// </summary>
    public class ErrorResponseBody
    {
        /// <summary>
        /// The Http Status code of the response
        /// </summary>
        [JsonProperty("status")]
        public HttpStatusCode Status { get; set; }
        
        /// <summary>
        /// Error Message
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// URL of Source of the error
        /// </summary>
        [JsonProperty("source")]
        public string Source { get; set; }

        /// <summary>
        /// any errors provided by the source (optional)
        /// </summary>
        [JsonProperty("errors")]
        public List<string> Errors { get; set; }
    }
}
