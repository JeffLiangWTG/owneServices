using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static NUnit.Framework.XmlAssertions;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using SimpleLogger = Enterprise.UniversalDataBuss.Management.SimpleLogger;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class UniversalXmlWorkflowProcessorTest : TestCaseWithFactory
	{
		public void TestTransactionIsRollingBack()
		{
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B300234222";

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = "USI";
			message.EM_MessageType = "BS";
			message.EM_ReceiveTransmit = "RCV";
			var messageBO = (BusinessObject)message;
			Factory.Save();

			var eventDataObject = new UniversalEvent()
			{
				EventType = Events.MessageAcceptedCode,
				EventReference = "HI",
				DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext()
				{
					DataSourceCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataSource>(),
					DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget>()
				},
				ContextCollection = new List<Context>(new[] {
					new Context()
					{
						Type = nameof(UniversalEvent.ContextTypes.GoodsDescription),
						Value = "HELLO WORLD"
					}
				})
			};
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B300234222");

			var mockTransaction = new Mock<IDelayedTransactionManager>();
			mockTransaction.Setup(t => t.IsRollingback).Returns(true);
			mockTransaction.Setup(t => t.ReportInvalidRollbackExceptionHandling()).Throws(new TransactionException("test"));

			UniversalXmlWorkflowProcessor.ShouldSaveResultMessageForTest.Value = true;

			Factory.RefreshEnabled = true;
			using (Factory.AddDisposableService())
			{
				var result = UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(Factory, messageBO, eventDataObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent, transaction: mockTransaction.Object);
				Factory.Save();
			}

			var internalMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
			AssertEquals(EDIMessageStatusList.Codes.Rejected, internalMessage.EM_Status);
			mockTransaction.Verify(t => t.ReportInvalidRollbackExceptionHandling(), Times.Once);
		}

		public void TestUniversalXmlWorkflowProcessorSuppressCheckContextSwitch()
		{
			Factory.RefreshEnabled = true;
			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger = ((IWorkflowProvider)dummyBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Received Goods";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;

			Factory.Save();

			var logBO = dummyBO.GetLogs().AddNew(new EventValue(Events.Received,
				eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

			var logger = new TestLogger();
			var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, dummyBO, Lazy.Create<IStmALog>(() => logBO))
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null))
				, (outboundSessionTracker) => new DummyForwardingShipmentDataObjectWriter()
				, dummyBO);

			using (Factory.AddDisposableService())
			{
				var glbStaff = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, User.WebUserCode)).FirstOrDefault();
				using (Env.SetTemporaryUserContext(glbStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					processor.Process(logger);
				}

				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestTryUseNewFactoryToChangeMessageStatusDoesntReportErrorOnBigMessages()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;

			Factory.Save();

			var bigMessage = new StringBuilder(32800);
			while (bigMessage.Length <= 32768)
			{
				bigMessage.Append(nameof(bigMessage));
			}

			UniversalXmlWorkflowProcessor.TryUseNewFactoryToChangeMessageStatus(message, new XmlSessionTracker(new SimpleLogger()), EDIMessageStatusList.Codes.Failed, new Exception(bigMessage.ToString()));
			AssertEquals("No errors should be reported", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestXudTriggerActionNotDoneWhenDocumentDoesNotHaveAttachment()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "DDI";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;

			((IWorkflowProvider)shipmentBO).Logs.AddNew(Events.DocumentImported);

			Factory.Save();

			var logs = MasterFilesTestHelper.RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());

			AssertEquals(0, messages.Length);
			AssertContains("Warning: [WorkflowEventTrigger] [Default] Ignoring invalid XUD trigger action: Business object has no matching eDocs", logs);
		}

		public void TestXudTriggerActionNotDoneWhenDocumentDoesNotSupportEDocs()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger = ((IWorkflowProvider)dummyBO).WorkflowItems.AddNew();
			trigger.P9_Description = "DDI";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentImportedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;

			dummyBO.Logs.AddNew(Events.DocumentImported);

			Factory.Save();

			var logs = MasterFilesTestHelper.RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());

			AssertEquals(0, messages.Length);
			AssertContains("Warning: [WorkflowEventTrigger] [Default] Ignoring invalid XUD trigger action: Business object does not support eDocs", logs);
		}

		public void TestXueTriggerActionFiredByMissingAuditLogEvent()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger = ((IWorkflowProvider)dummyBO).WorkflowItems.AddNew();
			trigger.P9_Description = "ADD";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;

			Factory.Save();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			dummyBO.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			var wte = trigger.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent);

			using (wte.LockForUpdatingKeyFieldsForTesting())
			{
				var wteData = new WorkflowTriggerEventData(wte);
				wteData.TriggeringLogPK = ZGuid.Empty;
				wte.SL_Reference = wteData.ToReference();
			}

			Factory.Save();

			var logs = MasterFilesTestHelper.RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());

			AssertEquals(1, messages.Length);

			AssertIsXml(messages[0].EM_MessageText)
				.HavingExactlyOneDescendantNode(node => node.WithName("Company")
					.HavingExactlyOneChildNode(node => node.WithName("Name").WithValue(GlbCompany.CurrentCompany.CompanyName)))
				.HavingExactlyOneDescendantNode(node => node.WithName("EventBranch")
					.HavingExactlyOneChildNode(node => node.WithName("Code").WithValue(GlbBranch.CurrentBranch.GB_Code)))
				.HavingExactlyOneDescendantNode(node => node.WithName("EventDepartment")
					.HavingExactlyOneChildNode(node => node.WithName("Code").WithValue(GlbDepartment.CurrentDepartment.GE_Code)))
				.HavingExactlyOneDescendantNode(node => node.WithName("EventType")
					.HavingExactlyOneChildNode(node => node.WithName("Code").WithValue(Events.AddedARecordToTheSystemCode)))
				.HavingExactlyOneDescendantNode(node => node.WithName("EventUser")
					.HavingExactlyOneChildNode(node => node.WithName("Name").WithValue(Env.CurrentUser.FullName)));
		}

		public void TestXusTriggerActionFiredByMissingAuditLogEvent()
		{
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_Module = "SHP";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = "UNVRSLVNT";
			communicationsMode.EK_PublishInternalMilestones = true;
			org.EDICommunicationsModes.Add(communicationsMode);
			org.OH_Code = "EXPORG";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "DEL";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.DeletedARecordInTheSystemCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_OH_Recipient = org.PK;

			Factory.Save();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			shipmentBO.GetLogs().AddNew(Events.DeletedARecordInTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			var wte = trigger.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent);

			using (wte.LockForUpdatingKeyFieldsForTesting())
			{
				var wteData = new WorkflowTriggerEventData(wte);
				wteData.TriggeringLogPK = ZGuid.Empty;
				wte.SL_Reference = wteData.ToReference();
			}

			Factory.Save();

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var logs = MasterFilesTestHelper.RunLogWalker();
			}

			var messages = Factory.Load<EDIMessage>(new ZQuery());

			AssertEquals(1, messages.Length);

			AssertIsXml(messages[0].EM_MessageText)
				.HavingExactlyOneDescendantNode(node => node.WithName("Company")
					.HavingExactlyOneChildNode(node => node.WithName("Name").WithValue(GlbCompany.CurrentCompany.CompanyName)))
				.HavingExactlyOneDescendantNode(node => node.WithName("EventBranch")
					.HavingExactlyOneChildNode(node => node.WithName("Code").WithValue(GlbBranch.CurrentBranch.GB_Code)))
				.HavingExactlyOneDescendantNode(node => node.WithName("EventDepartment")
					.HavingExactlyOneChildNode(node => node.WithName("Code").WithValue(GlbDepartment.CurrentDepartment.GE_Code)))
				.HavingExactlyOneDescendantNode(node => node.WithName("EventType")
					.HavingExactlyOneChildNode(node => node.WithName("Code").WithValue(Events.DeletedARecordInTheSystemCode)))
				.HavingExactlyOneDescendantNode(node => node.WithName("EventUser")
					.HavingExactlyOneChildNode(node => node.WithName("Name").WithValue(Env.CurrentUser.FullName)));
		}

		public void TestUniversalEventContainsTimestamp()
		{
			ObjectFactory.Get<IXMLDataProvider>().SetIgnoreTimestampForTest(false);
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_MasterBillNum] = "MB123456";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

				var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.P9_Description = "Received Goods";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = consolBO.GetLogs().AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();

				var actionWrapper = new ActionWrapper(action, consolBO, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);
				AssertIsXml(messages[0].EM_MessageText).HavingExactlyOneChildNode("Event/DataContext/Timestamp", n => n.WithValue(XMLDataProviderConstant.DefaultTimestamp.ToString()));
			}
		}

		public void TestUniversalShipmentContainsTimestamp()
		{
			ObjectFactory.Get<IXMLDataProvider>().SetIgnoreTimestampForTest(false);
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

				var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Broker;
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();

				var actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyForwardingShipmentDataObjectWriter()
					, shipmentBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);
				AssertIsXml(messages[0].EM_MessageText).HavingExactlyOneChildNode("Shipment/DataContext/Timestamp", n => n.WithValue(XMLDataProviderConstant.DefaultTimestamp.ToString()));
			}
		}

		public void TestNoCommunicationModes()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			Factory.Save();

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

			var logger = new TestLogger();
			var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
				, new UniversalXmlCommunicationModeProvider(() => (Array.Empty<IEDICommunicationsMode>(), (NoResString)"No communication modes found for trigger ATH-Send Data and its action XUS"))
				, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
				, shipmentBO);

			processor.Process(logger);

			AssertMultilineASCIIEquals(
				"GIVEN no communication modes, WHEN executing process, THEN warning should be logged",
				"Warning - No communication modes found for trigger ATH-Send Data and its action XUS",
				logger.GetAllLogsAsString());
		}

		public void TestProcessInternalUniversalEventTrigger()
		{
			var contextValues = new[]
			{
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("MAWBNumber"), new ZString("0816666666")),
			};

			using (Factory.AddDisposableService())
			using (DummyWithWorkflowDataContextManager.SetupAdditionalContextKeyValuePairs(contextValues))
			{
				DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Forwarder;

				var dummyBO = Factory.New<DummyWithWorkflow>();

				var trigger = ((IWorkflowProvider)dummyBO).WorkflowItems.AddNew();
				trigger.P9_Description = "Received Goods";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;
				dummyBO.InitRelatedDummyWithTasks();
				dummyBO.RelatedDummyWithTasks.Z0_Description = "";
				dummyBO.RelatedDummyWithTasks2.Z0_Description = "";

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;

				var logBO = dummyBO.GetLogs().AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

				var typeOfForwardingConsol = ObjectFactory.GetType<Forwarding.IForwardingConsol>();
				var consolBO = Factory.New(typeOfForwardingConsol);
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
				consolBO[JobConsolSchema.JK_TransportMode] = "AIR";
				consolBO[JobConsolSchema.JK_MasterBillNum] = "0816666666";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

				Factory.Save();

				var queuedLog = new QueuedLogForTesting(logBO, trigger);
				queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
				queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", logBO.PK, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code);

				var actionWrapper = new ActionWrapper(action, dummyBO, Lazy.Create<IStmALog>(() => logBO));
				var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, dummyBO);

				var logger = new TestLogger();

				var processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO
					, eventInfoProvider);

				processor.Process(logger);

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var consols = Factory.Load(ObjectFactory.GetType<Forwarding.IForwardingConsol>(), new ZQuery());
				AssertEquals("Should have applied event to the consol", 1, consolBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ReceivedCode)).Length);
			}
		}

		public void TestGetDataObject()
		{
			var typeOfForwardingConsol = ObjectFactory.GetType<Forwarding.IForwardingConsol>();
			var consolBO = Factory.New(typeOfForwardingConsol);
			consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
			consolBO[JobConsolSchema.JK_TransportMode] = "AIR";
			consolBO[JobConsolSchema.JK_MasterBillNum] = "0816666666";
			consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
			consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";
			Factory.Save();
			var dataObject = (UniversalShipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, consolBO);
			AssertEquals("dataObject.TransportMode.Code", "AIR", dataObject.TransportMode.Code);
			AssertEquals("dataObject.WayBillNumber", "081-6666666", dataObject.WayBillNumber);
		}

		[UseSnapshotProtection]
		public void TestUniversalEventDeliveryFailureExceptionFailsXUSMessage()
		{
			var dummyBO1 = Factory.New<DummyWithWorkflow>();

			var trigger = ((IWorkflowProvider)dummyBO1).WorkflowItems.AddNew();
			trigger.P9_Description = "Received Goods";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;

			Factory.Save();

			var logBO = dummyBO1.GetLogs().AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

			var logger = new TestLogger();
			var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, dummyBO1, Lazy.Create<IStmALog>(() => logBO))
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null))
				, (outboundSessionTracker) => new DummyForwardingShipmentDataObjectWriter()
				, dummyBO1);

			var exceptionThrown = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (!exceptionThrown && f is IUniversalBusinessObjectFactory)
				{
					exceptionThrown = true;
					var dummyBO2 = f.New<DummyBusinessObject>();
					throw new UniversalEventDeliveryFailureException(new UniversalEvent[1]);
				}
			});

			using (Factory.AddDisposableService())
			{
				processor.Process(logger);

				var dummyFromFactory = Factory.Load<DummyBusinessObject>(new ZQuery());
				AssertEquals("If a BuisnessObject is created before throwing UniversalEventDeliveryFailureException, it should't be saved", 1, dummyFromFactory.Length);

				var message = Factory.Load<IEDIMessage>(new ZQuery())[0];
				AssertEquals(EDIMessageStatusList.Codes.Failed, message.EM_Status);
				AssertEquals(ReceiveTransmitList.Codes.Internal, message.EM_ReceiveTransmit);
			}
		}

		[UseSnapshotProtection]
		public void TestProcessInternalUniversalShipmentWithNoActiveBranches()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger = ((IWorkflowProvider)dummyBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Received Goods";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;

			Factory.Save();

			var branches = Factory.Load<GlbBranch>(new ZQuery());
			foreach (var branch in branches)
			{
				branch.GB_IsActive = false;
			}

			Factory.Save();

			var logBO = dummyBO.GetLogs().AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

			var logger = new TestLogger();
			var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, dummyBO, Lazy.Create<IStmALog>(() => logBO))
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null))
				, (outboundSessionTracker) => new DummyForwardingShipmentDataObjectWriter()
				, dummyBO);

			using (Factory.AddDisposableService())
			{
				CombineAssertions(() =>
				{
					var events = new IXmlEventValueObject[1];
					AssertNoExceptionThrown(() => events = processor.Process(logger));
					Assert(ErrorReporter.LastExceptionReported == null);
					AssertEquals($"Error - Delivery failed due to validation exception.", logger.GetAllLogsAsString());
					AssertEquals($"Error - Message Rejected as Company '{GlbCompany.CurrentCompany.GC_Code}' has no active branches.", events[0].Context.FailureReason);
					AssertEquals("REJ", events[0].Context.ProcessingStatusCode);
				});
			}
		}

		public void TestProcessInternalUniversalShipmentTrigger()
		{
			ObjectFactory.Get<Customs.Shared.ICustomsDataRegistry>().LocalCountryCustomsInterfaceSubmissionType = "ITF";

			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Broker;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			Factory.Save();

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

			var logger = new TestLogger();
			var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null))
				, (outboundSessionTracker) => new DummyForwardingShipmentDataObjectWriter()
				, shipmentBO);

			using (Factory.AddDisposableService())
			{
				processor.Process(logger);
				Factory.Save();
			}

			AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
				, @"".Trim()
				, logger.GetAllLogsAsString());

			EDIMessage relatedEDIMessage = null;
			CombineAssertions("Sending BO log and linked EDIMessage", delegate
			{
				var shipmentBODEXLogs = shipmentBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DEX"));
				AssertEquals("Shipment should have 1 data export log.", 1, shipmentBODEXLogs.Length);
				var dEXLog = shipmentBODEXLogs[0];
				relatedEDIMessage = dEXLog.RelatedEDIMessage.Message as EDIMessage;
				AssertNotNull("EDIMessage should be linked to event", relatedEDIMessage);
				AssertForwardingShipmentMessageContent(relatedEDIMessage, EDIMessageStatusList.Codes.ProcessedOK, MessageRecipientPartyTypeList.Codes.Broker, @"
				No matching JobDeclaration found, creating new JobDeclaration.
Populating JobDeclaration...
Added Declaration from UniversalShipment.
Successfully saved Declaration B00001000.");
			});

			CombineAssertions("Receiving BO log and linked EDIMessage", delegate
			{
				var consols = Factory.Load(ObjectFactory.GetType<Customs.IBaseJobDeclaration>(), new ZQuery());
				AssertEquals("consol was created internally.", 1, consols.Length);
				var consolBODIMLogs = consols[0].GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DIM"));
				AssertEquals("consol should have 1 data import log.", 1, consolBODIMLogs.Length);
				var dIMLog = consolBODIMLogs[0];
				AssertEquals("Same EDIMessage should be linked to both the Exported and Imported Job.", relatedEDIMessage, dIMLog.RelatedEDIMessage.Message);
			});
		}

		public void TestProcessInternalUniversalShipmentTrigger_OverrideInternalPublishingToSendExternal()
		{
			TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);

			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var purpose = Factory.New<IEDIMessagePurpose>();
			purpose.EMP_Code = "AAA";
			purpose.EMP_Description = "AAA Desc";
			purpose.EMP_DisableOrgProxyRecipientOverride = true;

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_Module = "SHP";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = "UNVRSLVNT";
			communicationsMode.EK_PublishInternalMilestones = true;
			org.EDICommunicationsModes.Add(communicationsMode);
			org.OH_Code = "EXPORG";
			org.OH_FullName = "EXPORG company";
			org.MainAddress.OA_RN_NKCountryCode = "AU";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "1234";
			org.MainAddress.OA_City = "Sydney";
			org.MainAddress.OA_Address1 = $"Some address 1 EXPORG";
			org.MainAddress.OA_Address2 = $"Some address 2 EXPORG";

			var companyData = org.CompanyDataCollection.Cast<OrgCompanyData>().FirstOrDefault(x => x.OB_GC == GlbCompany.CurrentCompany.PK);
			if (companyData == null)
			{
				companyData = org.CompanyDataCollection.AddNew();
				companyData.OB_GC = GlbCompany.CurrentCompany.PK;
			}

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Broker;
			action.PQ_MessagePurpose = purpose.EMP_Code;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_OH_Recipient = org.PK;

			Factory.Save();

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

			var queuedLog = new QueuedLogForTesting(logBO, trigger);
			queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
			queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", logBO.PK, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code);

			Factory.Save();

			string logResult;
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				logResult = MasterFilesTestHelper.RunLogWalker();
			}

			AssertNotContains("No warnings", "Warning:", logResult);
			AssertNotContains("No errors", "Error:", logResult);

			var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 1, messages.Length);

			var message = messages[0];

			var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);

			var interchange = interchanges[0];

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

				AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
				AssertEquals("message.EM_LinkTable", "JobShipment", message.EM_LinkTable);
				AssertEquals("message.EM_LinkUniqueID", shipmentBO.PK, message.EM_LinkUniqueID);

				AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(TestProcessUniversalShipmentTrigger_ExpectedUniversalShipmentMessage_OverrideInternalPublishingToSendExternal.Trim(), SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

				AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
			});

			AssertEquals("message.EM_EI relates to found interchange", interchange.PK, message.EM_EI);
			CombineAssertions(delegate
			{
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("interchange.EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eAdaptorQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eAdaptor, interchange.EI_TransportType);
				AssertEquals("interchange.EI_InterchangeNum", "00000000000000000001", interchange.EI_InterchangeNum);
				AssertEquals("interchange.EI_From", "EDIEDIDAT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "UNVRSLVNT", interchange.EI_To);

				AssertMultilineASCIIEquals("interchange.EI_BodyText", string.Format(TestProcessUniversalShipmentTrigger_ExpectedUniversalShipmentInterchange_OverrideInternalPublishingToSendExternal.Trim(), SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), interchange.EI_BodyText);

				AssertEquals("interchange.EI_IsActive", ZBool.True, interchange.EI_IsActive);
			});
		}

		public void TestCommunicationPartyConfigPopulatedOnMessages()
		{
			TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);

			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>()) as IWorkflowProvider;

			var trigger = shipment.WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var partyConfig = Factory.NewWithValidTestData<EDICommunicationPartyConfig>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_Module = "SHP";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = "UNVRSLVNT";
			communicationsMode.EK_PublishInternalMilestones = true;
			communicationsMode.EK_ECC_CommunicationPartyConfig = partyConfig.PK;
			org.EDICommunicationsModes.Add(communicationsMode);

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Broker;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_OH_Recipient = org.PK;

			Factory.Save();

			shipment.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				MasterFilesTestHelper.RunLogWalker();
			}

			var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
			AssertEquals("interchanges.Length", 1, interchanges.Length);

			var interchange = interchanges[0];

			var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
			AssertEquals("messages.Length", 1, messages.Length);

			var message = messages[0];

			AssertEquals("Communication party config populated on interchange", partyConfig, interchange.CommunicationPartyConfig);
			AssertEquals("message.EM_EI relates to found interchange", interchange.PK, message.EM_EI);
			AssertEquals("Communication party config populated on message", partyConfig, message.CommunicationPartyConfig);
		}

		public void TestSendUniversalXML_AllCommunicationMode_ForFileFormat()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "EXPORG";

			CombineAssertions("These Communication Transport types are invalid for XUS", () =>
			{
				var expectedLog = "Warning: [WorkflowEventTrigger] [Default] Action [Type=XUS,Recipient=OTH,Purpose=] failed for trigger [ATH-Send Data] because Organization [EXPORG] for Company [EDI] has no matching Communication Modes.";
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.All, EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.All, EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.All, EDICommunicationsModeCommunicationsTransportList.Codes.FTP, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.All, EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.All, EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.All, EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, expectedLog);

				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.FTP, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.NativeXMLConnector, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss, expectedLog);

				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalActivity, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XML, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransactionBatch, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.CLP, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.DXL, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.NotificationEmail, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog);
			});

			CombineAssertions("These Communication Transport types are valid for XUS", () =>
			{
				var expectedLog = "[WorkflowEventTrigger] [Default] Action [Type=XUS,Recipient=OTH,Purpose=] starting for trigger [ATH-Send Data]";
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.All, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog, expectMessage: true);
				AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, expectedLog, expectMessage: true);

				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.All, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, expectedLog, expectMessage: true);
					AssertSendUniversalXML_ValidCommunicationModes(org, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface, expectedLog, expectMessage: true);
				}
			});
		}

		void AssertSendUniversalXML_ValidCommunicationModes(OrgHeader org, string fileFormat, string communicationTransport, string expectedLog, bool expectMessage = false)
		{
			TestCaseHelper.ClearTable(EDICommunicationsMode.Schema.TableName);
			TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);

			var job = Factory.New<Forwarding.IForwardingShipment>();

			var trigger = ((IWorkflowProvider)job).WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_Module = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			communicationsMode.EK_CommunicationsTransport = communicationTransport;
			communicationsMode.EK_FileFormat = fileFormat;
			communicationsMode.EK_Destination = "UNVRSLVNT";
			communicationsMode.EK_PublishInternalMilestones = true;
			org.EDICommunicationsModes.Add(communicationsMode);

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Broker;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_OH_Recipient = org.PK;

			Factory.Save();

			((BusinessObject)job).GetLogs().AddNew(Events.Authorised);

			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var logResult = MasterFilesTestHelper.RunLogWalker();

				AssertNotContains("No errors", "Error:", logResult);
				AssertContains(expectedLog, logResult);
			}

			var messages = new BusinessObjectFactory().Load<XmlEDIMessage>(new ZQuery());
			if (!expectMessage)
			{
				AssertEquals("messages.Length", 0, messages.Length);
			}
			else
			{
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];

				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);

				switch (communicationTransport)
				{
					case EDICommunicationsModeCommunicationsTransportList.Codes.EHubService:
					case EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface:
						AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
						break;
					default:
						Fail("Incorrect message status");
						break;
				}

				AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
				AssertEquals("message.EM_LinkTable", "JobShipment", message.EM_LinkTable);
				AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
			}
		}

		public void TestProcessInternalUniversalShipmentTrigger_NoReceivingBO()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			Factory.Save();

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

			using (Factory.AddDisposableService())
			{
				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null))
					, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
					, shipmentBO);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				CombineAssertions("Sending BO log and linked EDIMessage", delegate
				{
					var shipmentBODEXLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, shipmentBO.PK).AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DEX")));
					AssertEquals("Shipment should have 1 data export log.", 1, shipmentBODEXLogs.Length);
					var dEXLog = shipmentBODEXLogs[0];
					var relatedEDIMessage = dEXLog.RelatedEDIMessage.Message as EDIMessage;
					AssertNotNull("EDIMessage should be linked to event", relatedEDIMessage);
					AssertRelatedMessageContent(relatedEDIMessage, EDIMessageStatusList.Codes.Discarded, MessageRecipientPartyTypeList.Codes.OrgProxy, @"
No Module used this Universal Shipment data.
Hint: Adding an element in the DataTargetCollection will make the specified Module import where no existing data matches.
Message Discarded.");
				});
			}
		}

		class UniversalObjectFactorySaveListener : ITransactionParticipantListener
		{
			public class FactorySavingEventArgs : EventArgs
			{
				public FactorySavingEventArgs(BusinessObjectFactory factory) { Factory = factory; }
				public BusinessObjectFactory Factory { get; private set; }
			}

			public event EventHandler<FactorySavingEventArgs> FactorySaving = delegate { };

			public void FactorySaveBeginning(ITransactionParticipant[] factories)
			{
				factories.OfType<BusinessObjectFactory>()
						 .Where(f => f.NameForDebugging == "Universal Data Buss Import")
						 .ForEach(f => FactorySaving(this, new FactorySavingEventArgs(f)));
			}

			public void FactorySaveCompleted(ITransactionParticipant[] factories, bool successful)
			{
			}
		}

		public void TestWhenContentFilterIsSuspendedThenUniversalXmlContentFilterApplicatorIsNotApplied()
		{
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();

			var filterApplicatorMock = new Mock<IUniversalXmlContentFilterApplicator>(MockBehavior.Strict);
			filterApplicatorMock.Setup(x => x.ShouldExcludeEmptyElements(It.IsAny<IUniversalActionInfo>())).Returns(false);

			IXmlEventValueObject[] events;

			using (ObjectFactory.Substitute(filterApplicatorMock.Object))
			using (Factory.AddDisposableService())
			{
				var actionInfoMock = new Mock<IUniversalActionInfo>();
				actionInfoMock.SetupGet(m => m.PurposeCode).Returns("APP");
				actionInfoMock.SetupGet(m => m.ParentBO).Returns(shipment);
				actionInfoMock.SetupGet(m => m.FactoryForProcessing).Returns(Factory);
				actionInfoMock.SetupGet(m => m.TriggerEventCode).Returns(ZString.Empty);

				var communicationsModes = new IEDICommunicationsMode[]
				{
					new NonPersistentEDICommunicationMode
					{
						EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
						EK_Destination = "Valhalla"
					}
				};

				var communicationModesProvider = new UniversalXmlCommunicationModeProvider(() => (communicationsModes, null));

				var processor = new UniversalXmlWorkflowProcessor(
					actionInfoMock.Object,
					communicationModesProvider,
					_ => new DummyShipmentDataObjectWriter(),
					exportedBO: shipment);

				using (processor.SuspendUniversalXmlContentFilterApplicator())
				{
					events = processor.Process(new TestLogger());
				}
			}

			AssertEquals("Universal Shipment was produced", 1, events.Length);
			AssertEquals("Export succeeded", Events.DataExport.Code, events[0].EventType);
		}

		public void TestWhenContentFilterIsSuspendedTwiceThenTheSuspensionNeedsToBeRemovedTwice()
		{
			var communicationsModes = new IEDICommunicationsMode[]
			{
				new NonPersistentEDICommunicationMode
				{
					EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService
				}
			};

			var communicationModesProvider = new UniversalXmlCommunicationModeProvider(() => (communicationsModes, null));

			var processor = new UniversalXmlWorkflowProcessor(
				new DummyActionInfo(),
				communicationModesProvider,
				_ => new DummyShipmentDataObjectWriter(),
				exportedBO: Factory.New<DummyBusinessObject>());

			Assert("UniversalXml Content Filter is not suspended", !processor.IsUniversalXmlContentFilterApplicatorSuspended);

			using (processor.SuspendUniversalXmlContentFilterApplicator())
			{
				Assert("UniversalXml Content Filter is suspended", processor.IsUniversalXmlContentFilterApplicatorSuspended);

				using (processor.SuspendUniversalXmlContentFilterApplicator())
				{
					Assert("UniversalXml Content Filter is suspended after 2nd suspend", processor.IsUniversalXmlContentFilterApplicatorSuspended);
				}

				Assert("UniversalXml Content Filter is still suspended after first suspension removal", processor.IsUniversalXmlContentFilterApplicatorSuspended);
			}

			Assert("UniversalXml Content Filter is not suspended", !processor.IsUniversalXmlContentFilterApplicatorSuspended);
		}

		public void TestProcessInternalUniversalShipmentRetriesOnConcurrencyFail()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();

			var universalFactoryWasSaved = false;
			var listener = new UniversalObjectFactorySaveListener();
			EventHandler<UniversalObjectFactorySaveListener.FactorySavingEventArgs> throwZSaveConcurrencyExceptionHandler = null;
			throwZSaveConcurrencyExceptionHandler = (object sender, UniversalObjectFactorySaveListener.FactorySavingEventArgs e) =>
			{
				universalFactoryWasSaved = true;
				listener.FactorySaving -= throwZSaveConcurrencyExceptionHandler;
				var factory = e.Factory;
				var dummyCopy = factory.Load<DummyBusinessObject>(dummy.PK);
				dummyCopy.Delete();
				dummy.Z0_Bool = !dummy.Z0_Bool;
				Factory.Save();
			};
			listener.FactorySaving += throwZSaveConcurrencyExceptionHandler;
			BusinessObjectFactory.RegisterListener(listener);

			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, GlbCompany.CurrentCompany.OrgProxy, new RecipientRoleType[1] { RecipientRoleType.BRO }, (IWorkflowProvider)shipment);
				Factory.Save();
			}
			Assert("Factory was saved during processing", universalFactoryWasSaved);
			AssertEquals("Universal event was produced", 1, events.Length);
			AssertEquals("Import succeeded", Events.DataImport.Code, events[0].EventType);
		}

		[ExpectNoExceptions]
		public void TestProcessInternalUniversalShipmentTrigger_HandlesDataObjectValidationException()
		{
			AssertProcessInternalUniversalShipmentTrigger_HandlesException<DummyDataObjectWriterWithDataObjectValidationException>(
				"Logs generated while processing - Should detect DataObjectValidationException thrown and log it."
				, @"Error - Data Object Validation error detected as blah blah blah.");
		}

		void AssertProcessInternalUniversalShipmentTrigger_HandlesException<T>(string errorMessage, string expected)
			where T : DummyDataObjectWriterWithException, new()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			Factory.Save();

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));
			var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null))
				, (outboundSessionTracker) => new T()
				, shipmentBO);

			var logger = new TestLogger();

			processor.Process(logger);

			AssertMultilineASCIIEquals(errorMessage, expected, logger.GetAllLogsAsString());
		}

		[ExpectNoExceptions]
		public void TestProcessInternalUniversalShipmentTrigger_HandlesMessageProcessingBusinessFailureException()
		{
			AssertProcessInternalUniversalShipmentTrigger_HandlesException<DummyDataObjectWriterWithMessageProcessingBusinessFailureException>(
				"Logs generated while processing - Should detect MessageProcessingBusinessFailureException thrown and log it."
				, @"Error - Message Processing Business Failure error detected as blah blah blah.");
		}

		public void TestProcessInternalUniversalShipmentUsingPublishInteface_SendInternally()
		{
			ObjectFactory.Get<Customs.Shared.ICustomsDataRegistry>().LocalCountryCustomsInterfaceSubmissionType = "ITF";

			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			Factory.Save();

			var shipments = Factory.Load(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
			AssertEquals("PRECONDITION: 1 shipment", 1, shipments.Length);

			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, internalCompany, new RecipientRoleType[1] { RecipientRoleType.BRO }, (IWorkflowProvider)shipment);
				Factory.Save();
			}

			CombineAssertions(delegate
			{
				AssertEquals(1, events.Length);
				AssertEquals(Events.DataImportCode, events[0].EventType);
				AssertNotNull(events[0].ContextCollection);

				AssertEquals(2, events[0].ContextCollection.Count);
				AssertNotNull(events[0].ContextCollection.Single(context =>
					context.Type.Type.Value == nameof(Event.ContextTypes.ProcessingStatusCode) && context.Value.Value == EDIMessage.Status.ProcessedOK));

				AssertNotNull("DataSourceCollection", events[0].DataContext.DataSourceCollection);
				AssertEquals("DataSourceCollection.Count", 1, events[0].DataContext.DataSourceCollection.Count());
				AssertEquals("DataSource.Key", "B00001000", events[0].DataContext.DataSourceCollection.Select(s => s.Key).FirstOrDefault());
				AssertEquals("DataSource.Type", "CustomsDeclaration", events[0].DataContext.DataSourceCollection.Select(s => s.Type).FirstOrDefault());

				var companyDetails = events[0].DataContext.GetEnterpriseServerAndCompanyIDs();
				AssertEquals("EDI|EDI|DAT", FormatEnterpriseServerAndCompanyIDs(companyDetails));
				AssertNotNull("DataTargetCollection", events[0].DataContext.DataTargetCollection);
				AssertEquals("DataTargetCollection.Count", 1, events[0].DataContext.DataTargetCollection.Count());
				AssertNotNull("DataTarget", events[0].DataContext.DataTargetCollection.FirstOrDefault());
				AssertEquals("DataTarget.Key", "S00001010", events[0].DataContext.DataTargetCollection.FirstOrDefault().Key.ToString());
				AssertEquals("DataTarget.Type", "ForwardingShipment", events[0].DataContext.DataTargetCollection.FirstOrDefault().Type);

				var declarations = Factory.Load(ObjectFactory.GetType<Customs.IBaseJobDeclaration>(), new ZQuery());
				AssertEquals("Declaration was created internally", 1, declarations.Length);

				var importedDeclarations = declarations.Where(s => s.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DIM")).Length == 1).ToArray();
				AssertEquals("Declaration has data import log.", 1, importedDeclarations.Length);
			});
		}

		public void TestProcessInternalUniversalShipmentUsingPublishInteface__SendInternally_Fail()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			Factory.Save();

			var shipments = Factory.Load(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
			AssertEquals("PRECONDITION: 1 shipment", 1, shipments.Length);

			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, internalCompany, new RecipientRoleType[1] { RecipientRoleType.CLI }, (IWorkflowProvider)shipment);
				Factory.Save();
			}

			AssertEquals(1, events.Length);
			AssertEquals(Events.DataImportFailureCode, events[0].EventType);

			AssertNull("DataSourceCollection", events[0].DataContext.DataSourceCollection);

			AssertNotNull(events[0].DataContext);
			var companyDetails = events[0].DataContext.GetEnterpriseServerAndCompanyIDs();
			AssertEquals("EDI|EDI|DAT", FormatEnterpriseServerAndCompanyIDs(companyDetails));
			AssertNotNull(events[0].DataContext.DataTargetCollection);
			AssertEquals(1, events[0].DataContext.DataTargetCollection.Count());
			AssertNotNull(events[0].DataContext.DataTargetCollection.FirstOrDefault());
			AssertEquals("S00001010", events[0].DataContext.DataTargetCollection.FirstOrDefault().Key.ToString());
			AssertEquals("ForwardingShipment", events[0].DataContext.DataTargetCollection.FirstOrDefault().Type);

			AssertEquals(2, events[0].ContextCollection.Count);
			AssertNotNull(events[0].ContextCollection.Single(
				context => context.Type.Type.Value == nameof(Event.ContextTypes.FailureReason)
				&& context.Value.Value == "No Module used this Universal Shipment data."));
			AssertNotNull(events[0].ContextCollection.Single(
				context => context.Type.Type.Value == nameof(Event.ContextTypes.ProcessingStatusCode) && context.Value.Value == EDIMessage.Status.Discarded));

			shipments = Factory.Load(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
			AssertEquals("Shipment was created internally.", 1, shipments.Length);
		}

		public void TestProcessInternalUniversalShipmentUsingPublishInteface__SendInternally_WithMilestones()
		{
			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var milestone1 = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			milestone1.P9_Type = "MIL";

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_Module = "SHP";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = "UNVRSLVNT";
			communicationsMode.EK_PublishInternalMilestones = true;
			organization.EDICommunicationsModes.Add(communicationsMode);

			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;

			Factory.Save();

			AssertMileStonesCount(organization, 1);
			AssertMileStonesCount(internalCompany, 0);

			void AssertMileStonesCount(OrgHeader recipientOrganization, int expectCount)
			{
				using (Factory.AddDisposableService())
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, recipientOrganization, new[] { RecipientRoleType.FOR }, (IWorkflowProvider)shipment);
					Factory.Save();
				}

				var message = Factory.Load<EDIMessage>(new ZQuery()).OrderByDescending(x => x.EM_MessageDateTime).First();

				var doc = new XmlDocument();
				doc.LoadXml(message.EM_MessageText);
				var nsmgr = new XmlNamespaceManager(doc.NameTable);
				nsmgr.AddNamespace("ns", "http://www.cargowise.com/Schemas/Universal/2011/11");
				var shipmentMilestoneCollection = doc.SelectNodes("ns:UniversalShipment/ns:Shipment/ns:MilestoneCollection/ns:Milestone", nsmgr);
				AssertEquals(expectCount, shipmentMilestoneCollection.Count);
			}
		}

		public void TestDoNotHandleSqlLockExceptions()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			Factory.Save();

			var messages = Factory.Load(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
			var shipments = Factory.Load(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());

			UniversalXmlWorkflowProcessor.ThrowExceptionOnProcessAsInboundDataObject.Value = new SqlLockLostException();

			AssertExceptionThrown<SqlLockLostException>(() =>
			{
				using (Factory.AddDisposableService())
				{
					UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, internalCompany, new RecipientRoleType[1] { RecipientRoleType.CLI }, (IWorkflowProvider)shipment);
					Factory.Save();
				}
			});

			messages = Factory.Load(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
			AssertEquals("1 message created", 1, messages.Length);
			var message = (IEDIMessage)messages[0];

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageStatusList.Codes.Pending, message.EM_Status);
				var notes = ((IStmNoteParent)message).Notes.FindByDescription(PredefinedNoteTypes.Instance.DataImportLogNote.Description);
				AssertEquals("No data log for lock lost exception. This is a retry case.", 0, notes.Length);
			});
		}

		[TestDate(2017, 05, 16)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestSendUniversalXmlExternally_ComposesXML_FailOn_LastTwoXMLMessageMismatch()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				Assert("Precondition: must have no messages in EDI Messages", Factory.GetDatabaseCount(typeof(XmlEDIMessage)) == 0);

				#region Arranging Objects
				var organisation = Factory.NewWithValidTestData<OrgHeader>();

				var communicationModeOne = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationModeOne.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationModeOne.EK_Module = "CON";
				communicationModeOne.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationModeOne.EK_Destination = "DURBAN-ZA";
				organisation.EDICommunicationsModes.Add(communicationModeOne);

				var communicationModeTwo = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationModeTwo.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationModeTwo.EK_Module = "CON";
				communicationModeTwo.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationModeTwo.EK_Destination = "CAPETOWN-ZA";
				organisation.EDICommunicationsModes.Add(communicationModeTwo);

				Factory.Save();

				var consol = Factory.New<Forwarding.IForwardingConsol>();
				consol.JK_ConsolMode = Constants.ContainerModes.FTL;
				consol.JK_CorrectedConsolWeight = 10000.00;
				consol.JK_UniqueConsignRef = "TEST000" + new Random().Next(99999);
				consol.JK_RL_NKLoadPort = "WCRSA";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var container = Factory.New<Forwarding.IForwardingContainer>();
				container.JC_JK = consol.PK;
				container.JC_ContainerNum = "ORANGES001";

				Factory.Save();

				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				shipment.JS_UniqueConsignRef = consol.JK_UniqueConsignRef;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "ORTAM";
				shipment.JS_E_DEP = new ZDateTime(2017, 05, 16);
				shipment.JS_E_ARV = new ZDateTime(2017, 05, 20);
				shipment.JS_HouseBill = new Random().Next(99999999).ToString();
				shipment.JS_UnitFreightRate = new ZDecimal(0.74);
				var packLine = ((BusinessObjectCollection)((BusinessObject)shipment)["OuterPackLines"]).AddNew();

				Factory.Save();
				#endregion

				#region Act on Arranged Objects

				PublishUniversalXmlResult events;
				using (Factory.AddDisposableService())
				{
					events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, organisation, new RecipientRoleType[] { RecipientRoleType.FOR }, (IWorkflowProvider)consol);
					Factory.Save();
				}
				var messages = Factory.Load<XmlEDIMessage>(new ZQuery() { OrderBy = "EM_MessageNum DESC" });

				#endregion

				#region Assert expectations on Action outcomes

				Assert("Universal XML Messages: List must contain at least two items.", messages.Length == 2);

				var xml1 = XElement.Parse(messages[0].EM_MessageText);
				var xml2 = XElement.Parse(messages[1].EM_MessageText);

				xml1.Elements().First().Element(XName.Get("MessageNumberCollection", xml1.GetDefaultNamespace().NamespaceName)).Remove();
				xml2.Elements().First().Element(XName.Get("MessageNumberCollection", xml2.GetDefaultNamespace().NamespaceName)).Remove();

				AssertMultilineASCIIEquals("Message Text from both EDI Messages generated should match. (Containers were missing from one before)"
					, xml1.ToString(), xml2.ToString());

				#endregion
			}
		}

		public void TestSendUniversalXmlExternally_TransportBookingObject_ExportSucceeds_NotifyExportedCalled()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				Assert("Precondition: ObjectFactory hashtable \"UniversalDeliveryHooks\" exists", ObjectFactory.Get<Hashtable>("UniversalDeliveryHooks").Count != 0);

				var organisation = Factory.NewWithValidTestData<OrgHeader>();

				var communicationModeOne = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationModeOne.EK_Module = "TBM";
				communicationModeOne.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationModeOne.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationModeOne.EK_Destination = "CONTAINER_TRANSPORT_OPTIMIZATION";
				organisation.EDICommunicationsModes.Add(communicationModeOne);

				var communicationModeTwo = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationModeTwo.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationModeTwo.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationModeTwo.EK_Module = "TBM";
				communicationModeTwo.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationModeTwo.EK_Destination = "UNVRSLVNT";
				communicationModeTwo.EK_PublishInternalMilestones = true;
				organisation.EDICommunicationsModes.Add(communicationModeTwo);

				var consolidation = Factory.New<IDtbBookingConsolidation>();
				consolidation.KB_JobType = "BKG";

				var booking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
				booking.KM_KB_Booking = consolidation.PK;
				booking.KM_JobID = "TB00000001";

				Factory.Save();

				var exportHook = new Mock<IUniversalExportHook>(MockBehavior.Default);

				var objectHandle = new Mock<ObjectHandle>();
				objectHandle.Setup(x => x.GetObject()).Returns(exportHook.Object);

				var hashTable = new Hashtable();
				hashTable.Add("KM", objectHandle.Object);

				ObjectFactory.Substitute("UniversalDeliveryHooks", hashTable);

				using (Factory.AddDisposableService())
				{
					UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, organisation, new RecipientRoleType[] { RecipientRoleType.BRE }, (IWorkflowProvider)booking);
				}

				exportHook.Verify(h => h.OnUniversalXmlExport(
					It.Is<BusinessObject>(b => b is IDtbBooking),
					It.Is<IEDICommunicationsMode>(m => m.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService)), Times.Once, "This method should be executed.");
				exportHook.Verify(h => h.OnUniversalXmlExport(
					It.Is<BusinessObject>(b => b is IDtbBooking),
					It.Is<IEDICommunicationsMode>(m => m.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface)), Times.Never, "This method should NOT be executed.");
				exportHook.Verify(h => h.OnUniversalXmlExportValidationFailure(
					It.Is<BusinessObject>(b => b is IDtbBooking),
					It.IsAny<IEDICommunicationsMode>()), Times.Never, "This method should NOT be executed.");
			}
		}

		public void TestSendUniversalXmlExternally_TransportBookingObject_ExportFailsDueToValidationFailure_NotifyExportValidationFailureCalled()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				Assert("Precondition: ObjectFactory hashtable \"UniversalDeliveryHooks\" exists", ObjectFactory.Get<Hashtable>("UniversalDeliveryHooks").Count != 0);

				var organisation = Factory.NewWithValidTestData<OrgHeader>();

				var communicationModeOne = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationModeOne.EK_Module = "TBM";
				communicationModeOne.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationModeOne.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationModeOne.EK_Destination = "CONTAINER_TRANSPORT_OPTIMIZATION";
				organisation.EDICommunicationsModes.Add(communicationModeOne);

				var communicationModeTwo = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationModeTwo.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationModeTwo.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationModeTwo.EK_Module = "TBM";
				communicationModeTwo.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationModeTwo.EK_Destination = "UNVRSLVNT";
				communicationModeTwo.EK_PublishInternalMilestones = true;
				organisation.EDICommunicationsModes.Add(communicationModeTwo);

				var consolidation = Factory.New<IDtbBookingConsolidation>();
				consolidation.KB_JobType = "BKG";

				var booking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
				booking.KM_KB_Booking = consolidation.PK;
				booking.KM_JobID = "TB00000001";

				Factory.Save();

				var exportHook = new Mock<IUniversalExportHook>(MockBehavior.Default);

				var objectHandle = new Mock<ObjectHandle>();
				objectHandle.Setup(x => x.GetObject()).Returns(exportHook.Object);

				var hashTable = new Hashtable();
				hashTable.Add("KM", objectHandle.Object);

				ObjectFactory.Substitute("UniversalDeliveryHooks", hashTable);

				using (Factory.AddDisposableService())
				{
					UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, organisation, new RecipientRoleType[] { RecipientRoleType.BRE }, (IWorkflowProvider)booking, (outboundSessionTracker) => new DummyDataObjectWriterWithDataObjectValidationException());
				}

				exportHook.Verify(h => h.OnUniversalXmlExportValidationFailure(
					It.Is<BusinessObject>(b => b is IDtbBooking),
					It.Is<IEDICommunicationsMode>(m => m.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService)), Times.Once, "This method should be executed.");
				exportHook.Verify(h => h.OnUniversalXmlExportValidationFailure(
					It.Is<BusinessObject>(b => b is IDtbBooking),
					It.Is<IEDICommunicationsMode>(m => m.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface)), Times.Never, "This method should NOT be executed.");
				exportHook.Verify(h => h.OnUniversalXmlExport(
					It.Is<BusinessObject>(b => b is IDtbBooking),
					It.IsAny<IEDICommunicationsMode>()), Times.Never, "This method should NOT be executed.");
			}
		}

		public void TestSendUniversalXmlExternally_TransportBookingObject_WriterThrowsNonValidationException_NoExportNotificationMethodsAreCalled()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				Assert("Precondition: ObjectFactory hashtable \"UniversalDeliveryHooks\" exists", ObjectFactory.Get<Hashtable>("UniversalDeliveryHooks").Count != 0);

				var organisation = Factory.NewWithValidTestData<OrgHeader>();

				var communicationModeOne = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationModeOne.EK_Module = "TBM";
				communicationModeOne.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationModeOne.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationModeOne.EK_Destination = "CONTAINER_TRANSPORT_OPTIMIZATION";
				organisation.EDICommunicationsModes.Add(communicationModeOne);

				var communicationModeTwo = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationModeTwo.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationModeTwo.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationModeTwo.EK_Module = "TBM";
				communicationModeTwo.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationModeTwo.EK_Destination = "UNVRSLVNT";
				communicationModeTwo.EK_PublishInternalMilestones = true;
				organisation.EDICommunicationsModes.Add(communicationModeTwo);

				var consolidation = Factory.New<IDtbBookingConsolidation>();
				consolidation.KB_JobType = "BKG";

				var booking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
				booking.KM_KB_Booking = consolidation.PK;
				booking.KM_JobID = "TB00000001";

				Factory.Save();

				var exportHook = new Mock<IUniversalExportHook>(MockBehavior.Default);

				var objectHandle = new Mock<ObjectHandle>();
				objectHandle.Setup(x => x.GetObject()).Returns(exportHook.Object);

				var hashTable = new Hashtable();
				hashTable.Add("KM", objectHandle.Object);

				ObjectFactory.Substitute("UniversalDeliveryHooks", hashTable);

				using (Factory.AddDisposableService())
				{
					UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, organisation, new RecipientRoleType[] { RecipientRoleType.BRE }, (IWorkflowProvider)booking, (outboundSessionTracker) => new DummyDataObjectWriterWithMessageProcessingBusinessFailureException());
				}

				exportHook.Verify(h => h.OnUniversalXmlExport(
					It.Is<BusinessObject>(b => b is IDtbBooking),
					It.IsAny<IEDICommunicationsMode>()), Times.Never, "This method should NOT be executed.");
				exportHook.Verify(h => h.OnUniversalXmlExportValidationFailure(
					It.Is<BusinessObject>(b => b is IDtbBooking),
					It.IsAny<IEDICommunicationsMode>()), Times.Never, "This method should NOT be executed.");
			}
		}

		public void TestProcessInternalUniversalShipmentUsingPublishInteface_SendExternally()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();

				var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_Module = "SHP";
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "UNVRSLVNT";
				organization.EDICommunicationsModes.Add(communicationsMode);

				var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

				Factory.Save();

				var shipments = Factory.Load(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
				AssertEquals("PRECONDITION: 1 shipment", 1, shipments.Length);

				var messages = Factory.Load(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
				AssertEquals("PRECONDITION: 0 EDI Messages", 0, messages.Length);

				PublishUniversalXmlResult events;
				using (Factory.AddDisposableService())
				{
					events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, organization, new RecipientRoleType[2] { RecipientRoleType.FOR, RecipientRoleType.CLI }, (IWorkflowProvider)shipment);
					Factory.Save();
				}

				messages = Factory.Load(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
				AssertEquals("2 EDI Messages should be created", 2, messages.Length);

				AssertEquals(2, events.Length);
				AssertEquals(Events.DataExportCode, events[0].EventType);
				AssertEquals(Events.DataExportCode, events[1].EventType);

				AssertNotNull(events[0].ContextCollection);
				AssertEquals("RecipientOrganizationCode", events[0].ContextCollection[0].Type.Type);
				AssertEquals("XVBQP68SIYXQ", events[0].ContextCollection[0].Value.Value);
				AssertEquals("RecipientOrganizationName", events[0].ContextCollection[1].Type.Type);
				AssertEquals("", events[0].ContextCollection[1].Value.Value);
				AssertEquals("CommunicationsTransportMode", events[0].ContextCollection[2].Type.Type);
				AssertEquals("EDP", events[0].ContextCollection[2].Value.Value);
				AssertEquals("CommunicationsDestinationCode", events[0].ContextCollection[3].Type.Type);
				AssertEquals("UNVRSLVNT", events[0].ContextCollection[3].Value.Value);

				AssertNotNull(events[0].DataContext);
				var companyDetails1 = events[0].DataContext.GetEnterpriseServerAndCompanyIDs();
				AssertEquals("EDI|EDI|DAT", FormatEnterpriseServerAndCompanyIDs(companyDetails1));
				AssertNull(events[0].DataContext.DataSourceCollection);
				AssertNotNull(events[0].DataContext.DataTargetCollection);
				AssertEquals(1, events[0].DataContext.DataTargetCollection.Count());
				AssertNotNull(events[0].DataContext.DataTargetCollection.FirstOrDefault());
				AssertEquals("S00001010", events[0].DataContext.DataTargetCollection.FirstOrDefault().Key.ToString());
				AssertEquals("ForwardingShipment", events[0].DataContext.DataTargetCollection.FirstOrDefault().Type);

				AssertNotNull(events[1].ContextCollection);
				AssertEquals("RecipientOrganizationCode", events[1].ContextCollection[0].Type.Type);
				AssertEquals("XVBQP68SIYXQ", events[1].ContextCollection[0].Value.Value);
				AssertEquals("RecipientOrganizationName", events[1].ContextCollection[1].Type.Type);
				AssertEquals("", events[1].ContextCollection[1].Value.Value);
				AssertEquals("CommunicationsTransportMode", events[1].ContextCollection[2].Type.Type);
				AssertEquals("EDP", events[1].ContextCollection[2].Value.Value);
				AssertEquals("CommunicationsDestinationCode", events[1].ContextCollection[3].Type.Type);
				AssertEquals("UNVRSLVNT", events[1].ContextCollection[3].Value.Value);

				AssertNotNull(events[1].DataContext);
				var companyDetails2 = events[0].DataContext.GetEnterpriseServerAndCompanyIDs();
				AssertEquals("EDI|EDI|DAT", FormatEnterpriseServerAndCompanyIDs(companyDetails2));
				AssertNull(events[1].DataContext.DataSourceCollection);
				AssertNotNull(events[1].DataContext.DataTargetCollection);
				AssertEquals(1, events[1].DataContext.DataTargetCollection.Count());
				AssertNotNull(events[1].DataContext.DataTargetCollection.FirstOrDefault());
				AssertEquals("S00001010", events[1].DataContext.DataTargetCollection.FirstOrDefault().Key.ToString());
				AssertEquals("ForwardingShipment", events[1].DataContext.DataTargetCollection.FirstOrDefault().Type);
			}
		}

		public void TestProcessInternalUniversalShipmentUsingPublishInteface_SendExternally_WithMilestones()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_Module = "SHP";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = "UNVRSLVNT";
			communicationsMode.EK_PublishInternalMilestones = true;
			organization.EDICommunicationsModes.Add(communicationsMode);

			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";
			((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			var milestone1 = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			milestone1.P9_Type = "MIL";

			var subShipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			subShipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001011";
			subShipment[JobShipmentSchema.JS_JS_ColoadMasterShipment] = shipment.PK;
			var milestone2 = ((IWorkflowProvider)subShipment).WorkflowItems.AddNew();
			milestone2.P9_Type = "MIL";

			Factory.Save();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("PRECONDITION: 0 EDI Messages", 0, messages.Length);

			using (Factory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, organization, new[] { RecipientRoleType.FOR }, (IWorkflowProvider)shipment);
				Factory.Save();
			}
			messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("1 EDI Messages should be created", 1, messages.Length);

			var doc = new XmlDocument();
			doc.LoadXml(messages[0].EM_MessageText);
			var nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("ns", "http://www.cargowise.com/Schemas/Universal/2011/11");
			var shipmentMilestoneCollection = doc.SelectNodes("ns:UniversalShipment/ns:Shipment/ns:MilestoneCollection/ns:Milestone", nsmgr);
			AssertEquals(1, shipmentMilestoneCollection.Count);
			var subShipmentMilestoneCollection = doc.SelectNodes("ns:UniversalShipment/ns:Shipment/ns:SubShipmentCollection/ns:SubShipment/ns:MilestoneCollection/ns:Milestone", nsmgr);
			AssertEquals(1, subShipmentMilestoneCollection.Count);
		}

		public void TestProcessUniversalTransactionWithMilestonesWhenRecipientRoleTypeIsORP()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = organization.EDICommunicationsModes.AddNew();
			communicationsMode.EK_PublishInternalMilestones = true;
			TestProcessUniversalTransactionMilestonesCore(communicationsMode, RecipientRoleType.ORP);
		}

		public void TestProcessUniversalTransactionWithoutMilestonesWhenRecipientRoleTypeIsORP()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = organization.EDICommunicationsModes.AddNew();
			communicationsMode.EK_PublishInternalMilestones = false;
			TestProcessUniversalTransactionMilestonesCore(communicationsMode, RecipientRoleType.ORP);
		}

		public void TestProcessUniversalTransactionWithMilestonesWhenRecipientRoleTypeIsNotORP()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = organization.EDICommunicationsModes.AddNew();
			communicationsMode.EK_PublishInternalMilestones = true;
			TestProcessUniversalTransactionMilestonesCore(communicationsMode, RecipientRoleType.IDB);
		}

		public void TestProcessUniversalTransactionWithoutMilestonesWhenRecipientRoleTypeIsNotORP()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var communicationsMode = organization.EDICommunicationsModes.AddNew();
			communicationsMode.EK_PublishInternalMilestones = false;
			TestProcessUniversalTransactionMilestonesCore(communicationsMode, RecipientRoleType.IDB);
		}

		public void TestProcessUniversalTransactionMilestonesCore(EDICommunicationsMode communicationsMode, RecipientRoleType recipientRoleType)
		{
			var invoice = Factory.New(ObjectFactory.GetType<IARInvoice>());
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			glHeader.AG_AccountNum = "1234.56.78";
			glHeader.AG_Description = "Test GL Header";
			glHeader.Factory.Save();

			var accounting = ObjectFactory.Get<Enterprise.Integration.Accounting.IAccounting>();
			accounting.Registry.ARSuspenseControlAccount_ForTestOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			accounting.Registry.APSuspenseControlAccount_ForTestOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());
			accounting.Registry.JobRevenueJournalControlAccount_ForTestOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid());

			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_RecipientRole = recipientRoleType.ToString();
			communicationsMode.EK_Module = "RNV";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = "UNVRSLVNT";

			((IWorkflowProvider)invoice).WorkflowItems.AddNew();
			var milestone1 = ((IWorkflowProvider)invoice).WorkflowItems.AddNew();
			milestone1.P9_Description = "11";
			milestone1.P9_Type = "MIL";
			milestone1.P9_IsPublished = false;
			var milestone2 = ((IWorkflowProvider)invoice).WorkflowItems.AddNew();
			milestone2.P9_Description = "22";
			milestone2.P9_Type = "MIL";
			milestone2.P9_IsPublished = false;
			Factory.Save();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("PRECONDITION: 0 EDI Messages", 0, messages.Length);

			using (Factory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var workflowDescriptors = WorkflowDescriptors.Instance.TryGetValueSafe(((IWorkflowProvider)invoice).WorkflowType);
				Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = (outboundSessionTracker) => workflowDescriptors.GetUniversalTransactionDataObjectWriter(outboundSessionTracker);
				PublishUniversalTransaction(communicationsMode.Organisation, new[] { recipientRoleType }.ToRecipientRoleDetails(), (IWorkflowProvider)invoice, dataWriterGetter);
				Factory.Save();
			}
			messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("1 EDI Messages should be created", 1, messages.Length);

			var doc = new XmlDocument();
			doc.LoadXml(messages[0].EM_MessageText);
			var nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("ns", "http://www.cargowise.com/Schemas/Universal/2011/11");
			var transactionMilestoneCollection = doc.SelectNodes("ns:UniversalTransaction/ns:TransactionInfo/ns:MilestoneCollection/ns:Milestone", nsmgr);

			if (communicationsMode.EK_PublishInternalMilestones)
			{
				AssertEquals("with milestone", 2, transactionMilestoneCollection.Count);
				transactionMilestoneCollection.ToList<XmlNode>().Single(x => x.OuterXml.Contains("<Description>11</Description>"));
				transactionMilestoneCollection.ToList<XmlNode>().Single(x => x.OuterXml.Contains("<Description>22</Description>"));
			}
			else
			{
				AssertEquals("without milestone", 0, transactionMilestoneCollection.Count);
			}

			void PublishUniversalTransaction(OrgHeader recipientOrganisation, RecipientRoleDetail[] recipientRoleDetails, IWorkflowProviderCore workflowAndDataProvider, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter)
			{
				var dataProvider = workflowAndDataProvider as BusinessObject;
				var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowAndDataProvider.WorkflowType);
				var actionInfo = new ActionInfo(recipientRoleDetails, dataProvider) { ActionType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML };

				var communicationModesProvider = new UniversalXmlCommunicationModeProvider(() =>
				{
					var communicationModes = new List<IEDICommunicationsMode>();
					foreach (var recipientRoleDetail in recipientRoleDetails)
					{
						var modeQuery = new EDICommunicationModeQuery(
						parent: dataProvider,
						descriptor: workflowDescriptor,
						fileFormat: WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML,
						purpose: ZString.Empty,
						recipientRole: recipientRoleDetail.Type.ToString(),
						eventCode: ZString.Empty,
						eventReference: ZString.Empty);

						var modes = workflowDescriptor.GetCommunicationModesForRecipient(recipientOrganisation, modeQuery);
						communicationModes.AddRange(modes.communicationModes);
					}
					return (communicationModes.ToArray(), null);
				});

				var processor = new UniversalXmlWorkflowProcessor(actionInfo, communicationModesProvider, dataWriterGetter, dataProvider);

				processor.Process(new NotificationBuffer(), CancellationToken.None);
			}
		}

		public void TestProcessInternalUniversalShipmentUsingPublishInterface_SendExternally_Fail()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			organization.EDICommunicationsModes.Add(communicationsMode);

			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			Factory.Save();

			var shipments = Factory.Load(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
			AssertEquals("PRECONDITION: 1 shipment", 1, shipments.Length);

			var messages = Factory.Load(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
			AssertEquals("PRECONDITION: 0 EDI Messages", 0, messages.Length);

			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, organization, new RecipientRoleType[2] { RecipientRoleType.FOR, RecipientRoleType.CLI }, (IWorkflowProvider)shipment);
				Factory.Save();
			}
			messages = Factory.Load(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
			AssertEquals("0 EDI Messages should be created", 0, messages.Length);

			AssertEquals(1, events.Length);
			AssertEquals(Events.DataExportFailureCode, events[0].EventType);
			AssertEquals(1, events[0].ContextCollection.Count);
			AssertEquals("FailureReason", events[0].ContextCollection[0].Type.Type);
			AssertEquals("No EDI Communications settings were found on the Recipient Organization [XVBQP68SIYXQ]. Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.", events[0].ContextCollection[0].Value.Value);

			AssertNotNull(events[0].DataContext);
			var companyDetails = events[0].DataContext.GetEnterpriseServerAndCompanyIDs();
			AssertEquals("EDI|EDI|DAT", FormatEnterpriseServerAndCompanyIDs(companyDetails));
			AssertNull(events[0].DataContext.DataSourceCollection);
			AssertNotNull(events[0].DataContext.DataTargetCollection);
			AssertEquals(1, events[0].DataContext.DataTargetCollection.Count());
			AssertNotNull(events[0].DataContext.DataTargetCollection.FirstOrDefault());
			AssertEquals("S00001010", events[0].DataContext.DataTargetCollection.FirstOrDefault().Key.ToString());
			AssertEquals("ForwardingShipment", events[0].DataContext.DataTargetCollection.FirstOrDefault().Type);
		}

		[ExpectNoExceptions]
		public void TestProcessInternalUniversalShipmentUsingPublishInterface_SendExternally_Fail_HandlesDataObjectValidationException()
		{
			AssertProcessInternalUniversalShipmentUsingPublishInterface_SendExternally_Fail_HandlesException<DummyDataObjectWriterWithDataObjectValidationException>(
				"should still return events even when DataObjectValidationException occurs",
				"Data Object Validation error detected as blah blah blah.");
		}

		void AssertProcessInternalUniversalShipmentUsingPublishInterface_SendExternally_Fail_HandlesException<T>(string errorMessage, string expected)
			where T : DummyDataObjectWriterWithException, new()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_Module = "SHP";
			communicationsMode.EK_FileFormat = "ALL";
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			organization.EDICommunicationsModes.Add(communicationsMode);

			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			Factory.Save();

			var shipments = Factory.Load(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), new ZQuery());
			AssertEquals("PRECONDITION: 1 shipment", 1, shipments.Length);

			var messages = Factory.Load(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
			AssertEquals("PRECONDITION: 0 EDI Messages", 0, messages.Length);

			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, organization, new RecipientRoleType[2] { RecipientRoleType.FOR, RecipientRoleType.CLI }, (IWorkflowProvider)shipment, (outboundSessionTracker) => new T());
				Factory.Save();
			}
			messages = Factory.Load(ObjectFactory.GetType<IEDIMessage>(), new ZQuery());
			AssertEquals("0 EDI Messages should be created", 0, messages.Length);

			AssertNotNull(errorMessage, events);
			AssertEquals(1, events.Length);
			AssertEquals(Events.DataExportFailureCode, events[0].EventType);
			AssertEquals(1, events[0].ContextCollection.Count);
			AssertEquals("FailureReason", events[0].ContextCollection[0].Type.Type);
			AssertEquals(expected, events[0].ContextCollection[0].Value.Value);

			AssertNotNull(events[0].DataContext);
			var companyDetails = events[0].DataContext.GetEnterpriseServerAndCompanyIDs();
			AssertEquals("EDI|EDI|DAT", FormatEnterpriseServerAndCompanyIDs(companyDetails));
			AssertNull(events[0].DataContext.DataSourceCollection);
			AssertNotNull(events[0].DataContext.DataTargetCollection);
			AssertEquals(1, events[0].DataContext.DataTargetCollection.Count());
			AssertNotNull(events[0].DataContext.DataTargetCollection.FirstOrDefault());
			AssertEquals("S00001010", events[0].DataContext.DataTargetCollection.FirstOrDefault().Key.ToString());
			AssertEquals("ForwardingShipment", events[0].DataContext.DataTargetCollection.FirstOrDefault().Type);
		}

		[ExpectNoExceptions]
		public void TestProcessInternalUniversalShipmentUsingPublishInterface_SendExternally_Fail_HandlesMessageProcessingBusinessFailureException()
		{
			AssertProcessInternalUniversalShipmentUsingPublishInterface_SendExternally_Fail_HandlesException<DummyDataObjectWriterWithMessageProcessingBusinessFailureException>(
				"should still return events even when MessageProcessingBusinessFailureException occurs",
				"Message Processing Business Failure error detected as blah blah blah.");
		}

		public void TestPublishUniversalShipmentWithInvalidRecipientOrganizationReturnsEvent()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();
			var recipient = Factory.New<OrgHeader>();

			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, recipient, new RecipientRoleType[1] { RecipientRoleType.BRO }, (IWorkflowProvider)shipment);
			}
			AssertEquals("Universal event was produced", 1, events.Length);
			AssertEquals("Data Export Failure Code observed in event exported", Events.DataExportFailureCode, events[0].EventType);
		}

		string FormatEnterpriseServerAndCompanyIDs(EnterpriseServerAndCompanyID companyDetails)
		{
			return string.Format("{0}|{1}|{2}", companyDetails.CompanyCode, companyDetails.EnterpriseID, companyDetails.ServerID);
		}

		[TestDateIncremental(0, 0, 1)]
		public void TestProcessUniversalEventTriggerMoreThanOnceShowsTriggerCountAppropriately()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_MasterBillNum] = "MB123456";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

				var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.P9_Description = "Received Goods";
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = consolBO.GetLogs().AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();

				var actionWrapper = new ActionWrapper(action, consolBO, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());

				var newExpectedUniversalEventMessage = ExpectedUniversalEventMessage.Insert(
					ExpectedUniversalEventMessage.IndexOf("<IsEstimate>false</IsEstimate>"),
					string.Format("<CreatedTime>{0}</CreatedTime>{1}    ", SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_PostedTimeUtc.ToOffset(), () => 0), System.Environment.NewLine));

				var message = messages.MaxBy(m => m.EM_MessageNum);
				var interchange = message.Interchange;
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(newExpectedUniversalEventMessage.Trim(), SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				});

				logBO = consolBO.GetLogs().AddNew(new EventValue(Events.Received));

				Factory.Save();

				actionWrapper = new ActionWrapper(action, consolBO, Lazy.Create<IStmALog>(() => logBO));
				processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				message = messages.MaxBy(m => m.EM_MessageNum);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertContains("message.EM_MessageText", "<TriggerCount>2</TriggerCount>", message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				});

				logBO = consolBO.GetLogs().AddNew(new EventValue(Events.Received));

				Factory.Save();

				actionWrapper = new ActionWrapper(action, consolBO, Lazy.Create<IStmALog>(() => logBO));
				processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				message = messages.MaxBy(m => m.EM_MessageNum);
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertContains("message.EM_MessageText", "<TriggerCount>3</TriggerCount>", message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				});

				trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.P9_Description = "Action Authorised";
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				logBO = consolBO.GetLogs().AddNew(new EventValue(Events.Authorised));
				Factory.Save();
				actionWrapper = new ActionWrapper(action, consolBO, Lazy.Create<IStmALog>(() => logBO));
				processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				messages = Factory.Load<XmlEDIMessage>(new ZQuery());

				message = messages.MaxBy(m => m.EM_MessageNum);
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertContains("message.EM_MessageText", "<TriggerCount>1</TriggerCount>", message.EM_MessageText);
					AssertContains("message.EM_MessageText", @"
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>".Trim(), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				});
			}
		}

		[TestDateIncremental(0, 0, 1)]
		public void TestProcessUniversalShipmentTriggerMoreThanOnceShowsTriggerCountAppropriately()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";
				shipmentBO[JobShipmentSchema.JS_TransportMode] = "SEA";
				shipmentBO[JobShipmentSchema.JS_HouseBill] = "MB123456";

				var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "9CHARCODE";

				var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
					, shipmentBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];
				var interchange = message.Interchange;
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(ExpectedUniversalShipmentMessage.Trim(), SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				});

				logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised));

				Factory.Save();

				processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
					, shipmentBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				messages = Factory.Load<XmlEDIMessage>(new ZQuery());

				message = messages.MaxBy(m => m.EM_MessageNum);
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertContains("message.EM_MessageText", "<TriggerCount>2</TriggerCount>", message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				});

				logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised));

				Factory.Save();

				processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
					, shipmentBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				message = messages.MaxBy(m => m.EM_MessageNum);
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertContains("message.EM_MessageText", "<TriggerCount>3</TriggerCount>", message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				});
			}
		}

		public void TestProcessUniversalEventTriggerViaEAdaptor()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_MasterBillNum] = "MB123456";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

				var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.P9_Description = "Received Goods";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = consolBO.GetLogs().AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();

				var actionWrapper = new ActionWrapper(action, consolBO, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				Factory.Save();

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var newExpectedUniversalEventMessage = ExpectedUniversalEventMessage.Insert(ExpectedUniversalEventMessage.IndexOf("<IsEstimate>false</IsEstimate>"),
					string.Format("<CreatedTime>{0}</CreatedTime>{1}    ", SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_PostedTimeUtc.ToOffset(), () => 0), System.Environment.NewLine));

				var message = messages[0];
				var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
				AssertEquals("interchanges.Length", 1, interchanges.Length);
				var interchange = interchanges[0];

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "JobConsol", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", consolBO.PK, message.EM_LinkUniqueID);

					AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(newExpectedUniversalEventMessage.Trim(), SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});

				AssertEquals("message.EM_EI relates to found interchange", interchange.PK, message.EM_EI);

				var newExpectedUniversalEventInterchange = ExpectedUniversalEventInterchange.Insert(ExpectedUniversalEventInterchange.IndexOf("<IsEstimate>false</IsEstimate>"),
					string.Format("<CreatedTime>{0}</CreatedTime>{1}    ", SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_PostedTimeUtc.ToOffset(), () => 0), System.Environment.NewLine));

				CombineAssertions(delegate
				{
					AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
					AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("interchange.EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
					AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eAdaptorQueued, interchange.EI_Status);
					AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eAdaptor, interchange.EI_TransportType);
					AssertEquals("interchange.EI_InterchangeNum", "00000000000000000001", interchange.EI_InterchangeNum);
					AssertEquals("interchange.EI_From", "EDIEDIDAT", interchange.EI_From);
					AssertEquals("interchange.EI_To", "UNVRSLVNT", interchange.EI_To);

					AssertMultilineASCIIEquals("interchange.EI_BodyText", string.Format(newExpectedUniversalEventInterchange.Trim(), SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), interchange.EI_BodyText);

					AssertEquals("interchange.EI_IsActive", ZBool.True, interchange.EI_IsActive);
				});
			}
		}

		public void TestProcessUniversalEventTriggerViaEAdaptorShowsDataExportLogOnExportedBusinessObject()
		{
			TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
			TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
			var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
			consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
			consolBO[JobConsolSchema.JK_MasterBillNum] = "MB123456";
			consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
			consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

			var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Received Goods";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = "UNVRSLVNT";

			var logBO = consolBO.GetLogs().AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

			var logger = new TestLogger();

			var actionWrapper = new ActionWrapper(action, consolBO, Lazy.Create<IStmALog>(() => logBO));
			var processor = new UniversalXmlWorkflowProcessor(actionWrapper
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
				, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
				, logBO);

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				processor.Process(logger);
				Factory.Save();
			}

			AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
				, @"".Trim()
				, logger.GetAllLogsAsString());

			var message = Factory.LoadTop1<XmlEDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, consolBO.PK));
			AssertNotNull("Linked EDI Message", message);
			var interchange = message.Interchange;

			var newExpectedUniversalEventMessage = ExpectedUniversalEventMessage.Insert(
				ExpectedUniversalEventMessage.IndexOf("<IsEstimate>false</IsEstimate>"),
				string.Format("<CreatedTime>{0}</CreatedTime>{1}    ", SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_PostedTimeUtc.ToOffset(), () => 0), System.Environment.NewLine));

			CombineAssertions(delegate
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

				AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
				AssertEquals("message.EM_LinkTable", "JobConsol", message.EM_LinkTable);

				AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(newExpectedUniversalEventMessage.Trim(), SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

				AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
				AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
			});

			var messagePivot = Factory.LoadTop1<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation2ID, message.PK));
			AssertNotNull("EDI Message and StmALog pivot", messagePivot);
			var exportLog = consolBO.GetLogs().MostRecentLog;

			CombineAssertions(delegate
			{
				AssertEquals("exportLog.SL_SE_NKEvent", Events.DataExportCode, exportLog.SL_SE_NKEvent);
				AssertEquals("exportLog.SL_EventDescription", "Data Export", exportLog.SL_EventDescription);
				AssertEquals("messagePivot.XX_Relation1ID", exportLog.PK, messagePivot.XX_Relation1ID);
				AssertEquals("messagePivot.XX_Relation1TableCode", StmALogSchema.Constants.Prefix, messagePivot.XX_Relation1TableCode);
				AssertEquals("messagePivot.XX_RelationType", Constants.GenPivotTypes.XmlEdiMessage, messagePivot.XX_RelationType);
			});
		}

		public void TestFailureOfProcessingUniversalEventTriggerViaEAdaptorShowsAppropriateDataExportLogOnExportedBusinessObject()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_MasterBillNum] = "MB123456";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

				var conflictingConsolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				conflictingConsolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";

				var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.P9_Description = "Received Goods";
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = consolBO.GetLogs().AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();

				var actionWrapper = new ActionWrapper(action, consolBO, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessorForTestingExceptionMessage(actionWrapper
					, new IEDICommunicationsMode[] { communicationsMode }
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				processor.Process(logger);

				AssertExceptionThrown<ZSaveException>(Factory.Save);
				ErrorReporter.Clear();
			}
		}

		class UniversalXmlWorkflowProcessorForTestingExceptionMessage : UniversalXmlWorkflowProcessor
		{
			public UniversalXmlWorkflowProcessorForTestingExceptionMessage(IUniversalActionInfo actionInfo, IEDICommunicationsMode[] communicationModes, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject exportedBO, IEventInfo eventInfo = null)
				: base(actionInfo, new UniversalXmlCommunicationModeProvider(() => (communicationModes, null)), dataWriterGetter, exportedBO, eventInfo)
			{ }

			protected override Business.MessageDelivery.DeliveryContext CreateContext(INotifications notifications, IDataWritingManager outboundSessionTracker)
			{
				return new Business.MessageDelivery.DeliveryContext(actionInfo.FactoryForProcessing)
				{
					ParentInfo = EntityInfo.New(actionInfo.ParentBO),
					ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
					MessageTypeCode = EDIMessageTypeList.Codes.XDC,
					MessageSubTypeCode = dataWriterGetter(outboundSessionTracker).EDIMessageSubType,
					Notifications = notifications
				};
			}
		}

		public void TestProcessUniversalEventTriggerViaEHub()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var consolBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
				consolBO[JobConsolSchema.JK_UniqueConsignRef] = "C00001010";
				consolBO[JobConsolSchema.JK_TransportMode] = "SEA";
				consolBO[JobConsolSchema.JK_MasterBillNum] = "MB123456";
				consolBO[JobConsolSchema.JK_RL_NKLoadPort] = "AUSYD";
				consolBO[JobConsolSchema.JK_RL_NKDischargePort] = "NZAKL";

				var trigger = ((IWorkflowProvider)consolBO).WorkflowItems.AddNew();
				trigger.P9_Description = "Received Goods";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.ReceivedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "UNVRSLVNT";

				var logBO = consolBO.GetLogs().AddNew(new EventValue(Events.Received, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var actionWrapper = new ActionWrapper(action, consolBO, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				Factory.Save();

				var newExpectedUniversalEventMessage = ExpectedUniversalEventMessage.Insert(ExpectedUniversalEventMessage.IndexOf("<IsEstimate>false</IsEstimate>"),
					string.Format("<CreatedTime>{0}</CreatedTime>{1}    ", SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_PostedTimeUtc.ToOffset(), () => 0), System.Environment.NewLine));

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];
				var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
				AssertEquals("interchanges.Length", 1, interchanges.Length);

				var interchange = interchanges[0];
				AssertEquals("message.EM_EI relates to found interchange", interchange.PK, message.EM_EI);

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "JobConsol", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", consolBO.PK, message.EM_LinkUniqueID);

					AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(newExpectedUniversalEventMessage.Trim(), SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});

				var newExpectedUniversalEventInterchange = ExpectedUniversalEventInterchange.Insert(ExpectedUniversalEventInterchange.IndexOf("<IsEstimate>false</IsEstimate>"),
					string.Format("<CreatedTime>{0}</CreatedTime>{1}    ", SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_PostedTimeUtc.ToOffset(), () => 0), System.Environment.NewLine));

				CombineAssertions(delegate
				{
					AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
					AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("interchange.EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
					AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
					AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
					AssertEquals("interchange.EI_InterchangeNum", "00000000000000000001", interchange.EI_InterchangeNum);
					AssertEquals("interchange.EI_From", "EDIEDIDAT", interchange.EI_From);
					AssertEquals("interchange.EI_To", "UNVRSLVNT", interchange.EI_To);

					AssertMultilineASCIIEquals("interchange.EI_BodyText", string.Format(newExpectedUniversalEventInterchange.Trim(), SimpleTypeFormatter.GetFormattedValueForWritingToXml(logBO.SL_EventTimeOffset, () => 0), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), interchange.EI_BodyText);

					AssertEquals("interchange.EI_IsActive", ZBool.True, interchange.EI_IsActive);
				});
			}
		}

		#region const string ExpectedUniversalEventMessage

		const string ExpectedUniversalEventMessage = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001010</Key>
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
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>RCV</Code>
        <Description>Received</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{0}</TriggerDate>
      <TriggerDescription>Received Goods</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <EventTime>2010-12-25T00:00:00.000+10:00</EventTime>
    <EventType>RCV</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB123456</Value>
      </Context>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>AUSYD</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>NZAKL</Value>
      </Context>
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{1}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{2}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{3}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>"; //2010-12-25T00:00:00

		const string ExpectedUniversalEventInterchange = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>UNVRSLVNT</RecipientID>
  </Header>
  <Body>
    <UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001010</Key>
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
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>RCV</Code>
        <Description>Received</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{0}</TriggerDate>
      <TriggerDescription>Received Goods</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <EventTime>2010-12-25T00:00:00.000+10:00</EventTime>
    <EventType>RCV</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MB123456</Value>
      </Context>
      <Context>
        <Type>MBOLOriginUNLOCO</Type>
        <Value>AUSYD</Value>
      </Context>
      <Context>
        <Type>MBOLDestinationUNLOCO</Type>
        <Value>NZAKL</Value>
      </Context>
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{1}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{2}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{3}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>
  </Body>
