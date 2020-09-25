//------------------------------------------------------------------------------
//
// Copyright (c) 2002-2012 CodeSmith Tools, LLC.  All rights reserved.
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
using System.Text;

namespace CodeSmith.BaseTemplates
{
	/// <summary>
	/// The result of a script execution including any errors that may have occurred.
	/// </summary>
	public class ScriptResult
    {
        #region Constructor(s)

        private ScriptResult(){}

        public ScriptResult(bool success, ScriptErrorCollection errors)
		{
			Success = success;
			Errors = errors;
        }

        #endregion

        #region Public Read-Only Properties

        /// <summary>
	    /// Whether the script was executed successfully or not.
	    /// </summary>
	    public bool Success { get; private set; }

	    /// <summary>
	    /// Any errors that occurred as the result of executing the script.
	    /// </summary>
	    public ScriptErrorCollection Errors { get; private set; }

        #endregion

        #region Public Overridden Methods

        /// <summary>
		/// Returns a string containing the script execution results including errors.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
	        if (Success)
	            return "Script executed successfully.";

	        var builder = new StringBuilder();
	        builder.Append("Script execution failed with the following errors:\r\n\r\n");

	        foreach (ScriptError error in Errors)
	            builder.AppendFormat("{0}\r\n", error);

	        return builder.ToString();
        }

        #endregion
    }
}
