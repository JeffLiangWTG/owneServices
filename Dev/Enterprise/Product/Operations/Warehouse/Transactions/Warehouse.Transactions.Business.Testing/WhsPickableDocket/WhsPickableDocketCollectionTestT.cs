using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsPickableDocketCollectionTest<T> : WhsDocketCollectionTestCase<T> where T : WhsPickableDocketCollection
	{
		[ExpectExceptionMessage(typeof(ArgumentException), "PickableDocketCollection cannot contain a mix of both Orders and WorkOrders.")]
		public void TestOnlyOneTypeOfObjectsAllowedPerCollection()
		{
			var collection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
			collection.Add(Factory.New<WhsOrder>());
			collection.Add(Factory.New<WhsWorkOrder>());
		}

		public void TestContainsWorkOrders()
		{
			var collection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
			AssertEquals("Precondition", false, collection.ContainsWorkOrders);

			collection.Add(Factory.New<WhsOrder>());
			collection.Add(Factory.New<WhsOrder>());
			AssertEquals(false, collection.ContainsWorkOrders);

			// needed to avoid wrong argument exception
			collection.DeleteAll();

			collection.Add(Factory.New<WhsWorkOrder>());
			collection.Add(Factory.New<WhsWorkOrder>());
			AssertEquals(true, collection.ContainsWorkOrders);
		}

		protected override void TestRelationshipFilterCore()
		{
			if (this.GetType() != typeof(WhsPickableDocketCollectionTest))
			{
				base.TestRelationshipFilterCore();
			}
			else
			{
				var collection1 = (WhsDocketCollection)GetCollectionToTest();

				var whs = Helper.CreateWarehouse("WHS");
				var client = Helper.CreateClient();
				Helper.CreateWhsReceive(client, whs);
				Helper.CreateWhsTransfer(client, whs);
				var order = Helper.CreateWhsOrder(client, whs);
				Helper.CreateWhsAdjustment(client, whs);

				AssertEquals("Should only contain pickable dockets.", 1, collection1.Count);
				Assert(collection1.Contains(order));

				order.Delete();
				var workOrder = Helper.CreateWhsWorkOrder(client, whs);
				var collection2 = (WhsDocketCollection)GetCollectionToTest();
				AssertEquals("Should only contain pickable dockets.", 1, collection2.Count);
				Assert(collection2.Contains(workOrder));
			}
		}

		public void TestSetRelationshipDefaultsForElementCore_DoesNotAddElementIfSuspendAddToCollectionWasCalled()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var randomOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9);
			var pick = Helper.CreatePickByAttachingOrders(order);

			var collection = WhsPickableDocketCollection.GetCollectionFromPick(pick);
			AssertContainsExactElementsInAnyOrder("We should have just the new order we attached to this pick.", new[] { order }, collection);

			var anotherOrder = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 9);

			using (collection.SuspendAddToCollection())
			{
				collection.Add(anotherOrder);
			}

			AssertContainsExactElementsInAnyOrder("If the SuspendAddToCollection is suspended then it should not have added the order to the collection.", new[] { order }, collection);

			collection.Add(anotherOrder);
			AssertContainsExactElementsInAnyOrder("SuspendAddToCollection is not suspended, it should be added the order to the collection.", new[] { order, anotherOrder }, collection);
		}

		#region IOrdersDocumentSupport Members

		public void TestGetPackageLabelDocumentWrappers_Orders()
		{
			var collection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
			IOrdersDocumentSupport documentSupportedCollection = collection;
			AssertEquals("No documents", 0, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);

			collection.Add(Order1);
			AssertEquals("One documents", 1, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);

			collection.Add(Order2);
			AssertEquals("Two documents", 2, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);
		}

		public void TestGetPackageLabelDocumentWrappers_WorkOrders()
		{
			var collection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
			IOrdersDocumentSupport documentSupportedCollection = collection;
			AssertEquals("No documents", 0, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);

			WhsWorkOrder workOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			collection.Add(workOrder);
			AssertEquals("Still no documents", 0, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);
		}

		public void TestGetDeliveryLabelDocumentWrappers_Orders()
		{
			WhsPick pick = Factory.NewWithValidTestData<WhsPick>();
			var collection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
			IOrdersDocumentSupport documentSupportedCollection = collection;
			AssertEquals("No documents", 0, documentSupportedCollection.GetDeliveryLabelDocumentWrappers(pick.CallOnWhsOrderToPrint).Length);

			collection.Add(Order1);
			AssertEquals("One documents", 1, documentSupportedCollection.GetDeliveryLabelDocumentWrappers(pick.CallOnWhsOrderToPrint).Length);

			collection.Add(Order2);
			AssertEquals("Two documents", 2, documentSupportedCollection.GetDeliveryLabelDocumentWrappers(pick.CallOnWhsOrderToPrint).Length);

			Order1.WD_PackagesSent = 0;
			AssertEquals("Simulated change by user setting one order to zero labels", 1, documentSupportedCollection.GetDeliveryLabelDocumentWrappers(pick.CallOnWhsOrderToPrint).Length);
		}

		public void TestGetDeliveryLabelDocumentWrappers_WorkOrders()
		{
			var pick = Factory.NewWithValidTestData<WhsPick>();
			var collection = new WhsPickableDocketCollection(Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
			IOrdersDocumentSupport documentSupportedCollection = collection;
			AssertEquals("No documents", 0, documentSupportedCollection.GetDeliveryLabelDocumentWrappers(pick.CallOnWhsOrderToPrint).Length);

			var workOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			collection.Add(workOrder);
			AssertEquals("Still no documents", 0, documentSupportedCollection.GetDeliveryLabelDocumentWrappers(pick.CallOnWhsOrderToPrint).Length);
		}

		#endregion

		#region Implementation

		protected override bool AllowNew
		{
			get { return false; }
		}

		WhsOrder Order1
		{
			get
			{
				if (order1 == null)
				{
					order1 = Factory.NewWithValidTestData<WhsOrder>();
					order1.WD_PackagesSent = 10;

					WhsOrderLine line = order1.Lines.AddNew();
					line.WE_PackQuantity = 10;
				}
				return order1;
			}
		}
		WhsOrder order1;

		WhsOrder Order2
		{
			get
			{
				if (order2 == null)
				{
					order2 = Factory.NewWithValidTestData<WhsOrder>();
					order2.WD_PackagesSent = 10;

					WhsOrderLine line = order2.Lines.AddNew();
					line.WE_PackQuantity = 10;
				}
				return order2;
			}
		}
		WhsOrder order2;

		#endregion
	}
}
