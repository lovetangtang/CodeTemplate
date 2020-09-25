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

namespace CodeSmith.BaseTemplates
{
	public class Batch
    {
        #region Private Members

        private readonly int _startLineNumber = 1;
		private readonly int _lineCount = 1;
		private readonly string _content = String.Empty;

        #endregion

        #region Constructor(s)

        private Batch(){}

        public Batch(int startLineNumber, int lineCount, string content)
		{
			_startLineNumber = startLineNumber;
			_lineCount = lineCount;
			_content = content;
        }

        #endregion

        #region Public Read-Only Properties

        public int StartLineNumber
		{
			get {return _startLineNumber;}	
		}
		
		public int LineCount
		{
			get {return _lineCount;}	
		}
		
		public string Content
		{
			get {return _content;}
        }

        #endregion
    }
}
