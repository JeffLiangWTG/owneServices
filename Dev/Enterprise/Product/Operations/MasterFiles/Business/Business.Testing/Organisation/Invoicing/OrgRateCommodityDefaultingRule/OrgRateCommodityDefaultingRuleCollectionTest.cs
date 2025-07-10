using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRateCommodityDefaultingRuleCollection))]
	sealed class OrgRateCommodityDefaultingRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgRateCommodityDefaultingRuleCollection>
	{
		protected override OrgRateCommodityDefaultingRuleCollection GetCollectionToTest()
		{
			return new OrgRateCommodityDefaultingRuleCollection(Factory);
		}
	}
}
