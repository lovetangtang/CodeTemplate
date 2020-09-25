 SELECT
  object_name(id) AS OBJECT_NAME,
  schema_name(uid) AS USER_NAME,
  type AS TYPE,
  crdate AS DATE_CREATED,
  id as OBJECT_ID
FROM sysobjects
WHERE
  type = N'V'
  AND HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(uid)) + '.' + QUOTENAME(object_name(id)), 'OBJECT', 'SELECT') <> 0
  AND ObjectProperty(id, N'IsMSShipped') = 0
  AND NOT EXISTS (SELECT * FROM sys.extended_properties WHERE major_id = id AND name = 'microsoft_database_tools_support' AND value = 1)
ORDER BY object_name(id)