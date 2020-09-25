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
using System.IO;
using System.Windows.Forms;
using VistaDB;
using VistaDB.DDA;
using VistaDB.Provider;
using SchemaExplorer.Designer;

namespace SchemaExplorer
{
    public class VistaDB4SchemaProvider : IDbSchemaProvider, IDbConnectionStringEditor
    {
        private static readonly IVistaDBDDA _dda = VistaDBEngine.Connections.OpenDDA();

        #region IDbSchemaProvider Members

        public string Name
        {
            get { return "VistaDB4SchemaProvider"; }
        }

        public string Description
        {
            get { return "SchemaExplorer Provider for VistaDB 4.x"; }
        }

        public string GetDatabaseName(string connectionString)
        {
            using (IVistaDBDatabase vistaDb = GetDatabase(connectionString))
            {
                //HACK: name is the file name?
                return Path.GetFileNameWithoutExtension(vistaDb.Name);
            }
        }

        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            return new ExtendedProperty[0];
        }

        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            throw new NotImplementedException();
        }

        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            var tables = new List<TableSchema>();
            var extendedProperties = new List<ExtendedProperty>();

            using (IVistaDBDatabase vistaDb = GetDatabase(connectionString))
            {
                if (vistaDb == null)
                    return tables.ToArray();

                foreach (string tableName in vistaDb.GetTableNames())
                {
                    IVistaDBTableSchema table = vistaDb.TableSchema(tableName);
                    if (table == null)
                        continue;

                    extendedProperties.Clear();
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, table.Description));

                    var tableSchema = new TableSchema(database, table.Name, String.Empty, DateTime.MinValue, extendedProperties.ToArray());

                    tables.Add(tableSchema);
                }
            }

            return tables.ToArray();
        }

        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            var columns = new List<ColumnSchema>();
            var extendedProperties = new List<ExtendedProperty>();

            using (IVistaDBDatabase vistaDb = GetDatabase(connectionString))
            {
                if (vistaDb == null)
                    return columns.ToArray();

                IVistaDBTableSchema vistaTable = vistaDb.TableSchema(table.Name);
                if (vistaTable == null)
                    return columns.ToArray();

                foreach (IVistaDBColumnAttributes vistaColumn in vistaTable)
                {
                    string nativeType = vistaColumn.Type.ToString();

                    extendedProperties.Clear();
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, vistaColumn.Description));
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.ObjectID, vistaColumn.UniqueId));
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.DefaultValue, ""));
                    extendedProperties.Add(ExtendedProperty.Readonly("CS_CodePage", vistaColumn.CodePage));
                    extendedProperties.Add(ExtendedProperty.Readonly("CS_Encrypted", vistaColumn.Encrypted));
                    extendedProperties.Add(ExtendedProperty.Readonly("CS_Packed", vistaColumn.Packed));
                    extendedProperties.Add(ExtendedProperty.Readonly("CS_ReadOnly", vistaColumn.ReadOnly));

                    bool isIdentity = false;
                    foreach (IVistaDBIdentityInformation identity in vistaTable.Identities)
                    {
                        isIdentity = (identity.ColumnName == vistaColumn.Name);
                        if (isIdentity)
                            break;
                    }

                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsIdentity, isIdentity));

                    var column = new ColumnSchema(table, vistaColumn.Name, GetDbType(nativeType),
                        nativeType, vistaColumn.MaxLength, 0, 0, vistaColumn.AllowNull,
                        extendedProperties.ToArray());

                    columns.Add(column);
                }
            }


            return columns.ToArray();
        }

        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            var views = new List<ViewSchema>();
            var extendedProperties = new List<ExtendedProperty>();

            const string sql = "SELECT VIEW_NAME, DESCRIPTION, IS_UPDATABLE FROM GetViews()";
            using (VistaDBConnection connection = GetConnection(connectionString))
            using (var adapter = new VistaDBDataAdapter(sql, connection))
            using (var table = new DataTable())
            {
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    string name = row[0].ToString();
                    string description = row[1].ToString();
                    bool isUpdatable = (bool)(row[2] ?? true);

                    extendedProperties.Clear();
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, description));
                    extendedProperties.Add(ExtendedProperty.Readonly("CS_IsUpdatable", isUpdatable));

                    var view = new ViewSchema(database, name, string.Empty, DateTime.MinValue, extendedProperties.ToArray());

                    views.Add(view);
                }
            }

            return views.ToArray();
        }

        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            var columns = new List<ViewColumnSchema>();

            string sql = string.Format("SELECT * FROM GetViewColumns('{0}')", view.Name);

            using (VistaDBConnection connection = GetConnection(connectionString))
            using (var adapter = new VistaDBDataAdapter(sql, connection))
            using (var table = new DataTable())
            {
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    string name = row["COLUMN_NAME"].ToString();
                    string nativeType = row["DATA_TYPE_NAME"].ToString();
                    var size = (int)(row["COLUMN_SIZE"] ?? 0);
                    var allowNull = (bool)(row["ALLOW_NULL"] ?? true);

                    var column = new ViewColumnSchema(
                        view, name, GetDbType(nativeType),
                        nativeType, size, 0, 0, allowNull);

                    columns.Add(column);
                }
            }

            return columns.ToArray();
        }

        public string GetViewText(string connectionString, ViewSchema view)
        {
            string sql = string.Format("select VIEW_DEFINITION from GetViews() where VIEW_NAME = '{0}'", view.Name);

            using (VistaDBConnection connection = GetConnection(connectionString))
            using (var adapter = new VistaDBDataAdapter(sql, connection))
            using (var table = new DataTable())
            {
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    string definition = row[0].ToString();
                    return definition;
                }
            }

            return string.Empty;
        }

        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            using (IVistaDBDatabase vistaDb = GetDatabase(connectionString))
            {
                if (vistaDb == null)
                    return null;

                IVistaDBTableSchema vistaTable = vistaDb.TableSchema(table.Name);
                if (vistaTable == null)
                    return null;

                foreach (IVistaDBIndexInformation vistaIndex in vistaTable.Indexes)
                {
                    if (!vistaIndex.Primary)
                        continue;

                    var key = new PrimaryKeySchema(
                        table, vistaIndex.Name,
                        vistaIndex.KeyExpression.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                    return key;
                }
            }

            return null;
        }

        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            var keys = new List<TableKeySchema>();
            var foreignColumns = new List<string>();
            var primaryColumns = new List<string>();
            var extendedProperties = new List<ExtendedProperty>();

            using (IVistaDBDatabase vistaDb = GetDatabase(connectionString))
            {
                if (vistaDb == null)
                    return keys.ToArray();

                IVistaDBTableSchema vistaTable = vistaDb.TableSchema(table.Name);
                if (vistaTable == null)
                    return keys.ToArray();

                foreach (IVistaDBRelationshipInformation vistaKey in vistaTable.ForeignKeys)
                {
                    foreignColumns.Clear();
                    foreignColumns.AddRange(vistaKey.ForeignKey.Split(
                        new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                    IVistaDBTableSchema vistaPrimaryTable = vistaDb.TableSchema(vistaKey.PrimaryTable);
                    if (vistaPrimaryTable == null)
                        continue;

                    primaryColumns.Clear();
                    //find primary key index
                    foreach (IVistaDBIndexInformation i in vistaPrimaryTable.Indexes)
                        if (i.Primary)
                            primaryColumns.AddRange(i.KeyExpression.Split(
                                new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                    extendedProperties.Clear();
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, vistaKey.Description));

                    var key = new TableKeySchema(
                        table.Database, vistaKey.Name,
                        foreignColumns.ToArray(),
                        string.Empty, vistaKey.ForeignTable,
                        primaryColumns.ToArray(),
                        string.Empty, vistaKey.PrimaryTable,
                        extendedProperties.ToArray());

                    keys.Add(key);
                }
            }

            return keys.ToArray();
        }

        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            var indexes = new List<IndexSchema>();
            var memberColumns = new List<string>();
            var extendedProperties = new List<ExtendedProperty>();

            using (IVistaDBDatabase vistaDb = GetDatabase(connectionString))
            {
                if (vistaDb == null)
                    return indexes.ToArray();

                IVistaDBTableSchema vistaTable = vistaDb.TableSchema(table.Name);
                if (vistaTable == null)
                    return indexes.ToArray();

                foreach (IVistaDBIndexInformation vistaIndex in vistaTable.Indexes)
                {
                    memberColumns.Clear();
                    foreach (IVistaDBKeyColumn keyColumn in vistaIndex.KeyStructure)
                        memberColumns.Add(vistaTable[keyColumn.RowIndex].Name);

                    extendedProperties.Clear();
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, vistaIndex.Description));
                    extendedProperties.Add(ExtendedProperty.Readonly("CS_FastTextSearch", vistaIndex.FullTextSearch));

                    var index = new IndexSchema(table, vistaIndex.Name, vistaIndex.Primary,
                                                vistaIndex.Unique, false,
                                                memberColumns.ToArray(),
                                                extendedProperties.ToArray());

                    indexes.Add(index);
                }
            }

            return indexes.ToArray();
        }

        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            string sql = string.Format("select * from '{0}'", table.Name);

            using (VistaDBConnection connection = GetConnection(connectionString))
            using (var adapter = new VistaDBDataAdapter(sql, connection))
            using (var dataTable = new DataTable())
            {
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            string sql = string.Format("select * from '{0}'", view.Name);

            using (VistaDBConnection connection = GetConnection(connectionString))
            using (var adapter = new VistaDBDataAdapter(sql, connection))
            using (var dataTable = new DataTable())
            {
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            var commands = new List<CommandSchema>();
            var extendedProperties = new List<ExtendedProperty>();

            const string sql = "SELECT PROC_NAME, PROC_DESCRIPTION FROM sp_stored_procedures()";

            using (VistaDBConnection connection = GetConnection(connectionString))
            using (var adapter = new VistaDBDataAdapter(sql, connection))
            using (var table = new DataTable())
            {
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    string name = row[0].ToString();
                    string description = row[1].ToString();

                    extendedProperties.Clear();
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, description));

                    var command = new CommandSchema(database, name, string.Empty, DateTime.MinValue, extendedProperties.ToArray());

                    commands.Add(command);
                }
            }

            return commands.ToArray();
        }

        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            var parameters = new List<ParameterSchema>();
            var extendedProperties = new List<ExtendedProperty>();

            string sql = string.Format("SELECT PARAM_ORDER, PARAM_NAME, PARAM_TYPE, IS_PARAM_OUT, DEFAULT_VALUE FROM sp_stored_procedures() WHERE PROC_NAME = '{0}'", command.Name);

            using (VistaDBConnection connection = GetConnection(connectionString))
            using (var adapter = new VistaDBDataAdapter(sql, connection))
            using (var table = new DataTable())
            {
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    string name = row["PARAM_NAME"].ToString();
                    var vistaType = (VistaDBType)(row["PARAM_TYPE"] ?? VistaDBType.Unknown);
                    var isOut = (bool)(row["IS_PARAM_OUT"] ?? false);
                    string defaultValue = row["DEFAULT_VALUE"].ToString();

                    extendedProperties.Clear();
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Description, string.Empty));
                    extendedProperties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.DefaultValue, defaultValue));

                    var parameter = new ParameterSchema(command, name,
                                                        isOut ? ParameterDirection.InputOutput : ParameterDirection.Input,
                                                        GetDbType(vistaType.ToString()),
                                                        vistaType.ToString(), 0, 0, 0, true,
                                                        extendedProperties.ToArray());

                    parameters.Add(parameter);
                }
            }

            return parameters.ToArray();
        }

        public string GetCommandText(string connectionString, CommandSchema command)
        {
            string sql = string.Format("SELECT PROC_BODY FROM sp_stored_procedures() WHERE PROC_NAME = '{0}'", command.Name);

            using (VistaDBConnection connection = GetConnection(connectionString))
            using (var adapter = new VistaDBDataAdapter(sql, connection))
            using (var table = new DataTable())
            {
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    string body = row[0].ToString();
                    return body;
                }
            }
            return null;
        }

        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            return new CommandResultSchema[0];
        }

        #endregion

        private VistaDBConnection GetConnection(string connectionString)
        {
            var connection = new VistaDBConnection(connectionString);
            connection.Open();
            return connection;
        }

        private IVistaDBDatabase GetDatabase(string connectionString)
        {
            var builder = new VistaDBConnectionStringBuilder(connectionString);
            var password = !String.IsNullOrWhiteSpace(builder.Password) ? builder.Password : null;
            IVistaDBDatabase database = _dda.OpenDatabase(builder.DataSource, builder.OpenMode, password);
            return database;
        }

        private DbType GetDbType(string dataType)
        {
            switch (dataType.ToLower())
            {
                case "int":
                    return DbType.Int32;
                case "bit":
                    return DbType.Boolean;
                case "datetime":
                    return DbType.DateTime;
                case "image":
                    return DbType.Binary;
                case "money":
                    return DbType.Currency;
                case "char":
                    return DbType.AnsiStringFixedLength;
                case "nchar":
                    return DbType.StringFixedLength;
                case "float":
                    return DbType.Single;
                case "bigint":
                    return DbType.Int64;
                case "smalldatetime":
                    return DbType.DateTime;
                case "numeric":
                    return DbType.VarNumeric;
                case "real":
                    return DbType.Double;
                case "smallint":
                    return DbType.Int16;
                case "smallmoney":
                    return DbType.Currency;
                case "varchar":
                    return DbType.AnsiString;
                case "nvarchar":
                    return DbType.String;
                case "uniqueidentifier":
                    return DbType.Guid;
                case "varbinary":
                    return DbType.Binary;
                case "XML":
                    return DbType.String;
                case "ntext":
                    return DbType.String;
                default:
                    return DbType.String;
            }
        }

        #region Implementation of IDbConnectionStringEditor

        public bool ShowEditor(string currentConnectionString)
        {
            var editor = new VistaDBConnectionStringEditor(currentConnectionString);
            DialogResult result = editor.ShowDialog();
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
    }
}