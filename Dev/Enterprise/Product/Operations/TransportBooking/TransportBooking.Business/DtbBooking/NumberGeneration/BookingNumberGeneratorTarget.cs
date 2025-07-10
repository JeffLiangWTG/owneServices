using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public sealed class BookingNumberGeneratorTarget : NumberGeneratorTarget
	{
		public BookingNumberGeneratorTarget(DtbBooking parent)
			: base()
		{
			this.parent = Argument.NotNull(parent, "parent");
		}

		readonly DtbBooking parent;

		public override string NumberCustomisationLocation
		{
			get { return (NoResString)"Transport -> Transport Bookings -> Transport Booking Number Format"; }
		}

		public BillCustomisationByServiceLevelRegistryItem GetRegistryItemCore()
		{
			return TransportRegistry.Instance.TransportBookingNumberFormat;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("57a6d0aa-bsd3-4325-9935-af18606a1ac0", "booking number");
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			var transportModeCustomisation = Context.AccessRegistryByServiceLevel(GetRegistryItemCore());

			return transportModeCustomisation.BillOfLadingNumberCustomisations[parent.KM_RS_NKServiceLevel]
				?? transportModeCustomisation.BillOfLadingNumberCustomisations["ALL"];
		}

		protected override int GetMaxLengthCore()
		{
			return DtbBookingSchema.KM_JobID.MaxLength;
		}
	}
}
