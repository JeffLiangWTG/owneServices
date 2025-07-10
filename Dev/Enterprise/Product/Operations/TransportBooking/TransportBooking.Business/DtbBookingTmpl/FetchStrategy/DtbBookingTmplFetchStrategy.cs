using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingInstructionTmplFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbBookingInstructionTmplFetchStrategy(DtbBookingInstructionTmpl instruction)
			: base(instruction)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(DtbBookingTmplSchema.Constants.TableName, Instruction.K2_KT_BookingTmpl);
		}

		DtbBookingInstructionTmpl Instruction
		{
			get { return (DtbBookingInstructionTmpl)BusinessObject; }
		}
	}
}
