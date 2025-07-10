using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class LoadOrderUnpackedProductInfosForDirectedPackingTest : WhsSecureServiceTestCase
	{
		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			Assert("Order has no packages", !order.PackageJob.Packages.Any());

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var packageInfo = response.Package;
			AssertEquals(response.IsInvalidToteId, false);
			AssertNotNull(packageInfo);
			AssertPackageInfo(packageInfo, order, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly, isUsingDirectedPackingConsolidation: true);

			var productInfos = response.ProductInfos;
			AssertEquals("Should have 2 products related to this package.", 2, productInfos.Length);
			AssertNotNull(productInfos.SingleOrDefault(i =>
				i.ProductPK == data.Part1.PK
				&& i.ExpectedQty == 10m
				&& i.ProductDescription == data.Part1.OP_Desc
				&& i.ProductWeightUQ == data.Part1.OP_WeightUQ
				&& i.ProductWeight == data.Part1.OP_Weight));

			AssertNotNull(productInfos.SingleOrDefault(i =>
				i.ProductPK == data.Part2.PK
				&& i.ExpectedQty == 15m
				&& i.ProductDescription == data.Part2.OP_Desc
				&& i.ProductWeightUQ == data.Part2.OP_WeightUQ
				&& i.ProductWeight == data.Part2.OP_Weight));

			var orderInNewFactory = new BusinessObjectFactory().Load<WhsOrder>(order.PK);
			AssertNull("Tote is not created.", orderInNewFactory.PackageJob.Packages.FirstOrDefault(pkg => pkg.KP_PackageID == "ABC"));
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_PackingInfoIsCorrect_IsUsingCarrierLabelIntegration()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var carrierBookingAgent = Helper.CreateClient("CBA");
			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.CarrierBookingAgentDocAddress.OrganisationPK = carrierBookingAgent.PK;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			Assert("Order has no packages", !order.PackageJob.Packages.Any());

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var packageInfo = response.Package;
			AssertEquals(response.IsInvalidToteId, false);
			AssertNotNull(packageInfo);
			AssertPackageInfo(packageInfo, order, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly, isUsingCarrierLabelIntegration: true);

			var productInfos = response.ProductInfos;
			AssertEquals("Should have 1 product related to this package.", 1, productInfos.Length);
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_PackingInfoIsCorrect_AllowedToOverrideDockDoorLocation_ParamOn()
		{
			TestLoadOrderUnpackedProductInfosForDirectedPacking_PackingInfoIsCorrect_AllowedToOverrideDockDoorLocationCore(allowDDLOverride: true);
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_PackingInfoIsCorrect_AllowedToOverrideDockDoorLocation_ParamOff()
		{
			TestLoadOrderUnpackedProductInfosForDirectedPacking_PackingInfoIsCorrect_AllowedToOverrideDockDoorLocationCore(allowDDLOverride: false);
		}

		void TestLoadOrderUnpackedProductInfosForDirectedPacking_PackingInfoIsCorrect_AllowedToOverrideDockDoorLocationCore(bool allowDDLOverride)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var pickPackParam = Helper.CreatePickPackParameter(data.Org1, data.Whs1);
			pickPackParam.WPP_AllowPickDockDoorLocationOverride = allowDDLOverride;
			pickPackParam.WPP_IsPickAndPackEnabled = false;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			Assert("Order has no packages", !order.PackageJob.Packages.Any());

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var packageInfo = response.Package;
			AssertEquals(response.IsInvalidToteId, false);
			AssertNotNull(packageInfo);
			AssertPackageInfo(
				packageInfo,
				order,
				data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString,
				data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly,
				isAllowedToOverrideDockDoorLocation: allowDDLOverride);

			var productInfos = response.ProductInfos;
			AssertEquals("Should have 1 product related to this package.", 1, productInfos.Length);
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_PackingInfoIsCorrect_OrgMiscServRelatedInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var orgMiscServ = data.Org1.MiscServ;
			orgMiscServ.OM_WhsPackageWeightTolerancePercent = 10m;
			orgMiscServ.OM_WhsPackageToleranceEnabled = true;
			orgMiscServ.OM_WhsEnforceScanOfProductsWhenPackingTote = true;

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			Assert("Order has no packages", !order.PackageJob.Packages.Any());

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var packageInfo = response.Package;
			AssertEquals(response.IsInvalidToteId, false);
			AssertNotNull(packageInfo);
			AssertPackageInfo(packageInfo, order, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly, packageWeightTolerance: 10m, packageWeightToleranceEnabled: true);

			var productInfos = response.ProductInfos;
			AssertEquals("Should have 1 product related to this package.", 1, productInfos.Length);
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_PackingInfoIsCorrect_ClientPickPackParamsRelatedInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var clientParams = WhsClientPickingParams.GetClientPickingParams(data.Org1);
			var pickPackParams = clientParams.WarehousePickPackParams.AddNew();
			pickPackParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickPackParams.WPP_IsUsingCartonSizes = false;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.CreatePickNew(order);

			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			Assert("Order has no packages", !order.PackageJob.Packages.Any());

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var packageInfo = response.Package;
			AssertEquals(response.IsInvalidToteId, false);
			AssertNotNull(packageInfo);
			AssertPackageInfo(packageInfo, order, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly, isUsingCartonSizes: false);

			var productInfos = response.ProductInfos;
			AssertEquals("Should have 1 product related to this package.", 1, productInfos.Length);
		}

		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_NullOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.AssignToteForOrderDirectedPacking("ABC", Guid.Empty);
			AssertEquals("Should error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order does not exist.", response.ErrorMessage);
			AssertEquals(response.IsInvalidToteId, false);
		}

		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_NotAnOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");
			Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(receive.PK.ToGuid());
			AssertEquals("Should error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order does not exist.", response.ErrorMessage);
			AssertEquals(response.IsInvalidToteId, false);
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_PartiallyPacked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 6m);

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 10m);
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var packageInfo = response.Package;
			AssertNotNull(packageInfo);
			AssertPackageInfo(packageInfo, order, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);

			var productInfos = response.ProductInfos;
			AssertEquals("Should have 2 products related to this package.", 2, productInfos.Length);
			AssertNotNull(productInfos.SingleOrDefault(i => i.ProductPK == data.Part1.PK && i.ExpectedQty == 4m));
			AssertNotNull(productInfos.SingleOrDefault(i => i.ProductPK == data.Part2.PK && i.ExpectedQty == 5m));
		}

		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_NothingToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			transferLine2.FinaliseDocketLine();

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 15m);
			Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order 'O1' has nothing to pack.", response.ErrorMessage);
			AssertNull(response.Package);
			AssertNull(response.ProductInfos);
		}

		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_OrderNotReadyToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderline1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderline2 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Helper.CreatePickNew(order);

			var pickLine1 = orderline1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;
			transferLine1.FinaliseDocketLine();

			var pickLine2 = orderline2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;

			Factory.Save();

			AssertNotEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be error.", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should be error.", "Order 'O1' is not ready to pack.", response.ErrorMessage);
			AssertNull(response.Package);
			AssertNull(response.ProductInfos);
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_PackageWithSiblingPutawayToConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine.WE_WL = consolidationLocation.PK;
			newTransferLine.FinaliseDocketLine();
			newFactory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var packageInfo = response.Package;
			AssertEquals(response.IsInvalidToteId, false);
			AssertNotNull(packageInfo);
			AssertPackageInfo(packageInfo, order, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly, isUsingDirectedPackingConsolidation: true);

			AssertEquals("PS1", packageInfo.AssignedPutawayLocationString);
			AssertEquals("PS1", packageInfo.AssignedPutawayLocationStringUserFriendly);
			AssertEquals("CON", packageInfo.AssignedPutawayLocationClass);
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_PackageWithSiblingPutawayToDockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;

			var consolidationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.CON);
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS1", 1, 1).Locations[0];
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_UseDirectedPackingConsolidation = true;
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine1.PickLines.Single();
			var transferLine1 = Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			transferLine1.WE_WL = packingLocation.PK;

			var pickLine2 = orderLine2.PickLines.Single();
			var transferLine2 = Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);
			transferLine2.WE_WL = packingLocation.PK;
			Factory.Save();

			var transfer = pick.Transfers[0];
			AssertNotNull("Precondition: Transfer Created.", transfer);
			var webService1 = GetNewWebService(data.Whs1);
			var putawayToDockDoorResponse = webService1.PutawayStockInDockDoorOrPackingStation(pick.PK.ToGuid(), PickJobType.Pick, packingLocation.WLV_LocationString_UserFriendly);
			AssertEquals("Should be no error.", ErrorTypes.None, putawayToDockDoorResponse.Error);
			AssertNull("Should be no error.", putawayToDockDoorResponse.ErrorMessage);

			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.ReadyToPack, transferLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package.Pack(orderLine1.ReleaseLines[0], 10m);
			Helper.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orderLine1InNewFactory = newFactory.Load<WhsOrderLine>(orderLine1.PK);
			var pickLine1InNewFactory = orderLine1InNewFactory.PickLines.Single();
			var newTransferLine = Helper.PickAndMakeInTransitTransfer(pickLine1InNewFactory, ZDateTimeOffset.Now, allowMultipleSteps: true);
			newTransferLine.FinaliseDocketLine();
			newFactory.Save();

			var webService = GetNewWebService(data.Whs1, staff: user);
			var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
			AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
			AssertNull("Should be no error.", response.ErrorMessage);

			var packageInfo = response.Package;
			AssertEquals(response.IsInvalidToteId, false);
			AssertNotNull(packageInfo);
			AssertPackageInfo(packageInfo, order, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly, isUsingDirectedPackingConsolidation: true);

			AssertEquals("DOCKDOOR", packageInfo.AssignedPutawayLocationString);
			AssertEquals("DOCKDOOR", packageInfo.AssignedPutawayLocationStringUserFriendly);
			AssertEquals("DDL", packageInfo.AssignedPutawayLocationClass);
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct($"P1{i}", data.Org1);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", product, 10m);
				products.Add(product);
			}
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLines = new List<WhsOrderLine>();
			for (var i = 0; i < 10; i++)
			{
				var product = products[i];
				var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
				orderLines.Add(orderLine);
			}
			Helper.CreatePickNew(order);

			foreach (var orderLine in orderLines)
			{
				var pickLine = orderLine.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				transferLine.FinaliseDocketLine();
			}
			Factory.Save();

			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);
			Assert("Order has no packages", !order.PackageJob.Packages.Any());

			var expectedDBHits = new Dictionary<string, int>
			{
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsOrderStatusViewSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);

				var packageInfo = response.Package;
				AssertEquals(response.IsInvalidToteId, false);
				AssertNotNull(packageInfo);
				AssertPackageInfo(packageInfo, order, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);

				AssertEquals("Should have 10 products related to this package.", 10, response.ProductInfos.Length);
			}
		}

		[TestDate(2024, 09, 02, 8, 25, 0)]
		public void TestLoadOrderUnpackedProductInfosForDirectedPacking_DBHits_PartiallyPacked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var user = Helper.CreateGlbStaff("AAA", "AAAAA");

			PackingRegistry.Instance.SetWeightUnitForTest(Weight.Kilograms);
			PackingRegistry.Instance.SetDimensionUnitForTest(Length.Metres);

			var packingStationLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.PST);
			var newRow = Helper.CreateRowAndGenerateLocations(data.Whs1, "P");
			var packingLocation = newRow.Locations[0];
			packingLocation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var products = new List<OrgSupplierPart>();
			for (var i = 0; i < 10; i++)
			{
				var product = Helper.CreateProduct($"P1{i}", data.Org1);
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, $"R{i}", product, 10m);
				products.Add(product);
			}
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLines = new List<WhsOrderLine>();
			for (var i = 0; i < 10; i++)
			{
				var product = products[i];
				var orderLine = Helper.CreateWhsOrderLine(order, product, 10m);
				orderLines.Add(orderLine);
			}
			Helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			foreach (var orderLine in orderLines)
			{
				var pickLine = orderLine.PickLines.Single();
				var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				transferLine.WE_WL = packingLocation.PK;
				transferLine.FinaliseDocketLine();

				var package = PackingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
				package.Pack(orderLine.ReleaseLines[0], 6m);
			}
			Factory.Save();

			AssertEquals("Precondition", WhsOrderStatus.Codes.ReadyToPack, order.WarehouseOrderStatus);

			var expectedDBHits = new Dictionary<string, int>
			{
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsOrderStatusViewSchema.Constants.TableName, 1 },
			};

			var webService = GetNewWebService(data.Whs1, staff: user);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService.Factory))
			{
				var response = webService.LoadOrderUnpackedProductInfosForPackToCartonDirectedPacking(order.PK.ToGuid());
				AssertEquals("Should be no error.", ErrorTypes.None, response.Error);
				AssertNull("Should be no error.", response.ErrorMessage);

				var packageInfo = response.Package;
				AssertEquals(response.IsInvalidToteId, false);
				AssertNotNull(packageInfo);
				AssertPackageInfo(packageInfo, order, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString, data.Whs1.DefaultOutboundDockDoorLocation.WLV_LocationString_UserFriendly);

				AssertEquals("Should have 10 products related to this package.", 10, response.ProductInfos.Length);
			}
		}

		void AssertPackageInfo(PackageForPackingInfo packageInfo,
			WhsOrder order,
			string expectedAssignedDockDoorLocation,
			string expectedAssignedDockDoorLocationUserFriendly,
			bool isUsingCarrierLabelIntegration = false,
			bool isUsingCartonSizes = true,
			decimal packageWeightTolerance = 0m,
			bool packageWeightToleranceEnabled = false,
			bool isUsingDirectedPackingConsolidation = false,
			bool isAllowedToOverrideDockDoorLocation = false)
		{
			AssertEquals("IsTote", false, packageInfo.IsTote);
			AssertEquals("ToteID", string.Empty, packageInfo.ToteID);
			AssertEquals("PackageID", string.Empty, packageInfo.PackageID);
			AssertEquals("IsDirectedPacking", true, packageInfo.IsDirectedPacking);
			AssertEquals("DocketID", order.WD_DocketID, packageInfo.DocketID);
			AssertEquals("OrderReference", order.WD_ExternalReference, packageInfo.OrderReference);
			AssertEquals("PackType", PkgUnit.Carton, packageInfo.PackType);
			AssertEquals("DocketStatus", order.WD_DocketStatus, packageInfo.DocketStatus);
			AssertEquals("RequiredDate", order.WD_RequiredDate.ToDateTime(), packageInfo.RequiredDate);
			AssertEquals("ClientCode", order.Client.OH_Code, packageInfo.ClientCode);
			AssertEquals("IsUsingCarrierLabelIntegration", isUsingCarrierLabelIntegration, packageInfo.IsUsingCarrierLabelIntegration);
			AssertEquals("PackageWeightTolerance", packageWeightTolerance, packageInfo.PackageWeightTolerance);
			AssertEquals("PackageWeightToleranceEnabled", packageWeightToleranceEnabled, packageInfo.PackageWeightToleranceEnabled);
			AssertEquals("ClientEnforceProductScan", true, packageInfo.ClientEnforceProductScan);
			AssertEquals("IsUsingCartonSizes", isUsingCartonSizes, packageInfo.IsUsingCartonSizes);
			AssertEquals("EmptyWeight", 0m, packageInfo.EmptyWeight);
			AssertEquals("Weight", 0m, packageInfo.Weight);
			AssertEquals("WeightUQ", Weight.Kilograms, packageInfo.WeightUQ);
			AssertEquals("Length", 0m, packageInfo.Length);
			AssertEquals("Width", 0m, packageInfo.Width);
			AssertEquals("Height", 0m, packageInfo.Height);
			AssertEquals("DimensionUQ", Length.Metres, packageInfo.DimensionUQ);
			AssertEquals("IsUsingDirectedPackingConsolidation", isUsingDirectedPackingConsolidation, packageInfo.OrderIsUsingDirectedPackingConsolidation);
			AssertEquals("PackageRequiresPutaway", true, packageInfo.PackageRequiresPutaway);
			AssertEquals("AssignedDockDoorLocationString", expectedAssignedDockDoorLocation, packageInfo.AssignedDockDoorLocationString);
			AssertEquals("AssignedDockDoorLocationStringUserFriendly", expectedAssignedDockDoorLocationUserFriendly, packageInfo.AssignedDockDoorLocationStringUserFriendly);
			AssertEquals("AllowedToOverrideDockDoorLocation", isAllowedToOverrideDockDoorLocation, packageInfo.AllowedToOverrideDockDoorLocation);
		}

		#region Implementation

		BusinessObjectFactory Factory => Helper.Factory;

		PackingTestHelper PackingHelper => packingHelper ?? (packingHelper = new PackingTestHelper(Factory));
		PackingTestHelper packingHelper;

		#endregion
	}
}
