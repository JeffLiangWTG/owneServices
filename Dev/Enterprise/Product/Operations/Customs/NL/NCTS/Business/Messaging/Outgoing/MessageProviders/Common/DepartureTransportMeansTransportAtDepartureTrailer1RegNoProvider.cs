using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider : DepartureTransportMeansProvider
{
	public DepartureTransportMeansTransportAtDepartureTrailer1RegNoProvider(NctsCommonMovementHeader moveHeader, int sequenceNumber) : base(moveHeader, sequenceNumber)
	{
		InitializeTypeOfIdentificationRules(
			SeaTransportRule,
			RailTransportRule,
			RoadTransportRule,
			InlandWaterwayTransportRule,
			OwnPropulsionRule);
	}

	public override string Id => moveHeader.BM_TransportAtDepartureTrailer1RegNo;

	public override string Nationality => moveHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality;

	protected override bool IsMainTransportID => false;
}
