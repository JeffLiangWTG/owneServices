using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	sealed class USCATAIRMessageEventParentFinderTest : TestCaseWithFactory
	{
		public void TestUSCATAIRMessageEventParentFinderCanMatched()
		{
			CreateTestDeclaration();
			var logger = new TestErrorLogger();
			const string incomingEvent = @"
			<UniversalEvent>
				<Event>
					<DataContext>
						<DataSourceCollection>
						</DataSourceCollection>

						<ActionPurpose>
							<Code>BS</Code>
							<Description></Description>
						</ActionPurpose>
						<DataProvider>USCATAIR</DataProvider>

						<DataTargetCollection>
							<DataTarget>
								<Type>CustomsDeclaration</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>

					<EventTime>2014-11-27T12:30:00</EventTime>
					<EventType>MRR</EventType>
					<EventParameters>
						<Department>CBP</Department>
						<MessageType>BS</MessageType>
					</EventParameters>
					<EventReference>123456789</EventReference>

					<ContextCollection>
						<Context>
							<Type>EntryNumber</Type>
							<Value>XJ5C1234578</Value>
						</Context>
						<Context>
							<Type>EntryNumberType</Type>
							<Value>07</Value>
						</Context>
						<Context>
							<Type>NotificationDetails</Type>
							<Value>emial detail</Value>
						</Context>
						<Context>
							<Type>InternalTransactionNumber</Type>
							<Value>~15000</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var subscriber = new JobDeclarationEventParentFinder(Factory, new JobDeclarationDataContextManager(), logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as Event;
			var logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertNotNull(logParents);

			var declaration = (JobDeclaration)logParents[0];
			AssertNotNull(declaration);
			AssertEquals("XJ5", declaration.US_EntryFilerCode);
			AssertEquals("C1234578", declaration.DecEntryNumber);
		}

		public void TestUSCATAIRMessageEventParentFinderCannotMatch()
		{
			CreateTestDeclaration();
			var logger = new TestErrorLogger();
			const string incomingEvent = @"
			<UniversalEvent>
				<Event>
					<DataContext>
						<DataSourceCollection>
						</DataSourceCollection>

						<ActionPurpose>
							<Code>KNZ</Code>
							<Description></Description>
						</ActionPurpose>
						<DataProvider>ABCTest</DataProvider>

						<DataTargetCollection>
							<DataTarget>
								<Type>CustomsDeclaration</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>

					<EventTime>2014-11-27T12:30:00</EventTime>
					<EventType>MRR</EventType>
					<EventParameters>
						<Department>CBP</Department>
						<MessageType>BS</MessageType>
					</EventParameters>
					<EventReference>123456789</EventReference>

					<ContextCollection>
						<Context>
							<Type>EntryNumber</Type>
							<Value>XJ5C1234578</Value>
						</Context>
						<Context>
							<Type>EntryNumberType</Type>
							<Value>07</Value>
						</Context>
						<Context>
							<Type>NotificationDetails</Type>
							<Value>emial detail</Value>
						</Context>
						<Context>
							<Type>InternalTransactionNumber</Type>
							<Value>~15000</Value>
						</Context>
					</ContextCollection>
				</Event>
			</UniversalEvent>";

			var subscriber = new JobDeclarationEventParentFinder(Factory, new JobDeclarationDataContextManager(), logger);

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(incomingEvent);
			var eventDataObject = xmlEvent as Event;
			var logParents = subscriber.GetLogParentsForEvent(eventDataObject);
			AssertNull(logParents);
		}

		public void TestLinkMessageToJob()
		{
			var message = CreateTestMessage();
			var declaration = CreateTestDeclaration();
			AssertEquals(declaration.Messages.Count, 0);

			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();

			declaration.Messages.Reload(true);
			AssertEquals("message linking to a job", 1, declaration.Messages.Count);
			var linkMessage = declaration.Messages.FirstOrDefault();
			AssertEquals(message.PK, linkMessage.PK);

			var log = declaration.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.MessageReceived.ToString());
			AssertNotNull(log);
		}

		public void TestCannotMatchParents()
		{
			var declaration = CreateTestDeclaration();
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText =
					"B018888XJ5BS                                               286                  " +
					"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
					"B2THIS IS A TEST                                                                " +
					"36ANI111-NN-NNNN BOND USER NAME                          D112714121014          " +
					"Y  8888XJ5WR00005";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();

			message.Reload();
			declaration.Reload();
			AssertEquals(ZGuid.Empty, message.EM_LinkUniqueID);
			AssertEquals(message.EM_ApplicationReference, ZString.Empty);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			AssertNotNull(email);
		}

		public void TestProcessor()
		{
			CreateTestMessage();
			var declaration = CreateTestDeclaration();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();

			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull(universalEventMessage);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains(declaration.CusAgent.GS_EmailAddress));
			AssertNotNull(email);
			AssertEquals(true, email.Body.Contains(string.Format("<a href=\"{0}\">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration), declaration.JobNumber)));
			Assert(email.Subject.ToLower().Contains("enb"));

			declaration.Messages.Reload(true);
			AssertEquals("message linking to a job", 1, declaration.Messages.Count);
			declaration.Logs.GetAllLogs().Reload(true);
			var log = declaration.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == AutoEvents.MessageReceived.ToString());
			AssertNotNull(log);
		}

		public void TestProcessorRecipientWhoSendEntrySummary()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z9";
			staff.GS_EmailAddress = "dummy2@email.com";
			var message = CreateTestMessage();
			var declaration = CreateTestDeclaration();

			declaration.US_EnableCRL = true;

			var last3461Message = declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.AddNew(typeof(MQEDIMessage));
			last3461Message.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsImport;
			last3461Message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummary;
			last3461Message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit;
			last3461Message.EM_MessageText = "B018888XJ5AE                                                                    Y  8888XJ5JR00000";
			last3461Message.EM_SystemCreateUser = "Z9";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains("dummy2@email.com"));
			AssertNotNull(email);
			AssertEquals(true, email.Body.Contains(string.Format("<a href=\"{0}\">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration), declaration.JobNumber)));
		}

		public void TestProcessorRecipientWhoSendCargoRelease()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z9";
			staff.GS_EmailAddress = "dummy4@email.com";
			var message = CreateTestMessage();
			var declaration = CreateTestDeclaration();

			declaration.ActiveEntryHeaders[0].CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			var last3461Message = declaration.ActiveEntryHeaders.SimplifiedEntry.Messages.AddNew(typeof(MQEDIMessage));
			last3461Message.EM_ApplicationCode = Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsImport;
			last3461Message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoRelease;
			last3461Message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIInterchange.Direction.Transmit;
			last3461Message.EM_SystemCreateUser = "Z9";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains("dummy4@email.com"));
			AssertNotNull(email);
			AssertEquals(true, email.Body.Contains(string.Format("<a href=\"{0}\">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration), declaration.JobNumber)));
		}

		public void TestProcessorSendToGroup()
		{
			var message = CreateTestMessage();
			var declaration = CreateTestDeclaration();
			declaration.JE_GS_NKCusAgent = string.Empty;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z7";
			staff.GS_EmailAddress = "dummy3@email.com";
			var ebondGroup = Factory.NewWithValidTestData<GlbGroup>();
			ebondGroup.GG_Code = "G!1";
			ebondGroup.GG_Desc = "eBond GROUP";
			ebondGroup.Staff.Add(staff);
			Factory.Save();
			USCustomsDataRegistry.Instance.BondStatusNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ebondGroup.PK.ToGuid());

			new ABIIncomingMessageProcessor().ExecuteBatch();
			RunUMIMessageProcesserManually();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Recipients.Contains("dummy3@email.com"));
			AssertNotNull(email);
		}

		JobDeclaration CreateTestDeclaration()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";

			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DeclarationReference = "B0000124";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = "07";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.DecEntryNumber = "C1234578";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.JE_GS_NKCusAgent = "Z8";

			Factory.Save();
			return declaration;
		}

		MQEDIMessage CreateTestMessage()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification;
			message.EM_MessageNum = "~15000";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_GE = GlbDepartment.CurrentDepartment.PK;

			message.EM_MessageText =
				"B018888XJ5BS                                               286                  " +
				"B1123456789 ENB NEW BOND HAS BEEN ADDED IN ACE       END 2 1127141230Y          " +
				"B2THIS IS A TEST                                                                " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"10B93A30000012345112614123456789112014121514000000009 NY                        " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"12NNNNXXXNNAAAABBBCCAAAABBBDDAAAABBBFF                                          " +
				"20107XJ5C1234578                                                                " +
				"3034 NN-NNNNNNNXXPrincipal Name                                                 " +
				"35EI YYDDPP-NNNNNCo-principal Name1                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name2                                             " +
				"35EI YYDDPP-NNNNNCo-principal Name3                                             " +
				"36ANI111-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI222-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI333-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI444-NN-NNNN Bond User Name                          D112714121014          " +
				"36ANI555-NN-NNNN Bond User Name                          D112714121014          " +
				"40123777-88-9999Surety Name                              0000000005             " +
				"45111111-88-9999Surety Name                              0000000005             " +
				"45222222-88-9999Surety Name                              0000000005             " +
				"45333333-88-9999Surety Name                              0000000005             " +
				"Y  8888XJ5WR00005";
			Factory.Save();

			return message;
		}

		void RunUMIMessageProcesserManually()
		{
			var universalEventMessage = Factory.Load<EDIMessage>(GetMessageQuery()).FirstOrDefault();
			AssertNotNull("Generate new message", universalEventMessage);

			var serviceTaskLog = new UniversalDataBuss.Management.Testing.ServiceTaskLogForTesting();
			var manager = new UniversalDataBuss.Management.UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(universalEventMessage);
		}

		ZQuery GetMessageQuery()
		{
			var messageQuery = new ZQuery();
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging);
			messageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Internal);
			messageQuery.AddToFilter(EDIMessageSchema.EM_GB, GlbBranch.CurrentBranch.PK);
			messageQuery.AddToFilter(EDIMessageSchema.EM_GE, GlbDepartment.CurrentDepartment.PK);
			messageQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
			messageQuery.MaximumRows = 100;
			return messageQuery;
		}
	}
}
