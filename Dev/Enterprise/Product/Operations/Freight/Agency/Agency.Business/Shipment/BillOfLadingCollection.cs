using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Business
{
	[ModuleID(ModuleId.AgencyBillOfLading)]
	public class BillOfLadingCollection : ActiveBusinessObjectCollection<BillOfLading>
	{
		public BillOfLadingCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public BillOfLadingCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter) { }
	}
}
