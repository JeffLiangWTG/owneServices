using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteVehicleMovementWorkflowDescriptor))]
	public class GteVehicleMovementWorkflowDescriptorTest : WorkflowDescriptorTestCase<GteVehicleMovementWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Vehicle Movement", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.GteVehicleMovementWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			Assert(!WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			Assert(!WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			Assert(!WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			Assert(!WorkflowDescriptor.RequiresPort1);
			Assert(!WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			var subTypeInformation = WorkflowDescriptor.SubTypeInformation;

			AssertEquals(1, subTypeInformation.Length);
			AssertEquals("Facility Type", subTypeInformation[0].Description);

			var expectedTypes = new CodeDescriptionPairList();
			expectedTypes.AddPair(WarehouseTypes.Codes.Product, WarehouseTypes.Descriptions.Product);
			expectedTypes.AddPair(WarehouseTypes.Codes.ContainerYard, WarehouseTypes.Descriptions.ContainerYard);
			expectedTypes.AddPair(WarehouseTypes.Codes.Transit, WarehouseTypes.Descriptions.Transit);

			AssertContainsExactElementsInAnyOrder(expectedTypes, subTypeInformation[0].List);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var booking = gateMovement.GateMovementBooking.Booking;
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.DocAddressType = DocAddressType.BookingPartyDocumentaryAddress;
			address.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			address.Organisation.OH_Code = "R";
			var bookingPartyMode = address.Organisation.EDICommunicationsModes.AddNew();
			bookingPartyMode.EK_Module = WorkflowDescriptors.GteVehicleMovementWorkflowDescriptorCode;
			bookingPartyMode.EK_FileFormat = "NTF";
			bookingPartyMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			bookingPartyMode.EK_Destination = "@notificationemail.cargowise.com";
			booking.DocAddresses.Add(address);

			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			facility.WW_OA_WarehouseAddress = orgAddress.PK;
			orgAddress.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var bookingPartyMode2 = orgAddress.Header.EDICommunicationsModes.AddNew();
			bookingPartyMode2.EK_Module = WorkflowDescriptors.GteVehicleMovementWorkflowDescriptorCode;
			bookingPartyMode2.EK_FileFormat = "NTF";
			bookingPartyMode2.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			bookingPartyMode2.EK_Destination = "@notificationemail.cargowise.com";
			booking.GBK_WW_Facility = facility.PK;

			return new IWorkflowProvider[] { gateMovement.VehicleMovement };
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			var booking = (workflowProvider as GteVehicleMovement)?.GateMovements[0].GateMovementBooking.Booking;
			if (booking != null)
			{
				switch (partyTypeCode)
				{
					case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
						booking.Facility.WW_WarehouseType = WarehouseTypes.Codes.Transit;
						break;
					case MessageRecipientPartyTypeList.Codes.ContainerYard:
						booking.Facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
						break;
					default:
						break;
				}
			}
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(expected: true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
			=> base.ExpectedSupportedMessageRecipientParties
			| MessageRecipientPartyType.BookingParty
			| MessageRecipientPartyType.ArrivalTransitWarehouse
			| MessageRecipientPartyType.ContainerYard;

		protected override ZString[] ExpectedSupportedTriggerPartyServices(ZString recipient)
		{
			return (string)recipient switch
			{
				MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse => [ServiceCodesList.Codes.TransitWarehouseReceive, ServiceCodesList.Codes.TransitWarehouseDispatch],
				_ => []
			};
		}

		public void TestGetMessageRecipientPartyCollectionIncludesAllBookingsBookingParties()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			// Booking with unique address
			var gateMovement1 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement1.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleMovement.GateMovements.Add(gateMovement1);
			var booking1 = gateMovement1.GateMovementBooking.Booking;
			var address1 = Factory.NewWithValidTestData<JobDocAddress>();
			address1.DocAddressType = DocAddressType.BookingPartyDocumentaryAddress;
			address1.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			address1.Organisation.OH_Code = "R1";
			booking1.DocAddresses.Add(address1);
			var organisation1 = address1.Organisation;

			// Duplicated organisation
			var gateMovement2 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement2.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleMovement.GateMovements.Add(gateMovement2);
			var booking2 = gateMovement2.GateMovementBooking.Booking;
			var address2 = Factory.NewWithValidTestData<JobDocAddress>();
			address2.DocAddressType = DocAddressType.BookingPartyDocumentaryAddress;
			address2.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			address2.Organisation.OH_Code = "R2";
			booking2.DocAddresses.Add(address2);
			var organisation2 = address2.Organisation;

			var gateMovement3 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement3.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleMovement.GateMovements.Add(gateMovement3);
			var booking3 = gateMovement3.GateMovementBooking.Booking;
			var address3 = Factory.NewWithValidTestData<JobDocAddress>();
			address3.DocAddressType = DocAddressType.BookingPartyDocumentaryAddress;
			address3.OrganisationPK = address2.Organisation.PK;
			booking3.DocAddresses.Add(address3);

			var recipients = WorkflowDescriptor.GetMessageRecipientParty(vehicleMovement, MessageRecipientPartyTypeList.Codes.BookingParty);
			Assert("Should only be three recipients", recipients.Count() == 2);
			AssertCollectionContains("Recipients should contain correct organisations", organisation1, recipients.Select(x => x.Party));
			AssertCollectionContains("Recipients should contain correct organisations", organisation2, recipients.Select(x => x.Party));
		}

		public void TestGetMessageRecipientPartyCollectionOnlyReturnsBookingPartyAddress()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleMovement.GateMovements.Add(gateMovement);
			var booking = gateMovement.GateMovementBooking.Booking;
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.DocAddressType = DocAddressType.Acquirer;
			address.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			address.Organisation.OH_Code = "R";
			booking.DocAddresses.Add(address);
			var recipients = WorkflowDescriptor.GetMessageRecipientParty(vehicleMovement, MessageRecipientPartyTypeList.Codes.BookingParty);
			Assert("Should be no recipients", !(recipients.Any()));
		}

		public void TestGetMessageRecipientPartyCollection_GivenArrivalTransitWarehousePartyType_ReturnsAllMatchingTransitWarehouseOrganisation()
		{
			var booking1 = Factory.NewWithValidTestData<GteBooking>();
			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var gateMovement1 = Factory.NewWithValidTestData<GteGateMovement>();
			var facility1 = Factory.NewWithValidTestData<WhsWarehouse>();
			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			gateMovement1.GGM_GBM_MovementBooking = gateMovementBooking1.PK;
			gateMovementBooking1.GBM_GBK_Booking = booking1.PK;
			facility1.WW_OA_WarehouseAddress = address1.PK;
			facility1.WW_WarehouseType = "PRW";
			address1.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			booking1.GBK_WW_Facility = facility1.PK;

			var booking2 = Factory.NewWithValidTestData<GteBooking>();
			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var gateMovement2 = Factory.NewWithValidTestData<GteGateMovement>();
			var facility2 = Factory.NewWithValidTestData<WhsWarehouse>();
			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			gateMovement2.GGM_GBM_MovementBooking = gateMovementBooking2.PK;
			gateMovementBooking2.GBM_GBK_Booking = booking2.PK;
			facility2.WW_OA_WarehouseAddress = address2.PK;
			facility2.WW_WarehouseType = "TRW";
			address2.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			booking2.GBK_WW_Facility = facility2.PK;
			var expectedOrg1 = address2.Header;

			var booking3 = Factory.NewWithValidTestData<GteBooking>();
			var gateMovementBooking3 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var gateMovement3 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement3.GGM_GBM_MovementBooking = gateMovementBooking3.PK;
			gateMovementBooking3.GBM_GBK_Booking = booking3.PK;
			booking3.GBK_WW_Facility = Guid.Empty;

			var booking4 = Factory.NewWithValidTestData<GteBooking>();
			var gateMovementBooking4 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var gateMovement4 = Factory.NewWithValidTestData<GteGateMovement>();
			var facility4 = Factory.NewWithValidTestData<WhsWarehouse>();
			var address4 = Factory.NewWithValidTestData<OrgAddress>();
			gateMovement4.GGM_GBM_MovementBooking = gateMovementBooking4.PK;
			gateMovementBooking4.GBM_GBK_Booking = booking4.PK;
			facility4.WW_OA_WarehouseAddress = address4.PK;
			facility4.WW_WarehouseType = "TRW";
			address4.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			booking4.GBK_WW_Facility = facility4.PK;
			var expectedOrg2 = address4.Header;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			gateMovement1.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement2.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement3.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement4.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var recipients = WorkflowDescriptor.GetMessageRecipientParty(vehicleMovement, MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse).Select(x => x.Party);

			CombineAssertions("GetMessageRecipientParty contain the TRW facility organisation of every associated booking", () =>
			{
				AssertEquals(2, recipients.Count());
				AssertContainsExactElementsInAnyOrder([expectedOrg1, expectedOrg2], recipients);
			});
		}

		public void TestGetMessageRecipientPartyCollection_GivenContainerYardPartyTypes_MatchesAllAssociatedBookingContainerYardOrgs()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var gateMovement1 = Factory.NewWithValidTestData<GteGateMovement>();
			var gateMovement2 = Factory.NewWithValidTestData<GteGateMovement>();
			var gateMovement3 = Factory.NewWithValidTestData<GteGateMovement>();
			var gateMovement4 = Factory.NewWithValidTestData<GteGateMovement>();

			var facilityA = Factory.NewWithValidTestData<WhsWarehouse>();
			var facilityB = Factory.NewWithValidTestData<WhsWarehouse>();
			var facilityC = Factory.NewWithValidTestData<WhsWarehouse>();

			var addressA = Factory.NewWithValidTestData<OrgAddress>();
			var addressB = Factory.NewWithValidTestData<OrgAddress>();
			var addressC = Factory.NewWithValidTestData<OrgAddress>();

			var booking1 = gateMovement1.GateMovementBooking.Booking;
			var booking2 = gateMovement2.GateMovementBooking.Booking;
			var booking3 = gateMovement3.GateMovementBooking.Booking;
			var booking4 = gateMovement4.GateMovementBooking.Booking;

			gateMovement1.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement2.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement3.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement4.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			facilityA.WW_OA_WarehouseAddress = addressA.PK;
			facilityB.WW_OA_WarehouseAddress = addressB.PK;
			facilityC.WW_OA_WarehouseAddress = addressC.PK;

			addressA.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			addressB.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			addressC.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var containerYardOrgA = addressA.Header;
			var containerYardOrgC = addressC.Header;

			booking1.GBK_WW_Facility = ZGuid.Empty;
			booking2.GBK_WW_Facility = ZGuid.Empty;
			booking3.GBK_WW_Facility = ZGuid.Empty;
			booking4.GBK_WW_Facility = ZGuid.Empty;
			var recipients = WorkflowDescriptor.GetMessageRecipientParty(vehicleMovement, MessageRecipientPartyTypeList.Codes.ContainerYard).Select(x => x.Party);
			AssertEquals("Should have zero recipients when no bookings have a facility", 0, recipients.Count());

			booking1.GBK_WW_Facility = facilityA.PK;
			booking2.GBK_WW_Facility = facilityB.PK;
			booking3.GBK_WW_Facility = facilityC.PK;
			booking4.GBK_WW_Facility = facilityC.PK;
			recipients = WorkflowDescriptor.GetMessageRecipientParty(vehicleMovement, MessageRecipientPartyTypeList.Codes.ContainerYard).Select(x => x.Party);
			AssertEquals("Should have zero recipients when PartyType is 'Container Yard' and no facilities are a container yard", 0, recipients.Count());

			facilityA.WW_WarehouseType = "CYD";
			facilityC.WW_WarehouseType = "CYD";
			recipients = WorkflowDescriptor.GetMessageRecipientParty(vehicleMovement, MessageRecipientPartyTypeList.Codes.ContainerYard).Select(x => x.Party);

			CombineAssertions("GetMessageRecipientParty should match with correct facility organisation when PartyType is 'Container Yard' and facility is a container yard", () =>
			{
				AssertEquals(2, recipients.Count());
				AssertContainsExactElementsInAnyOrder([containerYardOrgA, containerYardOrgC], recipients);
			});
		}
	}
}
