using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentFilteredCollectionView))]
	public class HVLVConsignmentFilteredCollectionViewTest : BusinessObjectCollectionViewTestCase<HVLVConsignmentFilteredCollectionView>
	{
		protected override HVLVConsignmentFilteredCollectionView GetCollectionToTest()
		{
			return new HVLVConsignmentFilteredCollectionView(new HVLVShipmentConsignmentCollection(Factory.New<ForwardingShipment>()));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<HVLVConsignment>();
		}

		public void TestFilter()
		{
			var collectionView = GetCollectionToTest();
			var consignment1 = collectionView.AddNew();
			var consignment2 = collectionView.AddNew();

			consignment1.HVC_ShipperReference = "#1 Order";
			consignment2.HVC_ShipperReference = "#2 Order";

			collectionView.Filter = new ZQuery(HVLVConsignmentSchema.HVC_ShipperReference, "#1 Order");
			collectionView.Rebuild();

			AssertEquals("Only contains first consignment", consignment1.PK, collectionView.Single().PK);

			collectionView.Filter = new ZQuery(HVLVConsignmentSchema.HVC_ShipperReference, "#2 Order");
			collectionView.Rebuild();

			AssertEquals("Only contains second consignment", consignment2.PK, collectionView.Single().PK);
		}

		public void TestGetMatchingBizObject()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentCollection = new HVLVShipmentConsignmentCollection(shipment);
			var consignmentFilteredCollectionView = new HVLVConsignmentFilteredCollectionView(consignmentCollection);

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentCollection.Add(consignment1);
			consignment1.HVC_WaybillNumber = "waybillnumber01";

			Factory.Save();

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignmentCollection.Add(consignment2);
			consignment2.HVC_WaybillNumber = "waybillnumber02";

			AssertEquals("return matched data when it's in database", consignment1, ((IImportCollectionElementMatchingSupporter)consignmentFilteredCollectionView).GetMatchingBizObject("waybillnumber01"));
			AssertNull("return null when matched data not in database", ((IImportCollectionElementMatchingSupporter)consignmentFilteredCollectionView).GetMatchingBizObject("waybillnumber02"));
			AssertNull(((IImportCollectionElementMatchingSupporter)consignmentFilteredCollectionView).GetMatchingBizObject("waybillnumber not existing"));
		}

		public void TestSuspendAdditionallyForImport_SuspendsListChangedForParent()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignmentCollection = new HVLVShipmentConsignmentCollection(shipment);
			var consignmentFilteredCollectionView = new HVLVConsignmentFilteredCollectionView(consignmentCollection);

			var singleElementList = shipment.GetHVLVConsignmentHeader() as ISingleElementListInternal;

			Assert("Precondition: HVLVConsignmentHeader.ListChange is active", !singleElementList.IsListChangeSuspended);

			using (consignmentFilteredCollectionView.SuspendAdditionallyForImport())
			{
				Assert("HVLVConsignmentHeader.ListChange should be suspended", singleElementList.IsListChangeSuspended);
			}
		}

		public void TestSuspendAdditionallyForImport_WhenNotDependentBusinessObjectCollection_NoException()
		{
			var collection = new HVLVConsignmentFilteredCollectionView(new DummyBusinessObjectCollection(Factory));
			AssertNoExceptionThrown(() => collection.SuspendAdditionallyForImport());
		}

		public void TestFindGenericColumnMatches()
		{
			var collection = (IImportCollectionElementMatchingSupporter)GetCollectionToTest();
			Assert("FindGenericColumnMatches is false, to improve import speed", !collection.FindGenericColumnMatches);
		}
	}
}
