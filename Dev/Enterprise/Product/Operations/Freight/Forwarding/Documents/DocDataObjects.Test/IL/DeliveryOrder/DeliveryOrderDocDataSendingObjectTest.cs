using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.IL
{
	[TestedType(typeof(DeliveryOrderDocDataSendingObject))]
	sealed class DeliveryOrderDocDataSendingObjectTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("DeliveryOrderDocDataObject is must", () => new DeliveryOrderDocDataSendingObject(null, false));
		}

		public void TestDeliveryOrderDocDataObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var dataObject = new DeliveryOrderBuilder(shipment).Build();
			var deliveryOrderDocDataSendingObject = new DeliveryOrderDocDataSendingObject(dataObject, true);
			AssertEquals(deliveryOrderDocDataSendingObject.DeliveryOrderDocDataObject, dataObject);
			AssertEquals(deliveryOrderDocDataSendingObject.IsCancelActionTypeCode, true);
		}
	}
}
