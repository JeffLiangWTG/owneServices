#region SuppressResourceStringsCheckRegion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using WiseRates.Tools;

namespace Enterprise.Rating.Business
{
	public static class RatingUsageCollector
	{
		const string cacheKey = "RatingDbStats";
		public enum RateSelectorAction
		{
			Select,
			Cancel,
			Skip
		}

		public enum RateProvider
		{
			CargoSphere,
			Cargoguide,
			CW1,
			URS
		}

		public static (string name, object value)[] GetAutoRateScope(IRatingContext context, IAutoRating adapter, AutoRateOptions options, CostSell costOrSell)
		{
			var scope = new List<(string, object)>
			{
				(UsageProperties.IsRateSelector, context.IsManualCostSelectMode),
				(UsageProperties.TriggerSource, options.TriggerSource.ToString()),
				(UsageProperties.ContainerMode, adapter?.ContainerMode),
				(UsageProperties.TransportMode, adapter?.FreightMode.ToTransportMode()),
				(UsageProperties.Mode, GetAutoRateUsageMode(context, costOrSell)),
				(UsageProperties.JobType, adapter?.AdapterType.ToString()),
				(UsageProperties.JobID, adapter?.JobID)
			};

			if (context.IsManualCostSelectMode)
			{
				scope.Add((UsageProperties.RateSelectorType, RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection() ? RateSelectorConstants.CarrierConnect : RateSelectorConstants.Legacy));
			}

			return scope.Where(p => p.Item2 != default).ToArray();
		}

		public static void ReportAutoRate(BusinessObjectFactory factory, RatesAdditionInfo additionInfo, TimeSpan elapsedTime)
		{
			var properties = new List<(string name, object value)>();

			if (additionInfo != null)
			{
				properties.Add((UsageProperties.RatesSearchResult, GetRatesSearchResult(additionInfo)));
				properties.Add((UsageProperties.ElapsedTime, elapsedTime.Milliseconds));
				properties.Add((UsageProperties.DBStats, GetDbStats(new DbStatsRetriever())));

				// We group charges so that we don't duplicate messages for charges with the same parameters. It will save a lot of resources
				// in cases like autorating of periodic billing when thousands of charges are calculated with the same rate line.
				var charges = additionInfo.RatesFound.Select(r => new
				{
					r.Calculator,
					r.RateCategory,
					r.RateMode,
					r.RateType
				}).GroupBy(c => c);

				foreach (var group in charges)
				{
					UsageCollector.Report(factory, UsageFeatures.Codes.ChargeAutorated, new (string, object)[]
					{
						(UsageProperties.ChargeCalculator, group.Key.Calculator),
						(UsageProperties.RateCategory, group.Key.RateCategory),
						(UsageProperties.RateMode, group.Key.RateMode),
						(UsageProperties.RateType, group.Key.RateType),
						(UsageProperties.Count, group.Count()),
					});
				}
			}

			// Although UsageCollector has its own factory and passing a factory is not required, it is not the case for autorating events.
			// The autorating can be called from workflow and we should respect the workflow transaction, i.e. the usage event has to be saved
			// if the transaction gets saved. So, we need to use the factory used by autorating which supposed to use the workflow factory.
			UsageCollector.Report(factory, UsageFeatures.Codes.Autorate, properties.ToArray());
		}

		public static void ReportRateSelector(RateSelectorAction action, long elapsedTime = default, RateProvider? selectedProvider = null, UsageRatesSearchResult result = default, long sessionTime = default)
		{
			var properties = new List<(string name, object value)>
			{
				(UsageProperties.Action, GetRateSelectorActionValue(action))
			};

			if (selectedProvider != null)
			{
				properties.Add((UsageProperties.SelectedProvider, GetRateProviderValue(selectedProvider)));
			}

			if (result != null)
			{
				properties.Add((UsageProperties.RatesSearchResult, result));
			}

			if (sessionTime != default)
			{
				properties.Add((UsageProperties.SessionTime, sessionTime));
			}

			properties.Add((UsageProperties.ElapsedTime, elapsedTime));
			properties.Add((UsageProperties.RateSelectorType, RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection() ? RateSelectorConstants.CarrierConnect : RateSelectorConstants.Legacy));

			UsageCollector.Report(UsageFeatures.Codes.RateSelector, properties.ToArray());
		}

		public static void ReportRateSelectorSearch(UsageRatesSearchResult result)
		{
			var properties = new List<(string name, object value)>();

			if (result != null)
			{
				properties.Add((UsageProperties.RatesSearchResult, result));
			}

			properties.Add((UsageProperties.RateSelectorType, RatingFeatureHelper.CarrierConnect.IsEnabledForRateSelection() ? RateSelectorConstants.CarrierConnect : RateSelectorConstants.Legacy));

			properties.Add((UsageProperties.DBStats, GetDbStats(new DbStatsRetriever())));
			
			UsageCollector.Report(UsageFeatures.Codes.RateSelectorSearch, properties.ToArray());
		}

		public static void ReportRatesLoaded(RatesLoadedStats result, long elapsedTime)
		{
			if (result == null || (result.LoadedRatesCount == 0 && result.FilteredRatesCount == 0))
			{
				return;
			}

			var properties = new List<(string name, object value)>
			{
				(UsageProperties.RatesLoadedStats, result),
				(UsageProperties.DBStats, GetDbStats(new DbStatsRetriever())),
				(UsageProperties.ElapsedTime, elapsedTime)
			};
			UsageCollector.Report(UsageFeatures.Codes.RatesLoaded, properties.ToArray());
		}

