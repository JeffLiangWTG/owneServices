using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ProductionRules.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateMultiOrderPickActionMethodApplicator))]
	public class GenerateMultiOrderPickActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GenerateMultiOrderPickActionMethodApplicator(null));
		}

		#endregion

		#region TestActionCreatesPicksPerWarehouse

		public void TestActionCreatesPicksPerWarehouse()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var warehouse2 = Helper.CreateWarehouse("WHS2", "Test Warehouse 2");
			var warehouse3 = Helper.CreateWarehouse("WHS3", "Test Warehouse 3");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product1 = Helper.CreateProduct(client, "P1");

			Helper.CreateWhsReceiveWithInventory(client, warehouse1, "Receive1", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse2, "Receive3", product1, 10m);
			Helper.CreateWhsReceiveWithInventory(client, warehouse3, "Receive4", product1, 10m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "MultiOrderOA1", product1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "MultiOrderOA2", product1, 5m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, warehouse2, "MultiOrderOA3", product1, 5m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(client, warehouse3, "MultiOrderOA4", product1, 5m);
			Factory.Save();

			AssertEquals("Precondition - No existing picks", 0, Factory.Load<WhsPick>(new ZQuery()).Length);
			var expectedLogText = @"INFO: Attempting to Create Pick with 2x Order(s) for Warehouse WHS1
INFO: Created pick [HL P00000001] with 2x Order(s) for Warehouse WHS1.
INFO: Attempting to Create Pick with 1x Order(s) for Warehouse WHS2
INFO: Created pick [HL P00000002] with 1x Order(s) for Warehouse WHS2.
INFO: Attempting to Create Pick with 1x Order(s) for Warehouse WHS3
INFO: Created pick [HL P00000003] with 1x Order(s) for Warehouse WHS3.";

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				ApplyApplicator(new[] { order1, order2, order3, order4 }, expectedLogText);
			}

			var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
			AssertEquals("Only 3 picks created by OA", 3, numberOfPicks);
			AssertEquals("Orders from the same warehouse are on the same pick", order1.WD_WP, order2.WD_WP);
			AssertNotEquals("Orders from different warehouses are on different picks", order1.WD_WP, order3.WD_WP);
			AssertNotEquals("Orders from different warehouses are on different picks", order1.WD_WP, order4.WD_WP);
			AssertNotEquals("Orders from different warehouses are on different picks", order3.WD_WP, order4.WD_WP);

			AssertEquals(PickType.Codes.Order, order1.Pick.WP_PickType);
			AssertEquals(PickType.Codes.Order, order3.Pick.WP_PickType);
			AssertEquals(PickType.Codes.Order, order4.Pick.WP_PickType);
		}

		#endregion

		#region TestActionCreatesPicksPerWarehouseAndPerPickType

		public void TestActionCreatesPicksPerWarehouseAndPerPickType()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Held Order PickTypes only possible with EnableHeldGoodsForOrders
			{
				var warehouse1 = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
				var warehouse2 = Helper.CreateWarehouse("WHS2", "Test Warehouse 2");
				var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
				var product1 = Helper.CreateProduct(client, "P1");

				var receive1 = Helper.CreateWhsReceive(client, warehouse1, "Receive1");
				Helper.CreateWhsReceiveLine(receive1, product1, 20m, warehouse1.DefaultLocation, "", InventoryStatus.Codes.Putaway);
				Helper.CreateWhsReceiveLine(receive1, product1, 20m, warehouse1.DefaultLocation, "", InventoryStatus.Codes.Held, "DAM");
				receive1.FinaliseDocket();

				var receive2 = Helper.CreateWhsReceive(client, warehouse2, "Receive2");
				Helper.CreateWhsReceiveLine(receive2, product1, 20m, warehouse2.DefaultLocation, "", InventoryStatus.Codes.Putaway);
				Helper.CreateWhsReceiveLine(receive2, product1, 20m, warehouse2.DefaultLocation, "", InventoryStatus.Codes.Held, "DAM");
				receive2.FinaliseDocket();

				var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "Order1", product1, 5m);
				var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "Order2", product1, 5m);
				var order3 = Helper.CreateWhsOrderWithOrderLine(client, warehouse2, "Order3", product1, 5m);
				var order4 = Helper.CreateWhsOrderWithOrderLine(client, warehouse2, "Order4", product1, 5m);

				var heldOrder1 = Helper.CreateWhsOrder(client, warehouse1, "HeldOrder1");
				Helper.CreateWhsOrderLine(heldOrder1, product1, 5m).WE_WHC_NKOrderedHeldCode = "DAM";
				var heldOrder2 = Helper.CreateWhsOrder(client, warehouse1, "HeldOrder2");
				Helper.CreateWhsOrderLine(heldOrder2, product1, 5m).WE_WHC_NKOrderedHeldCode = "DAM";
				var heldOrder3 = Helper.CreateWhsOrder(client, warehouse2, "HeldOrder3");
				Helper.CreateWhsOrderLine(heldOrder3, product1, 5m).WE_WHC_NKOrderedHeldCode = "DAM";
				var heldOrder4 = Helper.CreateWhsOrder(client, warehouse2, "HeldOrder4");
				Helper.CreateWhsOrderLine(heldOrder4, product1, 5m).WE_WHC_NKOrderedHeldCode = "DAM";

				Factory.Save();

				AssertEquals("Precondition - No existing picks", 0, Factory.Load<WhsPick>(new ZQuery()).Length);

				var expectedLogText = @"INFO: Attempting to Create Pick with 2x Order(s) for Warehouse WHS1
