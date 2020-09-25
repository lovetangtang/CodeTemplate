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
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SchemaExplorer.Designer;

namespace SchemaExplorer
{
    public class SQLiteSchemaProvider : IDbSchemaProvider, IDbConnectionStringEditor
    {
        //private const string CatalogNameColumn = "NAME";
        private const string CatalogPathColumn = "DESCRIPTION";
        private const string Catalogs = "Catalogs";
        private const string Columns = "Columns";
        private const string ColumnsNameColumn = "COLUMN_NAME";
        private const string ColumnsNullableColumn = "IS_NULLABLE";
        private const string ColumnsTypeColumn = "DATA_TYPE";
        private const string ForeignKeys = "ForeignKeys";
        private const string ForeignKeysFromColColumn = "FKEY_FROM_COLUMN";
        private const string ForeignKeysFromTableColumn = "TABLE_NAME";
        private const string ForeignKeysNameColumn = "CONSTRAINT_NAME";
        private const string ForeignKeysToColColumn = "FKEY_TO_COLUMN";
        private const string ForeignKeysToTableColumn = "FKEY_TO_TABLE";
        private const string IndexColumns = "IndexColumns";
        private const string IndexColumnsNameColumn = "COLUMN_NAME";
        private const string Indexes = "Indexes";
        private const string IndexesIsClusteredColumn = "CLUSTERED";
        private const string IndexesIsPkeyColumn = "PRIMARY_KEY";
        private const string IndexesIsUniqueColumn = "UNIQUE";
        private const string IndexesNameColumn = "INDEX_NAME";
        private const string MainCatalog = "main";
        private const string TableNameColumn = "TABLE_NAME";
        private const string Tables = "Tables";
        private const string TableType = "table";
        //private const string TableTypeColumn = "TABLE_TYPE";
        private const string Views = "Views";
        private const string ViewsTextColumn = "VIEW_DEFINITION";
        private const string ViewType = "view";
        private const string ColumnsSize = "CHARACTER_MAXIMUM_LENGTH";
        private const string ColumnsPrecision = "NUMERIC_PRECISION";
        private const string ColumnsScale = "NUMERIC_SCALE";

        #region IDbSchemaProvider Members

        public string Name
        {
            get { return "SQLiteSchemaProvider"; }
        }

