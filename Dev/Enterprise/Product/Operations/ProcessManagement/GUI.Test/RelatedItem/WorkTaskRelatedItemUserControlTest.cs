using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.GUI.Test
{
	public class WorkTaskRelatedItemUserControlTest : TestCaseWithFactory
	{
		#region FormForTest

		protected class FormForTest : ZForm
		{
			public FormForTest(object workItem, bool isViewOrDeleteMode)
				: base(workItem)
			{
				SetToViewOrDeleteMode = isViewOrDeleteMode;
				Controls.Add(WorkTaskRelatedItemUserControl);
			}

			protected bool SetToViewOrDeleteMode;

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					userControl?.Dispose();
				}
				base.Dispose(disposing);
			}

			IWorkTaskRelatedItemUserControlForTest userControl;
			public IWorkTaskRelatedItemUserControlForTest UserControl
			{
				get { return userControl ?? (userControl = GetWorkTaskRelatedItemUserControlForTest()); }
			}

			protected virtual IWorkTaskRelatedItemUserControlForTest GetWorkTaskRelatedItemUserControlForTest() => new WorkTaskRelatedItemUserControlForTest(SetToViewOrDeleteMode);

			public WorkTaskRelatedItemUserControl WorkTaskRelatedItemUserControl => (WorkTaskRelatedItemUserControl)UserControl;
		}

		protected interface IWorkTaskRelatedItemUserControlForTest : IDisposable
		{
			ZGroupBox RelatedItemGroupBox { get; }
			ZGrid RelatedItemGrid { get; }
			ZButton DetachButton { get; }
			ZButton EditButton { get; }
			ZButton AttachButton { get; }
			ZButton NewButton { get; }
			ZGroupBox NetworkDiagramGroupBox { get; }
			ZGroupBox ParentWorkflowGroupBox { get; }
			ZGroupBox ChildWorkflowGroupBox { get; }

			void AttachProjectButton_Click();
			bool NewButton_Click(string text);
		}

		protected class WorkTaskRelatedItemUserControlForTest : WorkTaskRelatedItemUserControl, IWorkTaskRelatedItemUserControlForTest
		{
			public WorkTaskRelatedItemUserControlForTest(bool isViewOrDeleteMode) : base(isViewOrDeleteMode) { }

			public new ZGroupBox RelatedItemGroupBox => base.RelatedItemGroupBox;
			public new ZGrid RelatedItemGrid => base.RelatedItemGrid;
			public new ZButton DetachButton => base.DetachButton;
			public new ZButton EditButton => base.EditButton;
			public new ZButton AttachButton => base.AttachButton;
			public new ZButton NewButton => base.NewButton;

			public new ZGroupBox NetworkDiagramGroupBox => base.NetworkDiagramGroupBox;
			public new ZGroupBox ParentWorkflowGroupBox => base.ParentWorkflowGroupBox;
			public new ZGroupBox ChildWorkflowGroupBox => base.ChildWorkflowGroupBox;

			public void AttachProjectButton_Click()
			{
				FindMenuItem("Project").PerformClick();
			}

			public bool NewButton_Click(string text)
			{
				foreach (ToolStripMenuItem item in menuStripNew.Items)
				{
					if (item.Text == text)
					{
						item.PerformClick();
						return true;
					}
				}

				return false;
			}

			ToolStripMenuItem FindMenuItem(string text)
			{
				ToolStripMenuItem result = null;
				foreach (ToolStripMenuItem item in menuStripAttach.Items)
				{
					if (item.Text == text)
					{
						result = item;
						break;
					}
				}
				return result;
			}
		}

		#endregion

		public void TestDisableControlWhenViewOrDelete()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			using (var readOnlyForm = GetFormForTest(workItem, true))
			{
				readOnlyForm.Show();
				AssertEquals("Grid is enabled", false, readOnlyForm.UserControl.RelatedItemGrid.Enabled);
				AssertEquals("NewButton is enabled", false, readOnlyForm.UserControl.NewButton.Enabled);
				AssertEquals("AttachButton is enabled", false, readOnlyForm.UserControl.AttachButton.Enabled);
				AssertEquals("EditButton is enabled", false, readOnlyForm.UserControl.EditButton.Enabled);
				AssertEquals("DetachButton is enabled", false, readOnlyForm.UserControl.DetachButton.Enabled);
			}

			using (var editableForm = GetFormForTest(workItem, false))
			{
				editableForm.Show();
				AssertEquals("Grid is enabled", true, editableForm.UserControl.RelatedItemGrid.Enabled);
				AssertEquals("NewButton is enabled", true, editableForm.UserControl.NewButton.Enabled);
				AssertEquals("AttachButton is enabled", true, editableForm.UserControl.AttachButton.Enabled);
				AssertEquals("EditButton is enabled", true, editableForm.UserControl.EditButton.Enabled);
				AssertEquals("DetachButton is enabled", true, editableForm.UserControl.DetachButton.Enabled);
			}
		}

		#region Job Relationships

		public void TestAttachRelatedItems()
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			using (var form = GetFormForTest(workItem, false))
			{
				form.Show();
				Application.DoEvents();
				form.UserControl.AttachProjectButton_Click();
				AssertEquals("Projects", form.WorkTaskRelatedItemUserControl.LastAttacher.LastShownAttachPopupForTesting.Text);
				form.WorkTaskRelatedItemUserControl.LastAttacher.LastShownAttachPopupForTesting.Dispose();
			}
		}

		public void TestDetachRelatedItems()
		{
			var project1 = Factory.NewWithValidTestData<Project>();
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.RelatedItems.Add(project1);

			using (var form = GetFormForTest(workItem, false))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(1, form.UserControl.RelatedItemGrid.List.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.RelatedItemGrid.Select(0);
				form.UserControl.DetachButton.PerformClick();

				AssertEquals("Project should be detached", 0, form.UserControl.RelatedItemGrid.List.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.UserControl.DetachButton.PerformClick();
				AssertEquals(0, form.UserControl.RelatedItemGrid.List.Count);

				var project2 = Factory.NewWithValidTestData<Project>();
				var project3 = Factory.NewWithValidTestData<Project>();
				workItem.RelatedItems.Add(project2);
				workItem.RelatedItems.Add(project3);

				AssertEquals(2, form.UserControl.RelatedItemGrid.List.Count);

				form.UserControl.RelatedItemGrid.SelectAllElements();
				form.UserControl.DetachButton.PerformClick();

				AssertEquals("Both Projects should be detached", 0, form.UserControl.RelatedItemGrid.List.Count);
			}
		}

		public void TestAttachNewWorkItem_Security()
		{
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_Summary = "Testing";
			project.WKP_ProjectNumber = "";

			using (var form = GetFormForTest(project, false))
			{
				form.Show();
				Env.Security.WorkItemNew.IsAllowed = false;
				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () => form.UserControl.NewButton_Click("Work Item"));
				AssertNull(form.WorkTaskRelatedItemUserControl.LastController.LastShownForm);

				form.Show();
				Env.Security.WorkItemNew.IsAllowed = true;
				Assert("Button Clicked", form.UserControl.NewButton_Click("Work Item"));
				ZForm lastForm = (ZForm)form.WorkTaskRelatedItemUserControl.LastController.LastShownForm;
				AssertNotNull(lastForm);
				lastForm.Dispose();
			}
		}

		public void TestOpenRelatedItem()
		{
			var project = Factory.NewWithValidTestData<Project>();
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.RelatedItems.Add(project);
			Factory.Save();

			using (var form = GetFormForTest(workItem, false))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(1, form.UserControl.RelatedItemGrid.List.Count);

				int projectRow = 0;

				form.UserControl.RelatedItemGrid.Select(projectRow);
				form.UserControl.RelatedItemGrid.ListManager.Position = projectRow;
				form.UserControl.EditButton.PerformClick();
				AssertNotNull(form.WorkTaskRelatedItemUserControl.LastController);
				AssertEquals(ControllerIDs.Project, form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.ControllerID);
				form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.Dispose();
			}
		}

		public void TestOpenRelatedItem_DoubleClick()
		{
			var project = Factory.NewWithValidTestData<Project>();
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.RelatedItems.Add(project);
			Factory.Save();

			using (var form = GetFormForTest(workItem, false))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(1, form.UserControl.RelatedItemGrid.List.Count);

				int projectRow = 0;

				form.UserControl.RelatedItemGrid.Select(projectRow);
				form.UserControl.RelatedItemGrid.ListManager.Position = projectRow;
				form.UserControl.RelatedItemGrid.PerformDoubleClickForTest();
				AssertNotNull(form.WorkTaskRelatedItemUserControl.LastController);
				AssertEquals(ControllerIDs.Project, form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.ControllerID);
				form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.Dispose();
			}
		}

		public void TestOpenRelatedItem_ItemSourceHasChanges()
		{
			var project = Factory.NewWithValidTestData<Project>();
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.RelatedItems.Add(project);
			Factory.Save();

			workItem.WKI_Summary = "changed description";
			using (var form = GetFormForTest(workItem, false))
			{
				form.Show();
				Application.DoEvents();

				form.UserControl.RelatedItemGrid.ListManager.Position = 0;
				form.UserControl.RelatedItemGrid.Select(0);
				form.UserControl.EditButton.PerformClick();
				AssertNotNull(form.WorkTaskRelatedItemUserControl.LastController);
				AssertEquals(ControllerIDs.Project, form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.ControllerID);
				form.WorkTaskRelatedItemUserControl.LastController.LastShownForm.Dispose();
				AssertNullOrEmpty("Should allow opening related items even when there are unsaved changes", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNewRelatedItem()
		{
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_Summary = "Testing";
			project.WKP_ProjectNumber = "";
			Factory.Save();

			ZForm newItemForm;
			using (var form = GetFormForTest(project, false))
			{
				form.Show();

				Assert("button clicked", form.UserControl.NewButton_Click("Work Item"));
				newItemForm = (ZForm)form.WorkTaskRelatedItemUserControl.LastController.LastShownForm;
			}

			using (newItemForm)
			{
				newItemForm.BusinessEntity.Factory.Save();
			}
		}

		public void TestShouldDisplayRelatedItems_ForBWFWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BWF");
			AssertRelatedItemGroupBoxVisible(true);
		}

		public void TestShouldDisplayRelatedItems_ForEWFWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("EWF");
			AssertRelatedItemGroupBoxVisible(true);
		}

		public void TestShouldDisplayRelatedItems_ForBUFWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BUF");
			AssertRelatedItemGroupBoxVisible(true);
		}

		public void TestShouldDisplayRelatedItems_ForPLNWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("PLN");
			AssertRelatedItemGroupBoxVisible(true);
		}

		void AssertRelatedItemGroupBoxVisible(bool expectedVisible)
		{
			var workItem = Factory.NewWithValidTestData<WorkItem>();
			Factory.Save();

			using (var form = GetFormForTest(workItem, false))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(expectedVisible, form.UserControl.RelatedItemGroupBox.Visible);
			}
		}

		#endregion

		#region Network Diagrams

		public void TestShouldNotDisplayNetworkDiagrams_ForBWFWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BWF");
			helper.CreateSystem(Factory, "WKI");
			AssertNetworkDiagramGroupBoxVisible<WorkItem>(false);
		}

		public void TestShouldNotDisplayNetworkDiagrams_ForEWFWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("EWF");
			helper.CreateSystem(Factory, "WKI");
			AssertNetworkDiagramGroupBoxVisible<WorkItem>(false);
		}

		public void TestShouldNotDisplayNetworkDiagrams_ForBUFWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BUF");
			helper.CreateSystem(Factory, "WKI");
			AssertNetworkDiagramGroupBoxVisible<WorkItem>(false);
		}

		public void TestShouldDisplayNetworkDiagrams_ForPLNWorkflowManagementMode_WhenJobIsRegisteredForBMS()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("PLN");
			helper.CreateSystem(Factory, "WKI");
			AssertNetworkDiagramGroupBoxVisible<WorkItem>(true);
		}

		public void TestShouldNotDisplayNetworkDiagrams_ForPLNWorkflowManagementMode_WhenJobIsNotRegisteredForBMS()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("PLN");
			AssertNetworkDiagramGroupBoxVisible<WorkItem>(false);
		}

		void AssertNetworkDiagramGroupBoxVisible<T>(bool expectedVisible) where T : BusinessObject
		{
			var job = Factory.NewWithValidTestData<T>();
			Factory.Save();

			using (var form = GetFormForTest(job, false))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(expectedVisible, form.UserControl.NetworkDiagramGroupBox.Visible);
			}
		}

		#endregion

		#region Parent And Child Workflow Relationships Grids

		public void TestShouldNotDisplayParentAndChildGrids_ForBWFWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BWF");
			helper.CreateSystem(Factory, "WKI");
			AssertParentAndChildGroupBoxesVisible<WorkItem>(false);
		}

		public void TestShouldNotDisplayParentAndChildGrids_ForEWFWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("EWF");
			helper.CreateSystem(Factory, "WKI");
			AssertParentAndChildGroupBoxesVisible<WorkItem>(false);
		}

		public void TestShouldNotDisplayParentAndChildGrids_ForBUFWorkflowManagementMode()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("BUF");
			helper.CreateSystem(Factory, "WKI");
			AssertParentAndChildGroupBoxesVisible<WorkItem>(false);
		}

		public void TestShouldDisplayParentAndChildGrids_ForPLNWorkflowManagementMode_WhenJobIsRegisteredForBMS()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("PLN");
			helper.CreateSystem(Factory, "WKI");
			AssertParentAndChildGroupBoxesVisible<WorkItem>(true);
		}

		public void TestShouldNotDisplayParentAndChildGrids_ForPLNWorkflowManagementMode_WhenJobIsNotRegisteredForBMS()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.SetWorkflowManagementModeInRegistry("PLN");
			AssertParentAndChildGroupBoxesVisible<WorkItem>(false);
		}

		void AssertParentAndChildGroupBoxesVisible<T>(bool expectedVisible) where T : BusinessObject
		{
			var job = Factory.NewWithValidTestData<T>();
			Factory.Save();

			using (var form = GetFormForTest(job, false))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(expectedVisible, form.UserControl.ParentWorkflowGroupBox.Visible);
				AssertEquals(expectedVisible, form.UserControl.ChildWorkflowGroupBox.Visible);
			}
		}

		#endregion

		protected virtual FormForTest GetFormForTest(object workItem, bool isViewOrDeleteMode) => new FormForTest(workItem, isViewOrDeleteMode);
	}

	#region NoGUI Test

	public class WorkTaskRelatedItemCollectionNonTransactionedTest : TestCase
	{
		public void TestAddingToCollectionInNonUIMode_DoesNotShowModal()
		{
			var factory = new BusinessObjectFactory();
			var relatedItem = factory.NewWithValidTestData<Project>();
			var workItem = factory.NewWithValidTestData<WorkItem>();
			var collection = new WorkTaskRelatedItemGenPivotCollection<Project>(factory.NewWithValidTestData<Project>());

			AssertEquals(0, collection.Count);

			Globals.IsUserInteractive = false;
			collection.Add(relatedItem);
			collection.Add(workItem);
			var form = ZFormModaliser.LastFormShownDialogForTest;

			AssertNull("No modal dialog should be shown", form);
		}
	}

	#endregion
}
