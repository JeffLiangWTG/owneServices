using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class CusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_Description()
		{
			authorisationRule.CPR_RuleCode = "ABC";
			ValidationTestHelper.AssertFieldIsNotMandatory(authorisationRule.CPR_DescriptionInfo);

			authorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.MainCustomsOffice;
			ValidationTestHelper.AssertErrorIfNotEntered(authorisationRule.CPR_DescriptionInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorisationRule = authorizationHeader.CusAuthorisationRules.AddNew();
		}
		CusAuthorisationRule authorisationRule;
	}
}
