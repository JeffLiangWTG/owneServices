using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class XElementExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestMatches()
		{
			var element = XElement.Parse(TestHelper.DocumentValidationResponseFailedXml);
			var processedMessageHeader = element.Descendants().FirstOrDefault(x => x.Matches("ProcessedMessageHeader"));
			AssertNotNull(processedMessageHeader);
		}

		public void TestHasFailed()
		{
			var element = XElement.Parse(TestHelper.DocumentValidationResponseFailedXml);
			var messageHeader = element.Descendants().FirstOrDefault(x => x.Matches("MessageProcessingResult"));
			Assert(messageHeader.HasFailed());
			element = XElement.Parse(TestHelper.DocumentValidationPassedResponseXml);
			messageHeader = element.Descendants().FirstOrDefault(x => x.Matches("MessageProcessingResult"));
			Assert(!messageHeader.HasFailed());
		}
	}
}
