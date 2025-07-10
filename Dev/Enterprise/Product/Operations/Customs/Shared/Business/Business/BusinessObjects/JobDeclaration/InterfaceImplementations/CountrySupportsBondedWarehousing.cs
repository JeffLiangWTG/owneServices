using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class CountrySupportsBondedWarehousing : ISupportedForBonded
	{
		public bool IsSupportsBondedWarehousing(BusinessObjectFactory factory, ZString countryCode)
		{
			var subType = BaseJobDeclaration.TypeDecider.GetTypeForCountryCode(countryCode);
			var declaration = factory.GetNull(subType) as BaseJobDeclaration;
			return declaration.SupportsBondedWarehouse();
		}
	}
}
