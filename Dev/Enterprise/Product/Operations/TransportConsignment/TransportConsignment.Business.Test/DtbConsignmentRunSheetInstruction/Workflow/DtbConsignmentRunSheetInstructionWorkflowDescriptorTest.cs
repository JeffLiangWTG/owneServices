using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentRunSheetInstructionWorkflowDescriptor))]
	sealed class DtbConsignmentRunSheetInstructionWorkflowDescriptorTest : WorkflowDescriptorTestCase<DtbConsignmentRunSheetInstructionWorkflowDescriptor>
	{
		#region TestAreTasksCompanySpecific

		protected override bool ExpectingTasksToBeCompanySpecific
		{
			get { return false; }
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.DtbConsignmentRunSheetInstructionWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Consignment Run Sheet Instruction", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestGetWorkflowTriggerAction_ForNotificationEmailDelivery

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var org = GetNewOrgHeaderWithEDICommunications();
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, org.MainAddress);
			var deliveryAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, org.MainAddress);

			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, new[] { pickupAddress.PickupAction, deliveryAddress.DeliveryAction });

			return new IWorkflowProvider[] { instruction };
		}

		#endregion

		#region TestWorkflowProviderType

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(DtbConsignmentRunSheetInstruction), WorkflowDescriptor.WorkflowProviderType);
		}

		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestSupportsWorkflowTemplates

		public override void TestSupportsWorkflowTemplates()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTemplates);
			AssertEquals(false, WorkflowDescriptor.SupportsApplyWorkflowTemplate);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region  TestSupportsBufferManagement

		public void TestSupportsBufferManagement()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsBufferManagement);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return MessageRecipientPartyType.ArrivalTransitWarehouse | MessageRecipientPartyType.DepartureTransitWarehouse;
			}
		}

		#endregion

		#region TestRecipients

		#region TestRecipients_DepotInstruction

		#region TestRecipients_DeliveryDepotInstruction

		public void TestRecipients_DeliveryDepotInstruction()
		{
			AssertRecipients_DepotInstruction(ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse);
		}

		#endregion

		#region TestRecipients_PickupDepotInstruction

		public void TestRecipients_PickupDepotInstruction()
		{
			AssertRecipients_DepotInstruction(ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse);
		}

		#endregion

		void AssertRecipients_DepotInstruction(string consignmentAddressType, string actionType, string partyType)
		{
			var org = GetNewOrgHeaderWithEDICommunications();
			var consignment = Helper.CreateConsignment();
			var address = Helper.CreateConsignmentAddressWithAction(consignment, consignmentAddressType, actionType, DocAddressType.LocalCartageCFS, org.MainAddress);

			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, actionType == ActionTypes.Codes.Delivery ? address.DeliveryAction : address.PickupAction);

			var depotRecipient = new DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(instruction, partyType).Select(recipient => recipient.Party);
			AssertEquals(org, depotRecipient.Single());

			var otherPartyType = MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse == partyType ? MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse : MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse;
			var otherDepotRecipients = new DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(instruction, otherPartyType).Select(recipient => recipient.Party);
			AssertEquals($"Instruction is not for {otherDepotRecipients}, therefore, other depot recipient must be null.", 0, otherDepotRecipients.Count());

			var recipientsFordifferentRecipientType = new DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(instruction, MessageRecipientPartyTypeList.Codes.DepartureCTO).Select(recipient => recipient.Party);
			AssertEquals($"Only Arrival and departure CFS recipients are valid.", 0, recipientsFordifferentRecipientType.Count());
		}

		#endregion

		#region TestRecipients_InstructionWithBothArrivalAndDepartureActions

		public void TestRecipients_InstructionWithBothArrivalAndDepartureActions()
		{
			var org = GetNewOrgHeaderWithEDICommunications();
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, org.MainAddress);
			var deliveryAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, org.MainAddress);

			var runSheet = Helper.CreateRunSheet();
			var instruction = Helper.CreateRunSheetInstruction(runSheet, new[] { pickupAddress.PickupAction, deliveryAddress.DeliveryAction });

			var departureDepotRecipient = new DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(instruction, MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse).Select(recipient => recipient.Party);
			AssertEquals(org, departureDepotRecipient.Single());

			var arrivalDepotRecipient = new DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(instruction, MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse).Select(recipient => recipient.Party);
			AssertEquals(org, departureDepotRecipient.Single());

			var differentRecipientType = new DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(instruction, MessageRecipientPartyTypeList.Codes.DepartureCTO).Select(recipient => recipient.Party);
			AssertEquals($"Only Arrival and departure CFS recipients are valid.", 0, differentRecipientType.Count());
		}

		#endregion

		#region TestRecipients_WithNonRunSheetInstructionBizObject

		public void TestRecipients_WithNonRunSheetInstructionBizObject()
		{
			var org = GetNewOrgHeaderWithEDICommunications();
			var consignment = Helper.CreateConsignment();
			var pickupAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS, org.MainAddress);
			var deliveryAddress = Helper.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.Delivery, ActionTypes.Codes.Delivery, DocAddressType.LocalCartageCFS, org.MainAddress);

			var runSheet = Helper.CreateRunSheet();
			var workflow = new DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed();

			var departureDepotRecipient = new DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed().GetMessageRecipientPartyExposed(runSheet, MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse).Select(recipient => recipient.Party);

			AssertEquals("Any non-DtbConsignmentRunSheetInstruction Object should be ignored and not be added to receipient list.", 0, departureDepotRecipient.Count());
		}

		#endregion

		OrgHeader GetNewOrgHeaderWithEDICommunications()
		{
			var mode = Factory.New<EDICommunicationsMode>();
			mode.EK_Module = "TRI";
			mode.EK_FileFormat = "NTF";
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			mode.EK_Destination = "@notificationemail.cargowise.com";

			var org = Factory.New<OrgHeader>();
			org.EDICommunicationsModes.Add(mode);

			return org;
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

		#region Helper

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion

		#region DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed

		class DtbConsignmentRunSheetInstructionWorkflowDescriptorExposed : DtbConsignmentRunSheetInstructionWorkflowDescriptor
		{
			public MessageRecipientPartyCollection GetMessageRecipientPartyExposed(BusinessObject workflowProvider, ZString partyType)
			{
				return GetMessageRecipientParty(workflowProvider, partyType);
			}
		}

		#endregion
	}
}
