SELECT
  TB.[OBJECT_NAME],
  TB.[USER_NAME],
  TB.[TYPE],
  TB.[DATE_CREATED],
  'PRIMARY' AS [FILE_GROUP],
  TB.[OBJECT_ID]
FROM
  (
    SELECT
      T.name AS [OBJECT_NAME],
      SCHEMA_NAME(T.schema_id) AS [USER_NAME],
      T.schema_id AS [SCHEMA_ID],
      T.type AS [TYPE],
      T.create_date AS [DATE_CREATED],
      T.object_id AS [OBJECT_ID],
      HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(T.schema_id)) + '.' + QUOTENAME(T.name), 'OBJECT', 'SELECT') AS [HAVE_SELECT]
    FROM
      sys.tables T
    WHERE
      T.type = 'U'
  ) TB
WHERE
  TB.HAVE_SELECT = 1
  AND ObjectProperty(TB.[OBJECT_ID], N'IsMSShipped') = 0
ORDER BY
  TB.USER_NAME,
  TB.OBJECT_NAME