using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public interface IMessageFailStatusManager
	{
		bool IsMessageTypeSupported(ZString messageType);
		void SetFailStatus(MQEDIMessage message);
	}
}
