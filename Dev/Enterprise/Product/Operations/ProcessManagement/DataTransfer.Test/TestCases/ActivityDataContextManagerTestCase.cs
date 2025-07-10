using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	abstract class ActivityDataContextManagerTestCase<TManager, TBusinessObject> : DataContextManagerTestCase<TManager, TBusinessObject>
		where TManager : DataContextManager<TBusinessObject>, IActivityDataContextManager, new()
		where TBusinessObject : BusinessObject, IJobNumber, IWorkflowProvider
	{
		public void TestDataContextType()
		{
			var manager = new TManager();
			AssertEquals(ExpectedDataContextType, manager.DataContextType);
		}

		protected abstract DataContextType ExpectedDataContextType { get; }

		public void TestDataContextKey()
		{
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();
			BizoFactory.Save(); // to set the job number

			var manager = new TManager();
			manager.Init(businessObject);

			AssertEquals(businessObject.JobNumber, manager.DataContextKey);
		}

		public void TestGetActivityDataObjectWriter_ShouldReturnCorrectDataObjectWriter()
		{
			var manager = new TManager();
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();
			var writer = manager.GetActivityDataObjectWriter(new DataWritingManager(new ActionInfo(null, businessObject)), shouldIncludeRelatedItems: true);

			AssertEquals(ExpectedDataObjectWriterType, writer.GetType());
		}

		#region Universal Events

		public void TestIncomingUniversalEvent()
		{
			var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();
			BizoFactory.Save();

			var contextManager = new TManager();
			contextManager.Init(businessObject);

			var message = GetQueuedUniversalEventMessage($@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>{contextManager.DataContextType}</Type>
          <Key>{contextManager.DataContextKey}</Key>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>EVT</Code>
        <Description>Event</Description>
      </ActionPurpose>
      <Company>
        <Code>WTG</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>WiseTech Global</Name>
      </Company>
      <DataProvider>EDIDATWTG</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>Z69</Code>
        <Description></Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2019-03-14T13:45:45.063</TriggerDate>
      <TriggerDescription>Moose</TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2019-03-14T13:45:45.07</EventTime>
    <EventType>Z69</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
    </ContextCollection>
  </Event>
</UniversalEvent>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var expectedLog = $"Linked Event to {businessObject.HumanReadableName}.";
			AssertMultilineASCIIEquals("Service Task Log", expectedLog, serviceTaskLog.ToString());
			AssertMultilineASCIIEquals("Message Log Note", expectedLog, message.GetLogNoteText());
		}

		[TestDate(2019, 3, 15)]
		public void TestOutgoingUniversalEvent()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var businessObject = BizoFactory.NewWithValidTestData<TBusinessObject>();
				var trigger = MasterFilesTestHelper.CreateTrigger(businessObject, AutoEvents.CustomisableEvent69Code);
				var action = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, MessageRecipientPartyTypeList.Codes.OrgProxy);
				action.PQ_MessagePurpose = "EVT";
				BizoFactory.Save();

				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var mode = orgProxy.EDICommunicationsModes.AddNew();
				mode.EK_Module = businessObject.WorkflowType;
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				mode.EK_Destination = "Jan Michael Vincent";
				mode.Factory.Save();

				businessObject.Logs.AddNew(AutoEvents.CustomisableEvent69, "TREMENDOUS PROGRESS", ZDateTimeOffset.Now, false);
				BizoFactory.Save();

				MasterFilesTestHelper.RunLogWalker();

				var messages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("1 Message should have been generated by the trigger. SAD!", 1, messages.Length);

				var message = messages.Single();
				AssertEquals("Message Type", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("Message Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				var key = ObjectFactory.Get<IProductRegistration>().Key;
				var company = Company.New(GlbCompany.CurrentCompany);
				AssertMultilineASCIIEquals($@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>{ExpectedDataContextType}</Type>
          <Key>{businessObject.JobNumber}</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>EVT</Code>
        <Description>Event</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>{key.EnterpriseCode + key.ServerCode + company.Code}</DataProvider>
      <EnterpriseID>{key.EnterpriseCode}</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>Z69</Code>
        <Description>Customizable Event 69</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>{key.ServerCode}</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2019-03-15T00:00:00.000+00:00</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2019-03-15T00:00:00.000+00:00</EventTime>
    <EventType>Z69</EventType>
    <CreatedTime>2019-03-15T00:00:00.000+00:00</CreatedTime>
    <EventReference>TREMENDOUS PROGRESS</EventReference>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{message.Interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{message.Interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>", message.EM_MessageText);
			}
		}

		#endregion

		#region Implementation

		protected abstract Type ExpectedDataObjectWriterType { get; }

		protected BusinessObjectFactory BizoFactory => bizoFactory ?? (bizoFactory = new BusinessObjectFactory());
		BusinessObjectFactory bizoFactory;

		#endregion
	}
}
