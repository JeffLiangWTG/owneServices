using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(LinkedCusAuthorisationRule))]
	sealed class LinkedCusAuthorisationRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			(_, _, var linkedAuthorisationRule) = SetupData(Factory);
			AssertType<LinkedCusAuthorisationRuleLookups>(linkedAuthorisationRule.Lookups);
		}

		public void TestValidation()
		{
			(_, _, var linkedAuthorisationRule) = SetupData(Factory);
			AssertType<LinkedCusAuthorisationRuleValidation>(linkedAuthorisationRule.Validation);
		}

		public void TestIsMasterLinkedRuleForNewRule()
		{
			(_, var authorisationRule, var linkedAuthorisationRule) = SetupData(Factory);
			Assert("New rule is not master linked", !linkedAuthorisationRule.IsMasterLinkedRule);
			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
			linkedAuthorisationRule.CPR_ValueFrom = "ABC";
			Assert("Rule not matching parent is not master linked", !linkedAuthorisationRule.IsMasterLinkedRule);
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
			authorisationRule.CPR_ValueFrom = "DEF";
			Assert("Initial rule is not master linked. New rule created", !linkedAuthorisationRule.IsMasterLinkedRule);
			var masterLinkedRule = authorisationRule.LinkedCusAuthorisationRules.FirstOrDefault(x => x.CPR_ValueFrom == "DEF");
			AssertNotSame("Initial rule is not master linked. New rule created", masterLinkedRule, linkedAuthorisationRule);
			Assert("New linked rule matching parent has IsMasterLinkedRule = true", masterLinkedRule.IsMasterLinkedRule);

			Factory.Save();

			authorisationRule.Reload();
			var savedMasterLinkedRule = authorisationRule.LinkedCusAuthorisationRules.FirstOrDefault(x => x.CPR_ValueFrom == "DEF");
			AssertSame("Saving and reloading the data should preserve flag IsMasterLinkedRule", masterLinkedRule, savedMasterLinkedRule);
			Assert("Saving and reloading the data should preserve flag IsMasterLinkedRule", masterLinkedRule.IsMasterLinkedRule);
		}

		public void TestReadOnlyForNewRule()
		{
			(_, var authorisationRule, var linkedAuthorisationRule) = SetupData(Factory);
			Assert("New rule is not ReadOnly", !linkedAuthorisationRule.ReadOnly);
			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
			linkedAuthorisationRule.CPR_ValueFrom = "ABC";
			Assert("Rule not matching parent is not ReadOnly", !linkedAuthorisationRule.ReadOnly);
			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
			authorisationRule.CPR_ValueFrom = "DEF";
			Assert("Initial rule is not ReadOnly. New rule created", !linkedAuthorisationRule.ReadOnly);
			var masterLinkedRule = authorisationRule.LinkedCusAuthorisationRules.FirstOrDefault(x => x.CPR_ValueFrom == "DEF");
			Assert("New linked rule matching parent is ReadOnly = true", masterLinkedRule.ReadOnly);

			Factory.Save();

			authorisationRule.Reload();
			var savedMasterLinkedRule = authorisationRule.LinkedCusAuthorisationRules.FirstOrDefault(x => x.CPR_ValueFrom == "DEF");
			Assert("Saving and reloading the data should preserve flag ReadOnly", masterLinkedRule.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject() => SetupData(Factory).linkedAuthorisationRule;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			(_, var rule, var linkedRule) = SetupData(factory);
			rule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Location;
			rule.CPR_ValueFrom = "VALUE1";
			linkedRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
			linkedRule.CPR_ValueFrom = "VALUE2";
			return linkedRule;
		}

		(CusAuthorisationHeader authorisationHeader, CusAuthorisationRule authorisationRule, LinkedCusAuthorisationRule linkedAuthorisationRule) SetupData(BusinessObjectFactory factory)
		{
			var authorisationHeader = factory.NewWithValidTestData<CusAuthorisationHeader>();
			var authorisationRule = authorisationHeader.CusAuthorisationRules.AddNew();
			return (authorisationHeader, authorisationRule, authorisationRule.LinkedCusAuthorisationRules.AddNew());
		}
	}
}
