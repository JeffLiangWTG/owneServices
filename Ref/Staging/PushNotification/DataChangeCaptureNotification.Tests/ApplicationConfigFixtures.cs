using NUnit.Framework;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification.Tests
{
	[TestFixture]
	public class ApplicationConfigFixtures
	{
		[Test]
		public void ApplicationConfigTest()
		{
			Assert.AreEqual("mail.test.wisecloud.zone", ApplicationConfig.SmtpServer);
			Assert.AreEqual("donotreply_refservice", ApplicationConfig.NetworkUsername);
			Assert.That(ApplicationConfig.NetworkCredentialsPassword, Is.Empty);
		}
	}
}
