using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReleaseAdviceCollection : ActiveBusinessObjectCollection<CYDReleaseAdvice>
	{
		public CYDReleaseAdviceCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
