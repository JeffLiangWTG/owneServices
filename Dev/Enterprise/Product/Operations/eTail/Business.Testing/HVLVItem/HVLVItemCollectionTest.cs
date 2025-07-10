using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.DataMapping;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVItemCollection))]
	public class HVLVItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNewAndRemove()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			AssertAllowNew(shipment1, null, true);
			AssertAllowNew(shipment1, shipment1, true);
			AssertAllowNew(shipment1, shipment2, false);
		}

		public void TestGivenHVLVItemCollectionCreated_WhenIndexOperatorCalled_ThenCorrectElementReturned()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var itemCollection = new HVLVItemCollection(consignment);

			var item1 = itemCollection.AddNew();
			var item2 = itemCollection.AddNew();
			var item3 = itemCollection.AddNew();

			var collectionFirstItem = ((IHVLVItemCollection)itemCollection)[0];
			var collectionSecondItem = ((IHVLVItemCollection)itemCollection)[1];
			var collectionThirdItem = ((IHVLVItemCollection)itemCollection)[2];
			CombineAssertions("Index operator should access the correct items", () =>
			{
				AssertEquals(collectionFirstItem, item1);
				AssertEquals(collectionSecondItem, item2);
				AssertEquals(collectionThirdItem, item3);
			});
		}

		public void TestItemCollectionLoadsInactiveItemsByDefault()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_IsActive = false;

			var itemCollection = new HVLVItemCollection(consignment);
			itemCollection.Load();

			CombineAssertions(() =>
			{
				AssertEquals(1, itemCollection.Count);
				Assert("Item is inactive", !item.HVI_IsActive);
			});
		}

		public void TestLoad_HVLVFetchHintAllocatorService()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			Factory.Save();
			var service = Factory.ServiceContainer.GetService<HVLVFetchHintAllocatorService<HVLVItemCollection>>();
			AssertNull("Precondition: Factory does not have fetch hint allocator service", service);

			new HVLVItemCollection(consignment).Load();

			service = Factory.ServiceContainer.GetService<HVLVFetchHintAllocatorService<HVLVItemCollection>>();
			AssertNotNull("Service is added to factory in HVLVItemCollection.Load", service);
			AssertEquals("Count is 1 after 1 constructor call", 1, service.GetCollectionCountForTesting(consignment.HVC_ClusterKey));

			new HVLVItemCollection(consignment).Load();
			AssertEquals("HVLVItemCollection.Load increments count with each call", 2, service.GetCollectionCountForTesting(consignment.HVC_ClusterKey));
		}

		#region IImportCollectionElementMatchingSupporter

		public void TestIImportCollectionElementMatchingSupporterGetMatchingBizObject_IfMatchingKeyIsEmpty_ReturnNull()
		{
			const string barcodeForMatching = "98432782389213089";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var itemCollection = consignment.Items;
			var itemWithMatchingBarcode = itemCollection.AddNew();
			itemWithMatchingBarcode.HVI_CurrentBarcode = barcodeForMatching;

			var itemWithEmptyBarcode = itemCollection.AddNew();
			AssertEquals("Precondition: Item doesn't have barcode", string.Empty, itemWithEmptyBarcode.HVI_CurrentBarcode);

			var matchingSupporter = itemCollection as IImportCollectionElementMatchingSupporter;
			AssertNotNull("Precondition: HVLVItemCollection is IImportCollectionElementMatchingSupporter", matchingSupporter);

			var matchResult = matchingSupporter.GetMatchingBizObject(barcodeForMatching);
			AssertEquals("Precondition: Item with matching barcode should be returned", itemWithMatchingBarcode, matchResult);

			matchResult = matchingSupporter.GetMatchingBizObject(string.Empty);
			AssertNull("Matching result", matchResult);
		}

		public void TestGetMatchingBizObject()
		{
			var consignment = Factory.New<HVLVConsignment>();
			var itemCollection = new HVLVItemCollection(consignment);

			var item1 = itemCollection.AddNew();
			item1.HVI_CurrentBarcode = "000100010001";

			var item2 = itemCollection.AddNew();
			item2.HVI_CurrentBarcode = "111011101110";

			AssertEquals(item1, ((IImportCollectionElementMatchingSupporter)itemCollection).GetMatchingBizObject("000100010001"));
			AssertNull(((IImportCollectionElementMatchingSupporter)itemCollection).GetMatchingBizObject("barcode not existing"));
		}

		public void TestWhenBeginUpdate_ThenRemoveAllItemLines()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_CurrentBarcode = "HVI001";
			item.Lines.AddNew();

			((IImportCollectionElementMatchingSupporter)consignment.Items).PrepareForReuse(item);
			AssertEquals("Expected the HVS for item to be removed", 0, item.Lines.Count);
		}

		public void TestFindGenericColumnMatches()
		{
			var collection = (IImportCollectionElementMatchingSupporter)GetCollectionToTest();
			Assert("FindGenericColumnMatches is false, to improve import speed", !collection.FindGenericColumnMatches);
		}

		#endregion

		#region Implementation

		void AssertAllowNew(ForwardingShipment manifestedShipment, ForwardingShipment managingShipment, bool expectedAllowNewAndRemove)
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = manifestedShipment.PK;

			if (managingShipment != null)
			{
				consignment.ManagingShipment = managingShipment;
			}

			AssertEquals(expectedAllowNewAndRemove, consignment.Items.AllowNew);
			AssertEquals(expectedAllowNewAndRemove, consignment.Items.AllowRemove);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HVLVItemCollection(Factory.New<HVLVConsignment>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVItem>();
		}

		#endregion
	}
}
