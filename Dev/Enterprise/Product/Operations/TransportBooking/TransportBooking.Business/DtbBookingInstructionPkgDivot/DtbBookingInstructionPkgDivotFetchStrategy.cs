using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingInstructionPkgDivotFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbBookingInstructionPkgDivotFetchStrategy(DtbBookingInstructionPkgDivot divot)
			: base(divot)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(typeof(PkgPackage), ((DtbBookingInstructionPkgDivot)BusinessObject).KD_KP_Package);
		}
	}
}
