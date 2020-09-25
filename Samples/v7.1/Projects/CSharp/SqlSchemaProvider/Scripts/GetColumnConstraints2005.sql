WITH constraints AS (
    SELECT parent_object_id, parent_column_id, Name, definition, 'DEFAULT' AS [ConstraintType] FROM sys.default_constraints
    UNION ALL
    SELECT parent_object_id, parent_column_id, Name, definition, 'CHECK' AS [ConstraintType]  FROM sys.check_constraints)

SELECT 
    t.Name AS [TableName],
    SCHEMA_NAME(t.schema_id) AS [SchemaName], 
    c.Name AS [ColumnName],
    dc.Name AS ConstraintName,
    dc.ConstraintType,
    dc.definition AS ConstraintDef
FROM sys.tables t
INNER JOIN constraints dc ON t.object_id = dc.parent_object_id
INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND c.column_id = dc.parent_column_id