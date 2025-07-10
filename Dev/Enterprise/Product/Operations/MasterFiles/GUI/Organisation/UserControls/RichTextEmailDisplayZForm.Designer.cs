namespace Enterprise.MasterFiles.GUI
{
	public partial class RichTextEmailDisplayZForm
	{
		protected override void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RichTextEmailDisplayZForm));
			this.EmailViewerTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EmailViewerTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 520, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(264);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(265);
			// 
			// EmailViewerTextBox
			//
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EmailViewerTextBox, false);
			this.EmailViewerTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EmailViewerTextBox.Enabled = true;
			this.EmailViewerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EmailViewerTextBox.ReadOnly = true;
			this.EmailViewerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 520, true);
			this.EmailViewerTextBox.IsToolBarVisible = false;
			this.EmailViewerTextBox.TabIndex = 3;
			// 
			// RichTextEmailDisplayZForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(544, 542, true);
			this.Controls.Add(this.EmailViewerTextBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 535, true);
			this.Name = "RichTextEmailDisplayZForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.EmailViewerTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EmailViewerTextBox.ResumeLayout(true);
			this.EmailViewerTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
