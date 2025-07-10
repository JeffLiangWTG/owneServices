using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public class ContactPersonProvider : IContactPerson
{
	public ContactPersonProvider(GlbStaff glbStaff)
	{
		this.glbStaff = Argument.NotNull(glbStaff, nameof(glbStaff));
	}

	public static ContactPersonProvider NewOrNull(GlbStaff staff)
	{
		var provider = new ContactPersonProvider(staff);
		return !string.IsNullOrEmpty(provider.Name) && !string.IsNullOrEmpty(provider.PhoneNumber)
			? provider
			: null;
	}

	readonly GlbStaff glbStaff;

	public string Name => CachedValueHelper.GetValue(ref name, () => glbStaff.GS_FullName);
	CachedValue<string> name;

	public string PhoneNumber => CachedValueHelper.GetValue(ref phoneNumber, GetPhoneNumber);
	CachedValue<string> phoneNumber;

	public string EMailAddress => CachedValueHelper.GetValue(ref email, GetEmail);
	CachedValue<string> email;

	string GetEmail()
	{
		return !glbStaff.GS_EmailAddress.IsEmpty && glbStaff.GS_PublishEmailAddress
			? glbStaff.GS_EmailAddress.ToString()
			: null;
	}

	string GetPhoneNumber()
	{
		return !glbStaff.GS_WorkPhone.IsEmpty && glbStaff.GS_PublishWorkPhone
			? glbStaff.GS_WorkPhone.ToString()
			: !glbStaff.GS_MobilePhone.IsEmpty && glbStaff.GS_PublishMobilePhone
				? glbStaff.GS_MobilePhone.ToString()
				: !glbStaff.GS_HomePhone.IsEmpty && glbStaff.GS_PublishHomePhone
					? glbStaff.GS_HomePhone.ToString()
					: null;
	}
}
