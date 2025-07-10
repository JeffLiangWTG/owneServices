using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MailManager.ExternalMailInterface;
using MimeKit;

namespace Enterprise.Recruiter.ServiceTasks
{
	public class HRServiceEmail
	{
		public HRServiceEmail(byte[] eml)
		{
			this.eml = eml;
			message = MimeMessageExtensions.CreateMessageFromEml(eml);
		}

		public string From
		{
			get
			{
				return message.From.Count > 0 ? message.From.Mailboxes.First().Address : string.Empty;
			}
		}

		public string SenderNameAddress
		{
			get => ToNameAddress(message.GetSenderOrFrom());
		}

		public string SenderAddress
		{
			get => message.GetSenderOrFrom()?.Address ?? string.Empty;
		}

		public string SenderName
		{
			get => message.GetSenderOrFrom()?.Name ?? string.Empty;
		}

		public string Subject
		{
			get { return message.Subject ?? string.Empty; }
		}

		public ZDateTime EmailDate
		{
			get { return message.Date != DateTimeOffset.MinValue ? new ZDateTime(message.Date.LocalDateTime) : ZDateTime.Invalid; }
		}

		public string Body
		{
			get { return message.TextBody ?? string.Empty; }
		}

		public string HtmlBody
		{
			get { return message.HtmlBody ?? string.Empty; }
		}

		public string GetEml()
		{
			return Encoding.UTF8.GetString(eml) ?? string.Empty;
		}

		public byte[] GetEmlBytes()
		{
			return eml ?? Array.Empty<byte>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public bool SaveEml(string fileName, out string errorMessage)
		{
			try
			{
				File.WriteAllText(fileName, GetEml());
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				errorMessage = ex.Message;
				return false;
			}

			errorMessage = null;
			return true;
		}

		public int NonVisualCount
		{
			get { return message.GetNonVisualAttachments().Count(); }
		}

		public int VisualCount
		{
			get { return message.GetVisualAttachments().Count(); }
		}

		static string ToNameAddress(MailboxAddress mailbox)
		{
			string result = string.Empty;
			if (mailbox != null)
			{
				if (!string.IsNullOrEmpty(mailbox.Name))
				{
					result += '"' + mailbox.Name + '"';
				}

				if (!string.IsNullOrEmpty(mailbox.Address))
				{
					if (result.Length > 0)
					{
						result += ' ';
					}

					result += '<' + mailbox.Address + '>';
				}
			}
			return result;
		}

		public MimeMessage Mail => message;

		readonly MimeMessage message;
		readonly byte[] eml;
	}
}
