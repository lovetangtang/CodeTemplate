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

namespace CodeSmith.BaseTemplates
{
	/// <summary>
	/// An error that occurred during a script execution.
	/// </summary>
	public class ScriptError
    {
        #region Constructor(s)

        private ScriptError(){}

        public ScriptError(int infoNumber, byte errorState, byte errorClass, string errorMessage, string procedure, int lineNumber)
		{
			Class = errorClass;
			LineNumber = lineNumber;
			Message = errorMessage;
			Number = infoNumber;
			Procedure = procedure;
			State = errorState;
        }

        #endregion

        #region Public Overriden Methods

        public override string ToString()
		{
			return string.Format("Server: Msg {0}, Level {1}, State {2}, Line {3}\r\n{4}\r\n", Number, Class, State, LineNumber, Message);
        }

        #endregion

        #region Public Read-Only Properties

        public byte Class { get; private set; }

	    public int LineNumber { get; private set; }

	    public string Message { get; private set; }

	    public int Number { get; private set; }

	    public string Procedure { get; private set; }

	    public byte State { get; private set; }

        #endregion
    }
}
