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

namespace SchemaExplorer {
    internal static class SqlFactory {
        public static string GetTables(int majorVersion) {
            return SqlScripts.GetTables;
        }

        public static string GetAllTableColumns(int majorVersion) {
            return SqlScripts.GetAllTableColumns;
        }

        public static string GetTableColumns(int majorVersion) {
            return SqlScripts.GetTableColumns;
        }

        public static string GetColumnConstraints(int majorVersion) {
            return SqlScripts.GetColumnConstraints;
        }

        public static string GetColumnConstraintsWhere(int majorVersion) {
            return " AND t.Creator = USER_ID( ? ) AND t.table_name = ? AND c.column_name = ? ";
        }

        public static string GetIndexes(int majorVersion) {
            return SqlScripts.GetIndexes;
        }

        public static string GetKeys(int majorVersion) {
            return SqlScripts.GetKeys;
        }

        public static string GetExtendedData(int majorVersion) {
            return SqlScripts.GetExtendedData;
        }

        public static string GetExtendedProperties(int majorVersion) {
            //return SqlScripts.GetExtendedProperties;
            return "";
        }

        public static string GetViews(int majorVersion) {
            return SqlScripts.GetViews;

        }

        public static string GetViewColumns(int majorVersion) {
            return SqlScripts.GetViewColumns;
        }

        public static string GetAllViewColumns(int majorVersion) {
            return SqlScripts.GetAllViewColumns;
        }

        public static string GetCommands(int majorVersion) {
            return SqlScripts.GetCommands;
        }

        public static string GetCommandParameters(int majorVersion) {
            return SqlScripts.GetCommandParameters;
        }

        public static string GetAllCommandParameters(int majorVersion) {
            return SqlScripts.GetAllCommandParameters;
        }

        public static string GetCommandResultSchema(int majorVersion) {
            return SqlScripts.GetCommandResultSchema;
        }
    }
}