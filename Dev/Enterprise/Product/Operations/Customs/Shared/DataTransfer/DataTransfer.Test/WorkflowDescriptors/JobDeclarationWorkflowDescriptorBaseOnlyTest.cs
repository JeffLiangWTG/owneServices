using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.WorkflowDescriptor;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationWorkflowDescriptor))]
	sealed class JobDeclarationWorkflowDescriptorBaseOnlyTest : JobDeclarationWorkflowDescriptorAbstractTest<BaseJobDeclaration, JobDeclarationWorkflowDescriptor>
	{
		public void TestGetEventDataModel() => CombineAssertions(() =>
		{
			var workflowDescriptor = new JobDeclarationWorkflowDescriptor();
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertType<JobDeclarationEventDataModel>("BaseJobDeclaration", workflowDescriptor.GetEventDataModel(declaration));
			var entryHeader = Factory.New<CusEntryHeader>();
			AssertType<BusinessObjectEventDataModel>("CusEntryHeader", workflowDescriptor.GetEventDataModel(entryHeader));
		});

		public void TestReturnLogActionWhenTheConditionsOfSupporterAreNotMet_CA()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "CA";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.CA.IJobDeclaration>());
			declaration.JE_MessageType = "EXP";
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction1 = trigger.ProcessTaskNotifications.AddNew();
			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
			var triggerAction2 = trigger.ProcessTaskNotifications.AddNew();
			triggerAction2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery;
			var triggerAction3 = trigger.ProcessTaskNotifications.AddNew();
			triggerAction3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ConsolidateLVXDeclaration;

			declaration.Logs.AddNew(Events.MessageValidationPassed);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			CombineAssertions(() =>
			{
				var workflowProcessor1 = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction1, new QueuedLogForTesting(triggerWTELog, trigger));
				AssertType<LogAction>("workflowProcessor1 should be LogAction", workflowProcessor1);
				var logger1 = new NotificationBuffer();
				workflowProcessor1.Process(logger1);
				AssertContains("Log For SB3", $"Trigger Action Type {WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message} is only allowed for IMP/LVS declarations. Declaration:", logger1.AsString);

				var workflowProcessor2 = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction2, new QueuedLogForTesting(triggerWTELog, trigger));
				AssertType<LogAction>("workflowProcessor2 should be LogAction", workflowProcessor2);
				var logger2 = new NotificationBuffer();
				workflowProcessor2.Process(logger2);
				AssertContains("Log For AVS", $"Trigger Action Type {WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery} is only allowed for IMP declarations. Declaration:", logger2.AsString);

				var workflowProcessor3 = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction3, new QueuedLogForTesting(triggerWTELog, trigger));
				AssertType<LogAction>("workflowProcessor3 should be LogAction", workflowProcessor3);
				var logger3 = new NotificationBuffer();
				workflowProcessor3.Process(logger3);
				AssertContains("Log For CLX", $"Trigger Action Type {WorkflowTriggerActionTypeConstants.Codes.ConsolidateLVXDeclaration} is only allowed for LVX declarations. Declaration:", logger3.AsString);
			});
		}

		public void TestGetAllAvailableTransportModeListCore()
		{
			var supportCodes = Country.LicenceKeyBuilderSupportedCountryCodes.Select(countryCode => (ZString)countryCode).ToList();
			var expectedTransportModeList = new CodeDescriptionPairList();
			expectedTransportModeList.AddPairsIfNotExist(JobDeclarationWorkflowDescriptor.GetAllAvailableTransportModeListCore(supportCodes));
			expectedTransportModeList.Sort();

			AssertEquals(@"- Transport mode not applicable (ZA)
AIR - Air Freight, Air (BR, NZ, ZA), 4 - Air (SG), Air (Non Container, Container) (40, 41) (US)
AUT - Auto (32) (US)
BWB - Border Water-borne(only Mexico and Canada) (12) (US)
FIX - Fixed Transport Installations, Fixed Transport Installations(Includes pipeline and powerhouse) (70) (US), Fixed Installations (ZA)
IWT - Inland Waterways
LAK - Lake (BR)
MAI - Post/Mail, Postal (BR), 5 - Mail (SG), Mail (50) (US), Post (ZA)
NOC - No Carrier/Hand-Carried (CA)
OTH - Other (AU, BR), 7 - Pipeline (SG), Transport mode not specified (ZA)
OWN - Own Propulsion, Own Means (BR)
PED - Pedestrian (33) (US)
PHC - Passenger Carried (CN), Passenger, hand carried (60) (US)
PST - Post (NZ)
RAI - Rail Freight, Rail (BR, ZA), 2 - Rail (SG), Rail (Non Container, Container) (20, 21) (US)
RIV - River (BR)
ROA - Road Freight, Road (BR, ZA), 3 - Road (SG), Road Other (Includes foot and animal borne) (34) (US)
ROR - Ro Ro Freight (GB)
SEA - Sea Freight, Sea (BR, NZ), 1 - Sea (SG), Sea (Non Container, Container) (10, 11) (US), Maritime (ZA)
TRK - Truck (Non Container, Container) (30, 31) (US)", expectedTransportModeList.ElementsAsString);
		}

		public void TestGetTransportModeList()
		{
			foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var declaration = Factory.New<BaseJobDeclaration>();
					var expectedTransportModeList = new CodeDescriptionPairList();
					expectedTransportModeList.AddPairsIfNotExist(declaration.Lookups.TransportTypeList.Cast<ICodeDescription>());
					if (countryCode == Core.Constants.CountryCodes.Australia)
					{
						expectedTransportModeList.AddPairIfNotExist(Core.Constants.TransportModes.Other, Core.Constants.TransportModeDescriptions.Other);
					}
					AssertContainsExactElementsInAnyOrder(countryCode + ".GetTransportModeList", expectedTransportModeList, JobDeclarationWorkflowDescriptor.GetTransportModeListForCountry(countryCode));
				}
			}
		}

		public void TestGetSecurityCheckPoint()
		{
			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			AssertEquals(true, trigger.GetWorkflowDescriptor().WorkflowTriggerActionTypeSecurityCheckPoints.Contains(new KeyValuePair<ZString, SecurityCheckpoint>(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message, Env.Security.CAB3MsgSend)));
		}

		public void TestGetMessageTypeList()
		{
			var list = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry(Core.Constants.CountryCodes.NewZealand);
			AssertContains("EXC", list.CodesAsString);
			list = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry(Core.Constants.CountryCodes.Singapore);
			AssertContains("COO", list.CodesAsString);
			list = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry(Core.Constants.CountryCodes.SouthAfrica);
			AssertContains("Ex-Bond", list.ElementsAsString);
			list = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertContains("FTZ", list.CodesAsString);
			list = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry(Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("ARN, DEP, EXP, IMP, MSC, ULR", list.CodesAsString);
			list = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry(Core.Constants.CountryCodes.Malta);
			AssertEquals("ARN, DEP, EXP, IMP, MSC, ULR", list.CodesAsString);

			// Standards
			var listAU = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry(Core.Constants.CountryCodes.Australia);
			var listMY = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry(Core.Constants.CountryCodes.Malaysia);
			var listDefault = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry("XXX");
			AssertNotEquals(listDefault.CodesAsString, listAU.CodesAsString);
			AssertEquals(listDefault.CodesAsString, listMY.CodesAsString);

			var listCA = JobDeclarationWorkflowDescriptor.GetMessageTypeListForCountry(Core.Constants.CountryCodes.Canada);
			Assert("Should have IMO for Canada", listCA.ContainsCode("IMO"));
		}

		public void TestBaseJobDeclarationsJobDocsAndCartageImplementsIWorkflowTriggerFieldChangeSourceProperly()
		{
			BaseJobDeclaration dec = Factory.New<BaseJobDeclaration>();
			IWorkflowTriggerFieldChangeSource provider = dec.DocsAndCartage as IWorkflowTriggerFieldChangeSource;
			AssertNotNull("BaseJobDeclaration.DocsAndCartage should implement IWorkflowTriggerFieldChangeSource", provider);
			var providers = provider.ParentWorkflowProviders;
			AssertNotNull(providers);
			AssertEquals(1, providers.Count);
			AssertEquals("{BaseJobDeclaration.DocsAndCartage}.ParentWorkflowProviders should provide the BaseJobDeclaration", dec, providers[0]);
		}

		public void TestMilestoneTemplateHintCaption()
		{
			AssertEquals("", WorkflowDescriptor.MilestoneTemplateHintCaption);
		}

		public void TestConditionList1()
		{
			AssertEquals(typeof(JobDeclarationWorkflowCondition1CodeList), WorkflowDescriptor.GetConditionList1(null).GetType());
		}

		public void TestConditionListForCA()
		{
			WorkflowDescriptor.LastProcessTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var task = Factory.New<ProcessTask>();
				var list = WorkflowDescriptor.GetConditionList1(task);
				Assert("Have IMO for Canada", list.ContainsCode("IMO"));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var task = Factory.New<ProcessTask>();
				var list = WorkflowDescriptor.GetConditionList1(task);
				Assert("Do not have IMO for non Canada", !list.ContainsCode("IMO"));
			}
		}

		public void TestConditionList2()
		{
			AssertEquals(typeof(JobDeclarationWorkflowCondition2CodeList), WorkflowDescriptor.GetConditionList2(null).GetType());
		}

		public void TestEstimateDefaultedFromList()
		{
			AssertEquals(typeof(DeclarationEstimateDefaultedFromList), WorkflowDescriptor.EstimateDefaultedFromList.GetType());
		}

		[ExpectNoExceptions]
		public void TestXmlMessageDeliveryAction()
		{
			GlbCompany dummyNZCompany = Factory.NewWithValidTestData<GlbCompany>();
			dummyNZCompany.GC_RN_NKCountryCode = "NZ";
			dummyNZCompany.Branches.AddNew().FillWithValidTestData();
			dummyNZCompany.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			GlbCompany dummyAUCompany = Factory.NewWithValidTestData<GlbCompany>();
			dummyAUCompany.GC_RN_NKCountryCode = "AU";
			dummyAUCompany.Branches.AddNew().FillWithValidTestData();
			Factory.Save();

			BaseJobDeclaration auDeclaration;
			ProcessTask task;
			using (DisposableEnvironment.ForBranch(dummyAUCompany.FirstActiveBranch.PK.ToGuid()))
			{
				auDeclaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.AU.IJobDeclaration>());
				auDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
				EDICommunicationsMode mode = auDeclaration.Importer.EDICommunicationsModes.AddNew();
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				mode.EK_Module = JobInvoicingConsumerTypes.Brokerage.Code;
				Bill bill = auDeclaration.Bills.AddNew();
				task = auDeclaration.WorkflowItems.Triggers.AddNew();
			}

			using (Factory.AddDisposableService())
			using (DisposableEnvironment.ForBranch(dummyNZCompany.FirstActiveBranch.PK.ToGuid()))
			{
				ProcessTaskNotification notification = task.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;

				XmlMessageDeliver xmlMessageDeliver = (XmlMessageDeliver)WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertEquals(0, auDeclaration.Logs.Find(x => x.Event.SE_Code == "DEX").Count());
				xmlMessageDeliver.Process(new NotificationBuffer());
				Factory.Save();
				AssertEquals(1, auDeclaration.Logs.Find(x => x.Event.SE_Code == "DEX").Count());
			}
		}

		public void TestSendCusEntryHeaderAsXmlUniversalEvent()
		{
			using (Factory.AddDisposableService())
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
				Factory.Save();

				var jobDeclaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
				var header = jobDeclaration.ActiveEntryHeaders.AddNew();
				var bill = jobDeclaration.Bills.AddNew();

				var trigger = jobDeclaration.WorkflowItems.Triggers.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatusCode;
				trigger.ReferenceCode = CustomsEntryStatusList.Codes.ClearElectronicInvoiceOriginal;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var communicationsMode = orgProxy.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = JobInvoicingConsumerTypes.Brokerage.Code;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "HYEDAUIKB";

				var logBO = header.GetLogs().AddNew(Events.CustomsEntryStatus, ZDateTimeOffset.UtcNow);
				Factory.Save();

				AssertNotEquals("Precondition: Trigger should now have an actual date / time recorded.", ZDateTime.Empty, trigger.P9_ActualDate);

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
				var triggerLogs = Factory.Load<StmALog>(query);
				AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

				var triggerLog = triggerLogs[0];
				AssertContains("triggerLog.SL_Reference", logBO.PK.ToString(), triggerLog.SL_Reference);

				var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);
				var logger = new NotificationBuffer();
				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("loggger results from processor.Process()", @"".Trim(), logger.AsString);

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, jobDeclaration.PK));
				AssertEquals("EDIMessages linked to JobDeclaration", 1, messages.Length);
			}
		}

		public void TestSendCusEntryHeaderAsXmlUniversalShipment()
		{
			using (Factory.AddDisposableService())
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = "US";
				Factory.Save();

				var jobDeclaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
				var header = jobDeclaration.ActiveEntryHeaders.AddNew();
				jobDeclaration.Bills.AddNew();

				var trigger = jobDeclaration.WorkflowItems.Triggers.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatusCode;
				trigger.ReferenceCode = CustomsEntryStatusList.Codes.ClearElectronicInvoiceOriginal;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var communicationsMode = orgProxy.EDICommunicationsModes.AddNew();
				communicationsMode.EK_Module = JobInvoicingConsumerTypes.Brokerage.Code;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationsMode.EK_Destination = "HYEDAUIKB";

				var logBO = header.GetLogs().AddNew(Events.CustomsEntryStatus, ZDateTimeOffset.UtcNow);
				Factory.Save();

				AssertNotEquals("Precondition: Trigger should now have an actual date / time recorded.", ZDateTime.Empty, trigger.P9_ActualDate);

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
				var triggerLogs = Factory.Load<StmALog>(query);
				AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

				var triggerLog = triggerLogs[0];
				AssertContains("triggerLog.SL_Reference", logBO.PK.ToString(), triggerLog.SL_Reference);

				var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);
				var logger = new NotificationBuffer();
				processor.Process(logger);
				Factory.Save();

				AssertMultilineASCIIEquals("loggger results from processor.Process()", @"".Trim(), logger.AsString);

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, jobDeclaration.PK));
				AssertEquals("EDIMessages linked to JobDeclaration", 1, messages.Length);
			}
		}

		public void TestScheduleB3MessageAction_US()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "US";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			AssertCollectionNotContains("Trigger action Schedule CAD Message is valid only for CA", "SB3", triggerAction.Lookups.WorkflowTriggerActionTypes);
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

			declaration.Logs.AddNew(Events.MessageValidationPassed);
			Factory.Save();

			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Schedule CAD Message is valid only for CA.");
		}

		public void TestScheduleB3MessageAction_CA()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "CA";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.CA.IJobDeclaration>());
			declaration.JE_MessageType = "EXP";
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

			declaration.Logs.AddNew(Events.MessageValidationPassed);
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Schedule CAD Message is valid only for IMP or LVS declaration.");

			declaration.JE_MessageType = "IMP";
			triggerAction.Validation.ValidateAll();
			Factory.Save();

			AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Schedule CAD Message is valid only for IMP or LVS declaration.");

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
			AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction()", "Enterprise.Customs.Business.BatchProcessor.CustomsStmProcessQueueCreatorProcessor", workflowProcessor.GetType().FullName);

			AssertNoExceptionThrown(() =>
			{
				var notification = new NotificationBuffer();
				workflowProcessor.Process(notification);
				Factory.Save();
			});

			var logger = new Enterprise.BatchProcessor.LoggingInformation();
			AssertNoExceptionThrown(() =>
			{
				var autoSendMessageProcessor = new BatchProcessor.AutoSendCustomsMessagingBatchProcessor(logger);
				autoSendMessageProcessor.ExecuteBatch();
			});
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var logs = new ZStringBuilder();
			var enumerator = logger.UserLogStrings.GetEnumerator();
			while (enumerator.MoveNext())
			{
				logs.Append(enumerator.Current.Trim());
			}

			AssertContains("Processing: Declaration B00001000", logs.ToStringWithNewLineBetweenAppends());
			AssertContains("Sending CAD message for Declaration B00001000 is not allowed", logs.ToStringWithNewLineBetweenAppends());
		}

		public void TestSubmitAVSQueryAction_US()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "US";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			AssertCollectionNotContains("Trigger action Submit AVS Query is valid only for CA", "AVS", triggerAction.Lookups.WorkflowTriggerActionTypes);
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery;
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

			declaration.Logs.AddNew(Events.MessageValidationPassed);
			Factory.Save();

			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Submit AVS Query is valid only for CA.");
		}

		public void TestSubmitAVSQueryAction_CA()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "CA";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.CA.IJobDeclaration>());
			declaration.JE_MessageType = "LVS";
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery;
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

			declaration.Logs.AddNew(Events.MessageValidationPassed);
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Submit AVS Query is valid only for IMP declaration.");

			declaration.JE_MessageType = "IMP";
			triggerAction.Validation.ValidateAll();
			Factory.Save();

			AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Submit AVS Query is valid only for IMP declaration.");

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
			AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction()", "Enterprise.Customs.CA.Business.SubmitAVSQueryProcessor", workflowProcessor.GetType().FullName);
		}

		public void TestConsolidateLVXDeclarationAction_US()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "US";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			AssertCollectionNotContains("Trigger action Consolidate LVX Declaration is valid only for CA", "CLX", triggerAction.Lookups.WorkflowTriggerActionTypes);
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ConsolidateLVXDeclaration;
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

			declaration.Logs.AddNew(Events.MessageValidationPassed);
			Factory.Save();

			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Consolidate LVX Declaration is valid only for CA.");
		}

		public void TestConsolidateLVXDeclarationAction_CA()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "CA";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.CA.IJobDeclaration>());
			declaration.JE_MessageType = "IMP";
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ConsolidateLVXDeclaration;
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

			declaration.Logs.AddNew(Events.MessageValidationPassed);
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Consolidate LVX Declaration is valid only for LVX declaration.");

			declaration.JE_MessageType = "LVX";
			triggerAction.Validation.ValidateAll();
			Factory.Save();

			AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Consolidate LVX Declaration is valid only for LVX declaration.");

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
			AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction()", "Enterprise.Customs.CA.Business.ConsolidateLVXDeclarationProcessor", workflowProcessor.GetType().FullName);

			AssertNoExceptionThrown(() =>
			{
				var notification = new NotificationBuffer();
				workflowProcessor.Process(notification);
				Factory.Save();
			});
		}

		public void TestSynchronizeWithBondedWarehouse()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var helper = new WhsDataTestHelper(Factory);
				var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
				var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
				var whsInventory = helper.GetNewReceiveInventory(whsReceive, helper.Part, ZString.Empty, 1m, 10m, 10m, "NO", "PATT1", "PATT2", "PATT3", ZString.Empty, "EN00123-1", ZDateTimeOffset.Today.AddMonths(-1));
				var whsReceiveLine1CustomsData = helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 150m, 20m, "KG", "AU", ZDecimal.Zero, "",
					string.Format("{0}=CSC1*{1}=5*{2}=NO*{3}=AU", AUAddInfoSchema.ZA_CSC.Name.Substring(3), AUAddInfoSchema.ZA_WRQ.Name.Substring(3), AUAddInfoSchema.ZA_WRU.Name.Substring(3), AUAddInfoSchema.ZA_ORG.Name.Substring(3)),
					"EN00123", (ZShort)1, 50m, "AUD");

				helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
				whsReceive.FinaliseDocketWithoutUserConfirmation();
				whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
				Factory.Save();

				var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.AU.IJobDeclaration>());
				declaration.JE_MessageType = "IMP";
				declaration.JE_OH_Importer = helper.Importer.PK;
				var trigger = declaration.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Synchronise";
				trigger.TriggerConditions.TriggerEventCode = Events.DataImport.Code;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SynchronizeWithBondedWarehouse;

				declaration.Logs.AddNew(Events.DataImport);
				AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
				AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Synchronize with Inventory is valid only for warehouse jobs with Inventory Management Integration enabled.");

				declaration.JE_MessageType = "EXW";
				triggerAction.Validation.ValidateAll();
				Factory.Save();

				AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
				AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Synchronize with Inventory is valid only for warehouse jobs with Inventory Management Integration enabled.");

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
				var triggerWTELog = triggerWTELogs[0];

				var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
				var logger = new NotificationBuffer();
				workflowProcessor.Process(logger);
				AssertXMLEquals("No matching Warehouse found.", logger.AsString.Trim());

				declaration.WarehouseDocAddress.E2_OA_Address = helper.Warehouse.MainAddress.PK;
				logger.Clear();
				workflowProcessor.Process(logger);
				AssertXMLEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", logger.AsString.Trim());

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
				invoiceLine.JI_PartAttrib1 = "PATT1";
				invoiceLine.JI_PartAttrib2 = "PATT2";
				invoiceLine.JI_PartAttrib3 = "PATT3";
				invoiceLine.JI_CustomsUnitQty = "KG";
				invoiceLine.JI_InvoiceQuantity = 5m;
				logger.Clear();
				workflowProcessor.Process(logger);
				AssertXMLEquals("", logger.AsString);
				AssertEquals("JI_LinePrice", 75m, invoiceLine.JI_LinePrice);
			}
		}

		public void TestSynchronizeWithBondedWarehouse_WithSerialNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var helper = new WhsDataTestHelper(Factory);
				var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "N10");
				var whsReceive = helper.GetNewWhsReceive(whsWarehouse.PK, helper.Importer.PK);
				var whsInventory = helper.GetNewReceiveInventory(whsReceive, helper.Part, ZString.Empty, 1m, 1m, 1m, "NO", "PATT1", "PATT2", "PATT3", "SERNUM", "EN00123-1", ZDateTimeOffset.Today.AddMonths(-1));
				var whsReceiveLine1CustomsData = helper.GetNewWhsBondedWarehouseAttribute(whsInventory.WI_WE_InDocketLine, 150m, 20m, "KG", "AU", ZDecimal.Zero, "",
					string.Format("{0}=CSC1*{1}=5*{2}=NO*{3}=AU", AUAddInfoSchema.ZA_CSC.Name.Substring(3), AUAddInfoSchema.ZA_WRQ.Name.Substring(3), AUAddInfoSchema.ZA_WRU.Name.Substring(3), AUAddInfoSchema.ZA_ORG.Name.Substring(3)),
					"EN00123", (ZShort)1, 50m, "AUD");

				helper.WhsHelper.WhsReceiveAllocateLocationsMock(whsReceive.PK);
				whsReceive.FinaliseDocketWithoutUserConfirmation();
				whsInventory.WI_InventoryStatus = WhsDataTestHelper.InventoryAvailableStatusCode;
				Factory.Save();

				var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.AU.IJobDeclaration>());
				declaration.JE_MessageType = "IMP";
				declaration.JE_OH_Importer = helper.Importer.PK;
				var trigger = declaration.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Synchronise";
				trigger.TriggerConditions.TriggerEventCode = Events.DataImport.Code;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SynchronizeWithBondedWarehouse;

				declaration.Logs.AddNew(Events.DataImport);
				AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
				AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Synchronize with Inventory is valid only for warehouse jobs with Inventory Management Integration enabled.");

				declaration.JE_MessageType = "EXW";
				triggerAction.Validation.ValidateAll();
				Factory.Save();

				AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
				AssertNoError(triggerAction.PQ_TriggerTypeInfo, "Trigger action Synchronize with Inventory is valid only for warehouse jobs with Inventory Management Integration enabled.");

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
				var triggerWTELog = triggerWTELogs[0];

				var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
				var logger = new NotificationBuffer();
				workflowProcessor.Process(logger);
				AssertXMLEquals("No matching Warehouse found.", logger.AsString.Trim());

				declaration.WarehouseDocAddress.E2_OA_Address = helper.Warehouse.MainAddress.PK;
				logger.Clear();
				workflowProcessor.Process(logger);
				AssertXMLEquals("At least one Invoice Line with valid Previous Entry Details or Part and Invoice Quantity is required.", logger.AsString.Trim());

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_PartNo = helper.Part.OP_PartNum;
				invoiceLine.JI_PartAttrib1 = "PATT1";
				invoiceLine.JI_PartAttrib2 = "PATT2";
				invoiceLine.JI_PartAttrib3 = "PATT3";
				invoiceLine.JI_SerialNumber = "SERNUM";
				invoiceLine.JI_CustomsUnitQty = "KG";
				invoiceLine.JI_InvoiceQuantity = 1m;
				logger.Clear();
				workflowProcessor.Process(logger);
				AssertXMLEquals("", logger.AsString);
				AssertEquals("JI_LinePrice", 150m, invoiceLine.JI_LinePrice);
			}
		}

		[ExpectNoExceptions]
		public void TestGetWorkflowTriggerActionCoreWithNoException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.NZ.IJobDeclaration>());
				declaration.JE_MessageType = "IMP";
				var trigger = declaration.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Validate for Customs Messaging";
				trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

				var triggerAction = trigger.ProcessTaskNotifications.AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance;
				declaration.Logs.AddNew(Events.MessageValidationPassed);
				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
				var triggerWTELog = triggerWTELogs[0];

				var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
				AssertNull(declaration.Job);
				AssertType<LogAction>(workflowProcessor);
			}
		}

		public void TestSendEntryDeclarationMessageAction_US()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "US";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
			declaration.JE_MessageType = "EXP";
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send Entry/Declaration Message";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

			declaration.JE_MessageType = "IMP";
			((Integration.Customs.US.IJobDeclaration)declaration).US_EnableENS = false;
			triggerAction.Validation.ValidateAll();
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "For import declarations, the Send Entry/Declaration Message trigger is only supported when the message mode is 'ACE' and entry summary is enabled.");
		}

		public void TestSendEntryDeclarationMessageAction_CA()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "CA";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.CA.IJobDeclaration>());
			declaration.JE_MessageType = "EXP";
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send Entry/Declaration Message";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;
			AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", true, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
			AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", true, triggerAction.PQ_EmailAddrInfo.ReadOnly);

			declaration.JE_MessageType = "IMP";
			triggerAction.Validation.ValidateAll();
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "The Send Entry/Declaration Message trigger is only supported for export declarations.");
		}

		public void TestSendReleaseMessageAction_US()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "US";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
			declaration.JE_MessageType = "EXP";
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send Release Message";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage;
			triggerAction.Validation.ValidateAll();
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "The Send Release Message trigger is only supported for import declaration and message mode is 'ACE' and cargo release is enabled.");
		}

		public void TestSendReleaseMessageAction_CA()
		{
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_RN_NKCountryCode = "CA";

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.CA.IJobDeclaration>());
			declaration.JE_MessageType = "EXP";
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Send Release Message";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage;
			triggerAction.Validation.ValidateAll();
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Enter a valid Action.");
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "The Send Release Message trigger is only supported for import declarations and shipment type is not 'LVS' and 'LVX'.");
		}

		public void TestGetAction_NoNull()
		{
			// Set valid workflow provider for descriptor under test
			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);

			// Set invalid trigger action type for descriptor under test
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage;
			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(wteLog, trigger));
			AssertNull(processor);
		}

		public void TestGetLogActionWithNonSupportReason()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var trigger01 = declaration.WorkflowItems.Triggers.AddNew();
			var trigger02 = declaration.WorkflowItems.Triggers.AddNew();
			trigger01.P9_Description = "Send Entry/Declaration Message";
			trigger02.P9_Description = "Send Release Message";
			trigger01.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger02.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var wteLog01 = trigger01.Logs.AddNew(Events.WorkflowTriggerEvent);
			var wteLog02 = trigger02.Logs.AddNew(Events.WorkflowTriggerEvent);

			var triggerAction01 = trigger01.ProcessTaskNotifications.AddNew();
			var triggerAction02 = trigger02.ProcessTaskNotifications.AddNew();
			triggerAction01.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;
			triggerAction02.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage;
			var processor01 = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction01, new QueuedLogForTesting(wteLog01, trigger01));
			var processor02 = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction02, new QueuedLogForTesting(wteLog02, trigger02));
			var notification = new NotificationBuffer();
			processor01.Process(notification);
			processor02.Process(notification);
			Factory.Save();

			var logger = new Enterprise.BatchProcessor.LoggingInformation();
			var autoSendMessageProcessor = new BatchProcessor.AutoSendCustomsMessagingBatchProcessor(logger);
			autoSendMessageProcessor.ExecuteBatch();
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var logs = new ZStringBuilder();
			var enumerator = logger.UserLogStrings.GetEnumerator();
			while (enumerator.MoveNext())
			{
				logs.Append(enumerator.Current.Trim());
			}

			AssertContains("The Send Entry/Declaration Message trigger is not supported to be sent for ER.", logs.ToStringWithNewLineBetweenAppends());
			AssertContains("The Send Release Message trigger is not supported to be sent for ER.", logs.ToStringWithNewLineBetweenAppends());
		}

		public void TestCustomsMessageActions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var template = Factory.New<ProcessTaskTemplate>();
				var trigger = template.WorkflowItems.Triggers.AddNew();
				var testItem = new JobDeclarationWorkflowDescriptor();

				CombineAssertions(() =>
				{
					template.GlobalTemplate = false;
					var testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
					Assert("Should contain VCM for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));
					Assert("Should contain SB3 for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message));
					Assert("Should contain SEM for non global Templates", testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage));

					template.GlobalTemplate = true;
					testResult = testItem.GetWorkflowTriggerActionTypes(trigger, template);
					Assert("Should NOT contain VCM for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging));
					Assert("Should NOT contain SB3 for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message));
					Assert("Should NOT contain SEM for global Templates", !testResult.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage));
				});
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionOnTriggerActionWhenDeclarationCompanyIsUSButTriggerCompanyIsCA()
		{
			var canadaCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			canadaCompany.GC_RN_NKCountryCode = "CA";
			var unitedStatesCompany = Factory.NewWithValidTestData<GlbCompany>();
			unitedStatesCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = unitedStatesCompany.Branches.AddNew();
			Factory.Save();

			var declaration = (BaseJobDeclaration)Factory.New(ObjectFactory.GetType<Integration.Customs.US.IJobDeclaration>());
			declaration.JE_MessageType = "IMP";
			declaration.JE_GB = usBranch.PK;
			var trigger = declaration.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "ADD DOCUMENT TO eDocs";
			trigger.TriggerConditions.TriggerEventCode = Events.MessageValidationPassed.Code;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.P9_GC = canadaCompany.PK;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			AssertNoExceptionThrown("No exception should be thrown", () =>
			{
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue;
				triggerAction.Validation.ValidateAll();
			});
		}

		public void TestValidationToolSettings()
		{
			AssertType<JobDeclarationValidationToolSettings>(new JobDeclarationWorkflowDescriptor().ValidationToolSettings);
		}
	}
}