		static string GetAutoRateUsageMode(IRatingContext context, CostSell costSell)
		{
			if (context.IsInRebateCalculationMode)
			{
				return "Rebate";
			}

			if (costSell == CostSell.Cost)
			{
				return "Cost";
			}

			return "Revenue";
		}

		static string GetRateSelectorActionValue(RateSelectorAction action)
		{
			switch (action)
			{
				case RateSelectorAction.Select:
					return "Select";

				case RateSelectorAction.Cancel:
					return "Cancel";

				case RateSelectorAction.Skip:
					return "Skip";

				default:
					return action.ToString();
			}
		}

		static string GetRateProviderValue(RateProvider? provider)
		{
			if (provider == null)
			{
				return null;
			}

			switch (provider.Value)
			{
				case RateProvider.Cargoguide:
					return "Cargoguide";

				case RateProvider.CargoSphere:
					return "CargoSphere";

				case RateProvider.CW1:
					return "CW1";

				default:
					return provider.ToString();
			}
		}

		static UsageRatesSearchResult GetRatesSearchResult(RatesAdditionInfo additionInfo)
		{
			var ratesByProvider = new Dictionary<string, HashSet<string>>();
			var chargesByProvider = new Dictionary<string, int>();

			foreach (var charge in additionInfo.RatesFound)
			{
				if (!ratesByProvider.TryGetValue(charge.RateProviderCode, out var rates))
				{
					ratesByProvider[charge.RateProviderCode] = rates = new HashSet<string>();
				}

				rates.Add(charge.RateId);

				if (!chargesByProvider.ContainsKey(charge.RateProviderCode))
				{
					chargesByProvider[charge.RateProviderCode] = 0;
				}

				chargesByProvider[charge.RateProviderCode] += 1;
			}

			var result = new UsageRatesSearchResult();

			foreach (var rate in ratesByProvider)
			{
				var providerResult = result.GetProviderResult(rate.Key);
				providerResult.TotalRates = rate.Value.Count;
			}

			foreach (var charge in chargesByProvider)
			{
				var providerResult = result.GetProviderResult(charge.Key);
				providerResult.TotalCharges = charge.Value;
			}

			return result;
		}

		public static DbStats GetDbStats(IDbStatsRetriever retriever)
		{
			var cachedStats = MemoryCache.Default.Get(cacheKey) as DbStats;
			if (cachedStats != null)
			{
				return cachedStats;
			}
			
			var dbStats = new DbStats();
			var updateSuccessful = false;

			try
			{
				updateSuccessful = retriever.RetrieveDbStatsFromDatabase(ref dbStats);
			}
			catch (Exception ex)
			{
				var errorMessage = $"Failed to retrieve database statistics for rating: {ex.Message}";
				ErrorReporter.ReportOnce("RatingDbStatsRetrievalFailure", errorMessage);
				
				updateSuccessful = false;
			}
			
			var cacheItemPolicy = new CacheItemPolicy
			{
				AbsoluteExpiration = DateTimeOffset.Now.AddHours(updateSuccessful ? 24 : 1),
			};

			MemoryCache.Default.Set(cacheKey, dbStats, cacheItemPolicy);

			return dbStats;
		}
	}

	public interface IDbStatsRetriever
	{
		bool RetrieveDbStatsFromDatabase(ref DbStats dbStats);
	}

	public class DbStatsRetriever : IDbStatsRetriever
	{
		public bool RetrieveDbStatsFromDatabase(ref DbStats dbStats)
		{
			var collection = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			collection.Load("EXEC GetRateEntrySummary");

			if (collection.Count > 0)
			{
				var item = collection[0];

				// RatingHeaders
				dbStats.ClientRate.RatingHeaders = (ZInt)item["SAL_RatingHeaders"];
				dbStats.Costing.RatingHeaders = (ZInt)item["COS_RatingHeaders"];
				dbStats.CompanyTariff.RatingHeaders = (ZInt)item["GLB_RatingHeaders"];
				dbStats.InterCompanyTariff.RatingHeaders = (ZInt)item["ICT_RatingHeaders"];

				// RateEntries
				dbStats.ClientRate.RateEntries = (ZInt)item["SAL_RateEntries"];
				dbStats.Costing.RateEntries = (ZInt)item["COS_RateEntries"];
				dbStats.CompanyTariff.RateEntries = (ZInt)item["GLB_RateEntries"];
				dbStats.InterCompanyTariff.RateEntries = (ZInt)item["ICT_RateEntries"];

				// ExpiredRateEntries
				dbStats.ClientRate.ExpiredRateEntries = (ZInt)item["SAL_ExpiredRateEntries"];
				dbStats.Costing.ExpiredRateEntries = (ZInt)item["COS_ExpiredRateEntries"];
				dbStats.CompanyTariff.ExpiredRateEntries = (ZInt)item["GLB_ExpiredRateEntries"];
				dbStats.InterCompanyTariff.ExpiredRateEntries = (ZInt)item["ICT_ExpiredRateEntries"];
				
				return true;
			}
			
			return false;
		}
	}

	#region Constants

	public static class RateSelectorConstants
	{
		public const string Legacy = "Legacy";
		public const string CarrierConnect = "CarrierConnect";
	}

	#endregion
}

#endregion
