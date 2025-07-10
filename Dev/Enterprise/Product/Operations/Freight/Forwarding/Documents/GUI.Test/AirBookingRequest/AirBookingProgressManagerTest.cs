using CargoWise.Application;
using Enterprise.Freight.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.GUI.Testing
{
	sealed class AirBookingProgressManagerTest : TestCase
	{
		public void TestManagerShouldBeAccessibleViaObjectFactory()
		{
			var manager = ObjectFactory.Get<IAirBookingProgressManager>();
			AssertNotNull("Ait Booking progress manager should be accessible via ObjectFactory", manager);
		}
	}
}
