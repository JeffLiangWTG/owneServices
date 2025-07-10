using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.UserControls
{
	partial class AirRefContainerControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private IContainer components = null;

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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.LengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InsideLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InsideWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InsideHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HasTynesCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DimensionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TEUCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel21 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel20 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel19 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit9 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel18 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit8 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CapacityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NetWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IATARateClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ShippingModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RC_ContainerTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RC_HasVentsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsHighCubeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ISOEquipmentSizeTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HandlingRatingClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FreightRatingClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StorageClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ISOTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OversizeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsISOCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ISODescrioptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ISOSizeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ISOTypeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel29 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel30 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel31 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel32 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel33 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel34 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel35 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel36 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel37 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel38 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit10 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit11 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit12 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit13 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit14 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ContainerClasssGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CodeMapsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CodeMapsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DimensionsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.BaseDimensionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InsideDimensionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DescriptionTextBox.SuspendLayout();
			this.DimensionsGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.IATARateClassDropEdit.SuspendLayout();
			this.ShippingModeDropEdit.SuspendLayout();
			this.RC_ContainerTypeBoundDropEdit.SuspendLayout();
			this.ISOEquipmentSizeTypeDropEdit.SuspendLayout();
			this.HandlingRatingClassDropEdit.SuspendLayout();
			this.FreightRatingClassDropEdit.SuspendLayout();
			this.StorageClassDropEdit.SuspendLayout();
			this.ISOTypeGroupBox.SuspendLayout();
			this.ISOTypeFindBox.SuspendLayout();
			this.ContainerClasssGroupBox.SuspendLayout();
			this.CodeMapsGroupBox.SuspendLayout();
			this.DimensionsTabControl.SuspendLayout();
			this.BaseDimensionsTabPage.SuspendLayout();
			this.InsideDimensionsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeMapsGrid)).BeginInit();
			this.CodeMapsGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// CodeTextBox
			//
			this.BindingSource.SetBindingMember(this.CodeTextBox, "RC_Code");
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 24, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CodeTextBox.TabIndex = 1;
			//
			// DescriptionTextBox
			//
			this.DescriptionTextBox.AcceptsReturn = false;
			this.DescriptionTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "RC_Description");
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.GridCurrent = null;
			this.DescriptionTextBox.GridMember = null;
			this.DescriptionTextBox.IsLanguageEditingEnabled = true;
			this.DescriptionTextBox.IsMultiLine = false;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 48, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.ReadOnly = false;
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.DescriptionTextBox.TabIndex = 6;
			//
			// LengthCalcEdit
			//
			this.BindingSource.SetBindingMember(this.LengthCalcEdit, "LengthInches");
			this.LengthCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|9ab38653-9a0e-48e1-b218-0c4ee6f72494", "Container Length");
			this.LengthCalcEdit.DecimalPlaces = 3;
			this.LengthCalcEdit.Decimals = 3;
			this.LengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 8, true);
			this.LengthCalcEdit.Name = "LengthCalcEdit";
			this.LengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.LengthCalcEdit.TabIndex = 0;
			this.LengthCalcEdit.Text = "0.000";
			this.LengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// WidthCalcEdit
			//
			this.BindingSource.SetBindingMember(this.WidthCalcEdit, "WidthInches");
			this.WidthCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|8e6916ed-29cb-4a3a-b7a8-72ef6e705ecd", "Container Width");
			this.WidthCalcEdit.DecimalPlaces = 3;
			this.WidthCalcEdit.Decimals = 3;
			this.WidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 32, true);
			this.WidthCalcEdit.Name = "WidthCalcEdit";
			this.WidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.WidthCalcEdit.TabIndex = 7;
			this.WidthCalcEdit.Text = "0.000";
			this.WidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// HeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.HeightCalcEdit, "HeightInches");
			this.HeightCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|ec12ea16-918c-4ad8-8ab1-96bda9c103af", "Container Height");
			this.HeightCalcEdit.DecimalPlaces = 3;
			this.HeightCalcEdit.Decimals = 3;
			this.HeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 56, true);
			this.HeightCalcEdit.Name = "HeightCalcEdit";
			this.HeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.HeightCalcEdit.TabIndex = 14;
			this.HeightCalcEdit.Text = "0.000";
			this.HeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// HasTynesCheckEdit
			//
			this.HasTynesCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HasTynesCheckEdit, "RC_HasTynes");
			this.HasTynesCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.HasTynesCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HasTynesCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 75, true);
			this.HasTynesCheckEdit.Name = "HasTynesCheckEdit";
			this.HasTynesCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.HasTynesCheckEdit.TabIndex = 9;
			//
			// DimensionsGroupBox
			//
			this.DimensionsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|4a7be46f-793f-4f15-a6c4-eb3cd293133d", "Container Dimensions");
			this.DimensionsGroupBox.Controls.Add(this.DimensionsTabControl);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit10);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit11);
			this.DimensionsGroupBox.Controls.Add(this.zLabel30);
			this.DimensionsGroupBox.Controls.Add(this.zLabel29);
			this.DimensionsGroupBox.Controls.Add(this.zLabel32);
			this.DimensionsGroupBox.Controls.Add(this.zLabel31);
			this.DimensionsGroupBox.Controls.Add(this.TEUCalcEdit);
			this.DimensionsGroupBox.Controls.Add(this.CapacityCalcEdit);
			this.DimensionsGroupBox.Controls.Add(this.NetWeightCalcEdit);
			this.DimensionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 364, true);
			this.DimensionsGroupBox.Name = "DimensionsGroupBox";
			this.DimensionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 209, true);
			this.DimensionsGroupBox.TabIndex = 4;
			this.DimensionsGroupBox.TabStop = false;
			//
			// DimensionsTabControl
			//
			this.DimensionsTabControl.Controls.Add(this.BaseDimensionsTabPage);
			this.DimensionsTabControl.Controls.Add(this.InsideDimensionsTabPage);
			this.DimensionsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 364, true);
			this.DimensionsTabControl.Name = "DimensionsTabControl";
			this.DimensionsTabControl.SelectedIndex = 0;
			this.DimensionsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 105, true);
			this.DimensionsTabControl.Dock = DockStyle.Top;
			this.DimensionsTabControl.TabIndex = 5;
			//
			// BaseDimensionsTabPage
			//
			this.BaseDimensionsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|DAA4AB90-5019-4D01-AAFA-5B80C3EA5D1B", "Base Dimensions");
			this.BaseDimensionsTabPage.Controls.Add(this.LengthCalcEdit);
			this.BaseDimensionsTabPage.Controls.Add(this.HeightCalcEdit);
			this.BaseDimensionsTabPage.Controls.Add(this.WidthCalcEdit);
			this.BaseDimensionsTabPage.Controls.Add(this.zCalcEdit3);
			this.BaseDimensionsTabPage.Controls.Add(this.zCalcEdit8);
			this.BaseDimensionsTabPage.Controls.Add(this.zCalcEdit9);
			this.BaseDimensionsTabPage.Controls.Add(this.zLabel12);
			this.BaseDimensionsTabPage.Controls.Add(this.zLabel13);
			this.BaseDimensionsTabPage.Controls.Add(this.zLabel18);
			this.BaseDimensionsTabPage.Controls.Add(this.zLabel19);
			this.BaseDimensionsTabPage.Controls.Add(this.zLabel20);
			this.BaseDimensionsTabPage.Controls.Add(this.zLabel21);
			this.BaseDimensionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 364, true);
			this.BaseDimensionsTabPage.Name = "BaseDimensionsTabPage";
			this.BaseDimensionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 90, true);
			this.BaseDimensionsTabPage.TabIndex = 6;
			//
			// InsideDimensionsTabPage
			//
			this.InsideDimensionsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|E39EAC19-961B-4A4E-8990-2A25F4E4DC40", "Inside Dimensions");
			this.InsideDimensionsTabPage.Controls.Add(this.InsideLengthCalcEdit);
			this.InsideDimensionsTabPage.Controls.Add(this.InsideHeightCalcEdit);
			this.InsideDimensionsTabPage.Controls.Add(this.InsideWidthCalcEdit);
			this.InsideDimensionsTabPage.Controls.Add(this.zCalcEdit12);
			this.InsideDimensionsTabPage.Controls.Add(this.zCalcEdit13);
			this.InsideDimensionsTabPage.Controls.Add(this.zCalcEdit14);
			this.InsideDimensionsTabPage.Controls.Add(this.zLabel33);
			this.InsideDimensionsTabPage.Controls.Add(this.zLabel34);
			this.InsideDimensionsTabPage.Controls.Add(this.zLabel35);
			this.InsideDimensionsTabPage.Controls.Add(this.zLabel36);
			this.InsideDimensionsTabPage.Controls.Add(this.zLabel37);
			this.InsideDimensionsTabPage.Controls.Add(this.zLabel38);
			this.InsideDimensionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 364, true);
			this.InsideDimensionsTabPage.Name = "InsideDimensionsTabPage";
			this.InsideDimensionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 90, true);
			this.InsideDimensionsTabPage.TabIndex = 7;
			//
			// InsideLengthCalcEdit
			//
			this.BindingSource.SetBindingMember(this.InsideLengthCalcEdit, "InsideLengthInches");
			this.InsideLengthCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|0d22eb75-49d8-4de6-9c55-adb9e3942d03", "Container Length");
			this.InsideLengthCalcEdit.DecimalPlaces = 3;
			this.InsideLengthCalcEdit.Decimals = 3;
			this.InsideLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 8, true);
			this.InsideLengthCalcEdit.Name = "LengthCalcEdit";
			this.InsideLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.InsideLengthCalcEdit.TabIndex = 0;
			this.InsideLengthCalcEdit.Text = "0.000";
			this.InsideLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// InsideWidthCalcEdit
			//
			this.BindingSource.SetBindingMember(this.InsideWidthCalcEdit, "InsideWidthInches");
			this.InsideWidthCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|0e16e1b6-50df-4589-9d43-0c694f3e7cb0", "Container Width");
			this.InsideWidthCalcEdit.DecimalPlaces = 3;
			this.InsideWidthCalcEdit.Decimals = 3;
			this.InsideWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 32, true);
			this.InsideWidthCalcEdit.Name = "WidthCalcEdit";
			this.InsideWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.InsideWidthCalcEdit.TabIndex = 7;
			this.InsideWidthCalcEdit.Text = "0.000";
			this.InsideWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// InsideHeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.InsideHeightCalcEdit, "InsideHeightInches");
			this.InsideHeightCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|115391a6-aa1d-4aa7-9dea-cd09432aeaec", "Container Height");
			this.InsideHeightCalcEdit.DecimalPlaces = 3;
			this.InsideHeightCalcEdit.Decimals = 3;
			this.InsideHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 56, true);
			this.InsideHeightCalcEdit.Name = "HeightCalcEdit";
			this.InsideHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.InsideHeightCalcEdit.TabIndex = 14;
			this.InsideHeightCalcEdit.Text = "0.000";
			this.InsideHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit12
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit12, "InsideLengthCentimetres");
			this.zCalcEdit12.DecimalPlaces = 3;
			this.zCalcEdit12.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit12, false);
			this.zCalcEdit12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 8, true);
			this.zCalcEdit12.Name = "zCalcEdit12";
			this.zCalcEdit12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit12.TabIndex = 2;
			this.zCalcEdit12.Text = "0.000";
			this.zCalcEdit12.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit13
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit13, "InsideWidthCentimetres");
			this.zCalcEdit13.DecimalPlaces = 3;
			this.zCalcEdit13.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit13, false);
			this.zCalcEdit13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 32, true);
			this.zCalcEdit13.Name = "zCalcEdit12";
			this.zCalcEdit13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit13.TabIndex = 2;
			this.zCalcEdit13.Text = "0.000";
			this.zCalcEdit13.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit14
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit14, "InsideHeightCentimetres");
			this.zCalcEdit14.DecimalPlaces = 3;
			this.zCalcEdit14.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit14, false);
			this.zCalcEdit14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 56, true);
			this.zCalcEdit14.Name = "zCalcEdit12";
			this.zCalcEdit14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit14.TabIndex = 2;
			this.zCalcEdit14.Text = "0.000";
			this.zCalcEdit14.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel33
			//
			this.zLabel33.AutoSize = true;
			this.zLabel33.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|e6195c08-952e-4241-ac30-41daa2e28c0d", "in");
			this.zLabel33.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 12, true);
			this.zLabel33.Name = "zLabel33";
			this.zLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel33.TabIndex = 29;
			//
			// zLabel34
			//
			this.zLabel34.AutoSize = true;
			this.zLabel34.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|41b5773e-7718-40d5-a3c3-9f74923f45dd", "in");
			this.zLabel34.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 36, true);
			this.zLabel34.Name = "zLabel34";
			this.zLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel34.TabIndex = 29;
			//
			// zLabel35
			//
			this.zLabel35.AutoSize = true;
			this.zLabel35.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|ad660785-cf00-43f2-b956-c863ebc17868", "in");
			this.zLabel35.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel35.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 60, true);
			this.zLabel35.Name = "zLabel35";
			this.zLabel35.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel35.TabIndex = 29;
			//
			// zLabel36
			//
			this.zLabel36.AutoSize = true;
			this.zLabel36.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|4bda75c8-cd48-406c-b01e-c6cb3a43a7e6", "cm");
			this.zLabel36.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel36.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 10, true);
			this.zLabel36.Name = "zLabel36";
			this.zLabel36.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel36.TabIndex = 29;
			//
			// zLabel37
			//
			this.zLabel37.AutoSize = true;
			this.zLabel37.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|46232abb-37e9-473d-9aa0-5e685fb6f75b", "cm");
			this.zLabel37.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel37.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 34, true);
			this.zLabel37.Name = "zLabel37";
			this.zLabel37.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel37.TabIndex = 29;
			//
			// zLabel38
			//
			this.zLabel38.AutoSize = true;
			this.zLabel38.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|90776458-e468-40a9-b43d-14277a671116", "cm");
			this.zLabel38.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel38.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 58, true);
			this.zLabel38.Name = "zLabel38";
			this.zLabel38.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel38.TabIndex = 29;
			//
			// TEUCalcEdit
			//
			this.BindingSource.SetBindingMember(this.TEUCalcEdit, "RC_TEU");
			this.TEUCalcEdit.DecimalPlaces = 2;
			this.TEUCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 122, true);
			this.TEUCalcEdit.Name = "TEUCalcEdit";
			this.TEUCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TEUCalcEdit.TabIndex = 21;
			this.TEUCalcEdit.Text = "0";
			this.TEUCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel21
			//
			this.zLabel21.AutoSize = true;
			this.zLabel21.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|853ffd42-4b69-450c-94b8-e5e5d1abdbf4", "in");
			this.zLabel21.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 60, true);
			this.zLabel21.Name = "zLabel21";
			this.zLabel21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.zLabel21.TabIndex = 15;
			//
			// zLabel20
			//
			this.zLabel20.AutoSize = true;
			this.zLabel20.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|19e379b5-fad8-4d41-865b-9c10d8cd4dfc", "in");
			this.zLabel20.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 36, true);
			this.zLabel20.Name = "zLabel20";
			this.zLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.zLabel20.TabIndex = 8;
			//
			// zLabel19
			//
			this.zLabel19.AutoSize = true;
			this.zLabel19.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|5f2d9da8-9b93-46ba-b41e-725623db8e96", "cm");
			this.zLabel19.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 58, true);
			this.zLabel19.Name = "zLabel19";
			this.zLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel19.TabIndex = 17;
			//
			// zCalcEdit9
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit9, "HeightCentimetres");
			this.zCalcEdit9.DecimalPlaces = 3;
			this.zCalcEdit9.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit9, false);
			this.zCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 56, true);
			this.zCalcEdit9.Name = "zCalcEdit9";
			this.zCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit9.TabIndex = 16;
			this.zCalcEdit9.Text = "0.000";
			this.zCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel18
			//
			this.zLabel18.AutoSize = true;
			this.zLabel18.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|0b53cd2e-3ed0-4a2b-b87d-08125fda14cb", "cm");
			this.zLabel18.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 34, true);
			this.zLabel18.Name = "zLabel18";
			this.zLabel18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel18.TabIndex = 10;
			//
			// zCalcEdit8
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit8, "WidthCentimetres");
			this.zCalcEdit8.DecimalPlaces = 3;
			this.zCalcEdit8.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit8, false);
			this.zCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 32, true);
			this.zCalcEdit8.Name = "zCalcEdit8";
			this.zCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit8.TabIndex = 9;
			this.zCalcEdit8.Text = "0.000";
			this.zCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel13
			//
			this.zLabel13.AutoSize = true;
			this.zLabel13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|c476d1ab-dcbc-4234-bade-4a1a36cef50c", "cm");
			this.zLabel13.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 10, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel13.TabIndex = 3;
			//
			// zLabel12
			//
			this.zLabel12.AutoSize = true;
			this.zLabel12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|4ccfe51b-3e31-4217-970b-b4881f95bfeb", "in");
			this.zLabel12.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 12, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.zLabel12.TabIndex = 1;
			//
			// zCalcEdit3
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "LengthCentimetres");
			this.zCalcEdit3.DecimalPlaces = 3;
			this.zCalcEdit3.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit3, false);
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 8, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit3.TabIndex = 2;
			this.zCalcEdit3.Text = "0.000";
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// CapacityCalcEdit
			//
			this.BindingSource.SetBindingMember(this.CapacityCalcEdit, "CubicCapacityFeet");
			this.CapacityCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|2077e9b3-f704-41b4-88de-c3c73a30196a", "Capacity");
			this.CapacityCalcEdit.DecimalPlaces = 3;
			this.CapacityCalcEdit.Decimals = 3;
			this.CapacityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 170, true);
			this.CapacityCalcEdit.Name = "CapacityCalcEdit";
			this.CapacityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.CapacityCalcEdit.TabIndex = 34;
			this.CapacityCalcEdit.Text = "0.000";
			this.CapacityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// NetWeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.NetWeightCalcEdit, "NetWeightPounds");
			this.NetWeightCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|373267cf-c203-4e7d-9d85-b8b1daab2dd6", "Max Net Wgt.", "Max Net Weight");
			this.NetWeightCalcEdit.DecimalPlaces = 3;
			this.NetWeightCalcEdit.Decimals = 3;
			this.NetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 146, true);
			this.NetWeightCalcEdit.Name = "NetWeightCalcEdit";
			this.NetWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.NetWeightCalcEdit.TabIndex = 30;
			this.NetWeightCalcEdit.Text = "0.000";
			this.NetWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// DetailsGroupBox
			//
			this.DetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|3ec15f03-26db-4a9f-8de3-7eab5f4e309d", "Container Type Details");
			this.DetailsGroupBox.Controls.Add(this.IATARateClassDropEdit);
			this.DetailsGroupBox.Controls.Add(this.ShippingModeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.RC_ContainerTypeBoundDropEdit);
			this.DetailsGroupBox.Controls.Add(this.RC_HasVentsCheckBox);
			this.DetailsGroupBox.Controls.Add(this.IsHighCubeCheckBox);
			this.DetailsGroupBox.Controls.Add(this.IsActiveCheckBox);
			this.DetailsGroupBox.Controls.Add(this.CodeTextBox);
			this.DetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.DetailsGroupBox.Controls.Add(this.HasTynesCheckEdit);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 100, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			//
			// IATARateClassDropEdit
			//
			this.IATARateClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IATARateClassDropEdit, "RC_IATARateClass");
			this.IATARateClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 70, true);
			this.IATARateClassDropEdit.Name = "IATARateClassDropEdit";
			this.IATARateClassDropEdit.PreBoundMaxLength = 3;
			this.IATARateClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.IATARateClassDropEdit.TabIndex = 13;
			//
			// ShippingModeDropEdit
			//
			this.ShippingModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingModeDropEdit, "RC_ShippingMode");
			this.ShippingModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 22, true);
			this.ShippingModeDropEdit.Name = "ShippingModeDropEdit";
			this.ShippingModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			this.ShippingModeDropEdit.TabIndex = 4;
			//
			// RC_ContainerTypeBoundDropEdit
			//
			this.RC_ContainerTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RC_ContainerTypeBoundDropEdit, "RC_ContainerType");
			this.RC_ContainerTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 46, true);
			this.RC_ContainerTypeBoundDropEdit.Name = "RC_ContainerTypeBoundDropEdit";
			this.RC_ContainerTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			this.RC_ContainerTypeBoundDropEdit.TabIndex = 8;
			//
			// RC_HasVentsCheckBox
			//
			this.RC_HasVentsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RC_HasVentsCheckBox, "RC_HasVents");
			this.RC_HasVentsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RC_HasVentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RC_HasVentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 75, true);
			this.RC_HasVentsCheckBox.Name = "RC_HasVentsCheckBox";
			this.RC_HasVentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.RC_HasVentsCheckBox.TabIndex = 11;
			//
			// IsHighCubeCheckBox
			//
			this.IsHighCubeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsHighCubeCheckBox, "RC_IsHighCube");
			this.IsHighCubeCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsHighCubeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsHighCubeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 75, true);
			this.IsHighCubeCheckBox.Name = "IsHighCubeCheckBox";
			this.IsHighCubeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsHighCubeCheckBox.TabIndex = 10;
			//
			// IsActiveCheckBox
			//
			this.IsActiveCheckBox.AutoSize = true;
			this.IsActiveCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "RC_IsActive");
			this.IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 27, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsActiveCheckBox.TabIndex = 2;
			this.IsActiveCheckBox.UseVisualStyleBackColor = false;
			//
			// ISOEquipmentSizeTypeDropEdit
			//
			this.ISOEquipmentSizeTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ISOEquipmentSizeTypeDropEdit, "RC_ISOEquipmentSizeTypeCode");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ISOEquipmentSizeTypeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ISOEquipmentSizeTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 184, true);
			this.ISOEquipmentSizeTypeDropEdit.Name = "ISOEquipmentSizeTypeDropEdit";
			this.ISOEquipmentSizeTypeDropEdit.PreBoundMaxLength = 4;
			this.ISOEquipmentSizeTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ISOEquipmentSizeTypeDropEdit.TabIndex = 7;
			//
			// HandlingRatingClassDropEdit
			//
			this.HandlingRatingClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HandlingRatingClassDropEdit, "RC_HandlingRateClass");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.HandlingRatingClassDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.HandlingRatingClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 134, true);
			this.HandlingRatingClassDropEdit.Name = "HandlingRatingClassDropEdit";
			this.HandlingRatingClassDropEdit.PreBoundMaxLength = 4;
			this.HandlingRatingClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.HandlingRatingClassDropEdit.TabIndex = 5;
			//
			// FreightRatingClassDropEdit
			//
			this.FreightRatingClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FreightRatingClassDropEdit, "RC_FreightRateClass");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FreightRatingClassDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.FreightRatingClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 84, true);
			this.FreightRatingClassDropEdit.Name = "FreightRatingClassDropEdit";
			this.FreightRatingClassDropEdit.PreBoundMaxLength = 4;
			this.FreightRatingClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.FreightRatingClassDropEdit.TabIndex = 3;
			//
			// StorageClassDropEdit
			//
			this.StorageClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StorageClassDropEdit, "RC_StorageClass");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.StorageClassDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.StorageClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 35, true);
			this.StorageClassDropEdit.Name = "StorageClassDropEdit";
			this.StorageClassDropEdit.PreBoundMaxLength = 3;
			this.StorageClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.StorageClassDropEdit.TabIndex = 1;
			//
			// ISOTypeGroupBox
			//
			this.ISOTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ISOTypeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|066648c9-f7f1-408b-b9ef-e5026c5452d3", "ISO Container Details");
			this.ISOTypeGroupBox.Controls.Add(this.OversizeCheckBox);
			this.ISOTypeGroupBox.Controls.Add(this.IsISOCheckEdit);
			this.ISOTypeGroupBox.Controls.Add(this.ISODescrioptionTextBox);
			this.ISOTypeGroupBox.Controls.Add(this.ISOSizeTextBox);
			this.ISOTypeGroupBox.Controls.Add(this.ISOTypeFindBox);
			this.ISOTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 238, true);
			this.ISOTypeGroupBox.Name = "ISOTypeGroupBox";
			this.ISOTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 120, true);
			this.ISOTypeGroupBox.TabIndex = 3;
			this.ISOTypeGroupBox.TabStop = false;
			//
			// OversizeCheckBox
			//
			this.OversizeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OversizeCheckBox, "ISOType+OverDimensionAllowed");
			this.OversizeCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|4a1eebe0-38da-498c-96cf-2a3db331e1f2", "Is Commonly Oversize");
			this.OversizeCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OversizeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OversizeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(403, 27, true);
			this.OversizeCheckBox.Name = "OversizeCheckBox";
			this.OversizeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.OversizeCheckBox.TabIndex = 5;
			//
			// IsISOCheckEdit
			//
			this.IsISOCheckEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.IsISOCheckEdit.AutoSize = true;
			this.IsISOCheckEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.IsISOCheckEdit, "RC_IsIso");
			this.IsISOCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsISOCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsISOCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(577, 0, true);
			this.IsISOCheckEdit.Name = "IsISOCheckEdit";
			this.IsISOCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsISOCheckEdit.TabIndex = 1;
			this.IsISOCheckEdit.UseVisualStyleBackColor = false;
			//
			// ISODescrioptionTextBox
			//
			this.ISODescrioptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ISODescrioptionTextBox, "ISOType+Description");
			this.ISODescrioptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|3bc9c441-1a85-41fd-bd59-ab25df95c22f", "Description", "Closest ISO type for this container.");
			this.ISODescrioptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ISODescrioptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 72, true);
			this.ISODescrioptionTextBox.Multiline = true;
			this.ISODescrioptionTextBox.Name = "ISODescrioptionTextBox";
			this.ISODescrioptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 40, true);
			this.ISODescrioptionTextBox.TabIndex = 0;
			//
			// ISOSizeTextBox
			//
			this.ISOSizeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ISOSizeTextBox, "ISOType+Dimensions");
			this.ISOSizeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|343f3984-9bf4-4617-9fbe-81ce72dc3022", "Size");
			this.ISOSizeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ISOSizeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 48, true);
			this.ISOSizeTextBox.Name = "ISOSizeTextBox";
			this.ISOSizeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(618, 20, true);
			this.ISOSizeTextBox.TabIndex = 7;
			//
			// ISOTypeFindBox
			//
			this.ISOTypeFindBox.AllowDrop = true;
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefContainer)(null)).RC_ISOType)));
			this.BindingSource.SetBindingMember(this.ISOTypeFindBox, "RC_ISOType");
			this.ISOTypeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|E5A98989-E3A7-4E6F-8434-3C6B081F235A", "ISO Type");
			this.ISOTypeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 24, true);
			this.ISOTypeFindBox.Name = "ISOTypeFindBox";
			this.ISOTypeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ISOTypeFindBox.TabIndex = 3;
			this.ISOTypeFindBox.ShowDescriptionBox = false;
			this.ISOTypeFindBox.TabStop = false;
			//
			// zLabel29
			//
			this.zLabel29.AutoSize = true;
			this.zLabel29.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|2caa5bcc-5677-4ea5-9435-6b6a6d384097", "lb");
			this.zLabel29.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 150, true);
			this.zLabel29.Name = "zLabel29";
			this.zLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel29.TabIndex = 29;
			//
			// zLabel30
			//
			this.zLabel30.AutoSize = true;
			this.zLabel30.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|e07fa31a-06eb-480f-9f8a-3044700509bf", "cf");
			this.zLabel30.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 174, true);
			this.zLabel30.Name = "zLabel30";
			this.zLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel30.TabIndex = 30;
			//
			// zLabel31
			//
			this.zLabel31.AutoSize = true;
			this.zLabel31.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|fbb89fb3-af5f-40ff-a1c3-b03bde4ba62a", "kg");
			this.zLabel31.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 150, true);
			this.zLabel31.Name = "zLabel31";
			this.zLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel31.TabIndex = 31;
			//
			// zLabel32
			//
			this.zLabel32.AutoSize = true;
			this.zLabel32.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|129c6f75-cd77-42be-98ea-a9cf4804e8b3", "M3", "Meters Squared.");
			this.zLabel32.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 174, true);
			this.zLabel32.Name = "zLabel32";
			this.zLabel32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.zLabel32.TabIndex = 32;
			//
			// zCalcEdit10
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit10, "RC_CubicCapacity");
			this.zCalcEdit10.DecimalPlaces = 3;
			this.zCalcEdit10.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit10, false);
			this.zCalcEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 170, true);
			this.zCalcEdit10.Name = "zCalcEdit10";
			this.zCalcEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit10.TabIndex = 36;
			this.zCalcEdit10.Text = "0.000";
			this.zCalcEdit10.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit11
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit11, "RC_NetWeight");
			this.zCalcEdit11.DecimalPlaces = 3;
			this.zCalcEdit11.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit11, false);
			this.zCalcEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 146, true);
			this.zCalcEdit11.Name = "zCalcEdit11";
			this.zCalcEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit11.TabIndex = 32;
			this.zCalcEdit11.Text = "0.000";
			this.zCalcEdit11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ContainerClasssGroupBox
			//
			this.ContainerClasssGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ContainerClasssGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|db2d0adc-d286-400e-b95f-79d35e88f179", "Container Class");
			this.ContainerClasssGroupBox.Controls.Add(this.ISOEquipmentSizeTypeDropEdit);
			this.ContainerClasssGroupBox.Controls.Add(this.HandlingRatingClassDropEdit);
			this.ContainerClasssGroupBox.Controls.Add(this.FreightRatingClassDropEdit);
			this.ContainerClasssGroupBox.Controls.Add(this.StorageClassDropEdit);
			this.ContainerClasssGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 364, true);
			this.ContainerClasssGroupBox.Name = "ContainerClasssGroupBox";
			this.ContainerClasssGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 209, true);
			this.ContainerClasssGroupBox.TabIndex = 5;
			this.ContainerClasssGroupBox.TabStop = false;
			//
			// CodeMapsGroupBox
			//
			this.CodeMapsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.CodeMapsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4bb7fa4e-2a10-4c40-962d-c3f511cf7435", "Customs Container Codes");
			this.CodeMapsGroupBox.Controls.Add(this.CodeMapsGrid);
			this.CodeMapsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 114, true);
			this.CodeMapsGroupBox.Name = "CodeMapsGroupBox";
			this.CodeMapsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 118, true);
			this.CodeMapsGroupBox.TabIndex = 2;
			this.CodeMapsGroupBox.TabStop = false;
			//
			// CodeMapsGrid
			//
			this.CodeMapsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CodeMapsGrid, "CodeMapCollection");
			this.CodeMapsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8133142f-2c69-44a2-be9e-c4c878d203fc", "Country/Region Code");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "RCM_RN_NKCountry";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5f93d03d-204d-42e8-ba98-399c41b13dd4", "Usage");
			zDropEditColumnStyleInfo1.ColumnName = "RCM_Usage";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f717819f-f3c5-413e-9dd8-cd9b3d6f1856", "Code");
			zDropEditColumnStyleInfo2.ColumnName = "RCM_Code";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2879aecc-3a84-4c73-85a4-e623a39a18e7", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "RCM_Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.CodeMapsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CodeMapsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CodeMapsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CodeMapsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CodeMapsGrid.GridId = "7078772c-220a-4b8c-8b97-bfa757559626";
			this.CodeMapsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeMapsGrid.LayoutKey = "CodeMapsGrid";
			this.CodeMapsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.CodeMapsGrid.Name = "CodeMapsGrid";
			this.CodeMapsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 93, true);
			this.CodeMapsGrid.TabIndex = 0;
			this.Controls.Add(this.CodeMapsGroupBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.Controls.Add(this.DimensionsGroupBox);
			this.Controls.Add(this.ISOTypeGroupBox);
			this.Controls.Add(this.ContainerClasssGroupBox);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AirRefContainerControl|49bc242c-a99b-4f9b-aba8-16165e315ca5", "Air Ref Container Control");
			this.CaptionRenderingEnabled = true;
			this.Name = "AirRefContainerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 576, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DescriptionTextBox.ResumeLayout(true);
			this.DescriptionTextBox.PerformLayout();
			this.DimensionsGroupBox.ResumeLayout(false);
			this.DimensionsGroupBox.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.IATARateClassDropEdit.ResumeLayout(true);
			this.IATARateClassDropEdit.PerformLayout();
			this.ShippingModeDropEdit.ResumeLayout(true);
			this.ShippingModeDropEdit.PerformLayout();
			this.RC_ContainerTypeBoundDropEdit.ResumeLayout(true);
			this.RC_ContainerTypeBoundDropEdit.PerformLayout();
			this.ISOEquipmentSizeTypeDropEdit.ResumeLayout(true);
			this.ISOEquipmentSizeTypeDropEdit.PerformLayout();
			this.HandlingRatingClassDropEdit.ResumeLayout(true);
			this.HandlingRatingClassDropEdit.PerformLayout();
			this.FreightRatingClassDropEdit.ResumeLayout(true);
			this.FreightRatingClassDropEdit.PerformLayout();
			this.StorageClassDropEdit.ResumeLayout(true);
			this.StorageClassDropEdit.PerformLayout();
			this.ISOTypeGroupBox.ResumeLayout(false);
			this.ISOTypeGroupBox.PerformLayout();
			this.ISOTypeFindBox.ResumeLayout(true);
			this.ISOTypeFindBox.PerformLayout();
			this.ContainerClasssGroupBox.ResumeLayout(false);
			this.ContainerClasssGroupBox.PerformLayout();
			this.CodeMapsGroupBox.ResumeLayout(false);
			this.CodeMapsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CodeMapsGrid)).EndInit();
			this.CodeMapsGrid.ResumeLayout(false);
			this.CodeMapsGrid.PerformLayout();
			this.DimensionsTabControl.ResumeLayout(false);
			this.DimensionsTabControl.PerformLayout();
			this.BaseDimensionsTabPage.ResumeLayout(false);
			this.BaseDimensionsTabPage.PerformLayout();
			this.InsideDimensionsTabPage.ResumeLayout(false);
			this.InsideDimensionsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZCheckBox HasTynesCheckEdit;
		Enterprise.ZArchitecture.ZTextBox CodeTextBox;
		Enterprise.ZArchitecture.ZTranslatableTextControl DescriptionTextBox;
		Enterprise.ZArchitecture.ZCalcEdit LengthCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit WidthCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit HeightCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit InsideLengthCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit InsideWidthCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit InsideHeightCalcEdit;
		ZGroupBox DimensionsGroupBox;
		ZGroupBox DetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		Enterprise.ZArchitecture.ZCalcEdit NetWeightCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit CapacityCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit3;
		Enterprise.ZArchitecture.ZLabel zLabel12;
		Enterprise.ZArchitecture.ZLabel zLabel13;
		Enterprise.ZArchitecture.ZLabel zLabel18;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit8;
		Enterprise.ZArchitecture.ZLabel zLabel19;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit9;
		Enterprise.ZArchitecture.ZLabel zLabel20;
		Enterprise.ZArchitecture.ZLabel zLabel21;
		Enterprise.ZArchitecture.ZCalcEdit TEUCalcEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsISOCheckEdit;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox ISOTypeFindBox;
		ZGroupBox ISOTypeGroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit ISOEquipmentSizeTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit HandlingRatingClassDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit FreightRatingClassDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit StorageClassDropEdit;
		Enterprise.ZArchitecture.ZTextBox ISOSizeTextBox;
		Enterprise.ZArchitecture.ZTextBox ISODescrioptionTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox OversizeCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsHighCubeCheckBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox RC_HasVentsCheckBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit RC_ContainerTypeBoundDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit ShippingModeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit IATARateClassDropEdit;
		ZLabel zLabel32;
		ZLabel zLabel31;
		ZLabel zLabel30;
		ZLabel zLabel29;
		ZLabel zLabel33;
		ZLabel zLabel34;
		ZLabel zLabel35;
		ZLabel zLabel36;
		ZLabel zLabel37;
		ZLabel zLabel38;
		ZCalcEdit zCalcEdit10;
		ZCalcEdit zCalcEdit11;
		ZCalcEdit zCalcEdit12;
		ZCalcEdit zCalcEdit13;
		ZCalcEdit zCalcEdit14;
		ZGroupBox ContainerClasssGroupBox;
		ZArchitecture.GUI.ZGroupBox CodeMapsGroupBox;
		ZArchitecture.ZGrid CodeMapsGrid;
		Enterprise.ZArchitecture.GUI.ZTabControl DimensionsTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage BaseDimensionsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage InsideDimensionsTabPage;
	}
}

