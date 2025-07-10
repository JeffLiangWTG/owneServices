using System.Configuration;
using Microsoft.IdentityModel.Logging;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test
{
	[TestFixture]
	public class StartupTests
	{
		[TestCase("true", true)]
		[TestCase("false", false)]
		public void Configure_ShowPIISetInAppSettings_SetIdentityModelEventSource(string showPiiValue, bool expectedvalue)
		{
			ConfigurationManager.AppSettings["showPii"] = showPiiValue;

			using var server = TestServer.Create<TestStartup>();

			Assert.That(IdentityModelEventSource.ShowPII, Is.EqualTo(expectedvalue));
		}

		[Test]
		public void Configure_ShowPIINotSetInAppSettings_SetIdentityModelEventSourceToFalse()
		{
			using var server = TestServer.Create<TestStartup>();

			Assert.That(IdentityModelEventSource.ShowPII, Is.False);
		}

		[SetUp]
		public void Init()
		{
			ConfigurationManager.AppSettings["Issuer"] = "";
			ConfigurationManager.AppSettings["AudienceId"] = "";
			ConfigurationManager.AppSettings["AudienceSecret"] = "";
			ConfigurationManager.AppSettings["ServerName"] = System.Environment.MachineName;
			ConfigurationManager.AppSettings["DatabaseName"] = "Odyssey";
			ConfigurationManager.AppSettings["SchemaPath"] = "Schemas";
		}

		[TearDown]
		public void TearDown()
		{
			IdentityModelEventSource.ShowPII = false;
		}
	}
}
