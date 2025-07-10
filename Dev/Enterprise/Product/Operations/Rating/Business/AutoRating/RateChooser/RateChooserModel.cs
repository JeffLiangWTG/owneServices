using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.MeasureInfo;
using Rate = WiseRates.Api.Model.Rate;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;
using RefServiceLevel = WiseRates.Api.Model.RefServiceLevel;

namespace Enterprise.Rating.Business
{
	public class RateChooserModel
	{
		public static string DefaultCurrency => GlbCompany.CurrentCompany?.GC_RX_NKLocalCurrency ?? string.Empty;

		public RateChooserModel(RatingCriteria criteria, IRatingContext context)
		{
			this.context = context;
			Criteria = criteria;
			Criteria.ValuesCanBeSet = true;

			if (context.ProviderUrsRates is WiseRatesProvider existingProvider)
			{
				wiseRatesProvider = existingProvider;
			}
			else if (context.ProviderUrsRates != null)
			{
				// In this case, context.ProviderUrsRates should be UrsRatesProvider and Urs should be enabled.
				// Note: For Rate Selector, When Urs is enabled, we will use WiseRatesProvider & URS conversion.
				wiseRatesProvider = new WiseRatesProvider(Factory, ObjectFactory.Get<IWiseRatesClientFactory>(), context.ProviderUrsRates.Logger);
			}
			else
			{
				wiseRatesProvider = null;
			}

			cw1RatesProvider = context.ProviderCW1Rates;
			chooserServices = new RateChooserServices(context.Factory, ZDateTime.Today, DefaultCurrency);
			WiseRatesProviderLogger = wiseRatesProvider?.Logger ?? new ElementaryLogger();

			AutoRating = criteria.AutoRating;

			Converter = new WiseRatesConverter(context.Factory, WiseRatesProviderLogger);

			var containerTypePKs = Criteria.JobMeasures.GetContainerTypePKs()
				.Where(pk => pk != ContainerInfo.LCL)
				.Select(pk => pk);

			var refContainers = context.Factory.Load<RefContainer>(new ZQuery(RefContainerSchema.PK, containerTypePKs));

			criteriaContainerCommodityList = new List<ChooserContainerCommodity>();

			if (Criteria.IsContainerised)
			{
				foreach (var containerQualityCommodityGroup in Criteria.RateableMeasures.GetContainerTypeAndCommodityList()
					.GroupBy(x => new { x.ContainerTypePk, x.ContainerQuality, x.CommodityCode })
					.Where(x =>
						!x.Key.ContainerTypePk.IsEmpty
						&& x.Key.ContainerTypePk != ContainerInfo.LCL))
				{
					var refContainer = refContainers.FirstOrDefault(x => x.PK == containerQualityCommodityGroup.Key.ContainerTypePk);

					if (refContainer != null)
					{
						var containerCommodity = new ChooserContainerCommodity(
							containerRef: refContainer,
							containerQuality: containerQualityCommodityGroup.Key.ContainerQuality,
							commodityCode: containerQualityCommodityGroup.Key.CommodityCode,
							containerCount: containerQualityCommodityGroup.Sum(x => x.ContainerCount));
						criteriaContainerCommodityList.Add(containerCommodity);
					}
				}
			}
			else
			{
				var containerCommodity = new ChooserContainerCommodity();
				criteriaContainerCommodityList.Add(containerCommodity);
			}
		}

		public IAutoRating AutoRating { get; }
		public RatingCriteria Criteria { get; }
		RateChooserServices chooserServices { get; }

		readonly List<ChooserContainerCommodity> criteriaContainerCommodityList;
		readonly WiseRatesProvider wiseRatesProvider;
		readonly ICW1RatesProvider cw1RatesProvider;
		readonly IRatingContext context;
		public readonly ILogger WiseRatesProviderLogger;
		public bool RemoveZeroCharges { get; set; }
		public bool NeedShowBookingRequestDialog
		{
			get
			{
				var supporter = Criteria.AutoRating?.InvoicingSupporter;
				if (supporter == null)
				{
					return false;
				}
				if (supporter.TransportMode != TransportModes.Sea)
				{
					return false;
				}
				switch (supporter.ContainerMode)
				{
					case ContainerModes.FCL:
					case ContainerModes.Groupage:
					case ContainerModes.BuyersConsol:
						return criteriaContainerCommodityList.Any(i => i.SelectedRate?.WiseRateEntry?.BookingInfo?.BookingTerms?.Items?.Any() == true);
					default:
						return false;
				}
			}
		}

		WiseRatesConverter Converter { get; }
		public WiseRatesConversionContext ConversionContext { get; set; }

		public ILogger Logger => context.Logger;

		public BusinessObjectFactory Factory => context.Factory;

		public IEnumerable<ChooserContainerCommodity> ContainerGroups => criteriaContainerCommodityList;

		public AutoRateInfoCollection GetSelectedRate()
		{
			var relatedChargesProvider = new RateChooserRelatedChargesProvider(context.Factory, Logger, Criteria, cw1RatesProvider, GetCarrierFromSelectedRates());
			var charges = new AutoRateInfoCollection(context.Factory);
			var allCW1Charges = relatedChargesProvider.GetAllCW1Charges();

			foreach (var containerCommodityItem in criteriaContainerCommodityList.Where(x => x.SelectedRate != null))
			{
				var isRatesServiceRate = containerCommodityItem.SelectedRate.WiseRateEntry != null;

				if (isRatesServiceRate)
				{
					//For BOL charges,by now, we select the first BOL charge. All the same BOL charges should have the same value,
					//but in future if there is any difference, Product will decide the logic of picking up the BOL charge based on client incident
					//(More info in WI00398826)
					//Also If we have DDOC and SDOC under BOL for 20 GP and have DDOC, SDOC and SDOC1 under BOL for 40GP,
					//we select DDOC and SDOC from 20GP and SDOC1 from 40GP. We should have only one charge Code for BOL.
					var bolCharges = charges
						.Where(c => c.Line is WiseLine wiseLine && wiseLine.WiseCharge.CustomCategory == WRConstants.ChargeCustomCategory.BOL)
						.Select(line => line.ChargeCode.AC_Code)
						.ToList();

					var carrierCharges = CalculateForRateService(containerCommodityItem);
					// For Rates Service rate fallback included charges to CW1 charges if possible
					var updatedCarrierCharges = carrierCharges
						.Where(c => c.Line is WiseLine wiseLine && containerCommodityItem.SelectedRate.IsActive(wiseLine.WiseCharge)
									&& (wiseLine.WiseCharge.CustomCategory != WRConstants.ChargeCustomCategory.BOL || !bolCharges.Contains(wiseLine.ChargeCode.AC_Code)))
						.Select(c =>
							ApplySubjectToFallback(
								containerCommodityItem,
								c,
								carrierCharges,
								allCW1Charges))
						.WhereNotNull()
						// If a charge from rate service has subject-to fallback'ed to a CW1 charge
						// need to make sure that we do not add duplicates.
						.Where(proposed => proposed.IsFromRatesService || !charges.Contains(proposed));
					charges.AddRange(updatedCarrierCharges);
				}
				else
				{
					// For CW1 rate take charges as they are
					charges.AddRange(containerCommodityItem.SelectedRate.CalculatedResult.Where(c => c.Amount > 0 || c.IsInclusiveCalculator || !RemoveZeroCharges));
				}
			}

			var otherNonFreightCharges = relatedChargesProvider.GetNonCarrierCharges(charges)
				//Discard freight charges with Zero amount if user selected NOT to apply zero charges
				.Where(c => c.Amount > 0 || c.IsInclusiveCalculator || !RemoveZeroCharges)
				// Discard freight charges as they are loaded from the selected rate
				.Where(c => (Criteria.IsContainerised && !IsFCLFreightCost(c.Line.ParentRateEntry))
							|| (!Criteria.IsContainerised && !IsLCLFreightCost(c.Line.ParentRateEntry))
				);

			charges.AddRange(otherNonFreightCharges);

			NotApplicableRateLineRemover.RemoveItemsOverriddenBySpotCosts
				(
					Criteria,
					charges.Select(c => new Tuple<AccChargeCode, AutoRateInfo>(c.ChargeCode, c)).ToList(),
					(autoRateInfo, log) =>
					{
						charges.Remove(autoRateInfo);
						Logger.Information(Res.GetString("EA04684A-76D3-4C81-91B9-01AAC0F0A3C5", "Charge '{0}' from selected rate, {1}", autoRateInfo.ChargeCode.AC_Code, log));
					}
				);

			return charges;
		}

