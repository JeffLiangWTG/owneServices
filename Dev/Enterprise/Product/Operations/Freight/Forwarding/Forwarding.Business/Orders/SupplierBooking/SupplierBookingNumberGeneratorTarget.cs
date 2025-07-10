using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	internal class SupplierBookingNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get => OrderManagerRegistry.Instance.SupplierBookingNumberFormat.Location();
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(OrderManagerRegistry.Instance.SupplierBookingNumberFormat);
		}

		protected override int GetMaxLengthCore()
		{
			return JobSupplierBookingSchema.JSB_BookingId.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("f54984e7-36c7-4a56-9f6d-0cb0d30b27ea", "Supplier Booking Number Format");
		}
	}
}
