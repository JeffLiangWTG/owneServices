using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.GUI
{
	public partial class LandCostInputUserControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new Container();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			this.HeaderPanel = new ZPanel();
			this.EstimatedDutyPercentCalcEdit = new ZCalcEdit();
			this.LT_DateOfProcessingDateEdit = new ZDateEdit();
			this.LT_DateOfEntryDateEdit = new ZDateEdit();
			this.MainTabControl = new ZTemplateTabControl();
			this.CostTabPage = new ZTabPage();
			this.BodyPanel = new ZPanel();
			this.zPanel1 = new ZPanel();
			this.CostInputGrid = new ZGrid();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.LinePanel = new ZPanel();
			this.DistributionGroupBox = new ZGroupBox();
			this.LI_ServiceExRateCalcEdit = new ZCalcEdit();
			this.AmountCalcFindBox = new ZCalcFindBox();
			this.LI_DistributCostByDropEdit = new ZDropEdit();
			this.LinkedObjectDropEdit = new ZDropEdit();
			this.ChargesGroupBox = new ZGroupBox();
			this.LI_LandedCostGroupDropEdit = new ZDropEdit();
			this.LI_ChargeDescriptionTextBox = new ZTextBox();
			this.LI_AC_ChargeCodeGuidFindBox = new ZGuidFindBox();
			this.ExchangeRateTabPage = new ZTabPage();
			this.ExRatesGrid = new ZGrid();
			this.LinesTabPage = new ZTabPage();
			this.NotePanel = new ZPanel();
			this.NoteLabel = new ZLabel();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderPanel.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.CostTabPage.SuspendLayout();
			this.BodyPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			((ISupportInitialize)(this.CostInputGrid)).BeginInit();
			this.LinePanel.SuspendLayout();
			this.DistributionGroupBox.SuspendLayout();
			this.ChargesGroupBox.SuspendLayout();
			this.ExchangeRateTabPage.SuspendLayout();
			((ISupportInitialize)(this.ExRatesGrid)).BeginInit();
			this.NotePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(LandedCostHeader);
			// 
			// HeaderPanel
			// 
			this.HeaderPanel.Controls.Add(this.EstimatedDutyPercentCalcEdit);
			this.HeaderPanel.Controls.Add(this.LT_DateOfProcessingDateEdit);
			this.HeaderPanel.Controls.Add(this.LT_DateOfEntryDateEdit);
			this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.HeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 40, true);
			this.HeaderPanel.TabIndex = 0;
			// 
			// EstimatedDutyPercentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EstimatedDutyPercentCalcEdit, "LT_DefaultEstimatedDutyRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandedCostHeader)(null)).LT_DefaultEstimatedDutyRate);
			this.EstimatedDutyPercentCalcEdit.DecimalPlaces = 2;
			this.EstimatedDutyPercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 9, true);
			this.EstimatedDutyPercentCalcEdit.Name = "EstimatedDutyPercentCalcEdit";
			this.EstimatedDutyPercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.EstimatedDutyPercentCalcEdit.TabIndex = 4;
			this.EstimatedDutyPercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LT_DateOfProcessingDateEdit
			// 
			this.LT_DateOfProcessingDateEdit.AllowDrop = true;
			this.LT_DateOfProcessingDateEdit.AutoCompleteMonthThreshold = 1;
			this.LT_DateOfProcessingDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LT_DateOfProcessingDateEdit, "LT_DateOfProcessing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandedCostHeader)(null)).LT_DateOfProcessing);
			this.LT_DateOfProcessingDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LT_DateOfProcessingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 9, true);
			this.LT_DateOfProcessingDateEdit.Name = "LT_DateOfProcessingDateEdit";
			this.LT_DateOfProcessingDateEdit.TabIndex = 3;
			// 
			// LT_DateOfEntryDateEdit
			// 
			this.LT_DateOfEntryDateEdit.AllowDrop = true;
			this.LT_DateOfEntryDateEdit.AutoCompleteMonthThreshold = 1;
			this.LT_DateOfEntryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LT_DateOfEntryDateEdit, "LT_DateOfEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandedCostHeader)(null)).LT_DateOfEntry);
			this.LT_DateOfEntryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 9, true);
			this.LT_DateOfEntryDateEdit.Name = "LT_DateOfEntryDateEdit";
			this.LT_DateOfEntryDateEdit.TabIndex = 0;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.MainTabControl.Controls.Add(this.CostTabPage);
			this.MainTabControl.Controls.Add(this.ExchangeRateTabPage);
			this.MainTabControl.Controls.Add(this.LinesTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 62, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 530, true);
			this.MainTabControl.TabIndex = 1;
			this.MainTabControl.SelectedIndexChanged += new EventHandler(this.MainTabControl_SelectedIndexChanged);
			// 
			// CostTabPage
			// 
			this.CostTabPage.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|ebe5d74a-0fba-440e-8a2b-02c149c3fd4c", "Transport & Logistics Costs");
			this.CostTabPage.Controls.Add(this.BodyPanel);
			this.CostTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CostTabPage.Name = "CostTabPage";
			this.CostTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 507, true);
			this.CostTabPage.TabIndex = 0;
			// 
			// BodyPanel
			// 
			this.BodyPanel.Controls.Add(this.zPanel1);
			this.BodyPanel.Controls.Add(this.splitter1);
			this.BodyPanel.Controls.Add(this.LinePanel);
			this.BodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BodyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BodyPanel.Name = "BodyPanel";
			this.BodyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 507, true);
			this.BodyPanel.TabIndex = 8;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.CostInputGrid);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 387, true);
			this.zPanel1.TabIndex = 9;
			// 
			// CostInputGrid
			// 
			this.CostInputGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CostInputGrid, "CostInputs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandedCostHeader)(null)).CostInputs);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_AC_ChargeCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).Lookups.ChargeCodes);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_ChargeDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LCGroupString);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).Lookups.LandCostGroupList);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LinkedObjectUniqueCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).Lookups.ParentAssociableList);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_DistributeCostBy);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).Lookups.DistributeCostBy);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).DecimalPlaces);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_CostAmount);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_RX_NKCostCurrency);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_ServiceExRate);
			this.CostInputGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.ChargeCodes";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "LI_AC_ChargeCode";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCode;
			zTextBoxColumnStyleInfo1.ColumnName = "LI_ChargeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.LandCostGroupList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|e20b3e68-1c01-40be-b1a7-b9f8ade028f8", "Charge Group");
			zDropEditColumnStyleInfo1.ColumnName = "LCGroupString";
			zDropEditColumnStyleInfo2.BindToList = "Lookups.ParentAssociableList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|cbed14bc-a43b-4085-8af3-68af34e4a6ff", "Distribution Level");
			zDropEditColumnStyleInfo2.ColumnName = "LinkedObjectUniqueCode";
			zDropEditColumnStyleInfo3.BindToList = "Lookups.DistributeCostBy";
			zDropEditColumnStyleInfo3.ColumnName = "LI_DistributeCostBy";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "DecimalPlaces";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|fd25962b-7e05-4be5-9932-351c57b3911a", "Distribution Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "LI_CostAmount";
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|d41778a3-0551-4e84-a322-e7dcfca98be6", "Cost");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "LI_RX_NKCostCurrency";
			zCodeFindBoxColumnStyleInfo1.GroupName = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|d41778a3-0551-4e84-a322-e7dcfca98be6", "Cost");
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "LI_ServiceExRate";
			zCalcEditColumnStyleInfo2.Decimals = 9;
			this.CostInputGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CostInputGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CostInputGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CostInputGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CostInputGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CostInputGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CostInputGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CostInputGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CostInputGrid.CopySelectedRowsAllowed = true;
			this.CostInputGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CostInputGrid.GridId = "fdb29dea-c857-4f54-96d6-7734e00ffa41";
			this.CostInputGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CostInputGrid.LayoutKey = "CostInputGrid";
			this.CostInputGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CostInputGrid.Name = "CostInputGrid";
			this.CostInputGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 387, true);
			this.CostInputGrid.TabIndex = 1;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 387, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 8, true);
			this.splitter1.TabIndex = 8;
			this.splitter1.TabStop = false;
			// 
			// LinePanel
			// 
			this.LinePanel.Controls.Add(this.DistributionGroupBox);
			this.LinePanel.Controls.Add(this.ChargesGroupBox);
			this.LinePanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LinePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 395, true);
			this.LinePanel.Name = "LinePanel";
			this.LinePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 112, true);
			this.LinePanel.TabIndex = 7;
			// 
			// DistributionGroupBox
			// 
			this.DistributionGroupBox.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|18943591-5531-4aa7-a4ab-b7b7a521234c", "Distribution");
			this.DistributionGroupBox.Controls.Add(this.LI_ServiceExRateCalcEdit);
			this.DistributionGroupBox.Controls.Add(this.AmountCalcFindBox);
			this.DistributionGroupBox.Controls.Add(this.LI_DistributCostByDropEdit);
			this.DistributionGroupBox.Controls.Add(this.LinkedObjectDropEdit);
			this.DistributionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.DistributionGroupBox.Name = "DistributionGroupBox";
			this.DistributionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 48, true);
			this.DistributionGroupBox.TabIndex = 2;
			this.DistributionGroupBox.TabStop = false;
			// 
			// LI_ServiceExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LI_ServiceExRateCalcEdit, "CostInputs.LI_ServiceExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_ServiceExRate);
			this.LI_ServiceExRateCalcEdit.DecimalPlaces = 2;
			this.LI_ServiceExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(824, 16, true);
			this.LI_ServiceExRateCalcEdit.Name = "LI_ServiceExRateCalcEdit";
			this.LI_ServiceExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.LI_ServiceExRateCalcEdit.TabIndex = 3;
			this.LI_ServiceExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AmountCalcFindBox
			// 
			this.AmountCalcFindBox.AllowDrop = true;
			this.AmountCalcFindBox.BindToAmount = "CostInputs.LI_CostAmount";
			this.AmountCalcFindBox.BindToDecimalPlaces = "CostInputs.DecimalPlaces";
			this.AmountCalcFindBox.BindToList = "CostInputs.Lookups+CostCurrencies";
			this.AmountCalcFindBox.BindToUnit = "CostInputs.LI_RX_NKCostCurrency";
			this.AmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 16, true);
			this.AmountCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.AmountCalcFindBox.Name = "AmountCalcFindBox";
			this.AmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.AmountCalcFindBox.TabIndex = 2;
			// 
			// LI_DistributCostByDropEdit
			// 
			this.LI_DistributCostByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LI_DistributCostByDropEdit, "CostInputs.LI_DistributeCostBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_DistributeCostBy);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).Lookups.DistributeCostBy);
			this.LI_DistributCostByDropEdit.BindToList = "CostInputs.Lookups+DistributeCostBy";
			this.LI_DistributCostByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 16, true);
			this.LI_DistributCostByDropEdit.Name = "LI_DistributCostByDropEdit";
			this.LI_DistributCostByDropEdit.PreBoundMaxLength = 3;
			this.LI_DistributCostByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.LI_DistributCostByDropEdit.TabIndex = 1;
			// 
			// LinkedObjectDropEdit
			// 
			this.LinkedObjectDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LinkedObjectDropEdit, "CostInputs.LinkedObjectUniqueCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LinkedObjectUniqueCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).Lookups.ParentAssociableList);
			this.LinkedObjectDropEdit.BindToList = "CostInputs.Lookups+ParentAssociableList";
			this.LinkedObjectDropEdit.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|ca8d1d33-0e55-4ac3-8387-24bfdc31c341", "Level");
			this.LinkedObjectDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LinkedObjectDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 16, true);
			this.LinkedObjectDropEdit.Name = "LinkedObjectDropEdit";
			this.LinkedObjectDropEdit.PreBoundMaxLength = 35;
			this.LinkedObjectDropEdit.ShowDescriptionBox = false;
			this.LinkedObjectDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.LinkedObjectDropEdit.TabIndex = 0;
			// 
			// ChargesGroupBox
			// 
			this.ChargesGroupBox.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|f465db9c-986e-40d9-ba7b-73996aac2e12", "Charges");
			this.ChargesGroupBox.Controls.Add(this.LI_LandedCostGroupDropEdit);
			this.ChargesGroupBox.Controls.Add(this.LI_ChargeDescriptionTextBox);
			this.ChargesGroupBox.Controls.Add(this.LI_AC_ChargeCodeGuidFindBox);
			this.ChargesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.ChargesGroupBox.Name = "ChargesGroupBox";
			this.ChargesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 48, true);
			this.ChargesGroupBox.TabIndex = 1;
			this.ChargesGroupBox.TabStop = false;
			// 
			// LI_LandedCostGroupDropEdit
			// 
			this.LI_LandedCostGroupDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LI_LandedCostGroupDropEdit, "CostInputs.LCGroupString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LCGroupString);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).Lookups.LandCostGroupList);
			this.LI_LandedCostGroupDropEdit.BindToList = "CostInputs.Lookups+LandCostGroupList";
			this.LI_LandedCostGroupDropEdit.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|715cbf2a-dbc2-48b9-a246-a15dadce7fe2", "Group");
			this.LI_LandedCostGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(688, 16, true);
			this.LI_LandedCostGroupDropEdit.Name = "LI_LandedCostGroupDropEdit";
			this.LI_LandedCostGroupDropEdit.PreBoundMaxLength = 1;
			this.LI_LandedCostGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.LI_LandedCostGroupDropEdit.TabIndex = 2;
			// 
			// LI_ChargeDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.LI_ChargeDescriptionTextBox, "CostInputs.LI_ChargeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_ChargeDescription);
			this.LI_ChargeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 16, true);
			this.LI_ChargeDescriptionTextBox.Name = "LI_ChargeDescriptionTextBox";
			this.LI_ChargeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.LI_ChargeDescriptionTextBox.TabIndex = 1;
			// 
			// LI_AC_ChargeCodeGuidFindBox
			// 
			this.LI_AC_ChargeCodeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LI_AC_ChargeCodeGuidFindBox, "CostInputs.LI_AC_ChargeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).LI_AC_ChargeCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandCostInput)(((System.Collections.IList)(((LandedCostHeader)(null)).CostInputs)).SyncRoot)).Lookups.ChargeCodes);
			this.LI_AC_ChargeCodeGuidFindBox.BindToList = "CostInputs.Lookups+ChargeCodes";
			this.LI_AC_ChargeCodeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 16, true);
			this.LI_AC_ChargeCodeGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCode;
			this.LI_AC_ChargeCodeGuidFindBox.Name = "LI_AC_ChargeCodeGuidFindBox";
			this.LI_AC_ChargeCodeGuidFindBox.PreBoundMaxLength = 10;
			this.LI_AC_ChargeCodeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.LI_AC_ChargeCodeGuidFindBox.TabIndex = 0;
			// 
			// ExchangeRateTabPage
			// 
			this.ExchangeRateTabPage.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|48f4d683-5907-4cef-85ca-0892b3cf1e51", "Exchange Rates");
			this.ExchangeRateTabPage.Controls.Add(this.ExRatesGrid);
			this.ExchangeRateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ExchangeRateTabPage.Name = "ExchangeRateTabPage";
			this.ExchangeRateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 525, true);
			this.ExchangeRateTabPage.TabIndex = 1;
			// 
			// ExRatesGrid
			// 
			this.ExRatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExRatesGrid, "ExchangeRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandedCostHeader)(null)).ExchangeRates);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandedCostingExRate)(((System.Collections.IList)(((LandedCostHeader)(null)).ExchangeRates)).SyncRoot)).ReferenceNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandedCostingExRate)(((System.Collections.IList)(((LandedCostHeader)(null)).ExchangeRates)).SyncRoot)).CurrencyCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((LandedCostingExRate)(((System.Collections.IList)(((LandedCostHeader)(null)).ExchangeRates)).SyncRoot)).ExchangeRate);
			this.ExRatesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|823a49c7-585b-445e-be0e-40a1fb6e01e1", "Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|f9336003-2182-4137-acc7-c1a6939a706a", "Curr");
			zTextBoxColumnStyleInfo3.ColumnName = "CurrencyCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|36b0e0d7-00ca-4682-9170-51d9ba5dea86", "Exchange Rate");
			zCalcEditColumnStyleInfo3.ColumnName = "ExchangeRate";
			zCalcEditColumnStyleInfo3.Decimals = 9;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.ExRatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ExRatesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ExRatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ExRatesGrid.CopySelectedRowsAllowed = true;
			this.ExRatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExRatesGrid.GridId = "a762e215-da54-4907-ae2f-e48bcea5322e";
			this.ExRatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExRatesGrid.LayoutKey = "ExRatesGrid";
			this.ExRatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExRatesGrid.Name = "ExRatesGrid";
			this.ExRatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 525, true);
			this.ExRatesGrid.TabIndex = 1;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.CaptionResourceString = Enterprise.LandedCosting.GUI.Res.GetData("LandCostInputUserControl|862d310b-8e50-4b11-b36c-63672b14ef6f", "Lines");
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 507, true);
			this.LinesTabPage.TabIndex = 2;
			// 
			// NotePanel
			// 
			this.NotePanel.Controls.Add(this.NoteLabel);
			this.NotePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.NotePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.NotePanel.Name = "NotePanel";
			this.NotePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 22, true);
			this.NotePanel.TabIndex = 0;
			this.NotePanel.Visible = false;
			// 
			// NoteLabel
			// 
			this.NoteLabel.AutoSize = true;
			this.NoteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 2, true);
			this.NoteLabel.Name = "NoteLabel";
			this.NoteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 14, true);
			this.NoteLabel.TabIndex = 7;
			this.NoteLabel.Text = Enterprise.LandedCosting.GUI.Res.GetString("283E26DE-7E87-46F1-9076-7858F0931D80", "Note that manually entered foreign exchange rates need to be entered as a direct quote");
			// 
			// LandCostInputUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.NotePanel);
			this.Controls.Add(this.HeaderPanel);
			this.Name = "LandCostInputUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 592, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderPanel.ResumeLayout(false);
			this.HeaderPanel.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.CostTabPage.ResumeLayout(false);
			this.BodyPanel.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			((ISupportInitialize)(this.CostInputGrid)).EndInit();
			this.LinePanel.ResumeLayout(false);
			this.DistributionGroupBox.ResumeLayout(false);
			this.DistributionGroupBox.PerformLayout();
			this.ChargesGroupBox.ResumeLayout(false);
			this.ChargesGroupBox.PerformLayout();
			this.ExchangeRateTabPage.ResumeLayout(false);
			((ISupportInitialize)(this.ExRatesGrid)).EndInit();
			this.NotePanel.ResumeLayout(false);
			this.NotePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
