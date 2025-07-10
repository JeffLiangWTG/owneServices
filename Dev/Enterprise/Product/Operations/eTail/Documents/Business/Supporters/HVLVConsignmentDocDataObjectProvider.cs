using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.eTail.Documents.Business
{
	public class HVLVConsignmentDocDataObjectProvider : IForwardingDocDataObjectProvider
	{
		public HVLVConsignmentDocDataObjectProvider(ForwardingShipment shipment)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		public object GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters)
		{
			if (parent is HVLVConsignment consignment && dataContext == DataContext.HVLVConsignment && !consignment.HVC_JS_ManifestedOnShipment.IsEmpty)
			{
				return new US.HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters).Build();
			}

			return null;
		}

		public IEnumerable<HVLVConsignment> GetHVLVConsignmentsWithItemLoadedOnShipment()
		{
			return shipment.GetHVLVConsignmentsWithItemLoadedOnShipment();
		}
	}
}
