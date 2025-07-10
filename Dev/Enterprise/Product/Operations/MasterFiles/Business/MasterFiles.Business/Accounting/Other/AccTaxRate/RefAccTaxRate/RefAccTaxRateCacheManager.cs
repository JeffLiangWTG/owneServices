using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business
{
	class RefAccTaxRateCacheManager
	{
		public RefAccTaxRateCacheManager(BusinessObjectFactory factory, ZPropertyInfo country, ZPropertyInfo reference)
		{
			Factory = factory;
			Country = country;
			Country.ValueChanged += (sender, e) => ResetLastExtraRateDateRequested();
			Reference = reference;
			Reference.ValueChanged += (sender, e) => ResetLastExtraRateDateRequested();
		}

		public RefAccTaxRate GetRateComponents(ZDate taxDate)
		{
			if (!taxDate.IsValid && !taxDate.IsEmpty)
			{
				return null;
			}

			var taxDateVerified = taxDate.IsEmpty ? ZDate.Today : taxDate;
			if (lastExtraRateDateRequested.IsEmpty || lastExtraRateDateRequested != taxDateVerified)
			{
				lastExtraRateDateRequested = taxDateVerified;
				lastRateComponentsReturned = AllRatesCache.FirstOrDefault(x => x.ZAT_EndDate >= lastExtraRateDateRequested && x.ZAT_StartDate <= lastExtraRateDateRequested); //ZAT_EndDate compared first because AllRatesCache sorted by it.
			}

			return lastRateComponentsReturned;
		}

		ZDate lastExtraRateDateRequested = ZDate.Empty;
		RefAccTaxRate lastRateComponentsReturned;

		void ResetLastExtraRateDateRequested()
		{
			lastExtraRateDateRequested = ZDate.Empty;
		}

		ZPropertyInfo Country { get; }

		ZPropertyInfo Reference { get; }

		BusinessObjectFactory Factory { get; }

		RefAccTaxRate[] AllRatesCache
		{
			get
			{
				if (allRatesCache == null)
				{
					var query = new ZQuery(RefAccTaxRateSchema.ZAT_RN_NKCountry, Country.Value);
					query.AddToFilter(RefAccTaxRateSchema.ZAT_ReferenceRateType, Reference.Value);
					query.OrderBy = Invariant($"{RefAccTaxRateSchema.ZAT_EndDate.Name} DESC");
					allRatesCache = Factory.Load<RefAccTaxRate>(query);
				}

				return allRatesCache;
			}
		}
		RefAccTaxRate[] allRatesCache;
	}
}
