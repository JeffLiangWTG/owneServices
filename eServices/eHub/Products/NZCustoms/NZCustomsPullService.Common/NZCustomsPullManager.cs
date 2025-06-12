using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using CargoWise.eHub.Products.NZCustoms.Client;
using CargoWise.eHub.Products.NZCustoms.Client.RequestLodgeResponse_v1;
using CargoWise.eHub.Products.NZCustoms.Common;
using Common.Logging;
using System.Threading;

namespace CargoWise.eHub.Products.NZCustoms.PullService
{
	public interface INZCustomsPullManager
	{
		bool PullNZCustomsServiceForNewMessageAndPushItToBiztalk();
	}

	public class NZCustomsPullManager : INZCustomsPullManager
	{
		protected string PreviousMailboxMsgId { get; set; }
		const string MessageNotFoundPatternString = "<Status>NOT FOUND</Status>";

		public NZCustomsPullManager(ILog logger, IConfigurationProvider configurationProvider, IServiceApi serviceAPI, IFileManager fileManager)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;

			if (configurationProvider == null) throw new ArgumentNullException("configurationProvider");
			this.configurationProvider = configurationProvider;

			if (serviceAPI == null) throw new ArgumentNullException("serviceAPI");
			this.serviceAPI = serviceAPI;

			if (fileManager == null) throw new ArgumentNullException("fileManager");
			this.fileManager = fileManager;

			PreviousMailboxMsgId = string.Empty;
		}

		public bool PullNZCustomsServiceForNewMessageAndPushItToBiztalk()
		{
			var pullResult = PullNZCustomsServiceForNewMessage();

			if (pullResult != null && pullResult.Status == PullResult.PullStatus.MessageReceived)
			{
				var succeed = SendMessageToBiztalk(pullResult);
				if (succeed) logger.Info("Processed message successfully.");
				return succeed;
			}

			return false;
		}

		protected virtual PullResult PullNZCustomsServiceForNewMessage()
		{
			try
			{
				if (logger.IsDebugEnabled) logger.DebugFormat("Requesting lodgement response for MailboxMsgId '{0}'.", PreviousMailboxMsgId);

				var requestLodgeResponseResponse = serviceAPI.SendPullRequest(GetPullRequest(PreviousMailboxMsgId));

				if (requestLodgeResponseResponse == null || !requestLodgeResponseResponse.IsValid())
				{
					int retry = 0;
					while (retry < configurationProvider.RetryCount && (requestLodgeResponseResponse == null || !requestLodgeResponseResponse.IsValid()))
					{
						Thread.Sleep(500);
						requestLodgeResponseResponse = serviceAPI.SendPullRequest(GetPullRequest(PreviousMailboxMsgId));
						retry++;
					}

					if (requestLodgeResponseResponse == null)
					{
						if (InvalidResponseLogger.IsWarnEnabled) InvalidResponseLogger.WarnFormat("Invalid RequestLodgeResponseResponse Received, previous MailboxMsgId is [{0}]. RequestLodgeResponseResponse is null.", PreviousMailboxMsgId);
						return null;
					}
					if (!requestLodgeResponseResponse.IsValid())
					{
						if (InvalidResponseLogger.IsWarnEnabled) InvalidResponseLogger.WarnFormat("Invalid RequestLodgeResponseResponse Received, previous MailboxMsgId is [{0}]. {1}", PreviousMailboxMsgId, requestLodgeResponseResponse.ToString());
						
						if(!string.IsNullOrEmpty(requestLodgeResponseResponse.RequestResponseResponse?.MailboxMsgId))
						{
							PreviousMailboxMsgId = requestLodgeResponseResponse.RequestResponseResponse.MailboxMsgId;
						}
						
						return null;
					}
				}

				if (requestLodgeResponseResponse.IsEmpty())
				{
					logger.InfoFormat("Found no messages using MailboxMsgId [{0}]", PreviousMailboxMsgId);
					return null;
				}

				if (logger.IsDebugEnabled) logger.DebugFormat("requestLodgeResponseResponse.RequestResponseResponse.MailboxMsgId {0}", requestLodgeResponseResponse.RequestResponseResponse.MailboxMsgId);
				if (logger.IsDebugEnabled) logger.DebugFormat("PullNZCustomsServiceForNewMessage Attachment Count {0}", requestLodgeResponseResponse.attachments.Length);

				PreviousMailboxMsgId = requestLodgeResponseResponse.RequestResponseResponse.MailboxMsgId;

				return new PullResult(
					customerReference: GetClientID(requestLodgeResponseResponse.RequestResponseResponse.MessageName, requestLodgeResponseResponse.RequestResponseResponse.Partner),
					messageReference: requestLodgeResponseResponse.RequestResponseResponse.CustomerReference,
					messageName: requestLodgeResponseResponse.RequestResponseResponse.MessageName,
					attachments: requestLodgeResponseResponse.attachments
					);
			}
			catch (Exception ex)
			{
				logger.Warn("Exception thrown during message pulling.", ex);
			}

			return null;
		}

