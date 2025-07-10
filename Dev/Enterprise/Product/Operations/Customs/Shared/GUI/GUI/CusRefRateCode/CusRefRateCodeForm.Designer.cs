using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusRefRateCodeForm
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
			this.CusRefRateCodeControl = new Enterprise.Customs.GUI.CusRefRateCodeControl();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CusRefRateCodeControl.SuspendLayout();
			this.postingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 327, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefRateCode);
			// 
			// CusRefRateCodeControl
			// 
			this.CusRefRateCodeControl.AllowDrop = true;
			this.CusRefRateCodeControl.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.CusRefRateCodeControl, ".");
			this.CusRefRateCodeControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.CusRefRateCodeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CusRefRateCodeControl.Name = "CusRefRateCodeControl";
			this.CusRefRateCodeControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CusRefRateCodeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 282, true);
			this.CusRefRateCodeControl.TabIndex = 0;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.AllowDrop = true;
			this.postingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 290, true);
			this.postingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 25, true);
			this.postingButtonsUserControl.TabIndex = 1;
			// 
			// CusRefRateCodeForm
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(554, 331, true);
			this.Controls.Add(this.CusRefRateCodeControl);
			this.Controls.Add(this.postingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.Customs.Universal.CusRefRateCode);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 391, true);
			this.Name = "CusRefRateCodeForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.postingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.CusRefRateCodeControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CusRefRateCodeControl.ResumeLayout(true);
			this.CusRefRateCodeControl.PerformLayout();
			this.postingButtonsUserControl.ResumeLayout(true);
			this.postingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CusRefRateCodeControl CusRefRateCodeControl;
		Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
	}
}
