using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USCVisaTariffNonDependentCollection : BusinessObjectCollection<USCVisaTariff>
	{
		public USCVisaTariffNonDependentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
