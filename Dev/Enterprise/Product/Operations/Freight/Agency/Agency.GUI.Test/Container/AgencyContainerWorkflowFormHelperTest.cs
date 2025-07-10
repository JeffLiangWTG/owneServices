using System;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class AgencyContainerWorkflowFormHelperTest : TestCase
	{
		public void AssertMenuItems(ZGrid grid, AgencyShipmentContainer container)
		{
			var parentForm = grid.FindForm();
			parentForm.Show();
			grid.UnSelectAll();
			var menu = grid.ContextMenu.MenuItems.FindByText("Please save before opening the Container");
			AssertNotNull("menu", menu);
			AssertEquals(false, menu.Enabled);
			grid.Select(0);
			ShowPopupMenu(grid.ContextMenu);
			menu = grid.ContextMenu.MenuItems.FindByText("Please save before opening the Container");
			AssertNotNull("menu", menu);
			AssertEquals(false, menu.Enabled);
			container.Factory.Save();
			ShowPopupMenu(grid.ContextMenu);
			menu = grid.ContextMenu.MenuItems.FindByText("Open Workflow");
			AssertNotNull("menu", menu);
			AssertEquals(true, menu.Enabled);
			menu.PerformClick();
			var lastShownForm = ZFormModaliser.LastFormShownForTest as AgencyContainerWorkflowForm;
			AssertNotNull(lastShownForm);
			AssertEquals(ODisplayMode.Browse, lastShownForm.DisplayMode);
			lastShownForm.Dispose();
			container.JC_ReleaseNum = "111";
			ShowPopupMenu(grid.ContextMenu);
			menu = grid.ContextMenu.MenuItems.FindByText("Please save before opening the Container");
			AssertNotNull("menu", menu);
			AssertEquals(false, menu.Enabled);
		}

		static void ShowPopupMenu(ContextMenu contextMenu) => contextMenu.GetType().GetMethod("OnPopup", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(contextMenu, new[] { EventArgs.Empty });
	}
}
