using System.Xml.Linq;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class HtmlTableCreatorExtensionMethodsTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestWriteProcessingResultRows()
		{
			var xml = @"<MessageProcessingResult>
          <ProcessingEvent>INITIAL_VALIDATION</ProcessingEvent>
          <ProcessingStatus>PASSED</ProcessingStatus>
          <ProcessingLogText>Submitted message received successfully in DIS. Ready for Review</ProcessingLogText>
        </MessageProcessingResult>";
			var element = XElement.Parse(xml);
			var table = new HtmlTableCreator();
			table.WriteProcessingResultRows(element);
			var tableHtml = table.ToHtml();
			AssertContains("INITIAL_VALIDATION", tableHtml);
			AssertContains("PASSED", tableHtml);
			AssertContains("Submitted message received successfully in DIS. Ready for Review", tableHtml);
			xml = @"<DocumentReviewResult>
			<DocumentReviewStatus>REJECTED</DocumentReviewStatus>
			<DocumentReviewComment>BTA anticipated arrival information is missing</DocumentReviewComment>
			<DocumentRejectReason>INCOMPLETE_DOCUMENT_SET</DocumentRejectReason>
		</DocumentReviewResult>";
			element = XElement.Parse(xml);
			table = new HtmlTableCreator();
			table.WriteProcessingResultRows(element);
			tableHtml = table.ToHtml();
			AssertContains("REJECTED", tableHtml);
			AssertContains("INCOMPLETE_DOCUMENT_SET", tableHtml);
			AssertContains("BTA anticipated arrival information is missing", tableHtml);
		}
	}
}
