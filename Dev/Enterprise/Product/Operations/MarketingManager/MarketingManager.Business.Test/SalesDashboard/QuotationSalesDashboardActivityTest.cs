using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(QuotationSalesDashboardActivity))]
	sealed class QuotationSalesDashboardActivityTest : SalesDashboardActivityTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var parent = Factory.NewWithValidTestData<Quote>();
			var activity = Factory.New<QuotationSalesDashboardActivity>();
			activity.VSA_ParentId = parent.PK;
			return activity;
		}
	}
}