</UniversalInterchange>"; //2010-12-25T00:00:00
		#endregion

		public void TestProcessUniversalShipmentTrigger()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var typeOfForwardingShipment = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
				var shipmentBO = Factory.New(typeOfForwardingShipment);
				shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

				var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "9CHARCODE";

				Factory.Save();

				var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

				var queuedLog = new QueuedLogForTesting(logBO, trigger);
				queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
				queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", logBO.PK, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code);

				var actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO));
				var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, shipmentBO);

				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
					, shipmentBO
					, eventInfoProvider);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];
				var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
				AssertEquals("interchanges.Length", 1, interchanges.Length);

				var interchange = interchanges[0];
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "JobShipment", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", shipmentBO.PK, message.EM_LinkUniqueID);

					AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(TestProcessUniversalShipmentTrigger_ExpectedUniversalShipmentMessage.Trim(), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});

				AssertEquals("message.EM_EI relates to found interchange", interchange.PK, message.EM_EI);
				CombineAssertions(delegate
				{
					AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
					AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("interchange.EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
					AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
					AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
					AssertEquals("interchange.EI_From", "EDIEDIDAT", interchange.EI_From);
					AssertEquals("interchange.EI_To", "9CHARCODE", interchange.EI_To);

					AssertMultilineASCIIEquals("interchange.EI_BodyText", string.Format(TestProcessUniversalShipmentTrigger_ExpectedUniversalShipmentInterchange.Trim(), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), interchange.EI_BodyText);

					AssertEquals("interchange.EI_IsActive", ZBool.True, interchange.EI_IsActive);
				});
			}
		}

		#region const string TestProcessUniversalShipmentTrigger_ExpectedUniversalShipmentMessage

		const string TestProcessUniversalShipmentTrigger_ExpectedUniversalShipmentMessage = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventReference>Test Ref</EventReference>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>
