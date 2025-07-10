using System;
using System.Collections.Generic;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class OrderLinesToPackLinesMappingPageForTest : OrderLinesToPackLinesMappingPage
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public OrderLinesToPackLinesMappingPageForTest()
		{
			OrderLinesGrid = new ZDataGrid();
			Controls.Add(OrderLinesGrid);
			SetupOrderLinesGrid();
		}

		public TrackingBooking Booking_ForTest => Booking;

		public void Finish_ForTest()
		{
			Finish_Click(null, EventArgs.Empty);
		}

		public void LoadOrCreateDataSource_ForTest()
		{
			LoadOrCreateDataSource();
		}

		public ZDataGrid OrderLinesGridForTest => OrderLinesGrid;

		public List<ZPageConfirmation> PageConfirmationsForTest => PageConfirmations;
	}
}
