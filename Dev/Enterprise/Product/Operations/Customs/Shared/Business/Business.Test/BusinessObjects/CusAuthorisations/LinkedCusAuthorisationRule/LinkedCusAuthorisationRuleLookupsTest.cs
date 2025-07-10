using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	public class LinkedCusAuthorisationRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRuleCodeList()
		{
			var ruleCodeList = lookups.RuleCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("List", "CUS", ruleCodeList.CodesAsString);
				AssertSame("Cached", ruleCodeList, lookups.RuleCodeList);
			});
		}

		public void TestValueList_RuleCodeUnknown()
		{
			CombineAssertions(() =>
			{
				AssertType<CodeDescriptionPairList>("Type", lookups.ValueList);
				AssertEquals("Empty RuleCode: count", 0, lookups.ValueList.Count);
				lookups.Parent.CPR_RuleCode = "ABC";
				AssertEquals("Unknown RuleCode: count", 0, lookups.ValueList.Count);
			});
		}

		public void TestValueList_RuleCodeCUS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ITR", "Rome Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ITM", "Milan Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IN1", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "IN2", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IN3", "Invalid StartDate", ZDateTime.Today.AddDays(1), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IN4", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			Factory.Save();

			lookups.Parent.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
			var valueList = (ZZRefCusCodeListCombinedCollection)lookups.ValueList;
			valueList.Load();
			AssertContainsExactElementsInAnyOrder("List", new[] { "ITM", "ITR" }, valueList.Select(x => x.ZZD_Code));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var linkedCusAuthorisationRule = Factory.NewWithValidTestData<LinkedCusAuthorisationRule>();
			linkedCusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			lookups = new LinkedCusAuthorisationRuleLookups(linkedCusAuthorisationRule);
		}
		LinkedCusAuthorisationRuleLookups lookups;
	}
}
