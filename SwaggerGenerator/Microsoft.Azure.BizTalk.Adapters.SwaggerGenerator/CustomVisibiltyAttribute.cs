//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// 
        /// </summary>
        None,

        /// <summary>
        /// 
        /// </summary>
        Internal,

        /// <summary>
        /// 
        /// </summary>
        Advanced
    }

    /// <summary>
    /// 
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.All)]
    public class CustomVisibilityAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly Visibility Visibility;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Visibility"></param>
        public CustomVisibilityAttribute(Visibility visibility)
        {
            this.Visibility = visibility;
        }
    }
}