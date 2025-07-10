namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public static class SQLHelper
	{
		public static string GetDropUnnamedConstraint(string tableName, string columnName)
		{
			return $@"
DECLARE @tableName NVARCHAR(MAX)
DECLARE @columnName NVARCHAR(MAX)
DECLARE @command NVARCHAR(MAX)

SET @tableName = N'{tableName}'
SET @columnName = N'{columnName}'

SELECT @command = 'ALTER TABLE ' +  @tableName  + ' DROP CONSTRAINT ' + d.name
FROM 
	sys.tables t
	JOIN sys.default_constraints d ON d.parent_object_id = t.object_id
	JOIN sys.columns c ON c.object_id = t.object_id and c.column_id = d.parent_column_id
WHERE t.name = @tableName AND c.name = @columnName
IF @command IS NOT NULL
BEGIN
	EXECUTE( @command)
END
";
		}
	}
}
