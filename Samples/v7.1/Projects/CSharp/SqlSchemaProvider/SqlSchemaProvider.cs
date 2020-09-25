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
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Data.ConnectionUI;

namespace SchemaExplorer
{
    public class SqlSchemaProvider : IDbSchemaProvider, IDbConnectionStringEditor
    {
        private string _databaseName = String.Empty;
        private string _sqlServerVersion;
        private string _sqlServerEdition;
        private static readonly object _lock = new object();
        private int? _sqlServerMajorVersion;
        
        public SqlSchemaProvider()
        {
        }

        #region Properties
        public string Name
        {
            get { return "SqlSchemaProvider"; }
        }

        public string Description
        {
            get { return "SQL Server Schema Provider"; }
        }
        #endregion

        #region Schema Retrieval Methods

        #region GetDatabaseName
        public string GetDatabaseName(string connectionString)
        {
            if (_databaseName == String.Empty)
            {
                SqlService sql = new SqlService(connectionString) {IsSingleRow = true};
                using (SqlDataReader reader = sql.ExecuteSqlReader(SQL_GetDatabaseName))
                {
                    while (reader.Read())
                    {
                        _databaseName = reader.GetString(0);
                    }
                }
            }

            return _databaseName;
        }
        #endregion

        #region GetTables
        public TableSchema[] GetTables(string connectionString, DatabaseSchema database)
        {
            SqlBuilder sqlstring = new SqlBuilder();
            List<TableSchema> tables = new List<TableSchema>();
            List<ExtendedProperty> extendedPropertiesList = new List<ExtendedProperty>();
            int majorVersion = GetSqlServerMajorVersion(connectionString);
            bool isAzure = IsSqlAzure(connectionString);

            sqlstring.AppendStatement(SqlFactory.GetTables(majorVersion, isAzure));

            if (database.DeepLoad)
            {
                sqlstring.AppendStatement(SqlFactory.GetAllTableColumns(majorVersion));
                sqlstring.AppendStatement(SqlFactory.GetIndexes(majorVersion, isAzure));
                sqlstring.AppendStatement(SqlFactory.GetKeys(majorVersion));
                sqlstring.AppendStatement(SqlFactory.GetColumnConstraints(majorVersion));

                if (!isAzure)
                    sqlstring.AppendStatement(SqlFactory.GetExtendedData(majorVersion));
            }

            SqlService sql = new SqlService(connectionString);
            using (SafeDataReader reader = sql.ExecuteSafeReader(sqlstring))
            {

                while (reader.Read())
                {
                    extendedPropertiesList.Clear();
                    extendedPropertiesList.Add(new ExtendedProperty(
                        ExtendedPropertyNames.FileGroup, reader.GetString(4),
                        DbType.AnsiString, PropertyStateEnum.ReadOnly));

                    extendedPropertiesList.Add(new ExtendedProperty(
                        ExtendedPropertyNames.ObjectID, reader.GetInt32(5),
                        DbType.Int32, PropertyStateEnum.ReadOnly));

                    TableSchema tableSchema = new TableSchema(database, reader.GetString(0),
                        reader.GetString(1), reader.GetDateTime(3),
                        extendedPropertiesList.ToArray());

                    tables.Add(tableSchema);
                }

                if (database.DeepLoad)
                {
                    if (reader.NextResult())
                        PopulateTableColumns(reader, tables);

                    if (reader.NextResult())
                        PopulateTableIndexes(reader, tables);

                    if (reader.NextResult())
                        PopulateTableKeys(reader, tables);

                    if (reader.NextResult())
                        PopulateTableColumnConstraints(reader, tables);

                    if (reader.NextResult())
                        PopulateTableExtendedData(reader, tables);

                    PopulateTableDescriptions(tables);
                }
            }

            return tables.ToArray();
        }
        #endregion

        #region GetTableColumns
        public ColumnSchema[] GetTableColumns(string connectionString, TableSchema table)
        {
            List<ColumnSchema> columns;
            int majorVersion = GetSqlServerMajorVersion(connectionString);
            string sqlstring = SqlFactory.GetTableColumns(majorVersion);

            SqlService sql = new SqlService(connectionString);
            sql.AddParameter("@SchemaName", SqlDbType.NVarChar, table.Owner, 255);
            sql.AddParameter("@TableName", SqlDbType.NVarChar, table.Name, 255);

            using (SafeDataReader reader = sql.ExecuteSafeReader(sqlstring))
            {
                columns = GetColumnsFromReader(table, reader);
            }

            return columns.ToArray();
        }

        private List<ColumnSchema> GetColumnsFromReader(TableSchema table, SafeDataReader reader)
        {
            return GetColumnsFromReader(Utility.ToDictionary(table), reader, false);
        }

        private List<ColumnSchema> GetColumnsFromReader(IDictionary<string, TableSchema> tables, SafeDataReader reader, bool populateTable)
        {
            List<ColumnSchema> columns = new List<ColumnSchema>();
            List<ExtendedProperty> properties = new List<ExtendedProperty>();

            // using safe reader for nulls
            while (reader.Read())
            {
                string name = reader.GetString("Name");
                string nativeType = reader.GetString("DataType");
                string systemType = reader.GetString("SystemType");
                DbType dataType = Utility.GetDbType(systemType);
                int size = reader.GetInt32("Length");
                byte precision = reader.GetByte("NumericPrecision");
                int scale = reader.GetInt32("NumericScale");
                bool allowDBNull = reader.GetBoolean("IsNullable");
                string defaultValue = reader.GetString("DefaultValue");
                bool isIdentity = (reader.GetInt32("Identity") == 1);
                bool isRowGuidCol = (reader.GetInt32("IsRowGuid") == 1);
                bool isComputed = (reader.GetInt32("IsComputed") == 1);
                bool isDeterministic = reader.IsDBNull("IsDeterministic") || (reader.GetInt32("IsDeterministic") == 1);
                string identitySeed = reader.GetString("IdentitySeed");
                string identityIncrement = reader.GetString("IdentityIncrement");
                string computedDefinition = reader.GetString("ComputedDefinition");
                string collation = reader.GetString("Collation");
                int objectid = reader.GetInt32("ObjectId");
                string schemaName = reader.GetString("SchemaName");
                string tableName = reader.GetString("TableName");

                properties.Clear();
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsRowGuidColumn, isRowGuidCol, DbType.Boolean, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsIdentity, isIdentity, DbType.Boolean, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsComputed, isComputed, DbType.Boolean, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsDeterministic, isDeterministic, DbType.Boolean, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IdentitySeed, identitySeed, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IdentityIncrement, identityIncrement, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, defaultValue, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.ComputedDefinition, computedDefinition, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.Collation, collation, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.ObjectID, objectid, DbType.Int32, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.SystemType, systemType, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.UserDefinedType, nativeType, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty("CS_UserType", nativeType, DbType.String, PropertyStateEnum.ReadOnly)); // Added for backwards compatibility.
                ExtendedProperty[] extendedProperties = properties.ToArray();

                TableSchema table;

                if (!tables.TryGetValue(
                    SchemaObjectBase.FormatFullName(schemaName, tableName),
                    out table))
                    continue;

                ColumnSchema column = new ColumnSchema(table, name, dataType,
                    string.IsNullOrEmpty(systemType) ? nativeType : systemType,
                    size, precision, scale, allowDBNull, extendedProperties);

                if (populateTable)
                    table.Columns.Add(column);

                columns.Add(column);
            }

            return columns;
        }
        #endregion

        #region Populate Table
        private void PopulateTableColumns(SafeDataReader reader, IList<TableSchema> tables)
        {

            if (tables == null || tables.Count < 1)
                return;

            Dictionary<string, TableSchema> tableDictionary = new Dictionary<string, TableSchema>();
            foreach (TableSchema table in tables)
            {
                tableDictionary.Add(table.FullName, table);
                table.Columns = new ColumnSchemaCollection();
            }

            GetColumnsFromReader(tableDictionary, reader, true);
        }

        private void PopulateTableIndexes(SafeDataReader reader, IList<TableSchema> tables)
        {
            if (tables == null || tables.Count < 1)
                return;

            Dictionary<string, TableSchema> tableDictionary = new Dictionary<string, TableSchema>();
            foreach (TableSchema table in tables)
            {
                tableDictionary.Add(table.FullName, table);
                table.Indexes = new IndexSchemaCollection();
            }

            GetIndexesFromReader(tableDictionary, reader, true);
        }

        private void PopulateTableKeys(SafeDataReader reader, IList<TableSchema> tables)
        {
            if (tables == null || tables.Count < 1)
                return;

            Dictionary<string, TableSchema> tableDictionary = new Dictionary<string, TableSchema>();
            foreach (TableSchema table in tables)
            {
                tableDictionary.Add(table.FullName, table);
                table.Keys = new TableKeySchemaCollection();
            }

            GetKeysFromReader(tableDictionary, reader, true);
        }

