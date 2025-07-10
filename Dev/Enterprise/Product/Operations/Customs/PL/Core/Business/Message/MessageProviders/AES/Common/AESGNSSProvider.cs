using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.PL.Business;

public class AESGNSSProvider : IGNSS
{
	readonly ZGeography geography;

	public AESGNSSProvider(ZGeography geography)
	{
		this.geography = geography;
	}

	public string Latitude => geography.Latitude.ToString();
	public string Longitude => geography.Longitude.ToString();
}
