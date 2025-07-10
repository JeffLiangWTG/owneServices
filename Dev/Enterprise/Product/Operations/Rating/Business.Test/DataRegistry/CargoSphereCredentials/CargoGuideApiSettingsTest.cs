using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CargoGuideApiSettings))]
	public class CargoGuideApiSettingsTest : RegistryBusinessObjectTemplateTestCase<CargoGuideApiSettings>
	{
		public void TestApiURL()
		{
			AssertApiURL("Valid url", "https://api-acc.cargoguide.app\n", null);
			AssertApiURL("Invalid url - no protocol", "test", "HTTPS Address is not well formed.");
			AssertApiURL("Invalid url - start with newline", "\nhttps://api-acc.cargoguide.app", "Only HTTPS protocol is supported.");
		}

		void AssertApiURL(string message, string url, string expectedError)
		{
			var cargoGuideApiSettings = new CargoGuideApiSettings();
			cargoGuideApiSettings.ApiURL = url;

			CombineAssertions(message, () =>
			{
				AssertEquals("ApiURLInfo", url, cargoGuideApiSettings.ApiURLInfo.Value);

				if (expectedError != null)
				{
					AssertEquals("Should have error", expectedError, cargoGuideApiSettings.ApiURLInfo.Notifications.First().Message);
				}
				else
				{
					AssertEquals("Should not have error", false, cargoGuideApiSettings.ApiURLInfo.HasErrors());
				}
			});
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override CargoGuideApiSettings GetBusinessObjectToClone()
		{
			return new CargoGuideApiSettings { ApiURL = "https://www.cargoguide.info", ApiVersion = "1.1" };
		}

		protected override CargoGuideApiSettings GetBusinessObjectToSerialise()
		{
			return new CargoGuideApiSettings { ApiURL = "https://www.cargoguide.info", ApiVersion = "1.1" };
		}
	}
}
