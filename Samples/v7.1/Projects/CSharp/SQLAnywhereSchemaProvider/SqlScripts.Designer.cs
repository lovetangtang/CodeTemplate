﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
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
        ///	p.proc_name CommandName,
        ///    USER_NAME(p.creator) SchemaName,
        ///	c.parm_name ParameterName, 
        ///	c.parm_id ParameterID,
        ///	ISNULL((CASE d.domain_name WHEN &apos;numeric&apos; THEN Length ELSE d.&quot;precision&quot; END), 0) &quot;Precision&quot;, 
        ///	d.domain_name TypeName,
        ///	(CASE LOCATE(c.base_type_str, &apos;(&apos;) WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, &apos;(&apos;) - 1) END) BaseTypeName,
        ///	c.width Length, 
        ///	c.scale Scale, 
        ///	Cast ((CASE c.parm_mode_out WHEN &apos;N&apos; THEN 0 ELSE 1 END) as bit) IsOutput, 
        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetAllCommandParameters {
            get {
                return ResourceManager.GetString("GetAllCommandParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	c.column_name Name, 
        ///	d.domain_name AS DataType, 
        ///	(CASE LOCATE(c.base_type_str, &apos;(&apos;) WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, &apos;(&apos;) - 1) END) SystemType, 
        ///	c.width Length,
        ///	ISNULL((CASE d.domain_name WHEN &apos;numeric&apos; THEN Length ELSE d.&quot;precision&quot; END), 0) &quot;NumericPrecision&quot;,
        ///	c.scale NumericScale , 
        ///	CAST((CASE c.nulls WHEN &apos;Y&apos; THEN 1 ELSE 0 END) AS bit) IsNullable, 
        ///	c.&quot;default&quot; DefaultValue, 
        ///	CAST((CASE WHEN DefaultValue = &apos;autoincrement&apos; OR DefaultV [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetAllTableColumns {
            get {
                return ResourceManager.GetString("GetAllTableColumns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	c.column_name Name, 
        ///	d.domain_name AS DataType, 
        ///	(CASE LOCATE(c.base_type_str, &apos;(&apos;) WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, &apos;(&apos;) - 1) END) SystemType, 
        ///	c.width Length,
        ///	ISNULL((CASE d.domain_name WHEN &apos;numeric&apos; THEN Length ELSE d.&quot;precision&quot; END), 0) &quot;NumericPrecision&quot;,
        ///	c.scale NumericScale , 
        ///	CAST((CASE c.nulls WHEN &apos;Y&apos; THEN 1 ELSE 0 END) AS bit) IsNullable, 
        ///	c.&quot;default&quot; DefaultValue, 
        ///	CAST((CASE WHEN c.column_type = &apos;C&apos; THEN 1 ELSE 0 END) AS  [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetAllViewColumns {
            get {
                return ResourceManager.GetString("GetAllViewColumns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to   -- Unable to test. Need DB with column Constraints
        ///SELECT 
        ///	t.table_name TableName,
        ///	USER_NAME(t.creator) SchemaName,
        ///	c.column_name ColumnName, 
        ///	s.constraint_name ConstraintName,
        ///	&apos;CHECK&apos; ConstraintType,
        ///	 NULL ConstraintDef
        ///FROM sys.SYSTAB t 
        ///	JOIN sys.SYSCONSTRAINT s ON s.table_object_id = t.object_id
        ///	JOIN sys.SYSTABCOL c ON c.object_id = s.ref_object_id
        ///WHERE 
        ///	t.table_type = 1 
        ///	AND t.Creator in (USER_ID(&apos;dba&apos;), user_id()) 
        ///	AND s.constraint_type = &apos;C&apos; .
        /// </summary>
        internal static string GetColumnConstraints {
            get {
                return ResourceManager.GetString("GetColumnConstraints", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	p.proc_name CommandName,
        ///    USER_NAME(p.creator) SchemaName,
        ///	c.parm_name ParameterName, 
        ///	c.parm_id ParameterID,
        ///	ISNULL((CASE d.domain_name WHEN &apos;numeric&apos; THEN Length ELSE d.&quot;precision&quot; END), 0) &quot;Precision&quot;, 
        ///	d.domain_name TypeName,
        ///	(CASE LOCATE(c.base_type_str, &apos;(&apos;) WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, &apos;(&apos;) - 1) END) BaseTypeName,
        ///	c.width Length, 
        ///	c.scale Scale, 
        ///	Cast ((CASE c.parm_mode_out WHEN &apos;N&apos; THEN 0 ELSE 1 END) as bit) IsOutput, 
        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetCommandParameters {
            get {
                return ResourceManager.GetString("GetCommandParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	c.parm_name Name, 
        ///	ISNULL((CASE d.domain_name WHEN &apos;numeric&apos; THEN Length ELSE d.&quot;precision&quot; END), 0) AS NumericPrecision, 
        ///	c.scale NumericScale, 
        ///	c.width Length, 
        ///	d.domain_name TypeName,
        ///	(CASE LOCATE(c.base_type_str, &apos;(&apos;) WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, &apos;(&apos;) - 1) END) BaseTypeName,
        ///	&apos;Y&apos; AllowDBNull
        ///FROM sys.SYSPROCEDURE p 
        ///	JOIN sys.SYSPROCPARM c ON p.proc_id = c.proc_id 
        ///	JOIN sys.SYSDOMAIN d ON c.domain_id = d.domain_id
        ///WHERE 
        ///	p.pro [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetCommandResultSchema {
            get {
                return ResourceManager.GetString("GetCommandResultSchema", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	t.proc_name OBJECT_NAME, 
        ///	USER_NAME(t.creator) USER_NAME, 
        ///	o.creation_time DATE_CREATED, 
        ///	t.object_id OBJECT_ID,
        ///	&apos;P&apos; COMMAND_TYPE 
        ///FROM sys.SYSPROCEDURE t 
        ///	JOIN sys.SYSOBJECT o ON t.object_id = o.object_id 
        ///WHERE 
        ///	t.source IS NOT NULL 
        ///	AND t.Creator in (USER_ID(&apos;dbo&apos;), user_id()) 
        ///	ORDER BY OBJECT_NAME.
        /// </summary>
        internal static string GetCommands {
            get {
                return ResourceManager.GetString("GetCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT  
        ///	t.object_id ID, 
        ///	t.table_name ObjectName, 
        ///	USER_NAME(t.creator) ObjectOwner,  
        ///	(CASE t.table_type WHEN 1 THEN &apos;U&apos; ELSE &apos;V&apos; END) ObjectType, 
        ///	CAST(0 as integer) Minor,  
        ///	&apos;MS_Description&apos; PropertyName, 
        ///	r.remarks PropertyValue,
        ///	&apos;varchar&apos; PropertyBaseType,			    
        ///	NULL UserName,
        ///	NULL FieldName,
        ///	NULL IndexName,
        ///	NULL ParentName,
        ///	NULL  ParentOwner,
        ///	NULL  ParentType,
        ///	CAST(1 AS integer) Type        
        ///FROM SYS.SYSREMARK r 
        ///	JOIN SYS.SYSTAB t ON t.object_id = r.object_id
        ///WHERE        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetExtendedData {
            get {
                return ResourceManager.GetString("GetExtendedData", resourceCulture);
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
        ///   Looks up a localized string similar to SELECT 
        ///	(CASE IsPrimary WHEN 1 THEN &apos;PK&apos; ELSE &apos;&apos; END) + i.index_name IndexName,
        ///	CAST((CASE WHEN (t.clustered_index_id = i.index_id) THEN 1 ELSE 0 END) AS integer) IsClustered,
        ///	CAST((CASE WHEN (i.&quot;unique&quot; &lt; 3) THEN 1 ELSE 0 END) AS integer) IsUnique,
        ///    CAST((CASE WHEN (i.&quot;unique&quot; = 2) THEN 1 ELSE 0 END) AS integer) IsUniqueConstraint,
        ///	CAST((CASE WHEN (i.index_category = 1) THEN 1 ELSE 0 END) AS integer) IsPrimary,
        ///	CAST(0 AS integer) NoRecompute,
        ///	CAST(0 AS integer) IgnoreDupKey,
        ///	CAST((CASE WH [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetIndexes {
            get {
                return ResourceManager.GetString("GetIndexes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	t.table_name ForeignTableName,     
        ///	USER_NAME(t.Creator) ForeignTableOwner, 
        ///	p.table_name PrimaryTableName, 
        ///	USER_NAME(p.Creator) PrimaryTableOwner, 
        ///	p.table_name + t.table_name ConstraintName,
        ///	fc.column_name ForeignColumnName,
        ///	pc.column_name PrimaryColumnName,
        ///	CAST(0 AS integer) Disabled,
        ///	CAST(0 AS integer) IsNotForReplication,
        ///	CAST(1 AS integer) UpdateReferentialAction,
        ///	CAST(1 AS integer) DeleteReferentialAction,
        ///	CAST(1 AS integer) AS WithNoCheck
        ///FROM sys.SYSTAB t 
        ///	JOIN s [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetKeys {
            get {
                return ResourceManager.GetString("GetKeys", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	c.column_name Name, 
        ///	d.domain_name DataType, 
        ///	(CASE LOCATE(c.base_type_str, &apos;(&apos;) WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, &apos;(&apos;) - 1) END) SystemType,
        ///	c.width Length,
        ///	ISNULL((CASE d.domain_name WHEN &apos;numeric&apos; THEN Length ELSE d.&quot;precision&quot; END), 0) &quot;NumericPrecision&quot;,
        ///	c.scale NumericScale , 
        ///	CAST((CASE c.nulls WHEN &apos;Y&apos; THEN 1 ELSE 0 END) AS bit) IsNullable, 
        ///	c.&quot;default&quot; DefaultValue, 
        ///	CAST((CASE WHEN c.column_type = &apos;C&apos; THEN 1 ELSE 0 END) AS inte [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableColumns {
            get {
                return ResourceManager.GetString("GetTableColumns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	(CASE IsPrimary WHEN 1 THEN &apos;PK&apos; ELSE &apos;&apos; END) + i.index_name IndexName,
        ///	CAST((CASE WHEN (t.clustered_index_id = i.index_id) THEN 1 ELSE 0 END) AS integer) IsClustered,
        ///	CAST((CASE WHEN (i.&quot;unique&quot; &lt; 3) THEN 1 ELSE 0 END) AS integer) IsUnique,
        ///    CAST((CASE WHEN (i.&quot;unique&quot; = 2) THEN 1 ELSE 0 END) AS integer) IsUniqueConstraint,
        ///	CAST((CASE WHEN (i.index_category = 1) THEN 1 ELSE 0 END) AS integer) IsPrimary,
        ///	CAST(0 AS integer) NoRecompute,
        ///	CAST(0 AS integer) IgnoreDupKey,
        ///	CAST((CASE WH [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableIndexes {
            get {
                return ResourceManager.GetString("GetTableIndexes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT
        ///	f.table_name ForeignTableName,     
        ///	USER_NAME(f.Creator) ForeignTableOwner, 
        ///	p.table_name PrimaryTableName, 
        ///	USER_NAME(p.Creator) PrimaryTableOwner, 
        ///	p.table_name + f.table_name ConstraintName,
        ///	fc.column_name ForeignColumnName,
        ///	pc.column_name PrimaryColumnName,
        ///	CAST(0 AS integer) Disabled,
        ///	CAST(0 AS integer) IsNotForReplication,
        ///	CAST(1 AS integer) UpdateReferentialAction,
        ///	CAST(1 AS integer) DeleteReferentialAction,
        ///	CAST(1 AS integer) AS WithNoCheck
        ///FROM sys.SYSTAB f 
        ///	JOIN s [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetTableKeys {
            get {
                return ResourceManager.GetString("GetTableKeys", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	t.table_name OBJECT_NAME, 
        ///	USER_NAME(t.Creator) USER_NAME, 
        ///	&apos;U&apos; TYPE,
        ///	o.creation_time DATE_CREATED, 
        ///	&apos;&apos; FILE_GROUP, 
        ///	t.object_id OBJECT_ID 
        ///FROM sys.SYSTAB t 
        ///	JOIN sys.SYSOBJECT o ON t.object_id = o.object_id 
        ///WHERE 
        ///	t.table_type = 1 
        ///	AND t.Creator in (USER_ID(&apos;dba&apos;), user_id()) 
        ///	ORDER BY 1,2
        ///.
        /// </summary>
        internal static string GetTables {
            get {
                return ResourceManager.GetString("GetTables", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	c.column_name Name, 
        ///	d.domain_name DataType, 
        ///	(CASE LOCATE(c.base_type_str, &apos;(&apos;) WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, &apos;(&apos;) - 1) END) SystemType,
        ///	c.width Length,
        ///	ISNULL((CASE d.domain_name WHEN &apos;numeric&apos; THEN Length ELSE d.&quot;precision&quot; END), 0) &quot;NumericPrecision&quot;,
        ///	c.scale NumericScale , 
        ///	CAST((CASE c.nulls WHEN &apos;Y&apos; THEN 1 ELSE 0 END) AS bit) IsNullable, 
        ///	c.&quot;default&quot; DefaultValue, 
        ///	CAST((CASE WHEN c.column_type = &apos;C&apos; THEN 1 ELSE 0 END) AS inte [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string GetViewColumns {
            get {
                return ResourceManager.GetString("GetViewColumns", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SELECT 
        ///	t.table_name OBJECT_NAME, 
        ///	USER_NAME(t.Creator) USER_NAME, 
        ///	&apos;V&apos; TYPE,
        ///	o.creation_time DATE_CREATED, 
        ///	t.object_id OBJECT_ID 
        ///FROM sys.SYSTAB t 
        ///	JOIN sys.SYSOBJECT o ON t.object_id = o.object_id 
        ///WHERE 
        ///	t.table_type = 21 
        ///	AND t.Creator in (USER_ID(&apos;dba&apos;), user_id()) 
        ///	ORDER BY 1,2.
        /// </summary>
        internal static string GetViews {
            get {
                return ResourceManager.GetString("GetViews", resourceCulture);
            }
        }
    }
}
