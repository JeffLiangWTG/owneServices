using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	#region ProcessTaskTemplateLoaderTest

	[TestedType(typeof(ProcessTaskTemplate.Loader))]
	sealed class ProcessTaskTemplateLoaderTest : LoaderTestCase
	{
		#region Find

		public void TestFindTemplateForScreenLayout_Fallback()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_SubType1 = dummy.SubType1 = "PORK";
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = template2.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			Factory.Save();

			AssertEquals(template1, ((IProcessTaskTemplateLoader)new ProcessTaskTemplate.Loader(Factory)).FindTemplateForScreenLayout(dummy));
			template1.P0_IsScreenLayoutFallback = true;
			AssertEquals(template2, ((IProcessTaskTemplateLoader)new ProcessTaskTemplate.Loader(Factory)).FindTemplateForScreenLayout(dummy));
		}

		public void TestFindBestProcessTaskTemplateMatch_FallsBackToDefaultWhenNoMatchesAvailable_HostUpdatesTemplate()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();

			ProcessTaskTemplate match = ((IProcessTaskTemplateLoader)new ProcessTaskTemplate.Loader(Factory)).FindTemplateForScreenLayout(dummy) as ProcessTaskTemplate;
			AssertNotNull(match);
			ProcessTaskTemplate alsoMatch = ((IProcessTaskTemplateLoader)new ProcessTaskTemplate.Loader(Factory)).FindTemplateForScreenLayout(dummy) as ProcessTaskTemplate;
			AssertEquals(match, alsoMatch);

			DummyWithWorkflowProcessTaskTemplateUpdatable dummyProcessTaskTemplateUpdatable = Factory.New<DummyWithWorkflowProcessTaskTemplateUpdatable>();

			bool updateCalled = false;
			dummyProcessTaskTemplateUpdatable.ProcessTaskTemplateUpdateImplementation = (taskTemplate) =>
			{
				AssertNotNull(taskTemplate);
				updateCalled = true;
			};

			match = ((IProcessTaskTemplateLoader)new ProcessTaskTemplate.Loader(Factory)).FindTemplateForScreenLayout(dummyProcessTaskTemplateUpdatable) as ProcessTaskTemplate;
			AssertNotNull(match);
			Assert(updateCalled);
		}

		public void TestFindBestProcessTaskTemplateMatch_FallsBackToDefaultWhenNoMatchesAvailable()
		{
			ProcessTaskTemplate inactiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			inactiveTemplate.P0_ProcessType = "DUM";
			inactiveTemplate.P0_IsActive = false;
			inactiveTemplate.WorkflowItems.AddNew();
			Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Tasks not created as default settings do not have them", 0, Dummy.WorkflowItems.Count);

			ProcessTaskTemplate match = ((IProcessTaskTemplateLoader)new ProcessTaskTemplate.Loader(Factory)).FindTemplateForScreenLayout(Dummy) as ProcessTaskTemplate;
			AssertNotNull(match);
			AssertEquals("DUM", match.P0_ProcessType);
			Assert(!match.IsSavedByFactory);

			match.Factory.Save();

			AssertNull(new BusinessObjectFactory().Load<ProcessTaskTemplate>(match.PK));
			AssertNotNull(((IProcessTaskTemplateLoader)new ProcessTaskTemplate.Loader(Factory)).FindTemplateForScreenLayout(Dummy));
			AssertEquals(0, ((IProcessTaskTemplateLoader)new ProcessTaskTemplate.Loader(Factory)).FindMatches(Dummy).Matches.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_IgnoreInactiveTemplates()
		{
			ProcessTaskTemplate activeTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate.P0_ProcessType = "DUM";
			activeTemplate.WorkflowItems.AddNew();
			activeTemplate.WorkflowItems.AddNew();

			ProcessTaskTemplate inactiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			inactiveTemplate.P0_ProcessType = "DUM";
			inactiveTemplate.P0_IsActive = false;
			inactiveTemplate.WorkflowItems.AddNew();

			Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Tasks created using Active template", activeTemplate.WorkflowItems.Count, Dummy.WorkflowItems.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_IgnorePartialTemplates()
		{
			var inactiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			inactiveTemplate.P0_ProcessType = "DUM";
			inactiveTemplate.P0_IsActive = false;
			inactiveTemplate.WorkflowItems.AddNew();
			inactiveTemplate.WorkflowItems.AddNew();

			var partialTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			partialTemplate.P0_ProcessType = "DUM";
			partialTemplate.P0_IsActive = true;
			partialTemplate.P0_IsPartialTemplate = true;
			partialTemplate.WorkflowItems.AddNew();

			Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Tasks created using Active template", 0, Dummy.WorkflowItems.Count);
		}

		public void TestApplyTemplate_ShouldIgnoreUniversalTemplate()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);

			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Smelly Task";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_ContactName = "Villhelm";

			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Tasks.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_IgnoreSystemTemplateIfUserDefinedTemplateExists()
		{
			ProcessTaskTemplate userDefinedTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			userDefinedTemplate.P0_ProcessType = "DUM";
			userDefinedTemplate.P0_IsSystem = false;
			userDefinedTemplate.WorkflowItems.AddNew();
			userDefinedTemplate.WorkflowItems.AddNew();

			ProcessTaskTemplate systemTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			systemTemplate.P0_ProcessType = "DUM";
			systemTemplate.P0_IsSystem = true;
			systemTemplate.WorkflowItems.AddNew();

			Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Tasks created using the User defined template", userDefinedTemplate.WorkflowItems.Count, Dummy.WorkflowItems.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_DoNotCreateMultiple()
		{
			var globalTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			globalTemplate.P0_ProcessType = "DUM";
			globalTemplate.GlobalTemplate = true;
			globalTemplate.WorkflowItems.AddNew().P9_Description = "Global";

			Factory.Save();

			var companyTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			companyTemplate.P0_ProcessType = "DUM";
			companyTemplate.GlobalTemplate = false;
			companyTemplate.WorkflowItems.AddNew().P9_Description = "Company";

			Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("Tasks should be created using the company specific template", "Company", Dummy.WorkflowItems[0].P9_Description);
		}

		public void TestTaskShouldBeSharedOnGlobalTemplate()
		{
			var localTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			localTemplate.P0_ProcessType = "DUM";
			Assert("This template should not be global. And yet!", !localTemplate.GlobalTemplate);

			var localTask = localTemplate.WorkflowItems.Tasks.AddNew();
			var descriptor = DummyWorkflowDescriptor.Instance;
			descriptor.SetAreTasksCompanySpecific(true);
			Assert("The task created on the local template should not be Shared. And yet!", !localTask.P9_ShareTasksForAllCompanies);

			var globalTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			globalTemplate.P0_ProcessType = "DUM";
			globalTemplate.GlobalTemplate = true;

			var globalTask = globalTemplate.WorkflowItems.Tasks.AddNew();
			Assert("The task created on the global template should be automagically marked as Shared. And yet!", globalTask.P9_ShareTasksForAllCompanies);
		}

		public void TestTaskShouldNotBeSharedOnClonedGlobalTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.GlobalTemplate = true;
			var task = template.WorkflowItems.AddNew();
			task.P9_ShareTasksForAllCompanies = false;

			var newTemplate = (ProcessTaskTemplate)template.Clone();
			AssertEquals(1, newTemplate.WorkflowItems.Count);
			AssertEquals("An unshared task cloned on a global template should remain unshared", false, newTemplate.WorkflowItems[0].P9_ShareTasksForAllCompanies);
		}

		public void TestSharedFlagOnExistingTasksNotModified()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			var task1 = template.WorkflowItems.Tasks.AddNew();
			task1.P9_ShareTasksForAllCompanies = false;

			var task2 = template.WorkflowItems.Tasks.AddNew();
			task2.P9_ShareTasksForAllCompanies = true;

			template.GlobalTemplate = true;

			var descriptor = DummyWorkflowDescriptor.Instance;
			descriptor.SetAreTasksCompanySpecific(true);
			Assert("This task should still NOT be marked as Shared. And yet!", !task1.P9_ShareTasksForAllCompanies);
			Assert("This task should STILL be marked as Shared. And yet!", task2.P9_ShareTasksForAllCompanies);

			var task3 = template.WorkflowItems.Tasks.AddNew();

			Assert("This task should be marked as Shared. And yet!", task3.P9_ShareTasksForAllCompanies);
		}

		void SetupTemplateAndCreateItemsFromTemplate(ZString processTaskType, ZString fallbackMethod)
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUM";
			template1.P0_SubType1 = "AIR";

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "DUM";
			template2.P0_SubType1 = "AIR";
			template2.P0_LoadPortCountry = "AU";

			if (processTaskType == Core.Constants.Workflow.MilestoneType)
			{
				SetupTemplateWithMilestones(template1, template2, fallbackMethod);
			}
			else if (processTaskType == Core.Constants.Workflow.WorkflowTriggerType)
			{
				SetupTemplateWithTriggers(template1, template2, fallbackMethod);
			}
			else
			{
				SetupTemplateWithTasks(template1, template2, fallbackMethod);
			}

			Factory.Save();
			ExecuteCreateItemsFromTemplate(processTaskType);
		}

		void ExecuteCreateItemsFromTemplate(ZString processTaskType)
		{
			Dummy.SubType1 = "AIR";
			Dummy.LoadPort = "AUSYD";
			Dummy.WorkflowItems.RemoveAndDeleteAll();

			if (processTaskType == Core.Constants.Workflow.MilestoneType)
			{
				Dummy.WorkflowItems.Milestones.CreateItemsFromTemplate();
			}
			else if (processTaskType == Core.Constants.Workflow.WorkflowTriggerType)
			{
				Dummy.WorkflowItems.Triggers.CreateItemsFromTemplate();
			}
			else
			{
				Dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			}
		}

		void SetupTemplateWithMilestones(ProcessTaskTemplate template1, ProcessTaskTemplate template2, ZString fallbackMethod)
		{
			template1.P0_MilestoneFallbackMethod = fallbackMethod;
			var milestone1_1 = template1.WorkflowItems.Milestones.AddNew();
			milestone1_1.TriggerConditions.TriggerEventCode = "ATH";

			template2.P0_MilestoneFallbackMethod = fallbackMethod;
			if (fallbackMethod != FallbackTypeList.Codes.NeverFallback)
			{
				var milestone2_1 = template2.WorkflowItems.Milestones.AddNew();
				var milestone2_2 = template2.WorkflowItems.Milestones.AddNew();
				milestone2_1.TriggerConditions.TriggerEventCode = "AD1";
				milestone2_2.TriggerConditions.TriggerEventCode = "AD2";
			}
		}

		void SetupTemplateWithTriggers(ProcessTaskTemplate template1, ProcessTaskTemplate template2, ZString fallbackMethod)
		{
			template1.P0_TriggerFallbackMethod = fallbackMethod;
			var trigger1_1 = template1.WorkflowItems.Triggers.AddNew();
			trigger1_1.TriggerConditions.TriggerFieldName = "Field1";

			template2.P0_TriggerFallbackMethod = fallbackMethod;
			if (fallbackMethod != FallbackTypeList.Codes.NeverFallback)
			{
				var trigger2_1 = template2.WorkflowItems.Triggers.AddNew();
				var trigger2_2 = template2.WorkflowItems.Triggers.AddNew();
				trigger2_1.TriggerConditions.TriggerFieldName = "Field2";
				trigger2_2.TriggerConditions.TriggerFieldName = "Field3";
			}
		}

		void SetupTemplateWithTasks(ProcessTaskTemplate template1, ProcessTaskTemplate template2, ZString fallbackMethod)
		{
			template1.P0_TaskFallbackMethod = fallbackMethod;
			template1.WorkflowItems.Tasks.AddNew();

			template2.P0_TaskFallbackMethod = fallbackMethod;
			if (fallbackMethod != FallbackTypeList.Codes.NeverFallback)
			{
				template2.WorkflowItems.Tasks.AddNew();
				template2.WorkflowItems.Tasks.AddNew();
			}
		}

		public void TestFindBestProcessTaskTemplateMatch_Tasks_AlwaysFallback()
		{
			SetupTemplateAndCreateItemsFromTemplate("TSK", FallbackTypeList.Codes.AlwaysFallback);

			AssertEquals("Should fallback to the next specific template", 3, Dummy.WorkflowItems.Tasks.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_Tasks_EmptyFallback()
		{
			SetupTemplateAndCreateItemsFromTemplate("TSK", FallbackTypeList.Codes.EmptyFallback);

			AssertEquals("Should fallback to the next specific template", 2, Dummy.WorkflowItems.Tasks.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_Tasks_NeverFallback()
		{
			SetupTemplateAndCreateItemsFromTemplate("TSK", FallbackTypeList.Codes.NeverFallback);

			AssertEquals("Should not fallback to the next specific template", 0, Dummy.WorkflowItems.Tasks.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_Milestones_AlwaysFallback()
		{
			SetupTemplateAndCreateItemsFromTemplate(Core.Constants.Workflow.MilestoneType, FallbackTypeList.Codes.AlwaysFallback);

			AssertEquals("Should fallback to the next specific template", 3, Dummy.WorkflowItems.Milestones.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_Milestones_EmptyFallback()
		{
			SetupTemplateAndCreateItemsFromTemplate(Core.Constants.Workflow.MilestoneType, FallbackTypeList.Codes.EmptyFallback);

			AssertEquals("Should fallback to the next specific template", 2, Dummy.WorkflowItems.Milestones.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_Milestones_NeverFallback()
		{
			SetupTemplateAndCreateItemsFromTemplate(Core.Constants.Workflow.MilestoneType, FallbackTypeList.Codes.NeverFallback);

			AssertEquals("Should not fallback to the next specific template", 0, Dummy.WorkflowItems.Milestones.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_Triggers_AlwaysFallback()
		{
			SetupTemplateAndCreateItemsFromTemplate(Core.Constants.Workflow.WorkflowTriggerType, FallbackTypeList.Codes.AlwaysFallback);

			AssertEquals("Should fallback to the next specific template", 3, Dummy.WorkflowItems.Triggers.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_Triggers_EmptyFallback()
		{
			SetupTemplateAndCreateItemsFromTemplate(Core.Constants.Workflow.WorkflowTriggerType, FallbackTypeList.Codes.EmptyFallback);

			AssertEquals("Should fallback to the next specific template", 2, Dummy.WorkflowItems.Triggers.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_Triggers_NeverFallback()
		{
			SetupTemplateAndCreateItemsFromTemplate(Core.Constants.Workflow.WorkflowTriggerType, FallbackTypeList.Codes.NeverFallback);

			AssertEquals("Should not fallback to the next specific template", 0, Dummy.WorkflowItems.Triggers.Count);
		}

		public void TestFindBestProcessTaskTemplateMatch_NoDuplicateMilestone()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUM";
			template1.P0_SubType1 = "AIR";
			template1.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			var milestone1_1 = template1.WorkflowItems.Milestones.AddNew();
			milestone1_1.TriggerConditions.TriggerEventCode = "AD1";

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "DUM";
			template2.P0_SubType1 = "AIR";
			template2.P0_LoadPortCountry = "AU";
			template2.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			var milestone2_1 = template2.WorkflowItems.Milestones.AddNew();
			var milestone2_2 = template2.WorkflowItems.Milestones.AddNew();
			milestone2_1.TriggerConditions.TriggerEventCode = "AD1";
			milestone2_2.TriggerConditions.TriggerEventCode = "AD2";

			Factory.Save();

			Dummy.SubType1 = "AIR";
			Dummy.LoadPort = "AUSYD";
			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.Milestones.CreateItemsFromTemplate();

			AssertEquals("Should not have duplicate milestones", 2, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("AD1", Dummy.WorkflowItems.Milestones[0].P9_SE_NKMilestoneEvent);
			AssertEquals("AD2", Dummy.WorkflowItems.Milestones[1].P9_SE_NKMilestoneEvent);
		}

		public void TestTemplate_TriggerAndMilestonesAreIdentical()
		{
			var setupTrigger = new Action<ProcessTask>(t =>
			{
				t.P9_Description = "STOP THAT THAT IS SILLY";
				t.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
				var action = t.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = "NTF";
				action.PQ_TriggerParty = "EML";
				action.PQ_EmailText = "QUE QUE QUE QUE QUE";
				action.PQ_EmailAddr = "anton@morelike.banton";
			});

			var setupTask = new Action<ProcessTask>(t => { t.P9_Description = "The flask Task"; });

			ProcessTaskTemplate activeTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate1.P0_ProcessType = "DUM";
			activeTemplate1.P0_SubType1 = "AIR";

			setupTrigger(activeTemplate1.WorkflowItems.Triggers.AddNew());
			setupTrigger(activeTemplate1.WorkflowItems.Milestones.AddNew());
			setupTask(activeTemplate1.WorkflowItems.Tasks.AddNew());

			Factory.Save();

			Dummy.SubType1 = "AIR";
			Dummy.LoadPort = "AUSYD";
			Dummy.ApplyWorkflowTemplates();
			Dummy.ApplyWorkflowTemplates();
			Dummy.ApplyWorkflowTemplates();
			Dummy.ApplyWorkflowTemplates();
			AssertEquals(1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals(1, Dummy.WorkflowItems.Triggers.Count);
			AssertEquals(1, Dummy.WorkflowItems.Tasks.Count);

			Factory.Save();

			Dummy.ApplyWorkflowTemplates();
			AssertEquals(1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals(1, Dummy.WorkflowItems.Triggers.Count);
			AssertEquals(1, Dummy.WorkflowItems.Tasks.Count);
		}

		public void TestFindBestProcessTaskTemplateMatches()
		{
			ProcessTaskTemplate activeTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate1.P0_ProcessType = "DUM";
			activeTemplate1.P0_SubType1 = "AIR";

			ProcessTaskTemplate activeTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate2.P0_ProcessType = "DUM";
			activeTemplate2.P0_SubType1 = "AIR";
			activeTemplate2.P0_LoadPortCountry = "AU";

			ProcessTaskTemplate inactiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			inactiveTemplate.P0_ProcessType = "DUM";
			inactiveTemplate.P0_SubType1 = "AIR";
			inactiveTemplate.P0_LoadPortCountry = "AU";
			inactiveTemplate.P0_IsActive = false;

			Factory.Save();

			Dummy.SubType1 = "AIR";
			Dummy.LoadPort = "AUSYD";

			ProcessTaskTemplate[] templates = new ProcessTaskTemplate.Loader(Factory).FindMatches(Dummy);
			AssertEquals("Should found 2", 2, templates.Length);
			AssertEquals("Should be ordered correctly", activeTemplate2, templates[0]);
			AssertEquals("Should be ordered correctly", activeTemplate1, templates[1]);
		}

		public void TestFindBestProcessTaskTemplateMatches_NoDuplicates()
		{
			ProcessTaskTemplate activeTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate1.P0_ProcessType = "DUM";
			activeTemplate1.P0_SubType1 = "AIR";
			activeTemplate1.P0_SubType2 = "FOO";

			ProcessTaskTemplate activeTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate2.P0_ProcessType = "DUM";
			activeTemplate2.P0_SubType1 = "AIR";
			activeTemplate2.P0_SubType2 = "FOO";
			activeTemplate2.P0_LoadPortCountry = "AU";

			for (int i = 0; i < 100; i++)
			{
				ProcessTaskTemplate activeTemplate3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				activeTemplate3.P0_ProcessType = "DUM";
				activeTemplate3.P0_SubType1 = "SEA";
				activeTemplate3.P0_SubType2 = "FO" + i;
				activeTemplate3.P0_LoadPortCountry = "AU";
			}

			Factory.Save();

			Dummy.SubType1 = "air"; // Checking case sensitivity.
			Dummy.SubType1_Extra = "air";
			Dummy.Z0_Code = "FOO";
			Dummy.LoadPort = "AUSYD";

			ProcessTaskTemplate[] templates = new ProcessTaskTemplate.Loader(Factory).FindMatches(Dummy);
			AssertEquals("Should found 2", 2, templates.Length);
			AssertEquals("Should be ordered correctly", activeTemplate2, templates[0]);
			AssertEquals("Should be ordered correctly", activeTemplate1, templates[1]);
		}

		public void TestFindBestProcessTaskTemplateMatchesWithIgnoreCache()
		{
			ProcessTaskTemplate.Loader.CacheDurationMinutes_ForTest.Value = 20;
			ProcessTaskTemplate activeTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate1.P0_ProcessType = "DUM";
			activeTemplate1.P0_SubType1 = "AIR";

			ProcessTaskTemplate activeTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate2.P0_ProcessType = "DUM";
			activeTemplate2.P0_SubType1 = "AIR";
			activeTemplate2.P0_LoadPortCountry = "AU";

			ProcessTaskTemplate inactiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			inactiveTemplate.P0_ProcessType = "DUM";
			inactiveTemplate.P0_SubType1 = "AIR";
			inactiveTemplate.P0_LoadPortCountry = "AU";
			inactiveTemplate.P0_IsActive = false;

			Factory.Save();

			Dummy.SubType1 = "AIR";
			Dummy.LoadPort = "AUSYD";

			ProcessTaskTemplate[] templates = new ProcessTaskTemplate.Loader(Factory).FindMatches(Dummy);
			AssertEquals("Should found 2", 2, templates.Length);
			AssertEquals("Should be ordered correctly", activeTemplate2, templates[0]);
			AssertEquals("Should be ordered correctly", activeTemplate1, templates[1]);

			var template2InOtherFactory = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTaskTemplate>(activeTemplate2.PK);
			template2InOtherFactory.Delete();
			template2InOtherFactory.Factory.Save();

			templates = new ProcessTaskTemplate.Loader(Factory).FindMatches(Dummy);
			AssertEquals("Still should found 2", 2, templates.Length);
			AssertEquals(activeTemplate2, templates[0]);
			AssertEquals(activeTemplate1, templates[1]);

			templates = new ProcessTaskTemplate.Loader(Factory).FindMatches(Dummy, true);
			AssertEquals("Now should found 1", 1, templates.Length);
			AssertEquals(activeTemplate1, templates[0]);
		}

		public void TestFindMatchesDoesNotNeedToLoadRelatedTasks()
		{
			ProcessTaskTemplate activeTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate1.P0_ProcessType = "DUM";
			activeTemplate1.P0_SubType1 = "AIR";

			ProcessTaskTemplate activeTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate2.P0_ProcessType = "DUM";
			activeTemplate2.P0_SubType1 = "AIR";
			activeTemplate2.P0_LoadPortCountry = "AU";

			Dummy.SubType1 = "AIR";
			Dummy.LoadPort = "AUSYD";
			var task = Dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Murple";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dummyReloaded = newFactory.Load<DummyWithWorkflow>(Dummy.PK);
			AssertTableHitCount(0, ProcessTasksSchema.Constants.TableName, newFactory);
			new ProcessTaskTemplate.Loader(newFactory).FindMatches(dummyReloaded);
			AssertTableHitCount(0, ProcessTasksSchema.Constants.TableName, newFactory);
		}

		[TestDate(2013, 11, 10)]
		public void TestFindBestProcessTaskTemplateMatches_WithEffectiveStartDate()
		{
			ProcessTaskTemplate.Loader.CacheDurationMinutes_ForTest.Value = 20;

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUM";
			template1.P0_SubType1 = "SEA";

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "DUM";
			template2.P0_SubType1 = "SEA";
			template2.P0_LoadPortCountry = "AU";
			template2.P0_EffectiveStartDateUtc = new ZDateTime(2013, 11, 13);

			Factory.Save();

			var dummy1 = Factory.New<DummyWithWorkflow>(); // Before effective start date
			dummy1.SubType1 = "SEA";
			dummy1.LoadPort = "AUSYD";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			dummy1.Logs.AddNew(AutoEvents.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2013, 11, 15);
			var dummy2 = Factory.New<DummyWithWorkflow>(); // After effective start date
			dummy2.SubType1 = "SEA";
			dummy2.LoadPort = "AUSYD";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			dummy2.Logs.AddNew(AutoEvents.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			var dummy3 = Factory.New<DummyWithWorkflow>(); // Not yet saved to db
			dummy3.SubType1 = "SEA";
			dummy3.LoadPort = "AUSYD";

			AssertFindTemplates(dummy1, template1);
			AssertFindTemplates(dummy2, template2, template1);
			AssertFindTemplates(dummy3, template2, template1);
			AssertFindTemplates(dummy1, template1);
			AssertFindTemplates(dummy3, template2, template1);
			AssertFindTemplates(dummy1, template1);

			AssertExceptionThrown<RegistryValidationException>(() => Registry.Business.WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false));

			template1.P0_IsActive = false;
			template2.P0_EffectiveEndDateUtc = template2.P0_EffectiveStartDateUtc = ZDateTime.Empty;
			Factory.Save();

			Registry.Business.WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertFindTemplates(dummy1, true, template2);
			AssertFindTemplates(dummy2, true, template2);
			AssertFindTemplates(dummy3, true, template2);
		}

		[TestDate(2013, 11, 10)]
		public void TestFindBestProcessTaskTemplateMatches_WithEffectiveStartDate_DbHits()
		{
			ProcessTaskTemplate.Loader.CacheDurationMinutes_ForTest.Value = 20;
			var dummy1 = Factory.New<DummyWithWorkflow>(); // Before effective start date
			dummy1.SubType1 = "SEA";
			dummy1.LoadPort = "AUSYD";
			dummy1.ApplyWorkflowTemplates();

			var hits = new Dictionary<string, int>();
			hits[ProcessTaskTemplate.Schema.TableName] = 0;
			TestDateAttribute.AddMinutes(2);
			using (AssertMaxDbHitsForAllFactories(hits))
			{
				dummy1.ApplyWorkflowTemplates();
			}
		}

		[TestDate(2013, 11, 10)]
		public void TestFindBestProcessTaskTemplateMatches_OnNewBizO()
		{
			var effectiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			effectiveTemplate.P0_ProcessType = "DUM";
			effectiveTemplate.P0_SubType1 = "SEA";
			effectiveTemplate.P0_EffectiveStartDateUtc = new DateTime(2013, 1, 1);

			var nonEffectiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			nonEffectiveTemplate.P0_ProcessType = "DUM";
			nonEffectiveTemplate.P0_SubType1 = "SEA";
			nonEffectiveTemplate.P0_EffectiveStartDateUtc = new DateTime(2014, 1, 1);

			Factory.Save();

			var workflow = Factory.New<DummyWithWorkflow>();
			workflow.SubType1 = "SEA";
			workflow.LoadPort = "AUSYD";

			AssertFindTemplates(workflow, effectiveTemplate);

			AssertExceptionThrown<RegistryValidationException>(() => Registry.Business.WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false));
		}

		public void TestFindBestProcessTaskTemplateMatches_AlreadySpecifiedCompanyPK()
		{
			var dummyWithSpecifiedCompany = Factory.New<DummyWithWorkflowSpecifiedCompanyPK>();

			var globalTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			globalTemplate.P0_ProcessType = "DM1";
			globalTemplate.P0_GC = ZGuid.Empty;

			var activeTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate1.P0_ProcessType = "DM1";
			activeTemplate1.P0_GC = Env.CurrentCompanyPK;
			Factory.Save();

			AssertFindTemplates(dummyWithSpecifiedCompany, activeTemplate1, globalTemplate);

			var company = Factory.New<GlbCompany>();
			dummyWithSpecifiedCompany.SetCompany(company);

			var activeTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate2.P0_ProcessType = "DM1";
			activeTemplate2.P0_GC = company.PK;
			Factory.Save();

			AssertFindTemplates(dummyWithSpecifiedCompany, activeTemplate2, globalTemplate);
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveEndDate()
		{
			var effectiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			effectiveTemplate.P0_ProcessType = "DUM";
			effectiveTemplate.P0_SubType1 = "SEA";
			effectiveTemplate.P0_EffectiveEndDateUtc = new DateTime(2014, 1, 1);

			var nonEffectiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			nonEffectiveTemplate.P0_ProcessType = "DUM";
			nonEffectiveTemplate.P0_SubType1 = "SEA";
			nonEffectiveTemplate.P0_EffectiveEndDateUtc = new DateTime(2013, 1, 1);

			Factory.Save();

			var workflow = Factory.New<DummyWithWorkflow>();
			workflow.SubType1 = "SEA";
			workflow.LoadPort = "AUSYD";

			AssertFindTemplates(workflow, effectiveTemplate);
		}

		[TestDate(2013, 11, 10)]
		[TestUtcOffset(10, 0, 0)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEffectiveEndDate_TimeZone()
		{
			var effectiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			effectiveTemplate.P0_ProcessType = "DUM";
			effectiveTemplate.P0_SubType1 = "SEA";
			effectiveTemplate.P0_EffectiveStartDateUtc = ZDateTime.UtcNow;

			var nonEffectiveTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			nonEffectiveTemplate.P0_ProcessType = "DUM";
			nonEffectiveTemplate.P0_SubType1 = "SEA";
			nonEffectiveTemplate.P0_EffectiveEndDateUtc = ZDateTime.UtcNow;

			Factory.Save();

			var workflow = Factory.New<DummyWithWorkflow>();
			workflow.SubType1 = "SEA";
			workflow.LoadPort = "AUSYD";
			workflow.Z0_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(1);

			AssertFindTemplates(workflow, effectiveTemplate);

			workflow.Z0_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-1);
			AssertFindTemplates(workflow, nonEffectiveTemplate);
		}

		[TestDate(2013, 11, 10, 12, 04, 02)]
		public void TestEffectiveEndDate_UseCreateTimeForTemplateSelectorRatherThanNow()
		{
			var workflow = Factory.New<DummyWithWorkflow>();
			workflow.SubType1 = "SEA";
			workflow.LoadPort = "AUSYD";
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			workflow.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(10);

			var oldTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			oldTemplate.P0_ProcessType = "DUM";
			oldTemplate.P0_SubType1 = "SEA";
			oldTemplate.P0_EffectiveEndDateUtc = TestDateAttribute.Date.AddDays(-8);

			var newTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			newTemplate.P0_ProcessType = "DUM";
			newTemplate.P0_SubType1 = "SEA";
			newTemplate.P0_EffectiveStartDateUtc = TestDateAttribute.Date.AddDays(-8);

			Factory.Save();

			AssertFindTemplates(workflow, oldTemplate);

			workflow = Factory.New<DummyWithWorkflow>();
			workflow.SubType1 = "SEA";
			workflow.LoadPort = "AUSYD";

			AssertFindTemplates(workflow, newTemplate);
		}

		[TestDate(2013, 11, 10, 12, 04, 02)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEffectiveEndDate_UseUtcTime_Sydney()
		{
			AssertUseUtcTime();
		}

		[TestDate(2013, 11, 10, 12, 04, 02)]
		[TestTimeZoneUNLOCO("USCHI")]
		public void TestEffectiveEndDate_UseUtcTime_Chicago()
		{
			AssertUseUtcTime();
		}

		[TestDate(2013, 11, 10, 12, 04, 02)]
		[TestTimeZoneUNLOCO("GBLON")]
		public void TestEffectiveEndDate_UseUtcTime_London()
		{
			AssertUseUtcTime();
		}

		[TestDate(2013, 11, 10, 0, 0, 0)]
		[TestTimeZoneUNLOCO("GBLON")]
		public void TestEffectiveEndDate_UseUtcTime_LondonFence()
		{
			AssertUseUtcTime();
		}

		[TestDate(2013, 11, 10, 0, 0, 0)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEffectiveEndDate_UseUtcTime_SydneyFence()
		{
			AssertUseUtcTime();
		}

		[TestDate(2013, 11, 10, 0, 00, 00)]
		[TestTimeZoneUNLOCO("USCHI")]
		public void TestEffectiveEndDate_UseUtcTime_ChicagoFence()
		{
			AssertUseUtcTime();
		}

		void AssertUseUtcTime()
		{
			var newTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			newTemplate.P0_ProcessType = "DUM";
			newTemplate.P0_SubType1 = "SEA";
			newTemplate.P0_EffectiveStartDateUtc = ZDateTime.UtcNow.AddMinutes(-1);

			var workflow = Factory.New<DummyWithWorkflow>();
			workflow.SubType1 = "SEA";
			workflow.LoadPort = "AUSYD";

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			workflow.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			AssertFindTemplates(workflow, newTemplate);
		}

		[TestDate(2013, 11, 10, 12, 04, 02)]
		public void TestEffectiveEndDate_UseAuditDetailsWhenAvailable()
		{
			var newTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			newTemplate.P0_ProcessType = "DUM";
			newTemplate.P0_SubType1 = "SEA";
			newTemplate.P0_EffectiveStartDateUtc = ZDateTime.UtcNow.AddDays(2);

			Factory.Save();

			var workflow = Factory.New<DummyWithWorkflow>();
			workflow.SubType1 = "SEA";
			workflow.LoadPort = "AUSYD";
			workflow.Z0_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(3);

			AssertFindTemplates(workflow, newTemplate);
		}
		void AssertFindTemplates(IWorkflowProviderCore workflowProvider, params ProcessTaskTemplate[] expectedTemplates)
		{
			AssertFindTemplates(workflowProvider, false, expectedTemplates);
		}

		void AssertFindTemplates(IWorkflowProviderCore workflowProvider, bool ignoreCache, params ProcessTaskTemplate[] expectedTemplates)
		{
			ProcessTaskTemplate[] templates = new ProcessTaskTemplate.Loader(Factory).FindMatches(workflowProvider, ignoreCache);
			AssertEquals("Should be correct number of found templates", expectedTemplates.Length, templates.Length);
			for (int i = 0; i < expectedTemplates.Length; i++)
			{
				AssertEquals("Should be proper template in proper order", templates[i], expectedTemplates[i]);
			}
		}

		#endregion

		#region Implementation

		DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new ProcessTaskTemplate.Loader(Factory);
		}

		#endregion
	}

	#endregion

	#region ProcessTaskTemplateTest

	[TestedType(typeof(ProcessTaskTemplate))]
	class ProcessTaskTemplateTest : EnterpriseBusinessObjectTestCase
	{
		#region BusinessObject Overrides

		public void TestReadOnly()
		{
			AssertEquals("A ProcessTaskTemplate is not ReadOnly by default", false, ProcessTaskTemplate.P0_ProcessTypeInfo.ReadOnly);
			AssertEquals("A ProcessTaskTemplate is not ReadOnly by default", false, ProcessTaskTemplate.P0_LoadPortCountryInfo.ReadOnly);
			AssertEquals("A ProcessTaskTemplate is not ReadOnly by default", false, ProcessTaskTemplate.WorkflowItems.ReadOnly);
			AssertEquals("IsActive is not ReadOnly by default", false, ProcessTaskTemplate.P0_IsActiveInfo.ReadOnly);
			AssertEquals("ReleaseGroupRules should be editable by default", false, ProcessTaskTemplate.ReleaseGroupRules.ReadOnly);

			ProcessTaskTemplate.P0_IsSystem = true;
			Factory.Save();
			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			ProcessTaskTemplate systemTemplate = loadingFactory.Load<ProcessTaskTemplate>(ProcessTaskTemplate.PK);

			AssertEquals("A system ProcessTaskTemplate is ReadOnly", true, systemTemplate.P0_ProcessTypeInfo.ReadOnly);
			AssertEquals("A system ProcessTaskTemplate is ReadOnly", true, systemTemplate.P0_LoadPortCountryInfo.ReadOnly);
			AssertEquals("A system ProcessTaskTemplate is ReadOnly", true, systemTemplate.WorkflowItems.ReadOnly);
			AssertEquals("IsActive is not ReadOnly by default", false, systemTemplate.P0_IsActiveInfo.ReadOnly);
			AssertEquals("ReleaseGroupRules should NOT be editable for system-defined templates", true, ProcessTaskTemplate.ReleaseGroupRules.ReadOnly);

			ProcessTaskTemplate.P0_IsSystem = false;
			AssertEquals("WorkflowItems not read only once P0_IsSystem changed", false, ProcessTaskTemplate.WorkflowItems.ReadOnly);
			AssertEquals("ReleaseGroupRules not read only once P0_IsSystem changed", false, ProcessTaskTemplate.ReleaseGroupRules.ReadOnly);

			Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false;
			AssertEquals("IsActive should be read only if user only has permission to edit in-active templates", true, ProcessTaskTemplate.P0_IsActiveInfo.ReadOnly);
			Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
		}

		public void TestDefaultValues()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, ProcessTaskTemplate.P0_GC);
			AssertEquals(FallbackTypeList.Codes.NeverFallback, ProcessTaskTemplate.P0_CustomFieldFallback);
		}

		public void TestDelete()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var task = template.WorkflowItems.AddNew();
			var header = template.ProcessHeaders.AddNew();
			var releaseGroupRule = template.ReleaseGroupRules.AddNew();
			var templateValidationAction = template.ProcessTemplateValidationActions.AddNew();
			var templateValidation = template.ProcessTemplateValidations.AddNew();

			template.Delete();

			Assert(template.IsDeleted);
			Assert(task.IsDeleted);
			Assert(header.IsDeleted);
			Assert(releaseGroupRule.IsDeleted);
			Assert(templateValidationAction.IsDeleted);
			Assert(templateValidation.IsDeleted);
		}

		public void TestProcessTemplateValidationActions()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			AssertEquals(true, template.IsRegisteredEditableChildObject(template.ProcessTemplateValidationActions));
		}

		public void TestProcessTemplateValidations()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			AssertEquals(true, template.IsRegisteredEditableChildObject(template.ProcessTemplateValidations));
		}

		public void TestIValidationToolParent()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var validationToolParent = (IValidationToolParent)template;
			AssertSame("ProcessTemplateValidations", template.ProcessTemplateValidations, validationToolParent.ProcessTemplateValidations);
		}

		#endregion

		#region Completion Statements

		public void TestCompletionStatements()
		{
			MasterFilesTestHelper.MakeCompletionStatementTaskType("ORG", "COM");
			MasterFilesTestHelper.MakeCompletionStatementTaskType("SAL", "CMP");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";

			var task = template.WorkflowItems.Tasks.AddNew();
			var trigger = template.WorkflowItems.Triggers.AddNew();
			var milestone = template.WorkflowItems.Milestones.AddNew();
			var completionStatement1 = template.CompletionStatementTasks.AddNew();

			AssertEquals("COM", completionStatement1.P9_Type);
			AssertContainsExactElementsInAnyOrder(new[] { completionStatement1 }, template.CompletionStatementTasks);
			AssertContainsExactElementsInAnyOrder("Template tasks should exclude completion statements", new[] { task }, template.TasksExcludingCompletionStatements);

			template.P0_ProcessType = "SAL";
			AssertEquals("Should update completion statement task type when ProcessType is changed", "CMP", completionStatement1.P9_Type);

			var completionStatement2 = template.CompletionStatementTasks.AddNew();
			AssertEquals("New completion statements should default to task type for new process type", "CMP", completionStatement2.P9_Type);

			template.P0_ProcessType = "ZXY";
			AssertEquals("Should leave completion statement task type when ProcessType is changed to an invalid workflow type", "CMP", completionStatement1.P9_Type);
			AssertEquals("Should leave completion statement task type when ProcessType is changed to an invalid workflow type", "CMP", completionStatement2.P9_Type);
		}

		public void TestNewCompletionStatement_OnWorkflowTemplate_ShouldNotShowInvalidTaskType()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedTaskType = categorisedTaskTypes.AddNew();
			categorisedTaskType.Code = "ORG";
			var taskType = categorisedTaskType.TaskTypes.AddNew();
			taskType.Code = "COM";
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			var completionStatement = template.CompletionStatementTasks.AddNew();

			AssertNoErrors("We should have a valid task type immediately after adding a new task to the collection", completionStatement.P9_TypeInfo);

			completionStatement.Validation.ValidateP9_Type();

			AssertNoErrors("We should have a valid task type", completionStatement.P9_TypeInfo);
		}

		public void TestApplyTemplate_ShouldCopyCompletionStatements()
		{
			MasterFilesTestHelper.MakeCompletionStatementTaskType("DUM", "COM");
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var system = ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "DUM");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			var templateCompletionStatement1 = template.CompletionStatementTasks.AddNew();
			var templateCompletionStatement2 = template.CompletionStatementTasks.AddNew();

			var templateWorkflow1 = template.ProcessHeaders.AddNew();
			var templateWorkflow2 = template.ProcessHeaders.AddNew();

			templateWorkflow1.FH_CompletionStatement = "workflow1";
			templateWorkflow2.FH_CompletionStatement = "workflow2";

			var templateTask1 = template.TasksExcludingCompletionStatements.AddNew();
			var templateTask2 = template.TasksExcludingCompletionStatements.AddNew();

			templateCompletionStatement1.P9_FH_ProcessHeader = templateWorkflow1.PK;
			templateCompletionStatement2.P9_FH_ProcessHeader = templateWorkflow2.PK;

			templateTask1.P9_FH_ProcessHeader = templateWorkflow1.PK;
			templateTask2.P9_FH_ProcessHeader = templateWorkflow2.PK;

			templateCompletionStatement1.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("Follow the cops back home"));
			templateCompletionStatement2.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("And rob their houses"));

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow1");
			var workflow2 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow2");

			AssertEquals(4, jobHeader.Parent.WorkflowItems.Count);

			AssertEquals(1, workflow1.TaskCollection.Count);
			AssertEquals(1, workflow2.TaskCollection.Count);
			AssertEquals(1, workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);

			AssertEquals("Follow the cops back home", ORtfTextUtil.RtfToText(workflow1.CompletionStatementTasksIncludingChildWorkflowTasks[0].P9_Notes));
			AssertEquals("And rob their houses", ORtfTextUtil.RtfToText(workflow2.CompletionStatementTasksIncludingChildWorkflowTasks[0].P9_Notes));
		}

		#endregion

		#region HumanReadableName

		public void TestHumanReadableName()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Dat Template";

			AssertEquals("Workflow Template - Dat Template", template.HumanReadableName);
		}

		#endregion

		#region Code Property

		public void TestCodeProperty()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Panda Fodder";

			AssertEquals("Panda Fodder", ((ICodeDescription)template).Code);
		}

		#endregion

		#region IFindBoxListProvider

		public void TestFindBoxListProvider()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template1.P0_Name = "I am the Queen of France";
			template2.P0_Name = "Everybody dance!";
			template3.P0_Name = "This is fun!";

			var collection = new ProcessTaskTemplateCollection(Factory);
			collection.Load();

			var provider = (IFindBoxListProvider)collection;

			AssertEquals(template1, provider.GetBusinessObjectFromCode("I am the Queen of France"));
			AssertEquals(template2, provider.GetBusinessObjectFromCode("Everybody dance!"));
			AssertEquals(template3, provider.GetBusinessObjectFromCode("This is fun!"));

			AssertNull(provider.GetBusinessObjectFromCode("..."));
			AssertNull(provider.GetBusinessObjectFromCode(""));
			AssertNull(provider.GetBusinessObjectFromCode(null));
		}

		#endregion

		#region Universal Triggers

		public void TestUniversalTemplate_WhenNotSupportedByWorkflowType_ShouldAddError()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			CombineAssertions("Marking a template as Universal should add a validation error when that process type does not support Universal templates", () =>
			{
				foreach (var descriptor in WorkflowDescriptors.Instance.Values)
				{
					if (!descriptor.SupportsUniversalTemplates)
					{
						template.P0_ProcessType = descriptor.Code;
						template.P0_IsUniversal = true;

						AssertHasError(template.P0_IsUniversalInfo, "This Process Type does not support Universal templates.");
					}
				}
			});

			if (!template.P0_IsUniversal)
			{
				Assert("Great! Everything supports Universal templates.", true);
			}
		}

		#endregion

		public void TestAvoidEditsFromUsersWithoutPermissionForActiveTemplates()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			Factory.Save();

			var logs = template.Logs.GetAllLogs();
			var newFactory = Factory.CreateNewFactory();
			var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);
			Assert(!loadedTemplate.HasChanges);

			Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false;

			FormCustomisableElement tab = loadedTemplate.FormCustomisationSettings.DisplayTabs.AddNew();
			tab.HasChanges = true;
			tab.DisplayTabCode = "babab";
			tab.DisplayTab = "babab";

			Assert(loadedTemplate.FormCustomisationSettings.DisplayTabs.HasChanges);

			newFactory.Save();

			AssertEquals(logs.Count, loadedTemplate.Logs.GetAllLogs().Count);
		}

		public void TestRights_NewAccessButNotEditForActiveTemplates()
		{
			Env.Security.WorkflowTaskTemplatesNew.IsAllowed = true;
			Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false;

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_Name = "New";
			Factory.Save();
			AssertEquals("I am allowed create a new template", 0, ExceptionReporterTestListener.Instance.Count);

			template.P0_Name = "Edit new";
			Factory.Save();
			AssertEquals("The template is still 'New' until I close it", 0, ExceptionReporterTestListener.Instance.Count);

			var reloadedTemplate = new BusinessObjectFactory().Load<ProcessTaskTemplate>(template.PK);
			reloadedTemplate.P0_Name = "Cannot edit";
			reloadedTemplate.Factory.Save();
			AssertEquals("I cannot edit existing templates", 1, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestCustomizingFormLayoutShouldStillSaveThough()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			Factory.Save();

			var logs = template.Logs.GetAllLogs();
			var newFactory = Factory.CreateNewFactory();
			var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);
			Assert(!loadedTemplate.HasChanges);

			FormCustomisableElement tab = loadedTemplate.FormCustomisationSettings.DisplayTabs.AddNew();
			tab.HasChanges = true;
			tab.DisplayTabCode = "babab";
			tab.DisplayTab = "babab";

			Assert(loadedTemplate.FormCustomisationSettings.DisplayTabs.HasChanges);

			newFactory.Save();

			AssertEquals(logs.Count, loadedTemplate.Logs.GetAllLogs().Count);
		}

		public void TestWorkflowTypeDescription()
		{
			ProcessTaskTemplate.P0_ProcessType = "OPP";
			AssertEquals("Sales Opportunity", ProcessTaskTemplate.WorkflowTypeDescription);

			ProcessTaskTemplate.P0_ProcessType = "XXX";
			AssertEquals("", ProcessTaskTemplate.WorkflowTypeDescription);

			ProcessTaskTemplate.P0_ProcessType = "SHP";
			AssertEquals("Shipment", ProcessTaskTemplate.WorkflowTypeDescription);

			ProcessTaskTemplate.P0_ProcessType = "";
			AssertEquals("", ProcessTaskTemplate.WorkflowTypeDescription);
		}

		public void TestMilestoneTemplateHintCaption()
		{
			ProcessTaskTemplate.P0_ProcessType = "";
			AssertEquals("No WorkflowType", "", ProcessTaskTemplate.MilestoneTemplateHintCaption);
			ProcessTaskTemplate.P0_ProcessType = "XXX";
			AssertEquals("Invalid WorkflowType", "", ProcessTaskTemplate.MilestoneTemplateHintCaption);
			ProcessTaskTemplate.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertEquals("Shipment", true, ProcessTaskTemplate.MilestoneTemplateHintCaption.Length > 0);
		}

		public void TestTemplateCopyAndClone()
		{
			var task = ProcessTaskTemplate.WorkflowItems.AddNew();

			ProcessTaskTemplate clonedTemplate = (ProcessTaskTemplate)ProcessTaskTemplate.Clone();
			Assert(ProcessTaskTemplate.PK != clonedTemplate.PK);
			AssertEquals(false, ProcessTaskTemplate.P0_IsSystem);
			AssertEquals(1, clonedTemplate.WorkflowItems.Count);
			Assert(ProcessTaskTemplate.WorkflowItems[0].PK != clonedTemplate.WorkflowItems[0].PK);
			AssertNull(Factory.GetCachedValue<WorkflowTaskTypeCollection>("ProcessTaskTypesLookups" + ProcessTaskTemplate.WorkflowItems[0].WorkflowType, () => null));
			Factory.ClearCachedValue<WorkflowTaskTypeCollection>("ProcessTaskTypesLookups" + ProcessTaskTemplate.WorkflowItems[0].WorkflowType);

			ProcessTaskTemplate copiedTemplate = (ProcessTaskTemplate)((ITemplateCopyable)ProcessTaskTemplate).TemplateCopy();
			Assert(ProcessTaskTemplate.PK != copiedTemplate.PK);
			AssertEquals(false, ProcessTaskTemplate.P0_IsSystem);
			AssertEquals(1, copiedTemplate.WorkflowItems.Count);
			Assert(ProcessTaskTemplate.WorkflowItems[0].PK != copiedTemplate.WorkflowItems[0].PK);
			AssertEquals(typeof(TemplateProcessTask), copiedTemplate.WorkflowItems[0].GetType());
		}

		public void TestClone_ShouldAlsoCloneCustomFields()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var custom = template.GenCustomColumnDefinitions.AddNew();
			custom.XC_Name = "Hi";

			ProcessTaskTemplate clonedTemplate = (ProcessTaskTemplate)template.Clone();
			AssertEquals(1, clonedTemplate.GenCustomColumnDefinitions.Count);
			AssertEquals("Hi", clonedTemplate.GenCustomColumnDefinitions[0].XC_Name);
			Assert(!Object.ReferenceEquals(custom, clonedTemplate.GenCustomColumnDefinitions[0]));
		}

		public void TestClone_ShouldAlsoCloneScreenLayout()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var displayField1 = template.FormCustomisationSettings.DisplayFields.AddNew();
			displayField1.ElementName = "DF1";
			displayField1.ElementDescription = "display field 1";
			displayField1.ElementGroup = "PET";
			displayField1.DisplayTabCode = "TABB";
			displayField1.Placement = TabPlacement.Placements.BottomRight;
			displayField1.RowNumber = 1;

			var displayField2 = template.FormCustomisationSettings.DisplayFields.AddNew();
			displayField2.Placement = TabPlacement.Placements.TopLeft;
			displayField2.ElementName = "DF2";
			displayField2.ElementDescription = "display field 2";
			displayField2.ElementGroup = "TEP";
			displayField2.DisplayTabCode = "BBC";
			displayField2.Placement = TabPlacement.Placements.TopLeft;
			displayField2.RowNumber = 2;
			displayField2.IsAvailableFunction = () => false;

			var displayTab1 = template.FormCustomisationSettings.DisplayTabs.AddNew();
			displayTab1.ElementName = "TART";
			displayTab1.ElementDescription = "display tab 2";
			displayTab1.ElementGroup = "TARR";
			displayTab1.DisplayTabCode = "TABB";
			displayTab1.Visible = !displayTab1.Visible;

			ProcessTaskTemplate clonedTemplate = (ProcessTaskTemplate)template.Clone();

			var clonedDisplayField1 = clonedTemplate.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>().FirstOrDefault(x => x.ElementName == displayField1.ElementName);
			AssertNotNull("Cloned display field 1 shant be null", clonedDisplayField1);
			AssertEquals("Cloned display field 1 ElementDescription", displayField1.ElementDescription, clonedDisplayField1.ElementDescription);
			AssertEquals("Cloned display field 1 row ElementGroup", displayField1.ElementGroup, clonedDisplayField1.ElementGroup);
			AssertEquals("Cloned display field 1 DisplayTabCode", displayField1.DisplayTabCode, clonedDisplayField1.DisplayTabCode);
			AssertEquals("Cloned display field 1 row number", displayField1.RowNumber, clonedDisplayField1.RowNumber);
			AssertEquals("Cloned display field 1 placement", displayField1.Placement, clonedDisplayField1.Placement);
			AssertEquals("Cloned display field 1 row number", displayField1.RowNumber, clonedDisplayField1.RowNumber);

			var clonedDisplayField2 = clonedTemplate.FormCustomisationSettings.DisplayFields.Cast<FormCustomisableElement>().FirstOrDefault(x => x.ElementName == displayField2.ElementName);
			AssertNotNull("Cloned display field 2 shant be null", clonedDisplayField2);
			AssertEquals("Cloned display field 2 ElementDescription", displayField2.ElementDescription, clonedDisplayField2.ElementDescription);
			AssertEquals("Cloned display field 2 row ElementGroup", displayField2.ElementGroup, clonedDisplayField2.ElementGroup);
			AssertEquals("Cloned display field 2 DisplayTabCode", displayField2.DisplayTabCode, clonedDisplayField2.DisplayTabCode);
			AssertEquals("Cloned display field 2 row number", displayField2.RowNumber, clonedDisplayField2.RowNumber);
			AssertEquals("Cloned display field 2 placement", displayField2.Placement, clonedDisplayField2.Placement);
			AssertEquals("Cloned display field 2 row number", displayField2.RowNumber, clonedDisplayField2.RowNumber);

			var clonedDisplayTab1 = clonedTemplate.FormCustomisationSettings.DisplayTabs.Cast<FormCustomisableElement>().FirstOrDefault(x => x.ElementName == displayTab1.ElementName);
			AssertEquals("Cloned display tab 2 ElementDescription", displayTab1.ElementDescription, clonedDisplayTab1.ElementDescription);
			AssertEquals("Cloned display tab 2 row ElementGroup", displayTab1.ElementGroup, clonedDisplayTab1.ElementGroup);
			AssertEquals("Cloned display tab 2 DisplayTabCode", displayTab1.DisplayTabCode, clonedDisplayTab1.DisplayTabCode);
			AssertNotNull("CLoned display tab 1 shant be null", clonedDisplayTab1);
			AssertEquals("Cloned display tab 1 visible", displayTab1.Visible, clonedDisplayTab1.Visible);
		}

		public void TestClone_ScreenLayoutFromSystemTemplateIsNotReadOnly()
		{
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			var screenLayoutField = template.FormCustomisationSettings.DisplayFields.AddNew();
			screenLayoutField.CopyValues(descriptor.FormCustomisationSettings.DisplayFields[0]);
			screenLayoutField.ElementName = "Test display field";
			AssertEquals("Display field is visible", true, template.FormCustomisationSettings.DisplayFields.IsElementVisible("Test display field"));
			template.P0_IsSystem = true;
			Factory.Save();

			var clonedTemplate = (ProcessTaskTemplate)template.Clone();
			AssertEquals($"{template.P0_Name} - Copy", clonedTemplate.P0_Name);
			AssertEquals(false, clonedTemplate.P0_IsSystem);
			AssertEquals(false, clonedTemplate.FormCustomisationSettings.DisplayTabs.ReadOnly);
			AssertEquals(false, clonedTemplate.FormCustomisationSettings.DisplayFields.ReadOnly);
		}

		public void TestClone_ShouldAlsoCloneWorkflowsAndLinks()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "WKI";

			var workflow1 = template.ProcessHeaders.AddNew();
			var workflow2 = template.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "Slice";
			workflow2.FH_CompletionStatement = "Dice";

			var ruleTags = BMSTestHelper.CreateTagDefinition(Factory, "RUL");
			var ruleTag = BMSTestHelper.CreateTagMagnitude(ruleTags, "RUL");

			var tagLink = workflow1.Factory.New<TagLink>();
			tagLink.TGL_ParentId = workflow1.PK;
			tagLink.TGL_TGM_Magnitude = ruleTag.PK;

			BMSTestHelper.CreateDependencyLink(template, workflow1, workflow2);

			var task1 = template.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			var task2 = template.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;

			var clonedTemplate = (ProcessTaskTemplate)template.Clone();
			AssertEquals(3, clonedTemplate.ProcessHeaders.Count);

			var workflowClone1 = clonedTemplate.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "Slice");
			var workflowClone2 = clonedTemplate.ProcessHeaders.Cast<IProcessHeader>().Single(w => w.FH_CompletionStatement == "Dice");

			AssertEquals(1, workflowClone1.LinksFromMeToOthers.Count);
			AssertEquals(workflowClone1.PK, workflowClone1.LinksFromMeToOthers[0].FP_FH_HeaderFrom);
			AssertEquals(workflowClone2.PK, workflowClone1.LinksFromMeToOthers[0].FP_FH_HeaderTo);

			AssertEquals(2, clonedTemplate.WorkflowItems.Count);
			AssertEquals(workflowClone1.PK, clonedTemplate.WorkflowItems[0].P9_FH_ProcessHeader);
			AssertEquals(workflowClone2.PK, clonedTemplate.WorkflowItems[1].P9_FH_ProcessHeader);

			AssertContainsExactElementsInAnyOrder(new[] { ruleTag }, workflowClone1.TagLinks.Select(link => link.TagMagnitude));
		}

		public void TestCopyReleaseGroupRules()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "WKI";

			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();

			var rule1 = (IProcessTemplateReleaseGroupRule)template.ReleaseGroupRules.AddNew();
			rule1.PTR_IsActive = true;
			rule1.PTR_AreAllWorkflowCategoriesApplicable = false;
			rule1.PTR_Name = "The boss is always right";
			rule1.PTR_Sequence = 1;
			rule1.PTR_ValueSelectionMacro = "<WKI_Summary>";
			var mapping11 = (IProcessTemplateReleaseGroupRuleMapping)rule1.GroupMappings.AddNew();
			mapping11.PTM_GG_Group = releaseGroup1.PK;
			mapping11.PTM_Value = "Yes sir";
			var mapping21 = (IProcessTemplateReleaseGroupRuleMapping)rule1.GroupMappings.AddNew();
			mapping21.PTM_GG_Group = releaseGroup2.PK;
			mapping21.PTM_Value = "Can do sir";
			var category1 = (IProcessTemplateReleaseGroupRuleCategory)rule1.Categories.AddNew();
			category1.PTC_Category = "YES";
			var category2 = (IProcessTemplateReleaseGroupRuleCategory)rule1.Categories.AddNew();
			category2.PTC_Category = "AHA";

			var rule2 = (IProcessTemplateReleaseGroupRule)template.ReleaseGroupRules.AddNew();
			rule2.PTR_IsActive = false;
			rule2.PTR_AreAllWorkflowCategoriesApplicable = true;
			rule2.PTR_Name = "If the boss is wrong, see rule 1";
			rule2.PTR_Sequence = 2;
			rule2.PTR_ValueSelectionMacro = "<WKI_Summary>";
			var mapping12 = (IProcessTemplateReleaseGroupRuleMapping)rule2.GroupMappings.AddNew();
			mapping12.PTM_GG_Group = releaseGroup2.PK;
			mapping12.PTM_Value = "No sir";
			var mapping22 = (IProcessTemplateReleaseGroupRuleMapping)rule2.GroupMappings.AddNew();
			mapping22.PTM_GG_Group = releaseGroup1.PK;
			mapping22.PTM_Value = "Are you sure about that sir";

			var clonedTemplate = (ProcessTaskTemplate)template.Clone();

			AssertEquals(2, clonedTemplate.ReleaseGroupRules.Count);

			var clonedRule1 = clonedTemplate.ReleaseGroupRules.ToList<IProcessTemplateReleaseGroupRule>().SingleOrDefault(r => r.PTR_Sequence == 1);
			AssertNotNull(clonedRule1);
			AssertEquals(true, clonedRule1.PTR_IsActive);
			AssertEquals(false, clonedRule1.PTR_AreAllWorkflowCategoriesApplicable);
			AssertEquals("The boss is always right", clonedRule1.PTR_Name);
			AssertEquals("<WKI_Summary>", clonedRule1.PTR_ValueSelectionMacro);

			AssertEquals(2, clonedRule1.GroupMappings.Count);
			AssertEquals(2, clonedRule1.Categories.Count);

			var clonedMapping11 = clonedRule1.GroupMappings.ToList<IProcessTemplateReleaseGroupRuleMapping>().SingleOrDefault(m => m.PTM_GG_Group == releaseGroup1.PK);
			AssertNotNull(clonedMapping11);
			AssertEquals("Yes sir", clonedMapping11.PTM_Value);
			var clonedMapping12 = clonedRule1.GroupMappings.ToList<IProcessTemplateReleaseGroupRuleMapping>().SingleOrDefault(m => m.PTM_GG_Group == releaseGroup2.PK);
			AssertNotNull(clonedMapping12);
			AssertEquals("Can do sir", clonedMapping12.PTM_Value);

			AssertNotNull(clonedRule1.Categories.ToList<IProcessTemplateReleaseGroupRuleCategory>().SingleOrDefault(c => c.PTC_Category == "YES"));
			AssertNotNull(clonedRule1.Categories.ToList<IProcessTemplateReleaseGroupRuleCategory>().SingleOrDefault(c => c.PTC_Category == "AHA"));

			var clonedRule2 = clonedTemplate.ReleaseGroupRules.ToList<IProcessTemplateReleaseGroupRule>().SingleOrDefault(r => r.PTR_Sequence == 2);
			AssertNotNull(clonedRule2);
			AssertEquals(false, clonedRule2.PTR_IsActive);
			AssertEquals(true, clonedRule2.PTR_AreAllWorkflowCategoriesApplicable);
			AssertEquals("If the boss is wrong, see rule 1", clonedRule2.PTR_Name);
			AssertEquals("<WKI_Summary>", clonedRule2.PTR_ValueSelectionMacro);

			AssertEquals(2, clonedRule2.GroupMappings.Count);
			AssertEquals(0, clonedRule2.Categories.Count);

			var clonedMapping21 = clonedRule2.GroupMappings.ToList<IProcessTemplateReleaseGroupRuleMapping>().SingleOrDefault(m => m.PTM_GG_Group == releaseGroup2.PK);
			AssertNotNull(clonedMapping21);
			AssertEquals("No sir", clonedMapping21.PTM_Value);
			var clonedMapping22 = clonedRule2.GroupMappings.ToList<IProcessTemplateReleaseGroupRuleMapping>().SingleOrDefault(m => m.PTM_GG_Group == releaseGroup1.PK);
			AssertNotNull(clonedMapping22);
			AssertEquals("Are you sure about that sir", clonedMapping22.PTM_Value);
		}

		public void TestCopyWorkflowTemplate()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var releaseGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "WKI";

			var workflow0 = template.GetJobHeader();
			var workflow1 = template.ProcessHeaders.AddNew();
			var workflow2 = template.ProcessHeaders.AddNew();

			workflow0.FH_CompletionStatement = "Mince";
			workflow1.FH_CompletionStatement = "Slice";
			workflow2.FH_CompletionStatement = "Dice";

			workflow0.FH_GG_ReleaseGroup = releaseGroup1.PK;
			workflow1.FH_GG_ReleaseGroup = releaseGroup2.PK;
			workflow2.FH_GG_ReleaseGroup = releaseGroup2.PK;

			BMSTestHelper.CreateDependencyLink(template, workflow1, workflow2);

			var task1 = template.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			var task2 = template.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;

			var clonedTemplate = (ProcessTaskTemplate)template.Clone();

			var workflowClone0 = clonedTemplate.ProcessHeaders.Cast<ProcessHeader>().Single(w => w.FH_CompletionStatement == "Mince");
			var workflowClone1 = clonedTemplate.ProcessHeaders.Cast<ProcessHeader>().Single(w => w.FH_CompletionStatement == "Slice");
			var workflowClone2 = clonedTemplate.ProcessHeaders.Cast<ProcessHeader>().Single(w => w.FH_CompletionStatement == "Dice");

			AssertEquals("Should clone the job level workflow", false, workflowClone0.IsWorkflow);
			AssertNotNull("Should only be ONE job level workflow cloned", clonedTemplate.ProcessHeaders.Cast<ProcessHeader>().Single(w => !w.IsWorkflow));

			AssertEquals(1, workflowClone1.Links.Count());
			AssertEquals(ProcessHeaderLinkTypeList.Codes.Dependency, workflowClone1.Links.First().FP_LinkType);
			AssertEquals(1, workflowClone1.PostrequisiteLinks.Count());
			AssertEquals(1, workflowClone2.PrerequisiteLinks.Count());

			AssertEquals(workflow0.FH_GG_ReleaseGroup, workflowClone0.FH_GG_ReleaseGroup);
			AssertEquals(workflow1.FH_GG_ReleaseGroup, workflowClone1.FH_GG_ReleaseGroup);
			AssertEquals(workflow2.FH_GG_ReleaseGroup, workflowClone2.FH_GG_ReleaseGroup);
		}

		public void TestAllCompanies()
		{
			AssertEquals(GlbCompany.CurrentCompany.PK, ProcessTaskTemplate.P0_GC);
			AssertEquals(false, ProcessTaskTemplate.GlobalTemplate);

			ProcessTaskTemplate.GlobalTemplate = true;
			AssertEquals(true, ProcessTaskTemplate.GlobalTemplate);
			AssertEquals(true, ProcessTaskTemplate.P0_GC.IsEmpty);

			ProcessTaskTemplate.GlobalTemplate = false;
			AssertEquals(false, ProcessTaskTemplate.GlobalTemplate);
			AssertEquals(GlbCompany.CurrentCompany.PK, ProcessTaskTemplate.P0_GC);

			ProcessTaskTemplate.P0_GC = ZGuid.Empty;
			AssertEquals(true, ProcessTaskTemplate.GlobalTemplate);
			AssertEquals(true, ProcessTaskTemplate.P0_GC.IsEmpty);

			ProcessTaskTemplate.P0_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals(false, ProcessTaskTemplate.GlobalTemplate);
			AssertEquals(GlbCompany.CurrentCompany.PK, ProcessTaskTemplate.P0_GC);
		}

		public void TestTasks()
		{
			AssertNotNull(ProcessTaskTemplate.WorkflowItems);
		}

		public void TestWorkflowProvider()
		{
			AssertNull("No process type so no task provider yet", ProcessTaskTemplate.WorkflowDescriptor);
			ProcessTaskTemplate.P0_ProcessType = new OpportunityWorkflowDescriptor().Code;
			AssertEquals("Correct task provider loaded", typeof(OpportunityWorkflowDescriptor), ProcessTaskTemplate.WorkflowDescriptor.GetType());
		}

		public void TestWorkflowSubTypeInformation()
		{
			AssertEquals("No SubTypeInformation initially", 0, ProcessTaskTemplate.WorkflowSubTypeInformation.Length);
			ProcessTaskTemplate.P0_ProcessType = new OpportunityWorkflowDescriptor().Code;
			AssertEquals("Correct task provider loaded", typeof(OpportunityWorkflowDescriptor), ProcessTaskTemplate.WorkflowDescriptor.GetType());
			AssertEquals("When a valid workflow type is set", 3, ProcessTaskTemplate.WorkflowSubTypeInformation.Length);
		}

		public void TestSubTypeDescriptions()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";

			ProcessTaskTemplate.P0_SubType1 = "COL";
			AssertEquals("Colombia", ProcessTaskTemplate.SubType1Description);

			ProcessTaskTemplate.P0_SubType1 = "";
			AssertEquals("", ProcessTaskTemplate.SubType1Description);

			ProcessTaskTemplate.P0_SubType1 = "XXX";
			AssertEquals("", ProcessTaskTemplate.SubType1Description);

			ProcessTaskTemplate.P0_SubType2 = "AUS";
			AssertEquals("Australia", ProcessTaskTemplate.SubType2Description);

			ProcessTaskTemplate.P0_SubType3 = "USA";
			AssertEquals("United States of America", ProcessTaskTemplate.SubType3Description);

			ProcessTaskTemplate.P0_SubType4 = "RUS";
			AssertEquals("Russia", ProcessTaskTemplate.SubType4Description);

			ProcessTaskTemplate.P0_SubType5 = "MON";
			AssertEquals("The Moon", ProcessTaskTemplate.SubType5Description);
		}

		public void TestClientRequired()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";
			AssertEquals("Client Not Required", false, ProcessTaskTemplate.ClientExists);

			DummyWorkflowDescriptor.Instance.ClientNeeded = true;
			AssertEquals("Client Required", true, ProcessTaskTemplate.ClientExists);

			DummyWorkflowDescriptor.Instance.ClientNeeded = false;
			AssertEquals("Client not Required", false, ProcessTaskTemplate.ClientExists);
		}

		public void TestClientName()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";
			AssertEquals(DummyWorkflowDescriptor.Instance.ClientName, ProcessTaskTemplate.ClientName);

			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.AccDraftInvoiceCode;
			AssertEquals(DummyWorkflowDescriptor.Instance.CreditorName, ProcessTaskTemplate.ClientName);

			ProcessTaskTemplate.P0_ProcessType = WorkflowDescriptors.APInvoiceCode;
			AssertEquals(DummyWorkflowDescriptor.Instance.CreditorName, ProcessTaskTemplate.ClientName);
		}

		public void TestClientLookup()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";

			AssertNotNull("uses default client list when no provider present", ProcessTaskTemplate.ClientList);

			var clientList1 = new OrgHeaderCollection(Factory);
			var clientList2 = new OrgHeaderCollection(Factory);

			DummyWorkflowDescriptor.Instance.ClientListProviderExposed = template =>
			{
				switch (template.P0_SubType1)
				{
					case "one":
						return clientList1;

					case "two":
						return clientList2;

					default:
						return null;
				}
			};

			ProcessTaskTemplate.P0_SubType1 = "one";
			AssertEquals("uses overriden client list", clientList1, ProcessTaskTemplate.ClientList);

			ProcessTaskTemplate.P0_SubType1 = "two";
			AssertEquals("uses overriden client list", clientList2, ProcessTaskTemplate.ClientList);

			ProcessTaskTemplate.P0_SubType1 = "three";
			AssertNotNull("uses default client list because provider didn't return any", ProcessTaskTemplate.ClientList);
		}

		public void TestWarehouseExists()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";
			AssertEquals("Warehouse does not exist", false, ProcessTaskTemplate.WarehouseExists);

			DummyWorkflowDescriptor.Instance.WarehouseNeeded = true;
			AssertEquals("Warehouse  exists", true, ProcessTaskTemplate.WarehouseExists);

			DummyWorkflowDescriptor.Instance.WarehouseNeeded = false;
			AssertEquals("Warehouse does not exist", false, ProcessTaskTemplate.WarehouseExists);
		}

		public void TestPort1Required()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";
			AssertEquals("Port 1 Not Required", false, ProcessTaskTemplate.LoadPortExists);

			DummyWorkflowDescriptor.Instance.Port1Needed = true;
			AssertEquals("Port 1  Required", true, ProcessTaskTemplate.LoadPortExists);

			DummyWorkflowDescriptor.Instance.Port1Needed = false;
			AssertEquals("Port 1  not Required", false, ProcessTaskTemplate.LoadPortExists);
		}

		public void TestPort2Required()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";
			AssertEquals("Port 2 Not Required", false, ProcessTaskTemplate.DischargePortExists);

			DummyWorkflowDescriptor.Instance.Port2Needed = true;
			AssertEquals("Port 2  Required", true, ProcessTaskTemplate.DischargePortExists);

			DummyWorkflowDescriptor.Instance.Port2Needed = false;
			AssertEquals("Port 2  not Required", false, ProcessTaskTemplate.DischargePortExists);
		}

		public void TestSubTypeExistsAndLabels()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";

			Assert(ProcessTaskTemplate.SubType1Exists);
			Assert(ProcessTaskTemplate.SubType2Exists);
			Assert(ProcessTaskTemplate.SubType3Exists);
			Assert(ProcessTaskTemplate.SubType4Exists);

			AssertEquals("South America (1)", ProcessTaskTemplate.SubType1Label);
			AssertEquals("Asia (2)", ProcessTaskTemplate.SubType2Label);
			AssertEquals("North America (3)", ProcessTaskTemplate.SubType3Label);
			AssertEquals("Europe (4)", ProcessTaskTemplate.SubType4Label);
		}

		public void TestSubTypeIsMandatory()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";

			Assert(ProcessTaskTemplate.SubType1IsMandatory);
			Assert(!ProcessTaskTemplate.SubType2IsMandatory);
			Assert(ProcessTaskTemplate.SubType3IsMandatory);
			Assert(!ProcessTaskTemplate.SubType4IsMandatory);
		}

		public void TestSubTypeIsMandatory_NothingIsMadatoryForPartialTemplates()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";
			ProcessTaskTemplate.P0_IsPartialTemplate = true;

			Assert(!ProcessTaskTemplate.SubType1IsMandatory);
			Assert(!ProcessTaskTemplate.SubType2IsMandatory);
			Assert(!ProcessTaskTemplate.SubType3IsMandatory);
			Assert(!ProcessTaskTemplate.SubType4IsMandatory);
		}

		public void TestBranchExists()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";
			AssertEquals("Branch Not Required", false, ProcessTaskTemplate.BranchExists);

			DummyWorkflowDescriptor.Instance.BranchNeeded = true;
			AssertEquals("Branch Required", true, ProcessTaskTemplate.BranchExists);

			DummyWorkflowDescriptor.Instance.BranchNeeded = false;
			AssertEquals("Branch not Required", false, ProcessTaskTemplate.BranchExists);
		}

		public void TestDepartmentExists()
		{
			ProcessTaskTemplate.P0_ProcessType = "DUM";
			AssertEquals("Department Not Required", false, ProcessTaskTemplate.DepartmentExists);

			DummyWorkflowDescriptor.Instance.DepartmentNeeded = true;
			AssertEquals("Department Required", true, ProcessTaskTemplate.DepartmentExists);

			DummyWorkflowDescriptor.Instance.DepartmentNeeded = false;
			AssertEquals("Department not Required", false, ProcessTaskTemplate.DepartmentExists);
		}

		public void TestP0_ProcessType_CausesWorkflowItemsToBeValidated()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTask workflowItem = template.WorkflowItems.Triggers.AddNew();

			template.P0_ProcessType = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			ProcessTaskNotification notification = workflowItem.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "XMF";
			//workflowItem.MessagingTriggerParties = MessageRecipientPartyType.Consignee;
			AssertNoErrors("No errors as Order module supports XMF trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);

			workflowItem.MarkLightValidationAsValidForTesting();
			template.P0_ProcessType = WorkflowDescriptors.ContainerWorkflowDescriptorCode;
			AssertEquals("Workflow item should be re-validated now", false, workflowItem.LightValidationIsValid);
			template.RunPreSaveValidation();
			AssertHasErrors("Error as Container module doesn't supports XMF trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);
		}

		public void TestP0_ProcessType_CausesWorkflowItemsToBeValidatedForPOATrigger()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTask workflowItem = template.WorkflowItems.Triggers.AddNew();

			template.P0_ProcessType = "CON";
			ProcessTaskNotification notification = workflowItem.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "POA";
			AssertNoErrors("No errors as Consol module supports POA trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);

			workflowItem.MarkLightValidationAsValidForTesting();
			template.P0_ProcessType = WorkflowDescriptors.ContainerWorkflowDescriptorCode;
			AssertEquals("Workflow item should be re-validated now", false, workflowItem.LightValidationIsValid);
			template.RunPreSaveValidation();
			AssertHasErrors("Error as Container module doesn't supports POA trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);

			workflowItem.MarkLightValidationAsValidForTesting();
			template.P0_ProcessType = "BRK";
			AssertEquals("Workflow item should be re-validated now", false, workflowItem.LightValidationIsValid);
			template.RunPreSaveValidation();
			AssertNoErrors("No errors as Job Declaration module supports POA trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);

			workflowItem.MarkLightValidationAsValidForTesting();
			template.P0_ProcessType = "TRN";
			AssertEquals("Workflow item should be re-validated now", false, workflowItem.LightValidationIsValid);
			template.RunPreSaveValidation();
			AssertHasErrors("Error as Container module doesn't supports POA trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);

			workflowItem.MarkLightValidationAsValidForTesting();
			template.P0_ProcessType = "SHP";
			AssertEquals("Workflow item should be re-validated now", false, workflowItem.LightValidationIsValid);
			template.RunPreSaveValidation();
			AssertNoErrors("No errors as Shipment module supports POA trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);

			workflowItem.MarkLightValidationAsValidForTesting();
			template.P0_ProcessType = "BOL";
			AssertEquals("Workflow item should be re-validated now", false, workflowItem.LightValidationIsValid);
			template.RunPreSaveValidation();
			AssertHasErrors("Error as Warehouse Adjustment module doesn't supports POA trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);

			workflowItem.MarkLightValidationAsValidForTesting();
			template.P0_ProcessType = "QBK";
			AssertEquals("Workflow item should be re-validated now", false, workflowItem.LightValidationIsValid);
			template.RunPreSaveValidation();
			AssertNoErrors("No errors as Quoted Booking module supports POA trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);

			workflowItem.MarkLightValidationAsValidForTesting();
			template.P0_ProcessType = "ISF";
			AssertEquals("Workflow item should be re-validated now", false, workflowItem.LightValidationIsValid);
			template.RunPreSaveValidation();
			AssertHasErrors("Error as Transport Booking module doesn't supports POA trigger", workflowItem.ProcessTaskNotifications[0].PQ_TriggerTypeInfo);
		}

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(ProcessTaskTemplate)));
		}

		public void TestClearCriteriaIfRequired()
		{
			AssertClearCriteriaIfRequired(true, true, true, true, true);
			AssertClearCriteriaIfRequired(false, true, false, true, false);
			AssertClearCriteriaIfRequired(true, false, true, false, true);
			AssertClearCriteriaIfRequired(false, false, false, false, false);
		}

		public void TestP0_ProcessType_ResetsFormCustomisableFields()
		{
			bool resetCalled = false;

			ProcessTaskTemplateForTest template = Factory.New<ProcessTaskTemplateForTest>();
			template.P0_Name = "Test";
			FormCustomisationSettingsForTest settings = new FormCustomisationSettingsForTest(template);
			settings.ResetImplementation = () => resetCalled = true;

			template.GetNewFormCustomisationSettingsImplementation = () => settings;

			AssertEquals("ensure formCustomisationSettings field gets initialized", settings, template.FormCustomisationSettings);

			template.P0_ProcessType = "XXX";

			AssertEquals(true, resetCalled);

			Factory.Save();

			var reloadedTemplate = new BusinessObjectFactory().Load<ProcessTaskTemplateForTest>(template.PK);

			AssertEquals(false, reloadedTemplate.CalledFormCustimisationsSettings);
			reloadedTemplate.P0_ProcessType = "YYY";
			AssertEquals(true, reloadedTemplate.CalledFormCustimisationsSettings);
		}

		public void TestP0_ProcessTypeResetsCustomisedTabsAndP0_FormState()
		{
			ProcessTaskTemplateForTest template = Factory.New<ProcessTaskTemplateForTest>();
			template.P0_Name = "Test";
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			AssertEquals(0, template.P0_FormState.Length);
			AssertEquals(true, template.FormCustomisationSettings.DisplayTabs.Count > 0);

			template.FormCustomisationSettings.DisplayTabs.ForEach(element => ((FormCustomisableElement)element).Visible = false);

			Factory.Save();

			CombineAssertions("Display tabs is correctly serialized in P0_FormState", () =>
			{
				var formStateWithShipmentTabsHidden = WebUtility.HtmlDecode(template.P0_FormState.ToUTF8());
				template.FormCustomisationSettings.DisplayTabs.ForEach(element =>
				{
					var customElement = (FormCustomisableElement)element;
					AssertContains($"<Tab>\r\n    <Name>{customElement.ElementName}</Name>\r\n    <Description>{customElement.ElementDescription}</Description>\r\n    <Visible>false</Visible>\r\n  </Tab>", formStateWithShipmentTabsHidden);
				});
				AssertEquals("Tabs in DisplayTabs should match tabs in P0_FormState", template.FormCustomisationSettings.DisplayTabs.Count, new ZString(formStateWithShipmentTabsHidden).Split("<Tab>").Length - 1);
			});

			var reloadFactory = new BusinessObjectFactory();
			var reloadedTemplate = reloadFactory.Load<ProcessTaskTemplateForTest>(template.PK);
			reloadedTemplate.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			reloadFactory.Save();

			CombineAssertions("Display tabs is correctly serialized in P0_FormState", () =>
			{
				var formStateForConsol = WebUtility.HtmlDecode(reloadedTemplate.P0_FormState.ToUTF8());
				reloadedTemplate.FormCustomisationSettings.DisplayTabs.ForEach(element =>
				{
					var customElement = (FormCustomisableElement)element;
					AssertContains("All tabs should be included and marked as visible in P0_FormState", $"<Tab>\r\n    <Name>{customElement.ElementName}</Name>\r\n    <Description>{customElement.ElementDescription}</Description>\r\n    <Visible>true</Visible>\r\n  </Tab>", formStateForConsol);
				});
				AssertEquals("Tabs in DisplayTabs should match tabs in P0_FormState", reloadedTemplate.FormCustomisationSettings.DisplayTabs.Count, new ZString(formStateForConsol).Split("<Tab>").Length - 1);
			});
		}

		public void TestP0_ProcessTask_ValidatesSubType1()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_SubType1 = "COU";
			template.P0_Name = "Test";
			template.P0_Description = "Test";

			Factory.Save();

			var reloadedTemplate = new BusinessObjectFactory().Load<ProcessTaskTemplate>(template.PK);

			reloadedTemplate.P0_ProcessType = "CON";
			AssertHasError("COU is not valid for this Process Type", reloadedTemplate.P0_SubType1Info, "Enter a valid Process Sub Type 1.");
		}

		public void TestClone()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			ProcessTask task1 = template.WorkflowItems.Milestones.AddNew();
			task1.TriggerConditions.TriggerEventCode = "ARV";
			ProcessTaskNotification notification1 = task1.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = "ABC";
			notification1.PQ_EmailText = "Text";

			ProcessTask task2 = template.WorkflowItems.Triggers.AddNew();
			task2.TriggerConditions.TriggerEventCode = "DEP";
			task2.ProcessTaskNotifications.AddNew().PQ_TriggerType = "XYZ";

			ProcessTaskTemplate clonedTemplate = (ProcessTaskTemplate)template.Clone();

			AssertEquals(1, clonedTemplate.WorkflowItems.Milestones.Count);
			AssertNotEquals(task1.PK, clonedTemplate.WorkflowItems.Milestones[0].PK);
			AssertEquals("ARV", clonedTemplate.WorkflowItems.Milestones[0].P9_SE_NKMilestoneEvent);
			AssertEquals(1, clonedTemplate.WorkflowItems.Milestones[0].ProcessTaskNotifications.Count);
			AssertEquals("ABC", clonedTemplate.WorkflowItems.Milestones[0].ProcessTaskNotifications[0].PQ_TriggerType);
			AssertEquals("Text", clonedTemplate.WorkflowItems.Milestones[0].ProcessTaskNotifications[0].PQ_EmailText);

			AssertEquals(1, clonedTemplate.WorkflowItems.Triggers.Count);
			AssertNotEquals(task2.PK, clonedTemplate.WorkflowItems.Triggers[0].PK);
			AssertEquals("DEP", clonedTemplate.WorkflowItems.Triggers[0].P9_SE_NKMilestoneEvent);
			AssertEquals(1, clonedTemplate.WorkflowItems.Triggers[0].ProcessTaskNotifications.Count);
			AssertEquals("XYZ", clonedTemplate.WorkflowItems.Triggers[0].ProcessTaskNotifications[0].PQ_TriggerType);
			Assert(clonedTemplate.WorkflowItems.Triggers[0].ProcessTaskNotifications[0].PQ_EmailText.IsEmpty);
		}

		public void TestSave_ReportErrorWhenForbiddenNoReportWhenAllowed()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = false;

			var securityRecord = Factory.New<GlbSecurity>();
			securityRecord.GU_SecurityRight = Env.Security.WorkflowTaskTemplatesEdit.Code;
			securityRecord.GU_SecurityItemIsAllowed = false;
			securityRecord.GU_GS = staff.PK;
			staff.GroupSecurityPermissionsCollectionForBinding.Add(securityRecord);

			Factory.Save();

			var branchGuid = GlbBranch.CurrentBranch.PK.ToGuid();
			var departmentGuid = GlbDepartment.CurrentDepartment.PK.ToGuid();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(staff.GS_LoginName, branchGuid, departmentGuid))
				{
					var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

					AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
					Factory.Save();

					AssertEquals("We should start detecting when disallowed users save task templates.", 1, ExceptionReporterTestListener.Instance.Count);
					AssertContains("Enterprise.MasterFiles.Business.ProcessTaskTemplate.CheckPermissionAndRecordInvalidChanges(Object sender, HasChangesChangedEventArgs e)", ExceptionReporterTestListener.Instance[0].InnerException.Message);
					ExceptionReporterTestListener.Instance.Clear();
				}
			}

			securityRecord.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(staff.GS_LoginName, branchGuid, departmentGuid))
				{
					var taskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();

					AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
					Factory.Save();

					AssertEquals("User is allowed to edit template", 0, ExceptionReporterTestListener.Instance.Count);
				}
			}
		}

		public void TestTriggersDoNotSetActualDate_OnEventRaised()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			Assert("PreCheck", trigger.P9_ActualDate.IsEmpty);
			template.Logs.AddNew(Events.CustomisableEvent00);
			Assert("We should not be setting actual date on Templates.", trigger.P9_ActualDate.IsEmpty);
		}

		public void TestTriggersDoNotSetActualDate_Ever()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			Assert("PreCheck", trigger.P9_ActualDate.IsEmpty);
			trigger.SetMilestoneActualDateForTest(ZDateTime.Now);
			Assert("We should not be setting actual date on Templates.", trigger.P9_ActualDate.IsEmpty);
		}

		public void TestP0_IsScreenLayoutFallback_FormCustomization_ReadOnly()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			AssertEquals(false, template.FormCustomisationSettings.DisplayFields.ReadOnly);
			AssertEquals(false, template.FormCustomisationSettings.DisplayTabs.ReadOnly);
			template.P0_IsScreenLayoutFallback = true;
			AssertEquals(true, template.FormCustomisationSettings.DisplayFields.ReadOnly);
			AssertEquals(true, template.FormCustomisationSettings.DisplayTabs.ReadOnly);
		}

		public class InvalidDummyWorkflowProvider : DummyWithWorkflow, IWorkflowProvider
		{
			public InvalidDummyWorkflowProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString WorkflowType => new ZString("ZZZ");

			public IColumnValueRanker GetTemplateSelectionCriteria()
			{
				return new ColumnValueRanker();
			}

			public IWorkflowInformationProvider GetWorkflowInformationProvider()
			{
				throw null;
			}

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		public void TestInvalidWorkflowProviderErrorReport()
		{
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = "ZZZ";
			ProcessTask task = workflowTemplate.WorkflowItems.AddNew();
			Factory.Save();

			var dummy = Factory.New<InvalidDummyWorkflowProvider>();
			dummy.Z0_Description = "Hi";

			var dummyWithWorkflow = (IWorkflowProvider)dummy;
			Factory.Save();

			AssertEquals($"WorkflowProvider has no corresponding WorkflowDescriptor. WorkflowType: {dummy.WorkflowType}, Type: {dummy.GetType()}", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDeferFiringWorkflowInWorkflowTemplateLog()
		{
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var log = workflowTemplate.Logs.AddNew(new EventValue(Events.CustomisableEvent00, deferFiringWorkflow: true));

			Factory.Save();

			AssertNoExceptionThrown(() => MasterFilesTestHelper.RunLogWalker());
		}

		public void TestUsersWithoutActiveEditingPermissionCanEditInactiveTemplates()
		{
			using (new DisposableAction(() => Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false, () =>
			{
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
				ExceptionReporterTestListener.Instance.Clear();
			}))
			{
				var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
				template.P0_IsActive = false;
				Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);
				Assert("The active checkbox should be readonly when user without editing permissions opens an inactive template", loadedTemplate.P0_IsActiveInfo.ReadOnly);

				loadedTemplate.P0_Name = "Test Template";
				loadedTemplate.P0_Description = "I am a template that is being used in a test!";
				Factory.Save();

				AssertEquals("Users without template editing permissions can edit inactive workflow templates", 0, ExceptionReporterTestListener.Instance.Count);
			}
		}

		public void TestUserWithoutPermissionCanCopyAndCloneAnyTemplateAsInactive()
		{
			var securityCheckpointForCopy = (SecurityCheckpoint)EnvProxy.Instance.Security.FindOrCreateCopyCheckpoint(Env.Security.WorkflowTaskTemplates);
			securityCheckpointForCopy.IsAllowed = false;
			using (new DisposableAction(() => Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false, () =>
			{
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
				Env.Security.WorkflowTaskTemplatesNew.IsAllowed = true;
				securityCheckpointForCopy.IsAllowed = true;
				ExceptionReporterTestListener.Instance.Clear();
			}))
			{
				Env.Security.WorkflowTaskTemplatesNew.IsAllowed = false;
				var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
				template.WorkflowItems.AddNew();
				Factory.Save();

				var clonedTemplate = (ProcessTaskTemplate)template.Clone();
				Assert("The cloned template should not be active", !clonedTemplate.P0_IsActive);

				var copiedTemplate = (ProcessTaskTemplate)((ITemplateCopyable)ProcessTaskTemplate).TemplateCopy();
				Assert("The copied template should not be active", !copiedTemplate.P0_IsActive);
			}
		}

		[TestedType(typeof(ProcessTaskTemplate))]
		class ProcessTaskTemplateAuditParentTest : AuditParentTest<ProcessTaskTemplate>
		{
			protected override ProcessTaskTemplate NewTestAuditParent()
			{
				return Factory.New<ProcessTaskTemplate>();
			}
		}

		#region Delete

		[ExpectNoExceptions]
		public void TestCannotDeleteSystemTemplate()
		{
			ProcessTaskTemplate userDefinedTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			userDefinedTemplate.P0_IsSystem = false;
			ProcessTaskTemplate systemTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			systemTemplate.P0_IsSystem = true;

			userDefinedTemplate.Delete();
			try
			{
				systemTemplate.Delete();
				Fail("Expected an exception as a system-defined template cannot be deleted");
			}
			catch (CannotDeleteException)
			{
			}
		}

		#endregion

		#region Apply Templates From Multiple Companies

		void AssertGlobalTemplatesDuplicateTasks<T>(string processType)
			where T : class
		{
			var set1 = new StaffBranchDepartmentCompany(Factory);
			var set2 = new StaffBranchDepartmentCompany(Factory);

			DeactivateSystemTemplates(processType);

			var globalTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			globalTemplate.P0_ProcessType = processType;
			globalTemplate.GlobalTemplate = true;
			globalTemplate.P0_TaskFallbackMethod = FallbackTypeList.Codes.NeverFallback;
			var thatTask = globalTemplate.WorkflowItems.AddNew();
			thatTask.P9_Description = "Global";
			thatTask.P9_ShareTasksForAllCompanies = false;

			Factory.Save();
			IWorkflowProvider dummy = null;

			using (Env.SetTemporaryUserContext(set1.Staff.GS_LoginName, set1.Branch.PK.ToGuid(), set1.Department.PK.ToGuid()))
			{
				dummy = (IWorkflowProvider)Factory.New<T>();
				((BusinessObject)dummy).FillWithValidTestData();
				dummy.ApplyWorkflowTemplates();
				Factory.Save();
				AssertEquals(1, dummy.WorkflowItems.Count);
			}

			using (Env.SetTemporaryUserContext(set2.Staff.GS_LoginName, set2.Branch.PK.ToGuid(), set2.Department.PK.ToGuid()))
			{
				var newFactory = Factory.CreateNewFactory();
				var dummyReloaded = (IWorkflowProvider)newFactory.Load<T>(dummy.PK);
				dummyReloaded.ApplyWorkflowTemplates();
				AssertEquals(2, dummyReloaded.WorkflowItems.Count);
				newFactory.Save();
			}

			AssertEquals(2, Factory.CreateNewFactory().Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, dummy.PK)).Length);
		}

		static void DeactivateSystemTemplates(string templateCode)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(ProcessTaskTemplateSchema.P0_IsSystem, true);
			query.AddToFilter(ProcessTaskTemplateSchema.P0_ProcessType, templateCode);
			foreach (var template in factory.Load<ProcessTaskTemplate>(query))
			{
				template.P0_IsActive = false;
			}
			factory.Save();
		}

		class StaffBranchDepartmentCompany
		{
			public StaffBranchDepartmentCompany(BusinessObjectFactory factory)
			{
				Staff = factory.NewWithValidTestData<GlbStaff>();
				Company = factory.NewWithValidTestData<GlbCompany>();
				Branch = factory.NewWithValidTestData<GlbBranch>();
				Department = factory.NewWithValidTestData<GlbDepartment>();

				Branch.GB_GC = Company.PK;
				AllowedBranchDep = Branch.AllowedDepartments.AddNew();
				AllowedBranchDep.AAB_GB_Branch = Branch.PK;
				AllowedBranchDep.AAB_GE_Department = Department.PK;
			}

			public GlbBranch Branch { get; }
			public GlbDepartment Department { get; }
			public GlbCompany Company { get; }
			public GlbStaff Staff { get; }
			AccAllowedBranchDepartmentCombo AllowedBranchDep { get; }
		}

		public void TestApplyGlobalTemplate_FromDifferentCompanies_Shipments()
		{
			AssertGlobalTemplatesDuplicateTasks<IForwardingShipment>(WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
		}

		public void TestApplyGlobalTemplate_FromDifferentCompanies_OrgHeader()
		{
			AssertGlobalTemplatesDuplicateTasks<OrgHeader>(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
		}

		#endregion

		#region Implementation

		class ProcessTaskTemplateForTest : ProcessTaskTemplate
		{
			public ProcessTaskTemplateForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public Func<FormCustomisationSettings> GetNewFormCustomisationSettingsImplementation { get; set; }

			public bool CalledFormCustimisationsSettings { get; private set; }

			protected override FormCustomisationSettings GetNewFormCustomisationSettingsCore()
			{
				CalledFormCustimisationsSettings = true;
				return GetNewFormCustomisationSettingsImplementation != null ? GetNewFormCustomisationSettingsImplementation() : base.GetNewFormCustomisationSettingsCore();
			}
		}

		class FormCustomisationSettingsForTest : FormCustomisationSettings
		{
			public FormCustomisationSettingsForTest(ProcessTaskTemplate template)
				: base(template)
			{
			}

			public Action ResetImplementation { get; set; }
			protected override void ResetCore()
			{
				if (ResetImplementation != null)
				{
					ResetImplementation();
				}
				else
				{
					base.ResetCore();
				}
			}
		}

		ProcessTaskTemplate ProcessTaskTemplate
		{
			get { return processTaskTemplate ?? (processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>()); }
		}

		ProcessTaskTemplate processTaskTemplate;

		IDisposable nudgingControllerSubstitution;

		protected override void SetUp()
		{
			base.SetUp();
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			nudgingControllerSubstitution = ObjectFactory.Substitute(Mock.Of<INudgingController>());
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
		}

		protected override void TearDown()
		{
			nudgingControllerSubstitution?.Dispose();
			base.TearDown();
		}

		void AssertClearCriteriaIfRequired(bool hasClient, bool hasBranch, bool hasDepartment, bool hasPort1, bool hasPort2)
		{
			ProcessTaskTemplate.P0_ProcessType = "SHP";

			ZGuid client = ProcessTaskTemplate.P0_OH_Client = ZGuid.NewZGuid();
			ZGuid branch = ProcessTaskTemplate.P0_GB = ZGuid.NewZGuid();
			ZGuid department = ProcessTaskTemplate.P0_GE = ZGuid.NewZGuid();
			ProcessTaskTemplate.P0_LoadPortCountry = "abcd";
			ProcessTaskTemplate.P0_DischargePortCountry = "qwer";
			ProcessTaskTemplate.P0_SubType1 = "type1";
			ProcessTaskTemplate.P0_SubType3 = "type3";

			DummyWorkflowDescriptor.Instance.ClientNeeded = hasClient;
			DummyWorkflowDescriptor.Instance.BranchNeeded = hasBranch;
			DummyWorkflowDescriptor.Instance.DepartmentNeeded = hasDepartment;
			DummyWorkflowDescriptor.Instance.Port1Needed = hasPort1;
			DummyWorkflowDescriptor.Instance.Port2Needed = hasPort2;

			ProcessTaskTemplate.P0_ProcessType = "DUM";

			AssertEquals(hasClient ? client : ZGuid.Empty, ProcessTaskTemplate.P0_OH_Client);
			AssertEquals(hasBranch ? branch : ZGuid.Empty, ProcessTaskTemplate.P0_GB);
			AssertEquals(hasDepartment ? department : ZGuid.Empty, ProcessTaskTemplate.P0_GE);
			AssertEquals(hasPort1 ? new ZString("abcd") : ZString.Empty, ProcessTaskTemplate.P0_LoadPortCountry);
			AssertEquals(hasPort2 ? new ZString("qwer") : ZString.Empty, ProcessTaskTemplate.P0_DischargePortCountry);
			AssertEquals("type1", ProcessTaskTemplate.P0_SubType1);
			AssertEquals("type3", ProcessTaskTemplate.P0_SubType3);
		}

		internal IBMTestHelper BMSTestHelper
		{
			get { return ObjectFactory.Get<IBMTestHelper>(); }
		}

		#endregion
	}

	#endregion

	#region OnlySupportEventTrackingForUniversalTemplates

	[TestedType(typeof(ProcessTaskTemplate))]
	public class ProcessTaskTemplateWithEventTrackingByIsUniversal : EnterpriseBusinessObjectTestCase
	{
		public void TestOnlySupportEventTrackingForUniversalTemplates()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplateForTest>();
			AssertEquals(true, processTaskTemplate.WorkflowDescriptor.SupportsEventTracking);
			AssertEquals(true, processTaskTemplate.WorkflowDescriptor.OnlySupportEventTrackingForUniversalTemplates);

			AssertEquals(false, processTaskTemplate.P0_IsUniversal);
			AssertEquals("Milestone is not supported when P0_IsUniversal is false.", false, processTaskTemplate.SupportsTemplateEntityType(TemplateEntityType.Milestones).IsSupported);
			AssertEquals("Trigger is not supported when P0_IsUniversal is false.", false, processTaskTemplate.SupportsTemplateEntityType(TemplateEntityType.Triggers).IsSupported);

			processTaskTemplate.P0_IsUniversal = true;
			AssertEquals("Milestone is not supported when P0_IsUniversal is true.", false, processTaskTemplate.SupportsTemplateEntityType(TemplateEntityType.Milestones).IsSupported);
			AssertEquals("Trigger is supported when P0_IsUniversal is true.", true, processTaskTemplate.SupportsTemplateEntityType(TemplateEntityType.Triggers).IsSupported);

			processTaskTemplate.P0_IsUniversal = false;
			AssertEquals("Milestone is not supported when P0_IsUniversal is false.", false, processTaskTemplate.SupportsTemplateEntityType(TemplateEntityType.Milestones).IsSupported);
			AssertEquals("Trigger is not supported when P0_IsUniversal is false.", false, processTaskTemplate.SupportsTemplateEntityType(TemplateEntityType.Triggers).IsSupported);
		}

		public void TestSupportsTemplateEntityType_ValidationTool() => CombineAssertions(() =>
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			AssertValidationToolSupported(validationRulesSupported: true, isGlobalTemple: true, isAvailableForGlobalTemplates: true, expectedSupported: true);
			AssertValidationToolSupported(validationRulesSupported: true, isGlobalTemple: false, isAvailableForGlobalTemplates: true, expectedSupported: true);
			AssertValidationToolSupported(validationRulesSupported: true, isGlobalTemple: true, isAvailableForGlobalTemplates: false, expectedSupported: false);
			AssertValidationToolSupported(validationRulesSupported: true, isGlobalTemple: false, isAvailableForGlobalTemplates: false, expectedSupported: true);
			AssertValidationToolSupported(validationRulesSupported: false, isGlobalTemple: true, isAvailableForGlobalTemplates: true, expectedSupported: false);
			AssertValidationToolSupported(validationRulesSupported: false, isGlobalTemple: false, isAvailableForGlobalTemplates: true, expectedSupported: false);
			AssertValidationToolSupported(validationRulesSupported: false, isGlobalTemple: true, isAvailableForGlobalTemplates: false, expectedSupported: false);
			AssertValidationToolSupported(validationRulesSupported: false, isGlobalTemple: false, isAvailableForGlobalTemplates: false, expectedSupported: false);
			return;

			void AssertValidationToolSupported(bool validationRulesSupported, bool isGlobalTemple, bool isAvailableForGlobalTemplates, bool expectedSupported)
			{
				var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
				{
					ValidationRulesSupported = validationRulesSupported,
					IsValidationRulesAvailableForGlobalTemplatesForTest = isAvailableForGlobalTemplates
				};
				DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
				template.GlobalTemplate = isGlobalTemple;
				AssertEquals($"Validation Rules: {validationRulesSupported}; Is Available for Global Templates: {isAvailableForGlobalTemplates}; Global Template: {isGlobalTemple}", expectedSupported, template.SupportsTemplateEntityType(TemplateEntityType.ValidationTool).IsSupported);
			}
		});

		#region Implementation

		class ProcessTaskTemplateForTest : ProcessTaskTemplate
		{
			public ProcessTaskTemplateForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override WorkflowDescriptor WorkflowDescriptor
				=> workflowDescriptor ?? (workflowDescriptor = new DummyWorkflowDescriptorWithEventTrackingByIsUniversal());
			DummyWorkflowDescriptorWithEventTrackingByIsUniversal workflowDescriptor;
		}

		#endregion
	}

	class DummyWorkflowDescriptorWithEventTrackingByIsUniversal : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.DummyWorkflowDescriptorCode;
		public override IMultilingualString Description => (NoResString)"Dummy Task Provider";
		public override ControllerID ControllerID => DummyControllerIDs.Dummy;
		public override Type WorkflowProviderType => typeof(DummyWithWorkflow);

		public override bool SupportsEventTracking => true;
		public override bool OnlySupportEventTrackingForUniversalTemplates => true;
	}

	#endregion
}
