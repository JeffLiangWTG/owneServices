namespace Enterprise.MasterFiles.Business
{
	public static class UserRepositoryScripts
	{
		internal const string ObjectListSql = @"
			IF EXISTS(SELECT null FROM master.sys.databases WHERE name = '{0}')
				SELECT
					o.object_id AS ObjectId,
					s.name AS ObjectSchema,
					o.name AS ObjectName,
					CASE o.type
						WHEN 'FN' THEN 'SCALAR_FUNCTION'
						WHEN 'IF' THEN 'TABLE_FUNCTION'
						WHEN 'TF' THEN 'TABLE_FUNCTION'
						WHEN 'V'  THEN 'VIEW'
						WHEN 'P'  THEN 'PROCEDURE'
						WHEN 'U'  THEN 'USER_TABLE'
						ELSE o.type_desc
					END AS ObjectType
				FROM
					[{0}].sys.objects o
					INNER JOIN [{0}].sys.schemas s ON s.schema_id = o.schema_id
				WHERE
					o.type in ({1})
					AND o.is_ms_shipped = 0
				UNION
				SELECT
					t.user_type_id AS ObjectId,
					s.name AS ObjectSchema,
					t.name AS ObjectName,
					'USER_TYPE' AS ObjectType
				FROM
					[{0}].sys.types t
					INNER JOIN [{0}].sys.schemas s ON s.schema_id = t.schema_id
				WHERE
					is_user_defined = 1
				ORDER BY ObjectName;
			ELSE
				SELECT TOP 0
					convert(int, null) AS ObjectId,
					convert(sysname, null) AS ObjectSchema,
					convert(sysname, null) AS ObjectName,
					convert(nvarchar(60), null) AS ObjectType;";

		internal const string OtherDefinitionSql = "SELECT definition FROM [{0}].sys.sql_modules WHERE object_id = {1}";

		internal const string TableDefinitionSql = @"
			DECLARE @table_name SYSNAME
			SELECT @table_name = '{2}.{1}'

			DECLARE 
			      @object_name SYSNAME
			    , @object_id INT

			SELECT 
			      @object_name = '[' + s.name + '].[' + o.name + ']'
			    , @object_id = o.[object_id]
			FROM {0}.sys.objects o WITH (NOWAIT)
			JOIN {0}.sys.schemas s WITH (NOWAIT) ON o.[schema_id] = s.[schema_id]
			WHERE s.name + '.' + o.name = @table_name
			    AND o.[type] = 'U'
			    AND o.is_ms_shipped = 0

			DECLARE @SQL NVARCHAR(MAX) = ''

			;WITH index_column AS 
			(
			    SELECT 
			          ic.[object_id]
			        , ic.index_id
			        , ic.is_descending_key
			        , ic.is_included_column
			        , c.name
			    FROM {0}.sys.index_columns ic WITH (NOWAIT)
			    JOIN {0}.sys.columns c WITH (NOWAIT) ON ic.[object_id] = c.[object_id] AND ic.column_id = c.column_id
			    WHERE ic.[object_id] = @object_id
			),
			fk_columns AS 
			(
			     SELECT 
			          k.constraint_object_id
			        , cname = c.name
			        , rcname = rc.name
			    FROM {0}.sys.foreign_key_columns k WITH (NOWAIT)
			    JOIN {0}.sys.columns rc WITH (NOWAIT) ON rc.[object_id] = k.referenced_object_id AND rc.column_id = k.referenced_column_id 
			    JOIN {0}.sys.columns c WITH (NOWAIT) ON c.[object_id] = k.parent_object_id AND c.column_id = k.parent_column_id
			    WHERE k.parent_object_id = @object_id
			)
			SELECT @SQL = 'CREATE TABLE ' + @object_name + '[NewLine]' + '(' + '[NewLine]' + (
			    SELECT CHAR(9) + '  ' + STRING_AGG(CAST('[' + c.name + '] ' + 
			        CASE WHEN c.is_computed = 1
			            THEN 'AS ' + cc.[definition] 
			            ELSE UPPER(tp.name) + 
			                CASE WHEN tp.name IN ('varchar', 'char', 'varbinary', 'binary', 'text')
			                       THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length AS VARCHAR(5)) END + ')'
			                     WHEN tp.name IN ('nvarchar', 'nchar', 'ntext')
			                       THEN '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CAST(c.max_length / 2 AS VARCHAR(5)) END + ')'
			                     WHEN tp.name IN ('datetime2', 'time2', 'datetimeoffset') 
			                       THEN '(' + CAST(c.scale AS VARCHAR(5)) + ')'
			                     WHEN tp.name = 'decimal' 
			                       THEN '(' + CAST(c.[precision] AS VARCHAR(5)) + ',' + CAST(c.scale AS VARCHAR(5)) + ')'
			                    ELSE ''
			                END +
			                CASE WHEN c.collation_name IS NOT NULL THEN ' COLLATE ' + c.collation_name ELSE '' END +
			                CASE WHEN c.is_nullable = 1 THEN ' NULL' ELSE ' NOT NULL' END +
			                CASE WHEN dc.[definition] IS NOT NULL THEN ' DEFAULT' + dc.[definition] ELSE '' END + 
			                CASE WHEN ic.is_identity = 1 THEN ' IDENTITY(' + CAST(ISNULL(ic.seed_value, '0') AS CHAR(1)) + ',' + CAST(ISNULL(ic.increment_value, '1') AS CHAR(1)) + ')' ELSE '' END 
			        END + '[NewLine]' AS nvarchar(max)), CHAR(9) + ', ')
					WITHIN GROUP (ORDER BY c.column_id)
			    FROM {0}.sys.columns c WITH (NOWAIT)
			    JOIN {0}.sys.types tp WITH (NOWAIT) ON c.user_type_id = tp.user_type_id
			    LEFT JOIN {0}.sys.computed_columns cc WITH (NOWAIT) ON c.[object_id] = cc.[object_id] AND c.column_id = cc.column_id
			    LEFT JOIN {0}.sys.default_constraints dc WITH (NOWAIT) ON c.default_object_id != 0 AND c.[object_id] = dc.parent_object_id AND c.column_id = dc.parent_column_id
			    LEFT JOIN {0}.sys.identity_columns ic WITH (NOWAIT) ON c.is_identity = 1 AND c.[object_id] = ic.[object_id] AND c.column_id = ic.column_id
			    WHERE c.[object_id] = @object_id)
			    + ISNULL((SELECT CHAR(9) + ', CONSTRAINT [' + k.name + '] PRIMARY KEY (' + 
			                    (SELECT (
			                         SELECT STRING_AGG(CAST('[' + c.name + '] ' + CASE WHEN ic.is_descending_key = 1 THEN 'DESC' ELSE 'ASC' END AS nvarchar(max)), ', ')
			                         FROM {0}.sys.index_columns ic WITH (NOWAIT)
			                         JOIN {0}.sys.columns c WITH (NOWAIT) ON c.[object_id] = ic.[object_id] AND c.column_id = ic.column_id
			                         WHERE ic.is_included_column = 0
			                             AND ic.[object_id] = k.parent_object_id 
			                             AND ic.index_id = k.unique_index_id     
			                         ))
			            + ')' + '[NewLine]'
			            FROM {0}.sys.key_constraints k WITH (NOWAIT)
			            WHERE k.parent_object_id = @object_id 
			                AND k.[type] = 'PK'), '') + ')'  + '[NewLine]'
			    + ISNULL((SELECT (
			        SELECT '[NewLine]' +
			             'ALTER TABLE ' + @object_name + ' WITH' 
			            + CASE WHEN fk.is_not_trusted = 1 
			                THEN ' NOCHECK' 
			                ELSE ' CHECK' 
			              END + 
			              ' ADD CONSTRAINT [' + fk.name  + '] FOREIGN KEY(' 
			              + (
			                SELECT STRING_AGG(CAST('[' + k.cname + ']' AS nvarchar(max)), ', ')
			                FROM fk_columns k
			                WHERE k.constraint_object_id = fk.[object_id]
			                )
			               + ')' +
			              ' REFERENCES [' + SCHEMA_NAME(ro.[schema_id]) + '].[' + ro.name + '] ('
			              + (
			                SELECT STRING_AGG(CAST('[' + k.rcname + ']' AS nvarchar(max)), ', ')
			                FROM fk_columns k
			                WHERE k.constraint_object_id = fk.[object_id]
			                )
			               + ')'
			            + CASE 
			                WHEN fk.delete_referential_action = 1 THEN ' ON DELETE CASCADE' 
			                WHEN fk.delete_referential_action = 2 THEN ' ON DELETE SET NULL'
			                WHEN fk.delete_referential_action = 3 THEN ' ON DELETE SET DEFAULT' 
			                ELSE '' 
			              END
			            + CASE 
			                WHEN fk.update_referential_action = 1 THEN ' ON UPDATE CASCADE'
			                WHEN fk.update_referential_action = 2 THEN ' ON UPDATE SET NULL'
			                WHEN fk.update_referential_action = 3 THEN ' ON UPDATE SET DEFAULT'  
			                ELSE '' 
			              END 
			            + '[NewLine]' + 'ALTER TABLE ' + @object_name + ' CHECK CONSTRAINT [' + fk.name  + ']' + '[NewLine]'
			        FROM {0}.sys.foreign_keys fk WITH (NOWAIT)
			        JOIN {0}.sys.objects ro WITH (NOWAIT) ON ro.[object_id] = fk.referenced_object_id
			        WHERE fk.parent_object_id = @object_id
			        FOR XML PATH(N''), TYPE).value('.', 'NVARCHAR(MAX)')), '')
			    + ISNULL(((SELECT
			         '[NewLine]' + 'CREATE' + CASE WHEN i.is_unique = 1 THEN ' UNIQUE' ELSE '' END 
			                + ' NONCLUSTERED INDEX [' + i.name + '] ON ' + @object_name + ' (' +
			                (
			                SELECT STRING_AGG(CAST('[' + c.name + ']' + CASE WHEN c.is_descending_key = 1 THEN ' DESC' ELSE ' ASC' END AS nvarchar(max)), ', ')
			                FROM index_column c
			                WHERE c.is_included_column = 0
			                    AND c.index_id = i.index_id
			                ) + ')'  
			                + ISNULL('[NewLine]' + 'INCLUDE (' + 
			                    (
			                    SELECT STRING_AGG (CAST('[' + c.name + ']' AS nvarchar(max)), ', ')
			                    FROM index_column c
			                    WHERE c.is_included_column = 1
			                        AND c.index_id = i.index_id
			                    ) + ')', '')  + '[NewLine]'
			        FROM {0}.sys.indexes i WITH (NOWAIT)
			        WHERE i.[object_id] = @object_id
			            AND i.is_primary_key = 0
			            AND i.[type] = 2
			        FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)')
			    ), '')

			SELECT @SQL";
	}
}
