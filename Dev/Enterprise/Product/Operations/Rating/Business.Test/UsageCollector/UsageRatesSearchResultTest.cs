using System;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class UsageRatesSearchResultTest : TransactionedTestCase
	{
		public void TestProviderResults_IsEnabled()
		{
			void TestCase(bool cargoSphereEnabled, bool cargoguideEnabled)
			{
				using (DataRegistryRating.Instance.CargoSphereIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cargoSphereEnabled))
				using (DataRegistryRating.Instance.CargoguideIntegrationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, cargoguideEnabled))
				{
					var result = new UsageRatesSearchResult();
					AssertEquals("CargoSphere.IsEnabled should match the cargoSphereEnabled flag", cargoSphereEnabled, result.CargoSphere.IsEnabled);
					AssertEquals("Cargoguide.IsEnabled should match the cargoguideEnabled flag", cargoguideEnabled, result.Cargoguide.IsEnabled);
					AssertEquals("CW1.IsEnabled should always be true", true, result.CW1.IsEnabled);
				}
			}

			TestCase(false, false);
			TestCase(false, true);
			TestCase(true, false);
			TestCase(true, true);
		}

		public void TestProviderResults_IsAllowed()
		{
			void TestCase(bool cargoSphereAllowed, bool cargoguideAllowed)
			{
				var oldCargoSphereValue = Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed;
				var oldCargoguideValue = Env.Security.WiseRatesCargoguideRateSearch.IsAllowed;

				Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed = cargoSphereAllowed;
				Env.Security.WiseRatesCargoguideRateSearch.IsAllowed = cargoguideAllowed;

				try
				{
					var result = new UsageRatesSearchResult();
					AssertEquals("CargoSphere allowance mismatch", cargoSphereAllowed, result.CargoSphere.IsAllowed);
					AssertEquals("Cargoguide allowance mismatch", cargoguideAllowed, result.Cargoguide.IsAllowed);
					AssertEquals("CW1 should always be enabled", true, result.CW1.IsEnabled);
				}
				finally
				{
					Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed = oldCargoSphereValue;
					Env.Security.WiseRatesCargoguideRateSearch.IsAllowed = oldCargoguideValue;
				}
			}

			TestCase(false, false);
			TestCase(false, true);
			TestCase(true, false);
			TestCase(true, true);
		}
	}
}
