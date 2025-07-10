namespace Enterprise.MasterData.GUI
{
	partial class DeduplicationMonitoringDetailsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.detailsTextBox = new CargoWise.Windows.UI.KRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 427, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.Business.DeduplicationDebugReporter);
			// 
			// detailsTextBox
			// 
			this.detailsTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.detailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsTextBox.Font = new System.Drawing.Font("Courier New", 9.75F);
			this.detailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsTextBox.MaxLength = 10000000;
			this.detailsTextBox.Name = "detailsTextBox";
			this.detailsTextBox.ReadOnly = true;
			this.detailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 427, true);
			this.detailsTextBox.TabIndex = 1;
			this.detailsTextBox.WordWrap = false;
			// 
			// DeduplicationMonitoringDetailsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 451, true);
			this.Controls.Add(this.detailsTextBox);
			this.DataSourceType = typeof(Enterprise.MasterData.Business.DeduplicationDebugReporter);
			this.Name = "DeduplicationMonitoringDetailsForm";
			this.Text = "DeduplicationMonitoringDetailsForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.detailsTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KRichTextBox detailsTextBox;
	}
}