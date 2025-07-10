using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccountingTaxLocationsTest : TestCaseWithFactory
	{
		public void TestGetLocations()
		{
			var countriesCollection = new RefCountryCollection(Factory);
			var economicGroupList = new EconomicGroupList();

			var locations = AccountingTaxLocations.GetLocations(Factory);
			AssertEquals("The location list should contain all countries codes, Europien Union = EUN, ALL and tax Zones (at least one default QUBC zone)", countriesCollection.Count + 14, locations.Count);
			AssertEquals("The location list should contain all countries codes, all economic group codes, ALL and tax Zones (at least one default QUBC zone)", countriesCollection.Count + economicGroupList.Count + 10, locations.Count);

			var newTaxZone = Factory.New<RefZoneHeader>();
			newTaxZone.FZ_Code = "TAXZ";
			newTaxZone.FZ_Description = "Tax Zone";
			newTaxZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			var newRatZone = Factory.New<RefZoneHeader>();
			newRatZone.FZ_Code = "RATZ";
			newRatZone.FZ_Description = "Rat Zone";
			newRatZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;

			Factory.Save();

			locations = AccountingTaxLocations.GetLocations(Factory);
			int expectedLocationCount = countriesCollection.Count + economicGroupList.Count + 11;
			AssertEquals("Unexpected number of locations.", expectedLocationCount, locations.Count);

			// Validate expected tax zone codes in sequence
			string[] expectedCodes = { "BCTZ", "HSTC", "ITBL", "ONTZ", "QUBC", "TAXZ" };
			for (int i = 0; i < expectedCodes.Length; i++)
			{
				int index = economicGroupList.Count + 5 + i;
				AssertEquals($"Expected {expectedCodes[i]} at position {index}.", expectedCodes[i], locations[index].Code);
			}

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			locations = AccountingTaxLocations.GetLocations(Factory);
			AssertEquals(false, locations.ContainsCode(AccChargeTaxOverride.SameCountryAndStateAsLineBranch));
			AssertEquals(false, locations.ContainsCode(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch));

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			locations = AccountingTaxLocations.GetLocations(Factory);
			AssertEquals(true, locations.ContainsCode(AccChargeTaxOverride.SameCountryAndStateAsLineBranch));
			AssertEquals(true, locations.ContainsCode(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch));

			locations = AccountingTaxLocations.GetLocations(Factory, false);
			AssertEquals(false, locations.ContainsCode(AccChargeTaxOverride.SameCountryAndStateAsLineBranch));
			AssertEquals(false, locations.ContainsCode(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch));
		}

		public void TestGetTaxRegistrationLocations()
		{
			var sortedTaxCodes = new ZString[] { "ALL", "ASE", "NAF", "EUN", "BLN", "EUX", "SEZ" };
			var sortedCountryCodes = new RefCountryCollection(Factory).Select(c => c.RN_Code).OrderBy(c => c).ToArray();
			var expectedCodes = new ZStringBuilder(sortedTaxCodes);
			foreach (var countryCode in sortedCountryCodes)
			{
				expectedCodes.Append(countryCode);
			}

			var actual = AccountingTaxLocations.GetTaxRegistrationLocations(Factory);
			AssertEquals(expectedCodes.ToStringWithDelimiterBetweenAppends(", "), actual.CodesAsString);

			var taxZone = Factory.New<RefZoneHeader>();
			taxZone.FZ_Code = "BAD1";
			taxZone.FZ_Description = "Tax Zone";
			taxZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			var ratingZone = Factory.New<RefZoneHeader>();
			ratingZone.FZ_Code = "BAD2";
			ratingZone.FZ_Description = "Rating Zone";
			ratingZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;

			Factory.Save();

			actual = AccountingTaxLocations.GetTaxRegistrationLocations(Factory);
			AssertEquals("Tax Zones are not valid for Tax Registration Codes", false, actual.ContainsCode("BAD1"));
			AssertEquals("Rating Zones are not valid for Tax Registration Codes", false, actual.ContainsCode("BAD2"));
			AssertEquals("List should still be the same", expectedCodes.ToStringWithDelimiterBetweenAppends(", "), actual.CodesAsString);
		}

		public void TestGetCountryFallbackCodes()
		{
			var fallbackForNull = AccountingTaxLocations.GetCountryFallbackCodes(Factory, null, Env.CurrentCompany.Country.Code, "", "");
			AssertNotNull(fallbackForNull);
			AssertEquals(1, fallbackForNull.Length);
			AssertEquals("ALL", fallbackForNull[0]);

			var countriesWithoutEconomicGrouping = Factory.Load<RefCountry>(new ZQuery(RefCountrySchema.RN_EconomicGrouping, string.Empty));
			foreach (var country in countriesWithoutEconomicGrouping)
			{
				var fallback = AccountingTaxLocations.GetCountryFallbackCodes(Factory, country, Env.CurrentCompany.Country.Code, "", "");

				if (country.RN_Code == Env.CurrentCompany.Country.Code)
				{
					AssertNotNull(fallback);
					AssertEquals(4, fallback.Length);
					AssertEquals(country.RN_Code, fallback[0]);
					AssertEquals(AccChargeTaxOverride.NotEuropeanUnion, fallback[1]);
					AssertEquals(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch, fallback[2]);
					AssertEquals(AccChargeTaxOverride.ALL, fallback[3]);
				}
				else
				{
					AssertNotNull(fallback);
					AssertEquals(4, fallback.Length);
					AssertEquals(country.RN_Code, fallback[0]);
					AssertEquals(AccChargeTaxOverride.NotEuropeanUnion, fallback[1]);
					AssertEquals(AccChargeTaxOverride.AllCountriesExceptLoginCountry, fallback[2]);
					AssertEquals(AccChargeTaxOverride.ALL, fallback[3]);
				}
			}

			var france = RefCountry.LoadFromCountryCode(Factory, "FR");
			var italy = RefCountry.LoadFromCountryCode(Factory, "IT");
			AssertEquals(EconomicGroupList.Codes.EuropeanUnion, france.RN_EconomicGrouping);
			AssertEquals(EconomicGroupList.Codes.EuropeanUnion, italy.RN_EconomicGrouping);

			var fallbackEU = AccountingTaxLocations.GetCountryFallbackCodes(Factory, france, "FR", "", "");
			AssertNotNull(fallbackEU);
			AssertEquals(4, fallbackEU.Length);
			AssertEquals("FR", fallbackEU[0]);
			AssertEquals(EconomicGroupList.Codes.EuropeanUnion, fallbackEU[1]);
			AssertEquals(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch, fallbackEU[2]);
			AssertEquals(AccChargeTaxOverride.ALL, fallbackEU[3]);

			fallbackEU = AccountingTaxLocations.GetCountryFallbackCodes(Factory, france, "IT", "", "");
			AssertNotNull(fallbackEU);
			AssertEquals(5, fallbackEU.Length);
			AssertEquals("FR", fallbackEU[0]);
			AssertEquals(AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry, fallbackEU[1]);
			AssertEquals(EconomicGroupList.Codes.EuropeanUnion, fallbackEU[2]);
			AssertEquals(AccChargeTaxOverride.AllCountriesExceptLoginCountry, fallbackEU[3]);
			AssertEquals(AccChargeTaxOverride.ALL, fallbackEU[4]);

			var newCountry = Factory.NewWithValidTestData<RefCountry>();
			newCountry.RN_Code = "--";
			newCountry.RN_EconomicGrouping = "***";

			var fallbackNew = AccountingTaxLocations.GetCountryFallbackCodes(Factory, newCountry, "IT", "", "");
			AssertNotNull(fallbackNew);
			AssertEquals(5, fallbackNew.Length);
			AssertEquals("--", fallbackNew[0]);
			AssertEquals("***", fallbackNew[1]);
			AssertEquals(AccChargeTaxOverride.NotEuropeanUnion, fallbackNew[2]);
			AssertEquals(AccChargeTaxOverride.AllCountriesExceptLoginCountry, fallbackNew[3]);
			AssertEquals(AccChargeTaxOverride.ALL, fallbackNew[4]);
		}

		public void TestGetCountryFallbackCodes_State()
		{
			var refCountry = RefCountry.LoadFromCountryCode(Factory, "AU");
			var refCountryStates = Factory.New<RefCountryStates>();
			refCountryStates.RW_Code = "AAA";

			var fallback = AccountingTaxLocations.GetCountryFallbackCodes(Factory, refCountry, "CN", refCountryStates.RW_Code, "");
			AssertEquals(4, fallback.Length);
			AssertEquals("AU", fallback[0]);
			AssertEquals(AccChargeTaxOverride.NotEuropeanUnion, fallback[1]);
			AssertEquals(AccChargeTaxOverride.AllCountriesExceptLoginCountry, fallback[2]);
			AssertEquals(AccChargeTaxOverride.ALL, fallback[3]);

			fallback = AccountingTaxLocations.GetCountryFallbackCodes(Factory, refCountry, "CN", refCountryStates.RW_Code, "AAA");
			AssertEquals(4, fallback.Length);
			AssertEquals("AU", fallback[0]);
			AssertEquals(AccChargeTaxOverride.NotEuropeanUnion, fallback[1]);
			AssertEquals(AccChargeTaxOverride.AllCountriesExceptLoginCountry, fallback[2]);
			AssertEquals(AccChargeTaxOverride.ALL, fallback[3]);

			fallback = AccountingTaxLocations.GetCountryFallbackCodes(Factory, refCountry, "AU", refCountryStates.RW_Code, "");
			AssertEquals(4, fallback.Length);
			AssertEquals("AU", fallback[0]);
			AssertEquals(AccChargeTaxOverride.NotEuropeanUnion, fallback[1]);
			AssertEquals(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch, fallback[2]);
			AssertEquals(AccChargeTaxOverride.ALL, fallback[3]);

			fallback = AccountingTaxLocations.GetCountryFallbackCodes(Factory, refCountry, "AU", refCountryStates.RW_Code, "AAA");
			AssertEquals(4, fallback.Length);
			AssertEquals("AU", fallback[0]);
			AssertEquals(AccChargeTaxOverride.NotEuropeanUnion, fallback[1]);
			AssertEquals(AccChargeTaxOverride.SameCountryAndStateAsLineBranch, fallback[2]);
			AssertEquals(AccChargeTaxOverride.ALL, fallback[3]);
		}

		public void TestGetLocationFallbackCodesCacheForTaxZone()
		{
			var auUnlocoA = CreateUNLOCOForTest("TEST1", Core.Constants.CountryCodes.Australia);
			var auUnlocoB = CreateUNLOCOForTest("TEST2", Core.Constants.CountryCodes.Australia);
			var nzUnloco = CreateUNLOCOForTest("TEST3", Core.Constants.CountryCodes.NewZealand);

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Code = "Address1";
			address1.OA_RL_NKRelatedPortCode = auUnlocoA.Code;
			address1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			address1.OA_State = "NSW";

			var address2 = Factory.NewWithValidTestData<OrgAddress>();
			address2.OA_Code = "Address1";
			address2.OA_RL_NKRelatedPortCode = auUnlocoB.Code;
			address2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			address2.OA_State = "NSW";

			var address3 = Factory.NewWithValidTestData<OrgAddress>();
			address3.OA_RL_NKRelatedPortCode = nzUnloco.Code;
			address3.OA_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;

			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_RN_NKCountryCode = "AU";

			var taxZone = Factory.New<RefZoneHeader>();
			taxZone.FZ_Code = "TAXA";
			taxZone.FZ_Description = "Tax Zone A";
			taxZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			var portZonePivotA = Factory.New<RefZonePivot>();
			portZonePivotA.F2_FZ = taxZone.PK;
			portZonePivotA.F2_ParentID = auUnlocoA.PK;
			portZonePivotA.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var portZonePivotB = Factory.New<RefZonePivot>();
			portZonePivotB.F2_FZ = taxZone.PK;
			portZonePivotB.F2_ParentID = auUnlocoB.PK;
			portZonePivotB.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			var taxZoneNZ = Factory.New<RefZoneHeader>();
			taxZoneNZ.FZ_Code = "TXNZ";
			taxZoneNZ.FZ_Description = "Tax Zone NZ";
			taxZoneNZ.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			var portZonePivotNZ = Factory.New<RefZonePivot>();
			portZonePivotNZ.F2_FZ = taxZoneNZ.PK;
			portZonePivotNZ.F2_ParentID = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.NewZealand).PK;
			portZonePivotNZ.F2_ParentTableCode = RefCountrySchema.Constants.Prefix;

			Factory.Save();

			var result = new List<IZType>();
			result.Add(new ZString("TAXA"));
			result.Add(new ZString("AU"));
			result.Add(new ZString("NEU"));
			result.Add(new ZString("ALL"));
			Factory.ResetDatabaseLoadCount();
			AssertResultAndDbHitCount("Should be loaded from database: UNLOCOA", auUnlocoA, result, false);

			AssertResultAndDbHitCount("Should be loaded from cache: UNLOCOA", address1, result, true);

			Factory.ResetDatabaseLoadCount();
			AssertResultAndDbHitCount("Should be loaded from database: UNLOCOB", address2, result, false);//this should guard us incase we start to populate ILocation.Code in OrgAddress

			result = new List<IZType>();
			result.Add(new ZString("TXNZ"));
			result.Add(new ZString("NZ"));
			result.Add(new ZString("NEU"));
			result.Add(new ZString("ALL"));
			Factory.ResetDatabaseLoadCount();
			AssertResultAndDbHitCount("Should be loaded from database: NZUNLOCO", address3, result, false);

			result = new List<IZType>();
			result.Add(new ZString("AU"));
			result.Add(new ZString("NEU"));
			result.Add(new ZString("ALL"));
			Factory.ResetDatabaseLoadCount();
			AssertResultAndDbHitCount("Should not have a db hit as jobDocAddress does not have real address so will have no zones", jobDocAddress, result, true);

			jobDocAddress.E2_AddressOverride = false;
			jobDocAddress.E2_OA_Address = address1.PK;

			result = new List<IZType>();
			result.Add(new ZString("TAXA"));
			result.Add(new ZString("AU"));
			result.Add(new ZString("NEU"));
			result.Add(new ZString("ALL"));
			Factory.ResetDatabaseLoadCount();
			AssertResultAndDbHitCount("Should not have a db hit as Address1 is already in cache", jobDocAddress, result, true);
		}

		void AssertResultAndDbHitCount(string message, ILocation location, List<IZType> result, bool noDBHit)
		{
			var dbHitCount_Before = Factory.DatabaseLoadCount;
			var locations = AccountingTaxLocations.GetLocationFallbackCodes(Factory, location, "", "");
			AssertArrayEqualsByElements(result.ToArray(), locations.ToArray());

			if (noDBHit)
			{
				AssertEquals(message, dbHitCount_Before, Factory.DatabaseLoadCount);
			}
			else
			{
				AssertNotEquals(message, dbHitCount_Before, Factory.DatabaseLoadCount);
			}
		}

		RefUNLOCO CreateUNLOCOForTest(ZString uNLOCOCode, ZString countryCode)
		{
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_Code = uNLOCOCode;

			RefCountry country = RefCountry.LoadFromCountryCode(Factory, countryCode);
			uNLOCO.RL_RN_NKCountryCode = country.Code;

			return uNLOCO;
		}

		public void TestGetLocationFallBackCodesDoesNotIncludeNonTaxZones()
		{
			var italy = RefCountry.LoadFromCountryCode(Factory, "IT");
			var location = (ILocation)italy;
			var locationCodes = AccountingTaxLocations.GetLocationFallbackCodes(Factory, location, "", "");
			foreach (var code in locationCodes)
			{
				var zone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, code));
				if (zone != null)
				{
					AssertEquals(string.Format("Non Tax Zones should not be in this list. '{0}' is a non tax zone", code), RefZoneHeaderLookups.ZoneTypeCodes.Tax, zone.FZ_ZoneType);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestGetBranchState()
		{
			var branch = Factory.New<GlbBranch>();
			var company = Factory.New<GlbCompany>();
			var branchOrgProxy = Factory.New<OrgHeader>();
			branchOrgProxy.Addresses.RemoveAll();
			var companyOrgProxy = Factory.New<OrgHeader>();
			companyOrgProxy.Addresses.RemoveAll();

			branch.GB_OH_OrgProxy = branchOrgProxy.PK;
			branch.GB_GC = company.PK;
			company.GC_OH_OrgProxy = companyOrgProxy.PK;

			var branchAddress1 = branchOrgProxy.Addresses[0];
			var branchAddress2 = branchOrgProxy.Addresses.AddNew();

			var companyAddress = companyOrgProxy.Addresses[0];

			branchAddress1.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			branchAddress1.State = "AAA";
			branchAddress2.State = "BBB";

			companyAddress.State = "";

			AssertEquals("Main office's state of the branch org proxy", "AAA", AccountingTaxLocations.GetBranchState(branch));

			branchAddress2.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Office);
			Assert(!branchAddress1.AddressCapability.GetIsMainAddress(OrgConstants.AddressType.Office));
			AssertEquals("Main office's state of the branch org proxy", "BBB", AccountingTaxLocations.GetBranchState(branch));

			branch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("No main office in the branch org proxy", "", AccountingTaxLocations.GetBranchState(branch));

			companyAddress.State = "CCC";
			AssertEquals("Main office's state of the company org proxy", "CCC", AccountingTaxLocations.GetBranchState(branch));
		}

		public void TestGetLocationFallBackCodesForOrgAddressState()
		{
			var auUnlocoA = CreateUNLOCOForTest("TEST1", Core.Constants.CountryCodes.Australia);
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();

			AssertEquals("Predition Address Count", 1, testOrg1.Addresses.Count);
			testOrg1.Addresses[0].OA_RL_NKRelatedPortCode = auUnlocoA.Code;
			testOrg1.Addresses[0].OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			testOrg1.Addresses[0].State = "T1";

			var testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Predition Address Count", 1, testOrg2.Addresses.Count);
			testOrg2.Addresses[0].OA_RL_NKRelatedPortCode = auUnlocoA.Code;
			testOrg2.Addresses[0].OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			testOrg2.Addresses[0].State = "T2";

			var testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Predition Address Count", 1, testOrg3.Addresses.Count);
			testOrg3.Addresses[0].OA_RL_NKRelatedPortCode = auUnlocoA.Code;
			testOrg3.Addresses[0].OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			testOrg3.Addresses[0].State = "T1";

			AssertNotEquals("Predition", testOrg2.Addresses[0].State, testOrg1.Addresses[0].State);
			AssertEquals("Predition", testOrg3.Addresses[0].State, testOrg1.Addresses[0].State);

			var locationCodes1 = AccountingTaxLocations.GetLocationFallbackCodes(Factory, testOrg1.Addresses[0], "", "");
			var locationCodes2 = AccountingTaxLocations.GetLocationFallbackCodes(Factory, testOrg2.Addresses[0], "", "");
			var locationCodes3 = AccountingTaxLocations.GetLocationFallbackCodes(Factory, testOrg3.Addresses[0], "", "");

			AssertNotEquals(locationCodes2, locationCodes1);
			AssertEquals(locationCodes3, locationCodes1);

			testOrg3.Addresses[0].State = "T3";
			AssertNotEquals("Predition", testOrg3.Addresses[0].State, testOrg1.Addresses[0].State);
			locationCodes3 = AccountingTaxLocations.GetLocationFallbackCodes(Factory, testOrg3.Addresses[0], "", "");
			AssertNotEquals(locationCodes3, locationCodes1);
		}

		public void TestHomeCountryFallBackCodes()
		{
			var unloco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "INDEL");
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.Addresses[0].OA_RL_NKRelatedPortCode = unloco.Code;
			testOrg1.Addresses[0].OA_RN_NKCountryCode = Core.Constants.CountryCodes.India;
			testOrg1.Addresses[0].State = "DL";

			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					var fixedPlaceOfSupply = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, "DL");
					var locationFallBackCodes = AccountingTaxLocations.GetHomeCountryFallbacks(Factory, Core.Constants.CountryCodes.India, "DL", testOrg1, fixedPlaceOfSupply);
					AssertFallbackCodeCollection(new ZString[] { "IN", "NEU", "BST", "ALL", "" }, locationFallBackCodes);

					fixedPlaceOfSupply = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, "KL");
					locationFallBackCodes = AccountingTaxLocations.GetHomeCountryFallbacks(Factory, Core.Constants.CountryCodes.India, "DL", testOrg1, fixedPlaceOfSupply);
					AssertFallbackCodeCollection(new ZString[] { "IN", "NEU", "BSX", "ALL", "" }, locationFallBackCodes);

					fixedPlaceOfSupply = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, "ALX");
					locationFallBackCodes = AccountingTaxLocations.GetHomeCountryFallbacks(Factory, Core.Constants.CountryCodes.India, "DL", testOrg1, fixedPlaceOfSupply);
					AssertFallbackCodeCollection(new ZString[] { "ALX", "ALL", "" }, locationFallBackCodes);

					locationFallBackCodes = AccountingTaxLocations.GetHomeCountryFallbacks(Factory, Core.Constants.CountryCodes.India, "DL", testOrg1, null);
					AssertFallbackCodeCollection(new ZString[] { "IN", "NEU", "BST", "ALL", "" }, locationFallBackCodes);
				}

				using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					var fixedPlaceOfSupply = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, "DL");
					var locationFallBackCodes = AccountingTaxLocations.GetHomeCountryFallbacks(Factory, Core.Constants.CountryCodes.India, "DL", testOrg1, fixedPlaceOfSupply);
					AssertFallbackCodeCollection(new ZString[] { "IN", "NEU", "BST", "ALL", "" }, locationFallBackCodes);
				}

				using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
				{
					var fixedPlaceOfSupply = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, "DL");
					var locationFallBackCodes = AccountingTaxLocations.GetHomeCountryFallbacks(Factory, Core.Constants.CountryCodes.India, "DL", testOrg1, fixedPlaceOfSupply);
					AssertFallbackCodeCollection(new ZString[] { "IN", "NEU", "BST", "ALL", "" }, locationFallBackCodes);
				}
			}

			void AssertFallbackCodeCollection(ZString[] expected, IZType[] actual)
			{
				AssertEquals(expected?.Length ?? 0, actual?.Length ?? 0);
				for (int i = 0; i < (expected?.Length ?? 0); i++)
				{
					AssertEquals(expected[i], (ZString)actual[i]);
				}
			}
		}
	}
}
