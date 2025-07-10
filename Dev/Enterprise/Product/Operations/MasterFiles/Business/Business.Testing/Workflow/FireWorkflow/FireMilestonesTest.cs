using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class FireMilestonesTest : TemplateApplicationTestCase
	{
		#region Existing Events

		public void TestUpdateExistingEventBeforeSave()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			var log = dummy.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now));
			dummy.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddDays(-1)), log);
			AssertLogCount(1, Events.CustomisableEvent00, dummy);
			AssertMilestoneFireCount(1, milestone);
		}

		public void TestUpdateExistingEventAfterSave()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			var log = dummy.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now));
			Factory.Save();
			dummy.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddDays(-1)), log);
			AssertMilestoneFireCount(1, milestone);
		}

		public void TestWithdrawEventBeforeSave()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			var log = dummy.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now));
			AssertLogCount(1, Events.CustomisableEvent00, dummy);
			AssertMilestoneFireCount(1, milestone);
			log.Cancel();
			AssertLogCount(0, Events.CustomisableEvent00, dummy);
			AssertMilestoneFireCount(0, milestone);
		}

		public void TestWithdrawEventAfterSave()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			var log = dummy.Logs.CreateRecreateOrUpdateEventLog(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now));
			AssertLogCount(1, Events.CustomisableEvent00, dummy);
			AssertMilestoneFireCount(1, milestone);
			Factory.Save();
			log.Cancel();
			AssertMilestoneFireCount(1, milestone);
		}

		public void TestRaiseMultipleEventsBeforeMilestoneIsSavedTheFirstTime()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Arc Lightening"));
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Sparrows"));
			AssertLogCount(2, Events.CustomisableEvent00, dummy);
			AssertMilestoneFireCount(1, milestone);
		}

		public void TestRaiseMultipleEventsAfterMilestoneIsSavedTheFirstTime()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			Factory.Save();
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Japan"));
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Nipon"));
			AssertLogCount(2, Events.CustomisableEvent00, dummy);
			AssertMilestoneFireCount(1, milestone);
		}

		public void TestRaiseMultipleEventsWithSavesBetween()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			Factory.Save();
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Skooba"));
			Factory.Save();
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Jooba"));
			AssertLogCount(2, Events.CustomisableEvent00, dummy);
			AssertMilestoneFireCount(1, milestone);
		}

		public void TestRedactAllEventsDuringSave()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			var log1 = dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now));
			var log2 = dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddDays(-1)));
			var log3 = dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddDays(-2)));

			AssertMilestoneFireCount(1, milestone);
			using (Factory.OnSaveDelayer.TemporaryChangeToDelayStrategy())
			{
				log1.Delete();
				log2.Delete();
				log3.Delete();
			}
			AssertMilestoneFireCount(0, milestone);
		}

		public void TestRedactAllEvents()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			var log1 = dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now));
			var log2 = dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddDays(-1)));
			var log3 = dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddDays(-2)));

			AssertMilestoneFireCount(1, milestone);
			log1.Delete();
			log2.Delete();
			log3.Delete();
			AssertMilestoneFireCount(0, milestone);
		}

		public void TestRedactAllEvents_TheOtherOrder()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = MakeMilestone(dummy, Events.CustomisableEvent00Code);
			MakeNotification(milestone);
			var log1 = dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now));
			var log2 = dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddDays(-1)));
			var log3 = dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddDays(-2)));

			AssertMilestoneFireCount(1, milestone);
			log3.Delete();
			log2.Delete();
			log1.Delete();
			AssertMilestoneFireCount(0, milestone);
		}

		#endregion

		#region Duplicate Milestones

		public void TestIdenticalMilestonesBothFire_ExistingEventSaved()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var templateMilestone1 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m1");
			MakeNotification(templateMilestone1);
			var templateMilestone2 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m2");
			MakeNotification(templateMilestone2);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Skooba"));
			Factory.Save();
			dummy.ApplyWorkflowTemplates();

			var milestone1 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m1");
			var milestone2 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m2");
			AssertMilestoneFireCount(1, milestone1);
			AssertMilestoneFireCount(1, milestone2);
		}

		public void TestIdenticalMilestonesBothFire_ExistingEventNotSaved()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var templateMilestone1 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m1");
			MakeNotification(templateMilestone1);
			var templateMilestone2 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m2");
			MakeNotification(templateMilestone2);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Skooba"));
			dummy.ApplyWorkflowTemplates();

			var milestone1 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m1");
			var milestone2 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m2");
			AssertMilestoneFireCount(1, milestone1);
			AssertMilestoneFireCount(1, milestone2);
		}

		public void TestIdenticalMilestonesBothFire_EventAfterSave()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var templateMilestone1 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m1");
			MakeNotification(templateMilestone1);
			var templateMilestone2 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m2");
			MakeNotification(templateMilestone2);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			dummy.ApplyWorkflowTemplates();
			Factory.Save();
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Skooba"));

			var milestone1 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m1");
			var milestone2 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m2");
			AssertMilestoneFireCount(1, milestone1);
			AssertMilestoneFireCount(1, milestone2);
		}

		public void TestIdenticalMilestonesBothFire_EventBeforeSave()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var templateMilestone1 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m1");
			MakeNotification(templateMilestone1);
			var templateMilestone2 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m2");
			MakeNotification(templateMilestone2);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			dummy.ApplyWorkflowTemplates();
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Skooba"));

			var milestone1 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m1");
			var milestone2 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m2");
			AssertMilestoneFireCount(1, milestone1);
			AssertMilestoneFireCount(1, milestone2);
		}

		public void TestIdenticalMilestonesBothFire_DisparateApplications_EventBetween_NoSave()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var templateMilestone1 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m1");
			MakeNotification(templateMilestone1);
			var templateMilestone2 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m2", udfCondition: @"""<Z0_Code>""==""YUM""");
			MakeNotification(templateMilestone2);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			dummy.ApplyWorkflowTemplates();
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Skooba"));
			dummy.Z0_Code = "YUM";
			dummy.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { template }, true));

			var milestone1 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m1");
			var milestone2 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m2");
			AssertMilestoneFireCount(1, milestone1);
			AssertMilestoneFireCount(1, milestone2);
		}

		public void TestIdenticalMilestonesBothFire_DisparateApplications_EventBetween_SaveBeforeEvent()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var templateMilestone1 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m1");
			MakeNotification(templateMilestone1);
			var templateMilestone2 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m2", udfCondition: @"""<Z0_Code>""==""YUM""");
			MakeNotification(templateMilestone2);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			dummy.ApplyWorkflowTemplates();
			Factory.Save();
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Skooba"));
			dummy.Z0_Code = "YUM";
			dummy.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { template }, true));

			var milestone1 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m1");
			var milestone2 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m2");
			AssertMilestoneFireCount(1, milestone1);
			AssertMilestoneFireCount(1, milestone2);
		}

		public void TestIdenticalMilestonesBothFire_DisparateApplications_EventBetween_SaveAfterEventBeforeApplication()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var templateMilestone1 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m1");
			MakeNotification(templateMilestone1);
			var templateMilestone2 = MakeMilestone(template, Events.CustomisableEvent00Code, description: "m2", udfCondition: @"""<Z0_Code>""==""YUM""");
			MakeNotification(templateMilestone2);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			dummy.ApplyWorkflowTemplates();
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Skooba"));
			Factory.Save();
			dummy.Z0_Code = "YUM";
			dummy.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { template }, true));

			var milestone1 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m1");
			var milestone2 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m2");
			AssertMilestoneFireCount(1, milestone1);
			AssertMilestoneFireCount(1, milestone2);
		}

		#endregion

		#region IFC

		public void TestIFCWorksIfMultipleTemplatesApply_MilestoneMerge()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUM";
			template1.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			var templateMilestone1 = MakeMilestone(template1, Events.CustomisableEvent00Code, description: "m1");
			var notification = templateMilestone1.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			notification.PQ_FieldName = "<Z0_Code>";
			notification.PQ_FieldValue = "KEH";
			MakeNotification(templateMilestone1);

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "DUM";
			template2.P0_SubType1 = "JAP";
			template2.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			var templateMilestone2 = MakeMilestone(template2, Events.CustomisableEvent00Code, description: "m1"); // Should merge.

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			dummy.Logs.AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now, reference: "Skooba"));
			Factory.Save();
			dummy.SubType1 = "JAP";
			dummy.ApplyWorkflowTemplates();

			var milestone1 = dummy.WorkflowItems.Cast<ProcessTask>().Single(m => m.P9_Description == "m1");
			AssertMilestoneFireCount(1, milestone1);
			AssertEquals("The immediate field change should have worked even when template merging happens.", "KEH", dummy.Z0_Code);
		}

		#endregion

		#region Status

		public void TestMilestoneStatusNotUpdatedUntilSave()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = MakeMilestone(job, Events.CustomisableEvent01Code);
			Factory.Save();
			AssertEquals("NXT", milestone.P9_Status);
			job.Logs.AddNew(Events.CustomisableEvent01);
			AssertEquals("NXT", milestone.P9_Status);
			Factory.Save();
			AssertEquals("LST", milestone.P9_Status);
		}

		#endregion
	}
}
