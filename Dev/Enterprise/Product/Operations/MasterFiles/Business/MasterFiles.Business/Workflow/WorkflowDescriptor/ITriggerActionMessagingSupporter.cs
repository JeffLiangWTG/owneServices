using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface ITriggerActionMessagingSupporter
	{
		void SendMessage(INotifications notifications, ZString queuedUserNK, ZString triggerAction);
	}

	public interface ITriggerActionMessagingSupporterProvider
	{
		ITriggerActionMessagingSupporter GetSupporter(string triggerAction);
	}
}
