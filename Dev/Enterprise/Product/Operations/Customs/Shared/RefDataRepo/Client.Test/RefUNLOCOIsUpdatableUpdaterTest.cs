using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Client.SqlServer;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[UseSnapshotProtection]
	class RefUNLOCOIsUpdatableUpdaterTest : TestCase
	{
		protected string StringColumnForUpdate => nameof(IRefUNLOCO.RL_PortName);
		protected string FKColumnName => nameof(IRefLocoMap.RY_RL_NKLocoPort);

		protected IDataSetUpdater GetUpdater(IServerProxy proxy)
		{
			return new RefUNLOCOUpdater(proxy, dbHelper, versionControlManager, new SqlServerSQLBuilder(), new ValueConverter());
		}

		protected void SetServerChild(RefUNLOCO serverData, RefLocoMap[] serverChildData)
		{
			serverData.RefLocoMaps = serverChildData;
		}

		protected RefUNLOCO CreateServerData(DataTable storageData)
		{
			return Helper.CreateServerData<RefUNLOCO>(storageData);
		}

		protected RefLocoMap[] CreateChildServerData(object fkValue)
		{
			var serverChildData = new List<RefLocoMap>();
			var childStorageData = Helper.GetStorageData(conn, string.Format("SELECT * FROM {0} WHERE {1} = '{2}'",
				SharedSQLBuilder.GetTableName<IRefLocoMap>(),
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

		protected RefLocoMap CreateChildServerData(DataTable childStorageData, int rowNo)
		{
			return Helper.CreateServerData<RefLocoMap>(childStorageData, rowNo);
		}

		public void TestDoNotUpdateNonIsSystemUpdatableRecords()
		{
			var storageData = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0}", SharedSQLBuilder.GetTableName<IRefUNLOCO>()));
			conn.ExecuteNonQuery($@"UPDATE {SharedSQLBuilder.GetTableName<IRefUNLOCO>()} SET RL_IsUpdatable = 0 
	WHERE {nameof(IRefUNLOCO.RL_PK)} = '{storageData.Rows[0][0]}'");
			var serverData = CreateServerData(storageData);
			typeof(RefUNLOCO).GetProperty(StringColumnForUpdate).SetValue(serverData, "XX");
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			var dataTable = Helper.GetStorageData(conn, string.Format("SELECT TOP 1 * FROM {0} WHERE {1} = '{2}'", SharedSQLBuilder.GetTableName<IRefUNLOCO>(), SharedSQLBuilder.GetPKColumn<IRefUNLOCO>(),
				storageData.Rows[0][SharedSQLBuilder.GetPKColumn<IRefUNLOCO>()]));
			AssertNotEquals("XX", dataTable.Rows[0][StringColumnForUpdate]);
		}

		public void TestInsertLocoMapWhenUNLOCOIsNotUpdatable()
		{
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(SharedSQLBuilder.GetTableName<IRefUNLOCO>(), DateTime.UtcNow.AddDays(-1), string.Empty, 1), null);
			var locoMapPK = Guid.NewGuid();
			conn.ExecuteNonQuery($@"INSERT INTO dbo.RefLocoMap(RY_PK, RY_LocalPortCode, RY_RL_NKLocoPort, RY_RN, RY_SystemUsage)
				VALUES('{locoMapPK}', 'TST', (SELECT TOP 1 RL_Code FROM dbo.RefUNLOCO), (SELECT TOP 1 RN_PK FROM dbo.RefCountry), 'SCK')");

			var storageData = Helper.GetStorageDataHavingAtLeastOneChild<IRefUNLOCO, IRefLocoMap>(conn, nameof(IRefLocoMap.RY_RL_NKLocoPort), nameof(IRefUNLOCO.RL_Code));

			conn.ExecuteNonQuery($@"UPDATE {SharedSQLBuilder.GetTableName<IRefUNLOCO>()} SET RL_IsUpdatable = 0 
				WHERE {nameof(IRefUNLOCO.RL_PK)} = '{storageData.Rows[0][0]}'");

			var serverChildData = CreateChildServerData(storageData.Rows[0][nameof(IRefUNLOCO.RL_Code)]);
			var serverData = CreateServerData(storageData);

			SetServerChild(serverData, serverChildData);

			dbHelper.Delete<IRefLocoMap>();
			AssertEquals(0, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<IRefLocoMap>())));
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertGreaterThanOrEqualTo((int)conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0}", SharedSQLBuilder.GetTableName<IRefLocoMap>())), 1);
		}

		public void TestUpdateLocoMapWhenUNLOCOIsNotUpdatable()
		{
			versionControlManager.SaveVersionControl(null, new RefDbVersionControl(SharedSQLBuilder.GetTableName<IRefUNLOCO>(), DateTime.UtcNow.AddDays(-1), string.Empty, 1), null);
			var locoMapPK = Guid.NewGuid();
			dbHelper.Delete<IRefLocoMap>();
			conn.ExecuteNonQuery($@"INSERT INTO dbo.RefLocoMap(RY_PK, RY_LocalPortCode, RY_RL_NKLocoPort, RY_RN, RY_SystemUsage)
				VALUES('{locoMapPK}', 'TST', (SELECT TOP 1 RL_Code FROM dbo.RefUNLOCO), (SELECT TOP 1 RN_PK FROM dbo.RefCountry), 'SCK')");

			var storageData = Helper.GetStorageDataHavingAtLeastOneChild<IRefUNLOCO, IRefLocoMap>(conn, nameof(IRefLocoMap.RY_RL_NKLocoPort), nameof(IRefUNLOCO.RL_Code));

			conn.ExecuteNonQuery($@"UPDATE {SharedSQLBuilder.GetTableName<IRefUNLOCO>()} SET RL_IsUpdatable = 0 
				WHERE {nameof(IRefUNLOCO.RL_PK)} = '{storageData.Rows[0][0]}'");

			var serverChildData = CreateChildServerData(storageData.Rows[0][nameof(IRefUNLOCO.RL_Code)]);
			var serverData = CreateServerData(storageData);

			SetServerChild(serverData, serverChildData);

			conn.ExecuteNonQuery($@"UPDATE dbo.RefLocoMap SET RY_LocalPortCode = 'XXX'");
			AssertEquals(0, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0} WHERE RY_LocalPortCode = 'TST'", SharedSQLBuilder.GetTableName<IRefLocoMap>())));
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));
			AssertEquals(1, conn.ExecuteScalar(string.Format("SELECT COUNT(*) FROM {0} WHERE RY_LocalPortCode = 'TST'", SharedSQLBuilder.GetTableName<IRefLocoMap>())));
		}

		protected override void SetUp()
		{
			conn = ((IDbConnectionInternals)Db.NewAdminConnection()).ADOConnection;
			dbHelper = new DBHelper(conn);
			versionControlManager = new RefVersionControlManager(dbHelper);
			versionControlManager.IsMainDb = true;
		}

		protected IDbConnection conn;
		protected IDBHelper dbHelper;
		protected IRefVersionControlManager versionControlManager;
	}
}
