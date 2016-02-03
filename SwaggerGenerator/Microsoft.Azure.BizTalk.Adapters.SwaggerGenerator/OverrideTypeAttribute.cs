//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;

    /// <summary>
    /// Class used for specifying override attributes
    /// </summary>
    [AttributeUsageAttribute(AttributeTargets.All)]
    public class OverrideTypeAttribute : Attribute
    {
        /// <summary>
        /// new override type
        /// </summary>
        public readonly string FieldType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldType"></param>
        public OverrideTypeAttribute(string fieldType)
        {
            this.FieldType = fieldType;
        }
    }
}