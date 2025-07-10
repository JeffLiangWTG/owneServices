using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ
{
	public class NZCGroupCollection : BusinessObjectCollection<NZCGroup>
	{
		public NZCGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
