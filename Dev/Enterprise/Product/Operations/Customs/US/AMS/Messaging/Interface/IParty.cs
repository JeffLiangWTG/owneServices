using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface IParty
	{
		ZString Name { get; }
		ZString AddressLine1 { get; }
		ZString AddressLine2 { get; }
		ZString AddressLine3 { get; }
		ZString TelephoneOrTelexNumberOrAddressLine4 { get; }
	}
}
