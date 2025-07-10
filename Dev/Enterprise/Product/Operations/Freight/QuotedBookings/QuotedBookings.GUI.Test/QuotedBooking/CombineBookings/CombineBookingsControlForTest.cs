using System.Linq;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	class CombineBookingsControlForTest : CombineBookingsControl
	{
		public CombineBookingsControlForTest()
			: base() { }

		public ZModuleButtonGrid BookingsModuleButtonGrid
		{
			get
			{
				return bookingsModuleButtonGrid ?? (bookingsModuleButtonGrid = (ZModuleButtonGrid)Controls.Find("bookingsModuleButtonGrid", true).First());
			}
		}
		ZModuleButtonGrid bookingsModuleButtonGrid;

		public ZTextBox VesselTextBox
		{
			get
			{
				return vesselTextBox ?? (vesselTextBox = (ZTextBox)Controls.Find("VesselTextBox", true).First());
			}
		}
		ZTextBox vesselTextBox;

		public ZTextBox VoyageNumberTextBox
		{
			get
			{
				return voyageNumberTextBox ?? (voyageNumberTextBox = (ZTextBox)Controls.Find("VoyageNumberTextBox", true).First());
			}
		}
		ZTextBox voyageNumberTextBox;
	}
}
