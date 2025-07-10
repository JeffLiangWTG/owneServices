using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;

namespace Enterprise.Recruiter.ServiceTasks.Testing
{
	public sealed class EmailReaderForTest : HRServiceEmailReader
	{
		public EmailReaderForTest(string mailProtocol, string mailServer, int mailPort, string mailUsername, string mailPassword, string securityType)
			: base(mailProtocol, mailServer, mailPort, mailUsername, mailPassword, securityType)
		{
		}

		public IMailProtocol Protocol_Expoxed => protocol;

		public override long GetCount()
		{
			if (debugEmails != null)
			{
				return debugEmails.Count;
			}
			return -1;
		}

		public bool ThrowServerException;
		public bool ThrowServerExceptionOnSecondEmail;
		public bool ThrowSocketException;

		const string MailKitSource = "MailKit";

		public override string GetStringEmailFromPosition(long index)
		{
			if (ThrowServerException)
			{
				throw new InvalidOperationException("Message unavailable") { Source = MailKitSource };
			}

			if (ThrowServerExceptionOnSecondEmail && index == 2)
			{
				throw new InvalidOperationException("Server disconnected") { Source = MailKitSource };
			}

			if (ThrowSocketException)
			{
				throw new Exception("Error", new System.IO.IOException("Error", new SocketException(10053))) { Source = MailKitSource };
			}

			if (debugStringEmails != null)
			{
				return debugStringEmails[(int)(index - 1)];
			}
			return "";
		}

		public override HRServiceEmail GetRawEmailFromString(string eml)
		{
			return new EmailForTest(Encoding.UTF8.GetBytes(eml));
		}

		public override void DeleteProcessedMail(List<long> processedList)
		{
			if (debugEmails != null)
			{
				debugEmails.Clear();
			}
			if (debugStringEmails != null)
			{
				debugStringEmails.Clear();
			}
			return;
		}

		public void AddEmailBundle(params HRServiceEmail[] emails)
		{
			if (debugEmails == null)
			{
				debugEmails = new List<HRServiceEmail>();
			}
			debugEmails.AddRange(emails);

			if (debugStringEmails == null)
			{
				debugStringEmails = new List<string>();
			}
			foreach (HRServiceEmail email in emails)
			{
				debugStringEmails.Add(email == null ? "" : email.GetEml());
			}
		}
		List<HRServiceEmail> debugEmails;

		public void AddEmailBundle(params string[] emails)
		{
			if (debugStringEmails == null)
			{
				debugStringEmails = new List<string>();
			}
			debugStringEmails.AddRange(emails);

			if (debugEmails == null)
			{
				debugEmails = new List<HRServiceEmail>();
				foreach (string stringEmail in emails)
				{
					HRServiceEmail email = new EmailBuilderForTesting().Body(stringEmail).GetEmail();
					debugEmails.Add(email);
				}
			}
		}
		List<string> debugStringEmails;
	}
}
