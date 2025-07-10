using System.Collections.Generic;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	[TestedType(typeof(TransitConsignmentOrderReferenceCollectionReader))]
	public class TransitConsignmentOrderReferenceCollectionReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dcn = Helper.CreateDispatchConsignment("DCN", warehouse.PK);
			var orderNumberCollection = new List<OrderNumber>() { new OrderNumber() { OrderReference = "OrderRef1", Sequence = 1 },
				new OrderNumber() { OrderReference = "OrderRef2", Sequence = 2 } };

			var logger = new TestErrorLogger();
			var reader = new TransitConsignmentOrderReferenceCollectionReader(orderNumberCollection.ToArray(), dcn, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			AssertEquals("Logs",
@"Information - No matching WhsItemConsignmentOrderReference found, creating new WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...
Information - No matching WhsItemConsignmentOrderReference found, creating new WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...".Trim(), logger.Logs);
		}

		#region ReceiveConsignment

		public void TestConsignmentOrderReferenceCollectionReader_EmptyRCNOrderRefs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var orderNumberCollection = new List<OrderNumber>() { new OrderNumber() { OrderReference = "OrderRef1", Sequence = 1 },
				new OrderNumber() { OrderReference = "OrderRef2", Sequence = 2 } };

			var logger = new TestErrorLogger();
			var reader = new TransitConsignmentOrderReferenceCollectionReader(orderNumberCollection.ToArray(), rcn, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			AssertEquals("Consignment must have two consignment order references", 2, rcn.WhsItemConsignmentOrderReferences.Count);
			AssertEquals("Logs",
@"Information - No matching WhsItemConsignmentOrderReference found, creating new WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...
Information - No matching WhsItemConsignmentOrderReference found, creating new WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...".Trim(), logger.Logs);
		}

		public void TestConsignmentOrderReferenceCollectionReaderExistingReferences_ExistingRCNOrderRefs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var orderNumberCollection = new List<OrderNumber>() { new OrderNumber() { OrderReference = "OrderRef1", Sequence = 1 },
				new OrderNumber() { OrderReference = "OrderRef2", Sequence = 2 } };

			Helper.CreateWhsItemConsignmentOrderReference("OrderRef1", rcn);
			Helper.CreateWhsItemConsignmentOrderReference("OrderRef2", rcn);

			var logger = new TestErrorLogger();
			var reader = new TransitConsignmentOrderReferenceCollectionReader(orderNumberCollection.ToArray(), rcn, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			AssertEquals("Consignment must have two consignment order references", 2, rcn.WhsItemConsignmentOrderReferences.Count);
			AssertEquals("Logs",
@"Information - Successfully loaded matching WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...
Information - Successfully loaded matching WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...".Trim(), logger.Logs);
		}

		public void TestConsignmentOrderReferenceCollectionReaderExistingReferences_EmptyRCNOrderRefsDuplicateOrderItems()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var orderNumberCollection = new List<OrderNumber>() { new OrderNumber() { OrderReference = "OrderRef1", Sequence = 1 },
				new OrderNumber() { OrderReference = "OrderRef1", Sequence = 2 } };

			var logger = new TestErrorLogger();
			var reader = new TransitConsignmentOrderReferenceCollectionReader(orderNumberCollection.ToArray(), rcn, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			AssertEquals("Consignment must have one consignment order reference", 1, rcn.WhsItemConsignmentOrderReferences.Count);
			AssertEquals("Logs",
@"Information - No matching WhsItemConsignmentOrderReference found, creating new WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...
Information - Successfully loaded matching WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...".Trim(), logger.Logs);
		}

		#endregion

		#region DispatchConsignment

		public void TestConsignmentOrderReferenceCollectionReader_EmptyDCNOrderRefs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var orderNumberCollection = new List<OrderNumber>() { new OrderNumber() { OrderReference = "OrderRef1", Sequence = 1 },
				new OrderNumber() { OrderReference = "OrderRef2", Sequence = 2 } };

			var logger = new TestErrorLogger();
			var reader = new TransitConsignmentOrderReferenceCollectionReader(orderNumberCollection.ToArray(), dcn, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			AssertEquals("Consignment must have two consignment order references", 2, dcn.WhsItemConsignmentOrderReferences.Count);
			AssertEquals("Logs",
@"Information - No matching WhsItemConsignmentOrderReference found, creating new WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...
Information - No matching WhsItemConsignmentOrderReference found, creating new WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...".Trim(), logger.Logs);
		}

		public void TestConsignmentOrderReferenceCollectionReaderExistingReferences_ExistingDCNOrderRefs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var orderNumberCollection = new List<OrderNumber>() { new OrderNumber() { OrderReference = "OrderRef1", Sequence = 1 },
				new OrderNumber() { OrderReference = "OrderRef2", Sequence = 2 } };

			Helper.CreateWhsItemConsignmentOrderReference("OrderRef1", dcn);
			Helper.CreateWhsItemConsignmentOrderReference("OrderRef2", dcn);

			var logger = new TestErrorLogger();
			var reader = new TransitConsignmentOrderReferenceCollectionReader(orderNumberCollection.ToArray(), dcn, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			AssertEquals("Consignment must have two consignment order references", 2, dcn.WhsItemConsignmentOrderReferences.Count);
			AssertEquals("Logs",
@"Information - Successfully loaded matching WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...
Information - Successfully loaded matching WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...".Trim(), logger.Logs);
		}

		public void TestConsignmentOrderReferenceCollectionReaderExistingReferences_EmptyDCNOrderRefsDuplicateOrderItems()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var orderNumberCollection = new List<OrderNumber>() { new OrderNumber() { OrderReference = "OrderRef1", Sequence = 1 },
				new OrderNumber() { OrderReference = "OrderRef1", Sequence = 2 } };

			var logger = new TestErrorLogger();
			var reader = new TransitConsignmentOrderReferenceCollectionReader(orderNumberCollection.ToArray(), dcn, logger, Factory);

			reader.ReadIntoCollectionRetainingUnmatchedElements();
			Factory.SaveForTesting();

			AssertEquals("Consignment must have one consignment order reference", 1, dcn.WhsItemConsignmentOrderReferences.Count);
			AssertEquals("Logs",
@"Information - No matching WhsItemConsignmentOrderReference found, creating new WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...
Information - Successfully loaded matching WhsItemConsignmentOrderReference.
Information - Populating WhsItemConsignmentOrderReference...".Trim(), logger.Logs);
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;
	}
}
