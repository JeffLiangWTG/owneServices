namespace Enterprise.MasterFiles.GUI
{
	partial class NumberRangesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.RangesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RangesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NumberFountainsButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NumberFountainsDeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NumberFountainsEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NumberFountainsAddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NumberRangesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.RangesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MatchingDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MatchingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MatchingDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RangesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RangesGrid)).BeginInit();
			this.RangesGrid.SuspendLayout();
			this.NumberFountainsButtonsPanel.SuspendLayout();
			this.NumberRangesTabControl.SuspendLayout();
			this.RangesTabPage.SuspendLayout();
			this.MatchingDetailsTabPage.SuspendLayout();
			this.MatchingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MatchingDetailsGrid)).BeginInit();
			this.MatchingDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.IViewStmNumsOwner);
			// 
			// RangesGroupBox
			// 
			this.RangesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AFB5025B-E9D0-4402-A954-D7B3CEBE3613", "Number Ranges");
			this.RangesGroupBox.Controls.Add(this.RangesGrid);
			this.RangesGroupBox.Controls.Add(this.NumberFountainsButtonsPanel);
			this.RangesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RangesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RangesGroupBox.Name = "RangesGroupBox";
			this.RangesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 420, true);
			this.RangesGroupBox.TabIndex = 0;
			this.RangesGroupBox.TabStop = false;
			// 
			// RangesGrid
			// 
			this.RangesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RangesGrid, "Fountains");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).Fountains)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewStmNums)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).Fountains)).SyncRoot)).SN_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewStmNums)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).Fountains)).SyncRoot)).SN_TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewStmNums)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).Fountains)).SyncRoot)).SN_Prefix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ViewStmNums)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).Fountains)).SyncRoot)).SN_ValueForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ViewStmNums)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).Fountains)).SyncRoot)).SN_MinimumValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ViewStmNums)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).Fountains)).SyncRoot)).SN_Count)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ViewStmNums)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).Fountains)).SyncRoot)).SN_MaximumValue)));
			this.RangesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "SN_Type";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "SN_TypeDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.ColumnName = "SN_Prefix";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "SN_ValueForDisplay";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "SN_MinimumValue";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "SN_Count";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "SN_MaximumValue";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RangesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RangesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RangesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RangesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RangesGrid.GridId = "e39e05eb-413a-4e7e-a3f5-22c59df46416";
			this.RangesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RangesGrid.LayoutKey = "NumberFontainsGrid";
			this.RangesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.RangesGrid.Name = "RangesGrid";
			this.RangesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 374, true);
			this.RangesGrid.TabIndex = 1;
			// 
			// NumberFountainsButtonsPanel
			// 
			this.NumberFountainsButtonsPanel.Controls.Add(this.NumberFountainsDeleteButton);
			this.NumberFountainsButtonsPanel.Controls.Add(this.NumberFountainsEditButton);
			this.NumberFountainsButtonsPanel.Controls.Add(this.NumberFountainsAddButton);
			this.NumberFountainsButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.NumberFountainsButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 388, true);
			this.NumberFountainsButtonsPanel.Name = "NumberFountainsButtonsPanel";
			this.NumberFountainsButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 30, true);
			this.NumberFountainsButtonsPanel.TabIndex = 0;
			// 
			// NumberFountainsDeleteButton
			// 
			this.NumberFountainsDeleteButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("25e7df57-6d04-410e-a9f2-9da65768d099", "Delete");
			this.NumberFountainsDeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 3, true);
			this.NumberFountainsDeleteButton.Name = "NumberFountainsDeleteButton";
			this.NumberFountainsDeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NumberFountainsDeleteButton.TabIndex = 2;
			this.NumberFountainsDeleteButton.ToolTipCaption = null;
			this.NumberFountainsDeleteButton.Click += new System.EventHandler(this.NumberFountainsDeleteButton_Click);
			// 
			// NumberFountainsEditButton
			// 
			this.NumberFountainsEditButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a232eec5-e90d-4f80-b000-df28beccd9f0", "Edit...");
			this.NumberFountainsEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 3, true);
			this.NumberFountainsEditButton.Name = "NumberFountainsEditButton";
			this.NumberFountainsEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NumberFountainsEditButton.TabIndex = 1;
			this.NumberFountainsEditButton.ToolTipCaption = null;
			this.NumberFountainsEditButton.Click += new System.EventHandler(this.NumberFountainsEditButton_Click);
			// 
			// NumberFountainsAddButton
			// 
			this.NumberFountainsAddButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f9dbf32c-2186-4fa9-bb70-8ea7448f1a1c", "Add...");
			this.NumberFountainsAddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.NumberFountainsAddButton.Name = "NumberFountainsAddButton";
			this.NumberFountainsAddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NumberFountainsAddButton.TabIndex = 0;
			this.NumberFountainsAddButton.ToolTipCaption = null;
			this.NumberFountainsAddButton.Click += new System.EventHandler(this.NumberFountainsAddButton_Click);
			// 
			// NumberRangesTabControl
			// 
			this.NumberRangesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.NumberRangesTabControl.Controls.Add(this.RangesTabPage);
			this.NumberRangesTabControl.Controls.Add(this.MatchingDetailsTabPage);
			this.NumberRangesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumberRangesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NumberRangesTabControl.Name = "NumberRangesTabControl";
			this.NumberRangesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 450, true);
			this.NumberRangesTabControl.TabIndex = 1;
			// 
			// RangesTabPage
			// 
			this.RangesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("F9BFA215-EA9B-401F-BB75-A5E4ABFDCBF6", "Ranges");
			this.RangesTabPage.Controls.Add(this.RangesGroupBox);
			this.RangesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.RangesTabPage.Name = "RangesTabPage";
			this.RangesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RangesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 426, true);
			this.RangesTabPage.TabIndex = 0;
			this.RangesTabPage.UseVisualStyleBackColor = true;
			// 
			// MatchingDetailsTabPage
			// 
			this.MatchingDetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CC40D579-AEE4-4507-86F8-F89A9B2BA8A0", "Matching Details");
			this.MatchingDetailsTabPage.Controls.Add(this.MatchingDetailsGroupBox);
			this.MatchingDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.MatchingDetailsTabPage.Name = "MatchingDetailsTabPage";
			this.MatchingDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MatchingDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(858, 426, true);
			this.MatchingDetailsTabPage.TabIndex = 1;
			this.MatchingDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// MatchingDetailsGroupBox
			// 
			this.MatchingDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("622D6F58-A9E5-45BA-968D-0B631DC220B8", "Matching Details");
			this.MatchingDetailsGroupBox.Controls.Add(this.MatchingDetailsGrid);
			this.MatchingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MatchingDetailsGroupBox.Name = "MatchingDetailsGroupBox";
			this.MatchingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(851, 420, true);
			this.MatchingDetailsGroupBox.TabIndex = 0;
			this.MatchingDetailsGroupBox.TabStop = false;
			// 
			// MatchingDetailsGrid
			// 
			this.MatchingDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MatchingDetailsGrid, "NumberRangeMatchingDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.StmNumberRangeMatchingDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)).SyncRoot)).NRM_RangeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.StmNumberRangeMatchingDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)).SyncRoot)).NRM_Prefix)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.StmNumberRangeMatchingDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)).SyncRoot)).PatentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.StmNumberRangeMatchingDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)).SyncRoot)).CustomsArea)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.StmNumberRangeMatchingDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)).SyncRoot)).NRM_OH_Client)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.StmNumberRangeMatchingDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)).SyncRoot)).NRM_WW_Whs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.StmNumberRangeMatchingDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)).SyncRoot)).CurrentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.StmNumberRangeMatchingDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)).SyncRoot)).MinimumValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.StmNumberRangeMatchingDetail)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.IViewStmNumsOwner)(null)).NumberRangeMatchingDetails)).SyncRoot)).MaximumValue)));
			this.MatchingDetailsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "NRM_RangeType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "NRM_Prefix";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "PatentNumber";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CustomsArea";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "NRM_OH_Client";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "NRM_WW_Whs";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "CurrentNumber";
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "MinimumValue";
			zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "MaximumValue";
			zCalcEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.MatchingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MatchingDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MatchingDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MatchingDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.MatchingDetailsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.MatchingDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.MatchingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.MatchingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.MatchingDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.MatchingDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MatchingDetailsGrid.GridId = "c71f4979-727f-41f4-bbb1-4f8a875adf7f";
			this.MatchingDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MatchingDetailsGrid.LayoutKey = "MatchingDetailsGrid";
			this.MatchingDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.MatchingDetailsGrid.Name = "MatchingDetailsGrid";
			this.MatchingDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 403, true);
			this.MatchingDetailsGrid.TabIndex = 0;
			// 
			// NumberRangesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NumberRangesTabControl);
			this.Name = "NumberRangesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(864, 450, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RangesGroupBox.ResumeLayout(false);
			this.RangesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RangesGrid)).EndInit();
			this.RangesGrid.ResumeLayout(false);
			this.RangesGrid.PerformLayout();
			this.NumberFountainsButtonsPanel.ResumeLayout(false);
			this.NumberFountainsButtonsPanel.PerformLayout();
			this.NumberRangesTabControl.ResumeLayout(false);
			this.NumberRangesTabControl.PerformLayout();
			this.RangesTabPage.ResumeLayout(false);
			this.RangesTabPage.PerformLayout();
			this.MatchingDetailsTabPage.ResumeLayout(false);
			this.MatchingDetailsTabPage.PerformLayout();
			this.MatchingDetailsGroupBox.ResumeLayout(false);
			this.MatchingDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MatchingDetailsGrid)).EndInit();
			this.MatchingDetailsGrid.ResumeLayout(false);
			this.MatchingDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox RangesGroupBox;
		internal ZArchitecture.ZGrid RangesGrid;
		internal ZArchitecture.GUI.ZPanel NumberFountainsButtonsPanel;
		internal ZArchitecture.GUI.ZButton NumberFountainsAddButton;
		internal ZArchitecture.GUI.ZButton NumberFountainsDeleteButton;
		internal ZArchitecture.GUI.ZButton NumberFountainsEditButton;
		internal ZArchitecture.GUI.ZTabControl NumberRangesTabControl;
		internal ZArchitecture.GUI.ZTabPage RangesTabPage;
		internal ZArchitecture.GUI.ZTabPage MatchingDetailsTabPage;
		internal ZArchitecture.GUI.ZGroupBox MatchingDetailsGroupBox;
		internal ZArchitecture.ZGrid MatchingDetailsGrid;
		private System.ComponentModel.IContainer components;
	}
}
