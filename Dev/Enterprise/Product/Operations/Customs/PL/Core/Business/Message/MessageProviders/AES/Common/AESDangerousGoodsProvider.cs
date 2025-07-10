using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class AESDangerousGoodsProvider : IDangerousGoods
{
	public AESDangerousGoodsProvider(UNDGSubstance substance, int sequenceNumber)
	{
		this.substance = Argument.NotNull(substance, nameof(substance));
		this.sequenceNumber = sequenceNumber;
	}

	readonly UNDGSubstance substance;
	readonly int sequenceNumber;

	public int SequenceNumber => sequenceNumber;
	public string UNNumber => substance.DG_Code;
}
