using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class WhsTransferHelperTest : WhsTestCaseWithFactory
	{
		#region TestGetOldestMatchingTransfer

		public void TestGetOldestMatchingTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			receive.FinaliseDocket();

			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine.RunPreSaveValidation();

			Factory.Save();

			var result = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer", transfer, result);
		}

		public void TestGetOldestMatchingTransfer_AreaCode()
		{
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory, 3, 1);
			var area1 = Helper.CreateArea(data.Whs1, "AREA1");
			var area2 = Helper.CreateArea(data.Whs1, "AREA2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_WA_PickingArea = area1.PK;
			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA2.WLV_WA_PickingArea = area2.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA2);
			receive.FinaliseDocket();

			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Notify);
			transfer.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-10);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, locationA1, locationA2);
			transferLine1.RunPreSaveValidation();

			var transferWithMatching = Helper.CreateWhsTransfer(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Notify);
			transferWithMatching.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-7);
			var transferLine2 = Helper.CreateWhsTransferLine(transferWithMatching, data.Part1, 50m, locationA2, locationA1);
			transferLine2.RunPreSaveValidation();

			Factory.Save();

			var result = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "AREA1", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer with selected AreaCode", transfer.PK, result.PK);

			var result2 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "AREA2", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer with selected AreaCode", transferWithMatching.PK, result2.PK);
		}

		public void TestGetOldestMatchingTransfer_PickMethod()
		{
			var data = new TestDataSimpleEnvironment_ForFatDatTestsRequiringDataSetup(Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var locationA1 = data.Whs1.FindLocation("A-1");
			locationA1.WLV_PickMethod = "PM1";
			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA2.WLV_PickMethod = "PM2";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Notify);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50m, locationA2);
			receive.FinaliseDocket();

			AssertIsFinalisedPrecondition(receive);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Notify);
			transfer.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-10);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, locationA1, locationA2);
			transferLine1.RunPreSaveValidation();

			var transferWithMatching = Helper.CreateWhsTransfer(data.Org1, data.Whs1, data.GetUniqueNameForFatDat(WhsDocketSchema.WD_ExternalReference), Notify);
			transferWithMatching.WD_BookingDate = ZDateTimeOffset.Today.AddDays(-7);
			var transferLine2 = Helper.CreateWhsTransferLine(transferWithMatching, data.Part1, 50m, locationA2, locationA1);
			transferLine2.RunPreSaveValidation();

			Factory.Save();

			var result1 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "PM1" }, data.Org1.PK, data.Whs1.PK, null, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer with selected PickMethod", transfer.PK, result1.PK);

			var result2 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "PM2" }, data.Org1.PK, data.Whs1.PK, null, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer with selected PickMethod", transferWithMatching.PK, result2.PK);
		}

		public void TestGetOldestMatchingTransfer_Client()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "", false, true);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 50m, locationA1, "", false, true);

			var transferForOrg1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transferForOrg1, data.Part1, 10m, locationA1, locationA2);
			transferForOrg1.RunPreSaveValidation();

			var transferForOrg2 = Helper.CreateWhsTransfer(org2, data.Whs1, "TR2", Notify);
			Helper.CreateWhsTransferLine(transferForOrg2, data.Part1, 10m, locationA1, locationA2);
			transferForOrg2.RunPreSaveValidation();
			Factory.Save();

			var result1 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer with selected client", transferForOrg1.PK, result1.PK);

			var result2 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "PM2" }, org2.PK, data.Whs1.PK, null, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer with selected client", transferForOrg2.PK, result2.PK);
		}

		public void TestGetOldestMatchingTransfer_ExceptedPKs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			Helper.CreateWhsTransferLine(transfer1, data.Part1, 5m, locationA1, locationA2);
			transfer1.RunPreSaveValidation();
			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 5m, locationA1, locationA2);
			transfer2.RunPreSaveValidation();
			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", Notify);
			Helper.CreateWhsTransferLine(transfer3, data.Part1, 5m, locationA1, locationA2);
			transfer3.RunPreSaveValidation();
			Factory.Save();

			var result1 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, new Guid[] { transfer2.PK.ToGuid(), transfer3.PK.ToGuid() }, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer exclude excepted PKs", transfer1.PK, result1.PK);

			var result2 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, new Guid[] { transfer1.PK.ToGuid(), transfer3.PK.ToGuid() }, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer exclude excepted PKs", transfer2.PK, result2.PK);

			var result3 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, new Guid[] { transfer1.PK.ToGuid(), transfer2.PK.ToGuid() }, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer exclude excepted PKs", transfer3.PK, result3.PK);
		}

		public void TestGetOldestMatchingTransfer_Replenishment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 5, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var sourceLocation = data.Whs1.FindLocation("A-1");
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			var destinationLocation1 = data.Whs1.FindLocation("A-3");
			var destinationLocation2 = data.Whs1.FindLocation("A-4");
			var destinationLocation3 = data.Whs1.FindLocation("A-5");

			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 5m, 15m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, sourceLocation, "");

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", ZDateTimeOffset.Today.AddDays(-1), Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, sourceLocation, destinationLocation1);
			transfer1.RunPreSaveValidation();
			var pickLine1 = transferLine1.PickLines[0];
			pickLine1.WZ_GS_NKAssignedTo = staff.GS_Code;

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", ZDateTimeOffset.Today.AddDays(-1), Notify);
			Helper.CreateWhsTransferLine(transfer2, data.Part1, 10m, sourceLocation, pickfaceLocation);
			transfer2.RunPreSaveValidation();
			transfer2.WD_IsPickFaceReplenishment = true;

			var transfer3 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR3", ZDateTimeOffset.Today.AddDays(-1), Notify);
			Helper.CreateWhsTransferLine(transfer3, data.Part1, 10m, sourceLocation, destinationLocation2);
			transfer3.RunPreSaveValidation();

			var transfer4 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR4", ZDateTimeOffset.Today.AddDays(-1), Notify);
			Helper.CreateWhsTransferLine(transfer4, data.Part1, 10m, sourceLocation, destinationLocation3);
			transfer4.RunPreSaveValidation();

			Factory.Save();

			var result1 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, string.Empty, onlyReturnReplenishments: false);
			AssertEquals("System should find right transfer", transfer1.PK, result1.PK);

			var result2 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, string.Empty, onlyReturnReplenishments: true);
			AssertEquals("System should find replenishment transfer first", transfer2.PK, result2.PK);
		}

		public void TestGetOldestMatchingTransfer_Replenishment_PickNo()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var part3 = Helper.CreateProduct(data.Org1, "P3");

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			var pickfaceLocation = data.Whs1.FindLocation("A-4");

			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 0m, 50m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 0m, 50m);
			Helper.CreateProductPickFace(part3, data.Org1, pickfaceLocation, 0m, 50m);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locationA2, "");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, locationA3, "");
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 10m, locationA1, "");
			var receive5 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 10m, locationA2, "");
			var receive6 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", part3, 10m, locationA1, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m, WhsPickOption.Codes.Manual);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_IsAwaitingReplenishment = true;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20m, WhsPickOption.Codes.Manual);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_IsAwaitingReplenishment = true;

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", part3, 10m, WhsPickOption.Codes.Manual);
			var pick3 = Helper.CreatePickNew(order3);
			pick3.WP_IsAwaitingReplenishment = true;

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA1, pickfaceLocation);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA2, pickfaceLocation);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA3, pickfaceLocation);
			transfer1.WD_IsPickFaceReplenishment = true;
			transfer1.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transferLine4 = Helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, locationA1, pickfaceLocation);
			var transferLine5 = Helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, locationA2, pickfaceLocation);
			transfer2.WD_IsPickFaceReplenishment = true;
			transfer2.RunPreSaveValidation();
			Factory.Save();

			var result1 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, pick1.WP_PickNo, onlyReturnReplenishments: true);
			AssertEquals("System should find right replenishment transfer.", transfer1.PK, result1.PK);
			AssertEquals(3, result1.Lines.Count);
			result1.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			result1.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));
			result1.Lines.Single(l => l.PK.Equals(transferLine3.PK.ToGuid()));

			var result2 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, pick2.WP_PickNo, onlyReturnReplenishments: true);
			AssertEquals("System should find right replenishment transfer.", transfer2.PK, result2.PK);
			AssertEquals(2, result2.Lines.Count);
			result2.Lines.Single(l => l.PK.Equals(transferLine4.PK.ToGuid()));
			result2.Lines.Single(l => l.PK.Equals(transferLine5.PK.ToGuid()));

			var result3 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, pick3.WP_PickNo, onlyReturnReplenishments: true);
			AssertNull("There is no right replenishment transfer for this pick.", result3);
		}

		public void TestGetOldestMatchingTransfer_Replenishment_DocketId()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var part3 = Helper.CreateProduct(data.Org1, "P3");

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			var pickfaceLocation = data.Whs1.FindLocation("A-4");

			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 0m, 50m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 0m, 50m);
			Helper.CreateProductPickFace(part3, data.Org1, pickfaceLocation, 0m, 50m);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locationA2, "");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, locationA3, "");
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 10m, locationA1, "");
			var receive5 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 10m, locationA2, "");
			var receive6 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", part3, 10m, locationA1, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m, WhsPickOption.Codes.Manual);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_IsAwaitingReplenishment = true;

			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part2, 20m, WhsPickOption.Codes.Manual);
			var pick2 = Helper.CreatePickNew(order2);
			pick2.WP_IsAwaitingReplenishment = true;

			var order3 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", part3, 10m, WhsPickOption.Codes.Manual);
			var pick3 = Helper.CreatePickNew(order3);
			pick3.WP_IsAwaitingReplenishment = true;

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA1, pickfaceLocation);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA2, pickfaceLocation);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA3, pickfaceLocation);
			transfer1.WD_IsPickFaceReplenishment = true;
			transfer1.RunPreSaveValidation();

			var transfer2 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR2", Notify);
			var transferLine4 = Helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, locationA1, pickfaceLocation);
			var transferLine5 = Helper.CreateWhsTransferLine(transfer2, data.Part2, 10m, locationA2, pickfaceLocation);
			transfer2.WD_IsPickFaceReplenishment = true;
			transfer2.RunPreSaveValidation();
			Factory.Save();

			var result1 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, transfer1.WD_DocketID, onlyReturnReplenishments: true);
			AssertEquals("System should find right replenishment transfer.", transfer1.PK, result1.PK);
			AssertEquals(3, result1.Lines.Count);
			result1.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			result1.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));
			result1.Lines.Single(l => l.PK.Equals(transferLine3.PK.ToGuid()));

			var result2 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, transfer2.WD_DocketID, onlyReturnReplenishments: true);
			AssertEquals("System should find right replenishment transfer.", transfer2.PK, result2.PK);
			AssertEquals(2, result2.Lines.Count);
			result2.Lines.Single(l => l.PK.Equals(transferLine4.PK.ToGuid()));
			result2.Lines.Single(l => l.PK.Equals(transferLine5.PK.ToGuid()));

			var result3 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, order3.WD_DocketID, onlyReturnReplenishments: true);
			AssertNull("No matching transfer found for the order's docket id ", result3);
		}

		public void TestGetOldestMatchingTransfer_NotReturningReplenishment_DocketId()
		{
			var data = new TestDataSimpleEnvironment(Factory, 4, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var part3 = Helper.CreateProduct(data.Org1, "P3");

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var locationA3 = data.Whs1.FindLocation("A-3");
			var pickfaceLocation = data.Whs1.FindLocation("A-4");

			Helper.CreateProductPickFace(data.Part1, data.Org1, pickfaceLocation, 0m, 50m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, pickfaceLocation, 0m, 50m);
			Helper.CreateProductPickFace(part3, data.Org1, pickfaceLocation, 0m, 50m);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locationA2, "");
			var receive3 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, locationA3, "");
			var receive4 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R4", data.Part2, 10m, locationA1, "");
			var receive5 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R5", data.Part2, 10m, locationA2, "");
			var receive6 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R6", part3, 10m, locationA1, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 30m, WhsPickOption.Codes.Manual);
			var pick1 = Helper.CreatePickNew(order1);
			pick1.WP_IsAwaitingReplenishment = false;

			var transfer1 = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA1, pickfaceLocation);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA2, pickfaceLocation);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer1, data.Part1, 10m, locationA3, pickfaceLocation);
			transfer1.WD_IsPickFaceReplenishment = false;
			transfer1.RunPreSaveValidation();

			Factory.Save();

			var result1 = WhsTransferHelper.GetOldestMatchingTransfer(Factory, staff, new SearchFilterCriteriaInfo() { AreaCode = "ANY", PickMethod = "ANY" }, data.Org1.PK, data.Whs1.PK, null, transfer1.WD_DocketID, onlyReturnReplenishments: true);
			AssertNull("No matching transfer found for the transfer's docket id ", result1);
		}

		#endregion

		#region TestSetLinesForAllocateOrPutaway

		public void TestSetLinesForAllocateOrPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "");
			transfer.RunPreSaveValidation();
			var now = ZDateTimeOffset.Now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			Factory.Save();

			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.Entered, transferLine1.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine2.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine3.WE_DocketLineStatus);

			var response = new WhsDocketWebServiceResponse();
			WhsTransferHelper.SetLinesForAllocateOrPutaway(Factory, response, transfer, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, isForcedPutaway: false, ZGuid.Empty);

			AssertEquals("Return lines To Allocate", 1, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			AssertEquals("Should allcate the pick by staff for select line", staff.GS_Code, transferLine1.GS_NKPickedBy);
			AssertEquals("Should allcate the putaway by staff for select line", staff.GS_Code, transferLine1.WE_GS_NKPutawayBy);
		}

		public void TestSetLinesForAllocateOrPutaway_ForcedPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "");
			transfer.RunPreSaveValidation();
			var now = ZDateTimeOffset.Now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			Factory.Save();

			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.Entered, transferLine1.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine2.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine3.WE_DocketLineStatus);

			var response = new WhsDocketWebServiceResponse();
			WhsTransferHelper.SetLinesForAllocateOrPutaway(Factory, response, transfer, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, isForcedPutaway: true, ZGuid.Empty);

			AssertEquals("Return lines already allocated user and destination", 1, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));
		}

		public void TestSetLinesForAllocateOrPutaway_NoLinesToAllocate()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "");
			transfer.RunPreSaveValidation();
			var now = ZDateTimeOffset.Now;
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			Factory.Save();

			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine1.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine2.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine3.WE_DocketLineStatus);

			var response = new WhsDocketWebServiceResponse();
			WhsTransferHelper.SetLinesForAllocateOrPutaway(Factory, response, transfer, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, isForcedPutaway: false, ZGuid.Empty);

			AssertEquals("Return lines already allocated user and destination", 2, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));
		}

		public void TestSetLinesForAllocateOrPutaway_OnlyLinesToPutawayWithoutAllocatede()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "");
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "");
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "");
			transfer.RunPreSaveValidation();
			var now = ZDateTimeOffset.Now;
			transferLine1.PickedTime = now;
			transferLine2.PickedTime = now;
			transferLine3.PickedTime = now;
			Factory.Save();

			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine1.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine2.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.HeldForTransfer, transferLine3.WE_DocketLineStatus);

			var response = new WhsDocketWebServiceResponse();
			WhsTransferHelper.SetLinesForAllocateOrPutaway(Factory, response, transfer, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, isForcedPutaway: false, ZGuid.Empty);

			AssertEquals("Return lines should allocation destination", 3, response.Docket.Lines.Count);
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine1.PK.ToGuid()));
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine2.PK.ToGuid()));
			response.Docket.Lines.Single(l => l.PK.Equals(transferLine3.PK.ToGuid()));
		}

		public void TestSetLinesForAllocateOrPutaway_NoLinesReadyToBePutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, locationA1, "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			var transferLine3 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1, locationA2);
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.Entered, transferLine1.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.Entered, transferLine2.WE_DocketLineStatus);
			AssertEquals("TransferLine should have the right Status", DocketLineStatus.Codes.Entered, transferLine3.WE_DocketLineStatus);

			var response = new WhsDocketWebServiceResponse();
			WhsTransferHelper.SetLinesForAllocateOrPutaway(Factory, response, transfer, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, isForcedPutaway: true, ZGuid.Empty);

			AssertEquals("The Error type should be right", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Should get the right Error message", "None of the lines on this transfer is ready to be putaway. Please pick something first.", response.ErrorMessage);
		}

		#endregion

		public void TestSetLinesForAllocateOrPutaway_PalletsToTransferCompletely()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationA1, "PLT-1");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 10m, locationA1, "PLT-2");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R3", data.Part1, 10m, locationA1, "PLT-3");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "PLT-1", locationA2.ToLocationString(), "");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "PLT-2", locationA2.ToLocationString(), "");
			Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, locationA1.ToLocationString(), "PLT-3", locationA2.ToLocationString(), "");
			transfer.RunPreSaveValidation();
			Factory.Save();

			var response = new WhsDocketWebServiceResponse();
			WhsTransferHelper.SetLinesForAllocateOrPutaway(Factory, response, transfer, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, isForcedPutaway: false, ZGuid.Empty);

			AssertEquals("The Error type should be right", 3, response.Docket.Lines.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-1", "PLT-2", "PLT-3" }, response.Docket.PalletsToTransferCompletely);
		}

		public void TestSetLinesForAllocateOrPutaway_PalletsToTransferCompletely_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			var staff = Helper.CreateGlbStaff("S2", "S2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-2");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultInboundDockDoorLocation, "PLT-3");
			Factory.Save();

			var putawayTransfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			putawayTransfer.WD_IsPutawayTransfer = true;
			var putawayTransferLine1 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, locationA2, "PLT-1", 10m);
			var putawayTransferLine2 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, locationA2, "PLT-2", 10m);
			var putawayTransferLine3 = Helper.SetupTransferLineForDockDoorLocation(putawayTransfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, locationA2, "PLT-3", 10m);
			putawayTransferLine1.PickedTime = ZDateTimeOffset.Now;
			putawayTransferLine2.PickedTime = ZDateTimeOffset.Now;
			putawayTransferLine3.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			var response = new WhsDocketWebServiceResponse();
			WhsTransferHelper.SetLinesForAllocateOrPutaway(Factory, response, putawayTransfer, staff, new SearchFilterCriteriaInfo { AreaCode = "ANY", PickMethod = "ANY" }, isForcedPutaway: false, ZGuid.Empty);

			AssertEquals("The Error type should be right", 3, response.Docket.Lines.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "PLT-1" }, response.Docket.PalletsToTransferCompletely);
		}
	}
}
