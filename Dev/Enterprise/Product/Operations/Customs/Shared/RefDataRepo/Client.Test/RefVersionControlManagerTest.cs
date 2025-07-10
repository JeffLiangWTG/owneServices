using System;
using System.Data;
using System.Text.Json;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using CargoWise.RefDbRepo.Common.Contract_0_9;
using NUnit.Framework;
using static CargoWise.RefDbRepo.Client.Common.RefVersionControlManager;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	class RefVersionControlManagerTest : TransactionedTestCase
	{
		public void TestResetTimestamp()
		{
			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", DateTime.UtcNow, string.Empty, 1), null);
			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RefDbVersionalControl"));
			versionControlManager.ResetLastUpdatedUTC("RefCusTariff", Transaction);
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RefDbVersionalControl"));
		}

		public void TestDataSetGet()
		{
			var now = DateTime.UtcNow;
			var dataSetGet = new DataSetGet(null, now, DataContract.Version, null) { Checkpoint = "AAAA", Chunksize = 1000 };
			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", null, JsonSerializer.Serialize(dataSetGet), 1), null);
			var oldRefDbVersionControl = versionControlManager.GetVersionControl("RefCusTariff", Transaction);
			dataSetGet = oldRefDbVersionControl?.DataSetGet;
			AssertNull(dataSetGet.LowerTimestamp);
			AssertEquals(now, dataSetGet.UpperTimestamp);
			AssertEquals("AAAA", dataSetGet.Checkpoint);
			AssertEquals(1000, dataSetGet.Chunksize);
			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", null, string.Empty, 1), oldRefDbVersionControl);
			dataSetGet = versionControlManager.GetVersionControl("RefCusTariff", Transaction)?.DataSetGet;
			AssertNull(dataSetGet);
		}

		public void TestGetLastUpdatedUTC()
		{
			AssertNull(versionControlManager.GetVersionControl("RefCusTariff", Transaction)?.LastUpdatedUTC);
			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", null, string.Empty, 1), null);
			var oldRefDbVersionControl = versionControlManager.GetVersionControl("RefCusTariff", Transaction);
			AssertNull(oldRefDbVersionControl?.LastUpdatedUTC);
			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", new DateTime(2016, 09, 12, 23, 26, 12), string.Empty, 1), oldRefDbVersionControl);
			var version = versionControlManager.GetVersionControl("RefCusTariff", Transaction);
			AssertEquals(new DateTime(2016, 09, 12, 23, 26, 12), version.LastUpdatedUTC);
		}

		public void TestResetAll()
		{
			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", DateTime.UtcNow, string.Empty, 1), null);
			AssertEquals(1, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RefDbVersionalControl"));
			versionControlManager.ResetAll(Transaction);
			AssertEquals(0, TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM dbo.RefDbVersionalControl"));
		}

		public void TestGetVersionControlUpdaterVersion()
		{
			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", null, string.Empty, 1), null);
			var oldRefDbVersionControl = versionControlManager.GetVersionControl("RefCusTariff", Transaction);
			AssertEquals(1, oldRefDbVersionControl.UpdaterVersion);

			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", null, string.Empty, 2), oldRefDbVersionControl);
			AssertEquals(2, versionControlManager.GetVersionControl("RefCusTariff", Transaction).UpdaterVersion);
		}

		public void TestInMemoryCacheCanGetAndSave()
		{
			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", null, string.Empty, 1), null);
			var oldRefVersionControl = versionControlManager.GetVersionControl("RefCusTariff", Transaction);
			AssertEquals(1, oldRefVersionControl.UpdaterVersion);

			var dataSetGet = new DataSetGet(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, "1", null);

			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", null, JsonSerializer.Serialize(dataSetGet), 1), oldRefVersionControl);
			oldRefVersionControl = versionControlManager.GetVersionControl("RefCusTariff", Transaction);
			AssertEquals(1, oldRefVersionControl.UpdaterVersion);
			AssertEquals(dataSetGet.LowerTimestamp, oldRefVersionControl.DataSetGet.LowerTimestamp);
			AssertEquals(dataSetGet.UpperTimestamp, oldRefVersionControl.DataSetGet.UpperTimestamp);
			AssertEquals(dataSetGet.Version, oldRefVersionControl.DataSetGet.Version);

			var dateTime = DateTime.UtcNow;
			versionControlManager.SaveVersionControl(((IDbConnectionInternals)TestConnection).ADOTransaction, new RefDbVersionControl("RefCusTariff", dateTime, string.Empty, 2), oldRefVersionControl);
			oldRefVersionControl = versionControlManager.GetVersionControl("RefCusTariff", Transaction);
			AssertEquals(2, oldRefVersionControl.UpdaterVersion);
			AssertEquals(dateTime, oldRefVersionControl.LastUpdatedUTC);

			versionControlManager.ResetLastUpdatedUTC("RefCusTariff", ((IDbConnectionInternals)TestConnection).ADOTransaction);
			AssertNull(versionControlManager.GetVersionControl("RefCusTariff", Transaction));

			versionControlManager.SaveVersionControl(Transaction, new RefDbVersionControl("RefCusTariff", dateTime, string.Empty, 2), null);
			versionControlManager.ResetAll(Transaction);
			AssertNull(versionControlManager.GetVersionControl("RefCusTariff", Transaction));
		}

		public void TestOptimisticConcurrencyControlForRefDbVersionControl()
		{
			var lastUpdatedUTC = new DateTime(2017, 1, 1);
			var dataSetGet = new DataSetGet(new DateTime(2017, 1, 1), new DateTime(2017, 5, 1), "version1", "RefCusTariff");
			var updaterVersion = 1;

			var refDbVersionControl = new RefDbVersionControl("RefCusTariff", lastUpdatedUTC, JsonSerializer.Serialize(dataSetGet), updaterVersion);

			versionControlManager.SaveVersionControl(Transaction, refDbVersionControl, null);

			var sql = "UPDATE RefDbVersionalControl SET RVC_LastUpdatedUtc = '2018-01-01' WHERE RVC_DataSet='RefCusTariff'";

			TestConnection.ExecuteNonQuery(sql);

			var newRefDbVersionControl = new RefDbVersionControl("RefCusTariff", lastUpdatedUTC, JsonSerializer.Serialize(dataSetGet), 2);

			AssertExceptionThrown(typeof(OptimisticConcurrencyViolationException), delegate
			{ versionControlManager.SaveVersionControl(Transaction, newRefDbVersionControl, refDbVersionControl); });
		}

		IDBHelper dbHelper;
		IRefVersionControlManager versionControlManager;
		protected override void SetUp()
		{
			dbHelper = new DBHelper(((IDbConnectionInternals)TestConnection).ADOConnection);
			versionControlManager = new RefVersionControlManager(dbHelper);
			versionControlManager.IsMainDb = true;
			TestConnection.ExecuteNonQuery(@"IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RefDbVersionalControl')
	DELETE FROM RefDbVersionalControl");
		}

		IDbTransaction Transaction => ((IDbConnectionInternals)TestConnection).ADOTransaction;

		protected override void TearDown()
		{
			base.TearDown();
			TestConnection.ExecuteNonQuery(@"IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'RefDbVersionalControl')
	DELETE FROM RefDbVersionalControl");
		}
	}
}
