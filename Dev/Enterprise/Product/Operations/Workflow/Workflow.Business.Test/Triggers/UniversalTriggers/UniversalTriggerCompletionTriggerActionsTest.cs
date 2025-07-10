using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test.UniversalTriggers
{
	class UniversalTriggerCompletionTriggerActionsTest : WorkflowTestCase
	{
		public void TestTriggerAction_FieldChange()
		{
			var template = CreateTemplate(Factory, "WKI", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.SetField);
			triggerAction.PQ_FieldName = "<WKI_Summary>";
			triggerAction.PQ_FieldValue = "<WKI_Summary>. Schwifty song.";

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			var bizo = (BusinessObject)job;
			job.WKI_Summary = "I'm Mr Bulldops";

			bizo.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			RunLogWalker();

			bizo.Reload();
			AssertEquals("I'm Mr Bulldops. Schwifty song.", job.WKI_Summary);
		}

		public void TestProcessLogs_Universal()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);
			var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.SetField);
			triggerAction.PQ_FieldName = "<WKI_Summary>";
			triggerAction.PQ_FieldValue = "<WKI_Summary>. Schwifty song.";

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyEnterpriseBusinessObjectWithWorkflow>();

			Factory.Save();

			var logParent1 = Factory.NewWithValidTestData<DummyEnterpriseBusinessObjectWithWorkflow>();

			var subscripion = Factory.NewWithValidTestData<StmEventSubscription>();
			subscripion.SES_AgentDescriptor = DummyWorkflowDescriptor.Instance.Code;

			Factory.Save();

			var logs = new[]
			{
					new QueuedLogForTesting(Factory)
					{
						PK = ZGuid.NewZGuid(),
						SJ_TargetID = subscripion.PK,
						SJ_Reference = "logs[0]",
						SJ_ParentID = logParent1.PK,
						SJ_SE_NKEvent = AutoEvents.CustomisableEvent00Code,
						SJ_EventTime = new ZDateTime(2017, 6, 1)
					},
				};

			DummyWorkflowDescriptor.Instance.ExpectLogParentsToFireWorkflowForEvent(subscripion, WorkflowEventsPublisher.PublishedEvent.Create(logs[0]), new[] { logParent1 });
			new WorkflowEventsPublisherForTest().ProcessLogs(logs);

			var link = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, logParent1.PK)).Single();
			Assert(!((IWorkflowTrigger)link).LastFiredTime.IsEmpty);
		}

		public void TestUniversalTriggerActions_ContextIsAlways_EVT()
		{
			var template = CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true);
			template.GlobalTemplate = true;
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);
			var job = Factory.New<Forwarding.IForwardingShipment>();
			Factory.Save();

			((BusinessObject)job).GetLogs().AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			var link = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK)).Single();
			AssertEquals("Trigger was added by current company", Env.CurrentCompanyPK, link.P9L_GC_Company);
			RunLogWalker();

			var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.SetField);
			triggerAction.PQ_FieldName = "<JS_GoodsDescription>";
			triggerAction.PQ_FieldValue = "Test";
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var user = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(user.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				((BusinessObject)job).GetLogs().AddNew(AutoEvents.CustomisableEvent00);
				Factory.Save();
			}

			AssertEquals("Precondition: No EDT logs", 0, ((BusinessObject)job).GetLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode).Count());
			AssertNotEquals("Precondition: Field is not set", "Test", job.JS_GoodsDescription);

			RunLogWalker();

			var factory = new BusinessObjectFactory();
			job = factory.Load<Forwarding.IForwardingShipment>(job.PK);
			AssertEquals("Field set by FLD action", "Test", job.JS_GoodsDescription);

			var edtLog = ((BusinessObject)job).GetLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode).First();
			AssertNotNull(edtLog);

			CombineAssertions("The trigger actions for Universal Triggers should always run in the context of the triggering event", () =>
			{
				AssertEquals(company.GC_Code, edtLog.CompanyCode);
				AssertEquals(branch.GB_Code, edtLog.SL_GB_NKBranch);
				AssertEquals(department.GE_Code, edtLog.SL_GE_NKDepartment);
				AssertEquals(user.PK, edtLog.User.PK);
			});
		}

		[Serializable]
		class WorkflowEventsPublisherForTest : WorkflowEventsPublisher
		{
			internal void ProcessLogs(IQueuedLog[] logs)
			{
				ProcessLogQueueItems(logs);
			}
		}

		public void TestTriggerAction_SendEmailNotification()
		{
			var template = CreateTemplate(Factory, "WKI", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail);
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "this@daveeast.com";
			triggerAction.PQ_EmailText = "Work Item [(*WKI_Summary*)] was tagged";

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			var bizo = (BusinessObject)job;
			job.WKI_Summary = "Uh oh. You gotta get schwifty.";

			bizo.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			RunLogWalker();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertContains("Work Item [Uh oh. You gotta get schwifty.] was tagged", email.Body);
		}

		public void TestTriggerAction_ApplyWorkflowTemplate()
		{
			var partialTemplate = CreateTemplate(Factory, "WKI", isPartial: true);
			var templateTask = CreateTask(partialTemplate, "On the floor");

			var template = CreateTemplate(Factory, "WKI", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce);
			triggerAction.PQ_P0_WorkflowTemplate = partialTemplate.PK;

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			var bizo = (BusinessObject)job;

			job.WKI_Summary = "I'm Mr Bulldops";

			bizo.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			Factory.Save();

			AssertEquals(0, ((IWorkflowProvider)job).WorkflowItems.Tasks.Count);
			RunLogWalker();

			job = bizo.Factory.CreateNewFactory().Load<IWorkItem>(bizo.PK);
			AssertEquals(1, ((IWorkflowProvider)job).WorkflowItems.Tasks.Count);
			AssertEquals("On the floor", ((IWorkflowProvider)job).WorkflowItems.Tasks[0].P9_Description);
		}

		public void TestTriggerAction_SendUniversalXml_ShouldNotThrowException()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var company = GlbCompany.GetCurrentCompany(Factory);
				var mode = company.OrgProxy.EDICommunicationsModes.AddNew();

				mode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				mode.EK_Destination = "Europa";
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				mode.EK_Module = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

				var template = CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true);
				var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
				var triggerAction = CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML);
				triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				Factory.Save();

				var job = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

				job.FillWithValidTestData();

				job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
				Factory.Save();

				RunLogWalker();

				var messages = Factory.Load<IEDIMessage>(new ZQuery());

				AssertEquals(1, messages.Length);
				AssertEquals(job.PK, messages[0].EM_LinkUniqueID);
			}
		}
	}
}
