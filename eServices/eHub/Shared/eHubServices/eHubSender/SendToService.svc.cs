using System.Collections.Specialized;
using System.Configuration;
using CargoWise.eHub.Share.eHubServices.eHubSender.ServiceProvider;
using Common.Logging;

namespace CargoWise.eHub.Share.eHubServices.eHubSender
{
	public class SendToService : CargoWise.eHub.Share.eHubServices.eHubSender.Common.SendToService
	{
		public SendToService() : base() { }

		public SendToService(ILog logger) : base(logger) { }

		protected override CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider.ServiceProvider GetServiceProviderCore(string senderId, string recipientId, string message)
		{
			return ServiceProviderFactory.Create(logger, senderId, recipientId, message);
		}

		protected override NameValueCollection GetAppSettings()
		{
			return ConfigurationManager.AppSettings;
		}
	}
}


