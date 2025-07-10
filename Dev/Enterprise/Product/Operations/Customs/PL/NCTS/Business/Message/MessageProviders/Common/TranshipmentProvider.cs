using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class TranshipmentProvider : ITranshipment
{
	readonly EnRouteIncident incident;

	public TranshipmentProvider(EnRouteIncident incident)
	{
		this.incident = Argument.NotNull(incident, nameof(incident));
	}

	public NCTSIndicator ContainerIndicator => incident.IncidentContainers.Any(x => x.BC_Mode == Core.Constants.ContainerModes.Containerised) ? NCTSIndicator.YES : NCTSIndicator.NO;

	public ITransportMeans TransportMeans => CachedValueHelper.GetValue(ref transportMeans, () => !AllElementsAreEmpty
		? new TransportMeansProvider(incident.BN_TransportAtDepartureType, incident.BN_TransportAtDepartureID, incident.BN_RN_NKTransportAtDepartureIDNationality)
		: null);
	CachedValue<ITransportMeans> transportMeans;

	bool AllElementsAreEmpty => incident.BN_TransportAtDepartureType.IsEmpty
								&& incident.BN_TransportAtDepartureID.IsEmpty
								&& incident.BN_RN_NKTransportAtDepartureIDNationality.IsEmpty;
}
