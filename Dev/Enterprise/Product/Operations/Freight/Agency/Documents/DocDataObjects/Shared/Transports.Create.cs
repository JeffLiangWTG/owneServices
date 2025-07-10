using System.Collections.Generic;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public partial class Transports
	{
		public static Transports Create(IContext context, IEnumerable<Freight.Business.Transport> transportBizObjs)
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
