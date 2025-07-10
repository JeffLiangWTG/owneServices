using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CommercialInvoiceDetailsUserControl
	{

		private void InitializeComponent()
		{
			this.InvoiceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GroupInvoiceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceAmountConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.InvoiceCurrExRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IncoTermsUserControl = new Enterprise.Customs.GUI.CommercialInvoiceDetailsIncoTermsUserControl();
			this.IncoTermPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.InvoiceCurrLandedCostExRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NoOfPacksCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.AdditionalTermsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvoiceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ValuationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExporterAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.IncoTermDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupInvoiceDropEdit.SuspendLayout();
			this.InvoiceAmountConvertToLocalCurrencyControl.SuspendLayout();
			this.IncoTermsUserControl.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.NoOfPacksCalcDropEdit.SuspendLayout();
			this.InvoiceDateEdit.SuspendLayout();
			this.ValuationCodeDropEdit.SuspendLayout();
			this.ExporterAddressControl.SuspendLayout();
			this.IncoTermDropEdit.SuspendLayout();
			this.PaymentMethodDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobComInvoiceHeader);
			// 
			// InvoiceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.InvoiceNumberTextBox, "JZ_InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_InvoiceNumber)));
			this.InvoiceNumberTextBox.CaptionResourceString = null;
			this.InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 4, true);
			this.InvoiceNumberTextBox.Name = "InvoiceNumberTextBox";
			this.InvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.InvoiceNumberTextBox.TabIndex = 1;
			// 
			// GroupInvoiceDropEdit
			// 
			this.GroupInvoiceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GroupInvoiceDropEdit, "JZ_Calc_GroupInvoice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_Calc_GroupInvoice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).Lookups.JZ_JZ_GroupInvoiceFK_List)));
			this.GroupInvoiceDropEdit.BindToList = "Lookups+JZ_JZ_GroupInvoiceFK_List";
			this.GroupInvoiceDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("42616a6d-b59e-4746-9547-42bb7fc60340", "Group Invoice", "Immediate Group Invoice.");
			this.GroupInvoiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 28, true);
			this.GroupInvoiceDropEdit.Name = "GroupInvoiceDropEdit";
			this.GroupInvoiceDropEdit.PreBoundMaxLength = 35;
			this.GroupInvoiceDropEdit.ShowDescriptionBox = false;
			this.GroupInvoiceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.GroupInvoiceDropEdit.TabIndex = 2;
			// 
			// InvoiceAmountConvertToLocalCurrencyControl
			// 
			this.InvoiceAmountConvertToLocalCurrencyControl.AllowDrop = true;
			this.InvoiceAmountConvertToLocalCurrencyControl.BindToAmount = "JZ_InvoiceAmount";
			this.InvoiceAmountConvertToLocalCurrencyControl.BindToList = "Lookups.CurrencyList";
			this.InvoiceAmountConvertToLocalCurrencyControl.BindToUnit = "JZ_RX_NKInvoice_Currency";
			this.InvoiceAmountConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.InvoiceAmountConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 52, true);
			this.InvoiceAmountConvertToLocalCurrencyControl.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.InvoiceAmountConvertToLocalCurrencyControl.Name = "InvoiceAmountConvertToLocalCurrencyControl";
			this.InvoiceAmountConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.InvoiceAmountConvertToLocalCurrencyControl.TabIndex = 3;
			// 
			// InvoiceCurrExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceCurrExRateCalcEdit, "JZ_InvoiceCurrExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_InvoiceCurrExRate)));
			this.InvoiceCurrExRateCalcEdit.CaptionResourceString = null;
			this.InvoiceCurrExRateCalcEdit.DecimalPlaces = 2;
			this.InvoiceCurrExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 76, true);
			this.InvoiceCurrExRateCalcEdit.Name = "InvoiceCurrExRateCalcEdit";
			this.InvoiceCurrExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.InvoiceCurrExRateCalcEdit.TabIndex = 4;
			this.InvoiceCurrExRateCalcEdit.Text = "0.000000";
			this.InvoiceCurrExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IncoTermsUserControl
			// 
			this.IncoTermsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermsUserControl, ".");
			this.IncoTermsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 100, true);
			this.IncoTermsUserControl.Name = "IncoTermsUserControl";
			this.IncoTermsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 22, true);
			this.IncoTermsUserControl.TabIndex = 5;
			// 
			// IncoTermPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.IncoTermPlaceTextBox, "JZ_IncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_IncoTermPlace)));
			this.IncoTermPlaceTextBox.CaptionResourceString = null;
			this.IncoTermPlaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 123, true);
			this.IncoTermPlaceTextBox.Name = "IncoTermPlaceTextBox";
			this.IncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.IncoTermPlaceTextBox.TabIndex = 6;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_WeightUQ)));
			this.GrossWeightCalcDropEdit.BindToAmount = "JZ_Weight";
			this.GrossWeightCalcDropEdit.BindToUnit = "JZ_WeightUQ";
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 169, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 8;
			this.GrossWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_NetWeightUQ)));
			this.NetWeightCalcDropEdit.BindToAmount = "JZ_NetWeight";
			this.NetWeightCalcDropEdit.BindToUnit = "JZ_NetWeightUQ";
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 195, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 9;
			this.NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// InvoiceCurrLandedCostExRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.InvoiceCurrLandedCostExRateCalcEdit, "JZ_InvoiceCurrLandedCostExRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_InvoiceCurrLandedCostExRate)));
			this.InvoiceCurrLandedCostExRateCalcEdit.CaptionResourceString = null;
			this.InvoiceCurrLandedCostExRateCalcEdit.DecimalPlaces = 2;
			this.InvoiceCurrLandedCostExRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 221, true);
			this.InvoiceCurrLandedCostExRateCalcEdit.Name = "InvoiceCurrLandedCostExRateCalcEdit";
			this.InvoiceCurrLandedCostExRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.InvoiceCurrLandedCostExRateCalcEdit.TabIndex = 10;
			this.InvoiceCurrLandedCostExRateCalcEdit.Text = "0.000000";
			this.InvoiceCurrLandedCostExRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NoOfPacksCalcDropEdit
			// 
			this.NoOfPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NoOfPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_NoOfPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).NoOfPacksPackType)));
			this.NoOfPacksCalcDropEdit.BindToAmount = "JZ_NoOfPacks";
			this.NoOfPacksCalcDropEdit.BindToUnit = "NoOfPacksPackType";
			this.NoOfPacksCalcDropEdit.Decimals = 3;
			this.NoOfPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 247, true);
			this.NoOfPacksCalcDropEdit.Name = "NoOfPacksCalcDropEdit";
			this.NoOfPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.NoOfPacksCalcDropEdit.TabIndex = 11;
			this.NoOfPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// AdditionalTermsTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalTermsTextBox, "JZ_AdditionalTerms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_AdditionalTerms)));
			this.AdditionalTermsTextBox.CaptionResourceString = null;
			this.AdditionalTermsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 146, true);
			this.AdditionalTermsTextBox.Name = "AdditionalTermsTextBox";
			this.AdditionalTermsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.AdditionalTermsTextBox.TabIndex = 7;
			// 
			// InvoiceDateEdit
			// 
			this.InvoiceDateEdit.AllowDrop = true;
			this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "JZ_InvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_InvoiceDate)));
			this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 271, true);
			this.InvoiceDateEdit.Name = "InvoiceDateEdit";
			this.InvoiceDateEdit.TabIndex = 12;
			// 
			// ValuationCodeDropEdit
			// 
			this.ValuationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationCodeDropEdit, "JZ_ValuationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_ValuationCode)));
			this.ValuationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 295, true);
			this.ValuationCodeDropEdit.Name = "ValuationCodeDropEdit";
			this.ValuationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ValuationCodeDropEdit.TabIndex = 13;
			// 
			// UCRTextBox
			// 
			this.BindingSource.SetBindingMember(this.UCRTextBox, "JZ_UCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_UCR)));
			this.UCRTextBox.CaptionResourceString = null;
			this.UCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 319, true);
			this.UCRTextBox.Name = "UCRTextBox";
			this.UCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.UCRTextBox.TabIndex = 14;
			// 
			// ExporterAddressControl
			// 
			this.ExporterAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExporterAddressControl, "JZ_OA_ExporterAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_OA_ExporterAddress)));
			this.ExporterAddressControl.BindToOrgList = "Lookups+Exporters";
			this.ExporterAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 347, true);
			this.ExporterAddressControl.Name = "ExporterAddressControl";
			this.ExporterAddressControl.PopupCaption = "";
			this.ExporterAddressControl.ReadOnly = false;
			this.ExporterAddressControl.ShowAddress = false;
			this.ExporterAddressControl.ShowOrganisationName = false;
			this.ExporterAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.ExporterAddressControl.TabIndex = 15;
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermDropEdit, "JZ_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_IncoTerm)));
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 333, true);
			this.IncoTermDropEdit.Name = "IncoTermDropEdit";
			this.IncoTermDropEdit.PreBoundMaxLength = 3;
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.IncoTermDropEdit.TabIndex = 15;
			// 
			// PaymentMethodDropEdit
			// 
			this.PaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentMethodDropEdit, "JZ_PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(null)).JZ_PaymentMethod)));
			this.PaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 383, true);
			this.PaymentMethodDropEdit.Name = "PaymentMethodDropEdit";
			this.PaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PaymentMethodDropEdit.TabIndex = 16;
			// 
			// CommercialInvoiceDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PaymentMethodDropEdit);
			this.Controls.Add(this.NoOfPacksCalcDropEdit);
			this.Controls.Add(this.InvoiceCurrLandedCostExRateCalcEdit);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.IncoTermsUserControl);
			this.Controls.Add(this.InvoiceCurrExRateCalcEdit);
			this.Controls.Add(this.InvoiceAmountConvertToLocalCurrencyControl);
			this.Controls.Add(this.GroupInvoiceDropEdit);
			this.Controls.Add(this.InvoiceNumberTextBox);
			this.Controls.Add(this.AdditionalTermsTextBox);
			this.Controls.Add(this.InvoiceDateEdit);
			this.Controls.Add(this.ValuationCodeDropEdit);
			this.Controls.Add(this.UCRTextBox);
			this.Controls.Add(this.IncoTermPlaceTextBox);
			this.Controls.Add(this.ExporterAddressControl);
			this.Controls.Add(this.IncoTermDropEdit);
			this.Name = "CommercialInvoiceDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 435, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupInvoiceDropEdit.ResumeLayout(true);
			this.GroupInvoiceDropEdit.PerformLayout();
			this.InvoiceAmountConvertToLocalCurrencyControl.ResumeLayout(true);
			this.InvoiceAmountConvertToLocalCurrencyControl.PerformLayout();
			this.IncoTermsUserControl.ResumeLayout(true);
			this.IncoTermsUserControl.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.NoOfPacksCalcDropEdit.ResumeLayout(true);
			this.NoOfPacksCalcDropEdit.PerformLayout();
			this.InvoiceDateEdit.ResumeLayout(true);
			this.InvoiceDateEdit.PerformLayout();
			this.ValuationCodeDropEdit.ResumeLayout(true);
			this.ValuationCodeDropEdit.PerformLayout();
			this.ExporterAddressControl.ResumeLayout(true);
			this.ExporterAddressControl.PerformLayout();
			this.IncoTermDropEdit.ResumeLayout(true);
			this.IncoTermDropEdit.PerformLayout();
			this.PaymentMethodDropEdit.ResumeLayout(true);
			this.PaymentMethodDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZTextBox InvoiceNumberTextBox;
		internal ZDropEdit GroupInvoiceDropEdit;
		internal ConvertToLocalCurrencyControl InvoiceAmountConvertToLocalCurrencyControl;
		internal ZAddressControl ExporterAddressControl;
		internal ZCalcEdit InvoiceCurrExRateCalcEdit;
		internal CommercialInvoiceDetailsIncoTermsUserControl IncoTermsUserControl;
		internal ZTextBox IncoTermPlaceTextBox;
		internal ZTextBox AdditionalTermsTextBox;
		internal ZCalcDropEdit GrossWeightCalcDropEdit;
		internal ZCalcDropEdit NetWeightCalcDropEdit;
		internal ZCalcEdit InvoiceCurrLandedCostExRateCalcEdit;
		internal ZCalcDropEdit NoOfPacksCalcDropEdit;
		internal ZDateEdit InvoiceDateEdit;
		internal ZDropEdit ValuationCodeDropEdit;
		internal ZTextBox UCRTextBox;
		internal ZDropEdit IncoTermDropEdit;
		internal ZDropEdit PaymentMethodDropEdit;
	}
}
