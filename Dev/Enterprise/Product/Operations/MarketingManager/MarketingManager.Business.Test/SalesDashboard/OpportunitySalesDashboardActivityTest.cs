using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OpportunitySalesDashboardActivity))]
	sealed class OpportunitySalesDashboardActivityTest : SalesDashboardActivityTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var activity = Factory.New<OpportunitySalesDashboardActivity>();
			activity.VSA_ParentId = opportunity.PK;
			return activity;
		}
	}
}
