SELECT [sp].[id] AS [id], 
	[so].[name] AS [ObjectName], 
	[su].[name] AS [ObjectOwner],  
	[so].[type] AS [ObjectType], 
    CAST([sp].[smallid] AS INT) AS [Minor],
	[sp].[type] AS [type], 
	[sp].[name] AS [PropertyName], 
	[sp].[value] AS [PropertyValue],
    SQL_VARIANT_PROPERTY([sp].[value],'BaseType') AS [PropertyBaseType],
    CASE [sp].[type] WHEN 2 THEN USER_NAME([sp].[smallid]) END AS [UserName],
    CASE [sp].[type] WHEN 1 THEN (SELECT TOP 1 [name] FROM [dbo].[systypes] WHERE [xusertype] = [sp].[smallid]) END AS [UDTName],
    CASE [sp].[type] WHEN 1 THEN (SELECT TOP 1 [sysusers].[name] FROM [dbo].[sysusers] INNER JOIN [systypes] ON [systypes].[uid] = [sysusers].[uid] WHERE [xusertype] = [sp].[smallid]) END AS [UDTOwner],
    [sc].[name] AS [FieldName],
    [si].[name] AS [IndexName],
    [sop].[name] AS [ParentName],
    [sup].[name] AS [ParentOwner],
    [sop].[type] AS [ParentType]
FROM  [dbo].[sysproperties] [sp] WITH (NOLOCK)
    LEFT JOIN [dbo].[sysobjects] [so] WITH (NOLOCK) ON [so].[id] = [sp].[id]
    LEFT JOIN [dbo].[sysusers] [su] WITH (NOLOCK) ON [su].[uid] = [so].[uid]
    LEFT JOIN [dbo].[syscolumns] [sc] WITH (NOLOCK) ON [sc].[id] = [sp].[id] AND [sc].[colid] = [sp].[smallid]
    LEFT JOIN [dbo].[sysindexes] [si] WITH (NOLOCK) ON [si].[id] = [sp].[id] AND [si].[indid] = [sp].[smallid]
    LEFT JOIN [dbo].[sysobjects] [sop] WITH (NOLOCK) ON [so].[parent_obj] = [sop].[id]
    LEFT JOIN [dbo].[sysusers] [sup] WITH (NOLOCK) ON [sop].[uid] = [sup].[uid]
        -- eliminate the combination: (column and type 5 (Parameter)
WHERE    NOT    (NOT    (    (    [sc].[number] = 1
                    OR (    [sc].[number] = 0
                        AND OBJECTPROPERTY([sc].[id], N'IsScalarFunction') = 1
                        and ISNULL([sc].[name], '') != ''
                        )
                    )
                AND (    [sc].[id] =[so].[id])
                )
            AND [sp].[type] = 5
            )
      -- eliminate the combination: (param and type 4 (column)
      AND	NOT	(	(	(	[sc].[number] = 1 
					OR	(	[sc].[number] = 0 
						and OBJECTPROPERTY([sc].[id], N'IsScalarFunction')= 1 
						and ISNULL([sc].[name], '') != ''
						)
					) 
				AND	(	[sc].[id]=[so].[id])
				) 
			and		[sp].[type] = 4
		)
ORDER   BY [sp].[id], [sp].[smallid], [sp].[type], [sp].[name]
