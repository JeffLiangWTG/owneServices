using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderLineDeliverContainer))]
	sealed class OrderLineDeliverContainerTest : BusinessObjectWithCustomLabelsTestCase
	{
		#region Clone

		public void TestClone()
		{
			var container = Factory.NewWithValidTestData<OrderLineDeliverContainer>();
			var clonedContainer = (OrderLineDeliverContainer)container.Clone();
			AssertEquals("Should be linked to the same delivery after clone", container.J5_J4, clonedContainer.J5_J4);
		}

		#endregion

		#region Properties

		public void TestJ5_JD_OrderNumber()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");

			Factory.Save();

			var retrievingFactory = new BusinessObjectFactory();
			var retrievedContainer = retrievingFactory.Load<OrderLineDeliverContainer>(deliverContainer.PK);
			AssertEquals("J5_JD_OrderNumber", "ordernum", retrievedContainer.J5_JD_OrderNumber);
		}

		public void TestJ5_JO_Partno()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");

			Factory.Save();

			var retrievingFactory = new BusinessObjectFactory();
			var retrievedContainer = retrievingFactory.Load<OrderLineDeliverContainer>(deliverContainer.PK);
			AssertEquals("J5_JO_Partno", "1234", retrievedContainer.J5_JO_Partno);
		}

		#endregion

		#region Related Business Objects

		public void TestOrderLineDeliveryProperty()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");

			Factory.Save();

			var retrievingFactory = new BusinessObjectFactory();
			var retrievedContainer = retrievingFactory.Load<OrderLineDeliverContainer>(deliverContainer.PK);
			AssertNotNull("OrderLineDelivery not null", retrievedContainer.OrderLineDelivery);
		}

		public void TestRelatedContainersCollectionEditable()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");
			var delivery = deliverContainer.OrderLineDelivery;

			var relatedDeliverContainer1 = delivery.Containers.AddNew();
			var relatedDeliverContainer2 = delivery.Containers.AddNew();
			relatedDeliverContainer1.J5_ContainerNum = "1111";
			relatedDeliverContainer1.J5_RV_NKArrivalVessel = "vessel1";
			relatedDeliverContainer1.J5_Voyage = "voyage1";
			relatedDeliverContainer2.J5_ContainerNum = "2222";
			relatedDeliverContainer2.J5_RV_NKArrivalVessel = "vessel2";
			relatedDeliverContainer2.J5_Voyage = "voyage2";

			var relatedContainers = deliverContainer.RelatedContainers;
			deliverContainer.J5_ContainerNum = "1111";
			deliverContainer.J5_RV_NKArrivalVessel = "vessel1";
			deliverContainer.J5_Voyage = "voyage1";

			Factory.Save();

			AssertEquals("Parent of a related container shouldn't have HasChanges before the test", false, delivery.HasChanges);
			deliverContainer.RelatedContainers[0].J5_CustomAttribute1 = "modified";
			AssertEquals("Parent of a related container should now HasChanges due to edit", true, delivery.HasChanges);
		}

		public void TestUpdateContainerDetailsFromRelatedContainers()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");

			const string AssertMessage = "When changing the unique container/vessel/voyage, and the container/vessel/voyage we change to exists, the details should be updated";

			var relatedDeliverContainer1 = deliverContainer.OrderLineDelivery.Containers.AddNew();
			relatedDeliverContainer1.J5_ContainerNum = "1111";
			relatedDeliverContainer1.J5_RV_NKArrivalVessel = "vessel1";
			relatedDeliverContainer1.J5_Voyage = "voyage1";
			relatedDeliverContainer1.J5_ContainerSeal = "seal1";

			var relatedDeliverContainer2 = deliverContainer.OrderLineDelivery.Containers.AddNew();
			relatedDeliverContainer2.J5_ContainerNum = "2222";
			relatedDeliverContainer2.J5_RV_NKArrivalVessel = "vessel2";
			relatedDeliverContainer2.J5_Voyage = "voyage2";
			relatedDeliverContainer2.J5_ContainerSeal = "seal2";

			deliverContainer.J5_PackCount = 1;
			relatedDeliverContainer1.J5_PackCount = 2;
			relatedDeliverContainer2.J5_PackCount = 3;

			Factory.Save();

			deliverContainer.J5_PackCount = 4;
			relatedDeliverContainer1.J5_PackCount = 5;
			relatedDeliverContainer2.J5_PackCount = 6;

			deliverContainer.J5_ContainerNum = "1111";
			deliverContainer.J5_RV_NKArrivalVessel = "vessel1";
			deliverContainer.J5_Voyage = "voyage1";
			AssertEquals(AssertMessage, "seal1", deliverContainer.J5_ContainerSeal);
			AssertEquals(AssertMessage, "seal1", deliverContainer.RelatedContainers[0].J5_ContainerSeal);

			deliverContainer.J5_ContainerNum = "2222";
			deliverContainer.J5_Voyage = "voyage2";
			deliverContainer.J5_RV_NKArrivalVessel = "vessel2";
			AssertEquals(AssertMessage, "seal2", deliverContainer.J5_ContainerSeal);
			AssertEquals(AssertMessage, "seal2", deliverContainer.RelatedContainers[0].J5_ContainerSeal);

			deliverContainer.J5_RV_NKArrivalVessel = "vessel1";
			deliverContainer.J5_Voyage = "voyage1";
			deliverContainer.J5_ContainerNum = "1111";
			AssertEquals(AssertMessage, "seal1", deliverContainer.J5_ContainerSeal);
			AssertEquals(AssertMessage, "seal1", deliverContainer.RelatedContainers[0].J5_ContainerSeal);

			AssertEquals("Other container properties should remain unchanged", (ZShort)4, deliverContainer.J5_PackCount);
			AssertEquals("Other container properties should remain unchanged", (ZShort)5, relatedDeliverContainer1.J5_PackCount);
			AssertEquals("Other container properties should remain unchanged", (ZShort)6, relatedDeliverContainer2.J5_PackCount);
		}

		public void TestContainerTypeComesFromPlannedContainers()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");

			var container1 = deliverContainer.OrderLineDelivery.OrderLine.Order.PlannedContainers.AddNew();
			container1.J1_ContainerNumber = "CNT123";
			container1.J1_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var container2 = deliverContainer.OrderLineDelivery.OrderLine.Order.PlannedContainers.AddNew();
			container2.J1_ContainerNumber = "CNT40G";
			container2.J1_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40GP")).PK;

			var relatedDeliverContainer = deliverContainer.OrderLineDelivery.Containers.AddNew();

			relatedDeliverContainer.J5_ContainerNum = "2222";
			AssertEquals("", relatedDeliverContainer.J5_RC_NKContainerType);

			relatedDeliverContainer.J5_ContainerNum = "CNT123";
			AssertEquals("20GP", relatedDeliverContainer.J5_RC_NKContainerType);

			relatedDeliverContainer.J5_ContainerNum = "CNT40G";
			AssertEquals("40GP", relatedDeliverContainer.J5_RC_NKContainerType);
		}

		public void TestPropertiesAlsoSetRelatedContainerValues()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");
			var delivery = deliverContainer.OrderLineDelivery;

			var container1 = delivery.Containers.AddNew();
			var container2 = delivery.Containers.AddNew();

			container1.J5_ContainerNum = "c1234";
			container1.J5_Voyage = "voyage";
			container1.J5_RV_NKArrivalVessel = "vessel";
			container2.J5_ContainerNum = "c1234";
			container2.J5_Voyage = "voyage";
			container2.J5_RV_NKArrivalVessel = "vessel";

			container1.J5_SightedDate = ZDateTime.Now;
			AssertEquals("J5_SightedDate", container1.J5_SightedDate, container2.J5_SightedDate);

			container1.J5_ContainerSeal = "testseal";
			AssertEquals("J5_ContainerSeal", container1.J5_ContainerSeal, container2.J5_ContainerSeal);

			container1.J5_RC_NKContainerType = "type";
			AssertEquals("J5_RC_NKContainerType", container1.J5_RC_NKContainerType, container2.J5_RC_NKContainerType);

			container1.J5_ETA = ZDateTime.Now;
			AssertEquals("J5_ETA", container1.J5_ETA, container2.J5_ETA);

			container1.J5_ETD = ZDateTime.Now;
			AssertEquals("J5_ETD", container1.J5_ETD, container2.J5_ETD);

			container1.J5_MasterBill = "master";
			AssertEquals("J5_MasterBill", container1.J5_MasterBill, container2.J5_MasterBill);
		}

		#endregion

		#region Attach to Delivery

		public void TestAttachToDelivery()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");
			var deliveryToAttach = SetupDataForAttachDelivery("TestOrder", "PartNo", "AUADL");

			deliverContainer.Order.BuyerPK = deliveryToAttach.Order.BuyerPK;

			Factory.Save();

			AssertEquals("Precondition: no containers attached", deliveryToAttach.Containers.Count, 0);

			var succeeded = deliverContainer.TryAttachToDelivery(out var errorMessage);

			AssertEquals("Attaching should have succeeded", succeeded, true);

			var attachedContainer = deliveryToAttach.Containers.FirstOrDefault();
			AssertEquals("Attached to order", "TestOrder", attachedContainer.J5_JD_OrderNumber);
			AssertEquals("Attached to order line", "PartNo", attachedContainer.J5_JO_Partno);
			AssertEquals("Attached to delivery", "AUADL", attachedContainer.J5_J4_RL_NKDestinationPort);
		}

		public void TestAttachToDelivery_DuplicateOrderLineProducts()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");
			var firstDelivery = SetupDataForAttachDelivery("TestOrder", "PartNo", "AUSYD");
			deliverContainer.Order.BuyerPK = firstDelivery.Order.BuyerPK;

			var secondOrderLine = firstDelivery.OrderLine.Order.OrderLines.AddNew();
			secondOrderLine.JO_LineNo = 3;
			secondOrderLine.JO_Partno = "PartNo";

			var secondDelivery = secondOrderLine.Deliveries.AddNew();
			secondDelivery.J4_RL_NKDestinationPort = "AUADL";

			var succeeded = deliverContainer.TryAttachToDelivery(out var message);

			AssertEquals("Attaching should have succeeded", succeeded, true);

			var attachedContainer = secondDelivery.Containers.FirstOrDefault();
			AssertEquals("Attached to order", "TestOrder", attachedContainer.J5_JD_OrderNumber);
			AssertEquals("Attached to order line", "PartNo", attachedContainer.J5_JO_Partno);
			AssertEquals("Attached to delivery", secondDelivery.PK, attachedContainer.J5_J4);
		}

		public void TestAttachToDelivery_NoMatchToOrderNumber()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "TestOrder", "PartNo", "AUADL");
			var deliveryToAttach = SetupDataForAttachDelivery("testord", "partno", "AUADL");
			deliverContainer.Order.BuyerPK = deliveryToAttach.Order.BuyerPK;

			var succeeded = deliverContainer.TryAttachToDelivery(out var errorMessage);

			AssertEquals("Matching should have failed", false, succeeded);
			AssertEquals(errorMessage, "No order found matching order number: TestOrder");
		}

		public void TestAttachToDelivery_NoMatchToPartno()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "testord", "xxx", "AUADL");
			var deliveryToAttach = SetupDataForAttachDelivery("testord", "partno", "AUADL");
			deliverContainer.Order.BuyerPK = deliveryToAttach.Order.BuyerPK;

			var succeeded = deliverContainer.TryAttachToDelivery(out var errorMessage);

			AssertEquals("Matching should have failed", false, succeeded);
			AssertEquals(errorMessage, "Could not find order line with product #: xxx");
		}

		public void TestAttachToDelivery_NoMatchToDestinationPort()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "testord", "partno", "xxx");
			var deliveryToAttach = SetupDataForAttachDelivery("testord", "partno", "AUADL");
			deliverContainer.Order.BuyerPK = deliveryToAttach.Order.BuyerPK;

			var succeeded = deliverContainer.TryAttachToDelivery(out var errorMessage);
			AssertEquals("Matching should have failed", false, succeeded);
			AssertEquals(errorMessage, "Could not find delivery with destination port: xxx");
		}

		public void TestAttachToDelivery_DuplicateContainerNumber()
		{
			var deliverContainer = SetupAndGetDeliverContainer("C1", "testord ", "parTNo", "AUadL");
			var deliveryToAttach = SetupDataForAttachDelivery("testord", "partno", "AUADL");
			deliverContainer.Order.BuyerPK = deliveryToAttach.Order.BuyerPK;

			var succeeded = deliverContainer.TryAttachToDelivery(out var errorMessage);
			succeeded = deliverContainer.TryAttachToDelivery(out var secondMessage);
			AssertEquals("Matching should have failed", false, succeeded);
			AssertEquals(secondMessage, "Container with number 'C1' is already attached");
		}

		OrderLineDelivery SetupDataForAttachDelivery(ZString orderNumber, ZString partno, ZString destinationPort)
		{
			var orderToAttachTo = Factory.New<Order>();
			orderToAttachTo.JD_OrderNumber = orderNumber;
			orderToAttachTo.BuyerPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var orderLineToAttachTo = orderToAttachTo.OrderLines.AddNew();
			orderLineToAttachTo.JO_Partno = partno;

			var deliveryToAttachTo = orderLineToAttachTo.Deliveries.AddNew();
			deliveryToAttachTo.J4_RL_NKDestinationPort = destinationPort;

			return deliveryToAttachTo;
		}

		OrderLineDeliverContainer SetupAndGetDeliverContainer(ZString containerNumber, ZString orderNumber, ZString partNo, ZString destinationPort)
		{
			var order = Factory.New<Order>();
			order.JD_OrderNumber = "ordernum";

			order.SupplierPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			order.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 5;
			orderLine.JO_Partno = "1234";

			var delivery = orderLine.Deliveries.AddNew();

			var deliverContainer = delivery.Containers.AddNew();
			deliverContainer.J5_OrderNumberForAttach = orderNumber;
			deliverContainer.J5_PartnoForAttach = partNo;
			deliverContainer.J5_RL_DestinationPortForAttach = destinationPort;
			deliverContainer.J5_ContainerNum = containerNumber;

			return deliverContainer;
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var order = Factory.New<Order>();
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;

			var orderLine = order.OrderLines.AddNew();
			var delivery = orderLine.Deliveries.AddNew();

			return delivery.Containers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var buyer = factory.NewWithValidTestData<OrgHeader>();

			var order = factory.New<Order>();
			order.BuyerPK = buyer.PK;

			var orderLine = order.OrderLines.AddNew();
			var delivery = orderLine.Deliveries.AddNew();

			return delivery.Containers.AddNew();
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			OrderLineDeliverContainer deliverContainer = (OrderLineDeliverContainer)bO;
			return new OrderLineDeliverContainer.CustomLabelsProvider(deliverContainer.OrderLineDelivery.OrderLine.Order);
		}

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new OrderLineDeliverContainerLightValidationTester(bizObjToTest);
		}

		class OrderLineDeliverContainerLightValidationTester : LightValidationTester
		{
			public OrderLineDeliverContainerLightValidationTester(BusinessObject bo)
				: base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return propertyName != "E2_AddressType" && propertyName != "E2_AddressOverride" && propertyName != "E2_AddressSequence" && propertyName != "E2_OA_Address";
			}
		}

		#endregion
	}
}
