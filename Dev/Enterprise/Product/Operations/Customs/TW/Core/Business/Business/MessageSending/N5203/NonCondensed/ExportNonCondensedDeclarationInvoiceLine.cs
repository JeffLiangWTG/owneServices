using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;

namespace Enterprise.Customs.TW.Business
{
	class ExportNonCondensedDeclarationInvoiceLine : CommodityInvoiceLine
	{
		public ExportNonCondensedDeclarationInvoiceLine(CusEntryLine entryLine, JobComInvoiceLine invoiceLine) : base(entryLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		public override ZDecimal UnitPriceAmount => invoiceLine.JI_EnteredUnitPrice;

		public override ZDecimal ItemChargeAmount => invoiceLine.JI_CVAfterRecon;
	}
}
