using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ConsigneeWizardPage : WizardPage
	{
		private ZArchitecture.ZTextBox ShipmentHouseBillTextBox;
		private ZGroupBox groupBox1;
		private ZDisplayGrid SimilarOrgMatchesBoundGrid;
		private ZArchitecture.ZLabel SimilarOrganisationsLabel;
		private ZArchitecture.ZLabel zLabel8;
		private MasterFiles.GUI.ZOrganisationFindBox CoLoaderOrganisationFindBox1;
		private ZArchitecture.ZTextBox CW_TempOrgNameTextBox1;
		private ZArchitecture.ZTextBox CW_TempOrgAdress1TextBox2;
		private ZArchitecture.ZTextBox CW_TempOrgAdress2TextBox3;
		private ZArchitecture.ZTextBox StateTextBox;
		private ZArchitecture.ZTextBox CW_TempOrgPostCodeTextBox;
		private ZArchitecture.ZTextBox CW_TempOrgCityTextBox;
		private ZCodeFindBox CW_TempOrgUNLOCOCodeFindBox;
		private ZButton ClearButton;
		private System.ComponentModel.Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CoLoaderOrganisationFindBox1 = new MasterFiles.GUI.ZOrganisationFindBox();
			this.ShipmentHouseBillTextBox = new ZArchitecture.ZTextBox();
			this.groupBox1 = new ZGroupBox();
			this.CW_TempOrgNameTextBox1 = new ZArchitecture.ZTextBox();
			this.CW_TempOrgAdress1TextBox2 = new ZArchitecture.ZTextBox();
			this.CW_TempOrgAdress2TextBox3 = new ZArchitecture.ZTextBox();
			this.StateTextBox = new ZArchitecture.ZTextBox();
			this.CW_TempOrgPostCodeTextBox = new ZArchitecture.ZTextBox();
			this.SimilarOrgMatchesBoundGrid = new ZDisplayGrid();
			this.CW_TempOrgCityTextBox = new ZArchitecture.ZTextBox();
			this.SimilarOrganisationsLabel = new ZArchitecture.ZLabel();
			this.zLabel8 = new ZArchitecture.ZLabel();
			this.CW_TempOrgUNLOCOCodeFindBox = new ZCodeFindBox();
			this.ClearButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgMatchesBoundGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CoLoadWizardShipment);
			// 
			// CoLoaderOrganisationFindBox1
			// 
			this.BindingSource.SetBindingMember(this.CoLoaderOrganisationFindBox1, "CW_OH_CoLoadForwarder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CoLoadWizardShipment)(null)).CW_OH_CoLoadForwarder)));
			this.CoLoaderOrganisationFindBox1.CaptionResourceString = Res.GetData("ConsigneeWizardPage|47592e87-da09-4106-b8a6-43e161075455", "Co-Loader");
			this.CoLoaderOrganisationFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 8, true);
			this.CoLoaderOrganisationFindBox1.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(ZArchitecture.Modules.ModuleIDs.Organisation));
			this.CoLoaderOrganisationFindBox1.Name = "CoLoaderOrganisationFindBox1";
			this.CoLoaderOrganisationFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CoLoaderOrganisationFindBox1.TabIndex = 0;
			// 
			// ShipmentHouseBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShipmentHouseBillTextBox, "CW_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CoLoadWizardShipment)(null)).CW_HouseBill)));
			this.ShipmentHouseBillTextBox.CaptionResourceString = Res.GetData("ConsigneeWizardPage|6ad7a12b-4da4-454d-b44b-1d7568e55a3b", "House bill");
			this.ShipmentHouseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 32, true);
			this.ShipmentHouseBillTextBox.Name = "ShipmentHouseBillTextBox";
			this.ShipmentHouseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.ShipmentHouseBillTextBox.TabIndex = 1;
			// 
			// groupBox1
			// 
			this.groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 53, true);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 8, true);
			this.groupBox1.TabIndex = 9;
			this.groupBox1.TabStop = false;
			// 
			// CW_TempOrgNameTextBox1
			// 
			this.BindingSource.SetBindingMember(this.CW_TempOrgNameTextBox1, "CW_TempOrgName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CoLoadWizardShipment)(null)).CW_TempOrgName)));
			this.CW_TempOrgNameTextBox1.CaptionResourceString = Res.GetData("ConsigneeWizardPage|a712ec67-91b1-4d38-b38a-86aed5211911", "Name");
			this.CW_TempOrgNameTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 80, true);
			this.CW_TempOrgNameTextBox1.Name = "CW_TempOrgNameTextBox1";
			this.CW_TempOrgNameTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CW_TempOrgNameTextBox1.TabIndex = 2;
			// 
			// CW_TempOrgAdress1TextBox2
			// 
			this.BindingSource.SetBindingMember(this.CW_TempOrgAdress1TextBox2, "CW_TempOrgAddress1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CoLoadWizardShipment)(null)).CW_TempOrgAddress1)));
			this.CW_TempOrgAdress1TextBox2.CaptionResourceString = Res.GetData("ConsigneeWizardPage|8934988e-3171-4df6-90f5-b6b9f493ddaf", "Address");
			this.CW_TempOrgAdress1TextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 104, true);
			this.CW_TempOrgAdress1TextBox2.Name = "CW_TempOrgAdress1TextBox2";
			this.CW_TempOrgAdress1TextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CW_TempOrgAdress1TextBox2.TabIndex = 3;
			// 
			// CW_TempOrgAdress2TextBox3
			// 
			this.BindingSource.SetBindingMember(this.CW_TempOrgAdress2TextBox3, "CW_TempOrgAddress2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CoLoadWizardShipment)(null)).CW_TempOrgAddress2)));
			this.CW_TempOrgAdress2TextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 128, true);
			this.CW_TempOrgAdress2TextBox3.Name = "CW_TempOrgAdress2TextBox3";
			this.CW_TempOrgAdress2TextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CW_TempOrgAdress2TextBox3.TabIndex = 4;
			// 
			// StateTextBox
			// 
			this.BindingSource.SetBindingMember(this.StateTextBox, "CW_TempOrgState");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CoLoadWizardShipment)(null)).CW_TempOrgState)));
			this.StateTextBox.CaptionResourceString = Res.GetData("ConsigneeWizardPage|ea8ea2e6-bfcb-4da8-8ebe-601f90cf0c9d", "State");
			this.StateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 152, true);
			this.StateTextBox.Name = "StateTextBox";
			this.StateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.StateTextBox.TabIndex = 6;
			// 
			// CW_TempOrgPostCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CW_TempOrgPostCodeTextBox, "CW_TempOrgPostcode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CoLoadWizardShipment)(null)).CW_TempOrgPostcode)));
			this.CW_TempOrgPostCodeTextBox.CaptionResourceString = Res.GetData("ConsigneeWizardPage|5975b5e5-af4b-4cc1-8995-660ec4b86f9b", "Postcode");
			this.CW_TempOrgPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 176, true);
			this.CW_TempOrgPostCodeTextBox.Name = "CW_TempOrgPostCodeTextBox";
			this.CW_TempOrgPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.CW_TempOrgPostCodeTextBox.TabIndex = 7;
			// 
			// SimilarOrgMatchesBoundGrid
			// 
			this.SimilarOrgMatchesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SimilarOrgMatchesBoundGrid, "SimilarOrganisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OS_Rank)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OS_UNLOCO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_Calc_Address1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_Calc_Address2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_Calc_City)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_Calc_State)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_Calc_PostCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_Calc_Phone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_Calc_Fax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).OH_Calc_Email)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.OrgPatternMatch)(((System.Collections.IList)(((CoLoadWizardShipment)(null)).SimilarOrganisations)).SyncRoot)).LocalBusinessNumber)));
			this.SimilarOrgMatchesBoundGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OS_Rank";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo2.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "OS_UNLOCO";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.ColumnName = "OH_Calc_Address1";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "OH_Calc_Address2";
			zTextBoxColumnStyleInfo6.ColumnName = "OH_Calc_City";
			zTextBoxColumnStyleInfo7.ColumnName = "OH_Calc_State";
			zTextBoxColumnStyleInfo8.ColumnName = "OH_Calc_PostCode";
			zTextBoxColumnStyleInfo9.ColumnName = "OH_Calc_Phone";
			zTextBoxColumnStyleInfo10.ColumnName = "OH_Calc_Fax";
			zTextBoxColumnStyleInfo11.ColumnName = "OH_Calc_Email";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Res.GetData("ConsigneeWizardPage|1cb91048-2427-4e2d-bdc1-9dd404f6a7fd", "Reg. #");
			zTextBoxColumnStyleInfo12.ColumnName = "LocalBusinessNumber";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.SimilarOrgMatchesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.SimilarOrgMatchesBoundGrid.Dock = DockStyle.Bottom;
			this.SimilarOrgMatchesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SimilarOrgMatchesBoundGrid.IsWholeRowSelectedOnClick = true;
			this.SimilarOrgMatchesBoundGrid.LayoutKey = "zGrid1";
			this.SimilarOrgMatchesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 232, true);
			this.SimilarOrgMatchesBoundGrid.Name = "SimilarOrgMatchesBoundGrid";
			this.SimilarOrgMatchesBoundGrid.RemoveAction = ZArchitecture.RemoveAction.NoRemovePossible;
			this.SimilarOrgMatchesBoundGrid.ShouldSetErrorsOnTabPage = false;
			this.SimilarOrgMatchesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 128, true);
			this.SimilarOrgMatchesBoundGrid.TabIndex = 9;
			this.SimilarOrgMatchesBoundGrid.DoubleClick += new EventHandler(this.SimilarOrgMatchesBoundGrid_DoubleClick);
			// 
			// CW_TempOrgCityTextBox
			// 
			this.BindingSource.SetBindingMember(this.CW_TempOrgCityTextBox, "CW_TempOrgCity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CoLoadWizardShipment)(null)).CW_TempOrgCity)));
			this.CW_TempOrgCityTextBox.CaptionResourceString = Res.GetData("ConsigneeWizardPage|9864a9d9-c290-43a1-ba17-962b0429335a", "City");
			this.CW_TempOrgCityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 152, true);
			this.CW_TempOrgCityTextBox.Name = "CW_TempOrgCityTextBox";
			this.CW_TempOrgCityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.CW_TempOrgCityTextBox.TabIndex = 5;
			// 
			// SimilarOrganisationsLabel
			// 
			this.SimilarOrganisationsLabel.AutoSize = true;
			this.SimilarOrganisationsLabel.CaptionResourceString = Res.GetData("ConsigneeWizardPage|e8ea2471-cdb8-4b22-8be2-0c307c1d6c9b", "Similar Organizations:");
			this.SimilarOrganisationsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 204, true);
			this.SimilarOrganisationsLabel.Name = "SimilarOrganisationsLabel";
			this.SimilarOrganisationsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.SimilarOrganisationsLabel.TabIndex = 28;
			// 
			// zLabel8
			// 
			this.zLabel8.AutoSize = true;
			this.zLabel8.CaptionResourceString = Res.GetData("ConsigneeWizardPage|6da2db06-c377-4ff5-b002-44fb625da30c", "Consignee");
			this.zLabel8.IsFontBold = true;
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 64, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.zLabel8.TabIndex = 29;
			// 
			// CW_TempOrgUNLOCOCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.CW_TempOrgUNLOCOCodeFindBox, "CW_TempOrgUNLOCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CoLoadWizardShipment)(null)).CW_TempOrgUNLOCO)));
			this.CW_TempOrgUNLOCOCodeFindBox.CaptionResourceString = Res.GetData("ConsigneeWizardPage|a6b8bc12-9e97-4ec4-96d8-a6b1758fb96f", "UNLOCO");
			this.CW_TempOrgUNLOCOCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 176, true);
			this.CW_TempOrgUNLOCOCodeFindBox.Name = "CW_TempOrgUNLOCOCodeFindBox";
			this.CW_TempOrgUNLOCOCodeFindBox.PreBoundMaxLength = 5;
			this.CW_TempOrgUNLOCOCodeFindBox.ShowDescriptionBox = false;
			this.CW_TempOrgUNLOCOCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 20, true);
			this.CW_TempOrgUNLOCOCodeFindBox.TabIndex = 8;
			// 
			// ClearButton
			// 
			this.ClearButton.CaptionResourceString = Res.GetData("ConsigneeWizardPage|a65b0b93-db9c-464d-953e-53b6fcedd308", "Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 200, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ClearButton.TabIndex = 31;
			this.ClearButton.Click += new EventHandler(this.ClearButton_Click);
			// 
			// ConsigneeWizardPage
			// 
			this.Controls.Add(this.ClearButton);
			this.Controls.Add(this.SimilarOrgMatchesBoundGrid);
			this.Controls.Add(this.CW_TempOrgUNLOCOCodeFindBox);
			this.Controls.Add(this.zLabel8);
			this.Controls.Add(this.SimilarOrganisationsLabel);
			this.Controls.Add(this.CW_TempOrgCityTextBox);
			this.Controls.Add(this.CW_TempOrgPostCodeTextBox);
			this.Controls.Add(this.StateTextBox);
			this.Controls.Add(this.CW_TempOrgAdress2TextBox3);
			this.Controls.Add(this.CW_TempOrgAdress1TextBox2);
			this.Controls.Add(this.CW_TempOrgNameTextBox1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.ShipmentHouseBillTextBox);
			this.Controls.Add(this.CoLoaderOrganisationFindBox1);
			this.Name = "ConsigneeWizardPage";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 360, true);
			this.DoubleClick += new EventHandler(this.SimilarOrgMatchesBoundGrid_DoubleClick);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SimilarOrgMatchesBoundGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
