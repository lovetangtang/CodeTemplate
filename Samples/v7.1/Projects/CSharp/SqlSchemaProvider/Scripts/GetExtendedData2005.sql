SELECT  [sp].[major_id] AS [ID], 
        [so].[name] AS [ObjectName], 
        SCHEMA_NAME([so].[schema_id]) AS [ObjectOwner],  
        [so].[type] AS [ObjectType], 
        [sp].[minor_id] AS [Minor],  
        [sp].[name] AS [PropertyName], 
        [sp].[value] AS [PropertyValue],
        SQL_VARIANT_PROPERTY([sp].[value],'BaseType') AS [PropertyBaseType],			    
		CASE [sp].[class] WHEN 4 THEN USER_NAME([sp].[major_id]) END AS [UserName],
        CASE [sp].[class]
	        WHEN 2 THEN [spar].[name]
	        ELSE [sc].[name]
        END AS [FieldName],
        [si].[name] AS [IndexName],
        [sop].[name] AS [ParentName],
        SCHEMA_NAME([sop].[schema_id]) AS [ParentOwner],
        [sop].[type] AS [ParentType],
        [sp].[class] AS [Type]        
FROM [sys].[extended_properties] AS [sp] WITH (NOLOCK) 
	LEFT JOIN [sys].[objects] AS [so] WITH (NOLOCK) ON [so].[object_id] = [sp].[major_id]
	LEFT JOIN [sys].[columns] AS [sc] WITH (NOLOCK) ON [sc].[object_id] = [sp].[major_id] AND [sc].[column_id] = [sp].[minor_id]
	LEFT JOIN [sys].[parameters] AS [spar] WITH (NOLOCK) ON [spar].[object_id] = [sp].[major_id] AND [spar].[parameter_id] = [sp].[minor_id]
	LEFT JOIN [sysindexes] [si] WITH (NOLOCK) ON [si].[id] = [sp].[major_id] AND [si].[indid] = [sp].[minor_id]
	LEFT JOIN [sys].[objects] [sop] WITH (NOLOCK) ON [so].[parent_object_id] = [sop].[object_id]
