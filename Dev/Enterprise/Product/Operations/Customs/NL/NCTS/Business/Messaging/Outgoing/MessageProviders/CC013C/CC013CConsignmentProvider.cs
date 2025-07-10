using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC013CConsignmentProvider : ConsignmentProvider
{
	public CC013CConsignmentProvider(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override INCTSParty ConsignorCore => ReducedDatasetIndicator ? null : base.ConsignorCore;

	protected override int? InlandModeOfTransportCore => ReducedDatasetIndicator ? null : base.InlandModeOfTransportCore;
}
