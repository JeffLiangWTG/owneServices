using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public static class RatingCache
	{
		#region Cache Key Constants

		const string AutoRateDateByChargeGroupConfigurationCacheKey = "AutoRateDateByChargeGroupConfiguration";
		const string MeasureTypesCacheKey = "MeasureTypesCacheKey";

		#endregion

		#region Getters

		public static AutoRateDateByChargeGroupConfiguration AutoRateDateByChargeGroupConfiguration
		{
			get { return Get(AutoRateDateByChargeGroupConfigurationCacheKey, () => AccountingMasterFilesRegistry.Instance.AutoRateDateByChargeGroupSetup.Value); }
		}

		public static MeasureType GetMeasureTypeFromUnit(string unit)
		{
			var measureTypeDictionary = Get(MeasureTypesCacheKey, MeasureTypeRetriever.GetMeasureTypes);
			MeasureType result;
			return measureTypeDictionary.TryGetValue(unit, out result) ? result : MeasureType.Unidentified;
		}

		public static (RatingCriteriaLocationsCache, string) GetRatingCriteriaLocations(BusinessObjectFactory factory, RatingCriteriaLocationsCacheParameters parameters)
		{
			var cacheKey = parameters.GetCacheKey();
			return (Get(cacheKey, () => new RatingCriteriaLocationsCache(factory, parameters)), cacheKey);
		}

		public static (int level, bool applyGroupRate) GetCompanyTariffLevel(OrgHeader orgHeader, GlbCompany company, string category, string rateMode, OrgRateTariffLevel.Directions direction, ZDate effectiveDate)
		{
			var cacheKey = (orgHeader?.PK.ToStringKey() ?? string.Empty) + "|" + (company?.PK.ToStringKey() ?? string.Empty) + "|" + category + "|" + rateMode + "|" + direction + "|" + effectiveDate;
			return Get(cacheKey, () => CompanyTariffLevelGetter(orgHeader, company, category, rateMode, direction, effectiveDate));
		}

		public static IEnumerable<int> GetMultipleCompanyTariffLevels(OrgHeader orgHeader, GlbCompany company, string category, string rateMode, OrgRateTariffLevel.Directions direction)
		{
			var cacheKey = (orgHeader?.PK.ToStringKey() ?? string.Empty) + "|" + (company?.PK.ToStringKey() ?? string.Empty) + "|" + category + "|" + rateMode + "|" + direction;
			return Get(cacheKey, () => MultipleCompanyTariffLevelsGetter(orgHeader, company, category, rateMode, direction));
		}

		public static IEnumerable<IRatingHeader> GetCompanyTariffs(int tariffLevel, GlbCompany company, Func<IEnumerable<IRatingHeader>> retriever)
		{
			var cacheKey = tariffLevel + "|" + company.PK.ToStringKey();
			return Get(cacheKey, retriever);
		}

		#endregion

		#region Cache

		static T Get<T>(string cacheKey, Func<T> retriever)
		{
			return IsSessionActive ? ratingSession.GetCachedValue(cacheKey, retriever) : retriever();
		}

		static (int level, bool applyGroupRate) CompanyTariffLevelGetter(OrgHeader orgHeader, GlbCompany company, string category, string rateMode, OrgRateTariffLevel.Directions direction, ZDate effectiveDate)
		{
			if (effectiveDate.IsEmpty)
			{
				effectiveDate = ZDate.Today;
			}

			var level = CompanyTariff.GetLevelData(orgHeader, company, category, rateMode, direction, effectiveDate, effectiveDate);
			if (level != null)
			{
				return (level.P7_TariffLevel, level.P7_ApplyGroupRate);
			}

			return (Env.Registry.GlobalTariffDefault, false);
		}

		static IEnumerable<int> MultipleCompanyTariffLevelsGetter(OrgHeader orgHeader, GlbCompany company, string category, string rateMode, OrgRateTariffLevel.Directions direction)
		{
			var levels = CompanyTariff.GetLevelsData(orgHeader, company, category, rateMode, direction, ZDate.Empty, ZDate.Empty);
			if (levels != null && levels.Any())
			{
				return levels.Select(level => (int)level.P7_TariffLevel).ToList();
			}

			return [Env.Registry.GlobalTariffDefault];
		}

		#endregion

		#region Rating Session

		static bool IsSessionActive
		{
			get { return ratingSession != null; }
		}

		[ThreadStatic]
		static RatingSupporterSession ratingSession;

		public static RatingSupporterSession LocalSession
		{
			get { return ratingSession; }
		}

		internal static IDisposable StartRatingSession(IBusiness target)
		{
			if (!IsSessionActive)
			{
				ratingSession = new RatingSupporterSession(CleanUpCache, target);
				return ratingSession;
			}

			throw new InvalidOperationException("Rating Session has already been started");
		}

		static void CleanUpCache()
		{
			ratingSession = null;
		}

		#endregion
	}
}

