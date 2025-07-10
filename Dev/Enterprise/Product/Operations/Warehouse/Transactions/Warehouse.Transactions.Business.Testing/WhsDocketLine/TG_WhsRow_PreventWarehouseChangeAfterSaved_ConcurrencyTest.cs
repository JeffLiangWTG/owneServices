using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using WhsRowDO = CargoWise.Database.TestFramework.ObjectModel.WhsRow;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[UseSnapshotProtection]
	class TG_WhsRow_PreventWarehouseChangeAfterSaved_ConcurrencyTest : TG_WhsDocketLine_ConcurrencyTest
	{
		#region TestUpdatingWarehouseInRow_ConcurrencyCase_CommitLineFirst

		[ExpectNoExceptions]
		public void TestUpdatingWarehouseInRow_ConcurrencyCase_CommitLineFirst()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				DisablePageLocks(connection1);

				var factory1 = new BusinessObjectFactory(connection1) { RefreshEnabled = false };
				var helper = new WhsTestHelperFunctions(factory1);
				var data = new TestDataSimpleEnvironment(factory1, 2, 1);
				var whs2 = helper.CreateWarehouse("WH2", "B");
				var locationWhs1 = data.Whs1.DefaultLocation;
				var row = locationWhs1.Row;
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
				factory1.Save();

				var sqlDocket = CargoWise.Database.TestFramework.ObjectModel.WhsDocket
					.ShallowLoadFromDB(Db.Connection, w => w.PK == receive.PK)[0];
				var quantity = 10m;

				connection1.BeginTransaction();
				AssertNoExceptionThrown(() =>
					new CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine(sqlDocket, data.Part1.PK.ToGuid(), quantity,
						locationWhs1.PK.ToGuid())
					{
						WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway,
						WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway,
						WE_SystemCreateUser = "~BP",
						WE_SystemLastEditUser = "~BP",
						WE_StockOnHand = quantity
					}.Insert(connection1));

				AssertNoExceptionThrown(factory1.Save);

				var task = new Task(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection2 = Db.NewExtraConnectionToMainDb())
					{
						var factory2 = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
						var rowInFactory2 = factory2.Load<Environment.Business.WhsRow>(row.PK);
						var loadWhs2 =
							CargoWise.Database.TestFramework.ObjectModel.WhsWarehouse.ShallowLoadFromDB(connection2, whs2.PK.ToGuid());

						connection2.BeginTransaction();

						NUnit.Framework.Assert.That(delegate
						{
							WhsRowDO.UpdateWhere(row.PK.ToGuid()).Set(w => w.WR_WW_Whs, loadWhs2)
								.Post(connection2);
						}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change the Warehouse for a Row that is already saved.", true), "Trigger should prevent changing the Warehouse in a Row that is already saved.");
					}
				});

				task.Start();
				task.Wait(5000); // Should wait as the other transaction has a lock
				connection1.CommitTransaction();
				AssertEquals("Expected line to exist in DB", locationWhs1,
					factory1.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, receive.PK)).Single().Location);
			}
		}

		#endregion

		#region TestUpdatingWarehouseInRow_ConcurrencyCase_CommitWarehouseChangeFirst

		[ExpectNoExceptions]
		public void TestUpdatingWarehouseInRow_ConcurrencyCase_CommitWarehouseChangeFirst()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				DisablePageLocks(connection1);

				var factory1 = new BusinessObjectFactory(connection1) { RefreshEnabled = false };
				var helper = new WhsTestHelperFunctions(factory1);
				var data = new TestDataSimpleEnvironment(factory1, 2, 1);
				var whs2 = helper.CreateWarehouse("WH2", "B");
				var locationWhs1 = data.Whs1.DefaultLocation;
				var row = locationWhs1.Row;
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
				factory1.Save();

				connection1.BeginTransaction();
				var loadWhs2 = CargoWise.Database.TestFramework.ObjectModel.WhsWarehouse.ShallowLoadFromDB(connection1, whs2.PK.ToGuid());
				NUnit.Framework.Assert.That(
					delegate
					{
						WhsRowDO.UpdateWhere(row.PK.ToGuid()).Set(w => w.WR_WW_Whs, loadWhs2).Post(connection1);
					}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change the Warehouse for a Row that is already saved.", true), "Trigger should prevent changing the Warehouse in a Row that is already saved.");

				var sqlDocket = CargoWise.Database.TestFramework.ObjectModel.WhsDocket
					.ShallowLoadFromDB(Db.Connection, w => w.PK == receive.PK)[0];
				var quantity = 10m;

				using (Db.DisposableActionForDbConnection())
				using (var connection2 = Db.NewExtraConnectionToMainDb())
				{
					connection2.BeginTransaction();

					AssertNoExceptionThrown(() =>
						new CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine(sqlDocket, data.Part1.PK.ToGuid(), quantity,
							locationWhs1.PK.ToGuid())
						{
							WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway,
							WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway,
							WE_SystemCreateUser = "~BP",
							WE_SystemLastEditUser = "~BP",
							WE_StockOnHand = quantity
						}.Insert(connection2));

					connection2.CommitTransaction();
				}
			}
		}

		#endregion
	}
}