		AutoRateInfo ApplySubjectToFallback(ChooserContainerCommodity rateGroup, AutoRateInfo possibleSubjectToCharge, IEnumerable<AutoRateInfo> carrierCharges, IEnumerable<AutoRateInfo> allCW1Charges)
		{
			if (possibleSubjectToCharge.Amount == 0 && !possibleSubjectToCharge.IsInclusiveCalculator && RemoveZeroCharges)
			{
				return null;
			}

			if (!possibleSubjectToCharge.IsFromRatesService || !possibleSubjectToCharge.IsSubjectTo)
			{
				return possibleSubjectToCharge;
			}

			if (!DataRegistryRating.Instance.RateServiceFallbackSubjectToCharges.GetIsFallbackAllowed(WRConstants.RateProviders.CargoSphere, TransportModes.Sea, Criteria.ContainerMode.ToString()))
			{
				return possibleSubjectToCharge;
			}

			var frtCalculator = possibleSubjectToCharge.Line.GetCalculator<FreightInclusiveCalculator>();
			var matchingCW1Charges = allCW1Charges
				.Where(cw1Charge => cw1Charge.ChargeCode.PK == possibleSubjectToCharge.ChargeCode.PK);

			var bestMatchingCW1Charge = matchingCW1Charges
					.Select(c => new { Score = GetScoreMatchingChargeWithRateGroup(rateGroup, c), Charge = c })
					.OrderByDescending(c => c.Score)
					.Take(1)
					.Where(c => c.Score != 0)
					.Select(c => c.Charge)
					.FirstOrDefault();

			if (bestMatchingCW1Charge == null)
			{
				Logger.Information($"{LogEventTypes.RateLineFound} {possibleSubjectToCharge.Line.DisplayInfo()} with Subject To charge could not fallback to Costing as no match was found."); // Logging data
				return possibleSubjectToCharge;
			}

			carrierCharges
				.Where(c => c.ChargeCode.PK == frtCalculator?.ChargeCode)
				.ForEach(c => c.Line.IncludedLines.Remove(possibleSubjectToCharge.Line));

			Logger.Information($"{LogEventTypes.RateLineFound} {possibleSubjectToCharge.Line.DisplayInfo()} with Subject To charge fallback to RateLine {bestMatchingCW1Charge.Line.DisplayInfo()}."); // Logging data
			return bestMatchingCW1Charge;
		}

		int GetScoreMatchingChargeWithRateGroup(ChooserContainerCommodity rateGroup, AutoRateInfo cw1Charge)
		{
			var containerMatches = cw1Charge.Line.ParentRateEntry.Container?.PK == rateGroup.ContainerRef?.PK;
			//A blank commodity on the rateGroup means any rate's commodity is a match.
			var commodityMatches = (cw1Charge.Line.ParentRateEntry.TI_RH_NKCommodityCode == rateGroup.CommodityCode) || rateGroup.CommodityCode.IsEmpty;
			var containerIsBlank = cw1Charge.Line.ParentRateEntry.Container == null;
			var commodityIsBlank = cw1Charge.Line.ParentRateEntry.TI_RH_NKCommodityCode.IsEmpty;

			if (containerMatches && commodityMatches)
			{
				return 9;
			}
			else if ((containerMatches && commodityIsBlank) || (containerIsBlank && commodityMatches))
			{
				return 8;
			}
			else if (containerIsBlank && commodityIsBlank)
			{
				return 1;
			}
			else
			{
				return 0;
			}
		}

		IList<WiseEntry> Convert(Rate apiRate)
		{
			return Converter.Convert(ConversionContext, new[] { apiRate });
		}

		public ZString GetUniversalCarrierServiceLevel(ZString carrierServiceLevel, IEnumerable<OrgHeader> possibleServiceProviders)
		{
			var miscServices = context.Factory.Load<OrgMiscServ>(new ZQuery(OrgMiscServSchema.PK, possibleServiceProviders.WhereNotNull().Select(x => x.MiscServ.PK)));

			return miscServices.SelectMany(c => c.CarrierServiceLevels
					.Where(l => l.PL_Code == carrierServiceLevel)
					.Select(l => l.PL_CarrierServiceCode.IsEmpty ? l.PL_Code : l.PL_CarrierServiceCode))
				.FirstOrDefault();
		}

		public void ShowMoreRates()
		{
			CurrentPageID += 1;

			SendRatesRequest
			(
				filter: parentRateSelectorFilter,
				ratesQuery: previousRatesQuery,
				isValidForRatesService: true,
				pageID: CurrentPageID,
				universalCarrierLevels: previousUniversalCarrierLevels
			);
		}

