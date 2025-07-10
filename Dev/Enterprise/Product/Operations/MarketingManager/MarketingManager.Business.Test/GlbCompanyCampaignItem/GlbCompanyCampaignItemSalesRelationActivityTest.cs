using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItem))]
	sealed class GlbCompanyCampaignItemSalesRelationActivityTest : SalesRelationActivityTestCase<GlbCompanyCampaignItem>
	{
		protected override ITableSchema TableSchema
		{
			get { return GlbCompanyCampaignItemSchema.Instance; }
		}

		protected override GlbCompanyCampaignItem GetNewActivity()
		{
			var item = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = ZGuid.NewZGuid();
			return item;
		}
	}
}
