using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccCommissionRuleCollection))]
	sealed class AccCommissionRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<AccCommissionRuleCollection>
	{
		protected override AccCommissionRuleCollection GetCollectionToTest()
		{
			return new AccCommissionRuleCollection(Factory);
		}
	}
}
