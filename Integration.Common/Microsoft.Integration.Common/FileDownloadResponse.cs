//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Integration.Common
{
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator;
    using Microsoft.Integration.Common.Messaging;

    /// <summary>
    /// Model Class defining attributes of the response to be returned for Download File Operation
    /// </summary>
    public class File : FileContent
    {
        /// <summary>
        /// Empty constructor for json serilaization
        /// </summary>
        public File() : base()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        public File(Stream stream, ContentTransferEncoding encoding)
            : base(stream, encoding)
        {
        }
        /// <summary>
        /// FileName
        /// </summary>
        [Required]
        [CustomSummary(CommonConstants.FileName)]
        public string FileName { get; set; }

        /// <summary>
        /// FilePath
        /// </summary>
        [Required]
        [CustomSummary(CommonConstants.FilePath)]
        public string FilePath { get; set; }

        /// <summary>
        /// Path of the folder containg the file
        /// </summary>
        [Required]
        [CustomSummary(CommonConstants.FolderPath)]
        public string FolderPath { get; set; }

    }
}