using System.Data;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public class RenameToZZZNKDataGroupColumnTask : IDataTransformationTask
	{
		public RenameToZZZNKDataGroupColumnTask(int version)
		{
			Version = version;
		}

		public int Version { get; private set; }

		public void Run(IDbTransaction trans)
		{
			Argument.Argument.NotNull(trans, nameof(trans));
			var sql = @"
DECLARE @table_name NVARCHAR(MAX)
DECLARE @column_name NVARCHAR(MAX)
DECLARE @command NVARCHAR(MAX)
DECLARE schema_cursor CURSOR FOR 
SELECT c.name, t.name FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id WHERE c.name LIKE '%RN_CountryOrGrouping'

OPEN schema_cursor

FETCH NEXT FROM schema_cursor INTO @column_name, @table_name

WHILE @@FETCH_STATUS = 0
BEGIN
	IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE name = LEFT(@column_name, 3) + '_ZZZ_NKDataGrouping' AND object_id = OBJECT_ID('dbo.' + @table_name))
	BEGIN
		SET @command = 'ALTER TABLE ' + @table_name + ' ADD ' + LEFT(@column_name, 3) + '_ZZZ_NKDataGrouping NVARCHAR(3)'
		EXEC (@command)
		IF EXISTS (SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(@table_name + '_Version_Update'))
		BEGIN
			SET @command = 'DISABLE TRIGGER ' + @table_name + '_Version_Update ON ' + @table_name
			EXEC (@command)
		END
		SET @command = 'UPDATE ' + @table_name + ' SET ' + LEFT(@column_name, 3) + '_ZZZ_NKDataGrouping = ' + @column_name
		EXEC (@command)
		IF EXISTS (SELECT 1 FROM sys.triggers WHERE object_id = OBJECT_ID(@table_name + '_Version_Update'))
		BEGIN
			SET @command = 'ENABLE TRIGGER ' + @table_name + '_Version_Update ON ' + @table_name
			EXEC (@command)
		END
	END
	FETCH NEXT FROM schema_cursor INTO @column_name, @table_name
END

CLOSE schema_cursor;  
DEALLOCATE schema_cursor; 
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
