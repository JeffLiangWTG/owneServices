using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemConsignmentOrderReferenceCollection))]
	public class WhsItemConsignmentOrderReferenceCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemConsignmentOrderReferenceCollection>
	{
		public void TestGetConsignmentOrderReferenceByRCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var consignmentOrderReference1 = Helper.CreateWhsItemConsignmentOrderReference("REF1", rcn);
			var consignmentOrderReference2 = Helper.CreateWhsItemConsignmentOrderReference("REF2", rcn);
			var consignmentOrderReferenceDCN = Helper.CreateWhsItemConsignmentOrderReference("REF2", dcn);
			Factory.Save();

			var consignmentOrderReferenceCollection = new WhsItemConsignmentOrderReferenceCollection(rcn);
			AssertNotNull(consignmentOrderReferenceCollection);
			AssertEquals("Should have 2 order refs", 2, consignmentOrderReferenceCollection.Count);

			AssertEquals("Order Ref Should Be REF1", "REF1", consignmentOrderReferenceCollection.Single(s => s.PK == consignmentOrderReference1.PK).WOR_OrderReference);
			AssertEquals("Order Ref Should Be REF2", "REF2", consignmentOrderReferenceCollection.Single(s => s.PK == consignmentOrderReference2.PK).WOR_OrderReference);
		}

		public void TestGetConsignmentOrderReferenceByDCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var consignmentOrderReference1 = Helper.CreateWhsItemConsignmentOrderReference("REF1", dcn);
			var consignmentOrderReference2 = Helper.CreateWhsItemConsignmentOrderReference("REF2", dcn);
			var consignmentOrderReferenceRCN = Helper.CreateWhsItemConsignmentOrderReference("REF2", rcn);
			Factory.Save();

			var consignmentOrderReferenceCollection = new WhsItemConsignmentOrderReferenceCollection(dcn);
			AssertNotNull(consignmentOrderReferenceCollection);
			AssertEquals("Should have 2 order refs", 2, consignmentOrderReferenceCollection.Count);

			AssertEquals("Order Ref Should Be REF2", "REF1", consignmentOrderReferenceCollection.Single(s => s.PK == consignmentOrderReference1.PK).WOR_OrderReference);
			AssertEquals("Order Ref Should Be REF2", "REF2", consignmentOrderReferenceCollection.Single(s => s.PK == consignmentOrderReference2.PK).WOR_OrderReference);
		}

		protected override WhsItemConsignmentOrderReferenceCollection GetCollectionToTest()
		{
			return new WhsItemConsignmentOrderReferenceCollection(Factory.NewWithValidTestData<WhsItemReceiveConsignment>());
		}

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);
	}
}
