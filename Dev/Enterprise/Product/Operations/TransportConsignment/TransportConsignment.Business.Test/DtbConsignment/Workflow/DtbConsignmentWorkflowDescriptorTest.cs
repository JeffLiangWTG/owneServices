using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentWorkflowDescriptor))]
	sealed class DtbConsignmentWorkflowDescriptorTest : WorkflowDescriptorTestCase<DtbConsignmentWorkflowDescriptor>
	{
		#region TestAreTasksCompanySpecific

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		#endregion

		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Land Transport Consignment", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.DtbConsignmentWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestSupportsBufferManagement

		public void TestSupportsBufferManagement()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsBufferManagement);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy
					| MessageRecipientPartyType.Email
					| MessageRecipientPartyType.BookingParty
					| MessageRecipientPartyType.BillToParty
					| MessageRecipientPartyType.Consignor
					| MessageRecipientPartyType.Consignee
					| MessageRecipientPartyType.NotifyParty;
			}
		}

		#endregion

		#region TestDocumentContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.LTConsignment, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		#endregion

		#region TestRecipients

		public void TestAddToMessageRecipientPartyList()
		{
			var consignor = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var bookingParty = Factory.New<OrgHeader>();
			var billingParty = Factory.New<OrgHeader>();
			var notifyParty = Factory.New<OrgHeader>();

			var consignment = Helper.CreateConsignment();
			var address1 = Helper.CreateConsignmentAddressWithAction(consignment, OrganisationTypesList.Codes.CNR, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageExporter, consignor.MainAddress);
			var address2 = Helper.CreateConsignmentAddressWithAction(consignment, OrganisationTypesList.Codes.CNE, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageImporter, consignee.MainAddress);

			consignment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.OriginatingConsignorAddress).E2_OA_Address = consignor.MainAddress.PK;
			consignment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.FinalConsigneeAddress).E2_OA_Address = consignee.MainAddress.PK;
			consignment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ClientRequestedBillingParty).E2_OA_Address = billingParty.MainAddress.PK;
			consignment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.BookingPartyDocumentaryAddress).E2_OA_Address = bookingParty.MainAddress.PK;
			consignment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty).E2_OA_Address = notifyParty.MainAddress.PK;

			var consignors = new DtbConsignmentWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(consignment, MessageRecipientPartyTypeList.Codes.Consignor).Select(recipient => recipient.Party);
			AssertEquals(consignor, consignors.Single());

			var consignees = new DtbConsignmentWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(consignment, MessageRecipientPartyTypeList.Codes.Consignee).Select(recipient => recipient.Party);
			AssertEquals(consignee, consignees.Single());

			var billToParties = new DtbConsignmentWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(consignment, MessageRecipientPartyTypeList.Codes.BillToParty).Select(recipient => recipient.Party);
			AssertEquals(billingParty, billToParties.Single());

			var bookingParties = new DtbConsignmentWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(consignment, MessageRecipientPartyTypeList.Codes.BookingParty).Select(recipient => recipient.Party);
			AssertEquals(bookingParty, bookingParties.Single());

			var notifyParties = new DtbConsignmentWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(consignment, MessageRecipientPartyTypeList.Codes.NotifyParty).Select(recipient => recipient.Party);
			AssertEquals(notifyParty, notifyParties.Single());
		}

		#endregion

		#region Implementation

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var consignor = GetNewConfiguredOrgHeader();
			var consignee = GetNewConfiguredOrgHeader();
			var bookingParty = GetNewConfiguredOrgHeader();
			var billingParty = GetNewConfiguredOrgHeader();
			var notifyParty = GetNewConfiguredOrgHeader();

			var consignment = Helper.CreateConsignment();
			consignment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ClientRequestedBillingParty).E2_OA_Address = billingParty.MainAddress.PK;
			consignment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty).E2_OA_Address = notifyParty.MainAddress.PK;
			consignment.DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.BookingPartyDocumentaryAddress).E2_OA_Address = bookingParty.MainAddress.PK;

			Helper.CreateConsignmentAddressWithAction(consignment, OrganisationTypesList.Codes.CNR, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageExporter, consignor.MainAddress);
			Helper.CreateConsignmentAddressWithAction(consignment, OrganisationTypesList.Codes.CNE, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageImporter, consignee.MainAddress);

			return new IWorkflowProvider[] { consignment };
		}

		OrgHeader GetNewConfiguredOrgHeader()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Module = "LTC";
			mode.EK_FileFormat = "NTF";
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "@notificationemail.cargowise.com";

			var org = Factory.New<OrgHeader>();
			org.EDICommunicationsModes.Add(mode);

			return org;
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		class DtbConsignmentWorkflowDescriptorExposed : DtbConsignmentWorkflowDescriptor
		{
			public MessageRecipientPartyCollection GetMessageRecipientPartyExposed(BusinessObject workflowProvider, ZString partyType)
			{
				return GetMessageRecipientParty(workflowProvider, partyType);
			}
		}

		#endregion
	}
}
