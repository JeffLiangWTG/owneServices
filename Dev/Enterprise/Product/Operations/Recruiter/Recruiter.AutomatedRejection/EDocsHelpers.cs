using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.EConversation.Business;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.AutomatedRejection
{
	public static class EDocsHelpers
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Email File Extension")]
		public const string EmailFileExtension = "eml";

		public static IeDoc SaveEmailToEdocsAndAddToConversation(MailItem mail, JobConversation conversation)
		{
			Argument.NotNull(mail, nameof(mail));
			Argument.NotNull(conversation, nameof(conversation));

			var application = conversation.Parent as HRJobApplication;
			Argument.NotNull(application, nameof(application));

			var documentType = NullIfEmpty(RecruitmentDataRegistry.Instance.ReceivedMailDocType.Value);
			var edoc = AddToEDocs(application, mail, documentType);

			var originalSender = mail.GetFromEmailAddress();
			AddToConversation(conversation, originalSender, CreateMessageBody(mail.MI_Subject, edoc));

			application.Factory.Save();
			conversation.Factory.Save();

			return edoc;
		}

		internal static IeDoc AddToEDocs(IDocManagerSupport host, MailItem mail, string documentType)
		{
			var tempFilePath = Temp.GetTempFileNameWithExtension(EmailFileExtension);
			FileStream emailFileStream = null;
			try
			{
				emailFileStream = new FileStream(tempFilePath, FileMode.Open);
				mail.SaveEntireEmailAsEml(emailFileStream);

				var doc = host.DocManagerInfo.AddFileOrDocument(
					File.ReadAllBytes(tempFilePath),
					AsFileName(mail.MI_Subject) + "." + EmailFileExtension,
					documentType,
					description: mail.MI_Subject);

				host.DocManagerInfo.Save();

				return doc;
			}
			finally
			{
				try
				{ File.Delete(tempFilePath); }
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }

				emailFileStream?.Dispose();
			}
		}

		internal static string CreateMessageBody(string display, IeDoc edoc)
		{
			var docHyperlink = ObjectFactory.Get<IShowEDocUrlHandler>().Create(edoc);
			return Res.GetString("7f343425-7cde-485c-9ee3-4c523ff46531", "Received [{0}]({1}).", display, docHyperlink);
		}

		internal static void AddToConversation(JobConversation conversation, string from, string body)
			=> conversation.Messages.AddNew(GetOrAddParticipant(conversation, from), body);

		internal static JobConversationParticipant GetOrAddParticipant(JobConversation conversation, string from)
		{
			var existingSender = conversation.Participants
				.FirstOrDefault(p => from.Equals(p.EmailAddress, StringComparison.OrdinalIgnoreCase));

			return existingSender ?? (FindPersonWithEmail(conversation.Factory, from) is IConversationParticipant participant
				? conversation.Participants.AddNewParticipant(participant)
				: conversation.Participants.AddNewParticipant(from));
		}
		internal static BusinessObject FindPersonWithEmail(BusinessObjectFactory factory, string email)
			=> factory.LoadTop1(typeof(GlbStaff), new ZQuery(GlbStaffSchema.GS_EmailAddress, email))
				?? factory.LoadTop1(typeof(OrgContact), new ZQuery(OrgContactSchema.OC_Email, email));

		internal static string AsFileName(string s)
			=> Path.GetInvalidFileNameChars().Aggregate(s, (result, dodgyChar) => result.Replace(dodgyChar, '_'));

		public static string NullIfEmpty(string s)
			=> string.IsNullOrWhiteSpace(s) ? null : s;
	}
}
