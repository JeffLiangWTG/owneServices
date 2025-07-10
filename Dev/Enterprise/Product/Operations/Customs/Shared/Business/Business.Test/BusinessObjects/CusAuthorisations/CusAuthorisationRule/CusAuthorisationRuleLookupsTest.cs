using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAuthorisationRuleLookups))]
	sealed class CusAuthorisationRuleLookupsTest : CusAuthorisationRuleLookupsAbstractTest<CusAuthorisationRuleLookups>
	{
		public void TestValueList_UnknownRuleCode()
		{
			CombineAssertions(() =>
			{
				var lookups = CusAuthorisationRuleLookupsForTesting();
				AssertType<CodeDescriptionPairList>("Empty RuleCode: type", lookups.ValueList);
				AssertEquals("Empty RuleCode: count", 0, lookups.ValueList.Count);
				lookups.Parent.CPR_RuleCode = "ABC";
				AssertType<CodeDescriptionPairList>("Empty RuleCode: type", lookups.ValueList);
				AssertEquals("Unknown RuleCode: count", 0, lookups.ValueList.Count);
			});
		}

		public void TestValueList_LOC()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "DEFAC", "DE Facility", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var lookups = CusAuthorisationRuleLookupsForTesting();
				var cusAuthorisationRule = lookups.Parent;
				cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertType<ZZRefCusCodeListCombinedCollection>("LOC'-Lookup-Type", lookups.ValueList);

				cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				var valueListLOC = lookups.ValueList as ZZRefCusCodeListCombinedCollection;
				valueListLOC.Load();
				AssertContainsExactElementsInAnyOrder("German Codes", new[] { "DEFAC" }, valueListLOC.Select(x => x.ZZD_Code));

				cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
				valueListLOC = lookups.ValueList as ZZRefCusCodeListCombinedCollection;
				valueListLOC.Load();
				AssertEquals("No Latvian Codes", false, valueListLOC.Any());
			});
		}

		public void TestDescriptionList()
		{
			CombineAssertions(() =>
			{
				var codeList = new CodeDescriptionPairList();
				var lookups = CusAuthorisationRuleLookupsForTesting();
				AssertEquals("Empty RuleCode: type", 0, lookups.DescriptionList.Count);
				lookups.Parent.CPR_RuleCode = "ABC";
				AssertEquals("Empty RuleCode: type", 0, lookups.DescriptionList.Count);
				lookups.Parent.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertEquals("Empty RuleCode: type", 0, lookups.DescriptionList.Count);
			});
		}

		protected override CusAuthorisationRuleLookups CusAuthorisationRuleLookupsForTesting()
		{
			var cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			return new CusAuthorisationRuleLookups(cusAuthorisationRule);
		}
	}
}
