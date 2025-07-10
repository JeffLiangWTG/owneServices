using CargoWise.Schema;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaign))]
	sealed class GlbCompanyCampaignSalesRelationActivityTest : SalesRelationActivityTestCase<GlbCompanyCampaign>
	{
		protected override ITableSchema TableSchema
		{
			get { return GlbCompanyCampaignSchema.Instance; }
		}

		protected override GlbCompanyCampaign GetNewActivity()
		{
			return Factory.NewWithValidTestData<GlbCompanyCampaign>();
		}
	}
}
