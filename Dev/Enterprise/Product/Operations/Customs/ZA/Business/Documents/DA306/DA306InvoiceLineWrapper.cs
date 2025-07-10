using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Documents.DocDataObjects
{
	public class DA306InvoiceLineWrapper
	{
		public DA306InvoiceLineWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		public ZString CountryOfOrigin => invoiceLine.CountryOfOrigin.Code;

		public ZDecimal InvoiceQuantity => invoiceLine.JI_InvoiceQuantity;

		public ZString InvoiceQuantityUnit => invoiceLine.JI_InvoiceUQ;

		public ZDecimal Weight => invoiceLine.JI_Weight;

		public ZString WeightUnit => invoiceLine.JI_WeightUQ;

		public ZString Description => invoiceLine.JI_Description;

		public ZString HarmonisedCode => invoiceLine.JI_FormattedTariff;

		public ZDecimal LinePrice => invoiceLine.JI_LinePrice;

		public ZString InvoiceCurrencyCode => invoiceLine.LinePriceRefCurrency.Code;

		public ZDecimal ValueInRand => invoiceLine.JI_Calc_ActualPrice;

		public ZDecimal CustomsDuty => invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate;

		public ZDecimal ATV => invoiceLine.JI_Calc_ATV;

		public ZDecimal VAT => invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate;

		public ZDecimal TotalPayable => CustomsDuty + VAT;
	}
}
