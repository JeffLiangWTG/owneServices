using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using CargoWise.xTMessaging.Integration;
using CargoWise.xTMessaging.Shared;
using Environment = System.Environment;
using ILogger = CargoWise.xTMessaging.Integration.ILogger;
using LogType = CargoWise.xTMessaging.Integration.LogType;

namespace CargoWise.RefDbRepo.ILReferenceData.CmdLine
{
	abstract class BaseSending<IRequestParam>
	{
		protected async Task<bool> Send(ILogger logger, string[] cmdLineArguments)
		{
			if (!ArgumentParserCore(cmdLineArguments, out IRequestParam[] requestParams, out string message))
			{
				return ExitProgram(Errors.BadCommandLineArguments, new string[] { message }, logger);
			}

			var sentAllSuccessful = true;
			var errMsgs = new List<string>();

			if (requestParams is null || requestParams.Length == 0)
			{
				return ExitProgram(Errors.BadCommandLineArguments, new string[] { "No request parameters found." }, logger);
			}

			foreach (var requestParam in requestParams)
			{
				var (sentSuccessful, _, errMsg) = await SendRequestAsync(logger, requestParam);
				sentAllSuccessful &= sentSuccessful;
				if (!string.IsNullOrEmpty(errMsg))
				{
					errMsgs.Add(errMsg);
				}
			}


			return sentAllSuccessful
				? ExitProgram(Errors.None, errMsgs.ToArray(), logger)
				: ExitProgram(Errors.SendInterchangeFailed, errMsgs.ToArray(), logger);
		}

		private async Task<(bool, long, string)> SendRequestAsync(ILogger logger, IRequestParam requestParam)
		{
			var requestXml = ProviderRequestXmlCore(requestParam);
			var soapEnvelopeWithRawBody = Helpers.CreateSoapEnvelopeWithRawBody(
				requestXml,
				MessageServiceNameProvider.GetServiceName(MessageSubType),
				ApplicationConfig.Instance.ConsumerId,
				Guid.NewGuid().ToString());
			var xtMessageInfo = new XtMessageInfo(soapEnvelopeWithRawBody, MessageSubType);

			var connect = ApplicationConfig.Instance.MsgClientConnect;
			var caBundle = ApplicationConfig.Instance.MsgClientCaBundle;
			var uri = ApplicationConfig.Instance.MsgClientURI;
			var password = ApplicationConfig.Instance.MsgClientApplicationIdentification;

			byte[] caBytes = Encoding.UTF8.GetBytes(caBundle);
			string caBase64Encoded = Convert.ToBase64String(caBytes);

			logger.Log(LogType.Information, $"Sending request with message sub type: {MessageSubType}, connect: {connect}, uri: {uri}, {UniversalDataHelper.HashPassword(password)}, caBundle: {caBase64Encoded}");

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
				receiveHandler: null))
			{
				var (sentSuccessful, msgId, message) = await connector.SendAsync(xtMessageInfo);
				return (sentSuccessful, msgId, message);
			}
		}

		protected abstract string ProviderRequestXmlCore(IRequestParam requestParam);

		protected abstract bool ArgumentParserCore(string[] cmdLineArguments, out IRequestParam[] requestParams, out string message);

		protected abstract string MessageSubTypeCore();

		string MessageSubType => MessageSubTypeCore();

		protected virtual IReceiveHandler GetReceiveHandler(Logger logger, long msgId)
		{
			return null;
		}

		static bool ExitProgram(Errors error, string[] messages, ILogger logger)
		{
			switch (error)
			{
				case Errors.None:
					foreach (var message in messages)
					{
						logger.Log(LogType.Information, message);
					}
					break;
				default:
					foreach (var message in messages)
					{
						logger.Log(LogType.Error, message);
					}
					break;
			}

			Environment.Exit((int)error);

			return true;
		}

	}
}
