using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Models;

namespace CargoWise.RefDataRepo.Ent.Client
{
	[SuppressMessage("Microsoft.Design", "CA1005")] // re-visit in WI00134468
	public class TwoTableDataSetUpdater<TServerParent, TServerChild, TStorageParent, TStorageChild> : DataSetUpdater<TServerParent, TStorageParent>
		where TServerParent : RefDataSet
	{
		public TwoTableDataSetUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager, bool replaceChild = false, bool updateIsActiveColumnAlways = false)
			: base(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter())
		{
			this.replaceChild = replaceChild;
			this.updateIsActiveColumnAlways = updateIsActiveColumnAlways;
		}

		public TwoTableDataSetUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager, int updaterVersion, bool replaceChild = false, bool updateIsActiveColumnAlways = false)
			: this(proxy, dbHelper, versionControlManager, replaceChild, updateIsActiveColumnAlways)
		{
			overridenUpdaterVersion = updaterVersion;
		}

		readonly int? overridenUpdaterVersion;
		readonly bool replaceChild;
		readonly bool updateIsActiveColumnAlways;

		public override int UpdaterVersion => overridenUpdaterVersion ?? base.UpdaterVersion;

		public override IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(TStorageParent), typeof(TServerParent));
			yield return Tuple.Create(typeof(TStorageChild), typeof(TServerChild));
		}

		protected override string GetMergeSqlTextCore(IDbTransaction transaction, IDictionary<Type, ForeignKeyRelationship[]> fks, string dataSetName)
		{
			var fkRelationship = fks[typeof(TStorageParent)].Where(o => o.Table == typeof(TStorageChild));
			var result = new StringBuilder(SharedDeleteSQLBuilder.SaveDeleteRecord<TStorageParent>(sQLBuilder, dataSetName, fks, DBHelper.GetAllUniqueIndexes<TStorageParent>(transaction)));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<TStorageParent>(dataSetName, DBHelper.GetAllUniqueIndexes<TStorageParent>(transaction), ParentPKChangesTable, updateIsActiveColumnAlways: updateIsActiveColumnAlways));
			if (replaceChild)
			{
				result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<TStorageParent>(sQLBuilder, fks));
				result.AppendLine(SharedMergeSQLBuilder.CreateSourceTableForMergeSql<TStorageChild, TStorageParent>(sQLBuilder, dataSetName, MergeSourceName, ParentPKChangesTable, ParentTempCTEName, fkRelationship));
				result.AppendLine(ReplaceSQLBuilder.CreateReplaceSqlForDependentTableWhenParentMerges<TStorageChild>(sQLBuilder, dataSetName, fkRelationship, ParentTempCTEName, DBHelper.GetAllUniqueIndexes<TStorageParent>(transaction), DBHelper.GetAllUniqueIndexes<TStorageChild>(transaction), fks, MergeSourceName));
				return result.ToString();
			}
			result.AppendLine(MergeSQLBuilder.CreateMergeSqlForDependentTableSql<TStorageChild>(
				fks,
				DBHelper.GetAllUniqueIndexes<TStorageChild>(transaction),
				MergeSourceName,
				SharedMergeSQLBuilder.CreateSourceTableForMergeSql<TStorageChild, TStorageParent>(sQLBuilder, dataSetName, MergeSourceName, ParentPKChangesTable, ParentTempCTEName, fkRelationship),
				fkRelationship,
				ChildPKChangesTable,
				ParentTempCTEName));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<TStorageParent>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<TStorageParent>(sQLBuilder, fks));
			return result.ToString();
		}

		const string ParentPKChangesTable = "@ParentPKChangesTable";
		const string MergeSourceName = "TempChilTableCTE";
		const string ChildPKChangesTable = "@ChildPKChangesTable";
		const string ParentTempCTEName = "TempParentTableCTE";
	}
}
