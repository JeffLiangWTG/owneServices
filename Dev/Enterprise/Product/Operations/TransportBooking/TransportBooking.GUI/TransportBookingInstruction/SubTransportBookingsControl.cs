using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class SubTransportBookingsControl : ZUserControl
	{
		public SubTransportBookingsControl()
		{
			InitializeComponent();
			AddReadOnlyAttributes();
		}

		void AddReadOnlyAttributes()
		{
			TypeDescriptor.AddAttributes(AttachBookingButton, new CanBeReadOnlyUIAttribute());
			TypeDescriptor.AddAttributes(DetachBookingButton, new CanBeReadOnlyUIAttribute());
		}

		void AttachBookingButton_Click(object sender, EventArgs e)
		{
			AttachBookings();
		}

		void AttachBookings()
		{
			BookingsModuleButtonGrid.PerformClickOnAttachButton();
		}

		void DetachBookingButton_Click(object sender, EventArgs e)
		{
			DetachBookings();
		}

		void DetachBookings()
		{
			BookingsModuleButtonGrid.PerformClickOnDetachButton();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
