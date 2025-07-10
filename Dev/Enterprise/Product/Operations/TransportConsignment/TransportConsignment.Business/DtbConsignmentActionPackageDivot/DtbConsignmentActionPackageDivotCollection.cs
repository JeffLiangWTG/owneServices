using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentActionPackageDivotCollection : ActiveBusinessObjectCollection<DtbConsignmentActionPackageDivot>
	{
		public DtbConsignmentActionPackageDivotCollection(DtbConsignmentAction consignmentAction)
			: base(consignmentAction.Factory, consignmentAction, null, DtbConsignmentActionPackageDivotSchema.LTP_LTA_ConsignmentAction)
		{
		}
	}
}
