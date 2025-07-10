using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefUNLOCOUpdaterInfo : IDataSetUpdaterInfo
	{
		public RefUNLOCOUpdaterInfo(int cachedMonths = 24)
		{
			this.cachedMonths = cachedMonths;
		}

		readonly int cachedMonths;

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var fkRelationship = fks[typeof(IRefUNLOCO)].Where(o => o.ReferencedTable == typeof(IRefUNLOCO) && o.Table == typeof(IRefUNLOCOUtcOffset));
			var fkRelatedPortRelationship = fks[typeof(IRefUNLOCO)].Where(o => o.ReferencedTable == typeof(IRefUNLOCO) && o.Table == typeof(IRefUNLOCORelatedPort));
			var result = new StringBuilder();
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefUNLOCO>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefUNLOCO>(null), ParentPKChangesTable));

			result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefUNLOCOUtcOffset, IRefUNLOCO>(sQLBuilder, string.Empty, UtcOffsetMergeSourceName, ParentPKChangesTable, UtcoffsetParentTempCTEName, fkRelationship));
			result.AppendLine(SharedReplaceSQLBuilder.CreateInsertSqlToDeleteForDependentTable<IRefUNLOCOUtcOffset>(sQLBuilder, fkRelationship, schemaInfo.GetAllUniqueIndexes<IRefUNLOCOUtcOffset>(null), UtcoffsetParentTempCTEName, UtcOffsetMergeSourceName));

			result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefUNLOCORelatedPort, IRefUNLOCO>(sQLBuilder, string.Empty, RelatedPortsetMergeSourceName, ParentPKChangesTable, RelatedPortParentTempCTEName, fkRelatedPortRelationship));
			result.AppendLine(SharedReplaceSQLBuilder.CreateInsertSqlToDeleteForDependentTable<IRefUNLOCORelatedPort>(sQLBuilder, fkRelatedPortRelationship, schemaInfo.GetAllUniqueIndexes<IRefUNLOCORelatedPort>(null), RelatedPortParentTempCTEName, RelatedPortsetMergeSourceName));

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefUNLOCO>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefUNLOCO>(null), true, false));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefUNLOCOUtcOffset>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefUNLOCORelatedPort>(sQLBuilder, fks));

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefUNLOCO>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete(sQLBuilder, typeof(IRefUNLOCO), fks));
			result.AppendLine(SharedReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefUNLOCOUtcOffset>(sQLBuilder, string.Empty));
			result.AppendLine(SharedReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefUNLOCORelatedPort>(sQLBuilder, string.Empty));
			result.AppendLine(RemoveUtcOffsetRecordOutsideDateRange());
			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type, Type>>();
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefUNLOCO), typeof(IRefUNLOCO));
			yield return Tuple.Create(typeof(IRefUNLOCOUtcOffset), typeof(IRefUNLOCOUtcOffset));
			yield return Tuple.Create(typeof(IRefUNLOCORelatedPort), typeof(IRefUNLOCORelatedPort));
		}

		public Type GetStorageType()
		{
			return typeof(IRefUNLOCO);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}

		string RemoveUtcOffsetRecordOutsideDateRange()
		{
			var date = $"DATEADD(MONTH, -{cachedMonths}, GETUTCDATE())";
			var tableName = SharedSQLBuilder.GetTableName<IRefUNLOCOUtcOffset>();
			return $@"DELETE FROM {tableName}
WHERE RLO_StartTimeUtc < {date}
OR RLO_EndTimeUtc < {date};"; // SuppressCodeSmell Reason = SQL Statement
		}

		const string ParentPKChangesTable = "@ParentPKChangesTable";
		const string UtcOffsetMergeSourceName = "TempChildTableCTEForUtcoffSet";
		const string UtcoffsetParentTempCTEName = "TempParentTableCTEForUtcOffset";
		const string RelatedPortsetMergeSourceName = "TempChildTableCTEForRelatedPort";
		const string RelatedPortParentTempCTEName = "TempParentTableCTEForrelatedPort";
	}
}
