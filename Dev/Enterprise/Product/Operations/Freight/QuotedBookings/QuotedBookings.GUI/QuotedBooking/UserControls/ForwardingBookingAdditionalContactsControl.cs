using System;
using System.ComponentModel;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class ForwardingBookingAdditionalContactsControl : ZUserControl
	{
		public ForwardingBookingAdditionalContactsControl()
		{
			InitializeComponent();
#if DEBUG
			TypeDescriptor.AddAttributes(ReceivalPointAddressControl, new SuppressControlRequiresTextBasherAttribute());
#endif

			ControllingAgentAddressControl.AllowOverlap(ControllingCustomerAddressControl);
		}

		#region Binding

		QuotedBooking QuotedBooking
		{
			get { return (QuotedBooking)base.CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var quotedBooking = QuotedBooking;

			if (quotedBooking != null)
			{
				SetupLayout();
				quotedBooking.ModeInfo.ValueChanged += new EventHandler(BookingMode_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var quotedBooking = QuotedBooking;

			if (quotedBooking != null)
			{
				quotedBooking.ModeInfo.ValueChanged -= new EventHandler(BookingMode_ValueChanged);
			}
		}

		void BookingMode_ValueChanged(object sender, EventArgs e)
		{
			SetupLayout();
		}

		void SetupLayout()
		{
			var quotedBooking = QuotedBooking;
			var result = QuotedBookingHelper.GetPickupDeliveryOrgTitles(quotedBooking.Mode);
			ReceivalPointGroupBox.Text = result.PickupOrgTitle;
			DeliveryPointGroupBox.Text = result.DeliveryOrgTitle;
		}

		#endregion
	}
}
