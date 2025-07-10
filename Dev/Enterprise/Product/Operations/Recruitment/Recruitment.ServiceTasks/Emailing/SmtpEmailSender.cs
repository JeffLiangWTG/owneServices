using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruitment.Registry;

namespace Enterprise.Recruitment.ServiceTasks.Emailing
{
	public class SmtpEmailSender
	{
		public SmtpEmailSender()
		{ }

		public SmtpConfiguration SmtpConfig
		{
			get
			{
				if (smtpConfig == null)
				{
					smtpConfig = new SmtpConfiguration(
						RecruitmentDataRegistry.Instance.MiddleMan_SMTPServerAddress.Value,
						RecruitmentDataRegistry.Instance.MiddleMan_SMTPServerPort.Value,
						RecruitmentDataRegistry.Instance.MiddleMan_SMTPSecureConnection.Value);
				}
				return smtpConfig;
			}
			set { smtpConfig = value; }
		}
		SmtpConfiguration smtpConfig;

		public UserPasswordAuthConfiguration AuthConfig
		{
			get
			{
				if (authConfig == null)
				{
					authConfig = new UserPasswordAuthConfiguration(
						RecruitmentDataRegistry.Instance.MiddleMan_SMTPServerUsername.Value,
						RecruitmentDataRegistry.Instance.MiddleMan_SMTPServerPassword.Value);
				}
				return authConfig;
			}
			set { authConfig = value; }
		}

		UserPasswordAuthConfiguration authConfig;

		public string SenderAddress
		{
			get
			{
				if (senderAddress == null)
				{
					senderAddress = RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value;
				}
				return senderAddress;
			}
			set { senderAddress = value; }
		}

		string senderAddress;

		public IEnumerable<(EmailSendResult SendResult, MailItem ForwardedEmail)> SendEmails(IEnumerable<MailItem> emails)
		{
			_ = Argument.NotNull(emails, nameof(emails));

			foreach (var email in emails)
			{
				var sender = ObjectFactory.Get<IMailSenderProvider>().GetSmtpSender(SmtpConfig, AuthConfig, SenderAddress);
				yield return (SendEmail(sender, email), email);
			}
		}

#if DEBUG
		public EmailSendResult SendEmail(MailItem mailItem)
		{
			var sender = ObjectFactory.Get<IMailSenderProvider>().GetSmtpSender(SmtpConfig, AuthConfig, SenderAddress);
			return SendEmail(sender, mailItem);
		}
#endif

		public EmailSendResult SendEmail(ISmtpSender sender, MailItem mailItem)
		{
			_ = Argument.NotNull(sender, nameof(sender));

			try
			{
				sender.Send(mailItem);
			}
			// see MailKitMailSender.cs::SendMimeMessage() for exceptions
			catch (MailInterfaceException e)
			{
				ErrorReporter.ReportOnce("Unable to send email, exception in SMTP client", e);
				return EmailSendResult.Unsuccessful;
			}

			var rejectedRecipients = sender.GetRejectedRecipients();
			if (rejectedRecipients != null && rejectedRecipients.Any(e => mailItem.AllRecipients.Contains(e.Address)))
			{
				ErrorReporter.ReportOnce("Unable to send email, had rejected recipients");
				return EmailSendResult.Unsuccessful;
			}

			return EmailSendResult.Successful;
		}
	}
}
