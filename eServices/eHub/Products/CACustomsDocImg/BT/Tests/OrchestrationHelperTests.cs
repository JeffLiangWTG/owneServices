using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Products.CACustoms.DocImg.BT.Helpers;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.CACustoms.DocImg.BT.Tests
{
	[TestClass]
	public class OrchestrationHelperTests
	{
		const string filePath = ".TestFiles.";

		[TestMethod]
		public void TestOrchestrationHelper_GetIncludeId()
		{
			var newIncludedId = OrchestrationHelper.GetIncludeId();
			Assert.IsTrue(!string.IsNullOrEmpty(newIncludedId));
			Assert.IsTrue(newIncludedId.IndexOf("@") != -1);
			Assert.IsTrue(newIncludedId.StartsWith("-"));
		}

		[TestMethod]
		public void TestOrchestrationHelper_CreateLPCOSendMessage()
		{
			TestOrchestrationHelper_CreateLPCOSendMessage("-4202480791910392092.1510273062817@SYDCO-WWIL-1-BT", "----_DD0A738C-84FA-4853-8620-C8F2C3C4753C_", "-595518194224028533.1510272815044@SYDCO-WWIL-1-BT", "-2632253731244322668.1510272815044@SYDCO-WWIL-1-BT", "Test1_input.xml", "Test1_output.xml", "Test1_LpcoOutput.txt");
		}

		void TestOrchestrationHelper_CreateLPCOSendMessage(string messageId, string boundary, string startId, string attachmentId, string inputTestFile, string outputTestFile, string outputTestMimeFile)
		{
			var inputXml = GetResourceContent(inputTestFile);
			var doc = XDocument.Parse(inputXml);
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("s0", "http://www.cargowise.com/Schemas/Universal/2011/11");
			var imageElement = doc.XPathSelectElement("s0:UniversalEvent/s0:Event/s0:AttachedDocumentCollection/s0:AttachedDocument/s0:ImageData", namespaceManager);
			Assert.IsNotNull(imageElement);
			Assert.IsTrue(!string.IsNullOrEmpty(imageElement.Value));

			var lpcoMessage = GetResourceContent(outputTestFile);
			var lpcoDoc = XDocument.Parse(lpcoMessage);
			var lpcoNamespaceManager = new XmlNamespaceManager(new NameTable());
			lpcoNamespaceManager.AddNamespace("xop", "http://www.w3.org/2004/08/xop/include");
			var includeElement = lpcoDoc.XPathSelectElement("/LPCOImages/Image/xop:Include", lpcoNamespaceManager);
			includeElement.Attribute("href").Value = string.Format("cid:{0}", attachmentId);
			using (var writer = new Utf8StringWriter())
			{
				lpcoDoc.Save(writer);
				lpcoMessage = writer.ToString();
			}

			var attachmentContent = Convert.FromBase64String(imageElement.Value);
			Assert.IsTrue(attachmentContent.Length > 0);

			var requestMessage = (MemoryStream)OrchestrationHelper.CreateLPCOSendMessage(lpcoMessage, boundary, messageId, startId, imageElement.Value, attachmentId, "TESTSENDER_01", "CACustomsDocTest", Guid.NewGuid().ToString(), new NoOpLogger());
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
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestOrchestrationHelper_GetHttpErrorResponse_HTTPError()
		{
			var xmlContent = @"<?xml version='1.0' encoding='UTF-8'?>
<ns0:NACK xmlns:ns0='http://schema.microsoft.com/BizTalk/2003/NACKMessage.xsd' Type='NACK'>
    <NAckID>{9FB32E97-40C1-4A4D-AF2A-1AEC2B16741E}</NAckID>
    <ErrorCode>0xc0c0167a</ErrorCode>
    <ErrorCategory>0</ErrorCategory>
    <ErrorDescription>The remote server returned an error: (400) Bad Request.</ErrorDescription>
    <ErrorDetail>
        <HttpErrorDetail xmlns='http://schema.microsoft.com/BizTalk/2004/HttpErrorDetails.xsd'>
        <Headers>Date: Mon, 11 Dec 2017 07:44:34 GMT\r\nSet-Cookie: SMCHALLENGE=SSL_CHALLENGE_DONE; path=/; domain=.cbsa-asfc.gc.ca; secure,SMSESSION=BJbpsifnxRU3MwzF45wjZ3+cblZz8ubpu0qqlLMC0SXsVUkLAV5+sPGY6QgEyTF31yBKWPQBUNkM/neDUBESKdd3XUj91ppWpTzZTsjzHBeBR5v3sm4T5Ea7CCwQNBcBMgUNonsrtXWKKFDNVQhb75QCkxSmKVEAgSJsz/frQ7XZn/HrlG97pDn0YcjHsan7m0P2q2gAfBwHTG7hsFP8xHnp8h3OBYe3jpAXJySWmM7LMB9sn92hyXqnxWUPycb7mLZetSeNs0adzpqQS4Ha8dcC5HLrf9shH0yGWdsa8usmg5lV2FrkF030Rm1s/+MpJUAr8sR+OidmPfO0eD+RpAqDjg7hLWnrMnc72fpV4MCWKvh7YGmdkWLVP+wDWgcqG6oQvbHBDIjtN1yUmSmp3SdTyw0fKPMCXlNT+gbyZg4EyYm90lp4RGi81zEQJl6Q+M5GPW3YldV8gaG04NApAshkIYZm8V/hwXITBLPw5fuZNtJJX+wnIsgQA3o6BKTpGUffeKBezbplHRZY/8bIOeHsG9wtXusXLWXX+I6BefPbzcHFeBSwEwWxmezAjAPEkY6dDSIm4Jy5KLXI172ZyPd5XrN3pXeetcyW0TrpRtKBTaMnb0SfvvtYdNyJukM+3/zhQUqLsz1Cw8VtjjdsMA4yoaCBX6d5Qz3poOeKheQyqOJJ3Qx6XyHAKc0q/WTWlaa01zcuJYbSBKnBgqh+C91F0yGsraFNdjihXBDiFhp2zwC5cRMw8Y4MCjKZ0EpCrTWPPneS38C4vnLWKoKbpczXrZm0Hz0/N1rGZeZFeTAdB8JNeo8BhfRf0A+FyakFuSk1v5KvSygZodiaqT7fX3SAenbFa6h2q1Yypnldc5G1fEYA6lMCIe9Tk3jWYKJE/QDRtvVHM8fZ9YZ2qLLBw7nvvcvcDVobOXAdLWDZFOkvx9yGM9w3kYcr31PT9tbDCTbAIvmhsqSH8695pE9bpgY1YnF+NCEP2wVKkct1/uKjtEe/FaS/nETxF1eRmjaTHtp715d/2idZ+TMejQ4B40NYgXwAQyGiUgaZM4sVrRR0/dpRGD2Q14Wn8rp7PJsmSn0lTVleHylhRJoX013q5sD0pr/eXRlQWyW2pQNoarDHSBX4klORP+YzzOZhRpkCM93qZEVjDE818rs9DrU8gNIwnQts8kTevReTalgQFfmOmhX1J80N44AZQDg7528+; path=/; domain=.cbsa-asfc.gc.ca; secure,SMCHALLENGE=; expires=Wed, 14 Jun 2017 07:44:34 GMT; path=/; domain=.cbsa-asfc.gc.ca; secure\r\nServer: Apache/2.2.34 (Unix) mod_ssl/2.2.34 OpenSSL/1.0.2k\r\nX-Powered-By: Servlet/3.0\r\nConnection: close\r\nTransfer-Encoding: chunked\r\nContent-Type: text/xml\r\nContent-Language: en-US\r\n</Headers>
        <Body>&lt;b2b&gt;&lt;application&gt;swi.tcp.v1.lpco&lt;/application&gt;&lt;response&gt;&lt;uuid&gt;71f842fc-79f0-4fb0-b3ca-20551330bae0&lt;/uuid&gt;&lt;sentDateTime&gt;2017-12-11T02:44:38.499-05:00&lt;/sentDateTime&gt;&lt;error_code&gt;400&lt;/error_code&gt;&lt;error_text&gt;&lt;![CDATA[Failed content validation. Please refer to interface document for more info. [cvc-datatype-valid.1.2.1: '' is not a valid value for 'date'.]]]&gt;&lt;/error_text&gt;&lt;/response&gt;&lt;/b2b&gt;</Body>
        </HttpErrorDetail>
    </ErrorDetail>
</ns0:NACK>";

			var doc = new XmlDocument();
			doc.LoadXml(xmlContent);
			var soapException = new SoapException(string.Empty, XmlQualifiedName.Empty, "", doc.DocumentElement);
			var actual = OrchestrationHelper.GetHttpErrorResponse(soapException);
			Assert.AreEqual("400", actual.Code);
			Assert.AreEqual("Failed content validation. Please refer to interface document for more info. [cvc-datatype-valid.1.2.1: '' is not a valid value for 'date'.]", actual.Description);
		}

		[TestMethod]
		public void TestOrchestrationHelper_GetHttpErrorResponse_ConnectionFailedWithinSoapException()
		{
			var messagePrefix = @"An error occurred while processing the message, refer to the details section for more information 
Message ID: {50B422B8-F48E-46C2-B8FA-DD83C9DA0A79}
Instance ID: {B6BF4E4E-A0B5-40D0-838D-591A08BD1CEE}
Error Description: ";

			var message1 = @"System.ServiceModel.CommunicationException: An error occurred while making the HTTP request to https://apps.cbsa-asfc.gc.ca/b2bwssync/swi/tcp/v1/lpco. This could be due to the fact that the server certificate is not configured properly with HTTP.SYS in the HTTPS case. This could also be caused by a mismatch of the security binding between the client and the server.";
			var message2 = @"System.ServiceModel.CommunicationException: An error occurred while receiving the HTTP response to https://apps.cbsa-asfc.gc.ca/b2bwssync/swi/tcp/v1/lpco. This could be due to the service endpoint binding not using the HTTP protocol. This could also be due to an HTTP request context being aborted by the server (possibly due to the service shutting down). See server logs for more details.";
			var message3 = @"System.ServiceModel.CommunicationException: The underlying connection was closed: A connection that was expected to be kept alive was closed by the server.";
			var message4 = @"System.ServiceModel.Security.SecurityNegotiationException: Could not establish secure channel for SSL/TLS with authority 'apps-to1.cbsa-asfc.gc.ca'. ---> System.Net.WebException: The request was aborted: Could not create SSL/TLS secure channel.";

			var soapException1 = new SoapException(messagePrefix + message1, XmlQualifiedName.Empty, "", CreateSoapDetail(message1));
			var soapException2 = new SoapException(messagePrefix + message2, XmlQualifiedName.Empty, "", CreateSoapDetail(message2));
			var soapException3 = new SoapException(messagePrefix + message3, XmlQualifiedName.Empty, "", CreateSoapDetail(message3));
			var soapException4 = new SoapException(messagePrefix + message4, XmlQualifiedName.Empty, "", CreateSoapDetail(message4));

			var response1 = OrchestrationHelper.GetHttpErrorResponse(soapException1);
			var response2 = OrchestrationHelper.GetHttpErrorResponse(soapException2);
			var response3 = OrchestrationHelper.GetHttpErrorResponse(soapException3);
			var response4 = OrchestrationHelper.GetHttpErrorResponse(soapException4);

			Assert.AreEqual("", response1.Code);
			Assert.AreEqual("", response2.Code);
			Assert.AreEqual("", response3.Code);
			Assert.AreEqual("", response4.Code);
			Assert.AreEqual(message1, response1.Description);
			Assert.AreEqual(message2, response2.Description);
			Assert.AreEqual(message3, response3.Description);
			Assert.AreEqual(message4, response4.Description);
			Assert.IsTrue(response1.ShouldRetry);
			Assert.IsTrue(response2.ShouldRetry);
			Assert.IsTrue(response3.ShouldRetry);
			Assert.IsTrue(response4.ShouldRetry);
		}

		XmlNode CreateSoapDetail(string errorDescription)
		{
			var detail = $@"
<ns0:NACK Type=""NACK"" xmlns:ns0=""http://schema.microsoft.com/BizTalk/2003/NACKMessage.xsd"">
    <NAckID>{{50B422B8-F48E-46C2-B8FA-DD83C9DA0A79}}</NAckID>
    <ErrorCode>0xc0c0167a</ErrorCode>
    <ErrorCategory>0</ErrorCategory>
    <ErrorDescription>{ errorDescription }</ErrorDescription>
</ns0:NACK>";

			var doc = new XmlDocument();
			doc.LoadXml(detail);

			return doc.DocumentElement;
		}

		[TestMethod]
		public void TestOrchestrationHelper_GetHttpErrorResponse_InternalError()
		{
			var errorMessage = @"An error occurred while processing the message, refer to the details section for more information 
Message ID: {A39D693D-DA57-4762-B1B2-5D2417FB3C9E}
Instance ID: {05B8C4DA-2301-4079-A56B-034F94990853}
Error Description: The client certificate is not found in the certificate store
Parameter name: Certificate
";
			var soapException = new SoapException(errorMessage, XmlQualifiedName.Empty);
			try
			{
				OrchestrationHelper.GetHttpErrorResponse(soapException);
				Assert.Fail("Should throw InvalidOperationException");
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual(errorMessage, ex.Message);
			}
		}

		[TestMethod]
		public void TestOrchestrationHelper_GetHttpErrorResponse_UnknownError()
		{
			var errorMessage = @"An error occurred while processing the message, UNKNOWN ERROR...";
			var soapException = new SoapException(errorMessage, XmlQualifiedName.Empty);
			try
			{
				var httpResponse = OrchestrationHelper.HandleExceptions(soapException, new NoOpLogger());
				Assert.Fail("Should throw InvalidOperationException");
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("An error occurred while processing the message, UNKNOWN ERROR...", ex.Message);
			}
		}

		[TestMethod]
		public void TestOrchestrationHelper_GetHttpErrorResponse_UnexpectedResponse()
		{
			var xmlContent = @"<?xml version='1.0' encoding='UTF-8'?>
<ns0:NACK xmlns:ns0='http://schema.microsoft.com/BizTalk/2003/NACKMessage.xsd' Type='NACK'>
    <NAckID>{9FB32E97-40C1-4A4D-AF2A-1AEC2B16741E}</NAckID>
    <ErrorCode>0xc0c0167a</ErrorCode>
    <ErrorCategory>0</ErrorCategory>
    <ErrorDescription> The remote server returned an unexpected response: (400) Bad Request.</ErrorDescription>
</ns0:NACK>";
			var errorMessage = @"An error occurred while processing the message, refer to the details section for more information 
Message ID: {9FB32E97-40C1-4A4D-AF2A-1AEC2B16741E}
Instance ID: {4D9D266A-2EAC-4FA3-8AD5-245FBEEAD911}
Error Description: The remote server returned an unexpected response: (400) Bad Request.
";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlContent);
			var soapException = new SoapException(errorMessage, XmlQualifiedName.Empty, "", doc.DocumentElement);

			var httpResponse = OrchestrationHelper.HandleExceptions(soapException, new NoOpLogger());
			var actual = OrchestrationHelper.GetB2bResponse(soapException, httpResponse);
			Assert.AreEqual(@"<b2b>
	<application>swi.tcp.v1.lpco</application>
	<response>
		<error_code>400</error_code>
		<error_text>
			<![CDATA[Bad Request.]]>
		</error_text>
	</response>
</b2b>", actual);
			Assert.IsFalse(httpResponse.ShouldRetry);
		}

		[TestMethod]
		public void TestOrchestrationHelper_GetHttpErrorResponse_CommunicationException()
		{
			var xmlContent = @"<?xml version='1.0' encoding='UTF-8'?>
<ns0:NACK xmlns:ns0='http://schema.microsoft.com/BizTalk/2003/NACKMessage.xsd' Type='NACK'>
    <NAckID>{9FB32E97-40C1-4A4D-AF2A-1AEC2B16741E}</NAckID>
    <ErrorCode>0xc0c0167a</ErrorCode>
    <ErrorCategory>0</ErrorCategory>
    <ErrorDescription>  System.ServiceModel.CommunicationException: An error occurred while receiving the HTTP response to https://apps.cbsa-asfc.gc.ca/b2bwssync/swi/tcp/v1/lpco. This could be due to the service endpoint binding not using the HTTP protocol. This could also be due to an HTTP request context being aborted by the server (possibly due to the service shutting down). See server logs for more details. ---&gt; System.Net.WebException: The underlying connection was closed: An unexpected error occurred on a receive. ---&gt; System.IO.IOException: The decryption operation failed, see inner exception. ---&gt; System.ComponentModel.Win32Exception: The message or signature supplied for verification has been altered</ErrorDescription>
</ns0:NACK>";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlContent);
			var communicationException = new CommunicationException("System.ServiceModel.CommunicationException: An error occurred while receiving the HTTP response to https://apps.cbsa-asfc.gc.ca/b2bwssync/swi/tcp/v1/lpco. This could be due to the service endpoint binding not using the HTTP protocol. This could also be due to an HTTP request context being aborted by the server (possibly due to the service shutting down). See server logs for more details. ---&gt; System.Net.WebException: The underlying connection was closed: An unexpected error occurred on a receive. ---&gt; System.IO.IOException: The decryption operation failed, see inner exception. ---&gt; System.ComponentModel.Win32Exception: The message or signature supplied for verification has been altered");

			var httpResponse = OrchestrationHelper.HandleExceptions(communicationException, new NoOpLogger());
			var actual = OrchestrationHelper.GetB2bResponse(communicationException, httpResponse);
			Assert.AreEqual(@"<b2b>
	<application>swi.tcp.v1.lpco</application>
	<response>
		<error_code></error_code>
		<error_text>
			<![CDATA[System.ServiceModel.CommunicationException: An error occurred while receiving the HTTP response to https://apps.cbsa-asfc.gc.ca/b2bwssync/swi/tcp/v1/lpco. This could be due to the service endpoint binding not using the HTTP protocol. This could also be due to an HTTP request context being aborted by the server (possibly due to the service shutting down). See server logs for more details. ---&gt; System.Net.WebException: The underlying connection was closed: An unexpected error occurred on a receive. ---&gt; System.IO.IOException: The decryption operation failed, see inner exception. ---&gt; System.ComponentModel.Win32Exception: The message or signature supplied for verification has been altered]]>
		</error_text>
	</response>
</b2b>", actual);
			Assert.IsTrue(httpResponse.ShouldRetry);
		}

		[TestMethod]
		public void TestOrchestrationHelper_GetB2bResponse_InvalidStreamBody()
		{
			var xmlContent = @"<?xml version='1.0' encoding='UTF-8'?>
<ns0:NACK xmlns:ns0='http://schema.microsoft.com/BizTalk/2003/NACKMessage.xsd' Type='NACK'>
    <NAckID>{9FB32E97-40C1-4A4D-AF2A-1AEC2B16741E}</NAckID>
    <ErrorCode>0xc0c0167a</ErrorCode>
    <ErrorCategory>0</ErrorCategory>
    <ErrorDescription>The remote server returned an error: (500) Internal Error.</ErrorDescription>
</ns0:NACK>";
			var errorMessage = @"An error occurred while processing the message, refer to the details section for more information 
Message ID: {9FB32E97-40C1-4A4D-AF2A-1AEC2B16741E}
Instance ID: {4D9D266A-2EAC-4FA3-8AD5-245FBEEAD911}
Error Description: The remote server returned an error: (500) Internal Error.
";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlContent);
			var soapException = new SoapException(errorMessage, XmlQualifiedName.Empty, "", doc.DocumentElement);
			var httpResponse = OrchestrationHelper.GetHttpErrorResponse(soapException);
			var actual = OrchestrationHelper.GetB2bResponse(soapException, httpResponse);
			Assert.AreEqual(@"<b2b>
	<application>swi.tcp.v1.lpco</application>
	<response>
		<error_code>500</error_code>
		<error_text>
			<![CDATA[Internal Error.]]>
		</error_text>
	</response>
</b2b>", actual);
		}

		[TestMethod]
		public void TestOrchestrationHelper_GetB2bResponse_ValidStreamBody()
		{
			var xmlContent = @"<?xml version='1.0' encoding='UTF-8'?>
<ns0:NACK xmlns:ns0='http://schema.microsoft.com/BizTalk/2003/NACKMessage.xsd' Type='NACK'>
    <NAckID>{9FB32E97-40C1-4A4D-AF2A-1AEC2B16741E}</NAckID>
    <ErrorCode>0xc0c0167a</ErrorCode>
    <ErrorCategory>0</ErrorCategory>
    <ErrorDescription>The remote server returned an error: (400) Bad Request.</ErrorDescription>
    <ErrorDetail>
        <HttpErrorDetail xmlns='http://schema.microsoft.com/BizTalk/2004/HttpErrorDetails.xsd'>
        <Headers>Date: Mon, 11 Dec 2017 07:44:34 GMT\r\nSet-Cookie: SMCHALLENGE=SSL_CHALLENGE_DONE; path=/; domain=.cbsa-asfc.gc.ca; secure,SMSESSION=BJbpsifnxRU3MwzF45wjZ3+cblZz8ubpu0qqlLMC0SXsVUkLAV5+sPGY6QgEyTF31yBKWPQBUNkM/neDUBESKdd3XUj91ppWpTzZTsjzHBeBR5v3sm4T5Ea7CCwQNBcBMgUNonsrtXWKKFDNVQhb75QCkxSmKVEAgSJsz/frQ7XZn/HrlG97pDn0YcjHsan7m0P2q2gAfBwHTG7hsFP8xHnp8h3OBYe3jpAXJySWmM7LMB9sn92hyXqnxWUPycb7mLZetSeNs0adzpqQS4Ha8dcC5HLrf9shH0yGWdsa8usmg5lV2FrkF030Rm1s/+MpJUAr8sR+OidmPfO0eD+RpAqDjg7hLWnrMnc72fpV4MCWKvh7YGmdkWLVP+wDWgcqG6oQvbHBDIjtN1yUmSmp3SdTyw0fKPMCXlNT+gbyZg4EyYm90lp4RGi81zEQJl6Q+M5GPW3YldV8gaG04NApAshkIYZm8V/hwXITBLPw5fuZNtJJX+wnIsgQA3o6BKTpGUffeKBezbplHRZY/8bIOeHsG9wtXusXLWXX+I6BefPbzcHFeBSwEwWxmezAjAPEkY6dDSIm4Jy5KLXI172ZyPd5XrN3pXeetcyW0TrpRtKBTaMnb0SfvvtYdNyJukM+3/zhQUqLsz1Cw8VtjjdsMA4yoaCBX6d5Qz3poOeKheQyqOJJ3Qx6XyHAKc0q/WTWlaa01zcuJYbSBKnBgqh+C91F0yGsraFNdjihXBDiFhp2zwC5cRMw8Y4MCjKZ0EpCrTWPPneS38C4vnLWKoKbpczXrZm0Hz0/N1rGZeZFeTAdB8JNeo8BhfRf0A+FyakFuSk1v5KvSygZodiaqT7fX3SAenbFa6h2q1Yypnldc5G1fEYA6lMCIe9Tk3jWYKJE/QDRtvVHM8fZ9YZ2qLLBw7nvvcvcDVobOXAdLWDZFOkvx9yGM9w3kYcr31PT9tbDCTbAIvmhsqSH8695pE9bpgY1YnF+NCEP2wVKkct1/uKjtEe/FaS/nETxF1eRmjaTHtp715d/2idZ+TMejQ4B40NYgXwAQyGiUgaZM4sVrRR0/dpRGD2Q14Wn8rp7PJsmSn0lTVleHylhRJoX013q5sD0pr/eXRlQWyW2pQNoarDHSBX4klORP+YzzOZhRpkCM93qZEVjDE818rs9DrU8gNIwnQts8kTevReTalgQFfmOmhX1J80N44AZQDg7528+; path=/; domain=.cbsa-asfc.gc.ca; secure,SMCHALLENGE=; expires=Wed, 14 Jun 2017 07:44:34 GMT; path=/; domain=.cbsa-asfc.gc.ca; secure\r\nServer: Apache/2.2.34 (Unix) mod_ssl/2.2.34 OpenSSL/1.0.2k\r\nX-Powered-By: Servlet/3.0\r\nConnection: close\r\nTransfer-Encoding: chunked\r\nContent-Type: text/xml\r\nContent-Language: en-US\r\n</Headers>
        <Body>&lt;b2b&gt;&lt;application&gt;swi.tcp.v1.lpco&lt;/application&gt;&lt;response&gt;&lt;uuid&gt;71f842fc-79f0-4fb0-b3ca-20551330bae0&lt;/uuid&gt;&lt;sentDateTime&gt;2017-12-11T02:44:38.499-05:00&lt;/sentDateTime&gt;&lt;error_code&gt;400&lt;/error_code&gt;&lt;error_text&gt;&lt;![CDATA[Failed content validation. Please refer to interface document for more info. [cvc-datatype-valid.1.2.1: '' is not a valid value for 'date'.]]]&gt;&lt;/error_text&gt;&lt;/response&gt;&lt;/b2b&gt;</Body>
        </HttpErrorDetail>
    </ErrorDetail>
</ns0:NACK>";
			var errorMessage = @"An error occurred while processing the message, refer to the details section for more information 
Message ID: {9FB32E97-40C1-4A4D-AF2A-1AEC2B16741E}
Instance ID: {4D9D266A-2EAC-4FA3-8AD5-245FBEEAD911}
Error Description: The remote server returned an error: (400) Bad Request.
";
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xmlContent);
			var soapException = new SoapException(errorMessage, XmlQualifiedName.Empty, "", doc.DocumentElement);
			var httpResponse = OrchestrationHelper.GetHttpErrorResponse(soapException);
			var actual = OrchestrationHelper.GetB2bResponse(soapException, httpResponse);
			Assert.AreEqual(@"<b2b><application>swi.tcp.v1.lpco</application><response><uuid>71f842fc-79f0-4fb0-b3ca-20551330bae0</uuid><sentDateTime>2017-12-11T02:44:38.499-05:00</sentDateTime><error_code>400</error_code><error_text><![CDATA[Failed content validation. Please refer to interface document for more info. [cvc-datatype-valid.1.2.1: '' is not a valid value for 'date'.]]]></error_text></response></b2b>", actual);
		}

		[TestMethod]
		public void TestShouldRetryWhenNoEndpoint()
		{
			var soapExceptionInvalidMessage = new SoapException("This is an invalid error message.", XmlQualifiedName.Empty);
			var soapExceptionNoEndPointFormat1 = new SoapException("There was no endpoint listening at https://apps.cbsa-asfc.gc.ca/b2bwssync/swi/tcp/v1/lpco that could accept the message. This is often caused by an incorrect address or SOAP action", XmlQualifiedName.Empty);
			var soapExceptionNoEndPointInnerWebException = new SoapException("", XmlQualifiedName.Empty, new System.Net.WebException(@"There was no endpoint listening at https://apps.cbsa-asfc.gc.ca/b2bwssync/swi/tcp/v1/lpco that could accept the message. This is often caused by an incorrect address or SOAP action"));
			var soapExceptionNoEndPointFormat2 = new SoapException(
@"An error occurred while processing the message, refer to the details section for more information 
Message ID: {9FB32E97-40C1-4A4D-AF2A-1AEC2B16741E}
Instance ID: {4D9D266A-2EAC-4FA3-8AD5-245FBEEAD911}
Error Description: There was no endpoint listening at https://apps.cbsa-asfc.gc.ca/b2bwssync/swi/tcp/v1/lpco that could accept the message. This is often caused by an incorrect address or SOAP action
", XmlQualifiedName.Empty);
			HttpResponse response;
			try
			{
				response = OrchestrationHelper.GetHttpErrorResponse(soapExceptionInvalidMessage);
				Assert.Fail("Should throw InvalidOperationException");
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("This is an invalid error message.", ex.Message);
			}
			response = OrchestrationHelper.GetHttpErrorResponse(soapExceptionNoEndPointFormat1);
			Assert.IsTrue(response.ShouldRetry);
			response = OrchestrationHelper.GetHttpErrorResponse(soapExceptionNoEndPointFormat2);
			Assert.IsTrue(response.ShouldRetry);
			response = OrchestrationHelper.GetHttpErrorResponse(soapExceptionNoEndPointInnerWebException);
			Assert.IsTrue(response.ShouldRetry);
		}

		[TestMethod]
		public void TestShouldRetryWhenInternalServerError()
		{
			var errorMessage = "(500) Internal Server Error";
			var extendedErrorMessagePrefix = @"An error occurred while processing the message, refer to the details section for more information
Message ID: {F51F9BDC-37A6-4192-8F53-52B2D5759D8A}
Instance ID: {62BEA2E8-1A71-43B9-BFCE-171D1BCABA9D}
Error Description: ";

			var soapExceptionInvalidMessage = new SoapException("This is an invalid error message.", XmlQualifiedName.Empty);
			var soapExceptionInternalErrorFormat1 = new SoapException(errorMessage, XmlQualifiedName.Empty);
			var soapExceptionInternalErrorInnerWebException = new SoapException("", XmlQualifiedName.Empty, new System.Net.WebException(errorMessage));
			var soapExceptionInternalErrorFormat2 = new SoapException(extendedErrorMessagePrefix + errorMessage, XmlQualifiedName.Empty);
			var soapExceptionInternalErrorFormat3 = new SoapException(extendedErrorMessagePrefix + errorMessage, XmlQualifiedName.Empty, "", CreateSoapDetail(errorMessage));
			HttpResponse response;
			try
			{
				response = OrchestrationHelper.GetHttpErrorResponse(soapExceptionInvalidMessage);
				Assert.Fail("Should throw InvalidOperationException");
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("This is an invalid error message.", ex.Message);
			}
			response = OrchestrationHelper.GetHttpErrorResponse(soapExceptionInternalErrorFormat1);
			Assert.IsTrue(response.ShouldRetry);
			response = OrchestrationHelper.GetHttpErrorResponse(soapExceptionInternalErrorFormat2);
			Assert.IsTrue(response.ShouldRetry);
			response = OrchestrationHelper.GetHttpErrorResponse(soapExceptionInternalErrorInnerWebException);
			Assert.IsTrue(response.ShouldRetry);
			response = OrchestrationHelper.GetHttpErrorResponse(soapExceptionInternalErrorFormat3);
			Assert.IsTrue(response.ShouldRetry);
		}

		[TestMethod]
		public void TestOrchestrationHelper_GetHttpErrorResponse_OtherException()
		{
			var anotherException = new Exception("Other exception type");
			try
			{
				var httpResponse = OrchestrationHelper.HandleExceptions(anotherException, new NoOpLogger());
				Assert.Fail("Should throw InvalidOperationException");
			}
			catch (InvalidOperationException ex)
			{
				Assert.AreEqual("Error during sending.", ex.Message);
			}
		}

		string GetResourceContent(string resourceName)
		{
			using (var reader = new StreamReader(GetResourceStream(resourceName)))
			{
				return reader.ReadToEnd();
			}
		}

		[TestMethod]
		public void TestGetB2BResponseFromDescription()
		{
			var responseDescription = @"<detail><ns0:NACK Type='NACK' xmlns:ns0='http://schema.microsoft.com/BizTalk/2003/NACKMessage.xsd'><NAckID>{978D245E-69C4-49E9-9C9C-A0B59952EC34}</NAckID><ErrorCode>0xc0c0167a</ErrorCode><ErrorCategory>0</ErrorCategory><ErrorDescription>System.Net.WebException: The remote server returned an unexpected response: (400) Bad Request.&lt;b2b&gt;&lt;application&gt;swi.tcp.v1.lpco&lt;/application&gt;&lt;response&gt;&lt;uuid&gt;818df6cd-8e8a-4b14-8175-16341e48c9c0&lt;/uuid&gt;&lt;sentDateTime&gt;2021-01-07T22:17:31.783-05:00&lt;/sentDateTime&gt;&lt;error_code&gt;400&lt;/error_code&gt;&lt;error_text&gt;&lt;![CDATA[Failed content validation. Please refer to interface document for more info. [cvc-complex-type.2.4.a: Invalid content was found starting with element 'MessageType'. One of '{B2BTrackingInfo}' is expected.]]]&gt;&lt;/error_text&gt;&lt;/response&gt;&lt;/b2b&gt;</ErrorDescription></ns0:NACK></detail>";
			var xmlString = OrchestrationHelper.GetB2BResponseFromDescription(responseDescription);
			Assert.AreNotEqual(xmlString, null);
			try
			{
				var response = new XmlDocument();
				response.LoadXml(xmlString);
				Assert.AreEqual(response.DocumentElement.Name, "b2b");
				Assert.AreEqual(response.FirstChild.ChildNodes.Count, 2);
				Assert.AreEqual(response.FirstChild.ChildNodes.Item(0).Name, "application");
				Assert.AreEqual(response.FirstChild.ChildNodes.Item(1).Name, "response");
			}
			catch (Exception ex)
			{
				Assert.Fail("xmlString is invalid " + ex.Message);
			}
		}

		Stream GetResourceStream(string resourceName)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Concat(this.GetType().Namespace, filePath, resourceName));
		}

		private class Utf8StringWriter : StringWriter
		{
			public override Encoding Encoding { get { return Encoding.UTF8; } }
		}
	}
}
