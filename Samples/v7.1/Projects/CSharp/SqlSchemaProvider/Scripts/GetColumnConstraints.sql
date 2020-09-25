SELECT
  [tbl].[name] AS [TableName],
  [stbl].[name] AS [SchemaName], 
  [clmns].[name] AS [ColumnName],
  OBJECT_NAME([const].[constid]) AS ConstraintName,
  CASE
    WHEN [const].[status] & 5 = 5 THEN 'DEFAULT'
    WHEN [const].[status] & 4 = 4 THEN 'CHECK'
    ELSE ''
  END AS ConstraintType,
  [constdef].[text] AS ConstraintDef
FROM
  dbo.sysobjects AS tbl WITH (NOLOCK)
  INNER JOIN dbo.sysusers AS stbl WITH (NOLOCK) ON [stbl].[uid] = [tbl].[uid]
  INNER JOIN dbo.syscolumns AS clmns WITH (NOLOCK) ON [clmns].[id] = [tbl].[id]
  INNER JOIN dbo.sysconstraints const WITH (NOLOCK) ON [clmns].[id] = [const].[id] and [clmns].[colid] = [const].[colid]
  LEFT OUTER JOIN dbo.syscomments constdef WITH (NOLOCK) ON [const].[constid] = [constdef].[id]
WHERE ([const].[status] & 4 = 4 OR [const].[status] & 5 = 5)