using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusProcedureForm
	{
		RefCusProcedureUserControl RefCusProcedureUserControl;
		Core.Forms.ZPostingButtonsUserControl zPostingButtonsUserControl2;

		protected new void InitializeComponent()
		{
			this.zPostingButtonsUserControl2 = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.RefCusProcedureUserControl = new Enterprise.Customs.Universal.GUI.RefCusProcedureUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPostingButtonsUserControl2.SuspendLayout();
			this.RefCusProcedureUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 442, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.RefCusProcedure);
			// 
			// zPostingButtonsUserControl2
			// 
			this.zPostingButtonsUserControl2.AllowDrop = true;
			this.zPostingButtonsUserControl2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPostingButtonsUserControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 466, true);
			this.zPostingButtonsUserControl2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.zPostingButtonsUserControl2.Name = "zPostingButtonsUserControl2";
			this.zPostingButtonsUserControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 25, true);
			this.zPostingButtonsUserControl2.TabIndex = 1;
			// 
			// RefCusProcedureUserControl
			// 
			this.RefCusProcedureUserControl.AllowDrop = true;
			this.RefCusProcedureUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 10, true);
			this.RefCusProcedureUserControl.Name = "RefCusProcedureUserControl";
			this.RefCusProcedureUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 427, true);
			this.RefCusProcedureUserControl.TabIndex = 2;
			// 
			// RefCusProcedureForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 490, true);
			this.Controls.Add(this.zPostingButtonsUserControl2);
			this.Controls.Add(this.RefCusProcedureUserControl);
			this.DataSourceType = typeof(Enterprise.Customs.Universal.RefCusProcedure);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 528, true);
			this.Name = "RefCusProcedureForm";
			this.Controls.SetChildIndex(this.RefCusProcedureUserControl, 0);
			this.Controls.SetChildIndex(this.zPostingButtonsUserControl2, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPostingButtonsUserControl2.ResumeLayout(true);
			this.zPostingButtonsUserControl2.PerformLayout();
			this.RefCusProcedureUserControl.ResumeLayout(true);
			this.RefCusProcedureUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
