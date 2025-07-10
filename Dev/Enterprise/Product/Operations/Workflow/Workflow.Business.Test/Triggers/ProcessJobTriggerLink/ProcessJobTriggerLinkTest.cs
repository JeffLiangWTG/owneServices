using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Workflow.Business.Test
{
	class ProcessJobTriggerLinkTest : WorkflowTestCase
	{
		public void TestConcurrencyPolicy()
		{
			var bizo = Factory.New<ProcessJobTriggerLink>();
			var row = ((INeedRow)bizo).Row;
			var concurrencyPolicy = ConcurrencyInfo.Get(row, row.Table.Columns[nameof(ProcessJobTriggerLink.P9L_TriggerFiredCountdown)]);
			AssertEquals(ConcurrencyPolicy.Ignore, concurrencyPolicy);
		}

		public void TestDontOverflow()
		{
			var template = CreateTemplate(Factory, isUniversal: true, processType: "DUM");
			var trigger = CreateTrigger(template);
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			Factory.Save();

			var j1 = Factory.CreateNewFactory().Load<DummyWithWorkflow>(job.PK);
			trigger = j1.Factory.Load<ProcessTemplateTrigger>(trigger.PK);
			var link = CreateJobTriggerLink(trigger, j1);

			link.P9L_TriggerFiredCountdown = short.MinValue;
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = ZDateTime.Now;
				log.SL_Parent = job.PK;
			}
			AssertNoExceptionThrown(() => ((IBaseTrigger)link).SetEventTime(log, job, ZDateTimeOffset.Now));
		}

		#region Company

		public void TestCompany_SetOnCreate()
		{
			var link = CreateJobTriggerLink(Factory);

			AssertEquals(Env.CurrentCompanyPK, link.P9L_GC_Company);
		}

		#endregion

		#region IRootTypeProvider Members

		public void TestIRootTypeProviderMembers()
		{
			var template = CreateTemplate(Factory, isUniversal: true);
			var trigger = CreateTrigger(template);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var triggerLink = CreateJobTriggerLink(trigger, job);

			AssertIRootTypeProviderImplementation(triggerLink);

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			var ghostedTrigger = (ProcessTask)job.WorkflowItems.TriggersIncludingRelated.Single();
			AssertIRootTypeProviderImplementation(ghostedTrigger);
		}

		#endregion

		#region Delete

		public void TestDeletingProcessTaskCollection_DeletesJobTriggerLink()
		{
			var template = CreateTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode, isUniversal: true);
			CreateTrigger(template, AutoEvents.CustomisableEvent00Code);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			var trigger = job.WorkflowItems.TriggersIncludingRelated.First() as ProcessTask;
			AssertEquals(false, trigger.HasProcessJobTriggerLink);
			job.Logs.AddNew(AutoEvents.CustomisableEvent00);
			AssertEquals(true, trigger.HasProcessJobTriggerLink);
			var triggerLink = Factory.LoadTop1<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK));
			AssertNotNull(triggerLink);
			job.WorkflowItems.RemoveAndDeleteAll();
			AssertEquals(true, triggerLink.IsDeleted);
			AssertEquals(false, trigger.HasProcessJobTriggerLink);
		}

		public void TestDontRecreateDeletedTriggerLinkToUnfireWorkflow()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Dummy Workflow";
			template.P0_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			template.P0_IsUniversal = true;
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var universalTrigger = template.TemplateTriggers.AddNew() as ITemplateTrigger;
			universalTrigger.Description = "Universal Trigger";
			universalTrigger.TriggerEventCode = AutoEvents.CustomisableEvent00Code;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			Factory.Save();

			var log = dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);

			var query = new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, dummy.PK);
			var triggerLink = Factory.Load<ProcessJobTriggerLink>(query).SingleOrDefault(t => t.TemplateTrigger.P9T_SE_NKTriggerEvent == AutoEvents.CustomisableEvent00Code);
			AssertNotNull("Precondition: Universal Trigger should have fired ", triggerLink);

			dummy.WorkflowItems.RemoveAndDeleteAll();
			log.Delete();

			triggerLink = Factory.Load<ProcessJobTriggerLink>(query).SingleOrDefault(t => t.TemplateTrigger.P9T_SE_NKTriggerEvent == AutoEvents.CustomisableEvent00Code);
			AssertNull("Shouldn't recreate trigger link for a deleted log after the link is deleted", triggerLink);
		}

		#endregion

		#region Concurrency

		public void TestCreateConcurrent()
		{
			var template = CreateTemplate(Factory, isUniversal: true, processType: "DUM");
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);
			var triggerCountdown = trigger.TriggerConditions.TriggerFiredCountdown;
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			Factory.Save();

			var j1 = Factory.CreateNewFactory().Load<DummyWithWorkflow>(job.PK);
			AssertEquals(1, j1.WorkflowItems.TriggersIncludingRelated.Count);
			j1.Logs.AddNew(AutoEvents.CustomisableEvent00);
			j1.Logs.AddNew(AutoEvents.CustomisableEvent00);
			var persistedLink = j1.Factory.LoadTop1<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, j1.PK));
			AssertNotNull(persistedLink);
			AssertEquals("Trigger fired twice", triggerCountdown - 2, persistedLink.P9L_TriggerFiredCountdown);

			var j2 = Factory.CreateNewFactory().Load<DummyWithWorkflow>(job.PK);
			AssertEquals(1, j2.WorkflowItems.TriggersIncludingRelated.Count);
			j2.Logs.AddNew(AutoEvents.CustomisableEvent00);
			var deletedLink = j2.Factory.LoadTop1<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, j2.PK));
			AssertNotNull(deletedLink);
			AssertEquals("Trigger fired once ", triggerCountdown - 1, deletedLink.P9L_TriggerFiredCountdown);
			var wte = deletedLink.Logs.AddNew(AutoEvents.WorkflowTriggerEvent);

			j1.Factory.Save();
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => j2.Factory.Save(), () => { }, reportErrorsOnly: true);
			AssertEquals(true, deletedLink.IsDeleted);
			AssertEquals(true, wte.IsDeleted);

			AssertEquals("Here we should be accessing the persisted row, not the deleted one", persistedLink.P9L_TriggerFiredCountdown, (j2.WorkflowItems.TriggersIncludingRelated.First() as ProcessTask).TriggerConditions.TriggerFiredCountdown);
		}

		public void TestDeletingProcessJobTriggerLink()
		{
			var template = CreateTemplate(Factory, isUniversal: true, processType: "DUM");
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);
			var triggerCountdown = trigger.TriggerConditions.TriggerFiredCountdown;
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			Factory.Save();

			var j1 = Factory.CreateNewFactory().Load<DummyWithWorkflow>(job.PK);
			AssertEquals(1, j1.WorkflowItems.TriggersIncludingRelated.Count);
			j1.Logs.AddNew(AutoEvents.CustomisableEvent00);

			var j2 = Factory.CreateNewFactory().Load<DummyWithWorkflow>(job.PK);
			AssertEquals(1, j2.WorkflowItems.TriggersIncludingRelated.Count);
			j2.Logs.AddNew(AutoEvents.CustomisableEvent00);
			var deletedLink = j2.Factory.LoadTop1<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, j2.PK));
			AssertNotNull(deletedLink);
			AssertEquals("Trigger fired once ", triggerCountdown - 1, deletedLink.P9L_TriggerFiredCountdown);
			var wte = deletedLink.Logs.AddNew(AutoEvents.WorkflowTriggerEvent);

			j1.Factory.Save();
			deletedLink.SetOnTriggerLinkSaveHookForTest(triggerLink => deletedLink.Delete());
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() => j2.Factory.Save(), () => { }, reportErrorsOnly: true);
			AssertEquals(true, deletedLink.IsDeleted);
			AssertEquals(true, wte.IsDeleted);
		}

		#endregion

		#region Cancelling

		[TestDate(2017, 07, 01, 11, 30, 9)]
		public void TestCancellingUniversalTriggerEvent_FallbackTimeEmpty()
		{
			var template = CreateTemplate(Factory, processType: WorkflowDescriptors.DummyWorkflowDescriptorCode, isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerFiredCountdown = 2;

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			Factory.Save();

			var ghostTrigger = dummy.WorkflowItems.TriggersIncludingRelated[0];

			var log1 = dummy.Logs.AddNew(AutoEvents.CustomisableEvent00, ZDateTime.Now.AddHours(-2).ToOffset());

			var jobTrigger = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, dummy.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals("Trigger event time should be log1 event time", log1.SL_EventTime, ghostTrigger.P9_ActualDate);
				AssertEquals("Trigger countdown should decrease by one", (short)1, ghostTrigger.TriggerConditions.TriggerFiredCountdown);
				AssertEquals("Trigger should have 1 WTE log", 1, jobTrigger.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)).Length);

				log1.Cancel();

				AssertEquals("Trigger event time should be empty", true, ghostTrigger.P9_ActualDate.IsEmpty);
				AssertEquals("Trigger countdown should increase by one", (short)2, ghostTrigger.TriggerConditions.TriggerFiredCountdown);
				AssertEquals("Trigger should have no WTE logs", 0, jobTrigger.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)).Length);
			});
		}

		[TestDate(2017, 07, 01, 11, 30, 9)]
		public void TestCancellingUniversalTriggerEvent_FallbackTimeFilled()
		{
			var template = CreateTemplate(Factory, processType: WorkflowDescriptors.DummyWorkflowDescriptorCode, isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);
			trigger.TriggerConditions.TriggerFiredCountdown = 2;

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			Factory.Save();

			var ghostTrigger = dummy.WorkflowItems.TriggersIncludingRelated[0];

			var log1 = dummy.Logs.AddNew(AutoEvents.CustomisableEvent00, ZDateTime.Now.AddHours(-2).ToOffset());
			var log2 = dummy.Logs.AddNew(AutoEvents.CustomisableEvent00, ZDateTime.Now.AddHours(-1).ToOffset());

			var jobTrigger = Factory.Load<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, dummy.PK)).Single();
			CombineAssertions(() =>
			{
				AssertEquals("Trigger event time should be log2 event time",log2.SL_EventTime, ghostTrigger.P9_ActualDate);
				AssertEquals("Trigger countdown should have decreased by 2", (short)0, ghostTrigger.TriggerConditions.TriggerFiredCountdown);
				AssertEquals("Trigger should have 2 WTE logs", 2, jobTrigger.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)).Length);

				log2.Cancel();

				AssertEquals("Trigger event time should be log 1 event time",log1.SL_EventTime, ghostTrigger.P9_ActualDate);
				AssertEquals("Trigger countdown should have increased by 1", (short)1, ghostTrigger.TriggerConditions.TriggerFiredCountdown);
				AssertEquals("Trigger should have 1 WTE log", 1, jobTrigger.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)).Length);
			});
		}

		#endregion

		#region IBaseTrigger Members

		[ExpectNoExceptions]
		public void TestSetEstimateTime_DoNothing()
		{
			var trigger = (IBaseTrigger)Factory.New<ProcessJobTriggerLink>();
			trigger.SetEstimateTime(null, ZDateTimeOffset.Empty);
		}

		#endregion

		#region Fetch Hints

		public void TestEnsureGhostedTriggersPresent_DbHits()
		{
			Registry.Business.WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger1 = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			var trigger2 = CreateTrigger(template, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);
			var trigger3 = CreateTrigger(template, AutoEvents.StatusChangeCode);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedJob = newFactory.Load<DummyWithWorkflow>(job.PK);

			loadedJob.ApplyWorkflowTemplates();

			AssertEquals(3, loadedJob.WorkflowItems.TriggersIncludingRelated.Count);

			AssertDbHits(new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
				{ ProcessJobTriggerLinkSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTemplateTriggerSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		#endregion

		public void TestRootsWithLogCopy()
		{
			var template = CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.CustomisableEvent00Code);
			AssertEquals(trigger.TriggerActions.Count, 0);
			//Add IFC trigger action
			var notification = trigger.TriggerActions.AddNew();

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			notification.PQ_FieldName = "<Job.JH_Description>";
			notification.PQ_FieldValue = "CAN";

			Factory.Save();

			var job = Factory.New<IForwardingShipment>();
			job.JS_UniqueConsignRef = "1234";

			Factory.Save();

			var workflowProvider = (IWorkflowProvider)job;
			AssertEquals(1, workflowProvider.WorkflowItems.TriggersIncludingRelated.Count);
			var trigger1 = workflowProvider.WorkflowItems.Triggers[0];

			((IStmALogParent)job).Logs.AddNew(AutoEvents.CustomisableEvent00);

			AssertEquals(true, trigger1.HasProcessJobTriggerLink);
			var triggerLink = Factory.LoadTop1<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, job.PK));
			AssertNotNull(triggerLink);

			// create a StmALog here
			var log = Factory.NewWithValidTestData<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = "Z00";
			}

			var roots = trigger.GetRoots((BusinessObject)job);
			foreach (var root in roots)
			{
				AssertNotNull(root);
			}

			var logCopy = ((IStmALog)log).WeakCopy();
			AssertNoExceptionThrown(() =>
			{
				((IBaseTrigger)triggerLink).SetEventTime(logCopy, (IBusiness)job, ZDateTimeOffset.Now);
			});
		}
	}

	[TestedType(typeof(ProcessJobTriggerLink))]
	class ProcessJobTriggerLinkBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BusinessObject result;
			var businessObjectTestDataHelper = new ProcessJobTriggerLinkBusinessObjectTestDataHelper();
			try
			{
				result = businessObjectTestDataHelper.NewWithValidTestData(factory,
					 GetExpectedBusinessObjectType(),
					 TestBusinessObjectKind.MinimumRequiredToSave | TestBusinessObjectKind.PopulateDependentCollections);
			}
			catch
			{
				result = factory.New(GetExpectedBusinessObjectType());
				result.FillWithValidTestData();
			}
			return result;
		}
	}

	class ProcessJobTriggerLinkBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
	{
		protected override void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
		{
			if (property.Name != AutoProcessJobTriggerLink.Schema.P9L_ParentTableCode)
			{
				base.PopulateUniqueString(property, propertyPath, maxLength);
			}
			else
			{
				property.Value = new ZString("Z0");
			}
		}
	}
}
