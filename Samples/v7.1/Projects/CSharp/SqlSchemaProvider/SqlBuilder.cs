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
using System.Collections.Generic;
using System.Text;

namespace SchemaExplorer
{
    internal class SqlBuilder
    {
        private StringBuilder sqlStatements = new StringBuilder();

        public void AppendStatement(string sql)
        {
            string temp = sql.Trim();

            sqlStatements.Append(temp);
            if (!temp.EndsWith(";", StringComparison.OrdinalIgnoreCase))
                sqlStatements.Append(';');

            sqlStatements.AppendLine();
        }

        public override string ToString()
        {
            return sqlStatements.ToString();
        }

        #region implicit operators
        public static implicit operator string(SqlBuilder builder)
        {
            return builder.ToString();
        }
        #endregion
    }
}
