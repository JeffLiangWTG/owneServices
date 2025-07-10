using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Models;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class OneTableDataSetUpdater<TServer, TStorage> : DataSetUpdater<TServer, TStorage>
		where TStorage : class, IDataSetStorage
		where TServer : RefDataSet
	{
		public OneTableDataSetUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager, bool useReplace = false, bool updateIsActive = false)
			: base(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter())
		{
			this.useReplace = useReplace;
			this.updateIsActive = updateIsActive;
		}

		public OneTableDataSetUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager, int updaterVersion, bool useReplace = false)
			: this(proxy, dbHelper, versionControlManager, useReplace)
		{
			this.overridenUpdaterVersion = updaterVersion;
		}

		readonly int? overridenUpdaterVersion;
		readonly bool useReplace;
		readonly bool updateIsActive;

		public override int UpdaterVersion => overridenUpdaterVersion ?? base.UpdaterVersion;

		public override IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			yield return Tuple.Create(typeof(TStorage), typeof(TServer));
		}

		protected override string GetMergeSqlTextCore(IDbTransaction transaction, IDictionary<Type, ForeignKeyRelationship[]> fks, string dataSetName)
		{
			if (useReplace)
			{
				return ReplaceSQLBuilder.CreateReplaceSql<TStorage>(sQLBuilder, dataSetName, fks, DBHelper.GetAllUniqueIndexes<TStorage>(transaction));
			}
			var result = new StringBuilder();
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteRecord<TStorage>(sQLBuilder, dataSetName, fks, DBHelper.GetAllUniqueIndexes<TStorage>(transaction)));
			result.AppendLine(MergeSQLBuilder.CreateMergeSql<TStorage>(dataSetName, DBHelper.GetAllUniqueIndexes<TStorage>(transaction), null, true, "", updateIsActive));
			result.AppendLine(SharedDeleteSQLBuilder.SaveDeleteOrUpdateDependentRecord<TStorage>(sQLBuilder, fks));
			result.AppendLine(SharedDeleteSQLBuilder.Delete<TStorage>(sQLBuilder, fks));
			return result.ToString();
		}
	}
}


