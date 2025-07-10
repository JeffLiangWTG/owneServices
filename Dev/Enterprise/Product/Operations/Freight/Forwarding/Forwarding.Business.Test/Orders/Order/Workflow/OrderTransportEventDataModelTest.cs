using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	class OrderTransportEventDataModelTest : TestCaseWithFactory
	{
		#region Origin

		public void TestOriginGetter()
		{
			var order = GetOrderInstance();
			order.JD_RL_NKPortOfLoading = "UAIEV";

			var model = new OrderTransportEventDataModel(order);

			AssertEquals("Property value from Order", order.JD_RL_NKPortOfLoading, model.Origin);

			var transport = GetTransportInstance();
			transport.JW_RL_NKLoadPort = "AUSYD";

			model = new OrderTransportEventDataModel(transport);

			AssertEquals("Property value from Transport", transport.JW_RL_NKLoadPort, model.Origin);
		}

		#endregion

		#region Destination

		public void TestDestinationGetter()
		{
			var order = GetOrderInstance();
			order.JD_RL_NKPortOfDischarge = "SGSIN";

			var model = new OrderTransportEventDataModel(order);

			AssertEquals("Property value from Order", order.JD_RL_NKPortOfDischarge, model.Destination);

			var transport = GetTransportInstance();
			transport.JW_RL_NKDiscPort = "USLAX";

			model = new OrderTransportEventDataModel(transport);

			AssertEquals("Property value from Transport", transport.JW_RL_NKDiscPort, model.Destination);
		}

		#endregion

		protected virtual Order GetOrderInstance()
		{
			return Factory.New<Order>();
		}
		protected virtual Transport GetTransportInstance()
		{
			var shipment = Factory.New<CommonShipment>();
			return shipment.Transports.AddNew();
		}
	}
}
