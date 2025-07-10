using CargoWise.Windows.UI;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class CombineBookingsControl : ZUserControl
	{
		public CombineBookingsControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				SetControlsCaptionAndVisibility();
			}
		}

		void SetControlsCaptionAndVisibility()
		{
			var combineBookings = DataSource as CombineBookings;
			var quotedBooking = combineBookings?.MasterQuotedBooking;

			if (quotedBooking != null && quotedBooking.Booking != null)
			{
				VoyageNumberTextBox.GetExtension<LabelCaptionRenderer>().Caption = quotedBooking.ScheduleChooser.GetVoyageNoLabelDependingOnTransportMode();
				VesselTextBox.Visible = quotedBooking.Booking.IsSea || quotedBooking.Booking.IsRail;
			}
		}
	}
}
