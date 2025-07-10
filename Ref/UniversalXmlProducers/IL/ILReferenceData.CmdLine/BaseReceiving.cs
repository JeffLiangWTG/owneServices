using System;
using System.Text;
using System.Threading;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.xTMessaging.Integration;
using CargoWise.xTMessaging.Shared;
using ILogger = CargoWise.xTMessaging.Integration.ILogger;
using LogType = CargoWise.xTMessaging.Integration.LogType;

namespace CargoWise.RefDbRepo.ILReferenceData.CmdLine
{
	public class BaseReceiving
	{
		public BaseReceiving(Logger logger)
		{
			this.logger = logger;
		}

		public bool Receive()
		{
			var logger = new Logger();

			var (receivingSuccessful, errMsg) = ReceiveResponse();

			if (!receivingSuccessful)
			{
				logger.Log(LogType.Error, errMsg);
			}

			return receivingSuccessful;
		}

		private (bool, string) ReceiveResponse()
		{
			var receiveHandler = GetReceiveHandler(logger);
			if (receiveHandler == null)
			{
				return (true, string.Empty);
			}

			var connect = ApplicationConfig.Instance.MsgClientConnect;
			var caBundle = ApplicationConfig.Instance.MsgClientCaBundle;
			var uri = ApplicationConfig.Instance.MsgClientURI;
			var password = ApplicationConfig.Instance.MsgClientApplicationIdentification;

			byte[] caBytes = Encoding.UTF8.GetBytes(caBundle);
			string caBase64Encoded = Convert.ToBase64String(caBytes);
			logger.Log(LogType.Information, $"Receive response with connect: {connect}, uri: {uri}, {UniversalDataHelper.HashPassword(password)}, caBundle: {caBase64Encoded}");

			var msgClientProvider = new BasicMsgClientProvider(
				connect: connect,
				caBundle: caBundle,
				uri: uri,
				password: password,
				timeout: TimeSpan.FromSeconds(ApplicationConfig.Instance.MsgClientTimeoutInSeconds),
				token: CancellationToken.None);

			using (var connector = new DirectXtConnectorProxy(
				msgClientProvider: msgClientProvider,
				xTMessagingConfig: new DirectXtMessagingConfig(),
				logger: logger,
				msgAttributeModifier: null,
				receiveHandler: receiveHandler
				))
			{
				var (receiveSucceeded, message) = connector.Receive();

				if (receiveSucceeded)
				{
					(receiveHandler as IPostReceiveHandlerSupport)?.PostReceiveHandler();
				}

				return (receiveSucceeded, message);
			}
		}

		protected virtual IReceiveHandler GetReceiveHandler(ILogger logger) => new BaseReceiveHandler(logger);
		readonly ILogger logger;
	}
}
