using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	class OrderEventDataModelTest : TestCaseWithFactory
	{
		#region Origin

		public void TestOriginGetter()
		{
			var order = GetOrderInstance();
			order.JD_RL_NKPortOfLoading = "UAIEV";

			var model = new OrderEventDataModel(order);

			AssertEquals("Property value from Order", order.JD_RL_NKPortOfLoading, model.Origin);
		}

		#endregion

		#region Destination

		public void TestDestinationGetter()
		{
			var order = GetOrderInstance();
			order.JD_RL_NKPortOfDischarge = "UAIEV";

			var model = new OrderEventDataModel(order);

			AssertEquals("Property value from Order", order.JD_RL_NKPortOfDischarge, model.Destination);
		}

		#endregion

		#region FirstLeg

		public void TestFirstLegGetter()
		{
			var order = GetOrderInstance();
			order.JD_RL_NKPortOfLoading = "SGSIN";

			var model = new OrderEventDataModel(order);

			AssertEquals("Property value from Order", order.JD_RL_NKPortOfLoading, model.FirstLeg.Origin);

			order = GetOrderInstanceWithShipment();
			order.JD_RL_NKPortOfLoading = "SGSIN";
			order.Shipment.JS_RL_NKOrigin = "USCHI";

			model = new OrderEventDataModel(order);

			AssertNull("No Transport", model.FirstLeg);

			var transport1 = order.Shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = order.Shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = order.Shipment.Transports.New(from: "USNYC", to: "UAIEV");

			model = new OrderEventDataModel(order);

			AssertEquals("Property value from Transport", transport1.JW_RL_NKLoadPort, model.FirstLeg.Origin);
		}

		#endregion

		#region SecondLeg

		public void TestSecondLegGetter()
		{
			var order = GetOrderInstance();
			order.JD_RL_NKPortOfDischarge = "SGSIN";

			var model = new OrderEventDataModel(order);

			AssertNull("No Transport", model.SecondLeg);

			order = GetOrderInstanceWithShipment();
			order.JD_RL_NKPortOfDischarge = "SGSIN";
			order.Shipment.JS_RL_NKDestination = "USCHI";

			model = new OrderEventDataModel(order);

			AssertNull("No Transport", model.SecondLeg);

			var transport1 = order.Shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = order.Shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = order.Shipment.Transports.New(from: "USNYC", to: "UAIEV");

			model = new OrderEventDataModel(order);

			AssertEquals("Property value from Transport", transport2.JW_RL_NKDiscPort, model.SecondLeg.Destination);
		}

		#endregion

		#region ThirdLeg

		public void TestThirdLegGetter()
		{
			var order = GetOrderInstance();
			order.JD_RL_NKPortOfDischarge = "SGSIN";

			var model = new OrderEventDataModel(order);

			AssertNull("No Transport", model.ThirdLeg);

			order = GetOrderInstanceWithShipment();
			order.JD_RL_NKPortOfDischarge = "SGSIN";
			order.Shipment.JS_RL_NKDestination = "USCHI";

			model = new OrderEventDataModel(order);

			AssertNull("No Transport", model.ThirdLeg);

			var transport1 = order.Shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = order.Shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = order.Shipment.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = order.Shipment.Transports.New(from: "UAIEV", to: "AUMEL");

			model = new OrderEventDataModel(order);

			AssertEquals("Property value from Transport", transport3.JW_RL_NKDiscPort, model.ThirdLeg.Destination);
		}

		#endregion

		#region FourthLeg

		public void TestFourthLegGetter()
		{
			var order = GetOrderInstance();
			order.JD_RL_NKPortOfDischarge = "SGSIN";

			var model = new OrderEventDataModel(order);

			AssertNull("No Transport", model.FourthLeg);

			order = GetOrderInstanceWithShipment();
			order.JD_RL_NKPortOfDischarge = "SGSIN";
			order.Shipment.JS_RL_NKDestination = "USCHI";

			model = new OrderEventDataModel(order);

			AssertNull("No Transport", model.FourthLeg);

			var transport1 = order.Shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = order.Shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = order.Shipment.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = order.Shipment.Transports.New(from: "UAIEV", to: "AUMEL");
			var transport5 = order.Shipment.Transports.New(from: "AUMEL", to: "NZAKL");

			model = new OrderEventDataModel(order);

			AssertEquals("Property value from Transport", transport4.JW_RL_NKDiscPort, model.FourthLeg.Destination);
		}

		#endregion

		#region LastLeg

		public void TestLastLegGetter()
		{
			var order = GetOrderInstance();
			order.JD_RL_NKPortOfDischarge = "SGSIN";

			var model = new OrderEventDataModel(order);

			AssertEquals("Property value from Order", order.JD_RL_NKPortOfDischarge, model.LastLeg.Destination);

			order = GetOrderInstanceWithShipment();
			order.JD_RL_NKPortOfDischarge = "SGSIN";
			order.Shipment.JS_RL_NKDestination = "USCHI";

			model = new OrderEventDataModel(order);

			AssertNull("No Transport", model.LastLeg);

			var transport1 = order.Shipment.Transports.New(from: "AUSYD", to: "USLAX");
			var transport2 = order.Shipment.Transports.New(from: "USLAX", to: "USNYC");
			var transport3 = order.Shipment.Transports.New(from: "USNYC", to: "UAIEV");
			var transport4 = order.Shipment.Transports.New(from: "UAIEV", to: "AUMEL");
			var transport5 = order.Shipment.Transports.New(from: "AUMEL", to: "NZAKL");

			model = new OrderEventDataModel(order);

			AssertEquals("Property value from Transport", transport5.JW_RL_NKDiscPort, model.LastLeg.Destination);
		}

		#endregion

		protected virtual Order GetOrderInstance()
		{
			return Factory.New<Order>();
		}
		protected virtual Order GetOrderInstanceWithShipment()
		{
			var shipment = Factory.New<CommonShipment>();
			var order = Factory.New<Order>();
			order.JD_JS = shipment.PK;

			return order;
		}
	}
}
