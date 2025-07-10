using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class IncidentProvider : INCTSIncident
{
	readonly EnRouteIncident incident;
	public IncidentProvider(EnRouteIncident incident, int sequence)
	{
		this.incident = incident;
		SequenceNumeric = sequence;
	}

	public int SequenceNumeric { get; }

	public string Code => incident.BN_IncidentCode;

	public string Text => incident.BN_Information;

	public INCTSEndorsement Endorsement => endorsement ??= new EndorsementProvider(incident);
	INCTSEndorsement endorsement;

	public INCTSLocation Location => location ??= new LocationForIncidentsProvider(incident);
	INCTSLocation location;

	public IReadOnlyCollection<CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment> TransportEquipments => Code.In(IncidentCodeList.Codes._2, IncidentCodeList.Codes._3, IncidentCodeList.Codes._4, IncidentCodeList.Codes._6)
			? transportEquipments ??= incident.IncidentContainers.Cast<NctsContainer>().Select((ctr, index) => new TransportEquipmentsForNCTSContainerProvider(ctr, index + 1)).ToArray()
			: transportEquipments ??= Array.Empty<TransportEquipmentsForNCTSContainerProvider>();
	IReadOnlyCollection<CargoWise.Customs.NL.MessageContracts.Interfaces.INCTSTransportEquipment> transportEquipments;

	public ITranshipment Transhipment => Code.In(IncidentCodeList.Codes._3, IncidentCodeList.Codes._4)
		? transhipment ??= new TranshipmentProvider(incident)
		: null;
	ITranshipment transhipment;
}
