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
using System.Data;

namespace SchemaExplorer
{
    internal static class Utility
    {
        internal static DbType GetDbType(string nativeType)
        {
            switch (nativeType.Trim().ToLower())
            {
                case "bigint": return DbType.Int64;
                case "binary": return DbType.Binary;
                case "bit": return DbType.Boolean;
                case "char": return DbType.AnsiStringFixedLength;
                case "datetime": return DbType.DateTime;
                case "decimal": return DbType.Decimal;
                case "float": return DbType.Double;
                case "image": return DbType.Binary;
                case "int": return DbType.Int32;
                case "money": return DbType.Currency;
                case "nchar": return DbType.StringFixedLength;
                case "ntext": return DbType.String;
                case "numeric": return DbType.Decimal;
                case "nvarchar": return DbType.String;
                case "real": return DbType.Single;
                case "smalldatetime": return DbType.DateTime;
                case "smallint": return DbType.Int16;
                case "smallmoney": return DbType.Currency;
                case "sql_variant": return DbType.Object;
                case "sysname": return DbType.StringFixedLength;
                case "text": return DbType.AnsiString;
                case "timestamp": return DbType.Binary;
                case "tinyint": return DbType.Byte;
                case "uniqueidentifier": return DbType.Guid;
                case "varbinary": return DbType.Binary;
                case "varchar": return DbType.AnsiString;
                case "xml": return DbType.Xml;
                case "datetime2": return DbType.DateTime2;
                case "time": return DbType.Time;
                case "date": return DbType.Date;
                case "datetimeoffset": return DbType.DateTimeOffset;

                default: return DbType.Object;
            }
        }

        internal static SqlDbType GetSqlDbType(string nativeType)
        {
            switch (nativeType.Trim().ToLower())
            {
                case "bigint": return SqlDbType.BigInt;
                case "binary": return SqlDbType.Binary;
                case "bit": return SqlDbType.Bit;
                case "char": return SqlDbType.Char;
                case "datetime": return SqlDbType.DateTime;
                case "decimal": return SqlDbType.Decimal;
                case "float": return SqlDbType.Float;
                case "image": return SqlDbType.Image;
                case "int": return SqlDbType.Int;
                case "money": return SqlDbType.Money;
                case "nchar": return SqlDbType.NChar;
                case "ntext": return SqlDbType.NText;
                case "numeric": return SqlDbType.Decimal;
                case "nvarchar": return SqlDbType.NVarChar;
                case "real": return SqlDbType.Real;
                case "smalldatetime": return SqlDbType.SmallDateTime;
                case "smallint": return SqlDbType.SmallInt;
                case "smallmoney": return SqlDbType.SmallMoney;
                case "sql_variant": return SqlDbType.Variant;
                case "sysname": return SqlDbType.NChar;
                case "text": return SqlDbType.Text;
                case "timestamp": return SqlDbType.Timestamp;
                case "tinyint": return SqlDbType.TinyInt;
                case "uniqueidentifier": return SqlDbType.UniqueIdentifier;
                case "varbinary": return SqlDbType.VarBinary;
                case "varchar": return SqlDbType.VarChar;
                case "xml": return SqlDbType.Xml;
                case "datetime2": return SqlDbType.DateTime2;
                case "time": return SqlDbType.Time;
                case "date": return SqlDbType.Date;
                case "datetimeoffset": return SqlDbType.DateTimeOffset;

                default: return SqlDbType.Variant;
            }
        }

        internal static string GetNativeType(SqlDbType sqlDbType)
        {
            switch (sqlDbType)
            {
                case SqlDbType.BigInt: return "bigint";
                case SqlDbType.Binary: return "binary";
                case SqlDbType.Bit: return "bit";
                case SqlDbType.Char: return "char";
                case SqlDbType.DateTime: return "datetime";
                case SqlDbType.Decimal: return "decimal";
                case SqlDbType.Float: return "float";
                case SqlDbType.Image: return "image";
                case SqlDbType.Int: return "int";
                case SqlDbType.Money: return "money";
                case SqlDbType.NChar: return "nchar";
                case SqlDbType.NText: return "ntext";
                case SqlDbType.NVarChar: return "nvarchar";
                case SqlDbType.Real: return "real";
                case SqlDbType.SmallDateTime: return "smalldatetime";
                case SqlDbType.SmallInt: return "smallint";
                case SqlDbType.SmallMoney: return "smallmoney";
                case SqlDbType.Variant: return "sql_variant";
                case SqlDbType.Text: return "text";
                case SqlDbType.Timestamp: return "timestamp";
                case SqlDbType.TinyInt: return "tinyint";
                case SqlDbType.UniqueIdentifier: return "uniqueidentifier";
                case SqlDbType.VarBinary: return "varbinary";
                case SqlDbType.VarChar: return "varchar";
                case SqlDbType.Xml: return "xml";
                case SqlDbType.DateTime2: return "datetime2";
                case SqlDbType.Time: return "time";
                case SqlDbType.Date: return "date";
                case SqlDbType.DateTimeOffset: return "datetimeoffset";

                default: return "sql_variant";
            }
        }

        internal static ParameterDirection GetParameterDirection(short parameterType)
        {
            switch (parameterType)
            {
                case 1: return ParameterDirection.Input;
                case 2: return ParameterDirection.InputOutput;
                case 3: return ParameterDirection.Output;
                case 4: return ParameterDirection.ReturnValue;
                default: return ParameterDirection.Input;
            }
        }

