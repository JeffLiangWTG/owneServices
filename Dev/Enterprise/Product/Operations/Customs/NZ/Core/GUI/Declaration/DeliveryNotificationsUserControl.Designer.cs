namespace Enterprise.Customs.NZ.GUI.Declaration
{
	partial class DeliveryNotificationsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DeliveryNotificationsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeliveryNotificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeliveryNotificationPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DeliveryNotificationCCAATFDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DeliveryNotificationPartyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeliveryNotificationPartyFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeliveryNotificationsPanel.SuspendLayout();
			this.DeliveryNotificationGroupBox.SuspendLayout();
			this.DeliveryNotificationPortFindBox.SuspendLayout();
			this.DeliveryNotificationCCAATFDocAddressControl.SuspendLayout();
			this.DeliveryNotificationPartyGroupBox.SuspendLayout();
			this.DeliveryNotificationPartyFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.JobDeclaration);
			// 
			// DeliveryNotificationsPanel
			// 
			this.DeliveryNotificationsPanel.Controls.Add(this.DeliveryNotificationGroupBox);
			this.DeliveryNotificationsPanel.Controls.Add(this.DeliveryNotificationCCAATFDocAddressControl);
			this.DeliveryNotificationsPanel.Controls.Add(this.DeliveryNotificationPartyGroupBox);
			this.DeliveryNotificationsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DeliveryNotificationsPanel.Name = "DeliveryNotificationsPanel";
			this.DeliveryNotificationsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 561, true);
			this.DeliveryNotificationsPanel.TabIndex = 12;
			// 
			// DeliveryNotificationGroupBox
			// 
			this.DeliveryNotificationGroupBox.Controls.Add(this.DeliveryNotificationPortFindBox);
			this.DeliveryNotificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 121, true);
			this.DeliveryNotificationGroupBox.Name = "DeliveryNotificationGroupBox";
			this.DeliveryNotificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 43, true);
			this.DeliveryNotificationGroupBox.TabIndex = 2;
			this.DeliveryNotificationGroupBox.TabStop = false;
			this.DeliveryNotificationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("fe395e8c-6529-4e2e-a2dc-d1bee16a4665", "Delivery Notification Port");
			// 
			// DeliveryNotificationPortFindBox
			// 
			this.DeliveryNotificationPortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryNotificationPortFindBox, "JE_RL_NKPortOfDeliveryNotify");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_RL_NKPortOfDeliveryNotify)));
			this.DeliveryNotificationPortFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("9254b045-8c52-4c35-b800-d880675dddbc", "Delivery Notification Port");
			this.DeliveryNotificationPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 17, true);
			this.DeliveryNotificationPortFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.DeliveryNotificationPortFindBox.Name = "DeliveryNotificationPortFindBox";
			this.DeliveryNotificationPortFindBox.PreBoundMaxLength = 5;
			this.DeliveryNotificationPortFindBox.ShouldResize = true;
			this.DeliveryNotificationPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.DeliveryNotificationPortFindBox.TabIndex = 0;
			// 
			// DeliveryNotificationCCAATFDocAddressControl
			// 
			this.DeliveryNotificationCCAATFDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryNotificationCCAATFDocAddressControl, "NotifyParty2DocumentaryAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).NotifyParty2DocumentaryAddress)));
			this.DeliveryNotificationCCAATFDocAddressControl.BindToOrganisations = "Lookups.DeliveryNotificationCCPATFOrganisations";
			this.DeliveryNotificationCCAATFDocAddressControl.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("4dbd65b4-df62-4ed7-b3ee-6ee9c30ae2ba", "Delivery Notification CCA/ATF");
			this.DeliveryNotificationCCAATFDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.HideOverrideAndTabs;
			this.DeliveryNotificationCCAATFDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 49, true);
			this.DeliveryNotificationCCAATFDocAddressControl.Name = "DeliveryNotificationCCAATFDocAddressControl";
			this.DeliveryNotificationCCAATFDocAddressControl.ReadOnly = false;
			this.DeliveryNotificationCCAATFDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DeliveryNotificationCCAATFDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 68, true);
			this.DeliveryNotificationCCAATFDocAddressControl.TabIndex = 1;
			this.DeliveryNotificationCCAATFDocAddressControl.ValidationJustForced = false;
			// 
			// DeliveryNotificationPartyGroupBox
			// 
			this.DeliveryNotificationPartyGroupBox.Controls.Add(this.DeliveryNotificationPartyFindBox);
			this.DeliveryNotificationPartyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.DeliveryNotificationPartyGroupBox.Name = "DeliveryNotificationPartyGroupBox";
			this.DeliveryNotificationPartyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 43, true);
			this.DeliveryNotificationPartyGroupBox.TabIndex = 0;
			this.DeliveryNotificationPartyGroupBox.TabStop = false;
			this.DeliveryNotificationPartyGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("6fb5abd2-d87f-4f2c-82ea-c0d0320534d7", "Delivery Notification Party");
			// 
			// DeliveryNotificationPartyFindBox
			// 
			this.DeliveryNotificationPartyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeliveryNotificationPartyFindBox, "JE_OH_NotifyParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_OH_NotifyParty)));
			this.DeliveryNotificationPartyFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("515681d8-233b-4d16-9bc5-cf9aa0a2a956", "Delivery Notification Port");
			this.DeliveryNotificationPartyFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DeliveryNotificationPartyFindBox, false);
			this.DeliveryNotificationPartyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 16, true);
			this.DeliveryNotificationPartyFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.DeliveryNotificationPartyFindBox.Name = "DeliveryNotificationPartyFindBox";
			this.DeliveryNotificationPartyFindBox.ShouldResize = true;
			this.DeliveryNotificationPartyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 20, true);
			this.DeliveryNotificationPartyFindBox.TabIndex = 0;
			// 
			// DeliveryNotificationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeliveryNotificationsPanel);
			this.Name = "DeliveryNotificationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 566, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeliveryNotificationsPanel.ResumeLayout(false);
			this.DeliveryNotificationsPanel.PerformLayout();
			this.DeliveryNotificationGroupBox.ResumeLayout(false);
			this.DeliveryNotificationGroupBox.PerformLayout();
			this.DeliveryNotificationPortFindBox.ResumeLayout(true);
			this.DeliveryNotificationPortFindBox.PerformLayout();
			this.DeliveryNotificationCCAATFDocAddressControl.ResumeLayout(true);
			this.DeliveryNotificationCCAATFDocAddressControl.PerformLayout();
			this.DeliveryNotificationPartyGroupBox.ResumeLayout(false);
			this.DeliveryNotificationPartyGroupBox.PerformLayout();
			this.DeliveryNotificationPartyFindBox.ResumeLayout(true);
			this.DeliveryNotificationPartyFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox DeliveryNotificationGroupBox;
		protected ZArchitecture.GUI.ZGroupBox DeliveryNotificationPartyGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel DeliveryNotificationsPanel;
		protected ZArchitecture.GUI.ZCodeFindBox DeliveryNotificationPortFindBox;
		protected Enterprise.MasterFiles.GUI.ZOrganisationFindBox DeliveryNotificationPartyFindBox;
		protected Enterprise.MasterFiles.GUI.ZDocAddressControl DeliveryNotificationCCAATFDocAddressControl;
	}
}
