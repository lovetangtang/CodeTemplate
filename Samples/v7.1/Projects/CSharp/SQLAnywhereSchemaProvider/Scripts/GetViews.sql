SELECT 
	t.table_name OBJECT_NAME, 
	USER_NAME(t.Creator) USER_NAME, 
	'V' TYPE,
	o.creation_time DATE_CREATED, 
	t.object_id OBJECT_ID 
FROM sys.SYSTAB t 
	JOIN sys.SYSOBJECT o ON t.object_id = o.object_id 
WHERE 
	t.table_type = 21 
	AND t.Creator in (USER_ID('dba'), user_id()) 
	ORDER BY 1,2