using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class AdditionalSupplyChainActorProvider : IAdditionalSupplyChainActor
{
	public AdditionalSupplyChainActorProvider(CusReference reference, int sequenceNumber)
	{
		this.reference = Argument.NotNull(reference, nameof(reference));
		SequenceNumeric = sequenceNumber;
	}
	readonly CusReference reference;

	public string Id => reference.CFR_Reference;

	public string Role => reference.CFR_Code;

	public int SequenceNumeric { get; }
}