INFO: Created pick [HL P00000001] with 2x Order(s) for Warehouse WHS1.
INFO: Attempting to Create Pick with 2x Order(s) for Warehouse WHS1
INFO: Created pick [HL P00000002] with 2x Order(s) for Warehouse WHS1.
INFO: Attempting to Create Pick with 2x Order(s) for Warehouse WHS2
INFO: Created pick [HL P00000003] with 2x Order(s) for Warehouse WHS2.
INFO: Attempting to Create Pick with 2x Order(s) for Warehouse WHS2
INFO: Created pick [HL P00000004] with 2x Order(s) for Warehouse WHS2.
";

				using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
				{
					ApplyApplicator(new[] { order1, order2, heldOrder1, heldOrder2, order3, order4, heldOrder3, heldOrder4 }, expectedLogText);
				}

				var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
				AssertEquals("Only 4 picks created by OA", 4, numberOfPicks);
				AssertEquals("Orders from the same warehouse are on the same pick", order1.WD_WP, order2.WD_WP);
				AssertEquals("Orders from the same warehouse are on the same pick", order3.WD_WP, order4.WD_WP);
				AssertEquals("Held Inventory Orders from the same warehouse are on the same pick", heldOrder1.WD_WP, heldOrder2.WD_WP);
				AssertEquals("Held Inventory Orders from the same warehouse are on the same pick", heldOrder3.WD_WP, heldOrder4.WD_WP);

				AssertNotEquals("Orders and Held orders from the same warehouse are on different picks", order1.WD_WP, heldOrder1.WD_WP);
				AssertNotEquals("Orders and Held orders from the same warehouse are on different picks", order3.WD_WP, heldOrder3.WD_WP);

				AssertNotEquals("Orders from different warehouses are on different picks", order1.WD_WP, order3.WD_WP);
				AssertNotEquals("Held Inventory Orders from different warehouses are on different picks", heldOrder1.WD_WP, heldOrder3.WD_WP);

				AssertEquals(PickType.Codes.Order, order1.Pick.WP_PickType);
				AssertEquals(PickType.Codes.Order, order3.Pick.WP_PickType);
				AssertEquals(PickType.Codes.HeldInventoryOrder, heldOrder1.Pick.WP_PickType);
				AssertEquals(PickType.Codes.HeldInventoryOrder, heldOrder3.Pick.WP_PickType);
			}
		}

		#endregion

		#region TestActionOnlyAcceptsValidOrders

		public void TestActionOnlyAcceptsValidOrders()
		{
			var warehouse = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product = Helper.CreateProduct(client, "P1");

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA1", product, 5m); // CAN
			order1.CancelReactivateDocket();
			AssertEquals("Precondition: Order is Cancelled.", DocketStatus.Codes.Cancelled, order1.WD_DocketStatus);
			var order2 = Helper.CreateWhsOrder(client, warehouse, "MultiOrderOA2"); // No Lines
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA3", product, 10m); // No Stock
			var order4 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA4", product, 1m); // Already assigned to Pick
			Helper.CreatePickNew(order4);
			var order5 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA5", product, 1m); // Finalised Order
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: false, pickableDockets: order5);
			var order6 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA6", product, 1m); // Manual Pick Option
			order6.WD_PickOption = WhsPickOption.Codes.Manual;
			var order7 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "MultiOrderOA7", product, 1m); // ManualWithAutoAllocate Pick Option
			order7.WD_PickOption = WhsPickOption.Codes.ManualWithAutoAllocate;

			Factory.Save();
			AssertEquals("Precondition - Two existing picks", 2, Factory.Load<WhsPick>(new ZQuery()).Length);

			// Pick log works, given the precondition above
			string expectedLogText = string.Format(
				"ERROR: Order [HL {0}] is Canceled (CAN).\r\n" +
				"ERROR: Order [HL {1}] has no order lines.\r\n" +
				"ERROR: Order [HL {2}] is Attached To Pick (ATP).\r\n" +
				"ERROR: Order [HL {3}] is Attached To Pick (ATP).\r\n" +
				"ERROR: Order [HL {4}] does not have Pick Option AUT.\r\n" +
				"ERROR: Order [HL {5}] does not have Pick Option AUT.\r\n" +
				"INFO: Attempting to Create Pick with 1x Order(s) for Warehouse WHS1\r\n" +
				"WARNING: Order [HL {6}] could not be allocated stock.\r\n" +
				"WARNING: No Pick was created for Warehouse WHS1.",
				order1.WD_DocketID,
				order2.WD_DocketID,
				order4.WD_DocketID,
				order5.WD_DocketID,
				order6.WD_DocketID,
				order7.WD_DocketID,
				order3.WD_DocketID
			);

			var orders = new WhsOrder[]
			{
				order1,
				order2,
				order3,
				order4,
				order5,
				order6,
				order7,
			};

			ApplyApplicator(orders, expectedLogText);

			var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
			AssertEquals("No new picks created", 2, numberOfPicks);

			AssertEquals("Order should not have pick assigned", ZGuid.Empty, order1.WD_WP);
			AssertEquals("Order should not have pick assigned", ZGuid.Empty, order2.WD_WP);
			AssertEquals("Order should not have pick assigned", ZGuid.Empty, order3.WD_WP);
			AssertEquals("Order should not have pick assigned", ZGuid.Empty, order6.WD_WP);
			AssertEquals("Order should not have pick assigned", ZGuid.Empty, order7.WD_WP);
		}

		public void TestActionOnlyAcceptsValidOrders_AllocatesValidOnes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 1m);
			Factory.Save();

			var expectedLog = @"INFO: Attempting to Create Pick with 2x Order(s) for Warehouse 1
