using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Module.Testing
{
	sealed class EntryHeaderFilterLookupsTest : TestCaseWithFactory
	{
		public void TestMessageStatusListForENS()
		{
			var messageStatusListForENS = lookups.MessageStatusListForENS;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "AED, AEO, AER, AFC, AFT, CED, CEO, CER, CFC, CFT, WAC, WAW, WRC, WRW, EED, EEO, EER, EFC, ERT, NOT", messageStatusListForENS.CodesAsString);
				AssertSame("Cached", messageStatusListForENS, new EntryHeaderFilterLookups(filterBizObj).MessageStatusListForENS);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new EntryHeaderFilterBusinessObject();
			lookups = new EntryHeaderFilterLookups(filterBizObj);
		}

		EntryHeaderFilterBusinessObject filterBizObj;
		EntryHeaderFilterLookups lookups;
	}
}
