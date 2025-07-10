using System.Configuration;
using Enterprise.Services.Scim.Api.Config;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test.Config
{
	[TestFixture]
	sealed public class AppSettingTests
	{
		 AppSettings appSettings;

		[SetUp]
		public void Setup()
		{
			appSettings = new AppSettings();
		}

		[TestCase("true", true)]
		[TestCase("false", false)]
		[TestCase("", false)]
		[TestCase("asdf", false)]
		[TestCase(null, false)]
		public void ShowPii_WhenSet_ReturnsExpectedValue(string showPiiValue, bool expectedValue)
		{
			ConfigurationManager.AppSettings["showPii"] = showPiiValue;

			Assert.That(appSettings.ShowPii, Is.EqualTo(expectedValue));
		}

		[Test]
		public void ShowPii_WhenNotSet_ReturnsFalse()
		{
			Assert.That(appSettings.ShowPii, Is.False);
		}
	}
}
