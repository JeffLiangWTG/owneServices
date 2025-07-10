using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;

namespace Enterprise.Customs.TW.Business
{
	class ExportNonCondensedDeclarationGoodsStatisticalMeasure : GoodsStatisticalMeasure
	{
		public ExportNonCondensedDeclarationGoodsStatisticalMeasure(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine) : base(cusEntryLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
		}
		readonly JobComInvoiceLine invoiceLine;

		public override ZString StatisticalUnitCode => invoiceLine.JI_CustomsSecondUnitQty;

		public override ZDecimal TariffQuantity => invoiceLine.JI_CustomsSecondQuantity;
	}
}
