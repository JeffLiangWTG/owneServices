using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class DepartureTransportMeansAircraftIDAtDepartureProvider : DepartureTransportMeansProvider
{
	public DepartureTransportMeansAircraftIDAtDepartureProvider(NctsCommonMovementHeader moveHeader, int sequenceNumber) : base(moveHeader, sequenceNumber)
	{
		InitializeTypeOfIdentificationRules(
			SeaTransportRule,
			AirTransportRule,
			InlandWaterwayTransportRule,
			OwnPropulsionRule);
	}

	public override string Id => moveHeader.BM_AircraftIDAtDeparture;

	public override string Nationality => moveHeader.BM_RN_NKTransportAtDepartureCountry;

	protected override bool IsMainTransportID => false;
}
