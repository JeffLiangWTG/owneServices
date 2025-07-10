using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class OpportunityViewControllerForTest : OpportunityViewController
	{
		public OpportunityViewControllerForTest(OpportunityForm opportunityView)
			: base(opportunityView)
		{
		}

		protected override SalesMigrationResponse PromptUser(SalesMatching matching)
		{
			return new SalesMigrationResponse() { UserResult = SalesMigrationForm.MigrationResult.Append };
		}
	}
}
