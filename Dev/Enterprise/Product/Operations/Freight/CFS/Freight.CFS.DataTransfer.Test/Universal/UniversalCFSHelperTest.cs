using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;

namespace Enterprise.Freight.CFS.DataTransfer.Universal.Testing
{
	class UniversalCFSHelperTest : TestCaseWithFactory
	{
		public void TestConsolMatching_NormalConsolsAreMatched()
		{
			var forwardingConsol = CreateConsol(true, false);
			var cfsConsol = CreateConsol(false, true);
			var forwardingCfsConsol = CreateConsol(true, true);

			var query = new ZQuery();
			((IUniversalFreightHelper)new UniversalCFSHelper()).AddConsolParameters(query);

			var expectedConsols = new[] { cfsConsol };
			var actualConsols = Factory.Load<CommonConsol>(query);
			AssertContainsExactElementsInAnyOrder(expectedConsols, actualConsols);
		}

		public void TestConsolMatching_CancelledConsolsAreNotMatched()
		{
			var normalConsol = CreateConsol(false, true);
			normalConsol.JK_IsCancelled = false;

			var cancelledConsol = CreateConsol(false, true);
			cancelledConsol.JK_IsCancelled = true;

			var query = new ZQuery();
			((IUniversalFreightHelper)new UniversalCFSHelper()).AddConsolParameters(query);

			var expectedConsols = new[] { normalConsol };
			var actualConsols = Factory.Load<CommonConsol>(query);
			AssertContainsExactElementsInAnyOrder(expectedConsols, actualConsols);
		}

		CommonConsol CreateConsol(bool isForwarding, bool isCFS)
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_IsForwarding = isForwarding;
			consol.JK_IsCFS = isCFS;

			return consol;
		}

		public void TestShipmentMatching_NormalShipmentsAreMatched()
		{
			var forwardingShipment = CreateShipment(true, false);
			var cfsShipment = CreateShipment(false, true);
			var forwardingCfsShipment = CreateShipment(true, true);

			var query = new ZQuery();
			((IUniversalFreightHelper)new UniversalCFSHelper()).AddShipmentParameters(query);

			var expectedShipments = new[] { cfsShipment };
			var actualShipments = Factory.Load<CommonShipment>(query);
			AssertContainsExactElementsInAnyOrder(expectedShipments, actualShipments);
		}

		public void TestShipmentMatching_CancelledShipmentsAreNotMatched()
		{
			var normalShipment = CreateShipment(false, true);
			normalShipment.JS_IsCancelled = false;

			var cancelledShipment = CreateShipment(false, true);
			cancelledShipment.JS_IsCancelled = true;

			var query = new ZQuery();
			((IUniversalFreightHelper)new UniversalCFSHelper()).AddShipmentParameters(query);

			var expectedShipments = new[] { normalShipment };
			var actualShipments = Factory.Load<CommonShipment>(query);
			AssertContainsExactElementsInAnyOrder(expectedShipments, actualShipments);
		}

		CommonShipment CreateShipment(bool isForwarding, bool isCFS)
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_IsForwardRegistered = isForwarding;
			shipment.JS_IsCFSRegistered = isCFS;

			return shipment;
		}
	}
}
