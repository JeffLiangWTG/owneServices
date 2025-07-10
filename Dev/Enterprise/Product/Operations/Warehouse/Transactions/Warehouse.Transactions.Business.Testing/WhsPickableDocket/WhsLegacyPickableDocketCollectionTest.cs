using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLegacyPickableDocketCollection))]
	public class WhsLegacyPickableDocketCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		[ExpectExceptionMessage(typeof(ArgumentException), "WhsLegacyPickableDocketCollection cannot contain a mix of both Orders and WorkOrders.")]
		public void TestOnlyOneTypeOfObjectsAllowedPerCollection()
		{
			WhsLegacyPickableDocketCollection collection = (WhsLegacyPickableDocketCollection)GetCollectionToTest();
			collection.Add(Factory.New<WhsOrder>());
			collection.Add(Factory.New<WhsWorkOrder>());
			collection.Add(Factory.New<WhsTransfer>());
		}

		public void TestContainsWorkOrders()
		{
			WhsLegacyPickableDocketCollection collection = (WhsLegacyPickableDocketCollection)GetCollectionToTest();
			AssertEquals("Precondition", false, collection.ContainsWorkOrders);

			collection.Add(Factory.New<WhsOrder>());
			collection.Add(Factory.New<WhsOrder>());
			AssertEquals(false, collection.ContainsWorkOrders);

			// needed to avoid wrong argument exception
			collection.RemoveAndDeleteAll();

			collection.Add(Factory.New<WhsWorkOrder>());
			collection.Add(Factory.New<WhsWorkOrder>());
			AssertEquals(true, collection.ContainsWorkOrders);
		}

		#region TestSetAllowNew

		public void TestSetAllowNew()
		{
			var collection = (WhsLegacyPickableDocketCollection)GetCollectionToTest();

			AssertEquals("Collection.AllowNewCore should return false if SetAllowNew(bool) was not called.",
				false, collection.AllowNew);

			collection.SetAllowNew(true);
			AssertEquals(true, collection.AllowNew);

			collection.SetAllowNew(false);
			AssertEquals(false, collection.AllowNew);
		}

		#endregion

		#region IOrdersDocumentSupport Members

		public void TestGetPackageLabelDocumentWrappers_Orders()
		{
			WhsLegacyPickableDocketCollection collection = new WhsLegacyPickableDocketCollection(Factory);
			IOrdersDocumentSupport documentSupportedCollection = collection;
			AssertEquals("No documents", 0, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);

			collection.Add(Order1);
			AssertEquals("One documents", 1, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);

			collection.Add(Order2);
			AssertEquals("Two documents", 2, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);
		}

		public void TestGetPackageLabelDocumentWrappers_WorkOrders()
		{
			WhsLegacyPickableDocketCollection collection = new WhsLegacyPickableDocketCollection(Factory);
			IOrdersDocumentSupport documentSupportedCollection = collection;
			AssertEquals("No documents", 0, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);

			WhsWorkOrder workOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			collection.Add(workOrder);
			AssertEquals("Still no documents", 0, documentSupportedCollection.GetPackageLabelDocumentWrappers().Length);
		}

		public void TestGetDeliveryLabelDocumentWrappers_Orders()
		{
			WhsPick pick = Factory.NewWithValidTestData<WhsPick>();
			WhsLegacyPickableDocketCollection collection = new WhsLegacyPickableDocketCollection(Factory);
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
			WhsPick pick = Factory.NewWithValidTestData<WhsPick>();
			WhsLegacyPickableDocketCollection collection = new WhsLegacyPickableDocketCollection(Factory);
			IOrdersDocumentSupport documentSupportedCollection = collection;
			AssertEquals("No documents", 0, documentSupportedCollection.GetDeliveryLabelDocumentWrappers(pick.CallOnWhsOrderToPrint).Length);

			WhsWorkOrder workOrder = Factory.NewWithValidTestData<WhsWorkOrder>();
			collection.Add(workOrder);
			AssertEquals("Still no documents", 0, documentSupportedCollection.GetDeliveryLabelDocumentWrappers(pick.CallOnWhsOrderToPrint).Length);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsLegacyPickableDocketCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<WhsOrder>();
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
