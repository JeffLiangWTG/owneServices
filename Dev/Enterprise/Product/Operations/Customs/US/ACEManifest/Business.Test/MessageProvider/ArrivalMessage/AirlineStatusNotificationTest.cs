using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AirlineStatusNotificationTest : TestCaseWithFactory
	{
		public void TestSetAirlineStatusNotification()
		{
			var newAirlineStatus = new AirlineStatusNotification("7");
			AssertEquals("7", newAirlineStatus.StatusCode);
			AssertEquals(string.Empty, newAirlineStatus.ActionExplanation);
		}
	}
}
