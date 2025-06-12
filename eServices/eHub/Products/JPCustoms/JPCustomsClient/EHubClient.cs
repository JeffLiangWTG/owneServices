using System;
using System.ServiceModel;
using CargoWise.eHub.Products.JPCustoms.Common.Extensions;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public interface IEHubClient
	{
		void Send(string message);
	}

	public class EHubClient : IEHubClient
	{
		readonly IEHubClientConfiguration configuration;

		public EHubClient(IEHubClientConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			this.configuration = configuration;
		}

		public void Send(string message)
		{
			var channelFactory = new ChannelFactory<IJPCustomsReply>(configuration.EndpointConfigurationName);
			channelFactory.Endpoint.Contract.SessionMode = SessionMode.Allowed;
			try
			{
				var channel = channelFactory.CreateChannel();
				var encodedMessage = message.CompressAndEncode();
				channel.SendMessage(encodedMessage);
				channelFactory.Close();
			}
			catch (Exception)
			{
				channelFactory.Abort();
				throw;
			}

		}
	}
}
