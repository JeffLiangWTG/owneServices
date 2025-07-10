using System;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CodeLists;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsPickAvailableInventoryValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckPickLineQuantity

		public void TestCheckPickLineQuantity()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 80m);

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("We should be able to pick less that ordered.", 50m, availableInventory.PickLineQuantity);
			AssertEquals("We should be able to pick less that ordered.", 50m, availableInventory.Inventory[0].WI_AvailableToPickQuantity);
			AssertEquals("We should be able to pick less that ordered.", 50m, pick.GetAllPickLines().First().WZ_Units);
			AssertNoWarnings("No warnings expected when picked less stock that ordered.", availableInventory.PickLineQuantityInfo);

			availableInventory.PickLineQuantity = 90m;
			AssertEquals("PickLineQuantity could not be greater that Quantity Ordered.", 80m, availableInventory.PickLineQuantity);
			AssertHasWarning("When user try to overpick he should receive a warning.",
				availableInventory.PickLineQuantityInfo,
				"Quantity Allocated cannot be greater than Units Ordered. First deallocate stock from other inventories before allocating this stock."); // modify test to verify modified warning msg

			orderLine1.WE_TransactionQuantity = 120m;
			availableInventory.PickLineQuantity = 70m;
			AssertEquals("We should be able to pick less that ordered.", 70m, availableInventory.PickLineQuantity);
			AssertNoWarnings("No warnings expected when picked less stock that ordered.", availableInventory.PickLineQuantityInfo);

			availableInventory.PickLineQuantity = 120m;
			AssertEquals("PickLineQuantity could not be greater that what is available in stock.", 100m, availableInventory.PickLineQuantity);
			AssertHasWarning("When user try to Pick more stock that available to be picked he should receive a warning.",
				availableInventory.PickLineQuantityInfo,
				"Only 100 Units are available for allocation.");

			availableInventory.PickLineQuantity = -10m;
			AssertEquals("PickLineQuantity could not be greater that what is available in stock.", 0m, availableInventory.PickLineQuantity);
			AssertHasWarning("When user try to Pick more stock that available to be picked he should receive an error.",
				availableInventory.PickLineQuantityInfo,
				"Please enter a value greater than or equal to zero.");

			foreach (CodeDescriptionPair pair in new InventoryStatus())
			{
				if (pair.Code != InventoryStatus.Codes.Available)
				{
					data.Line111.WI_InventoryStatus = pair.Code;

					availableInventory.PickLineQuantity = 70m;
					AssertHasError(availableInventory.PickLineQuantityInfo, "Only Available stock can be allocated.");

					availableInventory.PickLineQuantity = 0m;
					AssertNoErrors(availableInventory.PickLineQuantityInfo);
				}
			}
		}

		#endregion

		#region TestCheckPickLineQuantity_HeldInventory

		public void TestCheckPickLineQuantity_HeldInventory()
		{
			using (WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)) // Only possible with EnableHeldGoodsForOrders
			{
				var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
				var location = data.Whs1.FindLocation("A-1");
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
				var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 100m, location, "", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
				receive.FinaliseDocketWithoutUserConfirmation();

				Factory.Save();

				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 80m);
				orderLine.WE_WHC_NKOrderedHeldCode = InventoryHoldCodes.Codes.Held;

				var pick = Helper.CreatePickNew(order);

				var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

				availableInventory.PickLineQuantity = 50m;
				AssertEquals("We should be able to pick less that ordered.", 50m, availableInventory.PickLineQuantity);
				AssertEquals("We should be able to pick less that ordered.", 50m, availableInventory.Inventory[0].WI_AvailableToTransferQuantity);
				AssertEquals("We should be able to pick less that ordered.", 50m, pick.GetAllPickLines().First().WZ_Units);
				AssertNoWarnings("No warnings expected when picked less stock that ordered.", availableInventory.PickLineQuantityInfo);

				availableInventory.PickLineQuantity = 90m;
				AssertEquals("PickLineQuantity could not be greater that Quantity Ordered.", 80m, availableInventory.PickLineQuantity);
				AssertHasWarning("When user try to overpick he should receive a warning.", availableInventory.PickLineQuantityInfo, "Quantity Allocated cannot be greater than Units Ordered. First deallocate stock from other inventories before allocating this stock.");

				orderLine.WE_TransactionQuantity = 120m;
				availableInventory.PickLineQuantity = 70m;
				AssertEquals("We should be able to pick less that ordered.", 70m, availableInventory.PickLineQuantity);
				AssertNoWarnings("No warnings expected when picked less stock that ordered.", availableInventory.PickLineQuantityInfo);

				availableInventory.PickLineQuantity = 120m;
				AssertEquals("PickLineQuantity could not be greater that what is available in stock.", 100m, availableInventory.PickLineQuantity);
				AssertHasWarning("When user try to Pick more stock that available to be picked he should receive a warning.", availableInventory.PickLineQuantityInfo, "Only 100 Units are available for allocation.");

				availableInventory.PickLineQuantity = -10m;
				AssertEquals("PickLineQuantity could not be greater that what is available in stock.", 0m, availableInventory.PickLineQuantity);
				AssertHasWarning("When user try to Pick more stock that available to be picked he should receive an error.", availableInventory.PickLineQuantityInfo, "Please enter a value greater than or equal to zero.");

				availableInventory.Inventory[0].WI_InventoryStatus = InventoryStatus.Codes.Available;
				availableInventory.PickLineQuantity = 70m;
				AssertHasError(availableInventory.PickLineQuantityInfo, "Only Held stock can be allocated.");
			}
		}

		#endregion

		#region TestPickLineQuantity_NoErrorOnPickByBOMKit

		public void TestPickLineQuantity_NoErrorOnPickByBOMKit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 100m, data.Whs1.FindLocation("A-1"));
			var wheelInventory = receive.Inventory[0];
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);

			var pick = Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 30m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			pick = newFactory.Load<WhsPick>(pick.PK);
			var bikeAvailableInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.SupplierPartPK == bike.PK).AvailableInventories[0];

			bikeAvailableInventory.PickLineQuantity = 20m;
			AssertEquals(true, bikeAvailableInventory.PickLineQuantityInfo.HasWarnings());

			bikeAvailableInventory.PickLineQuantity = 30m;
			AssertNoWarnings(bikeAvailableInventory.PickLineQuantityInfo);
			AssertNoErrors(bikeAvailableInventory.PickLineQuantityInfo);
		}

		#endregion

		#region TestCheckPickLineQuantity_DoesNotReduceBelowPackedQty

		public void TestCheckPickLineQuantity_DoesNotReduceBelowPackedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderLine.ReleaseLines[0];
			var package = order.PackageJob.Packages.AddNew();
			package.Pack(releaseLine, 6m);

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: 10 Units Picked.", 10m, availableInventory.PickLineQuantity);
			AssertNoWarnings(availableInventory.PickLineQuantityInfo);

			availableInventory.PickLineQuantity = 6m;
			AssertNoWarnings(availableInventory.PickLineQuantityInfo);

			availableInventory.PickLineQuantity = 4m;
			AssertEquals("PickLineQuantity should be reverted when PickLines are packed.", 6m, availableInventory.PickLineQuantity);
			AssertHasWarning(availableInventory.PickLineQuantityInfo, "Quantity Allocated cannot be reduced below what is Packed across this Pick."); // modify test to verify modified warning msg
		}

		#endregion

		#region TestCheckPickLineQuantity_WithReleaseCapturedAttribute

		public void TestCheckPickLineQuantity_WithReleaseCapturedAttributeAndFullyDeallocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: 10 Units Picked.", 10m, availableInventory.PickLineQuantity);
			AssertNoErrors(availableInventory.PickLineQuantityInfo);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			availableInventory.PickLineQuantity = 0m;
			AssertNoErrors(availableInventory.PickLineQuantityInfo);
			AssertNoWarnings(availableInventory.PickLineQuantityInfo);
			AssertEquals("PickLineQuantity of availableInventory has been changed to 0.", 0m, availableInventory.PickLineQuantity);
		}

		public void TestCheckPickLineQuantity_WithReleaseCapturedAttributeAndPartialDellocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: 10 Units Picked.", 10m, availableInventory.PickLineQuantity);
			AssertNoErrors(availableInventory.PickLineQuantityInfo);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			availableInventory.PickLineQuantity = 6m;
			AssertNoErrors(availableInventory.PickLineQuantityInfo);
			AssertHasWarning(availableInventory.PickLineQuantityInfo, "You cannot Allocate less Stock than has been Release Captured.");
			AssertEquals("Release Captured units should recover to 10m.", 10m, availableInventory.PickLineQuantity);
		}

		public void TestCheckPickLineQuantity_WithReleaseCapturedAttributeAndGreaterThanQuantityReleased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var pickLines = orderLine.PickLines;
			var pick = Helper.CreatePickNew(order);
			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			availableInventory.Allocate = true;
			availableInventory.PickLineQuantity = 10;

			AssertEquals("Precondition: 10 Units Picked.", 10m, availableInventory.PickLineQuantity);
			AssertNoErrors(availableInventory.PickLineQuantityInfo);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			availableInventory.PickLineQuantity = 15m;
			AssertNoErrors(availableInventory.PickLineQuantityInfo);
			AssertNoWarnings(availableInventory.PickLineQuantityInfo);
			AssertEquals("count of PickLines should be 2.", 2, pickLines.Count);
			AssertEquals("WZ_Units of PickLine 1 should be 10m.", 10m, pickLines[0].WZ_Units);
			AssertEquals("PickLine 1 should have ReleaseCapturedAttribs.", true, pickLines[0].HasReleaseCapturedAttribs);
			AssertEquals("WZ_Units of PickLine 2 should be 5m.", 5m, pickLines[1].WZ_Units);
			AssertEquals("PickLine 2 should not have ReleaseCapturedAttribs.", false, pickLines[1].HasReleaseCapturedAttribs);
			availableInventory.PickLineQuantity = 25m;
			AssertNoErrors(availableInventory.PickLineQuantityInfo);
			AssertHasWarning(availableInventory.PickLineQuantityInfo, "Quantity Allocated cannot be greater than Units Ordered. First deallocate stock from other inventories before allocating this stock.");
			AssertEquals("Release Captured units should recover to 20m.", 20m, availableInventory.PickLineQuantity);
		}

		#endregion

		#region TestCheckPickLineQuantity_MatchesQuantityReleased

		public void TestCheckPickLineQuantity_MatchesQuantityReleased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			pick.IsAlterPick = true;

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: 10 Units Picked.", 10m, availableInventory.PickLineQuantity);
			AssertNoErrors(availableInventory.PickLineQuantityInfo);

			var releaseLine = orderLine.ReleaseLines[0];
			releaseLine.PartAttribute1 = "RED";
			availableInventory.PickLineQuantity = 9m;
			AssertNoErrors(availableInventory.PickLineQuantityInfo);
			AssertHasWarning(availableInventory.PickLineQuantityInfo, "You cannot Allocate less Stock than has been Release Captured.");
			AssertEquals("Release Captured units is 10, cannot deallocate", 10m, availableInventory.PickLineQuantity);

			releaseLine.Quantity = 9m;
			availableInventory.Validation.Validate_PickLineQuantity();
			AssertNoErrors(availableInventory.PickLineQuantityInfo);
			AssertNoWarnings(availableInventory.PickLineQuantityInfo);
		}

		#endregion

		#region TestCheckPickLineQuantity_MatchesQuantityReleased_WithWorkOrder

		public void TestCheckPickLineQuantity_MatchesQuantityReleased_WithWorkOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsWorkOrderLine(order, data.Part1, 2m);
			var childLine = orderLine.ChildComponentLines.ElementAt(0);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: 10 Units Picked.", 2m, availableInventory.PickLineQuantity);
			AssertNoErrors(availableInventory.PickLineQuantityInfo);

			var pickLine = childLine.PickLines.Single();
			pickLine.WZ_ReleaseCapturedPartAttrib1 = "RED";
			availableInventory.PickLineQuantity = 1m;
			AssertNoErrors(availableInventory.PickLineQuantityInfo);
		}

		#endregion

		#region TestCheckPickLineQuantity_MatchesQuantityReleased_ComponentLineOnSalesOrder

		public void TestCheckPickLineQuantity_MatchesQuantityReleased_ComponentLineOnSalesOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true, setReleaseCaptured: true);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			var childLine = orderLine.ChildComponentLines.ElementAt(0);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(o => o.IsComponentOrderedInventoryOnSalesOrder);
			var availableInventory = orderedInventory.AvailableInventories[0];
			AssertEquals("Precondition: 10 Units Picked.", 2m, availableInventory.PickLineQuantity);
			AssertNoErrors(availableInventory.PickLineQuantityInfo);

			var pickLine = childLine.PickLines.Single();
			pickLine.WZ_ReleaseCapturedPartAttrib1 = "RED";
			availableInventory.PickLineQuantity = 1m;
			AssertNoErrors(availableInventory.PickLineQuantityInfo);
		}

		#endregion

		#region TestCheckPickLineQuantity_ThisShouldNeverHappen

		public void TestCheckPickLineQuantity_ThisShouldNeverHappen()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 120m);

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			//availableInventory.ClearPickLineQuantityCache();
			availableInventory.PickLines.ElementAt(0).WZ_Units = -10m; // hack PickLineQuantity.
			availableInventory.Validation.Validate_PickLineQuantity();
			AssertEquals("Precondition", -10m, availableInventory.PickLineQuantity);
			AssertHasError(availableInventory.PickLineQuantityInfo, "Please enter a value greater than or equal to zero.");

			//availableInventory.ClearPickLineQuantityCache();
			availableInventory.PickLines.ElementAt(0).WZ_Units = 110m; // hack PickLineQuantity.
			availableInventory.Validation.Validate_PickLineQuantity();
			AssertEquals("Precondition.", 110m, availableInventory.PickLineQuantity);
			AssertHasError(availableInventory.PickLineQuantityInfo, "Only 100 Units are available for allocation.");
		}

		#endregion

		#region TestCheckPickLineQuantity_OverCommittedStockShouldNeverAffectAnotherPick

		public void TestCheckPickLineQuantity_OverCommittedStockShouldNeverAffectAnotherPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m, data.Whs1.FindLocation("A-1"), "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 5m, data.Whs1.FindLocation("A-2"), "");

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order1, data.Part1, 20m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order2, data.Part1, 2m);
			Factory.Save();

			// pick1 has over-committed stock
			var pick1 = Helper.CreatePickByAttachingOrders(order1);
			var availableInventoryOverCommitted = pick1.OrderedInventories[0].AvailableInventories[0];
			availableInventoryOverCommitted.Allocate = true;
			//availableInventoryOverCommitted.ClearPickLineQuantityCache();
			availableInventoryOverCommitted.PickLines.ElementAt(0).WZ_Units = 20m; // hack PickLineQuantity.

			// pick 2 is ok
			var pick2 = Helper.CreatePickByAttachingOrders(order2);

			pick1.Validation.ValidateAll();
			pick2.Validation.ValidateAll();
			AssertEquals("Pick 1 stock is over-committed, should have a validation error.", true, pick1.HasErrors);
			AssertEquals("Pick 2 stock is ok, should *not* have a validation error.", false, pick2.HasErrors);
		}

		#endregion

		#region TestCheckPickLineQuantity_WhenPickIsFinalised

		public void TestCheckPickLineQuantity_WhenPickIsFinalised()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "1", data.Part1, 120m);
			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			AssertIsFinalisedPrecondition(order);
			AssertIsFinalisedPrecondition(pick);

			//availableInventory.ClearPickLineQuantityCache();
			((IBusinessObjectInternals)availableInventory.PickLines.ElementAt(0)).Row[WhsPickLineSchema.Constants.WZ_Units] = -10m; // hack PickLineQuantity.
			availableInventory.Validation.Validate_PickLineQuantity();
			AssertEquals("Precondition", -10m, availableInventory.PickLineQuantity);
			AssertNoErrors("When pick is finalised, no validation should be run.", availableInventory.PickLineQuantityInfo);
		}

		#endregion

		#region TestCheckPickLineQuantity_MinimalShelfLifeWarning

		[TestDate(2013, 3, 6)]
		public void TestCheckPickLineQuantity_MinimalShelfLifeWarning_JulianBatchNumberPartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // Will turn on usage of Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true); // Will turn on usage of Expiry Date
			data.Part2.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1).W3_MaximumShelfLife = 30;

			var consignee = Helper.CreateClient("CONSIGNEE");
			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Empty, ZDate.Empty, "", "3030ABC", "", ""); // Expiry date < Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, ZDate.Empty, ZDate.Empty, "", "3046ABC", "", ""); // Expiry date = Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Empty, ZDate.Empty, "", "3060ABC", "", ""); // Expiry date > Min Shelf Life
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1", ZDateTimeOffset.Today, Notify, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);

			var expectedWarningMessage = "This stock does not satisfy the Minimum Shelf Life of the Consignee.";

			// Normal Attribute
			var orderedInventory_NormalAttribute = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
			var availableInventory_NormalAttribute = orderedInventory_NormalAttribute.AvailableInventories[0];
			AssertNoWarning(availableInventory_NormalAttribute.PickLineQuantityInfo, expectedWarningMessage);

			availableInventory_NormalAttribute.PickLineQuantity = 3m;
			AssertNoWarning(availableInventory_NormalAttribute.PickLineQuantityInfo, expectedWarningMessage);

			// Julian Batch Number
			var orderedInventory_JulianBatchNumber = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2);
			var availableInventory_EarlierExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 5m);
			var availableInventory_MatchingExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 7m);
			var availableInventory_LaterExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 10m);
			AssertNoWarning(availableInventory_EarlierExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_MatchingExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_LaterExpiryDate.PickLineQuantityInfo, expectedWarningMessage);

			availableInventory_EarlierExpiryDate.PickLineQuantity = 3m;
			availableInventory_MatchingExpiryDate.PickLineQuantity = 3m;
			availableInventory_LaterExpiryDate.PickLineQuantity = 3m;
			AssertHasWarning(availableInventory_EarlierExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_MatchingExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_LaterExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
		}

		[TestDate(2019, 4, 15)]
		public void TestCheckPickLineQuantity_MinimalShelfLifeWarning_ExpiryDatePartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, true);

			var consignee = Helper.CreateClient("CONSIGNEE");
			consignee.MiscServ.OM_MinimumShelfLifeAccepted = 10;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Today.AddDays(5), ZDate.Empty, "", "", "", ""); // Expiry date < Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, ZDate.Today.AddDays(10), ZDate.Empty, "", "", "", ""); // Expiry date = Min Shelf Life
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Today.AddDays(15), ZDate.Empty, "", "", "", ""); // Expiry date > Min Shelf Life
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1", ZDateTimeOffset.Today, Notify, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);
			var expectedWarningMessage = "This stock does not satisfy the Minimum Shelf Life of the Consignee.";

			// No Expiry Date
			var orderedInventory_ProdWithNoExpDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
			var availableInventory_ProdWithNoExpDate = orderedInventory_ProdWithNoExpDate.AvailableInventories[0];
			AssertNoWarning(availableInventory_ProdWithNoExpDate.PickLineQuantityInfo, expectedWarningMessage);

			availableInventory_ProdWithNoExpDate.PickLineQuantity = 3m;
			AssertNoWarning(availableInventory_ProdWithNoExpDate.PickLineQuantityInfo, expectedWarningMessage);

			// With Expiry Date
			var orderedInventory_ProdWithExpDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2);
			var availableInventory_EarlierExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 5m);
			var availableInventory_MatchingExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 7m);
			var availableInventory_LaterExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 10m);
			AssertNoWarning(availableInventory_EarlierExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_MatchingExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_LaterExpiryDate.PickLineQuantityInfo, expectedWarningMessage);

			availableInventory_EarlierExpiryDate.PickLineQuantity = 3m;
			availableInventory_MatchingExpiryDate.PickLineQuantity = 3m;
			availableInventory_LaterExpiryDate.PickLineQuantity = 3m;
			AssertHasWarning(availableInventory_EarlierExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_MatchingExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_LaterExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
		}

		#endregion

		#region TestCheckPickLineQuantity_ExpiredStockWarning

		[TestDate(2019, 8, 12)]
		public void TestCheckPickLineQuantity_ExpiredStockWarning_JulianBatchNumberPartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.JulianBatchNumber); // Will turn on usage of Expiry Date
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, true); // Will turn on usage of Expiry Date
			data.Part2.RelatedOrganisations[0].OU_JulianBatchNoFormat = JulianBatchNumberFormatList.Codes.YDDD_BatchNumber;

			Helper.CreateProductParamsByWhsAndClient(data.Part2, data.Org1, data.Whs1).W3_MaximumShelfLife = 5;

			var consignee = Helper.CreateClient("CONSIGNEE");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "PA1", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Empty, ZDate.Empty, "", "9150ABC", "", ""); // Expiry date < Current date
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, ZDate.Empty, ZDate.Empty, "", "9219ABC", "", ""); // Expiry date = Current date
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Empty, ZDate.Empty, "", "9250ABC", "", ""); // Expiry date > Current date
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1", ZDateTimeOffset.Today, Notify, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);

			var expectedWarningMessage = "This stock is expired.";

			// Normal Attribute
			var orderedInventory_NormalAttribute = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
			var availableInventory_NormalAttribute = orderedInventory_NormalAttribute.AvailableInventories[0];
			AssertNoWarning(availableInventory_NormalAttribute.PickLineQuantityInfo, expectedWarningMessage);

			availableInventory_NormalAttribute.PickLineQuantity = 3m;
			AssertNoWarning(availableInventory_NormalAttribute.PickLineQuantityInfo, expectedWarningMessage);

			// Julian Batch Number
			var orderedInventory_JulianBatchNumber = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2);
			var availableInventory_EarlierExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 5m);
			var availableInventory_MatchingExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 7m);
			var availableInventory_LaterExpiryDate = orderedInventory_JulianBatchNumber.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 10m);
			AssertNoWarning(availableInventory_EarlierExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_MatchingExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_LaterExpiryDate.PickLineQuantityInfo, expectedWarningMessage);

			availableInventory_EarlierExpiryDate.PickLineQuantity = 3m;
			availableInventory_MatchingExpiryDate.PickLineQuantity = 3m;
			availableInventory_LaterExpiryDate.PickLineQuantity = 3m;
			AssertHasWarning(availableInventory_EarlierExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertHasWarning(availableInventory_MatchingExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_LaterExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
		}

		[TestDate(2019, 8, 12)]
		public void TestCheckPickLineQuantity_ExpiredStockWarning_ExpiryDatePartAttributeUsed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, true);

			var consignee = Helper.CreateClient("CONSIGNEE");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, ZDate.Empty, ZDate.Empty, "", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5m, ZDate.Today.AddDays(-5), ZDate.Empty, "", "", "", ""); // Expiry date < Current date
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 7m, ZDate.Today, ZDate.Empty, "", "", "", ""); // Expiry date = Current date
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, ZDate.Today.AddDays(5), ZDate.Empty, "", "", "", ""); // Expiry date > Current date
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1", ZDateTimeOffset.Today, Notify, WhsPickOption.Codes.Manual);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreateWhsOrderLine(order, data.Part2, 10m);

			var pick = Helper.CreatePickNew(order);
			var expectedWarningMessage = "This stock is expired.";

			// No Expiry Date
			var orderedInventory_ProdWithNoExpDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part1);
			var availableInventory_ProdWithNoExpDate = orderedInventory_ProdWithNoExpDate.AvailableInventories[0];
			AssertNoWarning(availableInventory_ProdWithNoExpDate.PickLineQuantityInfo, expectedWarningMessage);

			availableInventory_ProdWithNoExpDate.PickLineQuantity = 3m;
			AssertNoWarning(availableInventory_ProdWithNoExpDate.PickLineQuantityInfo, expectedWarningMessage);

			// With Expiry Date
			var orderedInventory_ProdWithExpDate = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(l => l.SupplierPart == data.Part2);
			var availableInventory_EarlierExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 5m);
			var availableInventory_MatchingExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 7m);
			var availableInventory_LaterExpiryDate = orderedInventory_ProdWithExpDate.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(l => l.QuantityAvailableToPick == 10m);
			AssertNoWarning(availableInventory_EarlierExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_MatchingExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_LaterExpiryDate.PickLineQuantityInfo, expectedWarningMessage);

			availableInventory_EarlierExpiryDate.PickLineQuantity = 3m;
			availableInventory_MatchingExpiryDate.PickLineQuantity = 3m;
			availableInventory_LaterExpiryDate.PickLineQuantity = 3m;
			AssertHasWarning(availableInventory_EarlierExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertHasWarning(availableInventory_MatchingExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
			AssertNoWarning(availableInventory_LaterExpiryDate.PickLineQuantityInfo, expectedWarningMessage);
		}

		#endregion

		#region TestCheckPickLineQuantity_WhenThereAre Loading/Loaded/Departed Orders

		public void TestCheckPickLineQuantity_WhenThereAreLoadingOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 40m;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: one Pick Line with 40 units.", 40m, pickLine.WZ_Units);

			var package1 = order.PackageJob.Packages.AddNew();
			var package2 = order.PackageJob.Packages.AddNew();

			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			Factory.Save();

			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loading, order.WarehouseOrderStatus);

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("No new Pick Line is Created.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Cannot allocate more.", 40m, availableInventory.PickLineQuantity);
			AssertEquals("Cannot allocate more.", true, availableInventory.PickLineQuantityInfo.HasWarning("You cannot Allocate more Stock for a Loading, Loaded or Departed Order."));
		}

		public void TestCheckPickLineQuantity_WhenThereAreLoadedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 40m;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: one Pick Line with 40 units.", 40m, pickLine.WZ_Units);

			var package1 = order.PackageJob.Packages.AddNew();
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Loaded, order.WarehouseOrderStatus);

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("No new Pick Line is Created.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Cannot allocate more.", 40m, availableInventory.PickLineQuantity);
			AssertEquals("Cannot allocate more.", true, availableInventory.PickLineQuantityInfo.HasWarning("You cannot Allocate more Stock for a Loading, Loaded or Departed Order."));
		}

		public void TestCheckPickLineQuantity_WhenThereAreDepartedOrders()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 50m);
			var pick = Helper.CreatePickNew(order);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 40m;

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: one Pick Line with 40 units.", 40m, pickLine.WZ_Units);

			var package1 = order.PackageJob.Packages.AddNew();
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Factory.Save();

			AssertEquals("Order Status correct", WhsOrderStatus.Codes.Departed, order.WarehouseOrderStatus);

			availableInventory.PickLineQuantity = 50m;
			AssertEquals("No new Pick Line is Created.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Cannot allocate more.", 40m, availableInventory.PickLineQuantity);
			AssertEquals("Cannot allocate more.", true, availableInventory.PickLineQuantityInfo.HasWarning("You cannot Allocate more Stock for a Loading, Loaded or Departed Order."));
		}

		public void TestCheckPickLineQuantity_WhenThereAreDepartedOrders_HasAnotherOrderCanBeAllocated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part1, 100m);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, jobID: "L3", transportUnit: truck, startTime: DateTimeOffset.Now);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order1, order2);

			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];
			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 5m;

			AssertEquals("Precondition: only 1 PickLine.", 1, pick.GetAllPickLines().Count());
			var pickLine = order1.Lines[0].PickLines[0];
			AssertEquals("Precondition: this PickLine is on order1.", 5m, pickLine.WZ_Units);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var package1 = order1.PackageJob.Packages.AddNew();
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			Helper.DepartPackageNow(loadPkgPackagePivot1);
			Factory.Save();

			AssertEquals("Order1 Status correct", WhsOrderStatus.Codes.Departed, order1.WarehouseOrderStatus);
			AssertEquals("Order2 Status correct", DocketStatus.Codes.AttachedToPick, order2.WarehouseOrderStatus);

			pick.IsAlterPick = true;
			availableInventory.PickLineQuantity = 20m;
			AssertEquals("Only 1 new Pick Line is Created.", 2, pick.GetAllPickLines().Count());
			AssertEquals("New Pick Line is on order2.", 10m, pick.GetAllPickLines().Single(l => l.WZ_WE_TransactionLine == order2.Lines[0].PK).WZ_Units);
			AssertEquals("Pick Line on order1 does not change.", 5m, pick.GetAllPickLines().Single(l => l.WZ_WE_TransactionLine == order1.Lines[0].PK).WZ_Units);
			AssertEquals("5 units cannot be allocated on order1 because it's departed.", 15m, availableInventory.PickLineQuantity);
			AssertEquals("Cannot allocate more.", true, availableInventory.PickLineQuantityInfo.HasWarning("You cannot Allocate more Stock for a Loading, Loaded or Departed Order."));
		}

		#endregion

		#region TestPickLineQuantity_CannotBeChangedForPickByBOM

		public void TestPickLineQuantity_CannotBeChangedForPickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, Constants.PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, Constants.PkgUnit.Unit);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive, frame, 25m, data.Whs1.FindLocation("A-1"));
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "KIT", bike, 10m);
			var kitOrderLine = order.Lines[0];
			var wheelLine = Helper.CreateWhsOrderLine(order, wheel, 1m);
			var frameLine = Helper.CreateWhsOrderLine(order, frame, 1m);

			var pick = Helper.CreatePickNew(order);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine.IsBOMProductPickedOnSalesOrder);
			AssertEquals("Precondition - Picked By BOM", true, kitOrderLine.ChildComponentLines.Count > 0);

			var wheelComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == wheel.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];
			var frameComponentInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == frame.PK && ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];
			var wheelInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == wheel.PK && !ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];
			var frameInv = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single(ordInv => ordInv.SupplierPartPK == frame.PK && !ordInv.IsComponentOrderedInventoryOnSalesOrder).AvailableInventories[0];

			AssertEquals("Precondition", 20m, wheelComponentInv.PickLineQuantity);
			AssertEquals("Precondition", 10m, frameComponentInv.PickLineQuantity);
			AssertEquals("Precondition", 1m, wheelInv.PickLineQuantity);
			AssertEquals("Precondition", 1m, frameInv.PickLineQuantity);

			wheelComponentInv.PickLineQuantity = 10m;
			frameComponentInv.PickLineQuantity = 5m;
			AssertEquals("Should not have changed quantity picked", 20m, wheelComponentInv.PickLineQuantity);
			AssertEquals("Should not have changed quantity picked", 10m, frameComponentInv.PickLineQuantity);
			AssertHasWarning(wheelComponentInv.PickLineQuantityInfo, "Quantity Allocated cannot be modified for BOM Products Allocated without Work Orders."); // modify test to verify modified warning msg
			AssertHasWarning(frameComponentInv.PickLineQuantityInfo, "Quantity Allocated cannot be modified for BOM Products Allocated without Work Orders.");

			wheelInv.PickLineQuantity = 0m;
			frameInv.PickLineQuantity = 0m;
			AssertEquals("Should be able to change PickLineQuantity for ordered component lines", 0m, wheelInv.PickLineQuantity);
			AssertEquals("Should be able to change PickLineQuantity for ordered component lines", 0m, frameInv.PickLineQuantity);
			AssertNoWarnings(wheelInv.PickLineQuantityInfo);
			AssertNoWarnings(frameInv.PickLineQuantityInfo);
		}

		#endregion

		#region TestValidatePickLineQuantityHasDifferentName

		public void TestValidatePickLineQuantityHasDifferentName()
		{
			AssertNull("So that ZGrid does not remove the validation notifications from simply tabbing off the cell we make the Validation Method name different so architecture cannot find the method to Validate with.",
				typeof(WhsPickAvailableInventoryValidation).GetMethod("Validate" + WhsPickAvailableInventory.Schema.PickLineQuantity, BindingFlags.Instance | BindingFlags.Public));
		}

		#endregion

		#region TestPickLineQuantity_Cartonising

		public void TestPickLineQuantity_Cartonising()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availInv = pick.OrderedInventories[0].AvailableInventories[0];
			AssertEquals("Precondition", 10m, availInv.PickLineQuantity);
			AssertNoErrors("Precondition", availInv.PickLineQuantityInfo);

			availInv.PickLineQuantity = 1m;
			AssertEquals("Precondition", 1m, availInv.PickLineQuantity);
			AssertNoErrors("Precondition", availInv.PickLineQuantityInfo);

			var cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, pick.PK.ToString());
			cartonisationMutex.Lock();

			using (cartonisationMutex)
			{
				AssertEquals("Precondition", true, pick.IsCartonising);
				availInv.PickLineQuantity = 10m;
				AssertEquals("Should not have changed.", 1m, availInv.PickLineQuantity);
				AssertHasWarning(availInv.PickLineQuantityInfo, "Quantity Allocated cannot be modified as the Pick is having Package Labels allocated."); // modify test to verify modified warning msg

				availInv.AvailableInventoriesSplitByPickedDetails.Cast<WhsPickAvailableInventorySplitByPickedDetails>().Single().PickedDate = ZDateTimeOffset.Now;
				availInv.PickLineQuantity = 10m;
				AssertEquals("Should not have changed.", 1m, availInv.PickLineQuantity);
				AssertHasWarning(availInv.PickLineQuantityInfo, "Quantity Allocated cannot be modified as the Pick is having Package Labels allocated.");
			}

			pick.WP_IsCartonised = true;
			availInv.PickLineQuantity = 10m;
			AssertEquals("Should have changed.", 10m, availInv.PickLineQuantity);
			AssertNoWarnings(availInv.PickLineQuantityInfo);
		}

		#endregion

		#region TestValidatePickLineQuantity_WhenPickingCommenced

		public void TestValidatePickLineQuantity_WhenPickingCommenced()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(); // create 100 units in stock.
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 80m);

			var pick = Helper.CreatePickNew(order);
			var availableInventory = pick.OrderedInventories[0].AvailableInventories[0];

			availableInventory.PickLineQuantity = 50m;
			AssertNoWarnings("No warnings expected when picked less stock that ordered.", availableInventory.PickLineQuantityInfo);

			var pickLine = pick.GetAllPickLines().Single();
			pickLine.WZ_IsPicking = true;
			pickLine.WZ_GS_NKAssignedTo = "A";
			Factory.Save();

			availableInventory.PickLineQuantity = 45m;
			AssertEquals("PickLineQuantity can not be changed when picking commenced.", 50m, availableInventory.PickLineQuantity);
			AssertHasWarning("When user try to reduce PickLineQuantity he should receive a warning.",
				availableInventory.PickLineQuantityInfo,
				"No changes can be made to this Pick's allocations because Picking has already commenced.");
		}

		#endregion

		#region TestValidateAll

		public void TestValidateAll()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories[0];
			var availableInventory = orderedInventory.AvailableInventories[0];

			bool quantityValidationCalled = false;
			availableInventory.PickLineQuantityInfo.AdditionalValidation += () => quantityValidationCalled = true;
			availableInventory.Validation.ValidateAll();
			AssertEquals("Quantity Picked Validation should have been called in ValidateAll().", true, quantityValidationCalled);
		}

		#endregion
	}
}
