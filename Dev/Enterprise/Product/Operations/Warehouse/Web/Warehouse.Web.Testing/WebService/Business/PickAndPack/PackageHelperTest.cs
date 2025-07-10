using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	class PackageHelperTest : WhsTestCaseWithFactory
	{
		#region TestGetPackageRelatedProductInfos

		public void TestGetPackageRelatedProductInfos_NoReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var part1 = data.Part1;
			part1.OP_Desc = "DESCRIPTION";
			part1.OP_Weight = 2.54m;
			part1.OP_WeightUQ = "KG";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", part1, 50m);
			var inventoryLine = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsPickableDocketLine(order, part1, 20);
			Helper.CreatePickNew(order);
			var pickLine1 = Helper.CreateWhsPickLine(orderLine, inventoryLine, 10);
			var pickLine2 = Helper.CreateWhsPickLine(orderLine, inventoryLine, 10);

			var package = order.PackageJob.Packages.AddNew("PLT", "123");
			var divot = package.PackedItemDivots.AddNew();
			divot.KI_PackedQty = 3;
			divot.KI_ParentTableCode = "WZ";
			divot.KI_ParentID = pickLine1.PK;

			var randomDivot1 = package.PackedItemDivots.AddNew();
			randomDivot1.KI_PackedQty = 1;
			randomDivot1.KI_ParentTableCode = "CUI";
			randomDivot1.KI_ParentID = pickLine2.PK;

			var randomDivot2 = package.PackedItemDivots.AddNew();
			randomDivot2.KI_PackedQty = 1;
			randomDivot2.KI_ParentTableCode = "WZ";
			randomDivot2.KI_ParentID = ZGuid.NewZGuid();

			Factory.Save();

			var resultDivot = PackageHelper.GetPackageRelatedProductInfos(Factory, package).Single();

			AssertEquals("Product Pk should be same", part1.PK, resultDivot.ProductPK);
			AssertEquals("Product Code should be same", "P1", resultDivot.ProductCode);
			AssertEquals("Product Weight should be same", 2.54m, resultDivot.ProductWeight);
			AssertEquals("Product Weight UQ should be same", "KG", resultDivot.ProductWeightUQ);
			AssertEquals("Product Description should be same", "DESCRIPTION", resultDivot.ProductDescription);
			AssertEquals("Expected Quantity should be same", 3m, resultDivot.ExpectedQty);
		}

		public void TestGetPackageRelatedProductInfos_WithReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Desc = "DESCRIPTION";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var inventoryLine = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 20);
			Helper.CreatePickNew(order);
			var pickLine = Helper.CreateWhsPickLine(orderLine, inventoryLine, 10);
			var package = order.PackageJob.Packages.AddNew("PLT", "123");

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "A1";
			releaseLine1.PartAttribute2 = "B1";
			releaseLine1.PartAttribute3 = "C1";
			releaseLine1.Quantity = 5m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "A2";
			releaseLine2.PartAttribute2 = "B2";
			releaseLine2.PartAttribute3 = "C2";
			releaseLine2.Quantity = 4m;

			var pickLine1 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "A1");
			var pickLine2 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "A2");
			var pickLine3 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "");

			var divot1 = package.PackedItemDivots.AddNew();
			divot1.KI_PackedQty = 2;
			divot1.KI_ParentTableCode = "WZ";
			divot1.KI_ParentID = pickLine1.PK;

			var divot2 = package.PackedItemDivots.AddNew();
			divot2.KI_PackedQty = 1;
			divot2.KI_ParentTableCode = "WZ";
			divot2.KI_ParentID = pickLine2.PK;

			var randomDivot1 = package.PackedItemDivots.AddNew();
			randomDivot1.KI_PackedQty = 1;
			randomDivot1.KI_ParentTableCode = "CUI";
			randomDivot1.KI_ParentID = pickLine3.PK;

			var randomDivot2 = package.PackedItemDivots.AddNew();
			randomDivot2.KI_PackedQty = 1;
			randomDivot2.KI_ParentTableCode = "WZ";
			randomDivot2.KI_ParentID = ZGuid.NewZGuid();

			Factory.Save();

			var resultDivot = PackageHelper.GetPackageRelatedProductInfos(Factory, package).Single();

			AssertEquals("Product Pk should be same", data.Part1.PK, resultDivot.ProductPK);
			AssertEquals("Product Code should be same", "P1", resultDivot.ProductCode);
			AssertEquals("Product Description should be same", "DESCRIPTION", resultDivot.ProductDescription);
			AssertEquals("Expected Quantity should be same", 3m, resultDivot.ExpectedQty);
		}

		public void TestGetPackageRelatedProductInfos_MultipleProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Desc = "DESCRIPTION1";
			data.Part2.OP_Desc = "DESCRIPTION2";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 50m);
			var inventoryLine1 = receive1.Inventory[0];
			var inventoryLine2 = receive2.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsPickableDocketLine(order, data.Part1, 20);
			var orderLine2 = Helper.CreateWhsPickableDocketLine(order, data.Part2, 20);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = Helper.CreateWhsPickLine(orderLine1, inventoryLine1, 10);
			var pickLine2 = Helper.CreateWhsPickLine(orderLine2, inventoryLine2, 10);

			var package = order.PackageJob.Packages.AddNew("PLT", "123");

			var releaseLine1 = orderLine1.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "A1";
			releaseLine1.PartAttribute2 = "B1";
			releaseLine1.PartAttribute3 = "C1";
			releaseLine1.Quantity = 5m;

			var newPickLine1 = orderLine1.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "A1");

			var divot1 = package.PackedItemDivots.AddNew();
			divot1.KI_PackedQty = 3;
			divot1.KI_ParentTableCode = "WZ";
			divot1.KI_ParentID = pickLine1.PK;

			var divot2 = package.PackedItemDivots.AddNew();
			divot2.KI_PackedQty = 2;
			divot2.KI_ParentTableCode = "WZ";
			divot2.KI_ParentID = newPickLine1.PK;

			var divot3 = package.PackedItemDivots.AddNew();
			divot3.KI_PackedQty = 1;
			divot3.KI_ParentTableCode = "WZ";
			divot3.KI_ParentID = pickLine2.PK;

			Factory.Save();

			var result = PackageHelper.GetPackageRelatedProductInfos(Factory, package);

			AssertEquals("Should be 2 package info returned", 2, result.Length);

			var resultDivot1 = result.Single(l => l.ProductPK == data.Part1.PK);
			var resultDivot2 = result.Single(l => l.ProductPK == data.Part2.PK);

			AssertEquals("Product Pk should be same", data.Part1.PK, resultDivot1.ProductPK);
			AssertEquals("Product Code should be same", "P1", resultDivot1.ProductCode);
			AssertEquals("Product Description should be same", "DESCRIPTION1", resultDivot1.ProductDescription);
			AssertEquals("Expected Quantity should be same", 5m, resultDivot1.ExpectedQty);

			AssertEquals("Product Pk should be same", data.Part2.PK, resultDivot2.ProductPK);
			AssertEquals("Product Code should be same", "P2", resultDivot2.ProductCode);
			AssertEquals("Product Description should be same", "DESCRIPTION2", resultDivot2.ProductDescription);
			AssertEquals("Expected Quantity should be same", 1m, resultDivot2.ExpectedQty);
		}

		#endregion

		#region TestGetPackageParentOrders

		public void TestGetPackageParentOrders()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 20m);
			Helper.Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 10m);

			Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg1";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order1.FinaliseDocketWithoutUserConfirmation();
			pick1.FinalisePick();
			Helper.Factory.Save();

			var result = PackageHelper.GetPackageParentOrdersWithNoFinalisedReturnReceive(Helper.Factory, "Pkg1");
			AssertEquals("1 package parent orders.", 1, result.Count);

			var packageParentOrderInfo1 = result.Single();
			AssertEquals("Package parent order info details are correct.", "O1", (ZString)packageParentOrderInfo1[WhsDocketSchema.WD_ExternalReference]);
			AssertEquals("Package parent order info details are correct.", data.Org1.OH_Code, (ZString)packageParentOrderInfo1[OrgHeaderSchema.OH_Code]);
			AssertEquals("Package parent order info details are correct.", package1.PK, (ZGuid)packageParentOrderInfo1[PkgPackageSchema.PK]);
		}

		public void TestGetPackageParentOrders_PackageWithUnfinalisedReturnReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 30m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 20m);
			Helper.Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 30m);

			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg1";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order1.FinaliseDocketWithoutUserConfirmation();
			pick1.FinalisePick();
			order2.FinaliseDocketWithoutUserConfirmation();
			pick2.FinalisePick();
			Helper.Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			returnReceive.WD_ExternalReference = order1.WD_ExternalReference;
			returnReceive.WD_WD_ParentDocket = order1.PK;
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 30m);

			var returnReference = returnReceive.References.AddNew();
			returnReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.Other;
			returnReference.WX_Reference = package1.KP_PackageID;
			Helper.Factory.Save();

			AssertEquals("Receive is not finalised.", false, returnReceive.IsFinalised);

			var result = PackageHelper.GetPackageParentOrdersWithNoFinalisedReturnReceive(Helper.Factory, "Pkg1");
			AssertEquals("2 package parent orders.", 2, result.Count);

			var packageParentOrderInfo1 = result.Where(info => (ZString)info[WhsDocketSchema.WD_ExternalReference] == "O1").Single();
			AssertEquals("Package parent order info details are correct.", data.Org1.OH_Code, (ZString)packageParentOrderInfo1[OrgHeaderSchema.OH_Code]);
			AssertEquals("Package parent order info details are correct.", package1.PK, (ZGuid)packageParentOrderInfo1[PkgPackageSchema.PK]);

			var packageParentOrderInfo2 = result.Where(info => (ZString)info[WhsDocketSchema.WD_ExternalReference] == "O2").Single();
			AssertEquals("Package parent order info details are correct.", data.Org1.OH_Code, (ZString)packageParentOrderInfo2[OrgHeaderSchema.OH_Code]);
			AssertEquals("Package parent order info details are correct.", package2.PK, (ZGuid)packageParentOrderInfo2[PkgPackageSchema.PK]);
		}

		public void TestGetPackageParentOrders_PackageWithFinalisedReturnReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 30m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 20m);
			Helper.Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = "Pkg1";
			package1.Pack(orderLine1.ReleaseLines[0], 30m);

			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg1";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order1.FinaliseDocketWithoutUserConfirmation();
			pick1.FinalisePick();
			order2.FinaliseDocketWithoutUserConfirmation();
			pick2.FinalisePick();
			Helper.Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			returnReceive.WD_ExternalReference = order1.WD_ExternalReference;
			returnReceive.WD_WD_ParentDocket = order1.PK;
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 30m);

			var returnReference = returnReceive.References.AddNew();
			returnReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.Other;
			returnReference.WX_Reference = package1.KP_PackageID;
			Helper.Factory.Save();

			returnReceive.AllocateLocationsWithMock();
			returnReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertEquals("Receive is finalised.", true, returnReceive.IsFinalised);

			var result = PackageHelper.GetPackageParentOrdersWithNoFinalisedReturnReceive(Helper.Factory, "Pkg1");
			AssertEquals("1 package parent orders.", 1, result.Count);

			var packageParentOrderInfo1 = result.Single();
			AssertEquals("Package parent order info details are correct.", "O2", (ZString)packageParentOrderInfo1[WhsDocketSchema.WD_ExternalReference]);
			AssertEquals("Package parent order info details are correct.", data.Org1.OH_Code, (ZString)packageParentOrderInfo1[OrgHeaderSchema.OH_Code]);
			AssertEquals("Package parent order info details are correct.", package2.PK, (ZGuid)packageParentOrderInfo1[PkgPackageSchema.PK]);
		}

		public void TestGetPackageParentOrders_PackageWithFinalisedReturnReceive_MaxPackageIdLength()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 30m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 20m);
			Helper.Factory.Save();

			AssertEquals("Precondition: package id max length is 46. Update this test if max length is updated.", 46, PkgPackageHeaderSchema.KPH_PackageID.MaxLength);
			AssertEquals("Precondition: reference max length is 25. Update this test if max length is updated.", 25, WhsDocketReferenceSchema.WX_Reference.MaxLength);

			var maxLengthPackageId = "1234567890123456789012345678901234567890123456";
			var pick1 = Helper.CreatePickNew(order1);
			var package1 = order1.PackageJob.Packages.AddNew();
			package1.KP_PackageID = maxLengthPackageId;
			package1.Pack(orderLine1.ReleaseLines[0], 30m);

			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = maxLengthPackageId;
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order1.FinaliseDocketWithoutUserConfirmation();
			pick1.FinalisePick();
			order2.FinaliseDocketWithoutUserConfirmation();
			pick2.FinalisePick();
			Helper.Factory.Save();

			var returnReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			returnReceive.WD_ExternalReference = order1.WD_ExternalReference;
			returnReceive.WD_WD_ParentDocket = order1.PK;
			returnReceive.WD_DocketSubType = ReceiveType.Codes.Returns;
			Helper.CreateWhsReceiveLine(returnReceive, data.Part1, 30m);

			var returnReference = returnReceive.References.AddNew();
			returnReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.Other;
			returnReference.WX_Reference = package1.KP_PackageID.Substring(package1.KP_PackageID.Length - WhsDocketReferenceSchema.WX_Reference.MaxLength);
			Helper.Factory.Save();

			returnReceive.AllocateLocationsWithMock();
			returnReceive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			AssertEquals("Package id has max length.", PkgPackageHeaderSchema.KPH_PackageID.MaxLength, package1.KP_PackageID.Length);
			AssertEquals("Package id has max length.", PkgPackageHeaderSchema.KPH_PackageID.MaxLength, package2.KP_PackageID.Length);
			AssertEquals("Receive is finalised.", true, returnReceive.IsFinalised);

			var result = PackageHelper.GetPackageParentOrdersWithNoFinalisedReturnReceive(Helper.Factory, "1234567890123456789012345678901234567890123456");
			AssertEquals("1 package parent orders.", 1, result.Count);

			var packageParentOrderInfo1 = result.Single();
			AssertEquals("Package parent order info details are correct.", "O2", (ZString)packageParentOrderInfo1[WhsDocketSchema.WD_ExternalReference]);
			AssertEquals("Package parent order info details are correct.", data.Org1.OH_Code, (ZString)packageParentOrderInfo1[OrgHeaderSchema.OH_Code]);
			AssertEquals("Package parent order info details are correct.", package2.PK, (ZGuid)packageParentOrderInfo1[PkgPackageSchema.PK]);
		}

		public void TestGetPackageParentOrders_OrderWithPackagesWithFinalisedAndUnfinalisedReturnReceive()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 30m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 30m);
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 10m);

			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part2, 20m);
			Helper.Factory.Save();

			var pick1 = Helper.CreatePickNew(order1);
			var package1a = order1.PackageJob.Packages.AddNew();
			package1a.KP_PackageID = "Pkg1";
			package1a.Pack(order1Line1.ReleaseLines[0], 30m);

			var package1b = order1.PackageJob.Packages.AddNew();
			package1b.KP_PackageID = "Pkg2";
			package1b.Pack(order1Line2.ReleaseLines[0], 30m);

			var pick2 = Helper.CreatePickNew(order2);
			var package2 = order2.PackageJob.Packages.AddNew();
			package2.KP_PackageID = "Pkg1";
			package2.Pack(orderLine2.ReleaseLines[0], 20m);
			Helper.Factory.Save();

			order1.FinaliseDocketWithoutUserConfirmation();
			pick1.FinalisePick();
			order2.FinaliseDocketWithoutUserConfirmation();
			pick2.FinalisePick();
			Helper.Factory.Save();

			var returnReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			returnReceive1.WD_ExternalReference = order1.WD_ExternalReference;
			returnReceive1.WD_ExternalReferenceSplit = 0;
			returnReceive1.WD_WD_ParentDocket = order1.PK;
			returnReceive1.WD_DocketSubType = ReceiveType.Codes.Returns;
			Helper.CreateWhsReceiveLine(returnReceive1, data.Part1, 30m);

			var returnReference = returnReceive1.References.AddNew();
			returnReference.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.Other;
			returnReference.WX_Reference = package1a.KP_PackageID;

			returnReceive1.AllocateLocationsWithMock();
			returnReceive1.FinaliseDocketWithoutUserConfirmation();

			var returnReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			returnReceive2.WD_ExternalReference = order1.WD_ExternalReference;
			returnReceive2.WD_ExternalReferenceSplit = 1;
			returnReceive2.WD_WD_ParentDocket = order1.PK;
			returnReceive2.WD_DocketSubType = ReceiveType.Codes.Returns;
			Helper.CreateWhsReceiveLine(returnReceive2, data.Part1, 30m);

			var returnReference2 = returnReceive2.References.AddNew();
			returnReference2.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.Other;
			returnReference2.WX_Reference = package1b.KP_PackageID;
			Helper.Factory.Save();

			AssertEquals("Receive is finalised.", true, returnReceive1.IsFinalised);
			AssertEquals("Receive is not finalised.", false, returnReceive2.IsFinalised);

			var result = PackageHelper.GetPackageParentOrdersWithNoFinalisedReturnReceive(Helper.Factory, "Pkg1");
			AssertEquals("1 package parent orders.", 1, result.Count);

			var packageParentOrderInfo1 = result.Single();
			AssertEquals("Package parent order info details are correct.", "O2", (ZString)packageParentOrderInfo1[WhsDocketSchema.WD_ExternalReference]);
			AssertEquals("Package parent order info details are correct.", data.Org1.OH_Code, (ZString)packageParentOrderInfo1[OrgHeaderSchema.OH_Code]);
			AssertEquals("Package parent order info details are correct.", package2.PK, (ZGuid)packageParentOrderInfo1[PkgPackageSchema.PK]);
		}

		#endregion

		#region TestGetPackageRelatedProductInfosWithAttributes

		public void TestGetPackageRelatedProductInfosWithAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, true);
			Helper.Factory.Save();

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, today.AddYears(1), today, "PA1", "PA2", "PA3", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, today.AddYears(2), today, "PB1", "PB2", "PB3", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 5m, today.AddYears(1), today, "PA1", "PA2", "PA3", "", "");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, today.AddYears(2), today, "PB1", "PB2", "PB3", "", "");

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 3m);
			package.Pack(orderLine2.ReleaseLines[0], 7m);
			Helper.Factory.Save();

			var result = PackageHelper.GetPackageRelatedProductInfosWithAttributes(Helper.Factory, package.PK);
			AssertEquals("", 2, result.Count);

			var packageRelatedProductInfo1 = result.Where(info => (ZDecimal)info[PkgPackageItemDivotSchema.KI_PackedQty] == 3m).Single();
			AssertEquals("Package related product info details are correct.", "PA1", (ZString)packageRelatedProductInfo1[WhsDocketLineSchema.WE_PartAttrib1]);
			AssertEquals("Package related product info details are correct.", "PA2", (ZString)packageRelatedProductInfo1[WhsDocketLineSchema.WE_PartAttrib2]);
			AssertEquals("Package related product info details are correct.", "PA3", (ZString)packageRelatedProductInfo1[WhsDocketLineSchema.WE_PartAttrib3]);
			AssertEquals("Package related product info details are correct.", today, (ZDateTime)packageRelatedProductInfo1[WhsDocketLineSchema.WE_PackingDate]);
			AssertEquals("Package related product info details are correct.", today.AddYears(1), (ZDateTime)packageRelatedProductInfo1[WhsDocketLineSchema.WE_ExpiryDate]);
			AssertEquals("Package related product info details are correct.", data.Part1.PK, (ZGuid)packageRelatedProductInfo1[WhsDocketLineSchema.WE_OP]);

			var packageRelatedProductInfo2 = result.Where(info => (ZDecimal)info[PkgPackageItemDivotSchema.KI_PackedQty] == 7m).Single();
			AssertEquals("Package related product info details are correct.", "PB1", (ZString)packageRelatedProductInfo2[WhsDocketLineSchema.WE_PartAttrib1]);
			AssertEquals("Package related product info details are correct.", "PB2", (ZString)packageRelatedProductInfo2[WhsDocketLineSchema.WE_PartAttrib2]);
			AssertEquals("Package related product info details are correct.", "PB3", (ZString)packageRelatedProductInfo2[WhsDocketLineSchema.WE_PartAttrib3]);
			AssertEquals("Package related product info details are correct.", today, (ZDateTime)packageRelatedProductInfo2[WhsDocketLineSchema.WE_PackingDate]);
			AssertEquals("Package related product info details are correct.", today.AddYears(2), (ZDateTime)packageRelatedProductInfo2[WhsDocketLineSchema.WE_ExpiryDate]);
			AssertEquals("Package related product info details are correct.", data.Part1.PK, (ZGuid)packageRelatedProductInfo2[WhsDocketLineSchema.WE_OP]);
		}

		public void TestGetPackageRelatedProductInfosWithAttributes_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.Factory.Save();

			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN1", "");
			Helper.CreateWhsReceiveLine(receive, data.Part1.PK, 1m, ZGuid.Empty, "", ZDate.Empty, ZDate.Empty, "", "", "", "SN2", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine1.WE_SerialNumber = "SN1";
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			orderLine2.WE_SerialNumber = "SN2";

			var pick = Helper.CreatePickNew(order);
			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine1.ReleaseLines[0], 1m);
			package.Pack(orderLine2.ReleaseLines[0], 1m);
			Helper.Factory.Save();

			var result = PackageHelper.GetPackageRelatedProductInfosWithAttributes(Helper.Factory, package.PK);
			AssertEquals("There should be 2 results.", 2, result.Count);

			var packageRelatedProductInfo1 = result.Where(info => (ZString)info[WhsDocketLineSchema.WE_SerialNumber] == "SN1").Single();
			AssertEquals("Package related product info details are correct.", data.Part1.PK, (ZGuid)packageRelatedProductInfo1[WhsDocketLineSchema.WE_OP]);
			AssertEquals("Package related product info details are correct.", 1m, (ZDecimal)packageRelatedProductInfo1[PkgPackageItemDivotSchema.KI_PackedQty]);

			var packageRelatedProductInfo2 = result.Where(info => (ZString)info[WhsDocketLineSchema.WE_SerialNumber] == "SN2").Single();
			AssertEquals("Package related product info details are correct.", data.Part1.PK, (ZGuid)packageRelatedProductInfo2[WhsDocketLineSchema.WE_OP]);
			AssertEquals("Package related product info details are correct.", 1m, (ZDecimal)packageRelatedProductInfo2[PkgPackageItemDivotSchema.KI_PackedQty]);
		}

		public void TestGetPackageRelatedProductInfosWithAttributes_WithReleaseCapturedAttributes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, true, setReleaseCaptured: true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, true, setReleaseCaptured: true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m);
			var inventoryLine = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 20);
			Helper.CreatePickNew(order);
			var pickLine = Helper.CreateWhsPickLine(orderLine, inventoryLine, 10);

			var package = order.PackageJob.Packages.AddNew();

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "A1";
			releaseLine1.PartAttribute2 = "B1";
			releaseLine1.PartAttribute3 = "C1";
			releaseLine1.Quantity = 5m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.PartAttribute1 = "A2";
			releaseLine2.PartAttribute2 = "B2";
			releaseLine2.PartAttribute3 = "C2";
			releaseLine2.Quantity = 5m;

			var pickLine1 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "A1");
			var pickLine2 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedPartAttrib1 == "A2");

			var divot1 = package.PackedItemDivots.AddNew();
			divot1.KI_PackedQty = 2;
			divot1.KI_ParentTableCode = "WZ";
			divot1.KI_ParentID = pickLine1.PK;

			var divot2 = package.PackedItemDivots.AddNew();
			divot2.KI_PackedQty = 1;
			divot2.KI_ParentTableCode = "WZ";
			divot2.KI_ParentID = pickLine2.PK;

			Helper.Factory.Save();

			var result = PackageHelper.GetPackageRelatedProductInfosWithAttributes(Helper.Factory, package.PK);

			var packageRelatedProductInfo = result.Single();
			AssertEquals("Package related product info details are correct.", 3m, (ZDecimal)packageRelatedProductInfo[PkgPackageItemDivotSchema.KI_PackedQty]);
			AssertEquals("Package related product info details are correct.", string.Empty, (ZString)packageRelatedProductInfo[WhsDocketLineSchema.WE_PartAttrib1]);
			AssertEquals("Package related product info details are correct.", string.Empty, (ZString)packageRelatedProductInfo[WhsDocketLineSchema.WE_PartAttrib2]);
			AssertEquals("Package related product info details are correct.", string.Empty, (ZString)packageRelatedProductInfo[WhsDocketLineSchema.WE_PartAttrib3]);
			AssertEquals("Package related product info details are correct.", ZDate.Empty, (ZDateTime)packageRelatedProductInfo[WhsDocketLineSchema.WE_PackingDate]);
			AssertEquals("Package related product info details are correct.", ZDate.Empty, (ZDateTime)packageRelatedProductInfo[WhsDocketLineSchema.WE_ExpiryDate]);
			AssertEquals("Package related product info details are correct.", data.Part1.PK, (ZGuid)packageRelatedProductInfo[WhsDocketLineSchema.WE_OP]);
		}

		public void TestGetPackageRelatedProductInfosWithAttributes_WithReleaseCapturedAttributes_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true, setReleaseCaptured: true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 5m);
			var inventoryLine = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 2m);
			Helper.CreatePickNew(order);
			var pickLine1 = Helper.CreateWhsPickLine(orderLine, inventoryLine, 1m);
			var pickLine2 = Helper.CreateWhsPickLine(orderLine, inventoryLine, 1m);
			var package = order.PackageJob.Packages.AddNew();

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.SerialNumber = "SN1";
			releaseLine1.Quantity = 1m;

			var releaseLine2 = orderLine.ReleaseLines.AddNew();
			releaseLine2.SerialNumber = "SN2";
			releaseLine2.Quantity = 1m;

			var newPickLine1 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedSerialNumber == "SN1");
			var newPickLine2 = orderLine.PickLines.Single(l => l.WZ_ReleaseCapturedSerialNumber == "SN2");

			var divot1 = package.PackedItemDivots.AddNew();
			divot1.KI_PackedQty = 1m;
			divot1.KI_ParentTableCode = "WZ";
			divot1.KI_ParentID = newPickLine1.PK;

			var divot2 = package.PackedItemDivots.AddNew();
			divot2.KI_PackedQty = 1;
			divot2.KI_ParentTableCode = "WZ";
			divot2.KI_ParentID = newPickLine2.PK;

			Helper.Factory.Save();

			var result = PackageHelper.GetPackageRelatedProductInfosWithAttributes(Helper.Factory, package.PK).Single();
			AssertEquals("Package related product info details are correct.", data.Part1.PK, (ZGuid)result[WhsDocketLineSchema.WE_OP]);
			AssertEquals("Package related product info details are correct.", 2m, (ZDecimal)result[PkgPackageItemDivotSchema.KI_PackedQty]);
			AssertEquals("Package related product info details are correct.", "", (ZString)result[WhsDocketLineSchema.WE_SerialNumber]);
		}

		public void TestGetPackageRelatedProductInfosWithAttributes_PickLineAndReleaseCapturedAttribute()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 5m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsPickableDocketLine(order, data.Part1, 15);
			var pick = Helper.CreatePickNew(order);

			var pickLine1 = orderLine.PickLines.Single(pickLine => pickLine.WZ_Units == 10);

			var package = order.PackageJob.Packages.AddNew();

			var releaseLine1 = orderLine.ReleaseLines[0];
			releaseLine1.PartAttribute1 = "PA1";
			releaseLine1.Quantity = 10m;

			var divot1 = package.PackedItemDivots.AddNew();
			divot1.KI_PackedQty = 10;
			divot1.KI_ParentTableCode = "WZ";
			divot1.KI_ParentID = pickLine1.PK;

			var pickLine2 = orderLine.PickLines.Single(pickLine => pickLine.WZ_Units == 5);
			var divot2 = package.PackedItemDivots.AddNew();
			divot2.KI_PackedQty = 5;
			divot2.KI_ParentTableCode = "WZ";
			divot2.KI_ParentID = pickLine2.PK;

			Helper.Factory.Save();

			var result = PackageHelper.GetPackageRelatedProductInfosWithAttributes(Helper.Factory, package.PK);
			AssertEquals("There should just be 1 result.", 1, result.Count);

			var packageRelatedProductInfo = result.Single();
			AssertEquals("Package related product info details are correct.", 15m, (ZDecimal)packageRelatedProductInfo[PkgPackageItemDivotSchema.KI_PackedQty]);
			AssertEquals("Package related product info details are correct.", "", (ZString)packageRelatedProductInfo[WhsDocketLineSchema.WE_PartAttrib1]);
			AssertEquals("Package related product info details are correct.", data.Part1.PK, (ZGuid)packageRelatedProductInfo[WhsDocketLineSchema.WE_OP]);
		}

		#endregion

		#region TestGetDocketPackageIdReference

		public void TestGetDocketPackageIdReference()
		{
			var packageIdWithMaxLength = "1234567890123456789012345678901234567890123456";
			var packageIdWithMaxDocketReferenceLength = "1234567890123456789012345";
			var packageId = "1234567890";

			AssertEquals("Precondition: package id max length is 46. Update this test if max length is updated.", 46, PkgPackageHeaderSchema.KPH_PackageID.MaxLength);
			AssertEquals("Precondition: reference max length is 25. Update this test if max length is updated.", 25, WhsDocketReferenceSchema.WX_Reference.MaxLength);
			AssertEquals("Precondition: package id has max length.", PkgPackageHeaderSchema.KPH_PackageID.MaxLength, packageIdWithMaxLength.Length);

			AssertEquals("Same package id is returned.", packageIdWithMaxDocketReferenceLength, PackageHelper.GetDocketPackageIdReference(packageIdWithMaxDocketReferenceLength));
			AssertEquals("Same package id is returned.", packageId, PackageHelper.GetDocketPackageIdReference(packageId));
			AssertEquals($"Part of the package id is returned, the last {WhsDocketReferenceSchema.WX_Reference.MaxLength} characters are returned.", "2345678901234567890123456", PackageHelper.GetDocketPackageIdReference(packageIdWithMaxLength));
		}

		#endregion

		#region TestGetOrderUnpackedProductInfos

		public void TestGetOrderUnpackedProductInfos()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Desc = "DESCRIPTION1";
			data.Part1.OP_Weight = 2.54m;
			data.Part1.OP_WeightUQ = "KG";
			data.Part2.OP_Desc = "DESCRIPTION2";
			data.Part2.OP_Weight = 3.78m;
			data.Part2.OP_WeightUQ = "LB";
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 15m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part2, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 15m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 10m);
			var orderLine4 = Helper.CreateWhsOrderLine(order, data.Part2, 15m);
			Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package1 = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 6m);

			var package2 = packingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 8m);

			var package3 = packingHelper.CreatePackage(packageJob, "PKG3", 1, PkgUnit.Box);
			package3.Pack(orderLine3.ReleaseLines[0], 10m);

			var package4 = packingHelper.CreatePackage(packageJob, "PKG4", 1, PkgUnit.Box);
			package4.Pack(orderLine4.ReleaseLines[0], 12m);
			Factory.Save();

			var result = PackageHelper.GetOrderUnpackedProductInfos(Factory, order.PK);

			AssertEquals("Should be 2 package infos returned", 2, result.Length);

			var resultPart1 = result.Single(l => l.ProductPK == data.Part1.PK);
			AssertEquals("Product Code should be correct", "P1", resultPart1.ProductCode);
			AssertEquals("Product Description should be correct", "DESCRIPTION1", resultPart1.ProductDescription);
			AssertEquals("Expected Quantity should be correct", 11m, resultPart1.ExpectedQty);
			AssertEquals("Product Weight should be correct", 2.54m, resultPart1.ProductWeight);
			AssertEquals("Product Weight UQ should be correct", "KG", resultPart1.ProductWeightUQ);

			var resultPart2 = result.Single(l => l.ProductPK == data.Part2.PK);
			AssertEquals("Product Code should be correct", "P2", resultPart2.ProductCode);
			AssertEquals("Product Description should be correct", "DESCRIPTION2", resultPart2.ProductDescription);
			AssertEquals("Expected Quantity should be correct", 3m, resultPart2.ExpectedQty);
			AssertEquals("Product Weight should be correct", 3.78m, resultPart2.ProductWeight);
			AssertEquals("Product Weight UQ should be correct", "LB", resultPart2.ProductWeightUQ);
		}

		#endregion

		#region TestGetOrderUnpackedProductInfos_PickByBOM

		public void TestGetOrderUnpackedProductInfos_PickByBOM()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			bike.OP_Desc = "AWESOME BIKE";
			bike.OP_Weight = 2.55m;
			bike.OP_WeightUQ = "KG";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package1 = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 3m);

			Factory.Save();

			var result = PackageHelper.GetOrderUnpackedProductInfos(Factory, order.PK);

			AssertEquals("Should be 1 package info returned", 1, result.Length);

			var resultPart1 = result.Single(l => l.ProductPK == bike.PK);
			AssertEquals("Product Code should be correct", "BIKE", resultPart1.ProductCode);
			AssertEquals("Product Description should be correct", "AWESOME BIKE", resultPart1.ProductDescription);
			AssertEquals("Expected Quantity should be correct", 2m, resultPart1.ExpectedQty);
			AssertEquals("Product Weight should be correct", 2.55m, resultPart1.ProductWeight);
			AssertEquals("Product Weight UQ should be correct", "KG", resultPart1.ProductWeightUQ);
		}

		#endregion

		#region TestGetOrderUnpackedProductInfos_PickByBOM_WithOrderedComponents

		public void TestGetOrderUnpackedProductInfos_PickByBOM_WithOrderedComponents()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			bike.OP_Desc = "AWESOME BIKE";
			bike.OP_Weight = 2.55m;
			bike.OP_WeightUQ = "KG";

			wheel.OP_Desc = "BIG WHEELS";
			wheel.OP_Weight = 1.1m;
			wheel.OP_WeightUQ = "LB";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, wheel, 20m);
			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package1 = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 3m);

			var package2 = packingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 8m);

			Factory.Save();

			var result = PackageHelper.GetOrderUnpackedProductInfos(Factory, order.PK);

			AssertEquals("Should be 2 package info returned", 2, result.Length);

			var resultPart1 = result.Single(l => l.ProductPK == bike.PK);
			AssertEquals("Product Code should be correct", "BIKE", resultPart1.ProductCode);
			AssertEquals("Product Description should be correct", "AWESOME BIKE", resultPart1.ProductDescription);
			AssertEquals("Expected Quantity should be correct", 2m, resultPart1.ExpectedQty);
			AssertEquals("Product Weight should be correct", 2.55m, resultPart1.ProductWeight);
			AssertEquals("Product Weight UQ should be correct", "KG", resultPart1.ProductWeightUQ);

			var resultPart2 = result.Single(l => l.ProductPK == wheel.PK);
			AssertEquals("Product Code should be correct", "WHEEL", resultPart2.ProductCode);
			AssertEquals("Product Description should be correct", "BIG WHEELS", resultPart2.ProductDescription);
			AssertEquals("Expected Quantity should be correct", 12m, resultPart2.ExpectedQty);
			AssertEquals("Product Weight should be correct", 1.1m, resultPart2.ProductWeight);
			AssertEquals("Product Weight UQ should be correct", "LB", resultPart2.ProductWeightUQ);
		}

		#endregion

		#region TestGetOrderUnpackedProductInfos_PickByBOM_TwoKindOfKits

		public void TestGetOrderUnpackedProductInfos_PickByBOM_TwoKindOfKits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var kit2 = Helper.CreateProduct(data.Org1, "KIT2");
			kit2.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(kit2, wheel, 3m, "UNT");

			bike.OP_Desc = "AWESOME BIKE";
			bike.OP_Weight = 2.55m;
			bike.OP_WeightUQ = "KG";

			kit2.OP_Desc = "KIT2DESC";
			kit2.OP_Weight = 3.6m;
			kit2.OP_WeightUQ = "LB";

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, kit2, 3m);
			var pick = Helper.CreatePickNew(order);

			var packingHelper = new PackingTestHelper(Factory);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			var package1 = packingHelper.CreatePackage(packageJob, "PKG1", 1, PkgUnit.Box);
			package1.Pack(orderLine1.ReleaseLines[0], 3m);

			var package2 = packingHelper.CreatePackage(packageJob, "PKG2", 1, PkgUnit.Box);
			package2.Pack(orderLine2.ReleaseLines[0], 2m);

			Factory.Save();

			var result = PackageHelper.GetOrderUnpackedProductInfos(Factory, order.PK);

			AssertEquals("Should be 2 package info returned", 2, result.Length);

			var resultPart1 = result.Single(l => l.ProductPK == bike.PK);
			AssertEquals("Product Code should be correct", "BIKE", resultPart1.ProductCode);
			AssertEquals("Product Description should be correct", "AWESOME BIKE", resultPart1.ProductDescription);
			AssertEquals("Expected Quantity should be correct", 2m, resultPart1.ExpectedQty);
			AssertEquals("Product Weight should be correct", 2.55m, resultPart1.ProductWeight);
			AssertEquals("Product Weight UQ should be correct", "KG", resultPart1.ProductWeightUQ);

			var resultPart2 = result.Single(l => l.ProductPK == kit2.PK);
			AssertEquals("Product Code should be correct", "KIT2", resultPart2.ProductCode);
			AssertEquals("Product Description should be correct", "KIT2DESC", resultPart2.ProductDescription);
			AssertEquals("Expected Quantity should be correct", 1m, resultPart2.ExpectedQty);
			AssertEquals("Product Weight should be correct", 3.6m, resultPart2.ProductWeight);
			AssertEquals("Product Weight UQ should be correct", "LB", resultPart2.ProductWeightUQ);
		}

		#endregion

		public void TestGetPackageRelatedProductInfos_MultiplePickLinesHaveSameAttributeButFromMultipleTypeInventories()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.NonMandatory, "Attr1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");

			var receiveForComponents = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part1, 30m);
			Helper.CreateWhsReceiveInventoryLine(receiveForComponents, data.Part2, 20m);
			receiveForComponents.AllocateLocationsWithMock();
			receiveForComponents.FinaliseDocketWithoutUserConfirmation();

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			part3.OP_IsComponentPickedOnSalesOrder = true;
			Helper.CreateProductBOM(part3, data.Part1, 1m, "UNT");
			Helper.CreateProductBOM(part3, data.Part2, 1m, "UNT");

			var receiveForPart3 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			Helper.CreateWhsReceiveInventoryLine(receiveForPart3, part3, 1m);
			receiveForPart3.AllocateLocationsWithMock();
			receiveForPart3.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();

			var workOrder1 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "work order 1");
			var workOrderLine1 = Helper.CreateWhsWorkOrderLine(workOrder1, part3, 1m);
			var workOrder2 = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1, "work order 2");
			var workOrderLine2 = Helper.CreateWhsWorkOrderLine(workOrder2, part3, 1m);

			Helper.CreatePickNew(workOrder1);
			workOrder1.FinaliseDocketAlwaysFinalisingPick();

			Helper.CreatePickNew(workOrder2);
			workOrder2.FinaliseDocketAlwaysFinalisingPick();

			workOrder1.Receive.FinaliseDocketWithoutUserConfirmation();
			workOrder2.Receive.FinaliseDocketWithoutUserConfirmation();

			Helper.Factory.Save();

			var pick = Helper.Factory.New<WhsPick>();
			pick.WP_WW_Whs = data.Whs1.PK;
			pick.WP_WL_DockDoor = data.Whs1.WW_DefaultInboundDockDoor;
			pick.WP_PickOption = WhsPickOption.Codes.Manual;
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "PickFinalized1", pickOption: WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, part3, 4m);
			pick.AddOrders(new[] { order });
			pick.AutoAllocateItemsWithMock();
			order.FinaliseDocketWithoutUserConfirmation();
			pick.FinalisePick();

			var package = order.PackageJob.Packages.AddNew();
			package.KP_PackageID = "Pkg1";
			package.Pack(orderLine.ReleaseLines[0], 5m);
			Helper.Factory.Save();

			var dynamicBusinessObjectCollection = PackageHelper.GetPackageRelatedProductInfosWithAttributes(Factory, package.PK);
			AssertEquals("Should contain 3 receive lines", 3, dynamicBusinessObjectCollection.Count);

			var returnReceive = Factory.New<WhsReceive>();
			foreach (DynamicBusinessObject bo in dynamicBusinessObjectCollection)
			{
				var line = returnReceive.Lines.AddNew();
				line.WE_OP = (ZGuid)bo[WhsDocketLineSchema.WE_OP];
				line.WE_ClientOrderedUnits = (ZDecimal)bo[PkgPackageItemDivotSchema.KI_PackedQty];
				line.WE_PartAttrib1 = (ZString)bo[WhsDocketLineSchema.WE_PartAttrib1];
				line.WE_PartAttrib2 = (ZString)bo[WhsDocketLineSchema.WE_PartAttrib2];
				line.WE_PartAttrib3 = (ZString)bo[WhsDocketLineSchema.WE_PartAttrib3];
				line.WE_SerialNumber = (ZString)bo[WhsDocketLineSchema.WE_SerialNumber];
				line.WE_PackingDate = ((ZDateTime)bo[WhsDocketLineSchema.WE_PackingDate]).Date;
				line.WE_ExpiryDate = ((ZDateTime)bo[WhsDocketLineSchema.WE_ExpiryDate]).Date;

				var workOrderInventoryPK = (ZGuid)bo["WorkOrderInventoryPK"];
				if (workOrderInventoryPK.IsValid)
				{
					var originalInventoryWithBOM = Factory.Load<WhsDocketLine>(workOrderInventoryPK);
					WhsRMAHelper.CopyBOMComponentLinks(originalInventoryWithBOM, line);
				}
			}

			var receiveLinesFromPBBAndNormalReceiveLine = returnReceive.Lines.Where(l => l.WE_ClientOrderedUnits == 2 && l.WE_OP == part3.PK && !l.BOMComponentLinks.Any()).ToArray();
			AssertEquals(1, receiveLinesFromPBBAndNormalReceiveLine.Length);

			var expectedComponentsInventoryPKs = receiveForComponents.Lines.Select(l => l.PK).ToList();
			AssertReturnReceiveLines(returnReceive, 1, 2, part3.PK, expectedComponentsInventoryPKs);
		}

		void AssertReturnReceiveLines(WhsDocket returnReceive, int expectedQuantity, int expectedLineCount, ZGuid originalPartPK, List<ZGuid> componentLinePKs)
		{
			var returnReceiveLine = returnReceive.Lines.Where(line => line.WE_ClientOrderedUnits == expectedQuantity && line.WE_OP == originalPartPK && line.BOMComponentLinks.Any()).ToList();
			AssertEquals($"Should be {expectedLineCount} receive line", expectedLineCount, returnReceiveLine.Count);
			for (int i = 0; i < expectedLineCount; i++)
			{
				var receiveLine = returnReceiveLine[i];
				AssertEquals(2, receiveLine.ComponentInventoryLines.Count);
				Assert(receiveLine.BOMComponentLinks.All(link => link.WIP_ComponentQuantity == expectedQuantity));
				Assert(receiveLine.ComponentInventoryLines.All(l => componentLinePKs.Contains(l.PK)));
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = null;
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = null;
		}

		#endregion
	}
}
