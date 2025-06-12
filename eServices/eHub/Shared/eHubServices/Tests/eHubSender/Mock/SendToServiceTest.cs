using System;
using CargoWise.eHub.Share.eHubServices.eHubSender;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender.Mock
{
	public class SendToServiceTest : SendToService
	{
		readonly ServiceProvider serviceProvider;

		public SendToServiceTest(ServiceProvider serviceProvider)
		{
			if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
			this.serviceProvider = serviceProvider;
		}

		protected override ServiceProvider GetServiceProvider(string senderId, string recipientId, string message)
		{
			return serviceProvider;
		}
	}
}
