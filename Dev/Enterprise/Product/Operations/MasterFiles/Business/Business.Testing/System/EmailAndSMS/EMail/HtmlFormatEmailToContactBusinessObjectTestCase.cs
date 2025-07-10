using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(HtmlFormatEmailToContactBusinessObject), ExcludePrivate = true)]
	public abstract class HtmlFormatEmailToContactBusinessObjectTestCase<T> : EmailToContactBusinessObjectTestCase<T> where T : HtmlFormatEmailToContactBusinessObject
	{
		protected override void AssertSentEmail()
		{
			AssertEmailSent();
			AssertEquals("Should be HtmlEmailDef", typeof(HtmlEmailDef), Env.OutgoingMailManager.EmailsCreated[0].GetType());

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("ContentType", EmailContentTypes.HTML, email.ContentType);
			AssertEquals("Body.Contains(EmailContactObject.HtmlStyleSheet)", true, email.Body.Contains(SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value));
			AssertEquals("Body.Contains(EmailContactObject.Body)", true, email.Body.Contains(EmailContactObject.Body));

			AssertEquals("email.FromDisplayName", "From Me", email.FromDisplayName);
			AssertEquals("email.FromAddress", "From@Me.com", email.FromAddress);
			AssertEquals("email.Recipients.Count", 2, email.Recipients.Count);
			AssertEquals("email.CCRecipients.Count", 2, email.CCRecipients.Count);
			AssertEquals("email.Recipients[0]", "1@1.com", email.Recipients[0]);
			AssertEquals("email.Recipients[1]", "2@2.com", email.Recipients[1]);
			AssertEquals("email.CCRecipients[1]", "3@3.com", email.CCRecipients[0]);
			AssertEquals("email.CCRecipients[2]", "4@4.com", email.CCRecipients[1]);
			AssertEquals("email.Subject", "This is the subject.", email.Subject);
			AssertContains("email.Body", "This is the body.", email.Body);
			AssertContains("email.Body: Html Tags", "<html", email.Body);
			AssertContains("email.Body: Html Tags", "<body", email.Body);
			AssertContains("email.Body: Html Tags", "</body>", email.Body);
			AssertContains("email.Body: Html Tags", "</html>", email.Body);
			AssertEquals("email.Priority", EmailDef.PriorityFlag.Medium, email.Priority);
		}
	}
}
