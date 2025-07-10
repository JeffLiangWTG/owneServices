using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface ITransitUniversalService
	{
		(string ErrorType, string Message) PublishUniversal(ZGuid[] jobPKs, string jobType);

		INotification GetNotificationForInstruction(string status, string failureReason);

		INotification GetNotificationForStopLoadInstructionEvent(string status, bool isCancel);
	}
}
