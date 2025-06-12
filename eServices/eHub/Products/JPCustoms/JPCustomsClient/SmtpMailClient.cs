using System;
using System.IO;
using System.Net.Mail;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public class SmtpMailClient : MailClient
	{
		readonly ISmtpMailClientConfiguration configuration;

		public SmtpMailClient(ISmtpMailClientConfiguration configuration)
			: base(configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			this.configuration = configuration;
		}

		public void Send(Stream message)
		{
			Open();
			Helo(configuration.DomainName);
			MailFrom(configuration.EmailFrom);
			RcptTo(configuration.EmailTo);
			Data();
			SendMessage(message);
			Quit();
		}

	    public bool Ping()
	    {
	        Open();
	        Helo(configuration.DomainName);
	        Quit();
	        return true;
	    }

		void SendMessage(Stream message)
		{
			SendMesageLineByLine(message);

			const string commandText = ".";
			configuration.Logger.Debug(commandText);

			Write(commandText);
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new SmtpException("SMTP server did not respond to End Data Command request");
			if (!text.StartsWith("250")) throw new SmtpException("SMTP server End Data Command respond error: " + text);
		}

		void SendMesageLineByLine(Stream message)
		{
			var reader = new StreamReader(message);
			string line;

			while ((line = reader.ReadLine()) != null)
			{
				if (line.StartsWith(".")) line = "." + line;

				configuration.Logger.Debug(line);
				Write(line);
			}
		}

		void Open()
		{
			var text = Read();
			configuration.Logger.Debug(text);
			if (text == null) throw new SmtpException("SMTP server did not respond to connection request");
			if (!text.StartsWith("220")) throw new SmtpException("SMTP server connection respond error: " + text);
		}

		void Helo(string domainName)
		{
			string commandText = string.Format("HELO {0}", domainName);
			configuration.Logger.Debug(commandText);

			Write(commandText);
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new SmtpException("SMTP server did not respond to HELO request");
			if (!text.StartsWith("250")) throw new SmtpException("SMTP server HELO respond error: " + text);
		}

		void MailFrom(string email)
		{
			string commandText = string.Format("MAIL FROM: {0}", email);
			configuration.Logger.Debug(commandText);

			Write(commandText);
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new SmtpException("SMTP server did not respond to MAIL FROM request");
			if (!text.StartsWith("250")) throw new SmtpException("SMTP server MAIL FROM respond error: " + text);
		}

		void RcptTo(string email)
		{
			string commandText = string.Format("RCPT TO: {0}", email);
			configuration.Logger.Debug(commandText);

			Write(commandText);
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new SmtpException("SMTP server did not respond to RCPT TO request");
			if (!text.StartsWith("250")) throw new SmtpException("SMTP server RCPT TO respond error: " + text);
		}

		void Data()
		{
			const string commandText = "DATA";
			configuration.Logger.Debug(commandText);

			Write(commandText);
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new SmtpException("SMTP server did not respond to DATA request");
			if (!text.StartsWith("354")) throw new SmtpException("SMTP server DATA respond error: " + text);
		}

		void Quit()
		{
			const string commandText = "QUIT";
			configuration.Logger.Debug(commandText);

			Write("QUIT");
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new SmtpException("SMTP server did not respond to QUIT request");
			if (!text.StartsWith("221")) throw new SmtpException("SMTP server QUIT respond error: " + text);
		}
	}
}