WARNING: Order [HL W00000003] could not be allocated stock.
INFO: Created pick [HL P00000001] with 1x Order(s) for Warehouse 1.";

			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				ApplyApplicator(new[] { order1, order2 }, expectedLog);
			}

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			AssertNotNull(order1InNewFactory.Pick);
			AssertNull(order2InNewFactory.Pick);
		}

		public void TestActionOnlyAcceptsValidOrders_DoesNotThrowException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 0m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 1m);
			Factory.Save();

			var expectedLog = @"INFO: Attempting to Create Pick with 2x Order(s) for Warehouse 1
WARNING: Order [HL W00000001] could not be attached to Pick:
This Order has no Units.
WARNING: Order [HL W00000002] could not be allocated stock.
WARNING: No Pick was created for Warehouse 1.";

			ApplyApplicator(new[] { order1, order2 }, expectedLog);

			var newFactory = new BusinessObjectFactory();
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			AssertNull(order1InNewFactory.Pick);
			AssertNull(order2InNewFactory.Pick);
		}

		public void TestActionOnlyAcceptsValidOrders_OrderMustPassPickability()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			Factory.Save();

			TestConnection.ExecuteNonQuery("DISABLE TRIGGER TG_PreventDeletionDefaultDockDoorLocation ON WhsLocation");
			data.Whs1.DefaultOutboundDockDoorLocation.Delete();
			Factory.Save();

			var expectedLog = @"INFO: Attempting to Create Pick with 1x Order(s) for Warehouse 1
