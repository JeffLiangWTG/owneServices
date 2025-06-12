using System.Reflection;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.Helpers
{
    [TestFixture]
    public class OrchestrationHelpersTests
    {
		[Test]
        public void OrchestrationHelpers_EncodeResponseText()
        {
            var responseText = "<?xml version='1.0' encoding='UTF-8'?><errorResponse><code>CDS60001</code><message>Declaration not found</message>";
            var encodedResponseText = OrchestrationHelpers.EncodeResponseText(responseText);
            Assert.AreEqual("PD94bWwgdmVyc2lvbj0nMS4wJyBlbmNvZGluZz0nVVRGLTgnPz48ZXJyb3JSZXNwb25zZT48Y29kZT5DRFM2MDAwMTwvY29kZT48bWVzc2FnZT5EZWNsYXJhdGlvbiBub3QgZm91bmQ8L21lc3NhZ2U+", encodedResponseText);
        }

		[TestCase("SuccessResponse.xml", "Empty", "SuccessResponse.xml")]
		[TestCase("SuccessResponse.json", "Empty", "SuccessResponse.json")]
		[TestCase("ErrorResponse_Small.xml", "Empty", "ErrorResponse_Small.xml")]
		[TestCase("ErrorResponse_Gateway.xml", "ErrorResponse_Gateway.html", "ErrorResponse_Gateway_Scrubbed.xml")]
		[TestCase("ErrorResponse_404.xml", "Empty", "ErrorResponse_404.xml")]
		[TestCase("ErrorResponse_Big.xml", "Empty", "ErrorResponse_Big.xml")]
		public void OrchestrationHelpers_ExtracAndRemoveHTMLcontentFromErrorResponse(string responseFile, string expectedHtmlFile, string expectedXmlFile)
        {
            var responseXml = GetResource(responseFile);
            var expectedResponseXml = GetResource(expectedXmlFile);
            var expectedHtml = expectedHtmlFile == "Empty" ? string.Empty : GetResource(expectedHtmlFile);

            var extractedHTML = OrchestrationHelpers.ExtracAndRemoveHTMLcontentFromErrorResponse(ref responseXml);

            Assert.AreEqual(expectedHtml, extractedHTML);
            Assert.AreEqual(expectedResponseXml, responseXml);
        }

		[Test]
		public void OrchestrationHelpers_GetInnerXmlFromXelement()
		{
			var responseText = "<?xml version='1.0' encoding='UTF-8'?><OuterTag><code>CDS60001</code><message>Declaration not found</message></OuterTag>";
			var xDoc = XDocument.Parse(responseText);
			Assert.AreEqual("<code>CDS60001</code><message>Declaration not found</message>", OrchestrationHelpers.GetInnerXmlFromXelement(xDoc.Root));
		}

		[Test]
		public void OrchestrationHelpers_ExtractJsonValue()
		{
			var jsonString = GetResource("BigFileUploadResponse_Success.txt");
			var extractedValue = OrchestrationHelpers.ExtractJsonValue(MockRepository.GenerateStub<ILog>(), jsonString, "uploadRequest.href");

			Assert.AreEqual("https://www.externaltest.upscan.tax.service.gov.uk/v1/uploads/fus-inbound-8a7204a4f7373185b08ff170fdbfc676", extractedValue);
		}

		[Test]
		public void OrchestrationHelpers_ExtractJsonValue_XmlMix()
		{
			var jsonString = GetResource("BigFileUploadResponse_JsonAndXml.txt");
			var extractedValue = OrchestrationHelpers.ExtractJsonValue(MockRepository.GenerateStub<ILog>(), jsonString, "departureId");

			Assert.AreEqual("668bc55638b8b197", extractedValue);
		}

		string GetResource(string source)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream($"CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.Helpers.TestFiles.{source}").ReadToEnd();
        }

    }
}
