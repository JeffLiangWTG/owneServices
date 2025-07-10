#if NETFRAMEWORK
using CargoWise.Common;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class CommodityFMCPair
	{
		public string CommodityCode { get; set; }
		public string FMCTariffID { get; set; }
		public string RateSource { get; set; }
	}

	public class RateCommodityFMCPairProvider
	{
		readonly BusinessObjectFactory factory;
		readonly RateCommodityFMCPairLogger rootLogger;

		public RateCommodityFMCPairProvider(BusinessObjectFactory factory, RateCommodityFMCPairLogger logger)
		{
			this.factory = factory;
			this.rootLogger = logger;
		}

		/// <summary>
		/// Returns RateEntries which correspond to unique Commodity/FMC pairs
		/// </summary>
		public IEnumerable<CommodityFMCPair> GetMatches(RatingCriteria criteria)
		{
			rootLogger.Log(LogType.Information, ResString.GetMultilingualString("37b176ea-b4bb-442b-b579-8faeb6a207a4", "===== Begin Search Commodity Code For Tariff ID ====="));

			rootLogger.Log(LogType.Information, "--- Begin Search for Client Rate Headers ---");

			var clientRateResult =
				GetCommodityOnlyMatch(criteria, RevenueLoadOptions.ClientRates).Union(
				GetChildCommodityOnlyMatch(criteria, RevenueLoadOptions.ClientRates)).Union(
				GetSiblingCommodityOnlyMatch(criteria, RevenueLoadOptions.ClientRates));
#if NETFRAMEWORK
			clientRateResult = clientRateResult.DistinctBy(x => new { x.CommodityCode, x.FMCTariffID, x.RateSource });
#elif NET
			clientRateResult = Enumerable.DistinctBy(clientRateResult, x => new { x.CommodityCode, x.FMCTariffID, x.RateSource });
#endif
			clientRateResult = clientRateResult.ToList();

			rootLogger.Log(LogType.Information, "--- Found Matches in Client Rate Headers ---");
			rootLogger.Log(LogType.Information, $"Client Matches Found: {clientRateResult.Count()}");

			rootLogger.Log(LogType.Information, "--- Begin Search for Company Tariff Headers ---");

			var tariffResult =
				GetCommodityOnlyMatch(criteria, RevenueLoadOptions.Tariffs).Union(
				GetChildCommodityOnlyMatch(criteria, RevenueLoadOptions.Tariffs)).Union(
				GetSiblingCommodityOnlyMatch(criteria, RevenueLoadOptions.Tariffs));
#if NETFRAMEWORK
			tariffResult = tariffResult.DistinctBy(x => new { x.CommodityCode, x.FMCTariffID, x.RateSource });
#elif NET
			tariffResult = Enumerable.DistinctBy(tariffResult, x => new { x.CommodityCode, x.FMCTariffID, x.RateSource });
#endif
			tariffResult = tariffResult.ToList();

			rootLogger.Log(LogType.Information, $"--- Found Matches in Company Tariff Headers ---");
			rootLogger.Log(LogType.Information, $"Company Matches Found: {tariffResult.Count()}");

			var allMatches = tariffResult.Concat(clientRateResult);

			var result = CollateByRateSource(allMatches);

			rootLogger.Log(LogType.Information, ResString.GetMultilingualString("48e4bdaf-64dd-4f1d-9a32-dc67b84442fc", "===== Found a total of {0} distinct pairs =====", result.Count()));
			return CollateByRateSource(result);
		}

		/// <summary>
		/// Will return any company tariffs RateEntries that exactly match
		/// the given commodity but with ANY (or none) FMC Tariff ID
		///
		/// It will return them if found, otherwise an empty enumerable.
		/// </summary>
		internal IEnumerable<CommodityFMCPair> GetCommodityOnlyMatch(RatingCriteria criteria, RevenueLoadOptions loadOptions)
		{
			var rateCommodities = criteria.OverriddenCommodity.ToList();
			rootLogger.Log(LogType.Information, "--- Getting exact Commodity Match ---");
			return FindMatchCommodityOnly(criteria, rateCommodities, loadOptions);
		}

		/// <summary>
		/// Will return any company tariffs RateEntries that exactly match
		/// the given commodity's children, with any (or empty) FMC Tariff ID
		///
		/// It will return them if found, otherwise an empty enumerable.
		/// </summary>
		internal IEnumerable<CommodityFMCPair> GetChildCommodityOnlyMatch(RatingCriteria criteria, RevenueLoadOptions loadOptions)
		{
			var childRateCommodities = criteria.OverriddenCommodity
				.SelectMany(x => x.RefCommodityRatingCodeMaps)
				.Select(x => x.CommodityChild);
#if NETFRAMEWORK
			childRateCommodities = childRateCommodities.DistinctBy(x => x.RH_Code);
#elif NET
			childRateCommodities = Enumerable.DistinctBy(childRateCommodities, x => x.RH_Code);
#endif
			childRateCommodities = childRateCommodities.ToList();

			rootLogger.Log(LogType.Information, "--- Getting child Commodity Only Match ---");
			return FindMatchCommodityOnly(criteria, childRateCommodities, loadOptions);
		}

		/// <summary>
		/// Will return any company tariffs RateEntries that exactly match
		/// the given commodity's siblings, with any (or empty) FMC Tariff ID
		///
		/// It will return them if found, otherwise an empty enumerable.
		/// </summary>
		internal IEnumerable<CommodityFMCPair> GetSiblingCommodityOnlyMatch(RatingCriteria criteria, RevenueLoadOptions loadOptions)
		{
			var query = GetRateCommoditySiblingQuery(criteria.OverriddenCommodity);
			var siblingRateCommodities = factory.Load<RefCommodityCode>(query);
			rootLogger.Log(LogType.Information, "--- Getting sibling Commodity Only Match ---");
			return FindMatchCommodityOnly(criteria, siblingRateCommodities, loadOptions);
		}

		///<summary>
		/// Assuming the given commodities are rateCodes, find their
		/// rateCode parent commodities, and then match all their children.
		/// In other words, match the sibling rate code.
		/// </summary>
		ZQuery GetRateCommoditySiblingQuery(IEnumerable<RefCommodityCode> rateCommodities)
		{
			var rateCodeChildrenCodes = rateCommodities.Select(x => x.RH_Code);

			var findParentsQuery = new ZDBOnlySubQuery(typeof(RefCommodityRatingCodeMap), RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityParent);
			findParentsQuery.AddToFilter(RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityChild, rateCodeChildrenCodes);
			var findSiblingQuery = new ZDBOnlySubQuery(typeof(RefCommodityRatingCodeMap), RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityChild);
			findSiblingQuery.AddSubQuery(RefCommodityRatingCodeMapSchema.RI_RH_NKCommodityParent, findParentsQuery, JoinCondition.And);

			var findCommodity = new ZDBOnlyQuery(typeof(RefCommodityCode));
			findCommodity.AddSubQuery(RefCommodityCodeSchema.RH_Code, findSiblingQuery, JoinCondition.And);

			return findCommodity;
		}

		IEnumerable<CommodityFMCPair> FindMatchCommodityOnly(RatingCriteria criteria, IEnumerable<RefCommodityCode> overriddenCommodity, RevenueLoadOptions loadOptions)
		{
			var originalRateCommodities = criteria.OverriddenCommodity?.ToList();

			using (var childLogger = rootLogger.CreateIndentedLogger())
			using (criteria.TemporarilyAllowCriteriaChanging())
			{
				criteria.FMCTariffIDMatchEnabled = false;
				criteria.OverriddenCommodity = overriddenCommodity;
				var entries = GetRateEntries(criteria, childLogger, loadOptions);

				var pairs = entries
					.Where(e => !e.TI_RH_NKCommodityCode.IsEmpty)
					.OfType<RateEntry>()
					.Select(x => new CommodityFMCPair()
					{
						CommodityCode = x.TI_RH_NKCommodityCode,
						FMCTariffID = x.TI_FMCTariffID,
						RateSource = GetRateSource(x)
					})
					.ToList();

				rootLogger.Log(LogType.Information, $"Found {pairs.Count} pairs:{System.Environment.NewLine}" +
					string.Join(System.Environment.NewLine, pairs.Select(p => $"Commodity Code: {p.CommodityCode}, FMC Tariff ID: {p.FMCTariffID}, Rate Source: {p.RateSource}")));

				criteria.FMCTariffIDMatchEnabled = true;
				criteria.OverriddenCommodity = originalRateCommodities;

				return pairs;
			}
		}

		string GetRateSource(RateEntry entry)
		{
			if (entry.IsClientRate())
			{
				return RateSources.Code.ClientRate;
			}
			else if (entry.IsCompanyTariff())
			{
				return RateSources.Code.CompanyTariff;
			}

			throw new InvalidOperationException("Only client and company tariffs supported.");
		}

		internal virtual List<IRateEntry> GetRateEntries(RatingCriteria criteria, ILogger logger, RevenueLoadOptions loadOptions)
		{
			var loader = new RevenueRatesLoader(factory, logger);
			var entries = loader.Load(criteria, loadOptions);

			entries = RateEntryFilter.Filter(criteria, false, entries, factory, logger);
			var context = new RatingContext(logger);
			var freightAutoRater = new FreightAutoRater(context);
			var finalEntries = new List<IRateEntry>();
			foreach (var commodity in criteria.OverriddenCommodity)
			{
				criteria.RateableMeasures.UpdateCommodities(commodity.RH_Code);

				using var linesRepository = new RateLinesRepository(criteria, entries.ToList(), logger);
				var options = new NotApplicableRateLineRemover.FilterOptions()
				{
					DisablePaymentTermsFilter = true
				};
				var remover = new NotApplicableRateLineRemover();
				var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, freightAutoRater);

				remover.RemoveNotApplicable(parameters, linesRepository, options, null);

				var survivingEntries = linesRepository.GetLines()
					.Select(l => l.ParentRateEntry)
					.Distinct();

				finalEntries.AddRange(survivingEntries);
			}

			return finalEntries
				.Distinct()
				.ToList();
		}
		IEnumerable<CommodityFMCPair> CollateByRateSource(IEnumerable<CommodityFMCPair> matches)
		{
			return matches
				.GroupBy(x => new { x.CommodityCode, x.FMCTariffID })
				.Select(group =>
				{
					var rateSources = group.Select(x => x.RateSource).Distinct().ToList();
					string rateSource = rateSources.Count == 1 ? rateSources[0] : RateSources.Code.Both;

					return new CommodityFMCPair
					{
						CommodityCode = group.Key.CommodityCode,
						FMCTariffID = group.Key.FMCTariffID,
						RateSource = rateSource
					};
				})
				.ToList();
		}
		public static class RateSources
		{
			public static class Code
			{
				public const string CompanyTariff = "CTR";
				public const string ClientRate = "CLR";
				public const string Both = "BTH";
			}
		}
	}
}
