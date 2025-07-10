using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaskWithDetailsAndFilterControl : ZUserControl, ITaskDetailsControl, IRequiresWorkflowSecurity
	{
		public TaskWithDetailsAndFilterControl()
		{
			InitializeComponent();
			CreateTasksFromTemplateLinkVisible = true;
			TasksControl.TasksControl.BindTo = "TasksView";
			selectionManager = new GridSelectionManager(TasksGrid, () => ParentForm, true);
			GroupFindbox.AllowOverlap(FilterButton);
			GroupFindbox.AllowOverlap(ClearButton);

			SimpleNotesGroupBox.AllowOutsideOfParent();
			SimpleScheduleGroupBox.AllowOutsideOfParent();
			SimpleDetailsGroupBox.AllowOutsideOfParent();
			GroupFindbox.AllowOutsideOfParent();
		}

		public ZRichTextBox SimpleNotesRichTextBox
		{
			get { return TasksControl.SimpleNotesRichTextBox; }
		}

		public ZGrid TasksGrid
		{
			get { return TasksControl.TasksControl.TasksGrid; }
		}

		public ZTabControl DetailsTabControl => TasksControl.DetailsTabControl;
		protected ZTabPage SimpleViewTabPage => TasksControl.SimpleViewTabPage;
		protected ZGroupBox SimpleDetailsGroupBox => TasksControl.SimpleDetailsGroupBox;
		protected ZGroupBox SimpleNotesGroupBox => TasksControl.SimpleNotesGroupBox;
		protected ZGroupBox SimpleScheduleGroupBox => TasksControl.SimpleScheduleGroupBox;

		internal ProcessTaskCollectionViewFilter TasksView
		{
			get { return (ProcessTaskCollectionViewFilter)BindingSource.DataSource; }
		}

		#region SetDataBinding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (BindingSource.DataSource != dataSource)
			{
				tasksWithFilter = null;
			}
			base.SetDataBinding(dataSource == null ? null : GetTasksWithFilter(dataSource), dataMember);

			IBusinessObjectCollection collection = TasksGrid.List as IBusinessObjectCollection;
			if (collection != null)
			{
				if (((IRequiresWorkflowSecurity)this).ViewOnly)
				{
					collection.IncrementReadOnlyIncludingChildren();
				}
			}
		}

		ProcessTaskCollectionViewFilter GetTasksWithFilter(object dataSource)
		{
			IWorkflowProvider workflowProvider = dataSource as IWorkflowProvider;
			if (tasksWithFilter == null || workflowProvider == null || workflowProvider.WorkflowItems.Tasks != tasksWithFilter.TasksView)
			{
				tasksWithFilter = workflowProvider == null ? null : CreateViewFilter(workflowProvider.WorkflowItems);
			}
			if (tasksWithFilter == null)
			{
				tasksWithFilter = dataSource as ProcessTaskCollectionViewFilter;
			}

			if (workflowProvider != null && ProcessJobHeaderProvider.BufferManagementEnabledForWorkflowProvider(workflowProvider, tasksWithFilter.Factory))
			{
				var tasksWithoutWorkflows = tasksWithFilter.TasksView.Cast<ProcessTask>().Where(x => x.P9_FH_ProcessHeader.IsEmpty).ToArray();

				if (tasksWithoutWorkflows.Length > 0)
				{
					var defaultWorkflow = ProcessJobHeaderProvider.GetDefaultWorkflowForTask(tasksWithoutWorkflows[0]);

					if (defaultWorkflow != null)
					{
						var defaultProcessHeaderPk = defaultWorkflow.PK;

						foreach (var task in tasksWithoutWorkflows)
						{
							task.P9_FH_ProcessHeader = defaultProcessHeaderPk;
						}
					}
				}
			}

			return tasksWithFilter;
		}
		ProcessTaskCollectionViewFilter tasksWithFilter;

		protected virtual ProcessTaskCollectionViewFilter CreateViewFilter(IWorkflowProvider workflowProvider)
		{
			return new ProcessTaskCollectionViewFilter(workflowProvider.WorkflowItems);
		}

		#endregion

		#region GUI Setup and Binding

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool CreateTasksFromTemplateLinkVisible
		{
			get { return TasksControl.TasksControl.CreateTasksFromTemplateLinkVisible; }
			set { TasksControl.TasksControl.CreateTasksFromTemplateLinkVisible = value; }
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("The property name (including path) of the ProcessTaskCollectionView to bind to."), DefaultValue("TasksView")]
		public string BindToGrid
		{
			get { return TasksControl.TasksControl.BindTo; }
			set { TasksControl.TasksControl.BindTo = value; }
		}

		[Browsable(true), Category(ZGUIConstants.DesignerCategory), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Description("The property name (including path) of the ProcessTaskCollectionViewFilter to bind to."), DefaultValue("")]
		public string BindToFilter
		{
			get { return fBindToFilter; }
			set
			{
				UpdateBindTo(fBindToFilter, value, StatusDropEdit, TypeDropEdit, StaffFindbox, GroupFindbox);
				fBindToFilter = value;
			}
		}
		string fBindToFilter = "";

		void UpdateBindTo(string oldPrefix, string newPrefix, params Control[] controls)
		{
			if (oldPrefix != newPrefix)
			{
				foreach (Control control in controls)
				{
					control.SetBindingMember(GetNewBindTo(oldPrefix, newPrefix, control.GetBindingMember()));
					IBindToList controlWithList = control as IBindToList;

					if (controlWithList != null)
					{
						controlWithList.BindToList = GetNewBindTo(oldPrefix, newPrefix, controlWithList.BindToList);
					}
				}
			}
		}

		string GetNewBindTo(string oldPrefix, string newPrefix, string currentBindTo)
		{
			string oldPrefixWithDot = oldPrefix + ".";
			string newPrefixWithDot = newPrefix + ".";
			string suffix = currentBindTo;

			if (suffix.StartsWith(oldPrefixWithDot))
			{
				suffix = suffix.Remove(0, oldPrefixWithDot.Length);
			}

			return newPrefixWithDot + suffix;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (TasksGrid?.ListManager != null)
			{
				TasksGrid.ListManager.ListChanged += selectionManager.ListChanged;
			}
		}

		readonly GridSelectionManager selectionManager;

		#endregion

		#region Filtering

		void FilterButton_Click(object sender, EventArgs e)
		{
			TasksView.FilterTasks();
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			TasksView.ClearTasksFilter();
		}

		#endregion

		#region Navigate to WorkflowItem

		public virtual void NavigateToWorkflowItem(ProcessTask task)
		{
			if (TasksGrid != null && TasksGrid.ListManager != null)
			{
				for (int i = 0; i < this.TasksGrid.ListManager.Count; i++)
				{
					ProcessTask currentTask = (ProcessTask)TasksGrid.List[i];
					if (task.PK == currentTask.PK)
					{
						TasksGrid.ListManager.Position = i;
						break;
					}
				}
			}
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.CheckAndConsumeWorkflowLicenceIfAllowed();
		}

		#region IRequireWorkflowSecurity Members

		string IRequiresWorkflowSecurity.WorkflowItemEditCheckpointCode
		{
			get { return SecurityCore.WorkflowTasksAutoGeneratedCode; }
		}

		string IRequiresWorkflowSecurity.WorkflowItemJustViewCheckpointCode
		{
			get { return SecurityCore.WorkflowTasksJustViewAutoGeneratedCode; }
		}

		ModuleIdentifier IRequiresWorkflowSecurity.WorkflowProviderModuleId
		{
			get { return null; }
		}

		bool IRequiresWorkflowSecurity.ViewOnly { get; set; }
		public ZRichTextBox NotesRichTextBox => SimpleNotesRichTextBox;

		#endregion
	}
}