		public void SendRatesRequest(
			IRateSelectorFilterValueProvider filter,
			RatesQuery ratesQuery,
			bool isValidForRatesService = true,
			int pageID = 0,
			IEnumerable<string> universalCarrierLevels = null)
		{
			parentRateSelectorFilter = filter;
			previousRatesQuery = ratesQuery;
			previousUniversalCarrierLevels = universalCarrierLevels;

			if (pageID == 0)
			{
				CurrentPageID = 0;
				RemoveAllRates();
			}
			RawData = string.Empty;

			if (ratesQuery == null)
			{
				return;
			}

			WiseRatesSearchRequestAsync wiseSearchAsync = null;
			var originalIsManualCostSelectMode = Criteria.IsManualCostSelectMode;
			var originalOrigin = Criteria.Origin;
			var originalDestination = Criteria.Destination;
			var originalCreditors = Criteria.Creditors;
			var originalContractNumbers = Criteria.CarrierContractNumbers;
			var originalNamedAccount = Criteria.NamedAccount;
			var measureChanger = Criteria.JobMeasures.BeginTemporaryChanges();
			IEnumerable<CW1RateCombinationResult> cw1Rates = null;
			IEnumerable<Rate> apiRates = null;
			List<(Rate, OrgHeader)> validWiseRates = null;
			IDisposable effectiveDateDisposer = null;

			long wiseRatesElapsedMilliseconds = 0;
			long cw1ElapsedMilliseconds = 0;
			Stopwatch wiseRatesStopwatch = null;

			try
			{
				if (wiseRatesProvider != null && isValidForRatesService && CanSearchWiseRates(ratesQuery, universalCarrierLevels))
				{
					if (!ratesQuery.ServiceLevel.IsNullOrEmpty() && universalCarrierLevels != null)
					{
						ratesQuery.ServiceLevel = ratesQuery.ServiceLevel.Intersect(universalCarrierLevels);
					}

					wiseRatesStopwatch = Stopwatch.StartNew();
					wiseSearchAsync = wiseRatesProvider.BeginGetRawRates(Criteria, ratesQuery, pageID);
				}
				if (cw1RatesProvider != null)
				{
					var cw1Stopwatch = Stopwatch.StartNew();
					(cw1Rates, effectiveDateDisposer) = new CW1RateProviderForRSL(context, Logger).FindRates(filter);
					cw1Stopwatch.Stop();
					cw1ElapsedMilliseconds = cw1Stopwatch.ElapsedMilliseconds;
				}

				if (wiseSearchAsync != null)
				{
					var wiseRates = wiseRatesProvider.EndGetRawRates(wiseSearchAsync);
					wiseRatesStopwatch.Stop();
					wiseRatesElapsedMilliseconds = wiseRatesStopwatch.ElapsedMilliseconds;

					if (wiseRates?.RatesSearchResponse != null)
					{
						var response = wiseRates.RatesSearchResponse;
						ConversionContext = new WiseRatesConversionContext(response, Criteria, searchTraceID: wiseRates.TraceID);
						TotalPages = response?.TotalPages ?? 1;
						apiRates = response?.Rates;
						UniversalServiceLevels = response?.ServiceLevels;

						filter.SetCargoSphereFilters(response?.CargoSphereContracts, response?.CargoSphereNamedAccounts);
					}

					RawData = wiseRates?.RawResponse;
				}

				validWiseRates = ValidateWiseRates(apiRates ?? Enumerable.Empty<Rate>());

				Criteria.Creditors = originalCreditors;
				Criteria.CarrierContractNumbers = originalContractNumbers;
				Criteria.NamedAccount = originalNamedAccount;
				AddCalculateCW1Rates(cw1Rates ?? Enumerable.Empty<CW1RateCombinationResult>());
				AddCalculatedWiseRates(validWiseRates);
			}
			finally
			{
				Criteria.IsManualCostSelectMode = originalIsManualCostSelectMode;
				Criteria.Origin = originalOrigin;
				Criteria.Destination = originalDestination;
				Criteria.Creditors = originalCreditors;
				Criteria.CarrierContractNumbers = originalContractNumbers;
				Criteria.NamedAccount = originalNamedAccount;
				measureChanger.Dispose();
				wiseSearchAsync?.Dispose();
				effectiveDateDisposer?.Dispose();
			}

			AutoSelectRates();

			ReportSearchUsage(wiseRatesElapsedMilliseconds, cw1ElapsedMilliseconds);
		}

		List<(Rate rate, OrgHeader serviceProvider)> ValidateWiseRates(IEnumerable<Rate> wiseRates)
		{
			var validRates = new List<(Rate, OrgHeader)>();

			foreach (var apiRate in wiseRates)
			{
				var (serviceProvider, convertedCarrierResult) = Converter.GetCarrierOrgHeader(ConversionContext, apiRate);
				if (convertedCarrierResult.IsMultiMapped)
				{
					wiseRatesProvider.Logger.Warning(Res.GetString("196A44EA-B5F3-4E96-9683-0B8AB63267FE", "{0} Rate ['{1}'] is discarded: {2}", apiRate.Provider, apiRate.ProviderRateId, convertedCarrierResult.Message));
					continue;
				}

				validRates.Add((apiRate, serviceProvider));
			}

			return validRates;
		}

		void ReportSearchUsage(long wiseRatesElapsedMilliseconds, long cw1ElapsedMilliseconds)
		{
			void PopulateResults(RateProviderSearchResult providerResult, IEnumerable<ChooserRateEntry> providerRates, long elapsedTime)
			{
				foreach (var rate in providerRates)
				{
					var isValid = true;

					if (rate.HasErrors)
					{
						providerResult.ErrorRates++;
						isValid = false;
					}

					if (rate.HasWarnings)
					{
						providerResult.WarningRates++;
						isValid = false;
					}

					if (isValid)
					{
						providerResult.ValidRates++;
					}

					providerResult.TotalRates++;
				}
				providerResult.ElapsedTime = (int)elapsedTime;
			}

			var rates = criteriaContainerCommodityList.SelectMany(c => c.Rates).ToArray();
			var cargoSphereRates = rates.Where(r => r.WiseRateEntry != null).ToArray();
			var cw1Rates = rates.Where(r => r.WiseRateEntry == null).ToArray();

			var result = new UsageRatesSearchResult();
			PopulateResults(result.CargoSphere, cargoSphereRates, wiseRatesElapsedMilliseconds);
			PopulateResults(result.CW1, cw1Rates, cw1ElapsedMilliseconds);

			RatingUsageCollector.ReportRateSelectorSearch(result);
		}

		public void AddCalculateCW1Rates(IEnumerable<CW1RateCombinationResult> cw1Rates)
		{
			foreach (var containerCommodityGroup in ContainerGroups)
			{
				var matchedRateCombinationResults = Criteria.IsContainerised
					? cw1Rates.Where(x =>
							x.ContainerTypePk == containerCommodityGroup.ContainerRef.PK &&
							x.CommodityCode == containerCommodityGroup.CommodityCode)
					: cw1Rates;

				foreach (var rateCombinationResult in matchedRateCombinationResults)
				{
					using (var measureChanger = rateCombinationResult.Criteria.JobMeasures.BeginTemporaryChanges())
					{
						if (rateCombinationResult.Criteria.IsContainerised)
						{
							measureChanger.SetTemporaryContainerTypeAndCommodityFilter(
								filterContainerTypePk: rateCombinationResult.ContainerTypePk,
								filterContainerQuality: Criteria.IsContainerised ? containerCommodityGroup.ContainerQuality : string.Empty,
								filterCommodityCode: rateCombinationResult.CommodityCode);
						}

						var serviceProvider = rateCombinationResult.HeadEntry.CarrierServiceLevelParent();
						if (serviceProvider != null)
						{
							Criteria.SelectedServiceProviderFromRateSelector = serviceProvider.PK;
						}

						var filterOptions = new NotApplicableRateLineRemover.FilterOptions
						{
							DisableSpotFilter = true,
							DisableInvalidRatesFilter = true
						};

						var entries = rateCombinationResult
							.Lines
							.Select(l => l.ParentRateEntry);
#if NETFRAMEWORK
						entries = entries.DistinctBy(x => x.PK);
#elif NET
						entries = Enumerable.DistinctBy(entries, x => x.PK);
#endif
						entries = entries.ToList();

						// When calculating lines with CST/CTB calculators, we will have to cache the line and traverse to original lines using autoRater.
						// Without passing the same factory to new RatingContext, removing a line from a cached list fails.
						// It may lead to duplicate key caching by Line.PK later.
						// For example, in FreightAutoRater.LoadCostOrCompanyTariffRateLines: linesRepository.Remove(fastLine,...)
						var context = RatingContext.CreateInstance(entries.FirstOrDefault());
						var freightAutoRater = new FreightAutoRater(context);

						var results = freightAutoRater.CalculateResultsForBestMatches(CostSell.Cost, rateCombinationResult.Criteria, entries, filterOptions);
						results.SumUpSameCharges();

						containerCommodityGroup.RateCollection.Add(
							new ChooserRateEntry(
								factory: context.Factory,
								logger: context.Logger,
								cw1Rate: rateCombinationResult.HeadEntry,
								services: chooserServices,
								calculatedResult: results,
								filters: parentRateSelectorFilter));
					}
				}
			}
		}

