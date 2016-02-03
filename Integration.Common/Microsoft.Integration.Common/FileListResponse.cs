//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator;

namespace Microsoft.Integration.Common
{

    /// <remarks/>
    public class FileInfo
    {
        /// <summary>
        /// The name of the file
        /// </summary>
        [Required]
        [CustomSummary(CommonConstants.FileName)]
        public string FileName { get; set; }

        /// <summary>
        /// The name of the folder which contains the file
        /// </summary>
        [Required]
        [CustomSummary(CommonConstants.FolderPath)]
        public string FolderPath { get; set; }

        /// <summary>
        /// Last updated time of the file
        /// </summary>
        [Required]
        [CustomVisibilityAttribute(Visibility.Advanced)]
        [CustomSummary(CommonConstants.LastModifiedUtc)]
        public string LastModifiedUtc { get; set; }

        /// <summary>
        /// The size of the file
        /// </summary>
        [Required]
        [CustomVisibilityAttribute(Visibility.Advanced)]
        [CustomSummary(CommonConstants.FileSizeInBytes)]
        public string FileSizeInBytes { get; set; }

        /// <summary>
        /// The path of the file
        /// </summary>
        [Required]
        [CustomSummary(CommonConstants.FilePath)]
        public string FilePath { get; set; }        

    }
}
