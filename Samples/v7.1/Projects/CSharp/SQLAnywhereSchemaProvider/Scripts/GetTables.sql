SELECT 
	t.table_name OBJECT_NAME, 
	USER_NAME(t.Creator) USER_NAME, 
	'U' TYPE,
	o.creation_time DATE_CREATED, 
	'' FILE_GROUP, 
	t.object_id OBJECT_ID 
FROM sys.SYSTAB t 
	JOIN sys.SYSOBJECT o ON t.object_id = o.object_id 
WHERE 
	t.table_type = 1 
	AND t.Creator in (USER_ID('dba'), user_id()) 
	ORDER BY 1,2
