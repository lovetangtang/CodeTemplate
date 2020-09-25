SELECT  [i].[name] AS [IndexName],
        CONVERT(bit, CASE [i].[type] WHEN 1 THEN 1 ELSE 0 END) AS [IsClustered],
        [i].[is_unique] AS [IsUnique],
        [i].[is_unique_constraint] AS [IsUniqueConstraint],
        [i].[is_primary_key] AS [IsPrimary],
        [s].[no_recompute] AS [NoRecompute], 
        [i].[ignore_dup_key] AS [IgnoreDupKey],
        CONVERT(bit, 0) AS [IsIndex], -- TODO, find value
        [i].[is_padded] AS [IsPadIndex],
        CONVERT(bit, CASE WHEN [o].[type] = 'U' THEN 1 ELSE 0 END) AS [IsTable],
        CONVERT(bit, CASE WHEN [o].[type] = 'V' THEN 1 ELSE 0 END) AS [IsView],
        CONVERT(bit, INDEXPROPERTY([i].[object_id], [i].[name], N'IsFulltextKey')) AS [IsFullTextKey],
        CONVERT(bit, 0) AS [IsStatistics], 
        CONVERT(bit, 0) AS [IsAutoStatistics], -- TODO, find value
        [i].[is_hypothetical] AS [IsHypothetical], 
        [fg].[name] AS [FileGroup],
        [o].[name] AS [ParentName],
        [os].[name] AS [SchemaName],
        [i].[fill_factor] AS [FillFactor],
        0 as [Status], -- TODO, find value
        [c].[name] AS [ColumnName],
        [ic].[is_descending_key] AS [IsDescending],
        CONVERT(bit, 0) AS [IsComputed] -- TODO, find value
FROM [sys].[indexes] i WITH (NOLOCK)
	LEFT JOIN [sys].[data_spaces] [fg] WITH (NOLOCK) ON [fg].[data_space_id] = [i].[data_space_id]
	LEFT JOIN [sys].[objects] [o] WITH (NOLOCK) ON [o].[object_id] = [i].[object_id]
	LEFT JOIN [sys].[schemas] [os] WITH (NOLOCK) ON [os].[schema_id] = [o].[schema_id]
	LEFT JOIN [sys].[index_columns] [ic] WITH (NOLOCK) ON [ic].[object_id] = [i].[object_id] AND [ic].[index_id] = [i].[index_id] AND [ic].[is_included_column] = 0
	LEFT JOIN [sys].[columns] [c] WITH (NOLOCK) ON [c].[object_id] = [ic].[object_id] AND [c].[column_id] = [ic].[column_id]
	LEFT JOIN [sys].[stats] [s] WITH (NOLOCK) ON [s].[object_id] = [i].[object_id] AND [s].[name] = [i].[name]
WHERE [i].[type] IN (0, 1, 2, 3)
	AND [o].[type] IN ('U', 'V', 'TF')
	AND [o].[name] = @tableName
	AND [os].[name] = @schemaName
ORDER BY [i].[object_id], [i].[name], [ic].[key_ordinal], [ic].[index_column_id]
