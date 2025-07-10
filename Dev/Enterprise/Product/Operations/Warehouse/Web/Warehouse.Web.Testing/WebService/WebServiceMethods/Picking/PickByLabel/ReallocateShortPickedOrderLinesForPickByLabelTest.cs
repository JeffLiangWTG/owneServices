using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ReallocateShortPickedOrderLinesForPickByLabelTest : WhsPickingSecureServiceTestCase
	{
		#region TestReallocateShortPickedOrderLinesForPickByLabel_SingleLine

		public void TestReallocateShortPickedOrderLinesForPickByLabel_SingleLine()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc2);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();

			var packageID1 = "PACKAGE-1";
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pickLine.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);
			AssertEquals("Precondition: No PickLines packed into package1.", 0, package1.PackedItemDivots.Count);
			var packType = package1.KP_F3_NKPackType;

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package1.PK.ToGuid(), new[] { orderLine.PK.ToGuid() }, 5m);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Job.Lines.Count);
				AssertEquals("The reallocated PickLine has 5 units to pick.", 5m, response.Job.Lines[0].Units);
				AssertEquals("PackageID should be different", true, response.Job.Reference != packageID1);
				var newPackage = order.PackageJob.Packages.Single(p => p.KP_PackageID == response.Job.Reference);
				AssertEquals("1 PickLine packed into new package.", 1, newPackage.PackedItemDivots.Count);
				AssertEquals("Not using Carrier Label Integration.", false, response.Job.IsUsingCarrierLabelIntegration);
				AssertEquals("HasStartedPicking.", true, response.Job.HasStartedPicking);
				AssertEquals("Packing Type of new Package should be the same as the old one.", packType, newPackage.KP_F3_NKPackType);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_MultipleLines

		public void TestReallocateShortPickedOrderLinesForPickByLabel_MultipleLines()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 4, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var loc3 = data.Whs1.FindLocation("A-3");
			var loc4 = data.Whs1.FindLocation("A-4");

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc2);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc3);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc4);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;

			var packageID1 = "PACKAGE-1";
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var packageID2 = "PACKAGE-2";
			var package2 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID2);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 2, pickLines.Length);
			AssertEquals("Precondition.", true, pickLines.All(l => l.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLines[0].PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order was shorted.", 5m, order.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);
			AssertEquals("Precondition: No PickLines packed into package1.", 0, package1.PackedItemDivots.Count);
			var packType = package1.KP_F3_NKPackType;

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package1.PK.ToGuid(), new[] { orderLine.PK.ToGuid() }, 5m);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Job.Lines.Count);
				AssertEquals("The reallocated PickLine has 5 units to pick.", 5m, response.Job.Lines[0].Units);
				AssertEquals("PackageID should be different", true, response.Job.Reference != packageID1);
				var newPackage = order.PackageJob.Packages.Single(p => p.KP_PackageID == response.Job.Reference);
				AssertEquals("1 PickLine packed into new package.", 1, newPackage.PackedItemDivots.Count);
				AssertEquals("HasStartedPicking.", true, response.Job.HasStartedPicking);
				AssertEquals("Packing Type of new Package should be the same as the old one.", packType, newPackage.KP_F3_NKPackType);

				var label1 = webService2.GetPickByLabelPackage(packageID1, shouldOverrideOtherUsers: false);
				AssertNull("PACKAGE-1 was deleted", label1.Job);

				var label2 = webService2.GetPickByLabelPackage(packageID2, shouldOverrideOtherUsers: false);
				AssertNotNull(label2.Job);
				AssertEquals("PACKAGE-2 still has 1 line to pick", 1, label2.Job.Lines.Count);
				AssertEquals("PACKAGE-2 still has 5m to pick.", 5m, label2.Job.Lines[0].Units);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_MultipleOrders

		public void TestReallocateShortPickedOrderLinesForPickByLabel_MultipleOrders()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickPalletsByLabel = true;

			var packageID1 = "PACKAGE-1";
			var package1 = order1.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var packageID2 = "PACKAGE-2";
			var package2 = order2.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID2);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			packingHelper.CreatePackageDivot(package2, pickLines[1]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 2, pickLines.Length);
			AssertEquals("Precondition.", true, pickLines.All(l => l.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse1 = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLines[0].PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order1 was shorted.", 0m, order1.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse1.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine1.PK, shortPickResponse1.ShortedOrderLinePKs[0]);
			AssertEquals("Precondition: No PickLines packed into package1.", 0, package1.PackedItemDivots.Count);

			var shortPickResponse2 = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLines[1].PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order2 was shorted.", 0m, order2.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse2.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine2 was returned.", orderLine2.PK, shortPickResponse2.ShortedOrderLinePKs[0]);
			AssertEquals("Precondition: No PickLines packed into package2.", 0, package2.PackedItemDivots.Count);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			receive2.AllocateLocationsWithMock();
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Helper.Factory.Save();

			AssertEquals("Precondition: Pick Lines for orderLine1 has been deleted", 0, orderLine1.PickLines.Count);
			AssertEquals("Precondition: Pick Lines for orderLine2 has been deleted", 0, orderLine2.PickLines.Count);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package2.PK.ToGuid(), new[] { orderLine2.PK.ToGuid() }, 5m);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Job.Lines.Count);
				AssertEquals("The reallocated PickLine has 5 units to pick.", 5m, response.Job.Lines[0].Units);
				AssertEquals("PackageID should be different", true, response.Job.Reference != packageID2);
				var newPackage = order2.PackageJob.Packages.Single(p => p.KP_PackageID == response.Job.Reference);
				AssertEquals("1 PickLine packed into new package.", 1, newPackage.PackedItemDivots.Count);
				AssertEquals("The quantity of this PickLine is 5m.", 5m, newPackage.PackedItemDivots.Single().KI_PackedQty);

				var pickLinesForOrderLine1 = orderLine1.PickLines;
				AssertEquals("Pick Lines for orderLine1 has been reallocated", 1, pickLinesForOrderLine1.Count);
				AssertEquals("Pick Lines for orderLine1 has been reallocated", 5m, pickLinesForOrderLine1.Sum(l => l.WZ_Units));
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_IsUsingCarrierLabelIntegration

		public void TestReallocateShortPickedOrderLinesForPickByLabel_IsUsingCarrierLabelIntegration()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc2);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			order.CarrierBookingAgentDocAddress.OrganisationPK = Helper.CreateClient("RTUS").PK;

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();

			var packageID1 = "PACKAGE-1";
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pickLine.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package1.PK.ToGuid(), new[] { orderLine.PK.ToGuid() }, 5m);

				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Job.Lines.Count);
				AssertEquals("The reallocated PickLine has 5 units to pick.", 5m, response.Job.Lines[0].Units);
				AssertEquals("PackageID should be different", true, response.Job.Reference != packageID1);
				AssertEquals("Order has Carrier Booking Agent, should be using Carrier Label Integration.", true, response.Job.IsUsingCarrierLabelIntegration);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_NoStockReallocated

		public void TestReallocateShortPickedOrderLinesForPickByLabel_NoStockReallocated()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();

			var packageID1 = "PACKAGE-1";
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pickLine.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package1.PK.ToGuid(), new[] { orderLine.PK.ToGuid() }, 5m);

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "No stock could be reallocated for the shorted products.", response.ErrorMessage);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_NoEnoughStockToReallocate

		public void TestReallocateShortPickedOrderLinesForPickByLabel_NoEnoughStockToReallocate()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2m, loc2);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();

			var packageID1 = "PACKAGE-1";
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pickLine.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package1.PK.ToGuid(), new[] { orderLine.PK.ToGuid() }, 5m);

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "No enough stock to reallocate.", response.ErrorMessage);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_ReallocatedDifferentUOM

		public void TestReallocateShortPickedOrderLinesForPickByLabel_ReallocatedDifferentUOM()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Pallet, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Pallet, 5m);

			var staff = Helper.CreateGlbStaff("A", "A");
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc2);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			pick.WP_PickCasesByLabel = true;
			Helper.Factory.Save();

			pick.AllocatePackageLabels();
			var package = order.PackageJob.Packages[0];

			var pickLine = pick.GetAllPickLines().Single();

			data.Part1.PartUnits.RemoveAndDeleteAll();
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Case, UOMPackTypesList.Codes.Case);
			data.Part1.OP_F3_NKPackType = Constants.PkgUnit.Box;
			Helper.CreateProductUnit(data.Part1, Constants.PkgUnit.Box, Constants.PkgUnit.Case, 5m);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pickLine.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package.PK.ToGuid(), new[] { orderLine.PK.ToGuid() }, 5m);

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "Inventory with the same UOM Type not available. No Stock could be reallocated for the shorted Products.", response.ErrorMessage);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_PartiallyShorted

		public void TestReallocateShortPickedOrderLinesForPickByLabel_PartiallyShorted()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();

			var packageID1 = "PACKAGE-1";
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pickLine.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet);

			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Case);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(2, false),
				null,
				false);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order was shorted.", 2m, order.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package1.PK.ToGuid(), new[] { orderLine.PK.ToGuid() }, 5m);

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "Cannot partially reallocate Pick By Label.", response.ErrorMessage);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_AllocationError

		public void TestReallocateShortPickedOrderLinesForPickByLabel_AllocationError()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();

			var packageID1 = "PACKAGE-1";
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pickLine.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);

			var allocationEngineMock = new Mock<IAllocationEngineManager>();
			allocationEngineMock.
				Setup(ae =>
					ae.Allocate(It.IsAny<WhsPick>(), It.IsAny<INotifications>(), It.IsNotNull<IPickStrategy>(), It.IsAny<IEnumerable<WhsPickOrderedInventory>>()))
				.Returns<WhsPick, INotifications, IPickStrategy, IEnumerable<WhsPickOrderedInventory>>((p, n, ps, ordInv) => AllocationResult.ErrorOrWarning)
				.Callback((WhsPick p, INotifications n, IPickStrategy ps, IEnumerable<WhsPickOrderedInventory> poi) =>
				{
					n.AddError("Some allocation error.");
				});

			using (ObjectFactory.Substitute(allocationEngineMock.Object))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package1.PK.ToGuid(), new[] { orderLine.PK.ToGuid() }, 5m);

				AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
				AssertEquals("Error returned in response.", "Some allocation error.", response.ErrorMessage.TrimEnd());
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_PackageNotFound

		public void TestReallocateShortPickedOrderLinesForPickByLabel_PackageNotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response = webService1.ReallocateShortPickedOrderLinesForPickByLabel(new Guid(), new[] { new Guid() }, 5m);

			AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Package was not found.", response.ErrorMessage);
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_ShortedOrderedInventoriesNotFound

		public void TestReallocateShortPickedOrderLinesForPickByLabel_ShortedOrderedInventoriesNotFound()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();

			var packageID1 = "PACKAGE-1";
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pickLine.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response = webService1.ReallocateShortPickedOrderLinesForPickByLabel(package1.PK.ToGuid(), new[] { new Guid() }, 5m);

			AssertEquals("Error returned in response.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Shorted Ordered Inventories not found.", response.ErrorMessage);
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_DBHits

		public void TestReallocateShortPickedOrderLinesForPickByLabel_DBHits()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var staff = Helper.CreateGlbStaff("A", "A");

			for (var i = 0; i < 10; i++)
			{
				var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);
				Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc1);
				Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc2);
				receive1.AllocateLocationsWithMock();
				receive1.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive1);
			}
			Helper.Factory.Save();

			var orders = new WhsOrder[10];
			for (var i = 0; i < 10; i++)
			{
				var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O" + i);
				var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
				orders[i] = order;
			}

			var pick = Helper.CreatePickNew(orders);
			pick.WP_PickPalletsByLabel = true;
			var pickLines = pick.GetAllPickLines().ToArray();

			var packages = new PkgPackage[10];
			for (var i = 0; i < 10; i++)
			{
				var packageID = "PACKAGE-" + i;
				var package = orders[i].PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID);
				packages[i] = package;
				packingHelper.CreatePackageDivot(package, pickLines[i]);
			}

			Helper.Factory.Save();

			AssertEquals("Precondition.", 10, pickLines.Length);
			AssertEquals("Precondition.", true, pickLines.All(l => l.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet));

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLines[0].PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GenAddOnColumnSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ JobDocAddressSchema.Constants.TableName, 1 },
				{ JobDocumentDeliverySchema.Constants.TableName, 2 },
				{ JobDocumentExclusionSchema.Constants.TableName, 2 },
				{ OrgAddressSchema.Constants.TableName, 2 },
				{ OrgCusCodeSchema.Constants.TableName, 2 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgPartRelationSchema.Constants.TableName, 1 },
				{ OrgPartUnitSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgSupplierPartBarcodeSchema.Constants.TableName, 1 },
				{ PkgPackageBookedDetailSchema.Constants.TableName,1 },
				{ PkgPackageSchema.Constants.TableName, 3 },
				{ PkgPackageHeaderSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ PkgPackageItemDivotSchema.Constants.TableName, 1 },
				{ PkgPackageJobPackageHeaderPivotSchema.Constants.TableName, 2 },
				{ PkgPackageScreeningSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 3 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ ProductionRuleSchema.Constants.TableName, 1 },
				{ StmDefaultPrinterSchema.Constants.TableName, 1 },
				{ StmDocDataOverrideSchema.Constants.TableName, 4 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 1 },
				{ StmUniversalCopySchema.Constants.TableName, 2 },
				{ UNDGDataItemSchema.Constants.TableName, 1 },
				{ WhsAreaSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 4 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 3 },
				{ WhsLocationViewSchema.Constants.TableName, 3 },
				{ WhsLoadPkgPackagePivotSchema.Constants.TableName, 1 },
				{ WhsPickByLabelJobSchema.Constants.TableName, 2 },
				{ WhsPickByLabelLabelSchema.Constants.TableName, 2 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsPickTrolleySlotSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ JobServiceLinkSchema.Constants.TableName, 1 },
				{ WhsClientPickPackParamsByWhsSchema.Constants.TableName, 1 },
			};

			var webService2 = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDbHits, webService2.Factory))
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(packages[0].PK.ToGuid(), new[] { orders[0].Lines[0].PK.ToGuid() }, 5m);

				AssertEquals(ErrorTypes.None, response.Error);
				AssertEquals("1 Pick Line Group returned after reallocation.", 1, response.Job.Lines.Count);
				AssertEquals("The reallocated PickLine has 5 units to pick.", 5m, response.Job.Lines[0].Units);
			}
		}

		#endregion

		#region TestReallocateShortPickedOrderLinesForPickByLabel_FactoryConcurrencySaveError

		public void TestReallocateShortPickedOrderLinesForPickByLabel_FactoryConcurrencySaveError()
		{
			var packingHelper = new PackingTestHelper(Helper.Factory);
			packingHelper.SetRefPackTypeUOM(Constants.PkgUnit.Unit, UOMPackTypesList.Codes.Pallet);

			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Part1.PartUnits.RemoveAndDeleteAll();

			var staff = Helper.CreateGlbStaff("A", "A");

			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, loc2);
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			pick.WP_PickPalletsByLabel = true;
			var pickLine = pick.GetAllPickLines().Single();

			var packageID1 = "PACKAGE-1";
			var package1 = order.PackageJob.Packages.AddNew(Constants.PkgUnit.Unit, packageID1);
			var pickLines = pick.GetAllPickLines().ToArray();
			packingHelper.CreatePackageDivot(package1, pickLines[0]);
			Helper.Factory.Save();

			AssertEquals("Precondition.", 1, pick.GetAllPickLines().Count());
			AssertEquals("Precondition.", true, pickLine.AllocatedPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var shortPickResponse = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false),
				null,
				true);

			AssertEquals("Precondition: Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Precondition: Order was shorted.", 0m, order.WD_UnitsSent);
			AssertEquals("Precondition: 1 shorted OrderLinePK returned.", 1, shortPickResponse.ShortedOrderLinePKs.Length);
			AssertEquals("Precondition: PK of orderLine1 was returned.", orderLine.PK, shortPickResponse.ShortedOrderLinePKs[0]);
			AssertEquals("Precondition: No PickLines packed into package1.", 0, package1.PackedItemDivots.Count);
			var packType = package1.KP_F3_NKPackType;

			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var webService2 = GetNewWebService(data.Whs1, staff);
				var innerException = new Exception();
				var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pickLines[0]).Row, TestConnection);
				webService2.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

				var response = webService2.ReallocateShortPickedOrderLinesForPickByLabel(package1.PK.ToGuid(), new[] { orderLine.PK.ToGuid() }, 5m);
				AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has been assigned to or modified this Job. Please restart the operation and try again.", response.ErrorMessage);
			}
		}

		#endregion
	}
}
