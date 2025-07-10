using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPermitRuleCollection))]
	sealed class CusPermitRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<CusPermitRuleCollection>
	{
		protected override CusPermitRuleCollection GetCollectionToTest()
		{
			return new CusPermitRuleCollection(Factory.New<BaseCusPermitHeader>());
		}
	}
}
