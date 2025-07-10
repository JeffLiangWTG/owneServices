using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCCountryCollection : BusinessObjectCollection<NZCCountry>
	{
		public NZCCountryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
