using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.TransportBookings.Module
{
	public class DtbBookingTabPlugInController : DtbBookingControllerShared
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.DtbBookingTabPlugIn; }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new DtbBookingTabPlugIn(businessEntity);
		}

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|TransportBooking", "Transport Bookings");
	}
}
