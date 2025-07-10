using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// All functions (not marked as Helpers) in the
	/// <see cref="RateEntryQueries"/> must take in exactly one instance of
	/// this parameter.
	///
	/// It's a struct to make sure that each function gets a clean copy of the
	/// parameters.
	/// </summary>
	struct RateEntryQueryParameters
	{
		// Not all queries care about IsCosting. But for those that do care,
		// then we want to throw an exception if it was not set. Hence the
		// nullable backing field
		public bool IsCosting { get => isCosting.Value; set => isCosting = value; }
		bool? isCosting;

		// Not all queries care about IsCosting. But for those that do care,
		// then we want to throw an exception if it was not set. Hence the
		// nullable backing field
		public bool IsInterCompanyTariff { get => isInterCompanyTariff.Value; set => isInterCompanyTariff = value; }
		bool? isInterCompanyTariff;

		public RatingCriteria Criteria { get; set; }
	}

	/// <summary>
	/// A collection of queries that can be made to help narrow-down the results
	/// of a database search for RateEntries.
	///
	/// Some of these queries will produce a ZQuery that filters the results
	/// down to the exact final match for the columns it's checking, others will
	/// merely reduce the number of results but will still need further refining
	/// later in the <see cref="BaseFallbackRateMatcher"/> or children.
	/// </summary>
	static class RateEntryQueries
	{
		/// <summary>
		/// Used to decide which queries you want applied to your database
		/// search./>
		/// </summary>
		[Flags]
		public enum Kind
		{
			None = 0,
			Location = 1 << 0,
			Carrier = 1 << 1,
			TimeRange = 1 << 2,
			RateCategoryCoarse = 1 << 3,
			ContractNumber = 1 << 4,
			/// <summary>
			/// BBK, BLK, ROR, BCN
			/// </summary>
			RateModesSpecialFreightModes = 1 << 5,

			CarrierServiceLevel = 1 << 6,
			ClientServiceLevel = 1 << 7,

			// Using F's to avoid manually specifying all the flags
			All = 0x7FFFFFFF
		}

		/// <summary>
		/// To request a query for filtering RateEntries, pass in your desired
		/// flags into <paramref name="whichQueries"/> and you'll get your result.
		/// </summary>
		/// <param name="isCosting">
		/// If your desired filters needs IsCosting, then this is mandatory
		/// </param>
		/// <param name="isInterCompanyTariff">
		/// If your desired filter needs IsInterCompanyTariff, then this is mandatory
		/// </param>
		internal static ZQuery GetQuery(
			RatingCriteria criteria,
			bool? isCosting = null,
			bool? isInterCompanyTariff = null,
			Kind whichQueries = Kind.All)
		{
			var parameters = new RateEntryQueryParameters()
			{
				Criteria = criteria,
			};

			if (isCosting.HasValue)
			{
				parameters.IsCosting = isCosting.Value;
			}

			if (isInterCompanyTariff.HasValue)
			{
				parameters.IsInterCompanyTariff = isInterCompanyTariff.Value;
			}

			return GetQuery(parameters, whichQueries);
		}

		static ZQuery GetQuery(RateEntryQueryParameters parameter, Kind whichQueries = Kind.All)
		{
			var query = new ZQuery()
				.AndQuery(CreateLocationQuery, Kind.Location, whichQueries, parameter)
				.AndQuery(CreateCarrierQuery, Kind.Carrier, whichQueries, parameter)
				.AndQuery(CreateTimeRangeQuery, Kind.TimeRange, whichQueries, parameter)
				.AndQuery(CreateRateCategoryQuery, Kind.RateCategoryCoarse, whichQueries, parameter)
				.AndQuery(CreateContractNumberQuery, Kind.ContractNumber, whichQueries, parameter)
				.AndQuery(CreateRateModeQuery_SpecialFreightModes, Kind.RateModesSpecialFreightModes, whichQueries, parameter)
				.AndQuery(CreateCarrierServiceLevelQuery, Kind.CarrierServiceLevel, whichQueries, parameter)
				.AndQuery(CreateClientServiceLevelQuery, Kind.ClientServiceLevel, whichQueries, parameter)
				;

			return query;
		}

		static ZQuery AndQuery(this ZQuery query, Func<RateEntryQueryParameters, ZQuery> function, Kind forTheFunction, Kind requestedByCaller, RateEntryQueryParameters functionParameter)
		{
			if (requestedByCaller.HasFlag(forTheFunction))
			{
				return query.And(function(functionParameter));
			}
			return query;
		}

		internal static ZQuery CreateClientServiceLevelQuery(RateEntryQueryParameters parameter)
		{
			var jobServiceLevel = parameter.Criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Client);
			var query =
				new ZQuery(RateEntrySchema.TI_RS_NKServiceLevel_NI, string.Empty)
				.Or(new ZQuery(RateEntrySchema.TI_RS_NKServiceLevel_NI, jobServiceLevel));

			return query;
		}

		internal static ZQuery CreateCarrierServiceLevelQuery(RateEntryQueryParameters parameter)
		{
			var jobCarrierServiceLevel =
				parameter.Criteria.CarrierServiceLevelOverride
				?? new List<ZString> { parameter.Criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier) };

			if (jobCarrierServiceLevel.Count == 0)
			{
				return new ZQuery();
			}

			var query =
				new ZQuery(RateEntrySchema.TI_PL_NKCarrierServiceLevel, string.Empty)
				.Or(new ZQuery(RateEntrySchema.TI_PL_NKCarrierServiceLevel, jobCarrierServiceLevel));

			return query;
		}

		static ZQuery CreateCarrierQuery(RateEntryQueryParameters parameter)
		{
			if (!parameter.Criteria.IsManualCostSelectMode)
			{
				var query = new ZQuery(RateEntrySchema.TI_OH_TransportProvider, null);
				var orgPKs = parameter.Criteria.AllDistinctCarriersAndConsortiumOrgProxies;
				if (orgPKs.Count > 0)
				{
					query.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_OH_TransportProvider, orgPKs);
				}
				return query;
			}
			return new ZQuery();
		}

		/// <summary>
		/// Returns a query where the start date and end date of a RateEntry as
		/// as follows
		///
		/// Time                  |--------------------------------------|
		/// EarliestPossibleDate  |  E                                   |
		/// LatestPossibleDate    |                                   L  |
		/// TI_RateStartDate      |===================================S  |
		/// TI_RateEndDate        |  E===================================|
		/// Desired dates (1)     |  <================================>  |
		/// Desired dates (2)     |  <===================================| (no rate end date)
		/// </summary>
		static ZQuery CreateTimeRangeQuery(RateEntryQueryParameters parameter)
		{
			var startDateFilter = new ZQuery();
			startDateFilter.AddToFilter(
				JoinCondition.Or,
				RateEntrySchema.TI_RateStartDate,
				SQLComparisonOperator.LessThanOrEqualToDatePartOnly,
				parameter.Criteria.LatestPossibleDate);

			var endDateFilter = new ZQuery(RateEntrySchema.TI_RateEndDate, null);
			endDateFilter.AddToFilter(
				JoinCondition.Or,
				RateEntrySchema.TI_RateEndDate,
				SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly,
				parameter.Criteria.EarliestPossibleDate);

			return new ZQuery(startDateFilter, JoinCondition.And, endDateFilter);
		}

		static ZQuery CreateRateCategoryQuery(RateEntryQueryParameters parameter)
		{
			var soughtCategories = parameter.Criteria.GetRateCategories(RateCategoryGroup.All);
			return new ZQuery(RateEntrySchema.TI_RateCategory, soughtCategories);
		}

		/// <summary>
		/// This is the main location query that considers all sorts of
		/// </summary>
		static ZQuery CreateLocationQuery(RateEntryQueryParameters parameter)
		{
			var zoneOwnerOrganizations = parameter.Criteria.ZoneOwnerOrganizations();
			var originSet = RatingZoneRetriever.GetApplicableLocationCodes(parameter.Criteria.Origin, zoneOwnerOrganizations);
			var destinationSet = RatingZoneRetriever.GetApplicableLocationCodes(parameter.Criteria.Destination, zoneOwnerOrganizations);
			var via = parameter.Criteria.GetVia(parameter.IsCosting ? CostSell.Cost : CostSell.Revenue);
			ZQuery viaFilter = null;
			if (via != null)
			{
				var viaSet = RatingZoneRetriever.GetApplicableLocationCodes(via, zoneOwnerOrganizations);
				// Via locations can match the entry destination for leg 1 and the entry origin for leg 2
				// Leg 1 = Origin -> Via
				// Leg 2 = Via -> Destination
				originSet.UnionWith(viaSet);
				destinationSet.UnionWith(viaSet);
				viaFilter = CreateLocationQueryHelper(parameter.Criteria, viaSet, RateEntrySchema.TI_ViaLRC);
			}
			var originFilter = CreateLocationQueryHelper(parameter.Criteria, originSet, RateEntrySchema.TI_OriginLRC);
			var destinationFilter = CreateLocationQueryHelper(parameter.Criteria, destinationSet, RateEntrySchema.TI_DestinationLRC);
			var locationFilter = originFilter.And(destinationFilter);
			if (viaFilter != null)
			{
				locationFilter.AddToFilter(viaFilter);
			}

			if (!parameter.IsCosting || parameter.IsInterCompanyTariff)
			{
				var rateOriginFilter = CreateLocationQueryHelper(parameter.Criteria, parameter.Criteria.RateOrigin, RateEntrySchema.TI_RateOrigin);
				var rateDestinationFilter = CreateLocationQueryHelper(parameter.Criteria, parameter.Criteria.RateDestination, RateEntrySchema.TI_RateDestination);
				var rateOriginDestinationFilter = rateOriginFilter.And(rateDestinationFilter);

				// should keep the fallback to Origin field
				var rateOriginFallBackFilter = CreateLocationQueryHelper(parameter.Criteria, parameter.Criteria.RateOrigin, RateEntrySchema.TI_OriginLRC);
				var rateDestinationFallbackFilter = CreateLocationQueryHelper(parameter.Criteria, parameter.Criteria.RateDestination, RateEntrySchema.TI_DestinationLRC);
				var rateOriginDestinationFallbackFilter = rateOriginFallBackFilter.And(rateDestinationFallbackFilter);

				locationFilter = locationFilter
					.Or(rateOriginDestinationFilter)
					.Or(rateOriginDestinationFallbackFilter);
			}

			var crossTradeCondition = parameter.Criteria.IsCrossTrade() ? JoinCondition.Or : JoinCondition.And;
			locationFilter.AddToFilter(crossTradeCondition, RateEntrySchema.TI_IsCrossTrade, parameter.Criteria.IsCrossTrade());

			var plannedLoadPlannedDischargeFilter = CreatePlannedLoadPlannedDischargeQuery(parameter);
			locationFilter = locationFilter.And(plannedLoadPlannedDischargeFilter);

			return locationFilter;
		}

		internal static ZQuery CreateLocationQueryHelper(RatingCriteria criteria, HashSet<ZString> locationSet, SchemaColumn locationColumn)
		{
			var result = new ZQuery();
			if (locationSet.Any())
			{
				result.AddToFilter(locationColumn, locationSet);
			}

			return result;
		}

		internal static ZQuery CreateLocationQueryHelper(RatingCriteria criteria, ILocation location, SchemaColumn locationColumn)
			=> CreateLocationQueryHelper(criteria, new[] { location }, locationColumn);

		internal static ZQuery CreateLocationQueryHelper(RatingCriteria criteria, IEnumerable<ILocation> locations, SchemaColumn locationColumn)
		{
			var locationSet = new HashSet<ZString>();

			foreach (var location in locations.WhereNotNull())
			{
				locationSet.UnionWith(RatingZoneRetriever.GetApplicableLocationCodes(location, criteria.ZoneOwnerOrganizations()));
			}

			return CreateLocationQueryHelper(criteria, locationSet, locationColumn);
		}

		static ZQuery CreatePlannedLoadPlannedDischargeQuery(RateEntryQueryParameters parameter)
		{
			var costSell = parameter.IsCosting ? CostSell.Cost : CostSell.Revenue;

			var plannedLoad = CreateLocationQueryHelper(parameter.Criteria, parameter.Criteria.PlannedLoad(costSell), RateEntrySchema.TI_PlannedLoadLRC);
			var plannedDischarge = CreateLocationQueryHelper(parameter.Criteria, parameter.Criteria.PlannedDischarge(costSell), RateEntrySchema.TI_PlannedDischargeLRC);

			var overridenPlannedLoad = CreateLocationQueryHelper(parameter.Criteria, parameter.Criteria.SortedOverridenPlannedLoad, RateEntrySchema.TI_PlannedLoadLRC);
			plannedLoad = plannedLoad.Or(overridenPlannedLoad);

			var overridenPlannedDischarge = CreateLocationQueryHelper(parameter.Criteria, parameter.Criteria.SortedOverridenPlannedDischarge, RateEntrySchema.TI_PlannedDischargeLRC);
			plannedDischarge = plannedDischarge.Or(overridenPlannedDischarge);

			var result = plannedLoad.And(plannedDischarge);
			return result;
		}

		static ZQuery CreateRateModeQuery_SpecialFreightModes(RateEntryQueryParameters parameter)
		{
			if (parameter.Criteria.FreightMode.In(FreightMode.BBK, FreightMode.BLK, FreightMode.ROR))
			{
				if (RatingDataRegistry.Instance.AutorateByBBK_BLK_ROR_BCNContainerModes.Value)
				{
					return new ZQuery(RateEntrySchema.TI_Mode, parameter.Criteria.FreightMode);
				}
				else
				{
					return new ZQuery(RateEntrySchema.TI_Mode, SQLComparisonOperator.NotEqual, new[] { FreightMode.BBK, FreightMode.BLK, FreightMode.ROR });
				}
			}
			return new ZQuery();
		}

		static ZQuery CreateContractNumberQuery(RateEntryQueryParameters parameter)
		{
			var contractNumbers = parameter.IsCosting ? parameter.Criteria.CarrierContractNumbers : parameter.Criteria.ClientContractNumbers;
			var distinctContractNumbers = new HashSet<string>(
				contractNumbers.Select(x => x.ToString()),
				StringComparer.OrdinalIgnoreCase);

			if (distinctContractNumbers.Count == 0)
			{
				return new ZQuery();
			}

			// Job's CarrierContractNumbers is overridden when using Rate Selector (with cost). Just form contract filter in query with the numbers.
			// Otherwise, get configuration from adapter to decide.
			if (!parameter.Criteria.IsManualCostSelectMode)
			{
				var configuration = parameter.Criteria.GetContractNumberConfiguration(parameter.IsCosting ? CostSell.Cost : CostSell.Revenue);
				if (configuration != null && !configuration.ShouldAddContractNumberQueryFilter)
				{
					return new ZQuery();
				}
			}

			// always load rates with blank contract number - more generic ones.
			distinctContractNumbers.Add(ZString.Empty);
			return new ZQuery(RateEntrySchema.TI_ContractNumber, distinctContractNumbers);
		}
	}
}
