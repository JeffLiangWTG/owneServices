namespace Enterprise.MasterData.GUI
{
	partial class DeduplicationOrganizationsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.FilterControlGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ResultsViewerUserControl = new Enterprise.MasterData.GUI.DeduplicationResultsViewerDetailsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.FilterControlGroupBox);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 655, true);
			this.MainSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.ResultsViewerUserControl);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.MainSplitContainer.TabIndex = 0;
			// 
			// FilterControlGroupBox
			// 
			this.FilterControlGroupBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("0595290b-ae94-44b8-84a1-404f9a7205c4", "Organizations");
			this.FilterControlGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterControlGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterControlGroupBox.Name = "FilterControlGroupBox";
			this.FilterControlGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 200, true);
			this.FilterControlGroupBox.TabIndex = 0;
			this.FilterControlGroupBox.TabStop = false;
			// 
			// ResultsViewerUserControl
			// 
			this.ResultsViewerUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResultsViewerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResultsViewerUserControl.Name = "ResultsViewerUserControl";
			this.ResultsViewerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 451, true);
			this.ResultsViewerUserControl.TabIndex = 0;
			// 
			// DeduplicationOrganizationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainSplitContainer);
			this.Name = "DeduplicationOrganizationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(973, 655, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private Enterprise.MasterData.GUI.DeduplicationResultsViewerDetailsUserControl ResultsViewerUserControl;
		private ZArchitecture.GUI.ZGroupBox FilterControlGroupBox;
	}
}
