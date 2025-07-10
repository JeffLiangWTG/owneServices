using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobServiceWorkflowDescriptor))]
	class JobServiceWorkflowDescriptorTest : WorkflowDescriptorTestCase<JobServiceWorkflowDescriptor>
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
			AssertEquals(WorkflowDescriptors.ServiceWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Service", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestGetWorkflowTriggerActionForUniversalEventXML

		public override void TestGetWorkflowTriggerActionForUniversalEventXML()
		{
			var dummy = GetParentsWithConfiguredOrganisationPartiesForTest()[0] as DummyConsignmentWithServices;
			var service = dummy.Services[0];

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.ServiceRequestedCode;
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.Service;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;

			AssertNotNull(
					"You must implement GetUniversalEventCollectionDataObjectWriter(IDataWritingManager) when you override SupportsWorkflowTriggerActionUniversalEventCollectionXML to 'true'.",
					new JobServiceWorkflowDescriptor().GetUniversalEventDataObjectWriter(ObjectFactory.New<IDataWritingManager>(new ActionWrapper(action, service, null), null, null, null)));
		}

		#endregion

		#region TestGetMessageRecipientParty

		public void TestGetMessageRecipientParty()
		{
			var dummy = (DummyConsignmentWithServices)GetParentsWithConfiguredOrganisationPartiesForTest()[0];
			var service = dummy.Services[0];

			var descriptor = new JobServiceWorkflowDescriptor();
			AssertEquals("Since BizO is null must not return recipient parties.", 0, descriptor.GetMessageRecipientParty(null, MessageRecipientPartyTypeList.Codes.Forwarder).Count());

			var messageRecipientPartyCollection = descriptor.GetMessageRecipientParty(service, MessageRecipientPartyTypeList.Codes.Forwarder);
			AssertEquals("Should return consignment booking party.", 1, messageRecipientPartyCollection.Count());

			var messageRecipientParty = messageRecipientPartyCollection.First();
			AssertEquals("Party", (dummy.BookingPartyDocAddress as JobDocAddress)?.Organisation, messageRecipientParty.Party);
			AssertEquals("FallbackEmail", "test@wisetechglobal.com", messageRecipientParty.FallbackEmail);
		}

		#endregion

		#region TestWorkflowProviderType

		public override void TestWorkflowProviderType()
		{
			AssertEquals(typeof(JobService), WorkflowDescriptor.WorkflowProviderType);
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

		#region TestSupportsWorkflowTemplates

		public override void TestSupportsWorkflowTemplates()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsWorkflowTemplates);
		}

		public void TestSupportsApplyWorkflowTemplate()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsApplyWorkflowTemplate);
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

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals("Should be true because we want to send universal events from service line level triggers.", true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Forwarder; }
		}

		#endregion

		#region TestGetWorkflowTriggerAction_ForNotificationEmailDelivery

		protected override BusinessObject SetupLineTriggerIfRequired(IWorkflowProvider parent, ProcessTask trigger)
		{
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.Service;
			return ((DummyConsignmentWithServices)parent).Services[0];
		}

		#endregion

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			DummyWithWorkflow.TypeDecider.TypeForLoadOverride = typeof(DummyConsignmentWithServices);

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.MainAddress.OA_Email = "test@wisetechglobal.com";

			var bookingPartyDocAddress = bookingParty.DocAddresses.AddNew();
			bookingPartyDocAddress.E2_OA_Address = bookingParty.MainAddress.PK;

			var bookingPartyMode = bookingParty.EDICommunicationsModes.AddNew();
			bookingPartyMode.EK_Module = WorkflowDescriptors.ServiceWorkflowDescriptorCode;
			bookingPartyMode.EK_FileFormat = "NTF";
			bookingPartyMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			bookingPartyMode.EK_Destination = "@notificationemail.cargowise.com";

			var dummy = Factory.New<DummyConsignmentWithServices>();
			dummy.BookingPartyDocAddressForTest = bookingPartyDocAddress;

			var service = dummy.Services.AddNew();
			service.ES_ParentTableCode = "Z1";

			return new IWorkflowProvider[] { dummy };
		}
	}
}
