using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingTmplFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbBookingTmplFetchStrategy(DtbBookingTmpl template)
			: base(template)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			Factory.AddFetchHint(DtbBookingInstructionTmplSchema.K2_KT_BookingTmpl, Template.PK);
		}

		DtbBookingTmpl Template
		{
			get { return (DtbBookingTmpl)BusinessObject; }
		}
	}
}
