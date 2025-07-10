using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.MasterFiles.Business
{
	public static class LocationHelper
	{
		#region Location Type

		[Flags]
		public enum LocationType
		{
			Unknown = 0,
			Port = 1,
			IATACityCode = 2,
			Country = 4,
			Zone = 8,
			All = Port | IATACityCode | Country | Zone
		}

		public static LocationType GetLocationType(ZString locationCode)
		{
			switch (locationCode.Length)
			{
				case 5:
					return LocationType.Port;

				case 4:
					return LocationType.Zone;

				case 3:
					return LocationType.IATACityCode;

				case 2:
					return LocationType.Country;

				default:
					return LocationType.Unknown;
			}
		}

		#endregion

		#region GetLocation

		#region GetLocationFromIDocAddress

		public static ILocation GetLocationFromIDocAddress(IDocAddress iDocAddress, BusinessObjectFactory factory)
		{
			var location = (iDocAddress.E2_AddressOverride) ? iDocAddress.CountryCode : iDocAddress.E2_PortCode;
			return GetLocationFromString(location, factory);
		}

		#endregion

		#region GetLocationFromString

		public static ILocation GetLocationFromString(ZString locationCode, BusinessObjectFactory factory)
		{
			switch (GetLocationType(locationCode))
			{
				case LocationType.Port:
					return factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, locationCode);

				case LocationType.IATACityCode:
					return IATACityCode.GetValidOrDefault(factory, locationCode);

				case LocationType.Country:
					return factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, locationCode);

				case LocationType.Zone:
					var zoneTypeQuery = new ZQuery(RefZoneHeaderSchema.FZ_Code, locationCode);
					zoneTypeQuery.AddToFilter(RefZoneHeaderSchema.FZ_ZoneType, SQLComparisonOperator.NotEqual, RefZoneHeaderLookups.ZoneTypeCodes.WiseRatesOcean);
					zoneTypeQuery.IgnoreActiveFilter = true;
					return factory.Load<RefZoneHeader>(zoneTypeQuery).SingleOrDefault();

				default:
					return null;
			}
		}

		public static ILocation GetCachedLocationFromString(ZString locationCode, BusinessObjectFactory factory, string cacheKey = nameof(GetCachedLocationFromString))
		{
			Argument.NotNull(factory, nameof(factory));

			var cache = factory.GetCachedValue(cacheKey, () => new Dictionary<ZString, ILocation>());
			return cache.GetOrAdd(locationCode, () => GetLocationFromString(locationCode, factory));
		}

		#endregion

		#region GetCityTown

		public static RefCityTown GetCityTown(this ILocation location, ZString internationalName, BusinessObjectFactory factory)
		{
			RefCityTown result = null;

			if (location != null && !internationalName.IsEmpty)
			{
				var country = location.Country;
				if (country != null && !country.RN_Code.IsEmpty)
				{
					var query = new ZQuery(RefCityTownSchema.R9_RN_NKCountry, country.RN_Code);
					query.AddToFilter(RefCityTownSchema.R9_InternationalName, internationalName);
					if (location.State != null && !location.State.RW_Code.IsEmpty)
					{
						query.AddToFilter(RefCityTownSchema.R9_RW_NKState, location.State.RW_Code);
					}

					var possibleResults = factory.Load<RefCityTown>(query);
					if (possibleResults.Length == 1)
					{
						result = possibleResults[0];
					}
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region CompletelyCovers

		public static bool CompletelyCovers(this ILocation mainLocation, ILocation otherLocation)
		{
			if (mainLocation is RefUNLOCO mainPort)
			{
				return mainPort.PK == otherLocation?.UNLOCO?.PK || otherLocation.IsEmptyZone();
			}
			if (mainLocation is IATACityCode mainIATACityCode)
			{
				return mainIATACityCode.Equals(otherLocation?.IATACityCode) || otherLocation.IsEmptyZone();
			}
			if (mainLocation is RefCountry mainCountry)
			{
				return mainCountry.PK == otherLocation?.Country?.PK || otherLocation.IsEmptyZone();
			}
			if (mainLocation is RefZoneHeader mainZone)
			{
				return (otherLocation?.Zones.Contains(mainZone, BusinessObjectEqualityComparer<RefZoneHeader>.PKOnlyComparer) ?? false) || otherLocation.IsEmptyZone();
			}
			if (mainLocation is OrgAddress mainOrgAddress)
			{
				return otherLocation is OrgAddress otherOrgAddress && mainOrgAddress.PK == otherOrgAddress.PK;
			}
			if (mainLocation is JobDocAddress mainJobDocAddress)
			{
				return otherLocation is JobDocAddress otherJobDocAddress && mainJobDocAddress.PK == otherJobDocAddress.PK;
			}

			throw new NotImplementedException("The location type has not been supported yet: " + mainLocation?.GetType().FullName ?? "null");
		}

		public static bool IsEmptyZone(this ILocation location) =>
			location is RefZoneHeader zone && zone.UNLOCOs.Count == 0 && zone.Countries.Count == 0;

		public static bool IsLocationRule(this ILocation location) => location is LocationRule;

		#endregion

		#region Location Filter

		public static ZQuery GetLocationFilter(BusinessObjectFactory factory, ZString value, SchemaColumn field, Type typeToQuery)
		{
			return GetLocationFilter(factory, field, false, typeToQuery, value);
		}

		public static ZQuery GetLocationFilter(BusinessObjectFactory factory, ZString value, SchemaColumn field, LocationType fieldType, Type typeToQuery)
		{
			return GetLocationFilter(factory, field, fieldType, false, typeToQuery, value);
		}

		public static ZQuery GetLocationFilter(BusinessObjectFactory factory, ZString value, SchemaColumn field, bool allowEmpty, Type typeToQuery)
		{
			return GetLocationFilter(factory, field, allowEmpty, typeToQuery, value);
		}

		public static ZQuery GetLocationFilter(BusinessObjectFactory factory, ZString value, SchemaColumn field, LocationType fieldType, bool allowEmpty, Type typeToQuery)
		{
			return GetLocationFilter(factory, field, fieldType, allowEmpty, typeToQuery, value);
		}

		public static ZQuery GetLocationFilter(BusinessObjectFactory factory, SchemaColumn field, bool allowEmpty, Type typeToQuery, params ZString[] values)
		{
			return GetLocationFilter(factory, field, DetermineLocationTypeFromField(field), allowEmpty, typeToQuery, values);
		}

		public static ZQuery GetLocationFilter(BusinessObjectFactory factory, SchemaColumn field, LocationType fieldType, bool allowEmpty, Type typeToQuery, params ZString[] values)
		{
			var result = new ZDBOnlyQuery(typeToQuery);

			var (sql, paramValues) = BuildLocationFilter(factory, field, allowEmpty, fieldType, values);
			result.AddFilterAndZSQLParameterCollection(sql, paramValues);

			return result;
		}

		#region SuppressResourceStringsCheckRegion

		static (ZString, ZSqlParameterCollection) BuildLocationFilter(BusinessObjectFactory factory, SchemaColumn field, bool allowEmpty, LocationType fieldType, params ZString[] values)
		{
			var paramValues = new ZSqlParameterCollection();

			var allValues = new Dictionary<ZString, ZString>();
			if (allowEmpty)
			{
				var empty = "''";
				allValues.Add(empty, empty);
			}

			var countryValues = new HashSet<ZString>();
			var zoneValues = new HashSet<ZString>();

			for (int i = 0; i < values.Length; i++)
			{
				var locationType = GetLocationType(values[i]);
				var location = GetLocationFromString(values[i], factory);

				var parameter = Invariant($"@{field.Name}{i}");

				bool addParameter = false;
				if ((fieldType.HasFlag(locationType) || locationType == LocationType.Unknown) && !allValues.ContainsKey(values[i]))
				{
					addParameter = true;
					allValues.Add(values[i], parameter);
				}

				if (locationType == LocationType.Country && fieldType.HasFlag(LocationType.Port))
				{
					var countryParameter = Invariant($"@Country{field.Name}{i}");
					var param = ZSqlParameter.New(countryParameter, values[i] + "%", field, SQLComparisonOperator.Like);
					paramValues.Add(param);

					countryValues.Add(((IFilterPart)param).ParameterisedSql(new ParameterNameFactory()).ParameterisedQueryText);
				}
				else if (locationType == LocationType.Zone && (fieldType.HasFlag(LocationType.Country) || fieldType.HasFlag(LocationType.Port)))
				{
					addParameter = true;
					zoneValues.Add(parameter);
				}
				else
				{
					if (location != null && locationType != LocationType.Country && fieldType.HasFlag(LocationType.Country))
					{
						var country = location.Country;
						if (country != null && !allValues.ContainsKey(country.RN_Code))
						{
							var unlocoCountryParameter = Invariant($"@RN{field.Name}{i}");
							allValues.Add(country.RN_Code, unlocoCountryParameter);
							paramValues.Add(unlocoCountryParameter, country.RN_Code, field);
						}
					}
				}

				if (addParameter)
				{
					paramValues.Add(parameter, values[i], field);
				}

				if (location != null && fieldType.HasFlag(LocationType.Zone) && locationType != LocationType.Zone && location.Zones != null)
				{
					var zonePosition = 0;
					foreach (var zone in location.Zones)
					{
						if (!allValues.ContainsKey(zone.FZ_Code))
						{
							var zoneParameter = Invariant($"@FZ{field.Name}{i}{zonePosition}");
							allValues.Add(zone.FZ_Code, zoneParameter);
							paramValues.Add(zoneParameter, zone.FZ_Code, field);
							zonePosition++;
						}
					}
				}
			}

			var sql = GetSqlTextFromValues(field, fieldType, allValues, countryValues, zoneValues);

			return (sql, paramValues);
		}

		static ZString GetSqlTextFromValues(SchemaColumn field, LocationType fieldType, Dictionary<ZString, ZString> allValues, HashSet<ZString> countryValues, HashSet<ZString> zoneValues)
		{
			var sqlBuilder = new ZStringBuilder();
			if (allValues.Any())
			{
				var addNotEmpty = ((SchemaStringColumn)field).IsNonBlankFilteredIndexParticipant && allValues.All(x => !x.Value.IsEmpty);
				if (addNotEmpty)
				{
					sqlBuilder.Append(Invariant($"{field.Name} <> '' AND "));
				}

				sqlBuilder.Append(Invariant($"{field.Name} IN ({string.Join(", ", allValues.Values)})"));
			}

			BuildSearchInCountrySql(field.Name, fieldType, string.Join(" OR ", countryValues), sqlBuilder);
			BuildSearchInZoneSql(field.Name, fieldType, string.Join(", ", zoneValues), sqlBuilder);

			ZString result = sqlBuilder.ToString();
			if (!result.IsEmpty)
			{
				result = Invariant($"({result})");
			}

			return result;
		}

		static void BuildSearchInCountrySql(string fieldName, LocationType fieldType, ZString countryValues, ZStringBuilder sqlBuilder)
		{
			if (!countryValues.IsEmpty)
			{
				if (fieldType == LocationType.Port)
				{
					sqlBuilder.AppendWithOptionalPrefix(countryValues);
				}
				else
				{
					sqlBuilder.AppendWithOptionalPrefix(Invariant($"(({countryValues}) AND LEN({fieldName}) = 5)"));
				}
			}
		}

		static void BuildSearchInZoneSql(string fieldName, LocationType fieldType, ZString zoneValues, ZStringBuilder sqlBuilder)
		{
			if (zoneValues.IsEmpty)
			{
				return;
			}

			if (fieldType == LocationType.Port)
			{
				var zoneByCountryOrUnlocoSql = Invariant($@"{fieldName} IN
(
	SELECT DISTINCT {RefUNLOCOSchema.RL_Code.Name}
	FROM dbo.vw_ZoneUNLOCO WITH (NOEXPAND)
	WHERE {RefZoneHeaderSchema.FZ_Code.Name} IN ({zoneValues})
	UNION ALL
	SELECT DISTINCT {RefUNLOCOSchema.RL_Code.Name}
	FROM dbo.vw_ZoneUNLOCOByCountry WITH (NOEXPAND)
	WHERE {RefZoneHeaderSchema.FZ_Code.Name} IN ({zoneValues})
)");
				sqlBuilder.AppendWithOptionalPrefix(zoneByCountryOrUnlocoSql);
			}
			else
			{
				sqlBuilder.AppendWithOptionalPrefix("(\r\n"); // start group

				if (fieldType.HasFlag(LocationType.Zone))
				{
					sqlBuilder.Append(Invariant($"\tLEN({fieldName}) IN (2, 5) AND\r\n"));
				}

				sqlBuilder.Append(Invariant($"\t{fieldName} IN\r\n\t(")); // start IN clause

				sqlBuilder.Append(Invariant($@"
		SELECT DISTINCT {RefCountrySchema.RN_Code.Name}
		FROM dbo.vw_ZoneCountry WITH (NOEXPAND)
		WHERE {RefZoneHeaderSchema.FZ_Code.Name} IN ({zoneValues})"));

				if (fieldType.HasFlag(LocationType.Port))
				{
					// Including all possible port codes appears faster than
					// truncating RateEntry port codes to country values
					sqlBuilder.Append(Invariant($@"
		UNION ALL
		SELECT DISTINCT {RefUNLOCOSchema.RL_Code.Name}
		FROM dbo.vw_ZoneUNLOCO WITH (NOEXPAND)
		WHERE {RefZoneHeaderSchema.FZ_Code.Name} IN ({zoneValues})
		UNION ALL
		SELECT DISTINCT {RefUNLOCOSchema.RL_Code.Name}
		FROM dbo.vw_ZoneUNLOCOByCountry WITH (NOEXPAND)
		WHERE {RefZoneHeaderSchema.FZ_Code.Name} IN ({zoneValues})"));
				}

				sqlBuilder.Append("\r\n\t)"); // end IN clause
				sqlBuilder.Append("\r\n)"); // end group
			}
		}

		static LocationType DetermineLocationTypeFromField(SchemaColumn field)
		{
			LocationType result = LocationType.All;
			string columnPrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(field.TableName);
			if (!string.IsNullOrEmpty(columnPrefix))
			{
				if (field.Name.StartsWith(Invariant($"{columnPrefix}_{RefUNLOCOSchema.Constants.Prefix}_NK"), StringComparison.OrdinalIgnoreCase))
				{
					result = LocationType.Port;
				}
				else if (field.Name.StartsWith(Invariant($"{columnPrefix}_{RefCountrySchema.Constants.Prefix}_NK"), StringComparison.OrdinalIgnoreCase))
				{
					result = LocationType.Country;
				}
			}

			return result;
		}

		static void AppendWithOptionalPrefix(this ZStringBuilder sqlBuilder, string message)
		{
			if (!sqlBuilder.IsEmpty)
			{
				sqlBuilder.Append(" OR ");
			}

			sqlBuilder.Append(message);
		}

		#endregion

		#endregion

		#region Retrieval

		#region SuppressResourceStringsCheckRegion

		public static ZString GetFullName(ILocation location, ZBool includeNonLocalCountry)
		{
			if (location == null)
			{
				return ZString.Empty;
			}

			var returnValue = location.Description;

			var loco = location as RefUNLOCO;
			if (loco != null)
			{
				if (loco.CountryStates != null && CountOfPortsInCountryWithSameName(loco) > 1)
				{
					returnValue = Invariant($"{returnValue} ({loco.CountryStates.RW_Code})");
				}

				if (includeNonLocalCountry)
				{
					var country = loco.Country;
					if (country != null && country.RN_Code != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
					{
						returnValue += ", " + country.RN_Code;
					}
				}
			}

			return returnValue;
		}

		static ZInt CountOfPortsInCountryWithSameName(RefUNLOCO loco)
		{
			var queryText = @"	SELECT		count(*) AS CountOfCities
									FROM		dbo.RefUNLOCO
									INNER JOIN	dbo.RefCountryStates ON RefCountryStates.RW_PK = RefUNLOCO.RL_RW
									WHERE		RL_NameWithDiacriticals = @CityName
									AND			RL_RN_NKCountryCode = @CountryCode";

			var paramValues = new ZSqlParameterCollection();
			paramValues.Add("@CityName", loco.Description, RefUNLOCOSchema.RL_NameWithDiacriticals);
			paramValues.Add("@CountryCode", loco.RL_RN_NKCountryCode, RefUNLOCOSchema.RL_Code);

			var query = new DynamicBusinessObjectCollection(loco.Factory);
			query.Load(queryText, paramValues);

			return (ZInt)query[0]["CountOfCities"];
		}

		#endregion

		#endregion
	}
}
