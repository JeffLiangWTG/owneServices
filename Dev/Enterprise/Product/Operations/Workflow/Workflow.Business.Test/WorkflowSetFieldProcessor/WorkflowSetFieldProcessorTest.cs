using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Testing
{
	public class WorkflowSetFieldProcessorTest : TestCaseWithFactory
	{
		class Container<TPropertyOwner> : NonPersistentBusinessObject, IAllowMacroAccessToAllPublicProperties
			where TPropertyOwner : IPropertyOwner
		{
			public IPropertyOwner Owner { get; set; }
		}

		interface IPropertyOwner
		{
			ZString Data { get; }
		}

		class PropertyOwner1 : IPropertyOwner, IAllowMacroAccessToAllPublicProperties
		{
			public ZString Data { get; set; }
		}

		class PropertyOwner2 : IPropertyOwner, IAllowMacroAccessToAllPublicProperties
		{
			public ZString Data { get; set; }
		}

		public void TestMacroProcessingWorksWithDualImplementationsOnGenericTypes()
		{
			var macroProcessor = ObjectFactory.Get<ITextMacroProcessor>();
			var container1 = new Container<PropertyOwner1> { Owner = new PropertyOwner1 { Data = "1" } };

			AssertEquals("1", macroProcessor.Replace("<Owner.Data>", new IBusiness[] { container1 }, throwError: true));

			var container2 = new Container<PropertyOwner2> { Owner = new PropertyOwner2 { Data = "2" } };

			AssertEquals("2", macroProcessor.Replace("<Owner.Data>", new IBusiness[] { container2 }, throwError: true));
		}

		public void TestIFCTriggerWithMacroCaching()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var task1 = job.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "task1";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task2 = job.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "task2";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task3 = job.WorkflowItems.Tasks.AddNew();
			task3.P9_Description = "task3";
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "TR1";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task1\").P9_Status>";
			action.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.ServiceContainer.AddService(ObjectFactory.Get<ITextMacroProcessingCachingService>(nameof(ITextMacroProcessingCachingService)));

			job.Logs.AddNew(AutoEvents.CustomisableEvent00);

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task1.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task2.P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, task3.P9_Status);
		}

		public void TestTextMacroProcessingCachingServiceDisabled_NestedOperations()
		{
			var service = ObjectFactory.Get<ITextMacroProcessingCachingService>(nameof(ITextMacroProcessingCachingService));

			var key = "Property";
			void AssertNoCaching()
			{
				service.CacheMacroValue(key, "value1");
				AssertEquals("Macro value should not be cached", false, service.TryGetMacroValue(key, out _));
			}

			using (service.WithNoCaching())
			{
				AssertNoCaching();

				using (service.WithNoCaching())
				{
					AssertNoCaching();
				}

				AssertNoCaching();
			}

			service.CacheMacroValue(key, "value2");
			AssertEquals("Macro value should not be cached", true, service.TryGetMacroValue(key, out var result));
			AssertEquals("value2", result);
		}

		[TestDate(2020, 1, 1)]
		public void TestProcessTaskFLDTriggerActionsAreSetAfterValidation()
		{
			//Job1
			var job1 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var task1_1 = job1.WorkflowItems.Tasks.AddNew();
			task1_1.P9_Description = "task1";
			task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task1_2 = job1.WorkflowItems.Tasks.AddNew();
			task1_2.P9_Description = "task2";
			task1_2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task1_3 = job1.WorkflowItems.Tasks.AddNew();
			task1_3.P9_Description = "task3";
			task1_3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task1_4 = job1.WorkflowItems.Tasks.AddNew();
			task1_4.P9_Description = "task4";
			task1_4.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task1_4.P9_GS_NKAssignedStaffMember = "";

			var trigger1_1 = job1.WorkflowItems.Triggers.AddNew();
			trigger1_1.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger1_1.P9_Description = "Trigger me";

			var action1_1_1 = trigger1_1.ProcessTaskNotifications.AddNew();
			action1_1_1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1_1_1.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task1\").P9_Status>";
			action1_1_1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var trigger1_2 = job1.WorkflowItems.Triggers.AddNew();
			trigger1_2.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger1_2.P9_Description = "Trigger me";

			var action1_2_1 = trigger1_2.ProcessTaskNotifications.AddNew();
			action1_2_1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1_2_1.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task2\").P9_Status>";
			action1_2_1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var action1_2_2 = trigger1_2.ProcessTaskNotifications.AddNew();
			action1_2_2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1_2_2.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task3\").P9_Status>";
			action1_2_2.PQ_FieldValue = "XXX";

			var trigger1_3 = job1.WorkflowItems.Triggers.AddNew();
			trigger1_3.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger1_3.P9_Description = "Trigger me";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "SSS";

			var action1_3_1 = trigger1_3.ProcessTaskNotifications.AddNew();
			action1_3_1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1_3_1.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task4\").P9_Status>";
			action1_3_1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var action1_3_2 = trigger1_3.ProcessTaskNotifications.AddNew();
			action1_3_2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action1_3_2.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task4\").P9_GS_NKAssignedStaffMember>";
			action1_3_2.PQ_FieldValue = staff.GS_Code;

			//Job2
			var job2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var task2_1 = job2.WorkflowItems.Tasks.AddNew();
			task2_1.P9_Description = "task1";
			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task2_2 = job2.WorkflowItems.Tasks.AddNew();
			task2_2.P9_Description = "task2";
			task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var trigger2 = job2.WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger2.P9_Description = "Trigger me";

			var action2_1 = trigger2.ProcessTaskNotifications.AddNew();
			action2_1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action2_1.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task1\").P9_Status>";
			action2_1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var action2_2 = trigger2.ProcessTaskNotifications.AddNew();
			action2_2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action2_2.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task2\").P9_Status>";
			action2_2.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			job1.Logs.AddNew(AutoEvents.CustomisableEvent00);
			job2.Logs.AddNew(AutoEvents.CustomisableEvent00);

			Factory.Save();

			var logwalker = MasterFilesTestHelper.RunLogWalker();

			job1.Reload();
			job1.WorkflowItems.Reload(true);
			job2.Reload();
			job2.WorkflowItems.Reload(true);

			CombineAssertions(() =>
			{
				//Job 1 Trigger 1
				AssertEquals("FLD action should close the task.", "CLS", task1_1.P9_Status);

				//Job 1 Trigger 2
				AssertEquals("FLD action should close the task.", "CLS", task1_2.P9_Status);
				AssertEquals("FLD action should be rolledback due to validation error.", "ASN", task1_3.P9_Status);
				AssertContains($"P9_Status of type ZString has validation error if set with value 'XXX' by Set Field (FLD) trigger action. Target Object: '{task1_3.HumanReadableName}'", logwalker);

				//Job 1 Trigger 3
				AssertEquals("FLD action should close the task.", "CLS", task1_4.P9_Status);

				//Job 2 Trigger 1
				AssertEquals("FLD action should close the task.", "CLS", task2_1.P9_Status);
				AssertEquals("FLD action should close the task.", "CLS", task2_2.P9_Status);
			});
		}

		[TestDate(2020, 1, 1)]
		public void TestProcessTaskIFCTriggerActionsAreSetAfterValidation()
		{
			//Job1
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var task1 = job.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "task1";
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task2 = job.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "task2";
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task3 = job.WorkflowItems.Tasks.AddNew();
			task3.P9_Description = "task3";
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task4 = job.WorkflowItems.Tasks.AddNew();
			task4.P9_Description = "task4";
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task4.P9_GS_NKAssignedStaffMember = "";

			var trigger1 = job.WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger1.P9_Description = "Trigger me";

			var action1_1 = trigger1.ProcessTaskNotifications.AddNew();
			action1_1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1_1.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task1\").P9_Status>";
			action1_1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var trigger2 = job.WorkflowItems.Triggers.AddNew();
			trigger2.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger2.P9_Description = "Trigger me";

			var action2_1 = trigger2.ProcessTaskNotifications.AddNew();
			action2_1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2_1.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task2\").P9_Status>";
			action2_1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var action2_2 = trigger2.ProcessTaskNotifications.AddNew();
			action2_2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2_2.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task3\").P9_Status>";
			action2_2.PQ_FieldValue = "XXX";

			var trigger3 = job.WorkflowItems.Triggers.AddNew();
			trigger3.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger3.P9_Description = "Trigger me";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "SSS";

			var action3_1 = trigger3.ProcessTaskNotifications.AddNew();
			action3_1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action3_1.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task4\").P9_GS_NKAssignedStaffMember>";
			action3_1.PQ_FieldValue = staff.GS_Code;

			var action3_2 = trigger3.ProcessTaskNotifications.AddNew();
			action3_2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action3_2.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task4\").P9_Status>";
			action3_2.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			job.Logs.AddNew(AutoEvents.CustomisableEvent00);

			Factory.Save();

			CombineAssertions(() =>
			{
				//Trigger 1
				AssertEquals("IFC action should close the task", "CLS", task1.P9_Status);
				AssertEquals("", trigger1.RowNotifications.ToMessageListString());

				//Trigger 2
				AssertEquals("IFC action should close the task", "CLS", task2.P9_Status);
				AssertEquals("IFC action should be reverted due to validation error", "ASN", task3.P9_Status);
				AssertContains($"P9_Status of type ZString has validation error if set with value 'XXX' by Set Field (IFC) trigger action. Target Object: '{task3.HumanReadableName}'", trigger2.RowNotifications.ToMessageListString());

				//Trigger 3
				AssertEquals("IFC action should close the task", "CLS", task4.P9_Status);
				AssertEquals("", trigger3.RowNotifications.ToMessageListString());
			});
		}

		public void TestIFCGlbBranchCodeValidation()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "111";
			branch1.GB_GC = company1.PK;

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "222";
			branch2.GB_GC = company2.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch2.PK;

			Factory.Save();

			var trigger1 = staff.WorkflowItems.Triggers.AddNew();
			trigger1.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger1.P9_Description = "Trigger me";

			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = "<HomeBranch.GB_Code>";
			action1.PQ_FieldValue = "111";

			Factory.Save();

			staff.Logs.AddNew(AutoEvents.CustomisableEvent00);
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestSetTaskStatusAndStaff()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "SSS";

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "task1";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Trigger me";

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task1\").P9_Status>";
			action1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task1\").P9_GS_NKAssignedStaffMember>";
			action2.PQ_FieldValue = staff.GS_Code;

			Factory.Save();

			job.Logs.AddNew(AutoEvents.CustomisableEvent00);

			AssertEquals("Status is set", ProcessTaskStatusCodeList.Codes.Closed, task.P9_Status);
			AssertEquals("Staff is set", "SSS", task.P9_GS_NKAssignedStaffMember);
			AssertEquals("Expecting no warnings", "", trigger.RowNotifications.ToMessageListString());
		}

		public void TestSetFieldPlaysATaskForAUserWhoIsAlreadyWorking()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "SSS";

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "task1";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task2 = job.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "task2";
			task2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Trigger me";

			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task1\").P9_Status>";
			action1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Working;

			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2.PQ_FieldName = "<WorkflowItems.where(\"<IsTask>\"==\"Y\"&&\"<P9_Description>\"==\"task1\").P9_GS_NKAssignedStaffMember>";
			action2.PQ_FieldValue = staff.GS_Code;

			Factory.Save();

			job.Logs.AddNew(AutoEvents.CustomisableEvent00);

			AssertEquals("Status is set", ProcessTaskStatusCodeList.Codes.Working, task.P9_Status);
			AssertEquals("Staff is set", "SSS", task.P9_GS_NKAssignedStaffMember);
			AssertEquals("Status of this task should be suspended", ProcessTaskStatusCodeList.Codes.Suspended, task2.P9_Status);
			AssertEquals("Expecting now warnings", "", trigger.RowNotifications.ToMessageListString());
		}

		public void TestShouldReturnUnEscapeValueWhenSetFieldValue()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "TESTS00001";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			trigger.P9_Description = "Test Set Field";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<JS_GoodsDescription>";
			action.PQ_FieldValue = @"<If(""<JS_HouseBill>""==""TESTS00001"", ""- \>$680 to MML"", """")> | <Substring(""\>\> 1"", 0, 1)>";

			Factory.Save();

			((IWorkflowProvider)shipment).Logs.AddNew(AutoEvents.CustomisableEvent00);

			Factory.Save();

			var logwalker = MasterFilesTestHelper.RunLogWalker();

			AssertEquals("Precondition: The action works", @"- >$680 to MML | >", shipment.JS_GoodsDescription);
		}

		public void TestSharedTasks()
		{
			AssertSharedTasks(true, true);
			AssertSharedTasks(true, false);
			AssertSharedTasks(false, true);
			AssertSharedTasks(false, false);
		}

		public void AssertSharedTasks(bool shareTasksForAllCompanies, bool areTasksCompanySpecific)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Face of Gorm";
			trigger.P9_GC = company.PK;
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(areTasksCompanySpecific);
			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Noon";
			task.P9_ShareTasksForAllCompanies = shareTasksForAllCompanies;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<WorkflowItems.Where(\"<IsTask>\" == \"Y\").P9_Description>";
			action.PQ_FieldValue = "Nomp";
			Factory.Save();

			dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
			AssertNotEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			task.Reload();

			if (!shareTasksForAllCompanies && areTasksCompanySpecific)
			{
				AssertEquals("Noon", task.P9_Description);
			}
			else
			{
				AssertEquals("Nomp", task.P9_Description);
			}
		}

		public void TestCannotUpdateNonSharedTasksDifferentCompany()
		{
			var companyOne = Factory.NewWithValidTestData<GlbCompany>();
			var branchOne = Factory.NewWithValidTestData<GlbBranch>();
			branchOne.GB_GC = companyOne.PK;
			branchOne.GB_Code = "BN1";

			var companyTwo = Factory.NewWithValidTestData<GlbCompany>();
			var branchTwo = Factory.NewWithValidTestData<GlbBranch>();
			branchTwo.GB_Code = "BN2";
			branchTwo.GB_GC = companyTwo.PK;
			var departmentTwo = Factory.NewWithValidTestData<GlbDepartment>();
			departmentTwo.GE_Code = "DP2";

			IGlbStaff staff = Factory.New<IGlbStaff>();
			staff.GS_Code = "JH";
			staff.GS_FullName = "John Howard";

			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Face of Gorm";
			trigger.P9_GC = companyOne.PK;
			trigger.TriggerConditions.TriggerContextCode = "EVT";
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<WorkflowItems.Where(\"<IsTask>\" == \"Y\").P9_Description>";
			action.PQ_FieldValue = "Nomp";
			Factory.Save();

			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Noon";
			task.P9_ShareTasksForAllCompanies = false;

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), branchTwo.PK.ToGuid(), departmentTwo.PK.ToGuid())))
			{
				var log = dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
				AssertEquals(log.SL_GB_NKBranch, "BN2");
				AssertEquals(log.SL_GE_NKDepartment, "DP2");
				Factory.Save();
				AssertNotEquals(ZDateTimeOffset.Empty, trigger.P9_ActualDateForBinding);
			}

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			task.Reload();

			AssertEquals("Noon", task.P9_Description);
		}

		public void TestMacroGetGuidByOrgCodeWithIFC()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "ABCDEF";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "TESTS00001";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			trigger.P9_Description = "Test Set Field";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<JS_OH_HandledOnBehalfOfForwarder>";
			action.PQ_FieldValue = $"<GetGuidByOrgCode(\"{organisation.OH_Code}\")>";

			Factory.Save();

			((IStmALogParent)shipment).Logs.AddNew(AutoEvents.CustomisableEvent00);

			Factory.Save();

			AssertEquals("GetGuidByOrgCode", organisation.PK, shipment.JS_OH_HandledOnBehalfOfForwarder);
		}

		#region TestInvalidSyntaxInFieldName

		public void TestInvalidSyntaxInFieldName()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertInvalidSyntaxInFieldName();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertInvalidSyntaxInFieldName();
			}
		}

		void AssertInvalidSyntaxInFieldName()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			var task = dummy.WorkflowItems.Triggers.AddNew();

			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			action.PQ_FieldName = "< WorkflowItems.Where(\"<IsException>\" == \"Y\""; // Invalid synxtax: no closing bracket

			AssertEquals("There should be an error about the where clause and not one about not being able to find a property, and yet...", 1, action.PQ_FieldNameInfo.Notifications.Count());
			AssertHasError(action.PQ_FieldNameInfo, "Where clause must be properly formatted.");
		}

		#endregion

		#region TestInvalidSyntaxInFieldValue

		[ExpectNoExceptions]
		public void TestInvalidSyntaxInFieldValue()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertInvalidSyntaxInFieldValue();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertInvalidSyntaxInFieldValue();
			}
		}

		void AssertInvalidSyntaxInFieldValue()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();

			var child0 = dummy.Collection.AddNew();
			child0.Z0_Description = "child-property-0";
			child0.Z0_Code = "code0";
			child0.Z0_VarCharMax = "123";

			var task = (DummyProcessTask)dummy.WorkflowItems.Triggers.AddNew();
			task.OverriddenParentTypeForTest = dummy.GetType();

			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<Collection.Where(\"<Z0_Code>\" == \"code0\").Z0_Number>";
			action.PQ_FieldValue = string.Format("<Collection.First("); // Invalid syntax: no closing bracket
			action.PQ_FieldValue = string.Format("<Collection.First()"); // Invalid syntax: no dot followed property-name

			var notifications = new NotificationsForTest();
			FireTriggerAction(dummy, task, action, notifications);
		}

		#endregion

		#region TestNestedMacro_Valid

		public void TestNestedMacro_Valid()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNestedMacro_Valid();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNestedMacro_Valid();
			}
		}

		void AssertNestedMacro_Valid()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			var task = (DummyProcessTask)dummy.WorkflowItems.Triggers.AddNew();
			task.P9_Description = "bc";
			task.OverriddenParentTypeForTest = dummy.GetType();

			Factory.Save();

			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "Z0_Description";
			action.PQ_FieldValue = "<WorkflowItems.First(\"<P9_Description>\"==\"<SubString(\"abcde\",1,2)>\").P9_Description>";

			AssertNoErrors("GIVEN valid nested macro 'substring', WHEN validating THEN should have no error", action.PQ_FieldValueInfo);

			AssertSetProperty(
				propertyPath: action.PQ_FieldName,
				valuePath: action.PQ_FieldValue,
				expectedValues: new IZType[] { new ZString("bc") },
				expectedWarnings: @"FLD setting [Z0_Description] with [<WorkflowItems.First(""<P9_Description>""==""<SubString(""abcde"",1,2)>"").P9_Description>] succeeded on 1 properties. Unchanged on 0. Failed on 0
Success logs:
Target Object: 'Dummy Business Object bc', Source: Default, Target: bc",
				action: action,
				expectedProperties: new[] { dummy.Z0_DescriptionInfo },
				clearValue: false,
				expectedLogCount: 1);
		}

		#endregion

		#region TestNestedMacro_Invalid

		public void TestNestedMacro_Invalid()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNestedMacro_Invalid();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNestedMacro_Invalid();
			}
		}

		void AssertNestedMacro_Invalid()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();

			var task = (DummyProcessTask)dummy.WorkflowItems.Triggers.AddNew();
			task.P9_Description = "bc";
			task.OverriddenParentTypeForTest = dummy.GetType();

			Factory.Save();

			AssertEquals("GIVEN Z0_Desription=Default", "Default", dummy.Z0_Description);

			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "Z0_Description";
			action.PQ_FieldValue = "<WorkflowItems.First(\"<P9_Description>\"==\"<SubStringXXX(\"abcde\",1,2)>\").P9_Description>";

			AssertNoErrors("GIVEN invalid nested macro 'substring', WHEN validating THEN should have no error because error validation happen on runtime", action.PQ_FieldValueInfo);

			AssertSetProperty(
				propertyPath: action.PQ_FieldName,
				valuePath: action.PQ_FieldValue,
				expectedValues: new IZType[] { new ZString("Default") },
				expectedWarnings: @"Set field (FLD) macro evaluated to null. Macro: [<WorkflowItems.First(""<P9_Description>""==""<SubStringXXX(""abcde"",1,2)>"").P9_Description>]",
				action: action,
				expectedProperties: new[] { dummy.Z0_DescriptionInfo },
				clearValue: false,
				assertMessage: "GIVEN invalid nested macro 'substringXXX WHEN process THEN field should not be set",
				expectedLogCount: 1);

			AssertSetProperty(
				dummy: dummy,
				propertyPath: "Z0_Description",
				valuePath: "<WorkflowItems.First(\"A\").P9_Description>",
				expectedValue: new ZString("Default"),
				// "Cannot evaluate expression" notification occurs once for each of the two actions on the dummy
				expectedWarnings: @"Cannot evaluate expression: ""A"". 
Result of ""A"" is not a True/False expression
Cannot evaluate expression: ""A"". 
Result of ""A"" is not a True/False expression",
				expectedProperties: new[] { dummy.Z0_DescriptionInfo },
				clearValue: false,
				assertMessage: "GIVEN invalid nested macro i.e. invalid true/false expression 'A' WHEN process THEN field should not be set");
		}

		#endregion

		#region TestNested_First

		public void TestNested_First()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNested_First();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNested_First();
			}
		}

		void AssertNested_First()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			var child0 = dummy.Collection.AddNew();
			child0.Z0_Description = "child-property-0";
			child0.Z0_Code = "code0";
			child0.Z0_VarCharMax = "123";
			child0.Z0_IsSystem = false;

			AssertSetProperty(
				dummy: dummy,
				propertyPath: "Z0_Description",
				valuePath: "<Collection.First().Z0_Code>",
				expectedValue: new ZString("code0"),
				expectedProperties: new[] { dummy.Z0_DescriptionInfo },
				assertMessage: "GIVEN collection only, WHEN processing macro THEN should return macro-result");

			AssertSetProperty(
				dummy: dummy,
				propertyPath: "Z0_Description",
				valuePath: "<Collection.First().Z0_Code> <Collection.First().Z0_Code>",
				expectedValue: new ZString("code0 code0"),
				expectedProperties: new[] { dummy.Z0_DescriptionInfo },
				assertMessage: "GIVEN multiple collections WHEN processing macro THEN should return macro-result");
		}

		#endregion

		#region TestContinuesAfterErrorInWhere

		public void TestContinuesAfterErrorInWhere()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var task1 = dummy.WorkflowItems.Triggers.AddNew();
			task1.P9_Description = "<Add(\"something\")>";
			task1.P9_Status = "OPN";

			var task2 = dummy.WorkflowItems.Triggers.AddNew();
			task2.P9_Description = "abc";
			task2.P9_Status = "OPN";

			var action = task1.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Description>\" == \"abc\").P9_Status>";
			action.PQ_FieldValue = "SUS";

			var notifications = new NotificationsForTest();
			FireTriggerAction(dummy, task1, action, notifications);

			AssertContains("Precondition", @"Cannot evaluate expression: ""<P9_Description>"" == ""abc"". 
Error evaluating ""<Add(""something"")>"" == ""abc""
", notifications.ToString());

			AssertEquals("OPN", task1.P9_Status);
			AssertEquals("SUS", task2.P9_Status);
		}

		#endregion

		#region TestMacroAndString

		[ExpectNoExceptions]
		public void TestMacroAndString()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertMacroAndString();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertMacroAndString();
			}
		}

		void AssertMacroAndString()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			var child0 = dummy.Collection.AddNew();
			child0.Z0_Description = "child-property-0";
			child0.Z0_Code = "code0";
			child0.Z0_VarCharMax = "123";
			child0.Z0_IsSystem = false;

			var task = (DummyProcessTask)dummy.WorkflowItems.Triggers.AddNew();
			task.OverriddenParentTypeForTest = dummy.GetType();

			var action = task.ProcessTaskNotifications.AddNew();

			AssertSetProperty(
				propertyPath: "<Z0_Description>",
				valuePath: "test: <Collection.First().Z0_Code>",
				expectedValues: new IZType[] { new ZString("test: code0") },
				expectedWarnings: null,
				action: action,
				expectedProperties: new[] { dummy.Z0_DescriptionInfo },
				assertMessage: "GIVEN multiple collections WHEN processing macro THEN should return macro-result");
		}

		#endregion

		public void TestMacroDateTimeStart()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			AssertSetProperty(
				dummy: dummy,
				propertyPath: "<Z0_Date>",
				valuePath: "<DateTimeStart>",
				expectedValue: ZDate.Empty,
				notExpectedProperties: new[] { dummy.Z0_DateInfo },
				assertMessage: "Should be valid");
		}

		[TestDate(2023, 02, 22)]
		public void TestDateDiffMacro()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			AssertSetProperty(
				dummy: dummy,
				propertyPath: "<Z0_Description>",
				valuePath: "<DateDiff(\"<Now>\", \"2023-02-23\", \"DAYS\")>",
				expectedValue: new ZString("1"),
				expectedProperties: new[] { dummy.Z0_DescriptionInfo });
		}

		public void TestContainsMacro()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			AssertSetProperty(
				dummy: dummy,
				propertyPath: "<Z0_Description>",
				valuePath: "<Contains(\"Apple\", \"App\")>",
				expectedValue: new ZString("Y"),
				expectedProperties: new[] { dummy.Z0_DescriptionInfo });
		}

		public void TestSetFieldPreservesDatePrecision()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;
			dummy.Z0_AnotherDate = new ZDateTime(2020, 10, 2, 1, 3, 5).AddMilliseconds(123);

			AssertSetProperty(
				dummy: dummy,
				propertyPath: "<Z0_Date>",
				valuePath: "<Z0_AnotherDate>",
				expectedValue: dummy.Z0_AnotherDate,
				expectedProperties: new[] { dummy.Z0_DateInfo });

			dummy.Z0_DateTimeOffset = new ZDateTimeOffset(2020, 1, 2, 3, 4, 5, 6, new TimeSpan(5, 30, 0)).AddMilliseconds(999).AddTicks(100000);

			AssertSetProperty(
				dummy: dummy,
				propertyPath: "<Z0_Description>",
				valuePath: "Test - <Z0_DateTimeOffset>",
				expectedValue: new ZString("Test - Thursday, 02 January 2020 03:04:06.0150000 +05:30"),
				expectedProperties: new[] { dummy.Z0_DescriptionInfo });
		}

		public void TestClosingTaskViaIFCActionsDoesNotCauseInfiniteLoop()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProviderIncludingRelated)shipment;

			var task1 = shipmentWithWorkflow.WorkflowItems.Tasks.AddNew();
			var task2 = shipmentWithWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_Type = "ABC";
			task2.P9_Type = "ABC";
			task1.P9_SE_NKTaskCompletionEvent = AutoEvents.DetachedCode;
			task2.P9_SE_NKTaskCompletionEvent = AutoEvents.AttachedCode;

			var milestone1 = shipmentWithWorkflow.WorkflowItems.Milestones.AddNew();
			var milestone2 = shipmentWithWorkflow.WorkflowItems.Milestones.AddNew();
			milestone1.TriggerConditions.TriggerEventCode = AutoEvents.AttachedCode;
			milestone2.TriggerConditions.TriggerEventCode = AutoEvents.DetachedCode;
			milestone1.P9_RespondToCascadedEvents = true;
			milestone2.P9_RespondToCascadedEvents = true;

			var action1 = milestone1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action1.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Type>\"==\"ABC\").P9_Status>";
			action1.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Closed;

			var action2 = milestone2.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2.PQ_FieldName = "<WorkflowItems.Where(\"<P9_Type>\"==\"ABC\").P9_Status>";
			action2.PQ_FieldValue = ProcessTaskStatusCodeList.Codes.Assigned;

			Factory.Save();

			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.AddShipment(shipment);

			Factory.Save();

			AssertEquals("Milestone1 fired once", new ZShort(99), Factory.Load<ProcessTask>(milestone1.PK).P9_TriggerFiredCountdown);
			AssertEquals("Milestone2 fired once", new ZShort(99), Factory.Load<ProcessTask>(milestone2.PK).P9_TriggerFiredCountdown);
		}

		#region TestSetProperty_WeirdTransactionLogicCanOccurInTheSetter

		public void TestSetProperty_WeirdTransactionLogicCanOccurInTheSetter()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSetProperty_WeirdTransactionLogicCanOccurInTheSetter();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSetProperty_WeirdTransactionLogicCanOccurInTheSetter();
			}
		}

		void AssertSetProperty_WeirdTransactionLogicCanOccurInTheSetter()
		{
			var dummy = (EnterpriseBusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var note = dummy.Notes.AddNew();

			note.ST_NoteTextInfo.ValueChanged += (s, e) =>
			{
				BusinessObjectFactory.SaveTogether(Factory, new BusinessObjectFactory());
			};

			AssertSetProperty(dummy, "Notes.VisibleNotes.ST_NoteText", "Hello");

			note.Reload();
			AssertEquals("Hello", note.ST_NoteText);
		}

		#endregion

		#region TestListChangesAreSuspended

		[ExpectNoExceptions]
		public void TestListChangesAreSuspended()
		{
			EnterpriseBusinessObject dummy = (EnterpriseBusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			dummy.Notes.AddNew();
			dummy.Notes.AddNew();
			dummy.Notes.AddNew();
			dummy.Notes.VisibleNotes.ShowRelatedNotes = false;

			ZGuid randomGuid = new ZGuid(Guid.NewGuid());

			AssertSetProperty(dummy, "Notes.VisibleNotes.ST_NoteText", "Hello");
			AssertSetProperty(dummy, "Notes.VisibleNotes.ST_NoteText", "Bye");
			AssertSetProperty(dummy, "Notes.VisibleNotes.ST_NoteText", "Hello again");

			Assert("Precondition: All notes should start with the correct parent ID", dummy.Notes.VisibleNotes.ToList<StmNote>().Aggregate(true, (allMatch, nextNote) => allMatch && nextNote.ST_ParentID == dummy.PK));

			AssertSetProperty(dummy, "Notes.VisibleNotes.ST_ParentID", randomGuid.ToString(), randomGuid, "Property Enterprise.ZArchitecture.Business.StmNote.ST_ParentID of type ZGuid holds system data and cannot be used in 'Set Field' trigger action");
			AssertSetProperty(dummy, "Notes.VisibleNotes.ST_ParentID", dummy.PK.ToString(), dummy.PK, $@"Set field (FLD) target not found. Macro [{dummy.PK.ToString()}] Target");
			AssertSetProperty(dummy, "Notes.VisibleNotes.ST_ParentID", randomGuid.ToString(), randomGuid, $@"Set field (FLD) target not found. Macro [{randomGuid.ToString()}] Target");
		}

		[ExpectNoExceptions]
		public void TestStackOverflowPrevention()
		{
			var dummy = Factory.New<DummyWithWorkflowWithDependents>();

			for (int i = 0; i < 1000; ++i)
			{
				dummy.Dependents.AddNew();
			}

			AssertSetProperty(dummy, "Dependents.MyParent.Dependents.Salutation", "Hello",
				expectedWarnings: @"FLD setting [Dependents.MyParent.Dependents.Salutation] with [Hello] succeeded on 1000 properties. Unchanged on 999000. Failed on 0
Success logs:
Target Object: 'DummyDependentBizo', Source: , Target: Hello
Target Object: 'DummyDependentBizo', Source: , Target: Hello
Target Object: 'DummyDependentBizo', Source: , Target: Hello
Target Object: 'DummyDependentBizo', Source: , Target: Hello
Target Object: 'DummyDependentBizo', Source: , Target: Hello
Success logs truncated
Unchanged logs:
Target Object: 'DummyDependentBizo', Source: Hello, Target: Hello");
		}

		#endregion

		#region TestWorkflowSetFieldReadonlyCheckBypass

		public void TestWorkflowSetFieldReadonlyCheckBypass()
		{
			var dummy = (EnterpriseBusinessObject)Factory.New<Forwarding.IForwardingShipment>();
			var propInfo = dummy.FindPropertyInfo("JS_Phase");

			AssertSetProperty(dummy, "JS_Phase", "ALL", new ZString("ALL"), @"FLD setting [JS_Phase] with [ALL] succeeded on 0 properties. Unchanged on 1. Failed on 0
Unchanged logs:
Target Object: 'Shipment S00001000', Source: ALL, Target: ALL", new[] { propInfo }, clearValue: false);

			dummy = (EnterpriseBusinessObject)Factory.New<Forwarding.IForwardingConsol>();
			propInfo = dummy.FindPropertyInfo("JK_Phase");
			((IZPropertyInfoObsolete)propInfo).ReadOnly = true;

			AssertSetProperty(dummy, "JK_Phase", "ALL", new ZString("ALL"), null, new[] { propInfo });
		}

		#endregion

		#region TestSetProperty

		[TestDate(2000, 1, 2)]
		public void TestSetProperty_Macro_DateTimeAsString()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSetProperty_Macro_DateTimeAsString();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSetProperty_Macro_DateTimeAsString();
			}
		}

		void AssertSetProperty_Macro_DateTimeAsString()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			AssertSetProperty(
				dummy: dummy,
				propertyPath: "<Z0_Date>",
				valuePath: "<DateTimeAsString('<Now>', 'dd-MMM-yy')>",
				expectedValue: new ZDateTime(2000, 1, 2),
				expectedProperties: new[] { dummy.Z0_DateInfo });
		}

		public void TestSetProperty_Macro_TriggeringEvent_FLD()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AuthorisedCode;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction.PQ_FieldName = "<Z0_Description>";
			triggerAction.PQ_FieldValue = "ref: <TriggeringEvent.SL_Reference>";

			var triggeringEvent = dummy.Logs.AddNew(AutoEvents.Authorised, "hello");

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			dummy.Reload();

			AssertEquals("ref: " + triggeringEvent.SL_Reference, dummy.Z0_Description);
		}

		public void TestSetProperty_Macro_StatusChangeContext_FLD()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AuthorisedCode;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction.PQ_FieldName = "<Z0_Description>";
			triggerAction.PQ_FieldValue = "<GetDummyValueForMacro>";

			var triggeringEvent = dummy.Logs.AddNew(AutoEvents.Authorised, "|SRC=No Sauce|CHG=Some Change");

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			dummy.Reload();

			AssertEquals($"hello|{triggeringEvent.SL_SE_NKEvent}{triggeringEvent.SL_Reference}", dummy.Z0_Description);

			var dummyAddedByTrigger = Factory.LoadTop1<DummyWithWorkflow>(new ZQuery(DummyBizoSchema.Z0_Description, $"{AutoEvents.AuthorisedCode}|SRC=No Sauce|CHG=Some Change"));
			AssertNotNull(dummyAddedByTrigger);
		}

		public void TestGetPropertyNotOnIStmALogInterface()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			dummy.Z0_Description = "SOMETHING";

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AuthorisedCode;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction.PQ_FieldName = "<Z0_Description>";
			triggerAction.PQ_FieldValue = "<TriggeringEvent.ReferenceFreeText>";

			var triggeringEvent = dummy.Logs.AddNew(AutoEvents.Authorised, "hello");

			var property = typeof(IStmALog).GetProperty("ReferenceFreeText");
			AssertNull("Precondition: Needs to be a property not on IStmALog", property);

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			dummy.Reload();

			AssertEquals(triggeringEvent.ReferenceFreeText, dummy.Z0_Description);
		}

		public void TestSetterThrowingExceptionShouldNotReport_IFC()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithErrors);
			var dummy = Factory.New<DummyWithErrors>();

			var trigger = ((IWorkflowProvider)dummy).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "InvoiceSent";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<Z0_Description>";
			action.PQ_FieldValue = "Spaghetti";
			dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals(true, trigger.HasRowWarnings);
		}

		public void TestGetterOfSetterThrowingExceptionShouldNotReport_IFC()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithErrors);
			var dummy = Factory.New<DummyWithErrors>();

			var trigger = ((IWorkflowProvider)dummy).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "InvoiceSent";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<Z0_VarCharMax>";
			action.PQ_FieldValue = "Spaghetti";
			dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals(true, trigger.HasRowWarnings);
		}

		public void TestAccessingADeletedBusinessObject()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithErrors);
			var dummy = Factory.New<DummyWithErrors>();
			var trigger = ((IWorkflowProvider)dummy).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "InvoiceSent";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<DeletedBusinessObject.Z0_Number>";
			action.PQ_FieldValue = "123";
			dummy.DeletedBusinessObject.Delete();
			dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
			var notifications = trigger.Notifications;

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals(2, notifications.Count());
			AssertContains("Property of type DummyBizo cannot be accessed because the row is deleted", notifications.Where(x => x.Message.Contains("DummyBizo")).GetFirstMessage());
			AssertContains("Set field (IFC) target not found. Macro [123] Target [Enterprise.Workflow.Business.Testing.WorkflowSetFieldProcessorTest+DummyWithErrors]", notifications.Where(x => x.Message.Contains("DummyWithErrors")).GetFirstMessage());
		}

		[GuiTest]
		public void TestGetterThrowingExceptionShouldNotReport_IFC()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithErrors);
			var dummy = Factory.New<DummyWithErrors>();
			var trigger = ((IWorkflowProvider)dummy).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "InvoiceSent";
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<Z0_NVarChar>";
			action.PQ_FieldValue = "<Z0_VarCharMax>";
			dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}
		class DummyWithErrors : DummyWithWorkflow
		{
			public DummyWithErrors(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			public override ZString Z0_VarCharMax
			{
				get => throw new InvalidOperationException("No code.");
				set => base.Z0_VarCharMax = value;
			}
			public override ZString Z0_Description
			{
				get => base.Z0_Description;
				set => throw new InvalidOperationException("No description");
			}
			protected override void SetDefaultValues()
			{
				Z0_IsSystem = false;
			}
			protected DummyWithErrors deletedBusinessObject;
			public DummyWithErrors DeletedBusinessObject
			{
				get
				{
					if (deletedBusinessObject == null)
					{
						deletedBusinessObject = Factory.New<DummyWithErrors>();
						Factory.Save();
					}
					return deletedBusinessObject;
				}
			}
		}

		public void TestSetProperty_Macro_TriggeringEvent_IFC()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AuthorisedCode;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			triggerAction.PQ_FieldName = "<Z0_Description>";
			triggerAction.PQ_FieldValue = "ref: <TriggeringEvent.SL_Reference>";

			var triggeringEvent = dummy.Logs.AddNew(AutoEvents.Authorised, "hello");

			Factory.Save();

			AssertEquals("ref: " + triggeringEvent.SL_Reference, dummy.Z0_Description);
		}

		public void TestSetProperty_Macro_StatusChangeContext_IFC()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.AuthorisedCode;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			triggerAction.PQ_FieldName = "<Z0_Description>";
			triggerAction.PQ_FieldValue = "<GetDummyValueForMacro>";

			var triggeringEvent = dummy.Logs.AddNew(AutoEvents.Authorised, "|SRC=No Sauce|CHG=Some Change");

			Factory.Save();

			AssertEquals($"hello|{triggeringEvent.SL_SE_NKEvent}{triggeringEvent.SL_Reference}", dummy.Z0_Description);

			var dummyAddedByTrigger = Factory.LoadTop1<DummyWithWorkflow>(new ZQuery(DummyBizoSchema.Z0_Description, $"{AutoEvents.AuthorisedCode}|SRC=No Sauce|CHG=Some Change"));
			AssertNotNull(dummyAddedByTrigger);
		}

		public void TestSetPropertyWithWhere()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSetPropertyWithWhere();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSetPropertyWithWhere();
			}
		}

		void AssertSetPropertyWithWhere()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			var child1 = dummy.Collection.AddNew();
			var child2 = dummy.Collection.AddNew();
			var child3 = dummy.Collection.AddNew();

			child1.Z0_IsSystem = false;
			child2.Z0_IsSystem = false;
			child3.Z0_IsSystem = false;

			child1.Z0_VarCharMax = "123";
			child2.Z0_VarCharMax = "456";
			child3.Z0_VarCharMax = "x(xx)";

			AssertSetProperty(dummy, "<Collection.Where(\"<Z0_VarCharMax>\" == \"123\").Z0_Number>", "123", new ZInt(123), null, new[] { dummy.Collection[0].Z0_NumberInfo }, new[] { dummy.Z0_NumberInfo, dummy.Collection[1].Z0_NumberInfo, dummy.Collection[2].Z0_NumberInfo });
			AssertSetProperty(dummy, "<Collection.Where(\"<Z0_Number>\" == \"123\").Z0_Bool>", "Y", new ZBool(true), null, new[] { dummy.Collection[0].Z0_BoolInfo }, new[] { dummy.Z0_NumberInfo, dummy.Collection[1].Z0_NumberInfo, dummy.Collection[2].Z0_NumberInfo });
			AssertSetProperty(dummy, "<Collection.Where(\"<Z0_VarCharMax>\" == \"456\").Z0_Number>", "456", new ZInt(456), null, new[] { dummy.Collection[1].Z0_NumberInfo }, new[] { dummy.Z0_NumberInfo, dummy.Collection[0].Z0_NumberInfo, dummy.Collection[2].Z0_NumberInfo });
			AssertSetProperty(dummy, "<Collection.Where(\"<Z0_VarCharMax>\" == \"789\").Z0_Number>", "789", new ZInt(789), @"Set field (FLD) target not found. Macro [789] Target [Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter]",
			Array.Empty<ZPropertyInfo>(), new[] { dummy.Z0_NumberInfo, dummy.Collection[0].Z0_NumberInfo, dummy.Collection[1].Z0_NumberInfo, dummy.Collection[2].Z0_NumberInfo });
			AssertSetProperty(dummy, "<Collection.Where(\"<Z0_VarCharMax>\" == \"x(xx)\").Z0_Number>", "987", new ZInt(987), null, new[] { dummy.Collection[2].Z0_NumberInfo }, new[] { dummy.Z0_NumberInfo, dummy.Collection[0].Z0_NumberInfo, dummy.Collection[1].Z0_NumberInfo });
		}

		public void TestSetPropertyWithWhere_DoubleQuotes()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSetPropertyWithWhere_DoubleQuotes();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSetPropertyWithWhere_DoubleQuotes();
			}
		}

		void AssertSetPropertyWithWhere_DoubleQuotes()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			var child1 = dummy.Collection.AddNew();
			var child2 = dummy.Collection.AddNew();

			child1.Z0_VarCharMax = "123";
			child2.Z0_VarCharMax = "x\"xx\"";

			child1.Z0_IsSystem = false;
			child2.Z0_IsSystem = false;

			AssertSetProperty(
				dummy,
				propertyPath: "<Collection.Where(\"<Z0_VarCharMax>\" == \"123\").Z0_Number>",
				valuePath: "123",
				expectedValue: new ZInt(123),
				expectedProperties: new[] { dummy.Collection[0].Z0_NumberInfo },
				notExpectedProperties: new[] { dummy.Z0_NumberInfo, dummy.Collection[1].Z0_NumberInfo });

			AssertSetProperty(
				dummy,
				propertyPath: "<Collection.Where(\"<Z0_VarCharMax>\" == \"789\").Z0_Number>",
				valuePath: "789",
				expectedValue: new ZInt(789),
				expectedWarnings: @"Set field (FLD) target not found. Macro [789] Target [Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter]",
				expectedProperties: Array.Empty<ZPropertyInfo>(),
				notExpectedProperties: new[] { dummy.Z0_NumberInfo, dummy.Collection[0].Z0_NumberInfo, dummy.Collection[1].Z0_NumberInfo });

			AssertSetProperty(
				dummy,
				propertyPath: "<Collection.Where(\"<Z0_VarCharMax>\" == \"x\"xx\"\").Z0_Number>",
				valuePath: "987",
				expectedValue: new ZInt(987),
				expectedProperties: new[] { dummy.Collection[1].Z0_NumberInfo },
				notExpectedProperties: new[] { dummy.Z0_NumberInfo, dummy.Collection[0].Z0_NumberInfo });
		}

		public void TestSetPropertyWithWhereChained_NoJS()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSetPropertyWithWhereChained();
			}
		}

		public void TestSetPropertyWithWhereChained_JS()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSetPropertyWithWhereChained();
			}
		}

		void AssertSetPropertyWithWhereChained()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();

			var org1 = dummy.OrgHeaderCollection.AddNew();
			var org2 = dummy.OrgHeaderCollection.AddNew();
			var org3 = dummy.OrgHeaderCollection.AddNew();

			org1.OH_Code = "org1";
			org2.OH_Code = "org2";
			org3.OH_Code = "org3";

			var address11 = org1.AddressesNoAutoCreate.AddNew();
			var address12 = org1.AddressesNoAutoCreate.AddNew();
			var address13 = org1.AddressesNoAutoCreate.AddNew();

			var address21 = org2.AddressesNoAutoCreate.AddNew();

			var address31 = org3.AddressesNoAutoCreate.AddNew();
			var address32 = org3.AddressesNoAutoCreate.AddNew();

			address11.OA_Address1 = "adr1";
			address12.OA_Address1 = "adr2";
			address13.OA_Address1 = "adr1";

			address21.OA_Address1 = "adr3";

			address31.OA_Address1 = "adr1";
			address32.OA_Address1 = "adr2";

			address11.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);
			address12.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);
			address13.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);

			address21.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);

			address31.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);
			address32.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);

			AssertSetProperty(dummy, "<OrgHeaderCollection.Where(\"<OH_Code>\" == \"org1\").AddressesNoAutoCreate.Where(\"<OA_Address1>\" == \"adr1\").OA_City>", "Randwick", new ZString("Randwick"), null,
				new[]
				{
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[2].OA_CityInfo
				},
				new[]
				{
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[1].OA_CityInfo,
					dummy.OrgHeaderCollection[1].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[1].OA_CityInfo,
				});

			AssertSetProperty(dummy, "<OrgHeaderCollection.AddressesNoAutoCreate.Where(\"<OA_Address1>\" == \"adr1\").OA_City>", "Coogee", new ZString("Coogee"), null,
				new[]
				{
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[2].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[0].OA_CityInfo,
				},
				new[]
				{
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[1].OA_CityInfo,
					dummy.OrgHeaderCollection[1].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[1].OA_CityInfo,
				});

			AssertSetProperty(dummy, "<OrgHeaderCollection.AddressesNoAutoCreate.Where(\"<OA_Address1>\" == \"adr5\").OA_City>", "Bondi", new ZString("Bondi"), @"Set field (FLD) target not found. Macro [Bondi] Target [Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter]", Array.Empty<ZPropertyInfo>(),
				new[]
				{
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[2].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[1].OA_CityInfo,
					dummy.OrgHeaderCollection[1].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[1].OA_CityInfo,
				});

			AssertSetProperty(dummy, "<OrgHeaderCollection.Where(\"<OH_Code>\" == \"org5\").AddressesNoAutoCreate.Where(\"<OA_Address1>\" == \"adr1\").OA_City>", "Mascot", new ZString("Mascot"), @"Set field (FLD) target not found. Macro [Mascot] Target [Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter]",
				Array.Empty<ZPropertyInfo>(),
				new[]
				{
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[2].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[1].OA_CityInfo,
					dummy.OrgHeaderCollection[1].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[1].OA_CityInfo,
				});

			AssertSetProperty(dummy, "<OrgHeaderCollection.Where(\"<OH_Code>\" == \"org1\").AddressesNoAutoCreate.OA_City>", "Alexandria", new ZString("Alexandria"), null,
				new[]
				{
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[1].OA_CityInfo,
					dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[2].OA_CityInfo
				},
				new[]
				{
					dummy.OrgHeaderCollection[1].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[0].OA_CityInfo,
					dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[1].OA_CityInfo,
				});
		}

		#region TestSetProperty that Value has First and FirstOfDefault

		public void TestSetPropertyWithField_First()
		{
			SetupAndAssert_SetPropertyWithValue_First_FirstOrDefault("First");
		}

		public void TestSetPropertyWithField_FirstOrDefault()
		{
			SetupAndAssert_SetPropertyWithValue_First_FirstOrDefault("FirstOrDefault");
		}

		public void TestSetPropertyWithValue_First_Chained()
		{
			SetupAndAssert_SetPropertyWithValue_First_FirstOrDefault_Chained("First");
		}

		public void TestSetPropertyWithValue_FirstOrDefault_Chained()
		{
			SetupAndAssert_SetPropertyWithValue_First_FirstOrDefault_Chained("FirstOrDefault");
		}

		public void TestSetPropertyWithValue_First_NoCollection()
		{
			SetupAndAssert_SetPropertyWithValue_NoCollection("First");
		}

		public void TestSetPropertyWithValue_FirstOrDefault_NoCollection()
		{
			SetupAndAssert_SetPropertyWithValue_NoCollection("FirstOrDefault");
		}

		public void TestSetPropertyWithValue_First_FirstOrDefault_WithExistingNotifications()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			var child0 = dummy.Collection.AddNew();
			var child1 = dummy.Collection.AddNew();

			child0.Z0_Description = "child-property-0";
			child1.Z0_Description = "child-property-1";

			child0.Z0_IsSystem = false;
			child1.Z0_IsSystem = false;

			child0.Z0_Code = "code0";
			child1.Z0_Code = "code1";

			child0.Z0_VarCharMax = "123";
			child1.Z0_VarCharMax = "456";

			var task = (DummyProcessTask)dummy.WorkflowItems.Triggers.AddNew();
			task.OverriddenParentTypeForTest = dummy.GetType();
			var action = task.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<Collection.Where(\"<Z0_Code>\" == \"code0\").Z0_Number>";
			action.PQ_FieldValue = string.Format("<Collection.First(\"<Z0_Code>\" == \"code1\").Z0_VarCharMax>");

			var notifications = new NotificationsForTest();

			notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Warning, "Test Notification"));

			FireTriggerAction(dummy, task, action, notifications);

			AssertContains("GIVEN Notifications has existing notification, WHEN executing WorkflowSetFieldTrigger.Process, should not be considered as error i.e. Re-added Test-notification text",
				@"FLD setting [Collection.Where(""<Z0_Code>"" == ""code0"").Z0_Number] with [<Collection.First(""<Z0_Code>"" == ""code1"").Z0_VarCharMax>] succeeded on 1 properties. Unchanged on 0. Failed on 0
Success logs:
Target Object: 'DummyBizo', Source: 0, Target: 456",
				notifications.ToString().Trim());

			AssertEquals("GIVEN Notifications has existing notification, WHEN executing WorkflowSetFieldTrigger.Process, should not error and property should be set",
				456,
				child0.Z0_Number);
		}

		public void SetupAndAssert_SetPropertyWithValue_NoCollection(string clause)
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();

			var child0 = dummy.Collection.AddNew();
			var child1 = dummy.Collection.AddNew();

			child0.Z0_Description = "child-property-0";
			child1.Z0_Description = "child-property-1";

			child0.Z0_Code = "code0";
			child1.Z0_Code = "code1";

			child0.Z0_VarCharMax = "123";
			child1.Z0_VarCharMax = "456";

			AssertSetProperty(
				dummy,
				propertyPath: "<Collection.Where(\"<Z0_Code>\" == \"code0\").Z0_Number>",
				valuePath: $"<Z0_Description.{clause}(\"<Z0_Code>\" == \"code1\").Z0_VarCharMax>",
				expectedValue: new ZInt("456"),
				expectedWarnings: $@"Set field (FLD) macro evaluated to null. Macro: [<Z0_Description.{clause}(""<Z0_Code>"" == ""code1"").Z0_VarCharMax>]",
				notExpectedProperties: dummy.Collection.Select(c => c.Z0_NumberInfo),
				assertMessage: $"{clause}: GIVEN value has First/FirstOrDefault Without collection, should error and not set value");
		}

		#endregion

		[TestDate(2012, 7, 3)]
		public void TestSetProperty()
		{
			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertSetProperty();
			}

			using (RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSetProperty();
			}
		}

		void AssertSetProperty()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			dummy.Collection.AddNew();
			dummy.Collection.AddNew();
			dummy.Collection[0].Z0_IsSystem = false;
			dummy.Collection[1].Z0_IsSystem = false;

			AssertSetProperty(dummy, "Z0_VarCharMax", "Abcde", new ZString("Abcde"), null, new[] { dummy.Z0_VarCharMaxInfo }, new[] { dummy.Collection[0].Z0_VarCharMaxInfo, dummy.Collection[1].Z0_VarCharMaxInfo });
			AssertSetProperty(dummy, "Z0_Number", "123", new ZInt(123), null, new[] { dummy.Z0_NumberInfo });
			AssertSetProperty(dummy, "Z0_Bool", "Y", new ZBool(true), null, new[] { dummy.Z0_BoolInfo });
			AssertSetProperty(dummy, "Z0_Bool", "N", new ZBool(false), null, new[] { dummy.Z0_BoolInfo });
			AssertSetProperty(dummy, "Z0_Date", "2011-03-15 16:09:11", new ZDateTime(2011, 3, 15, 16, 9, 11), null, new[] { dummy.Z0_DateInfo });

			AssertSetProperty(dummy, "Collection.Z0_VarCharMax", "Abcde", new ZString("Abcde"), null, new[] { dummy.Collection[0].Z0_VarCharMaxInfo, dummy.Collection[1].Z0_VarCharMaxInfo }, new[] { dummy.Z0_VarCharMaxInfo });
			AssertSetProperty(dummy, "Collection+Z0_VarCharMax", "Abcde", new ZString("Abcde"), null, new[] { dummy.Collection[0].Z0_VarCharMaxInfo, dummy.Collection[1].Z0_VarCharMaxInfo }, new[] { dummy.Z0_VarCharMaxInfo });
			AssertSetProperty(dummy, "<Collection.Z0_VarCharMax> ", "Abcde", new ZString("Abcde"), null, new[] { dummy.Collection[0].Z0_VarCharMaxInfo, dummy.Collection[1].Z0_VarCharMaxInfo }, new[] { dummy.Z0_VarCharMaxInfo });

			AssertSetProperty(dummy, "RelatedDummy.Z0_VarCharMax", "Abcde", new ZString("Abcde"), @"FLD setting [RelatedDummy.Z0_VarCharMax] with [Abcde] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Value of RelatedDummy was null. Source: Abcde, Target:", null, new[] { dummy.Z0_VarCharMaxInfo, dummy.Collection[0].Z0_VarCharMaxInfo, dummy.Collection[1].Z0_VarCharMaxInfo });

			AssertSetProperty(dummy, "Z0_WrongField", "Abcde", null,
				@"Cannot find property Z0_WrongField on Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter.");
			AssertSetProperty(dummy, "Z0_VarCharMax.Text", "Abcde", null,
				@"Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter.Z0_VarCharMax of type CargoWise.Types.ZString cannot be used in field path of 'Set Field' trigger action field path - only Business object are accepted.");
			AssertSetProperty(dummy, "Collection", "Abcde", null,
				@"Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter.Collection of type DummyChildEnterpriseBusinessObjectCollectionWithAdditionalFilter cannot be used in 'Set Field' trigger action - only simple fields are supported.");
			AssertSetProperty(dummy, "PK", "Abcde", null,
				@"Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter.PK of type ZGuid is read-only and cannot be used in 'Set Field' trigger action.");
#if NETFRAMEWORK
			AssertSetProperty(dummy, "Z0_Number", "Abcde", null,
				@"Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter.Z0_Number of type ZInt cannot be set with value 'Abcde' in 'Set Field' trigger action.
Input string was not in a correct format. (in ConvertFrom, value = 'Abcde')");
#else
			AssertSetProperty(dummy, "Z0_Number", "Abcde", null,
				@"Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter.Z0_Number of type ZInt cannot be set with value 'Abcde' in 'Set Field' trigger action.
The input string 'Abcde' was not in a correct format. (in ConvertFrom, value = 'Abcde')");
#endif
			AssertSetProperty(dummy, "Collection.Z0_WrongField", "Abcde", null,
				@"Cannot find property Z0_WrongField on Enterprise.ZArchitecture.Business.Testing.DummyChildEnterpriseBusinessObject.");
		}

		public void TestSetPropertyForAdditionalType()
		{
			CombineAssertions(() =>
			{
				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				AssertSetProperty(shipment, "JS_HouseBill", "H123", new ZString("H123"), null, null);
				AssertSetProperty(shipment, "JE_HouseBill", "H123", null, @"Cannot find property JE_HouseBill on Enterprise.Freight.Forwarding.Business.ForwardingShipment.");

				var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_HouseBill = "H123C";
				declaration.JE_JS = shipment.PK;
				AssertSetProperty(shipment, "JS_HouseBill", "H123B", new ZString("H123B"), null, null);
				AssertSetProperty(shipment, "JE_GoodsDescription", "Goods", new ZString("Goods"), null, null);
				AssertSetProperty(shipment, "JS_HouseBill", "JE_HouseBill", new ZString("H123C"), null, null);
				AssertSetProperty(shipment, "Z0_WrongField", "H123", null, @"Cannot find property Z0_WrongField on Enterprise.Freight.Forwarding.Business.ForwardingShipment.");
			});
		}

		public void TestSetPropertyWithDataSourcePrefix()
		{
			CombineAssertions(() =>
			{
				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				AssertSetProperty(shipment, "JS_HouseBill", "_DataSource", new ZString("_DataSource"), null, null);
				AssertSetProperty(shipment, "JS_HouseBill", "<_DataSource>", null, MacroDataSource.EmptyDataSourceTypeError + @"
Set field (FLD) macro evaluated with errors. Macro: [<_DataSource>] Source:");
				AssertSetProperty(shipment, "JS_HouseBill", "<_DataSource.>", null, MacroDataSource.EmptyDataSourceTypeError + @"
Set field (FLD) macro evaluated with errors. Macro: [<_DataSource.>] Source:");
				AssertSetProperty(shipment, "JS_HouseBill", "<_DataSource.XXX.>", null, MacroDataSource.GetDataSourceTypeNotFoundMessage("XXX") + @"
Set field (FLD) macro evaluated with errors. Macro: [<_DataSource.XXX.>] Source:");
				AssertSetProperty(shipment, "JS_HouseBill", "<_DataSource.JobDeclaration.JE_HouseBill>", null, MacroDataSource.GetDataSourceTypeNotFoundMessage("JobDeclaration") + @"
Set field (FLD) macro evaluated with errors. Macro: [<_DataSource.JobDeclaration.JE_HouseBill>] Source:");

				AssertSetProperty(shipment, "_DataSource.", "Goods1", null, MacroDataSource.EmptyDataSourceTypeError);
				AssertSetProperty(shipment, "_DataSource.XXX", "Goods1", null, MacroDataSource.GetDataSourceTypeNotFoundMessage("XXX"));
				AssertSetProperty(shipment, "_DataSource.JobDeclaration.JE_GoodsDescription", "Goods1", null, MacroDataSource.GetDataSourceTypeNotFoundMessage("JobDeclaration"));

				var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_HouseBill = "H123";
				declaration.JE_JS = shipment.PK;
				AssertSetProperty(shipment, "JE_GoodsDescription", "Goods", new ZString("Goods"), null, null);
				AssertSetProperty(shipment, "JS_HouseBill", "<_DataSource.JobDeclaration.JE_GoodsDescription>", new ZString("Goods"), null, null);
				AssertSetProperty(shipment, "JS_HouseBill", "<_DataSource.JobDeclaration.JE_HouseBill>|<JS_HouseBill>", new ZString("H123|Goods"), null, null);

				AssertSetProperty(shipment, "_DataSource.JobDeclaration.", "Goods1", null, @"Cannot find property  on Enterprise.Customs.AU.Declaration.Business.JobDeclaration.");
				AssertSetProperty(shipment, "_DataSource.JobDeclaration.JE_GoodsDescription", "Goods1", new ZString("Goods1"), null, null);
			});
		}

		public void TestNullRootWarningOnMissingDataSource()
		{
			var rootProvider = ObjectFactory.Get<ITriggerActionRootProvider>();

			var mockRootProvider = new Mock<ITriggerActionRootProvider>();
			mockRootProvider.Setup(p => p.GetRoots(It.IsAny<ProcessTaskNotification>(), It.IsAny<BusinessObject>(), It.IsAny<IStmALog>()))
				.Returns((ProcessTaskNotification action, BusinessObject parent, IStmALog @event) =>
				{
					var result = rootProvider.GetRoots(action, parent, @event);
					return result.Where(bo => bo?.GetType().Name != "JobDeclaration").ToArray();
				});

			using (ObjectFactory.Substitute(mockRootProvider.Object))
			{
				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_HouseBill = "H123";
				declaration.JE_JS = shipment.PK;

				AssertSetProperty(shipment, "_DataSource.JobDeclaration.JE_GoodsDescription", "Goods1", expectedWarnings: @"<<<NullRoot Happened V2>>>
Action Roots:
Field Value: (Type:CargoWise.Types.ZString, Value:Goods1)
Result of GetFinalPropertyInfoAndParentType: (CargoWise.Types.ZString JE_GoodsDescription, Enterprise.Customs.AU.Declaration.Business.JobDeclaration, JE_GoodsDescription)");
			}
		}

		public void TestSetCustomFieldOfIBusinessObjectCollection()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentBO = (Forwarding.IForwardingShipment)shipment;
			shipment.SetUserDefinedValue("shipment 1", new ZInt(1));
			shipment.SetUserDefinedValue("shipment 2", new ZString("Lots Of Ice"));
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			var consolBO = (Forwarding.IForwardingConsol)consol;
			var consolShipmentLink =
				Factory.New(ObjectFactory.GetType<Freight.Integration.IJobConShipLink>());
			var consolShipmentLinkBO = (Freight.Integration.IJobConShipLink)consolShipmentLink;
			consolShipmentLinkBO.JN_JK = consolBO.PK;
			consolShipmentLinkBO.JN_JS = shipmentBO.PK;

			CombineAssertions(() =>
			{
				AssertSetProperty(consol, "Shipments.GetCustomField(shipment 1)", "2", new ZInt(2));
				AssertSetProperty(consol, "Shipments.GetCustomField(shipment 2)", "change", new ZString("change"));
				AssertSetProperty(consol, "Shipments.GetCustomField(shipment XXX)", "false", null, @"FLD setting [Shipments.GetCustomField(shipment XXX)] with [false] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Cannot find custom field 'shipment XXX' on Enterprise.Freight.Forwarding.Business.ForwardingShipment to set value 'false' by Set Field (FLD) trigger action. Target Object: 'Shipment EBM22Q33TU475BXH3P60'. Source: false, Target:");
			});
		}

		public void TestSetCustomFieldOfRelatedOrganisation()
		{
			var bizo = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipment = (Forwarding.IForwardingShipment)bizo;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.SetUserDefinedValue("Custom Field 1", new ZString("Custom Value 1"));
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = org.PK;

			CombineAssertions(() =>
			{
				AssertSetProperty(bizo, "JS_GoodsDescription", "<Consignee.GetCustomField(Custom Field 1)>", new ZString("Custom Value 1"));
				AssertSetProperty(bizo, "Consignee.GetCustomField(Custom Field 1)", "New Value", new ZString("New Value"));
			});
		}

		public void TestSetCustomFieldOfRelatedDeclaration()
		{
			var bizo = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipment = (Forwarding.IForwardingShipment)bizo;

			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration.SetUserDefinedValue("Custom Field 1", new ZString("Custom Value 1"));
			var declarationBO = (Enterprise.Integration.Customs.IBaseJobDeclaration)declaration;
			declarationBO.JE_JS = shipment.PK;

			CombineAssertions(() =>
			{
				AssertSetProperty(bizo, "JS_GoodsDescription", "<DeclarationForDocuments.GetCustomField(Custom Field 1)>", new ZString("Custom Value 1"));
				AssertSetProperty(bizo, "DeclarationForDocuments.GetCustomField(Custom Field 1)", "New Value", new ZString("New Value"));
			});
		}

		public void TestSetCustomFieldForMultiSourceData()
		{
			CombineAssertions(() =>
			{
				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment.SetUserDefinedValue("shipment 1", new ZInt(1));
				shipment.SetUserDefinedValue("shipment 2", new ZString("Lots Of Ice"));

				AssertSetProperty(shipment, "GetCustomField(shipment 1)", "2", new ZInt(2));
				AssertSetProperty(shipment, "GetCustomField(shipment 2)", "change", new ZString("change"));
				AssertSetProperty(shipment, "GetCustomField(shipment XXX)", "false", null, GetCustomFieldNotFoundError("shipment XXX", shipmentType, new ZString("false")));

				var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
				declaration.SetUserDefinedValue("declaration 1", ZBool.True);
				declaration.SetUserDefinedValue("declaration 2", new ZString("Lots Of Ice"));
				var declarationBO = (Enterprise.Integration.Customs.IBaseJobDeclaration)declaration;
				declarationBO.JE_JS = shipment.PK;

				AssertSetProperty(shipment, "GetCustomField(shipment 2)", "change 2", new ZString("change 2"));
				AssertSetProperty(shipment, "GetCustomField(shipment 1)", "3", new ZInt(3));
				AssertSetProperty(shipment, "GetCustomField(declaration 1)", "false", new ZBool("false"));
				AssertSetProperty(shipment, "GetCustomField(declaration 2)", "change", new ZString("change"));
				AssertSetProperty(shipment, "GetCustomField(shipment XXX)", "false", null, GetCustomFieldNotFoundError("shipment XXX", shipmentType, new ZString("false")));
			});
		}

		public void TestSetCustomFieldWithDataSourcePrefix()
		{
			CombineAssertions(() =>
			{
				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment.SetUserDefinedValue("shipment 1", ZBool.True);

				AssertSetProperty(shipment, "GetCustomField(shipment 1)", "false", new ZBool("false"));
				AssertSetProperty(shipment, "_DataSource.JobDeclaration.GetCustomField(shipment 1)", "false", null, MacroDataSource.GetDataSourceTypeNotFoundMessage("JobDeclaration"));

				var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
				declaration.SetUserDefinedValue("declaration 2", new ZString("Lots Of Ice"));
				var declarationBO = (Enterprise.Integration.Customs.IBaseJobDeclaration)declaration;
				declarationBO.JE_JS = shipment.PK;

				AssertSetProperty(shipment, "_DataSource.JobDeclaration.GetCustomField(declaration 2)", "change", new ZString("change"));
				AssertSetProperty(shipment, "_DataSource.JobDeclaration.GetCustomField(shipment XXX)", "false", null, @"FLD setting [_DataSource.JobDeclaration.GetCustomField(shipment XXX)] with [false] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Cannot find custom field 'shipment XXX' on Enterprise.Customs.AU.Declaration.Business.JobDeclaration to set value 'false' by Set Field (FLD) trigger action. Target Object: 'Declaration EBM22Q33TU475BXH3P60'. Source: false, Target:");
			});
		}

		public void TestSetCustomFieldWithTypeOfIBusinessObjectCollection()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentBO = (Forwarding.IForwardingShipment)shipment;
			shipment.SetUserDefinedValue("shipment 1", new ZInt(1));
			shipment.SetUserDefinedValue("shipment 2", new ZString("Lots Of Ice"));
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			var consolBO = (Forwarding.IForwardingConsol)consol;
			var consolShipmentLink =
				Factory.New(ObjectFactory.GetType<Freight.Integration.IJobConShipLink>());
			var consolShipmentLinkBO = (Freight.Integration.IJobConShipLink)consolShipmentLink;
			consolShipmentLinkBO.JN_JK = consolBO.PK;
			consolShipmentLinkBO.JN_JS = shipmentBO.PK;

			CombineAssertions(() =>
			{
				AssertSetProperty(consol, "Shipments.GetCustomFieldWithType(shipment 1, INT)", "2", new ZInt(2));
				AssertSetProperty(consol, "Shipments.GetCustomFieldWithType(shipment 1, STR)", "2", new ZInt(2), @"FLD setting [Shipments.GetCustomFieldWithType(shipment 1, STR)] with [2] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Cannot find custom field 'shipment 1' on Enterprise.Freight.Forwarding.Business.ForwardingShipment to set value '2' by Set Field (FLD) trigger action. Target Object: 'Shipment EBM22Q33TU475BXH3P60'. Source: 2, Target:");
				AssertSetProperty(consol, "Shipments.GetCustomFieldWithType(shipment 2, STR)", "change", new ZString("change"));
				AssertSetProperty(consol, "Shipments.GetCustomFieldWithType(shipment XXX, STR)", "false", null, @"FLD setting [Shipments.GetCustomFieldWithType(shipment XXX, STR)] with [false] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Cannot find custom field 'shipment XXX' on Enterprise.Freight.Forwarding.Business.ForwardingShipment to set value 'false' by Set Field (FLD) trigger action. Target Object: 'Shipment EBM22Q33TU475BXH3P60'. Source: false, Target:");
			});
		}

		public void TestSetCustomFieldWithTypeForMultiSourceData()
		{
			CombineAssertions(() =>
			{
				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment.SetUserDefinedValue("shipment 2", new ZString("Lots Of Ice"));

				AssertSetProperty(shipment, "GetCustomFieldWithType(shipment 2, STR)", "H123", new ZString("H123"));
				AssertSetProperty(shipment, "GetCustomFieldWithType(shipment XXX, STR)", "H123", null, GetCustomFieldNotFoundError("shipment XXX(STR)", shipmentType, new ZString("H123")));

				var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
				declaration.SetUserDefinedValue("declaration 1", ZBool.True);
				declaration.SetUserDefinedValue("declaration 2", new ZString("Lots Of Ice"));
				var declarationBO = (Enterprise.Integration.Customs.IBaseJobDeclaration)declaration;
				declarationBO.JE_JS = shipment.PK;

				AssertSetProperty(shipment, "GetCustomFieldWithType(shipment 2, STR)", "H1234", new ZString("H1234"));
				AssertSetProperty(shipment, "GetCustomFieldWithType(declaration 2, STR)", "H123", new ZString("H123"));
				AssertSetProperty(shipment, "GetCustomFieldWithType(shipment XXX, STR)", "H123", null, GetCustomFieldNotFoundError("shipment XXX(STR)", shipmentType, new ZString("H123")));
			});
		}

		public void TestSetCustomFieldWithTypeWithDataSourcePrefix()
		{
			CombineAssertions(() =>
			{
				var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
				shipment.SetUserDefinedValue("shipment 2", new ZString("Lots Of Ice"));

				AssertSetProperty(shipment, "GetCustomFieldWithType(shipment 2, STR)", "H123", new ZString("H123"), null, null);
				AssertSetProperty(shipment, "_DataSource.JobDeclaration.GetCustomFieldWithType(shipment 2, STR)", "false", null, MacroDataSource.GetDataSourceTypeNotFoundMessage("JobDeclaration"));

				var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
				declaration.SetUserDefinedValue("declaration 2", new ZString("Lots Of Ice"));
				var declarationBO = (Enterprise.Integration.Customs.IBaseJobDeclaration)declaration;
				declarationBO.JE_JS = shipment.PK;

				AssertSetProperty(shipment, "_DataSource.JobDeclaration.GetCustomField(declaration 2)", "change", new ZString("change"), null, null);
				AssertSetProperty(shipment, "_DataSource.JobDeclaration.GetCustomField(shipment XXX)", "false", null, @"FLD setting [_DataSource.JobDeclaration.GetCustomField(shipment XXX)] with [false] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Cannot find custom field 'shipment XXX' on Enterprise.Customs.AU.Declaration.Business.JobDeclaration to set value 'false' by Set Field (FLD) trigger action. Target Object: 'Declaration EBM22Q33TU475BXH3P60'. Source: false, Target:");
			});
		}

		public void TestSetPropertyDoesRollbackOnException()
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Collection.AddNew();
			var item = dummy.Collection.AddNew();
			item.Z0_IsSystem = false;

			item.ThrowOnSettingZ0_Bool = true;

			var logWalkerOutput = AssertSetProperty(dummy, "Collection.Z0_Bool", "Y", new ZBool(false), "", null, new[] { dummy.Z0_VarCharMaxInfo, dummy.Collection[0].Z0_VarCharMaxInfo, dummy.Collection[1].Z0_VarCharMaxInfo });
			AssertContains("Action was forbidden by internal rules.", logWalkerOutput);
		}

		public void TestSetPropertyGetsBlockedBySetter()
		{
			//this can happen if the set field trigger is trying to set a field that is in the process of being set and the setter is blocking further changes. CommonShipment.JS_E_ARV for example
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			dummy.Z0_IsSystem = false;

			AssertSetProperty(dummy, "Z0_SetterThatBlocksChanges", "xxx", expectedWarnings: @"FLD setting [Z0_SetterThatBlocksChanges] with [xxx] succeeded on 0 properties. Unchanged on 1. Failed on 0
Unchanged logs:
Target Object: 'Dummy Business Object Default', Source: , Target: ");
		}

		#endregion

		#region TestSetCustomField

		public void TestSetCustomFieldString()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", "", new GetCustomFieldStrategy(dummy).GetCustomField("Field 1"));

			AssertSetProperty(dummy, "GetCustomField(Field 1)", "Some new text", ZString.Empty);
			AssertEquals("strategy.GetCustomField(\"Custom Text\")", "Some new text", new GetCustomFieldStrategy(dummy).GetCustomField("Field 1"));

			AssertSetProperty(
				dummy,
				"GetCustomField(Field 2)",
				"Other text",
				ZString.Empty,
				GetCustomFieldNotFoundError("Field 2", "Enterprise.MasterFiles.Business.Testing.DummyWithCustomFields", new ZString("Other text")));
		}

		public void TestSetCustomFieldDecimal()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", ZDecimal.Zero, new GetCustomFieldStrategy(dummy).GetCustomField("Field 3"));

			AssertSetProperty(dummy, "GetCustomField(Field 3)", "1234.1234", ZDecimal.Zero);
			AssertEquals("strategy.GetCustomField(\"Custom Text\")", new ZDecimal("1234.1234"), new GetCustomFieldStrategy(dummy).GetCustomField("Field 3"));
		}

		public void TestSetCustomFieldDecimalOverflow()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", ZDecimal.Zero, new GetCustomFieldStrategy(dummy).GetCustomField("Field 3"));

			AssertSetProperty(
				dummy,
				"GetCustomField(Field 3)",
				"123456789123456789123456789123456789123456789123456789123456789",
				ZDecimal.Zero,
				@"Custom field 'Field 3' of type ZDecimal on Enterprise.MasterFiles.Business.Testing.DummyWithCustomFields cannot be set with value '123456789123456789123456789123456789123456789123456789123456789' in 'Set Field' trigger action.
Value was either too large or too small for a Decimal.");
			AssertEquals("strategy.GetCustomField(\"Custom Text\")", ZDecimal.Zero, new GetCustomFieldStrategy(dummy).GetCustomField("Field 3"));
		}

		public void TestSetCustomFieldInt()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", ZInt.Zero, new GetCustomFieldStrategy(dummy).GetCustomField("Field 4"));

			AssertSetProperty(dummy, "GetCustomField(Field 4)", "1234", ZInt.Zero);
			AssertEquals("strategy.GetCustomField(\"Custom Text\")", new ZInt("1234"), new GetCustomFieldStrategy(dummy).GetCustomField("Field 4"));
		}

		public void TestSetCustomFieldIntOverflow()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", ZInt.Zero, new GetCustomFieldStrategy(dummy).GetCustomField("Field 4"));

			AssertSetProperty(
				dummy,
				"GetCustomField(Field 4)",
				"123456789123456789123456789",
				ZInt.Zero,
				@"Custom field 'Field 4' of type ZInt on Enterprise.MasterFiles.Business.Testing.DummyWithCustomFields cannot be set with value '123456789123456789123456789' in 'Set Field' trigger action.
Value was either too large or too small for an Int32.");
			AssertEquals("strategy.GetCustomField(\"Custom Text\")", ZInt.Zero, new GetCustomFieldStrategy(dummy).GetCustomField("Field 4"));
		}

		public void TestSetCustomFieldBoolean()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", ZBool.False, new GetCustomFieldStrategy(dummy).GetCustomField("Field 5"));
			AssertSetProperty(dummy, "GetCustomField(Field 5)", "1", ZInt.Zero);
			AssertEquals("strategy.GetCustomField(\"Custom Text\")", new ZBool("T"), new GetCustomFieldStrategy(dummy).GetCustomField("Field 5"));
		}

		public void TestSetCustomFieldDateTime()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", ZDateTime.Empty, new GetCustomFieldStrategy(dummy).GetCustomField("Field 6"));
			AssertSetProperty(dummy, "GetCustomField(Field 6)", "01/01/1970 12:00:00", ZDateTime.Empty);
			AssertEquals("strategy.GetCustomField(\"Custom DateTime\")", new ZDateTime("01/01/1970 12:00:00"), new GetCustomFieldStrategy(dummy).GetCustomField("Field 6"));
		}

		public void TestSetCustomFieldDateTimeOutOfRange()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", ZDateTime.Empty, new GetCustomFieldStrategy(dummy).GetCustomField("Field 6"));

			AssertSetProperty(
				dummy,
				"GetCustomField(Field 6)",
				"01/01/10000 12:00:00",
				ZDateTime.Empty,
				@"Custom field 'Field 6' of type ZDateTime on Enterprise.MasterFiles.Business.Testing.DummyWithCustomFields cannot be set with value '01/01/10000 12:00:00' in 'Set Field' trigger action.
Cannot initialise a CargoWise.Types.ZDateTime with <01/01/10000 12:00:00> (CargoWise.Types.ZString).");
			AssertEquals("strategy.GetCustomField(\"Custom DateTime\")", ZDateTime.Empty, new GetCustomFieldStrategy(dummy).GetCustomField("Field 6"));
		}

		public void TestSetCustomFieldsOnEditEvent()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000100";

			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "CF");

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.EditedARecordCode;
			trigger.P9_Description = "EDT";

			Factory.Save();

			((BusinessObject)shipment).SetPossiblyCustomProperty("__CF__prop__ZString", new ZString("make me upper case"));

			Factory.Save();

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<GetCustomField(CF)>";
			action.PQ_FieldValue = "<Upper(\"<GetCustomField(CF)>\")>";

			shipment.JS_GoodsDescription = "create EDT event";

			Factory.Save();

			var logs = ((BusinessObject)shipment).GetLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode);
			AssertEquals("Should have EDT event", true, logs.Any());
			AssertEquals("IFC Action Failed to update Custom Field", "MAKE ME UPPER CASE", new GetCustomFieldStrategy((BusinessObject)shipment).GetCustomField("CF"));
		}

		public void TestSetCustomFieldWhenCustomPropertyDeleted()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000100";

			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			var customField = MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "CF");

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.EditedARecordCode;
			trigger.P9_Description = "EDT";

			Factory.Save();

			((BusinessObject)shipment).SetPossiblyCustomProperty("__CF__prop__ZString", new ZString("adventure awaits"));
			Factory.Save();

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<GetCustomField(CF)>";
			action.PQ_FieldValue = "huzzah!";

			shipment.JS_GoodsDescription = "create EDT event";
			Factory.Save();

			var logs = ((BusinessObject)shipment).GetLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode);
			AssertEquals("Should have EDT event", true, logs.Any());
			AssertEquals("IFC Action Failed to update Custom Field", "huzzah!", new GetCustomFieldStrategy((BusinessObject)shipment).GetCustomField("CF"));

			((BusinessObject)shipment).SetPossiblyCustomProperty("__CF__prop__ZString", ZString.Empty);
			Factory.Save();

			customField.Delete();
			Factory.Save();

			var notifications = new NotificationsForTest();
			FireTriggerAction((BusinessObject)shipment, trigger, action, notifications);

			AssertContains(@"Cannot find custom field 'CF' on Enterprise.Freight.Forwarding.Business.ForwardingShipment to set value 'huzzah!' by Set Field trigger action.
Set field (IFC) target not found. Macro [huzzah!] Target [Enterprise.Freight.Forwarding.Business.ForwardingShipment]", notifications.ToString());
		}

		[TestDate(2020, 01, 01)]
		public void TestWorkflowChildTriggersIfcOnWorkflowProviderCustomFields()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000100";

			var processTaskTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "CF");
			MasterFilesTestHelper.CreateCustomField(processTaskTemplate, "CF2");

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.ServiceInvoicePosted.Code;
			trigger.P9_Description = "InvoiceSent";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<GetCustomField(CF)>";
			action.PQ_FieldValue = "<GetCustomField(CF2)>";

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";

			Factory.Save();

			((BusinessObject)shipment).SetPossiblyCustomProperty("__CF2__prop__ZString", new ZString("It worked"));

			Factory.Save();

			AssertNotEquals("Shipment has JobHeader as a child", null, shipment.GetType().GetProperty("ShipmentJobHeader", BindingFlags.Instance | BindingFlags.Public).GetValue(shipment, null));
			AssertEquals("CF2 is set", "It worked", new GetCustomFieldStrategy((BusinessObject)shipment).GetCustomField("CF2"));

			var log = jobHeader.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2020, 1, 1);
				log.SL_SE_NKEvent = AutoEvents.ServiceInvoicePostedCode;
			}

			AssertEquals("IFC Action Failed to update Custom Field", "It worked", new GetCustomFieldStrategy((BusinessObject)shipment).GetCustomField("CF"));
		}

		[TestDate(2020, 01, 01)]
		public void TestSetFieldOnDeletedBizo()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000100";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			trigger.P9_Description = "InvoiceSent";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<Job.JH_Description>";
			action.PQ_FieldValue = "TEST";

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";
			Factory.Save();

			var notifications = new NotificationsForTest();
			FireTriggerAction((BusinessObject)shipment, trigger, action, notifications);

			AssertEquals("Precondition: The action works", "TEST", jobHeader.JH_Description);

			jobHeader.MarkAsInactive();

			FireTriggerAction((BusinessObject)shipment, trigger, action, notifications);

			AssertContains("Header was deleted so set field fails", @"IFC setting [Job.JH_Description] with [TEST] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Value of Job was null. Source: TEST, Target: ", notifications.ToString());
		}

		[TestDate(2020, 01, 01)]
		public void TestWorkflowChildCanAccessItsOwnFieldValue()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000100";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.ServiceInvoicePosted.Code;
			trigger.P9_Description = "InvoiceSent";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			//Field Name belongs to workflow provider
			action.PQ_FieldName = "<JS_GoodsDescription>";
			//Field Value belongs to triggering job
			action.PQ_FieldValue = "<Job.JH_GS_NKRepSales>";

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_GS_NKRepSales = "AA";

			Factory.Save();

			AssertNotEquals("Shipment has JobHeader as a child", null, shipment.GetType().GetProperty("ShipmentJobHeader", BindingFlags.Instance | BindingFlags.Public).GetValue(shipment, null));

			var log = jobHeader.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2020, 1, 1);
				log.SL_SE_NKEvent = AutoEvents.ServiceInvoicePostedCode;
			}

			AssertEquals("IFC Action Failed", "AA", shipment.JS_GoodsDescription);
		}

		[TestDate(2020, 01, 01)]
		public void TestWorkflowChildTriggersIfcOnWorkflowProvider()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000100";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.ServiceInvoicePosted.Code;
			trigger.P9_Description = "InvoiceSent";

			var action = trigger.ProcessTaskNotifications.AddNew();
			var action2 = trigger.ProcessTaskNotifications.AddNew();
			var action3 = trigger.ProcessTaskNotifications.AddNew();
			var action4 = trigger.ProcessTaskNotifications.AddNew();
			var action5 = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action4.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action5.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;

			action.PQ_FieldName = "<JS_GoodsDescription>";
			action.PQ_FieldValue = "<JS_UniqueConsignRef>";

			action2.PQ_FieldName = "<JS_CFSReference>";
			action2.PQ_FieldValue = "<TriggeringEvent.User.GS_Code>";

			action3.PQ_FieldName = "<JS_BookingReference>";
			action3.PQ_FieldValue = "<P9_Description>";

			action4.PQ_FieldName = "<JS_AdditionalTerms>";
			action4.PQ_FieldValue = "<Job.JH_GS_NKRepOps>";

			action5.PQ_FieldName = "<Job.JH_Name>";
			action5.PQ_FieldValue = "Michael Collins";

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";
			jobHeader.JH_GS_NKRepSales = "AA";

			Factory.Save();

			AssertNotEquals("Shipment has JobHeader as a child", null, shipment.GetType().GetProperty("ShipmentJobHeader", BindingFlags.Instance | BindingFlags.Public).GetValue(shipment, null));

			var log = jobHeader.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2020, 1, 1);
				log.SL_SE_NKEvent = AutoEvents.ServiceInvoicePostedCode;
			}

			AssertEquals("IFC should find value on WorkflowProvider", shipment.JS_UniqueConsignRef, shipment.JS_GoodsDescription);
			AssertEquals("IFC should find value on ProcessTaskNotification", "E", shipment.JS_CFSReference);
			AssertEquals("IFC should find value on ProcessTask", trigger.P9_Description, shipment.JS_BookingReference);
			AssertEquals("IFC should find value on EventSource", jobHeader.JH_GS_NKRepOps, shipment.JS_AdditionalTerms);
			AssertEquals("IFC can set nested object", "Michael Collins", jobHeader.JH_Name);
		}

		public void TestActionRootsWhenTriggerJobIsNotAWorkflowProvider()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00000100";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";

			var actionRoots = ((IDynamicRootProvider)action).AugmentedRoots(jobHeader);
			AssertEquals("Action roots should include triggering job", "Enterprise.Freight.Forwarding.Business.ForwardingShipment, Enterprise.Accounting.Business.JobInvoicing.Job, Enterprise.Freight.Forwarding.Business.ForwardingShipmentProcessTask, Enterprise.Freight.Forwarding.Business.ForwardingShipmentProcessTaskNotification", string.Join(", ", actionRoots.Select(s => s.ToString())));
		}

		[TestDate(2020, 01, 01)]
		public void TestQuotedBookingSetFieldTrigger_QuickBooking()
		{
			QuotedBookingSetFieldTrigger("IFC", Source.Booking, Freight.Integration.QuoteBookingType.QuickBooking, 1);
			QuotedBookingSetFieldTrigger("IFC", Source.Job, Freight.Integration.QuoteBookingType.QuickBooking, 2);
			QuotedBookingSetFieldTrigger("IFC", Source.Shipment, Freight.Integration.QuoteBookingType.QuickBooking, 3);

			QuotedBookingSetFieldTrigger("FLD", Source.Booking, Freight.Integration.QuoteBookingType.QuickBooking, 4);
			QuotedBookingSetFieldTrigger("FLD", Source.Job, Freight.Integration.QuoteBookingType.QuickBooking, 5);
			QuotedBookingSetFieldTrigger("FLD", Source.Shipment, Freight.Integration.QuoteBookingType.QuickBooking, 6);
		}

		[TestDate(2020, 01, 01)]
		public void TestQuotedBookingSetFieldTrigger_BookingWithQuote()
		{
			QuotedBookingSetFieldTrigger("IFC", Source.Booking, Freight.Integration.QuoteBookingType.BookingWithQuote, 1);
			QuotedBookingSetFieldTrigger("IFC", Source.Job, Freight.Integration.QuoteBookingType.BookingWithQuote, 2);
			QuotedBookingSetFieldTrigger("IFC", Source.Shipment, Freight.Integration.QuoteBookingType.BookingWithQuote, 3);

			QuotedBookingSetFieldTrigger("FLD", Source.Booking, Freight.Integration.QuoteBookingType.BookingWithQuote, 4);
			QuotedBookingSetFieldTrigger("FLD", Source.Job, Freight.Integration.QuoteBookingType.BookingWithQuote, 5);
			QuotedBookingSetFieldTrigger("FLD", Source.Shipment, Freight.Integration.QuoteBookingType.BookingWithQuote, 6);
		}

		void QuotedBookingSetFieldTrigger(string actionType, Source source, Freight.Integration.QuoteBookingType bookingType, int index)
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.QuotedBookingWorkflowDescriptorCode);
			template.P0_Name = "T" + index;
			template.P0_SubType2 = bookingType == Freight.Integration.QuoteBookingType.BookingWithQuote ? "BWQ" : "QBN";

			MasterFilesTestHelper.CreateCustomField(template, "CF");
			MasterFilesTestHelper.CreateCustomField(template, "GET");
			MasterFilesTestHelper.CreateCustomField(template, "SET");

			Factory.Save();

			var builder = ObjectFactory.Get<IQuotedBookingBuilder>();
			var quotedBooking = builder.CreateNew(bookingType, Factory);
			var shipment = ((Forwarding.IForwardingShipment)quotedBooking.ForwardingShipment);
			shipment.JS_UniqueConsignRef = "S001000" + index;

			if (bookingType == Freight.Integration.QuoteBookingType.BookingWithQuote)
			{
				((IQuote)quotedBooking.Quote).TH_QuoteNumber = "S001000" + index;
			}

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			var job = new JobHeader.Loader((IJobHeaderParent)quotedBooking).TryLoadOrCreate();
			job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;

			var workflowProvider = (IWorkflowProvider)quotedBooking;

			var task = workflowProvider.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "TASK";
			task.P9_Type = "ESM";
			task.P9_Status = "ASN";

			var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "123";

			var action = trigger.ProcessTaskNotifications.AddNew();
			var action2 = trigger.ProcessTaskNotifications.AddNew();
			var action3 = trigger.ProcessTaskNotifications.AddNew();
			var action4 = trigger.ProcessTaskNotifications.AddNew();
			var action5 = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = actionType;
			action2.PQ_TriggerType = actionType;
			action3.PQ_TriggerType = actionType;
			action4.PQ_TriggerType = actionType;
			action5.PQ_TriggerType = actionType;

			action.PQ_FieldName = "<Booking.JS_A_RCV>";
			action.PQ_FieldValue = "<Now>";

			action2.PQ_FieldName = "<Booking.JS_InterimReceipt>";
			action2.PQ_FieldValue = "<if(\"<GetCustomField(CF)>\"==\"N\", \"N-<QuotedBookingNumber>\",\"Haz-<QuotedBookingNumber>\")>";

			action3.PQ_FieldName = bookingType == Freight.Integration.QuoteBookingType.BookingWithQuote ? "<Job.JH_GS_NKRepSales>" : "<Booking.Job.JH_GS_NKRepSales>";
			action3.PQ_FieldValue = "<LoginCode>";

			action4.PQ_FieldName = "<GetCustomField(SET)>";
			action4.PQ_FieldValue = "<GetCustomField(GET)>";

			action5.PQ_FieldName = "<WorkflowItems.Where('<IsTask>'=='Y'&&'<P9_Description>'=='TASK' && '<P9_Type>'=='ESM').P9_Status>";
			action5.PQ_FieldValue = "CLS";

			Factory.Save();

			((BusinessObject)quotedBooking).SetPossiblyCustomProperty("__CF__prop__ZString", new ZString("N"));
			((BusinessObject)quotedBooking).SetPossiblyCustomProperty("__GET__prop__ZString", new ZString("GET"));

			StmALog log = null;
			//source shouldn't matter, once the log is causing the trigger to fire the action should work
			switch (source)
			{
				case Source.Job:
					log = ((IStmALogParent)job).Logs.AddNew();
					break;
				case Source.Shipment:
					log = ((IStmALogParent)shipment).Logs.AddNew();
					break;
				case Source.Booking:
					log = ((IStmALogParent)quotedBooking).Logs.AddNew();
					break;
			}

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = new ZDateTime(2020, 1, 1);
				log.SL_SE_NKEvent = AutoEvents.CustomisableEvent00Code;
			}

			if (actionType == "FLD")
			{
				Factory.Save();
				ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
				var reloadFactory = new BusinessObjectFactory();
				task = reloadFactory.Load<ProcessTask>(task.PK);
				job = reloadFactory.Load<JobHeader>(job.PK);
				shipment = reloadFactory.Load<Forwarding.IForwardingShipment>(shipment.PK);
			}

			AssertEquals(ZDateTime.Now, shipment.JS_A_RCV);
			AssertContains("N-S001000" + index, shipment.JS_InterimReceipt);
			AssertEquals("GET", new GetCustomFieldStrategy((BusinessObject)quotedBooking).GetCustomField("SET", AddOnColumnDataType.Codes.String));
			AssertEquals("CLS", task.P9_Status);
			AssertEquals("E", job.JH_GS_NKRepSales);
		}

		enum Source
		{
			Booking,
			Job,
			Shipment
		}

		public void TestSetCustomField_WithDuplicate()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", "", new GetCustomFieldStrategy(dummy).GetCustomField("Duplicate Field", AddOnColumnDataType.Codes.String));

			AssertSetProperty(dummy, "GetCustomField(Duplicate Field)", "Some new text", ZString.Empty);
			AssertSetProperty(dummy, "GetCustomField(Duplicate Field)", "19", ZString.Empty);
			AssertEquals("Some new text", new GetCustomFieldStrategy(dummy).GetCustomField("Duplicate Field", AddOnColumnDataType.Codes.String));
			AssertEquals(19, new GetCustomFieldStrategy(dummy).GetCustomField("Duplicate Field", AddOnColumnDataType.Codes.Integer));
		}

		public void TestSetCustomField_WithType()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", "", new GetCustomFieldStrategy(dummy).GetCustomField("Duplicate Field", AddOnColumnDataType.Codes.String));

			AssertSetProperty(dummy, "GetCustomFieldWithType(Duplicate Field, STR)", "42", ZString.Empty);
			AssertSetProperty(dummy, "GetCustomFieldWithType(Duplicate Field, INT)", "19", ZString.Empty);
			AssertEquals("42", new GetCustomFieldStrategy(dummy).GetCustomField("Duplicate Field", AddOnColumnDataType.Codes.String));
			AssertEquals(19, new GetCustomFieldStrategy(dummy).GetCustomField("Duplicate Field", AddOnColumnDataType.Codes.Integer));
		}

		public void TestSetCustomField_WithType_Invalid()
		{
			SetUpCustomFields(Factory);
			var dummy = Factory.New<DummyWithCustomFields>();

			AssertEquals("strategy.GetCustomField(\"Custom Text\")", "", new GetCustomFieldStrategy(dummy).GetCustomField("Duplicate Field", AddOnColumnDataType.Codes.String));

			AssertSetProperty(dummy, "GetCustomFieldWithType(Duplicate Field, BOO)", "false", null, GetCustomFieldNotFoundError("Duplicate Field(BOO)", "Enterprise.MasterFiles.Business.Testing.DummyWithCustomFields", new ZString("false")));

			AssertSetProperty(dummy, "GetCustomFieldWithType(Duplicate Field, DUD)", "", ZString.Empty,
				expectedWarnings: @"FLD setting [GetCustomFieldWithType(Duplicate Field, DUD)] with [] succeeded on 0 properties. Unchanged on 1. Failed on 0
Unchanged logs:
Target Object: 'record', Source: , Target: ");
			AssertEquals("", new GetCustomFieldStrategy(dummy).GetCustomField("Duplicate Field", AddOnColumnDataType.Codes.String));
		}

		void SetUpCustomFields(BusinessObjectFactory factory)
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var template = factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Dummy Task Template";
			template.P0_ProcessType = "DUM";

			MasterFilesTestHelper.CreateCustomField(template, "Field 1", AddOnColumnDataType.Codes.String);
			// No field 2 deliberate for tests
			MasterFilesTestHelper.CreateCustomField(template, "Field 3", AddOnColumnDataType.Codes.Decimal);
			MasterFilesTestHelper.CreateCustomField(template, "Field 4", AddOnColumnDataType.Codes.Integer);
			MasterFilesTestHelper.CreateCustomField(template, "Field 5", AddOnColumnDataType.Codes.Boolean);
			MasterFilesTestHelper.CreateCustomField(template, "Field 6", AddOnColumnDataType.Codes.Datetime);

			MasterFilesTestHelper.CreateCustomField(template, "Duplicate Field", AddOnColumnDataType.Codes.String);
			MasterFilesTestHelper.CreateCustomField(template, "Duplicate Field", AddOnColumnDataType.Codes.Integer);

			factory.Save();
		}

		#endregion

		#region WorkflowSetFieldReadonly
		public void TestWorkflowSetFieldReadonly()
		{
			var dummy = Factory.New<DumpWorkflowSetFieldReadonly>();
			dummy.Z0_IsSystem = false;

			AssertSetProperty(
				dummy,
				"Z0_Description",
				"Abcde",
				new ZString(""),
				@"Property Enterprise.Workflow.Business.Testing.WorkflowSetFieldProcessorTest+DumpWorkflowSetFieldReadonly.Z0_Description of type ZString is read-only and cannot be used in 'Set Field' trigger action.",
				new[] { dummy.Z0_DescriptionInfo });
		}

		class DumpWorkflowSetFieldReadonly : DummyWithWorkflow
		{
			public DumpWorkflowSetFieldReadonly(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override string GetWorkflowType()
			{
				return "DUM";
			}

			[WorkflowSetFieldReadonly]
			public override ZString Z0_Description { get => base.Z0_Description; set => base.Z0_Description = value; }
		}
		#endregion

		#region DisableWorkflowSettingPropertiesAfterOnSaving
		public void TestDisableWorkflowSettingPropertiesAfterOnSaving()
		{
			using (new WorkflowDescriptorsForAttributesTest())
			{
				var dummy = Factory.New<DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving>();
				dummy.Z0_IsSystem = false;

				var trigger = (DummyProcessTask)dummy.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.EditedARecordCode;
				trigger.P9_Description = "Edited a record";
				trigger.OverriddenParentTypeForTest = typeof(DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving);

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
				action.PQ_FieldName = "<Z0_Description>";
				action.PQ_FieldValue = "Set";
				dummy.Z0_Description = "Not set";

				Factory.Save();

				var logs = dummy.GetLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode);
				AssertEquals("Should have EDT event", 1, logs.Count());
				AssertEquals("Should have fired trigger", 99, (int)trigger.P9_TriggerFiredCountdown);
				AssertEquals("Should not set read only when saving field", "Not set", dummy.Z0_Description);
				AssertCollectionContains("Should add set field warning to trigger", trigger.Notifications, s => s.Message.Contains("Class DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving is protected from making changes when saving and its properties cannot be changed by the Immediate Field Change (IFC) trigger action"));
			}
		}

		public void TestDisableWorkflowSettingPropertiesAfterOnSavingAppliesToSubFields()
		{
			using (new WorkflowDescriptorsForAttributesTest())
			{
				var dummy = Factory.New<DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving>();
				dummy.Z0_IsSystem = false;

				var trigger = (DummyProcessTask)dummy.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.EditedARecordCode;
				trigger.P9_Description = "Edited a record";
				trigger.OverriddenParentTypeForTest = typeof(DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving);

				var subFieldAction = trigger.ProcessTaskNotifications.AddNew();
				subFieldAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
				subFieldAction.PQ_FieldName = "<RelatedDummyWithTasks.Z0_Description>";
				subFieldAction.PQ_FieldValue = "Set";
				dummy.InitRelatedDummyWithTasks();
				dummy.RelatedDummyWithTasks.Z0_IsSystem = false;
				dummy.RelatedDummyWithTasks.Z0_Description = "Not set";

				Factory.Save();

				var logs = dummy.GetLogs().Find(l => l.SL_SE_NKEvent == AutoEvents.EditedARecordCode);
				AssertEquals("Should have EDT event", 1, logs.Count());
				AssertEquals("Should have fired trigger", 99, (int)trigger.P9_TriggerFiredCountdown);
				AssertEquals("Should not set read only when saving subfield", "Not set", dummy.RelatedDummyWithTasks.Z0_Description);
				AssertCollectionContains("Should add set subfield warning to trigger", trigger.Notifications, s => s.Message.Contains("Class DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving is protected from making changes when saving and its properties cannot be changed by the Immediate Field Change (IFC) trigger action"));
			}
		}

		public void TestDisableWorkflowSettingPropertiesAfterOnSavingOutsideSave()
		{
			using (new WorkflowDescriptorsForAttributesTest())
			{
				var dummy = Factory.New<DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving>();
				dummy.Z0_IsSystem = false;

				AssertSetProperty(
					dummy,
					"Z0_Description",
					"Abcde",
					new ZString("Abcde"),
					null,
					new[] { dummy.Z0_DescriptionInfo });
			}
		}

		class DummyWorkflowDescriptorForAttributesTest : DummyWorkflowDescriptor
		{
			public override Type WorkflowProviderType => typeof(DumpWorkflowDisableWorkflowSettingPropertiesAfterOnSaving);
			public override string Code => "DUA";
		}

		public class WorkflowDescriptorsForAttributesTest : WorkflowDescriptors, IDisposable
		{
			public WorkflowDescriptorsForAttributesTest()
			{
				var descriptor = new DummyWorkflowDescriptorForAttributesTest();
				Assert("Precondition", descriptor.WorkflowProviderType.GetCustomAttribute<DisableWorkflowSettingPropertiesAfterOnSavingAttribute>() != null);
				AddDescriptor(new DummyWorkflowDescriptorForAttributesTest());
				overrideDescriptorsDelegate = OverrideWorkflowDescriptorsDelegate(() => this);
			}

			public void Dispose()
			{
				overrideDescriptorsDelegate.Dispose();
			}

			readonly IDisposable overrideDescriptorsDelegate;
		}
		#endregion

		#region TestSetCustomFieldForPropertyWithMaxLength

		public void TestSetCustomFieldForPropertyWithMaxLength()
		{
			var dummy = Factory.New<DummyWithWrappedCodeInCustomField>();

			dummy.Z0_Code = "TEST";
			AssertEquals("TEST", new GetCustomFieldStrategy(dummy).GetCustomField("Custom Code"));

			AssertSetProperty(dummy, "GetCustomField(Custom Code)", "12345");
			AssertEquals("12345", new GetCustomFieldStrategy(dummy).GetCustomField("Custom Code"));
			AssertEquals("12345", dummy.Z0_Code);

			AssertSetProperty(
				dummy,
				"GetCustomField(Custom Code)",
				"ABCDEFGHIJK",
				expectedWarnings: @"FLD setting [GetCustomField(Custom Code)] with [ABCDEFGHIJK] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Custom field 'Custom Code' cannot be set with value 'ABCDEFGHIJK' by Set Field (FLD) trigger action because it exceeds maximum length of 5. Target Object: 'Dummy Business Object Default'. Source: ABCDEFGHIJK, Target:");

			AssertEquals("Should remain previous value", "12345", new GetCustomFieldStrategy(dummy).GetCustomField("Custom Code"));
			AssertEquals("Should remain previous value", "12345", dummy.Z0_Code);
		}

		[UserDefinedValues]
		class DummyWithWrappedCodeInCustomField : DummyWithWorkflow, ICustomFieldProvider
		{
			public DummyWithWrappedCodeInCustomField(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public CustomBusinessObject GetCustomBusinessObject(bool shouldRefresh = false)
			{
				if (customBizo == null || shouldRefresh)
				{
					var customPropertyCollection = new UserDefinedPropertyCollection(this);
					customPropertyCollection.Add(ZGuid.NewZGuid(), DummyBizoSchema.Constants.Z0_Code, "Custom Code");

					customBizo = new CustomBusinessObject(Factory, this, customPropertyCollection);
					RegisterEditableChildObject(customBizo);
				}
				return customBizo;
			}

			CustomBusinessObject customBizo;
		}

		public class DummyWithWorkflowWithDependents : DummyWithWorkflow
		{
			public DummyWithWorkflowWithDependents(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			#region Dependents

			[ChildEditable]
			public DummyWithWorkflowDependentBusinessObjectCollection Dependents
			{
				get
				{
					if (fDependents == null)
					{
						fDependents = new DummyWithWorkflowDependentBusinessObjectCollection(this, Factory);
						fDependents.Load();

						RegisterEditableChildObject(fDependents);
					}

					return fDependents;
				}
			}
			DummyWithWorkflowDependentBusinessObjectCollection fDependents;

			public ActiveBusinessObjectCollection<DummyDependentFromWorkflowCollection> ActiveDependents
			{
				get
				{
					DependentRelationship relationship = new DependentRelationship(this, typeof(DummyDependentFromWorkflowCollection));
					return new ActiveBusinessObjectCollection<DummyDependentFromWorkflowCollection>(Factory, relationship);
				}
			}

			#endregion
		}

		public class DummyDependentFromWorkflowCollection : DummyDependantBusinessObject
		{
			public DummyDependentFromWorkflowCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public DummyWithWorkflowWithDependents MyParent { get; set; }

			public ZString Salutation { get; set; }
		}

		public class DummyWithWorkflowDependentBusinessObjectCollection : DependentBusinessObjectCollection<DummyDependentFromWorkflowCollection, DummyWithWorkflowWithDependents>
		{
			public DummyWithWorkflowDependentBusinessObjectCollection(DummyWithWorkflowWithDependents master) : base(master)
			{
			}

			public DummyWithWorkflowDependentBusinessObjectCollection(BusinessObjectFactory factory) : base(factory)
			{
			}

			public DummyWithWorkflowDependentBusinessObjectCollection(DummyWithWorkflowWithDependents master, BusinessObjectFactory factory) : base(master, factory)
			{
			}

			public DummyWithWorkflowDependentBusinessObjectCollection(DummyWithWorkflowWithDependents master, ZQuery additionalFilter) : base(master, additionalFilter)
			{
			}

			public DummyWithWorkflowDependentBusinessObjectCollection(DummyWithWorkflowWithDependents master, BusinessObjectFactory factory, bool allowMasterFactoryToBeDifferent) : base(master, factory, allowMasterFactoryToBeDifferent)
			{
			}

			protected override void OnAdded(BusinessObject bizOAdded)
			{
				base.OnAdded(bizOAdded);
				((DummyDependentFromWorkflowCollection)bizOAdded).MyParent = this.Master;
			}
		}

		#endregion

		#region TestSetPropertyOnCollectionElementUnmatchingFilter

		public void TestSetPropertyOnCollectionElementUnmatchingFilter()
		{
			var dummy = Factory.New<DummyWithDependentsWithRelatedNotes>();
			var related = Factory.New<DummyEnterpriseBusinessObject>();
			dummy.SetBusinessObjectsWithRelatedNotes(new BusinessObject[] { related });

			var note1 = CreateNoteInDB(dummy, "Note1");
			AssertEquals("Precondition", "AAA", note1.ST_NoteContext);
			AssertEquals("Precondition", "A", note1.ST_NoteContextDirection);
			var note2 = CreateNoteInDB(related, "Note2");
			AssertEquals("Precondition", "AAA", note2.ST_NoteContext);
			AssertEquals("Precondition", "A", note2.ST_NoteContextDirection);

			dummy.OverriddenNoteContextsForRelatedNotes = new StmNoteContexts
			{
				Module = StmNoteContextModule.A,
				Direction = StmNoteContextDirection.I, // Filter == I(mport)
				FreightMode = StmNoteContextFreightMode.A
			};

			AssertEquals("All elements should be in visible notes", 2, dummy.Notes.VisibleNotes.Count);

			var newContext = (ZString)new StmNoteContexts
			{
				Module = StmNoteContextModule.A,
				Direction = StmNoteContextDirection.E, // Elements set to E(xport)
				FreightMode = StmNoteContextFreightMode.A
			}.ToString();

			var trigger = Factory.New<ProcessTask>();
			var action = Factory.New<ProcessTaskNotification>();
			var setFieldTrigger = new WorkflowSetFieldProcessorForTest(new WorkflowTriggerActionSource(dummy, trigger, action, new ExampleLog(action), null));

			setFieldTrigger.PropertyToIgnoreReadonly = AutoStmNote.Schema.ST_NoteContext;
			AssertEquals("", setFieldTrigger.SetPropertyExposedForTest(dummy, "Notes.VisibleNotes.ST_NoteContext", newContext));

			AssertEquals("Only 1 element should remain visible", 1, dummy.Notes.VisibleNotes.Count);
			AssertEquals(note1.PK, dummy.Notes.VisibleNotes[0].PK);
		}

		public void TestSetProperty_UseRootProvider()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var otherDummy = Factory.NewWithValidTestData<OrgHeader>();
			otherDummy.OH_FullName = "BigBoffin";

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "SwitchBoardOn";

			var notification = Factory.New<ProcessTaskNotification>();
			notification.PQ_P9 = trigger.PK;

			DummyWorkflowDescriptor.Instance.ExtraMacroBizo.Value = otherDummy;

			AssertSetProperty(
				propertyPath: "<Z0_Description>",
				valuePath: "<OH_FullName>",
				expectedValues: new IZType[] { new ZString("BigBoffin") },
				action: notification,
				expectedWarnings: null,
				expectedProperties: new[] { dummy.Z0_DescriptionInfo });
		}

		StmNote CreateNoteInDB(BusinessObject parent, string description)
		{
			if (!parent.IsInDatabase)
			{
				parent.Factory.Save();
			}

			var note = new BusinessObjectFactory().New<StmNote>();
			note.ST_ParentID = parent.PK;
			note.ST_Description = description;
			note.ST_Table = AutoDummyBizo.Schema.TableName;
			note.ST_NoteType = nameof(StmNoteVisibility.INT);
			note.Factory.Save();

			return note;
		}

		class DummyWithDependentsWithRelatedNotes : DummyWithDependentsEnterpriseBusinessObject
		{
			public DummyWithDependentsWithRelatedNotes(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SetBusinessObjectsWithRelatedNotes(BusinessObject[] relatedBusinessObjects)
			{
				this.relatedBusinessObjects = relatedBusinessObjects;
			}

			public override BusinessObject[] BusinessObjectsWithRelatedNotes
			{
				get { return relatedBusinessObjects; }
			}
			BusinessObject[] relatedBusinessObjects;

			protected override StmNoteContexts NoteContextsForRelatedNotes
			{
				get { return OverriddenNoteContextsForRelatedNotes ?? base.NoteContextsForRelatedNotes; }
			}

			public StmNoteContexts OverriddenNoteContextsForRelatedNotes { get; set; }
		}

		#endregion

		#region TestDoNotSetPropertyIfItCausesValidationError

		public void TestDoNotSetPropertyIfItCausesValidationError()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			AssertSetProperty(dummy, "Z0_Description", "Abcde", new ZString("Abcde"), @"FLD setting [Z0_Description] with [Abcde] succeeded on 1 properties. Unchanged on 0. Failed on 0
Success logs:
Target Object: 'Dummy Business Object Abcde', Source: , Target: Abcde", new[] { dummy.Z0_DescriptionInfo });
			AssertSetProperty(dummy, "Z0_Description", "MessageError", new ZString("MessageError"), null, new[] { dummy.Z0_DescriptionInfo });
			AssertSetProperty(dummy, "Z0_Description", "Bad", null, @"FLD setting [Z0_Description] with [Bad] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.Z0_Description of type ZString has validation error if set with value 'Bad' by Set Field (FLD) trigger action. Target Object: 'Dummy Business Object Bad'. Validation Error: 'Bad!'. Source: MessageError, Target: Bad");
			AssertEquals("Should remain previous value", "MessageError", dummy.Z0_Description);
		}

		#endregion

		#region TestCalculatedPropertyChangedBySetFieldActionIsValidated

		public void TestCalculatedPropertyChangedBySetFieldActionIsValidated()
		{
			var shipment = (Forwarding.IForwardingShipment)(Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>()));
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			shipment.JS_ActualVolume = 10;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.MegaLitre;
			var oldChargeable = shipment.JS_ActualChargeable;

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Test Trigger";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<JS_ActualVolume>";
			action.PQ_FieldValue = "1000";

			var logs = ((BusinessObject)shipment).GetLogs();
			logs.AddNew(AutoEvents.CustomisableEvent00);

			Factory.Save();

			var logWalkerLogs = MasterFilesTestHelper.RunLogWalker();
			AssertContains("LogWalker shows warning", "Properties with errors: [JS_ActualChargeable,", logWalkerLogs);
			AssertEquals("Volume unchanged", new ZDecimal(10), shipment.JS_ActualVolume);
			AssertEquals("Chargeable unchanged", oldChargeable, shipment.JS_ActualChargeable);
		}

		#endregion

		#region TestGetEventReferenceValueBySetFieldAction

		public void TestGetEventReferenceValueBySetFieldAction()
		{
			var shipment = (Forwarding.IForwardingShipment)(Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>()));
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			shipment.JS_GoodsDescription = "Test Desc";

			var trigger = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "Test Trigger";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<JS_GoodsDescription>";
			action.PQ_FieldValue = "<GetEventReferenceValue(\"<SL_Reference>\",\"ACT\")>";

			var log = ((BusinessObject)shipment).GetLogs().AddNew(AutoEvents.CustomisableEvent00);

			log.SetReferenceWithLock("AAA|BBB|{0}=ACTTEST|{1}=OLDTEST", "ACT", "OLD");

			Factory.Save();

			var logWalkerLogs = MasterFilesTestHelper.RunLogWalker();

			var reloadFactory = new BusinessObjectFactory();
			var reloadshipment = reloadFactory.Load<Forwarding.IForwardingShipment>(shipment.PK);

			AssertContains("succeeded on 1 properties", logWalkerLogs);
			AssertEquals("Field value set correctly", "ACTTEST", reloadshipment.JS_GoodsDescription);
		}

		#endregion

		#region TestDoNoSetPropertyIfItIsHidden
		public void TestDoNoSetPropertyIfItIsHidden()
		{
			var dummy = Factory.New<DummyWithHiddenField>();
			AssertSetProperty(
				dummy,
				"Z0_Description",
				"Abcde",
				new ZString(""),
				@"Property Enterprise.MasterFiles.Business.Testing.DummyWithHiddenField.Z0_Description of type ZString is read-only and cannot be used in 'Set Field' trigger action.",
				new[] { dummy.Z0_DescriptionInfo });
		}
		#endregion

		#region TestDoNotSetPropertyIfItIsReadOnly

		public void TestDoNotSetPropertyIfItIsReadOnly()
		{
			var dummy = Factory.New<DummyWithReadOnlyActionField>();
			AssertSetProperty(
				dummy,
				"Z0_Description",
				"Abcde",
				new ZString(""),
				@"Property Enterprise.MasterFiles.Business.Testing.DummyWithReadOnlyActionField.Z0_Description of type ZString is read-only and cannot be used in 'Set Field' trigger action.",
				new[] { dummy.Z0_DescriptionInfo });
		}

		public void TestCanGetReadOnlyField()
		{
			var dummy = Factory.New<DummyWithReadOnlyActionField>();
			dummy.Z0_IsSystem = false;
			dummy.Z0_Description = "test";
			AssertSetProperty(
				dummy,
				"Z0_NVarChar",
				"<Z0_Description>",
				new ZString("test"),
				@"FLD setting [Z0_NVarChar] with [<Z0_Description>] succeeded on 1 properties. Unchanged on 0. Failed on 0
Success logs:
Target Object: 'Dummy Business Object test', Source: , Target: test",
				new[] { dummy.Z0_NVarCharInfo });
		}

		#endregion

		#region TestDoNotSetPropertyIfItCausesTheMaximumLengthOfPropertyIsExceeded

		public void TestDoNotSetPropertyIfItCausesTheMaximumLengthOfPropertyIsExceeded()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			ZString exceededMaxLength = "AbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkg";

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Description",
				valuePath: "Abcde",
				expectedValue: new ZString("Abcde"),
				expectedProperties: new[] { dummy.Z0_DescriptionInfo });

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Description",
				valuePath: "MessageError",
				expectedValue: new ZString("MessageError"),
				expectedProperties: new[] { dummy.Z0_DescriptionInfo });

			AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Description",
				valuePath: exceededMaxLength,
				expectedWarnings: $@"FLD setting [Z0_Description] with [AbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkg] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.Z0_Description of type ZString cannot be set to value 'AbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkAbcdeghijkg' by Set Field (FLD) trigger action because it exceeds maximum length of 100.");
			AssertEquals("Should remain previous value", "MessageError", dummy.Z0_Description);

			using (dummy.GetValidationSuspender())
			{
				AssertEquals("Validation Suspended", true, dummy.IsValidationSuspended);

				AssertSetProperty(
					dummy,
					propertyPath: "Z0_Description",
					valuePath: "error",
					expectedValue: new ZString(""),
					expectedProperties: new[] { dummy.Z0_DescriptionInfo });
				AssertEquals("No notifications", false, dummy.Z0_DescriptionInfo.HasNotifications());
			}
		}

		public void TestPropertyWithNoZPropertyInfoMaxLength()
		{
			var dummy = Factory.New<DummyWithExtra>();
			dummy.Z0_IsSystem = false;

			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.EditedARecordCode;
			trigger.P9_Description = "EDT";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<CodeWrapper>";
			action.PQ_FieldValue = "THIS_IS_TOO_LONG";

			((DummyProcessTask)trigger).OverriddenParentTypeForTest = typeof(DummyWithExtra);

			Factory.Save();

			var notifications = new NotificationsForTest();
			FireTriggerAction(dummy, trigger, action, notifications);

			AssertContains("because it exceeds maximum length", notifications.ToString());
		}

		#endregion

		#region TestSetFieldsWithMacros

		[TestDate(2011, 3, 17, 14, 39, 47)]
		[TestUtcOffset(0, 0, 0)]
		public void TestSetFieldsWithMacros()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Code = "XYZ";
			dummy.Z0_Number = 123;
			dummy.Z0_IsSystem = false;

			AssertSetProperty(dummy, "Z0_Description", "<Z0_Code> - <Z0_Number>", new ZString("XYZ - 123"), null, new[] { dummy.Z0_DescriptionInfo });
			AssertSetProperty(dummy, "Z0_Date", "<Now>", new ZDateTime(2011, 3, 17, 14, 39, 47), null, new[] { dummy.Z0_DateInfo });
			AssertSetProperty(dummy, "Z0_VarCharMax", "<PQ_TriggerType>", new ZString("FLD"), null, new[] { dummy.Z0_VarCharMaxInfo });
		}

		#endregion

		#region TestGetFinalPropertyInfo

		public void TestGetFinalPropertyInfo()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Collection.AddNew();
			dummy.Collection.AddNew();

			AssertGetPropertyInfoAndParentType(dummy, "Z0_VarCharMax", "Z0_VarCharMax", "", "Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow");
			AssertGetPropertyInfoAndParentType(dummy, "Collection.Z0_VarCharMax", "Z0_VarCharMax", "", "Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow");

			AssertGetPropertyInfoAndParentType(dummy, "Z0_WrongField", null, "Cannot find property Z0_WrongField on Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.");
		}

		public void TestGetFinalPropertyInfoForMultiTypes()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "JS_HouseBill", "JS_HouseBill", "", "Enterprise.Freight.Forwarding.Business.ForwardingShipment");
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "JE_HouseBill", null, PropertyNotFoundOnShipmentError("JE_HouseBill"));

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "JS_HouseBill", "JS_HouseBill", "", "Enterprise.Freight.Forwarding.Business.ForwardingShipment");
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "JE_HouseBill", "JE_HouseBill", "", "Enterprise.Customs.AU.Declaration.Business.JobDeclaration");
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "Z0_WrongField", null, PropertyNotFoundOnShipmentError("Z0_WrongField") + "\r\n" + PropertyNotFoundOnDeclarationError("Z0_WrongField"));
		}

		public void TestGetFinalPropertyInfoForMultiTypes_WithDataSourcePrefix()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "_DataSource.", null, MacroDataSource.EmptyDataSourceTypeError);
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "_DataSource.XX", null, MacroDataSource.GetDataSourceTypeNotFoundMessage("XX"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "_DataSource.JobDeclaration.JE_HouseBill", null, MacroDataSource.GetDataSourceTypeNotFoundMessage("JobDeclaration"));

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "_DataSource.JobDeclaration.JE_HouseBill", "JE_HouseBill", "", "Enterprise.Customs.AU.Declaration.Business.JobDeclaration");
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "_DataSource.JobDeclaration.Z0_WrongField", null, PropertyNotFoundOnDeclarationError("Z0_WrongField"), "Enterprise.Customs.AU.Declaration.Business.JobDeclaration");
		}

		public void TestGetFinalPropertyInfo_DuplicatePropertyForMultiTypes()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "Consignee", null, "Property Enterprise.Freight.Forwarding.Business.ForwardingShipment.Consignee of type OrgHeader cannot be used in 'Set Field' trigger action - only simple fields are supported.");
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "Consignee.OH_FullName", "OH_FullName", "", "Enterprise.Freight.Forwarding.Business.ForwardingShipment");

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "Consignee.OH_FullName", "OH_FullName", "", "Enterprise.Freight.Forwarding.Business.ForwardingShipment");
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "_DataSource.JobDeclaration.Consignee.OH_FullName", "OH_FullName", "", "Enterprise.Customs.AU.Declaration.Business.JobDeclaration");
		}

		static string PropertyNotFoundOnDeclarationError(ZString propertyName) => $"Cannot find property {propertyName} on {decType}.";

		static string PropertyNotFoundOnShipmentError(ZString propertyName) => $"Cannot find property {propertyName} on {shipmentType}.";

		static string GetCustomFieldNotFoundError(ZString fieldName, ZString type, IZType value = null)
		{
			var valueString = value == null ? string.Empty : $" to set value '{value}'";
			return $"Cannot find custom field '{fieldName}' on {type}{valueString} by Set Field trigger action.";
		}

		const string shipmentType = "Enterprise.Freight.Forwarding.Business.ForwardingShipment";
		const string decType = "Enterprise.Customs.AU.Declaration.Business.JobDeclaration";

		public void TestGetFinalPropertyInfoForCustomsField()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment.SetUserDefinedValue("shipment 1", ZBool.True);
			shipment.SetUserDefinedValue("shipment 2", new ZString("Lots Of Ice"));

			var shipmentWorkflow = (IWorkflowProvider)shipment;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(shipment 1)", "", "", shipmentType);
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(shipment 1)", "", "", shipmentType, new ZString("change field"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(shipment 1)", "", "", shipmentType, new ZBool("false"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(shipment XXX)", null, GetCustomFieldNotFoundError("shipment XXX", shipmentType));

			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration.SetUserDefinedValue("declaration 1", ZBool.True);
			declaration.SetUserDefinedValue("declaration 2", new ZString("Lots Of Ice"));
			var declarationBO = (Enterprise.Integration.Customs.IBaseJobDeclaration)declaration;
			declarationBO.JE_JS = shipment.PK;

			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(shipment 1)", "", "", shipmentType);
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(shipment 1)", "", "", shipmentType, new ZString("change field"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(shipment 1)", "", "", shipmentType, new ZBool("false"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(declaration 1)", "", "", decType);
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(declaration 1)", "", "", decType, new ZString("change field"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(declaration 1)", "", "", decType, new ZBool("false"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(shipment XXX)", null, GetCustomFieldNotFoundError("shipment XXX", shipmentType) + "\r\n" + GetCustomFieldNotFoundError("shipment XXX", decType));
		}

		public void TestGetFinalPropertyInfoForCustomsFieldWithType()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			shipment.SetUserDefinedValue("shipment 1", ZBool.True);
			shipment.SetUserDefinedValue("shipment 2", new ZString("Lots Of Ice"));

			var shipmentWorkflow = (IWorkflowProvider)shipment;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(shipment 2, STR)", "", "", shipmentType);
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(shipment XXX)", null, GetCustomFieldNotFoundError("shipment XXX", shipmentType));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(shipment 1, INT)", null, GetCustomFieldNotFoundError("shipment 1(INT)", shipmentType));

			var declaration = Factory.New(ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>());
			declaration.SetUserDefinedValue("declaration 1", ZBool.True);
			declaration.SetUserDefinedValue("declaration 2", new ZString("Lots Of Ice"));
			var declarationBO = (Enterprise.Integration.Customs.IBaseJobDeclaration)declaration;
			declarationBO.JE_JS = shipment.PK;

			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(shipment 2, STR)", "", "", shipmentType);
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(declaration 2, STR)", "", "", decType);
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(shipment XXX)", null, GetCustomFieldNotFoundError("shipment XXX", shipmentType) + "\r\n" + GetCustomFieldNotFoundError("shipment XXX", decType));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(shipment 1, INT)", null, GetCustomFieldNotFoundError("shipment 1(INT)", shipmentType) + "\r\n" + GetCustomFieldNotFoundError("shipment 1(INT)", decType));
		}

		public void TestGetFinalPropertyInfoForCustomsField_DuplicateFieldWithTheSameNameAndDifferentType()
		{
			SetUpDuplicateCustomFieldsForSHP();
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(Duplicate Field)", "", "", shipmentType, new ZString("change field"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(Duplicate Field)", "", "", shipmentType, new ZInt("19"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomField(Duplicate Field)", "", "", shipmentType, null);
		}

		public void TestGetFinalPropertyInfoForCustomsFieldWithType_DuplicateFieldWithTheSameNameAndDifferentType()
		{
			SetUpDuplicateCustomFieldsForSHP();
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(Duplicate Field, STR)", "", "", shipmentType, new ZString("change field"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(Duplicate Field, INT)", "", "", shipmentType, new ZInt("19"));
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(Duplicate Field, STR)", "", "", shipmentType, null);
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(Duplicate Field, INT)", "", "", shipmentType, null);
			AssertGetPropertyInfoAndParentType(shipmentWorkflow, "GetCustomFieldWithType(Duplicate Field, BOO)", null, GetCustomFieldNotFoundError("Duplicate Field(BOO)", shipmentType));
		}

		public void TestSetFieldTriggerCustomFieldWithEventAddOnRule()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "SHP";

			var customField1 = MasterFilesTestHelper.CreateCustomField(template, "CF1", AddOnColumnDataType.Codes.Datetime);
			var createEventRule1 = new CreateEventRule { EventCode = AutoEvents.CustomisableEvent01Code, EventReference = "123", IsEstimate = false, IsEnabled = true };
			var rule1 = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			rule1.SetRules(createEventRule1);
			customField1.XC_XR = rule1.PK;

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "TRIGGER1";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.ProcessTaskNotifications.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<GetCustomField(CF1)>";
			var dateValue = ZDateTime.Now;
			action.PQ_FieldValue = dateValue.ToString();

			Factory.Save();

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>());

			Factory.Save();

			var workflowProvider = (IWorkflowProvider)shipment;
			AssertEquals("Pre-condition: Template applied to shipment", 1, workflowProvider.WorkflowItems.Triggers.Count);

			var logs = shipment.GetLogs();
			logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();
			logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();
			logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			AssertEquals("Expecting one event created by the add on rule as the IFC action set the same value", 1, logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomisableEvent01Code).Count());
		}

		void SetUpDuplicateCustomFieldsForSHP()
		{
			MasterFilesTestHelper.ClearWorkflowTables();

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Shipment Task Template";
			template.P0_ProcessType = "SHP";

			MasterFilesTestHelper.CreateCustomField(template, "Duplicate Field", AddOnColumnDataType.Codes.String);
			MasterFilesTestHelper.CreateCustomField(template, "Duplicate Field", AddOnColumnDataType.Codes.Integer);

			Factory.Save();
		}

		void FireTriggerAction(BusinessObject triggerJob, IBaseTrigger trigger, ProcessTaskNotification action, INotifications notifications = null)
		{
			using (WorkflowTriggerActionTracker.TrackTriggerActions(trigger.Factory))
			{
				new WorkflowSetFieldProcessor(new WorkflowTriggerActionSource(triggerJob, trigger, action, new ExampleLog(action), null)).Process(notifications);
				WorkflowTriggerActionTracker.OnAllActionsRun(trigger.Factory);
			}
		}

		#endregion

		#region Test data type check

		public void TestIsReferenceType()
		{
			var dummy = Factory.New<DummyWithExtra>();

			AssertSetProperty(
				dummy,
				"DocType.RT_Code",
				"XYZ",
				expectedWarnings: @"Property Enterprise.Workflow.Business.Testing.WorkflowSetFieldProcessorTest+DummyWithExtra.DocType of type Enterprise.MasterFiles.Business.RefDocType cannot be used in field path of 'Set Field' trigger action field path - reference data cannot be changed here.");
		}

		public void TestFieldsAndMacrosThatDontExist()
		{
			var dummy = Factory.New<DummyWithExtra>();
			dummy.Z0_IsSystem = false;
			dummy.Z0_Description = "Value";

			AssertEquals("Previous value is unchanged", "Value", dummy.Z0_Description);
			AssertSetProperty(
dummy,
propertyPath: "Z0_Description",
valuePath: "<Z0_Descriptoin>",
expectedWarnings: @"Set field (FLD) macro evaluated with errors. Macro: [<Z0_Descriptoin>]");

			AssertEquals("Previous value is unchanged", "Value", dummy.Z0_Description);

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Description",
				valuePath: "< Macro(\"10\") >",
				expectedWarnings: @"Set field (FLD) macro evaluated with errors. Macro: [< Macro(""10"") >]");
		}

		public void TestLogWalkerErrorMessageWhenFindMacroIsNull()
		{
			var dummy = Factory.New<DummyWithExtra>();
			dummy.Z0_IsSystem = false;
			dummy.Z0_Description = "Value";
			var trigger = dummy.AddNewTrigger();
			trigger.P9_Description = "test 123";

			CombineAssertions(() =>
			{
				AssertSetProperty(
					dummy,
					propertyPath: "Z0_Description",
					valuePath: "<WorkflowItems.Find(\"{P9_Description}\"==\"test 1\").P9_TriggerField>",
					expectedWarnings: "Macro evaluated to null: WorkflowItems.Find(\"{P9_Description}\"==\"test 1\").P9_TriggerField");

				AssertEquals("Previous value is unchanged", "Value", dummy.Z0_Description);
			});
		}

		[TestDate(2020, 10, 10)]
		public void TestMacroReportedErrors()
		{
			var dummy = Factory.New<DummyWithExtraDuration>();
			dummy.Z0_IsSystem = false;
			dummy.Z0_Description = "3";
			ZDateTime expectedDate = TimeSpan.FromHours(5);

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Date",
				valuePath: "<DurationAsDateTime(\" < Add(\"<Z0_Description>\", \"2\") >:0\")>");

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Date",
				valuePath: " <DurationAsDateTime(\"abd:de\")>",
				expectedWarnings: @"Error in DurationAsDateTime Macro: Incorrect format, should be <DurationAsDateTime(""HHH:MM"")> Input macro: [<DurationAsDateTime(""abd:de"")>]
Set field (FLD) macro evaluated with errors. Macro: [ <DurationAsDateTime(""abd:de"")>] Source:");

			AssertEquals("Previous value is unchanged", expectedDate, dummy.Z0_Date);

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Date",
				valuePath: "<DurationAsDateTime(\" 2< Add(\"a\", \"1\") >:0\")>",
				expectedWarnings: @"Error in Add Macro: Invalid parameters provided: < Add(""a"", ""1"") >, expected format: <Add(""value1"",""value2"",...)>. Input macro: [<DurationAsDateTime("" 2< Add(""a"", ""1"") >:0"")>]
FLD setting [Z0_Date] with [<DurationAsDateTime("" 2< Add(""a"", ""1"") >:0"")>] succeeded on 1 properties. Unchanged on 0. Failed on 0
Success logs:
Target Object: 'Dummy Business Object 3', Source: 01-Jan-00 05:00:00, Target: 01-Jan-00 02:00:00");

			dummy.Z0_Date = expectedDate;

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Date",
				valuePath: "<DurationAsDateTime(\" <Coalesce(\"< Add(\"a\", \"1\") >\", \"2\") >:0\")>",
				expectedWarnings: @"Error in Add Macro: Invalid parameters provided: < Add(""a"", ""1"") >, expected format: <Add(""value1"",""value2"",...)>. Input macro: [<DurationAsDateTime("" <Coalesce(""< Add(""a"", ""1"") >"", ""2"") >:0"")>]
FLD setting [Z0_Date] with [<DurationAsDateTime("" <Coalesce(""< Add(""a"", ""1"") >"", ""2"") >:0"")>] succeeded on 1 properties. Unchanged on 0. Failed on 0
Success logs:
Target Object: 'Dummy Business Object 3', Source: 01-Jan-00 05:00:00, Target: 01-Jan-00 02:00:00");

			dummy.Z0_Date = expectedDate;

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Date",
				valuePath: "<DurationAsDateTime(\" < Add(\"<Z0_WrongField>\", \"1\") >:0\")>",
				expectedWarnings: @"Field <Z0_WrongField> not found on any of the DataSource Types:");

			AssertEquals("Previous value is unchanged", expectedDate, dummy.Z0_Date);

			dummy.Z0_Description = "Not a number";

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Date",
				valuePath: "<DurationAsDateTime(\" 2< Add(\"<Z0_Description>\", \"1\") >:0\")>",
				expectedWarnings: @"Error in Add Macro: Invalid parameters provided: < Add(""Not a number"", ""1"") >, expected format: <Add(""value1"",""value2"",...)>. Input macro: [<DurationAsDateTime("" 2< Add(""<Z0_Description>"", ""1"") >:0"")>]
FLD setting [Z0_Date] with [<DurationAsDateTime("" 2< Add(""<Z0_Description>"", ""1"") >:0"")>] succeeded on 1 properties. Unchanged on 0. Failed on 0
Success logs:
Target Object: 'Dummy Business Object Not a number', Source: 01-Jan-00 05:00:00, Target: 01-Jan-00 02:00:00");
		}

		[TestDate(2020, 10, 10)]
		public void TestMacroWithPartialErrors()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Code = "XYZ";
			dummy.Z0_Number = 123;
			dummy.Z0_IsSystem = false;

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Description",
				valuePath: "<Z0_Code> - <Z0_InvalidField>",
				expectedWarnings: @"Field <Z0_InvalidField> not found on any of the DataSource Types: [DummyWithWorkflow], [DummyProcessTask], [ProcessTaskNotification], [StmALog], [TriggeringEventPropertyProvider`1].
FLD setting [Z0_Description] with [<Z0_Code> - <Z0_InvalidField>] succeeded on 1 properties. Unchanged on 0. Failed on 0
Success logs:
Target Object: 'Dummy Business Object XYZ -', Source: Default, Target: XYZ -");
		}

		[TestDate(2020, 10, 10)]
		public void TestIfMacroWithErrors()
		{
			DummyWithWorkflow dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Description = "Desc";
			dummy.Z0_Code = "XYZ";
			dummy.Z0_Number = 123;
			dummy.Z0_IsSystem = false;

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Description",
				valuePath: "<if(\"<ServiceLevel.InvalidField> \"==\"STD\",\"true\",\"false\")>",
				expectedWarnings: @"Field <ServiceLevel.InvalidField> not found on any of the DataSource Types: [DummyWithWorkflow], [DummyProcessTask], [ProcessTaskNotification], [StmALog], [TriggeringEventPropertyProvider`1].
Set field (FLD) macro evaluated with errors. Macro: [<if(""<ServiceLevel.InvalidField> ""==""STD"",""true"",""false"")>] Source:");

			AssertEquals("Previous value is unchanged", "Desc", dummy.Z0_Description);
		}

		public void TestInvalidFindMacroReportedErrors()
		{
			var dummy = Factory.New<DummyWithExtra>();
			dummy.Z0_IsSystem = false;

			AssertSetProperty(
				dummy,
				propertyPath: "Z0_Description",
				valuePath: "<TriggeringEvent.Parameters.Find(\"{ Key}\"==\"LOC\").Value>",
				expectedWarnings: @"Cannot evaluate <TriggeringEvent.Parameters.Find(""{ Key}""==""LOC"").Value>. The Find function only works on collections. Please check your Find macro and make sure it is used on a collection.");
		}

		public void TestIsSystemField()
		{
			var dummy = Factory.New<DummyWithExtra>();

			AssertSetProperty(
				dummy,
				"Group.GG_IsActive",
				"Y",
				expectedWarnings: @"Property Enterprise.MasterFiles.Business.GlbGroup.GG_IsActive of type ZBool holds system data and cannot be used in 'Set Field' trigger action.");
		}

		public void TestIsSystemRecord()
		{
			var dummy = Factory.New<DummyWithExtra>();
			dummy.Group.GG_IsSystemDefined = false;

			AssertSetProperty(dummy, "Group.GG_Code", "XYZ");

			dummy.Group.GG_IsSystemDefined = true;

			AssertSetProperty(
				dummy,
				"Group.GG_Code",
				"ABC",
				expectedWarnings: @"FLD setting [Group.GG_Code] with [ABC] succeeded on 0 properties. Unchanged on 0. Failed on 1
Failure with 1 reasons:
Cannot make changes on system record Enterprise.MasterFiles.Business.GlbGroup by Set Field (FLD) trigger action. Target Object: 'Group (XYZ)'. Source: ABC,");
		}

		class DummyWithExtra : DummyWithWorkflow
		{
			public DummyWithExtra(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public RefDocType DocType
			{
				get => docTypeCache.GetOrAdd(PK.ToGuid(), Factory.New<RefDocType>);
			}
			static readonly Dictionary<Guid, RefDocType> docTypeCache = new Dictionary<Guid, RefDocType>();

			public BusinessObject Consol
			{
				get => consolCache.GetOrAdd(PK.ToGuid(), () => (BusinessObject)Factory.New<ICommonConsol>());
			}
			static readonly Dictionary<Guid, BusinessObject> consolCache = new Dictionary<Guid, BusinessObject>();

			public GlbGroup Group
			{
				get => groupCache.GetOrAdd(PK.ToGuid(), Factory.New<GlbGroup>);
			}
			static readonly Dictionary<Guid, GlbGroup> groupCache = new Dictionary<Guid, GlbGroup>();

			public ZString CodeWrapper
			{
				get => Z0_Code;
				set => Z0_Code = value;
			}
		}

		class DummyWithExtraDuration : DummyWithExtra
		{
			public DummyWithExtraDuration(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override ZDateTime Z0_Date { get => base.Z0_Date; set => base.Z0_Date = value; }

			protected override DummyBizoValidation GetNewValidation()
			{
				return new DummyWithExtraDurationValidation(this);
			}
		}

		class DummyWithExtraDurationValidation : DummyBizoValidation
		{
			public DummyWithExtraDurationValidation(DummyBusinessObject bizO)
				: base(bizO)
			{
			}

			protected override void CheckZ0_DateIsValidZDateTimeRange()
			{
				//Duration value does not check valid range
			}
		}

		#endregion

		#region Setting Change Type Mode When Changing Process Task Status

		public void TestShouldSetChangeModeToTriggerOrOtherAutomation_WhenSettingStatusOnProcessTasks()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "TESTS00001";

			var task = ((IWorkflowProvider)shipment).WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Target Task";
			task.P9_Status = "ASN";

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			trigger.P9_Description = "Test Set Field";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = @"<WorkflowItems.Where(""<P9_Description>""==""Target Task"").P9_Status>";
			action.PQ_FieldValue = "CAN";

			Factory.Save();

			var taskLastLogReference = task.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.Other}", taskLastLogReference);

			FireTriggerAction((BusinessObject)shipment, trigger, action, new NotificationsForTest());

			Factory.Save();
			task.Reload();

			AssertEquals("CAN", task.P9_Status);

			taskLastLogReference = task.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.TriggerOrOtherAutomation}", taskLastLogReference);
		}

		public void TestShouldNotSetChangeMode_WhenSettingOtherFieldsOnProcessTasks()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "TESTS00001";

			var task = ((IWorkflowProvider)shipment).WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Target Task";
			task.P9_EstimateVariationFactor = 1;

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			trigger.P9_Description = "Test Set Field";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = @"<WorkflowItems.Where(""<P9_Description>""==""Target Task"").P9_EstimateVariationFactor>";
			action.PQ_FieldValue = "3";

			Factory.Save();

			var taskLastLogReference = task.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			AssertContains($"CHM={ProcessTaskStatusChangeModeCodeList.Codes.Other}", taskLastLogReference);

			FireTriggerAction((BusinessObject)shipment, trigger, action, new NotificationsForTest());

			Factory.Save();
			task.Reload();

			AssertEquals(3m, task.P9_EstimateVariationFactor);

			taskLastLogReference = task.Logs.MostRecentLogByEventTime(AutoEvents.StatusChange).SL_Reference;
			AssertContains("Should not change change mode", $"CHM={ProcessTaskStatusChangeModeCodeList.Codes.Other}", taskLastLogReference);
		}

		#endregion

		#region Setting Task Capability Code

		public void TestShouldBeAbleToChangeCapabilityByUsingCode()
		{
			// Arrange
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "TESTS00001";

			var oldCapability = Factory.NewWithValidTestData<GlbCapability>();
			oldCapability.G4_Code = "TST";

			var newCapability = Factory.NewWithValidTestData<GlbCapability>();
			newCapability.G4_Code = "TSI";

			var task = ((IWorkflowProvider)shipment).WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Target Task";
			task.P9_Status = "ASN";
			task.P9_G4_RequiredCapability = oldCapability.PK;

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.WorkflowTemplateApplied.Code;
			trigger.P9_Description = "Test Set Field";

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = @"<WorkflowItems.Where(""<CapabilityCode>""==""TST"").CapabilityCode>";
			action.PQ_FieldValue = "TSI";

			Factory.Save();

			// Act
			FireTriggerAction((BusinessObject)shipment, trigger, action, new NotificationsForTest());

			Factory.Save();
			task.Reload();

			// Assert
			AssertEquals("Tasks Capability PK reference must be changed by the trigger", newCapability.PK, task.P9_G4_RequiredCapability);
			AssertEquals("Tasks Capabilit Code must be updated", newCapability.G4_Code, task.CapabilityCode);
		}

		#endregion

		#region Implementation

		string AssertSetProperty(
			DummyWithWorkflow dummy,
			string propertyPath,
			string valuePath,
			IZType expectedValue = null,
			string expectedWarnings = "",
			IEnumerable<ZPropertyInfo> expectedProperties = null,
			IEnumerable<ZPropertyInfo> notExpectedProperties = null,
			bool clearValue = true,
			string assertMessage = default(string))
		{
			var task = (DummyProcessTask)dummy.WorkflowItems.Triggers.AddNew();
			if (dummy.GetType() != typeof(DummyWithWorkflow))
			{
				task.OverriddenParentTypeForTest = dummy.GetType();
			}

			var action = task.ProcessTaskNotifications.AddNew();

			return AssertSetProperty(
				propertyPath,
				valuePath, new[] { expectedValue },
				expectedWarnings,
				action,
				expectedProperties,
				notExpectedProperties,
				clearValue,
				assertMessage);
		}

		string AssertSetPropertyWithMultipleExpectedValues(
			DummyWithWorkflow dummy,
			string propertyPath,
			string valuePath,
			IEnumerable<IZType> expectedValues = null,
			string expectedWarnings = "",
			IEnumerable<ZPropertyInfo> expectedProperties = null,
			IEnumerable<ZPropertyInfo> notExpectedProperties = null,
			bool clearValue = true,
			string assertMessage = default(string))
		{
			var task = (DummyProcessTask)dummy.WorkflowItems.Triggers.AddNew();
			if (dummy.GetType() != typeof(DummyWithWorkflow))
			{
				task.OverriddenParentTypeForTest = dummy.GetType();
			}

			var action = task.ProcessTaskNotifications.AddNew();

			return AssertSetProperty(
				propertyPath,
				valuePath,
				expectedValues,
				expectedWarnings,
				action,
				expectedProperties,
				notExpectedProperties,
				clearValue,
				assertMessage);
		}

		string AssertSetProperty(
			BusinessObject dummy,
			string propertyPath,
			string valuePath,
			IZType expectedValue = null,
			string expectedWarnings = null,
			IEnumerable<ZPropertyInfo> expectedProperties = null,
			IEnumerable<ZPropertyInfo> notExpectedProperties = null,
			bool clearValue = true,
			string assertMessage = default(string))
		{
			var processTask = ((IWorkflowProvider)dummy).WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_P9 = processTask.PK;
			return AssertSetProperty(
				propertyPath,
				valuePath,
				new[] { expectedValue },
				expectedWarnings,
				action,
				expectedProperties,
				notExpectedProperties,
				clearValue,
				assertMessage);
		}

		string AssertSetProperty(
			string propertyPath,
			string valuePath,
			IEnumerable<IZType> expectedValues,
			string expectedWarnings,
			ProcessTaskNotification action,
			IEnumerable<ZPropertyInfo> expectedProperties = null,
			IEnumerable<ZPropertyInfo> notExpectedProperties = null,
			bool clearValue = true,
			string assertMessage = default(string),
			int expectedLogCount = 0)
		{
			if (clearValue)
			{
				foreach (ZPropertyInfo propertyInfo in expectedProperties ?? Enumerable.Empty<ZPropertyInfo>())
				{
					propertyInfo.ClearValue();
				}

				foreach (ZPropertyInfo propertyInfo in notExpectedProperties ?? Enumerable.Empty<ZPropertyInfo>())
				{
					propertyInfo.ClearValue();
				}
			}

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = propertyPath;
			action.PQ_FieldValue = valuePath;

			action.Parent.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			if (action.Parent.GetJob().GetLogs().DatabaseCount == expectedLogCount)
			{
				action.Parent.GetJob().GetLogs().AddNew(AutoEvents.CustomisableEvent00);
			}
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = action.Parent.GetJob().GetType();
			Factory.Save();

			var logWalkerOutput = MasterFilesTestHelper.RunLogWalker();
			var fldNotifications = MasterFilesTestHelper.GetFldNotificationsFromLogWalker(logWalkerOutput);

			if (!string.IsNullOrEmpty(expectedWarnings))
			{
				AssertContains(expectedWarnings, fldNotifications);
			}
			else if (expectedProperties == null && notExpectedProperties == null)
			{
				Assert($"Setting of [{propertyPath}] with [{valuePath}] should succeed if there are no warnings or assertions about expected properties. Notifications:\r\n{fldNotifications}",
					Regex.IsMatch(fldNotifications, $@"FLD setting \[{Regex.Escape(propertyPath)}\] with \[{Regex.Escape(valuePath)}\] succeeded on [1-9]\d* properties."));
			}

			action.Parent.GetJob().Reload();

			foreach (var expectedProperty in expectedProperties ?? Enumerable.Empty<ZPropertyInfo>())
			{
				Assert(
					$"{assertMessage}: Property [{propertyPath}] is expected to have value [{string.Join(", ", expectedValues)}] instead of [{expectedProperty.Value}]",
					expectedValues.Contains(expectedProperty.Value));
			}

			foreach (var notExpectedProperty in notExpectedProperties ?? Enumerable.Empty<ZPropertyInfo>())
			{
				Assert(
					$"{assertMessage}: Property [{propertyPath}] is NOT expected have value [{string.Join(", ", expectedValues)}] instead of [{notExpectedProperty.Value}] ",
					!expectedValues.Contains(notExpectedProperty.Value));
			}

			return logWalkerOutput;
		}

		void AssertGetPropertyInfoAndParentType(IWorkflowProvider dummy, string propertyPath, string expectedPropertyInfoName, string expectedWarnings, string expectedPropertyType = null, IZType value = null)
		{
			var action = dummy.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = propertyPath;

			var notifications = new NotificationsForTest();

			var (propertyInfo, parentType, _) = WorkflowSetFieldProcessor.GetFinalPropertyInfoAndParentType(action, notifications, fieldValue: value);

			if (!string.IsNullOrEmpty(expectedPropertyInfoName))
			{
				AssertEquals(expectedPropertyInfoName, propertyInfo.Name);
			}
			else
			{
				AssertNull(propertyInfo);
			}

			if (expectedPropertyType != null)
			{
				AssertEquals(expectedPropertyType, parentType.FullName);
			}
			else
			{
				AssertNull(parentType);
			}

			var logInfo = action.GetDiagnosticLogInfo();

			AssertEquals(string.Format(expectedWarnings, logInfo), notifications.ToString().Trim());
		}

		public void SetupAndAssert_SetPropertyWithValue_First_FirstOrDefault(string clause)
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();
			var child0 = dummy.Collection.AddNew();
			var child1 = dummy.Collection.AddNew();
			var child2 = dummy.Collection.AddNew();
			var child3 = dummy.Collection.AddNew();
			var child4 = dummy.Collection.AddNew();
			var child5 = dummy.Collection.AddNew();

			child0.Z0_Description = "child-property-0";
			child1.Z0_Description = "child-property-1";
			child2.Z0_Description = "child-property-2";
			child3.Z0_Description = "child-value-3";
			child4.Z0_Description = "child-value-4";
			child5.Z0_Description = "child-value-5";

			child0.Z0_IsSystem = false;
			child1.Z0_IsSystem = false;
			child2.Z0_IsSystem = false;
			child3.Z0_IsSystem = false;
			child4.Z0_IsSystem = false;
			child5.Z0_IsSystem = false;

			child0.Z0_Code = "code0";
			child1.Z0_Code = "code1";
			child2.Z0_Code = "code0";
			child3.Z0_Code = "codeA";
			child4.Z0_Code = "codeB";
			child5.Z0_Code = "codeA";

			child0.Z0_VarCharMax = "123";
			child1.Z0_VarCharMax = "456";
			child2.Z0_VarCharMax = "x(xx)";

			child3.Z0_NVarCharMax = "789";
			child4.Z0_NVarCharMax = "777";
			child5.Z0_NVarCharMax = "999";

			AssertSetProperty(
				dummy,
				propertyPath: "<Collection.Where(\"<Z0_Code>\" == \"code0\").Z0_Number>",
				valuePath: string.Format("<Collection.{0}(\"<Z0_Code>\" == \"codeA\").Z0_NVarCharMax>", clause),
				expectedValue: new ZInt(789),
				expectedProperties: dummy.Collection.Where(c => c.Z0_Code == "code0").Select(c => c.Z0_NumberInfo),
				notExpectedProperties: dummy.Collection.Where(c => c.Z0_Code != "code0").Select(c => c.Z0_NumberInfo).Concat(new[] { dummy.Z0_NumberInfo }),
				assertMessage: "\r\nGIVEN property-filter has results and value-filter has results, should be set to value-first-value");

			AssertSetProperty(
				dummy,
				propertyPath: "<Collection.Where(\"<Z0_Code>\" == \"code1\").Z0_Number>",
				valuePath: string.Format("<Collection.{0}(\"<Z0_Code>\" == \"codeB\").Z0_NVarCharMax>", clause),
				expectedValue: new ZInt(777),
				expectedProperties: dummy.Collection.Where(c => c.Z0_Code == "code1").Select(c => c.Z0_NumberInfo),
				notExpectedProperties: dummy.Collection.Where(c => c.Z0_Code != "code1").Select(c => c.Z0_NumberInfo).Concat(new[] { dummy.Z0_NumberInfo }),
				assertMessage: "\r\nGIVEN property-filter has results and value-filter has results, should be set to value-first-value");

			AssertSetProperty(
				dummy,
				propertyPath: "<Collection.Where(\"<Z0_Code>\" == \"nonExistedCode\").Z0_Number>",
				valuePath: string.Format("<Collection.{0}(\"<Z0_Code>\" == \"codeB\").Z0_NVarCharMax>", clause),
				expectedValue: new ZInt(777),
				notExpectedProperties: dummy.Collection.Select(c => c.Z0_NumberInfo).Concat(new[] { dummy.Z0_NumberInfo }),
				expectedWarnings: $@"Set field (FLD) target not found. Macro [<Collection.{clause}(""<Z0_Code>"" == ""codeB"").Z0_NVarCharMax>] Target [Enterprise.MasterFiles.Business.Testing.DummyWithWorkflowAndCollectionWithAdditionalFilter]",
				assertMessage: "\r\nGIVEN property-filter has NO results and value-filter has results, should not set");

			AssertSetProperty(
				dummy,
				propertyPath: "<Collection.Where(\"<Z0_Code>\" == \"code1\").Z0_Number>",
				valuePath: string.Format("<Collection.{0}().Z0_VarCharMax>", clause),
				expectedValue: new ZInt(123),
				expectedProperties: dummy.Collection.Where(c => c.Z0_Code == "code1").Select(c => c.Z0_NumberInfo),
				notExpectedProperties: dummy.Collection.Where(c => c.Z0_Code != "code1" && c.Z0_Description != "child-property-0").Select(c => c.Z0_NumberInfo).Concat(new[] { dummy.Z0_NumberInfo }), // exclude the setting-property i.e. child-property-0 
				assertMessage: "\r\nGIVEN property-filter has results and value-filter has empty argument, should set to first-value");

			dummy.Z0_Number = -1;
			dummy.Collection.ToList().ForEach(c => ((DummyChildBusinessObject)c).Z0_Number = -1);

			if (clause == "First")
			{
				AssertSetProperty(
					dummy,
					propertyPath: "<Collection.Where(\"<Z0_Code>\" == \"code0\").Z0_Number>",
					valuePath: string.Format("<Collection.{0}(\"<Z0_Code>\" == \"nonExistedCode\").Z0_NVarCharMax>", clause),
					expectedValue: new ZInt(-1),
					expectedProperties: dummy.Collection.Select(c => c.Z0_NumberInfo),
					clearValue: false,
					expectedWarnings: @"Set field (FLD) macro evaluated to null. Macro: [<Collection.First(""<Z0_Code>"" == ""nonExistedCode"").Z0_NVarCharMax>] Source:",
					assertMessage: "\r\nGIVEN property-filter has results and value-filter has no results, should not set");
			}
			else if (clause == "FirstOrDefault")
			{
				AssertSetProperty(
					dummy,
					propertyPath: "<Collection.Where(\"<Z0_Code>\" == \"code0\").Z0_Number>",
					valuePath: "<Collection.FirstOrDefault(\"<Z0_Code>\" == \"nonExistedCode\").Z0_NVarCharMax>",
					expectedValue: default(ZInt),
					expectedProperties: dummy.Collection.Where(c => c.Z0_Code == "code0").Select(c => c.Z0_NumberInfo),
					notExpectedProperties: dummy.Collection.Where(c => c.Z0_Code != "code0").Select(c => c.Z0_NumberInfo).Concat(new[] { dummy.Z0_NumberInfo }),
					clearValue: false,
					assertMessage: "\r\nGIVEN property-filter has results and value-filter has empty argument, should set to default-value");
			}
			else
			{
				Fail("This test should have clause First or FirstOrDefault");
			}
		}

		void SetupAndAssert_SetPropertyWithValue_First_FirstOrDefault_Chained(string clause)
		{
			var dummy = Factory.New<DummyWithWorkflowAndCollectionWithAdditionalFilter>();

			var org1 = dummy.OrgHeaderCollection.AddNew();
			var org2 = dummy.OrgHeaderCollection.AddNew();
			var org3 = dummy.OrgHeaderCollection.AddNew();

			org1.OH_Code = "org1";
			org2.OH_Code = "org2";
			org3.OH_Code = "org3";

			var address11 = org1.AddressesNoAutoCreate.AddNew();
			var address12 = org1.AddressesNoAutoCreate.AddNew();
			var address13 = org1.AddressesNoAutoCreate.AddNew();

			var address21 = org2.AddressesNoAutoCreate.AddNew();

			var address31 = org3.AddressesNoAutoCreate.AddNew();
			var address32 = org3.AddressesNoAutoCreate.AddNew();

			address11.OA_Address1 = "adr1";
			address12.OA_Address1 = "adr2";
			address13.OA_Address1 = "adr1";

			address21.OA_Address1 = "adr3";

			address31.OA_Address1 = "adr1";
			address32.OA_Address1 = "adr2";

			address12.OA_Language = "BND";

			address31.OA_Language = "RND";
			address32.OA_Language = "CGE";

			address11.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);
			address12.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);
			address13.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);

			address21.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);

			address31.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);
			address32.OA_Code = Guid.NewGuid().ToString("N").Substring(0, 8);

			var expectedProperties = new[] { dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[1].OA_CityInfo };

			var notExpectedProperties = new[] {
				dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[0].OA_CityInfo,
				dummy.OrgHeaderCollection[0].AddressesNoAutoCreate[2].OA_CityInfo,
				dummy.OrgHeaderCollection[1].AddressesNoAutoCreate[0].OA_CityInfo,
				dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[0].OA_CityInfo,
				dummy.OrgHeaderCollection[2].AddressesNoAutoCreate[1].OA_CityInfo };

			var allProperties = expectedProperties.Concat(notExpectedProperties);

			var propertyPath = "<OrgHeaderCollection.Where(\"<OH_Code>\" == \"org1\").AddressesNoAutoCreate.Where(\"<OA_Address1>\" == \"adr2\").OA_City>";

			AssertSetProperty(
				dummy,
				propertyPath: propertyPath,
				valuePath: string.Format("<OrgHeaderCollection.{0}(\"<OH_Code>\" == \"org3\").AddressesNoAutoCreate.{0}(\"<OA_Address1>\" == \"adr1\").OA_Language>", clause),
				expectedValue: new ZString("RND"),
				expectedProperties: expectedProperties,
				notExpectedProperties: notExpectedProperties,
				assertMessage: string.Format("{0}: WHEN chained and results found, should set", clause));

			AssertSetPropertyWithMultipleExpectedValues(
				dummy,
				propertyPath: propertyPath,
				valuePath: string.Format("<OrgHeaderCollection.AddressesNoAutoCreate.{0}(\"<OA_Address1>\" == \"adr2\").OA_Language>", clause),
				expectedValues: new IZType[] { new ZString("BND"), new ZString("CGE") },
				notExpectedProperties: notExpectedProperties,
				assertMessage: string.Format("{0}: WHEN 1st chain has no First/FirstOfDefault but 2nd chain return results, should set to 1st value in 2nd chain by iterating all 1st chain. There can be multiple results because the order wasn't specified", clause));

			if (clause == "FirstOrDefault")
			{
				AssertSetProperty(
					dummy,
					propertyPath: propertyPath,
					valuePath: string.Format("<OrgHeaderCollection.AddressesNoAutoCreate.{0}(\"<OA_Address1>\" == \"adr2\").OA_Language>", clause),
					expectedValue: new ZString("???"),
					notExpectedProperties: allProperties,
					assertMessage: string.Format("{0}: WHEN 1st chain has no First/FirstOfDefault and 2nd chain not return results, should not set", clause));
			}
			else
			{
				AssertSetProperty(
					dummy,
					propertyPath: propertyPath,
					valuePath: string.Format("<OrgHeaderCollection.AddressesNoAutoCreate.{0}(\"<OA_Address1>\" == \"adr2\").OA_Language>", clause),
					expectedValue: new ZString("???"),
					notExpectedProperties: allProperties,
					assertMessage: string.Format("{0}: WHEN 1st chain has no First/FirstOfDefault and 2nd chain not return results, should not set", clause));
			}

			address32.OA_Language = "MST";
			AssertSetProperty(
				dummy,
				propertyPath: propertyPath,
				valuePath: string.Format("<OrgHeaderCollection.{0}(\"<OH_Code>\" == \"org3\").AddressesNoAutoCreate.First(\"<OA_Address1>\" == \"adr2\").OA_Language>", clause),
				expectedValue: new ZString("???"),
				notExpectedProperties: allProperties,
				assertMessage: string.Format("{0}: WHEN 1st chain not return results, should not set", clause));

			address31.OA_Language = "ALX";
			AssertSetProperty(
				dummy,
				propertyPath: propertyPath,
				valuePath: string.Format("<OrgHeaderCollection.{0}(\"<OH_Code>\" == \"org3\").AddressesNoAutoCreate.OA_Language>", clause),
				expectedValue: new ZString("???"),
				notExpectedProperties: allProperties,
				assertMessage: string.Format("{0}: WHEN no First/FirstOrDefault on 2nd chain, should not set because element ambiguity to take on 2nd level. ", clause));

			if (clause == "FirstOrDefault")
			{
				address11.OA_Language = "SYD";

				address11.OA_City = "???";
				address12.OA_City = "???";
				address13.OA_City = "???";
				address21.OA_City = "???";
				address31.OA_City = "???";
				address32.OA_City = "???";
				AssertSetProperty(
					dummy,
					propertyPath: propertyPath,
					valuePath: "<OrgHeaderCollection.FirstOrDefault(\"<OH_Code>\" == \"NotExistingValue\").AddressesNoAutoCreate.FirstOrDefault(\"<OA_Address1>\" == \"NotExistingValue\").OA_Language>",
					expectedValue: default(ZString),
					expectedProperties: expectedProperties,
					notExpectedProperties: notExpectedProperties,
					clearValue: false,
					assertMessage: "WHEN chained with FirstOrDefault(not found) then FirstOrDefault(not found), should set value with default");

				address11.OA_Language = "SYD";
				AssertSetProperty(
					dummy,
					propertyPath: propertyPath,
					valuePath: "<OrgHeaderCollection.FirstOrDefault(\"<OH_Code>\" == \"NotExistingValue\").AddressesNoAutoCreate.First(\"<OA_Address1>\" == \"adr2\").OA_Language>",
					expectedValue: new ZString("???"),
					notExpectedProperties: allProperties,
					assertMessage: "WHEN chained with FirstOrDefault(not found) then First(found), should not set");

				address11.OA_Language = "SYD";
				AssertSetProperty(
					dummy,
					propertyPath: propertyPath,
					valuePath: "<OrgHeaderCollection.First(\"<OH_Code>\" == \"NotExistingValue\").AddressesNoAutoCreate.FirstOrDefault(\"<OA_Address1>\" == \"adr2\").OA_Language>",
					expectedValue: new ZString("???"),
					notExpectedProperties: allProperties,
					expectedWarnings: @"Set field (FLD) macro evaluated to null. Macro: [<OrgHeaderCollection.First(""<OH_Code>"" == ""NotExistingValue"").AddressesNoAutoCreate.FirstOrDefault(""<OA_Address1>"" == ""adr2"").OA_Language>] Source:",
					assertMessage: "WHEN chained with First(not found) then FirstOrDefault(found), should not set");

				address11.OA_Language = "SYD";
				AssertSetProperty(
					dummy,
					propertyPath: propertyPath,
					valuePath: "<OrgHeaderCollection.FirstOrDefault(\"<OH_Code>\" == \"org3\").AddressesNoAutoCreate.First(\"<OA_Address1>\" == \"NotExistingValue\").OA_Language>",
					expectedValue: new ZString("???"),
					notExpectedProperties: allProperties,
					expectedWarnings: @"Set field (FLD) macro evaluated to null. Macro: [<OrgHeaderCollection.FirstOrDefault(""<OH_Code>"" == ""org3"").AddressesNoAutoCreate.First(""<OA_Address1>"" == ""NotExistingValue"").OA_Language>] Source:",
					assertMessage: "WHEN chained with FirstOrDefault(found) then First(not found), should not set");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			AssertNotNull("Instantiate DummyWorkflowDescriptor", DummyWorkflowDescriptor.Instance);
		}

		#endregion

		class WorkflowSetFieldProcessorForTest : WorkflowSetFieldProcessor
		{
			public WorkflowSetFieldProcessorForTest(WorkflowTriggerActionSource source) : base(source) { }

			public string SetPropertyExposedForTest(IBusiness bizo, string propertyPath, IZType value)
			{
				var notifications = new NotificationsForTest();
				TrySetProperty(bizo, null, propertyPath, value, notifications);
				return notifications.ToString();
			}
		}
	}
}
