using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.Business
{
	public class GteVehicleMovementBookingCollection : DependentBusinessObjectCollection<GteVehicleMovementBooking, GteBooking>
	{
		public GteVehicleMovementBookingCollection(GteBooking booking)
			: base(booking)
		{ }

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return GteVehicleMovementBookingSchema.GBV_GBK_Booking; }
		}
	}
}
