using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class ApprovalMessageProcessor : BaseResponseSectionProcessor<ApprovalMessage>
	{
		public const string MessageType = "CAN";
		public const string MessageName = "Approval Message";

		public ApprovalMessageProcessor(LoggingInformation logger, ApprovalMessage approvalMessage)
			: base(logger, approvalMessage, MessageType, MessageName)
		{
			Argument.NotNull(approvalMessage, nameof(approvalMessage));
		}

		protected override string URN => GetReferenceNumber(Section.UniqueReferenceNumber);

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.STATUS;

		protected override string DoProcessingReturningStatusCore(EDIMessage message)
		{
			Logger.Log("Processing Approval Message");

			var result = EDIMessage.Status.Failed;
			incomingMessage = message;

			if (Section.ApprovalCondition != null)
			{
				Logger.Log("URN Number : " + URN);

				if (ProcessMessage())
				{
					result = EDIMessage.Status.Received;
					CompileCancellationResponseEmail();
					SendAcknowledgementReport(entry, responseEmail);
				}
			}
			else
			{
				responseEmail.Subject = "ERROR PROCESSING";
				responseEmail.Body = "FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + incomingMessage.EM_FormattedMessageText;

				SendErrorReport(null, responseEmail);
			}

			return result;
		}

		protected override void SetEntryStatus()
		{
			if (Section.CommonAccessReference == CommonAccessReferenceCodeList.Codes.STATUS)
			{
				entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationAccepted;
			}
		}

		void CompileCancellationResponseEmail()
		{
			var creator = new HtmlTableCreator(new string[] { "Condition Code", "Condition Description" });

			foreach (var condition in Section.ApprovalCondition)
			{
				creator.WriteRow(condition.ConditionCode ?? string.Empty, condition.FreeText != null ? string.Join(System.Environment.NewLine, condition.FreeText) : string.Empty);
			}

			CreateResponseEmail("Cancellation for ", "Cusres.htm", creator, entry.Declaration.JE_DeclarationReference, entry.CH_Status, URN);
		}
	}
}
