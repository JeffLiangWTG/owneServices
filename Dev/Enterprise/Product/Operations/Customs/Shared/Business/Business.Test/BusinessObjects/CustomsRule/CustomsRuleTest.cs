using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CustomsRule))]
	public class CustomsRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableNameAndShortName()
		{
			var rule = Factory.New<CustomsRule>();
			AssertEquals("ShortName", "Rule", rule.ShortName);
			AssertEquals("Rule  - ", rule.HumanReadableName);

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "PMT";
			rule.CPH_OH_PermitHolder = holder.PK;
			AssertEquals("Rule PMT - ", rule.HumanReadableName);

			rule.CPH_PermitDescription = "TEST";
			AssertEquals("Rule PMT - TEST", rule.HumanReadableName);
		}

		public void TestRules()
		{
			var customsRule = Factory.New<CustomsRule>();
			var rules = customsRule.Rules;
			AssertEquals("Rules", typeof(CustomsRuleRuleCollection), rules.GetType());
		}

		public void TestDefaultValues()
		{
			var customsRule = Factory.New<CustomsRule>();
			AssertEquals("CPH_ApplicationCode", CusPermitHeaderApplicationCodeList.Codes.Rule, customsRule.CPH_ApplicationCode);
			AssertEquals("CPH_Type", CustomsRule.CustomsRuleType, customsRule.CPH_Type);
			AssertEquals("CPH_GC_Company", GlbCompany.CurrentCompany.PK, customsRule.CPH_GC_Company);
			AssertEquals("CPH_RN_NKCountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, customsRule.CPH_RN_NKCountryCode);
		}

		public void TestGetNewValidation()
		{
			var validation = Factory.New<CustomsRule>().Validation;
			AssertEquals(typeof(CustomsRuleValidation), validation.GetType());
		}

		public void TestOrganizationDescription()
		{
			var customsRule = Factory.New<CustomsRule>();
			AssertEquals("Apply to All", customsRule.OrganizationDescription);

			var organization = Factory.NewWithValidTestData<OrgHeader>();
			organization.OH_FullName = "TEST ABCD";
			customsRule.CPH_OH_PermitHolder = organization.PK;
			AssertEquals("TEST ABCD", customsRule.OrganizationDescription);

			customsRule.CPH_OH_PermitHolder = ZGuid.Invalid;
			AssertEquals(Core.Constants.FindBoxMessages.InvalidSelection, customsRule.OrganizationDescription);

			customsRule.CPH_OH_PermitHolder = ZGuid.Empty;
			AssertEquals("Apply to All", customsRule.OrganizationDescription);
		}

		#region Override

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = base.GetNewBusinessObjectForDeleteTest(factory) as CustomsRule;
			result.Rules.ForEach(x =>
			{
				x.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement;
				x.CPR_ValueFrom = "ValueFrom";
			});
			return result;
		}

		#endregion
	}

	#region Loader Test

	[TestedType(typeof(CustomsRule.Loader))]
	class LoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CustomsRule.Loader(Factory);
		}
	}

	#endregion
}
