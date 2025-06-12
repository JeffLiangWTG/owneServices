using System;
using System.IO;
using CargoWise.eHub.Products.JPCustoms.Common;

namespace CargoWise.eHub.Products.JPCustoms.Client
{
	public class Pop3MailClient : MailClient
	{
		readonly IPop3MailClientConfiguration configuration;
		Mailbox mailbox;

		public Pop3MailClient(IPop3MailClientConfiguration configuration)
			: base(configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");
			this.configuration = configuration;
		}

		public void Connect()
		{
			Open();
			UserName(configuration.UserName);
			Password(configuration.Password, configuration.UserName);
			mailbox = new Mailbox(Stat());
		}

		public string GetNextMessage()
		{
			mailbox.PullMessageIndex();
			Retr(mailbox.CurrentIndex);
			return RetriveMessage();
		}

		public void Delete()
		{
			Dele(mailbox.CurrentIndex);
		}

		public void Diconnect()
		{
			Quit();
		}

		public int MessageCount
		{
			get
			{
				return mailbox.MessageCount;
			}
		}

		#region Implementation

		void Open()
		{
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new Pop3Exception("POP3 server did not respond to connection request");
			if (!text.StartsWith("+OK")) throw new Pop3Exception("POP3 server connection respond error: " + text);
		}

		void UserName(string userName)
		{
			string commandText = string.Format("USER {0}", userName);
			configuration.Logger.Debug(commandText);

			Write(commandText);
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new Pop3Exception("POP3 server did not respond to HELO request");
			if (!text.StartsWith("+OK")) throw new Pop3Exception("POP3 server USER respond error: " + text);
		}

		void Password(string password, string userName)
		{
			configuration.Logger.Debug("PASS **********");

			Write(string.Format("PASS {0}", password));
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new Pop3Exception("POP3 server did not respond to PASS request");
			if (text.StartsWith("+OK")) return;

			if (text.StartsWith(string.Format("-ERR .{0} lock busy!  Is another session active?", userName)))
			{
				throw new MailboxLockedByAnotherClientException(text);
			}

			throw new Pop3Exception("POP3 server PASS respond error: " + text);
		}

		int Stat()
		{
			const string commandText = "STAT";
			configuration.Logger.Debug(commandText);

			Write(commandText);
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new Pop3Exception("POP3 server did not respond to STAT request");
			if (!text.StartsWith("+OK")) throw new Pop3Exception("POP3 server STAT respond error: " + text);
			var parts = text.Split(' ');
			if (parts.Length != 3) throw new Pop3Exception("POP3 server STAT respond error. Expected (+OK MsgNum Size). Actual: " + text);

			int messageNumber;
			if (!Int32.TryParse(parts[1], out messageNumber)) throw new Pop3Exception("POP3 server STAT respond error. Expected (+OK MsgNum Size) and MsgNum should be interger. Actual: " + text);

			return messageNumber;
		}

		void Retr(int index)
		{
			string commandText = string.Format("RETR {0}", index);
			Write(commandText);
			configuration.Logger.Debug(commandText);

			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new Pop3Exception("POP3 server did not respond to RETR request");
			if (!text.StartsWith("+OK")) throw new Pop3Exception("POP3 server RETR respond error: " + text);
		}

		void Dele(int index)
		{
			string commandText = string.Format("DELE {0}", index);
			configuration.Logger.Debug(commandText);

			Write(commandText);
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new Pop3Exception("POP3 server did not respond to DELE request");
			if (!text.StartsWith("+OK")) throw new Pop3Exception("POP3 server DELE respond error: " + text);
		}

		string RetriveMessage()
		{
			var messageStream = new MemoryStream();
			var writer = new StreamWriter(messageStream) { AutoFlush = true };

			int lineNumber = 0;
			string line;
			bool firstLine = true;
			while ((line = Read()) != ".")
			{
				lineNumber++;
				if (lineNumber <= configuration.HeaderLineNumberToSkip) continue;
				if (firstLine) firstLine = false;
				else writer.Write("\r\n");

				WriteLine(writer, line);
			}

			messageStream.Position = 0;
			return new StreamReader(messageStream).ReadToEnd();
		}

		void WriteLine(StreamWriter writer, string line)
		{
			configuration.Logger.Debug(line);
			if (line.StartsWith("..")) line = line.Substring(1, line.Length - 1);
			writer.Write(line);
		}

		void Quit()
		{
			const string commandText = "QUIT";
			configuration.Logger.Debug(commandText);

			Write(commandText);
			var text = Read();
			configuration.Logger.Debug(text);

			if (text == null) throw new Pop3Exception("POP3 server did not respond to QUIT request");
			if (!text.StartsWith("+OK")) throw new Pop3Exception("POP3 server QUIT respond error: " + text);
		}

		#endregion
	}
}
