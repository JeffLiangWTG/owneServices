using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentInstructionFetchStrategy : DtbTransportInstructionFetchStrategy<DtbConsignmentInstructionPkgDivot, DtbConsignmentConfirmation>
	{
		public DtbConsignmentInstructionFetchStrategy(DtbConsignmentInstruction instruction)
			: base(instruction)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			// the route planner starts from confirmations --> instructions --> consignment --> booking (for the booking ID)
			Factory.AddFetchHint(typeof(DtbBookingConsignment), Instruction.KN_KM_BookingMovement);
			Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, Instruction.PK);
		}

		DtbConsignmentInstruction Instruction
		{
			get { return (DtbConsignmentInstruction)BusinessObject; }
		}
	}
}
