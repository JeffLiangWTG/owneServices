using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AddressBookRecipient : NonPersistentBusinessObject
		, IAddressBookRecipient
	{
		public AddressBookRecipient(IAddressBookRecipient recipient)
		{
			Recipient = recipient;
		}

		protected IAddressBookRecipient Recipient;

		public ZString AddressBook
		{
			get
			{
				if (addressBook.IsEmpty)
				{
					if (Organisation != null)
					{
						addressBook = Organisation.OH_Code;
					}
					else
					{
						addressBook = AddressBookSelection.AddressBookCodes.Staff;
					}
				}
				return addressBook;
			}
		}
		ZString addressBook;

		public OrgHeader Organisation
		{
			get { return Recipient.Organisation; }
		}

		public ZString Location
		{
			get { return Recipient.Location; }
		}

		public ZString Phone
		{
			get { return Recipient.Phone; }
		}

		public ZString Title
		{
			get { return Recipient.Title; }
		}

		public ZString Name
		{
			get { return Recipient.Name; }
		}

		public ZString Email
		{
			get { return Recipient.Email; }
		}

		public bool IsActive
		{
			get { return Recipient.IsActive; }
		}

		public ZString Role
		{
			get { return Recipient.Role; }
		}

		public new ZGuid PK
		{
			get { return Recipient.PK; }
		}

		public override bool Equals(object obj)
		{
			AddressBookRecipient recipient = obj as AddressBookRecipient;
			if (recipient == null)
			{
				return false;
			}
			return PK == recipient.PK && AddressBook == recipient.AddressBook;
		}

		public override int GetHashCode()
		{
			return PK.GetHashCode() ^ AddressBook.GetHashCode();
		}
	}
}
