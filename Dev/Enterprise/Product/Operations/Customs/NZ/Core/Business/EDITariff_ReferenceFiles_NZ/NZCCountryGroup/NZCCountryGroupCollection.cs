using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCCountryGroupCollection : BusinessObjectCollection<NZCCountryGroup>
	{
		public NZCCountryGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
