//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Azure.BizTalk.Adapters.SwaggerGenerator
{
    using System;
    using System.Text.RegularExpressions;

    public static class Helper
    {
        private static readonly Regex Regex = new Regex(@"[^a-zA-Z0-9_]");
        private const string Unique = "Unique";

        public static string SanitizeName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            string modifiedName = Regex.Replace(name, string.Empty);
            if (Char.IsDigit(name[0]))
            {
                // Adding Unique only to avoid the case where the name starts with a number
                return Unique + modifiedName;
            }
            else
            {
                return modifiedName;
            }
        }
    }
}
