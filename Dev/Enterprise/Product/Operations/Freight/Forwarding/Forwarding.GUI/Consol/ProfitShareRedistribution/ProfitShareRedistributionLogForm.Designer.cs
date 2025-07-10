namespace Enterprise.Freight.Forwarding.GUI.Consol.ProfitShareRedistribution
{
	partial class ProfitShareRedistributionLogForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.LogRichTextBox = new CargoWise.Windows.UI.KRichTextBox();
			this.ProgressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LogsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TopSpace = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProgressGroupBox.SuspendLayout();
			this.LogsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 323, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// ProgressBar
			// 
			this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 22, false);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(883, 24, false);
			this.ProgressBar.TabIndex = 2;
			// 
			// LogRichTextBox
			// 
			this.LogRichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.LogRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 15, true);
			this.LogRichTextBox.Name = "LogRichTextBox";
			this.LogRichTextBox.ReadOnly = true;
			this.LogRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(589, 268, true);
			this.LogRichTextBox.TabIndex = 3;
			this.LogRichTextBox.Text = "";
			this.LogRichTextBox.WordWrap = false;
			// 
			// ProgressGroupBox
			// 
			this.ProgressGroupBox.Controls.Add(this.ProgressBar);
			this.ProgressGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProgressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.ProgressGroupBox.Name = "ProgressGroupBox";
			this.ProgressGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 2, 5, 2, true);
			this.ProgressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 33, true);
			this.ProgressGroupBox.TabIndex = 4;
			this.ProgressGroupBox.TabStop = false;
			// 
			// LogsGroupBox
			// 
			this.LogsGroupBox.Controls.Add(this.LogRichTextBox);
			this.LogsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 35, true);
			this.LogsGroupBox.Name = "LogsGroupBox";
			this.LogsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 2, 5, 5, true);
			this.LogsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 287, true);
			this.LogsGroupBox.TabIndex = 5;
			this.LogsGroupBox.TabStop = false;
			// 
			// TopSpace
			// 
			this.TopSpace.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopSpace.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopSpace.Name = "TopSpace";
			this.TopSpace.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 3, true);
			this.TopSpace.TabIndex = 3;
			// 
			// ProfitShareRedistributionLogForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(598, 347, true);
			this.Controls.Add(this.LogsGroupBox);
			this.Controls.Add(this.ProgressGroupBox);
			this.Controls.Add(this.TopSpace);
			this.Name = "ProfitShareRedistributionLogForm";
			this.Controls.SetChildIndex(this.TopSpace, 0);
			this.Controls.SetChildIndex(this.ProgressGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LogsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProgressGroupBox.ResumeLayout(false);
			this.ProgressGroupBox.PerformLayout();
			this.LogsGroupBox.ResumeLayout(false);
			this.LogsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KProgressBar ProgressBar;
		CargoWise.Windows.UI.KRichTextBox LogRichTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox ProgressGroupBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox LogsGroupBox;
		Enterprise.ZArchitecture.GUI.ZPanel TopSpace;
	}
}
