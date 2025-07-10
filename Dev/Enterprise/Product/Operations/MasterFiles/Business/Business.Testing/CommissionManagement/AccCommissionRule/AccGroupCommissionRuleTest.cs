using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGroupCommissionRule))]
	sealed class AccGroupCommissionRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var rule = Factory.New<AccGroupCommissionRule>();
			AssertType(typeof(AccGroupCommissionRuleValidation), rule.Validation);
		}
	}
}
