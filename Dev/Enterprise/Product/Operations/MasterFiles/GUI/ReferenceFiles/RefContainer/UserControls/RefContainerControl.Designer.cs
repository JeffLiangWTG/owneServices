using System.ComponentModel;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.UserControls
{
	partial class RefContainerControl
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
			this.TareWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HasTynesCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DimensionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TEUCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel21 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel20 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel19 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit9 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel18 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit8 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel16 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel17 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit6 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit7 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel14 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel15 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit4 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit5 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CapacityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NetWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
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
			this.zLabel31 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel32 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel29 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel30 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel33 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel34 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel35 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel36 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit10 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit11 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit12 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit13 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ContainerClasssGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CodeMapsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CodeMapsGrid = new Enterprise.ZArchitecture.ZGrid();
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
			this.BindingSource.SetBindingMember(this.LengthCalcEdit, "RC_Length");
			this.LengthCalcEdit.DecimalPlaces = 3;
			this.LengthCalcEdit.Decimals = 3;
			this.LengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 16, true);
			this.LengthCalcEdit.Name = "LengthCalcEdit";
			this.LengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.LengthCalcEdit.TabIndex = 0;
			this.LengthCalcEdit.Text = "0.000";
			this.LengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// WidthCalcEdit
			//
			this.BindingSource.SetBindingMember(this.WidthCalcEdit, "RC_Width");
			this.WidthCalcEdit.DecimalPlaces = 3;
			this.WidthCalcEdit.Decimals = 3;
			this.WidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 40, true);
			this.WidthCalcEdit.Name = "WidthCalcEdit";
			this.WidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.WidthCalcEdit.TabIndex = 7;
			this.WidthCalcEdit.Text = "0.000";
			this.WidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// HeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.HeightCalcEdit, "RC_Height");
			this.HeightCalcEdit.DecimalPlaces = 3;
			this.HeightCalcEdit.Decimals = 3;
			this.HeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 64, true);
			this.HeightCalcEdit.Name = "HeightCalcEdit";
			this.HeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.HeightCalcEdit.TabIndex = 14;
			this.HeightCalcEdit.Text = "0.000";
			this.HeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// TareWeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.TareWeightCalcEdit, "TareWeightPounds");
			this.TareWeightCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|f6e8794f-bc18-487f-9cfd-2a012d022ef4", "Tare Wgt.", "Tare Weight");
			this.TareWeightCalcEdit.DecimalPlaces = 3;
			this.TareWeightCalcEdit.Decimals = 3;
			this.TareWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 136, true);
			this.TareWeightCalcEdit.Name = "TareWeightCalcEdit";
			this.TareWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TareWeightCalcEdit.TabIndex = 26;
			this.TareWeightCalcEdit.Text = "0.000";
			this.TareWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
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
			this.DimensionsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|4a7be46f-793f-4f15-a6c4-eb3cd293133d", "Container Dimensions");
			this.DimensionsGroupBox.Controls.Add(this.zLabel33);
			this.DimensionsGroupBox.Controls.Add(this.zLabel34);
			this.DimensionsGroupBox.Controls.Add(this.zLabel35);
			this.DimensionsGroupBox.Controls.Add(this.zLabel36);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit10);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit11);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit12);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit13);
			this.DimensionsGroupBox.Controls.Add(this.zLabel30);
			this.DimensionsGroupBox.Controls.Add(this.zLabel29);
			this.DimensionsGroupBox.Controls.Add(this.zLabel32);
			this.DimensionsGroupBox.Controls.Add(this.zLabel31);
			this.DimensionsGroupBox.Controls.Add(this.TEUCalcEdit);
			this.DimensionsGroupBox.Controls.Add(this.zLabel21);
			this.DimensionsGroupBox.Controls.Add(this.zLabel20);
			this.DimensionsGroupBox.Controls.Add(this.zLabel19);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit9);
			this.DimensionsGroupBox.Controls.Add(this.zLabel18);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit8);
			this.DimensionsGroupBox.Controls.Add(this.zLabel16);
			this.DimensionsGroupBox.Controls.Add(this.zLabel17);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit6);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit7);
			this.DimensionsGroupBox.Controls.Add(this.zLabel14);
			this.DimensionsGroupBox.Controls.Add(this.zLabel15);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit4);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit5);
			this.DimensionsGroupBox.Controls.Add(this.zLabel13);
			this.DimensionsGroupBox.Controls.Add(this.zLabel12);
			this.DimensionsGroupBox.Controls.Add(this.zLabel11);
			this.DimensionsGroupBox.Controls.Add(this.zLabel10);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit3);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit2);
			this.DimensionsGroupBox.Controls.Add(this.zCalcEdit1);
			this.DimensionsGroupBox.Controls.Add(this.CapacityCalcEdit);
			this.DimensionsGroupBox.Controls.Add(this.NetWeightCalcEdit);
			this.DimensionsGroupBox.Controls.Add(this.GrossWeightCalcEdit);
			this.DimensionsGroupBox.Controls.Add(this.TareWeightCalcEdit);
			this.DimensionsGroupBox.Controls.Add(this.LengthCalcEdit);
			this.DimensionsGroupBox.Controls.Add(this.HeightCalcEdit);
			this.DimensionsGroupBox.Controls.Add(this.WidthCalcEdit);
			this.DimensionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 364, true);
			this.DimensionsGroupBox.Name = "DimensionsGroupBox";
			this.DimensionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 209, true);
			this.DimensionsGroupBox.TabIndex = 4;
			this.DimensionsGroupBox.TabStop = false;
			//
			// TEUCalcEdit
			//
			this.BindingSource.SetBindingMember(this.TEUCalcEdit, "RC_TEU");
			this.TEUCalcEdit.DecimalPlaces = 2;
			this.TEUCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 88, true);
			this.TEUCalcEdit.Name = "TEUCalcEdit";
			this.TEUCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.TEUCalcEdit.TabIndex = 21;
			this.TEUCalcEdit.Text = "0";
			this.TEUCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel21
			//
			this.zLabel21.AutoSize = true;
			this.zLabel21.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|933032dc-909d-4e3d-bf4a-43ec8f5b1553", "ft");
			this.zLabel21.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel21.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 68, true);
			this.zLabel21.Name = "zLabel21";
			this.zLabel21.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.zLabel21.TabIndex = 15;
			//
			// zLabel20
			//
			this.zLabel20.AutoSize = true;
			this.zLabel20.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|7ed15bb4-d1dd-4c5a-a999-add59afae4fc", "ft");
			this.zLabel20.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel20.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 44, true);
			this.zLabel20.Name = "zLabel20";
			this.zLabel20.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.zLabel20.TabIndex = 8;
			//
			// zLabel19
			//
			this.zLabel19.AutoSize = true;
			this.zLabel19.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|a426d212-8a45-4fe9-99de-f4050d534590", "m");
			this.zLabel19.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel19.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 66, true);
			this.zLabel19.Name = "zLabel19";
			this.zLabel19.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel19.TabIndex = 17;
			//
			// zCalcEdit9
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit9, "HeightMetres");
			this.zCalcEdit9.DecimalPlaces = 3;
			this.zCalcEdit9.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit9, false);
			this.zCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 64, true);
			this.zCalcEdit9.Name = "zCalcEdit9";
			this.zCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit9.TabIndex = 16;
			this.zCalcEdit9.Text = "0.000";
			this.zCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel18
			//
			this.zLabel18.AutoSize = true;
			this.zLabel18.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|eea52b95-5359-4024-9a01-972a17bb2d25", "m");
			this.zLabel18.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel18.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 42, true);
			this.zLabel18.Name = "zLabel18";
			this.zLabel18.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel18.TabIndex = 10;
			//
			// zCalcEdit8
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit8, "WidthMetres");
			this.zCalcEdit8.DecimalPlaces = 3;
			this.zCalcEdit8.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit8, false);
			this.zCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 40, true);
			this.zCalcEdit8.Name = "zCalcEdit8";
			this.zCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit8.TabIndex = 9;
			this.zCalcEdit8.Text = "0.000";
			this.zCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel16
			//
			this.zLabel16.AutoSize = true;
			this.zLabel16.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|076dafbc-3074-40a3-83cc-ac47b0f3f53a", "in");
			this.zLabel16.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 68, true);
			this.zLabel16.Name = "zLabel16";
			this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel16.TabIndex = 20;
			//
			// zLabel17
			//
			this.zLabel17.AutoSize = true;
			this.zLabel17.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|45fb5dc9-1f7a-40b2-b0e1-e01705518c55", "ft");
			this.zLabel17.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 68, true);
			this.zLabel17.Name = "zLabel17";
			this.zLabel17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.zLabel17.TabIndex = 19;
			//
			// zCalcEdit6
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit6, "HeightInchesOnly");
			this.zCalcEdit6.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit6, false);
			this.zCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 64, true);
			this.zCalcEdit6.Name = "zCalcEdit6";
			this.zCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zCalcEdit6.TabIndex = 21;
			this.zCalcEdit6.Text = "0";
			this.zCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit7
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit7, "HeightFeetOnly");
			this.zCalcEdit7.DecimalPlaces = 0;
			this.zCalcEdit7.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit7, false);
			this.zCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 64, true);
			this.zCalcEdit7.Name = "zCalcEdit7";
			this.zCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.zCalcEdit7.TabIndex = 18;
			this.zCalcEdit7.Text = "0";
			this.zCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel14
			//
			this.zLabel14.AutoSize = true;
			this.zLabel14.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|85eeadfd-3d06-462c-871d-d6ad3fb6d5d7", "in");
			this.zLabel14.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 44, true);
			this.zLabel14.Name = "zLabel14";
			this.zLabel14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel14.TabIndex = 13;
			//
			// zLabel15
			//
			this.zLabel15.AutoSize = true;
			this.zLabel15.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|8f2f7c26-3774-415b-9c21-2de58b074bf8", "ft");
			this.zLabel15.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 44, true);
			this.zLabel15.Name = "zLabel15";
			this.zLabel15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.zLabel15.TabIndex = 12;
			//
			// zCalcEdit4
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit4, "WidthInchesOnly");
			this.zCalcEdit4.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit4, false);
			this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 40, true);
			this.zCalcEdit4.Name = "zCalcEdit4";
			this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zCalcEdit4.TabIndex = 12;
			this.zCalcEdit4.Text = "0";
			this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit5
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit5, "WidthFeetOnly");
			this.zCalcEdit5.DecimalPlaces = 0;
			this.zCalcEdit5.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit5, false);
			this.zCalcEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 40, true);
			this.zCalcEdit5.Name = "zCalcEdit5";
			this.zCalcEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.zCalcEdit5.TabIndex = 11;
			this.zCalcEdit5.Text = "0";
			this.zCalcEdit5.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel13
			//
			this.zLabel13.AutoSize = true;
			this.zLabel13.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|cad0d325-13e3-414e-b906-20d70758bbde", "m");
			this.zLabel13.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 18, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel13.TabIndex = 3;
			//
			// zLabel12
			//
			this.zLabel12.AutoSize = true;
			this.zLabel12.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|2e5265c3-7a1a-4bf4-95c9-4ec65d6a8ee5", "ft");
			this.zLabel12.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 20, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.zLabel12.TabIndex = 1;
			//
			// zLabel11
			//
			this.zLabel11.AutoSize = true;
			this.zLabel11.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|920b36ee-0aba-4285-bf3d-8002c6bb87cc", "in");
			this.zLabel11.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(356, 20, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel11.TabIndex = 6;
			//
			// zLabel10
			//
			this.zLabel10.AutoSize = true;
			this.zLabel10.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|7c0c77a5-5dbc-4d29-907a-c72d3427f338", "ft");
			this.zLabel10.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 20, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.zLabel10.TabIndex = 5;
			//
			// zCalcEdit3
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "LengthMetres");
			this.zCalcEdit3.DecimalPlaces = 3;
			this.zCalcEdit3.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit3, false);
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 16, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit3.TabIndex = 2;
			this.zCalcEdit3.Text = "0.000";
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit2
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "LengthInchesOnly");
			this.zCalcEdit2.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit2, false);
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(316, 16, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.zCalcEdit2.TabIndex = 7;
			this.zCalcEdit2.Text = "0";
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit1
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "LengthFeetOnly");
			this.zCalcEdit1.DecimalPlaces = 0;
			this.zCalcEdit1.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit1, false);
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 16, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.zCalcEdit1.TabIndex = 4;
			this.zCalcEdit1.Text = "0";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// CapacityCalcEdit
			//
			this.BindingSource.SetBindingMember(this.CapacityCalcEdit, "CubicCapacityFeet");
			this.CapacityCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|2077e9b3-f704-41b4-88de-c3c73a30196a", "Capacity");
			this.CapacityCalcEdit.DecimalPlaces = 3;
			this.CapacityCalcEdit.Decimals = 3;
			this.CapacityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 184, true);
			this.CapacityCalcEdit.Name = "CapacityCalcEdit";
			this.CapacityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.CapacityCalcEdit.TabIndex = 34;
			this.CapacityCalcEdit.Text = "0.000";
			this.CapacityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// NetWeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.NetWeightCalcEdit, "NetWeightPounds");
			this.NetWeightCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|373267cf-c203-4e7d-9d85-b8b1daab2dd6", "Max Net Wgt.", "Max Net Weight");
			this.NetWeightCalcEdit.DecimalPlaces = 3;
			this.NetWeightCalcEdit.Decimals = 3;
			this.NetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 160, true);
			this.NetWeightCalcEdit.Name = "NetWeightCalcEdit";
			this.NetWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.NetWeightCalcEdit.TabIndex = 30;
			this.NetWeightCalcEdit.Text = "0.000";
			this.NetWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// GrossWeightCalcEdit
			//
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit, "GrossWeightPounds");
			this.GrossWeightCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|f9e7688b-06f9-42c0-a686-2a0967cd06c0", "Max Gross Wt.", "Max Gross Weight");
			this.GrossWeightCalcEdit.DecimalPlaces = 3;
			this.GrossWeightCalcEdit.Decimals = 3;
			this.GrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 112, true);
			this.GrossWeightCalcEdit.Name = "GrossWeightCalcEdit";
			this.GrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.GrossWeightCalcEdit.TabIndex = 22;
			this.GrossWeightCalcEdit.Text = "0.000";
			this.GrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// DetailsGroupBox
			//
			this.DetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|3ec15f03-26db-4a9f-8de3-7eab5f4e309d", "Container Type Details");
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
			this.ISOTypeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|066648c9-f7f1-408b-b9ef-e5026c5452d3", "ISO Container Details");
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
			this.OversizeCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|4a1eebe0-38da-498c-96cf-2a3db331e1f2", "Is Commonly Oversize");
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
			this.ISODescrioptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|3bc9c441-1a85-41fd-bd59-ab25df95c22f", "Description", "Closest ISO type for this container.");
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
			this.ISOSizeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|343f3984-9bf4-4617-9fbe-81ce72dc3022", "Size");
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
			this.ISOTypeFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|E5A98989-E3A7-4E6F-8434-3C6B081F235A", "ISO Type");
			this.ISOTypeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 24, true);
			this.ISOTypeFindBox.Name = "ISOTypeFindBox";
			this.ISOTypeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ISOTypeFindBox.TabIndex = 3;
			this.ISOTypeFindBox.ShowDescriptionBox = false;
			this.ISOTypeFindBox.TabStop = false;
			//
			// zLabel31
			//
			this.zLabel31.AutoSize = true;
			this.zLabel31.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|fbb89fb3-af5f-40ff-a1c3-b03bde4ba62a", "kg");
			this.zLabel31.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 116, true);
			this.zLabel31.Name = "zLabel31";
			this.zLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel31.TabIndex = 25;
			//
			// zLabel32
			//
			this.zLabel32.AutoSize = true;
			this.zLabel32.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|129c6f75-cd77-42be-98ea-a9cf4804e8b3", "M3", "Meters Squared.");
			this.zLabel32.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 187, true);
			this.zLabel32.Name = "zLabel32";
			this.zLabel32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 13, true);
			this.zLabel32.TabIndex = 37;
			//
			// zLabel29
			//
			this.zLabel29.AutoSize = true;
			this.zLabel29.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|2caa5bcc-5677-4ea5-9435-6b6a6d384097", "kg");
			this.zLabel29.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel29.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 140, true);
			this.zLabel29.Name = "zLabel29";
			this.zLabel29.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel29.TabIndex = 29;
			//
			// zLabel30
			//
			this.zLabel30.AutoSize = true;
			this.zLabel30.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|e07fa31a-06eb-480f-9f8a-3044700509bf", "kg");
			this.zLabel30.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel30.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 164, true);
			this.zLabel30.Name = "zLabel30";
			this.zLabel30.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(19, 13, true);
			this.zLabel30.TabIndex = 33;
			//
			// zLabel33
			//
			this.zLabel33.AutoSize = true;
			this.zLabel33.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|eb17f46b-5d06-41bd-ac47-32429199f00c", "lb");
			this.zLabel33.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel33.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 164, true);
			this.zLabel33.Name = "zLabel33";
			this.zLabel33.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel33.TabIndex = 31;
			//
			// zLabel34
			//
			this.zLabel34.AutoSize = true;
			this.zLabel34.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|15aab7c0-ffee-46e7-95f1-cc3daba721bd", "lb");
			this.zLabel34.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel34.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 140, true);
			this.zLabel34.Name = "zLabel34";
			this.zLabel34.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel34.TabIndex = 27;
			//
			// zLabel35
			//
			this.zLabel35.AutoSize = true;
			this.zLabel35.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|cfd22e51-b46b-4f87-bd31-8d39db3163d5", "cf");
			this.zLabel35.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel35.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 187, true);
			this.zLabel35.Name = "zLabel35";
			this.zLabel35.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 13, true);
			this.zLabel35.TabIndex = 35;
			//
			// zLabel36
			//
			this.zLabel36.AutoSize = true;
			this.zLabel36.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|d18b7f46-5e78-4a01-948f-8ca8d80e0fa1", "lb");
			this.zLabel36.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel36.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 116, true);
			this.zLabel36.Name = "zLabel36";
			this.zLabel36.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel36.TabIndex = 23;
			//
			// zCalcEdit10
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit10, "RC_CubicCapacity");
			this.zCalcEdit10.DecimalPlaces = 3;
			this.zCalcEdit10.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit10, false);
			this.zCalcEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 184, true);
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
			this.zCalcEdit11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 160, true);
			this.zCalcEdit11.Name = "zCalcEdit11";
			this.zCalcEdit11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit11.TabIndex = 32;
			this.zCalcEdit11.Text = "0.000";
			this.zCalcEdit11.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit12
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit12, "RC_GrossWeight");
			this.zCalcEdit12.DecimalPlaces = 3;
			this.zCalcEdit12.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit12, false);
			this.zCalcEdit12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 112, true);
			this.zCalcEdit12.Name = "zCalcEdit12";
			this.zCalcEdit12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit12.TabIndex = 24;
			this.zCalcEdit12.Text = "0.000";
			this.zCalcEdit12.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit13
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit13, "RC_TareWeight");
			this.zCalcEdit13.DecimalPlaces = 3;
			this.zCalcEdit13.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit13, false);
			this.zCalcEdit13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 136, true);
			this.zCalcEdit13.Name = "zCalcEdit13";
			this.zCalcEdit13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.zCalcEdit13.TabIndex = 28;
			this.zCalcEdit13.Text = "0.000";
			this.zCalcEdit13.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ContainerClasssGroupBox
			//
			this.ContainerClasssGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ContainerClasssGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|db2d0adc-d286-400e-b95f-79d35e88f179", "Container Class");
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
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefContainerControl|7f475e81-f33d-483a-b86b-1d3970c4015e", "Ref Container Control");
			this.CaptionRenderingEnabled = true;
			this.Name = "RefContainerControl";
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
		Enterprise.ZArchitecture.ZCalcEdit TareWeightCalcEdit;
		ZGroupBox DimensionsGroupBox;
		ZGroupBox DetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		Enterprise.ZArchitecture.ZCalcEdit GrossWeightCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit NetWeightCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit CapacityCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit1;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit2;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit3;
		Enterprise.ZArchitecture.ZLabel zLabel10;
		Enterprise.ZArchitecture.ZLabel zLabel11;
		Enterprise.ZArchitecture.ZLabel zLabel12;
		Enterprise.ZArchitecture.ZLabel zLabel13;
		Enterprise.ZArchitecture.ZLabel zLabel14;
		Enterprise.ZArchitecture.ZLabel zLabel15;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit4;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit5;
		Enterprise.ZArchitecture.ZLabel zLabel16;
		Enterprise.ZArchitecture.ZLabel zLabel17;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit6;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit7;
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
		ZCalcEdit zCalcEdit10;
		ZCalcEdit zCalcEdit11;
		ZCalcEdit zCalcEdit12;
		ZCalcEdit zCalcEdit13;
		ZGroupBox ContainerClasssGroupBox;
		ZArchitecture.GUI.ZGroupBox CodeMapsGroupBox;
		ZArchitecture.ZGrid CodeMapsGrid;
	}
}

