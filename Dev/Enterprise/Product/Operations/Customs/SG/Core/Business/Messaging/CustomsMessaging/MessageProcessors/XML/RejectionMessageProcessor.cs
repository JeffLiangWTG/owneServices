using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.BatchProcessor;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging
{
	class RejectionMessageProcessor : BaseResponseSectionProcessor<RejectionMessage>
	{
		public const string MessageType = "ERR";
		public const string MessageName = "Rejection Message";

		public const string ControllingAgencyQuery = "AQR";
		public const string SingaporeCustomsQuery = "CQR";

		public RejectionMessageProcessor(LoggingInformation logger, RejectionMessage rejectionMessage)
			: base(logger, rejectionMessage, MessageType, MessageName)
		{
			Argument.NotNull(rejectionMessage, nameof(rejectionMessage));
			isQueryResponse = Section.StatusType == ControllingAgencyQuery || Section.StatusType == SingaporeCustomsQuery;
		}

		readonly bool isQueryResponse;

		protected override string URN => GetReferenceNumber(Section.UniqueReferenceNumber);

		protected override string GetCommonAccessReference() => CommonAccessReferenceCodeList.Codes.STATUS;

		protected override string DoProcessingReturningStatusCore(EDIMessage message)
		{
			Logger.Log("Processing Rejection Message");

			var result = EDIMessage.Status.Failed;
			incomingMessage = message;

			var rejectionDetails = Section.RejectionDetail;

			if (rejectionDetails != null)
			{
				Logger.Log("URN Number : " + URN);

				if (ProcessMessage())
				{
					result = EDIMessage.Status.Received;

					CompileErrorEmail();

					if (isQueryResponse)
					{
						SendImpedimentReport(entry, responseEmail);
					}
					else
					{
						SendErrorReport(entry, responseEmail);
					}
				}
			}
			else
			{
				responseEmail.Subject = "REJECTION PROCESSING";
				responseEmail.Body = "FATAL PROCESSING ERROR IN MESSAGE : " + System.Environment.NewLine + message.EM_FormattedMessageText;
				SendErrorReport(null, responseEmail);
			}

			return result;
		}

		protected override void SetEntryStatus()
		{
			if (isQueryResponse)
			{
				if (entry.IsOriginalPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationQuery;
				}
				else if (entry.IsAmendmentPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentQuery;
				}
				else if (entry.IsRefundPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.RefundQuery;
				}
				else if (entry.IsCancellationPending)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationQuery;
				}
			}
			else
			{
				if (entry.IsOriginalPending || entry.CH_Status == Core.SGConstants.DeclarationStatus.DeclarationQuery)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.DeclarationRejectedByCustoms;
				}
				else if (entry.IsAmendmentPending || entry.CH_Status == Core.SGConstants.DeclarationStatus.AmendmentQuery)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.AmendmentRejectedByCustoms;
				}
				else if (entry.IsRefundPending || entry.CH_Status == Core.SGConstants.DeclarationStatus.RefundQuery)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.RefundRejectedByCustoms;
				}
				else if (entry.IsCancellationPending || entry.CH_Status == Core.SGConstants.DeclarationStatus.CancellationQuery)
				{
					entry.CH_Status = Core.SGConstants.DeclarationStatus.CancellationRejectedByCustoms;
				}
			}
		}

		void CompileErrorEmail()
		{
			var rejectionDetails = Section.RejectionDetail
				.Where(IsNotEmpty);

			HtmlTableCreator creator = null;

			if (rejectionDetails.Any())
			{
				if (isQueryResponse)
				{
					creator = new HtmlTableCreator(new string[] { "Query Detail", "Query Description" });

					foreach (var rejectionDetail in rejectionDetails)
					{
						creator.WriteRow(rejectionDetail.ErrorID ?? string.Empty, string.Join(System.Environment.NewLine, rejectionDetail.ErrorDescription ?? Array.Empty<string>()));
					}
				}
				else
				{
					creator = new HtmlTableCreator(new string[] { "Error Code", "Error Sequence Numeric", "Error Description" });

					foreach (var rejectionDetail in rejectionDetails.OrderBy(c => c.ErrorSequenceNumeric))
					{
						creator.WriteRow(rejectionDetail.ErrorID ?? string.Empty, rejectionDetail.ErrorSequenceNumeric, string.Join(System.Environment.NewLine, rejectionDetail.ErrorDescription ?? Array.Empty<string>()));
					}
				}
			}

			var templateType = isQueryResponse ? "AperakQuery.htm" : "Aperak.htm";
			var subjectPrefix = isQueryResponse ? "Impediment Query from Customs for " : "Error for ";

			CreateResponseEmail(subjectPrefix, templateType, creator, entry.Declaration.JE_DeclarationReference, entry.CH_Status, URN);
		}

		bool IsNotEmpty(RejectionDetail rejectionDetail)
		{
			return rejectionDetail != null && (!string.IsNullOrWhiteSpace(rejectionDetail.ErrorID) || (rejectionDetail.ErrorDescription?.Any(c => !string.IsNullOrWhiteSpace(c)) ?? false));
		}
	}
}
