using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CusPermitRule))]
	sealed class CusPermitRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var permitHeader = Factory.New<CusPermitHeader>();
			var permitRule = permitHeader.CusPermitRules.AddNew();
			AssertEquals(PermitRuleCodeList.Codes.TAR, permitRule.CPR_RuleCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var permitHeader = Factory.New<CusPermitHeader>();
			return permitHeader.CusPermitRules.AddNew();
		}
	}
}
