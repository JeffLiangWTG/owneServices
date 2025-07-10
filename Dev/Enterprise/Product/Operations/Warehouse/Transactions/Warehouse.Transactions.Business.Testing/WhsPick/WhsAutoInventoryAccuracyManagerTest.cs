using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Moq;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAutoInventoryAccuracyManagerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsAutoInventoryAccuracyManager(null, Mock.Of<IWhsAutoStocktakeTaskCreator>()));
			AssertExceptionThrown<ArgumentNullException>(() => new WhsAutoInventoryAccuracyManager(Mock.Of<IWhsAutoCycleCountTaskCreator>(), null));
		}

		public void TestCreateAutoInventoryAccuracyManagementTasks_CycleCounting()
		{
			TestCreateAutoInventoryAccuracyManagementTasksCore("WCC");
		}

		public void TestCreateAutoInventoryAccuracyManagementTasks_Stocktake()
		{
			TestCreateAutoInventoryAccuracyManagementTasksCore("WST");
		}

		void TestCreateAutoInventoryAccuracyManagementTasksCore(string inventoryAccuractyManagementMethod)
		{
			using (WarehouseDataRegistry.Instance.InventoryAccuracyManagementMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inventoryAccuractyManagementMethod))
			{
				var autoStocktakeCreatorMock = new Mock<IWhsAutoStocktakeTaskCreator>();
				var autoCycleCountCreatorMock = new Mock<IWhsAutoCycleCountTaskCreator>();
				var autoInventoryAccuracyManager = new WhsAutoInventoryAccuracyManager(autoCycleCountCreatorMock.Object, autoStocktakeCreatorMock.Object);

				var data = new TestDataSimpleEnvironment(Factory);
				data.Whs1.WW_VerifyEmptyLocations = true;
				var location = data.Whs1.FindLocation("A");
				location.WLV_MaximumPickCountBeforeAutomatedStocktake = 2;
				location.WLV_FinalisedPickCount = 2;

				var receiveForOrg1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				Helper.CreateWhsReceiveInventoryLine(receiveForOrg1, data.Part1, 1m, location);
				receiveForOrg1.FinaliseDocket();
				Factory.Save();

				var orderForOrg1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
				Helper.CreateWhsOrderLine(orderForOrg1, data.Part1, 1m);

				var finalisedPick = Helper.CreatePickNew(orderForOrg1);
				var orderedInventories = finalisedPick.OrderedInventories;
				var pickAvailableInventory = orderedInventories[0].AvailableInventories[0];
				pickAvailableInventory.VerifiedNonEmpty = true;

				autoInventoryAccuracyManager.CreateAutoInventoryAccuracyManagementTasks(new[] { pickAvailableInventory }, data.Whs1);
				if (inventoryAccuractyManagementMethod.Equals("WCC"))
				{
					autoCycleCountCreatorMock.Verify(mock => mock.BeforeAutoTasksCreation(It.IsAny<IReadOnlyCollection<WhsPickAvailableInventory>>(), Factory));
					autoCycleCountCreatorMock.Verify(mock => mock.CreateInventoryAccuracyManagementTasksForAutoTouchCount(It.IsAny<IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>>>(), data.Whs1));
					autoCycleCountCreatorMock.Verify(mock => mock.CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation(It.IsAny<IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>>>(), data.Whs1));
					autoCycleCountCreatorMock.VerifyAll();
				}
				else
				{
					autoStocktakeCreatorMock.Verify(mock => mock.BeforeAutoTasksCreation(It.IsAny<IReadOnlyCollection<WhsPickAvailableInventory>>(), Factory));
					autoStocktakeCreatorMock.Verify(mock => mock.CreateInventoryAccuracyManagementTasksForAutoTouchCount(It.IsAny<IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>>>(), data.Whs1));
					autoStocktakeCreatorMock.Verify(mock => mock.CreateAutoInventoryAccuracyManagementTasksForAutoZeroConfirmation(It.IsAny<IEnumerable<IGrouping<WhsLocation, WhsPickAvailableInventory>>>(), data.Whs1));
					autoStocktakeCreatorMock.VerifyAll();
				}
			}
		}

		public void TestClearAutoCreatedTasks_CycleCount()
		{
			TestClearAutoCreatedTasksCore("WCC", requireRollback: true);
			TestClearAutoCreatedTasksCore("WCC", requireRollback: false);
		}

		public void TestClearAutoCreatedTasks_Stocktake()
		{
			TestClearAutoCreatedTasksCore("WST", requireRollback: true);
			TestClearAutoCreatedTasksCore("WST", requireRollback: false);
		}

		void TestClearAutoCreatedTasksCore(string inventoryAccuractyManagementMethod, bool requireRollback)
		{
			Assert(true);
			using (WarehouseDataRegistry.Instance.InventoryAccuracyManagementMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inventoryAccuractyManagementMethod))
			{
				var autoStocktakeCreatorMock = new Mock<IWhsAutoStocktakeTaskCreator>();
				var autoCycleCountCreatorMock = new Mock<IWhsAutoCycleCountTaskCreator>();
				var autoInventoryAccuracyManager = new WhsAutoInventoryAccuracyManager(autoCycleCountCreatorMock.Object, autoStocktakeCreatorMock.Object);

				autoInventoryAccuracyManager.ClearAutoCreatedTasks(requireRollback);
				if (inventoryAccuractyManagementMethod.Equals("WCC"))
				{
					autoCycleCountCreatorMock.Verify(mock => mock.ClearAutoCreatedTasks(requireRollback));
					autoCycleCountCreatorMock.VerifyAll();
				}
				else
				{
					autoStocktakeCreatorMock.Verify(mock => mock.ClearAutoCreatedTasks(requireRollback));
					autoStocktakeCreatorMock.VerifyAll();
				}
			}
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;
	}
}
