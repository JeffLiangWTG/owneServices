
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Integration;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVItemLineCollection))]
	public class HVLVItemLineCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDeletingItemLineRecalculatesItemManifestedWeight()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_WeightUQ = "KG";
			var item = consignment.Items.AddNew();

			var itemLine1 = item.Lines.AddNew();
			itemLine1.HVS_GrossWeight = 22;
			itemLine1.HVS_WeightUnit = "KG";

			var itemLine2 = item.Lines.AddNew();
			itemLine2.HVS_WeightUnit = "KG";
			itemLine2.HVS_GrossWeight = 123;
			AssertEquals(145M, item.HVI_ManifestedWeight);

			item.Lines.RemoveAndDelete(itemLine2);
			AssertEquals("Item manifested weight recalculated", 22M, item.HVI_ManifestedWeight);
		}

		public void TestGivenHVLVItemLineCollectionCreated_WhenIndexOperatorCalled_ThenCorrectElementReturned()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var item = consignment.Items.AddNew();

			var itemLine1 = item.Lines.AddNew();
			var itemLine2 = item.Lines.AddNew();
			var itemLine3 = item.Lines.AddNew();

			var collectionFirstItem = ((IHVLVItemLineCollection)item.Lines)[0];
			var collectionSecondItem = ((IHVLVItemLineCollection)item.Lines)[1];
			var collectionThirdItem = ((IHVLVItemLineCollection)item.Lines)[2];
			CombineAssertions("Index operator should access the correct item lines", () =>
			{
				AssertEquals(collectionFirstItem, itemLine1);
				AssertEquals(collectionSecondItem, itemLine2);
				AssertEquals(collectionThirdItem, itemLine3);
			});
		}

		public void TestLaod_HVLVFetchHintAllocatorService()
		{
			var item = Factory.NewWithValidTestData<HVLVItem>();
			var service = Factory.ServiceContainer.GetService<HVLVFetchHintAllocatorService<HVLVItemLineCollection>>();
			AssertNull("Precondition: Factory does not have fetch hint allocator service", service);
			Factory.Save();

			new HVLVItemLineCollection(item).Load();

			service = Factory.ServiceContainer.GetService<HVLVFetchHintAllocatorService<HVLVItemLineCollection>>();
			AssertNotNull("Service is added to factory in HVLVItemLineCollection.Load", service);
			AssertEquals("Count is 1 after 1 constructor call", 1, service.GetCollectionCountForTesting(item.HVI_ClusterKey));

			new HVLVItemLineCollection(item).Load();
			AssertEquals("HVLVItemLineCollection.Load increments count with each call", 2, service.GetCollectionCountForTesting(item.HVI_ClusterKey));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HVLVItemLineCollection(Factory.NewWithValidTestData<HVLVItem>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVItemLine>();
		}

		#endregion
	}
}
