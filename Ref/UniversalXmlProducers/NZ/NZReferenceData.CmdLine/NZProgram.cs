using System;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.NZReferenceData.Business;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using Microsoft.VisualBasic;

namespace CargoWise.RefDbRepo.NZReferenceData.CmdLine
{
	public abstract class NZProgram
	{
		public abstract string ProgramName { get; }

		protected ILogger Logger { get; set; } = new Logger();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
		public void Run()
		{
			try
			{
				RunCore();
			}
			catch (Exception ex)
			{
				var messageId = AddSendToEmailMessage($"Exception throwed during running program {ProgramName}", ex);
				Logger.LogError($"Exception throwed during running program, relevent message: {messageId}, exception message: {ex.Message}");
			}

			if (SendToEmailMessages.Count > 0)
			{
				SendEmail(Logger);
			}
		}

		protected abstract void RunCore();

		System.Collections.Generic.List<(Guid Id, string Message, Exception ReleventException)> SendToEmailMessages { get; } = new System.Collections.Generic.List<(Guid, string, Exception)>();

		protected Guid AddSendToEmailMessage(string message, Exception releventException)
		{
			var id = Guid.NewGuid();
			SendToEmailMessages.Add((id, message, releventException));
			return id;
		}

		void SendEmail(ILogger logger)
		{
			string ToHTMLLines(string input)
			{
				return input.Replace(" ", "&nbsp;")
					.Replace("\t", "&#09;")
					.Replace(Environment.NewLine, "<br/>");
			}

			void BuildException(StringBuilder builder, Exception ex)
			{
				if (ex == null)
				{
					builder.AppendLine("<tr><td colspan=\"2\">No Relevent Exception</td></tr>");
				}
				else
				{
					builder.AppendLine(CultureInfo.InvariantCulture, $"<tr><td>Exception Message</td><td>{ToHTMLLines(ex.Message)}</td></tr>")
						.AppendLine(CultureInfo.InvariantCulture, $"<tr><td>Exception Stack</td><td>{ToHTMLLines(ex.StackTrace)}</td></tr>");

					var innerException = ex.InnerException;
					while (innerException != null)
					{
						builder.AppendLine(CultureInfo.InvariantCulture, $"<tr><td>InnerException Message</td><td>{ToHTMLLines(innerException.Message)}</td></tr>")
							.AppendLine(CultureInfo.InvariantCulture, $"<tr><td>InnerException Stack</td><td>{ToHTMLLines(innerException.StackTrace)}</td></tr>");
						innerException = innerException.InnerException;
					}
				}
			}

			var bodyBuilder = new StringBuilder("<html><head><style>table,th,td{border:1px solid black;border-collapse:collapse;}</style></head><body>");

			if (SendToEmailMessages.Count > 0)
			{
				bodyBuilder.AppendLine(CultureInfo.InvariantCulture, $"<h1>Messages and Exceptions</h1>");
				foreach (var message in SendToEmailMessages)
				{
					bodyBuilder.AppendLine("<table>");
					bodyBuilder.AppendLine(CultureInfo.InvariantCulture, $"<tr><td>ID</td><td>{message.Id}</td></tr>");
					bodyBuilder.AppendLine(CultureInfo.InvariantCulture, $"<tr><td>Message</td><td>{ToHTMLLines(message.Message)}</td></tr>");
					BuildException(bodyBuilder, message.ReleventException);
					bodyBuilder.AppendLine("</table><br/>");
				}
			}

			bodyBuilder.AppendLine("<h1>Logs</h1><table><thead><tr><th>Log Time</th><th>Log Kind</th><th>Log Message</th></tr></thead><tbody>");
			foreach (var log in logger.Logs)
			{
				bodyBuilder.AppendLine(CultureInfo.InvariantCulture, $"<tr><td>{log.Time:u}</td><td>{log.Kind}</td><td>{ToHTMLLines(log.Mesage)}</td></tr>");
			}
			bodyBuilder.AppendLine("</tbody></table>");

			bodyBuilder.AppendLine("</body></html>");

			GetEmailService().SendEmail($"Reference Data Failure: {ProgramName}", bodyBuilder.ToString(), true);
		}

		protected virtual IEmailService GetEmailService() => new EmailService();
	}
}
