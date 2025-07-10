using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class MNRWorkOrderApprovalPartyCollection : ActiveBusinessObjectCollection<MNRWorkOrderApprovalParty>
	{
		public MNRWorkOrderApprovalPartyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
		public MNRWorkOrderApprovalPartyCollection(BusinessObjectFactory factory, BusinessObject master)
			: base(factory, master, new ZQuery(), MNRWorkOrderApprovalPartySchema.MNA_MWO_WorkOrder)
		{
		}
	}
}
