namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgSecurityGroupUpdateForm
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrgSecurityGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.OrgSecurityGroupFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 90, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgMiscServ);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 54, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 36, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 9, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 25, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.OrgSecurityGroupFindBox);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 54, true);
			this.MainPanel.TabIndex = 0;
			// 
			// OrgSecurityGroupFindBox
			// 
			this.OrgSecurityGroupFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgSecurityGroupFindBox, "OM_GG_OrgSecurityGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgMiscServ)(null)).OM_GG_OrgSecurityGroup)));
			this.OrgSecurityGroupFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgSecurityGroupUpdateForm|OrgSecurityGroupFindBox", "Security");
			this.OrgSecurityGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(71, 23, true);
			this.OrgSecurityGroupFindBox.Name = "OrgSecurityGroupFindBox";
			this.OrgSecurityGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 15, true);
			this.OrgSecurityGroupFindBox.TabIndex = 0;
			this.OrgSecurityGroupFindBox.TabStop = false;
			// 
			// OrgSecurityGroupUpdateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("887060aa-b701-47d3-9c74-d7607e9141fb", "Set Organization Security Group");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 114, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgMiscServ);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 150, true);
			this.Name = "OrgSecurityGroupUpdateForm";
			this.Text = "OrgSecurityGroupUpdateForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.OrgSecurityGroupFindBox.ResumeLayout(true);
			this.OrgSecurityGroupFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZGuidFindBox OrgSecurityGroupFindBox;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
	}
}