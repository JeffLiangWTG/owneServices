using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportConsignment.Business
{
	[ModuleID(ModuleId.DtbConsignmentRunSheet)]
	public class DtbConsignmentRunSheetAdHocCollection : ActiveBusinessObjectCollection<DtbConsignmentRunSheet>
	{
		public DtbConsignmentRunSheetAdHocCollection(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(DtbConsignmentRunSheet)))
		{
		}
	}
}
