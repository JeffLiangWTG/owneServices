using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class TranshipmentProvider : ITranshipment
{
	readonly EnRouteIncident incident;

	public TranshipmentProvider(EnRouteIncident incident)
	{
		this.incident = Argument.NotNull(incident, nameof(incident));
	}

	public bool ContainerIndicator => incident.IncidentContainers.Count > 0;

	public int? TypeOfIdentification => int.TryParse(incident.BN_TransportAtDepartureType, out var value) ? value : null;

	public string Nationality => incident.BN_RN_NKTransportAtDepartureIDNationality;

	public string Id => incident.BN_TransportAtDepartureID;
}
