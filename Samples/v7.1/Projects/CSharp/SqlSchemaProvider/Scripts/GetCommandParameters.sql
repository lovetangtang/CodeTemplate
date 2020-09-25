SELECT
	[t].[name] AS [CommandName],
	[stbl].[name] AS [SchemaName],
	[clmns].[name] AS [ParameterName],
	CAST([clmns].[colid] AS int) AS [ParameterID],
	CAST([clmns].[xprec] AS tinyint) AS [Precision],
	[usrt].[name] AS [TypeName],
	ISNULL([baset].[name], N'') AS [BaseTypeName],
	CAST(CASE WHEN [baset].[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN [clmns].[prec] ELSE [clmns].[length] END AS int) AS [Length],
	CAST([clmns].[xscale] AS tinyint) AS [Scale],
	CAST(CASE [clmns].[isoutparam] WHEN 1 THEN [clmns].[isoutparam] WHEN 0 THEN CASE COALESCE([clmns].[name], '') WHEN '' THEN 1 ELSE 0 END END AS bit) AS [IsOutput],
	[defaults].[text] AS [DefaultValue]	
FROM [dbo].[sysobjects] AS [t] WITH (NOLOCK)
	INNER JOIN [dbo].[sysusers] AS [stbl] WITH (NOLOCK) ON [stbl].[uid] = [t].[uid]
	INNER JOIN [dbo].[syscolumns] AS [clmns] WITH (NOLOCK) ON [clmns].[id] = [t].[id]
	LEFT JOIN [dbo].[systypes] AS [usrt] WITH (NOLOCK) ON [usrt].[xusertype] = [clmns].[xusertype]
	LEFT JOIN [dbo].[sysusers] AS [sclmns] WITH (NOLOCK) ON [sclmns].[uid] = [usrt].[uid]
	LEFT JOIN [dbo].[systypes] AS [baset] WITH (NOLOCK) ON [baset].[xusertype] = [clmns].[xtype] and [baset].[xusertype] = [baset].[xtype]
	LEFT JOIN [dbo].[syscomments] AS [defaults] WITH (NOLOCK) ON [defaults].[id] = [clmns].[cdefault]
WHERE [t].[type] IN ('P', 'RF', 'PC', 'FN', 'FS', 'IF', 'TF') 
	AND [t].[name] = @CommandName
	AND [stbl].[name]= @SchemaName
ORDER BY [t].[name], [clmns].[colorder]