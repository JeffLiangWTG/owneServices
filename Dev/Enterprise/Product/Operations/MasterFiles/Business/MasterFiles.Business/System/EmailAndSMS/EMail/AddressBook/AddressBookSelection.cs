using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AddressBookSelection : NonPersistentBusinessObject
	{
		public AddressBookSelection()
		{
			Lists.AddPair(AddressBookCodes.All, AddressBookDescriptions.All);
		}

		public List<string> ToAddresses = new List<string>();
		public CodeDescriptionPairList Lists = new CodeDescriptionPairList();

		public static class AddressBookCodes
		{
			public const string All = "";
			public const string Staff = "STAFF";
		}

		static class AddressBookDescriptions
		{
			public static MultilingualString All { get { return ResString.GetMultilingualString("AddressBookSelection|All", "All Address Books"); } }
			public static MultilingualString Staff { get { return ResString.GetMultilingualString("AddressBookSelection|Staff", "Company Staff"); } }
		}

		public void Add(AddressBookSelection selection)
		{
			Recipients.AddRange(selection.Recipients);
			ToAddresses.AddRange(selection.ToAddresses);
		}

		public AddressBookRecipientCollection Recipients
		{
			get
			{
				if (recipients == null)
				{
					recipients = new AddressBookRecipientCollection(Factory);
				}
				return recipients;
			}
		}
		AddressBookRecipientCollection recipients;

		public void AddRecipient(IAddressBookRecipient recipient)
		{
			AddRecipient(recipient, false);
		}

		public void AddRecipient(IAddressBookRecipient recipient, bool isAutoSelected)
		{
			if (recipient != null && recipient.IsActive)
			{
				if (isAutoSelected && !ToAddresses.Contains(recipient.Email))
				{
					ToAddresses.Add(recipient.Email);
				}
				AddList(recipient);
				Recipients.Add(new AddressBookRecipient(recipient));
			}
		}

		void AddList(IAddressBookRecipient recipient)
		{
			if (recipient.Organisation != null)
			{
				Lists.AddPairIfNotExist(recipient.Organisation.OH_Code, recipient.Organisation.OH_FullName);
			}
			else
			{
				Lists.AddPairIfNotExist(AddressBookCodes.Staff, AddressBookDescriptions.Staff);
			}
		}

		public void AddRecipient(OrgHeader org)
		{
			if (org != null)
			{
				foreach (IAddressBookRecipient contact in org.ContactsActive)
				{
					AddRecipient(contact);
				}
				foreach (OrgStaffAssignments assignment in org.StaffAssignments)
				{
					AddRecipient(assignment);
				}
			}
		}
	}
}
