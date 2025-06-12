using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.MCP
{
	[TestFixture]
	public class TransportAcknowledgementHandlerTests : HandlerTestBase
	{
		[Test]
		public void TestBusinessHandlerValidateIdAndGetRecipients_WhenMcpIdIsMissing_ShouldThrowHttpException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCP",
				ClientRegistrationType = "GBCustoms-MCP",
				RequestContent = GetMessage("MCP_WrongCSP.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_WrongCSP.xml")),
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = false,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
			};
			handler.SetUpMock(config);

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual((int)HttpStatusCode.NotImplemented, exception.GetHttpCode());
			Assert.AreEqual("Provider: MCP. No CSP/MCP-ID found from the received notification.", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerValidateIdAndGetRecipients_WhenNoSubscriberIsFound_ShouldThrowHttpException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCP",
				SubscriptionValue = "CAW-1538042784198",
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = null,
				RequestContent = GetMessage("MCP_TransportSuccessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_TransportSuccessResponse.xml")),
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
			};
			handler.SetUpMock(config);
			handler.Expect(x => x.GetRecipientsWaitForRetry()).Repeat.Times(3);
			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual(465, exception.GetHttpCode());
			Assert.AreEqual("No subscriber found for CSP/MCP-ID: [CAW-1538042784198]", exception.Message);

			Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
			Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);

			config.VerifyAll();
		}

		[Test]
		[TestCase("MCP_TransportSuccessResponse.xml")]
		[TestCase("MCP_X-CSP-ID_TransportSuccessResponse.xml")]
		[TestCase("MCP_X-MCP-ID_TransportSuccessResponse.xml")]
		public void TestTransportHandlerHandleSuccessResponse(string inputXmlFile)
		{
			var insertCount = 1;
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCP",
				SubscriptionValue = "CAW-1538042784198",
				SubscribedMessage = GetMessage("MCP_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage(inputXmlFile),
				MessageDoc = XDocument.Parse(GetMessage(inputXmlFile)),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_TransportSuccessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};
			handler.SetUpMock(config);

			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustoms-MCP"), 
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<MessageStatus>.Is.Equal(MessageStatus.Received),
				Arg<eHubGatewayMessage>.Is.Anything,
				Arg<bool>.Is.Equal(false),
				Arg<SqlTransaction>.Is.Anything,
				Arg<string>.Is.Null)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				Assert.AreEqual("Accepted", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Status", namespaceManager).Value);
				Assert.AreEqual("a2a62d75-d008-4666-9578-80f949f0758b", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/ConversationId", namespaceManager).Value);
				Assert.AreEqual($"recipient{insertCount}", message.ClientID);
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsTransportResponse", message.SchemaName);
				insertCount++;
			}).Repeat.Twice();

			config.SubscriptionAccessor.Expect(_ =>
				_.InsertSubscribedClients(
					Arg<SqlTransaction>.Is.Anything,
					Arg<string>.Is.Equal("GBCMCC"),
					Arg<string>.Is.Equal("GBCustoms-MCP"),
					Arg<string[]>.Is.Equal(config.ExpectedRecipients),
					Arg<string>.Is.Equal("a2a62d75-d008-4666-9578-80f949f0758b"),
					Arg<string>.Is.Equal("CAW-1538042784198"),
					Arg<string>.Is.Equal("CSP-ID"))).Repeat.Once();
			
			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestTransportHandlerHandleErrorResponse()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCP",
				SubscriptionValue = "CAW-1538042784198",
				SubscribedMessage = GetMessage("MCP_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = GetMessage("MCP_TransportErrorResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_TransportErrorResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_TransportErrorResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};

			handler.SetUpMock(config);

			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustoms-MCP"),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<MessageStatus>.Is.Equal(MessageStatus.Received),
				Arg<eHubGatewayMessage>.Is.Anything,
				Arg<bool>.Is.Equal(false),
				Arg<SqlTransaction>.Is.Anything,
				Arg<string>.Is.Null)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				Assert.AreEqual("Error", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Status", namespaceManager).Value);
				Assert.AreEqual("401", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Error", namespaceManager).Value);
				Assert.AreEqual("Something's Wrong", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/ErrorText", namespaceManager).Value);
				Assert.AreEqual("recipient", message.ClientID);
				Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Response/notifications", namespaceManager));
			});

			config.SubscriptionAccessor.Expect(_ =>
				_.InsertSubscribedClients(
					Arg<SqlTransaction>.Is.Anything,
					Arg<Guid>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<string[]>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<string>.Is.Anything)).Repeat.Never();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestTransportHandlerHandleSchemaErrorResponse()
		{
			const string expectedResponseText = @"<errorResponse>
  <code>400</code>
  <message>Payload is not valid according to schema</message>
  <errors>
    <error>
      <code>xml_validation_error</code>
      <message>cvc-pattern-valid: Value 'MOVEX
' is not facet-valid with respect to pattern '.*[^\s].*' for type '#AnonType_PackagingMarksNumbersIDType'.
			</message>
    </error>
    <error>
      <code>xml_validation_error</code>
      <message>cvc-complex-type.2.2: Element 'MarksNumbersID' must have no element [children], and the value must be valid.</message>
    </error>
  </errors>
</errorResponse>";

			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCP",
				SubscriptionValue = "CAW-1538042784198",
				SubscribedMessage = GetMessage("MCP_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = GetMessage("MCP_TransportSchemaErrorResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_TransportSchemaErrorResponse.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_TransportSchemaErrorResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};

			handler.SetUpMock(config);

			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustoms-MCP"),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<MessageStatus>.Is.Equal(MessageStatus.Received),
				Arg<eHubGatewayMessage>.Is.Anything,
				Arg<bool>.Is.Equal(false),
				Arg<SqlTransaction>.Is.Anything,
				Arg<string>.Is.Null)).WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
					var namespaceManager = new XmlNamespaceManager(new NameTable());
					namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
					Assert.AreEqual("Error", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Status", namespaceManager).Value);
					Assert.AreEqual("400", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Error", namespaceManager).Value);
					Assert.AreEqual("Payload is not valid according to schema", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/ErrorText", namespaceManager).Value);
					Assert.AreEqual(expectedResponseText.Replace("\n", "").Replace("\r", ""), messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/ResponseText", namespaceManager).Value.Replace("\n", "").Replace("\r", ""));
					Assert.AreEqual("recipient", message.ClientID);
					Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Response/notifications", namespaceManager));
				});

			config.SubscriptionAccessor.Expect(_ =>
				_.InsertSubscribedClients(
					Arg<SqlTransaction>.Is.Anything,
					Arg<Guid>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<string[]>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<string>.Is.Anything)).Repeat.Never();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestTransportHandlerHandleErrorResponseServiceUnavailable()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCP",
				SubscriptionValue = "CAW-1571358882612",
				SubscribedMessage = GetMessage("MCP_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = new[] { "recipient" },
				RequestContent = GetMessage("MCP_TransportErrorResponseServiceUnavailable.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_TransportErrorResponseServiceUnavailable.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_TransportErrorResponseBodyServiceUnavailable.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};

			handler.SetUpMock(config);

			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustoms-MCP"),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<MessageStatus>.Is.Equal(MessageStatus.Received),
				Arg<eHubGatewayMessage>.Is.Anything,
				Arg<bool>.Is.Equal(false),
				Arg<SqlTransaction>.Is.Anything,
				Arg<string>.Is.Null)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDdoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				Assert.AreEqual("Error", messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Status", namespaceManager).Value);
				Assert.AreEqual("503", messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Error", namespaceManager).Value);
				Assert.AreEqual("CDS_SERVICE_UNAVAILABLE", messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/ErrorText", namespaceManager).Value);
				Assert.AreEqual("recipient", message.ClientID);
				Assert.IsNotNull(messageDdoc.XPathSelectElement("/s0:GBCustomsTransportResponse/Response/notifications", namespaceManager));
			});

			config.SubscriptionAccessor.Expect(_ =>
				_.InsertSubscribedClients(
					Arg<SqlTransaction>.Is.Anything,
					Arg<Guid>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<string[]>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<string>.Is.Anything,
					Arg<string>.Is.Anything)).Repeat.Never();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerValidateIdAndGetRecipients_InvalidCspId_ButValidFallback()
		{
			var insertCount = 1;
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-MCP",
				SubscriptionType = "GBCMCP",
				SubscriptionValue = "LIGLBALBA0000000184510",
				SubscribedMessage = GetMessage("MCP_Subscription_LRN.xml"),
				ClientRegistrationType = "GBCustoms-MCP",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("MCP_InvalidCSP_ValidFallback.xml"),
				MessageDoc = XDocument.Parse(GetMessage("MCP_InvalidCSP_ValidFallback.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("MCP_TransportSuccessResponseBody.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true,
				ClientPermittedRecipient = true,
			};
			handler.SetUpMock(config);
			handler.Expect(x => x.GetRecipientsWaitForRetry()).Repeat.Times(3);

			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustoms-MCP"),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<MessageStatus>.Is.Equal(MessageStatus.Received),
				Arg<eHubGatewayMessage>.Is.Anything,
				Arg<bool>.Is.Equal(false),
				Arg<SqlTransaction>.Is.Anything,
				Arg<string>.Is.Null)).WhenCalled(x =>
				{
					var message = x.Arguments[4] as eHubGatewayMessage;
					var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
					var namespaceManager = new XmlNamespaceManager(new NameTable());
					namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
					Assert.AreEqual("74AF7930-9068-4F6A-BF46-B861FF26C34B", messageDoc.XPathSelectElement("/s0:GBCustomsTransportResponse/eHubMessageTrackingId", namespaceManager).Value);
					Assert.AreEqual($"recipient{insertCount}", message.ClientID);
					Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsTransportResponse", message.SchemaName);
					insertCount++;
				}).Repeat.Twice();

			config.SubscriptionAccessor.Expect(_ =>
				_.InsertSubscribedClients(
					Arg<SqlTransaction>.Is.Anything,
					Arg<string>.Is.Equal("GBCMCC"),
					Arg<string>.Is.Equal("GBCustoms-MCP"),
					Arg<string[]>.Is.Equal(config.ExpectedRecipients),
					Arg<string>.Is.Equal("261d8887-18d0-460d-8198-71d6c83f7624"),
					Arg<string>.Is.Equal("FRF-1694683743064"),
					Arg<string>.Is.Equal("CSP-ID"))).Repeat.Once();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
			Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);

			config.VerifyAll();
		}

		[SetUp]
		public void SetupTestHandler()
		{
			handler = MockRepository.GeneratePartialMock<TransportAcknowledgementHandlerForTest>(ProviderType.MCP);
			foreach (var settting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(settting.Key)).Return(settting.Value);
			}

			foreach (var settting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(settting.Key)).Return(settting.Value);
			}
		}

		private TransportAcknowledgementHandlerForTest handler;
		Dictionary<string, string> AppSettingConfiguration = new Dictionary<string, string>
		{
			{"SqlDeadlockRetryCount", "1"},
			{"SqlDeadlockRetryWait", "1"},
			{"GBCustomsID", "GBCustoms-MCP"},
			{"GBCustomsTestID", "GBCustomsTest-MCP"},
			{"GBCustomsRegistrationType", "GBCustoms-MCP"},
			{"AuthorisationHeaderKey", "Authorization"},
			{"GBCustomsSubscriptionTypeByCSPID", "GBCMCP" },
			{"GBCustomsSubscriptionTypeByConversationID", "GBCMCC"},
			{"GetRecipientsRetryAttempts", "3"},
			{"GetRecipientsRetryWaitTime", "1"},
			{"SubscriptionProviderIDList", "GBCustoms,GBCustomsTest" }
		};
		Dictionary<string, string> MessageHeaderConfiguration = new Dictionary<string, string>
		{
			{"Authorization", "HeaderAuthorization"}
		};
		public override string Provider => "MCP";
	}
}
