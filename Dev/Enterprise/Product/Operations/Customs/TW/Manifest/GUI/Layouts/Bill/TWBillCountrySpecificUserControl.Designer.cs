namespace Enterprise.Customs.TW.Manifest.GUI
{
	partial class TWBillCountrySpecificUserControl
	{
		private void InitializeComponent()
		{
			this.PortOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GoodsDescriptionLongTextControl = new Enterprise.ZArchitecture.ZTextBox();
			this.BagNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SplitQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ManifestQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.DGUNNOCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.IsEscortRequiredCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MarksAndNumbersLongTextControl = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PortOfLoadingCodeFindBox.SuspendLayout();
			this.BagNumberDropEdit.SuspendLayout();
			this.SplitQuantityCalcDropEdit.SuspendLayout();
			this.ManifestQtyCalcDropEdit.SuspendLayout();
			this.TariffFindBox.SuspendLayout();
			this.DGUNNOCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Manifest.Business.AsycudaBill);
			// 
			// PortOfLoadingCodeFindBox
			// 
			this.PortOfLoadingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingCodeFindBox, "ABL_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_RL_NKPortOfLoading)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PortOfLoadingCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.PortOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 157, true);
			this.PortOfLoadingCodeFindBox.Name = "PortOfLoadingCodeFindBox";
			this.PortOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PortOfLoadingCodeFindBox.ParentType = null;
			this.PortOfLoadingCodeFindBox.PreBoundMaxLength = 5;
			this.PortOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.PortOfLoadingCodeFindBox.TabIndex = 0;
			// 
			// GoodsDescriptionLongTextControl
			// 
			this.GoodsDescriptionLongTextControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsDescriptionLongTextControl, "GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).GoodsDescription)));
			this.GoodsDescriptionLongTextControl.CaptionResourceString = null;
			this.GoodsDescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 13, true);
			this.GoodsDescriptionLongTextControl.Multiline = true;
			this.GoodsDescriptionLongTextControl.Name = "GoodsDescriptionLongTextControl";
			this.GoodsDescriptionLongTextControl.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.GoodsDescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 114, true);
			this.GoodsDescriptionLongTextControl.TabIndex = 1;
			// 
			// BagNumberDropEdit
			// 
			this.BagNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BagNumberDropEdit, "BagNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).BagNumber)));
			this.BagNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 105, true);
			this.BagNumberDropEdit.Name = "BagNumberDropEdit";
			this.BagNumberDropEdit.ShouldResizeByMaxLength = false;
			this.BagNumberDropEdit.ShowDescriptionBox = false;
			this.BagNumberDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.BagNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.BagNumberDropEdit.TabIndex = 2;
			this.BagNumberDropEdit.UseFullWidthForCodeBox = true;
			// 
			// SplitQuantityCalcDropEdit
			// 
			this.SplitQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SplitQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_SplitQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_SplitQuantityUQ)));
			this.SplitQuantityCalcDropEdit.BindToAmount = "ABL_SplitQuantity";
			this.SplitQuantityCalcDropEdit.BindToUnit = "ABL_SplitQuantityUQ";
			this.SplitQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 237, true);
			this.SplitQuantityCalcDropEdit.MaxValue = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
			this.SplitQuantityCalcDropEdit.Name = "SplitQuantityCalcDropEdit";
			this.SplitQuantityCalcDropEdit.ShowDescriptionBox = true;
			this.SplitQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.SplitQuantityCalcDropEdit.TabIndex = 28;
			this.SplitQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// ManifestQtyCalcDropEdit
			// 
			this.ManifestQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManifestQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ManifestQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_ManifestUQ)));
			this.ManifestQtyCalcDropEdit.BindToAmount = "ABL_ManifestQty";
			this.ManifestQtyCalcDropEdit.BindToUnit = "ABL_ManifestUQ";
			this.ManifestQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 195, true);
			this.ManifestQtyCalcDropEdit.MaxValue = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
			this.ManifestQtyCalcDropEdit.Name = "ManifestQtyCalcDropEdit";
			this.ManifestQtyCalcDropEdit.ShowDescriptionBox = true;
			this.ManifestQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.ManifestQtyCalcDropEdit.TabIndex = 29;
			this.ManifestQtyCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// TariffFindBox
			// 
			this.TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffFindBox, "ABL_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_Tariff)));
			this.TariffFindBox.ErrorForUnsupportedCountry = null;
			this.TariffFindBox.GetEffectiveDate = null;
			this.TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 263, true);
			this.TariffFindBox.Name = "TariffFindBox";
			this.TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffFindBox.ParentType = null;
			this.TariffFindBox.PreBoundMaxLength = 11;
			this.TariffFindBox.SelectNomenclatureModes = null;
			this.TariffFindBox.ShouldResize = false;
			this.TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.TariffFindBox.TabIndex = 30;
			this.TariffFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			// 
			// DGUNNOCodeFindBox
			// 
			this.DGUNNOCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGUNNOCodeFindBox, "ABL_DG_UNNO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_DG_UNNO)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DGUNNOCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.DGUNNOCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 298, true);
			this.DGUNNOCodeFindBox.Name = "DGUNNOCodeFindBox";
			this.DGUNNOCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DGUNNOCodeFindBox.ParentType = null;
			this.DGUNNOCodeFindBox.PreBoundMaxLength = 5;
			this.DGUNNOCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DGUNNOCodeFindBox.TabIndex = 31;
			// 
			// IsEscortRequiredCheckBox
			// 
			this.IsEscortRequiredCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsEscortRequiredCheckBox, "IsEscortRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).IsEscortRequired)));
			this.IsEscortRequiredCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 333, true);
			this.IsEscortRequiredCheckBox.Name = "IsEscortRequiredCheckBox";
			this.IsEscortRequiredCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsEscortRequiredCheckBox.TabIndex = 32;
			this.IsEscortRequiredCheckBox.UseVisualStyleBackColor = true;
			// 
			// MarksAndNumbersLongTextControl
			// 
			this.MarksAndNumbersLongTextControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MarksAndNumbersLongTextControl, "ABL_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaBill)(null)).ABL_MarksAndNumbers)));
			this.MarksAndNumbersLongTextControl.CaptionResourceString = null;
			this.MarksAndNumbersLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 133, true);
			this.MarksAndNumbersLongTextControl.Multiline = true;
			this.MarksAndNumbersLongTextControl.Name = "MarksAndNumbersLongTextControl";
			this.MarksAndNumbersLongTextControl.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MarksAndNumbersLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 114, true);
			this.MarksAndNumbersLongTextControl.TabIndex = 33;
			// 
			// TWBillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MarksAndNumbersLongTextControl);
			this.Controls.Add(this.IsEscortRequiredCheckBox);
			this.Controls.Add(this.DGUNNOCodeFindBox);
			this.Controls.Add(this.TariffFindBox);
			this.Controls.Add(this.ManifestQtyCalcDropEdit);
			this.Controls.Add(this.SplitQuantityCalcDropEdit);
			this.Controls.Add(this.BagNumberDropEdit);
			this.Controls.Add(this.PortOfLoadingCodeFindBox);
			this.Controls.Add(this.GoodsDescriptionLongTextControl);
			this.Name = "TWBillCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 498, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PortOfLoadingCodeFindBox.ResumeLayout(true);
			this.PortOfLoadingCodeFindBox.PerformLayout();
			this.BagNumberDropEdit.ResumeLayout(true);
			this.BagNumberDropEdit.PerformLayout();
			this.SplitQuantityCalcDropEdit.ResumeLayout(true);
			this.SplitQuantityCalcDropEdit.PerformLayout();
			this.ManifestQtyCalcDropEdit.ResumeLayout(true);
			this.ManifestQtyCalcDropEdit.PerformLayout();
			this.TariffFindBox.ResumeLayout(true);
			this.TariffFindBox.PerformLayout();
			this.DGUNNOCodeFindBox.ResumeLayout(true);
			this.DGUNNOCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZCodeFindBox PortOfLoadingCodeFindBox;
		internal ZArchitecture.ZTextBox GoodsDescriptionLongTextControl;
		internal ZArchitecture.GUI.ZDropEdit BagNumberDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit SplitQuantityCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit ManifestQtyCalcDropEdit;
		internal Universal.GUI.TariffFindBox TariffFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox DGUNNOCodeFindBox;
		internal ZArchitecture.GUI.ZCheckBox IsEscortRequiredCheckBox;
		internal ZArchitecture.ZTextBox MarksAndNumbersLongTextControl;
	}
}
