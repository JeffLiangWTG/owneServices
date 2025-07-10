namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class LooseBookedMoveSplitForm
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		public new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.totalsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RemainingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcDropEdit1 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCalcDropEdit2 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCalcDropEdit3 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SplitsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OriginalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalVolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TotalWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TotalPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JJ_VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JJ_WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JJ_OuterPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TheCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SplitsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SplitsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SplitsButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.RemoveSplitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DivideEquallyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddSplitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveCancelPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.totalsGroupBox.SuspendLayout();
			this.SplitsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitsGrid)).BeginInit();
			this.SplitsButtonPanel.SuspendLayout();
			this.SaveCancelPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 338, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster);
			// 
			// totalsGroupBox
			// 
			this.totalsGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|4c00b694-547d-4eed-b65e-2f3856b221b2", "Totals", "Totals", "");
			this.totalsGroupBox.Controls.Add(this.zCheckBox1);
			this.totalsGroupBox.Controls.Add(this.RemainingLabel);
			this.totalsGroupBox.Controls.Add(this.zCalcDropEdit1);
			this.totalsGroupBox.Controls.Add(this.zCalcDropEdit2);
			this.totalsGroupBox.Controls.Add(this.zCalcDropEdit3);
			this.totalsGroupBox.Controls.Add(this.SplitsLabel);
			this.totalsGroupBox.Controls.Add(this.OriginalLabel);
			this.totalsGroupBox.Controls.Add(this.TotalVolumeCalcDropEdit);
			this.totalsGroupBox.Controls.Add(this.TotalWeightCalcDropEdit);
			this.totalsGroupBox.Controls.Add(this.TotalPacksCalcDropEdit);
			this.totalsGroupBox.Controls.Add(this.JJ_VolumeCalcDropEdit);
			this.totalsGroupBox.Controls.Add(this.JJ_WeightCalcDropEdit);
			this.totalsGroupBox.Controls.Add(this.JJ_OuterPacksCalcDropEdit);
			this.totalsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.totalsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.totalsGroupBox.Name = "totalsGroupBox";
			this.totalsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 103, true);
			this.totalsGroupBox.TabIndex = 0;
			this.totalsGroupBox.TabStop = false;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "AllowDiscrepancy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).AllowDiscrepancy)));
			this.zCheckBox1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|98965d3c-0f37-41e7-a319-2488df0bd298", "Allow Discrepancy");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(473, 80, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
			this.zCheckBox1.TabIndex = 12;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// RemainingLabel
			// 
			this.RemainingLabel.AutoSize = true;
			this.RemainingLabel.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|fd1422af-65aa-4e69-b86b-b030f43c1c65", "Remaining", "Remaining", "");
			this.RemainingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 81, true);
			this.RemainingLabel.Name = "RemainingLabel";
			this.RemainingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 13, true);
			this.RemainingLabel.TabIndex = 11;
			// 
			// zCalcDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).RemainingVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).VolumeUnit)));
			this.zCalcDropEdit1.BindToAmount = "RemainingVolume";
			this.zCalcDropEdit1.BindToUnit = "VolumeUnit";
			this.zCalcDropEdit1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|f8b9e955-85ce-41d7-b89a-5e098b902542", "Volume", "Volume", "");
			this.zCalcDropEdit1.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCalcDropEdit1, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zCalcDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 78, true);
			this.zCalcDropEdit1.Name = "zCalcDropEdit1";
			this.zCalcDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.zCalcDropEdit1.TabIndex = 10;
			this.zCalcDropEdit1.UnitPreBoundMaxLength = 2;
			// 
			// zCalcDropEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).RemainingWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).WeightUnit)));
			this.zCalcDropEdit2.BindToAmount = "RemainingWeight";
			this.zCalcDropEdit2.BindToUnit = "WeightUnit";
			this.zCalcDropEdit2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|00dfe268-8959-4d45-93cd-53a78d140a10", "Weight", "Weight", "");
			this.zCalcDropEdit2.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCalcDropEdit2, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zCalcDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 78, true);
			this.zCalcDropEdit2.Name = "zCalcDropEdit2";
			this.zCalcDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.zCalcDropEdit2.TabIndex = 9;
			this.zCalcDropEdit2.UnitPreBoundMaxLength = 2;
			// 
			// zCalcDropEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit3, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).RemainingPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).PackType)));
			this.zCalcDropEdit3.BindToAmount = "RemainingPacks";
			this.zCalcDropEdit3.BindToUnit = "PackType";
			this.zCalcDropEdit3.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|d523df1d-d6ba-431c-81ac-6d60fddd998f", "Packages", "Packages", "");
			this.zCalcDropEdit3.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCalcDropEdit3, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zCalcDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 78, true);
			this.zCalcDropEdit3.Name = "zCalcDropEdit3";
			this.zCalcDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.zCalcDropEdit3.TabIndex = 8;
			this.zCalcDropEdit3.UnitPreBoundMaxLength = 3;
			// 
			// SplitsLabel
			// 
			this.SplitsLabel.AutoSize = true;
			this.SplitsLabel.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|9570423e-159f-48e1-bab8-d076e9753c94", "Splits:", "Splits:", "Splits.");
			this.SplitsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 57, true);
			this.SplitsLabel.Name = "SplitsLabel";
			this.SplitsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 13, true);
			this.SplitsLabel.TabIndex = 7;
			// 
			// OriginalLabel
			// 
			this.OriginalLabel.AutoSize = true;
			this.OriginalLabel.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|c4ae221d-f750-4020-9cf2-7ca63f10544c", "Original:", "Original:", "Original.");
			this.OriginalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 34, true);
			this.OriginalLabel.Name = "OriginalLabel";
			this.OriginalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.OriginalLabel.TabIndex = 6;
			// 
			// TotalVolumeCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).TotalVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).VolumeUnit)));
			this.TotalVolumeCalcDropEdit.BindToAmount = "TotalVolume";
			this.TotalVolumeCalcDropEdit.BindToUnit = "VolumeUnit";
			this.TotalVolumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|4b5c26e8-050d-435e-a2f2-122c8e3e5e8b", "Volume", "Volume", "");
			this.TotalVolumeCalcDropEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.TotalVolumeCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.TotalVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 54, true);
			this.TotalVolumeCalcDropEdit.Name = "TotalVolumeCalcDropEdit";
			this.TotalVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.TotalVolumeCalcDropEdit.TabIndex = 5;
			this.TotalVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// TotalWeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).TotalWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).WeightUnit)));
			this.TotalWeightCalcDropEdit.BindToAmount = "TotalWeight";
			this.TotalWeightCalcDropEdit.BindToUnit = "WeightUnit";
			this.TotalWeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|f41c843b-a1e9-4478-bcf5-db61b830290d", "Weight", "Weight", "");
			this.TotalWeightCalcDropEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.TotalWeightCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.TotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 54, true);
			this.TotalWeightCalcDropEdit.Name = "TotalWeightCalcDropEdit";
			this.TotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.TotalWeightCalcDropEdit.TabIndex = 4;
			this.TotalWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// TotalPacksCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).TotalPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).PackType)));
			this.TotalPacksCalcDropEdit.BindToAmount = "TotalPacks";
			this.TotalPacksCalcDropEdit.BindToUnit = "PackType";
			this.TotalPacksCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|0272cd83-0c59-4baf-9839-57bdf5e8f128", "Packages", "Packages", "");
			this.TotalPacksCalcDropEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.TotalPacksCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.TotalPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 54, true);
			this.TotalPacksCalcDropEdit.Name = "TotalPacksCalcDropEdit";
			this.TotalPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.TotalPacksCalcDropEdit.TabIndex = 3;
			this.TotalPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// JJ_VolumeCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JJ_VolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).VolumeUnit)));
			this.JJ_VolumeCalcDropEdit.BindToAmount = "Volume";
			this.JJ_VolumeCalcDropEdit.BindToUnit = "VolumeUnit";
			this.JJ_VolumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|0f2e6f93-c6f3-4e2d-98f6-ecc472325cef", "Volume", "Volume", "");
			this.JJ_VolumeCalcDropEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JJ_VolumeCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JJ_VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 31, true);
			this.JJ_VolumeCalcDropEdit.Name = "JJ_VolumeCalcDropEdit";
			this.JJ_VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.JJ_VolumeCalcDropEdit.TabIndex = 2;
			this.JJ_VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JJ_WeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JJ_WeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).WeightUnit)));
			this.JJ_WeightCalcDropEdit.BindToAmount = "Weight";
			this.JJ_WeightCalcDropEdit.BindToUnit = "WeightUnit";
			this.JJ_WeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|09a4a4b2-6b1f-47dc-93f0-eae84c5ce187", "Weight", "Weight", "");
			this.JJ_WeightCalcDropEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JJ_WeightCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JJ_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 31, true);
			this.JJ_WeightCalcDropEdit.Name = "JJ_WeightCalcDropEdit";
			this.JJ_WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.JJ_WeightCalcDropEdit.TabIndex = 1;
			this.JJ_WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JJ_OuterPacksCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JJ_OuterPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Packs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).PackType)));
			this.JJ_OuterPacksCalcDropEdit.BindToAmount = "Packs";
			this.JJ_OuterPacksCalcDropEdit.BindToUnit = "PackType";
			this.JJ_OuterPacksCalcDropEdit.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|486c19ea-8b23-47c8-bca3-e63a45607e30", "Packages", "Packages", "");
			this.JJ_OuterPacksCalcDropEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.JJ_OuterPacksCalcDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.JJ_OuterPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 31, true);
			this.JJ_OuterPacksCalcDropEdit.Name = "JJ_OuterPacksCalcDropEdit";
			this.JJ_OuterPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.JJ_OuterPacksCalcDropEdit.TabIndex = 0;
			this.JJ_OuterPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|3d774382-8382-4baf-8e21-17f5d1e23250", "OK", "OK", "");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(364, 3, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.SaveButton.TabIndex = 0;
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// TheCancelButton
			// 
			this.TheCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TheCancelButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|3caa8396-d582-46a3-a25b-af730191d0ef", "Cancel", "Cancel", "");
			this.TheCancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.TheCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 3, true);
			this.TheCancelButton.Name = "TheCancelButton";
			this.TheCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.TheCancelButton.TabIndex = 1;
			this.TheCancelButton.UseVisualStyleBackColor = true;
			this.TheCancelButton.Click += new System.EventHandler(this.TheCancelButton_Click);
			// 
			// SplitsGroupBox
			// 
			this.SplitsGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|c622f160-b674-46c0-937d-83319b5729ad", "Splits", "Splits", "");
			this.SplitsGroupBox.Controls.Add(this.SplitsGrid);
			this.SplitsGroupBox.Controls.Add(this.SplitsButtonPanel);
			this.SplitsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 103, true);
			this.SplitsGroupBox.Name = "SplitsGroupBox";
			this.SplitsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 204, true);
			this.SplitsGroupBox.TabIndex = 2;
			this.SplitsGroupBox.TabStop = false;
			// 
			// SplitsGrid
			// 
			this.SplitsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SplitsGrid, "Splits");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Splits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplit)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Splits)).SyncRoot)).Packs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplit)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Splits)).SyncRoot)).PackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplit)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Splits)).SyncRoot)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplit)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Splits)).SyncRoot)).WeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplit)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Splits)).SyncRoot)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplit)(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster)(null)).Splits)).SyncRoot)).VolumeUnit)));
			this.SplitsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|34059e12-2fec-46a0-a595-5df2928ffa53", "Packs", "Packages", "");
			zCalcEditColumnStyleInfo1.ColumnName = "Packs";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|4f81ac95-9da2-448d-89b5-0d457a3b0912", "Packages");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|5c19b4cc-cb04-4117-b6ab-796dccec4c3c", "Type", "Pack Type", "");
			zDropEditColumnStyleInfo1.ColumnName = "PackType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|4f81ac95-9da2-448d-89b5-0d457a3b0912", "Packages");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|368a706f-0389-4b72-8de1-5bd27f73086a", "Weight", "Weight", "");
			zCalcEditColumnStyleInfo2.ColumnName = "Weight";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|4aee2098-09c7-4a81-bf79-98ac346461d0", "Weight");
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|7a26f732-c7a8-4a87-82f8-1ecec8b50843", "UW", "Weight Unit", "");
			zDropEditColumnStyleInfo2.ColumnName = "WeightUnit";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|4aee2098-09c7-4a81-bf79-98ac346461d0", "Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|7b3fb31c-39f8-420c-b0da-c84447710bb6", "Volume", "Volume", "");
			zCalcEditColumnStyleInfo3.ColumnName = "Volume";
			zCalcEditColumnStyleInfo3.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|", "Volume");
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|f501f758-05d2-4bb1-a7f6-ea86346f3755", "UV", "Volume Unit", "");
			zDropEditColumnStyleInfo3.ColumnName = "VolumeUnit";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|", "Volume");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.SplitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.SplitsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SplitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.SplitsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.SplitsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.SplitsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.SplitsGrid.GridId = "43358263-1ce8-4d11-afd0-a1ca167c4760";
			this.SplitsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SplitsGrid.LayoutKey = "SplitsGrid";
			this.SplitsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 47, true);
			this.SplitsGrid.Name = "SplitsGrid";
			this.SplitsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 154, true);
			this.SplitsGrid.TabIndex = 0;
			// 
			// SplitsButtonPanel
			// 
			this.SplitsButtonPanel.Controls.Add(this.RemoveSplitButton);
			this.SplitsButtonPanel.Controls.Add(this.DivideEquallyButton);
			this.SplitsButtonPanel.Controls.Add(this.AddSplitButton);
			this.SplitsButtonPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.SplitsButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SplitsButtonPanel.Name = "SplitsButtonPanel";
			this.SplitsButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 31, true);
			this.SplitsButtonPanel.TabIndex = 1;
			// 
			// RemoveSplitButton
			// 
			this.RemoveSplitButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|29f3afd2-ace9-4c27-9e50-d4635cd90339", "Remove Split", "Remove Split", "");
			this.RemoveSplitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 2, true);
			this.RemoveSplitButton.Name = "RemoveSplitButton";
			this.RemoveSplitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.RemoveSplitButton.TabIndex = 1;
			this.RemoveSplitButton.UseVisualStyleBackColor = true;
			this.RemoveSplitButton.Click += new System.EventHandler(this.RemoveSplitButton_Click);
			// 
			// DivideEquallyButton
			// 
			this.DivideEquallyButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|3f1ae8a2-e485-431a-84d2-8ed9776b3aea", "Divide Equally", "Divide Equally", "");
			this.DivideEquallyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 2, true);
			this.DivideEquallyButton.Name = "DivideEquallyButton";
			this.DivideEquallyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.DivideEquallyButton.TabIndex = 2;
			this.DivideEquallyButton.UseVisualStyleBackColor = true;
			this.DivideEquallyButton.Click += new System.EventHandler(this.DivideEquallyButton_Click);
			// 
			// AddSplitButton
			// 
			this.AddSplitButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|22887c70-580f-437f-91d6-b9122a075828", "Add Split", "Add Split", "");
			this.AddSplitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.AddSplitButton.Name = "AddSplitButton";
			this.AddSplitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 26, true);
			this.AddSplitButton.TabIndex = 0;
			this.AddSplitButton.UseVisualStyleBackColor = true;
			this.AddSplitButton.Click += new System.EventHandler(this.AddSplitButton_Click);
			// 
			// SaveCancelPanel
			// 
			this.SaveCancelPanel.Controls.Add(this.SaveButton);
			this.SaveCancelPanel.Controls.Add(this.TheCancelButton);
			this.SaveCancelPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SaveCancelPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 307, true);
			this.SaveCancelPanel.Name = "SaveCancelPanel";
			this.SaveCancelPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 31, true);
			this.SaveCancelPanel.TabIndex = 3;
			// 
			// LooseBookedMoveSplitForm
			// 
			this.AcceptButton = this.SaveButton;
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.TheCancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 362, true);
			this.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("LooseBookedMoveSplitForm|0590eb62-d542-4d53-8409-cbf87bd2c452", "Booked Loose Movement Splitter", "Booked Loose Movement Splitter", "");
			this.Controls.Add(this.SplitsGroupBox);
			this.Controls.Add(this.SaveCancelPanel);
			this.Controls.Add(this.totalsGroupBox);
			this.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.LooseBookedMoveSplitMaster);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 398, true);
			this.Name = "LooseBookedMoveSplitForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "LooseBookedMoveSplitForm";
			this.Controls.SetChildIndex(this.totalsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SaveCancelPanel, 0);
			this.Controls.SetChildIndex(this.SplitsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.totalsGroupBox.ResumeLayout(false);
			this.totalsGroupBox.PerformLayout();
			this.SplitsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitsGrid)).EndInit();
			this.SplitsButtonPanel.ResumeLayout(false);
			this.SaveCancelPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox totalsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		private Enterprise.ZArchitecture.GUI.ZButton TheCancelButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SplitsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel SaveCancelPanel;
		private Enterprise.ZArchitecture.ZGrid SplitsGrid;
		private Enterprise.ZArchitecture.GUI.ZPanel SplitsButtonPanel;
		private Enterprise.ZArchitecture.GUI.ZButton DivideEquallyButton;
		private Enterprise.ZArchitecture.GUI.ZButton AddSplitButton;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit JJ_VolumeCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit JJ_WeightCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit JJ_OuterPacksCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZButton RemoveSplitButton;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit TotalVolumeCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit TotalWeightCalcDropEdit;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit TotalPacksCalcDropEdit;
		private Enterprise.ZArchitecture.ZLabel OriginalLabel;
		private Enterprise.ZArchitecture.ZLabel SplitsLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private Enterprise.ZArchitecture.ZLabel RemainingLabel;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit1;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit2;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit3;
	}
}
