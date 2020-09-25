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
// Credits:
//    Provider was submitted by David Logan <david@loganent.net>.
//    Some original work was taken from Bill Hall and Dan Gowin's iSeries DB2 Provider.
//    Edited by Blake Niemyjski (mailto:bniemyjski@codesmithtools.com).
//
// Notes:
//    A user must have the IBM Data Server Client(or equivilant) installed to use this provider.
//    Requires zOS database. This provider was created and tested against z/os v9 with client 9.7 fp7.
//
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;

using IBM.Data.DB2;

using SchemaExplorer.Extensions;

namespace SchemaExplorer {
    public class DB2zOSSchemaProvider : IDbSchemaProvider, IDbConnectionStringEditor {
        
        public DB2zOSSchemaProvider() { }

        #region IDbConnectionStringEditor Members

        /// <summary>
        /// Connection String
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Connection Editor
        /// </summary>
        public bool EditorAvailable {
            get { return false; }
        }

        /// <summary>
        /// Shows the Connection Editor.
        /// </summary>
        /// <param name="currentConnectionString"></param>
        /// <returns></returns>
        public bool ShowEditor(string currentConnectionString) {
            // NOTE: This code is commented out as it requires a reference to Microsoft.VisualStudio.Interop and Microsoft.VisualStudio.OLE.Interop. These two assemblies require that you have VS 2010 installed.

            //IVsDataConnectionDialogFactory objIVsDataConnectionDialogFactory;
            //IVsDataConnectionDialog objIVsDataConnectionDialog;
            //String sConnectionString = "";
            //bool result = false;

            //var dte = new DB2DataAdapter();
            //var objService = GetService(dte, typeof(IVsDataConnectionDialogFactory));
            //objIVsDataConnectionDialogFactory = (IVsDataConnectionDialogFactory)objService;
            //objIVsDataConnectionDialog = objIVsDataConnectionDialogFactory.CreateConnectionDialog();

            //objIVsDataConnectionDialog.AddAllSources();

            //if (objIVsDataConnectionDialog.ShowDialog())
            //{
            //   sConnectionString = objIVsDataConnectionDialog.DisplayConnectionString;
            //}

            //objIVsDataConnectionDialog.Dispose();
            //if (sConnectionString.Length > 0)
            //{
            //    ConnectionString = sConnectionString;
            //    result = true;
            //}

            //return result;

            return false;
        }

        //public object GetService(object serviceProvider, Type type) {
        //    return GetService(serviceProvider, type.GUID);
        //}

        //public object GetService(object serviceProvider, Guid guid) {
        //    object objService = null;
        //    IntPtr objIntPtr;
        //    Guid objSIDGuid = guid;
        //    Guid objIIDGuid = objSIDGuid;
        //    IServiceProvider objIServiceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)serviceProvider;
        //    int hr = objIServiceProvider.QueryService(ref objSIDGuid, ref objIIDGuid, out objIntPtr);

        //    if (hr != 0) {
        //        System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hr);
        //    } else if (!objIntPtr.Equals(IntPtr.Zero)) {

        //        objService = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(objIntPtr);
        //        System.Runtime.InteropServices.Marshal.Release(objIntPtr);

        //    }

        //    return objService;
        //}

        #endregion

        #region IDbSchemaProvider Members

        public string Name {
            get { return "DB2zOSSchemaProvider"; }
        }

        public string Description {
            get { return @"DB2 zOS Schema Provider"; }
        }

        #endregion

        #region Table Retrieval Methods

