using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.OceanCarrier.Business
{
	sealed class CarrierShipmentHeaderInvoicingSupporter : JobInvoicingSupporter
	{
		public CarrierShipmentHeaderInvoicingSupporter(CarrierShipmentHeader shipment)
			: base(shipment)
		{
			this.shipment = shipment;
		}

		readonly CarrierShipmentHeader shipment;

		public override ZString HouseBillNumber => shipment.CSH_HouseBill;

		public override JobInvoicingConsumerType ConsumerType => JobInvoicingConsumerTypes.AgencyBooking;

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore() => Env.Security.AgencyBookingJobInvoicing;
	}
}
