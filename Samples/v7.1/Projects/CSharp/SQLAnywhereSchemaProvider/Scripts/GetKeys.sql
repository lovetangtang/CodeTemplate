SELECT
	t.table_name ForeignTableName,     
	USER_NAME(t.Creator) ForeignTableOwner, 
	p.table_name PrimaryTableName, 
	USER_NAME(p.Creator) PrimaryTableOwner, 
	p.table_name + t.table_name ConstraintName,
	fc.column_name ForeignColumnName,
	pc.column_name PrimaryColumnName,
	CAST(0 AS integer) Disabled,
	CAST(0 AS integer) IsNotForReplication,
	CAST((CASE WHEN EXISTS(SELECT * FROM SYS.SYSTRIGGER 
			WHERE table_id = y.primary_table_id AND foreign_table_id = y.foreign_table_id 
				AND foreign_key_id = y.foreign_index_id AND referential_action = 'C' AND "event" = 'C') 
			THEN 1 ELSE 0 END) AS integer) UpdateReferentialAction,
	CAST((CASE WHEN EXISTS(SELECT * FROM SYS.SYSTRIGGER 
			WHERE table_id = y.primary_table_id AND foreign_table_id = y.foreign_table_id 
				AND foreign_key_id = y.foreign_index_id AND referential_action = 'C' AND "event" = 'D') 
			THEN 1 ELSE 0 END) AS integer) DeleteReferentialAction,
	CAST(1 AS integer) AS WithNoCheck
FROM SYS.SYSTAB t 
	JOIN SYS.SYSIDX x ON t.table_id = x.table_id 
	JOIN SYS.SYSIDXCOL c ON c.table_id = x.table_id AND c.index_id = x.index_id
	JOIN SYS.SYSTABCOL fc ON t.table_id = fc.table_id AND fc.column_id = c.column_id
	JOIN SYS.SYSFKEY y ON y.foreign_table_id = t.table_id AND y.foreign_index_id = x.index_id
	JOIN SYS.SYSTAB p ON y.primary_table_id = p.table_id
	JOIN SYS.SYSTABCOL pc ON p.table_id = pc.table_id AND pc.column_id = c.primary_column_id
WHERE 
	t.table_type = 1 
	AND t.Creator in (USER_ID('dba'), user_id()) 
	AND x.index_Category = 2
ORDER BY ConstraintName, fc.column_id
