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
//    Provider was written by Bill Hall <bhall@dayspring.com> and Dan Gowin <dang@dayspring.com> from DaySpring Cards (http://www.dayspring.com/).
//    Some original work may have been used by DaySpring as reference by Geoff McElhanon's DB2 Provider.
//
// Notes:
//    A user must have the IBM iSeries Access Client installed to use this provider.
//    Requires iSeries OS that is v5.4 or greater. This provider has been tested against v6.1.
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using IBM.Data.DB2.iSeries;

namespace SchemaExplorer
{
    public class ISeriesSchemaProvider : IDbSchemaProvider
    {
        public ISeriesSchemaProvider() { }

        #region IDbSchemaProvider Members

        /// <summary>
        /// Return the Constant name of this SchemaProvider
        /// </summary>
        public string Name
        {
            get { return "ISeriesSchemaProvider"; }
        }


        /// <summary>
        /// Describe yourself
        /// </summary>
        public string Description
        {
            get
            {
                return "iSeries Schema Provider";
            }
        }

        #endregion

        #region Database Info

        /// <summary>
        /// Returns the DB name 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public string GetDatabaseName(string connectionString)
        {
            string userName = null;

            // Open a connection
            using (var conn = new iDB2Connection(connectionString))
            {
                conn.Open();
                string databaseName = conn.Database;

                // Parse the connect string
                Hashtable connectStringParms = ParseConnectStringToHashTable(connectionString);

                // Look for the user name in acceptable forms
                if (connectStringParms["user id"] != null)
                    userName = (string)connectStringParms["user id"];
                if (connectStringParms["uid"] != null)
                    userName = (string)connectStringParms["uid"];
                if (connectStringParms["user"] != null)
                    userName = (string)connectStringParms["user"];

                // If username was found, append it to the database name in parenthesis
                return !String.IsNullOrEmpty(userName) ? String.Format("{0} (as {1})", databaseName, userName) : databaseName;
            }
        }

        private static Hashtable ParseConnectStringToHashTable(string connectString)
        {
            string[] nameValuePairs = connectString.Split(';');
            var ht = new Hashtable(nameValuePairs.Length);
            foreach (string s in nameValuePairs)
            {
                if (s.Length > 0)
                {
                    string[] nameAndValue = s.Split('=');
                    if (nameAndValue.Length >= 2)
                    {
                        ht.Add(nameAndValue[0].ToLower(), nameAndValue[1]);
                    }
                }
            }

            return ht;
        }


        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            // Get the schema object name and type name to be used to retrieve the extended property.
            string containerObjectOwner;
            string objectName = schemaObject.Name;
            string schemaObjectTypeName = schemaObject.GetType().Name;

