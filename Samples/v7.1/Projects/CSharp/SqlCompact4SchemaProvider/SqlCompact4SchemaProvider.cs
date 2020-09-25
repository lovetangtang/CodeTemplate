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
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Windows.Forms;

namespace SchemaExplorer
{
    /// <summary>
    /// CodeSmith SQL CE Schema Provider
    /// Original code from JHeidt @ http://community.codesmithtools.com/forums/p/5921/23086.aspx#23086
    /// Enhancement performed by Chris Lasater
    /// Upgraded and enhanced for 4.0 by Erik Ejlskov @ http://erikej.blogspot.com And Blake Niemyjski http://blakeniemyjski.com/
    /// </summary>
    public class SqlCompact4SchemaProvider : IDbSchemaProvider, IDbConnectionStringEditor
    {
        #region IDbSchemaProvider Members

        /// <summary>
        /// Return the Constant name of the SchemaProvider.
        /// </summary>
        public string Name
        {
            get { return "SqlCompact4SchemaProvider"; }
        }

        /// <summary>
        /// Returns the name of the SchemaProvider.
        /// </summary>
        public string Description
        {
            get { return "SQL Compact Schema Provider (v4.0)"; }
        }

        #endregion

        #region Implementation of IDbConnectionStringEditor

        /// <summary>
        /// Shows the Connection Editor.
        /// </summary>
        /// <param name="currentConnectionString"></param>
        /// <returns></returns>
        public bool ShowEditor(string currentConnectionString)
        {
            var editor = new SqlCompactConnectionStringEditor(currentConnectionString);
            DialogResult result = editor.ShowDialog();
            if (result != DialogResult.OK)
                return false;

            ConnectionString = editor.ConnectionString;
            return true;
        }

        public string ConnectionString { get; private set; }

        public bool EditorAvailable
        {
            get { return true; }
        }

        #endregion

        #region Table Retrieval Methods

        /// <summary>
        /// Gets all of the tables available in the database.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {            
            //Erik Ejlskov - exclude system tables
            const string sql = @"select [TABLE_NAME] as [TableName] from INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'TABLE' ORDER BY TABLE_NAME";

            var tables = new List<TableSchema>();

            using (SqlCeCommand cmd = GetCeCommand(connectionString, sql))
            {
                using (SqlCeResultSet results = cmd.ExecuteResultSet(ResultSetOptions.None))
                {
                    while (results.Read())
                    {
                        var tableName = (string) results["TableName"];
                        tables.Add(new TableSchema(database, tableName, string.Empty, DateTime.MinValue));
                    }
                }
            }

            return tables.ToArray();
        }

        /// <summary>
        /// Gats all of the indexes for a given table.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="table"></param>
        /// <returns></returns>
        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            string sql = string.Format("SELECT DISTINCT INDEX_NAME as [IndexName] FROM INFORMATION_SCHEMA.INDEXES WHERE TABLE_NAME = '{0}'  AND SUBSTRING(COLUMN_NAME, 1,5) <> '__sys' ORDER BY INDEX_NAME ASC", table.Name);

            var indexNames = new List<string>();
            var indices = new List<IndexSchema>();

            using (SqlCeCommand cmd = GetCeCommand(connectionString, sql))
            {
                using (SqlCeResultSet results = cmd.ExecuteResultSet(ResultSetOptions.None))
                {
                    while (results.Read())
                    {
                        indexNames.Add((string) results["IndexName"] ?? String.Empty);
                    }
                }
            }

