namespace Enterprise.Customs.GUI
{
	public partial class GeneralCountryInvoiceLineUserControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.JI_ProcedureFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ProductCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JI_TariffFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.CustomsSecondQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsThirdQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TaxOrFeeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreferenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
			this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
			this.LineChargesTabPage.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.CusContainerInvoiceLineGrid.SuspendLayout();
			this.LineDetailsTabPage.SuspendLayout();
			this.NewLineDetailsTabPage.SuspendLayout();
			this.InvoiceLineDetailsUserControl.SuspendLayout();
			this.ClassificationPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).BeginInit();
			this.CustomsInvoiceLinesBoundGrid.SuspendLayout();
			this.JI_Calc_CIFConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_FreightConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_FOBConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_GSTConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.SuspendLayout();
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.SuspendLayout();
			this.CustomsQuantityCalcDropEdit.SuspendLayout();
			this.VolumeCalcDropEdit.SuspendLayout();
			this.JI_WeightCalcDropEdit.SuspendLayout();
			this.InvoiceQuantityCalcDropEdit.SuspendLayout();
			this.JI_DescriptionBoundTextBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JI_ProcedureFindBox.SuspendLayout();
			this.ProductCodeFindBox.SuspendLayout();
			this.JI_TariffFindBox.SuspendLayout();
			this.CustomsSecondQuantityCalcDropEdit.SuspendLayout();
			this.CustomsThirdQuantityCalcDropEdit.SuspendLayout();
			this.TaxOrFeeDropEdit.SuspendLayout();
			this.PreferenceDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// TopPanel
			// 
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 533, true);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Controls.Add(this.CustomsThirdQuantityCalcDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.CustomsSecondQuantityCalcDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.PreferenceDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.TaxOrFeeDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.CustomsQuantityCalcDropEdit);
			this.InvoiceDetailsGroupBox.Controls.Add(this.ProductCodeFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_TariffFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_ProcedureFindBox);
			this.InvoiceDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_ProcedureFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_TariffFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.ProductCodeFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.TaxOrFeeDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.PreferenceDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CustomsSecondQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CustomsThirdQuantityCalcDropEdit, 0);
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 173, true);
			this.JI_CountryOfOriginBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_CountryOfOriginBoundFindBox.TabIndex = 12;
			// 
			// JI_RH_NKCommodity_CodeBoundFindBox
			// 
			this.JI_RH_NKCommodity_CodeBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 219, true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.TabIndex = 14;
			// 
			// JI_LinePriceBoundCurrencyControl
			// 
			this.JI_LinePriceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 127, true);
			this.JI_LinePriceBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_LinePriceBoundCurrencyControl.TabIndex = 8;
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Visible = false;
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// CusContainerInvoiceLineGrid
			// 
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 274, true);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 51, true);
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 533, true);
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// NewLineDetailsTabPage
			// 
			this.NewLineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 293, true);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 533, true);
			// 
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GeneralCountryInvoiceLineUserControl|9c49754a-69df-439b-b746-2caabc961e88", "Insurance", "Insurance for current line item");
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 104, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 6;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 150, true);
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.VolumeCalcDropEdit.TabIndex = 11;
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 150, true);
			this.JI_WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.JI_WeightCalcDropEdit.TabIndex = 10;
			// 
			// InvoiceQuantityCalcDropEdit
			// 
			this.InvoiceQuantityCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+CustomsUQList";
			this.InvoiceQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 81, true);
			this.InvoiceQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.InvoiceQuantityCalcDropEdit.TabIndex = 4;
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 58, true);
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 20, true);
			this.JI_DescriptionBoundTextBox.TabIndex = 3;
			// 
			// Splitter
			// 
			this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			// 
			// JI_ProcedureFindBox
			// 
			this.JI_ProcedureFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_ProcedureFindBox, "FilteredInvoiceLines.JI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Procedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.Procedures)));
			this.JI_ProcedureFindBox.BindToList = "FilteredInvoiceLines.Lookups+Procedures";
			this.JI_ProcedureFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GeneralCountryInvoiceLineUserControl|906122A0-CEAF-4EE2-A570-B103DAA07F11", "Procedure Code");
			this.JI_ProcedureFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 12, true);
			this.JI_ProcedureFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
			this.JI_ProcedureFindBox.Name = "JI_ProcedureFindBox";
			this.JI_ProcedureFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JI_ProcedureFindBox.ParentType = null;
			this.JI_ProcedureFindBox.PreBoundMaxLength = 3;
			this.JI_ProcedureFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 20, true);
			this.JI_ProcedureFindBox.TabIndex = 0;
			// 
			// ProductCodeFindBox
			// 
			this.ProductCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductCodeFindBox, "FilteredInvoiceLines.JI_PartNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PartNo)));
			this.ProductCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 35, true);
			this.ProductCodeFindBox.Name = "ProductCodeFindBox";
			this.ProductCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ProductCodeFindBox.ParentType = null;
			this.ProductCodeFindBox.PreBoundMaxLength = 35;
			this.ProductCodeFindBox.ShowDescriptionBox = false;
			this.ProductCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.ProductCodeFindBox.TabIndex = 1;
			// 
			// JI_TariffFindBox
			// 
			this.JI_TariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_TariffFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.JI_TariffFindBox.ErrorForUnsupportedCountry = null;
			this.JI_TariffFindBox.GetEffectiveDate = null;
			this.JI_TariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 35, true);
			this.JI_TariffFindBox.Name = "JI_TariffFindBox";
			this.JI_TariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.JI_TariffFindBox.ParentType = null;
			this.JI_TariffFindBox.PreBoundMaxLength = 10;
			this.JI_TariffFindBox.SelectNomenclatureModes = null;
			this.JI_TariffFindBox.ShowDescriptionBox = false;
			this.JI_TariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.JI_TariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.JI_TariffFindBox.TabIndex = 2;
			this.JI_TariffFindBox.TariffType = null;
			// 
			// CustomsSecondQuantityCalcDropEdit
			// 
			this.CustomsSecondQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsSecondQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsSecondQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsSecondUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsUQList)));
			this.CustomsSecondQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsSecondQuantity";
			this.CustomsSecondQuantityCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+CustomsUQList";
			this.CustomsSecondQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_CustomsSecondUnitQty";
			this.CustomsSecondQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GeneralCountryInvoiceLineUserControl|6D7DF1F0-8954-4429-A87B-6B1BC9A6C9B0", "Additional Qty 1");
			this.CustomsSecondQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 81, true);
			this.CustomsSecondQuantityCalcDropEdit.Name = "CustomsSecondQuantityCalcDropEdit";
			this.CustomsSecondQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.CustomsSecondQuantityCalcDropEdit.TabIndex = 5;
			this.CustomsSecondQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CustomsThirdQuantityCalcDropEdit
			// 
			this.CustomsThirdQuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsThirdQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsThirdUnitQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsUQList)));
			this.CustomsThirdQuantityCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_CustomsThirdQuantity";
			this.CustomsThirdQuantityCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+CustomsUQList";
			this.CustomsThirdQuantityCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_CustomsThirdUnitQty";
			this.CustomsThirdQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GeneralCountryInvoiceLineUserControl|08554B6C-1E43-4E43-BBCE-C7EB02B1387C", "Additional Qty 2");
			this.CustomsThirdQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 104, true);
			this.CustomsThirdQuantityCalcDropEdit.Name = "CustomsThirdQuantityCalcDropEdit";
			this.CustomsThirdQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.CustomsThirdQuantityCalcDropEdit.TabIndex = 7;
			this.CustomsThirdQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// TaxOrFeeDropEdit
			// 
			this.TaxOrFeeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TaxOrFeeDropEdit, "FilteredInvoiceLines.JI_ZZF_NKTaxType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ZZF_NKTaxType)));
			this.TaxOrFeeDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GeneralCountryInvoiceLineUserControl|933DE95E-E80E-4493-8583-E4D47987DB2D", "Tax Type Code");
			this.TaxOrFeeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 127, true);
			this.TaxOrFeeDropEdit.Name = "TaxOrFeeDropEdit";
			this.TaxOrFeeDropEdit.PreBoundMaxLength = 4;
			this.TaxOrFeeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.TaxOrFeeDropEdit.TabIndex = 9;
			// 
			// PreferenceDropEdit
			// 
			this.PreferenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferenceDropEdit, "FilteredInvoiceLines.JI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PrimaryPreference)));
			this.PreferenceDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GeneralCountryInvoiceLineUserControl|BB29ADDB-4792-4C8F-8E25-BD08DE9E308C", "Preference");
			this.PreferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 196, true);
			this.PreferenceDropEdit.Name = "PreferenceDropEdit";
			this.PreferenceDropEdit.ShowDescriptionBox = false;
			this.PreferenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.PreferenceDropEdit.TabIndex = 13;
			// 
			// GeneralCountryInvoiceLineUserControl
			// 
			this.Name = "GeneralCountryInvoiceLineUserControl";
			this.InvoiceLinesSummaryGroupBox.ResumeLayout(false);
			this.InvoiceLinesSummaryGroupBox.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.LineDetailTabControl.ResumeLayout(false);
			this.LineDetailTabControl.PerformLayout();
			this.InvoiceDetailsGroupBox.ResumeLayout(false);
			this.InvoiceDetailsGroupBox.PerformLayout();
			this.JI_CountryOfOriginBoundFindBox.ResumeLayout(true);
			this.JI_CountryOfOriginBoundFindBox.PerformLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.ResumeLayout(true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.PerformLayout();
			this.JI_LinePriceBoundCurrencyControl.ResumeLayout(true);
			this.JI_LinePriceBoundCurrencyControl.PerformLayout();
			this.LineChargesTabPage.ResumeLayout(false);
			this.LineChargesTabPage.PerformLayout();
			this.CurrentInvoicePanel.ResumeLayout(false);
			this.CurrentInvoicePanel.PerformLayout();
			this.LineSummaryPanel.ResumeLayout(false);
			this.LineSummaryPanel.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).EndInit();
			this.CusContainerInvoiceLineGrid.ResumeLayout(false);
			this.CusContainerInvoiceLineGrid.PerformLayout();
			this.LineDetailsTabPage.ResumeLayout(false);
			this.LineDetailsTabPage.PerformLayout();
			this.NewLineDetailsTabPage.ResumeLayout(false);
			this.NewLineDetailsTabPage.PerformLayout();
			this.InvoiceLineDetailsUserControl.ResumeLayout(true);
			this.InvoiceLineDetailsUserControl.PerformLayout();
			this.ClassificationPanel.ResumeLayout(false);
			this.ClassificationPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsInvoiceLinesBoundGrid)).EndInit();
			this.CustomsInvoiceLinesBoundGrid.ResumeLayout(false);
			this.CustomsInvoiceLinesBoundGrid.PerformLayout();
			this.JI_Calc_CIFConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_FreightConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_FOBConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_FOBConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_GSTConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_DutyConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.PerformLayout();
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.ResumeLayout(true);
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.PerformLayout();
			this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsQuantityCalcDropEdit.PerformLayout();
			this.VolumeCalcDropEdit.ResumeLayout(true);
			this.VolumeCalcDropEdit.PerformLayout();
			this.JI_WeightCalcDropEdit.ResumeLayout(true);
			this.JI_WeightCalcDropEdit.PerformLayout();
			this.InvoiceQuantityCalcDropEdit.ResumeLayout(true);
			this.InvoiceQuantityCalcDropEdit.PerformLayout();
			this.JI_DescriptionBoundTextBox.ResumeLayout(true);
			this.JI_DescriptionBoundTextBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JI_ProcedureFindBox.ResumeLayout(true);
			this.JI_ProcedureFindBox.PerformLayout();
			this.ProductCodeFindBox.ResumeLayout(true);
			this.ProductCodeFindBox.PerformLayout();
			this.JI_TariffFindBox.ResumeLayout(true);
			this.JI_TariffFindBox.PerformLayout();
			this.CustomsSecondQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsSecondQuantityCalcDropEdit.PerformLayout();
			this.CustomsThirdQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsThirdQuantityCalcDropEdit.PerformLayout();
			this.TaxOrFeeDropEdit.ResumeLayout(true);
			this.TaxOrFeeDropEdit.PerformLayout();
			this.PreferenceDropEdit.ResumeLayout(true);
			this.PreferenceDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.Customs.Universal.GUI.TariffFindBox JI_TariffFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox JI_ProcedureFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox ProductCodeFindBox;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit CustomsSecondQuantityCalcDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZCalcDropEdit CustomsThirdQuantityCalcDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit TaxOrFeeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit PreferenceDropEdit;
	}
}
