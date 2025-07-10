using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(CommunicationSalesDashboardActivity))]
	sealed class CommunicationSalesDashboardActivityTest : SalesDashboardActivityTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var salesCall = Factory.NewWithValidTestData<OrgSalesCall>();
			var activity = Factory.New<CommunicationSalesDashboardActivity>();
			activity.VSA_ParentId = salesCall.PK;
			return activity;
		}
	}
}
