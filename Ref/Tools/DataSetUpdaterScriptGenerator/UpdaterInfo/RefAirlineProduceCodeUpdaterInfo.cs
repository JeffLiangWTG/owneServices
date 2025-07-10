using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefAirlineProduceCodeUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefAirlineProduceCodeUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder(SQLBuilder.UpdateReferenceFKStatement<IRefAirlineCommodityCode, IRefAirlineProductCodeCommodityCodePivot>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefAirlineCommodityCode>(null)));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefAirlineProductCode>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefAirlineProductCode>(null)));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefAirlineProductCode>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefAirlineProductCode>(null), ProductPKChangesTable));
			var fkRelationshipCommodityPivot = fks[typeof(IRefAirlineProductCode)].Where(o => o.Table == typeof(IRefAirlineProductCodeCommodityCodePivot));
			result.AppendLine(MergeSQLBuilder.CreateMergeSqlForDependentTableSql<IRefAirlineProductCodeCommodityCodePivot>(
				fks,
				schemaInfo.GetAllUniqueIndexes<IRefAirlineProductCodeCommodityCodePivot>(null),
				PivotMergeSourceName,
				SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefAirlineProductCodeCommodityCodePivot, IRefAirlineProductCode>(sQLBuilder, string.Empty, PivotMergeSourceName, ProductPKChangesTable, ParentPivotMergeSourceName, fkRelationshipCommodityPivot),
				fkRelationshipCommodityPivot,
				ProductPivotPKChangesTable,
				ParentPivotMergeSourceName));

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefAirlineProductCode>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefAirlineProductCode>(sQLBuilder, fks));

			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefAirlineCommodityCode), typeof(IRefAirlineCommodityCode));
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefAirlineProductCode), typeof(IRefAirlineProductCode));
			yield return Tuple.Create(typeof(IRefAirlineProductCodeCommodityCodePivot), typeof(IRefAirlineProductCodeCommodityCodePivot));
		}

		public Type GetStorageType()
		{
			return typeof(TStorage);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}

		const string ProductPKChangesTable = "@ProductPKChangesTable";
		const string PivotMergeSourceName = "TempProductPivotCTE";
		const string ProductPivotPKChangesTable = "@ProductPivotPKChangesTable";
		const string ParentPivotMergeSourceName = "TempParentProductPivotCTE";
	}
}
