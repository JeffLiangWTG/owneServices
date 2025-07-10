using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ImportedOrder))]
	class ImportedOrderTest : ImportedObjectTest
	{
		#region Relationships

		public void TestOrderLines()
		{
			AmendedOrder.OrderLines.DeleteAll();
			OrderLine orderLine1 = AmendedOrder.OrderLines.AddNew();
			OrderLine orderLine2 = AmendedOrder.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			orderLine2.JO_LineNo = 2;
			AssertEquals("Order line 1", "1", AmendedImportedOrder.OrderLines[0].JO_LineNoAndSplitAndSubLine.Value);
			AssertEquals("Order line 2", "2", AmendedImportedOrder.OrderLines[1].JO_LineNoAndSplitAndSubLine.Value);
		}

		public void TestOrderLineWithDeliveries()
		{
			AmendedOrder.OrderLines.DeleteAll();
			AmendedOrder.JD_OrderNumber = "ORD";
			AmendedOrder.JD_OrderNumberSplit = 2;

			OrderLine orderLine1 = AmendedOrder.OrderLines.AddNew();
			orderLine1.JO_LineNo = 1;
			orderLine1.JO_SubLineNo = 2;
			orderLine1.JO_LineSplitNumber = 3;

			OrderLine orderLine2 = AmendedOrder.OrderLines.AddNew();
			orderLine2.JO_LineNo = 2;
			orderLine2.JO_SubLineNo = 2;
			orderLine2.JO_LineSplitNumber = 3;

			OrderLineDelivery delivery1 = orderLine1.Deliveries.AddNew();
			OrderLineDelivery delivery2 = orderLine2.Deliveries.AddNew();
			delivery1.J4_JO = orderLine1.PK;
			delivery2.J4_JO = orderLine2.PK;

			AssertEquals("ORD-2:1.3 sub 2", AmendedImportedOrder.OrderLines[0].Deliveries[0].OrderNumberAndSplitAndLineNoAndSplitAndSubLine);
			AssertEquals("ORD-2:2.3 sub 2", AmendedImportedOrder.OrderLines[1].Deliveries[0].OrderNumberAndSplitAndLineNoAndSplitAndSubLine);
		}

		#endregion

		#region Properties

		public void TestJD_OrderNumberAndSplit_State()
		{
			AssertEquals(ImportedPropertyState.Unchanged, AmendedImportedOrder.JD_OrderNumberAndSplit.State);

			AmendedOrder.JD_OrderNumber = "Modified";
			AssertEquals(ImportedPropertyState.Modified, AmendedImportedOrder.JD_OrderNumberAndSplit.State);

			ImportedOrder newImportedOrder = new ImportedOrder(Factory.New<Order>());
			AssertEquals(ImportedPropertyState.New, newImportedOrder.JD_OrderNumberAndSplit.State);
		}

		public void TestShipmentPreadvice()
		{
			AssertEquals("Empty when no pre-advice is attached", "", AmendedImportedOrder.ShipmentPreadvice.DisplayValue);
			AmendedOrder.JD_EF_ShipmentPrePlanning = Factory.NewWithValidTestData<JobShipmentPreplanning>().PK;
			AmendedOrder.PreAdvice.EF_PreshipID = "PreadviceID";
			AssertEquals("When a pre-advice is attached", "PreadviceID", AmendedImportedOrder.ShipmentPreadvice.DisplayValue);
		}

		public void TestJD_OrderStatus()
		{
			AmendedOrder.JD_OrderStatus = Core.Constants.OrderStatus.Incomplete;
			AssertEquals("Order status should show description", "Incomplete", AmendedImportedOrder.JD_OrderStatus.DisplayValue);
		}

		#endregion

		public void TestEmptyOrderLines()
		{
			Order ordr = Factory.New<Order>();
			ordr.OrderLines.DeleteAll();
			ImportedOrder impOrd = new ImportedOrder(ordr);
			AssertEquals("should create a null order line for Order Import Report", 1, impOrd.OrderLines.Count);
			AssertEquals("should create a null order line for Order Import Report", true, impOrd.OrderLines[0].IsEmpty);
		}

		#region Amended Properties

		public void TestAmendedPropertiesToShowAlways()
		{
			AssertEquals("OrderNumberAndSplit 1st always", AmendedOrder.JD_OrderNumberAndSplitInfo.Name, AmendedImportedOrder.AmendedProperty1.Name);
			AssertEquals("Buyer 2nd always", AmendedOrder.BuyerPKInfo.Name, AmendedImportedOrder.AmendedProperty2.Name);
			AssertEquals("Supplier 3rd always", AmendedOrder.SupplierPKInfo.Name, AmendedImportedOrder.AmendedProperty3.Name);
			AssertEquals("OrderDate 4th always", AmendedOrder.JD_OrderDateInfo.Name, AmendedImportedOrder.AmendedProperty4.Name);
			AssertEquals("IncoTerms 5th always", AmendedOrder.JD_IncoTermInfo.Name, AmendedImportedOrder.AmendedProperty5.Name);
		}

		public void TestAmendedProperties()
		{
			int i = 6;
			TestAmendedProperty(AmendedOrder.JD_BookingConfRefInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_BookingConfDateInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_InvoiceNumberInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_InvoiceDateInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_OrderStatusInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_OrderGoodsDescriptionInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_RX_NKOrderCurrencyInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_TransportModeInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_ContainerModeInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_RN_NKCountryOfSupplyInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_OH_SendingAgentInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_OH_ReceivingAgentInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_RL_NKGoodsAvailableAtInfo, i++);
			TestAmendedProperty(AmendedOrder.JD_DeliveryRequiredByInfo, i++);
		}

		public void TestMilestoneProperties()
		{
			int i = 6;
			TestAmendedMilestone(AmendedOrder, Events.ExWorks, i);
			TestAmendedMilestone(AmendedOrder, Events.DeliveryCartageCompleteFinalised, i);
			TestAmendedMilestone(AmendedOrder, Events.GateIn, i);
			TestAmendedMilestone(AmendedOrder, Events.Departure, i);
			TestAmendedMilestone(AmendedOrder, Events.Arrival, i);
			TestAmendedMilestone(AmendedOrder, Events.CargoAvailable, i);
			TestAmendedMilestone(AmendedOrder, Events.DeliveryCartageAdvised, i);
		}

		void TestAmendedMilestone(Order order, Event eventType, int expectedAmendedPropertyIndex)
		{
			ProcessTask task = order.WorkflowItems.Milestones[eventType];
			TestAmendedProperty(task.P9_ScheduledDateInfo, "E_" + eventType.Code, expectedAmendedPropertyIndex);
			TestAmendedProperty(task.P9_ActualDateInfo, "A_" + eventType.Code, expectedAmendedPropertyIndex + 1);

			order.WorkflowItems.Remove(task);

			string amendedPropertyPropertyName = "AmendedProperty" + expectedAmendedPropertyIndex;
			ImportedProperty amendedProperty = GetAmendedImportedProperty(amendedPropertyPropertyName);
			AssertEquals("Property " + amendedPropertyPropertyName + " null after task removing", null, amendedProperty);

			amendedPropertyPropertyName = "AmendedProperty" + (expectedAmendedPropertyIndex + 1);
			amendedProperty = GetAmendedImportedProperty(amendedPropertyPropertyName);
			AssertEquals("Property " + amendedPropertyPropertyName + " null after task removing", null, amendedProperty);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ImportedOrder(AmendedOrder);
		}

		ImportedOrder AmendedImportedOrder
		{
			get { return new ImportedOrder(AmendedOrder); }
		}

		Order AmendedOrder
		{
			get { return OrdersWithChanges.AmendedOrder; }
		}

		OrdersWithChangesForTest OrdersWithChanges
		{
			get { return ordersWithChanges ?? (ordersWithChanges = new OrdersWithChangesForTest(Factory)); }
		}
		OrdersWithChangesForTest ordersWithChanges;

		#endregion
	}
}
