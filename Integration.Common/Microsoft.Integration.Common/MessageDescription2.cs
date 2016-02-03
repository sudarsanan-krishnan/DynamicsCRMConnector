//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common.Messaging
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Net.Mime;
    using Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator;

    /// <summary>
    /// Class that denotes a file/document message which comprises of content and properties
    /// </summary>
    public class MessageDescription2
    {
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public MessageDescription2()
        {
        }

        /// <summary>
        /// Initializes new instance of the class and is used for string content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="allProperties"></param>
        /// <param name="contentType"></param>
        public MessageDescription2(string content, IDictionary<string, string> allProperties, string contentType)
        {
            this.Content = new Content(content, contentType);
            this.Properties = allProperties;
        }

        /// <summary>
        /// Initializes new instance of the class and converts stream to base64 format and save
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="allProperties"></param>
        /// <param name="contentType"></param>
        public MessageDescription2(Stream stream, IDictionary<string, string> allProperties, string contentType)
        {
            this.Content = new Content(stream, contentType);
            this.Properties = allProperties;
        }

        /// <summary>
        /// Content Information of message/file/document
        /// </summary>
        [Required]
        [CustomSummary("Content")]
        public Content Content { get; set; }

        /// <summary>
        /// Properties/Metadata associated with message/file/document
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Needed")]
        [Required]
        [CustomSummary("Properties")]
        public IDictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Returns Stream associated with Content.
        /// </summary>
        /// <returns>Stream</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Needed")]
        public Stream GetContentStream()
        {
            if (this.Content == null)
            {
                return null;
            }

            return this.Content.GetStream();
        }
    }
}
