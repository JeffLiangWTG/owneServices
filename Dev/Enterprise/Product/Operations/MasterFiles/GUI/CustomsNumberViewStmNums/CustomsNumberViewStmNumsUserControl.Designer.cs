using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class CustomsNumberViewStmNumsUserControl
	{

		protected ZGroupBox ThresholdRunOutWarningGroupBox;
		protected ZGrid ThresholdRunOutWarningGrid;
		protected ZGroupBox NumberRangesGroupBox;
		protected ZGrid NumberRangesGrid;
		protected ZPanel NumberRangesButtonsPanel;
		protected ZPanel NumberRangesAddButtonPanel;
		protected ZPanel NumberRangesAdditionalButtonsPanel;
		protected ZPanel NumberRangesOtherButtonsPanel;
		protected CargoWise.Windows.UI.KSplitContainer NumberRangeSplitContainer;
		protected ZButton NumberRangesAddButton;
		protected ZButton NumberRangesEditButton;
		protected ZButton NumberRangesDeleteButton;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.NumberRangesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NumberRangesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NumberRangesButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NumberRangesOtherButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NumberRangesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NumberRangesDeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NumberRangesAdditionalButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NumberRangesAddButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NumberRangesAddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ThresholdRunOutWarningGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ThresholdRunOutWarningGrid = new Enterprise.ZArchitecture.ZGrid();
			this.NumberRangeSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NumberRangesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumberRangesGrid)).BeginInit();
			this.NumberRangesGrid.SuspendLayout();
			this.NumberRangesButtonsPanel.SuspendLayout();
			this.NumberRangesOtherButtonsPanel.SuspendLayout();
			this.NumberRangesAdditionalButtonsPanel.SuspendLayout();
			this.NumberRangesAddButtonPanel.SuspendLayout();
			this.ThresholdRunOutWarningGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ThresholdRunOutWarningGrid)).BeginInit();
			this.ThresholdRunOutWarningGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumberRangeSplitContainer)).BeginInit();
			this.NumberRangeSplitContainer.Panel1.SuspendLayout();
			this.NumberRangeSplitContainer.Panel2.SuspendLayout();
			this.NumberRangeSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider);
			// 
			// NumberRangesGroupBox
			// 
			this.NumberRangesGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3728c298-89d9-4d97-8d92-0e373b3889c0", "Number Ranges");
			this.NumberRangesGroupBox.Controls.Add(this.NumberRangesGrid);
			this.NumberRangesGroupBox.Controls.Add(this.NumberRangesButtonsPanel);
			this.NumberRangesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumberRangesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NumberRangesGroupBox.Name = "NumberRangesGroupBox";
			this.NumberRangesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.NumberRangesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 211, true);
			this.NumberRangesGroupBox.TabIndex = 0;
			this.NumberRangesGroupBox.TabStop = false;
			// 
			// NumberRangesGrid
			// 
			this.NumberRangesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.NumberRangesGrid, "CustomsNumberWrappers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_OwnerForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_ValueForDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_AvailableNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_MinimumValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_Count)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_MaximumValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_FountainName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsWrapper)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).CustomsNumberWrappers)).SyncRoot)).SN_SystemCreateTimeUtc)));
			this.NumberRangesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "SN_OwnerForDisplay";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(195);
			zTextBoxColumnStyleInfo2.ColumnName = "SN_Type";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(81);
			zTextBoxColumnStyleInfo3.ColumnName = "SN_TypeDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(167);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "SN_ValueForDisplay";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "SN_AvailableNumbers";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "SN_MinimumValue";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.IsMandatory = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "SN_Count";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.IsMandatory = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "SN_MaximumValue";
			zCalcEditColumnStyleInfo5.Decimals = 0;
			zCalcEditColumnStyleInfo5.IsMandatory = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84);
			zTextBoxColumnStyleInfo4.ColumnName = "SN_FountainName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo1.ColumnName = "SN_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			this.NumberRangesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.NumberRangesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.NumberRangesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.NumberRangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.NumberRangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.NumberRangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.NumberRangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.NumberRangesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.NumberRangesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.NumberRangesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.NumberRangesGrid.CopySelectedRowsAllowed = false;
			this.NumberRangesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumberRangesGrid.GridId = "e4a6e72f-f6c7-4df6-b012-25f688e3fdde";
			this.NumberRangesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.NumberRangesGrid.IsWholeRowSelectedOnClick = true;
			this.NumberRangesGrid.LayoutKey = "NumberRangesGrid";
			this.NumberRangesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 18, true);
			this.NumberRangesGrid.Name = "NumberRangesGrid";
			this.NumberRangesGrid.ReadOnly = true;
			this.NumberRangesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 158, true);
			this.NumberRangesGrid.TabIndex = 1;
			this.NumberRangesGrid.AfterBind += new System.EventHandler(this.NumberRangesGrid_AfterBind);
			// 
			// NumberRangesButtonsPanel
			// 
			this.NumberRangesButtonsPanel.Controls.Add(this.NumberRangesOtherButtonsPanel);
			this.NumberRangesButtonsPanel.Controls.Add(this.NumberRangesAdditionalButtonsPanel);
			this.NumberRangesButtonsPanel.Controls.Add(this.NumberRangesAddButtonPanel);
			this.NumberRangesButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.NumberRangesButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 176, true);
			this.NumberRangesButtonsPanel.Name = "NumberRangesButtonsPanel";
			this.NumberRangesButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 30, true);
			this.NumberRangesButtonsPanel.TabIndex = 1;
			// 
			// NumberRangesOtherButtonsPanel
			// 
			this.NumberRangesOtherButtonsPanel.Controls.Add(this.NumberRangesEditButton);
			this.NumberRangesOtherButtonsPanel.Controls.Add(this.NumberRangesDeleteButton);
			this.NumberRangesOtherButtonsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumberRangesOtherButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 0, true);
			this.NumberRangesOtherButtonsPanel.Name = "NumberRangesOtherButtonsPanel";
			this.NumberRangesOtherButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(550, 30, true);
			this.NumberRangesOtherButtonsPanel.TabIndex = 2;
			// 
			// NumberRangesEditButton
			// 
			this.NumberRangesEditButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a1a4116d-1456-4d07-8615-d3af6141b670", "&Edit");
			this.NumberRangesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.NumberRangesEditButton.Name = "NumberRangesEditButton";
			this.NumberRangesEditButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NumberRangesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.NumberRangesEditButton.TabIndex = 0;
			this.NumberRangesEditButton.ToolTipCaption = null;
			this.NumberRangesEditButton.Click += new System.EventHandler(this.NumberRangesEditButton_Click);
			// 
			// NumberRangesDeleteButton
			// 
			this.NumberRangesDeleteButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d23a927d-46d7-4ac7-9a43-f3294d669ee8", "&Delete");
			this.NumberRangesDeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 3, true);
			this.NumberRangesDeleteButton.Name = "NumberRangesDeleteButton";
			this.NumberRangesDeleteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NumberRangesDeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.NumberRangesDeleteButton.TabIndex = 1;
			this.NumberRangesDeleteButton.ToolTipCaption = null;
			this.NumberRangesDeleteButton.Click += new System.EventHandler(this.NumberRangesDeleteButton_Click);
			// 
			// NumberRangesAdditionalButtonsPanel
			// 
			this.NumberRangesAdditionalButtonsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.NumberRangesAdditionalButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 0, true);
			this.NumberRangesAdditionalButtonsPanel.Name = "NumberRangesAdditionalButtonsPanel";
			this.NumberRangesAdditionalButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 30, true);
			this.NumberRangesAdditionalButtonsPanel.TabIndex = 1;
			this.NumberRangesAdditionalButtonsPanel.Visible = false;
			// 
			// NumberRangesAddButtonPanel
			// 
			this.NumberRangesAddButtonPanel.Controls.Add(this.NumberRangesAddButton);
			this.NumberRangesAddButtonPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.NumberRangesAddButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NumberRangesAddButtonPanel.Name = "NumberRangesAddButtonPanel";
			this.NumberRangesAddButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 30, true);
			this.NumberRangesAddButtonPanel.TabIndex = 0;
			// 
			// NumberRangesAddButton
			// 
			this.NumberRangesAddButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e448c3b1-3bba-48fb-932b-2d9a8d07b3b1", "&Add");
			this.NumberRangesAddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.NumberRangesAddButton.Name = "NumberRangesAddButton";
			this.NumberRangesAddButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.NumberRangesAddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.NumberRangesAddButton.TabIndex = 0;
			this.NumberRangesAddButton.ToolTipCaption = null;
			this.NumberRangesAddButton.Click += new System.EventHandler(this.NumberRangesAddButton_Click);
			// 
			// ThresholdRunOutWarningGroupBox
			// 
			this.ThresholdRunOutWarningGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c4557093-c651-4849-a55e-b9d40d7eb13f", "Threshold Run Out Warning");
			this.ThresholdRunOutWarningGroupBox.Controls.Add(this.ThresholdRunOutWarningGrid);
			this.ThresholdRunOutWarningGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ThresholdRunOutWarningGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ThresholdRunOutWarningGroupBox.Name = "ThresholdRunOutWarningGroupBox";
			this.ThresholdRunOutWarningGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ThresholdRunOutWarningGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 156, true);
			this.ThresholdRunOutWarningGroupBox.TabIndex = 0;
			this.ThresholdRunOutWarningGroupBox.TabStop = false;
			// 
			// ThresholdRunOutWarningGrid
			// 
			this.ThresholdRunOutWarningGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ThresholdRunOutWarningGrid, "NumberRanges");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).NumberRanges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.CustomsNumberStmNumberRange)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).NumberRanges)).SyncRoot)).Detail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberStmNumberRange)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).NumberRanges)).SyncRoot)).SNR_ThresholdRunOutWarning)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.CustomsNumberStmNumberRange)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.CustomsNumberViewStmNumsBusinessProvider)(null)).NumberRanges)).SyncRoot)).TotalAvailableNumbers)));
			this.ThresholdRunOutWarningGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.ColumnName = "Detail";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(542);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "SNR_ThresholdRunOutWarning";
			zCalcEditColumnStyleInfo6.Decimals = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(162);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "TotalAvailableNumbers";
			zCalcEditColumnStyleInfo7.Decimals = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			this.ThresholdRunOutWarningGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ThresholdRunOutWarningGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ThresholdRunOutWarningGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.ThresholdRunOutWarningGrid.CopySelectedRowsAllowed = false;
			this.ThresholdRunOutWarningGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ThresholdRunOutWarningGrid.GridId = "e4a6e72f-f6c7-4df6-b012-25f688e3fdde";
			this.ThresholdRunOutWarningGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ThresholdRunOutWarningGrid.LayoutKey = "NumberRangesGrid";
			this.ThresholdRunOutWarningGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 18, true);
			this.ThresholdRunOutWarningGrid.Name = "ThresholdRunOutWarningGrid";
			this.ThresholdRunOutWarningGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 133, true);
			this.ThresholdRunOutWarningGrid.TabIndex = 0;
			this.ThresholdRunOutWarningGrid.AfterBind += new System.EventHandler(this.ThresholdRunOutWarningGrid_AfterBind);
			// 
			// NumberRangeSplitContainer
			// 
			this.NumberRangeSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NumberRangeSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NumberRangeSplitContainer.Name = "NumberRangeSplitContainer";
			this.NumberRangeSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// NumberRangeSplitContainer.Panel1
			// 
			this.NumberRangeSplitContainer.Panel1.Controls.Add(this.NumberRangesGroupBox);
			this.NumberRangeSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 371, true);
			this.NumberRangeSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			// 
			// NumberRangeSplitContainer.Panel2
			// 
			this.NumberRangeSplitContainer.Panel2.Controls.Add(this.ThresholdRunOutWarningGroupBox);
			this.NumberRangeSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(211);
			this.NumberRangeSplitContainer.TabIndex = 0;
			// 
			// CustomsNumberViewStmNumsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NumberRangeSplitContainer);
			this.Name = "CustomsNumberViewStmNumsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 371, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NumberRangesGroupBox.ResumeLayout(false);
			this.NumberRangesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NumberRangesGrid)).EndInit();
			this.NumberRangesGrid.ResumeLayout(false);
			this.NumberRangesGrid.PerformLayout();
			this.NumberRangesButtonsPanel.ResumeLayout(false);
			this.NumberRangesButtonsPanel.PerformLayout();
			this.NumberRangesOtherButtonsPanel.ResumeLayout(false);
			this.NumberRangesOtherButtonsPanel.PerformLayout();
			this.NumberRangesAdditionalButtonsPanel.ResumeLayout(false);
			this.NumberRangesAdditionalButtonsPanel.PerformLayout();
			this.NumberRangesAddButtonPanel.ResumeLayout(false);
			this.NumberRangesAddButtonPanel.PerformLayout();
			this.ThresholdRunOutWarningGroupBox.ResumeLayout(false);
			this.ThresholdRunOutWarningGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ThresholdRunOutWarningGrid)).EndInit();
			this.ThresholdRunOutWarningGrid.ResumeLayout(false);
			this.ThresholdRunOutWarningGrid.PerformLayout();
			this.NumberRangeSplitContainer.Panel1.ResumeLayout(false);
			this.NumberRangeSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.NumberRangeSplitContainer)).EndInit();
			this.NumberRangeSplitContainer.ResumeLayout(false);
			this.NumberRangeSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
