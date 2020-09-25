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
using iAnywhere.Data.SQLAnywhere;

namespace SchemaExplorer {
    internal static class Utility {
        internal static DbType GetDbType(string nativeType) {
            return GetDbType(nativeType, 0, 0);
        }

        internal static DbType GetDbType(string nativeType, int precision, int scale) {
            switch (nativeType.Trim().ToLower()) {
                case "bigint":
                    return DbType.Int64;
                case "binary":
                    return DbType.Binary;
                case "bit":
                    return DbType.Boolean;
                case "char":
                    return DbType.AnsiStringFixedLength;
                case "date":
                    return DbType.Date;
                case "datetime":
                    return DbType.DateTime;
                case "datetime2":
                    return DbType.DateTime2;
                case "datetimeoffset":
                    return DbType.DateTimeOffset;
                case "decimal":
                    // if the designer defined this a numeric or decimal, then that is the way to treat it. kiss
                    if (precision > 28)
                        return DbType.VarNumeric; // should be DbType.decimal? - is ok i think!
                    return DbType.Decimal;
                case "double":
                    return DbType.Double;
                case "float":
                    if (precision <= 7)
                        return DbType.Single;
                    return DbType.Double;
                case "integer":
                    return DbType.Int32;
                case "image":
                    return DbType.Binary;
                case "long nvarchar":
                    return DbType.String;
                case "long binary":
                    return DbType.Binary;
                case "long varbit":
                    return DbType.Binary;
                case "long varchar":
                    return DbType.AnsiString;
                case "money":
                    return DbType.Currency;
                case "nchar":
                    return DbType.StringFixedLength;
                case "ntext":
                    return DbType.String;
                case "numeric":
                    // if the designer defined this a numeric or decimal, then that is the way to treat it. kiss
                    if (precision > 28)
                        return DbType.VarNumeric; // should be DbType.decimal? - is ok i think!
                    return DbType.Decimal;
                case "nvarchar":
                    return DbType.String;
                case "real":
                    return DbType.Single;
                case "smalldatetime":
                    return DbType.DateTime;
                case "smallint":
                    return DbType.Int16;
                case "smallmoney":
                    return DbType.Currency;
                case "sql_variant":
                    return DbType.Object;
                case "sysname":
                    return DbType.StringFixedLength;
                case "text":
                    return DbType.AnsiString;
                case "time":
                    return DbType.Time;
                case "timestamp":
                    return DbType.DateTime;
                case "timestamp with time zone":
                    return DbType.Object;
                case "tinyint":
                    return DbType.Byte;
                case "uniqueidentifier":
                    return DbType.Guid;
                case "uniqueidentifierstr":
                    return DbType.AnsiStringFixedLength;
                case "unsigned bigint":
                    return DbType.UInt64;
                case "unsigned int":
                    return DbType.UInt32;
                case "unsigned smallint":
                    return DbType.UInt16;
                case "varbinary":
                    return DbType.Binary;
                case "varbit":
                    return DbType.Binary;
                case "varchar":
                    return DbType.AnsiString;
                case "xml":
                    return DbType.Xml;

                default:
                    return DbType.Object;
            }
        }

        internal static SADbType GetSqlDbType(string nativeType) {
            switch (nativeType.Trim().ToLower()) {
                case "bigint":
                    return SADbType.BigInt;
                case "binary":
                    return SADbType.Binary;
                case "bit":
                    return SADbType.Bit;
                case "char":
                    return SADbType.Char;
                case "date":
                    return SADbType.Date;
                case "datetime":
                    return SADbType.DateTime;
                case "datetime2":
                    return SADbType.DateTime;
                case "datetimeoffset":
                    return SADbType.DateTimeOffset;
                case "decimal":
                    return SADbType.Decimal;
                case "double":
                    return SADbType.Double;
                case "float":
                    return SADbType.Float;
                case "image":
                    return SADbType.Image;
                case "int":
                    return SADbType.Integer;
                case "integer":
                    return SADbType.Integer;
                case "long nvarchar":
                    return SADbType.LongNVarchar;
                case "long binary":
                    return SADbType.LongBinary;
                case "long varbit":
                    return SADbType.LongVarbit;
                case "long varchar":
                    return SADbType.LongVarchar;
                case "money":
                    return SADbType.Money;
                case "nchar":
                    return SADbType.NChar;
                case "ntext":
                    return SADbType.NText;
                case "numeric":
                    return SADbType.Decimal;
                case "nvarchar":
                    return SADbType.NVarChar;
                case "real":
                    return SADbType.Real;
                case "smalldatetime":
                    return SADbType.SmallDateTime;
                case "smallint":
                    return SADbType.SmallInt;
                case "smallmoney":
                    return SADbType.SmallMoney;
                case "sql_variant":
                    return SADbType.LongBinary;
                case "sysname":
                    return SADbType.SysName;
                case "text":
                    return SADbType.Text;
                case "time":
                    return SADbType.Time;
                case "timestamp":
                    return SADbType.DateTime;
                case "timestamp with time zone":
                    return SADbType.DateTime;
                case "tinyint":
                    return SADbType.TinyInt;
                case "uniqueidentifier":
                    return SADbType.UniqueIdentifier;
                case "unsigned bigint":
                    return SADbType.UnsignedBigInt;
                case "unsigned int":
                    return SADbType.UnsignedInt;
                case "unsigned smallint":
                    return SADbType.UnsignedSmallInt;
                case "varbinary":
                    return SADbType.VarBinary;
                case "varbit":
                    return SADbType.VarBit;
                case "varchar":
                    return SADbType.VarChar;
                case "xml":
                    return SADbType.Xml;

                default:
                    return SADbType.LongBinary;
            }
        }

