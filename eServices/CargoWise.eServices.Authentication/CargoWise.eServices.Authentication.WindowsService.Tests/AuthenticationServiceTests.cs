using NUnit.Framework;

namespace CargoWise.eServices.Authentication.WindowsService.Tests
{
	[TestFixture]
	public class AuthenticationServiceTests
	{
		[Test]
		public void TestServiceInitialisation()
		{
			var service = new AuthenticationService();
			Assert.AreEqual("Authentication Service", service.ServiceName);
		}
	}
}
