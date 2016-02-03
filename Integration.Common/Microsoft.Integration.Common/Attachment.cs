//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common.Messaging
{
    using System.IO;
    using System.Net.Mime;
    using Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator;

    /// <summary>
    /// This class denotes an Attachment
    /// </summary>
    public class Attachment : Content
    {
        /// <summary>
        /// This denotes the file name of the attachment
        /// </summary>
        [CustomSummary("File Name")]
        public string FileName { get; set; }

        /// <summary>
        /// Initializes a new instance of the Attachment class.
        /// </summary>
        public Attachment()
        {
        }

        /// <summary>
        /// Initializes an instance of the Attachment class
        /// </summary>
        /// <param name="content">Content of attachment</param>
        /// <param name="contentType">ContentType of attachment</param>
        /// <param name="fileName">File name of attachment</param>
        public Attachment(string content, string contentType, string fileName = null)
            : base(content, contentType)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                this.FileName = fileName;
            }
        }

        /// <summary>
        /// Initializes an instance of the Attachment class
        /// </summary>
        /// <param name="content">Content of attachment</param>
        /// <param name="contentType">ContentType of attachment</param>
        /// <param name="contentTransferEncoding">Content Transfer Encoding of attachment</param>
        /// <param name="fileName">File name of attachment</param>
        public Attachment(string content, string contentType, TransferEncoding contentTransferEncoding, string fileName = null)
            : base(content, contentType, contentTransferEncoding)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                this.FileName = fileName;
            }
        }

        /// <summary>
        /// Initializes an instance of the Attachment class
        /// </summary>
        /// <param name="stream">Content stream of attachment</param>
        /// <param name="contentType">ContentType of attachment</param>
        /// <param name="fileName">File name of attachment</param>
        public Attachment(Stream stream, string contentType, string fileName = null)
            : base(stream, contentType)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                this.FileName = fileName;
            }
        }
    }
}
