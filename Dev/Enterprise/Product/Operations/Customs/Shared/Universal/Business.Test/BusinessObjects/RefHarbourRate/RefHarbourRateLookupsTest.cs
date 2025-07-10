using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal.Testing;

sealed class RefHarbourRateLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestModeList()
	{
		AssertContainsExactElementsInAnyOrder("All codes in ModeList is hard-coded in the xml file.", new[] { "ALL", "BBK", "BLK", "CON", "EMP", "LIQ" }, lookups.ModeList.GetAllCodes());
	}

	public void TestDataGroupingList()
	{
		SetUpDataGroupingRefData();

		AssertContainsExactElementsInAnyOrder("RefDataGroupings should be loaded.", new[] { "DIE", "FR", "IT" }, lookups.DataGroupingList.Cast<RefDataGrouping>().Select(x => x.ZZZ_DataGrouping));

		void SetUpDataGroupingRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("DIE");
			helper.CreateNewOrGetExistingDataGrouping("FR");
			helper.CreateNewOrGetExistingDataGrouping("IT");
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		lookups = Factory.New<RefHarbourRate>().Lookups;
	}
	RefHarbourRateLookups lookups;
}
