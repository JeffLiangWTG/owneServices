using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(ShipmentAction))]
	sealed class ShipmentActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var shipment = Factory.New<Trip>().Shipments.AddNew();
			var action = new ShipmentAction(shipment, MessageTypes.Codes.eManifest);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.Original, action.B0_ActionCode);
			AssertEquals("B0_AmendmentReason.ReadOnly", true, action.B0_AmendmentReasonInfo.ReadOnly);
			AssertEquals("Shipment", shipment, action.Shipment);
			action = new ShipmentAction(shipment, MessageTypes.Codes.CompleteTrip);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.DoNotSend, action.B0_ActionCode);
			action = new ShipmentAction(shipment, MessageTypes.Codes.PreliminaryTrip);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.DoNotSend, action.B0_ActionCode);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Accepted;
			action = new ShipmentAction(shipment, MessageTypes.Codes.CompleteTrip);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.Link, action.B0_ActionCode);
			action = new ShipmentAction(shipment, MessageTypes.Codes.PreliminaryTrip);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.Link, action.B0_ActionCode);
			action = new ShipmentAction(shipment, MessageTypes.Codes.eManifest);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.Change, action.B0_ActionCode);
			AssertEquals("B0_AmendmentReason.ReadOnly", false, action.B0_AmendmentReasonInfo.ReadOnly);
			AssertEquals("Shipment", shipment, action.Shipment);
			action.B0_AmendmentReason = ShipmentAmendmentCodes.Codes.C01;
			action.B0_ActionCode = MessageActionCodes.Codes.Cancellation;
			AssertEquals("B0_AmendmentReason", ZString.Empty, action.B0_AmendmentReason);
			shipment.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Linked;
			action = new ShipmentAction(shipment, MessageTypes.Codes.CompleteTrip);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.DoNotSend, action.B0_ActionCode);
			action = new ShipmentAction(shipment, MessageTypes.Codes.PreliminaryTrip);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.DoNotSend, action.B0_ActionCode);
			shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			shipment.B0_ReleaseStatus = ZString.Empty;
			action = new ShipmentAction(shipment, MessageTypes.Codes.CompleteTrip);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.Link, action.B0_ActionCode);
			action = new ShipmentAction(shipment, MessageTypes.Codes.PreliminaryTrip);
			AssertEquals("B0_ActionCode", MessageActionCodes.Codes.Link, action.B0_ActionCode);
		}

		protected override BusinessObject GetNewBusinessObject() => new ShipmentAction(Factory.New<Trip>().Shipments.AddNew(), MessageTypes.Codes.eManifest);
	}
}
