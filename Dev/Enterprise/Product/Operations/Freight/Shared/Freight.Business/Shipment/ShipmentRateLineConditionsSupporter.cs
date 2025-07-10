using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Business
{
	public class ShipmentRateLineConditionsSupporter : RateLineConditionsSupporter
	{
		public ShipmentRateLineConditionsSupporter(CommonShipment shipment)
			: base(shipment) { }

		protected CommonShipment Shipment
		{
			get { return ObjectToWrap as CommonShipment; }
		}

		protected override OrgHeader GetDepartureCFS()
		{
			var address = Shipment.GetDepartureCFSDocAddress;
			return address != null ? address.Organisation : null;
		}

		protected override OrgHeader GetArrivalCFS()
		{
			var address = Shipment.GetArrivalCFSDocAddress;
			return address != null ? address.Organisation : null;
		}

		protected override OrgHeader GetExportBroker()
		{
			return Shipment.ExportBroker;
		}

		protected override OrgHeader GetImportBroker()
		{
			return Shipment.ImportBroker;
		}

		protected override OrgHeader GetSendingAgent()
		{
			var firstConsol = MovementLegComparer.FirstOrDefaultLegForTransportMode(Shipment.Consols.Cast<CommonConsol>(), Shipment.TransportMode);
			return firstConsol != null ? firstConsol.SendingForwarder : null;
		}

		protected override OrgHeader GetReceivingAgent()
		{
			var lastConsol = MovementLegComparer.LastOrDefaultLegForTransportMode(Shipment.Consols.Cast<CommonConsol>(), Shipment.TransportMode);
			return lastConsol != null ? lastConsol.ReceivingForwarder : null;
		}

		protected override OrgHeader GetControllingAgent()
		{
			var address = Shipment.DocAddresses.FindByDocAddressType(DocAddressType.ControllingAgent);
			return address != null ? address.Organisation : null;
		}

		protected override bool GetHasDangerousGoods()
		{
			return Shipment.OuterPackLines.Cast<PackLine>().Any(p => p.UNDGs.Any());
		}
	}
}
