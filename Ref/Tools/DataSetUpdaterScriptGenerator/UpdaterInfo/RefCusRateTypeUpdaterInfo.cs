using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusRateTypeUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefCusRateTypeUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefCusRateType>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefCusRateType>(null)));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefCusRateType>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusRateType>(null), RateTypePKChangesTable));
			result.AppendLine(GetChildMergeSql<IRefCusRateType, IRefCusRateTypeLanguage>(sQLBuilder, fks, RateTypePKChangesTable, RateTypeLanguagePKChangesTable, ParentRateTypeLanguageMergeSourceName, schemaInfo));
			result.AppendLine(GetChildMergeSql<IRefCusRateType, IRefCusRateCode>(sQLBuilder, fks, RateTypePKChangesTable, RateCodePKChangeTable, ParentRateCodeMergeSourceName, schemaInfo));
			result.AppendLine(GetChildMergeSql<IRefCusRateCode, IRefCusRateCodeLanguage>(sQLBuilder, fks, RateCodePKChangeTable, RateCodeLanguagePKChangesTable, ParentRateCodeLanguageMergeSourceName, schemaInfo));

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCusRateType>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefCusRateType>(sQLBuilder, fks));

			return result.ToString();
		}

		static string GetChildMergeSql<TParentStorage, TChildStorage>(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, string parentOutputPKTable, string outputPKTable, string parentCTETableName, ISchemaInfo schemaInfo)
		{
			var fkRelationship = fks[typeof(TParentStorage)].Where(o => o.Table == typeof(TChildStorage));
			var mergeSourceName = "Temp" + typeof(TChildStorage).Name + "CTE"; // SuppressCodeSmell Reason = SQL Statement
			return MergeSQLBuilder.CreateMergeSqlForDependentTableSql<TChildStorage>(
				fks,
				schemaInfo.GetAllUniqueIndexes<TChildStorage>(null),
				mergeSourceName,
				SharedMergeSQLBuilder.CreateSourceTableForMergeSql<TChildStorage, TParentStorage>(sQLBuilder, string.Empty, mergeSourceName, parentOutputPKTable, parentCTETableName, fkRelationship),
				fkRelationship,
				outputPKTable,
				parentCTETableName
			);
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type, Type>>();
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusRateType), typeof(IRefCusRateType));
			yield return Tuple.Create(typeof(IRefCusRateCode), typeof(IRefCusRateCode));
			yield return Tuple.Create(typeof(IRefCusRateCodeLanguage), typeof(IRefCusRateCodeLanguage));
			yield return Tuple.Create(typeof(IRefCusRateTypeLanguage), typeof(IRefCusRateTypeLanguage));
		}

		public Type GetStorageType()
		{
			return typeof(TStorage);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}

		const string RateTypePKChangesTable = "@RateTypePKChangesTable";
		const string RateTypeLanguagePKChangesTable = "@RateTypeLanguagePKChangesTable";
		const string RateCodePKChangeTable = "@RateCodePKChangesTable";
		const string RateCodeLanguagePKChangesTable = "@RateCodeLanguagePKChangesTable";
		const string ParentRateTypeLanguageMergeSourceName = "TempParentRateTypeRateTypeLanguageCTE";
		const string ParentRateCodeMergeSourceName = "TempParentRateTypeRateCodeCTE";
		const string ParentRateCodeLanguageMergeSourceName = "TempParentRateCodeRateCodeLanguageCTE";
	}
}
