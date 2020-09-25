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
using System.Collections;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Data.ConnectionUI;

namespace SchemaExplorer
{
    public class ADOXSchemaProvider : IDbSchemaProvider, IDbConnectionStringEditor
    {
        private ADODB.Connection _connection;
        private ADOX.Catalog _catalog;
        
        public ADOXSchemaProvider()
        {
        }
        
        public string Name
        {
            get	{return "ADOXSchemaProvider";}
        }
        
        public string Description
        {
            get	{return "ADOX Schema Provider";}
        }
        
        private ADOX.Catalog GetCatalog(string connectionString)
        {
            if (_catalog == null)
            {
                _connection = new ADODB.ConnectionClass();
                _connection.Open(connectionString, null, null, 0);

                _catalog = new ADOX.CatalogClass {ActiveConnection = _connection};
            }
            
            return _catalog;
        }
        
        private void Cleanup()
        {
            if (_connection != null && _connection.State == (int)ADODB.ObjectStateEnum.adStateOpen)
            {
                _connection.Close();
            }
            _connection = null;
            _catalog = null;
        }
        
        public string GetDatabaseName(string connectionString)
        {
            Regex databaseNameRegex = new Regex(@"Initial Catalog\W*=\W*(?<database>[^;]*)|Database\W*=\W*(?<database>[^;]*)", RegexOptions.IgnoreCase);
            Match databaseNameMatch;
            
            if ((databaseNameMatch = databaseNameRegex.Match(connectionString)).Success)
            {
                return databaseNameMatch.Groups["database"].ToString();
            }
            return connectionString;
        }
        
        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            return new ExtendedProperty[0];
        }

        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            
            ArrayList tables = new ArrayList();
            
            if (catalog.Tables != null)
            {
                for (int i = 0; i < catalog.Tables.Count; i ++)
                {
                    if (catalog.Tables[i].Type.Trim().ToUpper() == "TABLE")
                    {
                        // TODO: Need to figure out how to convert catalog.Tables[i].DateCreated to a DateTime object.
                        tables.Add(new TableSchema(database, catalog.Tables[i].Name, "", DateTime.MinValue, ConvertToExtendedProperties(catalog.Tables[i].Properties))); //catalog.GetObjectOwner(catalog.Tables[i].Name, ADOX.ObjectTypeEnum.adPermObjTable, null));
                    }
                }
            }
            
            Cleanup();
            
