using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.GPS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.GPS.Testing
{
	class GPSEventsGUIHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestGetLegFont()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var runSheet = Factory.New<CommonWorkSheet>();
			var helper = new GPSEventsGUIHelper(runSheet);
			var gpsEvent = new GPSEvent();
			gpsEvent.EventLeg = leg;
			gpsEvent.EventTime = ZDateTime.Invalid;
			runSheet.GPSEvents.Add(gpsEvent);
			var e = new FontDecidingEventArgs(leg, JobContainerLegsSchema.Constants.JU_PickupTimeIn, null);
			using (var grid = new ZGrid())
			{
				helper.GetLegFont(e, null, grid, true);
				gpsEvent.EventTime = ZDateTime.Empty;
				helper.GetLegFont(e, null, grid, true);
				gpsEvent.EventTime = ZDateTime.Today;
				helper.GetLegFont(e, null, grid, true);
			}
		}
	}
}
