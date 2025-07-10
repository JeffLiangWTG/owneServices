using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class QuotedBookingOrderManagementControl : ZUserControl
	{
		public QuotedBookingOrderManagementControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			QuotedBooking quotedBooking = dataSource as QuotedBooking;

			if (quotedBooking != null)
			{
				OrderLinks.SetDataBinding(quotedBooking.Booking, string.Empty);
			}
			else
			{
				OrderLinks.SetDataBinding(null, string.Empty);
			}
		}
	}
}
