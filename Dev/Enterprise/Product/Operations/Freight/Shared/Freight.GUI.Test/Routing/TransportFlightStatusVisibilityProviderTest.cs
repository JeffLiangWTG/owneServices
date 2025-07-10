using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class TransportFlightStatusVisibilityProviderTest : TestCaseWithFactory
	{
		public void TestVisibleGetter_AfterTransportRemoved_NoException()
		{
			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			var visibilityProvider = new TransportFlightStatusVisibilityProvider(transport);
			Assert("Precondition: Transport is not deleted.", !transport.IsDeleted);
			AssertNoExceptionThrown("Precondition: No exception was thrown when calling Visible.", CallVisible);

			transport.Delete();

			Assert("Transport is deleted.", transport.IsDeleted);
			AssertNoExceptionThrown("No exception was thrown when calling Visible.", CallVisible);

			void CallVisible()
			{
				_ = visibilityProvider.Visible;
			}
		}
	}
}
