using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.DataTransfer.Universal;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderNumberCollectionReader))]
	public class OrderNumberCollectionReaderTest : DataObjectCollectionReaderTest
	{
		#region CompleteCollection

		public override void TestReadIntoCollection()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject1 = new OrderNumber { Sequence = 1, OrderReference = "FF" };
			var orderNumberDataObject2 = new OrderNumber { Sequence = 2, OrderReference = "AA" };
			var orderNumberDataObject = new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2 });
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);

			var reader = new OrderNumberCollectionReader(orderNumberDataObject, jobDocsAndCartage, logger, Factory);
			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
				AssertEquals("FF", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
				AssertEquals("1", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
				AssertEquals("AA", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
				AssertEquals("2", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
				AssertEquals("Logs",
	@"Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoCollectionNoSequenceCompleteCollectionNonExistingOrderItems()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject1 = new OrderNumber { OrderReference = "FF" };
			var orderNumberDataObject2 = new OrderNumber { OrderReference = "AA" };
			var orderNumberDataObject = new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2 });
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);

			var reader = new OrderNumberCollectionReader(orderNumberDataObject, jobDocsAndCartage, logger, Factory);
			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
				AssertEquals("FF", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
				AssertEquals("0", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
				AssertEquals("AA", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
				AssertEquals("0", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
				AssertEquals("Logs",
	@"Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoCollectionNoSequenceCompleteCollectionExistingOrderItems()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject1 = new OrderNumber { OrderReference = "DD" };
			var orderNumberDataObject2 = new OrderNumber { OrderReference = "EE" };
			var orderNumberDataObject = new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2 });
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);

			var item1 = shipmentBO.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "AA";
			item1.JT_Sequence = 1;
			var item2 = shipmentBO.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "FF";
			item2.JT_Sequence = 2;

			var reader = new OrderNumberCollectionReader(orderNumberDataObject, jobDocsAndCartage, logger, Factory);
			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
				AssertEquals("DD", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
				AssertEquals("0", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
				AssertEquals("EE", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
				AssertEquals("0", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
				AssertEquals("Logs",
	@"Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...".Trim(), logger.Logs);
			});
		}

		#endregion CompleteCollection

		#region PartialCollection

		public void TestReadIntoCollectionNoSequencePartialCollectionNoExistingOrderItems()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject1 = new OrderNumber { OrderReference = "FF" };
			var orderNumberDataObject2 = new OrderNumber { OrderReference = "AA" };
			var orderNumberDataObject = new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2 });
			orderNumberDataObject.Content = CollectionContent.Partial;
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);

			var reader = new OrderNumberCollectionReader(orderNumberDataObject, jobDocsAndCartage, logger, Factory);
			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
				AssertEquals("FF", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
				AssertEquals("0", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
				AssertEquals("AA", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
				AssertEquals("0", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
				AssertEquals("Logs",
	@"Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoCollectionNoSequencePartialCollectionExistingOrderItems()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject1 = new OrderNumber { OrderReference = "FF" };
			var orderNumberDataObject2 = new OrderNumber { OrderReference = "AA" };
			var orderNumberDataObject = new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2 });
			orderNumberDataObject.Content = CollectionContent.Partial;

			var item1 = shipmentBO.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "AA";
			item1.JT_Sequence = 1;
			var item2 = shipmentBO.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "FF";
			item2.JT_Sequence = 2;
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);

			var reader = new OrderNumberCollectionReader(orderNumberDataObject, jobDocsAndCartage, logger, Factory);
			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
				AssertEquals("AA", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
				AssertEquals("1", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
				AssertEquals("FF", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
				AssertEquals("2", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
				AssertEquals("Logs", "", logger.Logs);
			});
		}

		public void TestReadIntoCollectionNoSequencePartialCollectionExistingOrderItemsAdditional()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject1 = new OrderNumber { OrderReference = "CC" };
			var orderNumberDataObject2 = new OrderNumber { OrderReference = "DD" };
			var orderNumberDataObject = new DataObjectList<OrderNumber>(new[] { orderNumberDataObject1, orderNumberDataObject2 });
			orderNumberDataObject.Content = CollectionContent.Partial;

			var item1 = shipmentBO.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "AA";
			item1.JT_Sequence = 1;
			var item2 = shipmentBO.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "FF";
			item2.JT_Sequence = 2;
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);

			var reader = new OrderNumberCollectionReader(orderNumberDataObject, jobDocsAndCartage, logger, Factory);
			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals(4, jobDocsAndCartage.OrderItems.Count);
				AssertEquals("AA", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
				AssertEquals("1", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
				AssertEquals("FF", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
				AssertEquals("2", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
				AssertEquals("CC", jobDocsAndCartage.OrderItems[2].JT_OrderReference);
				AssertEquals("0", jobDocsAndCartage.OrderItems[2].JT_Sequence.ToString());
				AssertEquals("DD", jobDocsAndCartage.OrderItems[3].JT_OrderReference);
				AssertEquals("0", jobDocsAndCartage.OrderItems[3].JT_Sequence.ToString());
				AssertEquals("Logs",
	@"Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...
Information - No matching OrderItem found, creating new OrderItem.
Information - Populating OrderItem...".Trim(), logger.Logs);
			});
		}

		public void TestReadIntoCollectionNoSequencePartialCollectionExistingOrderItemsEmptyOrderNumberCollection()
		{
			var shipmentBO = Factory.NewWithValidTestData<ForwardingShipment>();
			var orderNumberDataObject = new DataObjectList<OrderNumber>();
			orderNumberDataObject.Content = CollectionContent.Partial;

			var item1 = shipmentBO.DocsAndCartage.OrderItems.AddNew();
			item1.JT_OrderReference = "AA";
			item1.JT_Sequence = 1;
			var item2 = shipmentBO.DocsAndCartage.OrderItems.AddNew();
			item2.JT_OrderReference = "FF";
			item2.JT_Sequence = 2;
			var jobDocsAndCartage = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipmentBO);

			var reader = new OrderNumberCollectionReader(orderNumberDataObject, jobDocsAndCartage, logger, Factory);
			reader.ReadIntoCollection();

			CombineAssertions(() =>
			{
				AssertEquals(2, jobDocsAndCartage.OrderItems.Count);
				AssertEquals("AA", jobDocsAndCartage.OrderItems[0].JT_OrderReference);
				AssertEquals("1", jobDocsAndCartage.OrderItems[0].JT_Sequence.ToString());
				AssertEquals("FF", jobDocsAndCartage.OrderItems[1].JT_OrderReference);
				AssertEquals("2", jobDocsAndCartage.OrderItems[1].JT_Sequence.ToString());
				AssertEquals("Logs", "", logger.Logs);
			});
		}

		#endregion PartialCollection

		TestErrorLogger logger;

		protected override void SetUp()
		{
			logger = new TestErrorLogger();
			base.SetUp();
		}
	}
}
