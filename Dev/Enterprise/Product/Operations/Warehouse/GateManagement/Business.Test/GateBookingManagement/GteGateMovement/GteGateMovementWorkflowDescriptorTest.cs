using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteGateMovementWorkflowDescriptor))]
	public class GteGateMovementWorkflowDescriptorTest : WorkflowDescriptorTestCase<GteGateMovementWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Gate Movement", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.GteGateMovementWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public void TestWarehouseName()
		{
			AssertEquals("Facility", WorkflowDescriptor.WarehouseName);
		}

		protected override bool RequiresWarehouseExpectedResult => true;

		protected override WarehouseCollectionType WarehouseTypeExpectedResult
			=> WarehouseCollectionType.ProductWarehouse
			| WarehouseCollectionType.CYDWarehouse
			| WarehouseCollectionType.TransitWarehouse;

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
			var gteGateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var booking = gteGateMovement.GateMovementBooking.Booking;
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.DocAddressType = DocAddressType.BookingPartyDocumentaryAddress;
			address.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			address.Organisation.OH_Code = "R";
			var bookingPartyMode = address.Organisation.EDICommunicationsModes.AddNew();
			bookingPartyMode.EK_Module = WorkflowDescriptors.GteGateMovementWorkflowDescriptorCode;
			bookingPartyMode.EK_FileFormat = "NTF";
			bookingPartyMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			bookingPartyMode.EK_Destination = "@notificationemail.cargowise.com";
			booking.DocAddresses.Add(address);

			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			facility.WW_OA_WarehouseAddress = orgAddress.PK;
			orgAddress.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var bookingPartyMode2 = orgAddress.Header.EDICommunicationsModes.AddNew();
			bookingPartyMode2.EK_Module = WorkflowDescriptors.GteGateMovementWorkflowDescriptorCode;
			bookingPartyMode2.EK_FileFormat = "NTF";
			bookingPartyMode2.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			bookingPartyMode2.EK_Destination = "@notificationemail.cargowise.com";
			booking.GBK_WW_Facility = facility.PK;

			return new IWorkflowProvider[] { gteGateMovement };
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			var booking = (workflowProvider as GteGateMovement)?.GateMovementBooking.Booking;
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

		public void TestGetMessageRecipientPartyCollectionIncludesBookingPartyWhenExists()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var booking = gateMovement.GateMovementBooking.Booking;
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.DocAddressType = DocAddressType.BookingPartyDocumentaryAddress;
			address.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			address.Organisation.OH_Code = "R";
			booking.DocAddresses.Add(address);
			var organisation = address.Organisation;
			var recipients = WorkflowDescriptor.GetMessageRecipientParty(gateMovement, MessageRecipientPartyTypeList.Codes.BookingParty);
			Assert("Should only be one recipient", recipients.Count() == 1);
			AssertEquals("Recipient should be GteBooking BookingPartyDocumentaryAddress", organisation, recipients.First().Party);
		}

		public void TestGetMessageRecipientPartyCollectionEmptyWhenNoBookingPartyAddress()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var recipients = WorkflowDescriptor.GetMessageRecipientParty(gateMovement, MessageRecipientPartyTypeList.Codes.BookingParty);
			Assert("Should not be any recipients", !(recipients.Any()));
		}

		public void TestGetMessageRecipientPartyCollectionOnlyReturnsBookingPartyAddress()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var booking = gateMovement.GateMovementBooking.Booking;
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.DocAddressType = DocAddressType.Acquirer;
			address.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			address.Organisation.OH_Code = "R";
			booking.DocAddresses.Add(address);
			var recipients = WorkflowDescriptor.GetMessageRecipientParty(gateMovement, MessageRecipientPartyTypeList.Codes.BookingParty);
			Assert("Should be no recipients", !(recipients.Any()));
		}

		public void TestGetMessageRecipientPartyCollection_GivenArrivalTransitWarehousePartyType_ReturnsCorrectTransitWarehouseOrganisation()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			var address = Factory.NewWithValidTestData<OrgAddress>();

			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			facility.WW_OA_WarehouseAddress = address.PK;
			facility.WW_WarehouseType = "PRW";
			address.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var organisation = address.Header;

			booking.GBK_WW_Facility = facility.PK;
			var recipients = WorkflowDescriptor.GetMessageRecipientParty(gateMovement, MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse);
			AssertEquals("Should have zero recipients when PartyType is ArrivalTransitWarehouse and Facility is not a Transit Warehouse", 0, recipients.Count());

			facility.WW_WarehouseType = "TRW";
			recipients = WorkflowDescriptor.GetMessageRecipientParty(gateMovement, MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse);

			CombineAssertions("GetMessageRecipientParty should match with correct facility organisation PartyType is ArrivalTransitWarehouse and Facility is a Transit Warehouse", () =>
			{
				AssertEquals(1, recipients.Count());
				AssertEquals(organisation, recipients.First().Party);
			});
		}

		public void TestGetMessageRecipientPartyCollection_GivenContainerYardPartyType_ReturnsCorrectContainerYardOrg()
		{
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			var booking = gateMovement.GateMovementBooking.Booking;
			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			var address = Factory.NewWithValidTestData<OrgAddress>();

			facility.WW_OA_WarehouseAddress = address.PK;
			address.OA_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var organisation = address.Header;

			booking.GBK_WW_Facility = facility.PK;
			var recipients = WorkflowDescriptor.GetMessageRecipientParty(gateMovement, MessageRecipientPartyTypeList.Codes.ContainerYard);
			AssertEquals("Should have zero recipients when PartyType is 'Container Yard' and facility is not a container yard", 0, recipients.Count());

			facility.WW_WarehouseType = "CYD";
			recipients = WorkflowDescriptor.GetMessageRecipientParty(gateMovement, MessageRecipientPartyTypeList.Codes.ContainerYard);

			CombineAssertions("GetMessageRecipientParty should match with correct facility organisation when PartyType is 'Container Yard' and facility is a container yard", () =>
			{
				AssertEquals(1, recipients.Count());
				AssertEquals(organisation, recipients.First().Party);
			});
		}
	}
}
