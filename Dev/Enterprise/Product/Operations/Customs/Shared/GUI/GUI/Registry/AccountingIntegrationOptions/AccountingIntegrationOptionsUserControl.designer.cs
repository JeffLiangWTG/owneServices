namespace Enterprise.Customs.DataRegistry.GUI
{
	partial class AccountingIntegrationOptionsUserControl
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
			this.EnableIntegrationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DSBChargesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.APPostDSBCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ARPostDSBCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PreAppovalJobCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ChiefStatusCodesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CDSStatusCodesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EUStatusCodesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DSBChargesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DataRegistry.Business.AccountingIntegrationOptions);
			// 
			// EnableIntegrationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EnableIntegrationCheckBox, "EnableAccountingIntegration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.AccountingIntegrationOptions)(null)).EnableAccountingIntegration)));
			this.EnableIntegrationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.EnableIntegrationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.EnableIntegrationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.EnableIntegrationCheckBox.Name = "EnableIntegrationCheckBox";
			this.EnableIntegrationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 20, true);
			this.EnableIntegrationCheckBox.TabIndex = 0;
			this.EnableIntegrationCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("1EC61808-5B74-4378-92A6-8015B5056CF7", "Enable Integration");
			this.EnableIntegrationCheckBox.UseVisualStyleBackColor = true;
			// 
			// DSBChargesGroupBox
			// 
			this.DSBChargesGroupBox.Controls.Add(this.APPostDSBCheckBox);
			this.DSBChargesGroupBox.Controls.Add(this.ARPostDSBCheckBox);
			this.DSBChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 30, true);
			this.DSBChargesGroupBox.Name = "DSBChargesGroupBox";
			this.DSBChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 46, true);
			this.DSBChargesGroupBox.TabIndex = 1;
			this.DSBChargesGroupBox.TabStop = false;
			this.DSBChargesGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("92CE2F53-7B10-4FFA-827B-EE52A4738F2C", "Customs Disbursement Charges");
			// 
			// APPostDSBCheckBox
			// 
			this.BindingSource.SetBindingMember(this.APPostDSBCheckBox, "APPostDSB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.AccountingIntegrationOptions)(null)).APPostDSB)));
			this.APPostDSBCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.APPostDSBCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.APPostDSBCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 18, true);
			this.APPostDSBCheckBox.Name = "APPostDSBCheckBox";
			this.APPostDSBCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 24, true);
			this.APPostDSBCheckBox.TabIndex = 1;
			this.APPostDSBCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("6B4FD83D-D2D4-41C0-9F68-0C54C7119E3B", "Post AP");
			this.APPostDSBCheckBox.UseVisualStyleBackColor = true;
			// 
			// ARPostDSBCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ARPostDSBCheckBox, "ARPostDSB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.AccountingIntegrationOptions)(null)).ARPostDSB)));
			this.ARPostDSBCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ARPostDSBCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARPostDSBCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 18, true);
			this.ARPostDSBCheckBox.Name = "ARPostDSBCheckBox";
			this.ARPostDSBCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 24, true);
			this.ARPostDSBCheckBox.TabIndex = 0;
			this.ARPostDSBCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("38AFD7EA-AE47-4786-9B9C-6D81A8E9301B", "Post AR");
			this.ARPostDSBCheckBox.UseVisualStyleBackColor = true;
			// 
			// PreAppovalJobCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PreAppovalJobCheckBox, "PreApprovalBillingJob");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DataRegistry.Business.AccountingIntegrationOptions)(null)).PreApprovalBillingJob)));
			this.PreAppovalJobCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PreAppovalJobCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ARPostDSBCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PreAppovalJobCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 83, true);
			this.PreAppovalJobCheckBox.Name = "PreAppovalJobCheckBox";
			this.PreAppovalJobCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 24, true);
			this.PreAppovalJobCheckBox.TabIndex = 4;
			this.PreAppovalJobCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("654E23C5-D3FF-4874-962A-27377E3BF7A8", "Post AR && AP if job is approved");
			this.PreAppovalJobCheckBox.UseVisualStyleBackColor = true;
			// 
			// ChiefStatusCodesTextBox
			// 
			this.BindingSource.SetBindingMember(this.ChiefStatusCodesTextBox, Business.AccountingIntegrationOptions.Schema.ChiefCustomsStatusCodes);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.AccountingIntegrationOptions)(null)).ChiefCustomsStatusCodes)));
			this.ChiefStatusCodesTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("D5763C4C-FD25-43EB-9A91-47C399864352", "ICS status codes that cause auto billing to occur for CHIEF jobs", "Enter a comma-separated list of status codes which, when newly attained on an entry, will cause the auto billing to run.  For example \'01,02,03\'.  Leave this blank to use the default configuration to run auto billing only at acceptance.  If overridden, it must be a complete list of status codes, which are available via eLearning unit 1BGBXXX");
			this.ChiefStatusCodesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 113, true);
			this.ChiefStatusCodesTextBox.Name = "ChiefStatusCodesTextBox";
			this.ChiefStatusCodesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ChiefStatusCodesTextBox.TabIndex = 5;
			// 
			// CDSStatusCodesTextBox
			//
			this.BindingSource.SetBindingMember(this.CDSStatusCodesTextBox, Business.AccountingIntegrationOptions.Schema.CDSCustomsStatusCodes);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.AccountingIntegrationOptions)(null)).CDSCustomsStatusCodes)));
			this.CDSStatusCodesTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("4BDED78D-95E5-4F0A-9FA0-9DB038625887", "Entry status codes that cause auto billing to occur for CDS jobs", "Enter a comma-separated list of status codes which, when newly attained on an entry, will cause the auto billing to run.  For example \'01,02,03\'.  Leave this blank to use the default configuration to run auto billing only at acceptance.  If overridden, it must be a complete list of status codes, which are available via eLearning unit 1BGBXXX");
			this.CDSStatusCodesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 139, true);
			this.CDSStatusCodesTextBox.Name = "CDSStatusCodesTextBox";
			this.CDSStatusCodesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CDSStatusCodesTextBox.TabIndex = 6;
			// 
			// EUStatusCodesTextBox
			//
			this.BindingSource.SetBindingMember(this.EUStatusCodesTextBox, Business.AccountingIntegrationOptions.Schema.EUCustomsStatusCodes);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.AccountingIntegrationOptions)(null)).EUCustomsStatusCodes)));
			this.EUStatusCodesTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2BD4F7C4-B38F-4F7E-8F02-FF9C8BDCE85A", "Entry status codes that cause auto billing to occur", "Enter a comma-separated list of status codes which, when newly attained on an entry, will cause the auto billing to run.  For example \'CLR,CLP,CDA\'.  Leave this blank to use the default configuration to run auto billing only at acceptance.  If overridden, it must be a complete list of status codes.");
			this.EUStatusCodesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 139, true);
			this.EUStatusCodesTextBox.Name = "EUStatusCodesTextBox";
			this.EUStatusCodesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.EUStatusCodesTextBox.TabIndex = 6;
			// 
			// AccountingIntegrationOptionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EUStatusCodesTextBox);
			this.Controls.Add(this.CDSStatusCodesTextBox);
			this.Controls.Add(this.ChiefStatusCodesTextBox);
			this.Controls.Add(this.PreAppovalJobCheckBox);
			this.Controls.Add(this.DSBChargesGroupBox);
			this.Controls.Add(this.EnableIntegrationCheckBox);
			this.Name = "AccountingIntegrationOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 166, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DSBChargesGroupBox.ResumeLayout(false);
			this.DSBChargesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCheckBox EnableIntegrationCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox DSBChargesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox APPostDSBCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ARPostDSBCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox PreAppovalJobCheckBox;
		internal ZArchitecture.ZTextBox ChiefStatusCodesTextBox;
		internal ZArchitecture.ZTextBox CDSStatusCodesTextBox;
		internal ZArchitecture.ZTextBox EUStatusCodesTextBox;
	}
}
