SELECT  [fs].[name] AS [ForeignTableName], 
        [fsysusers].[name] AS [ForeignTableOwner], 
        [rs].[name] AS [PrimaryTableName], 
        [rsysusers].[name] AS [PrimaryTableOwner], 
        [cs].[name] AS [ConstraintName], 
        [fc].[name] AS [ForeignColumnName],
        [rc].[name] AS [PrimaryColumnName],
        CONVERT(bit, OBJECTPROPERTY([constid], N'CnstIsDisabled')) AS [Disabled],
        CONVERT(bit, OBJECTPROPERTY([constid], N'CnstIsNotRepl')) AS [IsNotForReplication],
        CONVERT(tinyint, ISNULL(OBJECTPROPERTY([constid], N'CnstIsUpdateCascade'), 0)) AS [UpdateReferentialAction],
        CONVERT(tinyint, ISNULL(OBJECTPROPERTY([constid], N'CnstIsDeleteCascade'), 0)) AS [DeleteReferentialAction],
        CONVERT(bit, OBJECTPROPERTY([constid], N'CnstIsNotTrusted')) AS [WithNoCheck]
FROM [dbo].[sysforeignkeys] WITH (NOLOCK) 
	INNER JOIN [dbo].[sysobjects] [fs] WITH (NOLOCK) ON [sysforeignkeys].[fkeyid] = [fs].[id]
	INNER JOIN [dbo].[sysobjects] [rs] WITH (NOLOCK) ON [sysforeignkeys].[rkeyid] = [rs].[id]
	INNER JOIN [dbo].[sysobjects] [cs] WITH (NOLOCK) ON [sysforeignkeys].[constid] = [cs].[id]
	LEFT JOIN [dbo].[sysusers] [fsysusers] WITH (NOLOCK) ON [fsysusers].[uid] = [fs].[uid] 
	LEFT JOIN [dbo].[sysusers] [rsysusers] WITH (NOLOCK) ON [rsysusers].[uid] = [rs].[uid]
	INNER JOIN [dbo].[syscolumns] [fc] WITH (NOLOCK) ON [sysforeignkeys].[fkey] = [fc].[colid] AND [sysforeignkeys].[fkeyid] = [fc].[id]
	INNER JOIN [dbo].[syscolumns] [rc] WITH (NOLOCK) ON [sysforeignkeys].[rkey] = [rc].[colid] AND [sysforeignkeys].[rkeyid] = [rc].[id]
WHERE OBJECTPROPERTY(object_id([fs].[Name]), 'IsMSShipped') = 0 --Added to check for replication.
ORDER BY [cs].[name], [sysforeignkeys].[keyno]
