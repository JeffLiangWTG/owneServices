using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Freight.Forwarding.GUI
{
	public abstract class DocDataObjectSendingMessageMethodApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		protected DocDataObjectSendingMessageMethodApplicator(DocDataObjectSendingMessageSettings settings, BusinessObjectFactory factory)
				: base(Res.GetString("0723785d-737c-4fef-a33d-d73f9217ba25", "Sending Messages In Bulk"), factory)
		{
			errorAction = settings.ErrorAction;
		}

		readonly ZString errorAction;

		protected abstract ZString NoSelectedErrorMessage { get; }
		protected abstract LogHyperlink GetHyperlink(BusinessObject bizObj);

		protected DocDataObjectReportSendingProvider ReportSendingProvider => docDataObjectReportSendingProvider ?? (docDataObjectReportSendingProvider = GetDocDataObjectReportSendingProvider());
		DocDataObjectReportSendingProvider docDataObjectReportSendingProvider;

		protected abstract DocDataObjectReportSendingProvider GetDocDataObjectReportSendingProvider();

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (IsValidBeforeSending(log, targets))
			{
				foreach (var target in targets)
				{
					if (!ContinueWithSendingMessage(log, target))
					{
						break;
					}
				}
			}
		}

		bool IsValidBeforeSending(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			if (targets.Length == 0)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, NoSelectedErrorMessage);
				return false;
			}

			var security = new DocumentSecurityService(ReportSendingProvider.MenuItem, ReportSendingProvider.ModuleIdentifier);
			if (!security.CanSendMessage)
			{
				var messageFormat = Res.GetString("10E82E94-DFB4-4E0F-9A95-1507F63E83BB", "{0}");
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, messageFormat, security.CannotSendMessageError);
				return false;
			}

			return true;
		}

		bool ContinueWithSendingMessage(IOperationalActionSectionLog log, BusinessObject target)
		{
			bool continueWithSendingMessage = true;
			var link = GetHyperlink(target);
			var notificationsHandler = new NotificationsHandler();

			if (ReportSendingProvider.SendMessage(target, notificationsHandler))
			{
				var messageFormat = Res.GetString("8F55CACD-BABB-42DC-8EB2-A730B7D55476", "{0:G} processed successfully.");
				log.NotifyFormat(OperationalActionLogErrorLevel.Informational, messageFormat, link);
			}

			else if (errorAction == DocDataObjectSendingMessageSettings.Codes.Skip)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "{0:G} {1}", link, string.Join("\r\n", notificationsHandler.Notifications.GetNotifications(NotificationType.MessageError).Select(x => x.Message))); // ErrorMessage
			}

			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Error, "{0:G} {1}", link, string.Join("\r\n", notificationsHandler.Notifications.GetNotifications(NotificationType.MessageError).Select(x => x.Message))); // ErrorMessage

				continueWithSendingMessage = false;
			}

			var warningNotification = notificationsHandler.Notifications.GetNotifications(NotificationType.Warning);
			if (warningNotification.Any())
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "{0:G} {1}", link, string.Join("\r\n", warningNotification.Select(x => x.Message))); // ErrorMessage
			}

			return continueWithSendingMessage;
		}
	}
}
