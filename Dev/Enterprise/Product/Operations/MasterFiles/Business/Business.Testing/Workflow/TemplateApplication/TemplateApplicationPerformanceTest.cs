using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TemplateApplicationPerformanceTest : TemplateApplicationTestCase
	{
		#region Builders 

		ProcessTaskTemplate MakeBigTemplate(string templateCode = null)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = templateCode ?? DummyWorkflowDescriptor.Instance.Code;

			var t1 = MakeTask(template, udfCondition: "\"1\"==\"1\"");
			var t2 = MakeTask(template);
			var t3 = MakeTask(template);

			var m1 = MakeMilestone(template, udfCondition: "\"1\"==\"1\"");
			var m2 = MakeMilestone(template);
			m2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			m2.TriggerConditions.TriggerConditionValue = "2";
			var m3 = MakeMilestone(template);
			m3.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			m3.TriggerConditions.TriggerConditionValue = "3";
			m3.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			m3.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\" == \"XXX\"";
			MakeNotification(m1);
			MakeNotification(m2);
			MakeNotification(m3);

			var r1 = MakeTrigger(template, udfCondition: "\"1\"==\"1\"");
			var r2 = MakeTrigger(template);
			r2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			r2.TriggerConditions.TriggerConditionValue = "2";
			var r3 = MakeTrigger(template);
			r3.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			r3.TriggerConditions.TriggerConditionValue = "3";
			r3.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			r3.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\" == \"XXX\"";
			MakeNotification(r1);
			MakeNotification(r2);
			MakeNotification(r3);

			return template;
		}

		#endregion

		#region No StmNotes

		public void TestStmNotes_Blank()
		{
			var template = MakeTemplate(true);
			var task = MakeTask(template, udfCondition: "");
			var trigger = MakeTrigger(template, udfCondition: "");
			var milestone = MakeTrigger(template, udfCondition: "");

			Factory.Save();
			AssertEquals(0, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, ProcessTasksSchema.Constants.TableName) { FetchOnlyFromLocalCache = true }).Length);

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(0, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, ProcessTasksSchema.Constants.TableName) { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestStmNotes_NotBlank()
		{
			var template = MakeTemplate(true);
			var task = MakeTask(template, udfCondition: "\"1\"==\"1\"");
			var trigger = MakeTrigger(template, udfCondition: "\"1\"==\"1\"");
			var milestone = MakeTrigger(template, udfCondition: "\"1\"==\"1\"");

			Factory.Save();
			AssertEquals(3, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, ProcessTasksSchema.Constants.TableName) { FetchOnlyFromLocalCache = true }).Length);

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Clone should not add notes.", 3, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_Table, ProcessTasksSchema.Constants.TableName) { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestCleanUpOldStmNotes_BecauseHashingIsSoMuchCooler()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = MakeTrigger(dummy);
			trigger.TemplateConditions.TemplateCondition2 = "UDF";
			trigger.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			Factory.Save();

			var note = new HiddenUdfConditionNote(trigger);
			Assert(note.HasNote);
			trigger.P9_Condition2ValueHash = "";
			AssertNotEquals("", trigger.P9_Condition2ValueHash);

			note = new HiddenUdfConditionNote(trigger);
			Assert("looking at the hash deleted the old note", !note.HasNote);
		}

		public void TestCleanUpOldStmNotes_ButNotOnTemplates()
		{
			var template = MakeTemplate(true);
			var trigger = MakeTrigger(template);
			trigger.TemplateConditions.TemplateCondition2 = "UDF";
			trigger.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			Factory.Save();

			var note = new HiddenUdfConditionNote(trigger);
			Assert(note.HasNote);
			AssertNotNull(trigger.P9_Condition2ValueHash);

			note = new HiddenUdfConditionNote(trigger);
			Assert("Don't just delete my note.", note.HasNote);
		}

		class HiddenUdfConditionNote : HiddenTextNote
		{
			public HiddenUdfConditionNote(IStmNoteParent parent) : base(parent) { }
			protected override ZString Description => "User Defined Condition";
		}

		#endregion

		#region Avoid unnecessary updates

		public void TestP9_IsValid_EagerlyEvaluate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			var templateTask = MakeTask(template);
			templateTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			templateTask.RunPreSaveValidation();
			Factory.Save();
			var templateValidationInternals = (ILightValidationInternals)templateTask;
			AssertEquals(true, templateValidationInternals.IsValid);

			var job = Factory.New<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();
			AssertEquals("Precondition: The tempplate applied", 1, job.WorkflowItems.Count);
			var task = (ILightValidationInternals)job.WorkflowItems[0];
			AssertEquals(true, task.IsValid);

			Factory.Save();
			AssertEquals(true, task.IsValid);
		}

		#endregion

		#region DB Hits

		[TestDateIncremental(minutes: 1)]
		public void TestDbHitsOfMultipleTemplateApplicationsOnSave_WithNullAdditionalJobs()
		{
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 4 },// 1 per template + 1 for all 3 jobs
				{ ProcessTaskNotificationSchema.Constants.TableName, 3 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 3 },
				{ StmNoteSchema.Constants.TableName, 0 },
				{ JobDeclarationSchema.Constants.TableName, 6 }, // Though having used CachedProperty to cache GetAdditionalJobs, the value will be recacluated when any template/task matched and added to shipment then Facotory version cheanged and the reCaculation invoked.
			};

			AssertDbHitsOfMultipleTemplateApplicationsOnSave(expectedHits, false);
		}

		[TestDateIncremental(minutes: 1)]
		public void TestDbHitsOfMultipleTemplateApplicationsOnSave_WithNotNullAdditionalJobs()
		{
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 4 },// 1 per template + 1 for all 3 jobs
				{ ProcessTaskNotificationSchema.Constants.TableName, 3 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 3 },
				{ StmNoteSchema.Constants.TableName, 0 },
			};

			AssertDbHitsOfMultipleTemplateApplicationsOnSave(expectedHits, true); // When the JobDeclaration attached the shipment is not null, it will be loaded from DB at the first time and then from Factory since then
		}

		void AssertDbHitsOfMultipleTemplateApplicationsOnSave(Dictionary<string, int> expectedHits, bool hasAdditionalJobs)
		{
			MakeBigTemplate(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			MakeBigTemplate(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			MakeBigTemplate(WorkflowDescriptors.GlbStaffDescriptorCode);
			Factory.Save();

			var shipment = Factory.New<IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();

			if (hasAdditionalJobs)
			{
				var declaration1 = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration1.JE_JS = shipment.PK;
			}
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			// Precondition: Template application actually worked.
			AssertNotEquals(0, ((IWorkflowProvider)shipment).WorkflowItems.Count);
			AssertNotEquals(0, org.WorkflowItems.Count);
			AssertNotEquals(0, staff.WorkflowItems.Count);

			var newFactory = new BusinessObjectFactory();
			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				shipment = newFactory.Load<IForwardingShipment>(shipment.PK);
				org = newFactory.Load<OrgHeader>(org.PK);
				staff = newFactory.Load<GlbStaff>(staff.PK);

				// Make all HasChanges to ensure template applies on save
				((BusinessObject)shipment).HasChanges = true;
				org.HasChanges = true;
				staff.HasChanges = true;

				newFactory.Save();
			}
		}

		public void TestApplyTemplateHits_HitDBOncePerWorkflowType()
		{
			ProcessTaskTemplate.Loader.CacheDurationMinutes_ForTest.Value = 20;

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template1.P0_SubType1 = "AAA";
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template2.P0_SubType1 = "BBB";

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var oneDBHit = new Dictionary<string, int> { { ProcessTaskTemplateSchema.Constants.TableName, 1 } };
			var noDBHits = new Dictionary<string, int> { { ProcessTaskTemplateSchema.Constants.TableName, 0 } };

			using (AssertDbHitsForAllFactories(oneDBHit))
			{
				var dummy = newFactory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Code = "XXX";
				dummy.SubType1 = "AAA";
				var matchingTemplates = new ProcessTaskTemplate.Loader(newFactory).FindMatches(dummy);
				AssertEquals(1, matchingTemplates.Length);
			}

			using (AssertDbHitsForAllFactories(noDBHits))
			{
				var dummy = newFactory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Code = "XXX";
				dummy.SubType1 = "BBB";
				var matchingTemplates = new ProcessTaskTemplate.Loader(newFactory).FindMatches(dummy);
				AssertEquals(1, matchingTemplates.Length);
			}

			using (AssertDbHitsForAllFactories(oneDBHit, ignoreUnspecified: true))
			{
				var org = newFactory.NewWithValidTestData<OrgHeader>();
				var matchingTemplates = new ProcessTaskTemplate.Loader(newFactory).FindMatches(org);
				AssertEquals(0, matchingTemplates.Length);
			}
		}

		[TestDateIncremental(minutes: 1)]
		public void TestApplyTemplateHits_NewBizo()
		{
			MakeBigTemplate();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var expectedHits = new Dictionary<string, int>
			{
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 0 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory))
			{
				var dummy = newFactory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.Z0_Code = "XXX";
				dummy.ApplyWorkflowTemplates();
				AssertEquals(9, dummy.WorkflowItems.Count);
			}
		}

		[TestDateIncremental(minutes: 1)]
		public void TestApplyTemplateDBHits_SavedBizo_WithoutAutoConcurrencyHandling()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.ServiceTasksOnly);
			AssertApplyTemplateDBHits_SavedBizo(shouldReloadProcessTasks: true);
		}

		[TestDateIncremental(minutes: 1)]
		public void TestApplyTemplateDBHits_SavedBizo_NoConcurrencyCheck()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.Off);
			AssertApplyTemplateDBHits_SavedBizo(shouldReloadProcessTasks: false);
		}

		[TestDateIncremental(minutes: 1)]
		public void TestApplyTemplateDBHits_SavedBizo_WithAutoConcurrencyHandling()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.UserInterfaceAndServiceTasks);
			AssertApplyTemplateDBHits_SavedBizo(shouldReloadProcessTasks: false);
		}

		[TestDateIncremental(minutes: 1)]
		public void TestApplyTemplateDBHits_SavedBizo_WithAutoConcurrencyHandlingForServiceTaskOnly()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.ServiceTasksOnly);
			using (BusinessObjectFactory.NotifyIsSavingTogether())
			{
				AssertApplyTemplateDBHits_SavedBizo(shouldReloadProcessTasks: false);
			}
		}

		void AssertApplyTemplateDBHits_SavedBizo(bool shouldReloadProcessTasks)
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			MakeBigTemplate();
			var initdummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			initdummy.Z0_Code = "XXX";
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var expectedHits = new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, shouldReloadProcessTasks ? 3 : 2 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmALogSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 0 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory))
			{
				var dummy = newFactory.Load<DummyWithWorkflow>(initdummy.PK);
				dummy.ApplyWorkflowTemplates();
				AssertEquals(9, dummy.WorkflowItems.Count);
			}
		}

		[TestDateIncremental(minutes: 1)]
		public void TestApplyTemplateHits_SavedBizo_SecondApply()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			MakeBigTemplate();
			Factory.Save();
			var initdummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			initdummy.Z0_Code = "XXX";
			initdummy.ApplyWorkflowTemplates();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var expectedHits = new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 0 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory))
			{
				var dummy = newFactory.Load<DummyWithWorkflow>(initdummy.PK);
				AssertEquals(9, dummy.WorkflowItems.Count);
				dummy.ApplyWorkflowTemplates();
			}
		}

		[TestDateIncremental(minutes: 1)]
		public void TestApplyTemplateHits_SavedBizo_SecondApply_Conditions()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			MakeBigTemplate();
			Factory.Save();
			var initdummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			initdummy.Z0_Code = "XXX";
			initdummy.ApplyWorkflowTemplates();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var expectedHits = new Dictionary<string, int>
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmNoteSchema.Constants.TableName, 0 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory))
			{
				var dummy = newFactory.Load<DummyWithWorkflow>(initdummy.PK);
				AssertEquals(9, dummy.WorkflowItems.Count);
				dummy.ApplyWorkflowTemplates();
			}
		}

		public void FAT_TestApplyTemplatePerformance()
		{
			var allowed = Env.Security.WorkflowTaskTemplatesEdit.IsAllowed;
			try
			{
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
				MakeBigTemplate();
				Factory.Save();

				for (int i = 0; i < 1000; i++)
				{
					var factory = Factory.CreateNewFactory();
					var initdummy = factory.NewWithValidTestData<DummyWithWorkflow>();
					initdummy.Z0_Code = "XXX";
					initdummy.ApplyWorkflowTemplates();
					factory.Save();
				}
			}
			finally
			{
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = allowed;
			}
		}

		#endregion
	}
}
