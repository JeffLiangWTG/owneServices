using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusHAWBDependentCollection : DependentBusinessObjectCollection<CusHAWB, CusMAWB>
	{
		public CusHAWBDependentCollection(CusMAWB parent)
			: base(parent)
		{
		}

		public CusHAWBDependentCollection(CusMAWB parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}
		public CusHAWBDependentCollection(CusMAWB parent, ZQuery additionalFilter)
			: base(parent, additionalFilter)
		{
		}
	}
}
