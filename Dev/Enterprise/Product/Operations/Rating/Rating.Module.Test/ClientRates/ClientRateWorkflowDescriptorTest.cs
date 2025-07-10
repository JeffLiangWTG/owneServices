using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(ClientRateWorkflowDescriptor))]
	public class ClientRateWorkflowDescriptorTest : RatingHeaderWorkflowDescriptorTest<ClientRate, ClientRateWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.ClientRateWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Client Rate", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public void TestGetWorkflowTriggerAction()
		{
			var clientRate = Factory.New<ClientRate>();
			ProcessTask processTask = clientRate.WorkflowItems.Triggers.AddNew();

			ProcessTaskNotification notification = processTask.ProcessTaskNotifications.AddNew();
			var descriptor = new ClientRateWorkflowDescriptor();

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			AssertEquals(typeof(XmlMessageDeliver), descriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)).GetType());

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			var workflowTriggerNotification = (WorkflowTriggerNotification)descriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			AssertNotNull(workflowTriggerNotification.ExtraDataSubstitution);

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertNotNull(descriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory)));
		}

		public void TestShouldDeriveCorrectSubTypeForRateXML()
		{
			AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(ObjectFactory.GetType<IEDIMessage>()));

			var testRateEntry = Factory.NewWithValidTestData<RateEntry>();
			var testRatingHeader = Factory.Load<ClientRate>(testRateEntry.TI_TH);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			testRatingHeader.TH_OH = org.PK;

			testRatingHeader.TH_RateType = "SAL";

			var trigger = testRatingHeader.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			trigger.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Client;
			var mode = org.EDICommunicationsModes.AddNew();
			mode.EK_Module = WorkflowDescriptors.ClientRateWorkflowDescriptorCode;
			mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			mode.EK_Destination = "ANYWHERE";
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;

			testRatingHeader.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Precondition: This assertion is for actions that use the XmlMessageDeliver processor", processor is XmlMessageDeliver);

			MasterFilesTestHelper.RunLogWalker();

			var ediMessage = (IEDIMessage)Factory.LoadTop1(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
			AssertNotNull(ediMessage);
			AssertEquals(EDIMessageSubTypeList.Codes.Rates, ediMessage.EM_MessageSubType);
		}

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeRate;
	}
}
