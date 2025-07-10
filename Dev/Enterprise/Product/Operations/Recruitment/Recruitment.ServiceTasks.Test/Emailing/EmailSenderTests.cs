using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.ServiceTasks.Emailing;
using MailManager;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.ServiceTasks
{
	sealed class EmailSenderTests : TestCase
	{
		public void TestConfiguration()
		{
			var factory = new BusinessObjectFactory();
			var mailSender = new MockIMailSender();
			using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
			{
				var mailItem = factory.NewWithValidTestData<MailItem>();
				mailItem.MI_Direction = DirectionList.Codes.Transmit;

				var sender = new SmtpEmailSender();
				AssertEquals(EmailSendResult.Successful, sender.SendEmail(mailItem));
			}
			ObjectFactory.DisposeSubstitutions();
		}

		public void TestSendEmail_Success()
		{
			var factory = new BusinessObjectFactory();
			var mailSender = new MockIMailSender();
			using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
			{
				var mailItem = factory.NewWithValidTestData<MailItem>();
				mailItem.MI_Direction = DirectionList.Codes.Transmit;

				var sender = new SmtpEmailSender();
				AssertEquals(EmailSendResult.Successful, sender.SendEmail(mailItem));
			}
			ObjectFactory.DisposeSubstitutions();
		}

		public void TestSendEmail_Failure_Exception()
		{
			var factory = new BusinessObjectFactory();
			var mailSender = new MockIMailSender();
			mailSender.SendAction = (mailItem) => throw new FailedToAuthenticateException("test failed to authenticate");

			using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
			{
				var mailItem = factory.NewWithValidTestData<MailItem>();
				mailItem.MI_Direction = DirectionList.Codes.Transmit;

				var sender = new SmtpEmailSender();
				AssertEquals(EmailSendResult.Unsuccessful, sender.SendEmail(mailItem));
				AssertEquals("Unable to send email, exception in SMTP client", ErrorReporter.LastMessageReported);
				AssertEquals("test failed to authenticate", ErrorReporter.LastExceptionReported.Message);
				AssertEquals(typeof(FailedToAuthenticateException), ErrorReporter.LastExceptionReported.GetType());
				ErrorReporter.Clear();
			}
			ObjectFactory.DisposeSubstitutions();
		}

		public void TestSendEmail_Failure_RejectedRecipients()
		{
			var emailAddress = "<someinvalidaddress@@@.com>";
			var factory = new BusinessObjectFactory();
			var mailSender = new MockIMailSender();
			mailSender.SendAction =
				(mailItem) => mailSender.RejectedRecipients = new RejectedRecipientInfo[] { new RejectedRecipientInfo { Address = emailAddress, ErrorCode = 123, ErrorMessage = "Bad address" } };

			using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
			{
				var mailItem = factory.NewWithValidTestData<MailItem>();
				mailItem.RemoveAndDeleteAllMailRecipients();
				mailItem.AddRecipientForUserCommunication(emailAddress);
				mailItem.MI_Direction = DirectionList.Codes.Transmit;

				var sender = new SmtpEmailSender();
				AssertEquals(EmailSendResult.Unsuccessful, sender.SendEmail(mailItem));
				AssertEquals("Unable to send email, had rejected recipients", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
			ObjectFactory.DisposeSubstitutions();
		}
	}
}
