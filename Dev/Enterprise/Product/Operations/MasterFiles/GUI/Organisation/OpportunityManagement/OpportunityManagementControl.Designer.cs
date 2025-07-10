namespace Enterprise.MasterFiles.GUI
{
	partial class OpportunityManagementControl
	{
		#region Component Designer generated code

		protected internal Enterprise.MasterFiles.GUI.TasksControl OppTasksControl;
		private Enterprise.ZArchitecture.ZLabel TasksLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OpportunitiesFilterGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit FromDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ToDateEdit;
		private Enterprise.ZArchitecture.GUI.ZButton ClearButton;
		private Enterprise.ZArchitecture.GUI.ZButton FindButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit DateTypeDropEdit;
		private System.ComponentModel.Container components = null;
		protected internal Enterprise.ZArchitecture.GUI.ZModuleButtonGrid OpportunitiesGrid;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.TasksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OpportunitiesFilterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DateTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FindButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OppTasksControl = new Enterprise.MasterFiles.GUI.TasksControl();
			this.OpportunitiesGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OpportunitiesFilterGroupBox.SuspendLayout();
			this.DateTypeDropEdit.SuspendLayout();
			this.ToDateEdit.SuspendLayout();
			this.FromDateEdit.SuspendLayout();
			this.OppTasksControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OpportunitiesGrid.InnerGrid)).BeginInit();
			this.OpportunitiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// TasksLabel
			// 
			this.TasksLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementControl|06536489-31f1-46c9-8f14-c52868c74e92", "Tasks");
			this.TasksLabel.IsFontBold = true;
			this.TasksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 318, true);
			this.TasksLabel.Name = "TasksLabel";
			this.TasksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 16, true);
			this.TasksLabel.TabIndex = 2;
			// 
			// OpportunitiesFilterGroupBox
			// 
			this.OpportunitiesFilterGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementControl|b84cfa90-cb71-47f9-aa2f-1a2832782fe1", "Opportunities Filter");
			this.OpportunitiesFilterGroupBox.Controls.Add(this.DateTypeDropEdit);
			this.OpportunitiesFilterGroupBox.Controls.Add(this.ClearButton);
			this.OpportunitiesFilterGroupBox.Controls.Add(this.FindButton);
			this.OpportunitiesFilterGroupBox.Controls.Add(this.ToDateEdit);
			this.OpportunitiesFilterGroupBox.Controls.Add(this.FromDateEdit);
			this.OpportunitiesFilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.OpportunitiesFilterGroupBox.Name = "OpportunitiesFilterGroupBox";
			this.OpportunitiesFilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 44, true);
			this.OpportunitiesFilterGroupBox.TabIndex = 0;
			this.OpportunitiesFilterGroupBox.TabStop = false;
			// 
			// DateTypeDropEdit
			// 
			this.DateTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DateTypeDropEdit, "OpportunitiesDateTypeToFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OpportunitiesDateTypeToFilter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OpportunitiesDateTypeToFilter)));
			this.DateTypeDropEdit.BindToForDescription = "OpportunitiesDateTypeToFilter";
			this.DateTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementControl|03717c9b-63c4-468a-8b18-889f07b13464", "Date", "Date Type", "");
			this.DateTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 17, true);
			this.DateTypeDropEdit.Name = "DateTypeDropEdit";
			this.DateTypeDropEdit.ShowDescriptionBox = false;
			this.DateTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DateTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.DateTypeDropEdit.TabIndex = 0;
			// 
			// ClearButton
			// 
			this.ClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ClearButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementControl|8de25e3d-04a5-43d3-9d2e-ea5808fd6aaf", "&Clear");
			this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(714, 15, true);
			this.ClearButton.Name = "ClearButton";
			this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.ClearButton.TabIndex = 4;
			this.ClearButton.UseVisualStyleBackColor = true;
			this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
			// 
			// FindButton
			// 
			this.FindButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.FindButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementControl|47ab355e-3ac3-4447-bc53-bb6b56fa0aa4", "&Find");
			this.FindButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(633, 15, true);
			this.FindButton.Name = "FindButton";
			this.FindButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.FindButton.TabIndex = 3;
			this.FindButton.UseVisualStyleBackColor = true;
			this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
			// 
			// ToDateEdit
			// 
			this.ToDateEdit.AllowDrop = true;
			this.ToDateEdit.AutoCompleteMonthThreshold = 1;
			this.ToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ToDateEdit, "OpportunityDateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OpportunityDateTo)));
			this.ToDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementControl|49b7c0ed-7965-4c66-82cd-8f6d91f138fe", "To", "To Date.");
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 17, true);
			this.ToDateEdit.Name = "ToDateEdit";
			this.ToDateEdit.TabIndex = 2;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateEdit, "OpportunityDateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OpportunityDateFrom)));
			this.FromDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OpportunityManagementControl|d8e9c8c4-bb60-4de8-861d-ea49d28509e9", "From", "From Date.");
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(237, 17, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.TabIndex = 1;
			// 
			// OppTasksControl
			// 
			this.OppTasksControl.AllowDrop = true;
			this.OppTasksControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OppTasksControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)))));
			this.OppTasksControl.BindTo = "SalesOpportunities.WorkflowItems";
			this.OppTasksControl.CreateTasksFromTemplateLinkVisible = true;
			this.OppTasksControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 335, true);
			this.OppTasksControl.Name = "OppTasksControl";
			this.OppTasksControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 105, true);
			this.OppTasksControl.TabIndex = 3;
			// 
			// OpportunitiesGrid
			// 
			this.OpportunitiesGrid.AllowDrop = true;
			this.OpportunitiesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OpportunitiesGrid, "SalesOpportunities");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SalesOpportunities)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).Lookups.Opportunities)));
			this.OpportunitiesGrid.BindToFindBoxList = "Lookups+Opportunities";
			zTextBoxColumnStyleInfo1.ColumnName = "P8_OpportunityID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "P8_OpportunityDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d6bec78e-5d75-41fd-aef5-e39a75bba9ca", "Sales Type");
			zDropEditColumnStyleInfo1.ColumnName = "P8_OpportunityType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|46f2f724-3333-4afd-bbea-eb30acd20521", "Sales Type Description");
			zTextBoxColumnStyleInfo3.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zDropEditColumnStyleInfo2.ColumnName = "P8_Source";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|312e7ea8-7955-4038-8ecd-c915d178c785", "Source Description");
			zTextBoxColumnStyleInfo4.ColumnName = "SourceDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.BindToList = "Lookups.ActiveSourceDetails";
			zMultiControlColumnStyleInfo1.ColumnName = "SourceDetails";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "SourceDetailsDataFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "P8_G0";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "P8_GS_NKPrimarySalesPerson";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|93b611c6-9c4a-4247-a4d5-830bc66db820", "Sales Person Name");
			zTextBoxColumnStyleInfo5.ColumnName = "SalesPersonName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo3.ColumnName = "P8_Outcome";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|0836bc03-b132-4ae7-b557-09a092dd24f7", "Outcome Description");
			zTextBoxColumnStyleInfo6.ColumnName = "OutcomeDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo4.ColumnName = "P8_Status";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|52cee761-80a3-4222-bb9a-870ce62d5401", "Status Description");
			zTextBoxColumnStyleInfo7.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo5.ColumnName = "P8_Stage";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|03d39740-e606-4f1b-99c4-640fa3efdb24", "Stage Description");
			zTextBoxColumnStyleInfo8.ColumnName = "StageDescription";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zDropEditColumnStyleInfo6.ColumnName = "P8_PackageType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo1.ColumnName = "P8_OA";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidDropEditColumnStyleInfo2.ColumnName = "P8_OC";
			zGuidDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "P8_EstimatedValue";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|24dc0aad-2e95-471e-a1e5-0451963ba967", "Estimated Value");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "P8_DiscountAmount";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "P8_RentalMultiplier";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "P8_RX_NKEstimatedValueCurrency";
			zCodeFindBoxColumnStyleInfo2.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|24dc0aad-2e95-471e-a1e5-0451963ba967", "Estimated Value");
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "P8_ClosedDateLocal";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "P8_EstimatedCloseDateLocal";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ZModuleButtonGrid|f9c24287-17ec-4899-8e2d-500bea8a2be9", "Close Certainty");
			zDropEditColumnStyleInfo7.ColumnName = "CloseCertaintyAsPercentageString";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo8.ColumnName = "P8_LostReason";
			zDropEditColumnStyleInfo8.IsVisible = false;
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.ColumnName = "P8_RecallDateLocal";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OpportunitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OpportunitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OpportunitiesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OpportunitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OpportunitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.OpportunitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.OpportunitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.OpportunitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.OpportunitiesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.OpportunitiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OpportunitiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.OpportunitiesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.OpportunitiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OpportunitiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OpportunitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.OpportunitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.OpportunitiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.OpportunitiesGrid.GridId = "cba5c5c2-99ac-4e01-9203-dcbecab3d34e";
			// 
			// 
			// 
			this.OpportunitiesGrid.InnerGrid.AllowNavigation = false;
			this.OpportunitiesGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OpportunitiesGrid.InnerGrid.CaptionVisible = false;
			this.OpportunitiesGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.OpportunitiesGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OpportunitiesGrid.InnerGrid.LayoutKey = "Grid";
			this.OpportunitiesGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.OpportunitiesGrid.InnerGrid.Name = "Grid";
			this.OpportunitiesGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 246, true);
			this.OpportunitiesGrid.InnerGrid.TabIndex = 0;
			this.OpportunitiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 51, true);
			this.OpportunitiesGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Opportunity;
			this.OpportunitiesGrid.Name = "OpportunitiesGrid";
			this.OpportunitiesGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("CED16A82-4E3D-430E-AD08-BE876A694E31", "Opportunity");
			this.OpportunitiesGrid.ReadOnly = false;
			this.OpportunitiesGrid.ShowAttachButton = false;
			this.OpportunitiesGrid.ShowDetachButton = false;
			this.OpportunitiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 265, true);
			this.OpportunitiesGrid.TabIndex = 1;
			// 
			// OpportunityManagementControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OppTasksControl);
			this.Controls.Add(this.TasksLabel);
			this.Controls.Add(this.OpportunitiesGrid);
			this.Controls.Add(this.OpportunitiesFilterGroupBox);
			this.Name = "OpportunityManagementControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 448, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OpportunitiesFilterGroupBox.ResumeLayout(false);
			this.OpportunitiesFilterGroupBox.PerformLayout();
			this.DateTypeDropEdit.ResumeLayout(true);
			this.DateTypeDropEdit.PerformLayout();
			this.ToDateEdit.ResumeLayout(true);
			this.ToDateEdit.PerformLayout();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.OppTasksControl.ResumeLayout(true);
			this.OppTasksControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OpportunitiesGrid.InnerGrid)).EndInit();
			this.OpportunitiesGrid.ResumeLayout(true);
			this.OpportunitiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
