using System.Linq;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchConsignmentWorkflowDescriptor))]
	class WhsItemDispatchConsignmentWorkflowDescriptorTest : WorkflowDescriptorTestCase<WhsItemDispatchConsignmentWorkflowDescriptor>
	{
		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Transit Dispatch Consignment", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.TransitDispatchConsignment, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.TransitDspConsignmnt,
				WorkflowDescriptor.DocumentBusinessContext.Single());
		}

		#endregion

		#region TestIncludeWorkflowTriggerActionXMLDebtorBalance

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
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

		#region TestRequiresWarehouse

		protected override bool RequiresWarehouseExpectedResult
		{
			get { return true; }
		}

		#endregion

		#region TestRequiresWarehouse

		protected override WarehouseCollectionType WarehouseTypeExpectedResult => WarehouseCollectionType.TransitWarehouse;

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
			AssertContainsExactElementsInAnyOrder("SupportedTriggerLineTypes", new[] { TriggerLineTypes.Codes.Service }, ((IWorkflowParentWithLines)WorkflowDescriptor).SupportedTriggerLineTypes);
		}

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.Forwarder |
					MessageRecipientPartyType.BookingParty |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email;
			}
		}

		#endregion

		#region SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);
			var consignment = (WhsItemDispatchConsignment)workflowProvider;
			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				consignment.ConsigneeDocAddress.E2_OA_Address = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
			else if (partyTypeCode == MessageRecipientPartyTypeList.Codes.BookingParty || partyTypeCode == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				consignment.BookingPartyDocAddress.E2_OA_Address = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
		}

		OrgHeader CreateOrgHeaderAndSetupEDICommunications()
		{
			var org = Factory.New<OrgHeader>();
			var orgMode = org.EDICommunicationsModes.AddNew();
			orgMode.EK_Module = WorkflowDescriptors.TransitDispatchConsignment;
			orgMode.EK_FileFormat = "NTF";
			orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			orgMode.EK_Destination = "@notificationemail.cargowise.com";
			return org;
		}

		#endregion

		#region GetParentsWithConfiguredOrganisationPartiesForTest

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<WhsItemDispatchConsignment>() };
		}

		#endregion

		#region TestSupportsSetFieldTriggerAction

		public void TestSupportsSetFieldTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		#endregion

		#region TestSupportsSendCRESAMessageTriggerAction

		public void TestSupportsSendCRESAMessageTriggerAction()
		{
			var parentBO = Factory.New<WhsItemDispatchConsignment>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCRESAMessage;

			var workFlowDescriptor = new WhsItemDispatchConsignmentWorkflowDescriptor();
			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(trigger.Logs.AddNew(Events.WorkflowTriggerEvent), trigger));

			Assert(result is TransitCRESAMessageProcessor<WhsItemDispatchConsignment>);
		}

		#endregion

		#region TestSupportsSendCIN750MessageTriggerAction

		public void TestSupportsSendCIN750MessageTriggerAction()
		{
			var parentBO = Factory.New<WhsItemDispatchConsignment>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCIN750Message;

			var workFlowDescriptor = new WhsItemDispatchConsignmentWorkflowDescriptor();
			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(trigger.Logs.AddNew(Events.WorkflowTriggerEvent), trigger));

			Assert(result is TransitCIN750MessageProcessor<WhsItemDispatchConsignment>);
		}

		#endregion

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get => new CodeDescriptionPair[] {
				new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendCRESAMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendCRESAMessage),
				new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendCIN750Message, WorkflowTriggerActionTypeConstants.Descriptions.SendCIN750Message),
			};
		}
	}
}
