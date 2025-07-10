using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.MessageProcessors.Testing
{
	public class EDIFACTMessageProcessorTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		protected void AssertSyntaxErrorEmail(string expectedSubject, string expectedBody, string messageText, string syntaxErrorText,
			string recipient = "UserToNotify@blah.com", string ccRecipient = "blah@blah.com")
		{
			var email = AssertEmailInternal(expectedSubject, expectedBody, recipient, ccRecipient, jobReference: string.Empty, expectNoRecipiants: false);
			AssertEquals("Contain Attachments", true, email.Attachments.Count > 0);
			AssertEquals("Attachment DisplayName", "Message.txt", email.Attachments[email.Attachments.Count - 2].DisplayName);
			AssertMultilineASCIIEquals("Attachment Data", messageText.Replace("'", "\r\n"), Encoding.ASCII.GetString(email.Attachments[email.Attachments.Count - 2].Data));

			AssertEquals("Attachment DisplayName", "SourceMessageWithErrorMarks.txt", email.Attachments[email.Attachments.Count - 1].DisplayName);
			AssertMultilineASCIIEquals("Attachment Data", syntaxErrorText, Encoding.ASCII.GetString(email.Attachments[email.Attachments.Count - 1].Data));
		}

		protected void AssertEmail(string expectedSubject, string expectedBodyContaining, string messageText,
			string recipient = "UserToNotify@blah.com", string ccRecipient = "blah@blah.com", string jobReferece = "", bool expectNoRecipiants = false)
		{
			var email = AssertEmailInternal(expectedSubject, expectedBodyContaining, recipient, ccRecipient, jobReferece, expectNoRecipiants);
			if (!string.IsNullOrEmpty(messageText))
			{
				AssertEquals("Contain Attachments", true, email.Attachments.Count > 0);
				AssertEquals("Attachment DisplayName", "Message.txt", email.Attachments[email.Attachments.Count - 1].DisplayName);
				AssertMultilineASCIIEquals("Attachment Data", messageText.Replace("'", "\r\n"), Encoding.ASCII.GetString(email.Attachments[email.Attachments.Count - 1].Data));
			}
			else
			{
				AssertEquals("Contain Attachments", 2, email.Attachments.Count);
			}
		}

		EmailDef AssertEmailInternal(string expectedSubject, string expectedBodyContaining, string recipient, string ccRecipient, string jobReference, bool expectNoRecipiants)
		{
			var email = mailManager.EmailsCreated.Find(emailToMatched => string.IsNullOrEmpty(jobReference) ? emailToMatched.Subject == expectedSubject : emailToMatched.Subject == expectedSubject && emailToMatched.Body.Contains(jobReference));
			AssertNotNull(string.Format("Email with '{0}' subject should be sent", expectedSubject), email);
			AssertContains("Email body", expectedBodyContaining, email.Body);
			AssertRecipients(email, recipient, ccRecipient, expectNoRecipiants);
			return email;
		}

		static void AssertRecipients(EmailDef email, string recipient, string ccRecipient, bool expectNoRecipiants)
		{
			Assert("Should not be any recipiants", !expectNoRecipiants || email.Recipients.Count == 0);

			AssertRecipients(recipient, email.Recipients);
			AssertRecipients(ccRecipient, email.CCRecipients, "CC");
		}

		static void AssertRecipients(string expectedRecipient, RecipientDefReadonlyCollection actualRecipients, string messagePrefix = "")
		{
			if (!string.IsNullOrEmpty(expectedRecipient))
			{
				var recipients = expectedRecipient.Split(';');
				AssertEquals(messagePrefix + "Recipients Count", recipients.Length, actualRecipients.Count);
				var recipientEmails = actualRecipients.OfType<RecipientDef>().Select(r => r.Email);
				foreach (var expectedEmail in recipients)
				{
					AssertCollectionContains(messagePrefix + "Recipient Email", expectedEmail, recipientEmails);
				}
			}
		}

		protected T GetEDIMessage<T>(ZString messageText) where T : EDIMessage
		{
			var message = Factory.New<T>();
			message.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
			var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var staff = group.Staff.AddNew();
			staff.GS_FullName = "blah";
			staff.GS_EmailAddress = "blah@blah.com";
			staff.GS_Code = "ZAC";

			userToNotify = Factory.NewWithValidTestData<GlbStaff>();
			userToNotify.GS_FullName = "UserToNotify";
			userToNotify.GS_EmailAddress = "UserToNotify@blah.com";
			userToNotify.GS_Code = "UTN";
			Factory.Save();

			mailManager = Env.OutgoingMailManager;
		}

		protected LoggingInformation logger;
		protected GlbStaff userToNotify;
		protected IOutgoingMailManager mailManager;
	}
}
