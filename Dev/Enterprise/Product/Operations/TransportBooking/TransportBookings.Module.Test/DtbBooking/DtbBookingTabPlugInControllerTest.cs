using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingTabPlugInController))]
	class DtbBookingTabPlugInControllerTest : DtbBookingControllerSharedTest<DtbBookingTabPlugInController>
	{
		public void TestPluginTabPageCaption()
		{
			AssertEquals("Transport Bookings", new DtbBookingTabPlugInController().PluginTabPageCaption.Caption);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.DtbBookingTabPlugIn;
	}
}