";

		const string TestProcessUniversalShipmentTrigger_ExpectedUniversalShipmentInterchange = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>9CHARCODE</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventReference>Test Ref</EventReference>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>
  </Body>
</UniversalInterchange>";

		const string TestProcessUniversalShipmentTrigger_ExpectedUniversalShipmentMessage_OverrideInternalPublishingToSendExternal = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>AAA</Code>
        <Description>AAA Desc</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventReference>Test Ref</EventReference>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{0}</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <ActualChargeable>0.000</ActualChargeable>
    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CartageWaybillNumber></CartageWaybillNumber>
    <CFSReference></CFSReference>
    <CommunityTransitStatus>
      <Code></Code>
    </CommunityTransitStatus>
    <CompanyTariffLevelOverride>0</CompanyTariffLevelOverride>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LCL</Code>
      <Description>Less Container Load</Description>
    </ContainerMode>
    <DocumentedChargeable>0.000</DocumentedChargeable>
    <DocumentedVolume>0.000</DocumentedVolume>
    <DocumentedWeight>0.000</DocumentedWeight>
    <EventBranchHomePort>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </EventBranchHomePort>
    <FMCTariffID></FMCTariffID>
    <FreightRate>0.0000</FreightRate>
    <FreightRateCurrency>
      <Code></Code>
    </FreightRateCurrency>
    <GoodsDescription></GoodsDescription>
    <GoodsValue>0.0000</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </GoodsValueCurrency>
    <GreenhouseGasEmission>
      <CO2e>0</CO2e>
      <CO2eDescriptiveStatus>
        <Code>NON</Code>
        <Description>Not Calculated</Description>
      </CO2eDescriptiveStatus>
      <CO2eUnit>
        <Code>KG</Code>
        <Description>Kilograms</Description>
      </CO2eUnit>
    </GreenhouseGasEmission>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
    <HouseBillOfLadingType>
      <Code></Code>
    </HouseBillOfLadingType>
    <InsuranceValue>0.0000</InsuranceValue>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </InsuranceValueCurrency>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsBooking>false</IsBooking>
    <IsCancelled>false</IsCancelled>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsHighRisk>false</IsHighRisk>
    <IsNeutralMaster>false</IsNeutralMaster>
    <IsShipping>false</IsShipping>
    <IsSplitShipment>false</IsSplitShipment>
    <ManifestedChargeable>0.000</ManifestedChargeable>
    <ManifestedVolume>0.000</ManifestedVolume>
    <ManifestedWeight>0.000</ManifestedWeight>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PackingOrder>0</PackingOrder>
    <PortOfDestination>
      <Code></Code>
    </PortOfDestination>
    <PortOfOrigin>
      <Code></Code>
    </PortOfOrigin>
    <RateCommodity>
      <Code></Code>
    </RateCommodity>
    <ReleaseType>
      <Code></Code>
    </ReleaseType>
    <ScreeningStatus>
      <Code>NOT</Code>
      <Description>Not Screened</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code></Code>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <ShipperCODAmount>0.0000</ShipperCODAmount>
    <ShipperCODPayMethod>
      <Code></Code>
    </ShipperCODPayMethod>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code></Code>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DeliveryRequiredFrom></DeliveryRequiredFrom>
      <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
      <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
      <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <ExportStatement>
        <Code></Code>
      </ExportStatement>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
      <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
      <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
      <FCLDeliveryEquipmentNeeded>
        <Code></Code>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
      <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
      <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
      <FCLPickupEquipmentNeeded>
        <Code></Code>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0.0000</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PickupRequiredFrom></PickupRequiredFrom>
      <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
      <PickupTruckWaitTime></PickupTruckWaitTime>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryDueDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>RevisedDeliveryDueDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>PickupReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>PickupDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{1}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{2}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{3}</MessageNumber>
    </MessageNumberCollection>
    <PackingLineCollection Content=""Complete"">
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

		const string TestProcessUniversalShipmentTrigger_ExpectedUniversalShipmentInterchange_OverrideInternalPublishingToSendExternal = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>UNVRSLVNT</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>AAA</Code>
        <Description>AAA Desc</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventReference>Test Ref</EventReference>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{0}</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <ActualChargeable>0.000</ActualChargeable>
    <AdditionalTerms></AdditionalTerms>
    <BookingConfirmationReference></BookingConfirmationReference>
    <CartageWaybillNumber></CartageWaybillNumber>
    <CFSReference></CFSReference>
    <CommunityTransitStatus>
      <Code></Code>
    </CommunityTransitStatus>
    <CompanyTariffLevelOverride>0</CompanyTariffLevelOverride>
    <ContainerCount>0</ContainerCount>
    <ContainerMode>
      <Code>LCL</Code>
      <Description>Less Container Load</Description>
    </ContainerMode>
    <DocumentedChargeable>0.000</DocumentedChargeable>
    <DocumentedVolume>0.000</DocumentedVolume>
    <DocumentedWeight>0.000</DocumentedWeight>
    <EventBranchHomePort>
      <Code>AUBNE</Code>
      <Name>Brisbane</Name>
    </EventBranchHomePort>
    <FMCTariffID></FMCTariffID>
    <FreightRate>0.0000</FreightRate>
    <FreightRateCurrency>
      <Code></Code>
    </FreightRateCurrency>
    <GoodsDescription></GoodsDescription>
    <GoodsValue>0.0000</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </GoodsValueCurrency>
    <GreenhouseGasEmission>
      <CO2e>0</CO2e>
      <CO2eDescriptiveStatus>
        <Code>NON</Code>
        <Description>Not Calculated</Description>
      </CO2eDescriptiveStatus>
      <CO2eUnit>
        <Code>KG</Code>
        <Description>Kilograms</Description>
      </CO2eUnit>
    </GreenhouseGasEmission>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <HBLContainerPackModeOverride></HBLContainerPackModeOverride>
    <HouseBillOfLadingType>
      <Code></Code>
    </HouseBillOfLadingType>
    <InsuranceValue>0.0000</InsuranceValue>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </InsuranceValueCurrency>
    <InterimReceiptNumber></InterimReceiptNumber>
    <IsBooking>false</IsBooking>
    <IsCancelled>false</IsCancelled>
    <IsCFSRegistered>false</IsCFSRegistered>
    <IsDirectBooking>false</IsDirectBooking>
    <IsForwardRegistered>true</IsForwardRegistered>
    <IsHighRisk>false</IsHighRisk>
    <IsNeutralMaster>false</IsNeutralMaster>
    <IsShipping>false</IsShipping>
    <IsSplitShipment>false</IsSplitShipment>
    <ManifestedChargeable>0.000</ManifestedChargeable>
    <ManifestedVolume>0.000</ManifestedVolume>
    <ManifestedWeight>0.000</ManifestedWeight>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>3</NoOriginalBills>
    <OuterPacks>0</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PackingOrder>0</PackingOrder>
    <PortOfDestination>
      <Code></Code>
    </PortOfDestination>
    <PortOfOrigin>
      <Code></Code>
    </PortOfOrigin>
    <RateCommodity>
      <Code></Code>
    </RateCommodity>
    <ReleaseType>
      <Code></Code>
    </ReleaseType>
    <ScreeningStatus>
      <Code>NOT</Code>
      <Description>Not Screened</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code></Code>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <ShipperCODAmount>0.0000</ShipperCODAmount>
    <ShipperCODPayMethod>
      <Code></Code>
    </ShipperCODPayMethod>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code></Code>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <LocalProcessing>
      <ArrivalCartageRef></ArrivalCartageRef>
      <DeliveryCartageAdvised></DeliveryCartageAdvised>
      <DeliveryCartageCompleted></DeliveryCartageCompleted>
      <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
      <DeliveryLabourTime></DeliveryLabourTime>
      <DeliveryRequiredBy></DeliveryRequiredBy>
      <DeliveryRequiredFrom></DeliveryRequiredFrom>
      <DeliveryTruckWaitCharge>0.0000</DeliveryTruckWaitCharge>
      <DeliveryTruckWaitTime></DeliveryTruckWaitTime>
      <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
      <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
      <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
      <DemurrageOnPickupTime></DemurrageOnPickupTime>
      <EstimatedDelivery></EstimatedDelivery>
      <EstimatedPickup></EstimatedPickup>
      <ExportStatement>
        <Code></Code>
      </ExportStatement>
      <FCLAvailable></FCLAvailable>
      <FCLDeliveryDetentionCharge>0.0000</FCLDeliveryDetentionCharge>
      <FCLDeliveryDetentionDays>0</FCLDeliveryDetentionDays>
      <FCLDeliveryDetentionFreeDays>0</FCLDeliveryDetentionFreeDays>
      <FCLDeliveryEquipmentNeeded>
        <Code></Code>
      </FCLDeliveryEquipmentNeeded>
      <FCLPickupDetentionCharge>0.0000</FCLPickupDetentionCharge>
      <FCLPickupDetentionDays>0</FCLPickupDetentionDays>
      <FCLPickupDetentionFreeDays>0</FCLPickupDetentionFreeDays>
      <FCLPickupEquipmentNeeded>
        <Code></Code>
      </FCLPickupEquipmentNeeded>
      <FCLStorageCommences></FCLStorageCommences>
      <HasProhibitedPackaging>false</HasProhibitedPackaging>
      <InsuranceRequired>false</InsuranceRequired>
      <IsContingencyRelease>false</IsContingencyRelease>
      <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
      <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
      <LCLAvailable></LCLAvailable>
      <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
      <LCLStorageCommences></LCLStorageCommences>
      <PickupCartageAdvised></PickupCartageAdvised>
      <PickupCartageCompleted></PickupCartageCompleted>
      <PickupLabourCharge>0.0000</PickupLabourCharge>
      <PickupLabourTime></PickupLabourTime>
      <PickupRequiredBy></PickupRequiredBy>
      <PickupRequiredFrom></PickupRequiredFrom>
      <PickupTruckWaitCharge>0.0000</PickupTruckWaitCharge>
      <PickupTruckWaitTime></PickupTruckWaitTime>
      <PrintOptionForPackagesOnAWB>
        <Code>DEF</Code>
        <Description>Default (Dims, fallback to Vol)</Description>
      </PrintOptionForPackagesOnAWB>
    </LocalProcessing>

    <DateCollection>
      <Date>
        <Type>BookingConfirmed</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryDueDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>RevisedDeliveryDueDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>PickupReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryReceiptRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>PickupDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>DeliveryDispatchRequested</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{1}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{2}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{3}</MessageNumber>
    </MessageNumberCollection>
    <PackingLineCollection Content=""Complete"">
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
  </Body>