        internal static string GetNativeType(SADbType saDbType) {
            switch (saDbType) {
                case SADbType.BigInt:
                    return "bigint";
                case SADbType.Binary:
                    return "binary";
                case SADbType.Bit:
                    return "bit";
                case SADbType.Char:
                    return "char";
                case SADbType.Date:
                    return "date";
                case SADbType.DateTime:
                    return "datetime";
                    //case SADbType.DateTime2: return "datetime2";
                case SADbType.DateTimeOffset:
                    return "datetimeoffset";
                case SADbType.Decimal:
                    return "decimal";
                case SADbType.Double:
                    return "double";
                case SADbType.Float:
                    return "float";
                case SADbType.Image:
                    return "image";
                case SADbType.Integer:
                    return "integer";
                case SADbType.LongBinary:
                    return "long binary";
                case SADbType.LongNVarchar:
                    return "long nvarchar";
                case SADbType.LongVarbit:
                    return "long varbit";
                case SADbType.LongVarchar:
                    return "long varchar";
                case SADbType.Money:
                    return "money";
                case SADbType.NChar:
                    return "nchar";
                case SADbType.NText:
                    return "ntext";
                case SADbType.NVarChar:
                    return "nvarchar";
                case SADbType.Real:
                    return "real";
                case SADbType.SmallDateTime:
                    return "smalldatetime";
                case SADbType.SmallInt:
                    return "smallint";
                case SADbType.SmallMoney:
                    return "smallmoney";
                    //case SADbType.Variant: return "sql_variant";
                case SADbType.Text:
                    return "text";
                case SADbType.Time:
                    return "time";
                case SADbType.TimeStamp:
                    return "timestamp";
                case SADbType.TimeStampWithTimeZone:
                    return "timestamp with time zone";
                case SADbType.TinyInt:
                    return "tinyint";
                case SADbType.UniqueIdentifier:
                    return "uniqueidentifier";
                case SADbType.UnsignedBigInt:
                    return "unsigned bigint";
                case SADbType.UnsignedInt:
                    return "unsigned int";
                case SADbType.UnsignedSmallInt:
                    return "unsigned smallint";
                case SADbType.VarBinary:
                    return "varbinary";
                case SADbType.VarChar:
                    return "varchar";
                case SADbType.Xml:
                    return "xml";

                default:
                    return "sql_variant";
            }
        }

        internal static ParameterDirection GetParameterDirection(short parameterType) {
            switch (parameterType) {
                case 1:
                    return ParameterDirection.Input;
                case 2:
                    return ParameterDirection.InputOutput;
                case 3:
                    return ParameterDirection.Output;
                case 4:
                    return ParameterDirection.ReturnValue;
                default:
                    return ParameterDirection.Input;
            }
        }

        internal static Dictionary<string, TableSchema> ToDictionary(TableSchema table) {
            Dictionary<string, TableSchema> tableDictionary = new Dictionary<string, TableSchema>();
            tableDictionary.Add(table.FullName, table);
            return tableDictionary;
        }

        internal static Dictionary<string, TableSchema> ToDictionary(IList<TableSchema> tables) {
            Dictionary<string, TableSchema> tableDictionary = new Dictionary<string, TableSchema>();
            foreach (TableSchema table in tables)
                tableDictionary.Add(table.FullName, table);

            return tableDictionary;
        }

        internal static Dictionary<string, ViewSchema> ToDictionary(ViewSchema view) {
            Dictionary<string, ViewSchema> viewDictionary = new Dictionary<string, ViewSchema>();
            viewDictionary.Add(view.FullName, view);
            return viewDictionary;
        }

        internal static Dictionary<string, ViewSchema> ToDictionary(IList<ViewSchema> views) {
            Dictionary<string, ViewSchema> viewDictionary = new Dictionary<string, ViewSchema>();
            foreach (ViewSchema view in views)
                viewDictionary.Add(view.FullName, view);

            return viewDictionary;
        }

        internal static Dictionary<string, CommandSchema> ToDictionary(CommandSchema command) {
            Dictionary<string, CommandSchema> commandDictionary = new Dictionary<string, CommandSchema>();
            commandDictionary.Add(command.FullName, command);
            return commandDictionary;
        }

        internal static Dictionary<string, CommandSchema> ToDictionary(IList<CommandSchema> commands) {
            Dictionary<string, CommandSchema> commandDictionary = new Dictionary<string, CommandSchema>();
            foreach (CommandSchema command in commands)
                commandDictionary.Add(command.FullName, command);

            return commandDictionary;
        }

        internal static bool IsAnyNullOrEmpty(params string[] values) {
            if (values == null || values.Length == 0)
                return false;

            for (int i = 0; i < values.Length; i++)
                if (string.IsNullOrEmpty(values[i]))
                    return true;

            return false;
        }

        internal static bool ParseBooleanExtendedProperty(SchemaObjectBase schemaObjectBase, string extendedProperty) {
            if (schemaObjectBase == null)
                return false;

            var properties = schemaObjectBase.GetLoadedExtendedProperties();
            if (!properties.Contains(extendedProperty))
                return false;

            bool temp = false;
            bool.TryParse(properties[extendedProperty].Value.ToString(), out temp);

            return temp;
        }

    }
}