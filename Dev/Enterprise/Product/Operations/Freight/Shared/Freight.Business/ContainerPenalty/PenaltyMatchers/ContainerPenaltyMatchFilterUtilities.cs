using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public static class ContainerPenaltyMatchFilterUtilities
	{
		public static IReadOnlyCollection<IOrgHeader> GetClients(IContainerPenaltyMatchFilter filter)
		{
			var clients = new HashSet<IOrgHeader>();

			var filterClient = filter.Client;
			if (filterClient != null)
			{
				clients.Add(filterClient);
			}

			var shipments = filter.Shipments;

			if (shipments == null || !shipments.Any())
			{
				clients.UnionWith(GetClients(filter, null));
				return clients;
			}

			var allShipmentClients = shipments.SelectMany(shipment => GetClients(filter, shipment));
			clients.UnionWith(allShipmentClients);
			
			return clients;
		}

		public static IReadOnlyCollection<IOrgHeader> GetClients(IContainerPenaltyMatchFilter filter, ICommonShipment shipment)
		{
			return filter.GetClientsFunction?.Invoke(shipment, filter.Direction) ?? Array.Empty<IOrgHeader>();
		}
	}
}
