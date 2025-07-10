using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	//#warning test
	public class DtbConsignmentRunSheetInstructionFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbConsignmentRunSheetInstructionFetchStrategy(DtbConsignmentRunSheetInstruction runSheetInstruction)
			: base(runSheetInstruction)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(DtbConsignmentConfirmation), DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction, BusinessObject.PK);
		}
	}
}
