namespace Enterprise.MasterFiles.GUI
{
	public partial class OrgCreditorGroupForm
	{

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.OG_ClassBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OG_DescBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.DefultHoldOptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HoldOptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FixedCheckBoxDNM = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CheckBoxALM = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.accExRateConfigs = new Enterprise.MasterFiles.GUI.AccExRateConfigs();
			this.zTemplateTabControl1 = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.claimsAndQueriesZTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.jobBillingExRatesZTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.DefultHoldOptionDropEdit.SuspendLayout();
			this.HoldOptionGroupBox.SuspendLayout();
			this.accExRateConfigs.SuspendLayout();
			this.zTemplateTabControl1.SuspendLayout();
			this.claimsAndQueriesZTabPage.SuspendLayout();
			this.jobBillingExRatesZTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 485, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(290);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgCreditorGroup);
			// 
			// OG_ClassBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OG_ClassBoundTextBox, "OG_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCreditorGroup)(null)).OG_Code)));
			this.OG_ClassBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCreditorGroupForm|2cc8dc16-41e8-4a9f-9be9-795f36627ee8", "Class", "Short code of up to 3 alphanumeric characters.");
			this.OG_ClassBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 7, true);
			this.OG_ClassBoundTextBox.Name = "OG_ClassBoundTextBox";
			this.OG_ClassBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.OG_ClassBoundTextBox.TabIndex = 1;
			// 
			// OG_DescBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OG_DescBoundTextBox, "OG_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgCreditorGroup)(null)).OG_Desc)));
			this.OG_DescBoundTextBox.CaptionResourceString = null;
			this.OG_DescBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 30, true);
			this.OG_DescBoundTextBox.Name = "OG_DescBoundTextBox";
			this.OG_DescBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.OG_DescBoundTextBox.TabIndex = 3;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 454, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 14;
			// 
			// DefultHoldOptionDropEdit
			// 
			this.DefultHoldOptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefultHoldOptionDropEdit, "OG_DefaultHoldOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgCreditorGroup)(null)).OG_DefaultHoldOption)));
			this.DefultHoldOptionDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCreditorGroupForm|4F97CD89-D55C-46a2-8B35-A911C778BC34", "Default Invoice Hold Option on New Claims");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DefultHoldOptionDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.DefultHoldOptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 132, true);
			this.DefultHoldOptionDropEdit.Name = "DefultHoldOptionDropEdit";
			this.DefultHoldOptionDropEdit.ShouldResizeByMaxLength = true;
			this.DefultHoldOptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.DefultHoldOptionDropEdit.TabIndex = 20;
			// 
			// HoldOptionGroupBox
			// 
			this.HoldOptionGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCreditorGroupForm|FA7FE313-07F4-4a00-AA42-F274A630DBE4", "Claims and Queries");
			this.HoldOptionGroupBox.Controls.Add(this.TitleLabel);
			this.HoldOptionGroupBox.Controls.Add(this.FixedCheckBoxDNM);
			this.HoldOptionGroupBox.Controls.Add(this.CheckBoxALM);
			this.HoldOptionGroupBox.Controls.Add(this.DefultHoldOptionDropEdit);
			this.HoldOptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HoldOptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HoldOptionGroupBox.Name = "HoldOptionGroupBox";
			this.HoldOptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 349, true);
			this.HoldOptionGroupBox.TabIndex = 21;
			this.HoldOptionGroupBox.TabStop = false;
			// 
			// TitleLabel
			// 
			this.TitleLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCreditorGroupForm|2440AB26-EA25-45ab-B331-0A7C3C367EE1", "Allowed Invoice Hold Options on Claims.\r\nWhen enabled, the following invoice hold options will be available for selection on claims by all users. If disabled, only authorized users will be able to select these options");
			this.TitleLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(709, 23, true);
			this.TitleLabel.TabIndex = 17;
			// 
			// FixedCheckBoxDNM
			// 
			this.BindingSource.SetBindingMember(this.FixedCheckBoxDNM, "OG_IsAllowDNM");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCreditorGroup)(null)).OG_IsAllowDNM)));
			this.FixedCheckBoxDNM.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCreditorGroupForm|AED705C2-F473-4D1D-A84E-89FE4C7720DD", "DNM - Do not match any transactions while claim is open", "This invoice hold option is mandatory and cannot be deselected. This is the default invoice hold option on all claims, unless another default is nominated for this creditor group at ‘Default Invoice Hold Option on New Claims’.");
			this.FixedCheckBoxDNM.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FixedCheckBoxDNM.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 44, true);
			this.FixedCheckBoxDNM.Name = "FixedCheckBoxDNM";
			this.FixedCheckBoxDNM.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 24, true);
			this.FixedCheckBoxDNM.TabIndex = 18;
			this.FixedCheckBoxDNM.UseVisualStyleBackColor = true;
			// 
			// CheckBoxALM
			// 
			this.BindingSource.SetBindingMember(this.CheckBoxALM, "OG_IsAllowALM");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgCreditorGroup)(null)).OG_IsAllowALM)));
			this.CheckBoxALM.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCreditorGroupForm|5629E9F1-585A-4c52-8F45-67352C2984F5", "ALM - Allow matching all transactions", "When enabled, all users will be able to select this option on AP Claims. If disabled, then this option can be selected only by users with security rights to Manage > Payables > Claims and Queries > Edit > Modify Invoice Hold Option.");
			this.CheckBoxALM.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CheckBoxALM.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 73, true);
			this.CheckBoxALM.Name = "CheckBoxALM";
			this.CheckBoxALM.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 24, true);
			this.CheckBoxALM.TabIndex = 19;
			this.CheckBoxALM.UseVisualStyleBackColor = true;
			// 
			// accExRateConfigs
			// 
			this.accExRateConfigs.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.accExRateConfigs, "AccExchangeRateConfigurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.AccExchangeRateConfigurationCollection)(((Enterprise.MasterFiles.Business.OrgCreditorGroup)(null)).AccExchangeRateConfigurations)));
			this.accExRateConfigs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.accExRateConfigs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.accExRateConfigs.Name = "accExRateConfigs";
			this.accExRateConfigs.ReadOnly = false;
			this.accExRateConfigs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 349, true);
			this.accExRateConfigs.TabIndex = 3;
			// 
			// zTemplateTabControl1
			// 
			this.zTemplateTabControl1.Controls.Add(this.claimsAndQueriesZTabPage);
			this.zTemplateTabControl1.Controls.Add(this.jobBillingExRatesZTabPage);
			this.zTemplateTabControl1.Controls.Add(this.zLogsTabPage1);
			this.zTemplateTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 66, true);
			this.zTemplateTabControl1.Name = "zTemplateTabControl1";
			this.zTemplateTabControl1.SelectedIndex = 0;
			this.zTemplateTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(729, 382, true);
			this.zTemplateTabControl1.TabIndex = 23;
			// 
			// claimsAndQueriesZTabPage
			// 
			this.claimsAndQueriesZTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6db8f14e-7c73-4686-8cdb-3a898b52cf99", "Claims and Queries");
			this.claimsAndQueriesZTabPage.Controls.Add(this.HoldOptionGroupBox);
			this.claimsAndQueriesZTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.claimsAndQueriesZTabPage.Name = "claimsAndQueriesZTabPage";
			this.claimsAndQueriesZTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.claimsAndQueriesZTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 355, true);
			this.claimsAndQueriesZTabPage.TabIndex = 0;
			this.claimsAndQueriesZTabPage.UseVisualStyleBackColor = true;
			// 
			// jobBillingExRatesZTabPage
			// 
			this.jobBillingExRatesZTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("384464f0-a797-43bc-a58d-11637b488249", "Job Billing Exchange Rates");
			this.jobBillingExRatesZTabPage.Controls.Add(this.accExRateConfigs);
			this.jobBillingExRatesZTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.jobBillingExRatesZTabPage.Name = "jobBillingExRatesZTabPage";
			this.jobBillingExRatesZTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.jobBillingExRatesZTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 355, true);
			this.jobBillingExRatesZTabPage.TabIndex = 1;
			this.jobBillingExRatesZTabPage.UseVisualStyleBackColor = true;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 355, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// OrgCreditorGroupForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrgCreditorGroupForm|88ab353a-4b8c-4906-ac5b-eda421d4a549", "Creditor Group");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 509, true);
			this.Controls.Add(this.zTemplateTabControl1);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.OG_DescBoundTextBox);
			this.Controls.Add(this.OG_ClassBoundTextBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgCreditorGroup);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
			this.Name = "OrgCreditorGroupForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.OG_ClassBoundTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OG_DescBoundTextBox, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.zTemplateTabControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.DefultHoldOptionDropEdit.ResumeLayout(true);
			this.DefultHoldOptionDropEdit.PerformLayout();
			this.HoldOptionGroupBox.ResumeLayout(false);
			this.HoldOptionGroupBox.PerformLayout();
			this.accExRateConfigs.ResumeLayout(true);
			this.accExRateConfigs.PerformLayout();
			this.zTemplateTabControl1.ResumeLayout(false);
			this.zTemplateTabControl1.PerformLayout();
			this.claimsAndQueriesZTabPage.ResumeLayout(false);
			this.claimsAndQueriesZTabPage.PerformLayout();
			this.jobBillingExRatesZTabPage.ResumeLayout(false);
			this.jobBillingExRatesZTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		System.ComponentModel.IContainer components;
		Enterprise.ZArchitecture.ZTextBox OG_ClassBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit DefultHoldOptionDropEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox HoldOptionGroupBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox FixedCheckBoxDNM;
		Enterprise.ZArchitecture.GUI.ZCheckBox CheckBoxALM;
		ZArchitecture.ZLabel TitleLabel;
		AccExRateConfigs accExRateConfigs;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl zTemplateTabControl1;
		Enterprise.ZArchitecture.GUI.ZTabPage claimsAndQueriesZTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage jobBillingExRatesZTabPage;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		Enterprise.ZArchitecture.ZTextBox OG_DescBoundTextBox;
	}
}