            foreach (string indexName in indexNames)
            {
                //Erik Ejlskov - corrected SQL statement to include table name!
                string getIndexSql = string.Format(@"SELECT 
                                                        INDEX_NAME as [Name],
                                                        PRIMARY_KEY as [IsPrimaryKey],
                                                        [UNIQUE] as [IsUnique],
                                                        [CLUSTERED] as [IsClustered],
                                                        [COLUMN_NAME] as [ColumnName] 
                                                    FROM 
                                                        INFORMATION_SCHEMA.INDEXES 
                                                    WHERE 
                                                        INDEX_NAME = '{0}' AND 
                                                        TABLE_NAME = '{1}' 
                                                    ORDER BY 
                                                        [Name],
                                                        [ORDINAL_POSITION]", indexName, table.Name);

                // string indexName = String.Empty;
                bool isPrimaryKey = false;
                bool isUnique = false;
                bool isClustered = false;
                var memberColumns = new List<string>();
                bool read = false;

                using (SqlCeCommand cmd = GetCeCommand(connectionString, getIndexSql))
                {
                    using (SqlCeResultSet results = cmd.ExecuteResultSet(ResultSetOptions.None))
                    {
                        while (results.Read())
                        {
                            if (!read)
                            {
                                // indexName = (string)results["Name"];
                                isPrimaryKey = (bool) results["IsPrimaryKey"];
                                isUnique = (bool) results["IsUnique"];
                                isClustered = (bool) results["IsClustered"];
                                memberColumns.Add((string) results["ColumnName"]);
                                read = true;
                            }
                            else
                            {
                                memberColumns.Add((string) results["ColumnName"]);
                            } // if(!read)
                        } // while

                        indices.Add(new IndexSchema(table, indexName, isPrimaryKey, isUnique, isClustered, memberColumns.ToArray()));
                    } // using(results)
                } // using(cmd)
            } //foreach(indexName)

            return indices.ToArray();
        }


