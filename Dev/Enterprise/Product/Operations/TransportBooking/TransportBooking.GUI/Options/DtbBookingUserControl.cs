using System;
using CargoWise.Application;
using Enterprise.Integration.Freight;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class DtbBookingUserControl : ZUserControl
	{
		public DtbBookingUserControl()
		{
			InitializeComponent();

			if (!TransportRegistry.Instance.MasterBookingsEnabled.Value)
			{
				var masterBookingNumberColumnName = nameof(DtbBooking.MasterBooking) + "+" + nameof(DtbBooking.KM_JobID);
				var masterBookingNumberColumn = TransportBookingsGrid.InnerGrid.GetColumnStyle(masterBookingNumberColumnName);
				TransportBookingsGrid.ColumnStyles.Remove(masterBookingNumberColumn);
			}

			if (!ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				var cO2eColumnName = nameof(DtbBooking.TotalCO2eForSorting);
				var cO2eColumn = TransportBookingsGrid.InnerGrid.GetColumnStyle(cO2eColumnName);
				TransportBookingsGrid.ColumnStyles.Remove(cO2eColumn);
			}

			FilterStripAuditDetails.AddAuditDetailsColumns(TransportBookingsGrid.InnerGrid, nameof(DtbBooking), typeof(DtbBooking));
			TransportBookingsGrid.BookingCreated += TransportBookingsGrid_BookingCreated;
		}

		void TransportBookingsGrid_BookingCreated(object sender, EventArgs e)
		{
			var wrapper = (DtbBookingParentWrapper)DataSource;
			if (wrapper != null)
			{
				SetDataBinding(null, "");
				SetDataBinding(new DtbBookingParentWrapper(wrapper.Parent, wrapper.Direction), "");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
			}

			if (TransportBookingsGrid != null)
			{
				TransportBookingsGrid.BookingCreated -= TransportBookingsGrid_BookingCreated;
			}

			base.Dispose(disposing);
		}
	}
}
