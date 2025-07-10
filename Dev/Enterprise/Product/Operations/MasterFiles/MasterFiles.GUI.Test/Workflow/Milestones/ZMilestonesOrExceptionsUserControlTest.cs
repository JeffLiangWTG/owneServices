using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	internal abstract class ZMilestonesOrExceptionsUserControlTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestRelatedMilestonesOrExceptionsColourDecided()
		{
			ProcessTask milestoneOrException = GetCollectionView(Dummy.WorkflowItems).AddNew();
			Dummy.InitRelatedDummyWithTasks();
			ProcessTask relatedMilestoneOrException = GetCollectionView(Dummy.RelatedDummyWithTasks.WorkflowItems).AddNew();
			PropertyInfo gridProperty = typeof(ZMilestonesOrExceptionsUserControl).GetProperty("Grid", BindingFlags.NonPublic | BindingFlags.Instance);
			PropertyInfo colourLegendProperty = typeof(ZMilestonesOrExceptionsUserControl).GetProperty("ColourLegend", BindingFlags.NonPublic | BindingFlags.Instance);
			ZGrid grid = (ZGrid)gridProperty.GetValue(UserControl, null);
			ColourLegend colourLegend = (ColourLegend)colourLegendProperty.GetValue(UserControl, null);

			Form.Show();
			Application.DoEvents();

			Color relatedColor = GetCustomRowBackgroundColour(grid, 0);
			Color color = GetCustomRowBackgroundColour(grid, 1);
			AssertEquals("Normal exception not coloured", Color.Empty, color);
			AssertEquals("Related exception shown in colour", colourLegend.GetColour(((BusinessObject)relatedMilestoneOrException.Parent).PK, ""), relatedColor);
		}

		[RequiresSTA]
		public void TestTooManyRelatedItems_ShowTooManyRelatedItemsHint()
		{
			WorkflowDataRegistry.Instance.RelatedWorkflowItemDisplayLimit.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			var dummiesToCreate = 10;
			var dummies = new List<DummyWithWorkflow>(dummiesToCreate);
			for (int i = 0; i < dummiesToCreate; i++)
			{
				var dummy = Factory.New<DummyWithWorkflow>();
				dummy.GetRelatedWorkflowProviders_ForTest = () => dummies.Except(new[] { dummy });
				dummies.Add(dummy);
			}
			Factory.Save();

			var job = dummies[0];
			using (var form = new ZForm { Size = ControlDpiScalingHelper.NewScaledSize(600, 400) })
			using (var control = GetNewUserControl())
			{
				form.Controls.Add(control);
				form.ControllerID = ControllerIDs.Organisation;
				control.SetDataBinding(GetCollectionIncludingRelatedView(job.WorkflowItems), string.Empty);
				form.Show();
				var workflowTasksControl = (IWorkflowTasksControl)control;

				Application.DoEvents();
				var splitContainer = workflowTasksControl.TasksHintSplitContainer;
				AssertEquals("Some tasks are not shown since they are specific to another company. We should show thie hint label.", false, splitContainer.Panel1Collapsed);
				var label = workflowTasksControl.TasksHintLabel;
				AssertEquals($"Related items are not shown as more than {5} related items were found. (Configurable via {WorkflowDataRegistry.Instance.RelatedWorkflowItemDisplayLimit.HumanReadableRegistryPath()})", label.Text);
			}
		}

		#region Implementation

		Color GetCustomRowBackgroundColour(ZGrid grid, int rowNumber)
		{
			return (Color)typeof(ZGrid).InvokeMember("GetCustomRowBackgroundColour", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, grid, new object[] { rowNumber });
		}

		protected abstract WorkflowItemCollectionIncludingRelatedView GetCollectionIncludingRelatedView(ProcessTaskCollection collection);
		protected abstract WorkflowItemCollectionView GetCollectionView(ProcessTaskCollection collection);

		protected DummyWithWorkflow Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithWorkflow>()); }
		}
		DummyWithWorkflow dummy;

		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZChildForm(GetCollectionIncludingRelatedView(Dummy.WorkflowItems));
					form.ControllerID = DummyControllerIDs.Dummy;
					form.Controls.Add(UserControl);
					form.Size = ControlDpiScalingHelper.NewScaledSize(1000, 700);
					form.CaptionRenderingEnabled = true;
					form.CaptionResourceString = Res.GetData("37AE40FE-BC7F-4EBA-8EBD-F9FDE360C165", "removing this will cause the resstring basher test fail");
				}
				return form;
			}
		}
		ZChildForm form;

		protected ZMilestonesOrExceptionsUserControl UserControl
		{
			get { return userControl ?? (userControl = GetNewUserControl()); }
		}

		ZMilestonesOrExceptionsUserControl userControl;
		protected abstract ZMilestonesOrExceptionsUserControl GetNewUserControl();

		protected override Form GetFormToBashCore()
		{
			return Form;
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		#endregion
	}
}
