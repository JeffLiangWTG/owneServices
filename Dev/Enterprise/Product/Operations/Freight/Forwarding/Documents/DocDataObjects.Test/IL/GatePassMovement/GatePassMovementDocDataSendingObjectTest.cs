using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL;
sealed class GatePassMovementDocDataSendingObjectTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("gatePassMovementDocDataObject is must", () => new GatePassMovementDocDataSendingObject(null, false));
	}

	public void TestGatePassMovementDocDataSendingObject_Properties()
	{
		var shipment = Factory.New<ForwardingShipment>();
		var dataObject = new GatePassMovementBuilder(new ForwardingShipmentGatePassMovementProvider(shipment)).Build();
		var gatePassMovementDocDataSendingObject = new GatePassMovementDocDataSendingObject(dataObject, true);

		AssertEquals(dataObject, gatePassMovementDocDataSendingObject.GatePassMovementDocDataObject);
		AssertEquals(true, gatePassMovementDocDataSendingObject.IsCancelActionTypeCode);
	}
}
