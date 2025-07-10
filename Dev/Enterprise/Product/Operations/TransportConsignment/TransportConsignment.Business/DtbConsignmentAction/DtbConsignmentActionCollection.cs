using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentActionCollection : ActiveBusinessObjectCollection<DtbConsignmentAction>
	{
		public DtbConsignmentActionCollection(DtbConsignmentAddress address)
			: base(address.Factory, address, null, DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress)
		{
		}

		public DtbConsignmentActionCollection(DtbConsignmentRunSheetInstruction runSheetInstruction)
			: base(runSheetInstruction.Factory, runSheetInstruction, null, DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction)
		{
		}
	}
}
