using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public interface IWorkflowTaskNavigationOverridable
	{
		void NavigateToWorkflowItem(ProcessTask task);
		void NavigateToWorkflowItem(IProcessHeader workflow);
	}

	public partial class TaskManagementForm : ZTemplateForm
	{
		public TaskManagementForm(ProcessTask task)
			: base(task)
		{
			InitializeComponent();
			taskOwnerPasswordEventManager = TaskOwnerPasswordEventManager.Manage(task);
			PlugIns.Add(ControllerIDs.Audit);

			task.IterationLinksViewModel.Build();
			task.IterationLinksViewModel.HasChanges = false;

			CustomiseTabsForWorkflowManagementMode();

			if (!(task.ParentTaskCollection?.SupportsContactAndAddress ?? true))
			{
				OrgDetailsPanel.Dispose();
			}

			var processTaskStatusChangeModeTracker = ObjectFactory.Get<ProcessTaskStatusChangeModeTracker>();
			P9_StatusDropEdit.Enter += (_, __) => processTaskStatusChangeModeTracker.SetCurrent(ProcessTaskStatusChangeModeCodeList.Codes.EditTaskScreen);
			P9_StatusDropEdit.Leave += (_, __) => processTaskStatusChangeModeTracker.Clear();
		}

		void CustomiseTabsForWorkflowManagementMode()
		{
			var bmsRegistry = ObjectFactory.Get<IBMSRegistry>();

			if (bmsRegistry.BufferManagementEnabled)
			{
				InitialiseWorkflowsTab();
				InitialiseTagsTab();
			}
			else
			{
				WorkflowDetailsTabPage.TabVisible = false;
				BMTagsTab.TabVisible = false;
			}
		}

		void InitialiseWorkflowsTab()
		{
			if (BusinessEntity.IsInDatabase && BusinessEntity.ProcessHeader != null)
			{
				var control = (ZUserControl)ObjectFactory.Get<IWorkflowDetailsUserControl>();
				var processHeader = BusinessEntity.ProcessHeader;
				control.AllowDrop = true;
				control.CaptionRenderingEnabled = true;
				control.SetDataBinding(processHeader, string.Empty);
				control.Dock = DockStyle.Fill;
				control.Name = "WorkflowDetailsUserControl";
				control.TabIndex = 0;
				WorkflowDetailsTabPage.Controls.Add(control);
			}
		}

		void InitialiseTagsTab()
		{
			var control = (ZUserControl)ObjectFactory.Get<ITagsUserControl>();
			control.SetDataBinding(((ITagBindable)BusinessEntity).TagLinks_ForBinding, string.Empty);
			control.Dock = DockStyle.Fill;
			control.Name = "TagUserControl";
			BMTagsTab.Controls.Add(control);
		}

		public override string FormCaption
		{
			get
			{
				if (BusinessEntity != null && !BusinessEntity.IsDeleted)
				{
					return String.Format("{0} {1}", base.FormCaption, BusinessEntity.P9_TaskID);
				}
				else
				{
					return base.FormCaption;
				}
			}
		}
		new ProcessTask BusinessEntity
		{
			get { return (ProcessTask)base.BusinessEntity; }
		}

		#region Parent

		void ShowParentButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.ParentControllerID != null && BusinessEntity.ParentBusinessObject != null)
			{
				lastShownParentForm = ZControllerFactory.Create(BusinessEntity.ParentControllerID).ShowEditForm(BusinessEntity.ParentBusinessObject);

				if (lastShownParentForm != null && ((Control)lastShownParentForm).IsHandleCreated)
				{
					WorkflowParentFormFactory.NavigateToWorkflowItem(lastShownParentForm, BusinessEntity);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("e9f322e6-b52c-4bb6-8a66-09c127a17aec", "This is a stand-alone task."));
				lastShownParentForm = null;
			}
		}

		internal IZForm lastShownParentForm;

		#endregion

		#region SupportEDocs

		internal protected new bool SupportsEDocs
		{
			get { return !(BusinessEntity is TemplateProcessTask); }
		}

		#endregion

		internal new MenuItem ActionsMenuItem
		{
			get { return base.ActionsMenuItem; }
		}

		#region Dispose

		readonly TaskOwnerPasswordEventManager taskOwnerPasswordEventManager;

		protected override void Dispose(bool isNotFinalizing)
		{
			taskOwnerPasswordEventManager.Dispose();
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
