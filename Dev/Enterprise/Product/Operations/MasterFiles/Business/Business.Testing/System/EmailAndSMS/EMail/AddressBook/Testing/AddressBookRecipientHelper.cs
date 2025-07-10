using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class AddressBookRecipientHelper
	{
		public static AddressBookRecipient CreateRecipient(ZGuid pk, OrgHeader org, string phone, string name, string location, string title, string role, string email)
		{
			return new AddressBookRecipient(new IAddressBookRecipientForTest(pk, org, phone, name, location, title, role, email));
		}

		public static AddressBookRecipient CreateRecipient(ZGuid pk, OrgHeader org)
		{
			return new AddressBookRecipient(new IAddressBookRecipientForTest(pk, org, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty));
		}
	}
}
