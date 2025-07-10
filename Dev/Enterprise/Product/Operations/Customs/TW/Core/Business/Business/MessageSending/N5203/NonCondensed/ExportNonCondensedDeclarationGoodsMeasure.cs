using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class ExportNonCondensedDeclarationGoodsMeasure : N5203.GoodsMeasure
	{
		public ExportNonCondensedDeclarationGoodsMeasure(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine) : base(cusEntryLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		readonly JobComInvoiceLine invoiceLine;

		public override ZDecimal NetWeightMeasure => invoiceLine.NetWeightInKG;

		public override ZDecimal TariffQuantity => invoiceLine.JI_InvoiceQuantity;

		public override ZString UnitCode => invoiceLine.JI_InvoiceUQ;
	}
}
