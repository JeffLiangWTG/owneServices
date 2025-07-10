using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	partial class SGInvoiceLineUserControl
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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				UnHookValueChanged(JobDeclaration as ICommonInvoiceDataProvider);
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AdditionalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.zTabPage4 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGroupBox4 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zTextBox13 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCalcDropEdit8 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zGroupBox7 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGroupBox8 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StrategicGoodsQtyCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.StrategicGoodsCategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zStgcGovLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.StrategicGoodsProductCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox14 = new Enterprise.ZArchitecture.ZTextBox();
			this.zDropEdit6 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit5 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit4 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsStrategicCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGrid2 = new Enterprise.ZArchitecture.ZGrid();
			this.zTabPage2 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGrid3 = new Enterprise.ZArchitecture.ZGrid();
			this.zTabPage3 = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zGrid4 = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OtherTaxUnitRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OtherTaxPercentageRateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OtherTaxPercentageRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit4 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcDropEdit9 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zLabel12 = new Enterprise.ZArchitecture.ZLabel();
			this.DutyPercentageRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit9 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcDropEdit10 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.ExciseUnitRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.ExcisePercentageRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.DutyUnitRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel16 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcDropEdit7 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCalcDropEdit5 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCalcDropEdit4 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCalcDropEdit6 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zDropEditPref = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CertificateOfOriginTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zPanel4 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zGroupBox5 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OriginCriterionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox17 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox15 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox5 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox10 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zTextBox11 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCalcDropEdit3 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCalcEdit2 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zCalcDropEdit2 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox8 = new Enterprise.ZArchitecture.ZTextBox();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			this.zDropEdit3 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox6 = new Enterprise.ZArchitecture.ZTextBox();
			this.JI_OutwardMAWBTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox7 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox16 = new Enterprise.ZArchitecture.ZTextBox();
			this.convertToLocalCurrencyControl1 = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.SGTariffFindBox = new Enterprise.Customs.SG.V4.GUI.TariffFindBox();
			this.OtherTaxAmountCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.MarksAndNosLabel = new Enterprise.ZArchitecture.ZLabel();
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
			this.AdditionalDetailsTabPage.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.zPanel2.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.zTabPage4.SuspendLayout();
			this.zGroupBox4.SuspendLayout();
			this.zDateEdit2.SuspendLayout();
			this.zCalcDropEdit8.SuspendLayout();
			this.zGroupBox7.SuspendLayout();
			this.zDropEdit1.SuspendLayout();
			this.zDropEdit2.SuspendLayout();
			this.zGroupBox8.SuspendLayout();
			this.StrategicGoodsQtyCalcDropEdit.SuspendLayout();
			this.StrategicGoodsCategoryDropEdit.SuspendLayout();
			this.StrategicGoodsProductCodeDropEdit.SuspendLayout();
			this.zDropEdit6.SuspendLayout();
			this.zDropEdit5.SuspendLayout();
			this.zDropEdit4.SuspendLayout();
			this.zTabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).BeginInit();
			this.zGrid2.SuspendLayout();
			this.zTabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid3)).BeginInit();
			this.zGrid3.SuspendLayout();
			this.zTabPage3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid4)).BeginInit();
			this.zGrid4.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zCalcDropEdit9.SuspendLayout();
			this.zCalcDropEdit10.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.zCalcDropEdit7.SuspendLayout();
			this.zCalcDropEdit5.SuspendLayout();
			this.zCalcDropEdit4.SuspendLayout();
			this.zCalcDropEdit6.SuspendLayout();
			this.zDropEditPref.SuspendLayout();
			this.CertificateOfOriginTabPage.SuspendLayout();
			this.zPanel4.SuspendLayout();
			this.zGroupBox5.SuspendLayout();
			this.OriginCriterionDropEdit.SuspendLayout();
			this.zCalcDropEdit3.SuspendLayout();
			this.zDateEdit1.SuspendLayout();
			this.zCalcDropEdit2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.zDropEdit3.SuspendLayout();
			this.convertToLocalCurrencyControl1.SuspendLayout();
			this.SGTariffFindBox.SuspendLayout();
			this.OtherTaxAmountCurrencyControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// InvoiceLinesSummaryGroupBox
			// 
			this.InvoiceLinesSummaryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 0, true);
			this.InvoiceLinesSummaryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 347, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 237, true);
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 347, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 534, true);
			// 
			// LineDetailTabControl
			// 
			this.LineDetailTabControl.Controls.Add(this.AdditionalDetailsTabPage);
			this.LineDetailTabControl.Controls.Add(this.CertificateOfOriginTabPage);
			this.LineDetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(762, 347, true);
			this.LineDetailTabControl.Controls.SetChildIndex(this.NewLineDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.CertificateOfOriginTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.AdditionalDetailsTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineChargesTabPage, 0);
			this.LineDetailTabControl.Controls.SetChildIndex(this.LineDetailsTabPage, 0);
			// 
			// InvoiceDetailsGroupBox
			// 
			this.InvoiceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 233, true);
			this.InvoiceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 92, true);
			// 
			// JI_CountryOfOriginBoundFindBox
			// 
			this.JI_CountryOfOriginBoundFindBox.TabIndex = 2;
			// 
			// JI_RH_NKCommodity_CodeBoundFindBox
			// 
			this.JI_RH_NKCommodity_CodeBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 17, true);
			this.JI_RH_NKCommodity_CodeBoundFindBox.TabIndex = 6;
			// 
			// JI_LinePriceBoundCurrencyControl
			// 
			this.JI_LinePriceBoundCurrencyControl.TabIndex = 4;
			// 
			// ClassificationDetailsGroupBox
			// 
			this.ClassificationDetailsGroupBox.Controls.Add(this.zDropEditPref);
			this.ClassificationDetailsGroupBox.Controls.Add(this.MarksAndNosLabel);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBox2);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBox7);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBox16);
			this.ClassificationDetailsGroupBox.Controls.Add(this.JI_OutwardMAWBTextBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBox6);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zDropEdit3);
			this.ClassificationDetailsGroupBox.Controls.Add(this.SGTariffFindBox);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zGrid1);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zCalcEdit1);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBox8);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBox4);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBox3);
			this.ClassificationDetailsGroupBox.Controls.Add(this.zTextBox1);
			this.ClassificationDetailsGroupBox.Controls.Remove(this.BondedWHSOrderLineNumberCalcEdit);
			this.ClassificationDetailsGroupBox.Controls.Remove(this.BondedWHSOrderNumberTextBox);
			this.ClassificationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 195, true);
			this.ClassificationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|51572C8A-1E27-4756-8CFA-2E0F56152725", "Customs Details");
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.CustomsQuantityCalcDropEdit, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox1, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox3, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox4, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox8, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zCalcEdit1, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zGrid1, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.SGTariffFindBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zDropEdit3, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox6, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.JI_OutwardMAWBTextBox, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox16, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox7, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zTextBox2, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.MarksAndNosLabel, 0);
			this.ClassificationDetailsGroupBox.Controls.SetChildIndex(this.zDropEditPref, 0);
			// 
			// LineChargesTabPage
			// 
			this.LineChargesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LineChargesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 325, true);
			// 
			// CurrentInvoicePanel
			// 
			this.CurrentInvoicePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 84, true);
			// 
			// LineSummaryPanel
			// 
			this.LineSummaryPanel.Controls.Add(this.OtherTaxAmountCurrencyControl);
			this.LineSummaryPanel.Controls.Add(this.convertToLocalCurrencyControl1);
			this.LineSummaryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 244, true);
			this.LineSummaryPanel.TabIndex = 0;
			this.LineSummaryPanel.Controls.SetChildIndex(this.oLabel8, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_DutyConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_GSTConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FOBConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_FreightConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_InsuranceConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.JI_Calc_CIFConvertToLocalCurrencyControl, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.convertToLocalCurrencyControl1, 0);
			this.LineSummaryPanel.Controls.SetChildIndex(this.OtherTaxAmountCurrencyControl, 0);
			// 
			// PendingApportionmentLabel
			// 
			this.PendingApportionmentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 326, true);
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 325, true);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 325, true);
			// 
			// CusContainerInvoiceLineGrid
			// 
			this.CusContainerInvoiceLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 308, true);
			// 
			// CantCreateInvoiceLinesLabel
			// 
			this.CantCreateInvoiceLinesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.CantCreateInvoiceLinesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 534, true);
			// 
			// LineDetailsTabPage
			// 
			this.LineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 325, true);
			// 
			// NewLineDetailsTabPage
			// 
			this.NewLineDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NewLineDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 325, true);
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.InvoiceLineDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 325, true);
			// 
			// ClassificationPanel
			// 
			this.ClassificationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ClassificationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 195, true);
			// 
			// CustomsInvoiceLinesBoundGrid
			// 
			this.CustomsInvoiceLinesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 534, true);
			// 
			// JI_Calc_CIFConvertToLocalCurrencyControl
			// 
			this.JI_Calc_CIFConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_CustomsValue";
			this.JI_Calc_CIFConvertToLocalCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.JI_Calc_CIFConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|9a7c44b5-b81e-4a69-b1ff-27a66431100d", "Customs Value");
			this.JI_Calc_CIFConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Guid;
			this.JI_Calc_CIFConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 94, true);
			this.JI_Calc_CIFConvertToLocalCurrencyControl.TabIndex = 4;
			// 
			// JI_Calc_InsuranceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 70, true);
			this.JI_Calc_InsuranceConvertToLocalCurrencyControl.TabIndex = 3;
			// 
			// JI_Calc_FreightConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FreightConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 46, true);
			this.JI_Calc_FreightConvertToLocalCurrencyControl.TabIndex = 2;
			// 
			// JI_Calc_FOBConvertToLocalCurrencyControl
			// 
			this.JI_Calc_FOBConvertToLocalCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_OtherAmount";
			this.JI_Calc_FOBConvertToLocalCurrencyControl.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|cbff40a7-668f-406e-888c-56abbdfbbb6b", "Other");
			this.JI_Calc_FOBConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 22, true);
			this.JI_Calc_FOBConvertToLocalCurrencyControl.TabIndex = 1;
			// 
			// JI_Calc_GSTConvertToLocalCurrencyControl
			// 
			this.JI_Calc_GSTConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 190, true);
			this.JI_Calc_GSTConvertToLocalCurrencyControl.TabIndex = 8;
			// 
			// JI_Calc_DutyConvertToLocalCurrencyControl
			// 
			this.JI_Calc_DutyConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 118, true);
			this.JI_Calc_DutyConvertToLocalCurrencyControl.TabIndex = 5;
			// 
			// JI_Calc_BalanceConvertToLocalCurrencyControl
			// 
			this.JI_Calc_BalanceConvertToLocalCurrencyControl.TabIndex = 3;
			// 
			// JI_Calc_LinesEnteredConvertToLocalCurrencyControl
			// 
			this.JI_Calc_LinesEnteredConvertToLocalCurrencyControl.TabIndex = 2;
			// 
			// JI_Calc_LinesTotalConvertToLocalCurrencyControl
			// 
			this.JI_Calc_LinesTotalConvertToLocalCurrencyControl.TabIndex = 1;
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 38, true);
			this.CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 1;
			this.CustomsQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// VolumeCalcDropEdit
			// 
			this.VolumeCalcDropEdit.TabIndex = 3;
			// 
			// JI_WeightCalcDropEdit
			// 
			this.JI_WeightCalcDropEdit.TabIndex = 5;
			// 
			// InvoiceQuantityCalcDropEdit
			// 
			this.InvoiceQuantityCalcDropEdit.TabIndex = 1;
			// 
			// JI_DescriptionBoundTextBox
			// 
			this.JI_DescriptionBoundTextBox.TabIndex = 0;
			// 
			// oLabel8
			// 
			this.oLabel8.TabIndex = 0;
			// 
			// Splitter
			// 
			this.Splitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.Splitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.Splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 227, true);
			this.Splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 10, true);
			this.Splitter.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.JobDeclaration);
			// 
			// AdditionalDetailsTabPage
			// 
			this.AdditionalDetailsTabPage.Controls.Add(this.zPanel1);
			this.AdditionalDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.AdditionalDetailsTabPage.Name = "AdditionalDetailsTabPage";
			this.AdditionalDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(757, 325, true);
			this.AdditionalDetailsTabPage.TabIndex = 3;
			this.AdditionalDetailsTabPage.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|E36429F5-0D31-4D15-8E4D-5BBF0525D5A1", "Additional Details");
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zPanel2);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 319, true);
			this.zPanel1.TabIndex = 0;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.zTabControl1);
			this.zPanel2.Controls.Add(this.zGroupBox1);
			this.zPanel2.Controls.Add(this.zGroupBox2);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(751, 319, true);
			this.zPanel2.TabIndex = 0;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.zTabControl1.Controls.Add(this.zTabPage4);
			this.zTabControl1.Controls.Add(this.zTabPage1);
			this.zTabControl1.Controls.Add(this.zTabPage2);
			this.zTabControl1.Controls.Add(this.zTabPage3);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 0, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 311, true);
			this.zTabControl1.TabIndex = 2;
			// 
			// zTabPage4
			// 
			this.zTabPage4.Controls.Add(this.zGroupBox4);
			this.zTabPage4.Controls.Add(this.zGroupBox7);
			this.zTabPage4.Controls.Add(this.zGroupBox8);
			this.zTabPage4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zTabPage4.Name = "zTabPage4";
			this.zTabPage4.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 289, true);
			this.zTabPage4.TabIndex = 3;
			this.zTabPage4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|8C600ECF-65E3-4B68-A552-B6FD6D78A2C8", "Goods Information");
			// 
			// zGroupBox4
			// 
			this.zGroupBox4.Controls.Add(this.zDateEdit2);
			this.zGroupBox4.Controls.Add(this.zTextBox13);
			this.zGroupBox4.Controls.Add(this.zCalcDropEdit8);
			this.zGroupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 195, true);
			this.zGroupBox4.Name = "zGroupBox4";
			this.zGroupBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 91, true);
			this.zGroupBox4.TabIndex = 2;
			this.zGroupBox4.TabStop = false;
			this.zGroupBox4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|595B425E-489E-48E4-9E93-AEEE8B384DB2", "Vehicle Details");
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AllowDrop = true;
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "FilteredInvoiceLines.SG_FirstRegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_FirstRegistrationDate)));
			this.zDateEdit2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|dd4d4359-0bdf-4416-9ffb-6e2202569d4e", "First Reg.");
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 56, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 2;
			// 
			// zTextBox13
			// 
			this.BindingSource.SetBindingMember(this.zTextBox13, "FilteredInvoiceLines.SG_VehicleRegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_VehicleRegistrationNumber)));
			this.zTextBox13.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|9eacf389-2f08-4664-af43-7194a4e6ffe9", "Reg. No");
			this.zTextBox13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 34, true);
			this.zTextBox13.Name = "zTextBox13";
			this.zTextBox13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.zTextBox13.TabIndex = 1;
			// 
			// zCalcDropEdit8
			// 
			this.zCalcDropEdit8.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit8, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_EngineCapacityPower)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_EngineCapacityPowerUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.EngineCapacityList)));
			this.zCalcDropEdit8.BindToAmount = "FilteredInvoiceLines.SG_EngineCapacityPower";
			this.zCalcDropEdit8.BindToList = "FilteredInvoiceLines.AddInfoLookups+EngineCapacityList";
			this.zCalcDropEdit8.BindToUnit = "FilteredInvoiceLines.SG_EngineCapacityPowerUnit";
			this.zCalcDropEdit8.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|c8d81d45-f6bc-4761-af16-88489874cb5a", "Engine CC/KW");
			this.zCalcDropEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 12, true);
			this.zCalcDropEdit8.Name = "zCalcDropEdit8";
			this.zCalcDropEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.zCalcDropEdit8.TabIndex = 0;
			this.zCalcDropEdit8.UnitPreBoundMaxLength = 3;
			// 
			// zGroupBox7
			// 
			this.zGroupBox7.Controls.Add(this.zDropEdit1);
			this.zGroupBox7.Controls.Add(this.zDropEdit2);
			this.zGroupBox7.Dock = System.Windows.Forms.DockStyle.Left;
			this.zGroupBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 195, true);
			this.zGroupBox7.Name = "zGroupBox7";
			this.zGroupBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 91, true);
			this.zGroupBox7.TabIndex = 1;
			this.zGroupBox7.TabStop = false;
			this.zGroupBox7.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|08B13BDD-C635-4DB3-8D36-436FF5840CEC", "Additional");
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "FilteredInvoiceLines.SG_TariffCommodityType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_TariffCommodityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).AddInfoLookups.CommodityTypes)));
			this.zDropEdit1.BindToList = "AddInfoLookups+CommodityTypes";
			this.zDropEdit1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|1d640b3f-82cd-4c29-80a6-cf9211f140a9", "Goods Type");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 34, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.ShowDescriptionBox = false;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "FilteredInvoiceLines.SG_ESNDPIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_ESNDPIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.ESNDPs)));
			this.zDropEdit2.BindToList = "FilteredInvoiceLines.AddInfoLookups+ESNDPs";
			this.zDropEdit2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|468592d3-f2aa-493f-88ea-12e1f218249c", "E/SDNP");
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 12, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.PreBoundMaxLength = 2;
			this.zDropEdit2.ShowDescriptionBox = false;
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 17, true);
			this.zDropEdit2.TabIndex = 0;
			// 
			// zGroupBox8
			// 
			this.zGroupBox8.Controls.Add(this.StrategicGoodsQtyCalcDropEdit);
			this.zGroupBox8.Controls.Add(this.StrategicGoodsCategoryDropEdit);
			this.zGroupBox8.Controls.Add(this.zStgcGovLink);
			this.zGroupBox8.Controls.Add(this.StrategicGoodsProductCodeDropEdit);
			this.zGroupBox8.Controls.Add(this.zTextBox14);
			this.zGroupBox8.Controls.Add(this.zDropEdit6);
			this.zGroupBox8.Controls.Add(this.zDropEdit5);
			this.zGroupBox8.Controls.Add(this.zDropEdit4);
			this.zGroupBox8.Controls.Add(this.IsStrategicCheckBox);
			this.zGroupBox8.Dock = System.Windows.Forms.DockStyle.Top;
			this.zGroupBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox8.Name = "zGroupBox8";
			this.zGroupBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 192, true);
			this.zGroupBox8.TabIndex = 0;
			this.zGroupBox8.TabStop = false;
			this.zGroupBox8.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|1F890261-8129-4A32-B0D0-A56C455A1A7F", "Strategic Goods");
			// 
			// StrategicGoodsQtyCalcDropEdit
			// 
			this.StrategicGoodsQtyCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StrategicGoodsQtyCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_StrategicGoodsProductCodeQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_StrategicGoodsProductCodeQuantityUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.ProductCodeUQList)));
			this.StrategicGoodsQtyCalcDropEdit.BindToAmount = "FilteredInvoiceLines.SG_StrategicGoodsProductCodeQuantity";
			this.StrategicGoodsQtyCalcDropEdit.BindToList = "FilteredInvoiceLines.AddInfoLookups+ProductCodeUQList";
			this.StrategicGoodsQtyCalcDropEdit.BindToUnit = "FilteredInvoiceLines.SG_StrategicGoodsProductCodeQuantityUnit";
			this.StrategicGoodsQtyCalcDropEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|0991BECD-081B-4AAD-AD31-38C86635E173", "Product Quantity");
			this.StrategicGoodsQtyCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 63, true);
			this.StrategicGoodsQtyCalcDropEdit.Name = "StrategicGoodsQtyCalcDropEdit";
			this.StrategicGoodsQtyCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.StrategicGoodsQtyCalcDropEdit.TabIndex = 4;
			this.StrategicGoodsQtyCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// StrategicGoodsCategoryDropEdit
			// 
			this.StrategicGoodsCategoryDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StrategicGoodsCategoryDropEdit, "FilteredInvoiceLines.SG_StrategicGoodsCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_StrategicGoodsCategory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.StrategicGoodsCategory)));
			this.StrategicGoodsCategoryDropEdit.BindToList = "FilteredInvoiceLines.AddInfoLookups+StrategicGoodsCategory";
			this.StrategicGoodsCategoryDropEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|e344b5b1-7b9a-4683-a92f-bce79c45d429", "Category");
			this.StrategicGoodsCategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 21, true);
			this.StrategicGoodsCategoryDropEdit.MaxItemsToShowInDropDown = 12;
			this.StrategicGoodsCategoryDropEdit.Name = "StrategicGoodsCategoryDropEdit";
			this.StrategicGoodsCategoryDropEdit.PreBoundMaxLength = 3;
			this.StrategicGoodsCategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.StrategicGoodsCategoryDropEdit.TabIndex = 1;
			// 
			// zStgcGovLink
			// 
			this.zStgcGovLink.AutoSize = true;
			this.zStgcGovLink.IsFontBold = false;
			this.zStgcGovLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 22, true);
			this.zStgcGovLink.Name = "zStgcGovLink";
			this.zStgcGovLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 13, true);
			this.zStgcGovLink.TabIndex = 2;
			this.zStgcGovLink.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|6CCC0D10-1BB0-48E2-9006-C993072E63FC", "Help on Codes");
			this.zStgcGovLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.zStgcGovLink_LinkClicked);
			// 
			// StrategicGoodsProductCodeDropEdit
			// 
			this.StrategicGoodsProductCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StrategicGoodsProductCodeDropEdit, "FilteredInvoiceLines.SG_CategoryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_CategoryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.StrategicGoodsProductCode)));
			this.StrategicGoodsProductCodeDropEdit.BindToList = "FilteredInvoiceLines.AddInfoLookups+StrategicGoodsProductCode";
			this.StrategicGoodsProductCodeDropEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|0e8fe613-1512-42df-992e-ac86fa8d2bca", "Product Code");
			this.StrategicGoodsProductCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 42, true);
			this.StrategicGoodsProductCodeDropEdit.MaxItemsToShowInDropDown = 15;
			this.StrategicGoodsProductCodeDropEdit.Name = "StrategicGoodsProductCodeDropEdit";
			this.StrategicGoodsProductCodeDropEdit.PreBoundMaxLength = 15;
			this.StrategicGoodsProductCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 17, true);
			this.StrategicGoodsProductCodeDropEdit.TabIndex = 3;
			// 
			// zTextBox14
			// 
			this.BindingSource.SetBindingMember(this.zTextBox14, "FilteredInvoiceLines.SG_EndUseDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_EndUseDescription)));
			this.zTextBox14.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|e1daba5a-ec66-4a77-ae4c-c8243d98b4d8", "End Use Desc.");
			this.zTextBox14.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 149, true);
			this.zTextBox14.Multiline = true;
			this.zTextBox14.Name = "zTextBox14";
			this.zTextBox14.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 32, true);
			this.zTextBox14.TabIndex = 8;
			// 
			// zDropEdit6
			// 
			this.zDropEdit6.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit6, "FilteredInvoiceLines.SG_EndUseCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_EndUseCode2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.EndUseCodes2)));
			this.zDropEdit6.BindToList = "FilteredInvoiceLines.AddInfoLookups+EndUseCodes2";
			this.zDropEdit6.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|b6a50856-e9e1-400a-89a5-a70c559561b0", "End Use Code 2");
			this.zDropEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 106, true);
			this.zDropEdit6.Name = "zDropEdit6";
			this.zDropEdit6.PreBoundMaxLength = 3;
			this.zDropEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 17, true);
			this.zDropEdit6.TabIndex = 6;
			// 
			// zDropEdit5
			// 
			this.zDropEdit5.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit5, "FilteredInvoiceLines.SG_EndUseCode3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_EndUseCode3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.EndUseCodes3)));
			this.zDropEdit5.BindToList = "FilteredInvoiceLines.AddInfoLookups+EndUseCodes3";
			this.zDropEdit5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|c5fd1a63-8984-4a10-877e-44442f8b7f96", "End Use Code 3");
			this.zDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 127, true);
			this.zDropEdit5.Name = "zDropEdit5";
			this.zDropEdit5.PreBoundMaxLength = 3;
			this.zDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 17, true);
			this.zDropEdit5.TabIndex = 7;
			// 
			// zDropEdit4
			// 
			this.zDropEdit4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit4, "FilteredInvoiceLines.SG_EndUseCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_EndUseCode1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.EndUseCodes1)));
			this.zDropEdit4.BindToList = "FilteredInvoiceLines.AddInfoLookups+EndUseCodes1";
			this.zDropEdit4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|c7ad1c5e-0fb7-4579-a38a-edec001a6c82", "End Use Code 1");
			this.zDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 85, true);
			this.zDropEdit4.Name = "zDropEdit4";
			this.zDropEdit4.PreBoundMaxLength = 3;
			this.zDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 17, true);
			this.zDropEdit4.TabIndex = 5;
			// 
			// IsStrategicCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsStrategicCheckBox, "FilteredInvoiceLines.SG_IsStrategic");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_IsStrategic)));
			this.IsStrategicCheckBox.Checked = true;
			this.IsStrategicCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.IsStrategicCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 0, true);
			this.IsStrategicCheckBox.Name = "IsStrategicCheckBox";
			this.IsStrategicCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.IsStrategicCheckBox.TabIndex = 0;
			this.IsStrategicCheckBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|7421F8DA-BEC6-458F-BCB3-2BE360CCF497", "Are the goods Strategic?");
			this.IsStrategicCheckBox.UseVisualStyleBackColor = true;
			// 
			// zTabPage1
			// 
			this.zTabPage1.Controls.Add(this.zGrid2);
			this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zTabPage1.Name = "zTabPage1";
			this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 289, true);
			this.zTabPage1.TabIndex = 0;
			this.zTabPage1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|74E64F16-612A-438B-A203-4B87654620AF", "CA/SC Code 1");
			// 
			// zGrid2
			// 
			this.zGrid2.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid2, "FilteredInvoiceLines.CASCCode1s");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CASCCode1s)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.CASCCode1)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CASCCode1s)).SyncRoot)).CY_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CASCCode1)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CASCCode1s)).SyncRoot)).CY_Data)));
			this.zGrid2.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8C6556D3-2DF3-4397-871E-DB97EFDD08B1", "Sequence No.");
			zCalcEditColumnStyleInfo1.ColumnName = "CY_Order";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("79FC245A-78DE-4813-95B6-A048FB899CFB", "CA/SC Code 1");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.zGrid2.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zGrid2.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid2.GridId = "aeb431d2-0b79-453c-abe7-9e1ae9784f5b";
			this.zGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid2.LayoutKey = "zGrid2";
			this.zGrid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGrid2.Name = "zGrid2";
			this.zGrid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 283, true);
			this.zGrid2.TabIndex = 0;
			// 
			// zTabPage2
			// 
			this.zTabPage2.Controls.Add(this.zGrid3);
			this.zTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zTabPage2.Name = "zTabPage2";
			this.zTabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 289, true);
			this.zTabPage2.TabIndex = 1;
			this.zTabPage2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|5B72E032-1E30-4835-B55F-52B317FA2534", "CA/SC Code 2");
			// 
			// zGrid3
			// 
			this.zGrid3.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid3, "FilteredInvoiceLines.CASCCode2s");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CASCCode2s)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.CASCCode2)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CASCCode2s)).SyncRoot)).CY_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CASCCode2)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CASCCode2s)).SyncRoot)).CY_Data)));
			this.zGrid3.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8C6556D3-2DF3-4397-871E-DB97EFDD08B1", "Sequence No.");
			zCalcEditColumnStyleInfo2.ColumnName = "CY_Order";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("DADBA290-B9E1-4F61-864E-C6D31103E9EC", "CA/SC Code 2");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.zGrid3.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zGrid3.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid3.GridId = "83a8ae76-d255-4299-af4b-f1eb317612e7";
			this.zGrid3.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid3.LayoutKey = "zGrid2";
			this.zGrid3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGrid3.Name = "zGrid3";
			this.zGrid3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 283, true);
			this.zGrid3.TabIndex = 0;
			// 
			// zTabPage3
			// 
			this.zTabPage3.Controls.Add(this.zGrid4);
			this.zTabPage3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.zTabPage3.Name = "zTabPage3";
			this.zTabPage3.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zTabPage3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 289, true);
			this.zTabPage3.TabIndex = 2;
			this.zTabPage3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|D0B63F46-7B47-47A5-B9EC-09A2F1B096B8", "CA/SC Code 3");
			// 
			// zGrid4
			// 
			this.zGrid4.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid4, "FilteredInvoiceLines.CASCCode3s");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CASCCode3s)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.CASCCode3)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CASCCode3s)).SyncRoot)).CY_Order)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CASCCode3)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CASCCode3s)).SyncRoot)).CY_Data)));
			this.zGrid4.CaptionVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8C6556D3-2DF3-4397-871E-DB97EFDD08B1", "Sequence No.");
			zCalcEditColumnStyleInfo3.ColumnName = "CY_Order";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("B45736B0-DCEE-4B05-A1F3-8C59F27AD519", "CA/SC Code 3");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.zGrid4.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.zGrid4.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGrid4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid4.GridId = "3f04651c-20e6-4ddf-9230-36ff6c9253ce";
			this.zGrid4.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid4.LayoutKey = "zGrid2";
			this.zGrid4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGrid4.Name = "zGrid4";
			this.zGrid4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 283, true);
			this.zGrid4.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.OtherTaxUnitRateCalcEdit);
			this.zGroupBox1.Controls.Add(this.OtherTaxPercentageRateLabel);
			this.zGroupBox1.Controls.Add(this.OtherTaxPercentageRateCalcEdit);
			this.zGroupBox1.Controls.Add(this.zCalcEdit4);
			this.zGroupBox1.Controls.Add(this.zCalcDropEdit9);
			this.zGroupBox1.Controls.Add(this.zLabel12);
			this.zGroupBox1.Controls.Add(this.DutyPercentageRateCalcEdit);
			this.zGroupBox1.Controls.Add(this.zCalcEdit9);
			this.zGroupBox1.Controls.Add(this.zCalcDropEdit10);
			this.zGroupBox1.Controls.Add(this.ExciseUnitRateCalcEdit);
			this.zGroupBox1.Controls.Add(this.zLabel5);
			this.zGroupBox1.Controls.Add(this.ExcisePercentageRateCalcEdit);
			this.zGroupBox1.Controls.Add(this.zLabel6);
			this.zGroupBox1.Controls.Add(this.DutyUnitRateCalcEdit);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 133, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 178, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|A8CDC913-1070-4C68-AD40-4B32B0066499", "Duty/Excise/Other");
			// 
			// OtherTaxUnitRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OtherTaxUnitRateCalcEdit, "FilteredInvoiceLines.SG_OtherTaxUnitRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_OtherTaxUnitRate)));
			this.OtherTaxUnitRateCalcEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("729d5b10-500a-40c7-8bd0-bd7894c502f3", "Other Tax Per Unit");
			this.OtherTaxUnitRateCalcEdit.DecimalPlaces = 4;
			this.OtherTaxUnitRateCalcEdit.Decimals = 4;
			this.OtherTaxUnitRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 151, true);
			this.OtherTaxUnitRateCalcEdit.Name = "OtherTaxUnitRateCalcEdit";
			this.OtherTaxUnitRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.OtherTaxUnitRateCalcEdit.TabIndex = 11;
			this.OtherTaxUnitRateCalcEdit.Text = "0.0000";
			this.OtherTaxUnitRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.OtherTaxUnitRateCalcEdit.TrackDisposedAccess = true;
			// 
			// OtherTaxPercentageRateLabel
			// 
			this.OtherTaxPercentageRateLabel.AutoSize = true;
			this.OtherTaxPercentageRateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OtherTaxPercentageRateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 155, true);
			this.OtherTaxPercentageRateLabel.Name = "OtherTaxPercentageRateLabel";
			this.OtherTaxPercentageRateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.OtherTaxPercentageRateLabel.TabIndex = 13;
			this.OtherTaxPercentageRateLabel.Text = "%";
			this.OtherTaxPercentageRateLabel.UseMnemonic = false;
			// 
			// OtherTaxPercentageRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OtherTaxPercentageRateCalcEdit, "FilteredInvoiceLines.SG_OtherTaxPercentageRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_OtherTaxPercentageRate)));
			this.OtherTaxPercentageRateCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OtherTaxPercentageRateCalcEdit, false);
			this.OtherTaxPercentageRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 152, true);
			this.OtherTaxPercentageRateCalcEdit.Name = "OtherTaxPercentageRateCalcEdit";
			this.OtherTaxPercentageRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.OtherTaxPercentageRateCalcEdit.TabIndex = 12;
			this.OtherTaxPercentageRateCalcEdit.Text = "0.00";
			this.OtherTaxPercentageRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.OtherTaxPercentageRateCalcEdit.TrackDisposedAccess = true;
			// 
			// zCalcEdit4
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit4, "FilteredInvoiceLines.SG_PercAlcohol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_PercAlcohol)));
			this.zCalcEdit4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|4805f110-c27e-4aa2-a8c8-dcc390b7a138", "% of Alcohol");
			this.zCalcEdit4.DecimalPlaces = 4;
			this.zCalcEdit4.Decimals = 4;
			this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 60, true);
			this.zCalcEdit4.Name = "zCalcEdit4";
			this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 17, true);
			this.zCalcEdit4.TabIndex = 2;
			this.zCalcEdit4.Text = "0.0000";
			this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEdit4.TrackDisposedAccess = true;
			// 
			// zCalcDropEdit9
			// 
			this.zCalcDropEdit9.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit9, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_UnitDutiableWGTVOLQTY)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_UnitDutiableWGTVOLQTYUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.UnitOfQuantityList)));
			this.zCalcDropEdit9.BindToAmount = "FilteredInvoiceLines.SG_UnitDutiableWGTVOLQTY";
			this.zCalcDropEdit9.BindToList = "FilteredInvoiceLines.AddInfoLookups+UnitOfQuantityList";
			this.zCalcDropEdit9.BindToUnit = "FilteredInvoiceLines.SG_UnitDutiableWGTVOLQTYUnit";
			this.zCalcDropEdit9.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|c223b7ca-7c55-47e7-9d95-4c996752ee1f", "Duty Qty/Wgt./Vol");
			this.zCalcDropEdit9.Decimals = 4;
			this.zCalcDropEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 15, true);
			this.zCalcDropEdit9.Name = "zCalcDropEdit9";
			this.zCalcDropEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.zCalcDropEdit9.TabIndex = 0;
			this.zCalcDropEdit9.UnitPreBoundMaxLength = 3;
			// 
			// zLabel12
			// 
			this.zLabel12.AutoSize = true;
			this.zLabel12.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel12.IsFontBold = true;
			this.zLabel12.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 89, true);
			this.zLabel12.Name = "zLabel12";
			this.zLabel12.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 13, true);
			this.zLabel12.TabIndex = 4;
			this.zLabel12.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|3A19CBD1-040E-4313-86AC-5B4250948BD3", "Duty/Excise/Other Rates:");
			this.zLabel12.UseMnemonic = false;
			// 
			// DutyPercentageRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyPercentageRateCalcEdit, "FilteredInvoiceLines.SG_DutyPercentageRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_DutyPercentageRate)));
			this.DutyPercentageRateCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DutyPercentageRateCalcEdit, false);
			this.DutyPercentageRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 109, true);
			this.DutyPercentageRateCalcEdit.Name = "DutyPercentageRateCalcEdit";
			this.DutyPercentageRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.DutyPercentageRateCalcEdit.TabIndex = 6;
			this.DutyPercentageRateCalcEdit.Text = "0.00";
			this.DutyPercentageRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DutyPercentageRateCalcEdit.TrackDisposedAccess = true;
			// 
			// zCalcEdit9
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit9, "FilteredInvoiceLines.SG_TobaccoMultiplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_TobaccoMultiplier)));
			this.zCalcEdit9.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|e443b0a4-c645-4740-926c-3dd91953ba60", "Tobacco X");
			this.zCalcEdit9.DecimalPlaces = 0;
			this.zCalcEdit9.Decimals = 0;
			this.zCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 60, true);
			this.zCalcEdit9.Name = "zCalcEdit9";
			this.zCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 17, true);
			this.zCalcEdit9.TabIndex = 3;
			this.zCalcEdit9.Text = "0";
			this.zCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEdit9.TrackDisposedAccess = true;
			// 
			// zCalcDropEdit10
			// 
			this.zCalcDropEdit10.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit10, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_TotalDutiableWGTVOLQTY)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_TotalDutiableWGTVOLQTYUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.UnitOfQuantityList)));
			this.zCalcDropEdit10.BindToAmount = "FilteredInvoiceLines.SG_TotalDutiableWGTVOLQTY";
			this.zCalcDropEdit10.BindToList = "FilteredInvoiceLines.AddInfoLookups+UnitOfQuantityList";
			this.zCalcDropEdit10.BindToUnit = "FilteredInvoiceLines.SG_TotalDutiableWGTVOLQTYUnit";
			this.zCalcDropEdit10.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|0d8edb81-cfaf-4825-922a-9d3ccd1ad5b3", "Total Qty/Wgt./Vol");
			this.zCalcDropEdit10.Decimals = 4;
			this.zCalcDropEdit10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 37, true);
			this.zCalcDropEdit10.Name = "zCalcDropEdit10";
			this.zCalcDropEdit10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.zCalcDropEdit10.TabIndex = 1;
			this.zCalcDropEdit10.UnitPreBoundMaxLength = 3;
			// 
			// ExciseUnitRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExciseUnitRateCalcEdit, "FilteredInvoiceLines.SG_ExciseUnitRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_ExciseUnitRate)));
			this.ExciseUnitRateCalcEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|fc9565de-47a2-4439-ac66-c879c0955ef5", "Excise Per Unit");
			this.ExciseUnitRateCalcEdit.DecimalPlaces = 4;
			this.ExciseUnitRateCalcEdit.Decimals = 4;
			this.ExciseUnitRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 130, true);
			this.ExciseUnitRateCalcEdit.Name = "ExciseUnitRateCalcEdit";
			this.ExciseUnitRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.ExciseUnitRateCalcEdit.TabIndex = 8;
			this.ExciseUnitRateCalcEdit.Text = "0.0000";
			this.ExciseUnitRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ExciseUnitRateCalcEdit.TrackDisposedAccess = true;
			// 
			// zLabel5
			// 
			this.zLabel5.AutoSize = true;
			this.zLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 134, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel5.TabIndex = 10;
			this.zLabel5.Text = "%";
			this.zLabel5.UseMnemonic = false;
			// 
			// ExcisePercentageRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ExcisePercentageRateCalcEdit, "FilteredInvoiceLines.SG_ExcisePercentageRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_ExcisePercentageRate)));
			this.ExcisePercentageRateCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExcisePercentageRateCalcEdit, false);
			this.ExcisePercentageRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 131, true);
			this.ExcisePercentageRateCalcEdit.Name = "ExcisePercentageRateCalcEdit";
			this.ExcisePercentageRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.ExcisePercentageRateCalcEdit.TabIndex = 9;
			this.ExcisePercentageRateCalcEdit.Text = "0.00";
			this.ExcisePercentageRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ExcisePercentageRateCalcEdit.TrackDisposedAccess = true;
			// 
			// zLabel6
			// 
			this.zLabel6.AutoSize = true;
			this.zLabel6.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 112, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 13, true);
			this.zLabel6.TabIndex = 7;
			this.zLabel6.Text = "%";
			this.zLabel6.UseMnemonic = false;
			// 
			// DutyUnitRateCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyUnitRateCalcEdit, "FilteredInvoiceLines.SG_DutyUnitRate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_DutyUnitRate)));
			this.DutyUnitRateCalcEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|f4e70260-9887-4901-aa08-f09a2ea663b1", "Duty Per Unit");
			this.DutyUnitRateCalcEdit.DecimalPlaces = 4;
			this.DutyUnitRateCalcEdit.Decimals = 4;
			this.DutyUnitRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 108, true);
			this.DutyUnitRateCalcEdit.Name = "DutyUnitRateCalcEdit";
			this.DutyUnitRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.DutyUnitRateCalcEdit.TabIndex = 5;
			this.DutyUnitRateCalcEdit.Text = "0.0000";
			this.DutyUnitRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DutyUnitRateCalcEdit.TrackDisposedAccess = true;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.zLabel16);
			this.zGroupBox2.Controls.Add(this.zLabel4);
			this.zGroupBox2.Controls.Add(this.zCalcDropEdit7);
			this.zGroupBox2.Controls.Add(this.zCalcDropEdit5);
			this.zGroupBox2.Controls.Add(this.zCalcDropEdit4);
			this.zGroupBox2.Controls.Add(this.zCalcDropEdit6);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 131, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|099C5C72-6552-4F8A-A66A-DF5F0FE8A644", "Pack Quantity");
			// 
			// zLabel16
			// 
			this.BindingSource.SetBindingMember(this.zLabel16, "FilteredInvoiceLines.TotalQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).TotalQuantity)));
			this.zLabel16.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel16.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel16.IsFontBold = true;
			this.zLabel16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 103, true);
			this.zLabel16.Name = "zLabel16";
			this.zLabel16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 19, true);
			this.zLabel16.TabIndex = 4;
			this.zLabel16.Text = "0";
			this.zLabel16.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel16.UseMnemonic = false;
			// 
			// zLabel4
			// 
			this.zLabel4.AutoSize = true;
			this.zLabel4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|b3d9e6e1-9cdb-457a-bce8-53a5a313bdaf", "Total Quantity");
			this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.zLabel4.IsFontBold = true;
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 107, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.zLabel4.TabIndex = 5;
			this.zLabel4.UseMnemonic = false;
			// 
			// zCalcDropEdit7
			// 
			this.zCalcDropEdit7.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit7, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_InmostPackQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_InmostPackQuantityUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.UnitOfQuantityList)));
			this.zCalcDropEdit7.BindToAmount = "FilteredInvoiceLines.SG_InmostPackQuantity";
			this.zCalcDropEdit7.BindToList = "FilteredInvoiceLines.AddInfoLookups+UnitOfQuantityList";
			this.zCalcDropEdit7.BindToUnit = "FilteredInvoiceLines.SG_InmostPackQuantityUnit";
			this.zCalcDropEdit7.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|58021b4d-0ab0-40fe-bd1c-d6c016ddfe96", "Inmost Packs");
			this.zCalcDropEdit7.Decimals = 0;
			this.zCalcDropEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 80, true);
			this.zCalcDropEdit7.MaxValue = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.zCalcDropEdit7.Name = "zCalcDropEdit7";
			this.zCalcDropEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.zCalcDropEdit7.TabIndex = 3;
			this.zCalcDropEdit7.UnitPreBoundMaxLength = 3;
			// 
			// zCalcDropEdit5
			// 
			this.zCalcDropEdit5.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit5, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_InnerPackQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_InnerPackQuantityUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.UnitOfQuantityList)));
			this.zCalcDropEdit5.BindToAmount = "FilteredInvoiceLines.SG_InnerPackQuantity";
			this.zCalcDropEdit5.BindToList = "FilteredInvoiceLines.AddInfoLookups+UnitOfQuantityList";
			this.zCalcDropEdit5.BindToUnit = "FilteredInvoiceLines.SG_InnerPackQuantityUnit";
			this.zCalcDropEdit5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|3f87ae8c-2af2-4929-bddc-7320efd29005", "Inner Packs");
			this.zCalcDropEdit5.Decimals = 0;
			this.zCalcDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 58, true);
			this.zCalcDropEdit5.MaxValue = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.zCalcDropEdit5.Name = "zCalcDropEdit5";
			this.zCalcDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.zCalcDropEdit5.TabIndex = 2;
			this.zCalcDropEdit5.UnitPreBoundMaxLength = 3;
			// 
			// zCalcDropEdit4
			// 
			this.zCalcDropEdit4.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit4, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_OuterPackQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_OuterPackQuantityUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.UnitOfQuantityList)));
			this.zCalcDropEdit4.BindToAmount = "FilteredInvoiceLines.SG_OuterPackQuantity";
			this.zCalcDropEdit4.BindToList = "FilteredInvoiceLines.AddInfoLookups+UnitOfQuantityList";
			this.zCalcDropEdit4.BindToUnit = "FilteredInvoiceLines.SG_OuterPackQuantityUnit";
			this.zCalcDropEdit4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|863daccd-fca7-4156-985a-5a6d5c70c160", "Outer Packs");
			this.zCalcDropEdit4.Decimals = 0;
			this.zCalcDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 14, true);
			this.zCalcDropEdit4.MaxValue = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.zCalcDropEdit4.Name = "zCalcDropEdit4";
			this.zCalcDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.zCalcDropEdit4.TabIndex = 0;
			this.zCalcDropEdit4.UnitPreBoundMaxLength = 3;
			// 
			// zCalcDropEdit6
			// 
			this.zCalcDropEdit6.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit6, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_InPackQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_InPackQuantityUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.UnitOfQuantityList)));
			this.zCalcDropEdit6.BindToAmount = "FilteredInvoiceLines.SG_InPackQuantity";
			this.zCalcDropEdit6.BindToList = "FilteredInvoiceLines.AddInfoLookups+UnitOfQuantityList";
			this.zCalcDropEdit6.BindToUnit = "FilteredInvoiceLines.SG_InPackQuantityUnit";
			this.zCalcDropEdit6.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|c1dcbef7-c919-48c0-9af5-ec9bafa7be77", "In Packs");
			this.zCalcDropEdit6.Decimals = 0;
			this.zCalcDropEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 36, true);
			this.zCalcDropEdit6.MaxValue = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.zCalcDropEdit6.Name = "zCalcDropEdit6";
			this.zCalcDropEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.zCalcDropEdit6.TabIndex = 1;
			this.zCalcDropEdit6.UnitPreBoundMaxLength = 3;
			// 
			// zDropEditPref
			// 
			this.zDropEditPref.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditPref, "FilteredInvoiceLines.JI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_PrimaryPreference)));
			this.zDropEditPref.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 60, true);
			this.zDropEditPref.MaxItemsToShowInDropDown = 6;
			this.zDropEditPref.Name = "zDropEditPref";
			this.zDropEditPref.PreBoundMaxLength = 3;
			this.zDropEditPref.ShowDescriptionBox = false;
			this.zDropEditPref.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.zDropEditPref.TabIndex = 2;
			// 
			// CertificateOfOriginTabPage
			// 
			this.CertificateOfOriginTabPage.Controls.Add(this.zPanel4);
			this.CertificateOfOriginTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CertificateOfOriginTabPage.Name = "CertificateOfOriginTabPage";
			this.CertificateOfOriginTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CertificateOfOriginTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(711, 325, true);
			this.CertificateOfOriginTabPage.TabIndex = 5;
			this.CertificateOfOriginTabPage.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|2E835C37-B857-435C-8BBF-274270BB3ED5", "Certificate of Origin");
			// 
			// zPanel4
			// 
			this.zPanel4.Controls.Add(this.zGroupBox5);
			this.zPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zPanel4.Name = "zPanel4";
			this.zPanel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 319, true);
			this.zPanel4.TabIndex = 0;
			// 
			// zGroupBox5
			// 
			this.zGroupBox5.Controls.Add(this.OriginCriterionDropEdit);
			this.zGroupBox5.Controls.Add(this.zTextBox17);
			this.zGroupBox5.Controls.Add(this.zTextBox15);
			this.zGroupBox5.Controls.Add(this.zTextBox5);
			this.zGroupBox5.Controls.Add(this.zTextBox10);
			this.zGroupBox5.Controls.Add(this.zCalcEdit3);
			this.zGroupBox5.Controls.Add(this.zTextBox11);
			this.zGroupBox5.Controls.Add(this.zCalcDropEdit3);
			this.zGroupBox5.Controls.Add(this.zCalcEdit2);
			this.zGroupBox5.Controls.Add(this.zDateEdit1);
			this.zGroupBox5.Controls.Add(this.zCalcDropEdit2);
			this.zGroupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox5.Name = "zGroupBox5";
			this.zGroupBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 319, true);
			this.zGroupBox5.TabIndex = 0;
			this.zGroupBox5.TabStop = false;
			this.zGroupBox5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|BC2D660E-CA3F-4874-AE7A-CC328911406B", "Certificate of Origin");
			// 
			// OriginCriterionDropEdit
			// 
			this.OriginCriterionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginCriterionDropEdit, "FilteredInvoiceLines.SG_CertOriginCriterion1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_CertOriginCriterion1)));
			this.OriginCriterionDropEdit.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|bf218f2c-550a-4bd8-980b-6602c76a63a9", "Origin Criterion 1");
			this.OriginCriterionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 14, true);
			this.OriginCriterionDropEdit.Name = "OriginCriterionDropEdit";
			this.OriginCriterionDropEdit.PreBoundMaxLength = 25;
			this.OriginCriterionDropEdit.ShowDescriptionBox = false;
			this.OriginCriterionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 17, true);
			this.OriginCriterionDropEdit.TabIndex = 3;
			// 
			// zTextBox17
			// 
			this.BindingSource.SetBindingMember(this.zTextBox17, "FilteredInvoiceLines.SG_CertHSCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_CertHSCode)));
			this.zTextBox17.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|622cccb9-62f7-4714-97f1-ca91a8b7a9c5", "HS Code");
			this.zTextBox17.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 77, true);
			this.zTextBox17.Name = "zTextBox17";
			this.zTextBox17.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 17, true);
			this.zTextBox17.TabIndex = 6;
			// 
			// zTextBox15
			// 
			this.BindingSource.SetBindingMember(this.zTextBox15, "FilteredInvoiceLines.SG_CertOriginCriterion3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_CertOriginCriterion3)));
			this.zTextBox15.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|0bc9aab4-f7d1-4612-a065-9794ee7a885b", "Origin Criterion 3");
			this.zTextBox15.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 56, true);
			this.zTextBox15.Name = "zTextBox15";
			this.zTextBox15.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.zTextBox15.TabIndex = 5;
			// 
			// zTextBox5
			// 
			this.BindingSource.SetBindingMember(this.zTextBox5, "FilteredInvoiceLines.SG_CertOriginCriterion2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_CertOriginCriterion2)));
			this.zTextBox5.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|4d52ee62-332e-4986-a199-4e078b1e2a31", "Origin Criterion 2");
			this.zTextBox5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 35, true);
			this.zTextBox5.Name = "zTextBox5";
			this.zTextBox5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.zTextBox5.TabIndex = 4;
			// 
			// zTextBox10
			// 
			this.BindingSource.SetBindingMember(this.zTextBox10, "FilteredInvoiceLines.SG_TextileCatCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_TextileCatCode)));
			this.zTextBox10.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|18f138d9-d89a-4877-8f04-bac0859876d4", "Textile Category Code");
			this.zTextBox10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 140, true);
			this.zTextBox10.Name = "zTextBox10";
			this.zTextBox10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 17, true);
			this.zTextBox10.TabIndex = 9;
			// 
			// zCalcEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "FilteredInvoiceLines.SG_CertItemValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_CertItemValue)));
			this.zCalcEdit3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|130244be-ce49-48e9-ba8f-89436e13d17b", "Item Value");
			this.zCalcEdit3.DecimalPlaces = 2;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.zCalcEdit3.TabIndex = 0;
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEdit3.TrackDisposedAccess = true;
			// 
			// zTextBox11
			// 
			this.BindingSource.SetBindingMember(this.zTextBox11, "FilteredInvoiceLines.CertItemDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).CertItemDescription)));
			this.zTextBox11.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|89ce95b6-59cb-4ff8-ae1b-b3ff63495fbb", "Item Description");
			this.zTextBox11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 58, true);
			this.zTextBox11.Multiline = true;
			this.zTextBox11.Name = "zTextBox11";
			this.zTextBox11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 223, true);
			this.zTextBox11.TabIndex = 2;
			// 
			// zCalcDropEdit3
			// 
			this.zCalcDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit3, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_TextileQuotaQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_TextileQuotaQuantityUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.ProductCodeUQList)));
			this.zCalcDropEdit3.BindToAmount = "FilteredInvoiceLines.SG_TextileQuotaQuantity";
			this.zCalcDropEdit3.BindToList = "FilteredInvoiceLines.AddInfoLookups+ProductCodeUQList";
			this.zCalcDropEdit3.BindToUnit = "FilteredInvoiceLines.SG_TextileQuotaQuantityUnit";
			this.zCalcDropEdit3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|38856f00-f713-4610-8c05-7ce732838ec6", "Textile Quota Quantity");
			this.zCalcDropEdit3.Decimals = 4;
			this.zCalcDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 161, true);
			this.zCalcDropEdit3.Name = "zCalcDropEdit3";
			this.zCalcDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 17, true);
			this.zCalcDropEdit3.TabIndex = 10;
			this.zCalcDropEdit3.UnitPreBoundMaxLength = 3;
			// 
			// zCalcEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit2, "FilteredInvoiceLines.SG_PercContent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_PercContent)));
			this.zCalcEdit2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|e6bf6eb7-9a97-4e19-a71a-0bf47679700f", "% Content");
			this.zCalcEdit2.DecimalPlaces = 2;
			this.zCalcEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 119, true);
			this.zCalcEdit2.Name = "zCalcEdit2";
			this.zCalcEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 17, true);
			this.zCalcEdit2.TabIndex = 8;
			this.zCalcEdit2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEdit2.TrackDisposedAccess = true;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AllowDrop = true;
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "FilteredInvoiceLines.SG_ManufacturingCostStatementDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_ManufacturingCostStatementDate)));
			this.zDateEdit1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|3b5d28aa-57b0-4c58-8372-e84ad95d1c1e", "Mfr. Cost Statement");
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 98, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 7;
			// 
			// zCalcDropEdit2
			// 
			this.zCalcDropEdit2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zCalcDropEdit2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_CertItemQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_CertItemQuantityUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).AddInfoLookups.ProductCodeUQList)));
			this.zCalcDropEdit2.BindToAmount = "FilteredInvoiceLines.SG_CertItemQuantity";
			this.zCalcDropEdit2.BindToList = "FilteredInvoiceLines.AddInfoLookups+ProductCodeUQList";
			this.zCalcDropEdit2.BindToUnit = "FilteredInvoiceLines.SG_CertItemQuantityUnit";
			this.zCalcDropEdit2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|55426ec7-0194-433c-9367-ee0d67097e95", "Item Quantity");
			this.zCalcDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 37, true);
			this.zCalcDropEdit2.Name = "zCalcDropEdit2";
			this.zCalcDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.zCalcDropEdit2.TabIndex = 1;
			this.zCalcDropEdit2.UnitPreBoundMaxLength = 3;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "FilteredInvoiceLines.JI_BrandName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_BrandName)));
			this.zTextBox1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|d99bd2a5-0ef3-41e7-a6eb-1818c3188a23", "Brand Name");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 82, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.zTextBox1.TabIndex = 3;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "FilteredInvoiceLines.SG_PreviousLotNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_PreviousLotNo)));
			this.zTextBox3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|731fd0b6-6643-4609-8c27-d552edad9b1f", "Previous Lot No");
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 172, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.zTextBox3.TabIndex = 7;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "FilteredInvoiceLines.SG_LotNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_LotNo)));
			this.zTextBox4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|dd8437f3-9b0d-4e30-9d17-fce6af2c139c", "Current Lot No");
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 148, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.zTextBox4.TabIndex = 6;
			// 
			// zTextBox8
			// 
			this.BindingSource.SetBindingMember(this.zTextBox8, "FilteredInvoiceLines.JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Model)));
			this.zTextBox8.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|ac0251de-b729-4549-99de-66bb76cbd81b", "Model");
			this.zTextBox8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 104, true);
			this.zTextBox8.Name = "zTextBox8";
			this.zTextBox8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.zTextBox8.TabIndex = 4;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "FilteredInvoiceLines.SG_LastSellingPrice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_LastSellingPrice)));
			this.zCalcEdit1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|86a734a0-3eeb-46a5-a479-495439255384", "Item LSP Value");
			this.zCalcEdit1.DecimalPlaces = 2;
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 126, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.zCalcEdit1.TabIndex = 5;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.zCalcEdit1.TrackDisposedAccess = true;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "FilteredInvoiceLines.ProductCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ProductCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ProductCodes)).SyncRoot)).BZ_Tariff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ProductCodes)).SyncRoot)).BZ_Qty1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ProductCodes)).SyncRoot)).BZ_UQ1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.CusLineTariffDetail)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).ProductCodes)).SyncRoot)).Lookups.ProductCodeUQList)));
			this.zGrid1.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "BZ_Tariff";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("761C94C0-A59F-483F-9B42-9405DB5989AA", "Qty");
			zCalcEditColumnStyleInfo4.ColumnName = "BZ_Qty1";
			zCalcEditColumnStyleInfo4.Decimals = 4;
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.IsMandatory = true;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.ProductCodeUQList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("C97D198F-8CD7-41EB-87A1-C3545F389BB9", "U/Q");
			zDropEditColumnStyleInfo2.ColumnName = "BZ_UQ1";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.zGrid1.GridId = "ca5d5615-d2f6-4009-950f-0595e48794f3";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 41, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 70, true);
			this.zGrid1.TabIndex = 8;
			// 
			// zDropEdit3
			// 
			this.zDropEdit3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEdit3, "FilteredInvoiceLines.JI_HazMatCodeQualifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_HazMatCodeQualifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.HazardousMaterialCodeQualifierList)));
			this.zDropEdit3.BindToList = "FilteredInvoiceLines.Lookups+HazardousMaterialCodeQualifierList";
			this.zDropEdit3.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|d30e5065-4daa-4508-ab98-44ed237f234b", "DG Indicator");
			this.zDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 115, true);
			this.zDropEdit3.Name = "zDropEdit3";
			this.zDropEdit3.PreBoundMaxLength = 1;
			this.zDropEdit3.ShowDescriptionBox = false;
			this.zDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 17, true);
			this.zDropEdit3.TabIndex = 13;
			// 
			// zTextBox6
			// 
			this.BindingSource.SetBindingMember(this.zTextBox6, "FilteredInvoiceLines.SG_InwardMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_InwardMAWB)));
			this.zTextBox6.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|a688a01c-90f6-477d-a4b5-6cf160f17347", "Inward M/B");
			this.zTextBox6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 67, true);
			this.zTextBox6.Name = "zTextBox6";
			this.zTextBox6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.zTextBox6.TabIndex = 11;
			// 
			// JI_OutwardMAWBTextBox
			// 
			this.BindingSource.SetBindingMember(this.JI_OutwardMAWBTextBox, "FilteredInvoiceLines.JI_OutwardMAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_OutwardMAWB)));
			this.JI_OutwardMAWBTextBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|3133387a-f12a-4d7f-8ebe-c0c0f9778b59", "Outward M/B");
			this.JI_OutwardMAWBTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 91, true);
			this.JI_OutwardMAWBTextBox.Name = "JI_OutwardMAWBTextBox";
			this.JI_OutwardMAWBTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.JI_OutwardMAWBTextBox.TabIndex = 12;
			// 
			// zTextBox7
			// 
			this.BindingSource.SetBindingMember(this.zTextBox7, "FilteredInvoiceLines.SG_OutwardHAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_OutwardHAWB)));
			this.zTextBox7.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|21cd1d39-cada-4385-87f7-d567e6b8323b", "Outward H/B");
			this.zTextBox7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 43, true);
			this.zTextBox7.Name = "zTextBox7";
			this.zTextBox7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.zTextBox7.TabIndex = 10;
			// 
			// zTextBox16
			// 
			this.BindingSource.SetBindingMember(this.zTextBox16, "FilteredInvoiceLines.SG_InwardHAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SG_InwardHAWB)));
			this.zTextBox16.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|fbfba580-d22e-4f84-a742-7b2b1640a8c4", "Inward H/B");
			this.zTextBox16.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 19, true);
			this.zTextBox16.Name = "zTextBox16";
			this.zTextBox16.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 17, true);
			this.zTextBox16.TabIndex = 9;
			// 
			// convertToLocalCurrencyControl1
			// 
			this.convertToLocalCurrencyControl1.AllowDrop = true;
			this.convertToLocalCurrencyControl1.BindToAmount = "FilteredInvoiceLines.JI_Calc_ExciseAmount";
			this.convertToLocalCurrencyControl1.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.convertToLocalCurrencyControl1.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.convertToLocalCurrencyControl1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|8e1d9ab2-89a3-44d8-831c-5925a4eca555", "Excise");
			this.convertToLocalCurrencyControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 142, true);
			this.convertToLocalCurrencyControl1.Name = "convertToLocalCurrencyControl1";
			this.convertToLocalCurrencyControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.convertToLocalCurrencyControl1.TabIndex = 6;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "FilteredInvoiceLines.MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).MarksAndNumbers)));
			this.zTextBox2.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|25c0aacb-6073-4581-ad96-a1588e7f2e93", "Marks & Nums.");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 139, true);
			this.zTextBox2.Multiline = true;
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 50, true);
			this.zTextBox2.TabIndex = 15;
			// 
			// SGTariffFindBox
			// 
			this.SGTariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SGTariffFindBox, "FilteredInvoiceLines.JI_Tariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_Tariff)));
			this.SGTariffFindBox.ErrorForUnsupportedCountry = null;
			this.SGTariffFindBox.GetEffectiveDate = null;
			this.SGTariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 16, true);
			this.SGTariffFindBox.Name = "SGTariffFindBox";
			this.SGTariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.SGTariffFindBox.ParentType = null;
			this.SGTariffFindBox.SelectNomenclatureModes = null;
			this.SGTariffFindBox.ShowDescriptionBox = false;
			this.SGTariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.SGTariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.SGTariffFindBox.TabIndex = 0;
			this.SGTariffFindBox.TariffType = "HSN";
			// 
			// OtherTaxAmountCurrencyControl
			// 
			this.OtherTaxAmountCurrencyControl.AllowDrop = true;
			this.OtherTaxAmountCurrencyControl.BindToAmount = "FilteredInvoiceLines.JI_Calc_OtherTaxAmount";
			this.OtherTaxAmountCurrencyControl.BindToList = "FilteredInvoiceLines.Lookups+CurrencyList";
			this.OtherTaxAmountCurrencyControl.BindToUnit = "FilteredInvoiceLines.JI_RX_LocalCurrency";
			this.OtherTaxAmountCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 166, true);
			this.OtherTaxAmountCurrencyControl.Name = "OtherTaxAmountCurrencyControl";
			this.OtherTaxAmountCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.OtherTaxAmountCurrencyControl.TabIndex = 7;
			// 
			// MarksAndNosLabel
			// 
			this.MarksAndNosLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MarksAndNosLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 118, true);
			this.MarksAndNosLabel.Name = "MarksAndNosLabel";
			this.MarksAndNosLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 16, true);
			this.MarksAndNosLabel.TabIndex = 14;
			this.MarksAndNosLabel.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("SGInvoiceLineUserControl|992E25AD-AD96-4A8E-90E6-315B3FD186A5", "Marks and Numbers");
			this.MarksAndNosLabel.UseMnemonic = false;
			// 
			// SGInvoiceLineUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "SGInvoiceLineUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 584, true);
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
			this.AdditionalDetailsTabPage.ResumeLayout(false);
			this.AdditionalDetailsTabPage.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.zTabPage4.ResumeLayout(false);
			this.zTabPage4.PerformLayout();
			this.zGroupBox4.ResumeLayout(false);
			this.zGroupBox4.PerformLayout();
			this.zDateEdit2.ResumeLayout(true);
			this.zDateEdit2.PerformLayout();
			this.zCalcDropEdit8.ResumeLayout(true);
			this.zCalcDropEdit8.PerformLayout();
			this.zGroupBox7.ResumeLayout(false);
			this.zGroupBox7.PerformLayout();
			this.zDropEdit1.ResumeLayout(true);
			this.zDropEdit1.PerformLayout();
			this.zDropEdit2.ResumeLayout(true);
			this.zDropEdit2.PerformLayout();
			this.zGroupBox8.ResumeLayout(false);
			this.zGroupBox8.PerformLayout();
			this.StrategicGoodsQtyCalcDropEdit.ResumeLayout(true);
			this.StrategicGoodsQtyCalcDropEdit.PerformLayout();
			this.StrategicGoodsCategoryDropEdit.ResumeLayout(true);
			this.StrategicGoodsCategoryDropEdit.PerformLayout();
			this.StrategicGoodsProductCodeDropEdit.ResumeLayout(true);
			this.StrategicGoodsProductCodeDropEdit.PerformLayout();
			this.zDropEdit6.ResumeLayout(true);
			this.zDropEdit6.PerformLayout();
			this.zDropEdit5.ResumeLayout(true);
			this.zDropEdit5.PerformLayout();
			this.zDropEdit4.ResumeLayout(true);
			this.zDropEdit4.PerformLayout();
			this.zTabPage1.ResumeLayout(false);
			this.zTabPage1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid2)).EndInit();
			this.zGrid2.ResumeLayout(false);
			this.zGrid2.PerformLayout();
			this.zTabPage2.ResumeLayout(false);
			this.zTabPage2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid3)).EndInit();
			this.zGrid3.ResumeLayout(false);
			this.zGrid3.PerformLayout();
			this.zTabPage3.ResumeLayout(false);
			this.zTabPage3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid4)).EndInit();
			this.zGrid4.ResumeLayout(false);
			this.zGrid4.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zCalcDropEdit9.ResumeLayout(true);
			this.zCalcDropEdit9.PerformLayout();
			this.zCalcDropEdit10.ResumeLayout(true);
			this.zCalcDropEdit10.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.zCalcDropEdit7.ResumeLayout(true);
			this.zCalcDropEdit7.PerformLayout();
			this.zCalcDropEdit5.ResumeLayout(true);
			this.zCalcDropEdit5.PerformLayout();
			this.zCalcDropEdit4.ResumeLayout(true);
			this.zCalcDropEdit4.PerformLayout();
			this.zCalcDropEdit6.ResumeLayout(true);
			this.zCalcDropEdit6.PerformLayout();
			this.zDropEditPref.ResumeLayout(true);
			this.zDropEditPref.PerformLayout();
			this.CertificateOfOriginTabPage.ResumeLayout(false);
			this.CertificateOfOriginTabPage.PerformLayout();
			this.zPanel4.ResumeLayout(false);
			this.zPanel4.PerformLayout();
			this.zGroupBox5.ResumeLayout(false);
			this.zGroupBox5.PerformLayout();
			this.OriginCriterionDropEdit.ResumeLayout(true);
			this.OriginCriterionDropEdit.PerformLayout();
			this.zCalcDropEdit3.ResumeLayout(true);
			this.zCalcDropEdit3.PerformLayout();
			this.zDateEdit1.ResumeLayout(true);
			this.zDateEdit1.PerformLayout();
			this.zCalcDropEdit2.ResumeLayout(true);
			this.zCalcDropEdit2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.zDropEdit3.ResumeLayout(true);
			this.zDropEdit3.PerformLayout();
			this.convertToLocalCurrencyControl1.ResumeLayout(true);
			this.convertToLocalCurrencyControl1.PerformLayout();
			this.SGTariffFindBox.ResumeLayout(true);
			this.SGTariffFindBox.PerformLayout();
			this.OtherTaxAmountCurrencyControl.ResumeLayout(true);
			this.OtherTaxAmountCurrencyControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage AdditionalDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel2;
		public Enterprise.ZArchitecture.GUI.ZTabPage CertificateOfOriginTabPage;
		private Enterprise.ZArchitecture.GUI.ZPanel zPanel4;
		private Enterprise.ZArchitecture.ZTextBox zTextBox3;
		private Enterprise.ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox8;
		private Enterprise.ZArchitecture.ZTextBox zTextBox4;
		private Enterprise.ZArchitecture.ZGrid zGrid1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit3;
		private Enterprise.ZArchitecture.ZTextBox JI_OutwardMAWBTextBox;
		private Enterprise.ZArchitecture.ZTextBox zTextBox6;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox2;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox5;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit2;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit3;
		private Enterprise.ZArchitecture.ZTextBox zTextBox11;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit3;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit2;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox10;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit7;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit6;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit5;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit4;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit10;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit9;
		private Enterprise.ZArchitecture.ZLabel zLabel5;
		private Enterprise.ZArchitecture.ZLabel zLabel6;
		internal Enterprise.ZArchitecture.ZCalcEdit ExciseUnitRateCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit ExcisePercentageRateCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit DutyUnitRateCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit DutyPercentageRateCalcEdit;
		private Enterprise.ZArchitecture.ZTextBox zTextBox5;
		private Enterprise.ZArchitecture.ZTextBox zTextBox7;
		private Enterprise.ZArchitecture.ZTextBox zTextBox16;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit9;
		private Enterprise.ZArchitecture.ZLabel zLabel12;
		private Enterprise.Customs.GUI.ConvertToLocalCurrencyControl convertToLocalCurrencyControl1;
		private Enterprise.ZArchitecture.ZTextBox zTextBox17;
		private Enterprise.ZArchitecture.ZTextBox zTextBox15;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit4;
		private Enterprise.ZArchitecture.ZTextBox zTextBox2;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.ZLabel zLabel4;
		private Enterprise.ZArchitecture.ZLabel zLabel16;
		private Enterprise.ZArchitecture.GUI.ZTabControl zTabControl1;
		private Enterprise.ZArchitecture.GUI.ZTabPage zTabPage4;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox4;
		private Enterprise.ZArchitecture.GUI.ZDateEdit zDateEdit2;
		private Enterprise.ZArchitecture.ZTextBox zTextBox13;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit8;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox7;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit2;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox8;
		private Enterprise.ZArchitecture.ZTextBox zTextBox14;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit6;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit5;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit4;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsStrategicCheckBox;
		private Enterprise.ZArchitecture.GUI.ZTabPage zTabPage1;
		private Enterprise.ZArchitecture.ZGrid zGrid2;
		private Enterprise.ZArchitecture.GUI.ZTabPage zTabPage2;
		private Enterprise.ZArchitecture.ZGrid zGrid3;
		private Enterprise.ZArchitecture.GUI.ZTabPage zTabPage3;
		private Enterprise.ZArchitecture.ZGrid zGrid4;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StrategicGoodsProductCodeDropEdit;
		private Enterprise.ZArchitecture.GUI.ZLinkLabel zStgcGovLink;
		private Enterprise.Customs.SG.V4.GUI.TariffFindBox SGTariffFindBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEditPref;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StrategicGoodsCategoryDropEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit OriginCriterionDropEdit;
		private Customs.GUI.ConvertToLocalCurrencyControl OtherTaxAmountCurrencyControl;
		private ZArchitecture.ZLabel MarksAndNosLabel;
		internal ZArchitecture.ZCalcEdit OtherTaxUnitRateCalcEdit;
		internal ZArchitecture.ZLabel OtherTaxPercentageRateLabel;
		internal ZArchitecture.ZCalcEdit OtherTaxPercentageRateCalcEdit;
		private ZArchitecture.GUI.ZCalcDropEdit StrategicGoodsQtyCalcDropEdit;
	}
}
