using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public static class SQLRenameColumnsHelper
	{
		internal static bool HasOldColumn(IEnumerable<RenameColumnObject> renameColumnObjectList, string tableName, IDbTransaction trans)
		{
			Argument.Argument.NotNull(renameColumnObjectList, nameof(renameColumnObjectList));
			Argument.Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.Argument.NotNull(trans, nameof(trans));

			var column = renameColumnObjectList.Select(x => x.OldColumn).FirstOrDefault();
			var hasColumnSql = FormattableString.Invariant($"SELECT COUNT(*) FROM sys.columns WHERE name = '{column}' AND object_id = OBJECT_ID('dbo.{tableName}')");
			var result = ExecuteScalar(hasColumnSql, trans);

			return (int)result > 0;
		}

		internal static string DropConstraintsAndRenameColumns(string tableName, IEnumerable<RenameColumnObject> renameColumnObjectList, IDbTransaction trans)
		{
			Argument.Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.Argument.NotNull(renameColumnObjectList, nameof(renameColumnObjectList));
			Argument.Argument.NotNull(trans, nameof(trans));

			var sqlBackupTable = FormattableString.Invariant($@"
{GetQueryToDropCheckConstraints(tableName, trans)}
{GetRenameColumnsSQL(tableName, renameColumnObjectList)}");

			return sqlBackupTable;
		}

		static List<string> LoadObjectConstraints(string tableName, IDbTransaction trans)
		{
			Argument.Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.Argument.NotNull(trans, nameof(trans));

			var checkContraints = new List<string>();
			var getCheckConstraintsSql = FormattableString.Invariant($@"SELECT c.name FROM 
sys.tables t
JOIN sys.check_constraints c on t.object_id = c.parent_object_id
where t.name = '{tableName}'");

			using (var reader = ExecuteDataReader(getCheckConstraintsSql, trans))
			{
				while (reader.Read())
				{
					var name = reader.GetString(0);
					checkContraints.Add(name);
				}
			}

			return checkContraints;
		}

		static string GetQueryToDropCheckConstraints(string tableName, IDbTransaction trans)
		{
			Argument.Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.Argument.NotNull(trans, nameof(trans));

			var objectCheckConstraints = LoadObjectConstraints(tableName, trans);
			if (objectCheckConstraints == null || !objectCheckConstraints.Any())
			{
				return string.Empty;
			}

			var sBuilder = new StringBuilder();
			foreach (var val in objectCheckConstraints)
			{
				sBuilder.AppendLine(FormattableString.Invariant($"ALTER TABLE {tableName} DROP CONSTRAINT {val};"));
			}

			return sBuilder.ToString();
		}

		static string GetRenameColumnsSQL(string tableName, IEnumerable<RenameColumnObject> renameColumnObjectList)
		{
			Argument.Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.Argument.NotNull(renameColumnObjectList, nameof(renameColumnObjectList));

			var renameColumnSql = new StringBuilder();

			foreach (var column in renameColumnObjectList)
			{
				renameColumnSql.AppendLine(FormattableString.Invariant($"EXEC sp_rename '{tableName}.{column.OldColumn}', '{column.NewColumn}', 'COLUMN';"));
			}

			return renameColumnSql.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		static object ExecuteScalar(string cmd, IDbTransaction trans)
		{
			Argument.Argument.NotNullOrEmpty(cmd, nameof(cmd));
			Argument.Argument.NotNull(trans, nameof(trans));
			using (var command = trans.Connection?.CreateCommand())
			{
				command.CommandText = cmd;
				command.Transaction = trans;

				return command.ExecuteScalar();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		static IDataReader ExecuteDataReader(string cmd, IDbTransaction trans)
		{
			Argument.Argument.NotNullOrEmpty(cmd, nameof(cmd));
			Argument.Argument.NotNull(trans, nameof(trans));

			using (var command = trans.Connection?.CreateCommand())
			{
				command.CommandText = cmd;
				command.Transaction = trans;

				return command.ExecuteReader();
			}
		}
	}
}
