using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ChangeAllocationToAnotherPalletIDTest : WhsSecureServiceTestCase
	{
		#region TestChangeAllocationToAnotherPalletID

		public void TestChangeAllocationToAnotherPalletID()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().First();

			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertNotEquals("Should be another pickline", pickLine.PK, response.NewPickLine.PKs[0]);

			var newPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			CombineAssertions(() =>
			{
				AssertNotNull(newPickLine);
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order.Lines[0].PK, newPickLine.DocketLine.PK);
				AssertEquals("OrderLine must still have only 1 pickLine", 1, order.Lines[0].PickLines.Count);
				AssertEquals("New pickLine must be in PickLines collection of original orderLine", newPickLine.PK, order.Lines[0].PickLines[0].PK);
				AssertEquals("No new order lines should be created", 1, order.Lines.Count);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 10m, newPickLine.WZ_Units);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_NoMatchingPickLines

		public void TestChangeAllocationToAnotherPalletID_NoMatchingPickLines()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { Guid.NewGuid() }, "Z2");

			AssertBusinessValidationError(webService, "No pick lines found to be allocated. Please reload this pick and try again.", response);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ManyInventoryRows

		public void TestChangeAllocationToAnotherPalletID_ManyInventoryRows()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			for (int i = 1; i <= 250; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z" + i);
			}

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().First();

			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z10");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertNotEquals("Should be another pickline", pickLine.PK, response.NewPickLine.PKs[0]);

			var webServiceFactory = webService.Factory;
			var newPickLine = webServiceFactory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			CombineAssertions(() =>
			{
				AssertNotNull(newPickLine);
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z10", newPickLine.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order.Lines[0].PK, newPickLine.DocketLine.PK);
				AssertEquals("OrderLine must still have only 1 pickLine", 1, order.Lines[0].PickLines.Count);
				AssertEquals("New pickLine must be in PickLines collection of original orderLine", newPickLine.PK, order.Lines[0].PickLines[0].PK);
				AssertEquals("No new order lines should be created", 1, order.Lines.Count);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 10m, newPickLine.WZ_Units);
			});

			var inventoryRowsInMemory = webServiceFactory.Load<WhsInventoryView>(new ZQuery { FetchOnlyFromLocalCache = true });
			var docketLineRowsInMemory = webServiceFactory.Load<WhsDocketLine>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals("Should load only the original and intended inventory lines, not all available inventory.", 2, inventoryRowsInMemory.Length);
			AssertEquals("Should load only the order line + the original and intended inventory lines, not all available inventory docketlines.", 3, docketLineRowsInMemory.Length);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_OnePalletIsPacked

		public void TestChangeAllocationToAnotherPalletID_OtherPalletIsPacked()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine2 = pick2.GetAllPickLines().Single();
			AssertEquals("Precondition: Order 1 stock taken from Z1 by default", "Z1", pickLine1.Inventory.WI_PalletID);
			AssertEquals("Precondition: Order 2 stock taken from Z2 by default", "Z2", pickLine2.Inventory.WI_PalletID);

			// pack Pallet Z2
			var package = order2.PackageJob.Packages.AddNew("PLT");
			package.Pack(pickLine2, order2.Lines[0].ReleaseLines[0]);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine1.PK.ToGuid() }, "Z2");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertNotEquals("Should be another pickline", pickLine1.PK, response.NewPickLine.PKs[0]);

			var newPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			CombineAssertions(() =>
			{
				AssertNotNull(newPickLine);
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order1.Lines[0].PK, newPickLine.DocketLine.PK);
				AssertEquals("OrderLine must still have only 1 pickLine", 1, order1.Lines[0].PickLines.Count);
				AssertEquals("New pickLine must be in PickLines collection of original orderLine", newPickLine.PK, order1.Lines[0].PickLines[0].PK);
				AssertEquals("No new order lines should be created", 1, order1.Lines.Count);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 10m, newPickLine.WZ_Units);
			});
		}

		public void TestChangeAllocationToAnotherPalletID_CurrentPalletIsPacked()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine2 = pick2.GetAllPickLines().Single();
			AssertEquals("Precondition: Order 1 stock taken from Z1 by default", "Z1", pickLine1.Inventory.WI_PalletID);
			AssertEquals("Precondition: Order 2 stock taken from Z2 by default", "Z2", pickLine2.Inventory.WI_PalletID);

			// pack Pallet Z1
			var package = order1.PackageJob.Packages.AddNew("PLT");
			package.Pack(pickLine1, order1.Lines[0].ReleaseLines[0]);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine1.PK.ToGuid() }, "Z2");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertNotEquals("Should be another pickline", pickLine1.PK, response.NewPickLine.PKs[0]);

			var newPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			CombineAssertions(() =>
			{
				AssertNotNull(newPickLine);
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order1.Lines[0].PK, newPickLine.DocketLine.PK);
				AssertEquals("OrderLine must still have only 1 pickLine", 1, order1.Lines[0].PickLines.Count);
				AssertEquals("New pickLine must be in PickLines collection of original orderLine", newPickLine.PK, order1.Lines[0].PickLines[0].PK);
				AssertEquals("No new order lines should be created", 1, order1.Lines.Count);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 10m, newPickLine.WZ_Units);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ReleaseTotalsUnchanged

		public void TestChangeAllocationToAnotherPalletID_ReleaseTotalsUnchanged()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 4m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 6m);
			var pick = Helper.CreatePickNew(new[] { order1, order2 });

			var pickLines = pick.GetAllPickLines();
			CombineAssertions(() =>
			{
				AssertEquals("Precondition: stock taken from Z1 by default", true, pickLines.All(p => p.Inventory.WI_PalletID == "Z1"));
				AssertEquals("Precondition: UnitSent for order 1 should be 6m", 4m, order1.WD_UnitsSent);
				AssertEquals("Precondition: CubicSent for order 1 should be 0.08m", 0.08m, order1.WD_CubicSent);
				AssertEquals("Precondition: WeightSent for order 1 should be 8m", 8m, order1.WD_WeightSent);

				AssertEquals("Precondition: UnitSent for order 2 should be 6m", 6m, order2.WD_UnitsSent);
				AssertEquals("Precondition: CubicSent for order 2 should be 0.12m", 0.12m, order2.WD_CubicSent);
				AssertEquals("Precondition: WeightSent for order 2 should be 12m", 12m, order2.WD_WeightSent);

				AssertEquals("Precondition: Available Inventory pick line quantity should be 10m.", 10m, pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().First(a => a.PalletID == "Z1").PickLineQuantity);
				AssertEquals("Precondition: Available Inventory pick line quantity should be 0m.", 0m, pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().First(a => a.PalletID == "Z2").PickLineQuantity);
			});

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(pickLines.Select(p => p.PK.ToGuid()).ToArray(), "Z2");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));

			var newPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			var newOrder1 = webService.Factory.Load<WhsOrder>(new ZGuid(order1.PK));
			var newOrder2 = webService.Factory.Load<WhsOrder>(new ZGuid(order2.PK));
			var newPick = webService.Factory.Load<WhsPick>(new ZGuid(pick.PK));

			CombineAssertions(() =>
			{
				AssertNotNull(newPickLine);
				AssertEquals("UnitSent should be unchanged", 4m, newOrder1.WD_UnitsSent);
				AssertEquals("CubicSent should be unchanged", 0.08m, newOrder1.WD_CubicSent);
				AssertEquals("WeightSent should be unchanged", 8m, newOrder1.WD_WeightSent);

				AssertEquals("UnitSent should be unchanged", 6m, newOrder2.WD_UnitsSent);
				AssertEquals("CubicSent should be unchanged", 0.12m, newOrder2.WD_CubicSent);
				AssertEquals("WeightSent should be unchanged", 12m, newOrder2.WD_WeightSent);

				AssertEquals("Available Inventory pick line quantity should be cleard.", 0m, newPick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().First(a => a.PalletID == "Z1").PickLineQuantity);
				AssertEquals("Available Inventory pick line quantity should be increased.", 10m, newPick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().First(a => a.PalletID == "Z2").PickLineQuantity);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SamePalletID

		public void TestChangeAllocationToAnotherPalletID_SamePalletID()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().First();
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z1");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));

			AssertNull("Requested palletID matches original allocation, no new picklines should be created", response.NewPickLine);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_NotEnoughStock

		public void TestChangeAllocationToAnotherPalletID_NotEnoughStock()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			// another pallet will have only 9 items available
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 9m, data.Whs1.DefaultLocation, "Z2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(ai => ai.PalletID == "Z1");
			availableInventory.Allocate = true;
			Helper.Factory.Save();

			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Pallet Z2 cannot be selected for this pick.", response.ErrorMessage);
				AssertNull("No new picklines should be created", response.NewPickLine);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_WrongOrderedAttributes

		[TestDate(2015, 2, 5)]
		public void TestChangeAllocationToAnotherPalletID_WrongOrderedAttributes()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// set usage of all attributes as mandatory ones
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var packingDate = new ZDate(2015, 1, 1);
			var expiryDate = new ZDate(2016, 1, 1);

			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1", expiryDate, packingDate, "A1", "B1", "C1", "");
			var invalidReceiveLines = new[]
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2", expiryDate.AddDays(1), packingDate, "A1", "B1", "C1", ""),
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z3", expiryDate, packingDate.AddDays(1), "A1", "B1", "C1", ""),
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z4", expiryDate, packingDate, "WRONG", "B1", "C1", ""),
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z5", expiryDate, packingDate, "A1", "WRONG", "C1", ""),
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z6", expiryDate, packingDate, "A1", "B1", "WRONG", ""),
				// this one is another product
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "Z7", expiryDate, packingDate, "A1", "B1", "C1", "")
			};

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order, data.Part1, 10, expiryDate, packingDate, "A1", "B1", "C1", "", "");
			var pick = Helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			foreach (var receiveLine in invalidReceiveLines)
			{
				var webService = GetNewWebService(data.Whs1, staff);
				var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, receiveLine.WI_PalletID);
				AssertSuccessfulResponse(response, webService);
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals($"Pallet {receiveLine.WI_PalletID} cannot be selected for this pick.", response.ErrorMessage);
				AssertNull("No new picklines should be created", response.NewPickLine);
			}
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_NotOrderedAttributes

		[TestDate(2016, 1, 1)]
		public void TestChangeAllocationToAnotherPalletID_NotOrderedAttributes()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			// set usage of all attributes as mandatory
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var packingDate = new ZDate(2016, 1, 1);
			var expiryDate = new ZDate(2016, 1, 2);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1", expiryDate, packingDate, "A1", "B1", "C1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2", expiryDate.AddDays(1), packingDate.AddDays(1), "OTHER", "OTHER", "OTHER", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			// order line have no attributes specified
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);

			var pickLine = pick.GetAllPickLines().Single();
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));

			AssertNotEquals("Should be another pickline", pickLine.PK, response.NewPickLine.PKs[0]);
			var newPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			AssertNotNull(newPickLine);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_AttributeNeutralSerial

		public void TestChangeAllocationToAnotherPalletID_AttributeNeutralSerial()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, "Z1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, "Z1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 1m, data.Whs1.DefaultLocation, "Z1");
			receiveLine1.WE_SerialNumber = "S1";
			receiveLine2.WE_SerialNumber = "S2";
			receiveLine3.WE_SerialNumber = "S3";

			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive1.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 3m);
			var pick = Helper.CreatePickNew(order);

			var pickLines = pick.GetAllPickLines().ToList();
			AssertEquals("Should have one pick line per serial.", 3, pickLines.Count);
			Assert(pickLines.Any(pl => pl.WZ_WE_InventoryLine == receiveLine1.PK));
			Assert(pickLines.Any(pl => pl.WZ_WE_InventoryLine == receiveLine2.PK));
			Assert(pickLines.Any(pl => pl.WZ_WE_InventoryLine == receiveLine3.PK));

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m, data.Whs1.DefaultLocation, "Z2");
			var receiveLine5 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m, data.Whs1.DefaultLocation, "Z2");
			var receiveLine6 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m, data.Whs1.DefaultLocation, "Z2");
			receiveLine4.WE_SerialNumber = "S4";
			receiveLine5.WE_SerialNumber = "S5";
			receiveLine6.WE_SerialNumber = "S6";

			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive2.IsFinalised);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(pickLines.Select(pl => pl.PK.ToGuid()).ToArray(), "Z2");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));

			AssertEquals("Should have single pick line with PKs per serial.", 3, response.NewPickLine.PKs.Length);
			var newPickLine1 = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			var newPickLine2 = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[1]));
			var newPickLine3 = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[2]));
			var newPickLines = new[] { newPickLine1, newPickLine2, newPickLine3 };
			Assert(newPickLines.Any(pl => pl.WZ_WE_InventoryLine == receiveLine4.PK.ToGuid()));
			Assert(newPickLines.Any(pl => pl.WZ_WE_InventoryLine == receiveLine5.PK.ToGuid()));
			Assert(newPickLines.Any(pl => pl.WZ_WE_InventoryLine == receiveLine6.PK.ToGuid()));

			CombineAssertions(() =>
			{
				AssertNotNull(newPickLine1);
				AssertNotNull(newPickLine2);
				AssertNotNull(newPickLine3);
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine1.Inventory.WI_PalletID);
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine2.Inventory.WI_PalletID);
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine3.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order.Lines[0].PK, newPickLine1.DocketLine.PK);
				AssertEquals("New pickLine should be linked to the same orderLine", order.Lines[0].PK, newPickLine2.DocketLine.PK);
				AssertEquals("New pickLine should be linked to the same orderLine", order.Lines[0].PK, newPickLine3.DocketLine.PK);
				AssertEquals("OrderLine must still have 3 pickLines", 3, order.Lines[0].PickLines.Count);
				AssertEquals("No new order lines should be created", 1, order.Lines.Count);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 1m, newPickLine1.WZ_Units);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 1m, newPickLine1.WZ_Units);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 1m, newPickLine1.WZ_Units);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine = pick1.GetAllPickLines().Single();
			var pickLinePK = pickLine.PK;
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);

			AssertNotEquals("Should be another pickline", pickLinePK, response.NewPickLine.PKs[0]);
			var newPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));

			CombineAssertions(() =>
			{
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order1.Lines[0].PK, newPickLine.DocketLine.PK);
				AssertEquals("OrderLine must still have only 1 pickLine", 1, order1.Lines[0].PickLines.Count);
				AssertEquals("New pickLine must be in PickLines collection of original ordreLine", newPickLine.PK, order1.Lines[0].PickLines[0].PK);
				AssertEquals("No new order lines should be created", 1, order1.Lines.Count);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 10m, newPickLine.WZ_Units);
			});

			var reloadedOrder2 = webService.Factory.Load<WhsOrder>(new ZGuid(order2.PK));
			CombineAssertions(() =>
			{
				AssertEquals("Other order still has 1 order line line", 1, reloadedOrder2.AllLines.Count);
				AssertEquals("Other order still has 1 pick line", 1, reloadedOrder2.AllLines[0].PickLines.Count);
			});

			var pickLine2 = reloadedOrder2.AllLines[0].PickLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Another pickline should point to Z1 because of swap", "Z1", pickLine2.Inventory.WI_PalletID);
				AssertEquals("Quantity of new pickLine must be the same", 10m, pickLine2.WZ_Units);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_PickByBOM

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_CurrentPalletIsPickByBOM()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 10);
			Helper.CreatePickNew(order2);

			var pickLine = pick1.GetAllPickLines().Single(pl => !pl.IsPickByBOMKitPickLine());
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.NewPickLine);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Unable to pick Pallet Z2 because either the Product is a Component Line on a Sales Order or the Pick is in the process of Cartonization.", response.ErrorMessage);
		}

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_OtherPalletIsPickByBOM()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part2, 10);
			var pick1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			Helper.CreatePickNew(order2);

			var pickLine = pick1.GetAllPickLines().Single();
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.NewPickLine);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Unable to pick Pallet Z2 because either the Product is a Component Line on a Sales Order or the Pick is in the process of Cartonization.", response.ErrorMessage);
		}

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_CurrentPalletIsPickByBOM_SecondPalletIsNotCommitted()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			Helper.CreateProductBOM(data.Part1, data.Part2);
			data.Part1.OP_IsComponentPickedOnSalesOrder = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);

			var pickLine = pick1.GetAllPickLines().Single(pl => !pl.IsPickByBOMKitPickLine());
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);
			AssertNull(response.NewPickLine);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Unable to pick Pallet Z2 because either the Product is a Component Line on a Sales Order or the Pick is in the process of Cartonization.", response.ErrorMessage);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_ManyInventoryRows

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_ManyInventoryRows()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);

			for (int i = 1; i <= 250; i++)
			{
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z" + i.ToString().PadLeft(3, '0'));
			}

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine = pick1.GetAllPickLines().Single();
			var pickLinePK = pickLine.PK;
			AssertEquals("Precondition: stock taken from Z001 by default", "Z001", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z002");
			AssertSuccessfulResponse(response, webService);

			AssertNotEquals("Should be another pickline", pickLinePK, response.NewPickLine.PKs[0]);
			var newPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));

			CombineAssertions(() =>
			{
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z002", newPickLine.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order1.Lines[0].PK, newPickLine.DocketLine.PK);
				AssertEquals("OrderLine must still have only 1 pickLine", 1, order1.Lines[0].PickLines.Count);
				AssertEquals("New pickLine must be in PickLines collection of original orderLine", newPickLine.PK, order1.Lines[0].PickLines[0].PK);
				AssertEquals("No new order lines should be created", 1, order1.Lines.Count);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 10m, newPickLine.WZ_Units);
			});

			var reloadedOrder2 = webService.Factory.Load<WhsOrder>(new ZGuid(order2.PK));
			CombineAssertions(() =>
			{
				AssertEquals("Other order still has 1 order line line", 1, reloadedOrder2.AllLines.Count);
				AssertEquals("Other order still has 1 pick line", 1, reloadedOrder2.AllLines[0].PickLines.Count);
			});

			var pickLine2 = reloadedOrder2.AllLines[0].PickLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Another pickline should point to Z001 because of swap", "Z001", pickLine2.Inventory.WI_PalletID);
				AssertEquals("Quantity of new pickLine must be the same", 10m, pickLine2.WZ_Units);
			});

			var inventoryRowsInMemory = webService.Factory.Load<WhsInventoryView>(new ZQuery { FetchOnlyFromLocalCache = true });
			var docketLineRowsInMemory = webService.Factory.Load<WhsDocketLine>(new ZQuery { FetchOnlyFromLocalCache = true });
			AssertEquals("Should load only the original and intended inventory lines, not all available inventory.", 2, inventoryRowsInMemory.Length);
			AssertEquals("Should load only the order lines + the original and intended inventory lines, not all available inventory docketlines.", 4, docketLineRowsInMemory.Length);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_ReleaseTotalsUnchanged

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_ReleaseTotalsUnchanged()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine = pick1.GetAllPickLines().Single();
			var pickLinePK = pickLine.PK;
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: UnitSent for order 1 should be 10m", 10m, order1.WD_UnitsSent);
				AssertEquals("Precondition: CubicSent for order 1 should be 0.2m", 0.2m, order1.WD_CubicSent);
				AssertEquals("Precondition: WeightSent for order 1 should be 20m", 20m, order1.WD_WeightSent);

				AssertEquals("Precondition: UnitSent for order 2 should be 10m", 10m, order2.WD_UnitsSent);
				AssertEquals("Precondition: CubicSent for order 2 should be 0.2m", 0.2m, order2.WD_CubicSent);
				AssertEquals("Precondition: WeightSent for order 2 should be 20m", 20m, order2.WD_WeightSent);
			});

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);
			AssertNotEquals("Should be other pickline", pickLinePK, response.NewPickLine.PKs[0]);
			var newPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine.Inventory.WI_PalletID);

			CombineAssertions(() =>
			{
				AssertEquals("UnitSent for order 1 should be 10m", 10m, order1.WD_UnitsSent);
				AssertEquals("CubicSent for order 1 should be 0.2m", 0.2m, order1.WD_CubicSent);
				AssertEquals("WeightSent for order 1 should be 20m", 20m, order1.WD_WeightSent);

				AssertEquals("UnitSent for order 2 should be 10m", 10m, order2.WD_UnitsSent);
				AssertEquals("CubicSent for order 2 should be 0.2m", 0.2m, order2.WD_CubicSent);
				AssertEquals("WeightSent for order 2 should be 20m", 20m, order2.WD_WeightSent);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_TargetOrderLineHasMultiplePickLinesAndOneIsUsingTheOriginalInventory

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_TargetOrderLineHasMultiplePickLinesAndOneIsUsingTheOriginalInventory()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 11m, data.Whs1.DefaultLocation, "Z1");
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive1.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive2.IsFinalised);
			Helper.Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 11);
			var orderLine2 = order2.Lines[0];
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine1PK = pickLine1.PK;

			Helper.Factory.Save();

			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine1.Inventory.WI_PalletID);
			AssertEquals("Precondition: orderLine2 has 2 pick lines.", 2, orderLine2.PickLines.Count);
			AssertEquals("Precondition: one pick line has 10m.", 1, orderLine2.PickLines.Count(l => l.WZ_Units == 10m));
			AssertEquals("Precondition: one pick line has 1m.", 1, orderLine2.PickLines.Count(l => l.WZ_Units == 1m));
			AssertEquals("Precondition: one pick line's stock is taken from Z2.", "Z2", orderLine2.PickLines.Single(l => l.WZ_Units == 10m).Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine1.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);

			AssertNotEquals("Should be another pickline", pickLine1PK, response.NewPickLine.PKs[0]);
			var newPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));

			CombineAssertions(() =>
			{
				AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order1.Lines[0].PK, newPickLine.DocketLine.PK);
				AssertEquals("OrderLine must still have only 1 pickLine", 1, order1.Lines[0].PickLines.Count);
				AssertEquals("New pickLine must be in PickLines collection of original ordreLine", newPickLine.PK, order1.Lines[0].PickLines[0].PK);
				AssertEquals("No new order lines should be created", 1, order1.Lines.Count);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 10m, newPickLine.WZ_Units);
			});

			var reloadedOrder2 = webService.Factory.Load<WhsOrder>(order2.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Other order still has 1 order line line", 1, reloadedOrder2.AllLines.Count);
				AssertEquals("Other order still has 1 pick line", 1, reloadedOrder2.AllLines[0].PickLines.Count);
			});

			var pickLineForOrder2 = reloadedOrder2.AllLines[0].PickLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("PickLine of order2 should point to Z1 because of swapping.", "Z1", pickLineForOrder2.Inventory.WI_PalletID);
				AssertEquals("Quantity updated to 11.", 11m, pickLineForOrder2.WZ_Units);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_OriginalOrderLineHasMultiplePickLinesAndOneIsUsingTheTargetInventory

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_OriginalOrderLineHasMultiplePickLinesAndOneIsUsingTheTargetInventory()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 11m, data.Whs1.DefaultLocation, "Z1");
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive1.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive2.IsFinalised);
			Helper.Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 11);
			var orderLine2 = order2.Lines[0];
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine1PK = pickLine1.PK;

			var pickLine2 = pick2.GetAllPickLines().Single(l => l.WZ_Units == 10m);
			var pickLine3 = pick2.GetAllPickLines().Single(l => l.WZ_Units == 1m);

			Helper.Factory.Save();

			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine1.Inventory.WI_PalletID);
			AssertEquals("Precondition: orderLine2 has 2 pick lines.", 2, orderLine2.PickLines.Count);
			AssertEquals("Precondition: one pick line has 10m.", 1, orderLine2.PickLines.Count(l => l.WZ_Units == 10m));
			AssertEquals("Precondition: one pick line has 1m.", 1, orderLine2.PickLines.Count(l => l.WZ_Units == 1m));
			AssertEquals("Precondition: one pick line's stock is taken from Z2.", "Z2", orderLine2.PickLines.Single(l => l.WZ_Units == 10m).Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine2.PK.ToGuid() }, "Z1");
			AssertSuccessfulResponse(response, webService);

			AssertNotEquals("Should be another pickline", pickLine1PK, response.NewPickLine.PKs[0]);
			var updatedPickLine = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));

			CombineAssertions(() =>
			{
				AssertEquals("Updated pick line should be linked to the stock from the other pallet", "Z1", updatedPickLine.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order2.Lines[0].PK, updatedPickLine.DocketLine.PK);
				AssertEquals("OrderLine must still have only 1 pickLine", 1, order2.Lines[0].PickLines.Count);
				AssertEquals("Updated pickLine must be in PickLines collection of original ordreLine", updatedPickLine.PK, order2.Lines[0].PickLines[0].PK);
				AssertEquals("No new order lines should be created", 1, order1.Lines.Count);
				AssertEquals("Quantity updated to 11.", 11m, updatedPickLine.WZ_Units);
			});

			var reloadedOrder1 = webService.Factory.Load<WhsOrder>(order1.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Other order still has 1 order line line", 1, reloadedOrder1.AllLines.Count);
				AssertEquals("Other order still has 1 pick line", 1, reloadedOrder1.AllLines[0].PickLines.Count);
			});

			var pickLineForOrder1 = reloadedOrder1.AllLines[0].PickLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("PickLine of order1 should point to Z2 because of swapping.", "Z2", pickLineForOrder1.Inventory.WI_PalletID);
				AssertEquals("Quantity updated to 10.", 10m, pickLineForOrder1.WZ_Units);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_TargetInventoryIsNotAvailableToPick

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_TargetInventoryIsNotAvailableToPick()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine = pick1.GetAllPickLines().Single();
			var pickLinePK = pickLine.PK;
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			// make inventory not available to pick
			data.Whs1.DefaultLocation.WLV_LocationStatus = LocationStatus.Codes.Held;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Cannot Reallocate to another Pallet, as location status must be NOR - (Normal).", response.ErrorMessage);
			AssertNull("No new picklines should be created", response.NewPickLine);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_IsPicking

		public void TestChangeAllocationToAnotherPalletID_IsPicking_Success()
		{
			ChangeAllocationToAnotherPalletID_IsPicking_Core(success: true);
		}

		public void TestChangeAllocationToAnotherPalletID_IsPicking_Failed()
		{
			ChangeAllocationToAnotherPalletID_IsPicking_Core(success: false);
		}

		void ChangeAllocationToAnotherPalletID_IsPicking_Core(bool success)
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick2 = Helper.CreatePickNew(order2);
			if (!success)
			{
				// to make the proccess fail
				pick2.GetAllPickLines().First().WZ_PickedDateTime = ZDateTimeOffset.Today;
			}

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick1.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			Helper.Factory.Save();

			var pickLine1 = pick1.GetAllPickLines().Single();
			var pickLine2 = pick2.GetAllPickLines().Single();
			CombineAssertions(() =>
			{
				AssertEquals("Precondition, GetWhsPick should set is picking to true", true, pickLine1.WZ_IsPicking);
				AssertEquals("Precondition, GetWhsPick should not change picking (false)", false, pickLine2.WZ_IsPicking);
				AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine1.Inventory.WI_PalletID);
			});

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ChangeAllocationToAnotherPalletID(new[] { pickLine1.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response2, webService2);

			if (success)
			{
				var newPickLine = webService2.Factory.Load<WhsPickLine>(new ZGuid(response2.NewPickLine.PKs[0]));
				CombineAssertions(() =>
				{
					AssertEquals("New pick line should be linked to the stock from second pallet", "Z2", newPickLine.Inventory.WI_PalletID);
					AssertEquals("New pickline should be created", true, pickLine1.IsDeleted);
					AssertEquals("New pickline should be created", true, pickLine2.IsDeleted);
					AssertEquals("Operator is finished picking, new pickline should set to false", false, newPickLine.WZ_IsPicking);
					AssertEquals("Is saved", false, newPickLine.HasChanges);
					AssertEquals(true, pick1.GetAllPickLines().All(pl => !pl.WZ_IsPicking));
					AssertEquals(true, pick2.GetAllPickLines().All(pl => !pl.WZ_IsPicking));
				});
			}
			else
			{
				CombineAssertions(() =>
				{
					AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
					AssertEquals("Pallet Z2 cannot be selected for this pick.", response2.ErrorMessage);
					AssertNull("No new picklines should be created", response2.NewPickLine);
					AssertEquals("Should not change is picking when is failed to pick.", true, pickLine1.WZ_IsPicking);
					AssertEquals("Operator is finished picking, should stay to false", false, pickLine2.WZ_IsPicking);
					AssertEquals("Is saved", false, pickLine1.HasChanges);
					AssertEquals("Is saved", false, pickLine2.HasChanges);
				});
			}
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLinesFailsIfAnotherPicklineIsFinalised

		public void TestChangeAllocationToAnotherPalletID_SwapPickLinesFailsIfAnotherPickIsFinalised()
		{
			TestChangeAllocationToAnotherPalletID_SwapPickLinesFailsIfAnotherPickIsFinalised_Core(
				pl =>
				{
					var orderLine = pl.DocketLine;

					pl.WZ_PickedDateTime = ZDateTimeOffset.Now;

					using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
					{
						Helper.Factory.Save();
						AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, orderLine.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
					}
				});
		}

		public void TestChangeAllocationToAnotherPalletID_SwapPickLinesFailsIfAnotherPickIsFinalised_WithInTransitTransfer()
		{
			TestChangeAllocationToAnotherPalletID_SwapPickLinesFailsIfAnotherPickIsFinalised_Core(
				pl =>
				{
					var inTransitTransferLine = Helper.PickAndMakeInTransitTransfer(pl, ZDateTimeOffset.Now);
					inTransitTransferLine.FinaliseDocketLine();
					Helper.Factory.Save();
					AssertIsFinalisedPrecondition(inTransitTransferLine);

					// Clear caches to reflect newly finalised inventory
					foreach (var pick in Helper.Factory.Load<WhsPick>(new ZQuery()))
					{
						pick.ClearAllInventoriesCache();
						pick.OrderedInventories[0].ClearAvailableInventoriesCache();
					}
				});
		}

		void TestChangeAllocationToAnotherPalletID_SwapPickLinesFailsIfAnotherPickIsFinalised_Core(Action<WhsPickLine> pickPickLine)
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pickPickLine(order2.Lines[0].PickLines[0]);

			var pickLine = pick1.GetAllPickLines().Single();
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pallet Z2 cannot be selected for this pick.", response.ErrorMessage);
			AssertNull("No new picklines should be created", response.NewPickLine);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLinesFailsIfAnotherPicklineIsTaken

		public void TestChangeAllocationToAnotherPalletID_SwapPickLinesFailsIfAnotherPicklineIsTaken()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.GetAllPickLines().First().WZ_PickedDateTime = ZDateTimeOffset.Today;

			Helper.Factory.Save();

			var pickLine = pick1.GetAllPickLines().First();
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pallet Z2 cannot be selected for this pick.", response.ErrorMessage);
			AssertNull("No new picklines should be created", response.NewPickLine);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_FailsIfAnotherPickLineCannotBeSwitched

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_FailsIfAnotherPickLineCannotBeSwitched()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1", ZDate.Empty, ZDate.Empty, "AAA", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2", ZDate.Empty, ZDate.Empty, "BBB", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var pick1 = Helper.CreatePickNew(order1);
			// second order can be fullfilled only with pallet Z2
			Helper.CreateWhsOrderLine(order2, data.Part1, 10, ZDate.Empty, ZDate.Empty, "BBB", "", "", "", "");
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine1 = pick1.GetAllPickLines().First();
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine1.Inventory.WI_PalletID);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ChangeAllocationToAnotherPalletID(new[] { pickLine1.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response1, webService1);

			// we cannot switch to Z2 because orderLine from order2 cannot use pallet Z1 - it has ordered attribute which does not match
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
			AssertEquals("Pallet Z2 cannot be selected for this pick.", response1.ErrorMessage);
			AssertNull("No new picklines should be created", response1.NewPickLine);

			var pickLine2 = pick2.GetAllPickLines().First();
			AssertEquals("Precondition: stock taken from Z2 by default", "Z2", pickLine2.Inventory.WI_PalletID);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ChangeAllocationToAnotherPalletID(new[] { pickLine2.PK.ToGuid() }, "Z1");
			AssertSuccessfulResponse(response2, webService2);

			// we cannot switch to Z1 because orderLine from order2 cannot use pallet Z1 - it has ordered attribute which does not match
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Pallet Z1 cannot be selected for this pick.", response2.ErrorMessage);
			AssertNull("No new picklines should be created", response2.NewPickLine);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_FailsIfAvailableInventoryHasMultiplePickLinesAllocated

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_FailsIfAvailableInventoryHasMultiplePickLinesAllocated()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, "Z1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 15);
			var pick1 = Helper.CreatePickNew(order1);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick2 = Helper.CreatePickNew(order2);

			var pickLine = pick1.GetAllPickLines().Single(pl => pl.WZ_Units == 10m);
			var pickLinePK = pickLine.PK;
			AssertEquals("Precondition: stock taken from Z1 by default", "Z1", pickLine.Inventory.WI_PalletID);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertSuccessfulResponse(response, webService);

			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pallet Z2 cannot be selected for this pick.", response.ErrorMessage);
			AssertNull("No new picklines should be created", response.NewPickLine);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_FailsIfOneOrderLineHasReservedPickLineButNotPicked

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_FailsIfOneOrderLineHasReservedPickLineButNotPicked()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var pick1 = Helper.CreatePickNew(order1);
			Helper.CreateReservePickLine(order2.AllLines[0], inventory2, 10m);
			Helper.Factory.Save();

			var pickLine = pick1.GetAllPickLines().Single();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "Z2");
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pallet Z2 cannot be selected for this pick.", response.ErrorMessage);
			AssertNull("No new picklines should be created", response.NewPickLine);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_SucceedsIfOneOrderLineHasReservedPickLineAndPicked

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_SucceedsIfOneOrderLineHasReservedPickLineAndPicked()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "Z2");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var pickLine2 = orderLine2.ReserveStockIfAbleTo(inventory2, 10m);
			Helper.CreatePickNew(order1);
			Helper.CreatePickNew(order2);
			pickLine2.WZ_GS_NKAssignedTo = "ST1";
			pickLine2.WZ_IsPicking = true;
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine2.PK.ToGuid() }, "Z1");

			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));
			AssertNotEquals("Should be another pickline", pickLine2.PK, response.NewPickLine.PKs[0]);

			var newPickLine = Helper.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			CombineAssertions(() =>
			{
				AssertNotNull(newPickLine);
				AssertEquals("New pick line should be linked to the stock from first pallet", "Z1", newPickLine.Inventory.WI_PalletID);
				AssertEquals("New pickLine should be linked to the same orderLine", order2.Lines[0].PK, newPickLine.DocketLine.PK);
				AssertEquals("OrderLine should have 2 pickLines as the reserved one is retained.", 2, order2.Lines[0].PickLines.Count);
				AssertCollectionContains("New pickLine must be in PickLines collection of original orderLine", newPickLine.PK, order2.Lines[0].PickLines.GetPKs());
				AssertEquals("No new order lines should be created", 1, order2.Lines.Count);
				AssertEquals("Quantity of new pickLine must be the same as an old one", 10m, newPickLine.WZ_Units);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_MultiplePickLines

		public void TestChangeAllocationToAnotherPalletID_MultiplePickLines()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			// 5 + 2 + 3 = 10 UNT on Pallet PLT-1
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.DefaultLocation, "PLT-1");

			// 4 + 6 = 10 UNT on Pallet PLT-2
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, data.Whs1.DefaultLocation, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, data.Whs1.DefaultLocation, "PLT-2");

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventories = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			var availInvForPLT1 = availableInventories.Single(ai => ai.PalletID == "PLT-1");
			var availInvForPLT2 = availableInventories.Single(ai => ai.PalletID == "PLT-2");

			// we want pick to be allocated to PLT-1
			if (availInvForPLT2.PickLineQuantity > 0)
			{
				availInvForPLT2.PickLineQuantity = 0;
				availInvForPLT1.PickLineQuantity = 10;
			}

			AssertEquals("Precondition: pick allocated to PLT-1", true, order.Lines[0].PickLines.All(pl => pl.InventoryLine.WE_PalletID == "PLT-1"));

			var pickLinePKs = availInvForPLT1.PickLines.Select(pl => pl.PK.ToGuid()).ToArray();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(pickLinePKs, "PLT-2");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));

			CombineAssertions(() =>
			{
				AssertEquals(2, response.NewPickLine.PKs.Length);
				foreach (var newPK in response.NewPickLine.PKs)
				{
					AssertEquals("All pickline PKs must be new", false, pickLinePKs.Contains(newPK));
				}
			});

			var newPickLines = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			CombineAssertions(() =>
			{
				AssertEquals("There should be a pickline for 6 units and PLT-2", true, newPickLines.Any(pl => pl.WZ_Units == 6m && pl.InventoryLine.WE_PalletID == "PLT-2"));
				AssertEquals("There should be a pickline for 4 units and PLT-2", true, newPickLines.Any(pl => pl.WZ_Units == 4m && pl.InventoryLine.WE_PalletID == "PLT-2"));
				AssertEquals("New picklines must point to existing order line", true, newPickLines.All(pl => pl.DocketLine.PK == order.Lines[0].PK));
				AssertEquals("No new order lines should be created", 1, order.Lines.Count);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_EmptyPickLinePKList

		public void TestChangeAllocationToAnotherPalletID_EmptyPickLinePKList()
		{
			var webService = GetNewWebService();
			AssertExceptionThrown<ArgumentException>(() => webService.ChangeAllocationToAnotherPalletID(Array.Empty<Guid>(), "PLT-1"));
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_EmptyPalletID

		public void TestChangeAllocationToAnotherPalletID_EmptyPalletID()
		{
			var webService = GetNewWebService();
			AssertExceptionThrown<ArgumentException>(() => webService.ChangeAllocationToAnotherPalletID(new[] { Guid.NewGuid() }, ""));
			AssertExceptionThrown<ArgumentException>(() => webService.ChangeAllocationToAnotherPalletID(new[] { Guid.NewGuid() }, "     "));
			AssertExceptionThrown<ArgumentException>(() => webService.ChangeAllocationToAnotherPalletID(new[] { Guid.NewGuid() }, null));
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_WhenPickLinesAreRelatedToDifferentAvailableInventoriesButTheSameOrderedInventory

		public void TestChangeAllocationToAnotherPalletID_WhenPickLinesAreRelatedToDifferentAvailableInventoriesButTheSameOrderedInventory()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			// 4 BLUE + 6 RED = 10 UNT on Pallet PLT-1
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "BLUE", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "RED", "", "", "");

			// plenty on Pallet PLT-2
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.DefaultLocation, "PLT-2", ZDate.Empty, ZDate.Empty, "BLUE", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.DefaultLocation, "PLT-2", ZDate.Empty, ZDate.Empty, "RED", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10);
			var pick = Helper.CreatePickNew(order);

			var availableInventories = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			var availInvForPLT1BLUE = availableInventories.Single(ai => ai.PalletID == "PLT-1" && ai.PartAttrib1 == "BLUE");
			var availInvForPLT1RED = availableInventories.Single(ai => ai.PalletID == "PLT-1" && ai.PartAttrib1 == "RED");

			var availInvForPLT2BLUE = availableInventories.Single(ai => ai.PalletID == "PLT-2" && ai.PartAttrib1 == "BLUE");
			var availInvForPLT2RED = availableInventories.Single(ai => ai.PalletID == "PLT-2" && ai.PartAttrib1 == "RED");
			if (availInvForPLT2BLUE.PickLineQuantity > 0 || availInvForPLT2RED.PickLineQuantity > 0)
			{
				availInvForPLT1BLUE.PickLineQuantity = 0;
				availInvForPLT2BLUE.PickLineQuantity = 0;
				availInvForPLT1RED.PickLineQuantity = 0;
				availInvForPLT2RED.PickLineQuantity = 0;

				availInvForPLT1BLUE.PickLineQuantity = 4;
				availInvForPLT1RED.PickLineQuantity = 6;
			}

			AssertEquals("Precondition: Order is fully picked.", 10m, order.Lines[0].PickLineQuantity);
			AssertEquals("Precondition: pick allocated to PLT-1.", true, order.Lines[0].PickLines.All(pl => pl.InventoryLine.WE_PalletID == "PLT-1"));

			Helper.Factory.Save();

			var pickLinePKs = order.Lines[0].PickLines.Select(pl => pl.PK.ToGuid()).ToArray();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(pickLinePKs, "PLT-2");
			AssertSuccessfulResponse(response, webService);

			CombineAssertions(() =>
			{
				AssertNull("No errors should be reported.", response.ErrorMessage);
				AssertEquals("Total picked units must remain the same.", 10m, response.NewPickLine.Units);
				AssertEquals("PLT-2", response.NewPickLine.PalletID);

				AssertEquals("Order is fully picked.", 10m, order.Lines[0].PickLineQuantity);
				AssertEquals("Pick allocated to PLT-2.", true, order.Lines[0].PickLines.All(pl => pl.InventoryLine.WE_PalletID == "PLT-2"));
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_NotWorkingForMultipleOrderedInventories

		public void TestChangeAllocationToAnotherPalletID_NotWorkingForMultipleOrderedInventories()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			// 4 BLUE + 6 RED = 10 UNT on Pallet PLT-1
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "BLUE", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, data.Whs1.DefaultLocation, "PLT-1", ZDate.Empty, ZDate.Empty, "RED", "", "", "");

			// plenty on Pallet PLT-2
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.DefaultLocation, "PLT-2", ZDate.Empty, ZDate.Empty, "BLUE", "", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m, data.Whs1.DefaultLocation, "PLT-2", ZDate.Empty, ZDate.Empty, "RED", "", "", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLineBlue = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			orderLineBlue.WE_PartAttrib1 = "BLUE";
			var orderLineAny = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var pick = Helper.CreatePickNew(order);

			var availableInventoriesForBlueOrderLine = pick.OrderedInventories.GetOrderedInventoryForLine(orderLineBlue).AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			foreach (var availableInventory in availableInventoriesForBlueOrderLine)
			{
				availableInventory.PickLineQuantity = 0;
			}
			var availableInventoriesForAnyOrderLine = pick.OrderedInventories.GetOrderedInventoryForLine(orderLineAny).AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			foreach (var availableInventory in availableInventoriesForAnyOrderLine)
			{
				availableInventory.PickLineQuantity = 0;
			}

			availableInventoriesForBlueOrderLine.Single(ai => ai.PalletID == "PLT-1" && ai.PartAttrib1 == "BLUE").PickLineQuantity = 4m;
			availableInventoriesForAnyOrderLine.Single(ai => ai.PalletID == "PLT-1" && ai.PartAttrib1 == "RED").PickLineQuantity = 6m;

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: Order is fully picked.", 10m, order.Lines.Cast<WhsOrderLine>().Sum(l => l.PickLineQuantity));
				AssertEquals("Precondition: pick allocated to PLT-1.", true, order.Lines.SelectMany(l => l.PickLines).All(pl => pl.InventoryLine.WE_PalletID == "PLT-1"));
			});

			Helper.Factory.Save();

			var pickLinePKs = pick.GetAllPickLines().Select(pl => pl.PK.ToGuid()).ToArray();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(pickLinePKs, "PLT-2");
			AssertSuccessfulResponse(response, webService);

			// Even though it looks completely valid to change allocation to PLT-2 it is too complicated to implement 
			// and it is a very rare case.
			// So we (DRD + MMC) decided to not to handle this case and force the user to get original palletID

			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Pallet PLT-2 cannot be selected for this pick.", response.ErrorMessage);
				AssertNull(response.NewPickLine);
			});
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_NotWorkingWhenTryingToSwapWithManyPickLines

		public void TestChangeAllocationToAnotherPalletID_NotWorkingWhenTryingToSwapWithManyPickLines()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			// 5 + 2 + 3 = 10 UNT on Pallet PLT-1
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.DefaultLocation, "PLT-1");

			// 4 + 6 = 10 UNT on Pallet PLT-2
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, data.Whs1.DefaultLocation, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, data.Whs1.DefaultLocation, "PLT-2");

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var pick1 = Helper.CreatePickNew(order1);

			var availableInventoriesForPick1 = pick1.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			var availInvForPick1PLT1 = availableInventoriesForPick1.Single(ai => ai.PalletID == "PLT-1");
			var availInvForPick1PLT2 = availableInventoriesForPick1.Single(ai => ai.PalletID == "PLT-2");

			// we want pick1 to be allocated to PLT-1
			if (availInvForPick1PLT2.PickLineQuantity > 0)
			{
				availInvForPick1PLT2.PickLineQuantity = 0;
				availInvForPick1PLT1.PickLineQuantity = 10;
			}

			// allocate PLT-2 to pick 2
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick2 = Helper.CreatePickNew(order2);
			AssertEquals("Precondition: pick2 allocated successfully", 10m, pick2.GetAllPickLines().Sum(pl => pl.WZ_Units));

			// now let's pretend that guy who is picking pick1 is trying to take PLT-2
			// Currently this is a very rare situation and we don't handle it.

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ChangeAllocationToAnotherPalletID(pick1.GetAllPickLines().Select(pl => pl.PK.ToGuid()).ToArray(), "PLT-2");
			AssertSuccessfulResponse(response1, webService1);
			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);
				AssertEquals("Pallet PLT-2 cannot be selected for this pick.", response1.ErrorMessage);
				AssertNull(response1.NewPickLine);
			});

			// and this shouldn't work vice versa
			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ChangeAllocationToAnotherPalletID(pick2.GetAllPickLines().Select(pl => pl.PK.ToGuid()).ToArray(), "PLT-1");
			AssertSuccessfulResponse(response2, webService2);
			CombineAssertions(() =>
			{
				AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
				AssertEquals("Pallet PLT-1 cannot be selected for this pick.", response2.ErrorMessage);
				AssertNull(response2.NewPickLine);
			});

			// in future if someone will make ChangeAllocationToAnotherPalletID method to swap sets of picklines - the assertions above should be changed.
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_DifferentLocation

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_DifferentLocation()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT2");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-2"), "PLT3");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 10);
			var pick = Helper.CreatePickNew(order1, order2, order3);

			var pickLine1 = pick.GetAllPickLines().Single(pl => pl.WZ_WE_InventoryLine == receiveLine1.PK);
			var pickLine2 = pick.GetAllPickLines().Single(pl => pl.WZ_WE_InventoryLine == receiveLine2.PK);
			var pickLine3 = pick.GetAllPickLines().Single(pl => pl.WZ_WE_InventoryLine == receiveLine3.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Precondition:", "PLT1", pickLine1.Inventory.WI_PalletID);
				AssertEquals("Precondition:", "PLT2", pickLine2.Inventory.WI_PalletID);
				AssertEquals("Precondition:", "PLT3", pickLine3.Inventory.WI_PalletID);
			});

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.ChangeAllocationToAnotherPalletID(new[] { pickLine1.PK.ToGuid() }, "PLT3");
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("Should have error.", "You can only pick a pallet from the allocated location.", response1.ErrorMessage);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ChangeAllocationToAnotherPalletID(new[] { pickLine1.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response2, webService2);

			var pickLinePLT2 = webService2.Factory.Load<WhsPickLine>(new ZGuid(response2.NewPickLine.PKs[0]));
			AssertEquals("Should be able to scan different pallet.", "PLT2", pickLinePLT2.Inventory.WI_PalletID);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_GetCompletePallets_SwapPalletIDs

		public void TestChangeAllocationToAnotherPalletID_GetCompletePallets_SwapPalletIDs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			location.LocationType.WLT_IsPalletIDNeutral = true;
			data.Part1.RelatedOrganisations[0].OU_CompletePalletPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(receiveLine1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			orderLine2.ReserveStockIfAbleTo(receiveLine2);

			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();

			var pick2 = Helper.CreatePickNew(order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertNotNull(response1.Pick);
			AssertEquals("Precondition: PLT1 can be complete picked", "PLT1", response1.Pick.CompletePalletPickingPallets.Single());

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.GetWhsPick(pick2.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertNotNull(response2.Pick);
			AssertEquals("Precondition: PLT2 can be complete picked", "PLT2", response2.Pick.CompletePalletPickingPallets.Single());

			var webService3 = GetNewWebService(data.Whs1);
			var response3 = webService3.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "PLT2");
			AssertEquals("New PalletID should be assgined", "PLT2", response3.CompletePalletPickingPallets.Single());
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_GetCompletePallets_AssignNewAvailablePalletID

		public void TestChangeAllocationToAnotherPalletID_GetCompletePallets_AssignNewAvailablePalletID()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			location.LocationType.WLT_IsPalletIDNeutral = true;
			data.Part1.RelatedOrganisations[0].OU_CompletePalletPicking = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.ReserveStockIfAbleTo(receiveLine1);

			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertNotNull(response1.Pick);
			AssertEquals("Precondition: PLT1 can be complete picked", "PLT1", response1.Pick.CompletePalletPickingPallets.Single());

			var webService2 = GetNewWebService(data.Whs1);
			var response2 = webService2.ChangeAllocationToAnotherPalletID(new[] { pickLine.PK.ToGuid() }, "PLT2");
			AssertEquals("New PalletID should be assgined", "PLT2", response2.CompletePalletPickingPallets.Single());
		}
		#endregion

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_OnePackageOneDivot

		public void TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_OnePackageOneDivot()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var pickLinePLT1 = pick.GetAllPickLines().Single(pl => pl.WZ_WE_InventoryLine == receiveLine1.PK);
			AssertEquals("Precondition:", "PLT1", pickLinePLT1.Inventory.WI_PalletID);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			package.Pack(pickLinePLT1, order.Lines[0].ReleaseLines[0]);
			Helper.Factory.Save();

			var packageDivotsPLT1 = package.PackedItemDivots;
			AssertEquals("Precondition: Pkg Divot for PLT1 exists", 1, packageDivotsPLT1.Count);
			AssertEquals("Precondition: Pkg Divot has correct ParentID", true, packageDivotsPLT1.First().KI_ParentID.Equals(pickLinePLT1.PK));
			AssertEquals("Precondition: Pkg Divot has correct PackedQty", 10m, packageDivotsPLT1.First().KI_PackedQty);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLinePLT1.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinePLT2 = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));
			AssertEquals("Should be able to scan different pallet.", "PLT2", pickLinePLT2.Inventory.WI_PalletID);

			var packageDivotsPLT2 = package.PackedItemDivots;
			AssertEquals("Pkg Divot for PLT2 exists", 1, packageDivotsPLT2.Count);
			AssertEquals("Pkg Divot has correct ParentID", true, packageDivotsPLT2.First().KI_ParentID.Equals(pickLinePLT2.PK));
			AssertEquals("Pkg Divot has correct PackedQty", 10m, packageDivotsPLT2.First().KI_PackedQty);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_OnePackageMultiplePicklines

		public void TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_OnePackageMultiplePicklines()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1A = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine1B = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2A = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, data.Whs1.FindLocation("A-1"), "PLT2");
			var receiveLine2B = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.FindLocation("A-1"), "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			order.WD_PickOption = WhsPickOption.Codes.Manual;
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(ai => ai.PalletID == "PLT1");
			availableInventory.Allocate = true;
			Helper.Factory.Save();

			var picklinesPLT1 = pick.GetAllPickLines();
			AssertEquals("Precondition: Correct picklines count", 2, picklinesPLT1.Count());
			var pickLinePLT1A = picklinesPLT1.Single(pl => pl.WZ_WE_InventoryLine == receiveLine1A.PK);
			var pickLinePLT1B = picklinesPLT1.Single(pl => pl.WZ_WE_InventoryLine == receiveLine1B.PK);
			AssertEquals("Precondition: PLT1 correct pickline A", "PLT1", pickLinePLT1A.Inventory.WI_PalletID);
			AssertEquals("Precondition: PLT1 correct pickline B", "PLT1", pickLinePLT1B.Inventory.WI_PalletID);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			package.Pack(pickLinePLT1A, order.Lines[0].ReleaseLines[0]);
			package.Pack(pickLinePLT1B, order.Lines[0].ReleaseLines[0]);
			Helper.Factory.Save();

			var packageDivotsPLT1 = package.PackedItemDivots;
			AssertEquals("Precondition: Pkg Divots for PLT1 exists", 2, packageDivotsPLT1.Count);

			var pickLine1ADivot = packageDivotsPLT1.Single(d => d.KI_ParentID.Equals(pickLinePLT1A.PK));
			AssertEquals("Precondition: Pkg Divot has correct PackedQty", 3m, pickLine1ADivot.KI_PackedQty);
			var pickLine1BDivot = packageDivotsPLT1.Single(d => d.KI_ParentID.Equals(pickLinePLT1B.PK));
			AssertEquals("Precondition: Pkg Divot has correct PackedQty", 7m, pickLine1BDivot.KI_PackedQty);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLinePLT1A.PK.ToGuid(), pickLinePLT1B.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinesPLT2 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			AssertEquals("All picklines returned should point to PLT2.", true, pickLinesPLT2.All(p => p.Inventory.WI_PalletID.Equals("PLT2")));
			var pickLinePLT2A = pickLinesPLT2.Single(p => p.WZ_Units.Equals(8m) && p.InventoryLine.PK.Equals(receiveLine2A.PK));
			var pickLinePLT2B = pickLinesPLT2.Single(p => p.WZ_Units.Equals(2m) && p.InventoryLine.PK.Equals(receiveLine2B.PK));

			var packageDivotsPLT2 = package.PackedItemDivots;
			AssertEquals("Pkg Divots for PLT2 exists", 2, packageDivotsPLT2.Count);
			AssertNotNull("Pkg DivotA connected to correct ParentID and package", packageDivotsPLT2.Single(d => d.KI_ParentID == pickLinePLT2A.PK && d.KI_PackedQty == 8m));
			AssertNotNull("Pkg DivotB connected to correct ParentID and package", packageDivotsPLT2.Single(d => d.KI_ParentID == pickLinePLT2B.PK && d.KI_PackedQty == 2m));
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesOnePicklineEach

		public void TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesOnePicklineEach()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var orderLine = order.Lines.Cast<WhsOrderLine>().Single();
			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			package1.Pack(orderLine.ReleaseLines[0], 2m);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Carton);
			package2.Pack(orderLine.ReleaseLines[0], 3m);
			Helper.Factory.Save();

			AssertEquals("Precondition: Correct number of picklines", 2, orderLine.PickLines.Count);
			var pickline2U = orderLine.PickLines.Single(p => p.WZ_Units == 2m);
			var pickline3U = orderLine.PickLines.Single(p => p.WZ_Units == 3m);

			var package1DivotsPLT1 = package1.PackedItemDivots;
			AssertEquals("Precondition: Pkg Divots for PLT1 PKG1 exists", 1, package1DivotsPLT1.Count);
			var package2DivotsPLT1 = package2.PackedItemDivots;
			AssertEquals("Precondition: Pkg Divots for PLT1 PKG2 exists", 1, package2DivotsPLT1.Count);

			var pickLine1ADivot = package1DivotsPLT1.Single(d => d.KI_ParentID.Equals(pickline2U.PK));
			AssertEquals("Precondition: Pkg Divot has correct PackedQty", 2m, pickLine1ADivot.KI_PackedQty);
			var pickLine1BDivot = package2DivotsPLT1.Single(d => d.KI_ParentID.Equals(pickline3U.PK));
			AssertEquals("Precondition: Pkg Divot has correct PackedQty", 3m, pickLine1BDivot.KI_PackedQty);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickline2U.PK.ToGuid(), pickline3U.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinesPLT2 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			AssertEquals("Correct amount of picklines should exist.", 2, pickLinesPLT2.Length);
			AssertEquals("All picklines returned should point to PLT2.", true, pickLinesPLT2.All(p => p.Inventory.WI_PalletID.Equals("PLT2")));
			var pickLinePLT2A = pickLinesPLT2.Single(p => p.WZ_Units.Equals(2m) && p.InventoryLine.PK.Equals(receiveLine2.PK));
			var pickLinePLT2B = pickLinesPLT2.Single(p => p.WZ_Units.Equals(3m) && p.InventoryLine.PK.Equals(receiveLine2.PK));

			var package1DivotsPLT2 = package1.PackedItemDivots;
			AssertEquals("Pkg Divots PKG1 for PLT2 exists", 1, package1DivotsPLT2.Count);
			AssertNotNull("Pkg DivotA connected to correct ParentID and package", package1DivotsPLT2.Single(d => d.KI_ParentID == pickLinePLT2A.PK && d.KI_PackedQty == 2m));
			var package2DivotsPLT2 = package2.PackedItemDivots;
			AssertEquals("Pkg Divot PKG2 for PLT2 exists", 1, package1DivotsPLT2.Count);
			AssertNotNull("Pkg DivotB connected to correct ParentID and package", package2DivotsPLT2.Single(d => d.KI_ParentID == pickLinePLT2B.PK && d.KI_PackedQty == 3m));
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesSwapPicklines

		public void TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesSwapPicklines()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			Helper.CreatePickNew(order1);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);
			Helper.CreatePickNew(order2);
			Helper.Factory.Save();

			var pickline1 = orderLine1.PickLines.Single(p => p.WZ_WE_InventoryLine == receiveLine1.PK);
			var pickline2 = orderLine2.PickLines.Single(p => p.WZ_WE_InventoryLine == receiveLine2.PK);

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			package1.Pack(pickline1, orderLine1.ReleaseLines[0]);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Carton);
			package2.Pack(pickline2, orderLine2.ReleaseLines[0]);
			Helper.Factory.Save();

			var packageDivotsPLT1 = package1.PackedItemDivots;
			AssertEquals("Precondition: Pkg Divots for PLT1 exists", 1, packageDivotsPLT1.Count);
			var pickLine1Divot = packageDivotsPLT1.Single(d => d.KI_ParentID.Equals(pickline1.PK));
			AssertEquals("Precondition: Pkg Divot has correct PackedQty", 5m, pickLine1Divot.KI_PackedQty);

			var packageDivotsPLT2 = package2.PackedItemDivots;
			AssertEquals("Precondition: Pkg Divots for PLT2 exists", 1, packageDivotsPLT2.Count);
			var pickLine2Divot = packageDivotsPLT2.Single(d => d.KI_ParentID.Equals(pickline2.PK));
			AssertEquals("Precondition: Pkg Divot has correct PackedQty", 5m, pickLine2Divot.KI_PackedQty);

			// Run webservice
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickline1.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinesPLT2 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			AssertEquals("All picklines returned should point to PLT2.", true, pickLinesPLT2.All(p => p.Inventory.WI_PalletID.Equals("PLT2")));
			AssertEquals("Pickline count for PLT2 is 1.", 1, pickLinesPLT2.Length);
			var pickLinePLT2 = pickLinesPLT2.Single(p => p.WZ_Units.Equals(5m) && p.InventoryLine.PK.Equals(receiveLine2.PK));
			AssertEquals("Pickline to correct orderline.", true, pickLinePLT2.WZ_WE_TransactionLine == orderLine1.PK);

			var package1DivotsAfter = package1.PackedItemDivots;
			AssertEquals("Correct Pkg1 Divot count exists", 1, package1DivotsAfter.Count);
			AssertNotNull("Pkg1 Divot connected to correct ParentID and package", package1DivotsAfter.Single(d => d.KI_ParentID == pickLinePLT2.PK && d.KI_PackedQty == 5m));

			var pickline2After = orderLine2.PickLines.Single();
			AssertEquals("Pickline to correct receiveline.", true, pickline2After.WZ_WE_InventoryLine == receiveLine1.PK);
			var package2DivotsAfter = package2.PackedItemDivots;
			AssertEquals("Correct Pkg2 Divot count exists", 1, package2DivotsAfter.Count);
			AssertNotNull("Pkg2 Divot connected to correct ParentID and package", package2DivotsAfter.Single(d => d.KI_ParentID == pickline2After.PK && d.KI_PackedQty == 5m));
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesMultiplePicklines

		public void TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesMultiplePicklines()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT2");
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.FindLocation("A-1"), "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var pickline1 = orderLine1.PickLines.Single();
			var pickline2 = orderLine2.PickLines.Single();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			package1.Pack(pickline1, orderLine1.ReleaseLines[0]);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Carton);
			package2.Pack(pickline2, orderLine2.ReleaseLines[0]);
			Helper.Factory.Save();

			// Run webservice
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickline1.PK.ToGuid(), pickline2.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinesPLT2 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			AssertEquals("All picklines returned should point to PLT2.", true, pickLinesPLT2.All(p => p.Inventory.WI_PalletID.Equals("PLT2")));
			AssertEquals("Pickline count for PLT2 is 2.", 2, pickLinesPLT2.Length);
			var pickLine1After = pickLinesPLT2.Single(p => p.WZ_Units.Equals(1m) && p.WZ_WE_TransactionLine.Equals(orderLine1.PK));
			var pickLine2After = pickLinesPLT2.Single(p => p.WZ_Units.Equals(1m) && p.WZ_WE_TransactionLine.Equals(orderLine2.PK));
			AssertEquals("pickLine1After is Not connected to same receiveline as pickLine2After.", true, pickLine1After.WZ_WE_InventoryLine != pickLine2After.WZ_WE_InventoryLine);
			AssertEquals("pickLine1After is connected to a correct receiveline.", true, pickLine1After.WZ_WE_InventoryLine == receiveLine3.PK || pickLine1After.WZ_WE_InventoryLine == receiveLine4.PK);
			AssertEquals("pickLine2After is connected to a correct receiveline.", true, pickLine2After.WZ_WE_InventoryLine == receiveLine3.PK || pickLine2After.WZ_WE_InventoryLine == receiveLine4.PK);

			var package1DivotsAfter = package1.PackedItemDivots;
			AssertEquals("Correct Pkg1 Divot count exists", 1, package1DivotsAfter.Count);
			var package1Divot = package1DivotsAfter.First();
			AssertEquals("Correct Pkg1 Divot PackedQty", 1m, package1Divot.KI_PackedQty);
			AssertEquals("Has Correct pickline from Divot", pickLine1After.PK, package1Divot.KI_ParentID);

			var package2DivotsAfter = package2.PackedItemDivots;
			AssertEquals("Correct Pkg2 Divot count exists", 1, package2DivotsAfter.Count);
			var package2Divot = package2DivotsAfter.First();
			AssertEquals("Correct Pkg2 Divot PackedQty", 1m, package2Divot.KI_PackedQty);
			AssertEquals("Has Correct pickline from Divot", pickLine2After.PK, package2Divot.KI_ParentID);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesSplitingPicklines

		public void TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesOneReceiveLineToTwo()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "PLT2");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.FindLocation("A-1"), "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 7m);
			var pick = Helper.CreatePickNew(order);

			var orderedInventory = pick.OrderedInventories.Cast<WhsPickOrderedInventory>().Single();
			var availableInventory = orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Single(ai => ai.PalletID == "PLT1");
			availableInventory.Allocate = true;
			Helper.Factory.Save();

			var pickline = orderLine.PickLines.Single(p => p.WZ_WE_InventoryLine == receiveLine1.PK);
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			package.Pack(pickline, orderLine.ReleaseLines[0]);
			Helper.Factory.Save();

			// Run webservice
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickline.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinesPLT2 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			AssertEquals("All picklines returned should point to PLT2.", true, pickLinesPLT2.All(p => p.Inventory.WI_PalletID.Equals("PLT2")));
			AssertEquals("Pickline count for PLT2 is 2.", 2, pickLinesPLT2.Length);
			var pickLine1After = pickLinesPLT2.Single(p => p.WZ_Units.Equals(5m));
			var pickLine2After = pickLinesPLT2.Single(p => p.WZ_Units.Equals(2m));
			AssertContainsExactElementsInAnyOrder(new[] { receiveLine2.PK, receiveLine3.PK }, new[] { pickLine1After.WZ_WE_InventoryLine, pickLine2After.WZ_WE_InventoryLine });
			AssertEquals("pickLine1After is connected to correct orderline.", true, pickLine1After.WZ_WE_TransactionLine == orderLine.PK);
			AssertEquals("pickLine2After is connected to correct orderline.", true, pickLine2After.WZ_WE_TransactionLine == orderLine.PK);

			var package1DivotsAfter = package.PackedItemDivots;
			AssertEquals("Correct Pkg1 Divot count exists", 2, package1DivotsAfter.Count);
			AssertNotNull("Pkg1 Divot1 connected to correct ParentID and package", package1DivotsAfter.Single(d => d.KI_ParentID == pickLine1After.PK && d.KI_PackedQty == 5m));
			AssertNotNull("Pkg1 Divot2 connected to correct ParentID and package", package1DivotsAfter.Single(d => d.KI_ParentID == pickLine2After.PK && d.KI_PackedQty == 2m));
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesFromTwoReceiveLinesToOne

		public void TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_MultiplePackagesFromTwoReceiveLinesToOne()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var packingHelper = new PackingTestHelper(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.FindLocation("A-1"), "PLT1");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.FindLocation("A-1"), "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var pickLine1 = orderLine1.PickLines.AddNew();
			pickLine1.WZ_WE_InventoryLine = receiveLine1.PK;
			pickLine1.WZ_WE_TransactionLine = orderLine1.PK;
			pickLine1.WZ_Units = 4m;

			var pickLine2 = orderLine1.PickLines.AddNew();
			pickLine2.WZ_WE_InventoryLine = receiveLine2.PK;
			pickLine2.WZ_WE_TransactionLine = orderLine2.PK;
			pickLine2.WZ_Units = 3m;
			Helper.Factory.Save();

			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			package1.Pack(pickLine1, orderLine1.ReleaseLines[0]);

			var package2 = packingHelper.CreatePackage(pkgJob, "PKG2", 1, Constants.PkgUnit.Carton);
			package2.Pack(pickLine2, orderLine2.ReleaseLines[0]);
			Helper.Factory.Save();

			// Run webservice
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine1.PK.ToGuid(), pickLine2.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinesPLT2 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			AssertEquals("All picklines returned should point to PLT2.", true, pickLinesPLT2.All(p => p.Inventory.WI_PalletID.Equals("PLT2")));
			AssertEquals("Pickline count for PLT2 is 2.", 2, pickLinesPLT2.Length);
			var pickLine1After = pickLinesPLT2.Single(p => p.WZ_Units.Equals(4m) && p.InventoryLine.PK.Equals(receiveLine3.PK));
			var pickLine2After = pickLinesPLT2.Single(p => p.WZ_Units.Equals(3m) && p.InventoryLine.PK.Equals(receiveLine3.PK));
			AssertEquals("pickLine1After is connected to correct orderline.", true, pickLine1After.WZ_WE_TransactionLine == orderLine1.PK);
			AssertEquals("pickLine2After is connected to correct orderline.", true, pickLine2After.WZ_WE_TransactionLine == orderLine2.PK);

			var package1DivotsAfter = package1.PackedItemDivots;
			AssertEquals("Correct Pkg1 Divot count exists", 1, package1DivotsAfter.Count);
			AssertNotNull("Pkg1 Divot connected to correct ParentID and package", package1DivotsAfter.Single(d => d.KI_ParentID == pickLine1After.PK && d.KI_PackedQty == 4m));

			var package2DivotsAfter = package2.PackedItemDivots;
			AssertEquals("Correct Pkg2 Divot count exists", 1, package2DivotsAfter.Count);
			AssertNotNull("Pkg2 Divot connected to correct ParentID and package", package2DivotsAfter.Single(d => d.KI_ParentID == pickLine2After.PK && d.KI_PackedQty == 3m));
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_TwoPackagesPickByLabel

		public void TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_TwoPackagesPickByLabel()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			var partRelation = data.Part1.RelatedOrganisations[0];
			partRelation.OU_PickMode = WhsPickMode.Codes.AttributeNeutral;

			var packingHelper = new PackingTestHelper(Helper.Factory);
			var refType1 = packingHelper.CreateRefPackType("1", "1", 2m, 4m, 8m, Constants.Length.Metres, 3, Constants.Weight.Kilograms, UOMPackTypesList.Codes.Case);

			var part1 = data.Part1;
			part1.OP_StockKeepingUnit = refType1.F3_Code;
			part1.OP_Cubic = 0m;
			part1.OP_Weight = 0m;
			Helper.Factory.Save();

			var palletIdNeutralLocType = Helper.CreateLocationType("PNE", "Pallet ID Neutral", true, 1, LocationClasses.Codes.NOR);
			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_WLT_LocationType = palletIdNeutralLocType.PK;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1A = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "PLT1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			receiveLine1A.WI_SerialNumber = "SN:001";
			var receiveLine1B = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "PLT1", ZDate.Empty, ZDate.Empty, "", "", "", "");
			receiveLine1B.WI_SerialNumber = "SN:002";
			var receiveLine2A = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "PLT2", ZDate.Empty, ZDate.Empty, "", "", "", "");
			receiveLine2A.WI_SerialNumber = "PLT2:001";
			var receiveLine2B = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "PLT2", ZDate.Empty, ZDate.Empty, "", "", "", "");
			receiveLine2B.WI_SerialNumber = "PLT2:002";
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m, WhsPickOption.Codes.Manual);
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(availInv => availInv.PalletID == "PLT1").Allocate = true;
			pick.WP_PickPalletsByLabel = true;
			pick.WP_PickCasesByLabel = true;
			Helper.Factory.Save();

			pick.AllocatePackageLabels();
			Helper.Factory.Save();

			var picklinesPLT1 = pick.GetAllPickLines();
			AssertEquals("Precondition: Correct picklines count", 2, picklinesPLT1.Count());
			var pickLineA = picklinesPLT1.Single(pl => pl.WZ_WE_InventoryLine == receiveLine1A.PK);
			var pickLineB = picklinesPLT1.Single(pl => pl.WZ_WE_InventoryLine == receiveLine1B.PK);
			AssertEquals("Precondition: PLT1 correct pickline A", "PLT1", pickLineA.Inventory.WI_PalletID);
			AssertEquals("Precondition: PLT1 correct pickline B", "PLT1", pickLineB.Inventory.WI_PalletID);

			var package1 = order.PackageJob.Packages.Single(p => p.PackedItemDivots.Single().KI_ParentID == pickLineA.PK);
			var package2 = order.PackageJob.Packages.Single(p => p.PackedItemDivots.Single().KI_ParentID == pickLineB.PK);

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLineA.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinesPLT2A = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			AssertEquals("All picklines returned should point to PLT2.", true, pickLinesPLT2A.All(p => p.Inventory.WI_PalletID.Equals("PLT2")));
			AssertEquals("One pickline returned", 1, pickLinesPLT2A.Length);
			AssertEquals("One pickline returned for 1 unit.", 1m, pickLinesPLT2A[0].WZ_Units);

			var packageDivotsPLT2 = package1.PackedItemDivots;
			AssertEquals("Pkg Divots for PLT2 exists", 1, packageDivotsPLT2.Count);
			AssertNotNull("Pkg DivotA connected to correct ParentID and package", packageDivotsPLT2.Single(d => d.KI_ParentID == pickLinesPLT2A[0].PK && d.KI_PackedQty == 1m));

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.ChangeAllocationToAnotherPalletID(new[] { pickLineB.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinesPLT2B = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response2.NewPickLine.PKs));
			AssertEquals("All picklines returned should point to PLT2.", true, pickLinesPLT2B.All(p => p.Inventory.WI_PalletID.Equals("PLT2")));
			AssertEquals("One pickline returned", 1, pickLinesPLT2B.Length);
			AssertEquals("One pickline returned for 1 unit.", 1m, pickLinesPLT2B[0].WZ_Units);
			AssertNotEquals("Pick line is for different inventory than the last pick line.", pickLinesPLT2B[0].WZ_WE_InventoryLine, pickLinesPLT2A[0].WZ_WE_InventoryLine);

			packageDivotsPLT2 = package2.PackedItemDivots;
			AssertEquals("Pkg Divots for PLT2 exists", 1, packageDivotsPLT2.Count);
			AssertNotNull("Pkg DivotA connected to correct ParentID and package", packageDivotsPLT2.Single(d => d.KI_ParentID == pickLinesPLT2B[0].PK && d.KI_PackedQty == 1m));
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_ReleaseCapturedAttribute

		public void TestChangeAllocationToAnotherPalletID_ChangePackageItemDivots_ReleaseCapturedAttribute()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			var packingHelper = new PackingTestHelper(Helper.Factory);
			Helper.Factory.Save();

			var palletIdNeutralLocType = Helper.CreateLocationType("PNE", "Pallet ID Neutral", true, 1, LocationClasses.Codes.NOR);
			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_WLT_LocationType = palletIdNeutralLocType.PK;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, locationA1, "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m);
			var orderline = order.Lines[0];
			var pick = Helper.CreatePickNew(order);

			var releaseLine = orderline.ReleaseLines[0];
			releaseLine.PartAttribute1 = "BLUE";
			Helper.Factory.Save();

			var pickline = pick.GetAllPickLines().Single();
			var pkgJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packingHelper.CreatePackage(pkgJob, "PKG1", 1, Constants.PkgUnit.Box);
			package.Pack(pickline, releaseLine);
			Helper.Factory.Save();

			// Run webservice
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickline.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinesPLT2 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			AssertEquals("All picklines returned should point to PLT2.", true, pickLinesPLT2.All(p => p.Inventory.WI_PalletID.Equals("PLT2")));
			AssertEquals("Pickline count for PLT2 is 1.", 1, pickLinesPLT2.Length);
			var pickLine1After = pickLinesPLT2.Single(p => p.WZ_Units.Equals(1m) && p.InventoryLine.PK.Equals(receiveLine2.PK));
			AssertEquals("pickLine1After is connected to correct orderline.", true, pickLine1After.WZ_WE_TransactionLine == orderline.PK);
			AssertEquals("Pickline for product has NO captured attributes.", false, pickLine1After.HasReleaseCapturedAttribs);

			var package1DivotsAfter = package.PackedItemDivots;
			AssertEquals("Correct Pkg1 Divot count exists", 1, package1DivotsAfter.Count);
			AssertNotNull("Pkg1 Divot connected to correct ParentID and package", package1DivotsAfter.Single(d => d.KI_ParentID == pickLine1After.PK && d.KI_PackedQty == 1m));
		}

		#endregion

		#endregion

		#region TestChangeAllocationToAnotherPalletID_SwapPickLines_DifferentLocation

		public void TestChangeAllocationToAnotherPalletID_SwapPickLines_LocationFormattedCheckDigit()
		{
			// Arrange
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var location = data.Whs1.FindLocation("A-1");
			location.FormattedCheckDigit = "11";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10);
			var pick = Helper.CreatePickNew(order1, order2);
			var pickLine1 = pick.GetAllPickLines().Single(pl => pl.WZ_WE_InventoryLine == receiveLine1.PK);
			var pickLine2 = pick.GetAllPickLines().Single(pl => pl.WZ_WE_InventoryLine == receiveLine2.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Precondition:", "PLT1", pickLine1.Inventory.WI_PalletID);
				AssertEquals("Precondition:", "PLT2", pickLine2.Inventory.WI_PalletID);
				AssertEquals("Precondition:", "11", pickLine1.Inventory.Location.FormattedCheckDigit);
				AssertEquals("Precondition:", "11", pickLine2.Inventory.Location.FormattedCheckDigit);
			});

			// Act
			location.FormattedCheckDigit = "22";
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(new[] { pickLine1.PK.ToGuid() }, "PLT2");
			AssertSuccessfulResponse(response, webService);

			var pickLinePLT = webService.Factory.Load<WhsPickLine>(new ZGuid(response.NewPickLine.PKs[0]));

			// Assert
			AssertEquals("Should be able to scan different pallet.", "PLT2", pickLinePLT.Inventory.WI_PalletID);
			AssertEquals("22", response.NewPickLine.LocationFormattedCheckDigit);
		}

		#endregion

		#region TestChangeAllocationToAnotherPalletID_UpdatesTasksAppropriately

		public void TestChangeAllocationToAnotherPalletID_UpdatesTasksAppropriately()
		{
			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			// 5 + 2 + 3 = 10 UNT on Pallet PLT-1
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3m, data.Whs1.DefaultLocation, "PLT-1");

			// 4 + 6 = 10 UNT on Pallet PLT-2
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, data.Whs1.DefaultLocation, "PLT-2");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, data.Whs1.DefaultLocation, "PLT-2");

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventories = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			var availInvForPLT1 = availableInventories.Single(ai => ai.PalletID == "PLT-1");
			var availInvForPLT2 = availableInventories.Single(ai => ai.PalletID == "PLT-2");

			// we want pick to be allocated to PLT-1
			if (availInvForPLT2.PickLineQuantity > 0)
			{
				availInvForPLT2.PickLineQuantity = 0;
				availInvForPLT1.PickLineQuantity = 10;
			}

			AssertEquals("Precondition: pick allocated to PLT-1", true, order.Lines[0].PickLines.All(pl => pl.InventoryLine.WE_PalletID == "PLT-1"));

			var pickLinePKs = availInvForPLT1.PickLines.Select(pl => pl.PK.ToGuid()).ToArray();

			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForPickJob(pick, staff);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.ChangeAllocationToAnotherPalletID(pickLinePKs, "PLT-2");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));

			CombineAssertions(() =>
			{
				AssertEquals(2, response.NewPickLine.PKs.Length);
				foreach (var newPK in response.NewPickLine.PKs)
				{
					AssertEquals("All pickline PKs must be new", false, pickLinePKs.Contains(newPK));
				}
			});

			var newPickLines = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, response.NewPickLine.PKs));
			CombineAssertions(() =>
			{
				AssertEquals("There should be a pickline for 6 units and PLT-2", true, newPickLines.Any(pl => pl.WZ_Units == 6m && pl.InventoryLine.WE_PalletID == "PLT-2"));
				AssertEquals("There should be a pickline for 4 units and PLT-2", true, newPickLines.Any(pl => pl.WZ_Units == 4m && pl.InventoryLine.WE_PalletID == "PLT-2"));
				AssertEquals("New picklines must point to existing order line", true, newPickLines.All(pl => pl.DocketLine.PK == order.Lines[0].PK));
				AssertEquals("No new order lines should be created", 1, order.Lines.Count);
			});

			var pickLinesForTask = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, task.PK));
			AssertContainsExactElementsInAnyOrder(newPickLines, pickLinesForTask);
		}

		public void TestChangeAllocationToAnotherPalletID_IgnoresTasksForOtherUsers()
		{
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			var staff2 = Helper.CreateGlbStaff("ST2", "ST2");
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			// 4 + 6 = 10 UNT on Pallet PLT-1
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, data.Whs1.DefaultLocation, "PLT-1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, data.Whs1.DefaultLocation, "PLT-1");

			// 10 UNT on Pallet PLT-2
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "PLT-2");

			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			var availableInventories = pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().ToArray();
			var availInvForPLT1 = availableInventories.Single(ai => ai.PalletID == "PLT-1");
			var availInvForPLT2 = availableInventories.Single(ai => ai.PalletID == "PLT-2");

			// we want pick to be allocated to PLT-1
			if (availInvForPLT2.PickLineQuantity > 0)
			{
				availInvForPLT2.PickLineQuantity = 0;
				availInvForPLT1.PickLineQuantity = 10;
			}

			AssertEquals("Precondition: pick allocated to PLT-1", true, order.Lines[0].PickLines.All(pl => pl.InventoryLine.WE_PalletID == "PLT-1"));

			var pickLines = availInvForPLT1.PickLines.ToArray();
			pickLines[0].WZ_GS_NKAssignedTo = staff1.GS_Code;
			pickLines[1].WZ_GS_NKAssignedTo = staff2.GS_Code;

			Helper.Factory.Save();

			var task1 = Helper.CreateProcessTaskForPickJob(pick, staff1);
			var task2 = Helper.CreateProcessTaskForPickJob(pick, staff2);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.ChangeAllocationToAnotherPalletID([pickLines[0].PK.ToGuid()], "PLT-2");
			AssertSuccessfulResponse(response, webService);
			AssertEquals("There should be no error message", true, string.IsNullOrEmpty(response.ErrorMessage));

			CombineAssertions(() =>
			{
				AssertEquals(1, response.NewPickLine.PKs.Length);
				AssertNotEquals("Pickline PK must be new", pickLines[0].PK.ToGuid(), response.NewPickLine.PKs[0]);
			});

			var newPickLine = webService.Factory.Load<WhsPickLine>(response.NewPickLine.PKs[0]);
			var pickLinesForTask1 = webService.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, task1.PK)).Single();
			AssertEquals(newPickLine, pickLinesForTask1);

			var pickLine2InNewFactory = webService.Factory.Load<WhsPickLine>(pickLines[1].PK);
			AssertEquals(staff2.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(staff2.GS_Code, pickLine2InNewFactory.WZ_GS_NKAssignedTo);
			AssertEquals(task2.PK, pickLine2InNewFactory.WZ_P9_Task);
		}

		#endregion
	}
}
