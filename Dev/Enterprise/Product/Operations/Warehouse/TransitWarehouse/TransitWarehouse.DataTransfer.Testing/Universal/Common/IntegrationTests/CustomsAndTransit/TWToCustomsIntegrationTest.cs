using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing.Universal.Common
{
	public class TWToCustomsIntegrationTest : IntegrationTestCaseWithFactory
	{
		#region TestGINEventFromTWToOutturn_ToUpdateCargoReceiptDateOnReceive

		[TestDate(2020, 10, 05, 10, 10, 00, 00)]
		public void TestGINEventFromTWToOutturn_ToUpdateCargoReceiptDateOnReceive()
		{
			var dateTimeOffset = ZDateTimeOffset.Now;
			var now = new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, 0, 0, dateTimeOffset.Offset);

			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PREMISEID");

			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_OA_OutturningPremise = warehouse.WarehouseAddress.PK;
			/* fields used for matching when TWH sends shipment back to SeaCargoOutturn
			- C6_LloydsIMO
			- C6_VoyageNum
			- C6_OutturningPremiseID
			*/
			outturnHeader.C6_OA_OutturningPremise = warehouse.WarehouseAddress.PK;
			outturnHeader.C6_VoyageNum = "Voyage123";
			outturnHeader.C6_VesselName = "Vessel123";
			outturnHeader.C6_LloydsIMO = "LloydsN";
			outturnHeader.C6_OutturningPremiseID = "PREMISEID";

			CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse(outturnHeader);

			// Matching outturns
			var matchingOutturnHSB1 = CreateCargoPackLine(outturnHeader, "LCL", "CN123", "HSB1", "MAS12345", 2, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
				1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			var matchingOutturnHSB2 = CreateCargoPackLine(outturnHeader, "LCL", "CN123", "HSB2", "MAS12345", 2, CMRPackageTypes.Codes.Carton, "SAFETY EQUIPMENT",
				1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			var matchingFCL = CreateCargoPackLine(outturnHeader, "FCL", "CN123", "", "", 2, CMRPackageTypes.Codes.Package, "", 1100m);

			// Non-Matching outturn
			var unMatchingOutturn = CreateCargoPackLine(outturnHeader, "LCL", "CNUnMatch", "HSB3", "MAS12345", 2, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
			1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			Factory.Save();

			// another header and cargo packline for the same container
			var anotherOutturnOutturnHeaderWithSameContainer = Factory.New<CusOutturnHeader>();
			anotherOutturnOutturnHeaderWithSameContainer.C6_OA_OutturningPremise = warehouse.WarehouseAddress.PK;
			anotherOutturnOutturnHeaderWithSameContainer.C6_LloydsIMO = "A";
			var matchingContainerNumberFCL = CreateCargoPackLine(anotherOutturnOutturnHeaderWithSameContainer, "FCL", "CN123", "", "", 2, CMRPackageTypes.Codes.Package, "", 1100m);
			var matchingContainerNumberWithoutCargoUnPackDate = CreateCargoPackLine(anotherOutturnOutturnHeaderWithSameContainer, "LCL", "CN123", "HSB2", "unmatchingMAB", 2, CMRPackageTypes.Codes.Carton, "SAFETY EQUIPMENT",
				1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			var matchingContainerNumberWitCargoUnPackDate = CreateCargoPackLine(anotherOutturnOutturnHeaderWithSameContainer, "LCL", "CN123", "HSB2", "unmatchingMAB", 2, CMRPackageTypes.Codes.Carton, "SAFETY EQUIPMENT",
				1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			matchingContainerNumberWitCargoUnPackDate.C5_CargoReceiptDate = now.AddDays(-2).DateTime;

			Factory.Save();

			// Sea cargo outturns created with packlines
			CombineAssertions(() =>
			{
				AssertEquals(4, outturnHeader.Outturns.Count);
				AssertEquals("Cargo Receipt Date has not been populated.", ZDateTime.Empty, outturnHeader.Outturns[0].C5_CargoReceiptDate);
				AssertEquals("Cargo Receipt Date has not been populated.", ZDateTime.Empty, outturnHeader.Outturns[1].C5_CargoReceiptDate);
				AssertEquals("Cargo Receipt Date has not been populated.", ZDateTime.Empty, outturnHeader.Outturns[2].C5_CargoReceiptDate);
				AssertEquals("Cargo Receipt Date has not been populated.", ZDateTime.Empty, outturnHeader.Outturns[3].C5_CargoReceiptDate);
			});

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			// Receive Consignments, Packages and ASN created from import
			AssertReceiveConsignmentAndPackageAndASNCreated("HSB1", "MAS12345", "PKG", 2, "CN123");
			AssertReceiveConsignmentAndPackageAndASNCreated("HSB2", "MAS12345", "CTN", 2, "CN123");
			AssertReceiveConsignmentAndPackageAndASNCreated("HSB3", "MAS12345", "PKG", 2, "CNUnMatch");

			// Receive Transportation Unit with container has gated in
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK, "CN123", vehicleRef: "CN123");
			receiveTransportationUnit.WRH_GateInTime = now;

			CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalEvent(receiveTransportationUnit);
			var eventLog = receiveTransportationUnit.Logs.AddNew(new EventValue(Events.GateIn, eventTime: ZDateTimeOffset.Now));
			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = true };
			var hsb1InNewFactory = newFactory.Load<CusOutturn>(matchingOutturnHSB1.PK);
			var hsb2InNewFactory = newFactory.Load<CusOutturn>(matchingOutturnHSB2.PK);
			var fclInNewFactory = newFactory.Load<CusOutturn>(matchingFCL.PK);
			var sameContainerNumberDifferentMasterBillFCLInNewFactory = newFactory.Load<CusOutturn>(matchingContainerNumberFCL.PK);
			var sameContainerNumberDifferentMasterBillWithoutUnPackDateInNewFactory =
				newFactory.Load<CusOutturn>(matchingContainerNumberWithoutCargoUnPackDate.PK);
			var sameContainerNumberDifferentMasterBillWithUnPackDateInNewFactory =
				newFactory.Load<CusOutturn>(matchingContainerNumberWitCargoUnPackDate.PK);
			var unMatchingOutturnInNewFactory = newFactory.Load<CusOutturn>(unMatchingOutturn.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Cargo Receipt Date has been populated.", now.DateTime, hsb1InNewFactory.C5_CargoReceiptDate);
				AssertEquals("Cargo Receipt Date has been populated.", now.DateTime, hsb2InNewFactory.C5_CargoReceiptDate);
				AssertEquals("Cargo Receipt Date has been populated.", now.DateTime, fclInNewFactory.C5_CargoReceiptDate);
				AssertEquals("Cargo Receipt Date has not been populated.", now.DateTime, sameContainerNumberDifferentMasterBillFCLInNewFactory.C5_CargoReceiptDate);
				AssertEquals("Cargo Receipt Date has not been populated.", now.DateTime, sameContainerNumberDifferentMasterBillWithoutUnPackDateInNewFactory.C5_CargoReceiptDate);
				AssertEquals("Cargo Receipt Date has not been populated.", now.AddDays(-2).DateTime, sameContainerNumberDifferentMasterBillWithUnPackDateInNewFactory.C5_CargoReceiptDate);
				AssertEquals("Cargo Receipt Date has been left empty.", ZDateTime.Empty, unMatchingOutturnInNewFactory.C5_CargoReceiptDate);
			});
		}

		#endregion

		#region TestTWSendSubShipmentsFromVehicle_UpdatesMatchingOutturnsInSeaCargoModule

		public void TestTWSendSubShipmentsFromVehicle_UpdatesMatchingOutturnsInSeaCargoModule()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PREMISEID");

			var outturnHeader = Factory.New<CusOutturnHeader>();
			/* fields used for matching when TWH sends shipment back to SeaCargoOutturn
				- C6_LloydsIMO
				- C6_VoyageNum
				- C6_OutturningPremiseID
			*/
			outturnHeader.C6_OA_OutturningPremise = warehouse.WarehouseAddress.PK;
			outturnHeader.C6_VoyageNum = "Voyage123";
			outturnHeader.C6_VesselName = "Vessel123";
			outturnHeader.C6_LloydsIMO = "LloydsN";
			outturnHeader.C6_OutturningPremiseID = "PREMISEID";

			CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse(outturnHeader);

			var cnt1234CargoLine = CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNT1234", "", "", 2, CMRPackageTypes.Codes.Package,
				"CONSOLIDATED CARGO SAFETY EQUIP, LIGHTIN G FIXTURES AUTO PARTS",
	19026, "KG", "Marks And Numbers", "ROA", 1000, "KG", 52.8m, "CU", "HENDRICKSON ASIA PACIFIC PTY LTD");
			var hsb1234CargoLine = CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1234", "MAS1234", 1, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
	1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			var hsb1235CargoLine = CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1235", "MAS1234", 1, CMRPackageTypes.Codes.PalletLift, "STC VALVES",
	861.37m, "KG", "HENDRICKSON 80909952", "SEA", 861.37m, "KG", 2.25m, "CU", "GIPPSLAND SEED SERVICES PTY LTD");

			CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNTABCD", "", "", 2, CMRPackageTypes.Codes.Package, "PNEUMATIC ENGINES",
	2160.95m, "KG", "Marks And Numbers", "ROA", 2160.95m, "KG", 6.17m, "CU", "MCCORMICK FOODS AUSTRALIA PTY LTD");
			var hsbABCDCargoLine = CreateCargoPackLine(outturnHeader, "LCL", "CNTABCD", "HSBABCD", "MASABCD", 1, CMRPackageTypes.Codes.Parcel, "Bolts",
10.37m, "KG", "HENDRICKSON 80909952", "ROA", 12m, "KG", 1m, "CU", "ABCD BOLTS SERVICES PTY LTD");
			Factory.Save();

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			var rcnForHSB1234 = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1234")).Single();
			var rcnForHSB1235 = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1235")).Single();
			var rcnForHSBABCD = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSBABCD")).Single();
			AssertEquals(1, rcnForHSB1234.PackageStates.Count);
			AssertEquals(1, rcnForHSB1235.PackageStates.Count);
			AssertEquals(1, rcnForHSBABCD.PackageStates.Count);

			var packageStateForHSB1234 = rcnForHSB1234.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Package);
			var packageStateForHSB1235 = rcnForHSB1235.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageStateForHSBABCD = rcnForHSBABCD.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals(2, receiveASNs.Length);
			var asnForMAS1234 = receiveASNs.Where(a => a.WRP_VehicleReference == "CNT1234").Single();
			var asnForMASABCD = receiveASNs.Where(a => a.WRP_VehicleReference == "CNTABCD").Single();
			AssertEquals(asnForMAS1234, packageStateForHSB1234.ReceiveASN);
			AssertEquals(asnForMAS1234, packageStateForHSB1235.ReceiveASN);
			AssertEquals(asnForMASABCD, packageStateForHSBABCD.ReceiveASN);

			var rtu1 = Helper.CreateReceiveTransportationUnit("CNT1234", warehouse.PK, warehouse.DefaultLocation.PK, "CNT1234");
			UnloadAndLabelPackage(packageStateForHSB1234, rtu1, "PKG1");
			Helper.CreatePackageState(rcnForHSB1234, 1, Constants.PkgUnit.Pallet, "PLT2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1);
			Helper.CreatePackageState(rcnForHSB1234, 1, Constants.PkgUnit.Pallet, "PLT3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1, isDamaged: true);
			CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalShipment(rtu1);

			var rtu2 = Helper.CreateReceiveTransportationUnit("CNTABCD", warehouse.PK, warehouse.DefaultLocation.PK, "CNTABCD");
			UnloadAndLabelPackage(packageStateForHSBABCD, rtu2, "BOX1");
			Helper.CreatePackageState(rcnForHSBABCD, 1, Constants.PkgUnit.Pallet, "PLT4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2, isPillaged: true);
			CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalShipment(rtu2);

			Factory.Save();

			CombineAssertions(() =>
			{
				TriggerAndFireToOutturn(rtu1);
				var factoryToLoadOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
				var hsb1234CargoLineInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(hsb1234CargoLine.PK);
				AssertCargoPackLine(hsb1234CargoLineInAnotherFactory, 3, 1333.10000m, 2.790m, isDamaged: true, isPillaged: false, "SU", packType: "PK");

				var hsb1235CargoLineInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(hsb1235CargoLine.PK);
				AssertCargoPackLine(hsb1235CargoLineInAnotherFactory, 0, 0.00000m, 0.000m, isDamaged: false, isPillaged: false, "SH");

				TriggerAndFireToOutturn(rtu2);
				var hsbABCDCargoLineInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(hsbABCDCargoLine.PK);
				AssertCargoPackLine(hsbABCDCargoLineInAnotherFactory, 2, 10.37000m, 1.000m, isDamaged: false, isPillaged: true, "SU", packType: "YF");
			});
		}

		#endregion

		#region TestTWSendSubShipmentsFromContainer_UpdatesMatchingOutturnsInSeaCargoModule

		[TestDate(2021, 10, 19)]
		public void TestTWSendSubShipmentsFromContainer_UpdatesMatchingOutturnsInSeaCargoModule()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			var now = ZDateTimeOffset.Now;

			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PREMISEID");

			var outturnHeader = Factory.New<CusOutturnHeader>();
			/* fields used for matching when TWH sends shipment back to SeaCargoOutturn
				- C6_LloydsIMO
				- C6_VoyageNum
				- C6_OutturningPremiseID
			*/
			outturnHeader.C6_OA_OutturningPremise = warehouse.WarehouseAddress.PK;
			outturnHeader.C6_VoyageNum = "Voyage123";
			outturnHeader.C6_VesselName = "Vessel123";
			outturnHeader.C6_LloydsIMO = "LloydsN";
			outturnHeader.C6_OutturningPremiseID = "PREMISEID";

			CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse(outturnHeader);

			var cnt1234CargoLine = CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNT1234", "", "", 1, CMRPackageTypes.Codes.Basket,
				"CONSOLIDATED CARGO SAFETY EQUIP, LIGHTIN G FIXTURES AUTO PARTS",
	19026, "KG", "Marks And Numbers", "ROA", 1000, "KG", 52.8m, "CU", "HENDRICKSON ASIA PACIFIC PTY LTD");
			var hsb1234CargoLine = CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1234", "MAS1234", 1, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
	1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			var hsb1235CargoLine = CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1235", "MAS1234", 1, CMRPackageTypes.Codes.PalletLift, "STC VALVES",
	861.37m, "KG", "HENDRICKSON 80909952", "SEA", 861.37m, "KG", 2.25m, "CU", "GIPPSLAND SEED SERVICES PTY LTD");

			var cntABCDCargoLine = CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNTABCD", "", "", 10, CMRPackageTypes.Codes.Package, "PNEUMATIC ENGINES",
	2160.95m, "KG", "Marks And Numbers", "ROA", 2160.95m, "KG", 6.17m, "CU", "MCCORMICK FOODS AUSTRALIA PTY LTD");
			var hsbABCDCargoLine = CreateCargoPackLine(outturnHeader, "LCL", "CNTABCD", "HSBABCD", "MASABCD", 1, CMRPackageTypes.Codes.Parcel, "Bolts",
10.37m, "KG", "HENDRICKSON 80909952", "ROA", 12m, "KG", 1m, "CU", "ABCD BOLTS SERVICES PTY LTD");
			Factory.Save();

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			var rcnForHSB1234 = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1234")).Single();
			var rcnForHSB1235 = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1235")).Single();
			var rcnForHSBABCD = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSBABCD")).Single();
			AssertEquals(1, rcnForHSB1234.PackageStates.Count);
			AssertEquals(1, rcnForHSB1235.PackageStates.Count);
			AssertEquals(1, rcnForHSBABCD.PackageStates.Count);

			var packageStateForHSB1234 = rcnForHSB1234.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Package);
			var packageStateForHSB1235 = rcnForHSB1235.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);
			var packageStateForHSBABCD = rcnForHSBABCD.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Pallet);

			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals(2, receiveASNs.Length);
			var asnForMAS1234 = receiveASNs.Where(a => a.WRP_VehicleReference == "CNT1234").Single();
			var asnForMASABCD = receiveASNs.Where(a => a.WRP_VehicleReference == "CNTABCD").Single();
			AssertEquals(asnForMAS1234, packageStateForHSB1234.ReceiveASN);
			AssertEquals(asnForMAS1234, packageStateForHSB1235.ReceiveASN);
			AssertEquals(asnForMASABCD, packageStateForHSBABCD.ReceiveASN);

			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			AssertEquals(2, rtus.Length);

			var rtuCreatedBySystemAndStillUnLoading = rtus.Where(r => r.WRH_VehicleReference == "CNT1234").Single();
			rtuCreatedBySystemAndStillUnLoading.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
			var query = new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, rtuCreatedBySystemAndStillUnLoading.PK);
			var link = Factory.Load<StmUniversalJobLink>(query).Single();
			AssertEquals(nameof(DataContextType.SeaCargoOutturn), link.UCL_SourceType);
			AssertEquals(ZGuid.Empty, link.UCL_OH_Owner);

			var packageJobForRTU1 = PkgPackageJob.LoadOrCreatePackageJob(rtuCreatedBySystemAndStillUnLoading);
			var package = Helper.CreatePackage(packageJobForRTU1, "CNT1234", 1, "CNT");
			package.KP_Weight = 1.5m;
			package.KP_Volume = 2.5m;
			UnloadAndLabelPackage(packageStateForHSB1234, rtuCreatedBySystemAndStillUnLoading, "PKG1");
			Helper.CreatePackageState(rcnForHSB1234, 1, Constants.PkgUnit.Pallet, "PLT2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtuCreatedBySystemAndStillUnLoading);
			Helper.CreatePackageState(rcnForHSB1234, 1, Constants.PkgUnit.Pallet, "PLT3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtuCreatedBySystemAndStillUnLoading, isDamaged: true);
			CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalShipment(rtuCreatedBySystemAndStillUnLoading);

			var manuallyCreatedUnloadCompletedRTU = Helper.CreateReceiveTransportationUnitWithContainerType("CNTABCD", warehouse.PK, warehouse.DefaultLocation.PK, "CNTABCD");
			UnloadAndLabelPackage(packageStateForHSBABCD, manuallyCreatedUnloadCompletedRTU, "BOX1");
			Helper.CreatePackageState(rcnForHSBABCD, 1, Constants.PkgUnit.Pallet, "PLT4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: manuallyCreatedUnloadCompletedRTU, isPillaged: true);
			manuallyCreatedUnloadCompletedRTU.WRH_GateInTime = now.AddDays(-1);
			manuallyCreatedUnloadCompletedRTU.WRH_UnloadCompleteTime = now;
			manuallyCreatedUnloadCompletedRTU.WRH_UnloadCompleteNotYetProcessedTime = manuallyCreatedUnloadCompletedRTU.WRH_UnloadCompleteTime;
			manuallyCreatedUnloadCompletedRTU.WRH_IsVehicleSecure = true;
			CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalShipment(manuallyCreatedUnloadCompletedRTU);

			Factory.Save();

			CombineAssertions(() =>
			{
				TriggerAndFireToOutturn(rtuCreatedBySystemAndStillUnLoading);
				var factoryToLoadOutturn = new BusinessObjectFactory() { RefreshEnabled = false };
				var fclCargoLineForStillUnloadingRTUInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(cnt1234CargoLine.PK);
				AssertEquals(ZDateTime.Empty, fclCargoLineForStillUnloadingRTUInAnotherFactory.C5_CargoUnpackDate);
				AssertEquals(3, fclCargoLineForStillUnloadingRTUInAnotherFactory.C5_PackagesOutturned);
				AssertEquals(CMRPackageTypes.Codes.Basket, fclCargoLineForStillUnloadingRTUInAnotherFactory.C5_PackagesUnits);
				AssertEquals(false, fclCargoLineForStillUnloadingRTUInAnotherFactory.C5_SealIntactIndicator);
				AssertEquals("SU", fclCargoLineForStillUnloadingRTUInAnotherFactory.C5_OutturnResultType);

				var hsb1234CargoLineInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(hsb1234CargoLine.PK);
				AssertCargoPackLine(hsb1234CargoLineInAnotherFactory, 3, 1333.10000m, 2.790m, isDamaged: true, isPillaged: false, "SU", packType: "PK");
				var hsb1235CargoLineInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(hsb1235CargoLine.PK);
				AssertCargoPackLine(hsb1235CargoLineInAnotherFactory, 0, 0.00000m, 0.000m, isDamaged: false, isPillaged: false, "SH", packType: "PF");

				TriggerAndFireToOutturn(manuallyCreatedUnloadCompletedRTU);
				var hsbABCDCargoLineInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(hsbABCDCargoLine.PK);
				AssertCargoPackLine(hsbABCDCargoLineInAnotherFactory, 2, 10.37000m, 1.000m, isDamaged: false, isPillaged: true, "SU", unpackDate: now.ToZDateTime(), packType: "YF");

				var fclCargoLineForUnloadCompletedRTUInAnotherFactory = factoryToLoadOutturn.Load<DepotCusOutturn>(cntABCDCargoLine.PK);
				AssertEquals(ZDateTime.Empty, fclCargoLineForUnloadCompletedRTUInAnotherFactory.C5_CargoUnpackDate);
				AssertEquals(2, fclCargoLineForUnloadCompletedRTUInAnotherFactory.C5_PackagesOutturned);
				AssertEquals(CMRPackageTypes.Codes.Package, fclCargoLineForUnloadCompletedRTUInAnotherFactory.C5_PackagesUnits);
				AssertEquals("SH", fclCargoLineForUnloadCompletedRTUInAnotherFactory.C5_OutturnResultType);
				AssertEquals(true, fclCargoLineForUnloadCompletedRTUInAnotherFactory.C5_SealIntactIndicator);
			});
		}

		public void TestTWSendSubShipmentsFromContainer_UseUniversalLinks()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PREMISEID");
			var outturnHeader = CreateOutturnHeader(warehouse);

			CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse(outturnHeader);

			var cnt1234CargoLine = CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNT1234", "", "", 2, CMRPackageTypes.Codes.Package,
				"CONSOLIDATED CARGO SAFETY EQUIP, LIGHTIN G FIXTURES AUTO PARTS",
	19026, "KG", "Marks And Numbers", "ROA", 1000, "KG", 52.8m, "CU", "HENDRICKSON ASIA PACIFIC PTY LTD");
			var hsb1234CargoLine = CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1234", "MAS1234", 1, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
	1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			Factory.Save();

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			var rcnForHSB1234 = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1234")).Single();
			AssertEquals(1, rcnForHSB1234.PackageStates.Count);

			var packageStateForHSB1234 = rcnForHSB1234.PackageStates.Single(p => p.Package.KP_F3_NKPackType == Constants.PkgUnit.Package);

			var asnForCNT1234 = Factory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertEquals("CNT1234", asnForCNT1234.WRP_VehicleReference);
			AssertEquals(asnForCNT1234, packageStateForHSB1234.ReceiveASN);

			var rtuForCNT1234 = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
			AssertEquals("CNT1234", rtuForCNT1234.WRH_VehicleReference);
			rtuForCNT1234.WRH_WL_StagingLocation = warehouse.DefaultLocation.PK;
			var query = new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, rtuForCNT1234.PK);
			var link = Factory.Load<StmUniversalJobLink>(query).Single();
			AssertEquals(nameof(DataContextType.SeaCargoOutturn), link.UCL_SourceType);
			AssertEquals(ZGuid.Empty, link.UCL_OH_Owner);

			// set link's owner to a different org so that outturn does not get updated
			link.UCL_OH_Owner = Helper.CreateClient("TES").PK;
			Factory.Save();

			var packageJobForRTU1 = PkgPackageJob.LoadOrCreatePackageJob(rtuForCNT1234);
			var package = Helper.CreatePackage(packageJobForRTU1, "CNT1234", 1, "CNT");
			package.KP_Weight = 1.5m;
			package.KP_Volume = 2.5m;
			UnloadAndLabelPackage(packageStateForHSB1234, rtuForCNT1234, "PKG1");
			Helper.CreatePackageState(rcnForHSB1234, 1, Constants.PkgUnit.Pallet, "PLT2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtuForCNT1234);
			Helper.CreatePackageState(rcnForHSB1234, 1, Constants.PkgUnit.Pallet, "PLT3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtuForCNT1234, isDamaged: true);
			CreateTriggerForTransitWarehouseToCustomsOutturnSendUniversalShipment(rtuForCNT1234);

			Factory.Save();
			TriggerAndFireToOutturn(rtuForCNT1234);
			var factoryToLoadAfterModifyingLink = new BusinessObjectFactory() { RefreshEnabled = false };
			var hsb1234CargoLineInAnotherFactory = factoryToLoadAfterModifyingLink.Load<DepotCusOutturn>(hsb1234CargoLine.PK);
			AssertCargoPackLine(hsb1234CargoLineInAnotherFactory, 0, 0m, 0m, expectedOutturnResultType: "SH");

			link.UCL_OH_Owner = ZGuid.Empty;
			Factory.Save();

			TriggerAndFireToOutturn(rtuForCNT1234);
			var factoryToLoadAfterRemovingOwner = new BusinessObjectFactory() { RefreshEnabled = false };
			var hsb1234CargoLineAfterRemovingOwner = factoryToLoadAfterRemovingOwner.Load<DepotCusOutturn>(hsb1234CargoLine.PK);
			AssertCargoPackLine(hsb1234CargoLineAfterRemovingOwner, 3, 1333.10000m, 2.790m, isDamaged: true, isPillaged: false, expectedOutturnResultType: "SU", packType: "PK");
		}

		CusOutturnHeader CreateOutturnHeader(WhsWarehouse warehouse)
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_OA_OutturningPremise = warehouse.WarehouseAddress.PK;
			outturnHeader.C6_VoyageNum = "Voyage123";
			outturnHeader.C6_VesselName = "Vessel123";
			outturnHeader.C6_LloydsIMO = "LloydsN";
			outturnHeader.C6_OutturningPremiseID = "PREMISEID";
			return outturnHeader;
		}

		#endregion

		#region Implementation

		#region Assertion

		void AssertReceiveConsignmentAndPackageAndASNCreated(string houseBill, string masterBill, string packType, int packageQty, string containerNum = "")
		{
			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, houseBill)).Single();
			AssertNotNull(receiveConsignment);
			var receiveASNs = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals(2, receiveASNs.Length);
			var asn = receiveASNs.Where(a => a.WRP_VehicleReference == containerNum).Single();
			var packageState = receiveConsignment.PackageStates.Single();
			AssertEquals(asn, packageState.ReceiveASN);
			var package = packageState.Package;
			AssertPackageProperties(package, qty: packageQty, packType: packType, goodsDescription: "SAFETY EQUIPMENT",
				marksAndNumbers: "COOL DRIVE", weight: 1333.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");
			AssertConsignmentAdditionalRefs(receiveConsignment, houseBill: houseBill, shipmentID: "", masterbill: masterBill);
		}

		static void AssertPackageProperties(PkgPackage package, int qty, string packType, string goodsDescription, string marksAndNumbers,
		decimal weight, string weightUQ, decimal volume, string volumeUQ)
		{
			AssertEquals(qty, package.KP_PackageQty);
			AssertEquals(packType, package.KP_F3_NKPackType);
			AssertEquals(goodsDescription, package.KP_GoodsDescription);
			AssertEquals(marksAndNumbers, package.KP_MarksAndNumbers);
			AssertEquals(weight, package.KP_Weight);
			AssertEquals(weightUQ, package.KP_WeightUQ);
			AssertEquals(volume, package.KP_Volume);
			AssertEquals(volumeUQ, package.KP_VolumeUQ);
		}

		#endregion

		#endregion
	}
}
