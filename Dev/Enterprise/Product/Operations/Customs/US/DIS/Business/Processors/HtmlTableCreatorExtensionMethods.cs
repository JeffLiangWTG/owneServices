using System.Linq;
using System.Xml.Linq;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.DIS.Business
{
	static class HtmlTableCreatorExtensionMethods
	{
		public static void WriteProcessingResultRows(this HtmlTableCreator htmlTable, XElement processingResultElement)
		{
			var xEvent = processingResultElement.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.ProcessingEvent));
			if (xEvent != null)
			{
				htmlTable.WriteRow("Event", xEvent.Value);
			}

			var xStatus = processingResultElement.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.ProcessingStatus));
			if (xStatus != null)
			{
				htmlTable.WriteRow("Status", xStatus.Value);
			}

			var xLogText = processingResultElement.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.ProcessingLogText));
			if (xLogText != null)
			{
				htmlTable.WriteRow("Log", xLogText.Value);
			}

			var xReviewStatusText = processingResultElement.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewStatus));
			if (xReviewStatusText != null)
			{
				htmlTable.WriteRow("Review Status", xReviewStatusText.Value);
			}

			var xReviewRejectReasonText = processingResultElement.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentRejectReason));
			if (xReviewRejectReasonText != null)
			{
				htmlTable.WriteRow("Review Reject Reason", xReviewRejectReasonText.Value);
			}

			var xDocumentReviewCommentText = processingResultElement.Elements().FirstOrDefault(x => x.Matches(EDIMessage.Constants.DocumentReviewComment));
			if (xDocumentReviewCommentText != null)
			{
				htmlTable.WriteRow("Document Review Comment", xDocumentReviewCommentText.Value);
			}
		}
	}
}
