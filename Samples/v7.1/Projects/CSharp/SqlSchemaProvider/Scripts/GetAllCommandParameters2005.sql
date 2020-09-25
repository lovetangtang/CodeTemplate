SELECT [t].[name] AS [CommandName], 
	[sc].[name] AS [SchemaName], 
	[c].[name] AS [ParameterName], 
	[c].[parameter_id] AS [ParameterID], 
	[c].[precision] AS [Precision],
	[types].[name] AS [TypeName],
	[basetypes].[name] AS [BaseTypeName],
	CASE WHEN [c].[max_length] >= 0
		AND [types].[name] IN (N'nchar', N'nvarchar') THEN [c].[max_length]/2 
		ELSE [c].[max_length] 
	END AS [Length],
	[c].[scale] AS [Scale],
	[is_output] as [IsOutput],
	[default_value] as [DefaultValue]
FROM [sys].[parameters] [c] WITH (NOLOCK)
	INNER JOIN [sys].[objects] [t] WITH (NOLOCK) ON [c].[object_id] = [t].[object_id]
	LEFT JOIN [sys].[schemas] [sc] WITH (NOLOCK) ON [t].[schema_id] = [sc].[schema_id]
	LEFT JOIN [sys].[types] [basetypes] WITH (NOLOCK) ON [c].[system_type_id] = [basetypes].[system_type_id] AND [basetypes].[system_type_id] = [basetypes].[user_type_id]
	LEFT JOIN [sys].[types] [types] WITH (NOLOCK) ON [c].[user_type_id] = [types].[user_type_id]
	LEFT JOIN [sys].[schemas] [st] WITH (NOLOCK) ON [st].[schema_id] = [types].[schema_id]
WHERE [t].[type] in ('P', 'RF', 'PC', 'FN', 'FS', 'IF', 'TF')
ORDER BY [t].[name], [c].[parameter_id]