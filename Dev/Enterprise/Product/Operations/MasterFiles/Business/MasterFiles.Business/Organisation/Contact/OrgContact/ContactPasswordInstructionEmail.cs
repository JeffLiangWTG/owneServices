using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public interface IPasswordInstructionEmailSource
	{
		ZGuid PK { get; }
		ZString Url { get; }
		ZString Name { get; }
		ZString Email { get; }
		ZString FromDisplayName { get; }
		ZString FromAddress { get; }
		ZString Language { get; }
		ZString Salutation { get; }
		ZString ExtraInstruction { get; }
		ZString OrgCode { get; }
		ZString OrgCodes { get; }
		Guid CompanyPKForLogin { get; }
		Guid CompanyPKForEmailTemplate { get; set; }
		BusinessObject SenderForLogs { get; }
		bool ShouldSendMasterPassword { get; }

		ContactPasswordInstructionEmail PasswordInstructionEmail { get; }

		ZString GeneratePasswordInstructionUrl(string token, PasswordInstructionType instructionType);
	}

	public enum PasswordInstructionType
	{
		Reset,
		Set,
	}

	public class ContactPasswordInstructionEmail : HtmlFormatEmailToContactBusinessObject
	{
		public ContactPasswordInstructionEmail(IPasswordInstructionEmailSource emailSource)
			: base((BusinessObject)emailSource, emailSource.FromAddress, emailSource.FromDisplayName)
		{
		}

		public ZString PasswordInstructionUrl { get; private set; }

		void SetEmailContent(ZString url, Guid emailTemplateCompanyPk)
		{
			PasswordInstructionUrl = url;

			var contact = (IPasswordInstructionEmailSource)BusinessObjectSendingEmail;
			ToDisplayName = contact.Name;
			ToEmailAddress = contact.Email;

			var companyPk = emailTemplateCompanyPk != Guid.Empty ? emailTemplateCompanyPk : PasswordInstructionEmailSource.CompanyPKForLogin;

			using (WebDataRegistry.Instance.AllowedLanguages.Value.Any(allowed => ((CodeSelection)allowed).Code == contact.Language) ? Res.TemporarilySwitchLanguage(contact.Language) : null)
			{
				Subject = EmailSubject(companyPk);
				Body = EmailBody(companyPk);
			}
		}

		public void SendEmailWithPasswordInstructionUrl(ZString url, PasswordInstructionType instructionType, Guid emailTemplateCompanyPk = default)
		{
			InstructionType = instructionType;
			SetEmailContent(url, emailTemplateCompanyPk);

			ShouldAddNoteAndEvent = false;
			ShouldSaveBizOFactoryOnSent = false;

			SendEmail(systemCommunication: true);
			AddEvent();
		}

		protected override void SetEmailWithRecipients(ZArchitecture.Environment.EmailDef email, bool systemCommunication = false)
		{
			for (var i = 0; i < email.Recipients.Count; ++i)
			{
				var recipient = email.Recipients[i];
				recipient.IsForSystemCommunication = true;
				recipient.IgnoreSystemEmailDestinationOverride = true;
			}
		}

		string EmailSubject(Guid companyPk)
		{
			switch (InstructionType)
			{
				case PasswordInstructionType.Reset:
					return Parser.Parse(BusinessObjectSendingEmail, WebDataRegistry.Instance.PasswordResetEmailTemplate.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty).EmailSubject);
				case PasswordInstructionType.Set:
					return Parser.Parse(BusinessObjectSendingEmail, WebDataRegistry.Instance.PasswordSetEmailTemplate.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty).EmailSubject);
				default:
					throw new InvalidOperationException(InstructionType.ToString());
			}
		}

		string EmailBody(Guid companyPk)
		{
			switch (InstructionType)
			{
				case PasswordInstructionType.Reset:
					return Parser.Parse(BusinessObjectSendingEmail, WebDataRegistry.Instance.PasswordResetEmailTemplate.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty).EmailBody);
				case PasswordInstructionType.Set:
					return Parser.Parse(BusinessObjectSendingEmail, WebDataRegistry.Instance.PasswordSetEmailTemplate.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty).EmailBody);
				default:
					throw new InvalidOperationException(InstructionType.ToString());
			}
		}

		protected IPasswordInstructionEmailSource PasswordInstructionEmailSource => (IPasswordInstructionEmailSource)BusinessObjectSendingEmail;

		void AddEvent()
		{
			if (ShouldSaveEmailInNewFactory)
			{
				var logFactory = BusinessObjectSendingEmail.CreateNewFactory();
				logFactory.RefreshEnabled = false;
				var senderForLogs = ((IPasswordInstructionEmailSource)BusinessObjectSendingEmail).SenderForLogs;

				if (senderForLogs != null)
				{
					var bizObj = logFactory.Load(senderForLogs.GetType(), senderForLogs.PK);
					bizObj?.GetLogs().AddNew(AutoEvents.WebAccessPasswordEmailSent);
					logFactory.Save();
				}
			}
			else
			{
				BusinessObjectSendingEmail.GetLogs().AddNew(Events.WebAccessPasswordEmailSent);
			}
		}

		PasswordInstructionType InstructionType = PasswordInstructionType.Reset;

		DocumentParser Parser
		{
			get
			{
				if (parser == null)
				{
					parser = DocumentParser.New(ObjectFactory.GetType<Enterprise.Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(), Factory);
				}
				return parser;
			}
		}
		DocumentParser parser;
	}
}
