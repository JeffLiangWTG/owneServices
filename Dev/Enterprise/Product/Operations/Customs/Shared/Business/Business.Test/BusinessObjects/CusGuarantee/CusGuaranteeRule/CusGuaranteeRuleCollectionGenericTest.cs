using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGuaranteeRuleCollection<CusGuaranteeRule>))]
	sealed class CusGuaranteeRuleCollectionGenericTest : ActiveBusinessObjectCollectionTestCase<CusGuaranteeRuleCollection<CusGuaranteeRule>>
	{
		protected override CusGuaranteeRuleCollection<CusGuaranteeRule> GetCollectionToTest()
		{
			return new CusGuaranteeRuleCollection<CusGuaranteeRule>(Factory.New<BaseCusGuaranteeHeader>());
		}
	}
}
