using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;

using NUnit.Framework;

using System.Collections.Generic;
using System.Xml.Linq;

namespace CargoWise.eHub.Products.GBCustoms.CSPs.Common.Tests
{
    [TestFixture]
    public class HTTPHelperTests
    {
        const string xPath = "/notifications/notification/headers/header";

        [Test]
        public void TestFindElementValueOrDefault_Response_ByxPath()
        {
            var responseText = @"<Response xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:noNamespaceSchemaLocation='https://www.myvan.descartes.com/schemas/myVan/HttpPostResponse.xsd'>
    <host>VAL3</host>
    <service>SimpleUploadHandler</service>
    <created>2020-12-15T16:19:41Z</created>
    <version>4.0.7448.36665</version>
    <bytesReceived>867</bytesReceived>
    <tid>e67f417f-93d9-48e2-a494-4be63d3e28ea</tid>
</Response>";
            string xPath2 = "//*[local-name()='Response']/*[local-name()='tid']";

            Assert.AreEqual("e67f417f-93d9-48e2-a494-4be63d3e28ea", HTTPHelper.FindElementValueOrDefault(responseText, xPath2));
        }

		[Test]
		public void TestFindElementValueOrDefault_Request_ByxPath()
		{
			var requestText = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
	<Header>
		<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
			<Provider>MCP</Provider>
			<Service>NewDeclaration</Service>
			<Credentials Key=""AIYPR1.GB238406267000.FT2"">
				<User>FTPX</User>
				<Password>DhgyDzlrnEFzleFfhCva1hZF7sZXnkVfslBLACD5nK9nSzWCG8CR+aIvEyBjkt3tgSjpCKPTUY7HWgSWazgZ5YNkp6x/j03JdJCd0SmBSdoP9wPldtYCvQ6afn3t7t04wl7kf/VXcsHrQA+cKu1vDsnGfmfnS6GOwFv7spzz8aU=</Password>
				<Topic>FTP0</Topic>
				<Badge>FTP</Badge>
			</Credentials>
			<JobNumber>SFF000018877</JobNumber>
		</GBCustomsRequest>
	</Header>
	<Body>
		<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
			<Declaration xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
				<FunctionCode>9</FunctionCode>
				<FunctionalReferenceID>AIY024PR10000000006739</FunctionalReferenceID>
				<TypeCode>IMD</TypeCode>
				<GoodsItemQuantity>2</GoodsItemQuantity>
				<TotalPackageQuantity>23</TotalPackageQuantity>
				<AdditionalDocument>
					<CategoryCode>1</CategoryCode>
					<ID>8497711</ID>
					<TypeCode>DAN</TypeCode>
				</AdditionalDocument>
				<Agent>
					<FunctionCode>2</FunctionCode>
				</Agent>
			</Declaration>
		</MetaData>
	</Body>
</ns0:GBCustoms>";
			string xPath2 = "//*[local-name()='Declaration']/*[local-name()='FunctionalReferenceID']";

			Assert.AreEqual("AIY024PR10000000006739", HTTPHelper.FindElementValueOrDefault(requestText, xPath2));
		}

		[Test]
		public void TestElementExists_Success()
		{
			var requestText = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
	<Header>
		<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
			<Provider>MCP</Provider>
			<Service>NewDeclaration</Service>
			<Credentials Key=""AIYPR1.GB238406267000.FT2"">
				<User>FTPX</User>
				<Password>DhgyDzlrnEFzleFfhCva1hZF7sZXnkVfslBLACD5nK9nSzWCG8CR+aIvEyBjkt3tgSjpCKPTUY7HWgSWazgZ5YNkp6x/j03JdJCd0SmBSdoP9wPldtYCvQ6afn3t7t04wl7kf/VXcsHrQA+cKu1vDsnGfmfnS6GOwFv7spzz8aU=</Password>
				<Topic>FTP0</Topic>
				<Badge>FTP</Badge>
			</Credentials>
			<JobNumber>SFF000018877</JobNumber>
		</GBCustomsRequest>
	</Header>
	<Body>
		<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
			<Declaration xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
				<FunctionCode>9</FunctionCode>
				<FunctionalReferenceID>AIY024PR10000000006739</FunctionalReferenceID>
				<TypeCode>IMD</TypeCode>
				<GoodsItemQuantity>2</GoodsItemQuantity>
				<TotalPackageQuantity>23</TotalPackageQuantity>
				<AdditionalDocument>
					<CategoryCode>1</CategoryCode>
					<ID>8497711</ID>
					<TypeCode>DAN</TypeCode>
				</AdditionalDocument>
				<Agent>
					<FunctionCode>2</FunctionCode>
				</Agent>
			</Declaration>
		</MetaData>
	</Body>
</ns0:GBCustoms>";

			string xPath = "//*[local-name()='Body']";

			Assert.IsTrue(HTTPHelper.ElementExists(requestText, xPath));
		}

		[Test]
		public void TestElementExists_Failure()
		{
			var requestText = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
	<Header>
		<GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
			<Provider>MCP</Provider>
			<Service>NewDeclaration</Service>
			<Credentials Key=""AIYPR1.GB238406267000.FT2"">
				<User>FTPX</User>
				<Password>DhgyDzlrnEFzleFfhCva1hZF7sZXnkVfslBLACD5nK9nSzWCG8CR+aIvEyBjkt3tgSjpCKPTUY7HWgSWazgZ5YNkp6x/j03JdJCd0SmBSdoP9wPldtYCvQ6afn3t7t04wl7kf/VXcsHrQA+cKu1vDsnGfmfnS6GOwFv7spzz8aU=</Password>
				<Topic>FTP0</Topic>
				<Badge>FTP</Badge>
			</Credentials>
			<JobNumber>SFF000018877</JobNumber>
		</GBCustomsRequest>
	</Header>
	<Body />
</ns0:GBCustoms>";

			string xPath = "//*[local-name()='Body']";

			Assert.IsFalse(HTTPHelper.ElementExists(requestText, xPath));
		}

