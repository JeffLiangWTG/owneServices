using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using Rhino.Mocks;

using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.Pentant
{
	[TestFixture]
	public class INVRESHandlerTests : HandlerTestBase
	{
		[Test]
		public void TestINVRESHandler_NoMessageID_ShouldThrowException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue= "qFrRJ9Y7NUeW0Jse98Xa7g==",
				SubscribedMessage = GetMessage("Pentant_INVRES_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-Pentant",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("Pentant_INVRES_NoMessageID.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_INVRES_NoMessageID.xml")),
				MessageBodyDoc = null,
				ServiceID = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
				Header = "HeaderAuthorization",
				UseMockForConnection = true,
				UseMockForClientRegistrationAccessor = true,
				UseMockForSubscriptionAccessor = false,
				UseMockForInboxAccessor = false,
				UseMockForLogger = true
			};

			handler.SetUpMock(config);

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual((int)HttpStatusCode.NotImplemented, exception.GetHttpCode());
			Assert.AreEqual("Provider: Pentant. No Message Number found from the received notification.", exception.Message);
			config.VerifyAll();
		}

		[Test]
		public void TestINVRESHandler_NoSubscription_ShouldThrowHTTPException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue = "qFrRJ9Y7NUeW0Jse98Xa7g==",
				ExpectedRecipients = new string[0],
				ClientRegistrationType = "GBCustoms-Pentant",
				RequestContent = GetMessage("Pentant_INVRES.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_INVRES.xml")),
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
			Assert.AreEqual("No subscriber found for Message Number: [qFrRJ9Y7NUeW0Jse98Xa7g==]", exception.Message);

			Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
			Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);

			config.VerifyAll();
		}

		[Test]
		public void TestINVRESHandlerSuccessResponse()
		{
			var insertCount = 1;
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue= "qFrRJ9Y7NUeW0Jse98Xa7g==",
				SubscribedMessage = GetMessage("Pentant_INVRES_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-Pentant",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("Pentant_INVRES.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_INVRES.xml")),
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
				var namespaceManager = new XmlNamespaceManager(new NameTable());
				namespaceManager.AddNamespace("s0", "http://cargowise.com/ehub/products/GBCustoms");
				Assert.AreEqual("3a46d3f2-e19d-4b4a-b1e4-ea2f65591f35", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:eHubTrackingId", namespaceManager).Value);
				Assert.AreEqual("qFrRJ9Y7NUeW0Jse98Xa7g==", messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:eHubTrackingIdHashed", namespaceManager).Value);
				Assert.AreEqual($"recipient{insertCount}", message.ClientID);
				Assert.IsNotNull(messageDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody", namespaceManager));
				Assert.AreEqual("http://cargowise.com/ehub/products/GBCustoms#GBCustomsBusinessResponse", message.SchemaName);
				insertCount++;
			}).Repeat.Twice();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}

		Dictionary<string, string> GetContextsFromUEvent(XDocument uEvent)
		{
			var contextCollection = uEvent.XPathSelectElement("/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='ContextCollection']");
			if (contextCollection == null) return null;

			var contexts = contextCollection.Elements().Select(e => e.Elements().Select(y => new { y.Value, y.Name.LocalName }).ToList()).ToList();
			if (contexts.Count == 0) return null;

			var contextDict = new Dictionary<string, string>();
			foreach (var context in contexts)
			{
				string key = "";
				string value = "";
				foreach (var contextPair in context)
				{
					if (contextPair.LocalName == "Type")
					{
						key = contextPair.Value;
					}
					if (contextPair.LocalName == "Value")
					{
						value = contextPair.Value;
					}
				}
				contextDict.Add(key, value);
			}
			if (contextDict.Count == 0) return null;

			return contextDict;
		}

		[SetUp]
		public void SetupTestHandler()
		{
			handler = MockRepository.GeneratePartialMock<INVRESHandlerForTest>(ProviderType.Pentant);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}

			foreach (var setting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(setting.Key)).Return(setting.Value);
			}
		}

		private INVRESHandlerForTest handler;
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
