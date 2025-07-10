using CargoWise.Definitions;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeaderWorkflowDescriptor))]
	sealed class OrgHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<OrgHeaderWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(OrgHeaderWorkflowDescriptor.WorkflowTypeCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Organization", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public void TestSupportsWorkflowTriggerActionXML()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTriggerActionXML);
		}

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.EDICommunication; }
		}

		public void TestDocumentBusinessContext()
		{
			AssertContainsExactElementsInAnyOrder(new[] { BusinessContext.Organisation }, WorkflowDescriptor.DocumentBusinessContext);
		}

		public void TestGetWorkflowTriggerActionForStandardXmlActionType()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentAllocated.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;
			Factory.Save();

			var triggerLogBO = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			Factory.Save();

			var workFlowDescriptor = new OrgHeaderWorkflowDescriptor();
			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerLogBO, trigger));
			AssertEquals("XmlMessageDeliver", result.GetType().Name);
		}

		#region Implementation

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			EDICommunicationsMode communicationMode = orgHeader.EDICommunicationsModes.AddNew();
			communicationMode.EK_Module = OrgHeaderWorkflowDescriptor.WorkflowTypeCode;
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			communicationMode.EK_Destination = orgHeader.OH_FullNameTruncated + "@notificationemail.cargowise.com";

			return new IWorkflowProvider[] { orgHeader };
		}

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeOrganization;

		#endregion
	}
}
