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
using System.Text.RegularExpressions;

using Npgsql;
using NpgsqlTypes;

namespace SchemaExplorer
{
    public class PostgreSQLSchemaProvider : IDbSchemaProvider, IDbConnectionStringEditor
    {
        #region Properties

        #region IDbSchemaProvider Members

        /// <summary>
        /// Return the Constant name of the SchemaProvider.
        /// </summary>
        public string Name
        {
            get { return "PostgreSQLSchemaProvider"; }
        }

        /// <summary>
        /// Returns the name of the SchemaProvider.
        /// </summary>
        public string Description
        {
            get { return "PostgreSQL Schema Provider"; }
        }

        #endregion

        #region IDbConnectionStringEditor Members

        /// <summary>
        /// Shows the Connection Editor.
        /// </summary>
        /// <param name="currentConnectionString"></param>
        /// <returns></returns>
        public bool ShowEditor(string currentConnectionString)
        {
            return false;
        }

        /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Connection Editor
        /// </summary>
        public bool EditorAvailable
        {
            get { return false; }
        }

        #endregion

        #endregion

        #region Table Retrieval Methods

        #region public TableSchema[] GetTables( string connectionString, DatabaseSchema database )

        /// <summary>
        /// Get the Array of tables...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            List<TableSchema> tables = new List<TableSchema>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                const string sql = "select tablename, tableowner from pg_catalog.pg_tables where schemaname = 'public' order by tablename";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            // Exclude the extended properties table if it is encountered
                            if (reader.GetString(0).ToUpper() != "CODESMITH_EXTENDED_PROPERTIES")
                            {
                                tables.Add(new TableSchema(database, reader.GetString(0), reader.GetString(1), DateTime.MinValue));
                            }
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return tables.ToArray();
        }

        #endregion

        #region public IndexSchema[] GetTableIndexes( string connectionString, TableSchema table )

        /// <summary>
        /// Get all the Indexes for a given Table
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            List<IndexSchema> indexSchemas = new List<IndexSchema>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                string sql = string.Format("select * from pg_catalog.pg_indexes where schemaname='public' and tablename = '{0}'", table.Name);
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            string indexName = reader.GetString(2);

