/*
 Return SQL to create an index on one table that matches the clustered index on another table and partition.
 Both tablenames should be in the given schema.
 Used for swapping partitions where we need to drop and create a clustered index
 in order to move a table to another filegroup.
*/
CREATE FUNCTION edi.GetCreateClusteredIndexSql (@schemaName sysname, @tableNameWithIndex sysname, @partitionNumber int, @tableNameToCreateIndex sysname)
RETURNS nvarchar(max)
AS
BEGIN
	DECLARE @sql nvarchar(max)

	declare @tableId int = (select t.object_id from sys.tables t join sys.schemas s on s.schema_id = t.schema_id where t.name = @tableNameWithIndex and s.name = @schemaName);
	declare @indexId int = (select ix.index_id from sys.indexes ix where ix.object_id = @tableId and type_desc = 'CLUSTERED');

	SET @sql =
	(
		SELECT 
			N'CREATE '
			+ (CASE WHEN ix.is_unique = 1 THEN N'UNIQUE ' ELSE N'' END)
			+ ix.type_desc + ' '
			+ N' INDEX '
			+ N'[' + ix.name + N'] ON '
			+ N'[' + @schemaName + N'].[' + @tableNameToCreateIndex + N'] ('
			+ col_names.column_names
			+ N') WITH ('
			+ N'IGNORE_DUP_KEY = ' + CASE WHEN ix.ignore_dup_key = 1 THEN N'ON' ELSE N'OFF' END
			+ N', DATA_COMPRESSION = ' + par.data_compression_desc
			+ N')'
			COLLATE database_default
		FROM sys.indexes ix 
		JOIN sys.tables t ON t.object_id = ix.object_id
		JOIN sys.partitions par ON par.object_id = ix.object_id AND par.index_id = ix.index_id
		CROSS APPLY (
			SELECT STRING_AGG(N'' + col.name, N',') WITHIN GROUP (ORDER BY ixc.key_ordinal) AS column_names
			FROM sys.index_columns ixc
			JOIN sys.columns col ON ixc.object_id = col.object_id AND ixc.column_id = col.column_id
			WHERE ixc.object_id = @tableId AND ixc.index_id = @indexId
		) col_names
		WHERE t.object_id = @tableId AND ix.index_id = @indexId AND par.partition_number = @partitionNumber
	);

	RETURN @sql
END
