using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(RelatedOrderLineDeliverContainerCollection))]
	sealed class RelatedOrderLineDeliverContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilter()
		{
			AssertEquals("1 related container as all details specified", 1, ContainerWithAllDetails1.RelatedContainers.Count);
			AssertEquals("1 related container as all details specified", 1, ContainerWithAllDetails2.RelatedContainers.Count);

			AssertEquals("None as vessel not specified", 0, ContainerWithMissingVessel1.RelatedContainers.Count);
			AssertEquals("None as vessel not specified", 0, ContainerWithMissingVessel2.RelatedContainers.Count);

			AssertEquals("None as voyage not specified", 0, ContainerWithMissingVoyage1.RelatedContainers.Count);
			AssertEquals("None as voyage not specified", 0, ContainerWithMissingVoyage2.RelatedContainers.Count);

			AssertEquals("None as Container Number not specified", 0, ContainerWithMissingContainerNumber1.RelatedContainers.Count);
			AssertEquals("None as Container Number not specified", 0, ContainerWithMissingContainerNumber2.RelatedContainers.Count);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrderLineDeliverContainer relatedContainer = Factory.New<OrderLineDeliverContainer>();
			return new RelatedOrderLineDeliverContainerCollection(relatedContainer);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrderLineDeliverContainer>();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Order order1 = Factory.New<Order>();
			order1.JD_OrderNumber = "1111";
			OrderLine orderLine1 = order1.OrderLines.AddNew();
			OrderLineDelivery delivery1 = orderLine1.Deliveries.AddNew();
			ContainerWithAllDetails1 = delivery1.Containers.AddNew();
			ContainerWithAllDetails1.J5_ContainerNum = "12345678";
			ContainerWithAllDetails1.J5_RV_NKArrivalVessel = "HELLO";
			ContainerWithAllDetails1.J5_Voyage = "123";

			Order order2 = Factory.New<Order>();
			order2.JD_OrderNumber = "2222";
			OrderLine orderLine2 = order2.OrderLines.AddNew();
			OrderLineDelivery delivery2 = orderLine2.Deliveries.AddNew();
			ContainerWithAllDetails2 = delivery2.Containers.AddNew();
			ContainerWithAllDetails2.J5_ContainerNum = "12345678";
			ContainerWithAllDetails2.J5_RV_NKArrivalVessel = "HELLO";
			ContainerWithAllDetails2.J5_Voyage = "123";

			Order order3 = Factory.New<Order>();
			order3.JD_OrderNumber = "333";
			OrderLine orderLine3 = order3.OrderLines.AddNew();
			OrderLineDelivery delivery3 = orderLine3.Deliveries.AddNew();
			ContainerWithMissingVessel1 = delivery3.Containers.AddNew();
			ContainerWithMissingVessel1.J5_ContainerNum = "12345678";
			ContainerWithMissingVessel1.J5_RV_NKArrivalVessel = "";
			ContainerWithMissingVessel1.J5_Voyage = "123";

			Order order4 = Factory.New<Order>();
			order4.JD_OrderNumber = "444";
			OrderLine orderLine4 = order4.OrderLines.AddNew();
			OrderLineDelivery delivery4 = orderLine4.Deliveries.AddNew();
			ContainerWithMissingVessel2 = delivery3.Containers.AddNew();
			ContainerWithMissingVessel2.J5_ContainerNum = "12345678";
			ContainerWithMissingVessel2.J5_RV_NKArrivalVessel = "";
			ContainerWithMissingVessel2.J5_Voyage = "123";

			Order order5 = Factory.New<Order>();
			order5.JD_OrderNumber = "555";
			OrderLine orderLine5 = order5.OrderLines.AddNew();
			OrderLineDelivery delivery5 = orderLine5.Deliveries.AddNew();
			ContainerWithMissingVoyage1 = delivery5.Containers.AddNew();
			ContainerWithMissingVoyage1.J5_ContainerNum = "12345678";
			ContainerWithMissingVoyage1.J5_RV_NKArrivalVessel = "HELLO";
			ContainerWithMissingVoyage1.J5_Voyage = "";

			Order order6 = Factory.New<Order>();
			order6.JD_OrderNumber = "666";
			OrderLine orderLine6 = order6.OrderLines.AddNew();
			OrderLineDelivery delivery6 = orderLine6.Deliveries.AddNew();
			ContainerWithMissingVoyage2 = delivery6.Containers.AddNew();
			ContainerWithMissingVoyage2.J5_ContainerNum = "12345678";
			ContainerWithMissingVoyage2.J5_RV_NKArrivalVessel = "HELLO";
			ContainerWithMissingVoyage2.J5_Voyage = "";

			Order order7 = Factory.New<Order>();
			order7.JD_OrderNumber = "777";
			OrderLine orderLine7 = order7.OrderLines.AddNew();
			OrderLineDelivery delivery7 = orderLine7.Deliveries.AddNew();
			ContainerWithMissingContainerNumber1 = delivery7.Containers.AddNew();
			ContainerWithMissingContainerNumber1.J5_ContainerNum = "";
			ContainerWithMissingContainerNumber1.J5_RV_NKArrivalVessel = "HELLO";
			ContainerWithMissingContainerNumber1.J5_Voyage = "123";

			Order order8 = Factory.New<Order>();
			order8.JD_OrderNumber = "888";
			OrderLine orderLine8 = order8.OrderLines.AddNew();
			OrderLineDelivery delivery8 = orderLine8.Deliveries.AddNew();
			ContainerWithMissingContainerNumber2 = delivery8.Containers.AddNew();
			ContainerWithMissingContainerNumber2.J5_ContainerNum = "";
			ContainerWithMissingContainerNumber2.J5_RV_NKArrivalVessel = "HELLO";
			ContainerWithMissingContainerNumber2.J5_Voyage = "123";
		}

		OrderLineDeliverContainer ContainerWithAllDetails1;
		OrderLineDeliverContainer ContainerWithAllDetails2;
		OrderLineDeliverContainer ContainerWithMissingVessel1;
		OrderLineDeliverContainer ContainerWithMissingVessel2;
		OrderLineDeliverContainer ContainerWithMissingVoyage1;
		OrderLineDeliverContainer ContainerWithMissingVoyage2;
		OrderLineDeliverContainer ContainerWithMissingContainerNumber1;
		OrderLineDeliverContainer ContainerWithMissingContainerNumber2;

		#endregion
	}
}
