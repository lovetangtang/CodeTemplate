﻿SELECT
  object_name(id) AS OBJECT_NAME,
  user_name(uid) AS USER_NAME,
  crdate AS DATE_CREATED,
  id as OBJECT_ID,
  type as COMMAND_TYPE
FROM
  sysobjects
WHERE
  type IN (
		N'P', -- SQL Stored Procedure
		N'PC', --Assembly (CLR) stored-procedure
		N'FN', --SQL scalar function
		N'FS', --Assembly (CLR) scalar-function
		N'IF', --SQL inline table-valued function
		N'TF' --SQL table-valued-function
	  )
  --AND permissions(id) & 32 <> 0 
  AND ObjectProperty(id, N'IsMSShipped') = 0
ORDER BY object_name(id)