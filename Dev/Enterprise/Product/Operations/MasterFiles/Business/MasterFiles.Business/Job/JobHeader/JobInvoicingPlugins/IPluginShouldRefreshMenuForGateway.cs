
namespace Enterprise.MasterFiles.Business
{
	public interface IPluginShouldRefreshMenuForGateway
	{
		void RefreshGatewayElements(bool isGatewayEnabled);
	}

	public interface IPluginForGatewaySellApportionments : IPluginShouldRefreshMenuForGateway
	{
		void SyncGatewaySellToCostIfNecessary();
	}
}