                            string sql2 = string.Format("SELECT n.nspname AS schemaname, c.relname AS tablename, i.relname AS indexname, t.spcname AS \"tablespace\", a.attname as \"colname\", x.indisunique as \"unique\", x.indisprimary as \"primary\", x.indisclustered as \"clustered\" FROM pg_catalog.pg_index x JOIN pg_catalog.pg_class c ON c.oid = x.indrelid JOIN pg_catalog.pg_class i ON i.oid = x.indexrelid JOIN pg_catalog.pg_attribute a ON a.attrelid = i.relfilenode LEFT JOIN pg_catalog.pg_namespace n ON n.oid = c.relnamespace LEFT JOIN pg_catalog.pg_tablespace t ON t.oid = i.reltablespace WHERE c.relkind = 'r'::\"char\" AND i.relkind = 'i'::\"char\" AND n.nspname='public' AND c.relname='{0}' AND i.relname= '{1}'", table.Name, indexName);
                            using (var command2 = new NpgsqlCommand(sql2, connection))
                            {
                                using (NpgsqlDataReader reader2 = command2.ExecuteReader())
                                {
                                    List<string> memberColumns = new List<string>();

                                    bool isPrimary = false;
                                    bool isUnique = false;
                                    bool isClustered = false;

                                    while (reader2.Read())
                                    {
                                        isPrimary = !reader2.IsDBNull(6) && reader2.GetBoolean(6);
                                        isUnique = !reader2.IsDBNull(5) && reader2.GetBoolean(5);
                                        isClustered = !reader2.IsDBNull(7) && reader2.GetBoolean(7);

                                        memberColumns.Add(reader2.IsDBNull(4) ? string.Empty : reader2.GetString(4));
                                    }

                                    indexSchemas.Add(new IndexSchema(table, indexName, isPrimary, isUnique, isClustered, memberColumns.ToArray()));

                                    if (!reader2.IsClosed)
                                    {
                                        reader2.Close();
                                    }
                                }
                            }
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            // Return the array of indexes
            return indexSchemas.ToArray();
        }

        #endregion

        #region public ColumnSchema[] GetTableColumns( string connectionString, TableSchema table )

        /// <summary>
        /// Return the Columns for a given Table.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            var columns = new List<ColumnSchema>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                string sql = string.Format("select column_name, is_nullable, character_maximum_length, numeric_precision, numeric_scale, data_type, udt_name from information_schema.columns where table_schema = 'public' and table_name='{0}'", table.Name);
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            bool allowDBNull = reader.IsDBNull(1) || reader.GetString(1) == "YES";
                            byte precision = (byte)(reader.IsDBNull(3) ? byte.MinValue : reader.GetInt32(3));
                            int size = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                            int scale = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                            string name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            string nativeType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            string dbtype = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

                            columns.Add(new ColumnSchema(table, name, GetDbType(dbtype), nativeType, size, precision, scale, allowDBNull, new[] { new ExtendedProperty("NpgsqlDbType", GetNativeDbType(nativeType), DbType.String) }));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return columns.ToArray();
        }

        #endregion

        #region public TableKeySchema[] GetTableKeys( string connectionString, TableSchema table )

        /// <summary>
        /// Return the Foreign key info for a given table...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            var tableKeys = new List<TableKeySchema>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                #region Get Foreign Keys

                string sql = string.Format("SELECT constraint_name as constrname FROM information_schema.table_constraints WHERE table_name = '{0}' AND constraint_type = 'FOREIGN KEY' AND constraint_schema='public'", table.Name);
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    var sql2 = string.Format("SELECT px.conname as constrname, att.attname as colname, fore.relname as reftabname, fatt.attname as refcolname, CASE px.confupdtype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS on_update, CASE px.confdeltype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS on_delete, CASE px.contype WHEN 'p' THEN true WHEN 'f' THEN false END as IsPrimaryKey from pg_constraint px left join pg_class home on (home.oid = px.conrelid) left join pg_class fore on (fore.oid = px.confrelid) left join pg_attribute att on (att.attrelid = px.conrelid AND att.attnum = ANY(px.conkey)) left join pg_attribute fatt on (fatt.attrelid = px.confrelid AND fatt.attnum = ANY(px.confkey)) where (home.relname = '{0}') and px.contype = 'f' order by constrname", table.Name);
                    using (var cmd = new NpgsqlCommand(sql2, connection))
                    {
                        var adapter = new NpgsqlDataAdapter();
                        var ds = new DataSet();
                        adapter.SelectCommand = command;
                        adapter.Fill(ds, "constraint");
                        adapter.SelectCommand = cmd;
                        adapter.Fill(ds, "keys");

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            // Add constraint to keys relationship
                            ds.Relations.Add("Contraint_to_Keys", ds.Tables[0].Columns["constrname"], ds.Tables[1].Columns["constrname"]);

                            foreach (DataRow constraintRow in ds.Tables[0].Rows)
                            {
                                string name = constraintRow["constrname"].ToString();

                                // Get the keys
                                DataRow[] keys = constraintRow.GetChildRows("Contraint_to_Keys");

                                var primaryKeys = new string[keys.Length];
                                var foreignKeys = new string[keys.Length];
                                string fkTable = table.Name;
                                string pkTable = keys[0]["reftabname"].ToString();

                                for (int i = 0; i < keys.Length; i++)
                                {
                                    foreignKeys[i] = keys[i]["colname"].ToString();
                                    primaryKeys[i] = keys[i]["refcolname"].ToString();
                                }

                                tableKeys.Add(new TableKeySchema(table.Database, name, foreignKeys, fkTable, primaryKeys, pkTable));
                            }
                        }
                    }
                }

                #endregion

                #region Get Reference Keys

                string sql3 = string.Format("SELECT px.conname as constrname FROM pg_constraint px left join pg_class fore on fore.oid = px.confrelid where fore.relname = '{0}'", table.Name);
                using (var command = new NpgsqlCommand(sql3, connection))
                {
                    var sql2 = string.Format("SELECT px.conname as constrname, fatt.attname as colname, home.relname as reftabname, att.attname as refcolname, CASE px.confupdtype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS on_update, CASE px.confdeltype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS on_delete, CASE px.contype WHEN 'p' THEN true WHEN 'f' THEN false END as IsPrimaryKey from pg_constraint px left join pg_class home on (home.oid = px.conrelid) left join pg_class fore on (fore.oid = px.confrelid) left join pg_attribute att on (att.attrelid = px.conrelid AND att.attnum = ANY(px.conkey)) left join pg_attribute fatt on (fatt.attrelid = px.confrelid AND fatt.attnum = ANY(px.confkey)) where (fore.relname = '{0}') order by constrname", table.Name);
                    using (var cmd = new NpgsqlCommand(sql2, connection))
                    {
                        var adapter = new NpgsqlDataAdapter();
                        var ds = new DataSet();
                        adapter.SelectCommand = command;
                        adapter.Fill(ds, "constraint");
                        adapter.SelectCommand = cmd;
                        adapter.Fill(ds, "keys");

                        if (ds.Tables[0].Rows.Count > 0)
                        {
                            // Add constraint to keys relationship
                            ds.Relations.Add("Contraint_to_Keys", ds.Tables[0].Columns["constrname"], ds.Tables[1].Columns["constrname"]);

                            foreach (DataRow constraintRow in ds.Tables[0].Rows)
                            {
                                string name = constraintRow["constrname"].ToString();

                                // Get the keys
                                DataRow[] keys = constraintRow.GetChildRows("Contraint_to_Keys");

                                var primaryKeys = new string[keys.Length];
                                var foreignKeys = new string[keys.Length];
                                string fkTable = keys[0]["reftabname"].ToString();
                                string pkTable = table.Name;

                                for (int i = 0; i < keys.Length; i++)
                                {
                                    foreignKeys[i] = keys[i]["refcolname"].ToString();
                                    primaryKeys[i] = keys[i]["colname"].ToString();
                                }

                                tableKeys.Add(new TableKeySchema(table.Database, name, foreignKeys, fkTable, primaryKeys, pkTable));
                            }
                        }
                    }
                }

                #endregion
            }

            return tableKeys.ToArray();
        }

        #endregion

        #region public PrimaryKeySchema GetTablePrimaryKey( string connectionString, TableSchema table )

        /// <summary>
        /// Return the PK for a given Table...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            PrimaryKeySchema primaryKeySchema = null;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                string sql = string.Format("select * from information_schema.table_constraints where constraint_schema='public' and table_name='{0}' and constraint_type='PRIMARY KEY'", table.Name);
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // NOTE: This should return only primary key column constraints and not fk constraints.
                            string sqlKey = string.Format("select px.conname as ConstraintName, att.attname as ColumnName from pg_constraint px inner join pg_class home on (home.oid = px.conrelid) left join pg_attribute att on (att.attrelid = px.conrelid AND att.attnum = ANY(px.conkey)) where (home.relname = '{0}') and px.contype = 'p'", table.Name);
                            using (var command2 = new NpgsqlCommand(sqlKey, connection))
                            {
                                using (NpgsqlDataReader reader2 = command2.ExecuteReader())
                                {
                                    var memberColumns = new List<string>();

                                    while (reader2.Read())
                                    {
                                        memberColumns.Add(reader2.IsDBNull(1) ? string.Empty : reader2.GetString(1));
                                    }

                                    primaryKeySchema = new PrimaryKeySchema(table, reader.GetString(0), memberColumns.ToArray());
                                }
                            }
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return primaryKeySchema;
        }

        #endregion

        #region public DataTable GetTableData( string connectionString, TableSchema table )

        /// <summary>
        /// Return the data from a table in a System.DataTable object...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            DataTable dataTable;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Create and fill the data table
                dataTable = new DataTable(table.Name);

                string sql = string.Format("SELECT * FROM {0}", table.Name);

                using (var npgsqlDataAdapter = new NpgsqlDataAdapter(sql, connection))
                {
                    npgsqlDataAdapter.Fill(dataTable);
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return dataTable;
        }

        #endregion

        #endregion

        #region Extended Properties

        /// <summary>
        /// Returns an array of extended properties
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="schemaObject"></param>
        /// <returns></returns>
        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            var extendedProperties = new List<ExtendedProperty>();

            if (schemaObject is ColumnSchema)
            {
                var columnSchema = schemaObject as ColumnSchema;

                string commandText = string.Format(
                        @"select pg_get_serial_sequence(c.table_name, c.column_name) as EXTRA, COLUMN_DEFAULT, data_type 
                          from pg_tables t
                          INNER JOIN information_schema.columns c on t.tablename = c.table_name
                          WHERE schemaname = '{0}' 
                          AND table_name = '{1}'
                          AND COLUMN_NAME = '{2}'
                          order by ordinal_position",
                        columnSchema.Table.Database.Name, columnSchema.Table.Name, columnSchema.Name);

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        using (IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (reader.Read())
                            {
                                string sequence = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).ToLower();
                                string defaultValue = reader.IsDBNull(1) ? null : reader.GetString(1).ToUpper();

                                string columntype = reader.GetString(2).ToUpper();
                                bool isIdentity = !string.IsNullOrEmpty(sequence);

                                extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IsIdentity, isIdentity, columnSchema.DataType));

                                if (isIdentity)
                                {
                                    extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IdentitySeed, 1, columnSchema.DataType));
                                    extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IdentityIncrement, 1, columnSchema.DataType));
                                }

                                extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, defaultValue, DbType.String));
                                extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.SystemType, columntype, DbType.String));
                                extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.Sequence, sequence.ToUpper(), DbType.String));
                            }

                            if (!reader.IsClosed)
                                reader.Close();
                        }
                    }

                    if (connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            }

            return extendedProperties.ToArray();
        }

        /// <summary>
        /// Sets an extended property
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="schemaObject"></param>
        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region View Retrieval Methods

        #region public ViewColumnSchema[] GetViewColumns( string connectionString, ViewSchema view )

        /// <summary>
        /// Return all the columns for a view...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            var viewColumns = new List<ViewColumnSchema>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                string sql = String.Format("SELECT column_name, is_nullable, character_maximum_length, numeric_precision, numeric_scale, data_type, udt_name FROM information_schema.columns WHERE table_schema='public' AND table_name='{0}' ORDER BY ordinal_position", view.Name);
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            bool allowDbNull = reader.IsDBNull(1) || reader.GetString(1) == "YES";

                            int size = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                            byte precision = (byte)(reader.IsDBNull(3) ? byte.MinValue : reader.GetInt32(3));
                            int scale = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);

                            string nativeType = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                            string dbtype = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

                            viewColumns.Add(new ViewColumnSchema(view, reader.GetString(0), GetDbType(dbtype), nativeType, size, precision, scale, allowDbNull, new[] { new ExtendedProperty("NpgsqlDbType", GetNativeDbType(nativeType), DbType.String) }));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return viewColumns.ToArray();
        }

        #endregion

        #region public DataTable GetViewData( string connectionString, ViewSchema view )

        /// <summary>
        /// Return all the Rows from a view
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            DataTable dataTable;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Create and fill the data table
                dataTable = new DataTable(view.Name);

                string sql = string.Format("SELECT * FROM {0}", view.Name);

                using (var dataAdapter = new NpgsqlDataAdapter(sql, connection))
                {
                    dataAdapter.Fill(dataTable);
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return dataTable;
        }

        #endregion

        #region public ViewSchema[] GetViews( string connectionString, DatabaseSchema database )

        /// <summary>
        /// Return array of Views
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            List<ViewSchema> views = new List<ViewSchema>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                const string sql = "select viewname, viewowner from pg_catalog.pg_views where schemaname = 'public' order by viewname;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            // Exclude the extended properties table if it is encountered
                            if (reader.GetString(0).ToUpper() != "CODESMITH_EXTENDED_PROPERTIES")
                            {
                                views.Add(new ViewSchema(database, reader.GetString(0), reader.GetString(1), DateTime.MinValue));
                            }
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return views.ToArray();
        }

        #endregion

        #region public string GetViewText( string connectionString, ViewSchema view )

        /// <summary>
        /// Returns the Text of a View's definition
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public string GetViewText(string connectionString, ViewSchema view)
        {
            string viewText;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                // Get SQL to retrieve source for the view
                string sql = string.Format("select view_definition from information_schema.views where table_schema='public' and table_name = '{0}'", view.Name);

                // Get the view text
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    viewText = (string)command.ExecuteScalar();
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return viewText;
        }

        #endregion

        #endregion

        #region Commnad Retrieval Methods

        #region public CommandSchema[] GetCommands( string connectionString, DatabaseSchema database )

        /// <summary>
        /// Return array of commands...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            var commands = new List<CommandSchema>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                const string sql = "SELECT routine_name, rolname, specific_name, data_type from information_schema.routines LEFT JOIN pg_catalog.pg_proc p ON p.proname = routine_name INNER JOIN pg_catalog.pg_namespace n ON n.oid = p.pronamespace INNER JOIN pg_catalog.pg_authid a on a.oid = proowner WHERE routine_schema='public' ORDER BY routine_name ";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            bool isFunction = !reader.IsDBNull(3) && reader.GetString(3).Trim().Equals("VOID", StringComparison.InvariantCultureIgnoreCase);
                            if (isFunction && !database.IncludeFunctions)
                                continue;

                            var extendedProperties = new List<ExtendedProperty>
                                                         {
                                                             new ExtendedProperty("CS_Name", reader.GetString(2), DbType.String, PropertyStateEnum.ReadOnly),
                                                             new ExtendedProperty(ExtendedPropertyNames.IsScalarFunction, isFunction, DbType.Boolean, PropertyStateEnum.ReadOnly),
                                                             new ExtendedProperty("CS_IsProcedure", isFunction, DbType.Boolean, PropertyStateEnum.ReadOnly), // Added for backwards compatibility.
                                                             new ExtendedProperty(ExtendedPropertyNames.IsTrigger, reader.GetString(3).Equals("TRIGGER", StringComparison.InvariantCultureIgnoreCase), DbType.Boolean, PropertyStateEnum.ReadOnly)
                                                         };

                            commands.Add(new CommandSchema(database, reader.GetString(0), reader.GetString(1), DateTime.MinValue, extendedProperties.ToArray()));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return commands.ToArray();
        }

        #endregion

        #region public ParameterSchema[] GetCommandParameters( string connectionString, CommandSchema command )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandSchema"></param>
        /// <returns></returns>
        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema commandSchema)
        {
            string specificName = commandSchema.ExtendedProperties["CS_Name"].Value as string;
            List<ParameterSchema> parameters = new List<ParameterSchema>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                string sql = string.Format("select * from information_schema.parameters where specific_schema='public' and specific_name = '{0}' order by ordinal_position", specificName);
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            string parameterName = reader.IsDBNull(7) ? string.Empty : reader.GetString(7);
                            int size = reader.IsDBNull(9) ? 0 : reader.GetInt32(9);
                            int scale = reader.IsDBNull(19) ? 0 : reader.GetInt32(19);
                            byte precision = reader.IsDBNull(17) ? (byte)0 : reader.GetByte(17);
                            string nativeType = reader.GetString(8);

                            parameters.Add(
                                new ParameterSchema(
                                    commandSchema,
                                    parameterName,
                                    GetParameterDirection(reader.GetString(4)),
                                    GetDbType(reader.GetString(8)),
                                    nativeType,
                                    size,
                                    precision,
                                    scale,
                                    false,
                                    new[] { new ExtendedProperty("NpgsqlDbType", GetNativeDbType(nativeType), DbType.String) }));
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return parameters.ToArray();
        }

        #endregion

        #region public CommandResultSchema[] GetCommandResultSchemas( string connectionString, CommandSchema command )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            CommandResultSchema[] resultSchema = null;
            var specificName = command.ExtendedProperties["CS_Name"].Value as string;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                string sql = string.Format("select data_type from information_schema.routines where specific_schema='public' and specific_name = '{0}'", specificName);
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            string dataType = reader.GetString(0);
                            if (dataType == "void")
                            {
                                resultSchema = new CommandResultSchema[0];
                            }
                            else if (dataType == "USER-DEFINED")
                            {
                                var typeInfoCommand = new NpgsqlCommand(string.Format("SELECT t.typname, attname, a.typname from pg_type t JOIN pg_class on (reltype = t.oid) JOIN pg_attribute on (attrelid = pg_class.oid) JOIN pg_type a on (atttypid = a.oid) WHERE t.typname = (SELECT t.typname FROM pg_catalog.pg_proc p LEFT JOIN pg_catalog.pg_namespace n ON n.oid = p.pronamespace INNER JOIN pg_type t ON p.prorettype = t.oid WHERE n.nspname = 'public' and proname = '{0}' ORDER BY proname);", command.Name), connection);
                                using (NpgsqlDataReader typeReader = typeInfoCommand.ExecuteReader(CommandBehavior.CloseConnection))
                                {
                                    string typeName = null;
                                    var colSchema = new List<CommandResultColumnSchema>();
                                    while (typeReader.Read())
                                    {
                                        if (string.IsNullOrEmpty(typeName))
                                            typeName = typeReader.GetString(0);
                                        string nativeType = typeReader.GetString(2);
                                        colSchema.Add(new CommandResultColumnSchema(command, typeReader.GetString(1), GetDbType(nativeType), nativeType, 0, 0, 0, true, new[] { new ExtendedProperty("NpgsqlDbType", GetNativeDbType(nativeType), DbType.String) }));
                                    }
                                    resultSchema = new[] { new CommandResultSchema(command, typeName, colSchema.ToArray()) };
                                }
                            }
                            //else if (dataType == "record")
                            //{
                            //    string path = Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"),"CodeSmith");
                            //    path = Path.Combine(path, command.Database.Name + ".xml");
                            //    if (File.Exists(path))
                            //    {
                            //        var dbInfo = Utilities.DeserialiseFromFile<DbInfo>(path);
                            //        var colSchema =
                            //            from commandInfo in
                            //                dbInfo.Commands.Where(cmdTmp => cmdTmp.Id == (string) command.ExtendedProperties["CS_Name"].Value)
                            //            from columnInfo in commandInfo.Columns
                            //            select
                            //                new CommandResultColumnSchema(command, columnInfo.Name, columnInfo.DataType, "UNKNOWN", columnInfo.Size,
                            //                                              columnInfo.Precision, columnInfo.Scale, columnInfo.AllowDbNull);
                            //        resultSchema = new[] { new CommandResultSchema(command, command.Name + "records", colSchema.ToArray()) };

                            //    }
                            //    else
                            //    {
                            //        Debug.WriteLine("Unable to find " + path);


                            //        var dummySchema = new CommandResultSchema(command, "record-SAMPLE", new[]
                            //                                                                                {
                            //                                                                                    new CommandResultColumnSchema(command,
                            //                                                                                                                  "record-SAMPLE",
                            //                                                                                                                  DbType.
                            //                                                                                                                    String,
                            //                                                                                                                  "record-SAMPLE",
                            //                                                                                                                  0, 0, 0,
                            //                                                                                                                  false)
                            //                                                                                });

                            //        resultSchema = new[] { dummySchema };
                            //    }
                            //}
                            //else if (dataType == "trigger")
                            //{
                            //    var dummySchema = new CommandResultSchema(command, "trigger-SAMPLE", new[]
                            //                                                                            {
                            //                                                                                new CommandResultColumnSchema(command,
                            //                                                                                                              "trigger-SAMPLE",
                            //                                                                                                              DbType.
                            //                                                                                                                String,
                            //                                                                                                              "trigger-SAMPLE",
                            //                                                                                                              0, 0, 0,
                            //                                                                                                              false)
                            //                                                                            });

                            //    resultSchema = new[] { dummySchema };
                            //}
                            //else
                            //{
                            //    var schema = new CommandResultSchema(command, "simple",
                            //                                         new[]
                            //                                            {
                            //                                                new CommandResultColumnSchema(command, "result", GetDbType(dataType),
                            //                                                                              dataType, 0, 0, 0, false)
                            //                                            });
                            //    resultSchema = new[] { schema };
                            //}
                            //break;
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return resultSchema ?? new CommandResultSchema[0];
        }

        #endregion

        #region public string GetCommandText( string connectionString, CommandSchema command )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="commandSchema"></param>
        /// <returns></returns>
        public string GetCommandText(string connectionString, CommandSchema commandSchema)
        {
            string commandText = string.Empty;
            string specificName = commandSchema.ExtendedProperties["CS_Name"].Value as string;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                string sql = string.Format("select routine_definition from information_schema.routines where specific_schema='public' and specific_name = '{0}'", specificName);
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            commandText = reader.GetString(0);
                            break;
                        }

                        if (!reader.IsClosed)
                        {
                            reader.Close();
                        }
                    }
                }

                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            return commandText;
        }

        #endregion

        #endregion

        #region Misc. Methods

        #region public string GetDatabaseName(string connectionString)

        /// <summary>
        /// Returns the DB name 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public string GetDatabaseName(string connectionString)
        {
            Regex databaseNameRegex = new Regex(@"Database\W*=\W*(?<database>[^;]*)", RegexOptions.IgnoreCase);

            if (databaseNameRegex.IsMatch(connectionString))
            {
                return databaseNameRegex.Match(connectionString).Groups["database"].ToString();
            }

            return connectionString;
        }

        #endregion

        #region private static DbType GetDbType(string type)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static DbType GetDbType(string type)
        {
            switch (type)
            {
                case "bit":
                case "bool":
                case "boolean":
                    return DbType.Boolean;
                case "bytea":
                    return DbType.Binary;
                case "bpchar":
                case "char":
                case "character":
                case "text":
                case "varchar":
                case "character varying":
                    return DbType.String;
                case "date":
                    return DbType.Date;
                case "float4":
                case "single precision":
                case "real":
                    return DbType.Single;
                case "float8":
                case "double precision":
                    return DbType.Double;
                case "int2":
                case "smallint":
                    return DbType.Int16;
                case "int4":
                case "integer":
                    return DbType.Int32;
                case "int8":
                case "bigint":
                    return DbType.Int64;
                case "money":
                case "numeric":
                    return DbType.Decimal;
                case "time":
                case "timetz":
                case "time without time zone":
                case "time without timezone":
                case "time with time zone":
                case "time with timezone":
                    return DbType.Time;
                case "interval":
                case "timestamp":
                case "timestamptz":
                case "timestamp without time zone":
                case "timestamp without timezone":
                case "timestamp with time zone":
                case "timestamp with timezone":
                    return DbType.DateTime;
                case "uuid":
                    return DbType.Guid;
                case "box":
                case "circle":
                case "inet":
                case "line":
                case "lseg":
                case "path":
                case "point":
                case "polygon":
                case "refcursor":
                    return DbType.Object;
                default:
                    return DbType.Object;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static NpgsqlDbType GetNativeDbType(string type)
        {
            switch (type.ToLower())
            {
                case "array":
                    return NpgsqlDbType.Array;
                case "bit":
                    return NpgsqlDbType.Bit;
                case "box":
                    return NpgsqlDbType.Box;
                case "bool":
                case "boolean":
                    return NpgsqlDbType.Boolean;
                case "bytea":
                    return NpgsqlDbType.Bytea;
                case "char":
                    return NpgsqlDbType.Char;
                case "bpchar":
                case "character":
                case "varchar":
                case "character varying":
                    return NpgsqlDbType.Varchar;
                case "date":
                    return NpgsqlDbType.Date;
                case "float4":
                case "single precision":
                case "real":
                    return NpgsqlDbType.Real;
                case "float8":
                case "double precision":
                case "double":
                    return NpgsqlDbType.Double;
                case "int2":
                case "smallint":
                    return NpgsqlDbType.Smallint;
                case "int4":
                case "integer":
                    return NpgsqlDbType.Integer;
                case "int8":
                case "bigint":
                    return NpgsqlDbType.Bigint;
                case "money":
                    return NpgsqlDbType.Money;
                case "name":
                    return NpgsqlDbType.Name;
                case "numeric":
                    return NpgsqlDbType.Numeric;
                case "text":
                case "user-defined":
                    return NpgsqlDbType.Text;
                case "oidvector":
                    return NpgsqlDbType.Oidvector;
                case "abstime":
                    return NpgsqlDbType.Abstime;

                case "time":
                case "time without time zone":
                case "time without timezone":
                    return NpgsqlDbType.Time;
                case "timetz":
                case "time with time zone":
                case "time with timezone":
                    return NpgsqlDbType.TimeTZ;

                case "interval":
                    return NpgsqlDbType.Interval;
                case "timestamp":
                case "timestamptz":
                case "timestamp without time zone":
                case "timestamp without timezone":
                    return NpgsqlDbType.Timestamp;
                case "timestamp with time zone":
                case "timestamp with timezone":
                    return NpgsqlDbType.TimestampTZ;
                case "uuid":
                    return NpgsqlDbType.Uuid;
                case "circle":
                    return NpgsqlDbType.Circle;
                case "inet":
                    return NpgsqlDbType.Inet;
                case "line":
                    return NpgsqlDbType.Line;
                case "lseg":
                    return NpgsqlDbType.LSeg;
                case "path":
                    return NpgsqlDbType.Path;
                case "point":
                    return NpgsqlDbType.Point;
                case "polygon":
                    return NpgsqlDbType.Polygon;
                case "refcursor":
                    return NpgsqlDbType.Refcursor;
                case "xml":
                    return NpgsqlDbType.Xml;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region private static ParameterDirection GetParameterDirection(string direction)

        /// <summary>
        /// 
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static ParameterDirection GetParameterDirection(string direction)
        {
            switch (direction)
            {
                case "IN":
                    return ParameterDirection.Input;
                case "OUT":
                    return ParameterDirection.Output;
            }

            return ParameterDirection.InputOutput;
        }

        #endregion

        #endregion
    }
}