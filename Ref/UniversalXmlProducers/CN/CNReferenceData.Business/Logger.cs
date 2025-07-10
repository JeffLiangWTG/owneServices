using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using CargoWise.RefDbRepo.CNReferenceData.Services;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class Logger : ILog
	{
		public Logger(LogLevel lowestLevel)
		{
			this.lowestLevel = lowestLevel;
		}
		internal readonly LogLevel lowestLevel;

		static string LogFormat => $"[{GlobalOption.Instance.Now:yy-MM-dd HH:mm:ss} {{0}}]:";

		public string ProgramName { get; set; }

		public void Debug(object message)
		{
			Log(LogLevel.Debug, message.ToString());
		}

		public void Info(object message)
		{
			Log(LogLevel.Info, message.ToString());
		}

		public void Warning(object message)
		{
			Log(LogLevel.Warn, message.ToString());
		}

		public void Error(object message)
		{
			Log(LogLevel.Error, message.ToString());
		}

		public void Error(string message, Exception ex)
		{
			if (ex is AggregateException aggregateException)
			{
				foreach (var innerException in aggregateException.InnerExceptions)
				{
					Error(message, innerException);
				}
			}
			else
			{
				Log(LogLevel.Error, $"{message}: {ex.Message}\r\n{ex.StackTrace}");

				if (ex.InnerException != null)
				{
					Error($"{message}:InnerException", ex.InnerException);
				}
			}
		}

		void Log(LogLevel level, string message)
		{
			switch (level)
			{
				case LogLevel.Debug:
					Console.ForegroundColor = ConsoleColor.Blue;
					break;
				case LogLevel.Info:
					Console.ForegroundColor = ConsoleColor.Gray;
					break;
				case LogLevel.Warn:
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case LogLevel.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
			}

			string formattedMessage = string.Format(CultureInfo.InvariantCulture, LogFormat, level) + message;
			messages.Add((level, formattedMessage));

			if (level == LogLevel.Error)
			{
				Console.Error.WriteLine(formattedMessage);
			}
			else
			{
				Console.WriteLine(formattedMessage);
			}
		}

		public void Dispose() => DisposeCore();

		protected virtual void DisposeCore()
		{
			string errors = Errors;

			if (!string.IsNullOrEmpty(errors))
			{
				throw new InvalidOperationException($"Errors occurred while running {ProgramName}.", new InvalidOperationException(errors));
			}
		}

		internal readonly List<(LogLevel, string)> messages = new List<(LogLevel, string)>();

		public string Errors => string.Join(Environment.NewLine, messages.Where(m => m.Item1 >= LogLevel.Error).Select(m => m.Item2).ToArray());

		public string All => string.Join(Environment.NewLine, messages.Select(m => m.Item2).ToArray());

		public enum LogLevel
		{
			Debug, Info, Warn, Error
		}
	}

	public class FileLogger : Logger
	{
		public FileLogger(string filename, LogLevel lowestLevel) : base(lowestLevel)
		{
			this.filename = filename;
		}
		readonly string filename;

		public void SaveToFile()
		{
			if (!string.IsNullOrEmpty(filename))
			{
				File.AppendAllLines(filename, messages.Where(m => m.Item1 >= lowestLevel).Select(m => m.Item2));
			}
			messages.Clear();
		}

		protected override void DisposeCore()
		{
			string errors = Errors;

			SaveToFile();

			if (!string.IsNullOrEmpty(errors))
			{
				SendEmail(errors, true);
				throw new InvalidOperationException(errors);
			}
		}

		public static void SendEmail(string body, bool isForErrors = false)
		{
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				var host = GlobalOption.Instance.Setting.SmtpClientHost;
				var recipient = GlobalOption.Instance.Setting.RecipientAddress;
				var address = GlobalOption.Instance.Setting.EmailAddress;
				var password = GlobalOption.Instance.Setting.EmailPassword;

				if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(recipient) && !string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(body))
				{
					using (var message = new MailMessage(address, recipient, "Tariff Producer Notification" + (isForErrors ? " Error" : ""), body)
					{
						Priority = isForErrors ? MailPriority.High : MailPriority.Normal
					})
					using (var client = new SmtpClient
					{
						Host = host,
						EnableSsl = true,
						UseDefaultCredentials = false,
						Credentials = new NetworkCredential(address, password)
					})
					{
						client.Send(message);
					}
				}
			}
			catch
			{
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		public void SendEmail()
		{
			SendEmail(All);
		}
	}
}
