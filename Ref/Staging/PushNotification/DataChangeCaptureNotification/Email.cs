using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.Staging.DataChangeCaptureNotification
{
	public abstract class Email<T>
	{
		readonly string _smtpServer;
		readonly ICredentialsByHost _credentials;
		readonly int _smtpPort;
		readonly string _system;

		protected Email(string smtpServer, ICredentialsByHost credentials, int smtpPort)
		{
			Argument.NotNullOrEmpty(smtpServer, nameof(smtpServer));
			Argument.InRangeWithBoundIncluded(smtpPort, 0, 65535, nameof(smtpPort));

			_smtpServer = smtpServer;
			_credentials = credentials;
			_smtpPort = smtpPort;

			_system = ApplicationConfig.System;
		}

		public virtual void Send(IEnumerable<T> data)
		{
			using (var client = new SmtpClient(_smtpServer) { Port = _smtpPort })
			{
				foreach (var emailAddress in GetContacts(data))
				{
					if (EmailValidator.IsValidEmail(emailAddress))
					{
						var emailFrom = "donotreply_refservice@wisetechglobal.com";
						using (var email = new MailMessage(emailFrom, emailAddress))
						{
							if (_credentials != null)
							{
								client.Credentials = _credentials;
							}
							email.IsBodyHtml = true;

							email.Subject = $"Error notification ({typeof(T).Name}) {DateTime.UtcNow:s} from {_system}";
							email.Body = ApplyTemplate(data);
							client.Send(email);
						}
					}
				}
			}
		}

		protected abstract string ApplyTemplate(IEnumerable<T> data);

		public virtual IEnumerable<string> GetContacts(IEnumerable<T> data)
		{
			var email = ApplicationConfig.Email;
			var emailsToSend = email.Split(';');
			return emailsToSend;
		}

		protected const string DefaultMessage = "Found errors in the registers below:";
	}
}
