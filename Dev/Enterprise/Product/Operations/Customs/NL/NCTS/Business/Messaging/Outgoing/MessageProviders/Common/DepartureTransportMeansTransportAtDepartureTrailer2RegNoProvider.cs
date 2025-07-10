using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider : DepartureTransportMeansProvider
{
	public DepartureTransportMeansTransportAtDepartureTrailer2RegNoProvider(NctsCommonMovementHeader moveHeader, int sequenceNumber) : base(moveHeader, sequenceNumber)
	{
		InitializeTypeOfIdentificationRules(
			SeaTransportRule,
			RoadTransportRule,
			InlandWaterwayTransportRule,
			OwnPropulsionRule);
	}

	public override string Id => moveHeader.BM_TransportAtDepartureTrailer2RegNo;

	public override string Nationality => moveHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality;

	protected override bool IsMainTransportID => false;
}