		[Test]
        public void TestFindHeaderValueOrDefault_Response_OneKey()
        {
            var response = new HTTPResponseMock();

            Assert.AreEqual("value2", HTTPHelper.FindHeaderValueOrDefault(response, "Key2"));
            Assert.AreEqual("", HTTPHelper.FindHeaderValueOrDefault(response, "key4"));
        }

        [Test]
        public void TestFindHeaderValueOrDefault_Response_ManyKeys()
        {
            var response = new HTTPResponseMock();

            Assert.AreEqual("value2", HTTPHelper.FindHeaderValueOrDefault(response, "Key2|key3"));
            Assert.AreEqual("value3", HTTPHelper.FindHeaderValueOrDefault(response, "key4|key3"));
            Assert.AreEqual("", HTTPHelper.FindHeaderValueOrDefault(response, "key4,key1,key2"));
            Assert.AreEqual("value1", HTTPHelper.FindHeaderValueOrDefault(response, "key4,key1,key2", ","));
            Assert.AreEqual("", HTTPHelper.FindHeaderValueOrDefault(response, "key4|key5"));
        }

        [Test]
        public void TestFindHeaderValueOrDefault_Response_NoKey()
        {
            var response = new HTTPResponseMock();

            Assert.AreEqual("", HTTPHelper.FindHeaderValueOrDefault(response, null));
            Assert.AreEqual("", HTTPHelper.FindHeaderValueOrDefault(response, ""));
        }

        [Test]
        public void TestFindHeaderValueOrDefault_Request_OneKey()
        {
            var request = GetMockRequestMessage();

            Assert.AreEqual("value2", HTTPHelper.FindHeaderValueOrDefault(request, xPath, new List<string>() { "key2" }));
            Assert.AreEqual("", HTTPHelper.FindHeaderValueOrDefault(request, xPath, new List<string>() { "key4" }));
        }

        [Test]
        public void TestFindHeaderValueOrDefault_Request_ManyKeys()
        {
            var request = GetMockRequestMessage();

            Assert.AreEqual("value2", HTTPHelper.FindHeaderValueOrDefault(request, xPath, new List<string>() { "key2", "key3" }));
            Assert.AreEqual("value3", HTTPHelper.FindHeaderValueOrDefault(request, xPath, new List<string>() { "key4", "key3" }));
            Assert.AreEqual("value1", HTTPHelper.FindHeaderValueOrDefault(request, xPath, new List<string>() { "key4", "key1", "key2" }));
            Assert.AreEqual("", HTTPHelper.FindHeaderValueOrDefault(request, xPath, new List<string>() { "key4", "key5" }));
        }

        [Test]
        public void TestFindHeaderValueOrDefault_Request_NoKey()
        {
            var request = GetMockRequestMessage();

            Assert.AreEqual("", HTTPHelper.FindHeaderValueOrDefault(request, xPath, new List<string>()));
            Assert.AreEqual("", HTTPHelper.FindHeaderValueOrDefault(request, xPath, new List<string>() { "" }));
        }

        [Test]
        public void TestGetMessageHeaders()
        {
            var response = new HTTPResponseMock();

            Assert.AreEqual(@"
key1 - value1
key2 - value2
key3 - value3
", HTTPHelper.GetMessageHeaders(response));
        }

		[Test]
		public void TestGetMessageHeadersNullHeaders()
		{
			var response = new HttpResponse();

			Assert.AreEqual("", HTTPHelper.GetMessageHeaders(response));
		}

		#region Implementation
		private XDocument GetMockRequestMessage()
        {
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<notifications topic=""CAWX"" count=""1"">
	<notification id=""5341"">
		<queuedDateTime>2018-09-27T11:06:27.743+01:00</queuedDateTime>
		<body>PHJlc3BvbnNlPjxjb2RlPjIwMjwvY29kZT48bWVzc2FnZT5EZWNsYXJhdGlvbiBzdWJtaXNzaW9uIHNlbnQgdG8gQ0RTPC9tZXNzYWdlPjx4TUNQSWQ+Q0FXLTE1MzgwNDI3ODQxOTg8L3hNQ1BJZD48L3Jlc3BvbnNlPg==</body>
		<headers>
			<header name=""key1"" value=""value1""/>
			<header name=""key2"" value=""value2""/>
			<header name=""key3"" value=""value3""/>
      <header name=""X-Notification-Type"" value=""API""/>
		</headers>
	</notification>
</notifications>";
            return XDocument.Parse(xml);
        }
        #endregion Implementation
    }

    #region Mocks
    public class HTTPResponseMock: HttpResponse
    {
        public HTTPResponseMock()
        {
            HttpHeader[] headers = {
                new HttpHeader() { Key = "key1", Value = "value1" },
                new HttpHeader() { Key = "key2", Value = "value2" },
                new HttpHeader() { Key = "key3", Value = "value3" }
            };
            Headers = headers;
        }
    }
    #endregion Mocks
}
