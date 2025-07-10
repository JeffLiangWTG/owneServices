using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderLineCollection : ActiveBusinessObjectCollection<MNRWorkOrderLine>
	{
		public MNRWorkOrderLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		public MNRWorkOrderLineCollection(BusinessObjectFactory factory, BusinessObject master)
			: base(factory, master, new ZQuery(), MNRWorkOrderLineSchema.MWL_MWO_MNRWorkOrderHeader)
		{
		}
	}
}
