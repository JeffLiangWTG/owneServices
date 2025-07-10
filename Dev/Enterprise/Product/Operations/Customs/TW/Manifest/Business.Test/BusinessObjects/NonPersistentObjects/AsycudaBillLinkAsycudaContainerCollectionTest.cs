using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillLinkAsycudaContainerCollection))]
	sealed class AsycudaBillLinkAsycudaContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AsycudaBillLinkAsycudaContainerCollection>
	{
		public void TestRebuildElements()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			var asycudaBillLinkAsycudaContainerCollection = new AsycudaBillLinkAsycudaContainerCollection(bill);
			AssertEquals(false, asycudaBillLinkAsycudaContainerCollection.Any());

			asycudaBillLinkAsycudaContainerCollection.RebuildElements();
			AssertEquals(1, asycudaBillLinkAsycudaContainerCollection.Count);

			header.Containers.AddNew();
			asycudaBillLinkAsycudaContainerCollection.RebuildElements();
			AssertEquals(2, asycudaBillLinkAsycudaContainerCollection.Count);
		}

		public void TestDeleteAsycudaBillLinkAsycudaContainerByContainer()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			var asycudaBillLinkAsycudaContainerCollection = new AsycudaBillLinkAsycudaContainerCollection(bill);
			AssertEquals(false, asycudaBillLinkAsycudaContainerCollection.Any());

			asycudaBillLinkAsycudaContainerCollection.RebuildElements();
			AssertEquals(1, asycudaBillLinkAsycudaContainerCollection.Count);

			asycudaBillLinkAsycudaContainerCollection.DeleteAsycudaBillLinkAsycudaContainerByContainer(container);
			AssertEquals(0, asycudaBillLinkAsycudaContainerCollection.Count);
		}

		protected override AsycudaBillLinkAsycudaContainerCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Containers.AddNew();
			var bill = header.Bills.AddNew();
			return bill.AsycudaBillLinkAsycudaContainers;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collectionForTesting = GetCollectionToTest();
			return collectionForTesting.Cast<AsycudaBillLinkAsycudaContainer>().First();
		}
	}
}
