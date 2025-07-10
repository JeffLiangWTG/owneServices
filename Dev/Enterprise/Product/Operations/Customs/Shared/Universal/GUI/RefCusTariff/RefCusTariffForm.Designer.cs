namespace Enterprise.Customs.Universal.GUI
{
	partial class RefCusTariffForm
	{
		protected override void InitializeComponent()
		{
            this.zPostingButtonsUserControl2 = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
            this.refCusTariffUserControl1 = new Enterprise.Customs.Universal.GUI.RefCusTariffUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zPostingButtonsUserControl2.SuspendLayout();
            this.refCusTariffUserControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 467, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.TariffView);
            // 
            // zPostingButtonsUserControl2
            // 
            this.zPostingButtonsUserControl2.AllowDrop = true;
            this.zPostingButtonsUserControl2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.zPostingButtonsUserControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 442, true);
            this.zPostingButtonsUserControl2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
            this.zPostingButtonsUserControl2.Name = "zPostingButtonsUserControl2";
            this.zPostingButtonsUserControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 25, true);
            this.zPostingButtonsUserControl2.TabIndex = 1;
            // 
            // refCusTariffUserControl1
            // 
            this.refCusTariffUserControl1.AllowDrop = true;
            this.refCusTariffUserControl1.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.refCusTariffUserControl1, ".");
            this.refCusTariffUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.refCusTariffUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.refCusTariffUserControl1.Name = "refCusTariffUserControl1";
            this.refCusTariffUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 442, true);
            this.refCusTariffUserControl1.TabIndex = 2;
            // 
            // RefCusTariffForm
            // 
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 491, true);
            this.Controls.Add(this.refCusTariffUserControl1);
            this.Controls.Add(this.zPostingButtonsUserControl2);
            this.DataSourceType = typeof(Enterprise.Customs.Universal.TariffView);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 528, true);
            this.Name = "RefCusTariffForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.zPostingButtonsUserControl2, 0);
            this.Controls.SetChildIndex(this.refCusTariffUserControl1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zPostingButtonsUserControl2.ResumeLayout(true);
            this.zPostingButtonsUserControl2.PerformLayout();
            this.refCusTariffUserControl1.ResumeLayout(true);
            this.refCusTariffUserControl1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
	}
}
