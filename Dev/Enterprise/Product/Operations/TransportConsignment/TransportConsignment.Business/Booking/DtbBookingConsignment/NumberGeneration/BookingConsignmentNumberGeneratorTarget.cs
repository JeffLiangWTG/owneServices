using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportConsignment.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportConsignment.Business
{
	internal sealed class BookingConsignmentNumberGeneratorTarget : TransportNumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return (NoResString)"Transport -> Land & Port Transport -> Land Transport -> Transport Consignment Number Format"; } // Points to non-localised registry entry
		}

		public BookingConsignmentNumberGeneratorTarget(DtbBookingConsignment parent)
			: base(parent)
		{
		}

		public override BillCustomisationByServiceLevelRegistryItem GetRegistryItemCore()
		{
			return LandTransportRegistry.Instance.TransportConsignmentNumberFormat;
		}

		protected override CargoWise.Types.ZString GetNameCore()
		{
			return Res.GetString("57a6d0aa-bsbs-4325-9935-af18606a1ac0", "consignment number");
		}
	}
}
