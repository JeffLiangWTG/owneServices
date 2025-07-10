
namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	partial class ProgressLog
	{
		private void InitializeComponent()
		{
			this.spacer2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.progressBar = new CargoWise.Windows.UI.KProgressBar();
			this.spacer1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.logTextBox = new CargoWise.Windows.UI.KRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// spacer2
			// 
			this.spacer2.Dock = System.Windows.Forms.DockStyle.Top;
			this.spacer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.spacer2.Name = "spacer2";
			this.spacer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 3, true);
			this.spacer2.TabIndex = 4;
			// 
			// progressBar
			// 
			this.progressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.progressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 510);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1262, 24);
			this.progressBar.TabIndex = 1;
			// 
			// spacer1
			// 
			this.spacer1.Dock = System.Windows.Forms.DockStyle.Top;
			this.spacer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.spacer1.Name = "spacer1";
			this.spacer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 3, true);
			this.spacer1.TabIndex = 2;
			// 
			// logTextBox
			// 
			this.logTextBox.DetectUrls = false;
			this.logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.logTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.logTextBox.MaxLength = 128000;
			this.logTextBox.Name = "logTextBox";
			this.logTextBox.ReadOnly = true;
			this.logTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 332, true);
			this.logTextBox.TabIndex = 0;
			this.logTextBox.Text = "";
			// 
			// ActionLog
			// 
			this.Controls.Add(this.logTextBox);
			this.Controls.Add(this.spacer2);
			this.Controls.Add(this.spacer1);
			this.Controls.Add(this.progressBar);
			this.Name = "ActionLog";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 359, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private CargoWise.Windows.UI.KProgressBar progressBar;
		private Enterprise.ZArchitecture.GUI.ZPanel spacer1;
		private CargoWise.Windows.UI.KRichTextBox logTextBox;
		private ZArchitecture.GUI.ZPanel spacer2;
	}
}
