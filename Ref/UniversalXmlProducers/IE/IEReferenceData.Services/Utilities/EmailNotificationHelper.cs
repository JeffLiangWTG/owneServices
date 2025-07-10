using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.IEReferenceData.Services
{
	public static class EmailNotificationHelper
	{
		[SuppressMessage("Design", "CA1031")]
		public async static Task SendWithRetries(EmailMessage emailMessage)
		{
			if (UnitTestDetector.IsRunningTests.Value)
			{
				SentEmails.Add(emailMessage);
				return;
			}

			using (var mailMessage = emailMessage.ToMailMessage())
			using (var smtpClient = NewSmtpClient())
			{
				while (emailMessage.RemainingTries-- > 0)
				{
					try
					{
						await smtpClient.SendMailAsync(mailMessage);
						return;
					}
					catch (Exception ex)
					{
						if (emailMessage.RemainingTries > 0)
						{
							await Task.Delay(TimeSpan.FromSeconds(ApplicationConfig.Instance.EmailSendingRetryDelaySeconds));
							Console.WriteLine(ex.Message);
							if(ex.InnerException != null)
							{
								Console.WriteLine(ex.InnerException.Message);
							}
						}
						else
						{
							Console.Error.WriteLine(ex.Message);
							if (ex.InnerException != null)
							{
								Console.Error.WriteLine(ex.InnerException.Message);
							}
						}
					}
				}
			}
		}

		public static List<EmailMessage> SentEmails { get; } = new List<EmailMessage>();

		static SmtpClient NewSmtpClient() => new SmtpClient(ApplicationConfig.Instance.EmailSmtpServer, ApplicationConfig.Instance.EmailSmtpPort)
		{
			Credentials = new NetworkCredential(ApplicationConfig.Instance.EmailCredentialsUserName, ApplicationConfig.Instance.EmailCredentialsPassword)
		};
	}


	public class EmailMessage
	{
		public string From { get; set; } = ApplicationConfig.Instance.EmailSender;
		public string[] To { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public int RemainingTries { get; set; } = ApplicationConfig.Instance.EmailSendingTries;

		public MailMessage ToMailMessage()
		{
			var mailMessage = new MailMessage
			{
				From = new MailAddress(From),
				Subject = Subject,
				Body = Body,
				IsBodyHtml = false
			};
			foreach (var to in To.Cast<string>())
			{
				mailMessage.To.Add(to);
			}
			return mailMessage;
		}
	}
}
