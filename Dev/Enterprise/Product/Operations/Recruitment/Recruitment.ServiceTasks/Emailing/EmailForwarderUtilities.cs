using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.AutomatedRejection;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Registry;
using MimeKit;

namespace Enterprise.Recruitment.ServiceTasks.Emailing
{
	public static class EmailForwarderUtilities
	{
		public static bool ProcessEmail(MailItem email, ILogger logger)
		{
			var conversation = TryFindExistingJobConversation(email);
			if (conversation == null)
			{
				logger.Log(LogType.Error, "Unable to find a job conversation for this email.");
				return false;
			}

			EDocsHelpers.SaveEmailToEdocsAndAddToConversation(email, conversation);

			var otherParticipants = GetOtherParticipantsInConversation(email.GetFromEmailAddress(), conversation.Participants, logger);
			if (otherParticipants == null || otherParticipants.Length == 0)
			{
				logger.Log(LogType.Error, "Unable to find any participants to forward an email to.");
				return false;
			}

			var fromAddress = EDocsHelpers.NullIfEmpty(RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value);
			var forwardedEmails = CreateForwardsEmails(otherParticipants, email, fromAddress);
			var sendResults = SendForwardsEmails(forwardedEmails).ToList();

			var unsuccessfulEmails = sendResults.Where(e => e.SendResult == EmailSendResult.Unsuccessful);
			var successfulEmails = sendResults.Where(e => e.SendResult == EmailSendResult.Successful);

			foreach (var (sendResult, forwardedEmail) in unsuccessfulEmails)
			{
				logger.Log(LogType.Error, $"Unable to send forwarded email: {GetFailedEmailSummary(forwardedEmail)}");
			}

			logger.Log(LogType.Information, $"Successfully forwarded {successfulEmails.Count()}/{sendResults.Count} emails.");

			return !unsuccessfulEmails.Any();
		}

		public static string GetFailedEmailSummary(MailItem email)
			=> $"Subject=\"{email.MI_Subject}\" From=\"{email.MI_From}\" To=\"{email.AllRecipients}\"";

		internal static JobConversation TryFindExistingJobConversation(MailItem mail)
		{
			_ = Argument.NotNull(mail, nameof(mail));

			var regex = new Regex($"###({EmailBizoEncoder.RegexString})###");
			var decodedBody = ExtractDecodedBody(mail);
			var matches = regex.Matches(decodedBody);
			foreach (Match match in matches)
			{
				if (match.Success)
				{
					var encoded = match.Groups[1].Value;
					return (JobConversation)EmailBizoEncoder.DecodeBizo(new BusinessObjectFactory(), encoded);
				}
			}
			return null;
		}

		internal static string ExtractDecodedBody(MailItem mail)
		{
			var bytes = mail.RawEmailBytes;
			var msg = MimeMessage.Load(new MemoryStream(bytes.ToArray()));

			if (!string.IsNullOrEmpty(msg.TextBody))
			{
				return msg.TextBody.Trim();
			}

			if (!string.IsNullOrEmpty(msg.HtmlBody))
			{
				return msg.HtmlBody.Trim();
			}

			return null;
		}

		internal static IEnumerable<(EmailSendResult SendResult, MailItem ForwardedEmail)> SendForwardsEmails(IEnumerable<MailItem> forwardedEmails)
		{
			return new SmtpEmailSender().SendEmails(forwardedEmails);
		}

