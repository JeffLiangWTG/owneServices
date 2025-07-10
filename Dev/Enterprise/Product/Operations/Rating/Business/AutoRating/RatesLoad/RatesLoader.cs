using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business;

public abstract class RatesLoader(BusinessObjectFactory factory, ILogger logger)
{
	#region Spot

	protected IEnumerable<IRateEntry> LoadSpotRates(RatingCriteria criteria, bool isCosting)
	{
		var freightSpotRate = CreateFreightSpotRate(criteria, isCosting);
		var serviceSpotRates = new SpotRateEntryCreator(criteria).GetAllJobServiceRates(isCosting);
		var containerSpotRates = GetContainerSpotRateEntries(criteria, isCosting);

		var spotRates = new [] { freightSpotRate }
			.Concat(containerSpotRates)
			.Concat(serviceSpotRates)
			.WhereNotNull()
			.ToList();

		return spotRates;
	}

	IRateEntry CreateFreightSpotRate(RatingCriteria criteria, bool isCosting)
	{
		var costSell = isCosting ? CostSell.Cost : CostSell.Revenue;
		var spotRateInfo = isCosting ? criteria.CostSpotRateInfo : criteria.SellSpotRateInfo;

		// Standard Rate basically means market rate, i.e. rates stored in the database in rating tables.
		// In this case, we should ignore spot rates and always use standard rates.
		// In other modes (FreightPlusRate, AllInRate), we load both - standard rates and spot rates and then auto rating engine
		// will decide which one to use based on priorities and matching rules.
		if (spotRateInfo.AutoratedMode == Constants.FreightRateAutoratingModes.Code.StandardRate)
		{
			return null;
		}

		if (!spotRateInfo.Rate.IsValid)
		{
			return null;
		}

		if (!criteria.IsGatewaySellApplicableToGatewayConsol(costSell))
		{
			return null;
		}

		var spotRateCreator = new SpotRateEntryCreator(criteria);

		var (spotRate, error) = spotRateCreator.CreateFreightSpotRate(isCosting);
		if (!string.IsNullOrWhiteSpace(error))
		{
			_Rating.Interactor.Error(error);
			return null;
		}

		return spotRate;
	}

	IEnumerable<IRateEntry> GetContainerSpotRateEntries(RatingCriteria criteria, bool isCosting)
	{
		var result = new List<IRateEntry>();

		var measures = criteria.JobMeasures;
		if (!measures.HasContainerMeasure)
		{
			return result;
		}

		var containerSpotRates = measures.GetContainerSpotRates();
		if (isCosting && !containerSpotRates.Any(x => x.CostSpotRateIsValid) || !isCosting && !containerSpotRates.Any(x => x.SellSpotRateIsValid))
		{
			return result;
		}

		var costSell = isCosting ? CostSell.Cost : CostSell.Revenue;
		if (!criteria.IsContainerNegotiatedCostApplicable(costSell))
		{
			return result;
		}

		var (containerSpotRateEntries, reason) = new SpotRateEntryCreator(criteria).CreateContainerFreightSpotRateEntries(isCosting, containerSpotRates);

		if (!string.IsNullOrWhiteSpace(reason))
		{
			_Rating.Interactor.Error(reason);
		}

		return containerSpotRateEntries;
	}

	#endregion

	#region Common

