using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ForwardingConsolDataObjectsCollection : IForwardingConsolDataObjectsCollection
	{
		public ForwardingConsolDataObjectsCollection(ForwardingShipment shipment)
		{
			this.shipment = shipment;
			lazyConsols = new Lazy<IDictionary<string, IForwardingConsolDataObject>>(CreateConsols);
		}

		readonly ForwardingShipment shipment;
		readonly Lazy<IDictionary<string, IForwardingConsolDataObject>> lazyConsols;

		public IEnumerator<IForwardingConsolDataObject> GetEnumerator() => lazyConsols.Value.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count => lazyConsols.Value.Count;

		public IForwardingConsolDataObject Departure
		{
			get
			{
				var key = shipment.MostInterestingDepartureConsol?.JK_UniqueConsignRef ?? ZString.Empty;

				if (!key.IsEmpty && lazyConsols.Value.TryGetValue(key, out var mostInterestingDepartureConsol))
				{
					return mostInterestingDepartureConsol;
				}

				var candidateConsols = shipment.CandidateConsolsForDepartureArrival.Cast<ForwardingConsol>().ToArray();
				foreach (var commonConsol in candidateConsols)
				{
					var candidateDepartureConsol = commonConsol;
					if (candidateDepartureConsol == null || !lazyConsols.Value.TryGetValue(candidateDepartureConsol?.JK_UniqueConsignRef ?? ZString.Empty, out var newConsol))
					{
						continue;
					}

					bool candidateSuits = true;
					foreach (var consol in candidateConsols)
					{
						if (candidateDepartureConsol.JK_RL_NKLoadPort == consol.DischargePort.Code)
						{
							candidateSuits = false;
							break;
						}
					}

					if (candidateSuits)
					{
						return newConsol;
					}
				}

				return null;
			}
		}

		public IForwardingConsolDataObject Arrival
		{
			get
			{
				if (shipment.Consols.Count == 1)
				{
					var key = shipment.Consols[0].JK_UniqueConsignRef;

					if (!key.IsEmpty && lazyConsols.Value.TryGetValue(key, out var onlyConsol))
					{
						return onlyConsol;
					}
				}

				var candidateConsols = shipment.CandidateConsolsForDepartureArrival.Cast<ForwardingConsol>().ToArray();
				foreach (var candidateArrivalConsol in candidateConsols)
				{
					if (candidateArrivalConsol == null || !lazyConsols.Value.TryGetValue(candidateArrivalConsol?.JK_UniqueConsignRef ?? ZString.Empty, out var newConsol))
					{
						continue;
					}

					bool candidateSuits = true;
					foreach (var consol in candidateConsols)
					{
						if (candidateArrivalConsol.JK_RL_NKDischargePort == consol.LoadPort.Code)
						{
							candidateSuits = false;
							break;
						}
					}

					if (candidateSuits)
					{
						return newConsol;
					}
				}

				return null;
			}
		}

		IDictionary<string, IForwardingConsolDataObject> CreateConsols()
		{
			var result = new Dictionary<string, IForwardingConsolDataObject>();
			var builder = new ForwardingConsolDataObjectBuilder();
			var orderedConsols = shipment.TransportsInLegOrder
				.Where(x => x.Parent != null && x.JW_ParentType == Core.Constants.TransportParentTypes.Consol)
				.Select(x => x.Parent)
				.OfType<ForwardingConsol>()
				.Distinct();

			orderedConsols.ForEach(consol => result.Add(consol.JK_UniqueConsignRef, builder.Build(consol)));

			return result;
		}
	}
}
