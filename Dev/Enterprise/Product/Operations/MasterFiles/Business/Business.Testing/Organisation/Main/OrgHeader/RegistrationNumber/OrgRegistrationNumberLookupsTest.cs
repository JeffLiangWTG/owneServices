using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRegistrationNumberLookupsTest : TestCaseWithFactory
	{
		public void TestNumberTypes()
		{
			OrgRegistrationNumber regNo = new OrgRegistrationNumber(Factory.New<OrgHeader>());
			regNo.Organization.OH_RL_NKClosestPort = "AUSYD";
			OrgRegistrationNumberTypeList initialList = regNo.Lookups.NumberTypes;
			AssertNotNull("NumberTypes", initialList);
			AssertEquals("NumberTypes should be cached.", initialList, regNo.Lookups.NumberTypes);
			regNo.Organization.OH_RL_NKClosestPort = "UAIEV";
			AssertNotEquals("NumberTypes should be recreated if the port changes.", initialList, regNo.Lookups.NumberTypes);
			AssertNotEquals("UNLOCO: UAIEV", 0, regNo.Lookups.NumberTypes.GetAllCodes().Where(x => x.StartsWith("UA:")).Count());

			regNo.Organization.MainAddress.OA_RN_NKCountryCode = "MY";
			AssertNotEquals("Country: MY", 0, regNo.Lookups.NumberTypes.GetAllCodes().Where(x => x.StartsWith("MY:")).Count());

			regNo.Organization.OH_RL_NKClosestPort = "JPTYO";
			AssertNotEquals("UNLOCO: JPTYO", 0, regNo.Lookups.NumberTypes.GetAllCodes().Where(x => x.StartsWith("JP:")).Count());

			regNo.Organization.MainAddress.OA_RN_NKCountryCode = "SG";
			AssertNotEquals("Country: SG", 0, regNo.Lookups.NumberTypes.GetAllCodes().Where(x => x.StartsWith("SG:")).Count());
		}

		public void TestNumberTypesFromFactoryCache()
		{
			OrgRegistrationNumber regNo = new OrgRegistrationNumber(Factory.New<OrgHeader>());
			regNo.Organization.OH_RL_NKClosestPort = "AUSYD";
			OrgRegistrationNumberTypeList initialList = regNo.Lookups.NumberTypes;
			AssertNotNull("NumberTypes", initialList);
			var key = FormattableString.Invariant($"OrgRegistrationNumberTypeList_{regNo.Organization.Country.Code}_{regNo.Organization?.MainAddress?.Country.Code}");
			var cachedValue = Factory.GetCachedValue(key, () => new OrgRegistrationNumberTypeList(new[] { regNo.Organization.Country, regNo.Organization?.MainAddress?.Country }));
			AssertEquals("NumberTypes should be cached in Factory.", initialList, cachedValue);
			regNo.Organization.OH_RL_NKClosestPort = "UAIEV";
			OrgRegistrationNumberTypeList newList = regNo.Lookups.NumberTypes;
			AssertNotNull("NumberTypes", newList);
			key = FormattableString.Invariant($"OrgRegistrationNumberTypeList_{regNo.Organization.Country.Code}_{regNo.Organization?.MainAddress?.Country.Code}");
			cachedValue = Factory.GetCachedValue(key, () => new OrgRegistrationNumberTypeList(new[] { regNo.Organization.Country, regNo.Organization?.MainAddress?.Country }));
			AssertEquals("NumberTypes should be cached in Factory.", newList, cachedValue);
			AssertNotEquals("Cache not updated", newList, initialList);
		}
	}
}
