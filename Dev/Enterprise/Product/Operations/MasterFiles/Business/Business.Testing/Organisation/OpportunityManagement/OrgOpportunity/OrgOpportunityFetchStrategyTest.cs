using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgOpportunityFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForView()
		{
			string tableName = "OrgOpportunity";
			var opportunity = Factory.New<OrgOpportunity>();

			OrgOpportunityFetchStrategy strategy = new OrgOpportunityFetchStrategy(opportunity);
			AssertEquals(0, Factory.ActiveTableFetchHints);

			TableColumn[] columns = new TableColumn[] {
				new TableColumn(tableName, "P8_PK"),
				new TableColumn(tableName, "P8_OpportunityID")
			};
			strategy.FetchForView(columns);
			AssertEquals(0, Factory.ActiveTableFetchHints);

			TableColumn[] columnsWithSourceCampaign = new TableColumn[] {
				new TableColumn(tableName, "P8_PK"),
				new TableColumn(tableName, "P8_OpportunityID"),
				new TableColumn(tableName, "SourceCampaign")
			};
			strategy.FetchForView(columnsWithSourceCampaign);
			AssertEquals(1, Factory.ActiveTableFetchHints);
		}
	}
}
