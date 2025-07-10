using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Integration;

namespace Enterprise.Customs.PL.Business;

public sealed class ContactPersonJobDocAddressProvider : IContactPerson
{
	ContactPersonJobDocAddressProvider(string name, string phone, string email)
	{
		Name = name;
		PhoneNumber = phone;
		EMailAddress = email;
	}

	public static ContactPersonJobDocAddressProvider NewOrNull(IJobDocAddress address) =>
		address.E2_Contact.IsEmpty || address.E2_Phone.IsEmpty
			? null
			: new ContactPersonJobDocAddressProvider(address.E2_Contact, address.E2_Phone, address.E2_Email);

	public string Name { get; private set; }

	public string PhoneNumber { get; private set; }

	public string EMailAddress { get; private set; }
}
