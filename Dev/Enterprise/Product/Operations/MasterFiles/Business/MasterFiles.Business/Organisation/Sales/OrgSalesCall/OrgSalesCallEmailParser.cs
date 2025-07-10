using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using MsgReader.Outlook;
using static Enterprise.Core.Constants.Sales;
using static Enterprise.MasterFiles.Business.BounceEmailParser;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallEmailParser
	{
		public enum Result
		{
			Success = 0,
			NotEmailFormat = 1,
			InvalidEmailFormat = 2,
			NoMatchingContacts = 3,
		}

		public interface IGUIProvider
		{
			OrgContact SelectOrgContact(FilteredContactsCollectionWrapper contacts);
		}

		public OrgSalesCallEmailParser(OrgSalesCall salesCall, IGUIProvider gUIProvider)
		{
			Parent = salesCall;
			Factory = salesCall.Factory;
			this.GUIProvider = gUIProvider;
		}

		protected readonly OrgSalesCall Parent;
		readonly BusinessObjectFactory Factory;
		readonly IGUIProvider GUIProvider;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public Result PopulateFromEmailFile(ZString fileName)
		{
			var result = Result.Success;
			const string msgExt = ".MSG";
			const string emlExt = ".EML";

			try
			{
				result = Result.NotEmailFormat;

				if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
				{
					var fileExtension = Path.GetExtension(fileName).ToUpperInvariant();

					if (fileExtension == msgExt)
					{
						using (var message = new Storage.Message(fileName))
						{
							return ParseMailItem(message);
						}
					}
					else if (fileExtension == emlExt)
					{
						return ParseMailItem(MimeMessage.Load(fileName));
					}
				}
			}
			catch (Exception)
			{
				result = Result.InvalidEmailFormat;
			}

			return result;
		}

		protected Result ParseMailItem(Storage.Message message)
		{
			var contact = GetContactFromBusiness(message.Headers?.UnknownHeaders[BounceEmailConstants.BusinessEntityTableCodeKey], message.Headers?.UnknownHeaders[BounceEmailConstants.BusinessEntityIDKey]);
			if (contact == null)
			{
				var emails = message.Recipients
					.Select(r => r.Email)
					.Append(message.Sender.Email)
					.Distinct()
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.ToArray();
				contact = GetContactFromEmailAddresses(emails);
			}

			var universalTime = message.SentOn?.ToUniversalTime();

			var body = GetPlainTextFromHtml(message.BodyHtml);
			if (string.IsNullOrWhiteSpace(body))
			{
				body = message.BodyText;
			}
			return ParseMailItemCore(universalTime, message.Subject, body, contact);
		}

		protected Result ParseMailItem(MimeMessage message)
		{
			var contact = GetContactFromBusiness(message.Headers[BounceEmailConstants.BusinessEntityTableCodeKey], message.Headers[BounceEmailConstants.BusinessEntityIDKey]);
			if (contact == null)
			{
				var emails = message.From.Mailboxes
					.Union(message.To.Mailboxes)
					.Union(message.Cc.Mailboxes)
					.Union(message.Bcc.Mailboxes)
					.Select(x => x.Address)
					.Distinct()
					.Where(x => !string.IsNullOrWhiteSpace(x))
					.ToArray();
				contact = GetContactFromEmailAddresses(emails);
			}

			DateTime? universalTime = null;
			if (message.Date != DateTimeOffset.MinValue)
			{
				universalTime = message.Date.UtcDateTime;
			}

			var body = GetPlainTextFromHtml(message.HtmlBody);
			if (string.IsNullOrWhiteSpace(body))
			{
				body = message.TextBody;
			}
			return ParseMailItemCore(universalTime, message.Subject, body, contact);
		}

		Result ParseMailItemCore(DateTime? universalTime, string subject, string body, OrgContact contact)
		{
			var result = Result.Success;

			if (contact != null)
			{
				Parent.OQ_OH = contact.OC_OH;
				Parent.OQ_OC = contact.PK;
			}
			else
			{
				result = Result.NoMatchingContacts;
			}

			Parent.OQ_TypeOfCall = CommunicationType.Email;
			Parent.OQ_Status = Status.Completed;
			if (universalTime.HasValue)
			{
				Parent.OQ_CallDate = (ZDateTime)universalTime;
			}
			Parent.OQ_CallSummary = new ZString(subject).SubstringSafe(0, OrgSalesCallSchema.OQ_CallSummary.MaxLength);

			var bodyText = new ZString(body);
			if (!bodyText.IsLettersAndNumbersAndPunctuationOnlyOrEmpty)
			{
				bodyText = bodyText.ConvertToOnlyLettersAndNumbersAndPunctuation();
			}
			Parent.OQ_SalesCallNotes = ZBlob.FromUTF8(bodyText);
			return result;
		}

		OrgContact GetContactFromBusiness(string businessEntityTableCode, string businessEntityID)
		{
			ZGuid campaignItemPk;

			if (businessEntityTableCode == GlbCompanyCampaignItemSchema.Constants.Prefix && ZGuid.TryParse(businessEntityID, out campaignItemPk))
			{
				var campaignItem = Factory.Load<IGlbCompanyCampaignItem>(campaignItemPk);

				if (campaignItem != null && campaignItem.G8_RecipientTableCode == OrgContactSchema.Constants.Prefix && !campaignItem.G8_RecipientID.IsEmpty)
				{
					return Factory.Load<OrgContact>(campaignItem.G8_RecipientID);
				}
			}

			return null;
		}

		protected OrgContact GetContactFromEmailAddresses(string[] emails)
		{
			var currentUserEml = Env.CurrentUser.EmailAddress;
			if (!string.IsNullOrWhiteSpace(currentUserEml))
			{
				emails = emails.Where(x => !x.Equals(currentUserEml, StringComparison.OrdinalIgnoreCase)).ToArray();

				var addressParts = currentUserEml.Split(new[] { '@' }, 2);
				if (addressParts.Length == 2)
				{
					var domainPart = '@' + addressParts[1];

					var sameDomainEmailsCount = emails.Where(a => a.EndsWith(domainPart, StringComparison.OrdinalIgnoreCase)).Count();

					if (emails.Length != sameDomainEmailsCount)
					{
						emails = emails.Where(x => (domainPart == null || !x.EndsWith(domainPart, StringComparison.OrdinalIgnoreCase))).ToArray();
					}
				}
			}

			if (!emails.Any())
			{
				return null;
			}

			var contactQuery = new ZQuery(OrgContactSchema.OC_Email, emails);
			var contacts = new OrgContactCollection(Factory, contactQuery);
			contacts.Load();

			if (contacts == null || contacts.Count == 0)
			{
				return null;
			}

			var activeContactQuery = new ZQuery(OrgContactSchema.OC_IsActive, true);
			var activeContacts = contacts.Find(activeContactQuery);
			if (activeContacts != null && activeContacts.Length == 1)
			{
				return (OrgContact)activeContacts.First();
			}

			if (activeContacts != null && activeContacts.Length > 1 && Parent.OQ_OH.IsValid)
			{
				var activeOrgContacts = activeContacts.ToList().FindAll(temp => ((OrgContact)temp).OC_OH == Parent.OQ_OH);
				if (activeOrgContacts != null && activeOrgContacts.Count == 1)
				{
					return (OrgContact)activeOrgContacts.First();
				}
			}

			var contactsWrapper = new FilteredContactsCollectionWrapper(contacts, typeof(OrgSalesCallAdditionalAttendee));
			contactsWrapper.IncludeInactiveContacts = true;
			return GUIProvider.SelectOrgContact(contactsWrapper);
		}

		protected string GetPlainTextFromHtml(string html) => new HtmlToTextUtility().GetPlainText(html);
	}
}
