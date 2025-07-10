using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public abstract class GridPopupWorkflowTest : TestCaseWithFactory
	{
		public void TestBookingLineContextMenu_FormIsNotSaved_DisableOpenWorkflowMenuItem()
		{
			using (var form = GetNewForm())
			{
				form.Show();
				Application.DoEvents();

				var grid = GetGrid(form);
				grid.Select(0);
				grid.ContextMenu.DoPopup();

				var openWorkflowMenuItem = grid.ContextMenu.MenuItems.FindByText("Please save before opening the workflow");
				AssertNotNull("menu", openWorkflowMenuItem);
				AssertEquals(false, openWorkflowMenuItem.Enabled);
			}
		}

		public void TestBookingLineContextMenu_FormIsSaved_EnableOpenWorkflowMenuItem()
		{
			using (var form = GetNewForm())
			{
				Factory.Save();
				form.Show();
				Application.DoEvents();

				var grid = GetGrid(form);
				grid.Select(0);
				grid.ContextMenu.DoPopup();

				var openWorkflowMenuItem = grid.ContextMenu.MenuItems.FindByText("Open Workflow");
				AssertNotNull("menu", openWorkflowMenuItem);
				AssertEquals(true, openWorkflowMenuItem.Enabled);
			}
		}

		public void TestBookingLineContextMenu_OpenWorkflowMenuItemClick_OpenWorkflow()
		{
			using (var form = GetNewForm())
			{
				Factory.Save();
				form.Show();
				Application.DoEvents();

				var grid = GetGrid(form);
				grid.Select(0);
				grid.ContextMenu.DoPopup();

				var openWorkflowMenuItem = grid.ContextMenu.MenuItems.FindByText("Open Workflow");
				openWorkflowMenuItem.PerformClick();

				var lastShownForm = ZFormModaliser.LastFormShownForTest as GridPopupWorkflowForm;
				AssertNotNull(lastShownForm);
				AssertEquals(ODisplayMode.Browse, lastShownForm.DisplayMode);

				lastShownForm.Dispose();
			}
		}

		protected abstract ZForm GetNewForm();
		protected abstract ZGrid GetGrid(ZForm form);
	}
}
