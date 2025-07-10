using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OpportunitySalesDashboardActivity))]
	sealed class SalesDashboardActivitySalesRelationActivityTest : SalesRelationActivityTestCase<SalesDashboardActivity>
	{
		public override void TestCodePropertyAttribute()
		{
			Assert("N/A - selectable in Related Activities Grid", true);
		}

		public override void TestDescriptionPropertyAttribute()
		{
			Assert("N/A - Not selectable in Related Activities Grid", true);
		}

		public override void TestRelatedPivotsDeletedOnDeletion()
		{
			Assert("Can not delete", true);
		}

		#region Implementation

		protected override ITableSchema TableSchema
		{
			get { return ViewSalesDashboardActivitySchema.Instance; }
		}

		protected override SalesDashboardActivity GetNewActivity()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			Factory.Save();

			return Factory.Load<OpportunitySalesDashboardActivity>(opportunity.PK);
		}

		#endregion
	}
}
