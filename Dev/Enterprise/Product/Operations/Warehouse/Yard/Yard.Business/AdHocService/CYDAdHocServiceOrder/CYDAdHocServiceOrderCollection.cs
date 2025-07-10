using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDAdHocServiceOrderCollection : ActiveBusinessObjectCollection<CYDAdHocServiceOrder>
	{
		public CYDAdHocServiceOrderCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
