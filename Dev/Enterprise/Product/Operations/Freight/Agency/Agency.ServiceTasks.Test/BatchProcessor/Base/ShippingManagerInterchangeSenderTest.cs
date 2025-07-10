using Enterprise.Environment;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class ShippingManagerInterchangeSenderTest : BaseAgencyTest
	{
		#region Implementation
		public void AssertEmailSent(string messagePrefix, string emailAddress, string subject, string filename, string attachmentText)
		{
			EmailDef email = GetSentEmail(emailAddress);
			AssertNotNull(messagePrefix + ": should have sent an email to " + emailAddress, email);
			AssertEquals(messagePrefix + ": email should have the correct subject", subject, email.Subject);
			AssertEquals(messagePrefix + ": email should have an attachment", 1, email.Attachments.Count);
			AssertEquals("No BCCs", 0, email.BCCRecipients.Count);
			string content = System.Text.Encoding.ASCII.GetString(email.Attachments[0].Data);
			AssertEquals(messagePrefix + ": email attachment should have the correct filename", filename, email.Attachments[0].DisplayName);
			AssertMessageEquals(messagePrefix + ": email attachment should contain the message text", attachmentText, content);
		}

		public void AssertEmailSent(string messagePrefix, string emailAddress, string subject, string body)
		{
			EmailDef email = GetSentEmail(emailAddress);
			AssertNotNull(messagePrefix + ": should have sent an email to " + emailAddress, email);
			CombineAssertions(delegate
			{
				AssertEquals(messagePrefix + ": email should have the correct subject", subject, email.Subject);
				AssertMultilineASCIIEquals(messagePrefix + ": email should have the correct body", body, email.Body);
				AssertEquals("No BCCs", 0, email.BCCRecipients.Count);
			});
		}

		public void AssertNoEmailSent(string message, string emailAddress)
		{
			AssertNull(message, GetSentEmail(emailAddress));
		}

		protected static EmailDef GetSentEmail(string emailAddress)
		{
			foreach (EmailDef email in Env.OutgoingMailManager.EmailsCreated)
			{
				if (email.Recipients.Contains(emailAddress))
				{
					return email;
				}
			}

			return null;
		}
		#endregion
	}
}
