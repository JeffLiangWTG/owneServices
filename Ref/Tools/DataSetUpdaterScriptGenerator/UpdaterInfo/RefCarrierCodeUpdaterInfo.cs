using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	public class RefCarrierCodeUpdaterInfo<TStorage> : IDataSetUpdaterInfo
		where TStorage : IDataSetStorage
	{
		public RefCarrierCodeUpdaterInfo()
		{
		}

		public string GetMergeSqlText(ISQLBuilder sQLBuilder, IDictionary<Type, ForeignKeyRelationship[]> fks, ISchemaInfo schemaInfo)
		{
			var result = new StringBuilder(SQLBuilder.UpdateReferenceFKStatement<IRefVesselZZ, IRefCarrierVesselPivot>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefVesselZZ>(null)));
			var fkRelationshipAttribute = fks[typeof(IRefCarrierCode)].Where(o => o.Table == typeof(IRefCarrierCodeAttribute));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<IRefCarrierCode>(sQLBuilder, string.Empty, fks, schemaInfo.GetAllUniqueIndexes<IRefCarrierCode>(null)));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IRefCarrierCode>(string.Empty, schemaInfo.GetAllUniqueIndexes<IRefCarrierCode>(null), CarrierPKChangesTable));
			result.AppendLine(MergeSQLBuilder.CreateMergeSqlForDependentTableSql<IRefCarrierCodeAttribute>(
				fks,
				schemaInfo.GetAllUniqueIndexes<IRefCarrierCodeAttribute>(null),
				AttributeMergeSourceName,
				SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefCarrierCodeAttribute, IRefCarrierCode>(sQLBuilder, string.Empty, AttributeMergeSourceName, CarrierPKChangesTable, ParentAttributeMergeSourceName, fkRelationshipAttribute),
				fkRelationshipAttribute,
				CarrierAttributePKChangesTable,
				ParentAttributeMergeSourceName));
			var fkRelationshipVesselPivot = fks[typeof(IRefCarrierCode)].Where(o => o.Table == typeof(IRefCarrierVesselPivot));
			result.AppendLine(MergeSQLBuilder.CreateMergeSqlForDependentTableSql<IRefCarrierVesselPivot>(
				fks,
				schemaInfo.GetAllUniqueIndexes<IRefCarrierVesselPivot>(null),
				PivotMergeSourceName,
				SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefCarrierVesselPivot, IRefCarrierCode>(sQLBuilder, string.Empty, PivotMergeSourceName, CarrierPKChangesTable, ParentPivotMergeSourceName, fkRelationshipVesselPivot),
				fkRelationshipVesselPivot,
				CarrierPivotPKChangesTable,
				ParentPivotMergeSourceName));

			var fkRelationshipLanguage = fks[typeof(IRefCarrierCode)].Where(o => o.Table == typeof(IRefCarrierCodeLanguage));
			result.AppendLine(MergeSQLBuilder.CreateMergeSqlForDependentTableSql<IRefCarrierCodeLanguage>(
				fks,
				schemaInfo.GetAllUniqueIndexes<IRefCarrierCodeLanguage>(null),
				LanguageMergeSourceName,
				SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IRefCarrierCodeLanguage, IRefCarrierCode>(sQLBuilder, string.Empty, LanguageMergeSourceName, CarrierPKChangesTable, ParentLanguageMergeSourceName, fkRelationshipLanguage),
				fkRelationshipLanguage,
				CarrierLanguagePKChangesTable,
				ParentLanguageMergeSourceName));

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IRefCarrierCode>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<IRefCarrierCode>(sQLBuilder, fks));

			return result.ToString();
		}

		public IEnumerable<Tuple<Type, Type>> GetReferenceTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefVesselZZ), typeof(IRefVesselZZ));
		}

		public IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IRefCarrierCode), typeof(IRefCarrierCode));
			yield return Tuple.Create(typeof(IRefCarrierCodeAttribute), typeof(IRefCarrierCodeAttribute));
			yield return Tuple.Create(typeof(IRefCarrierVesselPivot), typeof(IRefCarrierVesselPivot));
			yield return Tuple.Create(typeof(IRefCarrierCodeLanguage), typeof(IRefCarrierCodeLanguage));
		}

		public Type GetStorageType()
		{
			return typeof(TStorage);
		}

		public string GetOverriddenPrepareTemporaryTablesScripts()
		{
			return null;
		}

		const string CarrierPKChangesTable = "@CarrierPKChangesTable";
		const string AttributeMergeSourceName = "TempCarrierAttributeCTE";
		const string PivotMergeSourceName = "TempCarrierPivotCTE";
		const string CarrierPivotPKChangesTable = "@CarrierPivotPKChangesTable";
		const string CarrierAttributePKChangesTable = "@CarrierAttributePKChangesTable";
		const string ParentAttributeMergeSourceName = "TempParentCarrierAttributeCTE";
		const string ParentPivotMergeSourceName = "TempParentCarrierPivotCTE";

		const string LanguageMergeSourceName = "TempCarrierLanguageCTE";
		const string CarrierLanguagePKChangesTable = "@CarrierLanguagePKChangesTable";
		const string ParentLanguageMergeSourceName = "TempParentCarrierLanguageCTE";
	}
}
