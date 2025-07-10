using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class RecipientSelection : NonPersistentBusinessObject
	{
		public RecipientSelection(AddressBookSelection addressBookSelection)
		{
			AddressBookSelection = addressBookSelection;
			allRecipients = addressBookSelection.Recipients;
			AddressBook = AddressBookSelection.AddressBookCodes.All;
		}

		readonly AddressBookSelection AddressBookSelection;

		#region Search

		public void Search()
		{
			UpdateAvailableRecipientToSelectedAddressBook();
			ResetAdvancedSearchQueries();
			if (!SearchQuery.IsEmpty)
			{
				UpdateAvailableRecipientsByName();
			}
			AdvancedSearchFiltersBeingAppliedMessageInfo.RefreshBinding();
		}

		public ZString SearchQuery { get; set; }

		public void AdvancedSearch()
		{
			UpdateAvailableRecipientToSelectedAddressBook();
			SearchQuery = string.Empty;
			if (HasAdvancedSearchQueries)
			{
				UpdateAvailableRecipientsByMoreColumns();
			}
			AdvancedSearchFiltersBeingAppliedMessageInfo.RefreshBinding();
		}

		public void ResetAdvancedSearchQueries()
		{
			NameQuery = string.Empty;
			TitleQuery = string.Empty;
			PhoneQuery = string.Empty;
			LocationQuery = string.Empty;
			RoleQuery = string.Empty;
			AdvancedSearchFiltersBeingAppliedMessageInfo.RefreshBinding();
		}

		public void RetainPreviousAdvancedSearchQueries()
		{
			PreviousNameQuery = NameQuery;
			PreviousTitleQuery = TitleQuery;
			PreviousPhoneQuery = PhoneQuery;
			PreviousLocationQuery = LocationQuery;
			PreviousRoleQuery = RoleQuery;
		}

		public void RestorePreviousAdvancedSearchQueriesIfCancelled()
		{
			if (PreviousNameQuery != null)
			{
				NameQuery = PreviousNameQuery;
			}
			if (PreviousTitleQuery != null)
			{
				TitleQuery = PreviousTitleQuery;
			}
			if (PreviousPhoneQuery != null)
			{
				PhoneQuery = PreviousPhoneQuery;
			}
			if (PreviousLocationQuery != null)
			{
				LocationQuery = PreviousLocationQuery;
			}
			if (PreviousRoleQuery != null)
			{
				RoleQuery = PreviousRoleQuery;
			}
			AdvancedSearchFiltersBeingAppliedMessageInfo.RefreshBinding();
		}

		public ZString AdvancedSearchFiltersBeingAppliedMessage
		{
			get
			{
				if (HasAdvancedSearchQueries)
				{
					return Res.GetString("83A34C7E-EE07-4C56-B1C2-46B09F504FCF", "Advanced Search filters are being applied.");
				}
				return string.Empty;
			}
		}

		public ZPropertyInfo AdvancedSearchFiltersBeingAppliedMessageInfo
		{
			get { return GetZPropertyInfo(nameof(AdvancedSearchFiltersBeingAppliedMessage)); }
		}

		public ZString NameQuery { get; set; }
		string PreviousNameQuery;

		public ZString TitleQuery { get; set; }
		string PreviousTitleQuery;

		public ZString PhoneQuery { get; set; }
		string PreviousPhoneQuery;

		public ZString LocationQuery { get; set; }
		string PreviousLocationQuery;

		[MaxLength("Lookups.StaffRoles.MaxCodeLength")]
		[List("Lookups.StaffRoles")]
		public ZString RoleQuery { get; set; }
		string PreviousRoleQuery;

		bool HasAdvancedSearchQueries
		{
			get { return !NameQuery.IsEmpty || !TitleQuery.IsEmpty || !PhoneQuery.IsEmpty || !LocationQuery.IsEmpty || !RoleQuery.IsEmpty; }
		}

		public OrgStaffAssignmentsLookupsImplementer Lookups
		{
			get { return OrgStaffAssignmentsLookupsImplementer.Get(Factory); }
		}

		new BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}
		BusinessObjectFactory factory;

		#endregion

		#region Address Books

		public CodeDescriptionPairList AddressBooks
		{
			get { return AddressBookSelection.Lists; }
		}

		[List("AddressBooks")]
		public ZString AddressBook
		{
			get { return addressBook; }
			set
			{
				if (AddressBook != value)
				{
					addressBook = value;
					if (!SearchQuery.IsEmpty)
					{
						Search();
					}
					else if (HasAdvancedSearchQueries)
					{
						AdvancedSearch();
					}
					else
					{
						UpdateAvailableRecipientToSelectedAddressBook();
					}
					AddressBookInfo.RefreshBinding();
				}
			}
		}
		ZString addressBook;

		public ZPropertyInfo AddressBookInfo
		{
			get { return GetZPropertyInfo(nameof(AddressBook)); }
		}

		#endregion

		#region Available Recipients

		public AddressBookRecipientCollection AvailableRecipients
		{
			get
			{
				if (availableRecipients == null)
				{
					availableRecipients = new AddressBookRecipientCollection(Factory);
					availableRecipients.AddRange(allRecipients);
				}
				return availableRecipients;
			}
		}
		AddressBookRecipientCollection availableRecipients;
		readonly AddressBookRecipientCollection allRecipients;

		#endregion

		#region Updating Available Recipients

		void UpdateAvailableRecipientToSelectedAddressBook()
		{
			if (AddressBook == AddressBookSelection.AddressBookCodes.All)
			{
				AvailableRecipients.ReplaceRecipients(allRecipients);
			}
			else
			{
				AvailableRecipients.ReplaceRecipients(allRecipients.Cast<AddressBookRecipient>().Where(x => x.AddressBook == AddressBook));
			}
		}

		void UpdateAvailableRecipientsByName()
		{
			var selectedAddressBookRecipients = AvailableRecipients.Cast<AddressBookRecipient>().Where(x => x.Name.StartsWith(SearchQuery, StringComparison.InvariantCultureIgnoreCase)).ToArray();
			AvailableRecipients.ReplaceRecipients(selectedAddressBookRecipients);
		}

		void UpdateAvailableRecipientsByMoreColumns()
		{
			var selectedAddressBookRecipients = AvailableRecipients.Cast<AddressBookRecipient>().Where(x =>
				(NameQuery.IsEmpty || x.Name.StartsWith(NameQuery, StringComparison.InvariantCultureIgnoreCase)) &&
				(TitleQuery.IsEmpty || x.Title.StartsWith(TitleQuery, StringComparison.InvariantCultureIgnoreCase)) &&
				(PhoneQuery.IsEmpty || x.Phone.StartsWith(PhoneQuery, StringComparison.Ordinal)) &&
				(LocationQuery.IsEmpty || x.Location.StartsWith(LocationQuery, StringComparison.InvariantCultureIgnoreCase)) &&
				(RoleQuery.IsEmpty || x.Role.StartsWith(RoleQuery, StringComparison.OrdinalIgnoreCase))).ToArray();
			AvailableRecipients.ReplaceRecipients(selectedAddressBookRecipients);
		}

		#endregion

		#region To Email Address

		[MaxLength(EmailWithAttachment.ToEmailAddressMaxLength)]
		public ZString ToEmailAddress
		{
			get { return toEmailAddress; }
			set
			{
				CheckMaximumLength(ToEmailAddressInfo, value);
				SetNonPersistentPropertyValue(ToEmailAddressInfo, ref toEmailAddress, value.Replace(" ", ""));
			}
		}

		public ZPropertyInfo ToEmailAddressInfo
		{
			get { return GetZPropertyInfo(nameof(ToEmailAddress)); }
		}

		ZString toEmailAddress;

		#endregion

		#region Cc

		[MaxLength(512)]
		public ZString Cc
		{
			get { return cc; }
			set
			{
				CheckMaximumLength(CcInfo, value);
				SetNonPersistentPropertyValue(CcInfo, ref cc, value.Replace(" ", ""));
			}
		}

		public ZPropertyInfo CcInfo
		{
			get { return GetZPropertyInfo(nameof(Cc)); }
		}

		ZString cc;

		#endregion

		#region Bcc

		[MaxLength(512)]
		public ZString Bcc
		{
			get { return bcc; }
			set
			{
				CheckMaximumLength(BccInfo, value);
				SetNonPersistentPropertyValue(BccInfo, ref bcc, value.Replace(" ", ""));
			}
		}

		public ZPropertyInfo BccInfo
		{
			get { return GetZPropertyInfo(nameof(Bcc)); }
		}

		ZString bcc;

		#endregion

		#region Append AddressLine with Selected Recipients

		public void AppendRecipientsEmailToEmailAddress(AddressBookRecipient[] recipients)
		{
			ToEmailAddress = GetAppendedAddressLine(recipients, ToEmailAddress).SubstringSafe(0, ToEmailAddressInfo.MaxLength);
		}

		public void AppendRecipientsEmailToCc(AddressBookRecipient[] recipients)
		{
			Cc = GetAppendedAddressLine(recipients, Cc).SubstringSafe(0, CcInfo.MaxLength);
		}

		public void AppendRecipientsEmailToBcc(AddressBookRecipient[] recipients)
		{
			Bcc = GetAppendedAddressLine(recipients, Bcc).SubstringSafe(0, BccInfo.MaxLength);
		}

		ZString GetAppendedAddressLine(AddressBookRecipient[] recipients, ZString addressLine)
		{
			var recipientEmails = recipients.Select(x => x.Email).Where(x => !x.IsEmpty).Distinct();
			if (!recipientEmails.Any())
			{
				return addressLine;
			}

			if (!addressLine.IsEmpty)
			{
				var emails = addressLine.Split(";");
				var result = emails.Concat(recipientEmails).Distinct().ToArray();
				return string.Join(";", result);
			}

			return string.Join(";", recipientEmails);
		}

		#endregion

		#region Remove

		public void RemoveRecipientsEmailFromEmailAddress(AddressBookRecipient[] recipients)
		{
			ToEmailAddress = GetRemoveRecipientsEmailAddress(ToEmailAddress, recipients);
		}

		public void RemoveRecipientsEmailFromCc(AddressBookRecipient[] recipients)
		{
			Cc = GetRemoveRecipientsEmailAddress(Cc, recipients);
		}

		public void RemoveRecipientsEmailFromBcc(AddressBookRecipient[] recipients)
		{
			Bcc = GetRemoveRecipientsEmailAddress(Bcc, recipients);
		}

		string GetRemoveRecipientsEmailAddress(ZString emailAddress, AddressBookRecipient[] recipients)
		{
			if (!emailAddress.IsEmpty && recipients != null)
			{
				var recipientEmails = recipients.Select(x => x.Email).Where(x => !x.IsEmpty).ToArray();
				if (recipientEmails.Length > 0)
				{
					var emails = emailAddress.Split(";");
					var result = emails.Except(recipientEmails).ToArray();
					return string.Join(";", result);
				}
			}
			return emailAddress;
		}

		#endregion
	}
}
