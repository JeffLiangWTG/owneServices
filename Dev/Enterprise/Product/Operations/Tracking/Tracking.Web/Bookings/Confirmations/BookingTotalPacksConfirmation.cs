using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Bookings
{
	public class BookingTotalPacksConfirmation : ZPageConfirmation
	{
		#region Constructors

		public BookingTotalPacksConfirmation(ZPage page)
			: base(page)
		{
		}

		#endregion

		#region New

		public new EditBooking Page
		{
			get
			{
				return base.Page as EditBooking;
			}
		}

		#endregion

		#region Overrides

		protected new TrackingBooking DataSource
		{
			get
			{
				return base.DataSource as TrackingBooking;
			}
		}

		protected override ConfirmationTypes ConfirmationType
		{
			get
			{
				return ConfirmationTypes.Warning;
			}
		}

		protected override bool GetRequired()
		{
			if (DataSource != null)
			{
				if (DataSource.QuotedBooking.Booking != null)
				{
					return OuterPacksTotalsDifferAndCanBeUpdated;
				}
			}
			return false;
		}

		protected override string GetTitle()
		{
			return Res.GetString("4a343a8a-6117-4110-b3da-c001b225095b", "Totals do not match");
		}

		protected override string GetConfirmationMessage()
		{
			return Res.GetString("bf1ff843-3297-458a-8070-a088ea23c8b6", "Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?");
		}

		protected override string GetUserResponseHolderID()
		{
			return "TotalPacksConfirmation";
		}

		protected override void HandleResponseCore()
		{
			base.HandleResponseCore();
			if (YesButtonClicked)
			{
				DataSource.QuotedBooking.Booking.UpdateShipmentFromOuterPackLines();
			}
		}

		#endregion

		#region Implementation

		bool OuterPacksTotalsDifferAndCanBeUpdated
		{
			get
			{
				return DataSource.QuotedBooking.Booking.OuterPacksTotalsDifferAndCanBeUpdated && DataSource.QuotedBooking.Booking.OuterPackLines.TotalPackages > 0 && (DataSource.QuotedBooking.Booking.JS_OuterPacks > 0 || DataSource.QuotedBooking.Booking.JS_ActualWeight > 0 || DataSource.QuotedBooking.Booking.JS_ActualVolume > 0);
			}
		}

		#endregion
	}
}
