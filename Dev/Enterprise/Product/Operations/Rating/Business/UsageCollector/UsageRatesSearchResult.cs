using Enterprise.Environment;
using WiseRates.Constants;

namespace Enterprise.Rating.Business
{
	public class UsageRatesSearchResult
	{
		public UsageRatesSearchResult()
		{
			CargoSphere.IsEnabled = DataRegistryRating.Instance.CargoSphereIntegrationEnabled.Value;
			CargoSphere.IsAllowed = Env.Security.WiseRatesCargoSphereRateSearch.IsAllowed;

			Cargoguide.IsEnabled = DataRegistryRating.Instance.CargoguideIntegrationEnabled.Value;
			Cargoguide.IsAllowed = Env.Security.WiseRatesCargoguideRateSearch.IsAllowed;

			CW1.IsEnabled = true;
			CW1.IsAllowed = true;
		}

		public RateProviderSearchResult CargoSphere { get; } = new RateProviderSearchResult();
		public RateProviderSearchResult Cargoguide { get; } = new RateProviderSearchResult();
		public RateProviderSearchResult CW1 { get; } = new RateProviderSearchResult();
		public RateProviderSearchResult URS { get; } = new RateProviderSearchResult();

		public RateProviderSearchResult GetProviderResult(string providerCode)
		{
			switch (providerCode)
			{
				case WRConstants.RateProviders.CargoGuide:
					return Cargoguide;

				case WRConstants.RateProviders.CargoSphere:
					return CargoSphere;

				default:
					return CW1;
			}
		}
	}

	public class RateProviderSearchResult
	{
		public bool IsEnabled { get; set; }
		public bool IsAllowed { get; set; }
		public int TotalRates { get; set; }
		public int TotalCharges { get; set; }
		public int ValidRates { get; set; }
		public int WarningRates { get; set; }
		public int ErrorRates { get; set; }
		public int ElapsedTime { get; set; }
	}
	
	public class DbStats
	{
		public DbStats()
		{
			ClientRate = new RateTypeStats();
			Costing = new RateTypeStats();
			CompanyTariff = new RateTypeStats();
			InterCompanyTariff = new RateTypeStats();
		}

		public RateTypeStats ClientRate { get; set; }

		public RateTypeStats Costing { get; set; }

		public RateTypeStats CompanyTariff { get; set; }

		public RateTypeStats InterCompanyTariff { get; set; }
	}

	public class RateTypeStats
	{
		public int RatingHeaders { get; set; }

		public int RateEntries { get; set; }

		public int ExpiredRateEntries { get; set; }
	}
	
	public class RatesLoadedStats
	{
		/// <summary>
		/// Number of rates loaded before filtering
		/// </summary>
		public int LoadedRatesCount { get; set; }
		
		/// <summary>
		/// Number of rates after filtering
		/// </summary>
		public int FilteredRatesCount { get; set; }
	}
}
