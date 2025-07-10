#if NETFRAMEWORK
using CargoWise.Common;
#endif
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Rating.Business.AutoRating
{
	internal static class RateLocationFilter
	{
		public static void Apply(IList<IRateEntry> rateEntries, RatingCriteria criteria)
		{
			FilterRateEntriesByOriginAndDestinationZones(rateEntries, criteria);

			for (var i = rateEntries.Count - 1; i >= 0; i--)
			{
				if (!MeetsFilter(criteria, rateEntries[i]))
				{
					rateEntries.Remove(rateEntries[i]);
				}
			}
		}

		static bool MeetsFilter(RatingCriteria criteria, IRateEntry entry)
		{
			var result = AllZonesAreActive(entry);
			// Since the below checking are only specific to TBC entry: OriginZone, DestinationZone, OriginSuburb, and DestinationSuburb,
			// we just need information from PickupAddress and DeliveryAddress which are not allowed to override.
			// There won't be fallback logic for those addresses
			result = result && CriteriaCoveredBySuburbs(entry, criteria);

			return result;
		}

		// The reason this takes all entries rather then using the standard rateEntriesRepository.FilterEntries(filter function)
		// is due to an optimization for the address within zone filtering. The standard approach of looping through each entry,
		// and checking if the relevant criteria address is within the relevant zone for the entry, can lead to alot of duplicate
		// computation as many entries may use the same origin/destination zones. To avoid this, we instead look at all unique
		// zones (see .DistinctBy) from the full list of entries and check if the relevant criteria address is within each of
		// those.
		internal static void FilterRateEntriesByOriginAndDestinationZones(IList<IRateEntry> rateEntries, RatingCriteria criteria)
		{
#if NETFRAMEWORK
			var correctOriginZone =
				rateEntries
					.DistinctBy(entry => entry.OriginZone)
					.Where(entry => AddressIsWithinZone(criteria.PickupAddress, entry.OriginZone))
					.Select(entry => entry.OriginZone)
					.ToHashSet();
#elif NET
			var correctOriginZone =
				Enumerable.DistinctBy(rateEntries, entry => entry.OriginZone)
					.Where(entry => AddressIsWithinZone(criteria.PickupAddress, entry.OriginZone))
					.Select(entry => entry.OriginZone)
					.ToHashSet();
#endif

			if (correctOriginZone.Count > 0)
			{
				rateEntries.RemoveAll(entry => !correctOriginZone.Contains(entry.OriginZone));
			}
			else
			{
				rateEntries.RemoveAll(entry => entry.OriginZone != null);
			}

#if NETFRAMEWORK
			var correctDestinationZone =
				rateEntries
					.DistinctBy(entry => entry.DestinationZone)
					.Where(entry => AddressIsWithinZone(criteria.DeliveryAddress, entry.DestinationZone))
					.Select(entry => entry.DestinationZone)
					.ToHashSet();
#elif NET
			var correctDestinationZone =
				Enumerable.DistinctBy(rateEntries, entry => entry.DestinationZone)
					.Where(entry => AddressIsWithinZone(criteria.DeliveryAddress, entry.DestinationZone))
					.Select(entry => entry.DestinationZone)
					.ToHashSet();
#endif

			if (correctDestinationZone.Count > 0)
			{
				rateEntries.RemoveAll(entry => !correctDestinationZone.Contains(entry.DestinationZone));
			}
			else
			{
				rateEntries.RemoveAll(entry => entry.DestinationZone != null);
			}
		}

		static bool AllZonesAreActive(IRateEntry entry)
		{
			if (IsInternationalZoneInactive(entry.Origin()) || IsInternationalZoneInactive(entry.Destination()))
			{
				return false;
			}

			if (RatingConstants.RateCategory.SupportsTransportZones(entry.TI_RateCategory))
			{
				if (IsTransportZoneInactive(entry.OriginZone) || IsTransportZoneInactive(entry.DestinationZone))
				{
					return false;
				}
			}

			return true;
		}

		static bool IsInternationalZoneInactive(ILocation zone)
		{
			var internationalZone = zone as RefZoneHeader;
			return internationalZone != null && !internationalZone.FZ_IsActive;
		}

		static bool IsTransportZoneInactive(RateTransportZone transportZone)
		{
			return transportZone != null && !transportZone.TZ_IsActive;
		}

		static bool CriteriaCoveredBySuburbs(IRateEntry entry, RatingCriteria criteria)
		{
			var result = AddressIsWithinSuburb(criteria.PickupAddress, entry.OriginSuburbPK, criteria.Factory);
			result = result && AddressIsWithinSuburb(criteria.DeliveryAddress, entry.DestinationSuburbPK, criteria.Factory);

			return result;
		}

		static bool AddressIsWithinZone(IDocAddress address, RateTransportZone zone)
		{
			if (zone != null && zone.TZ_IsActive && address != null)
			{
				var result = zone.Items.Any(item => RateTransportZoneHelper.IsCityTownMatching(item, address.E2_City, false, address.E2_Postcode)
					|| RateTransportZoneHelper.IsPostCodeWithinRange(item, address.E2_Postcode, false));

				return result;
			}

			return true;
		}

		static bool AddressIsWithinSuburb(IDocAddress address, ZGuid suburbPK, BusinessObjectFactory factory)
		{
			if (!suburbPK.IsEmpty && address != null)
			{
				var suburb = factory.Load<RefCityTown>(suburbPK);
				return suburb != null && address.E2_City.ToUpper() == suburb.R9_InternationalName.ToUpper() && (address.E2_State.IsEmpty || suburb.State == null || address.E2_State.ToUpper() == suburb.State.RW_Code.ToUpper());
			}

			return true;
		}
	}
}


