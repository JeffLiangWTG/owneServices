using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Business.Test;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test
{
	[TestedType(typeof(ContainmentBarrierInvocationForm))]
	[TestDate(2014, 8, 20)]
	class ContainmentBarrierInvocationFormTest : ZFormBasherTest
	{
		#region ZForm things

		public void TestFormCaption()
		{
			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				AssertEquals("Containment Barrier Outcome", form.FormCaption);
			}
		}

		public void TestIterateReasonSelectDropdown()
		{
			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				var iterateReasonDropEdit = form.FindAll<ZGuidDropEdit>().FirstOrDefault(control => control.Name == "IterateReasonDropEdit");
				AssertNotNull(iterateReasonDropEdit);
			}
		}

		public void TestFormWorkflowSelectDropdown()
		{
			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				var iterateFromWorkflowDropEdit = form.FindAll<ZGuidDropEdit>().FirstOrDefault(control => control.Name == "IterateFromWorkflowDropEdit");
				AssertNotNull(iterateFromWorkflowDropEdit);
			}
		}

		public void TestResourceUnderReviewSelectDropdown()
		{
			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				var resourceUnderReview = form.FindAll<ZDropEdit>().FirstOrDefault(control => control.Name == "ResourceUnderReviewDropEdit");
				AssertNotNull(resourceUnderReview);
			}
		}

		public void TestFormWorkflowSelectDropdownResize()
		{
			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				form.Show();
				var old_size = form.Size;
				form.Size = new System.Drawing.Size(old_size.Width * 2, old_size.Height * 2);
				var iterateFromWorkflowDropEdit = form.FindAll<ZGuidDropEdit>().FirstOrDefault(control => control.Name == "IterateFromWorkflowDropEdit");
				var iterateFromTaskDropEdit = (form.GetFormControl()).Controls.Find("IterateFromTaskDropEdit", true).FirstOrDefault();
				Assert(iterateFromWorkflowDropEdit.Right <= iterateFromTaskDropEdit.Left);
			}
		}

		#endregion

		#region Build

		public void TestBuild_ShouldCreateButtonForValidResponses()
		{
			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				var table = form.OutcomeChoicePanel_ForTest.Controls.OfType<TableLayoutPanel>().Single();

				AssertEquals(3, table.Controls.Count);
				AssertEquals("\r\n\r\n\r\nPassed", table.Controls[0].Text);
				AssertEquals("\r\n\r\n\r\nIteration required", table.Controls[1].Text);
				AssertEquals("\r\n\r\n\r\nCancel", table.Controls[2].Text);
			}
		}

		public void TestBuild_ShouldCreateButtonForValidResponses_WhenDeferableToAnotherResource()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			qcbTask.P9_GS_NKAssignedStaffMember = resource1.GS_Code;

			var qcbTask2 = WorkflowTestCase.BMTestHelper.CreateTask((BusinessObject)qcbTask.Parent, staffCode: resource2.GS_Code, taskType: "QCB");
			qcbTask2.P9_Sequence = qcbTask.P9_Sequence;

			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				form.Show();
				var table = form.OutcomeChoicePanel_ForTest.Controls.OfType<TableLayoutPanel>().Single();

				AssertEquals(4, table.Controls.Count);
				AssertEquals("\r\n\r\n\r\nPassed", table.Controls[0].Text);
				AssertEquals("\r\n\r\n\r\nIteration required", table.Controls[1].Text);
				AssertEquals("\r\n\r\n\r\nDefer to another resource", table.Controls[2].Text);
				AssertEquals("\r\n\r\n\r\nCancel", table.Controls[3].Text);

				AssertEquals("Marks this Containment Barrier as Passed with no additional iterations required. (CTRL-P)", ((ZButton)table.Controls[0]).ToolTipCaption.GetUnresolvedString());
				AssertEquals("Marks this Containment Barrier as requiring an iteration, and creates the necessary tasks. (CTRL-I)", ((ZButton)table.Controls[1]).ToolTipCaption.GetUnresolvedString());
				AssertEquals("Defers the checking of this Containment Barrier to another resource with an open task. (CTRL-D)", ((ZButton)table.Controls[2]).ToolTipCaption.GetUnresolvedString());
				AssertEquals("Cancels checking of this Containment Barrier, and returns the task to its previous status.", ((ZButton)table.Controls[3]).ToolTipCaption.GetUnresolvedString());
			}
		}

		public void TestBuild_ShouldCreateButtonForValidResponses_WhenAnotherResourceHasCreatedAnIterationAlready()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();

			qcbTask.P9_GS_NKAssignedStaffMember = resource1.GS_Code;

			var iterateFromTask = WorkflowTestCase.BMTestHelper.CreateTask((BusinessObject)qcbTask.Parent, resource1.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1);
			var qcbTask2 = WorkflowTestCase.BMTestHelper.CreateTask((BusinessObject)qcbTask.Parent, resource2.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: qcbTask.P9_Sequence, taskType: "QCB");
			WorkflowTestCase.CreateQualityIteration(qcbTask2, iterateFromTask);

			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				form.Show();
				var table = form.OutcomeChoicePanel_ForTest.Controls.OfType<TableLayoutPanel>().Single();

				AssertEquals(4, table.Controls.Count);
				AssertEquals("\r\n\r\n\r\nPassed", table.Controls[0].Text);
				AssertEquals("\r\n\r\n\r\nIteration required", table.Controls[1].Text);
				AssertEquals("\r\n\r\n\r\nAccept iteration already created", table.Controls[2].Text);
				AssertEquals("\r\n\r\n\r\nCancel", table.Controls[3].Text);

				AssertEquals("Marks this Containment Barrier as Passed with no additional iterations required. (CTRL-P)", ((ZButton)table.Controls[0]).ToolTipCaption.GetUnresolvedString());
				AssertEquals("Marks this Containment Barrier as requiring an iteration, and creates the necessary tasks. (CTRL-I)", ((ZButton)table.Controls[1]).ToolTipCaption.GetUnresolvedString());
				AssertEquals("Accepts the iteration already created by another resource. (CTRL-D)", ((ZButton)table.Controls[2]).ToolTipCaption.GetUnresolvedString());
				AssertEquals("Cancels checking of this Containment Barrier, and returns the task to its previous status.", ((ZButton)table.Controls[3]).ToolTipCaption.GetUnresolvedString());
			}
		}

		#endregion

		#region Usability Shortcuts

		public void TestCloseWithoutChoosingOption_ShouldUseCancel()
		{
			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				form.Show();

				AssertNull(viewModel.Response);

				form.Close();
				AssertEquals(ContainmentBarrierResponses.Canceled, viewModel.Response);
			}
		}

		public void TestShowFormAndPressEnter_ShouldPassQCB_WhenResourceUnderReviewIsPrefilled()
		{
			// Prefilling the resource under review requires a workflow
			var job = (BusinessObject)Factory.New<IWorkItem>();
			var jobHeader = WorkflowTestCase.BMTestHelper.GetJobHeaderForParent((IWorkflowProviderCore)job, Factory);
			var workflow = WorkflowTestCase.BMTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var previousResource = Factory.NewWithValidTestData<GlbStaff>();
			previousResource.GS_FullName = "Previous Person";
			var previousTask = WorkflowTestCase.BMTestHelper.CreateTask(workflow, previousResource.GS_Code, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1);
			var qcbTask = WorkflowTestCase.BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskType: "QCB", sequence: 20);

			using (ContainmentBarrierInvocationForm.ShowNonModally_ForTest())
			{
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				using (var form = Application.OpenForms.OfType<ContainmentBarrierInvocationForm>().SingleOrDefault())
				{
					AssertNotNull("Should open containment barriers form when closing QCB task", form);

					AssertNotNullOrEmpty("User Under Review should be prefilled", form.FindSingle<ZDropEdit>("ResourceUnderReviewDropEdit").Text);

					var button = form.OutcomeChoicePanel_ForTest.Controls.OfType<TableLayoutPanel>().Single().Controls.OfType<ZButton>().Single(b => b.Focused);
					button.PerformClick();

					AssertEquals(true, form.IsDisposed);
					WorkflowTestCase.AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier Passed", qcbTask.P9_Notes);
				}
			}
		}

		public void TestShowFormAndPressEscape_ShouldCancelQCBCheck()
		{
			using (ContainmentBarrierInvocationForm.ShowNonModally_ForTest())
			{
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				using (var form = Application.OpenForms.OfType<ContainmentBarrierInvocationForm>().SingleOrDefault())
				{
					AssertNotNull("Should open containment barriers form when closing QCB task", form);
					var button = form.CancelButton;
					button.PerformClick();

					AssertEquals(true, form.IsDisposed);
					WorkflowTestCase.AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier verification canceled", qcbTask.P9_Notes);
				}
			}
		}

		public void TestShowForm_ShouldFocusResourceUnderReview_WhenNotPrefilled()
		{
			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				form.Show();

				var resourceControl = form.FindSingle<ZDropEdit>("ResourceUnderReviewDropEdit");

				AssertNullOrEmpty("User Under Review should not be prefilled", resourceControl.Text);

				AssertEquals("Outcome buttons should not have focus", false, form.OutcomeChoicePanel_ForTest.ContainsFocus);
				AssertEquals("User Under Review should have focus", true, resourceControl.ContainsFocus);
			}
		}

		public void TestShowForm_ShouldFocusPassedButton_WhenResourceUnderReviewPrefilled()
		{
			viewModel.ResourceUnderReviewNK = "FRO";
			using (var form = new ContainmentBarrierInvocationForm(viewModel))
			{
				form.Show();

				var resourceControl = form.FindSingle<ZDropEdit>("ResourceUnderReviewDropEdit");
				AssertNotNullOrEmpty("User Under Review should be prefilled", resourceControl.Text);

				AssertEquals("User Under Review should not have focus", false, resourceControl.ContainsFocus);

				var passedButton = form.FindSingle<ZButton>(nameof(ContainmentBarrierResponses.Passed));
				AssertEquals("Passed button should have focus", true, passedButton.ContainsFocus);
			}
		}

		#endregion

		#region Select Option

		public void TestSelectOption_IterationRequired_ShouldExpandForm()
		{
			var rollbackToTask = qcbTask.Parent.WorkflowItems.Tasks.AddNew();
			rollbackToTask.P9_Sequence = 10;

			using (ContainmentBarrierInvocationForm.ShowNonModally_ForTest())
			{
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				using (var form = Application.OpenForms.OfType<ContainmentBarrierInvocationForm>().SingleOrDefault())
				{
					AssertNotNull("Should open containment barriers form when closing QCB task", form);

					AssertEquals(FormBorderStyle.FixedToolWindow, form.FormBorderStyle);
					AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(230), form.Height);
					AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(230), form.MinimumSize.Height);

					var button = (ZButton)form.Controls.Find("IterationRequired", true)[0];
					button.PerformClick();
					Application.DoEvents();

					AssertEquals("Selecting Quality Iteration Required should not close the form", false, form.IsDisposed);

					AssertEquals(FormBorderStyle.SizableToolWindow, form.FormBorderStyle);
					AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(492), form.Height);
					AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(492), form.MinimumSize.Height);

					AssertGreaterThanOrEqualTo("Task Preview grid should be large enough to show about six rows", form.FindSingle<ZGrid>("TaskPreviewGrid").Height, ControlDpiScalingHelper.ScaleToCurrentDpiY(120));

					WorkflowTestCase.AssertRtfText("Should not commit quality iteration yet", ZString.Empty, qcbTask.P9_Notes);

					var createIterationButton = (ZButton)form.Controls.Find("CreateIterationButton", true)[0];
					AssertEquals("Should focus Create Iteration button for easy keyboard accessibility", true, createIterationButton.Focused);
					createIterationButton.PerformClick();

					AssertEquals("Selecting Create Iteration when there are errors should not close the form", false, form.IsDisposed);
					WorkflowTestCase.AssertRtfText("Should not commit quality iteration yet", ZString.Empty, qcbTask.P9_Notes);

					var viewModel = (ContainmentBarrierViewModel)form.DataSource;
					viewModel.IterateFromTaskPK = rollbackToTask.PK;
					createIterationButton.PerformClick();

					AssertEquals("Selecting Create Iteration when there are no errors should close the form", true, form.IsDisposed);
					WorkflowTestCase.AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: UDF - Undefined - You can modify this in the System Registry, under Workflow Manager/Quality Iteration Reasons", qcbTask.P9_Notes);
				}
			}
		}

		public void TestSelectOption_IterationRequired_WhenQCBTaskNotSelected()
		{
			var task1 = qcbTask.Parent.WorkflowItems.Tasks.AddNew();
			var task2 = qcbTask.Parent.WorkflowItems.Tasks.AddNew();

			task1.P9_Sequence = 1;
			task2.P9_Sequence = 2;

			using (ContainmentBarrierInvocationForm.ShowNonModally_ForTest())
			{
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				using (var form = Application.OpenForms.OfType<ContainmentBarrierInvocationForm>().SingleOrDefault())
				{
					AssertNotNull("Should open containment barriers form when closing QCB task", form);
					var viewModel = (ContainmentBarrierViewModel)form.DataSource;

					((ZButton)form.Controls.Find("IterationRequired", true)[0]).PerformClick();
					Application.DoEvents();

					viewModel.IterateFromTaskPK = task1.PK;

					AssertEquals(3, viewModel.IterationTaskPreviews.Count);
					AssertEquals(task1, viewModel.IterationTaskPreviews[0].Task);
					AssertEquals(task2, viewModel.IterationTaskPreviews[1].Task);
					AssertEquals(qcbTask, viewModel.IterationTaskPreviews[2].Task);

					viewModel.IterationTaskPreviews[0].IncludeInIteration = false;

					var createIterationButton = (ZButton)form.Controls.Find("CreateIterationButton", true)[0];
					createIterationButton.PerformClick();

					AssertEquals("Selecting Create Iteration when there are errors should not close the form", false, form.IsDisposed);

					viewModel.IterationTaskPreviews[0].IncludeInIteration = true;
					viewModel.IterationTaskPreviews[2].IncludeInIteration = false;

					createIterationButton.PerformClick();
					AssertEquals("Selecting Create Iteration when there are errors should not close the form", false, form.IsDisposed);

					viewModel.IterationTaskPreviews[2].IncludeInIteration = true;

					createIterationButton.PerformClick();
					AssertEquals("Selecting Create Iteration when there are no errors should close the form", true, form.IsDisposed);
					WorkflowTestCase.AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: UDF - Undefined - You can modify this in the System Registry, under Workflow Manager/Quality Iteration Reasons", qcbTask.P9_Notes);
				}
			}
		}

		public void TestSelectOption_IterationRequired_WhenAnotherResourceHasAlreadyCreatedIteration()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			var resource3 = Factory.NewWithValidTestData<GlbStaff>();
			resource3.GS_FullName = "Samwise Gamgee";

			qcbTask.P9_GS_NKAssignedStaffMember = resource3.GS_Code;

			var rollbackToTask = qcbTask.Parent.WorkflowItems.Tasks.AddNew();
			rollbackToTask.P9_Sequence = 10;
			rollbackToTask.P9_GS_NKAssignedStaffMember = resource1.GS_Code;
			rollbackToTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var qcbTask2 = qcbTask.Parent.WorkflowItems.Tasks.AddNew();
			qcbTask2.P9_Sequence = qcbTask.P9_Sequence;
			qcbTask2.P9_GS_NKAssignedStaffMember = resource2.GS_Code;
			qcbTask2.P9_Type = "QCB";

			using (ContainmentBarrierInvocationForm.ShowNonModally_ForTest())
			{
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				using (var form = Application.OpenForms.OfType<ContainmentBarrierInvocationForm>().SingleOrDefault())
				{
					AssertNotNull("Should open containment barriers form when closing QCB task", form);

					((ZButton)form.Controls.Find("IterationRequired", true)[0]).PerformClick();
					((ZButton)form.Controls.Find("CreateIterationButton", true)[0]).PerformClick();
					WorkflowTestCase.AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: UDF - Undefined - You can modify this in the System Registry, under Workflow Manager/Quality Iteration Reasons", qcbTask.P9_Notes);
				}

				qcbTask2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				using (var form = Application.OpenForms.OfType<ContainmentBarrierInvocationForm>().SingleOrDefault())
				{
					AssertNotNull("Should open containment barriers form when closing QCB task", form);

					((ZButton)form.Controls.Find("AcceptIterationCreatedByOtherResource", true)[0]).PerformClick();
					WorkflowTestCase.AssertRtfText("FRO 20-Aug-14 00:00: Accepted Quality Iteration created by Samwise Gamgee", qcbTask2.P9_Notes);
				}
			}
		}

		public void TestSelectOption_IterationRequired_IterationReasonNotNeeded()
		{
			var iterationReasonsColl = WorkflowDataRegistry.Instance.IterationReasons.Value;
			var iterationReason = iterationReasonsColl
				.OfType<CategorisedWorkflowIterationReasons>()
				.FirstOrDefault(x => x.Code == qcbTask.Parent.WorkflowType);

			if (iterationReason == null)
			{
				iterationReason = iterationReasonsColl.AddNew();
				iterationReason.Code = qcbTask.Parent.WorkflowType;
			}

			var iterationReason1 = iterationReason.IterationReasons.AddNew();
			iterationReason1.Code = "RS1";
			iterationReason1.Description = (NoResString)"Reason 1";

			var iterationReason2 = iterationReason.IterationReasons.AddNew();
			iterationReason2.Code = "RS2";
			iterationReason2.Description = (NoResString)"Reason 2";

			iterationReason.IterationReasonValidation = IterationReasonValidationList.Codes.None;

			WorkflowDataRegistry.Instance.IterationReasons.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, iterationReasonsColl);

			var rollbackToTask = qcbTask.Parent.WorkflowItems.Tasks.AddNew();
			rollbackToTask.P9_Sequence = 10;

			using (ContainmentBarrierInvocationForm.ShowNonModally_ForTest())
			{
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				using (var form = Application.OpenForms.OfType<ContainmentBarrierInvocationForm>().SingleOrDefault())
				{
					AssertNotNull("Should open containment barriers form when closing QCB task", form);

					var button = (ZButton)form.Controls.Find("IterationRequired", true)[0];
					button.PerformClick();
					Application.DoEvents();

					AssertEquals("Selecting Quality Iteration Required should not close the form", false, form.IsDisposed);

					WorkflowTestCase.AssertRtfText("Should not commit quality iteration yet", ZString.Empty, qcbTask.P9_Notes);

					var createIterationButton = (ZButton)form.Controls.Find("CreateIterationButton", true)[0];
					var viewModel = (ContainmentBarrierViewModel)form.DataSource;
					viewModel.IterateFromTaskPK = rollbackToTask.PK;
					createIterationButton.PerformClick();

					AssertEquals("Selecting Create Iteration when there are no errors should close the form", true, form.IsDisposed);
					WorkflowTestCase.AssertRtfText("FRO 20-Aug-14 00:00: Containment Barrier triggered a Quality Iteration", qcbTask.P9_Notes);
				}
			}
		}

		public void TestSettingIterateFromWorkflow_ShouldResetIterateFromTask()
		{
			var job = (BusinessObject)Factory.New<IWorkItem>();
			var jobHeader = WorkflowTestCase.BMTestHelper.GetJobHeaderForParent((IWorkflowProviderCore)job, Factory);
			var workflow = WorkflowTestCase.BMTestHelper.CreateWorkflow(jobHeader, "Workfow");
			var qcbTask = (ProcessTask)WorkflowTestCase.BMTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskType: "QCB");
			qcbTask.P9_Sequence = 20;

			var rollbackToTask = (ProcessTask)WorkflowTestCase.BMTestHelper.CreateTask(workflow, description: "rollbackToTask", sequence: 10);

			Factory.Save();

			using (ContainmentBarrierInvocationForm.ShowNonModally_ForTest())
			{
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				using (var form = Application.OpenForms.OfType<ContainmentBarrierInvocationForm>().SingleOrDefault())
				{
					AssertNotNull("Should open containment barriers form when closing QCB task", form);

					var button = (ZButton)form.Controls.Find("IterationRequired", true)[0];
					button.PerformClick();
					Application.DoEvents();

					var iterateFromWorkflowDropEdit = form.FindAll<ZGuidDropEdit>().FirstOrDefault(control => control.Name == "IterateFromWorkflowDropEdit");
					var iterateFromTaskDropEdit = form.FindAll<ZGuidDropEdit>().FirstOrDefault(control => control.Name == "IterateFromTaskDropEdit");

					var viewModel = (ContainmentBarrierViewModel)form.DataSource;
					viewModel.IterateFromTaskPK = rollbackToTask.PK;

					WaitForConditionToMeet(() => iterateFromTaskDropEdit.DescriptionBox.Text == "10  rollbackToTask  WI00000002 - Workfow");

					AssertEquals("Precondition", "10  rollbackToTask  WI00000002 - Workfow", iterateFromTaskDropEdit.DescriptionBox.Text);
					AssertEquals("Precondition", string.Empty, iterateFromWorkflowDropEdit.DescriptionBox.Text);

					viewModel.IterateFromWorkflowPK = workflow.PK;

					WaitForConditionToMeet(() => string.IsNullOrEmpty(iterateFromTaskDropEdit.DescriptionBox.Text));

					AssertEquals("WI00000002 - Workfow", iterateFromWorkflowDropEdit.DescriptionBox.Text);
					Assert("Should update the view model when selecting a workflow", !viewModel.IterateFromTaskPK.IsValid);
					AssertEquals("Should reset the Iterate From Task field when selecting a workflow", string.Empty, iterateFromTaskDropEdit.DescriptionBox.Text);
					Assert("Iterate From Task field should have focus", iterateFromTaskDropEdit.ContainsFocus);
				}
			}
		}

		void WaitForConditionToMeet(Func<bool> conditionChecker)
		{
			int maxNumOfAttempts = 100;

			for (int i = 0; i < maxNumOfAttempts; i++)
			{
				if (conditionChecker())
				{
					return;
				}
				Application.DoEvents();
				Thread.Sleep(10);
			}
		}

		#endregion

		#region Option to Create New Workflow for Iteration

		public void TestIterationTasksHintLabel_WhenShouldCreateWorkflowForIterationIsToggled_ShouldUpdateText()
		{
			viewModel.ShouldCreateWorkflowForIteration = true;

			using (var form = CreateAndShowFormAndSelectIterationRequired(viewModel))
			{
				var label = form.FindSingle<ZLabel>("IterationTasksHintLabel");
				AssertEquals("The following tasks will be duplicated and added to a Quality Iteration workflow. Un-tick those you do not wish to include.", label.Text);

				viewModel.ShouldCreateWorkflowForIteration = false;
				AssertEquals("The following tasks will be duplicated and added to the workflow for the containment barrier task. Un-tick those you do not wish to include.", label.Text);

				viewModel.ShouldCreateWorkflowForIteration = true;
				AssertEquals("The following tasks will be duplicated and added to a Quality Iteration workflow. Un-tick those you do not wish to include.", label.Text);
			}
		}

		public void TestShouldCreateWorkflowForIterationCheckBox_WhenPropertyIsNotReadOnly_ShouldBeVisible()
		{
			WorkflowDataRegistry.Instance.AllowUsersToChangeIterationWorkflowCreationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var newViewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed, deselectCancelledTasksFromIteration: true))
			{
				AssertEquals("Precondition", false, newViewModel.ShouldCreateWorkflowForIterationInfo.ReadOnly);

				using (var form = CreateAndShowFormAndSelectIterationRequired(newViewModel))
				{
					var checkbox = form.FindSingle<ZCheckBox>("ShouldCreateWorkflowForIterationCheckBox");
					AssertEquals("Users are allowed to change the setting, so the checkbox should be visible. SAD!", true, checkbox.Visible);
				}
			}
		}

		public void TestShouldCreateWorkflowForIterationCheckBox_WhenPropertyIsReadOnly_ShouldNotBeVisible()
		{
			WorkflowDataRegistry.Instance.AllowUsersToChangeIterationWorkflowCreationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var newViewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed, deselectCancelledTasksFromIteration: true))
			{
				AssertEquals("Precondition", true, newViewModel.ShouldCreateWorkflowForIterationInfo.ReadOnly);

				using (var form = CreateAndShowFormAndSelectIterationRequired(newViewModel))
				{
					var checkbox = form.FindSingle<ZCheckBox>("ShouldCreateWorkflowForIterationCheckBox");
					AssertEquals("Users are not allowed to change the setting, so the checkbox should not be visible. SAD!", false, checkbox.Visible);
				}
			}
		}

		#endregion

		#region Overrides

		protected override bool AllowHasChangesOnFormOpen => true;

		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			DisposeBasherForm = false;
			base.TestBoundListsAreNotLoadedOnAccess();
			DisposeBasherForm = true;
		}

		#endregion

		#region Implementation

		ProcessTask qcbTask;
		ContainmentBarrierViewModel viewModel;
		IDisposable userContextDisposable;
		bool DisposeBasherForm { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			WorkflowTestCase.EnableBufferManagement();
			WorkflowTestCase.SetAsQCBTaskType("QCB", "WKI");

			var resource = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, "FRO") ?? Factory.NewWithValidTestData<GlbStaff>();
			resource.GS_Code = "FRO";
			resource.GS_FullName = "Frodo Baggins";
			Factory.Save();

			qcbTask = (ProcessTask)WorkflowTestCase.BMTestHelper.CreateTask((BusinessObject)Factory.New<IWorkItem>(), GlbStaff.CurrentUser.GS_Code, taskType: "QCB");
			qcbTask.P9_Sequence = 20;
			viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed, deselectCancelledTasksFromIteration: true);

			userContextDisposable = Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			DisposeBasherForm = true;
		}

		ContainmentBarrierInvocationForm CreateAndShowFormAndSelectIterationRequired(ContainmentBarrierViewModel viewModel, bool setMinimumWidth = false)
		{
			var form = new ContainmentBarrierInvocationForm(viewModel);
			if (setMinimumWidth)
			{
				form.Width = form.MinimumSize.Width;
			}
			form.Show();
			Application.DoEvents();

			var button = (ZButton)form.Controls.Find("IterationRequired", true)[0];
			button.PerformClick();
			Application.DoEvents();

			return form;
		}

		protected override void TearDown()
		{
			viewModel.Dispose();
			base.TearDown();
			userContextDisposable.Dispose();
		}

		protected override Form GetFormToBashCore()
		{
			WorkflowDataRegistry.Instance.AllowUsersToChangeIterationWorkflowCreationOptions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (var newViewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed, deselectCancelledTasksFromIteration: true))
			{
				var form = CreateAndShowFormAndSelectIterationRequired(newViewModel, setMinimumWidth: true);

				if (DisposeBasherForm)
				{
					using (form)
					{
						return form;
					}
				}

				return form;
			}
		}

		#endregion
	}

	class ContainmentBarrierInvocationFormPerformanceTest : WorkflowTestCase
	{
		[TestDate(2014, 10, 16)]
		public void TestInvokeQualityIteration_DbHits()
		{
			EnableBufferManagement();
			SetAsQCBTaskType("QCB", "WKI");

			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_IsController = true;
			resource2.GS_Code = "FRO";
			var system = BMTestHelper.CreateSystem(Factory, "WKI");

			var jobHeader = BMTestHelper.CreateJobHeader<IWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var job = (BusinessObject)jobHeader.Parent;
			var workflow = BMTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task1 = BMTestHelper.CreateTask(workflow, resource1.GS_Code, taskType: "UDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMTestHelper.CreateTask(workflow, resource2.GS_Code, taskType: "QCB");

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource2.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (ContainmentBarrierInvocationForm.ShowNonModally_ForTest())
			using (var workItemForm = (ZForm)ZControllerFactory.Create(ControllerIDs.WorkItem).ShowEditForm(job))
			{
				workItemForm.Show();
				var formFactory = workItemForm.BusinessEntity.Factory;

				var qcbTask = formFactory.Load<ProcessTask>(task2.PK);
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				var baselineProcessTasksHits = formFactory.GetTableHitCount(ProcessTasksSchema.Constants.TableName);
				var baselineProcessHeaderHits = formFactory.GetTableHitCount(ProcessHeaderSchema.Constants.TableName);
				var baselineProcessHeaderLinkHits = formFactory.GetTableHitCount(ProcessHeaderLinkSchema.Constants.TableName);
				var baselineProcessTaskIterationLinkHits = formFactory.GetTableHitCount(ProcessTaskIterationLinkSchema.Constants.TableName);

				using (var form = Application.OpenForms.OfType<ContainmentBarrierInvocationForm>().Single())
				{
					((ZButton)form.Controls.Find("IterationRequired", true)[0]).PerformClick();

					AssertMaxDbHits("Db hits after choosing Iteration Required option", new Dictionary<string, int>
					{
						{ ProcessTasksSchema.Constants.TableName, baselineProcessTasksHits },
						{ ProcessHeaderSchema.Constants.TableName, baselineProcessHeaderHits },
						{ ProcessHeaderLinkSchema.Constants.TableName, baselineProcessHeaderLinkHits },
						{ ProcessTaskIterationLinkSchema.Constants.TableName, baselineProcessTaskIterationLinkHits },
						{ BMSystemSchema.Constants.TableName, 1 },
						{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
						{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
						{ GenCustomAddOnValueSchema.Constants.TableName, 1 },
						{ GenPivotSchema.Constants.TableName, 1 },
						{ JobHeaderSchema.Constants.TableName, 1 },
						{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
						{ StmALogSchema.Constants.TableName, 1 },
						{ StmNoteSchema.Constants.TableName, 1 },
						{ WorkItemSchema.Constants.TableName, 1 },
						{ WorkItemRequestLinkSchema.Constants.TableName, 1 }
					}, formFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);

					((ZButton)form.Controls.Find("CreateIterationButton", true)[0]).PerformClick();
					AssertRtfText("FRO 16-Oct-14 00:00: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: UDF - Undefined - You can modify this in the System Registry, under Workflow Manager/Quality Iteration Reasons", qcbTask.P9_Notes);
				}

				AssertMaxDbHits("Db hits after committing Iteration Required response", new Dictionary<string, int>
				{
					{ ProcessTasksSchema.Constants.TableName, baselineProcessTasksHits },
					{ ProcessHeaderSchema.Constants.TableName, baselineProcessHeaderHits + 1 },
					{ ProcessHeaderLinkSchema.Constants.TableName, baselineProcessHeaderLinkHits },
					{ ProcessTaskIterationLinkSchema.Constants.TableName, baselineProcessTaskIterationLinkHits + 1 },
					{ BMComponentSchema.Constants.TableName, 1 },
					{ BMSystemSchema.Constants.TableName, 1 },
					{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
					{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
					{ GenCustomAddOnRuleAckSchema.Constants.TableName, 1 },
					{ GenCustomAddOnValueSchema.Constants.TableName, 2 },
					{ GenPivotSchema.Constants.TableName, 1 },
					{ GlbStaffHolidaySchema.Constants.TableName, 3 },
					{ JobHeaderSchema.Constants.TableName, 1 },
					{ ProcessTaskIterationLinkPivotSchema.Constants.TableName, 2 },
					{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
					{ StmALogSchema.Constants.TableName, 1 },
					{ StmNoteSchema.Constants.TableName, 1 },
					{ WorkItemSchema.Constants.TableName, 1 },
					{ WorkItemRequestLinkSchema.Constants.TableName, 1 }
				}, formFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true);
			}
		}
	}
}
