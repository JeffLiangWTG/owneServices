using System;
using CargoWise.Application;
using Enterprise.BufferManagement.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class ProjectInformationControl : ZUserControl
	{
		public ProjectInformationControl()
		{
			InitializeComponent();
			InitializeComponentForReleaseSequence();
			ProjectProcessTemplateCustomFieldsControl.NothingSetupMessageLabelText = Res.GetString("AC78405E-F3E4-40FA-A961-B11BC34BE1EF", "To make use of this tab, please setup project custom fields in Workflow Manager.");
		}

		Project Project
		{
			get { return (Project)CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (Project != null)
			{
				CustomLabelHelper.Setup(ProjectTypeDropEdit, ProcessManagementRegistry.Instance.ProjectTypeLabel);
				CustomLabelHelper.Setup(ProjectSubTypeDropEdit, ProcessManagementRegistry.Instance.ProjectSubtypeLabel);
				CustomLabelHelper.Setup(ProjectModuleDropEdit, ProcessManagementRegistry.Instance.ProjectModuleLabel);
				CustomLabelHelper.Setup(PriorityDropEdit, ProcessManagementRegistry.Instance.ProjectPriorityLabel);
			}
		}

		void InitializeComponentForReleaseSequence()
		{
			if (ObjectFactory.Get<IBMSRegistry>().ReleaseSequencesModuleEnabled)
			{
				ReleaseSequence = new ZDynamicControlCreationUserControl();
				SplitContainerForReleaseSequence = new CargoWise.Windows.UI.KSplitContainer();
				splitContainer1.SuspendLayout();
				SplitContainerForReleaseSequence.SuspendLayout();
				ReleaseSequence.SuspendLayout();

				ReleaseSequence.AllowDrop = true;
				ReleaseSequence.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
				ReleaseSequence.Dock = System.Windows.Forms.DockStyle.Fill;
				ReleaseSequence.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
				ReleaseSequence.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 255, true);
				ReleaseSequence.Name = "ReleaseSequence";
				ReleaseSequence.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 255, true);
				ReleaseSequence.TabIndex = 1;
				ReleaseSequence.UserControlType = typeof(ProjectReleaseSequenceControl);

				SplitContainerForReleaseSequence.Dock = System.Windows.Forms.DockStyle.Fill;
				SplitContainerForReleaseSequence.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
				SplitContainerForReleaseSequence.Name = "SplitContainerForReleaseSequence";
				SplitContainerForReleaseSequence.Orientation = System.Windows.Forms.Orientation.Horizontal;
				SplitContainerForReleaseSequence.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 307, true);
				SplitContainerForReleaseSequence.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(173);
				SplitContainerForReleaseSequence.TabIndex = 1;

				splitContainer1.Panel1.Controls.Remove(InformationGroupBox);
				splitContainer1.Panel1.Controls.Add(SplitContainerForReleaseSequence);
				SplitContainerForReleaseSequence.Panel1.Controls.Add(InformationGroupBox);
				SplitContainerForReleaseSequence.Panel2.Controls.Add(ReleaseSequence);

				SplitContainerForReleaseSequence.ResumeLayout(true);
				splitContainer1.ResumeLayout(true);
				ReleaseSequence.ResumeLayout(true);
			}
		}

		ZDynamicControlCreationUserControl ReleaseSequence;
		CargoWise.Windows.UI.KSplitContainer SplitContainerForReleaseSequence;

#if DEBUG
		public ZDropEdit GetProjectTypeDropEdit()
		{
			return ProjectTypeDropEdit;
		}

		public ZDropEdit GetProjectSubTypeDropEdit()
		{
			return ProjectSubTypeDropEdit;
		}

		public ZDropEdit GetProjectModuleDropEdit()
		{
			return ProjectModuleDropEdit;
		}

		public ZDropEdit GetPriorityDropEdit()
		{
			return PriorityDropEdit;
		}
#endif
	}
}
