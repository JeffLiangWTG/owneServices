using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class SealWrapper : ISeal
{
	public SealWrapper(CusSeal seal, int sequenceNumeric)
		: this(Argument.NotNull(seal, nameof(seal)).BK_SealNumber, sequenceNumeric)
	{
	}

	public SealWrapper(string sealNo, int sequenceNumeric)
	{
		_ = Argument.NotNull(sealNo, nameof(sealNo));
		Id = sealNo;
		SequenceNumeric = sequenceNumeric;
	}

	public int SequenceNumeric { get; }

	public string Id { get; }
}
