using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZWorkflowUserControlTest : TestCaseWithFactory
	{
		#region ValidationToolTab

		[RequiresSTA]
		public void TestValidationToolTab_Visible_EventTrackingSupported()
		{
			var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
			{
				ValidationRulesSupported = true
			};
			DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
			Form.Show();
			var validationToolTab = TrackingUserControl.MainTabControl.GetTabPageByNameOrText("ValidationToolTab");
			var triggersTab = TrackingUserControl.MainTabControl.GetTabPageByNameOrText("TriggersTab");
			var eventsTab = TrackingUserControl.MainTabControl.GetTabPageByNameOrText("EventsTab");
			var validationTabIndex = TrackingUserControl.MainTabControl.TabPages.IndexOf(validationToolTab);
			var triggersToolTabIndex = TrackingUserControl.MainTabControl.TabPages.IndexOf(triggersTab);
			var eventsTabIndex = TrackingUserControl.MainTabControl.TabPages.IndexOf(eventsTab);
			AssertEquals(true, validationTabIndex > triggersToolTabIndex && validationTabIndex < eventsTabIndex);
		}

		[RequiresSTA]
		public void TestValidationToolTab_Visible_EventTrackingNotSupported()
		{
			var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
			{
				ValidationRulesSupported = true
			};
			DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
			DummyWorkflowDescriptor.Instance.EventTrackingSupported = false;
			Form.Show();
			var tab = TrackingUserControl.MainTabControl.GetTabPageByNameOrText("ValidationToolTab");
			AssertNotNull(tab);
		}

		[RequiresSTA]
		public void TestValidationToolTab_Invisible()
		{
			var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
			{
				ValidationRulesSupported = false
			};
			DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
			Form.Show();
			var tab = TrackingUserControl.MainTabControl.GetTabPageByNameOrText("ValidationToolTab");
			AssertNull(tab);
		}

		#endregion

		#region Mandatory Tasks

		public void TestDeleteTasksMandatory()
		{
			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowNewForm())
			{
				Application.DoEvents();

				var job = (SalesEnquiry)form.BusinessEntity;
				job.FillWithValidTestData();
				var task1 = job.WorkflowItems.Tasks.AddNew();
				task1.P9_Sequence = 1;
				task1.P9_TaskCannotBeDeleted = true;
				var task2 = job.WorkflowItems.Tasks.AddNew();
				task2.P9_Sequence = 2;

				var workflowUserControl = NavigateToWorkflowTab(form);

				var taskGrid = workflowUserControl.FindAll<TaskWithDetailsAndFilterTab>().First().FilterControl.TasksGrid;
				taskGrid.Select(0);
				Application.DoEvents();
				taskGrid.OnDeleteKeyPressed();
				Application.DoEvents();

				taskGrid.Select(1);
				Application.DoEvents();
				taskGrid.OnDeleteKeyPressed();
				Application.DoEvents();

				AssertEquals("1 Task - mandatory so doesn't get deleted", 1, job.WorkflowItems.Count);
				AssertEquals("Mandatory tasks cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteTasksWithIteration()
		{
			var bmsTestHelper = ObjectFactory.Get<IBMTestHelper>();
			bmsTestHelper.EnableBMSInRegistry();

			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var taskTypes = categorisedTaskTypes.GetTaskTypesFromWorkflowCode(SalesEnquiry.Codes.SalesEnquiry);
			var taskType = taskTypes.AddNew();
			taskType.Code = "MOO";
			taskType.ContainmentBarrierIterationType = ContainmentBarrierIterationTypeList.Codes.GLB;
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowNewForm())
			{
				Application.DoEvents();

				var jobHeader = bmsTestHelper.CreateJobHeader<OrgHeader>(Factory);
				var workflow = bmsTestHelper.CreateWorkflow(jobHeader, "workflow");

				var job = (SalesEnquiry)form.BusinessEntity;
				job.FillWithValidTestData();
				var task1 = job.WorkflowItems.Tasks.AddNew();
				task1.P9_FH_ProcessHeader = workflow.PK;
				task1.P9_Sequence = 1;
				var task2 = job.WorkflowItems.Tasks.AddNew();
				task2.P9_Sequence = 2;
				task2.P9_Type = "MOO";
				task2.P9_FH_ProcessHeader = workflow.PK;

				Factory.Save();

				var iterationWorkflow = bmsTestHelper.CreateQualityIteration(task1, task2, "Quality Iteration");

				var workflowUserControl = NavigateToWorkflowTab(form);

				var taskGrid = workflowUserControl.FindAll<TaskWithDetailsAndFilterTab>().First().FilterControl.TasksGrid;
				taskGrid.Select(0);
				Application.DoEvents();
				taskGrid.OnDeleteKeyPressed();
				Application.DoEvents();

				AssertEquals("The Task cannot be deleted, because there is at least one Quality Iteration Link referencing it.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Balloon Tips

#if !WINZOR

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestMouseMoveIsSafe()
		{
			TrackingUserControl.OnMouseMove_Exposed();
		}

#endif

		public void TestBalloonTips()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			TrackingUserControl.LastBalloonCaption = "";
			AssertBalloonTips(true);
			TrackingUserControl.LastBalloonCaption = "";
			AssertBalloonTips(false);
		}

		void AssertBalloonTips(bool shouldShow)
		{
			Enterprise.ZArchitecture.Environment.EnvProxy.Instance.Registry.TraningModeEnabled = shouldShow;
			TestWorkflowDescriptor workflowDescriptor = new TestWorkflowDescriptor();
			workflowDescriptor.SupportsEventTrackingExposed = true;
			workflowDescriptor.SupportsValidationRulesExposed = true;
			TestTrackingUserControl.WorkflowDescriptor = workflowDescriptor;

			Form.Show();
			Application.DoEvents();

			TrackingUserControl.SetFormMousePosition(TrackingUserControl.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(10, 10)));
			AssertMainTabControlBalloonText(shouldShow ? "Tasks" : "");
			MaybeAssertEquals(shouldShow, TrackingUserControl.MainTabControl.GetTabRect(0), TrackingUserControl.LastBalloonRectangle);

			TrackingUserControl.SetFormMousePosition(TrackingUserControl.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(100, 10)));
			AssertMainTabControlBalloonText(shouldShow ? "Milestones" : "");
			MaybeAssertEquals(shouldShow, TrackingUserControl.MainTabControl.GetTabRect(1), TrackingUserControl.LastBalloonRectangle);

			TrackingUserControl.SetFormMousePosition(TrackingUserControl.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(130, 10)));
			AssertMainTabControlBalloonText(shouldShow ? "Exceptions" : "");
			MaybeAssertEquals(shouldShow, TrackingUserControl.MainTabControl.GetTabRect(2), TrackingUserControl.LastBalloonRectangle);

			TrackingUserControl.SetFormMousePosition(TrackingUserControl.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(180, 10)));
			AssertMainTabControlBalloonText(shouldShow ? "Triggers" : "");
			MaybeAssertEquals(shouldShow, TrackingUserControl.MainTabControl.GetTabRect(3), TrackingUserControl.LastBalloonRectangle);

			TrackingUserControl.SetFormMousePosition(TrackingUserControl.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(230, 10)));
			AssertMainTabControlBalloonText(shouldShow ? "Validation" : "");
			MaybeAssertEquals(shouldShow, TrackingUserControl.MainTabControl.GetTabRect(4), TrackingUserControl.LastBalloonRectangle);

			TrackingUserControl.SetFormMousePosition(TrackingUserControl.PointToScreen(ControlDpiScalingHelper.NewScaledPoint(290, 10)));
			AssertMainTabControlBalloonText(shouldShow ? "Events" : "");
			MaybeAssertEquals(shouldShow, TrackingUserControl.MainTabControl.GetTabRect(5), TrackingUserControl.LastBalloonRectangle);
		}

		void MaybeAssertEquals(bool shouldAssert, object expected, object actual)
		{
			if (shouldAssert)
			{
				AssertEquals(expected, actual);
			}
		}

		#endregion

		#region Buffer Management

		[ExpectNoExceptions]
		public void TestBMSEnabledDoesntCauseExceptionOnNonBMSForms()
		{
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var org = Factory.NewWithValidTestData<OrgHeader>();

			using (OrganisationFormTest.OrgFormForTest form = new OrganisationFormTest.OrgFormForTest(org))
			{
				form.ValidateAndSave();
				Thread.Sleep(1000);
				Application.DoEvents();
			}
		}

		#endregion

		#region NavigateToWorkflowItem

		[RequiresSTA]
		public void TestNavigateToWorkflowItem()
		{
			TestWorkflowDescriptor workflowDescriptor = new TestWorkflowDescriptor();
			workflowDescriptor.SupportsEventTrackingExposed = true;
			TestTrackingUserControl.WorkflowDescriptor = workflowDescriptor;

			Form.Show();
			Application.DoEvents();

			Form.TrackingUserControl.NavigateToWorkflowItem(Dummy.WorkflowItems.Exceptions.AddNew());
			Assert("MainTabControl should be switched to exceptionsTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.ExceptionsTab);

			Form.TrackingUserControl.NavigateToWorkflowItem(Dummy.WorkflowItems.MilestonesIncludingRelated.AddNew());
			Assert("MainTabControl should be switched to milestonesTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.MilestonesTab);

			Form.TrackingUserControl.NavigateToWorkflowItem(Dummy.WorkflowItems.TriggersIncludingRelated.AddNew());
			Assert("MainTabControl should be switched to triggersTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.TriggersTab);

			Form.TrackingUserControl.NavigateToWorkflowItem(Dummy.WorkflowItems.Tasks.AddNew());
			Assert("MainTabControl should be switched to tasksTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.TasksTab);
		}

		[RequiresSTA]
		public void TestNavigateToWorkflowItemWithInvalidDWorkflowDescriptor()
		{
			Form.Show();
			Application.DoEvents();

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(Dummy.WorkflowItems.Exceptions.AddNew()));
			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(Dummy.WorkflowItems.MilestonesIncludingRelated.AddNew()));
			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(Dummy.WorkflowItems.TriggersIncludingRelated.AddNew()));
		}

		[RequiresSTA]
		public void TestNavigateToWorkflowItemIncompatibleWorkflowItem()
		{
			WorkflowDescriptors.Instance.TryGetValue(WorkflowDescriptors.GlbGroupWorkflowDescriptorCode, out var workflowDescriptor);
			TestTrackingUserControl.WorkflowDescriptor = workflowDescriptor;
			var group = Factory.New<GlbGroup>();

			Form.Show();
			Application.DoEvents();

			Form.TrackingUserControl.NavigateToWorkflowItem(group.WorkflowItems.Exceptions.AddNew());
			Assert("MainTabControl should be switched to exceptionsTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.ExceptionsTab);

			Form.TrackingUserControl.NavigateToWorkflowItem(group.WorkflowItems.MilestonesIncludingRelated.AddNew());
			Assert("MainTabControl should be switched to milestonesTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.MilestonesTab);

			Form.TrackingUserControl.NavigateToWorkflowItem(group.WorkflowItems.TriggersIncludingRelated.AddNew());
			Assert("MainTabControl should be switched to triggersTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.TriggersTab);

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(group.WorkflowItems.Tasks.AddNew()));
		}

		#endregion

		#region Loading

		[RequiresSTA]
		public void TestOpeningTabDoesNotReloadCollection()
		{
			TestWorkflowDescriptor workflowDescriptor = new TestWorkflowDescriptor();
			workflowDescriptor.SupportsEventTrackingExposed = true;
			TestTrackingUserControl.WorkflowDescriptor = workflowDescriptor;

			var task = Dummy.WorkflowItems.Tasks.AddNew();
			var trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var milestone = Dummy.WorkflowItems.Milestones.AddNew();
			var exception = Dummy.WorkflowItems.Exceptions.AddNew();
			Factory.Save();
			Dummy.WorkflowItems.Tasks.OnRebuild += (s, e) => throw new InvalidOperationException("Do not rebuild during load.");
			Dummy.WorkflowItems.Triggers.OnRebuild += (s, e) => throw new InvalidOperationException("Do not rebuild during load.");
			Dummy.WorkflowItems.TriggersIncludingRelated.OnRebuild += (s, e) => throw new InvalidOperationException("Do not rebuild during load.");
			Dummy.WorkflowItems.Milestones.OnRebuild += (s, e) => throw new InvalidOperationException("Do not rebuild during load.");
			Dummy.WorkflowItems.MilestonesIncludingRelated.OnRebuild += (s, e) => throw new InvalidOperationException("Do not rebuild during load.");
			Dummy.WorkflowItems.MilestonesIncludingRelatedSortable.OnRebuild += (s, e) => throw new InvalidOperationException("Do not rebuild during load.");
			Dummy.WorkflowItems.Exceptions.OnRebuild += (s, e) => throw new InvalidOperationException("Do not rebuild during load.");
			Dummy.WorkflowItems.ExceptionsIncludingRelated.OnRebuild += (s, e) => throw new InvalidOperationException("Do not rebuild during load.");

			var bindings = 0;
			Form.Show();
			Form.TrackingUserControl.ExceptionsTab.ExceptionsUserControl.AfterFirstBinding += (s, e) => bindings++;
			Form.TrackingUserControl.MilestonesTab.MilestonesUserControl.AfterFirstBinding += (s, e) => bindings++;
			Form.TrackingUserControl.TriggersTab.WorkflowTriggersUserControl.AfterFirstBinding += (s, e) => bindings++;

			Application.DoEvents();
			AssertEquals(0, bindings);

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(task));
			Assert("MainTabControl should be switched to tasksTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.TasksTab);
			Application.DoEvents();

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(trigger));
			Application.DoEvents();
			Assert("MainTabControl should be switched to triggersTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.TriggersTab);

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(milestone));
			Application.DoEvents();
			Assert("MainTabControl should be switched to milestonesTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.MilestonesTab);

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(exception));
			Application.DoEvents();
			Assert("MainTabControl should be switched to exceptionsTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.ExceptionsTab);

			AssertEquals("Each tab should not have its binding after being selected", 3, bindings);

			//Switch tabs again now that the bindings are set
			Application.DoEvents();

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(task));
			Assert("MainTabControl should be switched to tasksTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.TasksTab);
			Application.DoEvents();

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(trigger));
			Application.DoEvents();
			Assert("MainTabControl should be switched to triggersTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.TriggersTab);

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(milestone));
			Application.DoEvents();
			Assert("MainTabControl should be switched to milestonesTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.MilestonesTab);

			AssertNoExceptionThrown(() => Form.TrackingUserControl.NavigateToWorkflowItem(exception));
			Application.DoEvents();
			Assert("MainTabControl should be switched to exceptionsTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.ExceptionsTab);
		}

		#endregion

		#region ReadOnly

		[RequiresSTA]
		public void TestP9_ActualDateForBinding_Control_IsReadOnly_TriggersTab()
		{
			TestWorkflowDescriptor workflowDescriptor = new TestWorkflowDescriptor();
			workflowDescriptor.SupportsEventTrackingExposed = true;
			TestTrackingUserControl.WorkflowDescriptor = workflowDescriptor;
			Form.Show();
			Application.DoEvents();
			Form.TrackingUserControl.NavigateToWorkflowItem(Dummy.WorkflowItems.TriggersIncludingRelated.AddNew());
			Assert("MainTabControl should be switched to triggersTab", Form.TrackingUserControl.MainTabControl.SelectedTab == Form.TrackingUserControl.TriggersTab);
			AssertEquals(true, Form.TrackingUserControl.TriggersTab.FindAll<ZDateTimeOffsetEdit>(c => c.Name == "P9_ActualDateDateEdit").Single().ReadOnly);
		}

		public void TestEventTabFiltersEditableWhenFormViewOnly()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var controller = ZControllerFactory.Create(ControllerIDs.SalesEnquiry);

				var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				Factory.Save();

				controller.GetCheckPointForView(enquiry).IsAllowed = true;
				controller.GetCheckPointForEdit(enquiry).IsAllowed = false;
				controller.GetCheckPointForNew(enquiry).IsAllowed = false;

				using (var form = (ZTemplateForm)controller.ShowEditForm(enquiry))
				{
					form.Show();

					NavigateToWorkflowEventsTab(form);

					var filterControl = (ZStmALogFilterControl)form.Controls.Find("LogsControl", true)[0];
					var filterStrip = (ZFilterStrip)filterControl.Controls.Find("ZFilterStrip", true)[0];
					Assert("Filters should not be read only", !filterStrip.ReadOnly);
				}
			}
		}

		public void TestEventTabFiltersEditableWhenFormEditable()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var controller = ZControllerFactory.Create(ControllerIDs.SalesEnquiry);

				var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				Factory.Save();

				controller.GetCheckPointForView(enquiry).IsAllowed = true;
				controller.GetCheckPointForEdit(enquiry).IsAllowed = true;
				controller.GetCheckPointForNew(enquiry).IsAllowed = false;

				using (var form = (ZTemplateForm)controller.ShowEditForm(enquiry))
				{
					form.Show();

					NavigateToWorkflowEventsTab(form);

					var filterControl = (ZStmALogFilterControl)form.Controls.Find("LogsControl", true)[0];
					var filterStrip = (ZFilterStrip)filterControl.Controls.Find("ZFilterStrip", true)[0];
					Assert("Filters should not be read only", !filterStrip.ReadOnly);
				}
			}
		}

		#endregion

		#region Sorting

		public void TestSort()
		{
			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowNewForm())
			{
				Application.DoEvents();
				var workflowUserControl = NavigateToWorkflowTab(form);
				var job = (IWorkflowProvider)form.BusinessEntity;

				var t3 = job.WorkflowItems.Tasks.AddNew();
				var t2 = job.WorkflowItems.Tasks.AddNew();
				var t1 = job.WorkflowItems.Tasks.AddNew();
				t1.P9_Description = "AAA";
				t2.P9_Description = "AAB";
				t3.P9_Description = "AAC";

				var taskGrid = workflowUserControl.FindAll<TaskWithDetailsAndFilterTab>().First().FilterControl.TasksGrid;
				var property = ZCustomTypeDescriptor.GetProperties(typeof(ProcessTask)).Find(ProcessTasksSchema.Constants.P9_Description, false);
				taskGrid.List.ApplySort(property, System.ComponentModel.ListSortDirection.Ascending);
				Application.DoEvents();

				AssertArrayEqualsByElements(new[] { t1, t2, t3 }, job.WorkflowItems.Tasks.ToArray());

				var t5 = job.WorkflowItems.Tasks.AddNew();
				var t4 = job.WorkflowItems.Tasks.AddNew();
				t5.P9_Description = "AAA";
				t4.P9_Description = "AAA";

				job.WorkflowItems.Tasks.Rebuild();
				Application.DoEvents();

				AssertArrayEqualsByElements("Expect order to respect sequence as well as use supplied order", new[] { t1, t5, t4, t2, t3 }, job.WorkflowItems.Tasks.ToArray());
			}
		}

		#endregion

		#region Universal Triggers

		public void TestSaveFormWithUniversalTriggerConditions_ShouldUpdateTriggersOnJob()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);

			var templateTrigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			((BusinessObject)templateTrigger).FillWithValidTestData();
			templateTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			templateTrigger.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger.TemplateCondition2Value = "\"<O1_City>\"==\"Gush\"";

			Factory.Save();

			using (var form = (ZTemplateForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowNewForm())
			{
				form.Show();
				var job = (SalesEnquiry)form.BusinessEntity;
				job.FillWithValidTestData();

				AssertEquals(0, job.WorkflowItems.Triggers.Count);

				job.O1_CompanyName = "Mountain Dew";
				job.O1_EnquiryType = "INQ";
				job.O1_ContactName = "Do the Dew";
				job.O1_City = "Granny";

				job.RunPreSaveValidation();

				AssertNoErrors(job);

				var saveResult = form.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, saveResult);

				NavigateToWorkflowTriggersTab(form);

				AssertEquals("Job still does not match the trigger's template condition.", 0, job.WorkflowItems.TriggersIncludingRelated.Count);

				job.O1_City = "Gush";
				AssertEquals("Job still does not match the trigger's template condition.", 0, job.WorkflowItems.TriggersIncludingRelated.Count);

				saveResult = form.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, saveResult);
				AssertEquals("Wow we did it.", 1, job.WorkflowItems.TriggersIncludingRelated.Count);
			}
		}

		public void TestOpenTriggersTab_WhenUniversalTriggersExist_ShouldBePresent()
		{
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);

			var templateTrigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			((BusinessObject)templateTrigger).FillWithValidTestData();
			templateTrigger.TriggerEventCode = Events.TagWasAddedOrRemovedCode;

			Factory.Save();

			using (var form = (ZForm)ZControllerFactory.Create(ControllerIDs.SalesEnquiry).ShowNewForm())
			{
				Application.DoEvents();

				var job = (SalesEnquiry)form.BusinessEntity;
				job.FillWithValidTestData();
				Assert(string.Join("\r\n", job.GetMessageErrors().Select(e => e.Message)), !job.HasErrors);

				NavigateToWorkflowTab(form);

				var result = form.FireSaveButton();
				AssertNull(ErrorReporter.LastExceptionReported);
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals("Ghosted triggers should appear immediately because we are cool kids who know what they are doing.", 1, job.WorkflowItems.TriggersIncludingRelated.Count);

				NavigateToWorkflowTriggersTab(form);

				Application.DoEvents();

				AssertEquals("Switching to the triggers tab should cause ghosted triggers to appear", 1, job.WorkflowItems.TriggersIncludingRelated.Count);

				var triggersGrid = (ZGrid)form.Controls.Find("WorkflowTriggersGrid", true)[0];

				AssertEquals(1, triggersGrid.List.Count);
				var trigger = (ProcessTask)triggersGrid.List[0];

				AssertEquals(templateTrigger.Identifier, trigger.P9_ParentTemplateID);
			}
		}

		static ZWorkflowUserControl NavigateToWorkflowTab(ZForm form)
		{
			var workflowTabPage = form.FindSingle<ZWorkflowTabPage>();
			((TabControl)workflowTabPage.Parent).SelectTab(workflowTabPage);

			var workflowControl = (ZWorkflowUserControl)form.Controls.Find("ZWorkflowUserControl", true)[0];
			return workflowControl;
		}

		static void NavigateToWorkflowTriggersTab(ZForm form)
		{
			NavigateToWorkflowTab(form);

			var workflowControl = (ZWorkflowUserControl)form.Controls.Find("ZWorkflowUserControl", true)[0];
			var triggersTab = (ZWorkflowTriggersTabPage)workflowControl.MainTabControl.TabPages["TriggersTab"];
			workflowControl.MainTabControl.SelectedTab = triggersTab;
		}

		static void NavigateToWorkflowEventsTab(ZForm form)
		{
			NavigateToWorkflowTab(form);

			var workflowControl = (ZWorkflowUserControl)form.Controls.Find("ZWorkflowUserControl", true)[0];
			var eventsTab = (ZStmALogTabPage)workflowControl.MainTabControl.TabPages["EventsTab"];
			workflowControl.MainTabControl.SelectedTab = eventsTab;
		}

		#endregion

		#region Hiding Tasks Tab

		public void TestTasksTab_WhenWeDoNotWantIt_ShouldBeHidden()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			ShowFormAndAssertTabPageVisibility(true);
			ShowFormAndAssertTabPageVisibility(false);

			void ShowFormAndAssertTabPageVisibility(bool shouldTasksTabBeVisible)
			{
				using (var form = new ZForm())
				using (var control = new ZWorkflowUserControl())
				{
					if (!shouldTasksTabBeVisible)
					{
						DummyWorkflowDescriptor.Instance.ShouldHideTasksTabOnJobs_Exposed = true;
					}

					form.Controls.Add(control);
					control.SetDataBinding(dummy, "");

					var tab = control.MainTabControl.GetTabPageByNameOrText("Tasks");

					if (shouldTasksTabBeVisible)
					{
						AssertNotNull(tab);
					}
					else
					{
						AssertNull(tab);
					}
				}
			}
		}

		#endregion

		#region Test Classes

		class TestForm : ZChildForm
		{
			public TestForm(DummyWithWorkflow dummy)
				: base(dummy)
			{
				this.ControllerID = DummyControllerIDs.Dummy;
				ZTemplateTabControl tabControl = new ZTemplateTabControl();
				ZWorkflowTabPage tabPage = new ZWorkflowTabPage();
				Controls.Add(tabControl);
				tabControl.Controls.Add(tabPage);
				tabPage.Controls.Add(TrackingUserControl);
				tabPage.Initialize(dummy);
			}

			public static WorkflowDescriptor WorkflowDescriptor
			{
				get { return (WorkflowDescriptor)workflowDescriptor.Target; }
				set { workflowDescriptor.Target = value; }
			}
			public static WeakReference workflowDescriptor = new WeakReference(null);

			public TestTrackingUserControl TrackingUserControl
			{
				get
				{
					if (trackingUserControl == null)
					{
						trackingUserControl = new TestTrackingUserControl();
					}
					return trackingUserControl;
				}
			}
			TestTrackingUserControl trackingUserControl;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Controls.Add(TrackingUserControl);
			}

			protected override void Dispose(bool isNotFinalizing)
			{
				base.Dispose(isNotFinalizing);
				if (isNotFinalizing)
				{
					if (trackingUserControl != null)
					{
						trackingUserControl.Dispose();
					}
				}
			}
		}

		class TestTrackingUserControl : ZWorkflowUserControl
		{
			public new ZTabControl MainTabControl
			{
				get { return base.MainTabControl; }
			}

			public void SetFormMousePosition(Point point)
			{
				formMousePosition = point;
			}

			protected override Point FormMousePosition
			{
				get { return formMousePosition ?? base.FormMousePosition; }
			}
			Point? formMousePosition;

			public Rectangle LastBalloonRectangle { get; private set; }
			public string LastBalloonCaption { get; set; }

			protected override void ShowBalloon(string caption, string message, TabPageWithHeaderPostion mouseHoveringTabPage)
			{
				base.ShowBalloon(caption, message, mouseHoveringTabPage);
				LastBalloonRectangle = Balloon.Instance.BalloonWindowExposedForTesting.Descriptor.AnchorRectOnControl;
				Balloon.Instance.Hide();

				LastBalloonCaption = caption;
			}

			public static WorkflowDescriptor WorkflowDescriptor;
			protected override WorkflowDescriptor GetWorkflowDescriptor(ZString workflowType)
			{
				return WorkflowDescriptor ?? base.GetWorkflowDescriptor(workflowType);
			}

			public ZExceptionsTabPage ExceptionsTab
			{
				get { return (ZExceptionsTabPage)exceptionsTab; }
			}

			public ZMilestonesTabPage MilestonesTab
			{
				get { return (ZMilestonesTabPage)milestonesTab; }
			}

			public ZWorkflowTriggersTabPage TriggersTab
			{
				get { return (ZWorkflowTriggersTabPage)triggersTab; }
			}

			public TaskWithDetailsAndFilterTab TasksTab
			{
				get { return (TaskWithDetailsAndFilterTab)tasksTab; }
			}

#if !WINZOR

			internal void OnMouseMove_Exposed()
			{
				OnMouseMove();
			}

#endif
		}

		class TestWorkflowDescriptor : WorkflowDescriptor
		{
			public bool SupportsValidationRulesExposed;

			protected override ValidationToolSettings GetValidationToolSettings() => new TestValidationToolSettings(this, () => SupportsValidationRulesExposed);

			class TestValidationToolSettings(WorkflowDescriptor workflowDescriptor, Func<bool> getSupportsValidationRules) : ValidationToolSettings(workflowDescriptor)
			{
				protected override bool SupportsValidationRulesCore() => getSupportsValidationRules();
			}

			public bool SupportsEventTrackingExposed;
			public override bool SupportsEventTracking
			{
				get { return SupportsEventTrackingExposed; }
			}

			public override string Code
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public override IMultilingualString Description
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public override ControllerID ControllerID
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public override Type WorkflowProviderType
			{
				get { throw new NotImplementedException(); }
			}
		}

		#endregion

		#region Implementation

		void AssertMainTabControlBalloonText(string expectedCaption)
		{
			typeof(Control).InvokeMember("OnMouseHover", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TrackingUserControl.MainTabControl, new object[] { EventArgs.Empty });
			AssertEquals("Caption", expectedCaption, TrackingUserControl.LastBalloonCaption);
		}

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

		TestForm Form
		{
			get
			{
				if (form == null)
				{
					form = new TestForm(Dummy);
				}
				return form;
			}
		}
		TestForm form;

		TestTrackingUserControl TrackingUserControl
		{
			get { return Form.TrackingUserControl; }
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			TestTrackingUserControl.WorkflowDescriptor = null;
			Balloon.Instance.Hide();
		}

		#endregion
	}
}