        public string Description
        {
            get { return "SQLite 3 Schema Provider"; }
        }

        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            return new ParameterSchema[0];
        }

        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            return new CommandResultSchema[0];
        }

        public string GetCommandText(string connectionString, CommandSchema command)
        {
            return string.Empty;
        }

        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            //SQLite doesn't support stored commands eg stored procs
            return new CommandSchema[0];
        }

        public string GetDatabaseName(string connectionString)
        {
            return Path.GetFileNameWithoutExtension(GetSchemaDataColumn(connectionString, Catalogs, CatalogPathColumn, MainCatalog));
        }

        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            //No extended properties in SQLite            
            return new ExtendedProperty[0];
        }

        /// <summary>
        /// Sets the extended properties.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="schemaObject">The schema object.</param>
        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            throw new NotImplementedException();
        }

        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            DataTable dt = GetSchemaData(connectionString,
                                         Columns,
                                         null /*restrictions[0] - catalog*/,
                                         null /*restrictions[1] - unused*/,
                                         table.Name /*restrictions[2] - table*/,
                                         null /*restrictions[3] - column*/);

            var extendedProperties = new List<ExtendedProperty>();
            var columns = new ColumnSchema[dt.Rows.Count];
            int columnIndex = 0;

            foreach (DataRow dr in dt.Rows)
            {
                string name = (string)dr[ColumnsNameColumn];
                string nativeType = dr.IsNull(ColumnsTypeColumn) ? "text" : (string)dr[ColumnsTypeColumn];
                DbType dbType = DbTypeFromType(connectionString, nativeType);
                bool allowDBNull = dr.IsNull(ColumnsNullableColumn) || (bool)dr[ColumnsNullableColumn];
                int size = dr.IsNull(ColumnsSize) ? 0 : (int)dr[ColumnsSize];
                int precision = dr.IsNull(ColumnsPrecision) ? 0 : (int)dr[ColumnsPrecision];
                int scale = dr.IsNull(ColumnsScale) ? 0 : (int)dr[ColumnsScale];

                int ordinalPosition = dr.IsNull("ORDINAL_POSITION") ? 0 : (int)dr["ORDINAL_POSITION"];
                string edmType = dr.IsNull("EDM_TYPE") ? "String" : (string)dr["EDM_TYPE"];
                bool isAutoIncrement = !dr.IsNull("AUTOINCREMENT") && (bool)dr["AUTOINCREMENT"];
                bool isUnique = !dr.IsNull("UNIQUE") && (bool)dr["UNIQUE"];
                bool hasDefault = !dr.IsNull("COLUMN_HASDEFAULT") && (bool)dr["COLUMN_HASDEFAULT"];
                string columnDefault = dr.IsNull("COLUMN_DEFAULT") ? string.Empty : (string)dr["COLUMN_DEFAULT"];
                string collation = dr.IsNull("COLLATION_NAME") ? string.Empty : (string)dr["COLLATION_NAME"];

                extendedProperties.Clear();
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, ""));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.ObjectID, ordinalPosition));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsIdentity, isAutoIncrement));
                extendedProperties.Add(ExtendedProperty.Readonly("CS_IsUnique", isUnique));  // Added for backwards compatibility.
                extendedProperties.Add(ExtendedProperty.Readonly("CS_HasDefault", hasDefault));  // Added for backwards compatibility.
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.DefaultValue, columnDefault));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Collation, collation));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.SystemType, edmType));
                extendedProperties.Add(ExtendedProperty.Readonly("CS_SQLiteType", edmType)); // Added for backwards compatibility.

                var col = new ColumnSchema(table, name, dbType, nativeType, size, Convert.ToByte(precision), scale, allowDBNull, extendedProperties.ToArray());

                columns[columnIndex++] = col;
            }

            return columns;
        }

        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            DbProviderFactory factory = CreateDbProviderFactory();
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM {0};", table.Name);

                    var adapter = factory.CreateDataAdapter();
                    adapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    adapter.Fill(dataSet);

                    return dataSet.Tables[0];
                }
            }
        }

        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            DataTable dataTable = GetSchemaData(connectionString,
                                         Indexes,
                                         null /*restrictions[0] - catalog*/,
                                         null /*restrictions[1] - unused*/,
                                         table.Name /*restrictions[2] - table*/,
                                         null /*restrictions[3] - unused*/,
                                         null /*restrictions[4] - index*/);

            var indexes = new IndexSchema[dataTable.Rows.Count];
            int indexIndex = 0;

            foreach (DataRow row in dataTable.Rows)
            {
                string name = (string)row[IndexesNameColumn];

                //Get the list of columns in this index
                DataTable cols = GetSchemaData(connectionString,
                                               IndexColumns,
                                               null /*restrictions[0] - catalog*/,
                                               null /*restrictions[1] - unused*/,
                                               table.Name /*restrictions[2] - table*/,
                                               name /*restrictions[3] - index*/,
                                               null /*restrictions[4] - column*/);

                var columns = new string[cols.Rows.Count];
                int columnIndex = 0;
                foreach (DataRow col in cols.Rows)
                    columns[columnIndex++] = (string)col[IndexColumnsNameColumn];


                bool isPrimaryKey = !row.IsNull(IndexesIsPkeyColumn) && (bool)row[IndexesIsPkeyColumn];
                bool isUnique = !row.IsNull(IndexesIsUniqueColumn) && (bool)row[IndexesIsUniqueColumn];
                bool isClustered = !row.IsNull(IndexesIsClusteredColumn) && (bool)row[IndexesIsClusteredColumn];

                indexes[indexIndex++] = new IndexSchema(table, name, isPrimaryKey, isUnique, isClustered, columns);
            }

            return indexes;
        }

        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            // TODO: Verify that this is returning the correct PrimaryKeys.
            //0, 2, 3; catalog, table, key name
            DataTable dataTable = GetSchemaData(connectionString,
                                         ForeignKeys,
                                         null /*restrictions[0] - catalog*/,
                                         null /*restrictions[1] - unused*/,
                                         table.Name /*restrictions[2] - table*/,
                                         null /*restrictions[3] - key name*/);

            var keys = new TableKeySchema[dataTable.Rows.Count];
            int keyIndex = 0;

            foreach (DataRow dr in dataTable.Rows)
            {
                string name = (string)dr[ForeignKeysNameColumn];
                string[] foreignKeyColumns = new[] { (string)dr[ForeignKeysFromColColumn] };
                string foreignKeyTable = (string)dr[ForeignKeysFromTableColumn];
                string[] primaryKeyColumns = new[] { (string)dr[ForeignKeysToColColumn] };
                string primaryKeyTable = (string)dr[ForeignKeysToTableColumn];

                var key = new TableKeySchema(table.Database, name,
                    foreignKeyColumns, foreignKeyTable,
                    primaryKeyColumns, primaryKeyTable);

                keys[keyIndex++] = key;
            }

            return keys;
        }

        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            DataTable dt = GetSchemaData(connectionString,
                                         Indexes,
                                         null /*restrictions[0] - catalog*/,
                                         null /*restrictions[1] - unused*/,
                                         table.Name /*restrictions[2] - table*/,
                                         null /*restrictions[3] - unused*/,
                                         null /*restrictions[4] - index*/);

            //Find the primary key index
            foreach (DataRow dr in dt.Rows)
            {
                bool isPrimaryKey = !dr.IsNull(IndexesIsPkeyColumn) && (bool)dr[IndexesIsPkeyColumn];

                if (isPrimaryKey)
                {
                    //Get the list of columns in this primary key
                    string indexName = (string)dr[IndexesNameColumn];
                    DataTable cols = GetSchemaData(connectionString,
                                                   IndexColumns,
                                                   null /*restrictions[0] - catalog*/,
                                                   null /*restrictions[1] - unused*/,
                                                   table.Name /*restrictions[2] - table*/,
                                                   indexName /*restrictions[3] - index*/,
                                                   null /*restrictions[4] - column*/);

                    var columns = new string[cols.Rows.Count];
                    int colIndex = 0;
                    foreach (DataRow col in cols.Rows)
                        columns[colIndex++] = (string)col[IndexColumnsNameColumn];

                    return new PrimaryKeySchema(table, indexName, columns);
                }
            }

            //No pkey
            return null;
        }

        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            //Get only the 'tables' of type 'table'.  The 'Tables' catlog also includes
            //system tables, and the views

            DataTable dt = GetSchemaData(connectionString,
                                         Tables,
                                         null /*restrictions[0] - catalog*/,
                                         null /*restrictions[1] - unused*/,
                                         null /*restrictions[2] - table*/,
                                         TableType /*restrictions[3] - type*/);

            var extendedProperties = new List<ExtendedProperty>();
            var tables = new TableSchema[dt.Rows.Count];
            int tableIndex = 0;
            foreach (DataRow dr in dt.Rows)
            {
                string name = (string)dr[TableNameColumn];
                long tableID = dr.IsNull("TABLE_ID") ? 0 : (long)dr["TABLE_ID"];
                string tableType = dr.IsNull("TABLE_TYPE") ? "table" : (string)dr["TABLE_TYPE"];
                int rootpage = dr.IsNull("TABLE_ROOTPAGE") ? 0 : (int)dr["TABLE_ROOTPAGE"];

                extendedProperties.Clear();
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, ""));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.ObjectID, tableID));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.SystemType, tableType));
                extendedProperties.Add(ExtendedProperty.Readonly("CS_TableType", tableType)); // Added for backwards compatibility.
                extendedProperties.Add(ExtendedProperty.Readonly("CS_RootPage", rootpage));

                var table = new TableSchema(database, name, string.Empty, DateTime.MinValue,
                    extendedProperties.ToArray());

                tables[tableIndex++] = table;
            }

            return tables;
        }

        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            DataTable dt = GetSchemaData(connectionString,
                                         Columns,
                                         null /*restrictions[0] - catalog*/,
                                         null /*restrictions[1] - unused*/,
                                         view.Name /*restrictions[2] - table*/,
                                         null /*restrictions[3] - column*/);

            var extendedProperties = new List<ExtendedProperty>();
            var columns = new ViewColumnSchema[dt.Rows.Count];
            int columnIndex = 0;

            foreach (DataRow dr in dt.Rows)
            {
                string name = (string)dr[ColumnsNameColumn];
                string nativeType = dr.IsNull(ColumnsTypeColumn) ? "text" : (string)dr[ColumnsTypeColumn];
                DbType dbType = DbTypeFromType(connectionString, nativeType);
                bool allowDBNull = dr.IsNull(ColumnsNullableColumn) || (bool)dr[ColumnsNullableColumn];
                int size = dr.IsNull(ColumnsSize) ? 0 : (int)dr[ColumnsSize];
                int precision = dr.IsNull(ColumnsPrecision) ? 0 : (int)dr[ColumnsPrecision];
                int scale = dr.IsNull(ColumnsScale) ? 0 : (int)dr[ColumnsScale];

                int ordinalPosition = dr.IsNull("ORDINAL_POSITION") ? 0 : (int)dr["ORDINAL_POSITION"];
                string edmType = dr.IsNull("EDM_TYPE") ? "String" : (string)dr["EDM_TYPE"];
                bool isAutoIncrement = !dr.IsNull("AUTOINCREMENT") && (bool)dr["AUTOINCREMENT"];
                bool isUnique = !dr.IsNull("UNIQUE") && (bool)dr["UNIQUE"];
                bool hasDefault = !dr.IsNull("COLUMN_HASDEFAULT") && (bool)dr["COLUMN_HASDEFAULT"];
                string columnDefault = dr.IsNull("COLUMN_DEFAULT") ? string.Empty : (string)dr["COLUMN_DEFAULT"];
                string collation = dr.IsNull("COLLATION_NAME") ? string.Empty : (string)dr["COLLATION_NAME"];

                extendedProperties.Clear();
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, ""));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.ObjectID, ordinalPosition));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsIdentity, isAutoIncrement));
                extendedProperties.Add(ExtendedProperty.Readonly("CS_IsUnique", isUnique));  // Added for backwards compatibility.
                extendedProperties.Add(ExtendedProperty.Readonly("CS_HasDefault", hasDefault));  // Added for backwards compatibility.
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.DefaultValue, columnDefault));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Collation, collation));
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.SystemType, edmType));
                extendedProperties.Add(ExtendedProperty.Readonly("CS_SQLiteType", edmType));  // Added for backwards compatibility.


                var col = new ViewColumnSchema(view, name, dbType, nativeType, size,
                    Convert.ToByte(precision), scale, allowDBNull,
                    extendedProperties.ToArray());

                columns[columnIndex++] = col;
            }

            return columns;
        }

        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            DbProviderFactory factory = CreateDbProviderFactory();
            using (DbConnection connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = string.Format("SELECT * FROM {0};", view.Name);

                    var dataAdapter = factory.CreateDataAdapter();
                    dataAdapter.SelectCommand = command;

                    var dataSet = new DataSet();
                    dataAdapter.Fill(dataSet);

                    return dataSet.Tables[0];
                }
            }
        }

        public string GetViewText(string connectionString, ViewSchema view)
        {
            DataTable dataTable = GetSchemaData(connectionString,
                                         Views,
                                         null /*restrictions[0] - catalog*/,
                                         null /*restrictions[1] - unused*/,
                                         view.Name /*restrictions[2] - view*/);

            if (dataTable.Rows.Count != 1)
                throw new ArgumentException(string.Format("Unexpected number of rows in Views collection for view {0}", view.Name));

            return (string)dataTable.Rows[0][ViewsTextColumn];
        }

        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            //Get only the 'tables' of type 'view'.  The 'Tables' catlog also includes
            //system tables, and regular tables

            DataTable dataTable = GetSchemaData(connectionString,
                                         Views,
                                         null /*restrictions[0] - catalog*/,
                                         null /*restrictions[1] - unused*/,
                                         null /*restrictions[2] - table*/,
                                         ViewType /*restrictions[3] - type*/);

            var extendedProperties = new List<ExtendedProperty>();
            var views = new ViewSchema[dataTable.Rows.Count];
            int viewIndex = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                string name = (string)row[TableNameColumn];
                bool isUpdatable = !row.IsNull("IS_UPDATABLE") && (bool)row["IS_UPDATABLE"];

                extendedProperties.Clear();
                extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, ""));
                extendedProperties.Add(ExtendedProperty.Readonly("CS_IsUpdatable", isUpdatable));

                var view = new ViewSchema(database, name, string.Empty, DateTime.MinValue,
                    extendedProperties.ToArray());

                views[viewIndex++] = view;
            }

            return views;
        }

        #endregion

        private DataTable GetSchemaData(string connectionString, string schemaCollection, params string[] restrictions)
        {
            using (DbConnection connection = CreateConnection(connectionString))
            {
                connection.ConnectionString = connectionString;
                connection.Open();

                return connection.GetSchema(schemaCollection, restrictions);
            }
        }

        private string GetSchemaDataColumn(string connectionString, string schemaCollection, string schemaColumn, params string[] restrictions)
        {
            DataTable dt = GetSchemaData(connectionString,
                                         schemaCollection,
                                         restrictions);

            if (dt.Rows.Count != 1)
                throw new ArgumentException("The specified criteria do not yield a single row in the schema collection");

            return (string)dt.Rows[0][schemaColumn];
        }

        //cache data type table
        private static DataTable DataTypes;

        private DbType DbTypeFromType(string connectionString, string nativeType)
        {
            if (DataTypes == null)
                DataTypes = GetSchemaData(connectionString, "DataTypes");

            //use metadata
            DataRow[] types = DataTypes.Select(string.Format("TypeName = '{0}'", nativeType.ToLowerInvariant()));
            if (types.Length > 0)
                return (DbType)types[0]["ProviderDbType"];

            //use sqlite rules
            if (Regex.IsMatch(nativeType, "int", RegexOptions.IgnoreCase))
                return DbType.Int64;

            if (Regex.IsMatch(nativeType, "real|floa|doub", RegexOptions.IgnoreCase))
                return DbType.Double;

            if (Regex.IsMatch(nativeType, "numeric", RegexOptions.IgnoreCase))
                return DbType.Decimal;

            if (Regex.IsMatch(nativeType, "char|clob|text", RegexOptions.IgnoreCase))
                return DbType.String;

            return DbType.Object;
        }

        #region Implementation of IDbConnectionStringEditor

        public bool ShowEditor(string currentConnectionString)
        {
            var editor = new SQLiteConnectionStringEditor(currentConnectionString);
            var result = editor.ShowDialog();
            if (result != DialogResult.OK)
                return false;

            _connectionString = editor.ConnectionString;
            return true;
        }

        private string _connectionString = string.Empty;

        public string ConnectionString
        {
            get { return _connectionString; }
        }

        public bool EditorAvailable
        {
            get { return true; }
        }

        #endregion

        #region DbProvider Helpers

        private static DbProviderFactory CreateDbProviderFactory() {
            DbProviderFactory dbProviderFactory;

            try {
                dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SQLite");
            } catch (ArgumentException ex) {
                throw new ApplicationException("The System.Data.SQLite library is not installed on this computer. Please see the following documentation 'http://docs.codesmithtools.com/display/Generator/Configuring+and+troubleshooting+a+Schema+Provider' for more information. Error: {0}", ex);
            }

            return dbProviderFactory;
        }

        private static DbConnection CreateConnection(string connectionString)
        {
            DbProviderFactory factory = CreateDbProviderFactory();
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;

            return connection;
        }

        #endregion
    }
}