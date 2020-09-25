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
using System.Collections;

namespace CodeSmith.BaseTemplates
{
	public class ScriptErrorCollection : ICollection, IEnumerable
	{
		private ArrayList _errors;
		
		public ScriptErrorCollection()
		{
			_errors = new ArrayList();
		}
		
		internal void Add(ScriptError error)
		{
			_errors.Add(error);
		}
		
		public void CopyTo(Array array, int index)
		{
			_errors.CopyTo(array, index);
		}
		
		public IEnumerator GetEnumerator()
		{
			return _errors.GetEnumerator();
		}
		
		public int Count
		{
			get {return _errors.Count;}
		}
		
		public ScriptError this[int index]
		{
			get
			{
				return (ScriptError)_errors[index];
			}
		}
		
		bool ICollection.IsSynchronized
		{
			get	{return false;}
		}
		
		object ICollection.SyncRoot
		{
			get	{return null;}
		}
	}
}
