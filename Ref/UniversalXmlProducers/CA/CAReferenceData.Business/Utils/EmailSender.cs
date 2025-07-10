using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mail;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.CAReferenceData.Business
{
	public static class EmailSender
	{
		public static void SendEmail(string functionCode, string message)
		{
			var email = Email.GenerateCommonEmail(functionCode, message);
			if (UnitTestDetector.IsRunningTests.Value)
			{
				SentEmails.Add(email);
			}
			else
			{
				SendEmail(email);
			}
		}

		public static void SendEmail(Email email)
		{
			using (var client = new SmtpClient(ApplicationConfig.EmailSmtpServer)
			{
				Credentials = new NetworkCredential(ApplicationConfig.EmailUsername, ApplicationConfig.EmailCredentialsPassword),
				Port = ApplicationConfig.EmailSmtpPort
			})
			{
				using (var message = new MailMessage(email.From, email.To))
				{
					message.Subject = email.Subject;
					message.Body = email.Body;

					client.Send(message);
				}
			}
		}

		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static List<Email> SentEmails = new List<Email>();
	}

	public class Email
	{
		public string From { get; set; }
		public string To { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public static Email GenerateCommonEmail(string functionCode, string message) => new Email
		{
			From = ApplicationConfig.EmailSender,
			To = ApplicationConfig.EmailRecipients,
			Subject = $"[CAReferenceData {functionCode}] Notification {DateTime.UtcNow:s}",
			Body = message
		};
	}
}
