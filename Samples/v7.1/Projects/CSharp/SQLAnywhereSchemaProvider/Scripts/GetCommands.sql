SELECT 
	p.proc_name OBJECT_NAME, 
	USER_NAME(p.creator) USER_NAME, 
	o.creation_time DATE_CREATED, 
	p.object_id OBJECT_ID,
	(CASE UPPER(SUBSTR(p.source, 8, 4)) WHEN 'FUNC' THEN 'FN' ELSE 'P' END) COMMAND_TYPE 
FROM sys.SYSPROCEDURE p 
	JOIN sys.SYSOBJECT o ON p.object_id = o.object_id 
WHERE 
	p.source IS NOT NULL 
	AND p.Creator in (USER_ID('dbo'), user_id()) 
	ORDER BY OBJECT_NAME