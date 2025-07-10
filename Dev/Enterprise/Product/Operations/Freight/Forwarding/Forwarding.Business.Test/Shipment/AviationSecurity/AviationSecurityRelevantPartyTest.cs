using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class AviationSecurityRelevantPartyTest : TestCaseWithFactory
	{
		public void TestHumanReadableName()
		{
			SetUpEU();

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertEquals("Consignor (" + approvedAddress.Header.OH_Code + ")", party.HumanReadableName);

			approvedAddress.OA_OH = ZGuid.Empty;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertEquals("No error when OrgHeader is null", "Consignor", party.HumanReadableName);
		}

		public void TestConsignor()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertNull("Precondition", party);

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertEquals(true, party.IsAviationSecurityApproved);
			AssertEquals(approvedAddress.KnownShipper.PK, party.KnownShipperRecord.PK);

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertEquals(false, party.IsAviationSecurityApproved);
			AssertNull(party.KnownShipperRecord);

			AssertEquals("Consignor (HKCO1)", party.HumanReadableName);
		}

		public void TestConsignor_OverriddenAddress()
		{
			SetUpHK();

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_AddressOverride = true;

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);

			AssertEquals("Approval is false for overridden addresses", false, party.IsAviationSecurityApproved);
			AssertEquals("Human Readable Name", "Consignor", party.HumanReadableName);
			AssertNull(party.KnownShipperRecord);
		}

		public void TestConsignor_AU()
		{
			SetUpAU();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertNull("Precondition", party);

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertEquals(true, party.IsAviationSecurityApproved);

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertEquals("Human Readable Name", "Consignor (AUCO1)", party.HumanReadableName);
		}

		public void TestConsignor_Generic()
		{
			SetUpGeneric();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertNull("Precondition", party);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = approvedOrg.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertEquals(true, party.IsAviationSecurityApproved);

			shipment.ConsignorDocumentaryAddress.OrganisationPK = unapprovedOrg.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertEquals("Human Readable Name", "Consignor (GEN1)", party.HumanReadableName);
		}

		public void TestLocalClient()
		{
			SetUpHK();

			var job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.LocalClient);
			AssertNull("Precondition", party);

			job.JH_OA_LocalChargesAddr = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.LocalClient);
			AssertEquals(true, party.IsAviationSecurityApproved);

			job.JH_OA_LocalChargesAddr = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.LocalClient);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Local Client", party.HumanReadableName);
		}

		public void TestLocalClient_AU()
		{
			SetUpAU();

			var job = new JobHeader.Loader(shipment).TryCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.LocalClient);
			AssertNull("Precondition", party);

			job.JH_OA_LocalChargesAddr = approvedOrg.MainAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.LocalClient);
			AssertEquals(true, party.IsAviationSecurityApproved);

			job.JH_OA_LocalChargesAddr = unapprovedOrg.MainAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.LocalClient);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Local Client", party.HumanReadableName);
		}

		public void TestPickupTransportCompany()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany);
			AssertNull("Precondition", party);

			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany);
			AssertEquals(true, party.IsAviationSecurityApproved);

			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Shipment Pickup Transport Company", party.HumanReadableName);
		}

		public void TestPackingCFS()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS);
			AssertNull("Precondition", party);

			shipment.JS_OA_ExportReceivingDepot = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS);
			AssertEquals(true, party.IsAviationSecurityApproved);

			shipment.JS_OA_ExportReceivingDepot = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Shipment Packing CFS", party.HumanReadableName);
		}

		public void TestCoLoadMasterShipmentSendingForwarder()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.CoLoadMasterShipmentSendingForwarder);
			AssertNull("Precondition", party);

			var master = Factory.New<ForwardingShipment>();
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			master.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.CoLoadMasterShipmentSendingForwarder);
			AssertEquals(true, party.IsAviationSecurityApproved);

			master.ConsignorDocumentaryAddress.E2_OA_Address = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.CoLoadMasterShipmentSendingForwarder);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertEquals("Human Readable Name", "Co-Load Master Shipment Sending Forwarder (HKCO1)", party.HumanReadableName);
		}

		public void TestCoLoadMasterShipmentSendingForwarder_OverriddenAddress()
		{
			SetUpHK();

			var master = Factory.New<ForwardingShipment>();
			shipment.JS_JS_ColoadMasterShipment = master.PK;
			master.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;
			master.ConsignorDocumentaryAddress.E2_AddressOverride = true;

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.CoLoadMasterShipmentSendingForwarder);

			AssertEquals("Approval is false for overridden addresses", false, party.IsAviationSecurityApproved);
			AssertEquals("Human Readable Name", "Co-Load Master Shipment Sending Forwarder", party.HumanReadableName);
		}

		public void TestConsolSendingAgent()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolSendingAgent);
			AssertNull("Precondition", party);

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_SendingForwarderAddress = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolSendingAgent);
			AssertEquals(true, party.IsAviationSecurityApproved);

			consol.JK_OA_SendingForwarderAddress = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolSendingAgent);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Consol Sending Agent", party.HumanReadableName);
		}

		public void TestConsolAirline()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolAirline);
			AssertNull("Precondition", party);

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_ShippingLineAddress = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolAirline);
			AssertEquals(true, party.IsAviationSecurityApproved);

			consol.JK_OA_ShippingLineAddress = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolAirline);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Consol Airline", party.HumanReadableName);
		}

		public void TestConsolTransportCompany()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolTransportCompany);
			AssertNull("Precondition", party);

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_DeparturePackCFSTransportAddress = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolTransportCompany);
			AssertEquals(true, party.IsAviationSecurityApproved);

			consol.JK_OA_DeparturePackCFSTransportAddress = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolTransportCompany);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Consol Transport Company", party.HumanReadableName);
		}

		public void TestConsolCFS()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolCFS);
			AssertNull("Precondition", party);

			var consol = shipment.Consols.AddNew();
			consol.JK_OA_PackDepotAddress = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolCFS);
			AssertEquals(true, party.IsAviationSecurityApproved);

			consol.JK_OA_PackDepotAddress = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolCFS);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Consol CFS", party.HumanReadableName);
		}

		public void TestConsolCoLoaderAsForwarder()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization);
			AssertNull("Precondition", party);

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_OA_CreditorAddress = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization);
			AssertEquals(true, party.IsAviationSecurityApproved);

			consol.JK_OA_CreditorAddress = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Consol Co-Load With Organization", party.HumanReadableName);
		}

		public void TestConsolGatewayCoLoaderAsForwarder()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization);
			AssertNull("Precondition", party);

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_CreditorAddress = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization);
			AssertEquals(true, party.IsAviationSecurityApproved);

			consol.JK_OA_CreditorAddress = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Consol Co-Load With Organization", party.HumanReadableName);
		}

		public void TestConsolForwarderYouHaveBorrowedMAWBStockFrom()
		{
			SetUpHK();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolForwarderYouHaveBorrowedMAWBStockFrom);
			AssertNull("Precondition", party);

			var consol = shipment.Consols.AddNew();
			consol.JK_IsNeutralMaster = true;

			var mawb = Factory.New<JobMawb>();
			mawb.JM_Airline3DigitPrefix = "666";
			mawb.JM_MAWB = "10000001";
			mawb.JM_ParentID = consol.PK;
			mawb.JM_ParentTableCode = "JK";
			mawb.JM_OA_From = approvedOrgNewAddress.PK;

			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolForwarderYouHaveBorrowedMAWBStockFrom);
			AssertEquals(true, party.IsAviationSecurityApproved);

			mawb.JM_OA_From = unapprovedOrg.MainAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ConsolForwarderYouHaveBorrowedMAWBStockFrom);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertStartsWith("Human Readable Name", "Consol Forwarder you have borrowed MAWB Stock from", party.HumanReadableName);
		}

		public void TestShipmentPickupFromAddress()
		{
			SetUpEU();

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom);
			AssertNull("Precondition", party);

			shipment.ConsignorPickupAddress.E2_OA_Address = approvedAddress.PK;

			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom);
			AssertEquals(true, party.IsAviationSecurityApproved);

			shipment.ConsignorPickupAddress.E2_OA_Address = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom);
			AssertEquals(false, party.IsAviationSecurityApproved);

			AssertEquals("Human Readable Name", "Shipment Pickup From (FRCO)", party.HumanReadableName);
		}

		public void TestShipmentPickupFromAddress_OverriddenAddress()
		{
			SetUpEU();

			shipment.ConsignorPickupAddress.E2_OA_Address = approvedAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.ShipmentPickupFrom);
			AssertEquals("Approval is false for overridden addresses", false, party.IsAviationSecurityApproved);
			AssertEquals("Human Readable Name", "Shipment Pickup From", party.HumanReadableName);
		}

		public void TestApprovalFromOtherAddressIsUsedForOrgLevelApproval()
		{
			SetUpEU();

			var approval = approvedAddress.KnownShipperDetails[0];
			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = unapprovedAddress.PK;

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			Assert("AC party is approved even though approval is for the other OrgAddress", party.IsAviationSecurityApproved);
			AssertEquals(approvedAddress.KnownShipper.PK, party.KnownShipperRecord.PK);

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			Assert("AC party is still approved when the address with AC approval is selected", party.IsAviationSecurityApproved);
			AssertEquals(approvedAddress.KnownShipper.PK, party.KnownShipperRecord.PK);

			approval.OV_EXApprovedOrMajorExporter = "KC";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = unapprovedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			Assert("KC party is not approved when the approval is for another OrgAddress", !party.IsAviationSecurityApproved);
			AssertNull(party.KnownShipperRecord);

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;
			party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			Assert("KC party is approved when the address with KC approval is selected", !party.IsAviationSecurityApproved);
			AssertEquals(approvedAddress.KnownShipper.PK, party.KnownShipperRecord.PK);
		}

		public void TestIsApprovedToShipOnPassengerFlights()
		{
			SetUpEU();

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = unapprovedAddress.PK;
			var unapprovedParty = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			Assert("Unapproved org is not restricted from shipping on passenger flights", unapprovedParty.IsApprovedToShipOnPassengerFlights);

			var approval = approvedAddress.KnownShipperDetails[0];
			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;
			var acParty = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			Assert("Account Consignor is not approved to ship on passenger flights", !acParty.IsApprovedToShipOnPassengerFlights);

			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			var kcParty = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);
			Assert("Known Consignor is approved to ship on passenger flights", kcParty.IsApprovedToShipOnPassengerFlights);

			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
			Assert("Expired Known Consignor is not approved to ship on passenger flights", !kcParty.IsApprovedToShipOnPassengerFlights);
		}

		public void TestExpiryDateWillLapseBeforeDate()
		{
			SetUpHK();

			var approval = approvedAddress.KnownShipperDetails[0];
			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(5);
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = approvedAddress.PK;

			var party = AviationSecurityRelevantParty.New(shipment, SupplyChainSecurityOrganisationTypes.Consignor);

			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			AssertEquals("Approval is valid in one day", false, party.ExpiryDateWillLapseBeforeShipmentDateForAviationSecurity);

			shipment.JS_E_DEP = ZDate.Today.AddDays(10);
			AssertEquals("Approval is invalid in ten days", true, party.ExpiryDateWillLapseBeforeShipmentDateForAviationSecurity);
		}

		#region Implementation

		void SetUpHK()
		{
			GlbCompany.CurrentCompany.SetCountry("HK");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "HKCO1";

			unapprovedAddress = org.Addresses.AddNew();
			approvedAddress = org.Addresses.AddNew();

			var approval = approvedAddress.KnownShipperDetails.AddNew();
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "HKHKG";

			approvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			approvedOrg.OH_Code = "HKCO2";

			approvedOrgNewAddress = approvedOrg.Addresses.AddNew();
			approval = approvedOrgNewAddress.KnownShipperDetails.AddNew();
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;

			unapprovedOrg = Factory.NewWithValidTestData<OrgHeader>();
			unapprovedOrg.OH_Code = "HKCO3";
		}

		void SetUpEU()
		{
			GlbCompany.CurrentCompany.SetCountry("FR");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "FRCO";
			unapprovedAddress = org.Addresses.AddNew();
			approvedAddress = org.Addresses.AddNew();

			var approval = approvedAddress.KnownShipperDetails.AddNew();
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "FRCDG";
		}

		void SetUpAU()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "AUCO1";
			unapprovedAddress = org.Addresses.AddNew();
			approvedAddress = org.Addresses.AddNew();

			var approval = approvedAddress.KnownShipperDetails.AddNew();
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";

			approvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			approvedOrg.OH_Code = "AUCO2";
			approval = approvedOrg.MainAddress.KnownShipperDetails.AddNew();
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgentEACE;

			unapprovedOrg = Factory.NewWithValidTestData<OrgHeader>();
			unapprovedOrg.OH_Code = "AUCO3";
		}

		void SetUpGeneric()
		{
			GlbCompany.CurrentCompany.SetCountry("JM");

			unapprovedOrg = Factory.NewWithValidTestData<OrgHeader>();
			unapprovedOrg.OH_Code = "GEN1";

			approvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			approvedOrg.OH_Code = "GEN2";
			approvedOrg.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.Yes;

			shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry("AU");
		}

		ForwardingShipment shipment;
		OrgHeader approvedOrg;
		OrgHeader unapprovedOrg;
		OrgAddress approvedOrgNewAddress;
		OrgAddress approvedAddress;
		OrgAddress unapprovedAddress;

		#endregion
	}
}
