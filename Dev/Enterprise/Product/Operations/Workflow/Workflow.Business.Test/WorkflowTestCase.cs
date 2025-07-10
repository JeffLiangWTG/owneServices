using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.WorkflowManager.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Workflow.Business.Test
{
	public abstract class WorkflowTestCase : TestCaseWithFactory
	{
		#region Buffer Management

		public static IBMTestHelper BMTestHelper
		{
			get { return ObjectFactory.Get<IBMTestHelper>(); }
		}

		public static void EnableBufferManagement()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
		}

		#endregion

		#region Quality Iterations

		public static void SetAsQCBTaskType(string type, string workflowType)
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType(type, workflowType);
		}

		public static void AddIterationReasonToRegistry(string workflowType, string code, string description)
		{
			MasterFilesTestHelper.AddIterationReasonToRegistry(workflowType, code, description);
		}

		public static void ClearIterationReasonsFromRegistry(string workflowType)
		{
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry(workflowType);
		}

		#endregion

		#region Assertions

		public static void AssertRtfText(string expectedText, ZBlob rawRTF)
		{
			AssertRtfText(string.Empty, expectedText, rawRTF);
		}

		public static void AssertRtfText(string message, string expectedText, ZBlob rawRtf)
		{
			MasterFilesTestHelper.AssertRtfText(message, expectedText, rawRtf);
		}

		public static void AssertIRootTypeProviderImplementation(IBaseTrigger trigger)
		{
			var rootTypeProvider = (IRootTypeProvider)trigger;
			var job = trigger.GetParent();

			var rootTypes = rootTypeProvider.RootTypes;
			AssertEquals(2, rootTypes.Length);
			AssertEquals(typeof(OrgHeader), rootTypes[0]);

			var triggerType = trigger is ProcessTask ? trigger.GetType() : typeof(ProcessJobTriggerLink);

			AssertEquals(triggerType, rootTypes[1]);

			var roots = rootTypeProvider.Roots;

			if (trigger is IWorkflowTrigger)
			{
				AssertEquals(2, roots.Length);
				AssertEquals(job, roots[0]);
				AssertEquals(trigger, roots[1]);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(Array.Empty<BusinessObject>(), roots);
			}
		}

		public static void AssertUniversalTriggerFired<TWorkflowProvider>(TWorkflowProvider job, string eventCode, bool shouldHaveFired = true)
			where TWorkflowProvider : BusinessObject, IWorkflowProvider
		{
			AssertUniversalTriggerFired($"Trigger for event {eventCode} should{(shouldHaveFired ? "" : " NOT")} have fired for job: {job}", job, eventCode, shouldHaveFired);
		}

		public static void AssertUniversalTriggerFired<TWorkflowProvider>(string message, TWorkflowProvider job, string eventCode, bool shouldHaveFired = true)
			where TWorkflowProvider : BusinessObject, IWorkflowProvider
		{
			var query = new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK);
			var trigger = job.Factory.Load<ProcessJobTriggerLink>(query).SingleOrDefault(t => t.TemplateTrigger.P9T_SE_NKTriggerEvent == eventCode);

			if (shouldHaveFired)
			{
				AssertNotNull(message, trigger);
			}
			else
			{
				AssertNull(message, trigger);
			}
		}

		#endregion

		#region Create Business Objects

		public static ProcessTaskTemplate CreateTemplate(BusinessObjectFactory factory, string processType = "ORG", bool isUniversal = false, bool isPartial = false, string subType1 = null, string subType2 = null, string subType3 = null, string subType4 = null, string subType5 = null, string triggerFallbackMethod = null)
		{
			var template = factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = processType;
			template.P0_IsUniversal = isUniversal;
			template.P0_IsPartialTemplate = isPartial;
			template.P0_SubType1 = subType1;
			template.P0_SubType2 = subType2;
			template.P0_SubType3 = subType3;
			template.P0_SubType4 = subType4;
			template.P0_SubType5 = subType5;

			if (isUniversal && triggerFallbackMethod == null)
			{
				template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;
			}
			else if (triggerFallbackMethod != null)
			{
				template.P0_TriggerFallbackMethod = triggerFallbackMethod;
			}

			return template;
		}

		public static ProcessTemplateTrigger CreateTrigger(BusinessObjectFactory factory, string eventCode = null, string triggerFieldName = null)
		{
			var template = CreateTemplate(factory);

			return CreateTrigger(template, eventCode ?? AutoEvents.EditedARecordCode, triggerFieldName);
		}

		public static ProcessTemplateTrigger CreateTrigger(ProcessTaskTemplate template, string eventCode = null, string triggerFieldName = null)
		{
			var trigger = (ProcessTemplateTrigger)template.TemplateTriggers.AddNew();
			trigger.FillWithValidTestData();

			if (eventCode != null)
			{
				trigger.TriggerConditions.TriggerEventCode = eventCode;
			}

			if (triggerFieldName != null)
			{
				trigger.TriggerConditions.TriggerFieldName = triggerFieldName;
			}

			return trigger;
		}

		public static ProcessTaskNotification CreateTriggerAction(IBaseTrigger trigger, string triggerActionType, string emailAddress = null)
		{
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = triggerActionType;

			if (emailAddress != null)
			{
				triggerAction.PQ_EmailAddr = emailAddress;
				triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			}

			return triggerAction;
		}

		public static ProcessTask CreateTask(ProcessTaskTemplate template, string description, int? sequence = null)
		{
			var task = template.WorkflowItems.Tasks.AddNew();

			task.P9_Description = description;

			if (sequence != null)
			{
				task.P9_Sequence = sequence.Value;
			}

			return task;
		}

		public static ProcessJobTriggerLink CreateJobTriggerLink(BusinessObjectFactory factory)
		{
			var job = factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = CreateTrigger(factory);

			return CreateJobTriggerLink(trigger, job);
		}

		public static ProcessJobTriggerLink CreateJobTriggerLink(ProcessTemplateTrigger trigger, BusinessObject job)
		{
			var link = trigger.Factory.New<ProcessJobTriggerLink>();

			link.P9L_P9T_TemplateTrigger = trigger.PK;
			link.P9L_ParentId = job.PK;
			link.P9L_ParentTableCode = job.TablePrefix;

			link.FillWithValidTestData();

			return link;
		}

		#endregion

		#region Helper Functions

		public static string RunLogWalker()
		{
			return MasterFilesTestHelper.RunLogWalker();
		}

		public static void RunModifiedFieldChangeTriggerServiceTask()
		{
			var logger = new SimpleLogger();
			var serviceTask = new WorkflowModifiedFieldChangeTriggerServiceTask();

			serviceTask.ServiceLogger = logger;
			serviceTask.RunTask();
		}

		public static ProcessJobTriggerLink FindJobTriggerLink(IBusiness job, ITemplateTrigger trigger, BusinessObjectFactory factory = null)
		{
			var query = new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.Identifier).AddToFilter(ProcessJobTriggerLinkSchema.P9L_P9T_TemplateTrigger, trigger.Identifier);
			return (factory ?? job.Factory).LoadTop1<ProcessJobTriggerLink>(query);
		}

		public static IProcessHeader CreateQualityIteration(IProcessTask containmentBarrierTask, IProcessTask iterateFromTask, string resourceUnderReviewStaffCode = null, string iterationReasonCode = null)
		{
			return PublishedWorkflowTestHelper.CreateQualityIteration(containmentBarrierTask, iterateFromTask, resourceUnderReviewStaffCode, iterationReasonCode);
		}

		#endregion

		#region SetUp / Tear Down

		protected override void SetUp()
		{
			base.SetUp();

			disposables = new DisposableList(new IDisposable[]
			{
				new DisposableAction(() => DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow), () => DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null)
			});

			TypeDecider.AddSubstitution(typeof(WorkflowSupportableTableNames), typeof(WorkflowSupportableTableNamesWithDummy));
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		DisposableList disposables;

		#endregion
	}
}
