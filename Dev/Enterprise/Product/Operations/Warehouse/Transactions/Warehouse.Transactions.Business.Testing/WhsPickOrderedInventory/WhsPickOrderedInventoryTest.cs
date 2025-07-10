using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsPickOrderedInventory))]
	class WhsPickOrderedInventoryTest : WhsNonPersistentBusinessObjectTestCase
	{
		#region TestAvailableInventoryValidationSuspendedSemaphore

		public void TestAvailableInventoryValidationSuspendedSemaphore()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			orderLine.WE_TransactionQuantity = -10m;
			var pick = Factory.New<WhsPick>();
			var orderedInventory = new WhsPickOrderedInventory(Factory);
			((IWhsPickOrderedInventoryInternals)orderedInventory).SetAllProperties(pick, orderLine);
			using (new SemaphoreManager(orderedInventory.AvailableInventoryValidationSuspendedSemaphore))
			{
				var poke = orderedInventory.AvailableInventories;
			}
			AssertNoError(orderedInventory.PickLineQuantityInfo, "Quantity Allocated can not be greater than Units Ordered. Deallocate some stock in the Inventory grid to reduce this value.");

			orderedInventory.Validation.ValidatePickLineQuantity();
			AssertHasError(orderedInventory.PickLineQuantityInfo, "Quantity Allocated can not be greater than Units Ordered. Deallocate some stock in the Inventory grid to reduce this value.");
		}

		#endregion

		#region TestGetAvailableInventoriesForPalletIDs

		public void TestGetAvailableInventoriesForPalletIDs_NullThrows()
		{
			var orderedInventory = new WhsPickOrderedInventory(Factory);
			AssertExceptionThrown<ArgumentNullException>(() => orderedInventory.GetAvailableInventoriesForPalletIDs(null));
		}

		public void TestGetAvailableInventoriesForPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receive_inv1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			var receive_inv2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			var receive_inv3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();

			var unfilteredAvailableInventories = orderedInventory.AvailableInventories;
			AssertEquals(3, unfilteredAvailableInventories.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z1", "Z2", "Z3" }, unfilteredAvailableInventories.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));

			var filteredAvailInv1 = orderedInventory.GetAvailableInventoriesForPalletIDs(new[] { "Z1" });
			AssertEquals(1, filteredAvailInv1.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z1" }, filteredAvailInv1.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));

			var filteredAvailInv2 = orderedInventory.GetAvailableInventoriesForPalletIDs(new[] { "Z2", "Z3" });
			AssertEquals(2, filteredAvailInv2.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Z2", "Z3" }, filteredAvailInv2.Cast<WhsPickAvailableInventory>().Select(availInv => availInv.PalletID));
		}

		public void TestGetAvailableInventoriesForPalletIDs_DbHits() => TestGetAvailableInventoriesForPalletIDs_DbHitsCore();

		public void TestGetAvailableInventoriesForPalletIDs_DbHits_HeldGoodsForOrdersDisabled() => TestGetAvailableInventoriesForPalletIDs_DbHitsCore(enableHeldGoodsForOrders: false);

		void TestGetAvailableInventoriesForPalletIDs_DbHitsCore(bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				var rowB = Helper.CreateRowAndGenerateLocations(data.Whs1, "B", 10, 10);
				Factory.Save();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
				var palletIDs = new List<string>();

				for (int i = 1; i <= 25; i++)
				{
					var palletID = "Z" + i;
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, rowB.Locations[i], palletID);
					palletIDs.Add(palletID);
				}

				receive.FinaliseDocketWithoutUserConfirmation();
				AssertEquals(true, receive.IsFinalised);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
				var pick = Helper.CreatePickNew(order);

				var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
				var pickInFactory2 = factory2.Load<WhsPick>(pick.PK);

				using (RowFactory.SetCachedTables())
				{
					var availableInventories = pickInFactory2.OrderedInventories[0].GetAvailableInventoriesForPalletIDs(palletIDs);

					foreach (var availableInventory in availableInventories.Cast<WhsPickAvailableInventory>())
					{
						_ = availableInventory.PalletID;
					}
				}

				var expectedDbHits = new Dictionary<string, int>()
				{
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsInventoryViewSchema.Constants.TableName, 1 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 1 },
					{ WhsRowSchema.Constants.TableName, 1 },
					{ WhsPickFaceSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
				};

				AssertDbHits(expectedDbHits, factory2);
			}
		}

		#endregion

		#region TestPerformance_LoadPickLinesForThisOrderedInventory

		[TestDate(2019, 1, 1)]
		public void TestPerformance_LoadPickLinesForThisOrderedInventory() => TestPerformance_LoadPickLinesForThisOrderedInventory_Core();

		[TestDate(2019, 1, 1)]
		public void TestPerformance_LoadPickLinesForThisOrderedInventory_HeldGoodsForOrdersDisabled() => TestPerformance_LoadPickLinesForThisOrderedInventory_Core(enableHeldGoodsForOrders: false);

		void TestPerformance_LoadPickLinesForThisOrderedInventory_Core(bool enableHeldGoodsForOrders = true)
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableHeldGoodsForOrders))
			{
				const int NumberOfUniqueRecords = 10;
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

				for (int i = 0; i < NumberOfUniqueRecords; i++)
				{
					var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i, Notify);
					receive.WD_ArrivalDate = new ZDateTimeOffset(2018, 1, 1).AddDays(i);
					for (int j = 0; j < NumberOfUniqueRecords; j++)
					{
						Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1" + j, "", "", "");
						Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA1" + j, "", "", "");
						Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, ZDate.Empty, ZDate.Empty, "PA2" + j, "", "", "");
						Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, ZDate.Empty, ZDate.Empty, "PA3" + j, "", "", "");
					}
					receive.AllocateLocationsWithMock();
					receive.FinaliseDocket();
					AssertIsFinalisedPrecondition(receive);
				}
				Factory.Save();

				var ordersToPick = new List<WhsOrder>();
				for (int i = 0; i < NumberOfUniqueRecords; i++)
				{
					var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "OR" + i, Notify);
					for (int j = 0; j < NumberOfUniqueRecords; j++)
					{
						CreateWhsOrderLine(order, data.Part1, 2m, "PA1" + j);
						CreateWhsOrderLine(order, data.Part1, 1m, "PA2" + j);
						CreateWhsOrderLine(order, data.Part1, 1m, "PA3" + j);
						CreateWhsOrderLine(order, data.Part1, 1m, "PA3" + j);
					}
					ordersToPick.Add(order);
				}

				var pick = Helper.CreatePickByAttachingOrders(ordersToPick.ToArray());
				pick.AutoAllocateItems();
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var expectedDBHits = new Dictionary<string, int>
				{
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 1 },
					{ WhsInventoryViewSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 7 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
					{ WhsPickFaceSchema.Constants.TableName, 1 },
				};

				using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, otherFactory))
				using (RowFactory.SetCachedTables())
				{
					var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
					foreach (WhsPickOrderedInventory orderedInventory in pickInOtherFactory.OrderedInventories)
					{
						foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
						{
							var poke = availableInventory.PickLines.Count();
						}
					}
				}
			}
		}

		WhsOrderLine CreateWhsOrderLine(WhsOrder order, OrgSupplierPart part, ZDecimal units, ZString partAttribute1)
		{
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, part, units);
			orderLine.WE_PartAttrib1 = partAttribute1;
			return orderLine;
		}

		#endregion

		#region Business Object Overrides

		public void TestDelete()
		{
			WhsPickAvailableInventory availableInventory1 = OrderedInventory.AvailableInventories.AddNew();
			WhsPickAvailableInventory availableInventory2 = OrderedInventory.AvailableInventories.AddNew();
			AssertEquals("Precondition", 2, OrderedInventory.AvailableInventories.Count);
			AssertEquals("Precondition", false, availableInventory1.IsDeleted);
			AssertEquals("Precondition", false, availableInventory2.IsDeleted);

			OrderedInventory.Delete();

			AssertEquals(0, OrderedInventory.AvailableInventories.Count);
			AssertEquals(true, availableInventory1.IsDeleted);
			AssertEquals(true, availableInventory2.IsDeleted);
		}

		#endregion

		#region Related Business Objects

		public void TestInventory()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateMultiWarehouseClientProductInventory(TestDataForInventory.StockType.WithoutBondEntryKeys);

			var order = Helper.CreateWhsOrder(data.Org2, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 20m);
			Helper.SetDocketLineAttributes(orderLine2, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");

			Pick = Factory.New<WhsPick>();
			Pick.Orders.Add(order);

			AssertEquals("Precondition", 2, Pick.OrderedInventories.Count);

			var orderedInventory1 = Pick.OrderedInventories[1]; // sorted to 2nd element
			var orderedInventory2 = Pick.OrderedInventories[0]; // sorted to 1st element

			AssertEquals(7, orderedInventory1.AvailableInventories.Count);
			AssertEquals(5, orderedInventory2.AvailableInventories.Count);
		}

		public void TestClearInventoryCache()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 110m);
			Factory.Save();

			Pick = Factory.New<WhsPick>();
			Pick.Orders.Add(order);

			// create more inventory
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "receive", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var orderedInventory = Pick.OrderedInventories[0];
			AssertEquals("Precondition", 1, orderedInventory.AvailableInventories.Count);

			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition", 100m, availableInventory.QuantityAvailableToPick);
			Factory.Save();

			Pick.ClearAllInventoriesCache();
			orderedInventory.ClearAvailableInventoriesCache();
			AssertEquals("Clearing the Inventory cache should have loaded in the newly received stock.", 100m, orderedInventory.AvailableInventories[0].QuantityAvailableToPick);
			AssertEquals("Clearing the Inventory cache should have loaded in the newly received stock.", 10m, orderedInventory.AvailableInventories[1].QuantityAvailableToPick);
		}

		public void TestOwners()
		{
			var whs = Helper.CreateWarehouse("1");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");

			var order1 = Helper.CreateWhsOrder(org, whs, "1");
			var order2 = Helper.CreateWhsOrder(org, whs, "2");
			Helper.CreateWhsOrderLine(order1, part, 10);
			Helper.CreateWhsOrderLine(order1, part, 20);
			Helper.CreateWhsOrderLine(order1, part, 30, "BEK", "DummyOutward-1", "");
			Helper.CreateWhsOrderLine(order2, part, 40);
			Helper.CreateWhsOrderLine(order2, part, 50, "BEK", "DummyOutward-1", "");

			Pick = Factory.New<WhsPick>();
			Pick.Orders.Add(order1);
			Pick.Orders.Add(order2);

			AssertEquals(2, Pick.OrderedInventories.Count);

			var orderedInventory1 = Pick.OrderedInventories[1]; // sorted to 2nd element
			var orderedInventory2 = Pick.OrderedInventories[0]; // sorted to 1st element

			AssertEquals(3, orderedInventory1.Owners.Count);
			AssertEquals(2, orderedInventory2.Owners.Count);

			AssertEquals(true, orderedInventory1.Owners.Contains(order1.Lines[0]));
			AssertEquals(true, orderedInventory1.Owners.Contains(order1.Lines[1]));
			AssertEquals(false, orderedInventory1.Owners.Contains(order1.Lines[2]));
			AssertEquals(true, orderedInventory1.Owners.Contains(order2.Lines[0]));
			AssertEquals(false, orderedInventory1.Owners.Contains(order2.Lines[1]));

			AssertEquals(false, orderedInventory2.Owners.Contains(order1.Lines[0]));
			AssertEquals(false, orderedInventory2.Owners.Contains(order1.Lines[1]));
			AssertEquals(true, orderedInventory2.Owners.Contains(order1.Lines[2]));
			AssertEquals(false, orderedInventory2.Owners.Contains(order2.Lines[0]));
			AssertEquals(true, orderedInventory2.Owners.Contains(order2.Lines[1]));
		}

		public void TestClient()
		{
			TestDataForInventory testData = new TestDataForInventory(Factory);
			testData.CreateSimpleInventory();

			WhsOrder order = Helper.CreateWhsOrder(testData.Org1, testData.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, testData.Part1, 100m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition.", 1, pick.OrderedInventories.Count);
			AssertEquals("Precondition.", 1, pick.OrderedInventories[0].Owners.Count);
			pick.OrderedInventories[0].Owners[0].Delete();

			AssertEquals("OrderedInventories should be empty since order line has been deleted.", 0, pick.OrderedInventories.Count);
		}

		#region TestPickLines

		public void TestPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - 4 pick lines should be created 2 for Part1 and 2 for Part2.", 4, pick.GetAllPickLines().Count());

			var orderedInventory1 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part1);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { orderLine1 }, orderedInventory1.Owners);
			AssertContainsExactElementsInAnyOrder("Ordered inventory's PickLines collection should contain all pick lines from all of its owners.",
				orderedInventory1.Owners.SelectMany(o => o.PickLines),
				orderedInventory1.PickLines);

			var orderedInventory2 = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPart == data.Part2);
			AssertContainsExactElementsInAnyOrder("Precondition", new[] { orderLine2, orderLine3 }, orderedInventory2.Owners);
			AssertContainsExactElementsInAnyOrder("Ordered inventory's PickLines collection should contain all pick lines from all of its owners.",
				orderedInventory2.Owners.SelectMany(o => o.PickLines),
				orderedInventory2.PickLines);
		}

		public void TestPickLines_OnlyRelated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition.", 1, pick1.GetAllPickLines().Count());
			AssertEquals("Precondition.", 1, pick2.GetAllPickLines().Count());

			var orderedInventory1 = pick1.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var orderLine1 = orderedInventory1.Owners.Single();
			var pickLine1 = orderedInventory1.PickLines.Single();
			AssertEquals("Ensure that only 1 pick line is picked from the inventory.", orderLine1.PK, pickLine1.WZ_WE_TransactionLine);

			var orderedInventory2 = pick2.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var orderLine2 = orderedInventory2.Owners.Single();
			var pickLine2 = orderedInventory2.PickLines.Single();
			AssertEquals("Ensure that only 1 pick line is picked from the inventory.", orderLine2.PK, pickLine2.WZ_WE_TransactionLine);
		}

		#endregion

		#endregion

		#region Validation

		public void TestValidation()
		{
			AssertEquals(typeof(WhsPickOrderedInventoryValidation), OrderedInventory.Validation.GetType());
		}

		public virtual void TestValidationQuantityShortWarningMessage()
		{
			AssertEquals(true, OrderedInventory.ValidationQuantityShortWarningMessage.IsEmpty);
			AssertNoWarnings(OrderedInventory.QuantityShortInfo);

			OrderedInventory.ValidationQuantityShortWarningMessage = "Warning1";
			AssertEquals("Warning1", OrderedInventory.ValidationQuantityShortWarningMessage);
			AssertHasWarning("Validation of Quantity Short should have been triggered", OrderedInventory.QuantityShortInfo, "Warning1");

			OrderedInventory.ValidationQuantityShortWarningMessage = ZString.Empty;
			AssertEquals(ZString.Empty, OrderedInventory.ValidationQuantityShortWarningMessage);
			AssertNoWarnings(OrderedInventory.QuantityShortInfo);

			using (OrderedInventory.GetValidationSuspender())
			{
				OrderedInventory.ValidationQuantityShortWarningMessage = "Warning2";
				AssertEquals("Warning2", OrderedInventory.ValidationQuantityShortWarningMessage);
				AssertNoWarnings("Validation of Quantity Short should not have been triggered if validation suspended", OrderedInventory.QuantityShortInfo);
			}
		}

		#endregion

		#region Properties

		#region TestClientCodeInfo

		public void TestClientCodeInfo()
		{
			AssertEquals("ClientCode", OrderedInventory.ClientCodeInfo.Name);
		}

		#endregion

		#region TestProductCodeInfo

		public void TestProductCodeInfo()
		{
			AssertEquals("ProductCode", OrderedInventory.ProductCodeInfo.Name);
		}

		#endregion

		#region TestProductDescInfo

		public void TestProductDescInfo()
		{
			AssertEquals("ProductDesc", OrderedInventory.ProductDescInfo.Name);
		}

		#endregion

		#region TestSupplierPartPK

		public void TestSupplierPartPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "P123");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.DefaultLocation, "P456");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order1);
			var orderedInventory = pick.OrderedInventories[0];

			AssertEquals(nameof(orderedInventory.SupplierPartPK), data.Part1.PK, orderedInventory.SupplierPartPK);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part2, 20m);
			orderedInventory.Owners.RemoveAllFromRelationship();
			((IWhsPickOrderedInventoryInternals)orderedInventory).SetAllProperties(null, order2.Lines[0]);
			AssertEquals("Should have invalidated SupplierPartPK cache.", data.Part2.PK, orderedInventory.SupplierPartPK);
		}

		#endregion

		#region TestCommodityCode

		public void TestCommodityCode()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			orderLine.SupplierPart.OP_RH_NKCommodityCode = "CC1";
			((IWhsPickOrderedInventoryInternals)OrderedInventory).SetAllProperties(null, orderLine);
			AssertEquals("CC1", OrderedInventory.CommodityCode);
		}

		#endregion

		#region TestCommodityCodeInfo

		public void TestCommodityCodeInfo()
		{
			AssertEquals("CommodityCode", OrderedInventory.CommodityCodeInfo.Name);
		}

		#endregion

		#region TestIsBOMProductPickedOnSalesOrder

		public void TestIsBOMProductPickedOnSalesOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			var pickOrderInventoryForMainProduct = pick.OrderedInventories.GetOrderedInventoryForLine(orderLine);
			AssertEquals("pickOrderInventoryForMainProduct is a BOM Product picked on a Sales Order.", true, pickOrderInventoryForMainProduct.IsBOMProductPickedOnSalesOrder);

			var pickOrderInventoryForbomComponentProduct = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SingleOrDefault(i => i.SupplierPart.PK == bomComponentProduct.PK);
			AssertEquals("pickOrderInventoryForbomComponentProduct is not a BOM Product picked on a Sales Order.", false, pickOrderInventoryForbomComponentProduct.IsBOMProductPickedOnSalesOrder);
		}

		#endregion

		#region TestIsCustomsTransaction

		public void TestIsCustomsTransaction()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var pick = Factory.New<WhsPick>();
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);
			var customsOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			customsOrder.WD_DocketSubType = OrderType.Codes.Customs;

			var orderedInventory1 = new WhsPickOrderedInventory(Factory);
			AssertEquals(false, orderedInventory1.IsCustomsTransaction);

			((IWhsPickOrderedInventoryInternals)orderedInventory1).SetAllProperties(pick, order.Lines[0]);
			AssertEquals(false, orderedInventory1.IsCustomsTransaction);

			var orderedInventory2 = new WhsPickOrderedInventory(Factory);
			((IWhsPickOrderedInventoryInternals)orderedInventory2).SetAllProperties(pick, customsOrder.Lines[0]);
			AssertEquals(true, orderedInventory2.IsCustomsTransaction);
		}

		#endregion

		#region TestIsInwardProcessingJob

		public void TestIsInwardProcessingJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var pick = Factory.New<WhsPick>();

			var orderLine = Factory.New<WhsOrderLine>();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 5m);

			var customsOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			customsOrder.WD_DocketSubType = OrderType.Codes.Customs;

			var iprOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			iprOrder.WD_DocketSubType = OrderType.Codes.Customs;
			iprOrder.WD_IsInwardsProcessingJob = true;

			var orderedInventory1 = new WhsPickOrderedInventory(Factory);
			AssertEquals(false, orderedInventory1.IsInwardProcessingJob);

			var orderedInventory2 = new WhsPickOrderedInventory(Factory);
			((IWhsPickOrderedInventoryInternals)orderedInventory2).SetAllProperties(pick, orderLine);
			AssertEquals(false, orderedInventory2.IsInwardProcessingJob);

			var orderedInventory3 = new WhsPickOrderedInventory(Factory);
			((IWhsPickOrderedInventoryInternals)orderedInventory3).SetAllProperties(pick, order.Lines[0]);
			AssertEquals(false, orderedInventory3.IsInwardProcessingJob);

			var orderedInventory4 = new WhsPickOrderedInventory(Factory);
			((IWhsPickOrderedInventoryInternals)orderedInventory4).SetAllProperties(pick, customsOrder.Lines[0]);
			AssertEquals(false, orderedInventory4.IsInwardProcessingJob);

			var orderedInventory5 = new WhsPickOrderedInventory(Factory);
			((IWhsPickOrderedInventoryInternals)orderedInventory5).SetAllProperties(pick, iprOrder.Lines[0]);
			AssertEquals(true, orderedInventory5.IsInwardProcessingJob);
		}

		#endregion

		#region TestUnitsUQ

		public void TestUnitsUQ()
		{
			AssertEquals("UNT", OrderedInventory.UnitsUQ);
		}

		#endregion

		#region TestUnitsUQInfo

		public void TestUnitsUQInfo()
		{
			AssertEquals("UnitsUQ", OrderedInventory.UnitsUQInfo.Name);
		}

		#endregion

		#region TestOnlyShowAvailableStock

		public void TestOnlyShowAvailableStock()
		{
			OrderedInventory.OnlyShowAvailableStock = true;
			AssertEquals(true, OrderedInventory.OnlyShowAvailableStock);
			OrderedInventory.OnlyShowAvailableStock = false;
			AssertEquals(false, OrderedInventory.OnlyShowAvailableStock);
		}

		#endregion

		#region TestQuantityOrdered

		public void TestQuantityOrdered()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			orderLine.WE_TransactionQuantity = 10.5m;

			AssertEquals(false, OrderedInventory.QuantityShortInfo.HasWarnings());
			((IWhsPickOrderedInventoryInternals)OrderedInventory).SetAllProperties(null, orderLine);
			AssertEquals(10.5m, OrderedInventory.QuantityOrdered);

			var orderLine1 = order.Lines.AddNew();
			orderLine1.WE_OP = Factory.New<OrgSupplierPart>().PK;
			orderLine1.WE_TransactionQuantity = -12.5m;
			OrderedInventory.Owners.Add(orderLine1);
			AssertEquals(-2m, OrderedInventory.QuantityOrdered);
		}

		public void TestQuantityOrderedInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.QuantityOrdered, OrderedInventory.QuantityOrderedInfo.Name);
			AssertEquals(true, OrderedInventory.QuantityOrderedInfo.ReadOnly);
		}

		#endregion

		#region TestPalletIDOrdered

		public void TestPalletIDOrdered()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			orderLine.WE_OP = Factory.New<OrgSupplierPart>().PK;
			orderLine.WE_TransactionQuantity = 10.5m;
			orderLine.WE_PalletID = "ABC1";

			AssertEquals(false, OrderedInventory.QuantityShortInfo.HasWarnings());
			((IWhsPickOrderedInventoryInternals)OrderedInventory).SetAllProperties(null, orderLine);
			AssertEquals("ABC1", OrderedInventory.PalletIDOrdered);
		}

		public void TestPalletIDOrderedInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.PalletIDOrdered, OrderedInventory.PalletIDOrderedInfo.Name);
			AssertEquals(true, OrderedInventory.PalletIDOrderedInfo.ReadOnly);
		}

		#endregion

		#region TestOrderedHeldCode

		public void TestOrderedHeldCode()
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Damaged;
			((IWhsPickOrderedInventoryInternals)OrderedInventory).SetAllProperties(null, orderLine);
			AssertEquals(InventoryHoldCodes.Codes.Damaged, OrderedInventory.OrderedHeldCode);
		}

		#endregion

		#region TestOrderedHeldCodeInfo

		public void TestOrderedHeldCodeInfo()
		{
			AssertEquals("OrderedHeldCode", OrderedInventory.OrderedHeldCodeInfo.Name);
		}

		#endregion

		#region TestPickQuantity

		#region TestPickQuantity_SomePicked

		public void TestPickQuantity_SomePicked()
		{
			var orderedInventory = TestPickQuantitySetupGetOrderedInventory();

			orderedInventory.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			orderedInventory.PickLines[0].WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
			orderedInventory.PickLines[1].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			orderedInventory.PickLines[1].WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;

			orderedInventory.PickLines[2].WZ_PickedDateTime = ZDateTimeOffset.Now;
			orderedInventory.PickLines[3].WZ_PickedDateTime = ZDateTimeOffset.Now;
			orderedInventory.PickLines[4].WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertEquals(12m /* 3m + 4m + 5m */, orderedInventory.PickQuantity);
		}

		#endregion

		#region TestPickQuantity_AllPicked

		public void TestPickQuantity_AllPicked()
		{
			var orderedInventory = TestPickQuantitySetupGetOrderedInventory();

			orderedInventory.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			orderedInventory.PickLines[0].WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;
			orderedInventory.PickLines[1].WZ_PickedDateTime = ZDateTimeOffset.Empty;
			orderedInventory.PickLines[1].WZ_WE_OriginalPickedInventoryLine = ZGuid.Empty;

			orderedInventory.PickLines[0].WZ_PickedDateTime = ZDateTimeOffset.Now;
			orderedInventory.PickLines[1].WZ_PickedDateTime = ZDateTimeOffset.Now;
			orderedInventory.PickLines[2].WZ_PickedDateTime = ZDateTimeOffset.Now;
			orderedInventory.PickLines[3].WZ_PickedDateTime = ZDateTimeOffset.Now;
			orderedInventory.PickLines[4].WZ_PickedDateTime = ZDateTimeOffset.Now;

			AssertEquals(15m /* 1m + 2m + 3m + 4m + 5m */, orderedInventory.PickQuantity);
		}

		#endregion

		#region TestPickQuantitySetupGetOrderedInventory

		WhsPickOrderedInventory TestPickQuantitySetupGetOrderedInventory()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(1m, 2m, 3m, 4m, 5m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0]; // sorted to 1st element

			foreach (var i in Enumerable.Range(0, 5))
			{
				orderedInventory.AvailableInventories[i].Allocate = true;
			}

			return orderedInventory;
		}

		#endregion

		#region TestPickQuantityInfo

		public void TestPickQuantityInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.PickQuantity, OrderedInventory.PickQuantityInfo.Name);
			AssertEquals(true, OrderedInventory.PickQuantityInfo.ReadOnly);
		}

		#endregion

		#endregion

		#region TestPickLineQuantity

		public void TestPickLineQuantity()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(1m, 2m, 3m, 4m, 5m);
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			Pick = Factory.New<WhsPick>();
			Pick.Orders.Add(order);

			WhsPickOrderedInventory orderedInventory1 = Pick.OrderedInventories[0];
			AssertEquals(0m, orderedInventory1.PickLineQuantity);
			orderedInventory1.AvailableInventories[0].Allocate = true;
			AssertEquals(1m, orderedInventory1.PickLineQuantity);
			orderedInventory1.AvailableInventories[1].Allocate = true;
			AssertEquals(3m, orderedInventory1.PickLineQuantity);
			orderedInventory1.AvailableInventories[2].Allocate = true;
			AssertEquals(6m, orderedInventory1.PickLineQuantity);
			orderedInventory1.AvailableInventories[3].Allocate = true;
			AssertEquals(10m, orderedInventory1.PickLineQuantity);
			orderedInventory1.AvailableInventories[4].Allocate = true;
			AssertEquals(15m, orderedInventory1.PickLineQuantity);

			orderedInventory1.AvailableInventories[2].Allocate = false;
			AssertEquals(12m, orderedInventory1.PickLineQuantity);
		}

		public void TestPickLineQuantityInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.PickLineQuantity, OrderedInventory.PickLineQuantityInfo.Name);
			AssertEquals(true, OrderedInventory.PickLineQuantityInfo.ReadOnly);
		}

		#region TestPickLineQuantity_NotCached

		public void TestPickLineQuantity_NotCached()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "P123");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.DefaultLocation, "P456");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Precondition: All Inventory is picked.", 20m, orderedInventory.PickLineQuantity);

			var availableInventory1 = orderedInventory.AvailableInventories[0];
			var availableInventory2 = orderedInventory.AvailableInventories[1];
			availableInventory1.PickLines.Single().WZ_Units = 5m;

			AssertEquals("Quantity Picked is updated to the correct value.", 15m, orderedInventory.PickLineQuantity);
			AssertEquals("Quantity Picked is updated to the correct value.", 5m, availableInventory1.PickLineQuantity);
			AssertEquals("Quantity Picked is updated to the correct value.", 10m, availableInventory2.PickLineQuantity);

			availableInventory1.PickLines.Single().WZ_Units = 3m;
			availableInventory2.PickLines.Single().WZ_Units = 8m;
			AssertEquals("Quantity Picked is showing correct value.", 3m, availableInventory1.PickLineQuantity);
			AssertEquals("Quantity Picked is showing correct value.", 8m, availableInventory2.PickLineQuantity);
			AssertEquals("Quantity Picked is showing correct value.", 11m, orderedInventory.PickLineQuantity);
		}

		#endregion

		#endregion

		#region TestQuantityCrossDocked

		public void TestQuantityCrossDocked()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(1m, 2m, 3m, 4m, 5m);
			data.MakeSimpleInventoryLinesMergebleForPicking();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			var reservedOrder = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var reservedOrderLine = Helper.CreateWhsOrderLine(reservedOrder, data.Part1, 15m);

			Helper.CreateReservePickLine(reservedOrderLine, data.Line111, 1m);
			Helper.CreateReservePickLine(reservedOrderLine, data.Line112, 2m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals(3m, orderedInventory.QuantityCrossDocked);
		}

		public void TestQuantityCrossDockedInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.QuantityCrossDocked, OrderedInventory.QuantityCrossDockedInfo.Name);
			AssertEquals(true, OrderedInventory.QuantityCrossDockedInfo.ReadOnly);
		}

		#endregion

		#region TestChildLineHasNoReleaseLine

		public void TestChildLineHasNoReleaseLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 10m);
			Factory.Save();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			AssertEquals("1 Child lines should be created.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("1 release line should be added for order line.", 1, orderLine.ReleaseLines.Count);
			AssertEquals("No release line should be added for child line.", 0, orderLine.ChildComponentLines.ElementAt(0).ReleaseLines.Count);
		}

		#endregion

		#region TestQuantityShort

		public void TestQuantityShort()
		{
			TestDataForInventory data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(1m, 2m, 3m, 4m, 5m);
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			Pick = Factory.New<WhsPick>();
			Pick.Orders.Add(order);

			WhsPickOrderedInventory orderedInventory1 = Pick.OrderedInventories[0]; // sorted to 1st element
			AssertEquals(15m, orderedInventory1.QuantityShort);
			orderedInventory1.AvailableInventories[0].Allocate = true;
			AssertEquals(14m, orderedInventory1.QuantityShort);
			orderedInventory1.AvailableInventories[1].Allocate = true;
			AssertEquals(12m, orderedInventory1.QuantityShort);
			orderedInventory1.AvailableInventories[2].Allocate = true;
			AssertEquals(9m, orderedInventory1.QuantityShort);
			orderedInventory1.AvailableInventories[3].Allocate = true;
			AssertEquals(5m, orderedInventory1.QuantityShort);
			orderedInventory1.AvailableInventories[4].Allocate = true;
			AssertEquals(0m, orderedInventory1.QuantityShort);

			orderedInventory1.AvailableInventories[2].Allocate = false;
			AssertEquals(3m, orderedInventory1.QuantityShort);
		}

		public void TestIsComponentOrderedInventoryOnSalesOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			var pickOrderInventoryForMainProduct = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SingleOrDefault(i => i.SupplierPart.PK == mainProduct.PK);
			AssertEquals("pickOrderInventoryForMainProduct is not a ChildLineOrderedInventory", false, pickOrderInventoryForMainProduct.IsComponentOrderedInventoryOnSalesOrder);

			var pickOrderInventoryForbomComponentProduct = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SingleOrDefault(i => i.SupplierPart.PK == bomComponentProduct.PK);
			AssertEquals("pickOrderInventoryForbomComponentProduct is a ChildLineOrderedInventory", true, pickOrderInventoryForbomComponentProduct.IsComponentOrderedInventoryOnSalesOrder);
		}

		public void TestParentSumOfUnitsMetUpdatedFromChildLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			var pickOrderInventoryForMainProduct = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SingleOrDefault(i => i.SupplierPart.PK == mainProduct.PK);
			AssertEquals("SumOfUnitsMet 11, as 5 main product + 6 mainProduct can be created from 20 bomComponentProduct", 11m, orderLine.SumOfUnitsMet);
		}

		public void TestQuantityShortWithBOMComponent()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			var pickOrderInventoryForMainProduct = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SingleOrDefault(i => i.SupplierPart.PK == mainProduct.PK);
			AssertEquals("Shortfall 4, as 5 main product + 6 mainProduct can be created from 20 bomComponentProduct", 4m, pickOrderInventoryForMainProduct.QuantityShort);
		}

		public void TestQuantityShortWithBOMComponentCacheHasNoImpactOnCreatingNewPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var mainProduct = Helper.CreateProduct(data.Org1, "MP1");
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;
			var bomComponentProduct = Helper.CreateProduct(data.Org1, "BOM1");
			var bomPartForMainProduct = Helper.CreateProductBOM(mainProduct, bomComponentProduct, 3m, Constants.PkgUnit.Unit);

			var inventoryLocation = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, mainProduct, 5m, inventoryLocation);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, bomComponentProduct, 20m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, mainProduct, 15m);
			Factory.Save();

			orderLine.Validation.ValidateWE_ShortfallQuantityCached();
			AssertEquals("Shortfall 4, as 5 main product + 6 mainProduct can be created from 20 bomComponentProduct", 4m, orderLine.WE_ShortfallQuantityCached);

			bomPartForMainProduct.OE_ComponentQty = 5m;
			var pick = Helper.CreatePickNew(orderLine.PickableDocket);
			var pickOrderInventoryForMainProduct = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().SingleOrDefault(i => i.SupplierPart.PK == mainProduct.PK);
			AssertEquals("Component quantity is 5 and can build 4 product, so 9 product is available. Shortfall is 6.", 6m, pickOrderInventoryForMainProduct.QuantityShort);
		}

		public void TestQuantityShortInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.QuantityShort, OrderedInventory.QuantityShortInfo.Name);
		}

		#endregion

		#region TestMinimumShelfLife

		public void TestMinimumShelfLife_JulianBatchNumberPartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var consignee = Helper.CreateClient("CONSIGNEE");
			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.ConsigneePK = consignee.PK;
			var orderLine = order.Lines[0];
			var orderedInventory = (WhsPickOrderedInventory)GetNewBusinessObject();

			AssertEquals("Precondition", (ZShort)0, orderedInventory.MinimumShelfLife);

			orderedInventory.Owners.Add(orderLine);
			AssertEquals((ZShort)10, orderedInventory.MinimumShelfLife);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 5;
			AssertEquals((ZShort)5, orderedInventory.MinimumShelfLife);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			AssertEquals("Value remains as expiry date is still enabled on the product after attribute type is updated to non-julian batch number.",
				(ZShort)5, orderedInventory.MinimumShelfLife);
		}

		public void TestMinimumShelfLife_ExpiryDatePartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			var consignee = Helper.CreateClient("CONSIGNEE");
			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.ConsigneePK = consignee.PK;
			var orderLine = order.Lines[0];
			var orderedInventory = (WhsPickOrderedInventory)GetNewBusinessObject();

			AssertEquals("Precondition", (ZShort)0, orderedInventory.MinimumShelfLife);

			orderedInventory.Owners.Add(orderLine);
			AssertEquals((ZShort)10, orderedInventory.MinimumShelfLife);

			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 5;
			AssertEquals((ZShort)5, orderedInventory.MinimumShelfLife);
		}

		#endregion

		#region TestPackageGroupId

		public void TestPackageGroupId()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			orderLine1.WE_PackageGroupId = "ABC";

			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("ABC", orderedInventory.PackageGroupId);
		}

		#endregion

		#region TestGetClientPKFast

		public void TestGetClientPKFast()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-1");

			var client2 = Helper.CreateClient("222", "222");
			var part2 = Helper.CreateProduct(client2, "P432");
			Helper.CreateWhsReceiveWithInventory(client2, data.Whs1, "R2", part2, 10m, data.Whs1.DefaultLocation, "PLT-2");
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderline1 = Helper.CreateWhsOrderLine(order1, data.Part1, 20m);
			var order2 = Helper.CreateWhsOrder(client2, data.Whs1);
			var orderline2 = Helper.CreateWhsOrderLine(order2, part2, 20m);
			var pick = Helper.CreatePickNew(order1, order2);

			var orderedInventories = pick.OrderedInventories;
			AssertEquals("Pick has 2 ordered inventories", 2, orderedInventories.Count);
			AssertEquals("GetClientPKFast returns correct PK", data.Org1.PK, orderedInventories[0].GetClientPKFast(orderline1));
			AssertEquals("GetClientPKFast returns correct PK", client2.PK, orderedInventories[0].GetClientPKFast(orderline2));
		}

		#endregion

		#endregion

		#region Picking

		#region TestClearAllocationsValidatesShortfallQuantity

		public void TestClearAllocationsValidatesShortfallQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Precondition: Stock is picked.", 10m, orderedInventory.PickLineQuantity);
			AssertNoWarnings(orderLine.WE_ShortfallQuantityCachedInfo);
			AssertContainsExactElementsInAnyOrder("Precondition:", new[] { orderLine }, orderedInventory.Owners);

			var poke = orderLine.WE_ShortfallQuantityCached; // need to make sure it's cached.
			orderedInventory.ClearAllocations();
			AssertHasWarning(orderLine.WE_ShortfallQuantityCachedInfo, "Shortfall: Only 0 unit(s) have been selected for release");
		}

		#endregion

		#region TestClearAllocationsClearsReleaseLinesCache

		public void TestClearAllocationsClearsReleaseLinesCache()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];

			int validationHitCount = 0;
			var availableInventory = orderedInventory.AvailableInventories[0];
			availableInventory.PickLineQuantityInfo.AdditionalValidation += () => validationHitCount++;
			AssertEquals("Precondition: Stock is picked.", 10m, orderedInventory.PickLineQuantity);
			AssertEquals("Precondition: Release Lines are registered child editable.", true, orderLine.IsRegisteredEditableChildObject(orderLine.ReleaseLines));

			orderedInventory.ClearAllocations();
			AssertEquals("ClearAllocations() should have cleared Release Lines.", false, orderLine.IsRegisteredEditableChildObject(orderLine.ReleaseLines));
			AssertEquals("ClearAllocations() should suspend Validation on AvailableInventory.", 0, validationHitCount);
		}

		#endregion

		#region TestClearAllocations_AllowsRemovingPackedPickLines

		public void TestClearAllocations_AllowsRemovingPackedPickLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals("Precondition: Stock is picked.", 10m, orderedInventory.PickLineQuantity);

			var releaseLine = orderLine.ReleaseLines[0];
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, 10m);

			var availableInventory = orderedInventory.AvailableInventories[0];
			availableInventory.PickLineQuantity = 0m;
			AssertEquals("Should not allow the Pick Line to be deleted while packed.", 10m, availableInventory.PickLineQuantity);
			AssertEquals("Should not allow the Pick Line to be deleted while packed.", 10m, orderLine.PickLineQuantity);

			orderedInventory.ClearAllocations();
			AssertEquals("Should allow the Pick Line to be deleted while packed when using ClearAllocations.", 0m, availableInventory.PickLineQuantity);
			AssertEquals("Should allow the Pick Line to be deleted while packed when using ClearAllocations.", 0m, orderLine.PickLineQuantity);
		}

		#endregion

		#region TestPickFromCrossedDockLines

		public void TestPickFromCrossedDockLines()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryManyLines(10m, 20m, 30m, 40m, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 55m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, Helper.CreateProduct(data.Org1, "PRODUCT"), 30m);

			var pickLine1 = Helper.CreateReservePickLine(orderLine1, data.Receive11.Inventory[4], 50m);
			var pickLine2 = Helper.CreateReservePickLine(orderLine1, data.Receive11.Inventory[3], 5m);
			AssertEquals("Should have stocked reserved.", 55m, orderLine1.WE_CrossDockQuantity);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			var orderedInventory1 = pick.OrderedInventories[0];
			AssertEquals(55m, orderedInventory1.PickLineQuantity);
			AssertEquals("Attaching an Order should allocate reserved stock.", 0m, orderLine1.WE_CrossDockQuantity);
			AssertContainsExactElementsInAnyOrder("Attaching an Order should allocate reserved stock.", new[] { pickLine1, pickLine2 },
				orderedInventory1.AvailableInventories.Cast<WhsPickAvailableInventory>().SelectMany(a => a.PickLines));

			// order line 1 should have allocated 5 units from the cross docked 4th inventory line
			AssertEquals(5m, orderedInventory1.AvailableInventories[3].PickLineQuantity);
			AssertEquals(1, orderedInventory1.AvailableInventories[3].PickLines.Count());
			AssertEquals(pickLine2, orderedInventory1.AvailableInventories[3].PickLines.ElementAt(0));

			// order line 1 should have allocated 50 units from the cross docked 5th inventory line
			AssertEquals(50m, orderedInventory1.AvailableInventories[4].PickLineQuantity);
			AssertEquals(1, orderedInventory1.AvailableInventories[4].PickLines.Count());
			AssertEquals(pickLine1, orderedInventory1.AvailableInventories[4].PickLines.ElementAt(0));

			pick.Orders.Remove(order);
			AssertEquals("Removing an Order, should re-instate reserved stock.", 55m, orderLine1.WE_CrossDockQuantity);
			AssertEquals("Removing an Order, should re-instate reserved stock.", 0, pick.OrderedInventories.Count);
			AssertEquals("Removing an Order, should re-instate reserved stock.", 50m, pickLine1.ReservedQuantity);
			AssertEquals("Removing an Order, should re-instate reserved stock.", 5m, pickLine2.ReservedQuantity);

			pick.PickOrdersWithAllocationMock(order);
			var orderedInventory2 = pick.OrderedInventories[0];
			AssertEquals(55m, orderedInventory2.PickLineQuantity);
			AssertEquals(5m, orderedInventory2.AvailableInventories[3].PickLineQuantity);
			AssertEquals(1, orderedInventory2.AvailableInventories[3].PickLines.Count());
			AssertEquals(50m, orderedInventory2.AvailableInventories[4].PickLineQuantity);
			AssertEquals(1, orderedInventory2.AvailableInventories[4].PickLines.Count());
		}

		#endregion

		#region TestPickWarnsIfCrossDockedInventoryNotFinalised

		public void TestPickWarnsIfCrossDockedInventoryNotFinalised()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventoryNoAllocate();

			// create this Receive to ensure pick still picks something if cross docket inventory is unavailable
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals(true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			// cross dock the inventory in data.Receive11 that has not arrived yet, so should not be picked
			data.Receive11.RunPreSaveValidation(); // Creates Inventory Docket Lines
			Helper.CreateReservePickLine(orderLine1, data.Receive11.Inventory[0], 50m);
			Factory.Save();

			var pick = Factory.New<WhsPick>();
			pick.PickOrdersWithAllocationMock(order);
			OrderedInventory = pick.OrderedInventories[0];
			AssertEquals(50m, OrderedInventory.PickLineQuantity);
			AssertEquals(OrderedInventory.AvailableInventories[0].PickLines.ElementAt(0).WZ_WE_InventoryLine, receive.Inventory[0].WI_WE_InDocketLine);
			AssertHasWarning(OrderedInventory.QuantityShortInfo, "Some product could not be picked from cross dock allocations because either the product has not yet arrived (and the receipt is not finalized), the product is not available for picking, or it has already been picked");
		}

		#endregion

		#region TestCrossDockedInventoryHeldInventory

		public void TestPickWarnsIfCrossDockedInventoryNotAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, data.Whs1.DefaultLocation, "", InventoryStatus.Codes.Held);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Assert(receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 50m);

			// cross dock the inventory in data.Receive11 that is not available
			var reservedPickLine1 = Helper.CreateReservePickLine(orderLine1, inventory1, 10m);
			var reservedPickLine2 = Helper.CreateReservePickLine(orderLine1, inventory2, 15m);
			var reservedPickLine3 = Helper.CreateReservePickLine(orderLine1, inventory3, 20m);
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			AssertEquals(20m, orderedInventory.PickLineQuantity);
			AssertContainsExactElementsInAnyOrder("Reserved Pick Lines should not be deleted.", new[] { reservedPickLine1, reservedPickLine2, reservedPickLine3 }, pick.GetAllPickLines());

			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
			AssertContainsExactElementsInAnyOrder(new[] { reservedPickLine3 }, availableInventory.PickLines);
			AssertContainsExactElementsInAnyOrder(new[] { reservedPickLine1, reservedPickLine2, reservedPickLine3 }, orderedInventory.Owners.Single().PickLines);
			AssertEquals("Once order line is picked, it no longer has any reserved pick lines.", 0, orderedInventory.Owners.Single().ReservedPickLines.Count);
			AssertEquals(20m, availableInventory.PickLineQuantity);
			AssertEquals("Should set un-pickable reserved pickline's units to zero.", 0m, reservedPickLine1.WZ_Units);
			AssertEquals("Should set un-pickable reserved pickline's units to zero.", 0m, reservedPickLine2.WZ_Units);
			AssertEquals("Should have picked from reserved picklines.", 20m, reservedPickLine3.WZ_Units);
			AssertHasWarning(orderedInventory.QuantityShortInfo, "Some product could not be picked from cross dock allocations because either the product has not yet arrived (and the receipt is not finalized), the product is not available for picking, or it has already been picked");
		}

		#endregion

		#endregion

		#region TestAdjustPickAndMetLines

		public void TestAdjustDownPickAndAttributeMetLinesToMatchOrderQty_QuantityOrderedChangedForSingleInventoryAndOrderLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			WhsPick pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - ensure units were picked", 10m, orderLine.PickLineQuantity);
			AssertEquals("Precondition - ensure units were met", 10m, orderLine.SumOfUnitsMet);

			orderLine.WE_TransactionQuantity = 15m;
			((IWhsPickOrderedInventoryInternals)pick.OrderedInventories[0]).AdjustDownPickAndAttributeMetLinesToMatchOrderQty();
			AssertEquals("PickLineQuantity should not be affected by increase units on OrderLine", 10m, orderLine.PickLineQuantity);
			AssertEquals("SumOfUnitsMet should not be affected by increase units on OrderLine", 10m, orderLine.SumOfUnitsMet);

			orderLine.WE_TransactionQuantity = 8m;
			((IWhsPickOrderedInventoryInternals)pick.OrderedInventories[0]).AdjustDownPickAndAttributeMetLinesToMatchOrderQty();
			AssertEquals("PickLineQuantity should be reduced by decrease units on OrderLine", 8m, orderLine.PickLineQuantity);
			AssertEquals("SumOfUnitsMet should be reduced by decrease units on OrderLine", 8m, orderLine.SumOfUnitsMet);
		}

		public void TestAdjustDownPickAndAttributeMetLinesToMatchOrderQty_OrderQtyReduced()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 21m);
			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(orderLine.ReleaseLines[0], 13m);
			Factory.Save();

			orderLine.WE_TransactionQuantity = 8m;
			((IWhsPickOrderedInventoryInternals)pick.OrderedInventories[0]).AdjustDownPickAndAttributeMetLinesToMatchOrderQty();
			AssertEquals("PickLineQuantity should be reduced by decrease units on OrderLine, but not less than already packed", 13m, orderLine.PickLineQuantity);
			AssertEquals("SumOfUnitsMet should be reduced by decrease units on OrderLine, but not less than already packed", 13m, orderLine.SumOfUnitsMet);
		}

		public void TestAdjustPickAndMetLines_QuantityOrderedChangedForMultipleInventoryAndOrderLines_PickLineQuantityCouldBeAdjustedIntoOtherOrderLine()
		{
			TestAdjustPickAndMetLines_QuantityOrderedChangedForMultipleInventoryAndOrderLinesCore(2m, 8m, 2m);
		}

		public void TestAdjustPickAndMetLines_QuantityOrderedChangedForMultipleInventoryAndOrderLines_PickLineQuantityCouldBePartiallyAdjustedIntoOtherOrderLine()
		{
			TestAdjustPickAndMetLines_QuantityOrderedChangedForMultipleInventoryAndOrderLinesCore(1m, 8m, 1m);
		}

		void TestAdjustPickAndMetLines_QuantityOrderedChangedForMultipleInventoryAndOrderLinesCore(ZDecimal orderLine2Units, ZDecimal expectedOrderLine1PickLineQuantityAndMet, ZDecimal expectedOrderLine2PickLineQuantityAndMet)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			WhsReceive receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m); // to ensure we have a PickLineAvailable that contain multiple Inventory.
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m).WI_PalletID = "PLT-1"; // to ensure there we have multiple PickLineAvailable.
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m).WI_PalletID = "PLT-2";
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			WhsOrder order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			WhsOrderLine orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			WhsPick pick = Helper.CreatePickNew(order);

			WhsOrderLine orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, orderLine2Units);
			pick.OrderedInventories[0].Owners.Add(orderLine2); // adding second line forcefully since the cache update is on SyncPickWithOrder() and not in AdjustPickAndMetLines();

			AssertEquals("Precondition - ensure units were picked", 10m, orderLine1.PickLineQuantity);
			AssertEquals("Precondition - ensure units were met", 10m, orderLine1.SumOfUnitsMet);
			AssertEquals("Precondition - ensure units were not picked", 0m, orderLine2.PickLineQuantity);
			AssertEquals("Precondition - ensure units were not met", 0m, orderLine2.SumOfUnitsMet);

			orderLine1.WE_TransactionQuantity = 15m;
			((IWhsPickOrderedInventoryInternals)pick.OrderedInventories[0]).AdjustDownPickAndAttributeMetLinesToMatchOrderQty();
			AssertEquals("PickLineQuantity should not be affected by increase units on OrderLine", 10m, orderLine1.PickLineQuantity);
			AssertEquals("SumOfUnitsMet should not be affected by increase units on OrderLine", 10m, orderLine1.SumOfUnitsMet);
			AssertEquals("PickLineQuantity should not be affected by increase units on OrderLine", 0m, orderLine2.PickLineQuantity);
			AssertEquals("SumOfUnitsMet should not be affected by increase units on OrderLine", 0m, orderLine2.SumOfUnitsMet);

			orderLine1.WE_TransactionQuantity = 8m;
			((IWhsPickOrderedInventoryInternals)pick.OrderedInventories[0]).AdjustDownPickAndAttributeMetLinesToMatchOrderQty();
			AssertEquals("PickLineQuantity should be adjusted by decrease units on OrderLine", expectedOrderLine1PickLineQuantityAndMet, orderLine1.PickLineQuantity);
			AssertEquals("SumOfUnitsMet should be adjusted by decrease units on OrderLine", expectedOrderLine1PickLineQuantityAndMet, orderLine1.SumOfUnitsMet);
			AssertEquals("PickLineQuantity should be adjusted by decrease units on OrderLine", expectedOrderLine2PickLineQuantityAndMet, orderLine2.PickLineQuantity);
			AssertEquals("SumOfUnitsMet should be adjusted by decrease units on OrderLine", expectedOrderLine2PickLineQuantityAndMet, orderLine2.SumOfUnitsMet);
		}

		#endregion

		#region TestClearPickLinesCacheForInventory

		public void TestClearPickLinesCacheForInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-1");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-2");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = orderLine.PickLines.Single(pl => pl.InventoryLine.WE_PalletID == "PLT-1");
			var pickLine2 = orderLine.PickLines.Single(pl => pl.InventoryLine.WE_PalletID == "PLT-2");
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory1 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PalletID == "PLT-1");
			var availableInventory2 = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PalletID == "PLT-2");
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);

			var transferLine1 = Factory.New<WhsTransferLine>();
			var transferLine2 = Factory.New<WhsTransferLine>();
			var newPickLine1 = transferLine1.PickLines.AddNew();
			var newPickLine2 = transferLine2.PickLines.AddNew();
			newPickLine1.WZ_WE_InventoryLine = pickLine1.WZ_WE_InventoryLine;
			newPickLine2.WZ_WE_InventoryLine = pickLine2.WZ_WE_InventoryLine;
			pickLine1.WZ_WE_InventoryLine = transferLine1.PK;
			pickLine1.WZ_WE_OriginalPickedInventoryLine = receive1.Lines[0].PK;
			pickLine2.WZ_WE_InventoryLine = transferLine2.PK;
			pickLine2.WZ_WE_OriginalPickedInventoryLine = receive2.Lines[0].PK;
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);

			orderedInventory.ClearPickLinesCacheForInventory(receive1.Lines[0].PK);
			AssertContainsExactElementsInAnyOrder(new[] { newPickLine1 }, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine1 }, availableInventory1.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);

			orderedInventory.ClearPickLinesCacheForInventory(receive2.Lines[0].PK);
			AssertContainsExactElementsInAnyOrder(new[] { newPickLine2 }, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesForPickingDetails);
			AssertContainsExactElementsInAnyOrder(new[] { pickLine2 }, availableInventory2.AvailableInventoriesSplitByPickedDetails[0].PickLinesOnOrder);
		}

		#endregion

		#region TestGetAvailableInventories_SerialNumber

		public void TestGetAvailableInventories_SerialNumber_OneSelected()
		{
			TestGetAvailableInventories_SerialNumberCore(orderQuantity: 1, new[] { 2, 3, 1, 5, 4 });
		}

		public void TestGetAvailableInventories_SerialNumber_PartialSelected()
		{
			TestGetAvailableInventories_SerialNumberCore(orderQuantity: 3, new[] { 1, 5, 2, 4, 3 });
		}

		public void TestGetAvailableInventories_SerialNumber_AllSelected()
		{
			TestGetAvailableInventories_SerialNumberCore(orderQuantity: 5, new[] { 3, 1, 2, 5, 4 });
		}

		void TestGetAvailableInventories_SerialNumberCore(int orderQuantity, int[] serialNumbersOrder)
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R01");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation);
				foreach (var i in serialNumbersOrder)
				{
					var pivot = receiveLine.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{i}";
				}
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, orderQuantity);
				var pick = Helper.CreatePickNew(order);

				var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
				var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
				availableInventory.Allocate = true;
				var allSerialNumbers = new[] { "SN1", "SN2", "SN3", "SN4", "SN5" };
				AssertContainsExactElementsInExactOrder(allSerialNumbers, availableInventory.SerialNumbers.Select(s => s.SerialNumberValue));
				var expectedSelectedSerialNumbers = Enumerable.Range(1, orderQuantity).Select(i => $"SN{i}");
				AssertContainsExactElementsInExactOrder(expectedSelectedSerialNumbers, availableInventory.SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));

				availableInventory.Allocate = false;
				AssertContainsExactElementsInExactOrder(allSerialNumbers, availableInventory.SerialNumbers.Select(s => s.SerialNumberValue));
				AssertContainsExactElementsInExactOrder(Enumerable.Empty<WhsSerialNumberPivot>(), availableInventory.SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
			}
		}

		#endregion

		#region TestGetAvailableInventories_ChangeSelectedSerialNumber

		public void TestGetAvailableInventories_ChangeSelectedSerialNumber()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var arrivalDate = ZDateTimeOffset.Today;
				for (var k = 1; k <= 2; k++)
				{
					var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R0{k}");
					var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 3m, data.Whs1.DefaultLocation);
					receive.WD_ArrivalDate = arrivalDate.AddDays(-k);
					for (var i = 1; i <= 3; i++)
					{
						var pivot = receiveLine.SerialNumbers.AddNew();
						pivot.SerialNumberValue = $"SN{k}{i}";
					}
					receive.FinaliseDocket();
					AssertIsFinalisedPrecondition(receive);
				}
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1);
				var pick = Helper.CreatePickNew(order);

				var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
				var availableInventories = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
				availableInventories[0].Allocate = true;
				var allSerialNumbers = new[] { "SN21", "SN22", "SN23" };
				AssertContainsExactElementsInExactOrder(allSerialNumbers, availableInventories[0].SerialNumbers.Select(s => s.SerialNumberValue));
				AssertContainsExactElementsInExactOrder(new[] { "SN21" }, availableInventories[0].SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
				var pickLine = pick.GetAllPickLines().Single();
				Factory.Save();
				AssertEquals(pickLine.PK, GetPivotWithSetPickingLine().Single().WSV_WZ_PickingLine);

				availableInventories[0].Allocate = false;
				AssertContainsExactElementsInExactOrder(Enumerable.Empty<WhsSerialNumberPivot>(), availableInventories[0].SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));

				availableInventories[1].SerialNumbers[0].Selected = true;
				AssertContainsExactElementsInExactOrder(new[] { "SN11" }, availableInventories[1].SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
				Factory.Save();

				var newPickLine = pick.GetAllPickLines().Single();
				AssertNotEquals(pickLine.PK, newPickLine.PK);
				AssertEquals(newPickLine.PK, GetPivotWithSetPickingLine().Single().WSV_WZ_PickingLine);
			}

			WhsSerialNumberPivot[] GetPivotWithSetPickingLine()
			{
				return NewFactory().Load<WhsSerialNumberPivot>(new ZQuery(WhsSerialNumberPivotSchema.WSV_WZ_PickingLine, SQLComparisonOperator.NotEqual, null));
			}
		}

		public void TestGetAvailableInventories_ChangeSelectedSerialNumber_Delete_RandomOrder()
		{
			TestGetAvailableInventories_ChangeSelectedSerialNumber_Delete_Core(["SN1", "SN3", "SN2"]);
		}

		public void TestGetAvailableInventories_ChangeSelectedSerialNumber_Delete_DescendingOrder()
		{
			TestGetAvailableInventories_ChangeSelectedSerialNumber_Delete_Core(["SN3", "SN2", "SN1"]);
		}

		public void TestGetAvailableInventories_ChangeSelectedSerialNumber_Delete_AscendingOrder()
		{
			TestGetAvailableInventories_ChangeSelectedSerialNumber_Delete_Core(["SN1", "SN2", "SN3"]);
		}

		void TestGetAvailableInventories_ChangeSelectedSerialNumber_Delete_Core(ZString[] serialNumbers)
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var arrivalDate = ZDateTimeOffset.Today.AddDays(-1);
				for (var i = 0; i < serialNumbers.Length; i++)
				{
					var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, $"R{i}");
					var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
					receive.WD_ArrivalDate = arrivalDate;
					var pivot = receiveLine.SerialNumbers.AddNew();
					pivot.SerialNumberValue = serialNumbers[i];
					receive.FinaliseDocket();
					AssertIsFinalisedPrecondition(receive);
				}
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2);
				var pick = Helper.CreatePickNew(order);

				var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
				var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();
				availableInventory.Allocate = true;
				var allSerialNumbers = new[] { "SN1", "SN2", "SN3" };
				AssertContainsExactElementsInExactOrder(allSerialNumbers, availableInventory.SerialNumbers.Select(s => s.SerialNumberValue));
				AssertContainsExactElementsInExactOrder(new[] { "SN1", "SN2" }, availableInventory.SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
				Factory.Save();

				availableInventory.SerialNumbers[1].Selected = false;
				AssertContainsExactElementsInExactOrder(new[] { "SN1" }, availableInventory.SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
				pick.WP_PercentageComplete = (ZByte)12;
				Factory.Save();
			}
		}

		#endregion

		#region TestGetAvailableInventories_SerialNumber_Sort

		public void TestGetAvailableInventories_SerialNumber_Sort1()
		{
			TestGetAvailableInventories_SerialNumber_SortCore(new[] { 3, 1, 2, 5, 4 }, new[] { "SN1", "SN3" }, new[] { "SN2", "SN4" });
		}

		public void TestGetAvailableInventories_SerialNumber_Sort2()
		{
			TestGetAvailableInventories_SerialNumber_SortCore(new[] { 1, 5, 2, 4, 3 }, new[] { "SN1", "SN5" }, new[] { "SN2", "SN3" });
		}

		public void TestGetAvailableInventories_SerialNumber_Sort3()
		{
			TestGetAvailableInventories_SerialNumber_SortCore(new[] { 4, 1, 2, 5, 3 }, new[] { "SN1", "SN4" }, new[] { "SN2", "SN3" });
		}

		void TestGetAvailableInventories_SerialNumber_SortCore(int[] serialNumbersOrder, string[] expectedSNFirst, string[] expectedSNSecond)
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var today = ZDateTimeOffset.Today;
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 2, true, false);
				var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R02", data.Part1, 3, true, false);
				var receiveLine1 = receive1.Lines[0];
				receive1.WD_ArrivalDate = today.AddDays(-1);
				foreach (var i in serialNumbersOrder.Take(2))
				{
					var pivot = receiveLine1.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{i}";
				}
				receive1.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive1);
				Factory.Save();

				var receiveLine2 = receive2.Lines[0];
				receive2.WD_ArrivalDate = today;
				foreach (var i in serialNumbersOrder.Skip(2))
				{
					var pivot = receiveLine2.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{i}";
				}
				receive2.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive2);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2);
				var pick = Helper.CreatePickNew(order);

				AssertNotEquals("Precondition", receive1.WD_ArrivalDate, receive2.WD_ArrivalDate);

				var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
				var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
				var availableInventoryFirst = availableInventory[0];
				var availableInventorySecond = availableInventory[1];
				availableInventoryFirst.Allocate = true;
				AssertContainsExactElementsInExactOrder(expectedSNFirst, availableInventoryFirst.SerialNumbers.Select(s => s.SerialNumberValue));
				AssertContainsExactElementsInExactOrder(expectedSNFirst, availableInventoryFirst.SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));

				availableInventoryFirst.Allocate = false;
				availableInventorySecond.Allocate = true;
				AssertContainsExactElementsInExactOrder(receiveLine2.SerialNumbers.Select(s => s.SerialNumberValue).OrderBy(i => i), availableInventorySecond.SerialNumbers.Select(s => s.SerialNumberValue));
				AssertContainsExactElementsInExactOrder(expectedSNSecond, availableInventorySecond.SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
				AssertContainsExactElementsInExactOrder(Enumerable.Empty<WhsSerialNumberPivot>(), availableInventoryFirst.SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
			}
		}

		#endregion

		#region TestGetAvailableInventories_SerialNumber_ManualSerialNumbersSelection

		public void TestGetAvailableInventories_SerialNumber_ManualSerialNumbersSelection()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R01", data.Part1, 5, true, false);
				var receiveLine = receive.Lines[0];
				for (var i = 1; i <= 5; i++)
				{
					var pivot = receiveLine.SerialNumbers.AddNew();
					pivot.SerialNumberValue = $"SN{i}";
				}
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2);
				var pick = Helper.CreatePickNew(order);

				var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
				var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single();

				var manualSelectionHits = 0;
				var throwException = false;
				availableInventory.SerialNumbers[0].SelectedInfo.ValueChanged += (s, e) =>
				{
					manualSelectionHits++;
				};

				AssertContainsExactElementsInExactOrder(new[] { "SN1", "SN2", "SN3", "SN4", "SN5" }, availableInventory.SerialNumbers.Select(s => s.SerialNumberValue));
				AssertContainsExactElementsInExactOrder(new[] { "SN1", "SN2" }, availableInventory.SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));
				AssertEquals("Precondition", 0, manualSelectionHits);

				var serialNumberSN1 = availableInventory.SerialNumbers.Cast<WhsSerialNumberPivotSelector>().Single(s => s.SerialNumberValue == "SN1");
				var serialNumberPivotSN1 = receiveLine.SerialNumbers.Single(s => s.SerialNumberValue == "SN1");
				serialNumberPivotSN1.WSV_WZ_PickingLineInfo.ValueChanged += (s, e) =>
				{
					if (throwException)
					{
						throw new Exception("Oops!");
					}
				};

				serialNumberSN1.Selected = false;
				AssertEquals("SN1 hits.", 1, manualSelectionHits);
				AssertContainsExactElementsInExactOrder(new[] { "SN2" }, availableInventory.SerialNumbers.Where(s => s.Selected).Select(s => s.SerialNumberValue));

				throwException = true;
				AssertExceptionThrown(typeof(Exception), () => serialNumberSN1.Selected = true);
				AssertEquals("SN1 not hits again.", 1, manualSelectionHits);
			}
		}

		#endregion

		#region TestPerformance_SerialNumber_DBHits

		public void TestPerformance_SerialNumber_DBHits()
		{
			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var today = ZDateTimeOffset.Today;
				int numberOfDocketLines = 10;
				var data = new TestDataSimpleEnvironment(Factory);
				Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

				var expectedSerialNumbers = new List<ZString>();
				for (int n = 0; n < numberOfDocketLines; n++)
				{
					var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R0{n}", data.Part1, 5, true, false);
					var receiveLine = receive.Lines[0];
					receive.WD_ArrivalDate = today.AddDays(-n);
					for (int i = 1; i <= 5; i++)
					{
						var pivot = receiveLine.SerialNumbers.AddNew();
						pivot.SerialNumberValue = $"SN{n}{i}";
						expectedSerialNumbers.Add($"SN{n}{i}");
					}
					receive.FinaliseDocket();
					AssertIsFinalisedPrecondition(receive);
				}
				Factory.Save();

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 50);
				var pick = Helper.CreatePickNew(order);
				foreach (var availableInventory in pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>())
				{
					availableInventory.Allocate = false;
				}
				Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var expectedDBHits = new Dictionary<string, int>
				{
					{ OrgAddressSchema.Constants.TableName, 2 },
					{ OrgHeaderSchema.Constants.TableName, 1 },
					{ OrgMiscServSchema.Constants.TableName, 1 },
					{ OrgSupplierPartSchema.Constants.TableName, 1 },
					{ WhsDocketSchema.Constants.TableName, 2 },
					{ WhsDocketLineSchema.Constants.TableName, 2 },
					{ WhsInventoryViewSchema.Constants.TableName, 1 },
					{ WhsLocationViewSchema.Constants.TableName, 1 },
					{ WhsPickLineSchema.Constants.TableName, 1 },
					{ WhsPickSchema.Constants.TableName, 1 },
					{ WhsSerialNumberSchema.Constants.TableName, 1 },
					{ WhsSerialNumberPivotSchema.Constants.TableName, 1 },
					{ OrgPartRelationSchema.Constants.TableName, 1 },
					{ RefPackTypeSchema.Constants.TableName, 1 },
					{ WhsPickFaceSchema.Constants.TableName, 1 },
					{ WhsWarehouseSchema.Constants.TableName, 1 },
				};

				var serialNumbers = new List<ZString>();
				using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits, otherFactory))
				using (RowFactory.SetCachedTables())
				{
					var pickInOtherFactory = otherFactory.Load<WhsPick>(pick.PK);
					foreach (var availableInventory in pickInOtherFactory.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>())
					{
						availableInventory.Allocate = false;
					}

					foreach (WhsPickAvailableInventory availableInventory in pickInOtherFactory.OrderedInventories[0].AvailableInventories)
					{
						availableInventory.SerialNumbers.Select(s => _ = $"{s.Selected}{s.SerialNumberValue}");
						serialNumbers.AddRange(availableInventory.SerialNumbers.Select(s => s.SerialNumberValue));
					}
				}

				AssertContainsExactElementsInAnyOrder(expectedSerialNumbers, serialNumbers);
			}
		}

		#endregion

		// interfaces

		#region ILineAttributes Members

		[ExpectException(typeof(NotSupportedException))]
		public void TestILineAttributesSetAttributes(ILineAttributes src)
		{
			OrderedInventory.SetAttributes(new TestILineAttributes());
		}

		public void TestExpiryDate()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(ZDateTime.Today.AddDays(1), OrderedInventory.ExpiryDate);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(ZDateTime.Empty, OrderedInventory.ExpiryDate);
		}

		public void TestPackingDate()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(ZDateTime.Today.AddDays(-1), OrderedInventory.PackingDate);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(ZDateTime.Empty, OrderedInventory.PackingDate);
		}

		public void TestBondedEntryKey()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("BEK-1", OrderedInventory.BondedEntryKey);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.BondedEntryKey);
		}

		public void TestPartAttrib1()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("PA1", OrderedInventory.PartAttrib1);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.PartAttrib1);
		}

		public void TestPartAttrib2()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("PA2", OrderedInventory.PartAttrib2);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.PartAttrib2);
		}

		public void TestPartAttrib3()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("PA3", OrderedInventory.PartAttrib3);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.PartAttrib3);
		}

		public void TestSerialNumber()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("SN1", OrderedInventory.SerialNumber);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.SerialNumber);
		}

		public void TestAllocationKey_NoInventory()
		{
			var orderedInv = (WhsPickOrderedInventory)GetNewBusinessObject();
			AssertEquals("Precondition: Empty.", string.Empty, orderedInv.AllocationKey);
		}

		public void TestAllocationKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			order.Lines[0].WE_AllocationKey = "ALO-123";
			var pick = Helper.CreatePickNew(order);

			var orderedInv = pick.OrderedInventories[0];
			AssertEquals("Should have an allocation key.", "ALO-123", orderedInv.AllocationKey);
		}

		public void TestExpiryDateInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.ExpiryDate, OrderedInventory.ExpiryDateInfo.Name);
		}

		public void TestPackingDateInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.PackingDate, OrderedInventory.PackingDateInfo.Name);
		}

		public void TestBondedEntryKeyInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.BondedEntryKey, OrderedInventory.BondedEntryKeyInfo.Name);
		}

		public void TestPartAttrib1Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.PartAttrib1, OrderedInventory.PartAttrib1Info.Name);
		}

		public void TestPartAttrib2Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.PartAttrib2, OrderedInventory.PartAttrib2Info.Name);
		}

		public void TestPartAttrib3Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.PartAttrib3, OrderedInventory.PartAttrib3Info.Name);
		}

		public void TestSerialNumberInfo()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.SerialNumber, OrderedInventory.SerialNumberInfo.Name);
		}

		public void TestAllocationKeyInfo()
		{
			var orderedInv = (WhsPickOrderedInventory)GetNewBusinessObject();
			AssertEquals(WhsPickOrderedInventory.Schema.AllocationKey, orderedInv.AllocationKeyInfo.Name);
		}

		#endregion

		#region ILineCustomAttributes Members

		[ExpectException(typeof(NotSupportedException))]
		public void TestILineCustomAttributesSetCustomAttributes(ILineAttributes src)
		{
			OrderedInventory.SetCustomAttributes(new TestILineCustomAttributes());
		}

		public void TestCustomAttrib1()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("CA1", OrderedInventory.CustomAttrib1);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.CustomAttrib1);
		}

		public void TestCustomAttrib2()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("CA2", OrderedInventory.CustomAttrib2);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.CustomAttrib2);
		}

		public void TestCustomAttrib3()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("CA3", OrderedInventory.CustomAttrib3);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.CustomAttrib3);
		}

		public void TestCustomAttrib4()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("CA4", OrderedInventory.CustomAttrib4);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.CustomAttrib4);
		}

		public void TestCustomAttrib5()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("CA5", OrderedInventory.CustomAttrib5);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.CustomAttrib5);
		}

		public void TestCustomAttrib6()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("CA6", OrderedInventory.CustomAttrib6);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.CustomAttrib6);
		}

		public void TestCustomDecimal1()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(1.1m, OrderedInventory.CustomDecimal1);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(0m, OrderedInventory.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(2.2m, OrderedInventory.CustomDecimal2);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(0m, OrderedInventory.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(3.3m, OrderedInventory.CustomDecimal3);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(0m, OrderedInventory.CustomDecimal3);
		}

		public void TestCustomDecimal4()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(4.4m, OrderedInventory.CustomDecimal4);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(0m, OrderedInventory.CustomDecimal4);
		}

		public void TestCustomDecimal5()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(5.5m, OrderedInventory.CustomDecimal5);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(0m, OrderedInventory.CustomDecimal5);
		}

		public void TestCustomDate1()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(ZDateTime.Today.AddDays(1), OrderedInventory.CustomDate1);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(ZDateTime.Empty, OrderedInventory.CustomDate1);
		}

		public void TestCustomDate2()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(ZDateTime.Today.AddDays(2), OrderedInventory.CustomDate2);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(ZDateTime.Empty, OrderedInventory.CustomDate2);
		}

		public void TestCustomDate3()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(ZDateTime.Today.AddDays(3), OrderedInventory.CustomDate3);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(ZDateTime.Empty, OrderedInventory.CustomDate3);
		}

		public void TestCustomDate4()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(ZDateTime.Today.AddDays(4), OrderedInventory.CustomDate4);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(ZDateTime.Empty, OrderedInventory.CustomDate4);
		}

		public void TestCustomDate5()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(ZDateTime.Today.AddDays(5), OrderedInventory.CustomDate5);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(ZDateTime.Empty, OrderedInventory.CustomDate5);
		}

		public void TestCustomFlag1()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(true, OrderedInventory.CustomFlag1);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(false, OrderedInventory.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(true, OrderedInventory.CustomFlag2);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(false, OrderedInventory.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(true, OrderedInventory.CustomFlag3);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(false, OrderedInventory.CustomFlag3);
		}

		public void TestCustomFlag4()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(true, OrderedInventory.CustomFlag4);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(false, OrderedInventory.CustomFlag4);
		}

		public void TestCustomFlag5()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals(true, OrderedInventory.CustomFlag5);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals(false, OrderedInventory.CustomFlag5);
		}

		public void TestCustomTextBlob1()
		{
			SetupTestPickOrderedInventoryData(OrderedInventory);
			AssertEquals("TEXTBLOB1", OrderedInventory.CustomTextBlob1);
			foreach (var line in OrderedInventory.Owners)
			{
				orderedInventory.Owners.RemoveFromRelationship(line);
			}
			AssertEquals("", OrderedInventory.CustomTextBlob1);
		}

		public void TestCustomAttrib1Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomAttrib1, OrderedInventory.CustomAttrib1Info.Name);
		}

		public void TestCustomAttrib2Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomAttrib2, OrderedInventory.CustomAttrib2Info.Name);
		}

		public void TestCustomAttrib3Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomAttrib3, OrderedInventory.CustomAttrib3Info.Name);
		}

		public void TestCustomAttrib4Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomAttrib4, OrderedInventory.CustomAttrib4Info.Name);
		}

		public void TestCustomAttrib5Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomAttrib5, OrderedInventory.CustomAttrib5Info.Name);
		}

		public void TestCustomAttrib6Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomAttrib6, OrderedInventory.CustomAttrib6Info.Name);
		}

		public void TestCustomDecimal1Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDecimal1, OrderedInventory.CustomDecimal1Info.Name);
		}

		public void TestCustomDecimal2Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDecimal2, OrderedInventory.CustomDecimal2Info.Name);
		}

		public void TestCustomDecimal3Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDecimal3, OrderedInventory.CustomDecimal3Info.Name);
		}

		public void TestCustomDecimal4Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDecimal4, OrderedInventory.CustomDecimal4Info.Name);
		}

		public void TestCustomDecimal5Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDecimal5, OrderedInventory.CustomDecimal5Info.Name);
		}

		public void TestCustomDate1Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDate1, OrderedInventory.CustomDate1Info.Name);
		}

		public void TestCustomDat21Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDate2, OrderedInventory.CustomDate2Info.Name);
		}

		public void TestCustomDate3Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDate3, OrderedInventory.CustomDate3Info.Name);
		}

		public void TestCustomDate4Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDate4, OrderedInventory.CustomDate4Info.Name);
		}

		public void TestCustomDate5Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomDate5, OrderedInventory.CustomDate5Info.Name);
		}

		public void TestCustomFlag1Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomFlag1, OrderedInventory.CustomFlag1Info.Name);
		}

		public void TestCustomFlag2Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomFlag2, OrderedInventory.CustomFlag2Info.Name);
		}

		public void TestCustomFlag3Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomFlag3, OrderedInventory.CustomFlag3Info.Name);
		}

		public void TestCustomFlag4Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomFlag4, OrderedInventory.CustomFlag4Info.Name);
		}

		public void TestCustomFlag5Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomFlag5, OrderedInventory.CustomFlag5Info.Name);
		}

		public void TestCustomTextBlob1Info()
		{
			AssertEquals(WhsPickOrderedInventory.Schema.CustomTextBlob1, OrderedInventory.CustomTextBlob1Info.Name);
		}

		#endregion

		#region ICustomLabelsProvider Members

		public void TestCustomLabelsProvider()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_OH_Client = data.Org1.PK;
			order.WD_WW_Whs = data.Whs1.PK;

			var provider = new WhsPickOrderedInventory.CustomLabelsProvider(order);
			AssertEquals(order, provider.ConfigOrgProvider);

			var list = provider.GetCustomFields(data.Org1, Factory);
			AssertEquals("the client", list.ConfigOrgLocatedAt);

			AssertEquals("List count correct", 22, list.Count);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomAttrib1);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomAttrib2);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomAttrib3);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomAttrib4);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomAttrib5);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomAttrib6);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDate1);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDate2);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDate3);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDate4);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDate5);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDecimal1);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDecimal2);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDecimal3);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDecimal4);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomDecimal5);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomFlag1);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomFlag2);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomFlag3);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomFlag4);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomFlag5);
			AssertCustomLabelListContains(list, WhsPickOrderedInventory.Schema.CustomTextBlob1);
		}

		protected void AssertCustomLabelListContains(CustomLabelInfoList list, string propertyName)
		{
			foreach (CustomLabelInfo info in list)
			{
				if (info.PropertyName == propertyName)
				{
					return;
				}
			}
			Fail("Could not find property: " + propertyName + " in CustomLabelInfoList");
		}

		#endregion

		#region IWhsPickOrderedInventoryInternals Members

		public void TestIWhsPickOrderedInventoryInternals_SetAllProperties()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("1");
			OrgHeader org = Helper.CreateClient();
			OrgSupplierPart part = Helper.CreateProduct(org, "P1");
			part.OP_Desc = "PDESC1";
			part.OP_StockKeepingUnit = "CTN";
			WhsOrder order = Helper.CreateWhsOrder(org, whs);
			WhsOrderLine orderLine = Helper.CreateWhsOrderLine(order, part, 10m);

			var expiry = ZDate.Today.AddDays(1);
			var packing = ZDate.Today.AddDays(-1);
			Helper.SetDocketLineAttributes(orderLine, expiry, packing, "PA1", "PA2", "PA3", "SN3", "BEK1-1");

			WhsPick pick = Factory.New<WhsPick>();
			((IWhsPickOrderedInventoryInternals)OrderedInventory).SetAllProperties(pick, orderLine);
			AssertEquals(pick, OrderedInventory.Pick);
			AssertEquals(org, OrderedInventory.Client);
			AssertEquals(org.PK, OrderedInventory.ClientPK);
			AssertEquals("WHTEST", OrderedInventory.ClientCode);
			AssertEquals(part, OrderedInventory.SupplierPart);
			AssertEquals(part.PK, OrderedInventory.SupplierPartPK);
			AssertEquals("P1", OrderedInventory.ProductCode);
			AssertEquals("PDESC1", OrderedInventory.ProductDesc);
			AssertEquals(expiry, OrderedInventory.ExpiryDate);
			AssertEquals(packing, OrderedInventory.PackingDate);
			AssertEquals("BEK1-1", OrderedInventory.BondedEntryKey);
			AssertEquals("PA1", OrderedInventory.PartAttrib1);
			AssertEquals("PA2", OrderedInventory.PartAttrib2);
			AssertEquals("PA3", OrderedInventory.PartAttrib3);
			AssertEquals("SN3", OrderedInventory.SerialNumber);
		}

		#endregion

		#region Implementation

		void SetupTestPickOrderedInventoryData(WhsPickOrderedInventory orderedInventory)
		{
			var order = Factory.New<WhsOrder>();
			var orderLine = order.Lines.AddNew();
			orderedInventory.Owners.Add(orderLine);
			Helper.SetDocketLineAttributes(orderLine, ZDate.Today.AddDays(1), ZDate.Today.AddDays(-1), "PA1", "PA2", "PA3", "SN1", "BEK-1");
			Helper.SetDocketLineCustomAttributes(orderLine, "CA1", "CA2", "CA3", "CA4", "CA5", "CA6", 1.1m, 2.2m, 3.3m, 4.4m, 5.5m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(2), ZDateTime.Today.AddDays(3), ZDateTime.Today.AddDays(4), ZDateTime.Today.AddDays(5), true, true, true, true, true, "TEXTBLOB1");
		}

		protected WhsPickOrderedInventory OrderedInventory
		{
			get { return orderedInventory ?? (orderedInventory = (WhsPickOrderedInventory)GetNewBusinessObject()); }
			set { orderedInventory = value; }
		}

		protected WhsPickOrderedInventory orderedInventory;
		protected WhsPick Pick;

		#endregion

		#region TestAllocate_HighPriorityLocationsFirst

		[GuiTest]
		public void TestAllocate_HighPriorityLocationsFirst()
		{
			var today = ZDateTimeOffset.Today;
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			var highPriorityLocationType = Helper.CreateLocationType("HP1", "HighPriorityLocation", false, 0, LocationClasses.Codes.HPL);
			location2.WLV_WLT_LocationType = highPriorityLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", today, data.Part1, 10m, "", location1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", today, data.Part1, 10m, "", location2, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", today, data.Part1, 10m, "", location3, "");

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1m);
			Factory.Save();

			var pick = Helper.CreatePickNew_WithoutAllocationEngineMock(order);
			var orderedInventory = (WhsPickOrderedInventory)pick.OrderedInventories.Single();
			AssertEquals(3, orderedInventory.AvailableInventories.Count);

			var allocatedInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.Allocate);
			AssertEquals("Expected to pick from High Priority Location.", "A-2", allocatedInventory.Location.WLV_LocationString);
		}

		#endregion
	}
}
