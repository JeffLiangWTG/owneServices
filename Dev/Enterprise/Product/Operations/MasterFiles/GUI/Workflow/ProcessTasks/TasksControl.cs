using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Workflow.ProcessTasks;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis]
	public partial class TasksControl : ZUserControl, IBindTo, IWorkflowTasksControl
	{
		public TasksControl()
		{
			InitializeComponent();

			TasksGrid.ColourDeciding += new EventHandler<ColourDecidingEventArgs>(TasksGrid_ColourDeciding);
			TasksGrid.ContextMenu.Popup += new EventHandler(OnContextMenu_Popup);
			WorkflowDiagnosticContextMenuManager.AttachToTaskGrid(TasksGrid);

			var processTaskStatusChangeMode = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();

			TasksGrid.Entering += (_, __) => processTaskStatusChangeMode.SetCurrent(ProcessTaskStatusChangeModeCodeList.Codes.TasksTabGrid);
			TasksGrid.Leave += (_, __) => processTaskStatusChangeMode.Clear();
		}

		#region Context Menu Item

		void OnContextMenu_Popup(object sender, EventArgs e)
		{
			ContextMenu menu = (ContextMenu)sender;
			if (cloneTaskMenuItem == null)
			{
				AddCloneTaskMenuItemIfSecurityAllows(menu);
			}

			RefreshAssistMenuItemsIfApplicable(menu);
		}

		protected virtual void AddCloneTaskMenuItemIfSecurityAllows(ContextMenu menu)
		{
			if (ShouldAddTaskMenuItems())
			{
				cloneTaskMenuItem = new TaskCloneMenuItem(TasksGrid, () => Tasks, Shortcut.CtrlShiftK);
				if (!menu.MenuItems.Contains(cloneTaskMenuItem))
				{
					menu.MenuItems.Add(cloneTaskMenuItem);
				}
			}
		}

		void RefreshAssistMenuItemsIfApplicable(ContextMenu menu)
		{
			if (ShouldAddTaskMenuItems())
			{
				assistWithThisTaskMenuItem = RemoveAssistMenuItemIfItExists(assistWithThisTaskMenuItem, menu);
				addAssistanceTaskForMenuItem = RemoveAssistMenuItemIfItExists(addAssistanceTaskForMenuItem, menu);

				var selectedTask = AssistWithThisTaskMenuItem.GetSingleSelectedTask(TasksGrid);

				if (AssistWithThisTaskMenuItem.ShouldAddMenuItem(selectedTask))
				{
					assistWithThisTaskMenuItem = new AssistWithThisTaskMenuItem(selectedTask, () => Tasks);
					menu.MenuItems.Add(assistWithThisTaskMenuItem);
				}

				if (AddAssistanceTaskForMenuItem.ShouldAddMenuItem(selectedTask))
				{
					addAssistanceTaskForMenuItem = new AddAssistanceTaskForMenuItem(selectedTask,
						() => AssistWithThisTaskHelper.GetStaffCodesAndDescriptionsForAssistanceTask(selectedTask as ProcessTask, Tasks),
						() => AssistWithThisTaskHelper.GetCapabilityCodesAndDescriptionsForAssistanceTask(selectedTask as ProcessTask, Tasks),
						() => Tasks);
					menu.MenuItems.Add(addAssistanceTaskForMenuItem);
				}
			}
		}

		MenuItem RemoveAssistMenuItemIfItExists(MenuItem menuItem, ContextMenu menu)
		{
			if (menuItem != null)
			{
				if (menu.MenuItems.Contains(menuItem))
				{
					menu.MenuItems.Remove(menuItem);
				}

				return null;
			}

			return menuItem;
		}

		bool ShouldAddTaskMenuItems()
		{
			return Tasks != null && !Tasks.ReadOnly && Tasks.Tasks.AllowNew;
		}

		internal MenuItem cloneTaskMenuItem;
		MenuItem assistWithThisTaskMenuItem;
		MenuItem addAssistanceTaskForMenuItem;

		#endregion

		#region GUI Setup

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool CreateTasksFromTemplateLinkVisible
		{
			get { return TasksTemplateLinkLabel.Visible; }
			set { TasksTemplateLinkLabel.Visible = value; }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var workflowItems = GetWorkflowItems(dataSource, dataMember);
			var newTasks = ((IWorkflowProvider)workflowItems)?.WorkflowItems;
			RemoveColumnsIfRequired(newTasks);

			dataBindingDisposable?.Dispose();

			base.SetDataBinding(dataSource, dataMember);

			if (newTasks != null)
			{
				dataBindingDisposable = this.SetupTasksControl(workflowItems, saveOrder: true);
				var factory = newTasks.Factory;
				factory.ClearCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver));
				factory.GetCachedValue<ITaskStatusChangeConflictResolver>(nameof(ITaskStatusChangeConflictResolver), () => new TaskStatusChangeConflictResolver(), CacheStalenessPolicy.NeverStale);
			}
		}

		IDisposable dataBindingDisposable;

		void RemoveColumnsIfRequired(ProcessTaskCollection newTasks)
		{
			if (newTasks != null)
			{
				if (!newTasks.SupportsContactAndAddress)
				{
					RemoveColumn(ProcessTasksSchema.P9_OA.Name);
					RemoveColumn(ProcessTasksSchema.P9_OC.Name);
				}

				if (newTasks is TemplateProcessTaskCollection)
				{
					RemoveColumn(ProcessTasksSchema.P9_TaskID.Name);
					RemoveColumn(ProcessTasksSchema.P9_ScheduledDate.Name);
					RemoveColumn(ProcessTasksSchema.P9_ActualDate.Name);
					RemoveColumn(ProcessTasksSchema.P9_ActualDuration.Name);
					RemoveColumn(nameof(ProcessTask.ElapsedDuration));
					RemoveColumn(nameof(ProcessTask.HasNote));
					RemoveColumn(nameof(ProcessTask.SuspendedDuration));
					RemoveColumn(nameof(ProcessTask.Iteration));
					RemoveColumn(nameof(ProcessTask.TimeBecameStartable));
				}
				else
				{
					var templateConditionsPrefix = nameof(ProcessTask.TemplateConditions) + "+";

					RemoveColumn(templateConditionsPrefix + TemplateConditionsViewModel.Schema.TemplateCondition1);
					RemoveColumn(templateConditionsPrefix + TemplateConditionsViewModel.Schema.TemplateCondition2);
					RemoveColumn(templateConditionsPrefix + TemplateConditionsViewModel.Schema.TemplateCondition2Value);
					RemoveColumn(templateConditionsPrefix + TemplateConditionsViewModel.Schema.OriginCountryCode);
					RemoveColumn(templateConditionsPrefix + TemplateConditionsViewModel.Schema.DestinationCountryCode);
					RemoveColumn(ProcessTasksSchema.P9_RN_NKOriginCountry.Name);
					RemoveColumn(ProcessTasksSchema.P9_RN_NKDestinationCountry.Name);
					RemoveColumn(ProcessTasksSchema.P9_EstimatedDefaultedFrom.Name);
					RemoveColumn(ProcessTasksSchema.P9_EstimatedDefaultTimeDelta.Name);

					var triggerConditionsPrefix = nameof(ProcessTask.TriggerConditions) + "+";
					RemoveColumn(triggerConditionsPrefix + nameof(TriggerConditionsViewModel.TriggerContextCode));
					RemoveColumn(triggerConditionsPrefix + nameof(TriggerConditionsViewModel.TriggerCompany));
				}
			}
		}

		void RemoveColumn(ZString columnName)
		{
			if (TasksGrid.Columns.Contains(columnName))
			{
				TasksGrid.Columns.Remove(columnName);
			}
			ZGridColumnInfo[] columnStyleArray = (ZGridColumnInfo[])TasksGrid.ColumnStyles.ToArray(typeof(ZGridColumnInfo));
			ZGridColumnInfo columnStyle = Array.Find(columnStyleArray, column => column.ColumnName == columnName);
			if (columnStyle != null)
			{
				TasksGrid.ColumnStyles.Remove(columnStyle);
			}

			if (TasksGrid.TableStyles.Count > 0 && TasksGrid.TableStyles[0].GridColumnStyles.Contains(columnName))
			{
				DataGridColumnStyle colStyle = TasksGrid.TableStyles[0].GridColumnStyles[columnName];

				if (TasksGrid.ColumnStyles.Contains(colStyle))
				{
					TasksGrid.ColumnStyles.Remove(colStyle);
				}

				if (TasksGrid.TableStyles[0].GridColumnStyles.Contains(colStyle))
				{
					TasksGrid.TableStyles[0].GridColumnStyles.Remove(colStyle);
				}
			}
		}

		internal void TasksGrid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			ProcessTask task = (ProcessTask)e.ObjectAtRow;
			if (task.IsBehindSchedule)
			{
				e.Colour = Color.LightSalmon;
			}
			else if (task.IsSuspended)
			{
				e.Colour = Color.Yellow;
			}
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			HideIrrelevantColumnsIfBufferManagementDisabled();
		}

		void HideIrrelevantColumnsIfBufferManagementDisabled()
		{
			var workflowProvider = Tasks?.Parent as IWorkflowProvider;
			var hideIrrelevantColumns = workflowProvider == null ||
				!ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(workflowProvider, ((BusinessObject)workflowProvider).Factory);

			if (hideIrrelevantColumns)
			{
				TasksGrid.RemoveFromAvailableColumns("TimeBecameStartable");
			}
		}

		#endregion

		#region Tasks

		internal ProcessTaskCollection Tasks
		{
			get { return GetTasks(BindingSource.DataSource, BindingSource.DataMember); }
		}

		IWorkflowProviderCollection GetWorkflowItems(object dataSource, string dataMember)
		{
			CurrencyManager cm = dataSource == null ? null : (CurrencyManager)BindingContext[dataSource, new KBindingMemberInfo(dataMember, BindTo).BindingMember];
			return cm?.List as IWorkflowProviderCollection;
		}

		ProcessTaskCollection GetTasks(object dataSource, string dataMember) => GetWorkflowItems(dataSource, dataMember)?.WorkflowItems;

		#endregion

		#region BindTo

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[DefaultValue("")]
		public string BindTo
		{
			get { return TasksGrid.BindTo; }
			set { TasksGrid.BindTo = value; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				dataBindingDisposable?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Task Details Form

#if DEBUG
		internal ZController LastController;
#endif

		internal void TasksGrid_DoubleClick(object sender, EventArgs eventArgs)
		{
			var openformResult = GridEntityFormOpener.OpenFormForSavedParent((ZForm)ParentForm, TasksGrid, eventArgs as MouseEventArgs, ControllerIDs.ProcessTasks);
#if DEBUG
			LastController = openformResult.Controller;
#endif
		}

		#endregion

		#region Template Tasks

		internal void TasksTemplateLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var tasks = Tasks;

			if (tasks != null)
			{
				var workflowProvider = (IWorkflowProvider)tasks.Parent;

				if (ProcessTask.Loader.ShouldCreateTasksFromTemplate(tasks.Parent, workflowProvider))
				{
					var startingTaskCount = tasks.Tasks.Count;

					workflowProvider.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.Tasks, jobAttributesMayHaveChangedSinceLastTemplateApplication: true));

					if (tasks.Tasks.Count == startingTaskCount)
					{
						Globals.Message.Show(Res.GetString("8d5c8a96-6f65-408d-9fd0-2f7c10bca237", "No Tasks were added from templates for the specified details."), Res.GetString("dee26f9f-4b07-45ad-9911-4c3a51251250", "Task Creation"), MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("bad2a97c-99a6-43ee-882b-d23ba6bf10a1", "You can only create tasks from the template if you have no tasks already entered."), Res.GetString("dee26f9f-4b07-45ad-9911-4c3a51251250", "Task Creation"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		#endregion

		#region ReadOnly

		public void SetControlReadOnly(bool shouldBeReadOnly)
		{
			TasksTemplateLinkLabel.Enabled = !shouldBeReadOnly;
		}

		#endregion

		#region IWorkflowTasksControl Members

		KSplitContainer IWorkflowTasksControl.TasksHintSplitContainer => TasksHintSplitContainer;

		ZLabel IWorkflowTasksControl.TasksHintLabel => TasksHintLabel;

		ZGrid IWorkflowItemsControl.TasksGrid => TasksGrid;

		#endregion
	}
}
