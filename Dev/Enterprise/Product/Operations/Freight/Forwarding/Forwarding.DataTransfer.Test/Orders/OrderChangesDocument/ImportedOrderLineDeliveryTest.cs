using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ImportedOrderLineDelivery))]
	class ImportedOrderLineDeliveryTest : ImportedObjectTest
	{
		public void TestCreateNullDelivery()
		{
			ImportedOrderLine orderLine = new ImportedOrderLine(new ImportedOrder(OrdersWithChanges.AmendedOrder), OrdersWithChanges.AmendedOrderLine);
			ImportedOrderLineDelivery delivery = new ImportedOrderLineDelivery(orderLine, Factory.GetNull<OrderLineDelivery>());
			AssertEquals("Empty delivery created. This property's ToString() must return 'Y' for document engine.", "Y", delivery.IsEmpty.ToString());
		}

		public void TestOrderNumberAndSplitAndLineNoAndSplitAndSubLine()
		{
			AmendedOrderLineDelivery.OrderLine.Order.JD_OrderNumber = "OrderNumber";
			AmendedOrderLineDelivery.OrderLine.Order.JD_OrderNumberSplit = 2;
			AmendedOrderLineDelivery.OrderLine.JO_LineNo = 1;
			AmendedOrderLineDelivery.OrderLine.JO_SubLineNo = 2;
			AmendedOrderLineDelivery.OrderLine.JO_LineSplitNumber = 3;
			AssertEquals("OrderNumber-2:1.3 sub 2", AmendedImportedOrderLineDelivery.OrderNumberAndSplitAndLineNoAndSplitAndSubLine);
		}

		#region Amended Properties

		public void TestAmendedPropertiesToShowAlways()
		{
			AssertEquals("J4_RL_NKDestinationPort 1st always", AmendedOrderLineDelivery.J4_RL_NKDestinationPortInfo.Name, AmendedImportedOrderLineDelivery.AmendedProperty1.Name);
			AssertEquals("J4_OA_NKDeliveryPoint 2nd always", AmendedOrderLineDelivery.J4_OA_NKDeliveryPointInfo.Name, AmendedImportedOrderLineDelivery.AmendedProperty2.Name);
			AssertEquals("J4_Allocated 3rd always", AmendedOrderLineDelivery.J4_AllocatedInfo.Name, AmendedImportedOrderLineDelivery.AmendedProperty3.Name);
		}

		public void TestAmendedPropertiesToShowAlways_ForEmptyDelivery()
		{
			ImportedOrderLine orderLine = new ImportedOrderLine(new ImportedOrder(OrdersWithChanges.AmendedOrder), OrdersWithChanges.AmendedOrderLine);
			ImportedOrderLineDelivery emptyDelivery = new ImportedOrderLineDelivery(orderLine, Factory.GetNull<OrderLineDelivery>());
			AssertEquals("Don't ever show any delivery properties on an empty delivery, otherwise conditional formatting will show borders incorrectly.", 0, emptyDelivery.PropertiesToShowAlways.Count);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ImportedOrderLine orderLine = new ImportedOrderLine(new ImportedOrder(AmendedOrderLineDelivery.Order), AmendedOrderLineDelivery.OrderLine);
			return new ImportedOrderLineDelivery(orderLine, AmendedOrderLineDelivery);
		}

		ImportedOrderLineDelivery AmendedImportedOrderLineDelivery
		{
			get { return amendedImportedOrderLineDelivery ?? (amendedImportedOrderLineDelivery = (ImportedOrderLineDelivery)GetNewBusinessObject()); }
		}
		ImportedOrderLineDelivery amendedImportedOrderLineDelivery;

		OrderLineDelivery AmendedOrderLineDelivery
		{
			get
			{
				if (amendedOrderLineDelivery == null)
				{
					amendedOrderLineDelivery = OrdersWithChanges.AmendedOrderLine.Deliveries.AddNew();
				}
				return amendedOrderLineDelivery;
			}
		}
		OrderLineDelivery amendedOrderLineDelivery;

		OrdersWithChangesForTest OrdersWithChanges
		{
			get { return ordersWithChanges ?? (ordersWithChanges = new OrdersWithChangesForTest(Factory)); }
		}
		OrdersWithChangesForTest ordersWithChanges;

		#endregion
	}
}
