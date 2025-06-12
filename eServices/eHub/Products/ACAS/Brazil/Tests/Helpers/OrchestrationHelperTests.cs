using System;
using System.IO;
using System.Linq;
using CargoWise.eHub.Core.Orchestrations.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrchestrationHelper = CargoWise.eHub.Products.ACAS.BR.Helpers.OrchestrationHelper;
using System.Xml;

namespace CargoWise.eHub.Products.ACAS.BR.Tests.Helpers
{
    [TestClass]
	public class OrchestrationHelperTests : TestBase
    {
	    [TestMethod]
	    public void TestGetWcfInsertInboxAndDeleteAsyncPollingRegistration()
	    {
		    var pollingPK = new Guid("00000000-0000-0000-0000-000000000000");
		    var xmlTestString = OrchestrationHelper.GetWcfInsertInboxAndDeleteAsyncPollingRegistration(pollingPK.ToString(), "WTLEDINPN", "ACAS_BR", "<Content>Test</Content>");
			TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.InsertInboxAndDeleteSubscriptionType_Expected.xml"), xmlTestString);
	    }

	    [TestMethod]
	    public void TestGetWcfInsertInboxAndUpdateSubscriptionType()
	    {
            var content = new XmlDocument();
            content.LoadXml("<AAA><StateDescription>Protocol expired</StateDescription></AAA>");
		    var expectedXml = OrchestrationHelper.GetWcfInsertInboxAndUpdateAsyncPollingRegistration(
                "99999999-9999-9999-9999-999999999999", "WTLEDINPN", "ACAS_BR_FHL", content.InnerXml, content);
            TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.InsertInboxAndUpdateSubscriptionType_Expected.xml"), expectedXml);

        }

        [TestMethod]
        public void TestGetWcfInsertInbox()
        {
            var expectedXml = OrchestrationHelper.GetWcfInsertInbox(
                "WTLEDINPN", "ACAS_BR_FHL", "<Content>Test</Content>", Guid.Parse("33333333-3333-3333-3333-333333333333"));
            TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.InsertInbox_Expected.xml"), expectedXml);

        }

        [TestMethod]
        public void TestGetWcfInsertPollingXMLAndUpdateSubscriptionType()
        {
            var content = new XmlDocument();
            content.LoadXml("<AAA></AAA>");
            var expectedXml = OrchestrationHelper.GetWcfInsertPollingXMLAndUpdateAsyncPollingRegistration(
                "99999999-9999-9999-9999-999999999999", "WTLEDINPN", "ACAS_BR_FHL", content, "Protocol expired");
            TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.InsertInboxAndUpdateSubscriptionType_Expected.xml"), expectedXml);
        }

	    [TestMethod]
		public void TestGetWcfDeleteAsyncPollingRegistration()
	    {
			var expectedXml = OrchestrationHelper.GetWcfDeleteAsyncPollingRegistration("99999999-9999-9999-9999-999999999999");
			TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.DeleteAsyncPollingRegistration_Expected.xml"), expectedXml);
	    }

	    [TestMethod]
		public void TestGetResponseSenderID()
	    {
			Assert.AreEqual("ACAS_BRTest", OrchestrationHelper.GetResponseSenderID("ACAS_BR_FHL_TST"));
			Assert.AreEqual("ACAS_BR", OrchestrationHelper.GetResponseSenderID("ACAS_BR_FHL"));
	    }

		[TestMethod]
		public void TestGetViewableErrorDescription()
		{
			Assert.AreEqual("Error Description", OrchestrationHelper.GetViewableErrorDescription("Error Description"));

			var error =
				@"An error occurred while processing the message, refer to the details section for more information 
Message ID: {D0238657-ABC5-456E-AD19-D0DFD2393A8C}
Instance ID: {8DF0686F-1B38-45B9-A792-4D5BA010E0B0}
Error Description: There was a failure executing the response(receive) pipeline: ""CargoWise.eHub.Core.Pipelines.JsonReceive, CargoWise.eHub.Core.Pipelines, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350"" Source: ""JSON decoder"" Send Port: ""ACAS_BR_HTTP_Json_Snd"" URI: ""httpex://acas_br_json/"" Reason: Root node name is not specifed in Json decoder properties
";

			Assert.AreEqual("There was a failure executing the response(receive) pipeline: \"CargoWise.eHub.Core.Pipelines.JsonReceive, CargoWise.eHub.Core.Pipelines, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350\" Source: \"JSON decoder\" Send Port: \"ACAS_BR_HTTP_Json_Snd\" URI: \"httpex://acas_br_json/\" Reason: Root node name is not specifed in Json decoder properties", 
				OrchestrationHelper.GetViewableErrorDescription(error));
		}

        [TestMethod]
        public void TestGetResponseFromJson()
        {
            var doc = OrchestrationHelper.GetXmlResponse(new StreamReader(GetEmbeddedResource("Helpers.TestFiles.CCTCheckStatusResponse.txt")).ReadToEnd());
            TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.CCTCheckStatusResponse.xml"),
                doc);
        }

