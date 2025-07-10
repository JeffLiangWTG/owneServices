using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class CC013015CAdditionalSupplyChainActorProvider : IAdditionalSupplyChainActor
{
	public CC013015CAdditionalSupplyChainActorProvider(int sequenceNumber, CusSupplyChainActorReference cusSupplyChainActorReference)
	{
		this.cusSupplyChainActorReference = Argument.NotNull(cusSupplyChainActorReference, nameof(cusSupplyChainActorReference));
		SequenceNumber = sequenceNumber;
	}
	readonly CusSupplyChainActorReference cusSupplyChainActorReference;

	public int SequenceNumber { get; }

	public string Role => cusSupplyChainActorReference.CFR_Code;

	public string IdentificationNumber => cusSupplyChainActorReference.CFR_Reference;
}
