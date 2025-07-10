using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.PL.Business;

public static class OrgContactExtensions
{
	public static bool HasNonEmptyPhone(this IOrgContact contact)
		=> contact != null &&
			(!contact.OC_Phone.IsEmpty
			|| !contact.OC_Mobile.IsEmpty
			|| !contact.OC_HomePhone.IsEmpty
			|| !contact.OC_OtherPhone.IsEmpty);

	public static ZString GetFirstNonEmptyPhone(this IOrgContact contact)
		=> contact == null ? ZString.Empty
			: !contact.OC_Phone.IsEmpty ? contact.OC_Phone
			: !contact.OC_Mobile.IsEmpty ? contact.OC_Mobile
			: !contact.OC_HomePhone.IsEmpty ? contact.OC_HomePhone
			: !contact.OC_OtherPhone.IsEmpty ? contact.OC_OtherPhone
			: ZString.Empty;
}
