using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IncidentProvider : IIncident
{
	readonly EnRouteIncident incident;

	public IncidentProvider(int sequenceNumber, EnRouteIncident routeIncident)
	{
		incident = Argument.NotNull(routeIncident, nameof(routeIncident));
		SequenceNumber = sequenceNumber.ToString();
	}

	public string SequenceNumber { get; }

	public string Code => incident.BN_IncidentCode;

	public string Text => incident.BN_Information;

	public IEndorsement Endorsement => CachedValueHelper.GetValue(ref endorsement, () => EndorsementProvider.NewOrNull(incident));
	CachedValue<IEndorsement> endorsement;

	public ILocation Location => location ?? (location = new IncidentLocationProvider(incident));
	ILocation location;

	public IReadOnlyCollection<ITransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = GetTransportEquipment());
	IReadOnlyCollection<ITransportEquipment> transportEquipment;

	public ITranshipment Transhipment => transhipment ?? (transhipment = new TranshipmentProvider(incident));
	ITranshipment transhipment;

	IReadOnlyCollection<ITransportEquipment> GetTransportEquipment() => incident.IncidentContainers.Cast<NctsContainer>()
		.Select((x, i) => new TransportEquipmentsForArrivalProvider(i + 1, x))
		.ToArray();
}