        internal static Dictionary<string, TableSchema> ToDictionary(TableSchema table)
        {
            Dictionary<string, TableSchema> tableDictionary = new Dictionary<string, TableSchema>();
            tableDictionary.Add(table.FullName, table);
            return tableDictionary;
        }

        internal static Dictionary<string, TableSchema> ToDictionary(IList<TableSchema> tables)
        {
            Dictionary<string, TableSchema> tableDictionary = new Dictionary<string, TableSchema>();
            foreach (TableSchema table in tables)
                tableDictionary.Add(table.FullName, table);

            return tableDictionary;
        }

        internal static Dictionary<string, ViewSchema> ToDictionary(ViewSchema view)
        {
            Dictionary<string, ViewSchema> viewDictionary = new Dictionary<string, ViewSchema>();
            viewDictionary.Add(view.FullName, view);
            return viewDictionary;
        }

        internal static Dictionary<string, ViewSchema> ToDictionary(IList<ViewSchema> views)
        {
            Dictionary<string, ViewSchema> viewDictionary = new Dictionary<string, ViewSchema>();
            foreach (ViewSchema view in views)
                viewDictionary.Add(view.FullName, view);

            return viewDictionary;
        }

        internal static Dictionary<string, CommandSchema> ToDictionary(CommandSchema command)
        {
            Dictionary<string, CommandSchema> commandDictionary = new Dictionary<string, CommandSchema>();
            commandDictionary.Add(command.FullName, command);
            return commandDictionary;
        }

        internal static Dictionary<string, CommandSchema> ToDictionary(IList<CommandSchema> commands)
        {
            Dictionary<string, CommandSchema> commandDictionary = new Dictionary<string, CommandSchema>();
            foreach (CommandSchema command in commands)
                commandDictionary.Add(command.FullName, command);

            return commandDictionary;
        }

        internal static bool IsAnyNullOrEmpty(params string[] values)
        {
            if (values == null || values.Length == 0)
                return false;

            for (int i = 0; i < values.Length; i++)
                if (string.IsNullOrEmpty(values[i]))
                    return true;

            return false;
        }

        internal static bool ParseBooleanExtendedProperty(SchemaObjectBase schemaObjectBase, string extendedProperty)
        {
            if (schemaObjectBase == null)
                return false;

            var properties = schemaObjectBase.GetLoadedExtendedProperties();
            if(!properties.Contains(extendedProperty)) return false;

            bool temp;
            bool.TryParse(properties[extendedProperty].Value.ToString(), out temp);

            return temp;
        }

        internal static string CommandResultSchemaFMTQuery(CommandSchema command)
        {
            bool isTableValuedFunction = ParseBooleanExtendedProperty(command, ExtendedPropertyNames.IsTableValuedFunction);

            var sb = new StringBuilder();
            sb.Append("SET FMTONLY ON\r\n");

            if (!isTableValuedFunction)
            {
                sb.AppendFormat("EXEC [{0}].[{1}]\r\n", command.Owner, command.Name);
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    if (command.Parameters[i].Direction != ParameterDirection.ReturnValue)
                    {
                        // http://www.sommarskog.se/arrays-in-sql-2008.html && http://community.codesmithtools.com/Support_Forums/f/3/p/11224/42842.aspx#42842
                        if (!ParseBooleanExtendedProperty(command.Parameters[i], ExtendedPropertyNames.IsUserDefinedTableType))
                            sb.Append("\tNULL");
                        else
                            sb.Append("\tDEFAULT");

                        if (i < command.Parameters.Count - 1) sb.Append(",");
                        sb.Append("\r\n");
                    }
                }
            }
            else
            {
                // Table-valued function so select from it.
                sb.AppendFormat("SELECT * FROM [{0}].[{1}] (", command.Owner, command.Name);
                for (int i = 0; i < command.Parameters.Count; i++)
                {
                    if (command.Parameters[i].Direction != ParameterDirection.ReturnValue)
                    {
                        // http://www.sommarskog.se/arrays-in-sql-2008.html && http://community.codesmithtools.com/Support_Forums/f/3/p/11224/42842.aspx#42842
                        if (!ParseBooleanExtendedProperty(command.Parameters[i], ExtendedPropertyNames.IsUserDefinedTableType))
                            sb.Append("\tNULL");
                        else
                            sb.Append("\tDEFAULT");

                        if (i < command.Parameters.Count - 1) sb.Append(", ");
                    }
                }
                sb.Append(")\r\n");
            }

            sb.Append("SET FMTONLY OFF\r\n");

            return sb.ToString();
        }

        internal static string CommandResultSchemaTransactionalQuery(CommandSchema command)
        {
            var sb = new StringBuilder();
            sb.Append("BEGIN TRANSACTION\r\n");
            sb.AppendFormat("EXEC [{0}].[{1}]\r\n", command.Owner, command.Name);
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                if (command.Parameters[i].Direction != ParameterDirection.ReturnValue)
                {
                    // http://www.sommarskog.se/arrays-in-sql-2008.html && http://community.codesmithtools.com/Support_Forums/f/3/p/11224/42842.aspx#42842
                    if (!ParseBooleanExtendedProperty(command.Parameters[i], ExtendedPropertyNames.IsUserDefinedTableType))
                        sb.Append("\tNULL");
                    else
                        sb.Append("\tDEFAULT");

                    if (i < command.Parameters.Count - 1) sb.Append(",");
                    sb.Append("\r\n");
                }
            }
            sb.Append("ROLLBACK TRANSACTION\r\n");

            return sb.ToString();
        }
    }
}
