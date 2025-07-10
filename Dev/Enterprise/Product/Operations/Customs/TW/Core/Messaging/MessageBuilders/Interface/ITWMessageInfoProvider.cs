using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface ITWMessageInfoProvider
	{
		ZString EntryNumber { get; }
		ZString EntryNumberType { get; }
		ZString StaffCode { get; }
		ZString CompanyID { get; }
		ZString PasswordType { get; }
	}
}
