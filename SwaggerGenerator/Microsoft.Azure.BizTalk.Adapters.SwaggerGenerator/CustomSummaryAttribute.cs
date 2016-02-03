//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.All)]
    public class CustomSummaryAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string Summary;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="summary"></param>
        public CustomSummaryAttribute(string summary)
        {
            this.Summary = summary;
        }
    }
}