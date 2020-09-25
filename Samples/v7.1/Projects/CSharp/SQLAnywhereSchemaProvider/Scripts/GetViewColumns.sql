SELECT 
	c.column_name Name, 
	d.domain_name DataType, 
	(CASE LOCATE(c.base_type_str, '(') WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, '(') - 1) END) SystemType,
	c.width Length,
	ISNULL((CASE d.domain_name WHEN 'numeric' THEN Length WHEN 'decimal' THEN Length ELSE d."precision" END), 0) "NumericPrecision",
	c.scale NumericScale , 
	CAST((CASE c.nulls WHEN 'Y' THEN 1 ELSE 0 END) AS bit) IsNullable, 
	c."default" DefaultValue, 
	CAST((CASE WHEN c.column_type = 'C' THEN 1 ELSE 0 END) AS integer) IsComputed,
	CAST(0 AS integer) IsDeterministic,
	CAST((CASE IsComputed WHEN 1 THEN c."default" ELSE NULL END) AS VARCHAR(2048)) ComputedDefinition,
	NULL Collation,
	c.column_id ObjectId,
	USER_NAME(t.creator) SchemaName,
	t.table_name TableName
FROM sys.SYSTAB t 
	JOIN sys.SYSTABCOL c ON t.table_id = c.table_id 
	JOIN sys.SYSDOMAIN d ON c.domain_id = d.domain_id 
WHERE 
	t.table_name = ?
	AND t.Creator = USER_ID( ? )
ORDER by ObjectId