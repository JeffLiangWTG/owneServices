namespace Enterprise.MasterFiles.GUI
{
	public partial class LockReleaseComplianceBookForm
	{
		#region Windows Form Designer generated code

		public new void InitializeComponent()
		{
			this.MenuGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MenuGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 153, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(379);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.LockReleaseComplianceBook);
			// 
			// MenuGuidFindBox
			// 
			this.MenuGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MenuGuidFindBox, "XD_Calc_PK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.LockReleaseComplianceBook)(null)).XD_Calc_PK)));
			this.MenuGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2b2b350b-1738-4717-a263-88d4aee77fce", "Compliance Invoice Book");
			this.MenuGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.MenuGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 55, true);
			this.MenuGuidFindBox.Name = "MenuGuidFindBox";
			this.MenuGuidFindBox.PopupCaption = null;
			this.MenuGuidFindBox.ShouldResize = true;
			this.MenuGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
			this.MenuGuidFindBox.TabIndex = 1;
			// 
			// SaveButton
			// 
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 124, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.UseVisualStyleBackColor = true;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c2151b0e-da01-4cd3-822c-4ec6894ed5e5", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 124, true);
			this.CloseButton.Name = "CancelButton1";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// LockReleaseComplianceBookForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 177, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.MenuGuidFindBox);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.LockReleaseComplianceBook);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.LockReleaseComplianceBook";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(578, 220, true);
			this.Name = "LockReleaseComplianceBookForm";
			this.RememberFormSize = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MenuGuidFindBox, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MenuGuidFindBox.ResumeLayout(true);
			this.MenuGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox MenuGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
	}
}
