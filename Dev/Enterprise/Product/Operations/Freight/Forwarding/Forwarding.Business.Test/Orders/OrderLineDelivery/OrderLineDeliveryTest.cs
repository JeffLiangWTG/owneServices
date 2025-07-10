using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	[TestedType(typeof(OrderLineDelivery))]
	sealed class OrderLineDeliveryTest : BusinessObjectWithCustomLabelsTestCase
	{
		public void TestRelationships()
		{
			AssertEquals("Containers", 0, BO.Containers.Count);
			AssertEquals("Containers", typeof(OrderLineDeliverContainer), BO.Containers.AddNew().GetType());
		}

		const string TestPortCode = "USMSY";

		public void TestPopulateDeliveryPointFromDestinationPort()
		{
			OrgHeader buyer = BO.OrderLine.Order.Buyer;

			OrgAddress deliverPointAddress = buyer.Addresses.AddNew();
			deliverPointAddress.OA_RL_NKRelatedPortCode = TestPortCode;
			deliverPointAddress.OA_Address1 = "A";
			deliverPointAddress.OA_Address2 = "B";
			deliverPointAddress.OA_Code = "deliver_point";
			Factory.Save();

			BO.J4_RL_NKDestinationPort = "invld";
			AssertEquals("Invalid port should not populate delivery point address", ZString.Empty, BO.J4_OA_NKDeliveryPoint);
			BO.J4_RL_NKDestinationPort = ZString.Empty;

			BO.J4_RL_NKDestinationPort = TestPortCode;
			AssertEquals("Valid port should populate delivery point address", deliverPointAddress.OA_Code, BO.J4_OA_NKDeliveryPoint);
			BO.J4_RL_NKDestinationPort = "";
			AssertEquals("Emptying destination port should not empty delivery point", deliverPointAddress.OA_Code, BO.J4_OA_NKDeliveryPoint);
		}

		public void TestPopulateDeliveryPointFromDestinationPort_DontPopulateWhen2AddressesCanMatch()
		{
			OrgHeader buyer = BO.OrderLine.Order.Buyer;

			OrgAddress deliverPointAddress = buyer.Addresses.AddNew();
			deliverPointAddress.OA_RL_NKRelatedPortCode = TestPortCode;
			deliverPointAddress.OA_Address1 = "A";
			deliverPointAddress.OA_Address2 = "B";
			deliverPointAddress.OA_Code = "deliver_point";

			OrgAddress secondDeliverPointAddress = buyer.Addresses.AddNew();
			secondDeliverPointAddress.OA_RL_NKRelatedPortCode = TestPortCode;
			secondDeliverPointAddress.OA_Address1 = "X";
			secondDeliverPointAddress.OA_Address2 = "Y";
			secondDeliverPointAddress.OA_Code = "deliver_point2";
			Factory.Save();

			BO.J4_OA_NKDeliveryPoint = ZString.Empty;
			BO.J4_RL_NKDestinationPort = TestPortCode;
			AssertEquals("Delivery point should NOT populate when 2 possible addresses match", ZString.Empty, BO.J4_OA_NKDeliveryPoint);
		}

		public void TestPopulateDestinationPortFromDeliveryPoint()
		{
			OrgHeader buyer = BO.OrderLine.Order.Buyer;
			OrgAddress deliverPointAddress = buyer.Addresses.AddNew();

			deliverPointAddress.OA_RL_NKRelatedPortCode = TestPortCode;
			deliverPointAddress.OA_Address1 = "A";
			deliverPointAddress.OA_Address2 = "B";
			deliverPointAddress.OA_Code = "delivery_point";
			Factory.Save();

			BO.J4_OA_NKDeliveryPoint = deliverPointAddress.OA_Code;
			AssertEquals("Should populate destination port from delivery point address", BO.J4_RL_NKDestinationPort, TestPortCode);
			BO.J4_OA_NKDeliveryPoint = "1234";
			AssertEquals("Should not modify destination port after overriding delivery point address", BO.J4_RL_NKDestinationPort, TestPortCode);
			BO.J4_OA_NKDeliveryPoint = ZString.Empty;
			AssertEquals("Should not modify destination port after delivery point address emptied", BO.J4_RL_NKDestinationPort, TestPortCode);
		}

		public void TestJ4_DeliverPoint_List()
		{
			OrgHeader buyer = BO.OrderLine.Order.Buyer;
			buyer.OH_RL_NKClosestPort = "USMSY";
			RefUNLOCO buyerUnloco = buyer.UNLOCO;
			var otherUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_PortName, SQLComparisonOperator.Equal, "Clinton"));
			buyer.Addresses.RemoveAndDeleteAll();
			// NB: There should always be an Address in the addresses collection of an OrgHeader
			AddNewPostalAddress(buyer);

			OrgAddress newAddress1 = buyer.Addresses.AddNew();
			newAddress1.OA_RL_NKRelatedPortCode = buyerUnloco.RL_Code;
			newAddress1.OA_Address1 = "A";
			newAddress1.OA_Address2 = "B";
			newAddress1.OA_Code = "usage1";
			OrgAddress newAddress2 = buyer.Addresses.AddNew();
			newAddress2.OA_RL_NKRelatedPortCode = otherUnloco.RL_Code;
			newAddress2.OA_Address1 = "X";
			newAddress2.OA_Address2 = "Y";
			newAddress2.OA_Code = "usage2";
			Factory.Save();

			BO.J4_RL_NKDestinationPort = ZString.Empty;
			AssertEquals("Show all deliver points on buyer when destination port not selected", 3, BO.J4_DeliverPoint_List.Count);
			BO.J4_RL_NKDestinationPort = otherUnloco.RL_Code;
			BO.J4_OA_NKDeliveryPoint = ZString.Empty;
			AssertEquals("Show filtered deliver points on buyer and destination port when port selected", 1, BO.J4_DeliverPoint_List.Count);
		}

		void AddNewPostalAddress(OrgHeader org)
		{
			if (org.MainAddress == null)
			{
				OrgAddress address = org.Addresses.AddNew();
				address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Postal);
				address.OA_Address1 = "Postal Address";
			}
		}

		public void TestDeliveryPoint()
		{
			OrgAddress address1 = BO.OrderLine.Order.Buyer.Addresses.AddNew();
			address1.OA_Code = "1234";
			OrgAddress address2 = BO.OrderLine.Order.Buyer.Addresses.AddNew();
			address2.OA_Code = "5678";
			BO.J4_OA_NKDeliveryPoint = "5678";

			AssertEquals("Found correct DeliveryPoint", address2.PK, BO.DeliveryPoint.PK);
		}

		#region Implementation

		OrderLineDelivery BO;

		protected override void SetUp()
		{
			base.SetUp();
			BO = (OrderLineDelivery)GetNewBusinessObject("order");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject("x");
		}

		BusinessObject GetNewBusinessObject(string orderNumber)
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Order order = Factory.New<Order>();

			order.JD_OrderNumber = orderNumber;
			order.BuyerPK = org.PK;
			order.SupplierPK = org.PK;
			Factory.Save();
			OrderLine line = Factory.New<OrderLine>();
			line.JO_JD = order.PK;
			line.JO_LineNo = 1000;

			return line.Deliveries.AddNew();
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bO)
		{
			OrderLineDelivery delivery = (OrderLineDelivery)bO;
			return OrderLineDelivery.NewCustomLabelsProvider(delivery.OrderLine.Order);
		}

		#endregion
	}
}
