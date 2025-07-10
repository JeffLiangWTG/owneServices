using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccGroupCommissionRuleCollection))]
	sealed class AccGroupCommissionRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<AccGroupCommissionRuleCollection>
	{
		protected override AccGroupCommissionRuleCollection GetCollectionToTest()
		{
			var salesTeam = Factory.New<SalesTeam>();
			return new AccGroupCommissionRuleCollection(salesTeam);
		}
	}
}
