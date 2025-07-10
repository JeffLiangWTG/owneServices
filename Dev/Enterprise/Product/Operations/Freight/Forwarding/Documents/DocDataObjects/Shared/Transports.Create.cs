using System.Collections.Generic;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed partial class Transports
	{
		public static Transports Create(IContext context, IEnumerable<Freight.Business.Transport> transportBizObjs, ITransportParent parent = null)
		{
			var result = new Transports();
			var transportCollection = new List<Transport>();

			if (transportBizObjs != null)
			{
				int number = 0;

				foreach (var transportBizObj in transportBizObjs)
				{
					var transport = Transport.Create(context, transportBizObj);
					transport.LegOrder = ++number;

					transportCollection.Add(transport);
					result.RegisterEditableChildObject(transport);

					switch (transportBizObj.JW_TransportType)
					{
						case Core.Constants.TransportPlanningType.PreCarriage:
							if (result.PreCarriage == null)
							{
								result.PreCarriage = transport;
							}
							break;

						case Core.Constants.TransportPlanningType.MainVessel:
							if (parent is ForwardingShipment shipment
								&& transportBizObj.IsLegFromNonDirectConsolAttachedToDirectShipment(shipment)) // // WI00495056 - Attach DRT shipments to AGT Consols, Main Leg of DRT Shipment should be from DRT Consol.
							{
								break;
							}
							if (result.Main == null)
							{
								result.Main = transport;
							}
							break;

						case Core.Constants.TransportPlanningType.OnForwarding:
							if (result.OnForwarding == null)
							{
								result.OnForwarding = transport;
							}
							break;
					}
				}
			}

			result.Collection = transportCollection;

			return result;
		}

		public static Transports Create(IEnumerable<Transport> transports)
		{
			var result = new Transports();
			var transportCollection = new List<Transport>();

			if (transports != null)
			{
				foreach (var transport in transports)
				{
					transportCollection.Add(transport);
					result.RegisterEditableChildObject(transport);
				}
			}

			result.Collection = transportCollection;

			return result;
		}
	}
}