</UniversalInterchange>";
		#endregion

		public void TestProcessUniversalShipmentAllowsJobNumberMacroToWork()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S90001010";

				var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "9CHARCODE";
				communicationsMode.EK_Filename = "(*JobNumber*).xml";

				Factory.Save();

				var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
					, shipmentBO);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
				AssertEquals("interchanges.Length", 1, interchanges.Length);

				var interchange = interchanges[0];
				AssertEquals("HeaderText", "<EDIDelivery><FileName>S90001010.xml</FileName><EmailSubject></EmailSubject></EDIDelivery>", interchange.EI_HeaderText);
			}
		}

		public void TestProcessUniversalShipmentAllowsJobNumberMacroToWork_JobNumberForWorkflow()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S90001010";

				var consolidation = Factory.New<IDtbBookingConsolidation>();
				consolidation.KB_JobType = "BKG";
				consolidation.KB_ParentTableCode = shipmentBO.TablePrefix;
				consolidation.KB_ParentID = shipmentBO.PK;

				var booking = (IDtbBooking)Factory.New(ObjectFactory.GetType<IDtbBooking>());
				booking.KM_KB_Booking = consolidation.PK;
				booking.KM_JobID = "TB00000001";

				var trigger = ((IWorkflowProvider)booking).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "9CHARCODE";
				communicationsMode.EK_Filename = "(*JobNumber*).xml";

				Factory.Save();

				var logBO = ((BusinessObject)booking).GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, (BusinessObject)booking, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
					, (BusinessObject)booking);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
				AssertEquals("interchanges.Length", 1, interchanges.Length);

				var interchange = interchanges[0];
				AssertEquals("HeaderText", "<EDIDelivery><FileName>TB00000001.xml</FileName><EmailSubject></EmailSubject></EDIDelivery>", interchange.EI_HeaderText);
			}
		}

		public void TestProcessUniversalEventAllowsJobNumberMacroToWork()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S90001010";

				var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "9CHARCODE";
				communicationsMode.EK_Filename = "(*JobNumber*).xml";

				Factory.Save();

				var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO));
				var processor = new UniversalXmlWorkflowProcessor(actionWrapper
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new EventDataObjectWriter(outboundSessionTracker)
					, logBO);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
				AssertEquals("interchanges.Length", 1, interchanges.Length);

				var interchange = interchanges[0];
				AssertEquals("HeaderText", "<EDIDelivery><FileName>S90001010.xml</FileName><EmailSubject></EmailSubject></EDIDelivery>", interchange.EI_HeaderText);
			}
		}

		public void TestProcessUniversalShipmentTriggerHasBranchAndDepartmentInDataContext()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);
				var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

				var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "9CHARCODE";

				Factory.Save();

				var company = GlbCompany.GetActiveCompanies().First();
				var departmentCode = "";
				StmALog logBO = null;

				var branch = company.Branches.First(x => x.GB_IsActive && x.GB_Code != Env.CurrentBranch.Code);

				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					departmentCode = Env.CurrentDepartment.Code;
					logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));
				}

				AssertEquals(1, trigger.GetLogs().LogsNotInDB.Count(e => e.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode));
				AssertNotEquals("Branch code should be different for proper testing.", branch.GB_Code, Env.CurrentBranch.Code);
				var reference = string.Format(CultureInfo.InvariantCulture, "{0}|{1}|{2}|~BP|BNE|BRN||", logBO.PK, branch.GB_Code, departmentCode);
				AssertContains("Reference for WTE event should be Guid|BranchCode|DepartmentCode", reference, trigger.GetLogs().LogsNotInDB.First(e => e.SL_SE_NKEvent == AutoEvents.WorkflowTriggerEventCode).SL_Reference);

				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
					, shipmentBO);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);

				var message = messages[0];
				var interchange = message.Interchange;
				CombineAssertions(delegate
				{
					AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
					AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

					AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
					AssertEquals("message.EM_LinkTable", "JobShipment", message.EM_LinkTable);
					AssertEquals("message.EM_LinkUniqueID", shipmentBO.PK, message.EM_LinkUniqueID);

					AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(ExpectedUniversalShipmentMessage.Trim(), "2010-12-25T00:00:00.000+11:00", interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), message.EM_MessageText);

					AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
					AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
				});
			}
		}

		public void TestAddAnotherExportedBusinessObject()
		{
			AssertEquals("Should implement ISupportUniversalBatchExport", true, typeof(ISupportUniversalBatchExport).IsAssignableFrom(typeof(UniversalXmlWorkflowProcessor)));

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();

			AssertNoExceptionThrown(Factory.Save);

			var dataObjectWriter = new Mock<ITopLevelDataObjectWriter>(MockBehavior.Strict);
			dataObjectWriter.Setup(d => d.EDIMessageSubType).Returns(EDIMessageSubTypeList.Codes.XmlUniversalEvent);

			var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, dummyBO, null)
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() }, null))
				, (outboundSessionTracker) => dataObjectWriter.Object
				, dummyBO) as ISupportUniversalBatchExport;

			var dummyChild = Factory.New<DummyChildBusinessObject>();

			AssertExceptionThrown(typeof(InvalidOperationException),
				EDIMessageSubTypeList.Codes.XmlUniversalEvent + " message is not supported for batch export.",
				() => processor.AddAnotherExportedBusinessObject(dummyChild));

			dataObjectWriter = new Mock<ITopLevelDataObjectWriter>(MockBehavior.Strict);
			dataObjectWriter.Setup(d => d.EDIMessageSubType).Returns(EDIMessageSubTypeList.Codes.XmlUniversalShipment);

			processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, dummyBO, null)
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() }, null))
				, (outboundSessionTracker) => dataObjectWriter.Object
				, dummyBO);

			AssertExceptionThrown(typeof(ArgumentException),
				"All exported objects have to be of the same type",
				() => processor.AddAnotherExportedBusinessObject(dummyChild));

			var dummyInAnotherFactory = new BusinessObjectFactory().New<DummyWithWorkflow>();

			AssertExceptionThrown(typeof(ArgumentException),
				"All exported objects have to have the same Factory",
				() => processor.AddAnotherExportedBusinessObject(dummyInAnotherFactory));

			var anotherDummyBO = Factory.New<DummyWithWorkflow>();
			AssertNoExceptionThrown(() => processor.AddAnotherExportedBusinessObject(anotherDummyBO));

			dataObjectWriter = new Mock<ITopLevelDataObjectWriter>(MockBehavior.Strict);
			dataObjectWriter.Setup(d => d.EDIMessageSubType).Returns(EDIMessageSubTypeList.Codes.XmlUniversalTransaction);
			var transaction1 = Factory.New<AccTransactionHeader>();
			var transaction2 = Factory.New<AccTransactionHeader>();

			processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, dummyBO, null)
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() }, null))
				, (outboundSessionTracker) => dataObjectWriter.Object
				, transaction1);

			AssertNoExceptionThrown(() => processor.AddAnotherExportedBusinessObject(transaction2));

			AssertExceptionThrown(typeof(ArgumentException),
				"All exported objects have to be transaction header.",
				() => processor.AddAnotherExportedBusinessObject(dummyChild));
		}

		public void TestAddAnotherTopLevelDataObjectWriterAndXmlWriter()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();

			AssertNoExceptionThrown(Factory.Save);

			var dataObjectWriter = new Mock<ITopLevelDataObjectWriter>(MockBehavior.Strict);
			dataObjectWriter.Setup(d => d.EDIMessageSubType).Returns(EDIMessageSubTypeList.Codes.XmlUniversalEvent);

			var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, dummyBO, null)
				, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() }, null))
				, (outboundSessionTracker) => dataObjectWriter.Object
				, dummyBO) as ISupportUniversalBatchExport;

			var dataObjectWriter2 = new Mock<ITopLevelDataObjectWriter>(MockBehavior.Strict);
			dataObjectWriter2.Setup(d => d.EDIMessageSubType).Returns(EDIMessageSubTypeList.Codes.XmlUniversalEvent);

			AssertNoExceptionThrown(() => processor.AddAnotherTopLevelDataObjectWriterAndXmlWriter(dataObjectWriter2.Object, null));

			var dataObjectWriter3 = new Mock<ITopLevelDataObjectWriter>(MockBehavior.Strict);
			dataObjectWriter3.Setup(d => d.EDIMessageSubType).Returns(EDIMessageSubTypeList.Codes.XmlUniversalShipment);

			AssertExceptionThrown(typeof(ArgumentException),
				"All top level data object writer have to have the same EDI message sub type.",
				() => processor.AddAnotherTopLevelDataObjectWriterAndXmlWriter(dataObjectWriter3.Object, null));
		}

		public void TestShipmentBatchDelivery()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);

				var shipment1 = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment1[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

				var shipment2 = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment2[JobShipmentSchema.JS_UniqueConsignRef] = "S00002222";

				var shipment3 = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment3[JobShipmentSchema.JS_UniqueConsignRef] = "S00003333";

				var trigger = ((IWorkflowProvider)shipment1).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "9CHARCODE";

				Factory.Save();

				var logBO = shipment1.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipment1, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyShipmentDataObjectWriter()
					, shipment1);

				((ISupportUniversalBatchExport)processor).AddAnotherExportedBusinessObject(shipment2);
				((ISupportUniversalBatchExport)processor).AddAnotherExportedBusinessObject(shipment3);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery()).OrderBy(m => m.EM_MessageNum).ToList();
				AssertEquals("Three messages should have been created", 3, messages.Count);

				Action<XmlEDIMessage, string, BusinessObject> assertMessage = (message, expectedMessageNumber, expectedShipment) =>
					{
						CombineAssertions(delegate
						{
							AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
							AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
							AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
							AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
							AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

							AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
							AssertEquals("message.EM_LinkTable", "JobShipment", message.EM_LinkTable);
							AssertEquals("message.EM_LinkUniqueID", expectedShipment.PK, message.EM_LinkUniqueID);

							string shipmentNumber = (ZString)expectedShipment[JobShipmentSchema.JS_UniqueConsignRef];
							string messageText = string.Format(ExpectedUniversalShipmentMessage.Trim().Replace("S00001010", shipmentNumber), "2010-12-25T00:00:00.000+10:00", message.Interchange.EI_SessionGUID, message.Interchange.EI_InterchangeNum, message.EM_MessageNum);
							AssertMultilineASCIIEquals("message.EM_MessageText", messageText, message.EM_MessageText);

							AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
							AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
						});
					};

				assertMessage(messages[0], "00000000000000000001", shipment1);
				assertMessage(messages[1], "00000000000000000002", shipment2);
				assertMessage(messages[2], "00000000000000000003", shipment3);

				var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
				AssertEquals("A single interchange have been created", 1, interchanges.Length);

				var interchange = interchanges[0];
				AssertContainsExactElementsInAnyOrder("Interchange contains all messages", messages, interchange.ContainedMessages);

				CombineAssertions(delegate
				{
					AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
					AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("interchange.EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
					AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eAdaptorQueued, interchange.EI_Status);
					AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eAdaptor, interchange.EI_TransportType);
					AssertEquals("interchange.EI_InterchangeNum", "00000000000000000001", interchange.EI_InterchangeNum);
					AssertEquals("interchange.EI_From", "EDIEDIDAT", interchange.EI_From);
					AssertEquals("interchange.EI_To", "9CHARCODE", interchange.EI_To);

					string shipment2messageText = string.Format(ExpectedUniversalShipmentMessage.Trim().Replace("S00001010", "S00002222"), "2010-12-25T00:00:00.000+10:00", interchange.EI_SessionGUID, interchange.EI_InterchangeNum, messages[1].EM_MessageNum);
					string shipment3messageText = string.Format(ExpectedUniversalShipmentMessage.Trim().Replace("S00001010", "S00003333"), "2010-12-25T00:00:00.000+10:00", interchange.EI_SessionGUID, interchange.EI_InterchangeNum, messages[2].EM_MessageNum);

					int interchangeBodyClosingTag = ExpectedUniversalShipmentInterchange.Trim().IndexOf("</Body>");
					string interchangeMessageText = string.Format(ExpectedUniversalShipmentInterchange
						.Trim()
						.Insert(interchangeBodyClosingTag, "  " + shipment2messageText + "\r\n  " + "  " + shipment3messageText + "\r\n  "),
						interchange.EI_SessionGUID, interchange.EI_InterchangeNum, messages[0].EM_MessageNum);

					AssertMultilineASCIIEquals("interchange.EI_BodyText contains all messages", interchangeMessageText, interchange.EI_BodyText);

					AssertEquals("interchange.EI_IsActive", ZBool.True, interchange.EI_IsActive);
				});
			}
		}

		public void TestTransactionBatchDelivery()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(XmlEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(XmlEDIInterchange.Schema.TableName);

				var invoice1 = Factory.New(ObjectFactory.GetType<IARInvoice>());
				invoice1[AccTransactionHeaderSchema.AH_TransactionNum] = "00001000";

				var invoice2 = Factory.New(ObjectFactory.GetType<IARInvoice>());
				invoice2[AccTransactionHeaderSchema.AH_TransactionNum] = "00001001";

				var invoice3 = Factory.New(ObjectFactory.GetType<IARInvoice>());
				invoice3[AccTransactionHeaderSchema.AH_TransactionNum] = "00001002";

				var trigger = ((IWorkflowProvider)invoice1).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "9CHARCODE";

				Factory.Save();

				var logBO = invoice1.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, invoice1, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyAccountingInvoiceDataObjectWriter()
					, invoice1);

				((ISupportUniversalBatchExport)processor).AddAnotherExportedBusinessObject(invoice2);
				((ISupportUniversalBatchExport)processor).AddAnotherExportedBusinessObject(invoice3);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery()).OrderBy(m => m.EM_MessageNum).ToList();
				AssertEquals("Three messages should have been created", 3, messages.Count);

				Action<XmlEDIMessage, string, BusinessObject> assertMessage = (message, expectedMessageNumber, expectedInvoice) =>
				{
					CombineAssertions(delegate
					{
						AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
						AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
						AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
						AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalTransaction, message.EM_MessageSubType);
						AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);

						AssertEquals("message.EM_ApplicationReference", "", message.EM_ApplicationReference);
						AssertEquals("message.EM_LinkTable", "AccTransactionHeader", message.EM_LinkTable);
						AssertEquals("message.EM_LinkUniqueID", expectedInvoice.PK, message.EM_LinkUniqueID);

						string transactionNumber = (ZString)expectedInvoice[AccTransactionHeaderSchema.AH_TransactionNum];
						string messageText = string.Format(ExpectedUniversalTransactionMessage.Trim().Replace("00001000", transactionNumber), message.Interchange.EI_SessionGUID, message.Interchange.EI_InterchangeNum, message.EM_MessageNum);
						AssertMultilineASCIIEquals("message.EM_MessageText", messageText, message.EM_MessageText);

						AssertEquals("message.EM_IsActive", ZBool.True, message.EM_IsActive);
						AssertEquals("message.EM_IsTestMessage", ZBool.False, message.EM_IsTestMessage);
					});
				};

				assertMessage(messages[0], "00000000000000000001", invoice1);
				assertMessage(messages[1], "00000000000000000002", invoice2);
				assertMessage(messages[2], "00000000000000000003", invoice3);

				var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
				AssertEquals("A single interchange have been created", 1, interchanges.Length);

				var interchange = interchanges[0];
				AssertContainsExactElementsInAnyOrder("Interchange contains all messages", messages, interchange.ContainedMessages);

				CombineAssertions(delegate
				{
					AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
					AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
					AssertEquals("interchange.EI_InterchangeType", EDIMessageTypeList.Codes.XDC, interchange.EI_InterchangeType);
					AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eAdaptorQueued, interchange.EI_Status);
					AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eAdaptor, interchange.EI_TransportType);
					AssertEquals("interchange.EI_From", "EDIEDIDAT", interchange.EI_From);
					AssertEquals("interchange.EI_To", "9CHARCODE", interchange.EI_To);

					string invoice2messageText = string.Format(ExpectedUniversalTransactionMessage.Trim().Replace("00001000", "00001001"), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, messages[1].EM_MessageNum);
					string invoice3messageText = string.Format(ExpectedUniversalTransactionMessage.Trim().Replace("00001000", "00001002"), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, messages[2].EM_MessageNum);

					int interchangeBodyClosingTag = ExpectedUniversalTransactionInterchange.Trim().IndexOf("</Body>");
					string interchangeMessageText = string.Format(ExpectedUniversalTransactionInterchange
						.Trim()
						.Insert(interchangeBodyClosingTag, "  " + invoice2messageText + "\r\n  " + "  " + invoice3messageText + "\r\n  "),
						interchange.EI_SessionGUID, interchange.EI_InterchangeNum, messages[0].EM_MessageNum);

					AssertMultilineASCIIEquals("interchange.EI_BodyText contains all messages", interchangeMessageText, interchange.EI_BodyText);

					AssertEquals("interchange.EI_IsActive", ZBool.True, interchange.EI_IsActive);
				});
			}
		}

		public void TestTransactionBatchDeliveryWithError()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(AutoEDIMessage.Schema.TableName);
				TestCaseHelper.ClearTable(AutoEDIInterchange.Schema.TableName);

				var testObject = Factory.New(ObjectFactory.GetType<IARInvoice>());
				const string dummyError = "Dummy error message to trigger exception";
				testObject.AddRowError(dummyError);

				var trigger = ((IWorkflowProvider)testObject).WorkflowItems.AddNew();
				trigger.P9_Description = "Send Data";
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				var communicationsMode = Factory.New<EDICommunicationsMode>();
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationsMode.EK_Destination = "9CHARCODE";
				communicationsMode.EK_CommunicationsTransport = "HUB";

				Factory.Save();

				var logBO = testObject.GetLogs().AddNew(new EventValue(AutoEvents.Authorised, eventTime: new ZDateTimeOffset(2023, 10, 3)));

				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, testObject, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null))
					, (outboundSessionTracker) => new DummyAccountingInvoiceDataObjectWriter()
					, testObject);

				processor.Process(logger);
				Factory.Save();

				var messages = Factory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("No messages should have been created", 0, messages.Length);

				AssertMultilineASCIIEquals("Logs generated while processing - One error must be present."
					, "Error - " + dummyError
					, logger.GetAllLogsAsString());
			}
		}

		public void TestProcess_SchemaIsOverriden_UseOverridenSchema()
		{
			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_Destination = "McLaren";

			Factory.Save();

			var logBO = shipment.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

			IUniversalXmlSchema actualSchema = null;
			var processor = new UniversalXmlWorkflowProcessor(
				new ActionWrapper(action, shipment, Lazy.Create<IStmALog>(() => logBO)),
				new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationsMode }, null)),
				(outboundSessionTracker) =>
				{
					actualSchema = outboundSessionTracker.Schema;
					return new DummyShipmentDataObjectWriter(outboundSessionTracker);
				},
				shipment,
				schema: UniversalXmlSchema.Version_2012_11_DO_NOT_USE);

			using (Factory.AddDisposableService())
			{
				processor.Process(new TestLogger());
				Factory.Save();
			}

			AssertEquals("Schema", UniversalXmlSchema.Version_2012_11_DO_NOT_USE.Namespace, actualSchema.Namespace);
		}

		public void TestInternalProcessFactoryHasDisposableService()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();

			var factoryFound = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == "Universal Data Buss Import")
				{
					factoryFound = true;
					Assert("DisposableManager not registered", f.TryGetDisposableManager(out var _));
				}
			});
			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, GlbCompany.CurrentCompany.OrgProxy, new RecipientRoleType[1] { RecipientRoleType.BRO }, (IWorkflowProvider)shipment);
				Factory.Save();
			}
			Assert("Factory not found", factoryFound);
			AssertEquals("Universal event was produced", 1, events.Length);
			AssertEquals("Import succeeded", Events.DataImport.Code, events[0].EventType);
		}

		#region GetAdditionalDataObjectsToExportWithWriter

		public void TestInternalProcess_GetAdditionalDataObjectsToExportWithWriter()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			Factory.Save();

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

			using (Factory.AddDisposableService())
			{
				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { new NonPersistentEDICommunicationMode() { EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss } }, null))
					, (outboundSessionTracker) => new DummyForwardingShipmentDataObjectWriter()
					, shipmentBO);

				((ISupportUniversalBatchExport)processor).AddAnotherTopLevelDataObjectWriterAndXmlWriter(new DummyShipmentDataObjectWriter(), null);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				CombineAssertions("Sending BO log and linked EDIMessages", delegate
				{
					var shipmentBODEXLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, shipmentBO.PK).AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DEX")));
					AssertEquals("Shipment should have 2 data export log.", 2, shipmentBODEXLogs.Length);

					var expectedEDIMessages = new[]
					{
						@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
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
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>
",
						@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
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
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>
"
					};

					AssertContainsExactElementsInAnyOrder(expectedEDIMessages, shipmentBODEXLogs.Select(log => log.RelatedEDIMessage.Message.EM_MessageText));
				});
			}
		}

		public void TestExternalProcess_GetAdditionalDataObjectsToExportWithWriter()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipmentBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "Send Data";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			Factory.Save();

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25)));

			var communicationModeOne = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationModeOne.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationModeOne.EK_Module = "CON";
			communicationModeOne.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationModeOne.EK_Destination = "DURBAN-ZA";

			var communicationModeTwo = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationModeTwo.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationModeTwo.EK_Module = "CON";
			communicationModeTwo.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationModeTwo.EK_Destination = "CAPETOWN-ZA";

			using (Factory.AddDisposableService())
			{
				var logger = new TestLogger();
				var processor = new UniversalXmlWorkflowProcessor(new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO))
					, new UniversalXmlCommunicationModeProvider(() => (new IEDICommunicationsMode[] { communicationModeOne, communicationModeTwo }, null))
					, (outboundSessionTracker) => new DummyForwardingShipmentDataObjectWriter()
					, shipmentBO);

				((ISupportUniversalBatchExport)processor).AddAnotherTopLevelDataObjectWriterAndXmlWriter(new DummyShipmentDataObjectWriter(), null);

				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("Logs generated while processing - Apparently no news is good news."
					, @"".Trim()
					, logger.GetAllLogsAsString());

				CombineAssertions("Sending BO log and linked EDIMessages", delegate
				{
					var shipmentBODEXLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, shipmentBO.PK).AddToFilter(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DEX")));
					AssertEquals("Shipment should have 4 data export log.", 4, shipmentBODEXLogs.Length);
					AssertEquals(2, shipmentBODEXLogs.Count(log => log.RelatedEDIMessage.Message.EM_MessageText.Contains("C00001000")));

					var interchanges = Factory.Load<XmlEDIInterchange>(new ZQuery());
					AssertEquals("interchanges.Length", 2, interchanges.Length);
					AssertEquals(2, interchanges.Count(interchange => interchange.EI_BodyText.Contains("C00001000")));
				});
			}
		}

		#endregion

		#region const string ExpectedUniversalShipmentMessage

		const string ExpectedUniversalShipmentMessage = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>{0}</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{1}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{2}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{3}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>
