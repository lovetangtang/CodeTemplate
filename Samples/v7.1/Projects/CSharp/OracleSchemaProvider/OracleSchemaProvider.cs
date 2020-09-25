//------------------------------------------------------------------------------
//
//Copyright (C) 2004, Michael Toscano and Geoff McElhanon.  All Rights Reserved.
//
//This library is free software; you can redistribute it and/or
//modify it under the terms of the GNU Lesser General Public
//License as published by the Free Software Foundation; either
//version 2.1 of the License, or (at your option) any later version.
//
//This library is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//Lesser General Public License for more details.
//
//	NOTE: This copyright notice must accompany all modified versions of this code.
//  Implementation by Michael Toscano (mailto:VikingCoder@VikingSoftware.com) and Geoff McElhanon (mailto:gmcelhanon@austin.rr.com).
//  Edited by Blake Niemyjski (mailto:bniemyjski@codesmithtools.com).
//
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using CodeSmith.Engine;

using Microsoft.Data.ConnectionUI;

namespace SchemaExplorer
{
    public class OracleSchemaProvider : IDbSchemaProvider, IDbConnectionStringEditor
    {
        private bool? _hasExtendedPropertiesTable;

        public OracleSchemaProvider(){}

        #region IDbConnectionStringEditor Members

        /// <summary>
        /// Connection String
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Connection Editor
        /// </summary>
        public bool EditorAvailable
        {
            get { return true; }
        }

        /// <summary>
        /// Shows the Connection Editor.
        /// </summary>
        /// <param name="currentConnectionString"></param>
        /// <returns></returns>
        public bool ShowEditor(string currentConnectionString)
        {
            var dcd = new DataConnectionDialog();
            dcd.DataSources.Add(Microsoft.Data.ConnectionUI.DataSource.OracleDataSource);
            dcd.SelectedDataSource = Microsoft.Data.ConnectionUI.DataSource.OracleDataSource;
            dcd.SelectedDataProvider = DataProvider.OracleDataProvider;

            try
            {
                dcd.ConnectionString = currentConnectionString;
            }
            catch {}

            var result = DataConnectionDialog.Show(dcd);
            if (result == DialogResult.OK)
                ConnectionString = CleanForOracle(dcd.ConnectionString);

            return result == DialogResult.OK;
        }

        #endregion

        #region IDbSchemaProvider Members

        /// <summary>
        /// Return the Constant name of the SchemaProvider.
        /// </summary>
        public string Name
        {
            get { return "OracleSchemaProvider"; }
        }

        /// <summary>
        /// Returns the name of the SchemaProvider.
        /// </summary>
        public string Description
        {
            get { return "Oracle Schema Provider"; }
        }

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
            var tableList = new List<TableSchema>();

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                const int POS_OWNER = 0;
                const int POS_NAME = 1;
                const int POS_CREATED = 2;

                string sql = String.Format(
                    @"SELECT		owner, object_name, created 
                        FROM		all_objects
                        WHERE		{0}
                                    AND object_type = 'TABLE'
                                    AND object_name NOT LIKE 'BIN$%'
                                    AND NOT object_name = 'CODESMITH_EXTENDED_PROPERTIES'
                        ORDER BY	owner, 
                                    object_name",
                    GetSchemaOwnershipFilter());

