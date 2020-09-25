SELECT
  object_name(id)	AS [OBJECT_NAME],
  user_name(uid)	AS [USER_NAME],
  type				AS TYPE,
  crdate			AS DATE_CREATED,
  ''				AS FILE_GROUP,
  id				as [OBJECT_ID]
FROM
  sysobjects
WHERE
  type = N'U'
  AND permissions(id) & 4096 <> 0
  AND ObjectProperty(id, N'IsMSShipped') = 0
ORDER BY user_name(uid), object_name(id)