using CargoWise.Types;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface IAIMParty
	{
		ZString Name { get; }
		ZString StreetAddress { get; }
		ZString CityCountyTownship { get; }
		ZString StateOrProvince { get; }
		ZString CountryCode { get; }
		ZString PostalCode { get; }
		ZString TelephoneNumber { get; }
	}
}
