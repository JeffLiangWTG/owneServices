namespace Enterprise.MasterFiles.GUI
{
	partial class RefFacilityForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();

			// 
			// MainTabControl
			//
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 291, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);

			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 269, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 269, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 269, true);
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 291, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 291, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefFacility);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_FacilityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_PostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_StateCodeDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_CountryCodeDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_UNLOCODesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((((CargoWise.Types.ZGeography)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_GeoLocation))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_TerminalType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_SMDGCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefFacility)(null)).RFT_ContainerAutomationAvailable)));

			// 
			// RefFacilityForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 450, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefFacility);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 500, true);
			this.Name = "RefFacilityForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "RefFacilityForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.FacilityCodeTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AvailableIntegrationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RFT_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_FacilityTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_Address1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_Address2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_CityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_PostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_StateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_RN_NKCountryCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_RL_NKLocationCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_TerminalTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_SMDGCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LatitudeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LongitudeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RFT_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RFT_ContainerAutomationAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RefFacilityLocalCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();

			this.MainTabPage.SuspendLayout();
			this.FacilityCodeTabControl.SuspendLayout();
			this.AdditionalDetailsGroupBox.SuspendLayout();
			this.AvailableIntegrationGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.RefFacilityLocalCodesTabPage.SuspendLayout();

			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c324654b-627d-44f2-a9e5-94ae8c3bbcf4", "Facility");
			this.MainTabPage.Controls.Add(this.DetailsGroupBox);
			this.MainTabPage.Controls.Add(this.AdditionalDetailsGroupBox);
			this.MainTabPage.Controls.Add(this.AvailableIntegrationGroupBox);
			this.MainTabPage.Controls.Add(this.FacilityCodeTabControl);

			// 
			// FacilityCodeTabControl
			//
			this.FacilityCodeTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.FacilityCodeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 3, true);
			this.FacilityCodeTabControl.Controls.Add(this.RefFacilityLocalCodesTabPage);
			this.FacilityCodeTabControl.Name = "FacilityCodeTabControl";
			this.FacilityCodeTabControl.SelectedIndex = 0;
			this.FacilityCodeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 350, true);
			this.FacilityCodeTabControl.TabIndex = 0;
			this.FacilityCodeTabControl.TabStop = false;

			// 
			// DetailsGroupBox
			//
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c724654b-657d-44f2-a9e5-942e5c3aacf4", "Details");
			this.DetailsGroupBox.Controls.Add(this.RFT_IsActiveCheckBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_CodeTextBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_FacilityTypeTextBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_NameTextBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_Address1TextBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_Address2TextBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_CityTextBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_PostCodeTextBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_StateTextBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_RN_NKCountryCodeTextBox);
			this.DetailsGroupBox.Controls.Add(this.RFT_RL_NKLocationCodeTextBox);
			this.DetailsGroupBox.Controls.Add(this.LatitudeTextBox);
			this.DetailsGroupBox.Controls.Add(this.LongitudeTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 220, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;

			// 
			// RFT_FacilityType
			// 
			this.BindingSource.SetBindingMember(this.RFT_FacilityTypeTextBox, "RFT_FacilityType");
			this.RFT_FacilityTypeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("344c2693-b995-4ffb-9b71-5cdd123855bc", "Facility type");
			this.RFT_FacilityTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 20, true);
			this.RFT_FacilityTypeTextBox.Name = "RFT_FacilityType";
			this.RFT_FacilityTypeTextBox.ReadOnly = true;
			this.RFT_FacilityTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.RFT_FacilityTypeTextBox.TabIndex = 2;

			// 
			// RFT_IsActiveCheckBox
			// 
			this.RFT_IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RFT_IsActiveCheckBox, "RFT_IsActive");
			this.RFT_IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("344c2693-b995-4ffb-9b77-5cd8123858bc", "Is Active");
			this.RFT_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RFT_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 20, true);
			this.RFT_IsActiveCheckBox.Name = "RFT_IsActiveCheckBox";
			this.RFT_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 16, true);
			this.RFT_IsActiveCheckBox.TabIndex = 3;
			this.RFT_IsActiveCheckBox.ReadOnly = true;

			// 
			// RFT_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_CodeTextBox, "RFT_Code");
			this.RFT_CodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("344c2693-b995-4ff1-9b71-5cdd123835bc", "Code");
			this.RFT_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 40, true);
			this.RFT_CodeTextBox.Name = "RFT_Code";
			this.RFT_CodeTextBox.ReadOnly = true;
			this.RFT_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.RFT_CodeTextBox.TabIndex = 4;

			// 
			// RFT_NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_NameTextBox, "RFT_Name");
			this.RFT_NameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("144c2693-b995-4ff1-9b71-5cdd123825bc", "Name");
			this.RFT_NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 60, true);
			this.RFT_NameTextBox.Name = "RFT_Name";
			this.RFT_NameTextBox.ReadOnly = true;
			this.RFT_NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 17, true);
			this.RFT_NameTextBox.TabIndex = 5;

			// 
			// RFT_Address1TextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_Address1TextBox, "RFT_Address1");
			this.RFT_Address1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("124c2693-b995-4ff1-9b71-5cdd123825b1", "Address 1");
			this.RFT_Address1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 80, true);
			this.RFT_Address1TextBox.Name = "RFT_Address1";
			this.RFT_Address1TextBox.ReadOnly = true;
			this.RFT_Address1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 17, true);
			this.RFT_Address1TextBox.TabIndex = 6;

			// 
			// RFT_Address2TextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_Address2TextBox, "RFT_Address2");
			this.RFT_Address2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("524c2693-b995-4ff1-9b71-5bdd123815b1", "Address 2");
			this.RFT_Address2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 100, true);
			this.RFT_Address2TextBox.Name = "RFT_Address2";
			this.RFT_Address2TextBox.ReadOnly = true;
			this.RFT_Address2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 17, true);
			this.RFT_Address2TextBox.TabIndex = 7;

			// 
			// RFT_City
			// 
			this.BindingSource.SetBindingMember(this.RFT_CityTextBox, "RFT_City");
			this.RFT_CityTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("124c2693-b995-4ff1-9b71-5cdd123815b1", "City");
			this.RFT_CityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 120, true);
			this.RFT_CityTextBox.Name = "RFT_City";
			this.RFT_CityTextBox.ReadOnly = true;
			this.RFT_CityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.RFT_CityTextBox.TabIndex = 8;

			// 
			// RFT_StateTextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_StateTextBox, "RFT_StateCodeDesc");
			this.RFT_StateTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("124c2692-b995-4ff1-9b71-5cd5223825b1", "State");
			this.RFT_StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 140, true);
			this.RFT_StateTextBox.Name = "RFT_StateCodeDesc";
			this.RFT_StateTextBox.ReadOnly = true;
			this.RFT_StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.RFT_StateTextBox.TabIndex = 9;

			// 
			// RFT_PostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_PostCodeTextBox, "RFT_PostCode");
			this.RFT_PostCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("124c2692-b995-4ff1-9b71-5cd3123825b1", "Postcode");
			this.RFT_PostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 120, true);
			this.RFT_PostCodeTextBox.Name = "RFT_PostCode";
			this.RFT_PostCodeTextBox.ReadOnly = true;
			this.RFT_PostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
			this.RFT_PostCodeTextBox.TabIndex = 10;

			// 
			// RFT_CountryTextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_RN_NKCountryCodeTextBox, "RFT_CountryCodeDesc");
			this.RFT_RN_NKCountryCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("624c2642-b995-4ff1-9b71-5c45123825b1", "Country");
			this.RFT_RN_NKCountryCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 160, true);
			this.RFT_RN_NKCountryCodeTextBox.Name = "RFT_CountryCodeDesc";
			this.RFT_RN_NKCountryCodeTextBox.ReadOnly = true;
			this.RFT_RN_NKCountryCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.RFT_RN_NKCountryCodeTextBox.TabIndex = 11;

			// 
			// RFT_RL_NKLocationCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_RL_NKLocationCodeTextBox, "RFT_UNLOCODesc");
			this.RFT_RL_NKLocationCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("624c2642-b995-4ff1-9b71-5c55122825b1", "UNLOCO");
			this.RFT_RL_NKLocationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 180, true);
			this.RFT_RL_NKLocationCodeTextBox.Name = "RFT_UNLOCODesc";
			this.RFT_RL_NKLocationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
			this.RFT_RL_NKLocationCodeTextBox.TabIndex = 12;
			this.RFT_RL_NKLocationCodeTextBox.ReadOnly = true;

			// 
			// LatitudeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LatitudeTextBox, "RFT_GeoLocation.Latitude");
			this.LatitudeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("624c2641-b995-4ff1-9b71-5cd5126825b1", "Coordinates Latitude");
			this.LatitudeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 200, true);
			this.LatitudeTextBox.Name = "RFT_GeoLocation.Latitude";
			this.LatitudeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.LatitudeTextBox.TabIndex = 13;
			this.LatitudeTextBox.ReadOnly = true;

			// 
			// LongitudeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LongitudeTextBox, "RFT_GeoLocation.Longitude");
			this.LongitudeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("424c2642-b995-4ff1-9b71-5cd5722125b8", "Longitude");
			this.LongitudeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 200, true);
			this.LongitudeTextBox.Name = "RFT_GeoLocation.Longitude";
			this.LongitudeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
			this.LongitudeTextBox.TabIndex = 14;
			this.LongitudeTextBox.ReadOnly = true;

			// 
			// AdditionalDetailsGroupBox
			//

			this.AdditionalDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c724654b-657d-44f2-a9e5-992e8c3aacf4", "Additional Details");
			this.AdditionalDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 230, true);
			this.AdditionalDetailsGroupBox.Name = "AdditionalDetailsGroupBox";
			this.AdditionalDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 120, true);
			this.AdditionalDetailsGroupBox.TabIndex = 15;
			this.AdditionalDetailsGroupBox.TabStop = false;
			this.AdditionalDetailsGroupBox.Controls.Add(this.RFT_TerminalTypeTextBox);
			this.AdditionalDetailsGroupBox.Controls.Add(this.RFT_SMDGCodeTextBox);

			// RFT_TerminalTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_TerminalTypeTextBox, "RFT_TerminalType");
			this.RFT_TerminalTypeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("424b2642-b995-4ff1-9b71-5cd5711125b8", "Terminal Type");
			this.RFT_TerminalTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 20, true);
			this.RFT_TerminalTypeTextBox.Name = "RFT_TerminalType";
			this.RFT_TerminalTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.RFT_TerminalTypeTextBox.TabIndex = 16;
			this.RFT_TerminalTypeTextBox.ReadOnly = true;

			// RFT_SMDGCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RFT_SMDGCodeTextBox, "RFT_SMDGCode");
			this.RFT_SMDGCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("424c2622-b995-4ff1-9b41-5cd5722125b1", "SMDG Code");
			this.RFT_SMDGCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 40, true);
			this.RFT_SMDGCodeTextBox.Name = "RFT_SMDGCode";
			this.RFT_SMDGCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.RFT_SMDGCodeTextBox.TabIndex = 17;
			this.RFT_SMDGCodeTextBox.ReadOnly = true;

			// 
			// AvailableIntegrationGroupBox
			//

			this.AvailableIntegrationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c724654b-657d-44f2-a9e5-97ae5c3aacf4", "Available Integration");
			this.AvailableIntegrationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 230, true);
			this.AvailableIntegrationGroupBox.Name = "AvailableIntegrationGroupBox";
			this.AvailableIntegrationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 120, true);
			this.AvailableIntegrationGroupBox.TabIndex = 18;
			this.AvailableIntegrationGroupBox.TabStop = false;
			this.AvailableIntegrationGroupBox.Controls.Add(this.RFT_ContainerAutomationAvailableCheckBox);

			// 
			// Container Automation
			//
			this.RFT_ContainerAutomationAvailableCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RFT_ContainerAutomationAvailableCheckBox, "RFT_ContainerAutomationAvailable");
			this.RFT_ContainerAutomationAvailableCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b44c2691-b995-4ffb-4b77-5cd8123858bc", "Container Automation");
			this.RFT_ContainerAutomationAvailableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RFT_ContainerAutomationAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 20, true);
			this.RFT_ContainerAutomationAvailableCheckBox.Name = "RFT_ContainerAutomationAvailable";
			this.RFT_ContainerAutomationAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 16, true);
			this.RFT_ContainerAutomationAvailableCheckBox.TabIndex = 19;
			this.RFT_ContainerAutomationAvailableCheckBox.ReadOnly = true;

			// 
			// RefFacilityLocalCodesTabPage
			//
			this.RefFacilityLocalCodesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefFacilityForm|6a1b9437-3cb9-4994-b43b-c45374ac2428", "Local Codes");
			this.RefFacilityLocalCodesTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefFacilityLocalCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.RefFacilityLocalCodesTabPage.Name = "RefFacilityLocalCodesTabPage";
			this.RefFacilityLocalCodesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RefFacilityLocalCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 140, true);
			this.RefFacilityLocalCodesTabPage.TabIndex = 0;
			this.RefFacilityLocalCodesTabPage.UseVisualStyleBackColor = true;
			this.RefFacilityLocalCodesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RefFacilityLocalCodeTabPage_InitializeTab));
			this.MainTabPage.PerformLayout();
			this.FacilityCodeTabControl.ResumeLayout(false);
			this.FacilityCodeTabControl.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.AdditionalDetailsGroupBox.ResumeLayout(false);
			this.AdditionalDetailsGroupBox.PerformLayout();
			this.AvailableIntegrationGroupBox.ResumeLayout(false);
			this.AvailableIntegrationGroupBox.PerformLayout();
			this.RefFacilityLocalCodesTabPage.ResumeLayout(false);
			this.RefFacilityLocalCodesTabPage.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AvailableIntegrationGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AdditionalDetailsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl FacilityCodeTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage RefFacilityLocalCodesTabPage;
		private Enterprise.MasterFiles.GUI.RefFacilityLocalCodeControl RefFacilityLocalCodeControl;
		internal ZArchitecture.ZTextBox RFT_CodeTextBox;
		internal ZArchitecture.ZTextBox RFT_NameTextBox;
		internal ZArchitecture.ZTextBox RFT_Address1TextBox;
		internal ZArchitecture.ZTextBox RFT_Address2TextBox;
		internal ZArchitecture.ZTextBox RFT_CityTextBox;
		internal ZArchitecture.ZTextBox RFT_PostCodeTextBox;
		internal ZArchitecture.ZTextBox RFT_StateTextBox;
		internal ZArchitecture.ZTextBox RFT_RN_NKCountryCodeTextBox;
		internal ZArchitecture.ZTextBox RFT_RL_NKLocationCodeTextBox;
		internal ZArchitecture.ZTextBox LatitudeTextBox;
		internal ZArchitecture.ZTextBox LongitudeTextBox;
		internal ZArchitecture.ZTextBox RFT_TerminalTypeTextBox;
		internal ZArchitecture.ZTextBox RFT_SMDGCodeTextBox;
		internal ZArchitecture.ZTextBox RFT_FacilityTypeTextBox;
		internal ZArchitecture.GUI.ZCheckBox RFT_IsActiveCheckBox;
		internal ZArchitecture.GUI.ZCheckBox RFT_ContainerAutomationAvailableCheckBox;
	}
}
