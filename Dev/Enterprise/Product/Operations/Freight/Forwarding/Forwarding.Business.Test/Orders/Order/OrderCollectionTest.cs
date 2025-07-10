using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderCollection))]
	sealed class OrderCollectionTest : ActiveBusinessObjectCollectionTestCase<OrderCollection>
	{
		public void TestNewOrderDefaultsFromUnsavedPreAdvice()
		{
			var preAdvice = Factory.NewWithValidTestData<PreAdviceForTesting>();
			var collection = new OrderCollection(preAdvice, new ZQuery());
			preAdvice.EF_JS = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			Factory.Save();

			preAdvice.EF_JS = ZGuid.Empty;
			var order = collection.AddNew();
			AssertEquals(ZGuid.Empty, order.JD_JS);
		}

		public void TestLoading_NoParent()
		{
			Factory.NewWithValidTestData<Order>().JD_OrderNumber = "AAA";
			Factory.NewWithValidTestData<Order>().JD_OrderNumber = "ABC";
			Factory.NewWithValidTestData<Order>().JD_OrderNumber = "BBB";

			OrderCollection collection = new OrderCollection(Factory);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "ABC", "BBB" }, collection.Select(order => order.JD_OrderNumber));

			Order pirateOrder = collection.AddNew();
			pirateOrder.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			pirateOrder.JD_OrderNumber = "Arrgh!";

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "ABC", "BBB", "Arrgh!" }, collection.Select(order => order.JD_OrderNumber));

			collection = new OrderCollection(Factory, new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.StartsWith, "A"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "ABC", "Arrgh!" }, collection.Select(order => order.JD_OrderNumber));

			collection = new OrderCollection(Factory, new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, "BBB"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "BBB" }, collection.Select(order => order.JD_OrderNumber));

			Factory.Save();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();

			collection = new OrderCollection(anotherFactory, new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.StartsWith, "A"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "ABC", "Arrgh!" }, collection.Select(order => order.JD_OrderNumber));

			collection = new OrderCollection(anotherFactory, new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, "BBB"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "BBB" }, collection.Select(order => order.JD_OrderNumber));
		}

		public void TestLoading()
		{
			ShipmentForTesting shipment = Factory.New<ShipmentForTesting>();
			OrderCollection collection = new OrderCollection(Factory, shipment);

			Order order1 = collection.AddNew();
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Order order2 = collection.AddNew();
			order2.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Order standAloneOrder = Factory.NewWithValidTestData<Order>();

			Factory.Save();
			AssertContainsExactElementsInAnyOrder("Precondition", new Order[] { order1, order2 }, collection);

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			shipment = anotherFactory.Load<ShipmentForTesting>(shipment.PK);

			AssertEquals("Precondition", false, shipment.SetDefaultsOnOrderCalled);
			AssertEquals("Precondition", false, shipment.OnOrderAttachedCalled);

			collection = new OrderCollection(anotherFactory, shipment);
			AssertContainsExactElementsInAnyOrder("All elements loaded", new ZGuid[] { order1.PK, order2.PK }, collection.Select(order => order.PK));

			AssertEquals("Method shouldn't be called on loading", false, shipment.SetDefaultsOnOrderCalled);
			AssertEquals("Method shouldn't be  called on loading", false, shipment.OnOrderAttachedCalled);

			BusinessObject invoiceHeaderBO = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			var orderPivot1 = Factory.New<GenPivot>();
			orderPivot1.XX_Relation1ID = invoiceHeaderBO.PK;
			orderPivot1.XX_Relation2ID = order1.PK;

			var orderPivot2 = Factory.New<GenPivot>();
			orderPivot2.XX_Relation1ID = invoiceHeaderBO.PK;
			orderPivot2.XX_Relation2ID = order2.PK;

			collection = new OrderCollection(invoiceHeaderBO);
			AssertContainsExactElementsInAnyOrder("All elements loaded", new ZGuid[] { order1.PK, order2.PK }, collection.Select(order => order.PK));
		}

		public void TestRemoveFromRelationshipAndDelete()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			OrderCollection collection = new OrderCollection(Factory, shipment);
			Order order1 = collection.AddNew();
			Order order2 = collection.AddNew();

			AssertContainsExactElementsInAnyOrder(new Order[] { order1, order2 }, collection);

			collection.RemoveFromRelationship(order2);
			AssertContainsExactElementsInAnyOrder(new Order[] { order1 }, collection);

			order2.Delete();
			AssertContainsExactElementsInAnyOrder(new Order[] { order1 }, collection);

			Order order3 = collection.AddNew();
			AssertContainsExactElementsInAnyOrder(new Order[] { order1, order3 }, collection);

			collection.Delete(order1);
			AssertContainsExactElementsInAnyOrder(new Order[] { order3 }, collection);
		}

		public void TestAdding_IAttachParent()
		{
			ShipmentForTesting shipment = Factory.New<ShipmentForTesting>();
			OrderCollection collection = new OrderCollection(Factory, shipment);

			AssertEquals("Precondition", false, shipment.SetDefaultsOnOrderCalled);
			AssertEquals("Precondition", false, shipment.OnOrderAttachedCalled);

			Order order1 = collection.AddNew();
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			AssertEquals("Precondition", false, order1.IsInDatabase);
			AssertEquals(true, shipment.SetDefaultsOnOrderCalled);
			AssertEquals(false, shipment.OnOrderAttachedCalled);

			Order order2 = Factory.NewWithValidTestData<Order>();
			shipment.SetDefaultsOnOrderCalled = false;

			collection.Add(order2);
			AssertEquals("Precondition", false, order2.IsInDatabase);
			AssertEquals(true, shipment.SetDefaultsOnOrderCalled);
			AssertEquals(false, shipment.OnOrderAttachedCalled);

			Order order3 = Factory.NewWithValidTestData<Order>();
			AssertContainsExactElementsInAnyOrder(new Order[] { order1, order2 }, collection);

			Factory.Save();

			shipment.SetDefaultsOnOrderCalled = false;
			shipment.OnOrderAttachedCalled = false;

			collection.Add(order3);
			AssertEquals("Precondition", true, order3.IsInDatabase);
			AssertEquals(false, shipment.SetDefaultsOnOrderCalled);
			AssertEquals(true, shipment.OnOrderAttachedCalled);
			AssertContainsExactElementsInAnyOrder(new Order[] { order1, order2, order3 }, collection);

			shipment.SetDefaultsOnOrderCalled = false;
			shipment.OnOrderAttachedCalled = false;

			Order order4 = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			((ISupportDataImporting)shipment).IsImportingData = true;
			collection.Add(order4);
			AssertEquals("Precondition", true, order4.IsInDatabase);
			AssertEquals(false, shipment.SetDefaultsOnOrderCalled);
			AssertEquals(false, shipment.OnOrderAttachedCalled);
			AssertContainsExactElementsInAnyOrder(new Order[] { order1, order2, order3, order4 }, collection);
		}

		public void TestAdding_PreAdvice()
		{
			PreAdviceForTesting preAdvice = Factory.NewWithValidTestData<PreAdviceForTesting>();
			OrderCollection collection = new OrderCollection(preAdvice, new ZQuery());

			AssertEquals("Precondition", false, preAdvice.AppendContainersFromOrderCoreCalled);

			Order order1 = collection.AddNew();
			order1.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("Precondition", false, order1.IsInDatabase);
			AssertEquals(false, preAdvice.AppendContainersFromOrderCoreCalled);

			Order order2 = Factory.NewWithValidTestData<Order>();
			collection.Add(order2);
			AssertEquals("Precondition", false, order2.IsInDatabase);
			AssertEquals(false, preAdvice.AppendContainersFromOrderCoreCalled);

			Order order3 = Factory.NewWithValidTestData<Order>();
			AssertContainsExactElementsInAnyOrder(new Order[] { order1, order2 }, collection);

			Factory.Save();

			collection.Add(order3);
			AssertEquals("Precondition", true, order3.IsInDatabase);
			AssertEquals(true, preAdvice.AppendContainersFromOrderCoreCalled);
			AssertContainsExactElementsInAnyOrder(new Order[] { order1, order2, order3 }, collection);
		}

		public void TestSetDefaultsForNewElement()
		{
			OrderCollection collection = new OrderCollection(Factory);
			Order order = Factory.NewWithValidTestData<Order>();
			AssertNoExceptionThrown(() => ((IActiveBusinessObjectCollection)collection).SetDefaultsForNewElement(order));

			ShipmentForTesting shipment = Factory.New<ShipmentForTesting>();
			collection = new OrderCollection(Factory, shipment);

			AssertEquals("Precondition", false, shipment.IsInDatabase);
			AssertEquals("Precondition", false, shipment.SetDefaultsOnOrderCalled);
			AssertEquals("Precondition", false, shipment.OnOrderAttachedCalled);

			order = Factory.NewWithValidTestData<Order>();
			((IActiveBusinessObjectCollection)collection).SetDefaultsForNewElement(order);

			AssertEquals("Setting defaults on new order when parent not in database", true, shipment.SetDefaultsOnOrderCalled);
			AssertEquals(false, shipment.OnOrderAttachedCalled);

			shipment = Factory.New<ShipmentForTesting>();
			Factory.Save();

			collection = new OrderCollection(Factory, shipment);

			AssertEquals("Precondition", true, shipment.IsInDatabase);
			AssertEquals("Precondition", false, shipment.SetDefaultsOnOrderCalled);
			AssertEquals("Precondition", false, shipment.OnOrderAttachedCalled);

			order = Factory.NewWithValidTestData<Order>();
			((IActiveBusinessObjectCollection)collection).SetDefaultsForNewElement(order);

			AssertEquals("Not setting defaults on new order when parent is in database", false, shipment.SetDefaultsOnOrderCalled);
			AssertEquals(false, shipment.OnOrderAttachedCalled);
		}

		public void TestHasOrderWithQuantityReceived()
		{
			JobShipmentPreplanning preadvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			AssertEquals("No orders initially", false, preadvice.Orders.HasOrHadOrdersWithQuantityReceived());

			Order order = preadvice.Orders.AddNew();
			AssertEquals("No order lines initially", false, preadvice.Orders.HasOrHadOrdersWithQuantityReceived());

			OrderLine orderLine = order.OrderLines.AddNew();
			AssertEquals("No order lines with quantity received", false, preadvice.Orders.HasOrHadOrdersWithQuantityReceived());

			orderLine.JO_QtyReceived = 2m;
			AssertEquals("Precondition", true, order.OrderLines.HasOrHadOrderLinesWithQuantityReceived());
			AssertEquals("When order line has quantity received", true, preadvice.Orders.HasOrHadOrdersWithQuantityReceived());

			Factory.Save();
			orderLine.JO_QtyReceived = 0m;
			Factory.Save();
			AssertEquals("Precondition", true, order.OrderLines.HasOrHadOrderLinesWithQuantityReceived());
			AssertEquals("When order line had quantity received", true, preadvice.Orders.HasOrHadOrdersWithQuantityReceived());
		}

		#region AddNotificationWhenAdditionalFilterNotMet

		public void TestAddNotificationWhenAdditionalFilterNotMet_WithOrderAttachParent()
		{
			BusinessObjectFactory orderFactory = new BusinessObjectFactory();
			Order order = orderFactory.New<Order>();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			OrderCollection orders = new OrderCollection(Factory, shipment);

			OrgHeader org = GlbCompany.CurrentCompany.OrgProxy;
			shipment.ConsignorPK = org.PK;
			shipment.ConsigneePK = org.PK;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			order.JD_TransportMode = Constants.TransportModes.Air;
			ZString error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order cannot be chosen here as the Transport Mode is different to that of the Shipment.", error);

			order.JD_TransportMode = Constants.TransportModes.Sea;
			order.JD_ContainerMode = Constants.ContainerModes.LCL;
			error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order cannot be chosen here as the Container Mode is different to that of the Shipment.", error);

			order.JD_ContainerMode = Constants.ContainerModes.FCL;
			order.JD_IsCancelled = true;
			error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order cannot be chosen here as it is inactive.", error);

			order.JD_IsCancelled = false;
			order.JD_JS = ZGuid.NewZGuid();
			error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order cannot be chosen here as it is already attached to a Shipment.", error);

			order.JD_JS = ZGuid.Empty;
			order.JD_JE = ZGuid.NewZGuid();
			error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order cannot be chosen here as it is already attached to a Shipment.", error);

			order.JD_JE = ZGuid.Empty;
			error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order  cannot be chosen here. Please choose another Order .", error);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet_WithoutOrderAttachParent()
		{
			OrderCollection orders = new OrderCollection(Factory);
			Order order = Factory.New<Order>();

			order.JD_IsCancelled = true;
			ZString error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order cannot be chosen here as it is inactive.", error);

			order.JD_IsCancelled = false;
			order.JD_JS = ZGuid.NewZGuid();
			error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order cannot be chosen here as it is already attached to a Shipment.", error);

			order.JD_JS = ZGuid.Empty;
			order.JD_JE = ZGuid.NewZGuid();
			error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order cannot be chosen here as it is already attached to a Shipment.", error);

			order.JD_JE = ZGuid.Empty;
			error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertEquals("This Order  cannot be chosen here. Please choose another Order .", error);
		}

		public void TestAttachOrderLineAttachedToSupplierBooking()
		{
			var errorMessage = @"This Order is already linked to an active Supplier Booking. Orders in use in the Supplier Bookings module cannot be linked to Shipments directly.
Please cancel all active Supplier Bookings before attaching this Order directly, or proceed to the Supplier Booking and Container Load List process to create a new Shipment from this Order.";
			var orders = new OrderCollection(Factory);
			var order = Factory.NewWithValidTestData<Order>();
			var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var orderLine = order.OrderLines.AddNew();
			Factory.Save();
			var bookingLine = booking.SupplierBookingLines.AddNew();
			booking.JSB_Status = Constants.SupplierBookingStatus.Incomplete;
			bookingLine.JSL_JO_OrderLine = orderLine.PK;
			booking.JSB_Status = Constants.SupplierBookingStatus.Approved;
			var error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			Assert(order.IsAttachedToSupplierBooking);
			AssertEquals(errorMessage, error);

			booking.JSB_Status = Constants.SupplierBookingStatus.Incomplete;
			Assert(!order.IsAttachedToSupplierBooking);
			error = ((IActiveBusinessObjectCollection)orders).GetAllNotificationsWhenAdditionalFilterNotMet(order);
			AssertNotEquals(errorMessage, error);
		}

		#endregion

		#region Implementation

		class ShipmentForTesting : CommonShipment, IAttachOrders
		{
			public ShipmentForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IAttachOrders Members

			public OrderCollection AttachedOrders
			{
				get => new OrderCollection(Factory, this);
			}

			public OrderCollection PossibleOrdersForAttachment_List
			{
				get => new OrderCollection(Factory, this);
			}

			public JobDocAddress ControllingCustomerDocAddress => throw new System.NotImplementedException();

			public void SetDefaultsOnOrder(Order newOrder)
			{
				SetDefaultsOnOrderCalled = true;
			}
			public bool SetDefaultsOnOrderCalled;

			public void OnOrderAttached(Order attachedOrder)
			{
				OnOrderAttachedCalled = true;
			}
			public bool OnOrderAttachedCalled;

			#endregion
		}

		class PreAdviceForTesting : JobShipmentPreplanning
		{
			public PreAdviceForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void AppendContainersFromOrderCore(Order order)
			{
				AppendContainersFromOrderCoreCalled = true;
			}
			public bool AppendContainersFromOrderCoreCalled;
		}

		#endregion

		#region FindBoxListProvider

		public void TestFindBoxListProvider()
		{
			Order orderAAA = CreateOrder("AAA", 0);
			Order orderABC1 = CreateOrder("ABC", 1);
			Order orderABC2 = CreateOrder("ABC", 2);
			Order orderABC3 = CreateOrder("ABC", 3);
			Order orderBOO1 = CreateOrder("BOO", 1);
			Order orderBOO22 = CreateOrder("BOO", 22);
			Order orderWithDash = CreateOrder("ORD-27", 0);

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUYER";
			var orderWithBuyer = CreateOrder("AAA", 0, buyer);
			var orderWithBuyerAndDash = CreateOrder("AAA-27|OTHER", 0, buyer);

			Factory.Save();

			OrderCollection coll = new OrderCollection(Factory);
			IFindBoxListProvider provider = coll;
			AssertEquals(orderAAA, provider.GetBusinessObjectFromCode("AAA"));
			AssertEquals(orderABC1, provider.GetBusinessObjectFromCode("ABC-1"));
			AssertEquals(orderABC2, provider.GetBusinessObjectFromCode("ABC-2"));
			AssertEquals(orderABC3, provider.GetBusinessObjectFromCode("ABC-3"));
			AssertEquals(orderBOO1, provider.GetBusinessObjectFromCode("BOO-1"));
			AssertEquals(orderBOO22, provider.GetBusinessObjectFromCode("BOO-22"));
			AssertEquals(orderWithDash, provider.GetBusinessObjectFromCode("ORD-27"));
			AssertEquals(orderWithBuyer, provider.GetBusinessObjectFromCode("AAA|BUYER"));
			AssertEquals(orderWithBuyerAndDash, provider.GetBusinessObjectFromCode("AAA-27|OTHER"));
			AssertNull("No order", provider.GetBusinessObjectFromCode("ABB-3"));
			AssertNull("No order", provider.GetBusinessObjectFromCode("AAC"));
			AssertNull("No order", provider.GetBusinessObjectFromCode("ABC-"));
			AssertNull("No order", provider.GetBusinessObjectFromCode("ABC- 4"));
			AssertNull("No order", provider.GetBusinessObjectFromCode("- 4"));

			AssertEquals("ABC-1", provider.NearestMatch("ABC-1", false, -1).Item1);
			AssertEquals("ABC-2", provider.NearestMatch("ABC-2", false, -1).Item1);
			AssertEquals("ABC-3", provider.NearestMatch("ABC-3", false, -1).Item1);
			AssertEquals("ABC-1", provider.NearestMatch("ABC", false, -1).Item1);
			AssertEquals("ABC-1", provider.NearestMatch("ABC-", false, -1).Item1);
			AssertEquals("ZZZ-1", provider.NearestMatch("ZZZ-1", false, -1).Item1);
			AssertEquals("AAA", provider.NearestMatch("A", false, -1).Item1);
			AssertEquals("AAA", provider.NearestMatch("AA", false, -1).Item1);
			AssertEquals("ABC-1", provider.NearestMatch("AB", false, -1).Item1);
			AssertEquals("ABC-3", provider.NearestMatch("ABC-3", false, -1).Item1);
			AssertEquals("BOO-1", provider.NearestMatch("BOO-", false, -1).Item1);
			AssertEquals("BOO-22", provider.NearestMatch("BOO-2", false, -1).Item1);
			AssertEquals("ORD-27", provider.NearestMatch("ORD-2", false, -1).Item1);
		}

		Order CreateOrder(string orderNumber, byte splitNumber, OrgHeader buyer = null)
		{
			var order = Factory.New<Order>();
			order.BuyerPK = (buyer ?? Buyer).PK;
			order.JD_OrderNumber = orderNumber;
			if (splitNumber != 0)
			{
				order.JD_OrderNumberSplit = splitNumber;
			}
			return order;
		}

		OrgHeader Buyer
		{
			get { return fBuyer ?? (fBuyer = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader fBuyer;

		#endregion
	}
}
