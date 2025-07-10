using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ForwardingConsolDataObjectBuilder
	{
		public ForwardingConsolDataObject Build(ForwardingConsol consol)
		{
			if (consol == null)
			{
				return null;
			}

			var context = new CommonContext(consol.Factory);
			var transports = GetSortedTransports(consol);

			var dataObject = new ForwardingConsolDataObject(consol.PK)
			{
				ConsolNumber = consol.JK_UniqueConsignRef,
				TransportMode = new CodeDescription(context.TransportModes) { Code = consol.JK_TransportMode },
				LoadPort = Unloco.Create(context, consol.LoadPort),
				DischargePort = Unloco.Create(context, consol.DischargePort),
				DepartureCFS = AddressBuilder.Create(context, consol.GetDepartureCFSDocAddress),
				ArrivalCFS = AddressBuilder.Create(context, consol.GetArrivalCFSDocAddress),
				PortOfFirstLoading = Unloco.Create(context, transports.FirstOrDefault()?.LoadPort),
				PortOfLastDischarge = Unloco.Create(context, transports.LastOrDefault()?.DiscPort),
				PlaceOfReceipt = Unloco.Create(context, consol.LoadPort),
				PlaceOfDelivery = Unloco.Create(context, consol.DischargePort),
				FirstVoyageFlightNumber = transports.FirstOrDefault()?.JW_VoyageFlight ?? ZString.Empty,
				Transports = Transports.Create(context, transports)
			};

			return dataObject;
		}

		IReadOnlyCollection<Freight.Business.Transport> GetSortedTransports(ForwardingConsol consol)
		{
			consol.Transports.Sort(MovementLegComparer.PortsAndDatesBased(consol.Transports));
			return consol
				.Transports
				.OfType<Freight.Business.Transport>()
				.ToArray();
		}
	}
}
