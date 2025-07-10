using Enterprise.Packing.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(MasterBookingPackingPlugInController))]
	public class MasterBookingPackingPlugInControllerTest : PackingPlugInControllerTest
	{
		public override void TestID()
		{
			AssertEquals(ControllerIDs.MasterBookingPackingPlugIn, new MasterBookingPackingPlugInController().ID);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.MasterBookingPackingPlugIn;
		}
	}
}