		protected virtual bool SendMessageToBiztalk(PullResult pullResult)
		{
			if (logger.IsDebugEnabled) logger.Debug(String.Format("Sending to BizTalk message with CustomerReference: {0} and MessageReference: {1}", pullResult.CustomerReference, pullResult.MessageReference));

			var message = new NZCustomsReply { Reference = pullResult.CustomerReference, Content = pullResult.Content, Attachments = pullResult.Attachments };

			try
			{
				CreateChannelAndSendMessage(message);
				return true;
			}
			catch (Exception ex)
			{
				string matchedPattern = FindAny(ex.Message, configurationProvider.ExceptionMessagePatterns);
				if (!string.IsNullOrEmpty(matchedPattern))
				{
					string fileName = fileManager.SaveReceivedMessage(message, pullResult.MessageReference);
					if (logger.IsWarnEnabled) logger.Warn(string.Format("Unable to send message with CustomerReference = {0} to BizTalk. Exception message match '{1}' pattern from configuration. Message saved into {2} and removed from queue. Please fix the error and message will be reprocessed automatically.", pullResult.CustomerReference, matchedPattern, fileName), ex);
					return true;
				}
				else
				{
					PreviousMailboxMsgId = string.Empty;
					if (logger.IsWarnEnabled) logger.Warn(String.Format("Unable to send message with CustomerReference = {0} to BizTalk.", pullResult.CustomerReference), ex);
					return false;
				}
			}
		}

		protected virtual void CreateChannelAndSendMessage(NZCustomsReply message)
		{
			var channelFactory = new ChannelFactory<INZCustomsReply>("NZCustomsReplyService");
			channelFactory.Endpoint.Contract.SessionMode = SessionMode.Allowed;
			try
			{
				var channel = channelFactory.CreateChannel();
				channel.SendMessage(message);
				channelFactory.Close();
			}
			catch (Exception)
			{
				channelFactory.Abort();
				throw;
			}
		}

		#region Implementation

		RequestLodgeResponseRequest GetPullRequest(string previousMailboxMsgId)
		{
			return new RequestLodgeResponseRequest() { Submitter = configurationProvider.PartnerID, MailboxMsgId = previousMailboxMsgId };
		}

		string GetClientID(string messageName, string partner)
		{
			string clientId = partner;
			if (!string.IsNullOrWhiteSpace(messageName))
			{
				var parts = messageName.Split('_');

				if (parts.Length > 0)
				{
					clientId = parts[0];
				}
			}

			return clientId;
		}

		static string FindAny(string message, string[] patterns)
		{
			foreach (var pattern in patterns)
			{
				if (message.Contains(pattern))
				{
					return pattern;
				}
			}

			return string.Empty;
		}

		public virtual ILog InvalidResponseLogger
		{
			get
			{
				return invalidResponseLogger;
			}
		}

		readonly ILog invalidResponseLogger = LogManager.GetLogger("InvalidResponseLogger");
		readonly ILog logger;
		readonly IConfigurationProvider configurationProvider;
		readonly IFileManager fileManager;
		readonly IServiceApi serviceAPI;

		#endregion
	}
}
