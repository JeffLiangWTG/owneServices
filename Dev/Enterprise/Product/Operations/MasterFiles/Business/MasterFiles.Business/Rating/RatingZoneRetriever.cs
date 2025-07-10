using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class RatingZoneRetriever
	{
		public static HashSet<ZString> GetApplicableLocationCodes(ILocation location, OrgHeader[] zoneOwnerOrganizations)
		{
			var locationCodes = new HashSet<ZString>();
			if (location == null)
			{
				return locationCodes;
			}

			locationCodes.Add(location.Code);
			locationCodes.Add(ZString.Empty);

			if (location.IATACityCode != null)
			{
				locationCodes.Add(location.IATACityCode.Code);
			}

			if (location.Country != null)
			{
				locationCodes.Add(location.Country.RN_Code);
			}

			var internationalZones = GetApplicableRatingZones(location.Zones, zoneOwnerOrganizations);
			foreach (var zone in internationalZones)
			{
				locationCodes.Add(zone);
			}

			return locationCodes;
		}

		public static IEnumerable<ZString> GetApplicableRatingZones(RefZoneHeader[] zones, params OrgHeader[] zoneOwners)
		{
			return GetRefZoneHeaders(zones, zoneOwners)
					.Where(x => x.IsRatingAvailableZone)
					.Select(x => x.FZ_Code);
		}

		public static IEnumerable<ZString> GetApplicableWiseRatesZones(RefZoneHeader[] zones, params OrgHeader[] zoneOwners)
		{
			return GetRefZoneHeaders(zones, zoneOwners)
					.Where(x => x.FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean)
					.Select(x => x.FZ_Code);
		}

		static IEnumerable<RefZoneHeader> GetRefZoneHeaders(RefZoneHeader[] zones, OrgHeader[] zoneOwners)
		{
			return zones.Where(x => x.FZ_OH_RelatedParty.IsEmpty || zoneOwners.Where(y => y != null).Select(z => z.PK).Contains(x.FZ_OH_RelatedParty));
		}
	}
}

