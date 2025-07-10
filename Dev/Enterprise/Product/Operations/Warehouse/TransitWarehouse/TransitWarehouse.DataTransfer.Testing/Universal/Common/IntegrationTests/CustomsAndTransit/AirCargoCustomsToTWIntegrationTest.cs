using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class AirCargoCustomsToTWIntegrationTest : IntegrationTestCaseWithFactory
	{
		#region Universal Shipments Tests

		public void TestSendAirCargoReportToTW_CreatesReceiveConsignmentsAndPackages()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);

			var cusMAWB = CreateAirCargoReport(warehouse, "111-1111");

			var consignor = TestHelper.CreateOrganisation("CONSIGNOR", address1: "Consignor Address1");
			consignor.OH_FullName = "Consignor Company";
			consignor.MainAddress.City = "Auckland";

			var consignee = TestHelper.CreateOrganisation("CONSIGNEE", address1: "Consignee Address1");
			consignee.OH_FullName = "Consignee Company";
			consignee.MainAddress.City = "Sydney";

			var cusHAWBWithPieces = CreateAirCargoHouse(cusMAWB, "HB1", "MB1234", "NZAKL", "AUSYD", 10, consignor, consignee, "Air cargo pieces");
			cusHAWBWithPieces.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			cusHAWBWithPieces.CS_OA_ConsignorAddress = consignor.MainAddress.PK;
			var cusHAWBWithoutPieces = CreateAirCargoHouse(cusMAWB, "HB2", "MB1234", "NZAKL", "AUSYD", 0, consignor, consignee, "");
			cusHAWBWithoutPieces.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			cusHAWBWithoutPieces.CS_OA_ConsignorAddress = consignor.MainAddress.PK;

			// send UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			// Receive consignments and packages imported
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("2 receive consignments should have been created for two house bills.", 2, receiveConsignmentsAfterImport.Length);
			var receiveConsignment1 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB1");
			var receiveConsignment2 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB2");
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignment1.WRC_RL_NKNextDischargePort);
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignment2.WRC_RL_NKNextDischargePort);
			AssertEquals(TransportModes.Air, receiveConsignment1.WRC_TransportMode);
			AssertEquals(TransportModes.Air, receiveConsignment2.WRC_TransportMode);

			AssertJobDocAddress(receiveConsignment1.ConsignorDocAddress, companyName: "Consignor Company", address: "Consignor Address1", city: "Auckland");
			AssertJobDocAddress(receiveConsignment1.ConsigneeDocAddress, companyName: "Consignee Company", address: "Consignee Address1", city: "Sydney");
			AssertJobDocAddress(receiveConsignment1.BookingPartyDocAddress, companyName: "EDI CUSTOMS BROKERS", address: "10 HUTCHESON STREET", city: "Sydney");
			AssertJobDocAddress(receiveConsignment2.ConsignorDocAddress, companyName: "Consignor Company", address: "Consignor Address1", city: "Auckland");
			AssertJobDocAddress(receiveConsignment2.ConsigneeDocAddress, companyName: "Consignee Company", address: "Consignee Address1", city: "Sydney");
			AssertJobDocAddress(receiveConsignment2.BookingPartyDocAddress, companyName: "EDI CUSTOMS BROKERS", address: "10 HUTCHESON STREET", city: "Sydney");

			AssertConsignmentAdditionalRefs(receiveConsignment1, houseBill: "HB1", shipmentID: "", masterbill: "1111111");
			AssertConsignmentAdditionalRefs(receiveConsignment2, houseBill: "HB2", shipmentID: "", masterbill: "1111111");
			AssertEquals("RCN1 Destination", "AUSYD", receiveConsignment1.WRC_RL_NKDestination);
			AssertEquals("RCN2 Destination", "AUSYD", receiveConsignment2.WRC_RL_NKDestination);

			AssertEquals("1 packline created for the 10 pieces on HB1.", 1, receiveConsignment1.PackageStates.Count);
			AssertEquals("No packages created because HB2 has no pieces.", 0, receiveConsignment2.PackageStates.Count);

			var receiveConsignment1Package = receiveConsignment1.PackageStates.Select(ps => ps.Package).Single();
			AssertPackageProperties(receiveConsignment1Package, qty: 10, packType: "PCE", "Air cargo pieces");

			var universalLinkForRCN1 = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignment1, DataContextType.AirManifestLine, null).Single();
			var universalLinkForRCN2 = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignment2, DataContextType.AirManifestLine, null).Single();
			AssertEquals(cusHAWBWithPieces.CS_MessageReference, universalLinkForRCN1.Key);
			AssertEquals(cusHAWBWithoutPieces.CS_MessageReference, universalLinkForRCN2.Key);

			// resend UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			var newFactoryAfterReimport = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignmentsAfterReimport = newFactoryAfterReimport.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("Should have 2 receive consignments.", 2, receiveConsignmentsAfterReimport.Length);

			var receiveConsignment1AfterReimport = receiveConsignmentsAfterReimport.Single(rcn => rcn.WRC_ConsignmentID == "HB1");
			var receiveConsignment2AfterReimport = receiveConsignmentsAfterReimport.Single(rcn => rcn.WRC_ConsignmentID == "HB2");
			AssertEquals("Matched to the receive consignment from the previous import.", receiveConsignment1.PK, receiveConsignment1AfterReimport.PK);
			AssertEquals("Matched to the receive consignment from the previous import.", receiveConsignment2.PK, receiveConsignment2AfterReimport.PK);
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignment1AfterReimport.WRC_RL_NKNextDischargePort);
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignment2AfterReimport.WRC_RL_NKNextDischargePort);

			var universalLinkForRCN1AfterReimport = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignment1AfterReimport, DataContextType.AirManifestLine, null).Single();
			var universalLinkForRCN2AfterReimport = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignment2AfterReimport, DataContextType.AirManifestLine, null).Single();
			AssertEquals(cusHAWBWithPieces.CS_MessageReference, universalLinkForRCN1AfterReimport.Key);
			AssertEquals(cusHAWBWithoutPieces.CS_MessageReference, universalLinkForRCN2AfterReimport.Key);

			AssertJobDocAddress(receiveConsignment1AfterReimport.ConsignorDocAddress, companyName: "Consignor Company", address: "Consignor Address1", city: "Auckland");
			AssertJobDocAddress(receiveConsignment1AfterReimport.ConsigneeDocAddress, companyName: "Consignee Company", address: "Consignee Address1", city: "Sydney");
			AssertJobDocAddress(receiveConsignment1AfterReimport.BookingPartyDocAddress, companyName: "EDI CUSTOMS BROKERS", address: "10 HUTCHESON STREET", city: "Sydney");
			AssertJobDocAddress(receiveConsignment2AfterReimport.ConsignorDocAddress, companyName: "Consignor Company", address: "Consignor Address1", city: "Auckland");
			AssertJobDocAddress(receiveConsignment2AfterReimport.ConsigneeDocAddress, companyName: "Consignee Company", address: "Consignee Address1", city: "Sydney");
			AssertJobDocAddress(receiveConsignment2AfterReimport.BookingPartyDocAddress, companyName: "EDI CUSTOMS BROKERS", address: "10 HUTCHESON STREET", city: "Sydney");

			AssertConsignmentAdditionalRefs(receiveConsignment1AfterReimport, houseBill: "HB1", shipmentID: "", masterbill: "1111111");
			AssertConsignmentAdditionalRefs(receiveConsignment2AfterReimport, houseBill: "HB2", shipmentID: "", masterbill: "1111111");
			AssertEquals("RCN1 Destination", "AUSYD", receiveConsignment1AfterReimport.WRC_RL_NKDestination);
			AssertEquals("RCN2 Destination", "AUSYD", receiveConsignment2AfterReimport.WRC_RL_NKDestination);

			AssertEquals("1 packline created for the 10 pieces on HB1.", 1, receiveConsignment1AfterReimport.PackageStates.Count);
			AssertEquals("No packages created because HB2 has no pieces.", 0, receiveConsignment2AfterReimport.PackageStates.Count);

			var receiveConsignment1PackageAfterReimport = receiveConsignment1AfterReimport.PackageStates.Select(ps => ps.Package).Single();
			AssertPackageProperties(receiveConsignment1PackageAfterReimport, qty: 10, packType: "PCE", "Air cargo pieces");
		}

		public void TestSendAirCargoReportToTW_CreatesReceiveConsignmentsAndPackages_TWUnloadsSomePackages_ForwarderResends_ReceiveInstructions_WithoutError()
		{
			var (cfs, today, warehouse, consignor, consignee, vessel) = CreateTestData(true, "NZCHC");

			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);

			var cusMAWB = CreateAirCargoReport(warehouse, "111-1111");

			consignor.OH_FullName = "Consignor Company";
			consignor.MainAddress.City = "Auckland";

			consignee.OH_FullName = "Consignee Company";
			consignee.MainAddress.City = "Sydney";

			var cusHAWBWithPieces = CreateAirCargoHouse(cusMAWB, "HB1", "MB1234", "NZAKL", "AUSYD", 10, consignor, consignee, "Air cargo pieces");
			cusHAWBWithPieces.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			cusHAWBWithPieces.CS_OA_ConsignorAddress = consignor.MainAddress.PK;
			var cusHAWBWithoutPieces = CreateAirCargoHouse(cusMAWB, "HB2", "MB1234", "NZAKL", "AUSYD", 0, consignor, consignee, "");
			cusHAWBWithoutPieces.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			cusHAWBWithoutPieces.CS_OA_ConsignorAddress = consignor.MainAddress.PK;

			// send UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			// Receive consignments and packages imported
			var rcnsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("2 receive consignments should have been created for two house bills.", 2, rcnsAfterImport.Length);
			var rcn1 = rcnsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB1");
			var rcn2 = rcnsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB2");
			AssertEquals("Next discharge port imported.", "AUSYD", rcn1.WRC_RL_NKNextDischargePort);
			AssertEquals("Next discharge port imported.", "AUSYD", rcn2.WRC_RL_NKNextDischargePort);
			AssertEquals(TransportModes.Air, rcn1.WRC_TransportMode);
			AssertEquals(TransportModes.Air, rcn2.WRC_TransportMode);

			AssertConsignmentAdditionalRefs(rcn1, houseBill: "HB1", shipmentID: "", masterbill: "1111111");
			AssertConsignmentAdditionalRefs(rcn2, houseBill: "HB2", shipmentID: "", masterbill: "1111111");
			AssertEquals("RCN1 Destination", "AUSYD", rcn1.WRC_RL_NKDestination);
			AssertEquals("RCN2 Destination", "AUSYD", rcn2.WRC_RL_NKDestination);

			AssertEquals("1 packline created for the 10 pieces on HB1.", 1, rcn1.PackageStates.Count);
			AssertEquals("No packages created because HB2 has no pieces.", 0, rcn2.PackageStates.Count);

			var receiveConsignment1PackageState = rcn1.PackageStates.Single();
			var receiveConsignment1Package = receiveConsignment1PackageState.Package;
			AssertPackageProperties(receiveConsignment1Package, qty: 10, packType: "PCE", "Air cargo pieces");

			var universalLinkForRCN1 = UniversalJobLinkHelper.GetMatchingJobLinks(rcn1, DataContextType.AirManifestLine, null).Single();
			var universalLinkForRCN2 = UniversalJobLinkHelper.GetMatchingJobLinks(rcn2, DataContextType.AirManifestLine, null).Single();
			AssertEquals(cusHAWBWithPieces.CS_MessageReference, universalLinkForRCN1.Key);
			AssertEquals(cusHAWBWithoutPieces.CS_MessageReference, universalLinkForRCN2.Key);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);

			UnloadAndLabelPackage(receiveConsignment1PackageState, rtu, "BOX1", setDetails: true);
			rcn1.Reload();
			AssertEquals("There should be 2 package states after Unloading a package.", 2, rcn1.PackageStates.Count);

			var consignor2 = TestHelper.CreateOrganisation("CR2");
			var consignee2 = TestHelper.CreateOrganisation("CE2");
			var consol = CreateConsol("MB1234", vessel, "NZCHD", "AUADL");
			consol.JK_OA_UnpackDepotAddress = cfs.MainAddress.PK;
			var shipment = CreateShipment(consol, "HB1", "NZCHD", "AUADL", today.AddDays(-1), today.AddDays(9), consignor2, consignee2);
			CreateOuterPackline(shipment, 10, PkgUnit.Pallet, reference: "Pack1");

			TriggerAndFireTransitRequestUsingBookingRequested(consol);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rcn1AfterImport = newFactory.Load<WhsItemReceiveConsignment>(new ZQuery(WhsItemReceiveConsignmentSchema.WRC_ConsignmentID, "HB1")).Single();
			AssertEquals("The consignment should be matched.", rcn1.PK, rcn1AfterImport.PK);
			AssertEquals("RCN Consignor should be set to Shipment Consignor.", consignor2.PK, rcn1AfterImport.ConsignorDocAddress.OrganisationPK);
			AssertEquals("RCN Consignee should be set to Shipment Consignee.", consignee2.PK, rcn1AfterImport.ConsigneeDocAddress.OrganisationPK);

			var consolAfterImport = newFactory.Load<ForwardingConsol>(consol.PK);
			var exportLogAfterImport = UniversalHelper.GetMostRecentExportLog(consolAfterImport, AutoEvents.DataExportCode);
			var ediMessageAfterImport = UniversalHelper.GetEDIMessageFromDB(exportLogAfterImport);
			AssertEquals("The shipment should have a warning while export.", EDIMessageStatusList.Codes.Warning, ediMessageAfterImport?.EM_Status);
			var importNote = ediMessageAfterImport.Notes.GetAllNotesVisibleToCurrentCompany().SingleOrDefault();
			AssertContains("Log message must have a warning.",
@"Receive Consignment has been partially received. Package level data will be ignored and not read in.", importNote?.ST_NoteText);
		}

		public void TestSendAirCargoReportToTW_CreatesASN()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");

			CreateWorkflowTemplateForAirCargoCustomsToArrivalTransitWarehouse(JobInvoicingConsumerTypes.CusMAWB.Code);

			var cusMAWB = CreateAirCargoReport(warehouse, "111-1111");

			var consignor = TestHelper.CreateOrganisation("CONSIGNOR", address1: "Consignor Address1");
			consignor.OH_FullName = "Consignor Company";
			consignor.MainAddress.City = "Auckland";

			var consignee = TestHelper.CreateOrganisation("CONSIGNEE", address1: "Consignee Address1");
			consignee.OH_FullName = "Consignee Company";
			consignee.MainAddress.City = "Sydney";

			var cusHAWBWithPieces = CreateAirCargoHouse(cusMAWB, "HB1", "MB1234", "NZAKL", "AUSYD", 10, consignor, consignee, "Air cargo pieces");
			cusHAWBWithPieces.CS_OA_ConsigneeAddress = consignee.MainAddress.PK;
			cusHAWBWithPieces.CS_OA_ConsignorAddress = consignor.MainAddress.PK;

			// send UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			// Receive consignments and packages imported
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("1 receive consignments should have been created for two house bills.", 1, receiveConsignmentsAfterImport.Length);
			var receiveConsignment1 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB1");
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignment1.WRC_RL_NKNextDischargePort);
			AssertEquals(TransportModes.Air, receiveConsignment1.WRC_TransportMode);

			AssertJobDocAddress(receiveConsignment1.ConsignorDocAddress, companyName: "Consignor Company", address: "Consignor Address1", city: "Auckland");
			AssertJobDocAddress(receiveConsignment1.ConsigneeDocAddress, companyName: "Consignee Company", address: "Consignee Address1", city: "Sydney");
			AssertJobDocAddress(receiveConsignment1.BookingPartyDocAddress, companyName: "EDI CUSTOMS BROKERS", address: "10 HUTCHESON STREET", city: "Sydney");

			AssertConsignmentAdditionalRefs(receiveConsignment1, houseBill: "HB1", shipmentID: "", masterbill: "1111111");
			AssertEquals("RCN Destination", "AUSYD", receiveConsignment1.WRC_RL_NKDestination);
			AssertEquals("1 packline created for the 10 pieces on HB1.", 1, receiveConsignment1.PackageStates.Count);

			var receiveConsignment1Package = receiveConsignment1.PackageStates.Select(ps => ps.Package).Single();
			AssertPackageProperties(receiveConsignment1Package, qty: 10, packType: "PCE", "Air cargo pieces");

			var universalLinkForRCN1 = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignment1, DataContextType.AirManifestLine, null).Single();
			AssertEquals(cusHAWBWithPieces.CS_MessageReference, universalLinkForRCN1.Key);

			// resend UXML to Transit Warehouse
			TriggerAndFireTransitRequestForRelease(cusMAWB);

			var newFactoryAfterReimport = new BusinessObjectFactory() { RefreshEnabled = false };
			var receiveConsignmentsAfterReimport = newFactoryAfterReimport.Load<WhsItemReceiveConsignment>(new ZQuery());
			var receiveASN = Factory.Load<WhsItemReceiveASN>(new ZQuery()).SingleOrDefault();

			AssertNotNull("A single ASN should be created for the consol", receiveASN);
			AssertEquals("Should use Master Bill.", "1111111", receiveASN.WRP_VehicleReference);
			AssertEquals("Should have set the Booking Party.", GlbCompany.CurrentCompany.OrgProxy.PK, receiveASN.BookingPartyDocAddress?.Organisation.PK);
			AssertEquals("ASN's intended warehouse should be taken from consol (INTHEMSYD).", warehouse.PK, receiveASN.WRP_WW_IntendedWarehouse);

			AssertEquals("Should have 1 receive consignments.", 1, receiveConsignmentsAfterReimport.Length);

			var receiveConsignment1AfterReimport = receiveConsignmentsAfterReimport.Single(rcn => rcn.WRC_ConsignmentID == "HB1");
			AssertEquals("Matched to the receive consignment from the previous import.", receiveConsignment1.PK, receiveConsignment1AfterReimport.PK);
			AssertEquals("Next discharge port imported.", "AUSYD", receiveConsignment1AfterReimport.WRC_RL_NKNextDischargePort);

			var universalLinkForRCN1AfterReimport = UniversalJobLinkHelper.GetMatchingJobLinks(receiveConsignment1AfterReimport, DataContextType.AirManifestLine, null).Single();
			AssertEquals(cusHAWBWithPieces.CS_MessageReference, universalLinkForRCN1AfterReimport.Key);

			AssertJobDocAddress(receiveConsignment1AfterReimport.ConsignorDocAddress, companyName: "Consignor Company", address: "Consignor Address1", city: "Auckland");
			AssertJobDocAddress(receiveConsignment1AfterReimport.ConsigneeDocAddress, companyName: "Consignee Company", address: "Consignee Address1", city: "Sydney");
			AssertJobDocAddress(receiveConsignment1AfterReimport.BookingPartyDocAddress, companyName: "EDI CUSTOMS BROKERS", address: "10 HUTCHESON STREET", city: "Sydney");

			AssertConsignmentAdditionalRefs(receiveConsignment1AfterReimport, houseBill: "HB1", shipmentID: "", masterbill: "1111111");
			AssertEquals("1 packline created for the 10 pieces on HB1.", 1, receiveConsignment1AfterReimport.PackageStates.Count);
			AssertEquals("RCN Destination", "AUSYD", receiveConsignment1AfterReimport.WRC_RL_NKDestination);

			var receiveConsignment1PackageAfterReimport = receiveConsignment1AfterReimport.PackageStates.Select(ps => ps.Package).Single();
			AssertPackageProperties(receiveConsignment1PackageAfterReimport, qty: 10, packType: "PCE", "Air cargo pieces");
		}

		#endregion

		#region Air Cargo Depot Outturn Tests

		public void TestSendAirCargoDepotOutturnReportToTW_CreatesReceiveConsignments()
		{
			var (_, _, warehouse, _, _, _) = CreateTestData(false, "NZCHC");
			WhsTransitTestHelper.SetPremiseIDForWarehouse(warehouse, "9922W", CountryCodes.Australia);

			CreateWorkflowTemplateForAirCargoDepotOutturn();

			var outturn1 = CreateCusOutturn("HB3", "BG", 4);
			var outturn2 = CreateCusOutturn("HB4", "BG", 14);
			var outturn3 = CreateCusOutturn("HB5", "BG", 0);
			var aplVessel = CreateVessel("APL VESSEL", "9832343");

			var underbond = CreateCusUnderbond("OB1", "OR123", "9922W", aplVessel, new[] { outturn1, outturn2, outturn3 });

			TriggerAndFireTransitRequestForRelease(underbond);

			// Receive consignments imported
			var receiveConsignmentsAfterImport = Factory.Load<WhsItemReceiveConsignment>(new ZQuery());
			AssertEquals("3 receive consignments should have been created for two house bills.", 3, receiveConsignmentsAfterImport.Length);

			var receiveConsignment1 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB3");
			var receiveConsignment2 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB4");
			var receiveConsignment3 = receiveConsignmentsAfterImport.Single(rcn => rcn.WRC_ConsignmentID == "HB5");

			AssertEquals("Warehouse should be found from PremiseID.", warehouse.PK, receiveConsignment1.Warehouse.PK);
			AssertEquals("Warehouse should be found from PremiseID.", warehouse.PK, receiveConsignment2.Warehouse.PK);
			AssertEquals("1 packline created on HB3.", 1, receiveConsignment1.PackageStates.Count);
			AssertEquals("1 packline created on HB4.", 1, receiveConsignment2.PackageStates.Count);
			AssertEquals("1 packline created on HB5.", 1, receiveConsignment3.PackageStates.Count);

			var receiveConsignment1Package = receiveConsignment1.PackageStates.Select(ps => ps.Package).Single();
			var receiveConsignment2Package = receiveConsignment2.PackageStates.Select(ps => ps.Package).Single();
			var receiveConsignment3Package = receiveConsignment3.PackageStates.Select(ps => ps.Package).Single();

			AssertPackageProperties(receiveConsignment1Package, 4, "BG", string.Empty, false);
			AssertPackageProperties(receiveConsignment2Package, 14, "BG", string.Empty, false);
			AssertPackageProperties(receiveConsignment3Package, 0, "BG", string.Empty, true);

			var jobLinks = Factory.Load<StmUniversalJobLink>(new ZQuery());
			AssertNotNull(jobLinks);
			AssertEquals(3, jobLinks.Length);
			var jobLink1 = jobLinks.FirstOrDefault(j => j.UCL_ParentID == receiveConsignment1.PK);
			AssertEquals(nameof(DataContextType.UnderBond), jobLink1.UCL_SourceType);
		}

		#endregion

		TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(Factory);
	}
}