";

		const string ExpectedUniversalShipmentInterchange = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>9CHARCODE</RecipientID>
  </Header>
  <Body>
    <UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>
  </Body>
</UniversalInterchange>";
		#endregion

		#region const string ExpectedUniversalTransactionMessage

		const string ExpectedUniversalTransactionMessage = @"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccountingInvoice</Type>
          <Key>AR INV 00001000</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </TransactionInfo>
</UniversalTransaction>
";

		const string ExpectedUniversalTransactionInterchange = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
    <SenderID>EDIEDIDAT</SenderID>
    <RecipientID>9CHARCODE</RecipientID>
  </Header>
  <Body>
    <UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccountingInvoice</Type>
          <Key>AR INV 00001000</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
  </TransactionInfo>
</UniversalTransaction>
  </Body>
</UniversalInterchange>";
		#endregion

		#region Publish Universal Event Test

		public void TestProcessInternalUniversalEvent_SendInternally()
		{
			var contextValues = new[]
			{
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("MBOLNumber"), new ZString("MB100031")),
				new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("HBOLNumber"), new ZString("HB1000460")),
			};

			using (DummyWithWorkflowDataContextManager.SetupAdditionalContextKeyValuePairs(contextValues))
			{
				AssertNotNull(DummyWorkflowDescriptor.Instance);
				var dummyBO = Factory.New<DummyWithWorkflow>();
				dummyBO.Z0_Description = "XXX12345678";

				var logBO = dummyBO.GetLogs().AddNew(new EventValue(Events.ExportCustomsCleared, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				var job = (BusinessObject)Factory.New<Customs.US.IJobDeclaration>();
				job[JobDeclarationSchema.JE_DeclarationReference] = "B000004";
				job[JobDeclarationSchema.JE_MessageType] = "EXP";
				job[JobDeclarationSchema.JE_ExportDate] = ZDateTime.Now;
				job[JobDeclarationSchema.JE_MasterBill] = "MB100031";
				job[JobDeclarationSchema.JE_HouseBill] = "HB1000460";
				job[JobDeclarationSchema.JE_RL_NKFinalDestination] = "CATOR";

				Factory.Save();

				PublishUniversalXmlResult events;
				using (Factory.AddDisposableService())
				{
					events = UniversalXmlWorkflowProcessor.PublishUniversalEvent(Factory, new RecipientRoleType[1] { RecipientRoleType.SPM }, dummyBO, logBO);
					Factory.Save();
				}
				CombineAssertions(delegate
				{
					AssertEquals(1, events.Length);
					AssertEquals(Events.DataImportCode, events[0].EventType);
					AssertNotNull(events[0].ContextCollection);

					AssertNotNull("DataSourceCollection", events[0].DataContext.DataSourceCollection);
					AssertEquals("DataSourceCollection.Count", 1, events[0].DataContext.DataSourceCollection.Count());
					var firstDataSource = events[0].DataContext.DataSourceCollection.FirstOrDefault();
					AssertEquals("DataSource.Type", "CustomsDeclaration", firstDataSource.Type);

					var companyDetails = events[0].DataContext.GetEnterpriseServerAndCompanyIDs();
					AssertEquals("EDI|EDI|DAT", FormatEnterpriseServerAndCompanyIDs(companyDetails));
					AssertNotNull("DataTargetCollection", events[0].DataContext.DataTargetCollection);
					AssertEquals("DataTargetCollection.Count", 1, events[0].DataContext.DataTargetCollection.Count());
					AssertNotNull("DataTarget", events[0].DataContext.DataTargetCollection.FirstOrDefault());
					AssertEquals("DataTarget.Key", "XXX12345678", events[0].DataContext.DataTargetCollection.FirstOrDefault().Key.ToString());
					AssertEquals("DataTarget.Type", "DummyBusinessObject", events[0].DataContext.DataTargetCollection.FirstOrDefault().Type);
				});
			}
		}

		public void TestDoNotSaveInPublishUniversalXmlInternally()
		{
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B300234222";

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = "USI";
			message.EM_MessageType = "BS";
			message.EM_ReceiveTransmit = "RCV";
			var messageBO = (BusinessObject)message;
			Factory.Save();

			var eventDataObject = new UniversalEvent()
			{
				EventType = Events.MessageAcceptedCode,
				EventReference = "HI",
				DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext()
				{
					DataSourceCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataSource>(),
					DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget>()
				},
				ContextCollection = new List<Context>(new[] {
					new Context()
					{
						Type = nameof(UniversalEvent.ContextTypes.GoodsDescription),
						Value = "HELLO WORLD"
					}
				})
			};
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B300234222");

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				var onSaveAction = new Action<BusinessObjectFactory>(f => throw new InvalidOperationException("No saving here please"));
				BusinessObjectFactory.SetOnFactorySaveHookForTest(onSaveAction);
				AssertNoExceptionThrown(() => UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(Factory, messageBO, eventDataObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent));
			}
		}

		public void TestSendUniversalXmlInternallyMessageNotCreated()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var mock = new Mock<IXmlEDIMessage>();
			using (ObjectFactory.Substitute(mock.Object))
			using (Factory.AddDisposableService())
			{
				var exceptionThrown = false;
				try
				{
					UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, internalCompany, new RecipientRoleType[] { RecipientRoleType.CLI }, (IWorkflowProvider)shipment);
				}
				catch (ZException ex)
				{
					AssertContains("IXmlEDIMessageProxy is not a valid BusinessObject type.", ex.Message);
					exceptionThrown = true;
				}

				Assert("ZException with correct error message was expected", exceptionThrown);
			}
		}

		public void TestDataRefreshBussEnabledWhenParentFactoryIsEnabled()
		{
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B300234222";

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = "USI";
			message.EM_MessageType = "BS";
			message.EM_ReceiveTransmit = "RCV";
			var messageBO = (BusinessObject)message;
			Factory.Save();

			var eventDataObject = new UniversalEvent()
			{
				EventType = Events.MessageAcceptedCode,
				EventReference = "HI",
				DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext()
				{
					DataSourceCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataSource>(),
					DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget>()
				},
				ContextCollection = new List<Context>(new[] {
					new Context()
					{
						Type = nameof(UniversalEvent.ContextTypes.GoodsDescription),
						Value = "HELLO WORLD"
					}
				})
			};
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B300234222");

			Factory.Save();

			var count = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				if (factory.NameForDebugging.Equals("Publish Universal Xml Internally"))
				{
					count++;
					AssertEquals("DataRefreshBus should be disabled on internal factories when the parent factory is disabled", Factory.RefreshEnabled, factory.RefreshEnabled);
				}
			});

			Factory.RefreshEnabled = true;
			using (Factory.AddDisposableService())
			{
				UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(Factory, messageBO, eventDataObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
				Factory.Save();
			}

			Factory.RefreshEnabled = false;
			using (Factory.AddDisposableService())
			{
				UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(Factory, messageBO, eventDataObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
				Factory.Save();
			}

			AssertEquals("Expecting on save action to be hit twice", 2, count);
		}

		public void TestProcessInternalUniversalEvent_SpecialWriter()
		{
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B300234222";
			var declarationBO = (BusinessObject)declaration;

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = "USI";
			message.EM_MessageType = "BS";
			message.EM_ReceiveTransmit = "RCV";
			var messageBO = (BusinessObject)message;
			Factory.Save();

			var eventDataObject = new UniversalEvent()
			{
				EventType = Events.MessageAcceptedCode,
				EventReference = "HI",
				DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext()
				{
					DataSourceCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataSource>(),
					DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget>()
				},
				ContextCollection = new List<Context>(new[] {
					new Context()
					{
						Type = nameof(UniversalEvent.ContextTypes.GoodsDescription),
						Value = "HELLO WORLD"
					}
				})
			};
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B300234222");

			Factory.Save();

			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(Factory, messageBO, eventDataObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
				Factory.Save();
			}

			CombineAssertions(delegate
			{
				AssertEquals(1, events.Length);
				AssertEquals(Events.DataImportCode, events[0].EventType);
				AssertNotNull(events[0].ContextCollection);
				AssertEquals(3, events[0].ContextCollection.Count);
				AssertNotNull(events[0].ContextCollection.Single(context => context.Type == nameof(UniversalEvent.ContextTypes.ProcessingStatusCode) && context.Value.Value == EDIMessage.Status.ProcessedOK));
				AssertNotNull(events[0].ContextCollection.Single(context => context.Type == nameof(UniversalEvent.ContextTypes.ProcessingLog)));
				AssertNotNull(events[0].ContextCollection.Single(context => context.Type == nameof(UniversalEvent.ContextTypes.GoodsDescription) && context.Value.Value == "HELLO WORLD"));

				AssertNotNull("DataSourceCollection", events[0].DataContext.DataSourceCollection);
				AssertEquals("DataSourceCollection.Count", 1, events[0].DataContext.DataSourceCollection.Count());
				var firstDataSource = events[0].DataContext.DataSourceCollection.FirstOrDefault();
				AssertEquals("DataSource.Type", "CustomsDeclaration", firstDataSource.Type);
				AssertEquals("DataSource.Key", "B300234222", firstDataSource.Key.ToString());

				var companyDetails = events[0].DataContext.GetEnterpriseServerAndCompanyIDs();
				AssertEquals("EDI|EDI|DAT", FormatEnterpriseServerAndCompanyIDs(companyDetails));
				AssertNull("DataTargetCollection", events[0].DataContext.DataTargetCollection);

				var factory = new BusinessObjectFactory();
				var query = new ZQuery(StmALogSchema.SL_Parent, declaration.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode);
				query.AddToFilter(StmALogSchema.SL_Reference, "HI");
				var logs = factory.Load<StmALog>(query);
				AssertEquals(1, logs.Length);
			});

			EDIMessage relatedEDIMessage = null;
			CombineAssertions("Sending BO log and linked EDIMessage", delegate
			{
				var messageBODEXLogs = messageBO.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "DEX"));
				AssertEquals("Message should have 1 data export log.", 1, messageBODEXLogs.Length);
				var dEXLog = messageBODEXLogs[0];
				relatedEDIMessage = dEXLog.RelatedEDIMessage.Message as EDIMessage;
				AssertNotNull("relatedEDIMessage should be linked to event", relatedEDIMessage);
				AssertEquals("relatedEDIMessage.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, relatedEDIMessage.EM_ApplicationCode);
				AssertEquals("relatedEDIMessage.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Internal, relatedEDIMessage.EM_ReceiveTransmit);
				AssertEquals("relatedEDIMessage.EM_MessageType", EDIMessageTypeList.Codes.XDC, relatedEDIMessage.EM_MessageType);
				AssertEquals("relatedEDIMessage.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, relatedEDIMessage.EM_MessageSubType);
				AssertEquals("relatedEDIMessage.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, relatedEDIMessage.EM_Status);
			});

			CombineAssertions("Receiving BO log and linked EDIMessage", delegate
			{
				var declarationBOMAALogs = declarationBO.Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, declaration.PK).AddToFilter(StmALogSchema.SL_SE_NKEvent, "MAA"));
				AssertEquals("declarationBO should have 1 Message Accepted log.", 1, declarationBOMAALogs.Length);
				var maaLog = declarationBOMAALogs[0];
				AssertEquals("Same EDIMessage should be linked to both the Exported and Imported Job.", relatedEDIMessage, maaLog.RelatedEDIMessage.Message);
			});
		}

		public void TestProcessInternalUniversalEvent_SendFail()
		{
			var forwarder = Factory.NewWithValidTestData<OrgHeader>();

			var communicationsMode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss;
			communicationsMode.EK_Destination = "DummyDestination";
			communicationsMode.EK_FileFormat = "ALL";
			communicationsMode.EK_Module = "BRK";
			forwarder.EDICommunicationsModes.Add(communicationsMode);

			var job = (BusinessObject)Factory.New<Customs.US.IJobDeclaration>();
			job[JobDeclarationSchema.JE_DeclarationReference] = "B000004";
			job[JobDeclarationSchema.JE_MessageType] = "EXP";
			job[JobDeclarationSchema.JE_ExportDate] = ZDateTime.Now;
			job[JobDeclarationSchema.JE_OH_Forwarder] = forwarder.PK;

			var logBO = job.GetLogs().AddNew(new EventValue(Events.ExportCustomsCleared, eventTime: new ZDateTimeOffset(2010, 12, 25)));
			Factory.Save();

			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalEvent(Factory, new RecipientRoleType[1] { RecipientRoleType.SPM }, (IWorkflowProviderEvent)job, logBO);
				Factory.Save();
			}

			CombineAssertions(delegate
			{
				AssertEquals(1, events.Length);
				AssertEquals(Events.DataImportFailureCode, events[0].EventType);
			});

			using (Factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalEvent(Factory, new RecipientRoleType[1] { RecipientRoleType.SPM }, (IWorkflowProviderEvent)job, ((IWorkflowProviderEvent)job).RecipientOrganisations, logBO);
				Factory.Save();
			}

			CombineAssertions(delegate
			{
				AssertEquals(1, events.Length);
				AssertEquals(Events.DataImportFailureCode, events[0].EventType);

				AssertNull("DataSourceCollection", events[0].DataContext.DataSourceCollection);
			});
		}

		public void TestPublishUniversalEventFactorySaves()
		{
			var contextValues = new[] { new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("MBOLNumber"), new ZString("MB100031")) };
			using (DummyWithWorkflowDataContextManager.SetupAdditionalContextKeyValuePairs(contextValues))
			{
				AssertNotNull(DummyWorkflowDescriptor.Instance);
				var dummyBO = Factory.New<DummyWithWorkflow>();
				dummyBO.Z0_Description = "XXX12345678";

				var logBO = dummyBO.GetLogs().AddNew(new EventValue(Events.ExportCustomsCleared, eventTime: new ZDateTimeOffset(2010, 12, 25)));

				Factory.Save();

				PublishUniversalXmlResult events;
				using (Factory.AddDisposableService())
				{
					int factoryCount = 0;
					BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) => factoryCount++);

					events = UniversalXmlWorkflowProcessor.PublishUniversalEvent(Factory, new RecipientRoleType[1] { RecipientRoleType.SPM }, dummyBO, logBO);

					//As Factory Save's are removed this should change.
					AssertLessThanOrEqualTo("More than X Factory Saves made.", factoryCount, 2);

					Factory.Save();
				}
			}
		}

		public void TestPublishUniversalShipmentFactorySaves()
		{
			var internalCompany = GlbCompany.CurrentCompany.OrgProxy;
			var shipment = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			Factory.Save();

			PublishUniversalXmlResult events;
			using (Factory.AddDisposableService())
			{
				int factoryCount = 0;
				BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) => factoryCount++);

				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(Factory, internalCompany, new RecipientRoleType[1] { RecipientRoleType.BRO }, (IWorkflowProvider)shipment);

				//As Factory Save's are removed this should change.
				AssertLessThanOrEqualTo("More than X Factory Saves made.", factoryCount, 3);

				Factory.Save();
			}
		}

		public void TestPublishUniversalXMLInternally_WithoutXmlSessionTracker()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B300234222";

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = "USI";
			message.EM_MessageType = "BS";
			message.EM_ReceiveTransmit = "RCV";
			var messageBO = (BusinessObject)message;
			Factory.Save();

			var eventDataObject = new UniversalEvent()
			{
				EventType = Events.MessageAcceptedCode,
				EventReference = "HI",
				DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext()
				{
					DataSourceCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataSource>(),
					DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget>()
				},
				ContextCollection = new List<Context>(new[] {
					new Context()
					{
						Type = nameof(UniversalEvent.ContextTypes.GoodsDescription),
						Value = "HELLO WORLD"
					}
				})
			};
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(company);
			eventDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B300234222");

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(Factory, messageBO, eventDataObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestPublishUniversalXMLInternally_ErrorReportsNonCriticalExceptions()
		{
			var declaration = Factory.New<Customs.IBaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B300234222";
			var declarationBO = (BusinessObject)declaration;

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = "USI";
			message.EM_MessageType = "BS";
			message.EM_ReceiveTransmit = "RCV";
			var messageBO = (BusinessObject)message;
			Factory.Save();

			var eventDataObject = new UniversalEvent()
			{
				EventType = Events.MessageAcceptedCode,
				EventReference = "HI",
				DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext()
				{
					DataSourceCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataSource>(),
					DataTargetCollection = new List<UniversalDataBuss.DataObjects.Universal._2011_11.DataTarget>()
				},
				ContextCollection = new List<Context>(new[] {
					new Context()
					{
						Type = nameof(UniversalEvent.ContextTypes.GoodsDescription),
						Value = "HELLO WORLD"
					}
				})
			};
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, "B300234222");

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				void ThrowExceptionFromHere() => throw new NullReferenceException("Oh no a Null Reference Exception :(");
				UniversalXmlWorkflowProcessor.SetPreProcessActionHook(ThrowExceptionFromHere);
				UniversalXmlWorkflowProcessor.PublishUniversalXMLInternally(Factory, messageBO, eventDataObject, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
			}

			AssertEquals(true, ErrorReporter.LastExceptionReported is NullReferenceException);
			AssertContains("ThrowExceptionFromHere", ErrorReporter.LastExceptionReported.StackTrace);
			ErrorReporter.Clear();
		}

		#endregion

		#region Implementation

		internal class DummyDataObjectWriterWithDataObjectValidationException : DummyDataObjectWriterWithException
		{
			public DummyDataObjectWriterWithDataObjectValidationException() : base(() => new DataObjectValidationException("Data Object Validation error detected as blah blah blah."))
			{
			}
		}

		internal abstract class DummyDataObjectWriterWithException : ITopLevelDataObjectWriter
		{
			internal DummyDataObjectWriterWithException(Func<Exception> getDataObjectWithException)
			{
				this.getDataObjectWithException = getDataObjectWithException;
			}

			ITopLevelDataObject ITopLevelDataObjectWriter.GetDataObject(BusinessObject sourceBO)
			{
				throw getDataObjectWithException();
			}

			DataContextType ITopLevelDataObjectWriter.TopLevelDataContextType
			{
				get { return DataContextType.AccountingInvoice; }
			}

			ZString ITopLevelDataObjectWriter.EDIMessageSubType
			{
				get { return EDIMessageSubTypeList.Codes.XmlUniversalTransaction; }
			}

			ZString ITopLevelDataObjectWriter.RootElementName
			{
				get { return "UniversalShipment"; }
			}

			readonly Func<Exception> getDataObjectWithException;
		}

		internal class DummyDataObjectWriterWithMessageProcessingBusinessFailureException : DummyDataObjectWriterWithException
		{
			public DummyDataObjectWriterWithMessageProcessingBusinessFailureException() : base(() => new MessageProcessingBusinessFailureException("Message Processing Business Failure error detected as blah blah blah."))
			{
			}
		}

		internal class DummyShipmentDataObjectWriter : ITopLevelDataObjectWriter
		{
			internal DummyShipmentDataObjectWriter()
			{
			}

			internal DummyShipmentDataObjectWriter(IDataWritingManager dataWritingManager)
			{
				this.dataWritingManager = dataWritingManager;
			}

			ITopLevelDataObject ITopLevelDataObjectWriter.GetDataObject(BusinessObject sourceBO)
			{
				var schema = dataWritingManager != null ? dataWritingManager.Schema.Namespace : UniversalXmlInfo.Namespace_2011_11;
				var dataContext = DataContextFactory.New(sourceBO.GetUniversalDataContextManager(), schema);
				var dataSource = dataContext.DataSourceCollection.First();
				dataSource.Type = "ForwardingConsol";
				dataSource.Key = "C00001000";
				var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
				};
				return universalShipment;
			}

			DataContextType ITopLevelDataObjectWriter.TopLevelDataContextType
			{
				get { return DataContextType.ForwardingShipment; }
			}

			ZString ITopLevelDataObjectWriter.EDIMessageSubType
			{
				get { return EDIMessageSubTypeList.Codes.XmlUniversalShipment; }
			}

			ZString ITopLevelDataObjectWriter.RootElementName
			{
				get { return "UniversalShipment"; }
			}

			readonly IDataWritingManager dataWritingManager;
		}

		internal class DummyForwardingShipmentDataObjectWriter : ITopLevelDataObjectWriter
		{
			ITopLevelDataObject ITopLevelDataObjectWriter.GetDataObject(BusinessObject sourceBO)
			{
				var dataContext = DataContextFactory.New(sourceBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2011_11);
				var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
				};
				return universalShipment;
			}

			DataContextType ITopLevelDataObjectWriter.TopLevelDataContextType
			{
				get { return DataContextType.ForwardingShipment; }
			}

			ZString ITopLevelDataObjectWriter.EDIMessageSubType
			{
				get { return EDIMessageSubTypeList.Codes.XmlUniversalShipment; }
			}

			ZString ITopLevelDataObjectWriter.RootElementName
			{
				get { return "UniversalShipment"; }
			}
		}

		internal class DummyAccountingInvoiceDataObjectWriter : ITopLevelDataObjectWriter
		{
			ITopLevelDataObject ITopLevelDataObjectWriter.GetDataObject(BusinessObject sourceBO)
			{
				var dataContext = DataContextFactory.New(sourceBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2011_11);
				var universalTransaction = new UniversalTransaction(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
				};
				if (sourceBO.HasRowErrors)
				{
					throw new DataObjectValidationException(sourceBO.RowErrors.First().Message);
				}
				return universalTransaction;
			}

			DataContextType ITopLevelDataObjectWriter.TopLevelDataContextType
			{
				get { return DataContextType.AccountingInvoice; }
			}

			ZString ITopLevelDataObjectWriter.EDIMessageSubType
			{
				get { return EDIMessageSubTypeList.Codes.XmlUniversalTransaction; }
			}

			ZString ITopLevelDataObjectWriter.RootElementName
			{
				get { return "UniversalTransaction"; }
			}
		}

		void AssertRelatedMessageContent(EDIMessage message, string expectedStatus, string recipientRoleCode, string expectedLogText)
		{
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Internal, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", expectedStatus, message.EM_Status);

			#region exptectedMessageText_ForTestProcessInternalUniversalShipmentTrigger

			const string exptectedMessageText_ForTestProcessInternalUniversalShipmentTrigger =
	@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C00001000</Key>
        </DataSource>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
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
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>{0}</Code>
          <Description>{1}</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			#endregion

			string recipientRoleDescription = MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetDescriptionFromCode(recipientRoleCode);
			AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(exptectedMessageText_ForTestProcessInternalUniversalShipmentTrigger, recipientRoleCode, recipientRoleDescription), message.EM_MessageText);
			AssertMultilineASCIIEquals("EDIMessage.Notes('Data Import Log Text').Text", expectedLogText.Trim(), message.GetLogNoteText());
		}

		void AssertForwardingShipmentMessageContent(EDIMessage message, string expectedStatus, string recipientRoleCode, string expectedLogText)
		{
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Internal, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", expectedStatus, message.EM_Status);

			#region Expected XML

			const string exptectedMessageText_ForTestProcessInternalUniversalShipmentTrigger = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001010</Key>
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
      <DataProvider>EDIDATEDI</DataProvider>
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
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2010-12-25T00:00:00.000+10:00</TriggerDate>
      <TriggerDescription>Send Data</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>
  </Shipment>
</UniversalShipment>";
			#endregion

			string recipientRoleDescription = MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetDescriptionFromCode(recipientRoleCode);
			AssertMultilineASCIIEquals("message.EM_MessageText", string.Format(exptectedMessageText_ForTestProcessInternalUniversalShipmentTrigger, recipientRoleCode, recipientRoleDescription), message.EM_MessageText);
			AssertMultilineASCIIEquals("EDIMessage.Notes('Data Import Log Text').Text", expectedLogText.Trim(), message.GetLogNoteText());
		}

		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}

		#endregion
	}
}
