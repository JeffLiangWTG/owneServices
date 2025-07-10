using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class TwoTableDataSetUpdaterInfo<TStorageParent, TStorageChild> : IDataSetUpdaterInfo
	{
		public TwoTableDataSetUpdaterInfo(bool useReplace)
		{
			this.useReplace = useReplace;
		}
		readonly bool useReplace;

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var fkRelationship = fks[typeof(TStorageParent)].Where(o => o.Table == typeof(TStorageChild));
			var result = new StringBuilder(SharedDeleteSQLBuilder.SaveDeleteRecord<TStorageParent>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<TStorageParent>(null)));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<TStorageParent>(string.Empty, schemaInfo.GetAllUniqueIndexes<TStorageParent>(null), ParentPKChangesTable, updateIsActiveColumnAlways: true));
			if (useReplace)
			{
				result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<TStorageParent>(sQLBuilder, fks));
				result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<TStorageChild, TStorageParent>(sQLBuilder, string.Empty, MergeSourceName, ParentPKChangesTable, ParentTempCTEName, fkRelationship));
				result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTableWhenParentMerges<TStorageChild>(sQLBuilder, string.Empty, fkRelationship, ParentTempCTEName, schemaInfo.GetAllUniqueIndexes<TStorageParent>(null), schemaInfo.GetAllUniqueIndexes<TStorageChild>(null), fks, MergeSourceName));
				return result.ToString();
			}
			result.AppendLine(MergeSQLBuilder.CreateMergeSqlForDependentTableSql<TStorageChild>(
				fks,
				schemaInfo.GetAllUniqueIndexes<TStorageChild>(null),
				MergeSourceName,
				SharedMergeSQLBuilder.CreateSourceTableForMergeSql<TStorageChild, TStorageParent>(sQLBuilder, string.Empty, MergeSourceName, ParentPKChangesTable, ParentTempCTEName, fkRelationship),
				fkRelationship,
				ChildPKChangesTable,
				ParentTempCTEName));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<TStorageParent>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<TStorageParent>(sQLBuilder, fks));
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type,Type>>();
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(TStorageParent), typeof(TStorageParent));
			yield return Tuple.Create(typeof(TStorageChild), typeof(TStorageChild));
		}

		public Type GetStorageType()
		{
			return typeof(TStorageParent);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}

		const string ParentPKChangesTable = "@ParentPKChangesTable";
		const string MergeSourceName = "TempChilTableCTE";
		const string ChildPKChangesTable = "@ChildPKChangesTable";
		const string ParentTempCTEName = "TempParentTableCTE";
	}
}
