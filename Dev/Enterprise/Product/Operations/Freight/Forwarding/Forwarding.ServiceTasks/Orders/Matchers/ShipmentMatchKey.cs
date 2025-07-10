using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	public class ShipmentMatchKey
	{
		public ZString LoadMode { get; set; }

		public OrgAddress ConsigneeAddress { get; set; }

		public ForwardingConsol PackedConsol { get; set; }

		public JobSupplierBooking SupplierBooking { get; set; }

		public ForwardingShipment PlannedShipment { get; set; }

		public static ShipmentMatchKey BuildKey(ContainerLoadListLine loadListLine)
		{
			return new ShipmentMatchKey
			{
				PackedConsol = loadListLine.Container.Consol,
				PlannedShipment = GetPlannedShipment(loadListLine),
				LoadMode = loadListLine.CLL_LoadMode,
				SupplierBooking = loadListLine.SupplierBookingLine.SupplierBooking,
				ConsigneeAddress = loadListLine.SupplierBookingLine.SupplierBooking.ConsigneeDocumentaryAddress?.Address ?? loadListLine.SupplierBookingLine.OrderLine.Order.BuyerAddress,
			};
		}

		static ForwardingShipment GetPlannedShipment(ContainerLoadListLine loadListLine)
		{
			var plannedPackLinesQuery = new ZQuery(JobPackLinesSchema.JL_JSL_BookingLine, loadListLine.CLL_JSL_BookingLine);
			plannedPackLinesQuery.OrderBy = JobPackLinesSchema.Constants.JL_SystemCreateTimeUtc;
			var matchedPackLine = loadListLine.Factory.LoadTop1<ForwardingPackLine>(plannedPackLinesQuery);
			return matchedPackLine?.Shipment;
		}
	}
}
