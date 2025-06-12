using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Products.GBCustoms.CDS.BT.Helpers;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.CDS.BT.Tests.Helpers
{
	[TestClass]
    public class OrchestrationHelpersTests
    {
        const string filePath = ".TestFiles.";
        
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void OrchestrationHelpers_CreateAWSendMessage()
        {
            OrchestrationHelpers_CreateAWSendMessage("61c521e7db2044428b184d578429091e", "TestInput.xml", "TestInput2.xml", "TestAWSOutput.txt");
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void OrchestrationHelpers_CreateAWSendMessage_Example2()
		{
			OrchestrationHelpers_CreateAWSendMessage("c12ab61d57424665895510ae3442e792", "TestFileUpload_Example2_step1.xml", "TestFileUpload_Example2_step2.xml", "TestFileUpload_Example2_AWSOutput.txt");
		}

        void OrchestrationHelpers_CreateAWSendMessage(string boundary, string inputTestFile, string inputTestFile2, string outputTestMimeFile)
        {
            var inputXml = GetResourceContent(inputTestFile);
            var doc = XDocument.Parse(inputXml);

            var imageElement = doc.XPathSelectElement("UniversalEvent/Event/AttachedDocumentCollection/AttachedDocument/ImageData");
            Assert.IsNotNull(imageElement);
            Assert.IsTrue(!string.IsNullOrEmpty(imageElement.Value));

            var contentType = "multipart/form-data";
            var keys = "key,acl,x-amz-credential,x-amz-algorithm,x-amz-date,policy,x-amz-signature";
            
            inputXml = GetResourceContent(inputTestFile2);
            XmlDocument xmlMessage = new XmlDocument();
            xmlMessage.LoadXml(inputXml);
            var fileName = "CW1testV3.jpg";
            var senderId = "16893581-84F9-4850-B4AE-438BDB6370E7";
            var recipientId = "F1992A01-FFAD-446A-81D0-57E0DEAC8116";
            var fileContentType = "image/jpg";

            var requestMessage = (MemoryStream)OrchestrationHelpers.CreateAWSendMessage(keys, xmlMessage, contentType, boundary, imageElement.Value, fileName, fileContentType, senderId, recipientId, new NoOpLogger());
            Assert.IsNotNull(requestMessage);
            Assert.IsTrue(requestMessage.Length > 0);

            var contentStream = GetResourceStream(outputTestMimeFile);
            var expectedMessage = new MemoryStream();
            contentStream.CopyTo(expectedMessage);

            expectedMessage.Position = 0;
            requestMessage.Position = 0;

            var expected = new StreamReader(expectedMessage).ReadToEnd();
            var result = new StreamReader(requestMessage).ReadToEnd();

            requestMessage.Position = 0;
            expectedMessage.Position = 0;
            Assert.AreEqual(expected, result);
        }


        string GetResourceContent(string resourceName)
        {
            using (var reader = new StreamReader(GetResourceStream(resourceName)))
            {
                return reader.ReadToEnd();
            }
        }

        Stream GetResourceStream(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Concat(this.GetType().Namespace, filePath, resourceName));
        }

        [TestMethod]
        public void OrchestrationHelpers_GetHeaderValue()
        {
            const string headers = "Content-Length: 0\r\nX-Conversation-Id: 3cb3f94f-2098-4e08-99e8-dfcd41319643";

            var result = OrchestrationHelpers.GetHttpHeaderValue(headers, "X-Conversation-Id");

            Assert.AreEqual("3cb3f94f-2098-4e08-99e8-dfcd41319643", result);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void OrchestrationHelpers_RemoveImageDataNode()
        {
            var mockMessage = MockRepository.GenerateMock<XLANGMessage>();
            var mockPart = MockRepository.GenerateMock<XLANGPart>();
            string inputMsg =  GetResourceContent("TestRemoveImageDataNode_Input.xml");

            var mockLog = MockRepository.GenerateMock<ILog>();
            var bodyStream = new MemoryStream(Encoding.Default.GetBytes(inputMsg));

            mockMessage.Stub(x => x[0]).Return(mockPart);
            mockPart.Stub(x => x.RetrieveAs(typeof(Stream))).Return(bodyStream);

            var actual = OrchestrationHelpers.RemoveImageDataNode(mockMessage, mockLog);
           
            var expected = GetResourceContent("TestRemoveImageDataNode_Output.xml");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void OrchestrationHelpers_SubscribeUploadFiles()
        {
            var input = GetResourceContent("HMRC_Response.xml");
            var expectedOutput = GetResourceContent("FileUploadSubscription.xml");

            var inputXmlDoc = new XmlDocument();
            inputXmlDoc.LoadXml(input);

            var sqlOutput = OrchestrationHelpers.SubscribeUploadFiles(inputXmlDoc, "SubscriptionType", "Sender", "Recipient", "eHubTrackingID");

            Assert.AreEqual(expectedOutput, sqlOutput);
        }

        [TestMethod]
        public void OrchestrationHelpers_ConvertDestinationParty_ShouldChange()
        {
            var input = "GBCustomsTest-DirectDocument";
            var expectedOutput = "GBCustomsTest-Direct";
            Assert.AreEqual(expectedOutput, OrchestrationHelpers.ConvertDestinationParty(input));
        }

        [TestMethod]
        public void OrchestrationHelpers_ConvertDestinationParty_ShouldNotChange()
        {
            var input = "GBCustoms-GVMS";
            var expectedOutput = "GBCustoms-GVMS";
            Assert.AreEqual(expectedOutput, OrchestrationHelpers.ConvertDestinationParty(input));
        }

        [TestMethod]
        public void OrchestrationHelpers_ConvertDestinationParty_EmptyTest()
        {
            Assert.AreEqual(string.Empty, OrchestrationHelpers.ConvertDestinationParty(null));
        }

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void OrchestrationHelpers_EscapeXML()
        {
            var mockLog = MockRepository.GenerateMock<ILog>();
            var input = @"<ns0:GBCustomsTransportResponse xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms""><CSPID>hnHJTJZN00ii14uVXn+Xeg==</CSPID><eHubMessageTrackingId>4cc97186-4d96-48d3-a2d7-8b955e7f977a</eHubMessageTrackingId></ns0:GBCustomsTransportResponse>";

            Assert.AreEqual(@"&lt;ns0:GBCustomsTransportResponse xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms""&gt;&lt;CSPID&gt;hnHJTJZN00ii14uVXn+Xeg==&lt;/CSPID&gt;&lt;eHubMessageTrackingId&gt;4cc97186-4d96-48d3-a2d7-8b955e7f977a&lt;/eHubMessageTrackingId&gt;&lt;/ns0:GBCustomsTransportResponse&gt;", OrchestrationHelpers.EscapeXML(input, mockLog));
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void OrchestrationHelpers_GetQueryUrl()
		{
			Assert.AreEqual("MockBaseUrl/search?MockQueryString", OrchestrationHelpers.GetQueryUrl(QueryType.List, "MockBaseUrl", "MockEntryNumberType", "MockEntryNumber", "MockQueryString"));
			Assert.AreEqual("MockBaseUrl/mockentrynumbertype/MockEntryNumber/status", OrchestrationHelpers.GetQueryUrl(QueryType.Status, "MockBaseUrl/{0}/{1}/{2}", "MockEntryNumberType", "MockEntryNumber", "MockQueryString"));
			Assert.AreEqual("MockBaseUrl/MockEntryNumber", OrchestrationHelpers.GetQueryUrl(QueryType.VAT, "MockBaseUrl", "MockEntryNumberType", "MockEntryNumber", "MockQueryString"));
			Assert.AreEqual("MockBaseUrl", OrchestrationHelpers.GetQueryUrl(QueryType.NOP, "MockBaseUrl", "MockEntryNumberType", "MockEntryNumber", "MockQueryString"));
			Assert.AreEqual("MockBaseUrl", OrchestrationHelpers.GetQueryUrl(QueryType.EORI, "MockBaseUrl", "MockEntryNumberType", "MockEntryNumber", "MockQueryString"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void OrchestrationHelpers_GetQueryMethod()
		{
			Assert.AreEqual("GET", OrchestrationHelpers.GetQueryMethod(QueryType.List));
			Assert.AreEqual("GET", OrchestrationHelpers.GetQueryMethod(QueryType.Status));
			Assert.AreEqual("GET", OrchestrationHelpers.GetQueryMethod(QueryType.VAT));
			Assert.AreEqual("POST", OrchestrationHelpers.GetQueryMethod(QueryType.NOP));
			Assert.AreEqual("POST", OrchestrationHelpers.GetQueryMethod(QueryType.EORI));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void OrchestrationHelpers_GetQueryContentType()
		{
			Assert.AreEqual("application/xml; charset=UTF-8", OrchestrationHelpers.GetQueryContentType(QueryType.List));
			Assert.AreEqual("application/xml; charset=UTF-8", OrchestrationHelpers.GetQueryContentType(QueryType.Status));
			Assert.AreEqual("application/xml; charset=UTF-8", OrchestrationHelpers.GetQueryContentType(QueryType.VAT));
			Assert.AreEqual("application/json", OrchestrationHelpers.GetQueryContentType(QueryType.EORI));
			Assert.AreEqual("application/json", OrchestrationHelpers.GetQueryContentType(QueryType.NOP));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void OrchestrationHelpers_GetQueryAcceptHeader()
		{
			Assert.AreEqual("application/vnd.hmrc.5.0+xml", OrchestrationHelpers.GetQueryAcceptHeader(QueryType.List, "5.0"));
			Assert.AreEqual("application/vnd.hmrc.5.0+xml", OrchestrationHelpers.GetQueryAcceptHeader(QueryType.Status, "5.0"));
			Assert.AreEqual("application/vnd.hmrc.5.0+xml", OrchestrationHelpers.GetQueryAcceptHeader(QueryType.VAT, "5.0"));
			Assert.AreEqual("application/vnd.hmrc.5.0+json", OrchestrationHelpers.GetQueryAcceptHeader(QueryType.NOP, "5.0"));
			Assert.AreEqual("application/vnd.hmrc.5.0+json", OrchestrationHelpers.GetQueryAcceptHeader(QueryType.EORI, "5.0"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void OrchestrationHelpers_GetQueryBody()
		{
			Assert.AreEqual("<body />", OrchestrationHelpers.GetQueryBody(QueryType.List, "MockPayload"));
			Assert.AreEqual("<body />", OrchestrationHelpers.GetQueryBody(QueryType.Status, "MockPayload"));
			Assert.AreEqual("<body />", OrchestrationHelpers.GetQueryBody(QueryType.VAT, "MockPayload"));
			Assert.AreEqual("MockPayload", OrchestrationHelpers.GetQueryBody(QueryType.EORI, "MockPayload"));
			Assert.AreEqual("MockPayload", OrchestrationHelpers.GetQueryBody(QueryType.NOP, "MockPayload"));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void OrchestrationHelpers_SetResponseMessage_Status()
		{
			Assert.AreEqual(GetResourceContent("QueryResponseList.xml"), RemoveEventTime(OrchestrationHelpers.SetResponseMessage(QueryType.List, "MockResponseString", "MockEhubTrackingID", "MockJobNumber", "MochOrganization", "MockEntryNumberType", "MockEntryNumber")));
			Assert.AreEqual(GetResourceContent("QueryResponseStatus.xml"), RemoveEventTime(OrchestrationHelpers.SetResponseMessage(QueryType.Status, "MockResponseString", "MockEhubTrackingID", "MockJobNumber", "MochOrganization", "MockEntryNumberType", "MockEntryNumber")));
			Assert.AreEqual(GetResourceContent("QueryResponseVAT.xml"), RemoveEventTime(OrchestrationHelpers.SetResponseMessage(QueryType.VAT, "MockResponseString", "MockEhubTrackingID", "MockJobNumber", "MochOrganization", "MockEntryNumberType", "MockEntryNumber")));
			Assert.AreEqual(GetResourceContent("QueryResponseEORI.xml"), RemoveEventTime(OrchestrationHelpers.SetResponseMessage(QueryType.EORI, "MockResponseString", "MockEhubTrackingID", "MockJobNumber", "MochOrganization", "MockEntryNumberType", "MockEntryNumber")));
			Assert.AreEqual(GetResourceContent("QueryResponseNOP.xml"), RemoveEventTime(OrchestrationHelpers.SetResponseMessage(QueryType.NOP, "MockResponseString", "MockEhubTrackingID", "MockJobNumber", "MochOrganization", "MockEntryNumberType", "MockEntryNumber")));
		}

		private string RemoveEventTime(string xmlString)
		{
			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlString);

			var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
			nsmgr.AddNamespace("u", "http://www.cargowise.com/Schemas/Universal/2011/11");
			nsmgr.AddNamespace("ue", "http://www.cargowise.com/Schemas/Universal/2012/11");

			var eventTimeNode = xmlDoc.SelectSingleNode("//ue:EventTime", nsmgr);
			eventTimeNode.ParentNode.RemoveChild(eventTimeNode);

			return xmlDoc.OuterXml;
		}
	}
}
