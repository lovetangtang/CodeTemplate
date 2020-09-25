SELECT
	f.table_name ForeignTableName,     
	USER_NAME(f.Creator) ForeignTableOwner, 
	p.table_name PrimaryTableName, 
	USER_NAME(p.Creator) PrimaryTableOwner, 
	p.table_name + f.table_name ConstraintName,
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
			THEN 1 ELSE 0 END) AS integer) DeleteReferentialAction,	CAST(1 AS integer) AS WithNoCheck
FROM sys.SYSTAB f 
	JOIN sys.SYSIDX x ON f.table_id = x.table_id 
	JOIN sys.SYSIDXCOL c ON c.table_id = x.table_id AND c.index_id = x.index_id
	JOIN sys.SYSTABCOL fc ON f.table_id = fc.table_id AND fc.column_id = c.column_id
	JOIN sys.SYSFKEY y ON y.foreign_table_id = f.table_id AND y.foreign_index_id = x.index_id
	JOIN sys.SYSTAB p ON y.primary_table_id = p.table_id
	JOIN sys.SYSTABCOL pc ON p.table_id = pc.table_id AND pc.column_id = c.primary_column_id
WHERE 
	x.index_Category = 2
	AND ( (f.table_name = ? AND f.Creator = user_id( ? ))
		OR (p.table_name = ? AND p.Creator =  user_id( ? )) )
