using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business.Testing.Reports
{
	public class TransitWarehousePackageOnHandWithUNDGCodeReportTest : WhsTransitTestCaseWithFactory
	{
		#region TestCreateReportDataAndAssert

		public void TestCreateReportDataAndAssert()
		{
			CreateBasicTestData();

			CreateARVPackageAndRelativeEntities();
			CreatePackageOrderReference(packageStateARV, "KPO001");
			CreatePackageHold(packageARV, "DAM");
			CreatePackageHold(packageARV, "LCC");
			CreatePackageHold(packageARV, "HEL");
			CreateHistoryTransfers(packageStateARV, "ARV");
			packageStateARV.WPS_CustomsStatus = "CLR";

			CreateADJPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStateADJ, "ADJ");
			packageStateADJ.WPS_AdjustedOut = "OTH";

			CreateSTAPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStateSTA, "STA");
			CreatePackageHold(packageSTA, "DAM");
			CreatePackageHold(packageSTA, "LCC");
			CreatePackageHold(packageSTA, "HEL", removed: true);
			packageStateSTA.WPS_CustomsStatus = "CUS";

			CreatePUTPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStatePUT, "PUT");
			CreatePackageHold(packagePUT, "LCC");
			packageStatePUT.WPS_CustomsStatus = "PAN";

			CreateBKDPackageAndRelativeEntities();
			CreatePackageHold(packageBKD, "LCC", removed: true);

			CreateDEPPackageAndRelativeEntities();
			CreatePackageHold(packageDEP, "HEL");

			CreateCTTPackageAndRelativeEntities();
			CreatePackageHold(packageCTT, "DAM", removed: true);
			CreateHistoryTransfers(packageStateCTT, "CTT");
			var transferCTT = Helper.CreateTransferHeader("TSF1", warehouse1, false);
			Helper.CreateTransferLine(transferCTT, inBoundLocation, outBoundLocation, packageStateCTT);

			CreatePICPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStatePIC, "PIC");
			var transferPIC = Helper.CreateTransferHeader("TSF2", warehouse1, false);
			Helper.CreateTransferLine(transferPIC, inBoundLocation, outBoundLocation, packageStatePIC, ZDateTime.Now, "RON");

			CreateFLOPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStateFLO, "FLO");

			Factory.Save();

			AssertWithAllAndEmptyFilter();
			AssertIsFumigatedFilter();
			AssertIsTopLoadOnlyFilter();
			AssertIsFumigatedIsTopLoadOnlyFilters();
			AssertIsNonStackableFilter();
			AssertIsHeatTreatedFilter();
			AssertIsISPMPalletFilter();
			AssertIsDamagedFilter();
			AssertIsHeldFilter();
			AssertIsPillagedFilter();
			AssertDocAddressesFilter();
			AssertPackageStatusFilter();
			AssertHighRiskFilters();
			AssertWarehouseFilter();
			AssertAdditionalReferenceFilter();
			AssertArrivalDateFilter();
			AssertPackangeAndConsignmentIDsFilter();
			AssertHoldCodeFilter();
			AssertCustomsStatusFilter();
			AssertRTUIDFilter();
			AssertDLLIDFilter();
			AssertDLLReferenceNumberFilter();
			AssertDTUIDFilter();
			AssertTransferIDFilter();
			AssertPackageOrderReferenceFilter();
			AssertDamagedReasonFilter();
		}

		#endregion

		#region TestReportData_LocationOrArea

		public void TestReportData_LocationOrArea()
		{
			var staff1 = Helper.CreateGlbStaff("STF", "Staff");
			var whs1 = Helper.CreateWarehouse("WHS1");
			var stageArea1 = Helper.CreateArea(whs1, "STA1");
			var stageArea2 = Helper.CreateArea(whs1, "STA2");
			Helper.CreateRowAndGenerateLocations(whs1, "A", 1, 2, 3);

			var orgBookingParty = CreateOrgHeader("BkgPty", "BookingParty");
			var orgConsignee = CreateOrgHeader("CNE", "Consignee");
			var orgConsignor = CreateOrgHeader("CNR", "Consignor");

			var receiveHeader1 = CreateReceiveTransportationUnit(whs1, "RecHeader1");
			var receiveConsignment1 = CreateReceiveConsignmentAndFillWithData("RecCons1", staff1, orgBookingParty.Addresses[0], orgConsignor.Addresses[0], orgConsignee.Addresses[0], false, whs1.PK);
			var dispatchLoadListSTA = CreateDispatchLoadList(whs1, "dllSTA");
			var dispatchConsignment1 = Helper.CreateDispatchConsignment("DisCons1", whs1.PK);
			var package1 = CreatePackage("package1", receiveConsignment1, staff1, false, false, false, false, false, false, false, false);
			var packageState1 = CreatePackageState(receiveHeader1.PK, receiveConsignment1.PK, ZGuid.Empty, dispatchLoadListSTA.PK, dispatchConsignment1.PK, package1.PK, "STA", whs1.Areas[0].PickLocations[0].PK, whs1.PK);

			var receiveHeader2 = CreateReceiveTransportationUnit(whs1, "RecHeader2");
			var receiveConsignment2 = CreateReceiveConsignmentAndFillWithData("RecCons2", staff1, orgBookingParty.Addresses[0], orgConsignor.Addresses[0], orgConsignee.Addresses[0], false, whs1.PK);
			var dispatchConsignment2 = Helper.CreateDispatchConsignment("DisCons2", whs1.PK);
			var package2 = CreatePackage("package2", receiveConsignment2, staff1, false, false, false, false, false, false, false, false);
			var packageState2 = CreatePackageState(receiveHeader2.PK, receiveConsignment2.PK, ZGuid.Empty, dispatchLoadListSTA.PK, dispatchConsignment2.PK, package2.PK, "STA", whs1.Areas[0].PickLocations[0].PK, whs1.PK);
			packageState2.WPS_WL_LastLocation = whs1.Areas[0].PickLocations[1].PK;

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals(2, results.Count);

			AssertEquals(whs1.DefaultLocation.WLV_LocationString_UserFriendly, results[0]["LocationOrArea"]);
			AssertEquals("package1", results[0]["PackageID"]);

			AssertEquals(whs1.Areas[0].PickLocations[1].WLV_LocationString_UserFriendly, results[1]["LocationOrArea"]);
			AssertEquals("package2", results[1]["PackageID"]);
		}

		public void TestReportData_LocationOrArea_FixedWidthLocationWarehouse()
		{
			var staff1 = Helper.CreateGlbStaff("STF", "Staff");
			var whs1 = Helper.CreateFixedWidthLocationWarehouse("WHS1", 3, 2, 2);
			var stageArea1 = Helper.CreateArea(whs1, "STA1");
			var stageArea2 = Helper.CreateArea(whs1, "STA2");
			Helper.CreateRowAndGenerateLocations(whs1, "A", 4, 3, 2);

			var orgBookingParty = CreateOrgHeader("BkgPty", "BookingParty");
			var orgConsignee = CreateOrgHeader("CNE", "Consignee");
			var orgConsignor = CreateOrgHeader("CNR", "Consignor");

			var receiveHeader1 = CreateReceiveTransportationUnit(whs1, "RecHeader1");
			var receiveConsignment1 = CreateReceiveConsignmentAndFillWithData("RecCons1", staff1, orgBookingParty.Addresses[0], orgConsignor.Addresses[0], orgConsignee.Addresses[0], false, whs1.PK);
			var dispatchLoadListSTA = CreateDispatchLoadList(whs1, "dllSTA");
			var dispatchConsignment1 = Helper.CreateDispatchConsignment("DisCons1", whs1.PK);
			var package1 = CreatePackage("package1", receiveConsignment1, staff1, false, false, false, false, false, false, false, false);
			var packageState1 = CreatePackageState(receiveHeader1.PK, receiveConsignment1.PK, ZGuid.Empty, dispatchLoadListSTA.PK, dispatchConsignment1.PK, package1.PK, "STA", whs1.Areas[0].PickLocations[0].PK, whs1.PK);

			var receiveHeader2 = CreateReceiveTransportationUnit(whs1, "RecHeader2");
			var receiveConsignment2 = CreateReceiveConsignmentAndFillWithData("RecCons2", staff1, orgBookingParty.Addresses[0], orgConsignor.Addresses[0], orgConsignee.Addresses[0], false, whs1.PK);
			var dispatchConsignment2 = Helper.CreateDispatchConsignment("DisCons2", whs1.PK);
			var package2 = CreatePackage("package2", receiveConsignment2, staff1, false, false, false, false, false, false, false, false);
			var packageState2 = CreatePackageState(receiveHeader2.PK, receiveConsignment2.PK, ZGuid.Empty, dispatchLoadListSTA.PK, dispatchConsignment2.PK, package2.PK, "STA", whs1.Areas[0].PickLocations[0].PK, whs1.PK);
			packageState2.WPS_WL_LastLocation = whs1.Areas[0].PickLocations[1].PK;

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals(2, results.Count);

			AssertEquals(whs1.DefaultLocation.WLV_LocationString_UserFriendly, results[0]["LocationOrArea"]);
			AssertEquals("package1", results[0]["PackageID"]);

			AssertEquals(whs1.Areas[0].PickLocations[1].WLV_LocationString_UserFriendly, results[1]["LocationOrArea"]);
			AssertEquals("package2", results[1]["PackageID"]);
		}

		#endregion

		#region TestReportData_CustomsStatus

		public void TestReportData_CustomsStatus()
		{
			CreateBasicTestData();

			CreateARVPackageAndRelativeEntities();
			packageStateARV.WPS_CustomsStatus = "CLR";

			CreateSTAPackageAndRelativeEntities();
			packageStateSTA.WPS_CustomsStatus = "CUS";

			CreatePUTPackageAndRelativeEntities();
			packageStatePUT.WPS_CustomsStatus = "PAN";

			CreateCTTPackageAndRelativeEntities();
			packageStateCTT.WPS_CustomsStatus = "NON";

			CreatePICPackageAndRelativeEntities();

			Factory.Save();

			var results = LoadViewWithoutFilter();
			AssertEquals(5, results.Count);

			AssertPackageCustomsStatus(results[0], packageARV.KP_PackageID, "CLR");
			AssertPackageCustomsStatus(results[1], packageSTA.KP_PackageID, "NCL");
			AssertPackageCustomsStatus(results[2], packagePUT.KP_PackageID, "NCL");
			AssertPackageCustomsStatus(results[3], packageCTT.KP_PackageID, "NCR");
			AssertPackageCustomsStatus(results[4], packagePIC.KP_PackageID, "NCR");
		}

		void AssertPackageCustomsStatus(DynamicBusinessObject businessObject, string packageID, string customsStatus)
		{
			AssertEquals(packageID, businessObject["PackageID"]);
			AssertEquals(customsStatus, businessObject["CustomsStatus"]);
		}

		#endregion

		#region TestReportData_DGColumns

		public void TestReportData_DGColumns_BasicCase()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var bookedByParty = data.Org1;
			var consignor = data.Org2;
			var consignee = data.Org3;

			var receiveHeader = Helper.CreateReceiveTransportationUnit("RcvHeader", data.Whs1.PK, data.Whs1.Areas[0].PickLocations[0].PK);
			var receive = Helper.CreateReceiveConsignment("RCV1", data.Whs1.PK, bookedByParty, consignor, consignee);
			var receive2 = Helper.CreateReceiveConsignment("RCV2", data.Whs1.PK, bookedByParty, consignor, consignee);

			var package_NoDG = Helper.CreatePackageState(receive, 3, "PLT", "", TransitWarehouseStatuses.Codes.Arrived, receiveHeader);

			var package_WithDG1 = Helper.CreatePackageState(receive2, 3, "PLT", "", TransitWarehouseStatuses.Codes.Arrived, receiveHeader, Helper.CreateDispatchConsignment("DSPC3", data.Whs1.PK));
			Helper.PackingHelper.CreateDangerousGood(package_WithDG1.Package, "0004A", "1.1D");

			Factory.Save();

			var results = LoadViewWithoutFilter();
			AssertEquals("Should be 2 row.", 2, results.Count);

			AssertDGColumns(results[0], package_NoDG);
			AssertDGColumns(results[1], package_WithDG1);
		}

		void AssertDGColumns(DynamicBusinessObject row, WhsItemPackageState packageState)
		{
			var expectedSubstances = "";
			var expectedClasses = "";
			var expectedPackingGroups = "";
			var expectedShippingNames = "";

			var dangerousGoods = packageState.Package.UNDGs.OrderBy(dg => dg.Substance?.DG_Code).ToArray();
			for (int i = 0; i < dangerousGoods.Length; i++)
			{
				var dg = dangerousGoods[i];
				var newLineIfNeeded = dangerousGoods.Length > 0 && (i < dangerousGoods.Length - 1) ? System.Environment.NewLine : "";
				expectedSubstances += (dg.Substance != null ? dg.Substance.DG_Code : ZString.Empty) + newLineIfNeeded;
				expectedClasses += dg.DI_IMOClass + newLineIfNeeded;

				var substance = dg.Substance;
				expectedPackingGroups += (substance != null ? (string)substance.DG_PG : string.Empty) + newLineIfNeeded;
				expectedShippingNames += (substance != null ? (string)substance.DG_PSN : string.Empty) + newLineIfNeeded;
			}

			Assert("Dangerous Good Substance Code Does not match", string.Compare(expectedSubstances, row["DGSubstance"].ToString(), true) == 0);
			AssertEquals("Dangerous Good Class", expectedClasses, row["DGClass"]);
			AssertEquals("Dangerous Good Packing Group", expectedPackingGroups, row["DGPackingGroup"]);
			AssertEquals("Dangerous Good Proper Shipping Name", expectedShippingNames, row["DGPSN"]);
		}

		#endregion

		#region TestReportData_HoldColumns

		public void TestReportData_HoldColumns()
		{
			CreateBasicTestData();

			CreateARVPackageAndRelativeEntities();
			CreatePackageHold(packageARV, "DAM");

			CreateSTAPackageAndRelativeEntities();
			CreatePackageHold(packageSTA, "DAM");
			var deletedHold = CreatePackageHold(packageSTA, "LCC", removed: true);
			CreatePackageHold(packageSTA, "HEL");

			CreatePUTPackageAndRelativeEntities();
			CreatePackageHold(packagePUT, "LCC");
			CreatePackageHold(packagePUT, "DAM");
			CreatePackageHold(packagePUT, "HEL");

			CreateCTTPackageAndRelativeEntities();

			Factory.Save();

			var results = LoadViewWithoutFilter();
			AssertEquals(4, results.Count);

			AssertHoldColumns(results[0], new string[] { "DAM" }, new string[] { "Damaged" });
			AssertHoldColumns(results[1], new string[] { "DAM", "HEL" }, new string[] { "Damaged", "Held" });
			AssertHoldColumns(results[2], new string[] { "DAM", "LCC", "HEL" }, new string[] { "Damaged", "Lost in Cycle Count", "Held" });
			AssertHoldColumns(results[3], Array.Empty<string>(), Array.Empty<string>());
		}

		void AssertHoldColumns(DynamicBusinessObject row, string[] code, string[] description)
		{
			AssertContainsExactElementsInAnyOrder(row["HoldCode"].ToString().Replace("\r\n", "|").Split('|').Where(s => !s.IsNullOrEmpty()), code);
			AssertContainsExactElementsInAnyOrder(row["HoldReason"].ToString().Replace("\r\n", "|").Split('|').Where(s => !s.IsNullOrEmpty()), description);
		}

		#endregion

		#region TestReportData_TransportColumns

		public void TestReportData_TransportColumns()
		{
			CreateBasicTestData();

			CreatePUTPackageAndRelativeEntities();
			receiveHeaderPUT.WRH_VehicleReference = "P1RVR";
			dispatchHeaderPUT.WDH_VehicleReference = "P1DVR";

			CreateFLOPackageAndRelativeEntities();
			receiveHeaderFLO.WRH_VehicleReference = "P2RVR";
			dispatchHeaderFLO.WDH_VehicleReference = "P2DVR";

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals(2, results.Count);

			AssertEquals("P1RVR", results[0]["RTUID"]);
			AssertEquals(string.Empty, results[0]["DTUID"]);
			AssertEquals(string.Empty, results[0]["DLLID"]);
			AssertEquals(string.Empty, results[0]["DLLReferenceNumber"]);

			AssertEquals("P2RVR", results[1]["RTUID"]);
			AssertEquals("P2DVR", results[1]["DTUID"]);
			AssertEquals("DisLoadListFLO", results[1]["DLLID"]);
			AssertEquals("DisLoadListFLO", results[1]["DLLReferenceNumber"]);
		}

		#endregion

		#region TestReportData_TransferID

		public void TestReportData_TransferID()
		{
			CreateBasicTestData();

			CreateARVPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStateARV, "ARV");

			CreateSTAPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStateSTA, "STA");

			CreatePUTPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStatePUT, "PUT");

			CreateCTTPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStateCTT, "CTT");
			var transferCTT = Helper.CreateTransferHeader("CTTTSF", warehouse1, false);
			Helper.CreateTransferLine(transferCTT, inBoundLocation, outBoundLocation, packageStateCTT);

			CreatePICPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStatePIC, "PIC");
			var transferPIC = Helper.CreateTransferHeader("PICTSF", warehouse1, false);
			var transferLinePIC = Helper.CreateTransferLine(transferPIC, inBoundLocation, outBoundLocation, packageStatePIC, ZDateTime.Now, "RON");

			CreateFLOPackageAndRelativeEntities();
			CreateHistoryTransfers(packageStateFLO, "FLO");

			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals(6, results.Count);

			AssertTransferID(string.Empty, results[0]);
			AssertTransferID("STAHTSF02", results[1]);
			AssertTransferID("PUTHTSF02", results[2]);
			AssertTransferID("CTTTSF", results[3]);
			AssertTransferID("PICTSF", results[4]);
			AssertTransferID(string.Empty, results[5]);
		}

		void AssertTransferID(string expected, DynamicBusinessObject businessObject)
		{
			AssertEquals(expected, businessObject["TransferID"]);
		}

		#endregion

		#region TestReportData_PackageOrderReferenceColumns

		public void TestReportData_PackageOrderReferenceColumns()
		{
			CreateBasicTestData();
			CreateARVPackageAndRelativeEntities();
			CreatePackageOrderReference(packageStateARV, "KPO001");
			Factory.Save();

			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals(1, results.Count);
			string expected =
@"ORN KPO001
SKU SKU001
CIN CIN001
BAT KPO001
SRN SN001
EXP 04-Jan-2024
LNE LR001";
			AssertEquals(expected, results[0]["PackageOrderInformation"].ToString());
		}

		#endregion

		#region TestReportData_AdditionalReferenceColumn

		public void TestReportData_AdditionalReferenceInReceiveConsignment()
		{
			CreateBasicTestData();

			CreateARVPackageAndRelativeEntities();

			Factory.Save();

			var results = LoadViewWithoutFilter();
			AssertEquals(1, results.Count);

			AssertContainsExactElementsInAnyOrder("AAS RecCons1AAS, MMS RecCons1MMS", results[0]["AdditionalInformation"].ToString());
		}

		public void TestReportData_AdditionalReferenceInPackage()
		{
			CreateBasicTestData();

			CreateARVPackageAndRelativeEntities(true, false);

			Factory.Save();

			var results = LoadViewWithoutFilter();
			AssertEquals(1, results.Count);

			AssertContainsExactElementsInAnyOrder("AAS packageARVAAS, MMS packageARVMMS", results[0]["AdditionalInformation"].ToString());
		}

		public void TestReportData_AdditionalReferenceInReceiveConsignmentAndPackage()
		{
			CreateBasicTestData();

			CreateARVPackageAndRelativeEntities(true, true);

			Factory.Save();

			var results = LoadViewWithoutFilter();
			AssertEquals(1, results.Count);

			AssertContainsExactElementsInAnyOrder("AAS packageARVAAS, MMS packageARVMMS, AAS RecCons1AAS, MMS RecCons1MMS", results[0]["AdditionalInformation"].ToString());
		}

		#endregion

		#region Assert Functions

		void AssertWarehouseFilter()
		{
			var results = LoadView(warehouse1.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With Warehouse PK only, 6 records should return.", 6, results.Count);
			AssertDataRows(results, ARV, STA, CTT, PIC, FLO, ADJ);
		}

		void AssertPackageStatusFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }));
			AssertEquals("With Status only, 7 records should return.", 7, results.Count);
			AssertDataRows(results, ARV, STA, PUT, CTT, PIC, FLO, ADJ);
		}

		void AssertHighRiskFilters()
		{
			var resultsHRS = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), ZGuid.Empty, null, "", "", "", "",
				0, 0, 0, 0, 0, 0, 0, 0, onlyIsHighRisk: 1, onlyIsHighRiskAuthorized: 0, securityStatus: TransitWarehouseSecurityStatuses.Codes.RequiresHighRiskScreening);
			var resultsHRN = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), ZGuid.Empty, null, "", "", "", "",
				0, 0, 0, 0, 0, 0, 0, 0, onlyIsHighRisk: 1, onlyIsHighRiskAuthorized: 0, securityStatus: TransitWarehouseSecurityStatuses.Codes.HighRiskScreenedNotAuthorized);
			var resultsSCR = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), ZGuid.Empty, null, "", "", "", "",
				0, 0, 0, 0, 0, 0, 0, 0, onlyIsHighRisk: 1, onlyIsHighRiskAuthorized: 1, securityStatus: TransitWarehouseSecurityStatuses.Codes.Screened);

			AssertEquals("With IsHighRisk & securityStatus to HRS, 1 record should return.", 1, resultsHRS.Count);
			AssertDataRows(resultsHRS, ARV);
			AssertEquals("With IsHighRisk & securityStatus to HRN, 1 record should return.", 1, resultsHRN.Count);
			AssertDataRows(resultsHRN, ADJ);
			AssertEquals("With IsHighRisk, IsHighRiskAuthorized & securityStatus to SCR, 1 record should return.", 1, resultsSCR.Count);
			AssertDataRows(resultsSCR, STA);
		}

		void AssertWithAllAndEmptyFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("All empty parameters, 7 records should return.", 7, results.Count);
			AssertDataRows(results, ARV, STA, PUT, CTT, PIC, FLO, ADJ);

			results = LoadView(warehouse1.PK, BookingParty1.PK, Consignor1.PK, Consignee1.PK, packageARV.KP_PackageID, receiveConsignmentARV.WRC_ConsignmentID, dispatchConsignmentARV.WDC_ConsignmentID,
					receiveConsignmentARV.WRC_ConsignmentID + CusEntryNumber.EntryType.MainManifestStatus, string.Empty, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), new List<string>(new[] { ARV, PUT }));
			AssertEquals("With all valid parameters, 1 records should return.", 1, results.Count);
			AssertDataRows(results, ARV);
		}

		void AssertIsFumigatedFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), default, null, "", "", "", "", 1);
			AssertEquals("With IsFumigated only, 2 records should return.", 2, results.Count);
			AssertDataRows(results, ARV, ADJ);
		}

		void AssertIsTopLoadOnlyFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), default, null, "", "", "", "", 0, 1);
			AssertEquals("With IsTopLoadOnly only, 1 record should return.", 1, results.Count);
			AssertDataRows(results, ADJ);
		}

		void AssertIsNonStackableFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), default, null, "", "", "", "", 0, 0, 1);
			AssertEquals("With IsNonStackable only, 1 record should return.", 1, results.Count);
			AssertDataRows(results, STA);
		}

		void AssertIsHeatTreatedFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), default, null, "", "", "", "", 0, 0, 0, 1);
			AssertEquals("With IsHeatTreated only, 1 record should return.", 1, results.Count);
			AssertDataRows(results, STA);
		}

		void AssertIsISPMPalletFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), default, null, "", "", "", "", 0, 0, 0, 0, 1);
			AssertEquals("With IsISPMPallet only, 1 record should return.", 1, results.Count);
			AssertDataRows(results, PUT);
		}
		void AssertIsDamagedFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), default, null, "", "", "", "", 0, 0, 0, 0, 0, 1);
			AssertEquals("With IsDamaged only, 1 record should return.", 1, results.Count);
			AssertDataRows(results, CTT);
		}

		void AssertIsHeldFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), default, null, "", "", "", "", 0, 0, 0, 0, 0, 0, 1);
			AssertEquals("With IsHeld only, 1 record should return.", 1, results.Count);
			AssertDataRows(results, FLO);
		}

		void AssertIsPillagedFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), default, null, "", "", "", "", 0, 0, 0, 0, 0, 0, 0, 1);
			AssertEquals("With IsPillaged only, 1 record should return.", 1, results.Count);
			AssertDataRows(results, PIC);
		}

		void AssertIsFumigatedIsTopLoadOnlyFilters()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(new[] { ADJ, ARV, PUT, CTT, PIC, FLO, STA }), default, null, "", "", "", "", 1, 1);
			AssertEquals("With IsFumigated and IsTopLoadOnly, 1 record should return.", 1, results.Count);
			AssertDataRows(results, ADJ);
		}

		void AssertAdditionalReferenceFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, "WrongAdditionalReference", string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With Wrong AdditionalReference, 0 records should return.", 0, results.Count);
			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, receiveConsignmentARV.WRC_ConsignmentID + CusEntryNumber.EntryType.MainManifestStatus, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With AdditionalReference only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, ARV);
		}

		void AssertPackangeAndConsignmentIDsFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, "WrongPackageID", string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With Wrong PackageID, 0 records should return.", 0, results.Count);
			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, packageARV.KP_PackageID, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With PackageID only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, ARV);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, "WrongID", string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With Wrong ReceiptConsignmentReference, 0 records should return.", 0, results.Count);
			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, receiveConsignmentARV.WRC_ConsignmentID, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With ReceiptConsignmentReference only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, ARV);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, "WrongID", string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With Wrong DispatchConsignmentReference, 0 records should return.", 0, results.Count);
			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, dispatchConsignmentARV.WDC_ConsignmentID, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With DispatchConsignmentReference only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, ARV);
		}

		void AssertArrivalDateFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Now.AddDays(2), ZDateTime.Invalid, new List<string>());
			AssertEquals("With future ArrivalFrom date only, 0 records should return.", 0, results.Count);
			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Now.AddDays(-4), ZDateTime.Now.AddDays(-2), new List<string>());
			AssertEquals("With past ArrivalFrom and ArrivalTo date only, 0 records should return.", 0, results.Count);
			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), new List<string>());
			AssertEquals("With ArrivalFrom and ArrivalTo date only, 7 records should return.", 7, results.Count);
			AssertDataRows(results, ARV, STA, PUT, CTT, PIC, FLO, ADJ);
		}

		void AssertDocAddressesFilter()
		{
			var results = LoadView(ZGuid.Empty, BookingParty1.PK, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With BookingParty PK only, 6 records should return.", 6, results.Count);
			AssertDataRows(results, ARV, PUT, CTT, PIC, FLO, ADJ);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, Consignor1.PK, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With Consignor PK only, 6 records should return.", 6, results.Count);
			AssertDataRows(results, ARV, PUT, CTT, PIC, FLO, ADJ);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, Consignee1.PK, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With Consignee PK only, 6 records should return.", 6, results.Count);
			AssertDataRows(results, ARV, PUT, CTT, PIC, FLO, ADJ);
		}

		void AssertHoldCodeFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), holdCode: ZGuid.Missing);
			AssertEquals("With Wrong HoldCode, 0 records should return.", 0, results.Count);

			var dam = Factory.Load<IWhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "DAM")).Single();
			var lcc = Factory.Load<IWhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "LCC")).Single();
			var hel = Factory.Load<IWhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "HEL")).Single();
			var resultsDAM = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), holdCode: dam.PK);
			AssertEquals("With HoldCode 2 records should return.", 2, resultsDAM.Count);
			AssertDataRows(resultsDAM, ARV, STA);

			var resultsLCC = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), holdCode: lcc.PK);
			AssertEquals("With HoldCode 3 records should return.", 3, resultsLCC.Count);
			AssertDataRows(resultsLCC, ARV, STA, PUT);

			var resultsHEL = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), holdCode: hel.PK);
			AssertEquals("With HoldCode 1 records should return.", 1, resultsHEL.Count);
			AssertDataRows(resultsHEL, ARV);
		}

		void AssertCustomsStatusFilter()
		{
			var cleared = "CLR";
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), customsStatus: new List<string>() { cleared });
			AssertEquals("With Cleared Selected, 1 records should return.", 1, results.Count);
			AssertDataRows(results, ARV);

			var notCleared = "NCL";
			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), customsStatus: new List<string>() { notCleared });
			AssertEquals("With NotCleared Selected, 2 records should return.", 2, results.Count);
			AssertDataRows(results, STA, PUT);

			var noClearanceRequired = "NCR";
			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), customsStatus: new List<string>() { noClearanceRequired });
			AssertEquals("With NoClearanceRequired Selected, 4 records should return.", 4, results.Count);
			AssertDataRows(results, CTT, PIC, FLO, ADJ);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), customsStatus: new List<string>() { cleared, notCleared });
			AssertEquals("With Cleared and NotCleared Selected, 3 records should return.", 3, results.Count);
			AssertDataRows(results, ARV, STA, PUT);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), customsStatus: new List<string>() { cleared, noClearanceRequired });
			AssertEquals("With Cleared and NoClearanceRequired Selected, 5 records should return.", 5, results.Count);
			AssertDataRows(results, ARV, CTT, PIC, FLO, ADJ);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), customsStatus: new List<string>() { cleared, notCleared, noClearanceRequired });
			AssertEquals("With All Options Selected, 7 records should return.", 7, results.Count);
			AssertDataRows(results, ARV, STA, PUT, CTT, PIC, FLO, ADJ);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), customsStatus: new List<string>() { string.Empty });
			AssertEquals("With No CustomsStatus Selected, 7 records should return.", 7, results.Count);
			AssertDataRows(results, ARV, STA, PUT, CTT, PIC, FLO, ADJ);
		}

		void AssertRTUIDFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), rtuID: "WrongRTUID");
			AssertEquals("With Wrong RTUID, 0 records should return.", 0, results.Count);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), rtuID: "FLORTU");
			AssertEquals("With RTUID only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, FLO);
		}

		void AssertDLLIDFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), dllID: "WrongDLLID");
			AssertEquals("With Wrong DLLID, 0 records should return.", 0, results.Count);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), dllID: "DisLoadListFLO");
			AssertEquals("With DLLID only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, FLO);
		}

		void AssertDLLReferenceNumberFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), referenceNumber: "WrongReferenceNumber");
			AssertEquals("With Wrong ReferenceNumber, 0 records should return.", 0, results.Count);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), referenceNumber: "DisLoadListFLO");
			AssertEquals("With ReferenceNumber only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, FLO);
		}

		void AssertDamagedReasonFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), onlyIsDamaged: 1, damagedReason: "AAA");
			AssertEquals("With DamagedReason only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, CTT);
		}

		void AssertDTUIDFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), dtuID: "WrongDTUID");
			AssertEquals("With Wrong DTUID, 0 records should return.", 0, results.Count);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), dtuID: "FLODTU");
			AssertEquals("With DTUID only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, FLO);
		}

		void AssertTransferIDFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), transferID: "WrongTransferID");
			AssertEquals("With Wrong TransferID, 0 records should return.", 0, results.Count);

			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>(), transferID: "PUTHTSF02");
			AssertEquals("With Transfer only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, PUT);
		}

		void AssertPackageOrderReferenceFilter()
		{
			var results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "WrongPackageOrderReference", ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With Wrong PackageOrderReference, 0 records should return.", 0, results.Count);
			results = LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, packageOrderReference.KPO_OrderNumber, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
			AssertEquals("With PackageOrderReference only, 1 records should return.", 1, results.Count);
			AssertDataRows(results, ARV);
		}

		#endregion

		#region AssertDataRows

		void AssertDataRows(DynamicBusinessObjectCollection businessObjectCollection, params string[] packages)
		{
			AssertEquals(businessObjectCollection.Count, packages.Length);

			for (var i = 0; i < packages.Length; i++)
			{
				var package = packages[i];
				Assert(AssertMethodsMapping.ContainsKey(package));

				AssertMethodsMapping[package].Invoke(businessObjectCollection[i]);
			}
		}

		Dictionary<string, Action<DynamicBusinessObject>> AssertMethodsMapping
		{
			get
			{
				if (assertMethodsMapping == null)
				{
					assertMethodsMapping = new Dictionary<string, Action<DynamicBusinessObject>>()
					{
						{ ADJ , AssertDataRowIsADJPackageAndRelativeEntities },
						{ ARV , AssertDataRowIsARVPackageAndRelativeEntities },
						{ PUT , AssertDataRowIsPUTPackageAndRelativeEntities },
						{ CTT , AssertDataRowIsCTTPackageAndRelativeEntities },
						{ PIC , AssertDataRowIsPICPackageAndRelativeEntities },
						{ FLO , AssertDataRowIsFLOPackageAndRelativeEntities },
						{ STA , AssertDataRowIsSTAPackageAndRelativeEntities },
					};
				}
				return assertMethodsMapping;
			}
		}

		Dictionary<string, Action<DynamicBusinessObject>> assertMethodsMapping;

		void AssertDataRowIsPUTPackageAndRelativeEntities(DynamicBusinessObject businessObject)
		{
			AssertColumnValues(warehouse2.WW_WarehouseName, packagePUT, receiveConsignmentPUT.WRC_ConsignmentID, bookingParty1, consignorParty1, consigneeParty1, dispatchConsignmentPUT.WDC_ConsignmentID, true, PUT, warehouse2.Areas[0].PickLocations[0].WLV_LocationString_UserFriendly, businessObject);
		}

		void AssertDataRowIsCTTPackageAndRelativeEntities(DynamicBusinessObject businessObject)
		{
			AssertColumnValues(warehouse1.WW_WarehouseName, packageCTT, receiveConsignmentCTT.WRC_ConsignmentID, bookingParty1, consignorParty1, consigneeParty1, dispatchConsignmentCTT.WDC_ConsignmentID, true, CTT, warehouse1.Areas[0].PickLocations[0].WLV_LocationString_UserFriendly, businessObject);
		}

		void AssertDataRowIsPICPackageAndRelativeEntities(DynamicBusinessObject businessObject)
		{
			AssertColumnValues(warehouse1.WW_WarehouseName, packagePIC, receiveConsignmentPIC.WRC_ConsignmentID, bookingParty1, consignorParty1, consigneeParty1, dispatchConsignmentPIC.WDC_ConsignmentID, true, PIC, warehouse1.Areas[0].PickLocations[0].WLV_LocationString_UserFriendly, businessObject);
		}

		void AssertDataRowIsFLOPackageAndRelativeEntities(DynamicBusinessObject businessObject)
		{
			AssertColumnValues(warehouse1.WW_WarehouseName, packageFLO, receiveConsignmentFLO.WRC_ConsignmentID, bookingParty1, consignorParty1, consigneeParty1, dispatchConsignmentFLO.WDC_ConsignmentID, true, FLO, warehouse1.Areas[0].PickLocations[0].WLV_LocationString_UserFriendly, businessObject);
		}

		void AssertDataRowIsSTAPackageAndRelativeEntities(DynamicBusinessObject businessObject)
		{
			AssertColumnValues(warehouse1.WW_WarehouseName, packageSTA, receiveConsignmentSTA.WRC_ConsignmentID, bookingParty2, consignorParty2, consigneeParty2, dispatchConsignmentSTA.WDC_ConsignmentID, false, STA, warehouse1.Areas[0].PickLocations[0].WLV_LocationString_UserFriendly, businessObject, securityStatus: TransitWarehouseSecurityStatuses.Codes.Screened);
		}

		void AssertDataRowIsARVPackageAndRelativeEntities(DynamicBusinessObject businessObject)
		{
			AssertColumnValues(warehouse1.WW_WarehouseName, packageARV, receiveConsignmentARV.WRC_ConsignmentID, bookingParty1, consignorParty1, consigneeParty1, dispatchConsignmentARV.WDC_ConsignmentID, true, ARV, warehouse1.Areas[0].PickLocations[0].WLV_LocationString_UserFriendly, businessObject, securityStatus: TransitWarehouseSecurityStatuses.Codes.RequiresHighRiskScreening);
		}

		void AssertDataRowIsADJPackageAndRelativeEntities(DynamicBusinessObject businessObject)
		{
			AssertColumnValues(warehouse1.WW_WarehouseName, packageADJ, receiveConsignmentADJ.WRC_ConsignmentID, bookingParty1, consignorParty1, consigneeParty1, dispatchConsignmentADJ.WDC_ConsignmentID, true, ADJ, warehouse1.Areas[0].PickLocations[0].WLV_LocationString_UserFriendly, businessObject, securityStatus: TransitWarehouseSecurityStatuses.Codes.HighRiskScreenedNotAuthorized);
		}

		void AssertColumnValues(string warehouseName, PkgPackage package, string receiptConsignmentReference, string bookingPartyName, string consignorName, string consigneeName, string dispatchConsignmentReference,
				bool additionalInformation, string packageStatus, string locationOrArea, DynamicBusinessObject resultRow, string holdCode = "", string rtuID = "", string dllID = "", string dtuID = "", string transferID = "", string securityStatus = TransitWarehouseSecurityStatuses.Codes.Secured)
		{
			AssertEquals("Warehouse does not match with expected value.", warehouseName, resultRow["WarehouseName"]);
			AssertEquals("BookingParty does not match with expected value.", bookingPartyName, resultRow["BookingParty"]);
			AssertEquals("Consignor does not match with expected value.", consignorName, resultRow["Consignor"]);
			AssertEquals("Consignee does not match with expected value.", consigneeName, resultRow["Consignee"]);
			AssertEquals("PackageID does not match with expected value.", package.KP_PackageID, resultRow["PackageID"]);
			AssertEquals("ReceiptConsignmentReference does not match with expected value.", receiptConsignmentReference, resultRow["ReceiptConsignmentReference"]);
			AssertEquals("DispatchConsignmentReference does not match with expected value.", dispatchConsignmentReference, resultRow["DispatchConsignmentReference"]);

			if (additionalInformation)
			{
				AssertEquals("AdditionalInformation does not match with expected value.", true, resultRow["AdditionalInformation"].ToString().Contains(receiptConsignmentReference + CusEntryNumber.EntryType.MainManifestStatus));
				AssertEquals("AdditionalInformation does not match with expected value.", true, resultRow["AdditionalInformation"].ToString().Contains(receiptConsignmentReference + CusEntryNumber.EntryType.ActualArrivalStatus));
			}

			AssertEquals("PackageStatus does not match with expected value.", packageStatus, resultRow["PackageStatus"]);
			AssertEquals("PackageStatus does not match with expected value.", securityStatus, resultRow["PackageSecurityStatus"]);

			if (securityStatus == TransitWarehouseSecurityStatuses.Codes.RequiresHighRiskScreening ||
				securityStatus == TransitWarehouseSecurityStatuses.Codes.HighRiskScreenedNotAuthorized
				)
			{
				AssertEquals("IsHighRisk does not match with expected value.", true, resultRow["IsHighRisk"]);
				AssertEquals("IsHighRiskAuthorized does not match with expected value.", false, resultRow["IsHighRiskAuthorized"]);
			}
			else if (securityStatus == TransitWarehouseSecurityStatuses.Codes.Screened)
			{
				AssertEquals("IsHighRisk does not match with expected value.", true, resultRow["IsHighRisk"]);
				AssertEquals("IsHighRiskAuthorized does not match with expected value.", true, resultRow["IsHighRiskAuthorized"]);
			}
			else
			{
				AssertEquals("IsHighRisk does not match with expected value.", false, resultRow["IsHighRisk"]);
				AssertEquals("IsHighRiskAuthorized does not match with expected value.", false, resultRow["IsHighRiskAuthorized"]);
			}

			AssertEquals("LocaltionOrArea does not match with expected value.", locationOrArea, resultRow["LocationOrArea"]);

			AssertEquals("MarksAndNumbers does not match with expected value.", package.KP_MarksAndNumbers, resultRow["MarksAndNumbers"]);
			AssertEquals("PackageType does not match with expected value.", package.KP_F3_NKPackType, resultRow["PackageType"]);
			AssertEquals("Weights does not match with expected value.", package.KP_Weight, resultRow["Weights"]);
			AssertEquals("Lengths does not match with expected value.", package.KP_Length, resultRow["Lengths"]);
			AssertEquals("Width does not match with expected value.", package.KP_Width, resultRow["Width"]);
			AssertEquals("Height does not match with expected value.", package.KP_Height, resultRow["Height"]);
			AssertEquals("Volume does not match with expected value.", package.KP_Volume, resultRow["Volume"]);

			AssertEquals("IsFumigated does not match with expected value.", package.KP_IsFumigated, resultRow["IsFumigated"]);
			AssertEquals("IsTopLoadOnly does not match with expected value.", package.KP_IsTopLoadOnly, resultRow["IsTopLoadOnly"]);
			AssertEquals("IsNonStackable does not match with expected value.", package.KP_IsNonStackable, resultRow["IsNonStackable"]);
			AssertEquals("IsHeatTreated does not match with expected value.", package.KP_IsHeatTreated, resultRow["IsHeatTreated"]);
			AssertEquals("IsISPMPallet does not match with expected value.", package.KP_IsISPMPallet, resultRow["IsISPMPallet"]);
			AssertEquals("IsDamaged does not match with expected value.", package.KP_IsDamaged, resultRow["IsDamaged"]);
			AssertEquals("IsHeld does not match with expected value.", package.KP_IsHeld, resultRow["IsHeld"]);
			AssertEquals("IsPillaged does not match with expected value.", package.KP_IsPillaged, resultRow["IsPillaged"]);

			AssertEqualsIfExpectedIsNotEmpty("HoldCode does not match with expected value.", holdCode, resultRow["HoldCode"]);
			AssertEqualsIfExpectedIsNotEmpty("RTUID does not match with expected value.", rtuID, resultRow["RTUID"]);
			AssertEqualsIfExpectedIsNotEmpty("DLLID does not match with expected value.", dllID, resultRow["DLLID"]);
			AssertEqualsIfExpectedIsNotEmpty("ReferenceNumber does not match with expected value.", dllID, resultRow["DLLReferenceNumber"]);
			AssertEqualsIfExpectedIsNotEmpty("DTUID does not match with expected value.", dtuID, resultRow["DTUID"]);
			AssertEqualsIfExpectedIsNotEmpty("TransferID does not match with expected value.", transferID, resultRow["TransferID"]);
		}

		void AssertEqualsIfExpectedIsNotEmpty(string message, string expected, object actual)
		{
			if (!string.IsNullOrEmpty(expected))
			{
				AssertEquals(message, expected, actual);
			}
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView(ZGuid whsPK, ZGuid bookingPartyPK, ZGuid consignorPK, ZGuid consigneePK, string packageID, string receiptConsignmentReference, string dispatchConsignmentReference,
				string additionalReference, string packageOrderReference, ZDateTime arrivalDateFrom, ZDateTime arrivalDateTo, List<string> status, ZGuid holdCode = default, List<string> customsStatus = null, string rtuID = "", string dllID = "", string dtuID = "", string transferID = "",
				int onlyIsFumigated = 0, int onlyIsTopLoadOnly = 0, int onlyIsNonStackable = 0, int onlyIsHeatTreated = 0, int onlyIsISPMPallet = 0, int onlyIsDamaged = 0, int onlyIsHeld = 0, int onlyIsPillaged = 0, int onlyIsHighRisk = 0, int onlyIsHighRiskAuthorized = 0, string securityStatus = "", string referenceNumber = "", string damagedReason = "")
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var statements = new List<string>();

			string sql = @"Select * From Report_WhsTransitPackageOnHandWithUNDGCode";
			CreateParameterList(whsPK, bookingPartyPK, consignorPK, consigneePK, packageID, receiptConsignmentReference, dispatchConsignmentReference, additionalReference, packageOrderReference, arrivalDateFrom, arrivalDateTo, status, holdCode, customsStatus, rtuID, dllID, dtuID, transferID,
					onlyIsFumigated, onlyIsTopLoadOnly, onlyIsNonStackable, onlyIsHeatTreated, onlyIsISPMPallet, onlyIsDamaged, onlyIsHeld, onlyIsPillaged, onlyIsHighRisk, onlyIsHighRiskAuthorized, securityStatus, referenceNumber, damagedReason, statements);
			sql += "(" + string.Join(",", statements) + ") order by ReceiptConsignmentReference, Consignor";
			result.Load(sql);

			return result;
		}

		DynamicBusinessObjectCollection LoadViewWithoutFilter()
		{
			return LoadView(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, ZDateTime.Invalid, ZDateTime.Invalid, new List<string>());
		}

		#endregion

		#region CreateParameterList

		void AddParamByType(object parameter, List<string> statements)
		{
			if (parameter is ZGuid)
			{
				if ((ZGuid)parameter != ZGuid.Empty)
				{
					statements.Add("'" + parameter.ToString() + "'");
				}
				else
				{
					statements.Add("null");
				}
			}
			else if (parameter is string)
			{
				if (!string.IsNullOrEmpty((string)parameter))
				{
					statements.Add(" '" + parameter + "'");
				}
				else
				{
					statements.Add(" ''");
				}
			}
			else if (parameter is ZDateTime)
			{
				var valueDate = (ZDateTime)parameter;
				if (valueDate != ZDateTime.Invalid)
				{
					statements.Add(" '" + valueDate.ToString("yyyy-MM-dd HH:mm:ss") + "'");
				}
				else
				{
					statements.Add(" ''");
				}
			}
			else if (parameter is List<string>)
			{
				var valueList = (List<string>)parameter;
				if (valueList.Count > 0)
				{
					statements.Add(" '" + string.Join(", ", valueList) + "'");
				}
				else
				{
					statements.Add(" ''");
				}
			}
			else if (parameter is int)
			{
				statements.Add(" '" + parameter + "'");
			}
		}

		void CreateParameterList(ZGuid whsPK, ZGuid bookingPartyPK, ZGuid consignorPK, ZGuid consigneePK, string packageID, string receiptConsignmentReference, string dispatchConsignmentReference, string additionalReference, string packageOrderReference,
				ZDateTime arrivalDateFrom, ZDateTime arrivalDateTo, List<string> status, ZGuid holdCode, List<string> customsStatus, string rtuID, string dllID, string dtuID, string transferID, int onlyIsFumigated, int onlyIsTopLoadOnly,
				int onlyIsNonStackable, int onlyIsHeatTreated, int onlyIsISPMPallet, int onlyIsDamaged, int onlyIsHeld, int onlyIsPillaged, int onlyIsHighRisk, int onlyIsHighRiskAuthorized, string securityStatus, string referenceNumber, string damagedReason, List<string> statements)
		{
			AddParamByType(whsPK, statements);
			AddParamByType(bookingPartyPK, statements);
			AddParamByType(consignorPK, statements);
			AddParamByType(consigneePK, statements);
			AddParamByType(packageID, statements);
			AddParamByType(receiptConsignmentReference, statements);
			AddParamByType(dispatchConsignmentReference, statements);
			AddParamByType(additionalReference, statements);
			AddParamByType(packageOrderReference, statements);
			AddParamByType(arrivalDateFrom, statements);
			AddParamByType(arrivalDateTo, statements);
			AddParamByType(status, statements);
			AddParamByType(holdCode, statements);
			AddParamByType(customsStatus == null ? string.Empty : customsStatus, statements);
			AddParamByType(rtuID, statements);
			AddParamByType(dllID, statements);
			AddParamByType(dtuID, statements);
			AddParamByType(transferID, statements);
			AddParamByType(onlyIsFumigated, statements);
			AddParamByType(onlyIsTopLoadOnly, statements);
			AddParamByType(onlyIsNonStackable, statements);
			AddParamByType(onlyIsHeatTreated, statements);
			AddParamByType(onlyIsISPMPallet, statements);
			AddParamByType(onlyIsDamaged, statements);
			AddParamByType(onlyIsHeld, statements);
			AddParamByType(onlyIsPillaged, statements);
			AddParamByType(onlyIsHighRisk, statements);
			AddParamByType(onlyIsHighRiskAuthorized, statements);
			AddParamByType(securityStatus, statements);
			AddParamByType(referenceNumber, statements);
			AddParamByType(damagedReason, statements);
		}

		#endregion

		#region Create Methods

		void CreateBasicTestData()
		{
			staff = Helper.CreateGlbStaff("STF", "Staff");
			warehouse1 = Helper.CreateWarehouse("WHS1");
			Helper.CreateRowAndGenerateLocations(warehouse1, "A", 1, 2, 3);
			warehouse2 = Helper.CreateWarehouse("WHS2", "B");

			inBoundLocation = Helper.CreateLocation(warehouse1, "INB");
			outBoundLocation = Helper.CreateLocation(warehouse1, "OBU");

			BookingParty1 = CreateOrgHeader("BKD1", bookingParty1);
			Consignee1 = CreateOrgHeader("CNE1", consigneeParty1);
			Consignor1 = CreateOrgHeader("CNR1", consignorParty1);

			BookingParty2 = CreateOrgHeader("BKD2", bookingParty2);
			Consignee2 = CreateOrgHeader("CNE2", consigneeParty2);
			Consignor2 = CreateOrgHeader("CNR2", consignorParty2);
		}

		void CreateCTTPackageAndRelativeEntities()
		{
			receiveHeaderCTT = CreateReceiveTransportationUnit(warehouse1, "RecHeaderCTT");
			receiveConsignmentCTT = CreateReceiveConsignmentAndFillWithData("RecCons6", staff, BookingParty1.Addresses[0], Consignor1.Addresses[0], Consignee1.Addresses[0], true, warehouse1.PK);
			dispatchHeaderCTT = CreateDispatchTransportationUnit(warehouse1, "DisHeaderCTT");
			dispatchConsignmentCTT = Helper.CreateDispatchConsignment("DisConsCTT", warehouse1.PK);
			packageCTT = CreatePackage("packageCTT", receiveConsignmentCTT, staff, false, false, false, false, false, true, false, false);
			packageStateCTT = CreatePackageState(receiveHeaderCTT.PK, receiveConsignmentCTT.PK, ZGuid.Empty, ZGuid.Empty, dispatchConsignmentCTT.PK, packageCTT.PK, CTT, warehouse1.Areas[0].PickLocations[0].PK, warehouse1.PK);
		}

		void CreateSTAPackageAndRelativeEntities()
		{
			receiveHeaderSTA = CreateReceiveTransportationUnit(warehouse1, "RecHeaderSTA");
			receiveConsignmentSTA = CreateReceiveConsignmentAndFillWithData("RecCons2", staff, BookingParty2.Addresses[0], Consignor2.Addresses[0], Consignee2.Addresses[0], false, warehouse1.PK);
			var dispatchLoadListSTA = CreateDispatchLoadList(warehouse1, "DisLoadListSTA");
			dispatchConsignmentSTA = Helper.CreateDispatchConsignment("DisConsSTA", warehouse1.PK);
			packageSTA = CreatePackage("packageSTA", receiveConsignmentSTA, staff, false, false, true, true, false, false, false, false);
			packageStateSTA = CreatePackageState(receiveHeaderSTA.PK, receiveConsignmentSTA.PK, ZGuid.Empty, dispatchLoadListSTA.PK, dispatchConsignmentSTA.PK, packageSTA.PK, STA, warehouse1.Areas[0].PickLocations[0].PK, warehouse1.PK, TransitWarehouseSecurityStatuses.Codes.Screened);
		}

		void CreateARVPackageAndRelativeEntities(bool packageAdditionalInformation = false, bool receiveConsignmentAdditionalInformation = true)
		{
			receiveHeaderARV = CreateReceiveTransportationUnit(warehouse1, "RecHeaderARV");
			receiveConsignmentARV = CreateReceiveConsignmentAndFillWithData("RecCons1", staff, BookingParty1.Addresses[0], Consignor1.Addresses[0], Consignee1.Addresses[0], receiveConsignmentAdditionalInformation, warehouse1.PK);
			dispatchConsignmentARV = Helper.CreateDispatchConsignment("DisConsARV", warehouse1.PK);
			packageARV = CreatePackage("packageARV", receiveConsignmentARV, staff, true, false, false, false, false, false, false, false, packageAdditionalInformation);
			packageStateARV = CreatePackageState(receiveHeaderARV.PK, receiveConsignmentARV.PK, ZGuid.Empty, ZGuid.Empty, dispatchConsignmentARV.PK, packageARV.PK, ARV, warehouse1.Areas[0].PickLocations[0].PK, warehouse1.PK, TransitWarehouseSecurityStatuses.Codes.RequiresHighRiskScreening);
		}

		void CreateADJPackageAndRelativeEntities()
		{
			receiveHeaderADJ = CreateReceiveTransportationUnit(warehouse1, "RecHeaderADJ");
			receiveConsignmentADJ = CreateReceiveConsignmentAndFillWithData("RecCons9", staff, BookingParty1.Addresses[0], Consignor1.Addresses[0], Consignee1.Addresses[0], true, warehouse1.PK);
			dispatchConsignmentADJ = Helper.CreateDispatchConsignment("DisConsADJ", warehouse1.PK);
			packageADJ = CreatePackage("packageADJ", receiveConsignmentADJ, staff, true, true, false, false, false, false, false, false);
			packageStateADJ = CreatePackageState(receiveHeaderADJ.PK, receiveConsignmentADJ.PK, ZGuid.Empty, ZGuid.Empty, dispatchConsignmentADJ.PK, packageADJ.PK, ADJ, warehouse1.Areas[0].PickLocations[0].PK, warehouse1.PK, TransitWarehouseSecurityStatuses.Codes.HighRiskScreenedNotAuthorized);
		}

		void CreatePICPackageAndRelativeEntities()
		{
			receiveHeaderPIC = CreateReceiveTransportationUnit(warehouse1, "RecHeaderPIC");
			receiveConsignmentPIC = CreateReceiveConsignmentAndFillWithData("RecCons7", staff, BookingParty1.Addresses[0], Consignor1.Addresses[0], Consignee1.Addresses[0], true, warehouse1.PK);
			dispatchHeaderPIC = CreateDispatchTransportationUnit(warehouse1, "DisHeaderPIC");
			dispatchConsignmentPIC = Helper.CreateDispatchConsignment("DisConsPIC", warehouse1.PK);
			packagePIC = CreatePackage("packagePIC", receiveConsignmentPIC, staff, false, false, false, false, false, false, false, true);
			packageStatePIC = CreatePackageState(receiveHeaderPIC.PK, receiveConsignmentPIC.PK, ZGuid.Empty, ZGuid.Empty, dispatchConsignmentPIC.PK, packagePIC.PK, PIC, warehouse1.Areas[0].PickLocations[0].PK, warehouse1.PK);
		}

		void CreatePUTPackageAndRelativeEntities()
		{
			receiveHeaderPUT = CreateReceiveTransportationUnit(warehouse2, "RecHeaderPUT");
			receiveConsignmentPUT = CreateReceiveConsignmentAndFillWithData("RecCons3", staff, BookingParty1.Addresses[0], Consignor1.Addresses[0], Consignee1.Addresses[0], true, warehouse2.PK);
			dispatchHeaderPUT = CreateDispatchTransportationUnit(warehouse2, "DisHeaderPUT");
			dispatchConsignmentPUT = Helper.CreateDispatchConsignment("DisConsPUT", warehouse2.PK);
			packagePUT = CreatePackage("packagePUT", receiveConsignmentPUT, staff, false, false, false, false, true, false, false, false);
			packageStatePUT = CreatePackageState(receiveHeaderPUT.PK, receiveConsignmentPUT.PK, ZGuid.Empty, ZGuid.Empty, dispatchConsignmentPUT.PK, packagePUT.PK, PUT, warehouse2.Areas[0].PickLocations[0].PK, warehouse2.PK);
		}

		void CreateDEPPackageAndRelativeEntities()
		{
			receiveHeaderDEP = CreateReceiveTransportationUnit(warehouse2, "RecHeaderDEP");
			receiveConsignmentDEP = CreateReceiveConsignmentAndFillWithData("RecCons5", staff, BookingParty2.Addresses[0], Consignor2.Addresses[0], Consignee2.Addresses[0], true, warehouse2.PK);
			var dispatchLoadListDEP = CreateDispatchLoadList(warehouse2, "DisLoadListDEP");
			dispatchHeaderDEP = CreateDispatchTransportationUnit(warehouse2, "DisHeaderDEP");
			dispatchConsignmentDEP = Helper.CreateDispatchConsignment("DisConsDEP", warehouse2.PK);
			packageDEP = CreatePackage("packageDEP", receiveConsignmentDEP, staff, false, false, false, false, false, false, false, false);
			packageStateDEP = CreatePackageState(receiveHeaderDEP.PK, receiveConsignmentDEP.PK, dispatchHeaderDEP.PK, dispatchLoadListDEP.PK, dispatchConsignmentDEP.PK, packageDEP.PK, DEP, warehouse2.Areas[0].PickLocations[0].PK, warehouse2.PK);
		}

		void CreateBKDPackageAndRelativeEntities()
		{
			receiveHeaderBKD = CreateReceiveTransportationUnit(warehouse2, "RecHeaderBKD");
			receiveConsignmentBKD = CreateReceiveConsignmentAndFillWithData("RecCons4", staff, BookingParty2.Addresses[0], Consignor2.Addresses[0], Consignee2.Addresses[0], true, warehouse2.PK);
			dispatchConsignmentBKD = Helper.CreateDispatchConsignment("DisConsBKD", warehouse2.PK);
			packageBKD = CreatePackage("packageBKD", receiveConsignmentBKD, staff, false, false, false, false, false, false, false, false);
			packageStateBKD = CreatePackageState(ZGuid.Empty, receiveConsignmentBKD.PK, ZGuid.Empty, ZGuid.Empty, dispatchConsignmentBKD.PK, packageBKD.PK, BKD, warehouse2.Areas[0].PickLocations[0].PK, warehouse2.PK);
		}

		void CreateFLOPackageAndRelativeEntities()
		{
			receiveHeaderFLO = CreateReceiveTransportationUnit(warehouse1, "RecHeaderFLO", "FLORTU");
			receiveConsignmentFLO = CreateReceiveConsignmentAndFillWithData("RecCons8", staff, BookingParty1.Addresses[0],
				Consignor1.Addresses[0], Consignee1.Addresses[0], true, warehouse1.PK);
			var dispatchLoadListFLO = CreateDispatchLoadList(warehouse1, "DisLoadListFLO");
			dispatchHeaderFLO = CreateDispatchTransportationUnit(warehouse1, "DisHeaderFLO", "FLODTU");
			dispatchConsignmentFLO = Helper.CreateDispatchConsignment("DisConsFLO", warehouse1.PK);
			packageFLO = CreatePackage("packageFLO", receiveConsignmentFLO, staff, false, false, false, false, false, false, true, false);
			packageStateFLO = CreatePackageState(receiveHeaderFLO.PK, receiveConsignmentFLO.PK, dispatchHeaderFLO.PK,
				dispatchLoadListFLO.PK, dispatchConsignmentFLO.PK, packageFLO.PK, FLO, warehouse1.Areas[0].PickLocations[0].PK, warehouse1.PK);
		}

		WhsItemPackageState CreatePackageState(ZGuid receiveHeaderPK, ZGuid receiveConsignmentPK, ZGuid dispatchHeaderPK, ZGuid dispatchLoadListPK, ZGuid dispatchConsignmentPK, ZGuid packagePK, string status, ZGuid location, ZGuid warehousePK, string securityStatus = TransitWarehouseSecurityStatuses.Codes.Secured)
		{
			var packageState = Factory.New<WhsItemPackageState>();
			packageState.WPS_WDH_TransitDispatchHeader = dispatchHeaderPK;
			packageState.WPS_IsSecure = true;
			packageState.WPS_SecurityStatus = securityStatus;
			packageState.WPS_CustomsStatus = "NON";

			packageState.WPS_WDC_TransitDispatchConsignment = dispatchConsignmentPK;
			packageState.WPS_WDL_LoadList = dispatchLoadListPK;
			packageState.WPS_WRH_TransitReceiveHeader = receiveHeaderPK;
			packageState.WPS_WRC_TransitReceiveConsignment = receiveConsignmentPK;
			packageState.WPS_KP_Package = packagePK;
			packageState.WPS_Status = status;
			if (securityStatus == TransitWarehouseSecurityStatuses.Codes.RequiresHighRiskScreening || securityStatus == TransitWarehouseSecurityStatuses.Codes.HighRiskScreenedNotAuthorized)
			{
				packageState.WPS_IsHighRisk = true;
			}
			else if (securityStatus == TransitWarehouseSecurityStatuses.Codes.Screened)
			{
				packageState.WPS_IsHighRisk = true;
				packageState.WPS_IsHighRiskAuthorized = true;
			}
			packageState.WPS_WL_ReceiveLocation = location;
			packageState.WPS_WW_Warehouse = warehousePK;

			if (status != BKD)
			{
				packageState.WPS_WL_LastLocation = location;
			}

			return packageState;
		}

		PkgPackage CreatePackage(string packageID, WhsItemReceiveConsignment consignment, GlbStaff staff, bool isFumigated, bool isTopLoadOnly, bool isNonStackable, bool isHeatTreated, bool isISPMPallet, bool isDamaged, bool isHeld, bool isPillaged, bool additionalInformation = false)
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = consignment.PK;
			packageJob.KJ_ParentTableCode = WhsItemReceiveConsignmentSchema.Constants.Prefix;
			var package = packageJob.Packages.AddNew();
			package.KP_PackageID = packageID;
			package.KP_MarksAndNumbers = "Marks";
			package.KP_F3_NKPackType = Constants.PkgUnit.Carton;
			package.KP_Weight = 10m;
			package.KP_Length = 20m;
			package.KP_Width = 30m;
			package.KP_Height = 40m;
			package.KP_Volume = 50m;
			package.KP_IsFumigated = isFumigated;
			package.KP_IsTopLoadOnly = isTopLoadOnly;
			package.KP_IsNonStackable = isNonStackable;
			package.KP_IsHeatTreated = isHeatTreated;
			package.KP_IsISPMPallet = isISPMPallet;
			package.KP_IsDamaged = isDamaged;
			package.KP_IsHeld = isHeld;
			package.KP_IsPillaged = isPillaged;
			if (isDamaged)
			{
				package.KP_DamagedReason = "AAA";
			}
			CreateStmALog(ARV, staff, package, ZDateTime.Today);
			if (additionalInformation)
			{
				Helper.CreateAdditionalReference(package, packageID + CusEntryNumber.EntryType.MainManifestStatus, CusEntryNumber.EntryType.MainManifestStatus);
				Helper.CreateAdditionalReference(package, packageID + CusEntryNumber.EntryType.ActualArrivalStatus, CusEntryNumber.EntryType.ActualArrivalStatus);
			}

			return package;
		}

		public void TestReportData_DamagedReason()
		{
			var sql = $@"
INSERT dbo.StmData(SD_PK, SD_Name, SD_Type, SD_IsLogged, SD_BinaryValue) VALUES
	(NEWID(), 'DamagedReasons', 'BIN', 1, 0x3C003F0078006D006C002000760065007200730069006F006E003D00220031002E0030002200200065006E0063006F00640069006E0067003D0022007500740066002D003100360022003F003E003C00530079007300740065006D0044006500660069006E00610062006C00650043006F00640065004400650073006300720069007000740069006F006E0042006F006F006C0043006F006C006C0065006300740069006F006E002000440065006600610075006C00740043006F00640065003D00220022003E003C00530079007300740065006D0044006500660069006E00610062006C00650043006F00640065004400650073006300720069007000740069006F006E0042006F006F006C003E003C0043006F00640065004D00610078004C0065006E006700740068003E0033003C002F0043006F00640065004D00610078004C0065006E006700740068003E003C0043006F00640065003E004100410041003C002F0043006F00640065003E003C004400650073006300720069007000740069006F006E003E00410041004100200044004500530043003C002F004400650073006300720069007000740069006F006E003E003C002F00530079007300740065006D0044006500660069006E00610062006C00650043006F00640065004400650073006300720069007000740069006F006E0042006F006F006C003E003C00530079007300740065006D0044006500660069006E00610062006C00650043006F00640065004400650073006300720069007000740069006F006E0042006F006F006C003E003C0043006F00640065004D00610078004C0065006E006700740068003E0033003C002F0043006F00640065004D00610078004C0065006E006700740068003E003C0043006F00640065003E004200420042003C002F0043006F00640065003E003C004400650073006300720069007000740069006F006E003E00420042004200200044004500530043003C002F004400650073006300720069007000740069006F006E003E003C002F00530079007300740065006D0044006500660069006E00610062006C00650043006F00640065004400650073006300720069007000740069006F006E0042006F006F006C003E003C00530079007300740065006D0044006500660069006E00610062006C00650043006F00640065004400650073006300720069007000740069006F006E0042006F006F006C003E003C0043006F00640065004D00610078004C0065006E006700740068003E0033003C002F0043006F00640065004D00610078004C0065006E006700740068003E003C0043006F00640065003E004300430043003C002F0043006F00640065003E003C004400650073006300720069007000740069006F006E003E00430043004300200044004500530043003C002F004400650073006300720069007000740069006F006E003E003C002F00530079007300740065006D0044006500660069006E00610062006C00650043006F00640065004400650073006300720069007000740069006F006E0042006F006F006C003E003C002F00530079007300740065006D0044006500660069006E00610062006C00650043006F00640065004400650073006300720069007000740069006F006E0042006F006F006C0043006F006C006C0065006300740069006F006E003E00);";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}

			CreateBasicTestData();

			CreateARVPackageAndRelativeEntities();
			packageARV.KP_IsDamaged = true;
			packageARV.KP_DamagedReason = "AAA";

			CreateSTAPackageAndRelativeEntities();
			packageSTA.KP_IsDamaged = true;
			packageSTA.KP_DamagedReason = "BBB";

			Factory.Save();

			var results = LoadViewWithoutFilter();

			AssertEquals(2, results.Count);

			var arvResult = results.First(r => r["PackageID"].ToString() == packageARV.KP_PackageID);
			AssertEquals(true, arvResult["IsDamaged"]);
			AssertEquals("AAA", packageARV.KP_DamagedReason);
			AssertEquals("AAA DESC", arvResult["DamagedReason"]);

			var staResult = results.First(r => r["PackageID"].ToString() == packageSTA.KP_PackageID);
			AssertEquals(true, staResult["IsDamaged"]);
			AssertEquals("BBB", packageSTA.KP_DamagedReason);
			AssertEquals("BBB DESC", staResult["DamagedReason"]);
		}

		WhsItemDispatchTransportationUnit CreateDispatchTransportationUnit(WhsWarehouse warehouse, string referenceNumber, string vehicleReference = "")
		{
			var dispatchUnit = Factory.New<WhsItemDispatchTransportationUnit>();
			dispatchUnit.WDH_WW_Warehouse = warehouse.PK;
			dispatchUnit.WDH_ReferenceNumber = referenceNumber;
			dispatchUnit.WDH_VehicleReference = vehicleReference;

			return dispatchUnit;
		}

		WhsItemDispatchLoadList CreateDispatchLoadList(WhsWarehouse warehouse, string jobID, string referenceNumber = null)
		{
			var dispatchLoadList = Factory.New<WhsItemDispatchLoadList>();
			dispatchLoadList.WDL_WW_Warehouse = warehouse.PK;
			dispatchLoadList.WDL_JobID = jobID;
			dispatchLoadList.WDL_ReferenceNumber = referenceNumber ?? jobID;

			return dispatchLoadList;
		}

		WhsItemReceiveTransportationUnit CreateReceiveTransportationUnit(WhsWarehouse warehouse, string referenceNumber, string vehicleReference = "")
		{
			var receiveUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			receiveUnit.WRH_WW_Warehouse = warehouse.PK;
			receiveUnit.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
			receiveUnit.WRH_ReferenceNumber = referenceNumber;
			receiveUnit.WRH_VehicleReference = vehicleReference;

			return receiveUnit;
		}

		WhsItemReceiveConsignment CreateReceiveConsignment(string consignmentID, string bookingParty, ZGuid bookingPartyAddress, string consignor, ZGuid consignorAddress, string consignee, ZGuid consigneeAddress, ZGuid intendedWarehousePK)
		{
			var receiveConsignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			receiveConsignment.WRC_ConsignmentID = consignmentID;
			receiveConsignment.WRC_WW_IntendedWarehouse = intendedWarehousePK;
			receiveConsignment.WRC_CustomsStatus = "NON";

			CreateJobDocAddress(receiveConsignment.PK, DocAddressTypes.Codes.BookingPartyDocumentaryAddress, bookingPartyAddress);
			CreateJobDocAddress(receiveConsignment.PK, DocAddressTypes.Codes.LocalCartageExporter, consignorAddress);
			CreateJobDocAddress(receiveConsignment.PK, DocAddressTypes.Codes.ConsigneeDocumentaryAddress, consigneeAddress);

			return receiveConsignment;
		}

		WhsItemReceiveConsignment CreateReceiveConsignmentAndFillWithData(string consignmentID, GlbStaff staff, OrgAddress addressBookingParty, OrgAddress addressConsignor, OrgAddress addressConsignee, bool additionalInformation, ZGuid intendedWarehousePK)
		{
			var receiveConsignment = CreateReceiveConsignment(consignmentID, consignmentID, addressBookingParty.PK, consignmentID, addressConsignor.PK, consignmentID, addressConsignee.PK, intendedWarehousePK);
			if (additionalInformation)
			{
				Helper.CreateAdditionalReference(receiveConsignment, consignmentID + CusEntryNumber.EntryType.MainManifestStatus, CusEntryNumber.EntryType.MainManifestStatus);
				Helper.CreateAdditionalReference(receiveConsignment, consignmentID + CusEntryNumber.EntryType.ActualArrivalStatus, CusEntryNumber.EntryType.ActualArrivalStatus);
			}

			return receiveConsignment;
		}

		JobDocAddress CreateJobDocAddress(ZGuid receiveConsignmentPK, string type, ZGuid addressPK)
		{
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_OA_Address = addressPK;
			address.E2_AddressType = type;
			address.E2_ParentID = receiveConsignmentPK;

			return address;
		}

		OrgHeader CreateOrgHeader(string code, string fullName)
		{
			var client = Helper.CreateClient(code, fullName);
			var address = client.Addresses.AddNew();
			address.OA_Address1 = "Address fullName";
			address.OA_City = "SYD";

			return client;
		}

		void CreateStmALog(string eventCode, GlbStaff testStaff, PkgPackage package, ZDateTime eventTime)
		{
			var addEventLog = package.Logs.AddNew();
			AssertEquals("Event added successfully", package.PK, addEventLog.SL_Parent);
			using (addEventLog.LockForUpdatingKeyFieldsForTesting())
			{
				addEventLog.SL_SE_NKEvent = eventCode;
				addEventLog.SL_GS_NKUser = testStaff.GS_Code;
				addEventLog.SL_EventTime = eventTime;
			}
		}

		public PkgPackageHold CreatePackageHold(PkgPackage package, string holdCode, bool removed = false)
		{
			var packageHold = Factory.New<PkgPackageHold>();
			packageHold.KHR_KP_Package = package.PK;
			packageHold.KHR_WHC_NKHoldCode = holdCode;
			packageHold.KHR_GS_NKAddedBy = "~BP";
			packageHold.KHR_AddedTime = ZDateTimeOffset.Today;
			packageHold.KHR_SystemCreateUser = "~BP";
			packageHold.KHR_SystemLastEditUser = "~BP";
			if (removed)
			{
				packageHold.KHR_GS_NKRemovedBy = "~BP";
				packageHold.KHR_RemovedTime = ZDateTimeOffset.Today.AddHours(1);
			}
			return packageHold;
		}

		void CreateHistoryTransfers(WhsItemPackageState packageState, string referenceNumberPrefix)
		{
			var now = ZDateTime.Now;

			var header01 = Helper.CreateTransferHeader($"{referenceNumberPrefix}HTSF01", warehouse1, true);
			var line01 = (BusinessObject)Helper.CreateTransferLine(header01, inBoundLocation, outBoundLocation, packageState, now.AddDays(-5), "RON");
			line01[WhsItemTransferLineSchema.WTF_PutTime] = now.AddDays(-5).AddMinutes(10);
			line01[WhsItemTransferLineSchema.WTF_GS_NKPutUser] = "RON";

			var header02 = Helper.CreateTransferHeader($"{referenceNumberPrefix}HTSF02", warehouse1, true);
			var line02 = (BusinessObject)Helper.CreateTransferLine(header02, inBoundLocation, outBoundLocation, packageState, now.AddDays(-4), "RON");
			line02[WhsItemTransferLineSchema.WTF_PutTime] = now.AddDays(-4).AddMinutes(10);
			line02[WhsItemTransferLineSchema.WTF_GS_NKPutUser] = "RON";
		}

		void CreatePackageOrderReference(WhsItemPackageState packageState, string orderNumber)
		{
			packageOrderReference = Factory.New<PkgPackageOrderReference>();
			packageOrderReference.KPO_BatchNumber = "KPO001";
			packageOrderReference.KPO_CommercialInvoiceNumber = "CIN001";
			packageOrderReference.KPO_ExpiryDate = new ZDate(2024, 1, 4);
			packageOrderReference.KPO_KP_Package = packageState.Package.PK;
			packageOrderReference.KPO_LineReference = "LR001";
			packageOrderReference.KPO_OrderNumber = orderNumber;
			packageOrderReference.KPO_SerialNumber = "SN001";
			packageOrderReference.KPO_SKUPartNumber = "SKU001";
		}

		#endregion

		#region Members Variables

		const string bookingParty1 = "BookingParty1";
		const string bookingParty2 = "BookingParty2";

		const string consignorParty1 = "ConsignorParty1";
		const string consignorParty2 = "ConsignorParty2";

		const string consigneeParty1 = "ConsigneeParty1";
		const string consigneeParty2 = "ConsigneeParty2";

		const string ARV = "ARV";
		const string ADJ = "ADJ";
		const string PUT = "PUT";
		const string STA = "STA";
		const string BKD = "BKD";
		const string DEP = "DEP";
		const string CTT = "CTT";
		const string PIC = "PIC";
		const string FLO = "FLO";

		GlbStaff staff;

		WhsWarehouse warehouse1;
		WhsWarehouse warehouse2;

		WhsLocation inBoundLocation;
		WhsLocation outBoundLocation;

		OrgHeader BookingParty1;
		OrgHeader BookingParty2;
		OrgHeader Consignor1;
		OrgHeader Consignor2;
		OrgHeader Consignee1;
		OrgHeader Consignee2;

		WhsItemReceiveTransportationUnit receiveHeaderARV;
		WhsItemReceiveTransportationUnit receiveHeaderADJ;
		WhsItemReceiveTransportationUnit receiveHeaderSTA;
		WhsItemReceiveTransportationUnit receiveHeaderPUT;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CreateBKDPackageAndRelativeEntities")]
		WhsItemReceiveTransportationUnit receiveHeaderBKD;
		WhsItemReceiveTransportationUnit receiveHeaderDEP;
		WhsItemReceiveTransportationUnit receiveHeaderCTT;
		WhsItemReceiveTransportationUnit receiveHeaderPIC;
		WhsItemReceiveTransportationUnit receiveHeaderFLO;

		WhsItemReceiveConsignment receiveConsignmentARV;
		WhsItemReceiveConsignment receiveConsignmentADJ;
		WhsItemReceiveConsignment receiveConsignmentSTA;
		WhsItemReceiveConsignment receiveConsignmentPUT;
		WhsItemReceiveConsignment receiveConsignmentBKD;
		WhsItemReceiveConsignment receiveConsignmentDEP;
		WhsItemReceiveConsignment receiveConsignmentCTT;
		WhsItemReceiveConsignment receiveConsignmentPIC;
		WhsItemReceiveConsignment receiveConsignmentFLO;

		WhsItemDispatchTransportationUnit dispatchHeaderPUT;
		WhsItemDispatchTransportationUnit dispatchHeaderDEP;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CreateCTTPackageAndRelativeEntities")]
		WhsItemDispatchTransportationUnit dispatchHeaderCTT;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CreatePICPackageAndRelativeEntities")]
		WhsItemDispatchTransportationUnit dispatchHeaderPIC;
		WhsItemDispatchTransportationUnit dispatchHeaderFLO;

		WhsItemDispatchConsignment dispatchConsignmentARV;
		WhsItemDispatchConsignment dispatchConsignmentADJ;
		WhsItemDispatchConsignment dispatchConsignmentSTA;
		WhsItemDispatchConsignment dispatchConsignmentPUT;
		WhsItemDispatchConsignment dispatchConsignmentBKD;
		WhsItemDispatchConsignment dispatchConsignmentDEP;
		WhsItemDispatchConsignment dispatchConsignmentCTT;
		WhsItemDispatchConsignment dispatchConsignmentPIC;
		WhsItemDispatchConsignment dispatchConsignmentFLO;

		PkgPackage packageARV;
		PkgPackage packageADJ;
		PkgPackage packageSTA;
		PkgPackage packagePUT;
		PkgPackage packageBKD;
		PkgPackage packageDEP;
		PkgPackage packageCTT;
		PkgPackage packagePIC;
		PkgPackage packageFLO;

		WhsItemPackageState packageStateARV;
		WhsItemPackageState packageStateADJ;
		WhsItemPackageState packageStateSTA;
		WhsItemPackageState packageStatePUT;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CreateBKDPackageAndRelativeEntities")]
		WhsItemPackageState packageStateBKD;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in CreateDEPPackageAndRelativeEntities")]
		WhsItemPackageState packageStateDEP;
		WhsItemPackageState packageStateCTT;
		WhsItemPackageState packageStatePIC;
		WhsItemPackageState packageStateFLO;

		PkgPackageOrderReference packageOrderReference;

		#endregion
	}
}
