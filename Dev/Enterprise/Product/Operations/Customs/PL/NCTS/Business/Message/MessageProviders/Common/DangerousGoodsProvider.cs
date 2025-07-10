using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class DangerousGoodsProvider : IDangerousGoods
{
	public DangerousGoodsProvider(int sequenceNumber, UNDGSubstance dangerousSubstance)
	{
		this.dangerousSubstance = Argument.NotNull(dangerousSubstance, nameof(dangerousSubstance));
		SequenceNumber = sequenceNumber.ToString();
	}

	readonly UNDGSubstance dangerousSubstance;

	public string SequenceNumber { get; }

	public string UNNumber => dangerousSubstance.DG_UNNO;
}