        [TestMethod]
        public void TestGetResponseFromJsonWithNoJSONArray()
        {
            var doc = OrchestrationHelper.GetXmlResponse(new StreamReader(GetEmbeddedResource("Helpers.TestFiles.CCTCheckStatusResponse_SingleNode.txt")).ReadToEnd());
            TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.CCTCheckStatusResponse_SingleNode.xml"), doc);
		}

		[TestMethod]
		public void TestGetResponse_WithOffsetDateTime_ShouldReturnMatchingDateTime()
		{
			var doc = OrchestrationHelper.GetXmlResponse(new StreamReader(GetEmbeddedResource("Helpers.TestFiles.CCTCheckStatusResponse_OffsetDateTime.txt")).ReadToEnd());
			TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.CCTCheckStatusResponse_OffsetDateTime.xml"),
				doc);
		}

		[TestMethod]
		public void TestGetResponse_WithUniversalDateTime_ShouldReturnMatchingDateTime()
		{
			var doc = OrchestrationHelper.GetXmlResponse(new StreamReader(GetEmbeddedResource("Helpers.TestFiles.CCTCheckStatusResponse_UniversalDateTime.txt")).ReadToEnd());
			TestHelper.AssertXmlAreEqual(GetEmbeddedResource("Helpers.TestFiles.CCTCheckStatusResponse_UniversalDateTime.xml"),
				doc);
		}


		[TestMethod]
        public void TestGetValueFromJson_GetMessage()
        {
	        var jsonString = GetResourceAsString("Helpers.TestFiles.CCTResponseJson.txt");
	        var value = OrchestrationHelper.GetValueFromJson(jsonString, "message");

	        Assert.AreEqual("O Portal Único de Comércio Exterior encontra-se em uma parada programada que ocorre diariamente entre 01:00 e 03:00.", value);
        }

        [TestMethod]
        public void TestGetValueFromJson_GetCode()
        {
	        var jsonString = GetResourceAsString("Helpers.TestFiles.CCTResponseJson.txt");
	        var value = OrchestrationHelper.GetValueFromJson(jsonString, "code");

	        Assert.AreEqual("PUCX-ER0401", value);
        }

		[TestMethod]
		public void TestGetValueFromErrorResponseMsg_JsonGetCode()
		{
			var jsonString = GetResourceAsString("Helpers.TestFiles.CCTResponseJson.txt");
			var value = OrchestrationHelper.GetValueFromErrorResponseMsg(jsonString, "code");

			Assert.AreEqual("PUCX-ER0401", value);
		}

		[TestMethod]
		public void TestGetValueFromErrorResponseMsg_JsonGetMessage()
		{
			var jsonString = GetResourceAsString("Helpers.TestFiles.CCTResponseJson.txt");
			var value = OrchestrationHelper.GetValueFromErrorResponseMsg(jsonString, "message");

			Assert.AreEqual("O Portal Único de Comércio Exterior encontra-se em uma parada programada que ocorre diariamente entre 01:00 e 03:00.", value);
		}

		[TestMethod]
		public void TestGetValueFromErrorResponseMsg_XmlGetMessage()
		{
			var value = OrchestrationHelper.GetValueFromErrorResponseMsg(xmlResponseString, "message");

			Assert.AreEqual("Test message", value);
		}

		[TestMethod]
		public void TestGetValueFromErrorResponseMsg_XmlGetCode()
		{
			var value = OrchestrationHelper.GetValueFromErrorResponseMsg(xmlResponseString, "code");

			Assert.AreEqual("Test code", value);
		}

		[TestMethod]
		public void IsNonXmlResponse_ValidXml_ReturnsFalse()
		{
			var validXml = "<root><element>value</element></root>";
			var result = OrchestrationHelper.IsNonXmlResponse(validXml);
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void IsNonXmlResponse_InvalidXml_ReturnsTrue()
		{
			var htmlString = GetResourceAsString("Helpers.TestFiles.acas_page.html.txt");
			var result = OrchestrationHelper.IsNonXmlResponse(htmlString);
			Assert.IsTrue(result);
		}

		[TestInitialize]
	    public void TestInitialize()
	    {
		    pkFirstCharacter = 0;
			WcfSqlOperationMessages.InternalNewGuid = () => getNewGuid(pkFirstCharacter++);
			WcfSqlOperationMessages.GetCurrentUtcTime = () => new DateTime(2019, 11, 26);
	    }

	    private int pkFirstCharacter;
	    readonly Func<int, Guid> getNewGuid = (int i) => new Guid(Enumerable.Repeat((byte)((i % 16) * 0x11), 16).ToArray());

		readonly string xmlResponseString = @"<body>
				<error>
					<code>Test code</code>
					<message>Test message</message>
				</error>
			</body>";
	}
}
