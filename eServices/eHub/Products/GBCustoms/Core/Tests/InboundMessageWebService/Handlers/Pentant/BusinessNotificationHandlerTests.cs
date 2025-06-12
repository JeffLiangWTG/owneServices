using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.Pentant
{
	[TestFixture]
	public class BusinessNotificationHandlerTests : HandlerTestBase
	{
		[Test]
		public void TestBusinessHandlerValidateIdAndGetRecipients_WhenConversationIdIsMissing_ShouldThrowHttpException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPC",
				ClientRegistrationType = "GBCustoms-Pentant",
				RequestContent = GetMessage("Pentant_BusinessResponse_NoConvID.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_BusinessResponse_NoConvID.xml")),
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = false,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true
			};

			handler.SetUpMock(config);
			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual((int)HttpStatusCode.NotImplemented, exception.GetHttpCode());
			Assert.AreEqual("Provider: Pentant. No Conversation ID found from the received notification.", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerValidateIdAndGetRecipients_WhenNoSubscriberIsFound_ShouldThrowHttpException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPC",
				SubscriptionValue = "64322e5f-2dd9-47ac-9279-d427c126a48d",
				ClientRegistrationType = "GBCustoms-Pentant",
				ExpectedRecipients = new string[0],
				RequestContent = GetMessage("Pentant_BusinessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_BusinessResponse.xml")),
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = true,
				UseMockForInboxAccessor = true,
				UseMockForLogger = true
			};

			handler.SetUpMock(config);
			handler.Expect(x => x.GetRecipientsWaitForRetry()).Repeat.Times(3);

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual(465, exception.GetHttpCode());
			Assert.AreEqual("No subscriber found for Conversation ID: [64322e5f-2dd9-47ac-9279-d427c126a48d]", exception.Message);

			Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
			Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);

			config.VerifyAll();
		}

		[Test]
		public void TestBusinessHandlerHandleSuccessResponse()
		{
			var insertCount = 1;
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPC",
				SubscriptionValue = "64322e5f-2dd9-47ac-9279-d427c126a48d",
				ClientRegistrationType = "GBCustoms-Pentant",
				SubscriptionProviderIDList = new string[] { "GBCustoms", "GBCustomsTest" },
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("Pentant_BusinessResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_BusinessResponse.xml")),
				MessageBodyDoc = null,
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
			config.InboxAccessor.Expect(x => x.InsertToInboxAndOutbox(Arg<SqlTransaction>.Is.Anything, Arg<string>.Is.Equal("GBCustoms-Pentant"), Arg<string>.Is.Equal("GBCustoms"), Arg<Guid>.Is.Anything, Arg<eHubGatewayMessage>.Is.Anything)).WhenCalled(x =>
			{
				var message = x.Arguments[4] as eHubGatewayMessage;
				var messageDoc = XDocument.Parse(message.MessageStream.DecodeAndDecompress().ReadToEnd());
				Assert.AreEqual("64322e5f-2dd9-47ac-9279-d427c126a48d", messageDoc.XPathSelectElement("/*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='ConversationID']").Value);
				Assert.AreEqual("eHubTrackingID", messageDoc.XPathSelectElement("/*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='eHubTrackingId']").Value);
				Assert.IsNotNull(messageDoc.XPathSelectElement("/*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']"));
				Assert.AreEqual($"recipient{insertCount}", message.ClientID);
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
			handler = MockRepository.GeneratePartialMock<BusinessNotificationHandlerForTest>(ProviderType.Pentant);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}

			foreach (var setting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(setting.Key)).Return(setting.Value);
			}
		}

		private BusinessNotificationHandlerForTest handler;
		Dictionary<string, string> AppSettingConfiguration = new Dictionary<string, string>
		{
			{"SqlDeadlockRetryCount", "1"},
			{"SqlDeadlockRetryWait", "1"},
			{"GBCustomsID", "GBCustoms-Pentant"},
			{"GBCustomsTestID", "GBCustomsTest-Pentant"},
			{"GBCustomsRegistrationType", "GBCustoms-Pentant"},
			{"AuthorisationHeaderKey", "Authorization"},
			{"GBCustomsSubscriptionTypeByCSPID", "GBCDPE"},
			{"GBCustomsSubscriptionTypeByConversationID", "GBCDPC"},
			{"GetRecipientsRetryAttempts", "3"},
			{"GetRecipientsRetryWaitTime", "1"},
			{"SubscriptionProviderIDList", "GBCustoms,GBCustomsTest" }
		};
		Dictionary<string, string> MessageHeaderConfiguration = new Dictionary<string, string>
		{
			{"Authorization", "HeaderAuthorization"}
		};

		public override string Provider => "Pentant";
	}
}
