using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusRulingCombinedCollection))]
	class ZZRefCusRulingCombinedCollectionTest : ActiveBusinessObjectCollectionTestCase<ZZRefCusRulingCombinedCollection>
	{
		protected override ZZRefCusRulingCombinedCollection GetCollectionToTest()
		{
			return new ZZRefCusRulingCombinedCollection(Factory);
		}
	}
}
