using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.RateSelector
{
	#region SuppressResourceStringsCheckRegion

	public class CW1RateProviderForRSL
	{
		readonly Stopwatch ratesLoadStopwatch = new Stopwatch();

		public CW1RateProviderForRSL(IRatingContext ratingContext, ILogger logger = null)
		{
			RatingContext = ratingContext;
			Logger = RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.Value ? logger : null;
		}

		public (IEnumerable<CW1RateCombinationResult> rateCombinationResults, IDisposable effectiveDateDisposer) FindRates(IRateSelectorFilterValueProvider filter)
		{
			var filteredRatePKs = new HashSet<ZGuid>();

			var results = new List<CW1RateCombinationResult>();
			var criteria = filter.CreateCriteria();
			var adapter = criteria.GetRatingAdapter();
			var effectiveDateDisposer = UpdateEffectiveOn(filter, criteria);

			try
			{
				// We temporary override PaymentTerm on Rating Adapter because FreightAutoRater relies on its value rather
				// than on Rating Criteria value when filtering rates by PaymentTerm. It is a hack but there is nothing we can do at the moment.
				if (adapter != null && adapter.PaymentTerm != criteria.PaymentTerm)
				{
					adapter.PaymentTerm = criteria.PaymentTerm;
				}

				Logger?.Debug($@"Provided Filters:
    Is Containerised: {new ZBool(criteria.IsContainerised).ToString()}
    Container Type: {string.Join(", ", criteria.GetContainers().Select(x => $"({x.PK} / {x.RC_Code})"))}
    Commodity Codes from Job: {filter.CommodityCodesFromJobContainers}
    Universal Commodity Groups from Filters: {filter.UniversalCommodityGroupsFromFilters}
    Service Providers: {string.Join(", ", filter.Carriers.Select(x => $"({x.Org.PK} / {x.Org.OH_Code})"))}
    Contract Numbers: {string.Join(", ", filter.ContractNumbersFromFilters)}
    Carrier Service Levels: {string.Join(", ", filter.CarrierServiceLevelsFromFilters)}");

				var context = new CW1RateCombinationContext(filter, Logger);

				// Load rates
				var carrierOrgsFilter = filter.GetAdditionalCarrierOrgListFilter(typeof(RatingHeader), RatingHeaderSchema.TH_OH);
				ratesLoadStopwatch.Restart();
				var allEntries = GetCostRateEntries(criteria, carrierOrgsFilter).ToList();
				ratesLoadStopwatch.Stop();

				foreach (var containerGroup in context.ContainerGroups)
				{
					var allEntriesInContainerGroup = allEntries
						.Where(rate => IsApplicableBasedOnContainerTypeAndCommodityCode(rate, containerGroup.ContainerTypePk, containerGroup.CommodityCode, context))
						.ToList();

					var applicableTopRateEntries = allEntriesInContainerGroup
						.Where(rate => rate.IsFreightEntry())
						.Where(rate => IsApplicableBasedOnServiceProviderAndCarrier(rate, context))
						.ToList();

					var topLevelRatesGroupedByServiceProvider = applicableTopRateEntries
						.Where(rate => IsMostSpecificBasedOnServiceProviderAndCarrier(rate, applicableTopRateEntries))
						.GroupBy(r => r.ServiceProviderPK());

					foreach (var groupedByServiceProvider in topLevelRatesGroupedByServiceProvider)
					{
						var serviceProviderCode = filter.Factory.Load<OrgHeader>(groupedByServiceProvider.Key)?.OH_Code ?? "Standard Cost";
						Logger?.Debug($"Start processing by Service Provider ({groupedByServiceProvider.Key} {serviceProviderCode}) ...");

						var topLevelEntries = new List<IRateEntry>();
						var topLevelRatesGroupedByChargeCode = groupedByServiceProvider.ToList()
							.SelectMany(r => r.ChildRateLines)
							.GroupBy(l => l.TL_AC);

						foreach (var groupedByChargeCode in topLevelRatesGroupedByChargeCode)
						{
							var chargeCode = filter.Factory.Load<AccChargeCode>(groupedByChargeCode.Key).AC_Code;
							Logger?.Debug($"Start processing entries with the same Charge Code ({chargeCode}) ...");

							var parentRateEntries = groupedByChargeCode
								.ToList()
								.Select(l => l.ParentRateEntry);

#if NETFRAMEWORK
							var entriesByChargeCode = IEnumerableExtensions.DistinctBy(parentRateEntries, r => r.PK).ToList();
#else
							var entriesByChargeCode = Enumerable.DistinctBy(parentRateEntries, r => r.PK).ToList();
#endif

							foreach (var isApplicableAndMostSpecific in context.FilterMethodsOrderByPriorities)
							{
								entriesByChargeCode = entriesByChargeCode
									.Where(rate => isApplicableAndMostSpecific(rate, entriesByChargeCode))
									.ToList();
							}

							topLevelEntries.AddRange(entriesByChargeCode);

							Logger?.Debug($"End of processing entries with the same Charge Code ({chargeCode})");
						}

#if NETFRAMEWORK
						topLevelEntries = topLevelEntries
							.DistinctBy(r => r.PK)
							.ToList();
#elif NET
						topLevelEntries = Enumerable
							.DistinctBy(topLevelEntries, r => r.PK)
							.ToList();
#endif

						topLevelEntries = topLevelEntries
							.Where(mainRate => topLevelEntries.All(otherRate => IsMoreSpecific(mainRate: mainRate, otherRate: otherRate, context: context)))
							.ToList();

						var groupedByTopLevelRates = topLevelEntries
							.GroupBy(rate => new
							{
								ServiceProviderPK = rate.ServiceProviderPK(),
								CarrierPK = rate.TI_OH_TransportProvider,
								ContractNumber = rate.TI_ContractNumber,
								CarrierServiceLevel = rate.TI_PL_NKCarrierServiceLevel,
								CommodityCode = context.IsContainerised ? rate.TI_RH_NKCommodityCode : ZString.Empty
							});

						foreach (var topRates in groupedByTopLevelRates)
						{
							var topRate = topRates.ToList().First();
							Logger?.Debug($"Start processing head entry ({GetLogText(topRate)}) ...");

							var criteriaToGetLines = filter.CreateCriteria();
							UpdateCriteria(criteriaToGetLines, filter.OriginalCriteria, topRate);
							Logger?.Debug("Rating Criteria updated");

							var combinedRates = allEntriesInContainerGroup
								.Where(r => EqualsOrLessSpecificBasedOnServiceProvider(r, topRates.Key.ServiceProviderPK))
								.Where(r => EqualsOrLessSpecificBasedOnContractNumber(r, topRates.Key.ContractNumber))
								.Where(r => EqualsOrLessSpecificBasedOnCommodityCode(r, topRates.Key.CommodityCode, context.IsContainerised))
								.Where(r => EqualsOrLessSpecificBasedOnCarrierServiceLevel(r, topRates.Key.CarrierServiceLevel))
								.Where(r => EqualsOrLessSpecificBasedOnCarrier(r, topRates.Key.CarrierPK))
								.ToList();

							// Calculations
							using (SetTemporaryManualSelectionSearchingMode(criteriaToGetLines))
							{
								var charges = GetLines(combinedRates, criteriaToGetLines, filteredRatePKs);
								AddRateLinesIfNotExist(criteriaToGetLines, containerGroup.ContainerTypePk, containerGroup.CommodityCode, topRates, charges);

								if (!topRates.Key.ContractNumber.IsEmpty && combinedRates.Any(r => r.IsFreightEntry() && r.TI_ContractNumber.IsEmpty))
								{
									Logger?.Debug("Recalculating, because head rate has a specific Contract Number");
									var contractRates = GetLines(topRates, criteriaToGetLines, filteredRatePKs);
									AddRateLinesIfNotExist(criteriaToGetLines, containerGroup.ContainerTypePk, containerGroup.CommodityCode, topRates, contractRates);
								}

								Logger?.Debug($"End of processing head entry ({GetLogText(topRate)})");
							}
						}

						Logger?.Debug($"End of processing by Service Provider ({groupedByServiceProvider.Key} {serviceProviderCode})");
					}
				}
				
				CostRatesLoader.ReportRatesLoadedStats(new RatesLoadedStats
				{
					LoadedRatesCount = allEntries.Count,
					FilteredRatesCount = filteredRatePKs.Count
				}, ratesLoadStopwatch.ElapsedMilliseconds);
			}
			finally
			{
				if (adapter != null)
				{
					//Setting RatingAdapter's PaymentTerm to null means reseting the RatingAdapter to use its parent information on Payment Term
					adapter.PaymentTerm = null;
				}
			}

			return (rateCombinationResults: results, effectiveDateDisposer: effectiveDateDisposer);

			void AddRateLinesIfNotExist(RatingCriteria updatedCriteria, ZGuid containerTypePk, ZString commodityCode, IEnumerable<IRateEntry> topRateEntries, IEnumerable<IRateLine> newRateLines)
			{
				var emptyOrAllNonFreight =
					newRateLines.IsNullOrEmpty() ||
					!newRateLines.Any(l => l.ParentRateEntry.IsFreightEntry());

				var alreadyAdded = results
					.Where(r => r.ContainerTypePk == containerTypePk && r.CommodityCode == commodityCode)
					.Any(r => r.Lines.WhereNotNull().EqualIgnoringOrder(newRateLines));

				if (emptyOrAllNonFreight || alreadyAdded)
				{
					return;
				}

				var headEntry = newRateLines
					.Select(l => l.ParentRateEntry);
#if NETFRAMEWORK
				headEntry = headEntry.DistinctBy(e => e.PK);
#elif NET
				headEntry = Enumerable.DistinctBy(headEntry, e => e.PK);
#endif
				var firstHeadEntry = headEntry
					.Intersect(topRateEntries)
					.FirstOrDefault();

				if (firstHeadEntry != null)
				{
					results.Add(new CW1RateCombinationResult
					{
						Criteria = updatedCriteria,
						ContainerTypePk = containerTypePk,
						CommodityCode = commodityCode,
						HeadEntry = firstHeadEntry,
						Lines = newRateLines,
					});
				}
			}
		}

		static IDisposable UpdateEffectiveOn(IRateSelectorFilterValueProvider filter, RatingCriteria criteria)
		{
			var originalAutoRateDateOverride = criteria.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride);

			if ((ZDate)filter.EffectiveDate == (ZDate)originalAutoRateDateOverride)
			{
				return DisposableAction.NoAction;
			}

			return new DisposableAction(
				() =>
					criteria.UpdateAutoratingDate((ZDate)filter.EffectiveDate, isCosting: true),
				() =>
					criteria.UpdateAutoratingDate((ZDate)originalAutoRateDateOverride, isCosting: true)
			);
		}

		IEnumerable<IRateEntry> GetCostRateEntries(RatingCriteria criteria, ZQuery additionalOrganisationsFilter)
		{
			using (SetTemporaryManualSelectionSearchingMode(criteria))
			{
				var ratesLoader = new CostRatesLoader(RatingContext.Factory, Logger);
				var result = ratesLoader.Load(criteria, additionalOrganisationsFilter: additionalOrganisationsFilter);

				Logger?.Debug($"{result.Count()} CW1 cost entries found in total");

				return result;
			}
		}

		bool IsApplicableBasedOnContainerTypeAndCommodityCode(IRateEntry rate, ZGuid containerTypePK, ZString commodityCode, CW1RateCombinationContext context)
		{
			var matchedByContainer =
				(
					!rate.IsContainerTypeAllowed()
					&& rate.IsSupplementaryEntry()
				)
				|| rate.IsFCLEntryWithEmptyContainer()
				|| rate.IsSameContainerOrSameClass(containerTypePK, true);

			var matchedByCommodity =
				!context.IsContainerised ||
				rate.IsEmptyCommodityCode() ||
				(rate.IsGeneralCommodityCode() && commodityCode.IsEmpty) ||
				rate.TI_RH_NKCommodityCode == commodityCode;

			return matchedByContainer && matchedByCommodity;
		}

		bool IsApplicableBasedOnServiceProviderAndCarrier(IRateEntry rate, CW1RateCombinationContext context)
		{
			var rateServiceProviderPK = rate.ServiceProviderPK();

			var isApplicable =
				(!rateServiceProviderPK.IsEmpty && (context.CarrierPKsFromFilters.IsNullOrEmpty() || context.CarrierPKsFromFilters.Contains(rateServiceProviderPK))) ||
				(!rate.TI_OH_TransportProvider.IsEmpty && (context.CarrierPKsFromFilters.IsNullOrEmpty() || context.CarrierPKsFromFilters.Contains(rate.TI_OH_TransportProvider)));

			Logger?.Debug($"Entry  {GetLogText(rate)}  is{(isApplicable ? " " : " not ")}applicable based on Service Provider and Carrier");

			return isApplicable;
		}

		bool IsMostSpecificBasedOnServiceProviderAndCarrier(IRateEntry rate, IEnumerable<IRateEntry> rates)
		{
			var rateServiceProviderPK = rate.ServiceProviderPK();

			var isMostSpecific =
				!rateServiceProviderPK.IsEmpty ||
				(rateServiceProviderPK.IsEmpty && rates
					.Where(r => !r.IsStandardCostRate())
					.All(r => r.ServiceProviderPK() != rate.TI_OH_TransportProvider && r.TI_OH_TransportProvider != rate.TI_OH_TransportProvider));

			Logger?.Debug($"Entry  {GetLogText(rate)}  is{(isMostSpecific ? " " : " not ")}most specific based on Service Provider and Carrier");

			return isMostSpecific;
		}

		static bool EqualsOrLessSpecificBasedOnServiceProvider(IRateEntry rate, ZGuid serviceProviderPK)
		{
			var rateServiceProviderPK = rate.ServiceProviderPK();

			return rateServiceProviderPK.IsEmpty || rateServiceProviderPK == serviceProviderPK;
		}

		static bool EqualsOrLessSpecificBasedOnContractNumber(IRateEntry rate, ZString contractNumber) =>
			rate.TI_ContractNumber.IsEmpty || rate.TI_ContractNumber == contractNumber;

		static bool EqualsOrLessSpecificBasedOnCommodityCode(IRateEntry rate, ZString commodityCode, bool isContainerised) =>
			!isContainerised ||
			rate.IsEmptyCommodityCode() ||
			rate.TI_RH_NKCommodityCode == commodityCode;

		static bool EqualsOrLessSpecificBasedOnCarrierServiceLevel(IRateEntry rate, ZString carrierServiceLevel) =>
			rate.TI_PL_NKCarrierServiceLevel.IsEmpty || rate.TI_PL_NKCarrierServiceLevel == carrierServiceLevel;

		static bool EqualsOrLessSpecificBasedOnCarrier(IRateEntry rate, ZGuid carrierPK) =>
			rate.TI_OH_TransportProvider.IsEmpty || rate.TI_OH_TransportProvider == carrierPK;

		bool IsMoreSpecific(
			IRateEntry mainRate,
			IRateEntry otherRate,
			CW1RateCombinationContext context)
		{
			var equalsOrLessSpecific =
				EqualsOrLessSpecificBasedOnContractNumber(mainRate, otherRate.TI_ContractNumber) &&
				EqualsOrLessSpecificBasedOnCommodityCode(mainRate, otherRate.TI_RH_NKCommodityCode, context.IsContainerised) &&
				EqualsOrLessSpecificBasedOnCarrierServiceLevel(mainRate, otherRate.TI_PL_NKCarrierServiceLevel) &&
				EqualsOrLessSpecificBasedOnCarrier(mainRate, otherRate.TI_OH_TransportProvider);

			var isMoreSpecificBasedOnCommodityCode =
				!context.IsContainerised ||
				context.IsCommodityCodesFromFilterEmpty ||
				mainRate.IsSpecificCommodityCode() ||
				(mainRate.IsGeneralCommodityCode() && otherRate.IsEmptyOrGeneralCommodityCode()) ||
				otherRate.IsEmptyCommodityCode() ||
				!equalsOrLessSpecific;

			var isMoreSpecific =
				(context.IsContractNumberFilterEmpty || !mainRate.TI_ContractNumber.IsEmpty || otherRate.TI_ContractNumber.IsEmpty || !equalsOrLessSpecific) &&
				(context.IsCarrierServiceLevelFilterEmpty || !mainRate.TI_PL_NKCarrierServiceLevel.IsEmpty || otherRate.TI_PL_NKCarrierServiceLevel.IsEmpty || !equalsOrLessSpecific) &&
				(context.IsCarrierFilterEmpty || !mainRate.TI_OH_TransportProvider.IsEmpty || otherRate.TI_OH_TransportProvider.IsEmpty || !equalsOrLessSpecific) &&
				isMoreSpecificBasedOnCommodityCode;

			if (!isMoreSpecific)
			{
				Logger?.Debug($"Entry  {GetLogText(mainRate)} filered, because it is less specific than entry  {GetLogText(otherRate)}");
			}

			return isMoreSpecific;
		}

		void UpdateCriteria(RatingCriteria criteria, RatingCriteria originalCriteria, IRateEntry rate)
		{
			Creditors creditors = null;

			var serviceProvider = rate.ParentRatingHeader?.Header;
			if (serviceProvider != null)
			{
				var source = new List<string> { "CW1 Calculated Rate Selector" }; // internal use
				var chargeCodeGroups = originalCriteria.Creditors?.ChargeCodeGroups.ToArray();
				if (chargeCodeGroups.IsNullOrEmpty() || chargeCodeGroups.Any(string.IsNullOrWhiteSpace))
				{
					creditors = Creditors.New(OrgWithSource.New(serviceProvider, source));
				}
				else
				{
					creditors = new Creditors();
					foreach (var chargeCodeGroup in chargeCodeGroups)
					{
						creditors[chargeCodeGroup].Add(1, new[] { OrgWithSource.New(serviceProvider, source) });
					}
				}
			}

			criteria.Creditors = creditors;
			criteria.Carrier = rate.TransportProvider;
			criteria.CarrierServiceLevelOverride = rate.TI_PL_NKCarrierServiceLevel.IsEmpty ? new List<ZString>() : new List<ZString>(new[] { rate.TI_PL_NKCarrierServiceLevel });
			criteria.CarrierContractNumbers = rate.TI_ContractNumber.IsEmpty ? Array.Empty<ZString>() : new[] { rate.TI_ContractNumber };
		}

		IEnumerable<IRateLine> GetLines(IEnumerable<IRateEntry> rates, RatingCriteria criteria, HashSet<ZGuid> filteredRatePKs)
		{
			Logger?.Debug($"Combined rate entries: {string.Join("", rates.Select(x => $"{System.Environment.NewLine}    {GetLogText(x)}"))}");

			Logger?.Debug("Start filtering rates based on criteria ...");
			ratesLoadStopwatch.Start();
			var filteredEntries = RateEntryFilter.Filter(criteria, true, rates, criteria.Factory, Logger ?? new MemoryLogger());

			filteredEntries.Select(e => e.PK).ToList().ForEach(pk => filteredRatePKs.Add(pk));

			ratesLoadStopwatch.Stop();
			Logger?.Debug("End of filtering rates based on criteria");

			Logger?.Debug("Start filtering lines ...");

			var logger = new LoggerDecorator();

			using (var rateLinesRepository = new RateLinesRepository(criteria, filteredEntries.ToList(), logger))
			{
				// Rate lines are from 2 sources: filteredEntries and related lines (CTB and CST calculators).
				// Reuse the same factory in new FreightAutoRater so that RatingCriteriaCache and rateLinesRepository(s) can correctly cache/recall the lines.
				// For example,
				// - rateLinesRepository has Line1 (from factory 1)
				// - with CST calculator, AutoRater tries to load all lines including Line1 and original lines for Line1 from factory 2.
				// - at a certain point AutoRater's rateLinesRepository tries to remove Line1 (factory 1).
				// - If [factory 1] != [factory 2] then the removal fails because it sees [Line1 (factory 1)] != [Line1 (factory 2)]
				var context = Business.RatingContext.CreateInstance(filteredEntries.FirstOrDefault(), logger, RatingContext.DialogService);
				var freightAutoRater = new FreightAutoRater(context);

				var parameters = new AutoRatingCalculatorParametersWithoutFilter(criteria, freightAutoRater, true);
				var options = new NotApplicableRateLineRemover.FilterOptions { IsCosting = true, DisableSpotFilter = true };
				var remover = new NotApplicableRateLineRemover();
				remover.RemoveNotApplicable(parameters, rateLinesRepository, options, RatingContext.DialogService);

				var lines = rateLinesRepository.GetLines().Select(l => l.Line).ToList();

				Logger?.Debug($"Filtered lines: {string.Join("", lines.Select(x => $"{System.Environment.NewLine}    {x.ChargeCode?.AC_Code} {x.TL_RateCalculator}"))}");
				Logger?.Debug("End of filtering lines");

				return lines;
			}
		}

		IDisposable SetTemporaryManualSelectionSearchingMode(RatingCriteria criteria)
		{
			var originalIsManualCostSelectSearchingCW1Rates = criteria.IsManualCostSelectSearchingCW1Rates;

			return new DisposableAction(
				() => criteria.IsManualCostSelectSearchingCW1Rates = true,
				() => criteria.IsManualCostSelectSearchingCW1Rates = originalIsManualCostSelectSearchingCW1Rates);
		}

		static string GetLogText(IRateEntry entry)
		{
			return string.Format("{0}-{1}-{2}-{3}",
				entry.ParentRatingHeader.Header?.OH_Code ?? "Standard Cost", // used for log
				entry.TI_ContractNumber.IsEmpty ? "   " : entry.TI_ContractNumber.ToString(),
				entry.TI_PL_NKCarrierServiceLevel.IsEmpty ? "   " : entry.TI_PL_NKCarrierServiceLevel.ToString(),
				entry.TransportProvider?.OH_Code ?? "   ");
		}

		IRatingContext RatingContext { get; }
		ILogger Logger { get; }
	}

	#endregion
}
