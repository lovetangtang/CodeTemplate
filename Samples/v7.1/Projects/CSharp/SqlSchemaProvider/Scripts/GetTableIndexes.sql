SELECT  [sysindexes].[name] AS [IndexName],         
		CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsClustered')) AS [IsClustered],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsUnique')) AS [IsUnique],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 4096) = 0 THEN 0 ELSE 1 END) AS [IsUniqueConstraint],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 2048) = 0 THEN 0 ELSE 1 END) AS [IsPrimary],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 0x1000000) = 0 THEN 0 ELSE 1 END) AS [NoRecompute],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 0x1) = 0 THEN 0 ELSE 1 END) AS [IgnoreDupKey],
        CONVERT(bit, CASE WHEN ([sysindexes].[status] & 6144) = 0 THEN 0 ELSE 1 END) AS [IsIndex],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsPadIndex')) AS [IsPadIndex],
        CONVERT(bit, OBJECTPROPERTY([sysindexes].[id], N'IsTable')) AS [IsTable],
        CONVERT(bit, OBJECTPROPERTY([sysindexes].[id], N'IsView')) AS [IsView],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsFulltextKey')) AS [IsFullTextKey],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsStatistics')) AS [IsStatistics],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsAutoStatistics')) AS [IsAutoStatistics],
        CONVERT(bit, INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsHypothetical')) AS [IsHypothetical],
        [sysfilegroups].[groupname] AS [FileGroup],
        [sysobjects].[name] AS [ParentName], 
        [sysusers].[name] AS [SchemaName], 
        [sysindexes].[OrigFillFactor] AS [FillFactor], 
        [sysindexes].[status] as [Status], 
        [syscolumns].[name] AS [ColumnName],
        CONVERT(bit, ISNULL(INDEXKEY_PROPERTY([syscolumns].[id], [sysindexkeys].[indid], [keyno], N'IsDescending'), 0)) AS [IsDescending],
        CONVERT(bit, ISNULL(INDEXKEY_PROPERTY([syscolumns].[id], [sysindexkeys].[indid], [keyno], N'IsComputed'), 0)) AS [IsComputed]
FROM [dbo].[sysindexes] WITH (NOLOCK) 
	INNER JOIN [dbo].[sysindexkeys] WITH (NOLOCK) ON [sysindexes].[indid] = [sysindexkeys].[indid] AND [sysindexkeys].[id] = [sysindexes].[id]
	INNER JOIN [dbo].[syscolumns] WITH (NOLOCK) ON [syscolumns].[colid] = [sysindexkeys].[colid] AND [syscolumns].[id] = [sysindexes].[id]
	INNER JOIN [dbo].[sysobjects] WITH (NOLOCK) ON [sysobjects].[id] = [sysindexes].[id] 
	LEFT JOIN [dbo].[sysusers] WITH (NOLOCK) ON [sysusers].[uid] = [sysobjects].[uid]
	LEFT JOIN [dbo].[sysfilegroups] WITH (NOLOCK) ON [sysfilegroups].[groupid] = [sysindexes].[groupid] 
WHERE   (OBJECTPROPERTY([sysindexes].[id], N'IsTable') = 1 OR OBJECTPROPERTY([sysindexes].[id], N'IsView') = 1)
	AND OBJECTPROPERTY([sysindexes].[id], N'IsSystemTable') = 0 
	AND INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsAutoStatistics') = 0
	AND INDEXPROPERTY([sysindexes].[id], [sysindexes].[name], N'IsHypothetical') = 0
	AND [sysindexes].[name] IS NOT NULL
	AND [sysobjects].[name] = @tableName
	AND [sysusers].[name] = @schemaName
ORDER   BY [sysindexes].[id], [sysindexes].[name], [sysindexkeys].[keyno]
