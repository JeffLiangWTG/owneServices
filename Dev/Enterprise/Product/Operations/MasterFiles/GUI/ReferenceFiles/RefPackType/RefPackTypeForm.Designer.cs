namespace Enterprise.MasterFiles.GUI
{
	public sealed partial class RefPackTypeForm
	{
		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 349, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 322, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 322, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(683, 322, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 349, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 0;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(351);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(351);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefPackType);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_IsUpdatable)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_Description)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_Height)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_Length)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_Width)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_UnitOfDimension)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_Weight)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_UnitOfWeight)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_UOMType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefPackType)(null)).F3_KeepUpright)));
			// 
			// RefPackTypeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefPackTypeForm|eb6a0050-eb0c-438e-a9c2-d8b2d8dfd87b", "Package Type");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(691, 405, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefPackType);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 431, true);
			this.Name = "RefPackTypeForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.F3_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.F3_IsUpdatableBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.F3_DescriptionTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.F3_HeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.F3_LengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.F3_WidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.F3_UnitOfDimensionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.F3_WeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.F3_UnitOfWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.F3_UOMTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PackTypeIsReservedTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.KeepUprightCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabPage.SuspendLayout();
			this.F3_DescriptionTextBox.SuspendLayout();
			this.F3_UnitOfDimensionDropEdit.SuspendLayout();
			this.F3_UnitOfWeightDropEdit.SuspendLayout();
			this.F3_UOMTypeDropEdit.SuspendLayout();
			this.MainTabPage.Controls.Add(this.KeepUprightCheckBox);
			this.MainTabPage.Controls.Add(this.F3_UOMTypeDropEdit);
			this.MainTabPage.Controls.Add(this.F3_UnitOfWeightDropEdit);
			this.MainTabPage.Controls.Add(this.F3_WeightCalcEdit);
			this.MainTabPage.Controls.Add(this.F3_UnitOfDimensionDropEdit);
			this.MainTabPage.Controls.Add(this.F3_WidthCalcEdit);
			this.MainTabPage.Controls.Add(this.F3_LengthCalcEdit);
			this.MainTabPage.Controls.Add(this.F3_HeightCalcEdit);
			this.MainTabPage.Controls.Add(this.F3_DescriptionTextBox);
			this.MainTabPage.Controls.Add(this.F3_IsUpdatableBox);
			this.MainTabPage.Controls.Add(this.F3_CodeTextBox);
			this.MainTabPage.Controls.Add(this.PackTypeIsReservedTypeLabel);
			// 
			// F3_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.F3_CodeTextBox, "F3_Code");
			this.F3_CodeTextBox.CaptionResourceString = null;
			this.F3_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 28, true);
			this.F3_CodeTextBox.Name = "F3_CodeTextBox";
			this.F3_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.F3_CodeTextBox.TabIndex = 0;
			// 
			// F3_IsUpdatableBox
			// 
			this.F3_IsUpdatableBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.F3_IsUpdatableBox, "F3_IsUpdatable");
			this.F3_IsUpdatableBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.F3_IsUpdatableBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 31, true);
			this.F3_IsUpdatableBox.Name = "F3_IsUpdatableBox";
			this.F3_IsUpdatableBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 17, true);
			this.F3_IsUpdatableBox.TabIndex = 1;
			this.F3_IsUpdatableBox.UseVisualStyleBackColor = true;
			// 
			// F3_DescriptionTextBox
			// 
			this.F3_DescriptionTextBox.AcceptsReturn = false;
			this.F3_DescriptionTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.F3_DescriptionTextBox, "F3_Description");
			this.F3_DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.F3_DescriptionTextBox.GridCurrent = null;
			this.F3_DescriptionTextBox.GridMember = null;
			this.F3_DescriptionTextBox.IsLanguageEditingEnabled = true;
			this.F3_DescriptionTextBox.IsMultiLine = false;
			this.F3_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 54, true);
			this.F3_DescriptionTextBox.Name = "F3_DescriptionTextBox";
			this.F3_DescriptionTextBox.ReadOnly = false;
			this.F3_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.F3_DescriptionTextBox.TabIndex = 2;
			// 
			// F3_HeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.F3_HeightCalcEdit, "F3_Height");
			this.F3_HeightCalcEdit.CaptionResourceString = null;
			this.F3_HeightCalcEdit.DecimalPlaces = 2;
			this.F3_HeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 81, true);
			this.F3_HeightCalcEdit.Name = "F3_HeightCalcEdit";
			this.F3_HeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.F3_HeightCalcEdit.TabIndex = 3;
			this.F3_HeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// F3_LengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.F3_LengthCalcEdit, "F3_Length");
			this.F3_LengthCalcEdit.CaptionResourceString = null;
			this.F3_LengthCalcEdit.DecimalPlaces = 2;
			this.F3_LengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 108, true);
			this.F3_LengthCalcEdit.Name = "F3_LengthCalcEdit";
			this.F3_LengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.F3_LengthCalcEdit.TabIndex = 4;
			this.F3_LengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// F3_WidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.F3_WidthCalcEdit, "F3_Width");
			this.F3_WidthCalcEdit.CaptionResourceString = null;
			this.F3_WidthCalcEdit.DecimalPlaces = 2;
			this.F3_WidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 135, true);
			this.F3_WidthCalcEdit.Name = "F3_WidthCalcEdit";
			this.F3_WidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.F3_WidthCalcEdit.TabIndex = 5;
			this.F3_WidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// F3_UnitOfDimensionDropEdit
			// 
			this.F3_UnitOfDimensionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.F3_UnitOfDimensionDropEdit, "F3_UnitOfDimension");
			this.F3_UnitOfDimensionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 162, true);
			this.F3_UnitOfDimensionDropEdit.Name = "F3_UnitOfDimensionDropEdit";
			this.F3_UnitOfDimensionDropEdit.ShouldResizeByMaxLength = true;
			this.F3_UnitOfDimensionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.F3_UnitOfDimensionDropEdit.TabIndex = 6;
			// 
			// F3_WeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.F3_WeightCalcEdit, "F3_Weight");
			this.F3_WeightCalcEdit.CaptionResourceString = null;
			this.F3_WeightCalcEdit.DecimalPlaces = 2;
			this.F3_WeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 189, true);
			this.F3_WeightCalcEdit.Name = "F3_WeightCalcEdit";
			this.F3_WeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.F3_WeightCalcEdit.TabIndex = 7;
			this.F3_WeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// F3_UnitOfWeightDropEdit
			// 
			this.F3_UnitOfWeightDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.F3_UnitOfWeightDropEdit, "F3_UnitOfWeight");
			this.F3_UnitOfWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 216, true);
			this.F3_UnitOfWeightDropEdit.Name = "F3_UnitOfWeightDropEdit";
			this.F3_UnitOfWeightDropEdit.ShouldResizeByMaxLength = true;
			this.F3_UnitOfWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.F3_UnitOfWeightDropEdit.TabIndex = 8;
			// 
			// F3_UOMTypeDropEdit
			// 
			this.F3_UOMTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.F3_UOMTypeDropEdit, "F3_UOMType");
			this.F3_UOMTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 243, true);
			this.F3_UOMTypeDropEdit.Name = "F3_UOMTypeDropEdit";
			this.F3_UOMTypeDropEdit.ShouldResizeByMaxLength = true;
			this.F3_UOMTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(331, 20, true);
			this.F3_UOMTypeDropEdit.TabIndex = 9;
			// 
			// PackTypeIsReservedTypeLabel
			// 
			this.PackTypeIsReservedTypeLabel.AutoSize = true;
			this.PackTypeIsReservedTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PackTypeIsReservedTypeLabel.ForeColor = System.Drawing.Color.Green;
			this.PackTypeIsReservedTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 5, true);
			this.PackTypeIsReservedTypeLabel.Name = "PackTypeIsReservedTypeLabel";
			this.PackTypeIsReservedTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 13, true);
			this.PackTypeIsReservedTypeLabel.TabIndex = 10;
			this.PackTypeIsReservedTypeLabel.Text = Enterprise.MasterFiles.GUI.Res.GetString("d55f4417-aaac-4a41-8278-eaa99bd23828", "Package Type 'CNT' is system reserved for a sea or air freight container and cannot be used in Forwarding or Brokerage modules.");
			// 
			// KeepUprightCheckBox
			// 
			this.KeepUprightCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.KeepUprightCheckBox, "F3_KeepUpright");
			this.KeepUprightCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.KeepUprightCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 83, true);
			this.KeepUprightCheckBox.Name = "KeepUprightCheckBox";
			this.KeepUprightCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 17, true);
			this.KeepUprightCheckBox.TabIndex = 11;
			this.KeepUprightCheckBox.UseVisualStyleBackColor = true;
			this.MainTabPage.PerformLayout();
			this.F3_DescriptionTextBox.ResumeLayout(true);
			this.F3_DescriptionTextBox.PerformLayout();
			this.F3_UnitOfDimensionDropEdit.ResumeLayout(true);
			this.F3_UnitOfDimensionDropEdit.PerformLayout();
			this.F3_UnitOfWeightDropEdit.ResumeLayout(true);
			this.F3_UnitOfWeightDropEdit.PerformLayout();
			this.F3_UOMTypeDropEdit.ResumeLayout(true);
			this.F3_UOMTypeDropEdit.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);
		}

		#endregion

		private Enterprise.ZArchitecture.ZTranslatableTextControl F3_DescriptionTextBox;
		private Enterprise.ZArchitecture.ZTextBox F3_CodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit F3_UnitOfWeightDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit F3_UOMTypeDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit F3_WeightCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit F3_UnitOfDimensionDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit F3_WidthCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit F3_LengthCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit F3_HeightCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox F3_IsUpdatableBox;
		internal Enterprise.ZArchitecture.ZLabel PackTypeIsReservedTypeLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox KeepUprightCheckBox;
	}
}
