namespace Enterprise.Customs.GUI
{
	partial class CusRefTradeGroupForm
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
		private new void InitializeComponent()
		{
			this.CusRefTradeGroupControl = new Enterprise.Customs.GUI.CusRefTradeGroupControl();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CusRefTradeGroupControl.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 338, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefTradeGroup);
			// 
			// CusRefTradeGroupControl
			// 
			this.CusRefTradeGroupControl.AllowDrop = true;
			this.CusRefTradeGroupControl.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.CusRefTradeGroupControl, ".");
			this.CusRefTradeGroupControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.CusRefTradeGroupControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CusRefTradeGroupControl.Name = "CusRefTradeGroupControl";
			this.CusRefTradeGroupControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CusRefTradeGroupControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 307, true);
			this.CusRefTradeGroupControl.TabIndex = 0;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 313, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 25, true);
			this.postingButtonsUserControl.TabIndex = 1;
			// 
			// CusRefTradeGroupForm
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 362, true);
			this.Controls.Add(this.CusRefTradeGroupControl);
			this.Controls.Add(this.postingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefTradeGroup);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 391, true);
			this.Name = "CusRefTradeGroupForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.CusRefTradeGroupControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CusRefTradeGroupControl.ResumeLayout(true);
			this.CusRefTradeGroupControl.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CusRefTradeGroupControl CusRefTradeGroupControl;
		Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
	}
}
