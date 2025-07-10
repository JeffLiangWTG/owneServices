using System.ComponentModel;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public partial class CustomsInvoiceLineUserControl : DeclarationInvoiceLineUserControl
	{
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit PreferenceDropEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit SupplementaryCalcDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox DutiesLeviesGroupBox;
		protected internal Enterprise.ZArchitecture.ZCalcEdit CountervailingDutyCalcEdit;
		protected internal Enterprise.ZArchitecture.ZCalcEdit DutyCreditCalcEdit;
		protected internal Enterprise.ZArchitecture.ZCalcEdit AntiDumpingDutyCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryOfExportCodeFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox ConcessionCodeFindBox;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MiscTabPage;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SpecialClassificationsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit ParentLineDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl CodeInfosTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage PermitCodesPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage ProhibitedCodesTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage OtherInfosTabPage;
		protected Enterprise.ZArchitecture.ZGrid PermitCodesGrid;
		protected Enterprise.ZArchitecture.ZGrid ProhibitedCodesGrid;
		protected Enterprise.ZArchitecture.ZGrid OtherInfosGrid;
		protected Enterprise.ZArchitecture.ZTextBox DutyRateTextBox;
		protected internal Enterprise.ZArchitecture.ZCalcEdit DepositRefundCalcEdit;
		protected internal Enterprise.ZArchitecture.ZCalcEdit GSTCreditCalcEdit;
		protected internal Enterprise.ZArchitecture.ZCalcEdit ExciseDutyCreditCalcEdit;
		protected internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl LevyCurrencyControl;
		protected internal Enterprise.ZArchitecture.ZCalcEdit LevyCreditCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox ZeroRatedDutyRateTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ZeroRatedGroupBox;
		private ZDropEdit ZeroRatedGSTDropEdit;
		private ZDropEdit ZeroRatedLeviesDropEdit;
		private ZDropEdit ZeroRatedExciseDropEdit;
		private ZDropEdit ZeroRatedDutyDropEdit;
		internal NZCClassFindBox JI_PartsOfClassificationNZcClassFindBox;
		internal NZCClassFindBox NZCClassificationFindBox;
		public ZCalcDropEdit JI_NetWeightCalcDropEdit;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo16 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo17 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PreferenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupplementaryCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CountryOfExportCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ConcessionCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DutiesLeviesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GSTCreditCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DepositRefundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CountervailingDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LevyCreditCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutyCreditCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AntiDumpingDutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ExciseDutyCreditCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MiscTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ZeroRatedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ZeroRatedGSTDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ZeroRatedLeviesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ZeroRatedExciseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ZeroRatedDutyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ZeroRatedDutyRateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SpecialClassificationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ParentLineDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.JI_PartsOfClassificationNZcClassFindBox = new Enterprise.Customs.NZ.GUI.NZCClassFindBox();
			this.CodeInfosTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.PermitCodesPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PermitCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProhibitedCodesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ProhibitedCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.OtherInfosTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OtherInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DutyRateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LevyCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.NZCClassificationFindBox = new Enterprise.Customs.NZ.GUI.NZCClassFindBox();
			this.PreferentialCountryGroupDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TSWTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CommodityDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManufacturerAddressAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.LotNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TreatmentProviderFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.DateMarkingDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ProducerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.GrowerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.IntendedUseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IntendedUseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IntendedUseTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TemperatureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TempDetailsToBeSentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MaxTempCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MinTempCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StorageTempCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ProductCharacteristicGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GeneticallyModifiedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UsedGoodsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConstituentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityConstituentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ProductsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityProductsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ClassificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DangerousGoodsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HazMatGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zGuidFindBox2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ProductNameGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TradeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegisteredNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CommonNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BrandNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DangerousGoodsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.FlashPointTempCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HazardousMaterialContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.JI_NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.JI_EffectiveOriginRegionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_OriginRegionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackagingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ItemPackagingGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LevyCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TariffCodeFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.PartsOfClassificationFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.ConcessionCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceLinesSummaryGroupBox.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.LineDetailTabControl.SuspendLayout();
			this.InvoiceDetailsGroupBox.SuspendLayout();
			this.JI_CountryOfOriginBoundFindBox.SuspendLayout();
			this.JI_RH_NKCommodity_CodeBoundFindBox.SuspendLayout();
			this.JI_LinePriceBoundCurrencyControl.SuspendLayout();
			this.ClassificationDetailsGroupBox.SuspendLayout();
			this.LineChargesTabPage.SuspendLayout();
			this.CurrentInvoicePanel.SuspendLayout();
			this.LineSummaryPanel.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainerInvoiceLineGrid)).BeginInit();
			this.CusContainerInvoiceLineGrid.SuspendLayout();
			this.LineDetailsTabPage.SuspendLayout();
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
			this.PreferenceDropEdit.SuspendLayout();
			this.SupplementaryCalcDropEdit.SuspendLayout();
			this.CountryOfExportCodeFindBox.SuspendLayout();
			this.ConcessionCodeFindBox.SuspendLayout();
			this.DutiesLeviesGroupBox.SuspendLayout();
			this.MiscTabPage.SuspendLayout();
			this.ZeroRatedGroupBox.SuspendLayout();
			this.ZeroRatedGSTDropEdit.SuspendLayout();
			this.ZeroRatedLeviesDropEdit.SuspendLayout();
			this.ZeroRatedExciseDropEdit.SuspendLayout();
			this.ZeroRatedDutyDropEdit.SuspendLayout();
			this.SpecialClassificationsGroupBox.SuspendLayout();
			this.ParentLineDropEdit.SuspendLayout();
			this.JI_PartsOfClassificationNZcClassFindBox.SuspendLayout();
			this.CodeInfosTabControl.SuspendLayout();
			this.PermitCodesPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitCodesGrid)).BeginInit();
			this.PermitCodesGrid.SuspendLayout();
			this.ProhibitedCodesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProhibitedCodesGrid)).BeginInit();
			this.ProhibitedCodesGrid.SuspendLayout();
			this.OtherInfosTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherInfosGrid)).BeginInit();
			this.OtherInfosGrid.SuspendLayout();
			this.LevyCurrencyControl.SuspendLayout();
			this.NZCClassificationFindBox.SuspendLayout();
			this.PreferentialCountryGroupDropEdit.SuspendLayout();
			this.TSWTabPage.SuspendLayout();
			this.CommodityDetailsGroupBox.SuspendLayout();
			this.ManufacturerAddressAddressControl.SuspendLayout();
			this.TreatmentProviderFindBox.SuspendLayout();
			this.DateMarkingDateEdit.SuspendLayout();
			this.ProducerAddressControl.SuspendLayout();
			this.GrowerAddressControl.SuspendLayout();
			this.IntendedUseGroupBox.SuspendLayout();
			this.IntendedUseCodeDropEdit.SuspendLayout();
			this.TemperatureGroupBox.SuspendLayout();
			this.ProductCharacteristicGroupBox.SuspendLayout();
			this.ConstituentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommodityConstituentGrid)).BeginInit();
			this.CommodityConstituentGrid.SuspendLayout();
			this.ProductsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommodityProductsGrid)).BeginInit();
			this.CommodityProductsGrid.SuspendLayout();
			this.ClassificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommodityLinesGrid)).BeginInit();
			this.CommodityLinesGrid.SuspendLayout();
			this.DangerousGoodsGroupBox.SuspendLayout();
			this.zGuidFindBox2.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.ProductNameGroupBox.SuspendLayout();
			this.DangerousGoodsTabPage.SuspendLayout();
			this.DGGuidFindBox.SuspendLayout();
			this.HazardousMaterialContactGuidFindBox.SuspendLayout();
			this.JI_NetWeightCalcDropEdit.SuspendLayout();
			this.PackagingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemPackagingGrid)).BeginInit();
			this.ItemPackagingGrid.SuspendLayout();
			this.LevyCodeDropEdit.SuspendLayout();
			this.TariffCodeFindBox.SuspendLayout();
			this.PartsOfClassificationFindBox.SuspendLayout();
			this.ConcessionCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// InvoiceLinesSummaryGroupBox
			// 
			this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(754, 0, true);
			this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 381, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 115, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 385, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 385, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 496, true);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Controls.Add(this.TSWTabPage);
			this.LineDetailTabControl.Controls.Add(this.MiscTabPage);
			this.LineDetailTabControl.Controls.Add(this.DangerousGoodsTabPage);
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 378, true);
			this.LineDetailTabControl.Tag = "";
			this.LineDetailTabControl.Controls.SetChildIndex(this.DangerousGoodsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.MiscTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.TSWTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_OriginRegionTextBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_EffectiveOriginRegionTextBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.CountryOfExportCodeFindBox);
			this.InvoiceDetailsGroupBox.Controls.Add(this.JI_NetWeightCalcDropEdit);
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(740, 90, true);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_NetWeightCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.CountryOfExportCodeFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_RH_NKCommodity_CodeBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_EffectiveOriginRegionTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_OriginRegionTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_CountryOfOriginBoundFindBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.VolumeCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_LinePriceBoundCurrencyControl, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_DescriptionBoundTextBox, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.InvoiceQuantityCalcDropEdit, 0);
			this.InvoiceDetailsGroupBox.Controls.SetChildIndex(this.JI_WeightCalcDropEdit, 0);
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.BindToForDescription = "FilteredInvoiceLines.CountryOfOriginDescription";
			this.JI_CountryOfOriginBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 64, true);
			this.JI_CountryOfOriginBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 17, true);
			this.JI_CountryOfOriginBoundFindBox.TabIndex = 7;
			// 
			// JI_RH_NKCommodity_CodeBoundFindBox
			// 
			this.JI_RH_NKCommodity_CodeBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(672, 40, true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.TabIndex = 6;
			// 
			// JI_LinePriceBoundCurrencyControl
			// 
			this.JI_LinePriceBoundCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 40, true);
			this.JI_LinePriceBoundCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.JI_LinePriceBoundCurrencyControl.TabIndex = 5;
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)));
			this.ClassificationDetailsGroupBox.Controls.Add(this.ConcessionCodeDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.TariffCodeFindBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.PreferentialCountryGroupDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.ConcessionCodeFindBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.DutyRateTextBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.NZCClassificationFindBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.PreferenceDropEdit);
			this.ClassificationDetailsGroupBox.Controls.Add(this.SupplementaryCalcDropEdit);
			this.ClassificationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.ClassificationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 158, true);
			this.ClassificationDetailsGroupBox.Text = "Classification Details";
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SupplementaryCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.PreferenceDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.NZCClassificationFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.DutyRateTextBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.ConcessionCodeFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.PreferentialCountryGroupDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.TariffCodeFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.ConcessionCodeDropEdit, 0);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 356, true);
			// 
			// CurrentInvoicePanel
			// 
			this.CurrentInvoicePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 96, true);
			// 
			// LineSummaryPanel
			// 
			this.LineSummaryPanel.Controls.Add(this.LevyCurrencyControl);
			this.LineSummaryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 112, true);
			this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 266, true);
			this.LineSummaryPanel.Controls.SetChildIndex(this.oLabel8, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FreightConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_InsuranceConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIFConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.LevyCurrencyControl, 0);
			// 
			// PendingApportionmentLabel
			// 
			this.PendingApportionmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 366, true);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 356, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 356, true);
			// 
			// CusContainerInvoiceLineGrid
			// 
			this.CusContainerInvoiceLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 339, true);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 496, true);
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.Controls.Add(this.PackagingGroupBox);
			this.LineDetailsTabPage.Controls.Add(this.DutiesLeviesGroupBox);
			this.LineDetailsTabPage.Controls.Add(this.CodeInfosTabControl);
			this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 356, true);
			this.LineDetailsTabPage.Controls.SetChildIndex(this.ClassificationPanel, 0);
			this.LineDetailsTabPage.Controls.SetChildIndex(this.InvoiceDetailsGroupBox, 0);
			this.LineDetailsTabPage.Controls.SetChildIndex(this.CodeInfosTabControl, 0);
			this.LineDetailsTabPage.Controls.SetChildIndex(this.DutiesLeviesGroupBox, 0);
			this.LineDetailsTabPage.Controls.SetChildIndex(this.PackagingGroupBox, 0);
			// 
			// ClassificationPanel
			// 
			this.ClassificationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 96, true);
			this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 164, true);
			this.ClassificationPanel.TabIndex = 1;
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			zTextBoxColumnStyleInfo9.Caption = "Merged Line#";
			zTextBoxColumnStyleInfo9.ColumnName = "MergedLineNumber";
			zCodeFindBoxColumnStyleInfo2.BindToList = "Lookups.CountryList";
			zCodeFindBoxColumnStyleInfo2.Caption = "Export";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JI_RN_NKCountryOfExport";
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo2.ToolTip = "Country/Region where goods are exported from";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo6.BindToList = "Lookups.QualifiesForPreferentialDutyList";
			zDropEditColumnStyleInfo6.Caption = "Pref";
			zDropEditColumnStyleInfo6.ColumnName = "JI_QualifiesForPreferentialDuty";
			zDropEditColumnStyleInfo6.ToolTip = "Preference";
			zDropEditColumnStyleInfo7.Caption = "Pref Group";
			zDropEditColumnStyleInfo7.ColumnName = "JI_PreferentialCountryGroup";
			zDropEditColumnStyleInfo7.ToolTip = "Preference Group code used to calculate the rate of duty applicable.";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Supp. Qty";
			zCalcEditColumnStyleInfo2.ColumnName = "JI_SupplementaryQty";
			zCalcEditColumnStyleInfo2.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|568eff71-fe82-48d5-8534-358330972614", "Supp");
			zCalcEditColumnStyleInfo2.ToolTip = "Supplementary quantity";
			zTextBoxColumnStyleInfo10.Caption = "Supp. UQ";
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "JI_SupplementaryUQ";
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|568eff71-fe82-48d5-8534-358330972614", "Supp");
			zTextBoxColumnStyleInfo10.ToolTip = "Supplementary unit for the chosen tariff";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = "Countervailing Duty";
			zCalcEditColumnStyleInfo3.ColumnName = "JI_CountervailingDutyAmount";
			zCalcEditColumnStyleInfo3.ToolTip = "Countervailing Duty in NZD";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = "Anti Dumping Duty";
			zCalcEditColumnStyleInfo4.ColumnName = "JI_AntiDumpingDutyAmount";
			zCalcEditColumnStyleInfo4.ToolTip = "Anti Dumping Duty in NZD";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = "Duty Credit";
			zCalcEditColumnStyleInfo5.ColumnName = "JI_DutyCreditAmount";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.Caption = "GST Credit";
			zCalcEditColumnStyleInfo6.ColumnName = "JI_GSTCreditAmount";
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.Caption = "Levy Credit";
			zCalcEditColumnStyleInfo7.ColumnName = "JI_LevyCreditAmount";
			zCalcEditColumnStyleInfo7.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|567776BB-7089-48B8-A14D-3DC7A4BAD389", "Levy Credit Amount");
			zDropEditColumnStyleInfo15.Caption = "Levy Code";
			zDropEditColumnStyleInfo15.ColumnName = "JI_LevyCreditAmountCode";
			zDropEditColumnStyleInfo15.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|567776BB-7089-48B8-A14D-3DC7A4BAD389", "Levy Credit Amount");
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.Caption = "Deposit Refund";
			zCalcEditColumnStyleInfo8.ColumnName = "JI_DepositRefundAmount";
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.Caption = "Excise Duty Credit";
			zCalcEditColumnStyleInfo9.ColumnName = "JI_ExciseDutyCreditAmount";
			zDropEditColumnStyleInfo8.BindToList = "Lookups.YesNoList";
			zDropEditColumnStyleInfo8.ColumnName = "JI_IsZeroRatedDuty";
			zDropEditColumnStyleInfo9.BindToList = "Lookups.YesNoList";
			zDropEditColumnStyleInfo9.ColumnName = "JI_IsZeroRatedExcise";
			zDropEditColumnStyleInfo10.BindToList = "Lookups.YesNoList";
			zDropEditColumnStyleInfo10.ColumnName = "JI_IsZeroRatedGST";
			zDropEditColumnStyleInfo11.BindToList = "Lookups.YesNoList";
			zDropEditColumnStyleInfo11.ColumnName = "JI_IsZeroRatedLevies";
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.ColumnName = "NumberOfPackages1";
			zCalcEditColumnStyleInfo14.Decimals = 0;
			zCalcEditColumnStyleInfo14.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|1F92B4C8-4103-4CCF-99BA-489EA4B273B1", "Packages");
			zCalcEditColumnStyleInfo14.IsMandatory = true;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo14.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|1F92B4C8-4103-4CCF-99BA-489EA4B273B1", "Packages");
			zDropEditColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo14.ColumnName = "Packages1UQ";
			zDropEditColumnStyleInfo14.IsMandatory = true;
			zDropEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.ColumnName = "PackagesVolume1";
			zCalcEditColumnStyleInfo15.Decimals = 3;
			zCalcEditColumnStyleInfo15.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|9A23B832-1B44-4E03-A17C-8C975F890CAA", "Pkg. Volume");
			zCalcEditColumnStyleInfo15.IsMandatory = true;
			zCalcEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo17.ColumnName = "PackageVolume1UQ";
			zTextBoxColumnStyleInfo17.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|9A23B832-1B44-4E03-A17C-8C975F890CAA", "Pkg. Volume");
			zTextBoxColumnStyleInfo17.IsMandatory = true;
			zTextBoxColumnStyleInfo17.IsReadOnly = true;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo18.ColumnName = "PackagingMarks1";
			zTextBoxColumnStyleInfo18.IsMandatory = true;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo21.ColumnName = "PackagingMaterial1";
			zTextBoxColumnStyleInfo21.IsVisible = false;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo17.ColumnName = JobComInvoiceLine.Schema.JI_IntendedUseCode;
			zDropEditColumnStyleInfo17.IsVisible = false;
			zDropEditColumnStyleInfo17.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo17.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|05A4B883-B026-410E-8878-0EA71D3378D1", "Intended Use");
			zDropEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo22.ColumnName = JobComInvoiceLine.Schema.JI_IntendedUse;
			zTextBoxColumnStyleInfo22.IsVisible = false;
			zTextBoxColumnStyleInfo22.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo22.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|05A4B883-B026-410E-8878-0EA71D3378D1", "Intended Use");
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo23.ColumnName = JobComInvoiceLine.Schema.JI_OriginRegion;
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo15);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo14);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo17);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 496, true);
			// 
			// JI_Calc_GSTConvertToLocalCurrencyControl
			// 
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 165, true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 15;
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 108, true);
			this.CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 4;
			this.CustomsQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("7f4e2cf2-8e09-48b2-bc67-51b0c49e82a0", "Vol", "", "");
			this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 16, true);
			this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 17, true);
			this.VolumeCalcDropEdit.TabIndex = 3;
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("6ad97722-e4d0-43f4-8745-b0cc053ae8f7", "Gross Wgt", "", "");
			this.JI_WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 16, true);
			this.JI_WeightCalcDropEdit.TabIndex = 1;
			// 
			// InvoiceQuantityCalcDropEdit
			// 
			this.InvoiceQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 16, true);
			this.InvoiceQuantityCalcDropEdit.TabIndex = 0;
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 40, true);
			this.JI_DescriptionBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 21, true);
			this.JI_DescriptionBoundTextBox.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.JobDeclaration);
			// 
			// PreferenceDropEdit
			// 
			this.PreferenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferenceDropEdit, "FilteredInvoiceLines.JI_QualifiesForPreferentialDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_QualifiesForPreferentialDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.QualifiesForPreferentialDutyList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PrefDutyDescription)));
			this.PreferenceDropEdit.BindToForDescription = "FilteredInvoiceLines.PrefDutyDescription";
			this.PreferenceDropEdit.BindToList = "FilteredInvoiceLines.Lookups+QualifiesForPreferentialDutyList";
			this.PreferenceDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|e1a16961-0ff4-4d2c-a101-668cf1f9ad64", "Pref Duty", "Preferential Duty Indicator", "Indicator to show whether this line qualifies for a Country or Region specific Preferential Duty Rate.");
			this.PreferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 62, true);
			this.PreferenceDropEdit.Name = "PreferenceDropEdit";
			this.PreferenceDropEdit.PreBoundMaxLength = 1;
			this.PreferenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.PreferenceDropEdit.TabIndex = 2;
			// 
			// SupplementaryCalcDropEdit
			// 
			this.SupplementaryCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_SupplementaryQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_SupplementaryUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.SupplementaryUQList)));
			this.SupplementaryCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_SupplementaryQty";
			this.SupplementaryCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+SupplementaryUQList";
			this.SupplementaryCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_SupplementaryUQ";
			this.SupplementaryCalcDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|9e496e1c-2b71-4e2c-9e5a-a63371de1c59", "Supp. Qty", "Supplementary Qty", "Supplementary Quantity required as per the Working Tariff of New Zealand.");
			this.SupplementaryCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 131, true);
			this.SupplementaryCalcDropEdit.Name = "SupplementaryCalcDropEdit";
			this.SupplementaryCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.SupplementaryCalcDropEdit.TabIndex = 5;
			this.SupplementaryCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// CountryOfExportCodeFindBox
			// 
			this.CountryOfExportCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfExportCodeFindBox, "FilteredInvoiceLines.JI_RN_NKCountryOfExport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RN_NKCountryOfExport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CountryList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CountryOfExportDescription)));
			this.CountryOfExportCodeFindBox.BindToForDescription = "FilteredInvoiceLines.CountryOfExportDescription";
			this.CountryOfExportCodeFindBox.BindToList = "FilteredInvoiceLines.Lookups+CountryList";
			this.CountryOfExportCodeFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|1d4855a0-d7f9-4f24-ac28-ef8a66e62f97", "Export", "Country/Region of Export", "Goods Country/Region of Export - Defaults from Invoice Header if not entered here.");
			this.CountryOfExportCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 64, true);
			this.CountryOfExportCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.CountryOfExportCodeFindBox.Name = "CountryOfExportCodeFindBox";
			this.CountryOfExportCodeFindBox.PreBoundMaxLength = 2;
			this.CountryOfExportCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 17, true);
			this.CountryOfExportCodeFindBox.TabIndex = 10;
			// 
			// ConcessionCodeFindBox
			// 
			this.ConcessionCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConcessionCodeFindBox, "FilteredInvoiceLines.JI_ConcessionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ConcessionCode)));
			this.ConcessionCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 39, true);
			this.ConcessionCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.ConcessionCodeFindBox.Name = "ConcessionCodeFindBox";
			this.ConcessionCodeFindBox.PreBoundMaxLength = 7;
			this.ConcessionCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 17, true);
			this.ConcessionCodeFindBox.TabIndex = 1;
			// 
			// DutiesLeviesGroupBox
			// 
			this.DutiesLeviesGroupBox.Controls.Add(this.GSTCreditCalcEdit);
			this.DutiesLeviesGroupBox.Controls.Add(this.DepositRefundCalcEdit);
			this.DutiesLeviesGroupBox.Controls.Add(this.CountervailingDutyCalcEdit);
			this.DutiesLeviesGroupBox.Controls.Add(this.LevyCodeDropEdit);
			this.DutiesLeviesGroupBox.Controls.Add(this.LevyCreditCalcEdit);
			this.DutiesLeviesGroupBox.Controls.Add(this.DutyCreditCalcEdit);
			this.DutiesLeviesGroupBox.Controls.Add(this.AntiDumpingDutyCalcEdit);
			this.DutiesLeviesGroupBox.Controls.Add(this.ExciseDutyCreditCalcEdit);
			this.DutiesLeviesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 266, true);
			this.DutiesLeviesGroupBox.Name = "DutiesLeviesGroupBox";
			this.DutiesLeviesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 90, true);
			this.DutiesLeviesGroupBox.TabIndex = 2;
			this.DutiesLeviesGroupBox.TabStop = false;
			this.DutiesLeviesGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("3be0e9d3-b579-4310-b70e-5c115d5286c6", "Duties && Levies in NZD");
			// 
			// GSTCreditCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GSTCreditCalcEdit, "FilteredInvoiceLines.JI_GSTCreditAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_GSTCreditAmount)));
			this.GSTCreditCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|84812003-be56-46bb-94d8-55eb90b43b0d", "GST Credit", "GST Credit Amount", "");
			this.GSTCreditCalcEdit.DecimalPlaces = 2;
			this.GSTCreditCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 40, true);
			this.GSTCreditCalcEdit.Name = "GSTCreditCalcEdit";
			this.GSTCreditCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.GSTCreditCalcEdit.TabIndex = 9;
			this.GSTCreditCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DepositRefundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DepositRefundCalcEdit, "FilteredInvoiceLines.JI_DepositRefundAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DepositRefundAmount)));
			this.DepositRefundCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|72e1f3e3-b13e-411a-8817-bba6c1eb8364", "Deposit Refund", "Deposit Refund Amount", "");
			this.DepositRefundCalcEdit.DecimalPlaces = 2;
			this.DepositRefundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 40, true);
			this.DepositRefundCalcEdit.Name = "DepositRefundCalcEdit";
			this.DepositRefundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.DepositRefundCalcEdit.TabIndex = 12;
			this.DepositRefundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CountervailingDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CountervailingDutyCalcEdit, "FilteredInvoiceLines.JI_CountervailingDutyAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CountervailingDutyAmount)));
			this.CountervailingDutyCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|59e1c6eb-bd2e-469b-b758-77ee7d859e5a", "Countervailing Duty", "Countervailing Duty", "");
			this.CountervailingDutyCalcEdit.DecimalPlaces = 2;
			this.CountervailingDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 40, true);
			this.CountervailingDutyCalcEdit.Name = "CountervailingDutyCalcEdit";
			this.CountervailingDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.CountervailingDutyCalcEdit.TabIndex = 10;
			this.CountervailingDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LevyCodeDropEdit
			// 
			this.LevyCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LevyCodeDropEdit, "FilteredInvoiceLines.JI_LevyCreditAmountCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_LevyCreditAmountCode)));
			this.LevyCodeDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("663e8066-73c5-4f0e-a352-d59bbfdc8254", "Levy Credit Amount");
			this.LevyCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 16, true);
			this.LevyCodeDropEdit.Name = "LevyCodeDropEdit";
			this.LevyCodeDropEdit.PreBoundMaxLength = 4;
			this.LevyCodeDropEdit.ShowDescriptionBox = false;
			this.LevyCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 17, true);
			this.LevyCodeDropEdit.TabIndex = 5;
			// 
			// LevyCreditCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LevyCreditCalcEdit, "FilteredInvoiceLines.JI_LevyCreditAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_LevyCreditAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LevyCreditCalcEdit, false);
			this.LevyCreditCalcEdit.DecimalPlaces = 2;
			this.LevyCreditCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 16, true);
			this.LevyCreditCalcEdit.Name = "LevyCreditCalcEdit";
			this.LevyCreditCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.LevyCreditCalcEdit.TabIndex = 6;
			this.LevyCreditCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DutyCreditCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyCreditCalcEdit, "FilteredInvoiceLines.JI_DutyCreditAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DutyCreditAmount)));
			this.DutyCreditCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|8746edbd-9750-4166-8408-391eabfb9b7e", "Duty Credit", "Duty Credit Amount", "");
			this.DutyCreditCalcEdit.DecimalPlaces = 2;
			this.DutyCreditCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 16, true);
			this.DutyCreditCalcEdit.Name = "DutyCreditCalcEdit";
			this.DutyCreditCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.DutyCreditCalcEdit.TabIndex = 1;
			this.DutyCreditCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AntiDumpingDutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AntiDumpingDutyCalcEdit, "FilteredInvoiceLines.JI_AntiDumpingDutyAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_AntiDumpingDutyAmount)));
			this.AntiDumpingDutyCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|d1de398f-f06f-4aeb-8a17-946654a7d02e", "Anti Dumping Duty", "Anti Dumping Duty Amount", "");
			this.AntiDumpingDutyCalcEdit.DecimalPlaces = 2;
			this.AntiDumpingDutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 16, true);
			this.AntiDumpingDutyCalcEdit.Name = "AntiDumpingDutyCalcEdit";
			this.AntiDumpingDutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.AntiDumpingDutyCalcEdit.TabIndex = 3;
			this.AntiDumpingDutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExciseDutyCreditCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExciseDutyCreditCalcEdit, "FilteredInvoiceLines.JI_ExciseDutyCreditAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ExciseDutyCreditAmount)));
			this.ExciseDutyCreditCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|488eb049-25eb-424c-a93d-aad4b40defa2", "Excise Duty Credit", "Excise Duty Credit Amount", "");
			this.ExciseDutyCreditCalcEdit.DecimalPlaces = 2;
			this.ExciseDutyCreditCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 16, true);
			this.ExciseDutyCreditCalcEdit.Name = "ExciseDutyCreditCalcEdit";
			this.ExciseDutyCreditCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 17, true);
			this.ExciseDutyCreditCalcEdit.TabIndex = 4;
			this.ExciseDutyCreditCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MiscTabPage
			// 
			this.MiscTabPage.Controls.Add(this.ZeroRatedGroupBox);
			this.MiscTabPage.Controls.Add(this.SpecialClassificationsGroupBox);
			this.MiscTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MiscTabPage.Name = "MiscTabPage";
			this.MiscTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 298, true);
			this.MiscTabPage.TabIndex = 4;
			this.MiscTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("ee64a79b-ea6c-4622-bf80-074cd564bd3e", "Misc.");
			// 
			// ZeroRatedGroupBox
			// 
			this.ZeroRatedGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ZeroRatedGroupBox.Controls.Add(this.ZeroRatedGSTDropEdit);
			this.ZeroRatedGroupBox.Controls.Add(this.ZeroRatedLeviesDropEdit);
			this.ZeroRatedGroupBox.Controls.Add(this.ZeroRatedExciseDropEdit);
			this.ZeroRatedGroupBox.Controls.Add(this.ZeroRatedDutyDropEdit);
			this.ZeroRatedGroupBox.Controls.Add(this.ZeroRatedDutyRateTextBox);
			this.ZeroRatedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 92, true);
			this.ZeroRatedGroupBox.Name = "ZeroRatedGroupBox";
			this.ZeroRatedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 133, true);
			this.ZeroRatedGroupBox.TabIndex = 1;
			this.ZeroRatedGroupBox.TabStop = false;
			this.ZeroRatedGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("e2facc9d-7be1-4490-a36f-d6dd08391a98", "Zero Rating Flags    (NB: Must Use Customs \"Override\" Indicator when Sending)");
			// 
			// ZeroRatedGSTDropEdit
			// 
			this.ZeroRatedGSTDropEdit.AllowDrop = true;
			this.ZeroRatedGSTDropEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.ZeroRatedGSTDropEdit, "FilteredInvoiceLines.JI_IsZeroRatedGST");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_IsZeroRatedGST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.YesNoList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZeroRatedGSTDescription)));
			this.ZeroRatedGSTDropEdit.BindToForDescription = "FilteredInvoiceLines.ZeroRatedGSTDescription";
			this.ZeroRatedGSTDropEdit.BindToList = "Lookups+YesNoList";
			this.ZeroRatedGSTDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|8a642037-198b-4bc3-b94e-a10d2f408009", "Force Zero Rated GST");
			this.ZeroRatedGSTDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 103, true);
			this.ZeroRatedGSTDropEdit.Name = "ZeroRatedGSTDropEdit";
			this.ZeroRatedGSTDropEdit.PreBoundMaxLength = 2;
			this.ZeroRatedGSTDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.ZeroRatedGSTDropEdit.TabIndex = 9;
			// 
			// ZeroRatedLeviesDropEdit
			// 
			this.ZeroRatedLeviesDropEdit.AllowDrop = true;
			this.ZeroRatedLeviesDropEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.ZeroRatedLeviesDropEdit, "FilteredInvoiceLines.JI_IsZeroRatedLevies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_IsZeroRatedLevies)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.YesNoList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZeroRatedLeviesDescription)));
			this.ZeroRatedLeviesDropEdit.BindToForDescription = "FilteredInvoiceLines.ZeroRatedLeviesDescription";
			this.ZeroRatedLeviesDropEdit.BindToList = "Lookups+YesNoList";
			this.ZeroRatedLeviesDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|7e3387d7-c501-486a-8f31-64e963b0c3e5", "Force Zero Rated Levies");
			this.ZeroRatedLeviesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 77, true);
			this.ZeroRatedLeviesDropEdit.Name = "ZeroRatedLeviesDropEdit";
			this.ZeroRatedLeviesDropEdit.PreBoundMaxLength = 2;
			this.ZeroRatedLeviesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.ZeroRatedLeviesDropEdit.TabIndex = 7;
			// 
			// ZeroRatedExciseDropEdit
			// 
			this.ZeroRatedExciseDropEdit.AllowDrop = true;
			this.ZeroRatedExciseDropEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.ZeroRatedExciseDropEdit, "FilteredInvoiceLines.JI_IsZeroRatedExcise");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_IsZeroRatedExcise)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.YesNoList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZeroRatedExciseDescription)));
			this.ZeroRatedExciseDropEdit.BindToForDescription = "FilteredInvoiceLines.ZeroRatedExciseDescription";
			this.ZeroRatedExciseDropEdit.BindToList = "Lookups+YesNoList";
			this.ZeroRatedExciseDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|edf1f05b-117a-4b18-8497-60957fa1d472", "Force Zero Rated Excise");
			this.ZeroRatedExciseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 51, true);
			this.ZeroRatedExciseDropEdit.Name = "ZeroRatedExciseDropEdit";
			this.ZeroRatedExciseDropEdit.PreBoundMaxLength = 2;
			this.ZeroRatedExciseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.ZeroRatedExciseDropEdit.TabIndex = 4;
			// 
			// ZeroRatedDutyDropEdit
			// 
			this.ZeroRatedDutyDropEdit.AllowDrop = true;
			this.ZeroRatedDutyDropEdit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.ZeroRatedDutyDropEdit, "FilteredInvoiceLines.JI_IsZeroRatedDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_IsZeroRatedDuty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).Lookups.YesNoList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ZeroRatedDutyDescription)));
			this.ZeroRatedDutyDropEdit.BindToForDescription = "FilteredInvoiceLines.ZeroRatedDutyDescription";
			this.ZeroRatedDutyDropEdit.BindToList = "Lookups+YesNoList";
			this.ZeroRatedDutyDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|92be0776-40b9-4202-b9e5-974f22760e70", "Force Zero Rated Duty");
			this.ZeroRatedDutyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 26, true);
			this.ZeroRatedDutyDropEdit.Name = "ZeroRatedDutyDropEdit";
			this.ZeroRatedDutyDropEdit.PreBoundMaxLength = 2;
			this.ZeroRatedDutyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.ZeroRatedDutyDropEdit.TabIndex = 1;
			// 
			// ZeroRatedDutyRateTextBox
			// 
			this.ZeroRatedDutyRateTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ZeroRatedDutyRateTextBox, "FilteredInvoiceLines.JI_DutyRateComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DutyRateComplete)));
			this.ZeroRatedDutyRateTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|0518c02a-9a3a-408c-8ab3-7483ca268862", "Duty Rate", "Resulting Duty Rate", "");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ZeroRatedDutyRateTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.ZeroRatedDutyRateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 47, true);
			this.ZeroRatedDutyRateTextBox.Multiline = true;
			this.ZeroRatedDutyRateTextBox.Name = "ZeroRatedDutyRateTextBox";
			this.ZeroRatedDutyRateTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ZeroRatedDutyRateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 43, true);
			this.ZeroRatedDutyRateTextBox.TabIndex = 5;
			this.ZeroRatedDutyRateTextBox.Text = "DUTY RATE";
			// 
			// SpecialClassificationsGroupBox
			// 
			this.SpecialClassificationsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.SpecialClassificationsGroupBox.Controls.Add(this.PartsOfClassificationFindBox);
			this.SpecialClassificationsGroupBox.Controls.Add(this.ParentLineDropEdit);
			this.SpecialClassificationsGroupBox.Controls.Add(this.JI_PartsOfClassificationNZcClassFindBox);
			this.SpecialClassificationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.SpecialClassificationsGroupBox.Name = "SpecialClassificationsGroupBox";
			this.SpecialClassificationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 80, true);
			this.SpecialClassificationsGroupBox.TabIndex = 0;
			this.SpecialClassificationsGroupBox.TabStop = false;
			this.SpecialClassificationsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("6ea7ec1b-7281-4f96-8df4-e67457512fa2", "Special Classification Details");
			// 
			// PartsOfClassificationFindBox
			// 
			this.PartsOfClassificationFindBox.AllowDrop = true;
			this.PartsOfClassificationFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PartsOfClassificationFindBox, "FilteredInvoiceLines.JI_PartsOfClassification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PartsOfClassification)));
			this.PartsOfClassificationFindBox.ErrorForUnsupportedCountry = null;
			this.PartsOfClassificationFindBox.GetEffectiveDate = null;
			this.PartsOfClassificationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 18, true);
			this.PartsOfClassificationFindBox.Name = "PartsOfClassificationFindBox";
			this.PartsOfClassificationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.PartsOfClassificationFindBox.ParentType = null;
			this.PartsOfClassificationFindBox.PreBoundMaxLength = 15;
			this.PartsOfClassificationFindBox.SelectNomenclatureModes = null;
			this.PartsOfClassificationFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.PartsOfClassificationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 17, true);
			this.PartsOfClassificationFindBox.TabIndex = 4;
			this.PartsOfClassificationFindBox.TariffType = null;
			// 
			// ParentLineDropEdit
			// 
			this.ParentLineDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParentLineDropEdit, "FilteredInvoiceLines.JI_ParentLineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ParentLineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ParentLines)));
			this.ParentLineDropEdit.BindToList = "FilteredInvoiceLines.ParentLines";
			this.ParentLineDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|2b4dc7b8-e333-49e3-a896-66453adba54c", "24 Dollar Merge Parent Line", "Parent Line No for 24$ Parts line", "Choose the parent line this line belongs to so that this line can be exempt from duty.");
			this.ParentLineDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 45, true);
			this.ParentLineDropEdit.Name = "ParentLineDropEdit";
			this.ParentLineDropEdit.ShowDescriptionBox = false;
			this.ParentLineDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ParentLineDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.ParentLineDropEdit.TabIndex = 3;
			// 
			// JI_PartsOfClassificationNZcClassFindBox
			// 
			this.JI_PartsOfClassificationNZcClassFindBox.AllowDrop = true;
			this.JI_PartsOfClassificationNZcClassFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.JI_PartsOfClassificationNZcClassFindBox, "FilteredInvoiceLines.JI_PartsOfClassification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PartsOfClassification)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.PartsOfClassificationList)));
			this.JI_PartsOfClassificationNZcClassFindBox.BindToList = "FilteredInvoiceLines.Lookups+PartsOfClassificationList";
			this.JI_PartsOfClassificationNZcClassFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 18, true);
			this.JI_PartsOfClassificationNZcClassFindBox.MaxTariffLength = 14;
			this.JI_PartsOfClassificationNZcClassFindBox.Name = "JI_PartsOfClassificationNZcClassFindBox";
			this.JI_PartsOfClassificationNZcClassFindBox.PreBoundMaxLength = 15;
			this.JI_PartsOfClassificationNZcClassFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 17, true);
			this.JI_PartsOfClassificationNZcClassFindBox.TabIndex = 1;
			// 
			// CodeInfosTabControl
			// 
			this.CodeInfosTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.CodeInfosTabControl.Controls.Add(this.PermitCodesPage);
			this.CodeInfosTabControl.Controls.Add(this.ProhibitedCodesTabPage);
			this.CodeInfosTabControl.Controls.Add(this.OtherInfosTabPage);
			this.CodeInfosTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 98, true);
			this.CodeInfosTabControl.Name = "CodeInfosTabControl";
			this.CodeInfosTabControl.SelectedIndex = 0;
			this.CodeInfosTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 162, true);
			this.CodeInfosTabControl.TabIndex = 3;
			// 
			// PermitCodesPage
			// 
			this.PermitCodesPage.Controls.Add(this.PermitCodesGrid);
			this.PermitCodesPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.PermitCodesPage.Name = "PermitCodesPage";
			this.PermitCodesPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 140, true);
			this.PermitCodesPage.TabIndex = 0;
			this.PermitCodesPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("f58f3eda-3df2-4874-b3e9-a4b33d9d8a71", "Permits (0)");
			// 
			// PermitCodesGrid
			// 
			this.PermitCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PermitCodesGrid, "FilteredInvoiceLines.PermitCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PermitCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PermitCodes)).SyncRoot)).ZO_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PermitCodes)).SyncRoot)).ZO_CodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PermitCodes)).SyncRoot)).ZO_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.PermitCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PermitCodes)).SyncRoot)).ZO_Description)));
			this.PermitCodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo4.BindToList = "ZO_CodeList";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "ZO_Code";
			zDropEditColumnStyleInfo4.IsMandatory = true;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo7.ColumnName = "ZO_Data";
			zTextBoxColumnStyleInfo7.IsMandatory = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo8.ColumnName = "ZO_Description";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PermitCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PermitCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PermitCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.PermitCodesGrid.CopySelectedRowsAllowed = true;
			this.PermitCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitCodesGrid.GridId = "241621db-201a-42b9-94ed-7b886e97f144";
			this.PermitCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PermitCodesGrid.LayoutKey = "PermitGrid";
			this.PermitCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PermitCodesGrid.Name = "PermitCodesGrid";
			this.PermitCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 140, true);
			this.PermitCodesGrid.TabIndex = 0;
			// 
			// ProhibitedCodesTabPage
			// 
			this.ProhibitedCodesTabPage.Controls.Add(this.ProhibitedCodesGrid);
			this.ProhibitedCodesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ProhibitedCodesTabPage.Name = "ProhibitedCodesTabPage";
			this.ProhibitedCodesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 140, true);
			this.ProhibitedCodesTabPage.TabIndex = 1;
			this.ProhibitedCodesTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("a08180b9-9d7c-4c74-a07f-dc7602bbb470", "Prohibited Codes (0)");
			// 
			// ProhibitedCodesGrid
			// 
			this.ProhibitedCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProhibitedCodesGrid, "FilteredInvoiceLines.ProhibitedCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ProhibitedCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ProhibitedCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ProhibitedCodes)).SyncRoot)).ZO_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.ProhibitedCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ProhibitedCodes)).SyncRoot)).ZO_CodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ProhibitedCode)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ProhibitedCodes)).SyncRoot)).ZO_Description)));
			this.ProhibitedCodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo5.BindToList = "ZO_CodeList";
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "ZO_Code";
			zDropEditColumnStyleInfo5.IsMandatory = true;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo19.ColumnName = "ZO_Description";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(190);
			this.ProhibitedCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ProhibitedCodesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.ProhibitedCodesGrid.CopySelectedRowsAllowed = true;
			this.ProhibitedCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProhibitedCodesGrid.GridId = "77158416-3868-458b-b170-d6dce66cde78";
			this.ProhibitedCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProhibitedCodesGrid.LayoutKey = "ProhibitGrid";
			this.ProhibitedCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProhibitedCodesGrid.Name = "ProhibitedCodesGrid";
			this.ProhibitedCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 140, true);
			this.ProhibitedCodesGrid.TabIndex = 1;
			// 
			// OtherInfosTabPage
			// 
			this.OtherInfosTabPage.Controls.Add(this.OtherInfosGrid);
			this.OtherInfosTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.OtherInfosTabPage.Name = "OtherInfosTabPage";
			this.OtherInfosTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 140, true);
			this.OtherInfosTabPage.TabIndex = 2;
			this.OtherInfosTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("1aa56876-8477-4190-8a65-a0d26607bf39", "Other Infos (0)");
			// 
			// OtherInfosGrid
			// 
			this.OtherInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OtherInfosGrid, "FilteredInvoiceLines.OtherInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).OtherInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.LineOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).OtherInfos)).SyncRoot)).ZO_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.LineOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).OtherInfos)).SyncRoot)).ZO_CodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.LineOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).OtherInfos)).SyncRoot)).ZO_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.LineOtherInfo)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).OtherInfos)).SyncRoot)).ZO_Description)));
			this.OtherInfosGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo16.BindToList = "ZO_CodeList";
			zDropEditColumnStyleInfo16.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo16.ColumnName = "ZO_Code";
			zDropEditColumnStyleInfo16.IsMandatory = true;
			zDropEditColumnStyleInfo16.ToolTip = "Preference";
			zDropEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo20.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo20.ColumnName = "ZO_Data";
			zTextBoxColumnStyleInfo20.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|568eff71-fe82-48d5-8534-358330972614", "Supp");
			zTextBoxColumnStyleInfo20.IsMandatory = true;
			zTextBoxColumnStyleInfo20.ToolTip = "Supplementary unit for the chosen tariff";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo11.ColumnName = "ZO_Description";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.OtherInfosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo16);
			this.OtherInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.OtherInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.OtherInfosGrid.CopySelectedRowsAllowed = true;
			this.OtherInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OtherInfosGrid.GridId = "8f0a9705-afab-45e9-ac63-201432832907";
			this.OtherInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OtherInfosGrid.LayoutKey = "OtherInfoGrid";
			this.OtherInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherInfosGrid.Name = "OtherInfosGrid";
			this.OtherInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 140, true);
			this.OtherInfosGrid.TabIndex = 0;
			// 
			// DutyRateTextBox
			// 
			this.BindingSource.SetBindingMember(this.DutyRateTextBox, "FilteredInvoiceLines.JI_DutyRateComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DutyRateComplete)));
			this.DutyRateTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|8c102154-55b3-4ce2-bfb0-ad67c0bb2275", "Duty Rate");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DutyRateTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.DutyRateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 83, true);
			this.DutyRateTextBox.Multiline = true;
			this.DutyRateTextBox.Name = "DutyRateTextBox";
			this.DutyRateTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DutyRateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 68, true);
			this.DutyRateTextBox.TabIndex = 6;
			this.DutyRateTextBox.Text = "DUTY RATE";
			// 
			// LevyCurrencyControl
			// 
			this.LevyCurrencyControl.AllowDrop = true;
			this.LevyCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_LevyValueInNZD";
			this.LevyCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.LevyCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.LevyCurrencyControl.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|74dd8fcc-ca57-445a-be70-9f4ca2d178c4", "Levy", "Levy Value In NZD", "");
			this.LevyCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 141, true);
			this.LevyCurrencyControl.Name = "LevyCurrencyControl";
			this.LevyCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.LevyCurrencyControl.TabIndex = 13;
			// 
			// NZCClassificationFindBox
			// 
			this.NZCClassificationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NZCClassificationFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.TariffList)));
			this.NZCClassificationFindBox.BindToList = "FilteredInvoiceLines.Lookups+TariffList";
			this.NZCClassificationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 17, true);
			this.NZCClassificationFindBox.MaxTariffLength = 14;
			this.NZCClassificationFindBox.Name = "NZCClassificationFindBox";
			this.NZCClassificationFindBox.PreBoundMaxLength = 15;
			this.NZCClassificationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 17, true);
			this.NZCClassificationFindBox.TabIndex = 0;
			// 
			// PreferentialCountryGroupDropEdit
			// 
			this.PreferentialCountryGroupDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreferentialCountryGroupDropEdit, "FilteredInvoiceLines.JI_PreferentialCountryGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PreferentialCountryGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.PreferentialCountryGroupCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PrefGroupDescription)));
			this.PreferentialCountryGroupDropEdit.BindToForDescription = "FilteredInvoiceLines.PrefGroupDescription";
			this.PreferentialCountryGroupDropEdit.BindToList = "FilteredInvoiceLines.Lookups+PreferentialCountryGroupCodeList";
			this.PreferentialCountryGroupDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|d7ade110-08c9-4a75-9c8c-d20fcb545711", "Pref Group", "Preferential Group", "Preference Group code used to calculate the rate of duty applicable.");
			this.PreferentialCountryGroupDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 85, true);
			this.PreferentialCountryGroupDropEdit.Name = "PreferentialCountryGroupDropEdit";
			this.PreferentialCountryGroupDropEdit.PreBoundMaxLength = 3;
			this.PreferentialCountryGroupDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.PreferentialCountryGroupDropEdit.TabIndex = 3;
			// 
			// TSWTabPage
			// 
			this.TSWTabPage.Controls.Add(this.CommodityDetailsGroupBox);
			this.TSWTabPage.Controls.Add(this.IntendedUseGroupBox);
			this.TSWTabPage.Controls.Add(this.TemperatureGroupBox);
			this.TSWTabPage.Controls.Add(this.ProductCharacteristicGroupBox);
			this.TSWTabPage.Controls.Add(this.ConstituentsGroupBox);
			this.TSWTabPage.Controls.Add(this.ProductsGroupBox);
			this.TSWTabPage.Controls.Add(this.ClassificationGroupBox);
			this.TSWTabPage.Controls.Add(this.DangerousGoodsGroupBox);
			this.TSWTabPage.Controls.Add(this.ProductNameGroupBox);
			this.TSWTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TSWTabPage.Name = "TSWTabPage";
			this.TSWTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TSWTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 351, true);
			this.TSWTabPage.TabIndex = 3;
			this.TSWTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("78dd388f-ffc3-48a7-a376-3bd112ee1209", "Additional Detail");
			// 
			// CommodityDetailsGroupBox
			// 
			this.CommodityDetailsGroupBox.Controls.Add(this.ManufacturerAddressAddressControl);
			this.CommodityDetailsGroupBox.Controls.Add(this.LotNumberTextBox);
			this.CommodityDetailsGroupBox.Controls.Add(this.TreatmentProviderFindBox);
			this.CommodityDetailsGroupBox.Controls.Add(this.DateMarkingDateEdit);
			this.CommodityDetailsGroupBox.Controls.Add(this.ProducerAddressControl);
			this.CommodityDetailsGroupBox.Controls.Add(this.GrowerAddressControl);
			this.CommodityDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 124, true);
			this.CommodityDetailsGroupBox.Name = "CommodityDetailsGroupBox";
			this.CommodityDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 102, true);
			this.CommodityDetailsGroupBox.TabIndex = 3;
			this.CommodityDetailsGroupBox.TabStop = false;
			this.CommodityDetailsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("40edef1f-ee6c-4aee-8299-04bba8a56bef", "Commodity Details");
			// 
			// ManufacturerAddressAddressControl
			// 
			this.ManufacturerAddressAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressAddressControl, "FilteredInvoiceLines.JI_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OA_ManufacturerAddress)));
			this.ManufacturerAddressAddressControl.BindToOrgList = "Lookups.Importers";
			this.ManufacturerAddressAddressControl.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("57ab3d0a-bfa2-49f4-841d-37b39a5661c4", "Manufacturer", "Must be transmitted to state the Manufacturer, if different to the Supplier.");
			this.ManufacturerAddressAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 23, true);
			this.ManufacturerAddressAddressControl.Name = "ManufacturerAddressAddressControl";
			this.ManufacturerAddressAddressControl.PopupCaption = "Manufacturer";
			this.ManufacturerAddressAddressControl.ShowAddress = false;
			this.ManufacturerAddressAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 17, true);
			this.ManufacturerAddressAddressControl.TabIndex = 24;
			// 
			// LotNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LotNumberTextBox, "FilteredInvoiceLines.JI_LotNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_LotNumber)));
			this.LotNumberTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("4379c128-8ce5-4e4a-8a19-ef16639af763", "Lot Number", "LIMITED to Tariff chapter 2 – 22. Where known state the Lot Number of the goods.");
			this.LotNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 23, true);
			this.LotNumberTextBox.Name = "LotNumberTextBox";
			this.LotNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.LotNumberTextBox.TabIndex = 21;
			// 
			// TreatmentProviderFindBox
			// 
			this.TreatmentProviderFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TreatmentProviderFindBox, "FilteredInvoiceLines.JI_OH_TreatmentProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OH_TreatmentProvider)));
			this.TreatmentProviderFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("101e6c61-3464-4eca-ab93-500a5ac6d6ae", "Trt. Prov.", "Trt. Provider", "Treatment Provider", "May be transmitted to state a preferred treatment provider for the goods. e.g. fumigator.");
			this.TreatmentProviderFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 75, true);
			this.TreatmentProviderFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.TreatmentProviderFindBox.Name = "TreatmentProviderFindBox";
			this.TreatmentProviderFindBox.PreBoundMaxLength = 12;
			this.TreatmentProviderFindBox.ShowDescriptionBox = false;
			this.TreatmentProviderFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.TreatmentProviderFindBox.TabIndex = 23;
			// 
			// DateMarkingDateEdit
			// 
			this.DateMarkingDateEdit.AllowDrop = true;
			this.DateMarkingDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateMarkingDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateMarkingDateEdit, "FilteredInvoiceLines.JI_DateMarking");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_DateMarking)));
			this.DateMarkingDateEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("226bd2f4-b6eb-4750-8edd-9e50ba4d9fe4", "Date Marking", "LIMITED to Tariff chapter 2 – 22. Where known state any Date Marking of the product. Date Marking could be either the “best before date” or the “use by date” of the product.");
			this.DateMarkingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 49, true);
			this.DateMarkingDateEdit.Name = "DateMarkingDateEdit";
			this.DateMarkingDateEdit.TabIndex = 22;
			// 
			// ProducerAddressControl
			// 
			this.ProducerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProducerAddressControl, "FilteredInvoiceLines.JI_OA_ProducerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OA_ProducerAddress)));
			this.ProducerAddressControl.BindToOrgList = "Lookups.Importers";
			this.ProducerAddressControl.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("5cdef820-5353-45ac-8413-4c3cfc94e9fa", "Producer", "Must be transmitted to specify the producer details for processed food of plant origin and processed food of animal origin within Tariff chapters 2 - 22. Must be transmitted to state the name of the Producer if different to the Supplier.");
			this.ProducerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 75, true);
			this.ProducerAddressControl.Name = "ProducerAddressControl";
			this.ProducerAddressControl.PopupCaption = "Producer";
			this.ProducerAddressControl.ShowAddress = false;
			this.ProducerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 17, true);
			this.ProducerAddressControl.TabIndex = 26;
			// 
			// GrowerAddressControl
			// 
			this.GrowerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrowerAddressControl, "FilteredInvoiceLines.JI_OA_GrowerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OA_GrowerAddress)));
			this.GrowerAddressControl.BindToOrgList = "Lookups.Importers";
			this.GrowerAddressControl.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("f3aa35ea-29b5-4dd5-bf9b-ee3a6d90c60f", "Grower", "Specify the details of the grower of crops for Tariff chapters 6, 7, 8, 10, 12. Must be transmitted to state the name of the Grower if different to the Supplier.");
			this.GrowerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 49, true);
			this.GrowerAddressControl.Name = "GrowerAddressControl";
			this.GrowerAddressControl.PopupCaption = "Grower";
			this.GrowerAddressControl.ShowAddress = false;
			this.GrowerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 17, true);
			this.GrowerAddressControl.TabIndex = 25;
			// 
			// IntendedUseGroupBox
			// 
			this.IntendedUseGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("c2cb7aea-891b-429a-bb54-be967667c0fc", "Intended Use", "LIMITED to Tariff chapter 2 – 22. Where known state the intended use of the goods.");
			this.IntendedUseGroupBox.Controls.Add(this.IntendedUseCodeDropEdit);
			this.IntendedUseGroupBox.Controls.Add(this.IntendedUseTextBox);
			this.IntendedUseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 124, true);
			this.IntendedUseGroupBox.Name = "IntendedUseGroupBox";
			this.IntendedUseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 102, true);
			this.IntendedUseGroupBox.TabIndex = 4;
			this.IntendedUseGroupBox.TabStop = false;
			this.IntendedUseGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("50acbd60-20d5-46a9-9418-0770be28a0fc", "Intended Use");
			// 
			// IntendedUseCodeDropEdit
			// 
			this.IntendedUseCodeDropEdit.AllowDrop = true;
			this.IntendedUseCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "FilteredInvoiceLines.JI_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_IntendedUseCode)));
			this.IntendedUseCodeDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("cf77a92c-86ad-4e12-aab9-adb3f0e43ad8", "Code", "Intended Use Code", "LIMITED to Tariff chapter 2 – 22. Where known state the intended use of the goods.");
			this.IntendedUseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 15, true);
			this.IntendedUseCodeDropEdit.Name = "IntendedUseCodeDropEdit";
			this.IntendedUseCodeDropEdit.PreBoundMaxLength = 2;
			this.IntendedUseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 17, true);
			this.IntendedUseCodeDropEdit.TabIndex = 0;
			// 
			// IntendedUseTextBox
			// 
			this.IntendedUseTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.IntendedUseTextBox, "FilteredInvoiceLines.JI_IntendedUse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_IntendedUse)));
			this.IntendedUseTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("d05a8e3e-6f0e-47ad-a22a-b75dc19cbcdd", "Text", "Intended Use Text", "LIMITED to Tariff chapter 2 – 22. Where known and where a suitable code is not available, state the intended use of the goods (free text description).");
			this.IntendedUseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 37, true);
			this.IntendedUseTextBox.Multiline = true;
			this.IntendedUseTextBox.Name = "IntendedUseTextBox";
			this.IntendedUseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.IntendedUseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 58, true);
			this.IntendedUseTextBox.TabIndex = 1;
			// 
			// TemperatureGroupBox
			// 
			this.TemperatureGroupBox.Controls.Add(this.TempDetailsToBeSentCheckBox);
			this.TemperatureGroupBox.Controls.Add(this.MaxTempCalcEdit);
			this.TemperatureGroupBox.Controls.Add(this.MinTempCalcEdit);
			this.TemperatureGroupBox.Controls.Add(this.StorageTempCalcEdit);
			this.TemperatureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 78, true);
			this.TemperatureGroupBox.Name = "TemperatureGroupBox";
			this.TemperatureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 40, true);
			this.TemperatureGroupBox.TabIndex = 1;
			this.TemperatureGroupBox.TabStop = false;
			this.TemperatureGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("d8d04902-c018-4dc3-b1b3-2a38803042ac", "Temperature Details (degrees Celsius)");
			// 
			// TempDetailsToBeSentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TempDetailsToBeSentCheckBox, "FilteredInvoiceLines.JI_TemperatureDetailsToBeSent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TemperatureDetailsToBeSent)));
			this.TempDetailsToBeSentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.TempDetailsToBeSentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TempDetailsToBeSentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 14, true);
			this.TempDetailsToBeSentCheckBox.Name = "TempDetailsToBeSentCheckBox";
			this.TempDetailsToBeSentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 20, true);
			this.TempDetailsToBeSentCheckBox.TabIndex = 0;
			this.TempDetailsToBeSentCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("62e6efc8-0608-4de8-a6a0-7b2a04aef8d5", "Send");
			this.TempDetailsToBeSentCheckBox.UseVisualStyleBackColor = true;
			// 
			// MaxTempCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxTempCalcEdit, "FilteredInvoiceLines.JI_MaxTemp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MaxTemp)));
			this.MaxTempCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("ee37f85b-74da-40ec-8bb8-b0ba0173efef", "Max.", "Maximum Storage Temperature", "May be transmitted to state the maximum storage temperature. State the temperature in Celsius .");
			this.MaxTempCalcEdit.DecimalPlaces = 0;
			this.MaxTempCalcEdit.Decimals = 0;
			this.MaxTempCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(417, 14, true);
			this.MaxTempCalcEdit.Name = "MaxTempCalcEdit";
			this.MaxTempCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.MaxTempCalcEdit.TabIndex = 3;
			this.MaxTempCalcEdit.Text = "0";
			this.MaxTempCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinTempCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MinTempCalcEdit, "FilteredInvoiceLines.JI_MinTemp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_MinTemp)));
			this.MinTempCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("f026b65b-9a39-4c6d-a3e1-1f1bfda99d15", "Min.", "Minimum Storage Temperature", "May be transmitted to state the minimum storage temperature.  State the temperature in Celsius.");
			this.MinTempCalcEdit.DecimalPlaces = 0;
			this.MinTempCalcEdit.Decimals = 0;
			this.MinTempCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 14, true);
			this.MinTempCalcEdit.Name = "MinTempCalcEdit";
			this.MinTempCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.MinTempCalcEdit.TabIndex = 2;
			this.MinTempCalcEdit.Text = "0";
			this.MinTempCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StorageTempCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StorageTempCalcEdit, "FilteredInvoiceLines.JI_StorageTemp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_StorageTemp)));
			this.StorageTempCalcEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("45270e59-04c8-4321-b857-d71a4845a58d", "Storage", "Storage Temp.", "Use to state any special temperature information for the goods. Negative values must be preceded by a minus sign (-). State the temperature in Celsius ");
			this.StorageTempCalcEdit.DecimalPlaces = 0;
			this.StorageTempCalcEdit.Decimals = 0;
			this.StorageTempCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 14, true);
			this.StorageTempCalcEdit.Name = "StorageTempCalcEdit";
			this.StorageTempCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 17, true);
			this.StorageTempCalcEdit.TabIndex = 1;
			this.StorageTempCalcEdit.Text = "0";
			this.StorageTempCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProductCharacteristicGroupBox
			// 
			this.ProductCharacteristicGroupBox.Controls.Add(this.GeneticallyModifiedCheckBox);
			this.ProductCharacteristicGroupBox.Controls.Add(this.UsedGoodsCheckBox);
			this.ProductCharacteristicGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(514, 78, true);
			this.ProductCharacteristicGroupBox.Name = "ProductCharacteristicGroupBox";
			this.ProductCharacteristicGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 40, true);
			this.ProductCharacteristicGroupBox.TabIndex = 2;
			this.ProductCharacteristicGroupBox.TabStop = false;
			this.ProductCharacteristicGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("19108e42-fe4b-4011-98b8-4799429f7072", "Characteristics");
			// 
			// GeneticallyModifiedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GeneticallyModifiedCheckBox, "FilteredInvoiceLines.JI_GeneticallyModified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_GeneticallyModified)));
			this.GeneticallyModifiedCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("BC882479-9FCF-467B-B5C1-6491FB5CDA60", "Gen. Mod.", "Genetically Mod.", "Genetically Modified", "Conditional. Must be transmitted to specify whether any goods have undergone Genetic Modification.");
			this.GeneticallyModifiedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GeneticallyModifiedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 16, true);
			this.GeneticallyModifiedCheckBox.Name = "GeneticallyModifiedCheckBox";
			this.GeneticallyModifiedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 16, true);
			this.GeneticallyModifiedCheckBox.TabIndex = 1;
			this.GeneticallyModifiedCheckBox.UseVisualStyleBackColor = true;
			// 
			// UsedGoodsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UsedGoodsCheckBox, "FilteredInvoiceLines.JI_UsedGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_UsedGoods)));
			this.UsedGoodsCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("24F2E859-7974-4221-96AA-02DC3E435D33", "Used Goods", "Used Goods", "Used Goods", "Conditional. Must be transmitted to specify whether any manufactured commodity is used.");
			this.UsedGoodsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UsedGoodsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 16, true);
			this.UsedGoodsCheckBox.Name = "UsedGoodsCheckBox";
			this.UsedGoodsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 16, true);
			this.UsedGoodsCheckBox.TabIndex = 0;
			this.UsedGoodsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConstituentsGroupBox
			// 
			this.ConstituentsGroupBox.Controls.Add(this.CommodityConstituentGrid);
			this.ConstituentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 230, true);
			this.ConstituentsGroupBox.Name = "ConstituentsGroupBox";
			this.ConstituentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 81, true);
			this.ConstituentsGroupBox.TabIndex = 6;
			this.ConstituentsGroupBox.TabStop = false;
			this.ConstituentsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("e3a00456-1f61-4835-8276-3fda2dd5af16", "Constituents");
			// 
			// CommodityConstituentGrid
			// 
			this.CommodityConstituentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CommodityConstituentGrid, "FilteredInvoiceLines.CommodityConstituents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CommodityConstituents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.CommodityConstituent)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CommodityConstituents)).SyncRoot)).NZ_ConstituentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.CommodityConstituent)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CommodityConstituents)).SyncRoot)).NZ_ConstituentQty)));
			this.CommodityConstituentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "NZ_ConstituentName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "NZ_ConstituentQty";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.CommodityConstituentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CommodityConstituentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CommodityConstituentGrid.CopySelectedRowsAllowed = true;
			this.CommodityConstituentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityConstituentGrid.GridId = "b5edad30-5ecf-439d-ab35-78e8b3790cb9";
			this.CommodityConstituentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommodityConstituentGrid.LayoutKey = "CommodityConstituentGrid";
			this.CommodityConstituentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CommodityConstituentGrid.Name = "CommodityConstituentGrid";
			this.CommodityConstituentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 65, true);
			this.CommodityConstituentGrid.TabIndex = 9;
			this.CommodityConstituentGrid.Tag = "Commodity Constituents";
			// 
			// ProductsGroupBox
			// 
			this.ProductsGroupBox.Controls.Add(this.CommodityProductsGrid);
			this.ProductsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(673, 230, true);
			this.ProductsGroupBox.Name = "ProductsGroupBox";
			this.ProductsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 81, true);
			this.ProductsGroupBox.TabIndex = 7;
			this.ProductsGroupBox.TabStop = false;
			this.ProductsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("be4f08a0-cbab-4027-8a86-8642e9c1012e", "Products");
			// 
			// CommodityProductsGrid
			// 
			this.CommodityProductsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CommodityProductsGrid, "FilteredInvoiceLines.CommodityProducts");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CommodityProducts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.CommodityProduct)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CommodityProducts)).SyncRoot)).NZ_ProductID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.CommodityProduct)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CommodityProducts)).SyncRoot)).NZ_ProductIDType)));
			this.CommodityProductsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.ColumnName = "NZ_ProductID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.ColumnName = "NZ_ProductIDType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.CommodityProductsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CommodityProductsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CommodityProductsGrid.CopySelectedRowsAllowed = true;
			this.CommodityProductsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityProductsGrid.GridId = "b5edad30-5ecf-439d-ab35-78e8b3790cb9";
			this.CommodityProductsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommodityProductsGrid.LayoutKey = "CommodityProductsGrid";
			this.CommodityProductsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CommodityProductsGrid.Name = "CommodityProductsGrid";
			this.CommodityProductsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 65, true);
			this.CommodityProductsGrid.TabIndex = 9;
			this.CommodityProductsGrid.Tag = "Commodity Products";
			// 
			// ClassificationGroupBox
			// 
			this.ClassificationGroupBox.Controls.Add(this.CommodityLinesGrid);
			this.ClassificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 230, true);
			this.ClassificationGroupBox.Name = "ClassificationGroupBox";
			this.ClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 81, true);
			this.ClassificationGroupBox.TabIndex = 5;
			this.ClassificationGroupBox.TabStop = false;
			this.ClassificationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("a558007b-b5a9-4dd4-a6a4-d29bf78ccda8", "Classifications");
			// 
			// CommodityLinesGrid
			// 
			this.CommodityLinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CommodityLinesGrid, "FilteredInvoiceLines.CommodityLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CommodityLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.CommodityLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CommodityLines)).SyncRoot)).NZ_Classification)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.CommodityLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CommodityLines)).SyncRoot)).NZ_ClassificationType)));
			this.CommodityLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "NZ_Classification";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "NZ_ClassificationType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.CommodityLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CommodityLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CommodityLinesGrid.CopySelectedRowsAllowed = true;
			this.CommodityLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommodityLinesGrid.GridId = "b5edad30-5ecf-439d-ab35-78e8b3790cb9";
			this.CommodityLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CommodityLinesGrid.LayoutKey = "CommodityLinesGrid";
			this.CommodityLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.CommodityLinesGrid.Name = "CommodityLinesGrid";
			this.CommodityLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 65, true);
			this.CommodityLinesGrid.TabIndex = 9;
			this.CommodityLinesGrid.Tag = "Commodity Lines";
			// 
			// DangerousGoodsGroupBox
			// 
			this.DangerousGoodsGroupBox.Controls.Add(this.zCalcEdit1);
			this.DangerousGoodsGroupBox.Controls.Add(this.zGuidFindBox2);
			this.DangerousGoodsGroupBox.Controls.Add(this.zGuidFindBox1);
			this.DangerousGoodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 3, true);
			this.DangerousGoodsGroupBox.Name = "DangerousGoodsGroupBox";
			this.DangerousGoodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(919, 73, true);
			this.DangerousGoodsGroupBox.TabIndex = 0;
			this.DangerousGoodsGroupBox.TabStop = false;
			this.DangerousGoodsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("215fe08a-c12e-48a3-820f-f43dcaaabca1", "Dangerous Goods");
			// 
			// zCalcEdit1
			// 
			this.zCalcEdit1.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_DGFlashPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DGFlashPoint)));
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 45, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.zCalcEdit1.TabIndex = 4;
			this.zCalcEdit1.Text = "0.00";
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zGuidFindBox2
			// 
			this.zGuidFindBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox2, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.zGuidFindBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 19, true);
			this.zGuidFindBox2.Name = "zGuidFindBox2";
			this.zGuidFindBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 17, true);
			this.zGuidFindBox2.TabIndex = 0;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "FilteredInvoiceLines.UNDGs.DI_OC_DGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs)).SyncRoot)).DI_OC_DGContact)));
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(558, 19, true);
			this.zGuidFindBox1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.PopupCaption = null;
			this.zGuidFindBox1.PreBoundMaxLength = 20;
			this.zGuidFindBox1.ShowDescriptionBox = false;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 17, true);
			this.zGuidFindBox1.TabIndex = 1;
			// 
			// ProductNameGroupBox
			// 
			this.ProductNameGroupBox.Controls.Add(this.TradeNameTextBox);
			this.ProductNameGroupBox.Controls.Add(this.RegisteredNameTextBox);
			this.ProductNameGroupBox.Controls.Add(this.CommonNameTextBox);
			this.ProductNameGroupBox.Controls.Add(this.BrandNameTextBox);
			this.ProductNameGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 317, true);
			this.ProductNameGroupBox.Name = "ProductNameGroupBox";
			this.ProductNameGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 40, true);
			this.ProductNameGroupBox.TabIndex = 8;
			this.ProductNameGroupBox.TabStop = false;
			this.ProductNameGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("397761dd-cfd9-438f-bc2e-d64b22566764", "Product Name");
			// 
			// TradeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.TradeNameTextBox, "FilteredInvoiceLines.JI_TradeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_TradeName)));
			this.TradeNameTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("DF272229-34A0-4EC2-9A64-4ED701D707AE", "Trade", "Trade", "Trade Name", "Optional – may be transmitted to state any name for the product assigned by the manufacturer, producer or grower. State any trade name of the goods.");
			this.TradeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(750, 16, true);
			this.TradeNameTextBox.Name = "TradeNameTextBox";
			this.TradeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.TradeNameTextBox.TabIndex = 3;
			// 
			// RegisteredNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegisteredNameTextBox, "FilteredInvoiceLines.JI_RegisteredName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_RegisteredName)));
			this.RegisteredNameTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("A9B94060-BE67-46F3-84D9-A01584CE4E5C", "Reg.", "Reg. Name", "Registered Name", "Optional – may be transmitted to state any name for the product assigned by the manufacturer, producer or grower. State any registered name of the goods.");
			this.RegisteredNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 16, true);
			this.RegisteredNameTextBox.Name = "RegisteredNameTextBox";
			this.RegisteredNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.RegisteredNameTextBox.TabIndex = 2;
			// 
			// CommonNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommonNameTextBox, "FilteredInvoiceLines.JI_CommonName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CommonName)));
			this.CommonNameTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("54AB0E78-3DC3-467A-AB9B-9E695088243E", "Common", "Common", "Common Name", "Optional – may be transmitted to state any name for the product assigned by the manufacturer, producer or grower. State any common name of the goods.");
			this.CommonNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 16, true);
			this.CommonNameTextBox.Name = "CommonNameTextBox";
			this.CommonNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.CommonNameTextBox.TabIndex = 1;
			// 
			// BrandNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BrandNameTextBox, "FilteredInvoiceLines.JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BrandName)));
			this.BrandNameTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("5B1EC160-826A-427A-82B9-55F123C5063E", "Brand", "Brand", "Brand Name", "Optional – may be transmitted to state any name for the product assigned by the manufacturer, producer or grower. State any brand name of the goods.");
			this.BrandNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 16, true);
			this.BrandNameTextBox.Name = "BrandNameTextBox";
			this.BrandNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.BrandNameTextBox.TabIndex = 0;
			// 
			// DangerousGoodsTabPage
			// 
			this.DangerousGoodsTabPage.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("d3c7a5b2-42fd-4be7-8465-fafbd75ceb75", "DG (HAZMAT)");
			this.DangerousGoodsTabPage.Controls.Add(this.HazMatGroupBox);
			this.DangerousGoodsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DangerousGoodsTabPage.Name = "DangerousGoodsTabPage";
			this.DangerousGoodsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DangerousGoodsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 298, true);
			this.DangerousGoodsTabPage.TabIndex = 5;
			this.DangerousGoodsTabPage.Text = "DG (HAZMAT)";
			// 
			// HazMatGroupBox
			// 
			this.HazMatGroupBox.Controls.Add(this.DGGuidFindBox);
			this.HazMatGroupBox.Controls.Add(this.FlashPointTempCalcEdit);
			this.HazMatGroupBox.Controls.Add(this.HazardousMaterialContactGuidFindBox);
			this.HazMatGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.HazMatGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HazMatGroupBox.Name = "HazMatGroupBox";
			this.HazMatGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 120, true);
			this.HazMatGroupBox.TabIndex = 1;
			this.HazMatGroupBox.TabStop = false;
			this.HazMatGroupBox.Text = "Dangerous Goods Details";
			// 
			// DGGuidFindBox
			// 
			this.DGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGGuidFindBox, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.DGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 20, true);
			this.DGGuidFindBox.Name = "DGGuidFindBox";
			this.DGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 17, true);
			this.DGGuidFindBox.TabIndex = 2;
			// 
			// FlashPointTempCalcEdit
			// 
			this.FlashPointTempCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FlashPointTempCalcEdit, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_DGFlashPoint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DGFlashPoint)));
			this.FlashPointTempCalcEdit.DecimalPlaces = 2;
			this.FlashPointTempCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 45, true);
			this.FlashPointTempCalcEdit.Name = "FlashPointTempCalcEdit";
			this.FlashPointTempCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
			this.FlashPointTempCalcEdit.TabIndex = 9;
			this.FlashPointTempCalcEdit.Text = "0.00";
			this.FlashPointTempCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// HazardousMaterialContactGuidFindBox
			// 
			this.HazardousMaterialContactGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HazardousMaterialContactGuidFindBox, "FilteredInvoiceLines.UNDGs.DI_OC_DGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs)).SyncRoot)).DI_OC_DGContact)));
			this.HazardousMaterialContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 21, true);
			this.HazardousMaterialContactGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.HazardousMaterialContactGuidFindBox.Name = "HazardousMaterialContactGuidFindBox";
			this.HazardousMaterialContactGuidFindBox.PopupCaption = null;
			this.HazardousMaterialContactGuidFindBox.PreBoundMaxLength = 20;
			this.HazardousMaterialContactGuidFindBox.ShowDescriptionBox = false;
			this.HazardousMaterialContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 17, true);
			this.HazardousMaterialContactGuidFindBox.TabIndex = 3;
			// 
			// JI_NetWeightCalcDropEdit
			// 
			this.JI_NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JI_NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.WeightUQList)));
			this.JI_NetWeightCalcDropEdit.BindToAmount = "FilteredInvoiceLines.JI_NetWeight";
			this.JI_NetWeightCalcDropEdit.BindToList = "FilteredInvoiceLines.Lookups+WeightUQList";
			this.JI_NetWeightCalcDropEdit.BindToUnit = "FilteredInvoiceLines.JI_NetWeightUQ";
			this.JI_NetWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("50830502-f1bf-4573-b67b-aac289bc8cbc", "Net Wgt", "", "");
			this.JI_NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 16, true);
			this.JI_NetWeightCalcDropEdit.Name = "JI_NetWeightCalcDropEdit";
			this.JI_NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 17, true);
			this.JI_NetWeightCalcDropEdit.TabIndex = 2;
			this.JI_NetWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// JI_EffectiveOriginRegionTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_EffectiveOriginRegionTextBox, "FilteredInvoiceLines.JI_EffectiveOriginRegion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_EffectiveOriginRegion)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JI_EffectiveOriginRegionTextBox, false);
			this.JI_EffectiveOriginRegionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 64, true);
			this.JI_EffectiveOriginRegionTextBox.Name = "JI_EffectiveOriginRegionTextBox";
			this.JI_EffectiveOriginRegionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.JI_EffectiveOriginRegionTextBox.TabIndex = 9;
			// 
			// JI_OriginRegionTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_OriginRegionTextBox, "FilteredInvoiceLines.JI_OriginRegion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OriginRegion)));
			this.JI_OriginRegionTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|69F830CD-ED2D-454F-B486-DAF11E04DF82", "Origin Region");
			this.JI_OriginRegionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 64, true);
			this.JI_OriginRegionTextBox.Name = "JI_OriginRegionTextBox";
			this.JI_OriginRegionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.JI_OriginRegionTextBox.TabIndex = 8;
			// 
			// PackagingGroupBox
			// 
			this.PackagingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.PackagingGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("5532f61e-e60b-4abd-8c62-af2f73703cf1", "Packaging", "Not required for empty containers or for type I53 Periodic Imports.");
			this.PackagingGroupBox.Controls.Add(this.ItemPackagingGrid);
			this.PackagingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(461, 266, true);
			this.PackagingGroupBox.Name = "PackagingGroupBox";
			this.PackagingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 90, true);
			this.PackagingGroupBox.TabIndex = 9;
			this.PackagingGroupBox.TabStop = false;
			this.PackagingGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("e8928636-ce1a-489a-bace-cda98949811f", "Packaging");
			// 
			// ItemPackagingGrid
			// 
			this.ItemPackagingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemPackagingGrid, "FilteredInvoiceLines.ItemPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ItemPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.ItemPackaging)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ItemPackages)).SyncRoot)).NZ_NumberOfPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ItemPackaging)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ItemPackages)).SyncRoot)).NZ_PackageUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NZ.Business.Declaration.ItemPackaging)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ItemPackages)).SyncRoot)).NZ_PackageVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ItemPackaging)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ItemPackages)).SyncRoot)).NZ_VolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ItemPackaging)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ItemPackages)).SyncRoot)).NZ_ShippingMarks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.ItemPackaging)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ItemPackages)).SyncRoot)).NZ_PackingMaterial)));
			this.ItemPackagingGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "NZ_NumberOfPackages";
			zCalcEditColumnStyleInfo12.Decimals = 0;
			zCalcEditColumnStyleInfo12.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|1F92B4C8-4103-4CCF-99BA-489EA4B273B1", "Packages");
			zCalcEditColumnStyleInfo12.IsMandatory = true;
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo13.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|1F92B4C8-4103-4CCF-99BA-489EA4B273B1", "Packages");
			zDropEditColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo13.ColumnName = "NZ_PackageUQ";
			zDropEditColumnStyleInfo13.IsMandatory = true;
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.ColumnName = "NZ_PackageVolume";
			zCalcEditColumnStyleInfo13.Decimals = 3;
			zCalcEditColumnStyleInfo13.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|9A23B832-1B44-4E03-A17C-8C975F890CAA", "Pkg. Volume");
			zCalcEditColumnStyleInfo13.IsMandatory = true;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo14.ColumnName = "NZ_VolumeUQ";
			zTextBoxColumnStyleInfo14.GroupName = Enterprise.Customs.NZ.GUI.Res.GetData("CustomsInvoiceLineUserControl|9A23B832-1B44-4E03-A17C-8C975F890CAA", "Pkg. Volume");
			zTextBoxColumnStyleInfo14.IsMandatory = true;
			zTextBoxColumnStyleInfo14.IsReadOnly = true;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo15.ColumnName = "NZ_ShippingMarks";
			zTextBoxColumnStyleInfo15.IsMandatory = true;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo16.ColumnName = "NZ_PackingMaterial";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ItemPackagingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.ItemPackagingGrid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.ItemPackagingGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.ItemPackagingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ItemPackagingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ItemPackagingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ItemPackagingGrid.CopySelectedRowsAllowed = true;
			this.ItemPackagingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemPackagingGrid.GridId = "b5edad30-5ecf-439d-ab35-78e8b3790cb9";
			this.ItemPackagingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemPackagingGrid.LayoutKey = "ItemPackagingGrid";
			this.ItemPackagingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ItemPackagingGrid.Name = "ItemPackagingGrid";
			this.ItemPackagingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 65, true);
			this.ItemPackagingGrid.TabIndex = 10;
			this.ItemPackagingGrid.Tag = "Item Packaging";
			// 
			// TariffCodeFindBox
			// 
			this.TariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.TariffCodeFindBox.ErrorForUnsupportedCountry = null;
			this.TariffCodeFindBox.GetEffectiveDate = null;
			this.TariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 17, true);
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TariffCodeFindBox.ParentType = null;
			this.TariffCodeFindBox.PreBoundMaxLength = 15;
			this.TariffCodeFindBox.SelectNomenclatureModes = null;
			this.TariffCodeFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.TariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 17, true);
			this.TariffCodeFindBox.TabIndex = 7;
			this.TariffCodeFindBox.TariffType = null;
			// 
			// ConcessionCodeDropEdit
			// 
			this.ConcessionCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConcessionCodeDropEdit, "FilteredInvoiceLines.JI_ConcessionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_ConcessionCode)));
			this.ConcessionCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 39, true);
			this.ConcessionCodeDropEdit.Name = "ConcessionCodeDropEdit";
			this.ConcessionCodeDropEdit.ShowDescriptionBox = false;
			this.ConcessionCodeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.ConcessionCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.ConcessionCodeDropEdit.TabIndex = 8;
			// 
			// CustomsInvoiceLineUserControl
			// 
			this.Name = "CustomsInvoiceLineUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 496, true);
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
			this.ClassificationDetailsGroupBox.ResumeLayout(false);
			this.ClassificationDetailsGroupBox.PerformLayout();
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
			this.PreferenceDropEdit.ResumeLayout(true);
			this.PreferenceDropEdit.PerformLayout();
			this.SupplementaryCalcDropEdit.ResumeLayout(true);
			this.SupplementaryCalcDropEdit.PerformLayout();
			this.CountryOfExportCodeFindBox.ResumeLayout(true);
			this.CountryOfExportCodeFindBox.PerformLayout();
			this.ConcessionCodeFindBox.ResumeLayout(true);
			this.ConcessionCodeFindBox.PerformLayout();
			this.DutiesLeviesGroupBox.ResumeLayout(false);
			this.DutiesLeviesGroupBox.PerformLayout();
			this.MiscTabPage.ResumeLayout(false);
			this.MiscTabPage.PerformLayout();
			this.ZeroRatedGroupBox.ResumeLayout(false);
			this.ZeroRatedGroupBox.PerformLayout();
			this.ZeroRatedGSTDropEdit.ResumeLayout(true);
			this.ZeroRatedGSTDropEdit.PerformLayout();
			this.ZeroRatedLeviesDropEdit.ResumeLayout(true);
			this.ZeroRatedLeviesDropEdit.PerformLayout();
			this.ZeroRatedExciseDropEdit.ResumeLayout(true);
			this.ZeroRatedExciseDropEdit.PerformLayout();
			this.ZeroRatedDutyDropEdit.ResumeLayout(true);
			this.ZeroRatedDutyDropEdit.PerformLayout();
			this.SpecialClassificationsGroupBox.ResumeLayout(false);
			this.SpecialClassificationsGroupBox.PerformLayout();
			this.ParentLineDropEdit.ResumeLayout(true);
			this.ParentLineDropEdit.PerformLayout();
			this.JI_PartsOfClassificationNZcClassFindBox.ResumeLayout(true);
			this.JI_PartsOfClassificationNZcClassFindBox.PerformLayout();
			this.CodeInfosTabControl.ResumeLayout(false);
			this.CodeInfosTabControl.PerformLayout();
			this.PermitCodesPage.ResumeLayout(false);
			this.PermitCodesPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PermitCodesGrid)).EndInit();
			this.PermitCodesGrid.ResumeLayout(false);
			this.PermitCodesGrid.PerformLayout();
			this.ProhibitedCodesTabPage.ResumeLayout(false);
			this.ProhibitedCodesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProhibitedCodesGrid)).EndInit();
			this.ProhibitedCodesGrid.ResumeLayout(false);
			this.ProhibitedCodesGrid.PerformLayout();
			this.OtherInfosTabPage.ResumeLayout(false);
			this.OtherInfosTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OtherInfosGrid)).EndInit();
			this.OtherInfosGrid.ResumeLayout(false);
			this.OtherInfosGrid.PerformLayout();
			this.LevyCurrencyControl.ResumeLayout(true);
			this.LevyCurrencyControl.PerformLayout();
			this.NZCClassificationFindBox.ResumeLayout(true);
			this.NZCClassificationFindBox.PerformLayout();
			this.PreferentialCountryGroupDropEdit.ResumeLayout(true);
			this.PreferentialCountryGroupDropEdit.PerformLayout();
			this.TSWTabPage.ResumeLayout(false);
			this.TSWTabPage.PerformLayout();
			this.CommodityDetailsGroupBox.ResumeLayout(false);
			this.CommodityDetailsGroupBox.PerformLayout();
			this.ManufacturerAddressAddressControl.ResumeLayout(true);
			this.ManufacturerAddressAddressControl.PerformLayout();
			this.TreatmentProviderFindBox.ResumeLayout(true);
			this.TreatmentProviderFindBox.PerformLayout();
			this.DateMarkingDateEdit.ResumeLayout(true);
			this.DateMarkingDateEdit.PerformLayout();
			this.ProducerAddressControl.ResumeLayout(true);
			this.ProducerAddressControl.PerformLayout();
			this.GrowerAddressControl.ResumeLayout(true);
			this.GrowerAddressControl.PerformLayout();
			this.IntendedUseGroupBox.ResumeLayout(false);
			this.IntendedUseGroupBox.PerformLayout();
			this.IntendedUseCodeDropEdit.ResumeLayout(true);
			this.IntendedUseCodeDropEdit.PerformLayout();
			this.TemperatureGroupBox.ResumeLayout(false);
			this.TemperatureGroupBox.PerformLayout();
			this.ProductCharacteristicGroupBox.ResumeLayout(false);
			this.ProductCharacteristicGroupBox.PerformLayout();
			this.ConstituentsGroupBox.ResumeLayout(false);
			this.ConstituentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommodityConstituentGrid)).EndInit();
			this.CommodityConstituentGrid.ResumeLayout(false);
			this.CommodityConstituentGrid.PerformLayout();
			this.ProductsGroupBox.ResumeLayout(false);
			this.ProductsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommodityProductsGrid)).EndInit();
			this.CommodityProductsGrid.ResumeLayout(false);
			this.CommodityProductsGrid.PerformLayout();
			this.ClassificationGroupBox.ResumeLayout(false);
			this.ClassificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CommodityLinesGrid)).EndInit();
			this.CommodityLinesGrid.ResumeLayout(false);
			this.CommodityLinesGrid.PerformLayout();
			this.DangerousGoodsGroupBox.ResumeLayout(false);
			this.DangerousGoodsGroupBox.PerformLayout();
			this.zGuidFindBox2.ResumeLayout(true);
			this.zGuidFindBox2.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.ProductNameGroupBox.ResumeLayout(false);
			this.ProductNameGroupBox.PerformLayout();
			this.DangerousGoodsTabPage.ResumeLayout(false);
			this.DangerousGoodsTabPage.PerformLayout();
			this.HazMatGroupBox.ResumeLayout(false);
			this.HazMatGroupBox.PerformLayout();
			this.DGGuidFindBox.ResumeLayout(true);
			this.DGGuidFindBox.PerformLayout();
			this.HazardousMaterialContactGuidFindBox.ResumeLayout(true);
			this.HazardousMaterialContactGuidFindBox.PerformLayout();
			this.JI_NetWeightCalcDropEdit.ResumeLayout(true);
			this.JI_NetWeightCalcDropEdit.PerformLayout();
			this.PackagingGroupBox.ResumeLayout(false);
			this.PackagingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemPackagingGrid)).EndInit();
			this.ItemPackagingGrid.ResumeLayout(false);
			this.ItemPackagingGrid.PerformLayout();
			this.LevyCodeDropEdit.ResumeLayout(true);
			this.LevyCodeDropEdit.PerformLayout();
			this.TariffCodeFindBox.ResumeLayout(true);
			this.TariffCodeFindBox.PerformLayout();
			this.PartsOfClassificationFindBox.ResumeLayout(true);
			this.PartsOfClassificationFindBox.PerformLayout();
			this.ConcessionCodeDropEdit.ResumeLayout(true);
            this.ConcessionCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		protected ZDropEdit PreferentialCountryGroupDropEdit;
		protected ZTabPage TSWTabPage;
		protected ZTabPage DangerousGoodsTabPage;
		protected ZGroupBox HazMatGroupBox;
		protected ZGuidFindBox DGGuidFindBox;
		protected ZCalcEdit FlashPointTempCalcEdit;
		protected ZGuidFindBox HazardousMaterialContactGuidFindBox;
		private IContainer components;
		private ZTextBox JI_OriginRegionTextBox;
		private ZTextBox JI_EffectiveOriginRegionTextBox;
		protected ZGroupBox PackagingGroupBox;
		private ZGrid ItemPackagingGrid;
		private ZGroupBox DangerousGoodsGroupBox;
		protected ZCalcEdit zCalcEdit1;
		protected ZGuidFindBox zGuidFindBox2;
		protected ZGuidFindBox zGuidFindBox1;
		private ZGroupBox CommodityDetailsGroupBox;
		protected ZAddressControl ManufacturerAddressAddressControl;
		private ZTextBox LotNumberTextBox;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox TreatmentProviderFindBox;
		private ZDateEdit DateMarkingDateEdit;
		protected ZAddressControl ProducerAddressControl;
		protected ZAddressControl GrowerAddressControl;
		private ZGroupBox IntendedUseGroupBox;
		private ZDropEdit IntendedUseCodeDropEdit;
		private ZTextBox IntendedUseTextBox;
		private ZGroupBox TemperatureGroupBox;
		private ZCheckBox TempDetailsToBeSentCheckBox;
		private ZCalcEdit MaxTempCalcEdit;
		private ZCalcEdit MinTempCalcEdit;
		private ZCalcEdit StorageTempCalcEdit;
		private ZGroupBox ProductCharacteristicGroupBox;
		private ZCheckBox GeneticallyModifiedCheckBox;
		private ZCheckBox UsedGoodsCheckBox;
		private ZGroupBox ProductNameGroupBox;
		private ZTextBox TradeNameTextBox;
		private ZTextBox RegisteredNameTextBox;
		private ZTextBox CommonNameTextBox;
		private ZTextBox BrandNameTextBox;
		private ZGroupBox ConstituentsGroupBox;
		private ZGrid CommodityConstituentGrid;
		private ZGroupBox ProductsGroupBox;
		private ZGrid CommodityProductsGrid;
		private ZGroupBox ClassificationGroupBox;
		private ZGrid CommodityLinesGrid;
		protected internal ZDropEdit LevyCodeDropEdit;
		internal Universal.GUI.TariffFindBox TariffCodeFindBox;
		internal Universal.GUI.TariffFindBox PartsOfClassificationFindBox;
		protected internal ZDropEdit ConcessionCodeDropEdit;
	}
}
