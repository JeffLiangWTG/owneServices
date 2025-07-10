using System;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Workflow;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	static class WorkflowDiagnosticContextMenuManager
	{
		internal static void AttachToTriggerGrid(ZGrid grid)
		{
			AttachMenuItemToGrid(grid,
				WorkflowDiagnosticCaption,
				(sender, eventArgs) =>
				{
					if (grid.ListManager.GetCurrent() is ProcessTask task)
					{
						WorkflowDiagnosticUtilities.ShowWorkflowDiagnosisForm(task, grid.FindForm());
					}
				});

			AttachToTaskGrid(grid);
		}

		internal static void AttachToTaskGrid(ZGrid grid)
		{
			AttachMenuItemToGrid(grid,
				WorkflowTemplateMatchCaption,
				(sender, eventArgs) =>
				{
					if (grid.List is IWorkflowItemCollection collection)
					{
						WorkflowDiagnosticUtilities.ShowMatchingTemplateForm(collection);
					}
				});
		}

		static void AttachMenuItemToGrid(ZGrid grid, MultilingualString caption, EventHandler onClick, Func<ProcessTask, bool> enabledPredicate = null)
		{
			var menuItem = new ZMenuItem(caption);
			try
			{
				menuItem.Click += onClick;
				var contextMenu = grid.ContextMenu;
				contextMenu.MenuItems.Add(grid.ContextMenu.MenuItems.Count - 1, menuItem);
				contextMenu.Popup += (sender, eventArgs) =>
				{
					if (grid.ListManager.Position >= 0)
					{
						var task = grid.ListManager.GetCurrent() as ProcessTask;
						menuItem.Visible = CanShow(task);
						menuItem.Enabled = task != null && (enabledPredicate == null || enabledPredicate(task));
					}
				};
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				menuItem.Dispose();
				throw;
			}
		}

		internal static bool CanShow(ProcessTask task)
		{
			return task != null && !(task is TemplateProcessTask);
		}

		internal static MultilingualString WorkflowDiagnosticCaption => ResString.GetMultilingualString("1EE27601-825D-47A9-B654-98F790C6B6B6", "Workflow Diagnostic");

		internal static MultilingualString WorkflowTemplateMatchCaption => ResString.GetMultilingualString("1EE27601-825D-47A9-B654-98F790C6B6B7", "Workflow Template Matches");
	}
}