	/// <summary>
	///		Loads rate entries from the CW1 database for the <paramref name="criteria"/> and from the <paramref name="ratingHeaders"/> provided.
	///		Please note, it applies only basic filters from the <paramref name="criteria"/>, additional in-memory filtering (<see cref="RateEntryFilter"/>)
	///		must be applied to the returned entries.
	/// </summary>
	/// <param name="excludeFiltersForPossibleMatches">
	///		When <c>true</c>, the carrier filter is not applied to the rate entries.
	///
	///		NOTE: this flag is suspicious, I don't see any system behind it, sometimes it is used, sometimes it is not.
	///		Also, the it was added under suspicious circumstances like a temporary fix or solution.
	///		WI00221020 - PL: Unable to Autorate Costs - system stops responding (CR3)
	///		It must be revised and removed if possible.
	/// </param>
	protected IEnumerable<IRateEntry> GetRateEntries(IEnumerable<IRatingHeader> ratingHeaders, RatingCriteria criteria, bool excludeFiltersForPossibleMatches, bool isCosting)
	{
		var unfilteredEntries = new List<IRateEntry>();

		var distinctRatingHeaders = ratingHeaders.Distinct().ToList();
		if (!isCosting && criteria.TariffLevel > 0)
		{
			logger?.Information((FormattableString.Invariant($"Company Tariff Level {criteria.TariffLevel} Override in {criteria.AutoRating.HumanReadableName()}")));
		}

		var requiredKind = RateEntryQueries.Kind.All;
		if (excludeFiltersForPossibleMatches)
		{
			requiredKind ^=
				RateEntryQueries.Kind.Carrier |
				RateEntryQueries.Kind.ClientServiceLevel |
				RateEntryQueries.Kind.CarrierServiceLevel;
		}

		var rateEntryQuery = RateEntryQueries.GetQuery(criteria, isCosting: isCosting, isInterCompanyTariff: false, whichQueries: requiredKind);
		var rateEntryQueryForIntercompanyTariff = RateEntryQueries.GetQuery(criteria, isCosting: isCosting, isInterCompanyTariff: true, whichQueries: requiredKind);

		foreach (var header in distinctRatingHeaders)
		{
			var entriesFilter = header.IsIntercompanyTariff()
				? rateEntryQueryForIntercompanyTariff
				: rateEntryQuery;

			var entries = header.LoadRateEntriesForAutoRater(entriesFilter).ToArray();

			logger?.Information(FormattableString.Invariant($"{LogEventTypes.RatingHeaderFound} {header.DisplayInfo()} Entries: {entries.Length}")); // log message, subject to change, more for support people as of now
			unfilteredEntries.AddRange(entries);
		}

		AddServiceRateEntriesAtOtherLocations(distinctRatingHeaders, unfilteredEntries, criteria, isCosting);

		return unfilteredEntries;
	}

	/// <summary>
	/// Services can have locations that are different from any of the other locations on a job.
	/// The rates matching those locations are best to be loaded separately.
	/// For example, a job has destination AUSYD, and a destination service at AUNTL.
	/// </summary>
	void AddServiceRateEntriesAtOtherLocations(
		IEnumerable<IRatingHeader> distinctRatingHeaders,
		List<IRateEntry> entries,
		RatingCriteria criteria,
		bool isCosting)
	{
		if (!distinctRatingHeaders.Any())
		{
			return;
		}

		var applicableServices = criteria.JobServices
			.Where(service => service.IsEnabled &&
				LocationHelper.GetLocationType(service.LocationCode) == LocationHelper.LocationType.Port &&
				service.IsForRateSearch(isCosting));

		if (!applicableServices.Any())
		{
			return;
		}

		var entrySet = new HashSet<IRateEntry>(entries);
		if (isCosting)
		{
			var providerPks = GetProviderPks(applicableServices);
			foreach (var providerPk in providerPks)
			{
				var others = GetRateEntriesAtOtherLocations(providerPk, distinctRatingHeaders, applicableServices, criteria);
				AddIfNotPresent(entries, others, entrySet);
			}
		}
		else
		{
			var others = GetRateEntriesAtOtherLocations(ZGuid.Empty, distinctRatingHeaders, applicableServices, criteria);
			AddIfNotPresent(entries, others, entrySet);
		}
	}

	static void AddIfNotPresent(List<IRateEntry> entries, IEnumerable<IRateEntry> entriesToAdd, HashSet<IRateEntry> existing)
	{
		foreach (var entry in entriesToAdd)
		{
			if (!existing.Contains(entry))
			{
				entries.Add(entry);
				existing.Add(entry);
			}
		}
	}

	static HashSet<ZGuid> GetProviderPks(IEnumerable<JobServiceInfo> services)
	{
		var providerPks = new HashSet<ZGuid>();
		foreach (var service in services)
		{
			if (service.Contractor != null)
			{
				providerPks.Add(service.Contractor.PK);
			}
			if (service.FallbackContractors != null)
			{
				providerPks.UnionWith(service.FallbackContractors.Select(x => x.PK));
			}
		}
		return providerPks;
	}

	IEnumerable<IRateEntry> GetRateEntriesAtOtherLocations(
		ZGuid costProviderPk,
		IEnumerable<IRatingHeader> distinctRatingHeaders,
		IEnumerable<JobServiceInfo> services,
		RatingCriteria criteria)
	{
		var result = new List<IRateEntry>();
		foreach (var serviceChargeCodeGroup in services
			.Where(x => (x.ChargeCodeGroup == ChargeCodeGroupList.Codes.Destination || x.ChargeCodeGroup == ChargeCodeGroupList.Codes.Origin)
					&& (costProviderPk.IsEmpty || IsMatchingContractor(x, costProviderPk)))
			.GroupBy(x => x.ChargeCodeGroup))
		{
			bool isDestination = serviceChargeCodeGroup.Key == ChargeCodeGroupList.Codes.Destination;
			GetChargeGroupEntriesAtOtherLocations(costProviderPk, distinctRatingHeaders, criteria, result, serviceChargeCodeGroup, isDestination);
		}
		return result;
	}

