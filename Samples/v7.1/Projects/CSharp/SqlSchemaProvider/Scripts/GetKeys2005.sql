SELECT  [fs].[name] AS [ForeignTableName],
        [fschemas].[name] AS [ForeignTableOwner],
        [rs].[name] AS [PrimaryTableName], 
        [rschemas].[name] AS [PrimaryTableOwner],
        [sfk].[name] AS [ConstraintName],
        [fc].[name] AS [ForeignColumnName],
        [rc].[name] AS [PrimaryColumnName],
        [sfk].[is_disabled] AS [Disabled],
        [sfk].[is_not_for_replication] AS [IsNotForReplication],
        [sfk].[update_referential_action] AS [UpdateReferentialAction],
        [sfk].[delete_referential_action] AS [DeleteReferentialAction],
        [sfk].[is_not_trusted] AS [WithNoCheck]
FROM    [sys].[foreign_keys] AS [sfk] WITH (NOLOCK)
	INNER JOIN [sys].[foreign_key_columns] AS [sfkc] WITH (NOLOCK) ON [sfk].[object_id] = [sfkc].[constraint_object_id]
	INNER JOIN [sys].[objects] [fs] WITH (NOLOCK) ON [sfk].[parent_object_id] = [fs].[object_id]
	INNER JOIN [sys].[objects] [rs] WITH (NOLOCK) ON [sfk].[referenced_object_id] = [rs].[object_id] 
	LEFT JOIN [sys].[schemas] [fschemas] WITH (NOLOCK) ON [fschemas].[schema_id] = [fs].[schema_id]
	LEFT JOIN [sys].[schemas] [rschemas] WITH (NOLOCK) ON [rschemas].[schema_id] = [rs].[schema_id]
	INNER JOIN [sys].[columns] [fc] WITH (NOLOCK) ON [sfkc].[parent_column_id] = [fc].[column_id] AND [fc].[object_id] = [sfk].[parent_object_id]
	INNER JOIN [sys].[columns] [rc] WITH (NOLOCK) ON [sfkc].[referenced_column_id] = [rc].[column_id] AND [rc].[object_id] = [sfk].[referenced_object_id]
WHERE [sfk].[is_ms_shipped] = 0 --Added to check for replication.
ORDER BY [sfk].[name],[sfkc].[constraint_column_id]
