using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.RemoteDbManager.Test
{
	public static class TestDBHelper
	{
		public static bool ColumnExists(SqlConnection connection, string tableName, string columnName)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*) FROM sys.tables tab 
INNER JOIN sys.columns col ON tab.object_id = col.object_id
WHERE tab.name = '{tableName}' AND col.name = '{columnName}'";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool CheckConstraintExists(SqlConnection connection, string tableName, string constraintName)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*) FROM sys.check_constraints ch 
INNER JOIN sys.tables tab  ON tab.object_id = ch.parent_object_id
WHERE tab.name = '{tableName}' AND ch.name = '{constraintName}'";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool CheckUniqueConstraintExists(SqlConnection connection, string tableName)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*) FROM sys.tables tab
INNER JOIN sys.key_constraints ckc ON tab.object_id=ckc.parent_object_id
WHERE tab.name = '{tableName}' AND ckc.type = 'UQ'";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool CheckDefaultConstraintExists(SqlConnection connection, string tableName, string constraintName)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*) FROM sys.default_constraints ch 
INNER JOIN sys.tables tab  ON tab.object_id = ch.parent_object_id
WHERE tab.name = '{tableName}' AND ch.name = '{constraintName}'";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool CheckConstraintAndDefinitionExists(SqlConnection connection, string tableName, string constraintName, string definition)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*) FROM sys.check_constraints ch 
INNER JOIN sys.tables tab  ON tab.object_id = ch.parent_object_id
WHERE tab.name = '{tableName}' AND ch.name = '{constraintName}' and ch.definition = '{definition}'";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		static Dictionary<string, string> GetConstraintDefinition(SqlConnection connection, string tableName)
		{
			var constraintDefinitions = new Dictionary<string, string>();

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT ch.name, ch.definition FROM sys.check_constraints ch 
INNER JOIN sys.tables tab  ON tab.object_id = ch.parent_object_id
WHERE tab.name = '{tableName}'";
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						constraintDefinitions.Add(reader.GetString(0), reader.GetString(1));
					}
				}
			}

			return constraintDefinitions;
		}

		public static void AssertCheckConstraintsAndDefinitions(SqlConnection connection, string tableName,
			IEnumerable<(string Name, string Definition)> expectedConstraintsWithDefinitions)
		{
			var constraintDefinations = GetConstraintDefinition(connection, tableName);
			foreach (var constraint in expectedConstraintsWithDefinitions)
			{
				if (constraintDefinations.TryGetValue(constraint.Name, out var constraintDefinition))
				{
					Assert.AreEqual(constraint.Definition, constraintDefinition,
						$"Constraint {constraint.Name} definition didn't match for table {tableName}");
				}
				else
				{
					Assert.Fail($"Constraint {constraint.Name} doesn't exist for table {tableName}");
				}
			}
		}

		public static bool ConstraintFromColumnNameExists(SqlConnection connection, string tableName, string columnName)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*) FROM sys.objects WHERE object_id = (SELECT sys.columns.default_object_id FROM sys.objects INNER JOIN sys.columns ON objects.object_id = sys.columns.object_id 
WHERE sys.columns.name = '{columnName}' AND sys.objects.name = '{tableName}')";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool ForeignKeyExists(SqlConnection connection, string tableName, string foreignKeyName)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT count(*) FROM sys.tables tab 
INNER JOIN sys.foreign_keys fk ON tab.object_id = fk.parent_object_id
WHERE tab.name = '{tableName}' AND fk.name = '{foreignKeyName}'";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool ForeignKeyExistsWithDeleteAction(SqlConnection connection, string tableName, string foreignKeyName, bool deleteReferentialAction)
		{
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = $@"SELECT count(*) FROM sys.tables tab 
INNER JOIN sys.foreign_keys fk ON tab.object_id = fk.parent_object_id
WHERE tab.name = '{tableName}' AND fk.name = '{foreignKeyName}' AND fk.delete_referential_action = {(deleteReferentialAction ? 1 : 0)}";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static void ExecuteNonQuery(SqlConnection conn, string script, SqlTransaction trans = null)
		{
			DBHelper.ExecuteNonQuery(conn, script, trans);
		}

		public static int ExecuteScalar(SqlConnection conn, string script, SqlTransaction trans = null)
		{
			using (var command = conn.CreateCommand())
			{
				command.Transaction = trans;
				command.CommandText = script;
				return (int)command.ExecuteScalar();
			}
		}

		public static bool ColumnTypeMatchingLowercase(SqlConnection conn, string tableName, string columnName, string columnType)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";
				var actualType = cmd.ExecuteScalar().ToString();
				return actualType.ToUpper(CultureInfo.InvariantCulture) == columnType.ToUpper(CultureInfo.InvariantCulture);
			}
		}

		public static bool IndexExists(SqlConnection conn, string tableOrViewName, string indexName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"
SELECT COUNT(*)
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
)";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool IndexesExist(SqlConnection conn, string tableOrViewName, IEnumerable<string> expectedIndexes)
		{
			var result = true;
			foreach (var index in expectedIndexes)
			{
				result = IndexExists(conn, tableOrViewName, index);
				if (!result)
				{
					break;
				}
			}

			return result;
		}

		public static bool UniqueIndexExists(SqlConnection conn, string tableOrViewName, string indexName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"
SELECT COUNT(*)
FROM sys.indexes ind
WHERE ind.name = '{indexName}'
AND (
	EXISTS
	(
		SELECT NULL
		FROM sys.tables tab
		WHERE tab.object_id = ind.object_id AND tab.name = '{tableOrViewName}' AND ind.is_unique = 1
	) OR
	EXISTS
	(
		SELECT NULL
		FROM sys.views vie
		WHERE vie.object_id = ind.object_id AND vie.name = '{tableOrViewName}' AND ind.is_unique = 1
	)
)";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool TableExists(SqlConnection conn, string tableName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*) FROM sys.tables tab 
WHERE tab.name = '{tableName}'";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static bool ObjectExists(SqlConnection conn, string objectType, string objectName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*) FROM sys.objects o
WHERE o.type = '{objectType}' AND o.name = '{objectName}'";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static string GetViewDefinition(SqlConnection conn, string viewName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"SELECT VIEW_DEFINITION FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_NAME = '{viewName}'";
				return (string)cmd.ExecuteScalar();
			}
		}

		public static string GetCollation(SqlConnection conn, string dbName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"SELECT collation_name FROM sys.databases WHERE name = '{dbName}'";
				return (string)cmd.ExecuteScalar();
			}
		}

		public static IEnumerable<DbColumn> GetExtendedTableColumns(SqlConnection connection, string tableName)
		{
			var result = new List<DbColumn>();

			var sql = $@"
SELECT 
	COLUMN_NAME,
	DATA_TYPE,
	(CASE WHEN CHARACTER_MAXIMUM_LENGTH is null THEN -1 ELSE CHARACTER_MAXIMUM_LENGTH END) AS CHARACTER_MAXIMUM_LENGTH,
	CAST (CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS bit) AS IS_NULLABLE,
	COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = '{tableName}'";

			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = sql;
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var columnDefault = reader["COLUMN_DEFAULT"];
						result.Add(new DbColumn((string)reader["COLUMN_NAME"], (string)reader["DATA_TYPE"], (int)reader["CHARACTER_MAXIMUM_LENGTH"], (bool)reader["IS_NULLABLE"], columnDefault == DBNull.Value ? string.Empty : (string)columnDefault));
					}
				}
			}

			return result;
		}

		public static bool TableColumnsExist(SqlConnection refDbConn, string tableOrTableViewName, bool isTableView, IEnumerable<DbColumn> expectedColumns)
		{
			var result = true;
			var dbColumns = GetExtendedTableColumns(refDbConn, tableOrTableViewName);
			foreach (var expectedColumn in expectedColumns)
			{
				var dbColumn = dbColumns.FirstOrDefault(x => x.ColumnName == expectedColumn.ColumnName);
				if (dbColumn != default)
				{
					result = isTableView ? dbColumn.EqualsWithoutCheckColumnDeault(expectedColumn) : dbColumn.Equals(expectedColumn);
				}
				else
				{
					result = false;
				}

				if (!result)
				{
					break;
				}
			}

			return result;
		}

		public static bool ConstraintsExist(SqlConnection refDbConn, Dictionary<string, string> expectedConstraints)
		{
			var result = true;
			foreach (var constraint in expectedConstraints)
			{
				result = ObjectExists(refDbConn, constraint.Value, constraint.Key);
				if (!result)
				{
					break;
				}
			}

			return result;
		}

		public static bool LockEscalationDisabled(SqlConnection conn, string tableName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*)  FROM sys.tables tab 
