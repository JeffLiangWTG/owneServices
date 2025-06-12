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

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.CNS
{
	[TestFixture]
	public class CSPNotificationsHandlerTests : HandlerTestBase
	{
		[Test]
		public void TestCSPNotificationHandlerValidateCSPId_WhenCSPIdIsMissing_ShouldThrowHttpException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-CNS",
				SubscriptionType = "GBCCNS",
				ClientRegistrationType = "GBCustoms-CNS",
				RequestContent = GetMessage("CSP_Response_noCSPID.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CSP_Response_noCSPID.xml")),
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = false,
				UseMockForInboxAccessor = false,
				UseMockForLogger = true,
			};

			handler.SetUpMock(config);
			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual((int)HttpStatusCode.NotImplemented, exception.GetHttpCode());
			Assert.AreEqual("Provider: CNS. No CSP/CNS-ID found from the received notification.", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestCSPNotificationHandlerValidateCSPId_WhenNoSubscriberIsFound_ShouldThrowHttpException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-CNS",
				SubscriptionType = "GBCCNS",
				SubscriptionValue = "CAW-1538042784198",
				SubscribedMessage = GetMessage("CNS_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new string[0],
				RequestContent = GetMessage("CSP_Response.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CSP_Response.xml")),
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
			Assert.AreEqual("No subscriber found for CSP/CNS-ID: [CAW-1538042784198]", exception.Message);

			Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
			Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);

			config.VerifyAll();
		}

		[Test]
		public void TestCSPHandlerHandleCSPResponse()
		{
			var insertCount = 1;
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-CNS",
				SubscriptionType = "GBCCNS",
				SubscriptionValue = "CAW-1538042784198",
				SubscribedMessage = GetMessage("CNS_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("CSP_Response.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CSP_Response.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("CSP_ResponseBody.xml")),
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

			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustoms-CNS"), Arg<string>.Is.Equal("GBCustoms"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				Assert.AreEqual("74AF7930-9068-4F6A-BF46-B861FF26C34B", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:eHubTrackingId", namespaceManager).Value);
				Assert.AreEqual("CAW-1538042784198", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:CSPEntryTrackingID", namespaceManager).Value);
				Assert.AreEqual($"recipient{insertCount}", message.ClientID);
				Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody", namespaceManager));
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
				insertCount++;
			}).Repeat.Twice();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[Test]
		public void TestCSPNotificationHandlerValidateCSPId_WhenNoSubscriberIsFound_ButValidFallback()
		{
			var insertCount = 1;
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-CNS",
				SubscriptionType = "GBCCNC",
				SubscriptionValue = "OIALHRGLO0000000010454",
				SubscribedMessage = GetMessage("CNS_SubscriptionFallback.xml"),
				ClientRegistrationType = "GBCustoms-CNS",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("CSP_Response_InvalidCSPID_ValidFuncRef.xml"),
				MessageDoc = XDocument.Parse(GetMessage("CSP_Response_InvalidCSPID_ValidFuncRef.xml")),
				MessageBodyDoc = XDocument.Parse(GetMessage("CSP_Response_InvalidCSPID_ValidFuncRef_Body.xml")),
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

			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustoms-CNS"), Arg<string>.Is.Equal("GBCustoms"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				Assert.AreEqual("74AF7930-9068-4F6A-BF46-B861FF26C34B", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:eHubTrackingId", namespaceManager).Value);
				Assert.AreEqual("OIALHRGLO0000000010454", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:CSPEntryTrackingID", namespaceManager).Value);
				Assert.AreEqual($"recipient{insertCount}", message.ClientID);
				Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody", namespaceManager));
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
				insertCount++;
			}).Repeat.Twice();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		[SetUp]
		public void SetupTestHandler()
		{
			handler = MockRepository.GeneratePartialMock<CSPNotificationHandlerForTest>(ProviderType.CNS);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}

			foreach (var setting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(setting.Key)).Return(setting.Value);
			}
		}

		private CSPNotificationHandlerForTest handler;
		Dictionary<string, string> AppSettingConfiguration = new Dictionary<string, string>
		{
			{"SqlDeadlockRetryCount", "1"},
			{"SqlDeadlockRetryWait", "1"},
			{"GBCustomsID", "GBCustoms-CNS"},
			{"GBCustomsTestID", "GBCustomsTest-CNS"},
			{"GBCustomsRegistrationType", "GBCustoms-CNS"},
			{"AuthorisationHeaderKey", "Authorization"},
			{"GBCustomsSubscriptionTypeByCSPID", "GBCCNS" },
			{"GBCustomsSubscriptionTypeByConversationID", "GBCCNC"},
			{"GetRecipientsRetryAttempts", "3"},
			{"GetRecipientsRetryWaitTime", "1"},
			{"SubscriptionProviderIDList", "GBCustoms,GBCustomsTest" }
		};
		Dictionary<string, string> MessageHeaderConfiguration = new Dictionary<string, string>
		{
			{"Authorization", "HeaderAuthorization"}
		};
		public override string Provider => "CNS";
	}
}
