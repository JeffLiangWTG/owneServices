using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.SqlServer;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public class DummyDataSetUpdater : DataSetUpdater<Dummy, IDummyStorage>
	{
		public DummyDataSetUpdater(IServerProxy proxy, IDBHelper dbHelper, IRefVersionControlManager versionControlManager)
			: base(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter())
		{
			MergeText = string.Empty;
		}

		protected override void AddServerData(IEnumerable<DataTable> serverDataTables, Dummy dataSet)
		{
			if (addServerData != null)
			{
				addServerData(serverDataTables, dataSet);
			}
		}

		protected override string GetMergeSqlTextCore(IDbTransaction transaction, IDictionary<Type, ForeignKeyRelationship[]> fks, string dataSetName)
		{
			return MergeText;
		}

		public override IEnumerable<Tuple<Type, Type>> GetTypeByInsertOrder()
		{
			return TypesByInsertOrder;
		}

		public override int UpdaterVersion
		{
			get { return dummyVersion; }
		}

		public void CleanupTemporaryTables_Exposed(string dataSetName)
		{
			base.CleanupTemporaryTables(dataSetName);
		}

		public int dummyVersion;

		public Tuple<Type, Type>[] TypesByInsertOrder = Array.Empty<Tuple<Type, Type>>();
		public string MergeText { get; set; }
		public Action<IEnumerable<DataTable>, Dummy> addServerData;
	}
}
