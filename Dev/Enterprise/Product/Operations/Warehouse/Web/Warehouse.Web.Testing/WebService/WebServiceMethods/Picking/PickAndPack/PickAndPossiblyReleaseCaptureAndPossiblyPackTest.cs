using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class PickAndPossiblyReleaseCaptureAndPossiblyPackTest : WhsPickingSecureServiceTestCase
	{
		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_InvalidArguments

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_InvalidArguments_PickAndPackWithPackPickedPackageTypes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			AssertExceptionThrown<SoapException>(() => webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { Guid.NewGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(5m, false),
				new PackageInfo(),
				true));

			AssertEquals(typeof(ArgumentException), ErrorReporter.LastExceptionReported.GetType());
			AssertEquals("Should not attempt to create new packages for picked pack types when performing Pick and Pack.", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_SetIsPicking

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_SetIsPicking()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			var pickline = pick.GetAllPickLines().Single();
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline.PK).WZ_IsPicking);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(response1.Pick.Lines[0].Units, false), null, true);
			AssertSuccessfulResponse(response2, webService2);

			var pickedPickLine = pickline.InventoryLine.PickLines.Single();
			AssertEquals("Should not have unassigned picker.", "ST1", pickedPickLine.WZ_GS_NKAssignedTo);
			AssertEquals("Should not have changed Picked Time.", true, pickedPickLine.IsPicked);
			AssertEquals("Operator is finished picking.", false, new BusinessObjectFactory().Load<WhsPickLine>(pickline.PK).WZ_IsPicking);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackages

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackages()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			var pickline = pick.GetAllPickLines().Single();
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline.PK).WZ_IsPicking);

			var packTypeInfo1 = new PickedPackTypeInfo("BOX", 2m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo2 = new PickedPackTypeInfo("BAG", 3m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo3 = new PickedPackTypeInfo("PLT", 4m, "", Array.Empty<WhsReleaseCapturedInfo>());

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created package.", 3, order.PackageJob.Packages.Count);
			var package1 = order.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			var package2 = order.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			var package3 = order.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed);
			AssertEquals("Should have packed package.", 2m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have packed package.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have packed package.", 4m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_IgnoresInvalidPackTypes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			var pickline = pick.GetAllPickLines().Single();
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline.PK).WZ_IsPicking);

			var packTypeInfo1 = new PickedPackTypeInfo("ABC", 2m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo2 = new PickedPackTypeInfo("DEF", 3m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo3 = new PickedPackTypeInfo("GHE", 4m, "", Array.Empty<WhsReleaseCapturedInfo>());

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should not have created package.", 0, order.PackageJob.Packages.Count);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_DoesNotCreatePackagesIfArgFalse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			var pickline = pick.GetAllPickLines().Single();
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline.PK).WZ_IsPicking);

			var packTypeInfo1 = new PickedPackTypeInfo("BOX", 2m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo2 = new PickedPackTypeInfo("BAG", 3m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo3 = new PickedPackTypeInfo("PLT", 4m, "", Array.Empty<WhsReleaseCapturedInfo>());

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should not have created packages.", 0, order.PackageJob.Packages.Count);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_DoesNotCreatePackagesIfParamFalse()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);

			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = false;
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			var pickline = pick.GetAllPickLines().Single();
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline.PK).WZ_IsPicking);

			var packTypeInfo1 = new PickedPackTypeInfo("BOX", 2m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo2 = new PickedPackTypeInfo("BAG", 3m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo3 = new PickedPackTypeInfo("PLT", 4m, "", Array.Empty<WhsReleaseCapturedInfo>());

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should not have created packages.", 0, order.PackageJob.Packages.Count);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackages_TwoOrdersThreePackagesRCAs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 6m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var pickLines = pick.GetAllPickLines();
			var pickline1 = pickLines.Single(p => p.WZ_Units == 6m);
			var pickline2 = pickLines.Single(p => p.WZ_Units == 3m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline1.PK).WZ_IsPicking);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline2.PK).WZ_IsPicking);

			var packTypeInfo1 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });
			var packTypeInfo3 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });
			var packTypeInfo2 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test2", "", "", "", 3) });

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				createPackagesForPickedPacks: true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created 2 packages.", 2, order1.PackageJob.Packages.Count);
			var package1 = order1.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			var package2 = order1.PackageJob.Packages.Last(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package1.", 3m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have packed package2.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));

			AssertEquals("Should have created 1 package.", 1, order2.PackageJob.Packages.Count);
			var package3 = order2.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package3.", 3m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackages_ThreeOrdersRCAsComplex()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 12m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 6m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 2m);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2, order3);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var pickLines = pick.GetAllPickLines();
			var pickline1 = pickLines.Single(p => p.WZ_Units == 6m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline1.PK).WZ_IsPicking);

			var pickline2 = pickLines.Single(p => p.WZ_Units == 2m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline2.PK).WZ_IsPicking);

			var pickline3 = pickLines.Single(p => p.WZ_Units == 4m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline3.PK).WZ_IsPicking);

			var packTypeInfo1 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });
			var packTypeInfo2 = new PickedPackTypeInfo("BOX", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });
			var packTypeInfo3 = new PickedPackTypeInfo("BOT", 2m, "", new[] { new WhsReleaseCapturedInfo("Test2", "", "", "", 2) });
			var packTypeInfo4 = new PickedPackTypeInfo("", 0m, "", new[] { new WhsReleaseCapturedInfo("Test3", "", "", "", 4) });

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3, packTypeInfo4 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				createPackagesForPickedPacks: true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created 2 packages.", 2, order1.PackageJob.Packages.Count);
			var package1 = order1.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			var package2 = order1.PackageJob.Packages.Last(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			AssertEquals("Should have packed package1.", 3m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have packed package2.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));

			AssertEquals("Should have created 1 package.", 1, order3.PackageJob.Packages.Count);
			var package3 = order3.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BOT" && p.IsClosed);
			AssertEquals("Should have packed package3.", 2m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var query = new ZQuery(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1, SQLComparisonOperator.NotEqual, string.Empty);
			var rcaPickLines = newFactory.Load<WhsPickLine>(query);
			AssertEquals("Should be 6 Test1 RCAs.", 6m, rcaPickLines.Where(p => p.WZ_ReleaseCapturedPartAttrib1 == "Test1").Sum(d => d.WZ_Units));
			AssertEquals("Should be 2 Test2 RCAs.", 2m, rcaPickLines.Where(p => p.WZ_ReleaseCapturedPartAttrib1 == "Test2").Sum(d => d.WZ_Units));
			AssertEquals("Should be 4 Test3 RCAs.", 4m, rcaPickLines.Where(p => p.WZ_ReleaseCapturedPartAttrib1 == "Test3").Sum(d => d.WZ_Units));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackages_ThreeOrdersRCAs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 12m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 6m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 2m);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o3");
			Helper.CreateWhsOrderLine(order3, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2, order3);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var pickLines = pick.GetAllPickLines();
			var pickline1 = pickLines.Single(p => p.WZ_Units == 6m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline1.PK).WZ_IsPicking);

			var pickline2 = pickLines.Single(p => p.WZ_Units == 2m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline2.PK).WZ_IsPicking);

			var pickline3 = pickLines.Single(p => p.WZ_Units == 4m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline3.PK).WZ_IsPicking);

			var packTypeInfo1 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });
			var packTypeInfo2 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });
			var packTypeInfo3 = new PickedPackTypeInfo("BAG", 2m, "", new[] { new WhsReleaseCapturedInfo("Test2", "", "", "", 2) });
			var packTypeInfo4 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test3", "", "", "", 3) });
			var packTypeInfo5 = new PickedPackTypeInfo("", 0m, "", new[] { new WhsReleaseCapturedInfo("Test3", "", "", "", 1) });

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3, packTypeInfo4, packTypeInfo5 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				createPackagesForPickedPacks: true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created 2 packages.", 2, order1.PackageJob.Packages.Count);
			var package1 = order1.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package1.", 3m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			var package2 = order1.PackageJob.Packages.Last(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package2.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));

			AssertEquals("Should have created 1 package.", 1, order2.PackageJob.Packages.Count);
			var package3 = order2.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package3.", 2m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));

			AssertEquals("Should have created 1 package.", 1, order3.PackageJob.Packages.Count);
			var package4 = order3.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package3.", 3m, package4.PackedItemDivots.Sum(d => d.KI_PackedQty));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var query = new ZQuery(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1, SQLComparisonOperator.NotEqual, string.Empty);
			var rcaPickLines = newFactory.Load<WhsPickLine>(query);
			AssertEquals("Should be 6 Test1 RCAs.", 6m, rcaPickLines.Where(p => p.WZ_ReleaseCapturedPartAttrib1 == "Test1").Sum(d => d.WZ_Units));
			AssertEquals("Should be 2 Test2 RCAs.", 2m, rcaPickLines.Where(p => p.WZ_ReleaseCapturedPartAttrib1 == "Test2").Sum(d => d.WZ_Units));
			AssertEquals("Should be 4 Test3 RCAs.", 4m, rcaPickLines.Where(p => p.WZ_ReleaseCapturedPartAttrib1 == "Test3").Sum(d => d.WZ_Units));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackages_RCATwoOrdersNotFullySatisfied()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 12m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o2");
			Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o3");
			Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var pickLines = pick.GetAllPickLines();
			var pickline1 = pickLines.Single(p => p.WZ_Units == 2m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline1.PK).WZ_IsPicking);

			var pickline2 = pickLines.Single(p => p.WZ_Units == 4m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline2.PK).WZ_IsPicking);

			var packTypeInfo1 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });
			var packTypeInfo2 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				createPackagesForPickedPacks: true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created 0 packages.", 0, order1.PackageJob.Packages.Count);

			AssertEquals("Should have created 1 package.", 1, order2.PackageJob.Packages.Count);
			var package = order2.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package3.", 3m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var query = new ZQuery(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1, SQLComparisonOperator.NotEqual, string.Empty);
			var rcaPickLines = newFactory.Load<WhsPickLine>(query);
			AssertEquals("Should be 6 Test1 RCAs.", 6m, rcaPickLines.Where(p => p.WZ_ReleaseCapturedPartAttrib1 == "Test1").Sum(d => d.WZ_Units));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackages_RCATwoOrdersNotFullySatisfied_SerialNumbers()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, ZDate.Empty, ZDate.Empty, "", "RED", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o2");
			Helper.CreateWhsOrderLine(order1, data.Part1, 2m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o3");
			Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var pickLines = pick.GetAllPickLines();
			var pickline1 = pickLines.Single(p => p.WZ_Units == 2m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline1.PK).WZ_IsPicking);

			var pickline2 = pickLines.Single(p => p.WZ_Units == 4m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline2.PK).WZ_IsPicking);

			var releaseCapturedValues1 = new[]
			{
					new WhsReleaseCapturedInfo("SN1", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN2", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN3", "", "", "", 1),
				};
			var packTypeInfo1 = new PickedPackTypeInfo("BAG", 3m, "", releaseCapturedValues1);

			var releaseCapturedValues2 = new[]
			{
					new WhsReleaseCapturedInfo("SN4", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN5", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN6", "", "", "", 1),
				};
			var packTypeInfo2 = new PickedPackTypeInfo("BAG", 3m, "", releaseCapturedValues2);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				createPackagesForPickedPacks: true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created 0 packages.", 0, order1.PackageJob.Packages.Count);

			AssertEquals("Should have created 1 package.", 1, order2.PackageJob.Packages.Count);
			var package = order2.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package3.", 3m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var query = new ZQuery(WhsPickLineSchema.WZ_ReleaseCapturedPartAttrib1, SQLComparisonOperator.NotEqual, string.Empty);
			var rcaPickLines = newFactory.Load<WhsPickLine>(query);

			CombineAssertions(() =>
			{
				AssertEquals("Should be 6 RCA PickLines", 6, rcaPickLines.Length);
				AssertEquals("Should be a SN1 RCA.", 1, rcaPickLines.Count(p => p.WZ_ReleaseCapturedPartAttrib1 == "SN1"));
				AssertEquals("Should be a SN2 RCA.", 1, rcaPickLines.Count(p => p.WZ_ReleaseCapturedPartAttrib1 == "SN2"));
				AssertEquals("Should be a SN3 RCA.", 1, rcaPickLines.Count(p => p.WZ_ReleaseCapturedPartAttrib1 == "SN3"));
				AssertEquals("Should be a SN4 RCA.", 1, rcaPickLines.Count(p => p.WZ_ReleaseCapturedPartAttrib1 == "SN4"));
				AssertEquals("Should be a SN5 RCA.", 1, rcaPickLines.Count(p => p.WZ_ReleaseCapturedPartAttrib1 == "SN5"));
				AssertEquals("Should be a SN6 RCA.", 1, rcaPickLines.Count(p => p.WZ_ReleaseCapturedPartAttrib1 == "SN6"));
			});
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackages_PackingValuesOnOrder()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o1");
			Helper.CreateWhsOrderLine(order1, data.Part1, 6m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "o2");
			Helper.CreateWhsOrderLine(order2, data.Part1, 3m);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var pickLines = pick.GetAllPickLines();
			var pickline1 = pickLines.Single(p => p.WZ_Units == 6m);
			var pickline2 = pickLines.Single(p => p.WZ_Units == 3m);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline1.PK).WZ_IsPicking);
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline2.PK).WZ_IsPicking);

			var packTypeInfo1 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });
			var packTypeInfo3 = new PickedPackTypeInfo("PLT", 3m, "", new[] { new WhsReleaseCapturedInfo("Test1", "", "", "", 3) });
			var packTypeInfo2 = new PickedPackTypeInfo("BAG", 3m, "", new[] { new WhsReleaseCapturedInfo("Test2", "", "", "", 3) });

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				createPackagesForPickedPacks: true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			// Order #1
			var order1InNewFactory = newFactory.Load<WhsOrder>(order1.PK);
			AssertEquals("Should have created 2 packages.", 2, order1InNewFactory.PackageJob.Packages.Count);
			var package1 = order1InNewFactory.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package1.", 3m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			var package2 = order1InNewFactory.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed);
			AssertEquals("Should have packed package2.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));

			AssertEquals("Order1 WD_PackagesSent should be correct.", 1, order1InNewFactory.WD_PackagesSent);
			AssertEquals("Order1 WD_PalletsSent should be correct.", ((ZShort)1), order1InNewFactory.WD_PalletsSent);
			AssertEquals("Order1 WD_WeightSent should be correct.", 12m, order1InNewFactory.WD_WeightSent);
			AssertEquals("Order1 WD_CubicSent should be correct.", 0.12m, order1InNewFactory.WD_CubicSent);

			// Order #2
			var order2InNewFactory = newFactory.Load<WhsOrder>(order2.PK);
			AssertEquals("Should have created 1 package.", 1, order2InNewFactory.PackageJob.Packages.Count);
			var package3 = order2InNewFactory.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);
			AssertEquals("Should have packed package3.", 3m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));

			AssertEquals("Order2 WD_PackagesSent should be correct.", 1, order2InNewFactory.WD_PackagesSent);
			AssertEquals("Order2 WD_PalletsSent should be correct.", ZShort.Zero, order2InNewFactory.WD_PalletsSent);
			AssertEquals("Order2 WD_WeightSent should be correct.", 6m, order2InNewFactory.WD_WeightSent);
			AssertEquals("Order2 WD_CubicSent should be correct.", 0.06m, order2InNewFactory.WD_CubicSent);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_FullPallet()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;
			var loc = data.Whs1.FindLocation("A-1");

			Helper.CreateProductUnit(data.Part1, "PLT", 4m);
			Helper.CreateProductUnit(data.Part1, "BAG", 2m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, loc, palletID: "PID1");
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, loc, palletID: "PID2");
			var inventory21 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, loc, palletID: "PID2");
			var inventory3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, loc, palletID: "PID3");
			var inventory4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, loc, palletID: "PID4");
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 14m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var packTypeInfo1 = new PickedPackTypeInfo("PLT", 4m, "PID1", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo2 = new PickedPackTypeInfo("PLT", 4m, "PID2", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo3 = new PickedPackTypeInfo("BAG", 4m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo4 = new PickedPackTypeInfo("PLT", 2m, "PID4", Array.Empty<WhsReleaseCapturedInfo>());
			var pickLineToPackTypeInfo1 = new PickLinesToPickedPackTypeInfo(response1.Pick.Lines.Single(l => l.PalletID == "PID1").PKs, new[] { packTypeInfo1 });
			var pickLineToPackTypeInfo2 = new PickLinesToPickedPackTypeInfo(response1.Pick.Lines.Single(l => l.PalletID == "PID2").PKs, new[] { packTypeInfo2 });
			var pickLineToPackTypeInfo3 = new PickLinesToPickedPackTypeInfo(response1.Pick.Lines.Single(l => l.PalletID == "PID3").PKs, new[] { packTypeInfo3 });
			var pickLineToPackTypeInfo4 = new PickLinesToPickedPackTypeInfo(response1.Pick.Lines.Single(l => l.PalletID == "PID4").PKs, new[] { packTypeInfo4 });

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { pickLineToPackTypeInfo1, pickLineToPackTypeInfo2, pickLineToPackTypeInfo3, pickLineToPackTypeInfo4 },
				new PickingInfo(14m, false),
				null,
				true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created package.", 4, order.PackageJob.Packages.Count);
			var package1 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed && p.KP_PackageID == "PID1");
			var package2 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed && p.KP_PackageID == "PID2");
			var package3 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed && p.KP_PackageID == "");
			var package4 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed && p.KP_PackageID == "");
			AssertEquals("Should have packed package.", 4m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have packed package.", 4m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have packed package.", 4m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have packed package.", 2m, package4.PackedItemDivots.Sum(d => d.KI_PackedQty));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_FullPallet_CASPackType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;
			var loc = data.Whs1.FindLocation("A-1");

			Helper.CreateProductUnit(data.Part1, "PLT", 4m);
			Helper.CreateProductUnit(data.Part1, "BAG", 2m);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, loc, palletID: "PID1");
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, loc, palletID: "PID2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 6m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var packTypeInfo1 = new PickedPackTypeInfo("PLT", 4m, "PID1", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo2 = new PickedPackTypeInfo("CAS", 2m, "PID2", Array.Empty<WhsReleaseCapturedInfo>());
			var pickLineToPackTypeInfo1 = new PickLinesToPickedPackTypeInfo(response1.Pick.Lines.Single(l => l.PalletID == "PID1").PKs, new[] { packTypeInfo1 });
			var pickLineToPackTypeInfo2 = new PickLinesToPickedPackTypeInfo(response1.Pick.Lines.Single(l => l.PalletID == "PID2").PKs, new[] { packTypeInfo2 });

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { pickLineToPackTypeInfo1, pickLineToPackTypeInfo2 },
				new PickingInfo(6m, false),
				null,
				createPackagesForPickedPacks: true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created package.", 2, order.PackageJob.Packages.Count);
			var package1 = order.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed && p.KP_PackageID == "PID1");
			AssertEquals("Should have packed package.", 4m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));

			var package2 = order.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "CAS" && p.IsClosed && p.KP_PackageID == "PID2");
			AssertEquals("Should have packed package.", 2m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_FullPallet_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;
			var loc = data.Whs1.FindLocation("A-1");

			Helper.CreateProductUnit(data.Part1, "PLT", 4m);
			Helper.CreateProductUnit(data.Part1, "BAG", 2m);

			for (var i = 0; i < 20; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);
				Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, loc, palletID: "PID1_" + i);

				Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, loc, palletID: "PID2_" + i);
				Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, loc, palletID: "PID2_" + i);

				Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, loc, palletID: "PID3_" + i);
				receive.FinaliseDocketWithoutUserConfirmation();
			}

			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 240m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var pickLinesToPackTypes = new List<PickLinesToPickedPackTypeInfo>();
			for (var i = 0; i < 20; i++)
			{
				var packTypeInfo1 = new PickedPackTypeInfo("PLT", 4m, "PID1_" + i, Array.Empty<WhsReleaseCapturedInfo>());
				var packTypeInfo2 = new PickedPackTypeInfo("PLT", 4m, "PID2_" + i, Array.Empty<WhsReleaseCapturedInfo>());
				var packTypeInfo3 = new PickedPackTypeInfo("BAG", 4m, "", Array.Empty<WhsReleaseCapturedInfo>());
				var pickLineToPackTypeInfo1 = new PickLinesToPickedPackTypeInfo(response1.Pick.Lines.Single(l => l.PalletID == "PID1_" + i).PKs, new[] { packTypeInfo1 });
				var pickLineToPackTypeInfo2 = new PickLinesToPickedPackTypeInfo(response1.Pick.Lines.Single(l => l.PalletID == "PID2_" + i).PKs, new[] { packTypeInfo2 });
				var pickLineToPackTypeInfo3 = new PickLinesToPickedPackTypeInfo(response1.Pick.Lines.Single(l => l.PalletID == "PID3_" + i).PKs, new[] { packTypeInfo3 });

				pickLinesToPackTypes.Add(pickLineToPackTypeInfo1);
				pickLinesToPackTypes.Add(pickLineToPackTypeInfo2);
				pickLinesToPackTypes.Add(pickLineToPackTypeInfo3);
			}

			var expectedDBHits = new Dictionary<string, int>
			{
				{ WhsInventoryViewSchema.Constants.TableName, 3 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ StmALogSchema.Constants.TableName, 3 } // all the StmALog queries are unique.
			};

			var webService2 = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService2.Factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
					pickLinesToPackTypes.ToArray(),
					new PickingInfo(240m, false),
					null,
					true);

				AssertSuccessfulResponse(response2, webService2);
				AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
				AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			}
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackagesForMultiOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var packTypeInfo = new PickedPackTypeInfo("BOX", 3m, "", Array.Empty<WhsReleaseCapturedInfo>());

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo, packTypeInfo, packTypeInfo }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created package on Order1.", 1, order1.PackageJob.Packages.Count);
			var package1 = order1.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			AssertEquals("Should have packed package.", 3m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));

			AssertEquals("Should have created package on Order2.", 1, order2.PackageJob.Packages.Count);
			var package2 = order2.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			AssertEquals("Should have packed package.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackagesForMultiOrderPick_FirstAndThirdPackagesOnFirstOrder()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 12m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 8m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var packTypeInfo1 = new PickedPackTypeInfo("BOX", 5m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo2 = new PickedPackTypeInfo("CTN", 4m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo3 = new PickedPackTypeInfo("BOT", 3m, "", Array.Empty<WhsReleaseCapturedInfo>());

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created packages on Order1.", 2, order1.PackageJob.Packages.Count);
			var package1 = order1.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			AssertEquals("Should have packed package.", 5m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));

			var package2 = order1.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "BOT" && p.IsClosed);
			AssertEquals("Should have packed package.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));

			AssertEquals("Should have created package on Order2.", 1, order2.PackageJob.Packages.Count);
			var package3 = order2.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "CTN" && p.IsClosed);
			AssertEquals("Should have packed package.", 4m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_DoesNotCreatePackagesForPrePackedLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9m);
			var orderline = order.Lines[0];

			var pick = Helper.CreatePickNew(order);

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderline.ReleaseLines[0], 9m);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			var pickline = pick.GetAllPickLines().Single();
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline.PK).WZ_IsPicking);

			var packTypeInfo = new PickedPackTypeInfo("BOX", 3m, "", Array.Empty<WhsReleaseCapturedInfo>());

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo, packTypeInfo, packTypeInfo }) },
				new PickingInfo(9m, false),
				null,
				true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should not have modified packages.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Should not have modified packages.", "Pkg1", order.PackageJob.Packages[0].KP_PackageID);
			AssertEquals("Should not have modified packages.", false, order.PackageJob.Packages[0].IsClosed);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_PackageReleaseCaptured()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var staff = Helper.CreateGlbStaff("1", "1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "RED", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BLUE", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			// Deallocate 2 red.
			pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PartAttrib2 == "RED").PickLineQuantity -= 2m;
			factory.Save();

			var releaseCapturedValues1 = new[]
			{
					new WhsReleaseCapturedInfo("SN1", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN2", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN3", "", "", "", 1),
				};

			var releaseCapturedValues2 = new[]
			{
					new WhsReleaseCapturedInfo("SN6", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN7", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN8", "", "", "", 1),
				};

			var releaseCapturedValues3 = new[]
			{
					new WhsReleaseCapturedInfo("SN9", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN10", "", "", "", 1),
				};

			var pickLine1 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "RED");
			var pickLine2 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "BLUE");

			var packTypeInfo1 = new PickedPackTypeInfo("BOX", 3m, "", releaseCapturedValues1);
			var packTypeInfo2 = new PickedPackTypeInfo("PLT", 3m, "", releaseCapturedValues2);
			var packTypeInfo3 = new PickedPackTypeInfo("BAG", 2m, "", releaseCapturedValues3);
			var pickLineToPackTypeInfo1 = new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { packTypeInfo1 });
			var pickLineToPackTypeInfo2 = new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, new[] { packTypeInfo2, packTypeInfo3 });

			orderLine.ClearReleaseLines();
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { pickLineToPackTypeInfo1, pickLineToPackTypeInfo2 },
				new PickingInfo(8m, false),
				null,
				true);
			AssertEquals("There should be no error.", ErrorTypes.None, response.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response.ErrorMessage));

			AssertEquals("Should have created package.", 3, order.PackageJob.Packages.Count);
			var package1 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			var package2 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed);
			var package3 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);

			AssertEquals("Should have packed package.", 3, package1.PackedItemDivots.Count);
			AssertEquals("Should have packed correct Qty.", 3m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertRCAPackageContents(package1, new[] { "SN1", "SN2", "SN3" });

			AssertEquals("Should have packed package.", 3, package2.PackedItemDivots.Count);
			AssertEquals("Should have packed correct Qty.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertRCAPackageContents(package2, new[] { "SN6", "SN7", "SN8" });

			AssertEquals("Should have packed package.", 2, package3.PackedItemDivots.Count);
			AssertEquals("Should have packed correct Qty.", 2m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertRCAPackageContents(package3, new[] { "SN9", "SN10" });
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_PackageReleaseCaptured_PartiallyPacked()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;
			var staff = Helper.CreateGlbStaff("1", "1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "RED", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BLUE", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			// Deallocate 2 red.
			pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PartAttrib2 == "RED").PickLineQuantity -= 2m;
			factory.Save();

			var releaseCapturedValues1 = new[]
			{
					new WhsReleaseCapturedInfo("SN1", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN2", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN3", "", "", "", 1),
				};

			var releaseCapturedValues2 = new[]
			{
					new WhsReleaseCapturedInfo("SN6", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN7", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN8", "", "", "", 1),
				};

			var releaseCapturedValues3 = new[]
			{
					new WhsReleaseCapturedInfo("SN9", "", "", "", 1),
					new WhsReleaseCapturedInfo("SN10", "", "", "", 1),
				};

			var pickLine1 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "RED");
			var pickLine2 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "BLUE");

			var packTypeInfo1 = new PickedPackTypeInfo("BOX", 3m, "", releaseCapturedValues1);
			var packTypeInfo2 = new PickedPackTypeInfo("PLT", 3m, "", releaseCapturedValues2);
			var packTypeInfo3 = new PickedPackTypeInfo("", 0m, "", releaseCapturedValues3);
			var pickLineToPackTypeInfo1 = new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { packTypeInfo1 });
			var pickLineToPackTypeInfo2 = new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, new[] { packTypeInfo2, packTypeInfo3 });

			orderLine.ClearReleaseLines();
			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { pickLineToPackTypeInfo1, pickLineToPackTypeInfo2 },
				new PickingInfo(8m, false),
				null,
				true);
			AssertEquals("There should be no error.", ErrorTypes.None, response.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response.ErrorMessage));

			AssertEquals("Should have created packages.", 2, order.PackageJob.Packages.Count);
			var package1 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			var package2 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed);

			AssertEquals("Should have packed package.", 3, package1.PackedItemDivots.Count);
			AssertEquals("Should have packed correct Qty.", 3m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertRCAPackageContents(package1, new[] { "SN1", "SN2", "SN3" });

			AssertEquals("Should have packed package.", 3, package2.PackedItemDivots.Count);
			AssertEquals("Should have packed correct Qty.", 3m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertRCAPackageContents(package2, new[] { "SN6", "SN7", "SN8" });

			var orderLinePicklines = orderLine.PickLines;
			orderLinePicklines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "SN9" && pl.IsPickedFromPutawayLocation);
			orderLinePicklines.Single(pl => pl.WZ_ReleaseCapturedPartAttrib1 == "SN10" && pl.IsPickedFromPutawayLocation);
		}

		void AssertRCAPackageContents(PkgPackage package, string[] expectedRCAs)
		{
			foreach (var rcaValue in expectedRCAs)
			{
				AssertNotNull(
				$"Should have packed RCA with Attrib1 = {rcaValue}.",
				package.PackedItemDivots.SingleOrDefault(d => ((WhsPickLine)d.PackedItem).WZ_ReleaseCapturedPartAttrib1 == rcaValue));
			}
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_PackageReleaseCaptured_DBHits()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var staff = Helper.CreateGlbStaff("1", "1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			for (var i = 0; i < 20; i++)
			{
				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R" + i);
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m, ZDate.Empty, ZDate.Empty, "", "RED_" + i, "", "");
				Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 6m, ZDate.Empty, ZDate.Empty, "", "BLUE_" + i, "", "");
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocketWithoutUserConfirmation();
				AssertIsFinalisedPrecondition(receive);
			}

			factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 200m);
			var pick = Helper.CreatePickNew(order);
			factory.Save();

			// Deallocate 1 from each Red line.
			pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Where(a => a.PartAttrib2.StartsWith("RED")).ForEach(pl => pl.PickLineQuantity -= 1m);
			factory.Save();

			var pickLinesToPackTypes = new List<PickLinesToPickedPackTypeInfo>();
			for (var i = 0; i < 20; i++)
			{
				var releaseCapturedValues1 = new[]
				{
					new WhsReleaseCapturedInfo("SN1_" + i, "", "", "", 1),
					new WhsReleaseCapturedInfo("SN2_" + i, "", "", "", 1),
					new WhsReleaseCapturedInfo("SN3_" + i, "", "", "", 1),
				};

				var releaseCapturedValues2 = new[]
				{
					new WhsReleaseCapturedInfo("SN4_" + i, "", "", "", 1),
					new WhsReleaseCapturedInfo("SN5_" + i, "", "", "", 1),
					new WhsReleaseCapturedInfo("SN6_" + i, "", "", "", 1),
				};

				var releaseCapturedValues3 = new[]
				{
					new WhsReleaseCapturedInfo("SN7_" + i, "", "", "", 1),
					new WhsReleaseCapturedInfo("SN8_" + i, "", "", "", 1),
					new WhsReleaseCapturedInfo("SN9_" + i, "", "", "", 1),
				};

				var pickLine1 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "RED_" + i);
				var pickLine2 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "BLUE_" + i);

				var packTypeInfo1 = new PickedPackTypeInfo("BOX", 3m, "", releaseCapturedValues1);
				var packTypeInfo2 = new PickedPackTypeInfo("PLT", 3m, "", releaseCapturedValues2);
				var packTypeInfo3 = new PickedPackTypeInfo("BAG", 3m, "", releaseCapturedValues3);
				var pickLineToPackTypeInfo1 = new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { packTypeInfo1 });
				var pickLineToPackTypeInfo2 = new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, new[] { packTypeInfo2, packTypeInfo3 });

				pickLinesToPackTypes.Add(pickLineToPackTypeInfo1);
				pickLinesToPackTypes.Add(pickLineToPackTypeInfo2);
			}

			var expectedDBHits = new Dictionary<string, int>
			{
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 2 },
				{ StmALogSchema.Constants.TableName, 3 } // all the StmALog queries are unique.
			};

			var webService2 = GetNewWebService(data.Whs1, staff);
			using (TestCaseWithFactory.AssertDbHitsWithUsefulQueryInformation(expectedDBHits, webService2.Factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
					pickLinesToPackTypes.ToArray(),
					new PickingInfo(180m, false),
					null,
					true);

				AssertSuccessfulResponse(response2, webService2);
				AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
				AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			}

			AssertEquals("Should have created packages.", 60, order.PackageJob.Packages.Count);
			AssertEquals("Should have created packages.", 20, order.PackageJob.Packages.Count(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed));
			AssertEquals("Should have created packages.", 20, order.PackageJob.Packages.Count(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed));
			AssertEquals("Should have created packages.", 20, order.PackageJob.Packages.Count(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed));

			foreach (var package in order.PackageJob.Packages)
			{
				AssertEquals("Should have packed package.", 3, package.PackedItemDivots.Count);
				AssertEquals("Should have packed correct Qty.", 3m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			}
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_PackageReleaseCaptured_DuplicateAttributes()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			var staff = Helper.CreateGlbStaff("1", "1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "RED", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BLUE", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			// Deallocate 2 red.
			pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PartAttrib2 == "RED").PickLineQuantity -= 2m;
			factory.Save();

			var releaseCapturedValues1 = new[] { new WhsReleaseCapturedInfo("BIG", "", "", "", 3) };
			var releaseCapturedValues2 = new[] { new WhsReleaseCapturedInfo("BIG", "", "", "", 3) };
			var releaseCapturedValues3 = new[] { new WhsReleaseCapturedInfo("BIG", "", "", "", 2) };

			var pickLine1 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "RED");
			var pickLine2 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "BLUE");

			var packTypeInfo1 = new PickedPackTypeInfo("BOX", 3m, "", releaseCapturedValues1);
			var packTypeInfo2 = new PickedPackTypeInfo("PLT", 3m, "", releaseCapturedValues2);
			var packTypeInfo3 = new PickedPackTypeInfo("BAG", 2m, "", releaseCapturedValues3);
			var pickLineToPackTypeInfo1 = new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { packTypeInfo1 });
			var pickLineToPackTypeInfo2 = new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, new[] { packTypeInfo2, packTypeInfo3 });

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { pickLineToPackTypeInfo1, pickLineToPackTypeInfo2 },
				new PickingInfo(8m, false),
				null,
				true);
			AssertEquals("There should be no error.", ErrorTypes.None, response.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response.ErrorMessage));

			AssertEquals("Should have created package.", 3, order.PackageJob.Packages.Count);
			var package1 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			var package2 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed);
			var package3 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);

			AssertEquals("Should have packed single RCA.", 1, package1.PackedItemDivots.Count);
			AssertEquals("Should have packed correct RCA.", "BIG", ((WhsPickLine)package1.PackedItemDivots[0].PackedItem).WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Should have packed correct Qty.", 3m, package1.PackedItemDivots[0].KI_PackedQty);

			AssertEquals("Should have packed single RCA.", 1, package2.PackedItemDivots.Count);
			AssertEquals("Should have packed correct RCA.", "BIG", ((WhsPickLine)package2.PackedItemDivots[0].PackedItem).WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Should have packed correct Qty.", 3m, package2.PackedItemDivots[0].KI_PackedQty);

			AssertEquals("Should have packed single RCA.", 1, package3.PackedItemDivots.Count);
			AssertEquals("Should have packed correct RCA.", "BIG", ((WhsPickLine)package3.PackedItemDivots[0].PackedItem).WZ_ReleaseCapturedPartAttrib1);
			AssertEquals("Should have packed correct Qty.", 2m, package3.PackedItemDivots[0].KI_PackedQty);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortPickRelatedInventoryLine()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;
			var staff = Helper.CreateGlbStaff("1", "1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);

			AssertNoExceptionThrown("Should not catch Exception for 'Only one WHL_LogVersion per ParentDocketLine can be inserted in a single transaction. '",
				() => webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
					new[] { new PickLinesToPickedPackTypeInfo(pick.GetAllPickLines().Select(l => l.PK.ToGuid()).ToArray(), Array.Empty<PickedPackTypeInfo>()) },
					new PickingInfo(5, false),
					null,
					true));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_SplitPickLines()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;
			var staff = Helper.CreateGlbStaff("1", "1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			var pick = Helper.CreatePickNew(order);
			factory.Save();

			AssertEquals("Precondition: picklines count.", 2, orderLine.PickLines.Count);
			AssertContainsExactElementsInAnyOrder("Precondition: picklines units correct.", new[] { new ZDecimal(1m), new ZDecimal(5m) }, orderLine.PickLines.Select(p => p.WZ_Units));

			var packTypeInfo1 = new PickedPackTypeInfo("BOX", 2m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo2 = new PickedPackTypeInfo("PLT", 2m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var packTypeInfo3 = new PickedPackTypeInfo("BAG", 2m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var pickLineToPackTypeInfo = new PickLinesToPickedPackTypeInfo(new[] { orderLine.PickLines[0].PK.ToGuid(), orderLine.PickLines[1].PK.ToGuid() }, new[] { packTypeInfo1, packTypeInfo2, packTypeInfo3 });

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { pickLineToPackTypeInfo },
				new PickingInfo(6m, false),
				null,
				true);
			AssertEquals("There should be no error.", ErrorTypes.None, response.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response.ErrorMessage));

			AssertEquals("Should have created package.", 3, order.PackageJob.Packages.Count);
			var package1 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BOX" && p.IsClosed);
			var package2 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "PLT" && p.IsClosed);
			var package3 = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == "BAG" && p.IsClosed);

			AssertEquals("Should have packed package.", 2, package1.PackedItemDivots.Count);
			AssertEquals("Should have packed correct Qty.", 2m, package1.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have correct package status.", "Packed", new PackagePackingHelper(package1).GetPackageLabelStatus());

			AssertEquals("Should have packed package.", 1, package2.PackedItemDivots.Count);
			AssertEquals("Should have packed correct Qty.", 2m, package2.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have correct package status.", "Packed", new PackagePackingHelper(package2).GetPackageLabelStatus());

			AssertEquals("Should have packed package.", 1, package3.PackedItemDivots.Count);
			AssertEquals("Should have packed correct Qty.", 2m, package3.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should have correct package status.", "Packed", new PackagePackingHelper(package3).GetPackageLabelStatus());
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CreatesPackages_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;
			var staff = Helper.CreateGlbStaff("ST1", "ST1");

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;

			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, wheel, 50m, location);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "0T");
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, wheel, 2m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("Pick Lines are grouped by Parent Product now.", 2, response1.Pick.Lines.Count);
			var componentLinesForAssembly = response1.Pick.Lines.Single(pl => pl.BOMProductPK != Guid.Empty);
			var componentLinesOrdered = response1.Pick.Lines.Single(pl => pl.BOMProductPK == Guid.Empty);

			var packTypeInfo1 = new PickedPackTypeInfo("CAS", 2m, "", Array.Empty<WhsReleaseCapturedInfo>());

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(componentLinesOrdered.PKs, new[] { packTypeInfo1 }) },
				new PickingInfo(componentLinesOrdered.Units, false),
				null,
				true);
			CombineAssertions(() =>
			{
				AssertSuccessfulResponse(response2, webService2);
				AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
				AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));
			});

			AssertEquals("Should have created 1 package for Ordered Components.", 1, order.PackageJob.Packages.Count);
			var package = order.PackageJob.Packages.Single(p => p.KP_F3_NKPackType == "CAS" && p.IsClosed);
			AssertEquals("Should have packed package.", 1, package.PackedItemDivots.Count);
			AssertEquals("Should be correct divot KI_ParentID", orderLine2.PickLines[0].PK, package.PackedItemDivots[0].KI_ParentID);
			AssertEquals("Should have packed correct Qty.", 2m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals(true, orderLine2.PickLines[0].IsPickedFromPutawayLocation);

			var packTypeInfo2 = new PickedPackTypeInfo("BOX", 5m, "", Array.Empty<WhsReleaseCapturedInfo>());
			var webService3 = GetNewWebService(data.Whs1, staff);
			var response3 = webService3.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(componentLinesForAssembly.PKs, new[] { packTypeInfo2 }) },
				new PickingInfo(componentLinesForAssembly.Units, false),
				null,
				true);
			CombineAssertions(() =>
			{
				AssertSuccessfulResponse(response3, webService3);
				AssertEquals("There should be no error.", ErrorTypes.None, response3.Error);
				AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response3.ErrorMessage));
			});

			// PBB package should be ignored
			AssertEquals("Should NOT have created new packages.", 1, order.PackageJob.Packages.Count);
			AssertEquals("Should NOT have created new packages.", package.PK, order.PackageJob.Packages[0].PK);
			AssertEquals("Should NOT change KI_ParentID", orderLine2.PickLines[0].PK, package.PackedItemDivots[0].KI_ParentID);
			AssertEquals("Should NOT change Qty.", 2m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals(true, orderLine1.ChildComponentLines.Single().PickLines[0].IsPickedFromPutawayLocation);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ProductPackageSizes

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ProductPackageSizes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			var partUnit = Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, PkgUnit.Box, 2m);
			partUnit.OF_Depth = 2;
			partUnit.OF_Height = 4;
			partUnit.OF_Width = 6;
			partUnit.OF_Weight = 50;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			var pickline = pick.GetAllPickLines().Single();
			AssertEquals("Precondition - is picking .", true, new BusinessObjectFactory().Load<WhsPickLine>(pickline.PK).WZ_IsPicking);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { new PickedPackTypeInfo(PkgUnit.Box, 2m, "", Array.Empty<WhsReleaseCapturedInfo>()) }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("Should have created package.", 1, order.PackageJob.Packages.Count);
			var package = order.PackageJob.Packages.First(p => p.KP_F3_NKPackType == PkgUnit.Box && p.IsClosed);
			AssertEquals("Should have packed package.", 2m, package.PackedItemDivots.Sum(d => d.KI_PackedQty));
			AssertEquals("Should be correct KP_Length.", 2m, package.KP_Length);
			AssertEquals("Should be correct KP_Height.", 4m, package.KP_Height);
			AssertEquals("Should be correct KP_Width.", 6m, package.KP_Width);
			AssertEquals("Should be correct KP_Weight.", 54m, package.KP_Weight);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_AlreadyPicked

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_AlreadyPicked()
		{
			TestPickAndPossiblyReleaseCaptureAndPossiblyPack_AlreadyPicked_Core(false);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_InTransit()
		{
			TestPickAndPossiblyReleaseCaptureAndPossiblyPack_AlreadyPicked_Core(true);
		}

		void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_AlreadyPicked_Core(bool usingInTransitTransfer)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var pickLine = orderLine1.PickLines.Single();
			if (usingInTransitTransfer)
			{
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
				Helper.Factory.Save();
			}
			else
			{
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

				using (OutboundDockDoorHelper.MockOutboundDockDoorCreator())
				{
					Helper.Factory.Save();
					AssertEquals("Precondition: Picked Directly.", ZGuid.Empty, orderLine1.PickLines.Single().WZ_WE_OriginalPickedInventoryLine);
				}
			}

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(pick.GetAllPickLines().Select(pl => pl.PK.ToGuid()).ToArray(), Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(10m, false),
				null,
				true);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Should have shown an error as not all lines could be picked.", "Confirm Qty is greater than Requested Qty.", response2.ErrorMessage);
			AssertEquals("Should *not* have picked the unpicked pickline.", ZDateTimeOffset.Empty, orderLine2.PickLines[0].WZ_PickedDateTime);
		}

		#endregion

		#region PickAndPossiblyReleaseCaptureAndPossiblyPack

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_PickLineIsAssignedToOther()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickLineInfo = GetAndAssertPickLineInfo(data);
			var operator1 = Helper.CreateGlbStaff("OP1", "OP1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, operator1);
			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(pickLineInfo.PKs, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(pickLineInfo.Units, false),
				null,
				true);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Unable to update pick line. Pick line is assigned to another operator.", response.ErrorMessage);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_QtyError()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickLineInfo = GetAndAssertPickLineInfo(data);
			var webService = GetNewWebService(data.Whs1);
			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(pickLineInfo.PKs, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(pickLineInfo.Units + 1, false),
				null,
				true);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Confirm Qty is greater than Requested Qty.", response.ErrorMessage);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_Successful()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickLineInfo = GetAndAssertPickLineInfo(data);
			var webService = GetNewWebService(data.Whs1);
			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(pickLineInfo.PKs, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(pickLineInfo.Units, false),
				null,
				true);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("The response should not have any error.", ErrorTypes.None, response.Error);
			AssertNullOrEmpty("The error message of response should be null or empty.", response.ErrorMessage);
			var pickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery());
			AssertNullOrEmpty("Should not have assigned picker.", pickLines.Single(pl => pickLineInfo.PKs.Contains(pl.PK.ToGuid())).WZ_GS_NKAssignedTo);
		}

		WhsPickLineInfo GetAndAssertPickLineInfo(TestDataSimpleEnvironment data)
		{
			var warehouse = data.Whs1;
			Helper.CreateArea(warehouse, "A1", AreaTypes.Codes.FreeStore);
			var row11 = Helper.CreateRowAndGenerateLocations(warehouse, "Row11", 3);
			row11.Locations[0].WLV_WA_PickingArea = warehouse.Areas[1].PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, warehouse, "Receive1", data.Part1, 100m, row11.Locations[0], "Pallet1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, warehouse, "Order1", data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			pick.OrderedInventories[0].AvailableInventories[0].PickLineQuantity = 5;
			Helper.Factory.Save();

			var webService = GetNewWebService(warehouse);
			var response = webService.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Pick);
			AssertNotEquals(0, response.Pick.Lines.Count);
			AssertNotEquals(0, response.Pick.Lines[0].PKs.Length);
			AssertNotEquals(0, response.Pick.Lines[0].Units);

			return response.Pick.Lines[0];
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_MultiOrderPick()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { new PickedPackTypeInfo("", 9m, "", Array.Empty<WhsReleaseCapturedInfo>()) }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			var pickline1 = order1.Lines[0].PickLines[0];
			AssertEquals("PickLine1 is picked.", true, pickline1.IsPickedFromPutawayLocation);

			var pickline2 = order2.Lines[0].PickLines[0];
			AssertEquals("PickLine2 is picked.", true, pickline2.IsPickedFromPutawayLocation);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_MultiOrderPick_RCAs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			AssertEquals("Precondition.", true, order1.Lines[0].PickLines[0].WZ_PickedDateTime.IsEmpty);
			AssertEquals("Precondition.", true, order2.Lines[0].PickLines[0].WZ_PickedDateTime.IsEmpty);

			var releaseCapturedValues1 = new[] { new WhsReleaseCapturedInfo("BIG", "", "", "", 5m) };
			var releaseCapturedValues2 = new[] { new WhsReleaseCapturedInfo("SMALL", "", "", "", 4m) };
			var packTypeInfo1 = new PickedPackTypeInfo("", 5m, "", releaseCapturedValues1);
			var packTypeInfo2 = new PickedPackTypeInfo("", 4m, "", releaseCapturedValues2);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			var pickline1 = order1.Lines[0].PickLines[0];
			AssertEquals("PickLine1 is picked.", true, pickline1.IsPickedFromPutawayLocation);
			AssertEquals("PickLine1 has correct RCA.", "BIG", pickline1.WZ_ReleaseCapturedPartAttrib1);

			var pickline2 = order2.Lines[0].PickLines[0];
			AssertEquals("PickLine2 is picked.", true, pickline2.IsPickedFromPutawayLocation);
			AssertEquals("PickLine2 has correct RCA.", "SMALL", pickline2.WZ_ReleaseCapturedPartAttrib1);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_MultiOrderPick_SplitRCAs()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var pickParams = WhsClientPickingParams.GetClientPickingParams(data.Org1).WarehousePickPackParams.AddNew();
			pickParams.WPP_WW_Warehouse = data.Whs1.PK;
			pickParams.WPP_EnableAutoPackageCreationOnPicking = true;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 9m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 5m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);

			AssertEquals("Precondition.", false, orderLine1.PickLines[0].IsPickedFromPutawayLocation);
			AssertEquals("Precondition.", false, orderLine2.PickLines[0].IsPickedFromPutawayLocation);

			var releaseCapturedValues1 = new[] { new WhsReleaseCapturedInfo("BIG", "", "", "", 6m) };
			var releaseCapturedValues2 = new[] { new WhsReleaseCapturedInfo("SMALL", "", "", "", 3m) };
			var packTypeInfo1 = new PickedPackTypeInfo("", 6m, "", releaseCapturedValues1);
			var packTypeInfo2 = new PickedPackTypeInfo("", 3m, "", releaseCapturedValues2);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, new[] { packTypeInfo1, packTypeInfo2 }) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				false);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("There should be no error.", ErrorTypes.None, response2.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response2.ErrorMessage));

			AssertEquals("orderLine1 correct pickline count.", 1, orderLine1.PickLines.Count);
			var pickline1 = orderLine1.PickLines[0];
			AssertEquals("PickLine1 is picked.", true, pickline1.IsPickedFromPutawayLocation);
			AssertEquals("PickLine1 has correct RCA.", "BIG", pickline1.WZ_ReleaseCapturedPartAttrib1);

			AssertEquals("orderLine2 correct pickline count.", 2, orderLine2.PickLines.Count);
			var pickline2 = orderLine2.PickLines.Single(pl => pl.WZ_Units == 3m);
			AssertEquals("PickLine2 is picked.", true, pickline2.IsPickedFromPutawayLocation);
			AssertEquals("PickLine2 has correct RCA.", "SMALL", pickline2.WZ_ReleaseCapturedPartAttrib1);

			var pickline3 = orderLine2.PickLines.Single(pl => pl.WZ_Units == 1m);
			AssertEquals("PickLine3 is picked.", true, pickline3.IsPickedFromPutawayLocation);
			AssertEquals("PickLine3 has correct RCA.", "BIG", pickline3.WZ_ReleaseCapturedPartAttrib1);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CheckArguments

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_CheckArguments()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();

			var pickinfo = new PickingInfo(0, false, true);
			var webService1 = GetNewWebService(data.Whs1, staff);
			AssertBusinessValidationError(webService1, "Can only Pick zero units when shorting a Pick.",
				webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
					new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
					pickinfo,
					null,
					true));

			pickinfo.IsPickingSuspended = true;
			var webService2 = GetNewWebService(data.Whs1, staff);
			AssertNoExceptionThrown("It only call to suspend pick lines",
				() => webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
					new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
					pickinfo,
					null,
					true));
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_LineNotFound

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_LineNotFound()
		{
			var nonExistingLinePk = ZGuid.NewZGuid();
			AssertNull(new BusinessObjectFactory().Load<WhsPickLine>(nonExistingLinePk));
			var webService = GetNewWebService();
			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { nonExistingLinePk.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(10m, false),
				null,
				true);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Pick Line could not be found", response.ErrorMessage);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_NoOperator

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_NoOperator()
		{
			var webService = GetNewWebService();
			webService.SecurityHeader.UserName = "TST";
			webService.SecurityHeader.SecurityKey = "";

			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { Guid.NewGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(10m, false),
				null,
				true);
			AssertEquals(ErrorTypes.LoginFailed, response.Error);
			AssertEquals("You must enter a correct user name and/or password. Please try again.", response.ErrorMessage);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_DeletedWhenShortedToZero

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_DeletedWhenShortedToZero()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false, true, true),
				null,
				true);
			AssertEquals("Pick was suspended.", false, pickLine.IsDeleted);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var inventory2 = Helper.Factory.Load<WhsDocketLine>(pickLine.WZ_WE_InventoryLine);
			webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0, false, false, true),
				null,
				true);
			AssertEquals("Pick was shorted.", true, pickLine.IsDeleted);
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventory2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventory2.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortedSingle

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortedSingle()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLinePart1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Helper.Factory.Save();

			AssertEquals("Not yet Picked.", 0m, order.WD_UnitsSent);

			var pick = Helper.CreatePickNew(order);
			var pickLine = orderLinePart1.PickLines.Single();
			AssertEquals("Pre-condition: WD_UnitSent defaulted to be same as ordered quantity.", 10m, order.WD_UnitsSent);

			var webService = GetNewWebService(data.Whs1, staff);
			var inventoryLinePK = pickLine.WZ_WE_InventoryLine;
			webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				true);
			AssertEquals("Pickline was shorted.", 1m, pickLine.WZ_Units);
			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 1m, order.WD_UnitsSent);
			var inventoryLine = Helper.Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, inventoryLinePK));
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Inventory was held.", InventoryStatus.Codes.Held, inventoryLine.WE_CurrentInventoryStatus);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortedMulti

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortedMulti()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 5m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLinePart1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLinePart2 = Helper.CreateWhsOrderLine(order, data.Part2, 5m);
			AssertEquals("Not yet Picked.", 0m, order.WD_UnitsSent);

			var pick = Helper.CreatePickNew(order);
			var pickLinePart1 = orderLinePart1.PickLines.Single();
			var pickLinePart2 = orderLinePart2.PickLines.Single();
			var inventoryLinePK1 = pickLinePart1.WZ_WE_InventoryLine;
			var inventoryLinePK2 = pickLinePart2.WZ_WE_InventoryLine;
			Helper.Factory.Save();

			AssertEquals("Pre-condition: WD_UnitSent defaulted to be same as ordered quantity (sum of all order lines).", 15m, order.WD_UnitsSent);

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLinePart1.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				true);

			AssertEquals("Pickline was shorted.", 1m, pickLinePart1.WZ_Units);
			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 6m, order.WD_UnitsSent);
			var inventoryLine1 = Helper.Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, inventoryLinePK1));
			AssertEquals("Inventory's was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine1.WE_WHC_NKCurrentInventoryHeldCode);

			var webService2 = GetNewWebService(data.Whs1, staff);
			webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLinePart2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(2, false),
				null,
				true);
			AssertEquals("Pickline was shorted.", 2m, pickLinePart2.WZ_Units);
			AssertEquals("Pick is still marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order remains shorted.", 3m, order.WD_UnitsSent);
			var inventoryLine2 = Helper.Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, inventoryLinePK2));
			AssertEquals("Inventory was shorted.", InventoryHoldCodes.Codes.ShortPicked, inventoryLine2.WE_WHC_NKCurrentInventoryHeldCode);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_VerifiedEmpty

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_VerifiedEmpty_Yes()
		{
			TestPickAndPossiblyReleaseCaptureAndPossiblyPack_VerifiedEmpty(true);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_VerifiedEmpty_No()
		{
			TestPickAndPossiblyReleaseCaptureAndPossiblyPack_VerifiedEmpty(false);
		}

		void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_VerifiedEmpty(bool isVerifiedNonEmpty)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 100m);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			Helper.Factory.Save();

			AssertEquals(true, receive.IsFinalised);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 100m);
			var pick = Helper.CreatePickNew(order);

			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var pickResponse = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			var pickLineResponse = pickResponse.Pick.Lines[0];

			var pickLine = pick.GetAllPickLines().Single();
			var webService2 = GetNewWebService(data.Whs1, staff);
			webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(pickLineResponse.PKs, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(pickLineResponse.Units, isVerifiedNonEmpty),
				null,
				true);
			AssertEquals(isVerifiedNonEmpty ? "N" : "Y", pickLine.WZ_VerifiedEmpty);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_Suspend

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_Suspend()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			const decimal part1Quantity = 5m;
			const decimal part2Quantity = 10m;

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, part1Quantity);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, part2Quantity);
			var pick = Helper.CreatePickNew(order1, order2);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var pickFirstResponse = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(pickFirstResponse, webService1);

			var pickFromService = pickFirstResponse.Pick;
			AssertNotNull(pickFromService);
			AssertEquals(pick.WP_PickNo, pickFromService.Reference);
			AssertEquals(part1Quantity + part2Quantity, pickFirstResponse.Pick.Lines.Sum(l => l.Units));
			AssertEquals(2, pick.GetAllPickLines().Count());
			AssertEquals("Operator is picking.", true, pick.GetAllPickLines().All(pl => pl.WZ_IsPicking));

			const decimal numberOfFirstPick = 1m;
			var pickline = pick.GetAllPickLines().First();
			var previousQuantityToPick = pickline.WZ_Units;
			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickline.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(numberOfFirstPick, false, true, true),
				null,
				true);
			AssertSuccessfulResponse(response, webService2);
			AssertEquals("Operator not picking any more", true, pick.GetAllPickLines().All(pl => !pl.WZ_IsPicking));

			var webService3 = GetNewWebService(data.Whs1);
			var pickSecondResponse = webService3.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(pickSecondResponse, webService3);

			var picklines = new BusinessObjectFactory().Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, pickline.WZ_WE_TransactionLine));
			AssertEquals("Should split the pick line", 2, picklines.Length);
			AssertEquals("Should not change the total quantity when split the line", previousQuantityToPick, picklines.Sum(l => l.WZ_Units));
			AssertEquals("One line must be picked", 1, picklines.Count(pl => pl.WZ_Units == numberOfFirstPick && pl.IsPickedFromPutawayLocation));
			AssertEquals("Second line must not be picked", 1, picklines.Count(pl => !pl.IsPickedFromPutawayLocation));

			var pickFromService2 = pickSecondResponse.Pick;
			AssertNotNull(pickFromService2);
			AssertEquals(pick.WP_PickNo, pickFromService2.Reference);
			AssertEquals(part1Quantity + part2Quantity - numberOfFirstPick, pickSecondResponse.Pick.Lines.Sum(l => l.Units));
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_Suspend_ConcurrencyIssue()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var pick = Helper.CreatePickNew(order1);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var pickFirstResponse = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(pickFirstResponse, webService1);

			var pickFromService = pickFirstResponse.Pick;
			AssertNotNull(pickFromService);

			var pickline = pick.GetAllPickLines().First();
			var previousQuantityToPick = pickline.WZ_Units;
			var webService2 = GetNewWebService(data.Whs1);

			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)pickline).Row, Db.Connection);
			webService2.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickline.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1m, false, true, true),
				null,
				true);
			AssertBusinessValidationError(webService2, "While you have been working with this job another user has made changes. Please restart the operation and try again.", response);
			AssertEquals("Pick Line unchanged", true, pickline.WZ_IsPicking);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPackFromDifferentLocation_Suspend

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPackFromDifferentLocation_Suspend()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive1");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive1.FinaliseDocketWithoutUserConfirmation();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "Receive2");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation);
			receive2.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertIsFinalisedPrecondition(receive1);
			AssertIsFinalisedPrecondition(receive2);

			const decimal quantityToPick = 15m;

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, quantityToPick);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1);
			var pickFirstResponse = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(pickFirstResponse, webService1);

			var pickFromService = pickFirstResponse.Pick;
			AssertNotNull(pickFromService);
			AssertEquals(pick.WP_PickNo, pickFromService.Reference);
			AssertEquals(quantityToPick, pickFirstResponse.Pick.Lines.Sum(l => l.Units));
			AssertEquals(2, pick.GetAllPickLines().Count());

			const decimal numberOfFirstPick = 12m;
			var picklines = pick.GetAllPickLines().ToList();
			var webService2 = GetNewWebService(data.Whs1);
			var response = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(picklines.Select(l => l.PK.ToGuid()).ToArray(), Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(numberOfFirstPick, false, true),
				null,
				true);
			AssertSuccessfulResponse(response, webService2);

			var currentPicklines = new BusinessObjectFactory().Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, picklines.ToArray().Select(l => l.WZ_WE_TransactionLine).ToArray()));
			AssertEquals("Should split the pick line", 3, currentPicklines.Length);
			AssertEquals("Should not change the total quantity when split the line", quantityToPick, currentPicklines.Sum(l => l.WZ_Units));
			AssertEquals(numberOfFirstPick, currentPicklines.Where(l => l.IsPickedFromPutawayLocation).Sum(l => l.WZ_Units));
		}

		#endregion

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_HandleSaveExceptions

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_HandleZSaveException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			var pickline = pick.GetAllPickLines().Single();

			var webService2 = GetNewWebService(data.Whs1, staff);
			void action(BusinessObjectFactory factory)
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService2.Factory.Saving -= action;
				var row = ((INeedRow)order).Row;
				throw new ZSaveException(new DummyDataException(row, TestConnection), order.Factory);
			}
			webService2.Factory.Saving += action;

			var soapException = AssertExceptionThrown<SoapException>(() => webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				true));

			Assert("There should be a ZSaveException reported.", ErrorReporter.LastExceptionReported is ZSaveException);
			Assert("There should be a ZSaveException message reported.", ErrorReporter.LastMessageReported.Contains("Blah"));
			ErrorReporter.Clear();
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_HandleZCannotSaveException()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var staff = Helper.CreateGlbStaff("ST1", "ST1");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.GetWhsPick(pick.WP_PickNo, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY", PickGroup = 0 });
			AssertSuccessfulResponse(response1, webService1);
			var pickline = pick.GetAllPickLines().Single();

			var webService2 = GetNewWebService(data.Whs1, staff);
			void action(BusinessObjectFactory factory)
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService2.Factory.Saving -= action;
				throw new ZCannotSaveException("Test - Cannot Save", "Test Exception");
			}
			webService2.Factory.Saving += action;

			var response2 = webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(response1.Pick.Lines[0].PKs, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(response1.Pick.Lines[0].Units, false),
				null,
				true);
			AssertEquals("Should Error as validation prevents save.", ErrorTypes.BusinessValidationError, response2.Error);
			AssertEquals("Test - Cannot Save", response2.ErrorMessage);
			Assert("There should be no error reported.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_MultiplePickLinesToReleaseCapturedGroupingInfo

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_MultiplePickLinesToReleaseCapturedGroupingInfo()
		{
			var factory = Helper.Factory;
			var data = new TestDataSimpleEnvironment(factory);
			var staff = Helper.CreateGlbStaff("1", "1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.VIN);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "RED", "", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, ZDate.Empty, ZDate.Empty, "", "BLUE", "", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			// Deallocate 2 red.
			pick.OrderedInventories[0].AvailableInventories.Cast<WhsPickAvailableInventory>().Single(a => a.PartAttrib2 == "RED").PickLineQuantity -= 2m;
			factory.Save();

			var releaseCapturedValues1 = new[]
			{
				new WhsReleaseCapturedInfo("SN1", "", "", "", 1),
				new WhsReleaseCapturedInfo("SN2", "", "", "", 1),
				new WhsReleaseCapturedInfo("SN3", "", "", "", 1),
			};
			var releaseCapturedValues2 = new[]
			{
				new WhsReleaseCapturedInfo("SN6", "", "", "", 1),
				new WhsReleaseCapturedInfo("SN7", "", "", "", 1),
				new WhsReleaseCapturedInfo("SN8", "", "", "", 1),
				new WhsReleaseCapturedInfo("SN9", "", "", "", 1),
				new WhsReleaseCapturedInfo("SN10", "", "", "", 1),
			};

			var pickLine1 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "RED");
			var pickLine2 = orderLine.PickLines.Single(p => p.Inventory.WI_PartAttrib2 == "BLUE");

			var pickLinesToPackTypes1 = new PickLinesToPickedPackTypeInfo(new[] { pickLine1.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 3m, "", releaseCapturedValues1) });
			var pickLinesToPackTypes2 = new PickLinesToPickedPackTypeInfo(new[] { pickLine2.PK.ToGuid() }, new[] { new PickedPackTypeInfo("UNT", 5m, "", releaseCapturedValues2) });

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { pickLinesToPackTypes1, pickLinesToPackTypes2 },
				new PickingInfo(8m, false),
				null,
				false);
			AssertEquals("There should be no error.", ErrorTypes.None, response.Error);
			AssertEquals("There should be no error.", true, string.IsNullOrEmpty(response.ErrorMessage));

			var releaseLine1 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "SN1");
			var releaseLine2 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "SN2");
			var releaseLine3 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "SN3");
			var releaseLine4 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "SN6");
			var releaseLine5 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "SN7");
			var releaseLine6 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "SN8");
			var releaseLine7 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "SN9");
			var releaseLine8 = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(r => r.PartAttribute1 == "SN10");

			AssertContainsExactElementsInAnyOrder(new ZString[] { "SN1", "SN2", "SN3" }, orderLine.PickLines.Where(l => l.Inventory.WI_PartAttrib2 == "RED").Select(l => l.WZ_ReleaseCapturedPartAttrib1));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "SN6", "SN7", "SN8", "SN9", "SN10" }, orderLine.PickLines.Where(l => l.Inventory.WI_PartAttrib2 == "BLUE").Select(l => l.WZ_ReleaseCapturedPartAttrib1));
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack Return Value

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ReturnShortedOrderLines

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ReturnShortedOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 15m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			AssertEquals("Not yet Picked.", 0m, order.WD_UnitsSent);

			var pick = Helper.CreatePickNew(order);
			Helper.Factory.Save();

			AssertEquals("Pre-condition: WD_UnitSent defaulted to be same as ordered quantity (sum of all order lines).", 15m, order.WD_UnitsSent);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(pick.GetAllPickLines().Select(l => l.PK.ToGuid()).ToArray(), Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(1, false),
				null,
				true);

			AssertEquals("Pick was marked as shorted.", true, pick.HasShortfallItems);
			AssertEquals("Order was shorted.", 1m, order.WD_UnitsSent);
			AssertEquals("Both OrderLines were shorted.", 2, response.ShortedOrderLinePKs.Length);
			AssertContainsExactElementsInAnyOrder("OrderLine PK should match", new Guid[] { orderLine1.PK.ToGuid(), orderLine2.PK.ToGuid() }, response.ShortedOrderLinePKs);
		}

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ReturnEmptyIfAllPicked

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ReturnEmptyIfAllPicked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "Order1", Notify);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			AssertEquals("Not yet Picked.", 0m, order.WD_UnitsSent);

			var pick = Helper.CreatePickNew(order);
			var pickLinePart1 = orderLine1.PickLines.Single();
			Helper.Factory.Save();

			AssertEquals("Pre-condition: WD_UnitSent defaulted to be same as ordered quantity (sum of all order lines).", 10m, order.WD_UnitsSent);

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response = webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { pickLinePart1.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(10, false),
				null,
				true);

			AssertEquals("Pickline was fully picked.", 10m, pickLinePart1.WZ_Units);
			AssertEquals("Pick was not marked as shorted.", false, pick.HasShortfallItems);
			AssertEquals("Order was not shorted.", 10m, order.WD_UnitsSent);
			AssertEquals("OrderLine was not shorted.", 0, response.ShortedOrderLinePKs.Length);
		}

		#endregion

		#endregion

		#region TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortPickByBOMComponentLine

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortPickByBOMComponentLine_PartiallyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var componentPickLine = orderLine.ChildComponentLines.First().PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, componentPickLine.WZ_Units);

			var createdReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);

			var webService = GetNewWebService(data.Whs1, staff);
			webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(6m, false),
				null,
				true);
			AssertEquals("Picked 6 wheels.", 6m, componentPickLine.WZ_Units);
			AssertNotNull(componentPickLine.WZ_PickedDateTime);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedLink = reloadedReceiveLine.BOMComponentLinks.First();
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 3m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 3m);
			AssertCreatedLink(reloadedLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortPickByBOMComponentLine_FullyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var componentPickLine = orderLine.ChildComponentLines.First().PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, componentPickLine.WZ_Units);

			var createdReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);

			var webService = GetNewWebService(data.Whs1, staff);
			webService.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0m, false),
				null,
				true);
			AssertEquals("Fully shorted.", true, componentPickLine.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			AssertEquals("Pick Line deleted due to Short Picking.", 0, orderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, orderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery).Length);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortPickByBOMComponentLine_OtherComponentsAlreadyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { framePickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(4m, false),
				null,
				true);
			AssertEquals("Picked 4 frames.", 4m, framePickLine.WZ_Units);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			var reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 4m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 4m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 8m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 4m);

			var webService2 = GetNewWebService(data.Whs1, staff);
			webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { wheelPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(7m, false),
				null,
				true);
			AssertEquals("Picked 7 wheels.", 7m, wheelPickLine.WZ_Units);

			newFactory = new BusinessObjectFactory();
			reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			reloadedWheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == wheelComponentLine.PK);
			reloadedFrameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_WE_ComponentLine == frameComponentLine.PK);
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 3m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 3m);
			AssertCreatedLink(reloadedWheelLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);
			AssertCreatedLink(reloadedFrameLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 3m);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortPickByBOMComponentLine_OtherComponentsAlreadyFullyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			var frame = Helper.CreateProduct(data.Org1, "FRAME");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);
			Helper.CreateProductBOM(bike, frame, 1m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, frame, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 5m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 2, orderLine.ChildComponentLines.Count);
			var wheelComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK);
			var frameComponentLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK);
			AssertEquals("Should be allocated.", 1, orderLine.ChildComponentLines.First().PickLines.Count);
			var wheelPickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == wheel.PK).PickLines[0];
			var framePickLine = orderLine.ChildComponentLines.Single(l => l.WE_OP == frame.PK).PickLines[0];
			AssertEquals("Allocated 10 wheels.", 10m, wheelPickLine.WZ_Units);
			AssertEquals("Allocated 5 frames.", 5m, framePickLine.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 5m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 5m);
			AssertEquals("Links created.", 2, createdReceiveLine1.BOMComponentLinks.Count());
			var wheelLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 10m);
			var frameLink = createdReceiveLine1.BOMComponentLinks.Single(l => l.WIP_ComponentQuantity == 5m);
			AssertCreatedLink(wheelLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);
			AssertCreatedLink(frameLink, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == frame.PK), 5m);

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { framePickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0m, false),
				null,
				true);
			AssertEquals("Pick Line was deleted.", true, framePickLine.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(reloadedReceive, order);
			AssertEquals("Receive Line deleted due to Short Picking.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Pick Line deleted due to Short Picking.", 0, reloadedOrderLine.PickLines.Count);
			var linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, orderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links deleted due to Short Picking.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery).Length);

			var webService2 = GetNewWebService(data.Whs1, staff);
			webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { wheelPickLine.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(7m, false),
				null,
				true);

			newFactory = new BusinessObjectFactory();
			wheelPickLine = newFactory.Load<WhsPickLine>(wheelPickLine.PK);
			AssertEquals("Picked 7 wheels, but due to lack of frames we won't be able to build any bike.", 7m, wheelPickLine.WZ_Units);

			reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(reloadedReceive, order);
			AssertEquals("Receive Line remains deleted.", 0, reloadedReceive.Lines.Cast<WhsReceiveLine>().Count());
			reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine.PK);
			AssertEquals("Pick Line remains deleted.", 0, reloadedOrderLine.PickLines.Count);
			linkQuery = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, orderLine.ChildComponentLines.Select(l => l.PK).ToArray());
			AssertEquals("BOM Inventory Links remains deleted.", 0, Helper.Factory.Load<WhsBOMInventoryPivot>(linkQuery).Length);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortPickByBOMComponentLine_OtherPickLinesOnTheSameComponentAlreadyShorted()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-2"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, bike, 10m);

			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine.ChildComponentLines.Count);
			AssertEquals("Should be allocated.", 2, orderLine.ChildComponentLines.First().PickLines.Count);
			var componentPickLine1 = orderLine.ChildComponentLines.Single().PickLines[0];
			var componentPickLine2 = orderLine.ChildComponentLines.Single().PickLines[1];
			AssertEquals("Allocated 10 wheels.", 10m, componentPickLine1.WZ_Units);
			AssertEquals("Allocated 10 wheels.", 10m, componentPickLine2.WZ_Units);

			var createdReceive = Helper.Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			AssertCreatedReceive(createdReceive, order);
			var createdReceiveLine1 = createdReceive.Lines[0];
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 10m);
			var createdPickLines = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines.Length);
			var relatedPickLine = createdPickLines[0];
			AssertCreatedPickLine(relatedPickLine, createdReceiveLine1, orderLine, 10m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link, createdReceiveLine1, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 20m);

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine1.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(0m, false),
				null,
				true);
			AssertEquals("Nothing picked.", true, componentPickLine1.IsDeleted);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedLink = reloadedReceiveLine.BOMComponentLinks.First();
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 5m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 5m);
			AssertCreatedLink(reloadedLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 10m);

			var webService2 = GetNewWebService(data.Whs1, staff);
			webService2.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(4m, false),
				null,
				true);
			AssertEquals("Picked 4 wheels.", 4m, componentPickLine2.WZ_Units);

			newFactory = new BusinessObjectFactory();
			reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			reloadedReceiveLine = reloadedReceive.Lines.Cast<WhsReceiveLine>().Single();
			reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			reloadedLink = reloadedReceiveLine.BOMComponentLinks.First();
			AssertCreatedReceive(reloadedReceive, order);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 2m);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, orderLine, 2m);
			AssertCreatedLink(reloadedLink, reloadedReceiveLine, (WhsOrderLine)order.AllLines.Single(l => l.WE_OP == wheel.PK), 4m);
		}

		public void TestPickAndPossiblyReleaseCaptureAndPossiblyPack_ShortPickByBOMComponentLine_ShortedInventoryWereCommittedToDifferentOrderLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("OP1", "Test1");
			Helper.Factory.Save();

			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, PkgUnit.Unit);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive1, wheel, 10m, data.Whs1.FindLocation("A-1"));
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive1);

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order1, bike, 2m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", pickOption: WhsPickOption.Codes.Manual);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, bike, 3m);

			pick.AddOrders(new[] { order1, order2 });
			pick.AutoAllocateItemsWithMock(); // Mock rules with FIFO, we are testing the higher level BOM parts
			Helper.Factory.Save();
			AssertEquals("Added component orderline should exist.", 1, orderLine1.ChildComponentLines.Count);
			AssertEquals("Added component orderline should exist.", 1, orderLine2.ChildComponentLines.Count);
			var componentOrderLine1 = orderLine1.ChildComponentLines.Single();
			var componentOrderLine2 = orderLine2.ChildComponentLines.Single();
			AssertEquals("Should be allocated.", 1, orderLine1.ChildComponentLines.Single().PickLines.Count);
			AssertEquals("Should be allocated.", 1, orderLine2.ChildComponentLines.Single().PickLines.Count);
			var componentPickLine1 = orderLine1.ChildComponentLines.Single().PickLines[0];
			var componentPickLine2 = orderLine2.ChildComponentLines.Single().PickLines[0];
			AssertEquals("Allocated 4 wheels.", 4m, componentPickLine1.WZ_Units);
			AssertEquals("Allocated 6 wheels.", 6m, componentPickLine2.WZ_Units);

			var createdReceive = Helper.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			AssertCreatedReceive(createdReceive, order1);
			var createdReceiveLines = createdReceive.Lines.Cast<WhsReceiveLine>().ToArray();
			AssertEquals("There are 1 Receive Line for each Kit Order Line.", 2, createdReceiveLines.Length);
			var createdReceiveLine1 = createdReceiveLines.Single(l => l.WE_TransactionQuantity == 2m);
			var createdReceiveLine2 = createdReceiveLines.Single(l => l.WE_TransactionQuantity == 3m);
			AssertCreatedReceiveLine(createdReceiveLine1, bike, 2m);
			AssertCreatedReceiveLine(createdReceiveLine2, bike, 3m);
			var createdPickLines1 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine1.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines1.Length);
			var createdPickLines2 = Helper.Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, createdReceiveLine2.PK));
			AssertEquals("Should be committed to relevant Order Line through PickLine.", 1, createdPickLines2.Length);
			var relatedPickLine1 = createdPickLines1[0];
			var relatedPickLine2 = createdPickLines2[0];
			AssertCreatedPickLine(relatedPickLine1, createdReceiveLine1, orderLine1, 2m);
			AssertCreatedPickLine(relatedPickLine2, createdReceiveLine2, orderLine2, 3m);
			AssertEquals("Links created.", 1, createdReceiveLine1.BOMComponentLinks.Count());
			var link1 = createdReceiveLine1.BOMComponentLinks.First();
			AssertCreatedLink(link1, createdReceiveLine1, (WhsOrderLine)order1.AllLines.Single(l => l.WE_OP == wheel.PK), 4m);
			var link2 = createdReceiveLine2.BOMComponentLinks.First();
			AssertCreatedLink(link2, createdReceiveLine2, (WhsOrderLine)order2.AllLines.Single(l => l.WE_OP == wheel.PK), 6m);

			var webService1 = GetNewWebService(data.Whs1, staff);
			webService1.PickAndPossiblyReleaseCaptureAndPossiblyPack(
				new[] { new PickLinesToPickedPackTypeInfo(new[] { componentPickLine1.PK.ToGuid(), componentPickLine2.PK.ToGuid() }, Array.Empty<PickedPackTypeInfo>()) },
				new PickingInfo(2m, false),
				null,
				true);
			var componentPickLines = orderLine1.ChildComponentLines.Single().PickLines.Concat(orderLine2.ChildComponentLines.Single().PickLines).ToArray();
			AssertEquals("1 Pick Line remained.", 1, componentPickLines.Length);
			AssertEquals("Picked 2 wheels.", 2m, componentPickLines[0].WZ_Units);
			AssertNotNull(componentPickLines[0].WZ_PickedDateTime);

			var newFactory = new BusinessObjectFactory();
			var reloadedReceive = newFactory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK)).Single();
			var reloadedReceiveLines = reloadedReceive.Lines.Cast<WhsReceiveLine>().ToArray();
			AssertEquals("1 Receive Line is deleted because of shorting the same inventory.", 1, reloadedReceiveLines.Length);
			var reloadedReceiveLine = reloadedReceiveLines[0];
			var reloadedPickLine = newFactory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, reloadedReceiveLine.PK)).Single();
			var reloadedLink = reloadedReceiveLine.BOMComponentLinks.First();
			AssertCreatedReceive(reloadedReceive, order1);
			AssertCreatedReceiveLine(reloadedReceiveLine, bike, 1m);
			var reloadedOrderLine = newFactory.Load<WhsOrderLine>(orderLine2.PK);
			AssertCreatedPickLine(reloadedPickLine, reloadedReceiveLine, reloadedOrderLine, 1m);
			AssertCreatedLink(reloadedLink, reloadedReceiveLine, reloadedOrderLine.ChildComponentLines.Cast<WhsOrderLine>().Single(), 2m);
		}

		void AssertCreatedReceive(WhsReceive createdReceive, WhsOrder order)
		{
			AssertNotNull("Should created a new Receive.", createdReceive);
			AssertEquals(order.WD_WW_Whs, createdReceive.WD_WW_Whs);
			AssertEquals(order.WD_OH_Client, createdReceive.WD_OH_Client);
			AssertEquals((byte)0, createdReceive.WD_ExternalReferenceSplit);
			AssertEquals(0m, createdReceive.WD_TotalUnits);
			AssertEquals(0, createdReceive.WD_PackagesSent);
			AssertEquals((short)0, createdReceive.WD_TotalPallets);

			AssertNull("Should not create Billing Job for this Receive.", createdReceive.JobHeader);
		}

		void AssertCreatedReceiveLine(WhsReceiveLine createdReceiveLine1, OrgSupplierPart kit, decimal stockOnHand)
		{
			AssertEquals(kit.PK, createdReceiveLine1.WE_OP);
			AssertEquals(stockOnHand, createdReceiveLine1.WE_TransactionQuantity);
			AssertEquals(ZDateTimeOffset.Empty, createdReceiveLine1.WE_AdjustmentArrivalDate);
		}

		void AssertCreatedPickLine(WhsPickLine pickLine, WhsReceiveLine receiveLine, WhsOrderLine orderLine, ZDecimal units)
		{
			AssertEquals(orderLine.PK, pickLine.WZ_WE_TransactionLine);
			AssertEquals(receiveLine.PK, pickLine.WZ_WE_InventoryLine);
			AssertEquals(units, pickLine.WZ_Units);
			AssertEquals(receiveLine.WE_F3_NKPackType, pickLine.WZ_UnitsUQ);
			AssertEquals(0m, pickLine.WZ_OriginalReservedQty);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedPartAttrib1);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedPartAttrib2);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedPartAttrib3);
			AssertEquals(string.Empty, pickLine.WZ_ReleaseCapturedSerialNumber);
		}

		void AssertCreatedLink(WhsBOMInventoryPivot link, WhsReceiveLine receiveLine, WhsOrderLine orderLine, decimal componentQuantity)
		{
			AssertEquals(orderLine.PK, link.WIP_WE_ComponentLine);
			AssertEquals(receiveLine.PK, link.WIP_WE_InventoryLine);
			AssertEquals(componentQuantity, link.WIP_ComponentQuantity);
		}

		#endregion
	}
}
