namespace Enterprise.MasterFiles.GUI
{
  partial class CombineTranslatedAddressUserControl
  {
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;

	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	#region Dispose
	protected override void Dispose(bool disposing)
	{
	  if (disposing)
	  {
		if (components != null)
		{
		  components.Dispose();
		}
		if (cancellationToken != null)
		{
		  cancellationToken.Cancel();
		  cancellationToken.Dispose();
		  cancellationToken = null;
		}
	  }
	  base.Dispose(disposing);
	}

	#endregion

	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
			this.AddressDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OA_LanguageBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DsplayTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OA_StateBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OA_Address1BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OA_AdditionalAddressInformationBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OA_CityBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OA_PostCodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OA_Address2BoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OA_CompanyNameOverrideBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AddressDetailsPanel.SuspendLayout();
			this.OA_LanguageBoundDropEdit.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.OA_StateBoundDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgAddress);
			// 
			// AddressDetailsPanel
			// 
			this.AddressDetailsPanel.Controls.Add(this.OA_LanguageBoundDropEdit);
			this.AddressDetailsPanel.Controls.Add(this.DsplayTextTextBox);
			this.AddressDetailsPanel.Controls.Add(this.CountryFindBox);
			this.AddressDetailsPanel.Controls.Add(this.ValidateAddressButton);
			this.AddressDetailsPanel.Controls.Add(this.OA_StateBoundDropEdit);
			this.AddressDetailsPanel.Controls.Add(this.OA_Address1BoundTextBox);
			this.AddressDetailsPanel.Controls.Add(this.OA_AdditionalAddressInformationBoundTextBox);
			this.AddressDetailsPanel.Controls.Add(this.OA_CityBoundTextBox);
			this.AddressDetailsPanel.Controls.Add(this.OA_PostCodeBoundTextBox);
			this.AddressDetailsPanel.Controls.Add(this.OA_Address2BoundTextBox);
			this.AddressDetailsPanel.Controls.Add(this.OA_CompanyNameOverrideBoundTextBox);
			this.AddressDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressDetailsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.AddressDetailsPanel.Name = "AddressDetailsPanel";
			this.AddressDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 200, true);
			this.AddressDetailsPanel.TabIndex = 1;
			// 
			// OA_LanguageBoundDropEdit
			// 
			this.OA_LanguageBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OA_LanguageBoundDropEdit, "Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Language)));
			this.OA_LanguageBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d7db14ce-ec95-4209-8165-25c8bed64e86", "Language");
			this.OA_LanguageBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 3, true);
			this.OA_LanguageBoundDropEdit.Name = "OA_LanguageBoundDropEdit";
			this.OA_LanguageBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.OA_LanguageBoundDropEdit.TabIndex = 19;
			// 
			// DsplayTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.DsplayTextTextBox, "DisplayText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).DisplayText)));
			this.DsplayTextTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("028ceb45-77f0-4a8b-8e57-9369e63940b9", "Short Code");
			this.DsplayTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 28, true);
			this.DsplayTextTextBox.Name = "DsplayTextTextBox";
			this.DsplayTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.DsplayTextTextBox.TabIndex = 3;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "OA_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).OA_RN_NKCountryCode)));
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 151, true);
			this.CountryFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCodeForRegistry;
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShouldResize = true;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CountryFindBox.TabIndex = 9;
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 102, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 22, true);
			this.ValidateAddressButton.TabIndex = 8;
			this.ValidateAddressButton.Text = " ";
			this.ValidateAddressButton.ToolTipCaption = null;
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// OA_StateBoundDropEdit
			// 
			this.OA_StateBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OA_StateBoundDropEdit, "OA_State");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).OA_State)));
			this.OA_StateBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|92B57D2C-20BB-4D1C-AE19-FF080D78B6EB", "State");
			this.OA_StateBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 174, true);
			this.OA_StateBoundDropEdit.Name = "OA_StateBoundDropEdit";
			this.OA_StateBoundDropEdit.PreBoundMaxLength = 25;
			this.OA_StateBoundDropEdit.ShowDescriptionBox = false;
			this.OA_StateBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.OA_StateBoundDropEdit.TabIndex = 11;
			// 
			// OA_Address1BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_Address1BoundTextBox, "Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Address1)));
			this.OA_Address1BoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|7133F41E-F0AD-4EFB-B1F0-F99F649B40E3", "Address 1");
			this.OA_Address1BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 103, true);
			this.OA_Address1BoundTextBox.Name = "OA_Address1BoundTextBox";
			this.OA_Address1BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 20, true);
			this.OA_Address1BoundTextBox.TabIndex = 6;
			// 
			// OA_AdditionalAddressInformationBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_AdditionalAddressInformationBoundTextBox, "OA_AdditionalAddressInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).OA_AdditionalAddressInformation)));
			this.OA_AdditionalAddressInformationBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|F0D1C612-1307-4226-A462-C5E7D71438E2", "Additional Address Info");
			this.OA_AdditionalAddressInformationBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 79, true);
			this.OA_AdditionalAddressInformationBoundTextBox.MaxLength = 50;
			this.OA_AdditionalAddressInformationBoundTextBox.Name = "OA_AdditionalAddressInformationBoundTextBox";
			this.OA_AdditionalAddressInformationBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.OA_AdditionalAddressInformationBoundTextBox.TabIndex = 5;
			// 
			// OA_CityBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_CityBoundTextBox, "City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).City)));
			this.OA_CityBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|7211AFD9-2C22-44DA-99E7-F265CB494EFC", "City");
			this.OA_CityBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 151, true);
			this.OA_CityBoundTextBox.Name = "OA_CityBoundTextBox";
			this.OA_CityBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.OA_CityBoundTextBox.TabIndex = 10;
			// 
			// OA_PostCodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_PostCodeBoundTextBox, "Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Postcode)));
			this.OA_PostCodeBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|221751C0-24A8-4510-B3C2-2877024C1767", "Postcode");
			this.OA_PostCodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 174, true);
			this.OA_PostCodeBoundTextBox.Name = "OA_PostCodeBoundTextBox";
			this.OA_PostCodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.OA_PostCodeBoundTextBox.TabIndex = 9;
			// 
			// OA_Address2BoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_Address2BoundTextBox, "Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).Address2)));
			this.OA_Address2BoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|E3E4DEE6-5621-4896-8179-68DD630CE9A8", "Address 2");
			this.OA_Address2BoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 127, true);
			this.OA_Address2BoundTextBox.Name = "OA_Address2BoundTextBox";
			this.OA_Address2BoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.OA_Address2BoundTextBox.TabIndex = 7;
			// 
			// OA_CompanyNameOverrideBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.OA_CompanyNameOverrideBoundTextBox, "CompanyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgAddress)(null)).CompanyName)));
			this.OA_CompanyNameOverrideBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressesUserControl|06DE595A-E081-4162-B55E-8FE4C29B8072", "Company Name");
			this.OA_CompanyNameOverrideBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 54, true);
			this.OA_CompanyNameOverrideBoundTextBox.Name = "OA_CompanyNameOverrideBoundTextBox";
			this.OA_CompanyNameOverrideBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.OA_CompanyNameOverrideBoundTextBox.TabIndex = 4;
			// 
			// CombineTranslatedAddressUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressDetailsPanel);
			this.Name = "CombineTranslatedAddressUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AddressDetailsPanel.ResumeLayout(false);
			this.AddressDetailsPanel.PerformLayout();
			this.OA_LanguageBoundDropEdit.ResumeLayout(true);
			this.OA_LanguageBoundDropEdit.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.OA_StateBoundDropEdit.ResumeLayout(true);
			this.OA_StateBoundDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	protected ZArchitecture.GUI.ZPanel AddressDetailsPanel;
	private Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
	protected ZArchitecture.GUI.ZButton ValidateAddressButton;
	protected ZArchitecture.GUI.ZDropEdit OA_StateBoundDropEdit;
	protected ZArchitecture.ZTextBox OA_Address1BoundTextBox;
	protected ZArchitecture.ZTextBox OA_AdditionalAddressInformationBoundTextBox;
	protected ZArchitecture.ZTextBox OA_CityBoundTextBox;
	protected ZArchitecture.ZTextBox OA_PostCodeBoundTextBox;
	protected ZArchitecture.ZTextBox OA_Address2BoundTextBox;
	protected ZArchitecture.ZTextBox OA_CompanyNameOverrideBoundTextBox;
		protected ZArchitecture.ZTextBox DsplayTextTextBox;
		private ZArchitecture.GUI.ZDropEdit OA_LanguageBoundDropEdit;
	}
}
