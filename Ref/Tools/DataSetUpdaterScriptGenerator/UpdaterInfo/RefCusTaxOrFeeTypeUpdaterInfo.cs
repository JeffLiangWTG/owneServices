using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCusTaxOrFeeTypeUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefCusTaxOrFeeTypeUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var fkTaxFeeTypeToTaxFee = fks[typeof(IRefCusTaxOrFeeType)].Where(o => o.Table == typeof(IRefCusTaxOrFee));
			var result = new StringBuilder(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefCusTaxOrFeeType>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefCusTaxOrFeeType>(null)));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefCusTaxOrFeeType>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCusTaxOrFeeType>(null), TaxOrFeeTypePKChangesTable));

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCusTaxOrFeeType>(sQLBuilder, fks));
			result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefCusTaxOrFee, IRefCusTaxOrFeeType>(sQLBuilder, string.Empty, TempTaxOrFeeCTEName, TaxOrFeeTypePKChangesTable, TempTaxOrFeeTypeCTEName, fkTaxFeeTypeToTaxFee));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTableWhenParentMerges<IRefCusTaxOrFee>(sQLBuilder, string.Empty, fkTaxFeeTypeToTaxFee, TempTaxOrFeeTypeCTEName, schemaInfo.GetAllUniqueIndexes<IRefCusTaxOrFeeType>(null), schemaInfo.GetAllUniqueIndexes<IRefCusTaxOrFee>(null), fks, TempTaxOrFeeCTEName));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTable<IRefCusTaxOrFeeLanguage>(string.Empty));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCusTaxOrFeeType>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefCusTaxOrFeeType>(sQLBuilder, fks));

			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			return Enumerable.Empty<Tuple<Type, Type>>();
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCusTaxOrFeeType), typeof(IRefCusTaxOrFeeType));
			yield return Tuple.Create(typeof(IRefCusTaxOrFee), typeof(IRefCusTaxOrFee));
			yield return Tuple.Create(typeof(IRefCusTaxOrFeeLanguage), typeof(IRefCusTaxOrFeeLanguage));
		}

		public Type GetStorageType()
		{
			return typeof(TStorage);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}

		const string TaxOrFeeTypePKChangesTable = "@TaxOrFeeTypePKChangesTable";
		const string TempTaxOrFeeCTEName = "TempTaxOrFeeCTETable";
		const string TempTaxOrFeeTypeCTEName = "TempTaxOrFeeTypeCTEName";
	}
}
