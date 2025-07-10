using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetWorkflowDescriptor))]
	sealed class DtbConsignmentRunSheetWorkflowDescriptorTest : WorkflowDescriptorTestCase<DtbConsignmentRunSheetWorkflowDescriptor>
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
			AssertEquals("Transport Run Sheet", WorkflowDescriptor.Description);
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
			AssertEquals(WorkflowDescriptors.DtbConsignmentRunSheetWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestWorkflowProviderType

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(DtbConsignmentRunSheet), WorkflowDescriptor.WorkflowProviderType);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.OrgProxy
					| MessageRecipientPartyType.Email
					| MessageRecipientPartyType.TransportCo
					| MessageRecipientPartyType.DepartureTransitWarehouse
					| MessageRecipientPartyType.ArrivalTransitWarehouse;
			}
		}

		#endregion

		#region TestExpectedSupportedTriggerPartyServices

		protected override ZString[] ExpectedSupportedTriggerPartyServices(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseDispatch };
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive };
				default:
					return base.ExpectedSupportedTriggerPartyServices(recipient);
			}
		}

		#endregion

		#region TestDocumentContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.DtbConsignRunSheet, WorkflowDescriptor.DocumentBusinessContext.Single());
		}

		#endregion

		#region TestRecipients

		public void TestAddToMessageRecipientPartyList()
		{
			var runSheet = Helper.CreateRunSheet();
			var transportCo = Helper.CreateOrganisation("RAR");
			runSheet.KG_OH_TransportCo = transportCo.PK;
			AssertEquals(transportCo.PK, runSheet.KG_OH_TransportCo);

			var orgs = new DtbConsignmentRunSheetWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(runSheet, MessageRecipientPartyTypeList.Codes.TransportCo).Select(recipient => recipient.Party);
			AssertEquals(1, orgs.Count());
			AssertCollectionContains(transportCo, orgs);
		}

		#region TestAddToMessageRecipientPartyList_TransitWarehouse

		public void TestAddToMessageRecipientPartyList_TransitWarehouse()
		{
			AssertRecipientOrg_DepotInstruction(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
			AssertRecipientOrg_DepotInstruction(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
		}

		void AssertRecipientOrg_DepotInstruction(string consignmentAddressType, string actionType)
		{
			var cfs = Helper.CreateOrganisation("CFS");
			var consignment = ConsignmentHelper.CreateConsignment();
			var address = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, consignmentAddressType, actionType, DocAddressType.LocalCartageCFS, cfs.MainAddress);

			var otherConsignmentAddressType = consignmentAddressType == ConsignmentAddressTypes.Codes.Delivery ? ConsignmentAddressTypes.Codes.PickUp : ConsignmentAddressTypes.Codes.Delivery;
			var otherActionType = actionType == ActionTypes.Codes.Delivery ? ActionTypes.Codes.PickUp : ActionTypes.Codes.Delivery;
			var otherAddress = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, otherConsignmentAddressType, otherActionType, DocAddressType.LocalCartageCFS, cfs.MainAddress);

			var runSheet = ConsignmentHelper.CreateRunSheet();
			var instruction = ConsignmentHelper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address.DeliveryAction : address.PickupAction);
			var otherTypeOfInstruction = ConsignmentHelper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? otherAddress.PickupAction : otherAddress.DeliveryAction);

			var partyType = actionType == ActionTypes.Codes.Delivery ? MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse : MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse;
			var recipientOrg = new DtbConsignmentRunSheetWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(runSheet, partyType).Select(recipient => recipient.Party).Single();
			AssertCollectionContains("Recipient org must be CFS.", cfs, recipientOrg);
		}

		#endregion

		#region TestAddToMessageRecipientPartyList_TransitWarehouse_MoreThanOneDepotAddress

		public void TestAddToMessageRecipientPartyList_TransitWarehouse_MoreThanOneDepotAddress()
		{
			AssertRecipientOrg_DepotInstructionMoreThanOneDepotAddress(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery);
			AssertRecipientOrg_DepotInstructionMoreThanOneDepotAddress(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp);
		}

		void AssertRecipientOrg_DepotInstructionMoreThanOneDepotAddress(string consignmentAddressType, string actionType)
		{
			var cfs = Helper.CreateOrganisation("CFS");
			var consignment = ConsignmentHelper.CreateConsignment();
			var address1 = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, consignmentAddressType, actionType, DocAddressType.LocalCartageCFS, cfs.MainAddress);
			var address2 = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, consignmentAddressType, actionType, DocAddressType.LocalCartageCFS, cfs.MainAddress);

			var runSheet = ConsignmentHelper.CreateRunSheet();
			var depotInstruction1 = ConsignmentHelper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address1.DeliveryAction : address1.PickupAction);
			var depotInstruction2 = ConsignmentHelper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address2.DeliveryAction : address2.PickupAction);

			var partyType = actionType == ActionTypes.Codes.Delivery ? MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse : MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse;
			var recipientOrgs = new DtbConsignmentRunSheetWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(runSheet, partyType).Select(recipient => recipient.Party);
			AssertEquals("since there are two depot instructions, there must not be a recipient org.", 0, recipientOrgs.Count());
		}

		#endregion

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
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

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region TestSupportedTriggerLineTypes

		public void TestSupportedTriggerLineTypes()
		{
			AssertContainsExactElementsInAnyOrder("RunSheetInstruction line trigger type must be returned.",
				TriggerLineTypes.Codes.RunSheetInstruction, ((IWorkflowParentWithLines)WorkflowDescriptor).SupportedTriggerLineTypes.Single());
		}

		#endregion

		#region TestSupportsBufferManagement

		public void TestSupportsBufferManagement()
		{
			AssertEquals("We want our customers to be able to use PAVE with run sheets.", true, WorkflowDescriptor.SupportsBufferManagement);
		}

		#endregion

		#region Implementation

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var cfs = GetNewConfiguredOrgHeader();
			var consignment = ConsignmentHelper.CreateConsignment();
			var pickupAddress = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, cfs.MainAddress);
			var deliveryAddress = ConsignmentHelper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, cfs.MainAddress);

			var runSheet = ConsignmentHelper.CreateRunSheet();
			var pickupInstruction = ConsignmentHelper.CreateRunSheetInstruction(runSheet, pickupAddress.PickupAction);
			var deliveryInstruction = ConsignmentHelper.CreateRunSheetInstruction(runSheet, deliveryAddress.DeliveryAction);

			var transportCo = GetNewConfiguredOrgHeader();
			runSheet.KG_OH_TransportCo = transportCo.PK;

			return new IWorkflowProvider[] { runSheet };
		}

		OrgHeader GetNewConfiguredOrgHeader()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Module = "TRS";
			mode.EK_FileFormat = "NTF";
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "@notificationemail.cargowise.com";

			var org = Factory.New<OrgHeader>();
			org.EDICommunicationsModes.Add(mode);

			return org;
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		TransportConsignmentTestHelper ConsignmentHelper
		{
			get { return consignmentHelper ?? (consignmentHelper = new TransportConsignmentTestHelper(Factory)); }
		}
		TransportConsignmentTestHelper consignmentHelper;

		class DtbConsignmentRunSheetWorkflowDescriptorExposed : DtbConsignmentRunSheetWorkflowDescriptor
		{
			public MessageRecipientPartyCollection GetMessageRecipientPartyExposed(BusinessObject workflowProvider, ZString partyType)
			{
				return GetMessageRecipientParty(workflowProvider, partyType);
			}
		}

		#endregion
	}
}
