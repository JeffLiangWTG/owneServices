using Enterprise.MailManager.ExternalMailInterface;
using MimeKit;

namespace Enterprise.Recruiter.ServiceTasks.Testing
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

		public EmailBuilderForTesting To(string name, string address)
		{
			message.To.Add(new MailboxAddress(name, address));
			return this;
		}

		public EmailBuilderForTesting Cc(string name, string address)
		{
			message.Cc.Add(new MailboxAddress(name, address));
			return this;
		}

		public EmailBuilderForTesting Bcc(string name, string address)
		{
			message.Bcc.Add(new MailboxAddress(name, address));
			return this;
		}

		public EmailBuilderForTesting Body(string bodyText)
		{
			bodyBuilder.TextBody = bodyText;
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public EmailBuilderForTesting HtmlBody(string bodyText)
		{
			bodyBuilder.HtmlBody = bodyText;
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public EmailBuilderForTesting WithAttachment(string fileName)
		{
			bodyBuilder.Attachments.Add(fileName);
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public EmailBuilderForTesting WithAttachment(string fileName, byte[] data)
		{
			bodyBuilder.Attachments.Add(fileName, data);
			message.Body = bodyBuilder.ToMessageBody();
			return this;
		}

		public HRServiceEmail GetEmail()
		{
			return new HRServiceEmail(message.GetData());
		}

		public MimeMessage Message
		{
			get { return message; }
		}

		readonly MimeMessage message = new MimeMessage();
		readonly BodyBuilder bodyBuilder = new BodyBuilder();
	}
}
