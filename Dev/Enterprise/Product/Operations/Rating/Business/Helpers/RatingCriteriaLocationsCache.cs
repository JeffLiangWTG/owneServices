using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Recommended Improvement - make all the locations members of RatingCriteria 
	/// OR make a class with those properties and have a property of RatingCriteria return an instance
	/// No need for overhead of cache keys and and name lookups.
	/// If caching is needed between different instances of RatingCriteria for the same factory
	/// then it should be caching for each location code, not for the entire set.
	/// </summary>
	public class RatingCriteriaLocationsCache
	{
		public RatingCriteriaLocationsCache(BusinessObjectFactory factory, RatingCriteriaLocationsCacheParameters parameters)
		{
			Locations = new Dictionary<string, LocationInfo[]>();

			foreach (var pair in parameters.GetNameLocationsPair())
			{
				var locationInfos = new List<LocationInfo>();

				foreach (var locationCode in pair.Value)
				{
					locationInfos.Add(new LocationInfo(factory, parameters.ZoneOwners, locationCode));
				}

				Locations.Add(pair.Key, locationInfos.ToArray());
			}
		}

		public LocationInfo GetLocation(string name) => this[name].FirstOrDefault();

		public IEnumerable<LocationInfo> GetLocations(string name) => this[name];

		public IEnumerable<LocationInfo> this[string name] =>
			Locations.TryGetValue(name, out var locationInfo)
				? locationInfo
				: Enumerable.Empty<LocationInfo>();

		readonly IDictionary<string, LocationInfo[]> Locations;

		#region Keys

		public static class Keys
		{
			public const string Origin = nameof(RatingCriteriaLocationsCacheParameters.Origin);
			public const string Destination = nameof(RatingCriteriaLocationsCacheParameters.Destination);
			public const string Via = nameof(RatingCriteriaLocationsCacheParameters.Via);
			public const string PlannedLoad = nameof(RatingCriteriaLocationsCacheParameters.PlannedLoad);
			public const string PlannedDischarge = nameof(RatingCriteriaLocationsCacheParameters.PlannedDischarge);
			public const string RateOrigin = nameof(RatingCriteriaLocationsCacheParameters.RateOrigin);
			public const string RateDestination = nameof(RatingCriteriaLocationsCacheParameters.RateDestination);
			public const string FirstLoad = nameof(RatingCriteriaLocationsCacheParameters.FirstLoad);
			public const string LastDischarge = nameof(RatingCriteriaLocationsCacheParameters.LastDischarge);
			public const string FirstRouteSetLoad = nameof(RatingCriteriaLocationsCacheParameters.FirstRouteSetLoad);
			public const string LastRouteSetDischarge = nameof(RatingCriteriaLocationsCacheParameters.LastRouteSetDischarge);
			public const string SortedOverridenPlannedLoads = nameof(RatingCriteriaLocationsCacheParameters.SortedOverridenPlannedLoads);
			public const string SortedOverridenPlannedDischarges = nameof(RatingCriteriaLocationsCacheParameters.SortedOverridenPlannedDischarges);
			public const string OriginServices = nameof(RatingCriteriaLocationsCacheParameters.OriginServices);
			public const string DestinationServices = nameof(RatingCriteriaLocationsCacheParameters.DestinationServices);
		}

		#endregion

		public class LocationInfo
		{
			public LocationInfo()
			{
				Zones = Enumerable.Empty<ZString>();
			}

			public LocationInfo(BusinessObjectFactory factory, IEnumerable<OrgHeader> zoneOwners, string locationCode)
			{
				var locationBizObj = LocationHelper.GetCachedLocationFromString(locationCode, factory);

				UNLOCO = locationBizObj?.UNLOCO?.Code ?? ZString.Empty;
				IATACityCode = locationBizObj?.IATACityCode?.Code ?? ZString.Empty;
				Country = locationBizObj?.Country?.Code ?? ZString.Empty;
				Zones = RatingZoneRetriever
					.GetApplicableRatingZones(locationBizObj?.Zones ?? System.Array.Empty<RefZoneHeader>(), zoneOwners.ToArray())
					.Distinct()
					.ToArray();
			}

			public ZString UNLOCO { get; }
			public ZString IATACityCode { get; }
			public ZString Country { get; }
			public IEnumerable<ZString> Zones { get; }
		}
	}

	public class RatingCriteriaLocationsCacheParameters
	{
		public RatingCriteriaLocationsCacheParameters()
		{
			ZoneOwners = Enumerable.Empty<OrgHeader>();

			// Please don't forget to initialize Multi-Location properties here
			SortedOverridenPlannedLoads = Enumerable.Empty<ZString>();
			SortedOverridenPlannedDischarges = Enumerable.Empty<ZString>();
			OriginServices = Enumerable.Empty<ZString>();
			DestinationServices = Enumerable.Empty<ZString>();
		}

		// Single-Location Properties
		public ZString Origin { get; set; }
		public ZString Destination { get; set; }
		public ZString Via { get; set; }
		public ZString PlannedLoad { get; set; }
		public ZString PlannedDischarge { get; set; }
		public ZString RateOrigin { get; set; }
		public ZString RateDestination { get; set; }
		public ZString FirstLoad { get; set; }
		public ZString LastDischarge { get; set; }
		public ZString FirstRouteSetLoad { get; set; }
		public ZString LastRouteSetDischarge { get; set; }

		// Multi-Location Properties
		public IEnumerable<ZString> SortedOverridenPlannedLoads { get; set; }
		public IEnumerable<ZString> SortedOverridenPlannedDischarges { get; set; }
		public IEnumerable<ZString> OriginServices { get; set; }
		public IEnumerable<ZString> DestinationServices { get; set; }

		// Non-ZString/Non-Location Properties
		public IEnumerable<OrgHeader> ZoneOwners { get; set; }

		public string GetCacheKey()
		{
			var singleLocationCodes = GetType().GetProperties()
				.Where(p => p.PropertyType == typeof(ZString))
				.OrderBy(p => p.Name)
				.Select(p => (ZString)p.GetValue(this))
				.ToArray();

			var multiLocationCodes = GetType().GetProperties()
				.Where(p => p.PropertyType == typeof(IEnumerable<ZString>))
				.OrderBy(p => p.Name)
				.Select(p => ((IEnumerable<ZString>)p.GetValue(this))
					.Distinct()
					.OrderBy(locationCode => locationCode)
					.ToArray());

			var codeLists = new List<ZString[]> { singleLocationCodes };
			codeLists.AddRange(multiLocationCodes);
			codeLists.Add(ZoneOwners.Select(org => org?.OH_Code ?? ZString.Empty).Distinct().OrderBy(orgCode => orgCode).ToArray());

			var rawKeyList = codeLists.Select(codes => string.Join("~", codes));
			return string.Join("|", rawKeyList);
		}

		public IDictionary<string, ZString[]> GetNameLocationsPair()
		{
			var result = new Dictionary<string, ZString[]>();
			var properties = GetType().GetProperties();

			foreach (var prop in properties.Where(p => p.PropertyType == typeof(ZString)))
			{
				result.Add(prop.Name, new[] { (ZString)prop.GetValue(this) });
			}

			foreach (var prop in properties.Where(p => p.PropertyType == typeof(IEnumerable<ZString>)))
			{
				var locationCodes = (IEnumerable<ZString>)prop.GetValue(this);
				result.Add(prop.Name, locationCodes.Distinct().ToArray());
			}

			return result;
		}
	}
}

