using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ComInvOrderLineReconciliationCollection))]
	sealed class ComInvOrderLineReconciliationCollectionTest : ActiveBusinessObjectCollectionTestCase<ComInvOrderLineReconciliationCollection>
	{
		public void TestIndexer()
		{
			ComInvOrderLineReconciliation comInvLine = comInvOrderLineCollection.AddNew();
			comInvLine.JO_LineNo = 1;
			comInvLine.JO_Quantity = 1m;

			ComInvOrderLineReconciliation comInvLine2 = comInvOrderLineCollection.AddNew();
			comInvLine2.JO_LineNo = 2;
			comInvLine2.JO_Quantity = 1m;

			ComInvOrderLineReconciliation comInvLine3 = comInvOrderLineCollection.AddNew();
			comInvLine3.JO_LineNo = 2;
			comInvLine3.JO_Quantity = 1m;

			AssertNotNull(comInvOrderLineCollection[0]);
			AssertEquals(comInvLine, comInvOrderLineCollection[0]);
			AssertNotNull(comInvOrderLineCollection[1]);
			AssertEquals(comInvLine2, comInvOrderLineCollection[1]);
			AssertNotNull(comInvOrderLineCollection[2]);
			AssertEquals(comInvLine3, comInvOrderLineCollection[2]);
		}

		public void TestLoadOverride()
		{
			ComInvOrderLineReconciliationCollection collection = new ComInvOrderLineReconciliationCollection(comInvOrder);

			OrderLine line = Factory.New<OrderLine>();
			line.JO_LineNo = 1;
			line.JO_Quantity = 10m;
			line.JO_QtyInvoiced = 5m;
			line.JO_JD = comInvOrder.PK;

			line = Factory.New<OrderLine>();
			line.JO_LineNo = 2;
			line.JO_Quantity = 10m;
			line.JO_QtyInvoiced = 5m;
			line.JO_JD = comInvOrder.PK;

			line = Factory.New<OrderLine>();
			line.JO_LineNo = 3;
			line.JO_Quantity = 10m;
			line.JO_QtyInvoiced = 5m;
			line.JO_JD = comInvOrder.PK;

			comInvOrder.QuantityType = ComInvReconciliationQuantityType.OrderQuantity;
			//Collection.Load();
			AssertEquals(3, collection.Count);
			CheckLineUpdateWithParentOrderQuantityType(collection, comInvOrder);

			collection = new ComInvOrderLineReconciliationCollection(comInvOrder);
			//Collection.Load(new ZQuery(JobOrderLineSchema.JO_JD, ComInvOrder.PK));
			AssertEquals(3, collection.Count);
			CheckLineUpdateWithParentOrderQuantityType(collection, comInvOrder);
		}

		#region QuantityType_HasChanged

		public void TestQuantityType_HasChanged()
		{
			ComInvOrderLineReconciliation comInvLine = comInvOrderLineCollection.AddNew();
			comInvLine.JO_LineNo = 1;
			comInvLine.JO_Quantity = 13m;
			comInvLine.QuantityType = ComInvReconciliationQuantityType.OrderQuantity;

			ComInvOrderLineReconciliation comInvLine2 = comInvOrderLineCollection.AddNew();
			comInvLine2.JO_LineNo = 2;
			comInvLine2.JO_Quantity = 1m;
			comInvLine2.QuantityType = ComInvReconciliationQuantityType.OrderQuantity;

			ComInvOrderLineReconciliation comInvLine3 = comInvOrderLineCollection.AddNew();
			comInvLine3.JO_LineNo = 2;
			comInvLine3.JO_Quantity = 1m;
			comInvLine3.QuantityType = ComInvReconciliationQuantityType.OrderQuantity;

			comInvOrder.QuantityType = ComInvReconciliationQuantityType.InvoiceQuantity;
			CheckLineUpdateWithParentOrderQuantityType(comInvOrderLineCollection, comInvOrder);
			comInvOrder.QuantityType = ComInvReconciliationQuantityType.ReceivedQuantity;
			CheckLineUpdateWithParentOrderQuantityType(comInvOrderLineCollection, comInvOrder);
			comInvOrder.QuantityType = ComInvReconciliationQuantityType.OrderQuantity;
			CheckLineUpdateWithParentOrderQuantityType(comInvOrderLineCollection, comInvOrder);
		}

		void CheckLineUpdateWithParentOrderQuantityType(ComInvOrderLineReconciliationCollection collection, ComInvOrderReconciliation parentOrder)
		{
			foreach (ComInvOrderLineReconciliation line in comInvOrderLineCollection)
			{
				AssertEquals(parentOrder.QuantityType, line.QuantityType);
			}
		}

		#endregion

		#region Implementation

		protected override ComInvOrderLineReconciliationCollection GetCollectionToTest()
		{
			ComInvOrderReconciliation parentComInvOrder = Factory.New<ComInvOrderReconciliation>();
			return new ComInvOrderLineReconciliationCollection(parentComInvOrder);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(ComInvOrderLineReconciliation));
		}

		protected override void SetUp()
		{
			base.SetUp();
			comInvOrder = Factory.New<ComInvOrderReconciliation>();
			comInvOrderLineCollection = new ComInvOrderLineReconciliationCollection(comInvOrder);
		}

		ComInvOrderReconciliation comInvOrder;
		ComInvOrderLineReconciliationCollection comInvOrderLineCollection;

		#endregion
	}
}
