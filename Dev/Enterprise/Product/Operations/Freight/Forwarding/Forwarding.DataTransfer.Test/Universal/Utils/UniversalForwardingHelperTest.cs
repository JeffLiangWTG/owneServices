using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class UniversalForwardingHelperTest : TestCaseWithFactory
	{
		public void TestConsolMatching_NormalConsolsAreMatched()
		{
			var notForwardingConsol = Factory.New<CommonConsol>();
			notForwardingConsol.JK_IsForwarding = false;

			var forwardingConsol = Factory.New<CommonConsol>();
			forwardingConsol.JK_IsForwarding = true;

			var query = new ZQuery();
			((IUniversalFreightHelper)new UniversalForwardingHelper()).AddConsolParameters(query);

			var expectedConsols = new[] { forwardingConsol };
			var actualConsols = Factory.Load<CommonConsol>(query);
			AssertContainsExactElementsInAnyOrder(expectedConsols, actualConsols);
		}

		public void TestConsolMatching_CancelledConsolsAreNotMatched()
		{
			var normalConsol = Factory.New<CommonConsol>();
			normalConsol.JK_IsForwarding = true;
			normalConsol.JK_IsCancelled = false;

			var cancelledConsol = Factory.New<CommonConsol>();
			cancelledConsol.JK_IsForwarding = true;
			cancelledConsol.JK_IsCancelled = true;

			var query = new ZQuery();
			((IUniversalFreightHelper)new UniversalForwardingHelper()).AddConsolParameters(query);

			var expectedConsols = new[] { normalConsol };
			var actualConsols = Factory.Load<CommonConsol>(query);
			AssertContainsExactElementsInAnyOrder(expectedConsols, actualConsols);
		}

		public void TestShipmentMatching_NormalShipmentsAreMatched()
		{
			var notForwardingShipment = Factory.New<CommonShipment>();
			notForwardingShipment.JS_IsForwardRegistered = false;

			var forwardingShipment = Factory.New<CommonShipment>();
			forwardingShipment.JS_IsForwardRegistered = true;

			var query = new ZQuery();
			((IUniversalFreightHelper)new UniversalForwardingHelper()).AddShipmentParameters(query);

			var expectedShipments = new[] { forwardingShipment };
			var actualShipments = Factory.Load<CommonShipment>(query);
			AssertContainsExactElementsInAnyOrder(expectedShipments, actualShipments);
		}

		public void TestShipmentMatching_CancelledShipmentsAreNotMatched()
		{
			var normalShipment = Factory.New<CommonShipment>();
			normalShipment.JS_IsForwardRegistered = true;
			normalShipment.JS_IsCancelled = false;

			var cancelledShipment = Factory.New<CommonShipment>();
			cancelledShipment.JS_IsForwardRegistered = true;
			cancelledShipment.JS_IsCancelled = true;

			var query = new ZQuery();
			((IUniversalFreightHelper)new UniversalForwardingHelper()).AddShipmentParameters(query);

			var expectedShipments = new[] { normalShipment };
			var actualShipments = Factory.Load<CommonShipment>(query);
			AssertContainsExactElementsInAnyOrder(expectedShipments, actualShipments);
		}
	}
}
