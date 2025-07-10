namespace Enterprise.MasterFiles.GUI
{
	partial class MiscServiceFacilityUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.FacilityDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddressGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AvailableIntegrationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrgRefFacilityGrid = new Enterprise.MasterFiles.GUI.ZCusCodesGrid();

			this.FacilityTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityTerminalTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SMDGCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityAddress1TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityAddress2TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityStateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityCountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityUNLOCOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FacilityCityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ContainerAutomationAvailableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgRefFacilityGrid)).BeginInit();
			this.OrgRefFacilityGrid.SuspendLayout();
			this.AddressGroupBox.SuspendLayout();
			this.AvailableIntegrationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// FacilityDetailsGroupBox
			//
			this.FacilityDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.FacilityDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2c29B9f4-48E9-49C1-ADA5-1b8B10A6FD7E", "Details");
			this.FacilityDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 132, true);
			this.FacilityDetailsGroupBox.Name = "FacilityDetailsGroupBox";
			this.FacilityDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 155, true);
			this.FacilityDetailsGroupBox.TabIndex = 2;
			this.FacilityDetailsGroupBox.TabStop = false;
			this.FacilityDetailsGroupBox.Controls.Add(this.FacilityTypeTextBox);
			this.FacilityDetailsGroupBox.Controls.Add(this.FacilityCodeTextBox);
			this.FacilityDetailsGroupBox.Controls.Add(this.FacilityNameTextBox);
			this.FacilityDetailsGroupBox.Controls.Add(this.FacilityTerminalTypeTextBox);
			this.FacilityDetailsGroupBox.Controls.Add(this.SMDGCodeTextBox);
			// 
			// AddressGroupBox
			//
			this.AddressGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AddressGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2c29a9f4-48E5-49C1-ADA5-1a8B10A6FD7E", "Address");
			this.AddressGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 132, true);
			this.AddressGroupBox.Name = "AddressGroupBox";
			this.AddressGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 155, true);
			this.AddressGroupBox.TabIndex = 3;
			this.AddressGroupBox.TabStop = false;
			this.AddressGroupBox.Controls.Add(this.FacilityAddress1TextBox);
			this.AddressGroupBox.Controls.Add(this.FacilityAddress2TextBox);
			this.AddressGroupBox.Controls.Add(this.FacilityPostCodeTextBox);
			this.AddressGroupBox.Controls.Add(this.FacilityCityTextBox);
			this.AddressGroupBox.Controls.Add(this.FacilityStateTextBox);
			this.AddressGroupBox.Controls.Add(this.FacilityCountryTextBox);
			this.AddressGroupBox.Controls.Add(this.FacilityUNLOCOTextBox);
			// 
			// AvailableIntegrationGroupBox
			//
			this.AvailableIntegrationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AvailableIntegrationGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3c29a9f4-48E5-42C1-AbA1-1a8B10A6FD7b", "Available Integrations");
			this.AvailableIntegrationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 290, true);
			this.AvailableIntegrationGroupBox.Name = "AvailableIntegrationGroupBox";
			this.AvailableIntegrationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 50, true);
			this.AvailableIntegrationGroupBox.TabIndex = 4;
			this.AvailableIntegrationGroupBox.TabStop = false;
			this.AvailableIntegrationGroupBox.Controls.Add(this.ContainerAutomationAvailableCheckBox);
			// 
			// OrgRefFacilityGrid
			// 
			this.OrgRefFacilityGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgRefFacilityGrid, "OrgRefFacilities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).OFC_RFT_Facility)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).OFC_OA_PremisesAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_FacilityType)));

			this.OrgRefFacilityGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("206bb8bc-5c7d-46cd-968a-2f609185c523", "Code");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OFC_RFT_Facility";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("306bb8bc-5c7d-46cd-974a-2f609185c524", "Name");
			zTextBoxColumnStyleInfo1.ColumnName = "Facility.RFT_Name";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("606bb8bc-5c7d-46cb-978a-2f609285c424", "Type");
			zTextBoxColumnStyleInfo2.ColumnName = "Facility.RFT_FacilityType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("506bb81c-5c7d-46cb-818a-2f609183c424", "Address");
			zGuidDropEditColumnStyleInfo1.ColumnName = "OFC_OA_PremisesAddress";
			zGuidDropEditColumnStyleInfo1.ToolTip = "Select a physical address that this facility relates to.";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrgRefFacilityGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OrgRefFacilityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgRefFacilityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrgRefFacilityGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.OrgRefFacilityGrid.GridId = "1208e3b5-3235-40d4-8d8f-43f089d10b7b";
			this.OrgRefFacilityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgRefFacilityGrid.LayoutKey = "OrgRefFacilityGrid";
			this.OrgRefFacilityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 10, true);
			this.OrgRefFacilityGrid.Name = "OrgRefFacilityGrid";
			this.OrgRefFacilityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(676, 120, true);
			this.OrgRefFacilityGrid.TabIndex = 1;
			// 
			// FacilityTypeTextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityTypeTextBox, "OrgRefFacilities.Facility+RFT_FacilityTypeFullName");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_FacilityTypeFullName)));
			this.FacilityTypeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("206ba82c-5c7b-46cb-918a-2f609113c424", "Facility Type");
			this.FacilityTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 20, true);
			this.FacilityTypeTextBox.Name = "FacilityTypeTextBox";
			this.FacilityTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.FacilityTypeTextBox.TabIndex = 3;
			this.FacilityTypeTextBox.ReadOnly = true;
			// 
			// FacilityCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityCodeTextBox, "OrgRefFacilities.Facility+RFT_Code");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_Code)));
			this.FacilityCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("206ba82c-5c7b-46cb-918a-2f609773c428", "Code");
			this.FacilityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 42, true);
			this.FacilityCodeTextBox.Name = "FacilityCodeTextBox";
			this.FacilityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.FacilityCodeTextBox.TabIndex = 4;
			this.FacilityCodeTextBox.ReadOnly = true;
			// 
			// FacilityNameTextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityNameTextBox, "OrgRefFacilities.Facility+RFT_Name");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_Name)));
			this.FacilityNameTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("206ba82c-5c7b-46cb-908a-2f609773c428", "Name");
			this.FacilityNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 64, true);
			this.FacilityNameTextBox.Name = "FacilityNameTextBox";
			this.FacilityNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.FacilityNameTextBox.ReadOnly = true;
			this.FacilityNameTextBox.TabIndex = 5;
			// 
			// FacilityTerminalTypeTextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityTerminalTypeTextBox, "OrgRefFacilities.Facility+RFT_TerminalType");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_TerminalType)));
			this.FacilityTerminalTypeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("106ba02c-5c7b-46cb-918a-2f609773c424", "Terminal Type");
			this.FacilityTerminalTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 90, true);
			this.FacilityTerminalTypeTextBox.Name = "FacilityTerminalTypeTextBox";
			this.FacilityTerminalTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FacilityTerminalTypeTextBox.TabIndex = 6;
			this.FacilityTerminalTypeTextBox.ReadOnly = true;
			// 
			// SMDGCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.SMDGCodeTextBox, "OrgRefFacilities.Facility+RFT_SMDGCode");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_SMDGCode)));
			this.SMDGCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("106ba12c-5c7b-46cb-918a-2f600773b414", "SMDG Code");
			this.SMDGCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 112, true);
			this.SMDGCodeTextBox.Name = "SMDGCodeTextBox";
			this.SMDGCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SMDGCodeTextBox.TabIndex = 7;
			this.SMDGCodeTextBox.ReadOnly = true;
			// 
			// FacilityAddress1TextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityAddress1TextBox, "OrgRefFacilities.Facility+RFT_Address1");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_Address1)));
			this.FacilityAddress1TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("106ba12c-527b-46bb-918a-2f500773b412", "Address 1:");
			this.FacilityAddress1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 20, true);
			this.FacilityAddress1TextBox.Name = "FacilityAddressTextBox";
			this.FacilityAddress1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.FacilityAddress1TextBox.BorderStyle =System.Windows.Forms.BorderStyle.None;
			this.FacilityAddress1TextBox.TabIndex = 8;
			this.FacilityAddress1TextBox.ReadOnly = true;
			// 
			// FacilityAddress2TextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityAddress2TextBox, "OrgRefFacilities.Facility+RFT_Address2");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_Address2)));
			this.FacilityAddress2TextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("106ba12c-537b-46bb-918b-2f500773b442", "Address 2:");
			this.FacilityAddress2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 38, true);
			this.FacilityAddress2TextBox.Name = "FacilityAddress2TextBox";
			this.FacilityAddress2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 15, true);
			this.FacilityAddress2TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.FacilityAddress2TextBox.TabIndex = 9;
			this.FacilityAddress2TextBox.ReadOnly = true;
			// 
			// FacilityPostCodeTextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityPostCodeTextBox, "OrgRefFacilities.Facility+RFT_PostCode");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_PostCode)));
			this.FacilityPostCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("906ba12c-537b-46bb-918b-2f500772b442", "Postcode:");
			this.FacilityPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 60, true);
			this.FacilityPostCodeTextBox.Name = "FacilityPostCodeTextBox";
			this.FacilityPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.FacilityPostCodeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.FacilityPostCodeTextBox.TabIndex = 10;
			this.FacilityPostCodeTextBox.ReadOnly = true;
			// 
			// FacilityCityTextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityCityTextBox, "OrgRefFacilities.Facility+RFT_City");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_City)));
			this.FacilityCityTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("906ba12c-537b-46bb-918b-2f500571b442", "City:");
			this.FacilityCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 76, true);
			this.FacilityCityTextBox.Name = "FacilityCityTextBox";
			this.FacilityCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.FacilityCityTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.FacilityCityTextBox.TabIndex = 11;
			this.FacilityCityTextBox.ReadOnly = true;
			// 
			// FacilityStateTextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityStateTextBox, "OrgRefFacilities.Facility+RFT_StateCodeDesc");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_StateCodeDesc)));
			this.FacilityStateTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("906ba12c-537a-46bb-118b-2f500772b442", "State:");
			this.FacilityStateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 92, true);
			this.FacilityStateTextBox.Name = "FacilityStateTextBox";
			this.FacilityStateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
			this.FacilityStateTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.FacilityStateTextBox.TabIndex = 12;
			this.FacilityStateTextBox.ReadOnly = true;
			// 
			// FacilityCountryTextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityCountryTextBox, "OrgRefFacilities.Facility+RFT_CountryCodeDesc");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_CountryCodeDesc)));
			this.FacilityCountryTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("903ba12c-237a-46bb-118b-2f500772b442", "Country:");
			this.FacilityCountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 108, true);
			this.FacilityCountryTextBox.Name = "FacilityCountryTextBox";
			this.FacilityCountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
			this.FacilityCountryTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.FacilityCountryTextBox.TabIndex = 13;
			this.FacilityCountryTextBox.ReadOnly = true;
			// 
			// FacilityCountryTextBox
			//
			this.BindingSource.SetBindingMember(this.FacilityUNLOCOTextBox, "OrgRefFacilities.Facility+RFT_UNLOCODesc");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_UNLOCODesc)));
			this.FacilityUNLOCOTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("203bb12c-237a-41bb-118b-2f500772b442", "UNLOCO:");
			this.FacilityUNLOCOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 124, true);
			this.FacilityUNLOCOTextBox.Name = "FacilityUNLOCOTextBox";
			this.FacilityUNLOCOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 15, true);
			this.FacilityUNLOCOTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.FacilityUNLOCOTextBox.TabIndex = 13;
			this.FacilityUNLOCOTextBox.ReadOnly = true;
			// 
			// IsContainerAutomationCheckBox
			//
			this.BindingSource.SetBindingMember(this.ContainerAutomationAvailableCheckBox, "OrgRefFacilities.Facility+RFT_ContainerAutomationAvailable");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefFacility)(((Enterprise.MasterFiles.Business.OrgRefFacility)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgRefFacilities)).SyncRoot)).Facility)).RFT_ContainerAutomationAvailable)));
			this.ContainerAutomationAvailableCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("203bb12c-237a-41bb-018b-2f300572b441", "Container Automation");
			this.ContainerAutomationAvailableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 20, true);
			this.ContainerAutomationAvailableCheckBox.Name = "ContainerAutomationAvailableCheckBox";
			this.ContainerAutomationAvailableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ContainerAutomationAvailableCheckBox.TabIndex = 14;
			this.ContainerAutomationAvailableCheckBox.ReadOnly = true;
			// 
			// MiscServiceFacilityUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrgRefFacilityGrid);
			this.Controls.Add(this.FacilityDetailsGroupBox);
			this.Controls.Add(this.AddressGroupBox);
			this.Controls.Add(this.AvailableIntegrationGroupBox);
			this.Name = "MiscServiceFacilityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 778, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgRefFacilityGrid)).EndInit();
			this.OrgRefFacilityGrid.ResumeLayout(false);
			this.OrgRefFacilityGrid.PerformLayout();
			this.FacilityDetailsGroupBox.ResumeLayout(false);
			this.FacilityDetailsGroupBox.PerformLayout();
			this.AddressGroupBox.ResumeLayout(false);
			this.AddressGroupBox.PerformLayout();
			this.AvailableIntegrationGroupBox.ResumeLayout(false);
			this.AvailableIntegrationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.ZGrid OrgRefFacilityGrid;
		private ZArchitecture.GUI.ZGroupBox FacilityDetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox AddressGroupBox;
		private ZArchitecture.GUI.ZGroupBox AvailableIntegrationGroupBox;
		private ZArchitecture.ZTextBox FacilityTypeTextBox;
		private ZArchitecture.ZTextBox FacilityCodeTextBox;
		private ZArchitecture.ZTextBox FacilityNameTextBox;
		private ZArchitecture.ZTextBox FacilityTerminalTypeTextBox;
		private ZArchitecture.ZTextBox SMDGCodeTextBox;
		private ZArchitecture.ZTextBox FacilityAddress1TextBox;
		private ZArchitecture.ZTextBox FacilityAddress2TextBox;
		private ZArchitecture.ZTextBox FacilityPostCodeTextBox;
		private ZArchitecture.ZTextBox FacilityStateTextBox;
		private ZArchitecture.ZTextBox FacilityCountryTextBox;
		private ZArchitecture.ZTextBox FacilityUNLOCOTextBox;
		private ZArchitecture.ZTextBox FacilityCityTextBox;
		private ZArchitecture.GUI.ZCheckBox ContainerAutomationAvailableCheckBox;
		#endregion
	}
}
