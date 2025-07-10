using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class AccountingTaxLocations
	{
		public static CodeDescriptionPairList GetLocations(BusinessObjectFactory factory, bool shouldApplyBranchLevelTaxOverrideRuleConfigurations = true)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			result.AddPair(AccChargeTaxOverride.ALL, ALL);
			result.AddPair(AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry, EuropeanUnionExcludingLoginCountry);
			result.AddPair(AccChargeTaxOverride.NotEuropeanUnion, NotEuropeanUnion);
			result.AddPair(AccChargeTaxOverride.AllCountriesExceptLoginCountry, AllCountriesExceptLoginCountry);
			result.AddPair(AccChargeTaxOverride.OtherTerritories, OtherTerritories);

			if (shouldApplyBranchLevelTaxOverrideRuleConfigurations && AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value)
			{
				result.AddPair(AccChargeTaxOverride.SameCountryAndStateAsLineBranch, SameCountryAndStateAsLineBranch);
				result.AddPair(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch, SameCountryDifferentStateAsLineBranch);
			}

			result.AddRange(new EconomicGroupList());
			result.AddRange(GetTaxZones(factory));
			result.AddRange(GetCountries(factory));

			return result;
		}

		public static CodeDescriptionPairList GetTaxRegistrationLocations(BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(AccChargeTaxOverride.ALL, ALL);
			result.AddRange(new EconomicGroupList());
			result.AddPair(AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry, EUExcludingLoginCountry);
			result.AddPair(OrgCusCode.CodeTypes.SpecialEconomicZone, Res.GetString("OrgCusCode.CodeTypes.SpecialEconomicZone", "Special Economic Zone"));
			result.AddRange(GetCountries(factory));

			return result;
		}

		public static CodeDescriptionPairList GetCountries(BusinessObjectFactory factory)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			RefCountryCollection collection = new RefCountryCollection(factory);
			var pairs = collection.Cast<RefCountry>().Select(x => new CodeDescriptionPair(x.RN_Code.ToString(), x.RN_DescMultilingual)).ToList();
			result.AddRange(pairs);
			result.Sort();

			return result;
		}

		static CodeDescriptionPairList GetTaxZones(BusinessObjectFactory factory)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			ZQuery filter = new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, RefZoneHeaderLookups.ZoneTypeCodes.Tax);
			RefZoneHeaderCollection collection = new RefZoneHeaderCollection(factory, filter);

			var pairs = collection.Cast<RefZoneHeader>().Select(x => new CodeDescriptionPair(x.FZ_Code.ToString(), x.FZ_DescriptionMultilingual)).ToList();
			result.AddRange(pairs);
			result.Sort();

			return result;
		}

		public static string GetBranchState(GlbBranch branch)
		{
			return (branch?.OrgProxy ?? branch?.Company.OrgProxy)?.MainAddress?.OA_State ?? string.Empty;
		}

		static string GetLocationFallbackCodesCachedKey(ILocation location, string currentCountryCode, string branchState)
		{
			var locationCode = !string.IsNullOrWhiteSpace(location?.Code) ? location?.Code.ToString() : location?.UNLOCO?.Code.ToString();
			return string.Format(CultureInfo.InvariantCulture, "{0} : {1} : {2} : {3}",
				!string.IsNullOrWhiteSpace(locationCode) ? locationCode : ZGuid.NewZGuid().ToString(), //if location code is empty then we should not cache the location fallback codes
				(location as OrgAddress)?.OA_State ?? "NULL",
				!string.IsNullOrWhiteSpace(currentCountryCode) ? currentCountryCode : "NULL",
				!string.IsNullOrWhiteSpace(branchState) ? branchState : "NULL");
		}

		public static IZType[] GetLocationFallbackCodes(BusinessObjectFactory factory, ILocation location, string currentCountryCode, string branchState)
		{
			return factory.GetCachedValue(GetLocationFallbackCodesCachedKey(location, currentCountryCode, branchState),
			delegate
			{
				List<IZType> locationCodes = new List<IZType>();

				if (location != null)
				{
					RefZoneHeader[] zones = location.Zones;
					if (zones != null && zones.Length > 0)
					{
						foreach (RefZoneHeader zone in zones)
						{
							if (zone.FZ_ZoneType == RefZoneHeaderLookups.ZoneTypeCodes.Tax)
							{
								locationCodes.Add(zone.Code);
							}
						}
					}
				}

				if (location?.IsLocationRule() ?? false)
				{
					locationCodes.Add(location.Code);
				}

				locationCodes.AddRange(GetCountryFallbackCodes(factory, location?.Country, currentCountryCode,
					!string.IsNullOrWhiteSpace(branchState) ? (location as OrgAddress)?.OA_State ?? location?.State?.RW_Code : ZString.Empty,
					branchState));

				return locationCodes.ToArray();
			});
		}

		static string GetCountryFallbackCodesCachedKey(RefCountry country, string currentCountryCode, string state, string branchState)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} : {1} : {2} : {3}",
				country?.Code.ToString() ?? "NULL",
				!string.IsNullOrWhiteSpace(currentCountryCode) ? currentCountryCode : "NULL",
				!string.IsNullOrWhiteSpace(state) ? state : "NULL",
				!string.IsNullOrWhiteSpace(branchState) ? branchState : "NULL");
		}

		public static IZType[] GetCountryFallbackCodes(BusinessObjectFactory factory, RefCountry country, string currentCountryCode, string state, string branchState)
		{
			return factory.GetCachedValue(GetCountryFallbackCodesCachedKey(country, currentCountryCode, state, branchState), delegate
			{
				List<IZType> countryOrRegionCodes = new List<IZType>();

				if (country != null)
				{
					countryOrRegionCodes.Add(country.Code);

					if (!country.RN_EconomicGrouping.IsEmpty)
					{
						if (country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion &&
							!string.IsNullOrEmpty(currentCountryCode) &&
							country.Code != currentCountryCode)
						{
							countryOrRegionCodes.Add(new ZString(AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry)); // EU Not My Country
						}
						countryOrRegionCodes.Add(country.RN_EconomicGrouping);
					}

					if (country.RN_EconomicGrouping != EconomicGroupList.Codes.EuropeanUnion)
					{
						countryOrRegionCodes.Add(new ZString(AccChargeTaxOverride.NotEuropeanUnion)); // Not EU
					}

					if (!string.IsNullOrWhiteSpace(currentCountryCode) && country.Code != currentCountryCode)
					{
						countryOrRegionCodes.Add(new ZString(AccChargeTaxOverride.AllCountriesExceptLoginCountry));
					}

					if (!string.IsNullOrWhiteSpace(currentCountryCode) && country.Code == currentCountryCode)
					{
						if (!string.IsNullOrWhiteSpace(state) && state == branchState)
						{
							countryOrRegionCodes.Add(new ZString(AccChargeTaxOverride.SameCountryAndStateAsLineBranch));
						}
						else
						{
							countryOrRegionCodes.Add(new ZString(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch));
						}
					}
				}

				countryOrRegionCodes.Add(new ZString(AccChargeTaxOverride.ALL));

				return countryOrRegionCodes.ToArray();
			});
		}

		public static IZType[] GetHomeCountryFallbacks(BusinessObjectFactory factory, string currentCountryCode, string branchState,
			OrgHeader org,
			ILocation fixedPlaceOfSupplyLocation)
		{
			var homeCountryFallbacks = new IZType[] { ZString.Empty };

			if ((AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.Value || AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value) &&
			fixedPlaceOfSupplyLocation != null)
			{
				var fixedPlaceOfSupplyFallBackCodes = new List<IZType>(GetLocationFallbackCodes(factory, fixedPlaceOfSupplyLocation, currentCountryCode, branchState));
				fixedPlaceOfSupplyFallBackCodes.AddRange(homeCountryFallbacks);
				homeCountryFallbacks = fixedPlaceOfSupplyFallBackCodes.ToArray();
			}
			else if (!AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value && org?.UNLOCO != null)
			{
				var locationFallbackCodes = new List<IZType>(GetLocationFallbackCodes(factory, org.UNLOCO, currentCountryCode, branchState));
				locationFallbackCodes.AddRange(homeCountryFallbacks);
				homeCountryFallbacks = locationFallbackCodes.ToArray();
			}
			else if (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value &&
				org?.MainAddress != null &&
				fixedPlaceOfSupplyLocation == null)
			{
				var locationFallbackCodes = new List<IZType>(GetLocationFallbackCodes(factory, org.MainAddress, currentCountryCode, branchState));
				locationFallbackCodes.AddRange(homeCountryFallbacks);
				homeCountryFallbacks = locationFallbackCodes.ToArray();
			}

			return homeCountryFallbacks;
		}

		public static IZType[] GetOrganisationHomeCountry(BusinessObjectFactory factory, string currentCountryCode, OrgHeader org = null, ILocation fixedPlaceOfSupplyLocation = null)
		{
			var homeCountryFallbacks = new IZType[] { ZString.Empty };
			var locationFallbackCodes = org != null
				? new List<IZType>(GetLocationFallbackCodes(factory, org.UNLOCO, currentCountryCode, string.Empty))
				: new List<IZType>(GetLocationFallbackCodes(factory, fixedPlaceOfSupplyLocation, currentCountryCode,
					string.Empty));
			locationFallbackCodes.AddRange(homeCountryFallbacks);
			homeCountryFallbacks = locationFallbackCodes.ToArray();
			return homeCountryFallbacks;
		}

		#region Resource Strings

		static string ALL
		{
			get { return Res.GetString("6c3f8d5a-d7f7-4d7f-9836-e5da3c3ece05", "All Countries/Regions"); }
		}

		static string EuropeanUnionExcludingLoginCountry
		{
			get { return Res.GetString("434feb28-9ec0-4c7b-a3bc-5a5daf9503ae", "European Union, Excluding Login Country/Region"); }
		}

		static string EUExcludingLoginCountry
		{
			get { return Res.GetString("58b850bc-c063-43c9-8e65-bb9b039a1594", "EU, Excluding Login Country/Region"); }
		}

		static string NotEuropeanUnion
		{
			get { return Res.GetString("717d7c47-f3d0-4dc0-841b-329af2f3c634", "Not European Union"); }
		}

		static string AllCountriesExceptLoginCountry
		{
			get { return Res.GetString("579c484e-4634-4e91-a7da-71e431917048", "All Countries/Regions, Excluding Login Country/Region"); }
		}

		static string OtherTerritories
		{
			get { return Res.GetString("f1a0b5d4-2c3e-4a8b-8c7f-6d9e0f3a2b1b", "Other Territories"); }
		}

		static string SameCountryAndStateAsLineBranch
		{
			get { return Res.GetString("61bf1273-cf2d-42ba-9425-47ddf17c1803", "Same Country/Region and State as Line Branch"); }
		}

		static string SameCountryDifferentStateAsLineBranch
		{
			get { return Res.GetString("6853d39c-abb5-49da-862e-deff071902da", "Same Country/Region, different State as Line Branch"); }
		}

		#endregion
	}
}
