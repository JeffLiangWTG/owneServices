using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefTransitTimeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportModes()
		{
			RefTransitTime transitTime = Factory.New<RefTransitTime>();
			var lookups = transitTime.Lookups;
			AssertEquals(16, lookups.Modes.Count);
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.ALL));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.AIR));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.ULD));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.LSE));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.SEA));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.LCL));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.FCL));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.ROA));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.LRO));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.FRO));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.FTL));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.LRA));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.RAI));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.FRA));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.FWL));
			Assert(lookups.Modes.ContainsCode(Constants.RateMode.MAI));
		}

		public void TestServiceLevels()
		{
			RefTransitTime transitTime = Factory.New<RefTransitTime>();
			var lookups = transitTime.Lookups;
			 AssertEquals(5, lookups.ServiceLevels.Count);
			transitTime.Delete();

			var nonWeekendServiceLevel = Factory.New<RefServiceLevel>();
			nonWeekendServiceLevel.RS_Code = "NON";
			nonWeekendServiceLevel.RS_DeliverOnSaturday = false;
			nonWeekendServiceLevel.RS_DeliverOnSunday = false;

			var weekendServiceLevel1 = Factory.New<RefServiceLevel>();
			weekendServiceLevel1.RS_Code = "WKE";
			weekendServiceLevel1.RS_DeliverOnSaturday = true;

			var weekendServiceLevel2 = Factory.New<RefServiceLevel>();
			weekendServiceLevel2.RS_Code = "SUN";
			weekendServiceLevel2.RS_DeliverOnSunday = true;

			Factory.Save();

			transitTime = Factory.New<RefTransitTime>();
			lookups = transitTime.Lookups;
			AssertEquals("Only service levels without weekend delivery should be available", 6, lookups.ServiceLevels.Count);
			Assert("Weekend delivery service level should not be available",
				!lookups.ServiceLevels.Any(s =>
					s.RS_Code.Equals(weekendServiceLevel1.RS_Code) || s.RS_Code.Equals(weekendServiceLevel2.RS_Code)));
			Assert("Extra Non-Weekend delivery service level should be available", lookups.ServiceLevels.Any(sl => sl.RS_Code.Equals("NON")));
		}
	}
}
