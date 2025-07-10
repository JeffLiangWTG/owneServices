using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class CC013CTransitOperationProvider : TransitOperationProvider
{
	public CC013CTransitOperationProvider(NctsHeader header) : base(header)
	{
	}

	public override string LRN => MRN.IsEmpty() ? header.MovementHeader.BM_PaperlessInbondNum : null;
}
