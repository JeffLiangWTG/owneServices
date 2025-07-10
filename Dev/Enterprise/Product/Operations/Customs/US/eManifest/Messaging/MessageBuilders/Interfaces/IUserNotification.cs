using CargoWise.Types;
using Enterprise.Customs.US.eManifest.Business;

namespace Enterprise.Customs.US.eManifest.Messaging
{
	public interface IUserNotification : Customs.Business.MessageManagers.IUserNotification
	{
		bool ShowShipmentsActionsDialog(IShipmentActionsProvider provider, ZString messageDescription);
	}
}
