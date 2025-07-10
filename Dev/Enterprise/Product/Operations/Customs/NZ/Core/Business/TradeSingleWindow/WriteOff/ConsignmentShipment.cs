using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow
{
	class ConsignmentShipment : Consignment
	{
		public ConsignmentShipment(WriteOffResponseConsol response, string id, string status)
			: base(response, id, status, ZString.Empty)
		{
			shipment = response.Consol.Shipments
									  .Cast<ForwardingShipment>()
									  .FirstOrDefault(s => s.JS_HouseBill == id);
		}

		protected override ZString CustomsStatusCore
		{
			get { return StatusProvider != null ? StatusProvider.GetCustomsCargoStatusCode() : ZString.Empty; }
			set
			{
				if (StatusProvider != null)
				{
					StatusProvider.SetCustomsCargoStatusCode(value);
				}
			}
		}

		protected override ZString MessageStatusCore
		{
			get { return StatusProvider != null ? StatusProvider.GetCustomsMessageStatusCode() : ZString.Empty; }
			set
			{
				if (StatusProvider != null)
				{
					StatusProvider.SetCustomsMessageStatusCode(value);
				}
			}
		}

		protected override ZString HouseBillCore
		{
			get { return shipment != null ? shipment.JS_HouseBill : ZString.Empty; }
		}

		protected override string JobNumberCore
		{
			get { return shipment != null ? shipment.JS_UniqueConsignRef.ToString() : base.JobNumberCore; }
		}

		#region Implementation

		readonly ForwardingShipment shipment;

		ForwardingShipmentCustomsStatusProvider StatusProvider
		{
			get
			{
				if (statusProvider == null && shipment != null)
				{
					statusProvider = new ForwardingShipmentCustomsStatusProvider(shipment);
				}
				return statusProvider;
			}
		}
		ForwardingShipmentCustomsStatusProvider statusProvider;

		#endregion // Implementation
	}
}

// Tested in ICRMessageProcessorConsolTest
