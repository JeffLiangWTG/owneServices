using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class UNDGSubstanceUpdater : DataSetUpdater<UNDGSubstance, IZZUNDGSubstance>
	{
		public UNDGSubstanceUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager)
			: base(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter())
		{
		}

		public override int UpdaterVersion => 3;

		public override IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(IZZUNDGSubstance), typeof(UNDGSubstance));
			yield return Tuple.Create(typeof(IUNDGAttribute), typeof(UNDGAttribute));
		}

		protected override string GetMergeSqlTextCore(IDbTransaction transaction, IDictionary<Type, ForeignKeyRelationship[]> fks, string dataSetName)
		{
			var fkRelationshipAttribute = fks[typeof(IZZUNDGSubstance)].Where(o => o.Table == typeof(IUNDGAttribute));

			var result = new StringBuilder(SharedDeleteSQLBuilder.SaveDeleteRecord<IZZUNDGSubstance>(sQLBuilder, dataSetName, fks, DBHelper.GetAllUniqueIndexes<IZZUNDGSubstance>(transaction)));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<IZZUNDGSubstance>(dataSetName, DBHelper.GetAllUniqueIndexes<IZZUNDGSubstance>(transaction), SubstancePKChangesTable, updateIsActiveColumnAlways: true));

			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<IZZUNDGSubstance>(sQLBuilder, fks));
			result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<IUNDGAttribute, IZZUNDGSubstance>(sQLBuilder, dataSetName, AttributeMergeSourceName, SubstancePKChangesTable, ParentSubstanceAttributeMergeSourceName, fkRelationshipAttribute));
			result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTableWhenParentMerges<IUNDGAttribute>(sQLBuilder, dataSetName, fkRelationshipAttribute, ParentSubstanceAttributeMergeSourceName, DBHelper.GetAllUniqueIndexes<IZZUNDGSubstance>(transaction), DBHelper.GetAllUniqueIndexes<IUNDGAttribute>(transaction), fks, AttributeMergeSourceName));

			return result.ToString();
		}

		const string SubstancePKChangesTable = "@SubstancePKChangesTable";
		const string AttributeMergeSourceName = "TempSubstanceAttributeCTE";
		const string ParentSubstanceAttributeMergeSourceName = "TempParentSubstanceAttributeCTE";
	}
}
