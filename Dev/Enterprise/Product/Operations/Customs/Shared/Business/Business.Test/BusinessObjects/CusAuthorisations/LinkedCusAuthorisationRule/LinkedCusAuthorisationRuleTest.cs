using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(LinkedCusAuthorisationRule))]
	sealed class LinkedCusAuthorisationRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetNewLookup()
		{
			var linkedAuthorisationRule = Factory.New<LinkedCusAuthorisationRule>();
			AssertType<LinkedCusAuthorisationRuleLookups>(linkedAuthorisationRule.Lookups);
		}

		public void TestGetNewValidation()
		{
			var linkedAuthorisationRule = Factory.New<LinkedCusAuthorisationRule>();
			AssertType<LinkedCusAuthorisationRuleValidation>(linkedAuthorisationRule.Validation);
		}
		public void TestLookups()
		{
			AssertType<LinkedCusAuthorisationRuleLookups>(linkedAuthorisationRule.Lookups);
		}

		public void TestValidation()
		{
			AssertType<LinkedCusAuthorisationRuleValidation>(linkedAuthorisationRule.Validation);
		}

		public void TestCPR_Description_DefaultValue()
		{
			authorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			authorisationRule.CPR_ValueFrom = "ABC";
			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
			linkedAuthorisationRule.CPR_ValueFrom = "AA01";

			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AA01", "Customs Office AA01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "AA02", "Customs Office AA02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CPR_Description empty", ZString.Empty, linkedAuthorisationRule.CPR_Description);

				linkedAuthorisationRule.CPR_ValueFrom = "AA02";
				AssertEquals("CPR_Description from ValueDescription", "Customs Office AA02", linkedAuthorisationRule.CPR_Description);

				linkedAuthorisationRule.CPR_Description = "Edited description";
				linkedAuthorisationRule.CPR_ValueFrom = "AA01";
				AssertEquals("Updated CPR_ValueFrom but CPR_Description remains", "Edited description", linkedAuthorisationRule.CPR_Description);
			});
		}

		public void TestCPR_RuleCode_ReadOnly()
		{
			AssertEquals(true, linkedAuthorisationRule.CPR_RuleCodeInfo.ReadOnly);
		}

		public void TestCPR_RuleCode_ReadOnly_NoAuthHeader()
		{
			var linkedAuthorisationRule = Factory.New<LinkedCusAuthorisationRule>();
			AssertEquals(true, linkedAuthorisationRule.CPR_RuleCodeInfo.ReadOnly);
		}

		public void TestLinkedRule_ReadOnly()
		{
			AssertEquals(false, linkedAuthorisationRule.ReadOnly);
		}

		public void TestLinkedRule_ReadOnly_NoAuthHeader()
		{
			var linkedAuthorisationRule = Factory.New<LinkedCusAuthorisationRule>();
			AssertEquals(false, linkedAuthorisationRule.ReadOnly);
		}

		public void TestAuthorisationHeader()
		{
			AssertEquals(authorisationHeader, linkedAuthorisationRule.AuthorisationHeader);
		}

		public void TestAuthorisationRule()
		{
			AssertEquals(authorisationRule, linkedAuthorisationRule.AuthorisationRule);
		}

		public void TestCPR_ValueFromIsCodeField()
		{
			CombineAssertions(() =>
			{
				linkedAuthorisationRule.CPR_RuleCode = "XXX";
				Assert(!linkedAuthorisationRule.CPR_ValueFromIsCodeField);

				linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
				Assert(linkedAuthorisationRule.CPR_ValueFromIsCodeField);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => linkedAuthorisationRule;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rule = factory.NewWithValidTestData<CusAuthorisationHeader>().CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			rule.CPR_ValueFrom = "VALUE1";
			var linkedRule = rule.LinkedCusAuthorisationRules.AddNew();
			linkedRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
			linkedRule.CPR_ValueFrom = "VALUE2";
			return linkedRule;
		}

		protected override void SetUp()
		{
			base.SetUp();
			authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			linkedAuthorisationRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
		}

		CusAuthorisationHeader authorisationHeader;
		CusAuthorisationRule authorisationRule;
		LinkedCusAuthorisationRule linkedAuthorisationRule;
	}
}
