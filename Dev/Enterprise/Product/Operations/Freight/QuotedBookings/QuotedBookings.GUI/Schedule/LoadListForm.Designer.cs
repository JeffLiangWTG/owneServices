using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.QuotedBookings.GUI
{
	/// <summary>
	/// Summary description for LoadListForm.
	/// </summary>
	public partial class LoadListForm : ZForm
	{
		#region Auto

		private ZArchitecture.ZLabel VoyageLabel;
		private ZArchitecture.ZLabel VesselNameLabel;
		private ZGroupBox SailingSummaryGroupBox;
		private ZArchitecture.ZLabel PackagesLabel;
		private ZArchitecture.ZLabel WeightLabel;
		private ZArchitecture.ZLabel VolumeLabel;
		private ZCalcDropEdit JX_Calc_ReceivedWeightBoundCalcDropEdit;
		private ZCodeFindBox JX_JA_RL_NKPortOfLoadingBoundCodeFindBox;
		private ZCodeFindBox JX_JB_RL_NKPortOfDischargeBoundCodeFindBox;
		private ZDateEdit JX_CTOCutOffDateEdit;
		private ZDateEdit JX_DepotCutOffDateEdit;
		private ZDateEdit JX_JB_E_ARVDateEdit;
		private ZDateEdit JX_JA_E_DEPDateEdit;
		private ZCalcDropEdit TotalVolumeCalcDropEdit;
		private ZCalcDropEdit TotalWeightCalcDropEdit;
		private ZCalcDropEdit TotalPackagesCalcDropEdit;
		private ZArchitecture.ZGrid ShipmentsGrid;
		private Core.Forms.ZPostingButtonsUserControl LoadListPostingButtonsUserControl;
		private ZArchitecture.ZTextBox JX_JV_VoyageFlightBoundTextBox;
		private ZArchitecture.ZTextBox JX_JV_NKVesselBoundTextBox;
		private ZCalcDropEdit JX_Calc_ReceivedPackagesBoundCalcDropEdit;
		private ZCalcDropEdit JX_Calc_ReceivedVolumeBoundCalcDropEdit;
		private ZMenuItem CombineShipmentMenuItem;
		private ZTemplateTabControl LoadListTabControl;
		private ZTabPage LoadListTabPage;
		private ZStmNoteTabPage zStmNoteTabPage1;
		private ZLogsTabPage zEventTabPage1;
		private ZArchitecture.ZCalcEdit TotalTEUCalcEdit;
		private ZArchitecture.ZCalcEdit TotalContainersCalcEdit;

		#endregion
		private System.ComponentModel.IContainer components;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.JX_JV_VoyageFlightBoundTextBox = new ZArchitecture.ZTextBox();
			this.JX_JV_NKVesselBoundTextBox = new ZArchitecture.ZTextBox();
			this.VoyageLabel = new ZArchitecture.ZLabel();
			this.VesselNameLabel = new ZArchitecture.ZLabel();
			this.PackagesLabel = new ZArchitecture.ZLabel();
			this.WeightLabel = new ZArchitecture.ZLabel();
			this.VolumeLabel = new ZArchitecture.ZLabel();
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox = new ZCodeFindBox();
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox = new ZCodeFindBox();
			this.SailingSummaryGroupBox = new ZGroupBox();
			this.TotalContainersCalcEdit = new ZArchitecture.ZCalcEdit();
			this.TotalTEUCalcEdit = new ZArchitecture.ZCalcEdit();
			this.JX_JB_E_ARVDateEdit = new ZDateEdit();
			this.JX_JA_E_DEPDateEdit = new ZDateEdit();
			this.JX_CTOCutOffDateEdit = new ZDateEdit();
			this.JX_DepotCutOffDateEdit = new ZDateEdit();
			this.JX_Calc_ReceivedWeightBoundCalcDropEdit = new ZCalcDropEdit();
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit = new ZCalcDropEdit();
			this.TotalPackagesCalcDropEdit = new ZCalcDropEdit();
			this.TotalVolumeCalcDropEdit = new ZCalcDropEdit();
			this.JX_Calc_ReceivedVolumeBoundCalcDropEdit = new ZCalcDropEdit();
			this.TotalWeightCalcDropEdit = new ZCalcDropEdit();
			this.LoadListPostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.ShipmentsGrid = new ZArchitecture.ZGrid();
			this.CombineShipmentMenuItem = new ZMenuItem();
			this.LoadListTabControl = new ZTemplateTabControl();
			this.LoadListTabPage = new ZTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SailingSummaryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).BeginInit();
			this.LoadListTabControl.SuspendLayout();
			this.LoadListTabPage.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 732, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 24, true);
			this.MainStatusBar.TabIndex = 2;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(452);
			//
			// ErrorStatusBarPanel
			//
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(453);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(JobSailing);
			//
			// JX_JV_VoyageFlightBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JX_JV_VoyageFlightBoundTextBox, "JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).JX_JV_VoyageFlight);
			this.JX_JV_VoyageFlightBoundTextBox.CaptionResourceString = Res.GetData("LoadListForm|e9f43b04-607f-43e0-8463-92063b3e2881", "Voyage");
			this.JX_JV_VoyageFlightBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 7, true);
			this.JX_JV_VoyageFlightBoundTextBox.Name = "JX_JV_VoyageFlightBoundTextBox";
			this.JX_JV_VoyageFlightBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JX_JV_VoyageFlightBoundTextBox.TabIndex = 1;
			//
			// JX_JV_NKVesselBoundTextBox
			//
			this.BindingSource.SetBindingMember(this.JX_JV_NKVesselBoundTextBox, "JX_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).JX_JV_NKVessel);
			this.JX_JV_NKVesselBoundTextBox.CaptionResourceString = Res.GetData("LoadListForm|49b00e94-96bb-42a6-8fa4-431a43b96dd5", "Vessel");
			this.JX_JV_NKVesselBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 7, true);
			this.JX_JV_NKVesselBoundTextBox.Name = "JX_JV_NKVesselBoundTextBox";
			this.JX_JV_NKVesselBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.JX_JV_NKVesselBoundTextBox.TabIndex = 3;
			//
			// VoyageLabel
			//
			this.VoyageLabel.CaptionResourceString = Res.GetData("LoadListForm|e8b104a6-3787-4ba8-a582-40c32529c425", "Voyage No.:");
			this.VoyageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 5, true);
			this.VoyageLabel.Name = "VoyageLabel";
			this.VoyageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.VoyageLabel.TabIndex = 0;
			//
			// VesselNameLabel
			//
			this.VesselNameLabel.CaptionResourceString = Res.GetData("LoadListForm|fa702340-5939-4cf7-b346-97861e67263e", "Vessel Name:");
			this.VesselNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 5, true);
			this.VesselNameLabel.Name = "VesselNameLabel";
			this.VesselNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.VesselNameLabel.TabIndex = 2;
			//
			// PackagesLabel
			//
			this.PackagesLabel.AutoSize = true;
			this.PackagesLabel.CaptionResourceString = Res.GetData("LoadListForm|b4084634-ffb8-4a42-bdec-a2d27ca8dcc8", "Packages");
			this.PackagesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 15, true);
			this.PackagesLabel.Name = "PackagesLabel";
			this.PackagesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.PackagesLabel.TabIndex = 8;
			//
			// WeightLabel
			//
			this.WeightLabel.AutoSize = true;
			this.WeightLabel.CaptionResourceString = Res.GetData("LoadListForm|60d9ed25-a15c-4099-af4a-e061928d1d4e", "Weight");
			this.WeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 15, true);
			this.WeightLabel.Name = "WeightLabel";
			this.WeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.WeightLabel.TabIndex = 9;
			//
			// VolumeLabel
			//
			this.VolumeLabel.AutoSize = true;
			this.VolumeLabel.CaptionResourceString = Res.GetData("LoadListForm|e14fcac9-37aa-4d6e-9fc0-96a3da0d3999", "Volume");
			this.VolumeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 15, true);
			this.VolumeLabel.Name = "VolumeLabel";
			this.VolumeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.VolumeLabel.TabIndex = 10;
			//
			// JX_JA_RL_NKPortOfLoadingBoundCodeFindBox
			//
			this.BindingSource.SetBindingMember(this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox, "JX_JA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).JX_JA_RL_NKPortOfLoading);
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.CaptionResourceString = Res.GetData("LoadListForm|33eb00a4-46a0-4372-aff7-71d02850f53b", "Load");
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(424, 7, true);
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.Name = "JX_JA_RL_NKPortOfLoadingBoundCodeFindBox";
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.TabIndex = 5;
			//
			// JX_JB_RL_NKPortOfDischargeBoundCodeFindBox
			//
			this.BindingSource.SetBindingMember(this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox, "JX_JB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).JX_JB_RL_NKPortOfDischarge);
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.CaptionResourceString = Res.GetData("LoadListForm|88aa1ac6-fd86-47e7-bb2f-b8d6b60be122", "Disch.", "Discharge");
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 7, true);
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.Name = "JX_JB_RL_NKPortOfDischargeBoundCodeFindBox";
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.TabIndex = 7;
			//
			// SailingSummaryGroupBox
			//
			this.SailingSummaryGroupBox.CaptionResourceString = Res.GetData("LoadListForm|261eae4d-fd52-4310-91f1-60e73f394132", "Sailing Summary");
			this.SailingSummaryGroupBox.Controls.Add(this.TotalContainersCalcEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.TotalTEUCalcEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.JX_JB_E_ARVDateEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.JX_JA_E_DEPDateEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.JX_CTOCutOffDateEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.JX_DepotCutOffDateEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.PackagesLabel);
			this.SailingSummaryGroupBox.Controls.Add(this.WeightLabel);
			this.SailingSummaryGroupBox.Controls.Add(this.VolumeLabel);
			this.SailingSummaryGroupBox.Controls.Add(this.JX_Calc_ReceivedWeightBoundCalcDropEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.JX_Calc_ReceivedPackagesBoundCalcDropEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.TotalPackagesCalcDropEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.TotalVolumeCalcDropEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.JX_Calc_ReceivedVolumeBoundCalcDropEdit);
			this.SailingSummaryGroupBox.Controls.Add(this.TotalWeightCalcDropEdit);
			this.SailingSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 334, true);
			this.SailingSummaryGroupBox.Name = "SailingSummaryGroupBox";
			this.SailingSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 96, true);
			this.SailingSummaryGroupBox.TabIndex = 9;
			this.SailingSummaryGroupBox.TabStop = false;
			//
			// TotalContainersCalcEdit
			//
			this.BindingSource.SetBindingMember(this.TotalContainersCalcEdit, "TotalContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).TotalContainers);
			this.TotalContainersCalcEdit.CaptionResourceString = Res.GetData("LoadListForm|d48e4e76-686e-4527-b3e0-6300c07ddacd", "Containers");
			this.TotalContainersCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(840, 67, true);
			this.TotalContainersCalcEdit.Name = "TotalContainersCalcEdit";
			this.TotalContainersCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.TotalContainersCalcEdit.TabIndex = 22;
			this.TotalContainersCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// TotalTEUCalcEdit
			//
			this.BindingSource.SetBindingMember(this.TotalTEUCalcEdit, "TotalTEU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).TotalTEU);
			this.TotalTEUCalcEdit.CaptionResourceString = Res.GetData("LoadListForm|3ffbe3e6-422c-4f32-98e2-73e20bf9c930", "TEU");
			this.TotalTEUCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(840, 37, true);
			this.TotalTEUCalcEdit.Name = "TotalTEUCalcEdit";
			this.TotalTEUCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.TotalTEUCalcEdit.TabIndex = 16;
			this.TotalTEUCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// JX_JB_E_ARVDateEdit
			//
			this.JX_JB_E_ARVDateEdit.AutoCompleteMonthThreshold = 1;
			this.JX_JB_E_ARVDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JX_JB_E_ARVDateEdit, "JX_JB_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).JX_JB_E_ARV);
			this.JX_JB_E_ARVDateEdit.CaptionResourceString = Res.GetData("LoadListForm|a12b391d-3b3e-4534-b801-32f7ca558e1e", "ETA");
			this.JX_JB_E_ARVDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 67, true);
			this.JX_JB_E_ARVDateEdit.Name = "JX_JB_E_ARVDateEdit";
			this.JX_JB_E_ARVDateEdit.TabIndex = 7;
			//
			// JX_JA_E_DEPDateEdit
			//
			this.JX_JA_E_DEPDateEdit.AutoCompleteMonthThreshold = 1;
			this.JX_JA_E_DEPDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JX_JA_E_DEPDateEdit, "JX_JA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).JX_JA_E_DEP);
			this.JX_JA_E_DEPDateEdit.CaptionResourceString = Res.GetData("LoadListForm|332a2a32-506c-463b-a34a-3cb085fa398f", "ETD");
			this.JX_JA_E_DEPDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 67, true);
			this.JX_JA_E_DEPDateEdit.Name = "JX_JA_E_DEPDateEdit";
			this.JX_JA_E_DEPDateEdit.TabIndex = 5;
			//
			// JX_FCLCutOffDateEdit
			//
			this.JX_CTOCutOffDateEdit.AutoCompleteMonthThreshold = 1;
			this.JX_CTOCutOffDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JX_CTOCutOffDateEdit, "JX_JA_CTOCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).JX_JA_CTOCutOff);
			this.JX_CTOCutOffDateEdit.CaptionResourceString = Res.GetData("3bbe004b-2a55-4299-b04d-c2a9bde13a30", "CTO Cut Off");
			this.JX_CTOCutOffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 37, true);
			this.JX_CTOCutOffDateEdit.Name = "JX_FCLCutOffDateEdit";
			this.JX_CTOCutOffDateEdit.TabIndex = 3;
			//
			// JX_DepotCutOffDateEdit
			//
			this.JX_DepotCutOffDateEdit.AutoCompleteMonthThreshold = 1;
			this.JX_DepotCutOffDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JX_DepotCutOffDateEdit, "JX_DepotCutOff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).JX_DepotCutOff);
			this.JX_DepotCutOffDateEdit.CaptionResourceString = Res.GetData("LoadListForm|18317c88-c6c6-421d-9e47-f8f4d5d2d4e2", "CFS Cut Off");
			this.JX_DepotCutOffDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 37, true);
			this.JX_DepotCutOffDateEdit.Name = "JX_DepotCutOffDateEdit";
			this.JX_DepotCutOffDateEdit.TabIndex = 1;
			//
			// JX_Calc_ReceivedWeightBoundCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.JX_Calc_ReceivedWeightBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).ReceivedWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).ReceivedWeightUnit);
			this.JX_Calc_ReceivedWeightBoundCalcDropEdit.BindToAmount = "ReceivedWeight";
			this.JX_Calc_ReceivedWeightBoundCalcDropEdit.BindToUnit = "ReceivedWeightUnit";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JX_Calc_ReceivedWeightBoundCalcDropEdit, false);
			this.JX_Calc_ReceivedWeightBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 37, true);
			this.JX_Calc_ReceivedWeightBoundCalcDropEdit.Name = "JX_Calc_ReceivedWeightBoundCalcDropEdit";
			this.JX_Calc_ReceivedWeightBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JX_Calc_ReceivedWeightBoundCalcDropEdit.TabIndex = 13;
			this.JX_Calc_ReceivedWeightBoundCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// JX_Calc_ReceivedPackagesBoundCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.JX_Calc_ReceivedPackagesBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).ReceivedPackages);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).ReceivedPackagesUnit);
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit.BindToAmount = "ReceivedPackages";
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit.BindToUnit = "ReceivedPackagesUnit";
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit.CaptionResourceString = Res.GetData("LoadListForm|ec289864-5f5d-4cae-b0cf-63df44a38700", "Received", "Received Packages", "");
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit.Decimals = 0;
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 37, true);
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit.Name = "JX_Calc_ReceivedPackagesBoundCalcDropEdit";
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit.TabIndex = 12;
			this.JX_Calc_ReceivedPackagesBoundCalcDropEdit.UnitPreBoundMaxLength = 3;
			//
			// TotalPackagesCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.TotalPackagesCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).TotalPackages);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).TotalPackagesUnit);
			this.TotalPackagesCalcDropEdit.BindToAmount = "TotalPackages";
			this.TotalPackagesCalcDropEdit.BindToUnit = "TotalPackagesUnit";
			this.TotalPackagesCalcDropEdit.CaptionResourceString = Res.GetData("LoadListForm|8c366cc4-4395-4561-a57c-1414c5129c82", "Total", "Total Packages", "");
			this.TotalPackagesCalcDropEdit.Decimals = 0;
			this.TotalPackagesCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(416, 67, true);
			this.TotalPackagesCalcDropEdit.Name = "TotalPackagesCalcDropEdit";
			this.TotalPackagesCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.TotalPackagesCalcDropEdit.TabIndex = 18;
			this.TotalPackagesCalcDropEdit.UnitPreBoundMaxLength = 3;
			//
			// TotalVolumeCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.TotalVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).TotalVolume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).TotalVolumeUnit);
			this.TotalVolumeCalcDropEdit.BindToAmount = "TotalVolume";
			this.TotalVolumeCalcDropEdit.BindToUnit = "TotalVolumeUnit";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalVolumeCalcDropEdit, false);
			this.TotalVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 67, true);
			this.TotalVolumeCalcDropEdit.Name = "TotalVolumeCalcDropEdit";
			this.TotalVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TotalVolumeCalcDropEdit.TabIndex = 20;
			this.TotalVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// JX_Calc_ReceivedVolumeBoundCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.JX_Calc_ReceivedVolumeBoundCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).ReceivedVolume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).ReceivedVolumeUnit);
			this.JX_Calc_ReceivedVolumeBoundCalcDropEdit.BindToAmount = "ReceivedVolume";
			this.JX_Calc_ReceivedVolumeBoundCalcDropEdit.BindToUnit = "ReceivedVolumeUnit";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JX_Calc_ReceivedVolumeBoundCalcDropEdit, false);
			this.JX_Calc_ReceivedVolumeBoundCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 37, true);
			this.JX_Calc_ReceivedVolumeBoundCalcDropEdit.Name = "JX_Calc_ReceivedVolumeBoundCalcDropEdit";
			this.JX_Calc_ReceivedVolumeBoundCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JX_Calc_ReceivedVolumeBoundCalcDropEdit.TabIndex = 14;
			this.JX_Calc_ReceivedVolumeBoundCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// TotalWeightCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.TotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).TotalWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).TotalWeightUnit);
			this.TotalWeightCalcDropEdit.BindToAmount = "TotalWeight";
			this.TotalWeightCalcDropEdit.BindToUnit = "TotalWeightUnit";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalWeightCalcDropEdit, false);
			this.TotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 67, true);
			this.TotalWeightCalcDropEdit.Name = "TotalWeightCalcDropEdit";
			this.TotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TotalWeightCalcDropEdit.TabIndex = 19;
			this.TotalWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// LoadListPostingButtonsUserControl
			//
			this.LoadListPostingButtonsUserControl.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.LoadListPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(672, 698, true);
			this.LoadListPostingButtonsUserControl.Name = "LoadListPostingButtonsUserControl";
			this.LoadListPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.LoadListPostingButtonsUserControl.TabIndex = 1;
			//
			// ShipmentsGrid
			//
			this.ShipmentsGrid.AllowNavigation = false;
			this.ShipmentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.ShipmentsGrid, "AllBookingsOnSailing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((JobSailing)(null)).AllBookingsOnSailing);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_UniqueConsignRef);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).ConsignorPK);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).ConsignorDocumentaryAddress.E2_Contact);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_OuterPacks);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_F3_NKPackType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_ActualWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_UnitOfWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_ActualVolume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_UnitOfVolume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_ActualChargeable);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_GoodsValue);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_RX_NKGoodsValueCurr);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_GoodsDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_Calc_CurrentVessel);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_Calc_CurrentVoyageFlight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_PackingMode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_Calc_CurrentETD);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_Calc_CurrentETA);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).Sailing.JX_DepotReceivalCommences);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).Sailing.JX_DepotCutOff);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).Sailing.JX_JA_CTOReceivalCommences);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).Sailing.JX_JA_CTOCutOff);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_Calc_CurrentLoadPort);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_Calc_CurrentDischargePort);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_RL_NKOrigin);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_RL_NKDestination);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).ConsigneePK);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).DocsAndCartage.JP_FCLDeliveryEquipmentNeeded);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_BookingReference);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_InterimReceipt);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_A_RCV);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((System.Collections.IList)(((JobSailing)(null)).AllBookingsOnSailing)).SyncRoot)).JS_Calc_TEUCount);
			this.ShipmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("LoadListForm|fb95ceca-0d5f-4596-9d12-3465ac444029", "Booking ID");
			zTextBoxColumnStyleInfo1.ColumnName = "JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("LoadListForm|a23de3af-638e-4b9d-9851-42b3232390b3", "Consignor");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ConsignorPK";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("LoadListForm|73911ea1-1d23-4dc3-8638-a1ae19388fa4", "Contact");
			zTextBoxColumnStyleInfo2.ColumnName = "ConsignorDocumentaryAddress+E2_Contact";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("LoadListForm|acb730d4-3171-4caf-8a37-6232e331d513", "Packs");
			zCalcEditColumnStyleInfo1.ColumnName = "JS_OuterPacks";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("LoadListForm|e4263015-6145-4907-bf00-9934da2abdde", "Type");
			zTextBoxColumnStyleInfo3.ColumnName = "JS_F3_NKPackType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JS_ActualWeight";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Res.GetData("LoadListForm|6a47bc25-b894-40ca-966e-bc77d2b2308c", "UW");
			zTextBoxColumnStyleInfo4.ColumnName = "JS_UnitOfWeight";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JS_ActualVolume";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Res.GetData("LoadListForm|a4935277-ca3d-4266-b924-a70e3dbeb388", "UV");
			zTextBoxColumnStyleInfo5.ColumnName = "JS_UnitOfVolume";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Res.GetData("LoadListForm|2f6e350f-21fb-4140-ac54-38ec10a17a0c", "Chargeable");
			zCalcEditColumnStyleInfo4.ColumnName = "JS_ActualChargeable";
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Res.GetData("LoadListForm|eb15635b-b07f-48ad-811b-0627af4b7739", "Value");
			zCalcEditColumnStyleInfo5.ColumnName = "JS_GoodsValue";
			zCalcEditColumnStyleInfo5.GroupName = Res.GetData("LoadListForm|523e2920-406b-43c5-94c7-4e2705ec6165", "Value");
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Res.GetData("LoadListForm|eda57871-b0fb-4cb5-b969-dc883de2e8fc", "Curr.");
			zTextBoxColumnStyleInfo6.ColumnName = "JS_RX_NKGoodsValueCurr";
			zTextBoxColumnStyleInfo6.GroupName = Res.GetData("LoadListForm|523e2920-406b-43c5-94c7-4e2705ec6165", "Value");
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo7.ColumnName = "JS_GoodsDescription";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Res.GetData("LoadListForm|f84dbc22-7895-438e-8e6e-597ee68e0d4c", "Vessel Name");
			zTextBoxColumnStyleInfo8.ColumnName = "JS_Calc_CurrentVessel";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Res.GetData("LoadListForm|4589a827-57e2-4ca9-91e9-999fc084f7da", "Voyage No");
			zTextBoxColumnStyleInfo9.ColumnName = "JS_Calc_CurrentVoyageFlight";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Res.GetData("LoadListForm|75c7510c-12ff-445b-aad6-56e93fedb252", "Mode");
			zTextBoxColumnStyleInfo10.ColumnName = "JS_PackingMode";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDateEditColumnStyleInfo1.CaptionResourceString = Res.GetData("LoadListForm|a221405f-7a26-4b6a-87d5-a301acc32bfa", "ETD");
			zDateEditColumnStyleInfo1.ColumnName = "JS_Calc_CurrentETD";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo2.CaptionResourceString = Res.GetData("LoadListForm|2ffbf0b6-4e15-4125-b07f-0e8d1977b21c", "ETA");
			zDateEditColumnStyleInfo2.ColumnName = "JS_Calc_CurrentETA";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo3.CaptionResourceString = Res.GetData("LoadListForm|99ec6860-1c86-4681-8e5d-5a36ae271009", "CFS Recv.", "CFS Receival Start");
			zDateEditColumnStyleInfo3.ColumnName = "Sailing+JX_DepotReceivalCommences";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo4.CaptionResourceString = Res.GetData("LoadListForm|5bd44bd6-5ec7-4dd2-8ab7-95e5433505b4", "CFS Cut", "CFS Cut Off");
			zDateEditColumnStyleInfo4.ColumnName = "Sailing+JX_DepotCutOff";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo5.CaptionResourceString = Res.GetData("LoadListForm|ba9e69fd-2c86-475b-9ee3-a4ad16bfa7fc", "CTO Recv.", "CTO Receival Start");
			zDateEditColumnStyleInfo5.ColumnName = "Sailing+JX_JA_CTOReceivalCommences";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo6.CaptionResourceString = Res.GetData("LoadListForm|48ff97f6-d56d-43c1-8237-6fc85fc72786", "CTO Cut Off");
			zDateEditColumnStyleInfo6.ColumnName = "Sailing+JX_JA_CTOCutOff";
			zDateEditColumnStyleInfo6.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo6.IsReadOnly = true;
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Res.GetData("LoadListForm|f1e7ef23-62c6-4590-b1ec-1785906fa9dc", "Load");
			zTextBoxColumnStyleInfo11.ColumnName = "JS_Calc_CurrentLoadPort";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Res.GetData("LoadListForm|05d44ff2-4c3b-4c43-b0c8-a67a0d4ed16f", "Disch.", "Discharge");
			zTextBoxColumnStyleInfo12.ColumnName = "JS_Calc_CurrentDischargePort";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Res.GetData("LoadListForm|9af27528-7831-4ff7-b320-a85fc32c8631", "Origin");
			zTextBoxColumnStyleInfo13.ColumnName = "JS_RL_NKOrigin";
			zTextBoxColumnStyleInfo13.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Res.GetData("LoadListForm|4515980a-1de9-4a19-a7e1-1147f95baea4", "Dest.", "Destination");
			zTextBoxColumnStyleInfo14.ColumnName = "JS_RL_NKDestination";
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("LoadListForm|e96b1cf0-7432-4912-8ea0-35b422e8c759", "Consignee");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ConsigneePK";
			zGuidFindBoxColumnStyleInfo2.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo15.CaptionResourceString = Res.GetData("LoadListForm|bcff111a-b7c8-4667-afa3-6399742bab1e", "Equip. Req.", "Equipment Required.");
			zTextBoxColumnStyleInfo15.ColumnName = "DocsAndCartage+JP_FCLDeliveryEquipmentNeeded";
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Res.GetData("LoadListForm|79d9b277-d5ed-43f1-b353-b2cd3c8b6254", "Shippers Ref", "Shippers Reference.");
			zTextBoxColumnStyleInfo16.ColumnName = "JS_BookingReference";
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Res.GetData("LoadListForm|9b6572aa-9435-4ec6-abbc-080900804b95", "Interim #");
			zTextBoxColumnStyleInfo17.ColumnName = "JS_InterimReceipt";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zDateEditColumnStyleInfo7.CaptionResourceString = Res.GetData("LoadListForm|86de3aee-ac19-4c8c-b101-c95e82e6edfa", "Recv. Date", "Receival Date");
			zDateEditColumnStyleInfo7.ColumnName = "JS_A_RCV";
			zDateEditColumnStyleInfo7.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Res.GetData("LoadListForm|0dc18d3a-17d4-4cdd-8f2a-8c58e6f624ad", "TEU");
			zCalcEditColumnStyleInfo6.ColumnName = "JS_Calc_TEUCount";
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ShipmentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ShipmentsGrid.GridId = "f6edbfee-6007-4246-b9a8-9b304d68e894";
			this.ShipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentsGrid.LayoutKey = "ShipmentGrid";
			this.ShipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.ShipmentsGrid.Name = "ShipmentsGrid";
			this.ShipmentsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ShipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(896, 290, true);
			this.ShipmentsGrid.TabIndex = 8;
			//
			// CombineShipmentMenuItem
			//
			this.CombineShipmentMenuItem.Index = -1;
			this.CombineShipmentMenuItem.Text = "";
			//
			// LoadListTabControl
			//
			this.LoadListTabControl.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.LoadListTabControl.Controls.Add(this.LoadListTabPage);
			this.LoadListTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.LoadListTabControl.Controls.Add(this.zEventTabPage1);
			this.LoadListTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 7, true);
			this.LoadListTabControl.Name = "LoadListTabControl";
			this.LoadListTabControl.SelectedIndex = 0;
			this.LoadListTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 461, true);
			this.LoadListTabControl.TabIndex = 0;
			//
			// LoadListTabPage
			//
			this.LoadListTabPage.CaptionResourceString = Res.GetData("LoadListForm|6b7a4c28-09ad-4e9c-b13c-5c9810f7f8c4", "Load List");
			this.LoadListTabPage.Controls.Add(this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox);
			this.LoadListTabPage.Controls.Add(this.JX_JV_NKVesselBoundTextBox);
			this.LoadListTabPage.Controls.Add(this.VesselNameLabel);
			this.LoadListTabPage.Controls.Add(this.JX_JV_VoyageFlightBoundTextBox);
			this.LoadListTabPage.Controls.Add(this.VoyageLabel);
			this.LoadListTabPage.Controls.Add(this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox);
			this.LoadListTabPage.Controls.Add(this.ShipmentsGrid);
			this.LoadListTabPage.Controls.Add(this.SailingSummaryGroupBox);
			this.LoadListTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LoadListTabPage.Name = "LoadListTabPage";
			this.LoadListTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 434, true);
			this.LoadListTabPage.TabIndex = 0;
			//
			// zStmNoteTabPage1
			//
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 434, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			//
			// zEventTabPage1
			//
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 434, true);
			this.zEventTabPage1.TabIndex = 2;
			//
			// LoadListForm
			//

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 756, true);
			this.CaptionResourceString = Res.GetData("LoadListForm|30f6cc45-d176-433f-8547-f20f8d709f69", "Load List");
			this.Controls.Add(this.LoadListPostingButtonsUserControl);
			this.Controls.Add(this.LoadListTabControl);
			this.DataSourceType = typeof(JobSailing);
			this.Name = "LoadListForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.LoadListTabControl, 0);
			this.Controls.SetChildIndex(this.LoadListPostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SailingSummaryGroupBox.ResumeLayout(false);
			this.SailingSummaryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).EndInit();
			this.LoadListTabControl.ResumeLayout(false);
			this.LoadListTabPage.ResumeLayout(false);
			this.LoadListTabPage.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
