using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;
using WTG.NUnit;
using WhsDocketDO = CargoWise.Database.TestFramework.ObjectModel.WhsDocket;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[UseSnapshotProtection]
	class TG_WhsDocketLine_LocationIsInCorrectWarehouse_ConcurrencyTest : TG_WhsDocketLine_ConcurrencyTest
	{
		#region TestUpdatingLocationFromAnotherWarehouse_ConcurrencyCase_CommitLineFirst

		[ExpectNoExceptions]
		public void TestUpdatingLocationFromAnotherWarehouse_ConcurrencyCase_CommitLineFirst()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				DisablePageLocks(connection1);

				var factory1 = new BusinessObjectFactory(connection1) { RefreshEnabled = false };
				var helper = new WhsTestHelperFunctions(factory1);
				var data = new TestDataSimpleEnvironment(factory1, 2, 1);

				var otherWhs = helper.CreateWarehouse("W2", "A");
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
				factory1.Save();

				var sqlDocket = WhsDocketDO.ShallowLoadFromDB(Db.Connection, w => w.PK == receive.PK)[0];
				var quantity = 10m;

				connection1.BeginTransaction();
				AssertNoExceptionThrown(() =>
					new CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine(sqlDocket, data.Part1.PK.ToGuid(), quantity,
						data.Whs1.DefaultLocation.PK.ToGuid())
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
						var receiveInFactory2 = factory2.Load<WhsDocket>(receive.PK);
						receiveInFactory2.WD_WW_Whs = otherWhs.PK;

						connection2.BeginTransaction();

						// Some triggers may be deferred in production, this test should defer them to ensure (something else) is always locking as expected
						SuspendDeferrableTriggers(connection2);

						NUnit.Framework.Assert.That(delegate
						{
							WhsDocketDO.UpdateWhere(receive.PK.ToGuid())
								.Set(w => w.WD_WW_Whs, otherWhs.PK.ToGuid()).Post(connection2);
						}, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Attempt to change docket subtype or warehouse for a job with captured lines.", true), "Trigger should prevent saving a location for a warehouse different to the warehouse on the docket.");
					}
				});

				task.Start();
				task.Wait(5000); // Should wait as the other transaction has a lock
				connection1.CommitTransaction();
				task.Wait();
			}
		}

		#endregion

		#region TestUpdatingLocationFromAnotherWarehouse_ConcurrencyCase_CommitWarehouseChangeFirst

		[ExpectNoExceptions]
		public void TestUpdatingLocationFromAnotherWarehouse_ConcurrencyCase_CommitWarehouseChangeFirst()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				DisablePageLocks(connection1);

				var factory1 = new BusinessObjectFactory(connection1) { RefreshEnabled = false };
				var helper = new WhsTestHelperFunctions(factory1);
				var data = new TestDataSimpleEnvironment(factory1, 2, 1);

				var otherWhs = helper.CreateWarehouse("W2", "A");
				var receive = helper.CreateWhsReceive(data.Org1, data.Whs1);
				factory1.Save();

				var sqlDocket = CargoWise.Database.TestFramework.ObjectModel.WhsDocket
					.ShallowLoadFromDB(Db.Connection, w => w.PK == receive.PK)[0];
				var quantity = 15m;

				connection1.BeginTransaction();
				AssertNoExceptionThrown(() =>
					WhsDocketDO.UpdateWhere(receive.PK.ToGuid()).Set(w => w.WD_WW_Whs, otherWhs.PK.ToGuid())
						.Post(connection1));

				var orgSupplierPart1PK = data.Part1.PK.ToGuid();
				var whs1DefaultLocationPK = data.Whs1.DefaultLocation.PK.ToGuid();
				var task = new Task(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection2 = Db.NewExtraConnectionToMainDb())
					{
						connection2.BeginTransaction();

						// Some triggers may be deferred in production, this test should defer them to ensure (something else) is always locking as expected
						SuspendDeferrableTriggers(connection2);

						NUnit.Framework.Assert.That(() => new CargoWise.Database.TestFramework.ObjectModel.WhsDocketLine(sqlDocket, orgSupplierPart1PK,
								quantity, whs1DefaultLocationPK)
							{
								WE_CurrentInventoryStatus = InventoryStatus.Codes.Putaway,
								WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway,
								WE_SystemCreateUser = "~BP",
								WE_SystemLastEditUser = "~BP",
								WE_StockOnHand = quantity
							}.Insert(connection2), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "Locations should be in the warehouse specified on the parent Docket.", true), "Trigger should prevent saving a location for a warehouse different to the warehouse on the docket.");
					}
				});

				task.Start();
				task.Wait(5000); // Should wait as the other transaction has a lock
				connection1.CommitTransaction();
				task.Wait();
			}
		}

		#endregion

		#region TestUpdatingLocationFromAnotherWarehouse_ConcurrencyCase_CommitWarehouseChangeFirst_WE_WL_TransferFrom

		public void
			TestUpdatingLocationFromAnotherWarehouse_ConcurrencyCase_CommitWarehouseChangeFirst_WE_WL_TransferFrom()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection1 = Db.NewExtraConnectionToMainDb())
			{
				DisablePageLocks(connection1);

				var factory1 = new BusinessObjectFactory(connection1) { RefreshEnabled = false };
				var helper1 = new WhsTestHelperFunctions(factory1);
				var data = new TestDataSimpleEnvironment(factory1, 2, 1);
				var whs2 = helper1.CreateWarehouse("W2", "A");
				var whs3 = helper1.CreateWarehouse("W3", "A");
				factory1.Save();

				var receive = helper1.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
				factory1.Save();

				var transfer = helper1.CreateWhsTransfer(data.Org1, data.Whs1, "T1",
					transferType: TransferType.Codes.InterWhsSource);
				factory1.Save();

				connection1.BeginTransaction();
				AssertNoExceptionThrown(() =>
					WhsDocketDO.UpdateWhere(transfer.PK.ToGuid()).Set(w => w.WD_WW_Whs, whs3.PK.ToGuid())
						.Post(connection1));

				var task = new Task(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection2 = Db.NewExtraConnectionToMainDb())
					{
						var factory2 = new BusinessObjectFactory(connection2) { RefreshEnabled = false };
						var helper2 = new WhsTestHelperFunctions(factory2);

						var transferInFactory2 = factory2.Load<WhsTransfer>(transfer.PK);
						var transferLine =
							helper2.CreateWhsTransferLine(transferInFactory2, data.Part1, 10m, "A-1", whs2.PK, "A");
						transferLine.RunPreSaveValidation();
						AssertEquals("Precondition.", 10m, transferLine.QtyCommittedIncludingMatchingLines);

						connection2.BeginTransaction();
						// Some triggers may be deferred in production, this test should defer them to ensure (something else) is always locking as expected
						SuspendDeferrableTriggers(connection2);

						AssertExceptionThrown("Concurrency error should be raised after modifying Warehouse location", typeof(ZSaveConcurrencyException), () => factory2.Save());
					}
				});

				task.Start();
				task.Wait(5000); // Should wait as the other transaction has a lock
				connection1.CommitTransaction();
				task.Wait();
			}
		}

		#endregion

		#region SuspendDeferrableTriggers

		void SuspendDeferrableTriggers(DbConnection connection)
		{
			connection.ExecuteNonQuery(@"
				DECLARE @sql VARCHAR(MAX)
				SET @SQL = ''

				SELECT @sql = @sql + 'EXEC dbo.SuspendTrigger ''' + TriggerName + '''
				'
				FROM
					dbo.SuspendedTriggers

				EXEC(@sql)");
		}

		#endregion
	}
}
