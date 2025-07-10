using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	public class ShipmentWrapperTest : TestCaseWithFactory
	{
		public void TestShipmentAmendmentReasonCode()
		{
			var trip = Factory.New<Trip>();
			var shipment1 = trip.Shipments.AddNew();
			shipment1.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Cancelled;

			var shipmentAction = trip.ShipmentsActions.FirstOrDefault() as ShipmentAction;
			shipmentAction.B0_AmendmentReason = ZString.Empty;
			shipmentAction.B0_ActionCode = MessageActionCodes.Codes.Change;
			var shipmentWrapper = new ShipmentWrapper(shipmentAction);
			AssertEquals("Default to 03 for Change Action", ShipmentAmendmentCodes.Codes.C03, shipmentWrapper.ShipmentAmendmentReasonCode);

			shipmentAction.B0_ActionCode = MessageActionCodes.Codes.Cancellation;
			AssertEquals("No default for other Change Action", ZString.Empty, shipmentWrapper.ShipmentAmendmentReasonCode);

			shipmentAction.Shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			AssertEquals("Default to 03 for Split shipment", ShipmentAmendmentCodes.Codes.C03, shipmentWrapper.ShipmentAmendmentReasonCode);

			shipmentAction.B0_AmendmentReason = ShipmentAmendmentCodes.Codes.C01;
			AssertEquals("No default when amendment reason not empty", ShipmentAmendmentCodes.Codes.C01, shipmentWrapper.ShipmentAmendmentReasonCode);
		}
	}
}
