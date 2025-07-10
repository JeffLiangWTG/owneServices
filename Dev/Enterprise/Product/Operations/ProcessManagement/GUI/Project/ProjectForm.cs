using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Enterprise.EConversation.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class ProjectForm : RelatedItemsSupportableFormBase
	{
		public ProjectForm()
		{
			InitializeComponent();
			SetUpRelatedTabPage();
		}

		public ProjectForm(Project project)
			: base(project)
		{
			InitializeComponent();
			SetUpRelatedTabPage();

			PlugIns.Add(ControllerIDs.eConversationPlugIn);
			PlugIns.Add(ControllerIDs.JobInvoicing);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			WorkflowTabPage.Initialize(project);
			SetupActionsMenu();
			SetupEConversationPlugIn();
			SetupWorkflowHooks();
		}

		void SetupWorkflowHooks()
		{
			if (Project != null && Project.HasJobWorkflow)
			{
				Project.JobWorkflow.DoNotStartBeforeDateLocalInfo.ValueChanged += JobWorkflowDeferredDate_ValueChanged;
			}
		}

		void JobWorkflowDeferredDate_ValueChanged(object sender, EventArgs e)
		{
			Project.ClosedOrDeferredDateAsTextInfo.RefreshBinding();
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Type StateUserControlType
		{
			get { return ProjectDetailsControl.State.UserControlType; }
			set { ProjectDetailsControl.State.UserControlType = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Type ContactInformationUserControlType
		{
			get { return ProjectDetailsControl.ContactInformation.UserControlType; }
			set { ProjectDetailsControl.ContactInformation.UserControlType = value; }
		}

		#region Life Cycle

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				new TabConfigurationManager(null, MainTabControl).Enabled = true;
			}

			if (IsViewOrDeleteMode)
			{
				foreach (var menuItem in ProjectActionsMenuItems)
				{
					if (ActionsMenuItem.MenuItems.Contains(menuItem))
					{
						menuItem.Enabled = false;
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && (components != null))
				{
					components.Dispose();
				}

				if (Project != null && Project.HasJobWorkflow)
				{
					Project.JobWorkflow.DoNotStartBeforeDateLocalInfo.ValueChanged -= JobWorkflowDeferredDate_ValueChanged;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion

		#region Project

		protected Project Project
		{
			get { return (Project)DataSource; }
		}

		#endregion

		#region Plugins Setup

		protected virtual void SetupEConversationPlugIn()
		{
			var eConversation = PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);
			((EConversationFullControl)eConversation.UserControl).ShouldShowAddInternalCommentButton = true;
		}

		#endregion

		#region Menu

		void SetupActionsMenu()
		{
			SetupActionsMenuCore();
		}

		protected virtual void SetupActionsMenuCore()
		{
			ActionsMenuItem.MenuItems.AddRange(StandardProjectMenuItems.ToArray());
		}

		protected virtual IEnumerable<MenuItem> ProjectActionsMenuItems
		{
			get { return StandardProjectMenuItems; }
		}

		protected IEnumerable<MenuItem> StandardProjectMenuItems
		{
			get
			{
				if (standardProjectMenuItems == null)
				{
					MenuItem addToLogMenuItem = new ZMenuItem(ResString.GetMultilingualString("ProjectForm|AddToLogMenuItem", "Add to &Log"), AddToLog_Click);
					addToLogMenuItem.Shortcut = Shortcut.CtrlShiftL;
					addToLogMenuItem.ShowShortcut = true;

					MenuItem closeMenuItem = new ZMenuItem(ResString.GetMultilingualString("ProjectForm|CloseMenuItem", "&Close/Cancel"), Close_Click);
					closeMenuItem.Shortcut = Shortcut.CtrlShiftC;
					closeMenuItem.ShowShortcut = true;

					MenuItem reOpenMenuItem = new ZMenuItem(ResString.GetMultilingualString("ProjectForm|ReOpenMenuItem", "Re-&Open"), ReOpen_Click);
					reOpenMenuItem.Shortcut = Shortcut.CtrlShiftO;
					reOpenMenuItem.ShowShortcut = true;

					standardProjectMenuItems = new MenuItem[]
					{
						addToLogMenuItem,
						closeMenuItem,
						reOpenMenuItem,
						new ZMenuItem("-")
					};
				}
				return standardProjectMenuItems;
			}
		}

		MenuItem[] standardProjectMenuItems;

		void AddToLog_Click(object sender, EventArgs e)
		{
			var form = new AddProjectLogPopupForm(Action);
			ZFormModaliser.ShowDialogAndDispose(form);
		}

		void Close_Click(object sender, EventArgs e)
		{
			var form = new CloseProjectPopupForm(Action);
			ZFormModaliser.ShowDialogAndDispose(form);
		}

		void ReOpen_Click(object sender, EventArgs e)
		{
			var form = new ReOpenProjectPopupForm(Action);
			ZFormModaliser.ShowDialogAndDispose(form);
		}

		protected ProjectAction Action
		{
			get { return new ProjectAction(Project); }
		}

		#endregion

		public override string FormCaption
		{
			get
			{
				var project = Project;
				return Res.GetString("ProcessManagement|ProjectForm|FromCaptionPrefix", "Project") +
					(project != null ? " " + project.WKP_ProjectNumber : string.Empty) + (project != null && project.WKP_Summary != "" ? " - " + project.WKP_Summary : string.Empty) +
					(project != null && project.ClientOrganisation != null ? " - " + project.ClientOrganisation.OH_FullName : string.Empty);
			}
		}
	}
}