WHERE tab.name = '{tableName}' AND tab.lock_escalation_desc = 'DISABLE'";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static int GetColumnLength(SqlConnection conn, string tableName, string columnName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $"SELECT COL_LENGTH('{tableName}','{columnName}')";
				return Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		public static bool IndexIncludesColumn(SqlConnection conn, string indexName, string columnName)
		{
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = $@"SELECT COUNT(*) FROM sys.indexes si
JOIN sys.index_columns sic ON sic.object_id = si.object_id AND sic.index_id = si.index_id
JOIN sys.columns sc on sc.object_id = si.object_id AND sic.column_id = sc.column_id
WHERE 
	si.[name] = '{indexName}'
	and sc.[name] = '{columnName}'
	and sic.is_included_column = 1";
				return (int)cmd.ExecuteScalar() == 1;
			}
		}

		public static void RestoreColumnNameQuestionCodeForRefCusProfileQuestion(SqlConnection conn, SqlTransaction trans)
		{
			var refCusProfileQuestion = new RefCusProfileQuestion();
			var scripts = new StringBuilder();
			scripts.AppendLine(SharedDbSchemaChange.GetDropCheckConstranintIfExistsScript(refCusProfileQuestion.TableName, "CK_RefCusProfileQuestion_XQ2_QuestionCode"));
			scripts.AppendLine(SharedDbSchemaChange.GetDropIndexIfExistsScript(refCusProfileQuestion.TableName, "IX_RefCusProfileQuestion_XQ2_QuestionCode_XQ2_XXX_ProfileType_XQ2_ZZZ_NKDataGrouping_XQ2_StartDate"));
			scripts.AppendLine(SharedDbSchemaChange.GetRenameColumnIfExistsScript(refCusProfileQuestion.TableName, "XQ2_QuestionCode", "XQ2_Code"));
			ExecuteNonQuery(conn, scripts.ToString(), trans);
		}

		public static void AssertSchemaObjects(SqlConnection refDbConn,
			UpgradeScriptProvider provider,
			int version,
			string tableName,
			string tableViewName,
			IEnumerable<DbColumn> expectedColumns,
			Dictionary<string, string> expectedConstraints,
			IEnumerable<(string, string)> expectedConstraintsWithDefinitions,
			IEnumerable<string> expectedIndexes)
		{
			var dbTable = new DbTable(tableName, tableViewName, expectedColumns, expectedConstraints, expectedConstraintsWithDefinitions, expectedIndexes);
			AssertSchemaObjects(refDbConn, provider, version, dbTable);
		}

		public static void AssertSchemaObjects(
			SqlConnection refDbConn,
			UpgradeScriptProvider provider,
			int version,
			params DbTable[] expectedTables)
		{
			// Pre-condition
			foreach (var table in expectedTables)
			{
				Assert.False(TableExists(refDbConn, table.Name));
				Assert.False(ObjectExists(refDbConn, "V", table.ViewName));
			}

			//Execute DB Script
			ExecuteNonQuery(refDbConn, provider.GetUpgradeWrapperByVersion(version).UpgradeScript);

			//Assert
			foreach (var table in expectedTables)
			{
				Assert.True(TableExists(refDbConn, table.Name), $"{table.Name} table exists");
				Assert.True(TableColumnsExist(refDbConn, table.Name, false, table.Columns),
					$"{table.Name} columns existed with correct type and length and default value");

				Assert.True(ObjectExists(refDbConn, "V", table.ViewName), $"{table.ViewName} view exists");
				Assert.True(TableColumnsExist(refDbConn, table.ViewName, true, table.Columns),
					$"{table.ViewName} columns existed with correct type and length and default value");

				Assert.True(ConstraintsExist(refDbConn, table.Constraints), $"Should create all constraints for {table.Name}");
				AssertCheckConstraintsAndDefinitions(refDbConn, table.Name, table.ConstraintsWithDefinitions);

				Assert.True(IndexesExist(refDbConn, table.Name, table.Indexes), $"Should create all indexes for {table.Name}");
			}
		}
	}

	public class DbTable(string name, string viewName, IEnumerable<DbColumn> columns, Dictionary<string, string> constraints, IEnumerable<(string, string)> constraintsWithDefinitions, IEnumerable<string> indexes)
	{
		public string Name { get; } = name;
		public string ViewName { get; } = viewName;
		public IEnumerable<DbColumn> Columns { get; } = columns;
		public Dictionary<string, string> Constraints { get; } = constraints;
		public IEnumerable<(string, string)> ConstraintsWithDefinitions { get; } = constraintsWithDefinitions;
		public IEnumerable<string> Indexes { get; } = indexes;
	}

	public readonly struct DbColumn : IEquatable<DbColumn>
	{
		public string ColumnName { get; }
		public string DataType { get; }
		public int CharacterMaximumLength { get; }
		public bool IsNullable { get; }
		public string ColumnDefault { get; }

		public DbColumn(string columnName, string dataType, int characterMaximumLength, bool isNullable, string columnDefault)
		{
			ColumnName = columnName;
			DataType = dataType;
			CharacterMaximumLength = characterMaximumLength;
			IsNullable = isNullable;
			ColumnDefault = columnDefault;
		}

		public static bool operator ==(DbColumn a, DbColumn b) => Equals(a, b);

		public static bool operator !=(DbColumn a, DbColumn b) => !(a == b);

		public bool Equals(DbColumn other) => ColumnName == other.ColumnName && DataType == other.DataType && CharacterMaximumLength == other.CharacterMaximumLength && IsNullable == other.IsNullable && ColumnDefault == other.ColumnDefault;

		public bool EqualsWithoutCheckColumnDeault(DbColumn other) => ColumnName == other.ColumnName && DataType == other.DataType && CharacterMaximumLength == other.CharacterMaximumLength && IsNullable == other.IsNullable;

		public override bool Equals(object obj) => obj == null || !GetType().Equals(obj.GetType()) ? false : Equals((DbColumn)obj);

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = -1118741485;
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ColumnName);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DataType);
				hashCode = hashCode * -1521134295 + CharacterMaximumLength.GetHashCode();
				hashCode = hashCode * -1521134295 + IsNullable.GetHashCode();
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ColumnDefault);
				return hashCode;
			}
		}

		public override string ToString() => string.Format(CultureInfo.InvariantCulture, "Column Name: {0} | Data Type: {1} | Character Maximum Length: {2} | IsNullable: {3} | ColumnDefault: {4}", new object[] { ColumnName, DataType, CharacterMaximumLength, IsNullable, ColumnDefault });
	}
}
