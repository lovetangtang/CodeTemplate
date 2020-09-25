SELECT 
	p.proc_name CommandName,
    USER_NAME(p.creator) SchemaName,
	c.parm_name ParameterName, 
	c.parm_id ParameterID,
	ISNULL((CASE d.domain_name WHEN 'numeric' THEN Length WHEN 'decimal' THEN Length ELSE d."precision" END), 0) "Precision", 
	d.domain_name TypeName,
	(CASE LOCATE(c.base_type_str, '(') WHEN 0 THEN c.base_type_str ELSE LEFT(c.base_type_str, LOCATE(c.base_type_str, '(') - 1) END) BaseTypeName,
	c.width Length, 
	c.scale Scale, 
	Cast ((CASE c.parm_mode_out WHEN 'N' THEN 0 ELSE 1 END) as bit) IsOutput, 
	"default" DefaultValue
 FROM sys.SYSPROCEDURE p 
	JOIN sys.SYSPROCPARM c ON p.proc_id = c.proc_id 
	JOIN sys.SYSDOMAIN d ON c.domain_id = d.domain_id
 WHERE
	(c.parm_type = 0 or c.parm_type = 4)
 ORDER BY CommandName, ParameterID
