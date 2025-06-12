using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService;

using Common.Logging;

using NUnit.Framework;

using Rhino.Mocks;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CargoWise.eHub.Products.GBCustoms.Core.Tests.InboundMessageWebService
{
	[TestFixture]
	internal class InboundErrorTests
	{
		[Test]
		public void TestInboundError()
		{
			var testDate = DateTime.UtcNow;
			var messagePK = new Guid("11111111-1111-1111-1111-111111111111");

			var mockLogger = MockRepository.GenerateMock<ILog>();
			var mockException = new Exception("TestExceptionMessage");
			var mockMessage = GenerateTestMessage();
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockContext.Stub(x => x.eHubClients).Return(CreateGBCustomsClient());
			mockContext.Stub(x => x.eHubInboxMessages).Return(new TestDbSet<eHubInboxMessage>());
			mockContext.Stub(x => x.eHubErrors).Return(new TestDbSet<eHubError>());

			var expectedError = new eHubError()
			{
				EE_PK = messagePK,
				EE_Alerted = false,
				EE_DateTimeUTC = testDate,
				EE_Description = "TestSubject - TestExceptionMessage",
				EE_EI_Inbox = messagePK,
				EE_ErrorDetail = "<ErrorDetail>TestSubject - TestExceptionMessage</ErrorDetail>",
				EE_ErrorType = "EXP",
				EE_Source = "HUB"
			};

			var expectedMessage = new eHubInboxMessage()
			{
				EI_CC_Recipient = new Guid("00000000-0000-0000-0000-000000000000"),
				EI_ApplicationCode = "HUB",
				EI_MessageType = "CDS",
				EI_InsertUTC = testDate,
				EI_CC_Sender = new Guid("00000000-0000-0000-0000-000000000000"),
				EI_PK = messagePK,
				EI_Status = 255,
				EI_MessageTrackingID = messagePK.ToString(),
				EI_Content = "H4sIAAAAAAAEAHOtSE4tKMnMz1NwSS1JzMwptuLlCkktLnGFifumFhcnpqfycoGgZ15SfmleigJUUMEjNTEltQimxzu1UkFXAcQKS8wpTcWmwSk/pRKqGm4wAIYborGDAAAA"
			};

			var inboundError = new InboundError(mockContext, mockLogger, "GBCustoms");
			inboundError.InternalUtcNow = () => testDate;
			inboundError.InternalNewGuid = () => messagePK;
			inboundError.CreateFailedMessage(mockMessage, "TestSubject", mockException);

			Assert.AreEqual(1, mockContext.eHubErrors.Count());
			Assert.AreEqual(1, mockContext.eHubInboxMessages.Count());

			var error = mockContext.eHubErrors.First();
			var inboxMessage = mockContext.eHubInboxMessages.First();

			Assert.AreEqual(expectedError.EE_Alerted, error.EE_Alerted);
			Assert.AreEqual(expectedError.EE_DateTimeUTC, error.EE_DateTimeUTC);
			Assert.AreEqual(expectedError.EE_Description, error.EE_Description);
			Assert.AreEqual(expectedError.EE_EI_Inbox, error.EE_EI_Inbox);
			Assert.AreEqual(expectedError.EE_ErrorDetail, error.EE_ErrorDetail);
			Assert.AreEqual(expectedError.EE_ErrorType, error.EE_ErrorType);
			Assert.AreEqual(expectedError.EE_PK, error.EE_PK);

			Assert.AreEqual(expectedMessage.EI_Status, inboxMessage.EI_Status);
			Assert.AreEqual(expectedMessage.EI_PK, inboxMessage.EI_PK);
			Assert.AreEqual(expectedMessage.EI_MessageTrackingID, inboxMessage.EI_MessageTrackingID);
			Assert.AreEqual(expectedMessage.EI_CC_Sender, inboxMessage.EI_CC_Sender);
			Assert.AreEqual(expectedMessage.EI_CC_Recipient, inboxMessage.EI_CC_Recipient);
			Assert.AreEqual(expectedMessage.EI_ApplicationCode, inboxMessage.EI_ApplicationCode);
			Assert.AreEqual(expectedMessage.EI_MessageType, inboxMessage.EI_MessageType);
			Assert.AreEqual(expectedMessage.EI_Content, inboxMessage.EI_Content);
		}

		private HttpRequestMessage GenerateTestMessage()
		{
			var mockMessage = new HttpRequestMessage();
			mockMessage.Headers.Add("TestKey", "TestValue");
			mockMessage.Content = new StringContent("TestMessage", new UTF8Encoding(false), @"application/text");
			return mockMessage;
		}

		private TestDbSet<eHubClient> CreateGBCustomsClient()
		{
			var ehubClients = new TestDbSet<eHubClient>();
			ehubClients.Add(new eHubClient()
			{
				CC_PK = new Guid("00000000-0000-0000-0000-000000000000"),
				CC_ID = "GBCustoms"
			});

			return ehubClients;
		}
	}
}
