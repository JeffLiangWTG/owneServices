namespace Enterprise.MasterFiles.GUI
{
	public partial class RefZoneHeaderForm
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

		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RR_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RR_DescriptionBoundTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.IncludedPortsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CarrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.FZ_ZoneTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FZ_ZoneModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.IncludedCountriesModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.IncludedPortsModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.IncludedCountriesModuleButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludedPortsModuleButtonGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 467, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 2;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(443);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(443);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefZoneHeader);
			// 
			// RR_CodeBoundTextBox
			// 
			this.RR_CodeBoundTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RR_CodeBoundTextBox, "FZ_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).FZ_Code)));
			this.RR_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 8, true);
			this.RR_CodeBoundTextBox.Name = "RR_CodeBoundTextBox";
			this.RR_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.RR_CodeBoundTextBox.TabIndex = 1;
			// 
			// RR_DescriptionBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.RR_DescriptionBoundTextBox, "FZ_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).FZ_Description)));
			this.RR_DescriptionBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RR_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 8, true);
			this.RR_DescriptionBoundTextBox.Name = "RR_DescriptionBoundTextBox";
			this.RR_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.RR_DescriptionBoundTextBox.TabIndex = 2;
			// 
			// IncludedPortsLabel
			// 
			this.IncludedPortsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefZoneHeaderForm|bb005648-b0fd-4386-a248-e81351a9de60", "Included Ports:");
			this.IncludedPortsLabel.IsFontBold = true;
			this.IncludedPortsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 58, true);
			this.IncludedPortsLabel.Name = "IncludedPortsLabel";
			this.IncludedPortsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.IncludedPortsLabel.TabIndex = 8;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(646, 438, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 1;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 424, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefZoneHeaderForm|7bf3faf2-f7ff-4028-be79-4c8d198bd7de", "Region");
			this.MainTabPage.Controls.Add(this.IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.CarrierGuidFindBox);
			this.MainTabPage.Controls.Add(this.FZ_ZoneTypeDropEdit);
			this.MainTabPage.Controls.Add(this.FZ_ZoneModeDropEdit);
			this.MainTabPage.Controls.Add(this.zLabel1);
			this.MainTabPage.Controls.Add(this.IncludedCountriesModuleButtonGrid);
			this.MainTabPage.Controls.Add(this.IncludedPortsLabel);
			this.MainTabPage.Controls.Add(this.RR_CodeBoundTextBox);
			this.MainTabPage.Controls.Add(this.RR_DescriptionBoundTextBox);
			this.MainTabPage.Controls.Add(this.IncludedPortsModuleButtonGrid);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 397, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// CarrierGuidFindBox
			// 
			this.CarrierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CarrierGuidFindBox, "FZ_OH_RelatedParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).FZ_OH_RelatedParty)));
			this.CarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 33, true);
			this.CarrierGuidFindBox.Name = "CarrierGuidFindBox";
			this.CarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 20, true);
			this.CarrierGuidFindBox.TabIndex = 8;
			// 
			// FZ_ZoneTypeDropEdit
			// 
			this.FZ_ZoneTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FZ_ZoneTypeDropEdit, "FZ_ZoneType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).FZ_ZoneType)));
			this.FZ_ZoneTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 33, true);
			this.FZ_ZoneTypeDropEdit.Name = "FZ_ZoneTypeDropEdit";
			this.FZ_ZoneTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.FZ_ZoneTypeDropEdit.TabIndex = 4;
			// 
			// FZ_ZoneModeDropEdit
			// 
			this.FZ_ZoneModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FZ_ZoneModeDropEdit, "FZ_ZoneMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).FZ_ZoneMode)));
			this.FZ_ZoneModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 33, true);
			this.FZ_ZoneModeDropEdit.Name = "FZ_ZoneModeDropEdit";
			this.FZ_ZoneModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 20, true);
			this.FZ_ZoneModeDropEdit.TabIndex = 6;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefZoneHeaderForm|a1ebfeb0-bb0e-44dd-b460-1758d83272cc", "Included Countries/Regions:");
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 58, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 23, true);
			this.zLabel1.TabIndex = 10;
			// 
			// IncludedCountriesModuleButtonGrid
			// 
			this.IncludedCountriesModuleButtonGrid.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncludedCountriesModuleButtonGrid, "Countries");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).Countries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).Lookups.Countries)));
			this.IncludedCountriesModuleButtonGrid.BindToFindBoxList = "Lookups+Countries";
			zTextBoxColumnStyleInfo1.ColumnName = "RN_Code";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "RN_Desc";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.IncludedCountriesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IncludedCountriesModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.IncludedCountriesModuleButtonGrid.DetachMessage = Enterprise.MasterFiles.GUI.Res.GetData("CA2DBF5E-46BB-4984-BA15-57D60F7A7231", "Are you sure you want to detach the selected country/region?");
			this.IncludedCountriesModuleButtonGrid.GridId = "30c8c6d3-aeda-425e-867d-e59e6fbe45f8";
			// 
			// 
			// 
			this.IncludedCountriesModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.IncludedCountriesModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.IncludedCountriesModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.IncludedCountriesModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.IncludedCountriesModuleButtonGrid.InnerGrid.GridId = null;
			this.IncludedCountriesModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IncludedCountriesModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.IncludedCountriesModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.IncludedCountriesModuleButtonGrid.InnerGrid.Name = "Grid";
			this.IncludedCountriesModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.IncludedCountriesModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 250, true);
			this.IncludedCountriesModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.IncludedCountriesModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 82, true);
			this.IncludedCountriesModuleButtonGrid.Name = "IncludedCountriesModuleButtonGrid";
			this.IncludedCountriesModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("B9800BF1-7B78-49AA-98D7-E21119C4ECFB", "Country/Region");
			this.IncludedCountriesModuleButtonGrid.ReadOnly = false;
			this.IncludedCountriesModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 288, true);
			this.IncludedCountriesModuleButtonGrid.TabIndex = 11;
			// 
			// IncludedPortsModuleButtonGrid
			// 
			this.IncludedPortsModuleButtonGrid.AllowDrop = true;
			this.IncludedPortsModuleButtonGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.IncludedPortsModuleButtonGrid, "UNLOCOs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).UNLOCOs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).Lookups.UNLOCOs)));
			this.IncludedPortsModuleButtonGrid.BindToFindBoxList = "Lookups+UNLOCOs";
			zTextBoxColumnStyleInfo3.ColumnName = "RL_Code";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ColumnName = "RL_PortName";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.ColumnName = "RL_NameWithDiacriticals";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.ColumnName = "RL_IATA";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "RL_RN_NKCountryCode";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.ColumnName = "CoordinateText";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "RL_HasAirport";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo2.ColumnName = "RL_HasBorderCrossing";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo3.ColumnName = "RL_HasCustomsLodge";
			zCheckBoxColumnStyleInfo3.IsReadOnly = true;
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo4.ColumnName = "RL_HasDischarge";
			zCheckBoxColumnStyleInfo4.IsReadOnly = true;
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo5.ColumnName = "RL_HasPost";
			zCheckBoxColumnStyleInfo5.IsReadOnly = true;
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo6.ColumnName = "RL_HasOutport";
			zCheckBoxColumnStyleInfo6.IsReadOnly = true;
			zCheckBoxColumnStyleInfo6.IsVisible = false;
			zCheckBoxColumnStyleInfo7.ColumnName = "RL_HasRail";
			zCheckBoxColumnStyleInfo7.IsReadOnly = true;
			zCheckBoxColumnStyleInfo7.IsVisible = false;
			zCheckBoxColumnStyleInfo8.ColumnName = "RL_HasRoad";
			zCheckBoxColumnStyleInfo8.IsReadOnly = true;
			zCheckBoxColumnStyleInfo8.IsVisible = false;
			zCheckBoxColumnStyleInfo9.ColumnName = "RL_HasSeaport";
			zCheckBoxColumnStyleInfo9.IsReadOnly = true;
			zCheckBoxColumnStyleInfo9.IsVisible = false;
			zCheckBoxColumnStyleInfo10.ColumnName = "RL_HasStore";
			zCheckBoxColumnStyleInfo10.IsReadOnly = true;
			zCheckBoxColumnStyleInfo10.IsVisible = false;
			zCheckBoxColumnStyleInfo11.ColumnName = "RL_HasTerminal";
			zCheckBoxColumnStyleInfo11.IsReadOnly = true;
			zCheckBoxColumnStyleInfo11.IsVisible = false;
			zCheckBoxColumnStyleInfo12.ColumnName = "RL_HasUnload";
			zCheckBoxColumnStyleInfo12.IsReadOnly = true;
			zCheckBoxColumnStyleInfo12.IsVisible = false;
			zCheckBoxColumnStyleInfo13.ColumnName = "RL_IsActive";
			zCheckBoxColumnStyleInfo13.IsReadOnly = true;
			zCheckBoxColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|528a9261-2bf7-49d9-8790-303206294ce7", "GMT Offset");
			zCalcEditColumnStyleInfo1.ColumnName = "StandardZoneUTCOffset";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|166860cc-4d3b-4854-a97d-19a84382d107", "State Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "RL_RW";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|1bde09d5-c302-4d40-9f1f-65528a79f6f5", "State");
			zTextBoxColumnStyleInfo8.ColumnName = "StateDescription";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo11);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo12);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo13);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.IncludedPortsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.IncludedPortsModuleButtonGrid.DetachMessage = Enterprise.MasterFiles.GUI.Res.GetData("61CE388E-E343-4144-A8D0-741EDF580F92", "Are you sure you want to detach the selected UNLOCO?");
			this.IncludedPortsModuleButtonGrid.GridId = "a508338b-7e51-442e-a4a5-180b35fff9b2";
			// 
			// 
			// 
			this.IncludedPortsModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.IncludedPortsModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.IncludedPortsModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.IncludedPortsModuleButtonGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.IncludedPortsModuleButtonGrid.InnerGrid.GridId = null;
			this.IncludedPortsModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.IncludedPortsModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.IncludedPortsModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.IncludedPortsModuleButtonGrid.InnerGrid.Name = "Grid";
			this.IncludedPortsModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.IncludedPortsModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 250, true);
			this.IncludedPortsModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.IncludedPortsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 82, true);
			this.IncludedPortsModuleButtonGrid.Name = "IncludedPortsModuleButtonGrid";
			this.IncludedPortsModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("3F695792-B265-499E-B91C-710AF58138B8", "UNLOCO");
			this.IncludedPortsModuleButtonGrid.ReadOnly = false;
			this.IncludedPortsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 288, true);
			this.IncludedPortsModuleButtonGrid.TabIndex = 9;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 397, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(872, 397, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "FZ_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefZoneHeader)(null)).FZ_IsActive)));
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(750, 3, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.IsActiveCheckBox.TabIndex = 3;
			this.IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// RefZoneHeaderForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefZoneHeaderForm|e6495d8e-065c-4869-b782-6ea648ad531f", "Zone");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(886, 491, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefZoneHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 521, true);
			this.Name = "RefZoneHeaderForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.IncludedCountriesModuleButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.IncludedPortsModuleButtonGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		private Enterprise.ZArchitecture.ZTextBox RR_CodeBoundTextBox;
		private Enterprise.ZArchitecture.ZLabel IncludedPortsLabel;
		private Enterprise.ZArchitecture.ZTranslatableTextControl RR_DescriptionBoundTextBox;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CarrierGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit FZ_ZoneTypeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit FZ_ZoneModeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid IncludedCountriesModuleButtonGrid;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid IncludedPortsModuleButtonGrid;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
	}
}
