using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Extensions;

namespace Enterprise.Customs.Business
{
	public class EuropeanUnionCustomsMembersProvider : Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider
	{
		public EuropeanUnionCustomsMembersProvider()
		{
			var factory = new ReadOnlyBusinessObjectFactory() { NameForDebugging = "European Union Customs Members Provider" };
			europeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers = factory.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().ToArray();
			europeanUnionForCustomsMembers = factory.GetEuropeanUnionForCustomsMembers();
			europeanUnionAndCtCountries = factory.GetEuropeanUnionAndCtCountries().ToArray();
			countriesInEuropeanCustomsUnionOrInheritsFromEU = europeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers.Union(new[] { Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.Turkey }).Distinct().ToHashSet();
		}

		readonly IEnumerable<string> europeanUnionForCustomsMembers;
		readonly string[] europeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers;
		readonly string[] europeanUnionAndCtCountries;
		readonly HashSet<string> countriesInEuropeanCustomsUnionOrInheritsFromEU;

		public string[] GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers() => europeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers;

		public string[] GetEuropeanUnionAndCtCountries() => europeanUnionAndCtCountries;
		public IEnumerable<string> GetCountriesInEuropeanCustomsUnionOrInheritsFromEU() => countriesInEuropeanCustomsUnionOrInheritsFromEU;

		public bool IsMemberOfEU(string countryCode)
		{
			return europeanUnionForCustomsMembers.Contains(countryCode);
		}

		public bool IsInEuropeanCustomsUnion(string countryCode)
		{
			return GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().Contains(countryCode);
		}

		public bool IsCountryEuOrCtCountry(string countryCode)
		{
			return GetEuropeanUnionAndCtCountries().Contains(countryCode);
		}

		public bool IsInEuropeanCustomsUnionOrInheritsFromEU(string countryCode) => countriesInEuropeanCustomsUnionOrInheritsFromEU.Contains(countryCode);
	}
}
