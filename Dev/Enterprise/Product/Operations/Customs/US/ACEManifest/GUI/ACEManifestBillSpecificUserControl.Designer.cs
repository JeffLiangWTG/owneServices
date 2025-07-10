using Enterprise.Customs.ASYCUDA.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	partial class ACEManifestBillSpecificUserControl
	{
		internal ZArchitecture.GUI.ZCheckBox FDAIndicatorCheckBox;
		internal ZArchitecture.ZTextBox BillStatusTextBox;
		internal ZArchitecture.ZTextBox BillStatusDescriptionTextBox;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl GoodsValueConvertToLocalCurrencyControl;
		internal ZArchitecture.GUI.ZDropEdit EntryNumberTypeDropEdit;
		internal ZArchitecture.ZTextBox EntryNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox GoodsOriginCodeFindBox;
		internal Universal.GUI.TariffFindBox TariffCodeFindBox;

		void InitializeComponent()
		{
			this.FDAIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BillStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsValueConvertToLocalCurrencyControl.SuspendLayout();
			this.EntryNumberTypeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.EntryNumberTextBox = new ZArchitecture.ZTextBox();
			this.GoodsOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TariffCodeFindBox = new Universal.GUI.TariffFindBox();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.ACEManifest.Business.AsycudaBill);
			// 
			// FDAIndicatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FDAIndicatorCheckBox, "FDAIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaBill)(null)).FDAIndicator)));
			this.FDAIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FDAIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 2, true);
			this.FDAIndicatorCheckBox.Name = "FDAIndicatorCheckBox";
			this.FDAIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.FDAIndicatorCheckBox.TabIndex = 1;
			this.FDAIndicatorCheckBox.UseVisualStyleBackColor = true;
			// 
			// BillStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillStatusTextBox, "ABL_BillStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaBill)(null)).ABL_BillStatus)));
			this.BillStatusTextBox.CaptionResourceString = null;
			this.BillStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 31, true);
			this.BillStatusTextBox.Name = "BillStatusTextBox";
			this.BillStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.BillStatusTextBox.TabIndex = 2;
			// 
			// BillStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillStatusDescriptionTextBox, "ABL_BillStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaBill)(null)).ABL_BillStatusDescription)));
			this.BillStatusDescriptionTextBox.CaptionResourceString = Res.GetData("aa702ad7-386b-45f7-84bc-4856fea94fa8", " ");
			this.BillStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 31, true);
			this.BillStatusDescriptionTextBox.Name = "BillStatusDescriptionTextBox";
			this.BillStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 68, true);
			this.BillStatusDescriptionTextBox.Multiline = true;
			this.BillStatusDescriptionTextBox.WordWrap = true;
			this.BillStatusDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BillStatusDescriptionTextBox.TabIndex = 3;
			// 
			// CustomsValueConvertToLocalCurrencyControl
			// 
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaBill)(null)).ABL_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaBill)(null)).ABL_RX_NKGoodsValueCurrency)));
			this.GoodsValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.GoodsValueConvertToLocalCurrencyControl.Decimals = 0;
			this.GoodsValueConvertToLocalCurrencyControl.BindToAmount = AsycudaBill.Schema.ABL_GoodsValue;
			this.GoodsValueConvertToLocalCurrencyControl.BindToUnit = AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency;
			this.GoodsValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.GoodsValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(740, 111, true);
			this.GoodsValueConvertToLocalCurrencyControl.Name = "GoodsValueConvertToLocalCurrencyControl";
			this.GoodsValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.GoodsValueConvertToLocalCurrencyControl.TabIndex = 4;
			//
			// EntryNumberTypeDropEdit
			//
			this.BindingSource.SetBindingMember(this.EntryNumberTypeDropEdit, "CustomsEntryNumberType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaBill)(null)).CustomsEntryNumberType)));
			this.EntryNumberTypeDropEdit.CaptionResourceString = Res.GetData("FDBFEFCD-4327-4330-BB67-F7F0DBA27BA1", "Entry Type");
			this.EntryNumberTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 31, true);
			this.EntryNumberTypeDropEdit.Name = "EntryNumberTypeDropEdit";
			this.EntryNumberTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.EntryNumberTypeDropEdit.TabIndex = 5;
			//
			// EntryNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "CustomsEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaBill)(null)).CustomsEntryNumber)));
			this.EntryNumberTextBox.CaptionResourceString = Res.GetData("CCB84FCB-76DC-470D-9539-3E367009EBAA", "Entry Number");
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 31, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.EntryNumberTextBox.TabIndex = 6;
			// 
			// GoodsOriginCodeFindBox
			// 
			this.GoodsOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginCodeFindBox, "GoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaBill)(null)).GoodsOrigin)));
			this.GoodsOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 31, true);
			this.GoodsOriginCodeFindBox.Name = "GoodsOriginCodeFindBox";
			this.GoodsOriginCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsOriginCodeFindBox.ParentType = null;
			this.GoodsOriginCodeFindBox.PreBoundMaxLength = 2;
			this.GoodsOriginCodeFindBox.ShowDescriptionBox = true;
			this.GoodsOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.GoodsOriginCodeFindBox.TabIndex = 7;
			// 
			// TariffCodeFindBox
			// 
			this.TariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "ABL_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.ACEManifest.Business.AsycudaBill)(null)).ABL_Tariff)));
			this.TariffCodeFindBox.ErrorForUnsupportedCountry = null;
			this.TariffCodeFindBox.GetEffectiveDate = null;
			this.TariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 31, true);
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffCodeFindBox.ParentType = null;
			this.TariffCodeFindBox.PreBoundMaxLength = 11;
			this.TariffCodeFindBox.SelectNomenclatureModes = null;
			this.TariffCodeFindBox.ShouldResize = false;
			this.TariffCodeFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.TariffCodeFindBox.TabIndex = 8;
			this.TariffCodeFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			// 
			// ACEManifestBillSpecificUserControl
			// 
			this.Controls.Add(this.FDAIndicatorCheckBox);
			this.Controls.Add(this.BillStatusTextBox);
			this.Controls.Add(this.BillStatusDescriptionTextBox);
			this.Controls.Add(this.GoodsValueConvertToLocalCurrencyControl);
			this.Controls.Add(this.EntryNumberTypeDropEdit);
			this.Controls.Add(this.EntryNumberTextBox);
			this.Controls.Add(this.GoodsOriginCodeFindBox);
			this.Controls.Add(this.TariffCodeFindBox);
			this.Name = "ACEManifestBillSpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 67, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsValueConvertToLocalCurrencyControl.ResumeLayout(true);
			this.GoodsValueConvertToLocalCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
