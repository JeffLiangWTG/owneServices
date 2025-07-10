using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public static class WorkflowParentFormFactory
	{
		public static IZForm ShowFormAndNavigateToWorkflowItem(IBusiness businessObject, Func<ZController> getTargetController = null, bool isEditAllowed = true, bool alwaysShowTaskForm = false, bool shouldShowTaskFormIfNullParent = false, bool shouldNavigateToWorkflowIfNullParent = false)
		{
			Argument.NotNull(businessObject, nameof(businessObject));

			if (businessObject is BusinessObject bizo && bizo.IsDeleted)
			{
				return null;
			}

			if (businessObject is IProcessHeader processHeader)
			{
				return ShowFormAndNavigateToWorkflowItem(processHeader, getTargetController, isEditAllowed);
			}

			if (businessObject is ProcessTask task)
			{
				if (alwaysShowTaskForm || task.IsStandaloneTask || (shouldShowTaskFormIfNullParent && task.Parent == null))
				{
					var controller = ZControllerFactory.Create(ControllerIDs.ProcessTasks);
					var form = isEditAllowed ? controller.ShowEditForm(task) : controller.ShowViewForm(task);

					if (shouldNavigateToWorkflowIfNullParent && task.Parent == null && task.ProcessHeader != null)
					{
						NavigateToWorkflowItem(form, task.ProcessHeader);
					}

					return form;
				}
				else
				{
					return ShowFormAndNavigateToWorkflowItem(GetTaskController(task), getTargetController, task.Parent, task, null, isEditAllowed);
				}
			}

			if (businessObject is IWorkflowProvider workflowProvider)
			{
				task = (ProcessTask)workflowProvider.WorkflowItems.Tasks.FirstOrDefault();

				return ShowFormAndNavigateToWorkflowItem(null, getTargetController, workflowProvider, task, null, isEditAllowed);
			}

			throw new ArgumentException(FormattableString.Invariant($"BusinessObject type {businessObject.GetType()} is not supported."));
		}

		static ZController GetTaskController(ProcessTask task)
		{
			if (task.IsException)
			{
				return ZControllerFactory.Create(ControllerIDs.WorkflowExceptions);
			}
			else if (task.IsWorkflowTrigger)
			{
				return ZControllerFactory.Create(ControllerIDs.WorkflowTriggers);
			}
			else if (task.IsMilestone())
			{
				return ZControllerFactory.Create(ControllerIDs.WorkflowMilestones);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.ProcessTasks);
			}
		}

		static IZForm ShowFormAndNavigateToWorkflowItem(IProcessHeader processHeader, Func<ZController> getTargetController, bool isEditAllowed)
		{
			var task = processHeader is IProcessJobHeader ? null : processHeader.Tasks.OfType<ProcessTask>().MinBySafe(x => x.P9_Sequence);
			var workflowProvider = task == null ? (IWorkflowProvider)processHeader.Parent : task.Parent;

			if (task != null && task.IsDeleted)
			{
				task = null;
			}

			return ShowFormAndNavigateToWorkflowItem(ZControllerFactory.Create(ControllerIDs.ProcessHeader), getTargetController, workflowProvider, task, processHeader, isEditAllowed);
		}

		static IZForm ShowFormAndNavigateToWorkflowItem(ZController sourceController, Func<ZController> getTargetController, IWorkflowProvider workflowProvider, ProcessTask task, IProcessHeader workflow, bool isEditAllowed)
		{
			if (workflowProvider != null)
			{
				IZForm form = null;
				var controller = getTargetController?.Invoke() ?? WorkflowProviderHelper.GetControllerForWorkflowType(workflowProvider.WorkflowType);
				var bizoToShowFormFor = workflowProvider as BusinessObject;

				if (controller == null && ((task != null) && (task.ParentControllerID != null) && task.ParentControllerID.Equals(ControllerIDs.ProcessTemplates)))
				{
					controller = ZControllerFactory.Create(task.ParentControllerID);
				}

				if (controller == null)
				{
					ReportIfUnableToShowForm((NoResString)"Unable to show the form due to null controller.", workflowProvider, task);
					return null;
				}

				if (bizoToShowFormFor != null)
				{
					if (isEditAllowed)
					{
						form = controller.ShowEditForm(bizoToShowFormFor);
					}
					else
					{
						form = controller.ShowViewForm(bizoToShowFormFor);
					}

					if (form != null)
					{
						if (task != null)
						{
							NavigateToWorkflowItem(form, task);
						}
						else if (workflow != null)
						{
							NavigateToWorkflowItem(form, workflow);
						}
						if (sourceController != null)
						{
							AddLinkToRecent(sourceController, task, workflow);
						}
					}
				}

				return form;
			}
			else if (task != null)
			{
				var notify = new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("f2891083-2c13-4a3d-8fbb-a80675aae40d", "Process task parent no longer exist."));
				Globals.Message.ShowError(notify.Message);
			}
			else
			{
				ReportIfUnableToShowForm((NoResString)"Unable to show the form.", workflowProvider, task);
			}

			return null;
		}

		static void AddLinkToRecent(ZController sourceController, ProcessTask task, IProcessHeader workflow)
		{
			try
			{
				var sourceEntity = (BusinessObject)workflow ?? task;
				var identifier = sourceEntity.PK.ToGuid();
				var moduleName = sourceController.ModuleID.Name;
				var linkWrapper = new LinkWrapper(moduleName, identifier, ShowEditFormUrlHandler.Instance.Create(sourceController.ID, identifier), sourceEntity.HumanReadableShortcutName);
				RecentItemManager.Instance.AddOrUpdateRecentItems(moduleName, linkWrapper);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// I don't expect this to ever happen, but I don't want to break the exceptions module if it does.
				ErrorReporter.ReportOnce("Could not add Exception to recent items.", ex);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is for developer exception, this is for developer exception;")]
		static void ReportIfUnableToShowForm(string notificationText, IWorkflowProvider workflowProvider, ProcessTask task)
		{
			if (workflowProvider == null || new BusinessObjectFactory().Load(workflowProvider.GetType(), workflowProvider.PK) != null)
			{
				var key = "WorkflowFormFactoryError" + workflowProvider?.WorkflowType ?? "NoWorkflowType" + notificationText;
				var taskDetailsMessage = task != null ? (NoResString)" Task details: " + task.GetDiagnosticLogInfo() + (NoResString)"." : (NoResString)" Task is null.";
				var workflowProviderMessage = workflowProvider == null ? " WorkflowProvider is null." : " WorkflowProvider: " + workflowProvider + ", WorkflowType: " + workflowProvider.WorkflowType + ".";

				ErrorReporter.ReportOnce(key, notificationText + taskDetailsMessage + workflowProviderMessage);
			}
		}

		public static void NavigateToWorkflowItem(IZForm form, ProcessTask task)
		{
			if (form != null && CheckFormIsInvocable(form) && !ExecuteCustomNavigationLogicIfImplemented(form, task))
			{
				var tabPage = GetWorkflowTab(form);

				if (tabPage != null)
				{
					((Control)form).BeginInvoke(new Action(() => tabPage.NavigateToWorkflowItem(task)));
				}
			}
		}

		public static void NavigateToWorkflowItem(IZForm form, IProcessHeader workflow)
		{
			if (form != null && CheckFormIsInvocable(form))
			{
				if (form is TaskManagementForm taskForm)
				{
					NavigateToTaskManagementFormTabPage(taskForm);
				}
				else
				{
					var tabPage = GetWorkflowTab(form);

					if (tabPage != null)
					{
						((Control)form).BeginInvoke(new Action(() => tabPage.NavigateToWorkflowItem(workflow)));
					}
				}
			}
		}

		static void NavigateToTaskManagementFormTabPage(TaskManagementForm taskForm)
		{
			var tabPage = taskForm.FindAll<ZTabPage>().SingleOrDefault(t => t.Name == "WorkflowDetailsTabPage");

			if (tabPage != null)
			{
				var tabControl = tabPage.Parent as ZTabControl;
				if (tabControl != null)
				{
					tabControl.SelectedTab = tabPage;
				}
			}
		}

		static ZWorkflowTabPage GetWorkflowTab(IZForm form)
		{
			var tabs = ((Control)form).FindAll<ZWorkflowTabPage>().ToArray();

			if (tabs.Length > 1)
			{
				ErrorReporter.ReportOnce("WorkflowParentFormFactory.GetWorkflowTab.MultipleWorkflowTabs",
					string.Format(CultureInfo.InvariantCulture, "Tried to navigate to a workflow item on a form that has multiple ZWorkflowTabPages. Such a form must implement IWorkflowItemNavigationOverridable, see OrganisationForm.cs for an example. Form type: {0}", form.GetType()));
				return null;
			}

			return tabs.SingleOrDefault();
		}

		static bool ExecuteCustomNavigationLogicIfImplemented(IZForm form, ProcessTask task)
		{
			var implementingForm = form as IWorkflowTaskNavigationOverridable;

			if (implementingForm != null)
			{
				((Control)form).BeginInvoke(new Action(() => implementingForm.NavigateToWorkflowItem(task)));
				return true;
			}

			return false;
		}

		static bool CheckFormIsInvocable(IZForm form)
		{
			var control = (Control)form;
			return !control.IsDisposed && control.IsHandleCreated;
		}
	}
}
