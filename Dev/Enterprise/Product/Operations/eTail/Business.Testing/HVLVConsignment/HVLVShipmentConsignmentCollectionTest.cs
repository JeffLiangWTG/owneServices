using System;
using System.Linq;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVShipmentConsignmentCollection))]
	public class HVLVShipmentConsignmentCollectionTest : HVLVConsignmentCollectionWithPrefetchTest
	{
		public void TestConsignmentCollectionLoadsInactiveConsignmentsByDefault()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_IsActive = false;

			var consignmentCollection = new HVLVShipmentConsignmentCollection(shipment);
			consignmentCollection.Load();

			CombineAssertions(() =>
			{
				AssertEquals(1, consignmentCollection.Count);
				Assert("Consignment is inactive", !consignment.HVC_IsActive);
			});
		}

		public void TestManagingShipmentIsSetOnAllLoadedConsignments()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var header = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item1 = consignment1.Items.AddNew();
			item1.FillWithValidTestData();
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			var item2 = consignment2.Items.AddNew();
			item2.FillWithValidTestData();
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			header.Consignments.Add(consignment1);
			header.Consignments.Add(consignment2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
			var reloadedHeader = reloadedShipment.GetOrCreateHVLVConsignmentHeader();

			AssertEquals(2, reloadedHeader.Consignments.Count);
			AssertEquals(reloadedShipment, reloadedHeader.Consignments[0].ManagingShipment);
			AssertEquals(reloadedShipment, reloadedHeader.Consignments[1].ManagingShipment);
		}

		public void TestSetCollectionRelationships()
		{
			var shipment1 = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment1 = Factory.New<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment1.PK;
			consignment1.Items.AddNew();

			var consignment2 = Factory.New<HVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment2.PK;
			var item = consignment2.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment1.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedShipment1 = newFactory.Load<ForwardingShipment>(shipment1.PK);
			var collection = new HVLVShipmentConsignmentCollection(loadedShipment1);
			collection.Load();
			var consignment1InCollection = collection.FindByPK(consignment1.PK) as HVLVConsignment;
			var consignment2InCollection = collection.FindByPK(consignment2.PK) as HVLVConsignment;
			AssertEquals("Collection relationship for consignment1 should not change as it's already linked to a shipment", shipment1.PK, consignment1InCollection.HVC_JS_ManifestedOnShipment);
			AssertEquals("Collection relationship for consignment2 should not change as it's already linked to a shipment", shipment2.PK, consignment2InCollection.HVC_JS_ManifestedOnShipment);

			var newConsignment = collection.AddNew();
			AssertEquals("Collection relationship for new consignment should be set to collection master", shipment1.PK, newConsignment.HVC_JS_ManifestedOnShipment);
		}

		public void TestCollectionContainsAllChildrenAttachedToShipment()
		{
			var shipment1 = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var shipment2 = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			Func<ForwardingShipment, ForwardingShipment, HVLVConsignment> createConsignmentWithItem = (consignmentShipment, itemShipment) =>
			{
				var consignment = Factory.New<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = consignmentShipment.PK;
				var item = consignment.Items.AddNew();
				item.HVI_JS_LoadedOnShipment = itemShipment.PK;

				return consignment;
			};

			var consignment1 = createConsignmentWithItem(shipment1, shipment1);
			var consignment2 = createConsignmentWithItem(shipment1, shipment2);
			var consignment3 = createConsignmentWithItem(shipment2, shipment1);
			var consignment4 = createConsignmentWithItem(shipment2, shipment2);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var loadListShipment = factory2.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.PK, shipment1.PK)).Single();

			var collection = new HVLVShipmentConsignmentCollection(loadListShipment);
			collection.Load();

			var pkCollection = collection.Cast<HVLVConsignment>().Select(consignment => consignment.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consignment1.PK, consignment2.PK, consignment3.PK }, pkCollection);
		}

		public void TestHVLVShipmentConsignmentCollectionWouldNotLoadArchivedConsignments()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			var header = shipment.GetOrCreateHVLVConsignmentHeader();
			header.Consignments.Add(consignment);

			var item = consignment.Items.AddNew();
			item.FillWithValidTestData();
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			Factory.Save();

			var collection = new HVLVShipmentConsignmentCollection(shipment);
			collection.Load();

			AssertEquals("not archived consignment can be loaded on HVLVShipmentConsignmentCollection", 1, collection.Count);

			header.HCH_IsArchived = true;

			Factory.Save();

			collection.RemoveAll();
			collection.Load();

			AssertEquals("archived consignment can not be loaded on HVLVShipmentConsignmentCollection", 0, collection.Count);
		}

		public void TestShipmentHVLVConsignmentCollection_LoadAndReloadResultInSameCollection()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.Items.AddNew();

			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.Items[0].HVI_JS_LoadedOnShipment = shipment.PK;

			consignmentHeader.Consignments.Add(consignment1);
			consignmentHeader.Consignments.Add(consignment2);

			var collection = new HVLVShipmentConsignmentCollection(shipment);

			Factory.Save();
			collection.Load();

			AssertContainsExactElementsInAnyOrder(new[] { consignment1, consignment2 }, collection);
		}

		public void TestGivenHVLVShipmentConsignmentCollectionCreated_WhenIndexOperatorCalled_ThenCorrectElementReturned()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();

			var consignment1 = Factory.New<HVLVConsignment>();
			var consignment2 = Factory.New<HVLVConsignment>();
			var consignment3 = Factory.New<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;

			var collection = new HVLVShipmentConsignmentCollection(shipment);

			Factory.Save();
			collection.Load();

			var collectionFirstItem = ((IHVLVConsignmentCollectionForDocument)collection)[0];
			var collectionSecondItem = ((IHVLVConsignmentCollectionForDocument)collection)[1];
			var collectionThirdItem = ((IHVLVConsignmentCollectionForDocument)collection)[2];

			CombineAssertions("Index operator should access the correct consignments", () =>
			{
				AssertEquals(collectionFirstItem, consignment1);
				AssertEquals(collectionSecondItem, consignment2);
				AssertEquals(collectionThirdItem, consignment3);
			});
		}

		public void TestConsignmentHeader_ShouldCreateConsignmentHeaderWhenCollectionCreated()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var collection = new HVLVShipmentConsignmentCollection(shipment);

			var header = collection.Header;
			AssertNotNull("Consignment header should be created", header);
			AssertEquals("Consignment header should be linked to shipment", shipment.PK, header.HCH_JS_Shipment);
		}

		public void TestConsignmentHeader_GetExistingConsignmentHeader()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			Factory.Save();

			var collection = new HVLVShipmentConsignmentCollection(shipment);
			AssertNotNull(collection.Header);
			AssertEquals("Should load existing consignment header", consignmentHeader.PK, collection.Header.PK);
		}

		public void TestAddNew_ShouldLinkToConsignmentHeader()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var collection = new HVLVShipmentConsignmentCollection(shipment);

			var consignment = collection.AddNew();

			var consignmentHeader = collection.Header;
			AssertEquals("New consignment should be linked to consignment header", consignmentHeader.PK, consignment.HVC_HCH_Header);
			AssertEquals("New consignment should have cluster key from consignment header", consignmentHeader.HCH_ClusterKey, consignment.HVC_ClusterKey);
		}

		public void TestTriggerFieldNameCanAccessConsginmentProperties()
		{
			var hvlvConsignmentForDocumentType = typeof(IHVLVConsignmentForDocument);
			var propertyName = HVLVConsignmentSchema.HVC_ConsigneeInstructions.Name;
			AssertNull("Precondition: property name doesn't exist in IHVLVConsignmentForDocument",
			hvlvConsignmentForDocumentType.GetProperty(propertyName));

			var result = WorkflowMacroEvaluator.GetPropertyInfo(typeof(IHVLVConsignmentForDocument), propertyName);
			AssertNotNull(result);
		}

		public void TestRemove_DetachFromShipmentSuccessfully()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S000000001";
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			consignment.HVC_HCH_Header = consignmentHeader.PK;
			consignment.HVC_ConsignmentId = "C000000001";
			consignment.HVC_Status = HVLVConsignmentStatus.Codes.Booked;
			var item = consignment.Items.AddNew();
			Factory.Save();

			AssertEquals(true, consignment.CanDetach);
			AssertEquals(string.Empty, consignment.ReasonNotToBeAbleToDetach);
			consignmentHeader.Consignments.Remove(consignment);
			AssertEquals(0, consignmentHeader.Consignments.Count);
			AssertEquals(false, consignment.HVC_IsActive);
			AssertEquals(consignmentHeader.PK, consignment.HVC_HCH_Header);
			AssertEquals(consignmentHeader.HCH_ClusterKey, consignment.HVC_ClusterKey);
			AssertEquals(HVLVConsignmentStatus.Codes.Detached, consignment.HVC_Status);
			AssertEquals("S000000001|TYP=Shipment", consignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.Detached.Code).FirstOrDefault().SL_Reference);
			AssertEquals(Guid.Empty, item.HVI_JS_LoadedOnShipment);
			AssertEquals(false, item.HVI_IsActive);
		}

		public void TestAdd_AttachToShipmentSuccessfully()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			var header = shipment1.GetOrCreateHVLVConsignmentHeader();
			var consignment = header.Consignments.AddNew();
			shipment1.JS_UniqueConsignRef = "S000000001";
			consignment.HVC_ConsignmentId = "C000000001";
			consignment.HVC_IsActive = true;
			consignment.HVC_Status = HVLVConsignmentStatus.Codes.Booked;
			var item = consignment.Items.AddNew();
			item.HVI_ManifestedVolume = 10;
			item.HVI_ManifestedWeight = 10;
			AssertEquals(1, (int)consignment.HVC_ItemCount);
			Factory.Save();

			header.Consignments.Remove(consignment);
			AssertEquals(0m, consignment.HVC_ManifestedWeight);
			AssertEquals(0m, consignment.HVC_ManifestedVolume);
			AssertEquals(0, (int)consignment.HVC_ItemCount);

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S000000002";
			var consignmentHeader = shipment2.GetOrCreateHVLVConsignmentHeader();

			consignmentHeader.Consignments.Add(consignment);
			Factory.Save();
			var reloadedConsignment = new BusinessObjectFactory().Load<HVLVConsignment>(consignment.PK);
			AssertEquals(1, consignmentHeader.Consignments.Count);
			AssertEquals(true, reloadedConsignment.HVC_IsActive);
			AssertEquals(consignmentHeader.PK, reloadedConsignment.HVC_HCH_Header);
			AssertEquals(consignmentHeader.HCH_ClusterKey, reloadedConsignment.HVC_ClusterKey);
			AssertEquals(HVLVConsignmentStatus.Codes.Booked, reloadedConsignment.HVC_Status);
			AssertEquals(shipment2.PK, item.HVI_JS_LoadedOnShipment);
			AssertEquals(consignmentHeader.HCH_ClusterKey, item.HVI_ClusterKey);
			AssertEquals(10m, reloadedConsignment.HVC_ManifestedWeight);
			AssertEquals(10m, reloadedConsignment.HVC_ManifestedVolume);
			AssertEquals(1, (int)reloadedConsignment.HVC_ItemCount);
			AssertEquals("S000000002|TYP=Shipment", reloadedConsignment.Logs.Find(log => log.SL_SE_NKEvent == AutoEvents.Attached.Code).FirstOrDefault().SL_Reference);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var shipment = Factory.New<HVLVForwardingShipment>();
			return new HVLVShipmentConsignmentCollection(shipment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVConsignment>();
		}

		#endregion

	}
}
