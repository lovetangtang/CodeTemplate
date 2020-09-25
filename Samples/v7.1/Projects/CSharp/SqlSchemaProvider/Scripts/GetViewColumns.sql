SELECT
	clmns.[name] AS [Name],
	usrt.[name] AS [DataType],
	ISNULL(baset.[name], N'') AS [SystemType],
	CAST(CASE WHEN baset.[name] IN (N'char', N'varchar', N'binary', N'varbinary', N'nchar', N'nvarchar') THEN clmns.prec ELSE clmns.length END AS int) AS [Length],
	CAST(clmns.xprec AS tinyint) AS [NumericPrecision],
	CAST(clmns.xscale AS int) AS [NumericScale],
	CAST(clmns.isnullable AS bit) AS [IsNullable],
	defaults.text AS [DefaultValue],
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsComputed') AS int) AS IsComputed,
	CAST(COLUMNPROPERTY(clmns.id, clmns.[name], N'IsDeterministic') AS int) AS IsDeterministic,
	cdef.[text] AS ComputedDefinition,
	clmns.[collation] AS Collation,
	CAST(clmns.colid AS int) AS ObjectId,
	stbl.[name] AS [SchemaName],
	tbl.[name] AS [ViewName]
FROM dbo.sysobjects AS tbl WITH (NOLOCK)
	INNER JOIN dbo.sysusers AS stbl WITH (NOLOCK) ON stbl.[uid] = tbl.[uid]
	INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON clmns.id=tbl.id
	LEFT JOIN dbo.systypes AS usrt WITH (NOLOCK) ON usrt.xusertype = clmns.xusertype
	LEFT JOIN dbo.sysusers AS sclmns WITH (NOLOCK) ON sclmns.uid = usrt.uid
	LEFT JOIN dbo.systypes AS baset WITH (NOLOCK) ON baset.xusertype = clmns.xtype and baset.xusertype = baset.xtype
	LEFT JOIN dbo.syscomments AS defaults WITH (NOLOCK) ON defaults.id = clmns.cdefault
	LEFT JOIN dbo.syscomments AS cdef WITH (NOLOCK) ON cdef.id = clmns.id AND cdef.number = clmns.colid
WHERE (tbl.[type] = 'V')
	AND stbl.[name] = @SchemaName
	AND tbl.[name] = @ViewName
ORDER BY tbl.[name], clmns.colorder