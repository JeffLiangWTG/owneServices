using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StmUpgradeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStatuses()
		{
			StmUpgradeLookups lookups = new StmUpgradeLookups(null);

			AssertEquals("Count", 6, lookups.Statuses.Count);
			AssertEquals("GetDescriptionFromCode(\"CUR\")", "Current Version", lookups.Statuses.GetDescriptionFromCode("CUR"));
			AssertEquals("GetDescriptionFromCode(\"RDY\")", "Ready", lookups.Statuses.GetDescriptionFromCode("RDY"));
			AssertEquals("GetDescriptionFromCode(\"APL\")", "Applied", lookups.Statuses.GetDescriptionFromCode("APL"));
			AssertEquals("GetDescriptionFromCode(\"NAP\")", "Not Applied", lookups.Statuses.GetDescriptionFromCode("NAP"));
			AssertEquals("GetDescriptionFromCode(\"DEL\")", "Deleted", lookups.Statuses.GetDescriptionFromCode("DEL"));
			AssertEquals("GetDescriptionFromCode(\"OBS\")", "Obsolete", lookups.Statuses.GetDescriptionFromCode("OBS"));
		}
	}
}
