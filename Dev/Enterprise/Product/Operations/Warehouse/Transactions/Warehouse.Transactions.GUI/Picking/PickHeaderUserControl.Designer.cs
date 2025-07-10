using System;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI.Common;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class PickHeaderUserControl
	{
		ZGroupBox PickInfoGroupBox;
		ZTextBox PickNoTextBox1;
		ZTextBox PickStatusTextBox;
		ZGuidFindBox zGuidFindBox1;

		ZLabel PickStatusChartUnavailableLabel;
		ZChart PieChartBox;
		ZCalcEdit PickPriorityCalcEdit;
		ZCheckBox PickCasesByLabelCheckBox;
		ZCheckBox PickPalletsByLabelCheckBox;
		ZCheckBox CartoniseCheckBox;
		ZCheckBox ForcePickByCaseCheckBox;
		ZGuidFindBox DockDoorGuidFindBox;
		ZGuidFindBox PackingStationGuidFindBox;
		ZCheckBox ForcePickSplitCaseCheckBox;
		ZGuidFindBox PickAreaOverride;
		ZDropEdit zDropEdit1;
		ZCheckBox IsAwaitingReplenishmentCheckBox;
		protected ZButton DisplayAwaitingReplenishmentPicksButton;

		void InitializeComponent()
		{
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
			this.PickInfoGroupBox = new ZGroupBox();
			this.PickAreaOverride = new ZGuidFindBox();
			this.ForcePickSplitCaseCheckBox = new ZCheckBox();
			this.ForcePickByCaseCheckBox = new ZCheckBox();
			this.DockDoorGuidFindBox = new ZGuidFindBox();
			this.PackingStationGuidFindBox = new ZGuidFindBox();
			this.PickCasesByLabelCheckBox = new ZCheckBox();
			this.PickPalletsByLabelCheckBox = new ZCheckBox();
			this.CartoniseCheckBox = new ZCheckBox();
			this.PickPriorityCalcEdit = new ZCalcEdit();
			this.PieChartBox = new ZChart();
			this.PickStatusChartUnavailableLabel = new ZLabel();
			this.zGuidFindBox1 = new ZGuidFindBox();
			this.zDropEdit1 = new ZDropEdit();
			this.PickStatusTextBox = new ZTextBox();
			this.PickNoTextBox1 = new ZTextBox();
			this.IsAwaitingReplenishmentCheckBox = new ZCheckBox();
			this.DisplayAwaitingReplenishmentPicksButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PickInfoGroupBox.SuspendLayout();
			this.PickAreaOverride.SuspendLayout();
			this.DockDoorGuidFindBox.SuspendLayout();
			this.PackingStationGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PieChartBox)).BeginInit();
			this.zGuidFindBox1.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsPick);
			// 
			// PickInfoGroupBox
			// 
			this.PickInfoGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickHeaderUserControl|908df9d1-153f-4a84-a1cf-d97a10c51d61", "Pick Details");
			this.PickInfoGroupBox.Controls.Add(this.PickAreaOverride);
			this.PickInfoGroupBox.Controls.Add(this.ForcePickSplitCaseCheckBox);
			this.PickInfoGroupBox.Controls.Add(this.ForcePickByCaseCheckBox);
			this.PickInfoGroupBox.Controls.Add(this.DockDoorGuidFindBox);
			this.PickInfoGroupBox.Controls.Add(this.PackingStationGuidFindBox);
			this.PickInfoGroupBox.Controls.Add(this.PickCasesByLabelCheckBox);
			this.PickInfoGroupBox.Controls.Add(this.PickPalletsByLabelCheckBox);
			this.PickInfoGroupBox.Controls.Add(this.CartoniseCheckBox);
			this.PickInfoGroupBox.Controls.Add(this.PickPriorityCalcEdit);
			this.PickInfoGroupBox.Controls.Add(this.PieChartBox);
			this.PickInfoGroupBox.Controls.Add(this.PickStatusChartUnavailableLabel);
			this.PickInfoGroupBox.Controls.Add(this.zGuidFindBox1);
			this.PickInfoGroupBox.Controls.Add(this.zDropEdit1);
			this.PickInfoGroupBox.Controls.Add(this.PickStatusTextBox);
			this.PickInfoGroupBox.Controls.Add(this.PickNoTextBox1);
			this.PickInfoGroupBox.Controls.Add(this.IsAwaitingReplenishmentCheckBox);
			this.PickInfoGroupBox.Controls.Add(this.DisplayAwaitingReplenishmentPicksButton);
			this.PickInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PickInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PickInfoGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.PickInfoGroupBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.PickInfoGroupBox.Name = "PickInfoGroupBox";
			this.PickInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.PickInfoGroupBox.TabIndex = 0;
			this.PickInfoGroupBox.TabStop = false;
			// 
			// PickAreaOverride
			// 
			this.PickAreaOverride.AllowDrop = true;
			this.PickAreaOverride.AutoCompleteDisabled = true;
			this.BindingSource.SetBindingMember(this.PickAreaOverride, "WP_WA_DynamicPickAreaOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsPick)(null)).WP_WA_DynamicPickAreaOverride)));
			this.PickAreaOverride.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 88, true);
			this.PickAreaOverride.Name = "PickAreaOverride";
			this.PickAreaOverride.PreBoundMaxLength = 23;
			this.PickAreaOverride.ShowDescriptionBox = false;
			this.PickAreaOverride.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.PickAreaOverride.TabIndex = 9;
			// 
			// ForcePickSplitCaseCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ForcePickSplitCaseCheckBox, "WP_ForceSplitCaseUOMTypeAllocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((WhsPick)(null)).WP_ForceSplitCaseUOMTypeAllocation)));
			this.ForcePickSplitCaseCheckBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickHeaderUserControl|ForcePickSplitCaseCheckBox", "Force Pick Split Case UOM Type Allocation");
			this.ForcePickSplitCaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ForcePickSplitCaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(680, 42, true);
			this.ForcePickSplitCaseCheckBox.Name = "ForcePickSplitCaseCheckBox";
			this.ForcePickSplitCaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.ForcePickSplitCaseCheckBox.TabIndex = 15;
			this.ForcePickSplitCaseCheckBox.UseVisualStyleBackColor = true;
			// 
			// ForcePickByCaseCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ForcePickByCaseCheckBox, "WP_ForcePickByCaseUOMTypeAllocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((WhsPick)(null)).WP_ForcePickByCaseUOMTypeAllocation)));
			this.ForcePickByCaseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ForcePickByCaseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(680, 18, true);
			this.ForcePickByCaseCheckBox.Name = "ForcePickByCaseCheckBox";
			this.ForcePickByCaseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 16, true);
			this.ForcePickByCaseCheckBox.TabIndex = 14;
			this.ForcePickByCaseCheckBox.UseVisualStyleBackColor = true;
			// 
			// DockDoorGuidFindBox
			// 
			this.DockDoorGuidFindBox.AllowDrop = true;
			this.DockDoorGuidFindBox.AutoCompleteDisabled = true;
			this.BindingSource.SetBindingMember(this.DockDoorGuidFindBox, "DockDoorPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsPick)(null)).DockDoorPK)));
			this.DockDoorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 40, true);
			this.DockDoorGuidFindBox.Name = "DockDoorGuidFindBox";
			this.DockDoorGuidFindBox.PreBoundMaxLength = 23;
			this.DockDoorGuidFindBox.ShowDescriptionBox = false;
			this.DockDoorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.DockDoorGuidFindBox.TabIndex = 7;
			// 
			// PackingStationGuidFindBox
			// 
			this.PackingStationGuidFindBox.AllowDrop = true;
			this.PackingStationGuidFindBox.AutoCompleteDisabled = true;
			this.BindingSource.SetBindingMember(this.PackingStationGuidFindBox, "WP_WL_PackingStation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsPick)(null)).WP_WL_PackingStation)));
			this.PackingStationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 64, true);
			this.PackingStationGuidFindBox.Name = "PackingStationGuidFindBox";
			this.PackingStationGuidFindBox.PreBoundMaxLength = 23;
			this.PackingStationGuidFindBox.ShowDescriptionBox = false;
			this.PackingStationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.PackingStationGuidFindBox.TabIndex = 8;
			// 
			// PickCasesByLabelCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PickCasesByLabelCheckBox, "WP_PickCasesByLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((WhsPick)(null)).WP_PickCasesByLabel)));
			this.PickCasesByLabelCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PickCasesByLabelCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 42, true);
			this.PickCasesByLabelCheckBox.Name = "PickCasesByLabelCheckBox";
			this.PickCasesByLabelCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 16, true);
			this.PickCasesByLabelCheckBox.TabIndex = 11;
			this.PickCasesByLabelCheckBox.UseVisualStyleBackColor = true;
			// 
			// PickPalletsByLabelCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PickPalletsByLabelCheckBox, "WP_PickPalletsByLabel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((WhsPick)(null)).WP_PickPalletsByLabel)));
			this.PickPalletsByLabelCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PickPalletsByLabelCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 18, true);
			this.PickPalletsByLabelCheckBox.Name = "PickPalletsByLabelCheckBox";
			this.PickPalletsByLabelCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 16, true);
			this.PickPalletsByLabelCheckBox.TabIndex = 10;
			this.PickPalletsByLabelCheckBox.UseVisualStyleBackColor = true;
			// 
			// CartoniseCheckBox
			// 
			this.BindingSource.SetBindingMember(this.CartoniseCheckBox, "WP_CartoniseSplitCases");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((WhsPick)(null)).WP_CartoniseSplitCases)));
			this.CartoniseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CartoniseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 66, true);
			this.CartoniseCheckBox.Name = "CartoniseCheckBox";
			this.CartoniseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 16, true);
			this.CartoniseCheckBox.TabIndex = 12;
			this.CartoniseCheckBox.UseVisualStyleBackColor = true;
			// 
			// PickPriorityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PickPriorityCalcEdit, "PickPriority");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((INumericZType)(((WhsPick)(null)).PickPriority)));
			this.PickPriorityCalcEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|6F1087A6-4A0A-4BAF-998F-A0F01DEEC504", "Pick Priority");
			this.PickPriorityCalcEdit.DecimalPlaces = 0;
			this.PickPriorityCalcEdit.Decimals = 0;
			this.PickPriorityCalcEdit.IsCalculatorEnabled = false;
			this.PickPriorityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 88, true);
			this.PickPriorityCalcEdit.Name = "PickPriorityCalcEdit";
			this.PickPriorityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.PickPriorityCalcEdit.TabIndex = 5;
			this.PickPriorityCalcEdit.Text = "0";
			this.PickPriorityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PickPriorityCalcEdit.WordWrap = false;
			// 
			// IsAwaitingReplenishmentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsAwaitingReplenishmentCheckBox, "WP_IsAwaitingReplenishment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((WhsPick)(null)).WP_IsAwaitingReplenishment)));
			this.IsAwaitingReplenishmentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsAwaitingReplenishmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 66, true);
			this.IsAwaitingReplenishmentCheckBox.Name = "IsAwaitingReplenishmentCheckBox";
			this.IsAwaitingReplenishmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 16, true);
			this.IsAwaitingReplenishmentCheckBox.TabIndex = 3;
			this.IsAwaitingReplenishmentCheckBox.UseVisualStyleBackColor = true;
			// 
			// PieChartBox
			// 
			this.PieChartBox.BackColor = System.Drawing.SystemColors.Control;
			chartArea1.Area3DStyle.Enable3D = true;
			chartArea1.Area3DStyle.Inclination = 45;
			chartArea1.Area3DStyle.PointDepth = 200;
			chartArea1.BackColor = System.Drawing.SystemColors.Control;
			chartArea1.Name = "Pie3D";
			chartArea1.Position.Auto = false;
			chartArea1.Position.Width = 30;
			chartArea1.Position.Height = 100;
			this.PieChartBox.ChartAreas.Add(chartArea1);
			this.PieChartBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			legend1.BackColor = System.Drawing.SystemColors.Control;
			legend1.Name = "Pie3D";
			legend1.Position.Auto = false;
			legend1.Position.X = 30;
			legend1.Position.Y = 0;
			legend1.Position.Width = 70;
			legend1.Position.Height = 100;
			this.PieChartBox.Legends.Add(legend1);
			this.PieChartBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(920, 15, true);
			this.PieChartBox.Name = "PieChartBox";
			this.PieChartBox.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.SemiTransparent;
			series1.ChartArea = "Pie3D";
			series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
			series1.Legend = "Pie3D";
			series1.Name = "Pie3D";
			this.PieChartBox.Series.Add(series1);
			this.PieChartBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 90, true);
			this.PieChartBox.TabIndex = 16;
			this.PieChartBox.Text = "chart1";
			// 
			// PickStatusChartUnavailableLabel
			// 
			this.PickStatusChartUnavailableLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("14a6ec36-5d15-4641-8a4e-75ffab350f66", "Pick Status Chart Not Yet Available");
			this.PickStatusChartUnavailableLabel.Enabled = false;
			this.PickStatusChartUnavailableLabel.FontType = ((ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PickStatusChartUnavailableLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(992, 46, true);
			this.PickStatusChartUnavailableLabel.Name = "PickStatusChartUnavailableLabel";
			this.PickStatusChartUnavailableLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.PickStatusChartUnavailableLabel.TabIndex = 21;
			this.PickStatusChartUnavailableLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.PickStatusChartUnavailableLabel.Visible = false;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "WP_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsPick)(null)).WP_WW_Whs)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 16, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PreBoundMaxLength = 3;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.zGuidFindBox1.TabIndex = 6;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "WP_PickOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((WhsPick)(null)).WP_PickOption)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(570, 88, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.ShouldResizeByMaxLength = true;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.zDropEdit1.TabIndex = 13;
			// 
			// PickStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.PickStatusTextBox, "StatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsPick)(null)).StatusDesc)));
			this.PickStatusTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("PickHeaderUserControl|751bc522-0717-4da6-90b8-e8081228aa60", "Status");
			this.PickStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 40, true);
			this.PickStatusTextBox.Name = "PickStatusTextBox";
			this.PickStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 13, true);
			this.PickStatusTextBox.TabIndex = 2;
			// 
			// PickNoTextBox1
			// 
			this.BindingSource.SetBindingMember(this.PickNoTextBox1, "WP_PickNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsPick)(null)).WP_PickNo)));
			this.PickNoTextBox1.CaptionResourceString = null;
			this.PickNoTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 16, true);
			this.PickNoTextBox1.Name = "PickNoTextBox1";
			this.PickNoTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 13, true);
			this.PickNoTextBox1.TabIndex = 1;
			// 
			// DisplayAwaitingReplenishmentPicksButton
			// 
			this.DisplayAwaitingReplenishmentPicksButton.IsCaptionOverridden = true;
			this.DisplayAwaitingReplenishmentPicksButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 63, true);
			this.DisplayAwaitingReplenishmentPicksButton.Name = "DisplayAwaitingReplenishmentPicksButton";
			this.DisplayAwaitingReplenishmentPicksButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.DisplayAwaitingReplenishmentPicksButton.TabIndex = 4;
			this.DisplayAwaitingReplenishmentPicksButton.Text = "...";
			this.DisplayAwaitingReplenishmentPicksButton.ToolTipCaption = null;
			this.DisplayAwaitingReplenishmentPicksButton.Click += new EventHandler(this.DisplayAwaitingReplenishmentPicksButton_Click);
			// 
			// PickHeaderUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PickInfoGroupBox);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			this.Name = "PickHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 115, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PickInfoGroupBox.ResumeLayout(false);
			this.PickInfoGroupBox.PerformLayout();
			this.PickAreaOverride.ResumeLayout(true);
			this.PickAreaOverride.PerformLayout();
			this.DockDoorGuidFindBox.ResumeLayout(true);
			this.DockDoorGuidFindBox.PerformLayout();
			this.PackingStationGuidFindBox.ResumeLayout(true);
			this.PackingStationGuidFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PieChartBox)).EndInit();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
