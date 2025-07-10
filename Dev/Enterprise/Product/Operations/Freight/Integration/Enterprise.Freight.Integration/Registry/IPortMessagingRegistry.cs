using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Integration
{
	public interface IPortMessagingRegistry
	{
		BooleanRegistryItem AllowToSendExportNotification { get; }

		BooleanRegistryItem AllowToSendExportNotificationToCargonaut { get; }
	}
}
