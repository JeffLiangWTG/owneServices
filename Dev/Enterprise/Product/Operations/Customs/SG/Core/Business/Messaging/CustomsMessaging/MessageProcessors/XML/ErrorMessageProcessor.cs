using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class ErrorMessageProcessor : BaseResponseSectionProcessor<ErrorMessage>
	{
		public const string MessageType = "ERR";
		public const string MessageName = "Error Message";
		public const string DuplicateDeclarationErrorCode = "E17001";

		public ErrorMessageProcessor(LoggingInformation logger, ErrorMessage errorMessage)
			: base(logger, errorMessage, MessageType, MessageName)
		{
			Argument.NotNull(errorMessage, nameof(errorMessage));
		}

		protected override string URN => GetReferenceNumber(Section.UniqueReferenceNumber);

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.ERRORM;

		protected override string DoProcessingReturningStatusCore(EDIMessage message)
		{
			Logger.Log("Processing Error Message");

			var result = EDIMessage.Status.Failed;
			incomingMessage = message;

			var errorDetails = Section.ErrorDetail;

			if (errorDetails != null)
			{
				Logger.Log("URN Number : " + URN);

				if (ProcessMessage())
				{
					result = EDIMessage.Status.Received;

					if (IsDuplicateDeclarationError)
					{
						result = EDIMessage.Status.Discarded;
						if (OutgoingMessage.EM_Status == EDIMessage.Status.Queued || OutgoingMessage.EM_Status == EDIMessage.Status.Pending)
						{
							OutgoingMessage.EM_Status = EDIMessage.Status.Sent;
						}
						message.Notes.AddNew(true, "Incoming message discarded", "A duplicate error was returned. The message was discarded.");
					}

					if (syntaxError || !IsDuplicateDeclarationError)
					{
						CompileErrorEmail();
						SendErrorReport(entry, responseEmail);
					}
				}
			}
			else
			{
				responseEmail.Subject = "ERROR PROCESSING";
				responseEmail.Body = "FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText;
				SendErrorReport(null, responseEmail);
			}

			return result;
		}

		bool syntaxError;
		bool IsDuplicateDeclarationError => Section.ErrorDetail != null && Section.ErrorDetail.Any(c => string.Equals(c.ErrorCode, DuplicateDeclarationErrorCode, StringComparison.OrdinalIgnoreCase));

		protected override void SetEntryStatus()
		{
			syntaxError = false;

			if (Section.CommonAccessReference == CommonAccessReferenceCodeList.Codes.ERRORM && !IsDuplicateDeclarationError)
			{
				if (entry.IsOriginalPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationHadSyntaxErrors;
					syntaxError = true;
				}
				else if (entry.IsAmendmentPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentHadSyntaxErrors;
					syntaxError = true;
				}
				else if (entry.IsRefundPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.RefundHadSyntaxErrors;
					syntaxError = true;
				}
				else if (entry.IsCancellationPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationHadSyntaxErrors;
					syntaxError = true;
				}
			}
		}

		void CompileErrorEmail()
		{
			var errorDetails = Section.ErrorDetail.Where(IsNotEmpty);

			HtmlTableCreator creator = null;

			if (errorDetails.Any())
			{
				if (syntaxError)
				{
					creator = new HtmlTableCreator(new string[] { "Error Code", "Description", "Error Trace" });

					foreach (var errorDetail in errorDetails)
					{
						creator.WriteRow(errorDetail.ErrorCode ?? string.Empty, errorDetail.ErrorDescription ?? string.Empty, string.Join(System.Environment.NewLine, errorDetail.ErrorTrace ?? Array.Empty<string>()));
					}
				}
			}

			CreateResponseEmail("Error for ", "Aperak.htm", creator, entry.Declaration.JE_DeclarationReference, entry.CH_Status, URN);
		}

		bool IsNotEmpty(ErrorDetail errorDetail)
		{
			return errorDetail != null && (!string.IsNullOrWhiteSpace(errorDetail.ErrorCode) || !string.IsNullOrWhiteSpace(errorDetail.ErrorCode) || (errorDetail.ErrorTrace?.Any(c => !string.IsNullOrWhiteSpace(c)) ?? false));
		}
	}
}
