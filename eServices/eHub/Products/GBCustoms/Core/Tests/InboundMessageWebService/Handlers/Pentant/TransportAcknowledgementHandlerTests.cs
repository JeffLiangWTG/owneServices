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
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService.Handlers.Pentant
{
	[TestFixture]
	public class TransportAcknowledgementHandlerTests : HandlerTestBase
	{
		[Test]
		public void TestTransportHandlerValidateIdAndGetRecipients_WhenConversationIdIsMissing_ShouldThrowHttpException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue = "tmmhMroEKUqcI4LSAglg4g==",
				ClientRegistrationType = "GBCustoms-Pentant",
				RequestContent = GetMessage("Pentant_TransportResponse_NoConvID.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_TransportResponse_NoConvID.xml")),
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
		public void TestBusinessHandlerValidateIdAndGetRecipients_WhenNoSubscriberIsFound_ShouldThrowHttpException()
		{
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue = "tmmhMroEKUqcI4LSAglg4g==",
				ExpectedRecipients = new string[0],
				ClientRegistrationType = "GBCustoms-Pentant",
				RequestContent = GetMessage("Pentant_TransportResponse.xml"),
				MessageDoc = XDocument.Parse(GetMessage("Pentant_TransportResponse.xml")),
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

		[TestCase("Pentant_TransportResponse.xml")]
		public void TestTransportHandlerHandleSuccessResponse(string inputXmlFile)
		{
			var insertCount = 1;
			var config = new TestConfiguration
			{
				ConnectionState = ConnectionState.Closed,
				SenderID = "GBCustoms-Pentant",
				SubscriptionType = "GBCDPE",
				SubscriptionValue = "tmmhMroEKUqcI4LSAglg4g==",
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
					var ns = "{http://www.cargowise.com/Schemas/Universal/2012/11}";
					var contextCollectionPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='ContextCollection']";
					var contextCollectionRoot = messageDoc.XPathSelectElement(contextCollectionPath);
					var contextNodes = contextCollectionRoot.Elements(ns + "Context");
					var conversationID = contextNodes.Where(m => m.Element(ns + "Type").Value == "ConversationID")
						.Select(m => m.Element(ns + "Value").Value).FirstOrDefault();
					var cspEntryTrakcingID = contextNodes.Where(m => m.Element(ns + "Type").Value == "CSPEntryTrackingID")
						.Select(m => m.Element(ns + "Value").Value).FirstOrDefault();
					var eHubTrackingID = contextNodes.Where(m => m.Element(ns + "Type").Value == "eHubTrackingID")
						.Select(m => m.Element(ns + "Value").Value).FirstOrDefault();
					
					var jobKeyPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='DataContext']/*[local-name()='DataTargetCollection']/*[local-name()='DataTarget']/*[local-name()='Key']";
					var jobTypePath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='DataContext']/*[local-name()='DataTargetCollection']/*[local-name()='DataTarget']/*[local-name()='Type']";
					var jobKey = messageDoc.XPathSelectElement(jobKeyPath)?.Value;
					var jobType = messageDoc.XPathSelectElement(jobTypePath)?.Value;
					Assert.AreEqual("tmmhMroEKUqcI4LSAglg4g==", cspEntryTrakcingID);
					Assert.AreEqual("64322e5f-2dd9-47ac-9279-d427c126a48d", conversationID);
					Assert.AreEqual("3a46d3f2-e19d-4b4a-b1e4-ea2f65591f35", eHubTrackingID);
					Assert.AreEqual($"recipient{insertCount}", message.ClientID);
					Assert.AreEqual("CustomsDeclaration", jobType);
					Assert.AreEqual("S00030780", jobKey);
					insertCount++;
				}).Repeat.Twice();

			config.SubscriptionAccessor.Expect(_ =>
				_.InsertSubscribedClients(
					Arg<SqlTransaction>.Is.Anything,
					Arg<string>.Is.Equal("GBCDPC"),
					Arg<string>.Is.Equal("GBCustoms-Pentant"),
					Arg<string[]>.Is.Equal(config.ExpectedRecipients),
					Arg<string>.Is.Equal("64322e5f-2dd9-47ac-9279-d427c126a48d"),
					Arg<string>.Is.Equal("tmmhMroEKUqcI4LSAglg4g=="),
					Arg<string>.Is.Equal("Message Number"))).Repeat.Once();

			var response = handler.ExecuteProcessTest();
			Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
			config.VerifyAll();
		}
		
		Dictionary<string, string> GetContextsFromUEvent(XDocument uEvent)
		{
			var contextCollection = uEvent.XPathSelectElement("/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='ContextCollection']");
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
			handler = MockRepository.GeneratePartialMock<TransportAcknowledgementHandlerForTest>(ProviderType.Pentant);
			foreach (var setting in AppSettingConfiguration)
			{
				handler.Expect(x => x.GetSettings(setting.Key)).Return(setting.Value);
			}

			foreach (var setting in MessageHeaderConfiguration)
			{
				handler.Expect(x => x.ExtractHeader(setting.Key)).Return(setting.Value);
			}
		}

		private TransportAcknowledgementHandlerForTest handler;
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