        private void PopulateTableColumnConstraints(SafeDataReader reader, IList<TableSchema> tables)
        {
            if (tables == null || tables.Count < 1)
                return;

            Dictionary<string, TableSchema> tableDictionary = Utility.ToDictionary(tables);
            while (reader.Read())
            {
                string tableName = reader.GetString("TableName");
                string schemaName = reader.GetString("SchemaName");
                string columnName = reader.GetString("ColumnName");
                string name = reader.GetString("ConstraintName");
                string type = reader.GetString("ConstraintType");
                string definition = reader.GetString("ConstraintDef");

                if (Utility.IsAnyNullOrEmpty(tableName, schemaName, columnName))
                    continue;

                TableSchema table;
                if (!tableDictionary.TryGetValue(
                    SchemaObjectBase.FormatFullName(schemaName, tableName),
                    out table))
                    continue;

                ColumnSchema column = table.Columns[columnName];
                if (column == null)
                    continue;

                column.MarkLoaded();

                column.ExtendedProperties.Add(new ExtendedProperty(String.Format(ExtendedPropertyNames.ConstraintNameFormat, name), name, DbType.String));
                column.ExtendedProperties.Add(new ExtendedProperty(String.Format(ExtendedPropertyNames.ConstraintTypeFormat, name), type, DbType.String));
                column.ExtendedProperties.Add(new ExtendedProperty(String.Format(ExtendedPropertyNames.ConstraintDefinitionFormat, name), definition, DbType.String));
            }
        }

        private void PopulateTableExtendedData(SafeDataReader reader, IList<TableSchema> tables)
        {
            if (tables == null || tables.Count < 1)
                return;

            Dictionary<string, TableSchema> tableDictionary = Utility.ToDictionary(tables);

            while (reader.Read())
            {
                string propertyName = reader.GetString("PropertyName");
                object propertyValue = reader["PropertyValue"];
                string propertyBaseType = reader.GetString("PropertyBaseType");

                DbType propertyType = string.IsNullOrEmpty(propertyBaseType) ?
                    DbType.Object : Utility.GetDbType(propertyBaseType);

                string objectName = reader.GetString("ObjectName");
                string objectOwner = reader.GetString("ObjectOwner");
                string objectType = reader.GetString("ObjectType").Trim();

                string parentName = reader.GetString("ParentName");
                string parentOwner = reader.GetString("ParentOwner");
                //string parentType = reader.GetString("ParentType");

                string fieldName = reader.GetString("FieldName");
                string indexName = reader.GetString("IndexName");

                int type = Convert.ToInt32(reader.GetByte("Type"));
                int minor = reader.GetInt32("Minor");

                if ((propertyName != null)
                    && propertyName.StartsWith("microsoft_database_tools", StringComparison.Ordinal))
                    continue;

                ExtendedProperty property = new ExtendedProperty(propertyName, propertyValue, propertyType);

                TableSchema table;

                if (objectType == "U")
                {
                    tableDictionary.TryGetValue(SchemaObjectBase.FormatFullName(objectOwner, objectName), out table);
                    if(table == null) 
                        continue;

                    if (type == 1) // object or column
                    {
                        //TODO: check, can column id be 0?
                        if (minor > 0 && !string.IsNullOrEmpty(fieldName)) // column
                        {
                            ColumnSchema column = table.Columns[fieldName];
                            column.MarkLoaded();
                            column.ExtendedProperties.Add(property);
                        }
                        else // table
                        {
                            table.MarkLoaded();
                            table.ExtendedProperties.Add(property);
                        }
                    }
                    else if (type == 7)  // Index
                    {
                        //NOTE: Solves the Following SQL Server 2008 bug (http://connect.microsoft.com/SQLServer/feedback/ViewFeedback.aspx?FeedbackID=350673).
                        if (!string.IsNullOrEmpty(indexName))
                        {
                            IndexSchema index = table.Indexes[indexName];
                            index.MarkLoaded();
                            index.ExtendedProperties.Add(property);
                        }
                    }

                }
                else if (objectType == "PK" || objectType == "K") // Primary Key
                {
                    tableDictionary.TryGetValue(SchemaObjectBase.FormatFullName(parentOwner, parentName), out table);
                    if (table == null)
                        continue;

                    if (table.HasPrimaryKey)
                    {
                        table.PrimaryKey.MarkLoaded();
                        table.PrimaryKey.ExtendedProperties.Add(property);
                    }
                }
                else if (objectType == "F")
                {
                    tableDictionary.TryGetValue(SchemaObjectBase.FormatFullName(parentOwner, parentName), out table);
                    if (table == null)
                        continue;

                    TableKeySchema key = table.Keys[objectName];
                    key.MarkLoaded();
                    key.ExtendedProperties.Add(property);
                }

            } //while read
        }

        private void PopulateTableDescriptions(IEnumerable<TableSchema> tables)
        {
            //mark all loaded
            foreach (TableSchema t in tables)
            {
                SyncDescription(t);

                if (t.HasPrimaryKey)
                    SyncDescription(t.PrimaryKey);

                foreach (ColumnSchema c in t.Columns)
                    SyncDescription(c);

                foreach (IndexSchema i in t.Indexes)
                {
                    SyncDescription(i);
                    foreach (MemberColumnSchema c in i.MemberColumns)
                        SyncDescription(c);
                }
                foreach (TableKeySchema k in t.Keys)
                {
                    SyncDescription(k);
                    foreach (MemberColumnSchema c in k.ForeignKeyMemberColumns)
                        SyncDescription(c);

                    foreach (MemberColumnSchema c in k.PrimaryKeyMemberColumns)
                        SyncDescription(c);
                }
            }
        }

        private void SyncDescription(SchemaObjectBase schema)
        {
            schema.MarkLoaded();
            if (schema is MemberColumnSchema)
                ((MemberColumnSchema)schema).Column.MarkLoaded();

            string description = string.Empty;
            if (schema.ExtendedProperties.Contains("MS_Description"))
                description = schema.ExtendedProperties["MS_Description"].Value as string;

            schema.ExtendedProperties[ExtendedPropertyNames.Description] = 
                new ExtendedProperty(ExtendedPropertyNames.Description, description ?? string.Empty, DbType.String, PropertyStateEnum.ReadOnly);
        }
        #endregion

        #region GetViews
        public ViewSchema[] GetViews(string connectionString, DatabaseSchema database)
        {
            SqlBuilder sqlstring = new SqlBuilder();
            List<ViewSchema> views = new List<ViewSchema>();
            List<ExtendedProperty> extendedPropertiesList = new List<ExtendedProperty>();
            int majorVersion = GetSqlServerMajorVersion(connectionString);
            bool isAzure = IsSqlAzure(connectionString);

            sqlstring.AppendStatement(SqlFactory.GetViews(majorVersion, isAzure));
            if (database.DeepLoad)
            {
                sqlstring.AppendStatement(SqlFactory.GetAllViewColumns(majorVersion));

                if (!isAzure)
                    sqlstring.AppendStatement(SqlFactory.GetExtendedData(majorVersion));
            }

            SqlService sql = new SqlService(connectionString);
            using (SafeDataReader reader = sql.ExecuteSafeReader(sqlstring))
            {
                while (reader.Read())
                {
                    extendedPropertiesList.Clear();
                    extendedPropertiesList.Add(new ExtendedProperty(ExtendedPropertyNames.ObjectID, reader.GetInt32(4),  DbType.Int32, PropertyStateEnum.ReadOnly));

                    ViewSchema viewSchema = new ViewSchema(database, reader.GetString(0), reader.GetString(1), reader.GetDateTime(3), extendedPropertiesList.ToArray());
                    views.Add(viewSchema);
                }

                if (database.DeepLoad)
                {
                    if (reader.NextResult())
                        PopulateViewColumns(reader, views);
                    if (reader.NextResult())
                        PopulateViewExtendedData(reader, views);

                    PopulateViewDescriptions(views);
                }
            }

            return views.ToArray();
        }


        #endregion

        #region Populate View
        private void PopulateViewColumns(SafeDataReader reader, IList<ViewSchema> views)
        {
            if (views == null || views.Count < 1)
                return;

            Dictionary<string, ViewSchema> viewDictionary = new Dictionary<string, ViewSchema>();
            foreach (ViewSchema view in views)
            {
                viewDictionary.Add(view.FullName, view);
                view.Columns = new ViewColumnSchemaCollection();
            }

            GetViewColumnsFromReader(viewDictionary, reader, true);

        }

        private void PopulateViewExtendedData(SafeDataReader reader, IList<ViewSchema> views)
        {
            if (views == null || views.Count < 1)
                return;

            Dictionary<string, ViewSchema> viewDictionary = Utility.ToDictionary(views);

            while (reader.Read())
            {
                string propertyName = reader.GetString("PropertyName");
                object propertyValue = reader["PropertyValue"];
                string propertyBaseType = reader.GetString("PropertyBaseType");

                DbType propertyType = string.IsNullOrEmpty(propertyBaseType) ?
                    DbType.Object : Utility.GetDbType(propertyBaseType);

                string objectName = reader.GetString("ObjectName");
                string objectOwner = reader.GetString("ObjectOwner");
                string objectType = reader.GetString("ObjectType").Trim();

                string fieldName = reader.GetString("FieldName");

                //int type = Convert.ToInt32(reader.GetByte("Type"));
                int minor = reader.GetInt32("Minor");

                if ((propertyName != null)
                    && propertyName.StartsWith("microsoft_database_tools", StringComparison.Ordinal))
                    continue;

                ExtendedProperty property = new ExtendedProperty(propertyName, propertyValue, propertyType);
                if (objectType == "V")
                {
                    ViewSchema view;
                    viewDictionary.TryGetValue(SchemaObjectBase.FormatFullName(objectOwner, objectName), out view);

                    if (minor > 0 && !string.IsNullOrEmpty(fieldName))
                    {
                        // column
                        ViewColumnSchema column = view.Columns[fieldName];
                        column.MarkLoaded();
                        column.ExtendedProperties.Add(property);
                    }
                    else
                    {
                        // view
                        view.MarkLoaded();
                        view.ExtendedProperties.Add(property);
                    }
                }

            } //while read
        }

