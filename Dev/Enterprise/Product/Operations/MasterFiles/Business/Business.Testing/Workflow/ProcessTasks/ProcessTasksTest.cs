using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using CargoWise.Workflow;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Workflow.Triggers;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.MasterFiles.Business.Testing
{
	#region ProcessTaskLoaderTest

	[TestedType(typeof(ProcessTask.Loader))]
	sealed class ProcessTaskLoaderTest : LoaderTestCase
	{
		public void TestGetFilterForCurrentCompany()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			var activeCompanies = GlbCompany.GetActiveCompanies();

			// Create test data
			foreach (var company in activeCompanies)
			{
				using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
				{
					ProcessTask task = dummy.WorkflowItems.Tasks.AddNew();
					task.P9_Description = company.GC_Code;

					ProcessTask milestone = dummy.WorkflowItems.Milestones.AddNew();
					milestone.P9_Description = company.GC_Code;

					ProcessTask trigger = dummy.WorkflowItems.Triggers.AddNew();
					trigger.P9_Description = company.GC_Code;

					ProcessTask exception = dummy.WorkflowItems.Exceptions.AddNew();
					exception.P9_Description = company.GC_Code;
				}
			}

			// Assert
			foreach (var company in activeCompanies)
			{
				using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
				{
					ZQuery query = new ZQuery(ProcessTasksSchema.P9_ParentID, dummy.PK);
					query.AddToFilter(ProcessTask.Loader.GetFilterForCurrentCompany());
					ProcessTask[] recordsFound = Factory.Load<ProcessTask>(query);
					AssertEquals("Should include milestones from ALL companies", activeCompanies.Length, Array.FindAll(recordsFound, (t) => t.IsMilestone).Length);
					AssertEquals("Should include exceptions from ALL companies", activeCompanies.Length, Array.FindAll(recordsFound, (t) => t.IsException).Length);
					AssertEquals("Should include triggers from ALL companies", activeCompanies.Length, Array.FindAll(recordsFound, (t) => t.IsWorkflowTrigger).Length);
					AssertEquals("Should only include task for the current company", 1, Array.FindAll(recordsFound, (t) => t.IsTask).Length);
					AssertEquals("Belongs to the current company", company.GC_Code, Array.FindAll(recordsFound, (t) => t.IsTask)[0].P9_Description);
				}
			}
		}

		#region TestCreateTasksAndMilestonesFromTemplateIfRequired

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_WhenNoTasksDefined()
		{
			WorkflowTemplate.Factory.Save();
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.IsMilestone = true;

			AssertEquals("1 milestone initially", 1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("No tasks initially", 0, Dummy.WorkflowItems.Tasks.Count);
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("1 pre-created milestone remains", 1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("1 task newly created from the template which occurs when no tasks are defined", 1, Dummy.WorkflowItems.Tasks.Count);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_WithSharedTasks()
		{
			WorkflowTemplate.WorkflowItems.Tasks.AddNew().P9_ShareTasksForAllCompanies = true;
			WorkflowTemplate.Factory.Save();

			AssertEquals("No tasks initially", 0, Dummy.WorkflowItems.Tasks.Count);

			var oneBranchPerCompany = GlbBranch.GetOneActiveBranchPerCompany();
			foreach (var branch in oneBranchPerCompany)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				}
			}

			var sharedTaskPK = Dummy.WorkflowItems.Tasks.Cast<ProcessTask>().First(t => t.P9_ShareTasksForAllCompanies).PK;

			foreach (var branch in oneBranchPerCompany)
			{
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					AssertEquals("1 company-specific task + 1 shared task", 2, Dummy.WorkflowItems.Tasks.Count);
					Assert("Shared task is really shared (has same PK)", Dummy.WorkflowItems.Tasks.Cast<ProcessTask>().Any(t => t.PK == sharedTaskPK));
				}
			}
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_WhenNoMilestonesDefined()
		{
			WorkflowDataRegistry.Instance.AlwaysApplyTasksFromTemplateWhenFirstSavingJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			WorkflowTemplate.Factory.Save();
			ProcessTask task = Dummy.WorkflowItems.Tasks.AddNew();

			AssertEquals("No milestones initially", 0, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("1 task initially", 1, Dummy.WorkflowItems.Tasks.Count);
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("1 milestone newly created from the template which occurs when no milestones are defined", 1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("1 pre-created task remains", 1, Dummy.WorkflowItems.Tasks.Count);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_WhenNewConditionHasBeenMet()
		{
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			ProcessTask milestone1 = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Dummy.WorkflowItems.WorkflowType;
			milestone1.P9_Description = "milestone1";
			ProcessTask milestone2 = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Dummy.WorkflowItems.WorkflowType;
			milestone2.P9_Description = "milestone2";
			milestone2.TemplateConditions.TemplateCondition1 = "XXX";
			ProcessTask trigger1 = WorkflowTemplate.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "trigger2";
			ProcessTask trigger2 = WorkflowTemplate.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "trigger2";
			trigger2.TemplateConditions.TemplateCondition1 = "XXX";
			WorkflowTemplate.Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.Factory.Save();

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("1 milestone created from template", 1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("1 trigger created from template", 1, Dummy.WorkflowItems.Triggers.Count);
			Dummy.Z0_Code = "XXX";
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("2nd milestone created because workflow condition changed", 2, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("2nd trigger created because workflow condition changed", 2, Dummy.WorkflowItems.Triggers.Count);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_SuppressTaskAndMilestoneCreation()
		{
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			var milestone1 = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Dummy.WorkflowItems.WorkflowType;
			milestone1.P9_Description = "milestone1";
			var milestone2 = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Dummy.WorkflowItems.WorkflowType;
			milestone2.P9_Description = "milestone2";
			milestone2.TemplateConditions.TemplateCondition1 = "XXX";
			var trigger1 = WorkflowTemplate.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "trigger2";
			var trigger2 = WorkflowTemplate.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "trigger2";
			trigger2.TemplateConditions.TemplateCondition1 = "XXX";
			WorkflowTemplate.Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.Factory.Save();

			Dummy.Z0_Code = "XXX";

			using (ProcessTask.Loader.SuppressTemplateApplication())
			{
				Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy);
				AssertEquals("Milestones not created because we're currently suppressing task creation", 0, Dummy.WorkflowItems.Milestones.Count);
				AssertEquals("Triggers not created because we're currently suppressing task creation", 0, Dummy.WorkflowItems.Triggers.Count);
			}
		}

		public void TestGetWorkflowTemplateApplicators_ShouldAddWorkflowLinkApplicator()
		{
			var templates = new List<ProcessTaskTemplate>();
			var parameters = TemplateApplicationParameters.ApplyIgnoreHasChanges();
			var workflowsParameter = TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.Workflows);
			var releaseGroupRulesParameter = TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.ReleaseGroupRules);
			var applicators = Loader.GetWorkflowTemplateApplicators(Dummy, Dummy, templates, parameters).ToList();
			Assert(applicators.Any(x => ObjectFactory.GetType<IWorkflowLinkTemplateApplicator>().IsInstanceOfType(x)));
			var workflowApplicators = Loader.GetWorkflowTemplateApplicators(Dummy, Dummy, templates, workflowsParameter).ToList();
			Assert(workflowApplicators.Any(x => ObjectFactory.GetType<IWorkflowLinkTemplateApplicator>().IsInstanceOfType(x)));
			var releaseGroupRulesApplicators = Loader.GetWorkflowTemplateApplicators(Dummy, Dummy, templates, releaseGroupRulesParameter).ToList();
			Assert(!releaseGroupRulesApplicators.Any(x => ObjectFactory.GetType<IWorkflowLinkTemplateApplicator>().IsInstanceOfType(x)));
		}

		public void TestCreateTriggersAndMilestonesFromTemplateIfExistingWithSPKContext()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_GC = GlbCompany.CurrentCompany.PK;
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			ProcessTask milestone1 = shipmentWorkflow.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			milestone1.TriggerConditions.TriggerCompany = GlbCompany.CurrentCompany.PK;
			milestone1.TriggerConditions.TriggerBranch = GlbBranch.CurrentBranch.PK;
			milestone1.TriggerConditions.TriggerDepartment = GlbDepartment.CurrentDepartment.PK;
			milestone1.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			milestone1.P9_Description = "M1";

			ProcessTask trigger1 = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			trigger1.TriggerConditions.TriggerCompany = GlbCompany.CurrentCompany.PK;
			trigger1.TriggerConditions.TriggerBranch = GlbBranch.CurrentBranch.PK;
			trigger1.TriggerConditions.TriggerDepartment = GlbDepartment.CurrentDepartment.PK;
			trigger1.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger1.P9_Description = "T1";

			AssertEquals("Should be only one milestone to begin with", 1, shipmentWorkflow.WorkflowItems.Milestones.Count);
			AssertEquals("Should be only one trigger to begin with", 1, shipmentWorkflow.WorkflowItems.Triggers.Count);

			Factory.Save();

			ProcessTask milestone2 = template.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Default;
			milestone2.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			milestone2.P9_Description = "M2";

			ProcessTask trigger2 = template.WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Default;
			trigger2.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			trigger2.P9_Description = "T2";

			Factory.Save();

			shipment.JS_HouseBill = "123";

			Factory.Save();

			AssertEquals("Should add a new milestone", 2, shipmentWorkflow.WorkflowItems.Milestones.Count);
			AssertEquals("Should add the correct milestone", "M2", shipmentWorkflow.WorkflowItems.Milestones[1].P9_Description);
			AssertEquals("Should add a new trigger", 2, shipmentWorkflow.WorkflowItems.Triggers.Count);
			AssertEquals("Should add the correct trigger", "T2", shipmentWorkflow.WorkflowItems.Triggers[1].P9_Description);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_WithUserDefinedConditions()
		{
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();

			ProcessTask milestone1 = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			milestone1.P9_Description = "M1";

			ProcessTask milestone2 = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			milestone2.P9_Description = "M2";
			milestone2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			milestone2.TemplateConditions.TemplateCondition2Value = "\"<Z0_Description>\" == \"XYZ\"";

			ProcessTaskNotification action = milestone2.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies;

			WorkflowTemplate.Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.Factory.Save();

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("1 milestone created from template", 1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("M1", Dummy.WorkflowItems.Milestones[0].P9_Description);

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("No new milestones created as user condition is not yet met", 1, Dummy.WorkflowItems.Milestones.Count);

			Dummy.Z0_Description = "XYZ";
			Factory.Save();

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("Second milestone is created from template as user condition is met now", 2, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("M1", Dummy.WorkflowItems.Milestones[0].P9_Description);
			AssertEquals("M2", Dummy.WorkflowItems.Milestones[1].P9_Description);

			AssertEquals("M2's trigger actions contain 1 element", 1, Dummy.WorkflowItems.Milestones[1].ProcessTaskNotifications.Count);
			AssertEquals("M2's trigger action is the same as in tmeplate", action.PQ_TriggerType, Dummy.WorkflowItems.Milestones[1].ProcessTaskNotifications[0].PQ_TriggerType);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_WithUserDefinedConditions_NoDuplicateTasks()
		{
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();

			var task1 = WorkflowTemplate.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Task 1";

			var taks2 = WorkflowTemplate.WorkflowItems.Tasks.AddNew();
			taks2.P9_Description = "Task 2";
			taks2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			taks2.TemplateConditions.TemplateCondition2Value = "\"<Z0_Description>\" == \"XYZ\"";

			WorkflowTemplate.Factory.Save();

			Dummy.WorkflowItems.RemoveAndDeleteAll();
			Dummy.Factory.Save();

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("1 taks created from template", 1, Dummy.WorkflowItems.Tasks.Count);
			AssertEquals("Task 1", Dummy.WorkflowItems.Tasks[0].P9_Description);

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("No new tasks created as user condition is not yet met", 1, Dummy.WorkflowItems.Tasks.Count);

			Dummy.Z0_Description = "XYZ";
			Factory.Save();

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("Second taks is created from template as user condition is met now", 2, Dummy.WorkflowItems.Tasks.Count);
			AssertEquals("Task 1", Dummy.WorkflowItems.Tasks[0].P9_Description);
			AssertEquals("Task 2", Dummy.WorkflowItems.Tasks[1].P9_Description);

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals("No duplicate UDF tasks created - should match existing one", 2, Dummy.WorkflowItems.Tasks.Count);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_MergeMilestonesWhenLoggedIntoAnotherCompany()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			WorkflowTemplate.P0_GC = ZGuid.Empty;
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			var milestoneTemplate = WorkflowTemplate.WorkflowItems.Milestones.AddNew();

			milestoneTemplate.Delete();
			milestoneTemplate = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestoneTemplate.P9_Description = "Arrived";
			milestoneTemplate.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			WorkflowTemplate.Factory.Save();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals("Arrival milestone created", 1, dummy.WorkflowItems.Milestones.Count);

			milestoneTemplate.Delete();
			milestoneTemplate = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestoneTemplate.P9_Description = "Departure";
			milestoneTemplate.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			WorkflowTemplate.Factory.Save();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals("Departure milestone (with different criteria) not created until logged into another company", 1, dummy.WorkflowItems.Milestones.Count);

			BranchInDifferentCompany.Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, BranchInDifferentCompany.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				Factory.Save();
				AssertEquals("Departure milestone created now that we've logged into another company", 2, dummy.WorkflowItems.Milestones.Count);

				milestoneTemplate.Delete();
				milestoneTemplate = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
				milestoneTemplate.P9_Description = "Another Milestone";
				milestoneTemplate.TriggerConditions.TriggerEventCode = Events.Booked.Code;
				WorkflowTemplate.Factory.Save();
				Factory.Save();
				Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				AssertEquals("New milestone not created as the job has been saved for the current company", 2, dummy.WorkflowItems.Milestones.Count);
			}
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_DetectWrongWorkflowUserContext()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			WorkflowTemplate.P0_GC = ZGuid.Empty;
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			var milestoneTemplate = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestoneTemplate.IsMilestone = true;

			milestoneTemplate.P9_Description = "Arrived";
			milestoneTemplate.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			WorkflowTemplate.Factory.Save();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals("Arrival milestone created", 1, dummy.WorkflowItems.Milestones.Count);

			milestoneTemplate.Delete();
			milestoneTemplate = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			milestoneTemplate.P9_Description = "Departure";
			milestoneTemplate.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			WorkflowTemplate.Factory.Save();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals("Departure milestone (with different criteria) not created until logged into another company", 1, dummy.WorkflowItems.Milestones.Count);

			BranchInDifferentCompany.Factory.Save();

			var contextSwitchLogger = new UserContextSwitchLogger();
			using (contextSwitchLogger != null ? Env.StartContextSwitchTrace(contextSwitchLogger) : null)
			using (Factory.ServiceContainer.AddService(new WorkflowUserContextManager()).SetWorkflowUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, BranchInDifferentCompany.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()), contextSwitchLogger))
			{
				// Although we are not switching to the correct user context, the workflow context should be applied and an error reported
				Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				Factory.Save();
				AssertEquals("Departure milestone created now that we've logged into another company", 2, dummy.WorkflowItems.Milestones.Count);

				milestoneTemplate.Delete();
				milestoneTemplate = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
				milestoneTemplate.P9_Description = "Another Milestone";
				milestoneTemplate.TriggerConditions.TriggerEventCode = Events.Booked.Code;
				WorkflowTemplate.Factory.Save();
				Factory.Save();
				Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				AssertEquals("New milestone not created as the job has been saved for the current company", 2, dummy.WorkflowItems.Milestones.Count);
				AssertEquals("Wrong workflow user context should have been detected", 1, ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();
			}
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_MergeTriggersWhenLoggedIntoAnotherCompany()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			WorkflowTemplate.P0_GC = ZGuid.Empty;
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			var triggerTemplate = WorkflowTemplate.WorkflowItems.Triggers.AddNew();

			triggerTemplate.P9_Description = "Arrived";
			triggerTemplate.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			WorkflowTemplate.Factory.Save();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals("Arrival trigger created", 1, dummy.WorkflowItems.Triggers.Count);

			triggerTemplate.Delete();
			triggerTemplate = WorkflowTemplate.WorkflowItems.Triggers.AddNew();
			triggerTemplate.P9_Description = "Departure";
			triggerTemplate.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			WorkflowTemplate.Factory.Save();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals("Departure trigger (with different criteria) not created until logged into another company", 1, dummy.WorkflowItems.Triggers.Count);

			BranchInDifferentCompany.Factory.Save();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, BranchInDifferentCompany.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				Factory.Save();
				AssertEquals("Departure trigger created now that we've logged into another company", 2, dummy.WorkflowItems.Triggers.Count);

				triggerTemplate.Delete();
				triggerTemplate = WorkflowTemplate.WorkflowItems.Triggers.AddNew();
				triggerTemplate.P9_Description = "Another Trigger";
				triggerTemplate.TriggerConditions.TriggerEventCode = Events.Booked.Code;
				WorkflowTemplate.Factory.Save();
				Factory.Save();
				Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				AssertEquals("New trigger not created as the job has been saved for the current company", 2, dummy.WorkflowItems.Triggers.Count);
			}
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_ShouldMergeTriggersWhenCondition2ValueDiffersInAnotherCompany()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			WorkflowTemplate.P0_GC = ZGuid.Empty;
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			ProcessTask triggerTemplate1 = WorkflowTemplate.WorkflowItems.AddNew();
			triggerTemplate1.IsWorkflowTrigger = true;

			triggerTemplate1.P9_Description = "Arrived";
			triggerTemplate1.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			triggerTemplate1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			triggerTemplate1.TemplateConditions.TemplateCondition2Value = "\"<CompanyCode>\" == \"" + GlbCompany.CurrentCompany.GC_Code + "\"";

			ProcessTask triggerTemplate2 = WorkflowTemplate.WorkflowItems.AddNew();
			triggerTemplate2.IsWorkflowTrigger = true;
			triggerTemplate2.P9_Description = "Arrived";
			triggerTemplate2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			triggerTemplate2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			triggerTemplate2.TemplateConditions.TemplateCondition2Value = "\"<CompanyCode>\" == \"XXX\"";
			WorkflowTemplate.Factory.Save();

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals("Arrival trigger (with criteria <CompanyCode == XXX>) not created until logged into another company which matches Condition 2", 1, dummy.WorkflowItems.Triggers.Count);

			BranchInDifferentCompany.Factory.Save();
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, BranchInDifferentCompany.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				Factory.Save();
				AssertEquals("Another Arrival trigger created now that we've logged into another company", 2, dummy.WorkflowItems.Triggers.Count);
			}
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_ShouldMergeTriggersWhenCondition2ValueDiffersInSameCompany()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			WorkflowTemplate.P0_GC = ZGuid.Empty;
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			ProcessTask triggerTemplate1 = WorkflowTemplate.WorkflowItems.AddNew();
			triggerTemplate1.IsWorkflowTrigger = true;

			triggerTemplate1.P9_Description = "Arrived";
			triggerTemplate1.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			triggerTemplate1.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			triggerTemplate1.TemplateConditions.TemplateCondition2Value = "\"<CompanyCode>\" != \"...\"";

			ProcessTask triggerTemplate2 = WorkflowTemplate.WorkflowItems.AddNew();
			triggerTemplate2.IsWorkflowTrigger = true;
			triggerTemplate2.P9_Description = "Arrived";
			triggerTemplate2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			triggerTemplate2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			triggerTemplate2.TemplateConditions.TemplateCondition2Value = "\"<CompanyCode>\" != \";;;\"";
			WorkflowTemplate.Factory.Save();

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();

			AssertEquals("2 Arrival triggers (with different criteria in codition2value) have been created bacause both their Condition2 are matched ", 2, dummy.WorkflowItems.Triggers.Count);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_UseFallbacks()
		{
			ProcessTaskTemplate activeTemplate1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate1.P0_ProcessType = "DUM";
			activeTemplate1.P0_IsSystem = true;
			ProcessTask milestoneA = activeTemplate1.WorkflowItems.AddNew();
			milestoneA.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestoneA.P9_Description = "TEMPLATE 1";
			ProcessTask taskA = activeTemplate1.WorkflowItems.AddNew();
			taskA.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			taskA.P9_Description = "TEMPLATE 1";
			ProcessTask triggerA = activeTemplate1.WorkflowItems.AddNew();
			triggerA.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			triggerA.P9_Description = "TEMPLATE 1";

			ProcessTaskTemplate activeTemplate2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			activeTemplate2.P0_ProcessType = "DUM";
			activeTemplate2.P0_IsSystem = false;
			ProcessTask milestoneB = activeTemplate2.WorkflowItems.AddNew();
			milestoneB.P9_Type = Core.Constants.Workflow.MilestoneType;
			milestoneB.P9_Description = "TEMPLATE 2";

			Factory.Save();

			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals(1, dummy.WorkflowItems.Tasks.Count);
			AssertEquals("TEMPLATE 1", dummy.WorkflowItems.Tasks[0].P9_Description);
			AssertEquals(1, dummy.WorkflowItems.Milestones.Count);
			AssertEquals("TEMPLATE 2", dummy.WorkflowItems.Milestones[0].P9_Description);

			ProcessTask taskB = activeTemplate2.WorkflowItems.AddNew();
			taskB.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			taskB.P9_Description = "TEMPLATE 2";
			ProcessTask triggerB = activeTemplate2.WorkflowItems.AddNew();
			triggerB.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			triggerB.P9_Description = "TEMPLATE 2";
			activeTemplate2.WorkflowItems.Remove(milestoneB);

			Factory.Save();

			DummyWithWorkflow anotherDummy = Factory.New<DummyWithWorkflow>();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(anotherDummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals(1, anotherDummy.WorkflowItems.Tasks.Count);
			AssertEquals("TEMPLATE 2", anotherDummy.WorkflowItems.Tasks[0].P9_Description);
			AssertEquals(1, anotherDummy.WorkflowItems.Milestones.Count);
			AssertEquals("TEMPLATE 1", anotherDummy.WorkflowItems.Milestones[0].P9_Description);
			AssertEquals(1, anotherDummy.WorkflowItems.Triggers.Count);
			AssertEquals("TEMPLATE 2", anotherDummy.WorkflowItems.Triggers[0].P9_Description);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_MilestonesAndTriggersWithSameEventButDifferentReferences()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			ProcessTask milestone1 = template.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.Authorised.Code;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone1.TriggerConditions.TriggerConditionValue = "milestone ref 1";
			ProcessTask milestone2 = template.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = Events.Authorised.Code;
			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone2.TriggerConditions.TriggerConditionValue = "milestone ref 2";
			ProcessTask milestone3 = template.WorkflowItems.Milestones.AddNew();
			milestone3.TriggerConditions.TriggerEventCode = Events.Dehire.Code;
			milestone3.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone3.TriggerConditions.TriggerConditionValue = "milestone ref 2";

			ProcessTask trigger1 = template.WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			trigger1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger1.TriggerConditions.TriggerConditionValue = "trigger ref 1";
			ProcessTask trigger2 = template.WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			trigger2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger2.TriggerConditions.TriggerConditionValue = "trigger ref 2";
			ProcessTask trigger3 = template.WorkflowItems.Triggers.AddNew();
			trigger3.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			trigger3.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger3.TriggerConditions.TriggerConditionValue = "trigger ref 2";
			Factory.Save();

			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			CombineAssertions(delegate
			{
				AssertEquals(5, dummy.WorkflowItems.Count);

				AssertEquals(3, dummy.WorkflowItems.Milestones.Count);
				AssertEquals(Events.Authorised.Code, dummy.WorkflowItems.Milestones[0].P9_SE_NKMilestoneEvent);
				AssertEquals(false, dummy.WorkflowItems.Milestones[0].P9_TriggerCondition.IsEmpty);
				AssertEquals("milestone ref 1", dummy.WorkflowItems.Milestones[0].P9_TriggerConditionValue);
				AssertEquals(Events.Authorised.Code, dummy.WorkflowItems.Milestones[1].P9_SE_NKMilestoneEvent);
				AssertEquals(false, dummy.WorkflowItems.Milestones[1].P9_TriggerCondition.IsEmpty);
				AssertEquals("milestone ref 2", dummy.WorkflowItems.Milestones[1].P9_TriggerConditionValue);
				AssertEquals(Events.Dehire.Code, dummy.WorkflowItems.Milestones[2].P9_SE_NKMilestoneEvent);
				AssertEquals(false, dummy.WorkflowItems.Milestones[2].P9_TriggerCondition.IsEmpty);
				AssertEquals("milestone ref 2", dummy.WorkflowItems.Milestones[2].P9_TriggerConditionValue);

				AssertEquals(2, dummy.WorkflowItems.Triggers.Count);
				AssertEquals(Events.Arrival.Code, dummy.WorkflowItems.Triggers[0].P9_SE_NKMilestoneEvent);
				AssertEquals(false, dummy.WorkflowItems.Triggers[0].P9_TriggerCondition.IsEmpty);
				AssertEquals("trigger ref 1", dummy.WorkflowItems.Triggers[0].P9_TriggerConditionValue);
				AssertEquals(Events.Arrival.Code, dummy.WorkflowItems.Triggers[1].P9_SE_NKMilestoneEvent);
				AssertEquals(false, dummy.WorkflowItems.Triggers[1].P9_TriggerCondition.IsEmpty);
				AssertEquals("trigger ref 2", dummy.WorkflowItems.Triggers[1].P9_TriggerConditionValue);
			});
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_ExceptionCodes()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = Dummy.WorkflowItems.WorkflowType;

			ProcessTask milestoneWithoutException = template.WorkflowItems.Milestones.AddNew();
			milestoneWithoutException.P9_SE_NKExceptionEvent = "";
			milestoneWithoutException.TriggerConditions.TriggerEventCode = Events.Delivered.Code;

			ProcessTask milestoneWithDefaultException = template.WorkflowItems.Milestones.AddNew();
			milestoneWithDefaultException.TriggerConditions.TriggerEventCode = Events.DeliveryOrderReceived.Code;

			ProcessTask milestoneWithException = template.WorkflowItems.Milestones.AddNew();
			milestoneWithException.TriggerConditions.TriggerEventCode = Events.PickedUp.Code;
			milestoneWithException.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
			Factory.Save();

			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			AssertEquals(3, dummy.WorkflowItems.Milestones.Count);
			AssertEquals("", dummy.WorkflowItems.Milestones[0].P9_SE_NKExceptionEvent);
			AssertEquals(ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily, dummy.WorkflowItems.Milestones[1].P9_SE_NKExceptionEvent);
			AssertEquals(ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired, dummy.WorkflowItems.Milestones[2].P9_SE_NKExceptionEvent);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_ShouldNotKeepMergingTriggerActions()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.GlobalTemplate = true;
			template.P0_ProcessType = Dummy.WorkflowItems.WorkflowType;
			ProcessTask trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.PickedUp.Code;
			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "XML";
			action.PQ_TriggerParty = "BTP";
			Factory.Save();

			DummyWithWorkflow dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
			AssertEquals(1, dummy.WorkflowItems.Triggers[0].ProcessTaskNotifications.Count);

			using (DisposableEnvironment.ForBranchCodeSlowerThanPK("DEM"))
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				DummyWithWorkflow loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
				loadedDummy.Z0_AnotherDate = ZDateTime.Now;
				new ProcessTask.Loader(newFactory).CreateTasksAndMilestonesFromTemplateIfRequired(loadedDummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
				Factory.Save();
				AssertEquals(1, loadedDummy.WorkflowItems.Triggers.Count);
				AssertEquals("Should be matched and should not add another trigger action", 1, loadedDummy.WorkflowItems.Triggers[0].ProcessTaskNotifications.Count);
			}
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_ShouldNotAddNewIfCannotMergeForDifferentComapnies()
		{
			GlbCompany otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.GlobalTemplate = true;
			template.P0_ProcessType = Dummy.WorkflowItems.WorkflowType;

			ProcessTask trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;
			trigger.P9_Description = "Test 1";
			trigger.P9_Sequence = 1;

			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "NTF";
			action.PQ_TriggerParty = "EML";
			action.PQ_EmailText = "incident dispelled";
			action.PQ_EmailAddr = "anton.gorlin@cargowise.com";
			Factory.Save();

			DummyWithWorkflow dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();

			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
			dummy.WorkflowItems.Triggers[0].P9_GC = otherCompany.PK;
			Factory.Save();

			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_ShouldNotMergeTriggerActionsIfEmailTextNotEmpty()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.GlobalTemplate = true;
			template.P0_ProcessType = Dummy.WorkflowItems.WorkflowType;

			ProcessTask trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;
			trigger.P9_Description = "Test 1";
			trigger.P9_Sequence = 1;

			ProcessTaskNotification action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "NTF";
			action.PQ_TriggerParty = "EML";
			action.PQ_EmailText = "test text 1 (one)";
			action.PQ_EmailAddr = "alexander.korotun@cargowise.com";

			ProcessTask anotherTrigger = template.WorkflowItems.Triggers.AddNew();
			anotherTrigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;
			anotherTrigger.P9_Description = "Test 1";
			anotherTrigger.P9_Sequence = 2;
			ProcessTaskNotification anotherAction = anotherTrigger.ProcessTaskNotifications.AddNew();
			anotherAction.PQ_TriggerType = "NTF";
			anotherAction.PQ_TriggerParty = "EML";
			anotherAction.PQ_EmailText = "test text 2 (two)";
			anotherAction.PQ_EmailAddr = "simon.qu@cargowise.com";

			ProcessTaskNotification yetAnotherAction = anotherTrigger.ProcessTaskNotifications.AddNew();
			yetAnotherAction.PQ_TriggerType = "NTF";
			yetAnotherAction.PQ_TriggerParty = "EML";
			yetAnotherAction.PQ_EmailText = "test text 3 (three)";
			yetAnotherAction.PQ_EmailAddr = "anton.gorlin@cargowise.com";

			Factory.Save();

			DummyWithWorkflow dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());
			Factory.Save();
			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
			AssertEquals(3, dummy.WorkflowItems.Triggers[0].ProcessTaskNotifications.Count);

			AssertEquals("Email text of first trigger notification", "test text 1 (one)", dummy.WorkflowItems.Triggers[0].ProcessTaskNotifications[0].PQ_EmailTextFallbackToTemplate);
			AssertEquals("Email text of second trigger notification", "test text 2 (two)", dummy.WorkflowItems.Triggers[0].ProcessTaskNotifications[1].PQ_EmailTextFallbackToTemplate);
			AssertEquals("Email text of second trigger notification", "test text 3 (three)", dummy.WorkflowItems.Triggers[0].ProcessTaskNotifications[2].PQ_EmailTextFallbackToTemplate);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_SourceTemplateProperties()
		{
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.RemoveAndDeleteAll();

			var templateTask = WorkflowTemplate.WorkflowItems.Tasks.AddNew();
			var templateMilestone = WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			var templateTrigger = WorkflowTemplate.WorkflowItems.Triggers.AddNew();
			WorkflowTemplate.Factory.Save();

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());

			AssertEquals("1 task created from template", 1, Dummy.WorkflowItems.Tasks.Count);
			AssertEquals("1 milestone created template", 1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals("1 trigger created template", 1, Dummy.WorkflowItems.Triggers.Count);

			var workflowTask = Dummy.WorkflowItems.Tasks[0];
			var workflowMilestone = Dummy.WorkflowItems.Milestones[0];
			var workflowTrigger = Dummy.WorkflowItems.Triggers[0];

			AssertEquals("Task's ParentTemplateID matches template task", templateTask.PK, workflowTask.P9_ParentTemplateID);
			AssertEquals("Milestone's ParentTemplateID matches template milestone", templateMilestone.PK, workflowMilestone.P9_ParentTemplateID);
			AssertEquals("Trigger's ParentTemplateID matches template trigger", templateTrigger.PK, workflowTrigger.P9_ParentTemplateID);

			AssertEquals("Task's SourceTemplatePK matches workflow template", WorkflowTemplate.PK, workflowTask.SourceTemplatePK);
			AssertEquals("Milestone's SourceTemplatePK matches workflow template", WorkflowTemplate.PK, workflowMilestone.SourceTemplatePK);
			AssertEquals("Trigger's SourceTemplatePK matches workflow template", WorkflowTemplate.PK, workflowTrigger.SourceTemplatePK);

			AssertEquals("Task's SourceTemplateName matches workflow template", WorkflowTemplate.P0_Name, workflowTask.SourceTemplateName);
			AssertEquals("Milestone's SourceTemplateName matches workflow template", WorkflowTemplate.P0_Name, workflowMilestone.SourceTemplateName);
			AssertEquals("Trigger's SourceTemplateName matches workflow template", WorkflowTemplate.P0_Name, workflowTrigger.SourceTemplateName);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_OnDeleteErrorReporterThreadLocal()
		{
			ProcessTask milestone = Dummy.WorkflowItems.AddNew();

			var threadCount = 5;

			List<Thread> threads = new List<Thread>();

			int[] onDeleteActionList = new int[threadCount];

			for (int i = 0; i < threadCount; ++i)
			{
				var thread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						ProcessTask.OnDeleteAction.Value = (processTask) =>
						{
							onDeleteActionList[i] = i;
						};
						ProcessTask.OnDeleteAction.Value.Invoke(milestone);
					}
				});
				thread.Start();
				thread.Join();
			}

			AssertEquals(threadCount, onDeleteActionList.Length);

			AssertArrayEqualsByElements(new[] { 0, 1, 2, 3, 4 }, onDeleteActionList);

			ErrorReporter.Clear();
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_OnDeleteErrorReporter()
		{
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			Dummy.WorkflowItems.RemoveAndDeleteAll();

			ProcessTask milestone = Dummy.WorkflowItems.AddNew();
			milestone.IsMilestone = true;

			ProcessTask.SetOnCreateTasksAndMilestonesFromTemplateIfRequiredForTest(() =>
			{
				milestone.Delete();
			});

			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(Dummy);

			AssertContains("A Process Task was deleted during CreateTasksAndMilestonesFromTemplateIfRequired().", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_UseParametersFromSometimesWorkflowProvider()
		{
			WorkflowTemplate.WorkflowItems.RemoveAndDeleteAll();
			WorkflowTemplate.WorkflowItems.Milestones.AddNew();
			WorkflowTemplate.WorkflowItems.Triggers.AddNew();
			WorkflowTemplate.WorkflowItems.Tasks.AddNew();
			WorkflowTemplate.Factory.Save();

			var dummyProvider = Factory.New<DummyWorkflowTemplateParameterProvider>();
			dummyProvider.WorkflowItems.RemoveAndDeleteAll();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummyProvider, TemplateApplicationParameters.ApplyIgnoreHasChanges());

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: create 1 milestone when template application parameter is null", 1, dummyProvider.WorkflowItems.Milestones.Count);
				AssertEquals("Precondition: create 1 trigger when template application parameter is null", 1, dummyProvider.WorkflowItems.Triggers.Count);
				AssertEquals("Precondition: create 1 task when template application parameter is null", 1, dummyProvider.WorkflowItems.Tasks.Count);
			});

			dummyProvider.SetTemplateApplicationParameters(TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.Triggers));
			dummyProvider.WorkflowItems.RemoveAndDeleteAll();
			Loader.CreateTasksAndMilestonesFromTemplateIfRequired(dummyProvider, TemplateApplicationParameters.ApplyIgnoreHasChanges());

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Doesn't create milestone when template application parameter is TemplateEntityType.Triggers", 0, dummyProvider.WorkflowItems.Milestones.Count);
				AssertEquals("Only create trigger when template application parameter is TemplateEntityType.Triggers", 1, dummyProvider.WorkflowItems.Triggers.Count);
				AssertEquals("Doesn't create task when template application parameter is TemplateEntityType.Triggers", 0, dummyProvider.WorkflowItems.Tasks.Count);
			});
		}

		class DummyWorkflowTemplateParameterProvider : DummyWithWorkflow, IWorkflowTemplateParameterProvider
		{
			public DummyWorkflowTemplateParameterProvider(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public TemplateApplicationParameters TemplateApplicationParameters => templateApplicationParametersForTest;

			TemplateApplicationParameters templateApplicationParametersForTest;

			public void SetTemplateApplicationParameters(TemplateApplicationParameters templateApplicationParameters)
			{
				templateApplicationParametersForTest = templateApplicationParameters;
			}
		}

		#endregion

		#region Cancel and old Add Milestone

		public void TestCancelAndOldAddMilestone()
		{
			DummyWithWorkflowAndTemplateLoader dummy1 = new BusinessObjectFactory { RefreshEnabled = false }.NewWithValidTestData<DummyWithWorkflowAndTemplateLoader>();
			dummy1.Factory.Save();

			ZQuery query = new ZQuery(ProcessTasksSchema.P9_ParentID, dummy1.PK);
			query.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);
			query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, Events.AddedARecordToTheSystemCode);

			ProcessTask[] tasks = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTask>(query);
			Assert("No workflow tasks should be registered on bizo yet", tasks == null || tasks.Length == 0);

			ProcessTaskTemplate template = new BusinessObjectFactory { RefreshEnabled = false }.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = dummy1.WorkflowItems.WorkflowType;

			ProcessTask milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;

			template.Factory.Save();

			tasks = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTask>(query);
			Assert("No workflow tasks should be registered on bizo yet", tasks == null || tasks.Length == 0);

			DummyWithWorkflowAndTemplateLoader dummy2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyWithWorkflowAndTemplateLoader>(dummy1.PK);
			((ICancellable)dummy2).IsCancelled = true;
			dummy2.Factory.Save();

			tasks = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTask>(query);
			Assert("No workflow tasks should be registered on bizo yet", tasks == null || tasks.Length == 0);

			DummyWithWorkflowAndTemplateLoader dummy3 = new BusinessObjectFactory { RefreshEnabled = false }.Load<DummyWithWorkflowAndTemplateLoader>(dummy1.PK);
			((ICancellable)dummy3).IsCancelled = false;
			dummy3.Factory.Save();

			tasks = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTask>(query);
			AssertNotNull(tasks);
			AssertEquals(1, tasks.Length);
			AssertEquals(Core.Constants.Workflow.MilestoneType, tasks[0].P9_Type);
			AssertEquals(Events.AddedARecordToTheSystemCode, tasks[0].P9_SE_NKMilestoneEvent);
		}

		class DummyWithWorkflowAndTemplateLoader : DummyWithWorkflow, ICancellable
		{
			public DummyWithWorkflowAndTemplateLoader(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override void OnFactorySavingBeforeTransactionCore()
			{
				base.OnFactorySavingBeforeTransactionCore();
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}

			bool ICancellable.IsCancelled
			{
				get { return IsCancelled; }
				set
				{
					IsCancelled = value;
					isCancelledHasChanged = true;
					Z0_Bool = value;
				}
			}

			bool ICancellable.IsCancelledHasChanged
			{
				get { return isCancelledHasChanged || IsCancelledHasChanged; }
			}
			bool isCancelledHasChanged;
		}

		#endregion

		#region Implementation

		GlbCompany DifferentCompany
		{
			get
			{
				if (differentCompany == null)
				{
					differentCompany = Factory.New<GlbCompany>();
					differentCompany.GC_Code = "XXX";
				}
				return differentCompany;
			}
		}
		GlbCompany differentCompany;

		GlbBranch BranchInDifferentCompany
		{
			get
			{
				if (branchInDifferentCompany == null)
				{
					branchInDifferentCompany = DifferentCompany.Branches.AddNew();
					branchInDifferentCompany.GB_Code = "XXX";
				}
				return branchInDifferentCompany;
			}
		}
		GlbBranch branchInDifferentCompany;

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new ProcessTask.Loader(Factory);
		}

		ProcessTask.Loader Loader
		{
			get
			{
				if (loader == null)
				{
					loader = (ProcessTask.Loader)GetNewLoaderToTest();
				}
				return loader;
			}
		}
		ProcessTask.Loader loader;

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;

		ProcessTaskTemplate WorkflowTemplate
		{
			get
			{
				if (workflowTemplate == null)
				{
					BusinessObjectFactory taskTemplateFactory = new BusinessObjectFactory();
					workflowTemplate = taskTemplateFactory.NewWithValidTestData<ProcessTaskTemplate>();
					workflowTemplate.P0_ProcessType = Dummy.WorkflowItems.WorkflowType;

					ProcessTask task = workflowTemplate.WorkflowItems.AddNew();
					ProcessTask milestone = workflowTemplate.WorkflowItems.AddNew();
					milestone.IsMilestone = true;
				}
				return workflowTemplate;
			}
		}
		ProcessTaskTemplate workflowTemplate;

		protected override void SetUp()
		{
			base.SetUp();

			MasterFilesTestHelper.ClearWorkflowTables();
		}

		#endregion
	}

	#endregion

	[TestedType(typeof(DummyProcessTask))]
	public class ProcessTaskTest : EnterpriseBusinessObjectTestCase
	{
		CategorisedWorkflowTaskTypesCollection originalTaskTypes;

		protected override void SetUp()
		{
			base.SetUp();
			originalTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected override void TearDown()
		{
			base.TearDown();
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalTaskTypes);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Dummy.WorkflowItems.AddNew();
		}

		public virtual void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			ProcessTask task = GetNewBusinessObject() as ProcessTask;
			AssertNotNull("GetNewBusinessObject() should return valid ProcessTask", task);
			AssertNotNull("ParentControllerID should be overriden to return correct ID for none standalone task, or override this test to assert true", task.ParentControllerID);
		}

		public void TestLoadingProcessTaskChildAsProcessTaskLoadsCorrectType()
		{
			var expectedType = GetExpectedBusinessObjectType();
			if (!ExcludedDodgyProcessTasksWhichShouldBeFixedEventually.Contains(expectedType.Name))
			{
				var processTask = GetNewBusinessObject();
				((ProcessTask)processTask).ParentBusinessObject.FillWithValidTestData();
				processTask.Factory.Save();

				var otherFactory = new BusinessObjectFactory();
				var processTaskInOtherFactory = otherFactory.Load<ProcessTask>(processTask.PK);
				var assertionMessage = @"This failure generally means you haven't mapped your ProcessTasks Subclass in the ProcessTaskTypesConfiguration.xml file.
Open this file and check the 'ProcessTaskTypes' section, which is used by the ProcessTasks TypeDecider to load the Correct Type and map your ProcessTasks Subclass appropriately.
Otherwise perhaps your Type should actually be abstract as well as its TestCase if it is subclassed further.";
				AssertType(assertionMessage, expectedType, processTaskInOtherFactory);
			}
			else
			{
				// Whoever owns these ProcessTasks subclasses should fix up the Application Config or revisit whether this ProcessTask should be instantiated.
				Assert(true);
			}
		}

		public void TestEventLogDeletedBeforeTriggerFired()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.GlobalTemplate = true;
			template.P0_ProcessType = "SHP";

			var addOnRule = Factory.New<GenCustomAddOnRule>();
			#region SourceCode
			addOnRule.XR_SourceCode =
	@"<sourceCode>
		<rules>
			<rule code=""InvalidCode"" enabled=""false"">
				<details>
					<codeDescriptionList/>
				</details>
			</rule>
			<rule code=""CreateEvent"" enabled=""true"">
				<details>
					<CreateEventRuleCode>Z10</CreateEventRuleCode>
				</details>
			</rule>
			<rule code=""DateTimeFormat"" enabled=""true"">
				<details>
					<format>Long</format>
				</details>
			</rule>
			<rule code=""CheckEntered"" enabled=""false"">
				<details/>
			</rule>
		</rules>
	</sourceCode>";
			#endregion

			var def = template.GenCustomColumnDefinitions.AddNew();
			def.XC_Name = "Custom Field";
			def.XC_Type = AddOnColumnDataType.Codes.Datetime;
			def.XC_XR = addOnRule.PK;

			ProcessTask CreateTrigger(ZString eventCode, int sequence)
			{
				var trigger = template.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = eventCode;
				trigger.P9_Description = $"trigger {sequence}";
				((ITriggerConditions)trigger).TriggerFiredCountdown = 100;
				trigger.P9_Sequence = sequence;

				return trigger;
			}

			void AttachIFCAction(ProcessTask trigger, string fieldName, string fieldValue)
			{
				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
				action.PQ_FieldName = fieldName;
				action.PQ_FieldValue = fieldValue;
			}

			var trigger1 = CreateTrigger(Events.CustomisableEvent00.Code, 1);
			AttachIFCAction(trigger1, "<GetCustomField(Custom Field)>", "<JS_E_DEP>");

			var trigger2 = CreateTrigger(Events.CustomisableEvent10.Code, 2);
			AttachIFCAction(trigger2, "<JS_E_DEP>", "<GetCustomField(Custom Field)>");
			AttachIFCAction(trigger2, "<GetCustomField(Custom Field)>", "");

			var trigger3 = CreateTrigger(Events.CustomisableEvent10.Code, 3);
			Factory.Save();

			var parentBO = Factory.New<Forwarding.IForwardingShipment>();
			var parentBOWithWorkflow = (IWorkflowProvider)parentBO;

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(parentBOWithWorkflow);
			Factory.Save();

			var shipment = Factory.Load<Forwarding.IForwardingShipment>(parentBO.PK);

			shipment.JS_E_DEP = ZDateTime.Now;
			((IWorkflowProvider)shipment).Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(ProcessTasksSchema.P9_ParentID, shipment.PK);
			var triggers = Factory.Load<ProcessTask>(query);

			var expectedTriggerCountdown = new Dictionary<int, ZShort>() { { 1, 99 }, { 2, 100 }, { 3, 100 } };
			Assert("Last trigger should not be triggerd", triggers.All(c => c.P9_TriggerFiredCountdown.Equals(expectedTriggerCountdown[c.P9_Sequence])));
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		string[] ExcludedDodgyProcessTasksWhichShouldBeFixedEventually => new[]
		{
			"BaseJobComInvoiceHeaderProcessTask",
			"AgencyContainerProcessTask",
			"AgencyShipmentProcessTask",
			"GlbCompanyCampaignItemProcessTask",
			"CRMProcessTask",
			"GlbGroupProcessTask",
			"OrgPartRelationProcessTask",
			"TrackingOrderProcessTasks",
			"TrackingShipmentProcessTask"
		};

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}

		DummyWithWorkflow dummy;
	}

	/// <summary>
	/// The tests that don't need to be run against every derivative ProcessTask Type.
	/// </summary>
	[TestedType(typeof(ProcessTask))]
	sealed class ProcessTaskCoreTest : EnterpriseBusinessObjectTestCase
	{
		#region Performance

		public void TestMultipleIndexesAreNotCreatedLoadingProcessTaskNotifications()
		{
			var newFactory = Factory.CreateNewFactory();
			var template = newFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();

			var templateNotification = newFactory.New<ProcessTaskNotification>();
			templateNotification.PQ_P9 = templateTrigger.PK;

			newFactory.Save();

			for (int i = 0; i < 1000; i++)
			{
				var job = Factory.New<SalesEnquiry>();
				job.O1_EnquiryType = "CCR";
				job.O1_LeadSource = "WEB";

				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job);
			}

			var dataSet = (INeedDataSet)Factory;
			DataTable table = dataSet.Data.Tables["ProcessTaskNotification"];
			var indexesInfo = typeof(DataTable).GetField("indexes", BindingFlags.Instance | BindingFlags.NonPublic);
			var indexes = (IEnumerable)indexesInfo.GetValue(table);

			AssertEquals(@"Excessive RowFilter found on index on ProcessTaskNotification. Check where ProcessTaskNotificationCollection has been loaded or something else....
This will make large numbers of edits incredibly slow if you don't fix it.
Check out ((System.Data.DataExpression)rowFilter).Expression to see the key", 2, indexes.Cast<object>().Count());
		}

		[DeveloperOnlyTest]
		public void TestAssignResourceToTask_WhenManyTaskAreInvolved_ShouldHappenInAcceptableTime()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var categorisedWorkflowTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();
			var dumWorkflowCode = DummyWorkflowDescriptor.Instance.Code;

			var categorisedWorkflowTaskType = categorisedWorkflowTaskTypeCollection.AddNew();
			categorisedWorkflowTaskType.Code = dumWorkflowCode;

			var workflowTaskInvType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskCDUType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskCDFType = categorisedWorkflowTaskType.TaskTypes.AddNew();

			workflowTaskInvType.Code = "INV";
			workflowTaskCDUType.Code = "CDU";
			workflowTaskCDFType.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedWorkflowTaskTypeCollection);

			var collection = new TaskTypeRestrictionsCollection();

			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = dumWorkflowCode;
			restriction.TaskType = "INV";
			restriction.NotificationType = NotificationTypeList.Codes.Error;
			restriction.Scope = ScopeList.Codes.Job;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var samTaskType1 = restriction.TaskTypesCollection.AddNew();
			var samTaskType2 = restriction.TaskTypesCollection.AddNew();

			samTaskType1.Code = "CDU";
			samTaskType2.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			BMTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);

			var firstWorkflow = BMTestHelper.CreateWorkflow(jobHeader, "FirstWorkflow");
			var firstTask = BMTestHelper.CreateTask(firstWorkflow, taskType: "INV");
			BMTestHelper.CreateTask(firstWorkflow, taskType: "CDU");
			BMTestHelper.CreateTask(firstWorkflow, taskType: "CDF");

			for (int i = 0; i < 5; i++)
			{
				var workflow = BMTestHelper.CreateWorkflow(jobHeader, "W" + i);

				for (int y = 0; y < 10; y++)
				{
					BMTestHelper.CreateTask(workflow, taskType: "INV");
					BMTestHelper.CreateTask(workflow, taskType: "CDU");
					BMTestHelper.CreateTask(workflow, taskType: "CDF");
				}
			}

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			var stopwatch = Stopwatch.StartNew();

			firstTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			firstTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			stopwatch.Stop();

			foreach (var task in jobHeader.Tasks)
			{
				AssertEquals(staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
			}

			AssertLessThan(stopwatch.ElapsedMilliseconds, 200);
		}

		#endregion

		#region Validation 

		public void TestLightValidationWhenNotInUserInteractive()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				var trigger = dummy.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				notification.PQ_EmailAddr = "alex@xlea.com";

				trigger.RunPreSaveValidation();
				((ILightValidationInternals)trigger).IsValid = false;

				Factory.Save();
				AssertEquals(false, ((ILightValidationInternals)trigger).IsValid);

				trigger.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
				trigger.P9_Description = "Shelf Test";

				Factory.SuspendValidation();
				trigger.RunPreSaveValidation();
				Factory.Save();

				AssertEquals(false, ((ILightValidationInternals)trigger).IsValid);
			}
		}

		#endregion

		#region Conditions

		public void TestSetTemplateConditionsDirectly_ShouldReportError()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals("", ErrorReporter.LastMessageReported);

			task.P9_Condition1 = "NAM";
			AssertEquals("Set template condition property P9_Condition1 without calling through ITemplateConditional interface. Please use the ProcessTask.TemplateConditions property to set conditions.", ErrorReporter.LastMessageReported);

			task.P9_Condition2 = "NAM";
			AssertEquals("Set template condition property P9_Condition2 without calling through ITemplateConditional interface. Please use the ProcessTask.TemplateConditions property to set conditions.", ErrorReporter.LastMessageReported);

			task.P9_Condition2Value = "NAM";
			AssertEquals("Set template condition property P9_Condition2Value without calling through ITemplateConditional interface. Please use the ProcessTask.TemplateConditions property to set conditions.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestSetTriggerConditionsDirectly_ShouldReportError()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals("", ErrorReporter.LastMessageReported);

			foreach (var info in GetInfos(task))
			{
				info.Value = info.Value;
				AssertEquals($"Set trigger condition property {info.Name} without calling through ITriggerConditions interface. Please use the ProcessTask.TriggerConditions property to set conditions.", ErrorReporter.LastMessageReported);
			}
			ErrorReporter.Clear();
		}

		public void TestSetTriggerConditionsDirectly_ReadOnly()
		{
			foreach (var info in GetInfos(Factory.New<ProcessTask>()))
			{
				Assert(info.ReadOnly);
			}
		}

		IEnumerable<ZPropertyInfo> GetInfos(ProcessTask task)
		{
			yield return task.P9_SE_NKMilestoneEventInfo;
			yield return task.P9_TriggerFieldInfo;
			yield return task.P9_TriggerConditionInfo;
			yield return task.P9_TriggerConditionValueInfo;
			yield return task.P9_TriggerFiredCountdownInfo;
		}

		public void TestSetCapabilityCode_ShouldSetP9_G4_RequiredCapability()
		{
			// Arrange
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "TLT";
			Factory.Save();

			var task = Factory.New<ProcessTask>();

			// Act
			task.CapabilityCode = capability.G4_Code;

			// Assert
			AssertEquals("Setting correct capability code should set P9_G4_RequiredCapability", task.P9_G4_RequiredCapability, capability.PK);
		}

		public void TestGetCapabilityCode_ShouldReturnCapabilityCode_OfRequiredCapability()
		{
			// Arrange
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			Factory.Save();

			var task = Factory.New<ProcessTask>();
			task.P9_G4_RequiredCapability = capability.PK;

			// Act
			var capabilityCode = task.CapabilityCode;

			// Assert
			AssertEquals("CapabilityCode property of a valid process task must return RequiredCapability.G4_Code", capabilityCode, task.RequiredCapability.G4_Code);
		}

		public void TestTriggerConditionValueSupportsUnicodeCharacters()
		{
			var unicodeString = "ÀÂÄÈÉÊËÎÏÔŒÙÛÜŸ";

			var job = Factory.New<DummyWithWorkflow>();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerConditionValue = unicodeString;
			Factory.Save();

			var reloadedTrigger = new BusinessObjectFactory().Load<ProcessTask>(trigger.PK);
			AssertEquals(unicodeString, reloadedTrigger.TriggerConditions.TriggerConditionValue);
		}

		#endregion

		#region Delayed Milestone Application

		public void TestDelayedMilestoneApplied_WhenEventsExisted_ForADDEvent()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			// Create Template
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			template.P0_GB = branch.PK;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.P9_Description = "Test Milestone";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;

			var templateAction = templateMilestone.ProcessTaskNotifications.AddNew();
			templateAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			// Create Shipment
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			// Make sure shipment log event-date is the first ADD logs
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			Factory.Save();

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			AssertEquals("Milestone hasn't been applied", 0, shipmentAsWorkflowProvider.WorkflowItems.Milestones.Count);

			// Apply template-trigger
			jobHeader.JH_GB = branch.PK; // condition for template-milestone to be applied
			shipment.JS_GoodsDescription = "test"; // executing code to re-apply template

			Factory.Save();

			AssertEquals("Milestone has been applied", 1, shipmentAsWorkflowProvider.WorkflowItems.Milestones.Count);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, shipmentAsWorkflowProvider.WorkflowItems.Milestones.FirstOrDefault().PK);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("\r\nGIVEN delayed-ADD-Milestone\r\nWHEN it got applied with existing ADD events, SHOULD only fired once", 1, logs.Length);

			var log = logs[0];
			var sourceLog = ((EnterpriseBusinessObject)shipment).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystemCode));
			AssertContains("Should contain triggering Log pk", sourceLog[0].PK.ToString(), log.SL_Reference);
		}

		public void TestTriggerFiredFromExceptionRaisedWithCondition(bool isRegistryOn, string triggerCondition, string triggerConditionValue, string logReference)
		{
			WorkflowDataRegistry.Instance.ExceptionEXRReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isRegistryOn);

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ExceptionRaised.Code;
			trigger.TriggerConditions.TriggerCondition = triggerCondition;
			trigger.TriggerConditions.TriggerConditionValue = triggerConditionValue;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "NTF";
			action.PQ_TriggerParty = "EML";
			action.PQ_EmailAddr = "lolmail@lolmail.com";

			Factory.Save();

			var milestone = shipmentWithWorkflow.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Description";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			milestone.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
			milestone.P9_ScheduledDate = new ZDateTime(2020, 1, 1);

			Factory.Save();

			var exceptionRaisedLogOnParent = shipmentWithWorkflow.Logs.MostRecentLogByEventTime(Events.ExceptionRaised);

			AssertNotNull(exceptionRaisedLogOnParent);
			AssertEquals(logReference, exceptionRaisedLogOnParent.SL_Reference);
			AssertEquals(99, (int)trigger.P9_TriggerFiredCountdown);
		}

		[TestDate(2022, 1, 1)]
		public void TestTriggerFiredFromExceptionRaisedWithCondition_RFP()
		{
			TestTriggerFiredFromExceptionRaisedWithCondition(true, "RFP", "TYP=EXF,EVT=Z00,DES=Description", "|TYP=EXF|EVT=Z00|DES=Description");
		}

		[TestDate(2022, 1, 1)]
		public void TestTriggerFiredFromExceptionRaisedWithCondition_EVT()
		{
			TestTriggerFiredFromExceptionRaisedWithCondition(true, "EVT", "Z00", "|TYP=EXF|EVT=Z00|DES=Description");
			TestTriggerFiredFromExceptionRaisedWithCondition(false, "EVT", "Z00", "Type:[EXF]; Event:[Z00]");
		}

		[TestDate(2022, 1, 1)]
		public void TestTriggerFiredFromExceptionRaisedWithCondition_EXT()
		{
			TestTriggerFiredFromExceptionRaisedWithCondition(true, "EXT", "EXF", "|TYP=EXF|EVT=Z00|DES=Description");
			TestTriggerFiredFromExceptionRaisedWithCondition(false, "EXT", "EXF", "Type:[EXF]; Event:[Z00]");
		}

		public void TestDelayedMilestoneApplied_WhenNewEventsAdded_ForADDEvent()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			// Create Template
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			template.P0_GB = branch.PK;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.P9_Description = "Test Milestone";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;

			var templateAction = templateMilestone.ProcessTaskNotifications.AddNew();
			templateAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			// Create Shipment
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			// Make sure shipment log event-date is the first ADD logs
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			// Apply template-trigger
			jobHeader.JH_GB = branch.PK; // condition for template-milestone to be applied
			shipment.JS_GoodsDescription = "test"; // executing code to re-apply template

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			Factory.Save();

			AssertEquals("Milestone has been applied", 1, shipmentAsWorkflowProvider.WorkflowItems.Milestones.Count);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, shipmentAsWorkflowProvider.WorkflowItems.Milestones.FirstOrDefault().PK);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("\r\nGIVEN delayed-ADD-Milestone\r\nWHEN it got applied with existing ADD events, SHOULD only fired once", 1, logs.Length);
		}

		public void TestDelayedMilestoneApplied_WhenEventsExistedAndNewEventsAdded_ForADDEvents()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			// Create Template
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			template.P0_GB = branch.PK;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.P9_Description = "Test Milestone";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;

			var templateAction = templateMilestone.ProcessTaskNotifications.AddNew();
			templateAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			// Create Shipment
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			// Make sure shipment log event-date is the first ADD logs
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			AssertEquals("Milestone hasn't been applied", 0, shipmentAsWorkflowProvider.WorkflowItems.Milestones.Count);

			// Apply template-trigger
			jobHeader.JH_GB = branch.PK; // condition for template-milestone to be applied
			shipment.JS_GoodsDescription = "test"; // executing code to re-apply template

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			Factory.Save();

			AssertEquals("Milestone has been applied", 1, shipmentAsWorkflowProvider.WorkflowItems.Milestones.Count);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, shipmentAsWorkflowProvider.WorkflowItems.Milestones.FirstOrDefault().PK);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("\r\nGIVEN delayed-ADD-Milestone\r\nWHEN it got applied with existing ADD events, SHOULD only fired once", 1, logs.Length);

			var log = logs[0];
			var sourceLog = ((EnterpriseBusinessObject)declaration).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystemCode));
			AssertContains("Should contain triggering Log pk", sourceLog[0].PK.ToString(), log.SL_Reference);
		}

		public void TestDuplicateWTELogsDontFire()
		{
			// Create Shipment
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_GoodsDescription = "test";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Factory.Save();

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			var trigger = shipmentAsWorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test ADD";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "NTF";
			action.PQ_TriggerParty = "EML";
			action.PQ_EmailAddr = "email@email.com";

			var reference = "BOG|" + Guid.NewGuid().ToString();
			var log = shipmentAsWorkflowProvider.Logs.AddNew(Events.CustomisableEvent00, reference);
			var triggeringEvent = new EventSource(log);

			var triggeringEventData = new WorkflowTriggerEventData(
					triggeringEvent,
					ZGuid.Empty,
					GlbBranch.GetCurrentBranch(trigger.Factory).GB_Code,
					GlbDepartment.GetCurrentDepartment(trigger.Factory).GE_Code, "E", "BNE", "BRN");

			Factory.Save();

			WorkflowTriggerEventExtensions.AddWorkflowTriggerEventLog(trigger, (BusinessObject)shipment, triggeringEvent, null);
			shipmentAsWorkflowProvider.Logs.AddNew(Events.CustomisableEvent00, reference);

			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, triggeringEventData.ToReference());
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var logs = Factory.Load<StmALog>(query);

			AssertEquals("Two WTE events", 2, logs.Length);

			var logwalker = MasterFilesTestHelper.RunLogWalker();

			AssertContains("Withdraw WTE event", "Did not fire event", logwalker);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestWTEDefaultUserContextUsesTriggerParent()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company.PK;
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var declaration = Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			var cusEntryHeader = Factory.New<Enterprise.Integration.Customs.US.ICusEntryHeader>();
			cusEntryHeader.CH_JE = declaration.PK;
			cusEntryHeader.CH_MessageType = "SE";
			cusEntryHeader.CH_Status = "AEO";

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_GE = department.PK;
			jobHeader.JH_GB = branch2.PK;
			jobHeader.JH_GC = company.PK;
			jobHeader.JH_ParentID = declaration.PK;
			jobHeader.JH_JobNum = "Bork";

			Factory.Save();

			var logParent = (IStmALogParent)cusEntryHeader;
			var workflowProvider = (IWorkflowProvider)declaration;
			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomsEntryStatusCode;
			trigger.P9_GC = company.PK;

			Factory.Save();

			var log = logParent.Logs.AddNew(Events.CustomsEntryStatus);
			var wteLog = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var triggerEventData = new WorkflowTriggerEventData(wteLog);
			AssertEquals(branch2.GB_Code, triggerEventData.ContextBranchCode);
		}

		public void TestLWKLogsContextSwitchDueToInactiveUser()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";

			var staffInvalid = Factory.NewWithValidTestData<GlbStaff>();
			staffInvalid.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staffInvalid.GS_IsActive = false;

			var staffValid = Factory.NewWithValidTestData<GlbStaff>();
			staffValid.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			trigger.TriggerConditions.TriggerStaffCode = staffInvalid.GS_Code;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			trigger.P9_Description = "Test";

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction.PQ_FieldName = "<JobHeader.JH_Status>";
			triggerAction.PQ_FieldValue = "WHL";

			Factory.Save();
			shipmentWithWorkflow.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var logwalker = MasterFilesTestHelper.RunLogWalker();
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertContains($"User {staffInvalid.GS_Code} is inactive. Default context will be used.", logwalker);
		}

		public void TestLWKLogsContextSwitchHandlesNullUser()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction.PQ_FieldName = "<JS_HouseBill>";
			triggerAction.PQ_FieldValue = "123";

			Factory.Save();

			var log = shipmentWithWorkflow.Logs.AddNew(Events.CustomisableEvent00);
			var wteEvents = trigger.Logs.Find(log => log.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			AssertEquals("Has one wte event", 1, wteEvents.Count());
			var wte = wteEvents.First();

			var data = new WorkflowTriggerEventData(wte);
			data.ContextStaffCode = "INVALID";

			using (wte.LockForUpdatingKeyFieldsForTesting())
			{
				wte.SL_Reference = data.ToReference();
			}
			Factory.Save();
			MasterFilesTestHelper.RunLogWalker();

			((BusinessObject)shipment).Reload();
			AssertEquals("Set field", "123", shipment.JS_HouseBill);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestAddWorkflowTriggerEventLogForCommercialInvoiceAttachedOnDeclaration()
		{
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			var invoiceHeader = Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoiceHeader.JZ_InvoiceNumber = "INV00001";
			invoiceHeader.JZ_JE = declaration.PK;
			Factory.Save();

			var workflowProvider = (IWorkflowProvider)invoiceHeader;
			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test";
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecordCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			triggerAction.PQ_FieldName = "<JZ_MessageType>";
			triggerAction.PQ_FieldValue = "ASN";

			var reference = "INV|" + Guid.NewGuid().ToString();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var log = workflowProvider.Logs.AddNew(Events.EditedARecord, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var triggeringEvent = new EventSource(log);

			var triggeringEventData = new WorkflowTriggerEventData(
					triggeringEvent,
					ZGuid.Empty,
					GlbBranch.GetCurrentBranch(trigger.Factory).GB_Code,
					GlbDepartment.GetCurrentDepartment(trigger.Factory).GE_Code, "E", "BNE", "BRN");

			Factory.Save();

			AssertNoExceptionThrown("No trigger action performed since invoice header is attached to a customs declaration.", () =>
			{
				WorkflowTriggerEventExtensions.AddWorkflowTriggerEventLog(trigger, (BusinessObject)invoiceHeader, triggeringEvent, log);
			});
		}

		public void TestDelayedMilestoneApplied_WhenEventsExisted_WithDeclarationFilter()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			// Create Template
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			template.P0_GB = branch.PK;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.P9_Description = "Test Milestone";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;
			templateMilestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			templateMilestone.TriggerConditions.TriggerConditionValue = "Event.Source.Contains(\"Declaration\")";

			var templateAction = templateMilestone.ProcessTaskNotifications.AddNew();
			templateAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			// Create Shipment
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			Factory.Save();

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			AssertEquals("Milestone hasn't been applied", 0, shipmentAsWorkflowProvider.WorkflowItems.Milestones.Count);

			// Apply template-trigger
			jobHeader.JH_GB = branch.PK; // condition for template-milestone to be applied
			shipment.JS_GoodsDescription = "test"; // executing code to re-apply template

			Factory.Save();

			AssertEquals("Milestone has been applied", 1, shipmentAsWorkflowProvider.WorkflowItems.Milestones.Count);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, shipmentAsWorkflowProvider.WorkflowItems.Milestones.FirstOrDefault().PK);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("\r\nGIVEN delayed-ADD-Milestone\r\nWHEN it got applied with existing ADD events, SHOULD only fired once", 1, logs.Length);

			var log = logs[0];
			var sourceLog = ((EnterpriseBusinessObject)declaration).Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystemCode));
			AssertContains("Should contain triggering Log pk", sourceLog[0].PK.ToString(), log.SL_Reference);
		}

		public void TestMCR_AccessToEnv()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Test Milestone";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			milestone.TriggerConditions.TriggerConditionValue = $"\"<@env.Company.Code>\" == \"{company.GC_Code}\"";

			var templateAction = milestone.ProcessTaskNotifications.AddNew();
			templateAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			job.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			AssertEquals(0, milestone.GetWTELogs().Length);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				job.Logs.AddNew(Events.CustomisableEvent00);
				Factory.Save();
			}

			AssertEquals(1, milestone.GetWTELogs().Length);
		}

		public void TestMCR_AccessToEvent()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var milestoneDepartment = job.WorkflowItems.Milestones.AddNew();
			milestoneDepartment.P9_Description = "Test Milestone Department Code";
			milestoneDepartment.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestoneDepartment.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			milestoneDepartment.TriggerConditions.TriggerConditionValue = $"\"<Event.DepartmentCode>\" == \"{department.GE_Code}\"";

			var milestoneBranch = job.WorkflowItems.Milestones.AddNew();
			milestoneBranch.P9_Description = "Test Milestone Branch Code";
			milestoneBranch.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestoneBranch.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			milestoneBranch.TriggerConditions.TriggerConditionValue = $"\"<Event.BranchCode>\" == \"{branch.GB_Code}\"";

			var milestoneCompany = job.WorkflowItems.Milestones.AddNew();
			milestoneCompany.P9_Description = "Test Milestone Company Code";
			milestoneCompany.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestoneCompany.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			milestoneCompany.TriggerConditions.TriggerConditionValue = $"\"<Event.CompanyCode>\" == \"{company.GC_Code}\"";

			job.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(0, milestoneDepartment.GetWTELogs().Length);
				AssertEquals(0, milestoneBranch.GetWTELogs().Length);
				AssertEquals(0, milestoneCompany.GetWTELogs().Length);
			});

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				job.Logs.AddNew(Events.CustomisableEvent00);
				Factory.Save();
			}

			CombineAssertions(() =>
			{
				AssertEquals(1, milestoneDepartment.GetWTELogs().Length);
				AssertEquals(1, milestoneBranch.GetWTELogs().Length);
				AssertEquals(1, milestoneCompany.GetWTELogs().Length);
			});
		}

		public void TestMCR_DoNotContainHiddenReferenceText()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Test Milestone";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			milestone.TriggerConditions.TriggerConditionValue = "IsMatchingPattern(\"BOG\",Event.Reference)";

			var templateAction = milestone.ProcessTaskNotifications.AddNew();
			templateAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			var reference = "BOG|" + Guid.NewGuid().ToString();
			var log = job.Logs.AddNew(Events.CustomisableEvent00, reference);
			AssertEquals("BOG", log.SL_ReferenceForBinding);
			Factory.Save();
			AssertEquals(1, milestone.GetWTELogs().Length);
		}

		public void TestMCR_ProperlyEvaluatesPipe()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var milestone1 = job.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "Test Milestone";
			milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			milestone1.TriggerConditions.TriggerConditionValue = "IsMatchingPattern(\"LCO*\",Event.Reference)";

			var milestone2 = job.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "Test Milestone";
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			milestone2.TriggerConditions.TriggerConditionValue = "IsMatchingPattern(\"LCO|*\",Event.Reference)";

			var milestone3 = job.WorkflowItems.Milestones.AddNew();
			milestone3.P9_Description = "Test Milestone";
			milestone3.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone3.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			milestone3.TriggerConditions.TriggerConditionValue = "IsMatchingPattern(\"LOC|*\",Event.Reference)";

			var templateAction = milestone1.ProcessTaskNotifications.AddNew();
			templateAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			var reference = "LCO|RandomText" + Guid.NewGuid().ToString();
			var log = job.Logs.AddNew(Events.CustomisableEvent00, reference);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Milestone 1", 1, milestone1.GetWTELogs().Length);
				AssertEquals("Milestone 2", 1, milestone2.GetWTELogs().Length);
				AssertEquals("Milestone 3", 0, milestone3.GetWTELogs().Length);
			});
		}

		public void TestExistingMilestone_WithNewEvents()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			var milestone = shipmentAsWorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Test Milestone";
			milestone.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			var log1 = job.Logs.AddNew();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = ZDateTime.Now;
				log1.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			Factory.Save(); // Save is needed because milestone will use the event time of whichever even is last raised before it is saved.
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			var log2 = job.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = ZDateTime.Now;
				log2.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			Factory.Save();

			AssertEquals("GIVEN existing milestone, WHEN new events occurred, milestone actual date should be set to the earliest event", log1.SL_EventTime, milestone.P9_ActualDate);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, milestone.PK);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("\r\nGIVEN delayed-ADD-Milestone\r\nWHEN it got applied with existing ADD events, SHOULD only fired once", 1, logs.Length);

			var log = logs[0];
			var sourceLog = job.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
			AssertContains("Should contain triggering Log pk", sourceLog[0].PK.ToString(), log.SL_Reference);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();

			logs = Factory.Load<StmALog>(query);
			AssertEquals("Updating milestone actual-date, should not create new WTE.", 1, logs.Length);
		}

		public void TestMilestoneWithTheSameEventLogUpdated_WhenCountDownLessThanOne()
		{
			var (job, milestone) = prepareMilestoneWithCountdownOne();

			var log = job.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = ZDateTime.Now;
				log.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			AssertEquals("milestone fired once", (short)0, milestone.P9_TriggerFiredCountdown);

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = log.SL_EventTime.AddDays(-10);
			}

			AssertEquals("Milestone actual date should be set to the latest event time", log.SL_EventTime, milestone.P9_ActualDate);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, milestone.PK);

			var wtelogs = Factory.Load<StmALog>(query);
			AssertEquals("Milestone only fired once", 1, wtelogs.Length);
			AssertEquals("Milestone fired event time should be set to the latest event time", log.SL_EventTime, wtelogs[0].SL_EventTime);
		}

		public void TestMilestoneWithTheSameEventLogUpdatedAfterSave_WhenCountDownLessThanOne()
		{
			var (job, milestone) = prepareMilestoneWithCountdownOne();

			var log = job.Logs.AddNew();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = ZDateTime.Now;
				log.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			AssertEquals("milestone fired once", (short)0, milestone.P9_TriggerFiredCountdown);

			Factory.Save();

			var firstEventTime = log.SL_EventTime;
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = log.SL_EventTime.AddDays(-10);
			}

			AssertEquals("Milestone actual date should be set to the first event time", firstEventTime, milestone.P9_ActualDate);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, milestone.PK);

			var wtelogs = Factory.Load<StmALog>(query);
			AssertEquals("Milestone only fired once", 1, wtelogs.Length);
			AssertEquals("Milestone fired event time should be set to the first event time", firstEventTime, wtelogs[0].SL_EventTime);
		}

		public void TestMilestoneWithNewEventLog_WhenCountDownLessThanOne()
		{
			var (job, milestone) = prepareMilestoneWithCountdownOne();

			var log = job.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = ZDateTime.Now;
				log.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			AssertEquals("milestone fired once", (short)0, milestone.P9_TriggerFiredCountdown);

			var log2 = job.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = log2.SL_EventTime.AddDays(-10);
				log2.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			AssertEquals("Milestone actual date should be set to the first event time", log.SL_EventTime, milestone.P9_ActualDate);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, milestone.PK);

			var wtelogs = Factory.Load<StmALog>(query);
			AssertEquals("Milestone only fired once", 1, wtelogs.Length);
			AssertEquals("Milestone fired event time should be set to the first event time", log.SL_EventTime, wtelogs[0].SL_EventTime);
		}

		(JobHeader job, ProcessTask milestone) prepareMilestoneWithCountdownOne()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			var milestone = shipmentAsWorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Test Milestone";
			milestone.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			milestone.TriggerConditions.TriggerFiredCountdown = 1;

			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			return (job, milestone);
		}

		public void TestManuallyInsertMilestone_WithExistingEvents()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			Factory.Save();

			var log1 = job.Logs.AddNew();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = ZDateTime.Now;
				log1.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			var log2 = job.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = ZDateTime.Now;
				log2.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			Factory.Save();

			var milestone = shipmentAsWorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Test Milestone 2";
			milestone.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			AssertEquals("GIVEN new events, WHEN manually inserting milestone, should be set to the earliest event", log1.SL_EventTime, milestone.P9_ActualDate);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, milestone.PK);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("\r\nGIVEN delayed-ADD-Milestone\r\nWHEN it got applied with existing ADD events, SHOULD only fired once", 1, logs.Length);

			var log = logs[0];
			var sourceLog = job.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
			AssertContains("Should contain triggering Log pk", sourceLog[0].PK.ToString(), log.SL_Reference);
		}

		#endregion

		#region Saving Milestone together with events

		public void TestManuallyInsertMilestone_WithNewEventsAddedBeforeMilestone()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			Factory.Save();

			var log1 = job.Logs.AddNew();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = ZDateTime.Now;
				log1.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			var log2 = job.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = ZDateTime.Now;
				log2.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			var milestone = shipmentAsWorkflowProvider.WorkflowItems.AddNew();
			milestone.P9_ParentID = shipmentAsWorkflowProvider.PK;
			milestone.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			milestone.IsMilestone = true;
			milestone.P9_Description = "Test Milestone";
			milestone.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			AssertEquals("GIVEN new events, WHEN manually inserting milestone, should be set to the earliest event", log1.SL_EventTime, milestone.P9_ActualDate);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, milestone.PK);

			var logs = Factory.Load<StmALog>(query);
		}

		[TestDate(2017, 1, 1)]
		public void TestManuallyInsertMilestone_WithNewEventsAddedAfterMilestone()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = "JS";

			Factory.Save();

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			var milestone = shipmentAsWorkflowProvider.WorkflowItems.AddNew();
			milestone.P9_ParentID = shipmentAsWorkflowProvider.PK;
			milestone.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			milestone.IsMilestone = true;
			milestone.P9_Description = "Test Milestone";
			milestone.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			var log1 = job.Logs.AddNew();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_EventTime = ZDateTime.Now;
				log1.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			var log2 = job.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_EventTime = ZDateTime.Now;
				log2.SL_SE_NKEvent = Events.AuthorisedCode;
			}

			Factory.Save();

			AssertEquals("GIVEN new events, WHEN manually inserting milestone, should be set to the earliest event", log1.SL_EventTime, milestone.P9_ActualDate);

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, milestone.PK);

			var logs = Factory.Load<StmALog>(query);
			AssertEquals("\r\nGIVEN delayed-ADD-Milestone\r\nWHEN it got applied with existing ADD events, SHOULD only fired once", 1, logs.Length);

			var log = logs[0];
			var sourceLog = job.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AuthorisedCode));
			AssertContains("Should contain triggering Log pk", sourceLog[0].PK.ToString(), log.SL_Reference);
		}

		#endregion

		#region BufferManagement

		public void TestGetStringFromPropertyValueForZGuidWithRelatedBusinessObject_DontCrashOnDeleted()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			BMTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Foo";
			var task = workflow1.Parent.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var row = ((INeedRow)task).Row;
			task.P9_FH_ProcessHeader = workflow1.PK;
			AssertEquals("Foo", new DataVersionLogValueFormatter().GetStringFromPropertyValue(task.P9_FH_ProcessHeaderInfo, row));
			task.P9_FH_ProcessHeader = ZGuid.Empty;
			workflow1.Delete();
			task.P9_FH_ProcessHeader = workflow1.PK;
			AssertEquals(string.Empty, new DataVersionLogValueFormatter().GetStringFromPropertyValue(task.P9_FH_ProcessHeaderInfo, row));
		}

		public void TestProcessHeaderProperty_ShouldCacheBusinessObjectAndOnlyReloadWhenFH_ProcessHeaderChanged()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			BMTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var processHeaderType = workflow1.GetType().FullName;
			var task = workflow1.Parent.WorkflowItems.Tasks.AddNew();
			var getIProcessHeaderTypeCount = 0;

			var substituteDisposable = ObjectFactory.Substitute<IProcessHeader>(() =>
			{
				getIProcessHeaderTypeCount++;
				return workflow1;
			});

			using (substituteDisposable)
			{
				var currentWorkflow = task.ProcessHeader;

				AssertNull(currentWorkflow);
				AssertEquals("Should not try to load processHeader type, processHeader is already loaded", 0, getIProcessHeaderTypeCount);

				task.P9_FH_ProcessHeader = workflow1.PK;
				currentWorkflow = task.ProcessHeader;

				AssertEquals(workflow1, currentWorkflow);
				AssertEquals("Should have tried to load processHeader, to get the current one", 1, getIProcessHeaderTypeCount);

				getIProcessHeaderTypeCount = 0;
				currentWorkflow = task.ProcessHeader;

				AssertEquals(workflow1, currentWorkflow);
				AssertEquals("Should not try to load processHeader type, processHeader is already loaded", 0, getIProcessHeaderTypeCount);

				task.P9_FH_ProcessHeader = workflow2.PK;
				currentWorkflow = task.ProcessHeader;

				AssertEquals(workflow2, currentWorkflow);
				AssertEquals("Should have tried to load processHeader type only for the current one, previous is already loaded", 1, getIProcessHeaderTypeCount);

				getIProcessHeaderTypeCount = 0;

				AssertEquals(currentWorkflow, task.ProcessHeader);
				AssertEquals("Should not try to load ProcessHeader type, processHeader is already loaded", 0, getIProcessHeaderTypeCount);
			}
		}

		[ExpectException(typeof(ZSaveException))]
		public void TestEmptyParentTableCodeConstraint()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			BMTestHelper.CreateSystem(Factory, "DUM");

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_ParentTableCode = ZString.Empty;
			Factory.Save();
		}

		public void TestSetWorkflowOnNonTask()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			BMTestHelper.CreateSystem(Factory, "DUM");

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var task = job.WorkflowItems.Tasks.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertNoExceptionThrown(() => task.P9_FH_ProcessHeader = workflow.PK);

			AssertExceptionThrown<InvalidOperationException>(() => milestone.P9_FH_ProcessHeader = workflow.PK);
			AssertExceptionThrown<InvalidOperationException>(() => trigger.P9_FH_ProcessHeader = workflow.PK);
			AssertExceptionThrown<InvalidOperationException>(() => exception.P9_FH_ProcessHeader = workflow.PK);

			AssertEquals(workflow.PK, task.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, milestone.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, trigger.P9_FH_ProcessHeader);
			AssertEquals(ZGuid.Empty, exception.P9_FH_ProcessHeader);
		}

		public void TestChangeTaskToNonTask_ShouldRemoveWorkflowFK()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			BMTestHelper.CreateSystem(Factory, "DUM");

			var workflow = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory).ProcessHeaders.AddNew();
			var task = workflow.Parent.WorkflowItems.Tasks.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			task.P9_Type = "";
			task.P9_Type = "NAM";
			AssertEquals(workflow.PK, task.P9_FH_ProcessHeader);

			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			AssertEquals(ZGuid.Empty, task.P9_FH_ProcessHeader);

			task.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Type = Core.Constants.Workflow.ExceptionType;
			AssertEquals(ZGuid.Empty, task.P9_FH_ProcessHeader);

			task.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			AssertEquals(ZGuid.Empty, task.P9_FH_ProcessHeader);
		}

		public void TestRelevantEstimateHoursLabel()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = new ZDateTime(2013, 1, 1, 1, 0, 0); // 1 hour
			task.P9_EstimatedTimeToComplete = new ZDateTime(2013, 1, 2, 16, 0, 0); // 40 hours
			AssertEquals("40:00", task.RelevantEstimateHoursLabel);
		}

		public void TestP9_NotesAsString()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_NotesAsString = "boooo";
			AssertEquals("boooo", task.P9_NotesAsString);

			Factory.Save();

			var loadedTask = Factory.Load<ProcessTask>(task.PK);
			AssertEquals("boooo", loadedTask.P9_NotesAsString);
		}

		public void TestP9_NotesAsPlainText()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_NotesAsString = DummyProcessTask.GetAsRtfFormatted("boooo");
			AssertEquals("boooo", task.P9_NotesAsPlainText);

			Factory.Save();

			var loadedTask = Factory.Load<ProcessTask>(task.PK);
			AssertEquals("boooo", loadedTask.P9_NotesAsPlainText);
		}

		public void TestP9_NotesMerging()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();

			var taskInFactory1 = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTask>(task.PK);
			var taskInFactory2 = new BusinessObjectFactory { RefreshEnabled = false }.Load<ProcessTask>(task.PK);

			var note1 = "WOT?? WOT IS THIS TASK!??!?!?!?! Y U ASSIGN TO ME";
			var note2 = "Plz sir. Plz I need ur help this is too hard :'( :'(";
			taskInFactory1.P9_NotesAsString = DummyProcessTask.GetAsRtfFormatted(note1);
			taskInFactory2.P9_NotesAsString = DummyProcessTask.GetAsRtfFormatted(note2);
			taskInFactory1.Factory.Save();
			try
			{
				taskInFactory2.Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, new DummyNotificationHandler());
			}
			var mergeResult = taskInFactory2.P9_NotesAsString;
			AssertContains(note1, mergeResult);
			AssertContains(note2, mergeResult);

			var mergeResultPlainText = taskInFactory2.P9_NotesAsPlainText;
			AssertContains(note1, mergeResultPlainText);
			AssertContains(note2, mergeResultPlainText);
		}

		class DummyNotificationHandler : INotificationHandler
		{
			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null) { }
			public void ReportInformation(string message, string caption) { }
		}

		public void TestRtfMerge()
		{
			var note1 = "WOT?? WOT IS THIS TASK!??!?!?!?! Y U ASSIGN TO ME";
			var note2 = "Plz sir. Plz I need ur help this is too hard :'( :'(";
			var noteFormatted1 = DummyProcessTask.GetAsRtfFormatted(note1);
			var noteFormatted2 = DummyProcessTask.GetAsRtfFormatted(note2);

			var mergeResult = ORtfTextUtil.AppendRtfStrings(noteFormatted1, noteFormatted2);
			AssertContains(note1, mergeResult);
			AssertContains(note2, mergeResult);
		}

		public void TestRtfDefault()
		{
			var note = "BOGGO";
			AssertContains(note, ORtfTextUtil.AppendRtfStrings("Its time for spanko", note));
		}

		public void TestCompletionStatementTask_DescriptionShouldMatchNotesAsString()
		{
			MasterFilesTestHelper.AddCompletionStatementTaskType("COM", "ORG", "Completion Statement");

			var org = Factory.New<OrgHeader>();
			var task = org.WorkflowItems.AddNew();
			task.P9_Type = "COM";
			Assert(task.IsCompletionStatement);

			task.P9_NotesAsString = "0123456789 0123456789 0123456789 0123456789 0123456789 0123456789 0123456789";
			AssertEquals(task.P9_NotesAsString.Substring(0, ProcessTasksSchema.P9_Description.MaxLength - 3) + "...", task.P9_Description);
		}

		public void TestCompletionStatementTask_DescriptionShouldBeReadOnly()
		{
			MasterFilesTestHelper.AddCompletionStatementTaskType("COM", "ORG");

			var org = Factory.New<OrgHeader>();
			var task = org.WorkflowItems.AddNew();
			task.P9_Type = "COM";

			Assert(task.IsCompletionStatement);
			AssertEquals(true, task.P9_DescriptionInfo.ReadOnly);
		}

		[TestDate(2015, 1, 20)]
		public void TestClosedTaskWithZeroActualDuration_ShouldShowNotificationMessage()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = "ORG";
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "INV";
			AssertEquals("Default Value Should be 'false'", false, taskType.IsRequireActualDuration);
			taskType.IsRequireActualDuration = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var now = new ZDateTime(ZDateTime.UtcNow.Year, 1, 1);
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "NAM";
			var task1 = org.WorkflowItems.AddNew();
			task1.P9_Type = taskType.Code;
			var task2 = org.WorkflowItems.AddNew();
			task2.P9_Type = taskType.Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_ActualDuration = now.AddHours(1);

			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_ActualDuration = now;

			AssertHasErrors(task1.P9_ActualDurationInfo);
			AssertHasErrors(task2.P9_ActualDurationInfo);
		}

		public void TestDisplayOrder()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_Sequence = 999;
			AssertEquals(0, task.DisplayOrder);
			task.P9_Sequence = 1001;
			AssertEquals(1, task.DisplayOrder);
			task.P9_Sequence = 1002;
			AssertEquals(2, task.DisplayOrder);
		}

		public void TestIsEffectAchieved()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(false, task.IsEffectAchieved);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(true, task.IsEffectAchieved);

			task.IsEffectAchieved = true;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);

			task.IsEffectAchieved = false;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);
		}

		public void TestIsEffectAchieved_SetsToPreviousStateWhenUnchecked()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
			Assert(task.P9_GS_NKAssignedStaffMember.IsEmpty);
			Assert(task.P9_G4_RequiredCapability.IsEmpty);
			Assert(!task.IsEffectAchieved);

			foreach (ICodeDescription status in new ProcessTaskStatusCodeList())
			{
				AssertTaskStatus(task, status.Code);
			}
		}

		void AssertTaskStatus(ProcessTask task, ZString status)
		{
			task.P9_Status = status;

			task.IsEffectAchieved = true;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);

			task.IsEffectAchieved = false;
			AssertEquals(status == ProcessTaskStatusCodeList.Codes.Closed ? (ZString)ProcessTaskStatusCodeList.Codes.Assigned : status, task.P9_Status);
		}

		public void TestIsCompletionTask()
		{
			MasterFilesTestHelper.AddCompletionStatementTaskType("BOO", "ORG");

			var org = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			var completionStatementTask = jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			completionStatementTask.P9_FH_ProcessHeader = workflow.PK;
			AssertEquals("BOO", completionStatementTask.P9_Type);
			AssertEquals(true, completionStatementTask.IsCompletionTask());

			var task = org.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			AssertEquals(false, task.IsCompletionTask());
		}

		public void TestGetCompletionStatementTaskType()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypeCategory1 = categorisedTaskTypes.AddNew();
			taskTypeCategory1.Code = "FOO";
			var taskType1 = taskTypeCategory1.TaskTypes.AddNew();
			taskType1.Code = "BOO";
			taskType1.IsCompletionStatementTaskType = true;

			var taskTypeCategory2 = categorisedTaskTypes.AddNew();
			taskTypeCategory2.Code = "FAR";
			var taskType2 = taskTypeCategory2.TaskTypes.AddNew();
			taskType2.Code = "BAR";
			taskType2.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			AssertEquals("BOO", Business.ProcessTask.GetCompletionStatementTaskType("FOO"));
			AssertEquals("BAR", Business.ProcessTask.GetCompletionStatementTaskType("FAR"));
		}

		public void TestEstimateDescription_ValuesOverOneDay()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = new ZInt(60 * 20).GetDateTimeFromMinutes(); // 20 hours
			task.P9_EstimateVariationFactor = 2;

			AssertEquals("20:00 to 40:00 hours (standard estimate 30:00)", task.EstimateDescription);
		}

		public void TestJobNumber()
		{
			var dummy1 = Factory.New<DummyWithWorkflow>();
			dummy1.Z0_Description = "JOB1111";

			var dummy2 = Factory.New<DummyWithWorkflowAndJobNumberForWorkflow>();
			dummy2.Z0_Description = "X1234";
			dummy2.SetJobNumberForWorkflow("DUMMY2");

			var dummy1ProcessTask = dummy1.WorkflowItems.Milestones.AddNew();
			var dummy2ProcessTask = dummy2.WorkflowItems.Milestones.AddNew();
			((DummyProcessTask)dummy2ProcessTask).OverriddenParentTypeForTest = typeof(DummyWithWorkflowAndJobNumberForWorkflow);

			AssertEquals("JOB1111", dummy1ProcessTask.JobNumber);
			AssertEquals("DUMMY2", dummy2ProcessTask.JobNumber);
		}

		public void TestVisualBoardNoteText_OrderOfPrecedence()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = (BMSystem)helper.CreateSystem(Factory, "ORG");
			system.FS_ResourceCountdownHours = new ZInt(60).GetDateTimeFromMinutes();

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "COD";

			var workflow = (IProcessHeader)ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			var task1 = (ProcessTask)helper.CreateTask(workflow, resource1.GS_Code);

			var task2 = (ProcessTask)helper.CreateTask(workflow);
			task2.P9_GS_NKAssignedStaffMember = ZString.Empty;

			AssertEquals(ZString.Empty, task2.VisualBoardNoteText);

			task2.P9_G4_RequiredCapability = capability.PK;

			AssertEquals("Requires COD capability", task2.VisualBoardNoteText);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task1.P9_EstimatedHandoverTimeUtc = ZDateTime.UtcNow;
			task2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			task2.P9_CardNote = "Dat card note";

			AssertEquals("Dat card note", task2.VisualBoardNoteText);
		}

		public void TestCreateTasksAndMilestonesFromTemplateIfRequired_DbHits()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var system = helper.CreateSystem(Factory, "INQ");
			var bucket1 = helper.CreateBucket(system, "bucket1");
			var bucket2 = helper.CreateBucket(system, "bucket2");
			helper.LinkComponents(bucket1, bucket2);

			var tagGroup = helper.CreateTagDefinition(Factory, "NAM");
			var tag1 = helper.CreateTagMagnitude(tagGroup, "111");
			var tag2 = helper.CreateTagMagnitude(tagGroup, "222");
			var tag3 = helper.CreateTagMagnitude(tagGroup, "333");

			var tags = new[] { tag1, tag2, tag3 };

			var template1 = CreateInquiryTemplate(helper, tags, "template1", "CCR");
			var template2 = CreateInquiryTemplate(helper, tags, "template2", "CCR", "WEB");
			var template3 = CreateInquiryTemplate(helper, tags, "template3", "CCR", "WEB", "NEW");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var job = newFactory.New<SalesEnquiry>();
			job.O1_EnquiryType = "CCR";
			job.O1_LeadSource = "WEB";

			new ProcessTask.Loader(newFactory).CreateTasksAndMilestonesFromTemplateIfRequired(job);

			var hits = new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMComponentLinkSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleSchema.Constants.TableName, 1 },
				{ TagDefinitionSchema.Constants.TableName, 2 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ TagMagnitudeSchema.Constants.TableName, 1 },
			};

			AssertDbHits(hits, newFactory);

			newFactory.Save();

			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			AssertNotNull(jobHeader);
			AssertEquals(6, jobHeader.ProcessHeaders.Count);
			AssertStartsWith("Should have found the most-specific workflow template", "template3", jobHeader.ProcessHeaders[0].FH_CompletionStatement);
		}

		ProcessTaskTemplate CreateInquiryTemplate(IBMTestHelper helper, ITagMagnitude[] tags, string workflowNamePrefix, string inquiryType, string source = null, string organisation = null)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INQ";
			template.P0_SubType1 = inquiryType;
			template.P0_SubType2 = source;
			template.P0_SubType3 = organisation;

			var workflow1 = helper.CreateWorkflow(template, workflowNamePrefix + " workflow1");
			var workflow2 = helper.CreateWorkflow(template, workflowNamePrefix + " workflow2");
			var workflow3 = helper.CreateWorkflow(template, workflowNamePrefix + " workflow3");
			var workflow4 = helper.CreateWorkflow(template, workflowNamePrefix + " workflow4");
			var workflow5 = helper.CreateWorkflow(template, workflowNamePrefix + " workflow5");
			var workflow6 = helper.CreateWorkflow(template, workflowNamePrefix + " workflow6");

			workflow1.AddTag(tags[0]);
			workflow1.AddTag(tags[1]);
			workflow2.AddTag(tags[1]);
			workflow2.AddTag(tags[2]);
			workflow3.AddTag(tags[0]);
			workflow3.AddTag(tags[1]);

			var dependencyLink1 = helper.CreateDependencyLink(template, workflow1, workflow2);
			var dependencyLink2 = helper.CreateDependencyLink(template, workflow2, workflow3);
			var dependencyLink3 = helper.CreateDependencyLink(template, workflow3, workflow4);
			var parentChildLink1 = helper.CreateParentChildLink(template, workflow4, workflow5);
			var parentChildLink2 = helper.CreateParentChildLink(template, workflow5, workflow6);

			var task1 = template.WorkflowItems.Tasks.AddNew();
			var task2 = template.WorkflowItems.Tasks.AddNew();
			var task3 = template.WorkflowItems.Tasks.AddNew();
			var task4 = template.WorkflowItems.Tasks.AddNew();
			var task5 = template.WorkflowItems.Tasks.AddNew();
			var task6 = template.WorkflowItems.Tasks.AddNew();

			task1.P9_FH_ProcessHeader = workflow1.PK;
			task2.P9_FH_ProcessHeader = workflow2.PK;
			task3.P9_FH_ProcessHeader = workflow3.PK;
			task4.P9_FH_ProcessHeader = workflow4.PK;
			task5.P9_FH_ProcessHeader = workflow5.PK;
			task6.P9_FH_ProcessHeader = workflow6.PK;

			return template;
		}

		public void TestDelete_ShouldThrowInvalidOperationException_WhenCanotDelete()
		{
			var processTaskHelperMock = new Mock<IProcessTaskHelper>();
			processTaskHelperMock.Setup(h => h.CanDelete(It.IsAny<IProcessTask>())).Returns(false);

			ObjectFactory.Substitute(processTaskHelperMock.Object);

			var task = Factory.New<ProcessTask>();
			task.P9_Description = "description";

			AssertExceptionThrown<InvalidOperationException>("Should not delete it!", $"Trying to delete a task in service task that NEVER should delete tasks, PK:{task.PK}, Description:description", () => task.Delete());
		}

		#endregion

		#region Status Change Responders

		[ExpectNoExceptions]
		public void TestChangeStatus_ShouldInvokeStatusChangeResponders()
		{
			var responder = new Mock<IStatusChangeResponder>();

			var task = Factory.New<ProcessTask>();

			using (ObjectFactory.Substitute("TaskStatusChangeResponders", new ArrayList { responder.Object }))
			{
				responder.Setup(m => m.RespondsToStatusChange(task, "ASN")).Returns(true);
				responder.Setup(m => m.RespondToChange(task, "ASN")).Returns(StatusChangeResult.ChangeHandled);
				task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			}
		}

		[GuiTest]
		public void TestContainmentBarrierStatusChangeResponder_DoShowInClientSideTriggerAction()
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				BMTestHelper.EnableBMSInRegistry();

				var org = Factory.NewWithValidTestData<OrgHeader>();
				MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("ABC", org.GetWorkflowType());
				var task1 = org.WorkflowItems.Tasks.AddNew();
				task1.P9_Type = "ABC";
				var task2 = org.WorkflowItems.Tasks.AddNew();
				task2.P9_Type = "ABC";
				task2.P9_Description = "TASK";
				AssertEquals(true, task1.IsQualityContainmentBarrierTask());

				var responder = ObjectFactory.Get<IStatusChangeResponder>("ContainmentBarrierStatusChangeResponder");
				var view = new Mock<IContainmentBarrierResponseView>();
				using (ObjectFactory.Substitute("TaskStatusChangeResponders", new ArrayList { responder }))
				using (ObjectFactory.Substitute(view.Object))
				{
					var containmentBarrierFormShown = false;
					view.Setup(o => o.GetResponseFromUser()).Callback(() => containmentBarrierFormShown = true);

					task1.P9_Status = "CLS";
					AssertEquals(true, containmentBarrierFormShown);

					containmentBarrierFormShown = false;
					var trigger = org.WorkflowItems.Triggers.AddNew();
					trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
					var action1 = trigger.ProcessTaskNotifications.AddNew();
					action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
					action1.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Description>\"==\"TASK\").P9_Status";
					action1.PQ_FieldValue = "CLS";

					org.Logs.AddNew(Events.CustomisableEvent01);
					AssertEquals(false, containmentBarrierFormShown);
					AssertEquals("Asserting IFC action worked", "CLS", task2.P9_Status);
				}
			}
		}

		#endregion

		#region Everything should be in a region, right?

		public void TestWTEEventTimeShouldBeTheSameAsEvent()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_GoodsDescription = "test";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Factory.Save();

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			var trigger = shipmentAsWorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test ADD";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;

			var log = shipmentAsWorkflowProvider.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow);

			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var wteLogs = Factory.Load<StmALog>(query);

			AssertEquals("Two WTE events", 1, wteLogs.Length);
			AssertEquals("Two WTE events", log.SL_EventTimeUtc, wteLogs[0].SL_EventTimeUtc);
			AssertEquals("Two WTE events", log.SL_EventTimeOffset, wteLogs[0].SL_EventTimeOffset);
		}

		[TestDate(1995, 12, 23, 0, 0, 0)]
		public void TestStandardEstimateDurationCalculation()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = new ZDateTime(1995, 1, 1, 1, 0, 0);
			task.P9_EstimateVariationFactor = 2;
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(90), task.StandardEstimatedDuration);

			task.P9_EstDuration = new ZDateTime(1995, 1, 4, 4, 0, 0); // 100 hours.
			task.P9_EstimateVariationFactor = 999;
			AssertEquals(4417.5, task.StandardEstimatedDuration.ToTimeSpan().TotalHours);
		}

		public void TestWTERefferenceContainsUserContext_Specified()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "DEF";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "GHI";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "ABC";
			staff.GS_FullName = "President Alex";

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;
			branch2.GB_Code = "UVW";
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "XYZ";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "RST";
			staff2.GS_LoginName = "RST";
			staff2.GS_FullName = "President Joe";

			Factory.Save();

			var parentBO = Factory.New<DummyWithWorkflow>();

			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;

			trigger.TriggerConditions.TriggerStaffCode = staff.GS_Code;
			trigger.TriggerConditions.TriggerBranch = branch.PK;
			trigger.TriggerConditions.TriggerDepartment = department.PK;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "FLD";
			action.PQ_FieldName = "Z0_Description";
			action.PQ_FieldValue = "XXX";

			using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), branch2.PK.ToGuid(), department2.PK.ToGuid()))
			{
				parentBO.GetLogs().AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow);
			}

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "WTE");
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var workflowTriggerEvents = Factory.Load<StmALog>(query);
			AssertEquals("WTE linked to Trigger", 1, workflowTriggerEvents.Length);

			var workflowTriggerEvent = workflowTriggerEvents[0];

			CombineAssertions(() =>
			{
				AssertContains("ABC|DEF|GHI", workflowTriggerEvent.SL_Reference);
				AssertEquals(staff2.GS_Code, workflowTriggerEvent.SL_GS_NKUser);
				AssertEquals(branch2.GB_Code, workflowTriggerEvent.SL_GB_NKBranch);
				AssertEquals(department2.GE_Code, workflowTriggerEvent.SL_GE_NKDepartment);
			});
		}

		public void TestWTERefferenceContainsUserContext_Event()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "DEF";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "GHI";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "ABC";
			staff.GS_FullName = "President Alex";

			Factory.Save();

			var parentBO = Factory.New<DummyWithWorkflow>();

			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "FLD";
			action.PQ_FieldName = "Z0_Description";
			action.PQ_FieldValue = "XXX";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				parentBO.GetLogs().AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow);
			}

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "WTE");
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var workflowTriggerEvents = Factory.Load<StmALog>(query);
			AssertEquals("WTE linked to Trigger", 1, workflowTriggerEvents.Length);

			var workflowTriggerEvent = workflowTriggerEvents[0];

			CombineAssertions(() =>
			{
				AssertContains("ABC|DEF|GHI", workflowTriggerEvent.SL_Reference);
				AssertEquals(staff.GS_Code, workflowTriggerEvent.SL_GS_NKUser);
				AssertEquals(branch.GB_Code, workflowTriggerEvent.SL_GB_NKBranch);
				AssertEquals(department.GE_Code, workflowTriggerEvent.SL_GE_NKDepartment);
			});

			var logwalker = MasterFilesTestHelper.RunLogWalker();
		}

		public void TestWTERefferenceContainsUserContext_Default()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_Code = "DEF";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "GHI";
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "ABC";
			staff.GS_FullName = "President Alex";

			Factory.Save();

			ProcessTask trigger;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var parentBO = Factory.New<DummyWithWorkflow>();

				trigger = parentBO.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
				trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Default;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = "FLD";
				action.PQ_FieldName = "Z0_Description";
				action.PQ_FieldValue = "XXX";

				parentBO.GetLogs().AddNew(Events.CustomisableEvent00, ZDateTimeOffset.UtcNow);
			}

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "WTE");
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var workflowTriggerEvents = Factory.Load<StmALog>(query);
			AssertEquals("WTE linked to Trigger", 1, workflowTriggerEvents.Length);

			var workflowTriggerEvent = workflowTriggerEvents[0];

			CombineAssertions(() =>
			{
				AssertContains("ABC|DEF|GHI", workflowTriggerEvent.SL_Reference);
				AssertEquals(staff.GS_Code, workflowTriggerEvent.SL_GS_NKUser);
				AssertEquals(branch.GB_Code, workflowTriggerEvent.SL_GB_NKBranch);
				AssertEquals(department.GE_Code, workflowTriggerEvent.SL_GE_NKDepartment);
			});
		}

		public void TestShouldNotBeAnyMoreWorkFlowTriggerEventsLinkedToTriggerAfterParentBO_Logs_GetAllLogsIsCalled()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();

			var trigger = ((IWorkflowProvider)parentBO).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			trigger.ProcessTaskNotifications.AddNew();

			var actualTime = trigger.P9_ActualDate;
			parentBO.GetLogs().AddNew(Events.Authorised, ZDateTimeOffset.UtcNow);
			Factory.Save();

			AssertNotEquals("Process Task's actual day should change when add new event to business object", actualTime, trigger.P9_ActualDate);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "WTE");
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var workflowTriggerEvents = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, workflowTriggerEvents.Length);
			actualTime = trigger.P9_ActualDate;

			var reloadFactory = new BusinessObjectFactory();
			var reloadedParentBO = reloadFactory.Load<DummyWithWorkflow>(parentBO.PK);
			reloadedParentBO.Logs.GetAllLogs();
			workflowTriggerEvents = reloadFactory.Load<StmALog>(query);
			AssertEquals("Should not be any more WorkFlowTrigger Events linked to our Trigger after ParentBO.Logs.GetAllLogs() is called.", 1, workflowTriggerEvents.Length);
		}

		[ExpectNoExceptions]
		public void TestIsMilestoneDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			var task = Dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			task.Delete();

			bool dummyBool = task.IsMilestone;
		}

		public void TestProcessTaskDeletesHiddenNotes()
		{
			var task = Dummy.WorkflowItems.Milestones.AddNew();
			task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task.TemplateConditions.TemplateCondition2Value = "Biggybiggybiggy";

			AssertEquals(1, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, task.PK).AddToFilter(StmNoteSchema.ST_Table, "ProcessTasks")).Length);
			task.Delete();
			AssertEquals(0, Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, task.PK).AddToFilter(StmNoteSchema.ST_Table, "ProcessTasks")).Length);
		}

		#endregion

		#region Readonly fields

		public void TestReadOnly_P9_IsResetBeingAppliedToThisTask()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals(true, task.P9_IsResetBeingAppliedToThisTaskInfo.ReadOnly);
		}

		public void TestReadonlyDatesAndDurationsForStandAloneTasks()
		{
			var task = Factory.New<ProcessTask>();
			Assert(!task.P9_ScheduledDateInfo.ReadOnly);
			Assert(!task.P9_EstDurationInfo.ReadOnly);
			Assert(!task.P9_ActualDateInfo.ReadOnly);
			Assert(!task.P9_CompletedTimeUtcInfo.ReadOnly);
			Assert(!task.P9_ActualDurationInfo.ReadOnly);
		}

		public void TestReadonlyAuditFields()
		{
			var task = Factory.New<ProcessTask>();
			Assert(task.P9_SystemCreateTimeUtcInfo.ReadOnly);
			Assert(task.P9_SystemLastEditTimeUtcInfo.ReadOnly);
			Assert(task.P9_SystemCreateUserInfo.ReadOnly);
			Assert(task.P9_SystemLastEditUserInfo.ReadOnly);
		}

		public void TestReadonlyDatesAndDurationsForTemplateTasks()
		{
			var task = Factory.New<TemplateProcessTask>();
			task.P9_ParentTableCode = "P0";
			Assert(task.P9_ScheduledDateInfo.ReadOnly);
			Assert(!task.P9_EstDurationInfo.ReadOnly);
			Assert(task.P9_ActualDateInfo.ReadOnly);
			Assert(task.P9_CompletedTimeUtcInfo.ReadOnly);
			Assert(task.P9_ActualDurationInfo.ReadOnly);
		}

		public void TestReadonlyDates_ForAudittriggers()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			foreach (var code in Events.ChangeLogs.Select(c => c.Code))
			{
				trigger.TriggerConditions.TriggerEventCode = code;
				AssertEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);

				Assert(trigger.P9_ScheduledDateInfo.ReadOnly);
				Assert(trigger.P9_ActualDateInfo.ReadOnly);
				Assert(trigger.P9_ScheduledDateForBindingInfo.ReadOnly);
				Assert(trigger.P9_ActualDateForBindingInfo.ReadOnly);
				Assert(trigger.P9_OriginalScheduledDateUtcInfo.ReadOnly);
				Assert(trigger.P9_OriginalScheduledDateLocalForBindingInfo.ReadOnly);

				dummy.Logs.AddNew(Events.All[code]);
				AssertNotEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
				trigger = dummy.WorkflowItems.Triggers.AddNew();
			}
		}

		[TestDate(2025, 3, 6, 0, 0, 0)]
		public void TestEditingChildrenUpdatesAuditColumns()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();

			Factory.Save();

			AssertEquals("Initial edit time", new ZDateTime(2025, 3, 6, 0, 0, 0), trigger.P9_SystemLastEditTimeUtc);

			TestDateAttribute.AddMinutes(15);
			action.PQ_FieldName = "Z0_Description";
			Factory.Save();

			AssertEquals("Editing child action should update edit time", new ZDateTime(2025, 3, 6, 0, 15, 0), trigger.P9_SystemLastEditTimeUtc);
		}

		public void TestP9_ActualDateUpdateType_ReadOnly()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var milestone = template.WorkflowItems.Milestones.AddNew();
			AssertEquals("Never read only on templates", false, milestone.P9_ActualDateUpdateTypeInfo.ReadOnly);
			Factory.Save();
			AssertEquals("Never read only on templates", false, milestone.P9_ActualDateUpdateTypeInfo.ReadOnly);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();
			AssertEquals(1, job.WorkflowItems.Milestones.Count);
			var milestoneFromTemplate = job.WorkflowItems.Milestones.First() as ProcessTask;
			AssertEquals("Always readonly on tasks created from templates", true, milestoneFromTemplate.P9_ActualDateUpdateTypeInfo.ReadOnly);

			var manualMilestone = job.WorkflowItems.Milestones.AddNew();
			AssertEquals("Manually created tasks are editable until the first save", false, manualMilestone.P9_ActualDateUpdateTypeInfo.ReadOnly);
			Factory.Save();
			AssertEquals("Manually created tasks are editable until the first save", true, manualMilestone.P9_ActualDateUpdateTypeInfo.ReadOnly);
		}

		public void TestCapabilityCode_ReadOnly()
		{
			var bizo = Factory.New<DummyWithWorkflow>();
			var trigger = bizo.WorkflowItems.Triggers.AddNew();
			var task = bizo.WorkflowItems.Tasks.AddNew();
			var milestone = bizo.WorkflowItems.Milestones.AddNew();
			var exception = bizo.WorkflowItems.Exceptions.AddNew();

			Assert(!task.CapabilityCodeInfo.ReadOnly);
			Assert(trigger.CapabilityCodeInfo.ReadOnly);
			Assert(milestone.CapabilityCodeInfo.ReadOnly);
			Assert(exception.CapabilityCodeInfo.ReadOnly);

			Assert(!task.P9_G4_RequiredCapabilityInfo.ReadOnly);
			Assert(trigger.P9_G4_RequiredCapabilityInfo.ReadOnly);
			Assert(milestone.P9_G4_RequiredCapabilityInfo.ReadOnly);
			Assert(exception.P9_G4_RequiredCapabilityInfo.ReadOnly);
		}

		#endregion

		#region BusinessObject Overrides

		public void TestDefaultValues()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
			AssertEquals(GlbCompany.CurrentCompany.PK, task.P9_GC);
			AssertEquals(2m, task.P9_EstimateVariationFactor);

			TemplateProcessTask templateTask = Factory.New<TemplateProcessTask>();
			AssertNotEquals(GlbCompany.CurrentCompany.PK, templateTask.P9_GC);
		}

		public void TestProcessHeaderSetOnLoad_Job()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = (BMSystem)helper.CreateSystem(Factory, "DUM");

			var dummy = Factory.New<DummyWithWorkflow>();
			var task1 = dummy.WorkflowItems.Tasks.AddNew();
			AssertNotNull("Defaults to auto-created Header", task1.ProcessHeader);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory);
			var header = jobHeader.ProcessHeaders[0];
			var task2 = dummy.WorkflowItems.Tasks.AddNew();
			AssertEquals(header, task2.ProcessHeader);

			var task3 = dummy.WorkflowItems.Tasks.AddNew();
			AssertEquals(header, task2.ProcessHeader);

			header.FH_CompletionStatement = "blah";
			AssertEquals("Should track header after changing name", header, task3.ProcessHeader);

			header.FH_CompletionStatement = BMGlobalConstants.DefaultWorkflowCompletionStatement;
			Factory.Save();

			var task1Reloaded = new BusinessObjectFactory().Load<ProcessTask>(task1.PK);
			AssertEquals("Set on load even for existing items", header.PK, task1Reloaded.ProcessHeader.PK);
			AssertEquals(false, task1Reloaded.HasChanges);
		}

		public void TestProcessHeaderSetOnLoad_Template()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			BMTestHelper.CreateSystem(Factory, "WKI");

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "WKI";

			var task1 = template.WorkflowItems.Tasks.AddNew();
			AssertNull(task1.ProcessHeader);

			var header = template.ProcessHeaders.AddNew();
			var task2 = template.WorkflowItems.Tasks.AddNew();
			AssertEquals(header, task2.ProcessHeader);

			var header2 = template.ProcessHeaders.AddNew();
			var task3 = template.WorkflowItems.Tasks.AddNew();
			AssertNull("Only defaults if a single header exists", task3.ProcessHeader);

			header2.Delete();
			Factory.Save();

			var task1Reloaded = new BusinessObjectFactory().Load<ProcessTask>(task1.PK);
			AssertEquals("Set on load even for existing items", header.PK, task1Reloaded.ProcessHeader.PK);
			AssertEquals(false, task1Reloaded.HasChanges);
		}

		public void TestLogging()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				ProcessTask task = Factory.NewWithValidTestData<ProcessTask>();
				Factory.Save();

				AssertEquals("Autolog event reference description", "Task " + task.P9_TaskID, task.Logs.AutoCreatedLog.SL_Reference);
			}
		}

		public void TestClone()
		{
			ProcessTask task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task.P9_Type = "ZZZ";
			task.P9_TaskCannotBeDeleted = true;

			var rando = new Random();
			task.P9_NotesAsString = new string(Enumerable.Range(0, 10000).Select(new Func<int, char>(_ => (char)('a' + rando.Next(22)))).ToArray());
			Factory.Save();
			Assert(!task.P9_TaskID.IsEmpty);

			ProcessTask newTask = TemplateProcessTaskCopier.Clone(task, typeof(OpportunityProcessTasks));
			Assert("New Task ID is cleared", newTask.P9_TaskID.IsEmpty);
			AssertEquals("Status still there", ProcessTaskStatusCodeList.Codes.Cancelled, newTask.P9_Status);
			AssertEquals("Type", typeof(OpportunityProcessTasks), newTask.GetType());

			AssertEquals("Parent Table Code empty", ZString.Empty, newTask.P9_ParentTableCode);
			AssertEquals("Parent ID empty", ZGuid.Empty, newTask.P9_ParentID);
			AssertEquals("Origin Country empty", ZString.Empty, newTask.P9_RN_NKOriginCountry);
			AssertEquals("Destination Country Code empty", ZString.Empty, newTask.P9_RN_NKDestinationCountry);
			AssertEquals(GlbCompany.CurrentCompany.PK, newTask.P9_GC);
			AssertEquals(task.P9_Condition2Value, newTask.P9_Condition2Value);
			AssertEquals(task.P9_Notes, newTask.P9_Notes);
			AssertEquals(task.P9_TaskCannotBeDeleted, newTask.P9_TaskCannotBeDeleted);

			AssertNoErrors("Validation should be suspended ", newTask.P9_TypeInfo);
		}

		public void TestClone_HasChanges()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task.P9_Type = "ZZZ";
			Factory.Save();

			var clonedTask = (ProcessTask)task.Clone();

			Assert(clonedTask.HasChanges);
		}

		public void TestClone_FromTemplateProcessTask()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTask templateTask = template.WorkflowItems.Tasks.AddNew();
			AssertEquals("Sanity check", ZGuid.Empty, templateTask.P9_GC);

			DummyProcessTask dummyTask = TemplateProcessTaskCopier.Clone<DummyProcessTask>(templateTask);
			AssertEquals("ZGuid.Empty should not be copied from the TemplateTask", GlbCompany.CurrentCompany.PK, dummyTask.P9_GC);

			templateTask.P9_RecalculateScheduledDate = true;
			Assert(TemplateProcessTaskCopier.Clone<DummyProcessTask>(templateTask).P9_RecalculateScheduledDate);

			templateTask.P9_RecalculateScheduledDate = false;
			Assert(!TemplateProcessTaskCopier.Clone<DummyProcessTask>(templateTask).P9_RecalculateScheduledDate);
		}

		public void TestClone_FromTemplateIgnoresStatus()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Status = "CLS";
			templateTask.TaskProperties.ActualDate = ZDateTimeOffset.Now;

			var clonedTask = TemplateProcessTaskCopier.Clone<DummyProcessTask>(templateTask);
			AssertEquals("Should clone for tasks", "CLS", clonedTask.P9_Status);
			AssertEquals("Should clone for tasks", templateTask.P9_ActualDate, clonedTask.P9_ActualDate);

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.P9_Status = "CLS";
			templateTrigger.SetMilestoneActualDateForTest(ZDateTime.Now);

			var clonedTrigger = TemplateProcessTaskCopier.Clone<DummyProcessTask>(templateTrigger);
			AssertEquals("Should ignore for trigger", "OPN", clonedTrigger.P9_Status);
			AssertEquals("Should ignore for trigger", ZDateTime.Empty, clonedTrigger.P9_ActualDate);

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.P9_Status = "CLS";
			templateMilestone.SetMilestoneActualDateForTest(ZDateTime.Now);

			var clonedMilestone = TemplateProcessTaskCopier.Clone<DummyProcessTask>(templateMilestone);
			AssertEquals("Should ignore for milestone", "OPN", clonedMilestone.P9_Status);
			AssertEquals("Should ignore for milestone", ZDateTime.Empty, clonedMilestone.P9_ActualDate);
		}

		public void TestCloneNotes()
		{
			ProcessTask task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Notes = new ZBlob(new byte[5]);
			ProcessTask clonedTask = (ProcessTask)task.Clone();
			AssertEquals("Clone of a ProcessTask should include P9_Notes", 5, clonedTask.P9_Notes.Length);

			task.IsMilestone = true;
			ProcessTask clonedMilestone = (ProcessTask)task.Clone();
			AssertEquals("Clone of a milestone should include P9_Notes", 5, clonedMilestone.P9_Notes.Length);

			task.IsWorkflowTrigger = true;
			ProcessTask clonedTrigger = (ProcessTask)task.Clone();
			AssertEquals("Clone of a trigger should include P9_Notes", 5, clonedTrigger.P9_Notes.Length);
		}

		public void TestClone_OpenStatus()
		{
			var task = Dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var clonedTask = (ProcessTask)task.Clone();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, task.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Open, clonedTask.P9_Status);
		}

		public void TestClone_WorkingStatus()
		{
			var task = Dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var clonedTask = (ProcessTask)task.Clone();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, clonedTask.P9_Status);
		}

		public void TestClone_NonTask()
		{
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var clonedTrigger = (ProcessTask)trigger.Clone();

			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, trigger.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, clonedTrigger.P9_Status);
		}

		public void TestClone_WithUDFSAMTaskAssignmentRestriction()
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry("WKI", "CDU");
			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new TaskTypeRestrictionsCollection());
			var collection = WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.Value;

			var restriction = collection.AddNew();
			restriction.Active = true;
			restriction.WorkflowType = "WKI";
			restriction.TaskType = "UDF";
			restriction.NotificationType = NotificationTypeList.Codes.None;
			restriction.Scope = ScopeList.Codes.Job;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var cduTaskType = restriction.TaskTypesCollection.AddNew();
			cduTaskType.Code = "CDU";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var processTask = Dummy.WorkflowItems.Tasks.AddNew();
			processTask.P9_GS_NKAssignedStaffMember = "ARA";
			processTask.P9_Description = "Testing description";

			var clonedTask = (ProcessTask)processTask.Clone();

			AssertEquals("Error should not be reported as cloning should not trigger SAM task restrictions.", 0, ErrorReporter.TotalErrorCount);
			TestClone();
		}

		public void TestValidation()
		{
			ProcessTask.IsMilestone = false;
			ProcessTask.IsException = false;
			AssertEquals(typeof(ProcessTaskValidation), ProcessTask.Validation.GetType());

			ProcessTask.IsMilestone = true;
			ProcessTask.IsException = false;
			AssertEquals(typeof(MilestoneOrTriggerValidation), ProcessTask.Validation.GetType());

			ProcessTask.IsMilestone = false;
			ProcessTask.IsException = true;
			AssertEquals(typeof(ExceptionValidation), ProcessTask.Validation.GetType());

			ProcessTask.IsException = false;
			ProcessTask.IsWorkflowTrigger = true;
			AssertEquals(typeof(MilestoneOrTriggerValidation), ProcessTask.Validation.GetType());
		}

		[TestDate(2000, 1, 1, 13, 0, 0)]
		public void TestMilestoneExceptionCreatedOnSaveIfRequired()
		{
			ProcessTask.IsMilestone = true;
			Factory.Save();
			AssertEquals("No exceptions initially", 0, Dummy.WorkflowItems.Exceptions.Count);

			ProcessTask.SetMilestoneActualDateForTest(ZDateTime.Today);
			ProcessTask.P9_ScheduledDate = ZDateTime.Today.AddDays(-1);
			Factory.Save();
			AssertEquals("No exceptions created if the actual date is populated", 0, Dummy.WorkflowItems.Exceptions.Count);

			ProcessTask.SetMilestoneActualDateForTest(ZDateTime.Empty);
			ProcessTask.P9_ScheduledDate = new ZDateTime(2000, 1, 1, 0, 0, 0);
			TestDateAttribute.Date = new DateTime(2000, 1, 1, 23, 0, 0);
			Factory.Save();
			AssertEquals("Exception not created until the next day after the estimated date.", 0, Dummy.WorkflowItems.Exceptions.Count);

			TestDateAttribute.Date = new DateTime(2000, 1, 2, 0, 0, 0);
			ProcessTask.P9_Description = "Changed";
			Factory.Save();
			AssertEquals(
				"Exception created the day after the estimated date. This behaviour is to work around timezone related issues, and to be consistent with exceptions created by the batch processor.",
				1, Dummy.WorkflowItems.Exceptions.Count);
		}

		public void TestOnlyExceptionCanHaveIsExceptionActionedSet()
		{
			ProcessTask milestoneA = Dummy.WorkflowItems.AddNew();
			milestoneA.IsMilestone = true;
			milestoneA.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestoneA.TriggerConditions.TriggerConditionValue = "CUS";
			milestoneA.P9_SE_NKExceptionEvent = "EXC";
			milestoneA.P9_Description = "RECEIVED CUS";
			milestoneA.TriggerConditions.TriggerEventCode = "DDI";
			milestoneA.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);
			AssertEquals(false, milestoneA.IsException);
			milestoneA.IsExceptionActioned = true;
			AssertEquals(false, milestoneA.IsExceptionActioned);
			AssertEquals(false, milestoneA.IsException);
		}

		public void TestMilestonesWithSameEventButDifferentDescriptionsGenerateDifferentExceptions()
		{
			ProcessTask milestoneA = Dummy.WorkflowItems.AddNew();
			milestoneA.IsMilestone = true;
			milestoneA.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestoneA.TriggerConditions.TriggerConditionValue = "CUS";
			milestoneA.P9_SE_NKExceptionEvent = "EXC";
			milestoneA.P9_Description = "RECEIVED CUS";
			milestoneA.TriggerConditions.TriggerEventCode = "DDI";
			milestoneA.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			ProcessTask milestoneB = Dummy.WorkflowItems.AddNew();
			milestoneB.IsMilestone = true;
			milestoneB.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestoneB.TriggerConditions.TriggerConditionValue = "FUM";
			milestoneB.P9_SE_NKExceptionEvent = "EXC";
			milestoneB.P9_Description = "RECEIVED FUM";
			milestoneB.TriggerConditions.TriggerEventCode = "DDI";
			milestoneB.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			ProcessTask milestoneNoDesc = Dummy.WorkflowItems.AddNew();
			milestoneNoDesc.IsMilestone = true;
			milestoneNoDesc.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestoneB.TriggerConditions.TriggerConditionValue = "123";
			milestoneNoDesc.P9_SE_NKExceptionEvent = "EXC";
			milestoneNoDesc.P9_Description = "";
			milestoneNoDesc.TriggerConditions.TriggerEventCode = "DDI";
			milestoneNoDesc.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			AssertEquals("2 exceptions should be raised", 3, Dummy.WorkflowItems.Exceptions.Count);
			AssertEquals("Exceptions not actioned initially", false, Dummy.WorkflowItems.Exceptions[0].IsExceptionActioned);
			AssertEquals("Exceptions not actioned initially", false, Dummy.WorkflowItems.Exceptions[1].IsExceptionActioned);
			AssertEquals("Exceptions not actioned initially", false, Dummy.WorkflowItems.Exceptions[2].IsExceptionActioned);

			milestoneA.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();
			AssertEquals("Exception actioned when milestone is met", true, Dummy.WorkflowItems.Exceptions[0].IsExceptionActioned);
			AssertEquals("Second exception not actioned", false, Dummy.WorkflowItems.Exceptions[1].IsExceptionActioned);
			AssertEquals("Third exception not actioned", false, Dummy.WorkflowItems.Exceptions[2].IsExceptionActioned);

			milestoneNoDesc.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();
			AssertEquals("Second exception not actioned", false, Dummy.WorkflowItems.Exceptions[1].IsExceptionActioned);
			AssertEquals("Third exception actioned", true, Dummy.WorkflowItems.Exceptions[2].IsExceptionActioned);

			milestoneB.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();
			AssertEquals("Second exception actioned", true, Dummy.WorkflowItems.Exceptions[1].IsExceptionActioned);
		}

		public void TestActioningExceptionSettlesMatchingMilestoneByDescription()
		{
			ProcessTask milestoneA = Dummy.WorkflowItems.AddNew();
			milestoneA.IsMilestone = true;
			milestoneA.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestoneA.TriggerConditions.TriggerConditionValue = "CUS";
			milestoneA.P9_SE_NKExceptionEvent = "EXC";
			milestoneA.P9_Description = "RECEIVED CUS";
			milestoneA.TriggerConditions.TriggerEventCode = "DDI";
			milestoneA.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			ProcessTask milestoneB = Dummy.WorkflowItems.AddNew();
			milestoneB.IsMilestone = true;
			milestoneB.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestoneB.TriggerConditions.TriggerConditionValue = "FUM";
			milestoneB.P9_SE_NKExceptionEvent = "EXC";
			milestoneB.P9_Description = "RECEIVED FUM";
			milestoneB.TriggerConditions.TriggerEventCode = "DDI";
			milestoneB.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			ProcessTask milestoneNoDesc = Dummy.WorkflowItems.AddNew();
			milestoneNoDesc.IsMilestone = true;
			milestoneNoDesc.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestoneB.TriggerConditions.TriggerConditionValue = "123";
			milestoneNoDesc.P9_SE_NKExceptionEvent = "EXC";
			milestoneNoDesc.TriggerConditions.TriggerEventCode = "DDI";
			milestoneNoDesc.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			AssertEquals("3 exceptions should be raised", 3, Dummy.WorkflowItems.Exceptions.Count);
			AssertEquals("milestoneA has open exception", true, Dummy.WorkflowItems.Milestones[0].IsMilestoneOverdueAndHasOpenException);
			AssertEquals("milestoneB has open exception", true, Dummy.WorkflowItems.Milestones[1].IsMilestoneOverdueAndHasOpenException);
			AssertEquals("milestoneNoRef has open exception", true, Dummy.WorkflowItems.Milestones[2].IsMilestoneOverdueAndHasOpenException);

			Dummy.WorkflowItems.Exceptions[0].IsExceptionActioned = true;
			Factory.Save();
			AssertEquals("milestoneA's exception is settled", false, Dummy.WorkflowItems.Milestones[0].IsMilestoneOverdueAndHasOpenException);
			AssertEquals("milestoneB still has open exception", true, Dummy.WorkflowItems.Milestones[1].IsMilestoneOverdueAndHasOpenException);
			AssertEquals("milestoneNoRef still has open exception", true, Dummy.WorkflowItems.Milestones[2].IsMilestoneOverdueAndHasOpenException);

			Dummy.WorkflowItems.Exceptions[2].IsExceptionActioned = true;
			Factory.Save();
			AssertEquals("milestoneA's exception is still settled", false, Dummy.WorkflowItems.Milestones[0].IsMilestoneOverdueAndHasOpenException);
			AssertEquals("milestoneB's still has open exception", true, Dummy.WorkflowItems.Milestones[1].IsMilestoneOverdueAndHasOpenException);
			AssertEquals("milestoneNoRef's exception is settled", false, Dummy.WorkflowItems.Milestones[2].IsMilestoneOverdueAndHasOpenException);

			Dummy.WorkflowItems.Exceptions[1].IsExceptionActioned = true;
			Factory.Save();
			AssertEquals("milestoneA's exception is still settled", false, Dummy.WorkflowItems.Milestones[0].IsMilestoneOverdueAndHasOpenException);
			AssertEquals("milestoneB's exception is settled", false, Dummy.WorkflowItems.Milestones[1].IsMilestoneOverdueAndHasOpenException);
			AssertEquals("milestoneNoRef's exception is still settled", false, Dummy.WorkflowItems.Milestones[2].IsMilestoneOverdueAndHasOpenException);
		}

		public void TestMilestoneExceptionsMarkedAsActionedOnceMilestoneClosed()
		{
			ProcessTask milestone = Dummy.WorkflowItems.AddNew();
			milestone.IsMilestone = true;
			milestone.TriggerConditions.TriggerEventCode = Events.Booked.Code;
			milestone.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			ProcessTask decoyMilestone = Dummy.WorkflowItems.AddNew();
			decoyMilestone.IsMilestone = true;
			decoyMilestone.TriggerConditions.TriggerEventCode = Events.BookingConfirmed.Code;
			decoyMilestone.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			AssertEquals("2 exceptions should be raised", 2, Dummy.WorkflowItems.Exceptions.Count);
			AssertEquals("Exceptions not actioned initially", false, Dummy.WorkflowItems.Exceptions[0].IsExceptionActioned);
			AssertEquals("Exceptions not actioned initially", false, Dummy.WorkflowItems.Exceptions[1].IsExceptionActioned);
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();
			AssertEquals("Exception actioned when milestone is met", true, Dummy.WorkflowItems.Exceptions[0].IsExceptionActioned);
			AssertEquals("Unrelated exception not actioned", false, Dummy.WorkflowItems.Exceptions[1].IsExceptionActioned);
		}

		[ExpectNoExceptions]
		public void TestMilestoneExceptionLoadsCorrectRow()
		{
			ZGuid parentID = ZGuid.NewZGuid();

			var milestone1 = Factory.New<ProcessTask>();
			milestone1.P9_ParentID = parentID;
			milestone1.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			milestone1.IsMilestone = true;
			milestone1.TriggerConditions.TriggerEventCode = Events.Booked.Code;

			var milestone2 = Factory.New<ProcessTask>();
			milestone2.P9_SE_NKExceptionEvent = milestone1.P9_SE_NKExceptionEvent;
			milestone2.P9_ParentID = parentID;
			milestone2.P9_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix;
			milestone2.IsMilestone = true;
			milestone2.TriggerConditions.TriggerEventCode = Events.Booked.Code;

			var exception = Factory.New<ProcessTask>();
			exception.P9_SE_NKExceptionEvent = milestone1.P9_SE_NKExceptionEvent;
			exception.P9_ParentID = parentID;
			exception.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			exception.IsException = true;
			exception.TriggerConditions.TriggerEventCode = Events.Booked.Code;

			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();

			AssertNotNull("Exception for same parent table is loaded", otherFactory.Load<ProcessTask>(milestone1.PK).MilestoneException);
			AssertNull("There is no excection for different parent table", otherFactory.Load<ProcessTask>(milestone2.PK).MilestoneException);
		}

		public void TestMilestoneExceptionLoadsCorrectType()
		{
			ZGuid parentID = ZGuid.NewZGuid();

			var milestone = Factory.New<ProcessTask>();
			milestone.P9_ParentID = parentID;
			milestone.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			milestone.IsMilestone = true;
			milestone.TriggerConditions.TriggerEventCode = Events.Booked.Code;

			var exception1 = Factory.New<ProcessTask>();
			exception1.P9_SE_NKExceptionEvent = milestone.P9_SE_NKExceptionEvent;
			exception1.P9_ParentID = parentID;
			exception1.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			exception1.IsException = true;
			exception1.TriggerConditions.TriggerEventCode = Events.Booked.Code;

			var exception2 = Factory.New<ProcessTask>();
			exception2.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionFutureEvent;
			exception2.P9_ParentID = parentID;
			exception2.P9_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			exception2.IsException = true;
			exception2.TriggerConditions.TriggerEventCode = Events.Booked.Code;

			Factory.Save();

			var milestoneException = new BusinessObjectFactory().Load<ProcessTask>(milestone.PK).MilestoneException;

			AssertEquals("Expecting exception with P9_SE_NKExceptionEvent = milestone.P9_SE_NKExceptionEvent", exception1.PK, milestoneException?.PK);
		}

		public void TestDelete()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTaskNotification action1 = milestone.ProcessTaskNotifications.AddNew();
			ProcessTaskNotification action2 = milestone.ProcessTaskNotifications.AddNew();

			milestone.Delete();
			Assert("should be deleted", action1.IsDeleted);
			Assert("should be deleted", action2.IsDeleted);
		}

		public void TestDeleteTaskWithIterationLinks()
		{
			BMTestHelper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "DUM");

			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(Factory, "DUM");

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();

			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();

			task1.P9_Type = "QCB";

			var pivot1 = (IProcessTaskIterationLink)task1.IterationLinks.AddNew();
			pivot1.P9I_P9_IterationTask = task2.PK;
			pivot1.P9I_FH_IterationWorkflow = workflow.PK;

			var pivot2 = (IProcessTaskIterationLink)task1.IterationLinks.AddNew();
			pivot2.P9I_P9_IterationTask = task2.PK;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedTask = newFactory.Load<ProcessTask>(task1.PK);
			loadedTask.Delete();

			AssertEquals(false, ((BusinessObject)pivot1).IsDeleted);
			AssertEquals(false, ((BusinessObject)pivot2).IsDeleted);

			newFactory.Save();

			AssertEquals(true, ((BusinessObject)pivot1).IsDeleted);
			AssertEquals(true, ((BusinessObject)pivot2).IsDeleted);
		}

		public void TestDeleteTaskWithIterationLinks_AfterChangingTaskType()
		{
			BMTestHelper.EnableBMSInRegistry();
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "DUM");

			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(Factory, "DUM");

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Cast<IProcessHeader>().Single();

			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();

			task1.P9_Type = "QCB";

			var pivot1 = (IProcessTaskIterationLink)task1.IterationLinks.AddNew();
			pivot1.P9I_P9_IterationTask = task2.PK;
			pivot1.P9I_FH_IterationWorkflow = workflow.PK;

			var pivot2 = (IProcessTaskIterationLink)task1.IterationLinks.AddNew();
			pivot2.P9I_P9_IterationTask = task2.PK;

			task1.P9_Type = "UDF";

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedTask = newFactory.Load<ProcessTask>(task1.PK);
			loadedTask.Delete();

			AssertEquals(false, ((BusinessObject)pivot1).IsDeleted);
			AssertEquals(false, ((BusinessObject)pivot2).IsDeleted);

			newFactory.Save();

			AssertEquals(true, ((BusinessObject)pivot1).IsDeleted);
			AssertEquals(true, ((BusinessObject)pivot2).IsDeleted);
		}

		public void TestDelete_TaskWithIterationLinkPivot_ShouldDeletePivot()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");

			var task1 = (ProcessTask)helper.CreateTask(workflow);
			var task2 = (ProcessTask)helper.CreateTask(workflow);

			var iteration = Factory.New<IProcessTaskIterationLink>();
			iteration.P9I_FH_IterationWorkflow = workflow.PK;
			iteration.P9I_P9_ContainmentBarrierTask = task1.PK;
			iteration.P9I_P9_IterationTask = task2.PK;
			iteration.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			iteration.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
			iteration.P9I_GS_NKResourceUnderReview = GlbStaff.GetCurrentUser(Factory).GS_Code;

			iteration.TaskPivots.AddNewForTask(task1);
			Factory.Save();

			AssertEquals(false, ((BusinessObject)iteration).IsDeleted);

			task1.Delete();

			AssertEquals(true, ((BusinessObject)iteration).IsDeleted);

			AssertNoExceptionThrown("Dependent objects have been deleted, so there should be no exceptions when saving. SAD!", Factory.Save);
		}

		public void TestDelete_ForNonTask_ShouldNotHitIterationPivotTable()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var exception = dummy.WorkflowItems.Exceptions.AddNew();

			using (AssertDbHitsForAllFactories(new Dictionary<string, int> { { ProcessTaskIterationLinkPivotSchema.Constants.TableName, 0 } }, ignoreUnspecified: true))
			{
				milestone.Delete();
				trigger.Delete();
				exception.Delete();
			}
		}

		public void TestClone_WhenTaskHasIterationLinkPivot_ShouldCreateNewTaskWithIterationPivotToSameIteration()
		{
			bool DoesPivotExist(ZGuid taskPK)
			{
				return Factory.LoadTop1<IProcessTaskIterationLinkPivot>(new ZQuery(ProcessTaskIterationLinkPivotSchema.P9P_P9_Task, taskPK)) != null;
			}

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var jobHeader = helper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");

			var task1 = (ProcessTask)helper.CreateTask(workflow);
			AssertEquals(false, DoesPivotExist(task1.PK));

			var clone1 = task1.Clone();
			AssertEquals(false, DoesPivotExist(clone1.PK));

			var iteration = Factory.New<IProcessTaskIterationLink>();
			iteration.P9I_FH_IterationWorkflow = workflow.PK;
			iteration.P9I_P9_ContainmentBarrierTask = task1.PK;
			iteration.P9I_P9_IterationTask = task1.PK;
			iteration.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			iteration.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
			iteration.P9I_GS_NKResourceUnderReview = GlbStaff.GetCurrentUser(Factory).GS_Code;

			iteration.TaskPivots.AddNewForTask(task1);

			AssertEquals(true, DoesPivotExist(task1.PK));

			var clone2 = task1.Clone();
			AssertEquals("If one clones an iteration task, we assume that the cloned task should be in the iteration too, so we should create a pivot for it automatically. SAD!", true, DoesPivotExist(clone2.PK));
		}

		public void TestCanDelete()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			Env.Security.WorkflowTasksDelete.IsAllowed = false;
			Assert(!task.CanDelete);
			Env.Security.WorkflowTasksDelete.IsAllowed = true;
			Assert(task.CanDelete);
		}

		public void TestCanDeleteStandAloneTaskWhenTaskCannotBeDeletedIsSet()
		{
			var taskNoParent = Factory.New<ProcessTask>();
			taskNoParent.P9_TaskCannotBeDeleted = true;

			var taskBadParent = Factory.New<ProcessTask>();
			taskBadParent.P9_ParentID = Guid.NewGuid();
			taskBadParent.P9_ParentTableCode = "OH";
			taskBadParent.P9_TaskCannotBeDeleted = true;

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var taskGoodParent = template.WorkflowItems.AddNew();
			taskGoodParent.P9_TaskCannotBeDeleted = true;
			Factory.Save();

			AssertNull(taskNoParent.Parent);
			AssertEquals(true, taskNoParent.CanDelete);
			AssertNull(taskBadParent.Parent);
			AssertEquals(true, taskBadParent.CanDelete);
			AssertNotNull(taskGoodParent.Parent);
			AssertEquals(false, taskGoodParent.CanDelete);
		}

		public void TestCanDelete_P9_FormflowTypeSet()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("Org1");
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var product = (OrgSupplierPart)helper.CreateProduct(orgPK, "Pro01");
			var receive = (IWorkflowProvider)helper.CreateWhsReceiveWithInventory(orgPK, warehouse.PK, "R1", product.PK, 10m);
			Factory.Save();

			var taskNoFormFlowType = receive.WorkflowItems.AddNew();
			var taskWithFormFlowType = receive.WorkflowItems.AddNew();
			taskWithFormFlowType.P9_FormFlowType = "WUL";
			Factory.Save();

			AssertNotNull(taskNoFormFlowType.Parent);
			AssertEquals(true, taskNoFormFlowType.CanDelete);

			AssertNotNull(taskWithFormFlowType.Parent);
			AssertEquals(false, taskWithFormFlowType.CanDelete);
			AssertEquals("Cannot delete system maintained tasks linked to jobs.", taskWithFormFlowType.ReasonForNotAbleToDelete.ToString());
		}

		public void TestDeleteWithTagLinks()
		{
			var tag = Factory.NewWithValidTestData<TagMagnitude>();
			var job = Factory.New<OrgHeader>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			var task1 = job.WorkflowItems.AddNew();
			var task2 = job.WorkflowItems.AddNew();

			task1.P9_FH_ProcessHeader = workflow.PK;
			task2.P9_FH_ProcessHeader = workflow.PK;

			string failureMessage;
			BatchTagOperator.TryAddTag(tag, new[] { task1, task2 }, out failureMessage);

			AssertEquals(1, ((ITagable)task1).TagLinks.Count);
			AssertEquals(1, ((ITagable)task2).TagLinks.Count);

			task1.Delete();

			AssertEquals(0, ((ITagable)task1).TagLinks.Count);
			AssertEquals(1, ((ITagable)task2).TagLinks.Count);

			job.Delete();

			AssertEquals(0, ((ITagable)task1).TagLinks.Count);
			AssertEquals(0, ((ITagable)task2).TagLinks.Count);
		}

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);

			var processTask = (ProcessTask)info.BizObj;

			Disposables.Add(processTask.TemporarilyAllowSettingCondition(info.Name));
		}

		#endregion

		#region Clone Test

		public void TestCopyClone()
		{
			ZString taskTypeForNotSendingAppointment = "UTF";
			ZString taskTypeForSendingAppointment = "UTA";

			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes parent2 = collection.AddNew();
			parent2.Code = "STA";

			WorkflowTaskType setsAppointment2 = parent2.TaskTypes.AddNew();
			setsAppointment2.Code = taskTypeForSendingAppointment;
			setsAppointment2.Description = (NoResString)"fdsfsdf";
			setsAppointment2.CreatesAppointment = true;

			WorkflowTaskType doesntSetAppointment2 = parent2.TaskTypes.AddNew();
			doesntSetAppointment2.Code = taskTypeForNotSendingAppointment;
			doesntSetAppointment2.Description = (NoResString)"very long descriptionnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn";
			doesntSetAppointment2.CreatesAppointment = false;
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			ProcessTask originalTask = Factory.New<ProcessTask>();
			originalTask.P9_Description = "Test Task";
			originalTask.P9_EstDuration = new ZDateTime(2006, 11, 10);
			originalTask.P9_ScheduledDate = new ZDateTime(2006, 11, 11);
			originalTask.P9_Sequence = 1;
			originalTask.P9_GG_AssignedGroup = ZGuid.NewZGuid();
			originalTask.P9_GS_NKAssignedStaffMember = "ABC";
			originalTask.P9_Type = taskTypeForSendingAppointment;
			originalTask.P9_IsCalendarItem = true;
			originalTask.P9_ParentID = ZGuid.NewZGuid();
			originalTask.P9_ParentTableCode = "OP";
			originalTask.P9_TaskID = "TI0001";

			ProcessTask clonedTask = (ProcessTask)originalTask.Clone();

			AssertEquals(originalTask.WorkflowType, clonedTask.WorkflowType);
			AssertEquals(2, originalTask.Lookups.Types.Count);
			AssertEquals(2, clonedTask.Lookups.Types.Count);
			AssertEquals(originalTask.Lookups.Types[0].Code, clonedTask.Lookups.Types[0].Code);
			AssertEquals(originalTask.Lookups.Types[1].Code, clonedTask.Lookups.Types[1].Code);
			AssertEquals(originalTask.P9_Description, clonedTask.P9_Description);
			AssertEquals(originalTask.P9_EstDuration, clonedTask.P9_EstDuration);
			AssertEquals(originalTask.P9_ScheduledDate, clonedTask.P9_ScheduledDate);
			AssertEquals(originalTask.P9_Sequence, clonedTask.P9_Sequence);
			AssertEquals(originalTask.P9_GG_AssignedGroup, clonedTask.P9_GG_AssignedGroup);
			AssertEquals(originalTask.P9_GS_NKAssignedStaffMember, clonedTask.P9_GS_NKAssignedStaffMember);
			AssertEquals(originalTask.P9_Type, clonedTask.P9_Type);
			AssertEquals(originalTask.P9_IsCalendarItem, clonedTask.P9_IsCalendarItem);
			AssertEquals(originalTask.P9_Notes, clonedTask.P9_Notes);
			AssertEquals(originalTask.P9_OA, clonedTask.P9_OA);
			AssertEquals(originalTask.P9_OC, clonedTask.P9_OC);
			AssertEquals(originalTask.P9_TaskCannotBeDeleted, clonedTask.P9_TaskCannotBeDeleted);
			AssertEquals("ASN", clonedTask.P9_Status);
			AssertNotEquals(originalTask.P9_TaskID, clonedTask.P9_TaskID);
		}

		public void TestCloneOfTriggerDoesNotCloneAssignedStaffMember()
		{
			ProcessTask originalTask1 = Factory.New<ProcessTask>();
			originalTask1.P9_GS_NKAssignedStaffMember = "ABC";
			originalTask1.P9_Type = Core.Constants.Workflow.MilestoneType;

			ProcessTask originalTask2 = Factory.New<ProcessTask>();
			originalTask2.P9_GS_NKAssignedStaffMember = "DEF";
			originalTask2.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

			ProcessTask cloneTask1 = TemplateProcessTaskCopier.Clone(originalTask1, typeof(ProcessTask));
			ProcessTask cloneTask2 = TemplateProcessTaskCopier.Clone(originalTask2, typeof(ProcessTask));

			AssertEquals("Cloned assigned staff for workflow task", originalTask1.P9_GS_NKAssignedStaffMember, cloneTask1.P9_GS_NKAssignedStaffMember);
			AssertNotEquals("Did NOT clone assigned staff for trigger task", originalTask2.P9_GS_NKAssignedStaffMember, cloneTask2.P9_GS_NKAssignedStaffMember);
			Assert("Clone's assigned staff is empty", string.IsNullOrEmpty(cloneTask2.P9_GS_NKAssignedStaffMember));
		}

		#endregion

		#region Property Overrides

		#region Test P9_Status setting to LST and NXT

		public void TestP9_StatusSetting()
		{
			DummyWithWorkflow bizObj = Factory.New<DummyWithWorkflow>();
			ProcessTask milestone1 = bizObj.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = bizObj.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone3 = bizObj.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone4 = bizObj.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = Events.CallBackClient.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone3.TriggerConditions.TriggerEventCode = Events.CargoAvailable.Code;
			milestone4.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;
			Factory.Save();

			AssertEquals("Task1 should have NXT status", ProcessTasks.NextToBeCompletedStatusCode, milestone1.P9_Status);
			AssertEquals("Task2 should have OPN status", ProcessTaskStatusCodeList.Codes.Open, milestone2.P9_Status);
			AssertEquals("Task3 should have OPN status", ProcessTaskStatusCodeList.Codes.Open, milestone3.P9_Status);
			AssertEquals("Task4 should have OPN status", ProcessTaskStatusCodeList.Codes.Open, milestone4.P9_Status);

			milestone1.P9_ScheduledDate = ZDateTime.Today;
			milestone2.P9_ScheduledDate = ZDateTime.Today;
			milestone3.P9_ScheduledDate = ZDateTime.Today;
			milestone4.P9_ScheduledDate = ZDateTime.Today;
			Factory.Save();
			AssertEquals("Task1 should have NXT status", ProcessTasks.NextToBeCompletedStatusCode, milestone1.P9_Status);
			AssertEquals("Task2 should have OPN status", ProcessTaskStatusCodeList.Codes.Open, milestone2.P9_Status);
			AssertEquals("Task3 should have OPN status", ProcessTaskStatusCodeList.Codes.Open, milestone3.P9_Status);
			AssertEquals("Task4 should have OPN status", ProcessTaskStatusCodeList.Codes.Open, milestone4.P9_Status);

			milestone1.SetMilestoneActualDateForTest(ZDateTime.Today);
			Factory.Save();
			AssertEquals("Task1 should have LST status", ProcessTasks.LastCompletedStatusCode, milestone1.P9_Status);
			AssertEquals("Task2 should have NXT status", ProcessTasks.NextToBeCompletedStatusCode, milestone2.P9_Status);
			AssertEquals("Task3 should have OPN status", ProcessTaskStatusCodeList.Codes.Open, milestone3.P9_Status);
			AssertEquals("Task4 should have OPN status", ProcessTaskStatusCodeList.Codes.Open, milestone4.P9_Status);

			milestone2.SetMilestoneActualDateForTest(ZDateTime.Today.AddDays(1));
			Factory.Save();
			AssertEquals("Task1 should have CLS status", ProcessTaskStatusCodeList.Codes.Closed, milestone1.P9_Status);
			AssertEquals("Task2 should have LST status", ProcessTasks.LastCompletedStatusCode, milestone2.P9_Status);
			AssertEquals("Task3 should have NXT status", ProcessTasks.NextToBeCompletedStatusCode, milestone3.P9_Status);
			AssertEquals("Task4 should have OPN status", ProcessTaskStatusCodeList.Codes.Open, milestone4.P9_Status);

			milestone3.SetMilestoneActualDateForTest(ZDateTime.Today.AddDays(2));
			Factory.Save();
			AssertEquals("Task1 should have CLS status", ProcessTaskStatusCodeList.Codes.Closed, milestone1.P9_Status);
			AssertEquals("Task2 should have CLS status", ProcessTaskStatusCodeList.Codes.Closed, milestone2.P9_Status);
			AssertEquals("Task3 should have LST status", ProcessTasks.LastCompletedStatusCode, milestone3.P9_Status);
			AssertEquals("Task4 should have NXT status", ProcessTasks.NextToBeCompletedStatusCode, milestone4.P9_Status);

			milestone2.SetMilestoneActualDateForTest(ZDateTime.Empty);
			bizObj.WorkflowItems.Milestones.Remove(milestone3.PK);
			bizObj.WorkflowItems.Milestones.RemoveAndDelete(milestone4);
			Factory.Save();
			AssertEquals("Task1 should have LST status", ProcessTasks.LastCompletedStatusCode, milestone1.P9_Status);
			AssertEquals("Task2 should have NXT status", ProcessTasks.NextToBeCompletedStatusCode, milestone2.P9_Status);
		}

		#endregion

		public void TesP9_Description()
		{
			AssertEquals("IsTask=true for the test", true, ProcessTask.IsTask);
			AssertEquals("P9_Type empty initially", "", ProcessTask.P9_Type);
			ProcessTask.P9_Description = "Description";
			AssertEquals("P9_Type defaulted for tasks", "UDF", ProcessTask.P9_Type);
		}

		public void TestP9_StatusReadOnlyForFieldChange()
		{
			var bizo = Factory.New<DummyWithWorkflow>();

			var trigger = bizo.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew(); 
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "WorkflowItems.P9_Status";
			action.PQ_FieldValue = "OPN";

			var task = bizo.WorkflowItems.Tasks.AddNew();
			var milestone = bizo.WorkflowItems.Milestones.AddNew();
			var exception = bizo.WorkflowItems.Exceptions.AddNew();
			Factory.Save();

			bizo.Logs.AddNew(Events.CustomisableEvent00);

			AssertHasRowWarningContaining("Warnings for milestone and exception", trigger, "Failure with 2 reasons");
			AssertHasRowWarningContaining("P9_Status on milestone is read-only", trigger, $"P9_Status of type ZString is read-only and cannot be changed by the Set Field (IFC) trigger action. Target Object: 'Task {milestone.P9_TaskID}'");
			AssertHasRowWarningContaining("P9_Status on exception is read-only", trigger, $"P9_Status of type ZString is read-only and cannot be changed by the Set Field (IFC) trigger action. Target Object: 'Task {exception.P9_TaskID}'");
		}

		public void TestOrganisationPK()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.AddNew();
			Factory.Save();

			ProcessTask task = Factory.New<ProcessTask>();
			AssertEquals(ZGuid.Empty, task.OrganisationPK);
			AssertEquals(ZGuid.Empty, task.P9_OA_ZAddress.OrgPK);

			task.OrganisationPK = org.PK;
			AssertEquals(org.PK, task.OrganisationPK);
			AssertEquals(org.PK, task.P9_OA_ZAddress.OrgPK);

			task.P9_OC = org.Contacts[0].PK;
			task.P9_OA = org.MainAddress.PK;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			ProcessTask reloadedTask = newFactory.Load<ProcessTask>(task.PK);
			AssertEquals(org.PK, task.OrganisationPK);
			AssertEquals(org.PK, task.P9_OA_ZAddress.OrgPK);
			AssertEquals(org.MainAddress.PK, task.P9_OA);
			AssertEquals(org.Contacts[0].PK, task.P9_OC);
		}

		public void TestAddressAndOrganisationSynchronization()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgAddress mainAddress1 = org1.MainAddress;
			OrgAddress mainAddress2 = org2.MainAddress;

			ProcessTask task = Factory.New<ProcessTask>();
			Factory.Save();

			task.P9_OA = mainAddress1.PK;
			AssertEquals("The address (P9_OA) and Organisation (OrganisationPK) should be synchronized", org1.PK, task.OrganisationPK);

			task.P9_OA = mainAddress2.PK;
			AssertEquals("The address (P9_OA) and Organisation (OrganisationPK) should be synchronized", org2.PK, task.OrganisationPK);

			task.OrganisationPK = org1.PK;
			AssertEquals("The user will have to choose the address when OrganisationPK is changed", ZGuid.Empty, task.P9_OA);

			task.OrganisationPK = org2.PK;
			AssertEquals("The user will have to choose the address when OrganisationPK is changed", ZGuid.Empty, task.P9_OA);
		}

		public void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			ProcessTask task = GetNewBusinessObject() as ProcessTask;
			AssertNotNull("GetNewBusinessObject() should return valid ProcessTask", task);
			AssertNotNull("ParentControllerID should be overriden to return correct ID for none standalone task, or override this test to assert true", task.ParentControllerID);
		}

		public void TestTaskIDReadOnly()
		{
			Assert("Readonly ID", ProcessTask.P9_TaskIDInfo.ReadOnly);
		}

		public void TestTaskIDSetOnSaving()
		{
			AssertEquals("No ID", ZString.Empty, ProcessTask.P9_TaskID);

			Factory.Save();
			AssertEquals("ID set", false, ProcessTask.P9_TaskID.IsEmpty);
			var expectedID = ProcessTask.P9_TaskID;

			ProcessTask.P9_Description = "Test";
			Factory.Save();
			AssertEquals("ID not changed", expectedID, ProcessTask.P9_TaskID);
		}

		#region TestSyncUserOnSave

		public void TestSyncUserOnSave_WUL()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("Org1");
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var product = (OrgSupplierPart)helper.CreateProduct(orgPK, "Pro01");
			var receive = helper.CreateWhsReceiveWithInventory(orgPK, warehouse.PK, "R1", product.PK, 10m);
			var taskDocketParent = Factory.New<ProcessTask>();
			taskDocketParent.P9_ParentID = receive.PK;
			taskDocketParent.P9_ParentTableCode = "WD";
			taskDocketParent.P9_FormFlowType = "WUL";
			Factory.Save();

			AssertNotNull(taskDocketParent);
			var mockSyncProcessorFactory = new Mock<IProcessTasksSyncUserProcessorFactory>();
			var mockSyncProcessor = new Mock<IProcessTaskSyncUserProcessor>();
			mockSyncProcessorFactory.Setup(s => s.GetUserSyncProcessor("WUL")).Returns(mockSyncProcessor.Object);

			using (ObjectFactory.Substitute(mockSyncProcessorFactory.Object))
			using (ObjectFactory.Substitute(mockSyncProcessor.Object))
			{
				taskDocketParent.P9_GS_NKAssignedStaffMember = "FRT";
				Factory.Save();
			}
			mockSyncProcessorFactory.Verify(x => x.GetUserSyncProcessor("WUL"), Times.Once);
			mockSyncProcessor.Verify(x => x.SyncUser(It.IsAny<BusinessObjectFactory>(), taskDocketParent), Times.Once);
		}

		public void TestSyncUserOnSave_WPT()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("Org1");
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var transferPK = helper.CreateWhsTransfer(orgPK, warehouse.PK, "R2", new NotificationBuffer());
			var taskDocketParent = Factory.New<ProcessTask>();
			taskDocketParent.P9_ParentID = transferPK;
			taskDocketParent.P9_ParentTableCode = "WD";
			taskDocketParent.P9_FormFlowType = "WPT";
			Factory.Save();

			AssertNotNull(taskDocketParent);
			var mockSyncProcessorFactory = new Mock<IProcessTasksSyncUserProcessorFactory>();
			var mockSyncProcessor = new Mock<IProcessTaskSyncUserProcessor>();
			mockSyncProcessorFactory.Setup(s => s.GetUserSyncProcessor("WPT")).Returns(mockSyncProcessor.Object);

			using (ObjectFactory.Substitute(mockSyncProcessorFactory.Object))
			using (ObjectFactory.Substitute(mockSyncProcessor.Object))
			{
				taskDocketParent.P9_GS_NKAssignedStaffMember = "FRT";
				Factory.Save();
			}
			mockSyncProcessor.Verify(x => x.SyncUser(It.IsAny<BusinessObjectFactory>(), taskDocketParent), Times.Once);
			mockSyncProcessorFactory.Verify(x => x.GetUserSyncProcessor("WPT"), Times.Once);
		}

		public void TestSyncUserOnSave_ClosedTask()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = helper.CreateClient("Org1");
			var warehouse = helper.CreateWarehouse("WHS", "A");
			var product = (OrgSupplierPart)helper.CreateProduct(orgPK, "Pro01");
			var receive = helper.CreateWhsReceiveWithInventory(orgPK, warehouse.PK, "R1", product.PK, 10m);
			var taskDocketParent = Factory.New<ProcessTask>();
			taskDocketParent.P9_ParentID = receive.PK;
			taskDocketParent.P9_ParentTableCode = "WD";
			taskDocketParent.P9_FormFlowType = "WUL";
			taskDocketParent.P9_GS_NKAssignedStaffMember = "~BP";
			Factory.Save();

			AssertNotNull(taskDocketParent);
			var mockSyncProcessorFactory = new Mock<IProcessTasksSyncUserProcessorFactory>();
			var mockSyncProcessor = new Mock<IProcessTaskSyncUserProcessor>();

			using (ObjectFactory.Substitute(mockSyncProcessorFactory.Object))
			using (ObjectFactory.Substitute(mockSyncProcessor.Object))
			{
				taskDocketParent.P9_GS_NKAssignedStaffMember = "FRT";
				taskDocketParent.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}
			mockSyncProcessorFactory.VerifyNoOtherCalls();
			mockSyncProcessor.VerifyNoOtherCalls();
		}

		#endregion

		public void TestTaskCanNotBeDeleted()
		{
			AssertEquals(true, ProcessTask.P9_TaskCannotBeDeletedInfo.ReadOnly);

			TemplateProcessTask templateTask = Factory.New<TemplateProcessTask>();
			AssertEquals(false, templateTask.P9_TaskCannotBeDeletedInfo.ReadOnly);
		}

		public void TestSettingStaffMemberAssignsTask()
		{
			AssertEquals("Pre-condition - task is open", ProcessTaskStatusCodeList.Codes.Open, ProcessTask.P9_Status);

			ProcessTask.P9_GS_NKAssignedStaffMember = "";
			AssertEquals("Stays as open", ProcessTaskStatusCodeList.Codes.Open, ProcessTask.P9_Status);

			ProcessTask.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Changes to assigned", ProcessTaskStatusCodeList.Codes.Assigned, ProcessTask.P9_Status);

			ProcessTask task2 = Factory.New<ProcessTask>();
			AssertEquals("Pre-condition - task is open", ProcessTaskStatusCodeList.Codes.Open, task2.P9_Status);

			task2.P9_GG_AssignedGroup = Factory.New<GlbGroup>().PK;
			AssertEquals("Changes to assigned", ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);

			ProcessTask task3 = Factory.New<ProcessTask>();
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_GS_NKAssignedStaffMember = "ZZ";
			AssertEquals("Changes to assigned", ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
		}

		#region Reset Penetration Date

		#region Set Penetration

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestSetPenetrationReset_ForResourceWithinReleaseGroup()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group1.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = false;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var staff1 = group1.Staff.AddNew();
			staff1.GS_Code = "NAM";
			staff1.GS_LoginName = "AaronAAaronson";

			var staff2 = group1.Staff.AddNew();
			staff2.GS_Code = "SRV";
			staff2.GS_LoginName = "BarryBBearonson";

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var link2 = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link2.FO_GG_ReleaseGroup = group2.PK;
			link2.FO_ResetTaskPenetrationOutsideGroup = false;
			link2.FO_ResetTaskPenetrationInsideGroup = true;

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflowInsideGroup = helper.CreateWorkflow(jobHeader, "aaa", group1.PK.ToGuid());
			workflowInsideGroup.FH_FC_CurrentComponent = buffer.PK;
			var taskInsideGroup = helper.CreateTask(workflowInsideGroup, taskType: "UDF");

			var workflowOutsideGroup = helper.CreateWorkflow(jobHeader, "bbb", group2.PK.ToGuid());
			workflowOutsideGroup.FH_FC_CurrentComponent = buffer.PK;
			var taskOutsideGroup = helper.CreateTask(workflowOutsideGroup, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			Factory.Save();

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var taskInsideGroupLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, taskInsideGroup.PK);
			var taskOutsideGroupLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, taskOutsideGroup.PK);
			AssertEquals("Should not have added log yet.", 0, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);
			AssertEquals("Should not have added log yet.", 0, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);

			taskInsideGroup.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			taskOutsideGroup.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			var currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			var currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var taskInsideGroupLogs = Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList();
			AssertEquals("Should have added log for task penetration reset.", 1, taskInsideGroupLogs.Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);

			var eventLog = taskInsideGroupLogs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task inside release group belonging to staff has reset", true, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertNotEquals("Workflow inside release group belonging to staff has reset", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff has NOT reset", false, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff has NOT reset", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			taskInsideGroup.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			taskOutsideGroup.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			Factory.Save();

			AssertEquals("Should not have added another log since staff code is same as before.", 1, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);

			AssertEquals("Task inside release group belonging to staff remains unchanged, since staff code has not changed", true, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff remains unchanged, since staff code has not changed", false, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			taskInsideGroup.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			taskOutsideGroup.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			Factory.Save();

			taskInsideGroupLogs = Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList();
			AssertEquals("Should have added task penetration reset log since staff code changed.", 2, taskInsideGroupLogs.Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);

			eventLog = taskInsideGroupLogs[1];
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=SRV\|TO=NAM\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task inside release group belonging to staff remains unchanged, even though staff code has changed", true, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff remains unchanged, even though staff code has changed", false, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			taskInsideGroup.P9_EstDuration = new ZDateTime(2000, 1, 1, 10, 0, 0);
			taskOutsideGroup.P9_EstDuration = new ZDateTime(2000, 1, 1, 10, 0, 0);

			Factory.Save();

			AssertEquals("Should not have added log.", 2, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);

			AssertEquals("Task inside release group belonging to staff remains unchanged, even though an unrelated field was modified", true, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff remains unchanged, even though an unrelated field was modified", false, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			taskInsideGroup.P9_GS_NKAssignedStaffMember = ZString.Empty;
			taskOutsideGroup.P9_GS_NKAssignedStaffMember = ZString.Empty;

			Factory.Save();

			AssertEquals("Should not have added log.", 2, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);

			AssertEquals("Task inside release group belonging to staff remains unchanged, even when clearing the staff code", true, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff remains unchanged, even when clearing the staff code", false, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			var newTaskInsideGroup = helper.CreateTask(workflowInsideGroup, staff1.GS_Code, taskType: "UDF");
			var newTaskOutsideGroup = helper.CreateTask(workflowOutsideGroup, staff1.GS_Code, taskType: "UDF");

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var newTaskInsideGroupLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, newTaskInsideGroup.PK);
			var newTaskOutsideGroupLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, newTaskOutsideGroup.PK);
			var newTaskInsideGroupLogs = Factory.Load<StmALog>(newTaskInsideGroupLogsQuery).ToList();

			AssertEquals("Log added since new task inside release group with staff assigned before save should have the same reset applied to it as the original task in the workflow", 1, newTaskInsideGroupLogs.Count);
			AssertEquals("No logs should have been added.", 0, Factory.Load<StmALog>(newTaskOutsideGroupLogsQuery).ToList().Count);
			AssertEquals("No logs should have been added.", 2, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);
			AssertEquals("No logs should have been added.", 0, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);

			eventLog = newTaskInsideGroupLogs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=NAM\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("New task inside release group with staff assigned before save should have the same reset applied to it as the original task in the workflow", true, newTaskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("New task outside release group with staff assigned before save should NOT have a reset applied to it", false, newTaskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);
		}

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestSetPenetrationReset_ForResourceOutsideReleaseGroup()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system, "buffer1");

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var link1 = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link1.FO_GG_ReleaseGroup = group1.PK;
			link1.FO_ResetTaskPenetrationOutsideGroup = true;
			link1.FO_ResetTaskPenetrationInsideGroup = false;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			var link2 = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link2.FO_GG_ReleaseGroup = group2.PK;
			link2.FO_ResetTaskPenetrationOutsideGroup = true;
			link2.FO_ResetTaskPenetrationInsideGroup = false;

			var staff1 = group1.Staff.AddNew();
			staff1.GS_Code = "NAM";
			staff1.GS_LoginName = "AaronAAaronson";

			var staff2 = group1.Staff.AddNew();
			staff2.GS_Code = "SRV";
			staff2.GS_LoginName = "BarryBBearonson";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflowOutsideGroup = helper.CreateWorkflow(jobHeader, "aaa", group2.PK.ToGuid());
			workflowOutsideGroup.FH_FC_CurrentComponent = buffer.PK;
			var taskOutsideGroup = helper.CreateTask(workflowOutsideGroup, taskType: "UDF");

			var workflowInsideGroup = helper.CreateWorkflow(jobHeader, "bbb", group1.PK.ToGuid());
			workflowInsideGroup.FH_FC_CurrentComponent = buffer.PK;
			var taskInsideGroup = helper.CreateTask(workflowInsideGroup, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			Factory.Save();

			var taskInsideGroupLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, taskInsideGroup.PK);
			var taskOutsideGroupLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, taskOutsideGroup.PK);
			AssertEquals("Should not have added log yet.", 0, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);
			AssertEquals("Should not have added log yet.", 0, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			taskInsideGroup.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			taskOutsideGroup.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			var currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			var currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var taskOutsideGroupLogs = Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList();
			AssertEquals("Should have added log for task penetration reset.", 1, taskOutsideGroupLogs.Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);

			var eventLog = taskOutsideGroupLogs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task inside release group belonging to staff has NOT reset", false, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff has NOT reset", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff has reset", true, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertNotEquals("Workflow outside release group belonging to staff has reset", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			taskInsideGroup.P9_GS_NKAssignedStaffMember = staff2.GS_Code;
			taskOutsideGroup.P9_GS_NKAssignedStaffMember = staff2.GS_Code;

			Factory.Save();

			AssertEquals("Should not have added another log since staff code is same as before.", 1, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);

			AssertEquals("Task inside release group belonging to staff remains unchanged, since staff code has not changed", false, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff remains unchanged, since staff code has not changed", true, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			taskInsideGroup.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			taskOutsideGroup.P9_GS_NKAssignedStaffMember = staff1.GS_Code;

			Factory.Save();

			taskOutsideGroupLogs = Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList();
			AssertEquals("Should have added task penetration reset log since staff code changed.", 2, taskOutsideGroupLogs.Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);

			eventLog = taskOutsideGroupLogs[1];
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=SRV\|TO=NAM\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task inside release group belonging to staff remains unchanged, even though staff code has changed", false, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff remains unchanged, even though staff code has changed", true, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			taskInsideGroup.P9_EstDuration = new ZDateTime(2000, 1, 1, 10, 0, 0);
			taskOutsideGroup.P9_EstDuration = new ZDateTime(2000, 1, 1, 10, 0, 0);

			Factory.Save();

			AssertEquals("Should not have added log.", 2, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);

			AssertEquals("Task inside release group belonging to staff remains unchanged, even though an unrelated field was modified", false, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff remains unchanged, even though an unrelated field was modified", true, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			taskInsideGroup.P9_GS_NKAssignedStaffMember = ZString.Empty;
			taskOutsideGroup.P9_GS_NKAssignedStaffMember = ZString.Empty;

			Factory.Save();

			AssertEquals("Should not have added log.", 2, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);

			AssertEquals("Task inside release group belonging to staff remains unchanged, even when clearing the staff code", false, taskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("Task outside release group belonging to staff remains unchanged, even when clearing the staff code", true, taskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			var newTaskInsideGroup = helper.CreateTask(workflowInsideGroup, staff1.GS_Code, taskType: "UDF");
			var newTaskOutsideGroup = helper.CreateTask(workflowOutsideGroup, staff1.GS_Code, taskType: "UDF");

			currentInsideWorkflowResetTime = workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc;
			currentOutsideWorkflowResetTime = workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var newTaskInsideGroupLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, newTaskInsideGroup.PK);
			var newTaskOutsideGroupLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, newTaskOutsideGroup.PK);
			var newTaskOutsideGroupLogs = Factory.Load<StmALog>(newTaskOutsideGroupLogsQuery).ToList();

			AssertEquals("No logs should have been added.", 0, Factory.Load<StmALog>(newTaskInsideGroupLogsQuery).ToList().Count);
			AssertEquals("Log should have been added since new task inside release group with staff assigned before save should have the same reset applied to it as the original task in the workflow", 1, newTaskOutsideGroupLogs.Count);
			AssertEquals("No logs should have been added.", 0, Factory.Load<StmALog>(taskInsideGroupLogsQuery).ToList().Count);
			AssertEquals("No logs should have been added.", 2, Factory.Load<StmALog>(taskOutsideGroupLogsQuery).ToList().Count);

			eventLog = newTaskOutsideGroupLogs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=NAM\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("New task inside release group with staff assigned before save should NOT have a reset applied to it", false, newTaskInsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow inside release group belonging to staff remains unchanged", currentInsideWorkflowResetTime, workflowInsideGroup.FH_TaskPenetrationResetDateTimeUtc);

			AssertEquals("New task outside release group with staff assigned before save should have the same reset applied to it as the original task in the workflow", true, newTaskOutsideGroup.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow outside release group belonging to staff remains unchanged", currentOutsideWorkflowResetTime, workflowOutsideGroup.FH_TaskPenetrationResetDateTimeUtc);
		}

		public void TestSetPenetrationResetOnTwoTasks_WhenOnlySecondTaskIsAllowedResetAndFirstTaskIsAssignedAResourceFirst()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = false;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var oldStaff = group.Staff.AddNew();
			oldStaff.GS_Code = "NAM";
			oldStaff.GS_LoginName = "AaronAAaronson";

			var newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "SRV";
			newStaff.GS_LoginName = "BarryBBearonson";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow1.FH_FC_CurrentComponent = buffer.PK;

			var task1 = helper.CreateTask(workflow1, oldStaff.GS_Code, taskType: "CDF");

			var workflow2 = helper.CreateWorkflow(jobHeader, "bbb", group.PK.ToGuid());
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			var task2 = helper.CreateTask(workflow2, oldStaff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskTypeCDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeCDF.Code = "CDF";
			taskTypeCDF.AllowTaskReset = false;
			var taskTypeUDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeUDF.Code = "UDF";
			taskTypeUDF.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflow1ResetTime = workflow1.FH_TaskPenetrationResetDateTimeUtc;
			var currentWorkflow2ResetTime = workflow2.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var task1LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task1.PK);
			var task2LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task2.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			task1.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			AssertEquals("Task type CDF has NOT reset", false, task1.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Task type CDF has NOT reset", currentWorkflow1ResetTime, workflow1.FH_TaskPenetrationResetDateTimeUtc);

			task2.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			var task2Logs = Factory.Load<StmALog>(task2LogsQuery).ToList();
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should of added log for task penetration reset.", 1, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			var eventLog = task2Logs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task type UDF has reset", true, task2.P9_IsResetBeingAppliedToThisTask);
			AssertNotEquals("Task type UDF has reset", currentWorkflow2ResetTime, workflow2.FH_TaskPenetrationResetDateTimeUtc);
		}

		public void TestSetPenetrationResetOnTwoTasks_WhenOnlySecondTaskIsAllowedResetAndSecondTaskIsAssignedAResourceFirst()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = false;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var oldStaff = group.Staff.AddNew();
			oldStaff.GS_Code = "NAM";
			oldStaff.GS_LoginName = "AaronAAaronson";

			var newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "SRV";
			newStaff.GS_LoginName = "BarryBBearonson";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow1.FH_FC_CurrentComponent = buffer.PK;

			var task1 = helper.CreateTask(workflow1, oldStaff.GS_Code, taskType: "CDF");

			var workflow2 = helper.CreateWorkflow(jobHeader, "bbb", group.PK.ToGuid());
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			var task2 = helper.CreateTask(workflow2, oldStaff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskTypeCDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeCDF.Code = "CDF";
			taskTypeCDF.AllowTaskReset = false;
			var taskTypeUDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeUDF.Code = "UDF";
			taskTypeUDF.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflow1ResetTime = workflow1.FH_TaskPenetrationResetDateTimeUtc;
			var currentWorkflow2ResetTime = workflow2.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var task1LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task1.PK);
			var task2LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task2.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			task2.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			var task2Logs = Factory.Load<StmALog>(task2LogsQuery).ToList();
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should of added log for task penetration reset.", 1, task2Logs.Count);

			var eventLog = task2Logs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task type UDF has reset", true, task2.P9_IsResetBeingAppliedToThisTask);
			AssertNotEquals("Task type UDF has reset", currentWorkflow2ResetTime, workflow2.FH_TaskPenetrationResetDateTimeUtc);

			task1.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 1, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			AssertEquals("Task type CDF has NOT reset", false, task1.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Task type CDF has NOT reset", currentWorkflow1ResetTime, workflow1.FH_TaskPenetrationResetDateTimeUtc);
		}

		public void TestSetPenetrationResetOnTwoTasks_WhenBothTasksAreAllowedReset()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = false;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var oldStaff = group.Staff.AddNew();
			oldStaff.GS_Code = "NAM";
			oldStaff.GS_LoginName = "AaronAAaronson";

			var newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "SRV";
			newStaff.GS_LoginName = "BarryBBearonson";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow1.FH_FC_CurrentComponent = buffer.PK;

			var task1 = helper.CreateTask(workflow1, oldStaff.GS_Code, taskType: "CDF");

			var workflow2 = helper.CreateWorkflow(jobHeader, "bbb", group.PK.ToGuid());
			workflow2.FH_FC_CurrentComponent = buffer.PK;

			var task2 = helper.CreateTask(workflow2, oldStaff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskTypeCDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeCDF.Code = "CDF";
			taskTypeCDF.AllowTaskReset = true;
			var taskTypeUDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeUDF.Code = "UDF";
			taskTypeUDF.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflow1ResetTime = workflow1.FH_TaskPenetrationResetDateTimeUtc;
			var currentWorkflow2ResetTime = workflow2.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var task1LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task1.PK);
			var task2LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task2.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			task1.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			var task1Logs = Factory.Load<StmALog>(task1LogsQuery).ToList();
			AssertEquals("Should have added log for task penetration reset.", 1, task1Logs.Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			var eventLog = task1Logs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task type CDF has reset", true, task1.P9_IsResetBeingAppliedToThisTask);
			AssertNotEquals("Task type CDF has reset", currentWorkflow1ResetTime, workflow1.FH_TaskPenetrationResetDateTimeUtc);

			task2.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			var task2Logs = Factory.Load<StmALog>(task2LogsQuery).ToList();
			AssertEquals("Should have added log for task penetration reset.", 1, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 1, task2Logs.Count);

			eventLog = task2Logs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task type UDF has reset", true, task2.P9_IsResetBeingAppliedToThisTask);
			AssertNotEquals("Task type UDF has reset", currentWorkflow2ResetTime, workflow2.FH_TaskPenetrationResetDateTimeUtc);
		}

		public void TestSetPenetrationResetOnTwoTasks_InSameWorkflow_WhenOnlySecondTaskIsAllowedResetAndFirstTaskIsAssignedAResourceFirst()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = false;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var oldStaff = group.Staff.AddNew();
			oldStaff.GS_Code = "NAM";
			oldStaff.GS_LoginName = "AaronAAaronson";

			var newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "SRV";
			newStaff.GS_LoginName = "BarryBBearonson";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "CDF");
			var task2 = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskTypeCDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeCDF.Code = "CDF";
			taskTypeCDF.AllowTaskReset = false;
			var taskTypeUDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeUDF.Code = "UDF";
			taskTypeUDF.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var task1LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task1.PK);
			var task2LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task2.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			task1.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			AssertEquals("Task type CDF has NOT reset", false, task1.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Task type CDF has NOT reset", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);

			currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			task2.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			var task2Logs = Factory.Load<StmALog>(task2LogsQuery).ToList();
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should have added log because of task penetration reset.", 1, task2Logs.Count);

			var eventLog = task2Logs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task type UDF has reset", true, task2.P9_IsResetBeingAppliedToThisTask);
			AssertNotEquals("Task type UDF has reset", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
		}

		public void TestSetPenetrationResetOnTwoTasks_InSameWorkflow_WhenOnlySecondTaskIsAllowedResetAndSecondTaskIsAssignedAResourceFirst()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = false;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var oldStaff = group.Staff.AddNew();
			oldStaff.GS_Code = "NAM";
			oldStaff.GS_LoginName = "AaronAAaronson";

			var newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "SRV";
			newStaff.GS_LoginName = "BarryBBearonson";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "CDF");
			var task2 = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskTypeCDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeCDF.Code = "CDF";
			taskTypeCDF.AllowTaskReset = false;
			var taskTypeUDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeUDF.Code = "UDF";
			taskTypeUDF.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var task1LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task1.PK);
			var task2LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task2.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			task2.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			var task2Logs = Factory.Load<StmALog>(task2LogsQuery).ToList();
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should have added log because of task penetration reset.", 1, task2Logs.Count);

			var eventLog = task2Logs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task type UDF has reset", true, task2.P9_IsResetBeingAppliedToThisTask);
			AssertNotEquals("Task type UDF has reset", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);

			task1.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 1, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			AssertEquals("Task type CDF has NOT reset", false, task1.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Task type CDF has NOT reset", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
		}

		public void TestSetPenetrationResetOnTwoTasks_InSameWorkflow_WhenBothTasksAreAllowedReset()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = false;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var oldStaff = group.Staff.AddNew();
			oldStaff.GS_Code = "NAM";
			oldStaff.GS_LoginName = "AaronAAaronson";

			var newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "SRV";
			newStaff.GS_LoginName = "BarryBBearonson";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "CDF");
			var task2 = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskTypeCDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeCDF.Code = "CDF";
			taskTypeCDF.AllowTaskReset = true;
			var taskTypeUDF = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskTypeUDF.Code = "UDF";
			taskTypeUDF.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var task1LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task1.PK);
			var task2LogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task2.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			task1.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			var task1Logs = Factory.Load<StmALog>(task1LogsQuery).ToList();
			AssertEquals("Should have added log because of task penetration reset.", 1, task1Logs.Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(task2LogsQuery).ToList().Count);

			var eventLog = task1Logs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task type CDF has reset applied", true, task1.P9_IsResetBeingAppliedToThisTask);
			AssertNotEquals("Workflow has reset", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);

			task2.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var task2Logs = Factory.Load<StmALog>(task2LogsQuery).ToList();
			AssertEquals("Should not have added log.", 1, Factory.Load<StmALog>(task1LogsQuery).ToList().Count);
			AssertEquals("Should have added log because of task penetration reset.", 1, task2Logs.Count);

			eventLog = task2Logs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertEquals("Task type UDF has reset applied", true, task2.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Workflow has NOT reset", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
		}

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestSetPenetrationResetOnTaskButDoNotSetWorkflowResetDate_WhenTopmostParentInSameJobWorkflowResetDateHasAlreadyBeenSet()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = true;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var oldStaff = group.Staff.AddNew();
			oldStaff.GS_Code = "NAM";
			oldStaff.GS_LoginName = "AaronAAaronson";

			var newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "SRV";
			newStaff.GS_LoginName = "BarryBBearonson";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "UDF");

			var parentWorkflow = helper.CreateWorkflow(jobHeader, "bbb", group.PK.ToGuid());
			parentWorkflow.FH_FC_CurrentComponent = buffer.PK;
			var parentTask = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "UDF");

			var topMostWorkflow = helper.CreateWorkflow(jobHeader, "ccc", group.PK.ToGuid(), penetrationResetDateTime: DateTime.UtcNow);
			topMostWorkflow.FH_FC_CurrentComponent = buffer.PK;
			var topMostTask = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "UDF");

			workflow.GetOrCreateLinkToParent(parentWorkflow);
			parentWorkflow.GetOrCreateLinkToParent(topMostWorkflow);

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;
			var currentParentWorkflowResetTime = parentWorkflow.FH_TaskPenetrationResetDateTimeUtc;
			var currentTopMostWorkflowResetTime = topMostWorkflow.FH_TaskPenetrationResetDateTimeUtc;

			task.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			parentTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			topMostTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			AssertEquals("Workflow should not have been reset since its topmost parent already has its reset date set", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals(true, task.P9_IsResetBeingAppliedToThisTask);

			AssertEquals("Parent workflow should not have been reset since its topmost parent already has its reset date set", currentParentWorkflowResetTime, parentWorkflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals(true, parentTask.P9_IsResetBeingAppliedToThisTask);

			AssertEquals("Topmost workflow should not have been reset since it already has its reset date set", currentTopMostWorkflowResetTime, topMostWorkflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals(true, topMostTask.P9_IsResetBeingAppliedToThisTask);
		}

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestSetPenetrationResetOnTaskAndSetWorkflowResetDate_WhenTopmostParentWorkflowInDifferentJobResetDateHasAlreadyBeenSet()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = true;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var oldStaff = group.Staff.AddNew();
			oldStaff.GS_Code = "NAM";
			oldStaff.GS_LoginName = "AaronAAaronson";

			var newStaff = group.Staff.AddNew();
			newStaff.GS_Code = "SRV";
			newStaff.GS_LoginName = "BarryBBearonson";

			var job1 = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeaderProvider.GetForParent(job1, Factory, addDefaultProcessHeaderIfNone: false);

			var job2 = Factory.New<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeaderProvider.GetForParent(job2, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader1, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "UDF");

			var parentWorkflow = helper.CreateWorkflow(jobHeader1, "bbb", group.PK.ToGuid());
			parentWorkflow.FH_FC_CurrentComponent = buffer.PK;
			var parentTask = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "UDF");

			var topMostWorkflow = helper.CreateWorkflow(jobHeader2, "ccc", group.PK.ToGuid(), penetrationResetDateTime: DateTime.UtcNow);
			topMostWorkflow.FH_FC_CurrentComponent = buffer.PK;
			var topMostTask = helper.CreateTask(workflow, oldStaff.GS_Code, taskType: "UDF");

			workflow.GetOrCreateLinkToParent(parentWorkflow);
			parentWorkflow.GetOrCreateLinkToParent(topMostWorkflow);

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;
			var currentParentWorkflowResetTime = parentWorkflow.FH_TaskPenetrationResetDateTimeUtc;
			var currentTopMostWorkflowResetTime = topMostWorkflow.FH_TaskPenetrationResetDateTimeUtc;

			var taskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task.PK);
			var parentTaskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, parentTask.PK);
			var topMostTaskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, topMostTask.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskLogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(parentTaskLogsQuery).ToList().Count);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(topMostTaskLogsQuery).ToList().Count);

			task.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			parentTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;
			topMostTask.P9_GS_NKAssignedStaffMember = newStaff.GS_Code;

			Factory.Save();

			var taskLogs = Factory.Load<StmALog>(taskLogsQuery).ToList();
			var parentTaskLogs = Factory.Load<StmALog>(parentTaskLogsQuery).ToList();
			var topMostTaskLogs = Factory.Load<StmALog>(topMostTaskLogsQuery).ToList();
			AssertEquals("Should have added log for task penetration reset.", 1, taskLogs.Count);
			AssertEquals("Should have added log for task penetration reset.", 1, parentTaskLogs.Count);
			AssertEquals("Should have added log for task penetration reset.", 1, topMostTaskLogs.Count);

			var taskEventLog = topMostTaskLogs.First();
			var parentTaskEventLog = parentTaskLogs.First();
			var topMostTaskEventLog = topMostTaskLogs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=SRV\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), taskEventLog.SL_Reference);
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=SRV\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), parentTaskEventLog.SL_Reference);
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=SRV\|TO=SRV\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), topMostTaskEventLog.SL_Reference);

			AssertEquals("Workflow should not have been reset since its topmost parent within its job will have its reset date set", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals(true, task.P9_IsResetBeingAppliedToThisTask);

			AssertNotEquals("Parent workflow should have been reset since it has no topmost parent within its job", currentParentWorkflowResetTime, parentWorkflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals(true, parentTask.P9_IsResetBeingAppliedToThisTask);

			AssertEquals("Topmost workflow should not have been reset since it already has its reset date set", currentTopMostWorkflowResetTime, topMostWorkflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals(true, topMostTask.P9_IsResetBeingAppliedToThisTask);
		}

		#endregion

		#region Do Not Set Penetration

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestDoNotSetPenetrationResetOnMilestonesExceptionsOrTriggers()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = true;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var staff = group.Staff.AddNew();
			staff.GS_Code = "NAM";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task = helper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var taskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task.PK);
			var taskLogs = Factory.Load<StmALog>(taskLogsQuery).ToList();
			AssertEquals("Should have added log because of task penetration reset.", 1, taskLogs.Count);

			var eventLog = taskLogs.First();
			AssertMatch("SL_Reference should have the correct details.", new Regex(@"^\|FRM=NAM\|TO=NAM\|WPEN=\d\.\d\d\|TPEN=\d\.\d\d$"), eventLog.SL_Reference);

			AssertNotEquals("Workflow has reset", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals("Task has reset", true, task.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Milestone should not reset", false, milestone.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Trigger should not reset", false, trigger.P9_IsResetBeingAppliedToThisTask);
			AssertEquals("Exception should not reset", false, exception.P9_IsResetBeingAppliedToThisTask);
		}

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestDoNotSetPenetrationReset_WhenWorkflowNotInAReleaseGroup()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = true;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var staff = group.Staff.AddNew();
			staff.GS_Code = "NAM";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", releaseGroupPK: null);
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task = helper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var taskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskLogsQuery).ToList().Count);

			AssertEquals("Workflow has not reset since workflow is not in a release group", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals("Task has not reset since workflow is not in a release group", false, task.P9_IsResetBeingAppliedToThisTask);
		}

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestDoNotSetPenetrationReset_WhenResetTaskPenetrationRegistryItemDisabled()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = true;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var staff = group.Staff.AddNew();
			staff.GS_Code = "NAM";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task = helper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var taskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskLogsQuery).ToList().Count);

			AssertEquals("Workflow has not reset since task resetting has been disabled in the registry", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals("Task has not reset since task resetting has been disabled in the registry", false, task.P9_IsResetBeingAppliedToThisTask);
		}

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestDoNotSetPenetrationReset_WhenBufferManagementIsDisabled()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = true;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var staff = group.Staff.AddNew();
			staff.GS_Code = "NAM";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task = helper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");

			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var taskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskLogsQuery).ToList().Count);

			AssertEquals("Workflow has not reset since buffer management is disabled", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals("Task has not reset since buffer management is disabled", false, task.P9_IsResetBeingAppliedToThisTask);
		}

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestDoNotSetPenetrationReset_WhenSystemIsNotLive()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			system.FS_IsLive = false;
			var buffer = helper.CreateBuffer(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = true;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var staff = group.Staff.AddNew();
			staff.GS_Code = "NAM";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task = helper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var taskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskLogsQuery).ToList().Count);

			AssertEquals("Workflow has not reset since the system is not live", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals("Task has not reset since the system is not live", false, task.P9_IsResetBeingAppliedToThisTask);
		}

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestDoNotSetPenetrationReset_WhenTaskIsInABucket()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var bucket = helper.CreateBucket(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)bucket.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = true;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var staff = group.Staff.AddNew();
			staff.GS_Code = "NAM";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = helper.CreateWorkflow(jobHeader, "aaa", group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = bucket.PK;

			var task = helper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var currentWorkflowResetTime = workflow.FH_TaskPenetrationResetDateTimeUtc;

			Factory.Save();

			var taskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskLogsQuery).ToList().Count);

			AssertEquals("Workflow has not reset since it has not been released to a buffer", currentWorkflowResetTime, workflow.FH_TaskPenetrationResetDateTimeUtc);
			AssertEquals("Task has not reset since it has not been release to a buffer", false, task.P9_IsResetBeingAppliedToThisTask);
		}

		[TestDate(2019, 10, 3)]
		[TestDateIncremental(seconds: 10)]
		public void TestDoNotSetPenetrationReset_WhenTaskIsStandalone()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var bucket = helper.CreateBucket(system);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = (BMComponentReleaseGroupLink)bucket.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationOutsideGroup = true;
			link.FO_ResetTaskPenetrationInsideGroup = true;

			var staff = group.Staff.AddNew();
			staff.GS_Code = "NAM";

			var task = Factory.New<DummyProcessTask>();

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "DUM";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.AllowTaskReset = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			Factory.Save();

			var taskLogsQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.TaskPenetrationReset.Code).AddToFilter(StmALogSchema.SL_Parent, task.PK);
			AssertEquals("Should not have added log.", 0, Factory.Load<StmALog>(taskLogsQuery).ToList().Count);

			AssertEquals("Task has not reset since it is not associated with a workflow", false, task.P9_IsResetBeingAppliedToThisTask);
		}

		#endregion

		#endregion

		#region Auto Assign Task on status change

		public void TestSettingStatusAssignsTask_WhenCurrentUserMatchesCapability()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			staff.CapabilityPivots.AddNew().G5_G4_Capability = capability.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var status = ProcessTaskStatusCodeList.Codes.Working;
				var task = Dummy.WorkflowItems.AddNew();
				task.P9_G4_RequiredCapability = capability.PK;
				AssertEquals(ZString.Empty, task.P9_GS_NKAssignedStaffMember);

				task.P9_Status = status;

				AssertEquals(string.Format(CultureInfo.InvariantCulture, "Setting status [{0}] should assign task", status), staff.GS_Code, task.P9_GS_NKAssignedStaffMember);
			}
		}

		public void TestSettingStatusAssignsTask_WhenCurrentUserMatchesCapability_CantSetStatus()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			staff.CapabilityPivots.AddNew().G5_G4_Capability = capability.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var status = ProcessTaskStatusCodeList.Codes.Working;
				var task = Dummy.WorkflowItems.AddNew();
				task.CanChangeStatus = false;
				task.P9_G4_RequiredCapability = capability.PK;
				AssertEquals(ZString.Empty, task.P9_GS_NKAssignedStaffMember);

				task.P9_Status = status;

				AssertEquals("", task.P9_GS_NKAssignedStaffMember);
				AssertEquals("OPN", task.P9_Status);
			}
		}

		public void TestSettingStatusAssignsTask_WhenCurrentUserMatchesCapability_OnUserCancelAssign()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			staff.CapabilityPivots.AddNew().G5_G4_Capability = capability.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var status = ProcessTaskStatusCodeList.Codes.Working;
				var alreadyWorkingTask = Dummy.WorkflowItems.AddNew();
				alreadyWorkingTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				alreadyWorkingTask.P9_Status = status;
				Factory.Save();

				var task = Dummy.WorkflowItems.AddNew();
				task.P9_G4_RequiredCapability = capability.PK;
				task.P9_GS_NKAssignedStaffMember = ZString.Empty;

				var conflictResolverProvider = new ChangeTrackingITaskStatusChangeConflictResolverProvider();
				Factory.ClearCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver));
				Factory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => conflictResolverProvider);
				conflictResolverProvider.ShouldContinue = false;
				task.P9_Status = status;
				AssertEquals(string.Format(CultureInfo.InvariantCulture, "Setting status [{0}] should not assign task", status), ZString.Empty, task.P9_GS_NKAssignedStaffMember);
			}
		}

		public void TestSettingStatusAssignsTask_WhenCurrentUserDoesNotMatchCapability()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var status = ProcessTaskStatusCodeList.Codes.Working;
				var task = Dummy.WorkflowItems.AddNew();
				task.P9_G4_RequiredCapability = capability.PK;
				AssertEquals(ZString.Empty, task.P9_GS_NKAssignedStaffMember);

				task.P9_Status = status;

				AssertEquals(string.Format(CultureInfo.InvariantCulture, "Setting status [{0}] should not assign task", status), ZString.Empty, task.P9_GS_NKAssignedStaffMember);
			}
		}

		public void TestAssigningFromServiceTaskDoesNotAssignSystemUsers()
		{
			var systemStaff = Factory.NewWithValidTestData<GlbStaff>();
			systemStaff.GS_IsSystemAccount = true;
			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			Globals.IsUserInteractive = false;
			ProcessTask.P9_GS_NKAssignedStaffMember = systemStaff.GS_Code;
			AssertEquals("Assigning system staff does nothing.", ZString.Empty, ProcessTask.P9_GS_NKAssignedStaffMember);
			ProcessTask.P9_GS_NKAssignedStaffMember = otherStaff.GS_Code;
			AssertEquals(otherStaff.GS_Code, ProcessTask.P9_GS_NKAssignedStaffMember);
		}

		public void TestTemplatesWithSystemUsersStillAssignSystemUsers()
		{
			var systemStaff = Factory.NewWithValidTestData<GlbStaff>();
			systemStaff.GS_IsSystemAccount = true;
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_GS_NKAssignedStaffMember = systemStaff.GS_Code;

			Factory.Save();
			Globals.IsUserInteractive = false;
			var job = Factory.New<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();

			Factory.Save();
			AssertEquals("Templates are agnostic to system staff.", systemStaff.GS_Code, job.WorkflowItems.Tasks.Cast<ProcessTask>().Single().P9_GS_NKAssignedStaffMember);
		}

		#endregion

		public void TestDurationsToHours()
		{
			ProcessTask.P9_ActualDuration = new ZDateTime(2006, 1, 1, 13, 30, 0);
			AssertEquals(13.5m, ProcessTask.ActualDurationHours);

			ProcessTask.P9_ActualDuration = new ZDateTime(2006, 1, 1, 0, 0, 0);
			AssertEquals(0m, ProcessTask.ActualDurationHours);

			ProcessTask.P9_ActualDuration = new ZDateTime(2006, 1, 1, 0, 45, 0);
			AssertEquals(0.75m, ProcessTask.ActualDurationHours);

			ProcessTask.P9_EstDuration = new ZDateTime(2006, 1, 3, 10, 15, 0);
			AssertEquals(58.25m, ProcessTask.LowEstimatedDurationHours);
		}

		public void TestDurationsToHoursShouldRoundUpToMinute()
		{
			ProcessTask.P9_ActualDuration = new ZDateTime(2015, 1, 1, 0, 0, 1);
			AssertEquals("Automatically round up a minute", (ZDateTime)TimeSpan.FromMinutes(1), ProcessTask.P9_ActualDuration);
			AssertEquals("Decimal equivalent after rounding up a minute", 0.0166666666666667m, ProcessTask.ActualDurationHours);
		}

		public void TestDurationsForDays_DontRound()
		{
			ProcessTask.P9_ActualDuration = new ZDateTime(2006, 1, 2, 0, 0, 1);
			AssertEquals((ZDateTime)new TimeSpan(1, 0, 0, 1), ProcessTask.P9_ActualDuration);
		}

		#region Readonly P9_FH_ProcessHeader

		public void TestP9_FH_ProcessHeader_ReadOnly()
		{
			AssertEquals(true, ProcessTask.IsTask);
			AssertEquals("When IsTask, P9_FH_ProcessHeader is NOT read only", false, ProcessTask.P9_FH_ProcessHeaderInfo.ReadOnly);

			ProcessTask.IsMilestone = true;
			AssertEquals(false, ProcessTask.IsTask);
			AssertEquals("When !IsTask, P9_FH_ProcessHeader is read only", true, ProcessTask.P9_FH_ProcessHeaderInfo.ReadOnly);

			ProcessTask.IsException = true;
			AssertEquals("When !IsTask, P9_FH_ProcessHeader is read only", true, ProcessTask.P9_FH_ProcessHeaderInfo.ReadOnly);
		}

		#endregion

		#region P9_Status

		[TestDate(2006, 11, 7, 9, 0, 0)]
		public void TestTimeDifferentForSameDay()
		{
			WorkflowDataRegistry.Instance.TaskDurationTrackingOutOfHoursLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			UpdateWeekDaysTo9To5();

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2006, 11, 7, 11, 45, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(2.75m, task.ActualDurationHours);

			TestDateAttribute.Date = new DateTime(2006, 11, 7, 12, 0, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2006, 11, 7, 17, 0, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("Working Hours (time should count towards suspended time while task is closed or cancelled)", 7.75m, task.ActualDurationHours);

			TestDateAttribute.Date = new DateTime(2006, 11, 8, 9, 0, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2006, 11, 9, 16, 30, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("Working Hours (time should count towards suspended time while task is closed or cancelled)", 23.25m, task.ActualDurationHours);
		}

		public void TestRoundUpMinuteIfNeeded_DropMilliseconds()
		{
			var baseDateTime = ZDateTime.DefaultDurationEpoch;
			var task = Factory.New<ProcessTask>();
			task.P9_ActualDuration = baseDateTime.AddMilliseconds(100);
			AssertEquals("Should roundup to one minute without millisecond ", baseDateTime.AddMinutes(1).ToString("MM/dd/yyyy hh:mm:ss.fff tt"), task.P9_ActualDuration.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

			task.P9_ActualDuration = baseDateTime.AddSeconds(10);
			AssertEquals("Should roundup to one minute", baseDateTime.AddMinutes(1), task.P9_ActualDuration);

			task.P9_ActualDuration = baseDateTime.AddSeconds(100);
			AssertEquals("Should not change the duration", baseDateTime.AddSeconds(100), task.P9_ActualDuration);
		}

		[TestDate(2006, 11, 7, 9, 0, 0)]
		public void TestCompletedTimeSetOnClose()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(ZDateTime.Now, task.P9_ActualDate);
			AssertEquals(ZDateTime.Empty, task.P9_CompletedTimeUtc);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			TestDateAttribute.Date = new DateTime(2006, 11, 7, 11, 45, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(new ZDateTime(2006, 11, 7, 9, 0, 0), task.P9_ActualDate);
			AssertEquals("Closing it a second time does nothing since its already closed...", new ZDateTime(2006, 11, 7, 9, 0, 0), task.P9_CompletedTimeUtc);
		}

		public void TestSettingEstimateDoesNotSetRemainingEstimates()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = new ZDateTime(2012, 1, 1, 4, 0, 0);
			AssertEquals(ZDateTime.Empty, task.P9_EstimatedTimeToComplete);
		}

		public void TestEstimatedDurationHours()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = new ZDateTime(2012, 1, 1, 4, 30, 0);
			AssertEquals(4.5m, task.LowEstimatedDurationHours);
		}

		public void TestHighEstimatedDurationHours()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = new ZDateTime(2012, 1, 1, 4, 30, 0);
			task.P9_EstimateVariationFactor = 2;
			AssertEquals(9m, task.HighEstimatedDurationHours);
		}

		public void TestEstimatedTimeToCompleteHours()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstimatedTimeToComplete = new ZDateTime(2012, 1, 1, 4, 30, 0);
			AssertEquals(4.5m, task.EstimatedTimeToCompleteHours);
		}

		public void TestHighEstimatedDuration()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = new ZDateTime(2012, 1, 1, 4, 30, 0);
			task.P9_EstimateVariationFactor = 2;
			AssertEquals((ZDateTime)TimeSpan.FromHours(9), task.HighEstimatedDuration);
		}

		public void TestStandardEstimateHours()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = new ZDateTime(2012, 1, 1, 4, 30, 0);
			task.P9_EstimateVariationFactor = 3;
			AssertEquals(9m, task.StandardEstimateHours);

			task.P9_EstDuration = new ZDateTime(2012, 1, 1, 3, 0, 0);
			task.P9_EstimateVariationFactor = 2;
			AssertEquals(4.5m, task.StandardEstimateHours);
		}

		[TestDate(2006, 11, 9, 9, 0, 0)]
		public void TestTimeDifferentForSameDay_WithSuspendedTime()
		{
			UpdateWeekDaysTo9To5();

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2006, 11, 9, 11, 45, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;       // 2.75 hrs working
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 11, 9, 12, 30, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;       // .75 hrs suspended
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 11, 9, 12, 45, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;       // .15 hrs working
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 11, 9, 14, 15, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;       // 1.5 hrs suspended
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 11, 9, 16, 30, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;        // 2.25 hrs working
			Factory.Save();

			AssertEquals(5.25m, task.ActualDurationHours);
			AssertEquals(2.25m, task.SuspendedDurationHours);
		}

		[TestDate(2006, 11, 9, 9, 0, 0)]
		public void TestTimeDifferentForSameDay_WithSuspendedTime_SuspendToClosedDirectly()
		{
			UpdateWeekDaysTo9To5();

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			TestDateAttribute.Date = new DateTime(2006, 11, 9, 11, 45, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;       // 2.75 hrs working
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 11, 9, 12, 30, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;       // .75 hrs suspended
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 11, 9, 12, 45, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;       // .15 hrs working
			AssertEquals(0.75m, task.SuspendedDurationHours);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2006, 11, 9, 16, 30, 0);
			AssertEquals(4.5m, task.SuspendedDurationHours);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;        // 3.75 hrs suspended
			Factory.Save();

			AssertEquals(3m, task.ActualDurationHours);
			AssertEquals(4.5m, task.SuspendedDurationHours);
		}

		[ExpectNoExceptions]
		public void TestSuspendedDuration_EmptyInvalidDateTime()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_TotalSuspendedDuration = ZDateTime.Empty;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task.P9_SuspendedAtForBinding = ZDateTimeOffset.Now.AddMinutes(-3);

			ZDecimal hours = task.SuspendedDurationHours;

			task.P9_TotalSuspendedDuration = ZDateTime.Invalid;
			hours = task.SuspendedDurationHours;
		}

		public void TestSuspendedDuration_InvalidSuspendedAtDateTime()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.TaskProperties.ActualDate = ZDateTimeOffset.Empty;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Assert("Sanity check", task.P9_SuspendedAt.IsEmpty);
			AssertEquals("If ActualDate is not set, we should not calculate SuspendedDuration", ZDecimal.Zero, task.SuspendedDurationHours);
		}

		public void TestSuspendedAndWorkingTasksForAssignedUser()
		{
			GlbStaff otherUser = Factory.New<GlbStaff>();
			otherUser.GS_Code = "XOT";

			ProcessTask task1 = Factory.New<ProcessTask>();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			ProcessTask task2 = Factory.New<ProcessTask>();
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			ProcessTask task3 = Factory.New<ProcessTask>();
			task3.P9_GS_NKAssignedStaffMember = otherUser.GS_Code;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			ProcessTask task4 = Factory.New<ProcessTask>();
			task4.P9_GS_NKAssignedStaffMember = otherUser.GS_Code;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			ProcessTask task5 = Factory.New<ProcessTask>();
			task5.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			ProcessTask task6 = Factory.New<ProcessTask>();
			task6.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			AssertEquals(2, task4.SuspendedAndWorkingTasksForAssignedUser.Count);
			AssertEquals(task3.PK, task4.SuspendedAndWorkingTasksForAssignedUser[0].PK);
			AssertEquals(task4.PK, task4.SuspendedAndWorkingTasksForAssignedUser[1].PK);

			AssertEquals(2, task1.SuspendedAndWorkingTasksForAssignedUser.Count);
			AssertEquals(task2.PK, task1.SuspendedAndWorkingTasksForAssignedUser[0].PK);
			AssertEquals(task5.PK, task1.SuspendedAndWorkingTasksForAssignedUser[1].PK);

			AssertNull(task6.SuspendedAndWorkingTasksForAssignedUser);
		}

		public void TestSuspendExistingWorkingTask_ShouldSetTaskChangeModeToOTT()
		{
			var processTaskStatusChangeModeTracker = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();
			processTaskStatusChangeModeTracker.SetCurrent(ProcessTaskStatusChangeModeCodeList.Codes.WorkflowTasksGrid);

			var task1 = Factory.New<ProcessTask>();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var task2 = Factory.New<ProcessTask>();
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			var task1LastLogReference = task1.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			var task2LastLogReference = task2.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;

			AssertContains($"{ProcessTaskStatusChangeLog.StatusChangeModeParameterCode}={ProcessTaskStatusChangeModeCodeList.Codes.WorkflowTasksGrid}", task1LastLogReference);
			AssertContains($"{ProcessTaskStatusChangeLog.StatusChangeModeParameterCode}={ProcessTaskStatusChangeModeCodeList.Codes.WorkflowTasksGrid}", task2LastLogReference);

			processTaskStatusChangeModeTracker.SetCurrent(ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			task1LastLogReference = task1.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			task2LastLogReference = task2.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;

			AssertContains($"{ProcessTaskStatusChangeLog.StatusChangeModeParameterCode}={ProcessTaskStatusChangeModeCodeList.Codes.OtherTask}", task1LastLogReference);
			AssertContains($"{ProcessTaskStatusChangeLog.StatusChangeModeParameterCode}={ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu}", task2LastLogReference);

			var conflictResolverProvider = new ChangeTrackingITaskStatusChangeConflictResolverProvider();
			Factory.ClearCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver));
			Factory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => conflictResolverProvider);
			conflictResolverProvider.ShouldContinue = false;
			conflictResolverProvider.TaskToStart = task1;

			processTaskStatusChangeModeTracker.SetCurrent(ProcessTaskStatusChangeModeCodeList.Codes.TriggerOrOtherAutomation);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			task1LastLogReference = task1.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			task2LastLogReference = task2.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;

			// Setting task to WRK with an already existing WRK task for a given user in an automation environment
			// will now simply revert the newly set task to SUS (i.e. no change in this case)
			AssertContains($"{ProcessTaskStatusChangeLog.StatusChangeModeParameterCode}={ProcessTaskStatusChangeModeCodeList.Codes.OtherTask}", task1LastLogReference);
			AssertContains($"{ProcessTaskStatusChangeLog.StatusChangeModeParameterCode}={ProcessTaskStatusChangeModeCodeList.Codes.TaskMenu}", task2LastLogReference);
		}

		[TestDate(2006, 10, 30, 9, 0, 0)]
		public void TestSuspendedActualTimeSetOnStatusChange()
		{
			WorkflowDataRegistry.Instance.TaskDurationTrackingOutOfHoursLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			UpdateWeekDaysTo9To5();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = new ZDateTime(2006, 10, 30, 11, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(2m, ProcessTask.ActualDurationHours);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals("Does not get reset", 2m, ProcessTask.ActualDurationHours);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			TestDateAttribute.Date = new ZDateTime(2006, 10, 30, 13, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			TestDateAttribute.Date = new ZDateTime(2006, 10, 30, 14, 30, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(3.5m, ProcessTask.ActualDurationHours);
			AssertEquals(2m, ProcessTask.SuspendedDurationHours);
			AssertEquals((ZDateTime)TimeSpan.FromHours(2), ProcessTask.P9_TotalSuspendedDuration);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			TestDateAttribute.Date = new ZDateTime(2006, 10, 31, 10, 30, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(7.5m, ProcessTask.ActualDurationHours);
			AssertEquals(2m, ProcessTask.SuspendedDurationHours);
		}

		[TestDate(2006, 11, 7, 9, 0, 0)]
		public void TestSuspendedActualTimeSetOnStatusChange_FromAssignedDirectlyToSuspended()
		{
			UpdateWeekDaysTo9To5();

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Assert("Precondition", task.P9_ActualDate.IsEmpty);
			TestDateAttribute.Date = new DateTime(2006, 11, 7, 9, 0, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			TestDateAttribute.Date = new DateTime(2006, 11, 7, 15, 0, 0);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals("Working Hours (time should count towards suspended time while task is closed or cancelled)", ZDateTime.DefaultDurationEpoch, task.SuspendedDuration);
		}

		[TestDate(2013, 1, 1, 12, 0, 0)]
		public void TestActualDurationMaxValidValue()
		{
			UpdateWeekDaysTo9To5();

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			TestDateAttribute.Date = new ZDateTime(2015, 10, 1, 12, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			var maximumPositiveActualDuration = TimeSpan.FromDays(180);
			AssertEquals((decimal)maximumPositiveActualDuration.TotalHours, ProcessTask.ActualDurationHours);
		}

		[TestDate(2013, 1, 1, 12, 0, 0)]
		public void TestActualDurationCorrectForWhenItemReopened()
		{
			UpdateWeekDaysTo9To5();

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(20).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(20).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(20).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(20).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			var expectedActualDuration = TimeSpan.FromMinutes(20);
			AssertEquals((decimal)expectedActualDuration.TotalHours, ProcessTask.ActualDurationHours);
		}

		[TestDate(2014, 11, 13, 9, 0, 0)]
		public void TestSuspendedActualTimeDoesntGoNegative()
		{
			WorkflowDataRegistry.Instance.TaskDurationTrackingOutOfHoursLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			UpdateWeekDaysTo9To5();
			var baseTime = ZDateTime.DefaultDurationEpoch;

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();
			AssertEquals(baseTime, ProcessTask.ElapsedDuration);
			AssertEquals(0m, ProcessTask.ActualDurationHours);
			AssertEquals(0m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 13, 11, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			AssertEquals(baseTime, ProcessTask.ElapsedDuration);
			AssertEquals(0m, ProcessTask.ActualDurationHours);
			AssertEquals(0m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 13, 15, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(baseTime, ProcessTask.ElapsedDuration);
			AssertEquals(0m, ProcessTask.ActualDurationHours);
			AssertEquals(0m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 13, 17, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(baseTime.AddHours(2), ProcessTask.ElapsedDuration);
			AssertEquals(2m, ProcessTask.ActualDurationHours);
			AssertEquals(0m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 13, 23, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(baseTime.AddHours(2), ProcessTask.ElapsedDuration);
			AssertEquals(2m, ProcessTask.ActualDurationHours);
			AssertEquals(6m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 10, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(baseTime.AddHours(3), ProcessTask.ElapsedDuration);
			AssertEquals(3m, ProcessTask.ActualDurationHours);
			AssertEquals(6m, ProcessTask.SuspendedDurationHours);
			AssertEquals(baseTime.AddHours(6), ProcessTask.P9_TotalSuspendedDuration);
			AssertEquals(baseTime.AddHours(3), ProcessTask.P9_ActualDuration);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 14, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(baseTime.AddHours(3), ProcessTask.ElapsedDuration);
			AssertEquals(3m, ProcessTask.ActualDurationHours);
			AssertEquals(10m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 16, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(baseTime.AddHours(5), ProcessTask.ElapsedDuration);
			AssertEquals(5m, ProcessTask.ActualDurationHours);
			AssertEquals(10m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 18, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(baseTime.AddHours(5), ProcessTask.ElapsedDuration);
			AssertEquals(5m, ProcessTask.ActualDurationHours);
			AssertEquals(12m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 20, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(baseTime.AddHours(5), ProcessTask.ElapsedDuration);
			AssertEquals(5m, ProcessTask.ActualDurationHours);
			AssertEquals(14m, ProcessTask.SuspendedDurationHours);
			AssertEquals(baseTime.AddHours(14), ProcessTask.P9_TotalSuspendedDuration);
			AssertEquals(baseTime.AddHours(5), ProcessTask.P9_ActualDuration);

			//15/11/2014 and 16/11/2014 is weekend 
			TestDateAttribute.Date = new ZDateTime(2014, 11, 17, 20, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(baseTime.AddHours(13), ProcessTask.ElapsedDuration);
			AssertEquals(13m, ProcessTask.ActualDurationHours);
			AssertEquals(14m, ProcessTask.SuspendedDurationHours);
			AssertEquals(baseTime.AddHours(14), ProcessTask.P9_TotalSuspendedDuration);
			AssertEquals(baseTime.AddHours(13), ProcessTask.P9_ActualDuration);
		}

		[TestUtcOffset(9, 0, 0)]
		[TestDate(2014, 11, 13, 0, 0, 0)]
		//Test same senario as the test above but based on Sydney timezone
		public void TestActualDurationWithUTCOffset()
		{
			WorkflowDataRegistry.Instance.TaskDurationTrackingOutOfHoursLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			UpdateWeekDaysTo9To5();
			var baseTime = ZDateTime.DefaultDurationEpoch;

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();
			AssertEquals(baseTime, ProcessTask.ElapsedDuration);
			AssertEquals(0m, ProcessTask.ActualDurationHours);
			AssertEquals(0m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 13, 2, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			AssertEquals(baseTime, ProcessTask.ElapsedDuration);
			AssertEquals(0m, ProcessTask.ActualDurationHours);
			AssertEquals(0m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 13, 6, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(baseTime, ProcessTask.ElapsedDuration);
			AssertEquals(0m, ProcessTask.ActualDurationHours);
			AssertEquals(0m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 13, 8, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(baseTime.AddHours(2), ProcessTask.ElapsedDuration);
			AssertEquals(2m, ProcessTask.ActualDurationHours);
			AssertEquals(0m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 13, 14, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(baseTime.AddHours(2), ProcessTask.ElapsedDuration);
			AssertEquals(2m, ProcessTask.ActualDurationHours);
			AssertEquals(6m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 1, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(baseTime.AddHours(3), ProcessTask.ElapsedDuration);
			AssertEquals(3m, ProcessTask.ActualDurationHours);
			AssertEquals(6m, ProcessTask.SuspendedDurationHours);
			AssertEquals(baseTime.AddHours(6), ProcessTask.P9_TotalSuspendedDuration);
			AssertEquals(baseTime.AddHours(3), ProcessTask.P9_ActualDuration);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 5, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(baseTime.AddHours(3), ProcessTask.ElapsedDuration);
			AssertEquals(3m, ProcessTask.ActualDurationHours);
			AssertEquals(10m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 7, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(baseTime.AddHours(5), ProcessTask.ElapsedDuration);
			AssertEquals(5m, ProcessTask.ActualDurationHours);
			AssertEquals(10m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 9, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();
			AssertEquals(baseTime.AddHours(5), ProcessTask.ElapsedDuration);
			AssertEquals(5m, ProcessTask.ActualDurationHours);
			AssertEquals(12m, ProcessTask.SuspendedDurationHours);

			TestDateAttribute.Date = new ZDateTime(2014, 11, 14, 11, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(baseTime.AddHours(5), ProcessTask.ElapsedDuration);
			AssertEquals(5m, ProcessTask.ActualDurationHours);
			AssertEquals(14m, ProcessTask.SuspendedDurationHours);
			AssertEquals(baseTime.AddHours(14), ProcessTask.P9_TotalSuspendedDuration);
			AssertEquals(baseTime.AddHours(5), ProcessTask.P9_ActualDuration);

			//15/11/2014 and 16/11/2014 is weekend 
			TestDateAttribute.Date = new ZDateTime(2014, 11, 17, 11, 0, 0).ToDateTime();
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(baseTime.AddHours(13), ProcessTask.ElapsedDuration);
			AssertEquals(13m, ProcessTask.ActualDurationHours);
			AssertEquals(14m, ProcessTask.SuspendedDurationHours);
			AssertEquals(baseTime.AddHours(14), ProcessTask.P9_TotalSuspendedDuration);
			AssertEquals(baseTime.AddHours(13), ProcessTask.P9_ActualDuration);
		}

		public void TestClosingNonWorkingTaskSetsActualDate()
		{
			ProcessTask task = Factory.New<ProcessTask>();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(ZDateTime.Empty, task.P9_ActualDate);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertZDatesWithin5Minutes("Actual Date", ZDateTime.Now, task.P9_ActualDate);
		}

		public void TestClosingTask_CanCloseTasksNotAssignedToSelfSecurityOverride()
		{
			SetupEnvironmentForClosingTaskTests(true);

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = "ZZ";
			task.P9_Type = "RVW";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Security Override is switched on, should allow closing task", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		public void TestClosingTask_AssignedToCurrentlyLoggedInUser()
		{
			SetupEnvironmentForClosingTaskTests(false);

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Type = "RVW";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Assigned to the currently logged in user, should allow closing task", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		public void TestClosingTask_InvalidWorkflowTaskType()
		{
			SetupEnvironmentForClosingTaskTests(false);

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = "ZZ";
			task.P9_Type = "__1";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Not a valid workflow type, should allow closing task", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		public void TestClosingTask_CanCloseTasksNotAssignedToSelf()
		{
			SetupEnvironmentForClosingTaskTests(false);

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = "ZZ";
			task.P9_Type = "COD";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("CanCloseTaskNotAssignedToSelf is set to true for COD", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		public void TestClosingTask_TaskOwnerPasswordRequestedEventUnhooked()
		{
			SetupEnvironmentForClosingTaskTests(false);
			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = "ZZ";
			task.TaskOwnerPasswordRequested += (sender, e) =>
			{
				e.IsValidPassword = false;
			};
			task.P9_Type = "COD";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("CanCloseTaskNotAssignedToSelf is set to true for COD", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		public void TestClosingTask_CannotCloseTasksNotAssignedToSelf()
		{
			SetupEnvironmentForClosingTaskTests(false);

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = "ZZ";
			task.P9_Type = "RVW";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("TaskOwnerPasswordRequested event is unhooked", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			EventHandler<ProcessTask.PasswordRequestEventArgs> handler = (sender, e) => { e.IsValidPassword = false; };
			task.TaskOwnerPasswordRequested += handler;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Password invalid", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.TaskOwnerPasswordRequested -= handler;
			handler = (sender, e) => { e.IsValidPassword = true; };
			task.TaskOwnerPasswordRequested += handler;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Valid password", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		public void TestClosingTask_Unassigned()
		{
			SetupEnvironmentForClosingTaskTests(false);

			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Type = "RVW";
			task.P9_GS_NKAssignedStaffMember = "";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		[ExpectException(typeof(ZSaveException))]
		public void TestClosingTask_CompletionTimeConstraint()
		{
			this.SetupEnvironmentForClosingTaskTests(false);
			var processTask = this.Factory.New<ProcessTask>();
			processTask.P9_GS_NKAssignedStaffMember = "ZZ";
			processTask.P9_Type = "TSK";
			processTask.P9_Status = "CLS";
			processTask.P9_CompletedTimeUtc = new ZDateTime(null);
			this.Factory.Save();
		}

		public void TestShouldNotChangeTaskStatusBackToAssignedIfSetToCurrentStaff()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = "ZZ";
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);

			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertEquals("Should not be re-opened", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		void SetupEnvironmentForClosingTaskTests(bool allowSecurityOverride)
		{
			Env.Security.WorkflowTasksCloseTaskNotAssignedToSelf.IsAllowed = allowSecurityOverride;

			CategorisedWorkflowTaskTypesCollection categorisedTaskTypesCollection = new CategorisedWorkflowTaskTypesCollection();
			CategorisedWorkflowTaskTypes categorisedTaskTypes = categorisedTaskTypesCollection.AddNew();
			categorisedTaskTypes.Code = "STA";
			WorkflowTaskType codingTask = categorisedTaskTypes.TaskTypes.AddNew();
			codingTask.Code = "COD";
			WorkflowTaskType reviewTask = categorisedTaskTypes.TaskTypes.AddNew();
			reviewTask.Code = "RVW";
			reviewTask.CanCloseTaskNotAssignedToSelf = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypesCollection);
		}

		#endregion

		#region P9_ActualDate / P9_ActualDuration

		public void TestP9_ActualDate_ReadOnly()
		{
			AssertEquals(true, ProcessTask.IsTask);
			AssertEquals("When IsTask, P9_ActualDate is NOT read only", false, ProcessTask.P9_ActualDateInfo.ReadOnly);

			ProcessTask.IsMilestone = true;
			AssertEquals(false, ProcessTask.IsTask);
			AssertEquals("When !IsTask, P9_ActualDate is NOT read only", false, ProcessTask.P9_ActualDateInfo.ReadOnly);

			ProcessTask.IsWorkflowTrigger = true;
			AssertEquals("When IsWorkflowTrigger, P9_ActualDate is read only", true, ProcessTask.P9_ActualDateInfo.ReadOnly);

			ProcessTask.IsWorkflowTrigger = false;
			ProcessTask.P9_LineTriggerType = "NAM";
			AssertEquals("When P9_LineTriggerType is set, P9_ActualDate is read only", true, ProcessTask.P9_ActualDateInfo.ReadOnly);
		}

		public void TestP9_ActualDuration_ReadOnly()
		{
			AssertEquals(true, ProcessTask.IsTask);
			AssertEquals("When IsTask, P9_ActualDurationInfo is NOT read only", false, ProcessTask.P9_ActualDurationInfo.ReadOnly);

			ProcessTask.IsMilestone = true;
			AssertEquals(false, ProcessTask.IsTask);
			AssertEquals("When !IsTask, P9_ActualDurationInfo is NOT read only", false, ProcessTask.P9_ActualDurationInfo.ReadOnly);
		}

		public void TestP9_CompletedTimeUtc_ReadOnly()
		{
			AssertEquals(true, ProcessTask.IsTask);
			AssertEquals("When IsTask, P9_CompletedTimeUtc is NOT read only", false, ProcessTask.P9_CompletedTimeUtcInfo.ReadOnly);

			ProcessTask.IsMilestone = true;
			AssertEquals(false, ProcessTask.IsTask);
			AssertEquals("When !IsTask, P9_CompletedTimeUtc is NOT read only", false, ProcessTask.P9_CompletedTimeUtcInfo.ReadOnly);

			ProcessTask.IsWorkflowTrigger = true;
			AssertEquals("When IsWorkflowTrigger, P9_CompletedTimeUtc is read only", true, ProcessTask.P9_CompletedTimeUtcInfo.ReadOnly);
		}

		public void TestActuals_ReadOnlyControlledBySecurityCheckpoint()
		{
			Env.Security.WorkflowTasksEditActuals.IsAllowed = false;
			Assert("Not allowed, should be read only", ProcessTask.P9_ActualDateInfo.ReadOnly);
			Assert("Not allowed, should be read only", ProcessTask.P9_ActualDurationInfo.ReadOnly);
			Assert("Not allowed, should be read only", ProcessTask.P9_CompletedTimeUtcInfo.ReadOnly);

			Env.Security.WorkflowTasksEditActuals.IsAllowed = true;
			Assert("Allowed, should be read/write", !ProcessTask.P9_ActualDateInfo.ReadOnly);
			Assert("Allowed, should be read/write", !ProcessTask.P9_ActualDurationInfo.ReadOnly);
			Assert("Allowed, should be read/write", !ProcessTask.P9_CompletedTimeUtcInfo.ReadOnly);
		}

		[TestDate(2007, 7, 5, 10, 30, 0, 0)]
		public void TestP9_ActualDuration_SetWhenTaskClosed()
		{
			ProcessTask.TaskProperties.ActualDate = new ZDateTimeOffset(new ZDateTime(2007, 7, 5, 10, 0, 0));
			AssertEquals(ZDateTime.DefaultDurationEpoch, this.ProcessTask.ElapsedDuration);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(ZDateTime.DefaultDurationEpoch, this.ProcessTask.ElapsedDuration);

			TestDateAttribute.Date = new DateTime(2007, 7, 5, 10, 45, 0);
			AssertEquals("Does not change post close", ZDateTime.DefaultDurationEpoch, this.ProcessTask.ElapsedDuration);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			AssertEquals(ZDateTime.DefaultDurationEpoch, this.ProcessTask.ElapsedDuration);

			ProcessTask.OverrideActualDuration = new ZDateTime(2007, 1, 1, 0, 17, 0);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals("Uses override duration if set", (ZDateTime)TimeSpan.FromMinutes(17), this.ProcessTask.ElapsedDuration);
			AssertEquals("Uses override duration cleared", ZDateTime.Empty, this.ProcessTask.OverrideActualDuration);
		}

		[TestDate(2015, 3, 4, 10, 30, 0, 0)]
		public void TestElapsedDurationWithInvalidAndEmptyActualDuration()
		{
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ProcessTask.P9_ActualDuration = ZDateTime.Invalid;
			Assert(ProcessTask.ElapsedDuration.IsValidSmallDateTime);
			AssertEquals(ZDateTime.DefaultDurationEpoch, ProcessTask.ElapsedDuration);

			ProcessTask.P9_ActualDuration = ZDateTime.Empty;
			Assert(ProcessTask.ElapsedDuration.IsValidSmallDateTime);
			AssertEquals(ZDateTime.DefaultDurationEpoch, ProcessTask.ElapsedDuration);
		}

		[TestDate(2013, 10, 11, 10, 0, 0)] // Friday, 10am
		public void TestP9_ActualDurationUsesStaffHours()
		{
			var hours = "                ********  ******"; //8am to 3pm with 12-1 lunch.
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staff.PK, DayOfWeek.Thursday, hours);
			WorkingDaysTestHelper.UpdateStaffDay(Factory, staff.PK, DayOfWeek.Friday, hours);

			ProcessTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(5);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertDateTimeWithinOneSecond("Close time is correct", ProcessTask.P9_CompletedTimeUtc.ToDateTime(), ZDateTime.UtcNow.ToDateTime());
			AssertEquals("Don't use the staff hours since the amount of out-of-hours time isn't big enough.", 5m, ProcessTask.ActualDurationHours);
		}

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public void TestP9_ActualDuration_Started1YrAgoButOnlyWorkedFor1Hr()
		{
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2000, 1, 1, 1, 0, 0);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2001, 1, 1, 0, 0, 0);
			ProcessTask.P9_ActualDuration = ZDateTime.Empty;
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			AssertEquals(@"Shouldn't stupidly be looking at durations after someone has manually cleared it.
Honestly, why would you expect the time from a year ago to appear in the duration today? 
It makes no sense.", ZDateTime.DefaultDurationEpoch, this.ProcessTask.ElapsedDuration);
		}

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public void TestP9_ActualDuration_ClosingUnsavedTaskFromNonWorking()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = "ORG";
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "INV";
			AssertEquals("Default Value Should be 'false'", false, taskType.IsRequireActualDuration);
			taskType.IsRequireActualDuration = true;
			Factory.Save();

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "NAM";
			var task1 = org.WorkflowItems.AddNew();
			task1.P9_Type = taskType.Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertHasError(task1.P9_ActualDurationInfo, "For closed tasks of this type, you must enter a non-zero actual duration.");
		}

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public void TestP9_ActualDuration_ClosingTaskFromWorkingWithPreviouslySetZeroDuration()
		{
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ProcessTask.P9_ActualDuration = new ZDateTime(2000, 1, 1);
			Factory.Save();

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Closing task with status=WRK and duration=00:00 (instead of empty), duration must be set to 1 minute", 1, ProcessTask.P9_ActualDuration.Minute);
		}

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public void TestP9_ActualDuration_SetStatusToCloseAndWorkingRepeatedlyWithoutSaving_FromSavedSuspendedStatus()
		{
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			AssertEquals("WRK for 1-hour, should add actual-duration by 1-hour", 1, ProcessTask.P9_ActualDuration.Hour);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			AssertEquals("From saved SUS status, then CLS > WRK for 1-hour without saving, should add actual-duration by 1-hour", 2, ProcessTask.P9_ActualDuration.Hour);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			AssertEquals("without saving then WRK for 1-hour, should add actual-duration by 1-hour", 3, ProcessTask.P9_ActualDuration.Hour);
		}

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public void TestP9_ActualDuration_SetStatusToCloseAndWorkingRepeatedlyWithoutSaving_FromSavedWorkingStatus()
		{
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			CreateAndAssertActualDuration_SetStatusToCloseAndWorkingRepeatedlyWithoutSaving();
		}

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public void TestP9_ActualDuration_SetStatusToWorkingThenClosedWithoutSaving()
		{
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(2);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Without saving, duration should be calculated correctly - 2 hours.", 2, ProcessTask.P9_ActualDuration.Hour);
		}

		[TestDate(2000, 1, 1, 0, 0, 0)]
		public void TestP9_ActualDuration_SetStatusToCloseAndWorkingRepeatedlyWithoutSaving_FromSavedWorkingStatus_2()
		{
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CreateAndAssertActualDuration_SetStatusToCloseAndWorkingRepeatedlyWithoutSaving();
		}

		void CreateAndAssertActualDuration_SetStatusToCloseAndWorkingRepeatedlyWithoutSaving()
		{
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			AssertEquals("Be zero.", ZDateTime.Empty, ProcessTask.P9_ActualDuration);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			AssertEquals("WRK for 1-hour, should add actual-duration by 1-hour", 1, ProcessTask.P9_ActualDuration.Hour);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			AssertEquals("SUS for 1-hour, should not add to actual-duration", 1, ProcessTask.P9_ActualDuration.Hour);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals("From saved WRK status, then CLS > WRK for 1-hour without saving, should add actual-duration by 1-hour", 2, ProcessTask.P9_ActualDuration.Hour);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Without saving then WRK for 1-hour then CLS > WRK again for 1-hour, should add actual-duration by 2-hours", 4, ProcessTask.P9_ActualDuration.Hour);
		}

		[TestDate(2016, 2, 1, 10, 30, 0, 0)]
		public void TestNegativeDuration()
		{
			AssertEquals(ZDateTime.Empty, ProcessTask.P9_ActualDuration);
			ProcessTask.P9_ActualDuration = new ZDateTime(2016, 1, 1, 0, 6, 0); // 0:06
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2016, 2, 1, 10, 27, 0);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals(TimeSpan.FromMinutes(6), ProcessTask.P9_ActualDuration.ToTimeSpan());
		}

		[TestDate(2016, 2, 1, 10, 30, 0, 0)]
		public void TestP9_ActualDuration_SetWorkingAndDurationBeforeSave_ShouldUpdateOnStatusChange()
		{
			AssertEquals(ZDateTime.Empty, ProcessTask.P9_ActualDuration);
			ProcessTask.P9_ActualDuration = new ZDateTime(2016, 1, 1, 0, 6, 0); // 0:06
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2016, 2, 1, 10, 33, 0);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals(9.0, ProcessTask.P9_ActualDuration.ToTimeSpan().TotalMinutes);
		}

		[TestDate(2016, 2, 1, 10, 30, 0, 0)]
		public void TestP9_ActualDuration_SetDurationAndWorkingBeforeSave_ShouldUpdateOnStatusChange()
		{
			AssertEquals(ZDateTime.Empty, ProcessTask.P9_ActualDuration);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = new DateTime(2016, 2, 1, 10, 33, 0);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(3.0, ProcessTask.P9_ActualDuration.ToTimeSpan().TotalMinutes);
		}

		[TestDate(2016, 2, 1, 10, 30, 0, 0)]
		public void TestP9_ActualDuration_SetDurationAndWorkingBeforeSave_MultiFactory()
		{
			AssertEquals(ZDateTime.Empty, ProcessTask.P9_ActualDuration);
			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = new DateTime(2016, 2, 1, 10, 33, 0);
			Factory.Save();
			Factory.CreateNewFactory();
			var loadedTask = Factory.Load<ProcessTask>(ProcessTask.PK);
			loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(3.0, loadedTask.P9_ActualDuration.ToTimeSpan().TotalMinutes);
		}

		public void TestP9_ActualDate_SetWhenTaskWorking()
		{
			AssertEquals(ZDateTimeOffset.Empty, ProcessTask.P9_ActualDateForBinding);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertZDatesWithin5Minutes("Now", ZDateTimeOffset.Now, ProcessTask.P9_ActualDateForBinding);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(ZDateTimeOffset.Empty, ProcessTask.P9_ActualDateForBinding);

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertZDatesWithin5Minutes("Now", ZDateTimeOffset.Now, ProcessTask.P9_ActualDateForBinding);
			Factory.Save();

			ProcessTask.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertZDatesWithin5Minutes("Once saved, this field won't get reset", ZDateTimeOffset.Now, ProcessTask.P9_ActualDateForBinding);
		}

		#endregion

		#region P9_ScheduledDate

		public void TestSupressScheduledDateChangeErrorReport()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var date = new ZDateTime(1899, 1, 1);

			var milestone1 = dummy.WorkflowItems.Milestones.AddNew();
			milestone1.P9_ScheduledDate = ZDateTime.Empty;
			milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			milestone1.P9_ScheduledDateForBinding = new ZDateTimeOffset(date);

			AssertEquals(date, milestone1.P9_ScheduledDate);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var milestone2 = dummy.WorkflowItems.Milestones.AddNew();
			milestone2.P9_ScheduledDate = ZDateTime.Empty;
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02Code;
			milestone2.P9_ScheduledDate = date;

			AssertEquals(date, milestone2.P9_ScheduledDate);
			AssertEquals("Date: 01-Jan-99 00:00:00 Utc: 31-Dec-98 14:00:00 outside of range 01-Jan-1900 - 06-Jun-2079.", ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestLogsOlderThan10YearsDontChangeScheduleDate()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var date1 = ZDateTime.Now.AddYears(-11).ToSmallDateTimeFloor();
			var date2 = ZDateTime.Now.AddYears(-9).ToSmallDateTimeFloor();

			dummy.Logs.AddNew(Events.CustomisableEvent01, new ZDateTimeOffset(date1), true);
			dummy.Logs.AddNew(Events.CustomisableEvent02, new ZDateTimeOffset(date2), true);

			var milestone1 = dummy.WorkflowItems.Milestones.AddNew();
			milestone1.P9_ScheduledDate = ZDateTime.Empty;
			milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;

			var milestone2 = dummy.WorkflowItems.Milestones.AddNew();
			milestone2.P9_ScheduledDate = ZDateTime.Empty;
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02Code;

			Factory.Save();

			AssertEquals(ZDateTime.Empty, milestone1.P9_ScheduledDate);
			AssertEquals(date2, milestone2.P9_ScheduledDate);
		}

		[TestDate(2017, 01, 05)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestOnlyTaskDateFieldsAreCalculatedFromUTC_RealLifeExample()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.TaskProperties.ActualDate = ZDateTimeOffset.Now;
			AssertEquals(Env.Time.GetUtcFromLocalTime(task.P9_ActualDate.ToDateTime()), task.P9_ActualDateUtc);
		}

		[TestDate(2016, 09, 03)]
		public void TestScheduledDateUpdatesScheduleDateUTC()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var assert = MakeScheduleRefreshAssertion(milestone);
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
			AssertEquals(ZDateTime.UtcNow, milestone.P9_ScheduledDateUtcForBinding);
			AssertEquals(ZDateTime.UtcNow.ToLocalBranchTime(Factory), milestone.P9_ScheduledDateLocalForBinding);
			assert();
		}

		public void TestDateForBindingOffset()
		{
			var timeZone = Factory.New<RefTimeZone>();
			timeZone.R2_CivilianTimeZoneCode = "SPE";
			timeZone.R2_OffsetMinutesFromUTC = -480;

			var timeZone2 = Factory.NewWithValidTestData<RefTimeZone>();
			timeZone2.R2_CivilianTimeZoneCode = "SPP";
			timeZone2.R2_OffsetMinutesFromUTC = 300;

			var timeZoneSet = Factory.New<RefTimeZoneSet>();
			timeZoneSet.R3_R2_StandardZone = timeZone.PK;
			timeZoneSet.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>().PK;
			timeZoneSet.R3_TimeZoneSetName = "DD";

			var timeZoneSet2 = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZoneSet2.R3_R2_StandardZone = timeZone2.PK;
			timeZoneSet2.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<DaylightSavingTimeZone>().PK;
			timeZoneSet2.R3_TimeZoneSetName = "OO";

			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = "Tst01";
			port.RL_R3 = timeZoneSet.PK;
			port.RL_RN_NKCountryCode = "AA";

			var port2 = Factory.NewWithValidTestData<RefUNLOCO>();
			port2.RL_Code = "Tst02";
			port2.RL_R3 = timeZoneSet2.PK;

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_RL_NKHomePort = port.Code;

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_RL_NKHomePort = port2.Code;

			var branchTimeZone1 = new TimeSpan(-8, 0, 0);
			var branchTimeZone2 = new TimeSpan(5, 0, 0);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job1 = Factory.New<DummyWithWorkflow>();
				var milestone = job1.WorkflowItems.Milestones.AddNew();
				var assert = MakeScheduleRefreshAssertion(milestone);
				milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now;
				milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
				Factory.Save();
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job2 = Factory.LoadTop1<DummyWithWorkflow>(new ZQuery());
				var milestone = job2.WorkflowItems.Milestones.AddNew();
				milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now;
				milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
				Factory.Save();
			}

			var job = Factory.LoadTop1<DummyWithWorkflow>(new ZQuery());
			AssertEquals("P9_ActualDateForBinding should be equal to branch time zone 1", job.WorkflowItems.Milestones[0].P9_ActualDateForBinding.Offset, branchTimeZone1);
			AssertEquals("P9_ScheduledDateForBinding should be equal to branch time zone 1", job.WorkflowItems.Milestones[0].P9_ScheduledDateForBinding.Offset, branchTimeZone1);

			AssertEquals("P9_ActualDateForBinding should be equal to branch time zone 2", job.WorkflowItems.Milestones[1].P9_ActualDateForBinding.Offset, branchTimeZone2);
			AssertEquals("P9_ScheduledDateForBinding should be equal to branch time zone 2", job.WorkflowItems.Milestones[1].P9_ScheduledDateForBinding.Offset, branchTimeZone2);
		}

		Action MakeScheduleRefreshAssertion(ProcessTask milestone)
		{
			var scheduleRefreshed = 0;
			var scheduleUtcRefreshed = 0;
			var scheduleLocalRefreshed = 0;

			milestone.P9_ScheduledDateForBindingInfo.ValueChanged += (s, e) => scheduleRefreshed++;
			milestone.P9_ScheduledDateUtcForBindingInfo.ValueChanged += (s, e) => scheduleUtcRefreshed++;
			milestone.P9_ScheduledDateLocalForBindingInfo.ValueChanged += (s, e) => scheduleLocalRefreshed++;

			return () =>
			{
				AssertEquals(1, scheduleRefreshed);
				AssertEquals(1, scheduleUtcRefreshed);
				AssertEquals(1, scheduleLocalRefreshed);
			};
		}

		#endregion

		public void TestHyperlinkText()
		{
			var processTask = Factory.New<ProcessTask>();
			processTask.P9_TaskID = "T00001004";
			processTask.P9_Description = "backup";
			AssertEquals("Task + ID + Description", "Task T00001004 backup", processTask.HumanReadableName);
		}

		#region P9_CompletedTimeUtc

		[TestDate(2013, 06, 11, 10, 0, 0)]
		public void TestCompletedTimeLocal()
		{
			if (GlbBranch.CurrentBranch.HomePort.TimeZoneSet != null)
			{
				GlbBranch.CurrentBranch.HomePort.TimeZoneSet.Delete();
			}

			var task = Factory.New<ProcessTask>();
			task.P9_CompletedTimeUtc = ZDateTime.UtcNow;

			AssertDateTimeWithinOneSecond(string.Format("{0} should be in UTC", ProcessTasksSchema.P9_CompletedTimeUtc.Name), ZDateTime.UtcNow.ToDateTime(), task.P9_CompletedTimeUtc.ToDateTime());
			AssertDateTimeWithinOneSecond("CompletedTimeLocal should be in local time", ZDateTime.Now.ToDateTime(), task.CompletedTimeLocal.ToDateTime());

			ProcessTask.CompletedTimeLocal = ZDateTime.Now;
			AssertDateTimeWithinOneSecond("CompletedTimeUtc should be in UTC.", ProcessTask.P9_CompletedTimeUtc.ToDateTime(), ZDateTime.UtcNow.ToDateTime());

			ProcessTask.P9_CompletedTimeUtc = ZDateTime.Empty;
			AssertEquals("CompletedTime Local should be empty.", ProcessTask.CompletedTimeLocal, ZDate.Empty);

			ProcessTask.CompletedTimeLocal = ZDateTime.Invalid;
			AssertEquals("CompletedTimeUtc should be invalid.", ProcessTask.P9_CompletedTimeUtc, ZDate.Invalid);
		}

		#endregion

		#region P9_Condition2Value

		public void TestP9_Condition2ValueForMacroTypes()
		{
			RunTest(ProcessTasksLookups.UserDefinedCondition);
			RunTest(ProcessTasksLookups.MacroCondition);

			void RunTest(string macroType)
			{
				ProcessTask task = Dummy.WorkflowItems.Tasks.AddNew();
				task.TemplateConditions.TemplateCondition2 = "NAM";
				task.TemplateConditions.TemplateCondition2Value = "XYZ";
				var row = ((INeedRow)task).Row;
				AssertEquals("XYZ", task.TemplateConditions.TemplateCondition2Value);
				AssertEquals("XYZ", row[ProcessTasksSchema.Constants.P9_Condition2Value]);

				task.TemplateConditions.TemplateCondition2 = macroType;
				AssertEquals("", task.TemplateConditions.TemplateCondition2Value);
				AssertEquals("", row[ProcessTasksSchema.Constants.P9_Condition2Value]);

				task.TemplateConditions.TemplateCondition2Value = "Hello World";
				AssertEquals("Hello World", task.TemplateConditions.TemplateCondition2Value);
				AssertEquals("", row[ProcessTasksSchema.Constants.P9_Condition2Value]);

				task.TemplateConditions.TemplateCondition2 = "NAM";
				AssertEquals("Base value is cleared in default implementation", "",
					task.TemplateConditions.TemplateCondition2Value);
				AssertEquals("Base value is cleared in default implementation", "",
					row[ProcessTasksSchema.Constants.P9_Condition2Value]);

				task.TemplateConditions.TemplateCondition2 = macroType;
				AssertEquals("Hello World", task.TemplateConditions.TemplateCondition2Value);
				AssertEquals("", row[ProcessTasksSchema.Constants.P9_Condition2Value]);
			}
		}

		#endregion

		#endregion

		#region New Properties

		public void TestDocManagerInfoUseBusinessEntityFactoryAsInternal()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			var taskAsDocManagerSupport = task as IDocManagerSupport;
			Factory.Save();

			Factory.ChildFactories.Add(taskAsDocManagerSupport.DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs);
			var taskInDocManager = taskAsDocManagerSupport.DocManagerInfo.MasterFactory.FactoryForEverythingExceptEDocs.Load<ProcessTask>(task.PK);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			taskInDocManager.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Db.Connection.ExecuteNonQuery($"UPDATE {Business.ProcessTask.Schema.TableName} SET P9_Status = '{ProcessTaskStatusCodeList.Codes.Suspended}' WHERE P9_PK = '{task.PK}'");

			AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);
			AssertNullOrEmpty(ErrorReporter.LastKeyReported);
		}

		[TestDate(2015, 7, 14)]
		public void TestCreatedTime()
		{
			var branch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			branch.GB_RL_NKHomePort = "AUSYD";

			Factory.Save();

			var task = Factory.New<ProcessTask>();

			AssertEquals(ZDateTime.Empty, task.CreatedTimeUtc);
			AssertEquals(ZDateTime.Empty, task.CreatedTimeLocal);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals(new ZDateTime(2015, 7, 14), task.CreatedTimeUtc);
				AssertEquals(new ZDateTime(2015, 7, 14, 10, 0, 0), task.CreatedTimeLocal);
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestCreatedTime_UseSystemTimeField()
		{
			var tempValues = new EnableAddEditAndDeleteLogsItemCollection();
			tempValues.Add(new EnableAddEditAndDeleteLogsItem()
			{
				Table = ProcessTasksSchema.Constants.TableName,
				EnableADDLogs = true,
				EnableEDTLogs = true,
				EnableDELLogs = true,
			});

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValues))
			{
				var task = Factory.New<ProcessTask>();
				Factory.Save();

				task.P9_SystemCreateTimeUtc = ZDateTime.Empty;
				AssertEquals("Fallback to the log if the system date isn't set because the transform may not have run yet.", new ZDateTime(2015, 7, 14), task.CreatedTimeUtc);

				task.P9_SystemCreateTimeUtc = ZDateTime.Now.AddDays(3);
				AssertEquals(task.P9_SystemCreateTimeUtc, task.CreatedTimeUtc);
			}
		}

		public void TestP9_ShareTasksForAllCompanies()
		{
			var task = Factory.New<DummyProcessTask>();
			AssertEquals("Precondition", true, task.IsTask);
			AssertEquals("Disabled by default", false, task.P9_ShareTasksForAllCompanies);
			AssertEquals("Editable", false, task.P9_ShareTasksForAllCompaniesInfo.ReadOnly);

			task.P9_GC = ZGuid.NewZGuid();
			AssertEquals("Readonly in different company", true, task.P9_ShareTasksForAllCompaniesInfo.ReadOnly);

			task.P9_ShareTasksForAllCompanies = true;
			AssertEquals("Stores changes", true, task.P9_ShareTasksForAllCompanies);

			task.IsMilestone = true;
			AssertEquals("Disabled for non-tasks", false, task.P9_ShareTasksForAllCompanies);
			task.P9_ShareTasksForAllCompanies = true;
			AssertEquals("No changes for non-tasks", false, task.P9_ShareTasksForAllCompanies);
		}

		public void TestLongDescription_WhenCompletionStatement_ShouldReturnP9_NotesAsString()
		{
			MasterFilesTestHelper.AddCompletionStatementTaskType("COM", "ORG", "Completion Statement");

			var org = Factory.New<OrgHeader>();
			var task = org.WorkflowItems.AddNew();
			task.P9_Type = "COM";
			CombineAssertions("Precondition", () =>
			{
				AssertEquals("P9_NotesAsString should be empty", string.Empty, task.P9_NotesAsString);
				AssertEquals("P9_Description", "Completion Statement", task.P9_Description);
				AssertEquals("Should be a completion statement.", true, task.IsCompletionStatement);
			});

			const string completionStatement = "This is a completion statement. I have a question. John had 3 apples and then he ate 2. Now calculate the mass of the sun. Yes.";
			task.P9_NotesAsString = completionStatement;

			CombineAssertions("Given a completion statement task", () =>
			{
				AssertEquals("P9_NotesAsString should be the completion statement.", completionStatement, task.P9_NotesAsString);
				AssertEquals("P9_Description should be the truncated completion statement.", completionStatement.Substring(0, ProcessTasksSchema.P9_Description.MaxLength - 3) + "...", task.P9_Description);
				AssertEquals("LongDescription should be the completion statement.", completionStatement, task.LongDescription);
			});
		}

		public void TestLongDescription_WhenNotCompletionStatement_ShouldReturnP9_Description()
		{
			var org = Factory.New<OrgHeader>();
			var task = org.WorkflowItems.AddNew();
			task.P9_Type = "TLT";
			CombineAssertions("Precondition", () =>
			{
				AssertEquals("P9_NotesAsString should be empty", string.Empty, task.P9_NotesAsString);
				AssertEquals("P9_Description should be empty.", string.Empty, task.P9_Description);
				AssertEquals("Should not be a completion statement.", false, task.IsCompletionStatement);
			});

			const string description = "This is a descopriton! I like speling erors.";
			task.P9_Description = description;
			const string notes = "This is task notes. This is task notes. This is task notes. This is task notes. This is task notes. This is task notes. This is task notes.";
			task.P9_NotesAsString = notes;

			CombineAssertions("Given a task which is not a completion statement", () =>
			{
				AssertEquals("P9_NotesAsString should be the notes.", notes, task.P9_NotesAsString);
				AssertEquals("P9_Description should be the description.", description, task.P9_Description);
				AssertEquals("LongDescription should be the description.", description, task.LongDescription);
			});
		}

		public void TestDescriptionWithReference()
		{
			ProcessTaskWithReference task = Factory.New<ProcessTaskWithReference>();
			task.P9_Description = "Hello";
			AssertEquals("Hello", task.DescriptionWithReference);

			task.ReferenceCode = "Zubin";
			AssertEquals("Hello (Zubin)", task.DescriptionWithReference);

			task.ReferenceCode = "";
			AssertEquals("Hello", task.DescriptionWithReference);
		}

		class ProcessTaskWithReference : ProcessTask
		{
			public ProcessTaskWithReference(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZString ReferenceCode { get; set; }
		}

		public void TestIsSuspended()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			AssertEquals(false, task.IsSuspended);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(false, task.IsSuspended);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(false, task.IsSuspended);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(true, task.IsSuspended);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(false, task.IsSuspended);

			task.P9_Status = "";
			AssertEquals(false, task.IsSuspended);
		}

		public void TestIsClosed_ForTask()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			AssertEquals(false, task.IsClosed);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(false, task.IsClosed);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(true, task.IsClosed);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(false, task.IsClosed);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(false, task.IsClosed);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(true, task.IsClosed);

			task.P9_Status = "";
			AssertEquals(true, task.IsClosed);
		}

		public void TestIsClosed_ForMilestone()
		{
			DummyWithWorkflow bizObj = Factory.New<DummyWithWorkflow>();
			ProcessTask milestone = bizObj.WorkflowItems.Milestones.AddNew();
			milestone.IsMilestone = true;
			AssertEquals("Milestone not closed initially", false, milestone.IsClosed);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			Factory.Save();
			AssertEquals("Milestone closed when actual date is set", true, milestone.IsClosed);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			Factory.Save();
			AssertEquals("Milestone not closed when actual date cleared", false, milestone.IsClosed);
		}

		public void TestIsClosed_ForWorkflowTrigger()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			AssertEquals("Trigger not closed initially", false, trigger.IsClosed);

			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals("Trigger closed when actual date is set", true, trigger.IsClosed);

			log.Cancel();
			AssertEquals("Trigger not closed when event cancelled", false, trigger.IsClosed);
		}

		public void TestIsBehindScheduleAndEstimatedCompletionDate()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			AssertEquals(ZDateTime.Empty, task.EstimatedCompletionDate);
			AssertEquals(false, task.IsBehindSchedule);

			task.P9_ScheduledDate = ZDateTime.Now.AddDays(3);
			AssertEquals(task.P9_ScheduledDate, task.EstimatedCompletionDate);
			AssertEquals(false, task.IsBehindSchedule);

			task.P9_ScheduledDate = ZDateTime.Now.AddHours(-5);
			AssertEquals(task.P9_ScheduledDate, task.EstimatedCompletionDate);
			AssertEquals(true, task.IsBehindSchedule);

			task.P9_EstDuration = new ZDateTime(2006, 1, 1, 7, 0, 0);
			AssertEquals(task.P9_ScheduledDate.AddHours(7), task.EstimatedCompletionDate);
			AssertEquals(false, task.IsBehindSchedule);

			task.P9_EstDuration = new ZDateTime(2006, 1, 1, 4, 0, 0);
			AssertEquals(task.P9_ScheduledDate.AddHours(4), task.EstimatedCompletionDate);
			AssertEquals(true, task.IsBehindSchedule);

			task.TaskProperties.ActualDate = ZDateTimeOffset.Now;
			AssertEquals("doesn't change", task.P9_ScheduledDate.AddHours(4), task.EstimatedCompletionDate);
			AssertEquals("Since it's complete, it's no longer behind schedule", false, task.IsBehindSchedule);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHasNotes()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opp = org.SalesOpportunities.AddNew();
			ProcessTask task = Factory.NewWithValidTestData<ProcessTask>();
			Assert(!task.HasNote);
			Assert(((IDocManagerSupport)task).DocManagerInfo.MasterFactory.GetStorageMainForPK(task.PK) == null);

			task.P9_Notes = ZBlob.FromUTF8("Hello this is a test");
			Assert(task.HasNote);

			task.P9_Notes = ZBlob.Empty;
			Assert(!task.HasNote);
			Assert(((IDocManagerSupport)task).DocManagerInfo.MasterFactory.GetStorageMainForPK(task.PK) == null);

			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			BusinessObjectFactory documentFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);

			IeDoc doc = (IeDoc)documentFactory.New<IStorageDocs>();
			((IDocManagerSupport)task).DocManagerInfo.Documents.Add(doc);
			Assert(task.HasNote);
			Assert(((IDocManagerSupport)task).DocManagerInfo.MasterFactory.GetStorageMainForPK(task.PK) != null);

			((IDocManagerSupport)task).DocManagerInfo.Documents.Remove(doc);
			Assert(!task.HasNote);

			string testFile = BaseSourcePath + @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\System\DocManager\TestFiles\Sample.PDF";
			IeDoc docAdded = ((IDocManagerSupport)task).DocManagerInfo.AddFileOrDocument(testFile, "SOA");
			Assert(task.HasNote);

			((IDocManagerSupport)task).DocManagerInfo.Files.Remove(docAdded);
			Assert(!task.HasNote);
			Assert(((IDocManagerSupport)task).DocManagerInfo.MasterFactory.GetStorageMainForPK(task.PK) != null);
		}

		public void TestNotesHasChanges()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Notes = ZBlob.Empty;
			Assert(!task.HasChanges);

			task.P9_Notes = ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n{\\*\\generator Riched20 10.0.15063}\\viewkind4\\uc1 \r\n\\pard\\f0\\fs20\\par\r\n}\r\n");
			Assert("Empty content with style formats", !task.HasChanges);

			task.P9_Notes = ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n{\\*\\generator Riched20 10.0.15063}\\viewkind4\\uc1 \r\n\\pard\\f0\\fs20  \\par\r\n}\r\n");
			Assert("Space", task.HasChanges);

			task.HasChanges = false;
			task.P9_Notes = ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n{\\*\\generator Riched20 10.0.15063}\\viewkind4\\uc1 \r\n\\pard\\f0\\fs20 \\{\\par\r\n}\r\n");
			Assert("Curly brackets", task.HasChanges);

			task.HasChanges = false;
			task.P9_Notes = ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang1033{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n{\\*\\generator Riched20 10.0.15063}\\viewkind4\\uc1 \r\n\\pard\\f0\\fs20 \\\\\\par\r\n}\r\n");
			Assert("Backslash", task.HasChanges);
		}

		public void TestParentJobDetails()
		{
			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			opp.P8_OpportunityID = "O1234";
			opp.P8_OpportunityDescription = "Some Opportunity of mine";
			OpportunityProcessTasks task = (OpportunityProcessTasks)opp.WorkflowItems.AddNew();

			AssertEquals("Opportunity (O1234) - Some Opportunity of mine", task.ParentJobDetails);

			opp.P8_OpportunityDescription = "";
			AssertEquals("Opportunity (O1234)", task.ParentJobDetails);
		}

		public void TestETA_EmptyInBase()
		{
			ProcessTask task = Factory.NewWithValidTestData<ProcessTask>();

			Factory.Save();

			AssertEquals("ETA field should return empty value except for JobDeclarationProcessTask, ForwardingConsolProcessTask and ForwardingShipmentProcessTask", task.ETA, ZDateTime.Empty);
		}

		public void TestETD_EmptyInBase()
		{
			ProcessTask task = Factory.NewWithValidTestData<ProcessTask>();

			Factory.Save();

			AssertEquals("ETD field should return empty value except for JobDeclarationProcessTask, ForwardingConsolProcessTask and ForwardingShipmentProcessTask", task.ETD, ZDateTime.Empty);
		}

		public void TestLoadOrOriginPort_EmptyInBase()
		{
			ProcessTask task = Factory.NewWithValidTestData<ProcessTask>();

			Factory.Save();

			AssertEquals("TestLoadOrOriginPort field should return empty value except for JobDeclarationProcessTask , ForwardingConsolProcessTask and JobShipmentProcessTask", task.LoadOrOriginPort, ZString.Empty);
		}

		public void TestDischargeOrDestinationPort_EmptyInBase()
		{
			ProcessTask task = Factory.NewWithValidTestData<ProcessTask>();

			Factory.Save();

			AssertEquals("DischargeOrDestinationPort field should return empty value except for JobDeclarationProcessTask , ForwardingConsolProcessTask and JobShipmentProcessTask", task.DischargeOrDestinationPort, ZString.Empty);
		}

		#region MessagingTriggerParties / EmailTriggerParties

		public void TestGetMessagingTriggerPartiesList()
		{
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = Business.MessageRecipientPartyType.Consignee | Business.MessageRecipientPartyType.SendingAgent | Business.MessageRecipientPartyType.Broker;
			AssertEquals("No Parties available", 0, DummyWorkflowDescriptor.Instance.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs, ProcessTask, Dummy).Count);
			var sendDocumentTriggerParties = DummyWorkflowDescriptor.Instance.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendDocument, ProcessTask, Dummy);
			AssertEquals("Both EML, PRN and ADV available when Send Document", 3, sendDocumentTriggerParties.Count);
			AssertEquals("EML available when Send Document", "Email", sendDocumentTriggerParties[0].Description);
			AssertEquals("PRN available when Send Document", "Print", sendDocumentTriggerParties[1].Description);
			AssertEquals("ADV available when Send Document", "Auto-Deliver using Document Config", sendDocumentTriggerParties[2].Description);
			AssertEquals("All supported (WorkflowDescriptor.SupportedMessageRecipientParties) message trigger parties available when trigger is one of Send XML actions", 3, DummyWorkflowDescriptor.Instance.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, ProcessTask, Dummy).Count);
			AssertEquals("All supported (WorkflowDescriptor.SupportedMessageRecipientParties) message trigger parties available when trigger is one of Send XML actions", 3, DummyWorkflowDescriptor.Instance.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendXML, ProcessTask, Dummy).Count);
			AssertEquals("All supported (WorkflowDescriptor.SupportedMessageRecipientParties) message trigger parties available when trigger is one of Send XML actions or Notification Email action", 3, DummyWorkflowDescriptor.Instance.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback, ProcessTask, Dummy).Count);
		}

		#endregion

		public void TestParentCodeAndParentDescription()
		{
			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			opp.P8_OpportunityID = "O1234";
			opp.P8_OpportunityDescription = "Some Opportunity of mine";
			OpportunityProcessTasks task = (OpportunityProcessTasks)opp.WorkflowItems.AddNew();

			AssertEquals("O1234", task.ParentCode);
			AssertEquals("Some Opportunity of mine", task.ParentDescription);

			AssertEquals("P8_OpportunityID", CodePropertyAttribute.CodePropertyNameFromType(typeof(OrgOpportunity)));
			AssertEquals("P8_OpportunityDescription", DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(OrgOpportunity)));
		}

		[ExpectNoExceptions]
		public void TestParentCodeAndParentDescriptionDoNotThrowExceptionWhenNoCodeAttributeOnParent()
		{
			var task = Factory.New<ProcessTaskForTestToTestParentCodeDescription>();
			var parent = Factory.New<DummytWithWorkflowWithoutCodeDescription>();

			task.P9_ParentID = parent.PK;

			AssertEquals("Parent Code should be empty if Description attribute on Parent is not specified", "", task.ParentCode);
			AssertEquals("Parent Description should be empty if Description attribute on Parent is not specified", "", task.ParentDescription);
		}

		public void TestAreTriggerOrMilestoneActionConditionsMet()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			ProcessTask trigger = dummy.WorkflowItems.Triggers.AddNew();
			Assert("No additional action condition", TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, (StmALog)null, dummy));
			Assert("No additional action condition", TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, (IQueuedLog)null, dummy));

			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger.TriggerConditions.TriggerConditionValue = "MEH";
			Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, (StmALog)null, dummy));
			Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, (IQueuedLog)null, dummy));

			AssertAreTriggerReferenceConditionsMet(trigger, "MEH", true);
			AssertAreTriggerReferenceConditionsMet(trigger, "", false);
			AssertAreTriggerReferenceConditionsMet(trigger, "blkjasdf", false);
			AssertAreTriggerReferenceConditionsMet(trigger, "meH", true);
			AssertAreTriggerReferenceConditionsMet(trigger, "MEH|" + Guid.NewGuid().ToString(), true);
			AssertAreTriggerReferenceConditionsMet(trigger, Guid.NewGuid().ToString(), false);
			AssertAreTriggerReferenceConditionsMet(trigger, "MEH|blkjasdf", false);
		}

		void AssertAreTriggerReferenceConditionsMet(ProcessTask trigger, string eventReference, bool expectedResult)
		{
			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = eventReference;
			}

			AssertEquals(eventReference, expectedResult, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, trigger.GetJob()));

			var queuedLog = new Mock<IQueuedLog>();

			queuedLog.Setup(m => m.SJ_Reference).Returns(eventReference);
			queuedLog.Setup(m => m.SJ_ParentID).Returns(new ZGuid());
			queuedLog.Setup(m => m.SJ_ParentTableCode).Returns("");
			queuedLog.Setup(m => m.SJ_IsEstimate).Returns(false);
			queuedLog.Setup(m => m.SJ_SE_NKEvent).Returns("ARV");

			AssertEquals(eventReference, expectedResult, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, queuedLog.Object, trigger.GetJob()));
		}

		public void TestAreTriggerOrMilestoneActionConditionsMetForExceptionRaised()
		{
			ProcessTask trigger1 = Factory.New<ProcessTask>();
			trigger1.IsWorkflowTrigger = true;
			trigger1.TriggerConditions.TriggerCondition = ExceptionActionConditionList.Codes.ExceptionType;
			trigger1.TriggerConditions.TriggerConditionValue = "EXF";

			ProcessTask trigger2 = Factory.New<ProcessTask>();
			trigger2.IsWorkflowTrigger = true;
			trigger2.TriggerConditions.TriggerCondition = ExceptionActionConditionList.Codes.EventType;
			trigger2.TriggerConditions.TriggerConditionValue = "ARV";

			var job1 = trigger1.GetJob();
			var job2 = trigger2.GetJob();

			StmALog log = Factory.New<StmALog>();
			Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
			Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "EXF";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));

				log.SL_Reference = "Event:[EXF]";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));

				log.SL_Reference = "Type:[EXF]";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));

				log.SL_Reference = "ARV";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));

				log.SL_Reference = "Event:[ARV]";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));

				log.SL_Reference = "Type:[ARV]";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));

				log.SL_Reference = "Type:[ARV]; Event:[EXF]";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));

				log.SL_Reference = "Type:[EXF]; Event:[ARV]";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));

				log.SL_Reference = "Event:[ARV]; Type:[EXF]";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger1, log, job1));
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger2, log, job2));
			}
		}

		public void TestEstimatedPickup_MatchesOnRFPConditionValues()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var shipmentBizo = (BusinessObject)shipment;
			shipmentBizo.FillWithValidTestData();

			Factory.Save();

			var mil = shipmentWorkflow.WorkflowItems.Milestones.AddNew();
			mil.P9_Description = "TheMilestone"; // This milestone is necessary for the EST event to be raised.
			mil.TriggerConditions.TriggerEventCode = "PCF";

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Estimate Pickup";
			trigger.TriggerConditions.TriggerCondition = "RFP";
			trigger.TriggerConditions.TriggerEventCode = "ADD";
			trigger.TriggerConditions.TriggerConditionValue = "TYP=PCF";

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = "XUS";
			triggerAction.PQ_MessagePurpose = "214";

			Factory.Save();

			shipmentBizo["DocsAndCartage+JP_EstimatedPickup"] = ZDateTime.UtcNow;

			var query = new ZQuery { FetchOnlyFromLocalCache = true };
			var logs = Factory.Load<StmALog>(query);
			var estimateEvents = logs.Where(l => l.SL_SE_NKEvent == "EST").ToArray();

			AssertEquals(1, estimateEvents.Length);
			AssertEquals(true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, estimateEvents[0], shipmentBizo));
		}

		public void TestRegexTriggerConditionValueValidation()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var shipmentBizo = (BusinessObject)shipment;
			shipmentBizo.FillWithValidTestData();

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Estimate Pickup";
			trigger.TriggerConditions.TriggerCondition = "RFR";
			trigger.TriggerConditions.TriggerEventCode = "ADD";
			trigger.TriggerConditions.TriggerConditionValue = "*";
			Assert(trigger.HasErrors);

			trigger.TriggerConditions.TriggerConditionValue = ".*";
			AssertEquals(false, trigger.HasErrors);
		}

		public void TestEventReferenceWithWildcardsTrigger_ShouldFireForOrgStaffAssignmentLogs()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_FullName = "JEAN VALJEAN";

			var trigger = job.WorkflowItems.Triggers.AddNew();

			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystem.Code;
			trigger.P9_Description = "The miserable trigger";
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = "Staff Assignment*";

			var staff = job.StaffAssignments.AddNew();
			staff.O8_Role = "CON";
			staff.O8_GS_NKPersonResponsible = "JVJ";

			Factory.Save();

			AssertEquals(2, job.GetLogs().DatabaseCount);

			var log = job.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.AddedARecordToTheSystem.Code))[0];
			AssertEquals(ZString.Empty, log.SL_Reference);

			AssertEquals(1, staff.GetLogs().DatabaseCount);

			var staffLog = staff.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.AddedARecordToTheSystem.Code))[0];
			AssertEquals("Staff Assignment - Role CON Department ALL", staffLog.SL_Reference);

			AssertNotEquals("Workflow should not fire when SL_Reference for an ADD event is updated", log.SL_EventTime, trigger.P9_ActualDate);
			AssertEquals("Workflow should fire when SL_Reference for an ADD event is updated", staffLog.SL_EventTime, trigger.P9_ActualDate);
		}

		public void TestAreTriggerOrMilestoneActionConditionsMetWithWildcards()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger.TriggerConditions.TriggerConditionValue = "M*H";

			Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, (StmALog)null, dummy));

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "m*h";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "blkjasdf";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "mh";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "meh";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "maeiouh";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "meha";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
				trigger.TriggerConditions.TriggerConditionValue = "M*H";

				log.SL_Reference = "m*h";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "blkjasdf";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "mh";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "meh";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "maeiouh";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "meha";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));
			}
		}

		public void TestAreTriggerOrMilestoneActionConditionsMetWithRegularExpressions()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.IsWorkflowTrigger = true;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger.TriggerConditions.TriggerConditionValue = @"([A-Za-z ]* )?[+-]\$[0-9]+(\.[0-9]{2})?";

			Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, (StmALog)null, dummy));

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = @"([A-Za-z ]* )?[+-]\$[0-9]+(\.[0-9]{2})?";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "Luke was here :D";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "this is OK +$5.00";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "and this -$514";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "-$7 not this though";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "0R th!s$21.2";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "+$21394.50";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions;
				trigger.TriggerConditions.TriggerConditionValue = @"([A-Za-z ]* )?[+-]\$[0-9]+(\.[0-9]{2})?";

				log.SL_Reference = "Luke was here :D";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "this is OK +$5.00";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "and this -$514";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "-$7 not this though";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "0R th!s$21.2";
				Assert(!TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));

				log.SL_Reference = "+$21394.50";
				Assert(TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, dummy));
			}
		}

		public void TestAreTriggerOrMilestoneActionConditionsMet_ConditionWithMacros_NoMixMatchBetweenBusinessObjectAndEventDataModel()
		{
			using (WorkflowDataRegistry.Instance.McrDataFieldMapEnhancements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				CheckAreTriggerOrMilestoneActionConditionsMet_UsingJobProperties(EventReferenceConditionList.Codes.ConditionWithMacros, "LoadPort == \"UADOC\" && Source.DischargePort == \"RUPKV\"", false);
				CheckAreTriggerOrMilestoneActionConditionsMet_UsingJobProperties(EventReferenceConditionList.Codes.ConditionWithMacros, "Source.LoadPort == \"UADOC\" && DischargePort == \"RUPKV\"", false);
			}
		}

		public void TestAreTriggerOrMilestoneActionConditionsMet_ConditionWithMacros_MatchBusinessObjectOrEventDataModelFallback()
		{
			CheckAreTriggerOrMilestoneActionConditionsMet_UsingJobProperties(EventReferenceConditionList.Codes.ConditionWithMacros, "LoadPort == \"UADOC\" && DischargePort == \"RUPKV\"");
			CheckAreTriggerOrMilestoneActionConditionsMet_UsingJobProperties(EventReferenceConditionList.Codes.ConditionWithMacros, "Source.LoadPort == \"UADOC\" && Source.DischargePort == \"RUPKV\"");
		}

		public void TestAreTriggerOrMilestoneActionConditionsMet_ConditionWithMacros()
		{
			CheckAreTriggerOrMilestoneActionConditionsMet_UsingJobProperties(EventReferenceConditionList.Codes.ConditionWithMacros, "Source.LoadPort == \"UADOC\" && Source.DischargePort == \"RUPKV\"");
		}

		public void TestAreTriggerOrMilestoneActionConditionsMet_UserDefined()
		{
			CheckAreTriggerOrMilestoneActionConditionsMet_UsingJobProperties(EventReferenceConditionList.Codes.UserDefined, "\"<LoadPort>\" == \"UADOC\" && \"<DischargePort>\" == \"RUPKV\"");
		}

		public void CheckAreTriggerOrMilestoneActionConditionsMet_UsingJobProperties(string triggerCondition, string triggerConditionValue, bool isEvaluable = true)
		{
			Func<string, string, string, string> msg = (condition, loadPort, dischargePort) =>
			{
				return string.Format("The condition '{0}' when source's load port is '{1}' and source's discharge port is '{2}' is met", condition, loadPort, dischargePort);
			};

			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = triggerCondition;
			trigger.IsWorkflowTrigger = true;

			var log = Dummy.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				trigger.TriggerConditions.TriggerConditionValue = triggerConditionValue;

				Dummy.LoadPort = "UAIEV";
				Dummy.DischargePort = "RUPKV";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, Dummy.LoadPort, Dummy.DischargePort), false, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

				Dummy.LoadPort = "UADOC";
				Dummy.DischargePort = "RUPKV";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, Dummy.LoadPort, Dummy.DischargePort), isEvaluable, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestAreTriggerOrMilestoneActionConditionsMet_UserDefined_WithDates()
		{
			Func<string, string> msg = (condition) => string.Format("Condition: '{0}' is met", condition);

			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.IsWorkflowTrigger = true;

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				trigger.TriggerConditions.TriggerConditionValue = "\"<DateTimeAsString('<NOW>', 'dddd')>\" == \"Saturday\"";

				AssertEquals(msg(trigger.P9_TriggerConditionValue), false, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

				TestDateAttribute.AddDays(1);
				AssertEquals(msg(trigger.P9_TriggerConditionValue), true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

				trigger.TriggerConditions.TriggerConditionValue = "\"<DateTimeAsString('<NOW>', 'dddd')>\" == \"Friday\"";
				AssertEquals(msg(trigger.P9_TriggerConditionValue), false, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));
			}
		}

		public void TestAreTriggerOrMilestoneActionConditionsMet_EventReferenceParameters()
		{
			Func<string, string, string> msg = (condition, eventReference) => string.Format("Condition: '{0}' with EventReference: '{1}' is met", condition, eventReference);

			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger.IsWorkflowTrigger = true;

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				trigger.TriggerConditions.TriggerConditionValue = "LOC=UAIEV,NAM=MCLAREN,REF=FREETEXT".Replace("REF", Constants.EventReferenceReservedParameters.Codes.Reference);

				log.Parameters["LOC"] = "UAIEV";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, log.SL_Reference), false, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

				log.Parameters["NAM"] = "MCLAREN";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, log.SL_Reference), false, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

				log.ReferenceFreeText = "FREETEXT";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, log.SL_Reference), true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

				log.Parameters["LOC"] = "UAIEV";
				log.Parameters["NAM"] = "ManUtd";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, log.SL_Reference), false, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

				trigger.TriggerConditions.TriggerConditionValue = "LOC=<Origin>,NAM=ManUtd";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, log.SL_Reference), true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));
			}
		}

		public void TestAreTriggerOrMilestoneActionConditionsMet_EventReferenceParameters_ConditionValueIsEmpty()
		{
			Func<string, string, string> msg = (condition, eventReference) => string.Format("Condition: '{0}' with EventReference: '{1}' is met", condition, eventReference);

			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger.TriggerConditions.TriggerConditionValue = "LOC=,NAM=MCLAREN,REF=FREETEXT".Replace("REF", Constants.EventReferenceReservedParameters.Codes.Reference);

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.Parameters["NAM"] = "MCLAREN";
				log.ReferenceFreeText = "FREETEXT";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, log.SL_Reference), true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

				log.Parameters["LOC"] = "";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, log.SL_Reference), true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

				log.Parameters["LOC"] = "UAIEV";
				AssertEquals(msg(trigger.P9_TriggerConditionValue, log.SL_Reference), false, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));
			}
		}

		#region P9_TriggerConditionValue

		public void TestP9_TriggerConditionValue()
		{
			var task = Factory.New<ProcessTask>();
			task.IsWorkflowTrigger = true;
			task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;

			AssertEquals("", task.P9_TriggerConditionValue);

			task.TriggerConditions.TriggerConditionValue = "testvalue";
			AssertEquals("testvalue", task.P9_Notes.ToAscii());

			task.P9_Notes = ZBlob.FromUTF8("MEH");
			AssertEquals("MEH", task.P9_TriggerConditionValue);

			task.TriggerConditions.TriggerCondition = "";
			AssertEquals("Should be cleared", ZBlob.FromAscii("MEH"), task.P9_Notes);
		}

		public void TestP9_TriggerConditionValue_ActualLogExists_UpdateEventReferenceAccordingToCondition()
		{
			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReference,
				newConditionValue: "",
				expectedReference: "");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReference,
				newConditionValue: "McLaren",
				expectedReference: "McLaren");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions,
				newConditionValue: "McLaren",
				expectedReference: "McLaren");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions,
				newConditionValue: "[a-zA-Z]{5}",
				expectedReference: "[a-zA-Z]{5}");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				newConditionValue: "McLaren",
				expectedReference: "McLaren");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				newConditionValue: "McL*ren",
				expectedReference: "McL*ren");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				newConditionValue: "McL?ren",
				expectedReference: "McL?ren");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				newConditionValue: @"McLar\*en",
				expectedReference: @"McLar\*en");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceParameters,
				newConditionValue: "NAM=aaa,SRV=<bbb>,WHS=<cc,LOC=<Origin>",
				expectedReference: "NAM=aaa,SRV=<bbb>,WHS=<cc,LOC=<Origin>",
				expectedParams: new Dictionary<string, string> { });

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceParameters,
				newConditionValue: "NAM=aaa,SRV=<bbb>,LOC=<Origin>",
				expectedReference: "NAM=aaa,SRV=<bbb>,LOC=<Origin>",
				expectedParams: new Dictionary<string, string> { });

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceParameters,
				newConditionValue: "NAM=aaa,LOC=<Origin>",
				expectedReference: "|LOC=UAIEV|NAM=aaa",
				expectedParams: new Dictionary<string, string> { { "NAM", "aaa" }, { "LOC", "UAIEV" } });
		}

		[TestDate(2014, 8, 24)]
		public void TestP9_TriggerConditionValue_EstimatedLogNewStyleExists_UpdateEventReferenceAccordingToCondition()
		{
			EnsureEventReferenceUpdatedNewStyle(
				condition: EventReferenceConditionList.Codes.EventReference,
				isLogEstimated: true,
				oldConditionValue: "ManUtd",
				oldReference: "ManUtd|TYP=ARV|NEW=24-Aug-14",
				newConditionValue: "",
				expectedReference: "|TYP=ARV|NEW=24-Aug-14");

			EnsureEventReferenceUpdatedNewStyle(
				condition: EventReferenceConditionList.Codes.EventReference,
				isLogEstimated: true,
				oldConditionValue: "ManUtd",
				oldReference: "ManUtd|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14",
				newConditionValue: "McLaren|TYP=ARV|NEW=24-Aug-14",
				expectedReference: "McLaren|TYP=ARV|NEW=24-Aug-14");

			EnsureEventReferenceUpdatedNewStyle(
				condition: EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions,
				isLogEstimated: true,
				oldConditionValue: "ManUtd",
				oldReference: "ManUtd|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14",
				newConditionValue: "McLaren",
				expectedReference: "McLaren|TYP=ARV|NEW=24-Aug-14");

			EnsureEventReferenceUpdatedNewStyle(
				condition: EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions,
				isLogEstimated: true,
				oldConditionValue: "[a-z]*",
				oldReference: "manutd|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14",
				newConditionValue: "[A-Z]*",
				expectedReference: "[A-Z]*|TYP=ARV|NEW=24-Aug-14");

			EnsureEventReferenceUpdatedNewStyle(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				isLogEstimated: true,
				oldConditionValue: "ManUt?",
				oldReference: "ManUtd|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14",
				newConditionValue: "McLaren|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14",
				expectedReference: "McLaren|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14");

			EnsureEventReferenceUpdatedNewStyle(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				isLogEstimated: true,
				oldConditionValue: "ManUt?",
				oldReference: "ManUtd|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14",
				newConditionValue: @"McLar\*en|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14",
				expectedReference: @"McLar\*en|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14");

			EnsureEventReferenceUpdatedNewStyle(
				condition: EventReferenceConditionList.Codes.EventReferenceParameters,
				oldReference: "ManUtd|TYP=ARV|NEW=24-Aug-14",
				isLogEstimated: true,
				newConditionValue: "NAM=aaa,LOC=<Origin>",
				expectedReference: "NAM=aaa|LOC=UAEIV|TYP=ARV|OLD=24-Aug-14|NEW=24-Aug-14",
				expectedParams: new Dictionary<string, string> { { "NAM", "aaa" }, { "LOC", "UAIEV" }, { "TYP", "ARV" }, { "NEW", "24-Aug-14" } });
		}

		[TestDate(2014, 8, 24)]
		public void TestP9_TriggerConditionValue_EstimatedLogExists_UpdateEventReferenceAccordingToCondition()
		{
			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReference,
				isLogEstimated: true,
				oldConditionValue: "ManUtd",
				oldReference: "ManUtd From: 24-Aug-14 To: 24-Aug-14",
				newConditionValue: "",
				expectedReference: "From: 24-Aug-14 To: 24-Aug-14");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReference,
				isLogEstimated: true,
				oldConditionValue: "ManUtd",
				oldReference: "ManUtd From: 24-Aug-14 To: 24-Aug-14",
				newConditionValue: "McLaren",
				expectedReference: "McLaren From: 24-Aug-14 To: 24-Aug-14");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions,
				isLogEstimated: true,
				oldConditionValue: "ManUtd",
				oldReference: "ManUtd From: 24-Aug-14 To: 24-Aug-14",
				newConditionValue: "McLaren",
				expectedReference: "McLaren From: 24-Aug-14 To: 24-Aug-14");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions,
				isLogEstimated: true,
				oldConditionValue: "[a-z]*",
				oldReference: "manutd From: 24-Aug-14 To: 24-Aug-14",
				newConditionValue: "[A-Z]*",
				expectedReference: "[A-Z]* From: 24-Aug-14 To: 24-Aug-14");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				isLogEstimated: true,
				oldConditionValue: "ManUt?",
				oldReference: "ManUtd From: 24-Aug-14 To: 24-Aug-14",
				newConditionValue: "McLaren",
				expectedReference: "McLaren From: 24-Aug-14 To: 24-Aug-14");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				isLogEstimated: true,
				oldConditionValue: "ManUt?",
				oldReference: "ManUtd From: 24-Aug-14 To: 24-Aug-14",
				newConditionValue: "McL*ren",
				expectedReference: "From: 24-Aug-14 To: 24-Aug-14");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				isLogEstimated: true,
				oldConditionValue: "ManUt?",
				oldReference: "ManUtd From: 24-Aug-14 To: 24-Aug-14",
				newConditionValue: "McL?ren",
				expectedReference: "From: 24-Aug-14 To: 24-Aug-14");

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				isLogEstimated: true,
				oldConditionValue: "ManUt?",
				oldReference: "ManUtd From: 24-Aug-14 To: 24-Aug-14",
				newConditionValue: @"McLar\*en",
				expectedReference: @"From: 24-Aug-14 To: 24-Aug-14");
		}

		void EnsureEventReferenceUpdated(string condition, string newConditionValue, bool isLogEstimated = false, string oldConditionValue = "", string oldReference = "", string expectedReference = "", IDictionary<string, string> expectedParams = null)
		{
			var businessObject = Factory.New<DummyWithWorkflow>();

			var milestone = businessObject.WorkflowItems.AddNew();
			milestone.IsMilestone = true;
			milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone.TriggerConditions.TriggerCondition = condition;
			milestone.TriggerConditions.TriggerConditionValue = oldConditionValue;
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
			milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now;

			// Deleting automatically generated events
			businessObject.Logs.RemoveAndDeleteAll();

			var log = businessObject.Logs.AddNew(Events.Arrival, oldReference, ZDateTimeOffset.Now, isLogEstimated);

			milestone.TriggerConditions.TriggerConditionValue = newConditionValue;

			if (expectedParams != null)
			{
				AssertEquals("Parameters count on the log" + string.Join("|", log.Parameters.Select(s => s.Key + "=" + s.Value).ToArray()), expectedParams.Count, log.Parameters.Count);

				foreach (var p in expectedParams)
				{
					AssertEquals(string.Format("Value for parameter with name '{0}'", p.Key), p.Value, log.Parameters[p.Key]);
				}
			}
			AssertEndsWith("Reference on event", expectedReference, log.SL_Reference);
		}

		void EnsureEventReferenceUpdatedNewStyle(string condition, string newConditionValue, bool isLogEstimated = false, string oldConditionValue = "", string oldReference = "", string expectedReference = "", IDictionary<string, string> expectedParams = null)
		{
			var businessObject = Factory.New<DummyWithWorkflow>();

			var milestone = businessObject.WorkflowItems.AddNew();
			milestone.IsMilestone = true;
			milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone.TriggerConditions.TriggerCondition = condition;
			milestone.TriggerConditions.TriggerConditionValue = oldConditionValue;
			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
			milestone.P9_ActualDateForBinding = ZDateTimeOffset.Now;

			// Deleting automatically generated events
			businessObject.Logs.RemoveAndDeleteAll();

			var log = businessObject.Logs.AddNew(Events.EstimatedDateChanged, oldReference, ZDateTimeOffset.Now, false);

			milestone.TriggerConditions.TriggerConditionValue = newConditionValue;

			if (expectedParams != null)
			{
				AssertEquals("Parameters count on the log", expectedParams.Count, log.Parameters.Count);

				foreach (var p in expectedParams)
				{
					AssertEquals(string.Format("Value for parameter with name '{0}'", p.Key), p.Value, log.Parameters[p.Key]);
				}
			}
			else
			{
				Assert("Reference on event (new style)", log.CheckReferenceEquals(expectedReference));
			}
		}

		#endregion

		public void TestUpdateP9_EstDuration_ShouldNotUpdateEstimateToComplete()
		{
			var task = Factory.New<ProcessTask>();

			var now = ZDateTime.Now;
			var oneHourAgo = ZDateTime.Now.AddHours(-1);
			var twoHoursAgo = ZDateTime.Now.AddHours(-2);

			task.P9_EstDuration = now;
			task.P9_EstimatedTimeToComplete = oneHourAgo;

			var duration = now - new ZDateTime(now.Year, 1, 1);
			var durationBasedValue = ZDateTime.DefaultDurationEpoch + duration;

			AssertEquals("Initial value", durationBasedValue, task.P9_EstDuration);
			AssertEquals("Initial value", durationBasedValue.AddHours(-1), task.P9_EstimatedTimeToComplete);

			task.P9_EstDuration = now;

			AssertEquals("Updated value", durationBasedValue, task.P9_EstDuration);
			AssertEquals("Value unchanged - was different from EstDuration", durationBasedValue.AddHours(-1), task.P9_EstimatedTimeToComplete);

			task.P9_EstimatedTimeToComplete = now;
			task.P9_EstDuration = twoHoursAgo;

			AssertEquals("Updated value", durationBasedValue.AddHours(-2), task.P9_EstDuration);
			AssertEquals("Value unchanged - was invalid ZDateTime", durationBasedValue, task.P9_EstimatedTimeToComplete);
		}

		public void TestWorkflowSequence()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = (BMSystem)helper.CreateSystem(Factory, "ORG");

			var task = Factory.New<ProcessTask>();
			AssertEquals(ZString.Empty, task.WorkflowSequence);

			var jobHeader = ProcessJobHeaderProvider.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var link = Factory.New<ProcessHeaderLink>();
			link.FP_FH_HeaderFrom = jobHeader.ProcessHeaders.AddNew().PK;
			link.FP_FH_HeaderTo = jobHeader.ProcessHeaders[0].PK;
			link.FP_LinkType = "DEP";
			task.P9_FH_ProcessHeader = jobHeader.ProcessHeaders[0].PK;
			AssertEquals("2", task.WorkflowSequence);
		}

		public void TestAssignedGroupCode()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Factory.Save();
			var task = Factory.New<ProcessTask>();
			task.P9_GG_AssignedGroupCode = group.GG_Code;
			AssertEquals(group.PK, task.P9_GG_AssignedGroup);
			task.P9_GG_AssignedGroupCode = "";
			AssertEquals(ZGuid.Empty, task.P9_GG_AssignedGroup);
		}

		#region TimeBecameStartable

		[TestDate(2021, 9, 20, 5, 10, 30)]
		public void TestTimeBecameStartable_WithNoChangesAfterInitialisation()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			UpdateWeekDaysTo9To5();

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_ReleaseDateTime.AddSeconds(-3); // to make sure FH_ReleaseDateTime does not override event posted time

			var task1 = helper.CreateTask(workflow, taskType: "UDF", sequence: 1);
			var task2 = helper.CreateTask(workflow, taskType: "UDF", sequence: 2);

			Factory.Save();

			// task1: SRT=Y
			// task2: none
			CombineAssertions(() =>
			{
				AssertEquals("task1.TimeBecameStartable", new ZDateTime(2021, 9, 20, 15, 10, 30), task1.TimeBecameStartable);
				AssertEquals("task2.TimeBecameStartable", ZDateTime.Empty, task2.TimeBecameStartable);
			});
		}

		[TestDate(2021, 9, 20, 5, 10, 30)]
		public void TestTimeBecameStartable_And_WorkingTimeSinceBecomingStartable_WithTaskStartabilityChange()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_FC_CurrentComponent = buffer.PK;

			var task1 = helper.CreateTask(workflow, taskType: "UDF", sequence: 1);
			var task2 = helper.CreateTask(workflow, taskType: "UDF", sequence: 2);

			Factory.Save();

			// task1: SRT=Y
			// task2: none
			CombineAssertions(() =>
			{
				AssertEquals("task1.TimeBecameStartable", new ZDateTime(2021, 9, 20, 15, 10, 30), task1.TimeBecameStartable);
				AssertEquals("task2.TimeBecameStartable", ZDateTime.Empty, task2.TimeBecameStartable);
			});

			TestDateAttribute.AddMinutes(4);
			task1.P9_Sequence = 3;
			Factory.Save();

			// task1: SRT=Y, SRT=N
			// task2: SRT=Y
			CombineAssertions(() =>
			{
				AssertEquals("task1.TimeBecameStartable", ZDateTime.Empty, task1.TimeBecameStartable);
				AssertEquals("task2.TimeBecameStartable", new ZDateTime(2021, 9, 20, 15, 14, 30), task2.TimeBecameStartable);
			});

			TestDateAttribute.AddMinutes(67);

			CombineAssertions(() =>
			{
				AssertEquals("task1.WorkingTimeSinceBecomingStartable", TimeSpan.Zero, (task1 as ProcessTask).WorkingTimeSinceBecomingStartable);
				AssertEquals("task2.WorkingTimeSinceBecomingStartable", new TimeSpan(1, 7, 0), (task2 as ProcessTask).WorkingTimeSinceBecomingStartable);
				AssertEquals("task2.WorkingTimeSinceBecomingStartable", "-", (task1 as ProcessTask).WorkingTimeSinceBecomingStartableAsString);
				AssertEquals("task1.WorkingTimeSinceBecomingStartableAsString", "1:07", (task2 as ProcessTask).WorkingTimeSinceBecomingStartableAsString);
			});
		}

		[TestDate(2021, 9, 20, 5, 10, 30)]
		public void TestTimeBecameStartable_WithReleaseTimeLaterThanEvent()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.Now.AddSeconds(3); // to make sure FH_ReleaseDateTime overrides event posted time

			var task = helper.CreateTask(workflow, taskType: "UDF", sequence: 1);
			Factory.Save();

			AssertEquals("task.TimeBecameStartable", new ZDateTime(2021, 9, 20, 15, 10, 33), task.TimeBecameStartable);
		}

		[TestDate(2021, 9, 20, 5, 10, 30)]
		public void TestTimeBecameStartable_ForTaskWithNoProcessHeader()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var helper = ObjectFactory.Get<IBMTestHelper>();
			var system = helper.CreateSystem(Factory, "DUM");
			var buffer = helper.CreateBuffer(system);

			var job = Factory.New<OrgHeader>();
			job.OH_Code = "MOO";

			var task = helper.CreateTask(job, taskType: "UDF", sequence: 1);
			Factory.Save();

			AssertEquals("task.TimeBecameStartable", ZDateTime.Empty, task.TimeBecameStartable);
		}

		#endregion

		#endregion

		#region Related Business Objects

		public void TestParentType()
		{
			DummyProcessTask task = Factory.New<DummyProcessTask>();
			AssertEquals("Type is DummyWithWorkflow", typeof(DummyWithWorkflow), typeof(ProcessTask).InvokeMember("ParentType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, task, null));
		}

		public void TestParent_ForNonStandAloneTask()
		{
			ProcessTask task = Dummy.WorkflowItems.Tasks.AddNew();
			AssertEquals("Parent for a task", Dummy, task.Parent);

			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			AssertEquals("Parent for a milestone", Dummy, milestone.Parent);
		}

		public void TestParent_ForStandAloneTask()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			AssertNull("Task has no parent - is stand alone task", task.Parent);
		}

		public void TestAddParentFetchHint()
		{
			ProcessTask task1 = Dummy.WorkflowItems.Tasks.AddNew();
			task1.P9_ParentID = ZGuid.NewZGuid();
			task1.AddParentFetchHint();

			ProcessTask task2 = Dummy.WorkflowItems.Tasks.AddNew();
			task2.P9_ParentID = ZGuid.NewZGuid();
			task2.AddParentFetchHint();

			ProcessTask task3 = Dummy.WorkflowItems.Tasks.AddNew();
			task3.P9_ParentID = ZGuid.NewZGuid();
			task3.AddParentFetchHint();

			AssertNull(task1.Parent);
			AssertNull(task2.Parent);
			AssertNull(task3.Parent);

			AssertEquals("Three db hit to load all parents", 3, Factory.GetTableHitCount(DummyBizoSchema.Constants.TableName));
		}

		public void TestSetEventDate_DbHits()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var templateTrigger1 = template.WorkflowItems.Triggers.AddNew();
			var templateTrigger2 = template.WorkflowItems.Triggers.AddNew();

			templateTrigger1.TriggerConditions.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger2.TriggerConditions.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponentsCode;

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedJob = newFactory.Load<DummyWithWorkflow>(job.PK);

			AssertEquals(2, loadedJob.WorkflowItems.Triggers.Count);
			newFactory.ResetDatabaseLoadCount();

			using (AssertDbHitsWithUsefulQueryInformation(new Dictionary<string, int>
			{
				{ ProcessTaskNotificationSchema.Constants.TableName, 2 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 1 },
			}, newFactory))
			{
				loadedJob.GetLogs().AddNew(Events.TagWasAddedOrRemoved);
				loadedJob.GetLogs().AddNew(Events.WorkflowTransferredBetweenSystemComponents);
			}
		}

		public void TestSetEventDate_OnTask_ShouldNotHitTemplateVersion()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedJob = newFactory.Load<DummyWithWorkflow>(job.PK);

			AssertEquals(1, loadedJob.WorkflowItems.Tasks.Count);
			newFactory.ResetDatabaseLoadCount();

			loadedJob.WorkflowItems.Tasks[0].TaskProperties.ActualDate = ZDateTimeOffset.UtcNow;

			AssertDbHits(new Dictionary<string, int>(), newFactory);
		}

		#endregion

		#region Workflow Type

		public void TestWorkflowType()
		{
			ProcessTask standAloneTask = Factory.New<ProcessTask>();
			OrgOpportunity parent = Factory.New<OrgOpportunity>();
			ProcessTask taskWithParent = parent.WorkflowItems.Tasks.AddNew();
			taskWithParent.P9_ParentID = parent.PK;
			taskWithParent.P9_ParentTableCode = OrgOpportunitySchema.Constants.Prefix;

			AssertEquals("StandAloneTask.WorkflowType", "STA", standAloneTask.WorkflowType);
			AssertEquals("TaskWithParent.WorkflowType", ((IWorkflowProvider)parent).WorkflowType, taskWithParent.WorkflowType);
		}

		public void TestWorkflowTypeGetter_LineTriggerTypeIsSpecified_ReturnLineTriggerWorkflowType()
		{
			var processTask = Factory.New<ProcessTask>();
			processTask.P9_LineTriggerType = "DUM";

			AssertEquals("WorkflowType", "DUM", processTask.WorkflowType);
		}

		public void TestP9_LineTriggerType_ReadOnly()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			AssertEquals("This task does not support line triggers so should be readonly", true, task.P9_LineTriggerType_ReadOnly);

			task = Dummy.WorkflowItems.Triggers.AddNew();
			AssertEquals("The Dummy Bizo supports line triggers", false, task.P9_LineTriggerType_ReadOnly);
			task.P9_LineTriggerType = "XXX";
			AssertEquals(false, task.P9_LineTriggerType_ReadOnly);
			Factory.Save();
			AssertEquals(true, task.P9_LineTriggerType_ReadOnly);
		}

		public void TestProcessTaskNewDescription()
		{
			ProcessTask task1 = Factory.New<ProcessTask>();
			AssertEquals("Task Desription should be empty", ZString.Empty, task1.P9_Description);

			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();
			ZString taskTypeForNotSendingAppointment = "UTF";
			ZString taskTypeForSendingAppointment = "UTA";
			CategorisedWorkflowTaskTypes parent2 = collection.AddNew();
			parent2.Code = task1.WorkflowType;

			WorkflowTaskType setsAppointment2 = parent2.TaskTypes.AddNew();
			setsAppointment2.Code = taskTypeForSendingAppointment;
			setsAppointment2.Description = (NoResString)"fdsfsdf";
			setsAppointment2.Bool = true;

			WorkflowTaskType doesntSetAppointment2 = parent2.TaskTypes.AddNew();
			doesntSetAppointment2.Code = taskTypeForNotSendingAppointment;
			doesntSetAppointment2.Description = (NoResString)"very long descriptionnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn";
			doesntSetAppointment2.Bool = false;
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals("Task Desription should be empty", ZString.Empty, task1.P9_Description);
			task1.P9_Type = taskTypeForSendingAppointment;

			AssertEquals("Task Desription should be fdsfsdf", "fdsfsdf", task1.P9_Description);
			task1.P9_Description = ZString.Empty;
			task1.P9_Type = taskTypeForNotSendingAppointment;
			AssertEquals("Task Desription should be shortend", task1.TypeDescription.Substring(0, task1.P9_DescriptionInfo.MaxLength - 1), task1.P9_Description);
		}

		#endregion

		#region Working Tasks Event

		class ChangeTrackingITaskStatusChangeConflictResolverProvider : ITaskStatusChangeConflictResolver
		{
			public ITaskStatusChangeConflictResolution GetExistingWorkTaskExists(IProcessTask task)
			{
				ExistingWorkTaskExistsEventFiredCount++;
				return DoTheNeedful(task);
			}

			TaskStatusChangeConflictResolution DoTheNeedful(IProcessTask task)
			{
				LastExistingTask = (ProcessTask)task;
				if (ShouldContinue)
				{
					return TaskStatusChangeConflictResolution.Continue();
				}
				else
				{
					return TaskStatusChangeConflictResolution.SelectAlternative(TaskToStart);
				}
			}

			public ITaskStatusChangeConflictResolution GetExistingSuspendedTaskExists(IProcessTask task)
			{
				ExistingSuspendedTaskExistsEventFiredCount++;
				return DoTheNeedful(task);
			}

			public TimeSpan GetWorkingDayChoice(TimeSpan withoutWorkingDays, TimeSpan withWorkingDays) => withoutWorkingDays;

			public int ExistingWorkTaskExistsEventFiredCount { get; private set; }
			public int ExistingSuspendedTaskExistsEventFiredCount { get; private set; }

			public bool ShouldContinue { get; set; } = true;
			public ProcessTask LastExistingTask { get; set; }
			public ProcessTask TaskToStart { get; set; }
		}

		public void TestExistingTaskEventThrownWhenWorkingTasks()
		{
			GlbStaff otherUser = Factory.New<GlbStaff>();
			otherUser.GS_Code = "OTH";

			ProcessTask task1 = Factory.New<ProcessTask>();
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			ProcessTask task2 = Factory.New<ProcessTask>();
			task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			ProcessTask task3 = Factory.New<ProcessTask>();
			task3.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			ProcessTask task4 = Factory.New<ProcessTask>();
			task4.P9_GS_NKAssignedStaffMember = otherUser.GS_Code;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var conflictResolverProvider = new ChangeTrackingITaskStatusChangeConflictResolverProvider();
			Factory.ClearCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver));
			Factory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => conflictResolverProvider);

			AssertNull(conflictResolverProvider.LastExistingTask);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertNull(conflictResolverProvider.LastExistingTask);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);

			AssertNull(conflictResolverProvider.LastExistingTask);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(task1, conflictResolverProvider.LastExistingTask);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
			conflictResolverProvider.LastExistingTask = null;

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(task2, conflictResolverProvider.LastExistingTask);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task3.P9_Status);
			conflictResolverProvider.LastExistingTask = null;

			conflictResolverProvider.ShouldContinue = false;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(task3, conflictResolverProvider.LastExistingTask);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task3.P9_Status);
			conflictResolverProvider.LastExistingTask = null;

			conflictResolverProvider.ShouldContinue = true;
			task4.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			AssertEquals(task3, conflictResolverProvider.LastExistingTask);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task3.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task4.P9_Status);
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Working; // Since we reset Working tasks to Assigned when changing the assigned resource
			conflictResolverProvider.LastExistingTask = null;

			conflictResolverProvider.ShouldContinue = false;
			conflictResolverProvider.TaskToStart = task2;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(task4, conflictResolverProvider.LastExistingTask);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task3.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, task4.P9_Status);
			conflictResolverProvider.LastExistingTask = null;
		}

		#endregion

		#region IsTask / IsStandaloneTask

		public void TestIsTask_IsStandaloneTask()
		{
			var task = Dummy.WorkflowItems.Tasks.AddNew();
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			var standaloneTask = Factory.New<ProcessTask>();

			AssertEquals(true, task.IsTask);
			AssertEquals(false, trigger.IsTask);
			AssertEquals(false, milestone.IsTask);
			AssertEquals(false, exception.IsTask);
			AssertEquals(true, standaloneTask.IsTask);

			AssertEquals(false, task.IsStandaloneTask);
			AssertEquals(false, trigger.IsStandaloneTask);
			AssertEquals(false, milestone.IsStandaloneTask);
			AssertEquals(false, exception.IsStandaloneTask);
			AssertEquals(true, standaloneTask.IsStandaloneTask);
		}

		#endregion

		#region Triggers, Milestones and Exceptions

		public void TestMergingWithTriggerField()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTask taskTemplate1 = template.WorkflowItems.AddNew();
			taskTemplate1.P9_Type = Core.Constants.Workflow.MilestoneType;
			ProcessTask taskTemplate2 = template.WorkflowItems.AddNew();
			taskTemplate2.P9_Type = Core.Constants.Workflow.MilestoneType;

			Assert(taskTemplate1.IsMatchForItemTemplateMerge(taskTemplate2));
			taskTemplate2.TriggerConditions.TriggerFieldName = "ABC";
			Assert(!taskTemplate1.IsMatchForItemTemplateMerge(taskTemplate2));
			taskTemplate1.TriggerConditions.TriggerFieldName = "ABC";
			Assert(taskTemplate1.IsMatchForItemTemplateMerge(taskTemplate2));
		}

		public void TestIsMatchForItemTemplateMerge_ForMilestone()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTask taskTemplate = template.WorkflowItems.AddNew();
			ProcessTask arrivalMilestoneTemplate = template.WorkflowItems.AddNew();
			arrivalMilestoneTemplate.IsMilestone = true;
			arrivalMilestoneTemplate.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			ProcessTask departureMilestoneTemplate = template.WorkflowItems.AddNew();
			departureMilestoneTemplate.IsMilestone = true;
			departureMilestoneTemplate.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			ProcessTask task = Dummy.WorkflowItems.AddNew();
			ProcessTask arrivalMilestone = Dummy.WorkflowItems.AddNew();
			arrivalMilestone.IsMilestone = true;
			arrivalMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			ProcessTask departureMilestone = Dummy.WorkflowItems.AddNew();
			departureMilestone.IsMilestone = true;
			departureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			AssertEquals("An existing task can never be matched to a task template, ie. a new task is always created from a task template", false, task.IsMatchForItemTemplateMerge(taskTemplate));
			AssertEquals("An existing milestone matches when the event type is the same", true, arrivalMilestone.IsMatchForItemTemplateMerge(arrivalMilestoneTemplate));
			AssertEquals("An existing milestone doesn't match when the event type is different", false, arrivalMilestone.IsMatchForItemTemplateMerge(departureMilestoneTemplate));
		}

		public void TestIsMatchForItemTemplateMerge_ForTrigger()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTask taskTemplate = template.WorkflowItems.AddNew();
			ProcessTask arrivalTriggerTemplate = template.WorkflowItems.AddNew();
			arrivalTriggerTemplate.IsWorkflowTrigger = true;
			arrivalTriggerTemplate.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			ProcessTask departureTriggerTemplate = template.WorkflowItems.AddNew();
			departureTriggerTemplate.IsWorkflowTrigger = true;
			departureTriggerTemplate.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			ProcessTask task = Dummy.WorkflowItems.AddNew();
			ProcessTask arrivalTrigger = Dummy.WorkflowItems.AddNew();
			arrivalTrigger.IsWorkflowTrigger = true;
			arrivalTrigger.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			ProcessTask departureTrigger = Dummy.WorkflowItems.AddNew();
			departureTrigger.IsWorkflowTrigger = true;
			departureTrigger.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			AssertEquals("An existing task can never be matched to a task template, ie. a new task is always created from a task template", false, task.IsMatchForItemTemplateMerge(taskTemplate));
			AssertEquals("An existing trigger matches when the event type is the same", true, arrivalTrigger.IsMatchForItemTemplateMerge(arrivalTriggerTemplate));
			AssertEquals("An existing trigger doesn't match when the event type is different", false, arrivalTrigger.IsMatchForItemTemplateMerge(departureTriggerTemplate));
		}

		public void TestIsMatchForItemTemplateMerge_CascadedEvents()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			ProcessTask templateTrigger = template.WorkflowItems.AddNew();
			templateTrigger.IsWorkflowTrigger = true;
			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.IncidentClosedCode;

			ProcessTask trigger = Dummy.WorkflowItems.AddNew();
			trigger.IsWorkflowTrigger = true;
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.IncidentClosedCode;

			AssertEquals("Matches initially", true, trigger.IsMatchForItemTemplateMerge(templateTrigger));

			templateTrigger.P9_RespondToCascadedEvents = true;
			AssertEquals("Not matches - different P9_RespondToCascadedEvents", false, trigger.IsMatchForItemTemplateMerge(templateTrigger));

			trigger.P9_RespondToCascadedEvents = true;
			AssertEquals("Matches - same P9_RespondToCascadedEvents", true, trigger.IsMatchForItemTemplateMerge(templateTrigger));

			templateTrigger.P9_CascadedEventsContext = "XX YY";
			AssertEquals("Not matches - different P9_CascadedEventsContext", false, trigger.IsMatchForItemTemplateMerge(templateTrigger));

			trigger.P9_CascadedEventsContext = "XX YY";
			AssertEquals("Matches - same P9_CascadedEventsContext", true, trigger.IsMatchForItemTemplateMerge(templateTrigger));

			trigger.P9_CascadedEventsContext = "AA BB";
			AssertEquals("Not matches - different P9_CascadedEventsContext", false, trigger.IsMatchForItemTemplateMerge(templateTrigger));

			templateTrigger.P9_RespondToCascadedEvents = false;
			trigger.P9_RespondToCascadedEvents = false;
			AssertEquals("Matches - P9_CascadedEventsContext does not matter because P9_RespondToCascadedEvents = false", true, trigger.IsMatchForItemTemplateMerge(templateTrigger));
		}

		public void TestIsMatchForItemTemplateMerge_ForTask()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTask taskTemplate = template.WorkflowItems.AddNew();
			taskTemplate.P9_Description = "A Task";

			ProcessTask task = Factory.New<ProcessTask>();
			Assert("Precondition", task.IsTask);

			task.P9_Description = "A Task";
			AssertEquals("A task is not a template match even with same description", false, task.IsMatchForItemTemplateMerge(taskTemplate));

			task.P9_ParentTemplateID = taskTemplate.PK;
			AssertEquals("Only exception match - task was created from same template item", true, task.IsMatchForItemTemplateMerge(taskTemplate));
		}

		public void TestIsMatchForItemTemplateMerge_ChangedUDF()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			var templateTrigger = template.WorkflowItems.AddNew();
			templateTrigger.P9_Description = "Foo";
			templateTrigger.IsWorkflowTrigger = true;
			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.IncidentClosedCode;
			templateTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger.TemplateConditions.TemplateCondition2Value = "1==1";
			Factory.Save();

			ProcessTask trigger = Dummy.WorkflowItems.AddNew();
			trigger.P9_Description = "Foo";
			trigger.IsWorkflowTrigger = true;
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.IncidentClosedCode;
			trigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger.TemplateConditions.TemplateCondition2Value = "2==2";

			Dummy.ApplyWorkflowTemplates();

			AssertEquals("We add a new item because the UDF conditions don't match", 2, Dummy.WorkflowItems.Count);
		}

		public void TestIsMatchForItemTemplateMerge_SameUDF()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			var templateTrigger = template.WorkflowItems.AddNew();
			templateTrigger.P9_Description = "Foo";
			templateTrigger.IsWorkflowTrigger = true;
			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.IncidentClosedCode;
			templateTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger.TemplateConditions.TemplateCondition2Value = "1==1";
			Factory.Save();

			ProcessTask trigger = Dummy.WorkflowItems.AddNew();
			trigger.P9_Description = "Foo";
			trigger.IsWorkflowTrigger = true;
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.IncidentClosedCode;
			trigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger.TemplateConditions.TemplateCondition2Value = "1==1";

			Dummy.ApplyWorkflowTemplates();

			AssertEquals("We don't add a new item because it merges into the existing item of the same description/udf conditions", 1, Dummy.WorkflowItems.Count);
		}

		public void TestIsMatchForItemTemplateMerge_ChangedTriggerConditions()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			var templateTrigger = template.WorkflowItems.AddNew();
			templateTrigger.IsWorkflowTrigger = true;
			templateTrigger.TriggerConditions.TriggerEventCode = AutoEvents.IncidentClosedCode;
			templateTrigger.TriggerConditions.TriggerCondition = "NAM";

			ProcessTask trigger = Dummy.WorkflowItems.AddNew();
			trigger.IsWorkflowTrigger = true;
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.IncidentClosedCode;
			templateTrigger.TriggerConditions.TriggerCondition = "SRV";

			Assert("Should not match for merge with different trigger condition", !trigger.IsMatchForItemTemplateMerge(templateTrigger));

			trigger.P9_ParentTemplateID = templateTrigger.PK;

			Assert("Should not match for merge with different trigger condition even from same template item", !trigger.IsMatchForItemTemplateMerge(templateTrigger));
		}

		[TestDate(2005, 1, 2)]
		public void TestCreateMilestoneException()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Description";
			milestone.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionMiscellaneous;

			var assignedGroup = Factory.New<GlbGroup>();
			assignedGroup.GG_Code = GlbGroup.AllStaffGroupCode;
			assignedGroup.GG_Desc = "ALL STAFF";

			milestone.P9_GG_AssignedGroup = assignedGroup.PK;

			int dbLogsCount = milestone.Logs.LogsNotInDB.Length;
			milestone.P9_SE_NKExceptionEvent = ZString.Empty;

			var exception = milestone.CreateMilestoneException();

			AssertEquals("P9_SE_NKExceptionEvent should not be created", milestone.Logs.LogsNotInDB.Length, dbLogsCount);
			AssertNull("Exception should not be created", exception);

			milestone.P9_SE_NKExceptionEvent = "TST";
			exception = milestone.CreateMilestoneException();

			AssertEquals("P9_SE_NKExceptionEvent should not be created", milestone.Logs.LogsNotInDB.Length, dbLogsCount);
			AssertNull("Exception should not be created", exception);

			milestone.P9_SE_NKExceptionEvent = "EXC";
			var updatedException = milestone.CreateMilestoneException();
			exception = milestone.CreateMilestoneException();

			AssertEquals("No event codes on milestone", 0, milestone.Logs.Find(new ZQuery(ZArchitecture.Schema.StmALogSchema.SL_SE_NKEvent, exception.P9_SE_NKExceptionEvent)).Length);
			AssertEquals("Event code should be EXR on milestone.Parent", 2, milestone.Parent.Logs.Find(new ZQuery(ZArchitecture.Schema.StmALogSchema.SL_SE_NKEvent, Events.ExceptionRaisedCode)).Length);

			AssertEquals("1 exception created per P9_SE_NKExceptionEvent", 1, Dummy.WorkflowItems.Exceptions.Count);
			AssertEquals(true, Dummy.WorkflowItems.Exceptions.Contains(exception));
			AssertEquals("When P9_SE_NKExceptionEvent is unchanged exceptions should updated", exception.PK, updatedException.PK);
			AssertEquals("P9_Type", Core.Constants.Workflow.ExceptionType, exception.P9_Type);
			AssertEquals("P9_Description", "Description", exception.P9_Description);
			AssertEquals("P9_ParentID", milestone.P9_ParentID, exception.P9_ParentID);
			AssertEquals("P9_ParentTableCode", milestone.P9_ParentTableCode, exception.P9_ParentTableCode);
			AssertEquals("P9_SE_NKExceptionEvent", milestone.P9_SE_NKExceptionEvent, exception.P9_SE_NKExceptionEvent);
			AssertEquals("P9_MilestoneExceptionAdded", ZDateTime.Now, milestone.P9_MilestoneExceptionAdded);
			AssertEquals("P9_GS_NKAssignedStaffMember", milestone.P9_GS_NKAssignedStaffMember, exception.P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GG_AssignedGroup", milestone.P9_GG_AssignedGroup, exception.P9_GG_AssignedGroup);
			AssertEquals("Creating an exception a second time should return the same exception", exception, milestone.CreateMilestoneException());
			AssertEquals("A second exception not created", 1, Dummy.WorkflowItems.Exceptions.Count);
			AssertEquals("Test MilestoneException property", exception, milestone.MilestoneException);
		}

		public void TestCreateExceptionManualy_ShouldCreateEXR_Event()
		{
			var exception1 = Dummy.WorkflowItems.Exceptions.AddNew();
			exception1.P9_Description = "exception 1!";

			var exceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType1.WET_Code = "TP1";
			exceptionType1.WET_Description = nameof(exceptionType1);
			exception1.ExceptionTypeCode = exceptionType1.WET_Code;

			Factory.Save();

			var log = exception1.Parent.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExceptionRaisedCode)).Single();

			AssertEquals("Type:[TP1]", log.SL_Reference);

			WorkflowDataRegistry.Instance.ExceptionEXRReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var exception2 = Dummy.WorkflowItems.Exceptions.AddNew();
			exception2.P9_Description = "exception 2!";

			var exceptionType2 = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType2.WET_Code = "TP2";
			exceptionType2.WET_Description = nameof(exceptionType2);
			exception2.ExceptionTypeCode = exceptionType2.WET_Code;

			Factory.Save();

			log = exception2.Parent.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExceptionRaisedCode))
				.Where(l => l.SL_Reference.Contains("exception 2")).Single();

			AssertEquals("|TYP=TP2|DES=exception 2!", log.SL_Reference);
		}

		public void TestCreateExceptionManuallyAndActionCreatesSingleEXAEvent()
		{
			var exception1 = Dummy.WorkflowItems.Exceptions.AddNew();
			exception1.P9_Description = "exception 1!";

			var exceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType1.WET_Code = "TP1";
			exceptionType1.WET_Description = nameof(exceptionType1);
			exception1.ExceptionTypeCode = exceptionType1.WET_Code;
			exception1.IsExceptionActioned = true;

			Factory.Save();

			AssertEquals(1, exception1.Parent.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExceptionActionedCode)).Length);
		}

		public void TestExceptionActioned_ShouldCreateEXA_Event()
		{
			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "exception!";

			var exceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType1.WET_Code = "TP";
			exceptionType1.WET_Description = nameof(exceptionType1);
			exception.ExceptionTypeCode = exceptionType1.WET_Code;

			Factory.Save();

			var log = exception.Parent.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExceptionActionedCode)).SingleOrDefault();
			AssertNull(log);

			exception.IsExceptionActioned = true;
			Factory.Save();

			log = exception.Parent.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExceptionActionedCode)).Single();
			AssertEquals("|TYP=TP|DES=exception!", log.SL_Reference);
		}

		public void TestExceptionChangeStatus_ShouldCreateSTC_Event()
		{
			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "exception!";

			var exceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType1.WET_Code = "TP";
			exceptionType1.WET_Description = nameof(exceptionType1);
			exception.ExceptionTypeCode = exceptionType1.WET_Code;

			Factory.Save();

			var log = exception.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode)).SingleOrDefault();

			AssertNull(log);

			exception.IsExceptionActioned = true;
			Factory.Save();

			log = exception.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode)).SingleOrDefault();
			AssertEquals("|FRM=OPN|TO=RSL|CHM=OTH|ASN=E", log.SL_Reference);

			exception.IsExceptionActioned = false;
			Factory.Save();

			log = exception.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusChangeCode))
				.Where(l => l.SL_Reference.Contains("|FRM=RSL|TO=OPN|")).Single();
			AssertEquals("|FRM=RSL|TO=OPN|CHM=OTH", log.SL_Reference);
		}

		[TestDate(2023, 07, 04, 10, 15, 20)]
		public void TestExceptionActioned_ShouldSetCompletedTimeUtc()
		{
			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "exception!";

			var exceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType1.WET_Code = "TP";
			exceptionType1.WET_Description = nameof(exceptionType1);
			exception.ExceptionTypeCode = exceptionType1.WET_Code;

			Factory.Save();

			AssertEquals(ZDateTime.Empty, exception.P9_CompletedTimeUtc);

			exception.IsExceptionActioned = true;
			Factory.Save();

			AssertEquals(ZDateTime.UtcNow, exception.P9_CompletedTimeUtc);

			exception.IsExceptionActioned = false;
			Factory.Save();

			AssertEquals(ZDateTime.Empty, exception.P9_CompletedTimeUtc);
		}

		public void TestExceptionActioned_AutoAssignStaff()
		{
			WorkflowDataRegistry.Instance.ExceptionAutoAssignStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "exception!";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "GP1";

			Factory.Save();

			AssertEquals(ZString.Empty, exception.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, exception.P9_GG_AssignedGroupCode);

			exception.IsExceptionActioned = true;

			AssertEquals(ZString.Empty, exception.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, exception.P9_GG_AssignedGroupCode);

			exception.IsExceptionActioned = false;
			WorkflowDataRegistry.Instance.ExceptionAutoAssignStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			exception.IsExceptionActioned = true;

			AssertEquals(GlbStaff.CurrentUser.GS_Code, exception.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, exception.P9_GG_AssignedGroupCode);

			exception.IsExceptionActioned = false;

			AssertEquals(ZString.Empty, exception.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, exception.P9_GG_AssignedGroupCode);

			exception.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			exception.P9_GG_AssignedGroupCode = group.GG_Code;
			exception.IsExceptionActioned = true;

			AssertEquals(staff.GS_Code, exception.P9_GS_NKAssignedStaffMember);
			AssertEquals(group.GG_Code, exception.P9_GG_AssignedGroupCode);

			exception.IsExceptionActioned = false;

			AssertEquals(staff.GS_Code, exception.P9_GS_NKAssignedStaffMember);
			AssertEquals(group.GG_Code, exception.P9_GG_AssignedGroupCode);

			exception.P9_GS_NKAssignedStaffMember = ZString.Empty;
			exception.P9_GG_AssignedGroupCode = group.GG_Code;
			exception.IsExceptionActioned = true;

			AssertEquals(ZString.Empty, exception.P9_GS_NKAssignedStaffMember);
			AssertEquals(group.GG_Code, exception.P9_GG_AssignedGroupCode);
		}

		public void TestExceptionAssigned_ShouldCreateASN_Event()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "GP1";

			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "exception!";

			var exceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType1.WET_Code = "TP";
			exceptionType1.WET_Description = nameof(exceptionType1);
			exception.ExceptionTypeCode = exceptionType1.WET_Code;

			Factory.Save();

			var log = exception.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AssignedCode)).SingleOrDefault();
			AssertNull(log);

			exception.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			Factory.Save();

			log = exception.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AssignedCode)).SingleOrDefault();
			AssertEquals("|STF=ST1", log.SL_Reference);

			exception.P9_GG_AssignedGroupCode = group.GG_Code;
			Factory.Save();

			log = exception.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AssignedCode))
				.Where(l => l.SL_Reference.Contains("|GRP=GP1")).Single();
			AssertEquals("|GRP=GP1", log.SL_Reference);

			exception.P9_GS_NKAssignedStaffMember = ZString.Empty;
			exception.P9_GG_AssignedGroupCode = ZString.Empty;
			Factory.Save();

			AssertEquals("No logs should be create for empty values", 2, exception.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AssignedCode)).Length);

			exception.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			exception.P9_GG_AssignedGroupCode = group.GG_Code;
			Factory.Save();

			log = exception.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AssignedCode))
				.Where(l => l.SL_Reference.Contains("|STF=ST1|GRP=GP1")).Single();
			AssertEquals("|STF=ST1|GRP=GP1", log.SL_Reference);
		}

		public void TestCreateMilestoneExceptionAndLogOnParent_DefaultExceptionEXRReference()
		{
			DummyEnterpriseBusinessObjectWithWorkflow dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObjectWithWorkflow>();
			ProcessTask milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Description";
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			milestone.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionMiscellaneous;
			milestone.CreateMilestoneException();

			StmALog exceptionRaisedLogOnParent = dummy.Logs.MostRecentLogByEventTime(Events.ExceptionRaised);

			AssertNotNull(exceptionRaisedLogOnParent);
			AssertEquals(string.Format("Type:[{0}]; Event:[{1}]", ProcessWorkflowExceptionType.ExceptionMiscellaneous, Events.ArrivalCode), exceptionRaisedLogOnParent.SL_Reference);
		}

		public void TestCreateMilestoneExceptionAndLogOnParent_ExceptionEXRReferenceIsTrue()
		{
			WorkflowDataRegistry.Instance.ExceptionEXRReference.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			DummyEnterpriseBusinessObjectWithWorkflow dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObjectWithWorkflow>();
			ProcessTask milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Description";
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			milestone.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionMiscellaneous;
			milestone.CreateMilestoneException();

			StmALog exceptionRaisedLogOnParent = dummy.Logs.MostRecentLogByEventTime(Events.ExceptionRaised);

			AssertNotNull(exceptionRaisedLogOnParent);
			AssertEquals(string.Format("|TYP={0}|EVT={1}|DES=Description", ProcessWorkflowExceptionType.ExceptionMiscellaneous, Events.ArrivalCode), exceptionRaisedLogOnParent.SL_Reference);
		}

		[ExpectNoExceptions]
		public void TestDefaultDatesFromMilestoneEventDoesNotThrowExceptionWhenEventInTriggerConditionValueIsNotSpecified()
		{
			DummyEnterpriseBusinessObjectWithWorkflow dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObjectWithWorkflow>();
			ProcessTask trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Description";
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger.P9_SE_NKExceptionEvent = Events.ExceptionRaisedCode;
			trigger.TriggerConditions.TriggerCondition = ExceptionActionConditionList.Codes.EventType;
			trigger.TriggerConditions.TriggerConditionValue = ""; // P9_TriggerCondition set to Event and no event in P9_TriggerConditionValue leads to exception

			dummy.Logs.AddNew(Events.ExceptionRaised);
			trigger.DefaultDatesFromMilestoneEvent_ForTest(true);
			Assert(trigger.P9_ActualDate.IsEmpty); // If condition is event, but no value selected - we treat it as NOT matching 
		}

		public void TestIsMilestoneOverdueAndHasOpenException()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.Delivered.Code;
			milestone.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);
			AssertEquals("Milestone has no open exceptions yet", false, milestone.IsMilestoneOverdueAndHasOpenException);

			ProcessTask exception = milestone.CreateMilestoneException();
			AssertEquals("Milestone overdue and has an open exception", true, milestone.IsMilestoneOverdueAndHasOpenException);

			exception.IsExceptionActioned = true;
			AssertEquals("Exception resolved", false, milestone.IsMilestoneOverdueAndHasOpenException);

			exception.IsExceptionActioned = false;
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			AssertEquals("Milestone completed", false, milestone.IsMilestoneOverdueAndHasOpenException);
		}

		public void TestIsExceptionActioned()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTask exception = milestone.CreateMilestoneException();

			AssertEquals("Exception is not actioned by default", false, exception.IsExceptionActioned);
			exception.IsExceptionActioned = true;
			AssertEquals("Exception is actioned", true, exception.IsExceptionActioned);
			exception.IsExceptionActioned = false;
			AssertEquals("Exception is not actioned", false, exception.IsExceptionActioned);
		}

		public void TestExceptionActioned_ShouldSetDefaultCauseWhenTypeRequiresCause()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			var exception = milestone.CreateMilestoneException();

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);
			processWorkflowExceptionType.WET_IsCauseRequired = true;
			exception.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;

			var defaultCause = processWorkflowExceptionType.Causes.AddNew();
			defaultCause.WEC_Description = "Default Cause";
			defaultCause.WEC_IsDefault = true;
			var otherCause = processWorkflowExceptionType.Causes.AddNew();
			otherCause.WEC_Description = "Other Cause";

			AssertEquals("Exception is not actioned by default", false, exception.IsExceptionActioned);

			exception.IsExceptionActionedForBinding = true;

			Assert("When it is the user manualy setting should not set default cause", exception.ExceptionCausePK.IsEmpty);

			exception.IsExceptionActioned = false;
			exception.IsExceptionActioned = true;

			AssertEquals(defaultCause.PK, exception.ExceptionCausePK);
		}

		public void TestExceptionActioned_ShouldSetDefaultResolutionWhenTypeRequiresResolution()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			var exception = milestone.CreateMilestoneException();

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TYP";
			processWorkflowExceptionType.WET_Description = nameof(processWorkflowExceptionType);
			processWorkflowExceptionType.WET_IsResolutionRequired = true;
			exception.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;

			var defaultResolution = processWorkflowExceptionType.Resolutions.AddNew();
			defaultResolution.WER_Description = "Default Cause";
			defaultResolution.WER_IsDefault = true;
			var otherResolution = processWorkflowExceptionType.Resolutions.AddNew();
			otherResolution.WER_Description = "Other Cause";

			AssertEquals("Exception is not actioned by default", false, exception.IsExceptionActioned);

			exception.IsExceptionActionedForBinding = true;

			Assert("When it is the user manualy setting should not set default resolution", exception.ExceptionResolutionPK.IsEmpty);

			exception.IsExceptionActioned = false;
			exception.IsExceptionActioned = true;

			AssertEquals(defaultResolution.PK, exception.ExceptionResolutionPK);
		}

		[TestDate(2005, 1, 2)]
		public void TestP9_ActualDate_PopulatedOnSavingForException()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTask exception = milestone.CreateMilestoneException();
			AssertEquals("Exception P9_ActualDate empty initially for the test", ZDateTime.Empty, exception.P9_ActualDate);

			Factory.Save();
			AssertEquals("P9_ActualDate populated on save", ZDateTime.Now, exception.P9_ActualDate);
		}

		[TestDate(2023, 8, 1)]
		public void TestExceptionAssignedDate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "GP1";

			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "exception!";

			Factory.Save();

			Assert(exception.ExceptionAssignedDate.IsEmpty);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			exception.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			Factory.Save();

			AssertEquals(new ZDateTime(2023, 8, 2), exception.ExceptionAssignedDate);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			exception.P9_GG_AssignedGroup = group.PK;

			Factory.Save();

			AssertEquals(new ZDateTime(2023, 8, 3), exception.ExceptionAssignedDate);
		}

		public void TestDelayedTemplateMilestone_WithTriggerCondition()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INQ";
			template.P0_SubType1 = "NAM";

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			templateMilestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateMilestone.TemplateConditions.TemplateCondition2Value = "\"<O1_Phone>\" == \"123\"";
			templateMilestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			templateMilestone.TriggerConditions.TriggerConditionValue = "*test*";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "NAM";
			Factory.Save();
			AssertEquals("GIVEN delayed-template-trigger condition not met (Phone != '123'), WHEN saved should not apply delayed-template-trigger", 0, job.WorkflowItems.Milestones.Count);

			job.GetLogs().AddNew(eventType: Events.Arrival, reference: "this is a test", dateTime: ZDateTimeOffset.UtcNow);
			Factory.Save();
			AssertEquals("Event occurred", 1, job.GetLogs().Find(l => l.SL_SE_NKEvent == Events.Arrival.Code).Count());

			job.O1_Phone = "123";
			Factory.Save();
			AssertEquals("GIVEN delayed-template-trigger condition met (Phone == '123'), WHEN saved should apply delayed-template-trigger", 1, job.WorkflowItems.Milestones.Count);

			AssertEquals("GIVEN delayed-template-milestone applied AND event occurred, WHEN delayed-template-milestone TriggerCondition met SHOULD not raise occurred event again.",
					1,
					job.GetLogs().Find(l => l.SL_SE_NKEvent == Events.Arrival.Code).Count());
		}

		[TestDate(2017, 12, 14)]
		public void TestFutureMilestoneActualStart_RegistryOn_UserInteractive()
		{
			AssertFutureEventException(registrySetting: true, isUserInteractive: true);
		}

		[TestDate(2017, 12, 14)]
		public void TestFutureMilestoneActualStart_RegistryOff_UserInteractive()
		{
			AssertFutureEventException(registrySetting: false, isUserInteractive: true);
		}

		[TestDate(2017, 12, 14)]
		public void TestFutureMilestoneActualStart_RegistryOn_NonUserInteractive()
		{
			AssertFutureEventException(registrySetting: true, isUserInteractive: false);
		}

		[TestDate(2017, 12, 14)]
		public void TestFutureMilestoneActualStart_RegistryOff_NonUserInteractive()
		{
			AssertFutureEventException(registrySetting: false, isUserInteractive: false);
		}

		[TestDate(2017, 12, 14)]
		public void TestFutureMilestoneActualStart_RegistryOn_NonUserInteractive_ValidationSuspended()
		{
			Factory.SuspendValidation();
			AssertFutureEventException(registrySetting: true, isUserInteractive: false);
		}

		[TestDate(2017, 12, 14)]
		public void TestTriggersCanHaveFutureDates_UserInteractive()
		{
			AssertFutureEventException(registrySetting: true, isUserInteractive: true, isMilestone: false);
		}

		[TestDate(2017, 12, 14)]
		public void TestTriggersCanHaveFutureDates_NonUserInteractive()
		{
			AssertFutureEventException(registrySetting: true, isUserInteractive: false, isMilestone: false);
		}

		void AssertFutureEventException(bool registrySetting, bool isUserInteractive, bool isMilestone = true)
		{
			var expectingMilestoneException = isMilestone && registrySetting;
			var expectingValidationError = isMilestone && registrySetting && !Factory.IsValidationSuspended;

			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);
			using (Globals.SetIsUserInteractiveForTest(isUserInteractive))
			{
				var processTask = SetupDummyWithProcessTaskFiredWithFutureDate(isMilestone);
				Factory.Save();

				var exceptionQuery = new ZQuery(ProcessTasksSchema.P9_Description, processTask.P9_Description)
					.AddToFilter(ProcessTasksSchema.P9_SE_NKExceptionEvent, ProcessWorkflowExceptionType.ExceptionFutureEvent);
				var exceptions = Factory.Load<ProcessTask>(exceptionQuery).Length;
				var parentExceptionLogs = processTask.Parent.Logs.Find(l => l.SL_SE_NKEvent == Events.ExceptionRaisedCode).Count();
				var milestoneExceptionLogs = processTask.Logs.Find(l => l.SL_SE_NKEvent == ProcessWorkflowExceptionType.ExceptionFutureEvent).Count();

				if (expectingMilestoneException)
				{
					AssertEquals($"Expecting a {ProcessWorkflowExceptionType.ExceptionFutureEvent} exception to be raised", 1, exceptions);
					AssertEquals($"Expecting a {Events.ExceptionRaised} event on the parent", 1, parentExceptionLogs);
					AssertEquals($"Expecting no {ProcessWorkflowExceptionType.ExceptionFutureEvent} event on the milestone", 0, milestoneExceptionLogs);
				}
				else
				{
					AssertEquals($"Expecting no {ProcessWorkflowExceptionType.ExceptionFutureEvent} exceptions to be raised", 0, exceptions);
					AssertEquals($"Expecting no {Events.ExceptionRaised} events on the parent", 0, parentExceptionLogs);
					AssertEquals($"Expecting no {ProcessWorkflowExceptionType.ExceptionFutureEvent} events on the milestone", 0, milestoneExceptionLogs);
				}

				AssertEquals("Future Event Exception should not prevent normal milestone exceptions from being raised", ZDateTime.Empty, processTask.P9_ExceptionAddedUtc);

				if (expectingValidationError)
				{
					AssertHasError(processTask.P9_ActualDateInfo, ProcessTaskValidationBase.ActualDateIsInTheFutureErrorMessage);
				}
				else
				{
					AssertNoErrors(processTask.P9_ActualDateInfo);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestCreateRecreateOrUpdateEventLogReturnsDeletedLog()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone.P9_Description = "Some Trigger";

			ProcessTaskHandler.SetOnFireHookForTest((log, methodName) =>
			{
				if (methodName == "FireTriggers")
				{
					log.Delete();
				}
			});

			milestone.P9_ActualDateInternal = new ZDateTimeOffset(ZDateTime.Now);
			AssertHasRowError(milestone, $"Cannot complete this milestone as there is a completion trigger action causing the {Events.CustomisableEvent00Code} event to be canceled.");
		}

		public void TestTriggerSetActualDate()
		{
			// Note that this is not supported. We are sudo supporting this for legacy purposes until dependent code is fixed.
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_Description = "Some Trigger";
			trigger.P9_ActualDateInternal = new ZDateTimeOffset(ZDateTime.Now);
			AssertEquals((short)99, trigger.P9_TriggerFiredCountdown);
			AssertEquals(ErrorReporter.LastKeyReported, "TriggerActualDateSet");
			var logs = job.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.CustomisableEvent00Code);
			AssertEquals(1, logs.Count());
			Factory.Save();

			trigger.P9_ActualDateInternal = new ZDateTimeOffset(ZDateTime.Now);
			AssertEquals((short)98, trigger.P9_TriggerFiredCountdown);
			logs = job.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.CustomisableEvent00Code);
			AssertEquals(2, logs.Count());

			ErrorReporter.Clear();
		}

		public void TestWhenProcessTaskHasProcessJobTriggerLink_ThenDisplayProcessJobTriggerLinkTriggerFiredCountdown()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			universalTemplate.P0_Name = "UniversalTemplate";
			var templateTrigger = (IUniversalTemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger.Description = "Tagged";

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();

			var processTask = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger.Identifier);

			AssertEquals("Precondition", (short)100, processTask.P9_TriggerFiredCountdown);

			job.GetLogs().AddNew(Events.TagWasAddedOrRemoved);

			var processJobTriggerLink = templateTrigger.GetOrCreateJobVersionOfTrigger(processTask.GetJob(), false);

			AssertNotNull("Expected job to have a trigger link based on the template", processJobTriggerLink);
			processJobTriggerLink.TriggerFiredCountdown = 50;

			Factory.Save();

			AssertEquals("Precondition", (short)50, processTask.TriggerConditions.TriggerFiredCountdown);
		}

		public void TestGivenProcessTaskHasTemplateTrigger_WhenProcessTaskHasNoProcessJobTriggerLink_ThenDisplayTemplateTriggerTriggerFiredCountdown()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			universalTemplate.P0_Name = "UniversalTemplate";
			var universalTemplateTrigger = (IUniversalTemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			universalTemplateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			universalTemplateTrigger.Description = "Tagged";
			universalTemplateTrigger.TriggerFiredCountdown = 140;

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var processTask = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == universalTemplateTrigger.Identifier);
			job.ApplyWorkflowTemplates();

			universalTemplateTrigger = Factory.LoadTop1<IUniversalTemplateTrigger>(new ZQuery(ProcessTemplateTriggerSchema.PK, processTask.P9_ParentTemplateID) { FetchOnlyFromLocalCache = true });
			AssertNotNull("Expected processTask to have a UniversalTemplateTrigger, the same one created in this test.", universalTemplateTrigger);
			AssertEquals("Expected processTask to have the same value as the template when the processTask has a template", (short)140, processTask.TriggerConditions.TriggerFiredCountdown);
		}

		public void TestProcessJobTriggerLinkTriggerFireCountdownIsEditable_WhenProcessTaskHasProcessJobTriggerLink()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			universalTemplate.P0_Name = "UniversalTemplate";
			var templateTrigger = (IUniversalTemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger.Description = "Tagged";

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();

			var processTask = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger.Identifier);
			Factory.Save();

			AssertEquals("Precondition", (short)100, processTask.TriggerConditions.TriggerFiredCountdown);

			job.GetLogs().AddNew(Events.TagWasAddedOrRemoved);

			var processJobTriggerLink = templateTrigger.GetOrCreateJobVersionOfTrigger(processTask.GetJob(), false);
			AssertNotNull("Expected job to have a trigger link based on the template", processJobTriggerLink);

			processTask.TriggerConditions.TriggerFiredCountdown = (short)30;

			CombineAssertions(() =>
			{
				var triggerConditionsViewModel = new TriggerConditionsViewModel(processTask);
				Assert("Trigger remaining countdown field should be editable", !triggerConditionsViewModel.TriggerFiredCountdown_ReadOnly);
				Assert("Process Task BizO should be editable", !processTask.ReadOnly);
				AssertEquals("remaining countdown field edited ", (short)30, processTask.TriggerConditions.TriggerFiredCountdown);
			});
		}

		public void TestProcessJobTriggerLinkTrigger_PersistsInTwoFactories()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			universalTemplate.P0_Name = "UniversalTemplate";

			var templateTrigger = (IUniversalTemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger.Description = "Tagged";

			Factory.Save();

			var jobInFactory1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			jobInFactory1.ApplyWorkflowTemplates();

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var jobInFactory2 = factory2.Load<DummyWithWorkflow>(jobInFactory1.PK);

			jobInFactory1.GetLogs().AddNew(Events.TagWasAddedOrRemoved);

			factory2.Save();
			Factory.Save();

			var processTask = jobInFactory2.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger.Identifier);
			var processJobTriggerLink = templateTrigger.GetOrCreateJobVersionOfTrigger(processTask.GetJob(), false);
			Assert(processTask.HasProcessJobTriggerLink);
		}

		public void TestTriggerSetEmptyActualDate()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_Description = "Some Trigger";
			trigger.P9_ActualDateInternal = new ZDateTimeOffset(ZDateTime.Empty);
			AssertEquals((short)100, trigger.P9_TriggerFiredCountdown);
			var log = job.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.CustomisableEvent00Code).FirstOrDefault();
			AssertEquals(null, log);
		}

		ProcessTask SetupDummyWithProcessTaskFiredWithFutureDate(bool isMilestone)
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = isMilestone ? job.WorkflowItems.Milestones.AddNew() : job.WorkflowItems.Triggers.AddNew();
			task.TriggerConditions.TriggerEventCode = Events.EstimatedDateChangedCode;
			task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			task.TriggerConditions.TriggerConditionValue = "*ARV*OLD*";
			job.GetLogs().AddNew(eventType: Events.EstimatedDateChanged, reference: "|TYP=ARV|OLD=9-DEC-17|NEW=15-DEC-17|LOC=AUSYD", dateTime: ZDateTimeOffset.Now.AddDays(1));
			return task;
		}

		public void TestSkipFiringTriggersForDeletedLog()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Yo";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			Factory.Save();

			var mockTriggerProvider = new Mock<ITriggerProvider>();
			var results = Array.Empty<(IBaseTrigger, IBusiness)>();
			mockTriggerProvider.Setup(o => o.LoadAllMilestonesAndTriggersForEvent(It.IsAny<IBusiness>(), It.IsAny<IStmALog>(), It.IsAny<ZDateTimeOffset>()))
				.Callback<IBusiness, IStmALog, ZDateTimeOffset>((bizo, log, time) =>
				{
					results = TriggerProvider.LoadAllMilestonesAndTriggersForEvent((IStmALogParent)bizo, log, time);
					using (((IBusinessObjectInternals)log).SuppressReportRowDeletedError())
					{
						((BusinessObject)log).Delete();
					}
				})
				.Returns(results);

			using (ObjectFactory.Substitute(mockTriggerProvider.Object))
			{
				var stmALog = job.Logs.AddNew(Events.CustomisableEvent00);
			}

			AssertEquals("Trigger not fired if firing log is already deleted", null, trigger.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
			AssertEquals("No error report", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestPropertyCaptionsSwitchForExceptions()
		{
			var processTask = Factory.New<ProcessTask>();
			AssertEquals("Staff Caption", "Task Assigned To", processTask.P9_GS_NKAssignedStaffMemberInfo.HumanReadableName);
			AssertEquals("Status Caption", "Task Status", processTask.P9_StatusInfo.HumanReadableName);

			processTask.IsException = true;
			AssertEquals("Exception Staff Caption", "Exception Assigned To", processTask.P9_GS_NKAssignedStaffMemberInfo.HumanReadableName);
			AssertEquals("Exception Status Caption", "Exception Status", processTask.P9_StatusInfo.HumanReadableName);
		}

		#region IsTask / IsMilestone / IsException / IWorkflowTrigger

		public void TestIsTask()
		{
			AssertEquals("ProcessTask is a task by default", true, ProcessTask.IsTask);

			ProcessTask.IsMilestone = true;
			AssertEquals("ProcessTask not a task if it is a milestone", false, ProcessTask.IsTask);
			ProcessTask.IsMilestone = false;

			ProcessTask.IsException = true;
			AssertEquals("ProcessTask not a task if it is an exception", false, ProcessTask.IsTask);
			ProcessTask.IsException = false;

			ProcessTask.IsWorkflowTrigger = true;
			AssertEquals("ProcessTask not a task if it is an exception", false, ProcessTask.IsTask);
			ProcessTask.IsWorkflowTrigger = false;

			AssertEquals("ProcessTask is a task if not a milestone or exception", true, ProcessTask.IsTask);
		}

		public void TestIsMilestone()
		{
			AssertEquals("ProcessTask not a milestone by default", false, ProcessTask.IsMilestone);

			ProcessTask.P9_EstDuration = ZDateTime.Now;
			ProcessTask.IsMilestone = true;
			AssertEquals("P9_EstDuration empty when IsMilestone=true", true, ProcessTask.P9_EstDuration.IsEmpty);
			AssertEquals("ProcessTask is a milestone if IsMilestone=true", true, ProcessTask.IsMilestone);

			ProcessTask.P9_Type = Core.Constants.Workflow.MilestoneType;
			AssertEquals(true, ProcessTask.IsMilestone);

			ProcessTask.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			ProcessTask.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily;
			ProcessTask.IsMilestone = false;
			AssertEquals("P9_SE_NKMilestoneEvent cleared when IsMilestone=false", true, ProcessTask.P9_SE_NKMilestoneEvent.IsEmpty);
			AssertEquals("P9_SE_NKExceptionEvent cleared when IsMilestone=false", true, ProcessTask.P9_SE_NKMilestoneEvent.IsEmpty);
		}

		public void TestIsException()
		{
			AssertEquals("ProcessTask not a milestone by default", false, ProcessTask.IsException);

			ProcessTask.P9_EstDuration = ZDateTime.Now;
			ProcessTask.IsException = true;
			AssertEquals("P9_EstDuration empty when IsException=true", true, ProcessTask.P9_EstDuration.IsEmpty);
			AssertEquals("ProcessTask is a milestone if IsMilestone = true", true, ProcessTask.IsException);

			ProcessTask.P9_Type = Core.Constants.Workflow.ExceptionType;
			AssertEquals(true, ProcessTask.IsException);

			ProcessTask.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily;
			ProcessTask.IsException = false;
			AssertEquals("P9_SE_NKExceptionEvent cleared when IsException=false", true, ProcessTask.P9_SE_NKExceptionEvent.IsEmpty);
		}

		public void TestIsWorkflowTrigger()
		{
			AssertEquals("ProcessTask not a trigger by default", false, ProcessTask.IsWorkflowTrigger);
			ProcessTask.IsWorkflowTrigger = true;
			AssertEquals("ProcessTask is a milestone if IsEventTrigger = true", true, ProcessTask.IsWorkflowTrigger);
		}

		public void TestIsMilestoneOrWorkflowTrigger()
		{
			ProcessTask.IsMilestone = true;
			AssertEquals(true, ProcessTask.IsMilestoneOrWorkflowTrigger);

			ProcessTask.IsWorkflowTrigger = true;
			AssertEquals(true, ProcessTask.IsMilestoneOrWorkflowTrigger);

			ProcessTask.IsException = true;
			AssertEquals(false, ProcessTask.IsMilestoneOrWorkflowTrigger);
		}

		#endregion

		#region P9_SE_NKTaskCompletionEvent

		public void TestP9_SE_NKTaskCompletionEvent()
		{
			// Ensures this task isn't treated like a milestone.
			Dummy.GetLogs().AddNew(Events.Arrival);

			var task = Dummy.WorkflowItems.Tasks.AddNew();
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			var standaloneTask = Factory.New<ProcessTask>();

			AssertEquals(false, task.P9_SE_NKTaskCompletionEventInfo.ReadOnly);

			AssertEquals(true, trigger.P9_SE_NKTaskCompletionEventInfo.ReadOnly);
			AssertEquals(true, milestone.P9_SE_NKTaskCompletionEventInfo.ReadOnly);
			AssertEquals(true, exception.P9_SE_NKTaskCompletionEventInfo.ReadOnly);
			AssertEquals(true, standaloneTask.P9_SE_NKTaskCompletionEventInfo.ReadOnly);

			task.P9_SE_NKTaskCompletionEvent = Events.ArrivalCode;

			AssertNoErrors(task.P9_SE_NKTaskCompletionEventInfo);
			AssertEquals(ZDateTime.Empty, task.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, task.P9_ActualDate);

			task.P9_SE_NKTaskCompletionEvent = "ZZZ";
			BusinessObjectValidationTestCase.AssertListValidationInvalidCodeError(task.P9_SE_NKTaskCompletionEventInfo, true);
		}

		public void TestP9_SE_NKTaskCompletionEvent_ProductivityWiseModeEnabled()
		{
			Dummy.GetLogs().AddNew(Events.Arrival);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			var task = Dummy.WorkflowItems.Tasks.AddNew();

			task.P9_SE_NKTaskCompletionEvent = Events.Arrival.Code;
			AssertNoErrors("Valid PW event, Valid CW1 event", task.P9_SE_NKTaskCompletionEventInfo);

			task.P9_SE_NKTaskCompletionEvent = Events.CargoAcceptedAtOriginDepot.Code;
			AssertHasError("Invalid PW event, Valid CW1 event", task.P9_SE_NKTaskCompletionEventInfo, "Enter a valid Task Completion Event.");

			task.P9_SE_NKTaskCompletionEvent = "ZZZ";
			AssertHasError("Invalid PW event, Invalid CW1 event", task.P9_SE_NKTaskCompletionEventInfo, "Enter a valid Task Completion Event.");
		}

		public void TestMilestones_ProductivityWiseModeEnabled()
		{
			Dummy.GetLogs().AddNew(Events.Arrival);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();

			var lookups = milestone.TriggerConditions.Lookups.MilestoneEventTypes.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("Customised codes should only be added once", 1, lookups.Where(w => w.Code == Events.CustomisableEvent00.Code).Count());
				AssertEquals("Regular codes should only be added once", 1, lookups.Where(w => w.Code == Events.AddedARecordToTheSystem.Code).Count());
			});

			milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertNoErrors("Valid PW event, Valid CW1 event", milestone.TriggerConditions.TriggerEventCodeInfo);

			milestone.TriggerConditions.TriggerEventCode = Events.CargoAcceptedAtOriginDepot.Code;
			AssertHasError("Invalid PW event, Valid CW1 event", milestone.TriggerConditions.TriggerEventCodeInfo, "Enter a valid Event Code.");

			milestone.TriggerConditions.TriggerEventCode = "ZZZ";
			AssertHasError("Invalid PW event, Invalid CW1 event", milestone.TriggerConditions.TriggerEventCodeInfo, "Enter a valid Event Code.");
		}

		public void TestTriggers_ProductivityWiseModeEnabled()
		{
			Dummy.GetLogs().AddNew(Events.Arrival);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();

			var lookups = trigger.TriggerConditions.Lookups.MilestoneEventTypes.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals("Customised codes should only be added once", 1, lookups.Where(w => w.Code == Events.CustomisableEvent00.Code).Count());
				AssertEquals("Regular codes should only be added once", 1, lookups.Where(w => w.Code == Events.AddedARecordToTheSystem.Code).Count());
			});

			trigger.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertNoErrors("Valid PW event, Valid CW1 event", trigger.TriggerConditions.TriggerEventCodeInfo);

			trigger.TriggerConditions.TriggerEventCode = Events.CargoAcceptedAtOriginDepot.Code;
			AssertHasError("Invalid PW event, Valid CW1 event", trigger.TriggerConditions.TriggerEventCodeInfo, "Enter a valid Event Code.");

			trigger.TriggerConditions.TriggerEventCode = "ZZZ";
			AssertHasError("Invalid PW event, Invalid CW1 event", trigger.TriggerConditions.TriggerEventCodeInfo, "Enter a valid Event Code.");
		}

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestCloseAndSaveTask_ShouldRaiseCompletionEventOnParent()
		{
			var task = Dummy.WorkflowItems.Tasks.AddNew();

			task.P9_SE_NKTaskCompletionEvent = Events.WorkflowTransferredBetweenSystemComponentsCode;
			task.P9_Type = "ZAP";
			task.P9_Description = "Ze enrraged pentomime prrincess Marrgaret";

			MasterFilesTestHelper.AssertNoEventRaised(Dummy, Events.WorkflowTransferredBetweenSystemComponentsCode);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var log = MasterFilesTestHelper.AssertEventRaised("Task completion event should be raised when the task is saved (to avoid duplicate events). We defer firing workflow on this event for performance reasons.",
				Dummy, Events.WorkflowTransferredBetweenSystemComponentsCode, "|DSC=Ze enrraged pentomime prrincess Marrgaret|TYP=ZAP", deferFiringWorkflow: false);

			AssertEquals(false, log.SL_IsCancelled);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			AssertEquals("The previous event is now cancelled since the task has been un-closed", true, log.IsDeleted);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var newLog = MasterFilesTestHelper.AssertEventRaised("Task completion event should be raised when the task is saved (to avoid duplicate events). We defer firing workflow on this event for performance reasons.",
				Dummy, Events.WorkflowTransferredBetweenSystemComponentsCode, "|DSC=Ze enrraged pentomime prrincess Marrgaret|TYP=ZAP", deferFiringWorkflow: false);

			AssertNotEquals("Subsequent closes should raise a new event", log, newLog);
			AssertEquals(false, newLog.SL_IsCancelled);
			AssertEquals(true, log.IsDeleted);
		}

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestCloseAndSaveCompletionStatement_ShouldRaiseCompletionEventOnParent()
		{
			MasterFilesTestHelper.AddCompletionStatementTaskType("COM", "DUM");

			var completionStatement = Dummy.WorkflowItems.Tasks.AddNew();

			completionStatement.P9_Type = "COM";
			completionStatement.P9_SE_NKTaskCompletionEvent = Events.WorkflowTransferredBetweenSystemComponentsCode;
			completionStatement.P9_Type = "ZAP";
			completionStatement.P9_Description = "Ze enrraged pentomime prrincess Marrgaret";

			MasterFilesTestHelper.AssertNoEventRaised(Dummy, Events.WorkflowTransferredBetweenSystemComponentsCode);

			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var log = MasterFilesTestHelper.AssertEventRaised("Task completion event should be raised when the task is saved (to avoid duplicate events). We defer firing workflow on this event for performance reasons.",
				Dummy, Events.WorkflowTransferredBetweenSystemComponentsCode, "|DSC=Ze enrraged pentomime prrincess Marrgaret|TYP=ZAP", deferFiringWorkflow: false);

			AssertEquals(false, log.SL_IsCancelled);

			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			AssertEquals("The previous event is now cancelled since the task has been un-closed", true, log.IsDeleted);

			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var newLog = MasterFilesTestHelper.AssertEventRaised("Task completion event should be raised when the task is saved (to avoid duplicate events). We defer firing workflow on this event for performance reasons.",
				Dummy, Events.WorkflowTransferredBetweenSystemComponentsCode, "|DSC=Ze enrraged pentomime prrincess Marrgaret|TYP=ZAP", deferFiringWorkflow: false);

			AssertNotEquals("Subsequent closes should raise a new event", log, newLog);
			AssertEquals(false, newLog.SL_IsCancelled);
			AssertEquals(true, log.IsDeleted);
		}

		#endregion

		#region P9_SE_NKMilestoneEvent

		public void TestP9_SE_NKMilestoneEvent_ReadOnlyIfNotAMilestoneOrExceptionOrTrigger()
		{
			AssertEquals("Milestone event type not editable by default", true, ProcessTask.TriggerConditions.TriggerEventCodeInfo.ReadOnly);

			ProcessTask.IsMilestone = true;
			AssertEquals("Milestone event type editable when task is a milestone", false, ProcessTask.TriggerConditions.TriggerEventCodeInfo.ReadOnly);

			ProcessTask.IsMilestone = false;
			ProcessTask.IsException = true;
			AssertEquals("Milestone event type editable when task is an exception", false, ProcessTask.TriggerConditions.TriggerEventCodeInfo.ReadOnly);

			ProcessTask.IsException = false;
			ProcessTask.IsWorkflowTrigger = true;
			AssertEquals("Milestone event type editable when task is a trigger", false, ProcessTask.TriggerConditions.TriggerEventCodeInfo.ReadOnly);
		}

		[TestDate(2019, 1, 1)]
		public void TestP9_SE_NKMilestoneEvent_UpdatesDatesFromStmALogWhenSet()
		{
			ProcessTask.IsMilestone = true;

			ProcessTask.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			var estimateEventValue = new EventValue(Events.Departure, eventTime: new ZDateTimeOffset(2020, 1, 1), isEstimate: true);
			Dummy.Logs.AddNew(estimateEventValue);
			ProcessTask.SetMilestoneActualDateForTest(new ZDateTime(2020, 2, 2));

			ProcessTask.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			estimateEventValue = new EventValue(Events.Arrival, eventTime: new ZDateTimeOffset(2020, 3, 3), isEstimate: true);
			Dummy.Logs.AddNew(estimateEventValue);
			ProcessTask.SetMilestoneActualDateForTest(new ZDateTime(2020, 4, 4));

			ProcessTask.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			AssertEquals("P9_ScheduledDate updated when event type has changed", new ZDateTime(2020, 1, 1), ProcessTask.P9_ScheduledDate);
			AssertEquals("P9_ActualDate updated when event type has changed", new ZDateTime(2020, 2, 2), ProcessTask.P9_ActualDate);

			ProcessTask.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertEquals("P9_ScheduledDate updated when event type has changed", new ZDateTime(2020, 3, 3), ProcessTask.P9_ScheduledDate);
			AssertEquals("P9_ActualDate updated when event type has changed", new ZDateTime(2020, 4, 4), ProcessTask.P9_ActualDate);

			ClearAllDepartureLogs(Dummy.Logs);
			ProcessTask.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			AssertEquals("P9_ScheduledDate not updated when there is no 'estimate' log", new ZDateTime(2020, 3, 3), ProcessTask.P9_ScheduledDate);
			AssertEquals("P9_ActualDate cleared when there is no 'actual' log", ZDateTime.Empty, ProcessTask.P9_ActualDate);
		}

		public void TestP9_EstDuration_SetToEmptyWhenIsMilestoneTrue()
		{
			TestP9_EstDuration_SetToEmptyWhenIsMilestoneOrIsExceptionTrue(ProcessTask.IsMilestoneInfo);
		}

		public void TestP9_EstDuration_SetToEmptyWhenIsExceptionTrue()
		{
			TestP9_EstDuration_SetToEmptyWhenIsMilestoneOrIsExceptionTrue(ProcessTask.IsExceptionInfo);
		}

		[TestDate(2015, 06, 07, 12, 10, 11)]
		public void TestP9_TriggerConditionValue_SetTooLongSL_Reference()
		{
			var businessObject = Factory.New<DummyWithWorkflow>();

			var milestone = businessObject.WorkflowItems.AddNew();
			milestone.IsMilestone = true;
			milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions;
			milestone.TriggerConditions.TriggerConditionValue = ZString.Replicate('a', StmALogSchema.SL_Reference.MaxLength);

			milestone.P9_ScheduledDateForBinding = new ZDateTimeOffset(new ZDateTime(2015, 06, 07));

			var dateStamp = " To: 07-Jun-15";
			var referenceLog = ZString.Replicate('a', 300) + dateStamp;

			var dateStampNew = "|TYP=ARV|NEW=07-Jun-15";
			var referenceLogNew = ZString.Replicate('a', StmALogSchema.SL_Reference.MaxLength - dateStampNew.Length) + dateStampNew;

			AssertEndsWith("Reference", referenceLog, businessObject.Logs.MostRecentLog.SL_Reference);

			EnsureEventReferenceUpdated(
				condition: EventReferenceConditionList.Codes.EventReference,
				isLogEstimated: true,
				oldConditionValue: "ManUtd",
				oldReference: "ManUtd To: 07-Jun-15",
				newConditionValue: ZString.Replicate('a', StmALogSchema.SL_Reference.MaxLength),
				expectedReference: referenceLog);

			EnsureEventReferenceUpdatedNewStyle(
				condition: EventReferenceConditionList.Codes.EventReference,
				isLogEstimated: true,
				oldConditionValue: "ManUtd",
				oldReference: $"ManUtd{dateStampNew}",
				newConditionValue: ZString.Replicate('a', StmALogSchema.SL_Reference.MaxLength - dateStampNew.Length),
				expectedReference: referenceLogNew);
		}

		void TestP9_EstDuration_SetToEmptyWhenIsMilestoneOrIsExceptionTrue(ZPropertyInfo isMilestoneOrIsExceptionInfo)
		{
			ProcessTask.P9_EstDuration = new ZDateTime(2000, 1, 1);
			isMilestoneOrIsExceptionInfo.Value = ZBool.False;
			AssertEquals("P9_EstDuration not cleared if not a milestone", ZDateTime.DefaultDurationEpoch, ProcessTask.P9_EstDuration);

			isMilestoneOrIsExceptionInfo.Value = ZBool.True;
			AssertEquals("P9_EstDuration cleared when IsMilestone set to true", ZDateTime.Empty, ProcessTask.P9_EstDuration);
		}

		void ClearAllDepartureLogs(Logs logs)
		{
			foreach (StmALog log in new ArrayList(logs.GetAllLogs()))
			{
				if (log.SL_SE_NKEvent == Events.Departure.Code)
				{
					log.Delete();
				}
			}
		}

		#endregion

		#region P9_SE_NKExceptionEvent

		public void TestP9_SE_NKExceptionEvent_DefaultedWhenBecomesMilestone()
		{
			AssertEquals("No exception event initially", true, ProcessTask.P9_SE_NKExceptionEvent.IsEmpty);
			ProcessTask.IsMilestone = true;
			AssertEquals("Default exception event populated", ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily, ProcessTask.P9_SE_NKExceptionEvent);
		}

		public void TestP9_SE_NKExceptionEventDescription()
		{
			ProcessTask.P9_SE_NKExceptionEvent = "";
			AssertEquals("", ProcessTask.P9_SE_NKExceptionEventDescription);

			ProcessTask.P9_SE_NKExceptionEvent = "~ZZ";
			AssertEquals("", ProcessTask.P9_SE_NKExceptionEventDescription);

			ProcessTask.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily;
			AssertEquals("Exception-Scheduled Action Missed (checked daily)", ProcessTask.P9_SE_NKExceptionEventDescription);
		}

		public void TestP9_SE_NKExceptionEvent_ReadOnlyIfNotAMilestoneOrException()
		{
			AssertEquals("Exception event type not editable by default", true, ProcessTask.P9_SE_NKExceptionEventInfo.ReadOnly);

			ProcessTask.IsMilestone = true;
			AssertEquals("Exception event type editable when task is a milestone", false, ProcessTask.P9_SE_NKExceptionEventInfo.ReadOnly);

			ProcessTask.IsMilestone = false;
			ProcessTask.IsException = true;
			AssertEquals("Exception event type editable when task is an exception", false, ProcessTask.P9_SE_NKExceptionEventInfo.ReadOnly);
		}

		public void TestEXAEventLogIsCreatedWhenDelayedMilestoneExceptionIsActioned()
		{
			var milestone = Dummy.WorkflowItems.AddNew();
			milestone.IsMilestone = true;
			milestone.TriggerConditions.TriggerEventCode = Events.Booked.Code;
			milestone.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			Dummy.Logs.AddNew(Events.Booked);

			Factory.Save();

			Assert("Exception actioned when milestone is met", Dummy.WorkflowItems.Exceptions[0].IsExceptionActioned);
			MasterFilesTestHelper.AssertEventRaised(Dummy, Events.ExceptionActionedCode);
		}

		public void TestEXAEventLog_WhenExceptionIsActionedThenUnactioned()
		{
			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "exception!";

			var exceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType1.WET_Code = "TP";
			exceptionType1.WET_Description = nameof(exceptionType1);
			exception.ExceptionTypeCode = exceptionType1.WET_Code;

			Factory.Save();

			exception.IsExceptionActioned = true;
			exception.IsExceptionActioned = false;

			Factory.Save();

			var log = MasterFilesTestHelper.GetLatestLog(Dummy, Events.ExceptionActionedCode, "|TYP=TP|DES=exception!");
			Assert("If exception is changed to unactioned before saving, it should be cancelled", log.IsCancelled);

			exception.IsExceptionActioned = true;
			Factory.Save();
			exception.IsExceptionActioned = false;
			Factory.Save();

			log = MasterFilesTestHelper.GetLatestLog(Dummy, Events.ExceptionActionedCode, "|TYP=TP|DES=exception!");
			Assert("If exception is changed to unactioned after saving, it should not be cancelled", !log.IsCancelled);
		}

		public void TestActioningExceptionWithP9_StatusErrorReport()
		{
			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			exception.P9_Description = "error exception";

			exception.P9_Status = ExceptionStatusCodeList.Codes.Actioned;

			AssertEquals("Should error report when setting P9_Status directly for an exception", "Exceptions should not be actioned by setting P9_Status. Instead, use IsExceptionActioned.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region Synchronizing StmALog and Parent Date Properties

		public void TestP9_ActualDate_SynchronizedWithStmALog()
		{
			ProcessTask.IsMilestone = true;
			ProcessTask.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			ProcessTask.SetMilestoneActualDateForTest(new ZDateTime(2005, 1, 2));
			Factory.Save();

			StmALog log = Dummy.Logs.MostRecentLogByEventTime(Events.Arrival);
			AssertNotNull("StmALog created when actual date populated", log);
			AssertEquals("StmALog created with the correct date", ProcessTask.P9_ActualDate, log.SL_EventTime);

			ProcessTask.SetMilestoneActualDateForTest(ZDateTime.Empty);
			AssertEquals("StmALog cancelled when actual date cleared", true, log.SL_IsCancelled);
		}

		public void TestEventsOnChildrenCanFireWorkflowOnParent()
		{
			var shipment = (EnterpriseBusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			shipment[JobShipmentSchema.Constants.JS_UniqueConsignRef] = "S00000100";
			shipment[JobShipmentSchema.Constants.JS_TransportMode] = Core.Constants.TransportModes.Sea;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";

			Factory.Save();

			AssertNotEquals("Shipment has JobHeader as a child", null, shipment.GetType().GetProperty("ShipmentJobHeader", BindingFlags.Instance | BindingFlags.Public).GetValue(shipment, null));

			var log = jobHeader.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2020, 1, 1);
				log.SL_SE_NKEvent = Events.ServiceInvoicePostedCode;
			}

			Factory.Save();
			((IExternalWorkflowTrigger)log).FireWorkflow();

			var processTask = ((IWorkflowProvider)shipment).WorkflowItems.Milestones.AddNew();
			processTask.TriggerConditions.TriggerEventCode = Events.ServiceInvoicePostedCode;
			AssertEquals("ProcessTask on shipment (the parent) found log that was on JobHeader (the child)", log.SL_EventTime, processTask.P9_ActualDate);
			Assert("SIV not 'propagated' to shipment from dbo.JobHeader", shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, "SIV")).Length == 0);
		}

		#region P9_ActualDate

		[TestDate(2016, 7, 3)]
		public void TestP9_ActualDate_FutureDateEnteredByUser_ForMilestone_OnlyValidateOnChange()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-1));

			Factory.Save();

			milestone.Validation.ValidateAll();
			AssertNoError(milestone.P9_ActualDateInfo, "Milestones cannot have an Actual Start time that is in the future.");

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(-3); // Go Back in time.

			milestone.Validation.ValidateAll();
			AssertHasError(milestone.P9_ActualDateInfo, "Milestones cannot have an Actual Start time that is in the future.");
		}

		public void TestP9_ActualDate_FutureDateEnteredByUser_ForMilestone_WhenAllowedInRegistry()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			Factory.Save();

			MasterFilesTestHelper.AssertNoEventRaised(job, Events.ArrivalCode);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(1));

			MasterFilesTestHelper.AssertEventRaised(job, Events.ArrivalCode);
			AssertNoErrors(milestone.P9_ActualDateInfo);
			AssertNoErrors(milestone.P9_ActualDateForBindingInfo);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			MasterFilesTestHelper.AssertNoEventRaised(job, Events.ArrivalCode);
		}

		public void TestP9_ActualDate_FutureDateEnteredByUser_ForMilestone_WhenDisallowedInRegistry()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			Factory.Save();

			MasterFilesTestHelper.AssertNoEventRaised(job, Events.ArrivalCode);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(1));

			AssertHasError(milestone.P9_ActualDateInfo, "Milestones cannot have an Actual Start time that is in the future.");
			AssertHasError(milestone.P9_ActualDateForBindingInfo, "Milestones cannot have an Actual Start time that is in the future.");

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-1));
			var log = MasterFilesTestHelper.GetLatestLog(job, Events.ArrivalCode, null);
			AssertEquals(milestone.P9_ActualDate, log?.SL_EventTime);
		}

		public void TestP9_ActualDate_FutureDateEnteredByUser_ForMilestoneNotInDatabase_WhenAllowedInRegistry()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			MasterFilesTestHelper.AssertNoEventRaised(job, Events.ArrivalCode);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(1));

			MasterFilesTestHelper.AssertEventRaised(job, Events.ArrivalCode);
			AssertNoErrors(milestone.P9_ActualDateInfo);
			AssertNoErrors(milestone.P9_ActualDateForBindingInfo);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			MasterFilesTestHelper.AssertNoEventRaised(job, Events.ArrivalCode);
		}

		public void TestP9_ActualDate_FutureDateOnEvent_ForMilestone_WhenDisallowedInRegistry()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			Factory.Save();

			MasterFilesTestHelper.AssertNoEventRaised(job, Events.ArrivalCode);

			var eventTime = ZDateTimeOffset.Now.AddDays(1);
			var log = job.GetLogs().AddNew(Events.Arrival, eventTime);

			MasterFilesTestHelper.AssertEventRaised(job, Events.ArrivalCode);
			AssertEquals(eventTime.ToZDateTime(), milestone.P9_ActualDate);

			CombineAssertions(() =>
			{
				AssertHasError("P9_ActualDate", milestone.P9_ActualDateInfo, "Milestones cannot have an Actual Start time that is in the future.");
				AssertHasError("P9_ActualDateForBinding", milestone.P9_ActualDateForBindingInfo, "Milestones cannot have an Actual Start time that is in the future.");
			});

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);

			MasterFilesTestHelper.AssertEventRaised(job, Events.ArrivalCode);
			AssertNoErrors(milestone.P9_ActualDateInfo);
			AssertNoErrors(milestone.P9_ActualDateForBindingInfo);
		}

		public void TestP9_ActualDate_FutureDateOnEvent_ForTrigger_WhenDisallowedInRegistry()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;

			Factory.Save();

			MasterFilesTestHelper.AssertNoEventRaised(job, Events.ArrivalCode);

			job.GetLogs().AddNew(Events.Arrival, ZDateTimeOffset.Now.AddDays(1));

			MasterFilesTestHelper.AssertEventRaised(job, Events.ArrivalCode);
			AssertNoErrors(trigger.P9_ActualDateInfo);
			AssertNoErrors(trigger.P9_ActualDateForBindingInfo);
		}

		public void TestP9_ActualDate_FutureDateEnteredByUser_ForTask_WhenDisallowedInRegistry()
		{
			WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			task.TaskProperties.ActualDate = ZDateTimeOffset.Now.AddDays(1);

			AssertNoErrors(task.P9_ActualDateInfo);
			AssertNoErrors(task.P9_ActualDateForBindingInfo);
		}

		#endregion

		#endregion

		#region ReadOnly for P9_Type / P9_Status / P9_EstDuration

		public void TestP9_Type_ReadOnly()
		{
			AssertEquals("Editable by default", false, ProcessTask.P9_TypeInfo.ReadOnly);
			ProcessTask.IsMilestone = true;
			ProcessTask.IsException = false;
			AssertEquals("Not editable when a milestone", true, ProcessTask.P9_TypeInfo.ReadOnly);
			ProcessTask.IsMilestone = false;
			ProcessTask.IsException = true;
			AssertEquals("Not editable when an exception", true, ProcessTask.P9_TypeInfo.ReadOnly);
		}

		public void TestP9_Status_ReadOnlyIfMilestoneOrException()
		{
			AssertEquals("Editable by default", false, ProcessTask.P9_StatusInfo.ReadOnly);
			ProcessTask.IsMilestone = true;
			ProcessTask.IsException = false;
			AssertEquals("Not editable when a milestone", true, ProcessTask.P9_StatusInfo.ReadOnly);
			ProcessTask.IsMilestone = false;
			ProcessTask.IsException = true;
			AssertEquals("Not editable when an exception (open or resolved)", true, ProcessTask.P9_StatusInfo.ReadOnly);
		}

		public void TestP9_EstDuration_ReadOnlyIfMilestoneOrException()
		{
			AssertEquals("Editable by default", false, ProcessTask.P9_EstDurationInfo.ReadOnly);
			ProcessTask.IsMilestone = true;
			ProcessTask.IsException = false;
			AssertEquals("Not editable when a milestone", true, ProcessTask.P9_EstDurationInfo.ReadOnly);
			ProcessTask.IsMilestone = false;
			ProcessTask.IsException = true;
			AssertEquals("Not editable when an exception", true, ProcessTask.P9_EstDurationInfo.ReadOnly);
		}

		public void TestP9_HighEstDur_OutOfRange()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var task = dummy.WorkflowItems.Tasks.AddNew();

			task.P9_EstDuration = new ZDateTime(2013, 1, 1, 2, 0, 0);
			task.P9_EstimateVariationFactor = 77777777;

			AssertNoExceptionThrown(delegate
			{ ZDateTime dt = task.HighEstimatedDuration; });
		}

		#endregion

		#region ReadOnly for P9_GE_TriggerDepartment

		public void TestTriggerDepartmentReadOnly()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals(true, task.P9_GE_TriggerDepartmentInfo.ReadOnly);
		}

		#endregion

		#endregion

		public void TestTriggerDoesNotFireOnCanceledLog()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var trigger1 = dummy.WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			trigger1.P9_Description = "trigger 1";

			var trigger2 = dummy.WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			trigger2.P9_Description = "trigger 2";
			((ITriggerConditions)trigger2).TriggerFiredCountdown = 100;

			Factory.Save();

			ProcessTaskHandler.SetOnFireHookForTest((log, methodName) =>
			{
				if (methodName == "UpdateTriggerEventDates" && log.SL_SE_NKEvent == Events.CustomisableEvent00.Code)
				{
					using (((IBusinessObjectInternals)log).SuppressReportRowDeletedError())
					{
						((BusinessObject)log).Delete();
					}
				}
			});

			var stmALog = dummy.Logs.AddNew(Events.CustomisableEvent00);
			AssertEquals("second trigger should not fire", new ZShort(100), Factory.Load<ProcessTask>(trigger2.PK).P9_TriggerFiredCountdown);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		#region ProcessWorkflowException

		public void TestProcessWorkflowException_ShouldNotCreate_WhenNoRelevantChanges()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();
			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "XTP";
			processWorkflowExceptionType.WET_Description = "beType";
			exception.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;
			var processWorkflowExceptionCause = Factory.New<ProcessWorkflowExceptionCause>();
			processWorkflowExceptionCause.WEC_WET_Type = processWorkflowExceptionType.PK;
			processWorkflowExceptionCause.WEC_Code = "CA1";
			processWorkflowExceptionCause.WEC_Description = "beCause";
			Factory.Save();

			var processWorkflowException = exception.ProcessWorkflowException;
			Factory.Save();

			AssertEquals(false, processWorkflowException.IsInDatabase);

			task.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;
			Factory.Save();

			AssertEquals(false, processWorkflowException.IsInDatabase);

			exception.ProcessWorkflowException.WEX_WEC_Cause = processWorkflowExceptionCause.PK;
			Factory.Save();

			Assert(processWorkflowException.IsInDatabase);
		}

		public void TestProcessWorkflowException_ShouldCreateOne_WhenNewExceptionWithCode()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertNull(task.ProcessWorkflowException);
			AssertNull(milestone.ProcessWorkflowException);
			AssertNull(exception.ProcessWorkflowException);

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "EXT";
			task.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;
			milestone.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;
			exception.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;

			AssertNull(task.ProcessWorkflowException);
			AssertNull(milestone.ProcessWorkflowException);
			AssertNotNull(exception.ProcessWorkflowException);
		}

		public void TestProcessWorkflowException()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertNull(task.ProcessWorkflowException);
			AssertNull(milestone.ProcessWorkflowException);
			AssertNull(exception.ProcessWorkflowException);

			var processWorkflowExceptionForTask = Factory.New<ProcessWorkflowException>();
			processWorkflowExceptionForTask.WEX_P9_ProcessTask = task.PK;
			var processWorkflowExceptionForMilestone = Factory.New<ProcessWorkflowException>();
			processWorkflowExceptionForMilestone.WEX_P9_ProcessTask = milestone.PK;
			var processWorkflowExceptionForException = Factory.New<ProcessWorkflowException>();
			processWorkflowExceptionForException.WEX_P9_ProcessTask = exception.PK;

			AssertNull("Tasks should never have ProcessWorkflowException", task.ProcessWorkflowException);
			AssertNull("Milestone should never have ProcessWorkflowException", milestone.ProcessWorkflowException);
			AssertNull("Exception should not have ProcessWorkflowException yet because no ProcessWorkflowExceptionCode was set", exception.ProcessWorkflowException);

			var processWorkflowExceptionTypeCode = "BLA";

			task.ExceptionTypeCode = processWorkflowExceptionTypeCode;
			milestone.ExceptionTypeCode = processWorkflowExceptionTypeCode;
			exception.ExceptionTypeCode = processWorkflowExceptionTypeCode;

			AssertNull("Tasks should never have ProcessWorkflowException", task.ProcessWorkflowException);
			AssertNull("Milestone should never have ProcessWorkflowException", milestone.ProcessWorkflowException);
			AssertEquals("Exception should have ProcessWorkflowException", processWorkflowExceptionForException, exception.ProcessWorkflowException);
		}

		public void TestExceptionType()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Im a milestone!";
			var exception = job.WorkflowItems.Exceptions.AddNew();

			task.ExceptionTypeCode = milestone.ExceptionTypeCode = exception.ExceptionTypeCode = ZString.Empty;

			AssertNull(task.ProcessWorkflowException);
			AssertNull(milestone.ProcessWorkflowException);
			AssertNull(exception.ProcessWorkflowException);

			var processWorkflowExceptionType1 = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType1.WET_Code = "001";
			processWorkflowExceptionType1.WET_Description = nameof(processWorkflowExceptionType1);

			task.ExceptionTypeCode = processWorkflowExceptionType1.WET_Code;

			AssertNull("Tasks should never have ExceptionType", task.ExceptionType);
			AssertEquals("Tasks should never have ExceptionTypeCode", ZString.Empty, task.ExceptionTypeCode);
			AssertNull("Milestone should not have ExceptionType", milestone.ExceptionType);
			AssertEquals("Milestone should not have ExceptionTypeCode", ZString.Empty, task.ExceptionTypeCode);
			AssertNull("Exception should not have ExceptionType", exception.ExceptionType);
			AssertEquals("Exception should not have ExceptionTypeCode", ZString.Empty, exception.ExceptionTypeCode);

			milestone.ExceptionTypeCode = processWorkflowExceptionType1.WET_Code;

			AssertEquals("Milestone should have ExceptionType", milestone.ExceptionType, processWorkflowExceptionType1);
			AssertEquals("Milestone should have ExceptionTypeCode", milestone.ExceptionTypeCode, processWorkflowExceptionType1.WET_Code);
			AssertEquals("Milestone should keep its description", milestone.P9_Description, "Im a milestone!");

			exception.ExceptionTypeCode = processWorkflowExceptionType1.WET_Code;

			AssertEquals("Exception should have ProcessWorkflowExceptionType", exception.ExceptionType, processWorkflowExceptionType1);
			AssertEquals("Exception should have ProcessWorkflowExceptionTypeCode", exception.ExceptionTypeCode, processWorkflowExceptionType1.WET_Code);
			AssertEquals("Exception should have description of the type", exception.P9_Description, processWorkflowExceptionType1.WET_Description);

			var processWorkflowExceptionType2 = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType2.WET_Code = "002";
			processWorkflowExceptionType2.WET_Description = nameof(processWorkflowExceptionType2);

			exception.ExceptionTypeCode = processWorkflowExceptionType2.WET_Code;

			AssertEquals("Exception should change ProcessWorkflowExceptionType", exception.ExceptionType, processWorkflowExceptionType2);
			AssertEquals("Exception should change ProcessWorkflowExceptionTypeCode", exception.ExceptionTypeCode, processWorkflowExceptionType2.WET_Code);

			var processWorkflowExceptionTypeForJob = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionTypeForJob.WET_Code = "003";
			processWorkflowExceptionTypeForJob.WET_JobType = exception.WorkflowType;
			exception.ExceptionTypeCode = "003";

			AssertEquals("Exception ProcessWorkflowExceptionType should be the job type one", exception.ExceptionType, processWorkflowExceptionTypeForJob);
			AssertEquals("Exception ProcessWorkflowExceptionTypeCode should be the job type one", exception.ExceptionTypeCode, processWorkflowExceptionTypeForJob.WET_Code);
		}

		public void TestExceptionTypeCategory()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertNull(exception.ExceptionType);
			AssertEquals(ZString.Empty, exception.ExceptionTypeCategory);

			exception.ExceptionTypeCategory = "BLA";

			AssertEquals("BLA", exception.ExceptionTypeCategory);

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "TP1";
			processWorkflowExceptionType.WET_Category = "OPA";

			exception.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;

			AssertEquals("OPA", exception.ExceptionTypeCategory);
		}

		public void TestExceptionCausePK()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(ZGuid.Empty, exception.ExceptionCausePK);

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "EXT";
			exception.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;

			var processWorkflowException = exception.ProcessWorkflowException;

			AssertNotNull(processWorkflowException);

			var processWorkflowExceptionCause = Factory.New<ProcessWorkflowExceptionCause>();
			processWorkflowException.WEX_WEC_Cause = processWorkflowExceptionCause.PK;

			AssertEquals("Cause should be the same of the processWorkflowException", processWorkflowExceptionCause.PK, exception.ExceptionCausePK);
		}

		public void TestExceptionResolutionPK()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(ZGuid.Empty, exception.ExceptionCausePK);

			var processWorkflowExceptionType = Factory.New<ProcessWorkflowExceptionType>();
			processWorkflowExceptionType.WET_Code = "EXT";
			exception.ExceptionTypeCode = processWorkflowExceptionType.WET_Code;

			var processWorkflowException = exception.ProcessWorkflowException;

			AssertNotNull(processWorkflowException);

			var processWorkflowExceptionResolution = Factory.New<ProcessWorkflowExceptionResolution>();
			processWorkflowException.WEX_WER_Resolution = processWorkflowExceptionResolution.PK;

			AssertEquals("Cause should be the same of the processWorkflowException", processWorkflowExceptionResolution.PK, exception.ExceptionResolutionPK);
		}

		#endregion

		public void TestP9_DelayDuration()
		{
			ProcessTask.P9_DelayDuration = TimeSpan.FromMilliseconds(60000);
			AssertEquals(60, ProcessTask.P9_DelayDurationSeconds);

			ProcessTask.P9_DelayDuration = ProcessTask.P9_DelayDuration.AddYears(5);
			AssertEquals("Duration is calculated from start of the year so the year number doesn't matter", 60, ProcessTask.P9_DelayDurationSeconds);

			ProcessTask.P9_DelayDuration = TimeSpan.FromDays(366);
			AssertEquals("Wraps around 1 year due to the way GUI timespans are calculated", 86400, ProcessTask.P9_DelayDurationSeconds);
		}

		public void TestTaskEffectiveNudgeOnNudgelessTask()
		{
			var job = Factory.New<OrgHeader>();
			var task = job.WorkflowItems.AddNew();

			AssertEquals(ZString.Empty, task.EffectiveTaskNudge);
			AssertEquals(0m, task.GetEffectiveTaskNudgeValue());
		}

		public void TestTemplateApplicationRaceHanlderIsHooked()
		{
			var task1 = Factory.NewWithValidTestData<ProcessTask>();
			task1.P9_ParentTemplateID = ZGuid.NewZGuid();
			Factory.Save();

			AssertEquals("The race condition handler should be hooked on saving", false, WorkflowAfterOnSavingBOService.TryHookupRaceConditionHandlerService(Factory, TemplateApplicationRaceHandlingConfig.GetConfig()));
		}

		public void TestInMemoryDuplicateTasksAreMerged()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			var task2 = dummy.WorkflowItems.Tasks.AddNew();
			task2.P9_ParentTemplateID = ZGuid.NewZGuid();
			var task3 = dummy.WorkflowItems.Tasks.AddNew();
			task3.P9_ParentTemplateID = task2.P9_ParentTemplateID;

			Factory.Save();

			AssertEquals("The race condition handler should be hooked on saving", false, WorkflowAfterOnSavingBOService.TryHookupRaceConditionHandlerService(Factory, TemplateApplicationRaceHandlingConfig.GetConfig()));

			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			var reloadedDummy = factory.Load<DummyWithWorkflow>(dummy.PK);
			AssertEquals(1, reloadedDummy.WorkflowItems.Tasks.Count);
		}

		public void TestTaskEffectiveNudge_NoNudgeFromTaskTags()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			var helper = ObjectFactory.Get<IBMTestHelper>();

			var tagDef = helper.CreateTagDefinition(Factory, "DEF");
			var jobTag = helper.CreateTagMagnitude(tagDef, "JBT", nudge: 1);
			var workflowTag = helper.CreateTagMagnitude(tagDef, "WFT", nudge: 2);

			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var link1 = jobHeader.TagLinks_ForBinding.AddNew();
			link1.TGL_TGM_Magnitude = jobTag.PK;

			var link2 = workflow.TagLinks_ForBinding.AddNew();
			link2.TGL_TGM_Magnitude = workflowTag.PK;

			jobHeader.FH_VoteUpDownAmount = 3;
			workflow.FH_VoteUpDownAmount = 4;

			AssertEquals("10", task.EffectiveTaskNudge);
			AssertEquals(10m, task.GetEffectiveTaskNudgeValue());
		}

		public void TestTaskEffectiveNudge_TaskTagsOnly_NoWorkflow()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var helper = ObjectFactory.Get<IBMTestHelper>();

			var tagDef = helper.CreateTagDefinition(Factory, "DEF");
			var taskTag1 = helper.CreateTagMagnitude(tagDef, "TT1", nudge: 1);
			var taskTag2 = helper.CreateTagMagnitude(tagDef, "TT2", nudge: 2);

			var job = Factory.New<OrgHeader>();
			var task = job.WorkflowItems.AddNew();

			var link1 = (ITagLink)((ITagBindable)task).TagLinks_ForBinding.AddNew();
			link1.TGL_TGM_Magnitude = taskTag1.PK;

			var link2 = (ITagLink)((ITagBindable)task).TagLinks_ForBinding.AddNew();
			link2.TGL_TGM_Magnitude = taskTag2.PK;

			AssertEquals("3", task.EffectiveTaskNudge);
			AssertEquals(3m, task.GetEffectiveTaskNudgeValue());
		}

		public void TestTaskEffectiveNudge_WorkflowsAndTasks()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var helper = ObjectFactory.Get<IBMTestHelper>();

			var tagDef = helper.CreateTagDefinition(Factory, "DEF");

			var taskTag1 = helper.CreateTagMagnitude(tagDef, "TT1", nudge: 1);
			var taskTag2 = helper.CreateTagMagnitude(tagDef, "TT2", nudge: 2);
			var jobTag = helper.CreateTagMagnitude(tagDef, "JBT", nudge: -1);
			var workflowTag = helper.CreateTagMagnitude(tagDef, "WFT", nudge: 2);

			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var link1 = (ITagLink)((ITagBindable)task).TagLinks_ForBinding.AddNew();
			link1.TGL_TGM_Magnitude = taskTag1.PK;

			var link2 = (ITagLink)((ITagBindable)task).TagLinks_ForBinding.AddNew();
			link2.TGL_TGM_Magnitude = taskTag2.PK;

			var link3 = jobHeader.TagLinks_ForBinding.AddNew();
			link3.TGL_TGM_Magnitude = jobTag.PK;

			var link4 = workflow.TagLinks_ForBinding.AddNew();
			link4.TGL_TGM_Magnitude = workflowTag.PK;

			jobHeader.FH_VoteUpDownAmount = 3;
			workflow.FH_VoteUpDownAmount = 4;

			AssertEquals("11", task.EffectiveTaskNudge);
			AssertEquals(11m, task.GetEffectiveTaskNudgeValue());
		}

		public void TestTaskEffectiveNudge_WorkflowsAndTasks_TotalNegativeNudge()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var helper = ObjectFactory.Get<IBMTestHelper>();

			var tagDef = helper.CreateTagDefinition(Factory, "DEF");

			var taskTag1 = helper.CreateTagMagnitude(tagDef, "TT1", nudge: 1);
			var taskTag2 = helper.CreateTagMagnitude(tagDef, "TT2", nudge: 2);
			var jobTag = helper.CreateTagMagnitude(tagDef, "JBT", nudge: -1);
			var workflowTag = helper.CreateTagMagnitude(tagDef, "WFT", nudge: 2);

			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var link1 = (ITagLink)((ITagBindable)task).TagLinks_ForBinding.AddNew();
			link1.TGL_TGM_Magnitude = taskTag1.PK;

			var link2 = (ITagLink)((ITagBindable)task).TagLinks_ForBinding.AddNew();
			link2.TGL_TGM_Magnitude = taskTag2.PK;

			var link3 = jobHeader.TagLinks_ForBinding.AddNew();
			link3.TGL_TGM_Magnitude = jobTag.PK;

			var link4 = workflow.TagLinks_ForBinding.AddNew();
			link4.TGL_TGM_Magnitude = workflowTag.PK;

			jobHeader.FH_VoteUpDownAmount = 3;
			workflow.FH_VoteUpDownAmount = -40;

			AssertEquals("-33", task.EffectiveTaskNudge);
			AssertEquals(-33m, task.GetEffectiveTaskNudgeValue());
		}

		public void TestTaskEffectiveNudge_BufferManagementDisabled()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = false;

			var helper = ObjectFactory.Get<IBMTestHelper>();

			var tagDef = helper.CreateTagDefinition(Factory, "DEF");

			var taskTag1 = helper.CreateTagMagnitude(tagDef, "TT1", nudge: 1);
			var taskTag2 = helper.CreateTagMagnitude(tagDef, "TT2", nudge: 2);
			var jobTag = helper.CreateTagMagnitude(tagDef, "JBT", nudge: -1);
			var workflowTag = helper.CreateTagMagnitude(tagDef, "WFT", nudge: 2);

			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			var link1 = (ITagLink)((ITagBindable)task).TagLinks_ForBinding.AddNew();
			link1.TGL_TGM_Magnitude = taskTag1.PK;

			var link2 = (ITagLink)((ITagBindable)task).TagLinks_ForBinding.AddNew();
			link2.TGL_TGM_Magnitude = taskTag2.PK;

			var link3 = jobHeader.TagLinks_ForBinding.AddNew();
			link3.TGL_TGM_Magnitude = jobTag.PK;

			var link4 = workflow.TagLinks_ForBinding.AddNew();
			link4.TGL_TGM_Magnitude = workflowTag.PK;

			jobHeader.FH_VoteUpDownAmount = 3;
			workflow.FH_VoteUpDownAmount = 4;

			AssertEquals("11", task.EffectiveTaskNudge);
			AssertEquals(11m, task.GetEffectiveTaskNudgeValue());
		}

		public void TestWorkflowTriggersSecurityAllowed()
		{
			AssertTriggersSecurityReadonly(true);
		}

		public void TestWorkflowTriggersSecurityNotAllowed()
		{
			AssertTriggersSecurityReadonly(false);
		}

		void AssertTriggersSecurityReadonly(bool securityIsAllowed)
		{
			AssertPropertyReadOnly(Dummy.ControllerID,
				Dummy.WorkflowItems.Triggers,
				SecurityCore.WorkflowTriggersAutoGeneratedCode,
				securityIsAllowed,
				task => task.P9_DescriptionInfo,
				task => task.P9_RespondToCascadedEventsInfo);
		}

		public void TestWorkflowTriggersSecurityAllowedOtherCompany()
		{
			AssertTriggersSecurityReadonlyOtherCompany(true);
		}

		public void TestWorkflowTriggersSecurityNotAllowedOtherCompany()
		{
			AssertTriggersSecurityReadonlyOtherCompany(false);
		}

		void AssertTriggersSecurityReadonlyOtherCompany(bool securityIsAllowed)
		{
			AssertPropertyReadOnlyForOtherCompany(Dummy.ControllerID,
				Dummy.WorkflowItems.Triggers,
				SecurityCore.WorkflowTriggersAutoGeneratedCode,
				securityIsAllowed,
				task => task.P9_DescriptionInfo,
				task => task.P9_RespondToCascadedEventsInfo);
		}

		public void TestMilestoneSecurityAllowed()
		{
			AssertMilestonesSecurityReadonly(true);
		}

		public void TestMilestoneSecurityNotAllowed()
		{
			AssertMilestonesSecurityReadonly(false);
		}

		void AssertMilestonesSecurityReadonly(bool securityIsAllowed)
		{
			AssertPropertyReadOnly(Dummy.ControllerID,
				Dummy.WorkflowItems.Milestones,
				SecurityCore.WorkflowMilestonesAutoGeneratedCode,
				securityIsAllowed,
				task => task.P9_DescriptionInfo,
				task => task.P9_SequenceInfo,
				task => task.P9_ActualDateInfo);
		}

		public void TestMilestoneSecurityAllowedOtherCompany()
		{
			AssertMilestonesSecurityReadonlyOtherCompany(true);
		}

		public void TestMilestoneSecurityNotAllowedOtherCompany()
		{
			AssertMilestonesSecurityReadonlyOtherCompany(false);
		}

		void AssertMilestonesSecurityReadonlyOtherCompany(bool securityIsAllowed)
		{
			AssertPropertyReadOnlyForOtherCompany(Dummy.ControllerID,
				Dummy.WorkflowItems.Milestones,
				SecurityCore.WorkflowMilestonesAutoGeneratedCode,
				securityIsAllowed,
				task => task.P9_DescriptionInfo,
				task => task.P9_SequenceInfo,
				task => task.P9_ActualDateInfo);
		}

		public void TestExceptionSecurityAllowed()
		{
			AssertExceptionsSecurityReadonly(true);
		}

		public void TestExceptionSecurityNotAllowed()
		{
			AssertExceptionsSecurityReadonly(false);
		}

		void AssertExceptionsSecurityReadonly(bool securityIsAllowed)
		{
			AssertPropertyReadOnly(Dummy.ControllerID,
				Dummy.WorkflowItems.Exceptions,
				SecurityCore.WorkflowExceptionsAutoGeneratedCode,
				securityIsAllowed,
				task => task.P9_DescriptionInfo,
				task => task.P9_ActualDateInfo);
		}

		public void TestExceptionSecurityAllowedOtherCompany()
		{
			AssertExceptionsSecurityReadonlyOtherCompany(true);
		}

		public void TestExceptionSecurityNotAllowedOtherCompany()
		{
			AssertExceptionsSecurityReadonlyOtherCompany(false);
		}

		void AssertExceptionsSecurityReadonlyOtherCompany(bool securityIsAllowed)
		{
			AssertPropertyReadOnlyForOtherCompany(Dummy.ControllerID,
				Dummy.WorkflowItems.Exceptions,
				SecurityCore.WorkflowExceptionsAutoGeneratedCode,
				securityIsAllowed,
				task => task.P9_DescriptionInfo,
				task => task.P9_ActualDateInfo);
		}

		public void TestExceptionsSecurityAllowed_ExceptionsDurationHours()
		{
			TestExceptionsDurationHoursReadOnly(true);
		}

		public void TestExceptionsSecurityNotAllowed_ExceptionsDurationHours()
		{
			TestExceptionsDurationHoursReadOnly(false);
		}

		public void TestExceptionsDurationHoursReadOnly(bool securityIsAllowed)
		{
			AssertPropertyReadOnly(Dummy.ControllerID,
			Dummy.WorkflowItems.Exceptions,
			SecurityCore.WorkflowOverrideExceptionDurationAutoGeneratedCode,
			securityIsAllowed,
			isAllowedHelper: task => ProcessTaskSecurityMan.IsAllowOverrideExceptionDuration(task),
			task => task.P9_ExceptionDurationHoursInfo);
		}

		public void TestTaskSecurityAllowed()
		{
			AssertTasksSecurityReadonly(true);
		}

		public void TestTaskSecurityNotAllowed()
		{
			AssertTasksSecurityReadonly(false);
		}

		void AssertTasksSecurityReadonly(bool securityIsAllowed)
		{
			AssertPropertyReadOnly(Dummy.ControllerID,
				Dummy.WorkflowItems.Tasks,
				SecurityCore.WorkflowTasksAutoGeneratedCode,
				securityIsAllowed,
				task => task.P9_DescriptionInfo,
				task => task.P9_SequenceInfo,
				task => task.P9_EstDurationInfo,
				task => task.P9_ActualDateInfo);
		}
		public void TestTaskSecurityAllowedOtherCompany()
		{
			AssertTasksSecurityReadonlyOtherCompany(true);
		}

		public void TestTaskSecurityNotAllowedOtherCompany()
		{
			AssertTasksSecurityReadonlyOtherCompany(false);
		}

		void AssertTasksSecurityReadonlyOtherCompany(bool securityIsAllowed)
		{
			AssertPropertyReadOnlyForOtherCompany(Dummy.ControllerID,
				Dummy.WorkflowItems.Tasks,
				SecurityCore.WorkflowTasksAutoGeneratedCode,
				securityIsAllowed,
				task => task.P9_DescriptionInfo,
				task => task.P9_SequenceInfo,
				task => task.P9_EstDurationInfo,
				task => task.P9_ActualDateInfo);
		}

		void AssertPropertyReadOnly(ControllerID controllerId, WorkflowItemCollectionView workflowItemCollectionView, string securityCode, bool securityIsAllowed, params Func<ProcessTask, ZPropertyInfo>[] properties)
		{
			AssertPropertyReadOnly(controllerId, workflowItemCollectionView, securityCode, securityIsAllowed, task => ProcessTaskSecurityMan.IsAllowEdit(task), properties);
		}

		void AssertPropertyReadOnly(ControllerID controllerId, WorkflowItemCollectionView workflowItemCollectionView, string securityCode, bool securityIsAllowed, Func<ProcessTask, bool> isAllowedHelper, params Func<ProcessTask, ZPropertyInfo>[] properties)
		{
			// Create and set properties on checkpoint before creating new task, as permissions could be cached by virtue of creating a new task
			var controller = ObjectFactory.Get<IControllerFactory>().Create(controllerId);
			using (var module = ObjectFactory.Get<IModuleFactory>().Create(controller.ModuleID))
			{
				var security = Env.Security.FindOrCreateWorkflowItemCheckpoint((SecurityCheckpoint)module.SecurityCheckpoint, securityCode);
				security.IsAllowed = securityIsAllowed;
				Factory.Save();
			}

			var task = workflowItemCollectionView.AddNew();
			task.P9_GC = Env.CurrentCompanyPK;

			workflowItemCollectionView.RefreshBindingIncludingChildren();
			Factory.Save();

			CombineAssertions(() =>
			{
				foreach (var property in properties)
				{
					AssertEquals(property(task).Description, !securityIsAllowed, property(task).ReadOnly);
				}

				AssertEquals(securityIsAllowed, isAllowedHelper(task));
			});
		}

		void AssertPropertyReadOnlyForOtherCompany(ControllerID controllerId, WorkflowItemCollectionView workflowItemCollectionView, string securityCode, bool securityIsAllowed, params Func<ProcessTask, ZPropertyInfo>[] properties)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			// Create and set properties on checkpoint before creating new task, as permissions could be cached by virtue of creating a new task
			var controller = ObjectFactory.Get<IControllerFactory>().Create(controllerId);
			using (var module = ObjectFactory.Get<IModuleFactory>().Create(controller.ModuleID))
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var security = Env.Security.FindOrCreateWorkflowItemCheckpoint((SecurityCheckpoint)module.SecurityCheckpoint, securityCode);
					security.IsAllowed = securityIsAllowed;
				}
			}

			var task = workflowItemCollectionView.AddNew();
			task.P9_GC = company.PK;

			workflowItemCollectionView.RefreshBindingIncludingChildren();
			Factory.Save();

			CombineAssertions(() =>
			{
				foreach (var property in properties)
				{
					AssertEquals(property(task).Description, !securityIsAllowed, property(task).ReadOnly);
				}

				AssertEquals(securityIsAllowed, ProcessTaskSecurityMan.IsAllowEdit(task));
			});
		}

		#region Workflow Templates

		public void TestMilestonesAreNotRemovedForCancelledTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var mileStone = template.WorkflowItems.Milestones.AddNew();
			template.P0_IsActive = false;
			Factory.Save();
			Assert(!mileStone.IsDeleted);
		}

		public void TestCloneTaskOnTemplate_WhenMultipleWorkflowsPresent()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var workflow1 = template.ProcessHeaders.AddNew();
			var workflow2 = template.ProcessHeaders.AddNew();

			var task1 = template.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;

			AssertNoExceptionThrown(() =>
			{
				var task2 = (ProcessTask)task1.Clone();
				task2.P9_FH_ProcessHeader = workflow2.PK;
				task2.P9_FH_ProcessHeader = workflow1.PK;
			});
		}

		public void TestChangeTaskToNonTask_ShouldRemoveWorkflowFK_Template()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var workflow = template.ProcessHeaders.AddNew();
			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			task.P9_Type = "";
			task.P9_Type = "NAM";
			AssertEquals(workflow.PK, task.P9_FH_ProcessHeader);

			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			AssertEquals(ZGuid.Empty, task.P9_FH_ProcessHeader);

			task.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Type = Core.Constants.Workflow.ExceptionType;
			AssertEquals(ZGuid.Empty, task.P9_FH_ProcessHeader);

			task.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			AssertEquals(ZGuid.Empty, task.P9_FH_ProcessHeader);
		}

		public void TestApplyTemplate_WhenJobFirstSaved_WithManualTask_ShouldApplyTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template.P0_SubType1 = "NAM";

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Tarasque";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "NAM";

			AssertEquals("Template hasn't been applied yet", 0, job.WorkflowItems.Tasks.Count);

			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Basilisk";

			Factory.Save();

			AssertEquals("Template has been applied, even though a 'manual' task was created.", 2, job.WorkflowItems.Tasks.Count);
			AssertEquals("Basilisk", job.WorkflowItems.Tasks[0].P9_Description);
			AssertEquals("Tarasque", job.WorkflowItems.Tasks[1].P9_Description);
		}

		public void TestApplyTemplate_WithSubsequentJobSave_WithManualTask_ShouldNotApplyTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template.P0_SubType1 = "NAM";

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Tarasque";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();

			AssertEquals("Template hasn't been applied yet", 0, job.WorkflowItems.Tasks.Count);

			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Basilisk";

			Factory.Save();

			AssertEquals("Template hasn't been applied because the template doesn't match", 1, job.WorkflowItems.Tasks.Count);

			job.O1_EnquiryType = "NAM";
			Factory.Save();

			AssertEquals("Template hasn't been applied since the job has already been saved", 1, job.WorkflowItems.Tasks.Count);
		}

		public void TestApplyTemplate_WhenJobFirstSaved_WithManualTaskAndRegistryDisabled_ShouldNotApplyTemplate()
		{
			WorkflowDataRegistry.Instance.AlwaysApplyTasksFromTemplateWhenFirstSavingJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template.P0_SubType1 = "NAM";

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_Description = "Tarasque";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "NAM";

			AssertEquals("Template hasn't been applied yet", 0, job.WorkflowItems.Tasks.Count);

			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Basilisk";

			Factory.Save();

			AssertEquals("Template hasn't been applied since the registry item controlling this behaviour has been disabled", 1, job.WorkflowItems.Tasks.Count);
			AssertEquals("Basilisk", job.WorkflowItems.Tasks[0].P9_Description);
		}

		public void TestTask_ShouldNotAppearOnJobThatBelongsToAnotherCompanyWhenTaskNotShared()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "NAVCKID";
			client.OH_FullName = "DICK VAN DYKE";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";
			template.P0_OH_Client = client.PK;
			template.P0_GC = GlbCompany.CurrentCompany.PK;
			template.P0_IsActive = true;
			template.GlobalTemplate = false;

			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_ShareTasksForAllCompanies = false;

			Factory.Save();

			IWorkflowProvider shipment = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = client.PK;
			address.OA_Code = "CHERRY";
			address.OA_Address1 = "17 CHERRY TREE LANE";
			address.OA_City = "LONDON";
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_OA_Address = address.PK;
			docAddress.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
			docAddress.E2_ParentID = shipment.PK;
			docAddress.E2_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			shipment.WorkflowItems.RemoveAndDeleteAll();
			shipment.WorkflowItems.Tasks.CreateItemsFromTemplate();

			AssertEquals(1, shipment.WorkflowItems.Tasks.Count);
			AssertEquals(GlbCompany.CurrentCompany.PK, shipment.WorkflowItems.Tasks[0].P9_GC);

			Factory.Save();

			var taskID = shipment.WorkflowItems.Tasks[0].P9_TaskID;
			AssertNotEquals(ZString.Empty, taskID);

			shipment.WorkflowItems.Tasks[0].P9_GC = company.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedShipment = (IWorkflowProvider)newFactory.Load<Forwarding.IForwardingShipment>(shipment.PK);
			AssertEquals(0, loadedShipment.WorkflowItems.Tasks.Count);

			shipment.WorkflowItems.Tasks[0].P9_Status = "SUS";
			AssertEquals("Task count should not change in new factory", 0, loadedShipment.WorkflowItems.Tasks.Count);
			Factory.Save();

			AssertEquals("Task count should not change in new factory", 0, loadedShipment.WorkflowItems.Tasks.Count);
		}

		public void TestTriggeringEventIsAccessibleInMacro()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<JS_GoodsDescription>";
			action.PQ_FieldValue = "<TriggeringEvent.SL_SE_NKEvent>";

			Factory.Save();

			shipmentWithWorkflow.Logs.AddNew(AutoEvents.CustomisableEvent00);

			Factory.Save();

			AssertEquals("Z00", shipment.JS_GoodsDescription);
		}

		public void TestTemplateAppliedToStaffWithEscapeCharacters()
		{
			var staffList = new List<IGlbStaff>();

			staffList.Add(CreateUser(@"NRM"));

			staffList.Add(CreateUser(@"\\\"));
			staffList.Add(CreateUser(@"\AW"));
			staffList.Add(CreateUser(@"AW\"));
			staffList.Add(CreateUser(@"A\A"));

			staffList.Add(CreateUser(@"<A>"));
			staffList.Add(CreateUser(@">A<"));
			staffList.Add(CreateUser(@"<AA"));
			staffList.Add(CreateUser(@"AA>"));
			staffList.Add(CreateUser(@">AA"));
			staffList.Add(CreateUser(@"AA<"));

			staffList.Add(CreateUser(@""""""""));
			staffList.Add(CreateUser(@"\"""));
			staffList.Add(CreateUser(@"\\"""));
			staffList.Add(CreateUser(@"\'\"));
			staffList.Add(CreateUser(@"\\'"));
			staffList.Add(CreateUser(@"\t\"));
			staffList.Add(CreateUser(@"\t"));

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";

			ProcessTask milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			milestone.TemplateConditions.TemplateCondition2Value = @"""<JS_SystemCreateUser>"" != ""~BP"" && ""<JS_SystemCreateUser>"" != ""~AD""";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			milestone.P9_Description = "Milestone";

			CombineAssertions(() =>
			{
				foreach (var staff in staffList)
				{
					using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
					{
						Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
						var shipment = Factory.New<Forwarding.IForwardingShipment>();
						shipment.JS_SystemCreateUser = staff.GS_Code;
						var shipmentWithWorkflow = (IWorkflowProvider)shipment;

						Factory.Save();

						shipmentWithWorkflow.Logs.AddNew(AutoEvents.CustomisableEvent00);
						var templateLoader = new ProcessTask.Loader(Factory);
						templateLoader.CreateTasksAndMilestonesFromTemplateIfRequired(shipmentWithWorkflow, TemplateApplicationParameters.ApplyIgnoreHasChanges());

						Factory.Save();

						AssertEquals($"WTA applied for {staff.GS_Code}", 1, shipmentWithWorkflow.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTemplateAppliedCode).Count());
					}
				}
			});

			ZQuery query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			var wteLogs = Factory.Load<StmALog>(query);

			AssertEquals("Check all milestones fired", staffList.Count, wteLogs.Length);
		}

		IGlbStaff CreateUser(string staffCode)
		{
			IGlbStaff staff = Factory.New<IGlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = staffCode;
			staff.GS_FullName = $"{staffCode} Full Name";

			Factory.Save();

			return staff;
		}

		public void TestApplyTemplate_WhenJobFirstSaved_WithMultipleTriggerActions_AndNoFieldNameSet_ShouldApplyAllTriggerActions()
		{
			var partial1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			partial1.P0_IsPartialTemplate = true;
			partial1.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			partial1.WorkflowItems.Milestones.AddNew();

			var partial2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			partial2.P0_IsPartialTemplate = true;
			partial2.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			partial2.WorkflowItems.Milestones.AddNew();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template.P0_SubType1 = "NAM";

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			// Set field names, which causes everything to be okay in this defect:
			var action1 = templateTrigger.ProcessTaskNotifications.AddNew();
			action1.PQ_FieldName = "\"<O1_Address1>\"";
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action1.PQ_P0_WorkflowTemplate = partial1.PK;

			var action2 = templateTrigger.ProcessTaskNotifications.AddNew();
			action2.PQ_FieldName = "\"<O1_Address2>\"";
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action2.PQ_P0_WorkflowTemplate = partial2.PK;

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "NAM";

			Factory.Save();

			var trigger = job.WorkflowItems.Triggers.SingleOrDefault() as ProcessTask;
			AssertNotNull(trigger);
			AssertEquals("Both trigger actions should have been applied.", 2, trigger.ProcessTaskNotifications.Count);
			AssertNotNull(trigger.ProcessTaskNotifications.SingleOrDefault(x => x.PQ_P0_WorkflowTemplate == partial1.PK));
			AssertNotNull(trigger.ProcessTaskNotifications.SingleOrDefault(x => x.PQ_P0_WorkflowTemplate == partial2.PK));

			// Now do it again with no field names set. This will cause a failure
			templateTrigger.ProcessTaskNotifications.DeleteAll();

			action1 = templateTrigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action1.PQ_P0_WorkflowTemplate = partial1.PK;

			action2 = templateTrigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action2.PQ_P0_WorkflowTemplate = partial2.PK;

			Factory.Save();

			job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "NAM";

			Factory.Save();

			trigger = job.WorkflowItems.Triggers.SingleOrDefault() as ProcessTask;
			AssertNotNull(trigger);
			AssertEquals("Both trigger actions should have been applied.", 2, trigger.ProcessTaskNotifications.Count);
			AssertNotNull(trigger.ProcessTaskNotifications.SingleOrDefault(x => x.PQ_P0_WorkflowTemplate == partial1.PK));
			AssertNotNull(trigger.ProcessTaskNotifications.SingleOrDefault(x => x.PQ_P0_WorkflowTemplate == partial2.PK));

			// Now prove that it's not just the ApplyWorkflowTemplateOnce type
			templateTrigger.ProcessTaskNotifications.DeleteAll();

			action1 = templateTrigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			action2 = templateTrigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			Factory.Save();

			job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "NAM";

			Factory.Save();

			trigger = job.WorkflowItems.Triggers.SingleOrDefault() as ProcessTask;
			AssertNotNull(trigger);
			AssertEquals("Both trigger actions should have been applied.", 2, trigger.ProcessTaskNotifications.Count);
		}

		#endregion

		#region Defaulting Milestone Estimated Dates

		public void TestP9_EstimateDefaultedFromAsString_ForPredecessor()
		{
			ProcessTask milestone1 = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone1.P9_ScheduledDate = new ZDateTime(2005, 1, 1);
			milestone2.P9_ScheduledDate = new ZDateTime(2005, 2, 2);

			milestone1.P9_EstimateDefaultedFromAsString = "2";
			AssertEquals("2 - ARV (est)", milestone1.P9_EstimateDefaultedFromAsString);
			AssertEquals("Correct milestone estimated from", milestone2.P9_ScheduledDate.ToZDateTime(), milestone1.GetEstimateDefaultedFromDateForTest().ToZDateTime());

			milestone1.P9_EstimateDefaultedFromAsString = "99";
			AssertEquals("Invalid milestone estimated from", "", milestone1.P9_EstimateDefaultedFromAsString);
			AssertEquals("Invalid milestone estimated from", ZDateTime.Empty, milestone1.GetEstimateDefaultedFromDateForTest().ToZDateTime());

			milestone1.P9_EstimateDefaultedFromAsString = "2 - ARV";
			AssertEquals("2 - ARV (est)", milestone1.P9_EstimateDefaultedFromAsString);
			AssertEquals("Correct milestone estimated from date set", milestone2.P9_ScheduledDate.ToZDateTime(), milestone1.GetEstimateDefaultedFromDateForTest().ToZDateTime());

			milestone1.P9_EstimateDefaultedFromAsString = "rubbish";
			AssertEquals("Invalid milestone estimated from", "", milestone1.P9_EstimateDefaultedFromAsString);

			for (var i = 0; i < milestone1.Lookups.EstimateDefaultedFromList.Count; i++)
			{
				var dropDownSelection = milestone1.Lookups.EstimateDefaultedFromList[i].Code;
				milestone1.P9_EstimateDefaultedFromAsString = dropDownSelection;
				AssertEquals("When a user selects from the dropdown list it should match P9_EstimateDefaultedFromAsString", dropDownSelection, milestone1.P9_EstimateDefaultedFromAsString);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestMilestoneEstimateDateFromPredecessorActualDate()
		{
			var milestone1 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "Mil1";
			milestone1.P9_Sequence = 1;
			milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var milestone2 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "Mil2";
			milestone2.P9_Sequence = 2;
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			milestone2.P9_EstimateDefaultedFromAsString = "1 - Z00 (act)";//this milestone should get it's estimate date from milestone1 actual date
			milestone2.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone2.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;

			Factory.Save();

			Dummy.Logs.AddNew(Events.CustomisableEvent00);//trigger milestone1 to set actual date

			Factory.Save();

			CombineAssertions("Milestone2 should recieve it's schedule date from Milestone1's Actual Date + 24 hours", () =>
			{
				AssertEquals(ZDateTime.Now, milestone1.P9_ActualDate);
				AssertEquals(true, milestone2.IsEstimateDefaultedFromPredecessorActualDate);
				AssertEquals(ZDateTime.Now.AddHours(24), milestone2.P9_ScheduledDate);
			});

			TestDateAttribute.AddDays(2);

			milestone2.P9_Description = "Milestone2";//do something for force save
			Factory.Save();

			AssertEquals("We missed the schedule date so fire exception", ZDateTime.Now, milestone2.P9_MilestoneExceptionAdded);
		}

		[TestDate(2020, 1, 1)]
		public void TestMilestoneEstimateDateFromPredecessorEstimateDate()
		{
			var milestone1 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "Mil1";
			milestone1.P9_Sequence = 1;
			milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone1.P9_ScheduledDate = ZDateTime.Now;

			var milestone2 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "Mil2";
			milestone2.P9_Sequence = 2;
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			milestone2.P9_EstimateDefaultedFromAsString = "1 - Z00 (est)";//this milestone should get it's estimate date from milestone1 estimate date
			milestone2.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(24);
			milestone2.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;

			Factory.Save();

			CombineAssertions("Milestone2 should recieve it's schedule date from Milestone1's Schedule Date + 24 hours", () =>
			{
				AssertEquals(false, milestone2.IsEstimateDefaultedFromPredecessorActualDate);
				AssertEquals(ZDateTime.Now.AddHours(24), milestone2.P9_ScheduledDate);
			});

			TestDateAttribute.AddDays(2);

			milestone2.P9_Description = "Milestone2";//do something for force save
			Factory.Save();

			AssertEquals(ZDateTime.Now, milestone2.P9_MilestoneExceptionAdded);
		}

		public void TestP9_EstimateDefaultedFromAsString_ForOtherDateCode()
		{
			ProcessTask milestone1 = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = Dummy.WorkflowItems.Milestones.AddNew();

			milestone1.P9_EstimateDefaultedFromAsString = "FAV";
			AssertEquals("FAV", milestone1.P9_EstimateDefaultedFromAsString);
			AssertEquals("P9_EstimatedDefaultedFrom set", "FAV", milestone1.P9_EstimatedDefaultedFrom);
		}

		[TestDate(2005, 1, 2)]
		public void TestEstimateDefaultedFromDate_WhenCreatedFromTemplate()
		{
			ProcessTaskTemplate[] templates = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptors.OrderWorkflowDescriptorCode));
			foreach (var oldTemplate in templates)
			{
				oldTemplate.P0_IsActive = false;
			}

			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			ProcessTask milestoneTemplate = template.WorkflowItems.Milestones.AddNew();
			milestoneTemplate.P9_EstimatedDefaultedFrom = "ORD"; // order date
			Factory.Save();

			IWorkflowProvider order = (IWorkflowProvider)Factory.New<Forwarding.IOrder>();
			((BusinessObject)order)[JobOrderHeaderSchema.JD_OA_BuyerAddress.Name] = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Factory.Save();
			ProcessTask milestone = order.WorkflowItems.Milestones[0];
			AssertEquals("Correct milestone was created", "ORD", milestone.P9_EstimatedDefaultedFrom);
			AssertEquals("Estimate date defaulted from order created date, when instantiated from template", ZDateTime.Now, milestone.P9_ScheduledDate);
		}

		[TestDate(2005, 1, 10)]
		public void TestEstimateDefaultedFromDate_ExceptionGeneratedAutomaticallyIfRequired()
		{
			ProcessTaskTemplate[] templates = Factory.Load<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptors.OrderWorkflowDescriptorCode));
			foreach (var oldTemplate in templates)
			{
				oldTemplate.P0_IsActive = false;
			}

			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			ProcessTask milestoneTemplate = template.WorkflowItems.Milestones.AddNew();
			milestoneTemplate.P9_EstimatedDefaultedFrom = "ORD"; // order date
			milestoneTemplate.P9_EstimatedDefaultTimeDelta = new ZDateTime(2000, 1, 1).AddDays(-2);
			Factory.Save();

			IWorkflowProvider order = (IWorkflowProvider)Factory.New<Forwarding.IOrder>();
			((BusinessObject)order)[JobOrderHeaderSchema.JD_OA_BuyerAddress.Name] = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			Factory.Save();
			AssertEquals("1 milestone created", 1, order.WorkflowItems.Milestones.Count);
			AssertEquals("Estimated date before today", ZDateTime.Now.AddDays(-2), order.WorkflowItems.Milestones[0].P9_ScheduledDate);
			AssertEquals("1 exception created due to the milestone not being met already", 1, order.WorkflowItems.Exceptions.Count);
		}

		public void TestEstimateDefaultedFromDate()
		{
			ProcessTask milestone1 = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			milestone1.P9_ScheduledDate = new ZDateTime(2005, 1, 1);
			milestone2.P9_ScheduledDate = new ZDateTime(2005, 2, 2);

			ProcessTask.IsMilestone = true;
			ProcessTask.P9_EstimateDefaultedFromAsString = milestone1.P9_Sequence.ToString();
			AssertEquals("EstimateDefaultedFromProperty for milestone1", milestone1.P9_ScheduledDate.ToZDateTime(), ProcessTask.GetEstimateDefaultedFromDateForTest().ToZDateTime());

			ProcessTask.P9_EstimateDefaultedFromAsString = milestone2.P9_Sequence.ToString();
			AssertEquals("EstimateDefaultedFromProperty for milestone2", milestone2.P9_ScheduledDate.ToZDateTime(), ProcessTask.GetEstimateDefaultedFromDateForTest().ToZDateTime());

			milestone2.P9_Sequence = 99;
			AssertEquals("P9_EstimatedDefaultedFromPredecessorAsString after changing predecessor P9_Sequence", milestone2.P9_Sequence.ToString() + " - ARV (est)", ProcessTask.P9_EstimateDefaultedFromAsString);
			AssertEquals("EstimateDefaultedFromProperty after changing predecessor P9_Sequence", milestone2.P9_ScheduledDate.ToZDateTime(), ProcessTask.GetEstimateDefaultedFromDateForTest().ToZDateTime());
		}

		[TestDate(2005, 1, 1)]
		public void TestEstimatedDateDefaulted_FromEstimate()
		{
			TestEstimatedDateDefaulted(false);
		}

		[TestDate(2005, 1, 1)]
		public void TestEstimatedDateDefaulted_FromActual()
		{
			TestEstimatedDateDefaulted(true);
		}

		void TestEstimatedDateDefaulted(bool fromActual)
		{
			ProcessTask milestone1 = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			milestone2.P9_EstimatedDefaultFromPredecessor = milestone1.P9_Sequence;
			milestone2.IsEstimateDefaultedFromPredecessorActualDate = fromActual;
			milestone2.P9_EstimatedDefaultTimeDelta = new ZDateTime(2000, 1, 1, 3, 0, 0);
			milestone1.P9_Sequence = 67;
			milestone1.P9_Sequence = 52;

			ZDateTime testDate = new ZDateTime(2005, 1, 1);
			if (fromActual)
			{
				milestone1.SetMilestoneActualDateForTest(testDate);
			}
			else
			{
				milestone1.P9_ScheduledDate = testDate;
			}
			Factory.Save();
			AssertEquals("P9_ScheduledDate automatically defaulted", testDate.AddHours(3), milestone2.P9_ScheduledDate);
		}

		public void TestHiddenValueForP9_EstimatedDefaultedFromValidation()
		{
			ProcessTask pt = Dummy.WorkflowItems.Milestones.AddNew();
			AssertEquals("'ACT' is the only P9_EstimatedDefaultedFrom value hidden from user", "ACT", MasterFiles.Business.ProcessTask.P9_EstimatedDefaultedFrom_Actual);
		}

		[ExpectNoExceptions]
		public void TestEstimatedDateDefaulted_DontBlowUpWhenEstimatedDefaultTimeDeltaEmpty()
		{
			ProcessTask milestone1 = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone2 = Dummy.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone2.TriggerConditions.TriggerEventCode = Events.Arrival.Code;

			milestone2.P9_EstimatedDefaultFromPredecessor = milestone1.P9_Sequence;
			milestone2.IsEstimateDefaultedFromPredecessorActualDate = false;
			milestone2.P9_EstimatedDefaultTimeDelta = ZDateTime.Empty;

			milestone1.P9_ScheduledDate = ZDateTime.Now;
			Factory.Save();
		}

		public void TestP9_EstimatedDefaultTimeDelta()
		{
			ProcessTask milestoneToDefaultFrom = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultTimeDelta = ZDateTime.Empty;
			AssertEquals("P9_EstimatedDefaultTimeDelta read only initially", true, milestone.P9_EstimatedDefaultTimeDeltaInfo.ReadOnly);

			milestone.P9_EstimatedDefaultedFrom = "FAV";
			AssertEquals("P9_EstimatedDefaultTimeDelta not read only when defaulted", false, milestone.P9_EstimatedDefaultTimeDeltaInfo.ReadOnly);

			milestone.P9_EstimatedDefaultFromPredecessor = milestoneToDefaultFrom.P9_Sequence;
			milestone.P9_EstimatedDefaultedFrom = "";
			AssertEquals("P9_EstimatedDefaultTimeDelta not read only when defaulted from a predecessor", false, milestone.P9_EstimatedDefaultTimeDeltaInfo.ReadOnly);

			milestone.P9_EstimatedDefaultFromPredecessor = milestoneToDefaultFrom.P9_Sequence;
			milestone.P9_EstimatedDefaultTimeDelta = new ZDateTime(2006, 1, 1);
			milestone.P9_EstimateDefaultedFromAsString = "";
			AssertEquals("P9_EstimatedDefaultTimeDelta read only when not defaulted", true, milestone.P9_EstimatedDefaultTimeDeltaInfo.ReadOnly);
			AssertEquals("P9_EstimatedDefaultTimeDelta emptied when not defaulted", ZDateTime.Empty, milestone.P9_EstimatedDefaultTimeDelta);

			milestone.P9_EstimatedDefaultedFrom = "FAV";
			milestone.P9_EstimatedDefaultTimeDelta = new ZDateTime(2006, 1, 1);
			milestone.P9_EstimateDefaultedFromAsString = "";
			AssertEquals("P9_EstimatedDefaultTimeDelta read only when not defaulted", true, milestone.P9_EstimatedDefaultTimeDeltaInfo.ReadOnly);
			AssertEquals("P9_EstimatedDefaultTimeDelta emptied when not defaulted", ZDateTime.Empty, milestone.P9_EstimatedDefaultTimeDelta);
		}

		public void TestP9_EstimatedDefaultFromPredecessor_CascadeUpdatedFromSequence()
		{
			ProcessTask milestoneToDefaultFrom = Dummy.WorkflowItems.Milestones.AddNew();
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();

			milestoneToDefaultFrom.P9_Sequence = 5;
			AssertEquals("P9_EstimatedDefaultFromPredecessor not cascade updated when not yet set", 0, milestone.P9_EstimatedDefaultFromPredecessor);

			milestone.P9_EstimatedDefaultFromPredecessor = 5;
			milestoneToDefaultFrom.P9_Sequence = 8;
			AssertEquals("P9_EstimatedDefaultFromPredecessor cascade updated when set", 8, milestone.P9_EstimatedDefaultFromPredecessor);
		}

		[TestUtcOffset(5, 0, 0)]
		public void TestP9_OriginalScheduledDateUtc_DefaultFromScheduledDateOnMilestone()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();

			AssertEquals(ZDateTime.Empty, milestone.P9_OriginalScheduledDateUtc);
			AssertEquals(ZDateTime.Empty, trigger.P9_OriginalScheduledDateUtc);

			var date1 = new ZDateTime(ZDateTime.SmallDateTimeUtcNow.AddDays(1), DateTimeKind.Local);
			var utcDate1 = new ZDateTime(date1.ToDateTime() - TestUtcOffsetAttribute.Time);
			milestone.P9_ScheduledDate = date1;
			trigger.P9_ScheduledDate = date1;

			AssertEquals(date1, milestone.P9_ScheduledDate);
			AssertEquals("Should set from scheduled date", utcDate1, milestone.P9_OriginalScheduledDateUtc);
			AssertEquals(date1, trigger.P9_ScheduledDate);
			AssertEquals("Should not change for non-milestone", ZDateTime.Empty, trigger.P9_OriginalScheduledDateUtc);

			var date2 = ZDateTime.SmallDateTimeUtcNow.AddDays(2);
			milestone.P9_ScheduledDate = date2;
			trigger.P9_ScheduledDate = date2;

			AssertEquals(date2, milestone.P9_ScheduledDate);
			AssertEquals("Should not change non-empty date", utcDate1, milestone.P9_OriginalScheduledDateUtc);
			AssertEquals(date2, trigger.P9_ScheduledDate);
			AssertEquals("Should not change for non-milestone", ZDateTime.Empty, trigger.P9_OriginalScheduledDateUtc);
		}

		[TestUtcOffset(9, 0, 0)]
		public void TestP9_OriginalScheduledDateLocal()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			AssertEquals(ZDateTime.Empty, milestone.P9_OriginalScheduledDateUtc);
			AssertEquals(ZDateTime.Empty, milestone.P9_OriginalScheduledDateLocal);

			milestone.P9_OriginalScheduledDateUtc = new ZDateTime(2012, 12, 6, 16, 30, 00);
			AssertEquals(new ZDateTime(2012, 12, 7, 01, 30, 00), milestone.P9_OriginalScheduledDateLocal);

			milestone.P9_OriginalScheduledDateLocal = new ZDateTime(2012, 12, 6, 5, 30, 00);
			AssertEquals(new ZDateTime(2012, 12, 5, 20, 30, 00), milestone.P9_OriginalScheduledDateUtc);
		}

		public void TestP9_OriginalScheduledDateLocalAndConvertToUtcOutOfRange()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			ProcessTaskForTest processTask1 = Factory.NewWithValidTestData<ProcessTaskForTest>();
			processTask1.P9_OriginalScheduledDateLocal = ZDateTime.MinSmallDateTimeValue;
			AssertEquals(processTask1.P9_OriginalScheduledDateUtc, ZDateTime.MinSmallDateTimeValue);

			GlbCompany.CurrentCompany.SetCountry("US");
			GlbCompany.CurrentCompany.Factory.Save();

			ProcessTaskForTest processTask2 = Factory.NewWithValidTestData<ProcessTaskForTest>();
			processTask2.P9_OriginalScheduledDateLocal = ZDateTime.MaxSmallDateTimeValue;
			AssertEquals(processTask2.P9_OriginalScheduledDateUtc, ZDateTime.MaxSmallDateTimeValue);
		}

		public void TestFindOrCreateEstimatedEvent()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.IncidentClosedCode;
			AssertNull("Precondition - no events yet", Dummy.Logs.MostRecentLogByEventTime(Events.IncidentClosed));

			AssertScheduledDateChange(milestone, new ZDateTime(2012, 11, 29, 15, 00, 00), new ZDateTime(2012, 11, 29, 15, 00, 00), "To: 29-Nov-12");
			AssertScheduledDateChange(milestone, new ZDateTime(2012, 11, 30, 15, 00, 00), new ZDateTime(2012, 11, 30, 15, 00, 00), "To: 30-Nov-12");
			Factory.Save();
			AssertScheduledDateChange(milestone, new ZDateTime(2012, 12, 01, 15, 00, 00), new ZDateTime(2012, 12, 01, 15, 00, 00), "From: 30-Nov-12 To: 01-Dec-12");
			AssertScheduledDateChange(milestone, new ZDateTime(2012, 12, 02, 15, 00, 00), new ZDateTime(2012, 12, 02, 15, 00, 00), "From: 30-Nov-12 To: 02-Dec-12");
			Factory.Save();
			AssertScheduledDateChange(milestone, new ZDateTime(2012, 12, 02, 15, 01, 00), new ZDateTime(2012, 12, 02, 15, 01, 00), "From: 02-Dec-12 To: 02-Dec-12");
			Factory.Save();
			AssertScheduledDateChange(milestone, new ZDateTime(2012, 12, 05, 15, 01, 17, 123), new ZDateTime(2012, 12, 05, 15, 01, 17, 123), "From: 02-Dec-12 To: 05-Dec-12");

			var latestLog = Dummy.Logs.MostRecentLogByEventTime(Events.IncidentClosed);
			AssertEndsWith("", "From: 02-Dec-12 To: 05-Dec-12", latestLog.SL_Reference);
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone.TriggerConditions.TriggerConditionValue = "abcde";
			AssertEndsWith("Should update reference on latest log", "abcde From: 02-Dec-12 To: 05-Dec-12", latestLog.SL_Reference);

			AssertScheduledDateChange(milestone, new ZDateTime(2012, 12, 07, 15, 00, 00), new ZDateTime(2012, 12, 07, 15, 00, 00), "abcde From: 02-Dec-12 To: 07-Dec-12");
		}

		void AssertScheduledDateChange(ProcessTask milestone, ZDateTime newScheduledDate, ZDateTime expectedNewEventTime, ZString expectedNewReference)
		{
			var oldLog = Dummy.Logs.MostRecentLogByEventTime(Events.IncidentClosed);
			var oldEventTime = oldLog != null ? oldLog.SL_EventTime : ZDateTime.Empty;
			var oldReference = oldLog != null ? oldLog.SL_Reference : ZString.Empty;

			milestone.P9_ScheduledDate = newScheduledDate;
			var log = Dummy.Logs.MostRecentLogByEventTime(Events.IncidentClosed);

			AssertNotNull(log);
			if (oldLog == null || oldLog.IsInDatabase)
			{
				AssertNotEquals("Should create new log", oldLog, log);
			}
			else
			{
				AssertSame("Should not create new log if old is not in database yet", oldLog, log);
			}

			if (oldLog != null && oldLog != log)
			{
				AssertLog(oldLog, oldEventTime, oldReference, true, true);
			}
			AssertLog(log, expectedNewEventTime, milestone.PK + "|" + expectedNewReference, true, false);
		}

		void AssertLog(StmALog log, ZDateTime expectedEventTime, ZString expectedReference, ZBool expectedIsEstimate, ZBool expectedIsCancelled)
		{
			AssertEquals(expectedEventTime.ToSmallDateTimeFloor(), log.SL_EventTime);
			AssertEquals(expectedReference, log.SL_Reference);
			AssertEquals(expectedIsEstimate, log.SL_IsEstimate);
			AssertEquals(expectedIsCancelled, log.SL_IsCancelled);
		}

		public void TestSuspendedScheduledDateChange()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();

			milestone.P9_ScheduledDate = new ZDateTime(2012, 11, 29);
			AssertEquals(new ZDateTime(2012, 11, 29), milestone.P9_ScheduledDate);

			milestone.P9_ScheduledDate = new ZDateTime(2012, 11, 30);
			AssertEquals(new ZDateTime(2012, 11, 30), milestone.P9_ScheduledDate);

			using (milestone.SuspendedScheduledDateChange())
			{
				milestone.P9_ScheduledDate = new ZDateTime(2012, 12, 01);
				AssertEquals(new ZDateTime(2012, 11, 30), milestone.P9_ScheduledDate);

				using (milestone.SuspendedScheduledDateChange())
				{
					milestone.P9_ScheduledDate = new ZDateTime(2012, 12, 02);
					AssertEquals(new ZDateTime(2012, 11, 30), milestone.P9_ScheduledDate);
				}

				milestone.P9_ScheduledDate = new ZDateTime(2012, 12, 03);
				AssertEquals(new ZDateTime(2012, 11, 30), milestone.P9_ScheduledDate);
			}

			milestone.P9_ScheduledDate = new ZDateTime(2012, 12, 04);
			AssertEquals(new ZDateTime(2012, 12, 04), milestone.P9_ScheduledDate);
		}

		#endregion

		#region Workflow Triggers

		public void TestEdtEventOnFldTarget()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			trigger.TemporarilyAllowSettingCondition("P9_TriggerFiredCountdown");
			trigger.P9_TriggerFiredCountdown = 100;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<JS_GoodsDescription>";
			action.PQ_FieldValue = "<TriggeringEvent.SL_GS_NKUser>";

			Factory.Save();

			ZQuery query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			AssertEquals((ZShort)100, trigger.P9_TriggerFiredCountdown);

			shipment.JS_GoodsDescription = "Luke";
			Factory.Save();

			AssertEquals(1, Factory.Load<StmALog>(query).Length);
			AssertEquals((ZShort)99, trigger.P9_TriggerFiredCountdown);

			MasterFilesTestHelper.RunLogWalker();

			var newFactory = new BusinessObjectFactory();
			var updatedShipment = newFactory.Load<Forwarding.IForwardingShipment>(shipment.PK);
			var updatedTrigger = newFactory.Load<ProcessTask>(trigger.PK);

			CombineAssertions(() =>
			{
				AssertEquals(trigger.P9_SystemCreateUser, updatedShipment.JS_GoodsDescription);
				AssertEquals((ZShort)99, updatedTrigger.P9_TriggerFiredCountdown);
				AssertEquals(1, newFactory.Load<StmALog>(query).Length);
			});
		}

		public void TestEDTTriggerInNonUserinteractiveEnv()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			trigger.TemporarilyAllowSettingCondition("P9_TriggerFiredCountdown");
			trigger.P9_TriggerFiredCountdown = 100;

			Factory.Save();

			AssertEquals("100", trigger.P9_TriggerFiredCountdown.ToString());
			AssertEquals("Precondition: Not in a service task", true, ObjectFactory.Get<IEnv>().Instance.ServiceTaskCode.IsNullOrEmpty());

			using (Globals.SetIsUserInteractiveForTest(false))
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				shipmentWithWorkflow.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			AssertEquals("EDT events should fire triggers when !IsUserInteractive unless in a service task", (ZShort)99, trigger.P9_TriggerFiredCountdown);
		}

		public void TestEDTTriggerInWebEnv()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			trigger.TemporarilyAllowSettingCondition("P9_TriggerFiredCountdown");
			trigger.P9_TriggerFiredCountdown = 100;

			Factory.Save();

			AssertEquals("100", trigger.P9_TriggerFiredCountdown.ToString());

			var originalValue = Globals.IsWeb;

			try
			{
				Globals.IsWeb = true;
				using (Globals.SetIsUserInteractiveForTest(false))
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					shipmentWithWorkflow.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
			finally
			{
				Globals.IsWeb = originalValue;
			}

			AssertEquals("EDT events should fire triggers when !IsUserInteractive unless in a service task", (ZShort)99, trigger.P9_TriggerFiredCountdown);
		}

		public void TestEDTEvent_WithSL_FireWorkflow()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.EditedARecord.Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 100;
			Factory.Save();

			shipment.JS_GoodsDescription = "EDIT";
			using (Logs.DeferFiringWorkflow(true))
			{
				Factory.Save();
			}

			AssertEquals("Trigger does not fire when defer firing workflow is true", (ZShort)100, trigger.P9_TriggerFiredCountdown);
			MasterFilesTestHelper.RunLogWalker();
			trigger = new BusinessObjectFactory().Load<ProcessTask>(trigger.PK);
			AssertEquals("EDT events with SL_FireWorkflow = true should fire in LWK", (ZShort)99, trigger.P9_TriggerFiredCountdown);
		}

		public void TestSuppressFiringWorkflow()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 100;
			Factory.Save();

			using (Logs.SuppressFiringWorkflow())
			{
				shipmentWithWorkflow.Logs.AddNew(Events.CustomisableEvent00);
				Factory.Save();
			}

			AssertEquals("Trigger does not fire when suppress firing workflow is true", (ZShort)100, trigger.P9_TriggerFiredCountdown);
			MasterFilesTestHelper.RunLogWalker();
			trigger = new BusinessObjectFactory().Load<ProcessTask>(trigger.PK);
			AssertEquals("Trigger does not fire when suppress firing workflow is true", (ZShort)100, trigger.P9_TriggerFiredCountdown);
		}

		public void TestTriggerFireConditionOnLogSource()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SO101";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;
			var trigger = shipmentAsWorkflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Event.Source.Contains(\"This Shipment\")";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;

			Factory.Save();

			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.US.IJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;

			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, shipmentAsWorkflowProvider.WorkflowItems.Triggers.FirstOrDefault().PK);

			AssertEquals("Initially no WTE events", 0, Factory.Load<StmALog>(query).Length);

			var shipmentAsBizO = ((BusinessObject)shipment);

			var shipmentLog = shipmentAsBizO.GetLogs().AddNew();
			using (shipmentLog.LockForUpdatingKeyFieldsForTesting())
			{
				shipmentLog.SL_EventTime = new ZDateTime(2020, 1, 1);
				shipmentLog.SL_SE_NKEvent = Events.AuthorisedCode;
			}
			Factory.Save();

			AssertEquals("This Shipment SO101", shipmentLog.SL_TableFriendlyName);

			var declarationLog = declaration.GetLogs().AddNew();
			using (declarationLog.LockForUpdatingKeyFieldsForTesting())
			{
				declarationLog.SL_EventTime = new ZDateTime(2020, 1, 1);
				declarationLog.SL_SE_NKEvent = Events.AuthorisedCode;
			}
			Factory.Save();

			AssertEquals("This Declaration SO101", declarationLog.SL_TableFriendlyName);

			AssertEquals("GIVEN Shipment+Declaration with ATH Shipment-trigger AND condition-with-macros for event's table-friendly-name\r\nWHEN ATH event occurred, SHOULD only fire once", 1, Factory.Load<StmALog>(query).Length);
		}

		public void TestMatchesTriggerAction_ForTriggerAction()
		{
			var shipment = (IWorkflowProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var trigger1 = shipment.WorkflowItems.Triggers.AddNew();
			var trigger2 = shipment.WorkflowItems.Triggers.AddNew();
			AssertEquals(true, trigger1.MatchesTriggerAction(trigger2));

			ProcessTaskNotification notification1 = trigger1.ProcessTaskNotifications.AddNew();
			notification1.PQ_P9 = trigger1.PK;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));
			AssertEquals(false, trigger2.MatchesTriggerAction(trigger1));

			ProcessTaskNotification notification2 = trigger2.ProcessTaskNotifications.AddNew();
			notification2.PQ_P9 = trigger2.PK;
			AssertEquals(true, trigger1.MatchesTriggerAction(trigger2));
			AssertEquals(true, trigger2.MatchesTriggerAction(trigger1));

			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.AllPossiblePartyTypes[0].Code;
			notification2.PQ_TriggerParty = MessageRecipientPartyTypeList.AllPossiblePartyTypes[0].Code;
			ZGuid rightDocument = ZGuid.NewZGuid();
			ZGuid wrongDocument = ZGuid.NewZGuid();
			notification1.PQ_SU_Document = rightDocument;
			notification2.PQ_SU_Document = rightDocument;
			AssertEquals(true, trigger1.MatchesTriggerAction(trigger2));

			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));

			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.AllPossiblePartyTypes[1].Code;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));

			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.AllPossiblePartyTypes[0].Code;
			notification1.PQ_SU_Document = wrongDocument;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));

			notification1.PQ_SU_Document = rightDocument;
			AssertEquals(true, trigger1.MatchesTriggerAction(trigger2));

			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertEquals(true, trigger1.MatchesTriggerAction(trigger2));
			notification1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));
			notification2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
			AssertEquals(true, trigger1.MatchesTriggerAction(trigger2));
			notification1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));
			notification2.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertEquals(true, trigger1.MatchesTriggerAction(trigger2));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			notification1.PQ_OH_Recipient = org.PK;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));
			notification2.PQ_OH_Recipient = org.PK;
			AssertEquals(true, trigger1.MatchesTriggerAction(trigger2));

			ProcessTaskNotification notification1a = trigger1.ProcessTaskNotifications.AddNew();
			notification1a.PQ_P9 = trigger1.PK;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));
			AssertEquals(false, trigger2.MatchesTriggerAction(trigger1));

			ProcessTaskNotification notification2a = trigger2.ProcessTaskNotifications.AddNew();
			notification2a.PQ_P9 = trigger2.PK;
			AssertEquals(true, trigger1.MatchesTriggerAction(trigger2));
			AssertEquals(true, trigger2.MatchesTriggerAction(trigger1));
		}

		public void TestMatchesTriggerAction_ForDifferentParents()
		{
			var shipment = (IWorkflowProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var differentShipment = (IWorkflowProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var trigger1 = shipment.WorkflowItems.Triggers.AddNew();
			var trigger2 = differentShipment.WorkflowItems.Triggers.AddNew();

			var notification1 = trigger1.ProcessTaskNotifications.AddNew();
			notification1.PQ_P9 = trigger1.PK;
			var notification2 = trigger2.ProcessTaskNotifications.AddNew();
			notification2.PQ_P9 = trigger2.PK;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));

			notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification1.PQ_SU_Document = ZGuid.NewZGuid();
			notification2.PQ_SU_Document = notification1.PQ_SU_Document;
			AssertEquals(false, trigger1.MatchesTriggerAction(trigger2));
		}

		public void TestMilestonesFireOnceOnly()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			ProcessTaskNotification notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_SU_Document = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery()).PK;
			ZQuery query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, milestone.PK);
			StmALog log = Factory.LoadTop1<StmALog>(query);
			AssertNull("No completed trigger yet until completed", log);

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			log = Factory.LoadTop1<StmALog>(query);
			AssertNotNull("WTE log created even without a save. Can't do OnSaving otherwise there may be ordering issues that cause the trigger not to fire", log);
			AssertEquals("SL_Parent", milestone.PK, log.SL_Parent);
			AssertEquals("SL_Table", ProcessTasksSchema.Constants.TableName, log.SL_Table);
			AssertEquals("SL_SE_NKEvent", Events.WorkflowTriggerEvent.Code, log.SL_SE_NKEvent);
			log.Delete();
			Factory.Save();

			milestone.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(1));
			Factory.Save();
			log = Factory.LoadTop1<StmALog>(query);
			AssertNull("WTE log not created a second time", log);
		}

		public void TestTriggersFireMultipleTimes()
		{
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			ZQuery query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			StmALog log = Factory.LoadTop1<StmALog>(query);
			AssertNull("No completed trigger yet until completed", log);

			dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now.AddDays(1));
			log = Factory.LoadTop1<StmALog>(query);
			AssertNotNull("WTE log created even without a save. Can't do OnSaving otherwise there may be ordering issues that cause the trigger not to fire", log);
			log.Delete();
			Factory.Save();

			dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now.AddDays(2));
			log = Factory.LoadTop1<StmALog>(query);
			AssertNotNull("Trigger log is created a second time if it's a workflow trigger and not a milestone", log);
		}

		public void TestP9_TriggerField_MaxLength()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_SmallDateTime);
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_Bool);
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			AssertEquals("MaxLength is the longest possible trigger field name", DummyBizoSchema.Z0_SmallDateTime.Name.Length, milestone.TriggerConditions.TriggerFieldNameInfo.MaxLength);
		}

		public void TestP9_TriggerField_SettingBeforeParentAvailable()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(JobOrderHeaderSchema.JD_EF_ShipmentPrePlanning);
			DummyProcessTask trigger = Factory.New<DummyProcessTask>();

			AssertNull("A rogue ProcessTask can't possibly support workflow, so it can't have a workflow descriptor yet.", trigger.WorkflowDescriptor);
			trigger.TriggerConditions.TriggerFieldName = JobOrderHeaderSchema.JD_EF_ShipmentPrePlanning.Name; // more than 20 characters, expect no exception

			trigger.P9_ParentID = Dummy.PK;
			AssertEquals("WorkflowDescriptor now available", DummyWorkflowDescriptor.Instance, trigger.WorkflowDescriptor);
			trigger.TriggerConditions.TriggerFieldName = "";
			AssertEquals("Correct MaxLength set", trigger.TriggerConditions.TriggerFieldNameInfo.MaxLength, JobOrderHeaderSchema.JD_EF_ShipmentPrePlanning.Name.Length);
		}

		public void TestWorkflowTriggerEventCreated_FromSettingActualDateManually()
		{
			ProcessTask milestone = Dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			ProcessTaskNotification milestoneNotification = milestone.ProcessTaskNotifications.AddNew();

			ZQuery query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, milestone.PK);
			AssertEquals(0, Factory.Load<StmALog>(query).Length);
			milestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			StmALog[] wteLogs = Factory.Load<StmALog>(query);
			AssertEquals(1, wteLogs.Length);
			AssertEquals(milestone.PK, wteLogs[0].SL_Parent);
			AssertContains("|BNE|BRN", wteLogs[0].SL_Reference);
		}

		public void TestWorkflowTriggerEventCreated_FromEventLog()
		{
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			var milestoneNotification = milestone.ProcessTaskNotifications.AddNew();
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var triggerNotification = trigger.ProcessTaskNotifications.AddNew();

			AssertEquals(0, milestone.GetWTELogs().Length);

			var logValue = new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2008, 1, 1));
			var milestoneSourceLog = Dummy.GetLogs().AddNew(logValue);
			((IBaseTrigger)milestone).SetEventTime(milestoneSourceLog, Dummy, milestoneSourceLog.SL_EventTimeOffset);
			var wteLog = milestone.GetWTELogs().Single();
			AssertEquals(milestone.PK, wteLog.SL_Parent);
			AssertEquals($"V1|{milestoneSourceLog.PK}|BNE|BRN|E|BNE|BRN||", wteLog.SL_Reference);

			var triggerLogValue = new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2008, 1, 2));
			var triggerSourceLog = Dummy.GetLogs().AddNew(triggerLogValue);
			((IBaseTrigger)trigger).SetEventTime(triggerSourceLog, Dummy, triggerSourceLog.SL_EventTimeOffset);

			wteLog = trigger.GetWTELogs().Single();
			AssertEquals(trigger.PK, wteLog.SL_Parent);
			AssertEquals(triggerSourceLog.SL_EventTime, wteLog.SL_EventTime);
			AssertEquals($"V1|{triggerSourceLog.PK}|BNE|BRN|E|BNE|BRN||", wteLog.SL_Reference);
		}

		public void TestWorkflowTriggerEventCreated_DoubleAdd()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;

			Factory.Save();

			var parent = Factory.New<DummyWithWorkflow>();
			parent.Logs.AddNew(Events.CustomisableEvent00);

			var trigger1 = parent.WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification1 = trigger1.ProcessTaskNotifications.AddNew();
			notification1.PQ_TriggerType = "NTF";

			var log1 = Factory.New<StmChangeLog>();
			log1.SY_ParentID = parent.PK;
			log1.SY_ParentTableCode = parent.TablePrefix;
			log1.SY_Changes = "Foo||";

			trigger1.TrySetActualDateForEvent(log1, parent, ZDateTimeOffset.Now);

			AssertNoExceptionThrown(() =>
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					trigger1.TrySetActualDateForEvent(log1, parent, ZDateTimeOffset.Now);
				}
			});
		}
		public void TestTriggerConditionDoesntChange()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();

			string[] eventCodes = new string[] {
				EventReferenceConditionList.Codes.ConditionWithMacros,
				EventReferenceConditionList.Codes.EventReference,
				EventReferenceConditionList.Codes.EventReferenceParameters,
				EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions,
				EventReferenceConditionList.Codes.EventReferenceWithWildcards,
				EventReferenceConditionList.Codes.UserDefined,
			};

			foreach (string codeFrom in eventCodes)
			{
				foreach (string codeTo in eventCodes)
				{
					if (codeFrom != codeTo)
					{
						var trigger = ((IWorkflowProvider)parentBO).WorkflowItems.Triggers.AddNew();
						trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
						trigger.TriggerConditions.TriggerCondition = codeFrom;
						trigger.TriggerConditions.TriggerConditionValue = "test";
						trigger.ProcessTaskNotifications.AddNew();

						Factory.Save();

						trigger.TriggerConditions.TriggerCondition = codeTo;

						AssertEquals($"Trigger Condition {codeFrom} to {codeTo}", "test", trigger.TriggerConditions.TriggerConditionValue);
					}
				}
			}
		}

		public void TestEventSourceIsCorrectInMCREvaluation()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001005";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			var shipmentAsWorkflowProvider = (IWorkflowProvider)shipment;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_GS_NKRepSales = "AA";

			var triggerShipmentESC = shipmentAsWorkflowProvider.WorkflowItems.Triggers.AddNew();
			triggerShipmentESC.P9_Description = "Test Trigger 1";
			triggerShipmentESC.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			triggerShipmentESC.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			triggerShipmentESC.TriggerConditions.TriggerConditionValue = "Event.Source.Contains(\"Shipment\")";

			var triggerShipmentST = shipmentAsWorkflowProvider.WorkflowItems.Triggers.AddNew();
			triggerShipmentST.P9_Description = "Test Trigger 2";
			triggerShipmentST.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02.Code;
			triggerShipmentST.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			triggerShipmentST.TriggerConditions.TriggerConditionValue = "Source.TableName == \"JobShipment\"";

			var triggerInvoicingESC = shipmentAsWorkflowProvider.WorkflowItems.Triggers.AddNew();
			triggerInvoicingESC.P9_Description = "Test Trigger 3";
			triggerInvoicingESC.TriggerConditions.TriggerEventCode = Events.CustomisableEvent03.Code;
			triggerInvoicingESC.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			triggerInvoicingESC.TriggerConditions.TriggerConditionValue = "Event.Source.Contains(\"Invoicing\")";

			var triggerInvoicingST = shipmentAsWorkflowProvider.WorkflowItems.Triggers.AddNew();
			triggerInvoicingST.P9_Description = "Test Trigger 4";
			triggerInvoicingST.TriggerConditions.TriggerEventCode = Events.CustomisableEvent04.Code;
			triggerInvoicingST.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			triggerInvoicingST.TriggerConditions.TriggerConditionValue = "Source.TableName == \"JobHeader\"";

			Factory.Save();

			AssertEquals(100, (int)triggerShipmentESC.P9_TriggerFiredCountdown);
			AssertEquals(100, (int)triggerShipmentST.P9_TriggerFiredCountdown);
			AssertEquals(100, (int)triggerInvoicingESC.P9_TriggerFiredCountdown);
			AssertEquals(100, (int)triggerInvoicingST.P9_TriggerFiredCountdown);

			//triggerShipmentESC
			jobHeader.GetLogs().AddNew(Events.CustomisableEvent01);
			Factory.Save();
			AssertEquals(100, (int)triggerShipmentESC.P9_TriggerFiredCountdown);
			shipmentAsWorkflowProvider.Logs.AddNew(Events.CustomisableEvent01);
			Factory.Save();
			AssertEquals(99, (int)triggerShipmentESC.P9_TriggerFiredCountdown);

			//triggerShipmentST
			jobHeader.GetLogs().AddNew(Events.CustomisableEvent02);
			Factory.Save();
			AssertEquals(100, (int)triggerShipmentST.P9_TriggerFiredCountdown);
			shipmentAsWorkflowProvider.Logs.AddNew(Events.CustomisableEvent02);
			Factory.Save();
			AssertEquals(99, (int)triggerShipmentST.P9_TriggerFiredCountdown);

			//triggerInvoicingESC
			shipmentAsWorkflowProvider.Logs.AddNew(Events.CustomisableEvent03);
			Factory.Save();
			AssertEquals(100, (int)triggerInvoicingESC.P9_TriggerFiredCountdown);
			jobHeader.GetLogs().AddNew(Events.CustomisableEvent03);
			Factory.Save();
			AssertEquals(99, (int)triggerInvoicingESC.P9_TriggerFiredCountdown);

			//triggerInvoicingST
			shipmentAsWorkflowProvider.Logs.AddNew(Events.CustomisableEvent04);
			Factory.Save();
			AssertEquals(100, (int)triggerInvoicingST.P9_TriggerFiredCountdown);
			jobHeader.GetLogs().AddNew(Events.CustomisableEvent04);
			Factory.Save();
			AssertEquals(99, (int)triggerInvoicingST.P9_TriggerFiredCountdown);
		}

		public void TestWorkflowGunDoesntFireDeletedLogs()
		{
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = DummyBizoSchema.Z0_Description.Name;
			action.PQ_FieldValue = "💀";

			ProcessTaskHandler.SetOnFireHookForTest((log, methodName) =>
			{
				if (methodName == "FireTriggers")
				{
					((BusinessObject)log).Delete();
				}
			});

			AssertNoExceptionThrown(() => { Dummy.GetLogs().AddNew(Events.CustomisableEvent00); });
		}

		public void TestClientSideTriggerActionCanRunTwiceIfAllowed()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.PickupCartageCompleteFinalisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "IFC";
			action.PQ_FieldName = "<JS_E_DEP>";
			action.PQ_FieldValue = "<TriggeringEvent.SL_EventTime>";

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.UtcToday;
			AssertEquals("The trigger should've been fired", (short)99, shipmentWithWorkflow.WorkflowItems.Triggers[0].P9_TriggerFiredCountdown);
			AssertEquals("The depature field should have the same date as the first event time", ZDateTime.UtcToday, shipment.JS_E_DEP);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.UtcToday.AddDays(-2);

			AssertEquals("The trigger should not have been fired again", (short)99, shipmentWithWorkflow.WorkflowItems.Triggers[0].P9_TriggerFiredCountdown);
			AssertEquals("The custom field should have the new event time", ZDateTime.UtcToday.AddDays(-2), shipment.JS_E_DEP);
		}

		public void TestClientSideTriggerActionDoesNotRunTwiceIfNotAllowed()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.PickupCartageCompleteFinalisedCode;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "IFC";
			action.PQ_FieldName = "<JS_E_DEP>";
			action.PQ_FieldValue = "<TriggeringEvent.SL_EventTime>";

			var canRunAgainMock = new Mock<ICanTriggerActionRunAgain>();
			canRunAgainMock.Setup(x => x.CanRunAgain(action)).Returns(false);
			ObjectFactory.Substitute(canRunAgainMock.Object);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.UtcToday;
			AssertEquals("The trigger should've been fired", (short)99, shipmentWithWorkflow.WorkflowItems.Triggers[0].P9_TriggerFiredCountdown);
			AssertEquals("The depature field should have the same date as the first event time", ZDateTime.UtcToday, shipment.JS_E_DEP);

			shipment.DocsAndCartage.JP_PickupCartageCompleted = ZDateTime.UtcToday.AddDays(-2);

			AssertEquals("The trigger should not have been fired again", (short)99, shipmentWithWorkflow.WorkflowItems.Triggers[0].P9_TriggerFiredCountdown);
			AssertEquals("The custom field should have the old event time", ZDateTime.UtcToday, shipment.JS_E_DEP);
		}

		#region New Triggers for Old Events

		[TestDate(2015, 7, 14)]
		public void TestApplyTriggerFromTemplate_DefaultRegistryItemBehaviour_ShouldNotFireForEventsWhichHappenedBeforeTriggerCreated()
		{
			var templateFactory = Factory.CreateNewFactory();
			var template = templateFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INQ";
			template.P0_SubType1 = "NAM";

			templateFactory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "NAM";
			job.GetLogs().AddNew(Events.Arrival);

			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Triggers.Count);
			AssertEquals(0, job.WorkflowItems.Milestones.Count);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();

			templateTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateTrigger.P9_Description = "Arrival Trigger";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateMilestone.P9_Description = "Arrival Milestone";

			templateFactory.Save();

			job.HasChanges = true;
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);
			AssertEquals(1, job.WorkflowItems.Milestones.Count);

			AssertEquals("Trigger should not fire when applied from a template trigger created after the event was raised. This could cause documents to get sent out or other bad things to happen as templates change and old jobs are edited.",
				ZDateTime.Empty, job.WorkflowItems.Triggers[0].P9_ActualDate);

			AssertNotEquals("Milestones should still fire since the event has happened (and may not happen again)", ZDateTime.Empty, job.WorkflowItems.Milestones[0].P9_ActualDate);
		}

		[TestDate(2015, 7, 14)]
		public void TestApplyTriggerFromTemplate_DefaultRegistryItemBehaviour_ShouldFireForEventsWhichHappenedAfterTriggerCreatedButBeforeTriggerApplied()
		{
			var templateFactory = Factory.CreateNewFactory();
			var template = templateFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INQ";
			template.P0_SubType1 = "NAM";

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();

			templateTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateTrigger.P9_Description = "Arrival Trigger";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateMilestone.P9_Description = "Arrival Milestone";

			templateFactory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.GetLogs().AddNew(Events.Arrival);

			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Triggers.Count);
			AssertEquals(0, job.WorkflowItems.Milestones.Count);

			job.O1_EnquiryType = "NAM";
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);
			AssertEquals(1, job.WorkflowItems.Milestones.Count);

			AssertNotEquals("Trigger should fire when applied from a template that is now applicable, for an event that happened after the template trigger was created.", ZDateTime.Empty, job.WorkflowItems.Triggers[0].P9_ActualDate);
			AssertNotEquals("Milestones should still fire since the event has happened (and may not happen again)", ZDateTime.Empty, job.WorkflowItems.Milestones[0].P9_ActualDate);
		}

		[TestDate(2015, 7, 14)]
		public void TestApplyTriggerFromTemplate_WhenRegistryItemEnabled_ShouldFireForEventsWhichHappenedBeforeTriggerCreated()
		{
			WorkflowDataRegistry.Instance.AllowTriggersToFireForExistingEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var templateFactory = Factory.CreateNewFactory();
			var template = templateFactory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INQ";
			template.P0_SubType1 = "NAM";

			templateFactory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.O1_EnquiryType = "NAM";
			job.GetLogs().AddNew(Events.Arrival);

			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Triggers.Count);
			AssertEquals(0, job.WorkflowItems.Milestones.Count);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();

			templateTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateTrigger.P9_Description = "Arrival Trigger";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateMilestone.P9_Description = "Arrival Milestone";

			templateFactory.Save();

			job.HasChanges = true;
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);
			AssertEquals(1, job.WorkflowItems.Milestones.Count);

			AssertNotEquals("Trigger should fire when applied from a template trigger created after the event was raised. We've enabled it in the registry so we must know what we're doing.", ZDateTime.Empty, job.WorkflowItems.Triggers[0].P9_ActualDate);
			AssertNotEquals("Milestones should still fire since the event has happened (and may not happen again)", ZDateTime.Empty, job.WorkflowItems.Milestones[0].P9_ActualDate);
		}

		[TestDate(2015, 7, 14)]
		public void TestApplyTriggerFromTemplate_ShouldFireForNewEvents()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INQ";
			template.P0_SubType1 = "NAM";

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();

			templateTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateTrigger.P9_Description = "Arrival Trigger";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateMilestone.P9_Description = "Arrival Milestone";

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			var job = Factory.NewWithValidTestData<SalesEnquiry>();

			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Triggers.Count);
			AssertEquals(0, job.WorkflowItems.Milestones.Count);

			job.GetLogs().AddNew(Events.Arrival);
			job.O1_EnquiryType = "NAM";
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);
			AssertEquals(1, job.WorkflowItems.Milestones.Count);

			AssertNotEquals("Trigger should fire because the event has happened after the template was created", ZDateTime.Empty, job.WorkflowItems.Triggers[0].P9_ActualDate);
			AssertNotEquals("Milestones should still fire since the event has happened (and may not happen again)", ZDateTime.Empty, job.WorkflowItems.Milestones[0].P9_ActualDate);
		}

		[TestDate(2015, 7, 14)]
		public void TestTriggerAlreadyAppliedFromTemplate_ShouldFireForNewEvents()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "INQ";

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			var templateMilestone = template.WorkflowItems.Milestones.AddNew();

			templateTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateTrigger.P9_Description = "Arrival Trigger";
			templateMilestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateMilestone.P9_Description = "Arrival Milestone";

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);

			var job = Factory.NewWithValidTestData<SalesEnquiry>();

			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);
			AssertEquals(1, job.WorkflowItems.Milestones.Count);

			AssertEquals("Event hasn't happened yet", ZDateTime.Empty, job.WorkflowItems.Triggers[0].P9_ActualDate);
			AssertEquals("Event hasn't happened yet", ZDateTime.Empty, job.WorkflowItems.Milestones[0].P9_ActualDate);

			job.GetLogs().AddNew(Events.Arrival);
			Factory.Save();

			AssertNotEquals("Event has happened now so should fire trigger", ZDateTime.Empty, job.WorkflowItems.Triggers[0].P9_ActualDate);
			AssertNotEquals("Event has happened now so should fire milestone", ZDateTime.Empty, job.WorkflowItems.Milestones[0].P9_ActualDate);
		}

		public void TestManuallyCreatedTrigger_ShouldFireForExistingEvents()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.GetLogs().AddNew(Events.Arrival);

			Factory.Save();

			var trigger = job.WorkflowItems.Triggers.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();

			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger.P9_Description = "Arrival Trigger";
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			milestone.P9_Description = "Arrival Milestone";

			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);
			AssertEquals(1, job.WorkflowItems.Milestones.Count);

			AssertNotEquals("Manually created trigger should fire for an event that happened in the past.", ZDateTime.Empty, job.WorkflowItems.Triggers[0].P9_ActualDate);
			AssertNotEquals("Milestones should still fire since the event has happened (and may not happen again)", ZDateTime.Empty, job.WorkflowItems.Milestones[0].P9_ActualDate);
		}

		public void TestManuallyCreatedTrigger_ShouldFireForNewEvents()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();

			Factory.Save();

			var trigger = job.WorkflowItems.Triggers.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();

			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger.P9_Description = "Arrival Trigger";
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			milestone.P9_Description = "Arrival Milestone";

			job.GetLogs().AddNew(Events.Arrival);
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Triggers.Count);
			AssertEquals(1, job.WorkflowItems.Milestones.Count);

			AssertNotEquals("Trigger should fire because the existing event has not been saved yet, so is considered to be happening 'now'", ZDateTime.Empty, job.WorkflowItems.Triggers[0].P9_ActualDate);
			AssertNotEquals("Milestones should still fire since the event has happened (and may not happen again)", ZDateTime.Empty, job.WorkflowItems.Milestones[0].P9_ActualDate);
		}

		#endregion

		#region Fire/Withdraw Trigger

		public void TestFireTrigger()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var line = Factory.New<DummyWithWorkflow>();
			var eventPublisher = Factory.New<DummyWithWorkflow>();

			var evnt = eventPublisher.Logs.AddNew(Events.Arrival);

			var trigger = parent.WorkflowItems.AddNew();

			((IBaseTrigger)trigger).Fire(line, evnt);

			var wteEvent = trigger.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent);
			var expectedReference = new WorkflowTriggerEventData(evnt, line.PK, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code, "E", "BNE", "BRN").ToReference();

			AssertNotNull("WTE event on a trigger", wteEvent);
			AssertEquals("Reference", expectedReference, wteEvent.SL_Reference);
		}

		[TestDate(2017, 07, 02)]
		public void TestSetEstimate()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var eventPublisher = Factory.New<DummyWithWorkflow>();
			var evnt = eventPublisher.Logs.AddNew(Events.Arrival);
			var trigger = parent.WorkflowItems.Milestones.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			((IBaseTrigger)trigger).SetEstimateTime(evnt, ZDateTimeOffset.Now);
			AssertEquals(ZDateTime.Now, trigger.P9_ScheduledDateForBinding.ToZDateTime());
		}

		#endregion

		#region ImmediateFieldChange action

		public void TestFieldIsSetImmediatelyWithImmediateFieldChangeTriggerActionOnTrigger()
		{
			AssertFieldIsSetImmediatelyWithImmediateFieldChangeTriggerAction(collection => collection.Triggers.AddNew());
		}

		public void TestFieldIsSetImmediatelyWithImmediateFieldChangeTriggerActionOnMilestone()
		{
			AssertFieldIsSetImmediatelyWithImmediateFieldChangeTriggerAction(collection => collection.Milestones.AddNew());
		}

		void AssertFieldIsSetImmediatelyWithImmediateFieldChangeTriggerAction(Func<ProcessTaskCollection, ProcessTask> newTriggerFunc)
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_Description.Name;
			var defaultValue = "Some default value to check against";
			dummy.Z0_Description = defaultValue;

			var targetFieldValue = "Set from Trigger Action";

			var trigger = newTriggerFunc(dummy.WorkflowItems);
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			// Only ImmediateFieldChange should be run immediately/synchronously
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;
			action.PQ_FieldValue = targetFieldValue;

			Factory.Save();

			// Field should be default value before trigger action is applied
			AssertEquals(defaultValue, dummy.Z0_Description);
			// Add event with matching event code so that trigger fires and expect action to apply immediately
			dummy.GetLogs().AddNew(AutoEvents.CustomisableEvent01);
			AssertEquals(targetFieldValue, dummy.Z0_Description);
			AssertEquals(false, trigger.HasRowWarnings);
		}

		public void TestImmediateFieldChangeDoesNotTakeEffectOnWithdrawForTriggers()
		{
			AssertImmediateFieldChangeDoesNotTakeEffectOnWithdraw(collection => collection.Triggers.AddNew());
		}

		public void TestImmediateFieldChangeDoesNotTakeEffectOnWithdrawForMilestones()
		{
			AssertImmediateFieldChangeDoesNotTakeEffectOnWithdraw(collection => collection.Milestones.AddNew());
		}

		void AssertImmediateFieldChangeDoesNotTakeEffectOnWithdraw(Func<ProcessTaskCollection, ProcessTask> newTriggerFunc)
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_Description.Name;
			var defaultValue = "Some default value to check against";
			dummy.Z0_Description = defaultValue;

			var trigger = newTriggerFunc(dummy.WorkflowItems);
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			// Only ImmediateFieldChange should be run immediately/synchronously
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;

			var firstValue = "First value...";
			action.PQ_FieldValue = firstValue;
			Factory.Save();

			AssertEquals(defaultValue, dummy.Z0_Description);
			var log = dummy.GetLogs().AddNew(new EventValue(AutoEvents.CustomisableEvent01));
			AssertEquals(firstValue, dummy.Z0_Description);

			var secondValue = "Second value";
			action.PQ_FieldValue = secondValue;
			Factory.Save();

			// Withdraw
			log.Cancel();

			// Value should not be secondValue
			AssertEquals(firstValue, dummy.Z0_Description);
			AssertEquals(false, trigger.HasRowWarnings);
		}

		public void TestHasRowWarningsWillClearIfAddingEventAgain()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_Number.Name;
			var defaultValue = new ZInt(0);
			dummy.Z0_Number = defaultValue;

			var invalidValue = "One Two Three";

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;
			action.PQ_FieldValue = invalidValue;
			Factory.Save();

			AssertEquals(defaultValue, dummy.Z0_Number);

			dummy.GetLogs().AddNew(new EventValue(Events.CustomisableEvent01));

			AssertEquals(defaultValue, dummy.Z0_Number);
			AssertEquals(true, trigger.HasRowWarnings);
			AssertEquals(1, trigger.RowWarnings.Count());
			AssertEquals(@"Failure with 1 reasons:
Converting value [One Two Three] to type CargoWise.Types.ZInt returned null. Target Object: 'Dummy Business Object Default'. Source: One Two Three, Target: ",
trigger.RowWarnings.First().Message);

			// Change action to have a valid value now - warning should disappear after re-applying the action
			action.PQ_FieldValue = "123";
			Factory.Save();

			AssertEquals(defaultValue, dummy.Z0_Number);

			dummy.GetLogs().AddNew(new EventValue(Events.CustomisableEvent01));

			AssertEquals(123, dummy.Z0_Number);
			AssertEquals(false, trigger.HasRowWarnings);
		}

		public void TestTriggerHasRowWarningsWhenUnableToSetFieldUsingImmediateFieldChange()
		{
			AssertHasRowWarningsWhenUnableToSetFieldUsingImmediateFieldChange(collection => collection.Triggers.AddNew());
		}

		public void TestMilestoneHasRowWarningsWhenUnableToSetFieldUsingImmediateFieldChange()
		{
			AssertHasRowWarningsWhenUnableToSetFieldUsingImmediateFieldChange(collection => collection.Milestones.AddNew());
		}

		void AssertHasRowWarningsWhenUnableToSetFieldUsingImmediateFieldChange(Func<ProcessTaskCollection, ProcessTask> newTriggerFunc)
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_Number.Name;
			var defaultValue = new ZInt(0);
			dummy.Z0_Number = defaultValue;

			var invalidValue = "One Two Three";

			var trigger = newTriggerFunc(dummy.WorkflowItems);
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;
			action.PQ_FieldValue = invalidValue;

			Factory.Save();

			AssertEquals(defaultValue, dummy.Z0_Number);

			var log = dummy.GetLogs().AddNew(new EventValue(Events.CustomisableEvent01));

			AssertEquals(defaultValue, dummy.Z0_Number);
			AssertEquals(true, trigger.HasRowWarnings);
			AssertEquals(1, trigger.RowWarnings.Count());
			AssertEquals(@"Failure with 1 reasons:
Converting value [One Two Three] to type CargoWise.Types.ZInt returned null. Target Object: 'Dummy Business Object Default'. Source: One Two Three, Target: ",
trigger.RowWarnings.First().Message);

			// Change action to have a valid value now - warning should disappear after re-applying the action
			action.PQ_FieldValue = "123";

			// Cancel and add event again to retrigger action
			log.Cancel();
			dummy.HasChanges = true;

			AssertEquals(defaultValue, dummy.Z0_Number);

			dummy.GetLogs().AddNew(new EventValue(Events.CustomisableEvent01));

			AssertEquals(123, dummy.Z0_Number);
			AssertEquals(false, trigger.HasRowWarnings);
		}

		public void TestSettingOfMilestoneActualDateTriggersFieldAction()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_Description.Name;
			var defaultValue = "Some default value to start with";
			dummy.Z0_Description = defaultValue;

			var targetValue = "One Two Three";

			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Trigger 1";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;
			action.PQ_FieldValue = targetValue;

			Factory.Save();

			AssertEquals(defaultValue, dummy.Z0_Description);

			var validDt = new ZDateTimeOffset(ZDateTime.Now.AddMinutes(-1));
			milestone.P9_ActualDateForBinding = validDt;

			AssertEquals(targetValue, dummy.Z0_Description);
			AssertEquals(false, milestone.HasRowWarnings);
		}

		public void TestApplicationOfImmediateFieldChangeAfterResettingMilestoneDate()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_Number.Name;
			var defaultValue = new ZInt(0);
			dummy.Z0_Number = defaultValue;

			var invalidValue = "One Two Three";

			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Trigger 1";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;
			action.PQ_FieldValue = invalidValue;

			Factory.Save();

			AssertEquals(defaultValue, dummy.Z0_Number);

			milestone.P9_ActualDateForBinding = ZDateTime.Now.AddMinutes(-1).ToDateTime();

			AssertEquals(defaultValue, dummy.Z0_Number);
			AssertEquals(true, milestone.HasRowWarnings);
			AssertEquals(1, milestone.RowWarnings.Count());
			AssertEquals(@"Failure with 1 reasons:
Converting value [One Two Three] to type CargoWise.Types.ZInt returned null. Target Object: 'Dummy Business Object Default'. Source: One Two Three, Target: ",
milestone.RowWarnings.First().Message);

			// Change action to have a valid value now - warning should disappear after re-applying the action
			action.PQ_FieldValue = "123";

			// Cancel and add event again to retrigger action
			milestone.P9_ActualDateForBinding = default;

			AssertEquals(defaultValue, dummy.Z0_Number);

			milestone.P9_ActualDateForBinding = ZDateTime.Now.AddMinutes(-1).ToDateTime();

			AssertEquals(123, dummy.Z0_Number);
			AssertEquals(false, milestone.HasRowWarnings);
		}

		public void TestAddingImmediateFieldChangeActionAfterMilestoneActualDateWillStillApply()
		{
			/// This test case covers the following scenario:
			///		1) New Milestone is created. At this point the Milestone will have no Trigger Actions
			///		2) Set the Actual Date on the Milestone, effectively "triggering" it
			///		3) Add an IFC (Immediate Field Change) Trigger Action to the Milestone
			///		4) Save the business object. Expect that the Trigger Action in 3) to apply
			/// Note that this is an edge case - we usually don't expect users to add Trigger Actions to Milestones *after* setting the Actual Date.
			/// However, the existing implementation for other Trigger Actions (eg: 'Set Field' FLD) will apply any Trigger
			/// Actions added *after* the Actual Date.
			/// So, we make sure that the behaviour is consistent. The consistency will allow us to fix this together in future.

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_Description.Name;
			var defaultValue = "Some default value to start with";
			dummy.Z0_Description = defaultValue;

			var targetValue = "One Two Three";

			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Trigger 1";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;

			// Don't call Factory.Save() before setting P9_ActualDateForBinding, as we expect the trigger
			// action below to only apply when we save
			var validDt = new ZDateTimeOffset(ZDateTime.Now.AddMinutes(-1));
			milestone.P9_ActualDateForBinding = validDt;

			AssertEquals(defaultValue, dummy.Z0_Description);

			var action = milestone.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;
			action.PQ_FieldValue = targetValue;

			AssertEquals(defaultValue, dummy.Z0_Description);

			// Trigger action should apply upon saving
			Factory.Save();

			AssertEquals("It doesn't get set.", defaultValue, dummy.Z0_Description);
			AssertEquals(false, milestone.HasRowWarnings);
		}

		public void TestAddingTheSameEventAsAResultOfImmediateFieldChangeDoesNotSetFieldMultipleTimesForTriggers()
		{
			AssertAddingTheSameEventAsAResultOfImmediateFieldChangeDoesNotSetFieldMultipleTimes(collection => collection.Triggers.AddNew());
		}

		public void TestAddingTheSameEventAsAResultOfImmediateFieldChangeDoesNotSetFieldMultipleTimesForMilestones()
		{
			AssertAddingTheSameEventAsAResultOfImmediateFieldChangeDoesNotSetFieldMultipleTimes(collection => collection.Milestones.AddNew());
		}

		void AssertAddingTheSameEventAsAResultOfImmediateFieldChangeDoesNotSetFieldMultipleTimes(Func<ProcessTaskCollection, ProcessTask> newTriggerFunc)
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var fieldName = DummyBizoSchema.Z0_Number.Name;
			var defaultValue = new ZInt(0);
			dummy.Z0_Number = defaultValue;

			var trigger = newTriggerFunc(dummy.WorkflowItems);
			trigger.P9_Description = "Trigger 1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = fieldName;
			action.PQ_FieldValue = "<Add(\"<Z0_Number>\",\"1\")>";

			Factory.Save();

			// Simulate adding an event as a result of the field being set
			var changeCount = 0;
			ZPropertyInfo propertyInfo = dummy.FindPropertyInfo(fieldName);
			propertyInfo.ValueChanged += (o, e) =>
			{
				changeCount++;
				// Adding event each time will fire the trigger again
				//		Add event 01 -> Set field -> Add event 01 -> Set field...
				// Expect recursion protection to catch it
				dummy.GetLogs().AddNew(new EventValue(Events.CustomisableEvent01));
			};

			AssertEquals(defaultValue, dummy.Z0_Number);

			AssertNoExceptionThrown(() =>
			{
				// Start the chain of events
				dummy.GetLogs().AddNew(new EventValue(Events.CustomisableEvent01));
			});

			// Expect the field to be set once only
			AssertEquals(1, changeCount);
			AssertEquals(1, dummy.Z0_Number);
			AssertEquals(false, trigger.HasWarnings);
		}

		public void TestTwoImmediateFieldChangesInACycleWillTerminateForTriggers()
		{
			AssertTwoImmediateFieldChangesInACycleWillTerminate(collection => collection.Triggers.AddNew());
		}

		public void TestTwoImmediateFieldChangesInACycleWillTerminateForMilestones()
		{
			AssertTwoImmediateFieldChangesInACycleWillTerminate(collection => collection.Milestones.AddNew());
		}

		void AssertTwoImmediateFieldChangesInACycleWillTerminate(Func<ProcessTaskCollection, ProcessTask> newTriggerFunc)
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			// Set up field 1 and related triggers
			var field1 = DummyBizoSchema.Z0_Number.Name;
			var field1Default = new ZInt(0);
			dummy.Z0_Number = field1Default;

			var trigger1 = newTriggerFunc(dummy.WorkflowItems);
			trigger1.P9_Description = "Trigger 1";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = field1;
			// Increment the value each time so that the field value reflects how many times it was set by the action
			action1.PQ_FieldValue = "<Add(\"<Z0_Number>\",\"1\")>";

			// Set up field 2 and related triggers
			var field2 = DummyBizoSchema.Z0_Money.Name;
			var field2Default = new ZDecimal(0);
			dummy.Z0_Money = field2Default;

			var trigger2 = newTriggerFunc(dummy.WorkflowItems);
			trigger2.P9_Description = "Trigger 2";
			trigger2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent02.Code;
			var action2 = trigger2.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2.PQ_FieldName = field2;
			// Increment the value each time so that the field value reflects how many times it was set by the action
			action2.PQ_FieldValue = "<Add(\"<Z0_Money>\",\"1\")>";

			Factory.Save();

			// Simulate adding an event as a result of the field being set
			var field1ChangeCount = 0;
			var latestField1 = field1Default;
			ZPropertyInfo field1PI = dummy.FindPropertyInfo(field1);
			field1PI.ValueChanged += (o, e) =>
			{
				field1ChangeCount++;
				latestField1 = dummy.Z0_Number;
				// Fire Event02 to trigger action2
				dummy.GetLogs().AddNew(new EventValue(Events.CustomisableEvent02));
			};

			var field2ChangeCount = 0;
			var latestField2 = field2Default;
			ZPropertyInfo field2PI = dummy.FindPropertyInfo(field2);
			field2PI.ValueChanged += (o, e) =>
			{
				field2ChangeCount++;
				latestField2 = dummy.Z0_Money;
				// Fire Event01 to trigger action1
				dummy.GetLogs().AddNew(new EventValue(Events.CustomisableEvent01));
			};

			AssertEquals(field1Default, dummy.Z0_Number);
			AssertEquals(field2Default, dummy.Z0_Money);

			AssertNoExceptionThrown(() =>
			{
				// Start the chain of events
				dummy.GetLogs().AddNew(new EventValue(AutoEvents.CustomisableEvent01));
			});

			// Expect each field to be set once only
			AssertEquals(1, field1ChangeCount);
			AssertNotEquals(field1Default, dummy.Z0_Number);
			AssertEquals(1, dummy.Z0_Number);

			AssertEquals(1, field2ChangeCount);
			AssertNotEquals(field2Default, dummy.Z0_Money);
			AssertEquals(1m, dummy.Z0_Money);
		}

		public void TestSettingActualDateOnMilestoneWithIFCCreatesWTE()
		{
			TestSettingActualDateOnMilestoneCreatesWTE(WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange);
		}

		public void TestSettingActualDateOnMilestoneWithFLDCreatesWTE()
		{
			TestSettingActualDateOnMilestoneCreatesWTE(WorkflowTriggerActionTypeConstants.Codes.SetField);
		}

		public void TestSettingActualDateOnMilestoneCreatesWTE(ZString triggerType)
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;
			shipment.JS_UniqueConsignRef = "S00000100";

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "test trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = triggerType;
			action.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Description>\"==\"test milestone\"&&\"<IsMilestone>\"==\"Y\").P9_ActualDate>";
			action.PQ_FieldValue = "<WorkflowItems.Find(\"{P9_Description}\"==\"test trigger\"&&\"{IsWorkflowTrigger}\"==\"Y\").P9_ActualDate>";

			var milestone = shipmentWithWorkflow.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "test milestone";
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			milestone.TriggerConditions.TriggerConditionValue = "false";
			using (milestone.TemporarilyAllowSettingCondition("P9_TriggerFiredCountdown"))
			{
				milestone.P9_TriggerFiredCountdown = 100;
			}

			Factory.Save();

			var log = shipmentWithWorkflow.Logs.AddNew(Events.CustomisableEvent00, DateTime.Now.AddHours(-3));

			Factory.Save();

			var logwalker = MasterFilesTestHelper.RunLogWalker();
			milestone.Reload();

			CombineAssertions(() =>
			{
				Assert("milestone date should be set", milestone.P9_ActualDate.IsValid);
				AssertEquals((ZShort)99, milestone.P9_TriggerFiredCountdown);
				AssertNoExceptionThrown("Milstone has WTE log", () => milestone.GetWTELogs().Single());
			});
		}

		#endregion

		#endregion

		#region Calendar Reminders

		string ExpectedSubjectPrefix
		{
			get { return ""; }
		}

		string ExpectedExtraBodyDetails
		{
			get { return ""; }
		}

		string ExpectedExtraBodyPrefix
		{
			get { return ""; }
		}

		ProcessTask GetTaskForCalendarReminderTesting(OrgHeader org)
		{
			return Factory.New<ProcessTask>();
		}

		public void TestCalendarReminder()
		{
			RefTimeZoneSet timeZonesMumbai = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZonesMumbai.StandardZone.R2_CivilianTimeZoneCode = "IN1";
			timeZonesMumbai.StandardZone.R2_OffsetMinutesFromUTC = 300;

			RefTimeZoneSet timeZonesPerth = Factory.NewWithValidTestData<RefTimeZoneSet>();
			timeZonesPerth.StandardZone.R2_CivilianTimeZoneCode = "PER";
			timeZonesPerth.StandardZone.R2_OffsetMinutesFromUTC = 180;

			RefUNLOCO mumbai = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INBOM");
			mumbai.RL_R3 = timeZonesMumbai.PK;

			RefUNLOCO perth = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUPER");
			perth.RL_R3 = timeZonesPerth.PK;

			Factory.Save();

			GlbStaff zubin = Factory.NewWithValidTestData<GlbStaff>();
			zubin.GS_FullName = "Zubin Appoo";

			GlbStaff dexter = Factory.NewWithValidTestData<GlbStaff>();
			dexter.GS_FullName = "Dexter Morgan";

			GlbStaff debra = Factory.NewWithValidTestData<GlbStaff>();
			debra.GS_FullName = "Debra Morgan";
			debra.GS_EmailAddress = "debra@example.com";

			GlbGroup morganFamily = Factory.NewWithValidTestData<GlbGroup>();
			morganFamily.Staff.Add(dexter);
			morganFamily.Staff.Add(debra);
			morganFamily.Staff.Add(zubin);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Bits & Bobs";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";

			ProcessTask task = GetTaskForCalendarReminderTesting(org);
			task.P9_TaskID = "987";
			task.P9_Description = "Ring John <at home>";
			task.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GS_NKAssignedStaffMember = zubin.GS_Code;

			PrepareTaskForSendingReminder(task);
			Assert(task.P9_IsCalendarItem);

			AssertEquals("No Calendar Reminder Exists as no Date set", false, task.ShouldCreateTaskReminder);

			task.P9_ScheduledDate = new ZDateTime(2005, 11, 26, 9, 0, 0);
			task.P9_EstDuration = new ZDateTime(2007, 1, 1, 3, 23, 0);
			AssertEquals("Calendar Reminder not sent - No Email Address Specified", false, task.ShouldCreateTaskReminder);

			zubin.GS_EmailAddress = "test@example.com";
			task.P9_GG_AssignedGroup = morganFamily.PK;

			Factory.Save();

			Reminder rem = task.TaskReminder;
			AssertEquals("1 recipient - Zubin", 1, rem.Recipients.Count);
			AssertEquals("1 recipient - Zubin", "Zubin Appoo", rem.Recipients[0].Name);
			AssertEquals("1 recipient - Zubin", "test@example.com", rem.Recipients[0].Email);
			AssertEquals("Correct To Date", new ZDateTime(2005, 11, 26, 12, 23, 0), rem.LocalDateTo);

			AssertEquals("Correct From Date", task.P9_ScheduledDate, rem.LocalDateFrom);
			AssertEquals("Correct UTC From Date - using the specified time zone from the org", task.P9_ScheduledDate.AddHours(-5), rem.UTCDateFrom);
			AssertEquals("Correct Location", org.MainAddress.AddressAsASingleLine, rem.Location);

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Leanne";
			contact1.OC_Phone = "(02) 8888 8888";

			task.P9_OC = contact1.PK;
			task.Address.OA_RL_NKRelatedPortCode = perth.RL_Code;

			rem = task.TaskReminder;

			AssertEquals("Correct From Date", task.P9_ScheduledDate, rem.LocalDateFrom);
			AssertEquals("Correct Location", org.MainAddress.AddressAsASingleLine, rem.Location);
			AssertEquals("Correct UTC From Date - using the specified time zone from the address", task.P9_ScheduledDate.AddHours(-3), rem.UTCDateFrom);

			contact1.OC_Phone = ZString.Empty;
			rem = task.TaskReminder;

			AssertEquals("Correct From Date", task.P9_ScheduledDate, rem.LocalDateFrom);
			AssertEquals("Correct Location", org.MainAddress.AddressAsASingleLine, rem.Location);

			task.P9_Type = "CRP";
			Assert("Remains as it was for invalid items", task.P9_IsCalendarItem);

			task.P9_Type = TaskTypeForNotSendingAppointment;
			Assert(!task.P9_IsCalendarItem);

			AssertEquals("Task type is one that does not send appointments, no reminders should be set", false, task.ShouldCreateTaskReminder);

			task.P9_IsCalendarItem = true;
			AssertEquals("Task should send appointment", true, task.ShouldCreateTaskReminder);

			task.P9_IsCalendarItem = false;
			Factory.Save();
			AssertEquals("Task should not have any appointments", false, task.ShouldCreateTaskReminder);

			task.P9_IsCalendarItem = true;
			AssertEquals("Task should have 1 appointment as the Calendar flag was ticked", true, task.ShouldCreateTaskReminder);
		}

		public void TestCalendarReminderSubject()
		{
			GlbStaff zubin = Factory.NewWithValidTestData<GlbStaff>();
			zubin.GS_FullName = "Zubin Appoo";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Bits & Bobs";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";

			ProcessTask task = GetTaskForCalendarReminderTesting(org);
			task.P9_TaskID = "987";
			task.P9_Description = "Ring John <at home>";
			task.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GS_NKAssignedStaffMember = zubin.GS_Code;

			PrepareTaskForSendingReminder(task);
			Assert(task.P9_IsCalendarItem);

			task.P9_ScheduledDate = new ZDateTime(2005, 11, 26, 9, 0, 0);
			task.P9_EstDuration = new ZDateTime(2007, 1, 1, 3, 23, 0);
			zubin.GS_EmailAddress = "test@example.com";

			Factory.Save();

			Reminder rem = task.TaskReminder;
			string expectedPrefix = !string.IsNullOrEmpty(ExpectedSubjectPrefix) ? ExpectedSubjectPrefix + " - " : "";
			AssertEquals("Correct Subject", expectedPrefix + "Bits & Bobs - (02) 7777 8888 (Task " + task.P9_TaskID + ")", rem.Subject);

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Leanne";
			contact1.OC_Phone = "(02) 8888 8888";
			task.P9_OC = contact1.PK;
			rem = task.TaskReminder;
			AssertEquals("Correct Subject", expectedPrefix + "Bits & Bobs - Leanne, (02) 8888 8888 (Task " + task.P9_TaskID + ")", rem.Subject);
		}

		public void TestCalendarReminderBody()
		{
			GlbStaff zubin = Factory.NewWithValidTestData<GlbStaff>();
			zubin.GS_FullName = "Zubin Appoo";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Bits & Bobs";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";

			ProcessTask task = GetTaskForCalendarReminderTesting(org);
			task.P9_TaskID = "987";
			task.P9_Description = "Ring John <at home>";
			task.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GS_NKAssignedStaffMember = zubin.GS_Code;

			PrepareTaskForSendingReminder(task);
			Assert(task.P9_IsCalendarItem);

			task.P9_ScheduledDate = new ZDateTime(2005, 11, 26, 9, 0, 0);
			task.P9_EstDuration = new ZDateTime(2007, 1, 1, 3, 23, 0);
			zubin.GS_EmailAddress = "test@example.com";

			Factory.Save();

			Reminder rem = task.TaskReminder;

			string taskIdLine = $"Task ID: {task.P9_TaskID}";
			string taskUrlLine = "Task ID: " + string.Format(@"<a href=""{0}"">{1}</a>",
					ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.ProcessTasks, task.PK.ToGuid()),
					task.P9_TaskID);

			string expectedExtraBodyPrefixWithNewLine = !string.IsNullOrEmpty(ExpectedExtraBodyPrefix) ? ExpectedExtraBodyPrefix + System.Environment.NewLine : "";
			string body = expectedExtraBodyPrefixWithNewLine
				+ "Organization: Bits & Bobs"
				+ System.Environment.NewLine + $"Phone: (02) 7777 8888"
				+ System.Environment.NewLine + $"Task Address: {org.MainAddress.AddressAsASingleLine}" + System.Environment.NewLine
				+ System.Environment.NewLine + "Task Type: fdsfsdf"
				+ ExpectedExtraBodyDetails
				+ System.Environment.NewLine + "Task Description: Ring John <at home>" + System.Environment.NewLine
				+ taskIdLine + System.Environment.NewLine
				+ "Task Notes: Hello this is a test message." + System.Environment.NewLine;

			string expectedHtmlBody = string.Format("<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{0}</BODY></HTML>", WebUtility.HtmlEncode(body).Replace(taskIdLine, taskUrlLine));

			AssertEquals("Correct Body", body, rem.Body);
			AssertEquals("Correct HTML body", expectedHtmlBody, rem.HtmlBody);

			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Leanne";
			contact1.OC_Phone = "(02) 8888 8888";
			task.P9_OC = contact1.PK;
			rem = task.TaskReminder;

			body = expectedExtraBodyPrefixWithNewLine
				+ string.Format("Organization: {0}", "Bits & Bobs")
				+ System.Environment.NewLine + string.Format("Contact: {0}", "Leanne")
				+ System.Environment.NewLine + string.Format("Phone: {0}", "(02) 8888 8888")
				+ System.Environment.NewLine + $"Task Address: {org.MainAddress.AddressAsASingleLine}"
				+ System.Environment.NewLine
				+ System.Environment.NewLine + string.Format("Task Type: {0}", "fdsfsdf")
				+ ExpectedExtraBodyDetails
				+ System.Environment.NewLine + string.Format("Task Description: {0}", "Ring John <at home>")
				+ System.Environment.NewLine + taskIdLine + System.Environment.NewLine
				+ string.Format("Task Notes: {0}", "Hello this is a test message.") + System.Environment.NewLine;
			expectedHtmlBody = string.Format("<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{0}</BODY></HTML>", WebUtility.HtmlEncode(body).Replace(taskIdLine, taskUrlLine));

			AssertEquals("Correct Body", body, rem.Body);
			AssertEquals("Correct HTML body", expectedHtmlBody, rem.HtmlBody);

			contact1.OC_Phone = ZString.Empty;
			rem = task.TaskReminder;

			body = expectedExtraBodyPrefixWithNewLine
				+ string.Format("Organization: {0}", "Bits & Bobs")
				+ System.Environment.NewLine + string.Format("Contact: {0}", "Leanne")
				+ System.Environment.NewLine + string.Format("Company Phone: {0}", "(02) 7777 8888")
				+ System.Environment.NewLine + $"Task Address: {org.MainAddress.AddressAsASingleLine}"
				+ System.Environment.NewLine
				+ System.Environment.NewLine + string.Format("Task Type: {0}", "fdsfsdf")
				+ ExpectedExtraBodyDetails
				+ System.Environment.NewLine + string.Format("Task Description: {0}", "Ring John <at home>")
				+ System.Environment.NewLine + taskIdLine
				+ System.Environment.NewLine + string.Format("Task Notes: {0}", "Hello this is a test message.") + System.Environment.NewLine;
			expectedHtmlBody = string.Format("<HTML><HEAD><TITLE></TITLE></HEAD><BODY>{0}</BODY></HTML>", WebUtility.HtmlEncode(body).Replace(taskIdLine, taskUrlLine));

			AssertEquals("Correct Body", body, rem.Body);
			AssertEquals("Correct HTML body", expectedHtmlBody, rem.HtmlBody);
		}

		public void TestCalendarReminderStaffAssigned()
		{
			GlbStaff zubin = Factory.NewWithValidTestData<GlbStaff>();
			zubin.GS_FullName = "Zubin Appoo";
			zubin.GS_EmailAddress = "test@example.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "My Organisation in Newington";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";

			ProcessTask task = GetTaskForCalendarReminderTesting(org);
			task.P9_TaskID = "987";
			task.P9_Description = "Ring John";
			task.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GS_NKAssignedStaffMember = zubin.GS_Code;

			Factory.Save();

			Reminder rem = task.TaskReminder;
			AssertEquals("1 recipient - Zubin", 1, rem.Recipients.Count);
			AssertEquals("1 recipient - Zubin", "Zubin Appoo", rem.Recipients[0].Name);
			AssertEquals("1 recipient - Zubin", "test@example.com", rem.Recipients[0].Email);
		}

		public void TestCalendarReminderGroupAssigned()
		{
			GlbStaff dexter = Factory.NewWithValidTestData<GlbStaff>();
			dexter.GS_FullName = "Dexter Morgan";

			GlbStaff debra = Factory.NewWithValidTestData<GlbStaff>();
			debra.GS_FullName = "Debra Morgan";
			debra.GS_EmailAddress = "debra@example.com";

			GlbStaff harrison = Factory.NewWithValidTestData<GlbStaff>();
			harrison.GS_FullName = "Harrison Morgan";
			harrison.GS_EmailAddress = "harrison@example.com";

			GlbGroup morganFamily = Factory.NewWithValidTestData<GlbGroup>();
			morganFamily.Staff.Add(dexter);
			morganFamily.Staff.Add(debra);
			morganFamily.Staff.Add(harrison);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "My Organisation in Newington";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";

			ProcessTask task = GetTaskForCalendarReminderTesting(org);
			task.P9_TaskID = "987";
			task.P9_Description = "Ring John";
			task.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GG_AssignedGroup = morganFamily.PK;

			Factory.Save();

			Reminder rem = task.TaskReminder;
			AssertEquals("2 recipients - Debra and Harrison have an email", 2, rem.Recipients.Count);
			AssertEquals("2 recipients - Debra", "Debra Morgan", rem.Recipients[0].Name);
			AssertEquals("2 recipients - Debra", "debra@example.com", rem.Recipients[0].Email);
			AssertEquals("2 recipients - Harrison", "Harrison Morgan", rem.Recipients[1].Name);
			AssertEquals("2 recipients - Harrison", "harrison@example.com", rem.Recipients[1].Email);
		}

		public void TestCalendarReminderStaffAndGroupAssigned()
		{
			GlbStaff zubin = Factory.NewWithValidTestData<GlbStaff>();
			zubin.GS_FullName = "Zubin Appoo";
			zubin.GS_EmailAddress = "test@example.com";

			GlbStaff dexter = Factory.NewWithValidTestData<GlbStaff>();
			dexter.GS_FullName = "Dexter Morgan";

			GlbStaff debra = Factory.NewWithValidTestData<GlbStaff>();
			debra.GS_FullName = "Debra Morgan";
			debra.GS_EmailAddress = "debra@example.com";

			GlbGroup morganFamily = Factory.NewWithValidTestData<GlbGroup>();
			morganFamily.Staff.Add(dexter);
			morganFamily.Staff.Add(debra);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "My Organisation in Newington";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";

			ProcessTask task = GetTaskForCalendarReminderTesting(org);
			task.P9_TaskID = "987";
			task.P9_Description = "Ring John";
			task.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GS_NKAssignedStaffMember = zubin.GS_Code;
			task.P9_GG_AssignedGroup = morganFamily.PK;

			Factory.Save();

			Reminder rem = task.TaskReminder;
			AssertEquals("1 recipient - Zubin", 1, rem.Recipients.Count);
			AssertEquals("1 recipient - Zubin", "Zubin Appoo", rem.Recipients[0].Name);
			AssertEquals("1 recipient - Zubin", "test@example.com", rem.Recipients[0].Email);
		}

		public void TestCalendarReminderStaffWithoutEmailAndGroupAssigned()
		{
			GlbStaff zubin = Factory.NewWithValidTestData<GlbStaff>();
			zubin.GS_FullName = "Zubin Appoo";

			GlbStaff dexter = Factory.NewWithValidTestData<GlbStaff>();
			dexter.GS_FullName = "Dexter Morgan";

			GlbStaff debra = Factory.NewWithValidTestData<GlbStaff>();
			debra.GS_FullName = "Debra Morgan";
			debra.GS_EmailAddress = "debra@example.com";

			GlbStaff harrison = Factory.NewWithValidTestData<GlbStaff>();
			harrison.GS_FullName = "Harrison Morgan";
			harrison.GS_EmailAddress = "harrison@example.com";

			GlbGroup morganFamily = Factory.NewWithValidTestData<GlbGroup>();
			morganFamily.Staff.Add(dexter);
			morganFamily.Staff.Add(debra);
			morganFamily.Staff.Add(harrison);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "My Organisation in Newington";
			org.MainAddress.OA_Address1 = "88 Bob st";
			org.MainAddress.OA_Phone = "(02) 7777 8888";
			org.OH_RL_NKClosestPort = "INBOM";

			ProcessTask task = GetTaskForCalendarReminderTesting(org);
			task.P9_TaskID = "987";
			task.P9_Description = "Ring John";
			task.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("Hello this is a test message."));
			task.P9_OA = org.MainAddress.PK;
			task.P9_GS_NKAssignedStaffMember = zubin.GS_Code;
			task.P9_GG_AssignedGroup = morganFamily.PK;

			Factory.Save();

			Reminder rem = task.TaskReminder;
			AssertEquals("2 recipients - Debra and Harrison have an email", 2, rem.Recipients.Count);
			AssertEquals("2 recipients - Debra", "Debra Morgan", rem.Recipients[0].Name);
			AssertEquals("2 recipients - Debra", "debra@example.com", rem.Recipients[0].Email);
			AssertEquals("2 recipients - Harrison", "Harrison Morgan", rem.Recipients[1].Name);
			AssertEquals("2 recipients - Harrison", "harrison@example.com", rem.Recipients[1].Email);
		}

		public void TestReminderSubjectWithJobNumber()
		{
			BusinessObject quote = Factory.New(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType);
			quote["TH_QuoteNumber"] = "TH000110001";
			IWorkflowProvider businessObjectWithworkflow = quote as IWorkflowProvider;
			AssertNotNull(businessObjectWithworkflow);
			ProcessTask processTask = businessObjectWithworkflow.WorkflowItems.AddNew();
			GlbStaff staffMember = Factory.New<GlbStaff>();
			staffMember.GS_Code = "MB";
			processTask.P9_GS_NKAssignedStaffMember = staffMember.GS_Code;
			AssertContains("Quotation - TH000110001", processTask.TaskReminder.Subject);
		}

		static internal void PrepareTaskForSendingReminder(params ProcessTask[] tasks)
		{
			AddCodeDescriptionBoolToTaskTypes(tasks[0].Parent != null ? (string)(tasks[0].Parent.WorkflowType) : "OPP");
			foreach (var task in tasks)
			{
				task.Lookups.RefreshTypeList();
				task.P9_Type = TaskTypeForSendingAppointment;
			}
		}

		static void AddCodeDescriptionBoolToTaskTypes(string parentCode)
		{
			CategorisedWorkflowTaskTypesCollection collection = new CategorisedWorkflowTaskTypesCollection();

			CategorisedWorkflowTaskTypes parent = collection.AddNew();
			parent.Code = parentCode;

			WorkflowTaskType setsAppointment = parent.TaskTypes.AddNew();
			setsAppointment.Code = TaskTypeForSendingAppointment;
			setsAppointment.Description = (NoResString)"fdsfsdf";
			setsAppointment.Bool = true;

			WorkflowTaskType doesntSetAppointment = parent.TaskTypes.AddNew();
			doesntSetAppointment.Code = TaskTypeForNotSendingAppointment;
			doesntSetAppointment.Description = (NoResString)"sdfsdf";
			doesntSetAppointment.Bool = false;

			CategorisedWorkflowTaskTypes parent2 = collection.AddNew();
			parent2.Code = "STA";

			WorkflowTaskType setsAppointment2 = parent2.TaskTypes.AddNew();
			setsAppointment2.Code = TaskTypeForSendingAppointment;
			setsAppointment2.Description = (NoResString)"fdsfsdf";
			setsAppointment2.Bool = true;

			WorkflowTaskType doesntSetAppointment2 = parent2.TaskTypes.AddNew();
			doesntSetAppointment2.Code = TaskTypeForNotSendingAppointment;
			doesntSetAppointment2.Description = (NoResString)"sdfsdf";
			doesntSetAppointment2.Bool = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		const string TaskTypeForNotSendingAppointment = "qwz";
		const string TaskTypeForSendingAppointment = "zxc";

		#endregion

		#region Status

		[TestDate(1994, 01, 01)]
		public void TestInvalidP9_Status()
		{
			DummyProcessTask task = Factory.New<DummyProcessTask>();
			task.P9_Status = "WRK";
			task.P9_ActualDuration = ZDateTime.Invalid;
			task.P9_Status = "CLS";
			AssertEquals(true, task.P9_ActualDuration.IsValid);
			AssertEquals(ZDateTime.DefaultDurationEpoch, task.P9_ActualDuration);
		}

		[TestDate(1994, 01, 01)]
		public void TestEmptyP9_Status()
		{
			DummyProcessTask task = Factory.New<DummyProcessTask>();
			task.P9_Status = "WRK";
			task.P9_ActualDuration = ZDateTime.Empty;
			task.P9_Status = "CLS";
			AssertEquals(false, task.P9_ActualDuration.IsEmpty);
			AssertEquals(ZDateTime.DefaultDurationEpoch, task.P9_ActualDuration);
		}

		public void TestStatus()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			AssertEquals(ZDateTime.Empty, task.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, task.P9_ActualDate);

			task.P9_ScheduledDate = ZDateTime.Now.AddDays(3);
			AssertEquals("Pending", task.Status);

			task.P9_ScheduledDate = ZDateTime.Now.AddDays(-3);
			AssertEquals("Overdue", task.Status);

			task.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-5));
			AssertEquals("Completed", task.Status);

			task.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-3));
			AssertEquals("Completed", task.Status);

			task.SetMilestoneActualDateForTest(ZDateTime.Now.AddDays(-1));
			AssertEquals("Completed Late", task.Status);
		}

		public void TestStatus_ClosedTaskSetToAssignedWhenTypeIsChanged()
		{
			ProcessTask task = Factory.New<ProcessTask>();
			task.P9_Type = "001";
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Precondition", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);

			task.P9_Type = "002";
			AssertEquals("Should be back to Assigned", ProcessTaskStatusCodeList.Codes.Assigned, task.P9_Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task.P9_GS_NKAssignedStaffMember = "";
			task.P9_Type = "003";
			AssertEquals("AssignedStaffMember is not set, cannot be set back to Assigned", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
		}

		public void TestGetOpenTaskStatuses()
		{
			var task = Factory.New<ProcessTask>();

			foreach (ICodeDescription status in new ProcessTaskStatusCodeList())
			{
				task.P9_Status = status.Code;
				AssertEquals(string.Format("When task has status {0}, it should be considered open", status.Code), Business.ProcessTask.GetOpenTaskStatuses().Contains(status.Code), task.IsOpen);
			}
		}

		public void TestIsOpen_ShouldIgnoreCase()
		{
			var task = Factory.New<ProcessTask>();
			((INeedRow)task).Row[ProcessTasksSchema.Constants.P9_Status] = "can";

			AssertEquals("Precondition: we must have found a way to set the status to lowercase, otherwise this test is meaningless.", "can", task.P9_Status);
			AssertEquals("IsOpen should be case insensitive. SAD!", false, task.IsOpen);
		}

		public void TestSetP9_Status_WhenValueIsLowercase_ShouldSetUppercaseValue()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_Status = "can";

			AssertEquals("CAN", task.P9_Status);
		}

		#endregion

		#region IJobCanBeCancelled

		public void TestJobIsCancelled()
		{
			ProcessTask task1 = Dummy.WorkflowItems.Tasks.AddNew();
			ProcessTask task2 = Dummy.WorkflowItems.Tasks.AddNew();
			ProcessTask task3 = Dummy.WorkflowItems.Tasks.AddNew();
			ProcessTask task4 = Dummy.WorkflowItems.Tasks.AddNew();
			ProcessTask task5 = Dummy.WorkflowItems.Tasks.AddNew();
			ProcessTask task6 = Dummy.WorkflowItems.Tasks.AddNew();
			ProcessTask trg1 = Dummy.WorkflowItems.Triggers.AddNew();

			task1.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task2.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task3.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task4.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task5.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task6.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			task6.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			ProcessTask mil1 = Dummy.WorkflowItems.AddNew();
			mil1.IsMilestone = true;
			mil1.TriggerConditions.TriggerEventCode = Events.Booked.Code;
			mil1.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			ProcessTask mil2 = Dummy.WorkflowItems.AddNew();
			mil2.IsMilestone = true;
			mil2.TriggerConditions.TriggerEventCode = Events.BookingConfirmed.Code;
			mil2.P9_ScheduledDate = ZDateTime.Now.AddDays(-1);

			ProcessTask mil3 = Dummy.WorkflowItems.AddNew();
			mil3.IsMilestone = true;
			mil3.SetMilestoneActualDateForTest(ZDateTime.Now);

			Factory.Save();

			Dummy.IsCancelled = true;
			Factory.Save();

			AssertEquals("2 exceptions should be raised", 2, Dummy.WorkflowItems.Exceptions.Count);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, Dummy.WorkflowItems.Tasks[0].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, Dummy.WorkflowItems.Tasks[1].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, Dummy.WorkflowItems.Tasks[2].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, Dummy.WorkflowItems.Tasks[3].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, Dummy.WorkflowItems.Tasks[4].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, Dummy.WorkflowItems.Tasks[5].P9_Status);
			AssertEquals(1, Dummy.WorkflowItems.Milestones.Count);
			AssertEquals(mil3.PK, Dummy.WorkflowItems.Milestones[0].PK);
			AssertEquals(1, Dummy.WorkflowItems.Triggers.Count);
			Assert(Dummy.WorkflowItems.Exceptions[0].IsExceptionActioned);
			Assert(Dummy.WorkflowItems.Exceptions[1].IsExceptionActioned);
		}

		#endregion

		#region DefaultEstimateIfRequired

		[TestDate(2015, 1, 1)]
		public void TestDefaultEstimateIfRequired()
		{
			var milestone1 = AddNewMilestone(Dummy, 1, "NAM", 0, ZString.Empty, ZDateTime.Empty, true);
			var milestone2 = AddNewMilestone(Dummy, 2, "SRV", 1, "ACT", new ZDateTime(ZDateTime.Now.Year, 1, 1, 2, 15, 00), true);
			var milestone3 = AddNewMilestone(Dummy, 3, "WHS", 2, "EST", new ZDateTime(ZDateTime.Now.Year, 1, 1, 1, 30, 00), true);
			var milestone4 = AddNewMilestone(Dummy, 4, "PID", 3, "EST", new ZDateTime(ZDateTime.Now.Year, 1, 1, 1, 00, 00), true);
			var milestone5 = AddNewMilestone(Dummy, 5, "TID", 4, "EST", new ZDateTime(ZDateTime.Now.Year, 1, 1, 1, 10, 00), false);
			Factory.Save();

			AssertEquals(ZDateTime.Empty, milestone1.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, milestone2.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, milestone3.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, milestone4.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, milestone5.P9_ScheduledDate);

			milestone1.P9_ScheduledDate = new ZDateTime(2012, 12, 11, 13, 10, 00);
			Factory.Save();
			AssertEquals(new ZDateTime(2012, 12, 11, 13, 10, 00), milestone1.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, milestone2.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, milestone3.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, milestone4.P9_ScheduledDate);
			AssertEquals(ZDateTime.Empty, milestone5.P9_ScheduledDate);

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2012, 12, 11, 13, 05, 00));
			Factory.Save();
			AssertEquals(new ZDateTime(2012, 12, 11, 13, 10, 00), milestone1.P9_ScheduledDate);
			AssertEquals(new ZDateTime(2012, 12, 11, 15, 20, 00), milestone2.P9_ScheduledDate);
			AssertEquals(new ZDateTime(2012, 12, 11, 16, 50, 00), milestone3.P9_ScheduledDate);
			AssertEquals(new ZDateTime(2012, 12, 11, 17, 50, 00), milestone4.P9_ScheduledDate);
			AssertEquals(new ZDateTime(2012, 12, 11, 19, 00, 00), milestone5.P9_ScheduledDate);

			milestone2.P9_RecalculateScheduledDate = false;
			milestone2.P9_ScheduledDate = new ZDateTime(2012, 12, 13, 15, 20, 00);
			Factory.Save();
			AssertEquals(new ZDateTime(2012, 12, 11, 13, 10, 00), milestone1.P9_ScheduledDate);
			AssertEquals(new ZDateTime(2012, 12, 13, 15, 20, 00), milestone2.P9_ScheduledDate);
			AssertEquals(new ZDateTime(2012, 12, 13, 16, 50, 00), milestone3.P9_ScheduledDate);
			AssertEquals(new ZDateTime(2012, 12, 13, 17, 50, 00), milestone4.P9_ScheduledDate);
			AssertEquals("Should not be recalculated", new ZDateTime(2012, 12, 11, 19, 00, 00), milestone5.P9_ScheduledDate);

			milestone5.P9_RecalculateScheduledDate = true;
			Factory.Save();
			AssertEquals(new ZDateTime(2012, 12, 13, 19, 00, 00), milestone5.P9_ScheduledDate);

			milestone5.P9_ScheduledDate = new ZDateTime(2012, 12, 15, 12, 00, 00);
			AssertEquals(new ZDateTime(2012, 12, 15, 12, 00, 00), milestone5.P9_ScheduledDate);
			Factory.Save();
			AssertEquals("Should return to system caclulated value", new ZDateTime(2012, 12, 13, 19, 00, 00), milestone5.P9_ScheduledDate);
		}

		ProcessTask AddNewMilestone(DummyWithWorkflow dummy, ZInt sequence, ZString eventCode, ZInt estimatePredecessor, ZString estimatedType, ZDateTime estimatedTimeDelta, ZBool recalculate)
		{
			var milestone = (DummyProcessTask)dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = sequence.ToString();
			milestone.TriggerConditions.TriggerEventCode = eventCode;
			milestone.P9_Sequence = sequence;
			milestone.P9_EstimatedDefaultFromPredecessor = estimatePredecessor;
			milestone.P9_EstimatedDefaultedFrom = estimatedType;
			milestone.P9_EstimatedDefaultTimeDelta = estimatedTimeDelta;
			milestone.P9_RecalculateScheduledDate = recalculate;
			milestone.OverriddenParentTypeForTest = dummy.GetType();
			return milestone;
		}

		[ExpectNoExceptions]
		public void TestDefaultEstimateIfRequiredDoesNotThrowShouldNotBeAccessingPropertyOnDeletedBizO()
		{
			ProcessTask task1 = Dummy.WorkflowItems.Tasks.AddNew();
			task1.P9_Type = Core.Constants.Workflow.UndefinedTaskType;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			task1.Delete();

			task1.DefaultEstimateIfRequired();
		}

		public void TestDefaultEstimate_ShouldCreateEventOnChange()
		{
			var dummy = Factory.New<DummyWithWorkflowAndEventDateProperty>();
			var milestone1 = AddNewMilestone(dummy, 1, AutoEvents.IncidentClosedCode, 0, ZString.Empty, ZDateTime.Empty, true);
			Factory.Save();

			AssertEquals(1, dummy.GetLogs().GetAllLogs().Cast<StmALog>().Count());

			var time = new ZDateTime(2013, 04, 04, 11, 24, 00);
			dummy.Z0_Date = time;
			Factory.Save();

			AssertEquals(time, dummy.Z0_Date);
			AssertEquals(time, milestone1.P9_ScheduledDate);

			var eventLog = dummy.GetLogs().GetAllLogs().Cast<StmALog>().SingleOrDefault(log => log.SL_SE_NKEvent == Events.EstimatedDateChanged.Code);

			AssertNotNull("Even though P9_ScheduledDate was modified via ProcessTaskHandler, still add the event log.", eventLog);
		}

		[TestDate(2049, 6, 30)]
		public void TestDefaultEstimateIfRequiredFromSameEventMilestone()
		{
			var dummy = Factory.New<DummyWithWorkflowAndEventDateProperty>();
			var milestone1 = AddNewMilestone(dummy, 1, AutoEvents.IncidentClosedCode, 0, ZString.Empty, ZDateTime.Empty, true);
			var milestone2 = AddNewMilestone(dummy, 2, AutoEvents.IncidentClosedCode, 1, "EST", new ZDateTime(ZDateTime.Now.Year, 1, 1, 2, 00, 00), true);
			Factory.Save();

			var time = ZDateTime.UtcNow;
			dummy.Z0_Date = time;
			Factory.Save();

			AssertEquals(time, dummy.Z0_Date);
			AssertEquals(time, milestone1.P9_ScheduledDate);
			AssertEquals(time.AddHours(2), milestone2.P9_ScheduledDate);
		}

		public void TestDefaultEstimateIfRequiredUsesMilestoneOffset()
		{
			var milestone1 = AddNewMilestone(Dummy, 1, "NAM", 0, ZString.Empty, ZDateTime.Empty, true);
			var milestone2 = AddNewMilestone(Dummy, 2, "MIS", 1, "ACT", new ZInt(15).GetDateTimeFromMinutes(), true);

			Factory.Save();

			milestone1.SetMilestoneActualDateForTest(new ZDateTimeOffset(ZDateTime.Now.Year, 1, 1, 1, 00, 00, TimeSpan.FromHours(8)));
			Factory.Save();

			var estimateLog1 = Dummy.Logs.MostRecentLogByEventTime(Events.EstimatedDateChanged);
			var estimateLog2 = Dummy.Logs.MostRecentLogByEventTime(Events.MiscellaneousEvent);

			CombineAssertions(() =>
			{
				var expectedOffset = new ZDateTimeOffset(ZDateTime.Now.Year, 1, 1, 1, 00, 00, TimeSpan.FromHours(8)).AddMinutes(15);
				AssertEquals(expectedOffset, milestone2.P9_ScheduledDateForBinding);
				AssertEquals(expectedOffset, estimateLog1.EventTimeOffset);
				Assert(estimateLog2.SL_IsEstimate);
				AssertEquals(expectedOffset, estimateLog2.EventTimeOffset);
			});
		}

		#endregion

		#region ScheduleDateOffset

		public void TestMilestoneScheduleDateShouldBeInDischargePortTimeZone()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			templateMilestone.P9_Description = "M1";
			template.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var countryCode = "US";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "C" + countryCode;
			company.GC_RN_NKCountryCode = countryCode;

			var branch = company.Branches.AddNew();
			branch.GB_Code = "B" + countryCode;
			branch.GB_RL_NKHomePort = "USLAX";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_GB_HomeBranch = branch.PK;

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			template.P0_GC = company.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var consol = Factory.New<Forwarding.IForwardingConsol>();
				((BusinessObject)consol).FillWithValidTestData();
				consol.JK_RL_NKLoadPort = "USLAX";
				consol.JK_RL_NKDischargePort = "AUBNE";
				consol.Transports_Get(0).JW_ETA = ZDateTime.Now.AddDays(1);
				Factory.Save();

				var workflowConsol = (IWorkflowProvider)consol;
				var arvMil = workflowConsol.WorkflowItems.Milestones.Cast<ProcessTask>().SingleOrDefault(m => m.P9_SE_NKMilestoneEvent == Events.ArrivalCode && m.P9_Description == "M1");
				AssertEquals("The milestone's ScheduledDate should be in the Discharge Port time zone ('AUBNE')", TimeSpan.FromHours(10), arvMil.P9_ScheduledDateForBinding.Offset);
			}
		}

		public void TestEstimateEventSetsScheduleDateWithOffset()
		{
			var dummy = Factory.New<DummyWithWorkflowEventOffset>();
			var milestone1 = AddNewMilestone(dummy, 1, AutoEvents.IncidentClosedCode, 0, ZString.Empty, ZDateTime.Empty, true);
			Factory.Save();

			AssertEquals(1, dummy.GetLogs().GetAllLogs().Cast<StmALog>().Count());

			var fakeOffset = new TimeSpan(8, 0, 0);
			dummy.OffsetForTest = fakeOffset;

			var time = new ZDateTime(2013, 04, 04, 11, 24, 00);
			dummy.Z0_Date = time;
			Factory.Save();

			AssertEquals(new ZDateTimeOffset(time, fakeOffset), milestone1.P9_ScheduledDateOffset);

			var eventLog = dummy.GetLogs().GetAllLogs().Cast<StmALog>().SingleOrDefault(log => log.SL_SE_NKEvent == Events.EstimatedDateChanged.Code);
		}

		class DummyWithWorkflowEventOffset : DummyWithWorkflow
		{
			public DummyWithWorkflowEventOffset(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public TimeSpan OffsetForTest { get; set; }

			[EventDateProperty(AutoEvents.IncidentClosedCode, EstimateActual.MilestoneEstimateOnly, true)]
			public override ZDateTime Z0_Date
			{
				get { return base.Z0_Date; }
				set
				{
					base.Z0_Date = value;
					Logs.CreateRecreateOrUpdateEventLog(AutoEvents.IncidentClosed, EstimateActual.Estimate, new ZDateTimeOffset(value, OffsetForTest));
				}
			}
		}

		#endregion

		#region TestRecalculateScheduledDateDoNotCycle

		public void TestRecalculateScheduledDateDoNotCycle()
		{
			var time1Open = new ZDateTime(ZDateTime.Now.Year - 2, 9, 8, 10, 0, 0);
			var time1Close = time1Open.AddHours(2);

			var time2Open = time1Open.AddDays(2);
			var time2Close = time2Open.AddHours(2);

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflowAnd2EventDateProperties>();
			var milestoneOpen = AddNewMilestone(dummy, 1, AutoEvents.IncidentReopenedCode, 2, "EST", new ZDateTime(ZDateTime.Now.Year + 1, 1, 1).AddHours(-2), true);
			// -2 hours from next milestone
			var milestoneClose = AddNewMilestone(dummy, 2, AutoEvents.IncidentClosedCode, 0, ZString.Empty, ZDateTime.Empty, true);

			AssertDateChange("Should recalculate on initial setup", dummy, milestoneOpen, milestoneClose, time1Close, time1Open);
			AssertDateChange("Should recalculate on 1st change time1 -> time2", dummy, milestoneOpen, milestoneClose, time2Close, time2Open);
			AssertDateChange("Should recalculate on 1st change time2 -> time1", dummy, milestoneOpen, milestoneClose, time1Close, time1Open);
			AssertDateChange("Should recalculate on 2nd change time1 -> time2", dummy, milestoneOpen, milestoneClose, time2Close, time2Open);
			AssertDateChange("Should recalculate on 2nd change time2 -> time1", dummy, milestoneOpen, milestoneClose, time1Close, time1Open);
			AssertDateChange("Should recalculate on 3nd change time1 -> time2", dummy, milestoneOpen, milestoneClose, time2Close, time2Open);

			Globals.IsUserInteractive = false;

			CombineAssertions("Dates should be updating up to this point", () =>
			{
				AssertEquals(time2Open, dummy.Z0_Date);
				AssertEquals(time2Open, milestoneOpen.P9_ScheduledDate);
				AssertEquals(time2Close, dummy.Z0_AnotherDate);
				AssertEquals(time2Close, milestoneClose.P9_ScheduledDate);
			});

			dummy.Z0_AnotherDate = time1Close;//here we enter a cycle
			Factory.Save();

			AssertEquals("Cycle has new been detected", false, milestoneOpen.P9_RecalculateScheduledDate);

			Globals.IsUserInteractive = true;
			AssertDateChange("Should recalculate on 3rd change time1 -> time2 for user-interactive mode", dummy, milestoneOpen, milestoneClose, time2Close, time2Open);

			var time3Open = time1Open.AddDays(3);
			var time3Close = time3Open.AddHours(2);
			milestoneOpen.P9_RecalculateScheduledDate = true;

			AssertDateChange("Should recalculate on 1st change time2 -> time3", dummy, milestoneOpen, milestoneClose, time3Close, time3Open);
			AssertDateChange("Should recalculate on 1st change time3 -> time2", dummy, milestoneOpen, milestoneClose, time2Close, time2Open);
		}

		void AssertDateChange(string message, DummyBusinessObject dummy, ProcessTask openMilestone, ProcessTask closeMilestone, ZDateTime newCloseDate, ZDateTime expectedOpenTime)
		{
			dummy.Z0_AnotherDate = newCloseDate;
			dummy.Factory.Save();

			AssertEquals(message, expectedOpenTime, dummy.Z0_Date);
			AssertEquals(message, expectedOpenTime, openMilestone.P9_ScheduledDate);
			AssertEquals(newCloseDate, dummy.Z0_AnotherDate);
			AssertEquals(newCloseDate, closeMilestone.P9_ScheduledDate);
		}

		class DummyWithWorkflowAnd2EventDateProperties : DummyWithWorkflow
		{
			public DummyWithWorkflowAnd2EventDateProperties(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[EventDateProperty(AutoEvents.IncidentReopenedCode, EstimateActual.MilestoneEstimateOnly, true)]
			public override ZDateTime Z0_Date
			{
				get { return base.Z0_Date; }
				set
				{
					base.Z0_Date = value;
					Logs.CreateRecreateOrUpdateEventLog(AutoEvents.IncidentReopened, EstimateActual.Estimate, value.ToOffset());
				}
			}

			[EventDateProperty(AutoEvents.IncidentClosedCode, EstimateActual.MilestoneEstimateOnly, true)]
			public override ZDateTime Z0_AnotherDate
			{
				get { return base.Z0_AnotherDate; }
				set
				{
					base.Z0_AnotherDate = value;
					Logs.CreateRecreateOrUpdateEventLog(AutoEvents.IncidentClosed, EstimateActual.Estimate, value.ToOffset());
				}
			}
		}

		#endregion

		#region TestParentJobDetailsDoNotThrowsException

		[ExpectNoExceptions]
		public void TestParentJobDetailsDoNotThrowsException()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			parent.OverridenHumanReadableName = "Abcde";
			parent.Z0_Description = "Description";

			var task = parent.WorkflowItems.AddNew();

			AssertEquals("Abcde - Description", task.ParentJobDetails);
		}

		#endregion

		#region DefaultDatesFromMilestoneEvent

		public void TestDefaultDatesFromMilestoneEvent()
		{
			var date = new ZDateTime(1984, 7, 10);
			var task1 = Dummy.WorkflowItems.Tasks.AddNew();
			task1.P9_Type = Core.Constants.Workflow.MilestoneType;
			task1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			task1.TriggerConditions.TriggerConditionValue = "111";
			task1.TriggerConditions.TriggerEventCode = "ARV";
			task1.SetMilestoneActualDateForTest(date);

			Factory.Save();

			var task2 = Dummy.WorkflowItems.Tasks.AddNew();
			task2.P9_Type = Core.Constants.Workflow.MilestoneType;
			task2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			task2.TriggerConditions.TriggerConditionValue = "222";
			task2.TriggerConditions.TriggerEventCode = "ARV";

			AssertEquals("should NOT set defaults if Condition1 is REF and Event References do NOT match", ZDateTime.Empty, task2.P9_ActualDate);

			var task3 = Dummy.WorkflowItems.Tasks.AddNew();
			task3.P9_Type = Core.Constants.Workflow.MilestoneType;
			task3.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			task3.TriggerConditions.TriggerConditionValue = "111";
			task3.TriggerConditions.TriggerEventCode = "ARV";

			AssertEquals("should set defaults if Condition1 is REF and Event References match", date, task3.P9_ActualDate);
		}

		public void TestDefaultDatesFromMilestoneEvent_DocumentImportEvent()
		{
			var date = new ZDateTime(1984, 7, 10);
			var task1 = Dummy.WorkflowItems.Tasks.AddNew();
			task1.P9_Type = Core.Constants.Workflow.MilestoneType;
			task1.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			task1.TriggerConditions.TriggerConditionValue = "CIV|5D60D2AB-BF45-4BF6-9CF1-F382DB309EA4";
			task1.TriggerConditions.TriggerEventCode = AutoEvents.DocumentImported.Code;
			task1.SetMilestoneActualDateForTest(date);

			Factory.Save();

			var task2 = Dummy.WorkflowItems.Tasks.AddNew();
			task2.P9_Type = Core.Constants.Workflow.MilestoneType;
			task2.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			task2.TriggerConditions.TriggerConditionValue = "CIV";
			task2.TriggerConditions.TriggerEventCode = AutoEvents.DocumentImported.Code;

			AssertEquals("should set defaults if event is document import event and code contains in condition value matches.", date, task2.P9_ActualDate);
		}

		public void TestDefaultDatesFromMilestoneEventDoesntClearDates()
		{
			ZDateTime date = new ZDateTime(1984, 7, 10);

			ProcessTask task = Factory.New<DummyProcessTask>();
			task.P9_Type = Core.Constants.Workflow.MilestoneType;
			task.TriggerConditions.TriggerEventCode = Events.IncidentClosedCode;
			task.SetMilestoneActualDateForTest(date);
			task.P9_ParentID = Dummy.PK;

			AssertNull("There should be no event registered on bizo", Dummy.Logs.MostRecentLogByEventTime(Events.IncidentClosed));
			AssertEquals("Precondition", date, task.P9_ActualDate);

			task.DefaultDatesFromMilestoneEvent_ForTest(false);
			Assert("Date should not be cleared", !task.P9_ActualDate.IsEmpty);
			AssertEquals("Date should not be cleared", date, task.P9_ActualDate);

			task.DefaultDatesFromMilestoneEvent_ForTest(true);
			Assert("Date should be cleared", task.P9_ActualDate.IsEmpty);
		}

		public void TestDefaultDatesFromMilestoneEventEditedByBPandAD()
		{
			var time1 = new ZDateTimeOffset(2012, 11, 27, 15, 31, 01);
			var time2 = new ZDateTimeOffset(2012, 11, 27, 16, 32, 02);
			var time3 = new ZDateTimeOffset(2012, 11, 27, 17, 33, 03);
			var time4 = new ZDateTimeOffset(2012, 11, 27, 18, 33, 03);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, time1).SL_GS_NKUser = "XX";
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, time2).SL_GS_NKUser = "YY";
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, time3).SL_GS_NKUser = User.ServiceUserCode;
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, time4).SL_GS_NKUser = User.InterchangeUserCode;
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			var task = Dummy.WorkflowItems.Triggers.AddNew();
			task.TriggerConditions.TriggerEventCode = Events.EditedARecordCode;

			task.DefaultDatesFromMilestoneEvent_ForTest(false);
			AssertEquals("Should set date of latest event added by not a BP or AD", time2.ToZDateTime(), task.P9_ActualDate);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, time1).SL_GS_NKUser = "XX";
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, time2).SL_GS_NKUser = "YY";
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, time3).SL_GS_NKUser = User.ServiceUserCode;
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			task.DefaultDatesFromMilestoneEvent_ForTest(false);
			AssertEquals("Should set date of latest event including BP", time3.ToZDateTime(), task.P9_ActualDate);

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Dummy.Logs.AddNew(Events.EditedARecord, time4).SL_GS_NKUser = User.InterchangeUserCode;
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			task.DefaultDatesFromMilestoneEvent_ForTest(false);
			AssertEquals("Should set date of latest event including AD", time4.ToZDateTime(), task.P9_ActualDate);
		}

		public void TestDefaultDatesFromMilestoneEvent_ProcessTaskIsLineTrigger_DoNotDefault()
		{
			Dummy.Logs.AddNew(Events.Departure);

			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.ForwardingShipment;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_TriggerParty = "ORP";

			AssertEquals("PRECONDITION: Actual date before defaulting", ZDateTime.Empty, trigger.P9_ActualDate);
			AssertNull("PRECONDITION: WTE log before defaulting", Dummy.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));

			trigger.TriggerConditions.TriggerEventCode = Events.DepartureCode;

			AssertEquals("Actual date after defaulting", ZDateTime.Empty, trigger.P9_ActualDate);
			AssertNull("WTE log after defaulting", Dummy.Logs.MostRecentLogByEventTime(Events.WorkflowTriggerEvent));
		}

		#endregion

		#region TestIsMatchEventReference

		static bool DoesEventReferenceConditionMatch(IBaseTrigger triggerConditions, TriggerEventLog eventLog, IStmALog log, ZString? triggerConditionValueOverride = null, GetTriggerContext getContext = null, bool findExistingLogForRename = false)
		{
			return TriggerConditionEvaluator.DoesEventReferenceConditionMatch(triggerConditions.GetJob(), triggerConditions, eventLog, getContext, log, triggerConditionValueOverride, findExistingLogForRename);
		}

		public void TestIsMatchEventReference()
		{
			AssertIsMatchEventReference("",
				new[] { "" },
				new[] { "a", "*", "?" });

			AssertIsMatchEventReference("a",
				new[] { "a", "A" },
				new[] { "", "b", "aa", "*", "?" });

			AssertIsMatchEventReference(@"\?a\?",
				new[] { "?a?", "?A?" },
				new[] { "", "a", "aa", "aaa", "?", "??", "???", "*a*", "*A*" });

			AssertIsMatchEventReference(@"\*a\*",
				new[] { "*a*", "*A*" },
				new[] { "", "a", "aa", "aaa", "*", "**", "***", "?a?", "?A?" });

			AssertIsMatchEventReference("?a?",
				new[] { "?a?", "?A?", "*a*", "*A*", "aaa", "bac" },
				new[] { "", "a", "aa", "aaaa", "b?x", "?", "??", "???", "b*x", "*", "**", "***" });

			AssertIsMatchEventReference("*a*",
				new[] { "?a?", "?A?", "*a*", "*A*", "aaa", "bac", "bcdAxyz", "a", "ba", "bcdA", "ax", "Axyz" },
				new[] { "", "b", "bx", "b?x", "?", "??", "???", "b*x", "*", "**", "***" });

			AssertIsMatchEventReference("*a",
				new[] { "?a", "?A", "*a", "*A", "aaa", "a", "ba", "bcdA" },
				new[] { "", "b", "bx", "?a?", "?A?", "*a*", "*A*", "bac", "bcdAxyz", "ax", "Axyz" });

			AssertIsMatchEventReference("a*",
				new[] { "a?", "A?", "a*", "A*", "aaa", "a", "ax", "Axyz" },
				new[] { "", "b", "bx", "?a?", "?A?", "*a*", "*A*", "bac", "bcdAxyz", "ba", "bcdA" });

			AssertIsMatchEventReference("a*b?c?*d",
				new[] { "ab_c_d", "AB_C_D", "ab_c__d", "a*b?c?*d", "a_b_c_d", "a___b_c___d" },
				new[] { "", "abcd", "ABCD", "ab__c_d", "abc_c", "a_b_cd", "a___b___c___d", "_ab_c_d", "ab_c_d_" });

			AssertIsMatchEventReference("(test 1.)",
				new[] { "(test 1.)", "(TEST 1.)" },
				new[] { "", "test 1.", "(test1.)", "(test 1)", "(test 1_)", "(test_1.)", "_test 1._" });
		}

		void AssertIsMatchEventReference(string template, IEnumerable<string> matches, IEnumerable<string> unmatches)
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = template;

			GetTriggerContext getContext = () => (dummy, DummyWorkflowDescriptor.Instance);

			var log = Factory.NewWithValidTestData<StmALog>();

			foreach (string match in matches)
			{
				Assert(match + " should match to " + template, TriggerConditionEvaluator.DoesEventReferenceConditionMatch(dummy, trigger, TriggerEventLog.CreateLog_ForTest(match, false), getContext, log));
				Assert(match + " should match to " + template, TriggerConditionEvaluator.DoesEventReferenceConditionMatch(dummy, trigger, TriggerEventLog.CreateLog_ForTest(match, true), getContext, log));
			}

			foreach (string unmatch in unmatches)
			{
				Assert(unmatch + " should NOT match to " + template, !TriggerConditionEvaluator.DoesEventReferenceConditionMatch(dummy, trigger, TriggerEventLog.CreateLog_ForTest(unmatch, false), getContext, log));
				Assert(unmatch + " should NOT match to " + template, !TriggerConditionEvaluator.DoesEventReferenceConditionMatch(dummy, trigger, TriggerEventLog.CreateLog_ForTest(unmatch, true), getContext, log));
			}
		}

		public void TestIsMatchEventReferenceWhenChanged()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = "a?";

			var log = Factory.NewWithValidTestData<StmALog>();

			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("aa", false), log));
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("bb", false), log));

			trigger.TriggerConditions.TriggerConditionValue = "b?";

			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("aa", false), log));
			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("bb", false), log));
		}

		public void TestIsMatchEventReferencWithCondition_OldLogs()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.IsWorkflowTrigger = true;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			trigger.TriggerConditions.TriggerConditionValue = "a*";

			var log = Factory.NewWithValidTestData<StmALog>();

			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a*", false), log));
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a", false), log));
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("ab", false), log));
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("abc", false), log));

			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a* From: 28-11-2012 To: 29-11-2012", false), log));
			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a* From: 28-11-2012 To: 29-11-2012", true), log));
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a* To: 29-11-2012", false), log));
			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a* To: 29-11-2012", true), log));
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a* From: 28-11-2012", false), log));
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a* From: 28-11-2012", true), log));
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a From: 28-11-2012 To: 29-11-2012", false), log));
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a From: 28-11-2012 To: 29-11-2012", true), log));

			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;
			trigger.TriggerConditions.TriggerConditionValue = "a*";

			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a*", false), log));
			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a", false), log));
			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("ab", false), log));
			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("abc", false), log));
			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a From: 28-11-2012 To: 29-11-2012", false), log));
			Assert(DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("a From: 28-11-2012 To: 29-11-2012", true), log));
		}

		public void TestIsConditionWithMacroMetAndParentIsProcessTaskTemplate()
		{
			var processTaskTemplate = GetNewWorkflowTemplate();
			processTaskTemplate.P0_ProcessType = "SHP";
			var trigger = processTaskTemplate.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			var log = Factory.NewWithValidTestData<StmALog>();
			Assert(!DoesEventReferenceConditionMatch(trigger, TriggerEventLog.CreateLog_ForTest("ADD", false), log, getContext: () => (processTaskTemplate, WorkflowDescriptors.Instance.TryGetValueSafe("SHP"))));
		}

		public void TestUDFMacroContextForLogCopy()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			trigger.TriggerConditions.TriggerConditionValue = "\"<SL_Reference>\" == \"a\"";
			var log = Factory.NewWithValidTestData<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = "a";
			}
			var logCopy = ((IStmALog)log).WeakCopy();

			Assert(DoesEventReferenceConditionMatch(trigger, new TriggerEventLog(logCopy, trigger, dummy), log, getContext: () => (dummy, DummyWorkflowDescriptor.Instance)));
		}

		#endregion

		#region TestGetQueryForDefaultsWithWildcardsAndRegularExpressions

		public void TestLoadDefaultsWithWildcards()
		{
			var parent = Factory.New<DummyWithWorkflow>();

			var task = parent.WorkflowItems.AddNew();
			task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithWildcards;

			foreach (string reference in new[] { "A", "B", "AB", "BA", "AABB", "ACB", "CABC" })
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				task.Logs.AddNew(AutoEvents.EditedARecord, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "A*", new[] { "A", "AB", "AABB", "ACB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "*B", new[] { "B", "AB", "AABB", "ACB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "A*B", new[] { "AB", "AABB", "ACB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "*C*", new[] { "ACB", "CABC" });

			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "A?", new[] { "AB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "?B", new[] { "AB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "A?B", new[] { "ACB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "?C?", new[] { "ACB" });
		}

		public void TestLoadDefaultsWithRegularExpressions()
		{
			var parent = Factory.New<DummyWithWorkflow>();

			var task = parent.WorkflowItems.AddNew();
			task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceWithRegularExpressions;

			foreach (string reference in new[] { "A", "B", "AB", "BA", "AABB", "ACB", "CABC" })
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				task.Logs.AddNew(AutoEvents.EditedARecord, reference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "A.*", new[] { "A", "AB", "AABB", "ACB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, ".*B", new[] { "B", "AB", "AABB", "ACB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "A.*B", new[] { "AB", "AABB", "ACB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, ".*C.*", new[] { "ACB", "CABC" });

			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "A.", new[] { "AB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, ".B", new[] { "AB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, "A.B", new[] { "ACB" });
			AssertLoadDefaultsWithWildcardsOrRegularExpressions(task, ".C.", new[] { "ACB" });
		}

		public void TestGetQueryForDefaults_ConditionWithMacro()
		{
			var parent1 = Factory.New<DummyWithWorkflow>();
			var parent2 = Factory.New<DummyWithWorkflow>();

			var log1 = parent1.Logs.AddNew();
			var log2 = parent2.Logs.AddNew();
			var allLogs = new[] { log1, log2 };

			var task = parent1.WorkflowItems.AddNew();
			task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			task.TriggerConditions.TriggerConditionValue = "Source.LoadPort == \"UADOC\" && Source.DischargePort == \"RUPKV\"";

			var query = Business.WorkflowDefaultDateProvider.GetQueryForDefaults(task, false);

			parent1.LoadPort = "UAIEV";
			parent1.DischargePort = "RUPKV";
			var logs = allLogs.Where(x => query(x)).ToArray();
			AssertEquals("Number of matched logs", 0, logs.Length);

			parent1.LoadPort = "UADOC";
			parent1.DischargePort = "RUPKV";
			logs = allLogs.Where(x => query(x)).ToArray();
			AssertEquals("Number of matched logs", 1, logs.Length);
			AssertEquals("Log's PK", log1.PK, logs[0].PK);
		}

		public void TestGetQueryForDefaults_ConditionWithInvalidMacro_MCR()
		{
			var parent1 = Factory.New<DummyWithWorkflow>();
			var log1 = parent1.Logs.AddNew();
			var task = parent1.WorkflowItems.AddNew();
			task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			task.TriggerConditions.TriggerConditionValue = "Source.BadType";

			var query = Business.WorkflowDefaultDateProvider.GetQueryForDefaults(task, false);

			AssertNoExceptionThrown("Bad macro should not throw", () => query(log1));
		}

		public void TestGetQueryForDefaults_ConditionWithInvalidMacro_UDF()
		{
			var parent1 = Factory.New<DummyWithWorkflow>();
			var log1 = parent1.Logs.AddNew();
			var task = parent1.WorkflowItems.AddNew();
			task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			task.TriggerConditions.TriggerConditionValue = "\"<BadType>\"";

			var query = Business.WorkflowDefaultDateProvider.GetQueryForDefaults(task, false);

			AssertNoExceptionThrown("Bad macro should not throw", () => query(log1));
		}

		public void TestGetQueryForDefaults_EventReferenceParametersCondition()
		{
			var businessObject = Factory.New<DummyWithWorkflow>();
			var logs = new[]
			{
				businessObject.Logs.AddNew(AutoEvents.Arrival, "LOC".AsKeyFor("UAIEV"), "FAC".AsKeyFor("Port")),		// [0]
				businessObject.Logs.AddNew(AutoEvents.Arrival, "LOC".AsKeyFor("UAIEV"), "FAC".AsKeyFor("Terminal")),	// [1]
				businessObject.Logs.AddNew(AutoEvents.Arrival, "LOC".AsKeyFor("AUSYD"), "FAC".AsKeyFor("Port")),		// [2]
				businessObject.Logs.AddNew(AutoEvents.Arrival, "LOC".AsKeyFor("AUSYD"), "FAC".AsKeyFor("Terminal"))		// [3]
			};

			EnsureLogsAreFiltered(businessObject, "LOC=UAIEV", new StmALog[] { logs[0], logs[1] });
			EnsureLogsAreFiltered(businessObject, "FAC=Terminal", new StmALog[] { logs[1], logs[3] });
			EnsureLogsAreFiltered(businessObject, "LOC=AUSYD,FAC=PORT", new StmALog[] { logs[2] });
			EnsureLogsAreFiltered(businessObject, "LOC=AUSYD,FAC=PORT,NAM=SRV", Array.Empty<StmALog>());
		}

		public void EnsureLogsAreFiltered(IWorkflowProvider parent, string condition, IEnumerable<StmALog> expectedLogs)
		{
			var milestone = parent.WorkflowItems.AddNew();
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			milestone.TriggerConditions.TriggerConditionValue = condition;

			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;

			var actualLogs = Factory.Load<StmALog>(query).Where(x => Business.WorkflowDefaultDateProvider.GetQueryForDefaults(milestone, false)(x)).ToArray();
			AssertContainsExactElementsInAnyOrder("Filtered logs", expectedLogs.Select(l => l.PK), actualLogs.Select(l => l.PK));
		}

		public void AssertLoadDefaultsWithWildcardsOrRegularExpressions(ProcessTask task, string template, string[] results)
		{
			task.TriggerConditions.TriggerConditionValue = template;

			var queryFunc = Business.WorkflowDefaultDateProvider.GetQueryForDefaults(task, false);
			var query = new ZQuery();
			query.FetchOnlyFromLocalCache = true;

			var logs = Factory.Load<StmALog>(query).Where(x => queryFunc(x)).ToArray();

			AssertEquals(results.Length, logs.Length);
			AssertContainsExactElementsInAnyOrder(results, from log in logs select log.SL_Reference.ToString());
		}

		#endregion

		#region TestP9_CascadedEventsContext

		public void TestP9_CascadedEventsContext_ReadOnly()
		{
			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest = new[] { new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null)) };

			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();

			trigger.P9_RespondToCascadedEvents = false;
			AssertEquals("Context should be readonly", true, trigger.P9_CascadedEventsContextInfo.ReadOnly);

			trigger.P9_RespondToCascadedEvents = true;
			AssertEquals("Context should be editable", false, trigger.P9_CascadedEventsContextInfo.ReadOnly);
		}

		public void TestP9_RespondToCascadedEvents_ResetContextOnFalse()
		{
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_RespondToCascadedEvents = false;
			trigger.P9_CascadedEventsContext = "ABC";

			trigger.P9_RespondToCascadedEvents = true;
			AssertEquals("ABC", trigger.P9_CascadedEventsContext);

			trigger.P9_RespondToCascadedEvents = false;
			Assert(trigger.P9_CascadedEventsContext.IsEmpty);
		}

		public void TestAreTriggerOrMilestoneActionConditionsMet_ForCascadedEventContext()
		{
			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest =
				new List<WorkflowEventContextPair>
				{
						new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null)),
				};

			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.IncidentClosedCode;

			var log = Dummy.Factory.New<DummyWithWorkflow>().Logs.AddNew(Events.IncidentClosed);

			Assert(trigger.WorkflowDescriptor is DummyWorkflowDescriptor);

			AssertEquals("Matched by event type", true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

			trigger.P9_RespondToCascadedEvents = true;
			AssertEquals("Matched because context is not used", true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

			trigger.P9_CascadedEventsContext = "C1 T1";
			((DummyWorkflowDescriptor)trigger.WorkflowDescriptor).IsRelatedEntityInContextForTest = false;
			AssertEquals("Not matched context", false, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));

			((DummyWorkflowDescriptor)trigger.WorkflowDescriptor).IsRelatedEntityInContextForTest = true;
			AssertEquals("Matched context", true, TriggerConditionEvaluator.AreTriggerConditionsMet(trigger, log, Dummy));
		}

		#endregion

		#region ReleaseGroup

		public void TestReleaseGroupOnWorkflow_ShouldValidateWhenAddingTasks()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = (BMSystem)helper.CreateSystem(Factory, "ORG");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = system.ReleaseGroups.AddNew();
			releaseGroup.FSG_GG_Group = group.PK;

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = group.PK;
			AssertNoErrors(workflow.FH_GG_ReleaseGroupInfo);

			group.GG_IsActive = false;

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task1.P9_FH_ProcessHeader = workflow.PK;

			Factory.Save();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var loadedJob = loadedWorkflow.Factory.Load<OrgHeader>(job.PK);
			loadedWorkflow.Validation.ValidateFH_GG_ReleaseGroup();
			AssertNoError(loadedWorkflow.FH_GG_ReleaseGroupInfo, "This Release Group is inactive.");

			var newTask = loadedJob.WorkflowItems.AddNew();
			newTask.P9_FH_ProcessHeader = loadedWorkflow.PK;
			AssertHasError(loadedWorkflow.FH_GG_ReleaseGroupInfo, "This Release Group is inactive.");

			newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertNoError(loadedWorkflow.FH_GG_ReleaseGroupInfo, "This Release Group is inactive.");

			newTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertHasError(loadedWorkflow.FH_GG_ReleaseGroupInfo, "This Release Group is inactive.");
		}

		#endregion

		#region IRootTypeProvider

		public void TestIRootTypeProvider()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var task = Dummy.WorkflowItems.AddNew();

			var rootTypes = ((IRootTypeProvider)task).RootTypes;
			AssertEquals(2, rootTypes.Length);
			AssertEquals(typeof(DummyWithWorkflow), rootTypes[0]);
			AssertEquals(typeof(DummyProcessTask), rootTypes[1]);

			var roots = ((IRootTypeProvider)task).Roots;
			AssertEquals(2, roots.Length);
			AssertEquals(Dummy, roots[0]);
			AssertEquals(task, roots[1]);
		}

		public void TestRootTypes()
		{
			var task = Factory.New<ProcessTask>();
			var rootTypeProvider = task as IRootTypeProvider;
			AssertSequencesEqual("RootTypes", new[] { task.GetType() }, rootTypeProvider.RootTypes);

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var shipmentTask = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			rootTypeProvider = shipmentTask;
			AssertSequencesEqual("Task's Parent Job is shipment", new[] { shipment.GetType(), shipmentTask.GetType() }, rootTypeProvider.RootTypes);

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			AssertSequencesEqual("Task's Parent Job is shipment and shipment has attached declaration which company is the same as the context, has extra macro type(type of declaration)", new[] { shipment.GetType(), shipmentTask.GetType(), declaration.GetType() }, rootTypeProvider.RootTypes);
		}

		#endregion

		#region IsCurrentTask

		public void TestIsCurrentTask()
		{
			var task = Factory.New<ProcessTask>();

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Assert("The task should be treated as 'Current'.", task.IsCurrent);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Assert("The task should be treated as 'Current'.", task.IsCurrent);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Assert("The task should be treated as 'Current'.", task.IsCurrent);

			task.P9_Status = "XYZ";
			Assert("The task should NOT be treated as 'Current'.", !task.IsCurrent);
		}

		#endregion

		#region Visual board

		public void TestCustomisedCardExclusions()
		{
			var excludedProperties = typeof(ProcessTask).GetProperties()
				.Where(p => p.GetAttribute<CustomisedControlExcludeAttribute>() != null)
				.Select(p => p.Name)
				.ToArray();

			var expected = new[]
			{
				nameof(ProcessTasks.P9_TriggerCondition),
				nameof(ProcessTasks.P9_TriggerField),
				nameof(ProcessTasks.P9_Condition1),
				nameof(ProcessTasks.P9_Condition2),
				nameof(ProcessTasks.P9_Condition2Value),
				nameof(ProcessTasks.P9_Notes),
				nameof(ProcessTasks.P9_NotesAsString),
				nameof(ProcessTasks.P9_TriggerFiredCountdown),
				nameof(ProcessTasks.RelevantEstimateHours),
				nameof(ProcessTasks.P9_TriggerConditionValue),
				nameof(ProcessTasks.P9_SE_NKMilestoneEvent),
			};

			AssertContainsExactElementsInAnyOrder(expected, excludedProperties);
		}

		public void TestVisualBoardProperties()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var task = Factory.NewWithValidTestData<ProcessTask>();
			AssertEquals(ZString.Empty, task.VisualBoardNoteText);

			task.P9_CardNote = "Really important task";
			Factory.Save();

			task = new BusinessObjectFactory().Load<ProcessTask>(task.PK);
			AssertEquals("Really important task", task.VisualBoardNoteText);
		}

		public void TestVisualBoardNoteText_WhenCapabilityRequiredAndNoResourceAssigned()
		{
			var task = Factory.New<ProcessTask>();
			Assert(!task.RequiresResourceWithCapability);
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "COD";
			task.P9_G4_RequiredCapability = capability.PK;

			Assert(task.RequiresResourceWithCapability);
			AssertEquals("Requires COD capability", task.VisualBoardNoteText);

			task.P9_GS_NKAssignedStaffMember = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			Assert(!task.RequiresResourceWithCapability);
		}

		public void TestEstimateDescription()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_EstDuration = new ZDateTime(2013, 1, 1, 1, 0, 1); // 1 hour and one second (extra second to make sure rounding works)
			AssertEquals("1:00 to 2:00 hours (standard estimate 1:30)", task.EstimateDescription);
		}

		public void TestCardStatusDescription()
		{
			var task = Factory.New<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "DE";
			staff.GS_FullName = "Dave East";
			var capability = Factory.New<GlbCapability>();
			capability.G4_Description = "Coding";

			AssertEquals("Suspended", task.CardStatusDescription);

			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			AssertEquals("Suspended (Dave East)", task.CardStatusDescription);

			task.P9_G4_RequiredCapability = capability.PK;
			AssertEquals("Suspended (Dave East)", task.CardStatusDescription);

			task.P9_GS_NKAssignedStaffMember = ZString.Empty;
			AssertEquals("Suspended (requires Coding capability)", task.CardStatusDescription);
		}

		#endregion

		#region LastEditTime

		[TestDate(2013, 2, 25)]
		public void TestSavingProcessTask_ShouldUpdateLastEditTimeOfHeader()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = (BMSystem)helper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var processJobHeader = ProcessJobHeaderProvider.GetForParent(org, Factory);
			var header = processJobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);

			var task = org.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;

			Factory.Save();

			AssertEquals(TestDateAttribute.Date, header.FH_SystemLastEditTimeUtc);
		}

		[TestDate(2014, 6, 6, 12, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestSavingProcessTask_WorkflowConcurrentSaveMessage()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var system = (BMSystem)helper.CreateSystem(Factory, "ORG");

			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.GS_FullName = "Staff 1";
			staff1.GS_LoginName = "test.staff.1";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "ST2";
			staff2.GS_FullName = "Staff 2";
			staff2.GS_LoginName = "test.staff.2";

			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var org = factory1.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(org, factory1);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var task1 = org.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var orgInAnotherFactory = factory2.Load<OrgHeader>(org.PK);
			orgInAnotherFactory.WorkflowItems[0].P9_NotesAsString = "Factory2";
			orgInAnotherFactory.WorkflowItems[0].ProcessHeader.FH_DateAcceptability = "NAM";

			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				task1.P9_NotesAsString = "Factory1";
				task1.ProcessHeader.FH_DateAcceptability = "SRV";
				factory1.Save();
			}

			var notificationHandler = new TestNoficationHandler();
			try
			{
				using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					factory2.Save();
				}
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, notificationHandler, true);
			}

			var errorMessage = notificationHandler.GetErrorMessage();
			AssertContains(string.Format("{0} (Staff 1 @ 06 Jun 2014 22:00:00)", task1.HumanReadableName), errorMessage);
			AssertContains("Workflow:  (Staff 1 @ 06 Jun 2014 22:00:00)", errorMessage);
		}

		public void TestDoOnWTELogAddedActionOnce()
		{
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();

			var log = ((IWorkflowProvider)Dummy).Logs.AddNew(Events.CustomisableEvent00);
			var triggeringEvent = new EventSource(log);

			var counter = 0;

			WorkflowTriggerEventExtensions.AddWorkflowTriggerEventLog(trigger, Dummy, triggeringEvent, null, () => counter++);
			AssertEquals("onWTELogAdded action should only run once", 1, counter);
		}

		class TestNoficationHandler : INotificationHandler
		{
			public TestNoficationHandler()
			{
				messageBuilder = new ZStringBuilder();
			}
			readonly ZStringBuilder messageBuilder;

			public string GetErrorMessage()
			{
				return messageBuilder.ToString();
			}

			#region INotificationHandler Members

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				messageBuilder.AppendLine(caption);
				messageBuilder.AppendLine(message);
			}

			public void ReportInformation(string message, string caption)
			{
				messageBuilder.AppendLine(caption);
				messageBuilder.AppendLine(message);
			}

			#endregion
		}

		#endregion

		#region GLOW

		public void TestHasGlowInterfaceReference()
		{
			AssertNotNull(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(ProcessTask), true));
		}

		#endregion

		#region TestAddEventTrigger

		public void TestAddEventTriggerFromWorkflowInNewBizo()
		{
			var template = GetNewWorkflowTemplate();
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test ADD";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AddedARecordToTheSystemCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "NTF";
			action.PQ_TriggerParty = "EML";
			action.PQ_EmailAddr = "emailbox@is.full";
			template.Factory.Save();

			var newDummy = Factory.New<DummyWithWorkflow>();

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(newDummy, TemplateApplicationParameters.ApplyIgnoreHasChanges());

			AssertNull(newDummy.Logs.MostRecentLogByEventTime(AutoEvents.AddedARecordToTheSystem));

			AssertEquals(1, newDummy.WorkflowItems.Triggers.Count);
			var actualTrigger = newDummy.WorkflowItems.Triggers[0];
			AssertEquals(AutoEvents.AddedARecordToTheSystemCode, actualTrigger.P9_SE_NKMilestoneEvent);

			Assert(actualTrigger.P9_ActualDate.IsEmpty);
			AssertNull(actualTrigger.Logs.MostRecentLogByEventTime(AutoEvents.WorkflowTriggerEvent));

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			newDummy.Logs.AddNew(AutoEvents.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			AssertNotNull(newDummy.Logs.MostRecentLogByEventTime(AutoEvents.AddedARecordToTheSystem));

			Assert(!actualTrigger.P9_ActualDate.IsEmpty && actualTrigger.P9_ActualDate.IsValid);
			AssertNotNull(actualTrigger.Logs.MostRecentLogByEventTime(AutoEvents.WorkflowTriggerEvent));
		}

		public void TestAddEventTriggerAfterEvent()
		{
			var newDummy = Factory.New<DummyWithWorkflow>();
			newDummy.Logs.AddNew(AutoEvents.CustomisableEvent00);

			var trigger = newDummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test ADD";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "NTF";
			action.PQ_TriggerParty = "EML";
			action.PQ_EmailAddr = "emailbox@is.full";

			Assert(trigger.P9_ActualDate.IsValid);
			AssertNotNull(trigger.Logs.MostRecentLogByEventTime(AutoEvents.WorkflowTriggerEvent));
		}

		public void TestTriggerFireEvenWithNoActions()
		{
			var newDummy = Factory.New<DummyWithWorkflow>();

			var trigger = newDummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test ADD";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			newDummy.Logs.AddNew(AutoEvents.CustomisableEvent00);

			Assert(trigger.P9_ActualDate.IsValid);
			AssertNotNull(trigger.Logs.MostRecentLogByEventTime(AutoEvents.WorkflowTriggerEvent));
		}

		public void TestEventTrigger_AddingMultipleEventsOnDifferentFactoryInLessThan1Minute()
		{
			var event1 = Events.Authorised;
			var reference1 = "reference 1";
			var event2 = Events.DocumentAllocated;
			var reference2 = "reference 2";

			var testCases = new[] {
				new EventValueForTest(scenario: "Same Event, Same Reference", givenEvent: event1, givenReference: reference1),
				new EventValueForTest(scenario: "Same Event, Different Reference", givenEvent: event1, givenReference: reference2),
				new EventValueForTest(scenario: "Different Event, Same Reference", givenEvent: event2, givenReference: reference1),
				new EventValueForTest(scenario: "Different Event, Different Reference", givenEvent: event2, givenReference: reference2),
			};

			var combineAssertionMessage = string.Format("\r\nGIVEN previous-event={0}, previous-reference=\"{1}\"\r\nWHEN test event or reference is different from them\r\nTHEN WTE event should be created.", event1, reference1);

			CombineAssertions(combineAssertionMessage, () =>
			{
				foreach (var testCase in testCases)
				{
					var eventTime1 = ZDateTimeOffset.Now;
					var trigger1 = Dummy.WorkflowItems.Triggers.AddNew();
					var triggerNotification = trigger1.ProcessTaskNotifications.AddNew();
					var eventValue1 = new EventValue(event1, eventTime: eventTime1, reference: reference1);
					var sourceLog1 = Dummy.GetLogs().AddNew(eventValue1);
					((IBaseTrigger)trigger1).SetEventTime(sourceLog1, Dummy, sourceLog1.SL_EventTimeOffset);
					Factory.Save();

					BusinessObjectFactory factory2 = new BusinessObjectFactory();

					var eventTime2 = ZDateTimeOffset.Now;
					var trigger2 = factory2.Load<ProcessTask>(trigger1.PK);
					var eventValue2 = new EventValue(testCase.GivenEvent, eventTime: eventTime2, reference: testCase.GivenReference);
					var sourceLog2 = Dummy.GetLogs().AddNew(eventValue2);
					((IBaseTrigger)trigger2).SetEventTime(sourceLog2, Dummy, sourceLog2.SL_EventTimeOffset);
					factory2.Save();

					Assert("should be less than 1 minutes", (eventTime2 - eventTime1).Minutes < 1);

					var assertMessage = string.Format("\r\nSCENARIO={0}\r\nGIVEN event={1}, reference=\"{2}\"\r\nWHEN setting ActualDate for Event on Trigger\r\nSHOULD create WTE",
						testCase.Scenario,
						testCase.GivenEvent,
						testCase.GivenReference);

					var logs = trigger2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEvent.Code));

					var expectedPreviousReference1 = string.Format("{0}|{1}|{2}|E|{1}|{2}||", sourceLog1.PK, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code);
					var expectedPreviousReference2 = string.Format("{0}|{1}|{2}|E|{1}|{2}||", sourceLog2.PK, GlbBranch.CurrentBranch.GB_Code, GlbDepartment.CurrentDepartment.GE_Code);

					AssertEquals(assertMessage + " (2 WTE log should be created for previous-event and test-event)", 2, logs.Length);
					Assert("WTE event from trigger1 should exist", logs.Any(l => l.SL_Reference.Contains(expectedPreviousReference1)));
					Assert("WTE event from trigger2 should exist", logs.Any(l => l.SL_Reference.Contains(expectedPreviousReference2)));
				}
			});
		}

		class EventValueForTest
		{
			public readonly ZString Scenario;
			public readonly Event GivenEvent;
			public readonly ZString GivenReference;

			public EventValueForTest(ZString scenario, Event givenEvent, ZString givenReference)
			{
				Scenario = scenario;
				GivenEvent = givenEvent;
				GivenReference = givenReference;
			}
		}

		#endregion

		#region NextSequenceNumber

		public void TestNextSequenceNumber()
		{
			Dummy.WorkflowItems.Tasks.AddNew().P9_Sequence = 2;
			Dummy.WorkflowItems.Tasks.AddNew().P9_Sequence = 5;

			AssertEquals(2, Dummy.WorkflowItems.Tasks.Count);

			var newTask1 = Factory.New<DummyProcessTask>();
			newTask1.P9_ParentID = Dummy.PK;
			newTask1.P9_Sequence = newTask1.NextSequenceNumber;

			var newTask2 = Factory.New<DummyProcessTask>();
			newTask2.P9_ParentID = Dummy.PK;
			newTask2.P9_Sequence = newTask2.NextSequenceNumber;

			AssertEquals("Collection should not be updated automatically", 2, Dummy.WorkflowItems.Tasks.Count);
			AssertEquals(6, newTask1.P9_Sequence);
			AssertEquals("Sequence number should include elements not loaded into collection", 7, newTask2.P9_Sequence);
		}

		#endregion

		#region TaskAssignments

		public void TestTaskAssignments_Inactive()
		{
			var tasks = SetupEnvironment(activeRegistry: false, scope: ScopeList.Codes.Job);
			var task1 = tasks[0];
			var task2 = tasks[1];
			var task3 = tasks[2];
			var task4 = tasks[3]; // pre-requisite workflow
			var task5 = tasks[4]; // post-requisite workflow
			var task6 = tasks[5]; // child workflow
			var task7 = tasks[6]; // current workflow but P9_Type is not part of registry restriction

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals(resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task3.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task4.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task5.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task6.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, task7.P9_GS_NKAssignedStaffMember);
		}

		public void TestTaskAssignments_Job()
		{
			var tasks = SetupEnvironment(activeRegistry: true, scope: ScopeList.Codes.Job);
			var task1 = tasks[0];
			var task2 = tasks[1];
			var task3 = tasks[2];
			var task4 = tasks[3]; // pre-requisite workflow
			var task5 = tasks[4]; // post-requisite workflow
			var task6 = tasks[5]; // child workflow
			var task7 = tasks[6]; // current workflow but P9_Type is not part of registry restriction

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals(resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task3.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task4.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task5.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task6.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, task7.P9_GS_NKAssignedStaffMember);
		}

		public void TestTaskAssignments_Workflow()
		{
			var tasks = SetupEnvironment(activeRegistry: true, scope: ScopeList.Codes.Workflow);
			var task1 = tasks[0];
			var task2 = tasks[1];
			var task3 = tasks[2];
			var task4 = tasks[3]; // pre-requisite workflow
			var task5 = tasks[4]; // post-requisite workflow
			var task6 = tasks[5]; // child workflow
			var task7 = tasks[6]; // current workflow but P9_Type is not part of registry restriction

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals(resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task3.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task4.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task5.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task6.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, task7.P9_GS_NKAssignedStaffMember);
		}

		public void TestTaskAssignments_PreRequisite()
		{
			var tasks = SetupEnvironment(activeRegistry: true, scope: ScopeList.Codes.Prerequisites);
			var task1 = tasks[0];
			var task2 = tasks[1];
			var task3 = tasks[2];
			var task4 = tasks[3]; // pre-requisite workflow
			var task5 = tasks[4]; // post-requisite workflow
			var task6 = tasks[5]; // child workflow
			var task7 = tasks[6]; // current workflow but P9_Type is not part of registry restriction

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals(resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task3.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task4.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task5.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task6.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, task7.P9_GS_NKAssignedStaffMember);
		}

		public void TestTaskAssignments_PostRequisite()
		{
			var tasks = SetupEnvironment(activeRegistry: true, scope: ScopeList.Codes.Postrequisites);
			var task1 = tasks[0];
			var task2 = tasks[1];
			var task3 = tasks[2];
			var task4 = tasks[3]; // pre-requisite workflow
			var task5 = tasks[4]; // post-requisite workflow
			var task6 = tasks[5]; // child workflow
			var task7 = tasks[6]; // current workflow but P9_Type is not part of registry restriction

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals(resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task3.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task4.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task5.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task6.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, task7.P9_GS_NKAssignedStaffMember);
		}

		public void TestTaskAssignments_Child()
		{
			var tasks = SetupEnvironment(activeRegistry: true, scope: ScopeList.Codes.Child);
			var task1 = tasks[0];
			var task2 = tasks[1];
			var task3 = tasks[2];
			var task4 = tasks[3]; // pre-requisite workflow
			var task5 = tasks[4]; // post-requisite workflow
			var task6 = tasks[5]; // child workflow
			var task7 = tasks[6]; // current workflow but P9_Type is not part of registry restriction

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals(resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task3.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task4.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task5.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task6.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, task7.P9_GS_NKAssignedStaffMember);
		}

		public void TestTaskAssignments_DoesntOverwriteAssignedResource()
		{
			BMTestHelper.CreateSystem(Factory, "DUM");

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();
			var task3 = job.WorkflowItems.Tasks.AddNew();

			task1.P9_GS_NKAssignedStaffMember = "";
			task2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			task3.P9_GS_NKAssignedStaffMember = "";

			task1.P9_FH_ProcessHeader = workflow.PK;
			task2.P9_FH_ProcessHeader = workflow.PK;
			task3.P9_FH_ProcessHeader = workflow.PK;

			task1.P9_Type = "INV";
			task2.P9_Type = "CDU";
			task3.P9_Type = "CDF";

			SetupRegistry(true, ScopeList.Codes.Workflow, NotificationTypeList.Codes.Error);

			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;

			AssertEquals(resource1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource2.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource1.GS_Code, task3.P9_GS_NKAssignedStaffMember);
		}

		public void TestTaskAssignments_NotificationTypeNON()
		{
			var tasks = SetupEnvironment(activeRegistry: true, scope: ScopeList.Codes.Workflow, notificationType: NotificationTypeList.Codes.None);
			var task1 = tasks[0];
			var task2 = tasks[1];
			var task3 = tasks[2];
			var task4 = tasks[3]; // pre-requisite workflow
			var task5 = tasks[4]; // post-requisite workflow
			var task6 = tasks[5]; // child workflow
			var task7 = tasks[6]; // current workflow but P9_Type is not part of registry restriction

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			task1.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals(resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task3.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task4.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task5.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task6.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, task7.P9_GS_NKAssignedStaffMember);
		}

		public void TestTaskAssignments_Workflow_ShouldPropagateInverselyAlso()
		{
			var tasks = SetupEnvironment(activeRegistry: true, scope: ScopeList.Codes.Workflow);
			var task1 = tasks[0];
			var task2 = tasks[1];
			var task3 = tasks[2];
			var task4 = tasks[3]; // pre-requisite workflow
			var task5 = tasks[4]; // post-requisite workflow
			var task6 = tasks[5]; // child workflow
			var task7 = tasks[6]; // current workflow but P9_Type is not part of registry restriction

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			task2.P9_GS_NKAssignedStaffMember = resource.GS_Code;

			AssertEquals(resource.GS_Code, task2.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(resource.GS_Code, task3.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task4.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task5.P9_GS_NKAssignedStaffMember);
			AssertNotEquals(resource.GS_Code, task6.P9_GS_NKAssignedStaffMember);
			AssertEquals(ZString.Empty, task7.P9_GS_NKAssignedStaffMember);
		}

		public void TestTaskAssignments_ShouldNotAutoAssignStaffOrGroup()
		{
			WorkflowDataRegistry.Instance.TaskAssignmentAutoAssignStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			BMTestHelper.CreateSystem(Factory, "DUM");

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var group1 = Factory.New<GlbGroup>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var group2 = Factory.New<GlbGroup>();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			var task1 = job.WorkflowItems.Tasks.AddNew();

			task1.P9_GS_NKAssignedStaffMember = resource1.GS_Code;

			var task2 = job.WorkflowItems.Tasks.AddNew();

			AssertEquals(task2.P9_GS_NKAssignedStaffMember, ZString.Empty);
			AssertEquals(task2.P9_GG_AssignedGroup, ZGuid.Empty);
			AssertNotEquals(task2.P9_GS_NKAssignedStaffMember, resource1.GS_Code);

			task2.P9_GS_NKAssignedStaffMember = "";
			task2.P9_GG_AssignedGroup = group1.PK;

			var task3 = job.WorkflowItems.Tasks.AddNew();

			AssertEquals(task3.P9_GS_NKAssignedStaffMember, ZString.Empty);
			AssertEquals(task3.P9_GG_AssignedGroup, ZGuid.Empty);
			AssertNotEquals(task3.P9_GG_AssignedGroup, group1.PK);

			task3.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			task3.P9_GG_AssignedGroup = group2.PK;

			var task4 = job.WorkflowItems.Tasks.AddNew();

			AssertEquals(task4.P9_GS_NKAssignedStaffMember, ZString.Empty);
			AssertEquals(task4.P9_GG_AssignedGroup, ZGuid.Empty);
			AssertNotEquals(task4.P9_GS_NKAssignedStaffMember, resource2.GS_Code);
			AssertNotEquals(task4.P9_GG_AssignedGroup, group2.PK);
		}

		void SetupRegistry(bool active, string scope, string notificationType)
		{
			BMTestHelper.EnableBMSInRegistry();

			var categorisedWorkflowTaskTypeCollection = new CategorisedWorkflowTaskTypesCollection();

			var categorisedWorkflowTaskType = categorisedWorkflowTaskTypeCollection.AddNew();
			categorisedWorkflowTaskType.Code = "ORG";

			var workflowTaskType1 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType2 = categorisedWorkflowTaskType.TaskTypes.AddNew();
			var workflowTaskType3 = categorisedWorkflowTaskType.TaskTypes.AddNew();

			workflowTaskType1.Code = "INV";
			workflowTaskType2.Code = "CDU";
			workflowTaskType3.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedWorkflowTaskTypeCollection);

			var collection = new TaskTypeRestrictionsCollection();

			var restriction = collection.AddNew();
			restriction.Active = active;
			restriction.WorkflowType = "ORG";
			restriction.TaskType = "INV";
			restriction.NotificationType = notificationType;
			restriction.Scope = scope;
			restriction.RestrictionType = RestrictionTypeList.Codes.SameResource;

			var taskType1 = restriction.TaskTypesCollection.AddNew();
			var taskType2 = restriction.TaskTypesCollection.AddNew();

			taskType1.Code = "CDU";
			taskType2.Code = "CDF";

			WorkflowDataRegistry.Instance.TaskAssignmentRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		List<ProcessTask> SetupEnvironment(bool activeRegistry, string scope, string notificationType = NotificationTypeList.Codes.Error)
		{
			BMTestHelper.CreateSystem(Factory, "DUM");
			SetupRegistry(activeRegistry, scope, notificationType);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var currentWorkflow = jobHeader.ProcessHeaders.AddNew();
			var preReqWorkflow = jobHeader.ProcessHeaders.AddNew();
			var postReqWorkflow = jobHeader.ProcessHeaders.AddNew();
			var childWorklow = jobHeader.ProcessHeaders.AddNew();

			preReqWorkflow.GetOrCreateDependencyLink(currentWorkflow);
			currentWorkflow.GetOrCreateDependencyLink(postReqWorkflow);
			childWorklow.GetOrCreateLinkToParent(currentWorkflow);

			var task1 = job.WorkflowItems.Tasks.AddNew();
			var task2 = job.WorkflowItems.Tasks.AddNew();
			var task3 = job.WorkflowItems.Tasks.AddNew();
			var task4 = job.WorkflowItems.Tasks.AddNew();
			var task5 = job.WorkflowItems.Tasks.AddNew();
			var task6 = job.WorkflowItems.Tasks.AddNew();
			var task7 = job.WorkflowItems.Tasks.AddNew();

			task1.P9_GS_NKAssignedStaffMember = "";
			task2.P9_GS_NKAssignedStaffMember = "";
			task3.P9_GS_NKAssignedStaffMember = "";
			task4.P9_GS_NKAssignedStaffMember = "";
			task5.P9_GS_NKAssignedStaffMember = "";
			task6.P9_GS_NKAssignedStaffMember = "";
			task7.P9_GS_NKAssignedStaffMember = "";

			task1.P9_FH_ProcessHeader = currentWorkflow.PK;
			task2.P9_FH_ProcessHeader = currentWorkflow.PK;
			task3.P9_FH_ProcessHeader = currentWorkflow.PK;
			task4.P9_FH_ProcessHeader = preReqWorkflow.PK;
			task5.P9_FH_ProcessHeader = postReqWorkflow.PK;
			task6.P9_FH_ProcessHeader = childWorklow.PK;
			task7.P9_FH_ProcessHeader = currentWorkflow.PK;

			task1.P9_Type = "INV";
			task2.P9_Type = "CDU";
			task3.P9_Type = "CDF";
			task4.P9_Type = "CDU";
			task5.P9_Type = "CDU";
			task6.P9_Type = "CDF";
			task7.P9_Type = "UDF";

			return new List<ProcessTask>() { task1, task2, task3, task4, task5, task6, task7 };
		}

		#endregion

		#region TagLinks

		public void TestTagLinkCollection()
		{
			var link = (ITagLink)((ITagBindable)ProcessTask).TagLinks_ForBinding.AddNew();

			AssertEquals(ProcessTask.PK, link.TGL_ParentId);
		}

		#endregion

		#region Universal Triggers

		public void TestCopyUniversalTemplateShouldCopyTriggers()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			universalTemplate.P0_Name = "UniversalTemplate";

			var templateTrigger = (IBaseTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger.Description = "Universal Trigger";
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			ProcessTaskNotification action = (ProcessTaskNotification)templateTrigger.TriggerActions.AddNew();
			action.PQ_TriggerType = "NTF";
			action.PQ_TriggerParty = "EML";
			action.PQ_EmailText = "Some Text";
			action.PQ_EmailAddr = "test@test.com";
			Factory.Save();

			var copiedTemplate = (ProcessTaskTemplate)((ITemplateCopyable)universalTemplate).TemplateCopy();
			copiedTemplate.P0_Name = "CopiedUniversalTemplate";
			Factory.Save();

			AssertEquals(1, copiedTemplate.TemplateTriggers.Count);
			AssertEquals("Universal Trigger", copiedTemplate.TemplateTriggers[0].Description);
			AssertEquals(1, copiedTemplate.TemplateTriggers[0].TriggerActions.Count);
		}

		[TestDate(2015, 7, 14)]
		public void TestMixOfStandardAndUniversalTriggers_ShouldGhostOntoJob()
		{
			var standardTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			standardTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);

			var templateTrigger1 = (IBaseTrigger)standardTemplate.WorkflowItems.Triggers.AddNew();
			var templateTrigger2 = (IBaseTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponentsCode;

			templateTrigger1.Description = "Tagged";
			templateTrigger2.Description = "Transferred";

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();

			var jobTrigger1 = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger1.Identifier);
			var jobTrigger2 = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger2.Identifier);

			AssertEquals(standardTemplate.PK, jobTrigger1.SourceTemplatePK);
			AssertEquals(standardTemplate.P0_Name, jobTrigger1.SourceTemplateName);

			AssertEquals(templateTrigger1, jobTrigger1.TemplateVersion);
			AssertEquals(false, jobTrigger1.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(false, jobTrigger1.ReadOnly);

			AssertEquals(universalTemplate.PK, jobTrigger2.SourceTemplatePK);
			AssertEquals(universalTemplate.P0_Name, jobTrigger2.SourceTemplateName);
			AssertEquals(templateTrigger2, jobTrigger2.TemplateVersion);
			AssertEquals(true, jobTrigger2.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(true, jobTrigger2.ReadOnly);

			Factory.Save();

			AssertEquals("Triggers coped from workflow template versions should be saved to the db", true, jobTrigger1.IsInDatabase);
			AssertEquals("Ghosted triggers representing workflow template versions should not be saved to the db", false, jobTrigger2.IsInDatabase);

			job.GetLogs().AddNew(Events.TagWasAddedOrRemoved);
			job.GetLogs().AddNew(Events.WorkflowTransferredBetweenSystemComponents);

			AssertEquals(new ZDateTime(2015, 7, 14), jobTrigger1.P9_ActualDate);
			AssertEquals(new ZDateTime(2015, 7, 14), jobTrigger2.P9_ActualDate);
			AssertEquals("Ghosted triggers cannot be deleted.", false, jobTrigger2.CanDelete);
		}

		[TestDate(2015, 7, 14)]
		public void TestUniversalTriggers_ShouldGhostOntoJob_WhenTriggersDoNotMatch()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			universalTemplate.P0_Name = "Match game";

			var templateTrigger1 = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			var templateTrigger2 = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponentsCode;

			templateTrigger1.Description = "Tagged";
			templateTrigger2.Description = "Transferred";

			templateTrigger2.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger2.TemplateCondition2Value = "\"<Z0_VarCharMax>\"==\"It's a match!\"";

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			job.Z0_VarCharMax = "Don't match yet";

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);

			var jobTrigger1 = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger1.Identifier);

			AssertEquals(universalTemplate.PK, jobTrigger1.SourceTemplatePK);
			AssertEquals(universalTemplate.P0_Name, jobTrigger1.SourceTemplateName);
			AssertEquals(templateTrigger1, jobTrigger1.TemplateVersion);
			AssertEquals(true, jobTrigger1.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(true, jobTrigger1.ReadOnly);
			AssertEquals(false, jobTrigger1.IsInDatabase);

			job.Z0_VarCharMax = "It's a match!";
			Factory.Save();
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(2, job.WorkflowItems.TriggersIncludingRelated.Count);

			var jobTrigger2 = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger2.Identifier);

			AssertEquals(universalTemplate.PK, jobTrigger2.SourceTemplatePK);
			AssertEquals(universalTemplate.P0_Name, jobTrigger2.SourceTemplateName);
			AssertEquals(templateTrigger2, jobTrigger2.TemplateVersion);
			AssertEquals(true, jobTrigger2.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(true, jobTrigger2.ReadOnly);
			AssertEquals(false, jobTrigger2.IsInDatabase);

			job.Z0_VarCharMax = "Aww no match";
			Factory.Save();
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(false, jobTrigger1.IsDeleted);
			AssertEquals(true, jobTrigger2.IsDeleted);
			AssertEquals(1, job.WorkflowItems.Triggers.Count);

			templateTrigger1.IsActive = false;

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			AssertEquals(true, jobTrigger1.IsDeleted);
			AssertEquals(0, job.WorkflowItems.Triggers.Count);
		}

		public void TestUniversalTriggers_WhenGhostedOntoJob_WithMultipleTriggerActions_ShouldNotAppearInMultipleCollections()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);

			var trigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			trigger.Description = "Previously on Battlestar Galactica";
			trigger.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponentsCode;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction.PQ_FieldName = "<O1_City>";
			triggerAction.PQ_FieldValue = "Caprica";

			Factory.Save();

			for (var i = 0; i < 10; i++)
			{
				var job = Factory.NewWithValidTestData<SalesEnquiry>();
				AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			}

			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(1, trigger.CompletionTriggerActionsCollection().Count);
		}

		public void TestFieldValueClonedOnUniversalTriggers()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);

			var trigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			trigger.Description = "TRIGGER";
			trigger.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponentsCode;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoPack;
			triggerAction.PQ_FieldValue = "FieldValue";

			var triggerAction1 = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction1.PQ_FieldName = "<O1_City>";
			triggerAction1.PQ_FieldValue = "FieldValue";

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);

			var actions = job.WorkflowItems.Triggers[0].ProcessTaskNotifications;
			AssertEquals(2, actions.Count);
			AssertEquals("PQ_FieldValue is cloned for SetField trigger", 1, actions.Count(a => a.PQ_TriggerType == "FLD" && a.PQ_FieldValue == "FieldValue"));
			AssertEquals("PQ_FieldValue is not cloned for non SetField trigger", 1, actions.Count(a => a.PQ_TriggerType == "APK" && a.PQ_FieldValue == ""));
		}

		public void TestSourceTemplateNameCanBeUsedForConditionMarcoInTriggerActions()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = "TesterTemplate";
			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "TaskByTemplate";
			task.P9_Sequence = 1;
			task.P9_Type = "UDF";

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			triggerAction.PQ_FieldName = @"<WorkflowItems.Where(""<SourceTemplateName>"" == ""TesterTemplate"").P9_Status>";
			triggerAction.PQ_FieldValue = "CLS";

			Factory.Save();

			var job = Factory.New<Forwarding.IForwardingShipment>();
			job.JS_UniqueConsignRef = "S0001";

			var jobWithWorkflow = (IWorkflowProvider)job;

			Factory.Save();

			AssertEquals(1, jobWithWorkflow.WorkflowItems.Tasks.Count);
			AssertEquals(task.P9_Description, jobWithWorkflow.WorkflowItems.Tasks[0].P9_Description);
			AssertEquals("ASN", jobWithWorkflow.WorkflowItems.Tasks[0].P9_Status);
			AssertEquals(template.PK, jobWithWorkflow.WorkflowItems.Tasks[0].SourceTemplatePK);
			AssertEquals(template.P0_Name, jobWithWorkflow.WorkflowItems.Tasks[0].SourceTemplateName);

			var untemplatedTask = jobWithWorkflow.WorkflowItems.Tasks.AddNew();
			untemplatedTask.P9_Description = "UntemplatedTask";
			untemplatedTask.P9_Sequence = 2;
			untemplatedTask.P9_Type = "UDF";

			jobWithWorkflow.Logs.AddNew(Events.CustomisableEvent00);

			Factory.Save();

			AssertEquals(2, jobWithWorkflow.WorkflowItems.Tasks.Count);
			AssertEquals("CLS", jobWithWorkflow.WorkflowItems.Tasks.Cast<ProcessTask>().First(t => t.P9_Sequence == 1).P9_Status);
			AssertEquals("ASN", jobWithWorkflow.WorkflowItems.Tasks.Cast<ProcessTask>().First(t => t.P9_Sequence == 2).P9_Status);
		}

		public void TestUniversalTriggers_WithConditionsNowMatching_ShouldApplyDuringFactorySave()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);

			var trigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			trigger.Description = "Previously on Battlestar Galactica";
			trigger.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponentsCode;
			trigger.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger.TemplateCondition2Value = "\"<O1_EnquiryType>\"==\"BSG\"";

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction.PQ_FieldName = "<O1_City>";
			triggerAction.PQ_FieldValue = "Caprica";

			var job = Factory.New<SalesEnquiry>();
			job.O1_EnquiryType = "GAL";

			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Triggers.Count);

			job.O1_EnquiryType = "BSG";
			Factory.Save();

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);

			var jobTrigger = job.WorkflowItems.TriggersIncludingRelated[0];
			AssertEquals(trigger, jobTrigger.TemplateVersion);

			AssertEquals(1, jobTrigger.CompletionTriggerActionsCollection().Count);

			var jobTriggerAction = jobTrigger.CompletionTriggerActionsCollection()[0];
			AssertEquals(WorkflowTriggerActionTypeConstants.Codes.SetField, jobTriggerAction.PQ_TriggerType);
			AssertEquals("<O1_City>", jobTriggerAction.PQ_FieldName);
			AssertEquals("Caprica", jobTriggerAction.PQ_FieldValue);
			AssertEquals(jobTrigger.PK, jobTriggerAction.PQ_P9);
			AssertEquals(ZGuid.Empty, jobTriggerAction.PQ_P9T_Trigger);

			AssertEquals(false, jobTriggerAction.IsInDatabase);
			AssertEquals(true, jobTriggerAction.ReadOnly);
			AssertEquals(true, jobTrigger.CompletionTriggerActionsCollection().ReadOnly);
		}

		public void TestUniversalTriggerTemplateVersionIsNullForDeleteCheck()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			var templateTrigger1 = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger1.Description = "Tagged";
			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			templateTrigger1.Delete();

			var jobTrigger1 = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().First();
			AssertNull(jobTrigger1.TemplateVersion);
			AssertNoExceptionThrown(() =>
			{
				var deletableTarget = (ICanDelete)jobTrigger1;
				_ = deletableTarget.ReasonForNotAbleToDelete;
			});
		}

		public void TestUniversalTriggers_ShouldApplyRealTriggers_EvenWhenUniversalTriggersExist()
		{
			var standardTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			standardTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			standardTemplate.P0_SubType1 = "COL";

			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);

			var templateTrigger1 = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			var templateTrigger2 = (ITemplateTrigger)standardTemplate.WorkflowItems.Triggers.AddNew();
			templateTrigger1.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger2.TriggerConditions_ForBinding.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponentsCode;

			templateTrigger1.Description = "Tagged";
			templateTrigger2.Description = "Transferred";

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			AssertEquals(false, job.HasChanges);
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			AssertEquals(false, job.HasChanges);
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);

			var jobTrigger1 = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger1.Identifier);

			AssertEquals(universalTemplate.PK, jobTrigger1.SourceTemplatePK);
			AssertEquals(universalTemplate.P0_Name, jobTrigger1.SourceTemplateName);
			AssertEquals(templateTrigger1, jobTrigger1.TemplateVersion);
			AssertEquals(true, jobTrigger1.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(true, jobTrigger1.ReadOnly);
			AssertEquals(false, jobTrigger1.IsInDatabase);

			job.SubType1 = "COL";
			job.ApplyWorkflowTemplates();
			Factory.Save();

			AssertEquals(2, job.WorkflowItems.TriggersIncludingRelated.Count);

			var jobTrigger2 = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger2.Identifier);

			AssertEquals(standardTemplate.PK, jobTrigger2.SourceTemplatePK);
			AssertEquals(standardTemplate.P0_Name, jobTrigger2.SourceTemplateName);
			AssertEquals(templateTrigger2, jobTrigger2.TemplateVersion);
			AssertEquals(false, jobTrigger2.IsNonPersistedRepresentationOfTemplateTrigger);
			AssertEquals(false, jobTrigger2.ReadOnly);
			AssertEquals(true, jobTrigger2.IsInDatabase);
		}

		public void TestUniversalTriggers_ShouldApplyRealTriggers_EvenWhenUniversalTriggersExist_MultiCompanyContext()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;

			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.GlobalTemplate = false;
			template.P0_GC = Env.CurrentCompanyPK;
			template.P0_GB = Env.CurrentBranchPK;
			var templateTrigger = (ITemplateTrigger)template.WorkflowItems.Triggers.AddNew();
			templateTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger.Description = "Standard Trigger";

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();
			Factory.Save();
			Assert(job.WorkflowItems.Triggers.Cast<ProcessTask>().Any(trigger => trigger.P9_Description == "Standard Trigger"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ukBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var otherCompanyTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
				otherCompanyTemplate.GlobalTemplate = false;
				otherCompanyTemplate.P0_Name = "Dum2";
				otherCompanyTemplate.P0_GC = ukCompany.PK;
				otherCompanyTemplate.P0_GB = ukBranch.PK;
				var otherCompanyTemplateTrigger = (ITemplateTrigger)otherCompanyTemplate.WorkflowItems.Triggers.AddNew();
				otherCompanyTemplateTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
				otherCompanyTemplateTrigger.Description = "XX Trigger";

				var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
				universalTemplate.GlobalTemplate = false;
				universalTemplate.P0_GC = ukCompany.PK;
				universalTemplate.P0_GB = ukBranch.PK;
				var universalTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
				universalTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
				universalTrigger.Description = "Universal Trigger";
				Factory.Save();

				// This is key to this test. In a real job, the universal triggers would be loaded before save, this causes this to happen
				Assert(job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Any(trigger => trigger.P9_Description == "Universal Trigger"));

				job.Z0_Short = 5;
				job.ApplyWorkflowTemplates();
				Factory.Save();

				Factory.ReloadAll<DummyWithWorkflow>();
				Assert(job.WorkflowItems.Triggers.Cast<ProcessTask>().Any(trigger => trigger.P9_Description == "Standard Trigger"));
				Assert(job.WorkflowItems.Triggers.Cast<ProcessTask>().Any(trigger => trigger.P9_Description == "XX Trigger"));
				Assert(job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Any(trigger => trigger.P9_Description == "Universal Trigger"));
			}
		}

		[TestDate(2015, 7, 14)]
		public void TestApplyUniversalTrigger_WhenEventAlreadyExists_ShouldNotSetActualDate()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			var templateTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger.Description = "Tagged";
			templateTrigger.IsActive = false;

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			job.Logs.AddNew(Events.TagWasAddedOrRemoved);

			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			Factory.Save();

			AssertEquals(0, job.WorkflowItems.Triggers.Count);

			templateTrigger.IsActive = true;
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(ZDateTime.Empty, job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate);

			job.Logs.AddNew(Events.TagWasAddedOrRemoved);
			AssertEquals(new ZDateTime(2015, 7, 14), job.WorkflowItems.TriggersIncludingRelated[0].P9_ActualDate);
		}

		public void TestUniversalTriggerOnJob_ShouldNotBeDeletable()
		{
			var universalTemplate = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			universalTemplate.P0_Name = "Dummy Universal Template";

			var templateTrigger = (ITemplateTrigger)universalTemplate.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger.Description = "Tagged";

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			AssertEquals(false, job.WorkflowItems.TriggersIncludingRelated[0].CanDelete);
		}

		#endregion

		#region Implementation

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;

		ProcessTaskTemplate GetNewWorkflowTemplate()
		{
			var workflowTemplate = new BusinessObjectFactory().NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_ProcessType = Dummy.WorkflowItems.WorkflowType;
			return workflowTemplate;
		}

		DummyProcessTask ProcessTask
		{
			get
			{
				if (processTask == null)
				{
					processTask = Dummy.WorkflowItems.AddNew();
				}
				return processTask;
			}
		}

		DummyProcessTask processTask;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Dummy.WorkflowItems.AddNew();
		}

		EnableAddEditAndDeleteLogsItemCollection GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(bool value)
		{
			var tempValues = new EnableAddEditAndDeleteLogsItemCollection();
			tempValues.Add(new EnableAddEditAndDeleteLogsItem()
			{
				Table = ProcessTasksSchema.Constants.TableName,
				EnableADDLogs = value,
				EnableEDTLogs = value,
				EnableDELLogs = value,
			});
			return tempValues;
		}

		public static void UpdateWeekDaysTo9To5()
		{
			UpdateDaysTo9To5(
				DayOfWeek.Monday,
				DayOfWeek.Tuesday,
				DayOfWeek.Wednesday,
				DayOfWeek.Thursday,
				DayOfWeek.Friday);
		}

		static void UpdateDaysTo9To5(params DayOfWeek[] days)
		{
			foreach (var day in days)
			{
				UpdateDay(day, WorkingDaysTestHelper.NineToFive);
			}
		}

		static void UpdateDay(DayOfWeek day, string workingHours)
		{
			var intervals = WorkingDaysTestHelper.ConvertWorkingHoursToIntervals(workingHours, day);

			string deleteCommandText = string.Format(
			   "DELETE FROM {0} WHERE {1} = '{2}' AND {3} = '{4}' AND {5}='{6}'",
			   /*0*/GlbWorkTimeSchema.Constants.TableName,
			   /*1*/GlbWorkTimeSchema.Constants.GW_DayOfWeek,
			   /*2*/day,
			   /*3*/GlbWorkTimeSchema.Constants.GW_ParentID,
			   /*4*/GlbDepartment.CurrentDepartment.PK.ToGuid(),
			   /*5*/GlbWorkTimeSchema.Constants.GW_ParentTableCode,
			   /*6*/GlbDepartmentSchema.Constants.Prefix);
			var deleteCommand = Db.Connection.Command(deleteCommandText);
			deleteCommand.ExecuteScalar();

			foreach (var interval in intervals)
			{
				AddWorkTimeForCurrentDepartment(interval.DayCode, interval.Interval.StartTime, interval.Interval.EndTime);
			}
		}

		static void AddWorkTimeForCurrentDepartment(string dayCode, ZDateTime startTime, ZDateTime endTime)
		{
			var sql = @"
INSERT dbo.GlbWorkTime (GW_PK, GW_ParentID, GW_ParentTableCode, GW_DayOfWeek, GW_StartTime, GW_EndTime, GW_SystemCreateTimeUtc, GW_SystemCreateUser, GW_SystemLastEditTimeUtc, GW_SystemLastEditUser) 
VALUES (@workTimePK, @departmentPK, @table, @day, @startTime, @endTime, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@workTimePK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, GlbDepartment.CurrentDepartment.PK.ToGuid());
				command.AddParameter("@table", SqlDbType.Char, GlbWorkTimeSchema.GW_ParentTableCode.MaxLength, GlbDepartmentSchema.Constants.Prefix);
				command.AddParameter("@day", SqlDbType.Char, GlbWorkTimeSchema.GW_DayOfWeek.MaxLength, dayCode);
				command.AddParameter("@startTime", SqlDbType.SmallDateTime, startTime.ToDateTime());
				command.AddParameter("@endTime", SqlDbType.SmallDateTime, endTime.ToDateTime());
				command.ExecuteNonQuery();
			}
		}

		IBMTestHelper BMTestHelper
		{
			get { return bmTestHelper ?? (bmTestHelper = ObjectFactory.Get<IBMTestHelper>()); }
		}

		IBMTestHelper bmTestHelper;

		protected override void SetUp()
		{
			base.SetUp();
			originalTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			Disposables = new DisposableList(10);
		}

		protected override void TearDown()
		{
			base.TearDown();
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalTaskTypes);
			bmTestHelper = null;

			if (Disposables != null)
			{
				Disposables.Dispose();
				Disposables = null;
			}
		}
		DisposableList Disposables { get; set; }
		CategorisedWorkflowTaskTypesCollection originalTaskTypes;

		#endregion

		#region SourceTemplatePK

		public void TestSourceTemplatePK()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals("SourceTemplatePK is empty if not applied from template", ZGuid.Empty, task.SourceTemplatePK);

			task.P9_ParentTemplateID = ZGuid.Invalid;
			AssertEquals("SourceTemplatePK is also invalid when ParentTemplateId is invalid", ZGuid.Invalid, task.SourceTemplatePK);

			task.P9_ParentTemplateID = ZGuid.NewZGuid();
			AssertEquals("SourceTemplatePK is also invalid when unable to load ProcessTaskTemplate", ZGuid.Invalid, task.SourceTemplatePK);
		}

		public void TestSourceTemplateName()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals("SourceTemplateName is empty if not applied from template", ZString.Empty, task.SourceTemplateName);

			task.P9_ParentTemplateID = ZGuid.Invalid;
			AssertEquals("SourceTemplateName is empty when ParentTemplateId is invalid", ZString.Empty, task.SourceTemplateName);

			task.P9_ParentTemplateID = ZGuid.NewZGuid();
			AssertEquals("SourceTemplateName is empty when unable to load ProcessTaskTemplate", ZString.Empty, task.SourceTemplateName);
		}

		#endregion

		#region Setup

		DummyWithWorkflow GetDummy() => Factory.NewWithValidTestData<DummyWithWorkflow>();

		ProcessTask GetMilestone(IWorkflowProvider workflowProvider) => workflowProvider.WorkflowItems.Milestones.AddNew();

		#endregion

		#region Regression Tests

		public void TestGenerateReferenceForNewLog_AllowDuplicateParameters()
		{
			var dummy = GetDummy();
			var milestone = GetMilestone(dummy);
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			milestone.TriggerConditions.TriggerConditionValue = "|NEW=1|NEW=2";

			milestone.P9_ScheduledDateForBinding = ZDateTimeOffset.Now;
			AssertEquals("|NEW=1|TYP=Z00", dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.EstimatedDateChangedCode).Single().SL_Reference);
		}

		public void TestDeletingTaskLog()
		{
			var dummy = GetDummy();
			var milestone = GetMilestone(dummy);
			var log = milestone.Logs.AddNew(Events.CustomisableEvent00);
			milestone.Delete();
			AssertNoExceptionThrown(() => log.Delete());
		}

		#endregion

		#region Conditions with Macros

		[TestDate(2019, 1, 1)]
		public void TestTriggerParentMacroAlias_WhenEventRaisedOnDifferentBusinessObjects_ShouldEvaluateConditionAgainstTriggerParent_MCR()
		{
			CheckTriggerParentMacroAlias_WhenEventRaisedOnDifferentBusinessObjects_ShouldEvaluateConditionAgainstTriggerParent(EventReferenceConditionList.Codes.ConditionWithMacros, "TriggerSource.Z0_Description == \"Ivory supplies\"");
		}

		[TestDate(2019, 1, 1)]
		public void TestTriggerParentMacroAlias_WhenEventRaisedOnDifferentBusinessObjects_ShouldEvaluateConditionAgainstTriggerParent_UDF()
		{
			CheckTriggerParentMacroAlias_WhenEventRaisedOnDifferentBusinessObjects_ShouldEvaluateConditionAgainstTriggerParent(EventReferenceConditionList.Codes.UserDefined, "\"<Z0_Description>\" == \"Ivory supplies\"");
		}

		public void CheckTriggerParentMacroAlias_WhenEventRaisedOnDifferentBusinessObjects_ShouldEvaluateConditionAgainstTriggerParent(string triggerCondition, string triggerConditionValue)
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "DUM");

			var job = Factory.New<DummyWithWorkflow>();
			var trigger = MasterFilesTestHelper.CreateTrigger(job, Events.TagWasAddedOrRemoved, triggerCondition: triggerCondition, triggerConditionValue: triggerConditionValue);
			var jobHeader = (BusinessObject)ProcessJobHeaderProvider.GetForParent(job, Factory);

			AssertNotNull(jobHeader);

			Factory.Save();

			job.GetLogs().AddNew(Events.TagWasAddedOrRemoved);
			jobHeader.GetLogs().AddNew(Events.TagWasAddedOrRemoved);

			AssertEquals(ZDateTime.Empty, trigger.P9_ActualDate);

			job.Z0_Description = "Ivory supplies";

			job.GetLogs().AddNew(Events.TagWasAddedOrRemoved);
			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);

			TestDateAttribute.AddHours(1);

			job.GetLogs().AddNew(Events.TagWasAddedOrRemoved);
			AssertEquals(ZDateTime.Now, trigger.P9_ActualDate);
		}

		[TestDate(2019, 1, 1)]
		public void TestTriggerConditionEvaluator_ValidateZBoolProperty_LikeBoolean_MCR()
		{
			CheckTriggerConditionEvaluator_ValidateZBoolProperty(EventReferenceConditionList.Codes.ConditionWithMacros, "TriggerSource.Z0_IsSystem", "!TriggerSource.Z0_IsSystem");
		}

		[TestDate(2019, 1, 1)]
		public void TestTriggerConditionEvaluator_ValidateZBoolProperty_LikeYN_UDF()
		{
			CheckTriggerConditionEvaluator_ValidateZBoolProperty(EventReferenceConditionList.Codes.UserDefined, "\"<Z0_IsSystem>\" == \"Y\"", "\"<Z0_IsSystem>\" == \"N\"");
		}

		public void CheckTriggerConditionEvaluator_ValidateZBoolProperty(string triggerCondition, string trueTriggerConditionValue, string falseTriggerConditionValue)
		{
			var job = Factory.New<DummyWithWorkflow>();
			var trueConditionTrigger = MasterFilesTestHelper.CreateTrigger(job, Events.AttachedCode, triggerCondition: triggerCondition, triggerConditionValue: trueTriggerConditionValue);
			var falseConditionTrigger = MasterFilesTestHelper.CreateTrigger(job, Events.AttachedCode, triggerCondition: triggerCondition, triggerConditionValue: falseTriggerConditionValue);

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("true condition trigger", ZDateTime.Empty, trueConditionTrigger.P9_ActualDate);
				AssertEquals("false condition trigger", ZDateTime.Empty, falseConditionTrigger.P9_ActualDate);
			});

			job.Z0_IsSystem = true;
			job.GetLogs().AddNew(Events.Attached);
			CombineAssertions("WHEN Z0_IsSystem  = true", () =>
			{
				AssertEquals("true condition trigger", ZDateTime.Now, trueConditionTrigger.P9_ActualDate);
				AssertEquals("false condition trigger", ZDateTime.Empty, falseConditionTrigger.P9_ActualDate);
			});

			var startDate = ZDateTime.Now;
			TestDateAttribute.AddDays(1);
			AssertEquals("Now", startDate.AddDays(1), ZDateTime.Now);

			job.Z0_IsSystem = false;
			job.GetLogs().AddNew(Events.Attached);
			CombineAssertions("WHEN Z0_IsSystem  = false", () =>
			{
				AssertEquals("true condition trigger", startDate, trueConditionTrigger.P9_ActualDate);
				AssertEquals("false condition trigger", ZDateTime.Now, falseConditionTrigger.P9_ActualDate);
			});
		}

		public void TestChangeMilestoneActualDateModifiesOldLog()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			milestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			milestone.TriggerConditions.TriggerConditionValue = "FAC=1";
			var start = ZDateTimeOffset.Now;
			milestone.P9_ActualDateForBinding = start.AddDays(-10);

			var logs = dummy.Logs.LogsNotInDB.Where(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent00.Code).ToArray();
			AssertEquals(1, logs.Length);
			AssertEquals(logs[0].EventTimeOffset, start.AddDays(-10));
			milestone.P9_ActualDateForBinding = start.AddDays(-5);

			logs = dummy.Logs.LogsNotInDB.Where(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent00.Code).ToArray();
			AssertEquals(1, logs.Length);
			AssertEquals(logs[0].EventTimeOffset, start.AddDays(-5));
		}

		#endregion

		#region DB Hit test

		public void TestDeleteDBHits()
		{
			var task1 = Factory.NewWithValidTestData<ProcessTask>();
			var task2 = Factory.NewWithValidTestData<ProcessTask>();
			var task3 = Factory.NewWithValidTestData<ProcessTask>();
			var task4 = Factory.NewWithValidTestData<ProcessTask>();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedTasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, new[] { task1.PK, task2.PK, task3.PK, task4.PK }));

			loadedTasks.ForEach(l => l.FetchStrategy.FetchForDelete());
			loadedTasks.ForEach(l => l.Delete());

			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessTaskIterationLinkSchema.Constants.TableName, 1 },
				{ ProcessTaskIterationLinkPivotSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ StmDocDataOverrideSchema.Constants.TableName, 2 },
				{ StmNoteSchema.Constants.TableName, 2 },
				{ StmUniversalCopySchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		public void TestDelete_WhenItIsPAVE()
		{
			var tempValue = GetEnableAddEditAndDeleteLogsItemsRegistryItemValue(true);

			using (SystemDataRegistry.Instance.EnableAddEditAndDeleteLogsItemsRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempValue))
			{
				ProcessTask MakeTask(ProcessHeader processHeader)
				{
					var task = processHeader.TaskCollection.AddNew();
					task.P9_Description = "T" + task.P9_Sequence;
					return task;
				}
				ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
				ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "DUM");

				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);
				var header1 = jobHeader.ProcessHeaders.AddNew();
				header1.FH_CompletionStatement = "No bags";
				var header2 = jobHeader.ProcessHeaders.AddNew();
				header2.FH_CompletionStatement = "Yes bags";
				var task1 = MakeTask(header1);
				var task2 = MakeTask(header1);
				var task3 = MakeTask(header2);
				var task4 = MakeTask(header2);

				Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var loadedTasks = newFactory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, new[] { task1.PK, task2.PK, task3.PK, task4.PK }));

				loadedTasks.ForEach(l => l.FetchStrategy.FetchForDelete());
				loadedTasks.ForEach(l => l.Delete());

				AssertDbHits(new Dictionary<string, int>
				{
					{ DummyBizoSchema.Constants.TableName, 1 },
					{ ProcessHeaderSchema.Constants.TableName, 1 },
					{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
					{ ProcessTaskIterationLinkSchema.Constants.TableName, 1 },
					{ ProcessTaskIterationLinkPivotSchema.Constants.TableName, 1 },
					{ ProcessTasksSchema.Constants.TableName, 4 }, // A hit for finding triggers in the same job for the DEL event, and another one for finding tasks in the same workflow, and one more because some fetch hints use P9_ParentID without P9_ParentTableCode. Sad!
					{ StmDocDataOverrideSchema.Constants.TableName, 2 },
					{ StmNoteSchema.Constants.TableName, 2 },
					{ StmUniversalCopySchema.Constants.TableName, 1 },
					{ TagLinkSchema.Constants.TableName, 1 },
				}, newFactory);
			}
		}

		#endregion

		#region Readonly

		public void TestCardNoteIsReadonlyForTriggersAndMilestones()
		{
			var task = Factory.New<ProcessTask>();
			var trigger = Factory.New<ProcessTask>();
			var milestone = Factory.New<ProcessTask>();
			var exception = Factory.New<ProcessTask>();

			task.P9_Type = "UDF";
			trigger.P9_Type = "TRG";
			milestone.P9_Type = "MIL";
			exception.P9_Type = "EXC";

			AssertEquals("Tasks are not readonly", false, task.P9_CardNoteInfo.ReadOnly);
			AssertEquals("Triggers are readonly", true, trigger.P9_CardNoteInfo.ReadOnly);
			AssertEquals("Milestones are readonly", true, milestone.P9_CardNoteInfo.ReadOnly);
			AssertEquals("Exceptions are not readonly", false, exception.P9_CardNoteInfo.ReadOnly);
		}

		#endregion

		#region TaskID

		public void TestTaskNumberFountain_OnlyDoOne()
		{
			Enumerable.Range(0, 100).ForEach(n => Factory.New<ProcessTask>().P9_Description = "" + n);
			Factory.Save();
			AssertEquals(1, NumberFountainProxy.NumberOfFountainCommands_ForTest.Value);
		}

		public void TestTaskNumberFountain_RollbackOnConcurrency()
		{
			// In this test, we are proving that if save fails, we reset the value back to zero.
			var task = Factory.New<ProcessTask>();
			Factory.Save();
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loaded = newFactory.Load<ProcessTask>(task.PK);
			loaded.P9_Description = "Thankyou";
			newFactory.Save();
			task.P9_Description = "NoThankYou";
			var task2 = Factory.New<ProcessTask>();
			AssertEquals("Pre condition", ZString.Empty, task2.P9_TaskID);
			AssertNotEquals("Pre condition", ZString.Empty, task.P9_TaskID);
			AssertExceptionThrown<ZSaveConcurrencyException>(Factory.Save);
			AssertEquals("Roll back setting the task2 id", ZString.Empty, task2.P9_TaskID);
			AssertNotEquals(ZString.Empty, task.P9_TaskID);
		}

		public void TestTaskNumberFountain_Lossy()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals(false, ((INumberFountainConsumer)task).Fountain.EnsureConsistentSequence);
		}

		#endregion

		#region Universal Copy

		public void TestUniversalCopy()
		{
			var workItem = Factory.NewWithValidTestData<OrgHeader>();

			var task = workItem.WorkflowItems.AddNew();
			task.P9_Description = "Description";
			task.P9_Type = "UDF";
			task.P9_Status = "OPN";

			Factory.Save();

			AssertEquals(OrgHeaderSchema.Constants.Prefix, workItem.WorkflowItems[0].P9_ParentTableCode);

			var processTaskNode = new EntityCopyTemplateNode { Name = "ProcessTask" };
			processTaskNode.Nodes.Add(new PropertyCopyTemplateNode { Name = ProcessTasksSchema.Constants.P9_Description, CopyMethod = CopyMethod.Copy });
			processTaskNode.Nodes.Add(new PropertyCopyTemplateNode { Name = ProcessTasksSchema.Constants.P9_Status, CopyMethod = CopyMethod.Copy });

			var cusEntryNumbersNode = new CollectionCopyTemplateNode
			{
				Name = "ProcessTasks",
				ItemPropertyName = "ProcessTasks",
				ItemsTableName = ProcessTasksSchema.Constants.TableName,
				InnerNode = processTaskNode,
				CopyMethod = CollectionCopyMethod.All
			};

			var copyTree = new CopyTemplateTree { InnerNode = processTaskNode };

			var copyManager = new BusinessObjectCopyManager();

			var copiedProcessTask = copyManager.Copy(task, copyTree).Object as ProcessTask;
			AssertNotNull("New ProcessTask created by copy BusinessObjectCopyManager", copiedProcessTask);
			AssertEquals("Description", copiedProcessTask.P9_Description);
			AssertEquals("OPN", copiedProcessTask.P9_Status);
			AssertEquals(OrgHeaderSchema.Constants.Prefix, copiedProcessTask.P9_ParentTableCode);
		}

		public void TestUniversalCopyIgnoreElement()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			var componentType = task.GetType();
			var ignoreElementAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true);
			var attribute = ignoreElementAttributes[0] as UniversalCopyIgnoreElementAttribute;
			AssertCollectionContains("P9_TaskID", ProcessTasksSchema.Constants.P9_TaskID, attribute.ElementNames);
		}

		public void TestUniversalCopyWithParentTemplateID()
		{
			WorkflowDataRegistry.Instance.EnableTemplateApplicationConcurrencyProtection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TemplateApplicationRaceConditionHandlerOptions.Codes.UserInterfaceAndServiceTasks);

			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			var trigger = dummyWithWorkflow.WorkflowItems.Triggers.AddNew();
			var workflowTemplateID = Guid.NewGuid();
			trigger.P9_ParentTemplateID = workflowTemplateID;
			trigger.P9_Description = "Jango";
			Factory.Save();

			var entityNode = new EntityCopyTemplateNode();
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = ProcessTasksSchema.Constants.P9_ParentID, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = ProcessTasksSchema.Constants.P9_ParentTemplateID, CopyMethod = CopyMethod.Copy });
			entityNode.Nodes.Add(new PropertyCopyTemplateNode { Name = ProcessTasksSchema.Constants.P9_Description, CopyMethod = CopyMethod.Copy });
			var copyTree = new CopyTemplateTree { InnerNode = entityNode };

			var triggerCopy = (ProcessTask)new BusinessObjectCopyManager().Copy(trigger, copyTree).Object;
			AssertEquals(trigger.P9_Description, triggerCopy.P9_Description);

			Factory.Save();

			var filters = new ZQuery(ProcessTasksSchema.P9_ParentID, dummyWithWorkflow.PK);
			filters.AddToFilter(ProcessTasksSchema.P9_ParentTemplateID, workflowTemplateID);
			filters.AddToFilter(ProcessTasksSchema.P9_Description, trigger.P9_Description);
			var loadedTriggers = Factory.Load<ProcessTask>(filters);
			AssertEquals(2, loadedTriggers.Length);
		}

		#endregion

		#region Standalone Tasks

		[ExpectNoExceptions]
		public void TestStandaloneTasksMustNotHaveProcessHeader()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			var standaloneTask = job.WorkflowItems.AddNew();
			standaloneTask.P9_ParentID = ZGuid.Empty;
			standaloneTask.P9_FH_ProcessHeader = workflow.PK;

			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), "The INSERT statement conflicted with the CHECK constraint \"Constraint_TaskMustNotBeStandaloneAndHaveProcessHeader\"", true), "Saving a standalone task with process header should not be allowed.");
		}

		#endregion
	}
}
