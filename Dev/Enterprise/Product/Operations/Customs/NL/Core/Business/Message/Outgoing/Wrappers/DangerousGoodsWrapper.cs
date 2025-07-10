using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business;

public class DangerousGoodsWrapper : IDangerousGoods
{
	public DangerousGoodsWrapper(UNDGDataItem dataItem, int sequenceNumeric)
	{
		this.SequenceNumeric = sequenceNumeric;
		this.dataItem = Argument.NotNull(dataItem, nameof(dataItem));
	}
	readonly UNDGDataItem dataItem;

	public int SequenceNumeric { get; }

	public string Undgid => dataItem.UNDGSubstance.DG_UNNO;
}
