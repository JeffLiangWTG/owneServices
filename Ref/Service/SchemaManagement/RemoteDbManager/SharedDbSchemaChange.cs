using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public static class SharedDbSchemaChange
	{
		public static string GetAddColumnIfNotExistsScript(string tableName, string columnName, string columnDeclaration)
		{
			string script = $@"
IF NOT EXISTS(
	SELECT * FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = '{tableName}' AND col.name = '{columnName}'
)
BEGIN
	ALTER TABLE {tableName} ADD {columnName} {columnDeclaration}
END";

			return script;
		}

		public static string GetAlterColumnIfExistsScript(string tableName, string columnName, string columnProperties)
		{
			string script = $@"
IF EXISTS(
	SELECT * FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = '{tableName}' AND col.name = '{columnName}'
)
BEGIN
	ALTER TABLE {tableName} ALTER COLUMN {columnName} {columnProperties}
END";

			return script;
		}

		public static string GetAddCheckConstraintIfNotExistsScript(string tableName, string constraintName, string constraintDefination)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.check_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = '{0}' AND ckc.name = '{1}'
)
BEGIN
	ALTER TABLE {0} ADD CONSTRAINT {1} CHECK ({2})
END",
				tableName, constraintName, constraintDefination);

			return script;
		}

		public static string GetAddUniqueConstraintIfNotExistsScript(string tableName, string columnName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS(
	SELECT NULL FROM sys.tables tab
	INNER JOIN sys.key_constraints ckc ON tab.object_id=ckc.parent_object_id
	WHERE tab.name = '{0}' AND ckc.type = 'UQ'
)
BEGIN
	ALTER TABLE {0} ADD UNIQUE NONCLUSTERED ( {1} ASC) ON [PRIMARY]
END", tableName, columnName);

			return script;
		}

		public static string GetAddPrimaryKeyIfNotExistsScript(string tableName, string primaryKeyName, string primaryKeyField, string indexType = "CLUSTERED")
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS(
	SELECT NULL FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS con WHERE con.CONSTRAINT_TYPE = 'PRIMARY KEY'
	AND con.TABLE_NAME = '{0}'
)
BEGIN
	ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY {2} ({3});
END",
				tableName, primaryKeyName, indexType, primaryKeyField);

			return script;
		}

		public static string GetDropPrimaryKeyIfExistsScript(string tableName, string primaryKeyName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(
	SELECT NULL FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS con WHERE con.CONSTRAINT_TYPE = 'PRIMARY KEY'
	AND con.TABLE_NAME = '{0}'
)
BEGIN
	ALTER TABLE {0} DROP CONSTRAINT {1};
END",
				tableName, primaryKeyName);

			return script;
		}

		public static string GetAddForeignKeyIfNotExistsScript(string tableName, string foreignKeyName, string foreignKeyField, string foreignKeyReferences)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.foreign_keys fk ON tab.object_id = fk.parent_object_id
	WHERE tab.name = '{0}' AND fk.name = '{1}'
)
BEGIN
	ALTER TABLE {0} WITH CHECK ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}
END",
				tableName, foreignKeyName, foreignKeyField, foreignKeyReferences);

			return script;
		}

		public static string GetDropForeignKeyIfExistsScript(string tableName, string foreignKeyName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.foreign_keys fk ON tab.object_id = fk.parent_object_id
	WHERE tab.name = '{0}' AND fk.name = '{1}'
)
BEGIN
	ALTER TABLE {0} DROP CONSTRAINT {1};
END",
				tableName, foreignKeyName);

			return script;
		}

		public static string GetAddDefaultConstranintIfNotExistsScript(string tableName, string constraintName, string constraintDefination, string columnname)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF NOT EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.default_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = '{0}' AND ckc.name = '{1}'
)
BEGIN
	ALTER TABLE {0} ADD CONSTRAINT {1} DEFAULT ({2}) FOR {3}
END",
				tableName, constraintName, constraintDefination, columnname);

			return script;
		}

		public static string GetCreateIndexIfNotExistsScript(string tableOrViewName, string indexName, string createScript)
		{
			string script = $@"
IF NOT EXISTS(
	SELECT null
	FROM sys.indexes ind
	WHERE ind.name = '{indexName}'
	AND (
		EXISTS
		(
			SELECT NULL
			FROM sys.tables tab
			WHERE tab.object_id = ind.object_id AND tab.name = '{tableOrViewName}'
		) OR
		EXISTS
		(
			SELECT NULL
			FROM sys.views vie
			WHERE vie.object_id = ind.object_id AND vie.name = '{tableOrViewName}'
		)
	)
)
BEGIN
	{createScript}
END";

			return script;
		}

		public static string GetDropIndexIfExistsScript(string tableOrViewName, string indexName)
		{
			string script = $@"
IF EXISTS(
	SELECT null
	FROM sys.indexes ind
	WHERE ind.name = '{indexName}'
	AND (
		EXISTS
		(
			SELECT NULL
			FROM sys.tables tab
			WHERE tab.object_id = ind.object_id AND tab.name = '{tableOrViewName}'
		) OR
		EXISTS
		(
			SELECT NULL
			FROM sys.views vie
			WHERE vie.object_id = ind.object_id AND vie.name = '{tableOrViewName}'
		)
	)
)
BEGIN
	DROP INDEX {tableOrViewName}.{indexName}
END";

			return script;
		}

		public static string GetDropTriggerIfExistsScript(string tableName, string triggerName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(SELECT null FROM sys.tables tab 
				INNER JOIN sys.triggers tg ON tab.object_id = tg.parent_id
				WHERE tab.name = '{0}' AND tg.name = '{1}')
BEGIN
	 DROP TRIGGER {1}
END",
				tableName, triggerName);

			return script;
		}

		public static string GetDropStoredProcedureIfExistsScript(string storeProcedureName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
if (OBJECT_ID('{0}', 'P') is NOT NULL)
begin
	DROP PROCEDURE {0}
end",
				storeProcedureName);

			return script;
		}

		public static string GetCreateTableIfNotExistsScript(string tableName, string createScript)
		{
			string script = $@"
IF NOT EXISTS(SELECT null FROM sys.tables tab WHERE tab.name = '{tableName}')
BEGIN
	{createScript}
END";

			return script;
		}

		public static string GetDropConstraintIfExistsScript(string constraintType, string tableName, string constraintName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.{0} ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = '{1}' AND ckc.name = '{2}'
)
BEGIN
	ALTER TABLE {1} DROP CONSTRAINT {2};
