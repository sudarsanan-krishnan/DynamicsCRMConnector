//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Azure.Biztalk.DynamicInvoke
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Integration.Common.Messaging;

    public static class TransformUtilities
    {
        public static MessageDescription GetBase64Message(byte[] input, IDictionary<string, object> messageProperties)
        {
            string body = Convert.ToBase64String(input);            
            MessageDescription message = new MessageDescription(body, messageProperties, string.Empty);
            return message;
        }

        public static string ConvertFromBase64(string input)
        {
            byte[] data = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(data);
        }
    }
}
