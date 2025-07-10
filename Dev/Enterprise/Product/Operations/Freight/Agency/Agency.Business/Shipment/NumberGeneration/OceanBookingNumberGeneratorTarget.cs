using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class OceanBookingNumberGeneratorTarget : NumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return (NoResString)"Liner & Agency -> Booking Number Customization"; } // points to non-localised registry entry
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(AgencyRegistry.Instance.BookingNumberCustomisation);
		}

		protected override int GetMaxLengthCore()
		{
			return JobShipmentSchema.JS_CFSReference.MaxLength;
		}

		protected override ZString GetNameCore()
		{
			return Res.GetString("79c3b50d-423e-4360-b44f-1d890f163c4b", "booking number");
		}
	}
}
