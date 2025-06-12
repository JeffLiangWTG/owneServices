using Microsoft.Extensions.Configuration;

namespace OcmPoc.Transceivers
{
	public interface IConnectionBuilder
	{
		void BindConfiguration(IConfigurationRoot configRoot);
		IReceivingConnection BuildReceivingConnection();
		ISendingConnection BuildSendingConnection();
	}
}
