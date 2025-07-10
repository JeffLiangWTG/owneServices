using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Module.Testing
{
	public class RefHarbourRateFilterLookupTest : TestCaseWithFactory
	{
		public void TestModesList()
		{
			var modesList = lookups.ModeList;
			CombineAssertions(() =>
			{
				AssertEquals("ModesList", typeof(RefHarbourRateModeList), modesList.GetType());
				AssertEquals(6, modesList.Count);
				AssertEquals(RefHarbourRateModeList.Codes.ALL, modesList[0].Code);
				AssertEquals(RefHarbourRateModeList.Codes.BBK, modesList[1].Code);
				AssertEquals(RefHarbourRateModeList.Codes.BLK, modesList[2].Code);
				AssertEquals(RefHarbourRateModeList.Codes.CON, modesList[3].Code);
				AssertEquals(RefHarbourRateModeList.Codes.EMP, modesList[4].Code);
				AssertEquals(RefHarbourRateModeList.Codes.LIQ, modesList[5].Code);
			});
		}
		protected override void SetUp()
		{
			base.SetUp();
			lookups = new RefHarbourRateFilterLookup(new RefHarbourRateFilterStripBusinessObject());
		}

		RefHarbourRateFilterLookup lookups;
	}
}
