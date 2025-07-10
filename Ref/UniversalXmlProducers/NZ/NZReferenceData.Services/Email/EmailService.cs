using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;

namespace CargoWise.RefDbRepo.NZReferenceData.Services
{
	public class EmailService : IEmailService
	{
		private static readonly string SmtpServer = ApplicationConfig.EmailSmtpServer;
		private static readonly int SmtpPort = int.Parse(ApplicationConfig.EmailSmtpPort, CultureInfo.CurrentCulture);
		private static readonly string SmtpUser = ApplicationConfig.EmailUsername;
		private static readonly string SmtpPassword = ApplicationConfig.EmailCredentialsPassword;
		private static readonly string EmailSender = ApplicationConfig.EmailSender;

		private static IEnumerable<string> _parsedEmailGroups;
		private static IEnumerable<string> ParsedEmailGroups => _parsedEmailGroups ??= ParseEmailGroups();
		private static string EmailGroups => ApplicationConfig.EmailGroups;

		private static List<string> ParseEmailGroups()
		{
			return EmailGroups?.Split(';')
				.Select(email => email.Trim())
				.Where(email => !string.IsNullOrWhiteSpace(email))
				.ToList() ?? [];
		}

		public void SendEmail(string subject, string body, bool isHtmlBody)
		{
			try
			{
				if (!ParsedEmailGroups.Any())
				{
					Console.WriteLine("No email groups configured. Skipping email send.");
					return;
				}

				using var smtpClient = new SmtpClient(SmtpServer, SmtpPort);
				smtpClient.Credentials = new System.Net.NetworkCredential(SmtpUser, SmtpPassword);
				smtpClient.EnableSsl = true;

				using var mailMessage = new MailMessage();
				mailMessage.From = new MailAddress(EmailSender);
				mailMessage.Subject = subject;
				mailMessage.Body = body;
				mailMessage.IsBodyHtml = isHtmlBody;

				foreach (var email in ParsedEmailGroups)
				{
					mailMessage.To.Add(email);
				}

				smtpClient.Send(mailMessage);
				Console.WriteLine($"Email sent successfully to {string.Join(", ", ParsedEmailGroups)}");
			}
			catch (SmtpException smtpEx)
			{
				Console.WriteLine($"Failed to send email: {smtpEx.Message}");
			}
		}
	}
}
