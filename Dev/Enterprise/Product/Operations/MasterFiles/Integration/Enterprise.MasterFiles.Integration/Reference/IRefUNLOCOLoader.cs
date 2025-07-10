using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IRefUNLOCOLoader
	{
		IRefUNLOCO LoadFromIATA(BusinessObjectFactory factory, ZString iataCode);
		IRefUNLOCO LoadFromIATARegionCode(BusinessObjectFactory factory, ZString iataCityCode);
		IRefUNLOCO GetPortFromNameAndCountryCode(BusinessObjectFactory factory, ZString portName, ZString countryCode);
		IRefUNLOCO GetPortFromNameAndCountryName(BusinessObjectFactory factory, ZString portName, ZString countryName);
		IRefUNLOCO LoadFromLocalMap(BusinessObjectFactory factory, ZString localPortCode, ZString countryCode, ZString systemUsage);
		IRefUNLOCO LoadFromForeignCode(BusinessObjectFactory factory, ZString foreignCode, IOrgHeader organisation);
	}
}
