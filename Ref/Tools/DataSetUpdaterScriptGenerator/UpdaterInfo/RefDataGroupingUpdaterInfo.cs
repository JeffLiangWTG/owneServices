using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefDataGroupingUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefDataGroupingUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder();
			result.AppendLine(FormattableString.Invariant($@"
UPDATE tempTbl SET {nameof(IRefDataGrouping.ZZZ_ZZZ_Grouping)} = ISNULL(tbl.{nameof(IRefDataGrouping.ZZZ_PK)}, tempTbl2.{nameof(IRefDataGrouping.ZZZ_PK)})
FROM {SQLBuilder.GetTemporaryTableName<IRefDataGrouping>(string.Empty)} tempTbl
LEFT JOIN {SharedSQLBuilder.GetTableName<IRefDataGrouping>()} tbl ON tbl.{nameof(IRefDataGrouping.ZZZ_DataGrouping)} = tempTbl.{nameof(RefDataGrouping.ZZZ_ZZZ_NKGrouping)}
LEFT JOIN {SQLBuilder.GetTemporaryTableName<IRefDataGrouping>(string.Empty)} tempTbl2 ON tempTbl2.{nameof(IRefDataGrouping.ZZZ_DataGrouping)} = tempTbl.{nameof(RefDataGrouping.ZZZ_ZZZ_NKGrouping)}
"));

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefDataGrouping>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefDataGrouping>(null)));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefDataGrouping>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefDataGrouping>(null)));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefDataGrouping>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefDataGrouping>(sQLBuilder, fks));
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type, Type>>();
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefDataGrouping), typeof(RefDataGrouping));
		}

		public Type GetStorageType()
		{
			return typeof(TStorage);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			var allColumns = SharedSQLBuilder.GetAllColumnProperties<IRefDataGrouping>().Select(x => x.Name).Select(x => Tuple.Create(x, x)).ToList();
			allColumns.Add(SharedSQLBuilder.CreateDeletedColumn());
			var nullableColumnExpression = FormattableString.Invariant($"SUBSTRING({nameof(IRefDataGrouping.ZZZ_DataGrouping)}, 0, LEN({nameof(IRefDataGrouping.ZZZ_DataGrouping)}))");
			allColumns.Add(Tuple.Create(nullableColumnExpression, nameof(RefDataGrouping.ZZZ_ZZZ_NKGrouping)));

			var t = SQLBuilder.GetCreateTemporaryTableSql<IRefDataGrouping>(string.Empty, SharedSQLBuilder.GetTableName(typeof(IRefDataGrouping)), allColumns);

			var result = $@"{{ ""#TempRefDataGrouping"", @""{SQLBuilder.GetCreateTemporaryTableSql<IRefDataGrouping>(string.Empty, SharedSQLBuilder.GetTableName(typeof(IRefDataGrouping)), allColumns)}
"" }}";
			return result;
		}
	}
}
