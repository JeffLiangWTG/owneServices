using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class InvoiceHeaderUserControl : Customs.GUI.InvoiceHeaderUserControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ChargesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).BeginInit();
			this.ShipmentTypeGroupBox.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// InvoiceChargesGrid
			// 
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("8CA2AD70-00D3-44EA-A89C-85E7D383335C", "Included In Invoice Amount");
			zCheckBoxColumnStyleInfo1.ColumnName = "J7_Calc_IsIncludedInInvoiceAmount";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.ToolTip = "If you tick this flag, it indicates that this charge is included in Invoice Amoun" +
				"t. If this is ticked, then Included in Lines should be true.";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "NoOfDecimalsForPercentage";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("B55941B8-77AE-4B10-82DD-BCC231B6D753", "% of Line Price");
			zCalcEditColumnStyleInfo1.ColumnName = "J7_Percentage";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.ToolTip = "You can enter percentage only for Commission and Discount. This value will be def" +
				"aulted to all invoice lines and amount will be calculated based on Line Price.";
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ChargeDistributionBy";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("7F3CFC14-F54D-4D63-8A16-E6F344737282", "Distribute By");
			zDropEditColumnStyleInfo1.ColumnName = "J7_DistributeBy";
			this.InvoiceChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.InvoiceChargesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InvoiceChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).Lookups.ChargeTypeList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_ChargeTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_ChargeType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).ChargeCodeDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).ChargeCodeDescription)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_Amount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_AmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).Lookups.Currencies)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_RX_NKCurrencyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_RX_NKCurrency)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_IsDutiable)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_IsDutiableInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_IsGSTApplicable)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_IsGSTApplicableInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_IsIncludedInITOT)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_IsIncludedInITOTInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_Calc_IsIncludedInInvoiceAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_Calc_IsIncludedInInvoiceAmountInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).Lookups.PrepaidCollectList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_PrepaidCollectInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_PrepaidCollect)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).NoOfDecimalsForPercentage)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).NoOfDecimalsForPercentageInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_Percentage)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).J7_PercentageInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Customs.Business.BaseInvoiceCharge)(((object)(((Customs.Business.BaseJobComInvoiceHeader)(((object)(((Customs.Business.BaseJobDeclaration)(null)).Invoices)))).Charges)))).Lookups.ChargeDistributionBy)));
			// 
			// InvoiceHeaderUserControl
			// 
			this.Name = "InvoiceHeaderUserControl";
			this.ChargesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InvoiceChargesGrid)).EndInit();
			this.ShipmentTypeGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
