using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class BaseJobDeclarationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected internal BaseCustomsBrokerageUserControl fCustomsBrokerageUserControl;
		protected ZPanel BottomButtonPanel;
		protected internal Enterprise.Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl;
		private readonly System.ComponentModel.Container components = null;
		private ZPanel zPanel1;
		protected internal ZMenuItem TopLevelMenu;
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.BottomButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.oPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomButtonPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.oPostingButtonsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 659, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(501);
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Controls.Add(this.zPanel1);
			this.BottomButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 631, true);
			this.BottomButtonPanel.Name = "BottomButtonPanel";
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 28, true);
			this.BottomButtonPanel.TabIndex = 8;
			// 
			// zPanel1
			// 
			this.zPanel1.AutoSize = true;
			this.zPanel1.Controls.Add(this.oPostingButtonsUserControl);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Right;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(770, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 28, true);
			this.zPanel1.TabIndex = 9;
			// 
			// oPostingButtonsUserControl
			// 
			this.oPostingButtonsUserControl.AllowDrop = true;
			this.oPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.oPostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.oPostingButtonsUserControl.Name = "oPostingButtonsUserControl";
			this.oPostingButtonsUserControl.RequiresHacks = true;
			this.oPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.oPostingButtonsUserControl.TabIndex = 0;
			// 
			// BaseJobDeclarationForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1216, 698, true);
			this.Controls.Add(this.BottomButtonPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1224, 725, true);
			this.Name = "BaseJobDeclarationForm";
			this.Text = "Customs Declaration";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomButtonPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomButtonPanel.ResumeLayout(false);
			this.BottomButtonPanel.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.oPostingButtonsUserControl.ResumeLayout(true);
			this.oPostingButtonsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
