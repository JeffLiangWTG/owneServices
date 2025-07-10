using System.Text;
using Enterprise.MailManager.ExternalMailInterface;
using MimeKit;

namespace Enterprise.Recruiter.Module.Testing
{
	public sealed class EmailBuilderForTesting
	{
		public EmailBuilderForTesting()
		{
			bodyBuilder.TextBody = "";
			bodyBuilder.HtmlBody = "";
		}

		public EmailBuilderForTesting Subject(string subject)
		{
			message.Subject = subject;
			return this;
		}

		public EmailBuilderForTesting From(string address)
		{
			return From(null, address);
		}

		public EmailBuilderForTesting From(string name, string address)
		{
			message.From.Add(new MailboxAddress(name, address));
			return this;
		}

		public EmailBuilderForTesting WithAttachment(string fileName)
		{
			bodyBuilder.Attachments.Add(fileName);
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public string GetEmail()
		{
			var data = message.GetData();
			return Encoding.UTF8.GetString(data) ?? string.Empty;
		}

		readonly MimeMessage message = new MimeMessage();
		readonly BodyBuilder bodyBuilder = new BodyBuilder();
	}
}
