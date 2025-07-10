using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class FreightRateEntryFilter : RateEntryFilter
	{
		public FreightRateEntryFilter(RatingCriteria criteria, bool isCosting, BusinessObjectFactory factory, ILogger logger)
			: base(criteria, isCosting, factory, logger)
		{
		}

		#region Find

		protected override List<IRateEntry> FindBestMatches(IEnumerable<IRateEntry> entries)
		{
			var result = new List<IRateEntry>();
			var autoratingMode = isAutoratingForCost ? criteria.CostSpotRateInfo.AutoratedMode : criteria.SellSpotRateInfo.AutoratedMode;

			switch (autoratingMode)
			{
				case Constants.FreightRateAutoratingModes.Code.AllInRate:
					if (!isAutoratingForCost && criteria.GatewayConfiguration.ContinueAutoratingRevenueFromIntercompanyTariff)
					{
						result.AddRange(GetStandardRateEntries(entries));
					}
					break;
				case Constants.FreightRateAutoratingModes.Code.FreightPlusRate:
					result.AddRange(GetStandardRateEntries(entries));
					break;
				default:
					result.AddRange(GetStandardRateEntries(entries));
					break;
			}

			return result;
		}

		IEnumerable<IRateEntry> GetStandardRateEntries(IEnumerable<IRateEntry> unfilteredEntries)
		{
			var result = new List<IRateEntry>();

			result.AddRange(base.FindBestMatches(unfilteredEntries));

			if (result.Count == 0 && criteria.RateTypeToUse == RateType.Forwarding && criteria.GetVia(GetCostOrSell()) != null)
			{
				List<IRateEntry> leg1Entries;
				List<IRateEntry> leg2Entries;
				if (GetHubbingEntries(unfilteredEntries, out leg1Entries, out leg2Entries))
				{
					result.AddRange(leg1Entries);
					result.AddRange(leg2Entries);
				}
			}

			return result;
		}

		#endregion

		#region Filters

		protected override void FilterAdditional()
		{
			rateEntriesRepository.FilterEntries(x => RateCategoryFilter(RateCategoryGroup.Freight, x, criteria));
			rateEntriesRepository.FilterEntries(RateCategoryFilter);
			rateEntriesRepository.FilterEntries(DateFilter);

			if (!criteria.IsLooseRateSearchForCarrierConnect)
			{
				rateEntriesRepository.FilterEntries(AircraftTypeFilter);
				rateEntriesRepository.FilterEntries(FMCTariffIDFilter);
				rateEntriesRepository.FilterEntries(IsNonOperatingReeferFilter);
			}
		}

		protected override void FilterLocations()
		{
			rateEntriesRepository.FilterEntries(HasMatchingLocation);

			base.FilterLocations();
		}

		#region Container Class

		protected override RefContainerCollection GetContainersInSameClass(ZGuid containerPK)
		{
			return criteria.GetContainersInSameClass(containerPK);
		}

		#endregion

		#region Mode Filter

		string RateCategoryFilter(IRateEntry entry)
		{
			if (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.Value)
			{
				var bcnReason = RateCategoryBCNOrSCNFilter(criteria.FreightMode, FreightMode.BCN, entry, Constants.RateMode.BCN);
				if (bcnReason != null)
				{
					return bcnReason;
				}
			}

			var scnReason = RateCategoryBCNOrSCNFilter(criteria.FreightMode, FreightMode.SCN, entry, Constants.RateMode.SCN);
			if (scnReason != null)
			{
				return scnReason;
			}

			var raiReason = RateCategorySpecificModeFilter(criteria.FreightMode, entry, FreightMode.RAI, new[] { Constants.RateMode.ALL, Constants.RateMode.LRA, Constants.RateMode.FWL });
			if (raiReason != null)
			{
				return raiReason;
			}

			var roaReason = RateCategorySpecificModeFilter(criteria.FreightMode, entry, FreightMode.ROA, new[] { Constants.RateMode.ALL, Constants.RateMode.LRO, Constants.RateMode.FTL });
			if (roaReason != null)
			{
				return roaReason;
			}

			var couReason = RateCategorySpecificModeFilter(criteria.FreightMode, entry, FreightMode.COU, new[] { Constants.RateMode.ALL, Constants.RateMode.OBC, Constants.RateMode.UNA });
			if (couReason != null)
			{
				return couReason;
			}

			var criteriaMode = RatingHelper.ConvertToRateModeWhenItIsFreight(criteria.FreightMode);
			if (entry.TI_Mode != criteriaMode)
			{
				return JobReason(RateEntrySchema.TI_Mode, criteriaMode);
			}

			var categories = RatingHelper.GetFCL_LCLExclusiveCategories(criteria.FreightMode);

			return categories.Contains(entry.TI_RateCategory)
				? string.Empty
				: JobReason(RateEntrySchema.TI_RateCategory, categories.ToArray());
		}

		string RateCategoryBCNOrSCNFilter(FreightMode freightMode, FreightMode specificModeToCheck, IRateEntry entry, string entryRateModeToCheck)
		{
			if ((freightMode & specificModeToCheck) != 0 && entry.TI_Mode == entryRateModeToCheck)
			{
				var rateCategories = new HashSet<string>();
				if ((freightMode & FreightMode.AIR) != 0)
				{
					rateCategories.Add(RatingConstants.RateCategory.AIR);
				}
				else
				{
					rateCategories.UnionWith(RatingHelper.ContainerisedCategories);
					rateCategories.UnionWith(RatingHelper.NonContainerisedCategories);
				}

				return rateCategories.Contains(entry.TI_RateCategory)
					? string.Empty
					: JobReason(RateEntrySchema.TI_Mode, specificModeToCheck.ToString());
			}

			return null;
		}

		internal string RateCategorySpecificModeFilter(FreightMode freightMode, IRateEntry entry, FreightMode specificMode, string[] applicableModes)
		{
			if (freightMode != specificMode)
			{
				return null;
			}

			return !applicableModes.Contains(entry.TI_Mode.ToString())
				? JobReason(RateEntrySchema.TI_Mode, freightMode.ToString())
				: string.Empty;
		}

		#endregion

		#region Date Filter

		string DateFilter(IRateEntry entry)
		{
			var result = entry.TI_RateStartDate <= criteria.JobDatesProvider.LatestPossibleDate
				? string.Empty
				: CustomizedReason(DiscardReporter.Reason.DateAfter, RateEntrySchema.TI_RateStartDate, criteria.JobDatesProvider.LatestPossibleDate.ToShortDateString());

			if (string.IsNullOrEmpty(result))
			{
				if (RatesToFind == FreightAutoRater.RatesToFindEnum.JustExpiredRates)
				{
					result = entry.TI_RateEndDate < ZDate.Today && entry.TI_RateEndDate >= ZDate.Today.AddDays(-AutoRater.ExpiredRateNotificationDays)
						? string.Empty
						: CustomizedReason(DiscardReporter.Reason.DateBetween, RateEntrySchema.TI_RateEndDate, ZDate.Today.AddDays(-AutoRater.ExpiredRateNotificationDays).ToShortDateString(), ZDate.Today.ToShortDateString());
				}
				else if (RatesToFind == FreightAutoRater.RatesToFindEnum.RatesGoingToExpire)
				{
					result = entry.TI_RateEndDate <= ZDateTime.Today.AddDays(AutoRater.ExpiringRateNotificationDays) && entry.TI_RateEndDate >= ZDate.Today
						? string.Empty
						: CustomizedReason(DiscardReporter.Reason.DateBetween, RateEntrySchema.TI_RateEndDate, ZDate.Today.ToShortDateString(), ZDateTime.Today.AddDays(AutoRater.ExpiringRateNotificationDays).ToShortDateString());
				}
				else
				{
					result = (entry.TI_RateEndDate >= criteria.JobDatesProvider.EarliestPossibleDate || entry.TI_RateEndDate.IsEmpty)
						? string.Empty
						: CustomizedReason(DiscardReporter.Reason.DateBefore, RateEntrySchema.TI_RateEndDate, criteria.JobDatesProvider.EarliestPossibleDate.ToShortDateString());
				}
			}

			return result;
		}

		#endregion

		#region Has Matching Locations

		string HasMatchingOrigin(IRateEntry entry)
		{
			if (entry.TI_OriginLRC.IsEmpty)
			{
				return string.Empty;
			}

			var unmatchedLocations = new List<ZString>();

			foreach (var location in GetCriteriaOrigin(true))
			{
				if (string.Equals(entry.TI_OriginLRC, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}

				unmatchedLocations.Add(location);
			}

			return JobReason(RateEntrySchema.TI_OriginLRC, unmatchedLocations.ToArray());
		}

		string HasMatchingDestination(IRateEntry entry)
		{
			if (entry.TI_DestinationLRC.IsEmpty)
			{
				return string.Empty;
			}

			var unmatchedLocations = new List<ZString>();

			foreach (var location in GetCriteriaDestinations(true))
			{
				if (string.Equals(entry.TI_DestinationLRC, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}

				unmatchedLocations.Add(location);
			}

			return JobReason(RateEntrySchema.TI_DestinationLRC, unmatchedLocations.ToArray());
		}

		string HasMatchingLocation(IRateEntry entry)
		{
			var reasons = new List<string>();
			if (entry.TI_IsCrossTrade)
			{
				reasons.Add(IsCrossTrade(criteria.Origin, criteria.Destination));
			}
			else
			{
				reasons.Add(HasMatchingOrigin(entry));
				reasons.Add(HasMatchingDestination(entry));
			}

			return string.Join(ReasonDelimitor, reasons.Where(x => !string.IsNullOrEmpty(x)));
		}

		#endregion

		#endregion

		#region Find Similar Matches

		internal HashSet<IRateEntry> FindSimilarMatches(IEnumerable<IRateEntry> unfilteredEntries, IEnumerable<IRatingHeader> rates)
		{
			var possibleMatches = new HashSet<IRateEntry>();

			if (_Rating.Sell)
			{
				var excludeSrvCommCarrierEntries = Filter(unfilteredEntries, excludeFiltersForPossibleMatches: true, excludeVia: false).ToList();

				if (excludeSrvCommCarrierEntries.Any())
				{
					possibleMatches.UnionWith(excludeSrvCommCarrierEntries);
				}
				else
				{
					var excludeViaEntries = Filter(unfilteredEntries, excludeFiltersForPossibleMatches: false, excludeVia: true).ToList();

					if (excludeViaEntries.Any())
					{
						possibleMatches.UnionWith(excludeViaEntries);
					}
					else
					{
						var looseFilter = RateEntryQueries.GetQuery(criteria, whichQueries: RateEntryQueries.Kind.TimeRange);
						looseFilter.AddToFilter(RatingHelper.GetFreightModeAndFCL_LCLExclusiveFilter(criteria.FreightMode));

						var looseEntries = new List<IRateEntry>();

						foreach (var rate in rates)
						{
							looseEntries.AddRange(rate.LoadRateEntriesForAutoRater(looseFilter));
						}

						var clientEntries = looseEntries.Where(x => string.IsNullOrEmpty(RateCategoryFilter(x))).ToList();

						var possibleLocationMatchEntries = clientEntries.Where(IsPossibleMatch);
						if (!possibleLocationMatchEntries.Any())
						{
							return possibleMatches;
						}

						var criteriaLocationCache = ratingCriteriaLocationsCache;
						var matchedRatesWithoutLocationFilter = Filter(clientEntries, excludeFiltersForPossibleMatches: false, excludeVia: false, excludeIntercompanyTariff: false, LocationsFilterMode.EverythingButLocations);
						var locationsHashset = new HashSet<string>();
						var cacheKey = string.Empty;

						foreach (var entry in possibleLocationMatchEntries)
						{
							var parameters = new RatingCriteriaLocationsCacheParameters
							{
								Origin = entry.TI_OriginLRC,
								Destination = entry.TI_DestinationLRC,
								Via = entry.TI_ViaLRC,
								PlannedLoad = entry.TI_PlannedLoadLRC,
								PlannedDischarge = entry.TI_PlannedDischargeLRC,
								RateOrigin = entry.TI_OriginLRC,
								RateDestination = entry.TI_DestinationLRC,
								FirstLoad = entry.TI_FirstLoadLRC,
								LastDischarge = entry.TI_LastDischargeLRC,
								FirstRouteSetLoad = entry.TI_FirstRouteSetLoadPortLRC,
								LastRouteSetDischarge = entry.TI_LastRouteSetDischargePortLRC,
								ZoneOwners = criteria.ZoneOwnerOrganizations()
							};

							(ratingCriteriaLocationsCache, cacheKey) = RatingCache.GetRatingCriteriaLocations(factory, parameters);
							if (locationsHashset.Contains(cacheKey))
							{
								continue;
							}

							var matchedLocationsRates = Filter(matchedRatesWithoutLocationFilter, excludeFiltersForPossibleMatches: false, excludeVia: false, excludeIntercompanyTariff: false, LocationsFilterMode.LocationsOnly);
							possibleMatches.UnionWith(matchedLocationsRates);
							locationsHashset.Add(cacheKey);
						}

						ratingCriteriaLocationsCache = criteriaLocationCache;
					}
				}
			}

			return possibleMatches;
		}

		internal bool IsPossibleMatch(IRateEntry entry)
		{
			if (string.Equals(entry.TI_OriginLRC, criteria.OriginCode, StringComparison.OrdinalIgnoreCase) && !string.Equals(entry.TI_DestinationLRC, criteria.DestinationCode, StringComparison.OrdinalIgnoreCase))
			{
				return LocationIsSimilar(criteria.DestinationCode, entry.TI_DestinationLRC);
			}

			if (!string.Equals(entry.TI_OriginLRC, criteria.OriginCode, StringComparison.OrdinalIgnoreCase) && string.Equals(entry.TI_DestinationLRC, criteria.DestinationCode, StringComparison.OrdinalIgnoreCase))
			{
				return LocationIsSimilar(criteria.OriginCode, entry.TI_OriginLRC);
			}

			return false;
		}

		internal bool LocationIsSimilar(ZString location1, ZString location2)
		{
			if (location1.Length != location2.Length || LocationHelper.GetLocationType(location2) != LocationHelper.LocationType.Port)
			{
				return false;
			}

			var countryPK1 = LocationHelper.GetLocationFromString(location1, factory)?.Country?.PK ?? ZGuid.Empty;
			var countryPK2 = LocationHelper.GetLocationFromString(location2, factory)?.Country?.PK ?? ZGuid.Empty;
			if (!countryPK1.IsEmpty && countryPK1 == countryPK2)
			{
				return true;
			}

			var differentCharactersCount = 0;
			for (var i = 0; i < location1.Length && differentCharactersCount <= 1; i++)
			{
				if (location1[i] != location2[i])
				{
					differentCharactersCount++;
				}
			}

			return differentCharactersCount <= 1;
		}

		#endregion

		#region Transhipment Multi Leg Rates

		bool GetHubbingEntries(IEnumerable<IRateEntry> unfilteredEntries, out List<IRateEntry> leg1Entries, out List<IRateEntry> leg2Entries)
		{
			var costOrSell = GetCostOrSell();
			var leg1Party = criteria.ChargesPaidBy(criteria.Origin, criteria.GetVia(costOrSell), ChargeCodeGroupList.Codes.Freight, costOrSell);
			var leg2Party = criteria.ChargesPaidBy(criteria.GetVia(costOrSell), criteria.Destination, ChargeCodeGroupList.Codes.Freight, costOrSell);

			leg1Entries = new List<IRateEntry>();
			leg2Entries = new List<IRateEntry>();

			var criteriaLocationCache = ratingCriteriaLocationsCache;

			var parameters = new RatingCriteriaLocationsCacheParameters
			{
				Origin = criteria.OriginCode,
				Destination = criteria.GetViaCode(costOrSell),
				PlannedLoad = criteria.PlannedLoadCode(costOrSell),
				PlannedDischarge = criteria.PlannedDischargeCode(costOrSell),
				RateOrigin = criteria.RateOriginCode,
				RateDestination = criteria.RateDestinationCode,
				FirstLoad = criteria.GetFirstLoadCode(costOrSell),
				LastDischarge = criteria.GetLastDischargeCode(costOrSell),
				FirstRouteSetLoad = criteria.GetFirstRouteSetLoadCode(costOrSell),
				LastRouteSetDischarge = criteria.GetLastRouteSetDischargeCode(costOrSell),
				ZoneOwners = criteria.ZoneOwnerOrganizations(),
				SortedOverridenPlannedLoads = criteria.SortedOverridenPlannedLoad?.Select(x => x.Code),
				SortedOverridenPlannedDischarges = criteria.SortedOverridenPlannedDischarge?.Select(x => x.Code)
			};

			(ratingCriteriaLocationsCache, _) = RatingCache.GetRatingCriteriaLocations(factory, parameters);
			leg1Entries.AddRange(Filter(unfilteredEntries, excludeIntercompanyTariff: true));

			parameters.Origin = criteria.GetViaCode(costOrSell);
			parameters.Destination = criteria.DestinationCode;

			(ratingCriteriaLocationsCache, _) = RatingCache.GetRatingCriteriaLocations(factory, parameters);
			leg2Entries.AddRange(Filter(unfilteredEntries, excludeIntercompanyTariff: true));

			ratingCriteriaLocationsCache = criteriaLocationCache;

			return ((!leg1Party.HasFlag(ChargedParty.LocalClient) && leg1Party != ChargedParty.Unknown || leg1Entries.Count > 0)
					&& (!leg2Party.HasFlag(ChargedParty.LocalClient) && leg2Party != ChargedParty.Unknown || leg2Entries.Count > 0));
		}

		#endregion

		#region Spot Quote

		/// <summary>
		/// For One-Off Quotations, returns the matching freight entry that the one-off quotation uses
		/// </summary>
		/// <param name="quote">The One Off Quotation</param>
		/// <returns>Matching Quote Entry</returns>
		public QuoteEntry FindMatchingRateEntryForOneOffQuote(Quote quote)
		{
			var matchedEntries = Filter(quote.SummaryRateEntries.Cast<IRateEntry>()).ToArray();
			if (matchedEntries.Length > 0 && matchedEntries[0] is QuoteEntry)
			{
				return (QuoteEntry)matchedEntries[0];
			}

			return null;
		}

		#endregion
	}
}

