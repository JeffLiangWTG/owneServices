
namespace Enterprise.MasterFiles.GUI
{
	using System;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.GUI;

	public sealed class GridPopupWorkflow<T> : IDisposable where T : class, IWorkflowProvider
	{
		public GridPopupWorkflow(ZGrid grid, Func<T, ZString> getReference)
		{
			Argument.NotNull(grid, "grid");
			Argument.NotNull(getReference, "getReference");

			this.grid = grid;
			this.getReference = getReference;

			if (!DesignModeFinder.IsDesigning)
			{
				workflowMenuItem = new ZMenuItem(OpenWorkflowMenuItemText, WorkflowMenuItemClick);
				workflowMenuItem.Enabled = false;

				grid.ContextMenu.MenuItems.Add("-");
				grid.ContextMenu.MenuItems.Add(workflowMenuItem);
				grid.ContextMenu.Popup += ContextMenuPopup;
			}
		}

		public void Dispose()
		{
			if (grid.ContextMenu != null)
			{
				grid.ContextMenu.Popup -= ContextMenuPopup;
			}

			var menuItem = workflowMenuItem;
			if (menuItem != null)
			{
				if (grid.ContextMenu != null)
				{
					grid.ContextMenu.MenuItems.Remove(menuItem);
				}

				menuItem.Dispose();
			}

			var form = workflowForm;
			if (form != null)
			{
				form.Dispose();
			}
		}

		void WorkflowMenuItemClick(object sender, EventArgs e)
		{
			ShowWorkflow();
		}

		void ContextMenuPopup(object sender, EventArgs e)
		{
			var selectedLine = SelectedLine;
			var enabled = selectedLine != null && selectedLine.IsInDatabase;

			workflowMenuItem.Enabled = enabled;
			workflowMenuItem.Caption = enabled ? OpenWorkflowMenuItemText : WorkflowNotSavedMenuItemText;
		}

		void ShowWorkflow()
		{
			var selectedLine = SelectedLine;
			if (selectedLine != null && selectedLine.IsInDatabase)
			{
				var factory = new BusinessObjectFactory();
				var entity = factory.Load<T>(selectedLine.PK);
				if (entity != null)
				{
					var parentForm = (ZForm)grid.FindForm();

					if (workflowForm != null)
					{
						workflowForm.Dispose();
					}

					workflowForm = new GridPopupWorkflowForm(entity, getReference(entity));
					workflowForm.ControllerID = parentForm.ControllerID;

					ZFormModaliser.Show(workflowForm, parentForm);
				}
			}
		}

		BusinessObject SelectedLine
		{
			get
			{
				var listManager = grid.ListManager;
				var line = listManager != null ? listManager.GetCurrent() : null;

				return line as BusinessObject;
			}
		}

		static MultilingualString OpenWorkflowMenuItemText
		{
			get { return ResString.GetMultilingualString("b6c237f4-40fc-4842-8ce8-c8e51833d48e", "Open Workflow"); }
		}

		static MultilingualString WorkflowNotSavedMenuItemText
		{
			get { return ResString.GetMultilingualString("eba473b3-18c1-42b2-b7cf-18e974c78204", "Please save before opening the workflow"); }
		}

		readonly Func<T, ZString> getReference;
		readonly ZGrid grid;
		readonly ZMenuItem workflowMenuItem;
		GridPopupWorkflowForm workflowForm;
	}
}
