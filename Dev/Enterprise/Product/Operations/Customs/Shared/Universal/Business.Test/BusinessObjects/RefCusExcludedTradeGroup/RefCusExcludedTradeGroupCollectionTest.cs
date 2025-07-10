using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusExcludedTradeGroupCollection))]
	public class RefCusExcludedTradeGroupCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusExcludedTradeGroupCollection>
	{
		protected override RefCusExcludedTradeGroupCollection GetCollectionToTest()
		{
			var applic = Factory.New<CusRefApplicabilityView>();
			return applic.ExcludedTradeGroups;
		}
	}
}