        /// <summary>
        /// Gets all columns for a given table.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="table"></param>
        /// <returns></returns>
        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            //Erik Ejlskov - exclude system columns
            string getColumnSql = string.Format(@"SELECT 
                                                    COLUMN_NAME as [Name], 
                                                    COLUMN_DEFAULT as [Default], 
                                                    IS_NULLABLE as [IsNullable], 
                                                    DATA_TYPE as [DataType], 
                                                    CHARACTER_MAXIMUM_LENGTH as [Length], 
                                                    NUMERIC_PRECISION as [Precision], 
                                                    NUMERIC_SCALE as [Scale], 
                                                    AUTOINC_SEED, 
                                                    AUTOINC_INCREMENT, 
                                                    COLUMN_HASDEFAULT, 
                                                    COLUMN_FLAGS 
                                                 FROM 
                                                    INFORMATION_SCHEMA.COLUMNS 
                                                 WHERE 
                                                    TABLE_NAME = '{0}' AND 
                                                    COLUMN_FLAGS <> 98 AND 
                                                    COLUMN_FLAGS <> 114 
                                                 ORDER BY 
                                                    ORDINAL_POSITION", table.Name);

            var columns = new List<ColumnSchema>();

            using (SqlCeCommand cmd = GetCeCommand(connectionString, getColumnSql))
            {
                using (SqlCeResultSet results = cmd.ExecuteResultSet(ResultSetOptions.None))
                {
                    while (results.Read())
                    {
                        var extendedProperties = new List<ExtendedProperty>();

                        if (!results.IsDBNull(7))
                        {
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IsIdentity, true, DbType.Boolean));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IdentitySeed, results["AUTOINC_SEED"].ToString(), DbType.String));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IdentityIncrement, results["AUTOINC_INCREMENT"].ToString(), DbType.String));
                        }
                        else
                        {
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IsIdentity, false, DbType.Boolean));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IdentitySeed, "0", DbType.String));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IdentityIncrement, "0", DbType.String));
                        }

                        if (results["COLUMN_HASDEFAULT"] != null)
                        {
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, results["Default"].ToString(), DbType.String));
                        }
                        else
                        {
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, string.Empty, DbType.String));
                        }

                        var name = (string) results["Name"];
                        var nativeType = (string) results["DataType"];
                        //Erik Ejlskov - should be "timestamp" instead
                        if (nativeType == "rowversion")
                        {
                            nativeType = "timestamp";
                        }
                        DbType dataType = GetDbTypeFromString(nativeType);
                        if ((dataType == DbType.Guid && results.GetInt32(10) == 378) || (dataType == DbType.Guid && results.GetInt32(10) == 282))
                        {
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IsRowGuidColumn, true, DbType.Boolean));
                        }
                        else
                        {
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IsRowGuidColumn, false, DbType.Boolean));
                        }

                        int size;
                        int.TryParse(results["Length"].ToString(), out size);

                        byte precision;
                        byte.TryParse(results["Precision"].ToString(), out precision);

                        int scale;
                        int.TryParse(results["scale"].ToString(), out scale);

                        bool allowNull = GetBoolFromYesNo((string) results["IsNullable"]);

                        var s = new ColumnSchema(table, name, dataType, nativeType, size, precision, scale, allowNull, extendedProperties.ToArray());
                        columns.Add(s);
                    } // while(read)
                } // using(results)             
            } // using(command)

            return columns.ToArray();
        }

        /// <summary>
        /// Gets all of the table keys for a given table.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="table"></param>
        /// <returns></returns>
        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            var keyNames = new List<string>();

            string sql = string.Format("SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_TABLE_NAME = '{0}' ", table.Name);

            using (SqlCeCommand cmd = GetCeCommand(connectionString, sql))
            {
                using (SqlCeResultSet results = cmd.ExecuteResultSet(ResultSetOptions.None))
                {
                    while (results.Read())
                    {
                        keyNames.Add(results[0].ToString());
                    } // while
                }
            }

            var keys = new List<TableKeySchema>();

            string name = string.Empty;
            string fkTable = string.Empty;
            string pkTable = string.Empty;

            foreach (string keyName in keyNames)
            {
                sql = string.Format("SELECT KCU1.TABLE_NAME AS FK_TABLE_NAME, KCU1.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME, KCU1.COLUMN_NAME AS FK_COLUMN_NAME, KCU2.TABLE_NAME AS UQ_TABLE_NAME, KCU2.CONSTRAINT_NAME AS UQ_CONSTRAINT_NAME, KCU2.COLUMN_NAME AS UQ_COLUMN_NAME, KCU2.ORDINAL_POSITION AS UQ_ORDINAL_POSITION, KCU1.ORDINAL_POSITION AS FK_ORDINAL_POSITION FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU1 ON KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2 ON  KCU2.CONSTRAINT_NAME =  RC.UNIQUE_CONSTRAINT_NAME AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION WHERE KCU1.TABLE_NAME = '{0}' AND KCU2.TABLE_NAME = RC.UNIQUE_CONSTRAINT_TABLE_NAME AND  KCU1.CONSTRAINT_NAME = '{1}' ORDER BY FK_TABLE_NAME, FK_CONSTRAINT_NAME, FK_ORDINAL_POSITION", table.Name, keyName);

                using (SqlCeCommand cmd = GetCeCommand(connectionString, sql))
                {
                    using (SqlCeResultSet results = cmd.ExecuteResultSet(ResultSetOptions.None))
                    {
                        bool hasRows = false;
                        var pkCols = new List<string>();
                        var fkCols = new List<string>();

                        while (results.Read())
                        {
                            hasRows = true;
                            name = (string) results["FK_CONSTRAINT_NAME"];
                            fkTable = (string) results["FK_TABLE_NAME"];
                            pkTable = (string) results["UQ_TABLE_NAME"];
                            fkCols.Add((string) results["FK_COLUMN_NAME"]);
                            pkCols.Add((string) results["UQ_COLUMN_NAME"]);
                        } // while
                        if (hasRows)
                        {
                            keys.Add(new TableKeySchema(table.Database, name, fkCols.ToArray(), fkTable, pkCols.ToArray(), pkTable));
                        }
                    }
                }
            }

            keyNames.Clear();

            sql = string.Format("SELECT CONSTRAINT_NAME FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE UNIQUE_CONSTRAINT_TABLE_NAME = '{0}' ", table.Name);

            using (SqlCeCommand cmd = GetCeCommand(connectionString, sql))
            {
                using (SqlCeResultSet results = cmd.ExecuteResultSet(ResultSetOptions.None))
                {
                    while (results.Read())
                    {
                        keyNames.Add(results[0].ToString());
                    } // while
                }
            }

            foreach (string keyName in keyNames)
            {
                //Then get keys pointing to this table
                sql = string.Format("SELECT KCU1.TABLE_NAME AS FK_TABLE_NAME, KCU1.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME, KCU1.COLUMN_NAME AS FK_COLUMN_NAME, KCU2.TABLE_NAME AS UQ_TABLE_NAME, KCU2.CONSTRAINT_NAME AS UQ_CONSTRAINT_NAME, KCU2.COLUMN_NAME AS UQ_COLUMN_NAME, KCU2.ORDINAL_POSITION AS UQ_ORDINAL_POSITION, KCU1.ORDINAL_POSITION AS FK_ORDINAL_POSITION FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU1 ON KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU2 ON  KCU2.CONSTRAINT_NAME =  RC.UNIQUE_CONSTRAINT_NAME AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION WHERE KCU2.TABLE_NAME = '{0}' AND KCU1.TABLE_NAME <> '{0}' AND KCU2.TABLE_NAME = RC.UNIQUE_CONSTRAINT_TABLE_NAME AND KCU1.CONSTRAINT_NAME = '{1}' ORDER BY FK_TABLE_NAME, FK_CONSTRAINT_NAME, FK_ORDINAL_POSITION", table.Name, keyName);

                using (SqlCeCommand cmd = GetCeCommand(connectionString, sql))
                {
                    using (SqlCeResultSet results = cmd.ExecuteResultSet(ResultSetOptions.None))
                    {
                        bool hasRows = false;
                        var pkCols = new List<string>();
                        var fkCols = new List<string>();

                        while (results.Read())
                        {
                            hasRows = true;
                            name = (string) results["FK_CONSTRAINT_NAME"];

                            pkTable = (string) results["UQ_TABLE_NAME"];
                            pkCols.Add((string) results["UQ_COLUMN_NAME"]);

                            fkTable = (string) results["FK_TABLE_NAME"];
                            fkCols.Add((string) results["FK_COLUMN_NAME"]);
                        } // while
                        if (hasRows)
                        {
                            keys.Add(new TableKeySchema(table.Database, name, fkCols.ToArray(), fkTable, pkCols.ToArray(), pkTable));
                        }
                    }
                }
            }

            return keys.ToArray();
        }

        /// <summary>
        /// Gets the primary key for a given table.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="table"></param>
        /// <returns></returns>
        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            // Erik Ejlskov - corrected SQL statement
            string sql = string.Format("select KCU.[CONSTRAINT_NAME] as [Constraint], KCU.[COLUMN_NAME] as [ColumnName] from INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC ON TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME where KCU.TABLE_NAME = '{0}' AND TC.TABLE_NAME =  '{0}' AND TC.CONSTRAINT_TYPE = 'PRIMARY KEY' order by KCU.ORDINAL_POSITION", table.Name);

            string name = String.Empty;
            var cols = new List<string>();

            using (SqlCeCommand cmd = GetCeCommand(connectionString, sql))
            {
                using (SqlCeResultSet results = cmd.ExecuteResultSet(ResultSetOptions.None))
                {
                    while (results.Read())
                    {
                        name = (string) results["Constraint"];
                        cols.Add((string) results["ColumnName"]);
                    }
                }
            }
            if (name == String.Empty && cols.Count == 0)
            {
                return null;
            }
            return new PrimaryKeySchema(table, name, cols.ToArray());
        }

        /// <summary>
        /// Gets the data from the given table.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="table"></param>
        /// <returns></returns>
        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            var tbl = new DataTable(table.Name);
            string sql = string.Format("SELECT * FROM [{0}]", table.Name);
            using (var adapter = new SqlCeDataAdapter(GetCeCommand(connectionString, sql)))
            {
                adapter.Fill(tbl);
            }
            return tbl;
        }

        #endregion

        #region Extended Properties

        /// <summary>
        /// Gets the extended properties for a given schema object.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="schemaObject"></param>
        /// <returns></returns>
        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            return new ExtendedProperty[0];
        }

        /// <summary>
        /// Sets the extended properties.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="schemaObject">The schema object.</param>
        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region View Retrieval Methods

        /// <summary>
        /// Gets all the views available for a given database.
        /// Used by CodeSmith studio test button, so also try to open a connection
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            GetConnection(connectionString);
            return new ViewSchema[0];
        }

        /// <summary>
        /// Gets the definition for a given view.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="view"></param>
        /// <returns></returns>
        public string GetViewText(string connectionString, ViewSchema view)
        {
            return "";
        }

        /// <summary>
        /// Gets the columns for a given view.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="view"></param>
        /// <returns></returns>
        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            return new ViewColumnSchema[0];
        }

        /// <summary>
        /// Gets the data from a given view.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="view"></param>
        /// <returns></returns>
        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            return null;
        }

        #endregion

        #region Command Retrieval Methods

        /// <summary>
        /// Gets all commands for the given database.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            return new CommandSchema[0];
        }

        /// <summary>
        /// Gets the parameters for a given command.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="command"></param>
        /// <returns></returns>
        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets schema information about the results of a given command.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="command"></param>
        /// <returns></returns>
        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets the definition for a given command.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <param name="command"></param>
        /// <returns></returns>
        public string GetCommandText(string connectionString, CommandSchema command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region Misc. Methods

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <param name="connectionString">The connection string used to connect to the target database.</param>
        /// <returns>The name of the database</returns>
        public string GetDatabaseName(string connectionString)
        {            
            SqlCeConnection conn = GetConnection(connectionString);
            string path = conn.Database;
            path = Path.GetFileNameWithoutExtension(path);
            return path;
        }

        /// <summary>
        /// Gets the connection for a given connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns></returns>
        private static SqlCeConnection GetConnection(string connectionString)
        {
            var connection = new SqlCeConnection( connectionString );
            connection.Open();

            return connection;
        }

        /// <summary>
        /// Gets the ce command.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        private static SqlCeCommand GetCeCommand(string connectionString, string commandText)
        {
            return new SqlCeCommand(commandText, GetConnection(connectionString));
        }

        /// <summary>
        /// Gets the boolean value from a from yes/no input (ce uses them in the schema views)
        /// </summary>
        /// <param name="yesNo">The yes no.</param>
        /// <returns></returns>
        private static bool GetBoolFromYesNo(string yesNo)
        {
            if (string.IsNullOrEmpty(yesNo))
            {
                return false;
            }

            return yesNo.ToUpper() == "YES";
        }

        /// <summary>
        /// Gets the db type from string.
        /// </summary>
        /// <param name="dataType">Type of the ce data.</param>
        /// <returns></returns>
        private static DbType GetDbTypeFromString(string dataType)
        {
            switch (dataType)
            {
                case "bigint":
                    return DbType.Int64;
                case "binary":
                    return DbType.Binary;
                case "bit":
                    return DbType.Boolean;
                case "datetime":
                    return DbType.DateTime;
                case "float":
                    return DbType.Double;
                case "image":
                    return DbType.Binary;
                case "int":
                    return DbType.Int32;
                case "money":
                    return DbType.Currency;
                case "nchar":
                    return DbType.StringFixedLength;
                case "ntext":
                    return DbType.String;
                case "numeric":
                    return DbType.Decimal;
                case "nvarchar":
                    return DbType.String;
                case "real":
                    return DbType.Single;
                case "timestamp":
                    return DbType.Binary;
                case "smallint":
                    return DbType.Int16;
                case "tinyint":
                    return DbType.Byte;
                case "uniqueidentifier":
                    return DbType.Guid;
                case "varbinary":
                    return DbType.Binary;
                default:
                    throw new ArgumentOutOfRangeException("Unexpected input value: " + dataType);
            }
        }

        #endregion
    }
}