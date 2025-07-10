namespace Enterprise.MasterFiles.GUI
{
	partial class AddressDetailsControlWithLanguage
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ClearFieldsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Address2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValidateAddressButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PostcodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryFindBox = new Enterprise.MasterFiles.GUI.Internal.ZCodeFindBoxFixedPreBoundMaxLength();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LanguageDropEdit.SuspendLayout();
			this.StateDropEdit.SuspendLayout();
			this.CountryFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ISupportWebAddressValidation);
			// 
			// ClearFieldsButton
			// 
			this.ClearFieldsButton.Image = global::Enterprise.MasterFiles.GUI.Properties.Resources.xIcon;
			this.ClearFieldsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 80, true);
			this.ClearFieldsButton.Name = "ClearFieldsButton";
			this.ClearFieldsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.ClearFieldsButton.TabIndex = 5;
			this.ClearFieldsButton.TabStop = false;
			this.ClearFieldsButton.UseVisualStyleBackColor = true;
			this.ClearFieldsButton.Click += ClearFieldsButton_Click;
			// 
			// Address1TextBox
			// 
			this.BindingSource.SetBindingMember(this.Address1TextBox, "Address1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ISupportWebAddressValidation)(null)).Address1)));
			this.Address1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressDetailsControlWithLanguage|FF267403-FF3A-487C-8986-EA1793EDF992", "Address 1");
			this.Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 32, true);
			this.Address1TextBox.Name = "Address1TextBox";
			this.Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.Address1TextBox.TabIndex = 2;
			// 
			// Address2TextBox
			// 
			this.BindingSource.SetBindingMember(this.Address2TextBox, "Address2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ISupportWebAddressValidation)(null)).Address2)));
			this.Address2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressDetailsControlWithLanguage|D1FF071D-8DD4-4DEF-ADE9-7EA7ED272DD5", "Address 2");
			this.Address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 56, true);
			this.Address2TextBox.Name = "Address2TextBox";
			this.Address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.Address2TextBox.TabIndex = 3;
			// 
			// CityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CityTextBox, "City");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ISupportWebAddressValidation)(null)).City)));
			this.CityTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressDetailsControlWithLanguage|4EED4A51-AD77-45BA-95CC-0A1371FA3BA8", "City");
			this.CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 80, true);
			this.CityTextBox.Name = "CityTextBox";
			this.CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.CityTextBox.TabIndex = 4;
			// 
			// ValidateAddressButton
			// 
			this.ValidateAddressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 78, true);
			this.ValidateAddressButton.Name = "ValidateAddressButton";
			this.ValidateAddressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 22, true);
			this.ValidateAddressButton.TabIndex = 6;
			this.ValidateAddressButton.Text = " ";
			this.ValidateAddressButton.UseVisualStyleBackColor = true;
			this.ValidateAddressButton.Click += new System.EventHandler(this.ValidateAddressButton_Click);
			// 
			// PostcodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.PostcodeTextBox, "Postcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ISupportWebAddressValidation)(null)).Postcode)));
			this.PostcodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressDetailsControlWithLanguage|FD730EC3-A6EF-487B-B1D6-4103CFF84CCF", "Postcode");
			this.PostcodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 104, true);
			this.PostcodeTextBox.Name = "PostcodeTextBox";
			this.PostcodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.PostcodeTextBox.TabIndex = 7;
			// 
			// LanguageDropEdit
			// 
			this.LanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LanguageDropEdit, "Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ISupportWebAddressValidation)(null)).Language)));
			this.LanguageDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressDetailsControlWithLanguage|A3E37BF8-3EE3-4425-BAF9-C29B2012803B", "Language");
			this.LanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 130, true);
			this.LanguageDropEdit.Name = "LanguageDropEdit";
			this.LanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 20, true);
			this.LanguageDropEdit.TabIndex = 8;
			// 
			// StateDropEdit
			// 
			this.StateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateDropEdit, "StateCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ISupportWebAddressValidation)(null)).StateCode)));
			this.StateDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressDetailsControlWithLanguage|EA5AF304-A944-49D7-9AEE-F0F25210B7BC", "State");
			this.StateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 156, true);
			this.StateDropEdit.Name = "StateDropEdit";
			this.StateDropEdit.PreBoundMaxLength = 25;
			this.StateDropEdit.ShowDescriptionBox = false;
			this.StateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.StateDropEdit.TabIndex = 9;
			// 
			// CountryFindBox
			// 
			this.CountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryFindBox, "CountryCodeISO2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ISupportWebAddressValidation)(null)).CountryCodeISO2)));
			this.CountryFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddressDetailsControlWithLanguage|D6B1DDCA-FE69-488A-8481-DE8F6F332FA3", "Country/Region");
			this.CountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 156, true);
			this.CountryFindBox.Name = "CountryFindBox";
			this.CountryFindBox.PreBoundMaxLength = 3;
			this.CountryFindBox.ShowDescriptionBox = false;
			this.CountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.CountryFindBox.TabIndex = 10;
			// 
			// AddressDetailsControlWithLanguage
			// 
			this.Controls.Add(this.ClearFieldsButton);
			this.Controls.Add(this.Address1TextBox);
			this.Controls.Add(this.Address2TextBox);
			this.Controls.Add(this.CityTextBox);
			this.Controls.Add(this.ValidateAddressButton);
			this.Controls.Add(this.PostcodeTextBox);
			this.Controls.Add(this.LanguageDropEdit);
			this.Controls.Add(this.StateDropEdit);
			this.Controls.Add(this.CountryFindBox);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "AddressDetailsControlWithLanguage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 203, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LanguageDropEdit.ResumeLayout(true);
			this.LanguageDropEdit.PerformLayout();
			this.StateDropEdit.ResumeLayout(true);
			this.StateDropEdit.PerformLayout();
			this.CountryFindBox.ResumeLayout(true);
			this.CountryFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZTextBox Address1TextBox;
		protected ZArchitecture.ZTextBox Address2TextBox;
		protected ZArchitecture.ZTextBox CityTextBox;
		protected ZArchitecture.GUI.ZButton ValidateAddressButton;
		protected ZArchitecture.ZTextBox PostcodeTextBox;
		private ZArchitecture.GUI.ZDropEdit LanguageDropEdit;
		protected ZArchitecture.GUI.ZDropEdit StateDropEdit;
		private Internal.ZCodeFindBoxFixedPreBoundMaxLength CountryFindBox;
		protected ZArchitecture.GUI.ZButton ClearFieldsButton;
	}
}
