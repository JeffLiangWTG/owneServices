using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC015CConsignmentProvider : ConsignmentProvider
{
	public CC015CConsignmentProvider(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	protected override INCTSParty ConsignorCore => ReducedDatasetIndicator ? null : base.ConsignorCore;

	protected override int? InlandModeOfTransportCore => ReducedDatasetIndicator ? null : base.InlandModeOfTransportCore;
}
