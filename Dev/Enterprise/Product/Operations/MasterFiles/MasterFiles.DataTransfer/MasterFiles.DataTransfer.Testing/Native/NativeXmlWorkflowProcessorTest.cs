using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.MessageDelivery.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Native.Testing
{
	public class NativeXmlWorkflowProcessorTest : TestCaseWithFactory
	{
		Lazy<MessageProcessorCommunicationModesResult> GetModes(IList<IEDICommunicationsMode> modes)
		{
			return Lazy.Create(() => new MessageProcessorCommunicationModesResult(modes, null));
		}

		public void TestOrgHeaderGetsProcessedProperly()
		{
			using (Factory.AddDisposableService())
			{
				ObjectFactory.Get<IXMLDataProvider>().SetIgnoreTimestampForTest(false);
				TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				organisation.OH_Code = "ORGCODEME";

				var workflowParent = organisation as IWorkflowProvider;
				var trigger = workflowParent.WorkflowItems.Triggers.AddNew();
				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				Factory.Save();

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_Module = workflowParent.WorkflowType;
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				communicationsMode.EK_Destination = "SOMEONE";
				communicationsMode.EK_Filename = "(*JobNumber*).xml";
				communicationsMode.EK_ServerAddressSubject = "Organisation [(*JobNumber*)]";

				var notifications = new NotificationBuffer();

				var dateTimeOffset = new ZDateTimeOffset(2010, 12, 25, 0, 0, 0, new TimeSpan(11, 0, 0));

				var logBO = organisation.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: dateTimeOffset, reference: "Test Ref"));

				var expectDateStr = dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:sszzz");

				var queuedLog = new QueuedLogForTesting(logBO, trigger);
				queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
				queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", logBO.PK, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code);

				var actionWrapper = new ActionWrapper(action, organisation, Lazy.Create<IStmALog>(() => logBO));
				var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, organisation);
				IProcessor processor = new NativeXmlWorkflowProcessor(actionWrapper, GetModes(new[] { communicationsMode }), eventInfoProvider);
				processor.Process(notifications);
				Factory.Save();

				CombineAssertions(delegate
				{
					var messages = Factory.Load<IEDIMessage>(new ZQuery());
					AssertEquals("messages.Length", 1, messages.Length);
					var message = messages[0];
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.NativeDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlNativeOrganization, message.EM_MessageSubType);

					AssertStartsWith("message.EM_MessageTextDetail", $@"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{NativeXmlInfo.Version_2012_11_DO_NOT_USE}"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
    <nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <DataSourceCollection>
        <DataSource>
          <Type>Organization</Type>
          <Key>ORGCODEME</Key>
        </DataSource>
      </DataSourceCollection>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventReference>Test Ref</EventReference>
      <EventType>
        <Code />
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <ServerID>DAT</ServerID>
      <Timestamp>{XMLDataProviderConstant.DefaultTimestamp}</Timestamp>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{expectDateStr}</TriggerDate>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </nv:DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{message.Interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{message.Interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
  </Header>
  <Body>
    <Organ".Trim(), message.EM_MessageTextDetail);

					var interchange = message.Interchange;
					AssertNotNull("interchange", interchange);
					AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.NativeDataMessaging, interchange.EI_ApplicationCode);
					AssertEquals("interchange.EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
					AssertEquals("interchange.EI_HeaderText", "<EDIDelivery><FileName>ORGCODEME.xml</FileName><EmailSubject>Organisation [ORGCODEME]</EmailSubject></EDIDelivery>", interchange.EI_HeaderText);
					AssertStartsWith("interchange.EI_BodyText beginning should have Interchange Header", $@"
<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""{UniversalXmlInfo.Version_2011_11}"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>SOMEONE</RecipientID>
  </Header>
  <Body>
    <Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{NativeXmlInfo.Version_2012_11_DO_NOT_USE}"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
    <nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <DataSourceCollection>
        <DataSource>
          <Type>Organization</Type>
          <Key>ORGCODEME</Key>
        </DataSource>
      </DataSourceCollection>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <EventReference>Test Ref</EventReference>
      <EventType>
        <Code />
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <ServerID>DAT</ServerID>
      <Timestamp>{XMLDataProviderConstant.DefaultTimestamp}</Timestamp>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{expectDateStr}</TriggerDate>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </nv:DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{message.Interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{message.Interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
  </Header>
  <Body>
    <Orga".Trim(), interchange.EI_BodyText);

					AssertEndsWith("interchange.EI_BodyText end should have Interchange Footer", @"
      </OrgHeader>
    </Organization>
  </Body>
</Native>
  </Body>
</UniversalInterchange>
".Trim(), interchange.EI_BodyText);
				});
			}
		}

		public void TestOrgHeaderGetsProcessedProperlyToEHub_DoNotSaveFactoryOnDeliver()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "ORGCODEME";

			var workflowParent = organisation as IWorkflowProvider;
			var trigger = workflowParent.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			Factory.Save();

			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_Module = workflowParent.WorkflowType;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Filename = "(*JobNumber*).xml";
			communicationsMode.EK_ServerAddressSubject = "Organisation [(*JobNumber*)]";

			var notifications = new NotificationBuffer();

			var logBO = organisation.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));
			var queuedLog = new QueuedLogForTesting(logBO, trigger);
			queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
			queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", logBO.PK, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code);

			var noOfInterchangesBeforeDelivery = Factory.GetDatabaseCount(typeof(EDIInterchange));

			var actionWrapper = new ActionWrapper(action, organisation, Lazy.Create<IStmALog>(() => logBO));
			var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, organisation);
			IProcessor processor = new NativeXmlWorkflowProcessor(actionWrapper, GetModes(new[] { communicationsMode }), eventInfoProvider);
			using (Factory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				processor.Process(notifications);
				var noOfInterchangesAfterDelivery = Factory.GetDatabaseCount(typeof(EDIInterchange));
				AssertEquals("noOfInterchangesAfterDelivery", noOfInterchangesBeforeDelivery, noOfInterchangesAfterDelivery);

				Factory.Save();
				var noOfInterchangesAfterSave = Factory.GetDatabaseCount(typeof(EDIInterchange));
				AssertEquals("noOfInterchangesAfterSave", noOfInterchangesBeforeDelivery + 1, noOfInterchangesAfterSave);
			}
		}

		public void TestProcessEmailAsAttachment()
		{
			using (Factory.AddDisposableService())
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				IWorkflowProvider organisationBO = Factory.NewWithValidTestData<OrgHeader>();
				var processTask = organisationBO.WorkflowItems.Triggers.AddNew();
				var action = processTask.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;

				Factory.Save();

				var notifications = new NotificationBuffer();
				IProcessor processor = new NativeXmlWorkflowProcessor(new ActionWrapper(action, organisation, null), GetModes(new[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment, EK_Destination = "test@test.com" } }));
				AssertNoExceptionThrown(() =>
				{
					processor.Process(notifications);
				});

				Factory.Save();

				AssertEquals("Should create an email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestProcessMethodGetsThroughToDeliveryFactoryAndIsAbleToSerialize_ButNotSendAsTestingTheSendPartWouldRequireARealWebServiceToBeSetup()
		{
			using (Factory.AddDisposableService())
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				IWorkflowProvider organisationBO = Factory.NewWithValidTestData<OrgHeader>();
				var processTask = organisationBO.WorkflowItems.Triggers.AddNew();
				var action = processTask.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				Factory.Save();

				var dummyOrganisation = Factory.New<OrgHeader>();
				dummyOrganisation.OH_Code = "HHGTTGDP42";
				dummyOrganisation.OH_FullName = "Restaurant At The End Of The Universe";

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_Module = organisationBO.WorkflowType;
				communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector;
				communicationsMode.EK_Destination = @"http:\\slartibartfast.org\magrathea";
				communicationsMode.EK_ParentID = dummyOrganisation.PK;

				Enterprise.Registry.Business.eHubMessagingRegistry.Instance.HasNativeXMLConnector = false;

				var notifications = new NotificationBuffer();
				IProcessor processor = new NativeXmlWorkflowProcessor(new ActionWrapper(action, organisation, null), GetModes(new[] { communicationsMode }));
				AssertExceptionThrown(typeof(NotAllowedException), DeliveryFactoryTest.NativeXMLConnectorPermissionFailureMessage, delegate
				{ processor.Process(notifications); });
				Factory.Save();
			}
		}

		public void TestProcessSucceedsWhenStreamContentIsNull()
		{
			using (Factory.AddDisposableService())
			{
				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				IWorkflowProvider organisationBO = Factory.NewWithValidTestData<OrgHeader>();
				var processTask = organisationBO.WorkflowItems.Triggers.AddNew();
				var action = processTask.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				Factory.Save();

				var mock = new Mock<IBusinessObjectWithDataContextInfoSerializer>();

				mock.Setup(o => o.SerializeToStream(It.IsAny<BusinessObject>(), It.IsAny<IDataContextDataObject>(), It.IsAny<List<IMessageNumber>>()))
					.Callback(() => throw new Exception("Serialization failed"));

				var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(2) };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

				using (ObjectFactory.Substitute("NativeXmlSerializer", mock.Object))
				{
					var notifications = new NotificationBuffer();
					IProcessor processor = new NativeXmlWorkflowProcessor(new ActionWrapper(action, organisation, null), GetModes(new[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.FTP } }));
					AssertNoExceptionThrown(() => processor.Process(notifications));
				}
			}
		}
	}
}

