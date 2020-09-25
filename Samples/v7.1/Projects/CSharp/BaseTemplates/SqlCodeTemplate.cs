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
using System.ComponentModel;
using System.Data;

using CodeSmith.Engine;

using SchemaExplorer;

namespace CodeSmith.BaseTemplates {
    /// <summary>
    /// Templates can be derived from this class to gain various helper methods that are useful when generating code from a SQL metadata source.
    /// </summary>
    public class SqlCodeTemplate : OutputFileCodeTemplate {
        /// <summary>
        /// Generates an assignment statement that adds a parameter to a ADO object for the given column.
        /// </summary>
        /// <param name="statementPrefix"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetSqlParameterStatements(string statementPrefix, ColumnSchema column) {
            return GetSqlParameterStatements(statementPrefix, column, "sql");
        }

        /// <summary>
        /// Generates an assignment statement that adds a parameter to a ADO object for the given column.
        /// </summary>
        /// <param name="statementPrefix"></param>
        /// <param name="column"></param>
        /// <param name="sqlObjectName"></param>
        /// <returns></returns>
        public virtual string GetSqlParameterStatements(string statementPrefix, ColumnSchema column, string sqlObjectName) {
            string statements = String.Format("\r\n{0}{1}.AddParameter(\"@{2}\", SqlDbType.{3}, this.{4}{5}", statementPrefix, sqlObjectName, column.Name, GetSqlDbType(column), GetPropertyName(column), GetSqlParameterExtraParams(statementPrefix, column));

            return statements.Substring(statementPrefix.Length + 2);
        }

        /// <summary>
        /// Returns the C# member variable name for a given identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetMemberVariableName(string value) {
            string memberVariableName = String.Format("_{0}", StringUtil.ToCamelCase(value));

