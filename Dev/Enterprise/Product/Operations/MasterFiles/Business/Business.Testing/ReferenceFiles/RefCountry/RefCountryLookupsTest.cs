using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefCountryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNonCachedEconomicGroupList()
		{
			CodeDescriptionPairList list1 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).ListOfEconomicGroups;
			CodeDescriptionPairList list2 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).ListOfEconomicGroups;
			Assert("Lists should be different", list1 != list2);
		}

		public void TestNonCachedPostCodeValidationRules()
		{
			CodeDescriptionPairList list1 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).PostCodeValidationRules;
			CodeDescriptionPairList list2 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).PostCodeValidationRules;
			Assert("Lists should be different", list1 != list2);
		}
		public void TestPostCodeValidationRulesForCountryWithFormattingRules()
		{
			var australia = RefCountry.LoadFromCountryCode(Factory, "AU");
			var lookups = new RefCountryLookups(australia);
			var ruleList = lookups.PostCodeValidationRules;
			var provider = ObjectFactory.Get<IPostcodeFormattingRulesProvider>();
			var ausFormat = provider.GetRuleFromIso(australia.Code).Format;
			var codes = ruleList.GetAllCodes();
			var descriptions = codes.Select(x => ruleList.GetDescriptionFromCode(x)).ToArray();

			AssertEquals("Validation rules count", 3, ruleList.Count);
			AssertArrayEqualsByElements(
				"Validation codes contents and order",
				new string[] { "NVR", "MBE", "MBF" },
				codes);
			AssertArrayEqualsByElements(
				"Validation descriptions contents and order",
				new string[] { "No Validation Rule", "Must Be Entered", "Must Be Formatted: " + ausFormat },
				descriptions);
		}

		public void TestPostCodeValidationRulesForCountryWithNoFormattingRules()
		{
			var fakeCountry = Factory.NewWithValidTestData<RefCountry>();
			fakeCountry.RN_Code = "XX";
			var lookups = new RefCountryLookups(fakeCountry);
			var ruleList = lookups.PostCodeValidationRules;
			var codes = ruleList.GetAllCodes();
			var descriptions = codes.Select(x => ruleList.GetDescriptionFromCode(x)).ToArray();

			AssertEquals("Validation rules count", 2, ruleList.Count);
			AssertArrayEqualsByElements(
				"Validation codes contents and order",
				new string[] { "NVR", "MBE" },
				codes);
			AssertArrayEqualsByElements(
				"Validation descriptions contents and order",
				new string[] { "No Validation Rule", "Must Be Entered" },
				descriptions);
		}

		public void TestNonCachedStateAndProvinceValidationRules()
		{
			CodeDescriptionPairList list1 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).StateAndProvinceValidationRules;
			CodeDescriptionPairList list2 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).StateAndProvinceValidationRules;
			Assert("Lists should be different", list1 != list2);
		}

		public void TestNonCachedCountryAddressFormattingRules()
		{
			CodeDescriptionPairList list1 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).CountryAddressFormattingRules;
			CodeDescriptionPairList list2 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).CountryAddressFormattingRules;
			Assert("Lists should be different", list1 != list2);
		}

		public void TestNonCachedExternalAddressValidationRules()
		{
			CodeDescriptionPairList list1 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).ExternalAddressValidationRules;
			CodeDescriptionPairList list2 = new RefCountryLookups(GlbCompany.CurrentCompany.Country).ExternalAddressValidationRules;
			Assert("Lists should be different", list1 != list2);
		}
	}
}
