using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SharedCusPermitRuleTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var permit = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			var permitRule = permit.CusPermitRules.AddNew();
			permitRule.FillWithValidTestData();
			var guarantee = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var guaranteeRule = guarantee.CusGuaranteeRules.AddNew();
			guaranteeRule.FillWithValidTestData();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			AssertType<BaseCusPermitRule>(newFactory.Load<SharedCusPermitRule>(permitRule.PK));
			AssertType<CusGuaranteeRule>(newFactory.Load<SharedCusPermitRule>(guaranteeRule.PK));
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(BaseCusPermitRule), new SharedCusPermitRuleTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertNull(new SharedCusPermitRuleTypeDecider().GetTypeForBinding());
		}
	}
}
