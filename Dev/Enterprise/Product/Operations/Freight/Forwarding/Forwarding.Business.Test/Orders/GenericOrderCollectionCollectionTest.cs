using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(GenericOrderCollection))]
	sealed class GenericOrderCollectionCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestLoad()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var collection = new GenericOrderCollection(Factory, shipment);

			var order = Factory.NewWithValidTestData<Order>();
			var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();

			shipment.AttachedOrders.Add(order);
			shipment.AttachedWarehouseOrders.Add(whsOrder);

			collection.Load();
			AssertEquals(2, collection.Count);
		}

		public void TestShouldNotValidateAttachedOrdersWhenUserCanNotAccess()
		{
			Env.Security.WhsOrder.IsAllowed = true;
			Env.Security.OrderManager.IsAllowed = true;

			try
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var collection = new GenericOrderCollection(Factory, shipment);

				var order = Factory.NewWithValidTestData<Order>();
				var whsOrder = (BusinessObject)Factory.New<IWhsOrder>();

				AssertEquals("Should default to false", false, ((IAttachedOrder)order).ShouldSkipAllValidations);
				AssertEquals("Should default to false", false, ((IAttachedOrder)whsOrder).ShouldSkipAllValidations);

				collection.Add(order);
				collection.Add(whsOrder);

				AssertEquals(1, shipment.AttachedOrders.Count);
				AssertEquals(1, shipment.AttachedWarehouseOrders.Count);

				AssertEquals("The order is attached but user can access the order manager section", false, ((IAttachedOrder)order).ShouldSkipAllValidations);
				AssertEquals("The order is attached but user can access the warehouse order section", false, ((IAttachedOrder)whsOrder).ShouldSkipAllValidations);

				Env.Security.WhsOrder.IsAllowed = false;
				Env.Security.OrderManager.IsAllowed = false;

				AssertEquals("Should be true when the order is attached and user can not access the order manager section", true, ((IAttachedOrder)order).ShouldSkipAllValidations);
				AssertEquals("Should be true when the order is attached and user can not access the warehouse order section", true, ((IAttachedOrder)whsOrder).ShouldSkipAllValidations);
			}
			finally
			{
				Env.Security.WhsOrder.IsAllowed = true;
				Env.Security.OrderManager.IsAllowed = true;
			}
		}

		public void TestBuildCollection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var order = shipment.AttachedOrders.AddNew();
			var warehouseOrder = (BusinessObject)Factory.New<IWhsOrder>();
			shipment.AttachedWarehouseOrders.Add(warehouseOrder);

			var collection = new GenericOrderCollection(Factory, shipment);
			AssertContainsExactElementsInAnyOrder(Array.Empty<BusinessObject>(), collection);

			collection.BuildCollection();
			AssertContainsExactElementsInAnyOrder(new[] { order, warehouseOrder }, collection);
		}

		public void TestAttachedOrdersFromSupplierBookingWhileConvertedToShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var order = Factory.NewWithValidTestData<Order>();
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var containerLoadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
			var packLine = shipment.OuterPackLines.AddNew();
			var containerLoadPlanLine = containerLoadPlan.LoadListLines.AddNew();
			var supplierBookingLine = supplierBooking.SupplierBookingLines.AddNew();
			var orderLine = order.OrderLines.AddNew();
			supplierBookingLine.JSL_JO_OrderLine = orderLine.PK;
			containerLoadPlanLine.CLL_JSL_BookingLine = supplierBookingLine.PK;
			containerLoadPlanLine.CLL_JL_PackLine = packLine.PK;
			Factory.Save();

			AssertEquals(order.PK, shipment.AttachedOrdersFromSupplierBooking.Single().PK);
		}

		public void TestSetDefaultsForNewChild()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = "ROA";

			var collection = new GenericOrderCollection(Factory, shipment);
			var order = Factory.New<Order>();
			collection.SetupNewElementButDoNotAddIt(order, true);
			AssertEquals("ROA", order.JD_TransportMode);
		}

		public void TestAddToAndRemoveFromCollection()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals("Precondition", 0, shipment.AttachedOrders.Count);
			AssertEquals("Precondition", 0, shipment.AttachedWarehouseOrders.Count);

			var collection = new GenericOrderCollection(Factory, shipment);
			var order = Factory.New<Order>();
			collection.Add(order);
			AssertContainsExactElementsInAnyOrder(new[] { order }, shipment.AttachedOrders);

			var warehouseOrder = (BusinessObject)Factory.New<IWhsOrder>();
			collection.Add(warehouseOrder);
			AssertContainsExactElementsInAnyOrder(new[] { warehouseOrder }, shipment.AttachedWarehouseOrders);

			collection.Remove(order);
			AssertEquals(0, shipment.AttachedOrders.Count);

			collection.Remove(warehouseOrder);
			AssertEquals(0, shipment.AttachedWarehouseOrders.Count);
		}

		public void TestRemoveAttachedOrderFromSubCollection()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var order1 = Factory.New<Order>();
			shipment.GenericOrders.Add(order1);

			var order2 = Factory.New<Order>();
			shipment.GenericOrders.Add(order2);

			AssertContainsExactElementsInAnyOrder(new[] { order1, order2 }, shipment.GenericOrders);
			AssertContainsExactElementsInAnyOrder(new[] { order1, order2 }, shipment.AttachedOrders);

			shipment.AttachedOrders.RemoveFromRelationship(order1);

			AssertContainsExactElementsInAnyOrder(new[] { order2 }, shipment.GenericOrders);
			AssertContainsExactElementsInAnyOrder(new[] { order2 }, shipment.AttachedOrders);

			shipment.AttachedOrders.RemoveFromRelationship(order2);

			AssertEquals(0, shipment.GenericOrders.Count);
			AssertEquals(0, shipment.AttachedOrders.Count);
		}

		public void TestGetTypeOfElementsFromPK()
		{
			var collection = new GenericOrderCollection(Factory, Factory.New<ForwardingShipment>());
			IBusinessObjectCollection businessObjectCollection = collection;

			AssertExceptionThrown(typeof(NotSupportedException), () => businessObjectCollection.GetTypeOfElementsFromPK(ZGuid.Empty));

			collection.CurrentModuleID = ModuleId.Orders;
			AssertEquals(typeof(Order), businessObjectCollection.GetTypeOfElementsFromPK(ZGuid.Empty));

			collection.CurrentModuleID = ModuleId.WhsOrder;
			AssertEquals(ObjectFactory.GetType<IWhsOrder>(), businessObjectCollection.GetTypeOfElementsFromPK(ZGuid.Empty));
		}

		public void TestCurrentBindingList()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var collection = new GenericOrderCollection(Factory, shipment);
			IBusinessObjectCollection bindingList = null;
			AssertExceptionThrown(typeof(NotSupportedException), () => bindingList = collection.CurrentBindingList);
			AssertNull(bindingList);

			collection.CurrentModuleID = ModuleId.WhsOrder;
			bindingList = collection.CurrentBindingList;
			AssertEquals(ObjectFactory.GetType<IWhsOrderCollection>(), bindingList.GetType());

			var filterBusinessObjectDefaultsProvider = (IFilterBusinessObjectDefaultsProvider)bindingList;
			var clientFilterDefault = filterBusinessObjectDefaultsProvider.FilterBusinessObjectDefaults["Client:Property"];
			AssertEquals(shipment.ConsignorPK, clientFilterDefault.Value);

			var consigneeFilterDefault = filterBusinessObjectDefaultsProvider.FilterBusinessObjectDefaults["Consignee:Property"];
			AssertEquals(shipment.ConsigneePK, consigneeFilterDefault.Value);

			var orderStatus = filterBusinessObjectDefaultsProvider.FilterBusinessObjectDefaults["Order Status:Property"];
			AssertEquals("FIN", orderStatus.Value);

			collection.CurrentModuleID = ModuleId.Orders;
			bindingList = collection.CurrentBindingList;
			AssertEquals(typeof(OrderCollection), bindingList.GetType());
		}

		public void TestOrderLinkage_ShipmentInDatabase()
		{
			var shipment = Factory.New<ForwardingShipment>();
			Factory.Save();

			var order = Factory.New<Order>();
			shipment.GenericOrders.SetupNewElementButDoNotAddIt(order, true);

			AssertEquals("Order should be linked", shipment.PK, order.JD_JS);
		}

		public void TestOrderLinkage_ShipmentNotInDatabase()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var order = Factory.New<Order>();
			shipment.GenericOrders.SetupNewElementButDoNotAddIt(order, true);

			Assert("Order should not be linked", order.JD_JS.IsEmpty);
		}

		public void TestIndexer()
		{
			var collection = new GenericOrderCollection(Factory, Factory.New<ForwardingShipment>());
			collection.Add(Factory.New<Order>());
			AssertNotNull(collection[0]);
		}

		public void TestAllowNew()
		{
			var collection = new GenericOrderCollection(Factory, Factory.New<ForwardingShipment>());
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAddingNonIAttachedOrderObjectThrowsException()
		{
			var collection = new GenericOrderCollection(Factory, Factory.New<ForwardingShipment>());
			AssertExceptionThrown(typeof(ArgumentException),
				"The BusinessObject 'DummyBusinessObject' added to GenericOrderCollection must implement IAttachedOrder.",
				() => collection.Add(Factory.New<DummyBusinessObject>()));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GenericOrderCollection(Factory, Factory.New<ForwardingShipment>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DummyAttachedOrderBusinessObject>();
		}

		#endregion
	}
}
