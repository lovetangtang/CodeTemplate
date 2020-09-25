﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34003
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SchemaExplorer {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SqlScripts {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SqlScripts() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SchemaExplorer.SqlScripts", typeof(SqlScripts).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	[t].[name] AS [CommandName],
        ///	[stbl].[name] AS [SchemaName],
        ///	[clmns].[name] AS [ParameterName],
        ///	CAST([clmns].[colid] AS int) AS [ParameterID],
        ///	CAST([clmns].[xprec] AS tinyint) AS [Precision],
        ///	[usrt].[name] AS [TypeName],
        ///	ISNULL([baset].[name], N&apos;&apos;) AS [BaseTypeName],
        ///	CAST(CASE WHEN [baset].[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN [clmns].[prec] ELSE [clmns].[length] END AS int) AS [Length],
        ///	CAST([clmns].[xscale] AS tinyint) AS [Scale],
        ///	CA [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetAllCommandParameters {
            get {
                return ResourceManager.GetString("GetAllCommandParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT [t].[name] AS [CommandName], 
        ///	[sc].[name] AS [SchemaName], 
        ///	[c].[name] AS [ParameterName], 
        ///	[c].[parameter_id] AS [ParameterID], 
        ///	[c].[precision] AS [Precision],
        ///	[types].[name] AS [TypeName],
        ///	[basetypes].[name] AS [BaseTypeName],
        ///	CASE WHEN [c].[max_length] &gt;= 0
        ///		AND [types].[name] IN (N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN [c].[max_length]/2 
        ///		ELSE [c].[max_length] 
        ///	END AS [Length],
        ///	[c].[scale] AS [Scale],
        ///	[is_output] as [IsOutput],
        ///	[default_value] as [DefaultValue]
        ///FROM [sys].[parameters] [c] WITH [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetAllCommandParameters2005 {
            get {
                return ResourceManager.GetString("GetAllCommandParameters2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	clmns.[name] AS [Name],
        ///	usrt.[name] AS [DataType],
        ///	ISNULL(baset.[name], N&apos;&apos;) AS [SystemType],
        ///	CAST(CASE WHEN baset.[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
        ///	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
        ///	CAST(clmns.xscale AS int) AS [NumericScale],
        ///	CAST(clmns.isnullable AS bit) AS [IsNullable],
        ///	defaults.text AS [DefaultValue],
        ///	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N&apos;IsIdentity [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetAllTableColumns {
            get {
                return ResourceManager.GetString("GetAllTableColumns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	clmns.[name] AS [Name],
        ///	usrt.[name] AS [DataType],
        ///	ISNULL(baset.[name], N&apos;&apos;) AS [SystemType],
        ///	CAST(CASE WHEN baset.[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
        ///	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
        ///	CAST(clmns.xscale AS int) AS [NumericScale],
        ///	CAST(clmns.isnullable AS bit) AS [IsNullable],
        ///	object_definition(defaults.default_object_id) AS [DefaultValue],
        ///	CAST(COLUMNPROPERTY(clmn [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetAllTableColumns2005 {
            get {
                return ResourceManager.GetString("GetAllTableColumns2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	clmns.[name] AS [Name],
        ///	usrt.[name] AS [DataType],
        ///	ISNULL(baset.[name], N&apos;&apos;) AS [SystemType],
        ///	CAST(CASE WHEN baset.[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
        ///	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
        ///	CAST(clmns.xscale AS int) AS [NumericScale],
        ///	CAST(clmns.isnullable AS bit) AS [IsNullable],
        ///	defaults.text AS [DefaultValue],
        ///	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N&apos;IsComputed [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetAllViewColumns {
            get {
                return ResourceManager.GetString("GetAllViewColumns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	clmns.[name] AS [Name],
        ///	usrt.[name] AS [DataType],
        ///	ISNULL(baset.[name], N&apos;&apos;) AS [SystemType],
        ///	CAST(CASE WHEN baset.[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
        ///	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
        ///	CAST(clmns.xscale AS int) AS [NumericScale],
        ///	CAST(clmns.isnullable AS bit) AS [IsNullable],
        ///	object_definition(defaults.default_object_id) AS [DefaultValue],
        ///	CAST(COLUMNPROPERTY(clmn [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetAllViewColumns2005 {
            get {
                return ResourceManager.GetString("GetAllViewColumns2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///  [tbl].[name] AS [TableName],
        ///  [stbl].[name] AS [SchemaName], 
        ///  [clmns].[name] AS [ColumnName],
        ///  OBJECT_NAME([const].[constid]) AS ConstraintName,
        ///  CASE
        ///    WHEN [const].[status] &amp; 5 = 5 THEN &apos;DEFAULT&apos;
        ///    WHEN [const].[status] &amp; 4 = 4 THEN &apos;CHECK&apos;
        ///    ELSE &apos;&apos;
        ///  END AS ConstraintType,
        ///  [constdef].[text] AS ConstraintDef
        ///FROM
        ///  dbo.sysobjects AS tbl WITH (NOLOCK)
        ///  INNER JOIN dbo.sysusers AS stbl WITH (NOLOCK) ON [stbl].[uid] = [tbl].[uid]
        ///  INNER JOIN dbo.syscolumns AS clmns WITH  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetColumnConstraints {
            get {
                return ResourceManager.GetString("GetColumnConstraints", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	t.Name AS [TableName],
        ///	SCHEMA_NAME(t.schema_id) AS [SchemaName], 
        ///	c.Name AS [ColumnName],
        ///	dc.Name AS ConstraintName,
        ///	&apos;DEFAULT&apos; AS ConstraintType,
        ///	dc.definition AS ConstraintDef
        ///FROM sys.tables t
        ///INNER JOIN sys.default_constraints dc ON t.object_id = dc.parent_object_id
        ///INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND c.column_id = dc.parent_column_id
        ///UNION ALL
        ///SELECT 
        ///	t.Name AS [TableName],
        ///	SCHEMA_NAME(t.schema_id) AS [SchemaName], 
        ///	c.Name AS [ColumnName],
        ///	c [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetColumnConstraints2005 {
            get {
                return ResourceManager.GetString("GetColumnConstraints2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	[t].[name] AS [CommandName],
        ///	[stbl].[name] AS [SchemaName],
        ///	[clmns].[name] AS [ParameterName],
        ///	CAST([clmns].[colid] AS int) AS [ParameterID],
        ///	CAST([clmns].[xprec] AS tinyint) AS [Precision],
        ///	[usrt].[name] AS [TypeName],
        ///	ISNULL([baset].[name], N&apos;&apos;) AS [BaseTypeName],
        ///	CAST(CASE WHEN [baset].[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN [clmns].[prec] ELSE [clmns].[length] END AS int) AS [Length],
        ///	CAST([clmns].[xscale] AS tinyint) AS [Scale],
        ///	CA [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetCommandParameters {
            get {
                return ResourceManager.GetString("GetCommandParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT [t].[name] AS [CommandName], 
        ///	[sc].[name] AS [SchemaName], 
        ///	[c].[name] AS [ParameterName], 
        ///	[c].[parameter_id] AS [ParameterID], 
        ///	[c].[precision] AS [Precision],
        ///	[types].[name] AS [TypeName],
        ///	[basetypes].[name] AS [BaseTypeName],
        ///	CASE WHEN [c].[max_length] &gt;= 0
        ///		AND [types].[name] IN (N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN [c].[max_length]/2 
        ///		ELSE [c].[max_length] 
        ///	END AS [Length],
        ///	[c].[scale] AS [Scale],
        ///	[is_output] as [IsOutput],
        ///	[default_value] as [DefaultValue]
        ///FROM [sys].[parameters] [c] WITH [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetCommandParameters2005 {
            get {
                return ResourceManager.GetString("GetCommandParameters2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///  object_name(id) AS OBJECT_NAME,
        ///  user_name(uid) AS USER_NAME,
        ///  crdate AS DATE_CREATED,
        ///  id as OBJECT_ID,
        ///  type as COMMAND_TYPE
        ///FROM
        ///  sysobjects
        ///WHERE
        ///  type IN (
        ///		N&apos;P&apos;, -- SQL Stored Procedure
        ///		N&apos;PC&apos;, --Assembly (CLR) stored-procedure
        ///		N&apos;FN&apos;, --SQL scalar function
        ///		N&apos;FS&apos;, --Assembly (CLR) scalar-function
        ///		N&apos;IF&apos;, --SQL inline table-valued function
        ///		N&apos;TF&apos; --SQL table-valued-function
        ///	  )
        ///  --AND permissions(id) &amp; 32 &lt;&gt; 0 
        ///  AND ObjectProperty(id, N&apos;IsMSShipped&apos;) = 0
        ///ORDE [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetCommands {
            get {
                return ResourceManager.GetString("GetCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///  object_name(id) AS OBJECT_NAME,
        ///  schema_name(uid) AS USER_NAME,
        ///  crdate AS DATE_CREATED,
        ///  id as OBJECT_ID,
        ///  type as COMMAND_TYPE
        ///FROM
        ///  sysobjects
        ///WHERE
        ///	type IN (
        ///		N&apos;P&apos;, -- SQL Stored Procedure
        ///		N&apos;PC&apos;, --Assembly (CLR) stored-procedure
        ///		N&apos;FN&apos;, --SQL scalar function
        ///		N&apos;FS&apos;, --Assembly (CLR) scalar-function
        ///		N&apos;IF&apos;, --SQL inline table-valued function
        ///		N&apos;TF&apos; --SQL table-valued-function
        ///	  )
        ///	  --AND permissions(id) &amp; 32 &lt;&gt; 0 
        ///	  AND ObjectProperty(id, N&apos;IsMSShipped&apos;) = 0
        ///	 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetCommands2005 {
            get {
                return ResourceManager.GetString("GetCommands2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///  object_name(id) AS OBJECT_NAME,
        ///  schema_name(uid) AS USER_NAME,
        ///  crdate AS DATE_CREATED,
        ///  id as OBJECT_ID,
        ///  type as COMMAND_TYPE
        ///FROM
        ///  sysobjects
        ///WHERE
        ///	type IN (
        ///		N&apos;P&apos;, -- SQL Stored Procedure
        ///		N&apos;PC&apos;, --Assembly (CLR) stored-procedure
        ///		N&apos;FN&apos;, --SQL scalar function
        ///		N&apos;FS&apos;, --Assembly (CLR) scalar-function
        ///		N&apos;IF&apos;, --SQL inline table-valued function
        ///		N&apos;TF&apos; --SQL table-valued-function
        ///	  )
        ///	  --AND permissions(id) &amp; 32 &lt;&gt; 0 
        ///	  AND ObjectProperty(id, N&apos;IsMSShipped&apos;) = 0
        ///O [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetCommandsAzure {
            get {
                return ResourceManager.GetString("GetCommandsAzure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [sp].[major_id] AS [ID], 
        ///        [so].[name] AS [ObjectName], 
        ///        SCHEMA_NAME([so].[schema_id]) AS [ObjectOwner],  
        ///        [so].[type] AS [ObjectType], 
        ///        [sp].[minor_id] AS [Minor],  
        ///        [sp].[name] AS [PropertyName], 
        ///        [sp].[value] AS [PropertyValue],
        ///        SQL_VARIANT_PROPERTY([sp].[value],&apos;BaseType&apos;) AS [PropertyBaseType],			    
        ///		CASE [sp].[class] WHEN 4 THEN USER_NAME([sp].[major_id]) END AS [UserName],
        ///        CASE [sp].[class]
        ///	        WHEN 2 THEN [spar].[name]
        ///	  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetExtendedData2005 {
            get {
                return ResourceManager.GetString("GetExtendedData2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///    p.name AS [PropertyName],
        ///    p.value AS [PropertyValue],
        ///    SQL_VARIANT_PROPERTY(p.value,&apos;BaseType&apos;) AS [PropertyBaseType],
        ///    SQL_VARIANT_PROPERTY(p.value,&apos;MaxLength&apos;) AS [PropertyMaxLength],
        ///    SQL_VARIANT_PROPERTY(p.value,&apos;Precision&apos;) AS [PropertyPrecision],
        ///    SQL_VARIANT_PROPERTY(p.value,&apos;Scale&apos;) AS [PropertyScale]
        ///FROM
        ///    ::fn_listextendedproperty(NULL, @level0type, @level0name, @level1type, @level1name, @level2type, @level2name) p.
        /// </summary>
        internal static string GetExtendedProperties {
            get {
                return ResourceManager.GetString("GetExtendedProperties", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT [sp].[id] AS [id], 
        ///	[so].[name] AS [ObjectName], 
        ///	[su].[name] AS [ObjectOwner],  
        ///	[so].[type] AS [ObjectType], 
        ///    CAST([sp].[smallid] AS INT) AS [Minor],
        ///	[sp].[type] AS [type], 
        ///	[sp].[name] AS [PropertyName], 
        ///	[sp].[value] AS [PropertyValue],
        ///    SQL_VARIANT_PROPERTY([sp].[value],&apos;BaseType&apos;) AS [PropertyBaseType],
        ///    CASE [sp].[type] WHEN 2 THEN USER_NAME([sp].[smallid]) END AS [UserName],
        ///    CASE [sp].[type] WHEN 1 THEN (SELECT TOP 1 [name] FROM [dbo].[systypes] WHERE [xusertype] [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetExtenedData {
            get {
                return ResourceManager.GetString("GetExtenedData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [sysindexes].[name] AS [IndexName],         
        ///		CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N&apos;IsClustered&apos;)) AS [IsClustered],
        ///        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N&apos;IsUnique&apos;)) AS [IsUnique],
        ///        CONVERT(bit, CASE WHEN ([sysindexes].[status] &amp; 4096) = 0 THEN 0 ELSE 1 END) AS [IsUniqueConstraint],
        ///        CONVERT(bit, CASE WHEN ([sysindexes].[status] &amp; 2048) = 0 THEN 0 ELSE 1 END) AS [IsPrimary],
        ///        CONVERT(bit, CASE WHEN ([sysinde [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetIndexes {
            get {
                return ResourceManager.GetString("GetIndexes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [i].[name] AS [IndexName],
        ///        CONVERT(bit, CASE [i].[type] WHEN 1 THEN 1 ELSE 0 END) AS [IsClustered],
        ///        [i].[is_unique] AS [IsUnique],
        ///        [i].[is_unique_constraint] AS [IsUniqueConstraint],
        ///        [i].[is_primary_key] AS [IsPrimary],
        ///        [s].[no_recompute] AS [NoRecompute], 
        ///        [i].[ignore_dup_key] AS [IgnoreDupKey],
        ///        CONVERT(bit, 0) AS [IsIndex], -- TODO, find value
        ///        [i].[is_padded] AS [IsPadIndex],
        ///        CONVERT(bit, CASE WHEN [o].[type] = &apos;U&apos; THEN 1 ELSE [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetIndexes2005 {
            get {
                return ResourceManager.GetString("GetIndexes2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [i].[name] AS [IndexName],
        ///        CONVERT(bit, CASE [i].[type] WHEN 1 THEN 1 ELSE 0 END) AS [IsClustered],
        ///        [i].[is_unique] AS [IsUnique],
        ///        [i].[is_unique_constraint] AS [IsUniqueConstraint],
        ///        [i].[is_primary_key] AS [IsPrimary],
        ///        [s].[no_recompute] AS [NoRecompute], 
        ///        [i].[ignore_dup_key] AS [IgnoreDupKey],
        ///        CONVERT(bit, 0) AS [IsIndex], -- TODO, find value
        ///        [i].[is_padded] AS [IsPadIndex],
        ///        CONVERT(bit, CASE WHEN [o].[type] = &apos;U&apos; THEN 1 ELSE [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetIndexesAzure {
            get {
                return ResourceManager.GetString("GetIndexesAzure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [fs].[name] AS [ForeignTableName], 
        ///        [fsysusers].[name] AS [ForeignTableOwner], 
        ///        [rs].[name] AS [PrimaryTableName], 
        ///        [rsysusers].[name] AS [PrimaryTableOwner], 
        ///        [cs].[name] AS [ConstraintName], 
        ///        [fc].[name] AS [ForeignColumnName],
        ///        [rc].[name] AS [PrimaryColumnName],
        ///        CONVERT(bit, OBJECTPROPERTY([constid], N&apos;CnstIsDisabled&apos;)) AS [Disabled],
        ///        CONVERT(bit, OBJECTPROPERTY([constid], N&apos;CnstIsNotRepl&apos;)) AS [IsNotForReplication],
        ///        CONVERT( [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetKeys {
            get {
                return ResourceManager.GetString("GetKeys", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [fs].[name] AS [ForeignTableName],
        ///        [fschemas].[name] AS [ForeignTableOwner],
        ///        [rs].[name] AS [PrimaryTableName], 
        ///        [rschemas].[name] AS [PrimaryTableOwner],
        ///        [sfk].[name] AS [ConstraintName],
        ///        [fc].[name] AS [ForeignColumnName],
        ///        [rc].[name] AS [PrimaryColumnName],
        ///        [sfk].[is_disabled] AS [Disabled],
        ///        [sfk].[is_not_for_replication] AS [IsNotForReplication],
        ///        [sfk].[update_referential_action] AS [UpdateReferentialAction],
        ///        [sfk].[ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetKeys2005 {
            get {
                return ResourceManager.GetString("GetKeys2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	clmns.[name] AS [Name],
        ///	usrt.[name] AS [DataType],
        ///	ISNULL(baset.[name], N&apos;&apos;) AS [SystemType],
        ///	CAST(CASE WHEN baset.[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
        ///	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
        ///	CAST(clmns.xscale AS int) AS [NumericScale],
        ///	CAST(clmns.isnullable AS bit) AS [IsNullable],
        ///	defaults.text AS [DefaultValue],
        ///	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N&apos;IsIdentity [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableColumns {
            get {
                return ResourceManager.GetString("GetTableColumns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	clmns.[name] AS [Name],
        ///	usrt.[name] AS [DataType],
        ///	ISNULL(baset.[name], N&apos;&apos;) AS [SystemType],
        ///	CAST(CASE WHEN baset.[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
        ///	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
        ///	CAST(clmns.xscale AS int) AS [NumericScale],
        ///	CAST(clmns.isnullable AS bit) AS [IsNullable],
        ///	object_definition(defaults.default_object_id) AS [DefaultValue],
        ///	CAST(COLUMNPROPERTY(clmn [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableColumns2005 {
            get {
                return ResourceManager.GetString("GetTableColumns2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [sysindexes].[name] AS [IndexName],         
        ///		CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N&apos;IsClustered&apos;)) AS [IsClustered],
        ///        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N&apos;IsUnique&apos;)) AS [IsUnique],
        ///        CONVERT(bit, CASE WHEN ([sysindexes].[status] &amp; 4096) = 0 THEN 0 ELSE 1 END) AS [IsUniqueConstraint],
        ///        CONVERT(bit, CASE WHEN ([sysindexes].[status] &amp; 2048) = 0 THEN 0 ELSE 1 END) AS [IsPrimary],
        ///        CONVERT(bit, CASE WHEN ([sysinde [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableIndexes {
            get {
                return ResourceManager.GetString("GetTableIndexes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [i].[name] AS [IndexName],
        ///        CONVERT(bit, CASE [i].[type] WHEN 1 THEN 1 ELSE 0 END) AS [IsClustered],
        ///        [i].[is_unique] AS [IsUnique],
        ///        [i].[is_unique_constraint] AS [IsUniqueConstraint],
        ///        [i].[is_primary_key] AS [IsPrimary],
        ///        [s].[no_recompute] AS [NoRecompute], 
        ///        [i].[ignore_dup_key] AS [IgnoreDupKey],
        ///        CONVERT(bit, 0) AS [IsIndex], -- TODO, find value
        ///        [i].[is_padded] AS [IsPadIndex],
        ///        CONVERT(bit, CASE WHEN [o].[type] = &apos;U&apos; THEN 1 ELSE [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableIndexes2005 {
            get {
                return ResourceManager.GetString("GetTableIndexes2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [i].[name] AS [IndexName],
        ///        CONVERT(bit, CASE [i].[type] WHEN 1 THEN 1 ELSE 0 END) AS [IsClustered],
        ///        [i].[is_unique] AS [IsUnique],
        ///        [i].[is_unique_constraint] AS [IsUniqueConstraint],
        ///        [i].[is_primary_key] AS [IsPrimary],
        ///        [s].[no_recompute] AS [NoRecompute], 
        ///        [i].[ignore_dup_key] AS [IgnoreDupKey],
        ///        CONVERT(bit, 0) AS [IsIndex], -- TODO, find value
        ///        [i].[is_padded] AS [IsPadIndex],
        ///        CONVERT(bit, CASE WHEN [o].[type] = &apos;U&apos; THEN 1 ELSE [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableIndexesAzure {
            get {
                return ResourceManager.GetString("GetTableIndexesAzure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [fs].[name] AS [ForeignTableName], 
        ///        [fsysusers].[name] AS [ForeignTableOwner], 
        ///        [rs].[name] AS [PrimaryTableName], 
        ///        [rsysusers].[name] AS [PrimaryTableOwner], 
        ///        [cs].[name] AS [ConstraintName], 
        ///        [fc].[name] AS [ForeignColumnName],
        ///        [rc].[name] AS [PrimaryColumnName],
        ///        CONVERT(bit, OBJECTPROPERTY([constid], N&apos;CnstIsDisabled&apos;)) AS [Disabled],
        ///        CONVERT(bit, OBJECTPROPERTY([constid], N&apos;CnstIsNotRepl&apos;)) AS [IsNotForReplication],
        ///        CONVERT( [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableKeys {
            get {
                return ResourceManager.GetString("GetTableKeys", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  [fs].[name] AS [ForeignTableName],
        ///        [fschemas].[name] AS [ForeignTableOwner],
        ///        [rs].[name] AS [PrimaryTableName], 
        ///        [rschemas].[name] AS [PrimaryTableOwner],
        ///        [sfk].[name] AS [ConstraintName],
        ///        [fc].[name] AS [ForeignColumnName],
        ///        [rc].[name] AS [PrimaryColumnName],
        ///        [sfk].[is_disabled] AS [Disabled],
        ///        [sfk].[is_not_for_replication] AS [IsNotForReplication],
        ///        [sfk].[update_referential_action] AS [UpdateReferentialAction],
        ///        [sfk].[ [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableKeys2005 {
            get {
                return ResourceManager.GetString("GetTableKeys2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///  object_name(id)	AS [OBJECT_NAME],
        ///  user_name(uid)	AS [USER_NAME],
        ///  type				AS TYPE,
        ///  crdate			AS DATE_CREATED,
        ///  &apos;&apos;				AS FILE_GROUP,
        ///  id				as [OBJECT_ID]
        ///FROM
        ///  sysobjects
        ///WHERE
        ///  type = N&apos;U&apos;
        ///  AND permissions(id) &amp; 4096 &lt;&gt; 0
        ///  AND ObjectProperty(id, N&apos;IsMSShipped&apos;) = 0
        ///ORDER BY user_name(uid), object_name(id).
        /// </summary>
        internal static string GetTables {
            get {
                return ResourceManager.GetString("GetTables", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///  TB.[OBJECT_NAME],
        ///  TB.[USER_NAME],
        ///  TB.[TYPE],
        ///  TB.[DATE_CREATED],
        ///  TB.[FILE_GROUP],
        ///  TB.[OBJECT_ID]
        ///FROM
        ///  (
        ///    SELECT
        ///      T.name AS [OBJECT_NAME],
        ///      SCHEMA_NAME(T.schema_id) AS [USER_NAME],
        ///      T.schema_id AS [SCHEMA_ID],
        ///      T.type AS [TYPE],
        ///      T.create_date AS [DATE_CREATED],
        ///      FG.file_group AS [FILE_GROUP],
        ///      T.object_id AS [OBJECT_ID],
        ///      HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(T.schema_id)) + &apos;.&apos; + QUOTENAME(T.name), &apos;OBJECT&apos;, &apos;SELECT&apos;) AS [HA [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTables2005 {
            get {
                return ResourceManager.GetString("GetTables2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///  TB.[OBJECT_NAME],
        ///  TB.[USER_NAME],
        ///  TB.[TYPE],
        ///  TB.[DATE_CREATED],
        ///  &apos;PRIMARY&apos; AS [FILE_GROUP],
        ///  TB.[OBJECT_ID]
        ///FROM
        ///  (
        ///    SELECT
        ///      T.name AS [OBJECT_NAME],
        ///      SCHEMA_NAME(T.schema_id) AS [USER_NAME],
        ///      T.schema_id AS [SCHEMA_ID],
        ///      T.type AS [TYPE],
        ///      T.create_date AS [DATE_CREATED],
        ///      T.object_id AS [OBJECT_ID],
        ///      HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(T.schema_id)) + &apos;.&apos; + QUOTENAME(T.name), &apos;OBJECT&apos;, &apos;SELECT&apos;) AS [HAVE_SELECT]
        ///    FROM
        ///       [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTablesAzure {
            get {
                return ResourceManager.GetString("GetTablesAzure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	clmns.[name] AS [Name],
        ///	usrt.[name] AS [DataType],
        ///	ISNULL(baset.[name], N&apos;&apos;) AS [SystemType],
        ///	CAST(CASE WHEN baset.[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
        ///	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
        ///	CAST(clmns.xscale AS int) AS [NumericScale],
        ///	CAST(clmns.isnullable AS bit) AS [IsNullable],
        ///	defaults.text AS [DefaultValue],
        ///	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N&apos;IsComputed [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetViewColumns {
            get {
                return ResourceManager.GetString("GetViewColumns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	clmns.[name] AS [Name],
        ///	usrt.[name] AS [DataType],
        ///	ISNULL(baset.[name], N&apos;&apos;) AS [SystemType],
        ///	CAST(CASE WHEN baset.[name] IN (N&apos;char&apos;, N&apos;varchar&apos;, N&apos;binary&apos;, N&apos;varbinary&apos;, N&apos;nchar&apos;, N&apos;nvarchar&apos;) THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
        ///	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
        ///	CAST(clmns.xscale AS int) AS [NumericScale],
        ///	CAST(clmns.isnullable AS bit) AS [IsNullable],
        ///	object_definition(defaults.default_object_id) AS [DefaultValue],
        ///	CAST(COLUMNPROPERTY(clmn [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetViewColumns2005 {
            get {
                return ResourceManager.GetString("GetViewColumns2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///  object_name(id) AS OBJECT_NAME,
        ///  user_name(uid) AS USER_NAME,
        ///  type AS TYPE,
        ///  crdate AS DATE_CREATED,
        ///  id as OBJECT_ID
        ///FROM sysobjects
        ///WHERE type = N&apos;V&apos;
        ///  AND permissions(id) &amp; 4096 &lt;&gt; 0
        ///  AND ObjectProperty(id, N&apos;IsMSShipped&apos;) = 0
        ///ORDER BY object_name(id).
        /// </summary>
        internal static string GetViews {
            get {
                return ResourceManager.GetString("GetViews", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  SELECT
        ///  object_name(id) AS OBJECT_NAME,
        ///  schema_name(uid) AS USER_NAME,
        ///  type AS TYPE,
        ///  crdate AS DATE_CREATED,
        ///  id as OBJECT_ID
        ///FROM sysobjects
        ///WHERE
        ///  type = N&apos;V&apos;
        ///  AND HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(uid)) + &apos;.&apos; + QUOTENAME(object_name(id)), &apos;OBJECT&apos;, &apos;SELECT&apos;) &lt;&gt; 0
        ///  AND ObjectProperty(id, N&apos;IsMSShipped&apos;) = 0
        ///  AND NOT EXISTS (SELECT * FROM sys.extended_properties WHERE major_id = id AND name = &apos;microsoft_database_tools_support&apos; AND value = 1)
        ///ORDER BY object_name(id).
        /// </summary>
        internal static string GetViews2005 {
            get {
                return ResourceManager.GetString("GetViews2005", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  SELECT
        ///  object_name(id) AS OBJECT_NAME,
        ///  schema_name(uid) AS USER_NAME,
        ///  type AS TYPE,
        ///  crdate AS DATE_CREATED,
        ///  id as OBJECT_ID
        ///FROM sysobjects
        ///WHERE
        ///  type = N&apos;V&apos;
        ///  AND HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(uid)) + &apos;.&apos; + QUOTENAME(object_name(id)), &apos;OBJECT&apos;, &apos;SELECT&apos;) &lt;&gt; 0
        ///  AND ObjectProperty(id, N&apos;IsMSShipped&apos;) = 0
        ///ORDER BY object_name(id).
        /// </summary>
        internal static string GetViewsAzure {
            get {
                return ResourceManager.GetString("GetViewsAzure", resourceCulture);
            }
        }
    }
}
