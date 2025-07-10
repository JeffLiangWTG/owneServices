using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using WhsLocationDO = CargoWise.Database.TestFramework.ObjectModel.WhsLocation;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[UseSnapshotProtection]
	class TG_PreventInvalidConcurrencyChangeTest : TG_WhsDocketLine_ConcurrencyTest
	{
		#region TestUpdatingLocationRow_ConcurrencyCase_CommitLineFirst

		[ExpectNoExceptions]
		public void TestUpdatingLocationRow_ConcurrencyCase_CommitLineFirst()
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
				var row1 = locationWhs1.Row;
				var row2 = whs2.DefaultLocation.Row;
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
				row2.Locations.DeleteAll(); // Prevent duplication errors when assigning location in WH1 to this Row.
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

				var locationWhs1PK = data.Whs1.DefaultLocation.PK;
				var task = new Task(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection2 = Db.NewExtraConnectionToMainDb())
					{
						var factory2 = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
						var locationInFactory2 =
							factory2.Load<Environment.Business.WhsLocation>(locationWhs1PK);

						connection2.BeginTransaction();

						NUnit.Framework.Assert.That(delegate
						{
							WhsLocationDO.UpdateWhere(locationInFactory2.PK.ToGuid())
								.Set(w => w.WL_WR, row2.PK.ToGuid()).Post(connection2);
						}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change the Row for a Location that is already saved.", true), "Trigger should prevent changing the Row in a Location.");
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

		#region TestUpdatingLocationRow_ConcurrencyCase_CommitRowChangeFirst

		[ExpectNoExceptions]
		public void TestUpdatingLocationRow_ConcurrencyCase_CommitRowChangeFirst()
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
				var row1 = locationWhs1.Row;
				var row2 = whs2.DefaultLocation.Row;
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
				row2.Locations.DeleteAll(); // Prevent duplication errors when assigning location in WH1 to this Row.
				factory1.Save();

				connection1.BeginTransaction();
				NUnit.Framework.Assert.That(delegate
				{
					WhsLocationDO.UpdateWhere(locationWhs1.PK.ToGuid()).Set(w => w.WL_WR, row2.PK.ToGuid())
						.Post(connection1);
				}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change the Row for a Location that is already saved.", true), "Trigger should prevent changing the Row in a Location.");

				var sqlDocket = CargoWise.Database.TestFramework.ObjectModel.WhsDocket
					.ShallowLoadFromDB(Db.Connection, w => w.PK == receive.PK)[0];
				var quantity = 10m;

				using (Db.DisposableActionForDbConnection())
				using (var connection2 = Db.NewExtraConnectionToMainDb())
				{
					var factory2 = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
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

		#region TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Concurrency_AttachOrderToPickThenChangeOrderLine

		[ExpectNoExceptions]
		public void
			TestChangeOrderLinesCriticalFieldsWhenOrderIsPicked_Concurrency_AttachOrderToPickThenChangeOrderLine()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				DisablePageLocks(connection1);

				var factory1 = new BusinessObjectFactory(connection1) { RefreshEnabled = false };
				var helper = new WhsTestHelperFunctions(factory1);
				var data = new TestDataSimpleEnvironment(factory1, 2, 1);
				var whs2 = helper.CreateWarehouse("WH2", "B");
				var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
				factory1.Save();

				connection1.BeginTransaction();
				helper.CreatePickNew(order);
				AssertNoExceptionThrown(factory1.Save);

				var codeAfterUpdateDocketline = false;
				var task = new Task(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection2 = Db.NewExtraConnectionToMainDb())
					{
						var factory2 = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
						var orderLineInFactory2 = factory2.Load<WhsOrderLine>(order.Lines[0].PK);
						orderLineInFactory2.WE_PartAttrib1 = "test";

						connection2.BeginTransaction();

						NUnit.Framework.Assert.That(factory2.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change critical fields on picked order.", true), "Should not be able to change line if order is picked.");
						codeAfterUpdateDocketline = true;
					}
				});

				task.Start();
				task.Wait(5000); // Should wait as the other transaction has a lock
				AssertEquals("Lock should not let process to continue", false, codeAfterUpdateDocketline);
				connection1.CommitTransaction();
				task.Wait();
				AssertEquals("After releasing lock it should continue process.", true, codeAfterUpdateDocketline);
			}
		}

		#endregion
	}
}