		void AddCalculatedWiseRates(List<(Rate rate, OrgHeader serviceProvider)> csRates)
		{
			if (csRates == null || !csRates.Any())
			{
				return;
			}

			if (Criteria.IsContainerised)
			{
				var measures = Criteria.JobMeasures;

				var originalIsManualCostSelectMode = Criteria.IsManualCostSelectMode;
				var originalCreditors = Criteria.Creditors;

				try
				{
					Criteria.IsManualCostSelectMode = true;
					var chargeCodeGroups = Criteria.Creditors?.ChargeCodeGroups ?? Array.Empty<string>();

					// Group the received rates by their container code. This code is the one
					// received from the CargoSphere response. It is the ISO code. It is assumed
					// all containers (that are not-null), have an ISO Code.
					foreach (var csRateGroup in csRates.GroupBy(x => x.rate.Container?.Code))
					{
						// The converter returns containers that also exist in the
						// criteria. If there are none, then there are no matches and no
						// results.
						// MARK1 (this will be referenced by a later comment)
						var (csContainers, _) = Converter.ConvertContainer(csRateGroup.ToList().First().rate, ConversionContext);
						if (csContainers.IsNullOrEmpty())
						{
							continue;
						}

						// Loop through the container-commodity list. This is a list of container/
						// commodities that come from the RatingCriteria. This also corresponds to
						// the tabs on the screen
						foreach (var containerCommodity in criteriaContainerCommodityList)
						{
							// Identify that this containerCommodity has some container
							// that was converted from the CS rate. If there are non then
							// maybe there are no results for this containerCommodity.
							var containerMatch = csContainers.FirstOrDefault(c => c.PK == containerCommodity.ContainerRef.PK);
							if (containerMatch == null)
							{
								continue;
							}

							foreach (var apiRateTuple in csRateGroup)
							{
								var rateContainerQuality = apiRateTuple.rate.ContainerQuality();
								var groupContainerQuality = containerCommodity.ContainerQuality ?? string.Empty;
								if (!string.Equals(rateContainerQuality, groupContainerQuality))
								{
									continue;
								}

								if (containerCommodity.SelectedRate?.WiseRateEntry != apiRateTuple.rate)
								{
									using (var measureChanger = measures.BeginTemporaryChanges())
									{
										measureChanger.SetTemporaryContainerTypeAndCommodityFilter(
											filterContainerTypePk: containerCommodity.ContainerRef.PK,
											filterContainerQuality: containerCommodity.ContainerQuality,
											filterCommodityCode: containerCommodity.CommodityCode);

										if (apiRateTuple.serviceProvider != null)
										{
											Criteria.Creditors = CreateCreditors(apiRateTuple.serviceProvider, chargeCodeGroups);
										}

										// Getting the fully converted wiseRates can result in many results.
										// one result for each container that is found when converting the
										// rate container. This is similar to line marked by MARK1. Except
										// at MARK1 we dont care if we have many results for different containerCommodity.
										// Here, we do care. We only want the rate that matches this containerCommodity.
										var convertedRates =
											GetOrCreateConvertedWiseRate(apiRateTuple.rate)
											.Where(r => r.Container.PK == containerMatch.PK)
											.ToArray();

										var ratingContext = new RatingContext();
										var filterOptions = new NotApplicableRateLineRemover.FilterOptions { DisableSpotFilter = true };
										var freightAutoRater = new FreightAutoRater(ratingContext);
										var results = freightAutoRater.CalculateResultsForBestMatches(CostSell.Cost, Criteria, convertedRates, filterOptions);

										var chooserRate =
											new ChooserRateEntry
											(
												context.Factory,
												ratingContext.Logger,
												apiRateTuple.serviceProvider,
												apiRateTuple.rate,
												convertedRates,
												chooserServices,
												results,
												parentRateSelectorFilter
											);
										containerCommodity.RateCollection.Add(chooserRate);
										chooserRate.ChargeIsActiveChanged += ChooserRate_ChargeIsActiveChanged;
									}
								}
								else if (!containerCommodity.RateCollection.Contains(containerCommodity.SelectedRate))
								{
									containerCommodity.RateCollection.Add(containerCommodity.SelectedRate);
								}

								Criteria.Creditors = originalCreditors;
							}
						}
					}
				}
				finally
				{
					Criteria.IsManualCostSelectMode = originalIsManualCostSelectMode;
					Criteria.Creditors = originalCreditors;
				}
			}
			else
			{
				AddCalculatedLCLWiseRates(csRates);
			}
		}

		void AddCalculatedLCLWiseRates(List<(Rate rate, OrgHeader serviceProvider)> csRates)
		{
			var originalIsManualCostSelectMode = Criteria.IsManualCostSelectMode;
			var originalCreditors = Criteria.Creditors;

			try
			{
				Criteria.IsManualCostSelectMode = true;
				var chargeCodeGroups = Criteria.Creditors?.ChargeCodeGroups ?? Array.Empty<string>();

				var lclTab = criteriaContainerCommodityList.FirstOrDefault();

				if (lclTab != null)
				{
					foreach (var apiRateTuple in csRates)
					{
						if (lclTab.SelectedRate?.WiseRateEntry != apiRateTuple.rate)
						{
							if (apiRateTuple.serviceProvider != null)
							{
								Criteria.Creditors = CreateCreditors(apiRateTuple.serviceProvider, chargeCodeGroups);
							}

							var convertedRates = GetOrCreateConvertedWiseRate(apiRateTuple.rate);

							var ratingContext = new RatingContext();
							var filterOptions = new NotApplicableRateLineRemover.FilterOptions { DisableSpotFilter = true };
							var freightAutoRater = new FreightAutoRater(ratingContext);
							var results = freightAutoRater.CalculateResultsForBestMatches(CostSell.Cost, Criteria, convertedRates, filterOptions);
							var chooserRate = new ChooserRateEntry(context.Factory, ratingContext.Logger, apiRateTuple.serviceProvider, apiRateTuple.rate, convertedRates, chooserServices, results);

							lclTab.RateCollection.Add(chooserRate);
							chooserRate.ChargeIsActiveChanged += ChooserRate_ChargeIsActiveChanged;
						}
						else if (!lclTab.RateCollection.Contains(lclTab.SelectedRate))
						{
							lclTab.RateCollection.Add(lclTab.SelectedRate);
						}

						Criteria.Creditors = originalCreditors;
					}
				}
			}
			finally
			{
				Criteria.IsManualCostSelectMode = originalIsManualCostSelectMode;
				Criteria.Creditors = originalCreditors;
			}
		}

