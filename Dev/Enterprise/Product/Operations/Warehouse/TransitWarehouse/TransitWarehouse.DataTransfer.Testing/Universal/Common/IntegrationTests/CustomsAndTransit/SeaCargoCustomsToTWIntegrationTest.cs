using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class SeaCargoCustomsToTWIntegrationTest : IntegrationTestCaseWithFactory
	{
		#region Universal Events Tests

		public void TestImportCESEventFromSeaCargoOutturn_PopulatesReceiveConsignmentCustomsDetails()
		{
			var now = ZDateTimeOffset.Now;
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "AUSYD");

			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var consignmentWithNonDepPackage = Helper.CreateReceiveConsignment("HOUSEBILL", "STD", warehouse.PK, "RC00000001");
			Helper.CreatePackageState(consignmentWithNonDepPackage, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Booked);

			var outturnHeader = Factory.NewWithValidTestData<CusOutturnHeader>();
			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_HouseBill = "HOUSEBILL";
			outturn.C5_CustomsStatus = "CCL";
			outturn.FillWithValidTestData();
			var log = outturn.Logs.AddNew(Events.CustomsEntryStatus, "|SER=PCS|TYP=CLR|RES=CCL", now);

			Factory.Save();

			AssertNull("Customs Release Number hasn't been populated yet", consignmentWithNonDepPackage.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber));
			AssertEquals("Customs status is empty", TransitWarehouseCustomsStatuses.Codes.NoClearanceRequired, consignmentWithNonDepPackage.WRC_CustomsStatus);

			var outturnLog = new QueuedLogForTesting(NewFactory()) { SJ_ALogReference = log.PK, SJ_Reference = log.SL_Reference, SJ_ParentTableCode = CusOutturnSchema.Constants.Prefix, SJ_ParentID = outturn.PK, SJ_SE_NKEvent = AutoEvents.CustomsEntryStatusCode, SJ_EventTime = now.ToZDateTime() };

			//Process log
			using (outturnLog.Factory.AddDisposableService())
			{
				var subscriber = new CustomsStatusLogSubscriberForTest();
				subscriber.ProcessLogs(new IQueuedLog[] { outturnLog });

				outturnLog.Factory.Save();
			}

			var ediMessage = UniversalHelper.GetEDIMessageFromDB(outturn, AutoEvents.DataExportCode);

			var newFactory = new UniversalObjectFactory();
			var updatedRCN = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HOUSEBILL")).Single();
			var customsReleaseNumberReference = updatedRCN.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
			CombineAssertions(() =>
			{
				AssertNotNull(ediMessage);
				AssertEquals("EDIMessage record should exist for the universal event", EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
				AssertNotNull("Customs Release Number has been populated.", customsReleaseNumberReference);
				AssertEquals("Customs Release Number should be Customs Cleared", "Customs Cleared", customsReleaseNumberReference.CE_EntryNum);
				AssertEquals("RCN Customs Status should be Cleared", TransitWarehouseCustomsStatuses.Codes.Cleared, updatedRCN.WRC_CustomsStatus);
				AssertEquals("Customs Release Number should use the Customs category.", TransitWarehouseReferenceCategories.Codes.CustomsReference, customsReleaseNumberReference.CE_Category);
			});
		}

		#endregion

		#region Universal Shipments Tests

		#region TestImportSeaCargoOutturnShipmentToTW_PopulatesCustomsEntryNumberAndCustomsReleaseNumber

		public void TestImportSeaCargoOutturnShipmentToTW_PopulatesCustomsEntryNumberAndCustomsReleaseNumber()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PREMISEID");

			var outturnHeader = Factory.New<CusOutturnHeader>();
			/* fields used for matching when TWH sends shipment back to SeaCargoOutturn
				- C6_LloydsIMO
				- C6_VoyageNum
				- C6_OutturningPremiseID
			*/
			PopulateOutturnHeaderWithMatchingDataForImport(outturnHeader, warehouse, "PREMISEID", "LloydsN", "VoyageN", "Vessel123");
			CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse(outturnHeader);

			CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNT1234", "", "", 2, CMRPackageTypes.Codes.Package, "CONSOLIDATED CARGO SAFETY EQUIP, LIGHTIN G FIXTURES AUTO PARTS",
	19026, "KG", "Marks And Numbers", "SEA", 1000, "KG", 52.8m, "CU", "HENDRICKSON ASIA PACIFIC PTY LTD");
			CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1234", "MAS1234", 2, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
				1333.1m, "KG", "COOL DRIVE", "SEA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD", "CLR");
			Factory.Save();

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			var receiveConsignment = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1234")).Single();
			AssertEquals("VELCRO AUSTRALIA PTY LTD", receiveConsignment.ConsigneeDocAddress.CompanyName);
			AssertEquals("not specified", receiveConsignment.ConsigneeDocAddress.E2_Address1);
			AssertEquals("not specified", receiveConsignment.ConsigneeDocAddress.E2_City);
			AssertEquals(TransportModes.Sea, receiveConsignment.WRC_TransportMode);
			var receiveASN = Factory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertEquals(TransportModes.Sea, receiveASN.WRP_TransportMode);

			AssertEquals("MAS1234", receiveASN.MasterBillNumber);

			var packageState = receiveConsignment.PackageStates.Single();
			AssertEquals(receiveASN, packageState.ReceiveASN);

			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
			AssertEquals("CNT1234", rtu.WRH_VehicleReference);

			var additionalReferencesForRTU = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, rtu.PK));
			Helper.AssertAdditionalReferences(additionalReferencesForRTU, WarehouseAdditionalReferenceTypes.Codes.MasterBill, "MAS1234", WarehouseAdditionalReferenceTypes.Descriptions.MasterBill);
			Helper.AssertAdditionalReferences(additionalReferencesForRTU, WarehouseAdditionalReferenceTypes.Codes.VesselLloyds, "LloydsN", WarehouseAdditionalReferenceTypes.Descriptions.VesselLloyds);
			Helper.AssertAdditionalReferences(additionalReferencesForRTU, WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber, "VoyageN", WarehouseAdditionalReferenceTypes.Descriptions.VoyageFlightNumber);
			Helper.AssertAdditionalReferences(additionalReferencesForRTU, WarehouseAdditionalReferenceTypes.Codes.Vessel, "Vessel123", WarehouseAdditionalReferenceTypes.Descriptions.Vessel);

			var addtionalReferencesFromRTU = rtu.AdditionalReferenceNumbers;
			var universalLinkForRTU = UniversalJobLinkHelper.GetMatchingJobLinks(rtu, DataContextType.SeaCargoOutturn, null).Single();
			AssertEquals(outturnHeader.C6_SendersMessageReference, universalLinkForRTU.Key);

			var package = packageState.Package;
			AssertPackageProperties(package, qty: 2, packType: "PKG", goodsDescription: "SAFETY EQUIPMENT",
				marksAndNumbers: "COOL DRIVE", weight: 1333.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");

			AssertConsignmentAdditionalRefs(receiveConsignment, houseBill: "HSB1234", shipmentID: "", masterbill: "MAS1234");

			CombineAssertions(() =>
			{
				var customsEntryNumberReference = receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsNumber);
				var customsReleaseNumberReference = receiveConsignment.CustomsReferenceNumbers.First(TransitWarehouseCustomsReferenceTypes.Codes.CustomsReleaseNumber);
				AssertNotNull("Customs Entry Number should have been populated.", customsEntryNumberReference);
				AssertEquals("Customs Entry Number should be Customs Held.", "Customs Held", customsEntryNumberReference.CE_EntryNum);
				AssertEquals("Customs Entry Number should use the Customs category.", TransitWarehouseReferenceCategories.Codes.CustomsReference, customsEntryNumberReference.CE_Category);
				AssertNotNull("Customs Release Number has been populated.", customsReleaseNumberReference);
				AssertEquals("Customs Release Number should be Customs Cleared", "Customs Cleared", customsReleaseNumberReference.CE_EntryNum);
				AssertEquals("Customs Release Number should use the Customs category.", TransitWarehouseReferenceCategories.Codes.CustomsReference, customsReleaseNumberReference.CE_Category);
			});

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rtuInNewFactory = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
			AssertEquals(rtu.PK, rtuInNewFactory.PK);
			AssertEquals("CNT1234", rtu.WRH_VehicleReference);
			var universalLinkForRTUInNewFactory = UniversalJobLinkHelper.GetMatchingJobLinks(rtuInNewFactory, DataContextType.SeaCargoOutturn, null).Single();
			AssertEquals(outturnHeader.C6_SendersMessageReference, universalLinkForRTUInNewFactory.Key);

			AssertEquals(PkgUnit.Unit, rtu.PackageExtension.Package.KP_F3_NKPackType);
			AssertEquals(TransportUnitTypes.Container, rtu.WRH_UnitType);
			var rtuPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.Package.PK)).Single();
			AssertEquals(TransitWarehouseStatuses.Codes.Booked, rtuPackageState.WPS_Status);
			AssertEquals(true, rtuPackageState.WPS_IsHandlingUnit);
		}

		#endregion

		#region TestSeaCargoOutturnToTW_SingleContainer

		public void TestSeaCargoOutturnToTW_SingleContainer()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PREMISEID");

			var outturnHeader = Factory.New<CusOutturnHeader>();
			/* fields used for matching when TWH sends shipment back to SeaCargoOutturn
				- C6_LloydsIMO
				- C6_VoyageNum
				- C6_OutturningPremiseID
			*/
			PopulateOutturnHeaderWithMatchingDataForImport(outturnHeader, warehouse, "PREMISEID", "LloydsN", "VoyageN", "Vessel123");
			CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse(outturnHeader);

			CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNT1234", "", "", 2, CMRPackageTypes.Codes.Package, "CONSOLIDATED CARGO SAFETY EQUIP, LIGHTIN G FIXTURES AUTO PARTS",
				19026, "KG", "Marks And Numbers", "ROA", 1000, "KG", 52.8m, "CU", "HENDRICKSON ASIA PACIFIC PTY LTD");
			CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1234", "MAS1234", 2, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
				1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			Factory.Save();

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();
			AssertReceiveInstructions_SingleContainer(Factory);
			var containerCreated = Factory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertEquals("MAS1234", containerCreated.MasterBillNumber);

			AssertEquals("VoyageN", containerCreated.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber).Single());
			AssertEquals("LloydsN", containerCreated.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(WarehouseAdditionalReferenceTypes.Codes.VesselLloyds).Single());

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertReceiveInstructions_SingleContainer(newFactory);
			var containerAfterReimport = newFactory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertEquals("MAS1234", containerAfterReimport.MasterBillNumber);

			AssertEquals(containerCreated.PK, containerAfterReimport.PK);
			AssertEquals("VoyageN", containerAfterReimport.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber).Single());
			AssertEquals("LloydsN", containerAfterReimport.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(WarehouseAdditionalReferenceTypes.Codes.VesselLloyds).Single());
		}

		public void TestSeaCargoOutturnToTW_CreatesReceiveConsignmentsAndPackages_TWUnloadsSomePackages_ForwarderResends_ReceiveInstructions_WithoutError()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHC");

			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PREMISEID");

			var outturnHeader = Factory.New<CusOutturnHeader>();
			/* fields used for matching when TWH sends shipment back to SeaCargoOutturn
				- C6_LloydsIMO
				- C6_VoyageNum
				- C6_OutturningPremiseID
			*/
			PopulateOutturnHeaderWithMatchingDataForImport(outturnHeader, warehouse, "PREMISEID", "LloydsN", "VoyageN", "Vessel123");
			CreateWorkflowTemplateForCustomsToArrivalTransitWarehouse(outturnHeader);

			CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNT1234", "", "", 2, CMRPackageTypes.Codes.Package, "CONSOLIDATED CARGO SAFETY EQUIP, LIGHTIN G FIXTURES AUTO PARTS",
				19026, "KG", "Marks And Numbers", "ROA", 1000, "KG", 52.8m, "CU", "HENDRICKSON ASIA PACIFIC PTY LTD");
			CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1234", "MAS1234", 2, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
				1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			Factory.Save();

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();
			AssertReceiveInstructions_SingleContainer(Factory);
			var containerCreated = Factory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertEquals("MAS1234", containerCreated.MasterBillNumber);

			AssertEquals("VoyageN", containerCreated.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(WarehouseAdditionalReferenceTypes.Codes.VoyageFlightNumber).Single());
			AssertEquals("LloydsN", containerCreated.AdditionalReferenceNumbers.GetAllReferenceNumbersByType(WarehouseAdditionalReferenceTypes.Codes.VesselLloyds).Single());

			var rcn = Factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1234")).Single();
			AssertEquals("VELCRO AUSTRALIA PTY LTD", rcn.ConsigneeDocAddress.CompanyName);
			AssertEquals("ROA", rcn.WRC_TransportMode);

			var packageState = rcn.PackageStates.Single();

			var rtu = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
			AssertEquals("Container number should be CNT1234.", "CNT1234", rtu.WRH_VehicleReference);

			UnloadAndLabelPackage(packageState, rtu, "BOX1", setDetails: true);
			rcn.Reload();
			AssertEquals("There should be 2 package states after Unloading a package.", 2, rcn.PackageStates.Count);

			var consignor2 = TestHelper.CreateOrganisation("CR2");
			var consignee2 = TestHelper.CreateOrganisation("CE2");
			var consol = CreateConsol("MAS1234", vessel, "NZCHC", "AUADL");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			var shipment = CreateShipment(consol, "HSB1234", "NZCHC", "AUADL", today.AddDays(-1), today.AddDays(9), consignor2, consignee2);
			CreateOuterPackline(shipment, 10, PkgUnit.Pallet, reference: "Pack1");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcnAfterImport = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1234")).Single();
			AssertEquals("The consignment should be matched.", rcn.PK, rcnAfterImport.PK);
			AssertEquals("RCN Consignor should be set to Shipment Consignor.", consignor2.PK, rcnAfterImport.ConsignorDocAddress.OrganisationPK);
			AssertEquals("RCN Consignee should be set to Shipment Consignee.", consignee2.PK, rcnAfterImport.ConsigneeDocAddress.OrganisationPK);

			var consolAfterImport = newFactory.Load<ForwardingConsol>(consol.PK);
			var exportLogAfterImport = UniversalHelper.GetMostRecentExportLog(consolAfterImport, AutoEvents.DataExportCode);
			var ediMessageAfterImport = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport);
			AssertEquals("The shipment should have a warning while export.", EDIMessageStatusList.Codes.Warning, ediMessageAfterImport?.EM_Status);
			var importNote = ediMessageAfterImport.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("Log message must have a warning.",
@"Receive Consignment has been partially received. Package level data will be ignored and not read in.", importNote?.ST_NoteText);
		}

		void AssertReceiveInstructions_SingleContainer(BusinessObjectFactory factory)
		{
			var receiveConsignment = factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HSB1234")).Single();
			AssertEquals("VELCRO AUSTRALIA PTY LTD", receiveConsignment.ConsigneeDocAddress.CompanyName);
			AssertEquals("ROA", receiveConsignment.WRC_TransportMode);

			var rtu = factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery()).Single();
			AssertEquals("Container number should be CNT1234.", "CNT1234", rtu.WRH_VehicleReference);

			var pivot = factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery()).Single();
			var receiveASN = factory.Load<WhsItemReceiveASN>(new ZQuery()).Single();
			AssertEquals("MAS1234", receiveASN.MasterBillNumber);

			var packageState = receiveConsignment.PackageStates.Single();
			AssertEquals(receiveASN, packageState.ReceiveASN);
			AssertEquals(receiveASN.PK, pivot.WAR_WRP_TransitReceiveASN);
			AssertEquals(rtu.PK, pivot.WAR_WRH_TransitReceiveTransportationUnit);

			var package = packageState.Package;
			AssertPackageProperties(package, qty: 2, packType: "PKG", goodsDescription: "SAFETY EQUIPMENT",
				marksAndNumbers: "COOL DRIVE", weight: 1333.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3");

			AssertConsignmentAdditionalRefs(receiveConsignment, houseBill: "HSB1234", shipmentID: "", masterbill: "MAS1234");
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

		#region TestSeaCargoOutturnToTW_MultipleContainers

		public void TestSeaCargoOutturnToTW_MultipleContainers()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PREMISEID");

			var outturnHeader = Factory.New<CusOutturnHeader>();
			/* fields used for matching when TWH sends shipment back to SeaCargoOutturn
				- C6_LloydsIMO
				- C6_VoyageNum
				- C6_OutturningPremiseID
			*/
			PopulateOutturnHeaderWithMatchingDataForImport(outturnHeader, warehouse, "PREMISEID", "LloydsN", "VoyageN", "Vessel123");

			CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNT1234", "", "", 5, CMRPackageTypes.Codes.Package,
				"CONSOLIDATED CARGO SAFETY EQUIP, LIGHTIN G FIXTURES AUTO PARTS",
	19026, "KG", "Marks And Numbers", "ROA", 1000, "KG", 52.8m, "CU", "HENDRICKSON ASIA PACIFIC PTY LTD");
			CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1234", "MAS1234", 2, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
	1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");
			CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1235", "MAS1234", 3, CMRPackageTypes.Codes.PalletLift, "STC VALVES",
	861.37m, "KG", "HENDRICKSON 80909952", "SEA", 861.37m, "KG", 2.25m, "CU", "GIPPSLAND SEED SERVICES PTY LTD");

			CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNTABCD", "", "", 2, CMRPackageTypes.Codes.Package, "PNEUMATIC ENGINES",
	2160.95m, "KG", "Marks And Numbers", "ROA", 2160.95m, "KG", 6.17m, "CU", "MCCORMICK FOODS AUSTRALIA PTY LTD");
			CreateCargoPackLine(outturnHeader, "LCL", "CNTABCD", "HSBABCD", "MASABCD", 1, CMRPackageTypes.Codes.Parcel, "Bolts",
10.37m, "KG", "HENDRICKSON 80909952", "ROA", 12m, "KG", 1m, "CU", "ABCD BOLTS SERVICES PTY LTD");
			Factory.Save();

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();
			var rtus = Factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			var asns = Factory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals(2, rtus.Length);
			AssertEquals(2, asns.Length);

			AssertReceiveConsignmentAndASN(Factory, "HSB1234", "MAS1234", "VELCRO AUSTRALIA PTY LTD", "ROA", qty: 2, packType: "PKG", goodsDescription: "SAFETY EQUIPMENT",
				marksAndNumbers: "COOL DRIVE", weight: 1333.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3", "CNT1234", "MAS1234");

			AssertReceiveConsignmentAndASN(Factory, "HSB1235", "MAS1234", "GIPPSLAND SEED SERVICES PTY LTD", "SEA", qty: 3, packType: "PLT", goodsDescription: "STC VALVES",
	marksAndNumbers: "HENDRICKSON 80909952", weight: 861.37m, weightUQ: "KG", volume: 2.25m, volumeUQ: "M3", "CNT1234", "MAS1234");

			AssertReceiveConsignmentAndASN(Factory, "HSBABCD", "MASABCD", "ABCD BOLTS SERVICES PTY LTD", "ROA", qty: 1, packType: "PLT", goodsDescription: "Bolts",
marksAndNumbers: "HENDRICKSON 80909952", weight: 10.37m, weightUQ: "KG", volume: 1m, volumeUQ: "M3", "CNTABCD", "MASABCD");

			MasterFilesTestHelper.RunLogWalker();
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var rtusInNewFactory = newFactory.Load<WhsItemReceiveTransportationUnit>(new ZQuery());
			var asnsInNewFactory = newFactory.Load<WhsItemReceiveASN>(new ZQuery());
			AssertEquals(2, rtusInNewFactory.Length);
			AssertEquals(2, asnsInNewFactory.Length);

			AssertReceiveConsignmentAndASN(newFactory, "HSB1234", "MAS1234", "VELCRO AUSTRALIA PTY LTD", "ROA", qty: 2, packType: "PKG", goodsDescription: "SAFETY EQUIPMENT",
	marksAndNumbers: "COOL DRIVE", weight: 1333.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3", "CNT1234", "MAS1234");

			AssertReceiveConsignmentAndASN(newFactory, "HSB1235", "MAS1234", "GIPPSLAND SEED SERVICES PTY LTD", "SEA", qty: 3, packType: "PLT", goodsDescription: "STC VALVES",
	marksAndNumbers: "HENDRICKSON 80909952", weight: 861.37m, weightUQ: "KG", volume: 2.25m, volumeUQ: "M3", "CNT1234", "MAS1234");

			AssertReceiveConsignmentAndASN(newFactory, "HSBABCD", "MASABCD", "ABCD BOLTS SERVICES PTY LTD", "ROA", qty: 1, packType: "PLT", goodsDescription: "Bolts",
marksAndNumbers: "HENDRICKSON 80909952", weight: 10.37m, weightUQ: "KG", volume: 1m, volumeUQ: "M3", "CNTABCD", "MASABCD");
		}

		#endregion

		#region TestSeaCargoOutturnToTW_SingleContainer_MultipleMasterBillNumbers

		public void TestSeaCargoOutturnToTW_SingleContainer_MultipleMasterBillNumbers()
		{
			// Select first master bill number using alphabtical order.
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "PREMISEID");

			var outturnHeader = Factory.New<CusOutturnHeader>();
			/* fields used for matching when TWH sends shipment back to SeaCargoOutturn
				- C6_LloydsIMO
				- C6_VoyageNum
				- C6_OutturningPremiseID
			*/
			PopulateOutturnHeaderWithMatchingDataForImport(outturnHeader, warehouse, "PREMISEID", "LloydsN", "VoyageN", "Vessel123");

			CreateCargoPackLine(outturnHeader, CMRCargoTypes.Codes.FullContainerLoad, "CNT1234", "", "", 5, CMRPackageTypes.Codes.Package,
				"CONSOLIDATED CARGO SAFETY EQUIP, LIGHTIN G FIXTURES AUTO PARTS",
	19026, "KG", "Marks And Numbers", "ROA", 1000, "KG", 52.8m, "CU", "HENDRICKSON ASIA PACIFIC PTY LTD");
			CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1235", "B", 3, CMRPackageTypes.Codes.PalletLift, "STC VALVES",
861.37m, "KG", "HENDRICKSON 80909952", "SEA", 861.37m, "KG", 2.25m, "CU", "GIPPSLAND SEED SERVICES PTY LTD");
			CreateCargoPackLine(outturnHeader, "LCL", "CNT1234", "HSB1234", "A", 2, CMRPackageTypes.Codes.Package, "SAFETY EQUIPMENT",
	1333.1m, "KG", "COOL DRIVE", "ROA", 1333.1m, "KG", 2.79m, "CU", "VELCRO AUSTRALIA PTY LTD");

			Factory.Save();

			outturnHeader.Logs.AddNew(new EventValue(Events.BookingConfirmed, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			AssertReceiveConsignmentAndASN(Factory, "HSB1234", "A", "VELCRO AUSTRALIA PTY LTD", "ROA", qty: 2, packType: "PKG", goodsDescription: "SAFETY EQUIPMENT",
				marksAndNumbers: "COOL DRIVE", weight: 1333.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3", "CNT1234", "A");

			AssertReceiveConsignmentAndASN(Factory, "HSB1235", "B", "GIPPSLAND SEED SERVICES PTY LTD", "SEA", qty: 3, packType: "PLT", goodsDescription: "STC VALVES",
	marksAndNumbers: "HENDRICKSON 80909952", weight: 861.37m, weightUQ: "KG", volume: 2.25m, volumeUQ: "M3", "CNT1234", "A");

			MasterFilesTestHelper.RunLogWalker();
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertReceiveConsignmentAndASN(newFactory, "HSB1234", "A", "VELCRO AUSTRALIA PTY LTD", "ROA", qty: 2, packType: "PKG", goodsDescription: "SAFETY EQUIPMENT",
	marksAndNumbers: "COOL DRIVE", weight: 1333.1m, weightUQ: "KG", volume: 2.79m, volumeUQ: "M3", "CNT1234", "A");

			AssertReceiveConsignmentAndASN(newFactory, "HSB1235", "B", "GIPPSLAND SEED SERVICES PTY LTD", "SEA", qty: 3, packType: "PLT", goodsDescription: "STC VALVES",
	marksAndNumbers: "HENDRICKSON 80909952", weight: 861.37m, weightUQ: "KG", volume: 2.25m, volumeUQ: "M3", "CNT1234", "A");
		}

		void AssertReceiveConsignmentAndASN(BusinessObjectFactory factory, string houseBill, string masterBillNumberOnRCN, string consigneeName, string nextTransportMode,
			int qty, string packType, string goodsDescription, string marksAndNumbers,
			decimal weight, string weightUQ, decimal volume, string volumeUQ, string expectedContainerNumber, string masterBillNumberOnASN)
		{
			var rcnForHSB1234 = factory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, houseBill)).Single();
			AssertEquals(nextTransportMode, rcnForHSB1234.WRC_TransportMode);

			var rcnJobDocAddresses = factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, rcnForHSB1234.PK));
			var rcnBookingParty = rcnJobDocAddresses.Where(a => a.E2_AddressType == DocAddressTypes.Codes.BookingPartyDocumentaryAddress).FirstOrDefault();
			AssertNull("RCN created from Sea Cargo should have empty booking party", rcnBookingParty);
			var packageState = rcnForHSB1234.PackageStates.Single();

			var package = packageState.Package;
			AssertPackageProperties(package, qty, packType, goodsDescription, marksAndNumbers, weight, weightUQ, volume, volumeUQ);

			AssertConsignmentAdditionalRefs(rcnForHSB1234, houseBill: houseBill, shipmentID: "", masterbill: masterBillNumberOnRCN);

			var rtu = factory.Load<WhsItemReceiveTransportationUnit>(new ZQuery(WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference, expectedContainerNumber)).Single();
			var asn = factory.Load<WhsItemReceiveASN>(new ZQuery(WhsItemReceiveASNSchema.WRP_VehicleReference, expectedContainerNumber)).Single();
			AssertEquals(masterBillNumberOnASN, asn.MasterBillNumber);
			var pivot = factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, asn.PK)).Single();
			AssertEquals(asn, packageState.ReceiveASN);
			AssertEquals(rtu.PK, pivot.WAR_WRH_TransitReceiveTransportationUnit);

			if (rtu.IsContainerUnitType)
			{
				AssertEquals(PkgUnit.Unit, rtu.PackageExtension.Package.KP_F3_NKPackType);
				AssertEquals(TransportUnitTypes.Container, rtu.WRH_UnitType);
				var rtuPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.Package.PK)).Single();
				AssertEquals(TransitWarehouseStatuses.Codes.Booked, rtuPackageState.WPS_Status);
				AssertEquals(true, rtuPackageState.WPS_IsHandlingUnit);
			}
		}

		#endregion

		#endregion

		#region Implementation

		[Serializable]
		class CustomsStatusLogSubscriberForTest : Customs.Business.CustomsStatusLogSubscriber
		{
			public void ProcessLogs(IQueuedLog[] queuedLogs)
			{
				var allPrefix = TableNames.Select(c => EnterpriseSchema.GetTableSchema(c)?.PK.ColumnPrefix ?? string.Empty);
				var filterLogs = queuedLogs.Where(c => allPrefix.Any(d => d == c.SJ_ParentTableCode) && EventTypes.Any(d => d == c.SJ_SE_NKEvent)).ToArray();

				base.ProcessLogQueueItems(filterLogs);
			}
		}

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(Factory);

		#endregion
	}
}
