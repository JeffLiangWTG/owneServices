using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestedType(typeof(OrganizationDataContextManager))]
	public class OrganizationDataContextManagerTest : DataContextManagerTestCase<OrganizationDataContextManager, OrgHeader>
	{
		protected override OrgHeader GetNewBusinessObjectForTesting()
		{
			return Factory.NewWithValidTestData<OrgHeader>();
		}

		public void TestIncomingMonitorEvent_ShouldBeProcessed()
		{
			var org = CreateTestOrganisationForDuns("123456789");
			var message = CreateMonitorEventMessage("DunsNumber", "123456789");
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var expectedLog = $"Linked Event to {org.HumanReadableName}.";

			AssertMultilineASCIIEquals("Service Task Log", expectedLog, serviceTaskLog.ToString());
			AssertMultilineASCIIEquals("Message Log Note", expectedLog, message.GetLogNoteText());
		}

		public void TestIncomingMonitorEvent_NoMatch_ShouldBeDiscarded()
		{
			var org = CreateTestOrganisationForDuns("123");
			var message = CreateMonitorEventMessage("DunsNumber", "123456789");
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			var expectedLog = "Warning - No Module found a Business Entity to link this Universal Event to.";
			var expectedNotes = expectedLog + "\r\nMessage Discarded.";

			AssertMultilineASCIIEquals("Service Task Log", expectedLog, serviceTaskLog.ToString());
			AssertMultilineASCIIEquals("Message Log Note", expectedNotes, message.GetLogNoteText());
		}

		public void TestIncomingNonMonitorEvent_ShouldBeDiscarded()
		{
			var org = CreateTestOrganisationForDuns("123456789");
			var message = CreateMonitorEventMessage("WrongType", "123456789");
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			var expectedLog = "Warning - No Module found a Business Entity to link this Universal Event to.";
			var expectedNotes = expectedLog + "\r\nMessage Discarded.";

			AssertMultilineASCIIEquals("Service Task Log", expectedLog, serviceTaskLog.ToString());
			AssertMultilineASCIIEquals("Message Log Note", expectedNotes, message.GetLogNoteText());
		}

		public void TestIncomingMonitorEventDunsEmpty_ShouldBeDiscarded()
		{
			var org = CreateTestOrganisationForDuns("123456789");
			var message = CreateMonitorEventMessage("DunsNumber", string.Empty);
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			var expectedLog = "Warning - No Module found a Business Entity to link this Universal Event to.";
			var expectedNotes = expectedLog + "\r\nMessage Discarded.";

			AssertMultilineASCIIEquals("Service Task Log", expectedLog, serviceTaskLog.ToString());
			AssertMultilineASCIIEquals("Message Log Note", expectedNotes, message.GetLogNoteText());
		}

		public void TestOrgFoundByGuid_WhenMAAEventReceived()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.SaveForTesting();

			var message = CreateMessageAcceptedEventMessage(org.PK.ToString(), "12345678");
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

			var expectedUsingPKLog = $"Linked Event to Organization ({org.OH_Code}).";
			AssertContains(expectedUsingPKLog, message.GetLogNoteText());
		}

		public void TestNoOrgGuidProvided_ShouldLogWarningMessage_WhenMAAEventReceived()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.SaveForTesting();

			var message = CreateMessageAcceptedEventMessage(string.Empty, string.Empty);
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);

			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			var expectedLog = "Warning - No Module found a Business Entity to link this Universal Event to.";
			AssertContains(expectedLog, message.GetLogNoteText());
		}

		public void TestTRIData_ShouldSaveRelatedData_WhenMAAEventReceived()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsConsignor = true;
			org.OH_IsForwarder = true;
			Factory.SaveForTesting();

			var message = CreateMessageAcceptedEventMessage(org.PK.ToString(), "TRI123456789");
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);
			Factory.SaveForTesting();

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			Assert(org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol);
			Assert(org.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol);
			AssertEquals("TRI", org.CustomsCodes[0].OK_CodeType);
			AssertEquals("TRI123456789", org.CustomsCodes[0].OK_CustomsRegNo);

			var expectedLog = $"MAA event processed successfully, and the Bolero Entity Identifier(TRI) code value TRI123456789 will be updated into Org {org.OH_Code}.";
			AssertContains(expectedLog, message.GetLogNoteText());

			var maaLog = org.Logs.Find(log => log.SL_SE_NKEvent == Events.MessageAccepted.Code);
			AssertEquals("MAA event log should be added.", 1, maaLog.Count());
			AssertContains("Bolero MAA event reference should be set with the correct format.", "|DEP=Bolero|LOC=AU|MST=Enrollment Request|RFN=TRI123456789", maaLog.FirstOrDefault().SL_Reference);

			var messageToUpdate = CreateMessageAcceptedEventMessage(org.PK.ToString(), "TRI987654321");
			messageProcessingManager.Process(messageToUpdate);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			Assert(org.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol);
			Assert(org.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol);
			AssertEquals("TRI", org.CustomsCodes[0].OK_CodeType);
			AssertEquals("TRI987654321", org.CustomsCodes[0].OK_CustomsRegNo);

			expectedLog = $"MAA event processed successfully, and the Bolero Entity Identifier(TRI) code value TRI987654321 will be updated into Org {org.OH_Code}.";
			AssertContains(expectedLog, messageToUpdate.GetLogNoteText());

			maaLog = org.Logs.Find(log => log.SL_SE_NKEvent == Events.MessageAccepted.Code);
			AssertEquals("MAA event log should be added.", 2, maaLog.Count());
			AssertNotNull("MAA event reference should be set with Bolero Entity Identifier(TRI) code value.", maaLog.FirstOrDefault(log => log.SL_Reference.Contains("|RFN=TRI987654321")));
		}

		public void TestMAAEvent_WithMissingRecipientRole_ShouldLogError()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.SaveForTesting();

			var message = CreateMessageAcceptedEventMessage(org.OH_Code, "TRI123456789", false);
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);
			Factory.SaveForTesting();

			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertEquals(0, org.CustomsCodes.Count);

			var expectedLog = $"Invalid RecipientRole : RecipientRole with code 'BOR' not found in RecipientRoleCollection.";
			AssertContains(expectedLog, message.GetLogNoteText());
		}

		public void TestMAAEvent_WithInvalidMessageType_ShouldLogError()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.SaveForTesting();

			var message = CreateMessageAcceptedEventMessage(org.PK.ToString(), "TRI123456789", withDepAndMsgType: false);
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);
			Factory.SaveForTesting();

			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertEquals(0, org.CustomsCodes.Count);

			var expectedLog = $"Invalid EventParameters : Expected MessageType 'Enrollment Request' and Department 'Bolero' not found in EventParameters.";
			AssertContains(expectedLog, message.GetLogNoteText());
		}

		public void TestMAAEvent_WithMissingReferenceNumber_ShouldLogError()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.SaveForTesting();

			var message = CreateMessageAcceptedEventMessage(org.PK.ToString(), string.Empty, withDepAndMsgType: false);
			var contextManager = (IDataContextManager)new OrganizationDataContextManager();
			contextManager.Init(org);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);
			Factory.SaveForTesting();

			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertEquals(0, org.CustomsCodes.Count);

			var expectedLog1 = $"Invalid EventParameters : Expected MessageType 'Enrollment Request' and Department 'Bolero' not found in EventParameters.";
			var expectedLog2 = $"Invalid EventParameters : ReferenceNumber in EventParameters is missing.";

			AssertContains(expectedLog1, message.GetLogNoteText());
			AssertContains(expectedLog2, message.GetLogNoteText());
		}

		public void TestOnLogParentFoundFromEDIMessage()
		{
			using var table = ObjectFactory.Substitute("OrganizationEventProcessors", new Hashtable { { "TestValue", new TestObjectHandle(new TestEventProcessor()) } });

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = "TESTORG";
			testOrg.OH_FullName = "Unmodified";

			var manager = new OrganizationDataContextManager();
			var logger = new XmlSessionTracker(new SimpleLogger());

			var eventDeserializer = new XmlEventDeserializer();

			TestAndAssert("Without DataProvider", @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>TESTORG</Key>
					<Type>Organization</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
	</Event>
</UniversalEvent>", "Unmodified");

			TestAndAssert("With DataProvider=UnknownValue", @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>DataProvider</Type>
					<Key>UnknownValue</Key>
				</DataSource>
			</DataSourceCollection>
			<DataTargetCollection>
				<DataTarget>
					<Key>TESTORG</Key>
					<Type>Organization</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
	</Event>
</UniversalEvent>", "Unmodified");

			TestAndAssert("With DataProvider=TestValue", @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	<Event>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>DataProvider</Type>
					<Key>TestValue</Key>
				</DataSource>
			</DataSourceCollection>
			<DataTargetCollection>
				<DataTarget>
					<Key>TESTORG</Key>
					<Type>Organization</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
	</Event>
</UniversalEvent>", "MODIFIED");

			void TestAndAssert(string message, string xml, string expected)
			{
				var eventDataObject = eventDeserializer.Parse(xml);
				manager.OnLogParentFoundFromEDIMessage(logger, eventDataObject, null, testOrg);
				AssertEquals(message, expected, testOrg.OH_FullName);
			}
		}

		#region Implmentation

		OrgHeader CreateTestOrganisationForDuns(string duns)
		{
			var bizoFactory = new BusinessObjectFactory();
			var org = bizoFactory.NewWithValidTestData<OrgHeader>();
			var cusCode = bizoFactory.NewWithValidTestData<OrgCusCode>();
			cusCode.OK_CodeType = "DUN";
			cusCode.OK_CustomsRegNo = duns;
			cusCode.OK_OH = org.PK;
			bizoFactory.Save();

			return org;
		}

		IEDIMessage CreateMonitorEventMessage(string type, string value)
		{
			var xml = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
<Event>
<EventTime>2020-06-02T16:31:58</EventTime>
<EventType>CCE</EventType>
<EventParameters><Type>MON</Type><Reason>TST</Reason></EventParameters>
<ContextCollection><Context><Type>{0}</Type><Value>{1}</Value></Context></ContextCollection>
</Event>
</UniversalEvent>";

			return GetQueuedUniversalEventMessage(string.Format(xml, type, value));
		}

		IEDIMessage CreateMessageAcceptedEventMessage(ZString orgKey, string triValue, bool withRecipientRoleBOR = true, bool withDepAndMsgType = true)
		{
			var xml = @"
			<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <Event>
				<DataContext>
					<DataTargetCollection>
					<DataTarget>
						<Type>Organization</Type>
						<Key>{0}</Key>
					</DataTarget>
				  </DataTargetCollection>
				  <RecipientRoleCollection>
					<RecipientRole>
						{1}
					</RecipientRole>
				  </RecipientRoleCollection>
				</DataContext>
				<EventTime>2024-11-14T10:58:10.940+11:00</EventTime>
				<EventType>MAA</EventType>
				<EventParameters>
					{2}
					<ReferenceNumber>{3}</ReferenceNumber>
					<Location>AU</Location>
				</EventParameters>
			  </Event>
			</UniversalEvent>";

			var depAndMsgType = withDepAndMsgType
			? @"<Department>Bolero</Department>
				<MessageType>Enrollment Request</MessageType>"
			: @"<Department></Department>
				<MessageType></MessageType>";

			var recipientRole = withRecipientRoleBOR
			? @"<Code>BOR</Code>
				<Description>Bolero Onboarding Response</Description>"
			: @"<Code>AAD</Code>
				<Description>Bolero Onboarding Response</Description>";

			return GetQueuedUniversalEventMessage(string.Format(xml, orgKey, recipientRole, depAndMsgType, triValue));
		}

		class TestEventProcessor : IEventProcessor
		{
			public void ProcessMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
			{
				if (businessObject is OrgHeader org)
				{
					org.OH_FullName = "MODIFIED";
				}
			}
		}

		#endregion
	}
}