        private void PopulateViewDescriptions(IEnumerable<ViewSchema> views)
        {
            //mark all loaded
            foreach (ViewSchema v in views)
            {
                SyncDescription(v);

                foreach (ViewColumnSchema c in v.Columns)
                    SyncDescription(c);
            }
        }
        #endregion

        #region GetViewColumns
        public ViewColumnSchema[] GetViewColumns(string connectionString, ViewSchema view)
        {
            List<ViewColumnSchema> columns;
            int majorVersion = GetSqlServerMajorVersion(connectionString);
            string sqlstring = SqlFactory.GetViewColumns(majorVersion);

            SqlService sql = new SqlService(connectionString);
            sql.AddParameter("@SchemaName", SqlDbType.NVarChar, view.Owner);
            sql.AddParameter("@ViewName", SqlDbType.NVarChar, view.Name);

            using (SafeDataReader reader = sql.ExecuteSafeReader(sqlstring))
            {
                columns = GetViewColumnsFromReader(view, reader);
            }

            return columns.ToArray();
        }

        private List<ViewColumnSchema> GetViewColumnsFromReader(ViewSchema view, SafeDataReader reader)
        {
            return GetViewColumnsFromReader(Utility.ToDictionary(view), reader, false);
        }

        private List<ViewColumnSchema> GetViewColumnsFromReader(IDictionary<string, ViewSchema> views, SafeDataReader reader, bool populateView)
        {
            List<ViewColumnSchema> columns = new List<ViewColumnSchema>();
            List<ExtendedProperty> properties = new List<ExtendedProperty>();

            while (reader.Read())
            {
                string name = reader.GetString("Name");
                string nativeType = reader.GetString("DataType");
                string systemType = reader.GetString("SystemType");
                DbType dataType = Utility.GetDbType(systemType);
                int size = reader.GetInt32("Length");
                byte precision = reader.GetByte("NumericPrecision");
                int scale = reader.GetInt32("NumericScale");
                bool allowDBNull = reader.GetBoolean("IsNullable");
                string defaultValue = reader.GetString("DefaultValue");
                bool isComputed = (reader.GetInt32("IsComputed") == 1);
                bool isDeterministic = reader.IsDBNull("IsDeterministic") || (reader.GetInt32("IsDeterministic") == 1);
                string computedDefinition = reader.GetString("ComputedDefinition");
                string collation = reader.GetString("Collation");
                int objectid = reader.GetInt32("ObjectId");
                string schemaName = reader.GetString("SchemaName");
                string viewName = reader.GetString("ViewName");

                properties.Clear();
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsComputed, isComputed, DbType.Boolean, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsDeterministic, isDeterministic, DbType.Boolean, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, defaultValue, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.ComputedDefinition, computedDefinition, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.Collation, collation, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.ObjectID, objectid, DbType.Int32, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.SystemType, systemType, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.UserDefinedType, nativeType, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty("CS_UserType", nativeType, DbType.String, PropertyStateEnum.ReadOnly)); // Added for backwards compatibility.

                ViewSchema view;
                if (!views.TryGetValue(SchemaObjectBase.FormatFullName(schemaName, viewName), out view))
                    continue;

                ViewColumnSchema column = new ViewColumnSchema(view, name, dataType,
                    string.IsNullOrEmpty(systemType) ? nativeType : systemType,
                    size, precision, scale, allowDBNull, properties.ToArray());

                if (populateView)
                    view.Columns.Add(column);

                columns.Add(column);
            }

