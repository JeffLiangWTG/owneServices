namespace Enterprise.Rating.Business
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.Rating.Integration;
	using Enterprise.ZArchitecture.Schema;

	public class RateTransportZoneHelper : IRateTransportZoneHelper
	{
		#region Get Transport Zone Set

		#region Find Single Most Relevant and Specific Transport Zone Set

		/// <summary>
		/// Finds the most specific zone set for the provided parameters (i.e. An "OPS" zone set is more specific than an "ALL" zone set)
		/// </summary>
		internal static RateTransportProvider GetMostApplicableZoneSet(ILocation zoneSetLocation, IOrgHeader zoneSetOwner, string zoneSetType, string zoneMode, BusinessObjectFactory factory)
		{
			if (zoneSetLocation == null || zoneSetLocation.Country == null)
			{
				return null;
			}

			var possibleZoneSets = factory.Load<RateTransportProvider>(FindApplicableTransportZoneSetQuery(zoneSetType, zoneMode, zoneSetLocation, zoneSetOwner));
			if (possibleZoneSets.Length > 1)
			{
				Array.Sort(possibleZoneSets, (zoneSet1, zoneSet2) => new TransportZoneSetComparer().Compare(zoneSet1, zoneSet2));
			}

			return possibleZoneSets.FirstOrDefault();
		}

		/// <summary>
		/// Finds active Transport Zones sets matching given Location and Owner with expected fall backs
		/// </summary>
		/// <param name="zoneType">Zone Type for the zone set, falling back to "ALL"</param>
		/// <param name="rateEntryMode">rateEntry Mode matched with Zone Mode for the zone set, falling back to possible zone modes</param>
		/// <param name="location">City/Town or Country</param>
		/// <param name="owner">Related Party to the zone or null</param>
		static ZQuery FindApplicableTransportZoneSetQuery(string zoneType, string rateEntryMode, ILocation location, IOrgHeader owner)
		{
			var query = new ZQuery(RateTransportProviderSchema.TP_IsActive, true);

			var ownerQuery = new ZQuery(RateTransportProviderSchema.TP_OH_RelatedParty, DBNull.Value);
			if (owner != null)
			{
				ownerQuery.AddToFilter(JoinCondition.Or, RateTransportProviderSchema.TP_OH_RelatedParty, owner.PK);
			}

			query.AddToFilter(ownerQuery);

			if (zoneType == RatingConstants.RatingZoneTypes.All)
			{
				query.AddToFilter(RateTransportProviderSchema.TP_ZoneType, zoneType);
			}
			else
			{
				query.AddToFilter(RateTransportProviderSchema.TP_ZoneType, new object[] { zoneType, RatingConstants.RatingZoneTypes.All });
			}

			query.AddToFilter(RateTransportProviderSchema.TP_ZoneMode, GetPossibleZoneModes(rateEntryMode));

			var locationQuery = new ZQuery(RateTransportProviderSchema.TP_RN_NKCountry, location.Country.Code)
				.AddToFilter(JoinCondition.And, RateTransportProviderSchema.TP_R9_ZoneHubLocation, null);

			if (location.CityTown != null)
			{
				// Adding separate parameters for location and hub query, assuming that there could be records with a set location when hubLocation is set
				// "TP_RN_NKCountry = @CWO4_ or TP_R9_ZoneHubLocation = @CWO5_"
				// changes to
				// "(TP_RN_NKCountry = @CWO4_ and TP_R9_ZoneHubLocation is NULL) or TP_R9_ZoneHubLocation = @CWO5_"
				// This is expected:
				// |TP_R9_ZoneHubLocation	|TP_RN_NKCountry	|
				// |GUID					|Empty string		|
				// |NULL					|AU					|
				// This is a buggy scenario:
				// |TP_R9_ZoneHubLocation	|TP_RN_NKCountry	|
				// |GUID					|AU					|
				// |NULL					|AU					|

				var hubLocationQuery = new ZQuery(RateTransportProviderSchema.TP_R9_ZoneHubLocation, location.CityTown.PK);

				query.AddToFilter(new ZQuery(locationQuery, JoinCondition.Or, hubLocationQuery));
			}
			else
			{
				query.AddToFilter(locationQuery);
			}

			return query;
		}

		internal static IEnumerable<string> GetPossibleZoneModes(ZString rateEntryMode)
		{
			var results = new HashSet<string>();

			results.Add(rateEntryMode);
			results.Add(Core.Constants.RateMode.ALL);

			switch (rateEntryMode)
			{
				case Core.Constants.RateMode.ULD:
				case Core.Constants.RateMode.LSE:
					results.Add(Core.Constants.RateMode.AIR);
					break;

				case Core.Constants.RateMode.FCL:
				case Core.Constants.RateMode.LCL:
					results.Add(Core.Constants.RateMode.SEA);
					break;

				case Core.Constants.RateMode.FRO:
				case Core.Constants.RateMode.FTL:
				case Core.Constants.RateMode.LRO:
					results.Add(Core.Constants.RateMode.ROA);
					break;

				case Core.Constants.RateMode.LRA:
				case Core.Constants.RateMode.FRA:
				case Core.Constants.RateMode.FWL:
					results.Add(Core.Constants.RateMode.RAI);
					break;
			}

			return results;
		}

		#endregion

		#region Find Multiple Transport Zone Sets

		/// <summary>
		/// Finds the most applicable zone set for the location's country. If it's an international location, we look at the most applicable zone for each country.
		/// </summary>
		public static IEnumerable<RateTransportProvider> GetTransportZoneSets(ILocation zoneSetLocation, IOrgHeader zoneSetOwner, string zoneSetType, string zoneMode, BusinessObjectFactory factory = null)
		{
			if (zoneSetLocation == null)
			{
				throw new ArgumentNullException(nameof(zoneSetLocation));
			}

			if (factory == null)
			{
				factory = new ReadOnlyBusinessObjectFactory();
			}

			var internationalZone = zoneSetLocation as RefZoneHeader;
			var result = internationalZone != null
				? GetTransportZoneSetsForInternationalZones(internationalZone, zoneSetOwner, zoneSetType, zoneMode, factory)
				: new[] { GetMostApplicableZoneSet(zoneSetLocation, zoneSetOwner, zoneSetType, zoneMode, factory) };

			return result;
		}

		static IEnumerable<RateTransportProvider> GetTransportZoneSetsForInternationalZones(RefZoneHeader internationalZone, IOrgHeader zoneSetOwner, string zoneSetType, string zoneMode, BusinessObjectFactory factory)
		{
			if (!internationalZone.Countries.Any())
			{
				return Enumerable.Empty<RateTransportProvider>();
			}

			var results = new List<RateTransportProvider>();
			foreach (RefCountry country in internationalZone.Countries)
			{
				var resultForCountry = GetMostApplicableZoneSet(country, zoneSetOwner, zoneSetType, zoneMode, factory);
				if (resultForCountry != null)
				{
					results.Add(resultForCountry);
				}
			}

			return results;
		}

		#endregion

		#endregion

		#region Get Transport Zone

		internal static RateTransportZone GetZoneForPostCodeOrCityTown(RateTransportProvider zoneSet, ZString country, ZString postCode, ZString citySuburb)
		{
			if (zoneSet == null || !zoneSet.TP_IsActive)
			{
				return null;
			}

			// Country is mandatory.
			// Returns:
			// - Zones with a post code range
			// - Zones with just a 'From PostCode'
			// - Zones with a city/town and a 'From PostCode'
			// - Zones not excluding postcodes with just a city town. The postcode must be linked via pivot table.
			// - Zones not excluding postcodes with just a city town that has no linked post codes. Ignore the given postcode.
			// - Zones excluding postcodes.
			var sqlQuery = "SELECT TZ_PK FROM GetTransportZoneByLocation(@ZoneSetPK, @PostCode, @CityTown, @Country)"; // T-SQL query
			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@ZoneSetPK", zoneSet.PK, RateTransportProviderSchema.PK),
				ZSqlParameter.New("@PostCode", postCode, RefPostCodeSchema.RK_CityTownPostCode),
				ZSqlParameter.New("@CityTown", citySuburb, RefCityTownSchema.R9_InternationalName),
				ZSqlParameter.New("@Country", country, RateTransportZoneItemSchema.TQ_RN_NKCountry)
			};

			var factory = zoneSet.Factory;
			var queryResult = new DynamicBusinessObjectCollection(factory);
			queryResult.Load(sqlQuery, sqlParams);

			if (queryResult.Count == 1)
			{
				return factory.Load<RateTransportZone>((ZGuid)queryResult[0][RateTransportZonesSchema.PK]);
			}

			return null;
		}

		internal static RateTransportZone GetZoneForDistance(RateTransportProvider zoneSet, string countryCode, ZDecimal distance)
		{
			if (zoneSet == null || !zoneSet.TP_IsActive || string.IsNullOrEmpty(countryCode))
			{
				return null;
			}

			foreach (var zone in zoneSet.Zones)
			{
				if (zone.Items.Any(i => i.TQ_RN_NKCountry == countryCode && i.TQ_ToDistance > 0 && distance > i.TQ_FromDistance && distance <= i.TQ_ToDistance))
				{
					return zone;
				}
			}

			return null;
		}

		#endregion

		#region Get Transport Zone Item

		internal static bool IsCityTownMatching(RateTransportZoneItem item, string cityTown, bool checkPostCodeCities = true, string cityTownPostcode = null)
		{
			var result = false;

			if (item.CityTown != null)
			{
				result = item.CityTown.R9_InternationalName.EqualsIgnoringCase(cityTown) && IsCityTownPostcodeMatching(item, cityTownPostcode);
			}
			else if (checkPostCodeCities && !item.TQ_FromPostCode.IsEmpty)
			{
				var matchingCityTowns = item.Factory.Load<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, cityTown));
				result = IsPostCodeCitiesMatching(item, cityTownPostcode, matchingCityTowns);
			}

			return result;
		}

		internal static bool IsCityTownMatching(RateTransportZoneItem item, RefCityTown cityTown, bool checkPostCodeCities = true, string cityTownPostcode = null)
		{
			if (cityTown == null)
			{
				return false;
			}

			var result = false;
			if (item.CityTown != null)
			{
				result = item.CityTown.PK == cityTown.PK && IsCityTownPostcodeMatching(item, cityTownPostcode);
			}
			else if (checkPostCodeCities && !item.TQ_FromPostCode.IsEmpty)
			{
				result = IsPostCodeCitiesMatching(item, cityTownPostcode, cityTown);
			}

			return result;
		}

		static bool IsPostCodeCitiesMatching(RateTransportZoneItem item, string cityTownPostcode, params RefCityTown[] matchingCityTowns)
		{
			var result = false;
			var comparer = new PostcodeOrderComparer();
			foreach (var matchingCityTown in matchingCityTowns)
			{
				if (cityTownPostcode == null)
				{
					foreach (var postcode in matchingCityTown.PostCodes)
					{
						result = ComparePostCode(item, postcode.RK_CityTownPostCode, comparer);

						if (result)
						{
							break;
						}
					}
				}
				else
				{
					result = ComparePostCode(item, cityTownPostcode, comparer);
				}

				if (result)
				{
					break;
				}
			}

			return result;
		}

		static bool IsCityTownPostcodeMatching(RateTransportZoneItem item, string cityTownPostcode)
		{
			if (cityTownPostcode == null)
			{
				return true;
			}

			if (item.TQ_FromPostCode.IsEmpty)
			{
				var citiesPKsWithPostCode = GetAllCitiesPKsWithPostCode(item.Factory, cityTownPostcode);
				return citiesPKsWithPostCode.Contains(item.CityTown.PK);
			}

			return item.TQ_FromPostCode == cityTownPostcode;
		}

		static bool ComparePostCode(RateTransportZoneItem item, string cityTownPostcode, PostcodeOrderComparer comparer)
		{
			bool result;
			if (!item.TQ_ToPostCode.IsEmpty)
			{
				result = comparer.Compare(cityTownPostcode, item.TQ_FromPostCode) >= 0
					&& comparer.Compare(cityTownPostcode, item.TQ_ToPostCode) <= 0;
			}
			else
			{
				result = comparer.Compare(cityTownPostcode, item.TQ_FromPostCode) == 0;
			}

			return result;
		}

		internal static bool IsPostCodeWithinRange(RateTransportZoneItem item, ZString postcode, bool checkPostCodeCities = true, List<ZGuid> citiesPKsWithPostCode = null)
		{
			var result = false;

			if (!item.TQ_FromPostCode.IsEmpty)
			{
				var comparer = new PostcodeOrderComparer();
				if (!item.TQ_ToPostCode.IsEmpty)
				{
					result = comparer.Compare(postcode, item.TQ_FromPostCode) >= 0 &&
						comparer.Compare(postcode, item.TQ_ToPostCode) <= 0;
				}
				else
				{
					result = comparer.Compare(postcode, item.TQ_FromPostCode) == 0;
				}
			}
			else if (checkPostCodeCities && item.CityTown != null)
			{
				if (citiesPKsWithPostCode != null)
				{
					result = citiesPKsWithPostCode.Contains(item.CityTown.PK);
				}
			}

			return result;
		}

		internal static List<ZGuid> GetAllCitiesPKsWithPostCode(BusinessObjectFactory factory, string postCode)
		{
			var citiesPKsWithPostCode = new List<ZGuid>();

			var refCityQuery = new ZDBOnlyQuery(typeof(RefCityTown));
			var refCityPcodePivotSub = new ZDBOnlySubQuery(typeof(RefCityPCodePivot), RefCityPCodePivotSchema.R0_R9);

			var refPostCodeSub = new ZDBOnlySubQuery(typeof(RefPostCode), RefPostCodeSchema.PK, RefCityPCodePivotSchema.R0_RK);
			refPostCodeSub.AddToFilter(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, postCode));

			refCityPcodePivotSub.AddSubQuery(refPostCodeSub, JoinCondition.And);
			refCityQuery.AddSubQuery(refCityPcodePivotSub, JoinCondition.And);

			foreach (var city in factory.Load<RefCityTown>(refCityQuery))
			{
				citiesPKsWithPostCode.Add(city.PK);
			}

			return citiesPKsWithPostCode;
		}

		public static RateTransportZoneItem GetZoneItemForPostCode(RateTransportProvider provider, string postCode)
		{
			if (!string.IsNullOrEmpty(postCode))
			{
				var citiesPKsWithPostCode = GetAllCitiesPKsWithPostCode(provider.Factory, postCode);
				return GetZoneItem(provider, item => IsPostCodeWithinRange(item, postCode, checkPostCodeCities: true, citiesPKsWithPostCode));
			}

			return null;
		}

		static RateTransportZoneItem GetZoneItem(RateTransportProvider provider, Func<RateTransportZoneItem, bool> isZoneItem)
		{
			RateTransportZoneItem result = null;
			if (provider != null && provider.TP_IsActive)
			{
				AddFetchHintsForCityAndPostCode(provider);

				result = provider.Zones
					.Where(zone => zone != null && zone.TZ_IsActive)
					.SelectMany(zone => zone.Items)
					.FirstOrDefault(isZoneItem);
			}

			return result;
		}

		static void AddFetchHintsForCityAndPostCode(RateTransportProvider provider)
		{
			foreach (var zone in provider.Zones)
			{
				foreach (var item in zone.Items)
				{
					var cityPostCodePivotQuery = new ZDBOnlySubQuery(typeof(RefCityPCodePivot), RefCityPCodePivotSchema.R0_RK);
					cityPostCodePivotQuery.AddToFilter(RefCityPCodePivotSchema.R0_R9, item.TQ_R9_CityTown);
					var postCodeQuery = new ZDBOnlyQuery(typeof(RefPostCode));
					postCodeQuery.AddSubQuery(cityPostCodePivotQuery, JoinCondition.And);

					item.Factory.AddFetchHint(RefPostCodeSchema.Instance, postCodeQuery);

					item.Factory.AddFetchHint(typeof(RefCityPCodePivot), RefCityPCodePivotSchema.R0_R9, item.TQ_R9_CityTown);
				}
			}
		}

		#endregion

		#region IRateTransportZoneHelper Members

		string IRateTransportZoneHelper.GetZoneName(BusinessObjectFactory factory, IOrgHeader owner, object location, string countryCode, string postCode, string citySuburb)
		{
			if (!string.IsNullOrEmpty(postCode) || !string.IsNullOrEmpty(citySuburb))
			{
				var zoneSet = GetMostApplicableZoneSet((ILocation)location, owner, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL, factory);
				var zone = GetZoneForPostCodeOrCityTown(zoneSet, countryCode, postCode, citySuburb);
				if (zone != null)
				{
					return zone.TZ_ZoneName;
				}
			}

			return string.Empty;
		}

		bool IRateTransportZoneHelper.IsBeyond(BusinessObjectFactory factory, IOrgHeader owner, object location, string countryCode, string postCode, string citySuburb)
		{
			var hasPostCode = !string.IsNullOrEmpty(postCode);
			var hasCitySuburb = !string.IsNullOrEmpty(citySuburb);
			if (!hasPostCode && !hasCitySuburb)
			{
				return false;
			}

			var zoneSet = GetMostApplicableZoneSet((ILocation)location, owner, RatingConstants.RatingZoneTypes.Rating, Core.Constants.RateMode.ALL, factory);
			var zone = GetZoneForPostCodeOrCityTown(zoneSet, countryCode, postCode, citySuburb);
			if (zone != null)
			{
				RateTransportZoneItem matchingItem = null;

				if (hasPostCode)
				{
					var citiesPKsWithPostCode = GetAllCitiesPKsWithPostCode(factory, postCode);
					matchingItem = zone.Items.FirstOrDefault(item => IsPostCodeWithinRange(item, postCode, checkPostCodeCities: true, citiesPKsWithPostCode));

					if (matchingItem != null)
					{
						return matchingItem.TQ_IsBeyond;
					}
				}

				if (hasCitySuburb)
				{
					matchingItem = zone.Items.FirstOrDefault(item => IsCityTownMatching(item, citySuburb, false, postCode));
				}

				return matchingItem != null && matchingItem.TQ_IsBeyond;
			}

			return false;
		}

		#endregion
	}
}