		Creditors CreateCreditors(OrgHeader carrier, IEnumerable<string> chargeCodeGroups)
		{
			var source = new List<string> { (NoResString)"CS Calculated Rate Selector" };   // internal use

			if (chargeCodeGroups.Any(g => string.IsNullOrEmpty(g)))
			{
				return Creditors.New(OrgWithSource.New(carrier, source));
			}

			var creditors = new Creditors();
			foreach (var chargeCodeGroup in chargeCodeGroups)
			{
				creditors[chargeCodeGroup].Add(1, OrgWithSource.New(carrier, source));
			}

			return creditors;
		}

		AutoRateInfoCollection CalculateForRateService(ChooserContainerCommodity chooserContainerCommodity)
		{
			var originalCreditors = Criteria.Creditors;
			var originalIsManualCostSelectMode = Criteria.IsManualCostSelectMode;

			try
			{
				Criteria.IsManualCostSelectMode = true;

				var entry = chooserContainerCommodity.SelectedRate.RateEntry as WiseEntry;
				var commodityContainer = chooserContainerCommodity.ContainerRef;
				var ratesServiceCarrier = entry.CarrierServiceLevelParent();
				var chargeCodeGroups = Criteria.Creditors?.ChargeCodeGroups ?? Array.Empty<string>();

				if (ratesServiceCarrier != null)
				{
					Criteria.Creditors = CreateCreditors(ratesServiceCarrier, chargeCodeGroups);
				}

				chooserContainerCommodity.SelectedRate.ConvertedRates.ForEach(
					rate => UpdateWiseRateEntry
					(
						rate,
						chooserContainerCommodity.CommodityCode
					));

				var freightAutoRater = new FreightAutoRater(context);
				return freightAutoRater.CalculateResultsForBestMatches(CostSell.Cost, Criteria, chooserContainerCommodity.SelectedRate.ConvertedRates);
			}
			finally
			{
				Criteria.Creditors = originalCreditors;
				Criteria.IsManualCostSelectMode = originalIsManualCostSelectMode;
			}
		}

		void UpdateWiseRateEntry(IRateEntry rate, ZString commodityCode)
		{
			if (rate is WiseEntry wiseEntry)
			{
				wiseEntry.TI_RH_NKCommodityCode = commodityCode;
			}
		}

		bool CanSearchWiseRates(RatesQuery ratesQuery, IEnumerable<string> universalCarrierLevels)
		{
			Argument.NotNull(ratesQuery, nameof(ratesQuery));

			var canSearch = true;

			if (!ratesQuery.ServiceLevel.IsNullOrEmpty() && (universalCarrierLevels?.Any() ?? false) && !ratesQuery.ServiceLevel.Intersect(universalCarrierLevels).Any())
			{
				wiseRatesProvider?.Logger.Error(
					ResString.GetMultilingualString(
						"5EE8A67B-B28B-47DB-9004-86601C9C8EFF",
						"None of Carrier Service level is universal and cannot be used for CargoSphere rates search."));
				canSearch = false;
			}

			return canSearch;
		}

		public bool HasMorePages => TotalPages - 1 > CurrentPageID;
		public int TotalPages { get; private set; }
		public int CurrentPageID { get; private set; }

		IRateSelectorFilterValueProvider parentRateSelectorFilter;
		RatesQuery previousRatesQuery;
		IEnumerable<string> previousUniversalCarrierLevels;

		void AutoSelectRates()
		{
			var appliedCharges = Criteria.GetExistingCharges(fromAllCompanies: true);
			var appliedRateIds = appliedCharges
				.Select(c => c.RateAttributes?.GetSingleValue<string>(JobChargeAttribTypeList.Codes.RateId))
				.WhereNotNull()
				.Distinct()
				.ToList();

			foreach (var containerCommodity in criteriaContainerCommodityList)
			{
				// If we have 1 rate on the tab we can select it. There are no many options for the user to chose from.
				if (containerCommodity.SelectedRate == null && containerCommodity.RateCollection.Count == 1)
				{
					containerCommodity.SelectedRate = containerCommodity.RateCollection[0];
					continue;
				}

				// Find and auto select a rate that was previously applied to the job. For example if the user has already run rate selector
				// before and applied this rate. It makes sense to auto select it.
				var appliedRate = containerCommodity.Rates.FirstOrDefault(x => appliedRateIds.Contains(x.RateId));
				if (appliedRate != null)
				{
					containerCommodity.SelectedRate = appliedRate;
					continue;
				}

				var reservedRate = containerCommodity.Rates.FirstOrDefault(x => x.WiseRateEntry?.ReservedForJobIDs != null && x.WiseRateEntry.ReservedForJobIDs.Contains((string)Criteria.JobID));
				if (reservedRate != null)
				{
					containerCommodity.SelectedRate = reservedRate;
				}
			}
		}

		void RemoveAllRates()
		{
			wiseRateToConvertedRates.Clear();
			foreach (var containerCommodity in criteriaContainerCommodityList)
			{
				foreach (ChooserRateEntry rate in containerCommodity.RateCollection)
				{
					if (rate.WiseRateEntry != null)
					{
						rate.ChargeIsActiveChanged -= ChooserRate_ChargeIsActiveChanged;
					}
				}
				containerCommodity.RateCollection.RemoveAll();
			}
		}

		void ClearCachedMappings()
		{
			if (ConversionContext != null)
			{
				ConversionContext = new WiseRatesConversionContext(ConversionContext.Response, ConversionContext.Criteria);
			}
		}

		public void UpdateMappings()
		{
			ClearCachedMappings();
			wiseRateToConvertedRates.Clear();

			foreach (var rate in SelectedRates.Where(x => x.WiseRateEntry != null))
			{
				rate.UpdateConvertedRates(GetOrCreateConvertedWiseRate(rate.WiseRateEntry));
			}
		}

		public static bool IsFCLFreightCost(IRateEntry entry) => entry.TI_RateCategory == RatingConstants.RateCategory.FCL && RateHasFRTCharges(entry);
		public static bool IsLCLCost(IRateEntry entry) => AcceptedRatesCategoryForLCL.Contains(entry.TI_RateCategory) && IsValidLCLCharges(entry);
		public static bool IsLCLFreightCost(IRateEntry entry) => entry.TI_RateCategory == RatingConstants.RateCategory.LCL && RateHasFRTCharges(entry);

		static List<string> AcceptedRatesCategoryForLCL => new List<string> { RatingConstants.RateCategory.LCL, RatingConstants.RateCategory.ORG, RatingConstants.RateCategory.DST };
		static List<string> AcceptedChargeCodeGroupForLCL => new List<string> { ChargeCodeGroupList.Codes.Freight, ChargeCodeGroupList.Codes.Origin, ChargeCodeGroupList.Codes.Destination };

		public IEnumerable<string> ErrorsAndWarningsFromSearch => DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value
						? (WiseRatesProviderLogger as ElementaryLogger)?.GetAllLogs() ?? Enumerable.Empty<string>()
						: ErrorsFromSearch.Union(WarningsFromSearch);