WARNING: Order [HL W00000002] could not be attached to Pick:
Cannot create Pick. Default outbound dock door location is not specified for this warehouse.
WARNING: No Pick was created for Warehouse 1.";

			ApplyApplicator(new[] { order }, expectedLog);
		}

		public void TestActionOnlyAcceptsValidOrders_PICStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "MultiOrderOA1", data.Part1, 5m);
			Helper.CreatePickNew(order1);
			var transferLine = Helper.PickAndMakeInTransitTransfer(order1.Lines[0].PickLines.Single(), ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "MultiOrderOA2", data.Part1, 15m);
			Helper.CreatePickNew(order2);
			order2.Lines[0].PickLines[0].WZ_PickedDateTime = DateTime.Now;

			Factory.Save();
			AssertEquals("Precondition - Two existing picks", 2, Factory.Load<WhsPick>(new ZQuery()).Length);
			AssertEquals("Order1 Status correct.", WhsOrderStatus.Codes.Staged, order1.WarehouseOrderStatus);
			AssertEquals("Order2 Status correct.", DocketStatus.Codes.Picking, order2.WarehouseOrderStatus);

			string[] expectedLogText = {
				"ERROR: Order [HL W00000003] is Staged (STA).",
				"ERROR: Order [HL W00000005] is Picking (PIC).",
				string.Format("ERROR: No Valid Orders to Create Picks for.", order1.WD_DocketID, order2.WD_DocketID) };
			var orders = new WhsOrder[]
			{
				order1,
				order2
			};

			ApplyApplicatorLogOrderIsUnimportantIgnoreString(orders, expectedLogText, "\n");

			var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
			AssertEquals("No new picks created", 2, numberOfPicks);
		}

		#endregion

		#region TestDoesNotRemoveOrderIfAwaitingReplenishment

		public void TestDoesNotRemoveOrderIfAwaitingReplenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client2 = Helper.CreateClient("CLIENT2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var whs2 = Helper.CreateWarehouse("WHS", "A");
			var ruleSet = AllocationRulesHelper.MakeNewRuleSetAndDeactivateSystemAllocationRuleSet(Factory);
			AllocationRulesHelper.AddPickFaceRules(ruleSet, 10);
			AllocationRulesHelper.AddFifoRule(ruleSet, 20, preventPickingPickFacesFromBulk: true);

			var pickFaceLocation = data.Whs1.FindLocation("A-1");
			var bulkLocation = data.Whs1.FindLocation("A-2");
			Helper.CreateProductPickFace(data.Part1, data.Org1, pickFaceLocation);
			Helper.CreateProductPickFace(data.Part1, client2, pickFaceLocation);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", ZDateTimeOffset.Today.AddDays(-1), data.Part1, 10m, bulkLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 2m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "2", data.Part1, 2m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "3", data.Part2, 2m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, whs2, "4", data.Part2, 2m);

			Factory.Save();

			var expectedLog = @"INFO: Attempting to Create Pick with 3x Order(s) for Warehouse 1
WARNING: Order [HL W00000003] could not be allocated stock.
WARNING: Order [HL W00000004] could not be allocated stock.
WARNING: Pick P00000001 has been created and is awaiting replenishment.
INFO: Attempting to Create Pick with 1x Order(s) for Warehouse WHS
WARNING: Order [HL W00000005] could not be allocated stock.
WARNING: No Pick was created for Warehouse WHS.";

			// force the sequence of Orders loaded from the DB.
			var factoryServiceMock = new Mock<IFactoryService>();
			factoryServiceMock.Setup(foo => foo.GetFactory<Func<BusinessObjectFactory>>())
				.Returns(() => new HackyFactory { NameForDebugging = "HACK FACTORY" });

			using (ObjectFactory.Substitute(factoryServiceMock.Object))
			{
				ApplyApplicator(new[] { order1, order2, order3, order4 }, expectedLog);
			}

			AssertNotNull(order1.Pick);
			AssertNull(order2.Pick);
			AssertNull(order3.Pick);

			AssertEquals("Only Order 1 should be allocated since it's waiting for replenishment.", true, order1.Pick.WP_IsAwaitingReplenishment);
		}

		class HackyFactory : BusinessObjectFactory
		{
			public HackyFactory() : base() { }
			public HackyFactory(string databaseName) : base(databaseName) { }
			public HackyFactory(DbConnection connection) : base(connection) { }

			public override BusinessObject[] Load(Type bizOType, ZQuery sqlFilter)
			{
				var result = base.Load(bizOType, sqlFilter);

				if (bizOType == typeof(WhsOrder))
				{
					result = result.Cast<WhsOrder>().OrderBy(o => o.WD_DocketID).ToArray();
				}

				return result;
			}
		}

		#endregion

		#region TestLogging

		public void TestLogging_SaveFailureEvent()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product1 = Helper.CreateProduct(client, "P1");

			var receive = Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R1", product1, 5);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "O1", product1, 6);
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "O2", product1, 1);
			var pick = Helper.CreatePickByAttachingOrders(order2);
			Factory.Save();

			var eventFired = false;
			var newFactory = new BusinessObjectFactory { NameForDebugging = "Test Save Failure" };
			var factoryServiceMock = new Mock<IFactoryService>();
			factoryServiceMock.Setup(foo => foo.GetFactory<Func<BusinessObjectFactory>>())
				.Returns(() => newFactory);

			string message = null;

			newFactory.Saving += f =>
			{
				var order1InNewFactory = f.Load<WhsOrder>(order1.PK);
				order1InNewFactory.Pick.OrderedInventories[0].AvailableInventories[0].PickLines.ElementAt(0).WZ_Units++;

				order1InNewFactory.Pick.SaveFailureEvent += (sender, e) =>
				{
					eventFired = true;
					message = e.Message;
				};
			};

			var expectedLogText = @"INFO: Attempting to Create Pick with 1x Order(s) for Warehouse WHS1
