CREATE FUNCTION CreateCascadeDeleteTrigger(@refTblName varchar(50))
RETURNS varchar(max)
AS
BEGIN
	DECLARE fk_cursor CURSOR FOR
	SELECT tbl.name, col.name, refCol.name
	FROM sys.foreign_keys fk
	JOIN sys.foreign_key_columns fkCol ON fkCol.constraint_object_id = fk.object_id
	JOIN sys.tables tbl ON fkCol.parent_object_id = tbl.object_id 
	JOIN sys.tables refTbl ON fkCol.referenced_object_id = refTbl.object_id
	JOIN sys.all_columns col ON col.object_id = tbl.object_id and col.column_id = fkCol.parent_column_id
	JOIN sys.all_columns refCol ON refCol.object_id = fkCol.referenced_object_id and refCol.column_id = fkCol.referenced_column_id
	WHERE refTbl.name = @refTblName
	ORDER BY tbl.name

	DECLARE @tblName varchar(50), @colName varchar(50), @refColName varchar(50), @result varchar(max) = '';
	DECLARE @newline varchar(5) = CHAR(13) + CHAR(10);



	OPEN fk_cursor
	FETCH NEXT FROM fk_cursor
	INTO @tblName, @colName, @refColName

	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF @result = ''
		BEGIN
			SET @result = 'CREATE TRIGGER ' + @refTblName + '_Delete' + @newline +
'ON ' + @refTblName + @newline +
'INSTEAD OF DELETE' + @newline +
'AS' + @newline
		END
		SET @result = @result + @newline +
'DELETE en' + @newline +
'FROM deleted' + @newline +
'JOIN ' + @tblName + ' en ON ' + @refColName + ' = ' + @colName + @newline
		FETCH NEXT FROM fk_cursor
		INTO @tblName, @colName, @refColName
	END
	IF @result != ''
	BEGIN
		DECLARE @pk VARCHAR(50);
		SELECT @pk = COLUMN_NAME
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
AND TABLE_NAME = @refTblName AND TABLE_SCHEMA = 'dbo'

		SET @result = @result + @newline +
'DELETE en' + @newline +
'FROM ' + @refTblName + ' en' + @newline +
'JOIN deleted ON en.' + @pk + ' = deleted.' + @pk
	END
	CLOSE fk_cursor;
	DEALLOCATE fk_cursor;
	RETURN @result
END
