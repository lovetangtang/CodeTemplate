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

namespace SchemaExplorer {
    internal static class SqlFactory {
        public static string GetTables(int majorVersion, bool isAzure) {
            if (isAzure)
                return SqlScripts.GetTablesAzure;

            if (majorVersion >= 9)
                return SqlScripts.GetTables2005;

            return SqlScripts.GetTables;
        }

        public static string GetAllTableColumns(int majorVersion) {
            if (majorVersion >= 9)
                return SqlScripts.GetAllTableColumns2005;
            
            return SqlScripts.GetAllTableColumns;
        }

        public static string GetTableIndexes(int majorVersion, bool isAzure) {
            if (isAzure)
                return SqlScripts.GetTableIndexesAzure;

            if (majorVersion >= 9)
                return SqlScripts.GetTableIndexes2005;

            return SqlScripts.GetTableIndexes;
        }

        public static string GetTableColumns(int majorVersion) {
            if (majorVersion >= 9)
                return SqlScripts.GetTableColumns2005;
            
            return SqlScripts.GetTableColumns;
        }

        public static string GetColumnConstraints(int majorVersion) {
            if (majorVersion >= 9)
                return SqlScripts.GetColumnConstraints2005;
            
            return SqlScripts.GetColumnConstraints;
        }

        public static string GetColumnConstraintsWhere(int majorVersion) {
            if (majorVersion >= 9)
                return " WHERE SCHEMA_NAME([t].[schema_id]) = @SchemaName AND [t].[name] = @TableName AND [c].[name] = @ColumnName";
            
            return " AND [stbl].[name] = @SchemaName AND [tbl].[name] = @TableName AND [clmns].[name] = @ColumnName";
        }

        public static string GetIndexes(int majorVersion, bool isAzure) {
            if (isAzure)
                return SqlScripts.GetIndexesAzure;

            if (majorVersion >= 9)
                return SqlScripts.GetIndexes2005;
            
            return SqlScripts.GetIndexes;
        }

        public static string GetKeys(int majorVersion) {
            if (majorVersion >= 9)
                return SqlScripts.GetKeys2005;
            
            return SqlScripts.GetKeys;
        }

        public static string GetExtendedData(int majorVersion) {
            if (majorVersion >= 9)
                return SqlScripts.GetExtendedData2005;
            
            return SqlScripts.GetExtenedData;
        }

        public static string GetExtendedProperties(int majorVersion) {
            return SqlScripts.GetExtendedProperties;
        }

        public static string GetViews(int majorVersion, bool isAzure) {
            if (isAzure)
                return SqlScripts.GetViewsAzure;

            if (majorVersion >= 9)
                return SqlScripts.GetViews2005;
            
            return SqlScripts.GetViews;
        }

        public static string GetViewColumns(int majorVersion) {
            if (majorVersion >= 9)
                return SqlScripts.GetViewColumns2005;
            
            return SqlScripts.GetViewColumns;
        }

        public static string GetAllViewColumns(int majorVersion) {
            if (majorVersion >= 9)
                return SqlScripts.GetAllViewColumns2005;
            
            return SqlScripts.GetAllViewColumns;
        }

        public static string GetCommands(int majorVersion, bool isAzure) {
            if (isAzure)
                return SqlScripts.GetCommandsAzure;

            if (majorVersion >= 9)
                return SqlScripts.GetCommands2005;
            
            return SqlScripts.GetCommands;
        }

        public static string GetCommandParameters(int majorVersion) {
            if (majorVersion >= 9)
                return SqlScripts.GetCommandParameters2005;
            
            return SqlScripts.GetCommandParameters;
        }

        public static string GetAllCommandParameters(int majorVersion) {
            if (majorVersion >= 9)
                return SqlScripts.GetAllCommandParameters2005;
            
            return SqlScripts.GetAllCommandParameters;
        }
    }
}