ERROR: Another user has taken some of the stock that you have tried to allocate.
Close and re-open the form, then reallocate stock.";

			using (ObjectFactory.Substitute(factoryServiceMock.Object))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				AssertNoExceptionThrown(() => ApplyApplicator(new[] { order1 }, expectedLogText));
			}

			AssertEquals("The save fail event should have fired", true, eventFired);
			AssertEquals("Message should be correct.",
				"Another user has taken some of the stock that you have tried to allocate.\r\nClose and re-open the form, then reallocate stock.", message);
		}

		public void TestLogging_InvalidProductSpecs()
		{
			var warehouse = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");

			var product1 = Helper.CreateProduct(client, "P1"); // x weight
			var product2 = Helper.CreateProduct(client, "P2"); // x vol
			var product3 = Helper.CreateProduct(client, "P3"); // x weight vol
			var product4 = Helper.CreateProduct(client, "P4");

			Helper.SetProductWeightAndVolume(product1, 999999.99m, Constants.Weight.Kilograms, 0.2m, Constants.Volume.CubicMetres);
			Helper.SetProductWeightAndVolume(product2, 99.0m, Constants.Weight.Kilograms, 999999.99m, Constants.Volume.CubicMetres);
			Helper.SetProductWeightAndVolume(product3, 999999.99m, Constants.Weight.Kilograms, 999999.99m, Constants.Volume.CubicMetres);
			Helper.SetProductWeightAndVolume(product4, 9.0m, Constants.Weight.Kilograms, 9.0m, Constants.Volume.CubicMetres);

			var receive = Helper.CreateWhsReceive(client, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive, product1, 15m);
			Helper.CreateWhsReceiveInventoryLine(receive, product2, 15m);
			Helper.CreateWhsReceiveInventoryLine(receive, product3, 15m);
			Helper.CreateWhsReceiveInventoryLine(receive, product4, 15m);
			receive.WD_TotalWeight = 1m; // Hack to save receive to DB so that it won't exceed the DB limit
			receive.WD_TotalCubic = 1m;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "oa1", product1, 15m);
			order1.WD_DocketID = "W0001001";
			var order2 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "oa2", product2, 15m);
			order2.WD_DocketID = "W0001002";
			var order3 = Helper.CreateWhsOrderWithOrderLine(client, warehouse, "oa3", product3, 15m);
			order3.WD_DocketID = "W0001003";

			// Hack Entered Status
			order1.WD_DocketStatus = DocketStatus.Codes.Entered;
			order2.WD_DocketStatus = DocketStatus.Codes.Entered;
			order3.WD_DocketStatus = DocketStatus.Codes.Entered;

			AssertCollectionContains(
				"Error - WD_TotalWeight: The number 14,999,999.85 is too large, the maximum value allowed for Weight is 999,999.999.",
				order1.Notifications.Select(n => n.Message));
			AssertCollectionContains(
				"Error - WD_TotalCubic: The number 14,999,999.85 is too large, the maximum value allowed for Volume is 999,999.999.",
				order2.Notifications.Select(n => n.Message));
			AssertCollectionContains(
				"Error - WD_TotalCubic: The number 14,999,999.85 is too large, the maximum value allowed for Volume is 999,999.999.",
				order3.Notifications.Select(n => n.Message));
			AssertCollectionContains(
				"Error - WD_TotalWeight: The number 14,999,999.85 is too large, the maximum value allowed for Weight is 999,999.999.",
				order3.Notifications.Select(n => n.Message));

			var orders = new[] { order1, order2, order3 };
			string expectedLogText = string.Format(
				"ERROR: Order [HL {0}] has errors. Resolve these on the order before attempting to pick.\r\n" +
				"ERROR: Order [HL {1}] has errors. Resolve these on the order before attempting to pick.\r\n" +
				"ERROR: Order [HL {2}] has errors. Resolve these on the order before attempting to pick.\r\n" +
				"ERROR: No Valid Orders to Create Picks for.",
				order1.WD_DocketID,
				order2.WD_DocketID,
				order3.WD_DocketID);

			ApplyApplicator(orders, expectedLogText);

			AssertEquals("Order should not have pick assigned", ZGuid.Empty, order1.WD_WP);
			AssertEquals("Order should not have pick assigned", ZGuid.Empty, order2.WD_WP);
			AssertEquals("Order should not have pick assigned", ZGuid.Empty, order3.WD_WP);

			var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
			AssertEquals("No picks should have been created", 0, numberOfPicks);
		}

		public void TestLogging_NoAllocationRules()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var client = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var product1 = Helper.CreateProduct(client, "P1");

			var receive = Helper.CreateWhsReceiveWithInventory(client, warehouse1, "R1", product1, 5);
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, warehouse1, "O1", product1, 6);
			Factory.Save();

			var allocationRuleSetQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWA");
			allocationRuleSetQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var allocationRuleSet = Factory.LoadTop1<ProductionRuleSet>(allocationRuleSetQuery);
			allocationRuleSet.PRS_IsLive = false;
			Factory.Save();

			var expectedLogText = @"INFO: Attempting to Create Pick with 1x Order(s) for Warehouse WHS1
