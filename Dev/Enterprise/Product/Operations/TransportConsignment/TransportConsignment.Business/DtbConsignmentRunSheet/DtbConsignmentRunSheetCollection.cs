using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business
{
	[ModuleID(ModuleId.DtbConsignmentRunSheet)]
	public class DtbConsignmentRunSheetCollection : ActiveBusinessObjectCollection<DtbConsignmentRunSheet>
	{
		public DtbConsignmentRunSheetCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
