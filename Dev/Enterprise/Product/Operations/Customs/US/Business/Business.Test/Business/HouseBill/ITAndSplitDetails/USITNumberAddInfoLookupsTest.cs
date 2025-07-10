using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USITNumberAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUSCarrierList()
		{
			AssertType<USCarrierCombinedCollection>(lookups.USCarrierList);
		}

		USITNumberAddInfoLookups lookups;
		protected override void SetUp()
		{
			base.SetUp();
			var addInfo = new USITNumberAddInfo(Factory.New<ITAndSplitDetails>().B7_AddInfoDataInfo);
			lookups = new USITNumberAddInfoLookups(addInfo);
		}
	}
}
