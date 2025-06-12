using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.WSHttpEx;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.BizTalkAdapters.Tests
{
	[TestClass]
	public class WsHttpExTransferrerTest
	{
		string requestFilePath;
		DummyWebRequest request;
		DummyWebResponse response;
		IWebRequestFactory requestFactory;
		IBaseMessageFactory messageFactory;
		IBaseMessage replyMessage;
		IBaseMessageContext messageContext;
		IBaseMessagePart replyMessagePart;
		const string soapResponseMessage = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Header><ActivityId CorrelationId=""0311cc8d-7fce-47ef-bef1-0b7fd84c4a6d"" xmlns=""http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics"">d1c0853f-a668-4882-91c0-eca1c99b6a11</ActivityId></s:Header><s:Body><ResponseElement><ResponsePart/></ResponseElement></s:Body></s:Envelope>";
		const string soapResponseHeaders = @"<Headers><ActivityId CorrelationId=""0311cc8d-7fce-47ef-bef1-0b7fd84c4a6d"" xmlns=""http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics"">d1c0853f-a668-4882-91c0-eca1c99b6a11</ActivityId></Headers>";
		const string mtomResponseHeaders = @"multipart/related; boundary=""MIMEBoundaryurn_uuid_F2F27D7F0201D7CA621505867573546""; type=""application/xop+xml""; start=""<0.urn:uuid:F2F27D7F0201D7CA621505867573547@apache.org>""; start-info=""text/xml""";
		const string mtomResponseMessage = @"--MIMEBoundaryurn_uuid_F2F27D7F0201D7CA621505867573546
Content-Type: application/xop+xml; charset=utf-8; type=""text/xml""
Content-Transfer-Encoding: binary
Content-ID: <0.urn:uuid:F2F27D7F0201D7CA621505867573547@apache.org>

<?xml version=""1.0"" encoding=""utf-8""?><soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/""><soapenv:Body><ns2:lsResponse xmlns:ns2=""http://webservices.ftp.telematico.dogana.dogane.ag_dogane.finanze.it""><lsReturn></lsReturn></ns2:lsResponse></soapenv:Body></soapenv:Envelope>
--MIMEBoundaryurn_uuid_F2F27D7F0201D7CA621505867573546--";

		[TestMethod]
		public void WsHttpExTransferrer_OneWaySend()
		{
			var message = WSHttpExPropertiesTest.CreateMessage();
			var transferrer = new WSHttpExTransferrerForTest(message, null, requestFactory);
			requestFactory.Expect(x => x.CreateRequest(message, transferrer.Config)).Return(request);

			Assert.IsNull(transferrer.SendRequest(message));
			ValidateRequest();
		}

		[TestMethod]
		[ExpectedException(typeof(WebException))]
		public void WsHttpExTransferrer_OneWaySendWithNetworkError()
		{
			var message = WSHttpExPropertiesTest.CreateMessage();
			request.ResponseException = new WebException("NetworkError", null, WebExceptionStatus.ProtocolError, new DummyWebResponse());
			var transferrer = new WSHttpExTransferrerForTest(message, null, requestFactory);
			requestFactory.Expect(x => x.CreateRequest(message, transferrer.Config)).Return(request);

			try
			{
				transferrer.SendRequest(message);
			}
			finally
			{
				ValidateRequest();
			}
		}

		[TestMethod]
		[ExpectedException(typeof(FaultException))]
		public void WsHttpExTransferrer_OneWaySendWithSoapFault()
		{
			var message = WSHttpExPropertiesTest.CreateMessage();
			request.ResponseException = new WebException("SoapFault", null, WebExceptionStatus.ProtocolError, response);
			var transferrer = new WSHttpExTransferrerForTest(message, null, requestFactory);
			requestFactory.Expect(x => x.CreateRequest(message, transferrer.Config)).Return(request);
			var soapFaultXML = InitialiseResponseStream(SoapHelpersTest.SoapFaultWithEnvelope);

			try
			{
				transferrer.SendRequest(message);
			}
			catch (FaultException ex)
			{
				ValidateRequest();
				var faultXML = XDocument.Parse(ex.Message);
				Assert.IsTrue(SoapHelpersTest.ElementsAreEqual(faultXML.Root, soapFaultXML.Root, true));
				throw;
			}
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void WsHttpExTransferrer_TwoWaySend()
		{
			var message = WSHttpExPropertiesTest.CreateMessage(null, true);
			var transferrer = new WSHttpExTransferrerForTest(message, messageFactory, requestFactory);
			requestFactory.Expect(x => x.CreateRequest(message, transferrer.Config)).Return(request);
			InitialiseResponseStream(soapResponseMessage);
			InitialiseReplyMessage();

			var responseMessage = transferrer.SendRequest(message);
			ValidateRequest();
			Assert.IsNotNull(responseMessage);
			ValidateResponse(new XElement("ResponseElement", new XElement("ResponsePart")), soapResponseHeaders);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void WsHttpExTransferrer_TwoWaySendWithEntireEnvelopeResponse()
		{
			var message = WSHttpExPropertiesTest.CreateMessage(new Object[] { new XElement("inboundBodyLocation", ((int)InboundBodyLocations.EntireEnvelope).ToString()) }, true);
			var transferrer = new WSHttpExTransferrerForTest(message, messageFactory, requestFactory);
			requestFactory.Expect(x => x.CreateRequest(message, transferrer.Config)).Return(request);
			var responseXML = InitialiseResponseStream(soapResponseMessage);
			InitialiseReplyMessage();

			var responseMessage = transferrer.SendRequest(message);
			ValidateRequest();
			Assert.IsNotNull(responseMessage);
			ValidateResponse(responseXML.Root, soapResponseHeaders);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void WsHttpExTransferrer_TwoWaySendWithBodyPathResponse()
		{
			var configElements = new[] { new XElement("inboundBodyLocation", ((int)InboundBodyLocations.BodyPath).ToString()),
										new XElement("inboundBodyPathExpression", "/ResponseElement/ResponsePart") };
			var message = WSHttpExPropertiesTest.CreateMessage(configElements, true);
			var transferrer = new WSHttpExTransferrerForTest(message, messageFactory, requestFactory);
			requestFactory.Expect(x => x.CreateRequest(message, transferrer.Config)).Return(request);
			InitialiseResponseStream(soapResponseMessage);
			InitialiseReplyMessage();

			var responseMessage = transferrer.SendRequest(message);
			ValidateRequest();
			Assert.IsNotNull(responseMessage);
			ValidateResponse(new XElement("ResponsePart"), soapResponseHeaders);
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void WsHttpExTransferrer_TwoWaySendWithSoapFault()
		{
			var message = WSHttpExPropertiesTest.CreateMessage(null, true);
			request.ResponseException = new WebException("SoapFault", null, WebExceptionStatus.ProtocolError, response);
			var transferrer = new WSHttpExTransferrerForTest(message, messageFactory, requestFactory);
			requestFactory.Expect(x => x.CreateRequest(message, transferrer.Config)).Return(request);
			InitialiseResponseStream(SoapHelpersTest.SoapFaultWithEnvelope);
			InitialiseReplyMessage();

			var responseMessage = transferrer.SendRequest(message);
			ValidateRequest();
			Assert.IsNotNull(responseMessage);
			var soapFaultWithoutEnvelopeXML = XDocument.Parse(SoapHelpersTest.SoapFaultWithoutEnvelope);
			ValidateResponse(soapFaultWithoutEnvelopeXML.Root, @"<Headers />");
		}

		[TestMethod]
		[TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void WsHttpExTransferrer_TwoWaySendWithMtomMessage()
		{
			var message = WSHttpExPropertiesTest.CreateMessage(new[] { new XElement("responseMessageEncoding", "Mtom") }, true);
			var transferrer = new WSHttpExTransferrerForTest(message, messageFactory, requestFactory);
			requestFactory.Expect(x => x.CreateRequest(message, transferrer.Config)).Return(request);
			new MemoryStream(Encoding.UTF8.GetBytes(mtomResponseMessage)).CopyTo(response.GetResponseStream());
			response.GetResponseStream().Position = 0;
			var headers = new WebHeaderCollection();
			headers.Add("Content-Type", mtomResponseHeaders);
			response.headers = headers;
			InitialiseReplyMessage();

			var responseMessage = transferrer.SendRequest(message);
			ValidateRequest();
			Assert.IsNotNull(responseMessage);
		}

		[TestInitialize]
		public void TestInitialize()
		{
			requestFilePath = Path.GetTempFileName();
			response = new DummyWebResponse();
			requestFactory = MockRepository.GenerateStub<IWebRequestFactory>();
			request = new DummyWebRequest(new FileStream(requestFilePath, FileMode.Open), response);
			messageFactory = MockRepository.GenerateStub<IBaseMessageFactory>();
			replyMessage = MockRepository.GenerateStub<IBaseMessage>();
			messageContext = MockRepository.GenerateStub<IBaseMessageContext>();
			replyMessage.Context = messageContext;
			replyMessagePart = MockRepository.GenerateStub<IBaseMessagePart>();
		}

		[TestCleanup]
		public void TestCleanup()
		{
			File.Delete(requestFilePath);
		}

		XDocument InitialiseResponseStream(string data)
		{
			var responseXML = XDocument.Parse(data);
			responseXML.Save(response.GetResponseStream());
			response.GetResponseStream().Position = 0;

			return responseXML;
		}

		void InitialiseReplyMessage()
		{
			messageFactory.Expect(x => x.CreateMessage()).Return(replyMessage);
			messageFactory.Expect(x => x.CreateMessagePart()).Return(replyMessagePart);
		}

		void ValidateRequest()
		{
			using (var requestStream = new FileStream(requestFilePath, FileMode.Open))
			{
				var requestXML = XDocument.Load(requestStream);
				var expectedXML = XDocument.Parse(@"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""><s:Body><RequestElement/></s:Body></s:Envelope>");
				Assert.IsTrue(SoapHelpersTest.ElementsAreEqual(requestXML.Root, expectedXML.Root, true));
			}

			requestFactory.VerifyAllExpectations();
		}

		void ValidateResponse(XElement expectedResponseElement, string expectedSoapHeaders)
		{
			var actualResponseXML = XDocument.Load(replyMessagePart.Data);
			Assert.IsTrue(SoapHelpersTest.ElementsAreEqual(actualResponseXML.Root, expectedResponseElement, true));
			replyMessage.AssertWasCalled(x => x.AddPart("Body", replyMessagePart, true));
			messageContext.AssertWasCalled(x => x.Write("InboundHeaders", WSHttpExPropertiesTest.PropertyNamespace, expectedSoapHeaders));
			var soapHeadersXML = XDocument.Parse(expectedSoapHeaders);
			var currentElement = (XElement)soapHeadersXML.Root.FirstNode;
			while (currentElement != null)
			{
				messageContext.AssertWasCalled(x => x.Write(currentElement.Name.LocalName, currentElement.GetDefaultNamespace().ToString(), currentElement.ToString(SaveOptions.DisableFormatting)));
				currentElement = (XElement)currentElement.NextNode;
			}
		}
	}

	class DummyWebResponse : WebResponse
	{
		readonly Stream responseStream;
		public WebHeaderCollection headers;

		public DummyWebResponse()
		{
			responseStream = new MemoryStream();
		}

		public override Stream GetResponseStream()
		{
			return responseStream;
		}
		public override WebHeaderCollection Headers
		{
			get
			{
				return headers;
			}
		}
	}

	class DummyWebRequest : WebRequest
	{
		readonly Stream requestStream;
		readonly DummyWebResponse response;

		public DummyWebRequest(Stream requestStream, DummyWebResponse response)
		{
			this.requestStream = requestStream;
			this.response = response;
		}

		public Exception ResponseException { get; set; }

		public override Stream GetRequestStream()
		{
			return requestStream;
		}

		public override WebResponse GetResponse()
		{
			if (ResponseException != null)
			{
				throw ResponseException;
			}
			return response;
		}
	}

	class WSHttpExTransferrerForTest : WSHttpExTransferrer
	{
		public WSHttpExTransferrerForTest(IBaseMessage message, IBaseMessageFactory messageFactory, IWebRequestFactory requestFactory) : base(message, WSHttpExPropertiesTest.PropertyNamespace, messageFactory, requestFactory) { }
		public WSHttpExProperties Config { get { return config; } }
	}
}
