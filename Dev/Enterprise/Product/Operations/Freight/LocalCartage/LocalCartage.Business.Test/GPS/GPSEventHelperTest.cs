using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.GPS.Testing
{
	public class GPSEventHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPopulateEventLeg_HandlesInvalidDates()
		{
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_StartTime = ZDateTime.Empty;
			runSheet.EY_EndTime = ZDateTime.Empty;
			var gpsHelper = new GPSEventsHelper(runSheet);
			gpsHelper.PopulateEventLeg(runSheet);
			var runSheetWithDates = Factory.New<CommonWorkSheet>();
			runSheetWithDates.EY_StartTime = ZDateTime.Today;
			runSheetWithDates.EY_EndTime = ZDateTime.Today.AddDays(1);
			var gpsHelperWithDates = new GPSEventsHelper(runSheetWithDates);
			gpsHelperWithDates.PopulateEventLeg(runSheetWithDates);
		}
	}
}
