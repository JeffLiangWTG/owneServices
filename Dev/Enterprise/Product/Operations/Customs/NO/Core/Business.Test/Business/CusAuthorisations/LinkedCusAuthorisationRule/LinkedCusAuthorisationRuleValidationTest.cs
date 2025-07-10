using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class LinkedCusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_Description()
		{
			linkedAuthorisationRule.CPR_RuleCode = "ABC";
			ValidationTestHelper.AssertFieldIsNotMandatory(linkedAuthorisationRule.CPR_DescriptionInfo);

			linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
			ValidationTestHelper.AssertErrorIfNotEntered(linkedAuthorisationRule.CPR_DescriptionInfo);
		}

		public void TestCheckCPR_ValueFrom_UniqueCheckVCO()
		{
			CombineAssertions(() =>
			{
				linkedAuthorisationRule.CPR_RuleCode = "ABC";
				linkedAuthorisationRule.CPR_ValueFrom = "DEF";
				AssertNoErrorContaining(linkedAuthorisationRule.CPR_ValueFromInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(linkedAuthorisationRule.CPR_ValueFromInfo.HumanReadableName));
				var newLinkedAuthorisationRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
				AssertNoErrorContaining(newLinkedAuthorisationRule.CPR_ValueFromInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(newLinkedAuthorisationRule.CPR_ValueFromInfo.HumanReadableName));
				newLinkedAuthorisationRule.CPR_RuleCode = "ABC";
				newLinkedAuthorisationRule.CPR_ValueFrom = "DEF";
				AssertNoErrorContaining(newLinkedAuthorisationRule.CPR_ValueFromInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(newLinkedAuthorisationRule.CPR_ValueFromInfo.HumanReadableName));

				authorisationRule.LinkedCusAuthorisationRules.DeleteAll();

				linkedAuthorisationRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
				linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
				linkedAuthorisationRule.CPR_ValueFrom = "DEF";
				AssertNoErrorContaining(linkedAuthorisationRule.CPR_ValueFromInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(linkedAuthorisationRule.CPR_ValueFromInfo.HumanReadableName));
				newLinkedAuthorisationRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
				AssertNoErrorContaining(newLinkedAuthorisationRule.CPR_ValueFromInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(newLinkedAuthorisationRule.CPR_ValueFromInfo.HumanReadableName));
				newLinkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
				newLinkedAuthorisationRule.CPR_ValueFrom = "DEF";
				AssertHasErrorContaining(newLinkedAuthorisationRule.CPR_ValueFromInfo, PropertyIsUniqueInCollectionValidation.MustBeUniqueMessage(newLinkedAuthorisationRule.CPR_ValueFromInfo.HumanReadableName));
			});
		}

		public void TestCheckCPR_ValueFrom_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(linkedAuthorisationRule.CPR_ValueFromInfo);
		}

		public void TestCheckCPR_ValueFrom_ConditionalListValidation()
		{
			CombineAssertions(() =>
			{
				linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.ValidCustomsOffices;
				linkedAuthorisationRule.CPR_ValueFrom = "ABC";
				AssertNoMessageErrorContaining(linkedAuthorisationRule.CPR_ValueFromInfo, ListValidation.InvalidCodeMessageError);

				linkedAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
				linkedAuthorisationRule.CPR_ValueFrom = "ABC";
				AssertHasMessageErrorContaining(linkedAuthorisationRule.CPR_ValueFromInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationRule = authorizationHeader.CusAuthorisationRules.AddNew();
			linkedAuthorisationRule = authorisationRule.LinkedCusAuthorisationRules.AddNew();
		}
		CusAuthorisationRule authorisationRule;
		LinkedCusAuthorisationRule linkedAuthorisationRule;
	}
}
