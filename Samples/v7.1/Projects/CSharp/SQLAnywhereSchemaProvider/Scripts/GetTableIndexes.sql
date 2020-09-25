SELECT 
	(CASE IsPrimary WHEN 1 THEN 'PK' ELSE '' END) + i.index_name IndexName,
	CAST((CASE WHEN (t.clustered_index_id = i.index_id) THEN 1 ELSE 0 END) AS integer) IsClustered,
	CAST((CASE WHEN (i."unique" < 3) THEN 1 ELSE 0 END) AS integer) IsUnique,
    CAST((CASE WHEN (i."unique" = 2) THEN 1 ELSE 0 END) AS integer) IsUniqueConstraint,
	CAST((CASE WHEN (i.index_category = 1) THEN 1 ELSE 0 END) AS integer) IsPrimary,
	CAST(0 AS integer) NoRecompute,
	CAST(0 AS integer) IgnoreDupKey,
	CAST((CASE WHEN (i.index_category = 1 AND i."unique" = 1) THEN 1 ELSE 0 END) AS integer) IsIndex,
	CAST(0 AS integer) IsPadIndex,
	CAST((CASE WHEN (t.table_type = 1) THEN 1 ELSE 0 END) AS integer) IsTable,
	CAST((CASE WHEN (t.table_type = 21) THEN 1 ELSE 0 END) AS integer) IsView,
	CAST((CASE WHEN (i.index_category = 4) THEN 1 ELSE 0 END) AS integer) IsFullTextKey,
	CAST(0 AS integer) IsStatistics,
	CAST(0 AS integer) IsAutoStatistics,
	CAST(0 AS integer) IsHypothetical,
	'PRIMARY' FileGroup,
	t.table_name ParentName, 
	USER_NAME(t.Creator) SchemaName, 
	CAST(0 AS integer) "FillFactor", 
	CAST(0 AS integer) Status, 
	c.column_name ColumnName,
 	CAST((CASE WHEN l."order" = 'D' THEN 1 ELSE 0 END) AS integer) IsDescending,
    CAST(0 AS integer) IsComputed
FROM sys.SYSTAB t 
	JOIN sys.SYSIDX i ON t.table_id = i.table_id 
	JOIN sys.SYSIDXCOL l ON l.index_id = i.index_id and l.table_id = i.table_id
	JOIN sys.SYSTABCOL c ON c.table_id = l.table_id and c.column_id = l.column_id
WHERE 
	i.index_category != 2
	AND t.table_name= ? 
	AND t.Creator = user_id( ? )
ORDER BY IndexName, l.sequence ASC
