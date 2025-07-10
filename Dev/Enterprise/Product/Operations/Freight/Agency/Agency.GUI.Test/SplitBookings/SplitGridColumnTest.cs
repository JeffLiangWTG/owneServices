using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal abstract class SplitGridColumnTest : TestCaseWithFactory
	{
		public void TestColumnStyles()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var splitBookingHeader = SplitBookingsHeader.New(booking);
			using (var form = new ZForm(splitBookingHeader))
			{
				var split = GetSplitGridForTest();
				var splitControl = split as Control;
				splitControl.Dock = DockStyle.Fill;
				form.Controls.Add(splitControl);
				form.Show();
				var column = split.GetColumnControlForTest(AgencyShipmentContainer.Schema.JC_ContainerNum);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), column.GetType());
			}
		}

		protected abstract ISplitGridForColumnTest GetSplitGridForTest();
	}
}
