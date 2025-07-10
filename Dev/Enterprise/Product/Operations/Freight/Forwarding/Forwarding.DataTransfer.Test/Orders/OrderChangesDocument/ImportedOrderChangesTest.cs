using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ImportedOrderChanges))]
	class ImportedOrderChangesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNewOrders()
		{
			AssertEquals(1, OrderChanges.NewOrders.Count);
			AssertEquals(OrdersWithChanges.NewOrder.JD_OrderNumberAndSplit, OrderChanges.NewOrders[0].JD_OrderNumberAndSplit.Value);
		}

		public void TestAmendedOrders()
		{
			AssertEquals(1, OrderChanges.AmendedOrders.Count);
			AssertEquals(OrdersWithChanges.AmendedOrder.JD_OrderNumberAndSplit, OrderChanges.AmendedOrders[0].JD_OrderNumberAndSplit.Value);
		}

		public void TestUnchangedOrders()
		{
			AssertEquals(1, OrderChanges.UnchangedOrders.Count);
			AssertEquals(OrdersWithChanges.UnchangedOrder.JD_OrderNumberAndSplit, OrderChanges.UnchangedOrders[0].JD_OrderNumberAndSplit.Value);
		}

		public void TestCancelledOrders()
		{
			AssertEquals(1, OrderChanges.CancelledOrders.Count);
			AssertEquals(OrdersWithChanges.CancelledOrder.JD_OrderNumberAndSplit, OrderChanges.CancelledOrders[0].JD_OrderNumberAndSplit.Value);
		}

		public void TestAttachedOrders()
		{
			AssertEquals(1, OrderChanges.AttachedOrders.Count);
			AssertEquals(OrdersWithChanges.AttachedOrder.JD_OrderNumberAndSplit, OrderChanges.AttachedOrders[0].JD_OrderNumberAndSplit.Value);
		}

		public void TestAttachedOrderLineDeliveries()
		{
			Order order = OrdersWithChanges.AttachedOrder;
			order.OrderLines.DeleteAll();

			OrderLine orderLineWithoutDeliveries = order.OrderLines.AddNew();
			OrderLine orderLineWithDeliveries = order.OrderLines.AddNew();
			orderLineWithDeliveries.Deliveries.AddNew();

			AssertEquals(
				"To allow master-detail for Order->OrderLines->Deliveries by flattening, each order line must " +
				"have a delivery. 1 real delivery, 1 'empty' delivery.", 2, OrderChanges.AttachedOrderLineDeliveries.Count);
		}

		public void TestAmendedOrderLineDeliveries()
		{
			Order order = OrdersWithChanges.AmendedOrder;
			OrdersWithChanges.AmendedOrder.OrderLines.DeleteAll();

			OrderLine orderLineWithoutDeliveries = order.OrderLines.AddNew();
			OrderLine orderLineWithDeliveries = order.OrderLines.AddNew();
			orderLineWithDeliveries.Deliveries.AddNew();

			AssertEquals(
				"To allow master-detail for Order->OrderLines->Deliveries by flattening, each order line must " +
				"have a delivery. 1 real delivery, 1 'empty' delivery.", 2, OrderChanges.AmendedOrderLineDeliveries.Count);
		}

		public void TestGetImportedOrderState()
		{
			AssertEquals("Order state - New order", ImportedOrderState.New, new ImportedOrder(OrdersWithChanges.NewOrder).OrderState);
			AssertEquals("Order state - Amended order", ImportedOrderState.Amended, new ImportedOrder(OrdersWithChanges.AmendedOrder).OrderState);
			AssertEquals("Order state - Unchanged order", ImportedOrderState.Unchanged, new ImportedOrder(OrdersWithChanges.UnchangedOrder).OrderState);
			AssertEquals("Order state - Cancelled order", ImportedOrderState.Cancelled, new ImportedOrder(OrdersWithChanges.CancelledOrder).OrderState);
			AssertEquals("Order state - Attached order", ImportedOrderState.Attached, new ImportedOrder(OrdersWithChanges.AttachedOrder).OrderState);
		}

		public void TestImportedOrderChangesWithOrderState()
		{
			Order[] orders = new Order[]
			{
				OrdersWithChanges.NewOrder,
				OrdersWithChanges.AmendedOrder,
				OrdersWithChanges.UnchangedOrder,
				OrdersWithChanges.CancelledOrder,
				OrdersWithChanges.AttachedOrder,
			};

			List<ImportedOrder> importedOrders = new List<ImportedOrder>();
			foreach (Order order in orders)
			{
				importedOrders.Add(new ImportedOrder(order));
			}

			ImportedOrderChanges orderChangesWithOrderState = new ImportedOrderChanges(importedOrders);
			AssertEquals("ImportedOrderChanges created with 1 NewOrders", 1, orderChangesWithOrderState.NewOrders.Count);
			AssertEquals("ImportedOrderChanges created with 1 AmendedOrder1", 1, orderChangesWithOrderState.AmendedOrders.Count);
			AssertEquals("ImportedOrderChanges created with 1 UnchangedOrder", 1, orderChangesWithOrderState.UnchangedOrders.Count);
			AssertEquals("ImportedOrderChanges created with 1 CancelledOrder", 1, orderChangesWithOrderState.CancelledOrders.Count);
			AssertEquals("ImportedOrderChanges created with 1 AttachedOrder", 1, orderChangesWithOrderState.AttachedOrders.Count);
		}

		public void TestHasChangesToReport()
		{
			var orderChanges = OrderChangesWithOnly(OrdersWithChanges.NewOrder);
			Assert(orderChanges.HasChangesToReport);

			orderChanges = OrderChangesWithOnly(OrdersWithChanges.AttachedOrder);
			OrdersDataRegistry.Instance.IncludeChangesNotApplied.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!orderChanges.HasChangesToReport);
			OrdersDataRegistry.Instance.IncludeChangesNotApplied.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(orderChanges.HasChangesToReport);
		}

		ImportedOrderChanges OrderChangesWithOnly(Order order)
		{
			var importedOrders = new List<ImportedOrder>();
			importedOrders.Add(new ImportedOrder(order));
			return new ImportedOrderChanges(importedOrders);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			Order[] orders = new Order[]
			{
				OrdersWithChanges.NewOrder,
				OrdersWithChanges.AmendedOrder,
				OrdersWithChanges.UnchangedOrder,
				OrdersWithChanges.CancelledOrder,
				OrdersWithChanges.AttachedOrder,
			};

			List<ImportedOrder> importedOrders = new List<ImportedOrder>();
			foreach (Order order in orders)
			{
				importedOrders.Add(new ImportedOrder(order));
			}

			return new ImportedOrderChanges(importedOrders);
		}

		ImportedOrderChanges OrderChanges
		{
			get
			{
				if (orderChanges == null)
				{
					orderChanges = (ImportedOrderChanges)GetNewBusinessObject();
				}
				return orderChanges;
			}
		}
		ImportedOrderChanges orderChanges;

		OrdersWithChangesForTest OrdersWithChanges
		{
			get { return ordersWithChanges ?? (ordersWithChanges = new OrdersWithChangesForTest(Factory)); }
		}
		OrdersWithChangesForTest ordersWithChanges;

		#endregion
	}
}
