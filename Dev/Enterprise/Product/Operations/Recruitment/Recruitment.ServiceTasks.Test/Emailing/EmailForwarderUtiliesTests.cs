using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.ServiceTasks.Emailing;
using MailManager;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.ServiceTasks
{
	sealed class EmailForwarderUtilitiesTests : TransactionedTestCase
	{
		public static MailItem NewMailItem(BusinessObjectFactory factory)
		{
			var mailItem = factory.NewWithValidTestData<MailItem>();
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			return mailItem;
		}

		public void TestTryFindExistingJobConversation_Defaults()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			_ = factory.NewWithValidTestData<JobConversation>();
			factory.Save();

			// act
			var result = EmailForwarderUtilities.TryFindExistingJobConversation(mailItem);

			// assert
			AssertNull(result);
		}

		public void TestTryFindExistingJobConversation_ByBody()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			var conversation = factory.NewWithValidTestData<JobConversation>();
			var msgId = "test_message_id";
			mailItem.SetHeaderItem("Message-ID", msgId);
			mailItem.MI_Body = $"###{EmailBizoEncoder.EncodeBizo(conversation)}###";
			factory.Save();

			// act
			Assert(!conversation.Messages.Any());
			var result = EmailForwarderUtilities.TryFindExistingJobConversation(mailItem);

			// assert
			AssertEquals(conversation.PK, result.PK);
		}

		public void TestExtractDecodedBody_PlainText()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			var conversation = factory.NewWithValidTestData<JobConversation>();
			mailItem.MI_Body = $"###{EmailBizoEncoder.EncodeBizo(conversation)}###";
			factory.Save();

			// act
			var decodedBody = EmailForwarderUtilities.ExtractDecodedBody(mailItem);
			AssertEquals(mailItem.MI_Body, decodedBody);
		}

		public void TestExtractDecodedBody_EncodedBase64()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			var conversation = factory.NewWithValidTestData<JobConversation>();
			var plaintextBody = $"###{EmailBizoEncoder.EncodeBizo(conversation)}###";
			var base64Body = Convert.ToBase64String(Encoding.Default.GetBytes(plaintextBody));
			mailItem.MI_Body = base64Body;
			mailItem.SetHeaderItem("Content-Transfer-Encoding", "base64");
			mailItem.SetHeaderItem("Content-Type", "text/plain; charset=\"utf-8\"");
			factory.Save();

			// act
			var decodedBody = EmailForwarderUtilities.ExtractDecodedBody(mailItem);
			AssertEquals(plaintextBody, decodedBody);
		}

		public void TestExtractDecodedBody_MultipartQuotedPrintable()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			mailItem.MI_Header = RecruitmentEmailDataHelpers.MultiPartQuotedPrintableHeader;
			mailItem.MI_Body = RecruitmentEmailDataHelpers.MultiPartQuotedPrintableBody;

			var conversation = factory.NewWithPrimaryKey<JobConversation>(new Guid("c4bf2140-5d4b-4a7d-b482-7f8cdcf38f59"));
			conversation.FillWithValidTestData();
			factory.Save();

			// act
			var conv = EmailForwarderUtilities.TryFindExistingJobConversation(mailItem);

			// assert
			AssertNotNull(conv);
			AssertEquals(conversation.PK, conv.PK);
		}

		public void TestCreateForwardsEmails_Single()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			factory.Save();

			var fromAddress = "test@wisetechglobal.com";
			var recipientAddress = "test@example.com";

			// act
			var forwarded = EmailForwarderUtilities.CreateForwardsEmails(recipientAddress, mailItem, fromAddress);

			// assert
			AssertForwardedEmailIsAppropriate(mailItem, forwarded, fromAddress, recipientAddress);
		}
		public void TestCreateForwardsEmails_DoesNotContainSender()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			factory.Save();

			var fromAddress = "test@wisetechglobal.com";
			var recipientAddress = "test@example.com";
			var recipientAddresses = new List<string> { recipientAddress, fromAddress };

			// act
			var forwardedEmails = EmailForwarderUtilities.CreateForwardsEmails(recipientAddresses, mailItem, fromAddress);

			// assert
			AssertEquals(1, forwardedEmails.Count());
			Assert(!forwardedEmails.Any(m => m.GetHeaderItem("To").Contains(fromAddress)));
			foreach (var forwardedEmail in forwardedEmails)
			{
				AssertForwardedEmailIsAppropriate(mailItem, forwardedEmail, fromAddress, recipientAddress);
			}
		}

		void AssertForwardedEmailIsAppropriate(MailItem original, MailItem forwarded, string fromAddress, string recipientAddress)
		{
			// changed fields
			CombineAssertions(() =>
			{
				AssertEquals("MMT", forwarded.MI_Application);
				AssertEquals("QUE", forwarded.MI_Status);
				AssertEquals(DirectionList.Codes.Transmit, forwarded.MI_Direction);
				AssertEquals(fromAddress, forwarded.MI_From);
				AssertEquals(fromAddress, forwarded.MI_ReplyTo);
				AssertEquals(ZDateTime.Empty, forwarded.MI_SendDateTime);

				// copied fields
				AssertEquals(original.MI_Subject, forwarded.MI_Subject);
				AssertEquals(original.MI_LastAttemptDateTime, forwarded.MI_LastAttemptDateTime);
				AssertEquals(original.MI_ReceivedDateTime, forwarded.MI_ReceivedDateTime);
				AssertEquals(original.MI_SystemCreateTimeUtc, forwarded.MI_SystemCreateTimeUtc);
				AssertEquals(original.MI_SystemCreateUser, forwarded.MI_SystemCreateUser);
				AssertEquals(original.MI_Body, forwarded.MI_Body);
				AssertEquals(original.MI_XMLInfo, forwarded.MI_XMLInfo);
				AssertEquals(original.MI_ContentType, forwarded.MI_ContentType);
				AssertEquals(original.MI_Encoding, forwarded.MI_Encoding);

				// changed headers
				AssertEquals(original.MI_Subject, forwarded.GetHeaderItem("Subject"));
				AssertEquals(fromAddress, forwarded.GetHeaderItem("From"));
				AssertEquals(recipientAddress, forwarded.GetHeaderItem("To"));
				AssertEquals(fromAddress, forwarded.GetHeaderItem("Reply-To"));

				// copied headers
				AssertEquals(original.GetHeaderItem("References"), forwarded.GetHeaderItem("References"));
				AssertEquals(original.GetHeaderItem("MIME-Version"), forwarded.GetHeaderItem("MIME-Version"));
				AssertEquals(original.GetHeaderItem("Content-Transfer-Encoding"), forwarded.GetHeaderItem("Content-Transfer-Encoding"));
				AssertEquals(original.GetHeaderItem("Content-Type"), forwarded.GetHeaderItem("Content-Type"));
				AssertEquals(original.GetHeaderItem("Date"), forwarded.GetHeaderItem("Date"));
				AssertEquals(original.GetHeaderItem("Accept-Language"), forwarded.GetHeaderItem("Accept-Language"));
				AssertEquals(original.GetHeaderItem("Content-Language"), forwarded.GetHeaderItem("Content-Language"));
				AssertEquals(original.GetHeaderItem("Message-ID"), forwarded.GetHeaderItem("Message-ID"));
				AssertEquals(original.GetHeaderItem("Thread-Topic"), forwarded.GetHeaderItem("Thread-Topic"));
				AssertEquals(original.GetHeaderItem("Thread-Index"), forwarded.GetHeaderItem("Thread-Index"));
				AssertEquals(original.GetHeaderItem("In-Reply-To"), forwarded.GetHeaderItem("In-Reply-To"));
			});
		}

		public void TestCreateForwardsEmails_Multiple()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			factory.Save();

			var fromAddress = "test@wisetechglobal.com";
			var recipientAddresses = new string[] { "test1@example.com", "test2@example.com", "text3@example.com" };

			// act
			var forwardedEmails = EmailForwarderUtilities.CreateForwardsEmails(recipientAddresses, mailItem, fromAddress);

			// assert
			foreach (var forwarded in forwardedEmails.Zip(recipientAddresses, (mail, address) => (mail, address)))
			{
				AssertForwardedEmailIsAppropriate(mailItem, forwarded.mail, fromAddress, forwarded.address);
			}
		}

		public void TestCopyHeaderIfSet()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItemFrom = NewMailItem(factory);
			var mailItemTo = NewMailItem(factory);
			mailItemFrom.SetHeaderItem("Subject", "test");

			factory.Save();

			// act
			EmailForwarderUtilities.CopyHeaderIfSet(mailItemFrom, mailItemTo, "Subject");

			// assert
			AssertEquals(mailItemFrom.GetHeaderItem("Subject"), mailItemTo.GetHeaderItem("Subject"));
		}

		public void TestSendForwardsEmails_NoRecipients()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			var recipients = new List<string> { };

			var mailSender = new MockIMailSender();
			using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
			{
				var forwardedEmails = EmailForwarderUtilities.CreateForwardsEmails(recipients, mailItem, "test@example.com");

				// act and assert
				AssertSequencesEqual(
					"No recipients => no emails forwarded",
					Enumerable.Empty<(EmailSendResult SendResult, MailItem ForwardedEmail)>(),
					EmailForwarderUtilities.SendForwardsEmails(forwardedEmails));
			}
			ObjectFactory.DisposeSubstitutions();
		}

		public void TestSendForwardsEmails_Success()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			var recipients = new List<string> { "recipient1@example.com" };

			var mailSender = new MockIMailSender();
			using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
			{
				var forwardedEmails = EmailForwarderUtilities.CreateForwardsEmails(recipients, mailItem, "test@example.com");

				// act
				var sendResults = EmailForwarderUtilities.SendForwardsEmails(forwardedEmails);

				// assert
				AssertEquals("Should have 1 email total", 1, sendResults.Count());
				AssertEquals("Should have 1 email with successful result", 1, sendResults.Count(e => e.SendResult == EmailSendResult.Successful));
				AssertEquals("Should have 1 email with correct forwards address", 1, sendResults.Count(e => e.ForwardedEmail.MailRecipients.ContainsRecipientWithEmail("recipient1@example.com")));
			}
			ObjectFactory.DisposeSubstitutions();
		}

		public void TestSendForwardsEmails_Failures()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			var recipients = new List<string> { "recipientSuccess@example.com", "recipientFail@example.com" };

			var mailSender = new MockIMailSender();
			mailSender.SendAction = (mItem)
				=> mailSender.RejectedRecipients = mItem.MailRecipients.ContainsRecipientWithEmail("recipientFail@example.com")
					? (new RejectedRecipientInfo[] { new RejectedRecipientInfo { Address = "recipientFail@example.com", ErrorCode = 123, ErrorMessage = "Bad address" } })
					: (Array.Empty<RejectedRecipientInfo>());

			using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
			{
				var forwardedEmails = EmailForwarderUtilities.CreateForwardsEmails(recipients, mailItem, "test@example.com");
				AssertEquals("ErrorReporter shouldn't have pre-existing errors", 0, ErrorReporter.TotalErrorCount);

				// act
				var sendResults = EmailForwarderUtilities.SendForwardsEmails(forwardedEmails);

				// assert
				var (firstSendResult, firstForwardedEmail) = sendResults.First();
				AssertEquals("first email should be successful", EmailSendResult.Successful, firstSendResult);
				Assert("first email should have correct recipient", firstForwardedEmail.MailRecipients.ContainsRecipientWithEmail("recipientSuccess@example.com"));

				var (secondSendResult, secondForwardedEmail) = sendResults.Last();
				AssertEquals("second email should be unsuccessful", EmailSendResult.Unsuccessful, secondSendResult);
				Assert("second email should have correct recipient", secondForwardedEmail.MailRecipients.ContainsRecipientWithEmail("recipientFail@example.com"));

				AssertEquals("ErrorReporter should have 1 error", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Unable to send email, had rejected recipients", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
			ObjectFactory.DisposeSubstitutions();
		}
	}
}
