using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(InquirySalesDashboardActivity))]
	sealed class InquirySalesDashboardActivityTest : SalesDashboardActivityTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var activity = Factory.New<InquirySalesDashboardActivity>();
			activity.VSA_ParentId = salesEnquiry.PK;
			return activity;
		}
	}
}
