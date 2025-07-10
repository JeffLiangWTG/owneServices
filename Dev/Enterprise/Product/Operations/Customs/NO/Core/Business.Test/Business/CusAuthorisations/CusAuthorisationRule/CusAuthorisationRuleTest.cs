using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(CusAuthorisationRule))]
	sealed class CusAuthorisationRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			(_, var authorisationRule) = SetupData(Factory);
			AssertType<CusAuthorisationRuleLookups>(authorisationRule.Lookups);
		}

		public void TestLinkedCusAuthorisationRules()
		{
			(_, var authorisationRule) = SetupData(Factory);
			AssertType<LinkedCusAuthorisationRuleCollection>(authorisationRule.LinkedCusAuthorisationRules);
		}

		public void TestValidation()
		{
			(_, var authorisationRule) = SetupData(Factory);
			AssertType<CusAuthorisationRuleValidation>(authorisationRule.Validation);
		}

		public void TestCPR_DescriptionFieldType()
		{
			CombineAssertions(() =>
			{
				(_, var authorisationRule) = SetupData(Factory);
				authorisationRule.CPR_RuleCode = "ABC";
				AssertEquals("FieldType from Fallback", nameof(FieldType.Text), authorisationRule.CPR_DescriptionFieldType);
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
				AssertEquals("FieldType obtained from Provider", nameof(FieldType.Text), authorisationRule.CPR_DescriptionFieldType);
			});
		}

		public void TestMasterLinkedRuleCreatedOnCPR_ValueFromChanged()
		{
			CombineAssertions(() =>
			{
				(_, var authorisationRule) = SetupData(Factory);
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
				AssertEquals("No linked rules initially", 0, authorisationRule.LinkedCusAuthorisationRules.Count);
				authorisationRule.CPR_ValueFrom = "ABC";
				AssertEquals("Linked rule auto added", 1, authorisationRule.LinkedCusAuthorisationRules.Count);
				var masterLinkedRule = authorisationRule.LinkedCusAuthorisationRules.FirstOrDefault();
				AssertEquals("Auto added linked rule should be VCO", LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices, masterLinkedRule.CPR_RuleCode);
				AssertEquals("Auto added linked rule should match parent MCO", "ABC", masterLinkedRule.CPR_ValueFrom);
			});
		}

		public void TestMasterLinkedRuleUpdatedOnCPR_ValueFromChanged()
		{
			CombineAssertions(() =>
			{
				(_, var authorisationRule) = SetupData(Factory);
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
				AssertEquals(0, authorisationRule.LinkedCusAuthorisationRules.Count);
				authorisationRule.CPR_ValueFrom = "ABC";
				AssertEquals("Should auto create a new VCO linked rule", 1, authorisationRule.LinkedCusAuthorisationRules.Count);
				var masterLinkedRule = authorisationRule.LinkedCusAuthorisationRules.FirstOrDefault();
				AssertEquals("Linked VCO rule should match master MCO rule", LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices, masterLinkedRule.CPR_RuleCode);
				AssertEquals("Linked VCO rule should match master MCO rule", "ABC", masterLinkedRule.CPR_ValueFrom);
				_ = authorisationRule.LinkedCusAuthorisationRules.AddNew();
				authorisationRule.CPR_ValueFrom = "DEF";
				var newMasterLinkedRule = authorisationRule.LinkedCusAuthorisationRules.FirstOrDefault(x => x.IsMasterLinkedRule);
				AssertEquals("Linked rule should be VCO", LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices, newMasterLinkedRule.CPR_RuleCode);
				AssertEquals("Linked VCO rule should match master MCO rule", "DEF", newMasterLinkedRule.CPR_ValueFrom);
				AssertSame("Should not create a new linked VCO rule but update existing", masterLinkedRule, newMasterLinkedRule);
				AssertEquals("Should not create a new linked VCO rule but update existing", 2, authorisationRule.LinkedCusAuthorisationRules.Count);
			});
		}

		public void TestMasterLinkedRuleUpdatedOnCPR_DescriptionChanged()
		{
			CombineAssertions(() =>
			{
				(_, var authorisationRule) = SetupData(Factory);
				authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
				authorisationRule.CPR_ValueFrom = "ABC";
				var masterLinkedRule = authorisationRule.LinkedCusAuthorisationRules.FirstOrDefault();
				Assert("Default description for rule is empty", authorisationRule.CPR_Description.IsEmpty);
				Assert("Default description for rule is empty", masterLinkedRule.CPR_Description.IsEmpty);
				authorisationRule.CPR_Description = "ABC123";
				AssertEquals("Linked rule Description should match master rule", "ABC123", masterLinkedRule.CPR_Description);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => SetupData(Factory).authorisationRule;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			(_, var rule) = SetupData(factory);
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			rule.CPR_ValueFrom = "VALUE";
			return rule;
		}

		(CusAuthorisationHeader authorisationHeader, CusAuthorisationRule authorisationRule) SetupData(BusinessObjectFactory factory)
		{
			var authorisationHeader = factory.NewWithValidTestData<CusAuthorisationHeader>();
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			return (authorisationHeader, authorisationRule);
		}
	}
}