            switch (schemaObjectTypeName)
            {
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

                case "CommandSchema":
                    containerObjectOwner = ((CommandSchema)schemaObject).Owner;
                    break;

                case "DatabaseSchema":
                    objectName = "Database"; // TODO - Use user name here instead?

                    containerObjectOwner = "Database";
                    break;

                case "IndexSchema":
                    containerObjectOwner = ((IndexSchema)schemaObject).Table.Owner;  // SchemaExplorer ignores index ownership, use the table's owner instead.
                    break;

                case "ParameterSchema":
                    var parameter = (ParameterSchema)schemaObject;

                    containerObjectOwner = parameter.Command.Owner;
                    objectName = (string)parameter.Command.ExtendedProperties["CS_Name"].Value + "." + parameter.Name;

                    break;

                case "PrimaryKeySchema":
                    containerObjectOwner = ((PrimaryKeySchema)schemaObject).Table.Owner; // SchemaExplorer ignores constraint ownership, use the table's owner instead.
                    break;

                case "TableKeySchema":
                    containerObjectOwner = ((TableKeySchema)schemaObject).ForeignKeyTable.Owner; // SchemaExplorer ignores constraint ownership, use the foreign key table owner instead.
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
                    SELECT	property_name, property_value, clr_type
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

            // Instantiate an ArrayList to hold the extended properties temporarily
            var propertyList = new ArrayList();

            using (var conn = new iDB2Connection(connectionString))
            {
                conn.Open();

                try
                {
                    using (var cmd = new iDB2Command(sql, conn))
                    {
                        using (iDB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (reader.Read())
                            {
                                // Pull all the values into variables
                                string propertyName = reader.GetString(POS_PROPERTY_NAME);
                                string propertyValueText = reader.GetString(POS_PROPERTY_VALUE);
                                string clrTypeName = reader.GetString(POS_CLR_TYPE);

                                // Need to handle the two DBTypes that might occur.
                                switch (clrTypeName)
                                {
                                    case "AnsiString":
                                        clrTypeName = "System.String";
                                        break;
                                    case "Currency":
                                        clrTypeName = "System.Double";
                                        break;
                                    default:
                                        clrTypeName = "System." + clrTypeName;
                                        break;
                                }

                                // Get the System type for the value
                                Type clrType = Type.GetType(clrTypeName);

                                // Convert the string representation of the property to the appropriate CLR type
                                if (clrType != null)
                                {
                                    object propertyValue = Convert.ChangeType(propertyValueText, clrType);

                                    // Construct and add the extended property 
                                    propertyList.Add(new ExtendedProperty(propertyName, propertyValue, GetDbTypeFromSystemType(clrType)));
                                }
                            }
                        }
                    }
                }
                catch (iDB2Exception ex)
                {
                    // Check for the iDB2 exception that indicates that the table isn't there
                    // TODO: Change to iDB2 specific message
                    if (ex.Errors[0].MessageCode == -204 && ex.Message.IndexOf("CODESMITH") >= 0)
                    {
                        // Should we try to automatically create the table?
                        if (iSeriesConfiguration.Instance.AutoCreateExtendedPropertiesTable)
                        {
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

                // Return the extended properties as an array
                return (ExtendedProperty[])propertyList.ToArray(typeof(ExtendedProperty));
            }
        }

        /// <summary>
        /// Converts the provided system type into the corresponding DbType for CodeSmith's sake.
        /// </summary>
        /// <param name="type">The System type to be converted.</param>
        /// <returns>The corresponding DbType.</returns>
        private static DbType GetDbTypeFromSystemType(Type type)
        {
            if (type == typeof(String))
            {
                return DbType.AnsiString; // , DbType.AnsiStringFixedLength
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
                return DbType.DateTime; // DbType.Date
            }
            if (type == typeof(TimeSpan))
            {
                return DbType.Time;
            }
            if (type == typeof(Byte[]))
            {
                return DbType.Binary;
            }

            throw new NotSupportedException(String.Format("Conversion of System type '{0}' to a corresponding DbType is unhandled.", type));
        }

        #endregion

        #region TABLE Info

        /// <summary>
        /// Get the Array of tables...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            var tableList = new ArrayList();

            using (var conn = new iDB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                const int POS_OWNER = 0;
                const int POS_NAME = 1;
                const int POS_CREATED = 2;
                const int POS_TYPE = 3;
                const int POS_BASE_TABLE_SCHEMA = 4;
                const int POS_BASE_TABLE_NAME = 5;

                // what used to be in the SQL
                //SELECT		tabschema, tabname, create_time, type, base_tabschema, base_tabname 
                //FROM		syscat.tables
                //WHERE		type IN ('T', 'A')
                //            AND status = 'N'
                //            {0}
                //ORDER BY	tabschema, 
                //            tabname", 

                string sql = String.Format(@"
                    SELECT
                        table_schema,
                        table_name,
                        LAST_ALTERED_TIMESTAMP,
                        type,
                        coalesce(base_table_schema, '') as base_table_schema,
                        coalesce(base_table_name, '') as base_table_name 
                    FROM
                        qsys2.systables
                    WHERE
                        type IN ('T', 'A', 'P', 'L')
                        {0}
                        {1}
                    ORDER BY
                        table_schema,
                        table_name",
                        GetSchemaFilterCriteria("table_schema", true),
                        GetTableFilterCriteria("table_name", true));

                using (var cmd = new iDB2Command(sql, conn))
                {
                    cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    using (iDB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            // Exclude the extended properties table, if appropriate
                            if (reader.GetString(POS_NAME).ToUpper() != "CODESMITH_EXTENDED_PROPERTIES")
                            {
                                var extendedPropertyList = new ArrayList();
                                string type = reader.GetString(POS_TYPE);
                                string typeDesc;

                                switch (type)
                                {
                                    case "T":
                                    case "L":
                                    case "P":
                                        typeDesc = "Table";
                                        break;
                                    case "A":
                                        typeDesc = "Alias";
                                        break;
                                    default:
                                        typeDesc = type;
                                        break;
                                }

                                extendedPropertyList.Add(new ExtendedProperty(ExtendedPropertyNames.SystemType, typeDesc, DbType.String));

                                if (type == "A") // It's an alias
                                {
                                    // Add two more properties for table schema and name (to be used later as criteria for table related queries)
                                    extendedPropertyList.Add(new ExtendedProperty("CS_BaseTableSchema", reader.GetString(POS_BASE_TABLE_SCHEMA), DbType.String));
                                    extendedPropertyList.Add(new ExtendedProperty("CS_BaseTableName", reader.GetString(POS_BASE_TABLE_NAME), DbType.String));
                                }

                                var tableSchema = new TableSchema(
                                    database,
                                    reader.GetString(POS_NAME),
                                    reader.GetString(POS_OWNER).Trim(),
                                    reader.GetDateTime(POS_CREATED),
                                    (ExtendedProperty[])extendedPropertyList.ToArray(typeof(ExtendedProperty)));

                                tableList.Add(tableSchema);
                            }
                        }
                    }
                }
            }

            return (TableSchema[])tableList.ToArray(typeof(TableSchema));
        }


        /// <summary>
        /// Get all the Indexes for a given Table
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        /// 
        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            // Get underlying schema/name info (handle aliases)
            string tableSchema, tableName;
            GetTableInfo(table, out tableSchema, out tableName);

            string sql = String.Format(@"
                Select ind.INDEX_SCHEMA, ind.TABLE_NAME, ind.INDEX_NAME INDNAME, ind.IS_UNIQUE UNIQUERULE,
  indexcol.COLUMN_NAME COLNAME, indexcol.COLUMN_POSITION, indexcol.ORDINAL_POSITION,
  indexcol.ORDERING, indexcol.SYSTEM_COLUMN_NAME, indexcol.COLUMN_IS_EXPRESSION,
  indexcol.EXPRESSION_HAS_UDF, indexcol.KEY_EXPRESSION
From qsys2.sysindexes ind Inner Join
  qsys2.syskeys indexcol On ind.INDEX_NAME = indexcol.INDEX_NAME And
    ind.INDEX_SCHEMA = indexcol.INDEX_SCHEMA
WHERE
    ind.table_schema = '{0}' and ind.table_name = '{1}'
Order By ind.INDEX_NAME, indexcol.ordinal_position",
                tableSchema,
                tableName);

            // Removed:
            // idx.table_owner = '{0}'

            // Collections to hold index information
            var indexMemberCollection = new NameValueCollection();
            var isUniqueByIndexName = new Hashtable();
            var isPrimaryByIndexName = new Hashtable();
            var isClusteredByIndexName = new Hashtable();

            using (var conn = new iDB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                using (var cmd = new iDB2Command(sql, conn))
                {
                    cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    using (iDB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        //						int constraintTypeOrdinal = reader.GetOrdinal("CONSTRAINT_TYPE");
                        //						int tableTypeOrdinal = reader.GetOrdinal("TABLE_TYPE");

                        while (reader.Read())
                        {
                            // Add the column to the collection by index name
                            indexMemberCollection.Add((string)reader["INDNAME"], (string)reader["COLNAME"]);

                            // Determine if index is unique
                            bool isUnique = (reader["UNIQUERULE"].ToString() == "U");
                            isUniqueByIndexName[reader["INDNAME"]] = isUnique;

                            // Determine if index is the primary key index
                            bool isPrimary = (reader["UNIQUERULE"].ToString() == "P"); //(!reader.IsDBNull(constraintTypeOrdinal) && (string) reader["CONSTRAINT_TYPE"] == "P");
                            isPrimaryByIndexName[reader["INDNAME"]] = isPrimary;

                            // Determine if the index is on a TABLE or CLUSTER
                            // NOTE: A Microsoft® SQL Server™ clustered index is not like an Oracle cluster. 
                            // An Oracle cluster is a physical grouping of two or more tables that share the 
                            // same data blocks and use common columns as a cluster key. SQL Server does not 
                            // have a structure that is similar to an Oracle cluster.
                            //bool isClustered = ((string) reader["INDEXTYPE"] == "CLUS");
                            isClusteredByIndexName[reader["INDNAME"]] = false; // iSeries doesn't do Clustered
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
                        indexMemberCollection.GetValues(indexName));

                    indexSchemas[i++] = indexSchema;
                }
            }

            // Return the array of indexes
            return indexSchemas;
        }

        /// <summary>
        /// Return the Columns for a given Table.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            var columnList = new ArrayList();

            using (var conn = new iDB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                const int POS_COLUMN_NAME = 0;
                const int POS_DATA_TYPE = 1;
                // const int POS_DATA_LENGTH = 2; // LENGTH is precision for DECIMALS
                const int POS_DATA_SCALE = 3;
                const int POS_NULLABLE = 4;
                const int POS_IDENTITY = 5;
                // const int POS_COMMENTS = 6;

                // Get underlying schema/name info (handle aliases)
                string tableSchema, tableName;
                GetTableInfo(table, out tableSchema, out tableName);

                string sql = String.Format(@"
                    SELECT	name, 
                            coltype as typename, 
                            length, 
                            coalesce(scale,0) as scale, 
                            nulls, 
                            identity,
                            remarks
                    FROM	qsys2.syscolumns
                    WHERE	table_schema = '{0}'
                            AND TBNAME = '{1}'
                    ORDER BY colno",
                    tableSchema,
                    tableName);

                using (var cmd = new iDB2Command(sql, conn))
                {
                    cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    using (iDB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            var properties = new ExtendedPropertyCollection();
                            properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsIdentity, (reader.GetString(POS_IDENTITY) == "YES"), DbType.Boolean));
                            properties.Add(CreateExtendedPropertyForComments(reader));

                            // Get precision
                            byte precision = GetDBTypePrecision(reader);
                            Int32 scale = Convert.ToInt32(reader[POS_DATA_SCALE]);
                            bool isNullable = reader.GetString(POS_NULLABLE) == "Y";

                            var cs = new ColumnSchema(
                                table,
                                reader.GetString(POS_COLUMN_NAME),
                                GetDbType(reader),
                                reader.GetString(POS_DATA_TYPE),
                                GetDBTypeLength(reader),
                                precision,
                                scale,
                                isNullable,
                                properties.ToArray());

                            columnList.Add(cs);
                        }
                    }
                }
            }

            // Return the array of columns
            return (ColumnSchema[])columnList.ToArray(typeof(ColumnSchema));
        }

        /// <summary>
        /// Return the Foreign key info for a given table...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            var tableSchemaList = new ArrayList();

            using (var conn = new iDB2Connection(connectionString))
            {
                // Get underlying schema/name info (handle aliases)
                string tableSchema, tableName;
                GetTableInfo(table, out tableSchema, out tableName);

                // Open a connection
                conn.Open();

                string sql = String.Format(@"
                    Select child.CONSTRAINT_NAME As constraint_name, parent.TABLE_NAME As
                        parent_table_name, parent.COLUMN_NAME As parent_column_name, child.TABLE_NAME As
                        child_table_name, child.COLUMN_NAME As child_column_name, child.ORDINAL_POSITION As position
                    From
                        QSYS2.SYSKEYCST child
                            Inner Join QSYS2.SYSREFCST crossref On
                                child.CONSTRAINT_SCHEMA = crossref.CONSTRAINT_SCHEMA And
                                child.CONSTRAINT_NAME = crossref.CONSTRAINT_NAME
                            Inner Join QSYS2.SYSKEYCST parent On
                                crossref.UNIQUE_CONSTRAINT_SCHEMA = parent.CONSTRAINT_SCHEMA And crossref.UNIQUE_CONSTRAINT_NAME = parent.CONSTRAINT_NAME
                    Where
                        (child.TABLE_NAME = '{1}' or parent.Table_name = '{1}') And
                        child.TABLE_SCHEMA = '{0}' And
                        child.ORDINAL_POSITION = parent.ORDINAL_POSITION
                    ORDER BY
                        child.CONSTRAINT_NAME, parent.ordinal_position",
                    tableSchema,
                    tableName);

                var da = new iDB2DataAdapter(sql, conn) { SelectCommand = { CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout } };

                var ds = new DataSet();
                using (ds)
                {
                    da.Fill(ds);

                    string lastConstraint = String.Empty;
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (lastConstraint != row["constraint_name"].ToString())
                        {
                            lastConstraint = row["constraint_name"].ToString();

                            string pkTable = row["parent_table_name"].ToString();
                            string fkTable = row["child_table_name"].ToString();

                            var distinctColumns = ds.Tables[0].Select(String.Format("constraint_name = '{0}'", lastConstraint));
                            var fkMemberCols = new ArrayList();
                            var pkMemberCols = new ArrayList();
                            foreach (DataRow rdc in distinctColumns)
                            {
                                fkMemberCols.Add(rdc["child_column_name"].ToString());
                                pkMemberCols.Add(rdc["parent_column_name"].ToString());
                            }

                            try
                            {
                                var tks = new TableKeySchema(table.Database, lastConstraint, (string[])fkMemberCols.ToArray(typeof(string)), fkTable, (string[])pkMemberCols.ToArray(typeof(string)), pkTable);
                                tableSchemaList.Add(tks);
                            }
                            catch (Exception ex)
                            {
                                // Catch the exception.
                                throw new InvalidOperationException("Unable to add foreign keys.  Check that related schemas are in the filter schema list in options.  Error:\n\n" + ex.Message);
                            }
                        }
                    }
                }
            }

            return (TableKeySchema[])tableSchemaList.ToArray(typeof(TableKeySchema));
        }

        /// <summary>
        /// Return the PK for a given Table...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            PrimaryKeySchema primaryKeySchema = null;

            using (var conn = new iDB2Connection(connectionString))
            {
                // Get underlying schema/name info (handle aliases)
                string tableSchema, tableName;
                GetTableInfo(table, out tableSchema, out tableName);

                // Open a connection
                conn.Open();

                string sql = String.Format(@"
                    SELECT
                        indexes.constraint_name constraint_name,
                        cols.column_name column_name,
                        columns.ORDINAL_POSITION position
                    FROM
                        qsys2.syscst indexes INNER JOIN qsys2.syscstcol cols ON
                            (indexes.constraint_schema = cols.constraint_schema and indexes.constraint_name = cols.constraint_name)
                        INNER JOIN qsys2.syscolumns columns ON
                            (cols.column_name = columns.column_name and cols.constraint_schema = columns.table_schema and cols.table_name = columns.table_name)
                    WHERE
                        indexes.type = 'PRIMARY KEY' AND
                        indexes.table_name = '{1}' AND
                        indexes.table_schema = '{0}'
                    Order By
                        indexes.constraint_name, columns.ordinal_position",
                        tableSchema,
                        tableName);

                var da = new iDB2DataAdapter(sql, conn);

                var ds = new DataSet();
                da.SelectCommand.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                int rowCount = da.Fill(ds);

                if (rowCount > 0)
                {
                    var memberCols = new ArrayList();

                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        memberCols.Add(row["column_name"].ToString());
                    }

                    primaryKeySchema = new PrimaryKeySchema(table,
                                                 ds.Tables[0].Rows[0]["constraint_name"].ToString(),
                                                 (string[])memberCols.ToArray(typeof(string)));
                }
                else
                {
                    ds.Dispose();
                    da.Dispose();

                    // Check for legacy (DDS) defined file

                    string sql1 =
                                 string.Format(
                            @"
                                SELECT
                                    dbkfil,
                                    dbkfld,
                                    dbkpos
                                FROM
                                    qsys.QADBKATR
                                WHERE
                                    dbklib = '{0}' AND
                                    dbkfil = '{1}'
                                ORDER BY 
                                   dbkfil, dbkpos",
                            tableSchema,
                            tableName);
                    var da1 = new iDB2DataAdapter(sql1, conn);

                    var ds1 = new DataSet();
                    int rowCount1 = da1.Fill(ds1);

                    if (rowCount1 > 0)
                    {
                        var memberCols1 = new ArrayList();

                        foreach (DataRow row in ds1.Tables[0].Rows)
                        {
                            memberCols1.Add(((row["dbkfld"].ToString().Trim())));
                        }

                        primaryKeySchema = new PrimaryKeySchema(table,
                                                            "PK_" + tableName,
                                                            (string[])memberCols1.ToArray(typeof(string)));
                    }
                    else
                    {
                        ds1.Dispose();
                        da1.Dispose();

                        string sql2 = String.Format(@"
                                SELECT
                                    OBJECT_NAME,
                                    PROPERTY_VALUE
                                FROM
                                    {0}CODESMITH_EXTENDED_PROPERTIES
                                WHERE
                                    CONTAINER_OBJECT_OWNER = '{1}' AND
                                    OBJECT_NAME like '{2}.%' AND 
                                    CODESMITH_SCHEMA_TYPE = 'ColumnSchema' AND
                                    upper(PROPERTY_NAME) = '{3}' AND
                                    upper(PROPERTY_VALUE) = 'TRUE'",
                                GetExtendedPropertiesTableOwnerPrefix(),
                                tableSchema,
                                tableName,
                                iSeriesConfiguration.Instance.FakePrimaryKey.ToUpper());

                        var da2 = new iDB2DataAdapter(sql2, conn);

                        var ds2 = new DataSet();
                        da2.SelectCommand.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                        int rowCount2 = da2.Fill(ds2);

                        if (rowCount2 > 0)
                        {
                            var memberCols2 = new ArrayList();

                            foreach (DataRow row in ds2.Tables[0].Rows)
                            {
                                string currentCol = row["OBJECT_NAME"].ToString();
                                int dotIndex = currentCol.IndexOf(".");
                                currentCol = currentCol.Substring(dotIndex + 1);

                                memberCols2.Add(currentCol);
                            }

                            primaryKeySchema = new PrimaryKeySchema(table, "PK_" + tableName + "_fake", (string[])memberCols2.ToArray(typeof(string)));
                        }
                    }
                }
            }

            return primaryKeySchema;
        }


        /// <summary>
        /// Return the data from a table in a System.DataTable object...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            using (var conn = new iDB2Connection(connectionString))
            {
                // Create and fill the data table
                var dt = new DataTable(table.Name);
                string sql = String.Format("SELECT * FROM {0}.{1}", table.Owner, table.Name);
                using (var adp = new iDB2DataAdapter(sql, conn))
                {
                    adp.SelectCommand.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    adp.Fill(dt);
                }

                return dt;
            }
        }

        private void GetTableInfo(TableSchema table, out string tableSchema, out string tableName)
        {
            if ((string)table.ExtendedProperties[ExtendedPropertyNames.SystemType].Value == "Alias")
            {
                tableSchema = (string)table.ExtendedProperties["CS_BaseTableSchema"].Value;
                tableName = (string)table.ExtendedProperties["CS_BaseTableName"].Value;
            }
            else
            {
                tableSchema = table.Owner;
                tableName = table.Name;
            }
        }

        #endregion

        #region VIEW Info

        /// <summary>
        /// Return array of Views
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            var viewList = new ArrayList();

            using (var conn = new iDB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                const int POS_OWNER = 0;
                const int POS_NAME = 1;
                //				const int POS_CREATED = 2;

                string sql = String.Format(@"
                    SELECT		table_schema, table_name
                    FROM		qsys2.sysviews
                    WHERE		1=1
                                {0}
                                {1}
                    ORDER BY	table_schema, 
                                table_name",
                    GetSchemaFilterCriteria("table_schema", true), GetViewFilterCriteria("table_name", true));

                using (var cmd = new iDB2Command(sql, conn))
                {
                    cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    using (iDB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            // Exclude the extended properties table, if appropriate
                            ViewSchema viewSchema = new ViewSchema(database, reader.GetString(POS_NAME), reader.GetString(POS_OWNER).Trim(), DateTime.MinValue);
                            viewList.Add(viewSchema);
                        }
                    }
                }
            }

            return (ViewSchema[])viewList.ToArray(typeof(ViewSchema));
        }


        /// <summary>
        /// Returns the Text of a View's definition
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public string GetViewText(string connectionString, ViewSchema view)
        {
            string viewText;

            using (var conn = new iDB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                // Get SQL to retrieve source for the view
                string sql = String.Format(@"
                    Select
                        QSYS2.SYSVIEWS.VIEW_DEFINITION text
                    From
                        QSYS2.SYSVIEWS
                    Where
                        QSYS2.SYSVIEWS.SYSTEM_VIEW_SCHEMA = '{0}' And
                        QSYS2.SYSVIEWS.TABLE_NAME = '{1}'",
                    view.Owner, view.Name);

                // Get the view text
                using (var cmd = new iDB2Command(sql, conn))
                {
                    cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    viewText = (string)cmd.ExecuteScalar();
                }
            }

            return viewText;
        }

        /// <summary>
        /// Return all the columns for a view...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            var columnList = new ArrayList();

            using (var conn = new iDB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                const int POS_COLUMN_NAME = 0;
                const int POS_DATA_TYPE = 1;
                //				const int POS_DATA_LENGTH	= 2; // LENGTH is precision for DECIMALS
                const int POS_DATA_SCALE = 3;
                const int POS_NULLABLE = 4;
                //				const int POS_IDENTITY		= 5;
                //				const int POS_COMMENTS		= 6;

                string sql = String.Format(@"
                    SELECT	name, 
                            coltype as typename, 
                            length, 
                            coalesce(scale, 0) as scale, 
                            nulls, 
                            identity,
                            coalesce(remarks, '') as remarks
                    FROM	qsys2.syscolumns
                    WHERE	table_schema = '{0}'
                            AND TBNAME = '{1}'
                    ORDER BY colno",
                    view.Owner,
                    view.Name);

                using (var cmd = new iDB2Command(sql, conn))
                {
                    cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    using (iDB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            var properties = new ExtendedPropertyCollection { CreateExtendedPropertyForComments(reader) };
                            //properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsIdentity, (reader.GetString(POS_IDENTITY) == "Y"), DbType.Boolean));

                            // Get precision
                            byte precision = GetDBTypePrecision(reader);

                            var cs = new ViewColumnSchema(
                                view,
                                reader.GetString(POS_COLUMN_NAME),
                                GetDbType(reader),
                                reader.GetString(POS_DATA_TYPE),
                                GetDBTypeLength(reader),
                                precision,
                                Convert.ToInt32(reader[POS_DATA_SCALE]),
                                reader.GetString(POS_NULLABLE) == "Y",
                                properties.ToArray());

                            columnList.Add(cs);
                        }
                    }
                }
            }

            // Return the array of columns
            return (ViewColumnSchema[])columnList.ToArray(typeof(ViewColumnSchema));
        }

        /// <summary>
        /// Return all the Rows from a view
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            using (var conn = new iDB2Connection(connectionString))
            {
                // Create and fill the data table
                var dt = new DataTable(view.Name);
                string sql = String.Format("SELECT * FROM {0}.{1}", view.Owner, view.Name);

                using (var adp = new iDB2DataAdapter(sql, conn))
                {
                    adp.SelectCommand.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    adp.Fill(dt);
                }

                return dt;
            }
        }

        #endregion

        #region COMMAND Info

        /// <summary>
        /// Return array of commands...
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            var commandList = new ArrayList();

            using (var conn = new iDB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                const int POS_OWNER = 0;
                const int POS_PROCNAME = 1;
                const int POS_SPECIFICNAME = 2;
                //const int POS_PROCEDUREID	= 3;
                const int POS_CREATED = 3;
                const int POS_DEFINER = 4;
                const int POS_LANGUAGE = 5;
                //const int POS_VALID			= 7;
                const int POS_OVERLOADCOUNT = 6;
                const int POS_PARMSIGNATURE = 7;
                const int POS_RESULTSETS = 8;

                string schemaFilterOuter = GetSchemaFilterCriteria("procedures.procschema", false);

                // Insert the WHERE clause into the schema filter, if it's been added
                if (schemaFilterOuter != null)
                    schemaFilterOuter = "WHERE	" + schemaFilterOuter;

                string schemaFilterInner = GetSchemaFilterCriteria("inProcedures.procschema", false);

                // Insert the WHERE clause into the schema filter, if it's been added
                if (schemaFilterInner != null)
                    schemaFilterInner = "WHERE	" + schemaFilterInner;

                string sql = String.Format(@"
                    Select
                        procedures.ROUTINE_SCHEMA As procschema,
                        procedures.ROUTINE_NAME As procname,
                        procedures.SPECIFIC_NAME As specificname,
                        procedures.ROUTINE_CREATED As create_time,
                        procedures.ROUTINE_DEFINER As definer,
                        coalesce(procedures.EXTERNAL_LANGUAGE, '') As language,
                        o.overload_count,
                        procedures.PARM_SIGNATURE As parm_signature,
                        procedures.RESULT_SETS As result_sets
                    From
                        QSYS2.SYSPROCS procedures left join
                               (
                                Select
                                    inProcedures.ROUTINE_SCHEMA As procschema,
                                    inProcedures.ROUTINE_NAME As procname,
                                    Count(*) As overload_count
                                From
                                    QSYS2.SYSPROCS inProcedures
                                {0}
                                Group By
                                    inProcedures.ROUTINE_SCHEMA,
                                    inProcedures.ROUTINE_NAME
                                Having
                                    count(*) > 1
                                ) o
                        ON procedures.procschema = o.procschema AND procedures.procname = o.procname
                    {1}
                    ORDER BY
                        procedures.procschema,
                        procedures.procname",
                    schemaFilterInner,
                    schemaFilterOuter);

                using (var cmd = new iDB2Command(sql, conn))
                {
                    cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    using (iDB2DataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            var extendedProperties = new ExtendedPropertyCollection();
                            extendedProperties.Add(new ExtendedProperty("CS_SpecificName", reader.GetString(POS_SPECIFICNAME), DbType.String));
                            //extendedProperties.Add(new ExtendedProperty("CS_ProcedureId", reader.GetInt32(POS_SPECIFICNAME).GetHashCode().ToString(), DbType.String));
                            extendedProperties.Add(new ExtendedProperty("CS_Definer", reader.GetString(POS_DEFINER), DbType.String));
                            extendedProperties.Add(new ExtendedProperty("CS_Language", reader.GetString(POS_LANGUAGE), DbType.String));
                            extendedProperties.Add(new ExtendedProperty("CS_Valid", "Y", DbType.String));
                            extendedProperties.Add(new ExtendedProperty("CS_Resultsets", reader.GetInt16(POS_RESULTSETS), DbType.Int16));

                            bool isOverloaded = false;

                            if (!reader.IsDBNull(POS_OVERLOADCOUNT))
                            {
                                isOverloaded = true;
                            }

                            // Save the name and full name (minus the "hack" to support overloads) to the extended properties
                            string name = reader.GetString(POS_PROCNAME);
                            extendedProperties.Add(new ExtendedProperty("CS_Name", name, DbType.String));
                            extendedProperties.Add(new ExtendedProperty("CS_FullName", String.Format("{0}.{1}", reader.GetString(POS_OWNER).Trim(), name), DbType.String));

                            // Tack on overload # into the name as a means of supporting overloads in CodeSmith.
                            // Template writers will need to use the CS_Name property to get just the command name value,
                            // but this at least allows code to be generated that makes use of this great feature.
                            if (isOverloaded)
                            {
                                //name += "[" + reader.GetString(POS_SPECIFICNAME) + "]";

                                // Insert the hex representation of the hash code of the signature string in the syscat.
                                // This will result in a consistent name for the procedure based on the signature, 
                                // even if the procedure is dropped and recreated.
                                long readLength = reader.GetBytes(POS_PARMSIGNATURE, 0, null, 0, 180);
                                var signatureBytes = new byte[readLength];
                                reader.GetBytes(POS_PARMSIGNATURE, 0L, signatureBytes, 0, Convert.ToInt32(readLength));

                                var encoding = new UnicodeEncoding();
                                string signatureText = encoding.GetString(signatureBytes);

                                name += "[" + signatureText.GetHashCode().ToString("X") + "]";
                            }

                            var commandSchema = new CommandSchema(
                                database,
                                name,
                                reader.GetString(POS_OWNER).Trim(),
                                reader.GetDateTime(POS_CREATED),
                                extendedProperties.ToArray());

                            commandList.Add(commandSchema);
                        }
                    }
                }
            }

            // Return the array of commands
            return (CommandSchema[])commandList.ToArray(typeof(CommandSchema));
        }

        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            var paramList = new ArrayList();

            var specificName = (string)command.ExtendedProperties["CS_SpecificName"].Value;
            //var commandName = (string) command.ExtendedProperties["CS_Name"].Value;

            using (var conn = new iDB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                const int POS_PARM_NAME = 0;
                const int POS_TYPE_NAME = 1;
                const int POS_IN_OUT = 2;
                // const int POS_DATA_LENGTH = 3;
                const int POS_DATA_SCALE = 4;

                string sql = String.Format(@"
                    Select
                        parms.PARAMETER_NAME As parmname,
                        parms.DATA_TYPE As typename,
                        parms.PARAMETER_MODE As parmmode,
                        coalesce(CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION) As length,
                        coalesce(parms.NUMERIC_SCALE, 0) As scale
                    From
                        QSYS2.SYSPARMS parms
                    Where
                        parms.SPECIFIC_NAME = '{0}'
                    Order By
                        parms.ORDINAL_POSITION",
                        specificName);

                using (var cmd = new iDB2Command(sql, conn))
                {
                    cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    using (iDB2DataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string parmName = reader.GetString(POS_PARM_NAME);
                            string typeName = reader.GetString(POS_TYPE_NAME);
                            int length = GetDBTypeLength(reader);
                            int scale = Convert.ToInt32(reader.GetInt16(POS_DATA_SCALE));
                            byte precision = GetDBTypePrecision(reader);

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

                            paramList.Add(parameterSchema);
                        }
                    }
                }
            }

            return (ParameterSchema[])paramList.ToArray(typeof(ParameterSchema));
        }

        public string GetCommandText(string connectionString, CommandSchema command)
        {
            // TODO: Return command text of simple procedures?
            // throw new NotImplementedException("Retrieval of command text has not yet been implemented.");

            var specificName = (string)command.ExtendedProperties["CS_SpecificName"].Value;
            //var commandName = (string) command.ExtendedProperties["CS_Name"].Value;

            string sql = String.Format(@"select ROUTINE_DEFINITION from QSYS2.SYSPROCS WHERE specific_name = '{0}'", specificName);
            using (var conn = new iDB2Connection(connectionString))
            {
                // Open a connection
                conn.Open();

                using (var cmd = new iDB2Command(sql, conn))
                {
                    cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                    using (iDB2DataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Return the procedure text
                            return reader.GetString(0);
                        }
                        return String.Empty;
                    }
                }
            }
        }

        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            // Make sure the user has explicitly allowed this operation to proceed...
            if (!iSeriesConfiguration.Instance.AutoCreateExtendedPropertiesTable)
                throw new InvalidOperationException("The iDB2 Schema Provider supports reading column schema information. Please set AllowGetCommandResultSchemas to true in the Generator Options -> Schema Provider -> iSeries page as its usage can change the state of the server.");

            // Initialize array to size 0 for now
            var commandResultSchemas = new CommandResultSchema[0];

            // How many result sets are expected?
            int resultsetCount = (short)command.ExtendedProperties["CS_Resultsets"].Value;

            if (resultsetCount == 0)
            {
                // No resultsets expected, so return what we have now (empty array)
                return commandResultSchemas;
            }

            // Keep a count as to how many results are expected
            // int refCursorCount = 0;

            using (var conn = new iDB2Connection(connectionString))
            {
                // Open the connection
                conn.Open();

                using (var cmd = new iDB2Command((string)command.ExtendedProperties["CS_FullName"].Value, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Set up the parameters will null values
                    foreach (ParameterSchema parameterSchema in command.Parameters)
                    {
                        var parm = new iDB2Parameter
                        {
                            ParameterName = parameterSchema.Name,
                            DbType = parameterSchema.DataType,
                            Direction = parameterSchema.Direction
                        };

                        // // If the parameter is not a REF CURSOR
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
                                case DbType.DateTime:
                                    parm.Value = new DateTime(1967, 1, 1);
                                    break;

                                case DbType.Time:
                                    parm.Value = new TimeSpan(0, 0, 0);
                                    break;

                                case DbType.Guid:
                                    throw new NotSupportedException("Encountered unsupported DbType.Guid.");

                                default:
                                    throw new NotSupportedException(String.Format("Encountered unsupported type 'DbType.{0}'", parm.DbType));
                            }
                        }

                        // Add the parameter to the command for execution
                        cmd.Parameters.Add(parm);
                    }

                    // Will there be any results returned?
                    if (resultsetCount > 0)
                    {
                        // Wrap the operation in a transaction to prevent as much change to the server as possible
                        using (iDB2Transaction trans = conn.BeginTransaction())
                        {
                            // Assign the transaction to the command
                            cmd.Transaction = trans;

                            // Ask for reader, but the schema only.
                            using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection)) // CommandBehavior.SchemaOnly | 
                            {
                                int colIndex = 0;
                                int resultsetIndex = 0;

                                // Create an array to hold information about all the resultsets
                                // commandResultSchemas = new CommandResultSchema[resultsetCount];
                                var commandResultSchemaList = new ArrayList();

                                do
                                {
                                    // Get schema info from reader
                                    DataTable schemaTable = reader.GetSchemaTable();

                                    // Create an array of result column schemas to store column information
                                    if (schemaTable != null)
                                    {
                                        var resultColumnSchemas = new CommandResultColumnSchema[schemaTable.Rows.Count];

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

                                            byte numericPrecision = 0;

                                            // GetSchemaTable method of IBM iDB2 provider returns NumericPrecision equal to column length for VARCHAR columns (at the very least)
                                            if ((Type)row["DataType"] != typeof(string))
                                            {
                                                numericPrecision = row.IsNull("NumericPrecision") ? (byte)0 : Convert.ToByte(row["NumericPrecision"]);
                                            }

                                            var resultColumnSchema =
                                                new CommandResultColumnSchema(
                                                    command,
                                                    (string)row["ColumnName"],
                                                    GetDbTypeFromSystemType((Type)row["DataType"]),
                                                    nativeType,
                                                    row.IsNull("ColumnSize") ? 0 : Convert.ToInt32(row["ColumnSize"]),
                                                    numericPrecision,
                                                    row.IsNull("NumericScale") ? 0 : Convert.ToInt32(row["NumericScale"]),
                                                    row.IsNull("AllowDBNull") || (bool)row["AllowDBNull"]);

                                            // Assign the result column schema information to the array
                                            resultColumnSchemas[colIndex++] = resultColumnSchema;
                                        }

                                        // Construct a result schema for the results just processed
                                        resultsetIndex++;
                                        var commandResultSchema = new CommandResultSchema(command, "Table" + resultsetIndex, resultColumnSchemas);

                                        // Assign the result schema for this resultset to the array
                                        commandResultSchemaList.Add(commandResultSchema);  // commandResultSchemas[resultsetIndex++] = commandResultSchema;
                                    }
                                } while (reader.NextResult());

                                // Convert the array into an array of CommandResultSchema objects, for return
                                commandResultSchemas = (CommandResultSchema[])commandResultSchemaList.ToArray(typeof(CommandResultSchema));
                            }
                        }
                    }
                }

                // Return the array of command results
                return commandResultSchemas;
            }
        }

        #endregion

        #region IDbSchemaProvider Members

        /// <summary>
        /// Allows users to set Extended Properties from CodeSmith.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="schemaObject"></param>
        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            if (String.IsNullOrEmpty(connectionString) || schemaObject == null)
                return;

            //CreateExtended Properties Table
            if (iSeriesConfiguration.Instance.AutoCreateExtendedPropertiesTable)
                CreateExtendedPropertiesTable(connectionString);

            //if (!HasExtendedPropertiesTable(connectionString, schemaObject))
            //    return;

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
            using (var connection = new iDB2Connection(connectionString))
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
                        using (var db2Command = new iDB2Command(command, connection))
                        {
                            db2Command.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                            db2Command.ExecuteScalar();
                        }
                    }
                } // for each

                if (needRefresh)
                    schemaObject.Refresh();
            }
        }

        private void CreateExtendedPropertiesTable(string connectionString)
        {
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

            using (var conn = new iDB2Connection(connectionString))
            {
                conn.Open();

                try
                {
                    using (var cmd = new iDB2Command(sqlCreate, conn))
                    {
                        cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Wrap the exception in our own, and rethrow
                    throw new ApplicationException("Unable to automatically create extended properties because the following exception occurred:\n\n" + ex.Message, ex);
                }

                try
                {
                    using (var cmd = new iDB2Command(sqlAddPK, conn))
                    {
                        cmd.CommandTimeout = iSeriesConfiguration.Instance.CommandTimeout;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    // Wrap the exception in our own, and rethrow
                    throw new ApplicationException("Unable to add primary key to extended properties table during auto-creation because the following exception occurred:\n\n" + ex.Message, ex);
                }
            }
        }

        #endregion

        #region Helpers

        private string GetSchemaFilterCriteria(string columnNameToFilter, bool includeLeadingAND)
        {
            string temp = String.Empty;

            if (!String.IsNullOrEmpty(iSeriesConfiguration.Instance.FilterSchema.ToUpper()))
            {
                temp += includeLeadingAND ? "AND " : String.Empty;
                temp += "(";
                ArrayList criteria = new ArrayList(iSeriesConfiguration.Instance.FilterSchema.ToUpper().Split(','));
                foreach (string c in criteria)
                {
                    string x = c.Trim();
                    // Check for leading and trailing '
                    if (x.Substring(0, 1) != "'")
                    {
                        x = "'" + x;
                    }
                    if (x.Substring(x.Length - 1, 1) != "'")
                    {
                        x = x + "'";
                    }

                    // Now append the criteria with an OR
                    temp += String.Format("({0} like {1}) OR ", columnNameToFilter, x);
                }
                // Remove the trailing OR
                temp = temp.Substring(0, temp.Length - 4);
                temp += ") ";
                return temp;
            }

            return String.Empty;
        }

        private string GetTableFilterCriteria(string columnNameToFilter, bool includeLeadingAND)
        {
            string temp = String.Empty;

            if (!String.IsNullOrEmpty(iSeriesConfiguration.Instance.FilterTables.ToUpper()))
            {
                temp += includeLeadingAND ? "AND " : String.Empty;
                temp += "(";
                ArrayList criteria = new ArrayList(iSeriesConfiguration.Instance.FilterTables.ToUpper().Split(','));
                foreach (string c in criteria)
                {
                    string x = c.Trim();
                    // Check for leading and trailing '
                    if (x.Substring(0, 1) != "'")
                    {
                        x = "'" + x;
                    }
                    if (x.Substring(x.Length - 1, 1) != "'")
                    {
                        x = x + "'";
                    }

                    // Now append the criteria with an OR
                    temp += String.Format("({0} like {1}) OR ", columnNameToFilter, x);
                }
                // Remove the trailing OR
                temp = temp.Substring(0, temp.Length - 4);
                temp += ") ";
                return temp;
            }

            return String.Empty;
        }

        private string GetViewFilterCriteria(string columnNameToFilter, bool includeLeadingAND)
        {
            string temp = String.Empty;

            if (!String.IsNullOrEmpty(iSeriesConfiguration.Instance.FilterViews.ToUpper()))
            {
                temp += includeLeadingAND ? "AND " : String.Empty;
                temp += "(";
                ArrayList criteria = new ArrayList(iSeriesConfiguration.Instance.FilterViews.ToUpper().Split(','));
                foreach (string c in criteria)
                {
                    string x = c.Trim();
                    // Check for leading and trailing '
                    if (x.Substring(0, 1) != "'")
                    {
                        x = "'" + x;
                    }
                    if (x.Substring(x.Length - 1, 1) != "'")
                    {
                        x = x + "'";
                    }

                    // Now append the criteria with an OR
                    temp += String.Format("({0} like {1}) OR ", columnNameToFilter, x);
                }
                // Remove the trailing OR
                temp = temp.Substring(0, temp.Length - 4);
                temp += ") ";
                return temp;
            }

            return String.Empty;
        }

        private ExtendedProperty CreateExtendedPropertyForComments(iDB2DataReader reader)
        {
            int ordinal = reader.GetOrdinal("REMARKS");

            if (ordinal >= 0)
            {
                // Get the comments, or an empty string
                string comments = reader.IsDBNull(ordinal) ? String.Empty : reader.GetString(ordinal);

                return CreateExtendedPropertyForComments(comments);
            }

            throw new ApplicationException("There was no COMMENTS column in the iDB2DataReader provided.");
        }

        private ExtendedProperty CreateExtendedPropertyForComments(string comments)
        {
            var property = new ExtendedProperty(ExtendedPropertyNames.Description, comments, DbType.String);

            return property;
        }

        private string GetExtendedPropertiesTableOwnerPrefix()
        {
            // If an explicit owner name was provided, use it, and also build up the variable to be used for prefixing
            if (!String.IsNullOrEmpty(iSeriesConfiguration.Instance.ExtendedPropertiesTableSchema))
                return String.Format("{0}.", iSeriesConfiguration.Instance.ExtendedPropertiesTableSchema);

            // We don't want an empty String... we want it to always be 'null'
            return null;
        }

        #endregion

        #region DbType Conversions

        private static DbType GetDbType(string nativeType, int precision, int scale)
        {
            DbType result;

            // Convert native iDB2 types to appropriate DbType enumerated values
            switch (nativeType.ToUpper())
            {
                case "DECIMAL":
                    result = DbType.Decimal;
                    break;

                case "SMALLINT":
                    result = DbType.Int16;
                    break;

                case "INTEGER":
                    result = DbType.Int32;
                    break;

                case "BIGINT":
                    result = DbType.Int64;
                    break;

                case "REAL":
                    result = DbType.Single;
                    break;

                case "DOUBLE":
                    result = DbType.Double;
                    break;
                case "DATE":
                case "TIMESTMP":
                    result = DbType.DateTime;
                    break;

                case "TIME":
                    result = DbType.Time;
                    break;

                case "VARCHAR":
                    result = DbType.String;
                    break;

                case "CHAR":
                case "CHARACTER":
                    result = DbType.StringFixedLength;
                    break;

                case "CLOB":
                case "GRAPHIC":
                case "VARGRAPHIC":
                case "DBCLOB":
                case "BLOB":
                case "DATALINK":
                    result = DbType.Object;
                    break;

                default:
                    result = DbType.Object;
                    break;
            }

            return result;
        }

        private static DbType GetDbType(iDB2DataReader reader)
        {
            string nativeType = null;
            nativeType = reader["TYPENAME"].ToString();

            int scale = 0;
            int precision = 0;

            try
            {
                scale = Convert.ToInt32(reader["SCALE"]);
            }
            catch
            {
            }

            if (nativeType == "DECIMAL") // LENGTH is precisions for DECIMAL type
            {
                try
                {
                    precision = Convert.ToInt32(reader["LENGTH"]);
                }
                catch
                {
                }
            }
            else
            {
                // Get precisions for other iDB2 native types
                precision = Convert.ToInt32(GetDBTypePrecision(reader));
            }
            return GetDbType(nativeType, precision, scale);
        }

        #endregion

        private static byte GetDBTypePrecision(string dataTypeName, int length)
        {
            int result;

            switch (dataTypeName)
            {
                case "DECIMAL":
                    result = length; // For DECIMAL types, length is the precision
                    break;

                case "REAL":
                    result = 7;
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

        private static int GetDBTypeLength(string dataTypeName, int length)
        {
            switch (dataTypeName)
            {
                case "CHAR":  // <-- Is this even a iDB2 type?
                case "CHARACTER":
                case "VARCHAR":
                case "CLOB":
                case "GRAPHIC":
                case "VARGRAPHIC":
                case "DBCLOB":
                case "BLOB":
                    return length;

                default:
                    // Length is not relevant to other data types
                    return 0;
            }
        }

        private static byte GetDBTypePrecision(iDB2DataReader reader)
        {
            string nativeType = reader["TYPENAME"].ToString();

            int length = 0;

            try
            {
                length = Convert.ToInt32(reader["LENGTH"]);
            }
            catch (Exception)
            {
            }

            return GetDBTypePrecision(nativeType, length);
        }

        private static int GetDBTypeLength(iDB2DataReader reader)
        {
            var nativeType = (string)reader["TYPENAME"];

            int length = 0;

            try
            {
                length = Convert.ToInt32(reader["LENGTH"]);
            }
            catch (Exception)
            {
            }

            return GetDBTypeLength(nativeType, length);
        }

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

                    case "INOUT":
                        parmDirection = ParameterDirection.InputOutput;
                        break;
                }
            }

            return parmDirection;
        }
    }
}