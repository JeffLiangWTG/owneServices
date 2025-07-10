using System;
using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public class ShipmentConsolDetachRequest : IShipmentConsolDetachRequest
	{
		public ShipmentConsolDetachRequest(string message, Func<ZString> cutOffDatePassedMessageGetter, Func<ZString> detachSubShipmentsMessageGetter, Func<(ZString, ZString, ZString)> exportNotification755MessageGetter)
		{
			restrictedMessage = message;
			cutOffDatePassedMessage = new Lazy<ZString>(cutOffDatePassedMessageGetter ?? (() => { return ZString.Empty; }));
			detachSubShipmentsMessage = new Lazy<ZString>(detachSubShipmentsMessageGetter ?? (() => { return ZString.Empty; }));
			exportNotification755Message = new Lazy<(ZString, ZString, ZString)>(exportNotification755MessageGetter ?? (() => { return (ZString.Empty, ZString.Empty, ZString.Empty); }));
		}

		readonly string restrictedMessage;
		readonly Lazy<ZString> cutOffDatePassedMessage;
		readonly Lazy<ZString> detachSubShipmentsMessage;
		readonly Lazy<(ZString receivedFFMAndDepartureMessage, ZString receivedAcceptedMessage, ZString notReceivedResponseMessage)> exportNotification755Message;

		public ZString RestrictedMessage
		{
			get { return restrictedMessage; }
		}

		public ZString AdditionalMessage_CutOffDatePassed
		{
			get { return cutOffDatePassedMessage.Value; }
		}

		public ZString AdditionalMessage_DetachSubShipments
		{
			get { return detachSubShipmentsMessage.Value; }
		}

		public ZString AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage
		{
			get { return exportNotification755Message.Value.receivedFFMAndDepartureMessage; }
		}

		public ZString AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage
		{
			get { return exportNotification755Message.Value.receivedAcceptedMessage; }
		}

		public ZString AdditionalMessage_ExportNotification755_AwaitingResponseMessage
		{
			get { return exportNotification755Message.Value.notReceivedResponseMessage; }
		}
	}
}
