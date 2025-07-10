using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Testing
{
	class TransportBookingFormForTest : TransportBookingForm
	{
		public TransportBookingFormForTest(DtbBooking transportBooking)
			: base(transportBooking)
		{ }

		public ZBool ShowNotesTabForTest
		{
			get { return ShowNotesTab; }
		}

		public TabControl MainTabControlForTest
		{
			get { return MainTabControl; }
		}

		public TabPage MainTabPageForTest
		{
			get { return MainTabPage; }
		}

		public ZTabPage AdditionalReferencesTabPageForTest
		{
			get { return (ZTabPage)Controls.Find("AdditionalReferencesTabPage", true).Single(); }
		}

		public ZTabPage RoutingScheduleTabPageForTest
		{
			get { return (ZTabPage)Controls.Find("RoutingScheduleTabPage", true).SingleOrDefault(); }
		}

		public ZTabPage AccountingTabPageForTest
		{
			get { return (ZTabPage)Controls.Find("AccountingTabPage", true).SingleOrDefault(); }
		}

		public ZGrid AdditionalReferencesGrid { get { return (ZGrid)Controls.Find("numbersGrid", true).Single(); } }

		public MenuItem ActionsMenuForTest
		{
			get { return ActionsMenuItem; }
		}

		public DtbInstructionViewsControl InstructionViewsControlForTest
		{
			get { return InstructionViewsControl; }
		}
	}
}