            return columns;
        }
        #endregion

        #region GetTablePrimaryKey
        public PrimaryKeySchema GetTablePrimaryKey(string connectionString, TableSchema table)
        {
            PrimaryKeySchema pk = null;

            foreach (IndexSchema idx in table.Indexes)
            {
                if (idx.IsPrimaryKey)
                {
                    pk = new PrimaryKeySchema(table, idx.Name);

                    foreach (MemberColumnSchema mcs in idx.MemberColumns)
                        pk.MemberColumns.Add(mcs);

                    if (idx.ExtendedProperties.Contains(ExtendedPropertyNames.FileGroup))
                        pk.ExtendedProperties.Add(idx.ExtendedProperties[ExtendedPropertyNames.FileGroup]);

                    if (idx.ExtendedProperties.Contains(ExtendedPropertyNames.OriginalFillFactor))
                        pk.ExtendedProperties.Add(idx.ExtendedProperties[ExtendedPropertyNames.OriginalFillFactor]);

                    pk.ExtendedProperties.Add(new ExtendedProperty(ExtendedPropertyNames.IsClustered, idx.IsClustered, DbType.Boolean, PropertyStateEnum.ReadOnly));

                    break;
                }
            }

            return pk;
        }
        #endregion

        #region GetTableIndexes
        public IndexSchema[] GetTableIndexes(string connectionString, TableSchema table)
        {
            List<IndexSchema> indexes;

            SqlService sql = new SqlService(connectionString);
            sql.AddParameter("@tableName", SqlDbType.NVarChar, table.Name, 200);
            sql.AddParameter("@schemaName", SqlDbType.NVarChar, table.Owner, 200);

            string sqlString = SqlFactory.GetTableIndexes(GetSqlServerMajorVersion(connectionString), IsSqlAzure(connectionString));
            using (SafeDataReader reader = sql.ExecuteSafeReader(sqlString))
            {
                indexes = GetIndexesFromReader(table, reader);
            }

            return indexes.ToArray();
        }

        private List<IndexSchema> GetIndexesFromReader(TableSchema table, SafeDataReader reader)
        {
            return GetIndexesFromReader(Utility.ToDictionary(table), reader, false);
        }

        private List<IndexSchema> GetIndexesFromReader(IDictionary<string, TableSchema> tables, SafeDataReader reader, bool populateTable)
        {
            Dictionary<string, IndexSchema> indexes = new Dictionary<string, IndexSchema>();
            List<ExtendedProperty> properties = new List<ExtendedProperty>();

            while (reader.Read())
            {
                bool isNewIndex = false;

                string name = reader.GetString("IndexName");
                //int status = reader.GetInt32("Status");

                bool isPrimaryKey = reader.GetBoolean("IsPrimary");
                bool isUnique = reader.GetBoolean("IsUnique");
                bool isClustered = reader.GetBoolean("IsClustered");
                bool isIgnoreDupKey = reader.GetBoolean("IgnoreDupKey");
                bool isHypothetical = reader.GetBoolean("IsHypothetical");
                bool isPadIndex = reader.GetBoolean("IsPadIndex");
                bool isDRIUniqueKey = reader.GetBoolean("IsUniqueConstraint");
                bool isDRIIndex = reader.GetBoolean("IsIndex");
                bool isDropExist = false; //TODO fix me
                bool isNoRecompute = reader.GetBoolean("NoRecompute");
                bool isFullTextKey = reader.GetBoolean("IsFullTextKey");
                bool isTable = reader.GetBoolean("IsTable");
                bool isStatistics = reader.GetBoolean("IsStatistics");
                bool isAutoStatistics = reader.GetBoolean("IsAutoStatistics");
                bool isConstraint = reader.GetBoolean("IsUniqueConstraint");
                string schemaName = reader.GetString("SchemaName");
                string tableName = reader.GetString("ParentName");
                string memberColumn = reader.GetString("ColumnName");

                string fullIndexName = IndexSchema.FormatFullName(schemaName, tableName, name);

                if (Utility.IsAnyNullOrEmpty(name, tableName, memberColumn))
                    continue;

                TableSchema table;

                if (!tables.TryGetValue(SchemaObjectBase.FormatFullName(schemaName, tableName), out table))
                    continue;

                IndexSchema idx = null;

                if (indexes.ContainsKey(fullIndexName))
                    idx = indexes[fullIndexName];

                if (idx == null)
                {
                    properties.Clear();
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.FileGroup, reader.GetString("FileGroup"), DbType.AnsiString, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsFullTextKey, isFullTextKey, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsTableIndex, isTable, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsStatistics, isStatistics, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsAutoStatistics, isAutoStatistics, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsHypothetical, isHypothetical, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.IgnoreDuplicateKey, isIgnoreDupKey, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.PadIndex, isPadIndex, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.DRIPrimaryKey, isPrimaryKey, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.DRIUniqueKey, isDRIUniqueKey, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.DRIIndex, isDRIIndex, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.DropExist, isDropExist, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.NoRecompute, isNoRecompute, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsConstraint, isConstraint, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.OriginalFillFactor, reader.GetByte("FillFactor"), DbType.Byte, PropertyStateEnum.ReadOnly));

                    idx = new IndexSchema(table, name, isPrimaryKey, isUnique, isClustered, properties.ToArray());

                    indexes.Add(fullIndexName, idx);
                    isNewIndex = true;
                }


                properties.Clear();
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsDescending, reader.GetBoolean("IsDescending"), DbType.Boolean, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsComputed, reader.GetBoolean("IsComputed"), DbType.Boolean, PropertyStateEnum.ReadOnly));

                MemberColumnSchema mcs = new MemberColumnSchema(table.Columns[memberColumn], properties.ToArray());

                idx.MemberColumns.Add(mcs);

                if (populateTable)
                {
                    if (!table.Indexes.Contains(name))
                        table.Indexes.Add(idx);

                    if (isPrimaryKey)
                    {
                        if (isNewIndex)
                        {
                            properties.Clear();
                            properties.Add(new ExtendedProperty(ExtendedPropertyNames.FileGroup, reader.GetString("FileGroup"), DbType.AnsiString, PropertyStateEnum.ReadOnly));
                            properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsClustered, isClustered, DbType.Boolean, PropertyStateEnum.ReadOnly));
                            properties.Add(new ExtendedProperty(ExtendedPropertyNames.OriginalFillFactor, reader.GetByte("FillFactor"), DbType.Byte, PropertyStateEnum.ReadOnly));

                            PrimaryKeySchema pk = new PrimaryKeySchema(table, name, properties.ToArray());

                            table.PrimaryKey = pk;
                        }

                        table.PrimaryKey.MemberColumns.Add(mcs);
                    }
                }
            }

            return new List<IndexSchema>(indexes.Values);
        }
        #endregion

        #region GetTableKeys
        public TableKeySchema[] GetTableKeys(string connectionString, TableSchema table)
        {
            List<TableKeySchema> keys;

            SqlService sql = new SqlService(connectionString);
            sql.AddParameter("@tableName", SqlDbType.NVarChar, table.Name, 200);
            sql.AddParameter("@schemaName", SqlDbType.NVarChar, table.Owner, 200);

            string sqlString = IsSql2005OrNewer(connectionString) ? SqlScripts.GetTableKeys2005 : SqlScripts.GetTableKeys;
            using (SafeDataReader reader = sql.ExecuteSafeReader(sqlString))
            {
                keys = GetKeysFromReader(table, reader);
            }

            return keys.ToArray();
        }

        private List<TableKeySchema> GetKeysFromReader(TableSchema table, SafeDataReader reader)
        {
            return GetKeysFromReader(Utility.ToDictionary(table), reader, false);
        }

        private List<TableKeySchema> GetKeysFromReader(IDictionary<string, TableSchema> tables, SafeDataReader reader, bool populateTable)
        {
            Dictionary<string, TableKeySchema> keys = new Dictionary<string, TableKeySchema>();
            List<ExtendedProperty> properties = new List<ExtendedProperty>();

            DatabaseSchema database = tables.Values.Select(t => t.Database).FirstOrDefault();
            if (database == null)
                return new List<TableKeySchema>();

            while (reader.Read())
            {
                string name = reader.GetString("ConstraintName");
                string primaryTableOwner = reader.GetString("PrimaryTableOwner");
                string primaryTableName = reader.GetString("PrimaryTableName");
                string primaryColumnName = reader.GetString("PrimaryColumnName");
                string foreignTableOwner = reader.GetString("ForeignTableOwner");
                string foreignTableName = reader.GetString("ForeignTableName");
                string foreignColumnName = reader.GetString("ForeignColumnName");
                bool isNotForReplication = reader.GetBoolean("IsNotForReplication");
                bool cascadeDelete = (reader.GetByte("DeleteReferentialAction") == 1);
                bool cascadeUpdate = (reader.GetByte("UpdateReferentialAction") == 1);
                bool withNoCheck = reader.GetBoolean("WithNoCheck");


                if (Utility.IsAnyNullOrEmpty(name, primaryTableName, primaryColumnName, foreignTableName, foreignColumnName))
                    continue;

                string fullKeyName = string.Format("{0}.{1}",
                    TableKeySchema.FormatFullName(primaryTableOwner, primaryTableName, name),
                    SchemaObjectBase.FormatFullName(foreignTableOwner, foreignTableName));

                TableSchema primaryTable;
                if (!tables.TryGetValue(SchemaObjectBase.FormatFullName(primaryTableOwner, primaryTableName), out primaryTable))
                {
                    primaryTable = database.Tables[primaryTableOwner, primaryTableName];
                }

                TableSchema foreignTable;
                if (!tables.TryGetValue(SchemaObjectBase.FormatFullName(foreignTableOwner, foreignTableName), out foreignTable))
                {
                    foreignTable = database.Tables[foreignTableOwner, foreignTableName];
                }

                if (primaryTable == null || foreignTable == null)
                    continue;

                TableKeySchema key;

                if (!keys.TryGetValue(fullKeyName, out key))
                {
                    properties.Clear();
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.CascadeDelete, cascadeDelete, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.CascadeUpdate, cascadeUpdate, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsNotForReplication, isNotForReplication, DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.WithNoCheck, withNoCheck, DbType.Boolean, PropertyStateEnum.ReadOnly));

                    key = new TableKeySchema(name, foreignTable, primaryTable, properties.ToArray());
                    keys.Add(fullKeyName, key);

                    if (populateTable)
                    {
                        if (!ContainsKey(primaryTable, key))
                            primaryTable.Keys.Add(key);
                        if (!ContainsKey(foreignTable, key))
                            foreignTable.Keys.Add(key);
                    }
                }

                MemberColumnSchema primaryColumn = new MemberColumnSchema(primaryTable.Columns[primaryColumnName]);
                key.PrimaryKeyMemberColumns.Add(primaryColumn);

                MemberColumnSchema foreignColumn = new MemberColumnSchema(foreignTable.Columns[foreignColumnName]);
                key.ForeignKeyMemberColumns.Add(foreignColumn);
            }

            return new List<TableKeySchema>(keys.Values);
        }

        private bool ContainsKey(TableSchema tablesSchema, TableKeySchema keySchema)
        {
            return tablesSchema.Keys.Any(t => 
                t.Name == keySchema.Name 
                && t.PrimaryKeyTable.FullName == keySchema.PrimaryKeyTable.FullName 
                && t.ForeignKeyTable.FullName == keySchema.ForeignKeyTable.FullName);
        }

        #endregion

        #region GetTableData
        public DataTable GetTableData(string connectionString, TableSchema table)
        {
            string sqlstring = String.Format(SQL_GetObjectData, table.Owner, table.Name);

            SqlService sql = new SqlService(connectionString);
            DataTable tableData = sql.ExecuteSqlDataSet(sqlstring).Tables[0];

            return tableData;
        }
        #endregion

        #region GetViewData
        public DataTable GetViewData(string connectionString, ViewSchema view)
        {
            string sqlstring = String.Format(SQL_GetObjectData, view.Owner, view.Name);

            SqlService sql = new SqlService(connectionString);
            DataTable tableData = sql.ExecuteSqlDataSet(sqlstring).Tables[0];

            return tableData;
        }
        #endregion

        #region GetViewText
        public string GetViewText(string connectionString, ViewSchema view)
        {
            StringBuilder viewText = new StringBuilder();

            SqlService sql = new SqlService(connectionString);
            sql.AddParameter("@objectname", SqlDbType.NVarChar, String.Format("[{0}].[{1}]", view.Owner, view.Name.Replace("'", "''")), 200);
            using (SqlDataReader reader = sql.ExecuteSqlReader(SQL_GetObjectSource))
            {
                while (reader.Read())
                {
                    viewText.Append(reader.GetString(0));
                }
            }

            return viewText.ToString();
        }
        #endregion

        #region GetCommands
        public CommandSchema[] GetCommands(string connectionString, DatabaseSchema database)
        {
            List<CommandSchema> commands = new List<CommandSchema>();
            List<ExtendedProperty> properties = new List<ExtendedProperty>();
            SqlBuilder sqlstring = new SqlBuilder();
            int majorVersion = GetSqlServerMajorVersion(connectionString);
            bool isAzure = IsSqlAzure(connectionString);

            sqlstring.AppendStatement(SqlFactory.GetCommands(majorVersion, isAzure));
            if (database.DeepLoad)
            {
                sqlstring.AppendStatement(SqlFactory.GetAllCommandParameters(majorVersion));

                if (!isAzure)
                    sqlstring.AppendStatement(SqlFactory.GetExtendedData(majorVersion));
            }


            SqlService sql = new SqlService(connectionString);
            using (SafeDataReader reader = sql.ExecuteSafeReader(sqlstring))
            {
                while (reader.Read())
                {
                    properties.Clear();
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.ObjectID, reader.GetInt32(3), DbType.Int32, PropertyStateEnum.ReadOnly));

                    string type = reader.GetString(4).Trim().ToUpper();
                    bool isFunction = (type == "FN" || type == "FS" || type == "IF" || type == "TF");
                    if (isFunction && !database.IncludeFunctions)
                        continue;

                    bool isCLR = type == "FS" || type == "PC";
                    bool isScalarFunction = type == "FN" || type == "FS";
                    bool isTableValuedFunction = type == "TF" || type == "IF";

                    properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.ObjectType, type));

                    // Add an extended property for Assembly (CLR) stored-procedure.
                    properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsCLR, isCLR));

                    // Add an extended property for SQL scalar function.
                    properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsScalarFunction, isScalarFunction));

                    // Add an extended property for SQL table-valued-function.
                    properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsTableValuedFunction, isTableValuedFunction));

                    // Add an extended property for SQL inline table-valued function.
                    properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsInlineTableValuedFunction, type == "IF"));

                    // Add an extended property for SQL multi statement table-valued function.
                    properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.IsMultiStatementTableValuedFunction, type == "TF"));

                    var commandSchema = new CommandSchema(database, reader.GetString(0), reader.GetString(1), reader.GetDateTime(2), properties.ToArray());

                    commands.Add(commandSchema);
                }

                if (database.DeepLoad)
                {
                    if (reader.NextResult())
                        PopulateCommandParameters(reader, commands);
                    if (reader.NextResult())
                        PopulateCommandExtendedData(reader, commands);

                    PopulateCommandDescriptions(commands);
                }
            }

            return commands.ToArray();
        }
        #endregion

        #region Populate Command
        private void PopulateCommandParameters(SafeDataReader reader, IList<CommandSchema> commands)
        {
            if (commands == null || commands.Count < 1)
                return;

            Dictionary<string, CommandSchema> commandDictionary = new Dictionary<string, CommandSchema>();
            foreach (CommandSchema command in commands)
            {
                commandDictionary.Add(command.FullName, command);
                command.Parameters = new ParameterSchemaCollection();

                // We check to make sure it is not a scalar function. If it is then this type needs to be pulled down in the GetParametersFromReader.
                if (!Utility.ParseBooleanExtendedProperty(command, ExtendedPropertyNames.IsScalarFunction))
                    command.Parameters.Add(GetReturnParameter(command));
            }

            GetParametersFromReader(commandDictionary, reader, true);
        }

        private void PopulateCommandExtendedData(SafeDataReader reader, IList<CommandSchema> commands)
        {
            if (commands == null || commands.Count < 1)
                return;

            Dictionary<string, CommandSchema> commandDictionary = Utility.ToDictionary(commands);

            while (reader.Read())
            {
                string propertyName = reader.GetString("PropertyName");
                object propertyValue = reader["PropertyValue"];
                string propertyBaseType = reader.GetString("PropertyBaseType");

                DbType propertyType = string.IsNullOrEmpty(propertyBaseType) ?
                    DbType.Object : Utility.GetDbType(propertyBaseType);

                string objectName = reader.GetString("ObjectName");
                string objectOwner = reader.GetString("ObjectOwner");
                string objectType = reader.GetString("ObjectType").Trim();

                string fieldName = reader.GetString("FieldName");

                int minor = reader.GetInt32("Minor");

                if ((propertyName != null)
                    && propertyName.StartsWith("microsoft_database_tools", StringComparison.Ordinal))
                    continue;

                ExtendedProperty property = new ExtendedProperty(
                    propertyName, propertyValue, propertyType);

                if (objectType == "P")
                {
                    CommandSchema command;
                    commandDictionary.TryGetValue(SchemaObjectBase.FormatFullName(objectOwner, objectName), out command);

                    if (minor == 0 && string.IsNullOrEmpty(fieldName))
                    {
                        command.MarkLoaded();
                        command.ExtendedProperties.Add(property);
                    }
                }
            } //while read
        }

        private void PopulateCommandDescriptions(IEnumerable<CommandSchema> commands)
        {
            //mark all loaded
            foreach (CommandSchema c in commands)
            {
                SyncDescription(c);
            }
        }
        #endregion

        #region GetCommandParameters
        public ParameterSchema[] GetCommandParameters(string connectionString, CommandSchema command)
        {
            List<ParameterSchema> parameters = new List<ParameterSchema>();
            int majorVersion = GetSqlServerMajorVersion(connectionString);

            string sqlstring = SqlFactory.GetCommandParameters(majorVersion);

            SqlService sql = new SqlService(connectionString);
            sql.AddParameter("@CommandName", SqlDbType.NVarChar, command.Name);
            sql.AddParameter("@SchemaName", SqlDbType.NVarChar, command.Owner);

            // We check to make sure it is not a scalar function. If it is then this type needs to be pulled down in the GetParametersFromReader.
            if (!Utility.ParseBooleanExtendedProperty(command, ExtendedPropertyNames.IsScalarFunction))
                parameters.Add(GetReturnParameter(command));

            using (SafeDataReader reader = sql.ExecuteSafeReader(sqlstring))
            {
                parameters.AddRange(GetParametersFromReader(command, reader));
            }

            return parameters.ToArray();
        }

        private IEnumerable<ParameterSchema> GetParametersFromReader(CommandSchema command, SafeDataReader reader)
        {
            return GetParametersFromReader(Utility.ToDictionary(command), reader, false);
        }

        private List<ParameterSchema> GetParametersFromReader(IDictionary<string, CommandSchema> commands, SafeDataReader reader, bool populateCommand)
        {
            List<ExtendedProperty> properties = new List<ExtendedProperty>();
            List<ParameterSchema> parameters = new List<ParameterSchema>();

            while (reader.Read())
            {
                string name = reader.GetString("ParameterName");
                bool isOutput = reader.GetBoolean("IsOutput");
                ParameterDirection direction = isOutput ? ParameterDirection.InputOutput : ParameterDirection.Input;
                string nativeType = reader.GetString("BaseTypeName");
                DbType dataType = string.IsNullOrEmpty(nativeType) ? DbType.Object : Utility.GetDbType(nativeType);
                string userDefinedType = reader.GetString("TypeName");
                DbType userDefinedDataType = string.IsNullOrEmpty(userDefinedType) ? DbType.Object : Utility.GetDbType(userDefinedType);
                int size = reader.GetInt32("Length");
                byte precision = reader.GetByte("Precision");
                int scale = Convert.ToInt32(reader.GetByte("Scale"));
                const bool allowDBNull = true;
                string defaultValue = reader.GetString("DefaultValue");
                int parameterID = reader.GetInt32("ParameterID");
                string commandName = reader.GetString("CommandName");
                string schemaName = reader.GetString("SchemaName");

                if (Utility.IsAnyNullOrEmpty(name, commandName))
                {
                    // If the parameter is not an output parameter then continue.
                    if(!isOutput)
                        continue;

                    //NOTE: My findings are that every scalar function returns one empty result where the name is empty.
                    CommandSchema temp;
                    if (!commands.TryGetValue(SchemaObjectBase.FormatFullName(schemaName, commandName), out temp))
                        continue;

                    if(Utility.ParseBooleanExtendedProperty(temp, ExtendedPropertyNames.IsScalarFunction))
                    {
                        properties.Clear();
                        properties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, "0", DbType.String, PropertyStateEnum.ReadOnly));
                        properties.Add(new ExtendedProperty(ExtendedPropertyNames.ParameterID, 0, DbType.Int32, PropertyStateEnum.ReadOnly));

                        // Some documentation: http://msdn.microsoft.com/en-us/library/ms186755.aspx
                        var param = new ParameterSchema(temp, "@RETURN_VALUE", ParameterDirection.ReturnValue, dataType, nativeType, size, Convert.ToByte(precision), scale, allowDBNull, properties.ToArray());

                        if (populateCommand)
                            temp.Parameters.Add(param);

                        parameters.Add(param);
                    }

                    continue;
                }


                properties.Clear();
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, defaultValue, DbType.String, PropertyStateEnum.ReadOnly));
                properties.Add(new ExtendedProperty(ExtendedPropertyNames.ParameterID, parameterID, DbType.Int32, PropertyStateEnum.ReadOnly));

                // Check to see if the udt exists, if it does add it as an extended property.
                if (string.IsNullOrEmpty(nativeType) || (!string.IsNullOrEmpty(nativeType) && !nativeType.Equals(userDefinedType, StringComparison.InvariantCultureIgnoreCase)))
                {
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.IsUserDefinedTableType, string.IsNullOrEmpty(nativeType), DbType.Boolean, PropertyStateEnum.ReadOnly));
                    properties.Add(new ExtendedProperty(ExtendedPropertyNames.UserDefinedType, userDefinedType, userDefinedDataType, PropertyStateEnum.ReadOnly));
                }

                CommandSchema command;
                if (!commands.TryGetValue(SchemaObjectBase.FormatFullName(schemaName, commandName), out command))
                    continue;

                ParameterSchema parameter = new ParameterSchema(command, name, direction, dataType, nativeType, size, precision, scale, allowDBNull, properties.ToArray());

                if (populateCommand)
                    command.Parameters.Add(parameter);

                parameters.Add(parameter);
            }

            return parameters;
        }

        /// <summary>
        /// Adds a parameter for the default return value from a stored procedure.
        /// http://msdn.microsoft.com/en-us/library/ms174998.aspx; This is the 'error code' return value from SQL Server.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>The ParameterSchema</returns>
        private ParameterSchema GetReturnParameter(CommandSchema command)
        {
            const string name = "@RETURN_VALUE";
            const ParameterDirection direction = ParameterDirection.ReturnValue;
            const string nativeType = "int";
            const DbType dataType = DbType.Int32;
            const int size = 4;
            const short precision = 10;
            const int scale = 0;
            const bool allowDBNull = false;

            var properties = new List<ExtendedProperty>();
            properties.Add(new ExtendedProperty(ExtendedPropertyNames.DefaultValue, "0", DbType.String, PropertyStateEnum.ReadOnly));
            properties.Add(new ExtendedProperty(ExtendedPropertyNames.ParameterID, 0, DbType.Int32, PropertyStateEnum.ReadOnly));

            var parameter = new ParameterSchema(command, name, direction, dataType, nativeType, size, Convert.ToByte(precision), scale, allowDBNull, properties.ToArray());

            return parameter;
        }
        #endregion

        #region GetCommandText

        /// <summary>
        /// Gets the Command Text for the specified Command.
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <param name="command">The Command.</param>
        /// <returns>returns the command text if the procedure is not a CLR procedure; else returns string.Empty</returns>
        public string GetCommandText(string connectionString, CommandSchema command)
        {
            // NOTE: This code will only execute if the command is not a SQL-CLR procedure.
            if (!Utility.ParseBooleanExtendedProperty(command, ExtendedPropertyNames.IsCLR))
            {
                StringBuilder commandText = new StringBuilder();

                SqlService sql = new SqlService(connectionString);
                sql.AddParameter("@objectname", SqlDbType.NVarChar, String.Format("[{0}].[{1}]", command.Owner, command.Name.Replace("'", "''")), 200);

                using (SqlDataReader reader = sql.ExecuteSqlReader(SQL_GetObjectSource))
                {
                    while (reader.Read())
                    {
                        commandText.Append(reader.GetString(0));
                    }
                }

                return commandText.ToString();
            }

            return string.Empty;
        }

        #endregion

        #region GetCommandResultSchemas

        public CommandResultSchema[] GetCommandResultSchemas(string connectionString, CommandSchema command)
        {
            SqlService sql = new SqlService(connectionString);
            SqlDataReader reader = null;

            #region First attempt at getting the CommandResultSchema

            bool exceptionOnFMTQuery = false;
            bool shouldAttemptTransactionalQuery = true;

            try
            {
                reader = sql.ExecuteSqlReader(Utility.CommandResultSchemaFMTQuery(command));
            }
            catch (SqlException)
            {
                // try alternate method to retrieve schema information.
                exceptionOnFMTQuery = true;
            }

            // FMT Query failed so try Transactional Query.
            if (exceptionOnFMTQuery)
            {
                try
                {
                    shouldAttemptTransactionalQuery = false;
                    reader = sql.ExecuteSqlReader(Utility.CommandResultSchemaTransactionalQuery(command));
                }
                catch (SqlException ex)
                {
                    Trace.WriteLine(string.Format("An error occurred while getting CommandResultSchemas for the command: '{0}'\r\nException:{1}", command.Name, ex.Message));
                    return new CommandResultSchema[] {};
                }
            }

            #endregion

            // This will only be hit if the following conditions are met:
            // 1. The FMT query didn't fail.
            // 2. The FMT query failed but the transaction query succeeded.
            List<CommandResultSchema> results = new List<CommandResultSchema>();

            #region Process the CommandResultSchema results.

            try
            {
                results = CommandResultSchemaHelper(command, reader);
            }
            catch (SqlException)
            {
                if(!exceptionOnFMTQuery)
                    exceptionOnFMTQuery = true;
            }
            finally
            {
                if(reader != null && !reader.IsClosed)
                    reader.Close();
            }

            #region The FMT query failed and the transactional query hasn't been executed.

            if (exceptionOnFMTQuery && shouldAttemptTransactionalQuery)
            {
                try
                {
                    reader = sql.ExecuteSqlReader(Utility.CommandResultSchemaTransactionalQuery(command));
                    results = CommandResultSchemaHelper(command, reader);
                }
                catch (SqlException ex)
                {
                    Trace.WriteLine(string.Format("An error occurred while getting CommandResultSchemas for the command: '{0}'\r\nException:{1}", command.Name, ex.Message));
                }
                finally
                {
                    if (reader != null && !reader.IsClosed)
                        reader.Close();
                }
            }

            #endregion

            #endregion

            return results.ToArray();
        }

        private static List<CommandResultSchema> CommandResultSchemaHelper(CommandSchema command, IDataReader reader)
        {
            List<CommandResultSchema> results = new List<CommandResultSchema>();
            if (reader.IsClosed) return results;

            int resultCount = 0;

            do
            {
                DataTable schemaTable = reader.GetSchemaTable();
                if (schemaTable == null) continue;

                resultCount++;
                List<CommandResultColumnSchema> columns = new List<CommandResultColumnSchema>();

                for (int i = 0; i < schemaTable.Rows.Count; i++)
                {
                    string name = schemaTable.Rows[i].IsNull("ColumnName") ? "Column" + (i + 1) : (string)schemaTable.Rows[i]["ColumnName"];
                    if (string.IsNullOrEmpty(name))
                        name = "Column" + (i + 1);

                    SqlDbType sqlDbType = schemaTable.Rows[i].IsNull("ProviderType") ? SqlDbType.Variant : (SqlDbType)schemaTable.Rows[i]["ProviderType"];
                    string nativeType = Utility.GetNativeType(sqlDbType);
                    DbType dataType = Utility.GetDbType(nativeType);
                    int size = schemaTable.Rows[i].IsNull("ColumnSize") ? 0 : (int)schemaTable.Rows[i]["ColumnSize"];
                    byte precision = schemaTable.Rows[i].IsNull("NumericPrecision") ? (byte)0 : Convert.ToByte((short)schemaTable.Rows[i]["NumericPrecision"]);
                    int scale = schemaTable.Rows[i].IsNull("NumericScale") ? 0 : Convert.ToInt32((short)schemaTable.Rows[i]["NumericScale"]);
                    bool allowDBNull = !schemaTable.Rows[i].IsNull("AllowDBNull") && (bool)schemaTable.Rows[i]["AllowDBNull"];

                    CommandResultColumnSchema column = new CommandResultColumnSchema(command, name, dataType, nativeType, size, precision, scale, allowDBNull);
                    columns.Add(column);
                }

                CommandResultSchema result = new CommandResultSchema(command, resultCount.ToString(), columns.ToArray());

                results.Add(result);
                schemaTable.Dispose();

            } while (reader.NextResult());

            reader.Close();
            return results;
        }

        #endregion

        #endregion

        #region GetExtendedProperties

        public ExtendedProperty[] GetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            if (schemaObject is DatabaseSchema)
            {
                ExtendedPropertyCollection properties = GetDatabaseExtendedProperties(connectionString);
                return properties.ToArray();
            }
            if (schemaObject is TableSchema)
            {
                TableSchema table = (TableSchema)schemaObject;
                return GetExtendedProperties(connectionString, table.Owner, "Table", table.Name);
            }
            if (schemaObject is ColumnSchema)
            {
                ColumnSchema column = (ColumnSchema)schemaObject;
                ExtendedPropertyCollection properties = GetColumnExtendedProperties(connectionString, column);
                return properties.ToArray();
            }
            if (schemaObject is ViewSchema)
            {
                ViewSchema view = (ViewSchema)schemaObject;
                return GetExtendedProperties(connectionString, view.Owner, "View", view.Name);
            }
            if (schemaObject is ViewColumnSchema)
            {
                ViewColumnSchema viewColumn = (ViewColumnSchema)schemaObject;
                return GetExtendedProperties(connectionString, viewColumn.View.Owner, "View", viewColumn.View.Name, "Column", viewColumn.Name);
            }
            if (schemaObject is IndexSchema)
            {
                IndexSchema index = (IndexSchema)schemaObject;
                return GetExtendedProperties(connectionString, index.Table.Owner, "Table", index.Table.Name, "Index", index.Name);
            }
            if (schemaObject is CommandSchema)
            {
                CommandSchema command = (CommandSchema)schemaObject;
                return GetExtendedProperties(connectionString, command.Owner, "Procedure", command.Name);
            }
            if (schemaObject is ParameterSchema)
            {
                ParameterSchema parameter = (ParameterSchema)schemaObject;
                ExtendedPropertyCollection properties = GetParameterExtendedData(connectionString, parameter);
                return properties.ToArray();
            }
            if (schemaObject is PrimaryKeySchema)
            {
                PrimaryKeySchema primaryKey = (PrimaryKeySchema)schemaObject;
                return GetExtendedProperties(connectionString, primaryKey.Table.Owner, "Table", primaryKey.Table.Name, "Constraint", primaryKey.Name);
            }
            if (schemaObject is TableKeySchema)
            {
                TableKeySchema key = (TableKeySchema)schemaObject;
                return GetExtendedProperties(connectionString, key.ForeignKeyTable.Owner, "Table", key.ForeignKeyTable.Name, "Constraint", key.Name);
            }

            return new ExtendedProperty[0];
        }

        private ExtendedPropertyCollection GetParameterExtendedData(string connectionString, ParameterSchema parameter)
        {
            ExtendedPropertyCollection properties = new ExtendedPropertyCollection();
            properties.AddRange(GetExtendedProperties(connectionString, parameter.Command.Owner, "Procedure", parameter.Command.Name, "Parameter", parameter.Name));

            GetParameterExtendedData(parameter, properties);
            return properties;
        }

        private const string _whitespaceCommentRegex = @"
            (?:
                \s
              |
                --[^\r\n]*[\r\n]+
              |
                /\*(?:[^\*]|\*[^/])*\*/
            )
            ";

        private static readonly Regex _sprocHeaderRegex = new Regex(String.Format(@"
                ^[\ \t]*CREATE{0}+PROC(?:EDURE)?{0}+(?:\[?(?<owner>[a-zA-Z]\w*)\]?\s*\.\s*)?(?:\[?(?<name>[a-zA-Z]\w*)\]?)(?:{0}*\({0}*|{0}+)
            ", _whitespaceCommentRegex), RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex _sprocParamRegex = new Regex(String.Format(@"
                \G(?:{0}*,)?
                {0}*(?<pname>@[a-zA-Z@][\w@]*)
                (?:{0}+AS)?
                {0}+(?:(?<ptypeowner>[a-zA-Z]\w*)\s+\.\s+)?
                (?:(?<ptype>[a-zA-Z]\w*(?:\s*\(\s*\w+(?:\s*,\s*\d+)?\s*\))?))
                (?:{0}*={0}*(?:(?<pdefault>[A-Z0-9]+)|'(?<pdefault>(?:[^']|'')*)'))?
                (?:{0}+(?<poutput>OUT(?:PUT)?))?
                (?:{0}+(?<preadonly>READONLY))?
                (?:{0}*,)?
                (?:\s*--(?<pcomment>[^\r\n]*))?
            ", _whitespaceCommentRegex), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Dictionary<int, List<ParameterSchemaExtendedData>> _parameterSchemaExtendedDataCache = new Dictionary<int, List<ParameterSchemaExtendedData>>(); 
        private void GetParameterExtendedData(ParameterSchema parameter, ExtendedPropertyCollection properties)
        {
            if (parameter.Direction == ParameterDirection.ReturnValue)
                return;

            int key = parameter.Command.CommandText.GetHashCode();
            if (_parameterSchemaExtendedDataCache.ContainsKey(key) && _parameterSchemaExtendedDataCache[key] == null)
                return;

            if (!_parameterSchemaExtendedDataCache.ContainsKey(key))
            {
                Match match = _sprocHeaderRegex.Match(parameter.Command.CommandText);
                if(!match.Success)
                {
                    _parameterSchemaExtendedDataCache[key] = null;
                    return;
                }

                var results = new List<ParameterSchemaExtendedData>();
                foreach (Match p in _sprocParamRegex.Matches(parameter.Command.CommandText, match.Index + match.Length))
                {
                    string defaultValue = p.Groups["pdefault"].Value.Replace("''", "'");
                    string comment = p.Groups["pcomment"].Value.TrimEnd(new[] { ' ', '\r', '\n' });
                    results.Add(new ParameterSchemaExtendedData(p.Groups["pname"].Value, defaultValue, comment));
                }

                _parameterSchemaExtendedDataCache[key] = results;
            }

            var result = _parameterSchemaExtendedDataCache[key].FirstOrDefault(r => parameter.Name.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
            if (result == null)
                return;

            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.DefaultValue, result.DefaultValue));
            properties.Add(ExtendedProperty.Readonly(ExtendedPropertyNames.Comment, result.Comment));
        }

        private ExtendedPropertyCollection GetColumnExtendedProperties(string connectionString, ColumnSchema column)
        {
            ExtendedPropertyCollection properties = new ExtendedPropertyCollection();
            properties.AddRange(GetExtendedProperties(connectionString, column.Table.Owner, "Table", column.Table.Name, "Column", column.Name));

            if (!IsSql2000OrNewer(connectionString))
                return properties;

            int majorVersion = GetSqlServerMajorVersion(connectionString);
            string sqlstring = SqlFactory.GetColumnConstraints(majorVersion);
            sqlstring += SqlFactory.GetColumnConstraintsWhere(majorVersion);

            SqlService sql = new SqlService(connectionString);
            sql.AddParameter("@SchemaName", SqlDbType.NVarChar, column.Table.Owner);
            sql.AddParameter("@TableName", SqlDbType.NVarChar, column.Table.Name);
            sql.AddParameter("@ColumnName", SqlDbType.NVarChar, column.Name);

            using (SafeDataReader reader = sql.ExecuteSafeReader(sqlstring))
            {
                while (reader.Read())
                {
                    string name = reader.GetString("ConstraintName");
                    string type = reader.GetString("ConstraintType");
                    string definition = reader.GetString("ConstraintDef");

                    properties.Add(new ExtendedProperty(string.Format(ExtendedPropertyNames.ConstraintNameFormat, name), name, DbType.String));
                    properties.Add(new ExtendedProperty(string.Format(ExtendedPropertyNames.ConstraintTypeFormat, name), type, DbType.String));
                    properties.Add(new ExtendedProperty(string.Format(ExtendedPropertyNames.ConstraintDefinitionFormat, name), definition, DbType.String));
                }
            }

            return properties;
        }

        private ExtendedPropertyCollection GetDatabaseExtendedProperties(string connectionString)
        {
            ExtendedPropertyCollection properties = new ExtendedPropertyCollection();
            properties.AddRange(GetExtendedProperties(connectionString, "", "", "", "", "", ""));

            properties.Add(new ExtendedProperty(ExtendedPropertyNames.DatabaseVersion, GetSqlServerVersion(connectionString), DbType.String));
            properties.Add(new ExtendedProperty(ExtendedPropertyNames.DatabaseMajorVersion, GetSqlServerMajorVersion(connectionString), DbType.Int32));
            return properties;
        }

        private ExtendedProperty[] GetExtendedProperties(string connectionString, string owner, string level1type, string level1name)
        {
            return GetExtendedProperties(connectionString, GetLevelZero(connectionString),
                owner, level1type, level1name, "", "");
        }

        private ExtendedProperty[] GetExtendedProperties(string connectionString, string owner, string level1type, string level1name, string level2type, string level2name)
        {
            return GetExtendedProperties(connectionString, GetLevelZero(connectionString),
                owner, level1type, level1name, level2type, level2name);
        }

        private ExtendedProperty[] GetExtendedProperties(string connectionString, string level0type, string level0name, string level1type, string level1name, string level2type, string level2name)
        {
            if (!IsSql2000OrNewer(connectionString) || IsSqlAzure(connectionString))
                return new ExtendedProperty[0];

            var extendedProperties = new Dictionary<string, ExtendedProperty>(StringComparer.OrdinalIgnoreCase);
            string description = String.Empty;

            SqlService sql = new SqlService(connectionString);
            sql.AddParameter("@level0type", SqlDbType.VarChar, level0type, true);
            sql.AddParameter("@level0name", SqlDbType.VarChar, level0name, true);
            sql.AddParameter("@level1type", SqlDbType.VarChar, level1type, true);
            sql.AddParameter("@level1name", SqlDbType.VarChar, level1name, true);
            sql.AddParameter("@level2type", SqlDbType.VarChar, level2type, true);
            sql.AddParameter("@level2name", SqlDbType.VarChar, level2name, true);

            using (SafeDataReader reader = sql.ExecuteSafeReader(SqlScripts.GetExtendedProperties))
            {
                while (reader.Read())
                {
                    var extendedProperty = new ExtendedProperty(reader.GetString(0), reader.GetValue(1), reader.IsDBNull(2) ? DbType.Object : Utility.GetDbType(reader.GetString(2)));
                    extendedProperties[extendedProperty.Name] = extendedProperty;

                    if (extendedProperty.Name == "MS_Description" && extendedProperty.Value != null)
                        description = extendedProperty.Value.ToString();
                }
            }

            if (String.IsNullOrEmpty(description) && extendedProperties.ContainsKey(ExtendedPropertyNames.Description))
                description = extendedProperties[ExtendedPropertyNames.Description].Value as string ?? String.Empty;

            extendedProperties[ExtendedPropertyNames.Description] = ExtendedProperty.Readonly(ExtendedPropertyNames.Description, description);

            return extendedProperties.Values.ToArray();
        }
        #endregion

        #region Set Extended Properties
        public void SetExtendedProperties(string connectionString, SchemaObjectBase schemaObject)
        {
            if (schemaObject is DatabaseSchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    "",
                    "",
                    "",
                    "",
                    "",
                    "");
            }
            else if (schemaObject is TableSchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    GetLevelZero(connectionString),
                    ((TableSchema)schemaObject).Owner,
                    "Table",
                    schemaObject.Name,
                    "",
                    "");
            }
            else if (schemaObject is ColumnSchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    GetLevelZero(connectionString),
                    ((ColumnSchema)schemaObject).Table.Owner,
                    "Table",
                    ((ColumnSchema)schemaObject).Table.Name,
                    "Column",
                    schemaObject.Name);
            }
            else if (schemaObject is ViewSchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    GetLevelZero(connectionString),
                    ((ViewSchema)schemaObject).Owner,
                    "View",
                    schemaObject.Name,
                    "",
                    "");
            }
            else if (schemaObject is ViewColumnSchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    GetLevelZero(connectionString),
                    ((ViewColumnSchema)schemaObject).View.Owner,
                    "View",
                    ((ViewColumnSchema)schemaObject).View.Name,
                    "Column",
                    schemaObject.Name);
            }
            else if (schemaObject is IndexSchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    GetLevelZero(connectionString),
                    ((IndexSchema)schemaObject).Table.Owner,
                    "Table",
                    ((IndexSchema)schemaObject).Table.Name,
                    "Index",
                    schemaObject.Name);
            }
            else if (schemaObject is CommandSchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    GetLevelZero(connectionString),
                    ((CommandSchema)schemaObject).Owner,
                    "Procedure",
                    schemaObject.Name,
                    "",
                    "");
            }
            else if (schemaObject is ParameterSchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    GetLevelZero(connectionString),
                    ((ParameterSchema)schemaObject).Command.Owner,
                    "Procedure",
                    ((ParameterSchema)schemaObject).Command.Name,
                    "Parameter",
                    schemaObject.Name);
            }
            else if (schemaObject is PrimaryKeySchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    GetLevelZero(connectionString),
                    ((PrimaryKeySchema)schemaObject).Table.Owner,
                    "Table",
                    ((PrimaryKeySchema)schemaObject).Table.Name,
                    "Constraint",
                    schemaObject.Name);
            }
            else if (schemaObject is TableKeySchema)
            {
                SetExtendedProperties(
                    schemaObject,
                    connectionString,
                    GetLevelZero(connectionString),
                    ((TableKeySchema)schemaObject).ForeignKeyTable.Owner,
                    "Table",
                    ((TableKeySchema)schemaObject).ForeignKeyTable.Name,
                    "Constraint",
                    schemaObject.Name);
            }
        }

        private void SetExtendedProperties(
            SchemaObjectBase schemaObject,
            string connectionString,
            string level0type,
            string level0name,
            string level1type,
            string level1name,
            string level2type,
            string level2name)
        {
            SqlService sql = null;

            try
            {
                bool needRefresh = false;
                sql = new SqlService(connectionString);

                foreach (ExtendedProperty p in schemaObject.ExtendedProperties)
                {
                    string name = p.Name;
                    if (name == ExtendedPropertyNames.Description)
                    {
                        name = "MS_Description";
                        if (!schemaObject.ExtendedProperties.Contains(name))
                            p.PropertyState = PropertyStateEnum.New;
                    }

                    if (p.PropertyState == PropertyStateEnum.New)
                    {
                        sql.Reset();
                        sql.AddParameter("@name", SqlDbType.VarChar, name, true);
                        sql.AddParameter("@value", p.DataType, p.Value, true);
                        sql.AddParameter("@level0type", SqlDbType.VarChar, level0type, true);
                        sql.AddParameter("@level0name", SqlDbType.VarChar, level0name, true);
                        sql.AddParameter("@level1type", SqlDbType.VarChar, level1type, true);
                        sql.AddParameter("@level1name", SqlDbType.VarChar, level1name, true);
                        sql.AddParameter("@level2type", SqlDbType.VarChar, level2type, true);
                        sql.AddParameter("@level2name", SqlDbType.VarChar, level2name, true);
                        sql.ExecuteSP("sp_addextendedproperty");
                        needRefresh = true;
                    }
                    else if (p.PropertyState == PropertyStateEnum.Dirty)
                    {
                        sql.Reset();
                        sql.AddParameter("@name", SqlDbType.VarChar, name, true);
                        sql.AddParameter("@value", p.DataType, p.Value, true);
                        sql.AddParameter("@level0type", SqlDbType.VarChar, level0type, true);
                        sql.AddParameter("@level0name", SqlDbType.VarChar, level0name, true);
                        sql.AddParameter("@level1type", SqlDbType.VarChar, level1type, true);
                        sql.AddParameter("@level1name", SqlDbType.VarChar, level1name, true);
                        sql.AddParameter("@level2type", SqlDbType.VarChar, level2type, true);
                        sql.AddParameter("@level2name", SqlDbType.VarChar, level2name, true);
                        sql.ExecuteSP("sp_updateextendedproperty");
                        needRefresh = true;
                    }
                    else if (p.PropertyState == PropertyStateEnum.Deleted)
                    {
                        sql.Reset();
                        sql.AddParameter("@name", SqlDbType.VarChar, name, true);
                        sql.AddParameter("@level0type", SqlDbType.VarChar, level0type, true);
                        sql.AddParameter("@level0name", SqlDbType.VarChar, level0name, true);
                        sql.AddParameter("@level1type", SqlDbType.VarChar, level1type, true);
                        sql.AddParameter("@level1name", SqlDbType.VarChar, level1name, true);
                        sql.AddParameter("@level2type", SqlDbType.VarChar, level2type, true);
                        sql.AddParameter("@level2name", SqlDbType.VarChar, level2name, true);
                        sql.ExecuteSP("sp_dropextendedproperty");
                        needRefresh = true;
                    }
                } // for each

                if (needRefresh)
                    schemaObject.Refresh();
            }
            finally
            {
                if (sql != null)
                    sql.Disconnect();
            }

        }

        private string GetLevelZero(string connectionString)
        {
            return IsSql2005OrNewer(connectionString) ? "SCHEMA" : "USER";
        }

        #endregion

        #region SQL Server Version

        private Tuple<string, string> GetSqlServerVersion(string connectionString) {
            if (_sqlServerVersion == null || _sqlServerEdition == null) {
                lock (_lock) {
                    if (_sqlServerVersion != null && _sqlServerEdition != null)
                        return Tuple.Create(_sqlServerVersion, _sqlServerEdition);

                    try {
                        var sql = new SqlService(connectionString);
                        DataTable schemaTable = sql.ExecuteSqlDataSet(SQL_GetSqlServerVersion).Tables[0];

                        _sqlServerVersion = (string)schemaTable.Rows[0][0];
                        _sqlServerEdition = (string)schemaTable.Rows[0][1];
                    } catch {
                        _sqlServerVersion = _sqlServerEdition = String.Empty;
                    }
                }
            }

            return Tuple.Create(_sqlServerVersion, _sqlServerEdition);
        }

        private int GetSqlServerMajorVersion(string connectionString) {
            if (!_sqlServerMajorVersion.HasValue) {
                string version = GetSqlServerVersion(connectionString).Item1;
                if (version.Length > 0) {
                    int index = version.IndexOf('.');
                    _sqlServerMajorVersion = index > 0 ? Int32.Parse(version.Substring(0, index)) : 0;
                } else {
                    _sqlServerMajorVersion = 0;
                }
            }

            return _sqlServerMajorVersion.Value;
        }

        private bool IsSqlAzure(string connectionString) {
            return GetSqlServerVersion(connectionString).Item2.Equals("SQL Azure");
        }

        private bool IsSql2000OrNewer(string connectionString)
        {
            return GetSqlServerMajorVersion(connectionString) >= 8;
        }

        private bool IsSql2005OrNewer(string connectionString)
        {
            return GetSqlServerMajorVersion(connectionString) >= 9;
        }

        #endregion

        #region SQL Templates

        private const string SQL_GetDatabaseName = "SELECT db_name()";

        private const string SQL_GetObjectData = "SELECT * FROM [{0}].[{1}]";

        private const string SQL_GetObjectSource = "EXEC sp_helptext @objectname";

        private const string SQL_GetSqlServerVersion = "SELECT SERVERPROPERTY('ProductVersion'), SERVERPROPERTY('Edition')";

        #endregion

        #region IDbConnectionStringEditor Members

        public bool ShowEditor(string currentConnectionString)
        {
            bool userInstance = false;
            if (!String.IsNullOrEmpty(currentConnectionString))
            {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();

                // If the connection string is garbage then the following code will throw an exception.
                try
                {
                    builder.ConnectionString = currentConnectionString;
                }
                catch (ArgumentException) { }

                if (builder.ContainsKey("User Instance"))
                {
                    Boolean.TryParse(builder["User Instance"] as string, out userInstance);
                }
            }

            DataConnectionDialog dcd = new DataConnectionDialog();
            dcd.DataSources.Add(Microsoft.Data.ConnectionUI.DataSource.SqlDataSource);
            dcd.DataSources.Add(Microsoft.Data.ConnectionUI.DataSource.SqlFileDataSource);
            dcd.SelectedDataSource = userInstance ? Microsoft.Data.ConnectionUI.DataSource.SqlFileDataSource : Microsoft.Data.ConnectionUI.DataSource.SqlDataSource;
            if (!userInstance) dcd.SelectedDataProvider = DataProvider.SqlDataProvider;
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
