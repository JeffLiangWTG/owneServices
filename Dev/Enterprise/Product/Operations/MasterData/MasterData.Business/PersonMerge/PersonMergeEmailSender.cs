using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterData.Business
{
	public class PersonMergeEmailSender : NonPersistentBusinessObject
	{
#if DEBUG
		[Obsolete("Use the constructor that takes persons, this constructor is just for DocWrapper", true)]
		public PersonMergeEmailSender()
		{
		}
#endif

		public PersonMergeEmailSender(GlbPerson retainedPerson, IEnumerable<GlbPerson> dissolvedPersonCollection) : base(retainedPerson.Factory)
		{
			this.dissolvedPersonCollection = new ReadOnlyCollection<IPersonWrapper>(dissolvedPersonCollection.Select(x => (IPersonWrapper)new PersonWrapper(x)).ToArray());
			RetainedPerson = new PersonWrapper(retainedPerson);
		}

		readonly IReadOnlyCollection<IPersonWrapper> dissolvedPersonCollection;

		public IEnumerable<IPersonWrapper> SuccessfulDissolvedPersonCollection => dissolvedPersonCollection.Where(x => x.IsDissolvedSuccessfully);

		public IPersonWrapper RetainedPerson { get; }

		#region Wrapper Implementation

		public interface IPersonWrapper
		{
			ZGuid PK { get; }
			ZString Email { get; }
			ZString FullName { get; }
			bool HasPassword { get; }
			ZBlob PasswordHash { get; }
			bool IsDissolvedSuccessfully { get; }
			IReadOnlyCollection<IContactWrapper> ActiveContactsWithEmail { get; }
		}

		public interface IContactWrapper
		{
			ZString OrgCode { get; }
			ZString OrgName { get; }
			ZString Email { get; }
		}

		class PersonWrapper : IPersonWrapper
		{
			public PersonWrapper(GlbPerson person)
			{
				PK = person.PK;
				Email = person.PER_EmailAddress;
				FullName = person.PER_FullName;
				HasPassword = person.HasPassword;
				PasswordHash = person.PER_PasswordHash;
				ActiveContactsWithEmail = new ReadOnlyCollection<ContactWrapper>(person.ContactCollection.Cast<OrgContact>().Where(x => x.OC_IsActive && !x.OC_Email.IsEmpty).Select(x => new ContactWrapper(x)).ToArray());
			}

			public ZGuid PK { get; }
			public ZString Email { get; }
			public ZString FullName { get; }
			public bool HasPassword { get; }
			public ZBlob PasswordHash { get; }
			public bool IsDissolvedSuccessfully { get; set; }
			public IReadOnlyCollection<IContactWrapper> ActiveContactsWithEmail { get; }
		}

		class ContactWrapper : IContactWrapper
		{
			public ContactWrapper(OrgContact contact)
			{
				OrgCode = contact.OrgCode;
				OrgName = contact.WorkingAddressCompanyName;
				Email = contact.OC_Email;
			}

			public ZString OrgCode { get; }
			public ZString OrgName { get; }
			public ZString Email { get; }
		}

		#endregion

		public ReadOnlyCollection<string> ContactRecipientEmails
		{
			get
			{
				if (contactRecipientEmails == null)
				{
					var recipients = new List<string>();
					recipients.AddRange(RetainedPerson.ActiveContactsWithEmail.Select(x => x.Email.ToString()));
					foreach (var dissolvedPerson in SuccessfulDissolvedPersonCollection)
					{
						recipients.AddRange(dissolvedPerson.ActiveContactsWithEmail.Select(x => x.Email.ToString()));
					}

					contactRecipientEmails = new ReadOnlyCollection<string>(recipients.Distinct().ToArray());
				}

				return contactRecipientEmails;
			}
		}

		ReadOnlyCollection<string> contactRecipientEmails;

		public EmailDef GetMergedAccountsEmail()
		{
			if (!RetainedPerson.HasPassword && !SuccessfulDissolvedPersonCollection.Any(x => x.HasPassword) || SuccessfulDissolvedPersonCollection.All(x => !x.ActiveContactsWithEmail.Any()))
			{
				return null;
			}

			var recipients = new List<string>();
			if (!RetainedPerson.Email.IsEmpty)
			{
				recipients.Add(RetainedPerson.Email.ToString());
			}

			SuccessfulDissolvedPersonCollection.ForEach(x =>
			{
				if (!x.Email.IsEmpty)
				{
					recipients.Add(x.Email.ToString());
				}
			});

			if (!recipients.Any())
			{
				return null;
			}

			var emailTemplate = SystemDataRegistry.Instance.PersonMergeWithPasswordNotificationEmailTemplate.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);

			var email = new Security.EmailMsgFromTemplateBuilder().BuildEmailDefFromTemplate(
				Parser.Parse(this, emailTemplate.EmailSubject),
				Parser.Parse(this, emailTemplate.EmailBody),
				string.Empty,
				Guid.Empty);

			email.FromDisplayName = Env.Registry.MailboxDisplayName;
			email.FromAddress = Env.Registry.EnterpriseMailboxEmailAddress;
			var replyToEmail = Env.Registry.SMTPDefaultReturnEmailAddress;
			if (!string.IsNullOrWhiteSpace(replyToEmail))
			{
				email.ReplyTo = replyToEmail;
			}

			email.AddRecipientForUserCommunication(recipients.ToArray());

			return email;
		}

		DocumentParser Parser
		{
			get
			{
				if (parser == null)
				{
					parser = DocumentParser.New(ObjectFactory.GetType<Integration.DocumentWrappers.IDocPersonMergeEmailSender>(), Factory);
				}
				return parser;
			}
		}
		DocumentParser parser;

		public void MarkPersonAsDissolved(ZGuid pk)
		{
			var dissolvedPerson = (PersonWrapper)dissolvedPersonCollection.FirstOrDefault(x => x.PK.Equals(pk));
			if (dissolvedPerson != null)
			{
				dissolvedPerson.IsDissolvedSuccessfully = true;
			}
		}

		public bool PasswordMatchesRetainedPassword(IPersonWrapper person)
		{
			var retainedPassword = Factory.Load<GlbPerson>(RetainedPerson.PK).PER_PasswordHash;
			return person.PasswordHash == retainedPassword;
		}
	}
}
