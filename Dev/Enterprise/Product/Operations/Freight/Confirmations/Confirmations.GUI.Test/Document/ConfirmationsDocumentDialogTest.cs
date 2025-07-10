using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	sealed class ConfirmationsDocumentDialogTest : TestCaseWithFactory
	{
		public void TestShowDialogDisposeAndContinue()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			CommonPickupDeliveryConfirmCollection confirms = new CommonPickupDeliveryConfirmCollection(Factory, shipment, Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
			CommonPickupDeliveryConfirm confirmWithNoTransport = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm confirmWithSameTransport = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm confirmWithDiffTransport = shipment.PickupConfirms.AddNew();

			DocumentPickupDeliveryConfirmCollection pickupDeliveryConfirms = new DocumentPickupDeliveryConfirmCollection(shipment.PickupConfirms);

			ConfirmationsDocumentDialog dialog = new ConfirmationsDocumentDialog();
			dialog.ShowDialogDisposeAndContinue(pickupDeliveryConfirms, shipment);
			Assert(ZFormModaliser.LastFormShownDialogForTest is DocumentPickupDeliveryConfirmForm);
		}
	}
}
