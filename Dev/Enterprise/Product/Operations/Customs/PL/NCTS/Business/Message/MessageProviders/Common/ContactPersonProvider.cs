using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.PL.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class ContactPersonProvider : IContactPerson
{
	public static ContactPersonProvider NewOrNull(OrgHeader orgHeader)
	{
		if (orgHeader == null)
		{
			return null;
		}

		return OrgContactsHelper.TryGetContactForMessage(orgHeader, out var contactForMessage)
				&& TryGetValidData(contactForMessage, out var name, out var phone, out var email)
			? new ContactPersonProvider(name, phone, email)
			: null;
	}

	ContactPersonProvider(string name, string phoneNumber, string email)
	{
		Name = Argument.NotNullOrEmpty(name, nameof(name));
		PhoneNumber = Argument.NotNullOrEmpty(phoneNumber, nameof(phoneNumber));
		EMailAddress = !string.IsNullOrEmpty(email) ? email : null;
	}

	public string Name { get; }
	public string PhoneNumber { get; }
	public string EMailAddress { get; }

	internal static bool TryGetValidData(OrgContact contact, out ZString name, out ZString phoneNumber, out ZString email)
	{
		if (contact == null)
		{
			name = ZString.Empty;
			phoneNumber = ZString.Empty;
			email = ZString.Empty;
			return false;
		}

		name = contact.OC_ContactName;
		phoneNumber = contact.GetFirstNonEmptyPhone();
		email = contact.Email;

		return !name.IsEmpty && !phoneNumber.IsEmpty;
	}
}
