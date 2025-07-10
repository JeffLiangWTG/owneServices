using System;
using Enterprise.Packing.Module.Testing;
using Enterprise.TransportBookings.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.TransportBookings.Module.Testing
{
	public class MasterBookingPackingPlugInTest : PackingPlugInTest
	{
		protected override Type ExpectedUserControlType => typeof(MasterBookingPackingUserControl);

		protected override ZPlugIn GetPlugInToTest()
		{
			Data.CreatePackingData();
			return new MasterBookingPackingPlugIn(Data.Dummy);
		}
	}
}
