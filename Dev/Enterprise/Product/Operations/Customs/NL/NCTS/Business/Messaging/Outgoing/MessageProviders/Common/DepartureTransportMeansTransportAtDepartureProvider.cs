using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class DepartureTransportMeansTransportAtDepartureProvider : DepartureTransportMeansProvider
{
	public DepartureTransportMeansTransportAtDepartureProvider(NctsCommonMovementHeader moveHeader, int sequenceNumber) : base(moveHeader, sequenceNumber)
	{
		InitializeTypeOfIdentificationRules(
			SeaTransportWithVesselRule,
			SeaTransportRule,
			RailTransportRule,
			RoadTransportRule,
			AirTransportRule,
			InlandWaterwayTransportWithVesselRule,
			InlandWaterwayTransportRule,
			OwnPropulsionRule);
	}

	public override string Nationality => moveHeader.BM_RN_NKTransportAtDepartureCountry;
}
