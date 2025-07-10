using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public interface IExlMessageDeliveryCreator
	{
		IMessageProcessor CreateExlMessageDelivery(MessageProcessorCommunicationModesResult modes, BusinessObject bizObjToDeliver, ProcessTaskNotification action, EventInfoProvider eventInfoProvider);
	}
}
