using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class SupplierBookingUserControl : ZUserControl, INotifications
	{
		public SupplierBookingUserControl()
		{
			InitializeComponent();

			this.BookingDetailLinkLabel.LinkClicked += BookingDetailLinkLabel_LinkClicked;
		}

		void BookingDetailLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (this.DataSource is JobSupplierBooking supplierBooking)
			{
				GlowLinksHelper.OpenEnityInGlow(this, "Goto/JobSupplierBooking", supplierBooking);
			}
		}

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification);
		}

		#endregion
	}
}
