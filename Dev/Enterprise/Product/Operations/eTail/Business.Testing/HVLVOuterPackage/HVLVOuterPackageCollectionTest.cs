using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Integration;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOuterPackageCollection))]
	class HVLVOuterPackageCollectionTest : ActiveBusinessObjectCollectionTestCase<HVLVOuterPackageCollection>
	{
		public void TestGivenHVLVOuterPackageCollectionCreated_WhenIndexOperatorCalled_ThenCorrectElementReturned()
		{
			var loadlist = Factory.New<HVLVOriginLoadList>();

			var outerPackage1 = loadlist.OuterPackages.AddNew();
			var outerPackage2 = loadlist.OuterPackages.AddNew();
			var outerPackage3 = loadlist.OuterPackages.AddNew();

			var collectionFirstItem = ((IHVLVOuterPackageCollection)loadlist.OuterPackages)[0];
			var collectionSecondItem = ((IHVLVOuterPackageCollection)loadlist.OuterPackages)[1];
			var collectionThirdItem = ((IHVLVOuterPackageCollection)loadlist.OuterPackages)[2];
			CombineAssertions("Index operator should access the correct outer packages", () =>
			{
				AssertEquals(collectionFirstItem, outerPackage1);
				AssertEquals(collectionSecondItem, outerPackage2);
				AssertEquals(collectionThirdItem, outerPackage3);
			});
		}
	}
}
