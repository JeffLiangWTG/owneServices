using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.Business;

public class GoodsMeasureWrapper : IGoodsMeasure
{
	public GoodsMeasureWrapper(EU.Business.Declaration.CusEntryLine cusEntryLine)
	{
		this.cusEntryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
	}
	readonly EU.Business.Declaration.CusEntryLine cusEntryLine;

	public decimal GrossMassMeasure => cusEntryLine.InvoiceLines.Select(invoiceline => (decimal)invoiceline.JI_Weight).Sum();
	public decimal NetNetWeightMeasure => cusEntryLine.InvoiceLines.Select(invoiceline => (decimal)invoiceline.JI_NetWeight).Sum();
	public decimal? TariffQuantity => cusEntryLine.InvoiceLines.Select(line => (decimal)line.JI_CustomsSecondQuantity).Sum() is var sum && sum == 0 ? null : sum;

	public decimal? SupplementaryUnitsQty => decimal.Zero;
}
