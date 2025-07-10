using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class CustomsServiceErrorMessageProcessor : TRBranchCustomsApplicationTypeMessageProcessor<TRBaseMessage>
	{
		public CustomsServiceErrorMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		HtmlTableCreator tableCreator;

		protected override string MessageFriendlyNameCore => Res.GetString("3436D40D-7FE9-43B7-A8D7-0E9EBCCEC4A0", "TR Customs Service Error Message Processor");

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.TRCustoms;

		protected override bool ProcessMessageCore(TRBaseMessage message)
		{
			var result = false;
			tableCreator = new HtmlTableCreator();
			var headerAttachee = message.EM_LinkedObject as IMessageAttachee;

			if (headerAttachee != null)
			{
				var headerTextDictionary = MessageHelper.GetHeaderTextDictionary(message.EM_MessageText);
				tableCreator.WriteRow(Res.GetString("8F754687-AAE5-403E-B247-D428FA0901D6", "Error Type:"), headerTextDictionary.TryGetValue("custom.ErrorType", out var errorTypeValue) ? errorTypeValue : string.Empty);
				tableCreator.WriteRow(Res.GetString("599A38A2-745F-4E61-A525-C893A22FBDE9", "Notification Time:"), headerTextDictionary.TryGetValue("custom.NotificationTime", out var notificationTimeValue) ? notificationTimeValue : string.Empty);
				tableCreator.WriteRow(Res.GetString("4DF12BC0-3AFA-4182-97EB-08960AD0F418", "Notification Type:"), headerTextDictionary.TryGetValue("custom.NotificationType", out var notificationType) ? notificationType : string.Empty);
				tableCreator.WriteRow(Res.GetString("0C5550F5-DE29-4DF1-AB2E-FF2EB34FDBCD", "Error Description:"), headerTextDictionary.TryGetValue("custom.ErrorDescription", out var errorDescription) ? errorDescription : string.Empty);

				message.EM_MessageInterpretation = CreateInterpretationAndMailBodyContent(headerAttachee);

				SendNotificationEmailIfNeeded(headerAttachee, message, false);

				result = false;
			}

			return result;
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, TRBaseMessage message)
		{
			return Res.GetString("0467F08E-ECDB-44FB-9D5F-F8FA3D008CD5", "TR Customs Service Error Message");
		}

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, TRBaseMessage message, bool isSuccess)
		{
			return CreateInterpretationAndMailBodyContent(messageAttacheeBO);
		}

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var headerAttachee = linkedObject as IMessageAttachee;
			return headerAttachee?.GlobalBranchPK ?? ZGuid.Empty;
		}

		protected override void UpdateStatusCore(TRBaseMessage message, bool isSuccess)
		{
			base.UpdateStatusCore(message, isSuccess);

			var headerAttachee = message.EM_LinkedObject as IMessageAttachee;
			if (headerAttachee != null)
			{
				headerAttachee.CustomsStatus = isSuccess ? TRMessageStatusCodeList.Codes.CLR : TRMessageStatusCodeList.Codes.Error;
				headerAttachee.MessageStatus = isSuccess ? TRMessageStatusCodeList.Codes.Accepted : TRMessageStatusCodeList.Codes.Error;
			}
		}

		ZString CreateInterpretationAndMailBodyContent(IMessageAttachee messageAttacheeBO)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("B9F60F6E-8279-44EA-A812-4A01DB73D40C", "Message for job {0} has been rejected. For details please follow the Link to the job", messageAttacheeBO.JobReference));
			htmlBody.Append("<br /><br />");
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}
	}
}
