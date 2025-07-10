using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Module;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.Module.Testing
{
	public class DtbBookingConsolidationFilterControlTest : TestCaseWithFactory
	{
		public void TestGetNewFilterStripControl()
		{
			using (var form = new ZForm())
			{
				var bookings = new DtbBookingMultiJobConsolidationCollection(Factory);
				var filterBO = new DtbBookingConsolidationFilterBusinessObject();
				var filterControl = new DtbBookingConsolidationFilterControl(bookings, filterBO);

				form.Controls.Add(filterControl);
				form.Show();

				filterControl.AddNewFilterStrip();
				AssertEquals("Must return WorkflowFilterStrip so that workflow filter strips may be selected", typeof(WorkflowFilterStrip), filterControl.LastFilterStripType);
			}
		}
	}
}
