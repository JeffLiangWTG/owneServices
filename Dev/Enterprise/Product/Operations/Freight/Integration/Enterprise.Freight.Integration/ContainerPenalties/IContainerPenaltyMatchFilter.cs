using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Integration
{
	public interface IContainerPenaltyMatchFilter
	{
		IGlbCompany Company { get; }
		IOrgHeader Carrier { get; }
		IOrgHeader CTO { get; }
		IOrgHeader Client { get; }
		ICommonContainer Container { get; }
		ZString ProcessType { get; }
		ZString CreditorType { get; set; }
		ZString ContainerClass { get; }
		ZString OriginPort { get; }
		ZString DetentionPort { get; }
		ZString Direction { get; }
		GetClientsDelegate GetClientsFunction { get; }

		IReadOnlyCollection<ICommonShipment> Shipments { get; }
	}

	public delegate IReadOnlyCollection<IOrgHeader> GetClientsDelegate(ICommonShipment shipment, ZString direction);
}
