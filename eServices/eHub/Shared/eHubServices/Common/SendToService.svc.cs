using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.Extensions;
using Common.Logging;
using System.Collections.Specialized;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.Common
{
	public abstract class SendToService : ISendToService
	{
		protected readonly ILog logger;

		static readonly ILog StaticLogger = LogManager.GetLogger(typeof(SendToService));

		public SendToService()
			: this(StaticLogger)
		{

		}

		public SendToService(ILog logger)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
		}

		public void Send(string senderId, string recipientId, string message)
		{
			DisableServerCertificateValidation();
			string messageToLog = message.TruncateForLogging();
			var messageId = Guid.NewGuid();

			logger.InfoFormat("Sending message {3} from {0} to {1} . Details: {2}", senderId, recipientId, messageToLog, messageId);

			try
			{
				GetServiceProvider(senderId, recipientId, message).Process();
				logger.InfoFormat("Messsage {0} was sent", messageId);
			}
			catch (Exception ex)
			{
				logger.ErrorFormat("Error during message {0} sending", ex, messageId);
				throw new FaultException(ex.Message);
			}

		}

		protected virtual ServiceProvider.ServiceProvider GetServiceProvider(string senderId, string recipientId, string message)
		{
			return GetServiceProviderCore(senderId, recipientId, message);
		}

		protected abstract ServiceProvider.ServiceProvider GetServiceProviderCore(string senderId, string recipientId, string message);

		protected abstract NameValueCollection GetAppSettings();

		#region Remove Certificate

		void DisableServerCertificateValidation()
		{
			if (bool.Parse(GetAppSettings()["DisableSSLCertificateValidation"]) && ServicePointManager.ServerCertificateValidationCallback == null)
			{
				ServicePointManager.ServerCertificateValidationCallback = TrustAllCertificatesCallback;
			}
		}

		bool TrustAllCertificatesCallback(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors errors)
		{
			return true;
		}

		#endregion
	}
}


