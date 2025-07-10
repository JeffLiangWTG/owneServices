using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusGuaranteeRuleValidationTest : SharedCusPermitRuleValidationTest<CusGuaranteeRuleValidation, CusGuaranteeRule>
	{
		#region Implementation

		protected override CusGuaranteeRule GetNewRule(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.FillWithValidTestData();
			return guaranteeRule;
		}

		#endregion
	}
}
