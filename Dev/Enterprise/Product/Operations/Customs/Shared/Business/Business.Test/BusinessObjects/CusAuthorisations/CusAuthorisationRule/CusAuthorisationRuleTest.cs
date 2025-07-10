using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusAuthorisationRule))]
	sealed class CusAuthorisationRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<CusAuthorisationRuleLookups>(authorisationRule.Lookups);
		}

		public void TestValidation()
		{
			AssertType<CusAuthorisationRuleValidation>(authorisationRule.Validation);
		}

		public void TestCodeAndDescriptionProperty()
		{
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			authorisationRule.CPR_ValueFrom = "AA02";
			authorisationRule.CPR_Description = "AA02 Description";
			CombineAssertions(() =>
			{
				AssertEquals("Code", "AA02", CodePropertyAttribute.CodeFromBusinessObject(authorisationRule));
				AssertEquals("Description", "AA02 Description", DescriptionPropertyAttribute.DescriptionFromBusinessObject(authorisationRule));
			});
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Authorization Rule", authorisationRule.HumanReadableName);
		}

		public void TestAllowLinkedRules()
		{
			CombineAssertions(() =>
			{
				AssertEquals("RuleCode empty, AllowLinkedRules = false", false, authorisationRule.AllowLinkedRules);

				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertEquals("RuleCode LOC, AllowLinkedRules = true", true, authorisationRule.AllowLinkedRules);

				authorisationRule.CPR_RuleCode = "ABC";
				AssertEquals("RuleCode ABC, AllowLinkedRules = false", false, authorisationRule.AllowLinkedRules);
			});
		}

		public void TestLinkedRulesDeletedWhenLinkedRulesNotAllowed()
		{
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			authorisationRule.LinkedCusAuthorisationRules.AddNew();
			authorisationRule.LinkedCusAuthorisationRules.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("Added linked rules", 2, authorisationRule.LinkedCusAuthorisationRules.Count);

				authorisationRule.CPR_RuleCode = "ABC";
				AssertEquals("AllowLinkedRules = false, linked rules deleted", 0, authorisationRule.LinkedCusAuthorisationRules.Count);
			});
		}

		public void TestCPR_Description_DefaultValue()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facility Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA01", "Loading place AA01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "AA02", "Loading place AA02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			authorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			authorisationRule.CPR_ValueFrom = "AA03";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("CPR_Description empty", ZString.Empty, authorisationRule.CPR_Description);

				authorisationRule.CPR_ValueFrom = "AA02";
				AssertEquals("CPR_Description from ValueDescription", "Loading place AA02", authorisationRule.CPR_Description);

				authorisationRule.CPR_ValueFrom = "AA01";
				AssertEquals("Updated CPR_ValueFrom", "Loading place AA01", authorisationRule.CPR_Description);
			});
		}

		public void TestCPR_ValueFieldType()
		{
			CombineAssertions(() =>
			{
				authorisationRule.CPR_RuleCode = "ABC";
				AssertEquals("FieldType from Fallback", nameof(FieldType.Text), authorisationRule.CPR_ValueFromFieldType);
				AssertEquals(false, authorisationRule.CPR_ValueFromIsCodeField);
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertEquals("FieldType obtained from Provider", nameof(FieldType.TextCodeFindBox), authorisationRule.CPR_ValueFromFieldType);
				AssertEquals(true, authorisationRule.CPR_ValueFromIsCodeField);
			});
		}

		public void TestCPR_ValueFromShouldBeDefaultedWhenTypeChanges()
		{
			var rule = Factory.New<CusAuthorisationHeaderForTest>().CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = ((int)FieldType.TextCodeFindBox).ToString();
			AssertEquals(nameof(FieldType.TextCodeFindBox), rule.CPR_ValueFromFieldType);
			AssertEquals("Default to empty", ZString.Empty, rule.CPR_ValueFrom);

			rule.CPR_RuleCode = ((int)FieldType.Boolean).ToString();
			AssertEquals(nameof(FieldType.Boolean), rule.CPR_ValueFromFieldType);
			AssertEquals("Default to N", YesNoList.Codes.No, rule.CPR_ValueFrom);
		}

		public void TestCPR_ValueFromMaxLength()
		{
			AssertEquals(CusAuthorisationRule.Schema.CPR_ValueFromMaxLength, authorisationRule.CPR_ValueFromInfo.MaxLength);
		}

		public void TestCPR_DescriptionFieldType()
		{
			CombineAssertions(() =>
			{
				authorisationRule.CPR_RuleCode = "ABC";
				AssertEquals("FieldType from Fallback", nameof(FieldType.Text), authorisationRule.CPR_DescriptionFieldType);
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
				AssertEquals("FieldType obtained from Provider", nameof(FieldType.Text), authorisationRule.CPR_DescriptionFieldType);
			});
		}

		public void TestDelete()
		{
			var linkedRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
			authorisationRule.Delete();
			AssertEquals("Linked rules should have been deleted when master rule is deleted.", true, linkedRule.IsDeleted);
		}

		public void TestLinkedCusAuthorisationRules()
		{
			AssertType<LinkedCusAuthorisationRuleCollection>(authorisationRule.LinkedCusAuthorisationRules);
		}

		public void TestCPR_CPR_Rule_AlwaysEmpty()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CPR_CPR_Rule is Empty", ZGuid.Empty, authorisationRule.CPR_CPR_Rule);
				authorisationRule.CPR_CPR_Rule = ZGuid.BrettsGuid;
				AssertEquals("CPR_CPR_Rule is still Empty", ZGuid.Empty, authorisationRule.CPR_CPR_Rule);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => authorisationRule;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rule = factory.NewWithValidTestData<CusAuthorisationHeader>().CusAuthorisationRules.AddNew();
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			rule.CPR_ValueFrom = "VALUE";
			return rule;
		}

		protected override void SetUp()
		{
			base.SetUp();
			authorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
		}
		CusAuthorisationHeader authorisationHeader;
		CusAuthorisationRule authorisationRule;
	}
}
