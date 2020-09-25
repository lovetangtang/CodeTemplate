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
using System.ComponentModel;
using System.Data;

using CodeSmith.Engine;

using SchemaExplorer;

namespace CodeSmith.BaseTemplates {
    public class VBSqlCodeTemplate : SqlCodeTemplate {
        /// <summary>
        /// Generates an assignment statement that adds a parameter to a ADO object for the given column.
        /// </summary>
        /// <param name="statementPrefix"></param>
        /// <param name="column"></param>
        /// <param name="sqlObjectName"></param>
        /// <returns></returns>
        public override string GetSqlParameterStatements(string statementPrefix, ColumnSchema column, string sqlObjectName) {
            string statements = String.Format("\r\n{0}{1}.AddParameter(\"@{2}\", SqlDbType.{3}, {4}{5}", statementPrefix, sqlObjectName, column.Name, GetSqlDbType( column ), GetPropertyName( column ), GetSqlParameterExtraParams( statementPrefix, column ));

            return statements.Substring(statementPrefix.Length + 2);
        }

        /// <summary>
        /// Generates any extra parameters that are needed for the ADO parameter statement.
        /// </summary>
        /// <param name="statementPrefix"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        protected override string GetSqlParameterExtraParams(string statementPrefix, DataObjectBase column) {
            switch (column.DataType) {
                case DbType.String:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    if (column.NativeType != "text" && column.NativeType != "ntext" && column.Size != -1)
                        return ", " + column.Size + ")";

                    return ")";
                case DbType.Decimal:
                    return String.Format(")\r\n{0}prm.Scale = {1}\r\n{0}prm.Precision = {2}", statementPrefix, column.Scale, column.Precision);
                default:
                    return ")";
            }
        }

        #region GetMemberVariableDeclarationStatement

        /// <summary>
        /// Returns a VB.Net member variable declaration statement.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string GetMemberVariableDeclarationStatement(ColumnSchema column) {
            return GetMemberVariableDeclarationStatement("Protected", column);
        }

        /// <summary>
        /// Returns a VB.Net member variable declaration statement.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public override string GetMemberVariableDeclarationStatement(ViewColumnSchema column) {
            return GetMemberVariableDeclarationStatement("Protected", column);
        }

        /// <summary>
        /// Returns a VB.Net member variable declaration statement.
        /// </summary>
        /// <param name="protectionLevel"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        protected override string GetMemberVariableDeclarationStatement(string protectionLevel, DataObjectBase column) {
            string statement = protectionLevel + " ";
            statement += GetMemberVariableName(column.Name) + " As " + GetVBVariableType(column);

            string defaultValue = GetMemberVariableDefaultValue(column);
            if (defaultValue != "")
                statement += " = " + defaultValue;

            return statement;
        }

        #endregion

        /// <summary>
        /// Returns a typed VB.Net reader.ReadXXX() statement.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public override string GetSqlReaderAssignmentStatement(ColumnSchema column, int index) {
            string statement = String.Format("If Not (reader.IsDBNull({0})) Then \r\n", index);
            statement += String.Format("{0} = ", GetMemberVariableName(column.Name));
            statement += String.Format("reader.{0}({1}) \r\nEnd If", GetReaderMethod(column), index);

            return statement;
        }

        /// <summary>
        /// Generates a batch of C# validation statements based on the column.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="statementPrefix"></param>
        /// <returns></returns>
        public override string GetValidateStatements(TableSchema table, string statementPrefix) {
            string statements = "";

            foreach (ColumnSchema column in table.Columns) {
                if (IncludeEmptyCheck(column))
                    statements += String.Format("\r\n{0}If ({1} = {2}) Then \r\n this.ValidationErrors.Add(new ValidationError(ValidationTypeCode.Required, \"{3}\", \"{4}\", \"{4} is required.\")) \r\n End If", statementPrefix, GetMemberVariableName(column.Name), GetMemberVariableDefaultValue(column), table.Name, column.Name);
                if (IncludeMaxLengthCheck(column))
                    statements += String.Format("\r\n{0}If ({1}.Length > {2}) Then \r\n this.ValidationErrors.Add(new ValidationError(ValidationTypeCode.MaxLength, \"{3}\", \"{4}\", \"{4} is too long.\")) \r\n End If", statementPrefix, GetMemberVariableName(column.Name), column.Size, table.Name, column.Name);
            }

            return statements.Substring(statementPrefix.Length + 2);
        }

        #region GetVBVariableType

        /// <summary>
        /// Returns the C# variable type based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        private string GetVBVariableType(DataObjectBase column) {
            return DbTypeToVB[column.DataType.ToString(), column.SystemType.Name];
        }

        /// <summary>
        /// Returns the C# variable type based on the given dataType.
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public virtual string GetVBVariableType(DbType dataType) {
            return DbTypeToVB[dataType.ToString(), "Object"];
        }

        /// <summary>
        /// Returns the C# variable type based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetVBVariableType(ColumnSchema column) {
            return GetVBVariableType(column as DataObjectBase);
        }

        /// <summary>
        /// Returns the C# variable type based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetVBVariableType(ViewColumnSchema column) {
            return GetVBVariableType(column as DataObjectBase);
        }

        /// <summary>
        /// Returns the C# variable type based on the given column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetVBVariableType(CommandResultColumnSchema column) {
            return GetVBVariableType(column as DataObjectBase);
        }

        /// <summary>
        /// Returns the C# variable type based on the given parameter.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetVBVariableType(ParameterSchema column) {
            return GetVBVariableType(column as DataObjectBase);
        }

        #endregion

        #region Properties

        private MapCollection _dbTypeToVB;

        [Browsable(false)]
        public MapCollection DbTypeToVB {
            get {
                if (_dbTypeToVB == null) {
                    string directoryName = CodeTemplateInfo != null ? CodeTemplateInfo.DirectoryName : string.Empty;

                    string path;
                    Map.TryResolvePath("DbType-VB", directoryName, out path);
                    _dbTypeToVB = Map.Load(path);
                }
                return _dbTypeToVB;
            }
        }

        #endregion
    }
}