WARNING: Issue occurred during allocation: No rules found for context: 'PWA'
WARNING: Order [HL W00000002] could not be allocated stock.
WARNING: No Pick was created for Warehouse WHS1.";

			ApplyApplicator(new[] { order }, expectedLogText);
		}

		#endregion

		#region TestActionDbHits

		public void TestActionDbHits()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1", "Test Warehouse 1");
			var warehouse2 = Helper.CreateWarehouse("WHS2", "Test Warehouse 2");
			var warehouse3 = Helper.CreateWarehouse("WHS3", "Test Warehouse 3");
			var warehouse4 = Helper.CreateWarehouse("WHS4", "Test Warehouse 4");
			var warehouses = new[] { warehouse1, warehouse2, warehouse3, warehouse4 };

			var client1 = Helper.CreateClient("CQTC", "QTC Client", "QTC", "WKY");
			var client2 = Helper.CreateClient("Client2");
			var client3 = Helper.CreateClient("Client3");
			var client4 = Helper.CreateClient("Client4");

			var product1 = Helper.CreateProduct(client1, "P1");
			var product2 = Helper.CreateProduct(client1, "P2");
			var product3 = Helper.CreateProduct(client1, "P3");
			var product4 = Helper.CreateProduct(client1, "P4");
			var products = new[] { product1, product2, product3, product4 };

			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "moOArcv1", product1, 50m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse2, "moOArcv2", product2, 50m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse3, "moOArcv3", product3, 50m);
			Helper.CreateWhsReceiveWithInventory(client1, warehouse4, "moOArcv4", product4, 50m);
			Factory.Save();

			var orders = new List<WhsOrder> { };
			for (var i = 0; i < 40; i++)
			{
				var j = i % 4;
				var whsToUse = warehouses[j];
				var productTouse = products[j];
				var order = Helper.CreateWhsOrderWithOrderLine(client1, whsToUse, $"MultiOrderOA{i}", productTouse, 5m);
				var consignee = Helper.CreateClient($"CONSIGNEE{i}");
				order.ConsigneePK = consignee.PK;
				orders.Add(order);
			}

			Factory.Save();

			var factoryServiceMock = new Mock<IFactoryService>();
			factoryServiceMock.Setup(foo => foo.GetFactory<Func<BusinessObjectFactory>>())
				.Returns(() => new BusinessObjectFactory { NameForDebugging = "Test OA DB Hits" });

			AssertEquals("Precondition - No existing picks", 0, Factory.Load<WhsPick>(new ZQuery()).Length);

			var expectedLogText = @"INFO: Attempting to Create Pick with 10x Order(s) for Warehouse WHS1
