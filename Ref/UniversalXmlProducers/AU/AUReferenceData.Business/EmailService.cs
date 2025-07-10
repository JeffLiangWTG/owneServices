using System;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.ServiceModel.Channels;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public static class EmailService
	{
		public static void SendEmail(string emailGroup, string subject, string body)
		{
			try
			{
				var smtpServer = ApplicationConfig.EmailSmtpServer;
				var smtpPort = int.Parse(ApplicationConfig.EmailSmtpPort, CultureInfo.CurrentCulture);
				var smtpUser = ApplicationConfig.EmailUsername;
				var smtpPassword = ApplicationConfig.EmailCredentialsPassword;
				var emailSender = ApplicationConfig.EmailSender;

				using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
				{
					smtpClient.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
					smtpClient.EnableSsl = true;

					using (var mailMessage = new MailMessage())
					{
						mailMessage.From = new MailAddress(emailSender);
						mailMessage.Subject = subject;
						mailMessage.Body = body;
						mailMessage.IsBodyHtml = false;

						foreach (var email in emailGroup.Split(';'))
						{
							if (!string.IsNullOrWhiteSpace(email))
							{
								mailMessage.To.Add(email.Trim());
							}
						}

						smtpClient.Send(mailMessage);
						Console.WriteLine($"Email sent successfully to {string.Join(", ", emailGroup)}");
					}
				}
			}
			catch (SmtpException smtpEx)
			{
				Console.WriteLine($"Failed to send email: {smtpEx.Message}");
			}
		}
	}
}
