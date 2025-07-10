using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	//#warning test
	public class DtbConsignmentRunSheetFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbConsignmentRunSheetFetchStrategy(DtbConsignmentRunSheet runSheet)
			: base(runSheet)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			var runSheet = (DtbConsignmentRunSheet)BusinessObject;
			Factory.AddFetchHint(typeof(DtbConsignmentRunSheetInstruction), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet, runSheet.PK);
			Factory.AddFetchHint(typeof(RefEquipment), runSheet.KG_RQ_Truck);
			Factory.AddFetchHint(typeof(OrgHeader), runSheet.KG_OH_TransportCo);
		}
	}
}
