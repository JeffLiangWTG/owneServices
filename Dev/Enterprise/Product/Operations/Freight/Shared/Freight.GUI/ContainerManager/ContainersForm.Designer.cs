namespace Enterprise.Freight.GUI
{
	public partial class ContainersForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 461, true);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 434, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(755);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.CommonContainer);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersForm|b55b00f0-2de7-4ca5-bf51-327f3a61e23a", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 434, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// ContainersForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 567, true);
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ContainersForm|99dcde38-fe52-4304-bb6a-b8770473c963", "Container");
			this.DataSourceType = typeof(Enterprise.Freight.Business.CommonContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1067, 544, true);
			this.Name = "ContainersForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ContainersControl = new Enterprise.Freight.GUI.FreightContainersUserControl();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.ContainersControl);
			// 
			// ContainersControl
			// 
			this.BindingSource.SetBindingMember(this.ContainersControl, ".");
			this.ContainersControl.CurrentContainer = null;
			this.ContainersControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.ContainersControl.Name = "ContainersControl";
			this.ContainersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 434);
			this.ContainersControl.TabIndex = 0;
			this.MainTabPage.ResumeLayout(true);

		}

		private FreightContainersUserControl ContainersControl;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}
