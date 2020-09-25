SELECT  
	t.object_id ID, 
	t.table_name ObjectName, 
	USER_NAME(t.creator) ObjectOwner,  
	(CASE t.table_type WHEN 1 THEN 'U' ELSE 'V' END) ObjectType, 
	CAST(0 as integer) Minor,  
	'MS_Description' PropertyName, 
	r.remarks PropertyValue,
	'varchar' PropertyBaseType,			    
	NULL UserName,
	NULL FieldName,
	NULL IndexName,
	NULL ParentName,
	NULL  ParentOwner,
	NULL  ParentType,
	CAST(1 AS integer) Type        
FROM SYS.SYSREMARK r 
	JOIN SYS.SYSTAB t ON t.object_id = r.object_id
WHERE
	t.table_type IN (1, 21) 
	AND t.Creator in (USER_ID('dba'), user_id()) 
UNION
SELECT  
	c.object_id ID, 
	t.table_name ObjectName, 
	USER_NAME(t.creator) ObjectOwner,  
	(CASE t.table_type WHEN 1 THEN 'U' ELSE 'V' END) ObjectType, 
	c.column_id Minor,  
	'MS_Description' PropertyName, 
	r.remarks PropertyValue,
	'varchar' PropertyBaseType,			    
	NULL UserName,
	c.column_name FieldName,
	NULL IndexName,
	NULL ParentName,
	NULL  ParentOwner,
	NULL  ParentType,
	CAST(1 AS integer) Type        
FROM SYS.SYSREMARK r 
	JOIN SYS.SYSTABCOL c ON c.object_id = r.object_id
	JOIN SYS.SYSTAB t ON t.table_id = c.table_id
WHERE
	t.table_type IN (1, 21) 
	AND t.Creator in (USER_ID('dba'), user_id())	
UNION
SELECT  
	i.object_id ID, 
	(CASE WHEN (i.index_category = 1) THEN 'PK' + i.index_name 
		ELSE i.index_name END) ObjectName,
	USER_NAME(t.creator) ObjectOwner,  
	(CASE WHEN (i.index_category = 1) THEN 'PK' ELSE 'IN' END) ObjectType, 
	CAST (0 AS integer) Minor,  
	'MS_Description' PropertyName, 
	r.remarks PropertyValue,
	'varchar' PropertyBaseType,			    
	NULL UserName,
	NULL FieldName,
	NULL IndexName,
	NULL ParentName,
	NULL  ParentOwner,
	NULL  ParentType,
	CAST(7 AS integer) Type        
FROM SYS.SYSREMARK r 
	JOIN SYS.SYSIDX i ON i.object_id = r.object_id
	JOIN SYS.SYSTAB t ON t.table_id = i.table_id
WHERE
	t.table_type = 1
	AND t.Creator in (USER_ID('dba'), user_id())	
    AND i.index_category != 2
UNION
SELECT  
	x.object_id ID, 
	p.table_name + t.table_name ObjectName,
	USER_NAME(t.creator) ObjectOwner,  
	'FK' ObjectType, 
	CAST (0 AS integer) Minor,  
	'MS_Description' PropertyName, 
	r.remarks PropertyValue,
	'varchar' PropertyBaseType,			    
	NULL UserName,
	NULL FieldName,
	NULL IndexName,
	NULL ParentName,
	NULL  ParentOwner,
	NULL  ParentType,
	CAST(7 AS integer) Type        
FROM SYS.SYSREMARK r 
	JOIN SYS.SYSIDX x ON x.object_id = r.object_id
	JOIN SYS.SYSTAB t ON t.table_id = x.table_id
	JOIN SYS.SYSIDXCOL c ON c.table_id = x.table_id AND c.index_id = x.index_id
	JOIN SYS.SYSTABCOL fc ON t.table_id = fc.table_id AND fc.column_id = c.column_id
	JOIN SYS.SYSFKEY y ON y.foreign_table_id = t.table_id AND y.foreign_index_id = x.index_id
	JOIN SYS.SYSTAB p ON y.primary_table_id = p.table_id
	JOIN SYS.SYSTABCOL pc ON p.table_id = pc.table_id AND pc.column_id = c.primary_column_id
WHERE
	t.table_type = 1
	AND t.Creator in (USER_ID('dba'), user_id())	
    AND x.index_category = 2
UNION
SELECT  
	t.object_id ID, 
	t.proc_name ObjectName, 
	USER_NAME(t.creator) ObjectOwner,  
	'P' ObjectType, 
	CAST(0 as integer) Minor,  
	'MS_Description' PropertyName, 
	r.remarks PropertyValue,
	'varchar' PropertyBaseType,			    
	NULL UserName,
	NULL FieldName,
	NULL IndexName,
	NULL ParentName,
	NULL  ParentOwner,
	NULL  ParentType,
	CAST(2 AS integer) Type        
FROM SYS.SYSREMARK r 
	JOIN SYS.SYSPROCEDURE t ON t.object_id = r.object_id
WHERE
	t.source IS NOT NULL 
	AND t.Creator in (USER_ID('dbo'), user_id()) 
ORDER BY ObjectName, ObjectType, Minor
