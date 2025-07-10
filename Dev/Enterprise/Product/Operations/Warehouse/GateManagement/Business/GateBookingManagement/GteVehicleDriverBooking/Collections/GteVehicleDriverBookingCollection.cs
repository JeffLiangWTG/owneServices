using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteVehicleDriverBookingCollection : DependentBusinessObjectCollection<GteVehicleDriverBooking, GteBooking>
	{
		public GteVehicleDriverBookingCollection(GteBooking booking)
			: base(booking)
		{ }

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return GteVehicleDriverBookingSchema.GBD_GBK_Booking; }
		}
	}
}
