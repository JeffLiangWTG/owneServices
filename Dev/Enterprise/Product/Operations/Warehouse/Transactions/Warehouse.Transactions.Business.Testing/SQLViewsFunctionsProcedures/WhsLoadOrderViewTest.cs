using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLoadOrderViewTest : WhsTestCaseWithFactory
	{
		public void TestView_GeneralColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			order1.WD_WLO_PlannedLoad = load.PK;
			order1.WD_DocketStatus = "ENT";
			order1.WD_TotalWeight = 1m;
			order1.WD_TotalWeightUnit = "KG";
			order1.WD_TotalCubic = 0.5m;
			order1.WD_TotalCubicUnit = "M3";
			var pick1 = Helper.CreatePickNew(order1);

			var packageJob1 = order1.PackageJob;
			var package1 = packageJob1.Packages.AddNew("BOX", 1);
			var package2 = packageJob1.Packages.AddNew("BOX", 1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			order2.WD_DocketStatus = "ENT";
			order2.WD_TotalWeight = 2m;
			order2.WD_TotalWeightUnit = "G";
			order2.WD_TotalCubic = 1.5m;
			order2.WD_TotalCubicUnit = "WW";
			var pick2 = Helper.CreatePickNew(order2);
			var packageJob2 = order2.PackageJob;
			var package3 = packageJob2.Packages.AddNew("BOX", 1);
			var package4 = packageJob2.Packages.AddNew("BOX", 1);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load);

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(2, loadOrders.Length);

			var loadOrder1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load.PK).Single();
			AssertEquals(order1.WD_DocketID, (ZString)loadOrder1["WOV_DocketID"]);
			AssertEquals(order1.WD_ExternalReference, (ZString)loadOrder1["WOV_ExternalReference"]);
			AssertEquals(data.Org1.PK, (ZGuid)loadOrder1["WOV_OH_Client"]);
			AssertEquals(DocketStatus.Codes.AttachedToPick, (ZString)loadOrder1["WOV_OrderStatus"]);
			AssertEquals(order1.WD_TotalWeight, (ZDecimal)loadOrder1["WOV_TotalWeight"]);
			AssertEquals(order1.WD_TotalWeightUnit, (ZString)loadOrder1["WOV_TotalWeightUnit"]);
			AssertEquals(order1.WD_TotalCubic, (ZDecimal)loadOrder1["WOV_TotalCubic"]);
			AssertEquals(order1.WD_TotalCubicUnit, (ZString)loadOrder1["WOV_TotalCubicUnit"]);
			AssertEquals(load.PK, (ZGuid)loadOrder1["WOV_WLO_Load"]);
			AssertEquals("P00000001", (ZString)loadOrder1["WOV_PickNo"]);
			AssertEquals(false, loadOrder1["WOV_IsAttachedThroughPackage"]);
			AssertEquals(data.Org1.PK, (ZGuid)loadOrder1["WOV_OH_Consignee"]);
			AssertEquals(false, loadOrder1["WOV_EnforceScanningForLoad"]);
			AssertEquals(2, loadOrder1["WOV_PackageCount"]);

			var loadOrder2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load.PK).Single();
			AssertEquals(order2.WD_DocketID, (ZString)loadOrder2["WOV_DocketID"]);
			AssertEquals(order2.WD_ExternalReference, (ZString)loadOrder2["WOV_ExternalReference"]);
			AssertEquals(data.Org1.PK, (ZGuid)loadOrder2["WOV_OH_Client"]);
			AssertEquals(DocketStatus.Codes.AttachedToPick, (ZString)loadOrder2["WOV_OrderStatus"]);
			AssertEquals(order2.WD_TotalWeight, (ZDecimal)loadOrder2["WOV_TotalWeight"]);
			AssertEquals(order2.WD_TotalWeightUnit, (ZString)loadOrder2["WOV_TotalWeightUnit"]);
			AssertEquals(order2.WD_TotalCubic, (ZDecimal)loadOrder2["WOV_TotalCubic"]);
			AssertEquals(order2.WD_TotalCubicUnit, (ZString)loadOrder2["WOV_TotalCubicUnit"]);
			AssertEquals(load.PK, (ZGuid)loadOrder2["WOV_WLO_Load"]);
			AssertEquals("P00000002", (ZString)loadOrder2["WOV_PickNo"]);
			AssertEquals(true, loadOrder2["WOV_IsAttachedThroughPackage"]);
			AssertEquals(data.Org1.PK, (ZGuid)loadOrder2["WOV_OH_Consignee"]);
			AssertEquals(false, loadOrder2["WOV_EnforceScanningForLoad"]);
			AssertEquals(1, loadOrder2["WOV_PackageCount"]);
		}

		public void TestView_WPP_EnforceScanningForLoad()
		{
			AssertClientPickPackParamsByWhsFallbacks("WOV_EnforceScanningForLoad", (w, v) => w.WPP_EnforceScanningForLoad = v);
		}

		public void Test_WPP_EnforceTransportReferenceForLoad()
		{
			AssertClientPickPackParamsByWhsFallbacks("WOV_EnforceTransportReferenceForLoad", (w, v) => w.WPP_EnforceTransportReferenceForLoad = v);
		}

		void AssertClientPickPackParamsByWhsFallbacks(string attributeName, Action<WhsClientPickPackParamsByWhs, bool> setAttribute)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var client2 = Helper.CreateClient("CLIENT2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			order1.WD_WLO_PlannedLoad = load.PK;
			order1.WD_DocketStatus = "ENT";
			order1.WD_TotalWeight = 1m;
			order1.WD_TotalWeightUnit = "KG";
			order1.WD_TotalCubic = 0.5m;
			order1.WD_TotalCubicUnit = "M3";
			var pick1 = Helper.CreatePickNew(order1);

			var packageJob1 = order1.PackageJob;
			var package1 = packageJob1.Packages.AddNew("BOX", 1);
			var package2 = packageJob1.Packages.AddNew("BOX", 1);

			var order2 = Helper.CreateWhsOrderWithOrderLine(client2, data.Whs1, "ORD02", data.Part1, 10m);
			order2.WD_DocketStatus = "ENT";
			order2.WD_TotalWeight = 2m;
			order2.WD_TotalWeightUnit = "G";
			order2.WD_TotalCubic = 1.5m;
			order2.WD_TotalCubicUnit = "WW";
			var pick2 = Helper.CreatePickNew(order2);
			var packageJob2 = order2.PackageJob;
			var package3 = packageJob2.Packages.AddNew("BOX", 1);
			var package4 = packageJob2.Packages.AddNew("BOX", 1);
			var pivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load);
			var param1 = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
			param1.WPP_WW_Warehouse = data.Whs1.PK;
			setAttribute(param1, true);

			Factory.Save();

			var loadOrders1 = GetAllLoads();
			AssertEquals(2, loadOrders1.Length);

			var loadOrder1 = loadOrders1.Single(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load.PK);
			var loadOrder2 = loadOrders1.Single(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load.PK);
			AssertEquals("Default value should be 'false' when no ClientPickPackParamsByWhs", false, loadOrder1[attributeName]);
			AssertEquals("Should find ClientPickPackParamsByWhs when no SalesChannel", true, loadOrder2[attributeName]);

			var salesChannel = Helper.CreateWhsSalesChannel("ECO", "eCommerce");
			order2.WD_WSH_SalesChannel = salesChannel.PK;

			var param2 = WhsClientPickingParams.GetClientPickingParams(client2).WarehousePickPackParams.AddNew();
			param2.WPP_WW_Warehouse = data.Whs1.PK;
			param2.WPP_WSH_SalesChannel = salesChannel.PK;

			order1.WD_WSH_SalesChannel = salesChannel.PK;
			var param3 = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			param3.WPP_WW_Warehouse = data.Whs1.PK;
			setAttribute(param3, true);

			Factory.Save();

			var loadOrders2 = GetAllLoads();
			AssertEquals(2, loadOrders2.Length);

			var reloadedOrder1 = loadOrders2.Single(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load.PK);
			var reloadedOrder2 = loadOrders2.Single(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load.PK);
			AssertEquals("Should find ClientPickPackParamsByWhs when SalesChannel", true, reloadedOrder1[attributeName]);
			AssertEquals("Should prioritize ClientPickPackParamsByWhs with SalesChannel", false, reloadedOrder2[attributeName]);
		}

		public void TestView_PackageCount()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS");
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL02", "CDS", startTime: DateTimeOffset.Now);
			var load3 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL03", "CDS", startTime: DateTimeOffset.Now);
			var load4 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL04", "CDS");
			var load5 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL05", "CDS");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			order1.WD_WLO_PlannedLoad = load1.PK;
			order1.WD_DocketStatus = "ENT";
			order1.WD_TotalWeight = 1m;
			order1.WD_TotalWeightUnit = "KG";
			order1.WD_TotalCubic = 0.5m;
			order1.WD_TotalCubicUnit = "M3";
			var pick1 = Helper.CreatePickNew(order1);
			var packageJob1 = order1.PackageJob;
			var package1 = packageJob1.Packages.AddNew("BOX", 1);
			var package2 = packageJob1.Packages.AddNew("BOX", 1);
			var package3 = packageJob1.Packages.AddNew("BOX", 1);
			var package4 = packageJob1.Packages.AddNew("BOX", 1);
			Helper.CreateLoadPkgPackagePivot(package2.PK, load2);
			Helper.CreateLoadPkgPackagePivot(package3.PK, load3);
			Helper.CreateLoadPkgPackagePivot(package4.PK, load3);

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			order2.WD_DocketStatus = "ENT";
			order2.WD_TotalWeight = 1m;
			order2.WD_TotalWeightUnit = "KG";
			order2.WD_TotalCubic = 0.5m;
			order2.WD_TotalCubicUnit = "M3";
			var pick2 = Helper.CreatePickNew(order2);
			var packageJob2 = order2.PackageJob;
			var package5 = packageJob2.Packages.AddNew("BOX", 1);
			var package6 = packageJob2.Packages.AddNew("BOX", 1);
			Helper.CreateLoadPkgPackagePivot(package5.PK, load3);
			Helper.CreateLoadPkgPackagePivot(package6.PK, load3);

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD03", data.Part1, 10m);
			order3.WD_WLO_PlannedLoad = load4.PK;
			order3.WD_DocketStatus = "ENT";
			order3.WD_TotalWeight = 2m;
			order3.WD_TotalWeightUnit = "G";
			order3.WD_TotalCubic = 1.5m;
			order3.WD_TotalCubicUnit = "WW";
			var pick3 = Helper.CreatePickNew(order3);
			var packageJob3 = order3.PackageJob;

			// Ignore inner packages from count
			var package7 = packageJob3.Packages.AddNew("BOX", 1);
			package7.Packages.AddNew("BOX", 1);
			var inner7 = package7.Packages.AddNew("BOX", 1);
			inner7.Packages.AddNew("BOX", 1);
			var package8 = packageJob3.Packages.AddNew("BOX", 1);
			package8.Packages.AddNew("BOX", 1);
			package8.Packages.AddNew("BOX", 1);
			var package9 = packageJob3.Packages.AddNew("BOX", 1);
			package9.Packages.AddNew("BOX", 1);

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(5, loadOrders.Length);

			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, load1Order1["WOV_IsAttachedThroughPackage"]);
			AssertEquals(1, load1Order1["WOV_PackageCount"]);

			var load2Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load2.PK).Single();
			AssertEquals(true, load2Order1["WOV_IsAttachedThroughPackage"]);
			AssertEquals(1, load2Order1["WOV_PackageCount"]);

			var load3Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load3.PK).Single();
			AssertEquals(true, load3Order1["WOV_IsAttachedThroughPackage"]);
			AssertEquals(2, load3Order1["WOV_PackageCount"]);

			var load3Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load3.PK).Single();
			AssertEquals(true, load3Order2["WOV_IsAttachedThroughPackage"]);
			AssertEquals(2, load3Order2["WOV_PackageCount"]);

			var load4Order3 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order3.PK && (ZGuid)o["WOV_WLO_Load"] == load4.PK).Single();
			AssertEquals(false, load4Order3["WOV_IsAttachedThroughPackage"]);
			AssertEquals(3, load4Order3["WOV_PackageCount"]);
		}

		public void TestView_NoPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			order1.WD_WLO_PlannedLoad = load1.PK;
			order1.WD_DocketStatus = "ENT";
			order1.WD_TotalWeight = 1m;
			order1.WD_TotalWeightUnit = "KG";
			order1.WD_TotalCubic = 0.5m;
			order1.WD_TotalCubicUnit = "M3";

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(1, loadOrders.Length);

			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, load1Order1["WOV_IsAttachedThroughPackage"]);
			AssertEquals(0, load1Order1["WOV_PackageCount"]);
		}

		public void TestView_OrderStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			order1.WD_WLO_PlannedLoad = load1.PK;
			order1.WD_DocketStatus = "ENT";
			order1.WD_TotalWeight = 1m;
			order1.WD_TotalWeightUnit = "KG";
			order1.WD_TotalCubic = 0.5m;
			order1.WD_TotalCubicUnit = "M3";

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(1, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("ENT", (ZString)load1Order1["WOV_OrderStatus"]);

			var pick = Helper.CreatePickNew(order1);
			Factory.Save();
			loadOrders = GetAllLoads();
			AssertEquals(1, loadOrders.Length);
			load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(DocketStatus.Codes.AttachedToPick, (ZString)load1Order1["WOV_OrderStatus"]);

			order1.Lines.Single().PickLines.Single().WZ_PickedDateTime = ZDateTimeOffset.Now;
			pick.RunPreSaveValidation();
			Factory.Save();

			pick.Transfers[0].FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			loadOrders = GetAllLoads();
			AssertEquals(1, loadOrders.Length);
			load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(WhsOrderStatus.Codes.Staged, (ZString)load1Order1["WOV_OrderStatus"]);

			var package = order1.PackageJob.Packages.AddNew("CTN");
			package.Pack(order1.Lines[0].ReleaseLines[0], 10m);
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package.PK, load1);
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";
			Factory.Save();
			loadOrders = GetAllLoads();
			AssertEquals(1, loadOrders.Length);
			load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(WhsOrderStatus.Codes.Loaded, (ZString)load1Order1["WOV_OrderStatus"]);

			Helper.DepartPackageNow(loadPkgPackagePivot);
			Factory.Save();
			Factory.Save();
			loadOrders = GetAllLoads();
			AssertEquals(1, loadOrders.Length);
			load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(WhsOrderStatus.Codes.Departed, (ZString)load1Order1["WOV_OrderStatus"]);
		}

		public void TestView_QualityAuditRequired()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD03", data.Part1, 10m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD04", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2, order3, order4);

			order1.WD_QualityAuditRequired = true;
			order1.WD_WLO_PlannedLoad = load1.PK;

			order2.WD_QualityAuditRequired = false;
			order2.WD_WLO_PlannedLoad = load1.PK;

			var package1 = order3.PackageJob.Packages.AddNew("BOX");
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package1.PK, load1);
			order3.WD_QualityAuditRequired = true;

			var package2 = order4.PackageJob.Packages.AddNew("BOX");
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load1);
			order4.WD_QualityAuditRequired = false;

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(4, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(true, (ZBool)load1Order1["WOV_QualityAuditRequired"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order2["WOV_QualityAuditRequired"]);

			var load1Order3 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order3.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(true, (ZBool)load1Order3["WOV_QualityAuditRequired"]);

			var load1Order4 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order4.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order4["WOV_QualityAuditRequired"]);
		}

		public void TestView_HasHeldPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD03", data.Part1, 10m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD04", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2, order3, order4);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var package1 = order1.PackageJob.Packages.AddNew("BOX");
			package1.KP_IsHeld = true;

			order2.WD_WLO_PlannedLoad = load1.PK;
			var package2 = order2.PackageJob.Packages.AddNew("BOX");
			package2.KP_IsHeld = false;

			var package3 = order3.PackageJob.Packages.AddNew("BOX");
			package3.KP_IsHeld = true;
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);

			var package4 = order4.PackageJob.Packages.AddNew("BOX");
			package4.KP_IsHeld = false;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package4.PK, load1);

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(4, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(true, (ZBool)load1Order1["WOV_HasHeldPackages"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order2["WOV_HasHeldPackages"]);

			var load1Order3 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order3.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasHeldPackages comes from Pivot is hardcoded false", false, (ZBool)load1Order3["WOV_HasHeldPackages"]);

			var load1Order4 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order4.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasHeldPackages comes from Pivot is hardcoded false", false, (ZBool)load1Order4["WOV_HasHeldPackages"]);
		}

		public void TestView_HasHeldPackage_MultiplePackagesAndOneIsHeld()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var package1 = order1.PackageJob.Packages.AddNew("BOX");
			var package2 = order1.PackageJob.Packages.AddNew("BOX");
			package1.KP_IsHeld = true;

			var package3 = order2.PackageJob.Packages.AddNew("BOX");
			var package4 = order2.PackageJob.Packages.AddNew("BOX");
			package3.KP_IsHeld = true;
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package4.PK, load1);

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(2, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(true, (ZBool)load1Order1["WOV_HasHeldPackages"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasHeldPackages comes from Pivot is hardcoded false", false, (ZBool)load1Order2["WOV_HasHeldPackages"]);
		}

		public void TestView_HasUnauditedOuters_IsContainer_HasNoInner()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD03", data.Part1, 10m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD04", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2, order3, order4);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var package1 = order1.PackageJob.Packages.AddNew("CNT");
			package1.KP_PackageID = "P01";
			package1.Container.K0_RC_ContainerType = containerType20GP.PK;
			Helper.CreateWhsPackageAudit(order1, "P01");

			order2.WD_WLO_PlannedLoad = load1.PK;
			var package2 = order2.PackageJob.Packages.AddNew("CNT");
			package2.KP_PackageID = "P02";
			package2.Container.K0_RC_ContainerType = containerType20GP.PK;

			var package3 = order3.PackageJob.Packages.AddNew("CNT");
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			package3.KP_PackageID = "P03";
			package3.Container.K0_RC_ContainerType = containerType20GP.PK;
			Helper.CreateWhsPackageAudit(order3, "P03");

			var package4 = order4.PackageJob.Packages.AddNew("CNT");
			package4.Container.K0_RC_ContainerType = containerType20GP.PK;
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package4.PK, load1);
			package4.KP_PackageID = "P04";

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(4, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order1["WOV_HasUnauditedOuters"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order2["WOV_HasUnauditedOuters"]);

			var load1Order3 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order3.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order3["WOV_HasUnauditedOuters"]);

			var load1Order4 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order4.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order4["WOV_HasUnauditedOuters"]);
		}

		public void TestView_HasUnauditedOuters_IsContainer_HasInner()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD03", data.Part1, 10m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD04", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2, order3, order4);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var package1 = order1.PackageJob.Packages.AddNew("CNT");
			package1.KP_PackageID = "P01";
			package1.Container.K0_RC_ContainerType = containerType20GP.PK;
			var innerPackage1 = package1.Packages.AddNew("PLT");
			innerPackage1.KP_PackageID = "LEVEL2-1";
			var level3Package1 = innerPackage1.Packages.AddNew("BOX");
			level3Package1.KP_PackageID = "LEVEL301";
			Helper.CreateWhsPackageAudit(order1, "LEVEL2-1");

			order2.WD_WLO_PlannedLoad = load1.PK;
			var package2 = order2.PackageJob.Packages.AddNew("CNT");
			package2.KP_PackageID = "P02";
			package2.Container.K0_RC_ContainerType = containerType20GP.PK;
			var innerPackage2 = package2.Packages.AddNew("PLT");
			innerPackage2.KP_PackageID = "LEVEL2-2";
			var level3Package2 = innerPackage2.Packages.AddNew("BOX");
			level3Package2.KP_PackageID = "LEVEL3-2";
			Helper.CreateWhsPackageAudit(order2, "LEVEL3-2");

			var package3 = order3.PackageJob.Packages.AddNew("CNT");
			package3.KP_PackageID = "P03";
			package3.Container.K0_RC_ContainerType = containerType20GP.PK;
			var innerPackage3 = package2.Packages.AddNew("PLT");
			innerPackage3.KP_PackageID = "LEVEL2-3";
			var level3Package3 = innerPackage3.Packages.AddNew("BOX");
			level3Package3.KP_PackageID = "LEVEL3-3";
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			Helper.CreateWhsPackageAudit(order3, "LEVEL2-3");

			var package4 = order4.PackageJob.Packages.AddNew("CNT");
			package4.KP_PackageID = "P04";
			package4.Container.K0_RC_ContainerType = containerType20GP.PK;
			var innerPackage4 = package4.Packages.AddNew("PLT");
			innerPackage4.KP_PackageID = "LEVEL2-4";
			var level3Package4 = innerPackage4.Packages.AddNew("BOX");
			level3Package4.KP_PackageID = "LEVEL3-4";
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package4.PK, load1);
			Helper.CreateWhsPackageAudit(order4, "LEVEL3-4");

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(4, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order1["WOV_HasUnauditedOuters"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(true, (ZBool)load1Order2["WOV_HasUnauditedOuters"]);

			var load1Order3 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order3.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order3["WOV_HasUnauditedOuters"]);

			var load1Order4 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order4.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order4["WOV_HasUnauditedOuters"]);
		}

		public void TestView_HasUnauditedOuters_NotContainer_HasNoInner()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD03", data.Part1, 10m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD04", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2, order3, order4);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var package1 = order1.PackageJob.Packages.AddNew("PLT");
			package1.KP_PackageID = "P01";
			Helper.CreateWhsPackageAudit(order1, "P01");

			order2.WD_WLO_PlannedLoad = load1.PK;
			var package2 = order2.PackageJob.Packages.AddNew("PLT");
			package2.KP_PackageID = "P02";

			var package3 = order3.PackageJob.Packages.AddNew("PLT");
			package3.KP_PackageID = "P03";
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			Helper.CreateWhsPackageAudit(order3, "P03");

			var package4 = order4.PackageJob.Packages.AddNew("PLT");
			package4.KP_PackageID = "P04";
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package4.PK, load1);

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(4, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order1["WOV_HasUnauditedOuters"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(true, (ZBool)load1Order2["WOV_HasUnauditedOuters"]);

			var load1Order3 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order3.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order3["WOV_HasUnauditedOuters"]);

			var load1Order4 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order4.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order4["WOV_HasUnauditedOuters"]);
		}

		public void TestView_HasUnauditedOuters_MultiplePackagesAndOneIsNotAudited()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var package1 = order1.PackageJob.Packages.AddNew("PLT");
			var package2 = order1.PackageJob.Packages.AddNew("PLT");
			package1.KP_PackageID = "P01";
			package2.KP_PackageID = "P02";
			Helper.CreateWhsPackageAudit(order1, "P01");

			var package3 = order2.PackageJob.Packages.AddNew("PLT");
			var package4 = order2.PackageJob.Packages.AddNew("PLT");
			package3.KP_PackageID = "P03";
			package4.KP_PackageID = "P04";
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			Helper.CreateWhsPackageAudit(order2, "P03");

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(2, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(true, (ZBool)load1Order1["WOV_HasUnauditedOuters"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order2["WOV_HasUnauditedOuters"]);
		}

		public void TestView_HasUnauditedOuters_MultiplePackagesAndAllAudited()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var package1 = order1.PackageJob.Packages.AddNew("PLT");
			var package2 = order1.PackageJob.Packages.AddNew("PLT");
			package1.KP_PackageID = "P01";
			package2.KP_PackageID = "P02";
			Helper.CreateWhsPackageAudit(order1, "P01");
			Helper.CreateWhsPackageAudit(order1, "P02");

			var package3 = order2.PackageJob.Packages.AddNew("PLT");
			var package4 = order2.PackageJob.Packages.AddNew("PLT");
			package3.KP_PackageID = "P03";
			package4.KP_PackageID = "P04";
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			Helper.CreateWhsPackageAudit(order2, "P03");
			Helper.CreateWhsPackageAudit(order2, "P04");

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(2, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order1["WOV_HasUnauditedOuters"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order2["WOV_HasUnauditedOuters"]);
		}

		public void TestView_HasUnauditedOuters_Container_MultiplePackagesAndOneIsNotAudited()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var cnt1 = order1.PackageJob.Packages.AddNew("CNT");
			cnt1.Container.K0_RC_ContainerType = containerType20GP.PK;
			var package1 = cnt1.Packages.AddNew("PLT");
			var package2 = cnt1.Packages.AddNew("PLT");
			package1.KP_PackageID = "P01";
			package2.KP_PackageID = "P02";
			Helper.CreateWhsPackageAudit(order1, "P01");

			var cnt2 = order2.PackageJob.Packages.AddNew("CNT");
			cnt2.Container.K0_RC_ContainerType = containerType20GP.PK;
			var package3 = cnt2.Packages.AddNew("PLT");
			var package4 = cnt2.Packages.AddNew("PLT");
			package3.KP_PackageID = "P03";
			package4.KP_PackageID = "P04";
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			Helper.CreateWhsPackageAudit(order2, "P04");

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(2, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(true, (ZBool)load1Order1["WOV_HasUnauditedOuters"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order2["WOV_HasUnauditedOuters"]);
		}

		public void TestView_HasUnauditedOuters_Container_MultiplePackagesAndAllAudited()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var cnt1 = order1.PackageJob.Packages.AddNew("CNT");
			cnt1.Container.K0_RC_ContainerType = containerType20GP.PK;
			var package1 = cnt1.Packages.AddNew("PLT");
			var package2 = cnt1.Packages.AddNew("PLT");
			package1.KP_PackageID = "P01";
			package2.KP_PackageID = "P02";
			Helper.CreateWhsPackageAudit(order1, "P01");
			Helper.CreateWhsPackageAudit(order1, "P02");

			var cnt2 = order2.PackageJob.Packages.AddNew("CNT");
			cnt2.Container.K0_RC_ContainerType = containerType20GP.PK;
			var package3 = cnt2.Packages.AddNew("PLT");
			var package4 = cnt2.Packages.AddNew("PLT");
			package3.KP_PackageID = "P03";
			package4.KP_PackageID = "P04";
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			Helper.CreateWhsPackageAudit(order2, "P03");
			Helper.CreateWhsPackageAudit(order2, "P04");

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(2, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order1["WOV_HasUnauditedOuters"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order2["WOV_HasUnauditedOuters"]);
		}

		public void TestView_HasUnauditedOuters_NotContainer_HasInner()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD03", data.Part1, 10m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD04", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2, order3, order4);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var package1 = order1.PackageJob.Packages.AddNew("PLT");
			package1.KP_PackageID = "P01";
			var innerPackage1 = package1.Packages.AddNew("BOX");
			innerPackage1.KP_PackageID = "LEVEL2-1";
			Helper.CreateWhsPackageAudit(order1, "P01");

			order2.WD_WLO_PlannedLoad = load1.PK;
			var package2 = order2.PackageJob.Packages.AddNew("PLT");
			package2.KP_PackageID = "P02";
			var innerPackage2 = package2.Packages.AddNew("BOX");
			innerPackage2.KP_PackageID = "LEVEL2-2";
			Helper.CreateWhsPackageAudit(order2, "LEVEL2-2");

			var package3 = order3.PackageJob.Packages.AddNew("PLT");
			package3.KP_PackageID = "P03";
			var innerPackage3 = package2.Packages.AddNew("BOX");
			innerPackage3.KP_PackageID = "LEVEL2-3";
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			Helper.CreateWhsPackageAudit(order3, "P03");

			var package4 = order4.PackageJob.Packages.AddNew("PLT");
			package4.KP_PackageID = "P04";
			var innerPackage4 = package4.Packages.AddNew("BOX");
			innerPackage4.KP_PackageID = "LEVEL2-4";
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package4.PK, load1);
			Helper.CreateWhsPackageAudit(order4, "LEVEL2-4");

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(4, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order1["WOV_HasUnauditedOuters"]);

			var load1Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(true, (ZBool)load1Order2["WOV_HasUnauditedOuters"]);

			var load1Order3 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order3.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order3["WOV_HasUnauditedOuters"]);

			var load1Order4 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order4.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order4["WOV_HasUnauditedOuters"]);
		}

		public void TestView_HasUnauditedOuters_Mixed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var containerType20GP = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var load1 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL01", "CDS", startTime: DateTimeOffset.Now);
			var load2 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL02", "CDS", startTime: DateTimeOffset.Now);
			var load3 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL03", "CDS", startTime: DateTimeOffset.Now);
			var load4 = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL04", "CDS", startTime: DateTimeOffset.Now);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD01", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD02", data.Part1, 10m);
			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD03", data.Part1, 10m);
			var order4 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORD04", data.Part1, 10m);
			Helper.CreatePickNew(order1, order2, order3, order4);

			order1.WD_WLO_PlannedLoad = load1.PK;
			var package1 = order1.PackageJob.Packages.AddNew("CNT");
			package1.KP_PackageID = "P01";
			package1.Container.K0_RC_ContainerType = containerType20GP.PK;
			var innerPackage1 = package1.Packages.AddNew("PLT");
			innerPackage1.KP_PackageID = "LEVEL2-1";
			Helper.CreateWhsPackageAudit(order1, "LEVEL2-1");

			order2.WD_WLO_PlannedLoad = load2.PK;
			var package2 = order2.PackageJob.Packages.AddNew("PLT");
			package2.KP_PackageID = "P02";
			var innerPackage2 = package2.Packages.AddNew("BOX");
			innerPackage2.KP_PackageID = "LEVEL2-2";

			var package3 = order3.PackageJob.Packages.AddNew("CNT");
			package3.KP_PackageID = "P03";
			package3.Container.K0_RC_ContainerType = containerType20GP.PK;
			var innerPackage3 = package3.Packages.AddNew("PLT");
			innerPackage3.KP_PackageID = "LEVEL2-3";
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package3.PK, load1);
			Helper.CreateWhsPackageAudit(order3, "P03");

			var package4 = order4.PackageJob.Packages.AddNew("PLT");
			package4.KP_PackageID = "P04";
			var innerPackage4 = package4.Packages.AddNew("BOX");
			innerPackage4.KP_PackageID = "LEVEL2-4";
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package4.PK, load2);
			Helper.CreateWhsPackageAudit(order4, "LEVEL2-4");

			var package5 = order1.PackageJob.Packages.AddNew("BOX");
			package5.KP_PackageID = "P05";
			var loadPkgPackagePivot3 = Helper.CreateLoadPkgPackagePivot(package5.PK, load3);
			Helper.CreateWhsPackageAudit(order1, "P05");

			var package6 = order2.PackageJob.Packages.AddNew("BOX");
			package6.KP_PackageID = "P06";
			var loadPkgPackagePivot4 = Helper.CreateLoadPkgPackagePivot(package6.PK, load4);

			Factory.Save();

			var loadOrders = GetAllLoads();
			AssertEquals(6, loadOrders.Length);
			var load1Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals(false, (ZBool)load1Order1["WOV_HasUnauditedOuters"]);

			var load2Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load2.PK).Single();
			AssertEquals(true, (ZBool)load2Order2["WOV_HasUnauditedOuters"]);

			var load1Order3 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order3.PK && (ZGuid)o["WOV_WLO_Load"] == load1.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load1Order3["WOV_HasUnauditedOuters"]);

			var load2Order4 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order4.PK && (ZGuid)o["WOV_WLO_Load"] == load2.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load2Order4["WOV_HasUnauditedOuters"]);

			var load3Order1 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order1.PK && (ZGuid)o["WOV_WLO_Load"] == load3.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load3Order1["WOV_HasUnauditedOuters"]);

			var load4Order2 = loadOrders.Where(o => (ZGuid)o["WOV_WD_Docket"] == order2.PK && (ZGuid)o["WOV_WLO_Load"] == load4.PK).Single();
			AssertEquals("WOV_HasUnauditedOuters comes from Pivot is hardcoded false", false, (ZBool)load4Order2["WOV_HasUnauditedOuters"]);
		}

		DynamicBusinessObject[] GetAllLoads()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				$"select * from dbo.WhsLoadOrder";

			AssertNoExceptionThrown(() => result.Load(sql));
			return result.ToArray();
		}
	}
}
