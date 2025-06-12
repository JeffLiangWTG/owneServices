using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ZACustoms.Helpers;
using CargoWise.eHub.Products.ZACustoms.Tests.TestFiles;
using CargoWise.eHub.Shared.Crypto;
using CargoWise.eHub.Shared.Mime;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ZACustoms.Tests.Helpers
{
	[TestClass]
	public class AS2HelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AS2Helper_CreateSignedEncryptedMessage()
		{
			var senderCertificate = new eHubCertificate
			{
				CE_BinaryContainer = TestFileHelpers.GetResourceData("TestFiles.00505655TST.pfx"),
				CE_Password = "miragef7"
			};
			var zaCustomsCertificate = new eHubCertificate
			{
				CE_BinaryContainer = TestFileHelpers.GetResourceData("TestFiles.00505655TST.cer")
			};

			AS2Helper.GetSenderCertificate = (senderID, recipientID, as2From, retrying, logger) => senderCertificate;
			AS2Helper.GetZACustomsCertificate = (senderID, as2From, logger) => zaCustomsCertificate;
			MimePart.GetNewGuid = () => Guid.Empty;

			var messageStream = AS2Helper.CreateSignedEncryptedMessage(TestFileHelpers.GetResourceText("TestFiles.REQDOC.txt"), "SENDERID", "RECIPID", "AS2FROM", "AS2TO", "MSGID", "FILE.NAM", false, new NoOpLogger());

			string decryptedMessage = Encoding.UTF8.GetString(CmsHelpers.DecryptMessage(((MemoryStream)messageStream).ToArray(), senderCertificate.CE_BinaryContainer, senderCertificate.CE_Password));
			string expectedMessage = TestFileHelpers.GetResourceText("TestFiles.REQDOC-as2-signed.txt");

			Assert.AreEqual(expectedMessage, decryptedMessage);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AS2Helper_RetrieveStatusFromMdnMessage()
		{
			string mdnSuccessMsg = TestFileHelpers.GetResourceText("TestFiles.MDN-Success.txt");
			string mdnErrorMsg = TestFileHelpers.GetResourceText("TestFiles.MDN-Error.txt");

			bool wasSuccessful;
			string errorDescription;

			wasSuccessful = AS2Helper.RetrieveStatusFromMdnMessage(mdnSuccessMsg, out errorDescription);
			Assert.IsTrue(wasSuccessful);
			Assert.IsNull(errorDescription);

			wasSuccessful = AS2Helper.RetrieveStatusFromMdnMessage(mdnErrorMsg, out errorDescription);
			Assert.IsFalse(wasSuccessful);
			Assert.AreEqual("processed/error: decryption-failed", errorDescription);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void AS2Helper_RetrieveStatusFromMdnMessageWithBody()
		{
			string mdnSuccessWithBodyMsg = TestFileHelpers.GetResourceText("TestFiles.MDN-SuccessWithBody.txt");

			bool wasSuccessful;
			string errorDescription;

			wasSuccessful = AS2Helper.RetrieveStatusFromMdnMessage(mdnSuccessWithBodyMsg, out errorDescription);
			Assert.IsTrue(wasSuccessful);
			Assert.IsNull(errorDescription);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRetrieveStatusFromSubscription_NoSubscription_ReturnZero()
		{
			var subType = new eHubSubscriptionType
			{
				ST_PK = Guid.NewGuid(),
				ST_ID = Constants.SubscriptionType,
				ST_ExpiryDays = 2,
				ST_Name = "mock type",
			};
			var contextMock = MockRepository.GenerateMock<eHubTransactionsContext>();
			var logger = MockRepository.GenerateMock<ILog>();
			var eHubSubscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			contextMock.Expect(_ => _.eHubSubscriptionValues).Return(eHubSubscriptionValues).Repeat.AtLeastOnce();
			contextMock.Expect(_ => _.eHubSubscriptionTypes).Return(new TestDbSet<eHubSubscriptionType> { subType }).Repeat.AtLeastOnce();
			AS2Helper.GetContext = () => contextMock;

			var nonExistTrackingId = Guid.NewGuid().ToString();
			var result = AS2Helper.RetrieveStatusFromSubscription(nonExistTrackingId, logger);

			Assert.AreEqual((int)AS2Helper.SubscriptionStatus.NotExist, result);
			logger.VerifyAllExpectations();
			contextMock.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRetrieveStatusFromSubscription_SubscriptionExists_ReturnSubscriptionValue()
		{
			var existTrackingId = Guid.NewGuid().ToString();
			var subType = new eHubSubscriptionType
			{
				ST_PK = Guid.NewGuid(),
				ST_ID = Constants.SubscriptionType,
				ST_ExpiryDays = 2,
				ST_Name = "mock type",
			};
			var subValue = new eHubSubscriptionValue
			{
				SV_PK = Guid.NewGuid(),
				SV_ST = subType.ST_PK,
				SV_CC_Sender = Guid.NewGuid(),
				SV_CC_Recipient = Guid.NewGuid(),
				SV_SubscribedUTC = DateTime.UtcNow,
				SV_ExpiryUTC = DateTime.UtcNow.AddDays(subType.ST_ExpiryDays.Value),
				SV_Reference = ((int)AS2Helper.SubscriptionStatus.Processing).ToString(),
				SV_ReferenceType = nameof(AS2Helper.SubscriptionStatus),
				SV_Value = existTrackingId,
			};

			var contextMock = MockRepository.GenerateMock<eHubTransactionsContext>();
			var logger = MockRepository.GenerateMock<ILog>();
			contextMock.Expect(_ => _.eHubSubscriptionTypes).Return(new TestDbSet<eHubSubscriptionType>{ subType }).Repeat.AtLeastOnce();
			contextMock.Expect(_ => _.eHubSubscriptionValues).Return(new TestDbSet<eHubSubscriptionValue>{ subValue }).Repeat.AtLeastOnce();
			AS2Helper.GetContext = () => contextMock;

			var result = AS2Helper.RetrieveStatusFromSubscription(existTrackingId, logger);

			Assert.AreEqual((int)AS2Helper.SubscriptionStatus.Processing, result);
			logger.VerifyAllExpectations();
			contextMock.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestRetrieveStatusFromSubscription_ExceptionCalledLoggerAndReturnZero()
		{
			var existTrackingId = Guid.NewGuid().ToString();
			var subType = new eHubSubscriptionType
			{
				ST_PK = Guid.NewGuid(),
				ST_ID = Constants.SubscriptionType,
				ST_ExpiryDays = 2,
				ST_Name = "mock type",
			};

			var exceptionMock = new Exception("Mocking exception message");
			var contextMock = MockRepository.GenerateMock<eHubTransactionsContext>();
			var logger = MockRepository.GenerateMock<ILog>();
			contextMock.Expect(_ => _.eHubSubscriptionTypes).Return(new TestDbSet<eHubSubscriptionType> { subType }).Repeat.Any();
			contextMock.Expect(_ => _.eHubSubscriptionValues).Throw(exceptionMock).Repeat.AtLeastOnce();
			logger.Expect(_ => _.Error($"Retrieving subscription status error [MessageTrackingID: {existTrackingId}]", exceptionMock)).Repeat.Once();
			AS2Helper.GetContext = () => contextMock;

			var result = AS2Helper.RetrieveStatusFromSubscription(existTrackingId, logger);

			Assert.AreEqual((int)AS2Helper.SubscriptionStatus.NotExist, result);
			logger.VerifyAllExpectations();
			contextMock.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAddOrUpdateSubscriptionStatus_NewEntry_SaveNewEntry()
		{
			var nonExistTrackingId = Guid.NewGuid().ToString();
			var subType = new eHubSubscriptionType
			{
				ST_PK = Guid.NewGuid(),
				ST_ID = Constants.SubscriptionType,
				ST_ExpiryDays = 2,
				ST_Name = "mock type",
			};
			var senderCode = Guid.NewGuid().ToString();
			var recipientCode = Guid.NewGuid().ToString();
			var status = (int)AS2Helper.SubscriptionStatus.Success;

			var eHubClients = new TestDbSet<eHubClient>
			{
				new eHubClient
				{
					CC_PK = Guid.NewGuid(),
					CC_ID = senderCode
				},
				new eHubClient
				{
					CC_PK = Guid.NewGuid(),
					CC_ID = recipientCode,
				}
			};
			var eHubSubscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			var contextMock = MockRepository.GenerateMock<eHubTransactionsContext>();
			var logger = MockRepository.GenerateMock<ILog>();
			contextMock.Expect(_ => _.eHubSubscriptionTypes).Return(new TestDbSet<eHubSubscriptionType>{ subType }).Repeat.AtLeastOnce();
			contextMock.Expect(_ => _.eHubSubscriptionValues).Return(eHubSubscriptionValues).Repeat.AtLeastOnce();
			contextMock.Expect(_ => _.eHubClients).Return(eHubClients).Repeat.AtLeastOnce();
			contextMock.Expect(_ => _.SaveChanges()).Return(1).Repeat.Once();
			AS2Helper.GetContext = () => contextMock;

			AS2Helper.AddOrUpdateSubscriptionStatus(nonExistTrackingId, senderCode, recipientCode, status, logger);

			var result = eHubSubscriptionValues.FirstOrDefault();
			Assert.IsNotNull(result);
			Assert.AreEqual(eHubClients.Single(_ => _.CC_ID == senderCode).CC_PK.ToString(), result.SV_CC_Sender.ToString());
			Assert.AreEqual(eHubClients.Single(_ => _.CC_ID == recipientCode).CC_PK.ToString(), result.SV_CC_Recipient.ToString());
			Assert.AreEqual(subType.ST_PK, result.SV_ST);
			Assert.AreEqual(status.ToString(), result.SV_Reference);
			Assert.AreEqual(nameof(AS2Helper.SubscriptionStatus), result.SV_ReferenceType);
			Assert.AreEqual(nonExistTrackingId, result.SV_Value);
			Assert.AreEqual(subType.ST_ExpiryDays, (result.SV_ExpiryUTC.Value - result.SV_SubscribedUTC).Days);

			logger.VerifyAllExpectations();
			contextMock.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAddOrUpdateSubscriptionStatus_ExistingEntry_UpdateEntry()
		{
			var existTrackingId = Guid.NewGuid().ToString();
			var senderCode = "SenderCode";
			var recipientCode = "RecipientCode";
			var subType = new eHubSubscriptionType
			{
				ST_PK = Guid.NewGuid(),
				ST_ID = Constants.SubscriptionType,
				ST_ExpiryDays = 2,
				ST_Name = "mock type",
			};
			var subValue = new eHubSubscriptionValue
			{
				SV_PK = Guid.NewGuid(),
				SV_ST = subType.ST_PK,
				SV_CC_Sender = Guid.NewGuid(),
				SV_CC_Recipient = Guid.NewGuid(),
				SV_SubscribedUTC = DateTime.UtcNow,
				SV_ExpiryUTC = DateTime.UtcNow.AddDays(subType.ST_ExpiryDays.Value),
				SV_Reference = ((int)AS2Helper.SubscriptionStatus.Processing).ToString(),
				SV_ReferenceType = nameof(AS2Helper.SubscriptionStatus),
				SV_Value = existTrackingId,
			};

			var eHubClients = new TestDbSet<eHubClient>
			{
				new eHubClient
				{
					CC_PK = subValue.SV_CC_Sender,
					CC_ID = senderCode,
				},
				new eHubClient
				{
					CC_PK = subValue.SV_CC_Recipient,
					CC_ID = recipientCode,
				}
			};
			var status = (int)AS2Helper.SubscriptionStatus.Processing;
			var eHubSubscriptionValues = new TestDbSet<eHubSubscriptionValue> { subValue };
			var contextMock = MockRepository.GenerateMock<eHubTransactionsContext>();
			var logger = MockRepository.GenerateMock<ILog>();
			contextMock.Expect(_ => _.eHubSubscriptionTypes).Return(new TestDbSet<eHubSubscriptionType> { subType }).Repeat.AtLeastOnce();
			contextMock.Expect(_ => _.eHubSubscriptionValues).Return(eHubSubscriptionValues).Repeat.AtLeastOnce();
			contextMock.Expect(_ => _.eHubClients).Return(eHubClients).Repeat.Never();
			contextMock.Expect(_ => _.SaveChanges()).Return(1).Repeat.Once();
			AS2Helper.GetContext = () => contextMock;

			AS2Helper.AddOrUpdateSubscriptionStatus(existTrackingId, senderCode, recipientCode, status, logger);

			var result = eHubSubscriptionValues.FirstOrDefault();
			Assert.IsNotNull(result);
			Assert.AreEqual(1, eHubSubscriptionValues.Count());
			Assert.AreEqual(status.ToString(), result.SV_Reference);

			logger.VerifyAllExpectations();
			contextMock.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAddOrUpdateSubscriptionStatus_ExceptionCalledLogger()
		{
			var existTrackingId = Guid.NewGuid().ToString();
			var subType = new eHubSubscriptionType
			{
				ST_PK = Guid.NewGuid(),
				ST_ID = Constants.SubscriptionType,
				ST_ExpiryDays = 2,
				ST_Name = "mock type",
			};
			var subValue = new eHubSubscriptionValue
			{
				SV_PK = Guid.NewGuid(),
				SV_ST = subType.ST_PK,
				SV_CC_Sender = Guid.NewGuid(),
				SV_CC_Recipient = Guid.NewGuid(),
				SV_SubscribedUTC = DateTime.UtcNow,
				SV_ExpiryUTC = DateTime.UtcNow.AddDays(subType.ST_ExpiryDays.Value),
				SV_Reference = ((int)AS2Helper.SubscriptionStatus.Processing).ToString(),
				SV_ReferenceType = nameof(AS2Helper.SubscriptionStatus),
				SV_Value = existTrackingId,
			};
			
			var status = 1000;
			var exceptionMock = new Exception("testing exception message");
			var eHubSubscriptionValues = new TestDbSet<eHubSubscriptionValue> { subValue };
			var contextMock = MockRepository.GenerateMock<eHubTransactionsContext>();
			var logger = MockRepository.GenerateMock<ILog>();
			contextMock.Expect(_ => _.eHubSubscriptionTypes).Return(new TestDbSet<eHubSubscriptionType> { subType }).Repeat.AtLeastOnce();
			contextMock.Expect(_ => _.eHubSubscriptionValues).Return(eHubSubscriptionValues).Repeat.AtLeastOnce();
			contextMock.Expect(_ => _.SaveChanges()).Throw(exceptionMock).Repeat.Once();
			logger.Expect(_ => _.Error($"Failed to add/update subscription status, MessageTrackingID: {existTrackingId}, SenderID: SenderCode, RecipientID: RecipientCode, Status: {status}.", exceptionMock)).Repeat.Once();
			AS2Helper.GetContext = () => contextMock;

			AS2Helper.AddOrUpdateSubscriptionStatus(existTrackingId, "SenderCode", "RecipientCode", status, logger);

			logger.VerifyAllExpectations();
			contextMock.VerifyAllExpectations();
		}
	}
}
