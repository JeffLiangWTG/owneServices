using System.Linq;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class CertificateOfOriginApprovalProcessor : BaseResponseSectionProcessor<CertificateOfOriginApproval>
	{
		public const string MessageType = "DCI";
		public const string MessageName = "Certificate Of Origin Approval message";

		public CertificateOfOriginApprovalProcessor(LoggingInformation logger, CertificateOfOriginApproval approvalMessage)
			: base(logger, approvalMessage, MessageType, MessageName)
		{
			Argument.NotNull(approvalMessage, nameof(approvalMessage));
		}

		protected override string URN => GetReferenceNumber(Section.OriginalApplication?.Header?.UniqueReferenceNumber);

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.COODCI;

		protected override string DoProcessingReturningStatusCore(EDIMessage message)
		{
			Logger.Log("Processing Certificate Of Origin Approval Message");

			var result = EDIMessage.Status.Failed;
			incomingMessage = message;

			if (Section.Approval != null)
			{
				Logger.Log("URN Number : " + URN);

				if (ProcessMessage())
				{
					result = EDIMessage.Status.Received;
					CompileCertificateOfOriginEmail();
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
			if ((Section.OriginalApplication?.Header?.CommonAccessReference ?? string.Empty) == CommonAccessReferenceCodeList.Codes.COODCI)
			{
				entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationPermitReceived;
				entry.CertificateNumber = Section.Approval.CertificateNumber;
			}
		}

		void CompileCertificateOfOriginEmail()
		{
			HtmlTableCreator creator = null;

			var conditions = Section.Approval.SCApprovalCondition;
			if (conditions != null && conditions.Any())
			{
				creator = new HtmlTableCreator(new string[] { "Type", "Condition Code", "Condition Description" });

				foreach (var condition in conditions)
				{
					creator.WriteRow(condition.AgencyCode ?? string.Empty, condition.ConditionCode ?? string.Empty, condition.ConditionDescription ?? string.Empty);
				}
			}

			CreateResponseEmail("Certificate of Origin for ", "Tcodec.htm", creator, entry.Declaration.JE_DeclarationReference, Section.Approval.CertificateApprovalDate, Section.Approval.CertificateNumber, entry.CH_Status, URN);
		}
	}
}
