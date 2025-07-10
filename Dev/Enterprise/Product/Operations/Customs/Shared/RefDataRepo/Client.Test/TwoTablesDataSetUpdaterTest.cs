using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Common.Models;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	abstract class TwoTablesDataSetUpdaterTest<TServer, TServerChild, TStorage, TStorageChild> : TestCase
		where TServer : RefDataSet
		where TStorage : class, IDataSetStorage
	{
		public void TestUpdate_ForTwoDataSets()
		{
			var dataSet1Name = "dataset1";
			var dataSet2Name = "dataset2";
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(dataSet1Name, DateTime.UtcNow.AddDays(-1), string.Empty, 1), null);
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(dataSet2Name, DateTime.UtcNow.AddDays(-1), string.Empty, 1), null);
			PrepareData();

			DataTable storageData = Helper.GetStorageDataHavingAtLeastOneChild<TStorage, TStorageChild>(conn, FKColumnName, ParentRelationshipColumn);
			var serverChildData = CreateChildServerData(storageData.Rows[0][ParentRelationshipColumn]);
			var serverData = CreateServerData(storageData);
			if (!string.IsNullOrEmpty(StringColumnForUpdate))
			{
				typeof(TServer).GetProperty(StringColumnForUpdate).SetValue(serverData, "XX");
			}
			if (!string.IsNullOrEmpty(ChildStringColumnForUpdate) && serverChildData.Length > 0)
			{
				typeof(TServerChild).GetProperty(ChildStringColumnForUpdate).SetValue(serverChildData[0], ChildStringValueForUpdate);
			}
			SetServerChild(serverData, serverChildData);
			var pkCol = ParentRelationshipColumn;
			dbHelper.Delete<TStorage>(string.Format("WHERE {0} <> '{1}'", pkCol, storageData.Rows[0][pkCol]), 300);
			var proxy = Helper.GetServerProxy(serverData);
			var updater = GetUpdater(proxy);
			Helper.RunUpdater(updater, dataSet1Name);
			Helper.RunUpdater(updater, dataSet2Name);

			Assert(true);
		}

		string DataSetName => SharedSQLBuilder.GetTableName<TStorage>();

		public void TestUpdate()
		{
			var oldRefDbVersionControl = versionControlManager.GetVersionControl(DataSetName);
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow.AddDays(-1), string.Empty, 1), oldRefDbVersionControl);
			PrepareData();
			DataTable storageData = Helper.GetStorageDataHavingAtLeastOneChild<TStorage, TStorageChild>(conn, FKColumnName, ParentRelationshipColumn);
			var serverChildData = CreateChildServerData(storageData.Rows[0][ParentRelationshipColumn]);
			var serverData = CreateServerData(storageData);
			if (!string.IsNullOrEmpty(StringColumnForUpdate))
			{
				typeof(TServer).GetProperty(StringColumnForUpdate).SetValue(serverData, "XX");
			}
			if (!string.IsNullOrEmpty(ChildStringColumnForUpdate) && serverChildData.Length > 0)
			{
				typeof(TServerChild).GetProperty(ChildStringColumnForUpdate).SetValue(serverChildData[0], ChildStringValueForUpdate);
			}
			SetServerChild(serverData, serverChildData);
			var pkCol = ParentRelationshipColumn;
			dbHelper.Delete<TStorage>(string.Format("WHERE {0} <> '{1}'", pkCol, storageData.Rows[0][pkCol]), 300);
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			var newStorageData = Helper.GetStorageDataHavingAtLeastOneChild<TStorage, TStorageChild>(conn, FKColumnName, ParentRelationshipColumn);
			if (!string.IsNullOrEmpty(StringColumnForUpdate))
			{
				AssertEquals("XX", newStorageData.Rows[0][StringColumnForUpdate]);
				AssertParentSysColumnsWhenUpdate(storageData, newStorageData);
			}
			if (!string.IsNullOrEmpty(ChildStringColumnForUpdate) && serverChildData.Length > 0)
			{
				AssertEquals(1, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0} WHERE {1} = '{2}'", SharedSQLBuilder.GetTableName<TStorageChild>(), ChildStringColumnForUpdate, ChildStringValueForUpdate)));
				AssertChildSysColumnsWhenUpdate();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Windows Compact FrameWorks does not support Enterprise.Core.Globalisation")]
		protected virtual void AssertParentSysColumnsWhenUpdate(DataTable formerTable, DataTable updatedTable)
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

		protected virtual void AssertChildSysColumnsWhenUpdate()
		{
			AssertSysColumnsWhenInsert(typeof(TStorageChild));
		}

		public void TestDelete()
		{
			var oldRefDbVersionControl = versionControlManager.GetVersionControl(DataSetName);
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow.AddDays(-1), string.Empty, GetUpdater(null).UpdaterVersion), oldRefDbVersionControl);
			PrepareData();
			DataTable storageData = Helper.GetStorageDataHavingAtLeastOneChild<TStorage, TStorageChild>(conn, FKColumnName, ParentRelationshipColumn);

			var serverChildData = CreateChildServerData(storageData.Rows[0][ParentRelationshipColumn]);
			var serverData = CreateServerData(storageData);
			serverData.Deleted = true;

			var count = (int)conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorage>()));
			var childCount = (int)conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorageChild>()));
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(count - 1, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorage>())));
			AssertEquals(childCount - serverChildData.Length, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorageChild>())));
		}

		public void TestDeleteChild()
		{
			var oldRefDbVersionControl = versionControlManager.GetVersionControl(DataSetName);
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow.AddDays(-1), string.Empty, GetUpdater(null).UpdaterVersion), oldRefDbVersionControl);
			PrepareData();
			DataTable storageData = Helper.GetStorageDataHavingAtLeastOneChild<TStorage, TStorageChild>(conn, FKColumnName, ParentRelationshipColumn);
			var serverChildData = CreateChildServerData(storageData.Rows[0][ParentRelationshipColumn]);
			var serverData = CreateServerData(storageData);
			var childCount = (int)conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorageChild>()));
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(childCount - serverChildData.Length, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorageChild>())));
		}

		public void TestInsert()
		{
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(DataSetName, DateTime.UtcNow.AddDays(-1), string.Empty, 1), null);
			PrepareData();
			DataTable storageData = Helper.GetStorageDataHavingAtLeastOneChild<TStorage, TStorageChild>(conn, FKColumnName, ParentRelationshipColumn);

			var serverChildData = CreateChildServerData(storageData.Rows[0][ParentRelationshipColumn]);
			var serverData = CreateServerData(storageData);

			SetServerChild(serverData, serverChildData);
			dbHelper.Delete<TStorage>(null, 300);
			AssertEquals(0, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorage>())));
			AssertEquals(0, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorageChild>())));
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(1, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorage>())));
			AssertEquals(serverChildData.Length, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<TStorageChild>())));
			AssertSysColumnsWhenInsert(typeof(TStorage));
			AssertSysColumnsWhenInsert(typeof(TStorageChild));
		}

		protected virtual void AssertSysColumnsWhenInsert(Type storageType)
		{
			if (SharedSQLBuilder.HasSystemTimeAndUserColumns(storageType))
			{
				var tableName = SharedSQLBuilder.GetTableName(storageType);
				var systemColumns = SharedSQLBuilder.GetSystemTimeAndUserColumnsInOrder(storageType);
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

		protected TServerChild[] CreateChildServerData(object fkValue)
		{
			var serverChildData = new List<TServerChild>();
			var childStorageData = Helper.GetStorageData(conn, string.Format("SELECT * FROM {0} WHERE {1} = '{2}'",
				SharedSQLBuilder.GetTableName<TStorageChild>(),
				FKColumnName,
				fkValue));

			if (childStorageData.Rows.Count > 0)
			{
				for (int i = 0; i < childStorageData.Rows.Count; i++)
				{
					serverChildData.Add(CreateChildServerData(childStorageData, i));
				}
			}
			return serverChildData.ToArray();
		}

		protected virtual string ParentRelationshipColumn { get { return Helper.GetPKColumn<TStorage>(); } }

		protected virtual TServerChild CreateChildServerData(DataTable childStorageData, int rowNo)
		{
			return Helper.CreateServerData<TServerChild>(childStorageData, rowNo);
		}

		protected virtual TServer CreateServerData(DataTable storageData)
		{
			return Helper.CreateServerData<TServer>(storageData);
		}

		protected abstract void SetServerChild(TServer serverData, TServerChild[] serverChildData);

		protected abstract IDataSetUpdater GetUpdater(IServerProxy proxy);

		protected virtual string FKColumnName
		{
			get { return SharedSQLBuilder.GetFKColumns(typeof(TStorage), typeof(TStorageChild)).FirstOrDefault(); }
		}

		protected abstract string StringColumnForUpdate { get; }
		protected abstract string ChildStringColumnForUpdate { get; }
		protected virtual string ChildStringValueForUpdate => "XX";

		protected virtual void PrepareData() { }

		protected IDbConnection conn;
		protected IDBHelper dbHelper;
		protected IRefVersionControlManager versionControlManager;

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