		public IEnumerable<string> WarningsFromSearch => (WiseRatesProviderLogger as ElementaryLogger)?.Warnings ?? Enumerable.Empty<string>();

		public IEnumerable<string> ErrorsFromSearch => (WiseRatesProviderLogger as ElementaryLogger)?.Errors ?? Enumerable.Empty<string>();

		public string RawData { get; private set; }

		public List<ZString> GetZeroCharges()
		{
			var chargeCodesWithZeroPriceAmount = new List<ZString>();

			foreach (var containerCommodity in criteriaContainerCommodityList.Where(x => x.SelectedRate != null))
			{
				var wiseRate = containerCommodity.SelectedRate.WiseRateEntry;

				if (wiseRate != null)
				{
					var zeroAmountNonInclusiveCharges = containerCommodity.SelectedRate.ActiveConvertedCharges.Where(w => !w.Value.IsInclusive() && !w.Value.IsPercentage() && w.Key.HasZeroRate()).Select(w => (ZString)w.Key.ChargeCode);
					chargeCodesWithZeroPriceAmount.AddRange(zeroAmountNonInclusiveCharges);
				}

				var calc = containerCommodity.SelectedRate.SelectedCalculatedResult;

				if (calc != null)
				{
					var result = calc.Where(x => x.Amount == 0 && !x.IsInclusiveCalculator && !x.IsPercentageCalculator && x.ChargeCode != null)
						.Select(x => x.ChargeCode.AC_Code);
					chargeCodesWithZeroPriceAmount.AddRange(result);
				}
			}

			return chargeCodesWithZeroPriceAmount.Distinct().ToList();
		}

		public void Validate(IEnumerable<ZString> excludeFromValidation)
		{
			ValidateAllChargeCodesAreMapped(excludeFromValidation);
		}

		public void Validate() => Validate(Enumerable.Empty<ZString>());

		public void ValidateAllChargeCodesAreMapped(IEnumerable<ZString> excludeFromValidation)
		{
			if (excludeFromValidation == null)
			{
				throw new ArgumentNullException(nameof(excludeFromValidation));
			}

			if (UnmappedCharges.Count > 0)
			{
				ClearCachedMappings();
				wiseRateToConvertedRates.Clear();
				UnmappedCharges.RemoveAndDeleteAll();
			}
			var unmappedChargeCodes = new HashSet<string>();
			foreach (var containerCommodity in criteriaContainerCommodityList)
			{
				var wiseRate = containerCommodity.SelectedRate?.WiseRateEntry;
				if (wiseRate != null)
				{
					foreach (var pair in containerCommodity.SelectedRate.ActiveConvertedCharges)
					{
						var wiseLine = pair.Value;
						if (wiseLine.TL_AC.IsEmpty)
						{
							var wiseCharge = pair.Key;
							var universalCode = wiseCharge.ChargeCode;
							if (!string.IsNullOrEmpty(universalCode) && !unmappedChargeCodes.Contains(universalCode))
							{
								if (RemoveZeroCharges && excludeFromValidation.Contains(universalCode))
								{
									//this charge is zero and user doesn't want to apply zero charges,
									//so we don't ask user to map unmapped zero charges that we already discard
									continue;
								}

								var refCharge = wiseCharge.ChargeCodeInfo ?? ConversionContext?.Response?.ChargeCodes?.First(x => x.Code == universalCode);
								unmappedChargeCodes.Add(universalCode);
								UnmappedCharges.AddNew(universalCode, refCharge?.Description ?? universalCode);
							}
						}
					}
				}
			}
		}

		IEnumerable<ChooserRateEntry> SelectedRates => criteriaContainerCommodityList.Select(x => x.SelectedRate).Where(x => x != null);

		#region Booking Info

		public bool SelectedRatesHaveDifferentSchedule
		{
			get
			{
				return criteriaContainerCommodityList.Count > 1 &&
					SelectedRates
					.Select(s => s.WiseRateEntry?.BookingInfo?.Schedule)
					.Distinct(new CargoSphereScheduleComparer())
					.Skip(1)
					.Any();
			}
		}

		#endregion

		#region CarrierServiceLevel

		public string ServiceLevel => SelectedRates.FirstOrDefault()?.CarrierServiceLevel ?? string.Empty;
		public bool ServiceLevelIsMapped => SelectedRates.All(x => x.CarrierServiceLevelIsMapped);
		public bool CarrierIsMapped => SelectedRates.All(x => x.CarrierIsMapped);
		public IEnumerable<ChooserRateEntry> NotMappedServiceLevelRates => SelectedRates.Where(x => !x.CarrierServiceLevelIsMapped);
		public IEnumerable<ChooserRateEntry> NotMappedCarrierRates => SelectedRates.Where(x => !x.CarrierIsMapped);

		public bool SelectedRatesHaveDifferentServiceProvider
		{
			get
			{
				return criteriaContainerCommodityList.Count > 1 &&
					SelectedRates.Select(x => x.RateEntry.ParentRatingHeader.TH_OH)
						.Distinct()
						.Skip(1)
						.Any();
			}
		}

		public bool SelectedRatesHaveDifferentCarrierServiceLevel
		{
			get
			{
				return criteriaContainerCommodityList.Count > 1 &&
					SelectedRates.Select(x => x.CarrierServiceLevel)
						.Distinct(StringComparer.OrdinalIgnoreCase)
						.Skip(1)
						.Any();
			}
		}

		public RefServiceLevel[] UniversalServiceLevels;

		#endregion

		#region Contract Numbers

		public bool SelectedRatesHaveDifferentCarrierContractNumbers
		{
			get
			{
				return criteriaContainerCommodityList.Count > 1 &&
					SelectedRates.Select(x => x.ContractNumber ?? string.Empty)
						.Distinct(StringComparer.OrdinalIgnoreCase)
						.Skip(1)
						.Any();
			}
		}

		#endregion

		#region Spot rates

		public bool SelectedRatesHaveSpotRate
		{
			get
			{
				return SelectedRates?.Any(r => r.WiseRateEntry?.BookingInfo != null) ?? false;
			}
		}

		#endregion

		#region Named Account

		/// <summary>
		/// List of all named accounts that are on selected rates.
		/// If there are more than one, the user can pick one to go on the job.
		/// </summary>
		public IEnumerable<string> AllNamedAccounts
		{
			get
			{
				(_, var namedAccounts) = GetNamedAccountsOfSelectedRates(unionElseIntersection: true);
				return namedAccounts;
			}
		}

		public bool SelectedRatesHaveDifferentNamedAccounts
		{
			get
			{
				if (criteriaContainerCommodityList.Count <= 1)
				{
					return false;
				}

				(var hasNamedAccounts, var commonNames) = GetNamedAccountsOfSelectedRates(unionElseIntersection: false);
				return hasNamedAccounts && commonNames.Count == 0;
			}
		}

