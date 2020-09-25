SELECT 
	c.parm_name Name, 
	ISNULL((CASE d.domain_name WHEN 'numeric' THEN Length WHEN 'decimal' THEN Length ELSE d."precision" END), 0) NumericPrecision, 
	c.scale NumericScale, 
	c.width Length, 
	d.domain_name TypeName,
	(CASE LOCATE(c.base_type_str, '(') WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, '(') - 1) END) BaseTypeName,
	'Y' AllowDBNull
FROM sys.SYSPROCEDURE p 
	JOIN sys.SYSPROCPARM c ON p.proc_id = c.proc_id 
	JOIN sys.SYSDOMAIN d ON c.domain_id = d.domain_id
WHERE 
	p.proc_name = ?
	AND p.creator = USER_ID(?) 
	AND c.parm_type = 1
ORDER BY c.parm_id
