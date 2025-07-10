using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common.Models;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	abstract class TwoZZTablesDataSetUpdaterTest<TServer, TServerChild, TStorage, TStorageChild> : TwoTablesDataSetUpdaterTest<TServer, TServerChild, TStorage, TStorageChild>
		where TServer : RefDataSet
		where TStorage : class, IDataSetStorage
	{
		protected override void PrepareData()
		{
			RefDataHelper.PopulateDummyTariffData(conn);
			RefDataHelper.PopulateDummyCusCodeData(conn);
		}

		protected override void SetUp()
		{
			dbConnection = Db.NewAdminConnection();
			conn = ((IDbConnectionInternals)dbConnection).ADOConnection;
			dbHelper = new DBHelper(conn);
			versionControlManager = new RefVersionControlManager(dbHelper);
			versionControlManager.IsMainDb = true;
			conn.ExecuteNonQuery(@"
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = N'RefDbVersionControl')
DELETE FROM RefDbVersionControl");
		}

		DbConnection dbConnection;

		protected override void TearDown()
		{
			dbConnection?.Dispose();
			base.TearDown();
		}
	}
}
