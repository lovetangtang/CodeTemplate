SELECT
  TB.[OBJECT_NAME],
  TB.[USER_NAME],
  TB.[TYPE],
  TB.[DATE_CREATED],
  TB.[FILE_GROUP],
  TB.[OBJECT_ID]
FROM
  (
    SELECT
      T.name AS [OBJECT_NAME],
      SCHEMA_NAME(T.schema_id) AS [USER_NAME],
      T.schema_id AS [SCHEMA_ID],
      T.type AS [TYPE],
      T.create_date AS [DATE_CREATED],
      FG.file_group AS [FILE_GROUP],
      T.object_id AS [OBJECT_ID],
      HAS_PERMS_BY_NAME (QUOTENAME(SCHEMA_NAME(T.schema_id)) + '.' + QUOTENAME(T.name), 'OBJECT', 'SELECT') AS [HAVE_SELECT]
    FROM
      sys.tables T LEFT JOIN (
        SELECT
          S.name AS file_group,
          I.object_id AS id
        FROM sys.filegroups S INNER JOIN sys.indexes I ON I.data_space_id = S.data_space_id
        WHERE I.type < 2
      ) AS FG ON T.object_id = FG.id
    WHERE
      T.type = 'U'
  ) TB
WHERE
  TB.HAVE_SELECT = 1
  AND ObjectProperty(TB.[OBJECT_ID], N'IsMSShipped') = 0
  AND NOT EXISTS (SELECT * FROM sys.extended_properties WHERE major_id = TB.[OBJECT_ID] AND name = 'microsoft_database_tools_support' AND value = 1)
ORDER BY
  TB.USER_NAME,
  TB.OBJECT_NAME