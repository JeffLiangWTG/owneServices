using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderHeaderCollection : ActiveBusinessObjectCollection<MNRWorkOrderHeader>
	{
		public MNRWorkOrderHeaderCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
