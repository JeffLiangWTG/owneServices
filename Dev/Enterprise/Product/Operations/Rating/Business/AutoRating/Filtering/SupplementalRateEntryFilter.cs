using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;
using LocationKey = Enterprise.Rating.Business.RatingCriteriaLocationsCache.Keys;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Removes non-freight rates that don't match the criteria.
	/// Will be called for rates that return false from BaseFallbackRateMatcher.IsForFreightMatcher
	/// That is, RateCategory values that do not have the attribute IsFreight.
	/// For example, origin and destination rates.
	/// </summary>
	public class SupplementalRateEntryFilter : RateEntryFilter
	{
		public SupplementalRateEntryFilter(RatingCriteria criteria, bool isCosting, BusinessObjectFactory factory, ILogger logger)
			: base(criteria, isCosting, factory, logger)
		{
		}

		protected override void FilterAdditional()
		{
			rateEntriesRepository.FilterEntries(ModeFilter);
			rateEntriesRepository.FilterEntries(DateFilter);
			rateEntriesRepository.FilterEntries(AircraftTypeFilter);
			rateEntriesRepository.FilterEntries(FMCTariffIDFilter);
			rateEntriesRepository.FilterEntries(IsNonOperatingReeferFilter);
		}

		protected override void FilterLocations()
		{
			rateEntriesRepository.FilterEntries(CategoryLocationFilter);

			base.FilterLocations();
		}

		string CategoryLocationFilter(IRateEntry entry)
		{
			var categoryGroups = new[] { RateCategoryGroup.Origin, RateCategoryGroup.Destination, RateCategoryGroup.OtherSupplementary };
			var results = new List<string>();

			foreach (var categoryGroup in categoryGroups)
			{
				var result = categoryGroup == RateCategoryGroup.OtherSupplementary
					? RateCategoryFilter(categoryGroup, entry, criteria)
					: HasMatchingLocation(categoryGroup, entry);

				if (string.IsNullOrEmpty(result))
				{
					return string.Empty;
				}

				results.Add(result);
			}

			return string.Join(ReasonDelimitor, results);
		}

		protected override RefContainerCollection GetContainersInSameClass(ZGuid containerPK)
		{
			var refContainer = factory.Load<RefContainer>(containerPK);
			return refContainer != null ? refContainer.ContainersInSameHandlingRateClass : null;
		}

		string ModeFilter(IRateEntry entry)
		{
			var originDestinationModeExclusive = RatingHelper.GetPossibleModes(criteria.FreightMode);
			var acceptableRateModes = new []
			{
				Core.Constants.RateMode.COU,
				Core.Constants.RateMode.OBC,
				Core.Constants.RateMode.UNA,
				Core.Constants.RateMode.ALL
			};

			var filter = new FreightRateEntryFilter(criteria, true, factory, logger);
			var couReason = filter.RateCategorySpecificModeFilter(criteria.FreightMode, entry, FreightMode.COU, acceptableRateModes);
			if (couReason != null)
			{
				return couReason;
			}

			return originDestinationModeExclusive.Contains(entry.TI_Mode)
				? string.Empty
				: JobReason(RateEntrySchema.TI_Mode, criteria.FreightMode.ToString());
		}

		string HasMatchingOrigin(IRateEntry entry, bool isOriginRequired)
		{
			var areLocationsTheSame = entry.TI_OriginLRC == criteria.OriginCode
				|| entry.TI_OriginLRC == criteria.RateOriginCode
				|| (entry.TI_OriginLRC.IsEmpty && entry.TI_RateOrigin == criteria.RateOriginCode);

			var result = isOriginRequired
				? areLocationsTheSame
				: entry.TI_OriginLRC.IsEmpty || areLocationsTheSame;

			if (result)
			{
				return string.Empty;
			}

			if (isOriginRequired)
			{
				if (criteria.OriginServiceLocations.Contains(entry.TI_OriginLRC))
				{
					return string.Empty;
				}
			}

			return FilterOrigin(entry, isOriginRequired);
		}

		string FilterOrigin(IRateEntry entry, bool isOriginRequired)
		{
			var origin = entry.TI_OriginLRC;
			if (origin.IsEmpty &&
				!ratingCriteriaLocationsCache.GetLocation(LocationKey.Origin).Zones.Any() &&
				!ratingCriteriaLocationsCache.GetLocation(LocationKey.RateOrigin).Zones.Any())
			{
				return string.Empty;
			}

			var unmatchedLocations = new List<ZString>();

			foreach (var location in GetCriteriaOrigin())
			{
				if (string.Equals(origin, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}
				else if (origin.IsEmpty && string.Equals(entry.TI_RateOrigin, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}

				unmatchedLocations.Add(location);
			}

			if (isOriginRequired && !origin.IsEmpty)
			{
				var serviceLocations = ratingCriteriaLocationsCache.GetLocations(LocationKey.OriginServices);
				foreach (var location in GetLocationCodes(serviceLocations))
				{
					if (origin == location)
					{
						return string.Empty;
					}

					unmatchedLocations.Add(location);
				}
			}

			return JobReason(RateEntrySchema.TI_OriginLRC, unmatchedLocations.Distinct().ToArray());
		}

		string HasMatchingDestination(IRateEntry entry, bool isDestinationOptional)
		{
			var areLocationsTheSame = entry.TI_DestinationLRC == criteria.DestinationCode
				|| entry.TI_DestinationLRC == criteria.RateDestinationCode
				|| (entry.TI_DestinationLRC.IsEmpty && entry.TI_RateDestination == criteria.RateDestinationCode);

			var result = isDestinationOptional
				? entry.TI_DestinationLRC.IsEmpty || areLocationsTheSame
				: areLocationsTheSame;

			if (result)
			{
				return string.Empty;
			}

			if (!isDestinationOptional)
			{
				if (criteria.DestinationServiceLocations.Contains(entry.TI_DestinationLRC))
				{
					return string.Empty;
				}
			}

			return DestinationFilter(entry, isDestinationOptional);
		}

		string DestinationFilter(IRateEntry entry, bool isDestinationOptional)
		{
			var destination = entry.TI_DestinationLRC;
			if (destination.IsEmpty &&
				!ratingCriteriaLocationsCache.GetLocation(LocationKey.Destination).Zones.Any() &&
				!ratingCriteriaLocationsCache.GetLocation(LocationKey.RateDestination).Zones.Any())
			{
				return string.Empty;
			}

			var unmatchedLocations = new List<ZString>();

			foreach (var location in GetCriteriaDestinations())
			{
				if (string.Equals(destination, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}
				else if (destination.IsEmpty && string.Equals(entry.TI_RateDestination, location, StringComparison.OrdinalIgnoreCase))
				{
					return string.Empty;
				}

				unmatchedLocations.Add(location);
			}

			if (!isDestinationOptional && !destination.IsEmpty)
			{
				var destinationServiceLocations = ratingCriteriaLocationsCache.GetLocations(LocationKey.DestinationServices);
				foreach (var location in GetLocationCodes(destinationServiceLocations))
				{
					if (destination == location)
					{
						return string.Empty;
					}

					unmatchedLocations.Add(location);
				}
			}

			return JobReason(RateEntrySchema.TI_DestinationLRC, unmatchedLocations.Distinct().ToArray());
		}

		string HasMatchingLocation(RateCategoryGroup rateCategoryGroup, IRateEntry entry)
		{
			var isOriginCategory = rateCategoryGroup == RateCategoryGroup.Origin;
			var originReason = HasMatchingOrigin(entry, isOriginCategory);
			var destinationReason = HasMatchingDestination(entry, isOriginCategory);
			var result = new List<string> { originReason, destinationReason };

			if (!(string.IsNullOrEmpty(originReason) && string.IsNullOrEmpty(destinationReason)) && entry.TI_IsCrossTrade)
			{
				var reasonCrossTrade = IsCrossTrade(criteria.Origin, criteria.Destination);

				if (!string.IsNullOrEmpty(reasonCrossTrade))
				{
					result.Add(reasonCrossTrade);
					return string.Join(ReasonDelimitor, result.Where(x => !string.IsNullOrEmpty(x)));
				}

				result = new List<string>();
			}

			result.Add(RateCategoryFilter(rateCategoryGroup, entry, criteria));
			return string.Join(ReasonDelimitor, result.Where(x => !string.IsNullOrEmpty(x)));
		}

		string DateFilter(IRateEntry entry)
		{
			var result = entry.TI_RateStartDate <= criteria.JobDatesProvider.LatestPossibleDate
				? string.Empty
				: CustomizedReason(DiscardReporter.Reason.DateAfter, RateEntrySchema.TI_RateStartDate, criteria.JobDatesProvider.LatestPossibleDate.ToShortDateString());

			if (string.IsNullOrEmpty(result))
			{
				if (RatesToFind == FreightAutoRater.RatesToFindEnum.ActiveRates)
				{
					result = (entry.TI_RateEndDate.IsEmpty || entry.TI_RateEndDate >= criteria.JobDatesProvider.EarliestPossibleDate)
						? string.Empty
						: CustomizedReason(DiscardReporter.Reason.DateBefore, RateEntrySchema.TI_RateEndDate, criteria.JobDatesProvider.EarliestPossibleDate.ToShortDateString());
				}
			}

			return result;
		}
	}
}