        public TableSchema[] GetTables(string connectionString, DatabaseSchema database) {
            var tables = new List<TableSchema>();

            using (var conn = new DB2Connection(connectionString)) {
                // Open a connection
                conn.Open();

                const int POS_OWNER = 0;
                const int POS_NAME = 1;
                const int POS_CREATED = 2;
                const int POS_TYPE = 3;
                const int POS_BASE_TABLE_SCHEMA = 4;
                const int POS_BASE_TABLE_NAME = 5;

                string sql = String.Format(@"
                    SELECT
                        RTRIM(t.CREATOR),
                        RTRIM(t.NAME),
                        t.ALTEREDTS,
                        t.TYPE,
                        coalesce(RTRIM(t.TBCREATOR), '') as TBCREATOR,
                        coalesce(RTRIM(t.TBNAME), '') as TBNAME 
                    FROM
                        SYSIBM.SYSTABLES AS t
                    WHERE
                        t.TYPE IN ('T', 'A')
                        {0}
                        {1}
                    ORDER BY
                        t.CREATOR,
                        t.NAME",
                                GetFilterCriteria(DB2zOSConfiguration.Instance.FilterTables, "t.DBNAME", true),
                                GetFilterCriteria(DB2zOSConfiguration.Instance.FilterSchema, "t.CREATOR", true));

                using (var cmd = new DB2Command(sql, conn)) {
                    using (DB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)) {
                        while (reader.Read()) {
                            if (String.Equals(reader.GetString(POS_NAME), "CODESMITH_EXTENDED_PROPERTIES", StringComparison.OrdinalIgnoreCase))
                                continue;

                            var properties = new List<ExtendedProperty>();
    
                            var type = reader.GetString(POS_TYPE);
                            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.ObjectType, type));
                                
                            bool isAlias = String.Equals(type, "A", StringComparison.OrdinalIgnoreCase);
                            if (isAlias) {
                                properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.SourceSchemaOwner, reader.GetString(POS_BASE_TABLE_SCHEMA)));
                                properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.SourceSchemaName, reader.GetString(POS_BASE_TABLE_NAME)));
                            }
                            
                            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsSchemaAlias, isAlias));
                            
                            var tableSchema = new TableSchema(database, reader.GetString(POS_NAME), reader.GetString(POS_OWNER).Trim(), reader.GetDateTime(POS_CREATED), properties.ToArray());
                            tables.Add(tableSchema);
                        }
                    }
                }
            }

            return tables.ToArray();
        }

        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table) {
            // Get underlying schema/name info (handle aliases)
            string tableSchema, tableName;
            GetTableInfo(table, out tableSchema, out tableName);

            string sql = String.Format(@"
                SELECT 
                    RTRIM(ind.CREATOR) AS CREATOR, 
                    RTRIM(ind.TBNAME) AS TBNAME, 
                    RTRIM(ind.NAME) AS INDNAME, 
                    ind.UNIQUERULE,
                    RTRIM(indexcol.COLNAME) AS COLNAME, 
                    indexcol.COLNO, 
                    indexcol.COLSEQ,
                    indexcol.ORDERING
                FROM SYSIBM.SYSINDEXES ind 
                    Inner Join SYSIBM.SYSKEYS indexcol ON ind.NAME = indexcol.IXNAME 
                                        AND ind.CREATOR = indexcol.IXCREATOR
                WHERE
                     ind.CREATOR = '{0}' and ind.TBNAME = '{1}'
                ORDER BY ind.NAME, indexcol.COLSEQ",
                tableSchema,
                tableName);

            // Collections to hold index information
            var indexMemberCollection = new NameValueCollection();
            var isUniqueByIndexName = new Hashtable();
            var isPrimaryByIndexName = new Hashtable();
            var isClusteredByIndexName = new Hashtable();

            using (var conn = new DB2Connection(connectionString)) {
                // Open a connection
                conn.Open();

                using (var cmd = new DB2Command(sql, conn)) {
                    using (DB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)) {
                        // int constraintTypeOrdinal = reader.GetOrdinal("CONSTRAINT_TYPE");
                        // int tableTypeOrdinal = reader.GetOrdinal("TABLE_TYPE");

                        while (reader.Read()) {
                            Debug.Assert(reader["INDNAME"].ToString().Trim().Length > 0, "Invalid Index Name from SYSIBM.SYSINDEXES!");
                            Debug.Assert(reader["COLNAME"].ToString().Trim().Length > 0, "Invalid Column Name from SYSIBM.SYSINDEXES!");
                            // Add the column to the collection by index name
                            indexMemberCollection.Add((string)reader["INDNAME"], (string)reader["COLNAME"]);

                            // Determine if index is unique
                            switch (reader["UNIQUERULE"].ToString()) {
                                case "P":
                                case "U":
                                case "C":
                                case "R":
                                case "G":
                                    isUniqueByIndexName[reader["INDNAME"]] = true;
                                    break;
                                default:
                                    isUniqueByIndexName[reader["INDNAME"]] = false;
                                    break;
                            }

                            // Determine if index is the primary key index
                            isPrimaryByIndexName[reader["INDNAME"]] = String.Equals(reader["UNIQUERULE"].ToString(), "P", StringComparison.OrdinalIgnoreCase);

                            // Determine if the index is on a TABLE or CLUSTER
                            // NOTE: A Microsoft® SQL Server™ clustered index is not like an Oracle cluster. 
                            // An Oracle cluster is a physical grouping of two or more tables that share the 
                            // same data blocks and use common columns as a cluster key. SQL Server does not 
                            // have a structure that is similar to an Oracle cluster.
                            //bool isClustered = String.Equals(reader["INDEXTYPE"].ToString(), "CLUS", StringComparison.OrdinalIgnoreCase);
                            isClusteredByIndexName[reader["INDNAME"]] = false;
                        }
                    }
                }
            }

            // Declare an array to hold the index schemas to be returned
            var indexSchemas = new List<IndexSchema>(indexMemberCollection.Count);

            // Were there any index members found?
            if (indexMemberCollection.Count > 0) {
                // Iterate through the distinct keys (index names) of the NameValueCollection
                indexSchemas.AddRange(indexMemberCollection.AllKeys.Select(indexName => 
                    new IndexSchema(table, 
                        indexName, 
                        (bool)isPrimaryByIndexName[indexName], 
                        (bool)isUniqueByIndexName[indexName], 
                        (bool)isClusteredByIndexName[indexName], 
                        indexMemberCollection.GetValues(indexName))));
            }

            return indexSchemas.ToArray();
        }

        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table) {
            var columns = new List<ColumnSchema>();

            using (var conn = new DB2Connection(connectionString)) {
                // Open a connection
                conn.Open();

                const int POS_COLUMN_NAME = 0;
                const int POS_DATA_TYPE = 1;
                // const int POS_DATA_LENGTH	= 2; // LENGTH is precision for DECIMALS
                const int POS_DATA_SCALE = 3;
                const int POS_NULLABLE = 4;
                const int POS_IDENTITY = 5;
                // const int POS_COMMENTS		= 6;

                // Get underlying schema/name info (handle aliases)
                string tableSchema;
                string tableName;

                GetTableInfo(table, out tableSchema, out tableName);

                //COLTYPE is RTRIM()'d becuase "CHAR" and some other TYPES have trailing spaces on them.
                string sql = String.Format(@"
                    SELECT	RTRIM(NAME) AS NAME, 
                                RTRIM(COLTYPE) AS DBTYPE,
                                LENGTH, 
                                SCALE, 
                                NULLS, 
                                CASE DEFAULT
                                    WHEN 'I' THEN 'Y'
                                    ELSE 'N'
                                END AS IDENTITY,                    
                                REMARKS
                        FROM	SYSIBM.SYSCOLUMNS
                        WHERE	TBCREATOR = '{0}'
                                AND TBNAME = '{1}'
                        ORDER BY COLNO",
                    tableSchema,
                    tableName);

                using (var cmd = new DB2Command(sql, conn)) {
                    using (DB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)) {
                        while (reader.Read()) {
                            var properties = new List<ExtendedProperty> {
                                ExtendedProperty.Readonly(ExtendedPropertyNames.IsIdentity, String.Equals(reader.GetString(POS_IDENTITY), "Y", StringComparison.OrdinalIgnoreCase)),
                                CreateExtendedPropertyForComments(reader)
                            };

                            // Get precision
                            byte precision = GetDbTypePrecision(reader);

                            var cs = new ColumnSchema(
                                table,
                                reader.GetString(POS_COLUMN_NAME),
                                GetDbType(reader),
                                reader.GetString(POS_DATA_TYPE),
                                GetDbTypeLength(reader),
                                precision,
                                Convert.ToInt32(reader[POS_DATA_SCALE]),
                                String.Equals(reader.GetString(POS_NULLABLE), "Y", StringComparison.OrdinalIgnoreCase),
                                properties.ToArray());

                            columns.Add(cs);
                        }
                    }
                }
            }

            return columns.ToArray();
        }

        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table) {
            var tableKeySchemas = new List<TableKeySchema>();

            using (var conn = new DB2Connection(connectionString)) {
                string tableSchema;
                string tableName;

                GetTableInfo(table, out tableSchema, out tableName);

                // Open a connection
                conn.Open();

                string sql = String.Format(@"
                    SELECT 	RTRIM(ref.RELNAME) AS constraintName, 
                                RTRIM(ref.TBNAME) AS foreignTableName, 
                                RTRIM(foreign_tbl.COLNAME) AS foreignColumnName, 
                                RTRIM(ref.REFTBNAME) AS primaryTableName, 
                                RTRIM(primary_tbl.COLNAME) AS primaryColumnName, 
                                foreign_tbl.COLSEQ AS position
                        FROM SYSIBM.SYSRELS ref 
                            INNER JOIN SYSIBM.SYSFOREIGNKEYS foreign_tbl ON ref.CREATOR = foreign_tbl.CREATOR
                                    AND ref.TBNAME = foreign_tbl.TBNAME 
                                    And ref.RELNAME = foreign_tbl.RELNAME 
                            INNER JOIN SYSIBM.SYSINDEXES idx ON ref.CREATOR = idx.CREATOR 
                                AND ref.REFTBNAME = idx.TBNAME
                                AND idx.UNIQUERULE = 'P'
                            INNER JOIN SYSIBM.SYSKEYS primary_tbl ON 
                                idx.CREATOR = primary_tbl.IXCREATOR 
                                AND idx.NAME = primary_tbl.IXNAME
                        WHERE ref.CREATOR = '{0}'
                            AND (ref.TBNAME = '{1}' OR ref.REFTBNAME = '{1}') 
                            AND foreign_tbl.COLSEQ = primary_tbl.COLSEQ
                        ORDER BY ref.RELNAME, foreign_tbl.COLSEQ",
                    tableSchema,
                    tableName);

                var da = new DB2DataAdapter(sql, conn);

                var ds = new DataSet();
                using (ds) {
                    da.Fill(ds);

                    string lastConstraint = String.Empty;
                    foreach (DataRow row in ds.Tables[0].Rows) {
                        if (lastConstraint != row["constraintName"].ToString()) {
                            lastConstraint = row["constraintName"].ToString();
                            Debug.Assert(lastConstraint.Trim().Length > 0,
                                "Invalid constraintName from SYSIBM.SYSFOREIGNKEYS! for " + tableName);

                            string fkTable = row["foreignTableName"].ToString();
                            string pkTable = row["primaryTableName"].ToString();

                            DataRow[] distinctColumns = ds.Tables[0].Select(
                                String.Format("constraintName = '{0}'", lastConstraint));

                            var pkMemberCols = new List<string>();
                            var fkMemberCols = new List<string>();

                            foreach (DataRow rdc in distinctColumns) {
                                Debug.Assert(rdc["foreignColumnName"].ToString().Trim().Length > 0, String.Format("Invalid Foreign Column Name for {0} in table:{1}", lastConstraint, tableName));
                                Debug.Assert(rdc["primaryColumnName"].ToString().Trim().Length > 0, String.Format("Invalid Primary Column Name for {0} in table:{1}", lastConstraint, tableName));
                                pkMemberCols.Add(rdc["primaryColumnName"].ToString());
                                fkMemberCols.Add(rdc["foreignColumnName"].ToString());
                            }

                            Debug.Assert(fkMemberCols.Count == pkMemberCols.Count, String.Format("Key Count Mismatch with Table:{0}, Constraint:{1}, fkCount:{2}, pkCount:{3}", tableName, lastConstraint, fkMemberCols.Count, pkMemberCols.Count));

                            var tks = new TableKeySchema(table.Database,
                                lastConstraint,
                                fkMemberCols.ToArray(),
                                fkTable,
                                pkMemberCols.ToArray(),
                                pkTable);

                            tableKeySchemas.Add(tks);
                        }
                    }

                }
            }

            return tableKeySchemas.ToArray();
        }

        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table) {
            PrimaryKeySchema primaryKeySchema = null;

            using (var conn = new DB2Connection(connectionString)) {
                // Get underlying schema/name info (handle aliases)
                string tableSchema, tableName;
                GetTableInfo(table, out tableSchema, out tableName);

                // Open a connection
                conn.Open();

                string sql = String.Format(@"
                        SELECT 
                                indexes.NAME AS constraint_name, 
                                cols.COLNAME AS column_name,
                                cols.COLSEQ AS position
                            FROM SYSIBM.SYSINDEXES indexes 
                            INNER JOIN SYSIBM.SYSKEYS cols ON 
                                indexes.CREATOR = cols.IXCREATOR 
                                AND indexes.NAME = cols.IXNAME
                            WHERE CREATOR = '{0}'
                            AND TBNAME = '{1}'
                            AND UNIQUERULE = 'P'
                            ORDER BY indexes.NAME, cols.COLSEQ",
                    tableSchema,
                    tableName);

                var da = new DB2DataAdapter(sql, conn);
                using (var ds = new DataSet()) {
                    int rowCount = da.Fill(ds);

                    if (rowCount > 0) {
                        Debug.Assert(ds.Tables[0].Rows[0]["constraint_name"].ToString().Trim().Length > 0, "Invalid constraint_name from SYSKEYS");
                        var memberCols = new List<string>();

                        foreach (DataRow row in ds.Tables[0].Rows) {
                            Debug.Assert(row["column_name"].ToString().Trim().Length > 0, String.Format("Invalid Column Name for {0}", ds.Tables[0].Rows[0]["constraint_name"]));
                            memberCols.Add(row["column_name"].ToString());
                        }

                        primaryKeySchema = new PrimaryKeySchema(table, ds.Tables[0].Rows[0]["constraint_name"].ToString(), memberCols.ToArray());
                    } else if (DB2zOSConfiguration.Instance.UseUniqueIndexWhenNoPrimaryKey) {
                        var indexes = GetTableIndexes(connectionString, table);

                        foreach (var idx in indexes) {
                            if (idx.IsUnique) {
                                var memberCols = idx.MemberColumns.Select(row => row.Name).ToList();
                                primaryKeySchema = new PrimaryKeySchema(table, idx.Name, memberCols.ToArray());
                                break;
                            }
                        }
                    }
                }
            }

            return primaryKeySchema;
        }

        public DataTable GetTableData(string connectionString, TableSchema table) {
            using (var conn = new DB2Connection(connectionString)) {
                // Create and fill the data table
                var dt = new DataTable(table.Name);

                string sql = String.Format("SELECT * FROM {0}.{1}", table.Owner, table.Name);

                using (var adp = new DB2DataAdapter(sql, conn)) {
                    adp.Fill(dt);
                }

                return dt;
            }
        }

        private void GetTableInfo(TableSchema table, out string tableSchema, out string tableName) {
            if (table.ExtendedProperties.GetByKey<bool>(ExtendedPropertyNames.IsSchemaAlias)) {
                tableSchema = table.ExtendedProperties.GetByKey<string>(ExtendedPropertyNames.SourceSchemaOwner);
                tableName = table.ExtendedProperties.GetByKey<string>(ExtendedPropertyNames.SourceSchemaName);
            } else {
                tableSchema = table.Owner;
                tableName = table.Name;
            }
        }

        #endregion

        #region Extended Properties

        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject) {
            if (String.IsNullOrEmpty(connectionString) || schemaObject == null)
                return;

            //CreateExtended Properties Table
            if (DB2zOSConfiguration.Instance.AutoCreateExtendedPropertiesTable)
                CreateExtendedPropertiesTable(connectionString);
            //else if (!HasExtendedPropertiesTable(connectionString, schemaObject))
            //    return;

            // Get the schema object name and type name to be used to retrieve the extended property.
            string containerObjectOwner;
            string objectName = schemaObject.Name;
            string schemaObjectTypeName = schemaObject.GetType().Name;

            switch (schemaObjectTypeName) {
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
                    // SchemaExplorer ignores constraint ownership, use the table's owner instead.    
                    break;

                case "TableKeySchema":
                    containerObjectOwner = ((TableKeySchema)schemaObject).ForeignKeyTable.Owner;
                    // SchemaExplorer ignores constraint ownership, use the foreign key table owner instead.
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
            using (var connection = new DB2Connection(connectionString)) {
                connection.Open();

                foreach (ExtendedProperty extendedProperty in schemaObject.ExtendedProperties) {
                    string command = null;
                    string name = extendedProperty.Name;

                    if (String.Equals(ExtendedPropertyNames.Description, name, StringComparison.OrdinalIgnoreCase) && !schemaObject.ExtendedProperties.Contains(name))
                        extendedProperty.PropertyState = PropertyStateEnum.New;

                    switch (extendedProperty.PropertyState) {
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
                    if (!String.IsNullOrEmpty(command)) {
                        using (var db2Command = new DB2Command(command, connection)) {
                            db2Command.ExecuteScalar();
                        }
                    }
                } // for each

                if (needRefresh)
                    schemaObject.Refresh();
            }
        }

        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject) {
            // Get the schema object name and type name to be used to retrieve the extended property.
            string containerObjectOwner;
            string objectName = schemaObject.Name;
            string schemaObjectTypeName = schemaObject.GetType().Name;

            switch (schemaObjectTypeName) {
                case "ColumnSchema":
                    containerObjectOwner = ((ColumnSchema)schemaObject).Table.Owner;
                    var column = (ColumnSchema)schemaObject;
                    objectName = column.Table.Name + "." + column.Name;
                    break;
                case "MemberColumnSchema":
                    containerObjectOwner = ((ColumnSchema)schemaObject).Table.Owner;
                    var column1 = (ColumnSchema)schemaObject;
                    objectName = column1.Table.Name + "." + column1.Name;
                    break;
                case "ViewColumnSchema":
                    var viewColumn = (ViewColumnSchema)schemaObject;
                    containerObjectOwner = viewColumn.View.Owner;
                    objectName = viewColumn.View.Name + "." + viewColumn.Name;
                    break;
                case "CommandResultColumnSchema":
                    var commandResultColumn = (CommandResultColumnSchema)schemaObject;
                    containerObjectOwner = commandResultColumn.Command.Owner;
                    objectName = commandResultColumn.Command.Name;
                    break;
                case "CommandSchema":
                    containerObjectOwner = ((CommandSchema)schemaObject).Owner;
                    // 05-08-2013 from last version after release set objectName
                    objectName = ((CommandSchema)schemaObject).Name;
                  
                    break;
                case "DatabaseSchema":
                    objectName = "Database"; // TODO - Use user name here instead?
                    containerObjectOwner = "Database";
                    break;
                case "IndexSchema":
                    containerObjectOwner = ((IndexSchema)schemaObject).Table.Owner;
                    // SchemaExplorer ignores index ownership, use the table's owner instead.
                    // 05-08-2013 from last version after release set objectName
                    objectName = ((IndexSchema)schemaObject).Name;
            
                    break;
                case "ParameterSchema":
                    var parameter = (ParameterSchema)schemaObject;
                    containerObjectOwner = parameter.Command.Owner;
                    objectName = String.Format("{0}.{1}", parameter.Command.Name, parameter.Name);
                    break;
                case "PrimaryKeySchema":
                    containerObjectOwner = ((PrimaryKeySchema)schemaObject).Table.Owner;
                    // SchemaExplorer ignores constraint ownership, use the table's owner instead.
                    // 05-08-2013 from last version after release set objectName
                    objectName = ((PrimaryKeySchema)schemaObject).Name;        
                    break;
                case "TableKeySchema":
                    containerObjectOwner = ((TableKeySchema)schemaObject).ForeignKeyTable.Owner;
                    // SchemaExplorer ignores constraint ownership, use the foreign key table owner instead.
                    break;
                case "TableSchema":
                    containerObjectOwner = ((TableSchema)schemaObject).Owner;
                    break;
                case "ViewSchema":
                    containerObjectOwner = ((ViewSchema)schemaObject).Owner;
                    break;
                default:
                    // Don't recognize the SchemaExplorer type... need to throw an exception
                    throw new NotSupportedException(String.Format("Unexpected SchemaExplorer type '{0}' encountered.  Extended properties are not yet supported for this type.", schemaObjectTypeName));
            }

            // Define SQL to pull appropriate extended properties from the table
            string sql = String.Format(@"
                    SELECT	property_name, 
                                property_value, 
                                clr_type
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

            var properties = new List<ExtendedProperty>();

            using (var conn = new DB2Connection(connectionString)) {
                conn.Open();

                try {
                    using (var cmd = new DB2Command(sql, conn)) {
                        using (DB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)) {
                            while (reader.Read()) {
                                // Pull all the values into variables
                                string propertyName = reader.GetString(POS_PROPERTY_NAME);
                                string propertyValueText = reader.GetString(POS_PROPERTY_VALUE);
                                string clrTypeName = reader.GetString(POS_CLR_TYPE);

                                // Get the System type for the value
                                Type clrType = Type.GetType(clrTypeName);
                                if (clrType == null) 
                                    continue;

                                // Convert the string representation of the property to the appropriate CLR type
                                object propertyValue = Convert.ChangeType(propertyValueText, clrType);

                                // Construct and add the extended property 
                                properties.Add(new ExtendedProperty(propertyName, propertyValue, GetDbTypeFromSystemType(clrType)));
                            }
                        }
                    }
                } catch (DB2Exception ex) {
                    // Check for the DB2 exception that indicates that the table isn't there
                    // TODO: Change to DB2 specific message
                    if (ex.Errors[0].NativeError == -204 && ex.Message.IndexOf("CODESMITH", StringComparison.OrdinalIgnoreCase) >= 0) {
                        // Should we try to automatically create the table?
                        if (DB2zOSConfiguration.Instance.AutoCreateExtendedPropertiesTable) {
                            // Try to create the extended properties table
                            CreateExtendedPropertiesTable(connectionString);

                            // If we're still here, close the connection so recursive call doesn't try to open up another one needlessly
                            conn.Close();

                            // If we're still here, it was successful, so try to get property again
                            return GetExtendedProperties(connectionString, schemaObject);
                        }

                        throw new ApplicationException(String.Format("Extended properties table '{0}CODESMITH_EXTENDED_PROPERTIES' does not yet exist.", GetExtendedPropertiesTableOwnerPrefix()), ex);
                    }

                    throw;
                }

                return properties.ToArray();
            }
        }

        private string GetExtendedPropertiesTableOwnerPrefix() {
            // If an explicit owner name was provided, use it, and also build up the variable to be used for prefixing
            if (!String.IsNullOrEmpty(DB2zOSConfiguration.Instance.ExtendedPropertiesTableSchema))
                return String.Format("{0}.", DB2zOSConfiguration.Instance.ExtendedPropertiesTableSchema);

            // We don't want an empty String... we want it to always be 'null'
            return null;
        }

        private void CreateExtendedPropertiesTable(string connectionString) {
            string sqlCreate = String.Format(@"
                CREATE TABLE {0}codesmith_extended_properties
                (
                    container_object_owner     VARCHAR(50) NOT NULL,
                    object_name                VARCHAR(100) NOT NULL,
                    codesmith_schema_type      VARCHAR(200) NOT NULL,
                    property_name              VARCHAR(200) NOT NULL,
                    property_value             VARCHAR(4000),
                    clr_type                   VARCHAR(50) NOT NULL
                )", GetExtendedPropertiesTableOwnerPrefix());

            string sqlAddPK = String.Format(@"
                ALTER TABLE {0}codesmith_extended_properties
                    ADD CONSTRAINT pkcodesmithextprop PRIMARY KEY
                    (
                        container_object_owner,
                        object_name,
                        codesmith_schema_type,
                        property_name
                    )", GetExtendedPropertiesTableOwnerPrefix());

            using (var conn = new DB2Connection(connectionString)) {
                conn.Open();

                try {
                    using (var cmd = new DB2Command(sqlCreate, conn)) {
                        cmd.ExecuteNonQuery();
                    }
                } catch (Exception ex) {
                    // Wrap the exception in our own, and rethrow
                    throw new ApplicationException(String.Format("Unable to automatically create extended properties because the following exception occurred:\n\n{0}", ex.Message), ex);
                }

                try {
                    using (var cmd = new DB2Command(sqlAddPK, conn)) {
                        cmd.ExecuteNonQuery();
                    }
                } catch (Exception ex) {
                    // Wrap the exception in our own, and rethrow
                    throw new ApplicationException(String.Format("Unable to add primary key to extended properties table during auto-creation because the following exception occurred:\n\n{0}", ex.Message), ex);
                }
            }
        }

        private ExtendedProperty CreateExtendedPropertyForComments(DB2DataReader reader) {
            int ordinal = reader.GetOrdinal("REMARKS");
            if (ordinal < 0)
                throw new ApplicationException("There was no COMMENTS column in the DB2DataReader provided.");

            string comments = reader.IsDBNull(ordinal) ? String.Empty : reader.GetString(ordinal);
            return ExtendedProperty.Readonly(ExtendedPropertyNames.Comment, comments);
        }

        #endregion

        #region View Retrieval Methods

        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database) {
            var viewList = new List<ViewSchema>();

            using (var conn = new DB2Connection(connectionString)) {
                // Open a connection
                conn.Open();

                const int POS_OWNER = 0;
                const int POS_NAME = 1;
                // const int POS_CREATED = 2;

                string sql = String.Format(@"
                              SELECT
                                    RTRIM(t.CREATOR),
                                    RTRIM(t.NAME),
                                    t.TYPE
                              FROM
                                    SYSIBM.SYSTABLES AS t
                              WHERE
                                    t.TYPE = 'V'
                                    {0}
                                    {1}
                              ORDER BY
                                    t.CREATOR,
                                    t.NAME",
                    GetFilterCriteria(DB2zOSConfiguration.Instance.FilterViews, "t.DBNAME", true),
                    GetFilterCriteria(DB2zOSConfiguration.Instance.FilterSchema, "t.CREATOR", true));

                using (var cmd = new DB2Command(sql, conn)) {
                    using (DB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)) {
                        while (reader.Read()) {
                            // Exclude the extended properties table, if appropriate
                            var viewSchema = new ViewSchema(database,
                                reader.GetString(POS_NAME),
                                reader.GetString(POS_OWNER).Trim(),
                                DateTime.MinValue);
                                
                            viewList.Add(viewSchema);
                        }
                    }
                }
            }
            
            return viewList.ToArray();
        }

        public string GetViewText(string connectionString, ViewSchema view) {
            string viewText;

            using (var conn = new DB2Connection(connectionString)) {
                // Open a connection
                conn.Open();

                // Get SQL to retrieve source for the view
                string sql = String.Format(@"
                    SELECT TEXT 
                                FROM SYSIBM.SYSROUTINES 
                                WHERE CREATOR = '{0}'
                                    AND NAME = '{1}'
                                    AND LENGTH(TEXT) > 0",
                    view.Owner,
                    view.Name);

                // Get the view text
                using (var cmd = new DB2Command(sql, conn)) {
                    viewText = (string)cmd.ExecuteScalar();
                }
            }

            return viewText;
        }

        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view) {
            var columnList = new List<ViewColumnSchema>();

            using (var conn = new DB2Connection(connectionString)) {
                // Open a connection
                conn.Open();

                const int POS_COLUMN_NAME = 0;
                const int POS_DATA_TYPE = 1;
                // const int POS_DATA_LENGTH	= 2; // LENGTH is precision for DECIMALS
                const int POS_DATA_SCALE = 3;
                const int POS_NULLABLE = 4;
                // const int POS_IDENTITY		= 5;
                // const int POS_COMMENTS		= 6;

                string sql = String.Format(@"
                    SELECT	RTRIM(NAME) AS NAME, 
                                RTRIM(COLTYPE) AS DBTYPE,
                                LENGTH, 
                                SCALE, 
                                NULLS, 
                                CASE DEFAULT
                                    WHEN 'I' THEN 'Y'
                                    ELSE 'N'
                                END AS IDENTITY,                    
                                REMARKS
                        FROM	SYSIBM.SYSCOLUMNS
                        WHERE	TBCREATOR = '{0}'
                                AND TBNAME = '{1}'
                        ORDER BY COLNO",
                    view.Owner,
                    view.Name);

                using (var cmd = new DB2Command(sql, conn)) {
                    using (DB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)) {
                        while (reader.Read()) {
                            var properties = new List<ExtendedProperty> { CreateExtendedPropertyForComments(reader) };

                            // Get precision
                            byte precision = GetDbTypePrecision(reader);

                            var cs = new ViewColumnSchema(
                                view,
                                reader.GetString(POS_COLUMN_NAME),
                                GetDbType(reader),
                                reader.GetString(POS_DATA_TYPE),
                                GetDbTypeLength(reader),
                                precision,
                                Convert.ToInt32(reader[POS_DATA_SCALE]),
                                String.Equals(reader.GetString(POS_NULLABLE), "Y", StringComparison.OrdinalIgnoreCase),
                                properties.ToArray());

                            columnList.Add(cs);
                        }
                    }
                }
            }
          
            // Return the array of columns
            return columnList.ToArray();
        }

        public DataTable GetViewData(string connectionString, ViewSchema view) {
            using (var conn = new DB2Connection(connectionString)) {
                // Create and fill the data table
                var dt = new DataTable(view.Name);
                string sql = String.Format("SELECT * FROM {0}.{1}", view.Owner, view.Name);
                using (var adp = new DB2DataAdapter(sql, conn)) {
                    adp.Fill(dt);
                }

                return dt;
            }
        }

        #endregion

        #region Commnad Retrieval Methods

        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database) {
            var commands = new List<CommandSchema>();

            using (var conn = new DB2Connection(connectionString)) {
                // Open a connection
                conn.Open();

                // TODO, this query could return a lot more information: http://publib.boulder.ibm.com/infocenter/dzichelp/v2r2/index.jsp?topic=%2Fcom.ibm.db2z9.doc.sqlref%2Fsrc%2Ftpc%2Fdb2z_sysibmsysroutinestable.htm
                const int POS_OWNER = 0;
                const int POS_PROCNAME = 1;
                const int POS_SPECIFICNAME = 2;
                const int POS_PROCEDUREID = 3;
                const int POS_CREATED = 4;
                const int POS_DEFINER = 5;
                const int POS_LANGUAGE = 6;
                //const int POS_VALID = 7; -- Field no longer exists in DB2 zOS database
                const int POS_OVERLOADCOUNT = 7;
                const int POS_PARMSIGNATURE = 8;
                const int POS_RESULTSETS = 9;
                const int POS_FUNCTION_TYPE = 10;
                const int POS_ROUTINETYPE = 11;
                const int POS_RETURN_TYPE = 12;
                const int POS_ORIGIN = 13;
                const int POS_SOURCESCHEMA = 14;
                const int POS_SOURCESPECIFIC = 15;
                const int POS_DETERMINISTIC = 16;
                const int POS_REMARKS = 17;

                string schemaFilterOuter = GetFilterCriteria(DB2zOSConfiguration.Instance.FilterCommands, "c.SCHEMA", false);

                // Insert the WHERE clause into the schema filter, if it's been added
                if (schemaFilterOuter != null)
                    schemaFilterOuter = String.Format("WHERE {0}", schemaFilterOuter);

                string schemaFilterInner = GetFilterCriteria(DB2zOSConfiguration.Instance.FilterCommands, "SCHEMA", false);

                // Insert the WHERE clause into the schema filter, if it's been added
                if (schemaFilterInner != null)
                    schemaFilterInner = String.Format("WHERE {0}", schemaFilterInner);

                // http://publib.boulder.ibm.com/infocenter/dzichelp/v2r2/index.jsp?topic=%2Fcom.ibm.db2z9.doc.sqlref%2Fsrc%2Ftpc%2Fdb2z_sysibmsysroutinestable.htm
                string sql = String.Format(@"
                    SELECT  RTRIM(c.SCHEMA) AS SCHEMA,
                            RTRIM(c.NAME)  AS NAME,
                            RTRIM(c.SPECIFICNAME) AS SPCIFICNAME,
                            c.ROUTINEID,
                            c.ALTEREDTS,
                            RTRIM(c.CREATEDBY) AS CREATEDBY,
                            RTRIM(c.LANGUAGE) AS LANGUAGE,
                            o.overload_count,
                            c.PARM_SIGNATURE,
                            c.RESULT_SETS,
                            c.FUNCTION_TYPE,
                            c.ROUTINETYPE,
                            c.RETURN_TYPE,
                            c.ORIGIN,
                            c.SOURCESCHEMA,
                            c.SOURCESPECIFIC,
                            c.DETERMINISTIC,
                            c.REMARKS
                    FROM    SYSIBM.SYSROUTINES c LEFT JOIN
                                (SELECT SCHEMA,
                                        NAME,
                                        COUNT(*) AS overload_count
                                FROM    SYSIBM.SYSROUTINES
                                {0}
                                GROUP BY SCHEMA, NAME
                                HAVING count(*) > 1) o
                            ON c.SCHEMA = o.SCHEMA AND c.NAME = o.NAME
                    {1}
                    ORDER BY c.SCHEMA, c.NAME",
                    schemaFilterInner,
                    schemaFilterOuter);

                using (var cmd = new DB2Command(sql, conn)) {
                    using (DB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)) {
                        while (reader.Read()) {
                            var properties = new List<ExtendedProperty> {
                                ExtendedProperty.Readonly(ExtendedPropertyNames.ObjectID, reader.GetInt32(POS_PROCEDUREID)),
                                ExtendedProperty.Readonly(ExtendedPropertyNames.SpecificName, reader.GetString(POS_SPECIFICNAME)),
                                ExtendedProperty.Readonly(ExtendedPropertyNames.Comment, reader.GetString(POS_REMARKS)),
                                ExtendedProperty.Readonly("CS_Definer", reader.GetString(POS_DEFINER)),
                                ExtendedProperty.Readonly("CS_Language", reader.GetString(POS_LANGUAGE)),
                                ExtendedProperty.Readonly(ExtendedPropertyNames.NumberOfResultSets, reader.GetInt16(POS_RESULTSETS))
                            };
                            
                             //int ordinal = reader.GetOrdinal("REMARKS");
                             //string comments = reader.IsDBNull(ordinal) ? String.Empty : reader.GetString(ordinal);
                             //reader.GetString(POS_NULLABLE) == "Y",
                            string routineType = reader.GetString(POS_ROUTINETYPE);
                            bool isFunction = String.Equals(routineType, "F", StringComparison.OrdinalIgnoreCase); // User-defined function or cast function
                            if (isFunction && !database.IncludeFunctions) {
                                ExtendedProperty.Readonly(ExtendedPropertyNames.ObjectType, routineType);
                                continue;
                            }

                            // Origin
                            //E: External routine or external SQL procedure
                            //N: Native SQL procedure
                            //Q: SQL function
                            //S: System-generated function
                            //U: Sourced on user-defined function or built-in function
                            string origin = reader.GetString(POS_ORIGIN);
                            ExtendedProperty.Readonly(ExtendedPropertyNames.ObjectType, origin);
                              
                            // Function Type.
                            // C: Aggregate function
                            // S: Scalar function
                            // T: Table function
                            // blank: For a stored procedure (ROUTINETYPE = 'P')
                            string funtionType = reader.GetString(POS_FUNCTION_TYPE);

                            // Internal identifier of the result data type of the function. The column contains a -2 if the function is a table function.
                            bool isTableValuedFunction = reader.GetInt32(POS_RETURN_TYPE) == -2 || String.Equals(funtionType, "T", StringComparison.OrdinalIgnoreCase);
                            bool isAggregateFunction = String.Equals(funtionType, "C", StringComparison.OrdinalIgnoreCase);
                            bool isScalarFunction = String.Equals(funtionType, "S", StringComparison.OrdinalIgnoreCase);

                            // TODO: Update this to see what language the procedure was written in.
                            // Add an extended property for Assembly (CLR) stored-procedure.
                            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsCLR, false));

                            // Add an extended property for SQL Aggregate function.
                            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsAggregateFunction, isAggregateFunction));

                            // Add an extended property for SQL scalar function.
                            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsScalarFunction, isScalarFunction));

                            // Add an extended property for SQL table-valued-function.
                            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsTableValuedFunction, isTableValuedFunction));

                            // Add an extended property for SQL inline table-valued function.
                            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsInlineTableValuedFunction, false));

                            // Add an extended property for SQL multi statement table-valued function.
                            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsMultiStatementTableValuedFunction, false));

                            // If ORIGIN is 'U' and ROUTINETYPE is 'F', the schema of the source user-defined function ('SYSIBM' for a source built-in function). Otherwise, the value is blank.
                            // If ORIGIN is 'U' and ROUTINETYPE is 'F', the specific name of the source user-defined function or source built-in function name. Otherwise, the value is blank.	G
                            if (String.Equals(origin, "U", StringComparison.OrdinalIgnoreCase) && String.Equals(routineType, "F", StringComparison.OrdinalIgnoreCase)) {
                                properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.SourceSchemaOwner, reader.GetString(POS_SOURCESCHEMA)));
                                properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.SourceSchemaName, reader.GetString(POS_SOURCESPECIFIC)));
                            }

                            // The deterministic option of an external function or a stored procedure.
                            string deterministic = reader.GetString(POS_DETERMINISTIC);
                            if(!String.IsNullOrEmpty(deterministic))
                                properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsDeterministic, String.Equals(reader.GetString(POS_DETERMINISTIC), "Y", StringComparison.OrdinalIgnoreCase)));
                         
                            string name = reader.GetString(POS_PROCNAME);
                            
                            // Tack on overload # into the name as a means of supporting overloads in CodeSmith.
                            // Template writers will need to use the CS_Name property to get just the command name value,
                            // but this at least allows code to be generated that makes use of this great feature.
                            if (!reader.IsDBNull(POS_OVERLOADCOUNT)) {
                                // Insert the hex representation of the hash code of the signature string in the syscat.
                                // This will result in a consistent name for the procedure based on the signature, 
                                // even if the procedure is dropped and recreated.
                                long readLength = reader.GetBytes(POS_PARMSIGNATURE, 0, null, 0, 180);
                                var signatureBytes = new byte[readLength];
                                reader.GetBytes(POS_PARMSIGNATURE, 0L, signatureBytes, 0, Convert.ToInt32(readLength));

                                var encoding = new UnicodeEncoding();
                                string signatureText = encoding.GetString(signatureBytes);

                                properties.Add(ExtendedProperty.Readonly("CS_OverloadedName", String.Format("{0}[{1}]", name, signatureText.GetHashCode().ToString("X"))));
                            }

                            //if (DB2zOSConfiguration.Instance.FilterCommands.Length > 0 && DB2zOSConfiguration.Instance.FilterCommands != DB2zOSConfiguration.Instance.FilterTables)
                            //{
                            //   name = DB2zOSConfiguration.Instance.FilterCommands + "." + name;
                            //}

                            var commandSchema = new CommandSchema(
                                database,
                                name,
                                reader.GetString(POS_OWNER).Trim(),
                                reader.GetDateTime(POS_CREATED),
                                properties.ToArray());

                            commands.Add(commandSchema);
                        }
                    }
                }
            }

            return commands.ToArray();
        }

        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            var parameters = new List<ParameterSchema>();
            var specificName = (string)command.ExtendedProperties[ExtendedPropertyNames.SpecificName].Value;

            using (var conn = new DB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                const int POS_PARM_NAME = 0;
                const int POS_TYPE_NAME = 1;
                const int POS_IN_OUT = 2;
                //Use GetDBTypeLength function which uses named parameters rather than positional parameters.  LENGTH and SCALE.
                //const int POS_DATA_LENGTH	= 3;
                const int POS_DATA_SCALE = 4;

                //TYEPNAME is RTRIM()'d becuase "CHAR" and some other TYPES have trailing spaces on them.
                string sql = String.Format(@"
                    SELECT  RTRIM(PARMNAME) AS PARMNAME,
                            RTRIM(TYPENAME) AS DBTYPE,
                            ROWTYPE,
                            LENGTH,
                            SCALE
                    FROM    SYSIBM.SYSPARMS
                    WHERE   SPECIFICNAME = '{0}'
                    ORDER BY ORDINAL", specificName);

                using (var cmd = new DB2Command(sql, conn))
                {
                    using (DB2DataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string parmName = reader.GetString(POS_PARM_NAME);
                            string typeName = reader.GetString(POS_TYPE_NAME);
                            int length = GetDbTypeLength(reader);
                            int scale = Convert.ToInt32(reader.GetInt16(POS_DATA_SCALE));
                            byte precision = GetDbTypePrecision(reader);

                            var parameterSchema = new ParameterSchema(
                                command,
                                parmName,
                                GetParameterDirection(false, reader.GetString(POS_IN_OUT)),
                                GetDbType(reader),
                                typeName,
                                length,
                                precision,
                                scale,
                                true);

                            parameters.Add(parameterSchema);
                        }
                    }
                }
            }

            return parameters.ToArray();
        }

        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command) {
            // Initialize array to size 0 for now
            var commandResultSchemas = new List<CommandResultSchema>();

            // Make sure the user has explicitly allowed this operation to proceed...
            if (!DB2zOSConfiguration.Instance.AllowGetCommandResultSchemas)
                throw new InvalidOperationException(
                    "The DB2 zOS Schema Provider supports reading column schema information. Please set AllowGetCommandResultSchemas to true in the Generator Options -> Schema Provider -> DB2 zOS page as its usage can change the state of the server.");

            //Comment out until we decide if we need to support RefCurors
            //int refCursorCount = 0;
            // How many result sets are expected?
            var resultsetCount = command.ExtendedProperties.GetByKey<int>(ExtendedPropertyNames.NumberOfResultSets);

            // No resultsets expected, so return what we have now (empty array).
            if (resultsetCount != 0)
            {
                // Keep a count as to how many results are expected
                // int refCursorCount = 0;

                using (var conn = new DB2Connection(connectionString))
                {
                    // Open the connection
                    conn.Open();

                    using (var cmd = new DB2Command(command.FullName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        //var resultSchemaNames = new List<string>();

                        // Set up the parameters will null values
                        foreach (ParameterSchema parameterSchema in command.Parameters)
                        {
                            var parm = new DB2Parameter
                            {
                                ParameterName = parameterSchema.Name,
                                DbType = parameterSchema.DataType,
                                Direction = parameterSchema.Direction
                            };

                            // If the parameter is not an object or REF CURSOR
                            if (parm.DbType != DbType.Object) // || parameterSchema.NativeType != "REF CURSOR")
                            {
                                // Assign a null value for starters
                                parm.Value = DBNull.Value;

                                switch (parm.DbType)
                                {
                                    case DbType.AnsiString:
                                    case DbType.AnsiStringFixedLength:
                                    case DbType.String:
                                    case DbType.StringFixedLength:
                                        parm.Size = parameterSchema.Size;
                                        parm.Value = String.Empty;
                                        break;
                                    case DbType.Boolean:
                                    case DbType.Byte:
                                    case DbType.Currency:
                                    case DbType.Decimal:
                                    case DbType.Double:
                                    case DbType.Int16:
                                    case DbType.Int32:
                                    case DbType.Int64:
                                    case DbType.SByte:
                                    case DbType.Single:
                                    case DbType.UInt16:
                                    case DbType.UInt32:
                                    case DbType.UInt64:
                                    case DbType.VarNumeric:
                                        parm.Value = 0;
                                        break;
                                    case DbType.Date:
                                    case DbType.Time:
                                    case DbType.DateTime:
                                    case DbType.DateTime2:
                                        parm.Value = new DateTime(1967, 1, 1);
                                        break;
                                    //case DbType.Time:
                                    //   parm.Value = new TimeSpan(0, 0, 0);
                                    //   break;
                                    case DbType.Guid:
                                        throw new NotSupportedException("Encountered unsupported DbType.Guid.");
                                    default:
                                        throw new NotSupportedException(
                                            String.Format("Encountered unsupported type 'DbType.{0}'", parm.DbType.ToString()));
                                }
                            }
                            //Comment this out until we can determine if we need to support Ref_cursors.
                            //else
                            //{
                            //   // Keep track of the ref cursor variable names to use for naming the resultset
                            //   resultSchemaNames.Add(parameterSchema.Name);

                            //   // Set the ref cursor type
                            //   parm.DB2Type = DB2Type.Cursor;

                            //   //HACK: An exception will occur if the parameter is an Input/Output directional state ( http://forums.asp.net/t/1114447.aspx ).
                            //   if (parm.Direction == ParameterDirection.InputOutput)
                            //   {
                            //      parm.Direction = ParameterDirection.Output;
                            //   }

                            //   // Make note as to how many refcursor parms we have
                            //   refCursorCount++;
                            //}

                            // Add the parameter to the command for execution
                            cmd.Parameters.Add(parm);
                        }

                        // Will there be any results returned?
                        if (resultsetCount > 0)
                        //Comment out until we decide if we need to support RefCurors
                        //if (resultsetCount > 0 || refCursorCount > 0)
                        {
                            // Wrap the operation in a transaction to prevent as much change to the server as possible
                            using (DB2Transaction trans = conn.BeginTransaction())
                            {
                                // Assign the transaction to the command
                                cmd.Transaction = trans;

                                try
                                {
                                    // Ask for reader, but the schema only.
                                    using (DB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                                    //CommandBehavior.SchemaOnly | 
                                    {
                                        int colIndex = 0;
                                        int resultsetIndex = 0;

                                        // Create an array to hold information about all the resultsets
                                        //commandResultSchemas = new CommandResultSchema[resultsetCount];
                                        var commandResultSchemaList = new List<CommandResultSchema>();

                                        do
                                        {
                                            // Get schema info from reader
                                            DataTable schemaTable = reader.GetSchemaTable();

                                            // Create an array of result column schemas to store column information
                                            if (schemaTable != null)
                                            {
                                                // Create an array of result column schemas to store column information
                                                var resultColumnSchemas = new List<CommandResultColumnSchema>(schemaTable.Rows.Count);

                                                // Iterate through all rows in the schema table
                                                foreach (DataRow row in schemaTable.Rows)
                                                {
                                                    string nativeType;

                                                    // TODO: This needs work because we don't get Base Table/Column information. Native type information
                                                    // would have to be inferred from the DbType, which wouldn't be accurate.
                                                    try
                                                    {
                                                        // MAC 05-08-2013  Change SourceSchemaName to BaseSchemaName
                                                        string baseSchemaName = row["BaseSchemaName"].ToString();
                                                        string baseTableName = row["BaseTableName"].ToString();
                                                        string baseColumnName = row["BaseColumnName"].ToString();

                                                        if (!String.IsNullOrEmpty(baseSchemaName) && !String.IsNullOrEmpty(baseTableName))
                                                        {
                                                            TableSchema tableSchema = command.Database.Tables[baseSchemaName, baseTableName];
                                                            nativeType = tableSchema.Columns[baseColumnName].NativeType;
                                                        }
                                                        else
                                                        {
                                                            nativeType = "--UNKNOWN--";
                                                        }
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        nativeType = "--UNKNOWN--" + ex.Message;
                                                        Debug.WriteLine("***** Error Gettting Table and Column Result Schema Info : " + command.FullName + " Ex: "  + ex.Message);
                                                    }

                                                    byte numericPrecision = 0;

                                                    // GetSchemaTable method of IBM DB2 provider returns NumericPrecision equal to column length for VARCHAR columns (at the very least)
                                                    if ((Type)row["DataType"] != typeof(string))
                                                    {
                                                        numericPrecision = row.IsNull("NumericPrecision")
                                                            ? (byte)0
                                                            : Convert.ToByte(row["NumericPrecision"]);
                                                    }

                                                    var resultColumnSchema =
                                                        new CommandResultColumnSchema(
                                                            command,
                                                            (string)row["ColumnName"],
                                                            GetDbTypeFromSystemType((Type)row["DataType"]),
                                                            nativeType,
                                                            row.IsNull("ColumnSize")
                                                                ? 0
                                                                : Convert.ToInt32(row["ColumnSize"]),
                                                            numericPrecision,
                                                            row.IsNull("NumericScale")
                                                                ? 0
                                                                : Convert.ToInt32(row["NumericScale"]),
                                                            row.IsNull("AllowDBNull") || (bool)row["AllowDBNull"]);

                                                    // Assign the result column schema information to the array
                                                    //MAC 05-08-2013 change this to Add  resultColumnSchemas[colIndex++] = resultColumnSchema;
                                                    resultColumnSchemas.Add(resultColumnSchema);
                                                }

                                                // Construct a result schema for the results just processed
                                                resultsetIndex++;
                                                var commandResultSchema = new CommandResultSchema(command,
                                                    String.Format("ResultSet{0}", resultsetIndex),
                                                    resultColumnSchemas);

                                                // Assign the result schema for this resultset to the array
                                                commandResultSchemaList.Add(commandResultSchema);
                                                //commandResultSchemas[resultsetIndex++] = commandResultSchema;
                                            }
                                        } while (reader.NextResult());

                                        // Convert the array into an array of CommandResultSchema objects, for return
                                        commandResultSchemas = commandResultSchemaList;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Create an array of result column schemas to store column information
                                    Debug.WriteLine("****** Error " + ex.Message);

                                    var resultColumnSchemas = new List<CommandResultColumnSchema>(1) {
                                                                                                     new CommandResultColumnSchema
                                                                                                         (
                                                                                                         command,
                                                                                                         ex.Message,
                                                                                                         DbType.String,
                                                                                                         "VARCHAR",
                                                                                                         ex.Message.
                                                                                                             Length,
                                                                                                         0,
                                                                                                         0,
                                                                                                         true)
                                                                                                 };

                                    var commandResultSchema = new CommandResultSchema(command, "ErrorResultSet", resultColumnSchemas);
                                    commandResultSchemas = new List<CommandResultSchema> { commandResultSchema };
                                }
                            }
                        }
                    }
                } // if no results
            }
            return commandResultSchemas.ToArray();
        }

        public string GetCommandText(string connectionString, CommandSchema command) {
            var specificName = command.ExtendedProperties.GetByKey<string>(ExtendedPropertyNames.SpecificName);
            
            string sql =
                String.Format(
                    @"
                    SELECT TEXT 
                                FROM SYSIBM.SYSROUTINES 
                                WHERE SPECIFICNAME = '{0}'
                                    AND LENGTH(TEXT) > 0",
                    specificName);

            using (var conn = new DB2Connection(connectionString)) {
                // Open a connection
                conn.Open();

                using (var cmd = new DB2Command(sql, conn)) {
                    using (DB2DataReader reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            // Return the procedure text
                            return reader.GetString(0);
                        }
                        return String.Empty;
                    }
                }
            }
        }

        #endregion

        #region Misc. Methods

        public string GetDatabaseName(string connectionString) {
            // Open a connection
            using (var conn = new DB2Connection(connectionString)) {
                conn.Open();

                return conn.Database;
            }
        }

        private ParameterDirection GetParameterDirection(bool isReturnVal, string parmDirectionText) {
            var parmDirection = ParameterDirection.ReturnValue;
            if (!isReturnVal) {
                switch (parmDirectionText) {
                    case "P":
                        parmDirection = ParameterDirection.Input;
                        break;
                    case "O":
                        parmDirection = ParameterDirection.Output;
                        break;
                    case "B":
                        parmDirection = ParameterDirection.InputOutput;
                        break;
                }
            }

            return parmDirection;
        }

        private string GetFilterCriteria(string filter, string columnNameToFilter, bool includeLeadingAnd) {
            // filter of "*" means no filter at all, so exit with no filter
            if (!String.IsNullOrEmpty(filter) && filter != "*")
                return String.Format("{0}{1} = '{2}'", includeLeadingAnd ? "AND " : String.Empty, columnNameToFilter, filter);

            return null;
        }

        #endregion

        #region DbType Conversions

        private DbType GetDbType(DB2DataReader reader) {
            string nativeType = reader["DBTYPE"].ToString();

            int scale = 0;
            int precision = 0;

            try {
                scale = Convert.ToInt32(reader["SCALE"]);
            } catch {}

            if (String.Equals(nativeType, "DECIMAL", StringComparison.OrdinalIgnoreCase)) // LENGTH is precisions for DECIMAL type
            {
                try {
                    precision = Convert.ToInt32(reader["LENGTH"]);
                } catch {}
            } else {
                // Get precisions for other DB2 native types
                precision = Convert.ToInt32(GetDbTypePrecision(reader));
            }

            return GetDbType(nativeType, precision, scale);
        }

        private DbType GetDbType(string nativeType, int precision, int scale) {
            DbType result;

            // Convert native DB2 types to appropriate DbType enumerated values
            switch (nativeType.ToUpper().Trim()) {
                case "INTEGER":
                    result = DbType.Int32;
                    break;
                case "SMALLINT":
                    result = DbType.Int16;
                    break;
                case "FLOAT":
                    switch (precision) {
                        case 7:
                            result = DbType.Single;
                            break;
                        case 15:
                            result = DbType.Double;
                            break;
                        default:
                            result = DbType.Double;
                            break;
                    }
                    break;

                case "CHAR":
                    result = DbType.String;
                    //result = DbType.AnsiStringFixedLength; //DbType.StringFixedLength;
                    break;
                case "VARCHAR":
                case "LONGVAR":
                    result = DbType.String;
                    break;
                case "DECIMAL":
                    result = DbType.Decimal;
                    break;
                case "GRAPHIC":
                case "VARG":
                case "LONGVARG":
                case "BLOB":
                case "CLOB":
                case "DBCLOB":
                    result = DbType.Object;
                    break;
                case "DATE":
                    result = DbType.Date;
                    break;
                case "TIMESTMP":
                    result = DbType.DateTime2;
                    break;
                case "TIME":
                    result = DbType.DateTime;
                    break;
                    //ROWID - Row ID data type 
                    //DISTINCT - Distinct type		
                default:
                    result = DbType.Object;
                    break;
            }

            return result;
        }

        private DbType GetDbTypeFromSystemType(Type type) {
            if (type == typeof(Char))
                return DbType.AnsiStringFixedLength; // DbType.StringFixedLength, DbType.AnsiString, DbType.AnsiStringFixedLength
            
            if (type == typeof(String))
                return DbType.String; // DbType.StringFixedLength, DbType.AnsiString, DbType.AnsiStringFixedLength
            
            if (type == typeof(Boolean))
                return DbType.Boolean;
            
            if (type == typeof(Byte))
                return DbType.Byte;
            
            if (type == typeof(SByte))
                return DbType.SByte;
            
            if (type == typeof(Int16))
                return DbType.Int16;
            
            if (type == typeof(UInt16))
                return DbType.UInt16;
            
            if (type == typeof(Int32))
                return DbType.Int32;
           
            if (type == typeof(UInt32))
                return DbType.UInt32;
            
            if (type == typeof(Int64))
                return DbType.Int64;
            
            if (type == typeof(UInt64))
                return DbType.UInt64;
            
            if (type == typeof(Single))
                return DbType.Single;
            
            if (type == typeof(Double))
                return DbType.Double;
            
            if (type == typeof(Decimal))
                return DbType.Decimal;

            if (type == typeof(DateTime))
                return DbType.DateTime; // DbType.Date

            if (type == typeof(Byte[]))
                return DbType.Binary;

            //else if (type == typeof(TimeSpan))
            //   return DbType.Time;

            throw new NotSupportedException(String.Format("Conversion of System type '{0}' to a corresponding DbType is unhandled.", type));

            // Unhandled conversions:
            //		DbType.Currency:
            //		DbType.Guid:
            //		DbType.Object:
            //		DbType.VarNumeric:
        }

        private int GetDbTypeLength(DB2DataReader reader) {
            var nativeType = (string)reader["DBTYPE"];

            int length = 0;

            try {
                length = Convert.ToInt32(reader["LENGTH"]);
            } catch (Exception) {}

            return GetDbTypeLength(nativeType, length);
        }

        private int GetDbTypeLength(string dataTypeName, int length) {
            switch (dataTypeName) {
                case "INTEGER":
                case "SMALLINT":
                case "FLOAT":
                case "CHAR":
                case "VARCHAR":
                case "LONGVAR":
                case "DECIMAL":
                case "GRAPHIC":
                case "VARG":
                case "LONGVARG":
                case "DATA":
                case "DATE":
                case "TIME":
                case "TIMESTMP":
                case "BLOB":
                case "DBCLOB":
                case "CLOB":
                    return length;

                    //ROWID - 17 - The maximum length of the stored portion of the identifier. 
                    //DISTINCT - The length of the source data type. 
                default:
                    // Length is not relevant to other data types
                    return 0;
            }
        }

        private byte GetDbTypePrecision(DB2DataReader reader) {
            var nativeType = (string)reader["DBTYPE"];
            int length = 0;

            try {
                length = Convert.ToInt32(reader["LENGTH"]);
            } catch (Exception) {}

            return GetDbTypePrecision(nativeType, length);
        }

        private byte GetDbTypePrecision(string dataTypeName, int length) {
            int result;

            switch (dataTypeName) {
                case "DECIMAL":
                    result = length; // For DECIMAL types, length is the precision
                    break;
                case "REAL":
                    result = 7;
                    break;
                case "FLOAT":
                    switch (length) {
                        case 4:
                            result = 7;
                            break;
                        case 8:
                            result = 15;
                            break;
                        default:
                            result = 15;
                            break;
                    }
                    break;
                case "DOUBLE":
                    result = 15;
                    break;
                case "SMALLINT":
                    result = 5;
                    break;
                case "INTEGER":
                    result = 10;
                    break;
                case "BIGINT":
                    result = 19;
                    break;
                default:
                    result = 0;
                    break;
            }

            return Convert.ToByte(result);
        }

        #endregion
    }
}