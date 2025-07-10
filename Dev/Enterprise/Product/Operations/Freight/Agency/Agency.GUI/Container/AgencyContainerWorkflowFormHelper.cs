using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	class AgencyContainerWorkflowFormHelper : IDisposable
	{
		public AgencyContainerWorkflowFormHelper(ZGrid grid)
		{
			this.grid = Argument.NotNull(grid, "grid");
		}

		public void Hook()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				workflowMenuItem = new ZMenuItem(WorkflowNotSavedMenuItemText, WorkflowMenuItem_Click);
				workflowMenuItem.Enabled = false;
				grid.ContextMenu.MenuItems.Add("-");
				grid.ContextMenu.MenuItems.Add(workflowMenuItem);
				grid.ContextMenu.Popup += ContextMenu_Popup;
			}
		}

		public void UnHook()
		{
			if (grid.ContextMenu != null)
			{
				grid.ContextMenu.Popup -= ContextMenu_Popup;
			}
		}

		public void Dispose()
		{
			if (workflowMenuItem != null)
			{
				UnHook();
				workflowMenuItem.Dispose();
			}
		}

		#region Show Workflow

		void WorkflowMenuItem_Click(object sender, EventArgs e)
		{
			ShowWorkflow();
		}

		void ShowWorkflow()
		{
			var selectedEntity = SelectedContainer;
			if (selectedEntity != null && selectedEntity.IsInDatabase && !selectedEntity.HasChanges)
			{
				var parentForm = (ZForm)grid.FindForm();
				var form = new AgencyContainerWorkflowForm((AgencyShipmentContainer)new BusinessObjectFactory().Load(selectedEntity.GetType(), selectedEntity.PK));
				form.ControllerID = parentForm.ControllerID;
				ZFormModaliser.Show(form, grid.FindForm());
			}
		}

		#endregion

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			UpdateContextMenuItems();
		}

		void UpdateContextMenuItems()
		{
			bool enabled = SelectedContainer != null && SelectedContainer.IsInDatabase && !SelectedContainer.HasChanges;
			workflowMenuItem.Enabled = enabled;
			workflowMenuItem.Caption = enabled ? OpenWorkflowMenuItemText : WorkflowNotSavedMenuItemText;
		}

		AgencyShipmentContainer SelectedContainer
		{
			get
			{
				var listManager = grid.ListManager;
				return (AgencyShipmentContainer)(listManager != null ? listManager.GetCurrent() : null);
			}
		}

		static MultilingualString OpenWorkflowMenuItemText
		{
			get { return ResString.GetMultilingualString("9e4c6445-9190-45ed-a37d-cb27618d8556", "Open Workflow"); }
		}

		static MultilingualString WorkflowNotSavedMenuItemText
		{
			get { return ResString.GetMultilingualString("7bdcbd2d-d6da-497c-8387-cad504e4b091", "Please save before opening the Container"); }
		}

		ZMenuItem workflowMenuItem;
		readonly ZGrid grid;
	}
}
