using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusCalculationRuleCollection<CusCalculationRule>))]
	sealed class CusCalculationRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<CusCalculationRuleCollection<CusCalculationRule>>
	{
		public void TestConstructor()
		{
			AssertNoExceptionThrown(() => new CusCalculationRuleCollection<CusCalculationRule>(Factory));
		}

		protected override CusCalculationRuleCollection<CusCalculationRule> GetCollectionToTest() => new CusCalculationRuleCollection<CusCalculationRule>(Factory);
	}
}
