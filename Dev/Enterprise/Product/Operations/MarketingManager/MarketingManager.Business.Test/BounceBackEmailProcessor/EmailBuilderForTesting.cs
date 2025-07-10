using CargoWise.EntityFramework;
using Enterprise.MailManager.ExternalMailInterface;
using MimeKit;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class EmailBuilderForTesting
	{
		public EmailBuilderForTesting()
		{
			bodyBuilder.TextBody = string.Empty;
			bodyBuilder.HtmlBody = string.Empty;
			message.Body = bodyBuilder.ToMessageBody();
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

		public EmailBuilderForTesting CampaignItemID(string campaignItemPk)
		{
			message.Headers.Add("X-BusinessEntityID", campaignItemPk);
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

		public BounceBackEmailDetails GetNewBounceDetails(BusinessObjectFactory factory)
		{
			return new BounceBackEmailDetails(factory, message.GetData());
		}

		internal MimeMessage Message
		{
			get { return message; }
		}

		readonly MimeMessage message = new MimeMessage();
		readonly BodyBuilder bodyBuilder = new BodyBuilder();
	}
}
