using System;
using System.Linq;
using System.ServiceModel;
using CargoWise.eHub.Products.NZCustoms.Client;
using Common.Logging;
using System.Threading;
using CargoWise.eHub.Products.NZCustoms.Gateway.HealthCheck;
using CargoWise.eHub.Products.NZCustoms.Common;

namespace CargoWise.eHub.Products.NZCustoms.Gateway
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
	public class MessageGateway : IMessageGateway
	{
		const int MAX_RETRY_IN_MINUTES = 10;
		#region logger

		static readonly ILog logger = LogManager.GetLogger<MessageGateway>();

		protected virtual ILog Logger
		{
			get
			{
				return logger;
			}
		}

		const string messageLoggerName = "MessageLogger";
		static readonly ILog messageLogger = LogManager.GetLogger(messageLoggerName);

		protected virtual ILog MessageLogger
		{
			get
			{
				return messageLogger;
			}
		}

		#endregion

		public Response SendLodgement(string reference, string messageType, string authentication, string message, string eHubTrackingID)
		{
			if (string.IsNullOrEmpty(eHubTrackingID))
			{
				return SendLodgementOld(reference, messageType, authentication, message);
			}
			else
			{
				return SendLodgementWithRetries(reference, messageType, authentication, message, eHubTrackingID);
			}
		}

		Response SendLodgementOld(string reference, string messageType, string authentication, string message)
		{
			var error = string.Empty;

			try
			{
				error = SendLodgementCore(reference, messageType, authentication, message);
				if (error != string.Empty) Logger.Warn(failedToSendMessage + error);
			}
			catch (Exception ex)
			{
				Logger.Warn(failedToSendMessage, ex);
				error = ex.Message;
			}

			if (error != string.Empty) throw new FaultException(error);
			return null;
		}

		Response SendLodgementWithRetries(string reference, string messageType, string authentication, string message, string eHubTrackingID, int retries = 0)
		{
			var response = new Response { MessageTrackingID = eHubTrackingID };
			var shouldRetry = true;

			try
			{
				Logger.Info("Sending message with trackingID: " + eHubTrackingID);
				response.ErrorMessage = SendLodgementCore(reference, messageType, authentication, message);
				if (response.ErrorMessage != string.Empty)
				{
					Logger.Warn(failedToSendMessage + response.ErrorMessage);
					shouldRetry = false;
					response.IsSuccess = false;
				}
			}
			catch (Exception ex)
			{
				Logger.Warn(failedToSendMessage, ex);
				response.IsSuccess = false;
				response.ErrorMessage = ex.Message;
				if (ex is FaultException && ex.Message.StartsWith("General Security Fault"))
				{
					if (retries == 0)
					{
						return SendLodgementWithRetries(reference, messageType, authentication, message, eHubTrackingID, ++retries);
					}
					else if (retries >= 1 && retries <= MAX_RETRY_IN_MINUTES)
					{
						Sleep(60000);
						return SendLodgementWithRetries(reference, messageType, authentication, message, eHubTrackingID, ++retries);
					}
					else
					{
						return response;
					}
				}
				else if(ex is NZCustomsInvalidOperationException)
				{
					shouldRetry = false;
				}
			}

			// Let Biztalk SendPort retries by throwing FaultException, Transport Advanced Options's Enable routing for failed message option will fail the message at the end of the retries if it is enabled.
			if (!response.IsSuccess && shouldRetry)
			{
				throw new FaultException(response.ErrorMessage);
			}

			return response;
		}

		protected virtual void Sleep(int miliseconds)
		{
			Thread.Sleep(miliseconds);
		}

		string SendLodgementCore(string reference, string messageType, string authentication, string message)
		{
			if (string.IsNullOrEmpty(reference)) return "Message Reference is empty.";
			if (string.IsNullOrEmpty(messageType)) return "MessageType is empty.";
			if (string.IsNullOrEmpty(message)) return "Message to NZCustoms is empty.";

			if (MessageLogger.IsDebugEnabled) Logger.Debug(String.Format("Sending message with reference {0} and messageType {1} and authentication {2}", reference, messageType, authentication != null ? authentication : string.Empty));

			var lodgmentRequestBuilder = LodgmentRequestDirector.Create(Logger, messageType, authentication, message);
			if (MessageLogger.IsTraceEnabled) MessageLogger.Trace(lodgmentRequestBuilder.GetMessageForLogging());

			var submitLodgementRequest = lodgmentRequestBuilder.Create();
			if (submitLodgementRequest == null) return string.Format("Cannot parse NZ Customs message.");
			if (submitLodgementRequest.DocumentManifest != null
			    && submitLodgementRequest.DocumentManifest.ManifestItem.Any(item => item.UniformResourceIdentifier.StartsWith(" ")))
			{
				return string.Format("Attachment document filename can't start with empty spaces.");
			}

			var submitLodgementResponse = ServiceAPI.SendLodgementRequest(submitLodgementRequest);
			if (submitLodgementResponse == null) return "Response from NZ Customs: SubmitLodgementResponse is null.";

			var messageInfo = submitLodgementResponse.MessageInfo;
			if (messageInfo == null) return "Response from NZ Customs: MessageInfo is null.";

			if (messageInfo.Status != "Lodgement Received and being processed.") return String.Format("Failed message lodgment status recieved: {0}.", messageInfo.Status);

			Logger.Info("Sent message successfully");
			return string.Empty;
		}

        public bool Ping()
        {
            return true;
        }

		protected virtual IServiceApi ServiceAPI
		{
			get
			{
				return new ServiceApi(logger);
			}
		}

		const string failedToSendMessage = "Failed to send message. ";
	}
}
