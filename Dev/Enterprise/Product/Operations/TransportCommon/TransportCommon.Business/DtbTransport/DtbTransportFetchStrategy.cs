using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business
{
	/// <summary>
	/// Testeded in both DtbBooking and DtbConsignment.
	/// </summary>
	public class DtbTransportFetchStrategy<TInstruction> : EnterpriseBusinessObjectFetchStrategy
		where TInstruction : DtbTransportInstruction
	{
		public DtbTransportFetchStrategy(DtbTransport transport)
			: base(transport)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(TInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, BusinessObject.PK);
		}
	}
}