                using (var command = new OracleCommand(sql, connection))
                {
                    using (OracleDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            var tableSchema = new TableSchema(database, reader.GetString(POS_NAME),
                                                              reader.GetString(POS_OWNER),
                                                              reader.GetDateTime(POS_CREATED));
                            tableList.Add(tableSchema);
                        }
                    }
                }
            }

            return tableList.ToArray();
        }

        #endregion

        #region public IndexSchema[] GetTableIndexes( string connectionString, TableSchema table )

        /// <summary>
        /// Get all the Indexes for a given Table
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        /// 
        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            string sql = String.Format(
                @"select    idx.owner, idx.uniqueness, con.constraint_type, idx.table_type, idx.INDEX_TYPE, express.COLUMN_EXPRESSION, col.*
                from		all_ind_columns col,
                            all_indexes idx,
                            all_constraints con,
                            all_ind_expressions express
                where		idx.table_owner = '{0}'
                            AND idx.table_name = '{1}'
                            AND idx.owner = col.index_owner
                            AND idx.index_name = col.index_name
                            AND idx.owner = con.owner (+)
                            AND idx.table_name = con.table_name(+)
                            AND NOT idx.table_name = 'CODESMITH_EXTENDED_PROPERTIES'
                            AND idx.index_name = con.CONSTRAINT_NAME(+)
                            AND col.index_owner = express.index_owner (+)
                            AND col.table_name = express.table_name (+)
                            AND col.index_name = express.index_name (+)",
                table.Owner, table.Name);

            // Collections to hold index information
            var indexMemberCollection = new NameValueCollection();
            var isUniqueByIndexName = new Hashtable();
            var isPrimaryByIndexName = new Hashtable();
            var isClusteredByIndexName = new Hashtable();
            var extendedProperties = new List<ExtendedProperty>();

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                using (var command = new OracleCommand(sql, connection))
                {
                    using (OracleDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        //OWNER	    UNIQUENESS	CONSTRAINT_TYPE	TABLE_TYPE	INDEX_TYPE	            COLUMN_EXPRESSION	INDEX_OWNER	INDEX_NAME	TABLE_OWNER	TABLE_NAME  COLUMN_NAME
                        //HR    	NONUNIQUE	(null)	        TABLE	    FUNCTION-BASED NORMAL	TRUNC("Test")	    HR  	    IDX_Test	HR      	BLAH	    SYS_NC00063$

                        int constraintTypeOrdinal = reader.GetOrdinal("CONSTRAINT_TYPE");
                        int columnExpressionOrdinal = reader.GetOrdinal("COLUMN_EXPRESSION");
                        //int tableTypeOrdinal = reader.GetOrdinal( "TABLE_TYPE" );

                        while (reader.Read())
                        {
                            var columnName = (string)reader["COLUMN_NAME"];
                            if (!reader.IsDBNull(columnExpressionOrdinal))
                            {
                                // Region try and do a basic parse of the expression (E.G TRUNC("Column_Name") ).
                                var expression = (string)reader["COLUMN_EXPRESSION"];

                                columnName = (from name in (from c in table.Columns orderby c.Name.Length descending select c.Name)
                                              where expression.Contains(String.Format("\"{0}\"", name))
                                              select name).FirstOrDefault();

                                if (String.IsNullOrEmpty(columnName))
                                {
                                    Trace.WriteLine(String.Format("OracleSchemaProvider: Skipping Index '{0}' as the column expression '{1}' could not be parsed.", reader["INDEX_NAME"], expression));
                                    continue;
                                }

                                extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.ComputedDefinition, expression, DbType.String, PropertyStateEnum.ReadOnly));
                                extendedProperties.Add(new ExtendedProperty("CS_ColumnExpression", expression, DbType.String)); // Added for backwards compatibility.
                            }

                            // Add the column to the collection by index name
                            indexMemberCollection.Add((string)reader["INDEX_NAME"], columnName);

                            // Determine if index is unique
                            bool isUnique = ((string)reader["UNIQUENESS"] == "UNIQUE");
                            isUniqueByIndexName[reader["INDEX_NAME"]] = isUnique;

                            // Determine if index is the primary key index
                            bool isPrimary = (!reader.IsDBNull(constraintTypeOrdinal) && (string)reader["CONSTRAINT_TYPE"] == "P");
                            isPrimaryByIndexName[reader["INDEX_NAME"]] = isPrimary;

                            // Determine if the index is on a TABLE or CLUSTER
                            // NOTE: A Microsoft® SQL Server™ clustered index is not like an Oracle cluster. 
                            // An Oracle cluster is a physical grouping of two or more tables that share the 
                            // same data blocks and use common columns as a cluster key. SQL Server does not 
                            // have a structure that is similar to an Oracle cluster.
                            bool isClustered = ((string)reader["TABLE_TYPE"] == "CLUSTER");
                            isClusteredByIndexName[reader["INDEX_NAME"]] = isClustered;

                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.SystemType, reader["INDEX_TYPE"], DbType.String));
                            extendedProperties.Add(new ExtendedProperty("CS_IndexType", reader["INDEX_TYPE"], DbType.String)); // Added for backwards compatibility.
                        }
                    }
                }
            }

            // Declare an array to hold the index schemas to be returned
            var indexSchemas = new IndexSchema[indexMemberCollection.Count];

            // Were there any index members found?
            if (indexMemberCollection.Count > 0)
            {
                // Iterate through the distinct keys (index names) of the NameValueCollection
                int i = 0;
                foreach (string indexName in indexMemberCollection.AllKeys)
                {
                    var indexSchema = new IndexSchema(
                        table,
                        indexName,
                        (bool)isPrimaryByIndexName[indexName],
                        (bool)isUniqueByIndexName[indexName],
                        (bool)isClusteredByIndexName[indexName],
                        indexMemberCollection.GetValues(indexName),
                        extendedProperties.ToArray());

                    indexSchemas[i++] = indexSchema;
                }
            }

            // Return the array of indexes
            return indexSchemas;
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
            var columnList = new List<ColumnSchema>();

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                const int POS_COLUMN_NAME = 0;
                const int POS_DATA_TYPE = 1;
                //const int POS_data_length	= 2;
                const int POS_DATA_PRECISION = 3;
                const int POS_DATA_SCALE = 4;
                const int POS_NULLABLE = 5;
                //const int POS_COMMENTS		= 6;
                const int POS_DATA_DEFAULT = 7;
                const int POS_LAST_ANALYZED = 8;
                const int POS_Position = 9;

                string sql = String.Format(
                    @"select cols.column_name, 
                             cols.data_type, 
                             cols.data_length,
                             cols.data_precision, 
                             cols.data_scale,
                             cols.nullable,        
                             cmts.comments,
                             cols.DATA_DEFAULT,
                             cols.LAST_ANALYZED,
                             cols.COLUMN_ID
                      from  all_tab_columns cols, 
                            all_col_comments cmts 
                      where 
                            cols.owner = '{0}'
                        and cols.table_name = '{1}'
                        AND cols.column_name NOT LIKE 'BIN$%'
                        and cols.owner = cmts.owner 
                        and cols.table_name = cmts.table_name 
                        and cols.column_name = cmts.column_name
                        order by column_id",
                    table.Owner,
                    table.Name);

                using (var command = new OracleCommand(sql, connection))
                {
                    using (OracleDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            string defaultValue = reader.IsDBNull(POS_DATA_DEFAULT) ? "" : reader.GetString(POS_DATA_DEFAULT);
                            int objectID = reader.GetInt32(POS_Position);
                            DateTime lastAnalyzed = reader.IsDBNull(POS_LAST_ANALYZED) ? DateTime.MinValue : reader.GetDateTime(POS_LAST_ANALYZED);

                            var extendedProperties = new List<ExtendedProperty>();
                            extendedProperties.Add(CreateExtendedPropertyForComments(reader));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, defaultValue, DbType.String, PropertyStateEnum.ReadOnly));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.ObjectID, objectID, DbType.Int32, PropertyStateEnum.ReadOnly));
                            extendedProperties.Add(new ExtendedProperty("CS_LastAnalyzed", lastAnalyzed, DbType.DateTime, PropertyStateEnum.ReadOnly));

                            var cs = new ColumnSchema(
                                table,
                                reader.GetString(POS_COLUMN_NAME),
                                GetDbType(reader),
                                reader.GetString(POS_DATA_TYPE),
                                GetDBTypeLength(reader),
                                reader.IsDBNull(POS_DATA_PRECISION)
                                    ? (byte)0
                                    : Convert.ToByte(reader[POS_DATA_PRECISION]),
                                reader.IsDBNull(POS_DATA_SCALE) ? 0 : Convert.ToInt32(reader[POS_DATA_SCALE]),
                                reader.GetString(POS_NULLABLE) == "Y",
                                extendedProperties.ToArray());

                            columnList.Add(cs);
                        }
                    }
                }
            }

            // Return the array of columns
            return columnList.ToArray();
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
            var tableSchemaList = new List<TableKeySchema>();

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                string sql = String.Format(
                    @"select 
                    cols.CONSTRAINT_NAME,
                    cols.OWNER as table_owner,
                    cols.table_name, 
                    cols.column_name, 
                    cols.position,
                    r_cons.OWNER as related_table_owner,
                    r_cons.table_name related_table_name, 
                    r_cols.column_name related_column_name 
                from
                    all_constraints     cons,
                    all_cons_columns    cols,
                    all_constraints     r_cons,
                    all_cons_columns    r_cols
                where cons.OWNER = '{0}'
                  and (cons.table_name = '{1}' or r_cols.table_name = '{1}')
                  and cons.constraint_type='R'
                  and cols.owner = cons.owner
                  and cols.table_name = cons.table_name   
                  and cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME 
                  and NOT cols.table_name = 'CODESMITH_EXTENDED_PROPERTIES'
                  and r_cols.position = cols.position
                  and r_cols.owner = cons.r_owner 
                  and r_cols.CONSTRAINT_NAME = cons.r_constraint_name 
                  and r_cons.owner = r_cols.owner 
                  and r_cons.table_name = r_cols.table_name 
                  and r_cons.CONSTRAINT_NAME = r_cols.CONSTRAINT_NAME
                  and NOT r_cons.table_name = 'CODESMITH_EXTENDED_PROPERTIES'
                order by cons.CONSTRAINT_NAME, cols.position",
                    table.Owner,
                    table.Name);

                // GKM - Should we change to OracleDataReader implementation?
                var dataAdapter = new OracleDataAdapter(sql, connection);

                var dataSet = new DataSet();
                using (dataSet)
                {
                    dataAdapter.Fill(dataSet);

                    string lastConstraint = String.Empty;
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        if (lastConstraint != row["CONSTRAINT_NAME"].ToString())
                        {
                            lastConstraint = row["CONSTRAINT_NAME"].ToString();

                            string pkTable = row["related_table_name"].ToString();
                            string pkTableOwner = row["related_table_owner"].ToString();
                            string tableName = row["table_name"].ToString();
                            string tableOwner = row["table_owner"].ToString();

                            // Skip relationships that are contained outside of this schema..
                            // http://community.codesmithtools.com/Template_Frameworks/f/67/p/10503/43791.aspx
                            if (OracleConfiguration.Instance.ShowMySchemaOnly && !pkTableOwner.Equals(tableOwner))
                                continue;

                            DataRow[] distinctColumns = dataSet.Tables[0].Select(
                                String.Format("CONSTRAINT_NAME = '{0}'", lastConstraint));

                            var fkMemberCols = new List<string>();
                            var pkMemberCols = new List<string>();

                            foreach (DataRow rdc in distinctColumns)
                            {
                                fkMemberCols.Add(rdc["column_name"].ToString());
                                pkMemberCols.Add(rdc["related_column_name"].ToString());
                            }

                            var tks = new TableKeySchema(table.Database, lastConstraint, fkMemberCols.ToArray(), tableOwner, tableName, pkMemberCols.ToArray(), pkTableOwner, pkTable);

                            // Don't forget to add it to our result.
                            tableSchemaList.Add(tks);
                        }
                    }
                }
            }

            return tableSchemaList.ToArray();
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

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                string sql = String.Format(
                    @"select 
                        cols.CONSTRAINT_NAME, 
                        cols.column_name, 
                        cols.position 
                    from
                        all_constraints     cons,
                        all_cons_columns    cols
                    where 
                        cons.OWNER = '{0}'
                        and cons.table_name = '{1}'
                        and cons.constraint_type='P'
                        and cols.owner = cons.owner
                        and cols.table_name = cons.table_name   
                        AND NOT cols.table_name = 'CODESMITH_EXTENDED_PROPERTIES'
                        and cols.CONSTRAINT_NAME = cons.CONSTRAINT_NAME 
                    order by cons.CONSTRAINT_NAME, cols.position",
                    table.Owner,
                    table.Name);

                // GKM - Should we change to OracleDataReader implementation?
                var dataAdapter = new OracleDataAdapter(sql, connection);

                using (var dataSet = new DataSet())
                {
                    int rowCount = dataAdapter.Fill(dataSet);

                    if (rowCount > 0)
                    {
                        var memberCols = new List<string>();

                        foreach (DataRow row in dataSet.Tables[0].Rows)
                        {
                            memberCols.Add(row["column_name"].ToString());
                        }

                        primaryKeySchema = new PrimaryKeySchema(table, dataSet.Tables[0].Rows[0]["CONSTRAINT_NAME"].ToString(), memberCols.ToArray());
                    }
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
            using (var connection = new OracleConnection(connectionString))
            {
                // Create and fill the data table
                var dataTable = new DataTable(table.Name);

                string sql = String.Format("SELECT * FROM {0}.{1}", table.Owner, table.Name);

                using (var oracleDataAdapter = new OracleDataAdapter(sql, connection))
                {
                    oracleDataAdapter.Fill(dataTable);
                }

                return dataTable;
            }
        }

        #endregion

        #endregion

        #region Extended Properties

        #region public void SetExtendedProperties( string connectionString, SchemaObjectBase schemaObject )

        /// <summary>
        /// Sets an extended property
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="schemaObject"></param>
        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            if (String.IsNullOrEmpty(connectionString) || schemaObject == null)
                return;

            //CreateExtended Properties Table
            if (OracleConfiguration.Instance.AutoCreateExtendedPropertiesTable)
                CreateExtendedPropertiesTable(connectionString);
            else if (!HasExtendedPropertiesTable(connectionString))
                return;

            // Get the schema object name and type name to be used to retrieve the extended property.
            string containerObjectOwner;
            string objectName = schemaObject.Name;
            string schemaObjectTypeName = schemaObject.GetType().Name;

            switch (schemaObjectTypeName)
            {
                case "ColumnSchema":
                    containerObjectOwner = ((ColumnSchema)schemaObject).Table.Owner;

                    var column = (ColumnSchema)schemaObject;
                    objectName = String.Format("{0}.{1}", column.Table.Name, column.Name);
                    break;

                case "ViewColumnSchema":
                    containerObjectOwner = ((ViewColumnSchema)schemaObject).View.Owner;

                    var viewColumn = (ViewColumnSchema)schemaObject;
                    objectName = String.Format("{0}.{1}", viewColumn.View.Name, viewColumn.Name);
                    break;

                case "CommandSchema":
                    containerObjectOwner = ((CommandSchema)schemaObject).Owner;
                    break;

                case "DatabaseSchema":
                    objectName = "Database"; // TODO - Use user name here instead?
                    containerObjectOwner = "Database";
                    break;

                case "IndexSchema":
                    containerObjectOwner = ((IndexSchema)schemaObject).Table.Owner;
                    // SchemaExplorer ignores index ownership, use the table's owner instead.
                    break;

                case "ParameterSchema":
                    containerObjectOwner = ((ParameterSchema)schemaObject).Command.Owner;
                    break;

                case "PrimaryKeySchema":
                    containerObjectOwner = ((PrimaryKeySchema)schemaObject).Table.Owner;
                    // SchemaExplorer ignores CONSTRAINT ownership, use the table's owner instead.
                    break;

                case "TableKeySchema":
                    containerObjectOwner = ((TableKeySchema)schemaObject).ForeignKeyTable.Owner;
                    // SchemaExplorer ignores CONSTRAINT ownership, use the foreign key table owner instead.
                    break;

                case "TableSchema":
                    containerObjectOwner = ((TableSchema)schemaObject).Owner;
                    break;

                case "ViewSchema":
                    containerObjectOwner = ((ViewSchema)schemaObject).Owner;
                    break;

                case "MemberColumnSchema":
                    containerObjectOwner = ((MemberColumnSchema)schemaObject).Table.Owner;
                    var memberColumn = (MemberColumnSchema)schemaObject;
                    objectName = String.Format("{0}.{1}", memberColumn.Table.Name, memberColumn.Name);
                    break;

                case "CommandResultColumnSchema":
                    containerObjectOwner = ((CommandResultColumnSchema)schemaObject).Command.Owner;
                    var commandResultColumn = (CommandResultColumnSchema)schemaObject;
                    objectName = String.Format("{0}.{1}", commandResultColumn.Command.Name, commandResultColumn.Name);
                    break;

                default:
                    // Don't recognize the SchemaExplorer type... need to throw an exception
                    throw new NotSupportedException(String.Format("Unexpected SchemaExplorer type '{0}' encountered.  Extended properties are not yet supported for this type.", schemaObjectTypeName));
            }

            bool needRefresh = false;

            // Define SQL to pull appropriate extended properties from the table
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();

                foreach (ExtendedProperty extendedProperty in schemaObject.ExtendedProperties)
                {
                    string command = null;
                    string name = extendedProperty.Name;

                    if (name == ExtendedPropertyNames.Description && !schemaObject.ExtendedProperties.Contains(name))
                        extendedProperty.PropertyState = PropertyStateEnum.New;

                    switch (extendedProperty.PropertyState)
                    {
                        case PropertyStateEnum.New:
                            command = String.Format(
                                @"INSERT INTO {0}codesmith_extended_properties
                                        (container_object_owner,object_name,codesmith_schema_type,property_name,property_value,clr_type)
                                    VALUES
                                        ('{1}','{2}','{3}','{4}','{5}','{6}')",
                                GetExtendedPropertiesTableOwnerPrefix(),
                                containerObjectOwner.ToUpper(),
                                objectName.ToUpper(),
                                schemaObjectTypeName,
                                extendedProperty.Name,
                                extendedProperty.Value,
                                extendedProperty.DataType);

                            needRefresh = true;
                            break;

                        case PropertyStateEnum.Dirty:
                            command = String.Format(
                                @"UPDATE {0}codesmith_extended_properties
                                        SET property_value = '{5}'
                                    WHERE	container_object_owner = '{1}'
                                        AND object_name = '{2}'
                                        AND codesmith_schema_type = '{3}'
                                        AND property_name = '{4}'",
                                GetExtendedPropertiesTableOwnerPrefix(),
                                containerObjectOwner.ToUpper(),
                                objectName.ToUpper(),
                                schemaObjectTypeName,
                                extendedProperty.Name,
                                extendedProperty.Value);

                            needRefresh = true;
                            break;

                        case PropertyStateEnum.Deleted:
                            command = String.Format(
                                @"DELETE FROM {0}codesmith_extended_properties
                                    WHERE   container_object_owner = '{1}'
                                        AND object_name = '{2}'
                                        AND codesmith_schema_type = '{3}'
                                        AND property_name = '{4}'",
                                GetExtendedPropertiesTableOwnerPrefix(),
                                containerObjectOwner.ToUpper(),
                                objectName.ToUpper(),
                                schemaObjectTypeName,
                                extendedProperty.Name);

                            needRefresh = true;
                            break;

                        case PropertyStateEnum.ReadOnly:
                        case PropertyStateEnum.Unmodified:
                            break;

                        default:
                            throw new ArgumentException(String.Format("Type: {0} is not supported at this time.", extendedProperty.PropertyState));
                    }

                    // command will be null if PropertyStateEnum.Unmodified = true;
                    if (!String.IsNullOrEmpty(command))
                    {
                        using (var oracleCommand = new OracleCommand(command, connection))
                        {
                            oracleCommand.ExecuteScalar();
                        }
                    }
                } // for each

                if (needRefresh)
                    schemaObject.Refresh();
            }
        }

        #endregion

        #region private void CreateExtendedPropertiesTable( string connectionString )

        /// <summary>
        /// Creates the Extended Properties Table
        /// </summary>
        /// <param name="connectionString"></param>
        private void CreateExtendedPropertiesTable(string connectionString)
        {
            if (HasExtendedPropertiesTable(connectionString))
                return;

            string sqlCreate = String.Format(
                @"CREATE TABLE {0}codesmith_extended_properties
                (
                    container_object_owner     VARCHAR2(50) NOT NULL,
                    object_name                VARCHAR2(61) NOT NULL,
                    codesmith_schema_type      VARCHAR2(200) NOT NULL,
                    property_name              VARCHAR2(75) NOT NULL,
                    property_value             VARCHAR2(4000),
                    clr_type                   VARCHAR2(50) NOT NULL
                )", GetExtendedPropertiesTableOwnerPrefix());

            string sqlAddPK = String.Format(
                @"ALTER TABLE {0}codesmith_extended_properties
                    ADD CONSTRAINT	pkcodesmithextendedproperties PRIMARY KEY
                    (
                        container_object_owner,
                        object_name,
                        codesmith_schema_type,
                        property_name
                    )", GetExtendedPropertiesTableOwnerPrefix());

            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();

                try
                {
                    using (var command = new OracleCommand(sqlCreate, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Wrap the exception in our own, and rethrow
                    throw new ApplicationException(String.Format("Unable to automatically create extended properties because the following exception occurred:\n\n{0}", ex.Message), ex);
                }

                try
                {
                    using (var command = new OracleCommand(sqlAddPK, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Wrap the exception in our own, and rethrow
                    throw new ApplicationException(String.Format("Unable to add primary to extended properties table during auto-creation because the following exception occurred:\n\n{0}", ex.Message), ex);
                }

                // The Property has been successfully created!
                _hasExtendedPropertiesTable = true;

                // Close the connection.
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        #endregion

        #region private bool HasExtendedPropertiesTable(string connectionString)

        private bool HasExtendedPropertiesTable(string connectionString)
        {
            if (_hasExtendedPropertiesTable.HasValue)
                return _hasExtendedPropertiesTable.Value;

            string sql = String.Format(
                "SELECT count(9) FROM dual WHERE EXISTS (" +
                "SELECT * " +
                "FROM all_objects " +
                "WHERE object_type IN ('TABLE','VIEW') " +
                "AND object_name = 'CODESMITH_EXTENDED_PROPERTIES' " +
                "AND owner = {0} )",
                (String.IsNullOrEmpty(OracleConfiguration.Instance.ExtendedPropertiesTableSchema)) ? "user" : String.Format("'{0}'", OracleConfiguration.Instance.ExtendedPropertiesTableSchema));

            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();

                try
                {
                    using (var command = new OracleCommand(sql, connection))
                    {
                        var Exists = (OracleNumber)command.ExecuteOracleScalar();
                        _hasExtendedPropertiesTable = Exists.Value == 1;
                    }
                }
                catch (Exception ex)
                {
                    // Wrap the exception in our own, and rethrow
                    throw new ApplicationException(String.Format("Unable to check to see if to extended properties table exists because the following exception occurred:\n\n{0}", ex.Message), ex);
                }

                // Close the connection.
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return _hasExtendedPropertiesTable.Value;
        }

        #endregion

        #region public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)

        /// <summary>
        /// Returns an array of extended properties
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="schemaObject"></param>
        /// <returns></returns>
        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            if (String.IsNullOrEmpty(connectionString) || schemaObject == null)
                return new ExtendedProperty[0];

            //CreateExtended Properties Table
            if (OracleConfiguration.Instance.AutoCreateExtendedPropertiesTable)
                CreateExtendedPropertiesTable(connectionString);
            else if (!HasExtendedPropertiesTable(connectionString))
                return new ExtendedProperty[0];

            // Get the schema object name and type name to be used to retrieve the extended property.
            string containerObjectOwner;
            string objectName = schemaObject.Name;
            string schemaObjectTypeName = schemaObject.GetType().Name;

            switch (schemaObjectTypeName)
            {
                case "ColumnSchema":
                    containerObjectOwner = ((ColumnSchema)schemaObject).Table.Owner;

                    var column = (ColumnSchema)schemaObject;
                    objectName = String.Format("{0}.{1}", column.Table.Name, column.Name);
                    break;

                case "ViewColumnSchema":
                    containerObjectOwner = ((ViewColumnSchema)schemaObject).View.Owner;

                    var viewColumn = (ViewColumnSchema)schemaObject;
                    objectName = String.Format("{0}.{1}", viewColumn.View.Name, viewColumn.Name);
                    break;

                case "CommandSchema":
                    containerObjectOwner = ((CommandSchema)schemaObject).Owner;
                    break;

                case "DatabaseSchema":
                    objectName = "Database"; // TODO - Use user name here instead?
                    containerObjectOwner = "Database";
                    break;

                case "IndexSchema":
                    containerObjectOwner = ((IndexSchema)schemaObject).Table.Owner;
                    // SchemaExplorer ignores index ownership, use the table's owner instead.
                    break;

                case "ParameterSchema":
                    containerObjectOwner = ((ParameterSchema)schemaObject).Command.Owner;
                    break;

                case "PrimaryKeySchema":
                    containerObjectOwner = ((PrimaryKeySchema)schemaObject).Table.Owner;
                    // SchemaExplorer ignores CONSTRAINT ownership, use the table's owner instead.
                    break;

                case "TableKeySchema":
                    containerObjectOwner = ((TableKeySchema)schemaObject).ForeignKeyTable.Owner;
                    // SchemaExplorer ignores CONSTRAINT ownership, use the foreign key table owner instead.
                    break;

                case "TableSchema":
                    containerObjectOwner = ((TableSchema)schemaObject).Owner;
                    break;

                case "ViewSchema":
                    containerObjectOwner = ((ViewSchema)schemaObject).Owner;
                    break;

                case "MemberColumnSchema":
                    containerObjectOwner = ((MemberColumnSchema)schemaObject).Table.Owner;
                    var memberColumn = (MemberColumnSchema)schemaObject;
                    objectName = String.Format("{0}.{1}", memberColumn.Table.Name, memberColumn.Name);
                    break;

                case "CommandResultColumnSchema":
                    containerObjectOwner = ((CommandResultColumnSchema)schemaObject).Command.Owner;
                    var commandResultColumn = (CommandResultColumnSchema)schemaObject;
                    objectName = String.Format("{0}.{1}", commandResultColumn.Command.Name, commandResultColumn.Name);
                    break;

                default:
                    // Don't recognize the SchemaExplorer type... need to throw an exception
                    throw new NotSupportedException(String.Format("Unexpected SchemaExplorer type '{0}' encountered.  Extended properties are not yet supported for this type.", schemaObjectTypeName));
            }

            // Define SQL to pull appropriate extended properties from the table
            string sql = String.Format(
                @"SELECT	property_name, property_value, clr_type
                    FROM	{0}codesmith_extended_properties
                    WHERE	container_object_owner = '{1}'
                            AND object_name = '{2}'
                            AND codesmith_schema_type = '{3}'",
                GetExtendedPropertiesTableOwnerPrefix(),
                containerObjectOwner.ToUpper(),
                objectName.ToUpper(),
                schemaObjectTypeName);

            const int POS_PROPERTY_NAME = 0;
            const int POS_PROPERTY_VALUE = 1;
            const int POS_CLR_TYPE = 2;

            // Instantiate an List to hold the extended properties temporarily
            var extendedProperties = new List<ExtendedProperty>();

            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();

                try
                {
                    using (var command = new OracleCommand(sql, connection))
                    {
                        using (OracleDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (reader.Read())
                            {
                                //NOTE: AnsiString will always be converted to System.String and displayed as String. Even though it is stored in the database as AnsiString.

                                // Pull all the values into variables
                                string propertyName = reader.GetString(POS_PROPERTY_NAME);
                                string propertyValueText = reader.GetString(POS_PROPERTY_VALUE);
                                string dbType = DbTypeToCSharp[reader.GetString(POS_CLR_TYPE)];
                                string systemType = CSharpToSystemType[dbType];

                                // Get the System type for the value
                                Type clrType = Type.GetType(systemType);

                                // Convert the string representation of the property to the appropriate CLR type
                                if (clrType != null)
                                {
                                    object propertyValue = Convert.ChangeType(propertyValueText, clrType);

                                    // Construct and add the extended property 
                                    extendedProperties.Add(new ExtendedProperty(propertyName, propertyValue, GetDbTypeFromSystemType(clrType)));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Check for the Oracle exception that indicates that the table isn't there
                    if (ex.Message.IndexOf("ORA-00942") >= 0)
                    {
                        return new ExtendedProperty[0];
                    }

                    throw;
                }

                return extendedProperties.ToArray();
            }
        }

        #endregion

        #region private static ExtendedProperty CreateExtendedPropertyForComments(IDataRecord reader)

        /// <summary>
        /// Creates Extended Properties for Comments.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static ExtendedProperty CreateExtendedPropertyForComments(IDataRecord reader)
        {
            int ordinal = reader.GetOrdinal("COMMENTS");

            if (ordinal >= 0)
            {
                // Get the comments, or an empty string
                string comments = reader.IsDBNull(ordinal) ? String.Empty : reader.GetString(ordinal);

                return CreateExtendedPropertyForComments(comments);
            }

            throw new ApplicationException("There was no COMMENTS column in the OracleDataReader provided.");
        }

        #endregion

        #region private static ExtendedProperty CreateExtendedPropertyForComments(string comment)

        /// <summary>
        /// Creates a new Extended Property For Comments.
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        private static ExtendedProperty CreateExtendedPropertyForComments(string comment)
        {
            return new ExtendedProperty(ExtendedPropertyNames.Description, comment, DbType.String);
        }

        #endregion

        #endregion

        #region View Retrieval Methods

        #region public ViewSchema[] GetViews( string connectionString, DatabaseSchema database )

        /// <summary>
        /// Return array of Views
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            // Build SQL to get the views
            const int POS_OWNER_NAME = 0;
            const int POS_VIEW_NAME = 1;
            const int POS_CREATED = 2;

            string sql = String.Format(
                @"select 
                    v.owner, v.view_name, o.created
                from all_views   v,
                    all_objects o 
                where v.view_name = o.object_name 
                and o.object_type = 'VIEW' 
                and v.view_name NOT LIKE 'BIN$%'
                and {0}
                order by v.owner, v.view_name",
                GetSchemaOwnershipFilter("v"));

            // Create an array list to hold the values temporarily
            var viewList = new List<ViewSchema>();

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                using (var command = new OracleCommand(sql, connection))
                {
                    using (OracleDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            viewList.Add(new ViewSchema(database, reader.GetString(POS_VIEW_NAME),
                                                          reader.GetString(POS_OWNER_NAME),
                                                          reader.GetDateTime(POS_CREATED)));
                        }
                    }
                }
            }

            return viewList.ToArray();
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

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                // Get SQL to retrieve source for the view
                string sql = String.Format(
                    @"select text from all_views where owner = '{0}' and view_name = '{1}'", view.Owner, view.Name);

                // Get the view text
                using (var command = new OracleCommand(sql, connection))
                {
                    viewText = (string)command.ExecuteScalar();
                }
            }

            return viewText;
        }

        #endregion

        #region public ViewColumnSchema[] GetViewColumns( string connectionString, ViewSchema view )

        /// <summary>
        /// Return all the columns for a view...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            var columnList = new List<ViewColumnSchema>();

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                const int POS_COLUMN_NAME = 0;
                const int POS_DATA_TYPE = 1;
                //const int POS_data_length	= 2;
                const int POS_DATA_PRECISION = 3;
                const int POS_DATA_SCALE = 4;
                const int POS_NULLABLE = 5;
                //const int POS_COMMENTS		= 6;
                const int POS_DATA_DEFAULT = 7;
                const int POS_LAST_ANALYZED = 8;
                const int POS_Position = 9;

                string sql = String.Format(
                    @"select cols.column_name, 
                             cols.data_type, 
                             cols.data_length,
                             cols.data_precision, 
                             cols.data_scale,
                             cols.nullable,        
                             cmts.comments,
                             cols.DATA_DEFAULT,
                             cols.LAST_ANALYZED,
                             cols.COLUMN_ID
                      from  all_tab_columns cols, 
                            all_col_comments cmts 
                      where 
                            cols.owner = '{0}'
                        and cols.table_name = '{1}'
                        and cols.column_name NOT LIKE 'BIN$%'
                        and cols.owner = cmts.owner 
                        and cols.table_name = cmts.table_name 
                        and cols.column_name = cmts.column_name
                        order by column_id",
                    view.Owner,
                    view.Name);


                using (var command = new OracleCommand(sql, connection))
                {
                    using (OracleDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            string defaultValue = reader.IsDBNull(POS_DATA_DEFAULT) ? "" : reader.GetString(POS_DATA_DEFAULT);
                            int objectID = reader.GetInt32(POS_Position);
                            DateTime lastAnalyzed = reader.IsDBNull(POS_LAST_ANALYZED) ? DateTime.MinValue : reader.GetDateTime(POS_LAST_ANALYZED);

                            var extendedProperties = new List<ExtendedProperty>();
                            extendedProperties.Add(CreateExtendedPropertyForComments(reader));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, defaultValue, DbType.String, PropertyStateEnum.ReadOnly));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.ObjectID, objectID, DbType.Int32, PropertyStateEnum.ReadOnly));
                            extendedProperties.Add(new ExtendedProperty("CS_LastAnalyzed", lastAnalyzed, DbType.DateTime, PropertyStateEnum.ReadOnly));

                            var vcs = new ViewColumnSchema(
                                view,
                                reader.GetString(POS_COLUMN_NAME),
                                GetDbType(reader),
                                reader.GetString(POS_DATA_TYPE),
                                GetDBTypeLength(reader),
                                reader.IsDBNull(POS_DATA_PRECISION)
                                    ? (byte)0
                                    : Convert.ToByte(reader[POS_DATA_PRECISION]),
                                reader.IsDBNull(POS_DATA_SCALE) ? 0 : Convert.ToInt32(reader[POS_DATA_SCALE]),
                                reader.GetString(POS_NULLABLE) == "Y",
                                extendedProperties.ToArray());

                            columnList.Add(vcs);
                        }
                    }
                }
            }

            // Return the array of columns
            return columnList.ToArray();
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
            // Open a connection
            using (var connection = new OracleConnection(connectionString))
            {
                // Create and fill the data table
                var dataTable = new DataTable(view.Name);

                string sql = String.Format("SELECT * FROM {0}.{1}", view.Owner, view.Name);

                using (var dataAdapter = new OracleDataAdapter(sql, connection))
                {
                    dataAdapter.Fill(dataTable);
                }

                return dataTable;
            }
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
            var commandList = new List<CommandSchema>();

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                const int POS_OWNER = 0;
                const int POS_PACKAGE = 1;
                const int POS_METHOD = 2;
                const int POS_OVERLOAD = 3;
                const int POS_TYPE = 4;
                const int POS_CREATED = 5;
                const int POS_STATUS = 6;
                const int POS_OBJ_ID = 7;

                string sql = String.Format(
                    @"select methods.owner, 
                            methods.package_name, 
                            methods.object_name, 
                            methods.overload,
                            ao.object_type,
                            ao.created,
                            ao.status,
                            ao.object_id
                        from
                        (select distinct owner, package_name, object_name, overload, object_id from ALL_ARGUMENTS 
                            where {0}
                            ) methods,
                            all_objects ao
                        where ao.object_id = methods.object_id and methods.object_name NOT LIKE 'BIN$%'    
                        order by methods.owner, methods.package_name, methods.object_name",
                    GetSchemaOwnershipFilter(null));

                using (var command = new OracleCommand(sql, connection))
                {
                    using (OracleDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            var extendedProperties = new ExtendedPropertyCollection();
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.SystemType, reader.GetString(POS_TYPE), DbType.String));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.ObjectType, reader.GetString(POS_TYPE), DbType.String));  // Added for backwards compatibility.
                            extendedProperties.Add(new ExtendedProperty("CS_Status", reader.GetString(POS_STATUS), DbType.String));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.ObjectID, reader.GetInt32(POS_OBJ_ID), DbType.Int32));

                            Int32 overload = -1;
                            if (reader[POS_OVERLOAD].ToString().Length > 0)
                            {
                                overload = Int32.Parse(reader[POS_OVERLOAD].ToString());
                            }
                            extendedProperties.Add(new ExtendedProperty("CS_OverloadNumber", overload, DbType.Int32));

                            //if ("PACKAGE"==reader.GetString( POS_TYPE))
                            //{
                            //    name = String.Format( "{0}.{1}",reader.GetString(POS_PACKAGE) , reader.GetString( POS_METHOD ));
                            //}
                            //else
                            //{
                            //    name = reader.GetString( POS_METHOD );
                            //}

                            string owner;
                            var isPackage = "PACKAGE" == reader.GetString(POS_TYPE);
                            if (isPackage)
                            {
                                // Owner needs to be in the form of schema.package.procedure.
                                owner = String.Format("{0}.{1}", reader.GetString(POS_OWNER), reader.GetString(POS_PACKAGE));
                            }
                            else
                            {
                                // Owner needs to be in the form of schema.procedure.
                                owner = reader.GetString(POS_OWNER);
                            }

                            string name = reader.GetString(POS_METHOD);

                            // Save the name and full name (minus the "hack" to support overloads) to the extended properties
                            extendedProperties.Add(new ExtendedProperty("CS_Name", name, DbType.String));
                            extendedProperties.Add(new ExtendedProperty("CS_FullName", String.Format("{0}.{1}", owner, name), DbType.String));

                            // Tack on overload # into the name as a means of supporting overloads in CodeSmith.
                            // Template writers will need to use the CS_Name property to get just the command name value,
                            // but this at least allows code to be generated that makes use of this great Oracle feature.
                            if (overload >= 0)
                            {
                                // GKM - Actually > 0 would work because first overload is given ID of 1, second is 2, and so on.
                                name += String.Format("[{0}]", overload);
                            }

                            var commandSchema = new CommandSchema(database, name, owner, reader.GetDateTime(POS_CREATED), extendedProperties.ToArray());

                            commandList.Add(commandSchema);
                        }
                    }
                }
            }

            // Return the array of commands
            return commandList.ToArray();
        }

        #endregion

        #region public ParameterSchema[] GetCommandParameters( string connectionString, CommandSchema command )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            var paramList = new List<ParameterSchema>();

            var object_id = (Int32)command.ExtendedProperties[ExtendedPropertyNames.ObjectID].Value;
            var overload = (Int32)command.ExtendedProperties["CS_OverloadNumber"].Value;

            //string packageName = String.Empty;
            var commandName = (string)command.ExtendedProperties["CS_Name"].Value;

            int posDot = commandName.IndexOf('.');
            string methodName = posDot > 0 ? commandName.Substring(posDot + 1) : commandName;

            using (var connection = new OracleConnection(connectionString))
            {
                // Open a connection
                connection.Open();

                const int POS_ARGUMENT_NAME = 0;
                const int POS_Position = 1;
                const int POS_DATA_TYPE = 4;
                const int POS_IN_OUT = 5;
                const int POS_DATA_PRECISION = 7;
                const int POS_DATA_SCALE = 8;
                const int DEFAULT_VALUE = 9;
                const int POS_TYPE_NAME = 10;

                string sql = String.Format(
                    @"select 
                        ARGUMENT_NAME, 
                        POSITION, 
                        SEQUENCE, 
                        DATA_LEVEL, 
                        DATA_TYPE, 
                        IN_OUT, 
                        DATA_LENGTH, 
                        DATA_PRECISION, 
                        DATA_SCALE,
                        DEFAULT_VALUE,
                        TYPE_NAME  
                    from ALL_ARGUMENTS 
                    where object_ID={0}
                    and object_name = '{1}'
                    and {2}
                    order by position",
                    object_id,
                    methodName,
                    overload > 0 ? String.Format("overload = {0}", overload) : "overload is null");

                using (var oracleCommand = new OracleCommand(sql, connection))
                {
                    using (OracleDataReader reader = oracleCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.IsDBNull(POS_DATA_TYPE))
                                continue;

                            byte precision = 0;
                            if (!reader.IsDBNull(POS_DATA_PRECISION))
                            {
                                precision = Convert.ToByte(reader[POS_DATA_PRECISION]);
                            }

                            Int32 scale = 0;
                            if (!reader.IsDBNull(POS_DATA_SCALE))
                            {
                                scale = Convert.ToInt32(reader[POS_DATA_SCALE]);
                            }

                            string typeName = String.Empty;
                            if (!reader.IsDBNull(POS_TYPE_NAME))
                            {
                                typeName = Convert.ToString(reader[POS_TYPE_NAME]);
                            }

                            string argName = reader.IsDBNull(POS_ARGUMENT_NAME) ? "RETURN" : reader.GetString(POS_ARGUMENT_NAME);

                            string defaultValue = reader.IsDBNull(DEFAULT_VALUE) ? "" : reader.GetString(DEFAULT_VALUE);
                            int parameterID = reader.GetInt32(POS_Position);

                            var extendedProperties = new List<ExtendedProperty>();
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, defaultValue, DbType.String, PropertyStateEnum.ReadOnly));
                            extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.ParameterID, parameterID, DbType.Int32, PropertyStateEnum.ReadOnly));

                            if (!String.IsNullOrEmpty(typeName))
                            {
                                extendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.UserDefinedType, typeName, DbType.String, PropertyStateEnum.ReadOnly));
                            }

                            var parameterSchema = new ParameterSchema(
                                command,
                                argName,
                                GetParameterDirection(reader.IsDBNull(POS_ARGUMENT_NAME), reader.GetString(POS_IN_OUT)),
                                GetDbType(reader),
                                reader.GetString(POS_DATA_TYPE),
                                GetDBTypeLength(reader),
                                precision,
                                scale,
                                true,
                                extendedProperties.ToArray());

                            paramList.Add(parameterSchema);
                        }
                    }
                }
            }

            return paramList.ToArray();
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
            // Make sure the user has explicitly allowed this operation to proceed...
            if (!OracleConfiguration.Instance.AllowGetCommandResultSchemas)
                throw new InvalidOperationException("The Oracle Schema Provider supports reading column schema information. Please set AllowGetCommandResultSchemas to true in the Generator Options -> Schema Provider -> Oracle page as its usage can change the state of the server.");

            // Initialize array to size 0 for now
            var commandResultSchemas = new CommandResultSchema[0];

            // Keep a count as to how many results are expected
            int refCursorCount = 0;

            using (var connection = new OracleConnection(connectionString))
            {
                // Open the connection
                connection.Open();

                using (var oracleCommand = new OracleCommand((string)command.ExtendedProperties["CS_FullName"].Value, connection))
                {
                    oracleCommand.CommandType = CommandType.StoredProcedure;

                    var resultSchemaNames = new List<string>();

                    // Set up the parameters will null values
                    foreach (ParameterSchema parameterSchema in command.Parameters)
                    {
                        var parameter = new OracleParameter { ParameterName = parameterSchema.Name, DbType = parameterSchema.DataType, Direction = parameterSchema.Direction };

                        // If the parameter is not a REF CURSOR
                        if (parameter.DbType != DbType.Object || parameterSchema.NativeType != "REF CURSOR")
                        {
                            // Assign a null value
                            parameter.Value = DBNull.Value;
                        }
                        else
                        {
                            // Keep track of the ref cursor variable names to use for naming the resultset
                            resultSchemaNames.Add(parameterSchema.Name);

                            // Set the ref cursor type
                            parameter.OracleType = OracleType.Cursor;

                            //HACK: An exception will occur if the parameter is an Input/Output directional state ( http://forums.asp.net/t/1114447.aspx ).
                            if (parameter.Direction == ParameterDirection.InputOutput)
                            {
                                parameter.Direction = ParameterDirection.Output;
                            }

                            // Make note as to how many refcursor parms we have
                            refCursorCount++;
                        }

                        // Add the parameter to the command for execution
                        oracleCommand.Parameters.Add(parameter);
                    }

                    // Will there be any results returned?
                    if (refCursorCount > 0)
                    {
                        // Wrap the operation in a transaction to prevent as much change to the server as possible
                        using (OracleTransaction transaction = connection.BeginTransaction())
                        {
                            // Assign the transaction to the command
                            oracleCommand.Transaction = transaction;

                            // Ask for reader, but the schema only.
                            using (OracleDataReader reader = oracleCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.CloseConnection))
                            {
                                int refCursorIndex = 0;

                                // Create an array to hold information about all the resultsets
                                commandResultSchemas = new CommandResultSchema[refCursorCount];

                                do
                                {
                                    // Get schema info from reader
                                    DataTable schemaTable = reader.GetSchemaTable();

                                    // Create an array of result column schemas to store column information
                                    if (schemaTable != null)
                                    {
                                        var resultColumnSchemas = new CommandResultColumnSchema[schemaTable.Rows.Count];
                                        int colIndex = 0;

                                        // Iterate through all rows in the schema table
                                        foreach (DataRow row in schemaTable.Rows)
                                        {
                                            string nativeType;

                                            // TODO: This needs work because we don't get Base Table/Column information.  Native type information
                                            // would have to be inferred from the DbType, which wouldn't be accurate.
                                            try
                                            {
                                                TableSchema tableSchema = command.Database.Tables[(string)row["BaseSchemaName"], (string)row["BaseTableName"]];
                                                nativeType = tableSchema.Columns[(string)row["BaseColumnName"]].NativeType;
                                            }
                                            catch
                                            {
                                                nativeType = "--UNKNOWN--";
                                            }

                                            var resultColumnSchema =
                                                new CommandResultColumnSchema(
                                                    command,
                                                    (string)row["ColumnName"],
                                                    GetDbTypeFromSystemType((Type)row["DataType"]),
                                                    nativeType,
                                                    Convert.ToInt32(row["COLUMNSIZE"]),
                                                    Convert.ToByte(row["NUMERICPRECISION"]),
                                                    Convert.ToInt32(row["NUMERICSCALE"]),
                                                    (bool)row["AllowDBNull"]);

                                            // Assign the result column schema information to the array
                                            resultColumnSchemas[colIndex++] = resultColumnSchema;
                                        }

                                        // Construct a result schema for the results just processed
                                        var commandResultSchema = new CommandResultSchema(command,
                                                                                           resultSchemaNames[refCursorIndex],
                                                                                           resultColumnSchemas);

                                        // Assign the result schema for this resultset to the array
                                        commandResultSchemas[refCursorIndex++] = commandResultSchema;
                                    }
                                } while (reader.NextResult());
                            }
                        }
                    }
                }

                // Return the array of command results
                return commandResultSchemas;
            }
        }

        #endregion

        #region public string GetCommandText( string connectionString, CommandSchema command )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public string GetCommandText(string connectionString, CommandSchema command)
        {
            string sql;

            // Reported by Campos, Luis
            //http://www.exforsys.com/tutorials/oracle-9i/oracle-packages.html
            //http://www.exforsys.com/tutorials/oracle-9i/oracle-packages/1.html
            //http://forums.oracle.com/forums/thread.jspa?threadID=1080023&tstart=2&messageID=4334575
            if (command.Name.IndexOf(".") >= 0)
            {
                //TODO: parse the package stored procedure text from the package source.
                //package member
                string[] packageAndProcNames = command.Name.Split(new char[] { '.' }, 2);
                sql = String.Format(@"select TEXT from user_source where type = 'PACKAGE' and name = '{0}' order by line", packageAndProcNames[0]);
            }
            else
            {
                sql = String.Format(@"select TEXT from user_source where type IN ('FUNCTION', 'PROCEDURE') and name = '{0}' order by LINE", command.Name);
            }

            var source = new StringBuilder();

            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (var oracleCommand = new OracleCommand(sql, connection))
                {
                    using (OracleDataReader reader = oracleCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            source.Append(reader["TEXT"].ToString().Trim() + Environment.NewLine);
                        }
                    }
                }

                // Close the connection.
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return source.ToString();
        }

        #endregion

        #endregion

        #region General Schema Retrieval Methods

        #region private string GetSchemaOwnershipFilter()

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetSchemaOwnershipFilter()
        {
            return GetSchemaOwnershipFilter(null);
        }

        #endregion

        #region private string GetSchemaOwnershipFilter( string aliasOfTableContainingOwnerColumn )

        /// <summary>
        /// Filter what we see to be only our objects.
        /// </summary>
        /// <param name="aliasOfTableContainingOwnerColumn"></param>
        /// <returns></returns>
        private string GetSchemaOwnershipFilter(string aliasOfTableContainingOwnerColumn)
        {
            if (aliasOfTableContainingOwnerColumn == null)
                aliasOfTableContainingOwnerColumn = String.Empty;
            else if (aliasOfTableContainingOwnerColumn.Length > 0)
                aliasOfTableContainingOwnerColumn += ".";

            if (OracleConfiguration.Instance.ShowMySchemaOnly)
                return String.Format("({0}owner in ( select USERNAME from user_users  ))", aliasOfTableContainingOwnerColumn);

            return String.Format("(not {0}owner in ('SYS', 'SYSTEM'))", aliasOfTableContainingOwnerColumn);
        }

        private string GetExtendedPropertiesTableOwnerPrefix()
        {
            // If an explicit owner name was provided, use it, and also build up the variable to be used for prefixing
            if (!String.IsNullOrEmpty(OracleConfiguration.Instance.ExtendedPropertiesTableSchema))
                return String.Format("{0}.", OracleConfiguration.Instance.ExtendedPropertiesTableSchema);
            
            // We don't want an empty String... we want it to always be 'null'
            return null; 
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
            string userName = null;

            // Open a connection
            using (var connection = new OracleConnection(connectionString))
            {
                connection.Open();
                string databaseName = connection.DataSource;

                // Parse the connection string
                Hashtable connectStringParms = ParseConnectStringToHashTable(connectionString);

                // Look for the user name in acceptable forms
                if (connectStringParms["user id"] != null)
                    userName = (string)connectStringParms["user id"];
                else if (connectStringParms["uid"] != null)
                    userName = (string)connectStringParms["uid"];
                else if (connectStringParms["user"] != null)
                    userName = (string)connectStringParms["user"];

                //Account for TNS-less connection strings (http://www.connectionstrings.com/?carrier=oracle):
                bool isTnsLessConnectionString = !String.IsNullOrEmpty(databaseName) && (databaseName.Contains("ADDRESS_LIST") || databaseName.Contains("PROTOCOL=TCP"));
                if (isTnsLessConnectionString && !String.IsNullOrEmpty(userName))
                    return String.Format("{0} ({0})", userName);

                // If username was found, append it to the database name in parenthesis
                if (!String.IsNullOrEmpty(userName))
                    return String.Format("{0} ({1})", databaseName, userName);

                return databaseName;
            }
        }

        #endregion

        #region private static Hashtable ParseConnectStringToHashTable(string connectString)

        private static Hashtable ParseConnectStringToHashTable(string connectString)
        {
            string[] nameValuePairs = connectString.Split(';');

            var hashTable = new Hashtable(nameValuePairs.Length);

            foreach (string s in nameValuePairs)
            {
                if (s.Length > 0)
                {
                    string[] nameAndValue = s.Split('=');
                    if (nameAndValue.Length >= 2)
                    {
                        hashTable.Add(nameAndValue[0].ToLower(), nameAndValue[1]);
                    }
                }
            }

            return hashTable;
        }

        #endregion

        #region private static ParameterDirection GetParameterDirection( bool isReturnVal, string parmDirectionText )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isReturnVal"></param>
        /// <param name="parmDirectionText"></param>
        /// <returns></returns>
        private static ParameterDirection GetParameterDirection(bool isReturnVal, string parmDirectionText)
        {
            ParameterDirection parmDirection = ParameterDirection.ReturnValue;

            if (!isReturnVal)
            {
                switch (parmDirectionText)
                {
                    case "IN":
                        parmDirection = ParameterDirection.Input;
                        break;

                    case "OUT":
                        parmDirection = ParameterDirection.Output;
                        break;

                    case "IN/OUT":
                        parmDirection = ParameterDirection.InputOutput;
                        break;
                }
            }

            return parmDirection;
        }

        #endregion

        #region private static string CleanForOracle( string connectionString )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static string CleanForOracle(string connectionString)
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

            builder.Remove("Database");
            builder.Remove("Failover Partner");
            builder.Remove("AttachDbFilename");
            builder.Remove("Asynchronous Processing");
            builder.Remove("MultipleActiveResultSets");
            builder.Remove("Replication");
            builder.Remove("Connect Timeout");
            builder.Remove("Encrypt");
            builder.Remove("TrustServerCertificate");
            builder.Remove("Network Library");
            builder.Remove("Packet Size");
            builder.Remove("Current Language");
            builder.Remove("Workstation ID");
            builder.Remove("User Instance");
            builder.Remove("Context Connection");

            return builder.ConnectionString;
        }

        #endregion

        #endregion

        #region DbType Conversions

        #region public MapCollection DbTypeToCSharp

        private MapCollection _dbTypeToCSharp;
        public MapCollection DbTypeToCSharp
        {
            get
            {
                if (_dbTypeToCSharp == null)
                {
                    string path;
                    Map.TryResolvePath("DbType-CSharp", CodeSmith.Engine.Configuration.Instance.CodeSmithMapsDirectory, out path);
                    _dbTypeToCSharp = Map.Load(path);
                }

                return _dbTypeToCSharp;
            }
        }

        #endregion

        #region public MapCollection CSharpToSystemType

        private MapCollection _cSharpToSystemType;
        public MapCollection CSharpToSystemType
        {
            get
            {
                if (_cSharpToSystemType == null)
                {
                    string path;
                    Map.TryResolvePath("System-CSharpAlias", CodeSmith.Engine.Configuration.Instance.CodeSmithMapsDirectory, out path);
                    _cSharpToSystemType = new MapCollection(path, true);
                }

                return _cSharpToSystemType;
            }
        }

        #endregion

        #region private static int GetDBTypeLength( string dataTypeName, int length )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataTypeName"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static int GetDBTypeLength(string dataTypeName, int length)
        {
            switch (dataTypeName)
            {
                case "VARCHAR2":
                case "NVARCHAR2":
                case "VARCHAR":
                case "CHAR":
                case "LONG":
                case "RAW":
                case "LONG RAW":
                case "CLOB":
                case "NCLOB":
                case "BLOB":
                    return length;

                default:
                    // Size is not relevant to other data types
                    return 0;
            }
        }

        #endregion

        #region private static int GetDBTypeLength( IDataRecord reader )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static int GetDBTypeLength(IDataRecord reader)
        {
            var nativeType = (string)reader["DATA_TYPE"];

            Int32 length = 0;

            try
            {
                length = Convert.ToInt32(reader["DATA_LENGTH"]);
            }
            catch
            {
            }

            return GetDBTypeLength(nativeType, length);
        }

        #endregion

        #region private static DbType GetDbTypeFromSystemType(Type type)

        /// <summary>
        /// Converts the provided system type into the corresponding DbType for CodeSmith's sake.
        /// </summary>
        /// <param name="type">The System type to be converted.</param>
        /// <returns>The corresponding DbType.</returns>
        private static DbType GetDbTypeFromSystemType(Type type)
        {
            if (type == typeof(String))
            {
                return DbType.String; // DbType.StringFixedLength, DbType.AnsiString, DbType.AnsiStringFixedLength
            }
            if (type == typeof(Boolean))
            {
                return DbType.Boolean;
            }
            if (type == typeof(Byte))
            {
                return DbType.Byte;
            }
            if (type == typeof(SByte))
            {
                return DbType.SByte;
            }
            if (type == typeof(Int16))
            {
                return DbType.Int16;
            }
            if (type == typeof(UInt16))
            {
                return DbType.UInt16;
            }
            if (type == typeof(Int32))
            {
                return DbType.Int32;
            }
            if (type == typeof(UInt32))
            {
                return DbType.UInt32;
            }
            if (type == typeof(Int64))
            {
                return DbType.Int64;
            }
            if (type == typeof(UInt64))
            {
                return DbType.UInt64;
            }
            if (type == typeof(Single))
            {
                return DbType.Single;
            }
            if (type == typeof(Double))
            {
                return DbType.Double;
            }
            if (type == typeof(Decimal))
            {
                return DbType.Decimal;
            }
            if (type == typeof(DateTime))
            {
                return DbType.DateTime; // DbType.Date, DbType.Time
            }
            if (type == typeof(Byte[]))
            {
                return DbType.Binary;
            }
            throw new NotSupportedException(String.Format("Conversion of System type '{0}' to a corresponding DbType is unhandled.", type));

            // Unhandled conversions:
            //		DbType.Currency:
            //		DbType.Guid:
            //		DbType.Object:
            //		DbType.VarNumeric:
        }

        #endregion

        #region private static DbType GetDbType( string nativeType, int precision, int scale )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nativeType"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        private static DbType GetDbType(string nativeType, int precision, int scale)
        {
            //Note: These types were determined from: http://msdn.microsoft.com/en-us/library/yk72thhd.aspx
            switch (nativeType.Trim().ToUpper())
            {
                case "BFILE":
                    return DbType.Object;
                case "BLOB":
                    return DbType.Object;
                case "CHAR":
                    return DbType.AnsiStringFixedLength;
                case "CLOB":
                    return DbType.Object;
                case "DATE":
                    return DbType.DateTime;

                case "FLOAT":
                    return DbType.Decimal; //Single, Double, Decimal

                case "INTEGER":
                    return DbType.Decimal; //SByte, Int16, Int32, Int64, Decimal

                case "UNSIGNED INTEGER":
                    return DbType.Decimal; //Byte, UInt16, UInt32, UInt64, Decimal

                case "INTERVAL YEAR TO MONTH":
                    return DbType.Int32;
                case "INTERVAL DAY TO SECOND":
                    return DbType.Object;
                case "LONG":
                    return DbType.AnsiString;
                case "LONG RAW":
                    return DbType.Binary;
                case "NCHAR":
                    return DbType.StringFixedLength;
                case "NCLOB":
                    return DbType.Object;
                case "NUMBER":
                    // Standard Oracle NUMBER has scale and precision = 0.
                    if (precision == 0 && scale == 0) return DbType.Decimal;

                    // Integer types (http://msdn.microsoft.com/en-us/library/dwhawy9k(VS.80).aspx).
                    if (precision < 3 && scale == 0)
                        return DbType.Byte;
                    if (precision < 5 && scale == 0)
                        return DbType.Int16;
                    if (precision < 10 && scale == 0)
                        return DbType.Int32;
                    if (precision < 19 && scale == 0)
                        return DbType.Int64;
                    if (precision >= 19 && scale == 0)
                        return DbType.Decimal;
                    // Decimal types
                    if (precision < 8)
                        return DbType.Single;
                    if (precision < 16)
                        return DbType.Double;
                    return DbType.Decimal;
                case "NVARCHAR2":
                    return DbType.String;
                case "RAW":
                    return DbType.Binary;
                case "REF CURSOR":
                    return DbType.Object;
                case "ROWID":
                    return DbType.AnsiString;
                case "TIMESTAMP":
                    return DbType.DateTime;
                case "TIMESTAMP WITH LOCAL TIME ZONE":
                    return DbType.DateTime;
                case "TIMESTAMP WITH TIME ZONE":
                    return DbType.DateTime;
                case "VARCHAR2":
                    return DbType.AnsiString;

                // These were found on the following oracle site: http://download-east.oracle.com/docs/html/B28089_01/featTypes.htm#i1006604
                case "BINARY_DOUBLE":
                    return DbType.Decimal;
                case "BINARY_FLOAT":
                    return DbType.Decimal;
                case "BINARY_INTEGER":
                    return DbType.Decimal;
                case "PLS_INTEGER":
                    return DbType.Decimal;
                case "Collection":
                    return DbType.String;
                case "UROWID":
                    return DbType.String;
                case "XMLType":
                    return DbType.String;

                default:
                    return DbType.Object;
            }
        }

        #endregion

        #region private static DbType GetDbType( IDataRecord reader )

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static DbType GetDbType(IDataRecord reader)
        {
            string nativeType = reader["DATA_TYPE"].ToString();

            #region Scale

            int scale;
            if (!int.TryParse(reader["DATA_SCALE"].ToString(), out scale))
            {
                //Scale could not be parsed so fix it..
            }

            #endregion

            #region Precision

            int precision;
            if (!int.TryParse(reader["DATA_PRECISION"].ToString(), out precision))
            {
                //Precision could not be parsed so fix it..
            }

            #endregion

            // This fixes native types that still contain size information like TIMESTAMP(6).
            if (nativeType.Contains("("))
            {
                nativeType = nativeType.Remove(nativeType.IndexOf("("));
            }

            return GetDbType(nativeType, precision, scale);
        }

        #endregion

        #endregion
    }
}
