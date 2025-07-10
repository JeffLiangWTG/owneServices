using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Client.Common.TestHelper;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using CargoWise.RefDbRepo.Common.Models;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	[UseSnapshotProtection]
	abstract class DSUpdaterWithLanguageTextTest<TServer, TStorage> : DSUpdaterTest<TServer, TStorage>
		where TServer : RefDataSet
		where TStorage : class, IDataSetStorage
	{
		public void TestDoNotUpdateNonSystemRecords()
		{
			PrepareDatabase();
			var serverData = GetServerData();
			var proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));

			var storageData = Helper.GetStorageDataHavingAtLeastOneChild<TStorage, IRefLanguageText>(conn, nameof(IRefLanguageText.RLT_ParentId), string.Empty);
			var serverChildData = CreateChildServerData(storageData.Rows[0][SharedSQLBuilder.GetPKColumn<TStorage>()]);

			conn.ExecuteNonQuery($@"
UPDATE t
SET t.RLT_IsSystem = 0
FROM {SharedSQLBuilder.GetTableName<IRefLanguageText>()} t
JOIN {SharedSQLBuilder.GetTableName<TStorage>()} ON {SharedSQLBuilder.GetPKColumn<TStorage>()} = t.{nameof(IRefLanguageText.RLT_ParentId)}
WHERE
{SharedSQLBuilder.GetPKColumn<TStorage>()} = '{storageData.Rows[0][0]}'");

			typeof(RefLanguageText).GetProperty(nameof(IRefLanguageText.RLT_Text)).SetValue(serverChildData[0], "Updating");

			SetServerChild(serverData, serverChildData);

			proxy = Helper.GetServerProxy(serverData);
			Helper.RunUpdater(GetUpdater(proxy));

			AssertNotEquals("Updating", conn.ExecuteScalar($@"
SELECT {nameof(IRefLanguageText.RLT_Text)}
FROM {SharedSQLBuilder.GetTableName<IRefLanguageText>()}
WHERE {nameof(IRefLanguageText.RLT_ParentId)} = '{storageData.Rows[0][0]}'
"));
		}

		protected abstract void SetServerChild(TServer serverData, RefLanguageText[] serverChildData);

		protected RefLanguageText[] CreateChildServerData(object fkValue)
		{
			var serverChildData = new List<RefLanguageText>();
			var childStorageData = Helper.GetStorageData(conn, string.Format("SELECT * FROM {0} WHERE {1} = '{2}'",
				SharedSQLBuilder.GetTableName<IRefLanguageText>(),
				nameof(IRefLanguageText.RLT_ParentId),
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

		protected RefLanguageText CreateChildServerData(DataTable childStorageData, int rowNo)
		{
			return Helper.CreateServerData<RefLanguageText>(childStorageData, rowNo);
		}

		public override void TestDeleteDependentRecord()
		{
			Assert(true);
		}

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
			using (var mainConn = Db.NewAdminConnection())
			{
				mainConn.ExecuteNonQuery($@"
		IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RefDbVersionalControl')
			DELETE FROM dbo.RefDbVersionalControl");
			}
		}

		public virtual string ValueForUpdate => "XX";
	}
}
