using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class DangerousGoodsProvider : IDangerousGoods
{
	readonly UNDGDataItem dangerousGood;
	public DangerousGoodsProvider(UNDGDataItem dangerousGood, int sequenceNumber)
	{
		this.dangerousGood = Argument.NotNull(dangerousGood, nameof(dangerousGood));
		SequenceNumeric = sequenceNumber;
	}

	public int SequenceNumeric { get; }

	public string Undgid => dangerousGood.Substance?.DG_UNNO;
}