		(bool hasNamedAccounts, HashSet<string> namedAccounts) GetNamedAccountsOfSelectedRates(bool unionElseIntersection)
		{
			var namedAccounts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			bool hasNamedAccounts = false;
			bool isFirst = true;
			foreach (var selectedRate in SelectedRates)
			{
				var rateNames = selectedRate.WiseRateEntry != null
					? selectedRate.WiseRateEntry.NamedAccounts
					: selectedRate.RateEntry.NamedAccounts;
				var rateNamesNonBlank = rateNames?.Where(x => !string.IsNullOrEmpty(x)) ?? Enumerable.Empty<string>();
				if (isFirst)
				{
					isFirst = false;
					namedAccounts.UnionWith(rateNamesNonBlank);
					hasNamedAccounts = namedAccounts.Count > 0;
				}
				else if (unionElseIntersection)
				{
					namedAccounts.UnionWith(rateNamesNonBlank);
				}
				else
				{
					namedAccounts.IntersectWith(rateNamesNonBlank);
					if (namedAccounts.Count == 0)
					{
						break;
					}
				}
			}

			return (hasNamedAccounts, namedAccounts);
		}

		public void ApplyServiceLevelBackToJobIfNeeded()
		{
			var serviceLevel = SelectedRates.FirstOrDefault()?.CarrierServiceLevel;
			if (!string.IsNullOrEmpty(serviceLevel))
			{
				Criteria.UpdateServiceLevel(serviceLevel);
			}
		}

		public void ApplyNamedAccountBackToJob(string namedAccount)
		{
			if (!string.IsNullOrWhiteSpace(namedAccount))
			{
				Criteria.UpdateNamedAccount(namedAccount);
			}
		}

		public IList<string> SelectedDistinctNonBlankContractNumbers => SelectedRates
			.Select(s => s.ContractNumber)
			.Where(s => !string.IsNullOrEmpty(s))
			.Distinct()
			.ToList();

		public void ApplyContractNumberBackToJob(UpdateCarrierContractNumberToken token)
		{
			Criteria.UpdateCarrierContractNumber(token);
		}

		public void ApplyCarrierQuoteNumberBackToJob()
		{
			foreach (var item in criteriaContainerCommodityList.Where(x => x.SelectedRate != null && x.ContainerRef != null))
			{
				Criteria.UpdateContainersCarrierQuoteNumber(item.ContainerRef.PK, item.SelectedRate.CarrierQuoteNumber);
			}

			var spotQuoteNumbers = SelectedRates.Select(rate => rate.CarrierQuoteNumber)
				.Where(cqn => !string.IsNullOrEmpty(cqn) && cqn.Count(c => c == '_') == 2);

			if (spotQuoteNumbers.Any())
			{
				var number = string.Join("/", spotQuoteNumbers.Select(n => new ZString(n).SubstringSafe(0, n.IndexOf('_', 2))));
				while (number.Length > AutoCusEntryNum.Schema.CE_EntryNumMaxLength)
				{
					number = number.Substring(0, number.LastIndexOf('/'));
				}

				Criteria.UpdateCarrierQuoteNumber(number);
			}

			foreach (var rate in SelectedRates)
			{
				var cqn = rate.CarrierQuoteNumber;
				if (!string.IsNullOrEmpty(cqn) && (!spotQuoteNumbers.Any() || !spotQuoteNumbers.Contains(cqn)))
				{
					Criteria.UpdateCarrierQuoteNumber(cqn);
				}
			}
		}

		public void SendBookingInformationToCarrier()
		{
			Criteria.SendBookingInformationToCarrier();
		}

		public void ApplySpotBookingTermsBackToJobIfNeeded()
		{
			var spotBookingTerms =
				criteriaContainerCommodityList
				.Where(i => i.SelectedRate?.WiseRateEntry?.BookingInfo?.BookingTerms?.Items?.Any() == true)
				.ToDictionary(
					i => Res.GetString("8791929b-69ca-412a-9421-556e7adbd88b", "Spot Booking Terms for '{0} ({1})':"
					, i.ContainerRef.RC_Code, i.CommodityCode), i => i.SelectedRate.WiseRateEntry.BookingInfo.BookingTerms);

			var terms = BookingTermsConverter.Convert(spotBookingTerms);
			if (!string.IsNullOrEmpty(terms))
			{
				Criteria.UpdateSpotBookingTerms(terms);
			}
		}

		#endregion

		public OrgHeader GetNewCarrierToApplyToJob()
		{
			OrgHeader newCarrier = GetCarrierFromSelectedRates();

			return CarrierNeedsToBeAppliedToJob(Criteria, newCarrier)
				? newCarrier
				: null;
		}

		public OrgHeader GetCarrierFromSelectedRates()
		{
			OrgHeader carrier = null;
			var rateEntry = SelectedRates.Select(x => x.RateEntry).WhereNotNull().FirstOrDefault();
			if (rateEntry != null)
			{
				carrier = rateEntry.CarrierServiceLevelParent();
			}
			else
			{
				var wiseRate = SelectedRates.Select(x => x.WiseRateEntry).WhereNotNull().FirstOrDefault();
				if (wiseRate != null && ConversionContext != null)
				{
					(carrier, _) = Converter.GetCarrierOrgHeader(ConversionContext, wiseRate);
				}
			}
			return carrier;
		}

		bool CarrierNeedsToBeAppliedToJob(IAutoRating criteria, OrgHeader newCarrier)
		{
			if (newCarrier == null)
			{
				return false;
			}

			return !criteria.PossibleServiceProviders?.Any(i => i.PK == newCarrier.PK) ?? true;
		}

		public (ZString Origin, ZString Destination) GetNewLocationsToApplyBackToJob()
		{
			var firstRate = SelectedRates.FirstOrDefault()?.RateEntry;
			if (firstRate == null
				|| RatingDataRegistry.Instance.MultiModalRatingCost.Value)
			{
				return (ZString.Empty, ZString.Empty);
			}

			var firstRateOrigin = firstRate.Origin();
			var firstRateDestination = firstRate.Destination();

			if ((firstRateOrigin == null || firstRateDestination == null))
			{
				var wiseRate = SelectedRates.FirstOrDefault().WiseRateEntry;
				if (wiseRate != null)
				{
					var badOrigin = firstRateOrigin == null ? wiseRate.Origin : string.Empty;
					var badDestination = firstRateDestination == null ? wiseRate.Destination : string.Empty;
					var msg = (badOrigin + " " + badDestination).Trim();
					if (!string.IsNullOrEmpty(msg))
					{
						ErrorReporter.ReportOnce("WiseRate unknown location code", msg);
					}
				}

				return (ZString.Empty, ZString.Empty);
			}

			var firstRateOriginCode = firstRateOrigin.Code;
			var firstRateDestinationCode = firstRateDestination.Code;

			var allSelectedRatesHaveSameOriginDestination =
				SelectedRates.Skip(1).All(r =>
				r.RateEntry != null &&
				r.RateEntry.TI_OriginLRC == firstRateOriginCode &&
				r.RateEntry.TI_DestinationLRC == firstRateDestinationCode);

			if (!allSelectedRatesHaveSameOriginDestination)
			{
				return (ZString.Empty, ZString.Empty);
			}

			var newOrigin = ZString.Empty;
			var newDestination = ZString.Empty;

			if (LocationHelper.GetLocationType(firstRateOriginCode) == LocationHelper.LocationType.Port)
			{
				newOrigin = firstRateOriginCode;
			}

			if (LocationHelper.GetLocationType(firstRateDestinationCode) == LocationHelper.LocationType.Port)
			{
				newDestination = firstRateDestinationCode;
			}

			return (newOrigin, newDestination);
		}

