using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IShipmentConsolDetachRequest
	{
		ZString RestrictedMessage { get; }
		ZString AdditionalMessage_CutOffDatePassed { get; }
		ZString AdditionalMessage_DetachSubShipments { get; }
		ZString AdditionalMessage_ExportNotification755_ReceivedFFMAndDepartureMessage { get; }
		ZString AdditionalMessage_ExportNotification755_ReceivedAcceptedMessage { get; }
		ZString AdditionalMessage_ExportNotification755_AwaitingResponseMessage { get; }
	}
}
