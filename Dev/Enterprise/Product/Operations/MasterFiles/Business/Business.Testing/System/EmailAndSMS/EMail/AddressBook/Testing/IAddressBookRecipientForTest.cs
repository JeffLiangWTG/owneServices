using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IAddressBookRecipientForTest : NonPersistentBusinessObject, IAddressBookRecipient
	{
		public IAddressBookRecipientForTest(ZGuid pk, OrgHeader org, string phone, string name, string location, string title, string role, string email)
		{
			this.pk = pk;
			this.org = org;
			this.phone = phone;
			this.name = name;
			this.location = location;
			this.title = title;
			this.role = role;
			this.email = email;
		}

		readonly ZGuid pk;
		readonly OrgHeader org;
		readonly string phone;
		readonly string name;
		readonly string location;
		readonly string title;
		readonly string role;
		readonly string email;

		OrgHeader IAddressBookRecipient.Organisation { get { return org; } }
		ZString IAddressBookRecipient.Name { get { return name; } }
		ZString IAddressBookRecipient.Phone { get { return phone; } }
		bool IAddressBookRecipient.IsActive { get { return true; } }
		ZString IAddressBookRecipient.Location { get { return location; } }
		ZString IAddressBookRecipient.Title { get { return title; } }
		ZString IAddressBookRecipient.Role { get { return role; } }
		ZString IAddressBookRecipient.Email { get { return email; } }
		ZGuid IAddressBookRecipient.PK { get { return pk; } }
	}
}