INFO: Created pick [HL P00000001] with 10x Order(s) for Warehouse WHS1.
INFO: Attempting to Create Pick with 10x Order(s) for Warehouse WHS2
INFO: Created pick [HL P00000002] with 10x Order(s) for Warehouse WHS2.
INFO: Attempting to Create Pick with 10x Order(s) for Warehouse WHS3
INFO: Created pick [HL P00000003] with 10x Order(s) for Warehouse WHS3.
INFO: Attempting to Create Pick with 10x Order(s) for Warehouse WHS4
INFO: Created pick [HL P00000004] with 10x Order(s) for Warehouse WHS4.";

			var dbHitsNewFactory = new Dictionary<string, int>
			{
				{ WhsWarehouseSchema.Constants.TableName, 4 }, // 1 per factory
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 4 }, // 1 per factory
				{ WhsLocationTypeSchema.Constants.TableName, 8 }, // 2 per factory
				{ StmEventSchema.Constants.TableName, 16 }, // 4 per factory
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 4 }, // 2 per factory
				{ JobDocAddressSchema.Constants.TableName, 4 }, // 1 per factory
				{ OrgAddressCapabilitySchema.Constants.TableName, 4 }, // 1 per factory
				{ OrgContactSchema.Constants.TableName, 4 }, // 1 per factory
				{ OrgCusCodeSchema.Constants.TableName, 4 }, // 1 per factory
				{ OrgSupplierPartSchema.Constants.TableName, 4 }, // 1 per factory
				{ OrgPartUnitSchema.Constants.TableName, 4 }, // 1 per factory
				{ OrgPartRelationSchema.Constants.TableName, 4 }, // 1 per factory
				{ OrgMiscServSchema.Constants.TableName, 4 }, // 1 per factory
				{ OrgHeaderSchema.Constants.TableName, 4 }, // 1 per factory
				{ GlbBranchSchema.Constants.TableName, 12 }, // 3 per factory
				{ GlbCompanySchema.Constants.TableName, 8 }, // 2 per factory
				{ GlbStaffSchema.Constants.TableName, 4 }, // 1 per factory
				{ JobHeaderSchema.Constants.TableName, 44 }, // 11 per factory
				{ OrgAddressSchema.Constants.TableName, 8 }, // 2 per factory
				{ WhsInventoryViewSchema.Constants.TableName, 4 }, // 1 per factory
				{ PkgPackageJobSchema.Constants.TableName, 4 }, // 1 per factory
				{ ProcessTaskNotificationSchema.Constants.TableName, 8 }, // 2 per factory
				{ WhsDocketLineSchema.Constants.TableName, 8 }, // 2 per factory
				{ ProcessTasksSchema.Constants.TableName, 8 }, // 2 per factory
				{ WhsLocationViewSchema.Constants.TableName, 8 }, // 2 per factory
				{ WhsPickLineSchema.Constants.TableName, 8 }, // 2 per factory
				{ ProcessTaskTemplateSchema.Constants.TableName, 8 }, // 2 per factory
				{ WhsDocketSchema.Constants.TableName, 8 }, // 2 per factory
				{ RefCountrySchema.Constants.TableName, 4 }, // 1 per factory
				{ RefPackTypeSchema.Constants.TableName, 4 }, // 1 per factory
				{ RefTimeZoneSchema.Constants.TableName, 4 }, // 1 per factory
				{ RefTimeZoneSetSchema.Constants.TableName, 4 }, // 1 per factory
				{ RefUNLOCOSchema.Constants.TableName, 4 }, // 1 per factory
				{ StmALogSchema.Constants.TableName, 4 }, // 1 per factory
				{ WhsAreaSchema.Constants.TableName, 4 }, // 1 per factory
				{ ProductionRuleSchema.Constants.TableName, 4 }, // 1 per factory
				{ RefPacksSchema.Constants.TableName, 4 }, // 1 per factory
				{ WhsPickFaceSchema.Constants.TableName, 4 }, // 1 per factory
				{ WhsRowSchema.Constants.TableName, 4 }, // 1 per factory
			};

			using (ObjectFactory.Substitute(factoryServiceMock.Object))
			using (AssertDbHitsForAllFactories(
					dbHitsNewFactory,
					useOnlyNewFactories: true,
					includeFactoryPredicate: f => !f.NameForDebugging.StartsWith("MilestoneCollectionView Status Update"),
					tablesToCollectQueriesFor: dbHitsNewFactory.Keys.ToArray()))
			using (RowFactory.SetCachedTables())
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				ApplyApplicator(orders.ToArray(), expectedLogText);
			}

			var numberOfPicksNewFactory = new BusinessObjectFactory().Load<WhsPick>(new ZQuery()).Length;
			AssertEquals("4 picks created by OA", 4, numberOfPicksNewFactory);
			var numberOfPicks = Factory.Load<WhsPick>(new ZQuery()).Length;
			AssertEquals("4 picks created by OA", 4, numberOfPicks);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var factoryService = ObjectFactory.New<IFactoryService>();
			factoryService.RegisterFactory(() => new BusinessObjectFactory());

			return new GenerateMultiOrderPickActionMethodApplicator(factoryService);
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