END;",
				constraintType, tableName, constraintName);
			return script;
		}

		public static string GetDropUniqueConstraintIfExistsScript(string tableName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @constraint_name NVARCHAR(MAX)
SELECT @constraint_name = ckc.name
	FROM sys.tables tab
	INNER JOIN sys.key_constraints ckc ON tab.object_id=ckc.parent_object_id
	WHERE tab.name = '{0}' AND ckc.type = 'UQ'
IF @constraint_name IS NOT NULL
BEGIN
	EXEC('ALTER TABLE {0} DROP CONSTRAINT ' + @constraint_name);
END;", tableName);

			return script;
		}

		public static string GetDropColumnIfExistsScript(string tableName, string columnName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(
	SELECT * FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = '{0}' AND col.name = '{1}'
)
BEGIN
	ALTER TABLE {0} DROP COLUMN {1};
END;",
				tableName, columnName);
			return script;
		}

		public static string GetDropConstraintIfExistsFromColumnNameScript(string tableName, string columnName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
DECLARE @dfname_{1} NVARCHAR(MAX),
        @dropsql_{1} NVARCHAR(MAX)
SELECT @dfname_{1}=name FROM sys.objects WHERE object_id = (SELECT sys.columns.default_object_id FROM sys.objects INNER JOIN sys.columns ON objects.object_id = sys.columns.object_id 
    WHERE sys.columns.name = '{1}' AND sys.objects.name = '{0}')
    IF LEN(@dfname_{1})>0
    SET @dropsql_{1}='ALTER TABLE {0} DROP CONSTRAINT '+ CAST(@dfname_{1} AS NVARCHAR(50))
    IF LEN(@dfname_{1})>0
    EXEC sp_executesql @dropsql_{1}", tableName, columnName);
			return script;
		}

		public static string GetRenameColumnIfExistsScript(string tableName, string columnOldName, string columnNewName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(
	SELECT * FROM sys.tables tab 
	INNER JOIN sys.columns col ON tab.object_id = col.object_id
	WHERE tab.name = '{0}' AND col.name = '{1}'
)
BEGIN
	EXEC sp_rename '{0}.{1}', '{2}', 'COLUMN'
END;", tableName, columnOldName, columnNewName);
			return script;
		}

		public static string GetDropCheckConstranintIfExistsScript(string tableName, string constraintName)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
IF EXISTS(
	SELECT NULL FROM sys.tables tab 
	INNER JOIN sys.check_constraints ckc ON tab.object_id = ckc.parent_object_id
	WHERE tab.name = '{0}' AND ckc.name = '{1}'
)
BEGIN
	ALTER TABLE {0} DROP CONSTRAINT {1}
END",
				tableName, constraintName);

			return script;
		}

		public static string GetDropSqlObjectIfExistsScript(string objectType, string objectName, string objectTypeKeyword)
		{
			string result = $@"
IF EXISTS(
	SELECT * FROM sys.objects o
	WHERE o.type = '{objectType}' AND o.name = '{objectName}'
)
BEGIN
DROP {objectTypeKeyword} {objectName}
END";
			return result;
		}

		public static string GetCreateSqlObjectIfNotExistsScript(string sql, string objectType, string objectName)
		{
			string result = $@"
IF NOT EXISTS(
	SELECT * FROM sys.objects o
	WHERE o.type = '{objectType}' AND o.name = '{objectName}'
)
EXEC dbo.sp_executesql @statement = N'{sql.Replace("'", "''")}'";
			return result;
		}

		public static string GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(string subfolder, string objectName)
		{
			return SqlScriptHelper.GetSqlScriptFromZippedFile(PathToZipFile, subfolder, objectName);
		}

		public static string GetTableViewSqlScript(string tableName, int version)
		{
			var objectName = FormattableString.Invariant($"{tableName}{TableViewVersionSuffix}{version}");
			return SqlScriptHelper.GetSqlScriptFromZippedFile(PathToZipFile, TableViewSubfolder, objectName);
		}

		public static string GetSetLockEscalationScript(string tableName, string lockEscalationDesc)
		{
			string script = string.Format(CultureInfo.InvariantCulture, @"
ALTER TABLE {0} SET (LOCK_ESCALATION = {1});", tableName, lockEscalationDesc);
			return script;
		}

		public static readonly string TableViewVersionSuffix = "TableView_V";
		public static readonly string TableViewSubfolder = "TableView";
		public static readonly string TableSubfolder = "Table";
		public static readonly string PathToZipFile = @"RemoteDbSqlFiles.zip";
	}

	public interface ITableScript
	{
		string TableName { get; }
		string CreateTableScript { get; }
		Dictionary<int, string> TableViewScriptDictionary { get; }
	}
}
