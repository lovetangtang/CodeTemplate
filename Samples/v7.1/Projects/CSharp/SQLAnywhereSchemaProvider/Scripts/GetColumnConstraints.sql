  -- Unable to test. Need DB with column Constraints
SELECT 
	t.table_name TableName,
	USER_NAME(t.creator) SchemaName,
	c.column_name ColumnName, 
	s.constraint_name ConstraintName,
	'CHECK' ConstraintType,
	 NULL ConstraintDef
FROM sys.SYSTAB t 
	JOIN sys.SYSCONSTRAINT s ON s.table_object_id = t.object_id
	JOIN sys.SYSTABCOL c ON c.object_id = s.ref_object_id
WHERE 
	t.table_type = 1 
	AND t.Creator in (USER_ID('dba'), user_id()) 
	AND s.constraint_type = 'C' 