using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ContactWithoutCompanyInfo : NonPersistentBusinessObject
		, IPasswordInstructionEmailSource
	{
		public ContactWithoutCompanyInfo(string name, string email, ZString resetPasswordUrl, ZString salutation, ZString extraInstruction, ZString orgCode, ZString orgCodes, BusinessObject senderForLogs, bool shouldSendMasterPassword)
			: this(null, name, email, resetPasswordUrl, salutation, extraInstruction, orgCode, orgCodes, senderForLogs, shouldSendMasterPassword)
		{
		}

		public ContactWithoutCompanyInfo(BusinessObjectFactory factory, string name, string email, ZString resetPasswordUrl, ZString salutation, ZString extraInstruction, ZString orgCode, ZString orgCodes, BusinessObject senderForLogs, bool shouldSendMasterPassword)
			: base(factory)
		{
			Name = name;
			Email = email;
			Url = resetPasswordUrl;
			Salutation = salutation;
			ExtraInstruction = extraInstruction;
			OrgCode = orgCode;
			OrgCodes = orgCodes;
			PasswordInstructionEmail = new ContactPasswordInstructionEmail(this);
			SenderForLogs = senderForLogs;
			ShouldSendMasterPassword = shouldSendMasterPassword;
		}

		ZGuid IPasswordInstructionEmailSource.PK => ZGuid.Empty;
		public ZString Url { get; }
		public ZString Name { get; }
		public ZString Email { get; }
		public ZString Salutation { get; }
		public ZString ExtraInstruction { get; }
		public ZString OrgCode { get; }
		public ZString OrgCodes { get; }
		Guid IPasswordInstructionEmailSource.CompanyPKForLogin => Guid.Empty;
		public Guid CompanyPKForEmailTemplate { get; set; }

		public ZString FromDisplayName
		{
			get
			{
				ZString companyName = Env.Registry.MailboxDisplayName;
				if (companyName.IsValid && !companyName.IsEmpty)
				{
					return companyName;
				}

				return string.Empty;
			}
		}

		public ZString FromAddress => EnvProxy.Instance.Registry.SMTPDefaultDoNotReplyEmailAddress;
		public ZString Language => SharedConstants.Languages.English;
		public BusinessObject SenderForLogs { get; }
		public bool ShouldSendMasterPassword { get; }
		public ContactPasswordInstructionEmail PasswordInstructionEmail { get; }
		public ZString GeneratePasswordInstructionUrl(string token, PasswordInstructionType instructionType)
		{
			return instructionType == PasswordInstructionType.Reset ? string.Format(CultureInfo.InvariantCulture, "{0}{1}", Url, token) : throw new ArgumentOutOfRangeException(instructionType.ToString());
		}
	}
}
