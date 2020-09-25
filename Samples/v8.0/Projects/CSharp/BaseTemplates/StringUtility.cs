//------------------------------------------------------------------------------
//
// Copyright (c) 2002-2017 CodeSmith Tools, LLC.  All rights reserved.
// 
// The terms of use for this software are contained in the file
// named sourcelicense.txt, which can be found in the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by the
// terms of this license.
// 
// You must not remove this notice, or any other, from this software.
//
//------------------------------------------------------------------------------

using System;
using CodeSmith.Engine;

namespace CodeSmith.BaseTemplates
{
    /// <summary>
    /// Various string utility methods.
    /// </summary>
    [Obsolete("This method has been deprecated. Please use StringUtil instead.")]
    public class StringUtility
    {
        
        private StringUtility()
        {
        }
        
        /// <summary>
        /// Determines if a string is in plural form based on some simple rules.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("This method has been deprecated. Please use StringUtil.IsPlural() instead.")]
        public static bool IsPlural(string value)
        {
            return StringUtil.IsPlural(value);
        }
        
        /// <summary>
        /// Determines if a string is in singular form based on some simple rules.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("This method has been deprecated. Please use StringUtil.IsSingular() instead.")]
        public static bool IsSingular(string value)
        {
            return StringUtil.IsSingular(value);
        }
        
        /// <summary>
        /// Converts a string to plural based on some simple rules.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("This method has been deprecated. Please use StringUtil.ToPlural() instead.")]
        public static string ToPlural(string value)
        {
            return StringUtil.ToPlural(value);
        }
        
        /// <summary>
        /// Converts a string to singular based on some simple rules.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("This method has been deprecated. Please use StringUtil.ToSingular() instead.")]
        public static string ToSingular(string value)
        {
            return StringUtil.ToSingular(value);
        }
        
        /// <summary>
        /// Converts a string to use camelCase.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("This method has been deprecated. Please use StringUtil.ToCamelCase() instead.")]
        public static string ToCamelCase(string value)
        {
            return StringUtil.ToCamelCase(value);
        }
        
        /// <summary>
        /// Converts a string to use PascalCase.
        /// </summary>
        /// <param name="value">Text to convert</param>
        /// <returns></returns>
        [Obsolete("This method has been deprecated. Please use StringUtil.ToPascalCase() instead.")]
        public static string ToPascalCase(string value)
        {
            return StringUtil.ToPascalCase(value);
        }

        /// <summary>
        /// Takes a NameIdentifier and spaces it out into words "Name Identifier".
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("This method has been deprecated. Please use StringUtil.ToSpacedWords() instead.")]
        public static string ToSpacedWords(string value)
        {
            return StringUtil.ToSpacedWords(value);
        }
    }
}
