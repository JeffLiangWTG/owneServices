using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IAddress
	{
		ZString Line { get; }

		ZString ChineseLine { get; }

		ZString CountryCode { get; }

		ZString CountrySubDivisionID { get; }

		ZString CountrySubDivisionName { get; }
	}
}