	static bool IsMatchingContractor(JobServiceInfo service, ZGuid providerPk)
		=> service.Contractor?.PK == providerPk || (service.FallbackContractors != null && service.FallbackContractors.Any(y => y.PK == providerPk));

	public static void ReportRatesLoadedStats(RatesLoadedStats ratesLoadedStats, long elapsedTime)
	{
		RatingUsageCollector.ReportRatesLoaded(ratesLoadedStats, elapsedTime);
	}

	void GetChargeGroupEntriesAtOtherLocations(
		ZGuid costProviderPk,
		IEnumerable<IRatingHeader> distinctRatingHeaders,
		RatingCriteria criteria,
		List<IRateEntry> result,
		IGrouping<ZString, JobServiceInfo> serviceChargeCodeGroup,
		bool isDestination)
	{
		var jobLocation = isDestination ? criteria.Destination : criteria.Origin;

		if (jobLocation != null)
		{
			SchemaColumn locationColumn = null;
			string[] rateCategories = null;
			ZQuery oppositeLocationFilter = null;

			if (isDestination)
			{
				locationColumn = RateEntrySchema.TI_DestinationLRC;
				rateCategories = criteria.GetRateCategories(RateCategoryGroup.Destination);
				oppositeLocationFilter = RateEntryQueries.CreateLocationQueryHelper(criteria, criteria.Origin, RateEntrySchema.TI_OriginLRC);
			}
			else
			{
				locationColumn = RateEntrySchema.TI_OriginLRC;
				rateCategories = criteria.GetRateCategories(RateCategoryGroup.Origin);
				oppositeLocationFilter = RateEntryQueries.CreateLocationQueryHelper(criteria, criteria.Destination, RateEntrySchema.TI_DestinationLRC);
			}

			var factory = distinctRatingHeaders.First().Factory;
			var rateLocationSet = new HashSet<ZString>();
			var locationGroups = serviceChargeCodeGroup.GroupBy(x => x.LocationCode);
			var zoneOwnerOrganizations = criteria.ZoneOwnerOrganizations();
			foreach (var locationGroup in locationGroups)
			{
				var serviceLocation = LocationHelper.GetCachedLocationFromString(locationGroup.Key, factory);
				if (serviceLocation != null)
				{
					rateLocationSet.UnionWith(RatingZoneRetriever.GetApplicableLocationCodes(serviceLocation, zoneOwnerOrganizations));
				}
			}
			// job location is excluded since those rates are already loaded
			rateLocationSet.ExceptWith(RatingZoneRetriever.GetApplicableLocationCodes(jobLocation, zoneOwnerOrganizations));

			if (rateLocationSet.Count > 0)
			{
				var timeRangeFilter = RateEntryQueries.GetQuery(criteria, whichQueries: RateEntryQueries.Kind.TimeRange);
				var locationFilter = new ZQuery(locationColumn, rateLocationSet);
				var entriesFilter = new ZQuery(RateEntrySchema.TI_RateCategory, rateCategories);
				entriesFilter.AddToFilter(timeRangeFilter);
				entriesFilter.AddToFilter(locationFilter);
				entriesFilter.AddToFilter(oppositeLocationFilter);
				foreach (var header in distinctRatingHeaders.Where(x => costProviderPk.IsEmpty || x.TH_OH == costProviderPk))
				{
					var initialCount = result.Count;
					result.AddRange(header.LoadRateEntriesForAutoRater(entriesFilter));
					var addedCount = result.Count - initialCount;
					if (addedCount > 0)
					{
						var serviceLocationCodes = string.Join(", ", locationGroups.Select(x => x.Key));
						logger?.Information(FormattableString.Invariant($"{LogEventTypes.RatingHeaderFound} {header.DisplayInfo()} for {serviceChargeCodeGroup.Key} service location {serviceLocationCodes} Entries: {addedCount}")); // log message, subject to change, more for support people as of now
					}
				}
			}
		}
	}

	#endregion

	protected readonly BusinessObjectFactory factory = factory;
	protected readonly ILogger logger = logger;
}
