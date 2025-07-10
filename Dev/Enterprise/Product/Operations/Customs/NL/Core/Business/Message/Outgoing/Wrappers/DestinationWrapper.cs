using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public class DestinationWrapper : IDestination
{
	public DestinationWrapper(ZString countryCode)
	{
		destinationCountryCode = countryCode.Left(2);
	}
	readonly string destinationCountryCode;

	public string CountryCode => destinationCountryCode;

	public string RegionId => null;

	public string CCQualifierCode => null;
}
