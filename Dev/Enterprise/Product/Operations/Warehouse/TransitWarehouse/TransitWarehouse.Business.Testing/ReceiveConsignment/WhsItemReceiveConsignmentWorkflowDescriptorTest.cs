using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveConsignmentWorkflowDescriptor))]
	class WhsItemReceiveConsignmentWorkflowDescriptorTest : WorkflowDescriptorTestCase<WhsItemReceiveConsignmentWorkflowDescriptor>
	{
		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Transit Receive Consignment", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.TransitReceiveConsignment, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.TransitRcvConsignmnt, WorkflowDescriptor.DocumentBusinessContext.Single());
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

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
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
			var parentBO = Factory.New<WhsItemReceiveConsignment>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.FreightUnloadedCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCRESAMessage;

			var workFlowDescriptor = new WhsItemReceiveConsignmentWorkflowDescriptor();
			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(trigger.Logs.AddNew(Events.WorkflowTriggerEvent), trigger));

			Assert(result is TransitCRESAMessageProcessor<WhsItemReceiveConsignment>);
		}

		#endregion

		#region TestSupportsSendCIN750MessageTriggerAction

		public void TestSupportsSendCIN750MessageTriggerAction()
		{
			var parentBO = Factory.New<WhsItemReceiveConsignment>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.FreightUnloadedCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendCIN750Message;

			var workFlowDescriptor = new WhsItemReceiveConsignmentWorkflowDescriptor();
			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(trigger.Logs.AddNew(Events.WorkflowTriggerEvent), trigger));

			Assert(result is TransitCIN750MessageProcessor<WhsItemReceiveConsignment>);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Consignor |
					MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.BookingParty |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Forwarder |
					MessageRecipientPartyType.CustomsOutturnAgent |
					MessageRecipientPartyType.Email;
			}
		}

		#endregion

		#region TestAddToMessageRecipientPartyList

		public void TestAddToMessageRecipientPartyList_UnderBond()
		{
			var rcn = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			CreateJobLink(rcn, "AAA", "UnderBond");

			var workFlowDescriptor = new WhsItemReceiveConsignmentWorkflowDescriptor();
			var addresses = workFlowDescriptor.GetMessageRecipientParty(rcn, MessageRecipientPartyTypeList.Codes.CustomsOutturnAgent);
			AssertEquals(1, addresses.Count());
			AssertEquals("AAA", addresses.First().Party.OH_Code);
		}

		void CreateJobLink(WhsItemReceiveConsignment rcn, string orgHeaderCode, string sourceType)
		{
			var orgHeader = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgHeaderCode)).FirstOrDefault();
			if (orgHeader == null)
			{
				orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Code = orgHeaderCode;
				orgHeader.OH_FullName = orgHeaderCode + " Full Name";
			}

			var jobLink = Factory.NewWithValidTestData<StmUniversalJobLink>();
			jobLink.UCL_ParentID = rcn.PK;
			jobLink.UCL_ParentTableCode = WhsItemReceiveConsignmentSchema.Constants.Prefix;
			jobLink.UCL_SourceType = sourceType;
			jobLink.UCL_OH_Owner = orgHeader.PK;
		}

		#endregion

		#region TestSupportedTriggerLineTypes

		public void TestSupportedTriggerLineTypes()
		{
			AssertContainsExactElementsInAnyOrder("SupportedTriggerLineTypes", new[] { TriggerLineTypes.Codes.PkgPackage, TriggerLineTypes.Codes.Service }, ((IWorkflowParentWithLines)WorkflowDescriptor).SupportedTriggerLineTypes);
		}

		#endregion

		#region SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);
			var consignment = (WhsItemReceiveConsignment)workflowProvider;

			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.Consignor)
			{
				consignment.ConsignorDocAddress.E2_OA_Address = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
			else if (partyTypeCode == MessageRecipientPartyTypeList.Codes.Consignee)
			{
				consignment.ConsigneeDocAddress.E2_OA_Address = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
			else if (partyTypeCode == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				consignment.BookingPartyDocAddress.E2_OA_Address = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
			else if (partyTypeCode == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				consignment.BookingPartyDocAddress.E2_OA_Address = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
		}

		OrgHeader CreateOrgHeaderAndSetupEDICommunications()
		{
			var org = Factory.New<OrgHeader>();
			var orgMode = org.EDICommunicationsModes.AddNew();
			orgMode.EK_Module = WorkflowDescriptors.TransitReceiveConsignment;
			orgMode.EK_FileFormat = "NTF";
			orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			orgMode.EK_Destination = "@notificationemail.cargowise.com";
			return org;
		}

		#endregion

		#region GetParentsWithConfiguredOrganisationPartiesForTest

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<WhsItemReceiveConsignment>() };
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