		internal static IEnumerable<MailItem> CreateForwardsEmails(IEnumerable<string> recipients, MailItem mail, string fromAddress)
		{
			foreach (var r in recipients.Where(e => e != fromAddress))
			{
				yield return CreateForwardsEmails(r, mail, fromAddress);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Email header strings")]
		internal static MailItem CreateForwardsEmails(string recipientAddress, MailItem mail, string fromAddress)
		{
			var factory = new BusinessObjectFactory();
			var copy = factory.New<MailItem>();

			copy.CopyAttachmentsFrom(mail);

			copy.MI_Application = "MMT";
			copy.MI_Status = "QUE";
			copy.MI_Direction = "TRX";
			copy.MI_From = fromAddress;
			copy.MI_ReplyTo = fromAddress;
			copy.MI_SendDateTime = ZDateTime.Empty;

			copy.MI_Subject = mail.MI_Subject;
			copy.MI_LastAttemptDateTime = mail.MI_LastAttemptDateTime;
			copy.MI_ReceivedDateTime = mail.MI_ReceivedDateTime;
			copy.MI_SystemCreateTimeUtc = mail.MI_SystemCreateTimeUtc;
			copy.MI_SystemCreateUser = mail.MI_SystemCreateUser;
			copy.MI_Body = mail.MI_Body;
			copy.MI_XMLInfo = mail.MI_XMLInfo;
			copy.MI_ContentType = mail.MI_ContentType;
			copy.MI_Encoding = mail.MI_Encoding;

			// specifically not copied so a new UIDL can be generated for it
			//copy.MI_POP3UIDL = mail.MI_POP3UIDL;

			// we'll write our own headers
			copy.MI_Header = string.Empty;

			copy.SetHeaderItem("Subject", mail.MI_Subject);
			copy.SetHeaderItem("From", fromAddress);
			copy.SetHeaderItem("To", recipientAddress);
			copy.SetHeaderItem("Reply-To", fromAddress);

			CopyHeaderIfSet(mail, copy, "References");
			CopyHeaderIfSet(mail, copy, "MIME-Version");
			CopyHeaderIfSet(mail, copy, "Content-Transfer-Encoding");
			CopyHeaderIfSet(mail, copy, "Content-Type");
			CopyHeaderIfSet(mail, copy, "Date");
			CopyHeaderIfSet(mail, copy, "Accept-Language");
			CopyHeaderIfSet(mail, copy, "Content-Language");
			CopyHeaderIfSet(mail, copy, "Message-ID");
			CopyHeaderIfSet(mail, copy, "Thread-Topic");
			CopyHeaderIfSet(mail, copy, "Thread-Index");
			CopyHeaderIfSet(mail, copy, "In-Reply-To");

			return copy;
		}

		internal static void CopyHeaderIfSet(MailItem copyFrom, MailItem copyTo, string header)
		{
			var value = copyFrom.GetHeaderItem(header);
			if (!string.IsNullOrEmpty(value))
			{
				copyTo.SetHeaderItem(header, value);
			}
		}

		internal static string[] GetOtherParticipantsInConversation(string originalSender, JobConversationParticipantCollection participants, ILogger logger)
		{
			_ = Argument.NotNullOrEmpty(originalSender, nameof(originalSender));
			_ = Argument.NotNull(participants, nameof(participants));

			var otherParticipants = participants
				.Where(p => p.JCP_IsSubscribed && p.Parent.IsActive)
				.Select(p => p.EmailAddress.ToString())
				.Where(address => !string.IsNullOrEmpty(address) && !address.Equals(originalSender, StringComparison.OrdinalIgnoreCase))
				.ToArray();

			if (otherParticipants.Length == 0)
			{
				logger.Debug($"OtherParticipantsCount={otherParticipants.Length} ParticipantsCount={participants.Count} Original sender='{originalSender}'");
				foreach (var p in participants)
				{
					logger.Debug($"Name='{p.Parent.Name}' Subscribed={p.JCP_IsSubscribed} ParentIsActive={p.Parent.IsActive} Email='{p.EmailAddress}'");
				}
			}

			return otherParticipants;
		}

		public static string ParseEmailAddress(string email)
		{
			if (InternetAddressList.TryParse(email, out var addresses))
			{
				return addresses.Mailboxes.FirstOrDefault()?.Address;
			}

			return null;
		}
	}
}