            return memberVariableName;
        }

        #region GetSqlParameterExtraParams

        /// <summary>
        /// Generates any extra parameters that are needed for the ADO parameter statement.
        /// </summary>
        /// <param name="statementPrefix"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetSqlParameterExtraParams(string statementPrefix, ColumnSchema column) {
            return GetSqlParameterExtraParams(statementPrefix, column as DataObjectBase);
        }

        /// <summary>
        /// Generates any extra parameters that are needed for the ADO parameter statement.
        /// </summary>
        /// <param name="statementPrefix"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetSqlParameterExtraParams(string statementPrefix, ViewColumnSchema column) {
            return GetSqlParameterExtraParams(statementPrefix, column as DataObjectBase);
        }

        /// <summary>
        /// Generates any extra parameters that are needed for the ADO parameter statement.
        /// </summary>
        /// <param name="statementPrefix"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        protected virtual string GetSqlParameterExtraParams(string statementPrefix, DataObjectBase column) {
            switch (column.DataType) {
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    if (column.NativeType != "text" && column.NativeType != "ntext" && column.Size != -1)
                        return String.Format(", {0});", column.Size);

                    return ");";
                case DbType.Decimal:
                    return String.Format(");\r\n{0}prm.Scale = {1};\r\n{0}prm.Precision = {2};", statementPrefix, column.Scale, column.Precision);
                default:
                    return ");";
            }
        }

        #endregion

        #region GetMemberVariableDeclarationStatement

        /// <summary>
        /// Returns a C# member variable declaration statement.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual string GetMemberVariableDeclarationStatement(ColumnSchema column) {
            return GetMemberVariableDeclarationStatement("protected", column);
        }

        /// <summary>
        /// Returns a C# member variable declaration statement.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public virtual string GetMemberVariableDeclarationStatement(ViewColumnSchema column) {
            return GetMemberVariableDeclarationStatement("protected", column);
        }

        /// <summary>
        /// Returns a C# member variable declaration statement.
        /// </summary>
        /// <param name="protectionLevel"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetMemberVariableDeclarationStatement(string protectionLevel, ColumnSchema column) {
            return GetMemberVariableDeclarationStatement(protectionLevel, column as DataObjectBase);
        }

        /// <summary>
        /// Returns a C# member variable declaration statement.
        /// </summary>
        /// <param name="protectionLevel"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetMemberVariableDeclarationStatement(string protectionLevel, ViewColumnSchema column) {
            return GetMemberVariableDeclarationStatement(protectionLevel, column as DataObjectBase);
        }

        /// <summary>
        /// Returns a C# member variable declaration statement.
        /// </summary>
        /// <param name="protectionLevel"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        protected virtual string GetMemberVariableDeclarationStatement(string protectionLevel, DataObjectBase column) {
            string statement = String.Format("{0} ", protectionLevel);
            statement += String.Format("{0} {1}", GetCSharpVariableType(column), GetMemberVariableName(column.Name));

            string defaultValue = GetMemberVariableDefaultValue(column);
            if (defaultValue != "")
                statement += String.Format(" = {0}", defaultValue);

            statement += ";";

            return statement;
        }

        #endregion

        /// <summary>
        /// Returns a typed C# reader.ReadXXX() statement.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual string GetSqlReaderAssignmentStatement(ColumnSchema column, int index) {
            string statement = String.Format("if (!reader.IsDBNull({0})) ", index);
            statement += String.Format("{0} = ", GetMemberVariableName(column.Name));

            statement += String.Format("reader.{0}({1});", GetReaderMethod(column), index);

            return statement;
        }

        /// <summary>
        /// Generates a batch of C# validation statements based on the column.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="statementPrefix"></param>
        /// <returns></returns>
        public virtual string GetValidateStatements(TableSchema table, string statementPrefix) {
            string statements = "";

            foreach (ColumnSchema column in table.Columns) {
                if (IncludeEmptyCheck(column))
                    statements += String.Format("\r\n{0}if ({1} == {2}) this.ValidationErrors.Add(new ValidationError(ValidationTypeCode.Required, \"{3}\", \"{4}\", \"{4} is required.\"));", statementPrefix, GetMemberVariableName(column.Name), GetMemberVariableDefaultValue(column), table.Name, column.Name);
                if ( IncludeMaxLengthCheck(column))
                    statements += String.Format("\r\n{0}if ({1}.Length > {2}) this.ValidationErrors.Add(new ValidationError(ValidationTypeCode.MaxLength, \"{3}\", \"{4}\", \"{4} is too long.\"));", statementPrefix, GetMemberVariableName(column.Name), column.Size, table.Name, column.Name);
            }

            return statements.Substring(statementPrefix.Length + 2);
        }

        /// <summary>
        /// Returns the name of the public property for a given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetPropertyName(ColumnSchema column) {
            string propertyName = column.Name;

            if (propertyName == String.Format("{0}Name", column.Table.Name))
                return "Name";
            if (propertyName == String.Format("{0}Description", column.Table.Name))
                return "Description";

            return propertyName;
        }

        #region DbTypeToCSharp

        private MapCollection _dbTypeToCSharp;

        [Browsable(false)]
        public MapCollection DbTypeToCSharp {
            get {
                if (_dbTypeToCSharp == null) {
                    string directoryName = CodeTemplateInfo != null ? CodeTemplateInfo.DirectoryName : string.Empty;

                    string path;
                    Map.TryResolvePath("DbType-CSharp", directoryName, out path);
                    _dbTypeToCSharp = Map.Load(path);
                }
                return _dbTypeToCSharp;
            }
        }

        #endregion

        #region GetCSharpVariableType

        /// <summary>
        /// Returns the C# variable type based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private string GetCSharpVariableType(DataObjectBase column) {
            return DbTypeToCSharp[column.DataType.ToString(), column.SystemType.Name];
        }

        /// <summary>
        /// Returns the C# variable type based on the given dataType.
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public string GetCSharpVariableType(DbType dataType) {
            return DbTypeToCSharp[dataType.ToString(), "object"];
        }

        /// <summary>
        /// Returns the C# variable type based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetCSharpVariableType(ColumnSchema column) {
            return GetCSharpVariableType(column as DataObjectBase);
        }

        /// <summary>
        /// Returns the C# variable type based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetCSharpVariableType(ViewColumnSchema column) {
            return GetCSharpVariableType(column as DataObjectBase);
        }

        /// <summary>
        /// Returns the C# variable type based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetCSharpVariableType(CommandResultColumnSchema column) {
            return GetCSharpVariableType(column as DataObjectBase);
        }

        /// <summary>
        /// Returns the C# variable type based on the given parameter.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetCSharpVariableType(ParameterSchema column) {
            return GetCSharpVariableType(column as DataObjectBase);
        }

        #endregion

        #region DbTypeToDataReaderMethod

        private MapCollection _dbTypeToDataReaderMethod;

        [Browsable(false)]
        public MapCollection DbTypeToDataReaderMethod {
            get {
                if (_dbTypeToDataReaderMethod == null) {
                    string directoryName = CodeTemplateInfo != null ? CodeTemplateInfo.DirectoryName : string.Empty;

                    string path;
                    if (Map.TryResolvePath("DbType-DataReaderMethod", directoryName, out path))
                        _dbTypeToDataReaderMethod = Map.Load(path);
                }
                return _dbTypeToDataReaderMethod;
            }
        }

        #endregion

        #region GetReaderMethod

        /// <summary>
        /// Returns the name of the typed reader method for a given column.
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public string GetReaderMethod(DbType dataType) {
            return DbTypeToDataReaderMethod[dataType.ToString(), "GetValue"];
        }

        /// <summary>
        /// Returns the name of the typed reader method for a given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetReaderMethod(ColumnSchema column) {
            return GetReaderMethod(column.DataType);
        }

        /// <summary>
        /// Returns the name of the typed reader method for a given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetReaderMethod(ViewColumnSchema column) {
            return GetReaderMethod(column.DataType);
        }

        #endregion

        #region GetSqlDbType

        /// <summary>
        /// Returns the SqlDbType based on a string.
        /// </summary>
        /// <param name="nativeType"></param>
        /// <returns></returns>
        public string GetSqlDbType(string nativeType) {
            string lowerCaseNativeType = nativeType.Trim().ToLower();
            switch (lowerCaseNativeType) {
                case "bigint":
                    return "BigInt";
                case "binary":
                    return "Binary";
                case "bit":
                    return "Bit";
                case "char":
                    return "Char";
                case "datetime":
                    return "DateTime";
                case "decimal":
                    return "Decimal";
                case "float":
                    return "Float";
                case "image":
                    return "Image";
                case "int":
                    return "Int";
                case "money":
                    return "Money";
                case "nchar":
                    return "NChar";
                case "ntext":
                    return "NText";
                case "numeric":
                    return "Decimal";
                case "nvarchar":
                    return "NVarChar";
                case "real":
                    return "Real";
                case "smalldatetime":
                    return "SmallDateTime";
                case "smallint":
                    return "SmallInt";
                case "smallmoney":
                    return "SmallMoney";
                case "sql_variant":
                    return "Variant";
                case "sysname":
                    return "NChar";
                case "text":
                    return "Text";
                case "timestamp":
                    return "Timestamp";
                case "tinyint":
                    return "TinyInt";
                case "uniqueidentifier":
                    return "UniqueIdentifier";
                case "varbinary":
                    return "VarBinary";
                case "varchar":
                    return "VarChar";
                case "xml":
                    return "Xml";
                case "datetime2":
                    return "DateTime2";
                case "time":
                    return "Time";
                case "date":
                    return "Date";
                case "datetimeoffset":
                    return "DateTimeOffset";
                default:
                    return nativeType;
            }
        }

        public string GetSqlDbType(ColumnSchema column) {
            return GetSqlDbType(column.NativeType);
        }

        public string GetSqlDbType(ViewColumnSchema column) {
            return GetSqlDbType(column.NativeType);
        }

        #endregion

        #region IsUserDefinedType

        /// <summary>
        /// Determine if the given column is using a UDT.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected static bool IsUserDefinedType(DataObjectBase column) {
            switch (column.NativeType.Trim().ToLower()) {
                case "bigint":
                case "binary":
                case "bit":
                case "char":
                case "date":
                case "datetime":
                case "datetime2":
                case "datetimeoffset":
                case "time":
                case "decimal":
                case "float":
                case "image":
                case "int":
                case "money":
                case "nchar":
                case "ntext":
                case "numeric":
                case "nvarchar":
                case "real":
                case "smalldatetime":
                case "smallint":
                case "smallmoney":
                case "sql_variant":
                case "sysname":
                case "text":
                case "timestamp":
                case "tinyint":
                case "uniqueidentifier":
                case "varbinary":
                case "xml":
                case "varchar":
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Determine if the given column is using a UDT.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IsUserDefinedType(ViewColumnSchema column) {
            return IsUserDefinedType(column as DataObjectBase);
        }

        /// <summary>
        /// Determine if the given column is using a UDT.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IsUserDefinedType(ColumnSchema column) {
            return IsUserDefinedType(column as DataObjectBase);
        }

        #endregion

        #region GetMemberVariableDefaultValue

        /// <summary>
        /// Returns a default value based on a column's data type.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected static string GetMemberVariableDefaultValue(DataObjectBase column) {
            switch (column.DataType) {
                case DbType.Guid: {
                    return "Guid.Empty";
                }
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength: {
                    return "String.Empty";
                }
                default: {
                    return "";
                }
            }
        }

        public string GetMemberVariableDefaultValue(ColumnSchema column) {
            return GetMemberVariableDefaultValue(column as DataObjectBase);
        }

        public string GetMemberVariableDefaultValue(ViewColumnSchema column) {
            return GetMemberVariableDefaultValue(column as DataObjectBase);
        }

        #endregion

        #region IncludeMaxLengthCheck

        /// <summary>
        /// Determines if the given column's data type requires a max length to be defined.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IncludeMaxLengthCheck(ColumnSchema column) {
            return IncludeMaxLengthCheck(column as DataObjectBase);
        }

        /// <summary>
        /// Determines if the given column's data type requires a max length to be defined.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IncludeMaxLengthCheck(ViewColumnSchema column) {
            return IncludeMaxLengthCheck(column as DataObjectBase);
        }

        /// <summary>
        /// Determines if the given column's data type requires a max length to be defined.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private static bool IncludeMaxLengthCheck(DataObjectBase column) {
            switch (column.DataType) {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength: {
                    if (column.NativeType != "text" && column.NativeType != "ntext" && column.Size != -1)
                        return true;

                    return false;
                }
                default: {
                    return false;
                }
            }
        }

        #endregion

        /// <summary>
        /// Determines if a given column should use a check for an Empty value.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IncludeEmptyCheck(ColumnSchema column) {
            if (column.IsPrimaryKeyMember || column.AllowDBNull)
                return false;

            switch (column.DataType) {
                case DbType.Guid: {
                    return true;
                }
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength: {
                    return true;
                }
                default: {
                    return false;
                }
            }
        }

        #region GetSqlParameterStatement

        /// <summary>
        /// Returns a T-SQL parameter statement based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetSqlParameterStatement(ColumnSchema column) {
            return GetSqlParameterStatement(column, false);
        }

        /// <summary>
        /// Returns a T-SQL parameter statement based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetSqlParameterStatement(ViewColumnSchema column) {
            return GetSqlParameterStatement(column, false);
        }

        #endregion

        /// <summary>
        /// Returns a camel cased name from the given identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetCamelCaseName(string value) {
            return StringUtil.ToCamelCase(value);
        }

        /// <summary>
        /// Returns a spaced out version of the identifier.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetSpacedName(string value) {
            return StringUtil.ToSpacedWords(value);
        }

        #region GetSqlParameterStatement

        /// <summary>
        /// Returns a T-SQL parameter statement based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="isOutput"></param>
        /// <returns></returns>
        public string GetSqlParameterStatement(ColumnSchema column, bool isOutput) {
            return GetSqlParameterStatement(column as DataObjectBase, isOutput);
        }

        /// <summary>
        /// Returns a T-SQL parameter statement based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="isOutput"></param>
        /// <returns></returns>
        public string GetSqlParameterStatement(ViewColumnSchema column, bool isOutput) {
            return GetSqlParameterStatement(column as DataObjectBase, isOutput);
        }

        /// <summary>
        /// Returns a T-SQL parameter statement based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="isOutput"></param>
        /// <returns></returns>
        protected static string GetSqlParameterStatement(DataObjectBase column, bool isOutput) {
            string param = string.Format("@{0} {1}", GetDelimitedIdentifier(column), column.NativeType);
            if (!IsUserDefinedType(column))
                switch (column.DataType) {
                    case DbType.Decimal: {
                        param += string.Format("({0}, {1})", column.Precision, column.Scale);
                        break;
                    }
                    case DbType.Binary:
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                    case DbType.String:
                    case DbType.StringFixedLength: {
                        if (column.NativeType != "text" && column.NativeType != "ntext" &&
                            column.NativeType != "timestamp" && column.NativeType != "image")
                            if (column.Size > 0)
                                param += string.Format("({0})", column.Size);
                            else if (column.Size == -1)
                                param += "(max)";
                        break;
                    }
                }

            if (isOutput)
                param += " OUTPUT";

            return param;
        }

        #endregion

        #region GetSqlParameterName

        /// <summary>
        /// This will look up and see if a column name should be escaped with delimited identifiers '[]'.
        /// http://msdn.microsoft.com/en-us/library/aa224033(SQL.80).aspx
        /// </summary>
        /// <param name="column">The source.</param>
        /// <returns></returns>
        protected static string GetDelimitedIdentifier(DataObjectBase column) {
            return GetDelimitedIdentifier(column.Name, false);
        }

        /// <summary>
        /// This will look up and see if a column name should be escaped with delimited identifiers '[]'.
        /// http://msdn.microsoft.com/en-us/library/aa224033(SQL.80).aspx
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="includeDelimitedIdentifier">Enforces that a delimited identifiers are added.</param>
        /// <returns></returns>
        protected static string GetDelimitedIdentifier(string source, bool includeDelimitedIdentifier) {
            //TODO: update this list to support all TSQL keywords: http://msdn.microsoft.com/en-us/library/ms189822.aspx

            // If the name is already escaped then exit.. We could check to see if the front or the back is missing a bracket...
            if (string.IsNullOrEmpty(source) || source.StartsWith("[") || source.EndsWith("]"))
                return source;

            var escapeList = new List<string> {" ", "'", "~", "!", "%", "^", "&", "(", ")", "-", "{", "}", ".", @"\", "`"};
            if (StringUtil.ContainsString(source, escapeList) || includeDelimitedIdentifier)
                return string.Format("[{0}]", source);

            return source;
        }

        #endregion
    }
}