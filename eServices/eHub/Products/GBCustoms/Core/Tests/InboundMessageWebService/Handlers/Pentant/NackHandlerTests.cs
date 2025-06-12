using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
	public class NackHandlerTests : HandlerTestBase
	{
		[Test]
		public void TestNackHandler_NoConversationID_ShouldThrowException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue= "tmmhMroEKUqcI4LSAglg4g==",
				SubscribedMessage = GetMessage("Pentant_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-Pentant",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage("Pentant_NackResponse_NoConvID.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_NackResponse_NoConvID.xml")),
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
		public void TestNackHandler_NoSubscription_ShouldThrowHTTPException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue = "tmmhMroEKUqcI4LSAglg4g==",
				ExpectedRecipients = new string[0],
				ClientRegistrationType = "GBCustoms-Pentant",
				RequestContent = GetMessage("Pentant_NackResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_NackResponse.xml")),
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
			Assert.AreEqual("No subscriber found for Message Number: [tmmhMroEKUqcI4LSAglg4g==]", exception.Message);

			Assert.AreEqual(3, handler.GetRecipientsRetryAttempts);
			Assert.AreEqual(1, handler.GetRecipientsRetryWaitTime);

			config.VerifyAll();
		}

		[Test]
		public void TestNackHandler_SubscriptionWithInvalidXML_ShouldThrowHTTPException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue = "tmmhMroEKUqcI4LSAglg4g==",
				SubscribedMessage = GetMessage("Pentant_Subscription_InvalidXML.xml"),
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				ClientRegistrationType = "GBCustoms-Pentant",
				RequestContent = GetMessage("Pentant_NackResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_NackResponse.xml")),
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

			config.Logger.Expect(x => x.Error("[Service ID: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa] Original XML message from subscription is not valid - Data at the root level is invalid. Line 1, position 1."));

			var exception = Assert.Throws<HttpException>(() => handler.ExecuteProcessTest());

			Assert.AreEqual(500, exception.GetHttpCode());
			Assert.AreEqual("Critical internal error. Do not retry. Use the enquiry ID and contact WiseTech Global.", exception.Message);
			config.VerifyAll();
		}

		[TestCase("Pentant_NackResponse.xml")]
		public void TestNackHandlerResponse(string inputXmlFile)
		{
			var insertCount = 1;
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue= "tmmhMroEKUqcI4LSAglg4g==",
				SubscribedMessage = GetMessage("Pentant_Subscription.xml"),
				ClientRegistrationType = "GBCustoms-Pentant",
				ExpectedRecipients = new[] { "recipient1", "recipient2" },
				RequestContent = GetMessage(inputXmlFile),
				MessageDoc = XDocument.Parse(GetMessage(inputXmlFile)),
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
			config.InboxAccessor.Expect(x => x.InsertToInbox(
				Arg<string>.Is.Equal("GBCustoms-Pentant"),
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
					Assert.AreEqual("CustomsDeclaration", messageDoc.XPathSelectElement("/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='DataContext']/*[local-name()='DataTargetCollection']/*[local-name()='DataTarget']/*[local-name()='Type']").Value);
					Assert.AreEqual("S00030780", messageDoc.XPathSelectElement("/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='DataContext']/*[local-name()='DataTargetCollection']/*[local-name()='DataTarget']/*[local-name()='Key']").Value);
					Assert.AreEqual("MRJ", messageDoc.XPathSelectElement("/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='EventType']").Value);
					Assert.AreEqual($"recipient{insertCount}", message.ClientID);

					var contexts = GetContextsFromUEvent(messageDoc);
					Assert.NotNull(contexts);
					Assert.AreEqual("3a46d3f2-e19d-4b4a-b1e4-ea2f65591f35", contexts["eHubTrackingID"]);
					Assert.AreEqual("PGVycm9yUmVzcG9uc2U+DQogIDxjb2RlPkJBRF9SRVFVRVNUPC9jb2RlPg0KICA8bWVzc2FnZT5YLUJhZGdlLUlkZW50aWZpZXIgaGVhZGVyIGlzIG1pc3Npbmcgb3IgaW52YWxpZDwvbWVzc2FnZT4NCiAgPGVycm9yPg0KICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPg0KICAgIDxtZXNzYWdlPmN2Yy1wYXR0ZXJuLXZhbGlkOiBWYWx1ZSAnVSBEJyBpcyBub3QgZmFjZXQtdmFsaWQgd2l0aCByZXNwZWN0IHRvIHBhdHRlcm4gJ1tBLVpdezN9JyBmb3IgdHlwZSAnSVNPM0FscGhhQ3VycmVuY3lDb2RlQ29udGVudFR5cGUnLjwvbWVzc2FnZT4NCiAgPC9lcnJvcj4NCiAgPGVycm9yPg0KICAgIDxjb2RlPnhtbF92YWxpZGF0aW9uX2Vycm9yPC9jb2RlPg0KICAgIDxtZXNzYWdlPmN2Yy1hdHRyaWJ1dGUuMzogVGhlIHZhbHVlICdVIEQnIG9mIGF0dHJpYnV0ZSAnY3VycmVuY3lJRCcgb24gZWxlbWVudCAnSXRlbUNoYXJnZUFtb3VudCcgaXMgbm90IHZhbGlkIHdpdGggcmVzcGVjdCB0byBpdHMgdHlwZSwgJ0lTTzNBbHBoYUN1cnJlbmN5Q29kZUNvbnRlbnRUeXBlJy48L21lc3NhZ2U+DQogIDwvZXJyb3I+DQo8L2Vycm9yUmVzcG9uc2U+", contexts["ResponseText"]);
					Assert.AreEqual("Pentant Document Submission Failed", contexts["Error"]);
					Assert.AreEqual("http://www.cargowise.com/Schemas/Universal/2012/11", message.SchemaName);
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
			handler = MockRepository.GeneratePartialMock<NackHandlerForTest>(ProviderType.Pentant);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}

			foreach (var setting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(setting.Key)).Return(setting.Value);
			}
		}

		private NackHandlerForTest handler;
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
