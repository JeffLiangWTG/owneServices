using System;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using Moq;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public static class IDBHelperExtensions
	{
		public static void Delete<TStorage>(this IDBHelper dbHelper, string filter = null, int? timeout = null)
		{
			var proxy = new Mock<IServerProxy>();
			var fks = FKProvider.SetActionToFKS(dbHelper.GetReferencedForeignKeys(),
				Array.Empty<IDataSetInfo>());
			var sqlText = new StringBuilder();
			sqlText.AppendLine(SharedDeleteSQLBuilder.DeclareDeleteTables<TStorage>(new SqlServerSQLBuilder(), fks));
			var pkColumn = SharedSQLBuilder.GetPKColumn<TStorage>();
			var referencedColumn = SharedDeleteSQLBuilder.GetReferencedColumns(typeof(TStorage), fks);
			if (referencedColumn.Length != 0)
			{
				var referencedColumnDeclaration = new StringBuilder();

				for (var x = 0; x < referencedColumn.Length; x++)
				{
					referencedColumnDeclaration.Append(FormattableString.Invariant($"{referencedColumn[x]}{(x == referencedColumn.Length - 1 ? "" : ",")}"));
				}
				sqlText.AppendLine($@"INSERT INTO {DeleteSQLBuilder.GetDeleteTableName<TStorage>()}
SELECT {pkColumn}, {referencedColumnDeclaration} FROM {SharedSQLBuilder.GetTableName<TStorage>()}
{filter ?? string.Empty}
");
			}
			else
			{
				sqlText.AppendLine($@"INSERT INTO {DeleteSQLBuilder.GetDeleteTableName<TStorage>()}
SELECT {pkColumn} FROM {SharedSQLBuilder.GetTableName<TStorage>()}
{filter ?? string.Empty}
");
			}
			sqlText.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<TStorage>(new SqlServerSQLBuilder(), fks));
			sqlText.AppendLine(SharedDeleteSQLBuilder.Delete<TStorage>(new SqlServerSQLBuilder(), fks));
			dbHelper.ExecuteNonQuery(sqlText.ToString(), null, timeout);
		}
	}
}