            return (TableSchema[])tables.ToArray(typeof(TableSchema));
        }
        
        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            ADOX.Table adoxtable = catalog.Tables[table.Name];
            
            ArrayList columns = new ArrayList();
            
            if (adoxtable.Columns != null)
            {
                for (int i = 0; i < adoxtable.Columns.Count; i++)
                {
                    var properties = new ExtendedPropertyCollection(ConvertToExtendedProperties(adoxtable.Columns[i].Properties));
                    if(adoxtable.Columns[i].Properties["Default"] != null)
                        properties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, adoxtable.Columns[i].Properties["Default"].Value, DbType.String, PropertyStateEnum.ReadOnly));

                    bool allowDBNull = adoxtable.Columns[i].Properties["Nullable"] != null && (bool)adoxtable.Columns[i].Properties["Nullable"].Value;
                    columns.Add(new ColumnSchema(
                        table, 
                        adoxtable.Columns[i].Name, 
                        GetDbType(adoxtable.Columns[i].Type), 
                        adoxtable.Columns[i].Type.ToString(), 
                        adoxtable.Columns[i].DefinedSize, 
                        Convert.ToByte(adoxtable.Columns[i].Precision), 
                        adoxtable.Columns[i].NumericScale, 
                        allowDBNull,
                        properties.ToArray()));
                }
            }
            
            Cleanup();
            
            return (ColumnSchema[])columns.ToArray(typeof(ColumnSchema));
        }
        
        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            
            ArrayList views = new ArrayList();
            
            if (catalog.Views != null)
            {
                for (int i = 0; i < catalog.Views.Count; i ++)
                {
                    views.Add(new ViewSchema(database, catalog.Views[i].Name, "", DateTime.MinValue)); //catalog.GetObjectOwner(catalog.Views[i].Name, ADOX.ObjectTypeEnum.adPermObjView, null));
                }
            }
            
            Cleanup();
            
            return (ViewSchema[])views.ToArray(typeof(ViewSchema));
        }
        
        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            throw new NotSupportedException("GetViewColumns() is not supported with this provider.");
        }
        
        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            ADOX.Table adoxtable = catalog.Tables[table.Name];
            
            if (adoxtable.Keys != null)
            {
                for (int i = 0; i < adoxtable.Keys.Count; i++)
                {
                    if (adoxtable.Keys[i].Type == ADOX.KeyTypeEnum.adKeyPrimary)
                    {
                        string[] memberColumns = new string[adoxtable.Keys[i].Columns.Count];
                        for (int x = 0; x < adoxtable.Keys[i].Columns.Count; x++)
                        {
                            memberColumns[x] = adoxtable.Keys[i].Columns[x].Name;
                        }
                    
                        Cleanup();
                        
                        var extendedProperties = new ExtendedPropertyCollection();
                        extendedProperties.Add(new ExtendedProperty("DeleteRule", adoxtable.Keys[i].DeleteRule.ToString(), DbType.String, PropertyStateEnum.ReadOnly));
                        extendedProperties.Add(new ExtendedProperty("UpdateRule", adoxtable.Keys[i].UpdateRule.ToString(), DbType.String, PropertyStateEnum.ReadOnly));
                        
                        return new PrimaryKeySchema(table, adoxtable.Keys[i].Name, memberColumns, extendedProperties.ToArray());
                    }
                }
            }
            
            Cleanup();
            
            return null;
        }
        
        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            ADOX.Table adoxtable = catalog.Tables[table.Name];

            IndexSchema[] indexes = new IndexSchema[0];
            
            if (adoxtable.Indexes != null)
            {
                indexes = new IndexSchema[adoxtable.Indexes.Count];
                
                for (int i = 0; i < adoxtable.Indexes.Count; i++)
                {
                    string[] memberColumns = new string[adoxtable.Indexes[i].Columns.Count];
                
                    for (int x = 0; x < adoxtable.Indexes[i].Columns.Count; x++)
                    {
                        memberColumns[x] = adoxtable.Indexes[i].Columns[x].Name;
                    }
                
                    indexes[i] = new IndexSchema(table, adoxtable.Indexes[i].Name, adoxtable.Indexes[i].PrimaryKey, adoxtable.Indexes[i].Unique, adoxtable.Indexes[i].Clustered, memberColumns, ConvertToExtendedProperties(adoxtable.Indexes[i].Properties));
                }
            }
            
            Cleanup();
            
            return indexes;
        }
        
        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            ADOX.Table adoxtable = catalog.Tables[table.Name];
            
            List<TableKeySchema> keys = new List<TableKeySchema>();
            
            if (adoxtable.Keys != null)
            {
                for (int i = 0; i < adoxtable.Keys.Count; i++)
                {
                    if (adoxtable.Keys[i].Type == ADOX.KeyTypeEnum.adKeyForeign)
                    {
                        string[] primaryKeyMemberColumns = new string[adoxtable.Keys[i].Columns.Count];
                        string[] foreignKeyMemberColumns = new string[adoxtable.Keys[i].Columns.Count];

                        for (int x = 0; x < adoxtable.Keys[i].Columns.Count; x++)
                        {
                            primaryKeyMemberColumns[x] = adoxtable.Keys[i].Columns[x].RelatedColumn;
                            foreignKeyMemberColumns[x] = adoxtable.Keys[i].Columns[x].Name;
                        }
                        
                        var extendedProperties = new ExtendedPropertyCollection();
                        extendedProperties.Add(new ExtendedProperty("DeleteRule", adoxtable.Keys[i].DeleteRule.ToString(), DbType.String, PropertyStateEnum.ReadOnly));
                        extendedProperties.Add(new ExtendedProperty("UpdateRule", adoxtable.Keys[i].UpdateRule.ToString(), DbType.String, PropertyStateEnum.ReadOnly));
                        
                        keys.Add(new TableKeySchema(table.Database, adoxtable.Keys[i].Name, foreignKeyMemberColumns, table.Name, primaryKeyMemberColumns, adoxtable.Keys[i].RelatedTable, extendedProperties.ToArray()));
                    }
                }
            }
            
            for (int i = 0; i < catalog.Tables.Count; i++)
            {
                var catalogKeys = catalog.Tables[i].Keys;
                if (catalogKeys == null) continue;

                for (int x = 0; x < catalogKeys.Count; x++)
                {
                    if (catalogKeys[x].RelatedTable == table.Name && catalogKeys[x].Type == ADOX.KeyTypeEnum.adKeyForeign)
                    {
                        string[] primaryKeyMemberColumns = new string[catalogKeys[x].Columns.Count];
                        string[] foreignKeyMemberColumns = new string[catalogKeys[x].Columns.Count];
                            
                        for (int y = 0; y < catalogKeys[x].Columns.Count; y++)
                        {
                            primaryKeyMemberColumns[y] = catalogKeys[x].Columns[y].Name;
                            foreignKeyMemberColumns[y] = catalogKeys[x].Columns[y].RelatedColumn;
                        }
                            
                        var extendedProperties = new ExtendedPropertyCollection();
                        extendedProperties.Add(new ExtendedProperty("DeleteRule", catalogKeys[x].DeleteRule.ToString(), DbType.String, PropertyStateEnum.ReadOnly));
                        extendedProperties.Add(new ExtendedProperty("UpdateRule", catalogKeys[x].UpdateRule.ToString(), DbType.String, PropertyStateEnum.ReadOnly));
                        keys.Add(new TableKeySchema(table.Database, catalogKeys[x].Name, foreignKeyMemberColumns, catalog.Tables[i].Name, primaryKeyMemberColumns, table.Name, extendedProperties.ToArray()));
                    }
                }
            }
            
            Cleanup();
            
            return keys.ToArray();
        }
        
        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            string sqlstring = @"
                SELECT
                    *
                FROM
                    " + table.Name;
            
            ADODB.Connection cn = new ADODB.ConnectionClass();
            cn.Open(connectionString, null, null, 0);
            
            ADODB.Recordset rs = new ADODB.RecordsetClass();
            rs.Open(sqlstring, cn, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockReadOnly, 0);
            
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter();
            DataTable tableData = new DataTable();
            dataAdapter.Fill(tableData, rs);
            
            rs.Close();
            cn.Close();
            
            return tableData;
        }
        
        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            string sqlstring = @"
                SELECT
                    *
                FROM
                    " + view.Name;
            
            ADODB.Connection cn = new ADODB.ConnectionClass();
            cn.Open(connectionString, null, null, 0);
            
            ADODB.Recordset rs = new ADODB.RecordsetClass();
            rs.Open(sqlstring, cn, ADODB.CursorTypeEnum.adOpenForwardOnly, ADODB.LockTypeEnum.adLockReadOnly, 0);
            
            OleDbDataAdapter dataAdapter = new OleDbDataAdapter();
            DataTable viewData = new DataTable();
            dataAdapter.Fill(viewData, rs);
            
            rs.Close();
            cn.Close();
            
            return viewData;
        }
        
        public string GetViewText(string connectionString, ViewSchema view)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            ADOX.View v = catalog.Views[view.Name];
            ADODB.Command cmd = v.Command as ADODB.Command;
            
            return cmd != null ? cmd.CommandText : String.Empty;
        }
        
        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            
            ArrayList commands = new ArrayList();
            
            for (int i = 0; i < catalog.Procedures.Count; i++)
            {
                ADOX.Procedure proc = catalog.Procedures[i];
                ADODB.Command cmd = (ADODB.Command)proc.Command;
                commands.Add(new CommandSchema(database, catalog.Procedures[i].Name, "", DateTime.MinValue, ConvertToExtendedProperties(cmd.Properties))); //catalog.GetObjectOwner(catalog.Procedures[i].Name, ADOX.ObjectTypeEnum.adPermObjProcedure, null));
            }
            
            Cleanup();
            
            return (CommandSchema[])commands.ToArray(typeof(CommandSchema));
        }
        
        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            ADOX.Procedure adoxprocedure = catalog.Procedures[command.Name];
            
            ADODB.Command cmd = (ADODB.Command)adoxprocedure.Command;
            cmd.Parameters.Refresh();
            
            ParameterSchema[] parameters = new ParameterSchema[cmd.Parameters.Count];

            for (int i = 0; i < cmd.Parameters.Count; i++)
            {
                var properties = new ExtendedPropertyCollection(ConvertToExtendedProperties(cmd.Parameters[i].Properties));
                bool allowDBNull = false;

                try
                {
                    allowDBNull = cmd.Parameters[i].Properties["Nullable"] != null && (bool)cmd.Parameters[i].Properties["Nullable"].Value;

                    if (cmd.Parameters[i].Properties["Default"] != null)
                        properties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, cmd.Parameters[i].Properties["Default"].Value, DbType.String, PropertyStateEnum.ReadOnly));

                }
                catch {}
                
                parameters[i] = new ParameterSchema(
                    command, 
                    cmd.Parameters[i].Name, 
                    GetParameterDirection(cmd.Parameters[i].Direction), 
                    GetDbType(cmd.Parameters[i].Type), 
                    cmd.Parameters[i].Type.ToString(), 
                    cmd.Parameters[i].Size, 
                    cmd.Parameters[i].Precision, 
                    cmd.Parameters[i].NumericScale,
                    allowDBNull,
                    properties.ToArray());
            }
            
            Cleanup();
            
            return parameters;
        }
        
        public string GetCommandText(string connectionString, CommandSchema command)
        {
            ADOX.Catalog catalog = GetCatalog(connectionString);
            ADOX.Procedure procedure = catalog.Procedures[command.Name];
            ADODB.Command cmd = procedure.Command as ADODB.Command;
            
            return cmd != null ? cmd.CommandText : String.Empty;
        }
        
        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            throw new NotSupportedException("GetCommandResultSchemas() is not supported in this provider.");
        }
        
        private ExtendedProperty[] ConvertToExtendedProperties(ADOX.Properties properties)
        {
            ArrayList extendedProperties = new ArrayList();
            string description = "";
            
            for (int i = 0; i < properties.Count; i++)
            {
                object value;

                try
                {
                    value = properties[i].Value;
                }
                catch (InvalidCastException)
                {
                    //Interface is not supported. This was thrown by trying to load up indexes on the north wind access database.
                    continue;
                } 
                catch (System.Runtime.InteropServices.COMException)
                {
                    //This gets thrown by the VFP9 ADO provider for older-type FoxPro tables (version 2-2.6a) - the wonderful 0x80004005 - Unspecified error
                    //TODO: Make sure that this is the provider we're currently using so that we don't eat up exceptions from other types of providers
                    //NOTE: Maybe add our own expected property that says we've hit this exception
                    // Reference: http://community.codesmithtools.com/Template_Frameworks/f/67/p/9802/44467.aspx#44467
                    continue;
                }

                ExtendedProperty extendedProperty = new ExtendedProperty(properties[i].Name, value, GetDbType(properties[i].Type));
                extendedProperties.Add(extendedProperty);
                if (extendedProperty.Name == "Description" && extendedProperty.Value != DBNull.Value && extendedProperty.Value != null)
                {
                    description = (string)extendedProperty.Value;
                }
            }
            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.Description, description, DbType.String, PropertyStateEnum.ReadOnly));
            
            return (ExtendedProperty[])extendedProperties.ToArray(typeof(ExtendedProperty));
        }
        
        private ExtendedProperty[] ConvertToExtendedProperties(ADODB.Properties properties)
        {
            ArrayList extendedProperties = new ArrayList();
            
            for (int i = 0; i < properties.Count; i++)
            {
                ExtendedProperty extendedProperty = new ExtendedProperty(properties[i].Name, properties[i].Value, GetDbType(properties[i].Type));
                extendedProperties.Add(extendedProperty);
            }
            
            return (ExtendedProperty[])extendedProperties.ToArray(typeof(ExtendedProperty));
        }
        
        private DbType GetDbType(ADOX.DataTypeEnum type)
        {
            switch (type)
            {
                case ADOX.DataTypeEnum.adBigInt: return DbType.Int64;
                case ADOX.DataTypeEnum.adBinary: return DbType.Binary;
                case ADOX.DataTypeEnum.adBoolean: return DbType.Boolean;
                case ADOX.DataTypeEnum.adBSTR: return DbType.AnsiString;
                case ADOX.DataTypeEnum.adChapter: return DbType.Object; // TODO: Investigate data type.
                case ADOX.DataTypeEnum.adChar: return DbType.AnsiStringFixedLength;
                case ADOX.DataTypeEnum.adCurrency: return DbType.Currency;
                case ADOX.DataTypeEnum.adDate: return DbType.DateTime;
                case ADOX.DataTypeEnum.adDBDate: return DbType.DateTime;
                case ADOX.DataTypeEnum.adDBTime: return DbType.Time;
                case ADOX.DataTypeEnum.adDBTimeStamp: return DbType.Binary;
                case ADOX.DataTypeEnum.adDecimal: return DbType.Decimal;
                case ADOX.DataTypeEnum.adDouble: return DbType.Decimal;
                case ADOX.DataTypeEnum.adEmpty: return DbType.Object; // TODO: Investigate data type.
                case ADOX.DataTypeEnum.adError: return DbType.Object; // TODO: Investigate data type.
                case ADOX.DataTypeEnum.adFileTime: return DbType.Object; // TODO: Investigate data type.
                case ADOX.DataTypeEnum.adGUID: return DbType.Guid;
                case ADOX.DataTypeEnum.adIDispatch: return DbType.Object; // TODO: Investigate data type.
                case ADOX.DataTypeEnum.adInteger: return DbType.Int32;
                case ADOX.DataTypeEnum.adIUnknown: return DbType.Object; // TODO: Investigate data type.
                case ADOX.DataTypeEnum.adLongVarBinary: return DbType.Binary;
                case ADOX.DataTypeEnum.adLongVarChar: return DbType.String;
                case ADOX.DataTypeEnum.adLongVarWChar: return DbType.String;
                case ADOX.DataTypeEnum.adNumeric: return DbType.Decimal;
                case ADOX.DataTypeEnum.adPropVariant: return DbType.Object;
                case ADOX.DataTypeEnum.adSingle: return DbType.Decimal;
                case ADOX.DataTypeEnum.adSmallInt: return DbType.Int16;
                case ADOX.DataTypeEnum.adTinyInt: return DbType.SByte;
                case ADOX.DataTypeEnum.adUnsignedBigInt: return DbType.UInt64;
                case ADOX.DataTypeEnum.adUnsignedInt: return DbType.UInt32;
                case ADOX.DataTypeEnum.adUnsignedSmallInt: return DbType.UInt16;
                case ADOX.DataTypeEnum.adUnsignedTinyInt: return DbType.Byte;
                case ADOX.DataTypeEnum.adUserDefined: return DbType.Object;
                case ADOX.DataTypeEnum.adVarBinary: return DbType.Binary;
                case ADOX.DataTypeEnum.adVarChar: return DbType.AnsiString;
                case ADOX.DataTypeEnum.adVariant: return DbType.Object;
                case ADOX.DataTypeEnum.adVarNumeric: return DbType.Decimal;
                case ADOX.DataTypeEnum.adVarWChar: return DbType.String;
                case ADOX.DataTypeEnum.adWChar: return DbType.StringFixedLength;
                default: return DbType.Object;
            }
        }
        
        private DbType GetDbType(ADODB.DataTypeEnum type)
        {
            switch (type)
            {
                case ADODB.DataTypeEnum.adBigInt: return DbType.Int64;
                case ADODB.DataTypeEnum.adBinary: return DbType.Binary;
                case ADODB.DataTypeEnum.adBoolean: return DbType.Boolean;
                case ADODB.DataTypeEnum.adBSTR: return DbType.AnsiString;
                case ADODB.DataTypeEnum.adChapter: return DbType.Object; // TODO: Investigate data type.
                case ADODB.DataTypeEnum.adChar: return DbType.AnsiStringFixedLength;
                case ADODB.DataTypeEnum.adCurrency: return DbType.Currency;
                case ADODB.DataTypeEnum.adDate: return DbType.DateTime;
                case ADODB.DataTypeEnum.adDBDate: return DbType.DateTime;
                case ADODB.DataTypeEnum.adDBTime: return DbType.Time;
                case ADODB.DataTypeEnum.adDBTimeStamp: return DbType.Binary;
                case ADODB.DataTypeEnum.adDecimal: return DbType.Decimal;
                case ADODB.DataTypeEnum.adDouble: return DbType.Decimal;
                case ADODB.DataTypeEnum.adEmpty: return DbType.Object; // TODO: Investigate data type.
                case ADODB.DataTypeEnum.adError: return DbType.Object; // TODO: Investigate data type.
                case ADODB.DataTypeEnum.adFileTime: return DbType.Object; // TODO: Investigate data type.
                case ADODB.DataTypeEnum.adGUID: return DbType.Guid;
                case ADODB.DataTypeEnum.adIDispatch: return DbType.Object; // TODO: Investigate data type.
                case ADODB.DataTypeEnum.adInteger: return DbType.Int32;
                case ADODB.DataTypeEnum.adIUnknown: return DbType.Object; // TODO: Investigate data type.
                case ADODB.DataTypeEnum.adLongVarBinary: return DbType.Binary;
                case ADODB.DataTypeEnum.adLongVarChar: return DbType.String;
                case ADODB.DataTypeEnum.adLongVarWChar: return DbType.String;
                case ADODB.DataTypeEnum.adNumeric: return DbType.Decimal;
                case ADODB.DataTypeEnum.adPropVariant: return DbType.Object;
                case ADODB.DataTypeEnum.adSingle: return DbType.Decimal;
                case ADODB.DataTypeEnum.adSmallInt: return DbType.Int16;
                case ADODB.DataTypeEnum.adTinyInt: return DbType.SByte;
                case ADODB.DataTypeEnum.adUnsignedBigInt: return DbType.UInt64;
                case ADODB.DataTypeEnum.adUnsignedInt: return DbType.UInt32;
                case ADODB.DataTypeEnum.adUnsignedSmallInt: return DbType.UInt16;
                case ADODB.DataTypeEnum.adUnsignedTinyInt: return DbType.Byte;
                case ADODB.DataTypeEnum.adUserDefined: return DbType.Object;
                case ADODB.DataTypeEnum.adVarBinary: return DbType.Binary;
                case ADODB.DataTypeEnum.adVarChar: return DbType.AnsiString;
                case ADODB.DataTypeEnum.adVariant: return DbType.Object;
                case ADODB.DataTypeEnum.adVarNumeric: return DbType.Decimal;
                case ADODB.DataTypeEnum.adVarWChar: return DbType.String;
                case ADODB.DataTypeEnum.adWChar: return DbType.StringFixedLength;
                default: return DbType.Object;
            }
        }
        
        private ParameterDirection GetParameterDirection(ADODB.ParameterDirectionEnum direction)
        {
            switch (direction)
            {
                case ADODB.ParameterDirectionEnum.adParamInput: return ParameterDirection.Input;
                case ADODB.ParameterDirectionEnum.adParamInputOutput: return ParameterDirection.InputOutput;
                case ADODB.ParameterDirectionEnum.adParamOutput: return ParameterDirection.Output;
                case ADODB.ParameterDirectionEnum.adParamReturnValue: return ParameterDirection.ReturnValue;
                default: return ParameterDirection.Input;
            }
        }

        #region IDbConnectionStringEditor Members

        public bool ShowEditor(string currentConnectionString)
        {
            DataConnectionDialog dcd = new DataConnectionDialog();
            dcd.DataSources.Add(Microsoft.Data.ConnectionUI.DataSource.AccessDataSource);
            dcd.DataSources.Add(Microsoft.Data.ConnectionUI.DataSource.OdbcDataSource);
            dcd.SelectedDataSource = Microsoft.Data.ConnectionUI.DataSource.AccessDataSource;
            dcd.SelectedDataProvider = DataProvider.OleDBDataProvider;

            try
            {
                dcd.ConnectionString = currentConnectionString;
            }
            catch { }

            var result = DataConnectionDialog.Show(dcd);
            if (result == DialogResult.OK)
                ConnectionString = dcd.ConnectionString;

            return result == DialogResult.OK;
        }

        public string ConnectionString { get; private set; }

        public bool EditorAvailable
        {
            get { return true; }
        }

        #endregion
    }
}