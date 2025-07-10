using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AccessCodePinRuleValidationTest : SharedCusPermitRuleValidationTest<AccessCodePinRuleValidation, CusGuaranteeRule>
	{
		public void TestCheckCPR_ValueFrom()
		{
			var guarantee = Factory.New<BaseCusGuaranteeHeader>();
			guarantee.MainAccessCode = "123";
			guarantee.MainAccessCode = ZString.Empty;
			AssertNoErrors(guarantee.MainAccessCodeRule.CPR_ValueFromInfo);
		}

		#region Implementation

		protected override CusGuaranteeRule GetNewRule(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var accessCode = guaranteeHeader.AdditionalAccessCodes.AddNew();
			accessCode.FillWithValidTestData();
			return accessCode;
		}

		#endregion
	}
}
