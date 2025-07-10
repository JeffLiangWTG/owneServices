using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentConsolDetachRequestTest : TestCase
	{
		public void TestDefaultValues()
		{
			var request = new ShipmentConsolDetachRequest(null, null, null, null);
			Assert(request.RestrictedMessage.IsEmpty);
			Assert(request.AdditionalMessage_CutOffDatePassed.IsEmpty);
			Assert(request.AdditionalMessage_DetachSubShipments.IsEmpty);
			Assert(request.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage.IsEmpty);
			Assert(request.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage.IsEmpty);
			Assert(request.AdditionalMessage_ExportNotification755_AwaitingResponseMessage.IsEmpty);
		}

		public void TestProperties()
		{
			var cutOffDatePassedMessageGetterWasCalled = false;
			Func<ZString> cutOffDatePassedGetter = () =>
				{
					cutOffDatePassedMessageGetterWasCalled = true;
					return "cut off!";
				};

			var detachSubShipmentsMessageGetterWasCalled = false;
			Func<ZString> detachSubShipmentsGetter = () =>
			{
				detachSubShipmentsMessageGetterWasCalled = true;
				return "sub shipments";
			};

			var exportNotification755MessageGetterWasCalled = false;
			Func<(ZString, ZString, ZString)> exportNotification755MessageGetter = () =>
			{
				exportNotification755MessageGetterWasCalled = true;
				return ("Received FFM And Departure Message", "Received Accepted Message", "Awaiting Response Message");
			};

			var request = new ShipmentConsolDetachRequest("Hey", cutOffDatePassedGetter, detachSubShipmentsGetter, exportNotification755MessageGetter);
			AssertEquals("Hey", request.RestrictedMessage);
			AssertEquals("Precondition: lazy property", false, cutOffDatePassedMessageGetterWasCalled);
			AssertEquals("Precondition: lazy property", false, detachSubShipmentsMessageGetterWasCalled);
			AssertEquals("Precondition: lazy property", false, exportNotification755MessageGetterWasCalled);

			AssertEquals("cut off!", request.AdditionalMessage_CutOffDatePassed);
			AssertEquals("Property value was set", true, cutOffDatePassedMessageGetterWasCalled);
			AssertEquals("Precondition: lazy property", false, detachSubShipmentsMessageGetterWasCalled);

			AssertEquals("sub shipments", request.AdditionalMessage_DetachSubShipments);
			AssertEquals("Property value was set", true, detachSubShipmentsMessageGetterWasCalled);

			AssertEquals("Received FFM And Departure Message", request.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertEquals("Received Accepted Message", request.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertEquals("Awaiting Response Message", request.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);
			AssertEquals("Property value was set", true, exportNotification755MessageGetterWasCalled);

			cutOffDatePassedMessageGetterWasCalled = false;
			detachSubShipmentsMessageGetterWasCalled = false;
			exportNotification755MessageGetterWasCalled = false;

			AssertEquals("cut off!", request.AdditionalMessage_CutOffDatePassed);
			AssertEquals("Value created on first access, message getter is not called anymore", false, cutOffDatePassedMessageGetterWasCalled);

			AssertEquals("sub shipments", request.AdditionalMessage_DetachSubShipments);
			AssertEquals("Value created on first access, message getter is not called anymore", false, detachSubShipmentsMessageGetterWasCalled);

			AssertEquals("Received FFM And Departure Message", request.AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage);
			AssertEquals("Received Accepted Message", request.AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage);
			AssertEquals("Awaiting Response Message", request.AdditionalMessage_ExportNotification755_AwaitingResponseMessage);
			AssertEquals("Value created on first access, message getter is not called anymore", false, exportNotification755MessageGetterWasCalled);
		}
	}
}
