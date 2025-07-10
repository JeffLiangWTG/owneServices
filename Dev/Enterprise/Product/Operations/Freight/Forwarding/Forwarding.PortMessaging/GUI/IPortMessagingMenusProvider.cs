using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	interface IPortMessagingMenusProvider
	{
		void AddMenuItems(ZMenuItem parentMenu);
		bool Enabled { get; }
	}
}
