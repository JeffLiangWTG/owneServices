using System;
using System.Configuration;
using System.ServiceModel;
using CargoWise.eHub.Products.JPCustoms.Client;
using CargoWise.eHub.Products.JPCustoms.Common;
using CargoWise.eHub.Products.JPCustoms.Common.Extensions;
using CargoWise.eHub.Shared.IssueManager;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.PullService.Common
{
	public interface IJPCustomsPullManager
	{
		void PullCustomsForNewMessageAndPushToEHub();
	}

	public class JPCustomsPullManager : IJPCustomsPullManager
	{
		readonly ILog logger;
		readonly IAuditLogger auditLogger;
		readonly IJPCustomsPullClient pullClient;
		readonly IEHubClient eHubClient;
		protected internal IssueManager issueManager;

		public DateTime? IssueStartTime { get; set; }

		public virtual IssueManager IssueManager => issueManager ?? (issueManager = new IssueManager());

		public JPCustomsPullManager(ILog logger, IAuditLogger auditLogger, IJPCustomsPullClient pullClient, IEHubClient eHubClient, IssueManager issueManager)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;

			if (auditLogger == null) throw new ArgumentNullException("auditLogger");
			this.auditLogger = auditLogger;

			if (pullClient == null) throw new ArgumentNullException("pullClient");
			this.pullClient = pullClient;

			if (eHubClient == null) throw new ArgumentNullException("eHubClient");
			this.eHubClient = eHubClient;

			if (issueManager == null) throw new ArgumentNullException("issueManager");
			this.issueManager = issueManager;

			IssueStartTime = null;
		}

		public void PullCustomsForNewMessageAndPushToEHub()
		{
			logger.Debug("PullCustomsForNewMessageAndPushToEHub was called.");

			try
			{
				pullClient.Connect();

				for (int i = 1; i <= pullClient.MessageCount; i++)
				{
					var message = pullClient.GetNextMessage();

					if (!string.IsNullOrWhiteSpace(message))
					{
						var messageToLog = message.TruncateForLogging();

						try
						{
							eHubClient.Send(message);
						}
						catch (TimeoutException ex)
						{
							ReportToIssueManagerIfTimeHasElapsed(messageToLog, ex);
							continue;
						}
						catch (EndpointNotFoundException ex)
                        {
							ReportToIssueManagerIfTimeHasElapsed(messageToLog, ex);
							continue;
						}
						catch (Exception ex)
						{
							if (logger.IsErrorEnabled)
								logger.Error(string.Format("eHubClient.Send: Unable to send message to eHub: {0}", messageToLog), ex);
							issueManager.ReportToIssueManager("JPCustoms Pull Manager - eHubClient.Send: Unable to send message to eHub.", ex, logger, ConfigurationManager.AppSettings);
							continue;
						}
						IssueStartTime = null;
						logger.Info(string.Format("Message sent to eHub {0}", messageToLog));
						auditLogger.ReplyReceived(message);
					}
					else
					{
						logger.Warn("Empty message was received from JPCustoms. Message will be deleted.");
					}
					pullClient.Delete();
				}

				pullClient.Quit();
			}
			catch (MailboxLockedByAnotherClientException ex)
			{
				if (logger.IsInfoEnabled) logger.Info(string.Format("PullCustomsForNewMessageAndPushToEHub completed. Mailbox was locked but another client. Lock Details: {0}", ex.Message));
				issueManager.ReportToIssueManager("JPCustoms Pull Manager - PullCustomsForNewMessageAndPushToEHub completed. Mailbox was locked but another client.", ex, logger, ConfigurationManager.AppSettings);
			}
			finally
			{
				pullClient.Disconnect();
			}

			logger.Debug("PullCustomsForNewMessageAndPushToEHub completed, all message processed.");
		}


		internal void ReportToIssueManagerIfTimeHasElapsed(string messageToLog, Exception ex)
		{
			if (logger.IsErrorEnabled)
				logger.Error(string.Format("eHubClient.Send: Unable to send message to eHub: {0}", messageToLog), ex);
			if (IssueStartTime == null)
			{
				IssueStartTime = DateTime.UtcNow;
			}
			else if (DateTime.UtcNow >= IssueStartTime?.AddMinutes(Convert.ToDouble(ConfigurationManager.AppSettings["ReportToIssueManagerIntervalInMinutes"])))
			{
				var reportSubject = string.Format("JPCustoms Pull Manager - eHubClient.Send: Unable to send message to eHub after retrying for {0} minutes.", ConfigurationManager.AppSettings["ReportToIssueManagerIntervalInMinutes"]);
				issueManager.ReportToIssueManager(reportSubject, ex, logger, ConfigurationManager.AppSettings);
			}
		}
	}
}
