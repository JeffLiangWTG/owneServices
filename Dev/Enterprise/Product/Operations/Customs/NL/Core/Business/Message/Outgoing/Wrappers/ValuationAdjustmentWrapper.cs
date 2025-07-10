using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used later")]
public class ValuationAdjustmentWrapper : IValuationAdjustment
{
	public ValuationAdjustmentWrapper(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = invoiceLine;
	}
	readonly JobComInvoiceLine invoiceLine;

	public string AdditionCode
	{
		get
		{
			if (invoiceLine.JI_ValuationCode == "1")
			{
				var invoiceHeader = invoiceLine.InvoiceHeader;
				var flag1 = (invoiceHeader?.RelatedIndicator ?? ZBool.False) || invoiceLine.RelatedIndicator;
				var flag2 = (invoiceHeader?.RelatedIndicator2 ?? ZBool.False) || invoiceLine.RelatedIndicator2;
				var flag3 = (invoiceHeader?.RelatedIndicator3 ?? ZBool.False) || invoiceLine.RelatedIndicator3;
				var flag4 = (invoiceHeader?.RelatedIndicator4 ?? ZBool.False) || invoiceLine.RelatedIndicator4;
				return System.FormattableString.Invariant($"{BoolToBit(flag1)}{BoolToBit(flag2)}{BoolToBit(flag3)}{BoolToBit(flag4)}");
			}

			return ZString.Empty;
		}
	}
	static ZString BoolToBit(ZBool flag) => flag ? "1" : "0";
}
