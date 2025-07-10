using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(UnAllocatedShipmentView))]
	sealed class UnAllocatedShipmentViewBOTest : BusinessObjectCollectionViewTestCase<UnAllocatedShipmentView>
	{
		protected override UnAllocatedShipmentView GetCollectionToTest()
		{
			var shipments = new ShipmentCollection(Factory);
			var view = new UnAllocatedShipmentView(shipments, null);
			view.ShowOnlyThisSailing = false;
			view.ShowOnlyReceived = false;
			return view;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			shipment.JS_IsCancelled = false;
			return shipment;
		}
	}
}
