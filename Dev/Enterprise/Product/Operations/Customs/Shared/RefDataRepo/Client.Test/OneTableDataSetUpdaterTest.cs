using System;
using System.Data;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Common.Models;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	abstract class OneTableDataSetUpdaterTest<TServer, TStorage> : TestCase
		where TServer : RefDataSet
		where TStorage : class, IDataSetStorage
	{
		public virtual void TestUpdate()
		{
			if (ColumnForUpdate != null)
			{
				var oldRefDbVersionControl = versionControlManager.GetVersionControl(DataSetName);
				versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow.AddDays(-1), string.Empty, GetUpdater(null).UpdaterVersion), oldRefDbVersionControl);
				PrepareData();
				var storageData = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0}", SharedSQLBuilder.GetTableName<TStorage>()));
				var serverData = CreateServerData(storageData);
				typeof(TServer).GetProperty(ColumnForUpdate).SetValue(serverData, ValueForUpdate);
				var proxy = Helper.GetServerProxy(serverData);
				Helper.RunUpdater(GetUpdater(proxy));
				if (UseReplace)
				{
					var dataTable = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0} WHERE {1} = '{2}'", SharedSQLBuilder.GetTableName<TStorage>(), ColumnForUpdate, ValueForUpdate));
					AssertGreaterThan(dataTable.Rows.Count, 0);
					var oldRecord = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0} WHERE {1} = '{2}'", SharedSQLBuilder.GetTableName<TStorage>(), SharedSQLBuilder.GetPKColumn<TStorage>(),
						storageData.Rows[0][SharedSQLBuilder.GetPKColumn<TStorage>()]));
					AssertEquals(0, oldRecord.Rows.Count);
					AssertSysColumnsWhenInsert();
				}
				else
				{
					var dataTable = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0} WHERE {1} = '{2}'", SharedSQLBuilder.GetTableName<TStorage>(), SharedSQLBuilder.GetPKColumn<TStorage>(),
						storageData.Rows[0][SharedSQLBuilder.GetPKColumn<TStorage>()]));
					AssertEquals(ValueForUpdate, dataTable.Rows[0][ColumnForUpdate]);
					AssertSysColumnsWhenUpdate(storageData, dataTable);
				}
			}
			else
			{
				Assert(true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Windows Compact FrameWorks does not support Enterprise.Core.Globalisation")]
		protected virtual void AssertSysColumnsWhenUpdate(DataTable formerTable, DataTable updatedTable)
		{
			if (SharedSQLBuilder.HasSystemTimeAndUserColumns<TStorage>())
			{
				var prefix = SharedSQLBuilder.GetTablePrefix(typeof(TStorage));
				var formerLastEditTime = DateTime.Parse(formerTable.Rows[0][$"{prefix}_SystemLastEditTimeUtc"].ToString());
				var lastEditTime = DateTime.Parse(updatedTable.Rows[0][$"{prefix}_SystemLastEditTimeUtc"].ToString());
				Assert($"{prefix}_SystemLastEditTimeUtc should be updated.", DateTime.Compare(lastEditTime, formerLastEditTime) >= 0);
				AssertEquals($"{prefix}_SystemLastEditUser", "~BP", updatedTable.Rows[0][$"{prefix}_SystemLastEditUser"]);
			}
		}

		protected virtual bool UseReplace
		{
			get { return false; }
		}

		protected virtual object ValueForUpdate
		{
			get { return "XX"; }
		}

		protected virtual TServer CreateServerData(DataTable storageData)
		{
			return Helper.CreateServerData<TServer>(storageData);
		}

		public virtual void TestDelete()
		{
			var oldRefDbVersionControl = versionControlManager.GetVersionControl(DataSetName);
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow.AddDays(-1), string.Empty, GetUpdater(null).UpdaterVersion), oldRefDbVersionControl);
			PrepareData();
			var storageData = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0}", SharedSQLBuilder.GetTableName<TStorage>()));
			var serverData = CreateServerData(storageData);
			serverData.Deleted = true;

			var count = (int)conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorage>()));
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(count - 1, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorage>())));
		}

		public virtual void TestInsert()
		{
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow.AddDays(-1), string.Empty, GetUpdater(null).UpdaterVersion), null);
			PrepareData();
			var storageData = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0}", SharedSQLBuilder.GetTableName<TStorage>()));
			var serverData = CreateServerData(storageData);
			dbHelper.Delete<TStorage>(null, 300);
			AssertEquals(0, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorage>())));
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(1, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorage>())));

			AssertSysColumnsWhenInsert();
		}

		protected virtual void AssertSysColumnsWhenInsert()
		{
			if (SharedSQLBuilder.HasSystemTimeAndUserColumns<TStorage>())
			{
				var tableName = SharedSQLBuilder.GetTableName<TStorage>();
				var systemColumns = SharedSQLBuilder.GetSystemTimeAndUserColumnsInOrder(typeof(TStorage));
				using (var reader = conn.ExecuteReader($"SELECT TOP 1 {systemColumns} FROM {tableName}"))
				{
					if (reader.Read())
					{
						AssertNotNullOrEmpty($"{tableName}.SystemCreateTimeUtc", reader[0].ToString());
						AssertNotNullOrEmpty($"{tableName}.SystemLastEditTimeUtc", reader[2].ToString());
						AssertEquals($"{tableName}.SystemCreateUser", "~BP", reader[1].ToString());
						AssertEquals($"{tableName}.SystemLastEditUser", "~BP", reader[3].ToString());
					}
				}
			}
		}

		protected string DataSetName => SharedSQLBuilder.GetTableName<TStorage>();

		protected abstract IDataSetUpdater GetUpdater(IServerProxy proxy);

		protected abstract string ColumnForUpdate { get; }

		protected virtual void PrepareData() { }

		protected IDbConnection conn;
		protected IDBHelper dbHelper;
		protected IRefVersionControlManager versionControlManager;

		protected override void SetUp()
		{
			conn = ((IDbConnectionInternals)Db.NewAdminConnection()).ADOConnection;
			dbHelper = new DBHelper(conn);
			versionControlManager = new RefVersionControlManager(dbHelper);
			versionControlManager.IsMainDb = true;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (conn != null)
			{
				conn.Dispose();
			}
		}
	}
}
