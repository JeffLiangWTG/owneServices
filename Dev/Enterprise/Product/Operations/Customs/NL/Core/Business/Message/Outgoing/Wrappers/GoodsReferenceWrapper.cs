using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.Business;

public class GoodsReferenceWrapper : IGoodsReference
{
	public GoodsReferenceWrapper(CusContainerInvoiceLinePivot containerInvLinePivot, int sequenceNumeric)
	{
		this.containerInvLinePivot = Argument.NotNull(containerInvLinePivot, nameof(containerInvLinePivot));
		this.SequenceNumeric = sequenceNumeric;
	}
	readonly CusContainerInvoiceLinePivot containerInvLinePivot;

	public int SequenceNumeric { get; }

	int goodsItemNumber;
	public int GoodsItemNumericValue
	{
		get
		{
			int.TryParse(containerInvLinePivot.InvoiceLine.JI_Calc_MergedLineNumber, out goodsItemNumber);
			return goodsItemNumber;
		}
	}
}