		public void ApplyLocationsBackToJob(ZString origin, ZString destination)
		{
			if (!origin.IsEmpty)
			{
				Criteria.UpdateOrigin(origin);
			}

			if (!destination.IsEmpty)
			{
				Criteria.UpdateDestination(destination);
			}
		}

		public void ApplyCarrierBackToJob()
		{
			var carrier = GetCarrierFromSelectedRates();

			Criteria.SelectedServiceProviderFromRateSelector = carrier?.PK ?? ZGuid.Empty;

			if (CarrierNeedsToBeAppliedToJob(Criteria, carrier))
			{
				Criteria.UpdateCarrier(carrier);
			}
		}

		public IEnumerable<IContainerPenalty> GetContainerPenaltiesToApplyToJob()
		{
			var result = new List<ContainerPenalty>();

			foreach (var item in criteriaContainerCommodityList)
			{
				if (item.SelectedRate?.WiseRateEntry?.BookingInfo?.Penalties?.Any() == true)
				{
					result.AddRange(
						item
						.SelectedRate
						.WiseRateEntry
						.BookingInfo
						.Penalties
						.Select(p => new ContainerPenalty(item.ContainerRef.RC_Code)
						{
							CPY_ProcessType = p.Direction,
							CPY_PenaltyType = p.Type,
							CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
							CPY_FreeTime = new DateTime(ZDateTime.Now.Year, 1, p.StartDay),
							CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
							CPY_PerUnitCost = p.PerUnitRate,
							CPY_RX_NKCurrency = p.Currency
						})
					);
				}
			}

			return result;
		}

		public void ApplyContainerPenaltiesBackToJob(IEnumerable<IContainerPenalty> penalties, bool deleteExistingPenalties)
		{
			if (penalties.Any())
			{
				Criteria.UpdateContainerPenalties(penalties, deleteExistingPenalties);
			}
		}

		public ScheduleDetail[] GetSelectedScheduleDetails()
		{
			var bookingInfo =
				SelectedRates
				.Select(r => r.WiseRateEntry)
				.WhereNotNull()
				.Select(w => w.BookingInfo)
				.FirstOrDefault();
			if (bookingInfo?.Schedule?.ScheduleDetails?.Any() == true)
			{
				return bookingInfo.Schedule.ScheduleDetails;
			}

			return null;
		}

		public void ApplyTransportsBackToJob(IEnumerable<TransportLeg> transports)
		{
			if (transports.Any())
			{
				Criteria.UpdateTransports(transports);
			}
		}

		public UniversalChargeCodeMapBizoCollection UnmappedCharges => unmappedCharges ?? (unmappedCharges = new UniversalChargeCodeMapBizoCollection(context.Factory));

		public bool IsValid => UnmappedCharges.Count == 0;

		UniversalChargeCodeMapBizoCollection unmappedCharges;

		/// <summary>
		/// This may expand the single wiseRate into multiple, depending on
		/// how many CW1 containers (that are part of the criteria) match.
		/// </summary>
		IList<WiseEntry> GetOrCreateConvertedWiseRate(Rate wiseRate)
		{
			var containers = Criteria.GetContainers().Select(c => c.PK).ToArray();
			var key = (containers, wiseRate);

			if (!wiseRateToConvertedRates.TryGetValue(key, out var result))
			{
				result = Convert(wiseRate);
				wiseRateToConvertedRates[key] = result;
			}
			return result;
		}

		readonly Dictionary<(ZGuid[], Rate), IList<WiseEntry>> wiseRateToConvertedRates = new Dictionary<(ZGuid[], Rate), IList<WiseEntry>>();

#if DEBUG
		public void AddWiseRatesForTest(RatesSearchResponse response)
		{
			ConversionContext = new WiseRatesConversionContext(response, Criteria);
			var validRates = ValidateWiseRates(response.Rates);
			AddCalculatedWiseRates(validRates);
			AutoSelectRates();
		}
#endif

		bool inChargeIsActiveChanged;

		void ChooserRate_ChargeIsActiveChanged(object sender, ChooserRateEntry.ChargeIsActiveChangedEventArgs e)
		{
			// recursion prevention
			if (inChargeIsActiveChanged)
			{
				return;
			}

			inChargeIsActiveChanged = true;
			try
			{
				var chooserRate = (ChooserRateEntry)sender;
				var apiRate = chooserRate.WiseRateEntry;
				foreach (var containerCommodity in criteriaContainerCommodityList)
				{
					foreach (ChooserRateEntry otherRate in containerCommodity.Rates)
					{
						if (!object.ReferenceEquals(chooserRate, otherRate)
							&& object.ReferenceEquals(apiRate, otherRate.WiseRateEntry))
						{
							otherRate.SetActive(e.Charge, e.IsActive);
						}
					}
				}
			}
			finally
			{
				inChargeIsActiveChanged = false;
			}
		}

		static bool RateHasFRTCharges(IRateEntry rateEntry)
		{
			return rateEntry.ChildRateLines.Any(x => x.ChargeCode?.AC_ChargeGroup.ToString() == ChargeCodeGroupList.Codes.Freight);
		}

		static bool IsValidLCLCharges(IRateEntry rateEntry)
		{
			return rateEntry.ChildRateLines.Any(x => AcceptedChargeCodeGroupForLCL.Contains(x.ChargeCode?.AC_ChargeGroup.ToString()));
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public void MapCarrier(Func<ChooserRateEntry, (OrgHeader, string)> promptUserToMapCarrier)
		{
			var mappedCarriers = new Dictionary<string, OrgHeader>();
			foreach (var unmappedItem in NotMappedCarrierRates)
			{
				var carrierAsString = unmappedItem?.WiseRateEntry?.Carrier;
				if (carrierAsString != null)
				{
					if (!mappedCarriers.ContainsKey(carrierAsString))
					{
						var (carrier, scacOrC1Code) = promptUserToMapCarrier(unmappedItem);
						if (carrier != null)
						{
							Logger.Warning(Res.GetString("D36E5273-C56E-4687-B728-F2B963D363DC", "Carrier '{0}' is assigned with SCAC/C1 Code '{1}' during the Operation.", carrier.OH_Code, scacOrC1Code));
						}
						unmappedItem.ServiceProvider = carrier;
						mappedCarriers.Add(carrierAsString, carrier);
					}
					else
					{
						unmappedItem.ServiceProvider = mappedCarriers[carrierAsString];
					}
				}
			}
		}

		public ZString TransportMode
		{
			get
			{
				var criteria = Criteria;

				ZString transportMode = criteria.IsSeaFreight ? Core.Constants.TransportModes.Sea
					: criteria.IsAirFreight ? Core.Constants.TransportModes.Sea
					: criteria.IsRoadFreight ? Core.Constants.TransportModes.Road
					: criteria.IsRailFreight ? Core.Constants.TransportModes.Rail : "";
				return transportMode;
			}
		}

		public ZString ContainerMode => Criteria.IsContainerised ? ContainerModes.FCL : ContainerModes.LCL;
	}
}
